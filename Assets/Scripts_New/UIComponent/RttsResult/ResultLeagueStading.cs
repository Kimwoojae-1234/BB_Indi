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
            _Desc.text = L10n.F("UI.Format.YourTeamsFinalStandingLeagueChampionValueWinsValueDrawsValueLoses",
                FinalStanding[1], FinalStanding[2], FinalStanding[3]);
        }
        else
        {
            _WinnerEffect.gameObject.SetActive(false);
            _Desc.text = L10n.F("UI.Format.YourTeamsFinalStandingRankValueThValueWinsValueDrawsValue",
                FinalStanding[0], FinalStanding[1], FinalStanding[2], FinalStanding[3]);
        }

        KOBManager.Resource.LoadLeagueLogo(_Logo, CurLeague);
    }
}
