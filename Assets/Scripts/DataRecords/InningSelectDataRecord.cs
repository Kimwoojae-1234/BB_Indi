using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InningSelectData
{
	public int StadiumIdx { get; set; } // 스타디움 인덱스
	public byte InningIdx { get; set; } // 시작 이닝
	public byte OutCount { get; set; } // 아웃카운트
	public byte FirstBase { get; set; } // 1루상태
	public byte SecondBase { get; set; } // 2루상태
	public byte ThirdBase { get; set; } // 3루상태
}
public class InningSelectDataRecord : BaseDataRecord
{
    public InningSelectData[] InningSelectData;
}
