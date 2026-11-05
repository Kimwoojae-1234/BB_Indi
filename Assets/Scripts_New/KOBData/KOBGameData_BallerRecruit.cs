using System;

public partial class KOBGameData
{
    public int RecruitBaller(int idx)
    {
        var character = KOBManager.Backend.Chart.CharacterData.GetData(idx);
        if (character == null || character.char_type != CharacterType.Ballers)
            return (int)KOBErrorCode.Not_Have_Baller;
        if (PlayerInfo.BallerList.ContainsKey(idx)) return (int)KOBErrorCode.Baller_Already_Owned;

        int cost = PrototypeBallers.UnlockCost(character, GrowthInfo.League);
        if (cost <= 0 || CurrencyInfo.TotalGem < cost) return (int)KOBErrorCode.Not_Enough_Gem;

        // Persist currency and ownership together through the existing game-data save/rollback.
        long freeGems = Math.Min(CurrencyInfo.Gem_Free, cost);
        var baller = new KOBBaller();
        baller.InitBaller(idx, 1);
        PlayerInfo.BallerList.Add(idx, baller);
        GrowthInfo.SetBallerAdditionalInfo(idx);
        CurrencyInfo.UpdateCurrency(0, -(cost - freeGems), -freeGems, 0);
        isPlayerChange = isGrowthChange = isCurrencyChange = IsChangedData = true;
        return (int)KOBErrorCode.None;
    }
}
