using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScheduleTeam : MonoBehaviour
{
    [SerializeField] public RectTransform rectTrans;
    [SerializeField] Image[] logo;
    [SerializeField] TextMeshProUGUI rankTxt;
    [SerializeField] TextMeshProUGUI teamTxt;
    [SerializeField] TextMeshProUGUI recordTxt;

    private bool isMyTeam = false;

    public void Init(int idx, int gab)
    {
        RttsTeam teamInfo = KOBManager.Rtts.GetTeam(idx); //이걸로 리그 정보와 함께 실제 팀인덱스로(10보다 클수 있음) 팀정보 얻어오나,
        Dictionary<int, TeamRecord> LeagueTeamRecord = KOBManager.MyInfo.GameData.RttsInfo.LeagueTeamRecord;
        Sprite spr = null;

        if (idx == 0) //내팀 정보
        {
            //내팀인 경우
            isMyTeam = true;
            spr = KOBManager.Resource.LoadMyTeamLogoSprite();
            teamTxt.text = KOBTextUtil.GetMyTeamName();
        }
        else
        {
            //상대팀인 경우
            isMyTeam = false;
            spr = KOBManager.Resource.LoadTeamLogo(teamInfo.Logo);            
            teamTxt.text = teamInfo.Name;
        }

        //로고 세팅
        if (spr != null)
        {
            for (int i = 0; i < logo.Length; i++)
            {
                logo[i].sprite = spr;
                logo[i].SetNativeSize();
            }
        }

        //팀기록 관련 세팅
        if (LeagueTeamRecord.ContainsKey(idx))
        {
            recordTxt.text = string.Format("W{0} D{1} L{2}", LeagueTeamRecord[idx].Win, LeagueTeamRecord[idx].Draw, LeagueTeamRecord[idx].Lose);
            int rank = KOBManager.Rtts.CurrentRank[idx];
            rankTxt.text = KOBTextUtil.SetRankText(rank);
        }
        else
        {
            //기록이 없는 경우
            recordTxt.text = "W0 D0 L0";
            rankTxt.text = KOBTextUtil.SetRankText(0);
        }
    }
}
