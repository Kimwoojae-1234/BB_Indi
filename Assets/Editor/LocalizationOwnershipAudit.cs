#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BaseBall.BallPlay.UGUI;

// Read-only links from game controllers to authored objects. No assets are saved.
public static class LocalizationOwnershipAudit
{
    [Serializable] public class Reference
    {
        public string asset,owner,controller,field,target,targetType,targetGuid;
        public long targetId;
    }
    [Serializable] public class Binding
    {
        public string asset,path,sourceAsset,key,kind;
        public long sourceId;
    }
    [Serializable] public class Report { public Reference[] references; public Binding[] bindings; }
    public static void Run()
    {
        var output=new List<Reference>();
        var bindings=new List<Binding>();
        var paths=AssetDatabase.GetAllAssetPaths().Where(p=>(p.EndsWith(".prefab")||p.EndsWith(".unity")) &&
            new[]{"Assets/Resources/","Assets/ResourcesBundle/","Assets/Scenes/","Assets/MainGame/","Assets/Prefabs/","Assets/BundleResource/","Assets/VideoCharacterAnimation/","Assets/_Temp/"}.Any(p.StartsWith)).ToArray();
        void Inspect(GameObject[] roots,string asset)
        {
            var labels=roots.SelectMany(r=>r.GetComponentsInChildren<LocalizedText>(true)).Cast<Component>()
                .Concat(roots.SelectMany(r=>r.GetComponentsInChildren<GameUILocalize>(true)).Where(b=>b.element!=null && b.element.kind==GameUIElement.ElementKind.Label));
            foreach(var binding in labels)
            {
                Component source=binding;
                for(int i=0;i<16;i++)
                {
                    var next=PrefabUtility.GetCorrespondingObjectFromSource(source);
                    if(next==null || next==source)break;
                    source=next;
                }
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source,out string guid,out long id);
                string sourceAsset=AssetDatabase.GUIDToAssetPath(guid);
                string key=binding is LocalizedText label?label.key:((GameUILocalize)binding).CatalogKey;
                bindings.Add(new Binding{asset=asset,path=LocalizationBindingPlan.Hierarchy(binding.transform),sourceAsset=string.IsNullOrEmpty(sourceAsset)?asset:sourceAsset,sourceId=id,key=key,kind=binding.GetType().Name});
            }
            foreach(var controller in roots.SelectMany(r=>r.GetComponentsInChildren<MonoBehaviour>(true)).Where(c=>c!=null))
            {
                var script=MonoScript.FromMonoBehaviour(controller);
                string source=AssetDatabase.GetAssetPath(script);
                if(!new[]{"Assets/Scripts/","Assets/Scripts_New/","Assets/Scripts_Old/","Assets/MainGame/Script/","Assets/MainGame/PVP_Tester/"}.Any(source.StartsWith))continue;
                if(controller is LocalizedText || controller is GameUILocalize)continue;
                var serialized=new SerializedObject(controller);
                var iterator=serialized.GetIterator();
                while(iterator.Next(true))
                {
                    if(iterator.propertyType!=SerializedPropertyType.ObjectReference)continue;
                    var value=iterator.objectReferenceValue;
                    var target=value as GameObject;
                    var component=value as Component;
                    if(component!=null)target=component.gameObject;
                    if(target==null)continue;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value,out string guid,out long id);
                    output.Add(new Reference{asset=asset,owner=LocalizationBindingPlan.Hierarchy(controller.transform),controller=controller.GetType().FullName,
                        field=iterator.propertyPath,target=LocalizationBindingPlan.Hierarchy(target.transform),targetType=value.GetType().Name,targetGuid=guid,targetId=id});
                }
            }
        }
        foreach(string path in paths)
        {
            if(path.Contains("/AssetStore/"))continue;
            if(path.EndsWith(".prefab")){var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(root!=null)Inspect(new[]{root},path);}
            else{var scene=EditorSceneManager.OpenPreviewScene(path);try{Inspect(scene.GetRootGameObjects(),path);}finally{EditorSceneManager.ClosePreviewScene(scene);}}
        }
        File.WriteAllText("Library/Localization/controller-references.json",JsonUtility.ToJson(new Report{references=output.ToArray(),bindings=bindings.ToArray()},true));
    }
}
#endif
