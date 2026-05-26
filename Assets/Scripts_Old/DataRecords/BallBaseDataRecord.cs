using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BallBaseData
{
	public int Idx { get; set; } // 카드 인덱스
	public string NameId { get; set; } // 카드 이름
	public string DescId { get; set; } // 카드 설명
	public short AbilityTypeA { get; set; } // 
	public short AbilityTypeALevel { get; set; } // 특수능력치 타입 A 레벨
	public short AbilityTypeB { get; set; } // 
	public short AbilityTypeBLevel { get; set; } // 특수능력치 타입 B 레벨
	public short AbilityTypeC { get; set; } // 
	public short AbilityTypeCLevel { get; set; } // 특수능력치 타입 C 레벨
	public string IconTag { get; set; } // 아이콘 태그
	public string TextureTag { get; set; } // 아이콘 태그
	public string IngameColor { get; set; } // 인게임 컬러
	public int Display { get; set; } // 
}


public class BallBaseDataRecord : BaseDataRecord
{
	public BallBaseData[] BallBaseData = null;

    public override bool Initialize()
    {
        return base.Initialize();
    }

    public override bool Uninitialize()
    {
        return base.Uninitialize();
    }
}
