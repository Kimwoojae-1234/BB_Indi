#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class RunnerControlMigrationChecks
{
    public static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    [MenuItem("Tools/UI Migration/Run Runner Control Presentation Checks")]
    public static void Run()
    {
        Directory.CreateDirectory(RunnerControlUGUIConverter.Output);
        var scene = EditorSceneManager.NewPreviewScene();
        var colliders = new List<Collider>();
        string report;
        try
        {
            var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(RunnerControlUGUIConverter.Candidate));
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.localPosition = Vector3.zero;
            root.SetActive(true);
            var view = root.GetComponent<RunnerControlView>();
            Require(root.GetComponentsInChildren<UIWidget>(true).Length == 0 && root.GetComponentsInChildren<UITweener>(true).Length == 0 &&
                root.GetComponentsInChildren<UIEventTrigger>(true).Length == 0 && root.GetComponentsInChildren<Collider>(true).Length == 0, "NGUI-free branch");
            Require(root.GetComponentsInChildren<Graphic>(true).Count(g => g.raycastTarget) == 3, "Three base input targets only");
            Require(!view.intentionalWalk.activeSelf, "Unused intentional walk stays hidden");
            int cases = 0;
            for (int attack = 0; attack < 2; attack++)
            for (int mask = 0; mask < 8; mask++)
            for (int skillMask = 0; skillMask < 8; skillMask++)
            foreach (int value in new[] { 59, 60, 79, 80, 99, 100 })
            {
                for (int i = 0; i < 3; i++)
                {
                    bool occupied = (mask & (1 << i)) != 0, skill = (skillMask & (1 << i)) != 0;
                    view.SetRunner(i, occupied, value, skill, attack == 1);
                    Require(view.occupied[i].gameObject.activeSelf == occupied && view.action[i].gameObject.activeSelf == occupied &&
                        view.speed[i].gameObject.activeSelf == occupied && view.skill[i].gameObject.activeSelf == (occupied && skill) &&
                        !view.light[i].gameObject.activeSelf, "Base/skill visibility");
                    Color color = value >= 100 ? new Color(.74f,.15f,.89f) : value >= 80 ? new Color(.96f,.16f,.16f) : value >= 60 ? new Color(.16f,.58f,1) : new Color(.38f,.45f,.84f);
                    Require(view.speed[i].text == value.ToString() && view.speed[i].color == color && view.speed[i].font != null &&
                        view.speed[i].GetComponent<Outline>().effectColor == Color.white, "Speed/font/outline/tier");
                    Require(view.action[i].sprite.name == (attack == 1 ? "steal_1" : "pickoff_1"), "Mode label");
                    view.Select(i, attack == 1);
                    Require(view.occupied[i].sprite.name == "runnercon_steal" && view.action[i].sprite.name == (attack == 1 ? "steal_2" : "pickoff_2") &&
                        view.light[i].gameObject.activeSelf == occupied, "Selected presentation; empty base stays dark");
                }
                cases++;
            }
            var camera = MakeCamera(scene);
            var events = new GameObject("Fixture EventSystem", typeof(EventSystem));
            SceneManager.MoveGameObjectToScene(events, scene);
            view.canvas.SetInheritedRendering(1, 3040, 0, camera, true);
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var raycaster = root.GetComponent<GraphicRaycaster>();
            foreach (var button in view.bases)
            {
                // Preview scenes exclude colliders from physics. Keep disposable,
                // unsaved proxies in the active scene and destroy them in finally.
                var proxy = new GameObject("Legacy capsule") { hideFlags = HideFlags.HideAndDontSave };
                proxy.AddComponent<CapsuleCollider>();
                proxy.transform.position = button.transform.position;
                var capsule = proxy.GetComponent<CapsuleCollider>();
                capsule.radius = 40; capsule.height = 1;
                colliders.Add(capsule);
            }
            Physics.SyncTransforms();
            int hitCases = 0;
            for (float x = -95.3f; x < 95; x += 5)
            for (float y = -42.1f; y < 95; y += 5)
            {
                Vector2 point = camera.WorldToScreenPoint(new Vector3(x, y, 0));
                var hits = new List<RaycastResult>();
                raycaster.Raycast(new PointerEventData(events.GetComponent<EventSystem>()) { position = point }, hits);
                // Preview-scene colliders are not in Physics.defaultPhysicsScene.
                var legacyHits = new List<RaycastHit>();
                foreach (var collider in colliders)
                    if (collider.Raycast(camera.ScreenPointToRay(point), out var hit, 2000)) legacyHits.Add(hit);
                var oldHits = legacyHits.OrderBy(h => h.distance).ToArray();
                Require((hits.Count > 0) == (oldHits.Length > 0), "Circle boundary at " + x + "," + y + " UGUI=" + hits.Count + " legacy=" + oldHits.Length +
                    " button=" + view.bases[2].transform.position + " depth=" + view.bases[2].targetGraphic.depth + " filter=" + view.bases[2].IsRaycastLocationValid(point,camera));
                if (oldHits.Length > 0)
                    Require(hits[0].gameObject == view.bases[colliders.IndexOf(oldHits[0].collider)].gameObject, "Overlapping circles choose nearest base");
                hitCases++;
            }
            view.canvas.SetInheritedRendering(.25f, 3040, 2, camera, false);
            var blocked = new List<RaycastResult>();
            raycaster.Raycast(new PointerEventData(events.GetComponent<EventSystem>()) { position = camera.WorldToScreenPoint(view.bases[0].transform.position) }, blocked);
            Require(blocked.Count == 0 && !view.canvas.opacity.interactable && view.canvas.opacity.alpha == .25f && view.canvas.displayCanvas.sortingOrder == 2,
                "Fade/pause gate and inherited opacity/order");
            report = "PASS " + cases + " presentation combinations and " + hitCases + " raycasts compared with legacy capsule geometry, visibility, speed tiers, selection, font/outline, input gate, NGUI-free branch.\nEqual-depth overlap was unspecified in NGUI; UGUI chooses the nearest centre.\n";
        }
        catch (Exception exception) { report = "FAIL " + exception; Debug.LogException(exception); }
        finally
        {
            foreach (var collider in colliders) if (collider != null) Object.DestroyImmediate(collider.gameObject);
            EditorSceneManager.ClosePreviewScene(scene);
        }
        File.WriteAllText(RunnerControlUGUIConverter.Output + "/presentation-checks.txt", report);
        Capture(true, false);
        Capture(true, true);
    }

    private static Camera MakeCamera(Scene scene)
    {
        var obj = new GameObject("Rendering camera", typeof(Camera));
        SceneManager.MoveGameObjectToScene(obj, scene);
        var camera = obj.GetComponent<Camera>();
        camera.scene = scene;
        camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
        camera.orthographic = true; camera.orthographicSize = 360;
        camera.transform.position = new Vector3(0, 0, -1000);
        camera.nearClipPlane = .1f; camera.farClipPlane = 2000;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.12f,.16f,.2f,1);
        return camera;
    }

    public static void Capture(bool migrated, bool selected)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath).GetComponentInChildren<ControlRunner>(true);
        if (!migrated && source.UguiView != null) throw new InvalidOperationException("Preserve legacy captures after migration.");
        var scene = EditorSceneManager.NewPreviewScene();
        RenderTexture texture = null;
        Texture2D pixels = null;
        var previous = RenderTexture.active;
        try
        {
            var root = new GameObject("Runner rendering fixture");
            root.layer = source.gameObject.layer;
            SceneManager.MoveGameObjectToScene(root, scene);
            var panel = root.AddComponent<UIPanel>();
            var board = Object.Instantiate(migrated ? AssetDatabase.LoadAssetAtPath<GameObject>(RunnerControlUGUIConverter.Candidate) : source._active, root.transform, false);
            board.SetActive(true);
            board.transform.localPosition = new Vector3(-380,-120,0);
            var view = board.GetComponent<RunnerControlView>();
            for (int i = 0; i < 3; i++)
            {
                int value = new[] {59,80,105}[i];
                if (view != null)
                {
                    view.SetRunner(i, true, value, i == 1, !selected);
                    if (selected) view.Select(i, false);
                    view.light[i].color = Color.white;
                }
                else
                {
                    var onbase = board.transform.Find("base" + (i + 1) + "/onbase");
                    onbase.gameObject.SetActive(true);
                    onbase.GetComponent<UISprite>().spriteName = selected ? "runnercon_steal" : "runnercon_onbase";
                    onbase.Find("light").gameObject.SetActive(selected);
                    onbase.Find("light").GetComponent<TweenAlpha>().enabled = false;
                    onbase.Find("light").GetComponent<UISprite>().alpha = 1;
                    onbase.Find("skillIcon").gameObject.SetActive(i == 1);
                    onbase.Find("steal").GetComponent<UISprite>().spriteName = selected ? "pickoff_2" : "steal_1";
                    var label = onbase.Find("overall").GetComponent<UILabel>();
                    label.text = value.ToString();
                    label.color = value >= 100 ? new Color(.74f,.15f,.89f) : value >= 80 ? new Color(.96f,.16f,.16f) : new Color(.38f,.45f,.84f);
                }
            }
            if (view == null) board.transform.Find("intwalk").gameObject.SetActive(false);
            var camera = MakeCamera(scene);
            camera.cullingMask = 1 << root.layer;
            texture = new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);
            camera.targetTexture = texture;
            if (view != null) view.canvas.SetInheritedRendering(1,3000,0,camera,true);
            panel.SendMessage("Start");
            foreach (var widget in board.GetComponentsInChildren<UIWidget>(true)) { widget.SendMessage("Start"); widget.CreatePanel(); }
            panel.Refresh();
            foreach (var drawCall in panel.drawCalls) SceneManager.MoveGameObjectToScene(drawCall.gameObject, scene);
            Canvas.ForceUpdateCanvases(); camera.Render();
            RenderTexture.active = texture;
            pixels = new Texture2D(1280,720,TextureFormat.RGBA32,false);
            pixels.ReadPixels(new Rect(0,0,1280,720),0,0); pixels.Apply();
            string name = (migrated ? "ugui" : "legacy") + (selected ? "-selected" : "");
            File.WriteAllBytes(RunnerControlUGUIConverter.Output + "/" + name + ".png", pixels.EncodeToPNG());
            var colors = pixels.GetPixels32();
            Require(colors.Count(c => !c.Equals(colors[colors.Length-1])) > 500, "Nonblank capture");
        }
        finally
        {
            RenderTexture.active = previous;
            if (pixels != null) Object.DestroyImmediate(pixels);
            if (texture != null) Object.DestroyImmediate(texture);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
#endif
