#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>Isolated rendering checks; never saves the user's open scenes.</summary>
public static class ScoreboardMigrationChecks
{
    public const string PrefabPath = "Assets/Resources/MainGame/prefabs/gameUI/IngameUIPrefab.prefab";
    public const string OutputPath = "Docs/UIAudit/Scoreboard";

    [MenuItem("Tools/UI Migration/Capture Legacy Scoreboard")]
    public static void CaptureLegacy()
    {
        Capture(false);
    }

    [MenuItem("Tools/UI Migration/Capture UGUI Scoreboard")]
    public static void CaptureUGUI() { Capture(true); }

    [MenuItem("Tools/UI Migration/Run Scoreboard Presentation Checks")]
    public static void RunPresentationChecks()
    {
        Directory.CreateDirectory(OutputPath);
        var report = new StringBuilder();
        GameObject instance = null;
        try
        {
            instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardUGUIConverter.BoardPath));
            instance.hideFlags = HideFlags.HideAndDontSave;
            var view = instance.GetComponent<BaseBall.BallPlay.IngameScoreboardView>();
            Require(view != null && view.entrance != null && view.displayCanvas != null, "View bindings");
            Require(instance.GetComponentsInChildren<UIWidget>(true).Length == 0 &&
                instance.GetComponentsInChildren<UITweener>(true).Length == 0, "NGUI-free display branch");
            Require(instance.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).All(g => !g.raycastTarget) &&
                instance.GetComponentsInChildren<UnityEngine.UI.GraphicRaycaster>(true).Length == 0, "No pointer interception");
            foreach (var entry in view.sprites.entries) Require(entry.sprite != null, "Sprite " + entry.name);
            foreach (var text in instance.GetComponentsInChildren<TMPro.TMP_Text>(true))
                Require(text.font != null && text.font.material != null, "Font " + text.name);
            for (int team = 1; team <= 10; team++)
            {
                view.SetTeams(team, "HOME", 11 - team, "AWAY");
                Require(view.homeLogo.sprite.name == "logo_" + team && view.awayLogo.sprite.name == "logo_" + (11 - team), "Team logos");
                Require(view.homeName.text == "HOME" && view.awayName.text == "AWAY", "Team names");
                Require(view.homeLogo.rectTransform.sizeDelta == new Vector2(50, 50), "Native logo size");
            }
            int cases = 0;
            for (int home = 0; home < 2; home++)
            for (int top = 0; top < 2; top++)
            for (int balls = 0; balls <= 4; balls++)
            for (int strikes = 0; strikes <= 3; strikes++)
            for (int outs = 0; outs <= 3; outs++)
            for (int bases = 0; bases < 8; bases++)
            {
                view.SetState(12, top == 1, home == 1, 0, 12, balls, strikes, outs,
                    (bases & 1) != 0, (bases & 2) != 0, (bases & 4) != 0);
                Require(view.homeScore.text == (home == 1 ? "0" : "12") &&
                    view.awayScore.text == (home == 1 ? "12" : "0"), "Home/away score mapping");
                Require(view.inningInfo.text == "12" && view.topBottom.sprite.name == (top == 1 ? "scoreboard_top" : "scoreboard_bottom") &&
                    view.indicator[0].activeSelf == (top == 1) && view.indicator[1].activeSelf == (top == 0), "Inning/attack mapping");
                CheckCount(view.ballCount, balls, "scoreboard_ball");
                CheckCount(view.strikeCount, strikes, "scoreboard_strike");
                CheckCount(view.outCount, outs, "scoreboard_out");
                for (int index = 0; index < 3; index++)
                    Require(view.baseOn[index].sprite.name == ((bases & (1 << index)) != 0 ? "scoreboard_baseon" : "scoreboard_base"), "Base mapping");
                cases++;
            }
            view.SetInheritedRendering(.35f, 3021, 2);
            Require(Mathf.Approximately(view.opacity.alpha, .35f) && view.displayCanvas.sortingOrder == 2 &&
                instance.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).All(g =>
                    (g is TMPro.TMP_Text label ? label.fontSharedMaterial : g.material).renderQueue == 3021), "Inherited opacity / order");
            view.entrance.PlayFromStart();
            Require(view.entrance.enabled && instance.transform.localPosition == view.entrance.from, "Entrance restart");
            view.entrance.StopAt(new Vector3(-446, 0, 0));
            Require(!view.entrance.enabled && instance.transform.localPosition == new Vector3(-446, 0, 0), "Immediate position cancels tween");
            report.AppendLine("PASS serialized bindings, 10 logo mappings, NGUI-free display, no raycasts, " + cases + " state combinations, opacity/order, tween start/stop.");
            report.AppendLine("Editor fixture only; actual gameplay and device behavior are not covered by these checks.");
        }
        catch (Exception exception) { report.AppendLine("FAIL " + exception); Debug.LogException(exception); }
        finally
        {
            if (instance != null) Object.DestroyImmediate(instance);
            File.WriteAllText(OutputPath + "/presentation-checks.txt", report.ToString());
        }
        var game = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponentInChildren<BaseBall.BallPlay.UIScoreBoard>(true);
        if (game.UguiView == null) Capture(false);
        Capture(true);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void CheckCount(UnityEngine.UI.Image[] images, int count, string lit)
    {
        for (int index = 0; index < images.Length; index++)
            Require(images[index].sprite.name == (index < count ? lit : "scoreboard_round"), "Count slot " + index);
    }

    public static void Capture(bool migrated)
    {
        Directory.CreateDirectory(OutputPath);
        Scene scene = EditorSceneManager.NewPreviewScene();
        RenderTexture texture = null;
        Texture2D pixels = null;
        RenderTexture previous = RenderTexture.active;
        try
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)
                .GetComponentInChildren<BaseBall.BallPlay.UIScoreBoard>(true);
            if (!migrated && source.UguiView != null)
                throw new InvalidOperationException("Game prefab is migrated. Preserve the existing legacy baseline capture.");
            var root = new GameObject("Scoreboard rendering fixture");
            SceneManager.MoveGameObjectToScene(root, scene);
            root.layer = source.gameObject.layer;
            var panel = root.AddComponent<UIPanel>();
            var template = migrated ? AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardUGUIConverter.BoardPath) : source.board;
            var board = Object.Instantiate(template, root.transform, false);
            board.name = "board";
            board.SetActive(true);
            board.transform.localPosition = new Vector3(-446, 272, 0);
            foreach (var tween in board.GetComponentsInChildren<UITweener>(true))
                tween.enabled = false;
            foreach (var tween in board.GetComponentsInChildren<BaseBall.BallPlay.ScoreboardPositionTween>(true))
                tween.enabled = false;

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
            var view = board.GetComponent<BaseBall.BallPlay.IngameScoreboardView>();
            if (view != null) view.SetInheritedRendering(1, 3000, 0);
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
            File.WriteAllText(OutputPath + (migrated ? "/ugui-diagnostics.txt" : "/legacy-diagnostics.txt"), "Panel widgets: " + panel.widgets.Count +
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
            string path = OutputPath + (migrated ? "/ugui.png" : "/legacy.png");
            File.WriteAllBytes(path, pixels.EncodeToPNG());
            var colors = pixels.GetPixels32();
            int visiblePixels = colors.Count(color => !color.Equals(colors[colors.Length - 1]));
            if (visiblePixels < 1000) throw new InvalidOperationException("Blank capture: " + visiblePixels + " visible pixels");
            File.WriteAllText(OutputPath + (migrated ? "/ugui-capture-status.txt" : "/legacy-capture-status.txt"), "CAPTURED " + path + "\nVisible pixels: " + visiblePixels + "\n" + DateTime.UtcNow.ToString("O"));
            Debug.Log("Scoreboard capture: " + path);
        }
        catch (Exception exception)
        {
            File.WriteAllText(OutputPath + "/capture-status.txt", exception.ToString());
            Debug.LogException(exception);
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
