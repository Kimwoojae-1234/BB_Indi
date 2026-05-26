using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class AchievementChart : Chart
{
    private readonly Dictionary<int, AchievementData> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, AchievementData> Dictionary => (IReadOnlyDictionary<int, AchievementData>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "AchievementData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            AchievementData info = new AchievementData(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }


}

public class AchievementData
{
    public int idx { get; private set; }
    public string name_id { get; private set; }
    public string desc_id { get; private set; }
    public int count { get; private set; }
    public int next_count { get; private set; }
    public int reward { get; private set; }
    public int add_reward { get; private set; }
    public int rIndex { get; private set; } //레코드 인덱스
    public AchievementData(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        name_id = json["name_id"].ToString();
        desc_id = json["desc_id"].ToString();
        count = int.Parse(json["count"].ToString());
        next_count = int.Parse(json["next_count"].ToString());
        reward = int.Parse(json["reward"].ToString());
        add_reward = int.Parse(json["add_reward"].ToString());
        rIndex = int.Parse(json["rIndex"].ToString());
    }
}