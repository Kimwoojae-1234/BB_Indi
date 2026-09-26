using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultMyLeaders : MonoBehaviour
{
    public enum LeaderType
    { 
        Homerun = 0,
        Avg,
        Rbi,
        Hit,
        Ops
    }


    [SerializeField] private TextMeshProUGUI _Title;
    [SerializeField] private ResultLeaderComp _leaderComp;
    [SerializeField] private RectTransform _ballerPos;
    [SerializeField] private Reward_Item_small _rewardClone;

    public void Set(TResultRttsLeagueUpgrade res, LeaderType type)
    {
        int league = res.CurrentLeague;
        int plyer_idx = 0;
        if (type == LeaderType.Homerun)
        {
            plyer_idx = res.HRLeader[0];
            L10n.SetText(_Title, "UI.Label.HomeRunWinner");
            _leaderComp.Set(type, res.HRLeader, league);
        }
        else if (type == LeaderType.Avg)
        {
            plyer_idx = res.AvgLeader[0];
            L10n.SetText(_Title, "UI.Label.BattingAverageWinner");
            _leaderComp.Set(type, res.AvgLeader, league);
        }
        else if (type == LeaderType.Rbi)
        {
            plyer_idx = res.RbiLeader[0];
            L10n.SetText(_Title, "UI.Label.RbiWinner");
            _leaderComp.Set(type, res.RbiLeader, league);
        }
        else if (type == LeaderType.Hit)
        {
            plyer_idx = res.HitLeader[0];
            L10n.SetText(_Title, "UI.Label.HitWinner");
            _leaderComp.Set(type, res.HitLeader, league);
        }
        else if (type == LeaderType.Ops)
        {
            plyer_idx = res.OpsLeader[0];
            L10n.SetText(_Title, "UI.Label.OpsWinner");
            _leaderComp.Set(type, res.OpsLeader, league);
        }

        KOBManager.Resource.LoadGameObject("Ballers", "baller" + plyer_idx, _ballerPos.transform);

    }

}
