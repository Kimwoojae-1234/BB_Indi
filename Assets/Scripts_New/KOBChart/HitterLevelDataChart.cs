using System.Collections.Generic;
using BackendData.Base;
using LitJson;
using Unity.VisualScripting;

public class HitterLevelDataChart : Chart
{
    private readonly Dictionary<int, Dictionary<int, HitterLevelData>> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, Dictionary<int, HitterLevelData>> _Dictionary => (IReadOnlyDictionary<int, Dictionary<int, HitterLevelData>>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "HitterLevelData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            HitterLevelData info = new HitterLevelData(eachItem);
            int key = info.char_idx;
            if (!_dictionary.TryGetValue(key, out var list))
            {
                list = new Dictionary<int, HitterLevelData>();
                _dictionary[key] = list;
            }
            list.Add(info.level,info);
        }
        //UnityEngine.Debug.Log("aaaaaaaaaaaaaaaa");
    }


    public HitterLevelData GetData(int idx, int level)
    {
        if(_dictionary.ContainsKey(idx))
        {
            if (_dictionary[idx].ContainsKey(level))
            {
                return _dictionary[idx][level]; 
            }
        }
        return null;
    }

}

public class HitterLevelData
{
    public int idx { get; private set; }
    public int char_idx { get; private set; }
    public int level { get; private set; }
    public int power { get; private set; }
    public int contact { get; private set; }
    public int vision { get; private set; }
    public int fielding { get; private set; }
    public int throwing { get; private set; }
    public int speed { get; private set; }



    public HitterLevelData(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        char_idx = int.Parse(json["char_idx"].ToString());
        level = int.Parse(json["level"].ToString());
        power = int.Parse(json["power"].ToString());
        contact = int.Parse(json["contact"].ToString());
        vision = int.Parse(json["vision"].ToString());
        fielding = int.Parse(json["fielding"].ToString());
        throwing = int.Parse(json["throwing"].ToString());
        speed = int.Parse(json["speed"].ToString());
    }
}

