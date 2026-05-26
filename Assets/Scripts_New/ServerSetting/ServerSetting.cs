using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;

public class ServerSetting
{
    public int InitGem { get; private set; }
    public int InitGold { get; private set; }
    public int InitStamina { get; private set; }
    public int InitMaxStamina { get; private set; }
    public int [] MaxGameToken { get; private set; }
    public int MaxPassToken { get; private set; }
    public int[] InitStats_Type0 { get; private set; } //밸런스타입
    public int[] InitStats_Type1 { get; private set; } //파워타입
    public int[] InitStats_Type2 { get; private set; } //컨택타입
    public int[] InitStats_Type3 { get; private set; } //수비타입
    public int[] SkillSlotLevel { get; private set; }  //스킬 슬롯
    public int[] GearSlotLevel { get; private set; }  //장비 슬롯
    public int BaseStatValue { get; private set; }

    public void InitLocal()
    {
        InitGem = 0;
        InitGold = 0;
        InitStamina = 60;
        InitMaxStamina = 60;
        MaxGameToken = new int[] { 100, 150, 200 };
        MaxPassToken = 10;
        InitStats_Type0 = new int[] { 50, 50, 50, 50, 50, 50 };
        InitStats_Type1 = new int[] { 75, 45, 45, 35, 55, 45 };
        InitStats_Type2 = new int[] { 45, 65, 50, 50, 35, 55 };
        InitStats_Type3 = new int[] { 40, 40, 40, 65, 60, 55 };
        SkillSlotLevel = new int[] { 0, 5, 9, 12 };
        GearSlotLevel = new int[] { 0, 6, 11 };
        BaseStatValue = 49;
    }

    public void InitFromServer()
    {
        InitLocal();

        //서버세팅 폴더
        var bro = Backend.Chart.GetChartListByFolder(1305);
        if (bro.IsSuccess() == false)
        {
            return;            
        }
        JsonData json = bro.FlattenRows()[0];
        string id = json["selectedChartFileId"].ToString();

        var serverChartBro = Backend.Chart.GetChartContents(id);
        if(serverChartBro.IsSuccess()==false)
        {
            return;
        }

        JsonData json2 = serverChartBro.FlattenRows();
        for(int i=0; i<json2.Count;i++)
        {
            string key = json2[i]["idx"].ToString();
            string value = json2[i]["value"].ToString();
            SetValue(key, value);
        }

    }


    private void SetValue(string key, string value)
    {        
        switch (key)
        {
            case "InitGem":
                InitGem = int.Parse(value);
                break;
            case "InitGold":
                InitGold = int.Parse(value);
                break;
            case "InitStamina":
                InitStamina = int.Parse(value);
                break;
            case "InitMaxStamina":
                InitMaxStamina = int.Parse(value);
                break;
            case "MaxGameToken":
                MaxGameToken = JsonHelper.DeserializeObject<int[]>(value);
                break;
            case "MaxPassToken":
                MaxPassToken = int.Parse(value);
                break;
            case "InitStats_Type0":
                InitStats_Type0 = JsonHelper.DeserializeObject<int[]>(value);
                break;
            case "InitStats_Type1":
                InitStats_Type1 = JsonHelper.DeserializeObject<int[]>(value);
                break;
            case "InitStats_Type2":
                InitStats_Type2 = JsonHelper.DeserializeObject<int[]>(value);
                break;
            case "InitStats_Type3":
                InitStats_Type3 = JsonHelper.DeserializeObject<int[]>(value);
                break;
            case "SkillSlotLevel":
                SkillSlotLevel = JsonHelper.DeserializeObject<int[]>(value);
                break;
            case "GearSlotLevel":
                GearSlotLevel = JsonHelper.DeserializeObject<int[]>(value);
                break;
            case "BaseStatValue":
                BaseStatValue = int.Parse(value);
                break;

        }
    }
}
