using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, ISubmitHandler
{
    public enum ClickSound { Auto, Confirm, Back }
    [Tooltip("Uncheck to mute only this button.")]
    public bool soundEnabled = true;
    public ClickSound sound = ClickSound.Auto;
    private Button button;
    private bool validPress;
    private void Awake() { button = GetComponent<Button>(); }
    public void OnPointerDown(PointerEventData data)
    {
        validPress = data.button == PointerEventData.InputButton.Left && button.IsInteractable();
    }
    public void OnPointerClick(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left) return;
        if (validPress || button.IsInteractable()) Play();
        validPress = false;
    }
    public void OnSubmit(BaseEventData data) { if (button.IsInteractable()) Play(); }
    private void Play()
    {
        if (!enabled || !soundEnabled) return;
        bool back = sound == ClickSound.Back;
        if (sound == ClickSound.Auto)
        {
            string label = gameObject.name.ToLowerInvariant();
            back = label.Contains("back") || label.Contains("close") || label.Contains("cancel");
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                string method = button.onClick.GetPersistentMethodName(i).ToLowerInvariant();
                back |= method.Contains("back") || method.Contains("close") || method.Contains("cancel");
            }
        }
        FrontAudio.Click(back);
    }

    public static void AttachMissing(GameObject root)
    {
        foreach (var button in root.GetComponentsInChildren<Button>(true))
            if (button.GetComponent<UIButtonAudio>() == null) button.gameObject.AddComponent<UIButtonAudio>();
    }
}
