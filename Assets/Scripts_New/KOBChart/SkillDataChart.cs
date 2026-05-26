using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using BackEnd;
using LitJson;
using Unity.VisualScripting;

public class SkillDataChart : Chart
{
    private readonly Dictionary<int, SkillData> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    public IReadOnlyDictionary<int, SkillData> Dictionary => (IReadOnlyDictionary<int, SkillData>)_dictionary.AsReadOnlyCollection();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "SkillData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            SkillData info = new SkillData(eachItem);
            _dictionary.Add(info.skill_idx, info);
        }
    }

    public SkillData GetData(int idx)
    {
        if(_dictionary.ContainsKey(idx))
        {
            return _dictionary[idx];
        }
        else
        {
            return null;
        }
    }
}

public class SkillData
{
    public int skill_idx { get; private set; }    
    public KOBSkillType skill_type { get; private set; }
    public List<SkillElement> level_value { get; private set; }
    

    public SkillData(JsonData json)
    {
        skill_idx = int.Parse(json["skill_idx"].ToString());        
        skill_type = KOBTableUtil.ParseEnumFromJson<KOBSkillType>(json, "skill_type");
        level_value = new List<SkillElement>();
        string key1 = KOBTableUtil.ParseSafeString(json, "Key1");
        if (key1 != null)
        {
            level_value.Add(new SkillElement(json, 1));            
            string key2 = KOBTableUtil.ParseSafeString(json, "Key2");
            if (key2 != null)
            {
                level_value.Add(new SkillElement(json, 2));
                string key3 = KOBTableUtil.ParseSafeString(json, "Key3");
                if (key3 != null)
                {
                    level_value.Add(new SkillElement(json, 3));
                }
            }
        }
    }

    public string name_id
    {
        get
        {
            return string.Format("SkillName.{0}", skill_idx);
        }
    }
    public string desc_id
    {
        get
        {
            return string.Format("SkillDesc.{0}", skill_idx);
        }
    }
}


public class SkillElement
{
    public KOBSkillKey Key;         //스킬 키
    public KOBSkillCondition Con;   //조건
    public int[] Per;                 //발동 퍼센트(0~100)
    public int[] Val;                 //10000 보다 큰경우 %로 계산 (10000으로 나눈후 계산)

    public SkillElement(JsonData json, int order)
    {
        string _key = string.Format("Key{0}", order);
        string _con = string.Format("Con{0}", order);
        string _per = string.Format("Per{0}", order);
        string _val = string.Format("Val{0}", order);
        Key = KOBTableUtil.ParseEnumFromJson<KOBSkillKey>(json, _key);
        Con = KOBTableUtil.ParseEnumFromJson<KOBSkillCondition>(json, _con);
        Per = JsonHelper.DeserializeObject<int[]>(json[_per].ToString());
        Val = JsonHelper.DeserializeObject<int[]>(json[_val].ToString());
    }
}


