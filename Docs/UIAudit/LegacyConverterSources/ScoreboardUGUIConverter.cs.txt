#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using MatchScoreboard = BaseBall.BallPlay.UIScoreBoard;

/// <summary>One bounded migration. Rejects unsupported data instead of silently dropping it.</summary>
public static class ScoreboardUGUIConverter
{
    public const string AssetFolder = "Assets/MainGame/UI/ScoreboardUGUI";
    public const string BoardPath = AssetFolder + "/ScoreboardDisplay.prefab";

    [MenuItem("Tools/UI Migration/Apply Scoreboard To Game Prefab")]
    public static void ApplyToGamePrefab()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before applying the prefab.");
        if (!File.ReadAllText(ScoreboardMigrationChecks.OutputPath + "/presentation-checks.txt").StartsWith("PASS"))
            throw new InvalidOperationException("Run presentation checks successfully before applying.");
        var root = PrefabUtility.LoadPrefabContents(ScoreboardMigrationChecks.PrefabPath);
        try
        {
            var owner = root.GetComponentInChildren<MatchScoreboard>(true);
            if (owner.UguiView != null) throw new InvalidOperationException("Already migrated.");
            var oldBoard = owner.board;
            var oldObjects = new HashSet<Object>();
            foreach (var node in oldBoard.GetComponentsInChildren<Transform>(true))
            {
                oldObjects.Add(node.gameObject);
                foreach (var component in node.GetComponents<Component>()) oldObjects.Add(component);
            }
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || behaviour == owner || oldObjects.Contains(behaviour)) continue;
                var properties = new SerializedObject(behaviour).GetIterator();
                while (properties.Next(true))
                    if (properties.propertyType == SerializedPropertyType.ObjectReference && oldObjects.Contains(properties.objectReferenceValue))
                        throw new InvalidOperationException("External board binding requires migration: " + behaviour.name + "/" + properties.propertyPath);
            }
            var replacement = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(BoardPath), oldBoard.transform.parent, false);
            replacement.name = oldBoard.name;
            replacement.transform.SetSiblingIndex(oldBoard.transform.GetSiblingIndex());
            replacement.transform.localPosition = oldBoard.transform.localPosition;
            replacement.transform.localRotation = oldBoard.transform.localRotation;
            replacement.transform.localScale = oldBoard.transform.localScale;
            var view = replacement.GetComponent<IngameScoreboardView>();
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("uguiView").objectReferenceValue = view;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            owner.board = replacement;
            owner.indicator = view.indicator;
            owner.homeLogo = owner.awayLogo = owner.topBottom = null;
            owner.homeName = owner.awayName = owner.homeScore = owner.awayScore = owner.inningInfo = null;
            owner.ballCount = owner.strikeCount = owner.outCount = owner.baseOn = new UISprite[0];
            Object.DestroyImmediate(oldBoard);
            PrefabUtility.SaveAsPrefabAsset(root, ScoreboardMigrationChecks.PrefabPath);
            File.WriteAllText(ScoreboardMigrationChecks.OutputPath + "/apply-status.txt", "APPLIED " + DateTime.UtcNow.ToString("O"));
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    [MenuItem("Tools/UI Migration/Build UGUI Scoreboard Candidate")]
    public static void BuildCandidate()
    {
        try
        {
            Directory.CreateDirectory(AssetFolder);
            AssetDatabase.Refresh();
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath)
                .GetComponentInChildren<MatchScoreboard>(true);
            if (source.UguiView != null) throw new InvalidOperationException("Scoreboard is already migrated.");
            var sprites = BuildSprites(source.homeLogo.atlas);
            var fonts = new Dictionary<UIFont, TMP_FontAsset>();
            foreach (var label in source.board.GetComponentsInChildren<UILabel>(true))
                if (!fonts.ContainsKey(label.bitmapFont)) fonts.Add(label.bitmapFont, BuildFont(label.bitmapFont));

            var objects = new Dictionary<GameObject, GameObject>();
            var root = ConvertNode(source.board.transform, null, sprites, fonts, objects);
            try
            {
                var view = root.AddComponent<IngameScoreboardView>();
                view.displayCanvas = root.AddComponent<Canvas>();
                view.displayCanvas.renderMode = RenderMode.WorldSpace;
                view.opacity = root.AddComponent<CanvasGroup>();
                view.opacity.blocksRaycasts = false;
                view.opacity.interactable = false;
                view.sprites = sprites;
                view.entrance = root.GetComponent<ScoreboardPositionTween>();
                view.homeLogo = objects[source.homeLogo.gameObject].GetComponent<Image>();
                view.awayLogo = objects[source.awayLogo.gameObject].GetComponent<Image>();
                view.homeName = objects[source.homeName.gameObject].GetComponent<TMP_Text>();
                view.awayName = objects[source.awayName.gameObject].GetComponent<TMP_Text>();
                view.homeScore = objects[source.homeScore.gameObject].GetComponent<TMP_Text>();
                view.awayScore = objects[source.awayScore.gameObject].GetComponent<TMP_Text>();
                view.inningInfo = objects[source.inningInfo.gameObject].GetComponent<TMP_Text>();
                view.topBottom = objects[source.topBottom.gameObject].GetComponent<Image>();
                view.ballCount = source.ballCount.Select(item => objects[item.gameObject].GetComponent<Image>()).ToArray();
                view.strikeCount = source.strikeCount.Select(item => objects[item.gameObject].GetComponent<Image>()).ToArray();
                view.outCount = source.outCount.Select(item => objects[item.gameObject].GetComponent<Image>()).ToArray();
                view.baseOn = source.baseOn.Select(item => objects[item.gameObject].GetComponent<Image>()).ToArray();
                view.indicator = source.indicator.Select(item => objects[item]).ToArray();
                PrefabUtility.SaveAsPrefabAsset(root, BoardPath);
                AssetDatabase.SaveAssets();
                File.WriteAllText(ScoreboardMigrationChecks.OutputPath + "/build-status.txt", "BUILT " + BoardPath);
                Debug.Log("UGUI scoreboard candidate built; main game prefab has not been changed.");
            }
            finally { Object.DestroyImmediate(root); }
        }
        catch (Exception exception)
        {
            Directory.CreateDirectory(ScoreboardMigrationChecks.OutputPath);
            File.WriteAllText(ScoreboardMigrationChecks.OutputPath + "/build-status.txt", exception.ToString());
            Debug.LogException(exception);
        }
    }

    public static GameObject ConvertNode(Transform source, Transform parent, ScoreboardSpriteCatalog sprites,
        Dictionary<UIFont, TMP_FontAsset> fonts, Dictionary<GameObject, GameObject> objects, bool presentationOnly = false)
    {
        foreach (var component in source.GetComponents<Component>())
            if (!presentationOnly && !(component is Transform) && !(component is UISprite) && !(component is UILabel) && !(component is TweenPosition))
                throw new InvalidOperationException("Unsupported scoreboard component: " + component);

        var target = new GameObject(source.name, typeof(RectTransform)) { layer = source.gameObject.layer };
        var rect = (RectTransform)target.transform;
        rect.SetParent(parent, false);
        rect.sizeDelta = Vector2.zero;
        objects.Add(source.gameObject, target);
        var widget = source.GetComponent<UIWidget>();
        if (widget != null)
        {
            if (widget.isAnchored) throw new InvalidOperationException("Anchored widget requires explicit conversion: " + widget.name);
            rect.pivot = widget.pivotOffset;
            rect.sizeDelta = new Vector2(widget.width, widget.height);
        }
        rect.localPosition = source.localPosition;
        rect.localRotation = source.localRotation;
        rect.localScale = source.localScale;

        var sprite = source.GetComponent<UISprite>();
        if (sprite != null)
        {
            if (sprite.type != UIBasicSprite.Type.Simple && sprite.type != UIBasicSprite.Type.Sliced)
                throw new InvalidOperationException("Unsupported sprite mode: " + sprite.name);
            var image = target.AddComponent<Image>();
            image.sprite = sprites.Get(sprite.spriteName).sprite;
            image.color = sprite.color;
            image.type = sprite.type == UIBasicSprite.Type.Sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = false;
            // NGUI trims one scaled pixel from the right/top of odd-sized simple
            // sprites. The base diamonds and attack arrows depend on this rule.
            var data = sprite.GetAtlasSprite();
            int nativeWidth = data.width + data.paddingLeft + data.paddingRight;
            int nativeHeight = data.height + data.paddingTop + data.paddingBottom;
            if (sprite.type == UIBasicSprite.Type.Simple && ((nativeWidth & 1) != 0 || (nativeHeight & 1) != 0))
            {
                if (data.hasPadding) throw new InvalidOperationException("Odd padded sprite needs explicit geometry conversion: " + data.name);
                var originalSize = rect.sizeDelta;
                var size = new Vector2(originalSize.x * (nativeWidth - (nativeWidth & 1)) / nativeWidth,
                    originalSize.y * (nativeHeight - (nativeHeight & 1)) / nativeHeight);
                rect.sizeDelta = size;
                rect.localPosition += (Vector3)Vector2.Scale(size - originalSize, rect.pivot);
            }
        }
        var label = source.GetComponent<UILabel>();
        if (label != null && label.bitmapFont == null && presentationOnly)
        {
            if (label.trueTypeFont == null) throw new InvalidOperationException("Missing font: " + label.name);
            var text = target.AddComponent<Text>();
            text.font = label.trueTypeFont;
            text.fontSize = label.fontSize;
            text.fontStyle = label.fontStyle;
            text.resizeTextForBestFit = label.overflowMethod == UILabel.Overflow.ShrinkContent;
            text.resizeTextMinSize = 1;
            text.resizeTextMaxSize = label.fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = label.color;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.text = label.text;
            if (label.effectStyle == UILabel.Effect.Outline)
            {
                var outline = target.AddComponent<Outline>();
                outline.effectColor = label.effectColor;
                outline.effectDistance = label.effectDistance;
            }
            else if (label.effectStyle != UILabel.Effect.None) throw new InvalidOperationException("Unsupported text effect: " + label.name);
        }
        else if (label != null)
        {
            var text = target.AddComponent<TextMeshProUGUI>();
            text.font = fonts[label.bitmapFont];
            text.fontSize = label.fontSize;
            text.fontSizeMax = label.fontSize;
            text.fontSizeMin = 1;
            text.enableAutoSizing = label.overflowMethod == UILabel.Overflow.ShrinkContent;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = rect.pivot.x == 0 ? TextAlignmentOptions.MidlineLeft :
                rect.pivot.x == 1 ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.Midline;
            text.color = label.color;
            text.richText = false;
            text.raycastTarget = false;
            text.text = label.text;
        }
        var texture = source.GetComponent<UITexture>();
        if (texture != null && presentationOnly)
        {
            if (texture.type != UIBasicSprite.Type.Simple || texture.flip != UIBasicSprite.Flip.Nothing)
                throw new InvalidOperationException("Unsupported texture geometry: " + source.name);
            var image = target.AddComponent<RawImage>();
            image.texture = texture.mainTexture;
            image.uvRect = texture.uvRect;
            image.material = texture.material;
            image.color = texture.color;
            image.raycastTarget = false;
        }
        var oldTween = source.GetComponent<TweenPosition>();
        if (oldTween != null)
        {
            if (oldTween.method != UITweener.Method.Linear || oldTween.delay != 0 || oldTween.worldSpace ||
                oldTween.onFinished.Count != 0 || (oldTween.style != UITweener.Style.Once && oldTween.style != UITweener.Style.PingPong))
                throw new InvalidOperationException("Unsupported tween: " + source.name);
            var tween = target.AddComponent<ScoreboardPositionTween>();
            tween.from = oldTween.from;
            tween.to = oldTween.to;
            tween.duration = oldTween.duration;
            tween.ignoreTimeScale = oldTween.ignoreTimeScale;
            tween.pingPong = oldTween.style == UITweener.Style.PingPong;
            tween.curve = new AnimationCurve(oldTween.animationCurve.keys)
            { preWrapMode = oldTween.animationCurve.preWrapMode, postWrapMode = oldTween.animationCurve.postWrapMode };
            tween.enabled = oldTween.enabled;
        }
        foreach (Transform child in source) ConvertNode(child, rect, sprites, fonts, objects, presentationOnly);
        target.SetActive(source.gameObject.activeSelf);
        return target;
    }

    private static Texture2D ReadTexture(Texture texture)
    {
        var previous = RenderTexture.active;
        var temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        try
        {
            Graphics.Blit(texture, temporary);
            RenderTexture.active = temporary;
            var result = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            result.Apply();
            return result;
        }
        finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(temporary); }
    }

    private static ScoreboardSpriteCatalog BuildSprites(UIAtlas atlas)
    {
        return BuildSprites(atlas, AssetFolder, name => name.StartsWith("scoreboard_") ||
            Enumerable.Range(1, 10).Any(index => name == "logo_" + index));
    }

    public static ScoreboardSpriteCatalog BuildSprites(UIAtlas atlas, string folder, Func<string, bool> include)
    {
        var source = ReadTexture(atlas.texture);
        try
        {
            var entries = new List<ScoreboardSpriteCatalog.Entry>();
            foreach (var data in atlas.spriteList.Where(item => include(item.name)))
            {
                int width = data.width + data.paddingLeft + data.paddingRight;
                int height = data.height + data.paddingTop + data.paddingBottom;
                var pixels = new Texture2D(width, height, TextureFormat.RGBA32, false);
                pixels.SetPixels32(new Color32[width * height]);
                pixels.SetPixels(data.paddingLeft, data.paddingBottom, data.width, data.height,
                    source.GetPixels(data.x, source.height - data.y - data.height, data.width, data.height));
                pixels.Apply();
                string path = folder + "/" + data.name + ".png";
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                Object.DestroyImmediate(pixels);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.spriteBorder = new Vector4(data.borderLeft, data.borderBottom, data.borderRight, data.borderTop);
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = atlas.texture.filterMode;
                importer.SaveAndReimport();
                entries.Add(new ScoreboardSpriteCatalog.Entry { name = data.name,
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path), nativeSize = new Vector2(width, height) });
            }
            string catalogPath = folder + "/Sprites.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<ScoreboardSpriteCatalog>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ScoreboardSpriteCatalog>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }
            catalog.entries = entries.ToArray();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }
        finally { Object.DestroyImmediate(source); }
    }

    public static TMP_FontAsset BuildFont(UIFont source, string folder = AssetFolder)
    {
        string path = folder + "/" + source.name + " TMP.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (existing != null) return existing;
        var uv = source.uvRect;
        var bitmap = source.bmFont;
        var font = ScriptableObject.CreateInstance<TMP_FontAsset>();
        font.name = source.name + " TMP";
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        font.atlasTextures = new[] { source.texture };
        font.faceInfo = new FaceInfo { familyName = source.name, styleName = "Regular", pointSize = bitmap.charSize,
            scale = 1, lineHeight = bitmap.charSize, ascentLine = bitmap.charSize, capLine = bitmap.charSize,
            meanLine = bitmap.charSize * .5f, baseline = 0, descentLine = 0, underlineThickness = 1, tabWidth = bitmap.charSize };
        var serialized = new SerializedObject(font);
        serialized.FindProperty("m_Version").stringValue = "1.1.0";
        serialized.FindProperty("m_AtlasWidth").intValue = source.texture.width;
        serialized.FindProperty("m_AtlasHeight").intValue = source.texture.height;
        serialized.FindProperty("m_AtlasRenderMode").intValue = (int)GlyphRenderMode.SMOOTH;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        foreach (var item in bitmap.glyphs)
        {
            var glyph = new Glyph((uint)item.index,
                new GlyphMetrics(item.width, item.height, item.offsetX, bitmap.charSize - item.offsetY, item.advance),
                new GlyphRect(Mathf.RoundToInt(uv.xMin * source.texture.width) + item.x,
                    Mathf.RoundToInt(uv.yMax * source.texture.height) - item.y - item.height, item.width, item.height), 1, 0);
            font.glyphTable.Add(glyph);
            font.characterTable.Add(new TMP_Character((uint)item.index, glyph));
            if (item.kerning == null) continue;
            for (int i = 0; i < item.kerning.Count; i += 2)
                font.fontFeatureTable.glyphPairAdjustmentRecords.Add(new GlyphPairAdjustmentRecord(
                    new GlyphAdjustmentRecord((uint)item.kerning[i], new GlyphValueRecord(0, 0, item.kerning[i + 1], 0)),
                    new GlyphAdjustmentRecord((uint)item.index, new GlyphValueRecord())));
        }
        var material = new Material(Shader.Find("TextMeshPro/Bitmap Custom Atlas")) { name = font.name + " Material" };
        material.mainTexture = source.texture;
        font.material = material;
        AssetDatabase.CreateAsset(font, path);
        AssetDatabase.AddObjectToAsset(material, font);
        font.ReadFontAssetDefinition();
        EditorUtility.SetDirty(font);
        return font;
    }
}
#endif
