using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KOBTextUtil
{

    public static string[] _RecordTypeNameKeys = new string[] { "Baseball.Result.HomeRun", "UI.Label.BattingAverage", "UI.Label.RunBattedIn", "UI.Label.NumberOfHits", "UI.Label.OnBasePlusSlugging" };
        public static string[] _RecordTypeName => L10n.Texts(_RecordTypeNameKeys);

    public static string GetMyTeamName()
    {
        return L10n.T("UI.Label.MyTeam");
    }

    public static string GetMyPlayerName(int idx)
    {
        return L10n.T("UI.Label.MyPlayer");
    }


    public static string SetRankText(int rank)
    {
        if (rank <= 0)
        {
            return L10n.T("UI.Label.Rank.RichText");
        }
        else
        {
            return L10n.F("UI.Format.RankValue.RichText", ToOrdinal(rank));
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
            return L10n.F("UI.Format.ValueWValueDValueLValue", per, w, d, l);
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
            return L10n.F("UI.OrdinalOther", number);
        }

        switch (number % 10)
        {
            case 1: return L10n.F("UI.OrdinalFirst", number);
            case 2: return L10n.F("UI.OrdinalSecond", number);
            case 3: return L10n.F("UI.OrdinalThird", number);
            default: return L10n.F("UI.OrdinalOther", number);
        }
    }


    public static string SetResultState(int score1, int score2)
    {
        if (score1 == score2)
        {
            return L10n.T("UI.Label.Tie");
        }
        else if (score1 > score2)
        {
            return L10n.T("Baseball.Result.Win");
        }
        else
        {
            return L10n.T("UI.Label.Loss.Uppercase");
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
                return L10n.T("UI.Label.BlackCard");
            case KOBReward.Card_Legend:
                return L10n.T("UI.Label.LegendCard");
            case KOBReward.Card_Epic:
                return L10n.T("UI.Label.EpicCard");
            case KOBReward.Card_Rare:
                return L10n.T("UI.Label.RareCard");
            default:
                return L10n.T("UI.Label.CommonCard");
        }
    }
}
