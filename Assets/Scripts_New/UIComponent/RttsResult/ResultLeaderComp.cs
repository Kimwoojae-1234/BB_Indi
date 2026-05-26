using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultLeaderComp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _Desc;
    [SerializeField] private Image _Logo;

    public void Set(ResultMyLeaders.LeaderType type, int [] value, int league)
    {
        int _team = (value[0] / KOBConstant.PLAYER_RECORD_UNIT);        
        int _player_idx = (value[0] % KOBConstant.PLAYER_RECORD_UNIT);
        if (_team == 0) //내팀
        {
            KOBManager.Resource.LoadMyTeamLogo(_Logo);
        }
        else //타팀
        {
            RttsTeam info = KOBManager.Backend.Chart.RttsTeam.GetRttsTeam(_team);
            KOBManager.Resource.LoadTeamLogo(_Logo, info.Logo);
        }

        CharacterData cardData = KOBManager.Backend.Chart.CharacterData.GetData(_player_idx);
        _Desc.text =  string.Format("{0}\n{1} <size=100><color=#00ff00>{2}</color></size>",
                       cardData.name_id,
                       GetTitleType(type),
                       value[1]);

    }

    private string GetTitleType(ResultMyLeaders.LeaderType type)
    {
        if (type == ResultMyLeaders.LeaderType.Homerun)
        {
            return "HR LEADER";
        }
        else if (type == ResultMyLeaders.LeaderType.Avg)
        {
            return "AVG LEADER";
        }
        else if (type == ResultMyLeaders.LeaderType.Rbi)
        {
            return "RBI LEADER";
        }
        else if (type == ResultMyLeaders.LeaderType.Hit)
        {
            return "HIT LEADER";
        }
        else if (type == ResultMyLeaders.LeaderType.Ops)
        {
            return "OPS LEADER";
        }
        return string.Empty;
    }
}
