using System.Collections.Generic;
using BackendData.Base;
using LitJson;
using Unity.VisualScripting;
using System;

public class GearDataChart : Chart
{
    private readonly Dictionary<int, GearData> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, GearData> Dictionary => (IReadOnlyDictionary<int, GearData>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "GearData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            GearData info = new GearData(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }

    public GearData GetData(int idx)
    {
        if (_dictionary.ContainsKey(idx))
        {
            return _dictionary[idx];
        }
        else
        {
            return null;
        }
    }

}

public class GearData
{
    public int idx { get; private set; }
    public KOBGearType gearType { get; private set; }
    public int att_refID { get; private set; }
    public int att_lv { get; private set; }
    public GearData(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        gearType = (KOBGearType)Enum.Parse(typeof(KOBGearType), json["gearType"].ToString());
        att_refID = int.Parse(json["att_refID"].ToString());
        att_lv = int.Parse(json["att_lv"].ToString());
    }

    public string name_id
    {
        get 
        { 
            return string.Format("GearName.{0}",idx); 
        }
    }

    public string desc_id
    {
        get
        {
            return string.Format("GearDesc.{0}", idx);
        }
    }

}

