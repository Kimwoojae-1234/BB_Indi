using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkillLevelData
{
	public int Idx { get; set; } // 스킬 인덱스
	public short Level { get; set; } // 레벨
	public int ActivateChance { get; set; } // 발동 확률
	public int SuccessChance { get; set; } // 성공 확률
	public short Effect1Id { get; set; } // 
	public int Effect1Value { get; set; } // 효과 1 값
	public short Effect2Id { get; set; } // 
	public int Effect2Value { get; set; } // 효과 2 값
	public short Effect3Id { get; set; } // 
	public int Effect3Value { get; set; } // 효과 3 값
	public short Effect4Id { get; set; } // 
	public int Effect4Value { get; set; } // 효과 4 값
	public short Effect5Id { get; set; } // 
	public int Effect5Value { get; set; } // 효과 5 값
	public short Effect6Id { get; set; } // 
	public int Effect6Value { get; set; } // 효과 6 값

}


public class SkillLevelDataRecord : BaseDataRecord
{
	public SkillLevelData[] SkillLevelData;

	public override bool Initialize()
	{
		return base.Initialize();
	}

	public override bool Uninitialize()
	{
		return base.Uninitialize();
	}
}
