using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaseBall.BallPlay.UGUI
{
    /// <summary>Game-facing presentation API backed only by Unity UI graphics.
    /// The editor converter supplies the graphic and catalog; runtime never reads an NGUI asset.</summary>
    // ScrollRect moves content in LateUpdate at the default execution order.
    [DefaultExecutionOrder(50)]
    public sealed class GameUIElement : MonoBehaviour
    {
        public enum ElementKind { Widget, Sprite, Label, Texture }
        public ElementKind kind;
        public Graphic graphic;
        public Canvas displayCanvas;
        public GameUISpriteCatalog sprites;
        public Color mColor = Color.white;
        public int mWidth = 100, mHeight = 100, mDepth, mPivot = 4;
        public string mSpriteName = "", mText = "";
        public int mType, mFillDirection = 4, mFlip, mFontSize = 16;
        public float mFillAmount = 1;
        public bool mInvert, mEncoding = true;
        public Texture mTexture;
        public Rect mRect = new Rect(0, 0, 1, 1);
        public Material mMat;
        public Graphic[] shadows = Array.Empty<Graphic>();
        public Vector2[] shadowOffsets = Array.Empty<Vector2>();
        public Color shadowColor = Color.black;
        [SerializeField] private CanvasGroup widgetOpacity;
        public GameUIClip clipping;
        public RectTransform layoutRect;
        public GameUICanvasBatch canvasBatch;
        public RectTransform presentationRoot;
        private GameUIPanel panel;
        public GameUIPanel Panel => panel != null ? panel : (panel = GetComponentInParent<GameUIPanel>(true));
        private string appliedText, appliedSprite, shadowText;
        private int appliedDepth = int.MinValue;
        public bool supportEncoding { get => mEncoding; set { mEncoding = value; appliedText = null; Apply(); } }
        public TMP_FontAsset bitmapFont { get => (graphic as TMP_Text)?.font; set { if (graphic is TMP_Text label) label.font = value; } }
        public float CalculateFinalAlpha() => InheritedAlpha(transform);
        public static float InheritedAlpha(Transform parent)
        {
            float alpha = 1;
            for (; parent != null; parent = parent.parent)
            {
                // Native CanvasGroups are applied by CanvasRenderer. Legacy widget/panel
                // alpha is folded into the Graphic once, including both on the same owner.
                if (parent.TryGetComponent<GameUIElement>(out var e)) alpha *= e.mColor.a;
                if (parent.TryGetComponent<GameUIPanel>(out var p)) alpha *= p.mAlpha;
            }
            return alpha;
        }
        private GameUISpriteCatalog.Entry entry;

        public Color color { get => mColor; set { mColor = value; Apply(); } }
        public float alpha { get => mColor.a; set { mColor.a = Mathf.Clamp01(value); Apply(); } }
        public int width { get => layoutRect != null ? Mathf.RoundToInt(layoutRect.rect.width) : mWidth; set { SetDimensions(value, height); } }
        public int height { get => layoutRect != null ? Mathf.RoundToInt(layoutRect.rect.height) : mHeight; set { SetDimensions(width, value); } }
        public int depth { get => mDepth; set { mDepth = value; Apply(); } }
        public string spriteName { get => mSpriteName; set { mSpriteName = value ?? ""; Apply(); } }
        public string text { get => mText; set { mText = value ?? ""; Apply(); } }
        public float fillAmount { get => mFillAmount; set { mFillAmount = Mathf.Clamp01(value); Apply(); } }
        public Texture mainTexture { get => mTexture; set { mTexture = value; Apply(); } }
        public Rect uvRect { get => mRect; set { mRect = value; Apply(); } }
        public int fontSize { get => mFontSize; set { mFontSize = value; Apply(); } }
        public Vector2 pivotOffset => layoutRect != null ? layoutRect.pivot : new Vector2((mPivot % 3) * .5f, 1 - (mPivot / 3) * .5f);

        public void SetDimensions(int w, int h)
        {
            mWidth = w; mHeight = h;
            if (layoutRect != null)
            {
                layoutRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                layoutRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            }
            Apply();
        }
        public void MakePixelPerfect()
        {
            if (kind == ElementKind.Sprite && sprites != null && sprites.TryGet(mSpriteName, out var item))
                SetDimensions(Mathf.RoundToInt(item.size.x), Mathf.RoundToInt(item.size.y));
            else if (kind == ElementKind.Texture && mTexture != null) SetDimensions(mTexture.width, mTexture.height);
        }

        private void OnEnable() { GameUIRenderOrder.Register(this); appliedText = appliedSprite = null; Apply(); }
        private void OnTransformParentChanged() { panel = null; GameUIRenderOrder.Invalidate(); }
        private void OnDisable() { GameUIRenderOrder.Remove(this); if (canvasBatch != null && presentationRoot != null) presentationRoot.gameObject.SetActive(false); if (graphic != null) graphic.enabled = false; foreach (var shadow in shadows) if (shadow != null) shadow.enabled = false; }
        private void OnDestroy()
        {
            // Batched graphics are siblings of their layout owner, so runtime destruction
            // must remove that owned presentation too.
            if (Application.isPlaying && canvasBatch != null && presentationRoot != null) Destroy(presentationRoot.gameObject);
        }
        private void LateUpdate() { Apply(); }
        public void Apply()
        {
            if (layoutRect != null) { mWidth = width; mHeight = height; }
            if (canvasBatch != null) canvasBatch.UpdatePresentation(this);
            if (kind == ElementKind.Widget)
            {
                if (widgetOpacity != null) widgetOpacity.alpha = 1;
                return;
            }
            if (graphic == null) return;
            graphic.enabled = enabled;
            var visibleColor = mColor; visibleColor.a = CalculateFinalAlpha(); graphic.color = visibleColor;
            if (clipping != null) clipping.Apply();
            var rect = graphic.rectTransform;
            rect.pivot = pivotOffset;
            rect.sizeDelta = new Vector2(mWidth, mHeight);
            rect.localPosition = Vector3.zero;
            if (graphic is GameUITextureGraphic textureGraphic)
            {
                textureGraphic.SetTexture(mTexture, mRect);
                textureGraphic.fillAmount = mFillAmount;
            }
            else if (graphic is Image image)
            {
                image.material = sprites != null ? sprites.material : null;
                if (appliedSprite != mSpriteName)
                {
                    appliedSprite = mSpriteName;
                    entry = null;
                    if (sprites != null) sprites.TryGet(mSpriteName, out entry);
                    image.sprite = entry != null ? entry.sprite : null;
                }
                image.enabled = enabled && entry != null;
                image.fillAmount = mFillAmount;
                if (entry != null && mType == 0)
                {
                    var padding = entry.padding;
                    var size = new Vector2(mWidth * (1 - (padding.x + padding.z) / entry.size.x),
                        mHeight * (1 - (padding.y + padding.w) / entry.size.y));
                    rect.sizeDelta = size;
                    rect.localPosition = new Vector3(mWidth * padding.x / entry.size.x + (size.x - mWidth) * rect.pivot.x,
                        mHeight * padding.y / entry.size.y + (size.y - mHeight) * rect.pivot.y, 0);
                }
                bool flipX = mFlip == 1 || mFlip == 3, flipY = mFlip == 2 || mFlip == 3;
                rect.localScale = new Vector3(flipX ? -1 : 1, flipY ? -1 : 1, 1);
                var position = rect.localPosition;
                if (flipX) position.x = mWidth * (1 - 2 * rect.pivot.x) - position.x;
                if (flipY) position.y = mHeight * (1 - 2 * rect.pivot.y) - position.y;
                rect.localPosition = position;
            }
            else if (graphic is TMP_Text label)
            {
                if (appliedText != mText) { appliedText = mText; label.text = mEncoding ? ConvertMarkup(mText) : mText; shadowText = ShadowText(label.text); }
                label.fontSize = mFontSize; label.richText = mEncoding;
                label.verticalAlignment = pivotOffset.y > .5f ? VerticalAlignmentOptions.Top : pivotOffset.y < .5f ? VerticalAlignmentOptions.Bottom : VerticalAlignmentOptions.Geometry;
            }
            else if (graphic is Text legacyText)
            {
                if (appliedText != mText) { appliedText = mText; legacyText.text = mEncoding ? ConvertMarkup(mText) : mText; shadowText = ShadowText(legacyText.text); }
                legacyText.fontSize = mFontSize; legacyText.supportRichText = mEncoding;
                legacyText.alignment = (TextAnchor)((mPivot / 3) * 3 + (int)legacyText.alignment % 3);
            }
            else if (graphic is RawImage raw) { raw.texture = mTexture; raw.uvRect = mRect; }
            for (int i = 0; i < shadows.Length; i++)
            {
                var shadow = shadows[i];
                shadow.enabled = enabled;
                var c = shadowColor; c.a *= visibleColor.a; shadow.color = c;
                shadow.rectTransform.pivot = rect.pivot; shadow.rectTransform.sizeDelta = rect.sizeDelta;
                shadow.rectTransform.localPosition = rect.localPosition + (Vector3)shadowOffsets[i];
                if (shadow is TMP_Text tmpShadow && graphic is TMP_Text tmpMain)
                {
                    tmpShadow.text = shadowText; tmpShadow.fontSize = tmpMain.fontSize; tmpShadow.verticalAlignment = tmpMain.verticalAlignment; tmpShadow.richText = tmpMain.richText;
                }
                else if (shadow is Text textShadow && graphic is Text textMain)
                {
                    textShadow.text = shadowText; textShadow.fontSize = textMain.fontSize; textShadow.alignment = textMain.alignment; textShadow.supportRichText = textMain.supportRichText;
                }
            }
            if (displayCanvas != null)
            {
                if (!displayCanvas.isRootCanvas) displayCanvas.overrideSorting = true;
                if (appliedDepth != mDepth) { appliedDepth = mDepth; GameUIRenderOrder.Invalidate(); }
                if (canvasBatch == null) displayCanvas.sortingOrder = GameUIRenderOrder.Get(this);
                if (displayCanvas.worldCamera == null) displayCanvas.worldCamera = GameUIRoot.FindCameraForLayer(gameObject.layer);
            }
        }

        private string ShadowText(string value) => mEncoding ? Regex.Replace(value, @"</?color(?:=[^>]+)?>", "") : value;

        // NGUI color markup appears in live game strings; retain its nested color semantics.
        public static string ConvertMarkup(string value)
        {
            int colors = 0;
            string result = Regex.Replace(value ?? "", @"\[([0-9A-Fa-f]{6}(?:[0-9A-Fa-f]{2})?|-|/?[bius])\]", match =>
            {
                string token = match.Groups[1].Value;
                if (token == "-") { if (colors == 0) return ""; colors--; return "</color>"; }
                if (token.Length >= 6) { colors++; return "<color=#" + token + ">"; }
                return "<" + token + ">";
            });
            return result + string.Concat(System.Linq.Enumerable.Repeat("</color>", colors));
        }

        public void ConfigureWidgetOpacity(CanvasGroup group) { widgetOpacity = group; }
    }
}
