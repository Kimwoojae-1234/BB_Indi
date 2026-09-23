using UnityEngine;
using UnityEngine.UI;

namespace BaseBall.BallPlay.UGUI
{
    // Each independently sorted canvas owns its mask chain, so overrideSorting cannot bypass an ancestor mask.
    public sealed class GameUIClip : MonoBehaviour
    {
        public GameUIPanel[] panels;
        public RectTransform[] masks;
        public RectTransform presentation;
        public Transform source;
        public void Apply()
        {
            for (int i = 0; i < panels.Length; i++)
            {
                var panel = panels[i]; var rect = masks[i]; var clip = panel.clipRegion;
                Match(rect, panel.ClipTransform);
                rect.position = panel.ClipTransform.TransformPoint(new Vector3(clip.x, clip.y, 0));
                rect.sizeDelta = new Vector2(clip.z, clip.w);
            }
            Match(presentation, source);
        }
        private static void Match(RectTransform target, Transform origin)
        {
            target.SetPositionAndRotation(origin.position, origin.rotation);
            var parentScale = target.parent.lossyScale; var scale = origin.lossyScale;
            target.localScale = new Vector3(Divide(scale.x, parentScale.x), Divide(scale.y, parentScale.y), Divide(scale.z, parentScale.z));
        }
        private static float Divide(float a, float b) => Mathf.Abs(b) > .000001f ? a / b : 1;
        private void LateUpdate() { Apply(); }
    }
}
