#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay.UGUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Explicit isolated Play Mode fixture; restores editor scenes and never saves assets.
[InitializeOnLoad]
public static class CanvasConnectionPlayChecks
{
    private const string Key = "CanvasConnectionPlayChecks";
    private static string Report => SessionState.GetString(Key + "Report", "Docs/UIAudit/CanvasRestructure/Step4/play-checks.txt");
    [Serializable] private class SavedSetup { public SceneSetup[] scenes; }
    private static readonly List<string> results = new List<string>();
    private static GameUIRoot root;
    private static GameUIPanel outer, inner, clipPanel;
    private static GameUIElement group, left, right;
    private static GameUICanvasBatch batch;
    private static GameUIScroll scroll;
    private static GameUIPointer pointer;
    private static GameUIHitTarget hit;
    private static CanvasConnectionProbe probe;
    private static PointerEventData drag;
    private static RectTransform viewport, content;
    private static Vector2 initialContent;
    private static Texture2D maskTexture;
    private static int phase, nextFrame;
    static CanvasConnectionPlayChecks() { EditorApplication.update += Tick; }
    public static void Start() { StartWithReport("Docs/UIAudit/CanvasRestructure/Step4/play-checks.txt"); }
    public static void StartStep5() { StartWithReport("Docs/UIAudit/CanvasRestructure/Step5/connection-regression-checks.txt"); }
    public static void StartComplete() { StartWithReport("Docs/UIAudit/NGUIComplete/connection-regression-checks.txt"); }
    private static void StartWithReport(string report)
    {
        SessionState.SetString(Key + "Report", report);
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            Require(!SceneManager.GetSceneAt(i).isDirty, "Save scene changes before starting the fixture.");
        phase = nextFrame = 0; results.Clear();
        SessionState.SetBool(Key + "Background", Application.runInBackground);
        SessionState.SetString(Key + "Setup", JsonUtility.ToJson(new SavedSetup { scenes = EditorSceneManager.GetSceneManagerSetup() }));
        SessionState.SetBool(Key, true); SessionState.SetBool(Key + "Done", false);
        SessionState.SetString(Key + "Start", DateTime.UtcNow.ToString("O"));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Key, false)) return;
        if (SessionState.GetBool(Key + "Done", false))
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SessionState.SetBool(Key, false);
                if (Application.isBatchMode) EditorApplication.Exit(SessionState.GetInt(Key + "Exit", 1));
                else EditorSceneManager.RestoreSceneManagerSetup(JsonUtility.FromJson<SavedSetup>(SessionState.GetString(Key + "Setup", "")).scenes);
            }
            return;
        }
        try
        {
            Require((DateTime.UtcNow - DateTime.Parse(SessionState.GetString(Key + "Start", ""))).TotalSeconds < 120, "Fixture timed out");
            if (!EditorApplication.isPlayingOrWillChangePlaymode) { results.Add("Fixture interrupted before completion"); Finish(1); return; }
            EditorApplication.QueuePlayerLoopUpdate();
            if (!EditorApplication.isPlaying || Time.frameCount < nextFrame) return;
            switch (phase++)
            {
                case 0: results.Add(DateTime.UtcNow.ToString("O") + " isolated UGUI connection fixture"); Build(); break;
                case 1: CheckAlphaAndCapture(); CheckInput(); StartDrag(); break;
                case 2: ContinueDrag(); break;
                case 3: CancelAndReset(); CheckMasks(); break;
                case 4: CheckMaskRebind(); CheckPositionTween(); break;
                case 5: CheckTweenCompletion(); CheckGroupFallback(); break;
                case 6: CheckFallbackCapture(); Finish(0); return;
            }
            nextFrame = Time.frameCount + 3;
        }
        catch (Exception e) { results.Add(e.ToString()); Debug.LogException(e); Finish(1); }
    }
    private static void Build()
    {
        Application.runInBackground = true;
        EditorApplication.isPaused = false;
        var owner = new GameObject("Canvas connection fixture"); owner.SetActive(false);
        root = owner.AddComponent<GameUIRoot>();
        var cameraObject = new GameObject("UI Camera", typeof(Camera)); cameraObject.transform.SetParent(owner.transform, false);
        var camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 1;
        camera.transform.localPosition = new Vector3(0,0,-1); camera.nearClipPlane = .01f; camera.farClipPlane = 10;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
        var canvasRect = Rect("Canvas", owner.transform, Vector2.zero);
        var canvas = canvasRect.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera; canvas.planeDistance = 1; canvasRect.gameObject.AddComponent<GraphicRaycaster>();
        var scaler = canvasRect.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = 1;
        root.rootCanvas = canvas; root.uiCamera = camera;
        outer = Rect("Outer panel", canvasRect, Vector2.zero).gameObject.AddComponent<GameUIPanel>();
        inner = Rect("Inner panel and widget", outer.transform, Vector2.zero).gameObject.AddComponent<GameUIPanel>();
        group = inner.gameObject.AddComponent<GameUIElement>(); group.kind = GameUIElement.ElementKind.Widget;
        viewport = Rect("Viewport", inner.transform, new Vector2(200,120));
        content = Rect("Content", viewport, new Vector2(200,400));
        clipPanel = content.gameObject.AddComponent<GameUIPanel>(); clipPanel.clipping = 3;
        clipPanel.clipAnchor = viewport; clipPanel.clipRegion = new Vector4(0,0,200,120);
        var native = viewport.gameObject.AddComponent<ScrollRect>(); native.viewport = viewport; native.content = content;
        native.horizontal = false; native.vertical = true; native.movementType = ScrollRect.MovementType.Clamped;
        native.inertia = true; native.scrollSensitivity = 15;
        scroll = content.gameObject.AddComponent<GameUIScroll>(); scroll.scrollRect = native;
        var batchRect = Rect("Shared Canvas", content, Vector2.zero);
        var shared = batchRect.gameObject.AddComponent<Canvas>(); shared.overrideSorting = true;
        batch = batchRect.gameObject.AddComponent<GameUICanvasBatch>(); batch.displayCanvas = shared;
        batch.panel = clipPanel; batch.sourceParent = content; batch.depth = 5;
        left = Element("Left", -40, batchRect); right = Element("Right", 40, batchRect); batch.members = new[] { left, right };
        pointer = left.gameObject.AddComponent<GameUIPointer>(); pointer.scroll = scroll;
        var collider = left.gameObject.AddComponent<BoxCollider>(); collider.size = new Vector3(70,240,1); pointer.inputCollider = collider;
        var hitRect = Rect("Hit target", left.transform, new Vector2(70,240));
        hitRect.gameObject.AddComponent<Canvas>().overrideSorting = true; hitRect.gameObject.AddComponent<GraphicRaycaster>();
        hit = hitRect.gameObject.AddComponent<GameUIHitTarget>(); hit.pointer = pointer; hit.shape = collider;
        probe = owner.AddComponent<CanvasConnectionProbe>(); Require(probe != null, "Fixture callback probe missing");
        pointer.onPress.Add(Action("Press")); pointer.onRelease.Add(Action("Release")); pointer.onClick.Add(Action("Click"));
        owner.SetActive(true);
    }
    private static GameUIAction Action(string method) => new GameUIAction { mTarget = probe, mMethodName = method };
    private static RectTransform Rect(string name, Transform parent, Vector2 size)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false); rect.sizeDelta = size; return rect;
    }
    private static GameUIElement Element(string name, float x, RectTransform batchRect)
    {
        var rect = Rect(name, content, new Vector2(70,240)); rect.localPosition = new Vector3(x,0,0);
        var e = rect.gameObject.AddComponent<GameUIElement>(); e.kind = GameUIElement.ElementKind.Texture;
        e.layoutRect = rect; e.mWidth = 70; e.mHeight = 240; e.mDepth = 5; e.mTexture = Texture2D.whiteTexture;
        e.presentationRoot = Rect(name + " presentation", batchRect, Vector2.zero); e.canvasBatch = batch; e.displayCanvas = batch.displayCanvas;
        var mask = Rect("Clip", e.presentationRoot, new Vector2(200,120)); mask.gameObject.AddComponent<RectMask2D>();
        var presentation = Rect("Graphic presentation", mask, Vector2.zero);
        e.graphic = Rect("Graphic", presentation, Vector2.zero).gameObject.AddComponent<RawImage>(); e.graphic.raycastTarget = false;
        e.clipping = e.presentationRoot.gameObject.AddComponent<GameUIClip>(); e.clipping.source = rect;
        e.clipping.panels = new[] { clipPanel }; e.clipping.masks = new[] { mask }; e.clipping.presentation = presentation;
        return e;
    }
    private static void CheckAlphaAndCapture()
    {
        content.pivot = new Vector2(.2f,.7f);
        left.transform.localPosition = new Vector3(-40,0,0); right.transform.localPosition = new Vector3(40,0,0);
        root.SynchronizePresentation();
        Require(Vector3.Distance(left.clipping.masks[0].position,viewport.position) < .0001f,"Parent pivot change shifted the mask after synchronization");
        content.pivot = Vector2.one * .5f;
        left.transform.localPosition = new Vector3(-40,0,0); right.transform.localPosition = new Vector3(40,0,0);
        outer.alpha = .5f; inner.alpha = .5f; group.alpha = .5f; left.alpha = .5f;
        root.SynchronizePresentation();
        Near(left.graphic.color.a, .0625f, "Nested panel and widget alpha");
        Near(right.graphic.color.a, .125f, "Sibling inherited alpha");
        Near(ColorAt(left.transform.position).r, .0625f, "Rendered alpha applied more than once", .025f);
        var tween = GameUITweenAlpha.Begin(outer.gameObject, 1, 1); tween.Sample(1,true); tween.enabled = false;
        Near(ColorAt(left.transform.position).r, .125f, "Immediate alpha tween capture stale", .025f);
        outer.alpha = inner.alpha = group.alpha = left.alpha = 1;
        results.Add("PASS parent pivot change keeps mask origin; nested panel/widget alpha including same owner; shared rendering applies alpha once; immediate alpha tween capture.");
    }
    private static PointerEventData Event(int id = -1)
    {
        var data = new PointerEventData(EventSystem.current) { pointerId = id, button = PointerEventData.InputButton.Left };
        data.position = RectTransformUtility.WorldToScreenPoint(root.uiCamera, left.transform.position);
        data.pressPosition = data.position;
        data.pointerPressRaycast = new RaycastResult { gameObject = hit.gameObject, module = hit.GetComponent<GraphicRaycaster>() };
        return data;
    }
    private static bool Hits(Vector3 world)
    {
        root.SynchronizePresentation(); hit.Synchronize(); Canvas.ForceUpdateCanvases();
        var data = Event(); data.position = RectTransformUtility.WorldToScreenPoint(root.uiCamera, world);
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(data,hits);
        return hits.Any(r => r.gameObject == hit.gameObject);
    }
    private static void CheckInput()
    {
        Require(Hits(left.transform.position), "UGUI raycaster missed the button center");
        Require(!Hits(left.transform.TransformPoint(new Vector3(0,90,0))), "Raycast escaped the panel clip");
        outer.alpha = 0; Require(!Hits(left.transform.position), "Transparent panel received input"); outer.alpha = 1;
        var native = outer.gameObject.AddComponent<CanvasGroup>(); native.interactable = false;
        Require(!Hits(left.transform.position), "Non-interactable CanvasGroup received input"); native.interactable = true;
        native.blocksRaycasts = false; Require(!Hits(left.transform.position), "CanvasGroup blocksRaycasts ignored"); native.blocksRaycasts = true;
        native.alpha = 0; Require(!Hits(left.transform.position), "Native zero alpha received input");
        native.alpha = .01f; outer.alpha = .01f; Require(!Hits(left.transform.position), "Combined native/legacy transparency received input");
        native.alpha = outer.alpha = 1;
        var box = (BoxCollider)hit.shape; box.size = new Vector3(110,240,1); hit.Synchronize();
        Near(hit.rectTransform.rect.width,110,"Changed collider bounds did not update the UGUI hit rectangle");
        Require(Hits(left.transform.TransformPoint(new Vector3(-45,0,0))), "Resized collider hit area stale");
        box.enabled = false; Require(!Hits(left.transform.position), "Disabled collider received input"); box.enabled = true;
        var data = Event(); pointer.OnPointerDown(data); pointer.OnPointerUp(data); pointer.OnPointerClick(data);
        Require(probe.presses == 1 && probe.releases == 1 && probe.clicks == 1,"Click callbacks changed");
        var capsule = left.gameObject.AddComponent<CapsuleCollider>(); capsule.direction = 0; capsule.radius = 10; capsule.height = 100;
        hit.shape = capsule; hit.Synchronize(); Near(hit.rectTransform.rect.width,100,"Horizontal capsule bounds");
        Require(Hits(left.transform.TransformPoint(new Vector3(-40,0,0))),"Horizontal capsule raycast missed");
        hit.shape = box; Object.Destroy(capsule); hit.Synchronize();
        results.Add("PASS actual GraphicRaycaster: clip, legacy/native alpha, CanvasGroup input flags, collider disable/resize, horizontal capsule; click callbacks.");
    }
    private static void StartDrag()
    {
        scroll.scrollRect.velocity = new Vector2(0,200);
        drag = Event(); pointer.OnPointerDown(drag); pointer.OnInitializePotentialDrag(drag);
        Near(scroll.scrollRect.velocity.magnitude,0,"Potential drag failed to stop old inertia");
        pointer.OnBeginDrag(drag); initialContent = content.anchoredPosition;
        var other = Event(2); other.position += new Vector2(0,60); pointer.OnDrag(other); pointer.OnEndDrag(other);
        Require(content.anchoredPosition == initialContent, "Another pointer moved the active drag");
        drag.position += new Vector2(0,40); pointer.OnDrag(drag);
    }
    private static void ContinueDrag()
    {
        Require(Vector2.Distance(initialContent,content.anchoredPosition) > 1,"ScrollRect did not move from the forwarded drag");
        Require(Vector3.Distance(left.clipping.masks[0].position,viewport.position) < .0001f,"Mask followed scrolling content instead of viewport");
        Require(Vector3.Distance(left.clipping.presentation.position,left.transform.position) < .0001f,"Presentation lagged ScrollRect LateUpdate");
        pointer.OnPointerUp(drag); pointer.OnEndDrag(drag); pointer.OnPointerClick(drag);
        Require(probe.clicks == 1, "Drag release invoked click");
        drag = Event(); pointer.OnPointerDown(drag); pointer.OnBeginDrag(drag);
        drag.position += new Vector2(0,20); pointer.OnDrag(drag);
        pointer.enabled = false;
    }
    private static void CancelAndReset()
    {
        Near(scroll.scrollRect.velocity.magnitude,0,"Disabling pointer left inertia running");
        var stopped = content.anchoredPosition; drag.position += new Vector2(0,40); pointer.OnDrag(drag);
        Require(content.anchoredPosition == stopped,"Disabled drag changed content");
        pointer.enabled = true; scroll.ResetPosition(); Near(scroll.scrollRect.verticalNormalizedPosition,1,"Reset did not return to top");
        var wheel = Event(); wheel.scrollDelta = new Vector2(0,-1); pointer.OnScroll(wheel);
        Require(scroll.scrollRect.verticalNormalizedPosition < .999f,"Mouse wheel was not forwarded");
        scroll.enabled = false; Require(!scroll.scrollRect.enabled,"Scroll adapter disable left ScrollRect enabled");
        scroll.enabled = true; content.anchoredPosition = Vector2.zero; scroll.scrollRect.StopMovement();
        results.Add("PASS ScrollRect initialization, drag forwarding, second-pointer rejection, click suppression, hide cancellation, wheel/reset and adapter enable lifecycle; viewport stays fixed.");
    }
    private static void CheckMasks()
    {
        clipPanel.clipSoftness = new Vector2(7,11); root.SynchronizePresentation();
        Require(left.clipping.masks[0].GetComponent<RectMask2D>().softness == new Vector2Int(7,11),"Runtime softness stale");
        clipPanel.clipSoftness = Vector2.zero;
        Require(ColorAt(viewport.TransformPoint(new Vector3(-40,90,0))).r < .05f,"RectMask did not clip the graphic");
        clipPanel.clipRegion = new Vector4(0,0,200,220);
        Require(ColorAt(viewport.TransformPoint(new Vector3(-40,90,0))).r > .9f,"Clip region change did not reach rendering");
        maskTexture = new Texture2D(2,2); maskTexture.SetPixels(new[] {Color.clear,Color.clear,Color.clear,Color.clear}); maskTexture.Apply();
        clipPanel.clipping = 1; clipPanel.clipTexture = maskTexture;
        Require(ColorAt(left.transform.position).r < .05f,"Texture mask did not hide the graphic");
        clipPanel.clipTexture = Texture2D.whiteTexture;
        Require(ColorAt(left.transform.position).r > .9f,"Texture mask replacement was stale");
        Require(!left.clipping.masks[0].GetComponent<RectMask2D>().enabled && left.clipping.masks[0].GetComponent<Mask>().enabled,"Both mask modes enabled");
        clipPanel.clipping = 3; clipPanel.clipRegion = new Vector4(0,0,200,120); root.SynchronizePresentation();
        results.Add("PASS RectMask2D region/softness and Mask texture/mode changes affect actual rendering.");
        clipPanel.clipping = 0; root.SynchronizePresentation();
        Require(left.clipping.panels.Length == 0,"Removed clipping left an active mask chain");
        clipPanel.clipping = 3; root.SynchronizePresentation();
    }
    private static void CheckMaskRebind()
    {
        var originalGraphic = left.graphic;
        var newParent = Rect("New clipped parent", viewport,new Vector2(200,120));
        var panel = newParent.gameObject.AddComponent<GameUIPanel>(); panel.clipping = 3; panel.clipRegion = new Vector4(0,0,200,120);
        left.transform.SetParent(newParent,false); left.transform.localPosition = new Vector3(-40,0,0);
        root.SynchronizePresentation();
        Require(left.clipping.panels.Length == 1 && left.clipping.panels[0] == panel,"Reparent retained the old clipping panel");
        Require(left.graphic == originalGraphic && left.canvasBatch == null,"Rebind replaced Graphic identity or failed batch fallback");
        Require(ColorAt(left.transform.position).r > .9f,"Reparented masked widget stopped rendering");
        results.Add("PASS clip disable/re-enable and reparent rebuild generated mask containers while preserving Graphic identity and batch fallback.");
    }
    private static void CheckPositionTween()
    {
        var rect = left.layoutRect; var parent = (RectTransform)rect.parent;
        parent.sizeDelta = new Vector2(360,240); parent.pivot = new Vector2(.2f,.8f);
        rect.anchorMin = rect.anchorMax = new Vector2(.9f,.1f);
        var tween = left.gameObject.AddComponent<GameUITweenPosition>(); tween.enabled = false;
        tween.from = new Vector3(-40,-10,0); tween.to = new Vector3(20,30,0); tween.Sample(.5f,false);
        Require(Vector3.Distance(rect.localPosition,new Vector3(-10,10,0)) < .001f,"Migrated local tween changed coordinate space");
        tween.useAnchoredPosition = true; tween.Sample(.5f,false);
        Require(Vector3.Distance(rect.anchoredPosition3D,new Vector3(-10,10,0)) < .001f,"Anchored tween did not use RectTransform coordinates");
        tween.worldSpace = true; tween.from = rect.position; tween.to = rect.position + new Vector3(.05f,.03f,0); tween.Sample(1,true);
        Require(Vector3.Distance(rect.position,tween.to) < .0001f,"World tween mode changed");
        tween.worldSpace = tween.useAnchoredPosition = false; rect.anchorMin = rect.anchorMax = parent.pivot;
        var end = new Vector3(-25,0,0); GameUITweenPosition.Begin(left.gameObject,0,end);
        Require(Vector3.Distance(rect.localPosition,end) < .001f && ColorAt(left.transform.position).r > .9f,"Zero-duration tween or immediate unbatched capture stale");
        var scale = GameUITweenScale.Begin(left.gameObject,.001f,Vector3.one * .8f); scale.onFinished.Add(Action("Complete"));
        results.Add("PASS local, anchored and world position tweens, zero-duration endpoint and immediate unbatched masked capture.");
    }
    private static void CheckTweenCompletion()
    {
        Require(probe.completions == 1,"Tween completion callback must run exactly once");
        results.Add("PASS scale tween and serialized-style completion callback in real Play Mode.");
    }
    private static void CheckGroupFallback()
    {
        // Recreate a live batch to test native group ancestry, separate from reparent fallback.
        Object.Destroy(root.gameObject); Build();
        var native = left.gameObject.AddComponent<CanvasGroup>(); native.alpha = .4f;
        root.SynchronizePresentation();
        Require(left.canvasBatch == null && right.canvasBatch == null,"Owner CanvasGroup did not restore native ancestry");
    }
    private static void CheckFallbackCapture()
    {
        Near(ColorAt(left.transform.position).r,.4f,"CanvasGroup fallback applied wrong alpha",.025f);
        left.gameObject.SetActive(false);
        Require(ColorAt(left.transform.position).r < .05f,"Hidden owner exposed a presentation during capture");
        results.Add("PASS source CanvasGroup releases shared presentation to native ancestry; capture respects hidden owner.");
    }
    private static Color ColorAt(Vector3 world)
    {
        var camera = root.uiCamera; var old = camera.targetTexture; var active = RenderTexture.active;
        var rt = new RenderTexture(1280,720,24); var pixels = new Texture2D(1280,720,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture = rt; GameUIRoot.RenderForCapture(camera); RenderTexture.active = rt;
            pixels.ReadPixels(new Rect(0,0,1280,720),0,0); pixels.Apply();
            var point = camera.WorldToScreenPoint(world);
            return pixels.GetPixel(Mathf.Clamp(Mathf.RoundToInt(point.x),0,1279),Mathf.Clamp(Mathf.RoundToInt(point.y),0,719));
        }
        finally { camera.targetTexture = old; RenderTexture.active = active; Object.Destroy(pixels); Object.Destroy(rt); }
    }
    private static void Finish(int code)
    {
        Application.runInBackground = SessionState.GetBool(Key + "Background", false);
        if (maskTexture != null) Object.Destroy(maskTexture);
        results.Add(code == 0 ? "OVERALL PASS (isolated fixture; not a gameplay/device run)" : "FAILED");
        Directory.CreateDirectory(Path.GetDirectoryName(Report)); File.WriteAllLines(Report,results);
        SessionState.SetInt(Key + "Exit",code); SessionState.SetBool(Key + "Done",true); EditorApplication.ExitPlaymode();
    }
    private static void Near(float actual,float expected,string message,float epsilon=.001f) => Require(Mathf.Abs(actual-expected) <= epsilon,message + ": " + actual + " != " + expected);
    private static void Require(bool condition,string message) { if (!condition) throw new InvalidOperationException(message); }
}
#endif
