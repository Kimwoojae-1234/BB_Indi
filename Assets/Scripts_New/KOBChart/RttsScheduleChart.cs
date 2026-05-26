using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class RttsScheduleChart : Chart
{
    private readonly Dictionary<int, RttsSchedule> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, RttsSchedule> Dictionary => (IReadOnlyDictionary<int, RttsSchedule>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "RttsSchedule";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            RttsSchedule info = new RttsSchedule(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }

    public RttsSchedule GetSchedule(int idx)
    {
        return _dictionary[idx];
    }
}

public class RttsSchedule
{
    public int idx { get; private set; }
    public int opponent { get; private set; }
    public bool day { get; private set; }
    public bool home { get; private set; }

    public RttsSchedule(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        opponent = int.Parse(json["opponent"].ToString());
        day = bool.Parse(json["day"].ToString());
        home = bool.Parse(json["home"].ToString());
    }
}
