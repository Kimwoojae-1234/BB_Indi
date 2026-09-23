#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class PitchSelectionUGUIConverter
{
    public const string Folder = "Assets/MainGame/UI/PitchSelectionUGUI";
    public const string Candidate = Folder + "/PitchSelection.prefab";
    public const string Output = "Docs/UIAudit/PitchSelection";

    [MenuItem("Tools/UI Migration/Build UGUI Pitch Selection Candidate")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        Directory.CreateDirectory(Folder);
        Directory.CreateDirectory(Output);
        AssetDatabase.Refresh();
        var controller = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath).GetComponentInChildren<ControlPitchingSelect>(true);
        if (controller.UguiView != null) throw new InvalidOperationException("Already migrated.");
        foreach (var component in controller._active.GetComponentsInChildren<Component>(true))
            if (!(component is Transform) && !(component is UISprite) && !(component is UILabel) && !(component is UITexture) &&
                !(component is pitchingSelectButton) && !(component is UIEventTrigger) && !(component is UIButtonScale) && !(component is BoxCollider))
                throw new InvalidOperationException("Unsupported pitch selector component: " + component);
        // Capture the actual source before replacing anything.
        PitchSelectionMigrationChecks.Capture(false, false);
        PitchSelectionMigrationChecks.Capture(false, true);
        var catalog = ScoreboardUGUIConverter.BuildSprites(controller.button[0].back.atlas, Folder, name => name.StartsWith("pselect_"));
        var fonts = new Dictionary<UIFont, TMP_FontAsset>();
        foreach (var label in controller._active.GetComponentsInChildren<UILabel>(true))
            if (!fonts.ContainsKey(label.bitmapFont)) fonts.Add(label.bitmapFont, ScoreboardUGUIConverter.BuildFont(label.bitmapFont, Folder));
        var objects = new Dictionary<GameObject, GameObject>();
        var replacement = ScoreboardUGUIConverter.ConvertNode(controller._active.transform, null, catalog, fonts, objects, true);
        try
        {
            var view = replacement.AddComponent<PitchSelectionCanvas>();
            view.displayCanvas = replacement.AddComponent<Canvas>();
            view.displayCanvas.renderMode = RenderMode.WorldSpace;
            view.opacity = replacement.AddComponent<CanvasGroup>();
            replacement.AddComponent<GraphicRaycaster>();
            var effects = new GameObject("Selection effects", typeof(RectTransform)) { layer = replacement.layer };
            effects.transform.SetParent(replacement.transform, false);
            foreach (var original in controller.button)
            {
                ValidateButton(original);
                var target = objects[original.gameObject];
                // NGUI sorts by depth across the old nested hierarchy. Flatten each
                // row so the flash stays below text and the ring above everything.
                foreach (var widget in original.GetComponentsInChildren<UIWidget>(true).Where(w => w.gameObject != original.gameObject).OrderBy(w => w.depth))
                    objects[widget.gameObject].transform.SetParent(target.transform, true);
                var adapter = target.AddComponent<pitchingSelectButton>();
                var button = target.AddComponent<PitchSelectionButtonView>();
                button.strength = objects[original.back.gameObject].GetComponent<Image>();
                button.pitchName = objects[original.text.gameObject].GetComponent<Image>();
                button.number = objects[original.num.gameObject].GetComponent<TMP_Text>();
                button.effect = objects[original.effect.gameObject].GetComponent<RawImage>();
                button.effectOffset = target.transform.InverseTransformPoint(button.effect.transform.position);
                button.effect.transform.SetParent(effects.transform, true);
                button.light = objects[original._light];
                button.sprites = catalog;
                button.movement = target.AddComponent<ScoreboardPositionTween>();
                button.movement.enabled = false;
                button.targetGraphic = target.GetComponent<Image>();
                button.targetGraphic.raycastTarget = true;
                button.targetGraphic.raycastPadding = new Vector4(0, 3, 0, 3);
                button.transition = Selectable.Transition.None;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                UnityEventTools.AddPersistentListener(button.onClick, adapter.pushButton);
                var serialized = new SerializedObject(adapter);
                serialized.FindProperty("uguiView").objectReferenceValue = button;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            effects.transform.SetAsLastSibling();
            PrefabUtility.SaveAsPrefabAsset(replacement, Candidate);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Output + "/build-status.txt", "BUILT " + Candidate);
        }
        finally { Object.DestroyImmediate(replacement); }
        PitchSelectionMigrationChecks.Run();
    }

    private static void ValidateButton(pitchingSelectButton button)
    {
        var trigger = button.GetComponent<UIEventTrigger>();
        if (trigger.onRelease.Count != 1 || trigger.onRelease[0].target != button || trigger.onRelease[0].methodName != "pushButton")
            throw new InvalidOperationException("Unexpected release binding: " + button.name);
        if (new[] { trigger.onHoverOver, trigger.onHoverOut, trigger.onPress, trigger.onSelect, trigger.onDeselect, trigger.onClick,
            trigger.onDoubleClick, trigger.onDragStart, trigger.onDragEnd, trigger.onDragOver, trigger.onDragOut, trigger.onDrag }.Any(list => list.Count != 0))
            throw new InvalidOperationException("Additional event requires conversion: " + button.name);
        var scale = button.GetComponent<UIButtonScale>();
        if (scale.hover != Vector3.one || scale.pressed != Vector3.one * .9f || scale.duration != .2f)
            throw new InvalidOperationException("Unexpected press scale: " + button.name);
        var collider = button.GetComponent<BoxCollider>();
        if (collider.center != Vector3.zero || collider.size != new Vector3(230, 58, 1))
            throw new InvalidOperationException("Unexpected hit rectangle: " + button.name);
    }

    [MenuItem("Tools/UI Migration/Apply Pitch Selection To Game Prefab")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        if (!File.ReadAllText(Output + "/presentation-checks.txt").StartsWith("PASS")) throw new InvalidOperationException("Pass candidate checks first.");
        var root = PrefabUtility.LoadPrefabContents(ScoreboardMigrationChecks.PrefabPath);
        try
        {
            var owner = root.GetComponentInChildren<ControlPitchingSelect>(true);
            if (owner.UguiView != null) throw new InvalidOperationException("Already migrated.");
            var old = owner._active;
            var oldObjects = new HashSet<Object>();
            foreach (var node in old.GetComponentsInChildren<Transform>(true))
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
                        throw new InvalidOperationException("External pitch binding: " + behaviour.name + "/" + properties.propertyPath);
            }
            var replacement = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Candidate), old.transform.parent, false);
            replacement.name = old.name;
            replacement.transform.SetSiblingIndex(old.transform.GetSiblingIndex());
            replacement.transform.localPosition = old.transform.localPosition;
            replacement.transform.localRotation = old.transform.localRotation;
            replacement.transform.localScale = old.transform.localScale;
            owner._active = replacement;
            owner.button = replacement.GetComponentsInChildren<pitchingSelectButton>(true);
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("uguiView").objectReferenceValue = replacement.GetComponent<PitchSelectionCanvas>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Object.DestroyImmediate(old);
            PrefabUtility.SaveAsPrefabAsset(root, ScoreboardMigrationChecks.PrefabPath);
            File.WriteAllText(Output + "/apply-status.txt", "APPLIED " + DateTime.UtcNow.ToString("O"));
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
#endif
