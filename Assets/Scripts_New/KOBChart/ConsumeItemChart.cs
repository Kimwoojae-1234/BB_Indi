using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class ConsumeItemChart : Chart
{
    private readonly Dictionary<int, ConsumeItem> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, ConsumeItem> Dictionary => (IReadOnlyDictionary<int, ConsumeItem>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "ConsumeItem";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            ConsumeItem info = new ConsumeItem(eachItem);
            _dictionary.Add(info.idx, info);
        }
    }
}

public class ConsumeItem
{
    public int idx { get; private set; }
    public string name_id { get; private set; }
    public string desc_id { get; private set; }
    public KOBRarity rarity { get; private set; }
    public KOBConsumeItemType item_type { get; private set; }
    public ConsumeItemElement value1 { get; private set; }
    public ConsumeItemElement value2 { get; private set; }
    public ConsumeItemElement value3 { get; private set; }

    public ConsumeItem(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        name_id = json["name_id"].ToString();
        desc_id = json["desc_id"].ToString();
        rarity = (KOBRarity)System.Enum.Parse(typeof(KOBRarity), json["rarity"].ToString());
        //Value = JsonHelper.DeserializeObject<Dictionary<int, int>>(json["Value"].ToString());
        //Overall = int.Parse(json["Overall"].ToString());
    }
}



public class ConsumeItemElement
{
    public KOBItemKey Key;
    public int Activation;
    public int Value;
}