using System;
using UnityEngine;

[Serializable] public class PassRewardDefinition { public string kind; public int amount; }
[Serializable] public class PassCatalog
{
    public int tiers, xpPerGame, xpPerTier;
    public PassRewardDefinition[] golden, free;
    private static PassCatalog current;
    public static PassCatalog Current => current ?? (current = JsonUtility.FromJson<PassCatalog>(Resources.Load<TextAsset>("Prototype/SeasonPass").text));
    public int Completed(int xp) => Mathf.Clamp(xp / Math.Max(1, xpPerTier), 0, tiers);
    public PassRewardDefinition Reward(int tier, bool premium) { var entries = premium ? golden : free; return entries[(tier - 1) % entries.Length]; }
}
