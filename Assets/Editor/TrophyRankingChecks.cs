using System;
using UnityEditor;
using UnityEngine;

/// <summary>Explicit editor checks; never runs automatically on import.</summary>
public static class TrophyRankingChecks
{
    [MenuItem("KOB/Validation/Validate Ranking Assets")]
    public static void Validate()
    {
        var ranking = Resources.Load<GameObject>("UI/Window/UI_Ranking");
        Require(ranking != null && ranking.GetComponent<UI_Ranking>() != null, "Ranking prefab");
        foreach (string path in new[] { "ScrollRect/Mask/Content", "ScrollRect/List_Me/Text_NickName", "Tab_Menu/Text_Golbal", "Tab_Menu/Text_Country", "Tab_Menu/Text_Friends", "Icon_Time/Text_Time", "Icon_Time/Text_Reset" })
            Require(ranking.transform.Find(path) != null, path);
        var content = ranking.transform.Find("ScrollRect/Mask/Content");
        Require(content.childCount >= 4, "Ranking row templates");
        for (int i = 0; i < 4; i++)
        {
            Require(content.GetChild(i).Find("Text_NickName") != null, "Row nickname");
            Require(content.GetChild(i).Find("Text_Score") != null, "Row score");
            Require(content.GetChild(i).Find("Icon_Country") != null, "Row country");
        }
        for (int i = 1; i <= 8; i++) Require(UserIdentity.Portrait(i) != null, "Portrait " + i);
        Require(UserIdentity.Flag("KR") != null, "Korean flag");
        var popup = Resources.Load<GameObject>("UI/Popup/Popup_UserInfo");
        Require(popup.transform.Find("Popup/Group_Top/Text_Tag") != null, "Popup country label");
        UserIdentity.Decode("8|KR", out int profile, out string country);
        Require(profile == 8 && country == "KR", "Profile round trip");
        UserIdentity.Decode("invalid", out profile, out country);
        Require(profile == 1 && country == "", "Legacy profile fallback");
        Debug.Log("RANKING_ASSETS_PASS");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new Exception("Ranking validation: " + message);
    }
}

