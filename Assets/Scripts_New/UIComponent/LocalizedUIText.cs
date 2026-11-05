using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LocalizedUIText : MonoBehaviour
{
    [SerializeField] private string key;
    private TMP_Text tmp;
    private Text legacy;
    private object[] args;
    private string last;
    private bool rendered;
    private bool initialized;
    private string observedText;
    // Serialized so runtime clones retain the authored English style even when cloned in Korean/Japanese.
    [SerializeField, HideInInspector] private TMP_FontAsset originalFont;
    [SerializeField, HideInInspector] private Material originalMaterial;
    [SerializeField, HideInInspector] private Font originalLegacyFont;
    [SerializeField, HideInInspector] private float originalSize, originalMin, originalMax;
    [SerializeField, HideInInspector] private bool originalAutoSize, usingCjk;
    [SerializeField, HideInInspector] private FontStyles originalStyle;
    [SerializeField, HideInInspector] private FontStyle originalLegacyStyle;
    private void Awake()
    {
        if (initialized) return;
        initialized = true;
        tmp = GetComponent<TMP_Text>(); legacy = GetComponent<Text>();
        if (tmp != null && !usingCjk)
        {
            originalFont = tmp.font;
            originalMaterial = tmp.fontSharedMaterial;
            originalSize = tmp.fontSize;
            originalMin = tmp.fontSizeMin;
            originalMax = tmp.fontSizeMax;
            originalAutoSize = tmp.enableAutoSizing;
            originalStyle = tmp.fontStyle;
        }
        if (legacy != null && !usingCjk) { originalLegacyFont = legacy.font; originalLegacyStyle = legacy.fontStyle; }
    }
    private void OnEnable() { UILocalization.Changed += Refresh; Refresh(); }
    private void OnDisable() { UILocalization.Changed -= Refresh; }
    public void Bind(string value, params object[] values)
    {
        key = value; args = values; rendered = false; Refresh();
    }
    public void Refresh()
    {
        Awake();
        if (string.IsNullOrEmpty(key)) { ApplyFont(); return; }
        string current = tmp != null ? tmp.text : legacy != null ? legacy.text : "";
        // Preserve server/player data that has overwritten a prefab placeholder.
        if (rendered && current != last) { key = null; ApplyFont(); return; }
        last = args != null && args.Length > 0 ? UILocalization.Format(key, args) : UILocalization.Get(key);
        if (tmp != null) tmp.text = last;
        else if (legacy != null) legacy.text = last;
        rendered = true;
        ApplyFont();
    }

    private void LateUpdate()
    {
        // Scores and counters are assigned directly by existing UI code, after OnEnable.
        string current = tmp != null ? tmp.text : legacy != null ? legacy.text : "";
        if (current != observedText) ApplyFont();
    }

    public static bool IsNumericDisplay(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        bool tag = false;
        foreach (char c in value)
        {
            if (c == '<') { tag = true; continue; }
            if (tag) { if (c == '>') tag = false; continue; }
            if (char.IsDigit(c) || char.IsWhiteSpace(c) || ",.+-−/:%％()×".IndexOf(c) >= 0) continue;
            return false;
        }
        return true;
    }

    private void ApplyFont()
    {
        observedText = tmp != null ? tmp.text : legacy != null ? legacy.text : "";
        bool cjk = (UILocalization.Language != "en" || key == "language.ko" || key == "language.ja") && !IsNumericDisplay(observedText);
        if (cjk == usingCjk) return;
        if (tmp != null)
        {
            if (cjk)
            {
                originalFont = tmp.font; originalMaterial = tmp.fontSharedMaterial;
                originalSize = tmp.fontSize; originalMin = tmp.fontSizeMin; originalMax = tmp.fontSizeMax;
                originalAutoSize = tmp.enableAutoSizing;
                originalStyle = tmp.fontStyle;
            }
            var targetFont = cjk ? UILocalization.Font : originalFont;
            if (targetFont != null && tmp.font != targetFont)
            {
                tmp.font = targetFont;
                tmp.fontSharedMaterial = cjk ? UILocalization.CjkMaterial(originalFont, originalMaterial) : originalMaterial;
            }
            if (cjk)
            {
                tmp.fontStyle = originalStyle | FontStyles.Bold;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMax = originalAutoSize ? originalMax : originalSize;
                tmp.fontSizeMin = Mathf.Min(originalSize, Mathf.Max(12, originalSize * .55f));
            }
            else
            {
                tmp.fontStyle = originalStyle;
                tmp.enableAutoSizing = originalAutoSize;
                tmp.fontSize = originalSize;
                tmp.fontSizeMin = originalMin;
                tmp.fontSizeMax = originalMax;
            }
        }
        if (legacy != null)
        {
            if (cjk) { originalLegacyFont = legacy.font; originalLegacyStyle = legacy.fontStyle; }
            legacy.font = cjk ? Resources.Load<Font>(UILocalization.CjkFontResource) : originalLegacyFont;
            legacy.fontStyle = cjk ? (originalLegacyStyle == FontStyle.Italic || originalLegacyStyle == FontStyle.BoldAndItalic ? FontStyle.BoldAndItalic : FontStyle.Bold) : originalLegacyStyle;
        }
        usingCjk = cjk;
    }
}
