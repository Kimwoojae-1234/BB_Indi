using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting : UIPopup
{
    [Serializable]
    private sealed class LanguageDisplay
    {
        public GameDefine.eLanguage language;
        public string nativeName;
        public Sprite flag;
    }

    [SerializeField] private TMP_Text languageLabel;
    [SerializeField] private Image languageFlag;
    [SerializeField] private LanguageDisplay[] languageDisplays = Array.Empty<LanguageDisplay>();

    private void OnEnable()
    {
        L10n.LanguageChanged += RefreshLanguage;
        RefreshLanguage();
    }

    private void OnDisable()
    {
        L10n.LanguageChanged -= RefreshLanguage;
    }

    public override void Open()
    {
        base.Open();
        RefreshLanguage();
    }

    private void RefreshLanguage()
    {
        var display = Array.Find(languageDisplays, entry => entry.language == L10n.Language);
        if (display == null) return;
        if (languageLabel != null) languageLabel.text = display.nativeName;
        if (languageFlag != null) languageFlag.sprite = display.flag;
    }

    public void OnClickLanguageSetting()
    {
        KOBManager.Popup.OpenPopup<Popup_Setting_Language>().BackToPrevPopup = true;
    }
}
