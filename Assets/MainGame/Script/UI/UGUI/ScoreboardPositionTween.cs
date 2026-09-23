using UnityEngine;

namespace BaseBall.BallPlay
{
    /// <summary>Position-only animation for the migrated board and its attack indicators.</summary>
    public sealed class ScoreboardPositionTween : MonoBehaviour
    {
        public Vector3 from;
        public Vector3 to;
        public float duration = 0.5f;
        public bool pingPong;
        public bool ignoreTimeScale = true;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        private float elapsed;

        public void PlayFromStart()
        {
            elapsed = 0;
            transform.localPosition = from;
            enabled = true;
        }

        public void StopAt(Vector3 position)
        {
            enabled = false;
            transform.localPosition = position;
        }

        private void Update()
        {
            elapsed += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            float progress = duration <= 0 ? 1 : elapsed / duration;
            float factor = pingPong ? Mathf.PingPong(progress, 1) : Mathf.Clamp01(progress);
            transform.localPosition = Vector3.LerpUnclamped(from, to, curve.Evaluate(factor));
            if (!pingPong && progress >= 1) enabled = false;
        }
    }
}
