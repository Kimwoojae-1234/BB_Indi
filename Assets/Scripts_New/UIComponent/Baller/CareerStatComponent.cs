using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CareerStatComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Title;
    [SerializeField] private TextMeshProUGUI[] StatTxt;

    public void InitTotal(int _idx, Dictionary<int, int[]> BallerStat)
    {
        Title.text = "Career Stats";

        int[] totalRecord = new int[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        foreach (KeyValuePair<int, int[]> stat in BallerStat)
        {
            int[] record = stat.Value;
            for(int i = 0; i<record.Length;i++)
            {
                totalRecord[i] += record[i];
            }
        }

        string[] value = getValue(totalRecord);
        for(int i  =0;i< StatTxt.Length;i++)
        {
            StatTxt[i].text = value[i];
        }
    }

    public void InitLeague(int league, int _idx, Dictionary<int, int[]> BallerStat)
    {
        Title.text = string.Format("League {0} Stats", league);

        int[] totalRecord = new int[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        if(BallerStat.ContainsKey(league) == true)
        {
            int[] record = BallerStat[league];
            for (int i = 0; i < record.Length; i++)
            {
                totalRecord[i] += record[i];
            }
        }

        string[] value = getValue(totalRecord);
        for (int i = 0; i < StatTxt.Length; i++)
        {
            StatTxt[i].text = value[i];
        }
    }


    private static string [] getValue(int [] record)
    {/*
        int _pa = 0; //0
        int _ab = 0;//1
        int _h = 0;//2
        int _2b = 0;//3
        int _3b = 0;//4
        int _hr = 0;//5
        int _rbi = 0;//6
        int _sb = 0;//7
        int _bb = 0;//8
        int _so = 0;//9
        int run = 0;//10*/

        //ab //0
        //hit 1
        //hr 2
        //rbi 3 
        //run 4
        //steal 5
        //bb 6
        //so 7
        //avg 8
        //obp 9
        //slg 10
        //ops 11

        string[] value = new string[12];
        

        return value;
    }

}
