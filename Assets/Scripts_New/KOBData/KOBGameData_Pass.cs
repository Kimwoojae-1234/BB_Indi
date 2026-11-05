using System.Collections.Generic;
using System.Linq;

public partial class KOBGameData
{
    public int ClaimFreePass(int tier, out List<KOBRewardInfo> rewards)
    {
        rewards = new List<KOBRewardInfo>();
        var config = PassCatalog.Current;
        if (tier < 1 || tier > config.tiers || tier > config.Completed(GrowthInfo.PassXP) || GrowthInfo.PassFreeClaims.Contains(tier)) return 109;
        var entry = config.Reward(tier, false);
        if (entry.amount <= 0) return 109;
        if (entry.kind == "gold") rewards.Add(new KOBRewardInfo(KOBReward.Gold, 0, entry.amount));
        else if (entry.kind == "small" || entry.kind == "large")
        {
            var chest = ShopCatalog.Current.Chest(entry.kind);
            var owned = PlayerInfo.BallerList.Keys.Where(idx => KOBManager.Backend.Chart.CharacterData.GetData(idx)?.char_type == CharacterType.Ballers).ToArray();
            if (chest == null || owned.Length == 0) return 109;
            for (int i = 0; i < entry.amount; i++)
            {
                int idx = owned[UnityEngine.Random.Range(0, owned.Length)];
                var data = KOBManager.Backend.Chart.CharacterData.GetData(idx);
                rewards.Add(new KOBRewardInfo((KOBReward)((int)KOBReward.Card_Common + (int)data.rarity - 1), idx, chest.cards));
                rewards.Add(new KOBRewardInfo(KOBReward.Gold, 0, chest.gold));
            }
        }
        else return 109;
        foreach (var reward in rewards)
            if (reward.GetRewardType() == KOBRewardType.Card) AddBaller(reward); else UpdateReward(reward);
        GrowthInfo.PassFreeClaims.Add(tier);
        isGrowthChange = IsChangedData = true;
        return 0;
    }
}
public class TRequestPassClaim : TRequestBase
{
    public int Tier;
    public override DummyURL getURL() => DummyURL.PassClaim;
}
