using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class StadiumSpriteChecks
{
    static StadiumSpriteChecks() { EditorApplication.update += RunPending; }
    private static void RunPending()
    {
        if (!File.Exists("Temp/CheckStadiumSprites.request")) { EditorApplication.update -= RunPending; return; }
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= RunPending;
        File.Delete("Temp/CheckStadiumSprites.request");
        Validate();
    }
    [MenuItem("Tools/Clutch/Validate Stadium Sprites")]
    public static void Validate()
    {
        try
        {
            const string path = "Assets/Resources/MainGame/prefabs/BattingViewPrefab/bg/centerBGPrefab1.prefab";
            foreach (var name in new[] { "batterview2_001", "homebase", "bg1_side_001" })
            {
                string image = "Assets/Resources/Sprite/Stadium/" + name + ".png";
                AssetDatabase.ImportAsset(image, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                if (AssetDatabase.LoadAssetAtPath<Sprite>(image) == null) throw new Exception("Missing imported Sprite: " + image);
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (var name in new[] { "backL", "backR", "home" })
            {
                var obj = prefab.transform.Find("field/" + name);
                var renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer == null || renderer.sprite == null) throw new Exception("Missing SpriteRenderer sprite: " + name);
                if (renderer.sprite.pixelsPerUnit != 1) throw new Exception("Incorrect stadium scale: " + name);
                if (renderer.flipX != (name == "backR")) throw new Exception("Incorrect mirroring: " + name);
                if (obj.GetComponent<tk2dSprite>() != null) throw new Exception("Unexpected tk2d component: " + name);
            }
            foreach (var side in new[] { "left", "right" })
            {
                string sidePath = "Assets/Resources/MainGame/prefabs/BattingViewPrefab/bg_side/" + side + "BGPrefab1.prefab";
                AssetDatabase.ImportAsset(sidePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var sidePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sidePath);
                var obj = sidePrefab.transform.Find("field/spr");
                var renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer == null || renderer.sprite == null || renderer.sprite.name != "bg1_side_001") throw new Exception("Missing side field sprite: " + side);
                if (renderer.sprite.pixelsPerUnit != 0.5f || obj.localScale != new Vector3(0.8f, 1.2f, 1)) throw new Exception("Incorrect side field size: " + side);
                if (obj.GetComponent<tk2dSprite>() != null) throw new Exception("Unexpected side tk2d component");
            }
            File.WriteAllText("Logs/stadium-sprites-check.log", "PASS: left/right field sprites validated; Unity imported both sprites; backL/backR/home have valid Sprite references, 1 PPU, correct mirroring, no tk2d component.");
        }
        catch (Exception e) { File.WriteAllText("Logs/stadium-sprites-check.log", "FAIL: " + e); Debug.LogException(e); }
    }
}
