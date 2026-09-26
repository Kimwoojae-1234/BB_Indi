using UnityEngine;

namespace BaseBall.BallPlay.UGUI
{
    // Only contiguous, equal-depth siblings share a canvas. Keeping it beside their
    // layout owners preserves the exact local TRS, including rotated/nonuniform parents.
    [DefaultExecutionOrder(100)]
    public sealed class GameUICanvasBatch : MonoBehaviour
    {
        public Canvas displayCanvas;
        public Transform sourceParent;
        public GameUIPanel panel;
        public int depth;
        public GameUIElement[] members;

        public void UpdatePresentation(GameUIElement element)
        {
            if (element.transform.parent != sourceParent || (element.isActiveAndEnabled && element.Panel != panel) || element.depth != depth || element.gameObject.layer != gameObject.layer)
            {
                Release();
                return;
            }
            var presentation = element.presentationRoot;
            presentation.localPosition = element.transform.localPosition;
            presentation.localRotation = element.transform.localRotation;
            presentation.localScale = element.transform.localScale;
            presentation.gameObject.SetActive(element.isActiveAndEnabled);
        }

        public void Apply()
        {
            if (!enabled) return;
            // Keep the shared coordinate frame at the parent's origin even when the
            // parent RectTransform's pivot changes at runtime or in the layout editor.
            var rect = (RectTransform)transform;
            rect.anchorMin = rect.anchorMax = sourceParent is RectTransform parentRect ? parentRect.pivot : Vector2.one * .5f;
            rect.localPosition = Vector3.zero; rect.localRotation = Quaternion.identity; rect.localScale = Vector3.one;
            int first = int.MaxValue, last = int.MinValue, count = 0;
            foreach (var element in members)
            {
                if (element == null || element.canvasBatch != this || !element.isActiveAndEnabled) continue;
                int order = GameUIRenderOrder.Get(element);
                first = Mathf.Min(first, order); last = Mathf.Max(last, order); count++;
            }
            // A dynamic widget inserted between the members must retain its own draw order.
            // Fall back for the entire group rather than painting across that boundary.
            if (count > 1 && last - first != count - 1) { Release(); return; }
            displayCanvas.enabled = count != 0;
            if (count != 0) displayCanvas.sortingOrder = first;
            if (displayCanvas.worldCamera == null) displayCanvas.worldCamera = GameUIRoot.FindCameraForLayer(gameObject.layer);
        }

        private void LateUpdate() { Apply(); }

        public void Synchronize()
        {
            if (!enabled) return;
            foreach (var element in members)
                if (element != null && element.canvasBatch == this) element.Apply();
            Apply();
        }

        public void Release()
        {
            foreach (var element in members)
            {
                if (element == null || element.canvasBatch != this) continue;
                var rect = element.presentationRoot;
                rect.SetParent(element.transform, false);
                rect.anchorMin = rect.anchorMax = element.layoutRect != null ? element.layoutRect.pivot : Vector2.one * .5f;
                rect.localPosition = Vector3.zero; rect.localRotation = Quaternion.identity; rect.localScale = Vector3.one;
                rect.gameObject.SetActive(true);
                var canvas = rect.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace; canvas.overrideSorting = true;
                canvas.worldCamera = displayCanvas.worldCamera;
                canvas.sortingLayerID = displayCanvas.sortingLayerID;
                canvas.additionalShaderChannels = displayCanvas.additionalShaderChannels;
                element.displayCanvas = canvas; element.canvasBatch = null;
                canvas.sortingOrder = GameUIRenderOrder.Get(element);
            }
            displayCanvas.enabled = false;
            enabled = false;
            GameUIRenderOrder.Invalidate();
        }
    }
}
