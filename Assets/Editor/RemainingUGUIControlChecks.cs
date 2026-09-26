#if UNITY_EDITOR
using System;
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

[InitializeOnLoad]
public static class RemainingUGUIControlChecks
{
    const string Key = "BB.RemainingControls";
    [Serializable] sealed class Setup { public SceneSetup[] scenes; }
    static int phase, nextFrame;
    static GameUIInput input;
    static CanvasConnectionProbe probe;
    static GameObject fixture;
    static Camera camera;
    static string report;
    static RemainingUGUIControlChecks() { EditorApplication.update += Tick; }
    public static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required.");
        for (int i=0;i<SceneManager.sceneCount;i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene changes first.");
        SessionState.SetString(Key, JsonUtility.ToJson(new Setup { scenes = EditorSceneManager.GetSceneManagerSetup() }));
        SessionState.SetBool(Key+"Done",false); SessionState.SetBool(Key+"Background",Application.runInBackground);
        SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        phase = nextFrame = 0; EditorApplication.EnterPlaymode();
    }
    static void Tick()
    {
        string setup = SessionState.GetString(Key, ""); if (setup.Length == 0) return;
        if (SessionState.GetBool(Key+"Done",false))
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SessionState.EraseString(Key); Application.runInBackground = SessionState.GetBool(Key+"Background",false);
                EditorSceneManager.RestoreSceneManagerSetup(JsonUtility.FromJson<Setup>(setup).scenes);
            }
            return;
        }
        try
        {
            if ((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",""))).TotalSeconds >= 120) throw new TimeoutException("Control fixture timed out");
            EditorApplication.QueuePlayerLoopUpdate();
            if (!EditorApplication.isPlaying || Time.frameCount < nextFrame) return;
            if (phase++ == 0) BuildAndCheck();
            else
            {
                Require(input.input.isFocused,"InputField receives focus through migrated hit surface");
                input.input.text = "UGUI 한글 123";
                Require(input.value == "UGUI 한글 123" && input.label.text == input.value,"InputField synchronizes Korean/Latin text");
                input.input.onEndEdit.Invoke(input.value); Require(probe.completions == 1,"Input submit callback once");
                Finish(null); return;
            }
            nextFrame = Time.frameCount+5;
        }
        catch (Exception e) { Debug.LogException(e); Finish(e); }
    }
    static void BuildAndCheck()
    {
        Application.runInBackground=true; report="";
        fixture=new GameObject("Remaining controls fixture"); fixture.SetActive(false);
        camera=new GameObject("Fixture camera",typeof(Camera)).GetComponent<Camera>(); camera.orthographic=true; camera.orthographicSize=360;
        camera.transform.position=new Vector3(0,0,-100); camera.nearClipPlane=.01f; camera.farClipPlane=2000;
        var events=new GameObject("Fixture EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        probe=fixture.AddComponent<CanvasConnectionProbe>();
        var inventory=JsonUtility.FromJson<RemainingUGUIMigration.Inventory>(File.ReadAllText(RemainingUGUIMigration.Output+"/inventory.json"));
        var paths=inventory.assets.Where(a=>a.path.EndsWith(".prefab") && a.components.Length>0).Select(a=>a.path).ToArray();
        var first=CloneControl<GameUIToggle>(paths,"Toggle A"); var second=CloneControl<GameUIToggle>(paths,"Toggle B");
        var progress=CloneControl<GameUIProgress>(paths,"Slider", p=>p.slider.interactable && p.slider.fillRect!=null && p.fillArea!=null);
        input=CloneControl<GameUIInput>(paths,"Input"); input.savedAs="";
        first.group=second.group=987654; first.optionCanBeNone=second.optionCanBeNone=false;
        first.toggle.SetIsOnWithoutNotify(false); second.toggle.SetIsOnWithoutNotify(false);
        first.onChange.Add(new GameUIAction {mTarget=probe,mMethodName="Click"});
        input.onSubmit.Add(new GameUIAction {mTarget=probe,mMethodName="Complete"});
        fixture.SetActive(true); Canvas.ForceUpdateCanvases();
        Click(first.GetComponent<GameUIPointer>());
        Require(first.value && probe.clicks==1,"One pointer click toggles once and invokes one callback");
        Click(second.GetComponent<GameUIPointer>());
        Require(second.value && !first.value,"Radio group deselects peer");
        Click(second.GetComponent<GameUIPointer>()); Require(second.value,"Radio group cannot become empty");
        int changes=0; progress.slider.onValueChanged.AddListener(_=>changes++);
        progress.steps=5; progress.value=.61f;
        Require(Mathf.Abs(progress.value-.5f)<.001f,"Slider quantizes to original step count");
        foreach (float value in new[]{0f,.5f,1f})
        {
            progress.value=value; Canvas.ForceUpdateCanvases();
            var fill=progress.slider.fillRect;
            float fraction=progress.slider.direction==Slider.Direction.LeftToRight || progress.slider.direction==Slider.Direction.RightToLeft ? fill.anchorMax.x-fill.anchorMin.x : fill.anchorMax.y-fill.anchorMin.y;
            if (fill.TryGetComponent<Image>(out var image) && image.type==Image.Type.Filled) fraction=image.fillAmount;
            Require(Mathf.Abs(fraction-value)<.001f,"Slider fill geometry="+value);
        }
        progress.steps=0;
        var pointer=progress.GetComponent<GameUIPointer>(); var hit=pointer.GetComponentInChildren<GameUIHitTarget>(); hit.Synchronize();
        var area=progress.fillArea; var world=area.TransformPoint(new Vector3(area.rect.xMin+area.rect.width*.75f,area.rect.center.y,0));
        var data=Data(hit,camera.WorldToScreenPoint(world));
        ExecuteEvents.ExecuteHierarchy(hit.gameObject,data,ExecuteEvents.pointerDownHandler);
        ExecuteEvents.ExecuteHierarchy(hit.gameObject,data,ExecuteEvents.pointerUpHandler);
        Require(changes>0 && Mathf.Abs(progress.value-.75f)<.03f,"Slider pointer position maps to 75 percent");
        var clipHost=new GameObject("Dynamic scroll host").AddComponent<GameUIPanel>(); clipHost.transform.SetParent(fixture.transform,false);
        clipHost.clipping=3; clipHost.clipRegion=new Vector4(0,0,400,200);
        progress.transform.SetParent(clipHost.transform,false);
        var dynamicUI=progress.gameObject.AddComponent<GameUIDynamicUI>(); dynamicUI.RefreshUGUIHierarchy();
        progress.value=.5f; progress.foreground.Apply(); Canvas.ForceUpdateCanvases();
        Require(progress.foreground.clipping!=null && progress.slider.fillRect.parent==progress.fillArea,"Dynamically loaded slider retains its fill area inside the host mask");
        Require(Mathf.Abs(progress.slider.fillRect.anchorMax.x-progress.slider.fillRect.anchorMin.x-.5f)<.001f,"Clipped dynamic slider still renders 50 percent");
        var target=new GameObject("Anchor reference").AddComponent<GameUIElement>(); target.transform.SetParent(fixture.transform,false); target.SetDimensions(200,100);
        var anchored=new GameObject("Anchored widget").AddComponent<GameUIElement>(); anchored.transform.SetParent(fixture.transform,false);
        var anchors=anchored.gameObject.AddComponent<GameUIAnchors>(); anchors.element=anchored;
        anchors.left=new GameUIAnchors.Edge{target=target.transform,relative=0,absolute=10}; anchors.right=new GameUIAnchors.Edge{target=target.transform,relative=1,absolute=-10};
        anchors.bottom=new GameUIAnchors.Edge{target=target.transform,relative=0,absolute=5}; anchors.top=new GameUIAnchors.Edge{target=target.transform,relative=1,absolute=-5};
        anchors.SendMessage("LateUpdate"); Require(anchored.width==180 && anchored.height==90,"Anchors retain edge offsets");
        target.SetDimensions(400,200); anchors.SendMessage("LateUpdate"); Require(anchored.width==380 && anchored.height==190,"Anchors respond to resizing");
        CheckDynamicScroll();
        Click(input.GetComponent<GameUIPointer>());
    }
    static void CheckDynamicScroll()
    {
        var canvas = new GameObject("Dynamic list canvas",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.transform.SetParent(fixture.transform,false); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var viewport=(RectTransform)new GameObject("List viewport",typeof(RectTransform)).transform;
        viewport.SetParent(canvas.transform,false); viewport.sizeDelta=new Vector2(220,350);
        var content=(RectTransform)new GameObject("Empty list content",typeof(RectTransform)).transform;
        content.SetParent(viewport,false); content.sizeDelta=Vector2.zero;
        var native=viewport.gameObject.AddComponent<ScrollRect>(); native.viewport=viewport; native.content=content;
        native.horizontal=false; native.vertical=true; native.movementType=ScrollRect.MovementType.Clamped;
        var scroll=content.gameObject.AddComponent<GameUIScroll>(); scroll.scrollRect=native;
        var rows=new GameUIElement[11];
        for(int i=0;i<rows.Length;i++)
        {
            rows[i]=new GameObject("Runtime row "+i,typeof(RectTransform)).AddComponent<GameUIElement>();
            rows[i].transform.SetParent(content,false); rows[i].transform.localPosition=new Vector3(0,150-42*i,0);
            rows[i].SetDimensions(200,40);
        }
        scroll.RefreshContentBounds();
        Require(Mathf.Abs(content.rect.height-460)<.001f,"Dynamic list bounds include all 11 instantiated rows");
        Require(rows[10].transform.localPosition==new Vector3(0,-270,0),"Bounds refresh preserves anchored child positions");
        scroll.ResetPosition(); Canvas.ForceUpdateCanvases(); var start=content.anchoredPosition;
        Vector2 point=RectTransformUtility.WorldToScreenPoint(null,viewport.position);
        var data=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=point};
        data.pointerPressRaycast=new RaycastResult{gameObject=viewport.gameObject,module=canvas.GetComponent<GraphicRaycaster>(),screenPosition=point};
        scroll.OnBeginDrag(data); data.position+=new Vector2(0,60); scroll.OnDrag(data); scroll.OnEndDrag(data);
        Require(content.anchoredPosition.y>start.y+1,"Native ScrollRect drag reaches dynamically added rows");
        rows[10].gameObject.SetActive(false); scroll.RefreshContentBounds();
        Require(Mathf.Abs(content.rect.height-418)<.001f,"Dynamic list bounds shrink when a row is removed");
        var nested=new GameObject("Nested scroll").AddComponent<GameUIScroll>(); nested.transform.SetParent(content,false);
        var nestedRow=new GameObject("Nested row").AddComponent<GameUIElement>(); nestedRow.transform.SetParent(nested.transform,false);
        nestedRow.SetDimensions(200,2000); scroll.RefreshContentBounds();
        Require(Mathf.Abs(content.rect.height-418)<.001f,"Outer list bounds exclude nested scroll content");
        foreach(var row in rows) row.gameObject.SetActive(false);
        scroll.RefreshContentBounds(); Require(content.rect.size==Vector2.zero,"Empty dynamic list clears stale scroll bounds");
    }
    static T CloneControl<T>(string[] paths,string name,Func<T,bool> extra=null) where T:MonoBehaviour
    {
        foreach(string path in paths)
        foreach(var source in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<T>(true))
        {
            if (source.GetComponent<GameUIPointer>()?.inputCollider==null || extra!=null && !extra(source)) continue;
            if (source.GetComponentsInChildren<MonoBehaviour>(true).Any(c=>c==null || c.GetType().Namespace!=typeof(GameUIElement).Namespace && c.GetType().Namespace!="UnityEngine.UI" && c.GetType().Namespace!="TMPro")) continue;
            var clone=Object.Instantiate(source.gameObject,fixture.transform,false); clone.name=name; clone.SetActive(true); clone.transform.localPosition=Vector3.zero; clone.transform.localScale=Vector3.one;
            foreach(var canvas in clone.GetComponentsInChildren<Canvas>(true)) canvas.worldCamera=camera;
            foreach(var p in clone.GetComponentsInChildren<GameUIPointer>(true)) {p.onClick.Clear();p.onPress.Clear();p.onRelease.Clear();p.onHoverOver.Clear();p.onHoverOut.Clear();p.onDrag.Clear();p.onDragStart.Clear();p.onDragEnd.Clear();p.onDoubleClick.Clear();}
            foreach(var t in clone.GetComponentsInChildren<GameUIToggle>(true)) t.onChange.Clear();
            foreach(var p in clone.GetComponentsInChildren<GameUIProgress>(true)) p.onChange.Clear();
            foreach(var i in clone.GetComponentsInChildren<GameUIInput>(true)) {i.onSubmit.Clear();i.onChange.Clear();i.savedAs="";}
            report+="SOURCE "+name+" "+path+" / "+source.name+"\n";
            return clone.GetComponent<T>();
        }
        throw new InvalidOperationException("No isolated converted control: "+typeof(T).Name);
    }
    static PointerEventData Data(GameUIHitTarget hit,Vector2 point)
    {
        var data=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=point};
        data.pointerPressRaycast=new RaycastResult{gameObject=hit.gameObject,module=hit.GetComponent<GraphicRaycaster>(),screenPosition=point}; return data;
    }
    static void Click(GameUIPointer pointer)
    {
        var hit=pointer.GetComponentInChildren<GameUIHitTarget>(); hit.Synchronize(); var data=Data(hit,camera.WorldToScreenPoint(hit.transform.position));
        ExecuteEvents.ExecuteHierarchy(hit.gameObject,data,ExecuteEvents.pointerDownHandler); ExecuteEvents.ExecuteHierarchy(hit.gameObject,data,ExecuteEvents.pointerUpHandler); ExecuteEvents.ExecuteHierarchy(hit.gameObject,data,ExecuteEvents.pointerClickHandler);
    }
    static void Require(bool condition,string label) { if(!condition) throw new InvalidOperationException(label); report+="PASS "+label+"\n"; }
    static void Finish(Exception error)
    {
        File.WriteAllText(RemainingUGUIMigration.Output+"/control-play-checks.txt",(error==null?"PASS":"FAIL")+"\n"+report+(error==null?"":error.ToString()));
        SessionState.SetBool(Key+"Done",true); EditorApplication.ExitPlaymode();
    }
}
#endif
