using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    [DefaultExecutionOrder(90)]
    public sealed class GameUIProgress : MonoBehaviour
    {
        public Slider slider;
        public GameUIElement foreground;
        public RectTransform fillArea;
        public int steps;
        public List<GameUIAction> onChange = new List<GameUIAction>();
        public float value { get => slider.value; set => slider.value = Quantize(value); }
        float Quantize(float value) => steps > 1 ? Mathf.Round(Mathf.Clamp01(value) * (steps - 1)) / (steps - 1) : Mathf.Clamp01(value);
        void Awake() { slider.onValueChanged.AddListener(Changed); }
        void OnEnable() { if (slider != null) slider.enabled = true; }
        void OnDisable() { if (slider != null) slider.enabled = false; }
        void Changed(float value)
        {
            slider.SetValueWithoutNotify(Quantize(value));
            GameUIAction.InvokeAll(onChange);
        }
        void LateUpdate()
        {
            if (foreground == null || fillArea == null) return;
            fillArea.sizeDelta = new Vector2(foreground.width, foreground.height);
        }
        public void Press(PointerEventData data) { if (slider.interactable) slider.OnPointerDown(data); }
        public void Drag(PointerEventData data) { if (slider.interactable) slider.OnDrag(data); }
    }
}
