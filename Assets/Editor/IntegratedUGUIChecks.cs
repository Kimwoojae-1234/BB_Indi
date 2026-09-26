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
                        Require(!AnimationUtility.GetCurveBindings(clip).Any(b => RemainingUGUIChecks.IsLegacyUIType(b.type)), asset.source + ": old animation binding " + clip.name);
        }
        File.WriteAllText(reportPath, "PASS " + DateTime.UtcNow.ToString("O") + "\nassets=" + plan.assets.Length + " nativeWidgets=" + native + " retainedControllerReferences=" + count + "\nExact reference GUID/fileID mapping and CanvasRenderer presence checked. No NGUI behaviors or NGUI animator curves in converted assets. Gameplay and device validation reported separately.\n");
    }
    private static void CheckAction(GameUIAction action, string name)
    {
        if (action.mTarget == null) return;
        Require(action.mTarget.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(m => m.Name == action.mMethodName && m.GetParameters().Length == (action.mParameters?.Length ?? 0)), name + ": invalid native callback " + action.mMethodName);
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
#endif
