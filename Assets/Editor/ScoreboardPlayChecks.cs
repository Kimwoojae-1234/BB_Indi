#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using BaseBall.BallPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MatchScoreboard = BaseBall.BallPlay.UIScoreBoard;

/// <summary>Uses the project's existing _Test_Local entry point. No saved-game or scene writes.</summary>
[InitializeOnLoad]
public static class ScoreboardPlayChecks
{
    private const string SessionKey = "BB.ScoreboardPlayChecks.SceneSetup";
    [Serializable] private class SavedSetup { public SceneSetup[] scenes; }
    private static double nextSample;
    private static string previousState;
    private static string TraceOutput => SessionState.GetString(SessionKey + ".Output", ScoreboardMigrationChecks.OutputPath);

    static ScoreboardPlayChecks()
    {
        EditorApplication.playModeStateChanged += OnPlayMode;
        EditorApplication.update += Sample;
        Application.logMessageReceived += OnLog;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BallPlay" || string.IsNullOrEmpty(SessionState.GetString(SessionKey, ""))) return;
        // _Test_Local defaults to quick simulation, which never shows this HUD.
        // Select the supported manual mode before the match's Start methods run.
        Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
        Mode.bSimulationQuickPlay = false;
        Mode.bOnlyChanceMode = false;
    }

    [MenuItem("Tools/UI Migration/Start Offline Gameplay Check")]
    public static void StartOffline() { StartOffline(ScoreboardMigrationChecks.OutputPath); }

    public static void StartOffline(string output)
    {
#if _Test_Local
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save or discard open scene changes before running the fixture.");
        Directory.CreateDirectory(output);
        SessionState.SetString(SessionKey + ".Output", output);
        SessionState.SetString(SessionKey, JsonUtility.ToJson(new SavedSetup { scenes = EditorSceneManager.GetSceneManagerSetup() }));
        File.WriteAllText(TraceOutput + "/gameplay-trace.txt", "Offline _Test_Local entry, " + DateTime.UtcNow.ToString("O") + "\n");
        SessionState.SetBool(SessionKey + ".Bootstrap", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Logo.unity");
        EditorApplication.isPlaying = true;
#else
        throw new InvalidOperationException("This check requires the project's _Test_Local scripting symbol.");
#endif
    }

    private static void OnPlayMode(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;
        string saved = SessionState.GetString(SessionKey, "");
        if (string.IsNullOrEmpty(saved)) return;
        SessionState.EraseString(SessionKey);
        EditorSceneManager.RestoreSceneManagerSetup(JsonUtility.FromJson<SavedSetup>(saved).scenes);
    }

    private static void OnLog(string condition, string stack, LogType type)
    {
        if (!EditorApplication.isPlaying || string.IsNullOrEmpty(SessionState.GetString(SessionKey, ""))) return;
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        File.AppendAllText(TraceOutput + "/gameplay-trace.txt", "ERROR " + condition + "\n" + stack + "\n");
    }

    private static void Sample()
    {
        if (!EditorApplication.isPlaying || string.IsNullOrEmpty(SessionState.GetString(SessionKey, "")) || EditorApplication.timeSinceStartup < nextSample) return;
        nextSample = EditorApplication.timeSinceStartup + 1;
        // The legacy match expects persistent managers created by the normal boot.
        // Opening MainLoading directly produces a pre-existing MusicManager null error.
        if (SessionState.GetBool(SessionKey + ".Bootstrap", false) &&
            SceneManager.GetActiveScene().name == "MainLobby" && Time.time > 5)
        {
            SessionState.SetBool(SessionKey + ".Bootstrap", false);
            Mode.EndRttsGame();
            tempSelectPage.ConfigureDongneYagu();
            UnityEngine.Random.InitState(230923);
            SceneManager.LoadScene("MainLoading");
            return;
        }
        var manager = UnityEngine.Object.FindFirstObjectByType<BallPlayManager>();
        var board = UnityEngine.Object.FindFirstObjectByType<MatchScoreboard>(FindObjectsInactive.Include);
        string state = "Scene=" + SceneManager.GetActiveScene().name + " manager=" + (manager != null) + " board=" + (board != null);
        if (manager != null && board != null)
        {
            state += " ugui=" + (board.UguiView != null) + " visible=" + board._active.activeInHierarchy +
                " phase=" + manager.playState + " update=" + manager.bUpdate +
                " inning=" + manager.nInningCount + " top=" + manager.bTopInning + " home=" + manager.bMyHome +
                " score=" + string.Join(",", manager.nGameScore) +
                " count=" + manager.nBallCount + "/" + manager.nStrikeCount + "/" + manager.nOutCount;
            if (board.UguiView != null)
            {
                var view = board.UguiView;
                state += " display=" + view.homeScore.text + ":" + view.awayScore.text + " alpha=" + view.opacity.alpha;
            }
        }
        if (state == previousState) return;
        previousState = state;
        File.AppendAllText(TraceOutput + "/gameplay-trace.txt", Time.time.ToString("F1") + " " + state + "\n");
    }

    [MenuItem("Tools/UI Migration/Capture Gameplay Frame")]
    public static void CaptureFrame()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Gameplay capture requires Play Mode.");
        var board = UnityEngine.Object.FindFirstObjectByType<MatchScoreboard>(FindObjectsInactive.Include);
        ScreenCapture.CaptureScreenshot(ScoreboardMigrationChecks.OutputPath + (board != null && board.UguiView != null ? "/gameplay-ugui.png" : "/gameplay-legacy.png"));
    }
}
#endif
