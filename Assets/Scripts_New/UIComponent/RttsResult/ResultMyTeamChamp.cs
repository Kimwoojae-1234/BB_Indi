using UnityEngine;

public class ResultMyTeamChamp : ResultLeagueStading
{
    [SerializeField] private Reward_Item_small _rewardClone;

    public void SetChamp(TResultRttsLeagueUpgrade res)
    {
        int[] FinalStanding = res.FinalStanding;
        
        _WinnerEffect.gameObject.SetActive(true);
        _Desc.text = UILocalization.Format("ui.81befb160d3e",
                FinalStanding[1], FinalStanding[2], FinalStanding[3]);

        KOBManager.Resource.LoadMyTeamLogo(_Logo);
    }
}
