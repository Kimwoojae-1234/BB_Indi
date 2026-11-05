using System;
using System.Collections.Generic;
using System.Linq;

public partial class KOBGameData
{
    public int BuyShopProduct(string id, out List<KOBRewardInfo> rewards)
    {
        rewards = new List<KOBRewardInfo>();
        var product = ShopCatalog.Current.Find(id);
        if (product == null || product.currency != "gem" || product.cost <= 0 || product.gold < 0) return 109;
        if (CurrencyInfo.TotalGem < product.cost) return (int)KOBErrorCode.Not_Enough_Gem;
        if (!string.IsNullOrEmpty(product.chest))
        {
            var chest = ShopCatalog.Current.Chest(product.chest);
            if (chest == null || chest.cards <= 0 || chest.gold < 0 || product.chestCount < 1 || product.chestCount > 100) return 109;
            var owned = PlayerInfo.BallerList.Keys.Where(idx => KOBManager.Backend.Chart.CharacterData.GetData(idx)?.char_type == CharacterType.Ballers).ToArray();
            if (owned.Length == 0) return (int)KOBErrorCode.Not_Have_Baller;
            for (int i = 0; i < product.chestCount; i++)
            {
                int idx = owned[UnityEngine.Random.Range(0, owned.Length)];
                var character = KOBManager.Backend.Chart.CharacterData.GetData(idx);
                rewards.Add(new KOBRewardInfo((KOBReward)((int)KOBReward.Card_Common + (int)character.rarity - 1), idx, chest.cards));
                if (chest.gold > 0) rewards.Add(new KOBRewardInfo(KOBReward.Gold, 0, chest.gold));
            }
        }
        if (product.gold > 0) rewards.Add(new KOBRewardInfo(KOBReward.Gold, 0, product.gold));
        if (rewards.Count == 0) return 109;
        long free = Math.Min(CurrencyInfo.Gem_Free, product.cost);
        CurrencyInfo.UpdateCurrency(0, -(product.cost - free), -free, 0);
        foreach (var reward in rewards)
        {
            if (reward.GetRewardType() == KOBRewardType.Card) AddBaller(reward);
            else UpdateReward(reward);
        }
        isCurrencyChange = IsChangedData = true;
        return 0;
    }
}

public class TRequestShopPurchase : TRequestBase
{
    public string ProductId;
    public override DummyURL getURL() => DummyURL.ShopPurchase;
}
public class TResultShopPurchase : TResponseBase
{
    public List<KOBRewardInfo> Rewards;
}
