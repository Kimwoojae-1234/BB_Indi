#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Local, explicit editor commands only. No scene/play-mode changes or automatic migration on load.
[InitializeOnLoad]
public static class IntegratedUGUICommand
{
    public const string Request = "Library/UGUIMigration/request.txt";
    public const string Result = "Library/UGUIMigration/result.txt";
    static IntegratedUGUICommand() { EditorApplication.update += Update; }
    private static void Update()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
        string command;
        try { command = File.ReadAllText(Request).Trim(); File.Delete(Request); }
        catch (IOException) { return; } // The request writer may still hold its file handle.
        try
        {
            if (command == "refresh") AssetDatabase.Refresh();
            else if (command.StartsWith("remaining:")) RemainingUGUIMigration.Command(command.Substring(10));
            else if (command.StartsWith("integration:")) CanvasIntegrationChecks.Command(command.Substring(12));
            else if (command == "capture-connections") CanvasStructureChecks.CaptureStep4Baseline();
            else if (command == "check-connections") CanvasStructureChecks.CheckStep4();
            else if (command == "play-connections") CanvasConnectionPlayChecks.Start();
            else if (command == "play-batch-regression") CanvasBatchPlayChecks.StartStep4();
            else if (command == "capture-expansion") CanvasExpansionChecks.CaptureBaseline();
            else if (command == "check-expansion") CanvasExpansionChecks.Check();
            else if (command == "play-expansion") CanvasExpansionPlayChecks.Start();
            else if (command == "play-expansion-batches") CanvasBatchPlayChecks.StartStep5();
            else if (command == "play-expansion-connections") CanvasConnectionPlayChecks.StartStep5();
            else if (command == "capture-references") IntegratedUGUIChecks.CaptureReferences();
            else if (command == "check-applied") IntegratedUGUIChecks.CheckApplied();
            else if (command == "begin-apply") { AssetDatabase.DisallowAutoRefresh(); EditorApplication.LockReloadAssemblies(); }
            else if (command == "end-apply") { EditorApplication.UnlockReloadAssemblies(); AssetDatabase.AllowAutoRefresh(); AssetDatabase.Refresh(); }
            else if (command == "start-play-check") ScoreboardPlayChecks.StartOffline(IntegratedUGUIConverter.Output);
            else if (command == "stop-play-check") EditorApplication.isPlaying = false;
            else if (command == "snapshot") IntegratedUGUIPlayChecks.Snapshot();
            else if (command == "repair-renderers") IntegratedUGUIPlayChecks.RepairRenderers();
            else if (command.StartsWith("click:")) IntegratedUGUIPlayChecks.ClickAction(command.Substring(6));
            else if (command.StartsWith("fixture-click:")) IntegratedUGUIPlayChecks.ClickFixture(command.Substring(14));
            else if (command == "assist") IntegratedUGUIPlayChecks.EnableAssist();
            else if (command == "fixture-resume") BaseBall.BallPlay.IngameUI.GetPauseUI().pressContinue();
            else if (command == "fixture-auto" || command == "fixture-manual") IntegratedUGUIPlayChecks.ChangeModeFixture(command == "fixture-auto");
            else if (command != "ping") throw new InvalidOperationException("Unknown migration command: " + command);
            File.WriteAllText(Result, "PASS " + command + " " + DateTime.UtcNow.ToString("O"));
        }
        catch (Exception exception) { File.WriteAllText(Result, "FAIL " + command + "\n" + exception); Debug.LogException(exception); }
    }
}
#endif
