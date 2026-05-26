using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;
using LitJson;
using System.Linq;

public class UpgradeChart : Chart
{
    private readonly Dictionary<int, UpgradeData> _dictionary = new();

    // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
    //public IReadOnlyDictionary<int, UpgradeData> Dictionary => (IReadOnlyDictionary<int, UpgradeData>)_dictionary.AsReadOnlyCollection();

    private Dictionary<int, Dictionary<int,UpgradeData>> Dictionary = new Dictionary<int, Dictionary<int, UpgradeData>>();


    // 차트 파일 이름 설정 함수
    // 차트 불러오기를 공통적으로 처리하는 BackendChartDataLoad() 함수에서 해당 함수를 통해 차트 파일 이름을 얻는다.
    public override string GetChartFileName()
    {
        return "UpgradeData";
    }

    // Backend.Chart.GetChartContents에서 각 차트 형태에 맞게 파싱하는 클래스
    // 차트 정보 불러오는 함수는 BackendData.Base.Chart의 BackendChartDataLoad를 참고해주세요
    protected override void LoadChartDataTemplate(JsonData json)
    {
        foreach (JsonData eachItem in json)
        {
            UpgradeData info = new UpgradeData(eachItem);
            _dictionary.Add(info.idx, info);
        }

        Dictionary = _dictionary.Values
            .GroupBy(upgrade => upgrade.Group_Id) // Group_Id별로 그룹화
            .ToDictionary(
                g => g.Key, // key: Group_Id
                g => g.ToDictionary(
                    upgrade => upgrade.Level, // key: Level
                    upgrade => upgrade        // value
                )
            );
        //Debug.Log("aaaaaaaaaaaaaaaaa");
    }


    public int UpgradeGold(int level, KOBRarity rarity)
    {
        int lv = Mathf.Clamp(level, 1, 13);        
        if(Dictionary.ContainsKey(KOBConstant.UpgradeGoldIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UpgradeGoldIndex];
            return val[lv].Value[rarity];  
        }
        return 0;
    }

    public int UpgradeCard(int level, KOBRarity rarity)
    {
        int lv = Mathf.Clamp(level, 1, 13);        
        if (Dictionary.ContainsKey(KOBConstant.UpgradeCardIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UpgradeCardIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }

    public int UpgradeHittingSkill(int level, KOBRarity rarity)
    {
        int lv = Mathf.Clamp(level, 1, 5);
        if (Dictionary.ContainsKey(KOBConstant.UpgradeHittingSkillIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UpgradeHittingSkillIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }

    public int UpgradePhysicalSkill(int level, KOBRarity rarity)
    {        
        int lv = Mathf.Clamp(level, 1, 5);
        if (Dictionary.ContainsKey(KOBConstant.UpgradePhysicalSkillIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UpgradePhysicalSkillIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }

    public int UpgradeWideSkill(int level, KOBRarity rarity)
    {
        int lv = Mathf.Clamp(level, 1, 5);
        if (Dictionary.ContainsKey(KOBConstant.UpgradeWideSkillIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UpgradeWideSkillIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }


    public int UpgradeSpecialSkill(int level, KOBRarity rarity)
    {
        int lv = Mathf.Clamp(level, 1, 5);
        if (Dictionary.ContainsKey(KOBConstant.UpgradeSpecialSkillIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UpgradeSpecialSkillIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }

    public int UnlockHittingSkill(int level, KOBRarity rarity)
    {     
        int lv = Mathf.Clamp(level, 1, 5);
        if (Dictionary.ContainsKey(KOBConstant.UnlockHittingSkillIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UnlockHittingSkillIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }

    public int UnlockPhysicalSkill(int level, KOBRarity rarity)
    {        
        int lv = Mathf.Clamp(level, 1, 5);
        if (Dictionary.ContainsKey(KOBConstant.UnlockPhysicalSkillIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UnlockPhysicalSkillIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }

    public int UnlockWideSkill(int level, KOBRarity rarity)
    {        
        int lv = Mathf.Clamp(level, 1, 5);
        if (Dictionary.ContainsKey(KOBConstant.UnlockWideSkillIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UnlockWideSkillIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }


    public int UnlockSpecialSkill(int level, KOBRarity rarity)
    {
        int lv = Mathf.Clamp(level, 1, 5);
        if (Dictionary.ContainsKey(KOBConstant.UnlockSpecialSkillIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UnlockSpecialSkillIndex];
            return val[lv].Value[rarity];
        }
        return 0;
    }



    public Dictionary<KOBRarity, int> UnlockCost(int level)
    {        
        if (Dictionary.ContainsKey(KOBConstant.UnlockBallerIndex))
        {
            Dictionary<int, UpgradeData> val = Dictionary[KOBConstant.UnlockBallerIndex];
            return val[level].Value;
        }
        return null;
    }
}

public class UpgradeData
{
    public int idx { get; private set; }
    public int Group_Id { get; private set; }
    public int Level { get; private set; }
    public Dictionary<KOBRarity, int> Value { get; private set; }

    public UpgradeData(JsonData json)
    {
        idx = int.Parse(json["idx"].ToString());
        Group_Id = int.Parse(json["Group_Id"].ToString());
        Level = int.Parse(json["Level"].ToString());
        int common = int.Parse(json["COMMON"].ToString());
        int rare = int.Parse(json["RARE"].ToString());
        int epic = int.Parse(json["EPIC"].ToString());
        int legend = int.Parse(json["LEGENDARY"].ToString());
        int black = int.Parse(json["BLACK"].ToString());
        Value = new Dictionary<KOBRarity, int>();
        Value.Add(KOBRarity.COMMON, common);
        Value.Add(KOBRarity.RARE, rare);
        Value.Add(KOBRarity.EPIC, epic);
        Value.Add(KOBRarity.LEGENDARY, legend);
        Value.Add(KOBRarity.BLACK, black);
    }
}