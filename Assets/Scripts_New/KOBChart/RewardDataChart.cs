using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class RewardDataChart : Chart
{
    private readonly Dictionary<int, List<RewardData>> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, List<RewardData>> Dictionary => (IReadOnlyDictionary<int, List<RewardData>>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "RewardData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {        
        foreach (JsonData eachItem in json)
        {
            RewardData info = new RewardData(eachItem); // JsonData → RewardData 변환
            int key = info.reward_index;
            if (!_dictionary.TryGetValue(key, out var list))
            {
                list = new List<RewardData>();
                _dictionary[key] = list;
            }
            list.Add(info);
        }
    }

    public List<RewardData> GetRewards(int refID)
    {
        if(Dictionary.ContainsKey(refID))
        {
            return Dictionary[refID];
        }
        else
        {
            return null;
        }
    }

    public RewardData GetReward(int refID)
    {
        if (Dictionary.ContainsKey(refID))
        {
            return Dictionary[refID][0];
        }
        else
        {
            return null;
        }
    }

}

public class RewardData
{
    public int idx { get; private set; }
    public int reward_index { get; private set; }
    public KOBReward reward { get; private set; }
    public int pindex { get; private set; }
    public int min { get; private set; }
    public int max { get; private set; }


    public RewardData(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        reward_index = int.Parse(json["reward_index"].ToString());
        reward =  (KOBReward)System.Enum.Parse(typeof(KOBReward), json["reward"].ToString());
        pindex = int.Parse(json["pindex"].ToString());
        min = int.Parse(json["min"].ToString());
        max = int.Parse(json["max"].ToString());
    }

}
