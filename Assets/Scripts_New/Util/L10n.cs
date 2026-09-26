using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>Key-based game text. The Excel master is exported to Localization/LocalizationItem.</summary>
public static class L10n
{
    public const string ResourcePath = "Localization/LocalizationItem";
    public const string PreferenceKey = "RegistLanguae";
    private static Dictionary<string, LocalizationItem> items;
    private static readonly HashSet<string> reported = new HashSet<string>(StringComparer.Ordinal);
    private static readonly Dictionary<Array, Array> translatedArrays = new Dictionary<Array, Array>();
    private static GameDefine.eLanguage language = GameDefine.eLanguage.English;
    public static GameDefine.eLanguage Language => language;
    public static event Action LanguageChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        items = null;
        reported.Clear();
        translatedArrays.Clear();
        language = GameDefine.eLanguage.English;
        LanguageChanged = null;
    }

    public static void Initialize()
    {
        if (items != null) return;
        items = new Dictionary<string, LocalizationItem>(StringComparer.Ordinal);
        var asset = Resources.Load<TextAsset>(ResourcePath);
        if (asset == null) { Report("table", "Localization table is missing: " + ResourcePath); return; }
        try
        {
            var data = JsonUtility.FromJson<LocalizationDataRecord>(asset.text);
            if (data?.LocalizationItem == null) throw new FormatException("LocalizationItem array is missing.");
            foreach (var item in data.LocalizationItem)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.key)) continue;
                if (items.ContainsKey(item.key)) { Report(item.key, "Duplicate localization key: " + item.key); continue; }
                items.Add(item.key, item);
            }
        }
        catch (Exception exception) { Report("table", "Cannot read localization table: " + exception.Message); }
    }

    public static string T(string key) => T(key, language);
    public static string T(string key, GameDefine.eLanguage requestedLanguage)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        Initialize();
        if (!items.TryGetValue(key, out var item))
        {
            Report(key, "Missing localization key: " + key);
            return key;
        }
        string text = Value(item, requestedLanguage);
        if (string.IsNullOrEmpty(text)) text = item.Eng;
        if (string.IsNullOrEmpty(text)) text = item.Kor;
        if (string.IsNullOrEmpty(text))
        {
            if (item.allowEmpty) return string.Empty;
            Report(key, "Empty localization entry: " + key);
            return key;
        }
        return text.Replace("\\n", "\n");
    }

    public static string F(string key, params object[] args)
    {
        string text = T(key);
        try { return string.Format(Culture, text, args); }
        catch (FormatException exception)
        {
            Report(key + ":format", "Invalid localization arguments for " + key + ": " + exception.Message);
            return text;
        }
    }

    // Code owns these labels. Assign once; never add a component or register a refresh callback.
    public static void SetText(TMPro.TMP_Text target, string key, params object[] args)
    {
        if (target != null) target.text = args == null || args.Length == 0 ? T(key) : F(key, args);
    }
    public static void SetText(UnityEngine.UI.Text target, string key, params object[] args)
    {
        if (target != null) target.text = args == null || args.Length == 0 ? T(key) : F(key, args);
    }
    public static void SetText(BaseBall.BallPlay.UGUI.GameUIElement target, string key, params object[] args)
    {
        if (target != null) target.text = args == null || args.Length == 0 ? T(key) : F(key, args);
    }

    public static CultureInfo Culture => CultureInfo.GetCultureInfo(language == GameDefine.eLanguage.Korea ? "ko-KR" :
        language == GameDefine.eLanguage.Japan ? "ja-JP" : language == GameDefine.eLanguage.Spain ? "es-ES" :
        language == GameDefine.eLanguage.China_Traditional ? "zh-TW" : language == GameDefine.eLanguage.China_Simplified ? "zh-CN" : "en-US");

    public static bool Contains(string key) { Initialize(); return !string.IsNullOrEmpty(key) && items.ContainsKey(key); }

    // English is the release baseline. Existing translations remain available for explicit previews.
    public static void SetLanguage(GameDefine.eLanguage value)
    {
        if (!Enum.IsDefined(typeof(GameDefine.eLanguage), value) || value == GameDefine.eLanguage.MAX)
            value = GameDefine.eLanguage.English;
        GameConfig.CurrentLanguage = value;
        PlayerPrefs.SetString(PreferenceKey, value.ToString());
        PlayerPrefs.Save();
        if (language == value) return;
        language = value;
        translatedArrays.Clear();
        LanguageChanged?.Invoke();
    }

    public static string[] Texts(string[] keys)
    {
        if (translatedArrays.TryGetValue(keys, out var cached)) return (string[])cached;
        var result = new string[keys.Length];
        for (int i = 0; i < result.Length; i++) result[i] = T(keys[i]);
        translatedArrays[keys] = result;
        return result;
    }

    public static string[,] Texts(string[,] keys)
    {
        if (translatedArrays.TryGetValue(keys, out var cached)) return (string[,])cached;
        var result = new string[keys.GetLength(0), keys.GetLength(1)];
        for (int i = 0; i < result.GetLength(0); i++)
            for (int j = 0; j < result.GetLength(1); j++) result[i, j] = T(keys[i, j]);
        translatedArrays[keys] = result;
        return result;
    }

    public static LocalizationDataRecord LoadData()
    {
        Initialize();
        var result = new LocalizationItem[items.Count];
        items.Values.CopyTo(result, 0);
        return new LocalizationDataRecord { LocalizationItem = result };
    }

    private static string Value(LocalizationItem item, GameDefine.eLanguage value)
    {
        switch (value)
        {
            case GameDefine.eLanguage.Korea: return item.Kor;
            case GameDefine.eLanguage.Japan: return item.Jpn;
            case GameDefine.eLanguage.Spain: return item.Esp;
            case GameDefine.eLanguage.China_Traditional: return item.ChnT;
            case GameDefine.eLanguage.China_Simplified: return item.ChnS;
            case GameDefine.eLanguage.France: return item.Fra;
            case GameDefine.eLanguage.Germany: return item.Deu;
            case GameDefine.eLanguage.Indonesia: return item.Idn;
            case GameDefine.eLanguage.Italy: return item.Ita;
            case GameDefine.eLanguage.Portugal: return item.Prt;
            case GameDefine.eLanguage.Russia: return item.Rus;
            case GameDefine.eLanguage.Thailand: return item.Tha;
            case GameDefine.eLanguage.Turkey: return item.Tur;
            case GameDefine.eLanguage.Vietnam: return item.Vnm;
            default: return item.Eng;
        }
    }

    private static void Report(string id, string message)
    {
        if (reported.Add(id)) Debug.LogWarning("[Localization] " + message);
    }
}
