#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class FieldMinimapUGUIConverter
{
    public const string Folder = "Assets/MainGame/UI/FieldMinimapUGUI";
    public const string MapPath = Folder + "/FieldMinimap.prefab";
    public const string CountPath = Folder + "/FieldOutCount.prefab";
    public const string RunnerPath = Folder + "/MinimapRunner.prefab";
    public const string LegacyRunnerPath = "Assets/Resources/MainGame/prefabs/ControlUI/miniRunner2.prefab";
    public const string Output = "Docs/UIAudit/FieldMinimap";

    public static FieldOverlayCanvas AddRendering(GameObject target)
    {
        var rendering = target.AddComponent<FieldOverlayCanvas>();
        rendering.displayCanvas = target.AddComponent<Canvas>();
        rendering.displayCanvas.renderMode = RenderMode.WorldSpace;
        rendering.opacity = target.AddComponent<CanvasGroup>();
        rendering.opacity.blocksRaycasts = rendering.opacity.interactable = false;
        return rendering;
    }

    private static Transform Layer(string name, Transform parent)
    {
        var child = new GameObject(name, typeof(RectTransform)) { layer = parent.gameObject.layer };
        child.transform.SetParent(parent, false);
        ((RectTransform)child.transform).sizeDelta = Vector2.zero;
        return child.transform;
    }

    [MenuItem("Tools/UI Migration/Build UGUI Field Minimap Candidate")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first");
        Directory.CreateDirectory(Folder); Directory.CreateDirectory(Output); AssetDatabase.Refresh();
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath).GetComponentInChildren<UIFieldUI>(true);
        if (source.UguiMinimap != null) throw new InvalidOperationException("Already migrated");
        var marker = AssetDatabase.LoadAssetAtPath<GameObject>(LegacyRunnerPath).GetComponent<miniRunner>();
        foreach (var root in new[] { source.minimap, source.count.gameObject, marker.gameObject })
        foreach (var component in root.GetComponentsInChildren<Component>(true))
            if (!(component is Transform) && !(component is UISprite) && !(component is UILabel) && !(component is miniRunner))
                throw new InvalidOperationException("Unsupported component: " + component);
        var sprites = ScoreboardUGUIConverter.BuildSprites(source.minimap.GetComponent<UISprite>().atlas, Folder,
            n => n.StartsWith("minimap_") || n == "field_outbg" || n == "field_out_light");
        var fonts = new Dictionary<UIFont, TMP_FontAsset> { { marker._name.bitmapFont, ScoreboardUGUIConverter.BuildFont(marker._name.bitmapFont, Folder) } };
        var mapObjects = new Dictionary<GameObject, GameObject>();
        var map = ScoreboardUGUIConverter.ConvertNode(source.minimap.transform, null, sprites, fonts, mapObjects, true);
        try
        {
            var view = map.AddComponent<FieldMinimapView>();
            view.rendering = AddRendering(map);
            view.runners = Layer("Runners", map.transform);
            view.teams = Layer("Team icons", map.transform);
            view.namebars = Layer("Name backgrounds", map.transform);
            view.names = Layer("Names", map.transform);
            PrefabUtility.SaveAsPrefabAsset(map, MapPath);
        }
        finally { Object.DestroyImmediate(map); }
        var count = ScoreboardUGUIConverter.ConvertNode(source.count.transform, null, sprites, fonts, new Dictionary<GameObject, GameObject>(), true);
        try { AddRendering(count); PrefabUtility.SaveAsPrefabAsset(count, CountPath); }
        finally { Object.DestroyImmediate(count); }
        var runner = new GameObject("MinimapRunner", typeof(RectTransform)) { layer = marker.gameObject.layer };
        ((RectTransform)runner.transform).sizeDelta = Vector2.zero;
        try
        {
            var objects = new Dictionary<GameObject, GameObject>();
            var icon = ScoreboardUGUIConverter.ConvertNode(marker.transform, runner.transform, sprites, fonts, objects, true);
            icon.name = "Team"; icon.transform.localPosition = Vector3.zero;
            foreach (Transform child in icon.transform.Cast<Transform>().ToArray()) child.SetParent(runner.transform, false);
            var controller = runner.AddComponent<miniRunner>();
            var view = runner.AddComponent<MinimapRunnerView>();
            view.team = icon.GetComponent<Image>();
            view.namebar = objects[marker.transform.Find("namebar").gameObject].GetComponent<Image>();
            view.playerName = objects[marker._name.gameObject].GetComponent<TMP_Text>();
            view.sprites = sprites;
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("uguiView").objectReferenceValue = view;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(runner, RunnerPath);
            AssetDatabase.SaveAssets();
        }
        finally { Object.DestroyImmediate(runner); }
        FieldMinimapMigrationChecks.Capture(false);
        FieldMinimapMigrationChecks.Run();
    }

    [MenuItem("Tools/UI Migration/Apply Field Minimap To Game Prefab")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first");
        if (!File.ReadAllText(Output + "/presentation-checks.txt").StartsWith("PASS")) throw new InvalidOperationException("Pass checks first");
        var root = PrefabUtility.LoadPrefabContents(ScoreboardMigrationChecks.PrefabPath);
        try
        {
            var owner = root.GetComponentInChildren<UIFieldUI>(true);
            if (owner.UguiMinimap != null) throw new InvalidOperationException("Already migrated");
            var oldMap = owner.minimap; var oldCount = owner.count.gameObject;
            var removed = new HashSet<Object>();
            foreach (var branch in new[] { oldMap, oldCount })
            foreach (var t in branch.GetComponentsInChildren<Transform>(true))
            {
                removed.Add(t.gameObject);
                foreach (var component in t.GetComponents<Component>()) removed.Add(component);
            }
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || behaviour == owner || removed.Contains(behaviour)) continue;
                var property = new SerializedObject(behaviour).GetIterator();
                while (property.Next(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && removed.Contains(property.objectReferenceValue))
                        throw new InvalidOperationException("External binding: " + behaviour.name + "/" + property.propertyPath);
            }
            var map = Replace(oldMap, MapPath).GetComponent<FieldMinimapView>();
            var count = Replace(oldCount, CountPath);
            owner.minimap = map.runners.gameObject;
            owner.count = null;
            owner.outCount = new[] { count.transform.Find("count1").gameObject, count.transform.Find("count2").gameObject };
            var so = new SerializedObject(owner);
            so.FindProperty("uguiMinimap").objectReferenceValue = map;
            so.FindProperty("uguiMinimapRunner").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(RunnerPath).GetComponent<miniRunner>();
            so.ApplyModifiedPropertiesWithoutUndo();
            Object.DestroyImmediate(oldMap); Object.DestroyImmediate(oldCount);
            PrefabUtility.SaveAsPrefabAsset(root, ScoreboardMigrationChecks.PrefabPath);
            File.WriteAllText(Output + "/apply-status.txt", "APPLIED " + DateTime.UtcNow.ToString("O") + "\n");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static GameObject Replace(GameObject original, string asset)
    {
        var result = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(asset), original.transform.parent, false);
        result.name = original.name;
        result.transform.SetSiblingIndex(original.transform.GetSiblingIndex());
        result.transform.localPosition = original.transform.localPosition;
        result.transform.localRotation = original.transform.localRotation;
        result.transform.localScale = original.transform.localScale;
        return result;
    }
}
#endif
