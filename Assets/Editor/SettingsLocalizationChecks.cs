using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SettingsLocalizationChecks
{
    [MenuItem("Tools/Clutch/Validate Settings and UI Localization")]
    public static void Validate()
    {
        var json = Resources.Load<TextAsset>("Localization/UIStrings");
        Require(json != null, "Missing UI catalog");
        var entries = JsonUtility.FromJson<UILocalization.Catalog>(json.text).entries;
        var keys = new HashSet<string>();
        foreach (var e in entries)
        {
            Require(keys.Add(e.key), "Duplicate key: " + e.key);
            Require(!string.IsNullOrEmpty(e.en) && !string.IsNullOrEmpty(e.ko) && !string.IsNullOrEmpty(e.ja), "Empty translation: " + e.key);
            if (e.key.StartsWith("ui."))
            {
                string Args(string s) => string.Join(",", Regex.Matches(s, @"\{(\d+)(?:[^}]*)\}").Cast<Match>().Select(m => m.Groups[1].Value).OrderBy(v => v));
                Require(Args(e.en) == Args(e.ko) && Args(e.en) == Args(e.ja), "Format arguments: " + e.key);
            }
        }
        int texts = 0, bound = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/UI" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Require(prefab != null, "Cannot load " + path);
            foreach (var text in prefab.GetComponentsInChildren<TMP_Text>(true))
            {
                texts++;
                var binding = text.GetComponent<LocalizedUIText>();
                Require(binding != null, "Missing font/localization binding: " + path + "/" + text.name);
                var key = new SerializedObject(binding).FindProperty("key").stringValue;
                if (string.IsNullOrEmpty(key)) continue;
                bound++;
                Require(keys.Contains(key), "Unknown key: " + key);
            }
        }
        var settings = Resources.Load<GameObject>("UI/Popup/Popup_Setting");
        var left = settings.transform.Find("Popup/Group_Left");
        Require(left.childCount == 7, "Expected four rows and three dividers");
        Require(left.GetChild(0).Find("Toggle") != null, "Push row");
        Require(left.GetChild(2).GetComponentInChildren<Slider>(true) != null, "Sound row");
        Require(left.GetChild(4).GetComponentInChildren<Slider>(true) != null, "Music row");
        Require(left.GetChild(6).Find("Toggle") != null, "Vibration row");
        var settingsFields = new SerializedObject(settings.GetComponent<Popup_Setting>());
        foreach (var field in new[] { "sound", "music", "vibration", "pushAlarm" })
            Require(settingsFields.FindProperty(field).objectReferenceValue != null, "Unbound setting: " + field);
        Require(Resources.Load<GameObject>("UI/Popup/Popup_Setting_Language").transform.Find("Popup/Flags") != null, "Language panel path");
        Require(Resources.Load<GameObject>("UI/Window/UI_LobbyRe").transform.Find("Button_Settings/Button_Settings").GetComponent<Button>() != null, "Lobby settings button");
        var chars = new string(entries.Where(e => e.key.StartsWith("ui.") || e.key.StartsWith("language.")).SelectMany(e => e.ko + e.ja).Where(c => c >= 0x3000).Distinct().ToArray());
        Require(UILocalization.Font == Resources.Load<TMP_FontAsset>(UILocalization.CjkSdfResource), "CJK SDF asset");
        Require(UILocalization.Font.HasCharacters(chars, out uint[] missing, true, true), "Missing font glyphs: " + string.Join(",", missing ?? new uint[0]));
        var plainEnglish = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/BundleResource/Font/LilitaOne-Regular SDF.asset");
        var outlinedEnglish = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/BundleResource/Font/LilitaOne-Regular Outline 32 SDF.asset");
        Require(UILocalization.CjkMaterial(plainEnglish, plainEnglish.material) == UILocalization.Font.material, "Plain English must use plain CJK material");
        var outlineD = Resources.Load<Material>(UILocalization.CjkSdfResource + " OutLine D");
        Require(outlineD != null && UILocalization.CjkMaterial(outlinedEnglish, outlinedEnglish.material) == outlineD, "Outlined English must use OutLine D");
        Require(outlineD.mainTexture == UILocalization.Font.atlasTexture, "Outline material atlas reference");
        string old = UILocalization.Language;
        bool saved = PlayerPrefs.HasKey("UI.Language");
        var test = new GameObject("LocalizationCheck", typeof(RectTransform), typeof(TextMeshProUGUI));
        try
        {
            var text = test.GetComponent<TextMeshProUGUI>();
            var authoredFont = text.font;
            var authoredMaterial = text.fontSharedMaterial;
            var authoredStyle = text.fontStyle;
            text.fontSize = 31;
            text.enableAutoSizing = false;
            UILocalization.SetLanguage("en");
            UILocalization.Set(text, "Settings");
            foreach (string language in new[] { "en", "ko", "ja" })
            {
                UILocalization.SetLanguage(language);
                text.GetComponent<LocalizedUIText>().Refresh();
                Require(text.text == UILocalization.Get("Settings"), "Live language switch");
                Require(PlayerPrefs.GetString("UI.Language") == language, "Language persistence");
                Require(text.font == (language == "en" ? authoredFont : UILocalization.Font), "Language-specific font");
                Require(text.fontStyle == (language == "en" ? authoredStyle : authoredStyle | FontStyles.Bold), "Korean/Japanese bold style");
            }
            var clone = UnityEngine.Object.Instantiate(test);
            UILocalization.SetLanguage("en");
            text.GetComponent<LocalizedUIText>().Refresh();
            clone.GetComponent<LocalizedUIText>().Refresh();
            Require(text.font == authoredFont && text.fontSharedMaterial == authoredMaterial && text.fontSize == 31 && !text.enableAutoSizing,
                "English font, material and size must be restored exactly");
            Require(clone.GetComponent<TMP_Text>().font == authoredFont, "Cloned labels must restore their English font");
            UnityEngine.Object.DestroyImmediate(clone);
            foreach (string language in new[] { "ko", "ja" })
            {
                UILocalization.SetLanguage(language);
                foreach (string number in new[] { "1,234", "250/60", "+10", "99.5%", "<color=yellow>1,000</color>", "-" })
                {
                    text.text = number;
                    text.GetComponent<LocalizedUIText>().SendMessage("LateUpdate");
                    Require(text.font == authoredFont && text.fontSharedMaterial == authoredMaterial && text.fontStyle == authoredStyle,
                        "Numeric display must retain authored styling: " + number);
                }
                UILocalization.Set(text, "Settings");
                Require(text.font == UILocalization.Font && (text.fontStyle & FontStyles.Bold) != 0, "Text after a number must regain localized bold styling");
            }
            Require(!LocalizedUIText.IsNumericDisplay("레벨 10") && !LocalizedUIText.IsNumericDisplay("レベル10"), "Mixed labels are not numeric-only");
            text.text = "Player-owned name";
            text.GetComponent<LocalizedUIText>().Refresh();
            Require(text.text == "Player-owned name", "Player data must be preserved");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(test);
            UILocalization.SetLanguage(old);
            if (!saved) { PlayerPrefs.DeleteKey("UI.Language"); PlayerPrefs.Save(); }
        }
        Debug.Log($"Settings localization PASS: {entries.Length} keys, {texts} TMP labels, {bound} translated prefab labels, 3 languages, CJK glyph coverage.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
