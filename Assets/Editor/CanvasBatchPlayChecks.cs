#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using BaseBall.BallPlay.UGUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// An isolated UI fixture; does not start gameplay, change project settings, or save scenes.
[InitializeOnLoad]
public static class CanvasBatchPlayChecks
{
    private const string Key = "CanvasBatchPlayChecks";
    private const string Report = "Docs/UIAudit/CanvasRestructure/Step3/play-checks.txt";
    private static readonly List<string> results = new List<string>();
    private static GameUIRoot root;
    private static RectTransform content;
    private static GameUICanvasBatch batch;
    private static GameUIElement left, right;
    private static GameObject removedPresentation;
    private static int phase, nextFrame;

    static CanvasBatchPlayChecks() { EditorApplication.update += Tick; }
    public static void Start()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a separate batch editor.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Key, true); SessionState.SetBool(Key + "Done", false);
        SessionState.SetString(Key + "Start", DateTime.UtcNow.ToString("O"));
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
                EditorApplication.Exit(SessionState.GetInt(Key + "Exit", 1));
            }
            return;
        }
        try
        {
            Require((DateTime.UtcNow - DateTime.Parse(SessionState.GetString(Key + "Start", ""))).TotalSeconds < 150, "Play fixture timed out");
            if (!EditorApplication.isPlaying || Time.frameCount < nextFrame) return;
            switch (phase++)
            {
                case 0:
                    results.Add(DateTime.UtcNow.ToString("O") + " isolated UGUI Play Mode fixture");
                    Build();
                    break;
                case 1:
                    Require(left.canvasBatch == batch && right.canvasBatch == batch, "Initial batch released");
                    Require(left.graphic.canvas == right.graphic.canvas && root.GetComponentsInChildren<Canvas>(true).Length == 2, "Widgets do not share one canvas");
                    Require(ColorAt(left).r > .95f && ColorAt(right).b > .95f, "Shared widgets did not render");
                    results.Add("PASS shared Canvas renders both widgets.");
                    left.transform.localPosition = new Vector3(-180, 0, 0);
                    Require(ColorAt(left, true).r > .95f, "Immediate capture used a stale presentation transform");
                    results.Add("PASS manual capture synchronizes source movement before LateUpdate.");
                    left.layoutRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 130);
                    left.height = 140;
                    content.localScale = new Vector3(.6f, 1.2f, 1);
                    content.localRotation = Quaternion.Euler(0, 0, 37);
                    content.sizeDelta = new Vector2(200,120); content.pivot = new Vector2(.2f,.7f);
                    left.transform.localPosition = new Vector3(45, -29, 0);
                    left.transform.localRotation = Quaternion.Euler(0, 0, 16);
                    left.transform.localScale = new Vector3(.7f, 1.4f, 1);
                    break;
                case 2:
                    Require(left.width == 130 && left.height == 140 && left.mWidth == 130, "RectTransform dimensions were overwritten");
                    CheckPose(left); CheckPose(right);
                    results.Add("PASS RectTransform edits and size API persist; shared presentation follows translation, rotation and nonuniform scale exactly.");
                    content.localScale = Vector3.one; content.localRotation = Quaternion.identity;
                    content.sizeDelta = Vector2.zero; content.pivot = Vector2.one * .5f;
                    left.transform.localPosition = new Vector3(-60, 0, 0); left.transform.localRotation = Quaternion.identity; left.transform.localScale = Vector3.one;
                    left.gameObject.SetActive(false); right.enabled = false;
                    Require(!left.presentationRoot.gameObject.activeSelf && !right.presentationRoot.gameObject.activeSelf, "Disable left a visible presentation");
                    break;
                case 3:
                    Require(!batch.displayCanvas.enabled && ColorAt(left).maxColorComponent < .05f, "Hidden batch still renders");
                    root.gameObject.SetActive(false);
                    Require(EventSystem.current != null && EventSystem.current.isActiveAndEnabled, "UI root hide disabled EventSystem");
                    root.gameObject.SetActive(true); left.gameObject.SetActive(true); right.enabled = true;
                    break;
                case 4:
                    var red = ColorAt(left); var blue = ColorAt(right);
                    Require(batch.displayCanvas.enabled && red.r > .95f && blue.b > .95f, "Widgets failed to reappear: canvas=" + batch.displayCanvas.enabled + " batch=" + (left.canvasBatch != null) + "," + (right.canvasBatch != null) + " colors=" + red + "," + blue);
                    results.Add("PASS GameObject/component disable and root hide/show remove and restore graphics; EventSystem survives.");
                    left.depth++;
                    break;
                case 5:
                    CheckReleased();
                    Require(left.displayCanvas.sortingOrder > right.displayCanvas.sortingOrder, "Changed depth did not update draw order");
                    Require(ColorAt(left).r > .95f && ColorAt(right).b > .95f, "Fallback lost a graphic");
                    results.Add("PASS changing depth releases the whole batch, preserves rendering and updates order.");
                    Object.Destroy(root.gameObject);
                    Build();
                    break;
                case 6:
                    var parent = Rect("Moved owner", content);
                    parent.localRotation = Quaternion.Euler(0, 0, -23); parent.localScale = new Vector3(1.3f, .8f, 1);
                    left.transform.SetParent(parent, false);
                    break;
                case 7:
                    CheckReleased(); CheckPose(left); CheckPose(right);
                    results.Add("PASS reparenting releases the batch and retains the new parent transform.");
                    Object.Destroy(root.gameObject);
                    Build();
                    break;
                case 8:
                    removedPresentation = left.presentationRoot.gameObject;
                    Object.Destroy(left.gameObject);
                    break;
                case 9:
                    Require(removedPresentation == null && right.canvasBatch == batch && ColorAt(right).b > .95f, "Destroy left orphaned graphics or removed the other member");
                    results.Add("PASS destroying a widget removes its detached presentation without affecting its sibling.");
                    Finish(0);
                    return;
            }
            nextFrame = Time.frameCount + 2;
        }
        catch (Exception error) { results.Add(error.ToString()); Finish(1); }
    }

    private static void Build()
    {
        var owner = new GameObject("Canvas batch fixture"); owner.SetActive(false);
        root = owner.AddComponent<GameUIRoot>();
        var cameraObject = new GameObject("UI Camera", typeof(Camera)); cameraObject.transform.SetParent(owner.transform, false);
        var camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 1;
        camera.transform.localPosition = new Vector3(0, 0, -1); camera.nearClipPlane = .01f; camera.farClipPlane = 10;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
        var canvasRect = Rect("Canvas", owner.transform);
        var canvas = canvasRect.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera; canvas.planeDistance = 1;
        var scaler = canvasRect.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = 1;
        root.rootCanvas = canvas; root.uiCamera = camera;
        content = Rect("Content", canvasRect); var panel = content.gameObject.AddComponent<GameUIPanel>();
        var batchRect = Rect("Shared Canvas", content); var shared = batchRect.gameObject.AddComponent<Canvas>(); shared.overrideSorting = true;
        batch = batchRect.gameObject.AddComponent<GameUICanvasBatch>(); batch.displayCanvas = shared;
        batch.panel = panel; batch.sourceParent = content; batch.depth = 5;
        left = Element("Left", -60, Color.red, batchRect); right = Element("Right", 60, Color.blue, batchRect);
        batch.members = new[] { left, right };
        owner.SetActive(true);
    }

    private static RectTransform Rect(string name, Transform parent)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false); rect.sizeDelta = Vector2.zero; return rect;
    }
    private static GameUIElement Element(string name, float x, Color color, RectTransform batchRect)
    {
        var rect = Rect(name, content); rect.sizeDelta = new Vector2(100,100); rect.localPosition = new Vector3(x,0,0);
        var e = rect.gameObject.AddComponent<GameUIElement>(); e.kind = GameUIElement.ElementKind.Texture;
        e.layoutRect = rect; e.mWidth = e.mHeight = 100; e.mColor = color; e.mDepth = 5; e.mTexture = Texture2D.whiteTexture;
        e.presentationRoot = Rect(name + " presentation", batchRect); e.canvasBatch = batch; e.displayCanvas = batch.displayCanvas;
        e.graphic = Rect("Graphic", e.presentationRoot).gameObject.AddComponent<RawImage>();
        return e;
    }
    private static void CheckPose(GameUIElement e)
    {
        foreach (var point in new[] { Vector3.zero, new Vector3(50,35,0), new Vector3(-40,-25,0) })
            Require(Vector3.Distance(e.transform.TransformPoint(point), e.presentationRoot.TransformPoint(point)) < .00001f, "Presentation changed the affine transform");
    }
    private static void CheckReleased()
    {
        Require(left.canvasBatch == null && right.canvasBatch == null && !batch.displayCanvas.enabled, "Batch did not release all members");
        Require(left.presentationRoot.parent == left.transform && right.presentationRoot.parent == right.transform, "Fallback changed ownership");
        Require(left.graphic.canvas == left.displayCanvas && right.graphic.canvas == right.displayCanvas, "Fallback canvas not bound");
        Require(left.displayCanvas.worldCamera == root.uiCamera && right.displayCanvas.worldCamera == root.uiCamera, "Fallback camera changed");
    }
    private static Color ColorAt(GameUIElement e, bool capture = false)
    {
        var camera = root.uiCamera; var old = camera.targetTexture; var active = RenderTexture.active;
        var rt = new RenderTexture(1280,720,24); var pixels = new Texture2D(1280,720,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture = rt; Canvas.ForceUpdateCanvases();
            if (capture) GameUIRoot.RenderForCapture(camera); else camera.Render();
            RenderTexture.active = rt;
            pixels.ReadPixels(new Rect(0,0,1280,720),0,0); pixels.Apply();
            var point = camera.WorldToScreenPoint(e.transform.position);
            return pixels.GetPixel(Mathf.Clamp(Mathf.RoundToInt(point.x),0,1279),Mathf.Clamp(Mathf.RoundToInt(point.y),0,719));
        }
        finally { camera.targetTexture = old; RenderTexture.active = active; Object.Destroy(pixels); Object.Destroy(rt); }
    }
    private static void Finish(int code)
    {
        results.Add(code == 0 ? "OVERALL PASS (UI fixture only; not a gameplay/device run)" : "FAILED");
        Directory.CreateDirectory(Path.GetDirectoryName(Report)); File.WriteAllLines(Report, results);
        SessionState.SetInt(Key + "Exit", code); SessionState.SetBool(Key + "Done", true);
        EditorApplication.ExitPlaymode();
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
#endif
