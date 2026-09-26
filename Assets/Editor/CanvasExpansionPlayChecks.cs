#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseBall.BallPlay;
using BaseBall.BallPlay.UGUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class CanvasExpansionPlayChecks
{
    private const string Key = "CanvasExpansionPlayChecks";
    private const string Report = "Docs/UIAudit/CanvasRestructure/Step5/play-checks.txt";
    [Serializable] private class SavedSetup { public SceneSetup[] scenes; }
    private static readonly List<string> results = new List<string>();
    private static GameUIRoot first, second;
    private static GameUIPanel firstPanel, secondPanel;
    private static GameObject background, buff, holder;
    private static Texture2D clearMask;
    private static int phase,nextFrame;
    static CanvasExpansionPlayChecks() { EditorApplication.update += Tick; }
    public static void Start()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play Mode first.");
        for (int i=0;i<SceneManager.sceneCount;i++) Require(!SceneManager.GetSceneAt(i).isDirty,"Save scene changes before testing.");
        phase=nextFrame=0; results.Clear();
        SessionState.SetString(Key+"Setup",JsonUtility.ToJson(new SavedSetup { scenes=EditorSceneManager.GetSceneManagerSetup() }));
        SessionState.SetBool(Key+"Background",Application.runInBackground);
        SessionState.SetBool(Key,true); SessionState.SetBool(Key+"Done",false);
        SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Key,false)) return;
        if (SessionState.GetBool(Key+"Done",false))
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SessionState.SetBool(Key,false);
                if (Application.isBatchMode) EditorApplication.Exit(SessionState.GetInt(Key+"Exit",1));
                else EditorSceneManager.RestoreSceneManagerSetup(JsonUtility.FromJson<SavedSetup>(SessionState.GetString(Key+"Setup","")).scenes);
            }
            return;
        }
        try
        {
            Require((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",""))).TotalSeconds<180,"Fixture timed out");
            if (!EditorApplication.isPlayingOrWillChangePlaymode) { Finish(1); return; }
            EditorApplication.QueuePlayerLoopUpdate();
            if (!EditorApplication.isPlaying || Time.frameCount<nextFrame) return;
            switch(phase++)
            {
                case 0:
                    results.Add(DateTime.UtcNow.ToString("O")+" isolated dynamic UI fixture; resources loaded with the production Util.Load path.");
                    Application.runInBackground=true; EditorApplication.isPaused=false;
                    first=Host("Main",Vector3.zero,4,out firstPanel); second=Host("Quick",new Vector3(4,0,0),6,out secondPanel);
                    CheckCatalog(); break;
                case 1: CheckTextureAndMirror(); break;
                case 2: CheckHostChange(); break;
                case 3: CheckBatchAndLifecycle(); break;
                case 4: Object.Destroy(holder); Object.Destroy(background); break;
                case 5:
                    Require(second.GetComponentsInChildren<Canvas>(true).Length==1,"Destroyed dynamic UI left canvases behind");
                    Require(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Dynamic UI duplicated EventSystem");
                    results.Add("PASS dynamic subtree destruction removes detached presentations; one independent EventSystem remains.");
                    CheckProductionRoutes(); break;
                case 6: CheckPopupInputAndManualRoute(); Finish(0); return;
            }
            nextFrame=Time.frameCount+3;
        }
        catch(Exception e) { results.Add(e.ToString()); Debug.LogException(e); Finish(1); }
    }
    private static RectTransform Rect(string name,Transform parent)
    {
        var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;
        rect.SetParent(parent,false); rect.sizeDelta=Vector2.zero; return rect;
    }
    private static GameUIRoot Host(string name,Vector3 position,int depth,out GameUIPanel panel)
    {
        var owner=new GameObject(name); owner.SetActive(false); owner.transform.position=position;
        var root=owner.AddComponent<GameUIRoot>();
        var cameraObject=new GameObject("Camera",typeof(Camera)); cameraObject.transform.SetParent(owner.transform,false);
        var camera=cameraObject.GetComponent<Camera>(); camera.transform.localPosition=new Vector3(0,0,-1);
        camera.orthographic=true; camera.orthographicSize=1; camera.nearClipPlane=-9; camera.farClipPlane=11; camera.depth=depth;
        camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=Color.black;
        var rect=Rect("Canvas",owner.transform); var canvas=rect.gameObject.AddComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1;
        rect.gameObject.AddComponent<GraphicRaycaster>();
        var scaler=rect.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=1;
        root.rootCanvas=canvas; root.uiCamera=camera;
        root.hudLayer=Rect("HUD",rect); root.effectsLayer=Rect("Effects",rect); root.popupLayer=Rect("Popups",rect);
        panel=root.hudLayer.gameObject.AddComponent<GameUIPanel>(); panel.clipping=3; panel.clipRegion=new Vector4(0,0,200,120);
        owner.SetActive(true); return root;
    }
    private static GameObject Load(string path,Transform parent)
    {
        string resource=path.Replace("Assets/Resources/","").Replace(".prefab","");
        var instance=Util.Load(resource,parent,Vector3.zero); instance.transform.localScale=Vector3.one;
        foreach(var node in instance.GetComponentsInChildren<Transform>(true)) node.gameObject.SetActive(true);
        foreach(var tween in instance.GetComponentsInChildren<GameUITween>(true)) tween.enabled=false;
        return instance;
    }
    private static void CheckCatalog()
    {
        int widgets=0,batches=0;
        foreach(string path in CanvasExpansionChecks.Assets)
        {
            var instance=Load(path,first.hudLayer); first.SynchronizePresentation();
            Require(instance.transform is RectTransform && instance.GetComponent<GameUIDynamicUI>()!=null,path+" root binding missing");
            var elements=instance.GetComponentsInChildren<GameUIElement>(true); widgets+=elements.Length;
            foreach(var e in elements)
            {
                Require(e.layoutRect==e.transform,path+" layoutRect missing");
                Require(e.Panel==e.GetComponentInParent<GameUIPanel>(true),path+" inherited panel stale");
                if(e.graphic!=null) Require(e.clipping!=null && e.clipping.panels.Contains(firstPanel),path+" host clipping missing");
            }
            foreach(var c in instance.GetComponentsInChildren<Canvas>(true)) Require(c.worldCamera==first.uiCamera,path+" chose the other root camera");
            foreach(var batch in instance.GetComponentsInChildren<GameUICanvasBatch>(true))
            {
                batches++; Require(batch.enabled && batch.members.All(e=>e.canvasBatch==batch),path+" released its batch at attachment");
            }
            Object.Destroy(instance);
        }
        Require(widgets==42 && batches==5,"Unexpected catalog coverage: "+widgets+"/"+batches);
        results.Add("PASS 14 resource prefabs, 42 RectTransform widgets, 5 live batches: parent panel/camera/rectangular clip inherited after Util.Load; other root camera not selected.");
    }
    private static void CheckTextureAndMirror()
    {
        firstPanel.clipping=1; firstPanel.clipTexture=Texture2D.whiteTexture;
        background=Load("Assets/Resources/MainGame/prefabs/skillUI/bg/bg_Line.prefab",first.hudLayer);
        float visible=Brightness(first); Require(visible>1,"Skill background was not visible through opaque texture mask");
        clearMask=new Texture2D(2,2); clearMask.SetPixels(new[]{Color.clear,Color.clear,Color.clear,Color.clear}); clearMask.Apply();
        firstPanel.clipTexture=clearMask; Require(Brightness(first)<.01f,"Dynamic skill background bypassed texture mask");
        firstPanel.clipTexture=Texture2D.whiteTexture; background.transform.localScale=new Vector3(-1,1,1);
        Require(Brightness(first)>1,"Mirrored skill background disappeared inside the clip");
        var element=background.GetComponentInChildren<GameUIElement>();
        Require(Vector3.Distance(element.clipping.presentation.position,element.transform.position)<.0001f,"Mirrored presentation moved");
        firstPanel.clipping=0; first.SynchronizePresentation(); Require(element.clipping.panels.Length==0,"Disabling host clip retained masks");
        firstPanel.clipping=3; first.SynchronizePresentation(); Require(element.clipping.panels.Contains(firstPanel),"Re-enabling clip failed");
        results.Add("PASS actual skill background renders through texture mask, disappears through transparent mask, mirrors horizontally and follows clip mode changes.");
    }
    private static void CheckHostChange()
    {
        holder=Rect("Moving parent",first.hudLayer).gameObject;
        buff=Load("Assets/Resources/MainGame/prefabs/QuickUI/actionBuffPrefab.prefab",holder.transform);
        first.SynchronizePresentation();
        secondPanel.alpha=.25f;
        holder.transform.SetParent(second.hudLayer,false); background.transform.SetParent(second.hudLayer,false);
        second.SynchronizePresentation();
        foreach(var e in buff.GetComponentsInChildren<GameUIElement>(true))
        {
            Require(e.Panel==secondPanel,"Ancestor reparent retained old panel");
            if(e.graphic!=null)
            {
                Require(e.clipping.panels.Contains(secondPanel) && !e.clipping.panels.Contains(firstPanel),"Ancestor reparent retained old clipping");
                Require(Mathf.Abs(e.graphic.color.a-e.CalculateFinalAlpha())<.0001f,"New panel alpha missing");
            }
        }
        foreach(var c in holder.GetComponentsInChildren<Canvas>(true)) Require(c.worldCamera==second.uiCamera,"Ancestor reparent retained old camera");
        var batch=buff.GetComponentInChildren<GameUICanvasBatch>(true);
        Require(batch.enabled && batch.panel==secondPanel,"Attachment unnecessarily released the dynamic batch");
        Require(Brightness(second)>0,"Capture lost transferred dynamic UI");
        results.Add("PASS ancestor reparent between UI roots refreshes camera, panel alpha, clip chain and shared batch before immediate capture.");
    }
    private static void CheckBatchAndLifecycle()
    {
        var batch=buff.GetComponentInChildren<GameUICanvasBatch>(true);
        var element=batch.members[0]; var other=batch.members[1];
        element.gameObject.SetActive(false); second.SynchronizePresentation();
        Require(!element.presentationRoot.gameObject.activeSelf && other.presentationRoot.gameObject.activeSelf,"Hiding one dynamic widget hid its sibling or left stale presentation");
        element.gameObject.SetActive(true); element.depth++; second.SynchronizePresentation();
        Require(element.canvasBatch==null && other.canvasBatch==null,"Dynamic depth change did not preserve order with fallback");
        Require(element.displayCanvas.worldCamera==second.uiCamera,"Fallback lost transferred camera");
        results.Add("PASS dynamic member hide/show and depth fallback keep sibling visibility and the correct camera.");
    }
    private static void CheckProductionRoutes()
    {
        var quick=first.gameObject.AddComponent<QuickSimulator>(); quick.enabled=false;
        quick.pauseGame();
        var popup=first.popupLayer.GetComponentInChildren<UIQuit>(true);
        Require(popup!=null && popup.transform.parent==first.PopupParent && popup.transform.localScale==Vector3.one,"Quick quit popup spawned outside the pixel canvas");
        typeof(QuickSimulator).GetMethod("LoadDynamicUI",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(quick,new object[]{"playballPrefab",100f,30f,new Vector3(-63,-47,0)});
        Require(first.effectsLayer.childCount==1 && first.effectsLayer.GetChild(0).localScale==Vector3.one*100,"Quick effect did not use Effects pixels/scale");
    }
    private static void CheckPopupInputAndManualRoute()
    {
        var popup=first.popupLayer.GetComponentInChildren<UIQuit>(true);
        first.SynchronizePresentation();
        var target=popup.GetComponentsInChildren<GameUIHitTarget>().First(h=>h.pointer.Available && h.pointer.name=="no");
        foreach(var h in popup.GetComponentsInChildren<GameUIHitTarget>()) h.Synchronize(); Canvas.ForceUpdateCanvases();
        var data=new PointerEventData(EventSystem.current) { position=RectTransformUtility.WorldToScreenPoint(first.uiCamera,target.transform.position) };
        var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(data,hits);
        Require(hits.Any(h=>h.gameObject==target.gameObject),"Quick quit popup raycast: owner="+target.pointer.name+" world="+target.transform.position+" screen="+data.position+" canvasDepth="+target.depth+" camera="+target.canvas.worldCamera+" available="+target.pointer.Available+" valid="+target.IsRaycastLocationValid(data.position,first.uiCamera)+" hits="+string.Join(",",hits.Select(h=>h.gameObject.name)));
        var pause=Rect("Pause",first.hudLayer).gameObject.AddComponent<UIPause>();
        var mode=Mode.gameMode;
        try { Mode.gameMode=Mode.GamePlayMode.NineInningTwoOut; Require(pause.SetPause(null),"Pause fixture rejected popup"); }
        finally { Mode.gameMode=mode; }
        Require(first.popupLayer.GetComponentsInChildren<UIQuit>(true).Length==2,"Manual pause quit popup did not use Popups layer");
        results.Add("PASS production Quick pause/effect and manual pause popup paths use Popups/Effects; real popup GraphicRaycaster hit succeeds. Destructive quit callback not invoked.");
    }
    private static float Brightness(GameUIRoot root)
    {
        var camera=root.uiCamera; var old=camera.targetTexture; var active=RenderTexture.active;
        var rt=new RenderTexture(1280,720,24); var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=rt; GameUIRoot.RenderForCapture(camera); RenderTexture.active=rt;
            image.ReadPixels(new Rect(0,0,1280,720),0,0); image.Apply();
            double total=0; foreach(var c in image.GetPixels32()) total+=c.r+c.g+c.b;
            return (float)(total/255);
        }
        finally { camera.targetTexture=old; RenderTexture.active=active; Object.Destroy(image); Object.Destroy(rt); }
    }
    private static void Finish(int code)
    {
        Application.runInBackground=SessionState.GetBool(Key+"Background",false);
        if(clearMask!=null) Object.Destroy(clearMask);
        results.Add(code==0 ? "OVERALL PASS (isolated dynamic UI fixture; full match and device validation are step 6)" : "FAILED");
        Directory.CreateDirectory(Path.GetDirectoryName(Report)); File.WriteAllLines(Report,results);
        SessionState.SetInt(Key+"Exit",code); SessionState.SetBool(Key+"Done",true); EditorApplication.ExitPlaymode();
    }
    private static void Require(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
}
#endif
