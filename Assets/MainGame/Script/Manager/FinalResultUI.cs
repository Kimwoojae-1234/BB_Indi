using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class FinalResultUI : MonoBehaviour
    {
        public enum FinalRewardType
        {
            None,
            PenentEndPostSeason,    //페넌트레이스 종료 포스트시즌 시작
            PenentEndSeasonEnd,     //페넌트레이스 종료 시즌종료
            PostSeasonEnd,          //포스트 시즌 종료
            PostSeasonSeriesEnd,    //포스트시즌에서 각각 시리즈 종료
            RaceOnlyDayEnd,         //쟁탈 일일만 종료
            RaceDayAndWeekEnd,      //쟁탈 일일&주말 종료
            RaceOnlyWeekEnd,        //쟁탈 주말만 종료  
            LiveMatchWeekEnd,       //라이브매치 주간보상
            LiveMatchLeagueUpDown,  //라이브매치 리그승강
            WalkoffWeekEnd,         //9회투아웃 주간보상
        }
        //
        public UIFinalStanding finalStanding;
        public UILeagueFinalReward finalReward;
        public UILeagueFinalTitle leagueFinalTitle;
        public UILeagueUpDown leagueUpDown;
        public UIPostSeasonSchedule postSeasonSchedule;
        public UITotalReward totalReward;
        //
        public GameObject back;
        public GameObject backLight;
        public GameObject front;


        //보상 타입
        public FinalRewardType curType;

        private SeasonLobbyInfo lobbyInfo;
        private RacePlayLobbyInfo raceInfo;
        private LivePlayLobbyInfo liveInfo;
        private WalkoffPlayLobbyInfo walkoffInfo;

        private int teamPower;
        private int getGold;
        

        /// <summary>
        /// 시즌 보상 초기화
        /// </summary>
        /// <param name="info"></param>
        /// <param name="bCon"></param>
        public void InitSeasonReward(WebConnector.SeasonLobbyInfo info, bool bCon = false)
        {
            changeScene();

            if (bCon == true)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            lobbyInfo = info;
            SeasonAnnounceInfo annInfo = lobbyInfo.annInfo;

            //재화및 아이템 업데이트
            setBalanceAndItemUpdate(annInfo);

            //팀 전력 계산 
            // DISABLED_MGRS: teamPower = UI_HelperCalculator.CalTeamPower(Mgrs.userData.userHaveCard, Mgrs.userData.ALineUp).total;


            //모드 설정
            curType = FinalRewardType.None;
            
            if (annInfo.rsReport != null)
            {                
                if (annInfo.newInfo != null)
                {
                    //페넌트 시리즈 종료와 시즌 종료 동시에
                    curType = FinalRewardType.PenentEndSeasonEnd;
                    finalStanding.InitSeasonStanding();
                }
                else
                {
                    //페넌트 시리즈 종료 포스트 시즌 시작
                    curType = FinalRewardType.PenentEndPostSeason;
                    finalStanding.InitSeasonStanding();
                }
            }
            else if (annInfo.psReport != null)
            {
                if (annInfo.newInfo != null)
                {
                    //포스트 시즌 종료 시즌 완전 종료
                    curType = FinalRewardType.PostSeasonEnd;
                    postSeasonSchedule.InitPostSeason();
                }
                else
                {
                    //시리즈 종료
                    curType = FinalRewardType.PostSeasonSeriesEnd;
                    postSeasonSchedule.InitPostSeason();
                }
            }

            if (curType == FinalRewardType.None)
            {                
                deActive();
            }
        }


        /// <summary>
        /// 쟁탈 보상 초기화
        /// </summary>
        /// <param name="info"></param>
        public void InitRaceReward(RacePlayLobbyInfo info)
        {
            changeScene();

            raceInfo = info;
            RacePlayAnnounceInfo annInfo = raceInfo.annInfo;

            //팀 전력 계산 
            // DISABLED_MGRS: teamPower = UI_HelperCalculator.CalTeamPower(Mgrs.userData.userHaveCard, Mgrs.userData.ALineUp).total;
            //Debug.Log("=============================>> 쟁탈전 팀파워 계산 = " + teamPower);
            
            //재화 업데이트
            setRaceBalanceAndItemUpdate(annInfo);

            if (annInfo.weekRanking == 0)
            {
                //주간 보상 없는 경우
                if (annInfo.lgRanking == 0)
                {
                    //일일 보상도 없는 경우
                    Destroy(gameObject);
                }
                else
                {
                    //쟁탈 일일보상
                    curType = FinalRewardType.RaceOnlyDayEnd;
                    finalStanding.InitRaceStanding();
                }
            }
            else
            {
                //주간 보상 있는 경우
                if (annInfo.lgRanking == 0)
                {
                    //쟁탈 주간
                    curType = FinalRewardType.RaceOnlyWeekEnd;
                    totalReward.InitRaceWeekendReward();                    
                }
                else
                {
                    //쟁탈 일일&주간보상
                    curType = FinalRewardType.RaceDayAndWeekEnd;
                    finalStanding.InitRaceStanding();
                }
            }
            
        }

        /// <summary>
        /// 9회 투아웃 보상 초기화
        /// </summary>
        /// <param name="info"></param>
        public void InitWalkoffReward(WalkoffPlayLobbyInfo info)
        {
            changeScene();

            walkoffInfo = info;
            WalkoffPlayAnnounceInfo annInfo = info.annInfo;

            //팀 전력 계산 
            // DISABLED_MGRS: teamPower = UI_HelperCalculator.CalTeamPower(Mgrs.userData.userHaveCard, Mgrs.userData.ALineUp).total;
            
            //재화 업데이트
            setWalkoffBalanceAndItemUpdate(annInfo);

            curType = FinalRewardType.WalkoffWeekEnd;
            totalReward.InitWalkoffWeekendReward();
        }

        /// <summary>
        /// 라이브매치 주간보상 초기화
        /// </summary>
        /// <param name="info"></param>
        public void InitLiveMatchReward(LivePlayLobbyInfo info)
        {
            changeScene();

            liveInfo = info;
            LivePlayAnnounceInfo annInfo = info.annInfo;

            //팀 전력 계산 
            // DISABLED_MGRS: teamPower = UI_HelperCalculator.CalTeamPower(Mgrs.userData.userHaveCard, Mgrs.userData.ALineUp).total;
            
            //재화 업데이트
            setLivematchBalanceAndItemUpdate(annInfo);

            curType = FinalRewardType.LiveMatchWeekEnd;
            totalReward.InitLiveMatchWeekendReward();
        }


        public void changeScene()
        {
            TweenAlpha.Begin(front, 0.5f, 1);
            if (back.activeSelf == false)
            {
                Invoke("setBack", 0.5f);
            }
        }

        private void setBack()
        {
            back.SetActive(true);
        }

        public void fadeIn()
        {            
            TweenAlpha.Begin(front, 0.2f, 0);
        }


        /// <summary>
        /// 보상 프리팹을 비활성화 한다
        /// </summary>
        public void deActive()
        {
            backLight.SetActive(false);
            TweenAlpha.Begin(back.gameObject, 0.5f, 0);
                        
            Destroy(gameObject, 2.0f);
        }


        /// <summary>
        /// 시즌 로비 인포를 얻어옴
        /// </summary>
        /// <returns></returns>
        public SeasonLobbyInfo getLobbyInfo()
        {
            return lobbyInfo;
        }

        /// <summary>
        /// 쟁탈 로비 인포를 얻어옴
        /// </summary>
        /// <returns></returns>
        public RacePlayLobbyInfo getRaceInfo()
        {
            return raceInfo;
        }

        /// <summary>
        /// 라이브매치 로비 인포 얻어옴
        /// </summary>
        /// <returns></returns>
        public LivePlayLobbyInfo getLiveInfo()
        {
            return liveInfo;
        }

        /// <summary>
        /// 9회투아웃 로비 인포 얻어옴
        /// </summary>
        /// <returns></returns>
        public WalkoffPlayLobbyInfo getWalkoffInfo()
        {
            return walkoffInfo;
        }


        /// <summary>
        /// 팀 파워를 미리 저장한후 그 값을 얻어옴
        /// </summary>
        /// <returns></returns>
        public int getTeamPower()
        {
            return teamPower;
        }

        public int getRewardGold()
        {
            return getGold;
        }

        /// <summary>
        /// 시즌모드 내 팀 기록을 얻어옴
        /// </summary>
        /// <returns></returns>
        public SeasonTeamRecordInfo getMyTeamRecord()
        {
            List<SeasonTeamRecordInfo> teamRecords = lobbyInfo.annInfo.rsReport.teamRanking;
            for (int i = 0; i < teamRecords.Count; i++)
            {
                if (teamRecords[i].teamNo == BHConst.myTeamNo)
                {
                    return teamRecords[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 해당 팀의 팀 정보를 얻어옴
        /// </summary>
        /// <param name="teamNo"></param>
        /// <returns></returns>
        public SimpleTeamInfo getTeamInfo(int teamNo)
        {
            return lobbyInfo.teams[teamNo];
        }


        /// <summary>
        /// 시즌모드에서 총 재화와 아이템 업데이트를 함
        /// </summary>
        /// <param name="annInfo"></param>
        private void setBalanceAndItemUpdate(SeasonAnnounceInfo annInfo)
        {
            if (annInfo.balances != null)
            {
                //Debug.Log("=======================>> 총재화 업데이트");
                // DISABLED_MGRS: Mgrs.userData.SetUserBalances(annInfo.balances);
            }

            if (annInfo.items != null)
            {
                if (annInfo.items.Count > 0)
                {
                    //아이템 업데이트
                    //Debug.Log("=======================>> 총획들 아이템 업데이트");
                    // DISABLED_MGRS: Mgrs.userData.SetUserItem(annInfo.items, true);
                }
            }
        }

        /// <summary>
        /// 쟁탈모드에서 총 재화와 아이템 업데이트를 함
        /// </summary>
        /// <param name="annInfo"></param>
        private void setRaceBalanceAndItemUpdate(RacePlayAnnounceInfo annInfo)
        {
            if (annInfo.balances != null)
            {
                //Debug.Log("=======================>> 총재화 업데이트");
                // DISABLED_MGRS: Mgrs.userData.SetUserBalances(annInfo.balances);
            }

            if (annInfo.items != null)
            {
                if (annInfo.items.Count > 0)
                {
                    //아이템 업데이트
                    //Debug.Log("=======================>> 총획들 아이템 업데이트");
                    // DISABLED_MGRS: Mgrs.userData.SetUserItem(annInfo.items, true);
                }
            }
        }


        /// <summary>
        /// 9회 투아웃 주간 보상 재화 업데이트
        /// </summary>
        /// <param name="annInfo"></param>
        private void setWalkoffBalanceAndItemUpdate(WalkoffPlayAnnounceInfo annInfo)
        {
            if (annInfo.balances != null)
            {
                // DISABLED_MGRS: getGold = annInfo.balances[(int)DefineEnum.ECurrency.Gold] - Mgrs.userData.GetUserHaveGold();
                //Debug.Log("=======================>> 총재화 업데이트");
                // DISABLED_MGRS: Mgrs.userData.SetUserBalances(annInfo.balances);
            }
        }

        /// <summary>
        /// 라이브 매치 주간 보상 재화 및 아이템 업데이트
        /// </summary>
        /// <param name="annInfo"></param>
        private void setLivematchBalanceAndItemUpdate(LivePlayAnnounceInfo annInfo)
        {
            if (annInfo.balances != null)
            {
                //Debug.Log("=======================>> 총재화 업데이트");
                // DISABLED_MGRS: Mgrs.userData.SetUserBalances(annInfo.balances);
            }

            if (annInfo.items != null)
            {
                if (annInfo.items.Count > 0)
                {
                    //아이템 업데이트
                    //Debug.Log("=======================>> 총획들 아이템 업데이트");
                    // DISABLED_MGRS: Mgrs.userData.SetUserItem(annInfo.items, true);
                }
            }
        }
    }
}
