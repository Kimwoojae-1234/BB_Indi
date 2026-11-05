using System;
using UnityEngine;

// Local presentation-build reward settings. Max values are exclusive.
public static class BeachLeagueRewards
{
    [Serializable] public class Settings
    {
        public int CardAmount = 1, GoldMin = 150, GoldMaxExclusive = 251;
        public int FreeGemMin = 5, FreeGemMaxExclusive = 9;
        public int TrophyRoadPoint = 10, BallerReputationPoint = 10, RttsRewardPoint = 1;
    }
    private static Settings config;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetConfig() { config = null; }
    public static Settings Config
    {
        get
        {
            if (config != null) return config;
            var asset = Resources.Load<TextAsset>("Prototype/BeachLeagueRewards");
            try { config = asset == null ? new Settings() : JsonUtility.FromJson<Settings>(asset.text); }
            catch (Exception e) { Debug.LogError("Invalid BeachLeagueRewards: " + e.Message); }
            config ??= new Settings();
            config.CardAmount = Mathf.Max(0, config.CardAmount);
            config.GoldMin = Mathf.Clamp(config.GoldMin, 0, 1000000);
            config.GoldMaxExclusive = Mathf.Clamp(config.GoldMaxExclusive, config.GoldMin + 1, 1000001);
            config.FreeGemMin = Mathf.Clamp(config.FreeGemMin, 0, 1000000);
            config.FreeGemMaxExclusive = Mathf.Clamp(config.FreeGemMaxExclusive, config.FreeGemMin + 1, 1000001);
            config.TrophyRoadPoint = Mathf.Max(0, config.TrophyRoadPoint);
            config.BallerReputationPoint = Mathf.Max(0, config.BallerReputationPoint);
            config.RttsRewardPoint = Mathf.Max(0, config.RttsRewardPoint);
            return config;
        }
    }
}
