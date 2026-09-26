#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay.UGUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class RemainingUGUIChecks
{
    static readonly HashSet<string> LegacyTypes = new HashSet<string> { "UIWidget", "UISprite", "UITexture", "UILabel", "UIPanel", "UITweener", "TweenAlpha", "TweenColor", "TweenPosition", "TweenScale", "TweenRotation" };
    public static bool IsLegacyUIType(Type type) => type != null && LegacyTypes.Contains(type.Name);
    [Serializable] public sealed class Reference { public string asset, property, guid; public long component, target; }
    [Serializable] public sealed class References { public Reference[] values; }
    public static void Check()
    {
        var baseline = JsonUtility.FromJson<RemainingUGUIMigration.Inventory>(File.ReadAllText(RemainingUGUIMigration.Output + "/inventory.json"));
        var errors = new List<string>(); var report = new List<string>();
        foreach (string name in LegacyTypes)
            if (typeof(GameUIElement).Assembly.GetType(name) != null) errors.Add("Legacy UI type remains in runtime assembly: " + name);
        var shader = Shader.Find("Game/UI/Transparent Vertex Color");
        if (shader == null || !shader.isSupported || ShaderUtil.ShaderHasError(shader)) errors.Add("Native transparent shader is unavailable or has compilation errors");
        var references = JsonUtility.FromJson<References>(File.ReadAllText(RemainingUGUIMigration.Output + "/controller-references.json")).values.ToLookup(r => r.asset);
        int assets = 0, widgets = 0, missing = 0, curves = 0, retainedReferences = 0, unavailableReferences = 0;
        foreach (string path in AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/") && !p.StartsWith("Assets/NGUI/") && !p.StartsWith("Assets/Editor/Remaining") && (p.EndsWith(".prefab") || p.EndsWith(".unity"))).OrderBy(p => p))
        {
            var scene = default(UnityEngine.SceneManagement.Scene);
            try
            {
                GameObject[] roots;
                if (path.EndsWith(".unity")) { scene = EditorSceneManager.OpenPreviewScene(path); roots = scene.GetRootGameObjects(); }
                else { var root = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (root == null) { errors.Add("Cannot load " + path); continue; } roots = new[] { root }; }
                var components = roots.SelectMany(r => r.GetComponentsInChildren<MonoBehaviour>(true)).ToArray();
                int lost = components.Count(c => c == null); missing += lost;
                int oldMissing = baseline.assets.FirstOrDefault(a => a.path == path)?.missingScripts.Length ?? 0;
                if (lost != oldMissing) errors.Add(path + " missing scripts: before=" + oldMissing + " after=" + lost);
                foreach (var c in components.Where(c => c != null))
                {
                    string script = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(c));
                    if (script.StartsWith("Assets/NGUI/")) errors.Add(path + " remaining NGUI=" + c.GetType().Name);
                    if (c is GameUIElement e)
                    {
                        widgets++;
                        if (e.kind != GameUIElement.ElementKind.Widget && e.graphic == null) errors.Add(path + " missing native graphic=" + e.name);
                        if (e.graphic != null && e.graphic.GetComponent<CanvasRenderer>() == null) errors.Add(path + " missing renderer=" + e.name);
                    }
                    if (c is GameUIProgress progress && progress.slider == null) errors.Add(path + " missing Slider");
                    if (c is GameUIToggle toggle && toggle.toggle == null) errors.Add(path + " missing Toggle");
                    if (c is GameUIInput input && (input.input == null || input.input.textComponent == null)) errors.Add(path + " missing InputField");
                }
                var byId = components.Where(c => c != null).GroupBy(c => ObjectId(c)).ToDictionary(g => g.Key, g => g.First());
                foreach (var reference in references[path])
                {
                    // Missing source scripts and unresolved external resources are recorded,
                    // not converted into invented replacement controllers or resources.
                    if (!byId.TryGetValue(reference.component, out var owner)) { unavailableReferences++; continue; }
                    var property = new SerializedObject(owner).FindProperty(reference.property);
                    if (property == null || property.propertyType != SerializedPropertyType.ObjectReference) { unavailableReferences++; continue; }
                    string sourceGuid = AssetDatabase.AssetPathToGUID(path);
                    if (reference.guid != sourceGuid && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(reference.guid))) { unavailableReferences++; continue; }
                    var actual = property.objectReferenceValue;
                    if (actual == null)
                    {
                        // A known missing-script component is not a resolvable reference.
                        if (reference.guid == sourceGuid && !byId.ContainsKey(reference.target)) { unavailableReferences++; continue; }
                        errors.Add(path + " null reference " + owner.GetType().Name + "." + reference.property); continue;
                    }
                    var id = GlobalObjectId.GetGlobalObjectIdSlow(actual);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(actual, out string actualGuid, out long actualId);
                    if (scene.IsValid() && actual is Component component && component.gameObject.scene == scene || scene.IsValid() && actual is GameObject go && go.scene == scene)
                    { actualGuid = sourceGuid; actualId = (long)id.targetObjectId; }
                    if (actualGuid != reference.guid || actualId != reference.target) errors.Add(path + " changed reference " + owner.GetType().Name + "." + reference.property + " expected=" + reference.guid + ":" + reference.target + " actual=" + actualGuid + ":" + actualId);
                    else retainedReferences++;
                }
                report.Add(path + ": nativeWidgets=" + components.OfType<GameUIElement>().Count() + "; missingScripts=" + lost);
                assets++;
            }
            finally { if (scene.IsValid()) EditorSceneManager.ClosePreviewScene(scene); }
        }
        foreach (string path in AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/") && !p.StartsWith("Assets/NGUI/") && p.EndsWith(".anim")))
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                curves++;
                if (IsLegacyUIType(binding.type)) errors.Add(path + " legacy curve=" + binding.path + ":" + binding.propertyName);
            }
        }
        File.WriteAllLines(RemainingUGUIMigration.Output + "/applied-assets.txt", report);
        File.WriteAllText(RemainingUGUIMigration.Output + "/applied-checks.txt", (errors.Count == 0 ? "PASS" : "FAIL") + " assets=" + assets + " nativeWidgets=" + widgets + " existingMissingScripts=" + missing + " animationCurves=" + curves + " retainedReferences=" + retainedReferences + " unavailableSourceReferences=" + unavailableReferences + "\n" + string.Join("\n", errors));
        if (errors.Count > 0) throw new InvalidOperationException(errors.Count + " validation failures. See applied-checks.txt.");
    }
    static long ObjectId(UnityEngine.Object obj)
    {
        if (EditorUtility.IsPersistent(obj)) { AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string _, out long id); return id; }
        return (long)GlobalObjectId.GetGlobalObjectIdSlow(obj).targetObjectId;
    }
}
#endif
