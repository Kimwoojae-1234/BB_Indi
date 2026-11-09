using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

// Independent of GameConfig/NGUI Localization: only the Resources/UI interface opts in.
public static class UILocalization
{
    [Serializable] public class Entry { public string key, en, ko, ja; }
    [Serializable] public class Catalog { public Entry[] entries; }
    private static Dictionary<string, Entry> entries;
    private static string language;
    private static TMP_FontAsset font;
    public const string CjkFontResource = "Font/Localized/NotoSansCJKkr-Medium";
    public const string CjkSdfResource = "Font/Localized/NotoSans-Base-Medium SDF";
    private static Material outlineMaterial;
    public static event Action Changed;
    public static string Language => language ?? (language = Normalize(PlayerPrefs.GetString("UI.Language", "en")));
    public static string Normalize(string value) => value == "ko" || value == "ja" ? value : "en";
    public static GameDefine.eLanguage LegacyLanguage => Language == "ko" ? GameDefine.eLanguage.Korea :
        Language == "ja" ? GameDefine.eLanguage.Japan : GameDefine.eLanguage.English;
    public static void SetLanguage(string value)
    {
        language = Normalize(value);
        PlayerPrefs.SetString("UI.Language", language);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
    public static bool IsIngameScene
    {
        get
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            return scene == "BallPlay" || scene == "BallPlayRealTime" || scene == "MainLoading";
        }
    }
    public static string GetEnglish(string key)
    {
        if (string.IsNullOrEmpty(key)) return key ?? "";
        EnsureLoaded();
        return entries.TryGetValue(key, out var item) ? item.en : key;
    }
    public static string Get(string key)
    {
        if (IsIngameScene) return GetEnglish(key);
        if (string.IsNullOrEmpty(key)) return key ?? "";
        EnsureLoaded();
        if (!entries.TryGetValue(key, out var item)) return key;
        string value = Language == "ko" ? item.ko : Language == "ja" ? item.ja : item.en;
        return string.IsNullOrEmpty(value) ? item.en : value;
    }
    public static string Format(string key, params object[] args) => string.Format(CultureInfo.CurrentCulture, Get(key), args);
    public static bool Contains(string key) { EnsureLoaded(); return entries.ContainsKey(key); }
    private static void EnsureLoaded()
    {
        if (entries != null) return;
        entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        var asset = Resources.Load<TextAsset>("Localization/UIStrings");
        if (asset == null) { Debug.LogError("Missing Localization/UIStrings.json"); return; }
        foreach (var item in JsonUtility.FromJson<Catalog>(asset.text).entries)
        {
            entries[item.key] = item;
            if (!string.IsNullOrEmpty(item.en) && !entries.ContainsKey(item.en)) entries[item.en] = item;
        }
    }
    public static TMP_FontAsset Font
    {
        get
        {
            if (font != null) return font;
            font = Resources.Load<TMP_FontAsset>(CjkSdfResource);
            if (font == null) { Debug.LogError("Missing localized Noto Sans SDF font"); return TMP_Settings.defaultFontAsset; }
            return font;
        }
    }

    public static bool HasEnglishOutline(TMP_FontAsset englishFont, Material englishMaterial)
    {
        // The existing Lilita Outline variants bake their border into the atlas.
        if (englishFont != null && englishFont.name.IndexOf("Outline", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        return englishMaterial != null && englishMaterial.HasProperty("_OutlineWidth") &&
            englishMaterial.GetFloat("_OutlineWidth") > 0 &&
            (!englishMaterial.HasProperty("_OutlineColor") || englishMaterial.GetColor("_OutlineColor").a > 0);
    }

    public static Material CjkMaterial(TMP_FontAsset englishFont, Material englishMaterial)
    {
        if (!HasEnglishOutline(englishFont, englishMaterial)) return Font.material;
        if (outlineMaterial == null)
            outlineMaterial = Resources.Load<Material>(CjkSdfResource + " OutLine D");
        return outlineMaterial != null ? outlineMaterial : Font.material;
    }
    public static void Set(TMP_Text text, string key, params object[] args)
    {
        if (text == null) return;
        var binding = text.GetComponent<LocalizedUIText>();
        if (binding == null) binding = text.gameObject.AddComponent<LocalizedUIText>();
        binding.Bind(key, args);
    }
    public static void Unavailable() => KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(Get("settings.unavailable"));
}
