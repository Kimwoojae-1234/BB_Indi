#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay.UGUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class CanvasExpansionChecks
{
    public const string Output = "Docs/UIAudit/CanvasRestructure/Step5";
    private const string Baseline = "Library/UGUIMigration/CanvasExpansion";
    [Serializable] private class Scope { public string[] assets; }
    [Serializable] private class Point { public string id; public Vector2 min, max; }
    [Serializable] private class Layout { public List<Point> points = new List<Point>(); }
    public static string[] Assets => JsonUtility.FromJson<Scope>(File.ReadAllText(Output + "/scope.json")).assets;
    public static void CaptureBaseline() { CanvasStructureChecks.CaptureStep5Baseline(); Run(false); }
    public static void Check() { CanvasStructureChecks.CheckStep5(); Run(true); }
    private static void Run(bool compare)
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Stop Play Mode first.");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            Require(!UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty, "Save scene changes first.");
        if (!compare) Require(!Directory.Exists(Baseline), "Do not overwrite the expansion baseline.");
        var setup = EditorSceneManager.GetSceneManagerSetup();
        var report = new List<string> { DateTime.UtcNow.ToString("O"), "14 dynamic prefabs in an isolated UGUI canvas; owners activated, authored widget values preserved. Not gameplay." };
        Directory.CreateDirectory(compare ? Output + "/Dynamic" : Baseline);
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            foreach (string path in Assets)
            foreach (var size in new[] { new Vector2Int(1280,720), new Vector2Int(2340,1080), new Vector2Int(1024,768) })
            foreach (bool tween in new[] { false, true }) Capture(path,size,tween,compare,report);
            if (compare) IntegratedUGUIChecks.CheckApplied(Output + "/retained-references.txt");
            report.Add("OVERALL PASS");
        }
        catch (Exception e) { report.Add(e.ToString()); throw; }
        finally
        {
            File.WriteAllLines((compare ? Output : Baseline) + "/dynamic-checks.txt",report);
            if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }
    private static void Capture(string path, Vector2Int size, bool tween, bool compare, List<string> report)
    {
        var owner = new GameObject("Dynamic canvas fixture");
        var root = owner.AddComponent<GameUIRoot>();
        var cameraObject = new GameObject("UI Camera", typeof(Camera)); cameraObject.transform.SetParent(owner.transform,false);
        var camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 1;
        camera.transform.localPosition = new Vector3(0,0,-1); camera.nearClipPlane = -9; camera.farClipPlane = 11;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black; camera.enabled = false;
        camera.transparencySortMode = TransparencySortMode.Orthographic;
        var rect = (RectTransform)new GameObject("Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).transform;
        rect.SetParent(owner.transform,false);
        var canvas = rect.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
        var scaler = rect.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = 1;
        root.rootCanvas = canvas; root.uiCamera = camera;
        var parent = (RectTransform)new GameObject("Parent panel",typeof(RectTransform),typeof(GameUIPanel)).transform;
        parent.SetParent(rect,false); parent.sizeDelta = new Vector2(800,600); parent.pivot = new Vector2(.2f,.8f); parent.localPosition = Vector3.zero;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.SetParent(parent,false); instance.transform.localPosition = Vector3.zero;
        instance.transform.localScale = Vector3.one; instance.transform.localRotation = Quaternion.identity;
        foreach (var node in instance.GetComponentsInChildren<Transform>(true)) node.gameObject.SetActive(true);
        var rt = new RenderTexture(size.x,size.y,24); Texture2D image = null;
        try
        {
            camera.targetTexture = rt;
            foreach (var panel in owner.GetComponentsInChildren<GameUIPanel>(true)) GameUIRenderOrder.Register(panel);
            foreach (var element in instance.GetComponentsInChildren<GameUIElement>(true)) GameUIRenderOrder.Register(element);
            if (tween) foreach (var t in instance.GetComponentsInChildren<GameUITween>(true)) t.Sample(.5f,false);
            instance.SendMessage("RefreshUGUIHierarchy",SendMessageOptions.DontRequireReceiver);
            root.BindCanvases(); root.SynchronizePresentation();
            var layout = new Layout();
            foreach (var element in instance.GetComponentsInChildren<GameUIElement>(true))
            {
                if (element.graphic == null) continue;
                var source = PrefabUtility.GetCorrespondingObjectFromSource(element);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source,out string _,out long id);
                var corners = new Vector3[4]; element.graphic.rectTransform.GetWorldCorners(corners);
                var screen = corners.Select(p => (Vector2)camera.WorldToScreenPoint(p)).ToArray();
                layout.points.Add(new Point { id = id.ToString(), min = new Vector2(screen.Min(p=>p.x),screen.Min(p=>p.y)), max = new Vector2(screen.Max(p=>p.x),screen.Max(p=>p.y)) });
                if (compare) Require(element.layoutRect == element.transform, path + " missing layout owner");
            }
            string name = Path.GetFileNameWithoutExtension(path) + "-" + size.x + "x" + size.y + (tween ? "-tween" : "-initial");
            camera.Render(); RenderTexture.active = rt;
            image = new Texture2D(size.x,size.y,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,size.x,size.y),0,0); image.Apply();
            File.WriteAllBytes((compare ? Output + "/Dynamic" : Baseline) + "/" + name + ".png",image.EncodeToPNG());
            if (!compare) File.WriteAllText(Baseline + "/" + name + ".json",JsonUtility.ToJson(layout));
            else
            {
                var old = JsonUtility.FromJson<Layout>(File.ReadAllText(Baseline + "/" + name + ".json")).points.ToDictionary(p=>p.id);
                Require(old.Count == layout.points.Count,name + " graphic count"); float drift = 0;
                foreach (var point in layout.points)
                {
                    Require(old.TryGetValue(point.id,out var before),name + " missing graphic ID " + point.id);
                    drift = Mathf.Max(drift,Vector2.Distance(before.min,point.min),Vector2.Distance(before.max,point.max));
                }
                Require(drift < .02f,name + " coordinate drift=" + drift);
                var baseline = new Texture2D(2,2); baseline.LoadImage(File.ReadAllBytes(Baseline + "/" + name + ".png"));
                var a = baseline.GetPixels32(); var b = image.GetPixels32(); long difference = 0; int changed = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    int d = Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b);
                    difference += d; if (d > 24) changed++;
                }
                Object.DestroyImmediate(baseline);
                double mean = difference/(a.Length*3.0);
                report.Add("PASS " + name + " graphics=" + layout.points.Count + " boundsDrift=" + drift.ToString("F5") + "px meanAbsoluteRGB=" + mean.ToString("F6") + " pixelsAbove24=" + changed);
                Require(mean < .01 && changed <= a.Length*.0001,name + " pixel difference=" + mean + "/" + changed);
            }
        }
        finally
        {
            foreach (var panel in owner.GetComponentsInChildren<GameUIPanel>(true)) GameUIRenderOrder.Remove(panel);
            foreach (var element in owner.GetComponentsInChildren<GameUIElement>(true)) GameUIRenderOrder.Remove(element);
            RenderTexture.active = null; camera.targetTexture = null;
            if (image != null) Object.DestroyImmediate(image);
            Object.DestroyImmediate(owner); Object.DestroyImmediate(rt);
        }
    }
    private static void Require(bool condition,string message) { if (!condition) throw new InvalidOperationException(message); }
}
#endif
