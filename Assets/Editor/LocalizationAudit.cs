#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BaseBall.BallPlay.UGUI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Read-only asset inspection. Produces a report in Library without saving game assets.
[InitializeOnLoad]
public static class LocalizationAudit
{
    private const string Folder="Library/Localization";
    [Serializable] private class Row { public string asset,path,type,text,key; public long componentId; public long objectId; }
    [Serializable] private class Report { public int assets; public Row[] rows; }
    static LocalizationAudit(){EditorApplication.update+=Poll;}
    private static void Poll()
    {
        string request=Folder+"/request.txt";
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(request))return;
        string command;
        try{command=File.ReadAllText(request).Trim();File.Delete(request);}catch(IOException){return;}
        try
        {
            if(command=="audit")Run();
            else if(command=="ownership")LocalizationOwnershipAudit.Run();
            else if(command=="test")LocalizationChecks.Run();
            else if(command=="scope-check")LocalizationScopeChecks.Run();
            else if(command=="state")
            {
                var stage=PrefabStageUtility.GetCurrentPrefabStage();
                File.WriteAllText(Folder+"/editor-state.txt",stage==null?"Main stage":"Prefab stage: "+stage.assetPath+"; dirty="+stage.scene.isDirty);
            }
            else throw new InvalidOperationException("Unknown audit command: "+command);
            File.WriteAllText(Folder+"/result.txt","PASS "+command+" "+DateTime.UtcNow.ToString("O"));
        }
        catch(Exception e){File.WriteAllText(Folder+"/result.txt","FAIL "+e);Debug.LogException(e);}
    }
    [MenuItem("Tools/Localization/Audit Game UI")]
    public static void Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Exit Play mode before inspecting assets.");
        Directory.CreateDirectory(Folder);
        var rows=new List<Row>();
        var paths=AssetDatabase.GetAllAssetPaths().Where(p=>(p.EndsWith(".prefab")||p.EndsWith(".unity")) &&
            new[]{"Assets/Resources/","Assets/ResourcesBundle/","Assets/Scenes/","Assets/MainGame/","Assets/Prefabs/","Assets/BundleResource/","Assets/VideoCharacterAnimation/","Assets/_Temp/"}.Any(prefix=>p.StartsWith(prefix))).OrderBy(p=>p).ToArray();
        foreach(var path in paths)
        {
            if(path.EndsWith(".prefab"))
            {
                var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(root!=null)Visit(root,path,rows);
            }
            else
            {
                var scene=EditorSceneManager.OpenPreviewScene(path);
                try{foreach(var root in scene.GetRootGameObjects())Visit(root,path,rows);}
                finally{EditorSceneManager.ClosePreviewScene(scene);}
            }
        }
        File.WriteAllText(Folder+"/audit.json",JsonUtility.ToJson(new Report{assets=paths.Length,rows=rows.ToArray()},true));
        Debug.Log("[Localization] Audited "+paths.Length+" assets and "+rows.Count+" text entries without saving assets.");
    }
    private static void Visit(GameObject root,string asset,List<Row> rows)
    {
        var owned=new HashSet<Component>();
        foreach(var e in root.GetComponentsInChildren<GameUIElement>(true))
        {
            if(e.graphic!=null)owned.Add(e.graphic);
            foreach(var shadow in e.shadows)if(shadow!=null)owned.Add(shadow);
        }
        foreach(var i in root.GetComponentsInChildren<TMP_InputField>(true))if(i.textComponent!=null)owned.Add(i.textComponent);
        foreach(var i in root.GetComponentsInChildren<InputField>(true))if(i.textComponent!=null)owned.Add(i.textComponent);
        foreach(var d in root.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            if(d.captionText!=null)owned.Add(d.captionText);if(d.itemText!=null)owned.Add(d.itemText);
            foreach(var o in d.options)Record(d,o.text);
        }
        foreach(var d in root.GetComponentsInChildren<Dropdown>(true))
        {
            if(d.captionText!=null)owned.Add(d.captionText);if(d.itemText!=null)owned.Add(d.itemText);
            foreach(var o in d.options)Record(d,o.text);
        }
        void Record(Component component,string text)
        {
            if(owned.Contains(component))return;
            if(string.IsNullOrWhiteSpace(text)||!Regex.IsMatch(text,@"[A-Za-z\p{IsHangulSyllables}\p{IsCJKUnifiedIdeographs}]"))return;
            var binder=component.GetComponent<LocalizedText>();var old=component.GetComponent<GameUILocalize>();
            string key=binder!=null?binder.key:old!=null?old.CatalogKey:null;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(component,out string guid,out long id);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(component.gameObject,out string objectGuid,out long objectId);
            rows.Add(new Row{asset=asset,path=Hierarchy(component.transform),type=component.GetType().Name,text=text,key=key,componentId=id,objectId=objectId});
        }
        foreach(var e in root.GetComponentsInChildren<GameUIElement>(true))if(e.kind==GameUIElement.ElementKind.Label)Record(e,e.text);
        foreach(var t in root.GetComponentsInChildren<TMP_Text>(true))Record(t,t.text);
        foreach(var t in root.GetComponentsInChildren<Text>(true))Record(t,t.text);
        foreach(var t in root.GetComponentsInChildren<TextMesh>(true))Record(t,t.text);
        foreach(var t in root.GetComponentsInChildren<tk2dTextMesh>(true))Record(t,t.text);
    }
    private static string Hierarchy(Transform t)=>t.parent==null?t.name+"["+t.GetSiblingIndex()+"]":Hierarchy(t.parent)+"/"+t.name+"["+t.GetSiblingIndex()+"]";
}
#endif
