using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SkillBaseData
{
	public int Idx { get; set; } // 카드 인덱스
	public string NameId { get; set; } // 카드 이름
	public string DescId { get; set; } // 카드 설명
	public short SkillType { get; set; } // 
	public short InvokeType { get; set; } // 
	public int CounterskillId { get; set; } // 
    public int Counterskill2Id { get; set; }
    public int CounterskillWeakId { get; set; }
    public int CounterskillWeak2Id { get; set; }
    public string IconTag { get; set; } // 아이콘 태그
    public short IngameSkillType { get; set; }

    public GameDefine.InGameSkillType eInGameSkillType
    {
        get
        {
            return (GameDefine.InGameSkillType)IngameSkillType;
        }
    }
    public bool IsLegendSkill()
    {
        return (eInGameSkillType >= GameDefine.InGameSkillType.LEGEND_STAT_INCREASE);
    }
}


public class SkillBaseDataRecord : BaseDataRecord
{
    public SkillBaseData[] SkillBaseData;

    public override bool Initialize()
    {
        return base.Initialize();
    }

    public override bool Uninitialize()
    {
        return base.Uninitialize();
    }
}
