using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class CharacterChart : Chart
{
    private readonly Dictionary<int, CharacterData> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, CharacterData> Dictionary => (IReadOnlyDictionary<int, CharacterData>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "CharacterData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            CharacterData info = new CharacterData(eachItem);
            _dictionary.Add(info.char_idx, info);
        }
    }

    public CharacterData GetData(int idx)
    {
        if(Dictionary.ContainsKey(idx))
        {
            return Dictionary[idx];
        }
        else
        {
            return null;
        }
    }

    public List<int> GetBallersByRarityList(KOBRarity rarity) //희귀도에 구분된 볼러 리스트 얻어오기
    {
        List<int> list = new List<int>();
        foreach (KeyValuePair<int, CharacterData> player in Dictionary)
        {
            if(player.Value.rarity == rarity &&
               player.Value.char_type == CharacterType.Ballers) //볼러인 경우
            {
                list.Add(player.Key);
            }
        }
        return list;
    }


}

public class CharacterData
{
    public int idx { get; private set; }
    public int char_idx { get; private set; }
    public string name_id { get; private set; }
    public string desc_id { get; private set; }
    public int league { get; private set; }
    public KOBRarity rarity { get; private set; }
    public KOBHand hand { get; private set; }
    public PlayingType playingType { get; private set; } //쉽게 말하면 RPG의 클래스라고 보면됨 -> 특정스텟에보너스 -> 안타/홈런/타격/수비/주루 등 주작에 영향을 받는다(추후 상세기획)
    public CharacterType char_type { get; private set; } //캐릭터 타입에 타자/투수여부 포함
    public KOBPosition position { get; private set; } //포지션에 멀티포지션 여부 포함
    public KOBBody body { get; private set; } //body에 성별포함
   


    public CharacterData(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        char_idx = int.Parse(json["char_idx"].ToString());
        name_id = json["name_id"].ToString();
        desc_id = json["desc_id"].ToString();
        league = int.Parse(json["league"].ToString());
        rarity = (KOBRarity)int.Parse(json["rarity"].ToString());
        hand = (KOBHand)int.Parse(json["hand"].ToString());
        playingType = (PlayingType)int.Parse(json["playingType"].ToString());
        char_type = (CharacterType)int.Parse(json["char_type"].ToString());
        position = (KOBPosition)int.Parse(json["position"].ToString());
        body = (KOBBody)int.Parse(json["body"].ToString());
    }
}
