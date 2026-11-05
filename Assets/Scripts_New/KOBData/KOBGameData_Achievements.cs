using System;

// Stable persisted IDs, independent of career-stat array positions.
public enum BallerAchievementMetric
{
    Hits = 1,
    HomeRuns = 2,
    RunsBattedIn = 3,
    StolenBases = 4,
    Walks = 5,
    RunsScored = 6
}

public partial class KOBGameData
{
    /// <summary>
    /// Future match-result hook: call once per finalized result, for the controlled baller only.
    /// Adds to this baller's totals and marks GrowthInfo for the normal save/rollback flow.
    /// No match hooks or reward granting are enabled yet.
    /// </summary>
    public bool RecordBallerAchievement(int ballerIdx, BallerAchievementMetric metric, int amount)
    {
        if (amount <= 0 || !Enum.IsDefined(typeof(BallerAchievementMetric), metric) ||
            !PlayerInfo.BallerList.ContainsKey(ballerIdx)) return false;
        var character = KOBManager.Backend.Chart.CharacterData.GetData(ballerIdx);
        if (character == null || character.char_type != CharacterType.Ballers) return false;

        if (!GrowthInfo.BallerAchievementProgress.TryGetValue(ballerIdx, out var totals))
        {
            totals = new System.Collections.Generic.Dictionary<int, int>();
            GrowthInfo.BallerAchievementProgress.Add(ballerIdx, totals);
        }
        totals.TryGetValue((int)metric, out int previous);
        totals[(int)metric] = (int)Math.Min(int.MaxValue, (long)previous + amount);
        isGrowthChange = IsChangedData = true;
        return true;
    }
}
