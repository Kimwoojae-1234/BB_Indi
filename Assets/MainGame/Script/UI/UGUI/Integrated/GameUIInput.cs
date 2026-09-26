using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIInput : MonoBehaviour
    {
        public InputField input;
        public GameUIElement label;
        public string savedAs;
        public List<GameUIAction> onSubmit = new List<GameUIAction>(), onChange = new List<GameUIAction>();
        public string value { get => input.text; set => input.text = value; }
        void Awake()
        {
            if (!string.IsNullOrEmpty(savedAs) && PlayerPrefs.HasKey(savedAs)) input.SetTextWithoutNotify(PlayerPrefs.GetString(savedAs));
            input.onValueChanged.AddListener(text => { label.text = text; GameUIAction.InvokeAll(onChange); });
            input.onEndEdit.AddListener(text => { if (!string.IsNullOrEmpty(savedAs)) PlayerPrefs.SetString(savedAs, text); GameUIAction.InvokeAll(onSubmit); });
            label.text = input.text;
        }
    }
}
