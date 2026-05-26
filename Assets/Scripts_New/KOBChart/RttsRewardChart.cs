using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;


public class RttsRewardChart : Chart
{
    private readonly Dictionary<int, RttsReward> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, RttsReward> Dictionary => (IReadOnlyDictionary<int, RttsReward>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "RttsReward";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            RttsReward info = new RttsReward(eachItem);
            _dictionary.Add(info.league_idx, info);
        }
    }

    public RttsReward GetRttsReward(int League)
    {
        if (Dictionary.ContainsKey(League))
        {
            return Dictionary[League];
        }
        else
        {
            return null;
        }
    }



}


public class RttsReward
{
    public int league_idx { get; private set; }
    public int [] small_reward_pos { get; private set; }
    public int [] big_reward_pos { get; private set; }
    public int [] winall_reward { get; private set; }

    public RttsReward(JsonData json)
    {
        league_idx = int.Parse(json["league_idx"].ToString());
        small_reward_pos = JsonHelper.DeserializeObject<int[]>(json["small_reward_pos"].ToString());
        big_reward_pos = JsonHelper.DeserializeObject<int[]>(json["big_reward_pos"].ToString());
        winall_reward = JsonHelper.DeserializeObject<int[]>(json["winall_reward"].ToString());
    }
}