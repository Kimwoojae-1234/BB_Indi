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
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public sealed class PitchSelectionPlayProbe
{
    private string output;
    private IEnumerator routine;

    public IEnumerator StartCheck(BallPlayManager manager, ControlPitchingSelect selector, string folder)
    {
        output = folder;
        routine = Check(manager, selector);
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
                Debug.LogException(exception);
                yield break;
            }
            yield return next;
        }
    }

    private void Require(bool condition, string message)
    {
        PitchSelectionMigrationChecks.Require(condition, message);
        File.AppendAllText(output + "/interaction-checks.txt", "PASS " + message + "\n");
    }

    private IEnumerator Check(BallPlayManager manager, ControlPitchingSelect selector)
    {
        yield return new WaitForSeconds(1);
        var row = selector.button.First(b => b.gameObject.activeInHierarchy);
        var pitch = manager.pitcher.pPitcher.getBallType().First(p => p != PitchingArsenal.NONE);
        ScreenCapture.CaptureScreenshot(output + "/menu.png");
        Require(manager.bMyTurn == false, "Defensive inning reached through normal match flow");
        Require(selector._active.activeInHierarchy, "Pitch menu visible");
        int activeCount = selector.button.Count(b => b.gameObject.activeInHierarchy);
        File.AppendAllText(output + "/interaction-checks.txt", "Active slots=" + activeCount + " pitch=" + pitch + " before=" + manager.playState + "\n");
        if (row.UguiView != null)
        {
            Require(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1 && EventSystem.current.currentInputModule != null,
                "Exactly one active EventSystem with an input module");
            for (int i = 0; i < activeCount; i++)
                Require(Vector3.Distance(selector.button[i].transform.localPosition, new Vector3(20 * i, 66 * i, 0)) < .1f, "Entrance settles slot " + i);
            var camera = selector.UguiView.displayCanvas.worldCamera;
            Require(camera != null, "NGUI UI camera assigned to UGUI Canvas");
            var data = new PointerEventData(EventSystem.current)
            {
                pointerId = -1, button = PointerEventData.InputButton.Left,
                position = camera.WorldToScreenPoint(row.transform.position)
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, hits);
            Require(hits.Count > 0 && hits[0].gameObject == row.gameObject, "Actual match EventSystem raycast reaches pitch button");
            data.pointerCurrentRaycast = hits[0];
            ExecuteEvents.Execute(row.gameObject, data, ExecuteEvents.pointerDownHandler);
            yield return new WaitForSecondsRealtime(.25f);
            Require(Vector3.Distance(row.transform.localScale, Vector3.one * .9f) < .01f, "Press scale reaches 0.9");
            ExecuteEvents.Execute(row.gameObject, new PointerEventData(EventSystem.current) { pointerId = 8 }, ExecuteEvents.pointerUpHandler);
            Require(selector.GetSelectBall() == PitchingArsenal.NONE, "Unrelated touch release does not select");
            data.position = new Vector2(-100, -100);
            ExecuteEvents.Execute(row.gameObject, data, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(row.gameObject, data, ExecuteEvents.pointerClickHandler);
            Require(selector.GetSelectBall() == pitch, "Release outside selects original pressed pitch");
            Require(selector.button.Take(activeCount).All(b => !b.UguiView.interactable), "Selection locks all active choices");
            Require(row.UguiView.light.activeSelf, "Selection flash starts");
            yield return new WaitForSeconds(.25f);
            Require(!row.UguiView.light.activeSelf && row.UguiView.effect.gameObject.activeSelf, "Flash transitions into expanding ring");
            yield return new WaitForSeconds(.4f);
            Require(!row.UguiView.effect.gameObject.activeSelf, "Selection ring finishes");
        }
        else
        {
            row.gameObject.SendMessage("OnPress", true);
            yield return new WaitForSecondsRealtime(.25f);
            row.gameObject.SendMessage("OnPress", false);
            Require(selector.GetSelectBall() == pitch, "Legacy release selects pitch");
            yield return new WaitForSeconds(.65f);
        }
        yield return new WaitForSeconds(1);
        Require(!selector._active.activeInHierarchy, "Menu closes after original selection delay");
        Require(manager.playState == PlayState.PLAY_BATTING_VIEW, "Match advances to PLAY_BATTING_VIEW");
        ScreenCapture.CaptureScreenshot(output + "/after-selection.png");
        if (row.UguiView != null)
        {
            // Exercise re-entry and multi-pointer behavior on an isolated candidate,
            // with its gameplay callback replaced so this cannot affect the match.
            var scratch = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PitchSelectionUGUIConverter.Candidate));
            scratch.transform.position = new Vector3(10000, 10000, 0);
            scratch.SetActive(true);
            var button = scratch.GetComponentInChildren<PitchSelectionButtonView>();
            button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            int invoked = 0;
            button.onClick.AddListener(() => { invoked++; button.Release(0, 0); });
            button.Begin(PitchingArsenal.CURVE, 80, 0);
            yield return new WaitForSeconds(.4f);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = 2 };
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            Require(invoked == 1, "Release plus click plus duplicate release invokes once");
            scratch.SetActive(false);
            scratch.SetActive(true);
            button.Begin(PitchingArsenal.SLIDER, 59, 0);
            yield return new WaitForSeconds(1.4f);
            Require(button.interactable && Vector3.Distance(button.transform.localPosition, Vector3.zero) < .1f && button.transform.localScale == Vector3.one &&
                !button.light.activeSelf && !button.effect.gameObject.activeSelf, "Re-entry clears old exit, pointer, scale and effect state");
            Require(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "Re-entry does not duplicate EventSystem");
            Object.Destroy(scratch);
        }
        File.AppendAllText(output + "/interaction-checks.txt", "COMPLETE — synthetic Unity event dispatch, not native mouse/touch or PVP verification.\n");
    }

}
#endif
