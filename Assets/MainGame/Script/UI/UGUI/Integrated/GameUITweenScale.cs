using UnityEngine;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUITweenScale : GameUITween
    {
        public Vector3 from = Vector3.one, to = Vector3.one;
        public Vector3 value { get => transform.localScale; set => transform.localScale = value; }
        protected override void Apply(float factor) { value = Vector3.LerpUnclamped(from, to, factor); }
        public override void SetStartToCurrentValue() { from = value; }
        public override void SetEndToCurrentValue() { to = value; }
        public static GameUITweenScale Begin(GameObject target, float seconds, Vector3 end)
        {
            var tween = Begin<GameUITweenScale>(target, seconds); tween.from = tween.value; tween.to = end;
            if (seconds <= 0) { tween.Sample(1, true); tween.enabled = false; }
            return tween;
        }
    }
}
