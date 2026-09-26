#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using BaseBall.BallPlay;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class FieldMinimapPlayChecks
{
    private const string Key = "BB.FieldMinimapPlayCheck";
    private static string Output => FieldMinimapUGUIConverter.Output + (SessionState.GetBool(Key + ".UGUI", false) ? "/UGUI" : "/Legacy");
    static FieldMinimapPlayChecks()
    {
        EditorApplication.update += Sample;
        EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.EnteredEditMode) SessionState.SetBool(Key, false); };
    }

    [MenuItem("Tools/UI Migration/Start Field Minimap Gameplay Check")]
    public static void Start()
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath).GetComponentInChildren<UIFieldUI>(true);
        SessionState.SetBool(Key + ".UGUI", source.UguiMinimap != null);
        ScoreboardPlayChecks.StartOffline(Output);
        SessionState.SetBool(Key, true);
        SessionState.SetBool(Key + ".Started", false);
        File.WriteAllText(Output + "/interaction-checks.txt", "WAITING for offline match. Controlled field and runner state; no organic hit or complete match verification.\n");
    }

    private static void Sample()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false) || SessionState.GetBool(Key + ".Started", false)) return;
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        if (manager == null || !manager.bMyTurn) return;
        // Field.initFielder waits for EndOfFrame before loading the batting-view
        // mesh; batch mode has no Game View frame. Its field and player data are
        // already initialized here, so the isolated display fixture can run.
        bool batchReady = Application.isBatchMode && manager.playState == PlayState.NONE && manager.nInningCount == 1 &&
            !manager.bBatterForceLoad && manager.batter != null && manager.batter.pBatter != null &&
            manager.pitcher != null && manager.pitcher.pPitcher != null;
        if (manager.playState != PlayState.PLAY_BATTING_VIEW && !batchReady) return;
        SessionState.SetBool(Key + ".Started", true);
        File.AppendAllText(Output + "/interaction-checks.txt", batchReady
            ? "ENTRY batch fixture after field/player initialization; batting mesh EndOfFrame wait is not completed.\n"
            : "ENTRY normal batting-view initialization completed.\n");
        manager.StopAllCoroutines();
        manager.StartCoroutine(new FieldMinimapPlayProbe().Start(manager, Output));
    }
}

public sealed class FieldMinimapPlayProbe
{
    private BallPlayManager manager;
    private UIFieldUI field;
    private string output;
    private IEnumerator routine;
    private readonly Vector3[] bases = { new Vector3(84,0), new Vector3(0,84), new Vector3(-84,0), new Vector3(0,-84) };

    public IEnumerator Start(BallPlayManager match, string folder)
    {
        manager = match; field = IngameUI.GetFieldUI(); output = folder; routine = Check();
        return Guarded();
    }

    private IEnumerator Guarded()
    {
        while (true)
        {
            object next;
            try { if (!routine.MoveNext()) yield break; next = routine.Current; }
            catch (Exception exception)
            {
                File.AppendAllText(output + "/interaction-checks.txt", "FAIL " + exception + "\n");
                Debug.LogException(exception); yield break;
            }
            yield return next;
        }
    }

    private void Require(bool condition, string label)
    {
        FieldMinimapMigrationChecks.Require(condition, label);
        File.AppendAllText(output + "/interaction-checks.txt", "PASS " + label + "\n");
    }

    private void CheckPosition(miniRunner marker, Vector3 expected)
    {
        FieldMinimapMigrationChecks.Require(Vector3.Distance(marker.transform.localPosition, expected) < .01f, "Original minimap position");
        if (marker.UguiView == null) return;
        marker.UguiView.SyncPosition();
        FieldMinimapMigrationChecks.Require(Vector3.Distance(marker.UguiView.team.transform.position, marker.transform.position) < .01f &&
            Vector3.Distance(marker.UguiView.playerName.transform.position, marker.transform.TransformPoint(new Vector3(0,30))) < .01f,
            "UGUI visuals follow actual runner marker");
    }

    private IEnumerator Check()
    {
        manager.bUpdate = false;
        manager.pitcher.StopAllCoroutines();
        Object.FindFirstObjectByType<ControlBattingUI>().StopAllCoroutines();
        manager.field.StopAllCoroutines();
        manager.field.enabled = false;
        var runners = new Runner[3];
        for (int i = 0; i < 3; i++)
        {
            runners[i] = manager.field.run.getRunner(i) ?? manager.field.run.makeChanceRunner(manager.batter.pBatter, i);
            runners[i].enabled = false;
        }
        foreach (var existing in Object.FindObjectsByType<Runner>(FindObjectsSortMode.None)) existing.enabled = false;
        CameraManager.ChangeCamera(BallPlayManager._FIELDVIEW,
            BallPlayManager.FIELDVIEW_CAMERA_INITX + FieldSize.getMoundPosX(), BallPlayManager.FIELDVIEW_CAMERA_INITY + 843);
        manager.playState = PlayState.PLAY_FIELDING_VIEW;
        yield return null;
        var markers = runners.Select(r => r.minimapRunner.GetComponent<miniRunner>()).ToArray();
        Require(markers.All(m => m != null && m.transform.parent == field.minimap.transform), "Chance-runner factory creates and binds three real minimap markers");
        Require(field._active.activeInHierarchy, "Field camera enables minimap and out count");
        var marker = markers[0]; var runner = runners[0];
        // Manual sampling below isolates the UI transform from actual baseball outcomes.
        int initialMarkerCount = field.minimap.transform.childCount;
        foreach (var m in field.minimap.GetComponentsInChildren<miniRunner>(true)) m.enabled = false;
        for (int i = 0; i < 4; i++)
        {
            runner.currentPos = i;
            marker.set(manager, runner, 1);
            CheckPosition(marker, bases[i]);
            runner.state = RunState.STANDBY;
            marker.move(); // Reset initPosition's cache as in the existing implementation.
            marker.initPosition();
            CheckPosition(marker, bases[i]);
        }
        Require(true, "All four initial base positions and initPosition mappings preserved");
        float[] x = { FieldSize.getFirstBasePosX(), FieldSize.getSecondBasePosX(), FieldSize.getThirdBasePosX(), FieldSize.getHomePosX() };
        int samples = 0;
        for (int destination = 0; destination < 4; destination++)
        foreach (var state in new[] { RunState.MOVE, RunState.STEAL })
        for (int step = 0; step <= 8; step++)
        {
            float fraction = step / 8f;
            int previous = (destination + 3) % 4;
            runner.destPos = destination; runner.state = state;
            runner.posX = Mathf.Lerp(x[previous], x[destination], fraction);
            marker.move();
            CheckPosition(marker, Vector3.Lerp(bases[previous], bases[destination], fraction));
            samples++;
        }
        Require(true, samples + " movement samples across four segments and MOVE/STEAL states preserve interpolation");
        foreach (var state in new[] { RunState.GO_BENCH, RunState.PICKOFF })
        {
            var previous = marker.transform.localPosition;
            runner.state = state; runner.posX += 50;
            marker.move(); CheckPosition(marker, previous);
        }
        Require(true, "States outside STANDBY through STEAL do not move the marker");
        for (int count = 0; count <= 3; count++)
        {
            field.SetCountUpdate(count);
            Require(field.outCount[0].activeSelf == (count >= 1) && field.outCount[1].activeSelf == (count >= 2), "Out count " + count + " retains original visibility");
        }
        field.SetActive(false); yield return null;
        Require(!field.minimap.activeInHierarchy && !field.outCount[0].activeInHierarchy, "Hiding field UI hides map, markers and count");
        field.SetActive(true); yield return null;
        Require(field.minimap.activeInHierarchy && field.outCount[0].activeInHierarchy, "Showing field UI restores map and count");
        if (field.UguiMinimap != null)
        {
            var view = field.UguiMinimap;
            Require(markers.All(m => m.UguiView.team.gameObject.activeInHierarchy && m.UguiView.playerName.gameObject.activeInHierarchy), "Parent reactivation restores detached marker visuals");
            marker.gameObject.SetActive(false); yield return null;
            Require(!marker.UguiView.team.gameObject.activeSelf && !marker.UguiView.namebar.gameObject.activeSelf && !marker.UguiView.playerName.gameObject.activeSelf,
                "Disabling one marker hides its three detached graphics");
            marker.gameObject.SetActive(true); yield return null;
            Require(marker.UguiView.team.gameObject.activeInHierarchy && marker.UguiView.playerName.gameObject.activeInHierarchy, "Individual marker reactivation restores graphics");
            Require(view.GetComponentsInChildren<UIWidget>(true).Length == 0 && view.GetComponentsInChildren<GraphicRaycaster>(true).Length == 0 &&
                view.GetComponentsInChildren<Graphic>(true).All(g => !g.raycastTarget), "Migrated minimap is UGUI and cannot intercept gameplay input");
        }
        for (int i = 0; i < 3; i++) { runners[i].currentPos = i; markers[i].set(manager, runners[i], i + 1); markers[i].UguiView?.SyncPosition(); }
        field.SetCountUpdate(1);
        yield return null;
        if (!Application.isBatchMode) { ScreenCapture.CaptureScreenshot(output + "/field.png"); yield return null; }

        // The production Runner.destroyRunner API delegates its fade here. Hold the
        // runner alive first to measure alpha independently of its separate destruction.
        marker.FadeOut(.3f);
        yield return new WaitForSecondsRealtime(.15f);
        float alpha = marker.UguiView != null ? marker.UguiView.team.color.a : marker._team.alpha;
        Require(alpha < 1 && alpha > 0, "Marker fades before removal");
        if (marker.UguiView != null)
            Require(Mathf.Abs(marker.UguiView.namebar.color.a - alpha) < .01f && Mathf.Abs(marker.UguiView.playerName.color.a - alpha) < .01f && marker.GetComponent<UITweener>() == null,
                "All detached graphics fade without adding NGUI tween components");
        runner.destroyRunner();
        yield return new WaitForSeconds(.4f);
        yield return null;
        Require(marker == null && markers[1] != null && markers[2] != null, "Actual Runner.destroyRunner removes only its marker");
        if (field.UguiMinimap != null)
            Require(field.UguiMinimap.teams.childCount == initialMarkerCount - 1 && field.UguiMinimap.namebars.childCount == initialMarkerCount - 1 && field.UguiMinimap.names.childCount == initialMarkerCount - 1,
                "Runner destruction removes all detached visuals");

        var oldMode = Mode.gameMode;
        try
        {
            Mode.gameMode = Mode.GamePlayMode.NineInningTwoOut;
            field.MakeMinimapRunner(manager, runners[1], 2);
        }
        finally { Mode.gameMode = oldMode; }
        var special = runners[1].minimapRunner.GetComponent<miniRunner>();
        special.enabled = false;
        bool hidden = special.UguiView != null
            ? !special.UguiView.namebar.gameObject.activeSelf && !special.UguiView.playerName.gameObject.activeSelf && special.UguiView.team.gameObject.activeSelf
            : special.transform.Cast<Transform>().All(t => !t.gameObject.activeSelf) && special._team.gameObject.activeSelf;
        Require(hidden, "NineInningTwoOut hides names while retaining team icon (display branch only)");
        field.DestroyHitterRunner(); yield return null; yield return null;
        Require(special == null && markers[1] != null && markers[2] != null, "DestroyHitterRunner removes the most recently created marker only");

        field.MakeMinimapRunner(manager, runners[2], 3);
        var orphan = runners[2].minimapRunner.GetComponent<miniRunner>();
        orphan.runner = null; orphan.move(); yield return null; yield return null;
        Require(orphan == null, "Marker with a missing runner cleans itself up");
        field.DestroyAllMinimapRunner(); yield return null; yield return null;
        Require(field.minimap.transform.childCount == 0, "DestroyAllMinimapRunner clears every logical marker");
        if (field.UguiMinimap != null)
        {
            var view = field.UguiMinimap;
            Require(view.teams.childCount == 0 && view.namebars.childCount == 0 && view.names.childCount == 0 && view.GetComponent<Image>() != null && field.outCount.All(o => o != null),
                "Clearing runners removes detached graphics and preserves background, layers and count");
        }
        field.MakeMinimapRunner(manager, runners[2], 3);
        yield return null;
        Require(field.minimap.transform.childCount == 1 && runners[2].minimapRunner != null, "New marker can be created after clearing the minimap");
        File.AppendAllText(output + "/interaction-checks.txt", "COMPLETE — controlled Play Mode fixture. Native screen rendering, organic play outcomes, PVP and complete-match flow require separate verification.\n");
    }
}
#endif
