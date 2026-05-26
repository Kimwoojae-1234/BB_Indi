using System.Collections.Generic;

/// <summary>
/// 보상의 타입
/// </summary>
public enum KOBReward
{
    None = 0,
    Gold = 1,
    Gem,            //이게 보상으로 쓰이진 않음 (상점구매시)
    Gem_Free,       //젬보상은 무조건 이거임
    Energy,
    Card_Common = 100,
    Card_Rare = 101,
    Card_Epic = 102,
    Card_Legend = 103,
    Card_Black = 104,
    Box_Common = 200,
    Box_Rare = 201,
    Box_Epic = 202,
    Box_Legend = 203,
    Box_Black = 204,
    Gear = 300,
    Bat = 400,
    Skill = 500 //아마 안쓰일듯 -> 혹시 모르니 가지고나 있자!!
}

/// <summary>
/// 보상의 출처
/// </summary>
public enum KOBRewardFrom
{
    None = 0,
    Result = 1,         //게임 결과시 - XP, 트로피, 개인트로피
    TrophyRoad,         //트로피로드  - Gold, 카드, P카드, 박스
    BallerTrophyRoad,   //개인트로피로드 - Gold, 카드, 스킬, 기어
    SeasonTrophyRoad,   //시즌트로피로드 - 상자, 카드(언락)
    Pass,               //배틀패스 - XP, GOLD, 배트, 상자
    Mission,            //미션 - XP
    DynamicMission,     //실시간미션 - 상자
    Post,
}


/// <summary>
///DB에 저장되는 타입으로 구분
/// </summary>
public enum KOBRewardType
{
    None = 0,
    Currency,   //CurrencyInfo
    Card,       //PlayerInfo
    Inventory,  //ItemInfo
    Box
}



public class KOBRewardInfo
{
    public KOBReward reward;
    public KOBRewardFrom rewardFrom;
    public int pindex;
    public int amount;
    public bool unlock;
    public int refID;

    public KOBRewardInfo()
    {
        
    }

    public KOBRewardInfo(RewardData data)
    {
        reward = data.reward;
        //rewardFrom;
        pindex = data.pindex;
        amount = UnityEngine.Random.Range(data.min, data.max + 1);
        //refID;
    }

    public KOBRewardInfo(KOBReward _reward, int _pindex, int _amount)
    {
        reward = _reward;
        pindex = _pindex;
        amount = _amount;
    }

    public KOBRewardInfo(int _refID, int _pindex, KOBRewardFrom _from = KOBRewardFrom.None)
    {
        List<RewardData> list = KOBManager.Backend.Chart.RewardData.GetRewards(_refID);
        refID = _refID;
        if (list.Count > 0)
        {
            rewardFrom = _from;
            if (list.Count == 1) //단품보상
            {
                //단품 세팅
                reward = list[0].reward;                
                pindex = _pindex;
                amount = UnityEngine.Random.Range(list[0].min, list[0].max + 1);
            }
            else //박스보상 세팅
            {
                reward = KOBRewardUtil.SetBoxType(list);
                amount = 1;
            }
        }
        else
        {
            //보상이 없음
            reward = KOBReward.None;
        }

    }


    /// <summary>
    /// 관리자 우편으로부터 초기화
    /// </summary>
    /// <param name="boxInfo"></param>
    public KOBRewardInfo(AdminPostReward postInfo)
    {
        reward = postInfo.reward;
        rewardFrom = KOBRewardFrom.Post;
        pindex = postInfo.pindex;
        amount = postInfo.amount;
    }

    public KOBRewardType GetRewardType()
    {
        if(reward == KOBReward.Gold ||
            reward == KOBReward.Gem ||
            reward == KOBReward.Gem_Free ||
            reward == KOBReward.Energy)
        {
            return KOBRewardType.Currency;
        }
        else if (reward == KOBReward.Card_Common ||
                 reward == KOBReward.Card_Rare ||
                 reward == KOBReward.Card_Epic ||
                 reward == KOBReward.Card_Legend ||
                 reward == KOBReward.Card_Black)

        {
            return KOBRewardType.Card;
        }
        else if (reward == KOBReward.Box_Common ||
                 reward == KOBReward.Box_Rare ||
                 reward == KOBReward.Box_Epic ||
                 reward == KOBReward.Box_Legend ||
                 reward == KOBReward.Box_Black)

        {
            return KOBRewardType.Box;
        }
        else if (reward == KOBReward.Gear ||
                 reward == KOBReward.Bat)
        {
            return KOBRewardType.Inventory;
        }
        else
        {
            return KOBRewardType.None;
        }
    }
}

public class KOBBoxInfo
{
    public KOBReward reward;
    public int pIndex1; //카드인 경우 조건 : -1(있는것중에 랜덤), -2(모든카드중 랜덤), -3(없는것중에 랜덤)
    public int pIndex2; //카드인 경우 희귀도 : -1(희귀도 구분 안함)
}