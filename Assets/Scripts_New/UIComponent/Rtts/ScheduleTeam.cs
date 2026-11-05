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
        RttsTeam teamInfo = KOBManager.Rtts.GetTeam(idx); //�̰ɷ� ���� ������ �Բ� ���� ���ε�����(10���� Ŭ�� ����) ������ ������,
        Dictionary<int, TeamRecord> LeagueTeamRecord = KOBManager.MyInfo.GameData.RttsInfo.LeagueTeamRecord;
        Sprite spr = null;

        if (idx == 0) //���� ����
        {
            //������ ���
            isMyTeam = true;
            spr = KOBManager.Resource.LoadMyTeamLogoSprite();
            teamTxt.text = KOBTextUtil.GetMyTeamName();
        }
        else
        {
            //������� ���
            isMyTeam = false;
            spr = KOBManager.Resource.LoadTeamLogo(teamInfo.Logo);            
            teamTxt.text = teamInfo.Name;
        }

        //�ΰ� ����
        if (spr != null)
        {
            for (int i = 0; i < logo.Length; i++)
            {
                logo[i].sprite = spr;
                logo[i].SetNativeSize();
            }
        }

        //����� ���� ����
        if (LeagueTeamRecord.ContainsKey(idx))
        {
            UILocalization.Set(recordTxt, "ui.d39bd5be8014", LeagueTeamRecord[idx].Win, LeagueTeamRecord[idx].Draw, LeagueTeamRecord[idx].Lose);
            int rank = KOBManager.Rtts.CurrentRank[idx];
            rankTxt.text = KOBTextUtil.SetRankText(rank);
        }
        else
        {
            //����� ���� ���
            UILocalization.Set(recordTxt, "ui.c865b555105a");
            rankTxt.text = KOBTextUtil.SetRankText(0);
        }
    }
}
