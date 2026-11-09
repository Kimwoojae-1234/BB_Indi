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

        // Resolve both names and logos using the same league chart as StandingComponent.
        BindTeam(resultInfo.FirstTeamIndex, MyLogo, MyTeamName, out var firstName);
        BindTeam(resultInfo.SecondTeamIndex, OppLogo, OppTeamName, out var secondName);
        resultInfo.FirstTeamName = firstName;
        resultInfo.SecondTeamName = secondName;
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

        SetStatItem(0, resultInfo.Stat.Hit, UILocalization.Get("ui.395ed46c439c"), 20);
        SetStatItem(1, resultInfo.Stat.HomeRun, "HR", 7);
        SetStatItem(2, resultInfo.Stat.Steal, UILocalization.Get("ui.33cdb7043f59"), 5);
        SetStatItem(3, resultInfo.Stat.StrikeOut, "SO", 20);
        SetStatItem(4, resultInfo.Stat.Error, "ERROR", 5);
    }

    private static void BindTeam(int teamIndex, Image logo, TMP_Text label, out string name)
    {
        int team = teamIndex % KOBConstant.TEAM_RECORD_UNIT;
        var chart = KOBManager.Backend.Chart.RttsTeam.GetRttsTeam(
            RttsLiveRoster.League * KOBConstant.TEAM_RECORD_UNIT + team);
        name = team == 0 ? KOBTextUtil.GetMyTeamName() : chart != null ? chart.Name : UILocalization.Get("beach.team." + team);
        if (label != null) label.text = name;
        if (logo == null) return;
        logo.sprite = team == 0 ? TeamLogoCatalog.MyTeam : TeamLogoCatalog.Opponent(chart != null ? chart.Logo : team + 1);
        logo.enabled = logo.sprite != null;
        logo.preserveAspect = true;
    }

    private void SetStatItem(int index, int[] values, string title, int max)
    {
        if (StatItem != null && index >= 0 && index < StatItem.Length && StatItem[index] != null)
        {
            StatItem[index].InitItem(values, title, max);
        }
    }
}
