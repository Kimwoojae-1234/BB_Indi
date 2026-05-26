using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class RttsInfoChart : Chart
{
    private readonly Dictionary<int, RttsInfo> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, RttsInfo> Dictionary => (IReadOnlyDictionary<int, RttsInfo>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "RttsInfo";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            RttsInfo info = new RttsInfo(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }


    public RttsInfo GetRttsInfo(int league)
    {   

        if (Dictionary.ContainsKey(league) == true)
        {
            return Dictionary[league];
        }
        else
        {
            return null;
        }
    }

}

public class RttsInfo
{
    public int idx { get; private set; }
    public int TotalGame { get; private set; }
    public bool isSeries { get; private set; }
    public bool Playoff { get; private set; }
    public bool Open { get; private set; }
    public string Name { get; private set; }
    public string Desc { get; private set; }
    public int Win { get; private set; }
    public int Lose { get; private set; }
    public int Draw { get; private set; }
    public int Fame { get; private set; }
          

    public RttsInfo(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        TotalGame = int.Parse(json["TotalGame"].ToString());
        isSeries = bool.Parse(json["isSeries"].ToString());
        Playoff = bool.Parse(json["Playoff"].ToString());
        Open = bool.Parse(json["Open"].ToString());
        Name = json["Name"].ToString();
        Desc = json["Desc"].ToString();
        Win = int.Parse(json["Win"].ToString());
        Lose = int.Parse(json["Lose"].ToString());
        Draw = int.Parse(json["Draw"].ToString());
        Fame = int.Parse(json["Fame"].ToString());
        //Team = JsonHelper.DeserializeObject<int[]>(json["Team"].ToString());
    }
}