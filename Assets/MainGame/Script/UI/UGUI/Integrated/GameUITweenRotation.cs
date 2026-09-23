using UnityEngine;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUITweenRotation : GameUITween
    {
        public Vector3 from, to;
        public bool quaternionLerp;
        protected override void Apply(float factor) { transform.localRotation = quaternionLerp ? Quaternion.Slerp(Quaternion.Euler(from), Quaternion.Euler(to), factor) : Quaternion.Euler(Vector3.Lerp(from, to, factor)); }
        public override void SetStartToCurrentValue() { from = transform.localEulerAngles; }
        public override void SetEndToCurrentValue() { to = transform.localEulerAngles; }
        public static GameUITweenRotation Begin(GameObject target, float seconds, Quaternion end)
        {
            var tween = Begin<GameUITweenRotation>(target, seconds); tween.from = target.transform.localEulerAngles; tween.to = end.eulerAngles;
            if (seconds <= 0) { tween.Sample(1, true); tween.enabled = false; }
            return tween;
        }
        public static GameUITweenRotation Begin(GameObject target, float seconds, Vector3 end) { return Begin(target, seconds, Quaternion.Euler(end)); }
    }
}
