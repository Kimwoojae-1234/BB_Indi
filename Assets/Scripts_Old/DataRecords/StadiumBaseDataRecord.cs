using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StadiumBaseData
{
	public int Idx { get; set; } // 스타디움 인덱스
	public string NameId { get; set; } // 스타디움 이름
	public string DescId { get; set; } // 스타디움 설명
	public short UnlockTrophy { get; set; } // 스타디움 잠금해제트로피
	public int EntryFee { get; set; } // 스타디움 입장료
	public short MaxTrophy { get; set; } // 스타디움최대 획득 트로피
	public short WinGainTrophy { get; set; } // 승리시 획득 트로피
	public short LoseLostTrophy { get; set; } // 패배시 상실 트로피
	public int WinGainCoin { get; set; } // 승리시 획득 코인
	public short WinGainClanToken { get; set; } // 승리시 획득 클랜 토큰
	public byte StartInning { get; set; } // 시작 이닝
	public byte OutCount { get; set; } // 아웃카운트
	public byte FirstBase { get; set; } // 1루상태
	public byte SecondBase { get; set; } // 2루상태
	public byte ThirdBase { get; set; } // 3루상태
	public short FieldCentersize { get; set; } // 
	public short FieldLeftsize { get; set; } // 
	public short FieldRightsize { get; set; } // 
	public short FieldFenceheight { get; set; } // 
	public short WindConditionmin { get; set; } // 
	public short WindConditionmax { get; set; } // 
	public short WindIcondirection { get; set; } // 
	public string WindMaindirection { get; set; } // 
	public string WindSidedirection { get; set; } // 
	public short AirDensity { get; set; } // 
	public short FoulTerritory { get; set; } // 
	public string PortraitTag { get; set; } // 초상화 태그
	public short FriendlyType { get; set; } // 친화도
	public int UnlockedStadiumIdx { get; set; } // 잠금해제 스타디움인덱스
	public string ColorType { get; set; } // 스타디움 팝업색
	public int UnlockRewardRefid { get; set; }
	public int RewardValue { get; set; }
    public short IsActiveSelectInning { get; set; }
    public int BotGroupId { get; set; }
    public int StartBattingIdx { get; set; }
    public int EndInning { get; set; }
	public int PveRandomrate { get; set; }
}

public class StadiumBaseDataRecord : BaseDataRecord
{
	public StadiumBaseData[] StadiumBaseData;
}