using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Separate from PrototypeBallers.Portrait, which intentionally supplies full-body card art.
// Glow, border and Baller star are baked into each PNG; no extra UI effects are required.
public static class BallerFacePortraits
{
    private static readonly Dictionary<int, Sprite> cache = new Dictionary<int, Sprite>();
    public static string ResourcePath(int characterId) => "BallerPortrait/Face/baller" + characterId;

    public static Sprite GetSprite(int characterId)
    {
        if (cache.TryGetValue(characterId, out var sprite) && sprite != null) return sprite;
        sprite = Resources.Load<Sprite>(ResourcePath(characterId));
        if (sprite != null) cache[characterId] = sprite;
        return sprite;
    }

    // For future NGUI UITexture / RawImage consumers; texture covers the entire square badge.
    public static Texture2D GetTexture(int characterId) => GetSprite(characterId)?.texture;

    public static bool Apply(Image target, int characterId, bool fitParent = false)
    {
        var sprite = GetSprite(characterId);
        if (target == null || sprite == null) return false;
        target.sprite = sprite;
        target.preserveAspect = true;
        target.color = Color.white;
        if (fitParent)
        {
            var rect = target.rectTransform;
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        return true;
    }
}
