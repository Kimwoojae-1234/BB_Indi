using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine;

// One-shot request uses Unity's live PlayerSettings, rather than editing the YAML while the editor is open.
[InitializeOnLoad]
public static class BeachLeagueTestSetup
{
    private const string Symbol = "BEACH_LEAGUE_ONE_GAME_TEST";
    private const string Request = "Temp/EnableBeachLeagueOneGameTest.request";
    static BeachLeagueTestSetup() { EditorApplication.update += ApplyPending; }

    private static void ApplyPending()
    {
        if (!File.Exists(Request)) { EditorApplication.update -= ApplyPending; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        EditorApplication.update -= ApplyPending;
        var targets = new[] { NamedBuildTarget.Standalone, NamedBuildTarget.Android,
            NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup) };
        bool changed = false;
        foreach (var target in targets)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbols(target).Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (symbols.Contains(Symbol)) continue;
            symbols.Add(Symbol);
            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", symbols));
            changed = true;
        }
#if BEACH_LEAGUE_ONE_GAME_TEST
        if (!changed)
        {
            File.Delete(Request);
            const string message = "PASS: BEACH_LEAGUE_ONE_GAME_TEST is compiled and enabled in Unity PlayerSettings. League length = 1.";
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/beach-onegame-editor-check.log", message);
            Debug.Log(message);
            return;
        }
#endif
        AssetDatabase.SaveAssets();
        CompilationPipeline.RequestScriptCompilation();
    }
}
