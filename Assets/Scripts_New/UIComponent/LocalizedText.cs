using BaseBall.BallPlay.UGUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Automatic translation for static authored labels only. Code-owned text must not use this component.</summary>
[DisallowMultipleComponent]
public sealed class LocalizedText : MonoBehaviour
{
    public string key;
    [TextArea] public string sourceText;
    public TMP_Text tmp;
    public Text legacy;
    public GameUIElement element;
    public TextMesh mesh;
    public tk2dTextMesh spriteText;
    private string lastApplied;

    private void OnEnable() { L10n.LanguageChanged += Refresh; Refresh(); }
    private void OnDisable() { L10n.LanguageChanged -= Refresh; }
    private string CurrentText => element != null ? element.text : tmp != null ? tmp.text : legacy != null ? legacy.text : mesh != null ? mesh.text : spriteText != null ? spriteText.text : null;

    public void Refresh()
    {
        // Do not replace data assigned by a controller after the authored initial label.
        if (lastApplied != null && CurrentText != lastApplied) return;
        if (lastApplied == null && !string.IsNullOrEmpty(sourceText) && CurrentText != sourceText) return;
        Apply();
    }

    public void Apply()
    {
        if (string.IsNullOrEmpty(key)) return;
        lastApplied = L10n.T(key);
        if (element != null) element.text = lastApplied;
        else if (tmp != null) tmp.text = lastApplied;
        else if (legacy != null) legacy.text = lastApplied;
        else if (mesh != null) mesh.text = lastApplied;
        else if (spriteText != null) { spriteText.text = lastApplied; spriteText.Commit(); }
    }

}
