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
            int checkedPrefabs = 0;
            for (int n = 1; n <= 4; n++)
            {
                string number = n.ToString("000");
                foreach (var key in new[] { "BattingViewPrefab/bg/centerBGPrefab", "BattingViewPrefab/bg_side/leftBGPrefab", "BattingViewPrefab/bg_side/rightBGPrefab", "FieldViewPrefab/stadium/stadiumPrefab" })
                {
                    string assetPath = "Assets/Resources/MainGame/prefabs/" + key + n + ".prefab";
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (asset == null) throw new Exception("Missing stadium prefab: " + assetPath);
                    string expected = key.Contains("centerBG") ? "batterview2_" + number : key.Contains("bg_side") ? "bg1_side_" + number : "filed" + number + "_1";
                    string expectedBack = n == 1 ? "back" : "filed" + number + "_back";
                    bool found = false, foundBack = key.Contains("stadiumPrefab");
                    foreach (var renderer in asset.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        if (renderer.sprite == null) throw new Exception("Missing Sprite in " + assetPath + "/" + renderer.name);
                        if ((renderer.sprite.name == expected || renderer.sprite.name == expected + "_0")) found = true;
                        if ((renderer.sprite.name == expectedBack || renderer.sprite.name == expectedBack + "_0")) foundBack = true;
                    }
                    if (!found || !foundBack) throw new Exception("Incorrect numbered texture in " + assetPath);
                    checkedPrefabs++;
                }
            }
            File.WriteAllText("Logs/stadium-sprites-check.log", "PASS: " + checkedPrefabs + " prefabs across 4 stadiums; numbered field/front/side/background textures match, all SpriteRenderer references resolve.");
        }
        catch (Exception e) { File.WriteAllText("Logs/stadium-sprites-check.log", "FAIL: " + e); Debug.LogException(e); }
    }
}
