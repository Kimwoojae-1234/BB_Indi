using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting_Language : UIPopup
{
    public Popup_Setting ReturnToSettings { private get; set; }
    public override void Close()
    {
        if (ReturnToSettings == null) { base.Close(); return; }
        gameObject.SetActive(false);
        OnClose();
        ReturnToSettings.Open();
    }
    private bool built;
    public override void Open()
    {
        base.Open();
        if (built) return;
        built = true;
        var panel = transform.Find("Popup");
        var panelRect = (RectTransform)panel;
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(.5f, .5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(1100, 650);
        panel.Find("Flags").gameObject.SetActive(false);
        var glow = panel.Find("BackGlow");
        if (glow != null) glow.gameObject.SetActive(false);
        var group = new GameObject("Languages", typeof(RectTransform)).GetComponent<RectTransform>();
        group.SetParent(panel, false);
        group.anchorMin = group.anchorMax = new Vector2(.5f, .5f);
        group.sizeDelta = new Vector2(720, 330);
        string[] languages = { "en", "ko", "ja" };
        for (int i = 0; i < languages.Length; i++)
        {
            string code = languages[i];
            var row = new GameObject("Language_" + code, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)row.transform;
            rect.SetParent(group, false);
            rect.sizeDelta = new Vector2(650, 85);
            rect.anchoredPosition = new Vector2(0, 100 - i * 105);
            row.GetComponent<Image>().color = new Color(.02f, .35f, .58f);
            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(rect, false);
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            label.fontSize = 38; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            var title = Resources.Load<GameObject>("UI/Popup/Popup_Setting_Language").transform.Find("Popup/Text_Title").GetComponent<TMP_Text>();
            // Match the existing English UI style for newly created labels.
            label.font = title.font;
            label.fontSharedMaterial = title.fontSharedMaterial;
            UILocalization.Set(label, "language." + code);
            row.GetComponent<Button>().onClick.AddListener(() => { UILocalization.SetLanguage(code); Close(); });
        }
    }
}
