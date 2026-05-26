using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class SeasonPassRewardChart : Chart
{
    private readonly Dictionary<int, SeasonPassReward> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, SeasonPassReward> Dictionary => (IReadOnlyDictionary<int, SeasonPassReward>)_dictionary.AsReadOnlyCollection();

    
    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "SeasonPassReward";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            SeasonPassReward info = new SeasonPassReward(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }
}

public class SeasonPassReward
{
    public int idx { get; private set; }
    public int GoldenType { get; private set; }
    public int GoldenValue1 { get; private set; }
    public int GoldenValue2 { get; private set; }
    public int FreeType { get; private set; }
    public int FreeValue1 { get; private set; }
    public int FreeValue2 { get; private set; }

    public SeasonPassReward(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        GoldenType = int.Parse(json["GoldenType"].ToString());
        GoldenValue1 = int.Parse(json["GoldenValue1"].ToString());
        GoldenValue2 = int.Parse(json["GoldenValue2"].ToString());
        FreeType = int.Parse(json["FreeType"].ToString());
        FreeValue1 = int.Parse(json["FreeValue1"].ToString());
        FreeValue2 = int.Parse(json["FreeValue2"].ToString());
    }
}

