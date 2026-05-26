using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;
using System.Xml;

public class TrophyRoadChart : Chart
{
    private readonly Dictionary<int, TrophyRoad> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, TrophyRoad> Dictionary => (IReadOnlyDictionary<int, TrophyRoad>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "TrophyRoadData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            TrophyRoad info = new TrophyRoad(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }


    public int GetCurrentTier(int trophy)
    {
        int curTier = 0;
        foreach (KeyValuePair<int, TrophyRoad> pair in Dictionary)
        {
            TrophyRoad value = pair.Value;
            if(value.tier != 0)
            {
                if (trophy >= value.trophy)
                {
                    curTier = value.tier;
                }
                else
                {
                    break;
                }
            }
        }
        return curTier;
    }

    public LobbyTrophyInfo GetLobbyTrophyInfo(int trophy)
    {
        List<int> TropyGetList = KOBManager.MyInfo.GameData.GrowthInfo.TropyGetList;
        List<int> PossibleList = new List<int>(); //지났는데 받을수 있는 리스트
        PossibleList.Clear();

        int CurrentKey = 0; //현재 막 지난 키
        int CurTier = 0;    //현재 티어
        int NotiCount = 0;
        float _slider = 0;
        int _reward_idx = 0;
        int _pindex = 0;

        foreach (KeyValuePair<int, TrophyRoad> item in Dictionary)
        {
            if (trophy >= item.Value.trophy)
            {
                if (TropyGetList.Contains(item.Key) == false)
                {
                    NotiCount++;
                    PossibleList.Add(item.Key);
                }

                if (item.Value.tier != 0)
                {
                    CurTier = item.Value.tier;
                }

                if (Dictionary.ContainsKey(item.Key + 1) == true)
                {
                    if (trophy < Dictionary[item.Key + 1].trophy)
                    {
                        CurrentKey = item.Key; //다 받은 경우 이걸 사용
                        break;
                    }
                }
            }
        }

        bool _isMaxTier = (CurTier >= KOBConstant.MAX_TROPHY_TIER);

        if (PossibleList.Count > 0) //중간에 받을게 있다면
        {
            CurrentKey = PossibleList[0]; //중간에 받을게 --> 제일 왼쪽에 있는 거
            _reward_idx = Dictionary[CurrentKey].reward_index;
            _pindex = Dictionary[CurrentKey].pindex;
            _slider = 1;
        }
        else
        {
            //다 받았으면 다음 목표
            if (Dictionary.ContainsKey(CurrentKey + 1) == true && _isMaxTier == false)
            {
                _reward_idx = Dictionary[CurrentKey + 1].reward_index;
                _pindex = Dictionary[CurrentKey + 1].pindex;

                int current = 0;
                int gab = 1;
                if (CurrentKey == 0)
                {
                    current = trophy;
                    gab = Dictionary[CurrentKey + 1].trophy;
                }
                else
                {
                    current = trophy - Dictionary[CurrentKey].trophy;
                    gab = Dictionary[CurrentKey + 1].trophy - Dictionary[CurrentKey].trophy;
                }
                _slider = (float)current / (float)(gab);
            }
            else _slider = 0;
        }


        return new LobbyTrophyInfo()
        {
            noti = NotiCount,
            Tier = CurTier,
            slide = _slider,
            reward_index = _reward_idx, 
            pindex = _pindex,
            isMaxTier = _isMaxTier
        };

    }




}

public class TrophyRoad
{
    public int idx { get; private set; }
    public int trophy { get; private set; }
    public int league { get; private set; } //안쓰는 것 같음
    public int tier { get; private set; }
    public int reward_index { get; private set; }
    public int pindex { get; private set; }

    public TrophyRoad(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        trophy = int.Parse(json["trophy"].ToString());
        league = int.Parse(json["league"].ToString());
        tier = int.Parse(json["tier"].ToString());
        reward_index = int.Parse(json["reward_index"].ToString());
        pindex = int.Parse(json["pindex"].ToString());
    }
}

