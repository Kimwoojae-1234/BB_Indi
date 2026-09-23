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
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class RunnerControlUGUIConverter
{
    public const string Folder = "Assets/MainGame/UI/RunnerControlUGUI";
    public const string Candidate = Folder + "/RunnerControl.prefab";
    public const string Output = "Docs/UIAudit/RunnerControl";

    [MenuItem("Tools/UI Migration/Build UGUI Runner Control Candidate")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        Directory.CreateDirectory(Folder + "/Controls");
        Directory.CreateDirectory(Folder + "/Skills");
        Directory.CreateDirectory(Output);
        AssetDatabase.Refresh();
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreboardMigrationChecks.PrefabPath).GetComponentInChildren<ControlRunner>(true);
        if (source.UguiView != null) throw new InvalidOperationException("Already migrated.");
        foreach (var component in source._active.GetComponentsInChildren<Component>(true))
            if (!(component is Transform) && !(component is UISprite) && !(component is UILabel) && !(component is TweenAlpha) &&
                !(component is UIEventTrigger) && !(component is UIButtonScale) && !(component is Collider))
                throw new InvalidOperationException("Unsupported runner UI component: " + component);
        var controls = ScoreboardUGUIConverter.BuildSprites(source.baseObj[0].GetComponent<UISprite>().atlas,
            Folder + "/Controls", n => n.StartsWith("runnercon_") || n.StartsWith("steal_") || n.StartsWith("pickoff_"));
        var skills = ScoreboardUGUIConverter.BuildSprites(source.baseObj[0].transform.Find("onbase/skillIcon").GetComponent<UISprite>().atlas,
            Folder + "/Skills", n => n == "20007_S");
        controls.entries = controls.entries.Concat(skills.entries).ToArray();
        EditorUtility.SetDirty(controls);
        var objects = new Dictionary<GameObject, GameObject>();
        var replacement = ScoreboardUGUIConverter.ConvertNode(source._active.transform, null, controls,
            new Dictionary<UIFont, TMP_FontAsset>(), objects, true);
        try
        {
            var view = replacement.AddComponent<RunnerControlView>();
            view.canvas = replacement.AddComponent<IngameUGUICanvas>();
            view.canvas.displayCanvas = replacement.AddComponent<Canvas>();
            view.canvas.displayCanvas.renderMode = RenderMode.WorldSpace;
            view.canvas.opacity = replacement.AddComponent<CanvasGroup>();
            replacement.AddComponent<GraphicRaycaster>();
            view.sprites = controls;
            view.bases = new RunnerBaseButton[3];
            view.occupied = new Image[3]; view.action = new Image[3]; view.light = new Image[3]; view.skill = new Image[3]; view.speed = new Text[3];
            view.intentionalWalk = objects[source.intWalkButton];
            view.intentionalWalk.SetActive(false);
            // Global NGUI depths interleave the three overlapping base buttons.
            // Flatten their graphics; RunnerControlView owns visibility explicitly.
            foreach (var widget in source._active.GetComponentsInChildren<UIWidget>(true).OrderBy(w => w.depth))
            {
                objects[widget.gameObject].transform.SetParent(replacement.transform, true);
                objects[widget.gameObject].transform.SetAsLastSibling();
            }
            for (int i = 0; i < 3; i++)
            {
                var original = source.baseObj[i];
                ValidateBase(original, source, i);
                var target = objects[original];
                var button = target.AddComponent<RunnerBaseButton>();
                button.transition = Selectable.Transition.None;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.targetGraphic = target.GetComponent<Image>();
                button.targetGraphic.raycastTarget = true;
                view.bases[i] = button;
                view.occupied[i] = objects[original.transform.Find("onbase").gameObject].GetComponent<Image>();
                view.action[i] = objects[original.transform.Find("onbase/steal").gameObject].GetComponent<Image>();
                view.light[i] = objects[original.transform.Find("onbase/light").gameObject].GetComponent<Image>();
                view.skill[i] = objects[original.transform.Find("onbase/skillIcon").gameObject].GetComponent<Image>();
                view.speed[i] = objects[original.transform.Find("onbase/overall").gameObject].GetComponent<Text>();
                view.SetRunner(i, false, 0, false, true);
            }
            foreach (var button in view.bases) button.peers = view.bases;
            // Bind gameplay callbacks only when applying to the real controller.
            PrefabUtility.SaveAsPrefabAsset(replacement, Candidate);
            AssetDatabase.SaveAssets();
        }
        finally { Object.DestroyImmediate(replacement); }
        RunnerControlMigrationChecks.Capture(false, false);
        RunnerControlMigrationChecks.Capture(false, true);
        RunnerControlMigrationChecks.Run();
    }

    private static void ValidateBase(GameObject original, ControlRunner owner, int index)
    {
        var capsule = original.GetComponent<CapsuleCollider>();
        if (capsule == null || capsule.radius != 40 || capsule.height != 1 || capsule.center != Vector3.zero)
            throw new InvalidOperationException("Unexpected base hit geometry");
        string method = new[] { "pushFirstBase", "pushSecondBase", "pushThirdBase" }[index];
        var trigger = original.GetComponent<UIEventTrigger>();
        if (trigger.onPress.Count != 1 || trigger.onPress[0].target != owner || trigger.onPress[0].methodName != method)
            throw new InvalidOperationException("Unexpected base press binding");
        if (new[] { trigger.onHoverOver, trigger.onHoverOut, trigger.onRelease, trigger.onSelect, trigger.onDeselect, trigger.onClick,
            trigger.onDoubleClick, trigger.onDragStart, trigger.onDragEnd, trigger.onDragOver, trigger.onDragOut, trigger.onDrag }.Any(list => list.Count != 0))
            throw new InvalidOperationException("Additional event requires conversion");
        var tween = original.transform.Find("onbase/light").GetComponent<TweenAlpha>();
        if (tween.from != 1 || tween.to != 0 || tween.duration != .3f || !tween.ignoreTimeScale || tween.style != UITweener.Style.PingPong ||
            tween.method != UITweener.Method.Linear || tween.delay != 0 || tween.onFinished.Count != 0)
            throw new InvalidOperationException("Unexpected light tween");
    }

    [MenuItem("Tools/UI Migration/Apply Runner Control To Game Prefab")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        if (!File.ReadAllText(Output + "/presentation-checks.txt").StartsWith("PASS")) throw new InvalidOperationException("Pass candidate checks first.");
        var root = PrefabUtility.LoadPrefabContents(ScoreboardMigrationChecks.PrefabPath);
        try
        {
            var owner = root.GetComponentInChildren<ControlRunner>(true);
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
                        throw new InvalidOperationException("External runner binding: " + behaviour.name + "/" + properties.propertyPath);
            }
            var replacement = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Candidate), old.transform.parent, false);
            replacement.name = old.name;
            replacement.transform.SetSiblingIndex(old.transform.GetSiblingIndex());
            replacement.transform.localPosition = old.transform.localPosition;
            replacement.transform.localRotation = old.transform.localRotation;
            replacement.transform.localScale = old.transform.localScale;
            var view = replacement.GetComponent<RunnerControlView>();
            owner._active = replacement;
            owner.baseObj = view.bases.Select(b => b.gameObject).ToArray();
            owner.intWalkButton = view.intentionalWalk;
            UnityAction[] callbacks = { owner.pushFirstBase, owner.pushSecondBase, owner.pushThirdBase };
            for (int i = 0; i < 3; i++) UnityEventTools.AddPersistentListener(view.bases[i].onClick, callbacks[i]);
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("uguiView").objectReferenceValue = view;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Object.DestroyImmediate(old);
            PrefabUtility.SaveAsPrefabAsset(root, ScoreboardMigrationChecks.PrefabPath);
            File.WriteAllText(Output + "/apply-status.txt", "APPLIED " + DateTime.UtcNow.ToString("O"));
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
#endif
