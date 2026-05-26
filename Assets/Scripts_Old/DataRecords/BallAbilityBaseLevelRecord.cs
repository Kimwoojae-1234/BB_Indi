using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallAbilityLevelData
{
    public int Idx { get; set; }
	public short Level { get; set; }
	public short Effect1Id { get; set; }
	public int Effect1Value { get; set; }
	public short Effect2Id { get; set; }
	public int Effect2Value { get; set; }
	public short Effect3Id { get; set; }
	public int Effect3Value { get; set; }
}


public class BallAbilityLevelDataRecord : BaseDataRecord
{
	public BallAbilityLevelData[] BallAbilityLevelData = null;

	public override bool Initialize()
	{
		return base.Initialize();
	}

	public override bool Uninitialize()
	{
		return base.Uninitialize();
	}
}
