using System.Collections.Generic;
using UnityEngine;

namespace BaseBall.BallPlay.UGUI
{
    public abstract class GameUITween : MonoBehaviour
    {
        public enum Method { Linear, EaseIn, EaseOut, EaseInOut, BounceIn, BounceOut }
        public enum Style { Once, Loop, PingPong }
        public Method method;
        public Style style;
        public AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public bool ignoreTimeScale = true, steeperCurves;
        public float delay, duration = 1;
        public int tweenGroup;
        public List<GameUIAction> onFinished = new List<GameUIAction>();
        private float startTime, factor, direction = 1;
        private bool started;
        public float tweenFactor { get => factor; set => factor = Mathf.Clamp01(value); }
        protected virtual void OnDisable() { started = false; }
        private void Update()
        {
            float now = ignoreTimeScale ? Time.unscaledTime : Time.time;
            float delta = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            if (!started) { started = true; startTime = now + delay; delta = 0; }
            if (now < startTime) return;
            factor += duration == 0 ? 1 : direction * delta / Mathf.Abs(duration);
            if (style == Style.Loop) { if (factor > 1) factor -= Mathf.Floor(factor); }
            else if (style == Style.PingPong)
            {
                if (factor > 1) { factor = 1 - (factor - Mathf.Floor(factor)); direction = -direction; }
                else if (factor < 0) { factor = -factor; factor -= Mathf.Floor(factor); direction = -direction; }
            }
            if (style == Style.Once && (duration == 0 || factor > 1 || factor < 0))
            {
                factor = Mathf.Clamp01(factor); Sample(factor, true); enabled = false;
                GameUIAction.InvokeAll(onFinished); return;
            }
            Sample(factor, false);
        }
        public void PlayForward() { direction = 1; enabled = true; Update(); }
        public void PlayReverse() { direction = -1; enabled = true; Update(); }
        public void Play() { Play(true); }
        public void Play(bool forward) { if (forward) PlayForward(); else PlayReverse(); }
        public void ResetToBeginning() { factor = direction > 0 ? 0 : 1; started = false; Sample(factor, false); }
        public void Toggle() { direction = factor > 0 ? -direction : 1; enabled = true; }
        public void Sample(float value, bool finished)
        {
            value = Mathf.Clamp01(value);
            switch (method)
            {
                case Method.EaseIn: value = 1 - Mathf.Sin((1 - value) * Mathf.PI * .5f); if (steeperCurves) value *= value; break;
                case Method.EaseOut: value = Mathf.Sin(value * Mathf.PI * .5f); if (steeperCurves) value = 1 - (1 - value) * (1 - value); break;
                case Method.EaseInOut:
                    value -= Mathf.Sin(value * Mathf.PI * 2) / (Mathf.PI * 2);
                    if (steeperCurves) { float signed = value * 2 - 1; float inverse = 1 - Mathf.Abs(signed); value = Mathf.Sign(signed) * (1 - inverse * inverse) * .5f + .5f; }
                    break;
                case Method.BounceIn: value = Bounce(value); break;
                case Method.BounceOut: value = 1 - Bounce(1 - value); break;
            }
            Apply(animationCurve != null ? animationCurve.Evaluate(value) : value);
        }
        private static float Bounce(float t)
        {
            if (t < .363636f) return 7.5685f * t * t;
            if (t < .727272f) { t -= .545454f; return 7.5625f * t * t + .75f; }
            if (t < .909090f) { t -= .818181f; return 7.5625f * t * t + .9375f; }
            t -= .9545454f; return 7.5625f * t * t + .984375f;
        }
        protected abstract void Apply(float value);
        public abstract void SetStartToCurrentValue();
        public abstract void SetEndToCurrentValue();
        protected static T Begin<T>(GameObject target, float seconds) where T : GameUITween
        {
            T tween = null;
            foreach (var candidate in target.GetComponents<T>()) if (candidate.tweenGroup == 0) { tween = candidate; break; }
            if (tween == null) tween = target.AddComponent<T>();
            tween.duration = seconds; tween.style = Style.Once;
            tween.animationCurve = new AnimationCurve(new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0));
            tween.factor = 0; tween.direction = 1; tween.started = false; tween.enabled = true;
            return tween;
        }
    }
}
