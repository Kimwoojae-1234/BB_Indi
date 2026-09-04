using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultTeam : MonoBehaviour
{
    [SerializeField] private UIScoreBoard board;
    [SerializeField] private TextMeshProUGUI MyScore;
    [SerializeField] private TextMeshProUGUI OppScore;

    [SerializeField] private GameObject VictoryObj;
    [SerializeField] private GameObject DefeatedObj;
    [SerializeField] private GameObject DrawObj;

    [SerializeField] private ResultItem[] StatItem;

    [SerializeField] private Image MyLogo;
    [SerializeField] private Image OppLogo;
    [SerializeField] private TextMeshProUGUI MyTeamName;
    [SerializeField] private TextMeshProUGUI OppTeamName;

    public void SetResultTeam(RttsGameResult resultInfo)
    {
        if (resultInfo == null) return;

        board?.BoardUpdate(resultInfo);

        int myScore = resultInfo.Stat.Score[0];
        int oppScore = resultInfo.Stat.Score[1];
        if (MyScore != null) MyScore.text = myScore.ToString();
        if (OppScore != null) OppScore.text = oppScore.ToString();

        if (VictoryObj != null) VictoryObj.SetActive(myScore > oppScore);
        if (DefeatedObj != null) DefeatedObj.SetActive(myScore < oppScore);
        if (DrawObj != null) DrawObj.SetActive(myScore == oppScore);

        if (MyTeamName != null) MyTeamName.text = resultInfo.FirstTeamName;
        if (OppTeamName != null) OppTeamName.text = resultInfo.SecondTeamName;

        SetStatItem(0, resultInfo.Stat.Hit, "HIT", 20);
        SetStatItem(1, resultInfo.Stat.HomeRun, "HR", 7);
        SetStatItem(2, resultInfo.Stat.Steal, "STEAL", 5);
        SetStatItem(3, resultInfo.Stat.StrikeOut, "SO", 20);
        SetStatItem(4, resultInfo.Stat.Error, "ERROR", 5);
    }

    private void SetStatItem(int index, int[] values, string title, int max)
    {
        if (StatItem != null && index >= 0 && index < StatItem.Length && StatItem[index] != null)
        {
            StatItem[index].InitItem(values, title, max);
        }
    }
}
