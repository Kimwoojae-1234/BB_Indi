#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BaseBall.BallPlay;
using BaseBall.BallPlay.UGUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Explicit integration commands. Uses the existing offline boot and restores its scene setup.
[InitializeOnLoad]
public static class CanvasIntegrationChecks
{
    public const string Root = "Docs/UIAudit/CanvasRestructure/Step6";
    public static string Output => SessionState.GetString(Key + ".Output", Root);
    private const string Key = "BB.CanvasIntegration";
    private static double nextSample;
    private static string previousState;
    private static int inputFrame;

    static CanvasIntegrationChecks()
    {
        EditorApplication.update += Sample;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Key, false)) return;
            SessionState.SetBool(Key, false);
            SessionState.EraseString("BB.Integrated.Output");
            Application.runInBackground = SessionState.GetBool(Key + ".Background", false);
            Log("STOP Play Mode ended; offline fixture restores the original scene setup.");
        };
    }

    public static void Command(string action)
    {
        if (action == "check-result") { CheckResultScoreboard(); return; }
        if (action == "start" || action.StartsWith("start:"))
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before starting integration checks.");
            string run = action == "start" ? "" : action.Substring(6);
            if (run.Any(c => !char.IsLetterOrDigit(c) && c != '-')) throw new ArgumentException("Simple run name required.");
            SessionState.SetString(Key + ".Output", run.StartsWith("NGUI") ? "Docs/UIAudit/NGUIComplete/Integration/" + run : run.Length == 0 ? Root : Root + "/" + run);
            Directory.CreateDirectory(Output);
            // Preserve previous sessions, including interrupted/failed runs.
            string trace = Output + "/integration-trace.txt";
            if (File.Exists(trace)) throw new InvalidOperationException("Archive the previous Step6 run before starting another one.");
            ScoreboardPlayChecks.StartOffline(Output);
            SessionState.SetBool(Key + ".Background", Application.runInBackground);
            SessionState.SetBool(Key, true);
            SessionState.SetString("BB.Integrated.Output", Output);
            Log("START Unity=" + Application.unityVersion + "; platform=" + EditorUserBuildSettings.activeBuildTarget + "; offline seed=230923; normal match outcomes, no score/inning injection.");
            return;
        }
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false)) throw new InvalidOperationException("Start the integration session first.");
        if (action == "status") { Snapshot("status"); return; }
        if (action.StartsWith("capture:")) { Snapshot(action.Substring(8)); return; }
        if (action == "assist") { IntegratedUGUIPlayChecks.EnableAssist(); Log("TEST_INPUT enabled existing pitch selection/release assistance."); return; }
        if (action == "assist-off") { SessionState.SetBool("BB.Integrated.Assist", false); return; }
        if (action == "auto" || action == "manual") { IntegratedUGUIPlayChecks.ChangeModeFixture(action == "auto"); Log("FIXTURE existing mode transition API: " + action); return; }
        if (action == "pause") { IngameUI.GetScoreBoard().setPause(); Log("FIXTURE scoreboard pause API; paused=" + Mode.bPauseGame); return; }
        if (action == "resume") { IngameUI.GetPauseUI().pressContinue(); Log("FIXTURE existing resume API."); return; }
        if (action == "change") { IngameUI.GetPauseUI().pressChange(); Log("FIXTURE open player change via pause API."); return; }
        if (action == "close-change") { IngameUI.GetPlayerChangeUI().deactive(); Log("FIXTURE close player change via existing API."); return; }
        if (action == "return-lobby")
        {
            var result = Object.FindFirstObjectByType<ResultUI>();
            if (result == null || Mode.bRttsMode) throw new InvalidOperationException("Local legacy result required.");
            result.OnClickBacktoLobby(); Log("FIXTURE existing legacy result lobby-return API; no RTTS round/reward submission."); return;
        }
        if (action == "quick-pause") { Quick().pauseGame(); Log("FIXTURE Quick pause API."); return; }
        if (action == "skip-regression") { CheckSkip(); return; }
        if (action == "speed8") { Time.timeScale = 8; Log("FIXTURE timeScale=8; game rules and outcomes unchanged."); return; }
        if (action == "speed1") { Time.timeScale = 1; return; }
        if (action.StartsWith("click:")) { IntegratedUGUIPlayChecks.ClickAction(action.Substring(6)); return; }
        if (action == "stop") { EditorApplication.isPlaying = false; return; }
        throw new ArgumentException("Unknown integration action " + action);
    }

    private static QuickSimulator Quick()
    {
        var quick = Object.FindFirstObjectByType<QuickSimulator>();
        if (quick == null || !Mode.bSimulationQuickPlay) throw new InvalidOperationException("Active Quick simulation required.");
        return quick;
    }

    private static void CheckSkip()
    {
        var quick = Quick();
        var method = typeof(QuickSimulator).GetMethod("setHoldFastForward", BindingFlags.Instance | BindingFlags.NonPublic);
        float before = Time.timeScale;
        int missingSlots = quick.skillUI.Count(s => s != null && s.skillStot == null);
        try
        {
            for (int i = 0; i < 3; i++)
            {
                method.Invoke(quick, new object[] { true, true });
                foreach (var skill in quick.skillUI.Where(s => s != null))
                {
                    if ((skill._active != null && skill._active.activeSelf) || (skill.backSpr != null && skill.backSpr.activeSelf) ||
                        (skill.catureTexture != null && (skill.catureTexture.sprite != null || skill.catureTexture.gameObject.activeSelf)))
                        throw new InvalidOperationException("Skill presentation remained visible after skip: " + skill.name);
                }
                method.Invoke(quick, new object[] { false, true });
                if (!Mathf.Approximately(Time.timeScale, before)) throw new InvalidOperationException("Fast-forward did not restore timeScale.");
            }
            Log("PASS repeated fast-forward enter/exit x3 on live QuickSimulator; missing skill slots=" + missingSlots + "; presentations hidden; timeScale restored=" + before + ". Test invoked production private method, not OS hold input.");
        }
        finally { method.Invoke(quick, new object[] { false, true }); }
    }

    private static void Sample()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false)) return;
        Application.runInBackground = true;
        EditorApplication.QueuePlayerLoopUpdate();
        if (inputFrame != Time.frameCount && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))
        {
            inputFrame = Time.frameCount;
            Log("DEVICE_INPUT down=" + Input.GetMouseButtonDown(0) + " up=" + Input.GetMouseButtonUp(0) + " position=" + Input.mousePosition);
        }
        if (EditorApplication.timeSinceStartup < nextSample) return;
        nextSample = EditorApplication.timeSinceStartup + .5;
        string state = State();
        if (state != previousState) { previousState = state; Log(state); }
    }

    private static string State()
    {
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        var quick = Object.FindFirstObjectByType<QuickSimulator>();
        var board = Object.FindFirstObjectByType<BaseBall.BallPlay.UIScoreBoard>(FindObjectsInactive.Include);
        string state = "STATE scene=" + SceneManager.GetActiveScene().name + " mode=" + Mode.PlayTypeFlag + " quick=" + Mode.bSimulationQuickPlay + " paused=" + Mode.bPauseGame + " size=" + Screen.width + "x" + Screen.height;
        if (manager != null) state += " inning=" + manager.nInningCount + " top=" + manager.bTopInning + " score=" + string.Join(",", manager.nGameScore) + " B/S/O=" + manager.nBallCount + "/" + manager.nStrikeCount + "/" + manager.nOutCount + " phase=" + manager.playState;
        if (board != null && board.UguiView != null) state += " boardVisible=" + board._active.activeInHierarchy + " display=" + board.UguiView.homeScore.text + ":" + board.UguiView.awayScore.text;
        if (quick != null) state += " quickState=" + quick.curState + " quickInning=" + quick.inningLabel.text;
        state += " eventSystems=" + Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Count(e => e.isActiveAndEnabled);
        return state;
    }

    private static void Snapshot(string label)
    {
        if (label.Any(c => !char.IsLetterOrDigit(c) && c != '-')) throw new ArgumentException("Simple capture label required.");
        var text = new StringBuilder(State() + "\n");
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        if (manager != null)
        {
            text.AppendLine("DATA batterCard=" + (manager.batter.pBatter.getCard() != null) + " pitcherCard=" + (manager.pitcher.pPitcher.getCard() != null));
            foreach (var controller in Object.FindObjectsByType<changeController2>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                text.AppendLine("CHANGE_BINDING " + controller.name + " overRoll=" + (controller.overRoll != null) + " nameLabel=" + (controller.nameLabel != null));
        }
        var result = Object.FindFirstObjectByType<ResultUI>();
        if (result != null && result.resultMain.board != null)
        {
            foreach (var row in result.resultMain.board.teamObj)
                text.AppendLine("RESULT_ROW " + row.transform.Find("teamLabel").GetComponent<GameUIElement>().text + " scores=" + string.Join(",", row.transform.Find("score").GetComponentsInChildren<GameUIElement>().Select(l => l.text)) + " stats=" + string.Join(",", row.transform.Find("stat").GetComponentsInChildren<GameUIElement>().Select(l => l.text)));
        }
        foreach (var root in Object.FindObjectsByType<GameUIRoot>(FindObjectsSortMode.None))
            text.AppendLine("ROOT " + root.name + " camera=" + root.uiCamera + " canvases=" + root.GetComponentsInChildren<Canvas>().Length);
        foreach (var pointer in Object.FindObjectsByType<GameUIPointer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var actions = pointer.onPress.Concat(pointer.onRelease).Concat(pointer.onClick).Where(a => a != null).Select(a => a.mMethodName);
            text.AppendLine("INPUT " + AnimationUtility.CalculateTransformPath(pointer.transform, null) + " active=" + pointer.gameObject.activeInHierarchy + " available=" + pointer.Available + " methods=" + string.Join(",", actions));
        }
        var ngui = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(c => c != null && c.enabled && AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(c)).StartsWith("Assets/NGUI/")).ToArray();
        text.AppendLine("ACTIVE_NGUI " + string.Join(",", ngui.Select(c => c.name + ":" + c.GetType().Name)));
        foreach (var dynamicUI in Object.FindObjectsByType<GameUIDynamicUI>(FindObjectsSortMode.None))
            text.AppendLine("DYNAMIC " + AnimationUtility.CalculateTransformPath(dynamicUI.transform, null));
        File.WriteAllText(Output + "/" + label + ".txt", text.ToString());
        ScreenCapture.CaptureScreenshot(Output + "/" + label + ".png");
        Log("CAPTURE " + label);
    }

    private static void Log(string message)
    {
        File.AppendAllText(Output + "/integration-trace.txt", DateTime.UtcNow.ToString("O") + " gameTime=" + Time.time.ToString("F1") + " " + message + "\n");
    }

    private static void CheckResultScoreboard()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required.");
        var prefab = PrefabUtility.LoadPrefabContents("Assets/Resources/MainGame/prefabs/resultUI/resultPrefab.prefab");
        try
        {
            var result = prefab.GetComponent<ResultUI>();
            var board = result.resultMain.board;
            if (board == null || prefab.GetComponentInChildren<scoreboard>(true) != null) throw new InvalidOperationException("Result scoreboard controller binding is missing or incorrect.");
            board.initScoreBoard("Away", "Home", 1, 2);
            var away = Enumerable.Repeat(SimulParm.NOPLAY_INNING, 12).ToArray();
            var home = (int[])away.Clone();
            away[0] = 0; away[1] = 9; away[2] = SimulParm.GAMEEND_INNING;
            home[0] = 2; home[1] = -3;
            board.setResult(away, home, new[] { 9, 10, 1 }, new[] { 5, 8, 0 }, 1);
            string[] expected = { "0,9,X", "2,3X" };
            for (int team = 0; team < 2; team++)
            {
                var labels = board.teamObj[team].transform.Find("score").GetComponentsInChildren<GameUIElement>(true);
                if (labels.Length != 12 || string.Join(",", labels.Where(l => l.gameObject.activeSelf).Select(l => l.text)) != expected[team])
                    throw new InvalidOperationException("Incorrect result inning visibility/text.");
                if (board.teamObj[team].transform.Find("indicator").gameObject.activeSelf != (team == 1)) throw new InvalidOperationException("Incorrect team indicator.");
                var stat = board.teamObj[team].transform.Find("stat").GetComponentsInChildren<GameUIElement>(true);
                if (string.Join(",", stat.Select(l => l.text)) != (team == 0 ? "9,10,1" : "5,8,0")) throw new InvalidOperationException("Incorrect result totals.");
            }
            if (board.cur.activeSelf) throw new InvalidOperationException("Result retains active inning marker.");
            Directory.CreateDirectory(RemainingUGUIMigration.Output);
            File.WriteAllText(RemainingUGUIMigration.Output + "/result-scoreboard-checks.txt", "PASS actual UGUI result prefab controller binding; both 12-inning rows; zero, numeric, X and walkoff 3X; unplayed innings hidden; R/H/E; own-team indicator; current inning marker hidden. Prefab contents unloaded without saving.\n");
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
    }
}
#endif
