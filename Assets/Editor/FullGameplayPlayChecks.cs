#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Opt-in local gameplay input assistance. Never changes players, scores, innings,
// contact outcomes, or saved data; the ordinary match resolves every action.
[InitializeOnLoad]
public static class FullGameplayPlayChecks
{
    const string Key="BB.FullGameplay.Assist";
    static float releaseStart=-1, nextPitch;
    static bool accelerate;
    static int frame=-1, swings, selections, releases, hudSamples;
    static readonly HashSet<string> captured=new HashSet<string>();
    static readonly HashSet<string> mismatches=new HashSet<string>();
    static string pendingMismatch="";
    static float mismatchStart;
    static string Output=>CanvasIntegrationChecks.Output;
    static FullGameplayPlayChecks()
    {
        EditorApplication.update+=Tick;
        EditorApplication.playModeStateChanged+=s=>
        {
            if(s==PlayModeStateChange.ExitingPlayMode && SessionState.GetBool(Key,false)) Save();
            if(s==PlayModeStateChange.EnteredEditMode) SessionState.SetBool(Key,false);
        };
    }
    public static void Command(string action)
    {
        if(!EditorApplication.isPlaying || Mode.bRttsMode || Mode.bPvpMode) throw new InvalidOperationException("Local non-RTTS gameplay required.");
        if(action=="on")
        {
            accelerate=false;
            releaseStart=-1; nextPitch=0; swings=selections=releases=hudSamples=0;
            captured.Clear(); mismatches.Clear(); pendingMismatch="";
            SessionState.SetBool(Key,true); Log("START input assistance; normal game rules and outcomes unchanged.");
        }
        else if(action=="off") { Save(); accelerate=false; SessionState.SetBool(Key,false); }
        else if(action=="summary") Save();
        else if(action=="speed8") { accelerate=true; Log("FIXTURE accelerate manual batting phases to timeScale=8; gameplay outcomes unchanged."); }
        else if(action=="speed1") { accelerate=false; Time.timeScale=1; Log("FIXTURE restore timeScale=1."); }
        else if(action.StartsWith("runner:"))
        {
            int index=int.Parse(action.Substring(7));
            var view=Object.FindFirstObjectByType<RunnerControlView>();
            if(view==null || index<0 || index>=view.bases.Length || !Click(view.bases[index])) throw new InvalidOperationException("Runner button unavailable or occluded.");
            Log("RUNNER native raycast and pointer events index="+index+" selected="+view.light[index].gameObject.activeSelf);
        }
        else throw new ArgumentException(action);
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying || !SessionState.GetBool(Key,false) || frame==Time.frameCount) return;
        frame=Time.frameCount;
        var manager=Object.FindFirstObjectByType<BallPlayManager>();
        if(manager==null || Mode.bPauseGame || Mode.bSimulationQuickPlay || Mode.PlayTypeFlag!=Mode.ModeFlag.Manual) return;
        if(accelerate && manager.playState.ToString().StartsWith("PLAY_BATTING_VIEW")) Time.timeScale=8;
        var board=IngameUI.GetScoreBoard();
        if(board!=null && board.UguiView!=null && board._active.activeInHierarchy && manager.playState==PlayState.PLAY_BATTING_VIEW)
        {
            var view=board.UguiView; int home=manager.bMyHome?0:1;
            string mismatch=view.homeScore.text!=manager.nGameScore[home].ToString() || view.awayScore.text!=manager.nGameScore[1-home].ToString()
                ? "HUD score "+view.homeScore.text+":"+view.awayScore.text+" expected "+manager.nGameScore[home]+":"+manager.nGameScore[1-home] : "";
            if(mismatch!=pendingMismatch) { pendingMismatch=mismatch; mismatchStart=Time.unscaledTime; }
            if(mismatch.Length>0 && Time.unscaledTime-mismatchStart>1 && mismatches.Add(mismatch)) Log("FAIL "+mismatch);
            hudSamples++;
            string label="manual-"+manager.nInningCount+"-"+(manager.bTopInning?"top":"bottom");
            if(captured.Add(label)) { ScreenCapture.CaptureScreenshot(Output+"/"+label+".png"); Log("CAPTURE "+label); Save(); }
        }
        if(manager.bMyTurn)
        {
            if(manager.pitcher.pState!=PitcherState._RELEASE) { releaseStart=-1; return; }
            if(releaseStart<0) releaseStart=Time.time;
            if(Time.time-releaseStart<Mathf.Max(.08f,manager.batter.batterPerfectTime*.72f) || manager.batter.bSwing) return;
            var control=Object.FindFirstObjectByType<ControlBattingUI>();
            if(control==null || !control._active.activeInHierarchy) return;
            control.swing();
            if(manager.batter.bSwing) { swings++; Log("SWING production ControlBattingUI.swing callback; accepted=True"); }
        }
        else if(Time.unscaledTime>=nextPitch)
        {
            var selector=IngameUI.GetPitchingSelect();
            // The selector retains the previous pitch enum between pitches.
            // Active, interactable rows determine whether another selection is ready.
            if(selector._active.activeInHierarchy && selector.button.Any(b=>b.gameObject.activeInHierarchy && b.UguiView!=null && b.UguiView.interactable))
            {
                var row=selector.button.FirstOrDefault(b=>b.gameObject.activeInHierarchy && b.UguiView!=null && b.UguiView.interactable);
                if(row!=null && Click(row.UguiView)) { selections++; nextPitch=Time.unscaledTime+.5f; Log("PITCH_SELECTION actual EventSystem.RaycastAll and pointer down/up/click"); }
            }
            else if(manager.playState==PlayState.PLAY_BATTING_VIEW && manager.pitcher.pState==PitcherState._GET_SIGN && !manager.pitcher.bRelease)
            {
                var control=Object.FindFirstObjectByType<ControlPitchingUI>();
                if(control!=null && control._active.activeInHierarchy) { control.setRelease(); releases++; nextPitch=Time.unscaledTime+1; Log("PITCH_RELEASE production ControlPitchingUI.setRelease callback"); }
            }
        }
    }
    static bool Click(Button button)
    {
        if(!button.IsActive() || !button.IsInteractable() || EventSystem.current==null) return false;
        Canvas.ForceUpdateCanvases(); var canvas=button.GetComponentInParent<Canvas>();
        var rect=(RectTransform)button.transform;
        var point=RectTransformUtility.WorldToScreenPoint(canvas.worldCamera,rect.TransformPoint(rect.rect.center));
        var data=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=point,pressPosition=point};
        var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(data,hits);
        if(hits.Count==0 || hits[0].gameObject.GetComponentInParent<Button>()!=button) return false;
        data.pointerCurrentRaycast=data.pointerPressRaycast=hits[0];
        ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.pointerDownHandler);
        ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.pointerUpHandler);
        ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.pointerClickHandler);
        return true;
    }
    static void Log(string text) { File.AppendAllText(Output+"/full-gameplay-inputs.txt",DateTime.UtcNow.ToString("O")+" gameTime="+Time.time.ToString("F1")+" "+text+"\n"); }
    static void Save()
    {
        File.WriteAllText(Output+"/full-gameplay-summary.txt","Manual swings="+swings+"\nRaycast pitch selections="+selections+"\nPitch release callbacks="+releases+"\nHUD samples="+hudSamples+"\nPersistent HUD score mismatches="+mismatches.Count+"\nCaptured manual half innings="+string.Join(",",captured.OrderBy(s=>s))+"\n"+string.Join("\n",mismatches));
    }
}
#endif
