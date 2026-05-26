using System.Collections.Generic;
using BackendData.Base;
using LitJson;
using Unity.VisualScripting;

public class HitterSkillDataChart : Chart
{
    private readonly Dictionary<int, HitterSkillData> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, HitterSkillData> Dictionary => (IReadOnlyDictionary<int, HitterSkillData>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "HitterSkillData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            HitterSkillData info = new HitterSkillData(eachItem);
            _dictionary.Add(info.char_idx, info);
        }
    }

    public HitterSkillData GetData(int idx)
    {
        if (_dictionary.ContainsKey(idx))
        {
            return _dictionary[idx];
        }
        return null;
    }

}

public class HitterSkillData
{
    public int idx { get; private set; }
    public int char_idx { get; private set; }
    public List<int> slot_list { get; private set; }
    public List<int> skill_list { get; private set; }
    public int special_skill { get; private set; }
    public int[] special_unlock { get; private set; }



    public HitterSkillData(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        char_idx = int.Parse(json["char_idx"].ToString());
        slot_list = new List<int>();
        int slot1 = int.Parse(json["slot1"].ToString());
        if(slot1 > 0)
        {
            slot_list.Add(slot1);
            int slot2 = int.Parse(json["slot2"].ToString());
            if (slot2 > 0)
            {
                slot_list.Add(slot2);
                int slot3 = int.Parse(json["slot3"].ToString());
                if (slot3 > 0)
                {
                    slot_list.Add(slot3);
                    int slot4 = int.Parse(json["slot4"].ToString());
                    if (slot4 > 0)
                    {
                        slot_list.Add(slot4);
                        int slot5 = int.Parse(json["slot5"].ToString());
                        if (slot5 > 0)
                        {
                            slot_list.Add(slot5);
                        }
                    }
                }
            }
        }
        skill_list = JsonHelper.DeserializeObject<List<int>>(json["skill_list"].ToString());
        if (json.ContainsKey("special_skill") && json["special_unlock"].ToString() != "null")
            special_skill = int.Parse(json["special_skill"].ToString());
        special_unlock = JsonHelper.DeserializeObject<int[]>(json["special_unlock"].ToString());

    }
}

