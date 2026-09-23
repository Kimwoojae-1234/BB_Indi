#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class RunnerControlPlayChecks
{
    private const string Key = "BB.RunnerControlPlayCheck";
    private static string Output => RunnerControlUGUIConverter.Output + (SessionState.GetBool(Key + ".UGUI",false) ? "/UGUI" : "/Legacy");
    static RunnerControlPlayChecks()
    {
        EditorApplication.update += Sample;
        EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.EnteredEditMode) SessionState.SetBool(Key,false); };
    }
    [MenuItem("Tools/UI Migration/Start Runner Control Gameplay Check")]
    public static void Start()
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath).GetComponentInChildren<ControlRunner>(true);
        SessionState.SetBool(Key + ".UGUI",source.UguiView != null);
        ScoreboardPlayChecks.StartOffline(Output);
        SessionState.SetBool(Key,true); SessionState.SetBool(Key + ".Started",false);
        File.WriteAllText(Output + "/interaction-checks.txt", "WAITING for offline match initialization. Controlled runner setup and Unity event dispatch; not native touch or an organic hit/steal outcome.\n");
    }
    private static void Sample()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key,false) || SessionState.GetBool(Key + ".Started",false)) return;
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        if (manager == null || !manager.bMyTurn || manager.playState != PlayState.PLAY_BATTING_VIEW) return;
        SessionState.SetBool(Key + ".Started",true);
        manager.StartCoroutine(new RunnerControlPlayProbe().Start(manager,Output));
    }
}

public sealed class RunnerControlPlayProbe
{
    private string output;
    private IEnumerator routine;
    private ControlRunner control;
    private BallPlayManager manager;
    private Runner[] runners;
    public IEnumerator Start(BallPlayManager match, string folder)
    {
        output = folder; manager = match; control = IngameUI.GetControlRunner(); routine = Check();
        return Guarded();
    }
    private IEnumerator Guarded()
    {
        while (true)
        {
            object next;
            try { if (!routine.MoveNext()) yield break; next = routine.Current; }
            catch (Exception exception) { File.AppendAllText(output + "/interaction-checks.txt", "FAIL " + exception + "\n"); Debug.LogException(exception); yield break; }
            yield return next;
        }
    }
    private void Require(bool condition, string label)
    {
        RunnerControlMigrationChecks.Require(condition,label);
        File.AppendAllText(output + "/interaction-checks.txt", "PASS " + label + "\n");
    }
    private void Setup(int mask, bool attack)
    {
        manager.bMyTurn = attack;
        manager.playState = attack ? PlayState.PLAY_BATTING_VIEW : PlayState.PLAY_BATTING_VIEW_READY;
        for (int i = 0; i < 3; i++)
        {
            manager.field.run.bOnBase[i] = (mask & (1 << i)) != 0;
            runners[i].bStealFlag = runners[i].bPickOffFlag = false;
        }
        control.bUpdateNeed = true; control.bActiveAvailble = true;
        control.Init(manager); control.UpdateState(); control.SetActive(true,false);
    }
    private void Press(int index)
    {
        var go = control.baseObj[index];
        if (control.UguiView == null) go.SendMessage("OnPress",true);
        else
        {
            var data = new PointerEventData(EventSystem.current) { pointerId = -1, button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(go,data,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(go,data,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(go,data,ExecuteEvents.pointerClickHandler);
        }
    }
    private IEnumerator Check()
    {
        // Freeze match advancement, then use the game's chance-runner factory.
        // All data lives in this disposable Play Mode session.
        manager.bUpdate = false;
        manager.pitcher.StopAllCoroutines();
        // The delayed AI pitch is also hosted by the batting control, not only
        // the pitcher. Stop it so it cannot legitimately hide UI during this fixture.
        Object.FindFirstObjectByType<ControlBattingUI>().StopAllCoroutines();
        runners = new Runner[3];
        for (int i = 0; i < 3; i++) runners[i] = manager.field.run.getRunner(i) ?? manager.field.run.makeChanceRunner(manager.batter.pBatter,i);
        Require(runners.All(r => r != null && r.pRunner != null), "Real runner objects initialized through chance-runner factory");
        yield return null;
        Setup(7,true);
        yield return null;
        ScreenCapture.CaptureScreenshot(output + "/attack.png");
        if (control.UguiView != null)
        {
            var canvas = control.UguiView.canvas;
            Require(EventSystem.current != null && EventSystem.current.currentInputModule != null, "Active UGUI input module");
            for (int i = 0; i < 3; i++)
            {
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = canvas.displayCanvas.worldCamera.WorldToScreenPoint(control.baseObj[i].transform.position) },hits);
                Require(hits.Count > 0 && hits[0].gameObject == control.baseObj[i], "Actual match raycast reaches base " + i);
            }
            int presses = 0;
            UnityEngine.Events.UnityAction countPress = () => presses++;
            var button = control.UguiView.bases[0];
            button.onClick.AddListener(countPress);
            try
            {
                var first = new PointerEventData(EventSystem.current) { pointerId = 10, button = PointerEventData.InputButton.Left };
                var second = new PointerEventData(EventSystem.current) { pointerId = 11, button = PointerEventData.InputButton.Left };
                button.OnPointerDown(first);
                button.OnPointerDown(second);
                button.OnPointerUp(second);
                button.OnPointerClick(second);
                Require(presses == 1, "Second pointer cannot duplicate a held base press");
                button.OnPointerUp(first);
                button.OnPointerClick(first);
                Require(presses == 1, "Release and click do not duplicate press command");
                button.OnPointerDown(first);
                button.gameObject.SetActive(false);
                button.gameObject.SetActive(true);
                button.OnPointerDown(second);
                button.OnPointerUp(second);
                Require(presses == 3, "Disabling a base clears its held pointer");
            }
            finally { button.onClick.RemoveListener(countPress); }
            Setup(7,true); Press(0);
            float low = 1, high = 0, until = Time.unscaledTime + .7f;
            float oldScale = Time.timeScale;
            Time.timeScale = 0;
            try
            {
                while (Time.unscaledTime < until)
                {
                    yield return null;
                    float alpha = control.UguiView.light[0].color.a;
                    low = Mathf.Min(low, alpha); high = Mathf.Max(high, alpha);
                }
            }
            finally { Time.timeScale = oldScale; }
            Require(low < .25f && high > .75f, "Selected light pulses while scaled game time is stopped");
        }
        for (int mask = 1; mask < 8; mask++)
        for (int selected = 0; selected < 3; selected++)
        {
            if ((mask & (1 << selected)) == 0) continue;
            Setup(mask,true); Press(selected);
            int expected = 1 << selected;
            for (int next = selected + 1; next < 3 && (mask & (1 << next)) != 0; next++) expected |= 1 << next;
            int actual = 0;
            for (int i = 0; i < 3; i++) if (runners[i].bStealFlag) actual |= 1 << i;
            Require(actual == expected, "Steal chain mask=" + mask + " selected=" + selected + " result=" + actual);
        }
        Setup(0,true); Require(!control._active.activeSelf, "No runners hides control");
        Setup(7,true);
        manager.playState = PlayState.PLAY_BATTING_VIEW_READY;
        Press(0); Require(runners.All(r => !r.bStealFlag), "Wrong attack phase ignores input");
        Setup(7,true); control.bActiveAvailble = false; control.SetActive(true,false);
        Require(!control._active.activeSelf, "Unavailable control stays hidden");
        Setup(7,true);
        control.SetActive(false,true);
        Press(0); Require(runners.All(r => !r.bStealFlag), "Fading control rejects commands immediately");
        for (int i = 0; i < 72; i++) yield return new WaitForEndOfFrame();
        Require(!control._active.activeSelf, "Frame-based fade ends with hidden UI");
        Setup(7,true);
        yield return null;
        if (control.UguiView != null)
        {
            control.SetActive(false,true);
            for (int i = 0; i < 8; i++) yield return new WaitForEndOfFrame();
            control.SetActive(true,false);
            for (int i = 0; i < 72; i++) yield return new WaitForEndOfFrame();
            File.AppendAllText(output + "/interaction-checks.txt", "Reactivation state: active=" + control._active.activeSelf +
                " available=" + control.bActiveAvailble + " bases=" + string.Join(",", manager.field.run.bOnBase) +
                " panel=" + control.GetComponent<UIPanel>().alpha + " canvas=" + control.UguiView.canvas.opacity.alpha + "\n");
            Require(control._active.activeSelf && control.UguiView.canvas.opacity.alpha == 1, "Reactivation cancels stale fade");
            Mode.bPauseGame = true;
            yield return null;
            yield return null;
            Require(!control.UguiView.canvas.opacity.blocksRaycasts, "Pause blocks UGUI input");
            Mode.bPauseGame = false;
            yield return null;
            var pitch = IngameUI.GetPitchingSelect();
            pitch.SetActive(true);
            yield return null;
            Require(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "Runner and pitch selection share one EventSystem");
            pitch.SetActive(false);
            yield return null;
            Require(EventSystem.current != null && control.UguiView.canvas.opacity.blocksRaycasts, "Hiding pitch selection keeps runner input alive");
            control.SetActive(false,false);
            pitch.SetActive(true);
            yield return null;
            Require(EventSystem.current != null && Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "Hiding runner control keeps pitch input alive");
            pitch.SetActive(false);
        }
        Setup(7,false);
        yield return null;
        ScreenCapture.CaptureScreenshot(output + "/defense.png");
        // PVP433's no-pickoff guard is checked without entering a network session.
        bool oldPvp = Mode.bPvpMode433;
        Mode.bPvpMode433 = true;
        Press(1);
        Mode.bPvpMode433 = oldPvp;
        Require(runners.All(r => !r.bPickOffFlag), "PVP433 pickoff guard unchanged (no network session)");
        Press(1);
        Require(runners[1].bPickOffFlag && manager.field.run.bPickOff && manager.field.nTargetIndex == 1 && manager.playState == PlayState.PLAY_FIELDING_VIEW,
            "Second-base pickoff reaches existing field controller and PLAY_FIELDING_VIEW");
        File.AppendAllText(output + "/interaction-checks.txt", "COMPLETE — controlled Play Mode fixture; native touch, network play and complete steal/pickoff outcomes remain unverified.\n");
    }
}
#endif
