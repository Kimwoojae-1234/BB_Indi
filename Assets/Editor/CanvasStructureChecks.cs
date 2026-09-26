#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay.UGUI;
using BaseBall.BallPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Explicit batch checks only; never saves a scene or prefab.
public static class CanvasStructureChecks
{
    private static string Output = "Docs/UIAudit/CanvasRestructure/Step2";
    private static string Baseline = "Library/UGUIMigration/CanvasStructure";
    private static bool step3;
    private static readonly string[] Assets = {
        "Assets/Resources/MainGame/prefabs/gameUI/IngameUIPrefab.prefab",
        "Assets/Resources/MainGame/prefabs/QuickUI/QuickSimulatorPrefab.prefab",
        "Assets/Resources/MainGame/prefabs/ballplayPrefab/loadingPagePrefab.prefab"
    };
    [Serializable] private class Point { public string id; public Vector2 min, max; }
    [Serializable] private class Layout { public List<Point> points = new List<Point>(); }

    public static void CaptureBaseline() { Run(false); }
    public static void Check() { Run(true); }
    public static void CompareBaseline() { CaptureBaseline(); Check(); }
    public static void CaptureStep3Baseline() { Step3(); Run(false); }
    public static void CheckStep3() { Step3(); Run(true); }
    private static void Step3() { step3 = true; Output = "Docs/UIAudit/CanvasRestructure/Step3"; Baseline = "Library/UGUIMigration/CanvasLayout"; }

    private static void Run(bool compare)
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Run in a separate batch editor; open user scenes are not changed.");
        Directory.CreateDirectory(compare ? Output : Baseline);
        var report = new List<string> { DateTime.UtcNow.ToString("O"), "Static prefab rendering and projection checks; not a gameplay run." };
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        foreach (var path in Assets)
        foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(2340, 1080), new Vector2Int(1024, 768) })
        {
            string baselineCopy = (step3 ? "Assets/Editor/CanvasLayoutBaseline/" : "Assets/Editor/CanvasStructureBaseline/") + Path.GetFileName(path);
            string source = !compare && File.Exists(baselineCopy) ? baselineCopy : path;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(source);
            Require(prefab != null, "Prefab could not be loaded: " + source);
            if (!compare && !step3) Require(prefab.GetComponent<GameUIRoot>().rootCanvas == null, "Baseline capture requires the step-1 prefab, not the converted root: " + source);
            if (!compare && step3) Require(!prefab.GetComponentsInChildren<GameUIElement>(true).Any(e => e.layoutRect != null), "Step-3 baseline requires the unmodified step-2 prefab: " + source);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var root = instance.GetComponent<GameUIRoot>();
            var camera = root.uiCamera;
            var rt = new RenderTexture(size.x, size.y, 24);
            Texture2D image = null;
            try
            {
                foreach (var c in instance.GetComponentsInChildren<Camera>(true)) c.enabled = false;
                camera.targetTexture = rt;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.transparencySortMode = TransparencySortMode.Orthographic;
                var scaler = instance.GetComponentInChildren<CanvasScaler>(true);
                if (scaler == null)
                {
                    float height = root.fitWidth ? Mathf.Max(720, 1280f * size.y / size.x) : 720;
                    instance.transform.localScale = Vector3.one * (2 / height);
                }
                root.BindCanvases();
                // These components register in OnEnable during play, which an edit-mode prefab
                // fixture does not execute. Reproduce that registration before comparing order.
                foreach (var p in instance.GetComponentsInChildren<GameUIPanel>())
                    if (p.enabled) GameUIRenderOrder.Register(p);
                foreach (var e in instance.GetComponentsInChildren<GameUIElement>())
                    if (e.enabled) GameUIRenderOrder.Register(e);
                foreach (var e in instance.GetComponentsInChildren<GameUIElement>(true)) e.Apply();
                // These migrated views normally receive their panel order/material queue in LateUpdate.
                foreach (var view in instance.GetComponentsInChildren<FieldOverlayCanvas>(true))
                {
                    var panel = view.GetComponentInParent<GameUIPanel>();
                    view.ApplyRendering(panel != null ? panel.CalculateFinalAlpha(0) : 1, 3000, panel != null ? panel.Order : 0, camera);
                }
                foreach (var view in instance.GetComponentsInChildren<IngameUGUICanvas>(true))
                {
                    var panel = view.GetComponentInParent<GameUIPanel>();
                    view.SetInheritedRendering(panel != null ? panel.CalculateFinalAlpha(0) : 1, 3000, panel != null ? panel.Order : 0, camera, true);
                }
                foreach (var view in instance.GetComponentsInChildren<IngameScoreboardView>(true))
                {
                    var panel = view.GetComponentInParent<GameUIPanel>();
                    view.SetInheritedRendering(panel != null ? panel.CalculateFinalAlpha(0) : 1, 3000, panel != null ? panel.Order : 0);
                }
                Canvas.ForceUpdateCanvases();
                // A second update applies atlas padding after the canvas has driven its root transform.
                foreach (var e in instance.GetComponentsInChildren<GameUIElement>(true)) e.Apply();
                foreach (var batch in instance.GetComponentsInChildren<GameUICanvasBatch>(true)) batch.Apply();
                Canvas.ForceUpdateCanvases();
                var layout = new Layout();
                foreach (var e in instance.GetComponentsInChildren<GameUIElement>(true))
                {
                    if (e.graphic == null) continue;
                    var original = PrefabUtility.GetCorrespondingObjectFromSource(e);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(original, out string _, out long id);
                    var corners = new Vector3[4]; e.graphic.rectTransform.GetWorldCorners(corners);
                    var screen = corners.Select(p => (Vector2)camera.WorldToScreenPoint(p)).ToArray();
                    layout.points.Add(new Point { id = id.ToString(), min = new Vector2(screen.Min(p => p.x), screen.Min(p => p.y)), max = new Vector2(screen.Max(p => p.x), screen.Max(p => p.y)) });
                }
                string name = Path.GetFileNameWithoutExtension(path) + "-" + size.x + "x" + size.y;
                if (!compare) File.WriteAllText(Baseline + "/" + name + ".json", JsonUtility.ToJson(layout));
                else
                {
                    var before = JsonUtility.FromJson<Layout>(File.ReadAllText(Baseline + "/" + name + ".json")).points.ToDictionary(p => p.id);
                    Require(before.Count == layout.points.Count, name + " graphic count changed");
                    float maximum = 0;
                    string maximumId = "";
                    foreach (var point in layout.points)
                    {
                        Require(before.TryGetValue(point.id, out var old), name + " lost fileID " + point.id);
                        float drift = Mathf.Max(Vector2.Distance(old.min, point.min), Vector2.Distance(old.max, point.max));
                        if (drift > maximum) { maximum = drift; maximumId = point.id + " " + old.min + ".." + old.max + " -> " + point.min + ".." + point.max; }
                    }
                    Require(maximum < 1, name + " screen bounds drift " + maximum + " fileID=" + maximumId);
                    Require(scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize, name + " missing scaler");
                    var canvas = scaler.GetComponent<Canvas>();
                    Require(canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == camera, name + " root canvas");
                    Require(canvas.GetComponent<GraphicRaycaster>() != null, name + " missing raycaster");
                    Require(!camera.transform.IsChildOf(canvas.transform), name + " camera under its canvas");
                    report.Add("PASS " + name + " graphics=" + layout.points.Count + " maximumScreenBoundsDrift=" + maximum.ToString("F4") + "px canvasSize=" + ((RectTransform)canvas.transform).rect.size);
                }
                camera.Render();
                RenderTexture.active = rt;
                image = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); image.Apply();
                File.WriteAllBytes((compare ? Output : Baseline) + "/" + name + ".png", image.EncodeToPNG());
                if (compare)
                {
                    var before = new Texture2D(2,2); before.LoadImage(File.ReadAllBytes(Baseline + "/" + name + ".png"));
                    var a = before.GetPixels32(); var b = image.GetPixels32();
                    double sum = 0; int changed = 0;
                    for (int i = 0; i < a.Length; i++)
                    {
                        int difference = Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b);
                        sum += difference; if (difference > 24) changed++;
                    }
                    report.Add("IMAGE " + name + " meanAbsoluteRGB=" + (sum/(a.Length*3)).ToString("F4") + " pixelsAbove24=" + changed);
                    File.WriteAllLines(Output + "/projection-checks.txt", report);
                    Object.DestroyImmediate(before);
                    if (size.x == 1280)
                    {
                        // A root Canvas changes subpixel rasterization. Compare in both directions
                        // within one pixel so edge antialiasing is allowed but lost labels are not.
                        CheckRasterEdges(a,b,size.x,size.y,name,report);
                        CheckRasterEdges(b,a,size.x,size.y,name+" reverse",report);
                    }
                    if (size.x == 1280 && path.Contains("IngameUIPrefab")) CheckCapture(root, report);
                    if (size.x == 1280 && path.Contains("QuickSimulatorPrefab"))
                    {
                        foreach (string team in new[] { "myteam", "cputeam" })
                        {
                            var card = root.hudLayer.Find("Panel/player/" + team + "/PlayerCardSmall");
                            var decoration = card.Find("BG/line").GetComponent<GameUIElement>();
                            var portrait = card.Find("playerPicture").GetComponent<GameUIElement>();
                            Require(decoration.depth == portrait.depth && portrait.displayCanvas.sortingOrder > decoration.displayCanvas.sortingOrder, "Quick card portrait hidden behind same-depth decoration");
                        }
                        report.Add("PASS both Quick card portraits draw above same-depth decorations.");
                    }
                }
            }
            finally
            {
                RenderTexture.active = null; camera.targetTexture = null;
                foreach (var p in instance.GetComponentsInChildren<GameUIPanel>(true)) GameUIRenderOrder.Remove(p);
                foreach (var e in instance.GetComponentsInChildren<GameUIElement>(true)) GameUIRenderOrder.Remove(e);
                if (image != null) Object.DestroyImmediate(image);
                Object.DestroyImmediate(instance); Object.DestroyImmediate(rt);
            }
        }
        if (compare)
        {
            CheckSceneWiring("Assets/MainGame/Scene/BallPlay.unity", 2, report);
            CheckSceneWiring("Assets/MainGame/Scene/MainLoading.unity", 1, report);
            IntegratedUGUIChecks.CheckApplied(Output + "/retained-references.txt");
        }
        report.Add(compare ? "OVERALL PASS" : "BASELINE CAPTURED");
        File.WriteAllLines((compare ? Output : Baseline) + "/projection-checks.txt", report);
    }
    private static void CheckRasterEdges(Color32[] source, Color32[] target, int width, int height, string name, List<string> report)
    {
        long total = 0; int outliers = 0;
        for (int y=0; y<height; y++)
        for (int x=0; x<width; x++)
        {
            int minR=255,minG=255,minB=255,maxR=0,maxG=0,maxB=0;
            for (int yy=Mathf.Max(0,y-1); yy<=Mathf.Min(height-1,y+1); yy++)
            for (int xx=Mathf.Max(0,x-1); xx<=Mathf.Min(width-1,x+1); xx++)
            {
                var p=source[yy*width+xx]; minR=Math.Min(minR,p.r);minG=Math.Min(minG,p.g);minB=Math.Min(minB,p.b);
                maxR=Math.Max(maxR,p.r);maxG=Math.Max(maxG,p.g);maxB=Math.Max(maxB,p.b);
            }
            var t=target[y*width+x]; int r=Math.Max(0,Math.Max(minR-t.r,t.r-maxR));
            int g=Math.Max(0,Math.Max(minG-t.g,t.g-maxG)); int b=Math.Max(0,Math.Max(minB-t.b,t.b-maxB));
            total+=r+g+b; if (Math.Max(r,Math.Max(g,b))>8) outliers++;
        }
        double residual=total/(source.Length*3.0);
        report.Add("RASTER " + name + " onePixelResidual="+residual.ToString("F6")+" outliers="+outliers);
        // Intensities are measured on the 0..255 scale. Permit subpixel edge residuals
        // below 0.01 intensity and no more than 0.01% of pixels exceeding 8 levels.
        Require(residual<.01 && outliers<=source.Length/10000,name+" changed beyond one-pixel edge tolerance: "+residual+", "+outliers);
    }
    private static void CheckSceneWiring(string path, int expected, List<string> report)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<GameUIRoot>(true)).ToArray();
        Require(roots.Length == expected, path + " root count");
        foreach (var root in roots)
        {
            Require(root.rootCanvas != null && root.rootCanvas.worldCamera == root.uiCamera, path + " lost canvas/camera reference");
            Require(root.transform.localScale == Vector3.one, path + " legacy scale override remains");
            Require(root.hudLayer != null && root.effectsLayer != null && root.popupLayer != null, path + " lost layer reference");
            Require(root.uiCamera.transform.parent == root.transform && root.rootCanvas.transform.parent == root.transform, path + " camera/canvas ownership");
        }
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        if (manager != null)
        {
            // BallPlay and its manager prefab already serialize this legacy field as null.
            // Verify that baseline rather than inventing a new scene connection in this step.
            Require(manager.IngameUITrans == null, "Unexpected change to baseline manager IngameUITrans");
            report.Add("BASELINE NOTE BallPlayManager.IngameUITrans remains null, as serialized before this change.");
        }
        report.Add("PASS " + path + " resolved canvas roots=" + roots.Length + "; scene not saved.");
    }
    private static void CheckCapture(GameUIRoot root, List<string> report)
    {
        var capture = root.GetComponentsInChildren<Camera>(true).First(c => c != root.uiCamera);
        var go = new GameObject("Capture check", typeof(RectTransform)); go.layer = 19;
        go.transform.SetParent(root.rootCanvas.transform, false);
        var layer = go.AddComponent<Canvas>(); layer.overrideSorting = true; layer.sortingOrder = short.MaxValue;
        layer.worldCamera = root.uiCamera;
        var rect = (RectTransform)go.transform;
        rect.localPosition = new Vector3(capture.transform.localPosition.x, capture.transform.localPosition.y, 0);
        rect.localScale = Vector3.one; rect.sizeDelta = new Vector2(100,100);
        var imageObject = new GameObject("Marker", typeof(RectTransform)); imageObject.layer = 19;
        imageObject.transform.SetParent(rect, false);
        var marker = imageObject.AddComponent<Image>(); marker.color = Color.red;
        marker.rectTransform.localPosition = Vector3.zero; marker.rectTransform.sizeDelta = new Vector2(100,100);
        var rt = new RenderTexture(640,360,24);
        var oldTarget = capture.targetTexture;
        var oldFlags = capture.clearFlags; var oldColor = capture.backgroundColor;
        var canvasPosition = root.rootCanvas.transform.position; var canvasScale = root.rootCanvas.transform.lossyScale;
        Texture2D pixels = null;
        try
        {
            capture.targetTexture = rt; capture.clearFlags = CameraClearFlags.SolidColor; capture.backgroundColor = Color.black;
            GameUIRoot.RenderForCapture(capture);
            RenderTexture.active = rt; pixels = new Texture2D(640,360,TextureFormat.RGBA32,false);
            pixels.ReadPixels(new Rect(0,0,640,360),0,0); pixels.Apply();
            var center = pixels.GetPixel(320,180);
            Require(center.r > .95f && center.g < .05f && center.b < .05f, "Secondary camera did not capture UGUI marker: " + center);
            Require(root.rootCanvas.renderMode == RenderMode.ScreenSpaceCamera && root.rootCanvas.worldCamera == root.uiCamera, "Capture did not restore canvas mode/camera");
            Require(Vector3.Distance(canvasPosition,root.rootCanvas.transform.position)<.001f && Vector3.Distance(canvasScale,root.rootCanvas.transform.lossyScale)<.00001f,"Capture changed root placement");
            report.Add("PASS secondary skill camera captures UGUI marker and restores screen-space layout.");
        }
        finally
        {
            RenderTexture.active = null; capture.targetTexture = oldTarget; capture.clearFlags = oldFlags; capture.backgroundColor = oldColor;
            if (pixels != null) Object.DestroyImmediate(pixels);
            Object.DestroyImmediate(go); Object.DestroyImmediate(rt);
        }
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
#endif
