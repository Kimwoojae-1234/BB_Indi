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
}
