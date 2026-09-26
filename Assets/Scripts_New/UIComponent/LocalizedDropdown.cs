using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LocalizedDropdown : MonoBehaviour
{
    public string[] keys;
    public TMP_Dropdown tmp;
    public Dropdown legacy;
    private void OnEnable() { L10n.LanguageChanged += Refresh; Refresh(); }
    private void OnDisable() { L10n.LanguageChanged -= Refresh; }
    public void Refresh()
    {
        if (keys == null) return;
        for (int i = 0; i < keys.Length; i++)
        {
            if (string.IsNullOrEmpty(keys[i])) continue;
            if (tmp != null && i < tmp.options.Count) tmp.options[i].text = L10n.T(keys[i]);
            if (legacy != null && i < legacy.options.Count) legacy.options[i].text = L10n.T(keys[i]);
        }
        if (tmp != null) tmp.RefreshShownValue();
        if (legacy != null) legacy.RefreshShownValue();
    }
}
