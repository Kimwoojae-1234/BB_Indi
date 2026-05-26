using BackEnd;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum DummyURL
{
    None,
    TutorialStep,
    Reward,
    RewardList,
    TrophyRoadReward,
    BallerTrophyRoadReward,

    UpgradeCard,
    SelectBaller,
    ChangeDeck,


    RttsStart,
    RttsLeagueUpgrade,
    RttsBattleStart,
    RttsBattleEnd,


    TierUpgrade,
    BallerFameUpgrade,



    RandomTest,
    RandomTest2,
}


public abstract class TRequestBase
{
    public abstract DummyURL getURL();
}

public abstract class TResponseBase
{
    public bool isSuccess = true;
    public int ErrorCode = 0;
}




public class TRequestTutoStep : TRequestBase
{
    public override DummyURL getURL() => DummyURL.TutorialStep;
    public TutorialManager.TutoStep Step { get; set; }

}

public class TResultTutoStep : TResponseBase
{

}



public class TRequestRewardInfo : TRequestBase
{
    public override DummyURL getURL() => DummyURL.Reward;
    public KOBRewardInfo Reward { get; set; }
}

public class TResultRewardInfo : TResponseBase
{
    public KOBRewardInfo Reward = new KOBRewardInfo();
}



public class TRequestRewardListInfo : TRequestBase
{
    public override DummyURL getURL() => DummyURL.RewardList;
    public List<RewardData> RewardList = new List<RewardData>();
}

public class TResultRewardListInfo : TResponseBase
{
    public List<KOBRewardInfo> RewardList = new List<KOBRewardInfo>();
}

/// <summary>
/// 트로피 로드에서 보상 요청
/// </summary>
public class TRequestTrophyRoadReward : TRequestBase
{
    public override DummyURL getURL() => DummyURL.TrophyRoadReward;
    public List<RewardData> RewardList = new List<RewardData>();
    public int pIndex = -1;  //단일 상품인 경우... (-1인경우는 체크 안함)
    public int TrophyRoadIdx { get; set; }
}

public class TResultTrophyRoadReward : TResponseBase
{
    public List<KOBRewardInfo> RewardList = new List<KOBRewardInfo>();
}


public class TRequestBallerTrophyRoadReward : TRequestBase
{
    public override DummyURL getURL() => DummyURL.BallerTrophyRoadReward;
    public List<RewardData> RewardList = new List<RewardData>();
    public int pIndex = -1;  //단일 상품인 경우... (-1인경우는 체크 안함)
    public int CharIdx { get; set; }
    public int TrophyRoadIdx { get; set; }

}

public class TResultBallerTrophyRoadReward : TResponseBase
{
    public List<KOBRewardInfo> RewardList = new List<KOBRewardInfo>();
}



public class TRequestUpgradeCard : TRequestBase
{
    public override DummyURL getURL() => DummyURL.UpgradeCard;
    public int CardIdx{ get; set; }
}

public class TResultUpgradeCard : TResponseBase
{
    public int CardIdx { get; set; }
}








public class TRequestSelectBaller : TRequestBase
{
    public override DummyURL getURL() => DummyURL.SelectBaller;
    public int CardIdx { get; set; }
}

public class TResultSelectBaller : TResponseBase
{
    public int CardIdx { get; set; }
}




public class TRequestChangeDeck : TRequestBase
{
    public override DummyURL getURL() => DummyURL.ChangeDeck;
    public int DeckNo { get; set; }
    public Dictionary<int, KOBLineupInfo> NewDeck  { get; set; }
    public int SelectIdx { get; set; } = -1;
}

public class TResultChangeDeck : TResponseBase
{
    
}




public class TRequestRttsStart : TRequestBase
{
    public override DummyURL getURL() => DummyURL.RttsStart;
    public int League { get; set; }
}

public class TResultRttsStart : TResponseBase
{
    public int League { get; set; }
}




public class TRequestRttsLeagueUpgrade : TRequestBase
{
    public override DummyURL getURL() => DummyURL.RttsLeagueUpgrade;
    public int CurrentLeague { get; set; }
    

}

public class TResultRttsLeagueUpgrade : TResponseBase
{
    public int CurrentLeague { get; set; }
    public int NextLeague { get; set; }
    public int [] FinalStanding { get; set; }
    public int [] HRLeader { get; set; }
    public int[] AvgLeader { get; set; }
    public int[] RbiLeader { get; set; }
    public int[] HitLeader { get; set; }
    public int[] OpsLeader { get; set; }
    public Dictionary<int, List<KOBRewardInfo>> RewardData { get; set; }
}





public class TRequestBattleEnd : TRequestBase
{
    public override DummyURL getURL() => DummyURL.RttsBattleEnd;    
    public int League { get; set; }
    public int Result { get; set; } //0:승리 1: 무승부 2:패배
    public int[] myRecord { get; set; }
    public int ballerIdx { get; set; }
    public int GetXP { get; set; }
}

public class TResultBattleEnd : TResponseBase
{
    public List<KOBRewardInfo> RewardList = new List<KOBRewardInfo>();
}







public class TRequestTierUpgrade : TRequestBase
{
    public override DummyURL getURL() => DummyURL.TierUpgrade;
    public int Tier { get; set; }   
    
}

public class TResultTierUpgrade : TResponseBase
{
    
}




public class TRequestBallerFameUpgrade : TRequestBase
{
    public override DummyURL getURL() => DummyURL.BallerFameUpgrade;
    public int baller_idx { get; set; }

}

public class TResultBallerFameUpgrade : TResponseBase
{
    public int baller_idx { get; set; }
}




public class TRequestRandomTest : TRequestBase
{
    public override DummyURL getURL() => DummyURL.RandomTest;
}

public class TResultRandomTest : TResponseBase
{
    
}

public class TRequestRandomTest2 : TRequestBase
{
    public override DummyURL getURL() => DummyURL.RandomTest2;
}

public class TResultRandomTest2 : TResponseBase
{

}