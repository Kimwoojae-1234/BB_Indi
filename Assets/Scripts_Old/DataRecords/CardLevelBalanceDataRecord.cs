using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CardLevelBalanceData
{
	public int Idx { get; set; } // 카드 인덱스
	public short Level { get; set; } // 레벨
	public short LvupLevel { get; set; } // 다음레벨
	public int LvupExp { get; set; } // LvUP 경험치
	public int LvupGamemoney { get; set; } // LvUP 게임머니
	public short AbilContact { get; set; } // 컨택
	public short AbilPower { get; set; } // 파워
	public short AbilVision { get; set; } // 선구
	public short AbilFielding { get; set; } // 필딩
	public short AbilThrowing { get; set; } // 송구
	public short AbilSpeed { get; set; } // 주루
	public short AbilControl { get; set; } // 제구
	public short AbilFastball { get; set; } // 패스트볼
	public short AbilCurve { get; set; } // 커브
	public short AbilSlider { get; set; } // 슬라이더
	public short AbilSinker { get; set; } // 싱커
	public short AbilChangeup { get; set; } // 체인지업
	public short AbilPickoff { get; set; } // 견제 횟수
	public int Skill1Id { get; set; } // 
	public short Skill1Lv { get; set; } // 스킬 1 레벨
	public short Skill1Lvup { get; set; } // 스킬 1 레벨업 시점
	public int Skill2Id { get; set; } // 
	public short Skill2Lv { get; set; } // 스킬 2 레벨
	public short Skill2Lvup { get; set; } // 스킬 2 레벨업 시점
	public int Skill3Id { get; set; } // 
	public short Skill3Lv { get; set; } // 스킬 3 레벨
	public short Skill3Lvup { get; set; } // 스킬 3 레벨업 시점
}



public class CardLevelBalanceDataRecord : BaseDataRecord
{
    public CardLevelBalanceData[] cardLevelBalanceData;
}
