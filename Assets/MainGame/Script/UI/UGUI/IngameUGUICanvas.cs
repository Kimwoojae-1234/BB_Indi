using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BaseBall.BallPlay
{
    public class IngameUGUICanvas : MonoBehaviour
    {
        public Canvas displayCanvas;
        public CanvasGroup opacity;
        private readonly List<Material> materials = new List<Material>();
        private int currentQueue = -1;
        private GameObject ownedInput;
        private Transform inputRoot;

        public void SetInputRoot(Transform root)
        {
            inputRoot = root;
            if (ownedInput != null && root != null) ownedInput.transform.SetParent(root, false);
        }

        protected virtual void OnEnable()
        {
            if (!Application.isPlaying) return;
            if (ownedInput != null)
            {
                ownedInput.SetActive(EventSystem.current == null || EventSystem.current.gameObject == ownedInput);
                return;
            }
            if (EventSystem.current != null) return;
            // Share input across migrated controls for the lifetime of the match UI.
            var input = new GameObject("Ingame UGUI EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            input.transform.SetParent(inputRoot != null ? inputRoot : transform, false);
            input.GetComponent<EventSystem>().sendNavigationEvents = false;
            ownedInput = input;
        }

        public void SetInheritedRendering(float alpha, int queue, int order, Camera camera, bool allowInput)
        {
            displayCanvas.worldCamera = camera;
            displayCanvas.sortingOrder = order;
            opacity.alpha = alpha;
            opacity.blocksRaycasts = opacity.interactable = allowInput && alpha > .001f;
            if (materials.Count == 0)
            {
                var copies = new Dictionary<Material, Material>();
                foreach (var graphic in GetComponentsInChildren<Graphic>(true))
                {
                    var original = graphic is TMP_Text label ? label.fontSharedMaterial : graphic.material;
                    if (!copies.TryGetValue(original, out var copy))
                    {
                        copy = new Material(original) { name = original.name + " (Ingame UGUI)", hideFlags = HideFlags.DontSave };
                        copies.Add(original, copy);
                        materials.Add(copy);
                    }
                    if (graphic is TMP_Text text) text.fontSharedMaterial = copy;
                    else graphic.material = copy;
                }
            }
            if (currentQueue == queue) return;
            foreach (var material in materials) material.renderQueue = queue;
            currentQueue = queue;
        }

        protected virtual void OnDestroy()
        {
            foreach (var material in materials)
                if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
            materials.Clear();
        }
    }
}
