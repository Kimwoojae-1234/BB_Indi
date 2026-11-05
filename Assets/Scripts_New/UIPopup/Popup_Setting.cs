using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting : UIPopup
{
    [SerializeField] private Slider sound, music;
    [SerializeField] private Transform vibration, pushAlarm;
    private bool bound;

    public override void Open()
    {
        base.Open();
        Bind();
        sound.SetValueWithoutNotify(ClientSettings.SoundVolume);
        music.SetValueWithoutNotify(ClientSettings.MusicVolume);
        RefreshVibration();
        RefreshLanguage();
    }

    private void OnEnable() { if (bound) RefreshLanguage(); }

    private void RefreshLanguage()
    {
        foreach (var button in transform.Find("Popup/Group_Right").GetComponentsInChildren<Button>(true))
            if (button.name == "Button_Language")
            {
                var label = button.GetComponentInChildren<TMP_Text>(true);
                label.rectTransform.anchorMin = new Vector2(.06f, 0);
                label.rectTransform.anchorMax = new Vector2(.94f, 1);
                label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                label.alignment = TextAlignmentOptions.Center;
                UILocalization.Set(label, "language." + UILocalization.Language);
                foreach (var image in button.GetComponentsInChildren<Image>(true))
                    if (image.name == "Icon" || image.name == "Flag") image.gameObject.SetActive(false);
            }
    }

    private void Bind()
    {
        if (bound) return;
        bound = true;
        sound.minValue = music.minValue = 0;
        sound.maxValue = music.maxValue = 1;
        sound.wholeNumbers = music.wholeNumbers = false;
        foreach (var slider in new[] { sound, music })
        {
            var rect = (RectTransform)slider.transform;
            rect.sizeDelta = new Vector2(280, rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(-140, rect.anchoredPosition.y);
        }
        sound.onValueChanged.AddListener(ClientSettings.SetSound);
        music.onValueChanged.AddListener(ClientSettings.SetMusic);
        var alarmButton = pushAlarm.gameObject.AddComponent<Button>();
        alarmButton.targetGraphic = pushAlarm.GetComponent<Image>();
        alarmButton.onClick.AddListener(UILocalization.Unavailable);
        pushAlarm.parent.gameObject.AddComponent<CanvasGroup>().alpha = .55f;
        var vibrationButton = vibration.gameObject.AddComponent<Button>();
        vibrationButton.targetGraphic = vibration.GetComponent<Image>();
        vibrationButton.onClick.AddListener(() => { ClientSettings.SetVibration(!ClientSettings.Vibration); RefreshVibration(); });
        foreach (var button in transform.Find("Popup/Group_Right").GetComponentsInChildren<Button>(true))
        {
            if (button.name == "Button_Delete (1)") { button.gameObject.SetActive(false); continue; }
            button.onClick = new Button.ButtonClickedEvent();
            if (button.name == "Button_Language") button.onClick.AddListener(OnClickLanguageSetting);
            else
            {
                button.onClick.AddListener(UILocalization.Unavailable);
                var group = button.GetComponent<CanvasGroup>();
                if (group == null) group = button.gameObject.AddComponent<CanvasGroup>();
                group.alpha = .55f;
            }
        }
    }

    private void RefreshVibration()
    {
        if (vibration == null) return;
        vibration.GetComponent<Image>().color = ClientSettings.Vibration ? new Color(1, .7f, .1f) : new Color(.45f, .45f, .5f);
        var handle = vibration.GetChild(0) as RectTransform;
        var position = handle.anchoredPosition;
        position.x = ClientSettings.Vibration ? Mathf.Abs(position.x) : -Mathf.Abs(position.x);
        handle.anchoredPosition = position;
    }
    public void OnClickLanguageSetting()
    {
        KOBManager.Popup.OpenPopup<Popup_Setting_Language>().ReturnToSettings = this;
    }


}
