#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseBall.BallPlay.UGUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LocalizationScopeChecks
{
    [Serializable] private class Location { public string asset,path,key,kind; }
    [Serializable] private class Scope { public Location[] fixedLabels,excluded; }
    [Serializable] private class Result { public string checkedAt; public int assets,fixedLabels,excluded; public string[] errors; }
    [MenuItem("Tools/Localization/Check Static Text Scope")]
    public static void Run()
    {
        var scope=JsonUtility.FromJson<Scope>(File.ReadAllText("Docs/Localization/static-text-scope.json"));
        var assets=scope.fixedLabels.Concat(scope.excluded).Select(e=>e.asset).Distinct().ToArray();
        var errors=new List<string>();int fixedCount=0,excludedCount=0;
        foreach(string asset in assets)
        {
            void Inspect(GameObject[] roots)
            {
                var objects=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToDictionary(t=>LocalizationBindingPlan.Hierarchy(t),t=>t.gameObject);
                foreach(var entry in scope.fixedLabels.Where(e=>e.asset==asset))
                {
                    if(!objects.TryGetValue(entry.path,out var obj)){errors.Add(asset+": missing static object "+entry.path);continue;}
                    var binding=obj.GetComponent<LocalizedText>();var legacy=obj.GetComponent<GameUILocalize>();
                    string key=entry.kind=="GameUILocalize"?legacy?.CatalogKey:binding?.key;
                    if(key!=entry.key || !L10n.Contains(key))errors.Add(asset+": invalid static key "+entry.path);
                    if(binding!=null && binding.tmp==null && binding.element==null && binding.legacy==null && binding.mesh==null && binding.spriteText==null)
                        errors.Add(asset+": disconnected target "+entry.path);
                    fixedCount++;
                }
                foreach(var entry in scope.excluded.Where(e=>e.asset==asset))
                {
                    if(!objects.TryGetValue(entry.path,out var obj)){errors.Add(asset+": missing excluded object "+entry.path);continue;}
                    if(obj.GetComponent<LocalizedText>()!=null || obj.GetComponent<GameUILocalize>()!=null)
                        errors.Add(asset+": automatic localization on code/data-owned text "+entry.path);
                    excludedCount++;
                }
            }
            if(asset.EndsWith(".prefab")){var root=AssetDatabase.LoadAssetAtPath<GameObject>(asset);if(root==null){errors.Add("Cannot load "+asset);continue;}Inspect(new[]{root});}
            else{var scene=EditorSceneManager.OpenPreviewScene(asset);try{Inspect(scene.GetRootGameObjects());}finally{EditorSceneManager.ClosePreviewScene(scene);}}
        }
        var result=new Result{checkedAt=DateTime.UtcNow.ToString("O"),assets=assets.Length,fixedLabels=fixedCount,excluded=excludedCount,errors=errors.ToArray()};
        File.WriteAllText("Docs/Localization/binding-scope-check.json",JsonUtility.ToJson(result,true));
        if(errors.Count>0)throw new InvalidOperationException(string.Join("; ",errors.Take(8)));
        LocalizationChecks.Run();
        LocalizationAudit.Run();
        Debug.Log("[Localization] Static scope passed: "+fixedCount+" labels, "+excludedCount+" code/data-owned exclusions.");
    }
}
#endif
