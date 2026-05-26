//#define HITBALLTYPE_RECORD
//#define CHANGE_BATTER_TEST
//#define CHANGE_RUNNER_TEST
//#define _TEST_NETWORK //시뮬레이션 엔진을 이용하여 급 네트워크 테스트 할때 필요

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebConnector;
using System.IO;

namespace BaseBall.BallPlay
{
    public class SimulManager : MonoBehaviour
    {

        private static SimulManager Instance_;
        private SimulMain sMain;

        private SeasonGameInfo gameInfo; //시즌인포
        //private ArenaGameInfo arenaInfo;
        //private CampaignGameInfo campainInfo;
        //private ClassicGameInfo deathInfo;

        
        //경기 중계 정보
        private string strGameSummury2, strGameSummury;

        //
        void Awake()
        {
            sMain = new SimulMain();
            Instance_ = this;
        }

        
        void Start()
        {
            
            strGameSummury = "";
            strGameSummury2 = "";
            sMain.initPlayerData(SimulPlayerManager.GetPlayer());
        }

        //인스턴스 파괴자
        void OnDestroy()
        {
            sMain = null;
            Instance_ = null;
        }

        /////////////////////////////////////////////////////////////////////////
        //외부 호출용 static method
        /////////////////////////////////////////////////////////////////////////    
        /*
        public static SeasonGameInfo GetSeasonInfo()
        {
            return Instance_.gameInfo;
        }*/

        /*
        public static ArenaGameInfo GetArenaInfo()
        {
            return Instance_.arenaInfo;
        }

        public static CampaignGameInfo GetCampaignInfo()
        {
            return Instance_.campainInfo;
        }

        public static ClassicGameInfo GetDeathMatchInfo()
        {
            return Instance_.deathInfo;
        }*/

        /*
        public static void SetSeasonInfo(SeasonGameInfo info)
        {
            Instance_.gameInfo = info;
        }*/

        /*
        public static void SetArenaInfo(ArenaGameInfo info)
        {
            Instance_.arenaInfo = info;
        }

        public static void SetCampaignInfo(CampaignGameInfo info)
        {
            Instance_.campainInfo = info;
        }

        public static void SetDeathMatchInfo(ClassicGameInfo info)
        {
            Instance_.deathInfo = info;
        }*/

        //게임의 정보를 가지고 있는 SimulGameInfo 데이터를 얻어옴
        public static SimulGameInfo GetGameInfo()
        {
            return Instance_.sMain.GetGameInfo();
        }

        
        /*
        //밸런스 상수값
        public static GameConstCommon GetConstCommon()
        {
            return Instance_.sMain.GetConstCommon();
        }

        //밸런스 상수값 세팅
        public static void SetConstCommon(GameConstCommon info)
        {
            Instance_.sMain.setConstCommon(info);
        }*/

        //시뮬레이션 된 배팅의 결과를 오브젝트화 하여 반환(오토 모드 구성을 위해 반드시 필요함)
        public static SimulBattingData GetBattingResult()
        {
            return Instance_.sMain.GetBattingResult();
        }


        /*
        public static SimulBattingData GetBattingResultFromList(int index)
        {
            return Instance_.sMain.GetBattingResultFromList(index);
        }*/



#if _RewindMode
        //이미 시뮬레이션된 배팅의 결과를 리스트에 저장한후 해당 인덱스의 배팅결과를 반환(리와인드 모드 구성을 위해 반드시 필요)
        public static SimulBattingData GetRewindBattingData()
        {
            return Instance_.sMain.GetRewindBattingData();
        }

        //이미 시뮬레이션된 배팅의 결과를 리스트에 저장한후 해당 인덱스의 선수정보를 반환(리와인드 모드 구성을 위해 반드시 필요)
        public static SimulCurrentPlayerData GetRewindPlayerData()
        {
            return Instance_.sMain.GetRewindPlayerData();
        }

        public static void SetNextRewindIndex()
        {
            ////UnityEngine.//Debug.Log("=====================>>결과가 NONE이 들어오면 다음 인덱스");
            Instance_.sMain.setNextRewindIndex();
        }
#endif
        

        // [결과 빨리 보기 모드]이외에 모드시 이 함수가 유일한 초기화 수단
        public static void InitGame(bool bHome, GameLineup gameLineup)
        {            
            Instance_.sMain.InitGame(bHome, gameLineup);
        }

        /// <summary>
        /// 게임을 위해 필요한 각종 데이터를 초기화 시킨다.
        /// 게임에 필요한 선수데이터는 SimulPlayerManager에서 관리한다.
        /// [빠른 결과 보기]모드시 이함수를 이용해 초기화
        /// </summary>    
        public static bool SimulEnd = false;
        public static void SimulateOneGame(bool bHome, GameLineup lineup, int homeStartOrd, int awayStartOrd)
        {
            SimulEnd = false;
            Instance_.sMain.SimulateOneGame(bHome, lineup, homeStartOrd, awayStartOrd);
            SimulEnd = true;
        }

        /// <summary>
        /// 이미 초기화된 상태에서 게임 시뮬레이트
        /// </summary>
        public static void GameSimulate()
        {
            Instance_.sMain.GameSimulate();
        }

        public static void SimulateGameToInning(int inning) 
        {
            Instance_.sMain.SimulateGameToInning(inning);
        }

        public static void SimulNextInning(bool bChangeInningProcess)
        {
            //bChangeInning ==> true인경우 changeInning 프로세스를 진행해준다
            Instance_.sMain.SimulNextInning(bChangeInningProcess);
        }
        
        /// <summary>
        /// 배팅 시뮬레이션
        /// 배팅과 맞물려 주루 수비 득점 등 모든 결과를 얻어온다
        /// 이것은 [초고속 모드] 이용한다
        /// 이것을 역산하여 래더모드에서 지원하는 [리와인드 모드]에 이용할 수 있다.
        /// </summary>    
        public static void SimulationBatting(bool bPitchCount)
        {
            ////UnityEngine.Debug.Log("[#########################################################################]");
            ////UnityEngine.Debug.Log("[##################            리와인드 시뮬레이션 시작               #############]");
            Instance_.sMain.SimulationBatting(bPitchCount);
            ////UnityEngine.Debug.Log("[##################            리와인드 시뮬레이션 종료               #############]");
            ////UnityEngine.Debug.Log("[#########################################################################]");
        }

        /// <summary>
        /// 배팅 시뮬레이션
        /// 실플레이와 연계하여 시뮬레이션을 작동할때는 이 함수를 이용한다
        /// 단지 타구의 질을 산출해내며 나머지 결과는 실 야구 엔진에 의존한다
        /// 이것은 [자동 플레이 또는 유저 직접 플레이 모드]에 이용한다
        /// </summary>    
        public static void SimulationBattingOnly()
        {
            //UnityEngine.Debug.Log("[#########################################################################]");
            //UnityEngine.Debug.Log("[##################            오토 시뮬레이션 시작               #############]");
            Instance_.sMain.SimulationBattingOnly();
            //UnityEngine.Debug.Log("[##################            오토 시뮬레이션 종료               #############]");
            //UnityEngine.Debug.Log("[#########################################################################]");
        }

        /// <summary>
        /// 다음 타자 초기화
        /// 실플레이와 연계하여 시뮬레이션을 작동할때 시뮬레이션 타자 동기화를 위해 이함수를 호출한다.
        /// </summary>    
        public static void SimulInitBatter()
        {
            Instance_.sMain.SimulInitBatter();
        }

        //
        public static bool SimulNextBatter(bool bChangeCheck)
        {
            return Instance_.sMain.SimulNextBatter(bChangeCheck);
        }

        /// <summary>
        /// 배팅뷰 투타 대결 스킬
        /// </summary>
        /// <param name="manager"></param>
        public static bool CheckBattingviewSkill(BallPlayManager manager)
        {
            return Instance_.sMain.CheckBattingviewSkill(manager);
        }

        public static bool SetBattingviewSkill()
        {
            return Instance_.sMain.SetBattingviewSkill();
        }

        /// <summary>
        /// 매 피치시 스킬 검색
        /// </summary>
        public static void CheckSkillByPitch()
        {
            Instance_.sMain.CheckSkillByPitch();
        }

        /// <summary>
        /// 배팅뷰 투수 발동된 스킬 얻어오기
        /// </summary>
        /// <returns></returns>
        public static CSkill GetPitcherSkill()
        {
            return Instance_.sMain.getPitcherSkill();
        }

        //
        public static void SetPitcherSkill(CSkill skill)
        {
            Instance_.sMain.setPitcherSkill(skill);
        }

        /// <summary>
        /// 배팅뷰 타자 발동된 스킬 얻어오기
        /// </summary>
        /// <returns></returns>
        public static CSkill GetBatterSkill()
        {
            return Instance_.sMain.getBatterSkill();
        }


        //
        public static void SetBatterSkill(CSkill skill)
        {
            Instance_.sMain.setBatterSkill(skill);
        }

        /// <summary>
        /// 스킬버퍼
        /// </summary>
        public static int [] GetSkillBuff()
        {
            return Instance_.sMain.getSkillBuff();
        }

        /// <summary>
        /// 투타 스킬 대결시 타자 승리여부
        /// </summary>
        /// <returns></returns>
        public static bool CheckVsBatterWin()
        {
            return Instance_.sMain.checkVsBatterWin();
        }


        //
        public static void SetVsBatterWin(bool bBatterWin)
        {
            Instance_.sMain.setVsBatterWin(bBatterWin);
        }


        /// <summary>
        /// 매피치 발생한 투수 스킬
        /// </summary>
        /// <returns></returns>
        public static CSkill GetPitchPitcherSkill()
        {
            return Instance_.sMain.getPitchPitcherSkill();
        }

        /// <summary>
        /// 강제세팅
        /// </summary>
        /// <param name="skill"></param>
        public static void SetPitchPitcherSkill(CSkill skill)
        {
            Instance_.sMain.setPitchPitcherSkill(skill);
        }

        /// <summary>
        /// 매피치 발생한 타자 스킬
        /// </summary>
        /// <returns></returns>
        public static CSkill GetPitchBatterSkill()
        {
            return Instance_.sMain.getPitchBatterSkill();
        }

        /// <summary>
        /// 강제세팅
        /// </summary>
        /// <param name="skill"></param>
        public static void SetPitchBatterSkill(CSkill skill)
        {
            Instance_.sMain.setPitchBatterSkill(skill);
        }

        /// <summary>
        /// 매피치 발생한 포수 스킬
        /// </summary>
        /// <returns></returns>
        public static CSkill GetPitchCatcherSkill()
        {
            return Instance_.sMain.getPitchCatcherSkill();
        }

        /// <summary>
        /// 강제세팅
        /// </summary>
        /// <param name="skill"></param>
        public static void SetPitchCatcherSkill(CSkill skill)
        {
            Instance_.sMain.setPitchCatcherSkill(skill);
        }

        /// <summary>
        /// 뜬금포 리셋
        /// </summary>
        /// <param name="index"></param>
        public static void ResetSkillCount(int index, SkillID skill)
        {
            Instance_.sMain.ResetSkillCount(index, skill);
        }


        /// <summary>
        /// 다음 이닝 초기화
        /// 실플레이와 연계하여 시뮬레이션을 작동할때 시뮬레이션 이닝 동기화를 위해 이함수를 호출한다.
        /// </summary>    
        public static void SimulChangeInning(bool bChangeCheck)
        {
            Instance_.sMain.SimulChangeInning(bChangeCheck);
        }


        /// <summary>
        /// 리와인드 모드시 타자주자 동기화를 위해 이 함수를 호출한다.
        /// </summary>  
        public static bool GetHitterRunnerSafe()
        {
            return Instance_.sMain.GetHitterRunnerSafe();
        }

        
        /// <summary>
        /// 오토모드에서 타자 교체 여부 시뮬 매니저를 통해
        /// </summary>  
        public static bool CheckChangeBatter()
        {
            return Instance_.sMain.CheckChangeBatter();
        }

        /// <summary>
        /// 오토모드에서 주자 교체 여부를 시뮬 매니저를 통해
        /// </summary>  
        public static bool CheckChangeRunner()
        {
            return Instance_.sMain.CheckChangeRunner();
        }

        
#if HITBALLTYPE_RECORD
        public static void GetHitTypeRecord(int team, StreamWriter sw)
        {
            Instance_.getHitTypeRecord(team,sw);
        }
#endif

        public static void SyncGameRecord(BallPlayManager manager)
        {
            //Debug.Log("================================>>SyncGameRecord");
            Instance_.sMain.SyncGameRecord(manager);
        }

        public static void SyncData(BallPlayManager manager, bool bFromSimulation, bool fastInningSimul, bool changeFlag)
        {
            Instance_.sMain.SyncData(manager, bFromSimulation,fastInningSimul, changeFlag);
        }

        /// <summary>
        /// 게임의 결과를 세팅하여 서버에 보낼 준비를 한다.
        /// </summary> 
        public static void SimulResultSetting(BallPlayManager manager, bool bPitcherRecordSet)
        {
            //Debug.Log("SimulResultSetting");
            if (manager != null)
            {
                SimulGameInfo info = Instance_.sMain.GetGameInfo();
                for (int i = 0; i < 2; i++)
                {
                    info.run[i] = manager.nGameScore[i];
                    info.hit[i] = manager.nHitCount[i];
                    info.error[i] = manager.nErrorCount[i];
                    info.homerun[i] = manager.nHomerunCount[i];
                    info.steal[i] = manager.nStealCount[i];
                    info.strikeout[i] = manager.nStrikeOutCount[i];
                    info.doubleplay[i] = manager.nDPCount[i];
                    info.fourBall[i] = manager.nFourballCount[i];

                    for (int j = 0; j < SimulGameInfo.MAX_INNING; j++)
                    {
                        info.inningScore[i, j] = manager.nInningScore[i, j];
                    }

                    Instance_.sMain.SetWinLoseIndex(manager.winPitcherIndex, manager.losePitcherIndex);

                }
            }

            Instance_.sMain.SimulResultSetting(bPitcherRecordSet);
        }

        // 엔진과 시뮬엔진과의 주자 상태를 싱크시킨다
        public static void SyncRunner(runnerManager run)
        {
            Instance_.sMain.SyncRunner(run);
        }

        


        public static int GetChangerIndex(ChangerIndex index) //이함수를 아래함수로 바꿀것
        {
            return Instance_.sMain.GetChangerIndex(index);
        }

        public static void SetChangerIndex(ChangerIndex index, int value) //이함수를 아래함수로 바꿀것
        {
            Instance_.sMain.SetChangerIndex(index, value);
        }

        public static CPlayer GetChangePlayer(ChangerIndex index)
        {
            return Instance_.sMain.GetChangePlayer(index);
        }


        public static string GetGameSummury(int type = 1)
        {
            if (type == 1)
            {
                return Instance_.strGameSummury;
            }
            else
            {
                return Instance_.strGameSummury2;
            }
        }

        public static void AddGameSummuryInfo(string info, int type = 1)
        {
            if (Mode.gameMode == Mode.GamePlayMode.Ranking)
            {
                if (type == 1)
                {
                    //아레나에서만 적용가능 한걸로
                    Instance_.strGameSummury += info;
                }
                else
                {
                    Instance_.strGameSummury2 += info;
                }
                //////UnityEngine.//Debug.Log("=============================================>>> info add " + info);
            }
        }

        

        public static SimulRunner GetRunner(int index)
        {
            return Instance_.sMain.GetRunner(index);
        }

        public static void SetRunner(CPlayer player, int index, int curPos)
        {
            Instance_.sMain.SetRunner(player, index, curPos);
        }

        public static void SetQuickgameInfo(QuickGameInfo qInfo)
        {
            Instance_.sMain.SetQuickgameInfo(qInfo);
        }

        public static void SetGameInfo(QuickGameInfo qInfo, bool topInning, bool myTurn)
        {
            Instance_.sMain.SetGameInfo(qInfo, topInning, myTurn);
        }

        public static bool isEndGame()
        {
            return Instance_.sMain.isEndGame();
        }

        public static bool IsGoodByeCondition(bool[] onBase)
        {
            return Instance_.sMain.isGoodByeCondition(onBase);
        }


        public static void SetGoodBye(bool bGoodBye)
        {
            Instance_.sMain.setGoodByeHit(bGoodBye);
        }


        public static GameLineup GetSeasonGameLineup(SeasonTeamInfo homeTeam, SeasonTeamInfo awayTeam)
        {
            return Instance_.getSeasonGameLineup(homeTeam, awayTeam);
        }

        private GameLineup getSeasonGameLineup(SeasonTeamInfo homeTeam, SeasonTeamInfo awayTeam)
        {
            GameLineup lineup = new GameLineup();
            lineup.homeTeam = homeTeam.lineup;
            lineup.awayTeam = awayTeam.lineup;
            return lineup;
        }


        public static GameLineup GetRankGameLineup(RankedPlayTeamInfo homeTeam, RankedPlayTeamInfo awayTeam)
        {
            return Instance_.getRankGameLineup(homeTeam, awayTeam);
        }

        private GameLineup getRankGameLineup(RankedPlayTeamInfo homeTeam, RankedPlayTeamInfo awayTeam)
        {
            GameLineup lineup = new GameLineup();
            lineup.homeTeam = homeTeam.lineup;
            lineup.awayTeam = awayTeam.lineup;
            return lineup;
        }

        public static GameLineup GetRaceGameLineup(List<GameCardInfo> homeTeam, List<GameCardInfo> awayTeam)
        {
            return Instance_.getRaceGameLineup(homeTeam, awayTeam);
        }

        private GameLineup getRaceGameLineup(List<GameCardInfo> homeTeam, List<GameCardInfo> awayTeam)
        {
            GameLineup lineup = new GameLineup();
            lineup.homeTeam = homeTeam;
            lineup.awayTeam = awayTeam;
            return lineup;
        }

        /*
        //우선은 임시 구성
        public static GameLineup GetNineTwoGameLineup(WalkoffPlayGameInfo info)
        {
            return Instance_.getNineTwoGameLineup(info);
        }*/
        /*
        private GameLineup getNineTwoGameLineup(WalkoffPlayGameInfo info)
        {
            //우선은 임시 구성
            GameLineup lineup = new GameLineup();
            lineup.homeTeam = new List<GameCardInfo>();
            lineup.awayTeam = new List<GameCardInfo>();

            //초기화
            lineup.homeTeam.Clear();
            lineup.awayTeam.Clear();

            int Max = 25;
            for (int i = 0; i < 14; i++)
            {
                //우리팀 세팅
                lineup.homeTeam.Add(info.myHitter);
            }

            info.myHitter.PlayerType
            
            //lineup.awayTeam = team;

            return lineup;
        }*/



        public static CPlayer getWinPitcher()
        {
            return Instance_.sMain.getWinPitcher();
        }

        public static CPlayer getLosePitcher()
        {
            return Instance_.sMain.getLosePitcherIndex();
        }


    }
}