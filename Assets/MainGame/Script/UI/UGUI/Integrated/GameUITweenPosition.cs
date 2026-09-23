using UnityEngine;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUITweenPosition : GameUITween
    {
        public Vector3 from, to;
        public bool worldSpace;
        public Vector3 value { get => worldSpace ? transform.position : transform.localPosition; set { if (worldSpace) transform.position = value; else transform.localPosition = value; } }
        protected override void Apply(float factor) { value = Vector3.LerpUnclamped(from, to, factor); }
        public override void SetStartToCurrentValue() { from = value; }
        public override void SetEndToCurrentValue() { to = value; }
        public static GameUITweenPosition Begin(GameObject target, float seconds, Vector3 end)
        {
            var tween = Begin<GameUITweenPosition>(target, seconds); tween.from = tween.value; tween.to = end;
            if (seconds <= 0) { tween.Sample(1, true); tween.enabled = false; }
            return tween;
        }
    }
}
