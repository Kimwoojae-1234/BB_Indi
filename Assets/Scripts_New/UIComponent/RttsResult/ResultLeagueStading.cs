using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultLeagueStading : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _Title;
    [SerializeField] protected TextMeshProUGUI _Desc;
    [SerializeField] protected Image _Logo;
    [SerializeField] protected GameObject _WinnerEffect;

    public void Set(TResultRttsLeagueUpgrade res)
    {
        //_Title.text = "CHALLENGER LEAGUE"; //이값은 키값으로 할것

        int[] FinalStanding = res.FinalStanding;
        int CurLeague = res.CurrentLeague;

        if (FinalStanding[0] == 1) //1위
        {
            _WinnerEffect.gameObject.SetActive(true);
            _Desc.text = string.Format("Your team's final standing : <size=100>League Champion!</size>\n<color=#00ff00>{0} wins</color> <color=#AAAAAA>{1} draws</color> <color=#ff0000>{2} loses</color>",
                FinalStanding[1], FinalStanding[2], FinalStanding[3]);
        }
        else
        {
            _WinnerEffect.gameObject.SetActive(false);
            _Desc.text = string.Format("Your team's final standing : <size=100>Rank {0}th</size>\n<color=#00ff00>{1} wins</color> <color=#AAAAAA>{2} draws</color> <color=#ff0000>{3} loses</color>",
                FinalStanding[0], FinalStanding[1], FinalStanding[2], FinalStanding[3]);
        }

        KOBManager.Resource.LoadLeagueLogo(_Logo, CurLeague);
    }
}
