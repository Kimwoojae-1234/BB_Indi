using System;
using System.Collections.Generic;

/// <summary>
/// 성장 정보 관련
/// </summary>

public partial class KOBGameData : BackendData.Base.GameData
{
    public int TierUpgrade()
    {
        int curMaxTrophy = GrowthInfo.MaxTrophy;
        int curTier = KOBManager.Backend.Chart.TrophyRoadData.GetCurrentTier(curMaxTrophy);
        isGrowthChange = GrowthInfo.SetNewTier(curTier);
        if(isGrowthChange) IsChangedData = true;
        return 0;
    }


    public TResultTrophyRoadReward TrohyRewaredGet(TRequestTrophyRoadReward body)
    {
        TResultTrophyRoadReward response = new TResultTrophyRoadReward();

        int idx = body.TrophyRoadIdx;
        if (GrowthInfo.TropyGetList.Contains(idx) == true)
        {
            response.ErrorCode = (int)KOBErrorCode.Duplicate_Trophy_Rewards;
            response.isSuccess = false;
        }
        else
        {
            //트로피 슬롯 채우고
            GrowthInfo.AddTrophyReward(idx);
            isGrowthChange = true;

            int _pIndex = body.pIndex;

            //보상 처리
            for (int i = 0; i < body.RewardList.Count; i++)
            {
                RewardData item = body.RewardList[i];
                KOBRewardInfo reward = new KOBRewardInfo(item);

                KOBRewardType type = reward.GetRewardType();
                bool unlock = false;

                if (type == KOBRewardType.Currency)
                {
                    UpdateReward(reward);
                    //재화는 언락 이슈 없음
                }
                else if (type == KOBRewardType.Card)
                {
                    if (_pIndex > 0)
                    {
                        reward.pindex = _pIndex;
                    }
                    else
                    {
                        //해당 기호에 따른 pIndex세팅
                    }
                    reward.pindex = KOBRewardUtil.GetRandomPindex(reward);    //음수인 경우 랜덤 선택
                    unlock = AddBaller(reward);//.pindex, reward.amount);
                }
                else if (type == KOBRewardType.Inventory)
                {
                    //ItemInfo
                    //아이템도 unlock이슈 있음 -> 연출보다는 알람쪽임
                }
                else
                {

                }
                reward.unlock = unlock;
                response.RewardList.Add(reward);
            }
        }

        return response;
    }



    public TResultBallerTrophyRoadReward BallerTrohyRewaredGet(TRequestBallerTrophyRoadReward body)
    {
        TResultBallerTrophyRoadReward response = new TResultBallerTrophyRoadReward();

        int baller_idx = body.CharIdx;
        int idx = body.TrophyRoadIdx;

        if (GrowthInfo.BallerTropyGetList.ContainsKey(baller_idx) == false)
        {
            response.ErrorCode = (int)KOBErrorCode.Not_Have_Baller;
            response.isSuccess = false;
        }
        else
        {
            if (GrowthInfo.BallerTropyGetList[baller_idx].Contains(idx) == true)
            {
                response.ErrorCode = (int)KOBErrorCode.Duplicate_Trophy_Rewards;
                response.isSuccess = false;
            }
            else
            {
                //트로피 슬롯 채우고
                isGrowthChange = GrowthInfo.BallerTrophyGet(baller_idx, idx);
                int _pIndex = body.pIndex;
                if(_pIndex == KOBConstant.BALLPER_YOUPLAY) _pIndex = KOBManager.MyInfo.UISelectedBaller; // UI_Baller관련 됨 (명성트로피로드 보상이기때문)

                //보상 처리
                for (int i = 0; i < body.RewardList.Count; i++)
                {
                    RewardData item = body.RewardList[i];
                    KOBRewardInfo reward = new KOBRewardInfo(item);

                    KOBRewardType type = reward.GetRewardType();
                    bool unlock = false;

                    if (type == KOBRewardType.Currency)
                    {
                        UpdateReward(reward);
                        //재화는 언락 이슈 없음
                    }
                    else if (type == KOBRewardType.Card)
                    {
                        if (_pIndex > 0)
                        {
                            reward.pindex = _pIndex;
                        }
                        reward.pindex = KOBRewardUtil.GetRandomPindex(reward);    //음수인 경우 랜덤 선택
                        unlock = AddBaller(reward);//.pindex, reward.amount);
                    }
                    else if (type == KOBRewardType.Inventory)
                    {
                        //ItemInfo
                        //아이템도 unlock이슈 있음 -> 연출보다는 알람쪽임
                    }
                    else
                    {

                    }
                    reward.unlock = unlock;
                    response.RewardList.Add(reward);
                }
            }
        }

        return response;
    }
}
