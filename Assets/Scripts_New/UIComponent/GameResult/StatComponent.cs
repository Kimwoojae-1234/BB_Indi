using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatComponent : MonoBehaviour
{
    [SerializeField] private Image BG;
    [SerializeField] private TextMeshProUGUI No;
    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Overall;
    [SerializeField] private TextMeshProUGUI Pos;
    [SerializeField] private TextMeshProUGUI[] Stat; 


    public void InitItem(int idx)
    {
        gameObject.SetActive(true);
        BG.color = (idx%2 ==0) ? Color.white : new Color(0.57f,0.57f,0.57f);
        No.text = idx.ToString();
        //Name.text = player.getName_EN();
        //Overall.text = player.getOffenseRating().ToString();//아무거나
        /*Pos.text = Util.GetPositionStringEng(player.getPosition());
        Stat[0].text = player.getRecord(Param.ST_AB).ToString();
        Stat[1].text = player.getRecord(Param.ST_H).ToString();
        Stat[2].text = player.getRecord(Param.ST_HR).ToString();
        Stat[3].text = player.getRecord(Param.ST_RBI).ToString();
        Stat[4].text = player.getRecord(Param.ST_SBS).ToString();
        Stat[5].text = player.getRecord(Param.ST_BB).ToString();
        Stat[6].text = player.getRecord(Param.ST_R).ToString();*/
    }

    public void InitItem(RttsPlayerGameRecord player, int idx)
    {
        if (player == null) return;
        gameObject.SetActive(true);
        if (BG != null) BG.color = (idx % 2 == 0) ? Color.white : new Color(0.57f, 0.57f, 0.57f);
        if (No != null) No.text = idx.ToString();
        if (Name != null) Name.text = player.Name;
        if (Overall != null) Overall.text = player.Overall.ToString();
        if (Pos != null) Pos.text = GetPositionName(player.Position);

        SetStat(0, player.Record[RttsPlayerGameRecord.AtBat]);
        SetStat(1, player.Record[RttsPlayerGameRecord.Hit]);
        SetStat(2, player.Record[RttsPlayerGameRecord.HomeRun]);
        SetStat(3, player.Record[RttsPlayerGameRecord.Rbi]);
        SetStat(4, player.Record[RttsPlayerGameRecord.Steal]);
        SetStat(5, player.Record[RttsPlayerGameRecord.Walk]);
        SetStat(6, player.Record[RttsPlayerGameRecord.Run]);
    }

    private void SetStat(int index, int value)
    {
        if (Stat != null && index >= 0 && index < Stat.Length && Stat[index] != null)
        {
            Stat[index].text = value.ToString();
        }
    }

    private static string GetPositionName(int position)
    {
        return System.Enum.IsDefined(typeof(KOBPosition), position)
            ? ((KOBPosition)position).ToString().ToUpperInvariant()
            : "-";
    }
}
