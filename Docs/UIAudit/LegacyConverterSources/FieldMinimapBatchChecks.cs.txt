#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Explicit command-line entry point for a disposable project copy. Never runs
// automatically in the user's editor and never edits the original checkout.
[InitializeOnLoad]
public static class FieldMinimapBatchChecks
{
    private const string Key = "BB.FieldMinimapBatch";
    static FieldMinimapBatchChecks() { EditorApplication.update += Update; }

    public static void Run()
    {
        if (!Application.isBatchMode || !File.Exists(".field-minimap-disposable-copy"))
            throw new InvalidOperationException("Run only in an explicitly marked disposable batch project.");
        Directory.CreateDirectory(FieldMinimapUGUIConverter.Output);
        File.Delete(FieldMinimapUGUIConverter.Output + "/batch-result.txt");
        try
        {
            FieldMinimapUGUIConverter.Build();
            if (!File.ReadAllText(FieldMinimapUGUIConverter.Output + "/presentation-checks.txt").StartsWith("PASS"))
                throw new InvalidOperationException("Presentation checks failed");
            SessionState.SetString(Key, "Legacy");
            SessionState.SetFloat(Key + ".Deadline", (float)EditorApplication.timeSinceStartup + 240);
            FieldMinimapPlayChecks.Start();
        }
        catch (Exception exception) { Finish(false, exception.ToString()); }
    }

    private static void Update()
    {
        string stage = SessionState.GetString(Key, "");
        if (string.IsNullOrEmpty(stage)) return;
        try
        {
            if (EditorApplication.timeSinceStartup > SessionState.GetFloat(Key + ".Deadline", 0))
                throw new TimeoutException("Batch stage timed out: " + stage);
            if (stage == "Apply" || stage == "Finish")
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                if (stage == "Finish") { Finish(true, "Legacy and UGUI controlled Play Mode checks completed; static renders generated."); return; }
                FieldMinimapUGUIConverter.Apply();
                SessionState.SetString(Key, "UGUI");
                SessionState.SetFloat(Key + ".Deadline", (float)EditorApplication.timeSinceStartup + 240);
                FieldMinimapPlayChecks.Start();
                return;
            }
            string path = FieldMinimapUGUIConverter.Output + "/" + stage + "/interaction-checks.txt";
            if (!File.Exists(path)) return;
            string report = File.ReadAllText(path);
            if (stage == "UGUI")
            {
                var extra = Errors("UGUI").Except(Errors("Legacy")).ToArray();
                if (extra.Length != 0) throw new InvalidOperationException("New UGUI runtime errors: " + string.Join("; ", extra));
            }
            if (report.Contains("FAIL ")) throw new InvalidOperationException(report);
            if (!report.Contains("COMPLETE")) return;
            SessionState.SetString(Key, stage == "Legacy" ? "Apply" : "Finish");
            EditorApplication.isPlaying = false;
        }
        catch (Exception exception) { Finish(false, exception.ToString()); }
    }

    private static string[] Errors(string stage)
    {
        string path = FieldMinimapUGUIConverter.Output + "/" + stage + "/gameplay-trace.txt";
        if (!File.Exists(path)) return Array.Empty<string>();
        return Regex.Matches(File.ReadAllText(path), "^ERROR (.+)$", RegexOptions.Multiline).Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToArray();
    }

    private static void Finish(bool pass, string detail)
    {
        SessionState.EraseString(Key);
        File.WriteAllText(FieldMinimapUGUIConverter.Output + "/batch-result.txt", (pass ? "PASS " : "FAIL ") + DateTime.UtcNow.ToString("O") + "\n" + detail + "\n");
        EditorApplication.Exit(pass ? 0 : 1);
    }
}
#endif
