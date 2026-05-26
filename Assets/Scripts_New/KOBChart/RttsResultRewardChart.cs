using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;
using System.Linq;

public class RttsResultRewardChart : Chart
{
    private readonly Dictionary<int, List<RttsResultReward>> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, List<RttsResultReward>> Dictionary => (IReadOnlyDictionary<int, List<RttsResultReward>>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "RttsResultReward";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {       
        foreach (JsonData eachItem in json)
        {
            RttsResultReward info = new RttsResultReward(eachItem); // JsonData → RewardData 변환
            int key = info.League;
            if (!_dictionary.TryGetValue(key, out var list))
            {
                list = new List<RttsResultReward>();
                _dictionary[key] = list;
            }
            list.Add(info);
        }
    }

    public List<RttsResultReward> GetResultRewards(int league)
    {
        if (Dictionary.ContainsKey(league))
        {
            return Dictionary[league];
        }
        else
        {
            return null;
        }
    }


    public int GetRewardIndex(int league, int resultStep)
    {
        List<RttsResultReward> list = GetResultRewards(league);
        if (list != null)
        {
            RttsResultReward result = list.FirstOrDefault(r => r.ResultStep == resultStep);
            if(result != null)
            {
                return result.reward_index;
            }
        }

        return -1;
    }
}

public class RttsResultReward
{
    public int idx { get; private set; }
    public int League { get; private set; }
    public int ResultStep { get; private set; }
    public int reward_index { get; private set; }
    public int pindex { get; private set; }

    public RttsResultReward(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        League = int.Parse(json["League"].ToString());
        ResultStep = int.Parse(json["ResultStep"].ToString());
        reward_index = int.Parse(json["reward_index"].ToString());
        pindex = int.Parse(json["pindex"].ToString());
    }
}