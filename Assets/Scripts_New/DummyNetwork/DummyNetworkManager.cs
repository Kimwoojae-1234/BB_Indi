using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BackEnd;
using static BackEnd.Backend;
using BackEnd.Tcp;

public class DummyNetworkManager : MonoBehaviour
{
    public void SendPacket(TRequestBase packetBody, BackendManager.AfterUpdateFunc onResult)
    {
        KOBManager.FrontUI.OpenPopup<FrontUI_NetworkLoading>();
        TResponseBase response = processingPacket(packetBody);        
        KOBManager.Backend.UpdateAllGameData(onResult, response);
    }


    /// <summary>
    /// 더미 서버 처리
    /// </summary>
    /// <param name="packetBody"></param>
    private TResponseBase processingPacket(TRequestBase packetBody)
    {
        TResponseBase response = null;
        switch (packetBody.getURL())
        {           
            case DummyURL.TutorialStep:
                response = RequestTutorialStep(packetBody);
                break;
            case DummyURL.Reward:
                response = RequestRewardInfo(packetBody);
                break;
            case DummyURL.RewardList:
                response = RequestRewardListInfo(packetBody);
                break;
            case DummyURL.TrophyRoadReward:
                response = RequestTrophyRoadReward(packetBody);
                break;
            case DummyURL.BallerTrophyRoadReward:
                response = RequestBallerTrophyRoadReward(packetBody);
                break;

            case DummyURL.UpgradeCard:
                response = RequestUpgradeCard(packetBody);
                break;
            case DummyURL.SelectBaller:
                response = RequestSelectBaller(packetBody);
                break;
            case DummyURL.ChangeDeck:
                response = RequestChangeDeck(packetBody);
                break;



            case DummyURL.RttsStart:
                response = RequestRttsStart(packetBody); 
                break;
            case DummyURL.RttsLeagueUpgrade:
                response = RequestRttsLeagueUpgrade(packetBody);
                break;


            case DummyURL.RttsBattleEnd:
                response = RequestRttsBattleEnd(packetBody);
                break;


            case DummyURL.TierUpgrade:
                response = RequestTierUpgrade(packetBody);
                break;
            case DummyURL.BallerFameUpgrade:
                response = RequestBallerUpgrade(packetBody);
                break;

        }

        return response;
    }



    private TResponseBase RequestTutorialStep(TRequestBase packetBody)
    {
        TRequestTutoStep body = (TRequestTutoStep)packetBody;
        KOBManager.Backend.GameData.KOBGameData.TutorailCompete(body.Step);
        TResultTutoStep response = new TResultTutoStep();
        return response;
    }




    private TResponseBase RequestRewardInfo(TRequestBase packetBody)
    {
        TRequestRewardInfo body = (TRequestRewardInfo)packetBody;
        bool unlock = false;

        KOBRewardType type = body.Reward.GetRewardType();

        if(type == KOBRewardType.Currency)
        { 
            KOBManager.Backend.GameData.KOBGameData.UpdateReward(body.Reward);
            //재화는 언락 이슈 없음
        }
        else if (type == KOBRewardType.Card)
        {
            body.Reward.pindex = KOBRewardUtil.GetRandomPindex(body.Reward);    //음수인 경우 랜덤 선택
            unlock = KOBManager.Backend.GameData.KOBGameData.AddBaller(body.Reward);//.pindex, body.Reward.amount, body.Reward.reward);
        }
        else if (type == KOBRewardType.Inventory)
        {
            //ItemInfo
            //아이템도 unlock이슈 있음 -> 연출보다는 알람쪽임
        }
        else
        {
            
        }


        TResultRewardInfo response = new TResultRewardInfo();
        response.Reward = body.Reward;
        response.Reward.unlock = unlock;  //언락 여부   
        return response;
    }


    private TResponseBase RequestRewardListInfo(TRequestBase packetBody)
    {
        TRequestRewardListInfo body = (TRequestRewardListInfo)packetBody;

        TResultRewardListInfo response = new TResultRewardListInfo();
        
        for (int i = 0; i < body.RewardList.Count; i++)
        {
            RewardData item = body.RewardList[i];
            KOBRewardInfo reward = new KOBRewardInfo(item);

            KOBRewardType type = reward.GetRewardType();
            bool unlock = false;

            if (type == KOBRewardType.Currency)
            {
                KOBManager.Backend.GameData.KOBGameData.UpdateReward(reward);
                //재화는 언락 이슈 없음
            }
            else if (type == KOBRewardType.Card)
            {
                reward.pindex = KOBRewardUtil.GetRandomPindex(reward);    //음수인 경우 랜덤 선택
                unlock = KOBManager.Backend.GameData.KOBGameData.AddBaller(reward);//.pindex, reward.amount, reward.reward);
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
       
        return response;
    }


    private TResponseBase RequestTrophyRoadReward(TRequestBase packetBody)
    {
        TRequestTrophyRoadReward body = (TRequestTrophyRoadReward)packetBody;

        TResultTrophyRoadReward response = KOBManager.Backend.GameData.KOBGameData.TrohyRewaredGet(body);
        

        return response;
    }



    private TResponseBase RequestBallerTrophyRoadReward(TRequestBase packetBody)
    {
        TRequestBallerTrophyRoadReward body = (TRequestBallerTrophyRoadReward)packetBody;

        TResultBallerTrophyRoadReward response = KOBManager.Backend.GameData.KOBGameData.BallerTrohyRewaredGet(body);


        return response;
    }




    private TResponseBase RequestUpgradeCard(TRequestBase packetBody)
    {
        TRequestUpgradeCard body = (TRequestUpgradeCard)packetBody;
        
        int ErrorCode = KOBManager.Backend.GameData.KOBGameData.UpgradeCard(body.CardIdx);
        TResultUpgradeCard response = new TResultUpgradeCard();
        response.CardIdx = body.CardIdx;
        response.ErrorCode = ErrorCode;
        response.isSuccess = (ErrorCode == 0);
        return response;
    }

    private TResponseBase RequestSelectBaller(TRequestBase packetBody)
    {
        TRequestSelectBaller body = (TRequestSelectBaller)packetBody;

        int ErrorCode = KOBManager.Backend.GameData.KOBGameData.SetBaller(body.CardIdx);
        TResultSelectBaller response = new TResultSelectBaller();
        response.CardIdx = body.CardIdx;
        response.ErrorCode = ErrorCode;
        response.isSuccess = (ErrorCode == 0);
        return response;
    }
    //

    private TResponseBase RequestChangeDeck(TRequestBase packetBody)
    {
        TRequestChangeDeck body = (TRequestChangeDeck)packetBody;

        int ErrorCode = KOBManager.Backend.GameData.KOBGameData.ChangeDeck(body);
        TResultChangeDeck response = new TResultChangeDeck();
        response.ErrorCode = ErrorCode;
        response.isSuccess = (ErrorCode == 0);
        return response;
    }



    private TResponseBase RequestRttsStart(TRequestBase packetBody)
    {
        TRequestRttsStart body = (TRequestRttsStart)packetBody;

        int ErrorCode = KOBManager.Backend.GameData.KOBGameData.RttsStart();
        TResultRttsStart response = new TResultRttsStart();
        response.League = body.League;
        response.ErrorCode = ErrorCode;
        response.isSuccess = (ErrorCode == 0);
        return response;
    }




    private TResponseBase RequestRttsLeagueUpgrade(TRequestBase packetBody)
    {
        TRequestRttsLeagueUpgrade body = (TRequestRttsLeagueUpgrade)packetBody;

        //int ErrorCode = KOBManager.Backend.GameData.KOBGameData.RttsLeagueUpgrade();
        TResultRttsLeagueUpgrade response = KOBManager.Backend.GameData.KOBGameData.RttsLeagueUpgrade();        
        return response;
    }


    private TResponseBase RequestRttsBattleEnd(TRequestBase packetBody)
    {
        TRequestBattleEnd body = (TRequestBattleEnd)packetBody;
        int ErrorCode = KOBManager.Backend.GameData.KOBGameData.RttsBattleEnd(body);
        //보상계산 -> 추가작업할것 -> 작업되면 response에 넣을것!!
        List<KOBRewardInfo> rewardList = KOBManager.Backend.GameData.KOBGameData.RttsBattleReward(body);
        TResultBattleEnd response = new TResultBattleEnd()
        {
            RewardList = rewardList
        };
        response.ErrorCode = ErrorCode;
        response.isSuccess = (ErrorCode == 0);
        return response;
    }




    private TResponseBase RequestTierUpgrade(TRequestBase packetBody)
    {
        TRequestTierUpgrade body = (TRequestTierUpgrade)packetBody;
        int ErrorCode = KOBManager.Backend.GameData.KOBGameData.TierUpgrade();
        
        TResultTierUpgrade response = new TResultTierUpgrade();
        response.ErrorCode = ErrorCode;
        response.isSuccess = (ErrorCode == 0);
        return response;
    }


    private TResponseBase RequestBallerUpgrade(TRequestBase packetBody)
    {
        TRequestBallerFameUpgrade body = (TRequestBallerFameUpgrade)packetBody;
        int ErrorCode = KOBManager.Backend.GameData.KOBGameData.BallerFameUpgrade(body.baller_idx);

        TResultBallerFameUpgrade response = new TResultBallerFameUpgrade();
        response.ErrorCode = ErrorCode;
        response.isSuccess = (ErrorCode == 0);
        response.baller_idx = body.baller_idx;
        return response;
    }
}
