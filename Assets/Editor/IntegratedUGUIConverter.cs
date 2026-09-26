#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
// Native migration records and material lookup used by regression checks.
// Completed conversion implementation is preserved in Docs/UIAudit/LegacyConverterSources.
public static class IntegratedUGUIConverter
{
    public const string Folder = "Assets/MainGame/UI/IntegratedUGUI";
    public const string Output = "Docs/UIAudit/Integrated";
    public static bool CompleteMigration;
    private static string ResourceFolder => CompleteMigration ? "Assets/MainGame/UI/CompleteUGUI" : Folder;
    [Serializable] public sealed class Replacement { public long original, replacement; public string originalType, replacementType; }
    [Serializable] public sealed class AssetPlan { public string source, candidate; public Replacement[] components; public string[] controllers; public Replacement[] identities; public long[] synthetic; }
    [Serializable] public sealed class Plan { public AssetPlan[] assets; }
    public static Material NativeMaterial(Material source)
    {
        if (source == null) return null;
        bool particle = source.shader.name.EndsWith("Particles/Additive", StringComparison.Ordinal);
        bool atlas = source.shader.name == "Unlit/Transparent Colored Additive 1";
        if (!particle && !atlas) return source;
        string path = ResourceFolder + "/Material-" + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)) + ".mat";
        var target = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (target == null)
        {
            target = new Material(Shader.Find("Game/UI/Additive")) { name = source.name + " UGUI" };
            target.SetColor("_TintColor", particle ? source.GetColor("_TintColor") : new Color(.5f, .5f, .5f, .5f));
            if (atlas) target.SetFloat("_ColorMask", 7);
            target.mainTexture = source.mainTexture;
            AssetDatabase.CreateAsset(target, path);
        }
        return target;
    }
}
#endif
