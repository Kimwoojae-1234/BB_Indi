using BaseBall.BallPlay.UGUI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaseBall.BallPlay
{
    // Synchronizes the field overlay's canvas with its owning UI panel.
    public sealed class FieldOverlayCanvas : MonoBehaviour
    {
        public Canvas displayCanvas;
        public CanvasGroup opacity;
        private GameUIPanel panel;
        private Camera uiCamera;
        private readonly Dictionary<Material, Material> materials = new Dictionary<Material, Material>();
        private readonly HashSet<Graphic> graphics = new HashSet<Graphic>();
        private int queue = 3000;
        private bool initialized;

        public void Register(Graphic graphic)
        {
            if (!graphics.Add(graphic)) return;
            var original = graphic is TMP_Text label ? label.fontSharedMaterial : graphic.material;
            if (!materials.TryGetValue(original, out var copy))
            {
                copy = new Material(original) { name = original.name + " (Field UGUI)", hideFlags = HideFlags.DontSave, renderQueue = queue };
                materials.Add(original, copy);
            }
            if (graphic is TMP_Text text) text.fontSharedMaterial = copy;
            else graphic.material = copy;
            graphic.raycastTarget = false;
        }

        public void Forget(Graphic graphic) { graphics.Remove(graphic); }

        public void ApplyRendering(float alpha, int renderQueue, int order, Camera camera)
        {
            if (!initialized)
            {
                initialized = true;
                foreach (var graphic in GetComponentsInChildren<Graphic>(true)) Register(graphic);
            }
            displayCanvas.worldCamera = camera;
            if (!displayCanvas.isRootCanvas) displayCanvas.overrideSorting = true;
            displayCanvas.sortingOrder = order;
            opacity.alpha = alpha;
            opacity.blocksRaycasts = opacity.interactable = false;
            if (queue == renderQueue) return;
            queue = renderQueue;
            foreach (var material in materials.Values) material.renderQueue = queue;
        }

        private void LateUpdate()
        {
            if (panel == null) panel = GetComponentInParent<GameUIPanel>();
            if (uiCamera == null) uiCamera = GameUIRoot.FindCameraForLayer(gameObject.layer);
            ApplyRendering(panel != null ? panel.CalculateFinalAlpha(Time.frameCount) : 1,
                panel != null ? panel.startingRenderQueue : 3000, panel != null ? panel.sortingOrder : 0, uiCamera);
        }

        private void OnDestroy()
        {
            foreach (var material in materials.Values)
                if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
            materials.Clear();
            graphics.Clear();
        }
    }
}
