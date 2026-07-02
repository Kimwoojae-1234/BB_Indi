using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StandingComponent : MonoBehaviour
{
    [SerializeField] private Image bg;
    [SerializeField] private TextMeshProUGUI RankTxt;
    [SerializeField] private Image logo;
    [SerializeField] private TextMeshProUGUI TeamTxt;
    [SerializeField] private TextMeshProUGUI WinLoseDrawTxt;
    [SerializeField] private TextMeshProUGUI GabTxt;



    private int _idx;

    public void InitComp(int idx)
    {
        RttsTeam teamInfo = KOBManager.Backend.Chart.RttsTeam.GetRttsTeam(idx);
        _idx = idx;
        if ((idx % 1000)== 0) //idx 0 (realIndex도 마찬가지)는 무조건 내팀임
        {
            KOBManager.Resource.LoadMyTeamLogo(logo);
            TeamTxt.text = KOBTextUtil.GetMyTeamName();// teamInfo.Name;
        }
        else
        {
            //idx는 0~9까지의 순차적인 인덱스
            KOBManager.Resource.LoadTeamLogo(logo, teamInfo.Logo);
            TeamTxt.text = teamInfo.Name;
        }
        logo.transform.localScale = new Vector2(0.4f, 0.4f);
    }


    public int SetRank(int rank, int wld, int _firstwld, int lastRank, bool bSameGab)
    {
        bool isMyTeam = ((_idx % 1000) == 0) ? true : false;
        bg.sprite = KOBManager.Atlas.GetStandingRankBgSprite(rank, isMyTeam);

        RankTxt.text = rank.ToString();

        int win = wld / 1000000;
        int lose = (wld / 1000) % 1000;
        int draw = wld % 1000;

        int first_win = _firstwld / 1000000;
        int first_lose = (_firstwld / 1000) % 1000;
        int first_draw = _firstwld % 1000;

        WinLoseDrawTxt.text = KOBTextUtil.SetWinPer(win, lose, draw, true);// string.Format("<color=green>W {0}</color>  <color=#FF1000>L {1}</color>  <color=#81B6FF>D {2}</color>", win, lose, draw);

        

        if (_firstwld == 0)
        {
            GabTxt.text = "0.0";
        }
        else
        {
            float gab = (float)((first_win - win) - (first_lose - lose)) / 2.0f;
            GabTxt.text = string.Format("{0:0.0} ", gab);            
        }

        if (bSameGab == true)
        {
            RankTxt.text = lastRank.ToString();
            return lastRank;
        }
        else
        {
            return rank;
        }
    }

    public void SetMyRankUpDown(bool isUp)
    {

    }
}
