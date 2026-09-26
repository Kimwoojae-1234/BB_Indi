using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIToggle : MonoBehaviour
    {
        public Toggle toggle;
        public GameUIElement activeSprite;
        public bool invertSpriteState, instantTween;
        public GameUITween tween;
        public Animation activeAnimation;
        public Animator animator;
        public Behaviour[] activate, deactivate;
        public int group;
        public bool optionCanBeNone;
        public List<GameUIAction> onChange = new List<GameUIAction>();
        static readonly List<GameUIToggle> instances = new List<GameUIToggle>();
        public bool value { get => toggle.isOn; set => toggle.isOn = value; }
        void Awake() { toggle.onValueChanged.AddListener(Changed); }
        void OnEnable() { instances.Add(this); Apply(value); }
        void OnDisable() { instances.Remove(this); }
        public void Click()
        {
            if (!isActiveAndEnabled || !toggle.interactable || (group != 0 && value && !optionCanBeNone)) return;
            value = !value;
        }
        void Changed(bool state)
        {
            if (state && group != 0)
                foreach (var other in instances.ToArray()) if (other != this && other.group == group) other.value = false;
            Apply(state); GameUIAction.InvokeAll(onChange);
        }
        void Apply(bool state)
        {
            if (activeSprite != null)
            {
                float alpha = state != invertSpriteState ? 1 : 0;
                if (instantTween) activeSprite.alpha = alpha; else GameUITweenAlpha.Begin(activeSprite.gameObject, .15f, alpha);
            }
            if (activate != null) foreach (var component in activate) if (component != null) component.enabled = state;
            if (deactivate != null) foreach (var component in deactivate) if (component != null) component.enabled = !state;
            if (tween != null) { if (state) tween.PlayForward(); else tween.PlayReverse(); }
            if (activeAnimation != null)
                foreach (AnimationState clip in activeAnimation) { clip.speed = state ? 1 : -1; clip.time = state ? 0 : clip.length; activeAnimation.Play(clip.name); }
            if (animator != null) { animator.speed = state ? 1 : -1; animator.Play(0, 0, state ? 0 : 1); }
        }
    }
}
