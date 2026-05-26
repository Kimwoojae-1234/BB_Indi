using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class RttsTeamChart : Chart
{
    private readonly Dictionary<int, RttsTeam> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, RttsTeam> Dictionary => (IReadOnlyDictionary<int, RttsTeam>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "RttsTeam";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            RttsTeam info = new RttsTeam(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }

    public RttsTeam GetRttsTeam(int idx)
    {
        if (Dictionary.ContainsKey(idx))
        {
            return Dictionary[idx];
        }
        else
        {
            return null;
        }
    }
}

public class RttsTeam
{
    public int idx { get; private set; }
    public int[] Player { get; private set; }
    public int Pitcher { get; private set; }
    public int[] Level { get; private set; }
    public int[] Pos { get; private set; }
    public int Logo { get; private set; }
    public string Name { get; private set; }

    public RttsTeam(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        Player = JsonHelper.DeserializeObject<int[]>(json["Player"].ToString());
        Pitcher = int.Parse(json["Pitcher"].ToString());
        Level = JsonHelper.DeserializeObject<int[]>(json["Level"].ToString());
        Pos = JsonHelper.DeserializeObject<int[]>(json["Pos"].ToString());
        Logo = int.Parse(json["Logo"].ToString()); ;
        Name = json["Name"].ToString();

    }
}