using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어 정보 관련
/// </summary>

public partial class KOBGameData : BackendData.Base.GameData
{
    public int RttsStart()
    {
        int errorCode = 0;

        RttsInfo.StartLeague();
        isRttsChange = true;
        IsChangedData = true;

        return errorCode;
    }


    public TResultRttsLeagueUpgrade RttsLeagueUpgrade()
    {
        int errorCode = 0;

        int curLeague = RttsInfo.League;
        int nextLeague = curLeague;
        bool isNextLeagueExist = false;         //다음 리그 있는지        

        Dictionary<int, List<RewardData>> rewardData = new Dictionary<int, List<RewardData>>();

        //리그 순위
        int[] stading = KOBManager.Rtts.GetLeagueStanding();     //내등수와 승,무,패
        if (stading[0] == 1 ||  //1위 인 경우
            curLeague == 0)     //또는 현재 리그 튜토리얼 리그인 경우 무조건 다음리그로 이어지게
        {
            RttsInfo info = KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(curLeague + 1);
            if(info?.Open == true) //다음리그 데이터가 있는 경우
            {
                isNextLeagueExist = true;
                nextLeague = curLeague + 1; //다음 리그로 진출할 수 있도록 설정
            }
            int resultStep = (int)UI_RTTSResult.ResultStep.LeagueStading; //현재 보상 스텝
            int reward_index = KOBManager.Backend.Chart.RttsResultReward.GetRewardIndex(curLeague, resultStep); //해당 보상 인덱스
            rewardData.Add(resultStep, KOBManager.Backend.Chart.RewardData.GetRewards(reward_index)); //리스트에 담는다
        }


        //개인 순위
        List<int[]> leaders = KOBManager.Rtts.GetLeagueLeaders(); //1등의 인덱스와 개수
        for(int i = 0; i < leaders.Count; i++)
        {
            int [] value = leaders[i];
            bool isMyTeamate = ((value[0] / KOBConstant.PLAYER_RECORD_UNIT) == 0); //내선수 여부 판별
            if(isMyTeamate == true) //내선수가 1등한 경우
            {
                int resultStep = (int)(UI_RTTSResult.ResultStep.HrLeader) + i; //현재 보상 스텝
                int reward_index = KOBManager.Backend.Chart.RttsResultReward.GetRewardIndex(curLeague, resultStep); //해당 보상 인덱스
                rewardData.Add(resultStep, KOBManager.Backend.Chart.RewardData.GetRewards(reward_index)); //리스트에 담는다
            }
        }

        //Rtts 리그 업데이트
        isRttsChange = RttsInfo.UpdateLeague(isNextLeagueExist); 

        //여기다가 보상 업데이트
        Dictionary<int, List<KOBRewardInfo>> resultRewardData = RttsResultReward(rewardData);
        

        IsChangedData = true;

        TResultRttsLeagueUpgrade req = new TResultRttsLeagueUpgrade()
        {
            ErrorCode = errorCode,
            NextLeague = nextLeague,
            FinalStanding = stading,
            HRLeader = leaders[0],
            AvgLeader = leaders[1],
            RbiLeader = leaders[2],
            HitLeader = leaders[3],
            OpsLeader = leaders[4],
            RewardData = resultRewardData
        };
        return req;
    }


    private Dictionary<int, List<KOBRewardInfo>> RttsResultReward(Dictionary<int, List<RewardData>> rewardData)
    {
        Dictionary<int, List<KOBRewardInfo>> resultRewardData = new Dictionary<int, List<KOBRewardInfo>>();
        if (rewardData != null)
        {
            foreach (KeyValuePair<int, List<RewardData>> pair in rewardData)
            {
                int key = pair.Key;
                List<RewardData> rewardDatas = pair.Value;

                List<KOBRewardInfo> listItem = new List<KOBRewardInfo>();
                for (int i = 0; i < rewardDatas.Count; i++)
                {
                    KOBRewardInfo reward = new KOBRewardInfo(rewardDatas[i]);   
                    KOBRewardType type = reward.GetRewardType();
                    bool unlock = false;

                    if (type == KOBRewardType.Currency)
                    {
                        UpdateReward(reward);
                        //재화는 언락 이슈 없음
                    }
                    else if (type == KOBRewardType.Card)
                    {
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
                    listItem.Add(reward);
                }

                resultRewardData.Add(key, listItem);    
            }   
            IsChangedData = true;
            return resultRewardData;
        }
        return null;
    }


    public int RttsBattleEnd(TRequestBattleEnd req)
    {
        int errorCode = 0;

        int AddTrophy = KOBPointCalUtil.CaculateAddTrophy(req.League, req.Result); //req로부터 트로피 계산
        int AddXP = KOBPointCalUtil.CaculateAddXP(req);     //req로부터 XP 계산
        int AddFame = KOBPointCalUtil.CaculateAddFame(req.League, req.myRecord);    //req로부터 Fame 계산
        int ballerIDX = req.ballerIdx;

        isRttsChange = RttsInfo.BattleEnd();     //rtts계산
        isGrowthChange = GrowthInfo.AddTrophy(AddTrophy); //트로피계산 -> 추가
        isGrowthChange = GrowthInfo.AddXP(AddXP); //XP계산 -> 추가
        isGrowthChange = GrowthInfo.AddRecord(ballerIDX, req.League, req.myRecord); //업적등을 계산하기 위한
        isPlayerChange = PlayerInfo.AddFame(ballerIDX, AddFame);
        
        //업적   -> 미션작업 후     
        IsChangedData = true;

        return errorCode;
    }

    public List<KOBRewardInfo> RttsBattleReward(TRequestBattleEnd req)
    {        
        List<KOBRewardInfo> kobRewardList = KOBManager.Rtts.GetRttsRewardInfo(req.Result);
        if(kobRewardList != null)
        { 
            for (int i = 0; i < kobRewardList.Count; i++)
            {
                KOBRewardInfo reward = kobRewardList[i];
                KOBRewardType type = reward.GetRewardType();
                bool unlock = false;

                if (type == KOBRewardType.Currency)
                {
                    UpdateReward(reward);
                    //재화는 언락 이슈 없음
                }
                else if (type == KOBRewardType.Card)
                {
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
            }
            IsChangedData = true;
            return kobRewardList;
        }

        return null;
    }


}
