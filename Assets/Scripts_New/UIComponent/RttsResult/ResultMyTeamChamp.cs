using UnityEngine;

public class ResultMyTeamChamp : ResultLeagueStading
{
    [SerializeField] private Reward_Item_small _rewardClone;

    public void SetChamp(TResultRttsLeagueUpgrade res)
    {
        int[] FinalStanding = res.FinalStanding;
        
        _WinnerEffect.gameObject.SetActive(true);
        _Desc.text = L10n.F("UI.Format.YourTeamsFinalStandingLeagueChampionValueWinsValueDrawsValueLoses",
                FinalStanding[1], FinalStanding[2], FinalStanding[3]);

        KOBManager.Resource.LoadMyTeamLogo(_Logo);
    }
}
