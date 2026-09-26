using System;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting_Language : UIPopup
{
    [Serializable]
    private sealed class LanguageOption
    {
        public GameDefine.eLanguage language;
        public Button button;
        public GameObject selectedIndicator;
    }

    [SerializeField] private LanguageOption[] languageOptions = Array.Empty<LanguageOption>();

    private void OnEnable()
    {
        L10n.LanguageChanged += RefreshSelection;
        RefreshSelection();
    }

    private void OnDisable()
    {
        L10n.LanguageChanged -= RefreshSelection;
    }

    public override void Open()
    {
        base.Open();
        RefreshSelection();
    }

    public void OnClickLanguage(int language)
    {
        var option = Array.Find(languageOptions, entry => (int)entry.language == language);
        if (option == null || option.button == null || !option.button.gameObject.activeSelf)
            return;

        L10n.SetLanguage(option.language);
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        foreach (var option in languageOptions)
            if (option.selectedIndicator != null)
                option.selectedIndicator.SetActive(option.language == L10n.Language);
    }
}
