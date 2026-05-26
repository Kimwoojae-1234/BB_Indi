using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KOBTextUtil
{

    public static string[] _RecordTypeName = new string[] { "Home Run", "Batting\nAverage", "Run Batted In", "Number of\nHits", "On-base\nPlus Slugging" };

    public static string GetMyTeamName()
    {
        return "<color=#ffff00>MY TEAM</color>";
    }

    public static string GetMyPlayerName(int idx)
    {
        return "<color=#ffff00>MY PLAYER</color>";
    }


    public static string SetRankText(int rank)
    {
        if (rank <= 0)
        {
            return "<color=#ffff00>RANK</color> - ";
        }
        else
        {
            return string.Format("<color=#ffff00>RANK</color> {0} ", ToOrdinal(rank));
        }
    }


    public static string SetWinPer(int w, int l, int d, bool isDetailShow = false)
    {
        string per = string.Empty;
        if (w == 0) per = ".000";
        else if (l == 0) per = "1.000";
        else
        {
            int value = (w * 1000) / (w + l);
            per = string.Format(".{0:D3}", value);
        }

        if (isDetailShow)
        {
            return string.Format("{0} (W{1} D{2} L{3})", per, w, d, l);
        }
        else
        {
            return per;
        }
    }


    public static string ToOrdinal(int number)
    {
        if (number <= 0) return number.ToString();

        int lastTwo = number % 100;

        if (lastTwo >= 11 && lastTwo <= 13)
        {
            return number + "th";
        }

        switch (number % 10)
        {
            case 1: return number + "st";
            case 2: return number + "nd";
            case 3: return number + "rd";
            default: return number + "th";
        }
    }


    public static string SetResultState(int score1, int score2)
    {
        if (score1 == score2)
        {
            return "TIE";
        }
        else if (score1 > score2)
        {
            return "WIN";
        }
        else
        {
            return "LOSS";
        }
    }

    public static Color32 SetResultColor(int score1, int score2)
    {
        if (score1 == score2)
        {
            return new Color32(149, 165, 166, 255); // #95A5A6 회색
        }
        else if (score1 > score2)
        {
            return new Color32(76, 175, 80, 255);   // #4CAF50 초록
        }
        else
        {
            return new Color32(231, 76, 60, 255);   // #E74C3C 빨강
        }
    }


    public static string SetAvgText(int value)
    {
        if (value >= 1000)
            return (value / 1000) + "." + (value % 1000).ToString("000");
        else
            return "." + (value % 1000).ToString("000");
    }


    public static string GetCardType(KOBReward reward)
    {
        switch (reward)
        {
            case KOBReward.Card_Black:
                return "Black Card";
            case KOBReward.Card_Legend:
                return "Legend Card";
            case KOBReward.Card_Epic:
                return "Epic Card";
            case KOBReward.Card_Rare:
                return "Rare Card";
            default:
                return "Common Card";
        }
    }
}
