#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseBall.BallPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class FieldMinimapMigrationChecks
{
    public static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

    [MenuItem("Tools/UI Migration/Run Field Minimap Presentation Checks")]
    public static void Run()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        string report;
        try
        {
            var map = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(FieldMinimapUGUIConverter.MapPath));
            SceneManager.MoveGameObjectToScene(map, scene);
            var view = map.GetComponent<FieldMinimapView>();
            var count = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(FieldMinimapUGUIConverter.CountPath));
            SceneManager.MoveGameObjectToScene(count, scene);
            map.SetActive(false);
            var marker = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(FieldMinimapUGUIConverter.RunnerPath), view.runners, false);
            var runner = marker.GetComponent<MinimapRunnerView>();
            runner.SetPresentation(1, "HIDDEN", false);
            Require(runner.team.transform.parent == view.teams && !runner.team.gameObject.activeSelf && !runner.playerName.gameObject.activeSelf,
                "Runner can be initialized before the field UI is shown");
            map.SetActive(true);
            int cases = 0;
            for (int team = 1; team <= 10; team++)
            foreach (string name in new[] { "A", "RUNNER", "AAAAAAAAAAAAAA" })
            foreach (bool hide in new[] { false, true })
            {
                runner.SetPresentation(team, name, hide);
                Require(runner.team.sprite.name == "minimap_team" + team && runner.playerName.text == name, "Team and name binding");
                Require(runner.team.gameObject.activeSelf && runner.namebar.gameObject.activeSelf == !hide && runner.playerName.gameObject.activeSelf == !hide, "Special-mode label visibility");
                Require(runner.team.transform.parent == view.teams && runner.namebar.transform.parent == view.namebars && runner.playerName.transform.parent == view.names,
                    "Global depth layers preserve marker overlap order");
                Require(runner.playerName.font != null && runner.playerName.fontSize == 12 && runner.playerName.enableAutoSizing, "Original font and name fitting");
                cases++;
            }
            map.transform.localScale = Vector3.one * .75f;
            foreach (var position in new[] { new Vector3(0,-84), new Vector3(84,0), new Vector3(0,84), new Vector3(-84,0), new Vector3(42,42) })
            {
                marker.transform.localPosition = position;
                runner.SyncPosition();
                Require(Vector3.Distance(runner.team.transform.position, marker.transform.position) < .01f &&
                    Vector3.Distance(runner.playerName.transform.position, marker.transform.TransformPoint(new Vector3(0,30))) < .01f,
                    "Detached graphics follow scaled minimap position");
            }
            view.rendering.ApplyRendering(.35f, 3070, 2, null);
            foreach (var root in new[] { map, count, marker })
            {
                Require(root.GetComponentsInChildren<UIWidget>(true).Length == 0 && root.GetComponentsInChildren<UITweener>(true).Length == 0, "No NGUI visual component");
                Require(root.GetComponentsInChildren<GraphicRaycaster>(true).Length == 0 && root.GetComponentsInChildren<Collider>(true).Length == 0 &&
                    root.GetComponentsInChildren<Graphic>(true).All(g => !g.raycastTarget), "Display cannot intercept input");
            }
            Require(view.rendering.opacity.alpha == .35f && !view.rendering.opacity.blocksRaycasts && view.rendering.displayCanvas.sortingOrder == 2 &&
                runner.team.material.renderQueue == 3070 && runner.playerName.fontSharedMaterial.renderQueue == 3070, "Inherited alpha/order and dynamic material queue");
            // Normal MonoBehaviour lifecycle callbacks do not run in this edit-mode
            // preview scene. Automatic hide/destroy behavior is checked in Play Mode.
            report = "PASS " + cases + " team/name/mode combinations, initialization under an inactive field UI, scaled positions, global depth layers, inherited rendering and no input interception. Lifecycle is checked separately in Play Mode.\n";
        }
        catch (Exception exception) { report = "FAIL " + exception; Debug.LogException(exception); }
        finally { ClosePreview(scene); }
        File.WriteAllText(FieldMinimapUGUIConverter.Output + "/presentation-checks.txt", report);
        Capture(true);
    }

    public static void Capture(bool migrated)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath).GetComponentInChildren<UIFieldUI>(true);
        if (!migrated && source.UguiMinimap != null) throw new InvalidOperationException("Preserve original capture after migration");
        var scene = EditorSceneManager.NewPreviewScene();
        var previous = RenderTexture.active;
        RenderTexture texture = null; Texture2D pixels = null; Camera camera = null;
        try
        {
            var root = new GameObject("Field minimap rendering fixture") { layer = source.gameObject.layer };
            SceneManager.MoveGameObjectToScene(root, scene);
            var panel = root.AddComponent<UIPanel>();
            var map = Object.Instantiate(migrated ? AssetDatabase.LoadAssetAtPath<GameObject>(FieldMinimapUGUIConverter.MapPath) : source.minimap, root.transform, false);
            var count = Object.Instantiate(migrated ? AssetDatabase.LoadAssetAtPath<GameObject>(FieldMinimapUGUIConverter.CountPath) : source.count.gameObject, root.transform, false);
            map.transform.localPosition = new Vector3(320,-100);
            count.transform.localPosition = new Vector3(330.1172f,-214.60938f);
            var positions = new[] { new Vector3(84,0), new Vector3(0,84), new Vector3(-84,0), new Vector3(42,42) };
            for (int i = 0; i < positions.Length; i++)
            {
                var view = map.GetComponent<FieldMinimapView>();
                var marker = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(migrated ? FieldMinimapUGUIConverter.RunnerPath : FieldMinimapUGUIConverter.LegacyRunnerPath),
                    migrated ? view.runners : map.transform, false);
                marker.transform.localPosition = positions[i];
                if (migrated)
                {
                    marker.GetComponent<MinimapRunnerView>().SetPresentation(i + 1, "RUNNER " + (i + 1), false);
                }
                else
                {
                    var controller = marker.GetComponent<miniRunner>();
                    controller._team.spriteName = "minimap_team" + (i + 1);
                    controller._name.text = "RUNNER " + (i + 1);
                }
            }
            var cameraObject = new GameObject("Rendering camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            camera = cameraObject.GetComponent<Camera>();
            camera.scene = scene; camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
            camera.orthographic = true; camera.orthographicSize = 360;
            camera.transform.position = new Vector3(0,0,-1000); camera.nearClipPlane = .1f; camera.farClipPlane = 2000;
            camera.cullingMask = 1 << root.layer; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.12f,.16f,.2f,1);
            texture = new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32); camera.targetTexture = texture;
            if (migrated)
            {
                map.GetComponent<FieldOverlayCanvas>().ApplyRendering(1,3000,0,camera);
                count.GetComponent<FieldOverlayCanvas>().ApplyRendering(1,3000,0,camera);
            }
            StartRect(panel);
            foreach (var widget in root.GetComponentsInChildren<UIWidget>(true)) { StartRect(widget); widget.CreatePanel(); }
            panel.Refresh();
            foreach (var drawCall in panel.drawCalls) SceneManager.MoveGameObjectToScene(drawCall.gameObject, scene);
            Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = texture;
            pixels = new Texture2D(1280,720,TextureFormat.RGBA32,false);
            pixels.ReadPixels(new Rect(0,0,1280,720),0,0); pixels.Apply();
            var colors = pixels.GetPixels32();
            Require(colors.Count(c => !c.Equals(colors[colors.Length-1])) > 500, "Nonblank capture");
            File.WriteAllBytes(FieldMinimapUGUIConverter.Output + (migrated ? "/ugui.png" : "/legacy.png"), pixels.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            if (camera != null) camera.targetTexture = null;
            if (pixels != null) Object.DestroyImmediate(pixels);
            if (texture != null) Object.DestroyImmediate(texture);
            ClosePreview(scene);
        }
    }

    private static void StartRect(UIRect rect)
    {
        // SendMessage would also invoke Start on miniRunner in an edit-only preview.
        typeof(UIRect).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(rect, null);
    }

    private static void ClosePreview(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        foreach (var canvas in root.GetComponentsInChildren<FieldOverlayCanvas>(true))
            typeof(FieldOverlayCanvas).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(canvas, null);
        EditorSceneManager.ClosePreviewScene(scene);
    }
}
#endif
