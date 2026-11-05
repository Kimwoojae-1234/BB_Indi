using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KOBTextUtil
{

    public static string[] _RecordTypeName => new string[] { UILocalization.Get("ui.ecf97abbc4b5"), UILocalization.Get("ui.9acfc28b3e11"), UILocalization.Get("ui.48c3eda42f37"), UILocalization.Get("ui.97724e923ef0"), UILocalization.Get("ui.4109aee5faa1") };

    public static string GetMyTeamName()
    {
        return UILocalization.Get("ui.4d050045d551");
    }

    public static string GetMyPlayerName(int idx)
    {
        return UILocalization.Get(KOBManager.Backend.Chart.CharacterData.GetData(idx)?.name_id ?? "");
    }


    public static string SetRankText(int rank)
    {
        if (rank <= 0)
        {
            return UILocalization.Get("ui.14dfd4fe467b");
        }
        else
        {
            return UILocalization.Format("ui.ff53c335fc87", ToOrdinal(rank));
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
            return UILocalization.Format("ui.021833c2a40d", per, w, d, l);
        }
        else
        {
            return per;
        }
    }


    public static string ToOrdinal(int number)
    {
        if (number <= 0 || UILocalization.Language != "en") return number.ToString();

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
            return UILocalization.Get("ui.3f9069621a44");
        }
        else if (score1 > score2)
        {
            return UILocalization.Get("ui.662fae5f621a");
        }
        else
        {
            return UILocalization.Get("ui.72ab4b12b8a9");
        }
    }

    public static Color32 SetResultColor(int score1, int score2)
    {
        if (score1 == score2)
        {
            return new Color32(149, 165, 166, 255); // #95A5A6 ȸ��
        }
        else if (score1 > score2)
        {
            return new Color32(76, 175, 80, 255);   // #4CAF50 �ʷ�
        }
        else
        {
            return new Color32(231, 76, 60, 255);   // #E74C3C ����
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
                return UILocalization.Get("ui.56d3f4f620f8");
            case KOBReward.Card_Legend:
                return UILocalization.Get("ui.d8fd756f00d3");
            case KOBReward.Card_Epic:
                return UILocalization.Get("ui.a770463e4e13");
            case KOBReward.Card_Rare:
                return UILocalization.Get("ui.c2b39e11fc28");
            default:
                return UILocalization.Get("ui.8609d467ec7e");
        }
    }
}
