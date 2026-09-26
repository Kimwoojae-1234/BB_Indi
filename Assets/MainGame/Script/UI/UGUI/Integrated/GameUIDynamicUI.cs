using UnityEngine;

namespace BaseBall.BallPlay.UGUI
{
    // Dynamic prefabs inherit the host canvas/camera and clip panels after instantiation.
    // Root lifecycle callbacks run before their widgets, including inactive child widgets.
    [DefaultExecutionOrder(25)]
    public sealed class GameUIDynamicUI : MonoBehaviour
    {
        private GameUIElement[] elements;
        private GameUICanvasBatch[] batches;
        private Canvas[] canvases;
        private Transform previousParent;
        private GameUIRoot previousRoot;
        private GameUIPanel previousPanel;

        private void OnEnable() { RefreshUGUIHierarchy(); }
        private void OnTransformParentChanged() { RefreshUGUIHierarchy(); }
        private void LateUpdate()
        {
            if (previousParent != transform.parent || previousRoot != GetComponentInParent<GameUIRoot>(true) ||
                previousPanel != GetComponentInParent<GameUIPanel>(true)) RefreshUGUIHierarchy();
            // A host can turn clipping on after loading the prefab.
            foreach (var element in elements) if (element != null && element.clipping == null) EnsureClipping(element);
        }
        public void RefreshUGUIHierarchy()
        {
            if (elements == null)
            {
                elements = GetComponentsInChildren<GameUIElement>(true);
                batches = GetComponentsInChildren<GameUICanvasBatch>(true);
                canvases = GetComponentsInChildren<Canvas>(true);
            }
            previousParent = transform.parent;
            previousRoot = GetComponentInParent<GameUIRoot>(true);
            previousPanel = GetComponentInParent<GameUIPanel>(true);
            foreach (var element in elements)
            {
                if (element == null) continue;
                element.RefreshHierarchy();
                EnsureClipping(element);
            }
            foreach (var batch in batches)
            {
                if (batch == null || !batch.enabled) continue;
                foreach (var element in batch.members)
                    if (element != null && element.canvasBatch == batch) { batch.panel = element.Panel; break; }
            }
            // Include canvases created by a runtime batch release too.
            canvases = GetComponentsInChildren<Canvas>(true);
            foreach (var canvas in canvases)
            {
                var camera = previousRoot != null ? previousRoot.uiCamera : GameUIRoot.FindCameraForLayer(canvas.gameObject.layer);
                if (camera != null) canvas.worldCamera = camera;
                if (canvas.transform.parent != null && canvas.transform.parent.GetComponentInParent<Canvas>(true) != null)
                    canvas.overrideSorting = true;
            }
            GameUIRenderOrder.Invalidate();
        }
        private static void EnsureClipping(GameUIElement element)
        {
            if (element.clipping != null || element.graphic == null) return;
            bool clipped = false;
            for (var parent = element.transform; parent != null; parent = parent.parent)
                if (parent.TryGetComponent<GameUIPanel>(out var panel) && (panel.clipping == 1 || panel.clipping == 3)) { clipped = true; break; }
            if (!clipped) return;
            var container = element.presentationRoot != null ? element.presentationRoot : element.displayCanvas != null ? element.displayCanvas.transform as RectTransform : null;
            if (container == null) return;
            var presentation = (RectTransform)new GameObject("Dynamic clipped presentation",typeof(RectTransform)).transform;
            presentation.gameObject.layer = element.gameObject.layer;
            presentation.SetParent(container,false); presentation.sizeDelta = Vector2.zero;
            element.graphic.transform.SetParent(presentation,false);
            foreach (var shadow in element.shadows) if (shadow != null) shadow.transform.SetParent(presentation,false);
            element.clipping = container.gameObject.AddComponent<GameUIClip>();
            element.clipping.source = element.transform; element.clipping.presentation = presentation;
            element.clipping.Apply();
        }
    }
}
