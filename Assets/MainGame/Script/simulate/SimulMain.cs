//#define HITBALLTYPE_RECORD
//#define CHANGE_BATTER_TEST
//#define CHANGE_RUNNER_TEST
//#define _TEST_NETWORK //시뮬레이션 엔진을 이용하여 급 네트워크 테스트 할때 필요
//#define _DPTEST
//아래 3개만 유효한 상태 - 나중에 다 지워
//#define STEAL_TEST
//#define NOSCORE_GAME
//#define LOOK_BALL
//#define _PITCHER_CHANGE_TEST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebConnector;
using System.IO;
using System;

namespace BaseBall.BallPlay
{
    public class SimulMain
    {
        const int MYTEAM = 0,
                  CPUTEAM = 1;

        private SimulPlayer sPlayer;
                
        //game info
        private SimulGameInfo gameInfo; //게임 정보를 이 오브젝트로부터 얻어온다
        
        //리와인드 데이터 리스트
#if _RewindMode
        private List<SimulBattingData> rewindData = new List<SimulBattingData>();
        private List<SimulCurrentPlayerData> rewindPlayerData = new List<SimulCurrentPlayerData>();
        private int rewindIndex, rewindPlayerIndex;
#endif

        //Rule
        private int curBallCount, curStrikeCount;    //현재 볼카운트
        private int curIndex;                        //공격팀 인덱스
        private int currentInning;                   //현재 이닝
        private int outCountIF;                             //에러를 포함한 아웃카운트(3이 넘어가면 더이상 자책이 기록되지 않는다)
        private bool bTopInning;                     //초이닝    
        private bool bMyHome;                               //player의 홈여부
        private bool bGameEnd;          //게임 종료 플래그 (true시 게임 종료)
        private bool bInningEnd;        //이닝 종료 플래그 (true시 이닝 종료)
        private bool bBattingEnd;       //배팅 종료 플래그 (true시 배팅 종료)
        private bool bGoodbyeFlag;      //굿바이 여부
        private bool bErrorHappen;      //에러 발생     -> 매 배팅시 초기화
        private bool bFromGame;         //게임 엔진과의 연동 여부
        private bool bStealThreeOut;    //도루시 주루사로 인한 공수교대

        //투타 대결 결과
        private SimulResultState resultState;           //배팅의 최종결과 상태 
        private SimulResultByPitch pitchedResult;       //투구의 결과
        private SimulHitType hitType;                   //타격의 타입 (플라이,라이너,그라운더)
        private SimulBuntType buntType, buntSuccessType;
        private SpecificGrounderType grounderType;      //그라운더의 타입
        private SpecificLinerType linerType;            //라이너의 타입
        private SpecificFlyType flyType;                //플라이의 타입
        private SpecificBuntType buntResultType;               //번트의 타입


        //컨택밸류 타이밍밸류 최종배팅파워
        private BattingTiming bTimingResult;
        private BattingContact bContactResult;
        private float batterMaxPower; //

        //투수
        private CPlayer pitcher, lastPitcher;
        private int curStamina;
        private int pitchedNum;     //한타자에게 던진 공의 수
        private int pControl, pGuwee, pFinalGuwee;
        private int pGujong;
        private int pSpeed;     //130,125,120,115,110,105,100         
        private bool bStrike;
        private bool bLastFastBall; //이전에 직구를 던졌는지 여부
        //private int fastballValue;  //직구의 위력    
        private ControlValue controlValue;
        private FatigueStep fatigueStep;
        private PinchStep pinchStep;
        private int pitcherPower;   //컨트롤과 구위의 도합으로 산출한값

        //투수교체 및 업적(승,패,세이브, 홀드, 블론)        
        private ChangeType pitcherChangeType;
        private bool bPitcherChangeFlag;
        private bool[] bLongReliefOn;
        private bool[] bChaseOn;
        private bool[] bSetupOn;
        private bool[] bSaveOn;
        private int[] allowRun;      //실점허용
        private int[] startInning;   //시작이닝
        private int defense, offense;
        private int inPitcher;
        private int[] winPitcherIndex, losePitcherIndex;


        //타자
        private CPlayer batter;
        private int bContact, bPower, bTando, bEye, bBonus;
        private bool bCheckSwing;
        
        //주루
        private SimulRunner[] runner;
        private bool[] runnerActive;
        private int[] runnerValue;
        private int[] runnerCurPos;
        private int[] runnerRunningOut; //주루사 관련 (quick에서만 필요
        private bool[] bOnBase;
        private int hitterRunnerIndex;
        private bool bGrounderFlag, bBaseTagFlag, bOneMoreBaseFlag, bFourballFlag;
        private bool bRunnerOutFlag;
        private int stealPitchNum;  //매 배터당 초기화
        private int curRunnerIndex;
        private int pickoffCount;

        //주루 관련
        private SimulStealState runnerStealState;
        private bool bStealTry;//, bDelayStealTry;
        private bool bStealSuccess;//, bDelayStealSuccess;
        private int doublePlayStep; //0:없음, 1:더블플레이 2:홈->1루 더블플레이 3:병살저지 스킬발동
        private bool doublePlaySuccess; //

        //수비
        private SimulFielder[] fielder;  //수비수    
        private GrounderCatchType grounderCatchType;        //땅볼의 처리 타입
        private FlyCatchType flyCatchType;                  //플라이의 처리 타입
        private ErrorType errorType;
        private int grounderThrowBase;                      //그라운더시 던진 베이스
        private int fIndex, fIndex2, buntFIndex;                        //처리야수, 2차 처리 야수
        private int grounderFiedingValue;                          //그라운더의 딜레이 고려한 필딩값
        private int shiftBonus;                                    //루상에 주자로 인한 쉬프트 보너스 

        //스킬
        /// <summary>
        /// 현재 타석에서 발생되는 모든 스킬정보
        /// 엔진에서는 이데이터를 참조하지 않는다.
        /// </summary>
        private Dictionary<SkillUseStep, SimulSkillInfo> curBattingSkillInfo = new Dictionary<SkillUseStep, SimulSkillInfo>();

        private bool MeaNoonShow;
        private bool HeosimShow;
        private bool MeahogShow;
        private bool bMeahogSkillInvalidity;    //매혹에 의한 스킬 무효화



        //
        private bool bAutoMode;

                
        //기타
        private int[] changerIndex = new int[7];// private int inBatter, outBatter, outPitcher, outRunner, inFielder, outFielder, firstBase
        //step
        private SimulStep step; //필요없을듯

        private string strBatterResult = null;

        //스킬 정보 잃은 경우 버퍼
        private int[] vsSkillBuffer = new int[4];


        public void initPlayerData(SimulPlayer player)
        {
            sPlayer = player;
        }

        ////////////////////////////////////////////////////////////////////
        //외부호출
        ////////////////////////////////////////////////////////////////////
        public SimulGameInfo GetGameInfo()  //false
        {
            return gameInfo;            
        }



#if _RewindMode
        //재생을 위해 필요한 데이터를 얻어옴
        public RewindData GetRewindData()
        {
            RewindData data = new RewindData();

            data.rewindData = rewindData;
            data.rewindPlayerData = rewindPlayerData;

            return data;
        }
#endif

        //시뮬레이션 된 배팅의 결과를 오브젝트화 하여 반환(오토 모드 구성을 위해 반드시 필요함)
        public SimulBattingData GetBattingResult()
        {
            return makeResult(-1);  //인덱스화 시키지 않음
        }

        /*
        public SimulBattingData GetBattingResultFromList(int index)
        {
            return battingData[index];
        }*/

#if _RewindMode
        //이미 시뮬레이션된 배팅의 결과를 리스트에 저장한후 해당 인덱스의 배팅결과를 반환(리와인드 모드 구성을 위해 반드시 필요)
        public SimulBattingData GetRewindBattingData()
        {
            return getRewindBattingData();
        }

        //이미 시뮬레이션된 배팅의 결과를 리스트에 저장한후 해당 인덱스의 선수정보를 반환(리와인드 모드 구성을 위해 반드시 필요)
        public SimulCurrentPlayerData GetRewindPlayerData()
        {
            return getRewindPlayerData();
        }
#endif


        // [결과 빨리 보기 모드]이외에 모드시 이 함수가 유일한 초기화 수단
        public void InitGame(bool bHome, GameLineup gameLineup)
        {
            bMyHome = bHome;
            curIndex = (bMyHome ? CPUTEAM : MYTEAM);  //현재 공격 인덱스 초기화
            initGame();
        }

        /// <summary>
        /// 게임을 위해 필요한 각종 데이터를 초기화 시킨다.
        /// 게임에 필요한 선수데이터는 SimulPlayerManager에서 관리한다.
        /// [빠른 결과 보기]모드시 이함수를 이용해 초기화
        /// </summary>    
        public void SimulateOneGame(bool bHome, GameLineup lineup, int homeStartOrd, int awayStartOrd)
        {
            simulate(bHome, lineup, homeStartOrd, awayStartOrd);
        }

        public void GameSimulate()
        {
            gameSimulate();
        }

        public void SimulateGameToInning(int inning)
        {
            gameSimulateToInning(inning);
        }

        public void SimulNextInning(bool bChangeInningProcess)
        {
            gameSimulateNextInning(bChangeInningProcess);
        }

        /// <summary>
        /// 배팅 시뮬레이션
        /// 배팅과 맞물려 주루 수비 득점 등 모든 결과를 얻어온다
        /// 이것은 [초고속 모드] 이용한다
        /// 이것을 역산하여 래더모드에서 지원하는 [리와인드 모드]에 이용할 수 있다.
        /// </summary>    
        public void SimulationBatting(bool bPitchCount)
        {
            bAutoMode = true;
            battingSimulate(bPitchCount);
        }

        /// <summary>
        /// 배팅 시뮬레이션
        /// 실플레이와 연계하여 시뮬레이션을 작동할때는 이 함수를 이용한다
        /// 단지 타구의 질을 산출해내며 나머지 결과는 실 야구 엔진에 의존한다
        /// 이것은 [자동 플레이 또는 유저 직접 플레이 모드]에 이용한다
        /// </summary>    
        public void SimulationBattingOnly()
        {
            battingOnlySimulate();
        }

        /// <summary>
        /// 다음 타자 초기화
        /// 실플레이와 연계하여 시뮬레이션을 작동할때 시뮬레이션 타자 동기화를 위해 이함수를 호출한다.
        /// </summary>    
        public void SimulInitBatter()
        {
            gameInfo.initCount(false);
            initBatter();
        }


        /// <summary>
        /// 외부에서 다음타자 호출
        /// </summary>
        public bool SimulNextBatter(bool bChangeCheck)
        {
            nextBatter(bChangeCheck);
            return vsSkillType;
        }

        /// <summary>
        /// 외부에서 배팅뷰 스킬 대결
        /// 야구 엔진에서 참조
        /// </summary>
        /// <param name="manager">액션엔진 매니저</param>
        /// <returns>대결 스킬 발생시 true 리턴</returns>
        public bool CheckBattingviewSkill(BallPlayManager manager)
        {
#if _Skill_Display
            //연출테스트용
            return returnSkillDisplay(manager);
#else
            ////UnityEngine.//Debug.Log("===========================>>시뮬레이션의 타자 투수 스킬 세팅은 여기서!!!!");
            skillConditionState(manager);
            return checkBattingviewSkill();
#endif
        }

        /// <summary>
        /// 외부에서 배팅뷰 스킬 대결
        /// 시뮬모드에서 참조
        /// </summary>
        public bool SetBattingviewSkill()
        {
            bAutoMode = true;
            return setBattingviewSkill();
        }

#if _Skill_Display
        //연출테스트용
        private bool returnSkillDisplay(BallPlayManager manager)
        {
            curPitcherSkill = null;
            curBatterSkill = null;
            bool bVs = false;
            if (manager.pitcherSkill_Display_test != pSkillDisplay.NoSkill)
            {
                if (manager.pitcherSkill_Display_test == pSkillDisplay.Sun_Du_Ta_Ja)
                {
                    curPitcherSkill = new CSkill(10003, SkillIndex.SunduKiller, true);
                }
                else if (manager.pitcherSkill_Display_test == pSkillDisplay.Chu_Gyeog_Bon_Neung)
                {
                    bVs = true;
                    curPitcherSkill = new CSkill(10004, SkillIndex.ChaseInstinct, true);
                }
                else if (manager.pitcherSkill_Display_test == pSkillDisplay.Bul_Kkot_Tu_Hon)
                {
                    bVs = true;
                    curPitcherSkill = new CSkill(10005, SkillIndex.FrameFight, true);
                }
                else if (manager.pitcherSkill_Display_test == pSkillDisplay.Kang_Sim_Jang)
                {
                    bVs = true;
                    curPitcherSkill = new CSkill(10006, SkillIndex.SteelHeart, true);
                }
                else if (manager.pitcherSkill_Display_test == pSkillDisplay.Too_Soo_Wi_Ab)
                {
                    bVs = true;
                    curPitcherSkill = new CSkill(10009, SkillIndex.PitcherOverwhelming, true);
                }
                else if (manager.pitcherSkill_Display_test == pSkillDisplay.Chrisma)
                {
                    curPitcherSkill = new CSkill(10011, SkillIndex.Charisma, true);
                }
            }
            if (manager.batterSkill_Display_test != bSkillDisplay.NoSkill)
            {
                if (manager.batterSkill_Display_test == bSkillDisplay.Ta_Ja_Wi_Ab)
                {
                    curBatterSkill = new CSkill(20010, SkillIndex.BatterOverwhelming, false);
                    if (bVs == true)
                    {
                        vsBatterWin = MyMath.Half();
                        return true;
                    }
                }
                else if (manager.batterSkill_Display_test == bSkillDisplay.Gang_Seub_Ta_Gu)
                {
                    curBatterSkill = new CSkill(20011, SkillIndex.AssaultBall, false);
                }
                else if (manager.batterSkill_Display_test == bSkillDisplay.Chance_Man)
                {
                    curBatterSkill = new CSkill(20012, SkillIndex.ChanceMan, false);
                    if (bVs == true)
                    {
                        vsBatterWin = MyMath.Half();
                        return true;
                    }
                }
                else if (manager.batterSkill_Display_test == bSkillDisplay.Bunt_Sin)
                {
                    curBatterSkill = new CSkill(20013, SkillIndex.GodOfBunt, false);
                }
                else if (manager.batterSkill_Display_test == bSkillDisplay.Tteun_Geum_Po)
                {
                    curBatterSkill = new CSkill(20014, SkillIndex.Unexpected, false);
                }
            }
            return false;
        }

#endif

        /// <summary>
        /// 외부에서 매 피치별 스킬 체크
        /// </summary>
        public void CheckSkillByPitch()
        {
            checkSkillByPitch();
        }

        /// <summary>
        /// 다음 이닝 초기화
        /// 실플레이와 연계하여 시뮬레이션을 작동할때 시뮬레이션 이닝 동기화를 위해 이함수를 호출한다.
        /// </summary>    
        public void SimulChangeInning(bool bChangeCheck)
        {
            changeInning(bChangeCheck);
        }

        /// <summary>
        /// 외부에서 현재 투수 스킬값 얻어옴
        /// </summary>
        /// <returns></returns>
        public CSkill getPitcherSkill()
        {
            return curPitcherSkill;
        }


        /// <summary>
        /// 외부에서 스킬 강제 세팅
        /// </summary>
        /// <param name="skill"></param>
        public void setPitcherSkill(CSkill skill)
        {
            curPitcherSkill = skill;
        }

        /// <summary>
        /// 외부에서 현재 타자 스킬값 얻어옴
        /// </summary>
        /// <returns></returns>
        public CSkill getBatterSkill()
        {
            return curBatterSkill;
        }



        /// <summary>
        /// 외부에서 스킬 강제 세팅
        /// </summary>
        /// <param name="skill"></param>
        public void setBatterSkill(CSkill skill)
        {
            curBatterSkill = skill;
        }

        /// <summary>
        /// 투타 스킬 대결시 타자 승리여부
        /// </summary>
        /// <returns></returns>
        public bool checkVsBatterWin()
        {
            return vsBatterWin;
        }

        /// <summary>
        /// 투타 스킬대결 세팅
        /// </summary>
        /// <param name="batterWin"></param>
        public void setVsBatterWin(bool batterWin)
        {
            vsBatterWin = batterWin;
        }


        /// <summary>
        /// 외부에서 현재 발동중이 투수 피치 스킬값 얻어옴
        /// </summary>
        /// <returns></returns>
        public CSkill getPitchPitcherSkill()
        {
            return pitchPitcherSkill;
        }

        /// <summary>
        /// 외부에서 현재 발동중인 타자 피치 스킬값 얻어옴
        /// </summary>
        /// <returns></returns>
        public CSkill getPitchBatterSkill()
        {
            return pitchBatterSkill;
        }

        /// <summary>
        /// 외부에서 현재 발동중이 포수 피치 스킬값 얻어옴
        /// </summary>
        /// <returns></returns>
        public CSkill getPitchCatcherSkill()
        {
            return pitchCatcherSkill;
        }

        /// <summary>
        /// 외부에서 투수 피치 강제세팅
        /// </summary>
        /// <returns></returns>
        public void setPitchPitcherSkill(CSkill skill)
        {
            pitchPitcherSkill = skill;
        }

        /// <summary>
        /// 외부에서 타자 피치 강제세팅
        /// </summary>
        /// <returns></returns>
        public void setPitchBatterSkill(CSkill skill)
        {
            pitchBatterSkill = skill;
        }

        /// <summary>
        /// 외부에서 포수 피치 강제세팅
        /// </summary>
        /// <returns></returns>
        public void setPitchCatcherSkill(CSkill skill)
        {
            pitchCatcherSkill = skill;
        }

        /// <summary>
        /// 스킬 버퍼 리턴
        /// </summary>
        /// <returns></returns>
        public int[] getSkillBuff()
        {
            return vsSkillBuffer;
        }


        /// <summary>
        /// 뜬금포 리셋
        /// </summary>
        /// <param name="index"></param>
        public void ResetSkillCount(int index, SkillID skill)
        {
            resetSkillCount(index, skill);
        }

        /// <summary>
        /// 리와인드 모드시 타자주자 동기화를 위해 이 함수를 호출한다.
        /// </summary>  
        public bool GetHitterRunnerSafe()
        {
            return getHitterRunnerSafe();
        }


        /// <summary>
        /// 오토모드에서 타자 교체 여부 시뮬 매니저를 통해
        /// </summary>  
        public bool CheckChangeBatter()
        {
            return checkChangeBatter();
        }

        /// <summary>
        /// 오토모드에서 주자 교체 여부를 시뮬 매니저를 통해
        /// </summary>  
        public bool CheckChangeRunner()
        {
            return checkChangeRunner();
        }


#if HITBALLTYPE_RECORD
        public void GetHitTypeRecord(int team, StreamWriter sw)
        {
            getHitTypeRecord(team,sw);
        }
#endif


        /// <summary>
        /// 게임의 결과를 세팅하여 서버에 보낼 준비를 한다.
        /// </summary> 
        public void SimulResultSetting(bool bPitcherRecordSet)
        {
            resultSetting(bPitcherRecordSet);
        }


        public int GetChangerIndex(ChangerIndex index)  //이함수를 아래함수로 바꿀것
        {
            return changerIndex[(int)index];
        }

        public void SetChangerIndex(ChangerIndex index, int value)  //이함수를 아래함수로 바꿀것
        {
            changerIndex[(int)index] = value;
        }

        public CPlayer GetChangePlayer(ChangerIndex index)
        {
            CPlayer player = null;

            return player;
        }

        public SimulRunner GetRunner(int index)
        {
            if (runnerActive[index] == true)
            {
                return runner[index];
            }
            else
            {
                return null;
            }
        }


        public void SetRunner(CPlayer player, int index, int curPos)
        {
            if (runnerActive[index] == true)
            {
                if (runner[index] == null)
                {
                    runner[index] = new SimulRunner();
                }
                runner[index].runner = player;
                runner[index].curPos = curPos;
            }            
        }

        public void SetWinLoseIndex(int[] winIndex, int[] loseIndex)
        {
            for (int i = 0; i < 2; i++)
            {
                winPitcherIndex[i] = winIndex[i];
                losePitcherIndex[i] = loseIndex[i];
            }
        }

        //게임 종료 여부
        public bool isEndGame()
        {
            return bGameEnd;
        }

        //게임끝내기 여부
        public bool isGoodByeCondition(bool [] onBase)
        {
            if (bTopInning == false && currentInning >= Mode.finalInning)
            {
                int gab = getScoreGab(curIndex);
                if (gab < 0)
                {
                    //지고 있는 경우
                    int runner = 1;
                    foreach (bool bOn in onBase)
                    {
                        if (bOn == true)    runner++;
                    }

                    if (gab + runner > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /*
        public void setConstCommon(GameConstCommon info)
        {
            constCommon = info;

        }*/

        public void setGoodByeHit(bool bGoodBye)
        {
            bGoodbyeFlag = bGoodBye;
            if (bGoodbyeFlag == true)
            {
                bGameEnd = true;
                bInningEnd = true;
                bBattingEnd = true;
            }
        }


        /// <summary>
        /// 액션엔진과 시뮬레이션엔진과의 기록 동기화
        /// </summary>
        /// <param name="manager">액션 엔진 매니저</param>
        public void SyncGameRecord(BallPlayManager manager)
        {
            for (int i = 0; i < 2; i++)
            {
                gameInfo.run[i] = manager.nGameScore[i];
                gameInfo.hit[i] = manager.nHitCount[i];
                gameInfo.error[i] = manager.nErrorCount[i];
                gameInfo.homerun[i] = manager.nHomerunCount[i];
                gameInfo.steal[i] = manager.nStealCount[i];
                gameInfo.strikeout[i] = manager.nStrikeOutCount[i];
                gameInfo.doubleplay[i] = manager.nDPCount[i];
                gameInfo.fourBall[i] = manager.nFourballCount[i];

                for (int j = 0; j < SimulGameInfo.MAX_INNING; j++)
                {
                    gameInfo.inningScore[i, j] = manager.nInningScore[i, j];
                }

                //gameInfo.curAP[i] = manager.curAP[i];
                //gameInfo.maxAP[i] = manager.maxAP[i];

            }
            currentInning = manager.nInningCount;
            gameInfo.outCount = manager.nOutCount;
            gameInfo.strikeCount = gameInfo.ballCount = 0;

            bTopInning = manager.bTopInning;
            if (bMyHome)
            {
                curIndex = bTopInning ? CPUTEAM : MYTEAM;
            }
            else
            {
                curIndex = bTopInning ? MYTEAM : CPUTEAM;
            }

            for (int i = 0; i < 2; i++)
            {
                winPitcherIndex[i] = manager.winPitcherIndex[i];
                losePitcherIndex[i] = manager.losePitcherIndex[i];
            }
        }


        /// <summary>
        /// 시뮬 <-> 액션과 데이터 동기화
        /// </summary>
        /// <param name="manager">액션엔진 매니저</param>
        /// <param name="bFromSimulation">true인 경우 시뮬로부터</param>
        /// <param name="fastInningSimul">안씀</param>
        /// <param name="changeFlag">안씀</param>
        public void SyncData(BallPlayManager manager, bool bFromSimulation, bool fastInningSimul, bool changeFlag)
        {
            if (bFromSimulation == false)
            {
                Debug.Log("SyncData To Simulation");
                //선수데이터 동기화
                batter = manager.batter.pBatter;
                pitcher = manager.pitcher.pPitcher;
                for (int i = 0; i < 9; i++)
                {
                    fielder[i].setFielder(manager.field.fielder[i].pFielder);
                }
                //게임 기록 동기화
                SyncGameRecord(manager);
                //이닝종려 플래그
                bInningEnd = changeFlag;
            }
            else
            {
                Debug.Log("SyncData To Game");
                for (int i = 0; i < 2; i++)
                {
                    manager.nGameScore[i] = gameInfo.run[i];
                    manager.nHitCount[i] = gameInfo.hit[i];
                    manager.nErrorCount[i] = gameInfo.error[i];
                    manager.nHomerunCount[i] = gameInfo.homerun[i];
                    manager.nStealCount[i] = gameInfo.steal[i];
                    manager.nStrikeOutCount[i] = gameInfo.strikeout[i];
                    manager.nDPCount[i] = gameInfo.doubleplay[i];
                    manager.nFourballCount[i] = gameInfo.fourBall[i];

                    for (int j = 0; j < SimulGameInfo.MAX_INNING; j++)
                    {
                        manager.nInningScore[i, j] = gameInfo.inningScore[i, j];
                    }

                    //manager.curAP[i] = gameInfo.curAP[i];
                    //manager.maxAP[i] = gameInfo.maxAP[i];
                }

                manager.nInningCount = currentInning;
                manager.nOutCount = gameInfo.outCount;
                manager.nStrikeCount = manager.nBallCount = 0;

                for (int i = 0; i < 2; i++)
                {
                    manager.winPitcherIndex[i] = winPitcherIndex[i];
                    manager.losePitcherIndex[i] = losePitcherIndex[i];
                }

                if (fastInningSimul == false)
                {
                    //이닝까지 시뮬후 노멀모드로 실행하는 경우 이 부분은 반드시 빼고 SyncData를 실행해야함
                    //찬스모드 사용시만 필요
                    manager.bTopInning = bTopInning;
                    manager.offenseIndex = curIndex;
                    manager.defenseIndex = 1 - curIndex;
                }

            }
        }



        /// <summary>
        /// 액션엔진과 시뮬엔진의 주자 동기화
        /// </summary>
        /// <param name="run">액션엔진의 주자매니저</param>
        public void SyncRunner(runnerManager run)
        {
            for (int i = 0; i < 4; i++)
            {
                runnerActive[i] = run.runnerActive[i];
                bOnBase[i] = run.bOnBase[i];
                if (runnerActive[i] == true)
                {
                    if (runner[i] == null)
                    {
                        runner[i] = new SimulRunner();
                    }

                    runner[i].runner = run.runner[i].pRunner;
                    runner[i].arrayInex = run.runner[i].arrayIndex;
                    runner[i].dstPos = run.runner[i].destPos;
                    runner[i].curPos = run.runner[i].currentPos;
                    runner[i].bHitterRunner = (run.nHitterRunnerIndex == run.runner[i].runnerIndex ? true : false);
                    runner[i].bErrorRunner = run.runner[i].bErrorRunner;
                    runner[i].bLastPitcher = run.runner[i].bLastPitcher;
                    runner[i].bChangedRunner = run.runner[i].bChangedRunner;
                    runnerValue[i] = (int)RunnerState.OnBase;
                    runnerCurPos[i] = runner[i].curPos;
                }
                else
                {
                    if (runner[i] == null)
                    {
                        runner[i] = new SimulRunner();
                    }
                    runner[i].dstPos = -1;
                    runner[i].curPos = -1;
                    runnerValue[i] = (int)RunnerState.None;
                    runnerCurPos[i] = -1;
                }
            }
        }


        /// <summary>
        /// 퀵게임 인포를 이용해서 gameInfo 동기화
        /// </summary>
        /// <param name="qInfo"></param>
        public void SetGameInfo(QuickGameInfo qInfo, bool topInning, bool myTurn)
        {
            if (gameInfo == null)
            {
                gameInfo = new SimulGameInfo();
            }

            gameInfo.run[0] = qInfo.run[0];
            gameInfo.run[1] = qInfo.run[1];

            currentInning = qInfo.currentInning;
            bInningEnd = qInfo.bInningEnd;
            bGameEnd = qInfo.bGameEnd;
            gameInfo.outCount = qInfo.curOutCount;

            bTopInning = topInning;
            curIndex = (myTurn ? MYTEAM : CPUTEAM);
        }

        /// <summary>
        /// 퀵 시뮬레이터와 시뮬엔진과의 기록 동기화
        /// </summary>
        /// <param name="qInfo">퀵시뮬레이터의 데이터 인포</param>
        public void SetQuickgameInfo(QuickGameInfo qInfo)
        {
            qInfo.gameInfo = gameInfo;
            //qInfo.curBallCount = curBallCount;
            //qInfo.curStrikeCount = curStrikeCount;

            qInfo.run[0] = gameInfo.run[0];
            qInfo.run[1] = gameInfo.run[1];

            //qInfo.curIndex = curIndex;
            qInfo.currentInning = currentInning;
            qInfo.bTopInning = bTopInning;
            qInfo.bInningEnd = bInningEnd;
            qInfo.bGameEnd = bGameEnd;
            qInfo.curOutCount = gameInfo.outCount;

            //qInfo.hitterRunnerIndex = curRunnerIndex;
            //qInfo.doublePlaySuccess = doublePlaySuccess;
            /*
            qInfo.pRecord[0] = pitcher.getStat(Param.ST_PER);
            qInfo.pRecord[1] = pitcher.getStat(Param.ST_PH);
            qInfo.pRecord[2] = pitcher.getStat(Param.ST_PSO);
            qInfo.pRecord[3] = pitcher.getStat(Param.ST_PBB);
            qInfo.batterName = batter.getName();
            */
            //qInfo.batter = batter;
            //qInfo.pitcher = pitcher;


            qInfo.skillInfo = curBattingSkillInfo;

            qInfo.MeaNoonShow = MeaNoonShow;
            qInfo.HeosimShow = HeosimShow;
            qInfo.MeahogShow = MeahogShow;
                        

            doublePlaySuccess = false;
        }


        public CPlayer getWinPitcher()
        {
            for (int i = 0; i < 2; i++)
            {
                int index = winPitcherIndex[i];
                if (index >= 0)
                {
                    return sPlayer.GetPitcher(i, index, false);
                }
            }
            return null;
        }

        public CPlayer getLosePitcherIndex()
        {
            for (int i = 0; i < 2; i++)
            {
                int index = losePitcherIndex[i];
                if (index >= 0)
                {
                    return sPlayer.GetPitcher(i, index, false);
                }
            }
            return null;
        }

        /////////////////////////////////////////////////////////////////////////
        //시뮬레이션의 방법론
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 게임 시뮬레이션을 초기화 -> 시뮬레이션 ->결과 세팅까지 한프레임만에 수행한다.
        /// [빠른 결과 보기] 혹은 [테스트]용으로 사용
        /// </summary>
        private void simulate(bool bHome, GameLineup lineup, int homeStartOrd, int awayStartOrd)
        {
            buntTryCount = 0;
            ////UnityEngine.//Debug.Log("==============>>시작 time = " + System.DateTime.Now);
            //데이터 초기화
            initPlayer(bHome, lineup, homeStartOrd, awayStartOrd);

            //게임을 시뮬레이트 한다.
            step = SimulStep.Simulate;
            bAutoMode = false;
            gameSimulate();

            //결과와 기록을 세팅한다.
            step = SimulStep.Result;
            resultSetting(true);


            ////UnityEngine.//Debug.Log("==============>>종료 time = " + System.DateTime.Now);
        }

        

        /// <summary>
        /// 시뮬레이션을 배팅시뮬레이션 -> 다음타자 혹은 다음이닝 초기화 단위로 수행
        /// [초고속 모드] 와 역산하여 [리와인드 모드]에 이용가능 하다.
        /// 현재 안쓰임 ㅋㅋ
        /// </summary>
        private bool simulateByBatting()
        {
            battingSimulate(false);
            ///////////////////////////////////////
            //다음 타자 - 딜레이 필요 혹은 이 함수를 기준으로 위/아래 분리
            ///////////////////////////////////////
            nextBatter(true);
            if (bGameEnd == true)
            {
                //UnityEngine.Debug.Log("[게임 종료]");
                return true;
            }
            else
            {
                if (bInningEnd == true)
                {
                    //UnityEngine.Debug.Log("[이닝 종료]");
                    changeInning(true);
                }
            }
            return false;
        }

        /////////////////////////////////////////////////////////////////////////
        //시뮬레이션 메인 프레임
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 게임을 위해 필요한 각종 데이터를 초기화 시킨다.
        /// 게임에 필요한 선수데이터는 SimulPlayerManager에서 관리한다.
        /// [빠른 결과 보기]모드시 이함수를 이용해 초기화
        /// </summary>
        private void initPlayer(bool bHome, GameLineup lineup, int homeStartOrd, int awayStartOrd)
        {
            sPlayer.init();
#if _Test_Local
            {
                bMyHome = InGameDebug.MYHOME;
                curIndex = MYTEAM;  //현재 공격 인덱스 초기화
                //로컬
                sPlayer.MakePlayerLocal(0);
                sPlayer.MakePlayerLocal(1);
                sPlayer.MakePlayerLocal(2);
                sPlayer.MakePlayerLocal(3);
                sPlayer.MakePlayerLocal(4);
                initGame();
            }
#else
            {
                bMyHome = bHome;
                curIndex = (bMyHome ? CPUTEAM : MYTEAM);  //현재 공격 인덱스 초기화
                //서버
                sPlayer.MakePlayer(bMyHome, lineup, homeStartOrd, awayStartOrd);
                initGame();
            }
#endif
        }


        /// <summary>
        /// 전체 시뮬레이션을 한프레임에 수행하고 게임을 끝낸다.
        /// 반드시 선수데이터 초기화 이후에 호출되어야 한다.
        /// [빠른 결과 보기]모드시 반드시 이 함수를 이용해야 한다.
        /// </summary>
        private void gameSimulate() //bool bSetBattingData)
        {
            /*
            if (bSetBattingData == true)
            {
                rewindData.Clear();
                rewindPlayerData.Clear();
                rewindPlayerIndex = rewindIndex = 0;
            }*/

            //int battingResultIndex = 0;

            bFromGame = false; //게임 엔진의 영향을 받지 않음 -> 오히려 gameSimulate()함수는 게임엔진과 아예 상관이 없고 한번에 게임을 시뮬레이션 한다.
            bGameEnd = false;
            while (gamePlaying())
            {
                bInningEnd = false;
                while (inningPlaying())
                {
                    bBattingEnd = false;
                    checkChangePitcher();
                    while (battingPlaying(true))
                    {
                        bool bThreeOutChange = false;
                        pitchedNum++;
                        gameInfo.addPitch(1 - curIndex, pitcher);

                        resultState = getBattingState(); //진짜

                        if (resultState != SimulResultState.NONE)
                        {
                            //어떤 배팅의 최종결과가 발생했을 경우 결과값을 얻어온다    
                            bThreeOutChange = getBattingResult(resultState);

                        }
                        if (bThreeOutChange == true)
                        {
                            //쓰리아웃시 이닝 종료 플래그
                            bInningEnd = true;
                        }
                    }

                    /*리와인드 플레이인 경우
                    if (bSetBattingData == true)
                    {                        
                        SimulBattingData data = makeResult(battingResultIndex);
                        rewindData.Add(data);
                        SimulCurrentPlayerData player = makePlayerData(battingResultIndex);
                        rewindPlayerData.Add(player);
                        battingResultIndex++;
                    }*/

                    nextBatter(true);
                }
                changeInning(true);
            }
            ////Debug.Log("===============================>> 홈팀: " + gameInfo.run[0] + " : 원정: " + gameInfo.run[1]);
            //endGame();
            /////UnityEngine.//Debug.Log("======================>>> 시뮬레이팅 완료!!!!!");
        }


        /// <summary>
        /// 특정이닝까지의 시뮬레이션 실행을 한 프레임에 수행한다.
        /// 반드시 선수데이터 초기화 이후에 호출되어야 한다.
        /// [3이닝 플레이]모드시 반드시 이 함수를 이용해야 한다.
        /// </summary>
        private void gameSimulateToInning(int inningLimit)
        {
            //int battingResultIndex = 0;
            bAutoMode = false;
            bFromGame = false; //게임 엔진의 영향을 받지 않음 -> 오히려 gameSimulate()함수는 게임엔진과 아예 상관이 없고 한번에 게임을 시뮬레이션 한다.
            bGameEnd = false;
            while (gamePlaying())
            {
                bInningEnd = false;
                while (inningPlaying())
                {
                    bBattingEnd = false;
                    checkChangePitcher();
                    while (battingPlaying(true))
                    {
                        bool bThreeOutChange = false;
                        pitchedNum++;
                        gameInfo.addPitch(1 - curIndex, pitcher);

                        resultState = getBattingState(); //진짜

                        if (resultState != SimulResultState.NONE)
                        {
                            //어떤 배팅의 최종결과가 발생했을 경우 결과값을 얻어온다    
                            bThreeOutChange = getBattingResult(resultState);

                        }
                        if (bThreeOutChange == true)
                        {
                            //쓰리아웃시 이닝 종료 플래그
                            bInningEnd = true;
                        }
                    }


                    //battingData.Add(makeResult(battingResultIndex));
                    //battingResultIndex++;

                    nextBatter(true);
                }

                if (currentInning >= inningLimit && bTopInning == false)
                {
                    ////UnityEngine.//Debug.Log("=======================>> My득점: " + gameInfo.run[0] + "  vs  CPU득점: " + gameInfo.run[1]);
                    ////UnityEngine.//Debug.Log("======================>>6회말까지 수행 완료");
                    return;
                }
                else
                {
                    ////UnityEngine.//Debug.Log("======================>>currentInning = " + currentInning + " //bTopInning = " + bTopInning);
                    changeInning(true);
                }
            }
            
        }

        /// <summary>
        /// 한 이닝만 시뮬레이트
        /// 다음 이닝을 플레이 하기 위한 상태까지 세팅한다
        /// </summary>
        private void gameSimulateNextInning(bool bChangeInningProcess)
        {
            bAutoMode = false;
            bInningEnd = false;
            while (inningPlaying())
            {
                bBattingEnd = false;
                checkChangePitcher();
                while (battingPlaying(true))
                {
                    bool bThreeOutChange = false;
                    pitchedNum++;
                    gameInfo.addPitch(1 - curIndex, pitcher);

                    resultState = getBattingState(); //진짜

                    if (resultState != SimulResultState.NONE)
                    {
                        //어떤 배팅의 최종결과가 발생했을 경우 결과값을 얻어온다    
                        bThreeOutChange = getBattingResult(resultState);

                    }
                    if (bThreeOutChange == true)
                    {
                        //쓰리아웃시 이닝 종료 플래그
                        bInningEnd = true;
                    }
                }
                nextBatter(true);
            }

            if (bChangeInningProcess == true)
            {
                changeInning(true);
            }
        }
        
        /// <summary>
        /// 한프레임에 한타석의 시뮬레이션을 수행하고 다음 타자 혹은 이닝까지 초기화 한다. 
        /// 타격에 의한 결과값까지 얻어옴
        /// [시뮬레이션 모드] 혹은 (나중에 리와인드 플레이) 에 사용되는 함수
        /// </summary>
        //int count = 0;
        private void battingSimulate(bool bPithCount)
        {
            ////UnityEngine.//Debug.Log("==================>>battingSimulate batter = " + batter.getName());
            bFromGame = false; //게임 엔진의 영향을 받지 않고 오히려 엔진에 영향을 줌
            bBattingEnd = false;
            checkChangePitcher();
            while (battingPlaying(true))
            {
                bool bThreeOutChange = false;
                pitchedNum++;

                if (bPithCount == true)
                {
                    gameInfo.addPitch(1 - curIndex, pitcher);
                }

                resultState = getBattingState();

                if (resultState != SimulResultState.NONE)
                {
                    //어떤 배팅의 최종결과가 발생했을 경우 결과값을 얻어온다
                    curBallCount = gameInfo.ballCount;
                    curStrikeCount = gameInfo.strikeCount;
                    bThreeOutChange = getBattingResult(resultState);
                }
                if (bThreeOutChange == true)
                {
                    //쓰리아웃시 이닝 종료 플래그
                    bInningEnd = true;
                }
            }
        }

        /// <summary>
        /// 함프레임에 한타석의 시뮬레이션만 수행한다. 
        /// 파생되는 타구질만을 요구된다.
        /// 타격에 의한 결과값은 엔진에 맡긴다. (이것이 battingSimulate()와 가장 큰 차이)
        /// [유저 직접 플레이] 혹은 [오토 플레이 모드]에서 사용되는 함수이다.
        /// 타석의 초기화 교체 등등은 엔진의 몫
        /// </summary>
        private void battingOnlySimulate()
        {
            ////UnityEngine.//Debug.Log("==================>>battingOnlySimulate batter = " + batter.getName());
            bFromGame = true;   //게임 엔진의 영향을 받는다는 뜻
            bBattingEnd = false;
            //checkChangePitcher();//-->>**중요** 이놈은 여기서 불리지 않는다(게임엔진에서 처리)
            while (battingPlaying(false)) //battingPlaying에 false를 넘기는것은 도루를 체크하지 않는다는 뜻
            {
                pitchedNum++;
                resultState = getBattingState();

                if (resultState != SimulResultState.NONE)
                {
                    curBallCount = gameInfo.ballCount;
                    curStrikeCount = gameInfo.strikeCount;
                    bBattingEnd = true;
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////
        //시뮬레이션 진행 관련 루프
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 한게임이 끝나지 않는한 계속해서 루프를 돌게 만든다.
        /// 게임종료 플래그가 on이 되면 loop문을 빠져나오며 게임을 마무리한다.
        /// </summary>
        private bool gamePlaying()
        {
            if (bGameEnd == true)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 한 이닝이 끝나지 않는한 계속해서 루프를 돌게 만든다.
        /// 이닝종료 플래그가 on이 되면 loop문을 빠져나오며 게임을 마무리한다.
        /// </summary>
        private bool inningPlaying()
        {
            if (bInningEnd == true)
            {
                return false;
            }


            return true;
        }

        /// <summary>
        /// 한 타석이 끝나지 않는한 계속해서 루프를 돌게 만든다.
        /// 타석종료 플래그가 on이 되면 loop문을 빠져나오며 게임을 마무리한다.
        /// </summary>
        private bool battingPlaying(bool bStealCheck)
        {
            if (bBattingEnd == true)
            {
                return false;
            }

            pitchedResult = getPitchedResult(bStealCheck);

            return true;
        }

        /////////////////////////////////////////////////////////////////////////
        //시뮬레이션 결과 세팅
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 어떤 형식으로도 시뮬레이션이 종료되면 이함수를 호출한다.
        /// 경기를 하면서 발생된 각종 기록과 데이터를 서버에 보낼 수 있게 세팅한다
        /// </summary>
        private void resultSetting(bool bPitcherRecordSet)
        {
            //strWinPitcher = "";
            //strLosePitcher = "";
            //UnityEngine.//Debug.Log("===============================>>winPitcherIndex MYTEAM : " + winPitcherIndex[0]+" CPUTEAM : "+winPitcherIndex[1]);
            //UnityEngine.//Debug.Log("===============================>>losePitcherIndex MYTEAM : " + losePitcherIndex[0] + " CPUTEAM : " + losePitcherIndex[1]);
            //투수의 업적 세팅           

            if (bPitcherRecordSet == true)
            {
                setPitcherResult(sPlayer.GetPitcher(MYTEAM), MYTEAM);
                setPitcherResult(sPlayer.GetPitcher(CPUTEAM), CPUTEAM);
            }

            //string win = " (무)";
            //if (gameInfo.run[0] > gameInfo.run[1]) win = " (승)";
            //else if (gameInfo.run[0] < gameInfo.run[1]) win = " (패)";            
            //SimulManager.AddGameSummuryInfo("[ff3a3a]" + SimulPlayerManager.strMyTeam + " vs " + SimulPlayerManager.strCPUTeam + " : " + gameInfo.run[0] + "-" + gameInfo.run[1] + win + "[-]\n\n" + strWinPitcher + strLosePitcher, 2);

            //게임의 팀스탯 세팅
            sPlayer.setTeamStat(gameInfo, currentInning, bTopInning); //SimulResult.SetTeamStat(gameInfo);
            ////Debug.Log("===============================>> 홈팀: "+gameInfo.run[0] + " : 원정: " + gameInfo.run[1]);

        }

        /// <summary>
        /// 시뮬레이션에서 파생된 결과를 엔진에서 나타내어질 수 있도록 이함수를 호출하여 엔진에 전달
        /// </summary>
        private SimulBattingData makeResult(int index)
        {
            SimulBattingData resultData = new SimulBattingData();
            //카운트
            resultData.curBallCount = MyMath.SetMinMax(gameInfo.ballCount, 0, 3);
            resultData.curStrikeCount = MyMath.SetMinMax(gameInfo.strikeCount, 0, 2);
            resultData.pitchNum = pitchedNum;
            //타구질
            resultData.hitType = hitType;
            //resultData.flyType = flyType;
            //resultData.grounderType = grounderType;
            //resultData.linerType = linerType;
            resultData.buntResultType = buntResultType;
            //resultData.errorType = errorType;
            resultData.result = resultState;
            resultData.fIndex = fIndex;
            //resultData.fIndex2 = fIndex2;
            //resultData.throwBase = grounderThrowBase;
            resultData.bTimingResult = bTimingResult;
            resultData.bContactResult = bContactResult;
            //피칭
            //resultData.pGujong = pGujong;
            //resultData.bStrike = bStrike;
            //resultData.bCheckSwing = bCheckSwing;
            //resultData.controlValue = controlValue;
            //resultData.pitchResult = pitchedResult;
            //주루, 도루
            resultData.stealState = runnerStealState;
            //resultData.bStealTry = bStealTry;
            //resultData.bStealSuccess = bStealSuccess;
            //resultData.bDelayStealTry = bDelayStealTry;
            //resultData.bDelayStealSuccess = bDelayStealSuccess;
            //resultData.doublePlayStep = doublePlayStep;
            //주자 상황
            for (int i = 0; i < 4; i++)
            {                
                if (runnerActive[i] == true)
                {
#if !_Test_Local
                    resultData.runnerSeq[i] = runner[i].getRunner().getCard().cardSeq;
#endif
                    resultData.runnerName[i] = runner[i].getRunner().getName();
                    resultData.bRunnerActive[i] = runnerActive[i];
                    resultData.runnerCurPos[i] = runner[i].curPos;
                    resultData.runnerLineup[i] = runner[i].lineup;
                }
                else
                {
                    resultData.bRunnerActive[i] = false;
                    resultData.runnerCurPos[i] = -1;
                }

                resultData.runnerValue[i] = runnerValue[i];
            }

            /*
            if (index != -1)
            {
                resultData.qInfo = new QuickGameInfo();
                SetQuickgameInfo(resultData.qInfo);
            }*/

            resultData.index = index;

            pitchedNum = 0;
            //초기화
            bStealTry = false;
            bStealSuccess = false;
            //bDelayStealTry = false;
            //bDelayStealSuccess = false;
            doublePlayStep = 0;
            buntType = SimulBuntType.NONE;
            buntResultType = SpecificBuntType.NONE;

            
            

            //UnityEngine.Debug.Log(" #######################################>> " + currentInning + "회" + (bTopInning ? "초  " : "말  ") + "Batter " + batter.getName() + "   RESULT : " + resultState);

            return resultData;
        }

#if _RewindMode
        /// <summary>
        /// 리와인드시 현재 어떤선수가 있는지 여부를 알려줌
        /// </summary>
        private SimulCurrentPlayerData makePlayerData(int index)
        {
            SimulCurrentPlayerData player = new SimulCurrentPlayerData();
            player.index = index;
#if _Test_Local
            {
                //테스트용
                player.curBatterSeq = batter.picIndex;
                player.curPitcherSeq = pitcher.picIndex;
                for (int i = 0; i < 9; i++)
                {
                    player.fielderSeq[i] = fielder[i].getFielder().picIndex;
                }
            }
#else
            {
                player.curBatterSeq = batter.getCard().cardSeq;
                player.curPitcherSeq = pitcher.getCard().cardSeq;
                for (int i = 0; i < 9; i++)
                {
                    player.fielderSeq[i] = fielder[i].getFielder().getCard().cardSeq;
                }
            }
#endif
            player.curRunnerIndex = curRunnerIndex;
            return player;
        }


        /// <summary>
        /// 리스트에 저장된 리와인드 데이터의 인덱스를 검색하여 리턴
        /// </summary>
        private SimulBattingData getRewindBattingData()
        {
            SimulBattingData data = rewindData.Find(
                delegate(SimulBattingData rt)
                {
                    return rt.index == rewindIndex;
                }
            );
            rewindIndex++;

            return data;
        }


        /// <summary>
        /// 리스트에 저장된 리와인드 재생용 선수 정보데이터의 인덱스를 검색하여 리턴
        /// </summary>
        private SimulCurrentPlayerData getRewindPlayerData()
        {
            SimulCurrentPlayerData player = rewindPlayerData.Find(
                delegate(SimulCurrentPlayerData pl)
                {
                    return pl.index == rewindPlayerIndex;
                }
            );
            rewindPlayerIndex++;

            return player;
        }

        public void setNextRewindIndex()
        {
            rewindIndex++;
            rewindPlayerIndex++;
        }
#endif
        /////////////////////////////////////////////////////////////////////////
        // RULE
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 게임의 룰, 투타 및 주루 야수 초기화
        /// 게임 시작시 단 한번만 호출된다.
        /// </summary>
        private void initGame()
        {
#if HITBALLTYPE_RECORD
            linerOut = new int[2]{0,0};
            flyOut = new int[2] { 0, 0 };
            grounderOut = new int[2] { 0, 0 };
            linerHit = new int[2] { 0, 0 };
            flyHit = new int[2] { 0, 0 };
            grounderHit = new int[2] { 0, 0 };
            infieldHit = new int[2] { 0, 0 };
#endif
            //스킬 사용 초기화
            initSkillUseCount();
            curBattingSkillInfo.Clear();

            strBatterResult = null;
            buntTryCount = 0;   //번트 체크 예외처리
            pickoffCount = 0;   //픽오프 카운트
            
            gameInfo = new SimulGameInfo();
            gameInfo.init();    //게임 정보 초기화
            gameInfo.initCount(true);
            bTopInning = true;

            currentInning = 1;  //현재 이닝(one 베이스)            
            bGameEnd = false;
            bInningEnd = false;
            bBattingEnd = false;
            bStealThreeOut = false;
            bGoodbyeFlag = false;

            
            //주자
            runner = new SimulRunner[4];
            runnerActive = new bool[4];
            bOnBase = new bool[4];
            runnerValue = new int[4];
            runnerCurPos = new int[4];
            runnerRunningOut = new int[4];

            //야수
            fielder = new SimulFielder[9];
            for (int i = 0; i < 9; i++) fielder[i] = new SimulFielder();

            //투수
            bLongReliefOn = new bool[2];
            bChaseOn = new bool[2];
            bSetupOn = new bool[2];
            bSaveOn = new bool[2];
            allowRun = new int[2];
            startInning = new int[2];
            winPitcherIndex = new int[2];
            losePitcherIndex = new int[2];
            for (int i = 0; i < 2; i++)
            {
                //기세
                //gameInfo.maxAP[i] = 100;
                //gameInfo.curAP[i] = UnityEngine.Random.Range(30, 60);

                setChangeFlagInit(i);
                //승패 인덱스 초기화
                winPitcherIndex[i] = -1;
                losePitcherIndex[i] = -1;
            }

            pitchedNum = 0;
            shiftBonus = 0;
            changerIndex[(int)ChangerIndex.OutFielder] = -1;//  outFielder = -1;
            doublePlayStep = 0;
            doublePlaySuccess = false;
            bStealTry = false;
            //bDelayStealTry = false;


            initRunnerBase();

            if (Mode.bTieBreaker == true)
            {
                tiebreakSetting();
            }
            else if (Mode.b2outBaseLoadedMode == true)
            {
                currentInning = 9;
                gameInfo.outCount = 2;
                twoOutBaseLoadSetting();
            }

            initPitcher(false); //선발이면서 선발 플래그 on
            initBatter();
            initFielder(false);

            bInningOnceCheck = true;
            gameInfo.setCurrentInning(currentInning);
        }

        private void tiebreakSetting()
        {
            //아레나 승부치기 3번타자 세팅
            SimulPlayerManager.SetLineup(0, 2);
            SimulPlayerManager.SetLineup(1, 2);            
            //SimulManager.AddGameSummuryInfo("\n\n[" + currentInning + "회" + (bTopInning ? "초 " + SimulPlayerManager.strAwayTeam + " 공격" : "말 " + SimulPlayerManager.strHomeTeam + " 공격")+"]");
            
            //2루주자 세팅
            CPlayer secondRunner = sPlayer.GetFielder(curIndex, 0, false);
            //SimulManager.AddGameSummuryInfo("\n\n[ffde00]1번타자 " + secondRunner.getName() + ": 2루에 출루[-]");
            SimulRunner _runner2 = new SimulRunner();
            _runner2.makeRunnerOnBase(secondRunner, 0, SimulParm.SECONDBASE_INDEX);
            runnerActive[0] = true;
            runner[0] = _runner2;
            runnerValue[0] = (int)RunnerState.OnBase;
            runnerCurPos[0] = SimulParm.SECONDBASE_INDEX;
            bOnBase[SimulParm.SECONDBASE_INDEX] = true;

            //1루주자 세팅
            CPlayer firstRunner = sPlayer.GetFielder(curIndex, 1, false);
            //SimulManager.AddGameSummuryInfo("\n[ffde00]2번타자 " + firstRunner.getName() + ": 1루에 출루[-]");
            SimulRunner _runner1 = new SimulRunner();
            hitterRunnerIndex = 1;
            _runner1.makeRunnerOnBase(firstRunner, 1,SimulParm.FIRSTBASE_INDEX);
            runnerActive[1] = true;
            runner[1] = _runner1;
            runnerValue[1] = (int)RunnerState.OnBase;
            runnerCurPos[1] = SimulParm.FIRSTBASE_INDEX;
            bOnBase[SimulParm.FIRSTBASE_INDEX] = true;
        }

        private void twoOutBaseLoadSetting()
        {
            
        }

        /// <summary>
        /// 이닝 상태의 초기화
        /// 이닝 전환시 단 한번만 호출된다.
        /// </summary>
        private void changeInning(bool bChangeCheck)
        {
            if (checkEndGame() == true)
            {
                gameInfo.DEBUG_CONTROL_RESULT();
                bGameEnd = true;
                return;
            }

            curBattingSkillInfo.Clear();

            //필승의지 스킬체크
            if (gameInfo.allowChulu == 0)
            {
                pitcher.setPiledupSkill(SkillIndex.WinSpirit, 1, true);
                gameInfo.allowChulu = -1;
            }


            if (bTopInning == false)
            {
                currentInning++;
                gameInfo.setCurrentInning(currentInning);
            }

            curIndex = 1 - curIndex;
            bTopInning = !bTopInning;
            gameInfo.initCount(true);

            //이닝시 스킬 카운트 초기화
            initSkillCountEveryInning();
            
            //UnityEngine.Debug.Log("[이닝 체인지 중]=================================================================================================>>" + currentInning + (bTopInning ? "회초" : "회말") + "진행 중");

            bGameEnd = false;
            bInningEnd = false;
            bBattingEnd = false;

            outCountIF = 0;
            changerIndex[(int)ChangerIndex.OutFielder] = -1;// outFielder = -1;
            shiftBonus = 0;

            initRunnerBase();
            if (Mode.bTieBreaker == true)
            {
                tiebreakSetting();
            }
            else if (Mode.b2outBaseLoadedMode == true)
            {
                twoOutBaseLoadSetting();
            }
            initPitcher(false);
            initBatter();

            //UnityEngine.Debug.Log("[이닝 체인지 중]=================================================================================================>>" + currentInning + (bTopInning ? "회초" : "회말") + "진행 중             bChangeCheck = " + bChangeCheck);
            initFielder(bChangeCheck);


            bInningOnceCheck = true;

        }

        /// <summary>
        /// 게임이 끝나는지 여부를 체크하는 함수
        /// true리턴시 시뮬레이션 루프가 종료되고 resultSetting 프로세스로 진행된다.
        /// </summary>
        private bool checkEndGame()
        {
            if (bGoodbyeFlag == true)
            {
                return true;
            }

            if (Mathf.Abs(getScoreGab(curIndex)) >= SimulGameInfo.ColdGame)
            {
                if (bTopInning == false)
                    {
                        //콜드 게임 종료
                        //UnityEngine.Debug.Log("##################게임최종결과################################");
                        ////UnityEngine.//Debug.Log("====================>>" + currentInning + "회 콜드 게임으로 게임 종료");
                        //UnityEngine.Debug.Log("MYTEAM " + gameInfo.run[MYTEAM] + "  :  " + "CPUTEAM " + gameInfo.run[CPUTEAM]);
                        //UnityEngine.Debug.Log("##################게임최종결과################################");
                        setInningScoreClose(bTopInning);
                        return true;
                    }
            }

            if (currentInning >= Mode.maxInning)        //연장포함
            {
                if (bTopInning == false)
                {
                    //연장 12회말끝나면 
                    //UnityEngine.Debug.Log("##################게임최종결과################################");
                    //UnityEngine.//Debug.Log("====================>>연장 12회 종료로 게임 끝");
                    //UnityEngine.Debug.Log("MYTEAM " + gameInfo.run[MYTEAM] + "  :  " + "CPUTEAM " + gameInfo.run[CPUTEAM]);
                    //UnityEngine.Debug.Log("##################게임최종결과################################");
                    return true;
                }
            }

            if (currentInning >= Mode.finalInning) //정규이닝
            {
                //9회시
                ////UnityEngine.//Debug.Log("================================>> bTopInning " + bTopInning + " ===> bMyHome = " + bMyHome);
                ////UnityEngine.//Debug.Log("================================>> gameInfo.run[MYTEAM] = " + gameInfo.run[MYTEAM]);
                ////UnityEngine.//Debug.Log("================================>> gameInfo.run[CPUTEAM] = " + gameInfo.run[CPUTEAM]); 
                if (bTopInning == true)
                {
                    //초공격 끝난후
                    if (bMyHome == true)
                    {
                        //내가 홈인데 이기고 있으면 종료
                        if (gameInfo.run[MYTEAM] > gameInfo.run[CPUTEAM])
                        {
                            //UnityEngine.Debug.Log("##################게임최종결과################################");
                            ////UnityEngine.//Debug.Log("====================>>9회초 공격 후 게임 끝");
                            //UnityEngine.Debug.Log("MYTEAM " + gameInfo.run[MYTEAM] + "  :  " + "CPUTEAM " + gameInfo.run[CPUTEAM]);
                            //UnityEngine.Debug.Log("##################게임최종결과################################");
                            setInningScoreClose(bTopInning);
                            return true;
                        }
                    }
                    else
                    {
                        //상대가 홈인데 이기고 있으면 종료
                        if (gameInfo.run[MYTEAM] < gameInfo.run[CPUTEAM])
                        {
                            //UnityEngine.Debug.Log("##################게임최종결과################################");
                            //UnityEngine.Debug.Log("MYTEAM " + gameInfo.run[MYTEAM] + "  :  " + "CPUTEAM " + gameInfo.run[CPUTEAM]);
                            ////UnityEngine.//Debug.Log("====================>>9회초 공격 후 게임 끝");
                            //UnityEngine.Debug.Log("##################게임최종결과################################");
                            ////UnityEngine.//Debug.Log("===============================>>pre winPitcherIndex MYTEAM : " + winPitcherIndex[0] + " CPUTEAM : " + winPitcherIndex[1]);
                            ////UnityEngine.//Debug.Log("===============================>>pre losePitcherIndex MYTEAM : " + losePitcherIndex[0] + " CPUTEAM : " + losePitcherIndex[1]);
                            setInningScoreClose(bTopInning);
                            return true;
                        }
                    }
                }
                else
                {
                    //말공격 끝난 후
                    if (gameInfo.run[MYTEAM] != gameInfo.run[CPUTEAM])
                    {
                        //승부가 나면 종료
                        //UnityEngine.Debug.Log("##################게임최종결과################################");
                        ////UnityEngine.//Debug.Log("====================>>9회말 공격 후 게임 끝");
                        //UnityEngine.Debug.Log("MYTEAM " + gameInfo.run[MYTEAM] + "  :  " + "CPUTEAM " + gameInfo.run[CPUTEAM]);
                        //UnityEngine.Debug.Log("##################게임최종결과################################");
                        setInningScoreClose(bTopInning);
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// 굿바이 게임 체크
        /// </summary>
        private bool checkGoodByeGame()
        {
            if (currentInning >= Mode.finalInning)
            {
                //마지막회                
                if (bTopInning == false)
                {                    
                    //말공격 끝난 후
                    if (gameInfo.run[curIndex] > gameInfo.run[1 - curIndex])
                    {
                        //승부가 나면 종료
                        bGoodbyeFlag = true;
                        setInningScoreClose(bTopInning);
                        return true;
                    }
                }
            }
            return false;
        }



        //플레이 하지 않은 이닝을 클로즈해버린다.
        private void setInningScoreClose(bool topInning)
        {
            for (int i = (currentInning); i < SimulGameInfo.MAX_INNING; i++)
            {
                gameInfo.inningScore[MYTEAM, i] = SimulParm.NOPLAY_INNING;// -2000;
                gameInfo.inningScore[CPUTEAM, i] = SimulParm.NOPLAY_INNING;// -2000;
            }

            int bottom = bMyHome ? MYTEAM : CPUTEAM;
            if (topInning == true)
            {
                gameInfo.inningScore[bottom, currentInning - 1] = SimulParm.GAMEEND_INNING;//-1000;
            }
            else
            {
                gameInfo.inningScore[bottom, currentInning - 1] = -gameInfo.inningScore[bottom, currentInning - 1];
            }
        }




        /////////////////////////////////////////////////////////////////////////
        // 투수관련 초기화 및 세팅
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 투수를 초기화 한다
        /// 이 한수는 매 이닝 전환시 혹은 투수 교체시 호출된다.
        /// </summary>
        private void initPitcher(bool bChanged) //false
        {
            ////Debug.Log("==============>>투수 초기화");

            pitcher = sPlayer.GetPitcher(1 - curIndex);

#if _Test_Local
            //지워지워
        /*    List<SkillIndex> tempList = pitcher.getSkillList();
            if (tempList.Count > 0)
            {
                for (int i = 0; i < tempList.Count; i++)
                {
                    //Debug.Log("==========>>현재 투수에게 걸려있는 스킬  " + SimulParm.GetSkillInfo(tempList[i]).skillName);
                }
            }*/
#endif
            
            if (bChanged == true)
            {
                //구원투수인 경우
                //UnityEngine.Debug.Log("[투수 초기화]================>> 구원투수 스태미너 초기화");
                allowRun[1 - curIndex] = 0;
                startInning[1 - curIndex] = currentInning * 10 + gameInfo.outCount;
            }
            else
            {
                if (currentInning == 1)
                {
                    //UnityEngine.Debug.Log("[투수 초기화]================>> 선발투수 스태미너 초기화");
                    allowRun[1 - curIndex] = 0;
                    startInning[1 - curIndex] = 10;
                }
            }

            
            gameInfo.allowChulu = 0;
            gameInfo.conHit = gameInfo.conHR = gameInfo.conRun = 0;

            setStaminaTotalUpdate();
        }


        /// <summary>
        /// 투수교체를 체크한다
        /// 매 battingPlaying()루프가 시작되기전에 체크
        /// 조건을 만족하면 투수교체를 한후 타석을 시작
        /// </summary>
        private void checkChangePitcher()
        {
            curStamina = pitcher.getCurrentStamina();
            if (bPitcherChangeFlag == true)
            {
                checkPitcherChanged();
                //UnityEngine.Debug.Log("[투수교체 체크]&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&>>투수교체를 체크해본다");
                bPitcherChangeFlag = false;

                if (bFromGame == false)
                {
                    //UnityEngine.Debug.Log("[시뮬기록]&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&>>투수피타수 현재타자 "+batter.getName());
                    pitcher.setRecord(Param.ST_TBF);
                }
            }
        }

        /// <summary>
        /// 상황에 맞는 교체 플래그를 초기화 한다.
        /// </summary>
        private void setChangeFlagInit(int team)
        {
            bLongReliefOn[team] = false;
            bChaseOn[team] = false;
            bSetupOn[team] = false;
            bSaveOn[team] = false;
        }

        /// <summary>
        /// 투수의 최종 성적을 세팅한다(승패세이브 홀드)
        /// </summary>
        //private string strWinPitcher, strLosePitcher;
        private void setPitcherResult(CPlayer player, int team)
        {
            //최후 결과에 부르는것
            if (sPlayer.GetPitcherIndex(team) == sPlayer.GetStarterIndex(team))
            {
                //UnityEngine.Debug.Log("[투수업적))==============>> 선발투수가 끝까지 던진경우 team: " + team + "   투수이름: " + player.getName());
                //완투 처리
                //UnityEngine.Debug.Log("[투수업적))==============>> 완투처리   투수이름: " + player.getName());
                player.setPitcherAchieve(Param.ST_CG, Param.P_ACHIEVE_COMPLETE);
                //완봉  처리
                if (gameInfo.run[1 - team] == 0 && gameInfo.run[team] > 0)
                {
                    //UnityEngine.Debug.Log("[투수업적))==============>> 완봉처리   투수이름: " + player.getName());                    
                    player.setPitcherAchieve(Param.ST_SHO, Param.P_ACHIEVE_COMPLETE);
                }

                //승 패 처리
                if (gameInfo.run[team] > gameInfo.run[1 - team])
                {
                    //UnityEngine.Debug.Log("[투수업적))==============>> 승리처리   투수이름: " + player.getName());
                    //strWinPitcher = "[ffde00]승리투수: " + player.getName() + "[-]";
                    player.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_COMPLETE);
                }
                else if (gameInfo.run[team] < gameInfo.run[1 - team])
                {
                    //UnityEngine.Debug.Log("[투수업적))==============>> 패배처리   투수이름: " + player.getName());
                    //strLosePitcher = "\n패전투수: " + player.getName();
                    player.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_COMPLETE);
                }
            }
            else
            {
                if (gameInfo.run[team] > gameInfo.run[1 - team])
                {
                    //UnityEngine.Debug.Log("[투수업적))==============>> team " + team + "이 승리한 경우");
                    if (winPitcherIndex[team] == -1)
                    {
                        //마지막 투수가 승리투수
                        //UnityEngine.Debug.Log("[투수업적))==============>> 마지막에 던진 투수가 승리하는 경우 // 투수이름: " + player.getName());
                        //strWinPitcher = "[ffde00]승리투수: " + player.getName() + "[-]";
                        player.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_COMPLETE);
                    }
                    else
                    {
                        //승리투수는 
                        //manager.winPitcherIndex 이놈
                        //UnityEngine.Debug.Log("[투수업적))==============>> 승리투수 // 투수이름: " + sPlayer.GetPitcher(team, winPitcherIndex[team],false).getName());
                        //strWinPitcher = "[ffde00]승리투수: " + player.getName() + "[-]";
                        sPlayer.GetPitcher(team, winPitcherIndex[team],false).setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_COMPLETE);

                        //세이브 투수는
                        if (player.getStat(Param.ST_SV) == Param.P_ACHIEVE_TRY)
                        {
                            //UnityEngine.Debug.Log("[투수업적))==============>> 세이브 조건 만족시 세이브 투수 // 투수이름: " + player.getName());
                            player.setPitcherAchieve(Param.ST_SV, Param.P_ACHIEVE_COMPLETE);
                        }
                    }
                }
                else if (gameInfo.run[team] < gameInfo.run[1 - team])
                {
                    if (losePitcherIndex[team] == -1)
                    {
                        //마지막 투수가 패전투수
                        //UnityEngine.Debug.Log("[투수업적))==============>> 마지막에 던진 투수가 패배 // 투수이름: " + player.getName());
                        //strLosePitcher = "\n패전투수: " + player.getName();
                        player.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_COMPLETE);

                        //블론 투수는
                        if (player.getStat(Param.ST_SV) == Param.P_ACHIEVE_TRY)
                        {
                            //UnityEngine.Debug.Log("[투수업적))==============>> 블론 조건 만족시 블론투수 // 투수이름: " + player.getName());
                            player.setPitcherAchieve(Param.ST_BS, Param.P_ACHIEVE_COMPLETE);
                        }
                    }
                    else
                    {
                        //manager.losePitcherIndex 이놈
                        //UnityEngine.Debug.Log("[투수업적))==============>> 패배투수 // 투수이름: " + sPlayer.GetPitcher(team, losePitcherIndex[team],false).getName());
                        //strLosePitcher = "\n패전투수: " + player.getName();
                        sPlayer.GetPitcher(team, losePitcherIndex[team],false).setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_COMPLETE);
                    }
                }

            }
        }

        private int getGuweeValue(PitchingArsenal selectType)
        {
            int value = (int)((pitcher.getBallValue(selectType)) + pitcher.getGuweeBonus());
            return PitchingMechanism.reductionByFatigue(value, fatigueStep);
        }



        //스태미너의 변화를 세팅해준다
        private void setStaminaChange()  //defualt 1
        {
            //UnityEngine.//Debug.Log("==================>>STAMINA CHANGE");
            //스태미너 변동
            pitcher.setStamina();//, batter.tujiUpActiveRate);

            //상태 토털 업데이트
            setStaminaTotalUpdate();
        }

        private void setStaminaTotalUpdate()
        {
            //체력
            fatigueStep = pitcher.getFatigueStep();
            curStamina = pitcher.getCurrentStamina();

            //핀치
            setPinchState();
            pinchStep = pitcher.getPinchState();   
        }

        //핀치 상황 초기화
        private void setPinchInit()
        {
            gameInfo.conHit = gameInfo.conHR = gameInfo.conRun = 0;
            pitcher.setPinchState(PinchStep.Normal);
            pitcher.setPinchScore(-10);
        }


        //핀치상태 세팅
        private void setPinchState()
        {
            int pinchScore = pitcher.getPinchScore();
            int scoreGab = getScoreGab(defense);
            bool bScorePosition = (bOnBase[SimulParm.SECONDBASE_INDEX] == true || bOnBase[SimulParm.THIRDBASE_INDEX] == true);

            if (pinchScore > 0)
            {
                pitcher.setPinchState(PinchStep.Pinch);
            }
            else
            {
                if (gameInfo.conHit >= 3 || gameInfo.conHR >= 2 || gameInfo.conRun >= 4)
                {
                    gameInfo.conHit = gameInfo.conHR = gameInfo.conRun = 0;
                    pitcher.setPinchScore(PitchingMechanism.PINCH_SCORE);
                    pitcher.setPinchState(PinchStep.Pinch);
                }
                else
                {            
                    pitcher.setPinchState(PinchStep.Normal);                    
                }
            }
        }

            

        //스코어 차이를 구하기 - 상태
        private int getScoreGab(int team)
        {
            return (gameInfo.run[team] - gameInfo.run[1 - team]);
        }

        //공격이 얻은 점수 - 상태
        private int getOffeseScore()
        {
            return gameInfo.run[curIndex];
        }

        //수비가 얻은 점수 - 상태
        private int getDefenseScore()
        {
            return gameInfo.run[1 - curIndex];
        }

        //잠재 허용 점수 - 상태
        private int potentialScoreLoss()
        {
            int score = 0;
            for (int i = 0; i < 3; i++)
            {
                if (bOnBase[i] == true) score++;
            }
            return score;
        }

        //현재투수가 던진 이닝 (1이닝이 10단위, 아웃이 1단위)
        private int getPitchedInning()
        {
            return (currentInning * 10 + gameInfo.outCount) - startInning[1 - curIndex];
        }

        //현재점수에 따라 승리투수 패배투수의 인덱스를 정한다
        private void checkWinLoseIndex()
        {
            ////UnityEngine.//Debug.Log("==============>>checkWinLoseIndex======================>>gameInfo.run[MYTEAM] :" + gameInfo.run[MYTEAM] + " VS gameInfo.run[CPUTEAM] : " + gameInfo.run[CPUTEAM]);
            if (gameInfo.run[MYTEAM] > gameInfo.run[CPUTEAM])
            {
                //Player가 승리시
                winPitcherIndex[CPUTEAM] = -1;
                losePitcherIndex[MYTEAM] = -1;

                if (winPitcherIndex[MYTEAM] == -1)
                {
                    //myTeam 승리투수 조건 이 없은 경우
                    winPitcherIndex[MYTEAM] = sPlayer.GetPitcherIndex(MYTEAM);
                }
                if (losePitcherIndex[CPUTEAM] == -1)
                {
                    //cpuTeam 패배투수 조건이 없는 경우
                    losePitcherIndex[CPUTEAM] = sPlayer.GetPitcherIndex(CPUTEAM);
                }

            }
            else if (gameInfo.run[MYTEAM] < gameInfo.run[CPUTEAM])
            {
                winPitcherIndex[MYTEAM] = -1;
                losePitcherIndex[CPUTEAM] = -1;

                if (winPitcherIndex[CPUTEAM] == -1)
                {
                    //cpuTeam 승리투수 조건 이 없은 경우
                    winPitcherIndex[CPUTEAM] = sPlayer.GetPitcherIndex(CPUTEAM);
                }
                if (losePitcherIndex[MYTEAM] == -1)
                {
                    //myTeam 패배투수 조건이 없는 경우                
                    losePitcherIndex[MYTEAM] = sPlayer.GetPitcherIndex(MYTEAM);
                }
            }
            else
            {
                winPitcherIndex[MYTEAM] = winPitcherIndex[CPUTEAM] = -1;
                losePitcherIndex[MYTEAM] = losePitcherIndex[CPUTEAM] = -1;
            }

            ////UnityEngine.//Debug.Log("==============>>winPitcherIndex  MYTEAM : " + winPitcherIndex[0] + "  CPUTEAM :" + winPitcherIndex[1]);
            ////UnityEngine.//Debug.Log("==============>>losePitcherIndex  MYTEAM : " + losePitcherIndex[0] + "  CPUTEAM :" + losePitcherIndex[1]);
        }


        /////////////////////////////////////////////////////////////////////////
        // 타자관련 초기화 및 세팅
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 타자를 초기화 한다
        /// 이 함수는 타자 전환시 혹은 타자 교체시 호출된다.
        /// 타자를 초기화 하면서 동시에 타자주자도 생성한다.
        /// </summary>
        private void initBatter()
        {
            //다음타석시 스킬 카운트 초기화
            initSkillCountEveryBatting();
            curBattingSkillInfo.Clear();

            ////Debug.Log("==============>>타자 초기화");

            gameInfo.strikeCount = gameInfo.ballCount = 0;
            batter = sPlayer.GetBatter(curIndex);

#if _Test_Local
            /*
            //지워지워
            List<SkillIndex> tempList = batter.getSkillList();
            if (tempList.Count > 0)
            {
                for (int i = 0; i < tempList.Count;i++ )
                {
                    //Debug.Log("==========>>현재 타자에게 걸려있는 스킬  " + SimulParm.GetSkillInfo(tempList[i]).skillName);
                }
            }*/
#endif
            ///////////////////////////////////////////////////////////////////////////////////
            ////Debug.Log("=================================>> 타석마다 초기화 되어야 하는 값들 초기화");
            //매우중요
            //타자의 각종 보너스 밸류 초기화
            batter.setBonusValue(0);    //기본 보너스 초기화
            batter.setDebuffValue(0);   //디버프 밸류 초기화
            //투수의 각종 보너스 밸류 초기화
            pitcher.setDebuffValue(0);  //투수의 디버프 밸류 초기화
            ///////////////////////////////////////////////////////////////////////////////////

            ////UnityEngine.//Debug.Log("================>>팀 "+curIndex + "   ====>>InitBatter: " + batter.getName());
            SimulRunner _runner = new SimulRunner();
            int index = getAvailableIndex();
            if (index != -1)
            {
                curRunnerIndex = index;
                //UnityEngine.Debug.Log("시뮬엔진================>>curRunnerIndex = " + curRunnerIndex);
                hitterRunnerIndex = index;
                _runner.makeHitterRunner(batter, index);
                _runner.lineup = sPlayer.GetLineupCount(curIndex);
                runnerActive[index] = true;
                runner[index] = _runner;
                runnerValue[index] = (int)RunnerState.OnBase;
                runnerCurPos[index] = SimulParm.HOMEBASE_INDEX;
            }
            else
            {
                //UnityEngine.Debug.Log("[Error]================>>타자주자 세팅 실패");
            }

            //SimulManager.AddGameSummuryInfo("\n\n" + (sPlayer.GetLineupCount(curIndex) + 1) + "번타자: [63b7ff]" + batter.getName() + "[-]");

            runnerStealState = SimulStealState.NONE;
            stealPitchNum = 0;// UnityEngine.Random.Range(0, 2);

            //번트 체크
            checkBuntTry();

            bPitcherChangeFlag = true;
            bErrorHappen = false;
            bMeahogSkillInvalidity = false;
        }

        /// <summary>
        /// 메인 루프에서 한타석이 종료되면 호출된다.
        /// initBatter를 호출하고 
        /// 타순을 바꾸며 그에 따른 상태값을 초기화 한다.
        /// </summary>
        private bool vsSkillType = false;
        private void nextBatter(bool bChangeCheck)
        {
            ////UnityEngine.//Debug.Log("========================================>>시뮬메니저 nextBatter " + batter.getName() + "strBatterResult = " + strBatterResult);
            //타자 상대에 따른 체력 감소
            setStaminaChange();

            if (strBatterResult != null)
            {
                ////UnityEngine.//Debug.Log("========================================>>타격결과");
                //SimulManager.AddGameSummuryInfo("\n[ffde00]" + batter.getName() + ": " + strBatterResult + "[-]");
                strBatterResult = null;
            }

            if (checkGoodByeGame() == true)
            {
                //SimulManager.AddGameSummuryInfo("[ff3a3a] (끝내기)[-]");                
                bGameEnd = true;
                bInningEnd = true;
                bBattingEnd = true;
                return;
            }

            
            if (bStealThreeOut == false)
            {
                //도루자로 인한 공수교대시 라인업 카운트를 하지 않는다.
                sPlayer.SetLineupCount(curIndex);
            }
            bStealThreeOut = false;

            if (bInningEnd == false)
            {
                bOnBase[0] = bOnBase[1] = bOnBase[2] = bOnBase[3] = false;
                shiftBonus = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (runnerActive[i] == true)
                    {
                        if (runner[i] != null)
                        {
                            int curBase = runner[i].curPos;
                            bOnBase[curBase] = true;
                            runner[i].lastPos = curBase;
                            if (runner[i].bHitterRunner == false) shiftBonus += 50;
                        }
                    }
                }

                bOnBase[SimulParm.HOMEBASE_INDEX] = false;

                if (bChangeCheck == true)
                {
                    checkChangeRunner();
                    checkChangeBatter();
                }

                initBatter();
            }
            gameInfo.pitchNum[1 - curIndex] += pitchedNum;
            pitchedNum = 0;
            gameInfo.initCount(false);

            //새타자 한후 투타 스킬
            vsSkillType = setBattingviewSkill();

        }

        /// <summary>
        /// vs 스킬인경우 true를 리턴한다
        /// </summary>
        /// <returns></returns>
        private bool setBattingviewSkill()
        {
            skillConditionState(null);
            if (checkBattingviewSkill() == true)
            {
                //버퍼저장
                vsSkillBuffer[0] = (int)curBatterSkill.ID;// .effectIndex;
                vsSkillBuffer[1] = (int)curBatterSkill.rank;
                vsSkillBuffer[2] = (int)curPitcherSkill.ID;//.effectIndex;
                vsSkillBuffer[3] = (int)curPitcherSkill.rank;
                //카운터 스킬 발동시
                if (SimulParm.checkOffenseSkillWin(curBatterSkill.rank, curPitcherSkill.rank) == true)
                {
                    //타자승리                    
                    curPitcherSkill = null; //투수 스킬 무효화
                }
                else
                {
                    //투수승리
                    curBatterSkill = null; //타자 스킬 무효화
                }
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// 타자 교체가 필요한지를 체크한다
        /// </summary>
        private bool checkChangeBatter()
        {
            //UnityEngine.Debug.Log("[시뮬] ========================>> 타자교체 체크");
            if (checkBatterChanged() == true)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 번트 작전이 나왔는지 여부를 체크
        /// </summary>
        int buntTryCount = 0;
        private SimulBuntType checkBuntTry()
        {
            buntTryCount++;
            //UnityEngine.Debug.Log("[시뮬] ========================>> checkBuntTry");
            buntResultType = SpecificBuntType.NONE;
            buntType = SimulBuntType.NONE;
            buntSuccessType = SimulBuntType.NONE;


            if (bNoRunner && batter.checkSkillInvoke(SkillIndex.GodOfBunt) == true)
            {
                //번트신공
                buntType = SimulBuntType.DRAG;
                buntResultType = SpecificBuntType.DRAG_SUCCESS;
            }
            else ////if (buntTryCount > 1)
            {
                //조건
                int scoreGab = getScoreGab(curIndex);

                //번트 결과 초기화
                SpecificBuntType result = SpecificBuntType.NONE;
                
                //스퀴즈 체크
                result = SimulBunt.getSqueezeResult(gameInfo.outCount, scoreGab, bOnBase, pitcher, batter);
                if (result != SpecificBuntType.NONE)
                {
                    //스퀴즈
                    buntType = SimulBuntType.SQUEEZE;
                    buntResultType = result;
                    if (buntResultType == SpecificBuntType.SQUEEZ_SUCCESS || buntResultType == SpecificBuntType.SQUEEZ_FIELDER_CHOICE)
                    {
                        buntSuccessType = buntType;
                    }
                }
                else
                {
                    //희생번트 체크                                       
                    result = SimulBunt.getSacResult(gameInfo.outCount, scoreGab, bOnBase, pitcher, batter);
                    if (result != SpecificBuntType.NONE)
                    {
                        //희생번트
                        buntType = SimulBuntType.SACRIFY;
                        buntResultType = result;
                        if (buntResultType == SpecificBuntType.SAC_SUCCESS || buntResultType == SpecificBuntType.SAC_FIELDER_CHOICE)
                        {
                            buntSuccessType = buntType;
                        }
                    }
                    else
                    {
                        //드래그번트
                        result = SpecificBuntType.NONE;// SimulBunt.getDragBuntResult(scoreGab, bOnBase, pitcher, batter);
                        if (result != SpecificBuntType.NONE)
                        {
                            //UnityEngine.Debug.Log("[시뮬] ========================>> 드래그번트 체크");
                            buntType = SimulBuntType.DRAG;
                            buntResultType = result;
                        }
                    }
                }

                if (buntType != SimulBuntType.NONE)
                {
                    //UnityEngine.Debug.Log("[Error]================>>번트 작전시 도루를 하지 않는다는 의미");
                    stealPitchNum = 1000;
                }
            }

            return buntType;
        }



        /////////////////////////////////////////////////////////////////////////
        // 주자관련 초기화 및 세팅
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 베이스 상태를 초기화 한다.
        /// </summary>
        private void initRunnerBase()	//InitIning에서 호출
        {
            for (int i = 0; i < 4; i++)
            {
                runnerActive[i] = false;
                bOnBase[i] = false;			//	        
                runnerValue[i] = (int)RunnerState.None;
                runnerCurPos[i] = -1;
            }
        }



        /// <summary>
        /// 주자 교체가 필요한지를 체크한다
        /// </summary>
        private bool checkChangeRunner()
        {
            /*
            changerIndex[(int)ChangerIndex.InRunner] = -1; // firstBase = secondBase = -1;
            bool bChange = false;
            for (int i = 0; i < 2; i++)
            {
                if (runnerChangeNeedCheck(i) == true)
                {
                    bChange = true;
                }
            }
            return bChange;*/

            //무조건 안되게 만든다 현재는...
            return false;
        }

        //해당 인덱스의 주자를 제거한다
        private void removeRunner(int index, RunnerState stateValue, int curPosIndex) //stateValue = RunnerState.None , curPosIndex = -1
        {
            ////UnityEngine.//Debug.Log("===========================>>RemoveRunner Index: " + index);
            if (index < 0) return;
            runnerValue[index] = (int)stateValue;
            runnerCurPos[index] = (curPosIndex == -1 ? runner[index].dstPos : curPosIndex);//  (bHomeIn == true ? SimulParm.HOMEBASE_INDEX : runner[index].dstPos);
            runnerActive[index] = false;
            runner[index].setEmpty();
        }

        /// <summary>
        /// 사용 가능한 주자의 슬롯을 리턴해준다.
        /// -1을 리턴시 사용가능한 슬롯이 없다 - 이것은 즉 오류
        /// </summary>
        private int getAvailableIndex()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == false)
                {
                    return i;
                }
            }

            //bGameEnd = true;
            //UnityEngine.Debug.Log("[Error]================>>해당 주자 인덱스 슬롯이 없음 중대버그임!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            return -1; //DEBUG 용
        }

        //해당 베이스에 있는 주자의 array index(0~3값)
        private int getRunnerIndex(int basePos)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].curPos == basePos) return i;
                }
            }
            return -1;
        }

        //해당 인덱스로 향하는 주자의 array index값을 얻어온다
        private int getRunnerDestIndex(int dstPos)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].dstPos == dstPos) return i;
                }
            }
            return -1;
        }

        //해당 베이스의 러너 정보를 가져옴
        private SimulRunner getRunner(int _base)
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].curPos == _base)
                    {
                        return runner[i];
                    }
                }
            }
            return null;
        }

        //현재 야수(fIndex)가 특정루(throwBase)로 던지는 경우 자살 야수 인덱스를 얻어온다
        private int getPutOutIndex(int throwBase, int fIndex)
        {
            if (throwBase == SimulParm.FIRSTBASE_INDEX)
            {
                return CPlayer._FIRSTBASEMAN;
            }
            else if (throwBase == SimulParm.THIRDBASE_INDEX)
            {
                return CPlayer._THIRDBASEMAN;
            }
            else if (throwBase == SimulParm.HOMEBASE_INDEX)
            {
                return CPlayer._CATCHER;
            }
            else
            {
                if (fIndex == CPlayer._THIRDBASEMAN || fIndex == CPlayer._SHORTSTOP)
                    return CPlayer._SECONDBASEMAN;
                else
                    return CPlayer._SHORTSTOP;
            }
        }

        /// <summary>
        /// SimulResultState(배팅결과) 상태에 따라 타자주자와 루상주자가 얼만큼의 기대 베이스를 얻을 수 있는지를 얻어오는 함수
        /// bOneMoreBaseFlag,bGrounderFlag,bOneMoreBaseFlag 플래그를 상태에 따라 on 해줌으로서 루상 주자 능력치에 따른 추가 베이스를 얻어올 수 있다.
        /// </summary>
        private int getAddBase(SimulResultState state)
        {
            int addBase = -1;
            bGrounderFlag = false;
            bFourballFlag = false;
            bBaseTagFlag = false;
            bOneMoreBaseFlag = false;   //이 플래그가 on인 경우 보너스 베이스 활성화
            if (state == SimulResultState.Single ||
                state == SimulResultState.CatchError ||
                state == SimulResultState.BoundError)
            {
                addBase = 1;
                bOneMoreBaseFlag = true;    //한베이스를 더갈 수 있는 여지 - 루상 주자에만 해당
            }
            else if (state == SimulResultState.InfieldSingle ||
                     state == SimulResultState.BuntSingle ||
                     state == SimulResultState.InfieldTurboSingle)
            {
                addBase = 1;
                bOneMoreBaseFlag = true;    //한베이스를 더갈 수 있는 여지 - 루상 주자에만 해당
            }
            else if (state == SimulResultState.Double ||
                     state == SimulResultState.SingleOneError ||
                     state == SimulResultState.ThrowError)
            {
                addBase = 2;
                bOneMoreBaseFlag = true;    //한베이스를 더갈 수 있는 여지 - 루상 주자에만 해당
            }
            else if (state == SimulResultState.Triple || state == SimulResultState.DoubleOneError)
            {
                addBase = 3;
            }
            else if (state == SimulResultState.HomeRun || state == SimulResultState.TripleOneError)
            {
                addBase = 4;
            }
            else if (state == SimulResultState.FourBall)
            {
                addBase = 0;
                bFourballFlag = true;
            }
            else if (state == SimulResultState.Grounder)
            {
                ////UnityEngine.//Debug.Log("=========================>>bGrounderFlag 발생");
                addBase = 0;
                bGrounderFlag = true;     //병살을 당할 수 있는 여지
            }
            else if (state == SimulResultState.FlyOut)
            {
                addBase = 0;
                if (flyType == SpecificFlyType.OutfieldOverHead || flyType == SpecificFlyType.OutfieldHighFly)
                {
                    bBaseTagFlag = true;        //베이스택을 할 수 있는 여지
                }
            }
            else if (state == SimulResultState.LineOut)
            {
                addBase = 0;
            }
            else if (state == SimulResultState.FielderChoice)
            {
                addBase = 1;
            }
            return addBase;
        }

        /// <summary>
        /// *** 시뮬레이션 주루 플레이의 핵심 ***
        /// 각 루(baseIndex)에 존재하는 주자가 getAddBase()로 부터 얻어온 추가 베이스를 통해 얼마만큼 이동하고
        /// 이동함으로서 발생되는 기록(득점,자살,보살,기타등등)들을 컨트롤 해주는 함수
        /// 능력치에 따라 주루사가 발생할 수 있는데 발생하는 outCount를 리턴해준다
        /// </summary>
        private int moveRunner(int baseIndex, int addBase, int currentRunnerIndex)
        {
            int index = currentRunnerIndex;// getRunnerIndex(baseIndex);
            int curAddBase = addBase;

            if (index == -1) return 0;

            int nextBase;
            int bounsBase = 0; //주자 능력치에 따른 추가 베이스

            CPlayer curFielder = fielder[fIndex].getFielder();
            CPlayer curRunner = runner[index].getRunner();

            if (bBaseTagFlag == true)
            {
                curAddBase = 0;
                if (baseIndex == SimulParm.THIRDBASE_INDEX || baseIndex == SimulParm.SECONDBASE_INDEX)
                {
                    //베이스택 처리
                    int oneMoreBaseValue = runner[index].checkBaseTag(curFielder, flyCatchType);
                    
                    if (oneMoreBaseValue != 0)
                    {
                        //베이스텍 발생시 레이저 스킬 체크
                        if (curFielder.fieldSkillSuccess(SkillIndex.Laser) == true)
                        {
                            //Debug.Log("=======================>> 언더베이스 레이저로 잡음");
                            oneMoreBaseValue = -1;
                            if (bAutoMode == true)
                            {
                                CSkill fielderSkill = curFielder.getSkillValue(SkillIndex.Laser);
                                setUseSkillFlag(SkillUseStep.Fielding, null, fielderSkill, null, VsResult.None);
                            }
                        }
                    }

                    if (oneMoreBaseValue == 1)
                    {
                        //UnityEngine.Debug.Log("[주루상황]===================>[" + batter.getName() + "] 타석시 [" + curRunner.getName() + "] 베이스택 성공");
                        curAddBase = 1;     //한베이스 추가
                    }
                    else if (oneMoreBaseValue == -1)
                    {
                        //자살 익덱스 추가해줘~~
                        int poIndex = (baseIndex == SimulParm.THIRDBASE_INDEX ? CPlayer._CATCHER : CPlayer._THIRDBASEMAN);
                        //주루사
                        gameInfo.setRunnerOut(curRunner, fielder[poIndex].getFielder(), curFielder,false); //SimulFieldState.BaseRunningOut, 

                        //UnityEngine.Debug.Log("[주루상황]===================>[" + batter.getName() + "] 타석시 [" + curRunner.getName() + "] 베이스택 하다가 주루사 outCount :" + gameInfo.outCount + "향하고 있던 베이스 " + (baseIndex + 1));
                        removeRunner(index, RunnerState.AssistOutHB, (baseIndex + 1));

                        //SimulManager.AddGameSummuryInfo("\n-" + (baseIndex + 1) + "루주자 " + runner[index].getRunner().getName() + ": " + (baseIndex == SimulParm.THIRDBASE_INDEX ? "홈" : "3루") + "에서 아웃");

                        return 1;   //아웃카운트 하나 추가
                    }
                    else
                    {
                        //UnityEngine.Debug.Log("[주루상황]===================>[" + batter.getName() + "] 타석시 [" + curRunner.getName() + "] 베이스택을 포기함");
                    }
                }
            }
            else if (bFourballFlag == true)
            {
                if (baseIndex == SimulParm.THIRDBASE_INDEX)
                {
                    if(getRunnerIndex(SimulParm.SECONDBASE_INDEX) != -1 && getRunnerIndex(SimulParm.FIRSTBASE_INDEX) != -1)
                    {
                        curAddBase = 1;
                    }
                    else
                    {
                        curAddBase = 0;
                    }
                }
                else if (baseIndex == SimulParm.SECONDBASE_INDEX)
                {
                    if (getRunnerIndex(SimulParm.FIRSTBASE_INDEX) != -1)
                    {
                        curAddBase = 1;
                    }
                    else
                    {
                        curAddBase = 0;
                    }
                }
                else
                {
                    curAddBase = 1;
                }
            }
            else if (bGrounderFlag == true)
            {
                //병살 처리
                //type 1: 진루
                //type 2: 야선
                //type 3: 병살 
                ////UnityEngine.//Debug.Log("==================================>>moverRunner bGrounderFalg!!!!!!!!!!!!!!");
                ////UnityEngine.//Debug.Log("==================================>>runner[index].dstPos = " + runner[index].dstPos);
                ////UnityEngine.//Debug.Log("==================================>>grounderThrowBase = " + grounderThrowBase);

                if (runner[index].dstPos == grounderThrowBase)
                {
                    //if (grounderThrowBase == SimulParm.FIRSTBASE_INDEX) UnityEngine.Debug.Log("[주루상황]===================>[" + curRunner.getName() + "] 그라운더 의해서 아웃됨");
                    //else UnityEngine.Debug.Log("[주루상황]===================>[" + curRunner.getName() + "] 야수 선택에 의해서 아웃됨");

                    //자살 보살 카운트는 미리 해뒀음
                    removeRunner(index, RunnerState.FourceOut, grounderThrowBase);
                    int poIndex = getPutOutIndex(grounderThrowBase, fIndex);
                    //UnityEngine.Debug.Log("[주루상황]===================>[보살: " + fielder[fIndex].getName() + "] , [자살: " + fielder[poIndex].getName());
                    gameInfo.setFieldOut(SimulResultState.Grounder, hitType, null, null, fielder[poIndex].getFielder(), fielder[fIndex].getFielder(), false);

                    if (grounderThrowBase != SimulParm.FIRSTBASE_INDEX)
                    {
                        //UnityEngine.Debug.Log("[주루상황]=============>>야수가 그라운더시 1루가 아닌 다를 루를 선택시 타자주자 능력치 감안 병살 가능성 체크");
                        
                        if (hitType == SimulHitType.Bunt)
                        {
                            if (buntResultType != SpecificBuntType.SAC_DOUBLE_PLAY)
                            {
                                //UnityEngine.Debug.Log("[번트 예외]=====================================>> 단순번트 실패시 더블플레이는 없다!!!");
                                grounderFiedingValue = 0;
                            }
                        }

                        //병살저지 스킬세팅
                        bool bDpBreak = runner[index].skillAvailable(SkillIndex.RunnerDoublePlayBreaker);
                        if (bDpBreak == false)
                        {
                            //UnityEngine.Debug.Log("[스킬발동]=============>>더블플레이 체크");
                            if (runner[hitterRunnerIndex].checkDoublePlay(grounderFiedingValue - 1000) == true)
                            {
                                //SimulManager.AddGameSummuryInfo("\n-" + (runner[index].lastPos + 1) + "루주자 " + runner[index].getRunner().getName() + ": " + (grounderThrowBase == SimulParm.HOMEBASE_INDEX ? "홈" : "2루") + "에서 아웃");
                                //UnityEngine.Debug.Log("[스킬발동]=============>>병살 성공");
                                gameInfo.addDoublePlayCount(curIndex, runner[hitterRunnerIndex].getRunner());
                                //fIndex = poIndex;
                                grounderThrowBase = SimulParm.FIRSTBASE_INDEX;
                                doublePlaySuccess = true;                                
                                strBatterResult = Util.getBatterResult(fIndex, true, 0, "병살"); //땅볼을 병살로 바꿈
                                return 1;   //아웃카운트 하나 추가
                            }
                        }
                    }
                }
                else
                {
                    //진루 상황 발생
                    //UnityEngine.Debug.Log("[주루상황]===================>[" + curRunner.getName() + "] 야수 선택에 의한 진루");
                    curAddBase = 1;
                }

            }

            //기타 처리
            if (curAddBase >= 1)
            {
                if (baseIndex == SimulParm.HOMEBASE_INDEX)
                {
                    //타자 주자
                    nextBase = -1 + curAddBase; //다음 도착 베이스
                }
                else
                {
                    //루상 주자
                    nextBase = baseIndex + curAddBase + bounsBase;//다음 도착 베이스

                    if (runner[index].bHitterRunner == false)
                    {
                        if (bOneMoreBaseFlag == true)
                        {
                            //한베이스 더 체크(시뮬 전용)
                            SimulOverrunState oneMoreBaseValue = SimulBaseRunning.checkGetOneMoreBaseSimul(runner[index].getRunner(), curFielder, flyCatchType);

                            if (bAutoMode == true)
                            {
                                if (oneMoreBaseValue == SimulOverrunState.LaserOut || oneMoreBaseValue == SimulOverrunState.VsOut || oneMoreBaseValue == SimulOverrunState.VsSafe)
                                {
                                    //Debug.Log("=======================>> 한베이스 더 진루시 레이저로 잡거나 vs 발동");
                                    CSkill fielderSkill = curFielder.getSkillValue(SkillIndex.Laser);
                                    CSkill runnerSkill = null;
                                    VsResult vsType = VsResult.None;
                                    if (oneMoreBaseValue != SimulOverrunState.LaserOut)
                                    {
                                        //레이저 단독이 아니고 vs로 가는 경우
                                        runnerSkill = runner[index].getRunner().getSkillValue(SkillIndex.RunnerSliding);
                                        vsType = oneMoreBaseValue == SimulOverrunState.VsOut ? VsResult.DefenseWin : VsResult.OffenseWin;
                                    }
                                    setUseSkillFlag(SkillUseStep.Fielding, runnerSkill, fielderSkill, null, vsType);
                                }                                
                            }

                            if (oneMoreBaseValue == SimulOverrunState.SAFE || oneMoreBaseValue == SimulOverrunState.VsSafe)
                            {
                                //오버런 해서 사는 경우
                                //UnityEngine.Debug.Log("[주루상황]===================>[" + batter.getName() + "] 타석시 [" + curRunner.getName() + "] 한베이스 더 진행 가능");
                                nextBase += 1;
                            }
                            else if (oneMoreBaseValue == SimulOverrunState.OUT || oneMoreBaseValue == SimulOverrunState.VsOut || oneMoreBaseValue == SimulOverrunState.LaserOut)
                            {
                                //오버런 해서 죽는 경우
                                if (index != hitterRunnerIndex)
                                {
                                    if (bRunnerOutFlag == false)
                                    {
                                        int outBase = nextBase + 1;
                                        if (outBase >= SimulParm.HOMEBASE_INDEX) outBase = SimulParm.HOMEBASE_INDEX;
                                        if (outBase == SimulParm.HOMEBASE_INDEX || bOnBase[outBase] == false)
                                        {
                                            bRunnerOutFlag = true;  //4케이스중 한번만 발동 가능
                                            //자살 익덱스 추가해줘~~
                                            int poIndex = CPlayer._CATCHER;
                                            if (outBase == SimulParm.SECONDBASE_INDEX) poIndex = CPlayer._SECONDBASEMAN;
                                            else if (outBase == SimulParm.THIRDBASE_INDEX) poIndex = CPlayer._THIRDBASEMAN;
                                            //주루사
                                            gameInfo.setRunnerOut(curRunner, fielder[poIndex].getFielder(), curFielder, false); //SimulFieldState.BaseRunningOut, 

                                            //UnityEngine.Debug.Log("[주루상황]===================>[" + batter.getName() + "] 타석시 [" + curRunner.getName() + "] 한베이스 더 가다가 주루사 outCount :" + gameInfo.outCount + "향하고 있던 베이스 : " + (nextBase + 1));
                                            removeRunner(index, (outBase == SimulParm.HOMEBASE_INDEX ? RunnerState.AssistOutHB : RunnerState.AssistOut3B), outBase);

                                            //SimulManager.AddGameSummuryInfo("\n-" + (baseIndex + 1) + "루주자 " + runner[index].getRunner().getName() + ": " + (outBase == SimulParm.HOMEBASE_INDEX ? "홈" : (nextBase + 1) + "루") + "에서 아웃");

                                            runnerRunningOut[index] = outBase;

                                            return 1;   //아웃카운트 하나 추가
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                if (nextBase >= SimulParm.HOMEBASE_INDEX)
                {
                    nextBase = SimulParm.HOMEBASE_INDEX;
                }
                else
                {
                    if (nextBase >= SimulParm.FIRSTBASE_INDEX)
                    {
                        if (bOnBase[nextBase] == true)
                        {
                            //도착예정 베이스에 주자가 있는 경우 하나 차감
                            nextBase -= 1;
                        }
                    }
                }

                bool bAriveHome = runner[index].setArriveBase(nextBase);
                if (bAriveHome == true)
                {
                    runner[index].bAddScore = true;
                }
                else
                {
                    //UnityEngine.Debug.Log("[주루상황]====================>>[" + curRunner.getName() + "주자 " + (nextBase + 1) + "루로 진루");
                    /*
                    if (index != hitterRunnerIndex)
                    {
                        if (nextBase == SimulParm.SECONDBASE_INDEX || nextBase == SimulParm.THIRDBASE_INDEX)
                        {
                            if (nextBase > runner[index].lastPos)
                            {
                                SimulManager.AddGameSummuryInfo("\n-" + (runner[index].lastPos + 1) + "루주자 " + curRunner.getName() + ": " + (nextBase + 1) + "루까지 진루");
                            }
                        }
                    }*/
                    bOnBase[nextBase] = true;
                    runnerCurPos[index] = nextBase;
                }
            }

            return 0;
        }

        //득점 후 처리
        private void addScore(int index)
        {
            CPlayer curRunner = runner[index].getRunner();
            //UnityEngine.Debug.Log("[주루상황]====================>>[" + curRunner.getName() + "득점 성공");
            //int lastPosition = (runner[index].lastPos + 1);
            //SimulManager.AddGameSummuryInfo("\n-" + (lastPosition == 4 ? "타자주자 " : (lastPosition + "루주자 ")) + curRunner.getName() + ": [ff3a3a]홈인[-]");
            //득점처리
            bool bErrorFlag = false;
            bool bRbiFlag = true;

            if (outCountIF >= 3)
            {
                //UnityEngine.Debug.Log("[투수자책판단]====================>>투아웃 에러 상태임으로 투수 [" + pitcher.getName() + "]은 비자책");
                bErrorFlag = true;
            }
            else if (runner[index].bErrorRunner == true)
            {
                //UnityEngine.Debug.Log("[투수자책판단]====================>>[" + curRunner.getName() + "]는 에러의 이득을 본 주자임으로 투수 [" + pitcher.getName() + "]은 비자책");
                bErrorFlag = true;
            }

            if (bErrorHappen == true)
            {
                //UnityEngine.Debug.Log("[투수자책판단]====================>>[" + batter.getName() + "]는 에러에 의한 출루임으로 타점 무효 /  투수 [" + pitcher.getName() + "]은 비자책");
                bErrorFlag = true;
                bRbiFlag = false;
            }

            allowRun[1 - curIndex]++;
            gameInfo.addRun(curIndex, currentInning, curRunner, batter, (runner[index].bLastPitcher == true ? lastPitcher : pitcher), bErrorFlag, bRbiFlag);
            removeRunner(index, RunnerState.Score, SimulParm.HOMEBASE_INDEX);

            //투수 승패 처리
            checkWinLoseIndex();
        }


        /// <summary>
        /// 도루 상황을 체크한다.
        /// </summary>
        /// 
        SimulRunner stealRunner;
        float stealSuccessRate;
        private bool checkStealBase()
        {
            if (gameInfo.outCount < 2)
            {
                if (stealPitchNum == pitchedNum)
                {
                    if (getRunnerIndex(SimulParm.FIRSTBASE_INDEX) != -1 //1루에 주자가 있고
                     && getRunnerIndex(SimulParm.SECONDBASE_INDEX) == -1)
                    {
                        stealRunner = runner[getRunnerIndex(SimulParm.FIRSTBASE_INDEX)];
                        int scoreGab = getScoreGab(curIndex);
                        CPlayer catcher = fielder[CPlayer._CATCHER].getFielder();
                        if (SimulSteal.checkStealPossible(stealRunner.runner, catcher, currentInning, gameInfo.outCount, scoreGab, bOnBase) == true)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;

        }


        //도루의 결과에 따른 상태를 세팅해준다
        private bool setSteal(SimulStealState stealState)
        {
            int moveRunner = getRunnerIndex(SimulParm.FIRSTBASE_INDEX);

            //견제
            if (stealState == SimulStealState.NONE || stealState == SimulStealState.PickOffVsSafe)
            {
                //견제했는데 안죽은 경우
                return false;
            }
            else if (stealState == SimulStealState.PickOffOut || stealState == SimulStealState.PickOffLaserOut || stealState == SimulStealState.PickOffVsOut)
            {                
                //견제로 죽은 경우
                pickoffCount++;
                gameInfo.setRunnerOut(runner[moveRunner].getRunner(), fielder[CPlayer._FIRSTBASEMAN].getFielder(), fielder[CPlayer._PITCHER].getFielder(), false); //SimulFieldState.StealOut, 
                removeRunner(moveRunner, RunnerState.RunningOut, SimulParm.SECONDBASE_INDEX);
                //SimulManager.AddGameSummuryInfo("\n-" + (stealRunner.lastPos + 1) + "루주자 " + stealRunner.getRunner().getName() + ": " + (stealRunner.lastPos + 2) + "루 도루 실패");
                return gameInfo.checkOut(pitcher);
            }
            else if (stealState == SimulStealState.Fail || stealState == SimulStealState.Fail_Skill || stealState == SimulStealState.VsSkill_CatcherWin)
            {
                bStealTry = true;                        
                //도루실패 케이스
                bStealSuccess = false;
                int poIndex = (batter.getHitHand() == CPlayer._RIGHTHAND ? CPlayer._SECONDBASEMAN : CPlayer._SHORTSTOP);
                //runnerValue[moveRunner] = RunnerState.RunningOut;
                gameInfo.setRunnerOut(runner[moveRunner].getRunner(), fielder[poIndex].getFielder(), fielder[CPlayer._CATCHER].getFielder(), true); //SimulFieldState.StealOut, 
                removeRunner(moveRunner, RunnerState.RunningOut, SimulParm.SECONDBASE_INDEX);
                //SimulManager.AddGameSummuryInfo("\n-" + (stealRunner.lastPos + 1) + "루주자 " + stealRunner.getRunner().getName() + ": " + (stealRunner.lastPos + 2) + "루 도루 실패");
                return gameInfo.checkOut(pitcher);
            }
            else if (stealState == SimulStealState.Success || stealState == SimulStealState.Success_Skill || stealState == SimulStealState.VsSkill_RunnerWin)
            {
                bStealTry = true;                        
                //도루성공 케이스
                bStealSuccess = true;
                gameInfo.addSteal(curIndex, true, runner[moveRunner].getRunner(), fielder[CPlayer._CATCHER].getFielder());
                runner[moveRunner].setArriveBase(SimulParm.SECONDBASE_INDEX);
                bOnBase[SimulParm.SECONDBASE_INDEX] = true;
                bOnBase[SimulParm.FIRSTBASE_INDEX] = false;
                runnerCurPos[moveRunner] = SimulParm.SECONDBASE_INDEX;
                //SimulManager.AddGameSummuryInfo("\n-" + (stealRunner.lastPos + 1) + "루주자 " + stealRunner.getRunner().getName() + ": " + (stealRunner.lastPos + 2) + "루 도루 성공");
                stealRunner.lastPos = stealRunner.curPos;
            }

            return false;
        }

        /////////////////////////////////////////////////////////////////////////
        // 필딩관련 초기화 및 세팅
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 야수를 초기화 시킨다.
        /// </summary>
        private void initFielder(bool bChangeCheck)
        {
            int team = 1 - curIndex;
            for (int i = 0; i < SimulPlayer.NUM_LINEUP; i++)
            {
                int pos = sPlayer.GetCurPosition(team, i);
                if (pos != CPlayer._DH)
                {
                    if (pos == CPlayer._PITCHER)
                    {
                        //fielder[pos] = pitcher;
                        fielder[pos].setFielder(pitcher);
                    }
                    else
                    {
                        //fielder[pos] = sPlayer.GetFielder(team, i);
                        fielder[pos].setFielder(sPlayer.GetFielder(team, i, false));
                        fielder[pos].fielder.setFieldBounsValue(0); //보너스 밸류 초기화
                        //미스매치 체크
                        //if(fielder[pos].getFielder().getMissMatch() == true) //Debug.Log("========================>>>> "+pos + " 포지션 미스매치 상태");
                        //Debug.Log(pos+ " 위치 수비 능력치================>>"+fielder[pos].getName() + "포지션 "+ fielder[pos].getPosition() +"   수비: " + fielder[pos].getFielding() + "송구 : " + fielder[pos].getThrowing());
                        //Debug.Log(fielder[pos].getName() + "포지션 " + fielder[pos].getPosition() + "/ 현재포지션 " + fielder[pos].getCurPos());
                    }
                    //UnityEngine.Debug.Log("[수비 초기화]===============>> " + (pos + 1) + "  포지션 : " + fielder[pos].getFielder().getName());
                }
            }
                        
            if (bChangeCheck == true)
            {
                checkChangeFielder(team);
            }
        }

        /// <summary>
        /// 수비교체의 필요를 체크한다.
        /// </summary>
        private void checkChangeFielder(int team)
        {
            for (int i = CPlayer._CATCHER; i <= CPlayer._RIGHTFIELDER; i++)
            {
                fielderChangeNeedCheck(team, i);
            }
        }

        //posIndex가 내야수 인덱스인 경우 (공을 잡지 못했다는 가정하에) 뒤를 받혀주는 외야수의 인덱스를 얻어온다
        private int resetCatchFielder(int posIndex)
        {
            if (posIndex >= CPlayer._LEFTFIELDER) return posIndex;

            if (posIndex == CPlayer._FIRSTBASEMAN) return CPlayer._RIGHTFIELDER;
            else if (posIndex == CPlayer._THIRDBASEMAN) return CPlayer._LEFTFIELDER;
            else
            {
                if (posIndex == CPlayer._SHORTSTOP)
                {
                    posIndex = MyMath.Percent() < 63 ? CPlayer._LEFTFIELDER : CPlayer._CENTERFIELDER;
                }
                else
                {
                    posIndex = MyMath.Percent() < 63 ? CPlayer._RIGHTFIELDER : CPlayer._CENTERFIELDER;
                }
            }
            return CPlayer._CENTERFIELDER;
        }

        //야수가 땅볼시 베이스를 선택하게 하는 함수
        //디폴트는 1루이다.
        private int getThrowBaseIndex(int fielderIndex)
        {
            //야수선택은
            //1.만루시 P,1,3루시 홈송구
            //주자가 1루에 있는 경우 병살을 위한 2루 송구만 존재
            //번트도 고려

            int selectBase = SimulParm.FIRSTBASE_INDEX;

            //UnityEngine.Debug.Log("[주자상황]=====================>>bOnBase[0] = " + bOnBase[0] + "=====>>bOnBase[1] = " + bOnBase[1] + "=====>>bOnBase[2] = " + bOnBase[2]);

            if (hitType == SimulHitType.Bunt)
            {
                if (buntResultType == SpecificBuntType.SQUEEZ_FAIL)
                {
                    //UnityEngine.Debug.Log("[스퀴즈 실패]=====================>>홈으로 던져 3루주자 아웃");
                    //SimulManager.AddGameSummuryInfo("\n-3루주자 " + getRunner(SimulParm.THIRDBASE_INDEX).getRunner().getName() + ": 홈에서 아웃");
                    return SimulParm.HOMEBASE_INDEX;
                }
                else if (buntResultType == SpecificBuntType.SAC_FAIL || buntResultType == SpecificBuntType.SAC_DOUBLE_PLAY)
                {
                    //UnityEngine.Debug.Log("[보내기 실패]=====================>>");
                    int index = (bOnBase[SimulParm.SECONDBASE_INDEX] ? SimulParm.SECONDBASE_INDEX : SimulParm.FIRSTBASE_INDEX);
                    //SimulManager.AddGameSummuryInfo("\n-" + (index + 1) + "루주자 " + getRunner(index).getRunner().getName() + ": "+(index+2)+"루에서 아웃");
                    return (index+1);
                }
            }

            //홈을 거치는 병살 체크
            if (bOnBase[0] && bOnBase[1] && bOnBase[2])
            {
                //UnityEngine.Debug.Log("[야수선택상황]=====================>>만루인 경우");
                if (fielderIndex == CPlayer._PITCHER ||
                    fielderIndex == CPlayer._FIRSTBASEMAN ||
                    fielderIndex == CPlayer._THIRDBASEMAN)
                {
                    if (runner[getRunnerDestIndex(SimulParm.HOMEBASE_INDEX)].checkDoublePlay(grounderFiedingValue) == true)
                    {
                        //UnityEngine.Debug.Log("[야수선택상황]=====================>>홈으로 던져 병살시도");
                        selectBase = SimulParm.HOMEBASE_INDEX;
                        doublePlayStep = 2; //홈을 거치는 병살시도
                        return selectBase;
                    }
                }
            }
            //2루를 거치는 병살 체크
            if (bOnBase[0])
            {
                int firstBaseIndex = getRunnerDestIndex(SimulParm.SECONDBASE_INDEX);
                if (firstBaseIndex != -1)
                {
                    //UnityEngine.Debug.Log("[야수선택상황]=====================>>1루에 주자가 있는 경우");
                    if (runner[firstBaseIndex].checkDoublePlay(grounderFiedingValue) == true)
                    {
                        //UnityEngine.Debug.Log("[야수선택상황]=====================>>2루로 던져 병살시도");
                        selectBase = SimulParm.SECONDBASE_INDEX;
                        doublePlayStep = 1; //2루를 거치는 병살시도
                        return selectBase;
                    }
                }
            }
            //디폴트는 1루 값으로
            return selectBase;
        }

        //에러가 발생한 경우 에러 플래그 설정
        private void setErrorCase(SimulResultState state)
        {
            runner[hitterRunnerIndex].bErrorRunner = true;
            outCountIF++;

            if (state == SimulResultState.CatchError || state == SimulResultState.BoundError || state == SimulResultState.ThrowError)
            {
                bErrorHappen = true;
                int _firstBase = getRunnerIndex(SimulParm.FIRSTBASE_INDEX);
                if (_firstBase != -1)
                {
                    runner[_firstBase].bErrorRunner = true;
                }
            }
            else if (state == SimulResultState.DoubleOneError)
            {
                int _firstBase = getRunnerIndex(SimulParm.FIRSTBASE_INDEX);
                if (_firstBase != -1)
                {
                    runner[_firstBase].bErrorRunner = true;
                }
            }
            else if (state == SimulResultState.SingleOneError)
            {
                int _firstBase = getRunnerIndex(SimulParm.FIRSTBASE_INDEX);
                if (_firstBase != -1)
                {
                    runner[_firstBase].bErrorRunner = true;
                }
                int _secondBase = getRunnerIndex(SimulParm.SECONDBASE_INDEX);
                if (_secondBase != -1)
                {
                    runner[_secondBase].bErrorRunner = true;
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // 투타의 결과를 얻어오는 함수
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 현재 투구의 결과를 얻어오는 함수
        /// 상황이 종료되면 SimulResultByPitch형 열거자를 리턴
        /// </summary>
        /// <returns></returns>
        private SimulResultByPitch getPitchedResult(bool bStealCheck)
        {
            //피치시 발생하는 스킬
            if (bFromGame == false)
            {
                //액션 엔진에서 참조시 피치스킬을 발동하지 않는다.
                checkSkillByPitch();
            }

            bCheckSwing = false;
            PitchingArsenal balltype = PitchingMechanism.getSelectedBall(pitcher, gameInfo.ballCount, gameInfo.strikeCount, bLastFastBall);       //던지려 하는 구종의 형태
            //투수 능력치 세팅
            ////UnityEngine.//Debug.Log("======================================>>투수능력세팅");            
            pGujong = (int)PitchingMechanism.getBallType(balltype);
            int curSpeed = 150;
            pSpeed = ((curSpeed - 50) * 10) + 400;

            bLastFastBall = (pGujong == 0 ? true : false);                         //이전에 직구를 던졌는지 여부를 체크  

            
            pGuwee = getGuweeValue(balltype);                                                //최종적으로 선택된 구종의 구위
            
             
            pControl = pGuwee;//

            //타자 능력치
            bBonus = batter.getBonusValue();                     //보너스값
            bEye = getEyeValueSimul();                           //타자의 선구력
            bContact = getContactValueSimul();                   //타자의 컨택력
            bPower = getPowerValueSimul();                       //타자의 파워

            int conHit = gameInfo.allowChulu;//.conHit;
            if (conHit >= 2)
            {
                int rate = Mathf.Clamp(100 - (conHit * 10), 50, 100);
                bContact = (bContact * rate) / 100;
                bPower = (bPower * rate) / 100;
            }

            //투수 2차 능력치 세팅
            bool bMiss = PitchingMechanism.getMiss(pGuwee, fatigueStep);
            if (bMiss == true)
            {
                controlValue = ControlValue.Miss;
            }
            else
            {
                UserControlValue value =PitchingMechanism.getControlValue(pGuwee);
                controlValue = (ControlValue)value;
            }
            pFinalGuwee = PitchingMechanism.GetFinalGuwee(pGuwee, controlValue); //getFinalGuwee(pGuwee);

            pitcherPower = PitchingMechanism.GetPitcherPower(controlValue, pGuwee);  

            //투수가 스트라이크를 던지는지 여부
            bStrike = PitchingMechanism.pitchStrike(gameInfo.ballCount, gameInfo.strikeCount, bLastFastBall,
                                                          bContact + bPower,                      //배팅 밸류
                                                          pGuwee + pGuwee,                        //피칭 밸류
                                                          true);
            //gameInfo.DEBUG_CONTROLINFO(controlValue);

            if (controlValue == ControlValue.Bad && bStrike == true)
            {
                if (MyMath.Half()) bStrike = false;
            }

            if (checkStealBase() == true && bStealCheck == true)
            {
                /////////////////////////////////////////////////////////////////////////
                //도루 
                /////////////////////////////////////////////////////////////////////////                
                SimulStealState stealState = SimulStealState.NONE;

                CPlayer runnerPlayer = stealRunner.getRunner();
                CPlayer catcherPlayer = fielder[CPlayer._CATCHER].getFielder();
                stealState = SimulSteal.getStealResult(runnerPlayer, catcherPlayer, pitcher,false);                
                if (stealState != SimulStealState.NONE)
                {
                    if (stealState == SimulStealState.VsSkill)
                    {
                        int oRank = runnerPlayer.getSkillRank(SkillIndex.RunnerStealMaster); //대도 랭크
                        int dRank = catcherPlayer.getSkillRank(SkillIndex.CatcherSitThrow); //앉아쏴 랭크
                        bool bOffenseWin = SimulParm.checkOffenseSkillWin(oRank, dRank);
                        stealState = (bOffenseWin ? SimulStealState.VsSkill_RunnerWin : SimulStealState.VsSkill_CatcherWin);
                    }
                    else
                    {
                        //견제 세팅
                        SimulPickOffState pickoffState = SimulSteal.getPickOffResult(runnerPlayer, pitcher, pickoffCount);
                        if (pickoffState == SimulPickOffState.VsSkill)
                        {
                            //대결
                            int oRank = runnerPlayer.getSkillRank(SkillIndex.RunnerLead); //리드 랭크
                            int dRank = pitcher.getSkillRank(SkillIndex.LaserPickOff);    //광속견제 랭크
                            bool bOffenseWin = SimulParm.checkOffenseSkillWin(oRank, dRank);
                            stealState = bOffenseWin ? SimulStealState.PickOffVsSafe : SimulStealState.PickOffVsOut;
                        }
                        else if (pickoffState == SimulPickOffState.LaserPickOff)
                        {
                            //레이저로 죽음
                            stealState = SimulStealState.PickOffLaserOut;
                        }
                        else if (pickoffState == SimulPickOffState.Success)
                        {
                            //그냥 죽음
                            stealState = SimulStealState.PickOffOut;
                        }
                        else if (pickoffState == SimulPickOffState.Fail)
                        {
                            //도루 무효화
                            stealState = SimulStealState.NONE;
                        }
                    }

                    if (setSteal(stealState) == true)
                    {
                        bStealThreeOut = true;
                        bBattingEnd = true;
                        bInningEnd = true;
                    }
                }
                runnerStealState = stealState;

                if (bStrike == true)
                {
                    return SimulResultByPitch.LOOK_STRIKE;
                }
                else
                {
                    return SimulResultByPitch.LOOK_BALL;
                }
            }
            else
            {
                /////////////////////////////////////////////////////////////////////////
                //타격
                /////////////////////////////////////////////////////////////////////////
                if (bStrike == true)
                {
#if LOOK_BALL
                    MeaNoonShow = true;
                    return SimulResultByPitch.LOOK_STRIKE;
#else
                    //UnityEngine.Debug.Log("[투수 타자 대결] #############   " + pitcher.getName() + ": 스트라이크 존으로 던짐");
                    //투수가 스트라이크를 던질 경우
                    if (BattingMechanism.checkSwingDecide(controlValue,
                                                          pGuwee,
                                                          bEye, 
                                                          true, 
                                                          gameInfo.ballCount,
                                                          gameInfo.strikeCount, true) == true)
                    {
                        
                        //타이밍 결과
                        bTimingResult = BattingMechanism.getTimingResult(false, bLastFastBall,     //직구여부
                                                                         pFinalGuwee,            //투수능력치    
                                                                         bEye);          //타자능력치

                        //컨택 결과
                        bContactResult = BattingMechanism.getConctactResult(false, bStrike,
                                                                            pFinalGuwee,                  //투수구위
                                                                            bContact);                    //타자컨택


                        batterMaxPower = BattingMechanism.getBatterMaxPower(bPower, pFinalGuwee);

                        gameInfo.DEBUG_CONTACTINFO(bContactResult);
                        //gameInfo.DEBUG_TIMINGINFO(bTimingResult);

                                                
                        if( (bTimingResult >= BattingTiming.EARLY && bTimingResult <= BattingTiming.LATE)
                            && bContactResult != BattingContact.HUT_SWING)
                        {
                            //UnityEngine.Debug.Log("[투수 타자 대결] #############   "+batter.getName()+": 타격");
                            if (pitchPitcherSkill != null)
                            {
                                if (pitchPitcherSkill.ID == (int)SkillID.mea_hog)
                                {
                                    //매혹에 의한 헛스윙
                                    if (curBatterSkill != null)
                                    {
                                        if (curBatterSkill.effectIndex == SkillIndex.Unexpected)
                                        {
                                            //뜬금포 계열 초기화
                                            bMeahogSkillInvalidity = true;
                                            resetSkillCount(curIndex, (SkillID)curBatterSkill.ID);
                                            curBatterSkill = null;
                                        }
                                        else if (curBatterSkill.effectIndex == SkillIndex.AssaultBall)
                                        {
                                            //강습타구 계열 무효
                                            bMeahogSkillInvalidity = true;                                            
                                            curBatterSkill = null;
                                        }
                                    }
                                    return SimulResultByPitch.SWING;
                                }
                            }
                            
                            return SimulResultByPitch.HIT;
                        }
                        else
                        {
                            if (UnityEngine.Random.Range(0, SimulParm.Contact_Foul_Max_Value) < bContact)
                            {
                                //UnityEngine.Debug.Log("[투수 타자 대결] #############   " + batter.getName() + ": 파울");
                                return SimulResultByPitch.FOUL;
                            }
                            else
                            {
                                //UnityEngine.Debug.Log("[투수 타자 대결] #############   " + batter.getName() + ": 헛스윙");
                                return SimulResultByPitch.SWING;
                            }
                        }

                    }
                    else
                    {
                        //UnityEngine.Debug.Log("[투수 타자 대결] #############   "+batter.getName()+" 스트라이크를 지켜봄");
                        return SimulResultByPitch.LOOK_STRIKE;
                    }
#endif
                }
                else
                {
#if LOOK_BALL

                    return SimulResultByPitch.LOOK_BALL;
#else
                    //bStrike = false;
                    //UnityEngine.Debug.Log("[투수 타자 대결] #############   " + pitcher.getName() + ": 볼을 던짐");
                    //투수가 볼을 던질 경우
                    if (BattingMechanism.checkSwingDecide(controlValue,
                                                          pGuwee,
                                                          bEye, 
                                                          false, 
                                                          gameInfo.ballCount,
                                                          gameInfo.strikeCount, true) == true)
                    {
                        //UnityEngine.Debug.Log("[투수 타자 대결] #############   " + batter.getName() + ": 볼을 헛스윙");
                        return SimulResultByPitch.SWING;
                    }
                    else
                    {
                        //UnityEngine.Debug.Log("[투수 타자 대결] #############   " + batter.getName() + ": 볼을 걸러냄");
                        return SimulResultByPitch.LOOK_BALL;
                    }
#endif
                }
            }
        }

        /// <summary>
        /// 현재 타석에서의 최종적 상태를 얻어오는 함수
        /// 상황이 종료되면 SimulResultState형 열거자를 리턴하나 그렇지 않으면 SimulResultState.NONE을 리턴
        /// </summary>
        //int count = 0; 
        private SimulResultState getBattingState()
        {   
#if NOSCORE_GAME
            return SimulResultState.FlyOut;
#else
            bool bChrisma = false;
            //스킬에의한 결과 조작 세팅
            SimulResultState skillResult = SimulResultState.NONE;
            if (pitchedResult == SimulResultByPitch.HIT)
            {
                //카리스마
                if (curPitcherSkill != null)
                {
                    if (curPitcherSkill.ID == (int)SkillID.chrisma)
                    {
                        bChrisma = true;
                    }
                }

                //스킬에 의한 타구 결과
                skillResult = SimulSkillResult.GetSkillResult(curBatterSkill, pitchPitcherSkill, pitchBatterSkill, fielder);
                
                //뜬금포 무효화 체크
                if (SimulSkillResult.bHoesimSkillInvalidity == true)
                {
                    //스킬 무효화 여부
                    if (curBatterSkill.effectIndex == SkillIndex.Unexpected)
                    {
                        //뜬금포계열
                        resetSkillCount(curIndex, (SkillID)curBatterSkill.ID);
                        curBatterSkill = null;
                    }
                    SimulSkillResult.bHoesimSkillInvalidity = false;
                }
            }


            if (skillResult != SimulResultState.NONE)
            {
                fIndex = SimulSkillResult.skillFieder;
                hitType = SimulSkillResult.hitType;
                if (bAutoMode == true)
                {
                    if (SimulSkillResult.bCounterFieldSkill == true)
                    {
                        //특정 스킬을 카운터 필드 스킬로 막아냄
                        setUseSkillFlag(SkillUseStep.Fielding, null, SimulSkillResult.fielderSkill, null, VsResult.None);
                    }
                }
                return skillResult;
            }
            else
            {
                if (pitchedResult == SimulResultByPitch.FOUL)
                {
                    //파울
                    bStrike = true;
                    int st = (gameInfo.strikeCount + 1);    //나중에 지워
                    //UnityEngine.Debug.Log("[배팅 결과] ==============>> 파울 | B: " + gameInfo.ballCount+" S: "+(st>2?2:st)+" O: "+gameInfo.outCount);                
                    return gameInfo.checkStrike(pitcher, true);
                }
                else if (pitchedResult == SimulResultByPitch.LOOK_BALL)
                {
                    //볼
                    bStrike = false;
                    //UnityEngine.Debug.Log("[배팅 결과] ==============>>  볼 | B: " + (gameInfo.ballCount + 1) + " S: " + gameInfo.strikeCount + " O: " + gameInfo.outCount);
                    return gameInfo.checkBall();
                }
                else if (pitchedResult == SimulResultByPitch.LOOK_STRIKE || pitchedResult == SimulResultByPitch.SWING)
                {
                    //스트라이크
                    bStrike = true;
                    //UnityEngine.Debug.Log("[배팅 결과] ==============>> 스트라이크 | B: " + gameInfo.ballCount + " S: " + (gameInfo.strikeCount + 1) + " O: " + gameInfo.outCount);
                    if (gameInfo.strikeCount >= 1)
                    {
                        //쓰리번트 시도 안함
                        buntType = SimulBuntType.NONE;
                    }
                    return gameInfo.checkStrike(pitcher, false);
                }
                else if (pitchedResult == SimulResultByPitch.HIT)
                {
                    errorType = ErrorType.NONE;
                    SimulResultState result = SimulResultState.NONE;
                    if (buntType != SimulBuntType.NONE)
                    {
                        //UnityEngine.Debug.Log("[번트] =================================================>> 번트타입 = " + buntType);
                        fIndex = buntFIndex;
                        hitType = SimulHitType.Bunt;
                        grounderCatchType = GrounderCatchType.Bunt;
                        result = SimulParm.GetBuntResultType(buntResultType);
                    }
                    else
                    {
                        bTando = batter.getTando() + bBonus;
                        hitType = SimulParm.GetHitType(bTando, pGuwee, bContact, bPower, pitcherPower);

                        float powerCoef1 = BattingMechanism.getContactPowerCoef(bContactResult);
                        float powerCoef2 = BattingMechanism.getTimingPowerCoef(bTimingResult);
                        float curPower = batterMaxPower * powerCoef1 * powerCoef2;

                        CPlayer hitterRunner = runner[hitterRunnerIndex].getRunner();

                        if (hitType == SimulHitType.Fly)
                        {
                            //UnityEngine.Debug.Log("[타구] ==============>> 플라이볼");
                            gameInfo.DEBUG_FLY_COUNT();
                            flyType = SimulParm.GetFlySpecificType(curPower, bTando); // bContact, bPower, pitcherPower);  //파워, 컨택, 컨트롤, 구위-> 플라이 타입 결정
                            //UnityEngine.Debug.Log("[플라이 타입]================>> flyType = " + flyType);
                            fIndex = SimulParm.GetFlyCatchFieder(flyType);                               //flyType에 따른 어떤 야수가 잡을지

                            //플라이 스킬 체크
                            SimulResultState flySkillResult = SimulSkillResult.GetFlyFieldSkill(fielder[fIndex].getFielder(), fIndex, bChrisma);
                            if (flySkillResult != SimulResultState.NONE)
                            {
                                //스킬에 의한 플라이 처리
                                if (bAutoMode == true)
                                {
                                    setUseSkillFlag(SkillUseStep.Fielding, null, SimulSkillResult.fielderSkill, null, VsResult.None);
                                }

                                result = flySkillResult;
                            }
                            else
                            {
                                //UnityEngine.Debug.Log("[플라이 처리 야수]================>> fIndex = " + fIndex);
                                flyCatchType = SimulParm.GetFlyCatchType(flyType, fielder[fIndex]);          //야수의 수비능력,flyType에 따라 어떤식으로 공을 처리할지
                                //UnityEngine.Debug.Log("[플라이 처리 타입]================>> flyCatchType = " + flyCatchType);
                                if ((int)flyCatchType < 4)
                                {
                                    //에러타입
                                    errorType = (ErrorType)flyCatchType;
                                }
                                //UnityEngine.Debug.Log("[플라이시 에러 타입]================>> errorType = " + errorType);
                                result = SimulParm.GetFlyHitType(flyType, flyCatchType, hitterRunner, fielder[fIndex].getFielder()); //flyType,flyCatchType,주루능력에 따른 최종결과
                                //UnityEngine.Debug.Log("[플라이 최종 결과]================>> result = " + result);
                            }
                        }
                        else if (hitType == SimulHitType.Grounder)
                        {
                            //UnityEngine.Debug.Log("[타구] ==============>> 땅볼");
                            gameInfo.DEBUG_GROUNDER_COUNT();
                            grounderType = SimulParm.GetGrounderSpecificType(curPower);  //파워, 컨택, 컨트롤, 구위-> 땅볼 타입 결정
                            //UnityEngine.Debug.Log("[땅볼 타입]================>> grounderType = " + grounderType);
                            fIndex = SimulParm.GetGrounderCatchFieder(grounderType);                               //grounderType에 따른 어떤 야수가 잡을지

                            //그라운딩 스킬 체크
                            SimulResultState grounderSkillResult = SimulSkillResult.GetGrounderFieldSkill(hitterRunner, fielder[fIndex].getFielder(), fIndex, bChrisma);
                            if (grounderSkillResult != SimulResultState.NONE)
                            {
                                //스킬에 의한 땅볼처리
                                if (bAutoMode == true)
                                {
                                    setUseSkillFlag(SkillUseStep.Fielding, SimulSkillResult.runnerSkill, SimulSkillResult.fielderSkill, null, SimulSkillResult.fieldVs);
                                }

                                result = grounderSkillResult;
                            }
                            else
                            {
                                //일반 땅볼 처리
                                //UnityEngine.Debug.Log("[땅볼 처리 야수]================>> fIndex = " + fIndex);
                                grounderCatchType = SimulParm.GetGrounderCatchType(grounderType, fielder[fIndex], fIndex, shiftBonus);          //야수의 수비능력,grounderType에 따라 어떤식으로 공을 처리할지
                                //UnityEngine.Debug.Log("[땅볼 처리 타입]================>> grounderCatchType = " + grounderCatchType);
                                if ((int)grounderCatchType < 4)
                                {
                                    //에러타입
                                    errorType = (ErrorType)grounderCatchType;
                                }
                                //UnityEngine.Debug.Log("[땅볼 에러 타입]================>> errorType = " + errorType);
                                grounderFiedingValue = SimulParm.getGrounderFiedingValue(grounderType, grounderCatchType, fielder[fIndex]);   //그라운더 딜레이값
                                result = SimulParm.GetGrounderHitType(grounderCatchType, hitterRunner, grounderFiedingValue); //grounderType,grounderCatchType,주루능력에 따른 최종결과
                                //UnityEngine.Debug.Log("[땅볼 최종 결과]================>> result = " + result);
                            }
                        }
                        else //if (hitType == SimulHitType.Liner)
                        {
                            //UnityEngine.Debug.Log("[타구] ==============>> 직선타");
                            gameInfo.DEBUG_LINER_COUNT();
                            linerType = SimulParm.GetLinerSpecificType(curPower);  //파워, 컨택, 컨트롤, 구위-> 라이너 타입 결정
                            //UnityEngine.Debug.Log("[직선타 타입]================>> linerType = " + linerType);
                            fIndex = SimulParm.GetLinerCatchFieder(linerType);                               //linerType에 따라 어떤 야수가 잡을지
                            //UnityEngine.Debug.Log("[직선타 처리 야수]================>> fIndex = " + fIndex);
                            flyCatchType = SimulParm.GetLinerCatchType(linerType, fielder[fIndex], fIndex);          //야수의 수비능력,linerType에 따라 어떤식으로 공을 처리할지
                            //UnityEngine.Debug.Log("[직선타 처리 타입]================>> flyCatchType = " + flyCatchType);
                            if ((int)flyCatchType < 4)
                            {
                                //에러타입
                                errorType = (ErrorType)flyCatchType;
                            }
                            //UnityEngine.Debug.Log("[직선타 에러 타입]================>> errorType = " + errorType);
                            result = SimulParm.GetLinerHitType(linerType, flyCatchType, hitterRunner, fielder[fIndex].getFielder()); //linerType,flyCatchType,주루능력에 따른 최종결과
                            //UnityEngine.Debug.Log("[직선타 최종 결과]================>> result = " + result);
                        }
                    }
                    return result;
                }
            }
            return SimulResultState.NONE;
#endif
        }


        /// <summary>
        /// getBattingState 함수로 부터 얻어온 SimulResultState 데이터를 바탕으로 타석 최종결과를 리턴
        /// 쓰리아웃인 경우 true를 리턴하고 이닝종료 플래그를 on한다.
        /// </summary>
        /// <returns></returns>
        
        private bool getBattingResult(SimulResultState state)
        {
            MeaNoonShow = false;
            HeosimShow = false;
            MeahogShow = false;

            if (bMeahogSkillInvalidity == true)
            {
                //매혹 무효화시
                MeahogShow = true;
                bMeahogSkillInvalidity = false;
            }


            bool threeOut = false;

            if (state == SimulResultState.StrikeOut)      //삼진
            {
                MeahogShow = true; //매혹 연출 플래그 (매발동시 필요없고 효과를 보여줘야 연출)
                pitcher.setPiledupSkill(SkillIndex.DoctorK, 3, true); //닥터 K효과
                outCountIF++;
                threeOut = gameInfo.checkOut(pitcher);
                removeRunner(hitterRunnerIndex, RunnerState.StrikeOut, -1);
                gameInfo.addStrkeOutCount(curIndex, batter, pitcher, fielder[CPlayer._CATCHER].getFielder());
                //UnityEngine.Debug.Log("[배팅 결과] ==============>> 삼진아웃 |  Out: " + gameInfo.outCount);
                strBatterResult = "삼진 아웃";
            }
            else if (state == SimulResultState.Grounder)      //땅볼
            {
                ////UnityEngine.//Debug.Log("==========================================>>getBattingResult Grounder Check!");
                ////UnityEngine.//Debug.Log("==========================================>>buntType = "+buntType);
                ////UnityEngine.//Debug.Log("==========================================>>처리야수 = " + fIndex);
                HeosimShow = true; //회심일격 연출 플래그 (매발동시 필요없고 효과를 보여줘야 연출)
                outCountIF++;
                threeOut = gameInfo.checkOut(pitcher);

                if (buntType != SimulBuntType.NONE)
                {
                    grounderFiedingValue = SimulParm.GetBuntFieldValue(buntResultType);
                }

                if (threeOut == true)
                {
                    //UnityEngine.Debug.Log("[그라운드 송구시] ==============>> 쓰리 아웃된경우 미리 모두 처리 해줘");
                    grounderThrowBase = SimulParm.FIRSTBASE_INDEX;
                    removeRunner(getRunnerDestIndex(grounderThrowBase), RunnerState.FourceOut, grounderThrowBase);
                    int poIndex = getPutOutIndex(grounderThrowBase, fIndex);
                    gameInfo.setFieldOut(state, hitType, batter, pitcher, fielder[poIndex].getFielder(), fielder[fIndex].getFielder(), false);

                }
                else
                {
                    //UnityEngine.Debug.Log("[그라운드 송구시] ==============>> 쓰리 아웃이 아닌 경우 setRun에서 처리 맡김");
                    grounderThrowBase = getThrowBaseIndex(fIndex);
                    //UnityEngine.Debug.Log("[송구] ==============>> " + (grounderThrowBase + 1) + "루로 송구 ");
                    if (buntSuccessType == SimulBuntType.NONE)
                    {
                        gameInfo.setFieldOut(state, hitType, batter, pitcher, null, null, false);
                        if (buntType != SimulBuntType.NONE && (buntResultType == SpecificBuntType.SQUEEZ_FAIL || buntResultType == SpecificBuntType.SAC_FAIL))
                        {
                            //번트 실패의 경우
                            strBatterResult = Util.getBatterResult(fIndex, true, 0, "땅볼 (야수선택)");
                        }
                        else
                        {
                            strBatterResult = Util.getBatterResult(fIndex, true, 0, "땅볼 아웃");
                        }
                    }
                    else
                    {
                        gameInfo.setFieldOut(state, hitType, batter, pitcher, null, null, true);
                        strBatterResult = (buntSuccessType == SimulBuntType.SQUEEZE ? "스퀴즈 번트" : "희생 번트");
                    }
                }
                ////UnityEngine.//Debug.Log("==========================================>>grounderThrowBase = " + grounderThrowBase);
                //UnityEngine.Debug.Log("[배팅 결과] ==============>> " + state + " |  Out: " + gameInfo.outCount);
#if HITBALLTYPE_RECORD
                grounderOut[curIndex]++;
#endif
                
            }
            else if (state == SimulResultState.FlyOut ||         //뜬공
                    state == SimulResultState.LineOut)          //직선타
            {
                HeosimShow = true; //회심일격 연출 플래그 (매발동시 필요없고 효과를 보여줘야 연출)
                outCountIF++;
                threeOut = gameInfo.checkOut(pitcher);
                removeRunner(hitterRunnerIndex, RunnerState.None, -1);
                gameInfo.setFieldOut(state, hitType, batter, pitcher, fielder[fIndex].getFielder(), null, false);
                //UnityEngine.Debug.Log("[배팅 결과] ==============>> " + state + " |  Out: " + gameInfo.outCount);
#if HITBALLTYPE_RECORD
                if (state == SimulResultState.FlyOut) flyOut[curIndex]++;
                else linerOut[curIndex]++;
#endif
                //SimulManager.AddGameSummuryInfo("\n[ffde00]" + batter.getName() + ": " + Util.getBatterResult(fIndex, false, 0, "뜬공 아웃") + "[-]");
            }
            else if (state == SimulResultState.FourBall)    //포볼
            {
                gameInfo.addFourBall(curIndex,  batter, pitcher,false);
                //UnityEngine.Debug.Log("[배팅 결과] ==============>> 볼넷 |  Out: " + gameInfo.outCount);
                strBatterResult = "베이스 온 볼";
            }
            else if (state == SimulResultState.InfieldSingle || //내안
                     state == SimulResultState.BuntSingle || //내안
                     state == SimulResultState.InfieldTurboSingle) //내안(터보)
                 
            {
                gameInfo.addHit(curIndex, state, hitType, batter, pitcher);
                strBatterResult = Util.getBatterResult(fIndex, true, 1, "");
            }
            else if (state == SimulResultState.Single ||    //단타
                     state == SimulResultState.Double ||    //2루타
                     state == SimulResultState.Triple ||    //3루타
                     state == SimulResultState.HomeRun)     //홈런
            {

                MeaNoonShow = true; //매의눈 연출 플래그 (매발동시 필요없고 효과를 보여줘야 연출)
                int run = (bOnBase[0] ? 1 : 0) + (bOnBase[1] ? 1 : 0) + (bOnBase[2] ? 1 : 0);
                gameInfo.addHit(curIndex, state, hitType, batter, pitcher, run);
                //UnityEngine.Debug.Log("[배팅 결과] ==============>> " + state + " |  Out: " + gameInfo.outCount);
                fIndex2 = fIndex = resetCatchFielder(fIndex);   //타구 처리 야수 재설정
                //UnityEngine.Debug.Log("[야수 재설정]======================>> fIndex = " + fIndex);
                strBatterResult = Util.getBatterResult(fIndex, true, (int)state - 5, "");
#if HITBALLTYPE_RECORD
                if (state == SimulResultState.InfieldSingle || state == SimulResultState.InfieldTurboSingle)
                {
                    infieldHit[curIndex]++;
                }
                else
                {
                    if (hitType == SimulHitType.Fly) flyHit[curIndex]++;
                    else if (hitType == SimulHitType.Grounder) grounderHit[curIndex]++;
                    else linerHit[curIndex]++;
                }
#endif
            }
            else if (state == SimulResultState.SingleOneError ||    //단타
                     state == SimulResultState.DoubleOneError ||    //2루타
                     state == SimulResultState.TripleOneError)
            {
                MeaNoonShow = true; //매의눈 연출 플래그 (매발동시 필요없고 효과를 보여줘야 연출)
                setErrorCase(state);
                gameInfo.addHit(curIndex, state, hitType, batter, pitcher);
                gameInfo.addError(1 - curIndex, hitType, null, pitcher, fielder[fIndex].getFielder());
                //UnityEngine.Debug.Log("[배팅 결과] ==============>> " + state + " |  Out: " + gameInfo.outCount);
                fIndex2 = fIndex = resetCatchFielder(fIndex);   //타구 처리 야수 재설정
                //UnityEngine.Debug.Log("[야수 재설정]======================>> fIndex = " + fIndex);
                strBatterResult = Util.getBatterResult(fIndex, true, (int)state - 15, "");
#if HITBALLTYPE_RECORD
                if (hitType == SimulHitType.Fly) flyHit[curIndex]++;
                else if (hitType == SimulHitType.Grounder) grounderHit[curIndex]++;
                else linerHit[curIndex]++;
#endif
            }
            else if (state == SimulResultState.CatchError ||
                     state == SimulResultState.BoundError ||
                     state == SimulResultState.ThrowError)
            {
                setErrorCase(state);
                gameInfo.addError(1 - curIndex, hitType, batter, pitcher, fielder[fIndex].getFielder());
                //UnityEngine.Debug.Log("[배팅 결과] ==============>> " + state + " |  Out: " + gameInfo.outCount);
                fIndex2 = fIndex = resetCatchFielder(fIndex);   //타구 처리 야수 재설정
                //UnityEngine.Debug.Log("[야수 재설정]======================>> fIndex = " + fIndex);
                strBatterResult = "에러로 출루";
#if HITBALLTYPE_RECORD
                if (hitType == SimulHitType.Fly) flyOut[curIndex]++;
                else if (hitType == SimulHitType.Grounder) grounderOut[curIndex]++;
                else linerOut[curIndex]++;
#endif
            }
            /*
            else if (state == SimulResultState.BuntDoublePlay || state == SimulResultState.BuntFail || state == SimulResultState.BuntSuccess)      //번트
            {
                UnityEngine.Debug.Log("[번트처리] ==============>> 번트 처리 로직 state = "+state);
                //번트처리
                outCountIF++;
                threeOut = gameInfo.checkOut(pitcher);

                grounderThrowBase = SimulParm.FIRSTBASE_INDEX;
                if(buntType == SimulBuntType.SACRIFY)
                {
                    if (state == SimulResultState.BuntDoublePlay)
                    {
                        grounderFiedingValue = 99999;
                        grounderThrowBase = (bOnBase[1] == true ? SimulParm.THIRDBASE_INDEX : getThrowBaseIndex(fIndex));
                    }
                    else if(state == SimulResultState.BuntFail)
                    {
                        //실패
                        grounderFiedingValue = 0;
                        grounderThrowBase = (bOnBase[1] == true?SimulParm.THIRDBASE_INDEX:SimulParm.SECONDBASE_INDEX);
                    }
                }
                else if(buntType == SimulBuntType.SQUEEZE)
                {
                    if (state == SimulResultState.BuntFail)
                    {
                        //실패
                        grounderThrowBase = SimulParm.HOMEBASE_INDEX;
                    }
                }

                removeRunner(getRunnerDestIndex(grounderThrowBase), RunnerState.FourceOut, grounderThrowBase);
                int poIndex = getPutOutIndex(grounderThrowBase, fIndex);
                gameInfo.setFieldOut(state, hitType, batter, pitcher, fielder[poIndex].getFielder(), fielder[fIndex].getFielder(), false);
                
            }*/
            else if (state == SimulResultState.FielderChoice)
            {
                strBatterResult = "야수 선택";
            }
            else
            {
                ////UnityEngine.//Debug.Log("====================>>[" + batter.getName() + "배팅 최종 결과] 기타");
            }
            ////UnityEngine.//Debug.Log("====================>>" + batter.getName() + "배팅 최종 결과 state " + state + " |  Out: " + gameInfo.outCount);

            bBattingEnd = true; //아웃시 배팅 플래그 종료 -> 다음 타자로 넘어간다.

            setBatter(state);   //타자 세팅
            setPitcher(state);  //투수 세팅
            //setField(state);    //수비 세팅

            if (threeOut == false)
            {
                threeOut = setRun(state);      //주자 세팅
            }


            return threeOut;
        }

        //타자주자가 살아나갔는지 여부
        private bool getHitterRunnerSafe()
        {
            if (resultState == SimulResultState.StrikeOut ||
                resultState == SimulResultState.FlyOut ||
                resultState == SimulResultState.Grounder ||
                resultState == SimulResultState.LineOut)
            {
                //UnityEngine.Debug.Log("[타자주자 결과]================>> 아웃");
                return false;
            }
            else
            {
                //UnityEngine.Debug.Log("[타자주자 결과]================>> 세입");
                return true;
            }
        }
        /////////////////////////////////////////////////////////////////////////
        // 투타의 결과 바뀌게 되는 투수,타자,주자,야수의 상태를 세팅하고 
        // 상태에 따라 파생되는 기록을 처리한다.
        /////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 야수의 상태 변화를 세팅하고 발생되는 야수기록을 처리한다.
        /// </summary>
        /// <param name="state"></param>
        private void setField(SimulResultState state)
        {

        }

        /// <summary>
        /// 주자의 상태 변화를 세팅하고 발생되는 주자기록을 처리한다.
        /// </summary>
        /// <param name="state"></param>
        private bool setRun(SimulResultState state)
        {
            int curOut = gameInfo.outCount;
            int basicAddBase = getAddBase(state);   //기본적으로 얻는 베이스 수
            if (basicAddBase == -1) return false;         //얻는 베이스가 없으면 더이상 수행 불가

            int[] currentRunnerIndex = new int[4];
            for (int i = 0; i < 4; i++)
            {
                bOnBase[i] = false;                         //베이스를 비워둠
                currentRunnerIndex[i] = getRunnerIndex(i);  //각베이스의 주자 인덱스 버퍼
                runnerRunningOut[i] = -1;                   //주루사 관련 초기화
            }

            bRunnerOutFlag = false;

            curOut += moveRunner(SimulParm.THIRDBASE_INDEX, basicAddBase, currentRunnerIndex[2]);  //3루주자
            curOut += moveRunner(SimulParm.SECONDBASE_INDEX, basicAddBase, currentRunnerIndex[1]); //2루주자
            curOut += moveRunner(SimulParm.FIRSTBASE_INDEX, basicAddBase, currentRunnerIndex[0]);  //1루주자
            curOut += moveRunner(SimulParm.HOMEBASE_INDEX, basicAddBase, currentRunnerIndex[3]);                         //타자주자  

            bRunnerOutFlag = false;

            if (curOut >= 3) curOut = 3;

            if (curOut != gameInfo.outCount)
            {
                if (bFromGame == false)
                {
                    //주루사에 의한 이닝 카운트
                    //UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수이닝");
                    pitcher.setRecord(Param.ST_IP, (curOut - gameInfo.outCount));
                }
            }

            gameInfo.outCount = curOut;
            //UnityEngine.Debug.Log("&&&&&&&&&&&    주자 상태 curOut/outCount = "+curOut+ " / "+gameInfo.outCount);

           

            if (curOut >= 3)
            {
                return true;
            }
            else
            {
                int run = 0;
                //3아웃이 아닌 경우 득점 가산
                for (int i = 0; i < 4; i++)
                {
                    if (runner[i] != null)
                    {
                        if (runner[i].bAddScore == true)
                        {
                            runner[i].bAddScore = false;
                            addScore(i);
                            run++;
                        }
                    }
                }
                if (bErrorHappen == false)
                {
                    if (run > 0)
                    {
                        strBatterResult += " (타점 " + run + ")";
                    }
                }

                return false;
            }

        }

        /// <summary>
        /// 투수의 상태 변화를 세팅하고 발생되는 투수기록을 처리한다.
        /// </summary>
        /// <param name="state"></param>
        private void setPitcher(SimulResultState state)
        {

        }

        /// <summary>
        /// 타자의 상태 변화를 세팅하고 발생되는 타자기록을 처리한다.
        /// </summary>
        /// <param name="state"></param>
        private void setBatter(SimulResultState state)
        {

        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //투수교체
        //////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 투수교체 여부를 리턴해주고
        /// 현재 어떤 형태의 투수교체인지를 세팅해준다 (bChaseOn,bSetupOn,bSaveOn,bLongReliefOn등의 플래그 세팅
        //pitcherChanging()f 호출하여 투수가 교체 되었는지 여부를 리턴해줌
        private bool checkPitcherChanged()
        {
            defense = 1 - curIndex;
            offense = curIndex;

            //////UnityEngine.//Debug.Log("==================>>투수교체체크???");
            if (pitcherChanging())
            {
                //UnityEngine.Debug.Log("##############################투수교체 pitcherChangeType = "+pitcherChangeType);
                if (getChangePitcher(defense, pitcherChangeType) != -1)   //교체할 투수의 인덱스를 얻어옴
                {
                    setChangeFlagInit(defense);
                    if (pitcherChangeType == ChangeType.CHASE)
                    {
                        //추격조 출전
                        bChaseOn[defense] = true;
                    }
                    else if (pitcherChangeType == ChangeType.MUST_WIN)
                    {
                        //필승조 출전
                        bSetupOn[defense] = true;
                    }
                    else if (pitcherChangeType == ChangeType.SAVE)
                    {
                        //마무리 출전
                        bSaveOn[defense] = true;
                    }
                    else if (pitcherChangeType == ChangeType.LONGRIELEF)
                    {
                        //롱릴리프
                        bLongReliefOn[defense] = true;
                    }

                    lastPitcher = pitcher;

                    int currentPitcherIndex = sPlayer.GetPitcherIndex(defense);


                    if (lastPitcher.getStat(Param.ST_SV) == Param.P_ACHIEVE_TRY)
                    {
                        //홀드 조건
                        //UnityEngine.Debug.Log("[투수교체]==============>> 이전 투수가 홀드/세이브 상황에서 등판 // 투수이름: " + lastPitcher.getName());
                        if (gameInfo.run[defense] > gameInfo.run[offense])
                        {
                            //UnityEngine.Debug.Log("[투수업적))==============>> 홀드조건 만족하고 강판 // 투수이름: " + lastPitcher.getName());
                            lastPitcher.setPitcherAchieve(Param.ST_HLD, Param.P_ACHIEVE_COMPLETE);
                        }
                        else
                        {
                            //UnityEngine.Debug.Log("[투수업적))==============>> 블론을 저지르고 강판 // 투수이름: " + lastPitcher.getName());
                            lastPitcher.setPitcherAchieve(Param.ST_BS, Param.P_ACHIEVE_COMPLETE);
                        }
                    }
                    else
                    {
                        //승패조건
                        //UnityEngine.Debug.Log("[투수교체]==============>> 강판시 승패 조건을 체크");
                        if (checkWinPitcher(defense, currentPitcherIndex) == true)
                        {
                            //UnityEngine.Debug.Log("[투수교체]==============>> 승리투수 요건 만족하고 강판 // 투수이름: " + lastPitcher.getName());
                            lastPitcher.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_TRY);
                        }
                        else if (checkLosePitcher(defense, currentPitcherIndex) == true)
                        {
                            //UnityEngine.Debug.Log("[투수교체]==============>> 패배상황에서 강판 // 투수이름: " + lastPitcher.getName());
                            lastPitcher.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_TRY);
                        }
                        else
                        {
                            //UnityEngine.Debug.Log("[투수교체]==============>> 승패 상관 없이 강판 // 투수이름: " + lastPitcher.getName());
                            lastPitcher.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_NONE);
                            lastPitcher.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_NONE);
                        }
                    }


                    //승계주자 셋팅 //run.setLastPitcher();
                    for (int i = 0; i < 4; i++)
                    {
                        if (runnerActive[i] == true)
                        {
                            if (runner[i].bHitterRunner == false)
                            {
                                runner[i].bLastPitcher = true;
                            }
                        }
                    }

                    sPlayer.SetCurrentPitcherIndex(defense, inPitcher, false); //바뀐투수의 피처 인덱스 세팅
                    sPlayer.SetPitcherOut(defense, inPitcher, true);           //현재 투수를 출전한 선수로 세팅

                    //UnityEngine.Debug.Log("[현재 스코어]==================================================>> MYTEAM " + gameInfo.run[0] + ":  CPUTEAM " + gameInfo.run[1]);

                    initPitcher(true);
                    pitcher.bChangeIn = true;

                    //세이브 혹은 홀드
                    //UnityEngine.Debug.Log("[투수교체]==============>> 등판시 홀드/세이브 조건을 체크");
                    if (checkSavePitcher(defense) == true)
                    {
                        //UnityEngine.Debug.Log("[투수교체]==============>> 세이브 조건에서 출격 // 투수이름: " + pitcher.getName());
                        pitcher.setPitcherAchieve(Param.ST_SV, Param.P_ACHIEVE_TRY);
                    }

                    //pitcher.changedOrder = (lastPitcher.changedOrder + 1);

                    setPinchInit();
                    
                    return true;
                }
            }
            return false;
        }

        //AI가 현재 투수를 바꿔야하는지를 판단
        private int getChangePitcher(int team, ChangeType changeType)
        {
            int inIndex = -1;
            inIndex = getReiefIndex(team, changeType);

            if (inIndex != -1)
            {
                inPitcher = inIndex;
            }

            return inIndex;
        }

        //투수 가치값을 리턴
        private int getPitcherValue(CPlayer player)
        {
            //나중에 특수 능력 고려해라~~
            PitchingArsenal[] ballType = player.getBallType();
            int value = 0;
            
            for (int i = 0; i < 5; i++)
            {
                if (ballType[i] != PitchingArsenal.NONE)    //구종이 있는 경우
                {
                    value += player.getBallValue(ballType[i]);  //ballValue[i];
                }
            }

            return (value);

        }

        //바꿔야할 중계 투수의 인덱스를 리턴
        private int getReiefIndex(int team, ChangeType changeType)
        {
            // 상황에 맞는 밸류를 
            int inIndex = -1;
            int scoreGab = getScoreGab(team);
            int[] changeValue = new int[SimulPlayer.NUM_PITCHER];
            int[] _weight = new int[PitchingMechanism.TYPE_NUM];    //가중치
            bool bDescendingOrder = false; //내림차순

            if (changeType == ChangeType.CHASE)
            {
                if (scoreGab <= -4)
                {
                    //4점차 이상
                    for (int i = 0; i < PitchingMechanism.TYPE_NUM; i++)
                    {
                        _weight[i] = PitchingMechanism.chaseValueOver4Point[i];
                    }
                    bDescendingOrder = false;
                }
                else
                {
                    //4점차 이하
                    for (int i = 0; i < PitchingMechanism.TYPE_NUM; i++)
                    {
                        _weight[i] = PitchingMechanism.chaseValueUnder4Point[i];
                    }
                    bDescendingOrder = true;
                }
            }
            else if (changeType == ChangeType.MUST_WIN)
            {
                if (currentInning >= 8)
                {
                    //8회 이후
                    for (int i = 0; i < PitchingMechanism.TYPE_NUM; i++)
                    {
                        _weight[i] = PitchingMechanism.setupValueOver8[i];
                    }
                    bDescendingOrder = true;
                }
                else if (currentInning == 7)
                {
                    //7회
                    for (int i = 0; i < PitchingMechanism.TYPE_NUM; i++)
                    {
                        _weight[i] = PitchingMechanism.setupValue7[i];
                    }
                    bDescendingOrder = false;
                }
                else //if (manager.nInningCount >= 8)
                {
                    //6회 이전
                    for (int i = 0; i < PitchingMechanism.TYPE_NUM; i++)
                    {
                        _weight[i] = PitchingMechanism.setupValue6[i];
                    }
                    bDescendingOrder = false;
                }

            }
            else if (changeType == ChangeType.SAVE)
            {
                //세이브
                for (int i = 0; i < PitchingMechanism.TYPE_NUM; i++)
                {
                    _weight[i] = PitchingMechanism.saveValue[i];
                }
                bDescendingOrder = true;
            }
            else //if (changeType == ChangeType.LONGRELEIF)
            {
                //롱 릴리프
                for (int i = 0; i < PitchingMechanism.TYPE_NUM; i++)
                {
                    _weight[i] = PitchingMechanism.longReliefValue[i];
                }
                bDescendingOrder = true;
            }



            for (int i = 0; i < SimulPlayer.NUM_PITCHER; i++)
            {
                int rating = getPitcherValue(sPlayer.GetPitcher(team, i, false));
                int outWeight = (sPlayer.GetPitcherOut(team, i) == true ? -1 : 1);
                int pitcherPosition = sPlayer.GetPitcher(team, i, false).getPitcherPosition();
                if (pitcherPosition == 0 || sPlayer.GetPitcherOut(team, i) == true)
                {
                    changeValue[i] = -1;
                }
                else
                {
                    changeValue[i] = _weight[pitcherPosition] + (outWeight * rating);
                }
            }


            int value = bDescendingOrder ? 0 : 1000000;

            //UnityEngine.Debug.Log("##############################>> 차순 bDescendingOrder : " + bDescendingOrder + "##### value = " + value);

            for (int i = 0; i < SimulPlayer.NUM_PITCHER; i++)
            {
                if (changeValue[i] >= 0)
                {
                    if (bDescendingOrder == true)   //내림
                    {
                        //큰 밸류
                        if (changeValue[i] > value)
                        {
                            inIndex = i;
                            value = changeValue[i];
                        }

                    }
                    else //오름
                    {
                        //작은 밸류
                        if (changeValue[i] < value)
                        {
                            inIndex = i;
                            value = changeValue[i];
                        }
                    }
                }
            }
            //UnityEngine.Debug.Log("##############################>> 선택된 구원 투수 인덱스 : " + inIndex);

            return inIndex;
        }

        //투수를 바꾸어야 하는지 여부를 체크 // 매 이닝과 addScore마다 호출
        private bool pitcherChanging()	//매 AddScore 마다 호출
        {
            pitcherChangeType = ChangeType.NA;
            CPlayer nextBatter = sPlayer.GetNextBatter(offense, 1);
            CPlayer nextBatter2 = sPlayer.GetNextBatter(offense, 2);

#if _PITCHER_CHANGE_TEST
	        //return true;
#endif

            //실점에 따른 투수교체 타이밍 조절
            int scoreGab = getScoreGab(defense);
            int allowrun = allowRun[defense];

            if (sPlayer.IsStartPitcher(defense) == true)//if (manager.starterIndex[def] == manager.pitcherIndex[def])
            {
                //선발투수 교체

                //세이브 조건
                if (checkSaveCondition(true, scoreGab) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>세이브 투수 출현");
                    pitcherChangeType = ChangeType.SAVE;
                    return true;
                }

                //롱릴리프
                if (checkLongReliefCondition(true, scoreGab, allowrun, ChangeType.NA) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>롱리리프 투수 출현");
                    pitcherChangeType = ChangeType.LONGRIELEF;
                    return true;
                }

                //추격 패전
                if (checkChaseCondition(true, scoreGab, allowrun, ChangeType.NA) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>추격조 투수 출현");
                    pitcherChangeType = ChangeType.CHASE;
                    return true;
                }
                //필승
                if (checkSetupCondition(true, scoreGab, allowrun, ChangeType.NA) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>필승조 투수 출현");
                    pitcherChangeType = ChangeType.MUST_WIN;
                    return true;
                }

                //방전시 바꿈
                if (checkFatigueCase(true, scoreGab, allowrun) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>방전으로 인한 투수 바꿈");
                    if (currentInning <= 5)
                    {
                        ////UnityEngine.//Debug.Log("===============>>5회 이전 롱릴리프 투수 바꿈");
                        pitcherChangeType = ChangeType.LONGRIELEF;
                    }
                    else if (currentInning <= 6)
                    {
                        //6회 이전
                        ////UnityEngine.//Debug.Log("===============>>7회 이전 추격조 투수 바꿈");
                        pitcherChangeType = ChangeType.CHASE;
                    }
                    else
                    {
                        //7회 이후
                        if (scoreGab >= 0 && scoreGab < 5)
                        {
                            ////UnityEngine.//Debug.Log("===============>>7회 이후 이기고 있으면서 5점 이하인 경우 경우 필승조 투수 바꿈");
                            pitcherChangeType = ChangeType.MUST_WIN;
                        }
                        else
                        {
                            ////UnityEngine.//Debug.Log("===============>>7회 이후 지고 있는 경우 패배조 투수 바꿈");
                            pitcherChangeType = ChangeType.CHASE;
                        }
                    }

                    return true;
                }
            }
            else
            {
                int curInning = getPitchedInning();

                if (bSaveOn[defense] == true)
                {
                    //세이브 투수 
                    //1이닝 이상을 던진 경우
                    if (curInning >= 10)
                    {
                        ////UnityEngine.//Debug.Log("==============>>마무리 투수가 1이닝 이사을 던진경우");
                        if (scoreGab < 0)
                        {
                            ////UnityEngine.//Debug.Log("==============>>마무리 투수가 던져서 지고 있을떄");
                            //지고 있는 경우
                            //추격 패전
                            if (checkChaseCondition(false, scoreGab, allowrun, ChangeType.SAVE) == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>마무리 투수가 던져서 지고 있을떄 패전조");
                                pitcherChangeType = ChangeType.CHASE;
                                return true;
                            }
                        }
                        else
                        {
                            ////UnityEngine.//Debug.Log("==============>>마무리 투수가 던져서 최소 동점 허용");
                            //최소 동점
                            //필승
                            if (checkSetupCondition(false, scoreGab, allowrun, ChangeType.SAVE) == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>마무리 투수가 던져서 최소 동점 허용 추격조");
                                pitcherChangeType = ChangeType.MUST_WIN;
                                return true;
                            }
                        }
                    }
                }
                else if (bChaseOn[defense] == true)
                {
                    //추격조 투수 

                    //1이닝 이상을 던지고 
                    //세이브 조건
                    if (curInning >= 10)
                    {
                        ////UnityEngine.//Debug.Log("==============>>추격조 투수가 1이닝 이사을 던진경우");
                        if (scoreGab >= 0)
                        {
                            ////UnityEngine.//Debug.Log("==============>>추격조 투수가 던져 최소 동점이상이 되었을때");
                            //동점 이상을 된경우
                            if (checkSaveCondition(false, scoreGab) == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>추격조 투수가 던져 최소 동점이상이 되었을때 세이브 조건 만족하면");
                                pitcherChangeType = ChangeType.SAVE;
                                return true;
                            }

                            //필승
                            if (checkSetupCondition(false, scoreGab, allowrun, ChangeType.CHASE) == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>추격조 투수가 던져 최소 동점이상이 되었을때 추격조 조건 만족하면");
                                pitcherChangeType = ChangeType.MUST_WIN;
                                return true;

                            }
                        }
                        else
                        {
                            ////UnityEngine.//Debug.Log("==============>>추격조 투수가 던져 여전히 지고 있는 경우");
                            //여전히 지고 있는 경우
                            if (checkChaseCondition(false, scoreGab, allowrun, ChangeType.CHASE) == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>추격조 투수가 던져 여전히 지고 있는 경우");
                                pitcherChangeType = ChangeType.CHASE;
                                return true;
                            }
                        }
                    }
                }
                else if (bSetupOn[defense] == true)
                {
                    //필승 투수 
                    if (curInning >= 10)
                    {
                        ////UnityEngine.//Debug.Log("==============>>필승조 투수가 1이닝 이사을 던진경우");
                        if (scoreGab > 0)
                        {
                            ////UnityEngine.//Debug.Log("==============>>이기고 있는 경우 세이브 조건 검색");
                            //세이브 조건
                            if (checkSaveCondition(false, scoreGab) == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>이기고 있는 경우 세이브 조건 검색");
                                pitcherChangeType = ChangeType.SAVE;
                                return true;
                            }
                        }

                        if (scoreGab >= 0 && scoreGab < 5)
                        {
                            ////UnityEngine.//Debug.Log("==============>>동점이상 5점차 이하 필승조 가동 조건 검색");
                            //필승
                            if (checkSetupCondition(false, scoreGab, allowrun, ChangeType.MUST_WIN) == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>동점이상 5점차 이하 필승조 가동 조건 검색");
                                pitcherChangeType = ChangeType.MUST_WIN;
                                return true;
                            }
                        }
                        else
                        {
                            ////UnityEngine.//Debug.Log("==============>>필승조가 나왔으나 지고 있거나 5점이상으로 이기고 있는 경우");
                            //1이닝 이상을 던지고 블론을 한 경우
                            //추격 패전
                            if (checkChaseCondition(false, scoreGab, allowrun, ChangeType.MUST_WIN) == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>필승조가 나왔으나 지고 있거나 5점이상으로 이기고 있는 경우");
                                pitcherChangeType = ChangeType.CHASE;
                                return true;
                            }
                        }
                    }
                }
                else if (bLongReliefOn[defense] == true)
                {
                    //6이닝 이후인 경우 선발 조건이랑 동일시
                    if (currentInning >= 6)
                    {
                        if (scoreGab >= 0)
                        {
                            //세이브 조건
                            if (checkSaveCondition(true, scoreGab) == true)
                            {
                                ////UnityEngine.//Debug.Log("===============>>롱릴리프가 던지다가 세이브 조건시 투수 바꿈");
                                pitcherChangeType = ChangeType.SAVE;
                                return true;
                            }

                            //필승
                            if (checkSetupCondition(true, allowrun, scoreGab, ChangeType.NA) == true)
                            {
                                ////UnityEngine.//Debug.Log("===============>>롱릴리프가 던지다가 필승조 조건시 투수 바꿈");
                                pitcherChangeType = ChangeType.MUST_WIN;
                                return true;
                            }
                        }
                        else
                        {
                            //추격 패전
                            if (checkChaseCondition(true, allowrun, scoreGab, ChangeType.NA) == true)
                            {
                                ////UnityEngine.//Debug.Log("===============>>롱릴리프가 던지다가 추격조 조건시 투수 바꿈");
                                pitcherChangeType = ChangeType.CHASE;
                                return true;
                            }
                        }
                    }
                }


                //방전시 바꿈
                if (checkFatigueCase(false, scoreGab, allowrun) == true)
                {
                    if (currentInning <= 6)
                    {
                        //6회 이전
                        ////UnityEngine.//Debug.Log("===============>>7회 이전 추격조 투수 바꿈");
                        pitcherChangeType = ChangeType.CHASE;
                    }
                    else
                    {
                        //7회 이후
                        if (scoreGab >= 0 && scoreGab < 5)
                        {
                            ////UnityEngine.//Debug.Log("===============>>7회 이후 이기고 있는 경우 필승조 투수 바꿈 5점 이하");
                            pitcherChangeType = ChangeType.MUST_WIN;
                        }
                        else
                        {
                            ////UnityEngine.//Debug.Log("===============>>7회 이후 지고 있는 경우 패배조 투수 바꿈 또는 5점이상");
                            pitcherChangeType = ChangeType.CHASE;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        //승리투수 요건 체크
        private bool checkWinPitcher(int team, int index)
        {
            if (getScoreGab(team) > 0) //gameInfo.run[team] > gameInfo.run[1 - team])
            {
                if (index == sPlayer.GetStarterIndex(team) && currentInning < 6)
                {
                    //UnityEngine.Debug.Log("[투수교체]==============>>선발투수 이기고 있지만 5이닝 못채우면");
                    winPitcherIndex[team] = -1;
                    return false;
                }

                if (winPitcherIndex[team] == index)
                {
                    return true;
                }
            }
            return false;
        }

        //패전투수 요건 체크
        private bool checkLosePitcher(int team, int index)
        {
            if (getScoreGab(team) < 0) //if (gameInfo.run[team] < gameInfo.run[1 - team]) 
            {
                if (index == sPlayer.GetStarterIndex(team))
                {
                    //UnityEngine.Debug.Log("[투수교체]==============>>선발투수 지고 있을 떄 강판 되면 무조건 패전 인덱스");
                    losePitcherIndex[team] = index;
                    return true;
                }

                if (losePitcherIndex[team] == index)
                {
                    return true;
                }
            }
            return false;
        }

        //세이브 투수 요건 체크
        private bool checkSavePitcher(int team)
        {
            int gab = getScoreGab(team); //manager.nGameScore[team] - manager.nGameScore[1 - team];
            if (currentInning >= 7 && (gab > 0 && gab <= 3))
            {
                return true;
            }
            return false;
        }

        //투수가 지쳐서 바꿔야 되는지를 체크
        private bool checkFatigueCase(bool bStart, int scoreGab, int allowrun)
        {
            if (bStart == true && pitcher.getStat(Param.ST_PNP) > 110)
            {
                //투구수 110개 넘어가면 바꿔
                return true;
            }
            if (bStart == false && pitcher.getStat(Param.ST_PNP) > 50)
            {
                //투구수 50개 넘어가면 바꿔
                return true;
            }

            if (curStamina <= 10 || allowrun >= 7)  //10프로이하
            {
                if (bStart == true && currentInning >= 9 && scoreGab > 0)
                {
                    //완봉을 노리는 경우만 제외하고 방전시 무조건 바꿔
                    return false;
                }
                return true;
            }
            return false;
        }

        //마무리 조건 체크
        private bool checkSaveCondition(bool bStart, int scoreGab)
        {
            //전제조건 : 세이브 조건 충족
            //8,9회
            if (currentInning <= 7)
            {
                //////UnityEngine.//Debug.Log("================>>7이닝 이전이므로 마무리 안나옴");
                return false;        //세이브 조건 1
            }
            if (scoreGab <= 0)
            {
                //////UnityEngine.//Debug.Log("================>>점수가 뒤지고 있으므로 안나옴");
                return false;    //세이브 조건 2
            }

            int realScoreGab = (scoreGab - potentialScoreLoss());

            //////UnityEngine.//Debug.Log("===============>>본격적인 세이브 조건 체크 realScoreGab = " + realScoreGab);

            if (bStart == true)
            {
                if (getOffeseScore() == 0)
                {
                    //////UnityEngine.//Debug.Log("================>>선발이 완봉을 노리고 있으므로 마무리 안나옴");
                    return false; //조건1 불만족: 9이닝 선발이 완투 완봉을 노리지 않는 상태에서 세이브 상황
                }
                if (currentInning >= 9 && realScoreGab > 0)
                {
                    //////UnityEngine.//Debug.Log("================>>선발이 완투승을 노리고 동점주자가 루상에 없으므로 안바꿈");
                    return false;//조건2 불만족: 9이닝 완투 완봉을 노리지 않는 상태에서 루상의 주자로 인해 세이브가 된 상황
                }

                //조건1,2 만족시
                if (currentInning >= 9 && realScoreGab <= 3)
                {
                    ////UnityEngine.//Debug.Log("================>>정석적인 세이브 상황");
                    return true;    //정석적인 세이브 상황
                }
                if (currentInning == 8 && realScoreGab <= 0)
                {
                    ////UnityEngine.//Debug.Log("================>>8회 동점 주자로 인한 세이브 상황");
                    return true;    //조건3: 8회 세이브 상황 (주자가 동점을 허용할 수 있는 상황)ㄴ            
                }
            }
            else
            {
                if (currentInning >= 9 && realScoreGab <= 3)
                {
                    ////UnityEngine.//Debug.Log("================>>중계의 정석적인 세이브 상황");
                    return true;    //정석적인 세이브 상황
                }
                if (currentInning == 8 && realScoreGab <= 0)
                {
                    ////UnityEngine.//Debug.Log("================>>중계의 8회 동점 주자로 인한 세이브 상황");
                    return true;    //조건3: 8회 세이브 상황 (주자가 동점을 허용할 수 있는 상황)
                }
            }


            return false;
        }

        //롱릴리프 조건 체크
        private bool checkLongReliefCondition(bool bStart, int scoreGab, int allowrun, ChangeType type) //ChangeType type = ChangeType.NA
        {
            //5회 이전
            if (currentInning > 5)
            {
                //////UnityEngine.//Debug.Log("================>>롱릴리프 6회 이후에 나오지 않는다");
                return false;
            }

            //////UnityEngine.//Debug.Log("===============>>롱릴리프 조건 체크");
            if (bStart)
            {
                if (currentInning >= 3)
                {
                    if (scoreGab <= -5)
                    {
                        ////UnityEngine.//Debug.Log("================>>5점이상 뒤지고 있는경우 롱릴리프로 교체");
                        return true;
                    }
                    if (curStamina < 50 && allowrun >= 5)    //스태미너 50이하
                    {
                        ////UnityEngine.//Debug.Log("================>>5점이상 이기고 지침인 경우 롱릴리프로 교체");
                        return true;
                    }
                    if (curStamina < 20 && allowrun >= 3)     //스태미너 20이하
                    {
                        ////UnityEngine.//Debug.Log("================>>3점이상 이기고 스태미나 방전 있는경우 롱릴리프로 교체");
                        return true;
                    }
                }
                else if (allowrun >= 7)
                {
                    ////UnityEngine.//Debug.Log("================>>7점이상 이기고 있는경우 롱릴리프로 교체");
                    return true;
                }
            }

            return false;
        }

        //추격 조건 체크
        bool checkChaseCondition(bool bStart, int scoreGab, int allowrun, ChangeType type) //ChangeType type = ChangeType.NA
        {
            //6,7,8,9회
            if (currentInning <= 5)
            {
                //////UnityEngine.Debug.Log("====>>6회이전 패전조가 나오지 않는다");
                return false;
            }

            int realScoreGab = (scoreGab - potentialScoreLoss());
            
            if (bStart)
            {
                if ((curStamina < 25 || allowrun >= 3) && scoreGab < 0) //스태미너 25이하
                {
                    //조건3: 6이닝 이후 게임이 지고 있는 경우 선발 체력이 매우 지침이 된 경우
                    ////UnityEngine.Debug.Log("====>>6이닝 이후 게임이 지고 있는 경우 선발 체력이 매우 지침 패전조 출격");
                    return true;
                }

                if ((curStamina < 25 || allowrun >= 3) && scoreGab >= 5 && getOffeseScore() != 0) //스태미너 25이하
                {
                    //조건4: 6이닝 이후 5점차 이상으로 이기고 있는 경우 선발의 체력이 매우지침 & 완투 완봉을 노리는 상태가 아닌 경우
                    ////UnityEngine.Debug.Log("====>>6이닝 이후 선발이 완봉을 노리고 있지 않으면서 게임을 5점차이상으로 이기고 있는 경우 선발 체력이 매우 지침 패전조 출격");
                    return true;
                }

                if (realScoreGab <= -3)
                {
                    //조건1: 6이닝 이후 체력과 관계없이 선발이 3점차 이상으로 경기를 뒤지고 있는 경우
                    //조건2: 6이닝 이후 체력과 관계없이 선발이 3점차 이상으로 지게 만들 주자를 루상에 허용한 경우
                    ////UnityEngine.Debug.Log("====>>6이닝 이후 체력과 관계없이 선발이 (주자포함) 3점차 이상으로 뒤지고 있는 경우 패전조 출격");
                    return true;
                }

                if ((curStamina < 25 || allowrun >= 3) && realScoreGab < 0 && currentInning == 6 && getOffeseScore() != 0) //스태미너 25이하
                {
                    //조건5: 정확히 6이닝 이기고 있으나 역전주자 허용 스태미너 레이트 0.25이하 완봉조건이 아닌경우
                    ////UnityEngine.Debug.Log("====>>정확히 6이닝 이기고 있으나 역전주자 허용 스태미너 레이트 0.2이하 완봉조건이 아닌경우 패전조 출격");
                    return true;
                }

            }
            else
            {
                if (type == ChangeType.SAVE)
                {
                    //세이브 투수에 이은 또는 필승조 투수에 이은
                    ////UnityEngine.Debug.Log("====>>마무리 블론시 역전당하면 바로 패배조 투입");
                    return true;
                }
                else if (type == ChangeType.MUST_WIN)
                {
                    //필승조에 이은
                    if (curStamina < 50 || allowrun >= 2)    //50이하
                    {
                        ////UnityEngine.Debug.Log("====>>역전당하면 승리조의 체력은 50%소진후");
                        return true;
                    }
                }
                else //if (type == ChangeType.CHASE)
                {
                    if (curStamina < 30 || allowrun >= 3)
                    {
                        ////UnityEngine.Debug.Log("====>>여전히 지고 있는 경우 70프로 소진후");
                        return true;
                    }
                }

            }

            return false;
        }

        //셋업 조건 체크
        bool checkSetupCondition(bool bStart, int scoreGab, int allowrun, ChangeType type) //ChangeType type = ChangeType.NA
        {
            //6,7,8,9회
            if (currentInning <= 5)
            {
                //////UnityEngine.Debug.Log("====>>6회이전 승리조가 나오지 않는다");
                return false;
            }

            if (scoreGab >= 5 || scoreGab < 0)
            {
                //////UnityEngine.Debug.Log("====>>5점이상으로 이기고 있거나 지고 있는경우 승리조는 나오지 않는다.");
                return false;
            }

            int realScoreGab = (scoreGab - potentialScoreLoss());
                
            if (bStart)
            {
                if (currentInning == 7)
                {
                    //7이닝 조건
                    if (curStamina < 25 || allowrun >= 3)   //스태미너 25이하
                    {
                        //조건3: 7이닝에 선발 체력이 매우 지침이된 경우
                        ////UnityEngine.Debug.Log("====>>7이닝에 선발 체력이 매우 지침이된 경우 승리조 출격");
                        return true;
                    }
                    if ((curStamina < 40 || allowrun >= 3) && realScoreGab <= 0) //스태미너 40이하
                    {
                        //조건3: 7이닝에 선발 체력이 매우 지침이된 경우
                        ////UnityEngine.Debug.Log("====>>7이닝에 선발 체력이 지침이 되고 동점주자를 허용한 경우 승리조 출격");
                        return true;
                    }
                }
                else if (currentInning == 8)
                {
                    //8이닝 조건
                    if ((curStamina < 25 || allowrun >= 3) && getOffeseScore() != 0)    //스태미너 25이하
                    {
                        //조건5: 8이닝에 선발이 완봉을 노리지 않는 상태에서 체력이 매우 지침이 된 경우 (2순위 구원)
                        ////UnityEngine.Debug.Log("====>>8이닝에 선발이 완봉을 노리지 않는 상태에서 체력이 매우 지침이 된 경우 승리조 출격");
                        return true;
                    }
                    if ((curStamina < 40 || allowrun >= 3) && getOffeseScore() != 0 && realScoreGab <= 0)    //스태미너 40이하
                    {
                        //조건6: 8이닝에 선발이 완봉을 노리지 않는 상태에서 체력이 지침 이하이며 루상에 동점주자를 허용한 경우 (2순위 구원)
                        ////UnityEngine.Debug.Log("====>>8이닝에 선발이 완봉을 노리지 않는 상태에서 체력이 지침 이하이며 루상에 동점주자를 허용한 경우 승리조 출격");
                        return true;
                    }
                }
                else if (currentInning >= 9)
                {
                    //9이닝 이후
                    if ((curStamina < 10 || allowrun >= 3) && getOffeseScore() != 0 && scoreGab > 3) //스태미너 10이하
                    {
                        //조건7: 9이닝에 세이브 조건이 아니며 완봉을 노리지 않는 상태에서 체력이 방전된 경우
                        ////UnityEngine.Debug.Log("====>>9이닝에 세이브 조건이 아니며 완봉을 노리지 않는 상태에서 체력이 방전된 경우 경우 승리조 출격");
                        return true;
                    }
                }

                if ((curStamina < 25 || allowrun >= 3) && realScoreGab < 0 && currentInning == 7 && getOffeseScore() != 0) //스태미너 25이하
                {
                    //조건8: 정확히 7이닝 이기고 있으나 역전주자 허용 스태미너 레이트 0.2이하 완봉조건이 아닌경우
                    ////UnityEngine.Debug.Log("====>>정확히 7이닝 이기고 있으나 역전주자 허용 스태미너 레이트 0.2이하 완봉조건이 아닌경우 승리조 출격");
                    return true;
                }

            }
            else
            {
                if (type == ChangeType.SAVE)
                {
                    //세이브 투수에 이은 또는 필승조 투수에 이은
                    ////UnityEngine.Debug.Log("====>>마무리 블론시 최소 동점이면 바로 승리조 투입");
                    return true;
                }
                else if (type == ChangeType.MUST_WIN)
                {
                    //필승조에 이은
                    ////UnityEngine.Debug.Log("====>>다음 승리조 투입");
                    return true;
                }
                else// if (type == ChangeType.CHASE)
                {
                    ////UnityEngine.Debug.Log("====>>추격시 역전시 승리조 투입");
                    return true;
                }
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //타자교체
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //타자가 바뀌는지를 체크하고 바뀐경우 true를 리턴
        private bool checkBatterChanged()
        {
            int team = curIndex;
            int inPlayer = 0;
            int currentOrder = sPlayer.GetLineupCount(team);


            if (changeBatterCase(team) == true)
            {
                CPlayer nextBatter = sPlayer.GetBatter(curIndex);
                inPlayer = getChangeBatterIndex(team, nextBatter);
                if (inPlayer != -1)
                {
                    ////UnityEngine.//Debug.Log("==================================================================================>>" + nextBatter.getName() + " 타석에 대타 " + sPlayer.GetFielder(team, inPlayer,false).getName());                    
                    changerIndex[(int)ChangerIndex.OutBatter] = inPlayer;// outBatter = inPlayer;
                    changePlayer(team, inPlayer, nextBatter.getOrder(), 100);
                    return true;
                }
            }

            return false;
        }

        //대타 가능한 벤치멘버 인덱스 가져오기
        private int getChangeBatterIndex(int team, CPlayer curBatter)
        {
            int index = -1;
            int curBatterPos = curBatter.getPosition();

            bool bSimilarPosition = checkSimilarPositionOnBench(team, curBatterPos);

#if CHANGE_BATTER_TEST
            bSimilarPosition = true;
#endif
            if (bSimilarPosition == true)
            {
                bool bLeftPitcher = (pitcher.getThrowHand() == CPlayer._LEFTHAND ? true : false);
                int lastValue = curBatter.getBatterPowerValue(bLeftPitcher);

                for (int i = SimulPlayer.NUM_BATTER; i < SimulPlayer.NUM_FIELDER; i++)
                {
                    if (sPlayer.GetFielderOut(team, i, false) == false)
                    {
                        CPlayer benchBatter = sPlayer.GetFielder(team, i, false);
#if CHANGE_BATTER_TEST                        
                        return i;
#else
                        int curValue = benchBatter.getBatterPowerValue(bLeftPitcher);
                        if (curValue > lastValue)
                        {
                            ////UnityEngine.//Debug.Log("==========================>>>"+benchBatter.getName()+" 대타가능");
                            lastValue = curValue;
                            index = i;
                        }
#endif
                    }
                }
            }

            return index;
        }

        //대타 여부 - true리턴시 대타
        private bool changeBatterCase(int team)
        {
#if CHANGE_BATTER_TEST
            if (currentInning ==1 && sPlayer.GetLineupCount(team) == 2)
                return true;
            else return false;
#else
            if (currentInning >= SimulParm.BATTER_CHANGE_INNING)
            {
                int gab = getScoreGab(team);

                if (currentInning <= 7)
                {
                    //7이닝 이전
                    if (gab > 0 && gab <= 2)
                    {
                        //2점차 이내로 이기고 있는 경우 대타 고려
                        return true;
                    }
                    else if (gab < 0)
                    {
                        //지고 있는 경우 대타 고려
                        return true;
                    }
                }
                else
                {
                    //8이닝 이후
                    if (gab <= 0)
                    {
                        //지거나 비기고 있는 경우 대타 고려
                        return true;
                    }
                }
            }
            return false;
#endif
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //주자교체
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //주자가 바뀌는지를 체크하고 바뀐경우 true를 리턴
        private bool runnerChangeNeedCheck(int bIndex)
        {
            int index = getRunnerIndex(bIndex);
            if (index != -1)
            {
                //UnityEngine.Debug.Log("[bChangedRunner 체크]=======================================>>bChangedRunner: " + runner[index].bChangedRunner +" index = "+index);
                if (runner[index].bChangedRunner == false)
                {
                    int team = curIndex;
                    int inPlayer = 0;

                    if (changeRunnerCase(team, bIndex) == true)
                    {
                        CPlayer curRunner = runner[index].getRunner();
                        inPlayer = getChangeRunnerIndex(team, curRunner);
                        if (inPlayer != -1)
                        {
#if _Test_Local
                            runner[index].runner = sPlayer.GetFielder(team, inPlayer, false);
                            changerIndex[(int)ChangerIndex.InRunner] = index;  // firstBase = index;
               
#else
                            changerIndex[(int)ChangerIndex.InRunner] = SimulPlayerManager.GetFielderIndexFromCard(team, curRunner.getCard().cardSeq);                            
#endif

                            runner[index].bChangedRunner = true;
                            changerIndex[(int)ChangerIndex.OutRunner] = inPlayer;// outRunner = inPlayer;
                            changePlayer(team, inPlayer, curRunner.getOrder(), 200);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //벤치 상태 고려 주자 교체가 가능한경우
        private bool changeRunnerCase(int team, int bIndex)
        {
#if CHANGE_RUNNER_TEST
            return true;
#else
            if (currentInning >= SimulParm.RUNNER_CHANGE_INNING)
            {
                int gab = getScoreGab(team);
                if (Mathf.Abs(gab) <= 2)
                {
                    //2점 이내
                    return true;
                }
            }
            return false;
#endif
        }

        //대주자로 교체가능한 벤치 멤버  인덱스 가져오기
        private int getChangeRunnerIndex(int team, CPlayer curRunner)
        {
            int index = -1;
            int curRunnerPos = curRunner.getPosition();

#if CHANGE_RUNNER_TEST
            bool bSimilarPosition = true;
#else
            bool bSimilarPosition = checkSimilarPositionOnBench(team, curRunnerPos);
#endif

            if (bSimilarPosition == true)
            {
                int lastValue = curRunner.getRunnerPowerValue();

                for (int i = SimulPlayer.NUM_BATTER; i < SimulPlayer.NUM_FIELDER; i++)
                {
                    if (sPlayer.GetFielderOut(team, i, false) == false)
                    {
#if CHANGE_RUNNER_TEST
                        return i;
#else
                        CPlayer benchRunner = sPlayer.GetFielder(team, i, false);
                        int benchValue = benchRunner.getRunnerPowerValue();

                        if (benchValue > lastValue && benchValue >= 50000)
                        {
                            //주자 능력치가 300이하거나 벤치플레이어랑 능력치가 400이상 차이
                            if (curRunner.getRunnerPowerValue() < 30000 || (benchValue - curRunner.getRunnerPowerValue()) > 30000)
                            {
                                //UnityEngine.//Debug.Log("==========================>>>" + benchRunner.getName() + " 대주가능" + "=============>>index = " + index + " benchValue = " + benchValue);
                                lastValue = benchValue;
                                index = i;
                            }
                        }
#endif
                    }
                }
            }

            return index;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //수비교체
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //수비 교체의 필요성을 체크하고 필요한 경우 해당 수비수를 교체
        private void fielderChangeNeedCheck(int team, int fIndex)
        {
            ////UnityEngine.//Debug.Log("===========================>>>" + fielder[fIndex].getFielder().getName() + "  " + fielder[fIndex].getFielder().getPosition() + "   " + fIndex + "=========>>>수비교체의 필요성을 검사");
            int position = fielder[fIndex].getFielder().getPosition();
            int curPosition = fIndex;

            if ( position != fIndex) //포지션이 다른경우
            {
                if (checkSimilarPosition(position, curPosition) == false) //유사포지션도 아닌경우
                {
                    //UnityEngine.//Debug.Log("===========================>>>" + fielder[fIndex].getFielder().getName() + "  " + fielder[fIndex].getFielder().getPosition() + "   " + fIndex + " 포지션 수비교체의 필요!!");
                    ////UnityEngine.//Debug.Log("===========================>>>" + fielder[fIndex].getFielder().getOrder());
                    int order = fielder[fIndex].getFielder().getOrder();
                    changeFielder(team, fIndex, order);
                }
            }
        }

        //벤치 상태 고려 수비수 교체
        private void changeFielder(int team, int fIndex, int order)
        {
            int samePosition = getChangeFielderIndex(team, fIndex, true);
            if (samePosition != -1)
            {
                //같은 포지션 야수시
                ////UnityEngine.//Debug.Log("===========================>>>같은 포지션 야수로 교체 ORDER " + order);
                changerIndex[(int)ChangerIndex.OutFielder] = samePosition; // outFielder = samePosition;
                changerIndex[(int)ChangerIndex.InFielder] = order;// inFielder = order;
                ////UnityEngine.//Debug.Log("===========================>>>" + sPlayer.GetFielder(team, samePosition, false).getName() + " 선수로 수비교체!");
                changePlayer(team, samePosition, order, 300);
                return;
            }

            int similarPosition = getChangeFielderIndex(team, fIndex, false);
            if (similarPosition != -1)
            {
                //비슷한 포지션 야수시
                ////UnityEngine.//Debug.Log("===========================>>>유사 포지션 야수로 교체");
                changerIndex[(int)ChangerIndex.OutFielder] = samePosition; //outFielder = similarPosition;
                changerIndex[(int)ChangerIndex.InFielder] = order;// inFielder = order;
                ////UnityEngine.//Debug.Log("===========================>>>" + sPlayer.GetFielder(team, similarPosition, false).getName() + " 선수로 수비교체!");
                changePlayer(team, similarPosition, order, 300);
                return;
            }

            ////UnityEngine.//Debug.Log("===========================>>>바꿀 야수가 없어 그대로 속행함");
        }

        //바꿀 야수의 인덱스를 가져온다
        private int getChangeFielderIndex(int team, int fIndex, bool bSamePosition)
        {
            for (int i = SimulPlayer.NUM_BATTER; i < SimulPlayer.NUM_FIELDER; i++)
            {
                if (sPlayer.GetFielderOut(team, i, false) == false)
                {
                    int pos = sPlayer.GetFielder(team, i, false).getPosition();
                    if (bSamePosition == true)
                    {
                        if (pos == fIndex)
                        {
                            //UnityEngine.Debug.Log("[동일포지션]===============>>>>" + sPlayer.GetFielder(team, i).getName() + " 수비 :" + sPlayer.GetFielder(team, i).getFielding() + " 송구 :" + sPlayer.GetFielder(team, i).getThrowing());
                            return i;
                        }
                    }
                    else
                    {
                        if (checkSimilarPosition(pos, fIndex) == true)
                        {
                            //UnityEngine.Debug.Log("[유사포지션]=============>>>>" + sPlayer.GetFielder(team, i).getName());
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //교체관련 메쏘드
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void changePlayer(int team, int inPlayer, int outPlayer, int changeType)
        {
            ////Debug.Log("==================>>야수 교체");
            //changeType
            //100대타, 200대주, 300대수
            sPlayer.SetFielderChange(team, inPlayer, outPlayer, changeType);

            /*
            for (int i = 0; i < 14; i++)
            {
                CPlayer batter = sPlayer.GetFielder(team, i);
                ////UnityEngine.//Debug.Log("====================>> " + batter.getName() + "  오더 " + batter.getOrder() + " 출전여부: " + sPlayer.GetFielderOut(team, i));
            }*/
        }

        //비교되는 두 포지션이 유사 포지션인 여부
        private bool checkSimilarPosition(int pos1, int pos2)
        {
            if (pos1 >= CPlayer._LEFTFIELDER)
            {
                if (pos2 >= CPlayer._LEFTFIELDER) return true;
            }
            else if (pos1 >= CPlayer._FIRSTBASEMAN)
            {
                if (pos2 >= CPlayer._FIRSTBASEMAN && pos2 < CPlayer._LEFTFIELDER) return true;
            }
            else if (pos1 >= CPlayer._CATCHER)
            {
                if (pos2 == CPlayer._CATCHER) return true;
            }

            return false;
        }

        //벤치에 유사 포지션이 벤치에 있는지 여부
        private bool checkSimilarPositionOnBench(int team, int currentPosition)
        {
            for (int i = SimulPlayer.NUM_BATTER; i < SimulPlayer.NUM_FIELDER; i++)
            {
                if (sPlayer.GetFielderOut(team, i, false) == false)
                {
                    if (checkSimilarPosition(sPlayer.GetFielder(team, i, false).getPosition(), currentPosition) == true)
                    {
                        ////UnityEngine.//Debug.Log("==========================>>>벤치에 유사포지션이 있음");
                        return true;
                    }
                }
            }
            return false;
        }



        private int getEyeValueSimul()
        {
            float value = (batter.getEye() + bBonus);// *1.1f;
            return (int)value;
        }

        private int getContactValueSimul()
        {
            float value = (batter.getContact() + bBonus);//
            return (int)value;
        }

        private int getPowerValueSimul()
        {
            float value = (batter.getPower() + bBonus);
            return (int)value;
        }



        ////////////////////////////////////////////////////////////////////////////
        //스킬 체크 엔진
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 해당 스킬 사용 회수
        /// </summary>
        private Dictionary<SkillID, int>[] skillUseCount = new Dictionary<SkillID, int>[2];

        
        /// <summary>
        /// 한타석에 적용되는 투수와 타자의 스킬 
        /// </summary>
        private CSkill curPitcherSkill, curBatterSkill;
        private CSkill pitchPitcherSkill, pitchBatterSkill, pitchCatcherSkill;

        /// <summary>
        /// 대결 스킬이 타자가 이긴경우
        /// </summary>
        private bool vsBatterWin;

        /// <summary>
        /// 스킬 발동 조건
        /// </summary>
        private bool bCrisisState, bScoringPosition, bNoRunner;
        private bool bInningOnceCheck;
        private int defensScoreGab, offenseScoreGab;

        /// <summary>
        /// 피치 검색 전용 -> 속도 늘리기 위한
        /// </summary>
        private List<SkillIndex> searchPitchSkill = new List<SkillIndex>();


        /// <summary>
        /// 각각의 스킬 최초 사용수 초기화
        /// </summary>
        private void initSkillUseCount()
        {
#if _Test_Local
            //맵초기화 -> 나중에 지워
            SimulParm.InitSkillMap(null);
#endif

            //피치에서 사용할 리스트 클리어
            searchPitchSkill.Clear();
            foreach (var item in (SkillIndex[])Enum.GetValues(typeof(SkillIndex)))
            {
                SkillID primaryID = SimulParm.GetPrimaryID_FromSkillEffect(item);
                skillEffectMap info = SimulParm.GetSkillInfo((int)primaryID);
                if (info.invokeCondition == Effect_InvokeCondition.PitchStart)
                {
                    searchPitchSkill.Add(item);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                skillUseCount[i] = new Dictionary<SkillID, int>();
                foreach (var item in (SkillID[])Enum.GetValues(typeof(SkillID)))
                {
                    if (item != SkillID.None)
                    {
                        skillUseCount[i].Add(item, 0);
                    }
                }
            }

            ////Debug.Log("==============>>스킬 카운트 초기화 완료 test = " + test);
        }

        /// <summary>
        /// 매이닝 초기화 해야 하는 스킬 사용 카운트
        /// </summary>
        private void initSkillCountEveryInning()
        {
            ////Debug.Log("==============>>매이닝 스킬 초기화");

            if (pitcher != null)
            {
                //투수 매이닝 발동기간 끝난 스킬 초기화
                List<CSkill> curPitcherSkillList = pitcher.getSkillList();
                if (curPitcherSkillList.Count > 0)
                {
                    for (int i = curPitcherSkillList.Count - 1; i >= 0; i--)
                    {
                        int id = curPitcherSkillList[i].ID;
                        SkillIndex effectIndex = curPitcherSkillList[i].effectIndex;
                        if (SimulParm.GetSkillInfo((int)id).effectValidity == Effect_Validity.InningEnd)
                        {
                            //Debug.Log("유효기간 이닝인 [" + SimulParm.GetSkillInfo(id).skillName + "] 투수 스킬 초기화");
                            pitcher.setPitcherBonus(effectIndex, 0); //해당 구위 초기화
                            curPitcherSkillList.Remove(curPitcherSkillList[i]);
                        }
                    }
                }
            }
            //타자는 필요없음

          


            ////Debug.Log("==============>>매이닝 스킬 카운트 초기화");
            for (int i = 0; i < 2; i++)
            {
                foreach (var item in (SkillID[])Enum.GetValues(typeof(SkillID)))
                {
                    if (item != SkillID.None)
                    {
                        int id = (int)item;
                        if (SimulParm.GetSkillInfo(id).restriction == Restriction_Type.Inning)
                        {
                            skillUseCount[i][item] = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 카운트된 스킬을 하나 뒤로 돌림 -> 무효화 되었을 때 사용
        /// </summary>
        private void resetSkillCount(int index, SkillID skill)
        {
            if (skillUseCount[index].ContainsKey(skill) == true)
            {
                if (skillUseCount[index][skill] > 0)
                {
                    skillUseCount[index][skill]--;
                }
            }
        }

        /// <summary>
        /// 매 배팅 초기화 해야 하는 스킬 카운트
        /// </summary>
        private void initSkillCountEveryBatting()
        {
            ////Debug.Log("==============>>매타석 스킬 초기화");

            //투수 매타석 발동기간 끝난 스킬 초기화
            if (pitcher != null)
            {
                List<CSkill> curPitcherSkillList = pitcher.getSkillList();
                if (curPitcherSkillList.Count > 0)
                {
                    for (int i = curPitcherSkillList.Count - 1; i >= 0; i--)
                    {
                        int id = curPitcherSkillList[i].ID;
                        SkillIndex effectIndex = curPitcherSkillList[i].effectIndex;
                        if (SimulParm.GetSkillInfo((int)id).effectValidity == Effect_Validity.BattingEnd)
                        {
                            //Debug.Log("유효기간 타석인 [" + SimulParm.GetSkillInfo(id).skillName + "] 투수 스킬 초기화");
                            pitcher.setPitcherBonus(effectIndex, 0); //해당 구위 초기화
                            curPitcherSkillList.Remove(curPitcherSkillList[i]);

                            if (effectIndex == SkillIndex.Charisma)
                            {
                                //Debug.Log("카리스마 예외처리 -> 야수능력치 원래대로");
                                for (int kk = 1; kk < 9; kk++) fielder[kk].fielder.setFieldBounsValue(0);
                            }
                        }
                    }
                }
            }

            //타자 매타석 발동기간 끝난 스킬 초기화
            if (batter != null)
            {
                List<CSkill> curBatterSkillList = batter.getSkillList();
                if (curBatterSkillList.Count > 0)
                {
                    for (int i = curBatterSkillList.Count - 1; i >= 0; i--)
                    {
                        int id = curBatterSkillList[i].ID;
                        SkillIndex effectIndex = curBatterSkillList[i].effectIndex;
                        Effect_Validity validity = SimulParm.GetSkillInfo((int)id).effectValidity;
                        if (validity == Effect_Validity.BattingEnd || validity == Effect_Validity.InningEnd)
                        {
                            //타자능력치 초기화
                            //Debug.Log("유효기간 타석인 [" + SimulParm.GetSkillInfo(id).skillName + "] 타자 스킬 초기화");
                            curBatterSkillList.Remove(curBatterSkillList[i]);
                        }
                    }
                }
            }

            //양팀다            
            for (int i = 0; i < 2; i++)
            {
                foreach (var item in (SkillID[])Enum.GetValues(typeof(SkillID)))
                {
                    if (item != SkillID.None)
                    {
                        int id = (int)item;
                        if (SimulParm.GetSkillInfo(id).restriction == Restriction_Type.Batter)
                        {
                            skillUseCount[i][item] = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 스킬 발동 가능한지 체크
        /// </summary>
        /// <param name="player">스킬 시전 플레이어</param>
        /// <param name="bPitcher">투수 여부</param>
        /// <returns>시전 스킬 리턴</returns>
        private CSkill checkSkill(CPlayer player, bool bPitcher)
        {
            int team = (bPitcher ? (1 - curIndex) : (curIndex));

            List<CSkill> curAvailableSkill = new List<CSkill>();
            curAvailableSkill.Clear();

            foreach (var item in (SkillIndex[])Enum.GetValues(typeof(SkillIndex)))
            {
                SkillID primaryID = SimulParm.GetPrimaryID_FromSkillEffect(item);
                if (checkSkillCondition(primaryID) == true)
                {
                    CSkill curSkill = player.bvSkillSuccess(item, skillUseCount[team]);
                    if (curSkill != null)
                    {
                        //Debug.Log((bPitcher ? "투수 " : "타자 ") + curSkill.skillName + " 스킬 발동가능 리스트에 추가");
                        curAvailableSkill.Add(curSkill);
                    }
                }
            }

            int count = curAvailableSkill.Count;
            if (count > 0)
            {
                int index = UnityEngine.Random.Range(0, count);
                CSkill usedSkill = curAvailableSkill[index];
                skillUseCount[team][(SkillID)usedSkill.ID]++;
                int effectValue = usedSkill.effectValue;
                if (bPitcher == true)
                {
                    if (usedSkill.effectIndex == SkillIndex.Charisma)
                    {
                        //Debug.Log("카리스마 예외처리 -> 야수능력치 UP");
                        for (int i = 1; i < 9; i++)
                        {
                            fielder[i].fielder.setFieldBounsValue(effectValue);
                        }
                    }
                    else
                    {
                        if (usedSkill.effectIndex == SkillIndex.FrameFight)
                        {
                            //Debug.Log("불꽃 투혼 처리 -> 핀치복구");
                            setPinchInit();
                        }

                        if (effectValue > 0)
                        {
                            //해당스킬의 보너스값 설정
                            //Debug.Log("버프를 얻는 경우");
                            player.setPitcherBonus(usedSkill.effectIndex, effectValue);
                        }
                        else if (effectValue < 0)
                        {
                            //음수인 경우 상대의 디버프값 설정
                            //Debug.Log("디버프를 거는 경우");
                            batter.setDebuffValue(effectValue);
                        }
                    }
                }
                else
                {
                    //해당 효과를 뽑아내서 더해줘
                    if (effectValue > 0)
                    {
                        //해당스킬의 보너스값 설정
                        //Debug.Log("버프를 얻는 경우");
                        player.setBonusValue(effectValue);
                    }
                    else if (effectValue < 0)
                    {
                        //음수인 경우 상대의 디버프값 설정
                        //Debug.Log("디버프를 거는 경우");
                        pitcher.setDebuffValue(effectValue);
                    }
                }
                player.getSkillList().Add(usedSkill);
                //Debug.Log((bPitcher ? "투수" : "타자") + " [" + SimulParm.GetSkillInfo(usedSkill.ID).skillName + "] 스킬 발동!!!");
                return usedSkill;
            }
            else
            {
                return null;
            }
        }

        
        /// <summary>
        /// 해당 스킬 발동 제약 조건 검색
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true리턴 시 사용가능</returns>
        private bool checkSkillCondition(SkillID item)
        {
            Effect_InvokeCondition invokeCondition = SimulParm.GetSkillInfo((int)item).invokeCondition;

            if (invokeCondition == Effect_InvokeCondition.ScoringPosition)
            {
                //스코어링 포지션
                return bScoringPosition;                
            }
            else if (invokeCondition == Effect_InvokeCondition.Crisis) 
            {
                //위기
                return bCrisisState;                
            }
            else if (invokeCondition == Effect_InvokeCondition.NoRunner)
            {
                //주자없는 경우
                return bNoRunner;
            }
            else if (invokeCondition == Effect_InvokeCondition.InningStart)
            {
                //이닝시작에 체크
                return bInningOnceCheck;
            }
            else if (invokeCondition == Effect_InvokeCondition.BattingStart)
            {
                if (item == SkillID.chu_gyeog_bon_neung) //추격 본능
                {
                    if (defensScoreGab < 0 && defensScoreGab >= -2)
                    {
                        //2점차로 지고 있는 경우
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (item == SkillID.chrisma)
                {
                    if (defensScoreGab <= -3)
                    {
                        //3점차 이상으로 지고 있을때
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //그밖의 케이스는 여기서 취급 안함
                return false;
            }
            
        }



        /// <summary>
        /// 해당 스텝에 발생한 스킬을 맵핑
        /// </summary>
        /// <param name="step"></param>
        /// <param name="offense"></param>
        /// <param name="defense"></param>
        /// <param name="result"></param>
        private void setUseSkillFlag(SkillUseStep step, CSkill offense, CSkill defense, CSkill catcher, VsResult result)
        {
            SimulSkillInfo info = new SimulSkillInfo();
            info.bAvailable = false;
            if (offense != null)
            {
                info.setOffense(offense.ID, offense.rank);
            }

            if (defense != null)
            {
                info.setDefense(defense.ID, defense.rank);
            }

            if (catcher != null)
            {
                info.setCatcher(catcher.ID, catcher.rank);
            }

            if (info.bAvailable == true)
            {
                if (curBattingSkillInfo.ContainsKey(step) == false)
                {
                    info.vsType = result;
                    curBattingSkillInfo.Add(step, info);
                }
            }
        }

        /// <summary>
        /// 투수와 타자가 대결할때 스킬 검색 및 발동 체크
        /// </summary>
        /// <returns>카운터 스킬 발생시 true리턴</returns>
        private bool checkBattingviewSkill()
        {
            ////Debug.Log("=================================>>> 다음 타석 스킬 대결");
            curPitcherSkill = null;
            curBatterSkill = null;
                        
            int team = curIndex;

            curPitcherSkill = checkSkill(pitcher, true);
            //if (curPitcherSkill != null) //Debug.Log("=================================>>> 발동된 투수 스킬 = " + curPitcherSkill.skillName);

            if (curPitcherSkill != null)
            {
                //투수 스킬에 대한 카운터 스킬이 있는경우 카운터 스킬 우선 검색
                List<int?> counter = SimulParm.GetSkillInfo(curPitcherSkill.ID).counter;
                if (counter != null)
                {
                    for(int i=0; i< counter.Count ;i++)
                    {
                        SkillIndex item = SimulParm.GetSkillEffect((int)counter[i]);
                        SkillID primaryID = SimulParm.GetPrimaryID_FromSkillEffect(item);
                        if (checkSkillCondition(primaryID) == true)
                        {
                            CSkill curSkill = batter.bvSkillSuccess(item, skillUseCount[team]);
                            if (curSkill != null)
                            {
                                curBatterSkill = curSkill;
                                skillUseCount[team][(SkillID)curSkill.ID]++;
                                //Debug.Log("타자 카운터로 " + " [" + SimulParm.GetSkillInfo(curBatterSkill.ID).skillName + "] 스킬 발동!!!");

                                //투타 대결
                                vsBatterWin = SimulParm.checkOffenseSkillWin(curBatterSkill.rank, curPitcherSkill.rank);

                                if (vsBatterWin == true)
                                {
                                    //타자가 이긴경우
                                    batter.getSkillList().Add(curBatterSkill);      //리스트에 추가해주고
                                    int bonusValue = curBatterSkill.effectValue; 
                                    if (bonusValue > 0)
                                    {
                                        //Debug.Log("버프를 얻는 경우");
                                        batter.setBonusValue(bonusValue);
                                        batter.setDebuffValue(0);
                                    }
                                    else
                                    {
                                        //Debug.Log("디버프를 거는 경우");
                                        pitcher.setDebuffValue(bonusValue);
                                    }
                                    pitcher.setPitcherBonus(curPitcherSkill.effectIndex, 0);
                                    pitcher.getSkillList().Remove(curPitcherSkill); //투수는 제거
                                }
                                else
                                {
                                    //투수가 이긴경우                                 
                                }

                                if (bAutoMode == true)
                                {
                                    //오토모드(시뮬레이션모드)에서만 참조한다
                                    setUseSkillFlag(SkillUseStep.BattingView, curBatterSkill, curPitcherSkill, null, vsBatterWin ? VsResult.OffenseWin : VsResult.DefenseWin);
                                }

                                ////Debug.Log("=================================>>> 카운터로 발동된 타자 스킬 = " + curBatterSkill.skillName);
                                bInningOnceCheck = false;
                                return true;
                            }
                        }
                    }
                }
            }

            //카운터 스킬 미발생시 타자 스킬
            curBatterSkill = checkSkill(batter, false);

            if (bAutoMode == true)
            {
                //오토모드(시뮬레이션모드)에서만 참조한다
                setUseSkillFlag(SkillUseStep.BattingView, curBatterSkill, curPitcherSkill, null, VsResult.None);
            }

            //if(curBatterSkill != null) //Debug.Log("=================================>>> 발동된 타자 스킬 = " + curBatterSkill.skillName);
            bInningOnceCheck = false;
            return false;
        }


        /// <summary>
        /// 투수 혹은 타자가 공을 던질때 발생하는 스킬
        /// </summary>
        private void checkSkillByPitch()
        {
            List<CSkill> tempBatter = new List<CSkill>();
            List<CSkill> tempCatcher = new List<CSkill>();
            List<CSkill> tempPitcher = new List<CSkill>();

            pitchPitcherSkill = null;
            pitchBatterSkill = null;
            pitchCatcherSkill = null;

            //타자 피치시 스킬 발동 가능 여부
            bool bBatterPossible = true;  
            if (curBatterSkill != null)
            {
                if (curBatterSkill.effectIndex == SkillIndex.Unexpected ||
                    curBatterSkill.effectIndex == SkillIndex.AssaultBall ||
                    curBatterSkill.effectIndex == SkillIndex.GodOfBunt)
                {
                    //뜬금포, 강습타구, 번트의신 효과 발동시 무효처리
                    bBatterPossible = false;
                }
            }

            CPlayer catcher = fielder[CPlayer._CATCHER].getFielder();
            int count = searchPitchSkill.Count;
            
            for(int i = 0 ; i < count ; i++)
            {                   
                SkillIndex effectIndex = searchPitchSkill[i];
                if (effectIndex == SkillIndex.CatcherProvoke)  //미트질은 시뮬레이션 모드에서 제외
                {
                    //포수전용
                    CSkill curSkill = catcher.bvSkillSuccess(effectIndex, skillUseCount[1 - curIndex]);
                    if (curSkill != null)
                    {
                        //Debug.Log((bPitcher ? "투수 " : "타자 ") + curSkill.skillName + " 스킬 발동가능 리스트에 추가");
                        tempCatcher.Add(curSkill);                        
                    }
                }
                else
                {
                    if (effectIndex < SkillIndex.SpecialCatch)
                    {
                        //투수전용
                        CSkill curSkill = pitcher.bvSkillSuccess(effectIndex, skillUseCount[1 - curIndex]);
                        if (curSkill != null)
                        {
                            //Debug.Log((bPitcher ? "투수 " : "타자 ") + curSkill.skillName + " 스킬 발동가능 리스트에 추가");
                            tempPitcher.Add(curSkill);                            
                        }
                    }
                    else
                    {
                        if (effectIndex == SkillIndex.CatcherProvoke || effectIndex == SkillIndex.CatcherMeatJil)  //미트질은 시뮬레이션 모드에서 제외
                        {
                            //포수스킬 제외
                        }
                        else
                        {
                            if (bBatterPossible == true)
                            {
                                //타자전용
                                CSkill curSkill = batter.bvSkillSuccess(effectIndex, skillUseCount[curIndex]);
                                if (curSkill != null)
                                {
                                    //Debug.Log((bPitcher ? "투수 " : "타자 ") + curSkill.skillName + " 스킬 발동가능 리스트에 추가");
                                    tempBatter.Add(curSkill);
                                }
                            }
                        }
                    }
                }
            }

            if (tempCatcher.Count > 0)
            {
                //피치시 발생하는 포수 스킬
                pitchCatcherSkill = tempCatcher[UnityEngine.Random.Range(0, tempCatcher.Count)];
                skillUseCount[1 - curIndex][(SkillID)pitchCatcherSkill.ID]++;
                if (pitchCatcherSkill.effectIndex == SkillIndex.CatcherProvoke)
                {
                    batter.setDebuffValue(pitchCatcherSkill.effectValue);
                }

                //catcher.getSkillList().Add(pitchCatcherSkill.effectIndex);
            }
            if (tempPitcher.Count > 0)
            {
                //피치시 발생하는 투수 스킬
                pitchPitcherSkill = tempPitcher[UnityEngine.Random.Range(0, tempPitcher.Count)];
                skillUseCount[1 - curIndex][(SkillID)pitchPitcherSkill.ID]++;
                //pitcher.getSkillList().Add(pitchPitcherSkill.effectIndex);
            }
            if (tempBatter.Count > 0)
            {
                //피치시 발생하는 타자 스킬
                pitchBatterSkill = tempBatter[UnityEngine.Random.Range(0, tempBatter.Count)];
                skillUseCount[curIndex][(SkillID)pitchBatterSkill.ID]++;
                //batter.getSkillList().Add(pitchBatterSkill.effectIndex);
            }

            if (bAutoMode == true)
            {
                //오토모드(시뮬레이션모드)에서만 참조한다
                setUseSkillFlag(SkillUseStep.Pitching, pitchBatterSkill, pitchPitcherSkill, pitchCatcherSkill, VsResult.None);
            }

            tempCatcher = null;
            tempPitcher = null;
            tempBatter = null;

        }

        /// <summary>
        /// 스킬의 기본 조건을 결정짓는 조건 플래그 설정
        /// </summary>
        /// <param name="manager"></param>
        private void skillConditionState(BallPlayManager manager)
        {
            bNoRunner = false;
            bCrisisState = false;
            bScoringPosition = false;
            defensScoreGab = 0;
            offenseScoreGab = 0;

            if (manager == null)
            {
                //시뮬에서 불림
                ////Debug.Log("=================================>>> 스킬조건 세팅 시뮬에서 세팅 bInningOnceCheck : " + bInningOnceCheck);
                defensScoreGab = getScoreGab(1 - curIndex);
                offenseScoreGab = getScoreGab(curIndex);
            }
            else
            {
                ////Debug.Log("=================================>>> 스킬조건 세팅 액션 엔진에서 세팅 bInningOnceCheck : " + bInningOnceCheck);
                defensScoreGab = manager.getScoreGab(1 - curIndex);
                offenseScoreGab = manager.getScoreGab(curIndex);

                //베이스 동기화
                for (int i = 0; i < 4; i++)
                {
                    bOnBase[i] = manager.field.run.bOnBase[i];
                }
                //핀치스텝 동기화
                pinchStep = manager.pitcher.pinchState;
            }


            //스코어링 포지션
            if (bOnBase[SimulParm.SECONDBASE_INDEX] == true || bOnBase[SimulParm.THIRDBASE_INDEX] == true)
            {
                bScoringPosition = true;
            }

            //노 러너
            if (bOnBase[SimulParm.FIRSTBASE_INDEX] == false && bOnBase[SimulParm.SECONDBASE_INDEX] == false && bOnBase[SimulParm.THIRDBASE_INDEX] == false)
            {
                bNoRunner = true;
            }

            //위기상황
            if (bScoringPosition == true)
            {
                if (Mathf.Abs(offenseScoreGab) <= 2 || pinchStep == PinchStep.Pinch)
                {
                    bCrisisState = true;
                }
            }
        }


    }
}
