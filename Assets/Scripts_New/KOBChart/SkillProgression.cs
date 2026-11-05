using System;
using UnityEngine;

public static class SkillProgression
{
    [Serializable] private class Rules { public int[] upgradeGoldByTargetLevel; }
    private static Rules rules;
    public static int MaxLevel(SkillData skill)
    {
        if (skill == null || skill.level_value.Count == 0) return 0;
        int max = int.MaxValue;
        foreach (var effect in skill.level_value)
        {
            if (effect.Val == null || effect.Per == null) return 0;
            max = Math.Min(max, Math.Min(effect.Val.Length, effect.Per.Length));
        }
        return max;
    }
    public static int UpgradeGold(int targetLevel)
    {
        if (rules == null)
        {
            var asset = Resources.Load<TextAsset>("Prototype/Charts/SkillProgression");
            if (asset != null) rules = JsonUtility.FromJson<Rules>(asset.text);
        }
        var costs = rules?.upgradeGoldByTargetLevel;
        return costs != null && targetLevel >= 2 && targetLevel < costs.Length ? costs[targetLevel] : 0;
    }
}
