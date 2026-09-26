using UnityEngine;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUITweenColor : GameUITween
    {
        public Color from = Color.white, to = Color.white;
        public Color value
        {
            get
            {
                if (TryGetComponent<GameUIElement>(out var e)) return e.color;
                if (TryGetComponent<Graphic>(out var g)) return g.color;
                if (TryGetComponent<SpriteRenderer>(out var s)) return s.color;
                if (TryGetComponent<Light>(out var l)) return l.color;
                if (TryGetComponent<Renderer>(out var r)) return r.material.color;
                return Color.white;
            }
            set
            {
                if (TryGetComponent<GameUIElement>(out var e)) e.color = value;
                else if (TryGetComponent<Graphic>(out var g)) g.color = value;
                else if (TryGetComponent<SpriteRenderer>(out var s)) s.color = value;
                else if (TryGetComponent<Light>(out var l)) l.color = value;
                else if (TryGetComponent<Renderer>(out var r)) r.material.color = value;
            }
        }
        protected override void Apply(float factor) { value = Color.Lerp(from, to, factor); }
        public override void SetStartToCurrentValue() { from = value; }
        public override void SetEndToCurrentValue() { to = value; }
        public static GameUITweenColor Begin(GameObject target, float seconds, Color color)
        {
            var tween = Begin<GameUITweenColor>(target, seconds); tween.from = tween.value; tween.to = color;
            if (seconds <= 0) { tween.value = color; tween.enabled = false; }
            return tween;
        }
    }
}
