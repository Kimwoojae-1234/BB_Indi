#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LanguageSettingsChecks
{
    private static readonly string[] Names = { "English", "Japanese", "Korean", "Spanish", "ChineseTraditional" };
    private static readonly GameDefine.eLanguage[] Languages = {
        GameDefine.eLanguage.English, GameDefine.eLanguage.Japan, GameDefine.eLanguage.Korea,
        GameDefine.eLanguage.Spain, GameDefine.eLanguage.China_Traditional
    };

    [MenuItem("Tools/UI/Check Language Selection")]
    public static void Run()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Stop Play Mode before checking language selection.");
        var prefab = Resources.Load<GameObject>("UI/Popup/Popup_Setting_Language");
        Require(prefab != null, "Language popup resource is missing.");
        var originalFlags = prefab.transform.Find("Popup/Flags");
        Require(originalFlags != null && originalFlags.childCount == 20, "Language flag container is missing.");
        Require(originalFlags.Cast<Transform>().Count(t => t.gameObject.activeSelf) == 5, "The prefab must show exactly five languages.");
        Require(originalFlags.Cast<Transform>().Where(t => t.gameObject.activeSelf).Select(t => t.name)
            .SequenceEqual(Names.Select(n => "Language_" + n)), "The visible language list is incorrect.");

        var preview = EditorSceneManager.NewPreviewScene();
        var previousLanguage = L10n.Language;
        var previousConfig = GameConfig.CurrentLanguage;
        bool hadPreference = PlayerPrefs.HasKey(L10n.PreferenceKey);
        string previousPreference = PlayerPrefs.GetString(L10n.PreferenceKey);
        GameObject fixture = null;
        Popup_Setting settings = null;
        var boundLabels = new List<LocalizedText>();
        try
        {
            fixture = new GameObject("Language selection check", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(fixture, preview);
            fixture.SetActive(false);
            var popup = UnityEngine.Object.Instantiate(prefab, fixture.transform).GetComponent<Popup_Setting_Language>();
            popup.gameObject.SetActive(false);
            settings = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("UI/Popup/Popup_Setting"), fixture.transform).GetComponent<Popup_Setting>();
            var lobby = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("UI/Window/UI_LobbyRe"), fixture.transform);
            lobby.SetActive(false);
            fixture.SetActive(true);
            popup.Open();
            settings.Open();
            // Preview objects do not receive the normal Play Mode enable callbacks.
            InvokeLifecycle(settings, "OnEnable");
            HookBindings(popup.gameObject, boundLabels);
            HookBindings(settings.gameObject, boundLabels);
            HookBindings(lobby, boundLabels);
            var languageName = settings.transform.Find("Popup/Group_Right/Button_List/Button_Language/Text_Laguage").GetComponent<TMPro.TMP_Text>();
            var languageFlag = settings.transform.Find("Popup/Group_Right/Button_List/Button_Language/Flag").GetComponent<Image>();
            Require(languageName.GetComponent<LocalizedText>() == null, "Selected language name must remain code-owned.");
            var nativeNames = new[] { "English", "日本語", "한국어", "Español", "繁體中文" };
            var flagSuffixes = new[] { "_Eng", "_Jpn", "_Kor", "_Esp", "_Twn" };
            var settingsTitles = new[] { "Settings", "設定", "설정", "Ajustes", "設定" };
            var catalog = L10n.LoadData().LocalizationItem.ToDictionary(row => row.key);
            var flags = popup.transform.Find("Popup/Flags");
            for (int i = 0; i < Names.Length; i++)
            {
                var button = flags.Find("Language_" + Names[i]).GetComponent<Button>();
                Require(button != null && button.interactable && button.targetGraphic.raycastTarget, "Language flag is not clickable: " + Names[i]);
                Require(button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentTarget(0) == popup &&
                    button.onClick.GetPersistentMethodName(0) == nameof(Popup_Setting_Language.OnClickLanguage), "Language click is disconnected: " + Names[i]);
                button.onClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
                button.onClick.Invoke();
                Require(L10n.Language == Languages[i] && GameConfig.CurrentLanguage == Languages[i], "Wrong selected language: " + Names[i]);
                CheckGlyphs(languageName);
                Require(languageName.text == nativeNames[i], "Settings still shows the old language name: " + Names[i]);
                Require(languageFlag.sprite != null && languageFlag.sprite.name.EndsWith(flagSuffixes[i], StringComparison.Ordinal), "Settings still shows the old flag: " + Names[i]);
                foreach (var binding in boundLabels)
                {
                    string expected = Translated(catalog[binding.key], Languages[i]);
                    Require(!string.IsNullOrWhiteSpace(expected), "Visible UI translation is missing: " + binding.key + "/" + Names[i]);
                    string actual = binding.tmp != null ? binding.tmp.text : binding.legacy != null ? binding.legacy.text : binding.element.text;
                    if (binding.tmp != null) CheckGlyphs(binding.tmp);
                    Require(actual == expected, "Visible label did not change: " + binding.key + "/" + Names[i]);
                }
                Require(settings.transform.Find("Popup/Text_Title").GetComponent<TMPro.TMP_Text>().text == settingsTitles[i], "Settings title is unchanged.");
                Require(GameConfig.GetRegistLanguage() == Languages[i] && PlayerPrefs.GetString(L10n.PreferenceKey) == Languages[i].ToString(),
                    "Language preference was not saved: " + Names[i]);
                Require(flags.Cast<Transform>().Count(t => t.gameObject.activeSelf && t.Find("Icon_check").gameObject.activeSelf) == 1 &&
                    button.transform.Find("Icon_check").gameObject.activeSelf, "Selected language marker is incorrect: " + Names[i]);
                popup.gameObject.SetActive(false);
                popup.Open();
                Require(button.transform.Find("Icon_check").gameObject.activeSelf, "Selection is lost when reopening: " + Names[i]);
            }
            popup.OnClickLanguage((int)GameDefine.eLanguage.China_Simplified);
            popup.OnClickLanguage(-1);
            popup.OnClickLanguage((int)GameDefine.eLanguage.MAX);
            Require(L10n.Language == GameDefine.eLanguage.China_Traditional, "A hidden or invalid language was accepted.");
            Require(flags.Cast<Transform>().Count(t => t.gameObject.activeSelf) == 5, "Hidden languages became visible.");
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)flags);
            var activeFlags = flags.Cast<RectTransform>().Where(t => t.gameObject.activeSelf).ToArray();
            Require(activeFlags.All(t => Mathf.Approximately(t.anchoredPosition.y, activeFlags[0].anchoredPosition.y)), "Hidden flags left empty rows in the grid.");
            Require(activeFlags.Zip(activeFlags.Skip(1), (left, right) => right.anchoredPosition.x - left.anchoredPosition.x > left.rect.width).All(x => x),
                "Visible language buttons overlap.");
        }
        finally
        {
            foreach (var binding in boundLabels) if (binding != null) InvokeLifecycle(binding, "OnDisable");
            if (settings != null) InvokeLifecycle(settings, "OnDisable");
            if (fixture != null) UnityEngine.Object.DestroyImmediate(fixture);
            EditorSceneManager.ClosePreviewScene(preview);
            L10n.SetLanguage(previousLanguage);
            GameConfig.CurrentLanguage = previousConfig;
            if (hadPreference) PlayerPrefs.SetString(L10n.PreferenceKey, previousPreference);
            else PlayerPrefs.DeleteKey(L10n.PreferenceKey);
            PlayerPrefs.Save();
        }
        Directory.CreateDirectory("Library/UIRegression");
        File.WriteAllText("Library/UIRegression/language-settings.txt",
            "PASS: five visible languages; all five serialized clicks; saved language; selection markers; reopening; hidden/invalid rejection; compact layout; actual lobby/settings/picker text in all five languages; selected language name/flag; glyph coverage for translated labels.\n" + DateTime.UtcNow.ToString("O"));
        Debug.Log("[UI] Language selection checks passed.");
    }

    private static void CheckGlyphs(TMPro.TMP_Text text)
    {
        Require(text.font != null && text.font.HasCharacters(text.text, out uint[] missing, true, true),
            "Glyphs are missing for translated label: " + text.name + " = " + text.text);
    }

    private static void InvokeLifecycle(MonoBehaviour component, string method)
    {
        component.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);
    }

    private static void HookBindings(GameObject root, List<LocalizedText> bindings)
    {
        foreach (var binding in root.GetComponentsInChildren<LocalizedText>(true))
        {
            InvokeLifecycle(binding, "OnEnable");
            bindings.Add(binding);
        }
    }

    private static string Translated(LocalizationItem row, GameDefine.eLanguage language)
    {
        switch (language)
        {
            case GameDefine.eLanguage.Japan: return row.Jpn;
            case GameDefine.eLanguage.Korea: return row.Kor;
            case GameDefine.eLanguage.Spain: return row.Esp;
            case GameDefine.eLanguage.China_Traditional: return row.ChnT;
            default: return row.Eng;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
#endif
