using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatAbilityBaseData
{
	public int Idx { get; set; } // 능력치 인덱스
	public string NameId { get; set; } // 능력치 이름
	public string DescId { get; set; } // 능력치 설명
	public string IconTag { get; set; } // 아이콘 태그
}


public class BatAbilityBaseDataRecord : BaseDataRecord
{
	public BatAbilityBaseData[] BatAbilityBaseData = null;

	public override bool Initialize()
	{
		return base.Initialize();
	}

	public override bool Uninitialize()
	{
		return base.Uninitialize();
	}
}
