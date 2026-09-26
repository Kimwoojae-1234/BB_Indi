using System;
using TMPro;
using UnityEngine;

/// <summary>Game manager facade for the shared localization catalog.</summary>
public class LocalizationManager : MonoBehaviour
{
    public event Action OnLanguageChanged
    {
        add { L10n.LanguageChanged += value; }
        remove { L10n.LanguageChanged -= value; }
    }

    private void Awake()
    {
        L10n.Initialize();
        GameConfig.ChangeLanguage();
    }

    public LocalizationDataRecord LoadLocalizeData() => L10n.LoadData();
    public string GetLocalizedText(string key) => L10n.T(key);
    public string GetLocalizedText(string key, GameDefine.eLanguage language) => L10n.T(key, language);
    public string Format(string key, params object[] args) => L10n.F(key, args);
    public void SetLanguage(GameDefine.eLanguage language) => L10n.SetLanguage(language);

    public string GetUILocalizedValue2(string key, TextMeshProUGUI text = null) => GetLocalizedValue2(L10n.Language, key, text);
    public string GetLocalizedValue2(GameDefine.eLanguage language, string key, TextMeshProUGUI text = null) => L10n.T(key, language);
    public string GetUILocalizedValue(string key, UnityEngine.UI.Text text) => GetLocalizedValue(L10n.Language, key, text);
    public string GetLocalizedValue(GameDefine.eLanguage language, string key, UnityEngine.UI.Text text = null) => L10n.T(key, language);
}
