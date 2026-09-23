using UnityEngine;

namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIPanel : MonoBehaviour
    {
        public float mAlpha = 1;
        public int mDepth;
        public int orderBase;
        public CanvasGroup opacity;
        public float alpha { get => mAlpha; set { mAlpha = Mathf.Clamp01(value); Apply(); } }
        public int depth { get => mDepth; set { mDepth = value; } }
        public Transform clipAnchor;
        public Transform ClipTransform => clipAnchor != null ? clipAnchor : transform;
        public Vector4 clipRegion;
        public Vector2 clipSoftness;
        public Texture2D clipTexture;
        public int clipping;
        private int appliedDepth;
        public int Order => GameUIRenderOrder.Get(this, 0);
        public int startingRenderQueue => 3000;
        public int sortingOrder => Order;
        public float CalculateFinalAlpha(int frame) => mAlpha * GameUIElement.InheritedAlpha(transform.parent);
        private void OnEnable() { GameUIRenderOrder.Register(this); Apply(); }
        private void OnDisable() { GameUIRenderOrder.Remove(this); }
        private void LateUpdate() { Apply(); }
        private void Apply()
        {
            // Graphics and existing migrated canvases consume the final alpha exactly once.
            if (opacity != null) opacity.alpha = 1;
            if (appliedDepth != mDepth) { appliedDepth = mDepth; GameUIRenderOrder.Invalidate(); }
        }
    }
}
