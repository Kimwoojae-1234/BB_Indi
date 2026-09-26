#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using BaseBall.BallPlay;
using BaseBall.BallPlay.UGUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class IntegratedUGUIPlayChecks
{
    private static string Output => SessionState.GetString("BB.Integrated.Output", IntegratedUGUIConverter.Output);
    private static float nextInput;
    private static int inputFrame;
    static IntegratedUGUIPlayChecks()
    {
        EditorApplication.update += Assist;
        EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.EnteredEditMode) SessionState.SetBool("BB.Integrated.Assist", false); };
    }
    public static void EnableAssist() { SessionState.SetBool("BB.Integrated.Assist", true); }
    public static void ChangeModeFixture(bool automatic)
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Play Mode required");
        if (automatic) IngameUI.GetScoreBoard().setGameSpeedControl();
        else Object.FindFirstObjectByType<QuickSimulator>(FindObjectsInactive.Include).goToGame();
        File.AppendAllText(Output + "/input-checks.txt", "FIXTURE " + Time.time + " existing mode API; automatic=" + automatic + "; mode=" + Mode.PlayTypeFlag + "; input visibility unchanged\n");
    }
    private static void Assist()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool("BB.Integrated.Assist", false)) return;
        if (inputFrame != Time.frameCount && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))
        {
            inputFrame = Time.frameCount;
            File.AppendAllText(Output + "/input-checks.txt", "DEVICE_INPUT " + Time.time + " down=" + Input.GetMouseButtonDown(0) + " up=" + Input.GetMouseButtonUp(0) + " screen=" + Input.mousePosition + "\n");
        }
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        if (manager == null || manager.bMyTurn || Mode.bPauseGame || Mode.PlayTypeFlag != Mode.ModeFlag.Manual || Time.unscaledTime < nextInput) return;
        var selector = IngameUI.GetPitchingSelect();
        if (selector._active.activeInHierarchy && selector.GetSelectBall() == PitchingArsenal.NONE)
        {
            var row = selector.button.FirstOrDefault(b => b.gameObject.activeInHierarchy && b.UguiView != null && b.UguiView.interactable);
            if (row == null || EventSystem.current == null) return;
            var data = new PointerEventData(EventSystem.current) { pointerId = -1, button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(row.gameObject, data, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(row.gameObject, data, ExecuteEvents.pointerUpHandler);
            File.AppendAllText(Output + "/input-checks.txt", "TEST_INPUT pitch selection through Unity pointer events at " + Time.time + "\n");
            nextInput = Time.unscaledTime + 2;
        }
        else if (manager.playState == PlayState.PLAY_BATTING_VIEW && manager.pitcher.pState == PitcherState._GET_SIGN && !manager.pitcher.bRelease)
        {
            var control = Object.FindFirstObjectByType<ControlPitchingUI>();
            if (control == null || !control._active.activeInHierarchy) return;
            control.setRelease(); nextInput = Time.unscaledTime + 4;
            File.AppendAllText(Output + "/input-checks.txt", "TEST_INPUT existing pitch-release API at " + Time.time + "\n");
        }
    }
    public static void ClickFixture(string method)
    {
        var pointer = FindAction(method);
        bool enabled = pointer.inputCollider.enabled;
        pointer.inputCollider.enabled = true;
        try { ClickAction(method); }
        finally { if (pointer != null) pointer.inputCollider.enabled = enabled; }
        File.AppendAllText(Output + "/input-checks.txt", "FIXTURE collider temporarily enabled for " + method + "; original enabled=" + enabled + "; asset unchanged\n");
    }
    private static GameUIPointer FindAction(string method)
    {
        var pointer = Object.FindObjectsByType<GameUIPointer>(FindObjectsSortMode.None).FirstOrDefault(p => p.onPress.Concat(p.onRelease).Concat(p.onClick).Any(a => a.mMethodName == method));
        if (pointer == null) throw new InvalidOperationException("No active input for " + method);
        return pointer;
    }
    public static void ClickAction(string method)
    {
        var pointer = FindAction(method);
        var target = pointer.GetComponentsInChildren<GameUIHitTarget>().First(h => h.pointer == pointer);
        Canvas.ForceUpdateCanvases();
        var screen = RectTransformUtility.WorldToScreenPoint(target.canvas.worldCamera, target.rectTransform.TransformPoint(target.rectTransform.rect.center));
        var data = new PointerEventData(EventSystem.current) { pointerId = -1, button = PointerEventData.InputButton.Left, position = screen, pressPosition = screen };
        var hits = new System.Collections.Generic.List<RaycastResult>(); EventSystem.current.RaycastAll(data, hits);
        if (hits.Count == 0 || hits[0].gameObject.GetComponentInParent<GameUIPointer>() != pointer)
            throw new InvalidOperationException(method + " raycast failed: " + screen + " depth=" + target.depth + " culled=" + target.canvasRenderer.cull + " enabled=" + target.raycastTarget + " rect=" + target.rectTransform.rect + " camera=" + target.canvas.worldCamera + " localFilter=" + target.IsRaycastLocationValid(screen, target.canvas.worldCamera) + " graphicFilter=" + target.Raycast(screen, target.canvas.worldCamera) + " groups=" + string.Join(",", target.GetComponentsInParent<CanvasGroup>().Select(g => g.name + ":" + g.blocksRaycasts + ":" + g.alpha)) + " raycasters=" + string.Join(",", RaycasterManager.GetRaycasters().Select(r => r.name + ":" + r.isActiveAndEnabled)) + " hits=" + string.Join(",", hits.Select(h => h.gameObject.name)));
        data.pointerCurrentRaycast = data.pointerPressRaycast = hits[0];
        ExecuteEvents.ExecuteHierarchy(target.gameObject, data, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.ExecuteHierarchy(target.gameObject, data, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.ExecuteHierarchy(target.gameObject, data, ExecuteEvents.pointerClickHandler);
        File.AppendAllText(Output + "/input-checks.txt", "TEST_INPUT " + Time.time + " " + method + " through EventSystem.RaycastAll + Unity pointer events; paused=" + Mode.bPauseGame + " mode=" + Mode.PlayTypeFlag + "\n");
        Snapshot();
    }
    public static void Snapshot()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Play Mode required");
        var text = new StringBuilder();
        text.AppendLine("STATE time=" + Time.time + " mode=" + Mode.PlayTypeFlag + " paused=" + Mode.bPauseGame);
        foreach (var camera in Camera.allCameras)
            text.AppendLine("CAM " + camera.name + " depth=" + camera.depth + " mask=" + camera.cullingMask + " ortho=" + camera.orthographicSize + " pos=" + camera.transform.position + " far=" + camera.farClipPlane);
        foreach (var root in Object.FindObjectsByType<GameUIRoot>(FindObjectsSortMode.None))
            text.AppendLine("ROOT " + root.name + " pos=" + root.transform.position + " scale=" + root.transform.lossyScale + " camera=" + root.uiCamera);
        foreach (var panel in Object.FindObjectsByType<GameUIPanel>(FindObjectsSortMode.None))
            text.AppendLine("PANEL " + panel.name + " depth=" + panel.depth + " order=" + panel.Order + " alpha=" + panel.CalculateFinalAlpha(Time.frameCount));
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.enabled).Take(400))
        {
            var camera = canvas.worldCamera;
            text.AppendLine("CANVAS " + AnimationUtility.CalculateTransformPath(canvas.transform, null) + " order=" + canvas.sortingOrder + " camera=" + camera + " pos=" + canvas.transform.position + " scale=" + canvas.transform.lossyScale + " screen=" + (camera != null ? camera.WorldToScreenPoint(canvas.transform.position) : Vector3.zero));
        }
        var ngui = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(c => c != null && c.enabled && AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(c)).StartsWith("Assets/NGUI/")).ToArray();
        text.AppendLine("ACTIVE_NGUI " + string.Join(",", ngui.Select(c => c.name + ":" + c.GetType().Name)));
        File.WriteAllText(Output + "/runtime-snapshot.txt", text.ToString());
        ScreenCapture.CaptureScreenshot(Output + "/gameplay-current.png");
    }
    public static void RepairRenderers()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first");
        var plan = JsonUtility.FromJson<IntegratedUGUIConverter.Plan>(File.ReadAllText(IntegratedUGUIConverter.Output + "/candidate-plan.json"));
        foreach (var path in Directory.GetFiles(IntegratedUGUIConverter.Folder, "Atlas-*.asset"))
        {
            string guid = Path.GetFileNameWithoutExtension(path).Substring(6);
            var atlas = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)).GetComponent<GameUIAsset>();
            var catalog = AssetDatabase.LoadAssetAtPath<GameUISpriteCatalog>(path);
            catalog.material = atlas != null && atlas.sprites != null ? atlas.sprites.material : null;
            EditorUtility.SetDirty(catalog);
        }
        AssetDatabase.SaveAssets();
        int count = 0;
        foreach (var asset in plan.assets)
        {
            var root = PrefabUtility.LoadPrefabContents(asset.source);
            try
            {
                bool changed = false;
                foreach (var element in root.GetComponentsInChildren<GameUIElement>(true))
                    if (element.graphic != null) { element.Apply(); changed = true; }
                foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                {
                    if (graphic.GetComponent<CanvasRenderer>() == null) { graphic.gameObject.AddComponent<CanvasRenderer>(); changed = true; count++; }
                    var material = IntegratedUGUIConverter.NativeMaterial(graphic.material);
                    if (material != graphic.material) { graphic.material = material; changed = true; }
                }
                if (changed || root.GetComponentInChildren<GameUIHitTarget>(true) != null) PrefabUtility.SaveAsPrefabAsset(root, asset.source);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        File.WriteAllText(IntegratedUGUIConverter.Output + "/renderer-repair.txt", "Added missing CanvasRenderer components: " + count);
    }
}
#endif
