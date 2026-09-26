using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BaseBall.BallPlay.UGUI
{
    // A sorting boundary owns its mask chain; an ancestor mask cannot cross overrideSorting.
    // GameUIElement applies the chain after ScrollRect's LateUpdate and before capture.
    public sealed class GameUIClip : MonoBehaviour
    {
        public GameUIPanel[] panels;
        public RectTransform[] masks;
        public RectTransform presentation;
        public Transform source;
        private readonly List<GameUIPanel> ancestors = new List<GameUIPanel>();
        private readonly List<GameUIPanel> clipped = new List<GameUIPanel>();
        private Binding[] bindings;
        private sealed class Binding
        {
            public RectTransform rect;
            public RectMask2D rectangle;
            public RawImage image;
            public Mask texture;
        }
        public void Apply()
        {
            if (source == null || presentation == null) return;
            source.GetComponentsInParent(true, ancestors);
            clipped.Clear();
            for (int i = ancestors.Count - 1; i >= 0; i--)
                if (ancestors[i].clipping == 1 || ancestors[i].clipping == 3) clipped.Add(ancestors[i]);
            bool changed = panels == null || masks == null || panels.Length != clipped.Count || masks.Length != clipped.Count;
            if (!changed)
                for (int i = 0; i < panels.Length; i++)
                    if (panels[i] != clipped[i] || masks[i] == null) { changed = true; break; }
            if (changed) Rebind();
            if (bindings == null)
            {
                bindings = new Binding[masks.Length];
                for (int i = 0; i < masks.Length; i++)
                    bindings[i] = new Binding { rect = masks[i], rectangle = masks[i].GetComponent<RectMask2D>(), image = masks[i].GetComponent<RawImage>(), texture = masks[i].GetComponent<Mask>() };
            }
            for (int i = 0; i < panels.Length; i++)
            {
                var panel = panels[i]; var binding = bindings[i]; var rect = binding.rect; var clip = panel.clipRegion;
                Match(rect, panel.ClipTransform);
                rect.position = panel.ClipTransform.TransformPoint(new Vector3(clip.x, clip.y, 0));
                rect.sizeDelta = new Vector2(Mathf.Max(0, clip.z), Mathf.Max(0, clip.w));
                bool textured = panel.clipping == 1 && panel.clipTexture != null;
                if (textured)
                {
                    if (binding.image == null) binding.image = rect.gameObject.AddComponent<RawImage>();
                    if (binding.texture == null) binding.texture = rect.gameObject.AddComponent<Mask>();
                    binding.image.raycastTarget = false;
                    binding.image.texture = panel.clipTexture;
                    binding.texture.showMaskGraphic = false;
                }
                else
                {
                    if (binding.rectangle == null) binding.rectangle = rect.gameObject.AddComponent<RectMask2D>();
                    binding.rectangle.softness = Vector2Int.Max(Vector2Int.zero, Vector2Int.RoundToInt(panel.clipSoftness));
                }
                if (binding.rectangle != null) binding.rectangle.enabled = !textured;
                if (binding.image != null) binding.image.enabled = textured;
                if (binding.texture != null) binding.texture.enabled = textured;
            }
            Match(presentation, source);
        }
        private void Rebind()
        {
            // Keep the existing Graphic and its references. Only generated mask containers change.
            presentation.SetParent(transform, false);
            if (masks != null)
                foreach (var mask in masks)
                {
                    if (mask == null) continue;
                    mask.gameObject.SetActive(false);
                    if (Application.isPlaying) Destroy(mask.gameObject); else DestroyImmediate(mask.gameObject);
                }
            panels = clipped.ToArray(); masks = new RectTransform[panels.Length];
            Transform parent = transform;
            for (int i = 0; i < panels.Length; i++)
            {
                var rect = (RectTransform)new GameObject("Clip " + panels[i].name, typeof(RectTransform)).transform;
                rect.gameObject.layer = gameObject.layer; rect.SetParent(parent, false);
                masks[i] = rect; parent = rect;
            }
            presentation.SetParent(parent, false); bindings = null;
        }
        private static void Match(RectTransform target, Transform origin)
        {
            target.SetPositionAndRotation(origin.position, origin.rotation);
            var parentScale = target.parent.lossyScale; var scale = origin.lossyScale;
            target.localScale = new Vector3(Divide(scale.x, parentScale.x), Divide(scale.y, parentScale.y), Divide(scale.z, parentScale.z));
        }
        private static float Divide(float a, float b) => Mathf.Abs(b) > .000001f ? a / b : 1;
    }
}
