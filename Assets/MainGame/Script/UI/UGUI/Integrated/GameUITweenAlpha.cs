using UnityEngine;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUITweenAlpha : GameUITween
    {
        public float from, to = 1;
        public float value
        {
            get
            {
                if (TryGetComponent<GameUIElement>(out var element)) return element.alpha;
                if (TryGetComponent<GameUIPanel>(out var panel)) return panel.alpha;
                if (TryGetComponent<CanvasGroup>(out var group)) return group.alpha;
                if (TryGetComponent<SpriteRenderer>(out var sprite)) return sprite.color.a;
                if (TryGetComponent<tk2dBaseSprite>(out var tk)) return tk.color.a;
                if (TryGetComponent<Graphic>(out var graphic)) return graphic.color.a;
                if (TryGetComponent<Renderer>(out var renderer)) return renderer.material.color.a;
                var child = GetComponentInChildren<GameUIElement>(); return child != null ? child.alpha : 1;
            }
            set
            {
                if (TryGetComponent<GameUIElement>(out var element)) element.alpha = value;
                else if (TryGetComponent<GameUIPanel>(out var panel)) panel.alpha = value;
                else if (TryGetComponent<CanvasGroup>(out var group)) group.alpha = value;
                else if (TryGetComponent<SpriteRenderer>(out var sprite)) { var c = sprite.color; c.a = value; sprite.color = c; }
                else if (TryGetComponent<tk2dBaseSprite>(out var tk)) { var c = tk.color; c.a = value; tk.color = c; }
                else if (TryGetComponent<Graphic>(out var graphic)) { var c = graphic.color; c.a = value; graphic.color = c; }
                else if (TryGetComponent<Renderer>(out var renderer)) { var c = renderer.material.color; c.a = value; renderer.material.color = c; }
                else { var child = GetComponentInChildren<GameUIElement>(); if (child != null) child.alpha = value; }
            }
        }
        protected override void Apply(float factor) { value = Mathf.Lerp(from, to, factor); }
        public override void SetStartToCurrentValue() { from = value; }
        public override void SetEndToCurrentValue() { to = value; }
        public static GameUITweenAlpha Begin(GameObject target, float seconds, float end)
        {
            var tween = Begin<GameUITweenAlpha>(target, seconds); tween.from = tween.value; tween.to = end;
            if (seconds <= 0) { tween.Sample(1, true); tween.enabled = false; }
            return tween;
        }
    }
}
