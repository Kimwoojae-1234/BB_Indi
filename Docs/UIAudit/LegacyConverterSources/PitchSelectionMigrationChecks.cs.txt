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

public static class PitchSelectionMigrationChecks
{
    private const string PrefabPath = ScoreboardMigrationChecks.PrefabPath;
    public static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    [MenuItem("Tools/UI Migration/Run Pitch Selection Presentation Checks")]
    public static void Run()
    {
        Directory.CreateDirectory(PitchSelectionUGUIConverter.Output);
        var scene = EditorSceneManager.NewPreviewScene();
        string report;
        try
        {
            var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PitchSelectionUGUIConverter.Candidate));
            SceneManager.MoveGameObjectToScene(root, scene);
            root.SetActive(true);
            root.transform.localPosition = Vector3.zero;
            var view = root.GetComponent<PitchSelectionCanvas>();
            var rows = root.GetComponentsInChildren<pitchingSelectButton>(true);
            Require(rows.Length == 5, "Five button bindings");
            Require(root.GetComponentsInChildren<UIWidget>(true).Length == 0 && root.GetComponentsInChildren<UITweener>(true).Length == 0 &&
                root.GetComponentsInChildren<UIEventTrigger>(true).Length == 0 && root.GetComponentsInChildren<Collider>(true).Length == 0, "No NGUI components or colliders");
            int cases = 0;
            foreach (var adapter in rows)
            {
                var button = adapter.UguiView;
                Require(button != null && button.number.font != null && button.movement != null, "UGUI bindings");
                Require(button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentTarget(0) == adapter &&
                    button.onClick.GetPersistentMethodName(0) == "pushButton", "Release callback targets legacy controller adapter");
                for (int pitch = 1; pitch <= 24; pitch++)
                foreach (int value in new[] { 0, 59, 60, 79, 80, 99, 100, 150 })
                {
                    button.SetPresentation((PitchingArsenal)pitch, value);
                    Require(button.pitchName.enabled && button.pitchName.sprite.name == "pselect_" + pitch, "Pitch sprite " + pitch);
                    Require(button.number.text == value.ToString(), "Strength number");
                    int tier = value >= 100 ? 4 : value >= 80 ? 3 : value >= 60 ? 2 : 1;
                    Require(button.strength.sprite.name == "pselect_ball_stat" + tier, "Strength tier boundary");
                    cases++;
                }
                foreach (var pitch in new[] { PitchingArsenal.H_FORK, PitchingArsenal.UPSHOOT })
                {
                    button.SetPresentation(pitch, 70);
                    Require(!button.pitchName.enabled, "Preserve pre-existing absent special pitch art");
                }
                Require(button.targetGraphic.raycastTarget && button.targetGraphic.raycastPadding == new Vector4(0, 3, 0, 3), "230x58 hit rectangle");
                Require(button.GetComponentsInChildren<Graphic>(true).Count(g => g.raycastTarget) == 1, "Only background accepts input");
                Require(button.light.transform.GetSiblingIndex() < button.number.transform.GetSiblingIndex() &&
                    button.effect.transform.parent.GetSiblingIndex() > button.transform.GetSiblingIndex(), "Flash / number / ring order");
                button.SetPresentation(PitchingArsenal.FASTBALL, 70);
            }
            var cameraObject = new GameObject("Raycast fixture camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 360;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 100;
            camera.scene = scene;
            camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
            var eventObject = new GameObject("Fixture EventSystem", typeof(EventSystem));
            SceneManager.MoveGameObjectToScene(eventObject, scene);
            var eventSystem = eventObject.GetComponent<EventSystem>();
            view.SetInheritedRendering(1, 3021, 2, camera, true);
            var raycaster = root.GetComponent<GraphicRaycaster>();
            for (int count = 1; count <= 5; count++)
            {
                for (int i = 0; i < 5; i++)
                {
                    rows[i].gameObject.SetActive(i < count);
                    rows[i].transform.localPosition = new Vector3(20 * i, 66 * i - 160, 0);
                }
                Canvas.ForceUpdateCanvases();
                camera.Render();
                for (int i = 0; i < count; i++)
                {
                    var data = new PointerEventData(eventSystem) { position = camera.WorldToScreenPoint(rows[i].transform.position) };
                    var hits = new List<RaycastResult>();
                    raycaster.Raycast(data, hits);
                    Require(hits.Count == 1 && hits[0].gameObject == rows[i].gameObject, "Hit each visible slot, count=" + count + ", slot=" + i + ", hits=" + hits.Count + ", depth=" + rows[i].UguiView.targetGraphic.depth + ", position=" + data.position);
                }
                foreach (var offset in new[] { new Vector3(0, 28, 0), new Vector3(0, 30, 0), new Vector3(114, 0, 0), new Vector3(116, 0, 0) })
                {
                    var hits = new List<RaycastResult>();
                    raycaster.Raycast(new PointerEventData(eventSystem) { position = camera.WorldToScreenPoint(rows[0].transform.TransformPoint(offset)) }, hits);
                    bool inside = Mathf.Abs(offset.x) < 115 && Mathf.Abs(offset.y) < 29;
                    Require(hits.Any(h => h.gameObject == rows[0].gameObject) == inside, "Hit rectangle edge " + offset);
                }
            }
            view.SetInheritedRendering(.35f, 3021, 2, camera, false);
            Canvas.ForceUpdateCanvases();
            var blocked = new List<RaycastResult>();
            raycaster.Raycast(new PointerEventData(eventSystem) { position = camera.WorldToScreenPoint(rows[0].transform.position) }, blocked);
            Require(blocked.Count == 0 && !view.opacity.interactable, "Pause/input gate");
            Require(Mathf.Approximately(view.opacity.alpha, .35f) && view.displayCanvas.sortingOrder == 2 &&
                root.GetComponentsInChildren<Graphic>(true).All(g => (g is TMPro.TMP_Text t ? t.fontSharedMaterial : g.material).renderQueue == 3021), "Inherited rendering");
            report = "PASS " + cases + " presentation cases, 1–5 slot raycasts, callback bindings, hit geometry, effect order, pause gate, alpha/order, NGUI-free branch.\n" +
                "Known source gap: pselect_106 / pselect_107 art is absent; preserved blank labels.\nEditor fixture; native touch and PVP are not covered.\n";
        }
        catch (Exception exception) { report = "FAIL " + exception; Debug.LogException(exception); }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
        File.WriteAllText(PitchSelectionUGUIConverter.Output + "/presentation-checks.txt", report);
        Capture(true, false);
        Capture(true, true);
    }
    public static void Capture(bool migrated, bool effects)
    {
        Directory.CreateDirectory(PitchSelectionUGUIConverter.Output);
        Scene scene = EditorSceneManager.NewPreviewScene();
        RenderTexture texture = null;
        Texture2D pixels = null;
        RenderTexture previous = RenderTexture.active;
        try
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)
                .GetComponentInChildren<BaseBall.BallPlay.ControlPitchingSelect>(true);
            if (!migrated && source.UguiView != null)
                throw new InvalidOperationException("Game prefab is migrated. Preserve the existing legacy baseline capture.");
            var root = new GameObject("Pitch selection rendering fixture");
            SceneManager.MoveGameObjectToScene(root, scene);
            root.layer = source.gameObject.layer;
            var panel = root.AddComponent<UIPanel>();
            var template = migrated ? AssetDatabase.LoadAssetAtPath<GameObject>(PitchSelectionUGUIConverter.Candidate) : source._active;
            var board = Object.Instantiate(template, root.transform, false);
            board.name = "board";
            board.SetActive(true);
            board.transform.localPosition = new Vector3(280, -200, 0);
            foreach (var tween in board.GetComponentsInChildren<UITweener>(true))
                tween.enabled = false;
            foreach (var tween in board.GetComponentsInChildren<BaseBall.BallPlay.ScoreboardPositionTween>(true))
                tween.enabled = false;

            var rows = board.GetComponentsInChildren<BaseBall.BallPlay.pitchingSelectButton>(true);
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].transform.localPosition = new Vector3(20 * i, 66 * i, 0);
                var row = rows[i].UguiView;
                if (row != null)
                {
                    row.SetPresentation((BaseBall.BallPlay.PitchingArsenal)(i + 1), 50 + 15 * i);
                    row.light.SetActive(effects && i == 0);
                    row.effect.gameObject.SetActive(effects && i == 2);
                    row.SetEffectScale(.87f);
                    row.effect.color = Color.white;
                }
                else
                {
                    rows[i].text.spriteName = "pselect_" + (i + 1);
                    int value = 50 + 15 * i;
                    rows[i].num.text = value.ToString();
                    rows[i].back.spriteName = "pselect_ball_stat" + (value >= 100 ? 4 : value >= 80 ? 3 : value >= 60 ? 2 : 1);
                    rows[i]._light.SetActive(effects && i == 0);
                    rows[i].effect.gameObject.SetActive(effects && i == 2);
                    rows[i].effect.transform.localScale = new Vector3(.87f, .87f, 1);
                    rows[i].effect.alpha = 1;
                }
            }
            var cameraObject = new GameObject("Rendering camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            var camera = cameraObject.AddComponent<Camera>();
            camera.scene = scene;
            camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
            camera.orthographic = true;
            camera.orthographicSize = 360;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.16f, 0.2f, 1);
            camera.cullingMask = 1 << root.layer;
            texture = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = texture;
            foreach (var canvas in board.GetComponentsInChildren<Canvas>(true)) canvas.worldCamera = camera;
            var view = board.GetComponent<BaseBall.BallPlay.PitchSelectionCanvas>();
            if (view != null) view.SetInheritedRendering(1, 3000, 0, camera, true);
            // This synchronous preview runs before Unity's next editor frame.
            // NGUI refuses panel creation until its Start lifecycle has run.
            panel.SendMessage("Start");
            foreach (var widget in board.GetComponentsInChildren<UIWidget>(true))
            {
                widget.SendMessage("Start");
                widget.CreatePanel();
            }
            panel.Refresh();
            // NGUI creates hidden draw-call objects in the active scene, not the
            // widget's preview scene. Keep this fixture's draw calls with its camera.
            foreach (var drawCall in panel.drawCalls)
                SceneManager.MoveGameObjectToScene(drawCall.gameObject, scene);
            File.WriteAllText(PitchSelectionUGUIConverter.Output + (migrated ? "/ugui-diagnostics.txt" : "/legacy-diagnostics.txt"), "Panel widgets: " + panel.widgets.Count +
                " draw calls: " + panel.drawCalls.Count + "\n" +
                string.Join("\n", board.GetComponentsInChildren<UIWidget>(true).Select(w =>
                    w.name + " active=" + w.gameObject.activeInHierarchy + " alpha=" + w.finalAlpha + " position=" + w.transform.position +
                    " visible=" + w.isVisible + " verts=" + w.geometry.verts.size)));
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = texture;
            pixels = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            pixels.Apply();
            string path = PitchSelectionUGUIConverter.Output + ((migrated ? "/ugui" : "/legacy") + (effects ? "-effects" : "") + ".png");
            File.WriteAllBytes(path, pixels.EncodeToPNG());
            var colors = pixels.GetPixels32();
            int visiblePixels = colors.Count(color => !color.Equals(colors[colors.Length - 1]));
            if (visiblePixels < 1000) throw new InvalidOperationException("Blank capture: " + visiblePixels + " visible pixels");
            File.WriteAllText(PitchSelectionUGUIConverter.Output + (migrated ? "/ugui-capture-status.txt" : "/legacy-capture-status.txt"), "CAPTURED " + path + "\nVisible pixels: " + visiblePixels + "\n" + DateTime.UtcNow.ToString("O"));
            Debug.Log("Pitch selection capture: " + path);
        }
        catch (Exception exception)
        {
            File.WriteAllText(PitchSelectionUGUIConverter.Output + "/capture-status.txt", exception.ToString());
            Debug.LogException(exception);
            throw;
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
