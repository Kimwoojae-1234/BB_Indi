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

[InitializeOnLoad]
public static class PitchSelectionPlayChecks
{
    private const string Key = "BB.PitchSelectionPlayCheck";
    public static string Output => PitchSelectionUGUIConverter.Output + (SessionState.GetBool(Key + ".UGUI", false) ? "/UGUI" : "/Legacy");

    static PitchSelectionPlayChecks()
    {
        EditorApplication.update += Sample;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode) SessionState.SetBool(Key, false);
        };
    }

    [MenuItem("Tools/UI Migration/Start Pitch Selection Gameplay Check")]
    public static void Start()
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath).GetComponentInChildren<ControlPitchingSelect>(true);
        SessionState.SetBool(Key + ".UGUI", source.UguiView != null);
        ScoreboardPlayChecks.StartOffline(Output);
        SessionState.SetBool(Key, true);
        SessionState.SetBool(Key + ".Started", false);
        File.WriteAllText(Output + "/interaction-checks.txt", "WAITING for the normal offline match's pitching selection.\n");
    }

    private static void Sample()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false) || SessionState.GetBool(Key + ".Started", false)) return;
        var manager = Object.FindFirstObjectByType<BallPlayManager>();
        var selector = Object.FindFirstObjectByType<ControlPitchingSelect>();
        if (manager == null || selector == null || !selector._active.activeInHierarchy || !selector.button.Any(b => b.gameObject.activeInHierarchy)) return;
        SessionState.SetBool(Key + ".Started", true);
        manager.StartCoroutine(new PitchSelectionPlayProbe().StartCheck(manager, selector, Output));
    }
}

#endif
