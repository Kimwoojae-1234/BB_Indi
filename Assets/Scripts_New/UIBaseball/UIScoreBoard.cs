using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScoreBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] awayScore;
    [SerializeField] private TextMeshProUGUI[] homeScore;
    [SerializeField] private TextMeshProUGUI[] awayRBH;
    [SerializeField] private TextMeshProUGUI[] homeRBH;

    [SerializeField] private TextMeshProUGUI awayTeam;
    [SerializeField] private TextMeshProUGUI homeTeam;


    public void InitScoreBoard(string _awayTeam, string _homeTeam)
    {
        for(int i = 0; i < awayScore.Length; i++)
        {
            awayScore[i].text = "0";
            awayScore[i].gameObject.SetActive(i == 0);
        }

        for (int i = 0; i < homeScore.Length; i++)
        {
            homeScore[i].text = "0";
            homeScore[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < awayRBH.Length; i++)
        {
            awayRBH[i].text = "0";
            awayRBH[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < homeRBH.Length; i++)
        {
            homeRBH[i].text = "0";
            homeRBH[i].gameObject.SetActive(true);
        }

        awayTeam.text = _awayTeam;
        homeTeam.text = _homeTeam;
    }


    public void BoardUpdate(int[] away, int[] home, int inning, bool bTopInning, bool bEnd)
    {

    }

    public void BoardUpdate(int away, int home)
    {
        
    }

    public void BoardUpdate(RttsGameResult resultInfo)
    {
        if (resultInfo == null) return;

        int homeSlot = resultInfo.FirstTeamIsHome ? 0 : 1;
        int awaySlot = 1 - homeSlot;
        SetInningScores(awayScore, resultInfo.Stat.InningScore[awaySlot]);
        SetInningScores(homeScore, resultInfo.Stat.InningScore[homeSlot]);

        SetSummary(awayRBH, resultInfo, awaySlot);
        SetSummary(homeRBH, resultInfo, homeSlot);

        if (awayTeam != null)
            awayTeam.text = awaySlot == 0 ? resultInfo.FirstTeamName : resultInfo.SecondTeamName;
        if (homeTeam != null)
            homeTeam.text = homeSlot == 0 ? resultInfo.FirstTeamName : resultInfo.SecondTeamName;
    }

    private static void SetInningScores(TextMeshProUGUI[] labels, int[] scores)
    {
        if (labels == null || scores == null) return;
        int count = Mathf.Min(labels.Length, scores.Length);
        for (int i = 0; i < count; i++)
        {
            if (labels[i] == null) continue;
            int value = scores[i];
            labels[i].gameObject.SetActive(value != RttsGameResult.NoPlayInning);
            labels[i].text = value == RttsGameResult.GameEndInning ? "X" : Mathf.Max(0, value).ToString();
        }
    }

    private static void SetSummary(TextMeshProUGUI[] labels, RttsGameResult resultInfo, int teamSlot)
    {
        if (labels == null) return;
        if (labels.Length > 0 && labels[0] != null) labels[0].text = resultInfo.Stat.Score[teamSlot].ToString();
        if (labels.Length > 1 && labels[1] != null) labels[1].text = resultInfo.Stat.Hit[teamSlot].ToString();
        if (labels.Length > 2 && labels[2] != null) labels[2].text = resultInfo.Stat.Walk[teamSlot].ToString();
    }


    string GetBoardValue(GameObject obj, int value)
    {
        
        return value.ToString();
        
    }

}
