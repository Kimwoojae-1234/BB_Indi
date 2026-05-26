using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;
using System.Linq;

public class BallerTrophyRoadChart : Chart
{
    private readonly Dictionary<int, BallerTrophyRoad> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, BallerTrophyRoad> Dictionary => (IReadOnlyDictionary<int, BallerTrophyRoad>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "BallerTrophyRoadData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            BallerTrophyRoad info = new BallerTrophyRoad(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }

    public BallerTierInfo GetTierInfo(int trophy)
    {
        int ballerTier = 0;
        int curTrophy = 0;
        int nextTrophy = 1;
        int tier_idx = 1;
        bool _isMax = (trophy >= KOBConstant.MAX_BALLER_FAME);
        
        while (true)
        {
            if (Dictionary.ContainsKey(tier_idx) == true)
            {
                if (trophy < Dictionary[tier_idx].trophy)
                {
                    nextTrophy = Dictionary[tier_idx].trophy;
                    break;
                }
                else
                {
                    curTrophy = Dictionary[tier_idx].trophy;
                    ballerTier++;
                    tier_idx++;
                }
            }
            else
            {
                break;
            }
        }

        BallerTierInfo value = new BallerTierInfo();
        value.CurTier = ballerTier;
        value.CurIdx = tier_idx;
        value.CurTrophy = curTrophy;
        value.NextTrophy = nextTrophy;
        value.isMax = _isMax;

        return value;

    }



    public int GetFameByTrophy(int currentTrophy)
    {
        // trophy 기준으로 정렬
        var sorted = Dictionary.Values.OrderBy(d => d.trophy).ToList();

        if (currentTrophy < sorted[0].trophy)
        {
            return 0;
        }
        else
        {
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                if (currentTrophy >= sorted[i].trophy && currentTrophy < sorted[i + 1].trophy)
                {
                    return sorted[i].tier;
                }
            }

            // 마지막 구간 이상이면 최고 티어
            return sorted.Last().tier;
        }
    }


}

public class BallerTrophyRoad
{
    public int idx { get; private set; }
    public int trophy { get; private set; }
    public int tier { get; private set; }
    public int reward_index { get; private set; }
    public int pindex { get; private set; }

    public BallerTrophyRoad(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        trophy = int.Parse(json["trophy"].ToString());
        tier = int.Parse(json["tier"].ToString());
        reward_index = int.Parse(json["reward_index"].ToString());
        pindex = int.Parse(json["pindex"].ToString());
    }
}


public class BallerTierInfo
{
    public int CurTier;
    public int CurIdx;
    public int CurTrophy;
    public int NextTrophy;
    public bool isMax;
}
