#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseBall.BallPlay.UGUI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class IntegratedUGUIChecks
{
    [Serializable] private class Reference { public string asset, property, targetGuid; public long component, target; }
    [Serializable] private class References { public List<Reference> values = new List<Reference>(); }
    public static void CaptureReferences()
    {
        var plan = JsonUtility.FromJson<IntegratedUGUIConverter.Plan>(File.ReadAllText(IntegratedUGUIConverter.Output + "/candidate-plan.json"));
        var refs = File.Exists(IntegratedUGUIConverter.Output + "/source-references.json") ? JsonUtility.FromJson<References>(File.ReadAllText(IntegratedUGUIConverter.Output + "/source-references.json")) : new References();
        foreach (var asset in plan.assets)
        {
            if (refs.values.Any(r => r.asset == asset.source)) continue;
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(asset.source);
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null || AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(component)).StartsWith("Assets/NGUI/")) continue;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(component, out string _, out long id);
                var property = new SerializedObject(component).GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue == null) continue;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.objectReferenceValue, out string guid, out long target);
                    refs.values.Add(new Reference { asset = asset.source, component = id, property = property.propertyPath, targetGuid = guid, target = target });
                }
            }
        }
        File.WriteAllText(IntegratedUGUIConverter.Output + "/source-references.json", JsonUtility.ToJson(refs, true));
    }
    public static void CheckApplied()
    {
        CheckApplied(IntegratedUGUIConverter.Output + "/applied-checks.txt");
    }
    public static void CheckApplied(string reportPath)
    {
        var plan = JsonUtility.FromJson<IntegratedUGUIConverter.Plan>(File.ReadAllText(IntegratedUGUIConverter.Output + "/candidate-plan.json"));
        var refs = JsonUtility.FromJson<References>(File.ReadAllText(IntegratedUGUIConverter.Output + "/source-references.json"));
        int count = 0, native = 0;
        foreach (var asset in plan.assets)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(asset.source);
            var components = root.GetComponentsInChildren<MonoBehaviour>(true).Where(c => c != null).ToArray();
            Require(!components.Any(c => AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(c)).StartsWith("Assets/NGUI/")), asset.source + ": remaining NGUI behavior");
            native += root.GetComponentsInChildren<GameUIElement>(true).Length;
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                Require(graphic.GetComponent<CanvasRenderer>() != null, asset.source + ": missing CanvasRenderer " + graphic.name);
            var byId = components.ToDictionary(c => { AssetDatabase.TryGetGUIDAndLocalFileIdentifier(c, out string _, out long id); return id; });
            foreach (var reference in refs.values.Where(r => r.asset == asset.source))
            {
                Require(byId.TryGetValue(reference.component, out var component), asset.source + ": lost controller " + reference.component);
                var property = new SerializedObject(component).FindProperty(reference.property);
                Require(property != null && property.objectReferenceValue != null, asset.source + " / " + component.name + ": lost reference " + reference.property);
                long expected = reference.target;
                string expectedGuid = reference.targetGuid;
                if (property.objectReferenceValue is Material)
                {
                    var sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(reference.targetGuid));
                    if (sourceMaterial != null)
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(IntegratedUGUIConverter.NativeMaterial(sourceMaterial), out expectedGuid, out expected);
                }
                if (reference.targetGuid == AssetDatabase.AssetPathToGUID(asset.source))
                {
                    var identity = asset.identities.FirstOrDefault(i => i.original == reference.target);
                    var replacement = identity == null ? null : asset.components.FirstOrDefault(c => c.original == identity.replacement);
                    if (replacement != null)
                        expected = asset.identities.FirstOrDefault(i => i.replacement == replacement.replacement)?.original ?? replacement.replacement;
                }
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.objectReferenceValue, out string actualGuid, out long actualId);
                Require(actualGuid == expectedGuid && actualId == expected, asset.source + " / " + component.name + ": changed reference target " + reference.property + " expected=" + expectedGuid + ":" + expected + " actual=" + actualGuid + ":" + actualId);
                count++;
            }
            foreach (var tween in root.GetComponentsInChildren<GameUITween>(true))
                foreach (var action in tween.onFinished) CheckAction(action, tween.name);
            foreach (var pointer in root.GetComponentsInChildren<GameUIPointer>(true))
                foreach (var action in pointer.onPress.Concat(pointer.onRelease).Concat(pointer.onClick)) CheckAction(action, pointer.name);
            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
                if (animator.runtimeAnimatorController != null)
                    foreach (var clip in animator.runtimeAnimatorController.animationClips)
                        Require(!AnimationUtility.GetCurveBindings(clip).Any(b => typeof(UIWidget).IsAssignableFrom(b.type) || b.type == typeof(UIPanel)), asset.source + ": old animation binding " + clip.name);
        }
        File.WriteAllText(reportPath, "PASS " + DateTime.UtcNow.ToString("O") + "\nassets=" + plan.assets.Length + " nativeWidgets=" + native + " retainedControllerReferences=" + count + "\nExact reference GUID/fileID mapping and CanvasRenderer presence checked. No NGUI behaviors or NGUI animator curves in converted assets. Gameplay and device validation reported separately.\n");
    }
    private static void CheckAction(GameUIAction action, string name)
    {
        if (action.mTarget == null) return;
        Require(action.mTarget.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(m => m.Name == action.mMethodName && m.GetParameters().Length == (action.mParameters?.Length ?? 0)), name + ": invalid native callback " + action.mMethodName);
    }
    [MenuItem("Tools/UI Migration/Integrated/Check Candidates")]
    public static void Run()
    {
        var plan = JsonUtility.FromJson<IntegratedUGUIConverter.Plan>(File.ReadAllText(IntegratedUGUIConverter.Output + "/candidate-plan.json"));
        var report = new List<string>();
        int widgets = 0, tweenSamples = 0, callbacks = 0, inertCallbacks = 0;
        foreach (var asset in plan.assets)
        {
            var root = PrefabUtility.LoadPrefabContents(asset.candidate);
            try
            {
                foreach (var original in root.GetComponentsInChildren<UIWidget>(true))
                {
                    var target = original.GetComponent<GameUIElement>();
                    Require(target != null, original.name + ": missing replacement");
                    Require(target.width == original.width && target.height == original.height, original.name + ": dimensions");
                    Require(target.color == original.color && target.pivotOffset == original.pivotOffset, original.name + ": color/pivot");
                    Require(target.enabled == original.enabled, original.name + ": enabled");
                    target.Apply();
                    if (original is UISprite sprite)
                    {
                        Require(target.spriteName == sprite.spriteName, original.name + ": sprite name");
                        bool exists = target.sprites.TryGet(sprite.spriteName, out var entry);
                        Require(target.sprites.material == (sprite.atlas == null ? null : IntegratedUGUIConverter.NativeAtlasMaterial(sprite.atlas)), original.name + ": atlas blend material");
                        Require(exists == (sprite.GetAtlasSprite() != null), original.name + ": sprite lookup");
                        if (exists)
                        {
                            Require(entry.sprite != null, original.name + ": sprite asset");
                            foreach (float fill in new[] { 0f, .25f, .5f, 1f })
                            {
                                target.fillAmount = fill; Require(Mathf.Approximately(((Image)target.graphic).fillAmount, fill), "Fill propagation");
                            }
                            target.fillAmount = sprite.fillAmount;
                        }
                    }
                    if (original is UILabel label)
                    {
                        Require(target.text == label.text, original.name + ": text");
                        Require(target.graphic is TMP_Text || target.graphic is Text, original.name + ": text graphic");
                        string marked = "[FFEA00]Fastball[-]   150km";
                        target.text = marked;
                        string actual = target.graphic is TMP_Text tmp ? tmp.text : ((Text)target.graphic).text;
                        Require(actual == (label.supportEncoding ? "<color=#FFEA00>Fastball</color>   150km" : marked), original.name + ": live markup");
                        target.text = label.text;
                    }
                    if (target.graphic != null) Require(!target.graphic.raycastTarget, original.name + ": display intercepts input");
                    widgets++;
                }
                foreach (var original in root.GetComponentsInChildren<UITweener>(true))
                {
                    Type type = original is TweenAlpha ? typeof(GameUITweenAlpha) : original is TweenPosition ? typeof(GameUITweenPosition)
                        : original is TweenScale ? typeof(GameUITweenScale) : typeof(GameUITweenRotation);
                    var target = (GameUITween)original.GetComponent(type);
                    Require(target != null, original.name + ": tween replacement");
                    for (int index = 0; index <= 20; index++)
                    {
                        float sample = index / 20f;
                        original.Sample(sample, index == 20);
                        Vector4 expected = TweenValue(original);
                        target.Sample(sample, index == 20);
                        Vector4 actual = TweenValue(target);
                        Require(Vector4.Distance(expected, actual) < .003f, original.name + ": tween sample " + index + " expected=" + expected + " actual=" + actual);
                        tweenSamples++;
                    }
                }
                foreach (var pointer in root.GetComponentsInChildren<GameUIPointer>(true))
                {
                    Require(pointer.inputCollider != null, pointer.name + ": input shape");
                    Require(pointer.GetComponentsInChildren<GameUIHitTarget>(true).Any(h => h.pointer == pointer), pointer.name + ": raycast target");
                    foreach (var action in pointer.onPress.Concat(pointer.onRelease).Concat(pointer.onClick).Concat(pointer.onHoverOver).Concat(pointer.onHoverOut))
                    {
                        if (action.mTarget == null) { inertCallbacks++; continue; }
                        int count = action.mParameters?.Length ?? 0;
                        Require(action.mTarget.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Any(m => m.Name == action.mMethodName && m.GetParameters().Length == count), pointer.name + ": callback " + action.mMethodName);
                        callbacks++;
                    }
                }
                report.Add("PASS candidate " + asset.source);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        report.Insert(0, "PASS " + DateTime.UtcNow.ToString("O"));
        report.Add("widgets=" + widgets + " tweenSamples=" + tweenSamples + " callbacks=" + callbacks + " preexistingNullCallbacks=" + inertCallbacks);
        report.Add("Candidate structural/presentation comparison only. Applied asset/reference checks and gameplay evidence are reported separately.");
        File.WriteAllLines(IntegratedUGUIConverter.Output + "/candidate-checks.txt", report);
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static Vector4 TweenValue(Component tween)
    {
        if (tween is TweenAlpha oldAlpha) return new Vector4(oldAlpha.value, 0, 0, 0);
        if (tween is GameUITweenAlpha alpha) return new Vector4(alpha.value, 0, 0, 0);
        if (tween is TweenPosition || tween is GameUITweenPosition) return tween.transform.localPosition;
        if (tween is TweenScale || tween is GameUITweenScale) return tween.transform.localScale;
        var rotation = tween.transform.localRotation; return new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);
    }
}
#endif
