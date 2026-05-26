using System.Collections.Generic;
using BackendData.Base;
using LitJson;
using Unity.VisualScripting;
using System;

public class GearAttributesChart : Chart
{
    private readonly Dictionary<int, GearAttributes> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, GearAttributes> Dictionary => (IReadOnlyDictionary<int, GearAttributes>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "GearAttributes";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            GearAttributes info = new GearAttributes(eachItem);
            _dictionary.Add(info.ref_idx, info);
        }
    }

    public GearAttributes GetData(int idx)
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

public class GearAttributes
{
    public int ref_idx { get; private set; }
    public int pow { get; private set; }
    public int con { get; private set; }
    public int vis { get; private set; }
    public int fld { get; private set; }
    public int thw { get; private set; }
    public int spd { get; private set; }
    public int skill1 { get; private set; }
    public int skill2 { get; private set; }

    public GearAttributes(JsonData json)
    {
        ref_idx = KOBTableUtil.ParseSafeInt(json, "ref_idx");
        pow = KOBTableUtil.ParseSafeInt(json, "pow");
        con = KOBTableUtil.ParseSafeInt(json, "con");
        vis = KOBTableUtil.ParseSafeInt(json, "vis");
        fld = KOBTableUtil.ParseSafeInt(json, "fld");
        thw = KOBTableUtil.ParseSafeInt(json, "thw");
        spd = KOBTableUtil.ParseSafeInt(json, "spd");
        skill1 = KOBTableUtil.ParseSafeInt(json, "skill1");
        skill2 = KOBTableUtil.ParseSafeInt(json, "skill2");
    }
}

