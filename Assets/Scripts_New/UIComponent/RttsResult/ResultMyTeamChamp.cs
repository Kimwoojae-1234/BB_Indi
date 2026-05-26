using UnityEngine;

public class ResultMyTeamChamp : ResultLeagueStading
{
    [SerializeField] private Reward_Item_small _rewardClone;

    public void SetChamp(TResultRttsLeagueUpgrade res)
    {
        int[] FinalStanding = res.FinalStanding;
        
        _WinnerEffect.gameObject.SetActive(true);
        _Desc.text = string.Format("Your team's final standing : <size=100>League Champion!</size>\n<color=#00ff00>{0} wins</color> <color=#AAAAAA>{1} draws</color> <color=#ff0000>{2} loses</color>",
                FinalStanding[1], FinalStanding[2], FinalStanding[3]);

        KOBManager.Resource.LoadMyTeamLogo(_Logo);
    }
}
