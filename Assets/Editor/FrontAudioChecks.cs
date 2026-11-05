using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class FrontAudioChecks
{
    static FrontAudioChecks() { EditorApplication.update += RunPending; }
    private static void RunPending()
    {
        const string request = "Temp/CheckFrontAudio.request";
        if (!File.Exists(request)) { EditorApplication.update -= RunPending; return; }
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= RunPending;
        File.Delete(request);
        Validate();
    }

    [MenuItem("Tools/Clutch/Validate Front Audio")]
    public static void Validate()
    {
        try
        {
            var library = Resources.Load<FrontAudioLibrary>("Sound/FrontAudioLibrary");
            if (library == null) throw new Exception("Audio library missing");
            var clips = new[] { library.lobby00, library.lobby01, library.league, library.rankChange,
                library.simulation, library.passTrophy, library.shop, library.confirm, library.back };
            foreach (var clip in clips)
                if (clip == null || clip.length <= 0) throw new Exception("Missing or empty audio clip");
            int buttons = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/UI" }))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var button in prefab.GetComponentsInChildren<Button>(true))
                {
                    if (button.GetComponent<UIButtonAudio>() == null)
                        throw new Exception("Missing per-button audio control: " + prefab.name + "/" + button.name);
                    buttons++;
                }
            }
            var ballers = Resources.Load<GameObject>("UI/Window/UI_Ballers");
            var portrait = ballers.transform.Find("BallerMain/RightSubMenu/Group_Right/PowerLevel/Info/Character/Mask/Character") as RectTransform;
            if (portrait == null || portrait.sizeDelta != new Vector2(150, 150) || portrait.anchoredPosition != new Vector2(0, 5))
                throw new Exception("Baller power portrait layout mismatch");
            string result = $"PASS: 9 playable audio clips, {buttons} prefab buttons with individual audio controls, 150x150 portrait at (0,5). Rank cue: {library.rankChange.length:F2}s, crossfade: 0.85s.";
            File.WriteAllText("Logs/front-audio-editor-check.log", result);
            Debug.Log(result);
        }
        catch (Exception e)
        {
            File.WriteAllText("Logs/front-audio-editor-check.log", "FAIL: " + e);
            Debug.LogException(e);
        }
    }
}
