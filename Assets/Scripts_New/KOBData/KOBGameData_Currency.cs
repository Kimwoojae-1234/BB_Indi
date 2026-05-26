using System;

/// <summary>
/// 커런시 세팅
/// </summary>

public partial class KOBGameData : BackendData.Base.GameData
{
    public void UpdateReward(KOBRewardInfo RewardInfo)
    {
        IsChangedData = CurrencyInfo.UpdateCurrency(RewardInfo);        
        if (IsChangedData == true)
        {
            isCurrencyChange = true;
        }
    }

    public void UpdateCurrency(long gold, long gem, long gem_free, int energy)
    {
        IsChangedData = CurrencyInfo.UpdateCurrency(gold, gem, gem_free, energy);
        if (IsChangedData == true)
        {
            isCurrencyChange = true;
        }
    }

}
