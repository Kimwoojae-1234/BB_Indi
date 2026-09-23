#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using BaseBall.BallPlay;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

// Observes the normal offline match, without injecting hits or changing runners.
// Starts from the currently open Logo scene so unsaved scene state survives Play.
[InitializeOnLoad]
public static class FieldMinimapScreenChecks
{
    private const string Key = "BB.FieldMinimapScreen";
    private const string Root = "Docs/UIAudit/FieldMinimap/ScreenCheck";
    public static string Output => SessionState.GetString(Key + ".Output", Root);
    private static string lastState;
    private static double nextSample;
    private static float nextFieldCapture;
    private static string lastInput;
    private static float nextTestInput;

    static FieldMinimapScreenChecks()
    {
        EditorApplication.update += Sample;
        Application.logMessageReceived += Log;
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (!SessionState.GetBool(Key, false) || scene.name != "BallPlay") return;
            Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            Mode.bSimulationQuickPlay = false;
            Mode.bOnlyChanceMode = false;
        };
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Key, false)) return;
            SessionState.SetBool(Key, false);
            File.AppendAllText(Output + "/trace.txt", "STOPPED; Unity restored the original open scene, including its unsaved state.\n");
        };
    }

    [MenuItem("Tools/UI Migration/Start Field Screen Check")]
    public static void Start() { Begin(false); }

    [MenuItem("Tools/UI Migration/Start Field Screen Check With Test Inputs")]
    public static void StartAssisted() { Begin(true); }

    private static void Begin(bool assisted)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().name != "Logo")
            throw new InvalidOperationException("Start in Edit Mode with the original Logo scene open.");
        SessionState.SetString(Key + ".Output", assisted ? Root + "/Assisted" : Root);
        SessionState.SetBool(Key + ".Assisted", assisted);
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "/trace.txt", "Native Unity Game View observation, " + DateTime.UtcNow.ToString("O") +
            "\nOriginal Logo dirty=" + SceneManager.GetActiveScene().isDirty + "; no scene save/open operation.\n");
        if (assisted) File.AppendAllText(Output + "/trace.txt", "Synthetic pitch-selection events and existing pitch-release API; no injected hit or runner state.\n");
        File.WriteAllText(Output + "/input.txt", "Read-only native input observations.\n");
        SessionState.SetBool(Key, true);
        SessionState.SetBool(Key + ".Bootstrap", true);
        SessionState.SetInt(Key + ".Capture", 0);
        EditorApplication.isPlaying = true;
    }

    private static void Log(string condition, string stack, LogType type)
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false) ||
            (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)) return;
        File.AppendAllText(Output + "/trace.txt", "ERROR " + condition + "\n" + stack + "\n");
    }

    [MenuItem("Tools/UI Migration/Capture Field Screen")]
    public static void Capture()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false)) return;
        int number = SessionState.GetInt(Key + ".Capture", 0) + 1;
        SessionState.SetInt(Key + ".Capture", number);
        ScreenCapture.CaptureScreenshot(Output + "/frame-" + number.ToString("D2") + ".png");
        File.AppendAllText(Output + "/trace.txt", "CAPTURE " + number + " " + State() + "\n");
    }

    private static string State()
    {
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        if (manager == null) return "Scene=" + SceneManager.GetActiveScene().name;
        var field = IngameUI.GetFieldUI();
        string state = "phase=" + manager.playState + " update=" + manager.bUpdate + " inning=" + manager.nInningCount +
            " top=" + manager.bTopInning + " attack=" + manager.bMyTurn + " score=" + string.Join(",", manager.nGameScore) +
            " count=" + manager.nBallCount + "/" + manager.nStrikeCount + "/" + manager.nOutCount;
        state += " contact=" + (manager.batter != null && manager.batter.bHitted) + " hit=" + manager.strHitType;
        if (field == null) return state;
        state += " field=" + field._active.activeInHierarchy + " ugui=" + (field.UguiMinimap != null) +
            " countLights=" + string.Join(",", field.outCount.Select(o => o.activeInHierarchy)) + " markers=" + field.minimap.transform.childCount;
        return state;
    }

    private static void Sample()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false)) return;
        TrackInput();
        if (EditorApplication.timeSinceStartup < nextSample) return;
        nextSample = EditorApplication.timeSinceStartup + .1;
        if (SessionState.GetBool(Key + ".Bootstrap", false) && SceneManager.GetActiveScene().name == "MainLobby" && Time.time > 5)
        {
            SessionState.SetBool(Key + ".Bootstrap", false);
            Mode.EndRttsGame(); tempSelectPage.ConfigureDongneYagu(); UnityEngine.Random.InitState(230923);
            SceneManager.LoadScene("MainLoading"); return;
        }
        string state = State();
        if (state != lastState)
        {
            File.AppendAllText(Output + "/trace.txt", Time.time.ToString("F2") + " " + state + "\n");
            lastState = state;
        }
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        if (manager != null && SessionState.GetBool(Key + ".Assisted", false)) TestPitchInput(manager);
        if (manager == null || manager.playState != PlayState.PLAY_FIELDING_VIEW || !IngameUI.GetFieldUI()._active.activeInHierarchy ||
            Time.unscaledTime < nextFieldCapture || SessionState.GetInt(Key + ".Capture", 0) >= 30) return;
        nextFieldCapture = Time.unscaledTime + .6f;
        Capture();
    }

    private static void TrackInput()
    {
        var system = EventSystem.current;
        string value = "focused=" + Application.isFocused + " window=" + EditorWindow.focusedWindow?.GetType().Name +
            " mouse=" + Input.mousePosition + " down=" + Input.GetMouseButtonDown(0) +
            " held=" + Input.GetMouseButton(0) + " up=" + Input.GetMouseButtonUp(0) +
            " input=" + (system != null ? system.currentInputModule?.GetType().Name : "none");
        if (value == lastInput) return;
        lastInput = value;
        if (system != null)
        {
            var hits = new System.Collections.Generic.List<RaycastResult>();
            system.RaycastAll(new PointerEventData(system) { position = Input.mousePosition }, hits);
            value += " hits=" + string.Join(",", hits.Take(3).Select(h => h.gameObject.name));
        }
        File.AppendAllText(Output + "/input.txt", Time.time.ToString("F2") + " " + value + "\n");
    }

    private static void TestPitchInput(BallPlayManager manager)
    {
        if (manager.bMyTurn || Mode.bPauseGame || Time.unscaledTime < nextTestInput) return;
        var selector = IngameUI.GetPitchingSelect();
        if (selector._active.activeInHierarchy && selector.GetSelectBall() == PitchingArsenal.NONE)
        {
            var row = selector.button.FirstOrDefault(b => b.gameObject.activeInHierarchy && b.UguiView != null && b.UguiView.interactable);
            if (row == null || EventSystem.current == null) return;
            var pointer = new PointerEventData(EventSystem.current) { pointerId = -1, button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(row.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(row.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            File.AppendAllText(Output + "/trace.txt", "TEST_INPUT pitch selection through Unity pointer events\n");
            nextTestInput = Time.unscaledTime + 2;
        }
        else if (manager.playState == PlayState.PLAY_BATTING_VIEW && manager.pitcher.pState == PitcherState._GET_SIGN && !manager.pitcher.bRelease)
        {
            var control = Object.FindFirstObjectByType<ControlPitchingUI>();
            if (control == null || !control._active.activeInHierarchy) return;
            control.setRelease();
            File.AppendAllText(Output + "/trace.txt", "TEST_INPUT existing pitch-release API at initialized cursor position\n");
            nextTestInput = Time.unscaledTime + 4;
        }
    }
}
#endif
