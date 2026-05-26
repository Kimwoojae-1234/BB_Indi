using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class SeasonPassBaseChart : Chart
{
    private readonly Dictionary<int, SeasonPassBase> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, SeasonPassBase> Dictionary => (IReadOnlyDictionary<int, SeasonPassBase>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "SeasonPassBase";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            SeasonPassBase info = new SeasonPassBase(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }
}

public class SeasonPassBase
{
    public int idx { get; private set; }
    public int season { get; private set; }
    public string pass_start_date { get; private set; }
    public string pass_end_date { get; private set; }

    public SeasonPassBase(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        season = int.Parse(json["season"].ToString());
        pass_start_date = json["pass_start_date"].ToString();
        pass_end_date = json["pass_end_date"].ToString();
    }
}

