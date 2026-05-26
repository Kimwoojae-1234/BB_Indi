using UnityEngine;
using System.Collections;
using WebConnector;
using System.Collections.Generic;


namespace BaseBall.BallPlay
{
    public class BallPlayManager : MonoBehaviour
    {

#if _Skill_Display
        //연출테스트용
        public pSkillDisplay pitcherSkill_Display_test = pSkillDisplay.NoSkill;
        public bSkillDisplay batterSkill_Display_test = bSkillDisplay.NoSkill;
#endif

        //화면 사이즈
        public const int m_lcdW = 1280;
        public const int m_lcdWC = 640;
        public const int m_lcdH = 720;
        public const int m_lcdHC = 360;

        //각 상태별 카메라 기본 위치
        public const int BATTINGVIEW_CAMERA_INITX = 0;//-640;
        public const int BATTINGVIEW_CAMERA_INITY = 480;
        public const int FIELDVIEW_CAMERA_INITX = 2500;// 0;
        public const int FIELDVIEW_CAMERA_INITY = 0;//-4000;
        public const int ACTIVE_CENTER_CAMERA_INITX = 0;
        public const int ACTIVE_CENTER_CAMERA_INITY = -8000;

        //라인업 선수
        public const int PLAYING_PLAYER = 25;
        public const int NUM_FIELDER = 14;
        public const int NUM_PITCHER = 11;
        public const int NUM_LINEUP = 10;
        public const int PLAYING_TEAM = 2;
        //public const int _MAX_INNING = 12;//최대이닝
        //상태
        public const int _BATTINGVIEW = 0;
        public const int _FIELDVIEW = 1;
        public const int _BATTERCAMERA = 100;
        //배팅뷰 줌
        public float BattingViewInitZoom = 1;//0.9f;


        public QuickSimulator simulator;
        public Pitcher pitcher;
        public Batter batter;
        public Field field;        
        public Batting battingview;
        public PitchSystem pitch;
        public PitchSystem pitchPv;
        public Transform IngameUITrans = null;
       

        //배팅뷰 타구타입
        public HITBALLTYPE hitBallType;
        //플레이 상태
        public PlayState playState;
        
        //ui상태
        public bool bReadyFinish = false;
        public bool bReadyFinish2 = false;
        public bool bReadyFinish3 = false;
        public float readyZoom;

        //아웃 플래그 관련
        public const int _SO_FLAG = 0x01;   //삼진
        public const int _DP_FLAG = 0x02;   //병살    
        public const int _K_FLAG = 0x04;    //삼진(투수)    
        public const int _PO_FLAG = 0x08;   //자살
        public const int _A_FLAG = 0x10;    //보살
        public const int _CS_FLAG = 0x20;    //도루저지
        public const int _SBF_FLAG = 0x40;    //도루자

        int outFlag;
        public int poIndex, aoIndex, sbfIndex;  //자살 보살 도루자 인덱스
        public int offenseIndex, defenseIndex;  //팀의 공 수 인덱스 (0,1)

        
        //교체 관련
        public bool bChangeFlag;                    //교체 관련 활성화 플래그
        public bool bChangeFlagBatter, bChangeFlagRunner;
        public bool bNoChange;
        public bool bMyChange;                      //내 교체 여부
        public int nHeroModeChangeIndex;            //히어로 모드시 교체 될때 인덱스


        //룰 관련 플래그
        public bool bTopInning, bMyTurn;        //초말, player 공격여부
        public bool bMyHome;                    //홈 어웨이
        public bool bTopInningFinish;           //9(혹은 그이상)회초에 게임이 끝나는지 여부 
        public bool bPlayBall, bGameSet, bGoodByeHitCall;   //플레이볼, 게임셋, 굿바이 힛
        public bool bStrike, bStrikeOut, bThreeOutChange;   //스트라이크, 스트라이크 아웃, 쓰리아웃 체인지
        public bool bStealStrikeOut;
        public bool bStrikeCheck;
        public bool bBall, bBaseOnBalls;                    //볼, 베이스온볼
        public bool bSqueeze;                               //스퀴즈        

        //카운트 (볼, 스트라이크, 아웃, 이닝 등)
        public int nInningCount;
        public int nOutCount, nStrikeCount, nBallCount, outCountIF;
        public int newStrikeCount, newBallCount, newOutCount;
        public int fieldOutCountNum;
        public int nBatterCount;    //이닝에 등장한 타자 수


        //기록
        public int[] nGameScore = new int[2];    
        public int[] nHitCount = new int[2];
        public int[] nErrorCount = new int[2];
        public int[] nFourballCount = new int[2];   //사구 카운트
        public int[] nStrikeOutCount = new int[2];  //삼진 카운트
        public int[] nHomerunCount = new int[2];    //홈런 카운트
        public int[] nDPCount = new int[2];         //병살 카운트
        public int[] nStealCount = new int[2];      //도루 카운트
        //public int[] nPickOffOuntCount = new int[2];//견제사 카운트
        public string strBatterResult, strFieldOutType, strHitType, strHitType2;  //타자의 타석 결과를 문자로 나타냄(문자중계용)
        public int[] nCurPitcherPitchNum = new int[2] { 0, 0 }; ////볼던진수 
        public bool[] bTurnAroundFlag = new bool[2];    //역전 플래그
        public int[] nCurScore = new int[2];            //현재스코어
        public int[,] nInningScore = new int[2, SimulGameInfo.MAX_INNING];	//게임 스코어
        public int nLastScore;		//타점계산
        public int[] winPitcherIndex = new int[2],      //승리투수 인덱스
                     losePitcherIndex = new int[2];     //패전투수 인덱스

        
        //기타        
        public bool bSaveGame2;	        //저장여부 플래그 추가
        public int nRand1, nRand2, nRand3, nRand4, eyeRand;
        public int nTeamSkillPoint;     //팀스킬        

        //
        public bool bUpdate = false;

        //체인지 관련 플래그
        public bool bPitcherChangeFlag;
        public bool bBatterChangeFlag;
        public bool bFielderChangeFlag;
        public bool bRunnerChangeFlag;

        //기타
        public bool bInningChange;
        public bool bPitcherChangeException;
        private bool bTieBreakPvpBatterInit;// 승부치기는 특수한 곳에서 타자 초기화
        public bool bPlayBallEvent;  //true임ㄴ 플레이볼 이벤트 발생
        
        //로딩
        int loadCount;
        public bool bBatterForceLoad;   //타자 강제 로딩

        //시뮬레이션에서 넘어온 배팅결과 데이터
        public SimulBattingData battingResultData;
        //찬스모드
        public bool bCurrentChanceModeState; //현재 찬스 모드 여부인지

        //9회 투아웃 모드
        public int nineTwoRound;
        public int nineTwoScore;
        public int nineTwoFinalScore, nineTwoFinalRound;
        public int [] nineTwoRoundScore;// = new int[10];
        public bool bNineTwoNextRound;
        public CPlayer walkOffBatter, walkOffPitcher;


        //pvp
        //private PVP_Check pvpCheck = PVP_Check.None;
        private bool bPvpLoadEnd = false;
        public bool bPVPHitInfoCheck = false; //이놈을 받아야 수비쪽에서 게임을 진행시킴!! 아주 중요
        public NoHitStatus nohitType = NoHitStatus.None;
        public bool Pvp_bWildPitch;
        public bool Pvp_bStrikeCheck;
        public bool Pvp_bSwing;
        public bool Pvp_bBunt;        
        public bool Pvp_bContact;
        public bool Pvp_bBuntContact;
        //public bool Pvp_bCheckSwing;
        public BattingTiming Pvp_TimingPoint; 
        public BattingContact Pvp_ContactPoint;
        public SimulStealState Pvp_StealResult;

        public int Pvp_RandonSeed; //필드용 랜덤시드

        //타구
        public float Pvp_BallPower;
        public float Pvp_AngleZ;
        public float Pvp_Angle;
        public float Pvp_AngleHookSlice;
        public bool Pvp_HookorSlice;
        public bool Pvp_TopSpin;

        //번트        
        public SimulBuntType Pvp_BuntType;
        public SpecificBuntType Pvp_BuntResult;
        public int Pvp_BuntFielder;

        //특수능력
        public bool Pvp_spcatch, Pvp_spthrow, Pvp_diving, Pvp_hrsteal;

        //주루
        public bool[] Pvp_OneMore = new bool[4];
        public SimulOverrunState[] Pvp_moreSkillSense = new SimulOverrunState[4];

        //필딩
        public bool Pvp_FiendSync = false;
        public float[] Pvp_GroundTimeH = new float[9];
        public float[] Pvp_GroundTimeF = new float[9];
        public float[] Pvp_possibleDis = new float[9];
        public float[] Pvp_distanceToBall = new float[9];

        //송구
        public int[] Pvp_throwTarget = new int[9];

        //필드결과 동기화
        public bool Pvp_FiendResultSync = false;
        public bool[] Pvp_bOnBase = new bool[3];
        public int Pvp_myScore, Pvp_otherScore;
        public int Pvp_outCount;
        public bool Pvp_bThreeOut, Pvp_bGoodBye;


        private void Awake()
        {
            Debug.Log("BallPlay Awake");
            //디스커넥트 이벤트 설정
            //Debug.Log("BallPlayManager Awake");
            //PhotonManager.EventMyDisconnect += EventMyDisconnect; //내 디스커넥트
            //PhotonManager.EventOtherDisconnect += EventOtherDisconnect; //내 디스커넥트
            pvpmanager.OnPlay += OnPlay;

            bPvpLoadEnd = false;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            //Debug.Log("BallPlay Destroy");
            //PhotonManager.EventMyDisconnect -= EventMyDisconnect; //내 디스커넥트
            //PhotonManager.EventOtherDisconnect -= EventOtherDisconnect; //내 디스커넥트
            pvpmanager.OnPlay -= OnPlay;
        }


        //내 디스커넥트 이벤트
        private void EventMyDisconnect(string message)
        {
            Debug.Log("EventMyDisconnect");
            Mode.bPauseGame = false;
            AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Disconnect");
        }

        //상대 디스커넥트 이벤트
        private void EventOtherDisconnect(string message)
        {
            Debug.Log("EventOtherDisconnect");
            if (Mode.bPvpMode433 == true)
            {
                //if (PhotonNetwork.connected == true)
                //{
                //    PhotonManager.Get().Disconnect();
                //}
            }
            Mode.bPauseGame = false;
            AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Disconnect");
        }

        //디스커넥트 프로세스
        private IEnumerator disconnectProcess()
        {
            MusicManager.Get().StopMusic();

            GameObject obj1 = GameObject.Find("skillEffectDisplayManager").gameObject;
            if (obj1 != null) Destroy(obj1);
            yield return new WaitForEndOfFrame();
            GameObject obj2 = GameObject.Find("simulator").gameObject;
            if (obj2 != null) Destroy(obj2);
            yield return new WaitForEndOfFrame();
            GameObject obj3 = GameObject.Find("Managers").gameObject;
            if (obj3 != null) Destroy(obj3);
            yield return new WaitForEndOfFrame();
            GameObject obj4 = GameObject.Find("pvpmanager").gameObject;
            if (obj4 != null) Destroy(obj4);
            yield return new WaitForEndOfFrame();
            GameObject obj5 = GameObject.Find("PhotonManager").gameObject;
            if (obj5 != null) Destroy(obj5);
            GameObject obj6 = GameObject.Find("SoundManager").gameObject;
            if (obj6 != null) Destroy(obj6);
            GameObject obj7 = GameObject.Find("MusicManager").gameObject;
            if (obj7 != null) Destroy(obj7);
            yield return new WaitForEndOfFrame();
            Mode.bPauseGame = false;
        }


        public void pvpFlagInit()
        {
            nohitType = NoHitStatus.None;
            bPVPHitInfoCheck = false;
            Pvp_bSwing = false;
            Pvp_bBunt = false;
            Pvp_bContact = false;
            Pvp_bBuntContact = false;
            Pvp_StealResult = SimulStealState.NONE;
            //Pvp_bCheckSwing = false;

            for (int i = 0; i < 4; i++) Pvp_OneMore[i] = false;
            for (int i = 0; i < 9; i++) Pvp_throwTarget[i] = -100;

            Pvp_FiendSync = false;
        }


        public void setPvpBattingInfo()
        {
            field.ballPower = Pvp_BallPower;
            field.ball.angleZ = Pvp_AngleZ;
            field.ball.angle = Pvp_Angle;
            //Debug.Log("적용정보  ballPower = " + field.ballPower + "   angleZ = " + field.ball.angleZ + "    angle = " + field.ball.angle);
            field.ball.angleHookSlice = Pvp_AngleHookSlice;
            field.ball.bHookorSlice = Pvp_HookorSlice;
            field.ball.bTopSpin = Pvp_TopSpin;

            batter.bBunt = Pvp_bBunt;
            if (batter.bBunt == true)
            {
                batter.buntType = Pvp_BuntType;
                batter.buntResult = Pvp_BuntResult;
                batter.buntFielder = Pvp_BuntFielder;
                
            }
        }

        public void RandomSeedSync()
        {
            if (Mode.bPvpMode433 == true)
            {
                if (bMyTurn == true)
                {
                    Pvp_RandonSeed = (int)(Time.time * 100f);
                    pvpmanager.Get().SendRandomSeedInfo(Pvp_RandonSeed);
                }
                else
                {

                }
                //Debug.Log("랜덤싱크 시드값 : " + Pvp_RandonSeed);
                Random.InitState(Pvp_RandonSeed);
            }
        }
                

        /// <summary>
        /// PVP 야구 플레이 이벤트
        /// </summary>
        /// <param name="eventcode"></param>
        /// <param name="content"></param>
        private void OnPlay(byte eventcode, object content)
        {
            switch (eventcode)
            {
                case pvpmanager.GAME_READY:
                    {
                        //양쪽 모두 수신
                        Debug.Log("게임 레디 상태 수신");
                        pvpmanager.pvpCheck = PVP_Check.GameReady;
                    }
                    break;
                case pvpmanager.BATTER_SYNC:
                    {
                        //수비만 수신
                        if (bMyTurn == false)
                        {
                            //Debug.Log("수비자 pvp플래그 초기화");
                            pvpFlagInit();                            
                            //이 정보를 받으면 수비 플레이어 타자 정보 동기화 함                            
                            string recieveCode = (string)(content);
                            PvpBatterSync batterSync = Utils.JsonUtils.Deserialize<PvpBatterSync>(recieveCode);
                            //타자 동기화 정보 수신
                            GetBatterSync(batterSync);
                        }
                    }
                    break;
                case pvpmanager.PITCH_INFO1:
                    {
                        //공격만 수신
                        if (bMyTurn == true)
                        {
                            //Debug.Log("공격자 피치 정보 수신");
                            IngameUI.GetControlRunner().SetActive(false, true); //도루 UI 비활성화
                            //Debug.Log("공격자 pvp플래그 초기화");
                            pvpFlagInit();
                            string recieveCode = (string)(content);
                            PvpPitchInfo pitchInfo = Utils.JsonUtils.Deserialize<PvpPitchInfo>(recieveCode);
                            //정보수신 후 피치
                            GetPitchInfo(pitchInfo);                            
                        }
                    }
                    break;
                case pvpmanager.PITCH_INFO2:
                    {
                        //수비만 수신
                        if (bMyTurn == false)
                        {
                            //이 정보를 받으면 수비 플레이어 투수 공던짐
                            string recieveCode = (string)(content);
                            PvpPitchInfo2 pitchInfo2 = Utils.JsonUtils.Deserialize<PvpPitchInfo2>(recieveCode);
                            //피치 정보 재수신 후 공던짐
                            GetPitchInfo2(pitchInfo2);
                        }
                    }
                    break;
                case pvpmanager.PITCH_SELECT:
                    {
                        //공격만 수신
                        if (bMyTurn == true)
                        {
                            GetPitchSelect();
                        }
                    }
                    break;
                case pvpmanager.PITCH_TIMER:
                    {
                        //공격만 수신
                        if (bMyTurn == true)
                        {
                            GetPitchTimer();
                        }
                    }
                    break;
                case pvpmanager.NO_HIT_INFO:
                    {
                        //수비만 수신
                        if (bMyTurn == false)
                        {
                            //이정보를 받으면 수비 플레이어의 타자 공 안침
                            string recieveCode = (string)(content);
                            PvpNoHitInfo nohitInfo = Utils.JsonUtils.Deserialize<PvpNoHitInfo>(recieveCode);
                            bPVPHitInfoCheck = true;    //hit관련 인포 수신
                            //공을 안친 경우 케이스
                            GetNohitInfo(nohitInfo);
                        }
                    }
                    break;
                case pvpmanager.BATTING_INFO:
                    {
                        //수비만 수신
                        if (bMyTurn == false)
                        {
                            //이 정보를 받으면 수비 플레이어 타자 공침
                            //Debug.Log("=============>>배팅인포 수신");
                            string recieveCode = (string)(content);
                            PvpBattingInfo battingInfo = Utils.JsonUtils.Deserialize<PvpBattingInfo>(recieveCode);
                            GetBattingInfo(battingInfo);
                        }
                    }
                    break;
                case pvpmanager.FIELDING_INFO:
                    {
                        //수비만 수신
                        if (bMyTurn == false)
                        {
                            //이 정보를 참조하여 수비 플레이어 필딩 상태를 동기화 함
                        }
                    }
                    break;
                case pvpmanager.FIELD_RESULT_INFO:
                    //수비만 수신
                    {
                        if (bMyTurn == false)
                        {
                            //이 정보를 참조하여 수비 플레이어 필딩 결과를 동기화함
                        }
                    }
                    break;
                case pvpmanager.STEAL_INFO:
                    //수비만 수신
                    {
                        if (bMyTurn == false)
                        {
                            //이 정보를 참조하여 도루 결과를 동기화함
                            string recieveCode = (string)(content);
                            PvpStealInfo stealInfo = Utils.JsonUtils.Deserialize<PvpStealInfo>(recieveCode);
                            //도루 관련 케이스
                            GetStealInfo(stealInfo);
                        }
                    }
                    break;
                case pvpmanager.STEAL_INFO_RETURN:
                    //공격만 수신
                    {
                        if (bMyTurn == true)
                        {
                            //돌려받은 후 도루 시작
                            string recieveCode = (string)(content);
                            PvpStealInfo stealInfo = Utils.JsonUtils.Deserialize<PvpStealInfo>(recieveCode);
                            //도루 관련 케이스
                            GetStealReturnInfo(stealInfo);
                        }
                    }
                    break;
                case pvpmanager.RANDOMSEED_INFO:
                    //수비만 수신
                    {
                        if (bMyTurn == false)
                        {
                            string seed = (string)(content);
                            Pvp_RandonSeed = int.Parse(seed);
                        }
                    }
                    break;
                case pvpmanager.ONEMORE_INFO:
                    //수비만 수신
                    {
                        if (bMyTurn == false)
                        {
                            string recieveCode = (string)(content);
                            PvpOnemoreInfo oneMoreInfo = Utils.JsonUtils.Deserialize<PvpOnemoreInfo>(recieveCode);
                            //한베이스 더 관련 케이스
                            GetOnemoreInfo(oneMoreInfo);
                        }
                    }
                    break;
                case pvpmanager.FIELDING_SYNC:
                    //수비만 수신
                    {
                        if (bMyTurn == false)
                        {
                            string recieveCode = (string)(content);
                            PvpFieldingSyncInfo fieldSyncInfo = Utils.JsonUtils.Deserialize<PvpFieldingSyncInfo>(recieveCode);
                            //필딩 동기화
                            GetFieldSyncInfo(fieldSyncInfo);
                        }
                    }
                    break;
                case pvpmanager.THROWING_SYNC:
                    //수비만수신
                    {
                        if (bMyTurn == false)
                        {
                            string recieveCode = (string)(content);
                            PvpThrowingSyncInfo throwInfo = Utils.JsonUtils.Deserialize<PvpThrowingSyncInfo>(recieveCode);
                            //송구 동기화
                            GetThrowSyncInfo(throwInfo);
                        }
                    }
                    break;
                case pvpmanager.FIELD_RESULT_SYNC:
                    //수비만 수신
                    {
                        if(bMyTurn == false)
                        {
                            string recieveCode = (string)(content);
                            PvpFieldResultSync resultSync = Utils.JsonUtils.Deserialize<PvpFieldResultSync>(recieveCode);
                            GetFieldResultSync(resultSync);
                        }
                    }
                    break;

            }
        }

        /// <summary>
        /// 타자 동기화 정보 수신
        /// </summary>
        /// <param name="batterSync"></param>
        private void GetBatterSync(PvpBatterSync batterSync)
        {
            //Debug.Log("타자 동기화 수신");
            int randomSeed = batterSync.randSeed;
            Pvp_RandonSeed = batterSync.randSeed2;   
            
            //카운트         
            nBallCount = batterSync.ballCount;
            nStrikeCount = batterSync.strikeCount;
            nOutCount = batterSync.outCount;
            setNewCount();

            //베이스
            //for (int i = 0; i < 3; i++) field.run.bOnBase[i] = batterSync.bBaseOn[i];

            //특수능력 동기화
            Pvp_spcatch = batterSync.spcatch;
            Pvp_spthrow = batterSync.spthrow;
            Pvp_diving = batterSync.spdiving;
            Pvp_hrsteal = batterSync.sphrsteal;

            //Debug.Log("수신된 특수능력 발동상황 Pvp_spcatch =" + Pvp_spcatch + "   Pvp_spthrow =" + Pvp_spthrow + "   Pvp_diving =" + Pvp_diving + "   Pvp_hrsteal =" + Pvp_hrsteal);

            Random.InitState(randomSeed);
            //이 정보를 받아야 수비 플레이어 플레이가 가능함
            //Debug.Log("======================>>> Pitcher pstate " + pitcher.pState);
            //StartCoroutine(checkPitchUI());//settingPitchingUIbyForce();

            pvpmanager.pvpCheck = PVP_Check.PitchReady;
        }



        /// <summary>
        /// 피치 정보 수신
        /// </summary>
        /// <param name="pitchInfo"></param>
        private void GetPitchInfo(PvpPitchInfo pitchInfo)
        {
            //볼스피드
            pitcher.curBallSpeed = pitchInfo.curBallSpeed;
            //선택구종
            pitcher.selectedBallIndex = (PitchingArsenal)pitchInfo.selectBallType; // -> setBallAndGuwee()함수에 넣어
            //미스 여부
            pitcher.bMissControl = pitchInfo.bMissControl;
            //유저 컨트롤 밸류
            pitcher.userControlValue = (UserControlValue)pitchInfo.userControlValue; // 

            pitcher.coursePvpX = pitchInfo.courseX;
            pitcher.coursePvpY = pitchInfo.courseY;
            ////Debug.Log("전달받은값===============>> courseX = " + manager.pitcher.coursePvpX);
            ////Debug.Log("전달받은값===============>> courseY = " + manager.pitcher.coursePvpY);
            pitcher.hitByPitchStep = pitchInfo.hitByPitchStep;

            //던지기
            PitchingArsenal index = pitcher.selectedBallIndex;
            pitcher.setBallAndGuwee(index);
            pitcher.coursePvpX -= (pitcher.preHenkaX);
            pitcher.coursePvpY -= (pitcher.preHenkaY);
            pitcher.courseX = pitcher.courseX2 = pitcher.coursePvpX;
            pitcher.courseY = pitcher.courseY2 = pitcher.coursePvpY;
            //Debug.Log("받는 course??===========>>cursorX = " + pitcher.courseX + "=======>>cursorY = " + pitcher.courseY);
            pitch.pitchOrigin.setPvpZoneInit(pitcher.courseX, pitcher.courseY, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
            pitcher.startPitchingAnim();

            //에러정보 체크
            //for (int i = 0; i < 9; i++) Debug.Log("타자 :: bCatchErrorFlag : " + field.fielder[i].bCatchErrorFlag + "     bThrowErrorFlag : " + field.fielder[i].bThrowErrorFlag);

            //타자 타이머 닫기
            IngameUI.GetScoreBoard().SetBatterTimer(false);

            Invoke("returnPitchInfo", 0.2f);
        }

        /// <summary>
        /// 피치 정보를 리턴해준다
        /// </summary>
        private void returnPitchInfo()
        {
            pvpmanager.Get().SendPitchInfo2(this);

        }



        /// <summary>
        /// 피치 정보 수신2
        /// </summary>
        /// <param name="pitchInfo2"></param>
        private void GetPitchInfo2(PvpPitchInfo2 pitchInfo2)
        {
            //카운트 동기화
            nBallCount = pitchInfo2.ballCount;
            nStrikeCount = pitchInfo2.strikeCount;
            nOutCount = pitchInfo2.outCount;
            setNewCount();

            //에러정보
            for (int i = 0; i < 9; i++)
            {
                field.fielder[i].bCatchErrorFlag = pitchInfo2.bCatchError[i];
                field.fielder[i].bThrowErrorFlag = pitchInfo2.bThrowError[i];
                //Debug.Log("투수:: bCatchErrorFlag : "+ field.fielder[i].bCatchErrorFlag + "     bThrowErrorFlag : "+ field.fielder[i].bThrowErrorFlag);
            }

            //도루 정보 한번 더
            field.run.stealResult = Pvp_StealResult;

            pitcher.startPitchingAnim();
        }

        /// <summary>
        /// 피치 셀렉트 인포 전달
        /// </summary>
        private void GetPitchSelect()
        {
            //Debug.Log(" 피치 셀렉트 인포 수신");
            //도루 창을 닫는다
            IngameUI.GetControlRunner().SetActive(false, true);
            //채팅창을 닫는다.
            IngameUI.GetEmoticonChatting().chattingDisable();
            //배터 타이머 상태 변환
            IngameUI.GetScoreBoard().selectPitch();
        }

        /// <summary>
        /// 피치 타이머 수신과 동시에 배터타이머
        /// </summary>
        private void GetPitchTimer()
        {
            //Debug.Log(" 피치 타이머 인포 수신");
            //타자 타이머 열기
            IngameUI.GetScoreBoard().SetBatterTimer(true);
        }

        /// <summary>
        /// 노 힛 정보 수신
        /// </summary>
        /// <param name="nohitInfo"></param>
        private void GetNohitInfo(PvpNoHitInfo nohitInfo)
        {
            //Debug.Log("Get No Hit Info");
            nohitType = nohitInfo.noHitType;
            Pvp_bWildPitch = nohitInfo.bWildPitch;
            Pvp_bStrikeCheck = nohitInfo.bStrikeCheck;
            if (nohitType == NoHitStatus.NoSwing)
            {
                //Debug.Log("스윙 안함");
                Pvp_bSwing = false;
                Pvp_bBunt = false;
            }
            else if (nohitType == NoHitStatus.HutSwing)
            {
                //Debug.Log("헛스윙");
                Pvp_TimingPoint = nohitInfo.TimingPoint; //타이밍 세팅
                //Debug.Log("수신하는 헛스윙 타이밍==============>>" + Pvp_TimingPoint);
                Pvp_bSwing = true;
                Pvp_bContact = false;
                Pvp_bBunt = false;
                //Pvp_bCheckSwing = false;
            }
            else if (nohitType == NoHitStatus.CheckSwing)
            {
                Debug.Log("체크스윙");
                Pvp_TimingPoint = nohitInfo.TimingPoint; //타이밍 세팅
                Debug.Log("수신하는 헛스윙 타이밍==============>>" + Pvp_TimingPoint);
                Pvp_bSwing = true;
                Pvp_bContact = false;
                Pvp_bBunt = false;
                batter.bCheckSwingActivate = true;
                //Pvp_bCheckSwing = true;
            }
            else if (nohitType == NoHitStatus.BuntSwing)
            {
                Debug.Log("번트 헛스윙 수신");                
                //Debug.Log("수신하는 헛스윙 타이밍==============>>" + Pvp_TimingPoint);
                Pvp_bSwing = false;
                Pvp_bBunt = true;
                Pvp_bContact = false;
                Pvp_bBuntContact = false;
                
            }
        }


        /// <summary>
        /// 도루정보 수신
        /// </summary>
        /// <param name="stealInfo"></param>
        private void GetStealInfo(PvpStealInfo stealInfo)
        {
            //Debug.Log("=======================>> 도루정보 수신 stealResult :" + stealInfo.stealResult);
            int target = stealInfo.stealTarget;
            field.run.myControlSteal(target);


            Pvp_StealResult = stealInfo.stealResult;
            field.run.stealResult = Pvp_StealResult;
        }


        /// <summary>
        /// 도루정보 돌려받기 수신
        /// </summary>
        /// <param name="stealInfo"></param>
        private void GetStealReturnInfo(PvpStealInfo stealInfo)
        {
            Debug.Log("=======================>> 도루리턴 정보 수신 target :" + stealInfo.stealTarget);
            int target = stealInfo.stealTarget;
            field.run.set_Steal_Pickoff(target);
        }

        /// <summary>
        /// 타격 정보 받아오기
        /// </summary>
        /// <param name="battingInfo"></param>
        private void GetBattingInfo(PvpBattingInfo battingInfo)
        {
            Pvp_bBunt = false;
            Pvp_bSwing = false;

            //타구속성
            Pvp_BallPower = battingInfo.ballPower;
            Pvp_AngleZ = battingInfo.angleZ;
            Pvp_Angle = battingInfo.angle;
            //Debug.Log("배팅인포 수신정보  ballPower = " + Pvp_BallPower + "   angleZ = " + Pvp_AngleZ + "    angle = " + Pvp_Angle);
            Pvp_AngleHookSlice = battingInfo.angleHookSlice;
            Pvp_HookorSlice = battingInfo.bHookorSlice;
            Pvp_TopSpin = battingInfo.bTopSpin;

            //번트 속성
            Pvp_bBunt = battingInfo.bBunt;
            Pvp_BuntType = battingInfo.buntType;
            Pvp_BuntResult = battingInfo.buntResult;
            Pvp_BuntFielder = battingInfo.buntFielder;

            //플라이볼
            for (int i = 0; i < 9; i++)
            {
                Pvp_possibleDis[i] = battingInfo.possibleDis[i];
                Pvp_distanceToBall[i] = battingInfo.distanceToBall[i];
            }


            //번트 or 타격
            if (Pvp_bBunt == true)
            {
                //번트 세팅
                Pvp_bBuntContact = true;
            }
            else
            {
                //타격 세팅
                Pvp_bSwing = true;
                Pvp_bContact = true;
            }
            batter.aiTimingPoint = BattingTiming.PERFECT;
        }

        /// <summary>
        /// 한베이스 더 관련 인포
        /// </summary>
        /// <param name="oneMoreInfo"></param>
        private void GetOnemoreInfo(PvpOnemoreInfo oneMoreInfo)
        {
            int dst = oneMoreInfo.dst;
            Pvp_OneMore[dst] = oneMoreInfo.oneMore;
            Pvp_moreSkillSense[dst] = oneMoreInfo.moreSkill;
        }


        /// <summary>
        /// 필딩 싱크
        /// </summary>
        /// <param name="fieldSyncInfo"></param>
        private void GetFieldSyncInfo(PvpFieldingSyncInfo fieldSyncInfo)
        {
            //Debug.Log("필딩 동기화 정보 수신");
            Pvp_FiendSync = true;
            for (int i = 0; i < 9; i++)
            {
                Pvp_GroundTimeH[i] = fieldSyncInfo.groundTimeH[i];
                Pvp_GroundTimeF[i] = fieldSyncInfo.groundTimeF[i];
                //Pvp_possibleDis[i] = fieldSyncInfo.possibleDis[i];
                //Pvp_distanceToBall[i] = fieldSyncInfo.distanceToBall[i];
            }
        }

        /// <summary>
        /// 송구 싱크
        /// </summary>
        /// <param name="throwInfo"></param>
        private void GetThrowSyncInfo(PvpThrowingSyncInfo throwInfo)
        {
            //Debug.Log("송구 동기화 정보 수신");
            int index = throwInfo.index;
            Pvp_throwTarget[index] = throwInfo.target; 
        }


        /// <summary>
        /// 필드 리절트 싱크
        /// </summary>
        /// <param name="resultSync"></param>
        private void GetFieldResultSync(PvpFieldResultSync resultSync)
        {
            //Debug.Log("필드 결과 동기화 정보 수신");
            Pvp_FiendResultSync = true;
            for (int i = 0; i < 3; i++) Pvp_bOnBase[i] = resultSync.bOnBase[i];
            Pvp_myScore = resultSync.myScore;
            Pvp_otherScore = resultSync.otherScore;
            Pvp_outCount = resultSync.outCount;
            Pvp_bThreeOut = resultSync.bThreeOut;
            Pvp_bGoodBye = resultSync.bGoodBye;
        }


        private void pvpCheckUpdate()
        {
            if(pvpmanager.pvpCheck == PVP_Check.GameReady)
            {
                checkGameReady();
            }
            else if (pvpmanager.pvpCheck == PVP_Check.PitchReady)
            {
                //Debug.Log("pitcher.pState = " + pitcher.pState + "       playState = " + playState);
                checkPitchUI();
            }

        }

        /// <summary>
        /// 게임레디를 서로 수신
        /// </summary>
        private void checkGameReady()
        {   
            if (bPvpLoadEnd == true) //살려살려
            {
                Debug.Log("checkGameReady  bPvpLoadEnd : " + bPvpLoadEnd);
                pvpmanager.pvpCheck = PVP_Check.None;
                startGame();
                StartCoroutine(startgameCheck());
            }
        }

        IEnumerator startgameCheck()
        {
            //모든 리소스 로딩되었는지 체크
            while (bFielderLoadComp == false)
            {
                yield return new WaitForSeconds(0.2f);
            }
            bUpdate = true;
        }


        //피칭 UI 활성화
        private void checkPitchUI()
        { 
            int count = 0;
            if (pitcher.pState == PitcherState._GET_SIGN && 
                (playState == PlayState.PLAY_BATTING_VIEW_READY || playState == PlayState.PLAY_BATTING_VIEW || playState == PlayState.PLAY_BATTING_VIEW_PRE))
            {
                Debug_UI.SetNetwork(false);
                settingPitchingUIbyForce();
                pvpmanager.pvpCheck = PVP_Check.None;
            }
            else
            { 
                //무한 루프 돌어                
                if (count == 1)
                {
                    Debug_UI.SetNetwork(true);
                }
                count++;
            }
        }

        //////////////////////////////////////////////////////////////////////////////
        // 업데이트
        //////////////////////////////////////////////////////////////////////////////
        void Update()// FixedUpdate() 
        {
            if (bUpdate == true)
            {
                updateFrame();
            }


            if(pvpmanager.pvpCheck != PVP_Check.None)
            {
                pvpCheckUpdate();
            }
            
        }

        //////////////////////////////////////////////////////////////////////////////
        // 게임 초기화 (1번 호출)
        //////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 게임초기화 -> 플레이를 초기화를 불려지는 함수
        /// </summary>
        private GameObject loadObj;
        public void initGame(GameObject loading)
        {
            //Debug.Log("initGame");
            loadObj = loading.gameObject;
            bInningChange = false;
            bUpdate = false;
            bPlayBallEvent = false;
            loadCount = 0;
            StartCoroutine(loadObject());
        }

        public void destroyLoadingObj()
        {
            if (loadObj != null)
            {
                Destroy(loadObj);
                loadObj = null;
            }
        }

        /// <summary>
        /// 신로딩이 완료 된 후 기타 리소스를 로딩하기 위한
        /// </summary>
        /// <returns></returns>
        private IEnumerator loadObject()
        {
            //Debug.Log("loadObject");
            //게임세팅 초기화
            initGameSetting();
            UILoading.loadingCount = 0.55f;
            yield return null;
            
            //타자 인스턴스 초기화
            batter.initInstance(this);
            UILoading.loadingCount = 0.6f;
            yield return null;

            //투수 인스턴스 초기화
            pitcher.initInstance(this);
            UILoading.loadingCount = 0.65f;
            yield return null;
            

            //배팅뷰의 센터 로딩및 초기화
            battingview.loadCenter();
            UILoading.loadingCount = 0.7f;
            yield return null;

            //배팅뷰의 레프트 로딩및 초기화
            battingview.loadLeft();
            yield return null;

            //배팅뷰의 라이트 로딩및 초기화
            battingview.loadRight();
            yield return null;

            //배팅뷰의 투수뷰 로딩및 초기화
            battingview.loadPitcher();
            yield return null;

            //배팅뷰 인스턴스 초기화
            battingview.initInstance(this);
            UILoading.loadingCount = 0.75f;
            yield return null;

            //존 인스턴스 초기화
            battingview.zoneUI.initInstance(this);
            UILoading.loadingCount = 0.8f;
            yield return null;

            //필드 인스턴스 초기화
            field.initInstance(this);
            UILoading.loadingCount = 0.85f;
            yield return null;

            //필드 볼 인스턴스 초기화
            field.ball.initInstance(this);
            yield return null;

            
            //필드 주자 인스턴스 초기화
            field.run.initInstance(field);
            //Debug.Log("6");
            yield return null;

            //컨트롤 매니저 인스턴스 초기화
            ControlManager.InitInstance(this);//gameUI.initInstance(this); //[UI]필요없는 오브젝트가 됨
            UILoading.loadingCount = 0.9f;//Debug.Log("7");
            yield return null;

            //배팅뷰 볼 인스턴스 초기화
            pitcher.bball.initInstance(this);
            UILoading.loadingCount = 1.0f;//Debug.Log("5");
            yield return null;

            //스킬 연출 매니처 초기화
            SkillEffectDisplayManager.InitInstance(this);
            fieldSkillDisplayManager.InitInstance(this);

            //배팅뷰 오브젝트 초기화
            BattingViewInitZoom = 0.9f;
            battingview.transform.localScale = new Vector3(0.9f, 0.9f, 1); //new Vector3(BattingViewInitZoom, BattingViewInitZoom, 1);
            pitch.transform.parent = battingview.transform;
            pitchPv.transform.parent = battingview.transform;
            yield return null;


            yield return new WaitForSeconds(1.0f);

            //오프닝 연출
            IngameUI.OpeningInit(this);
            yield return null;

            //로딩 완료
            UILoading.loadingCount = 1.0f;//Debug.Log("10");            
            pitcher.setBVFielder();

            bPvpLoadEnd = true;

        }

        /// <summary>
        /// 누적 기록 설정
        /// </summary>
        /// <param name="homeRecInfo"></param>
        /// <param name="awayRecInfo"></param>
        private void setRecordSetting(RecordInfo homeRecInfo, RecordInfo awayRecInfo, bool bHome)
        {
#if _Test_Local
            //설정 안함
#else
            RecordInfo[] rInfo = new RecordInfo[2];
            rInfo[0] = bHome ? homeRecInfo : awayRecInfo;
            rInfo[1] = bHome ? awayRecInfo : homeRecInfo;

            for (int i = 0; i < 2; i++)
            {
                //타자 세팅
                for (int j = 0; j < NUM_FIELDER; j++)
                {
                    CPlayer curPlayer = SimulPlayerManager.GetFielder(i,j);
                    GameRecordHitter record = rInfo[i].GetGameRecordHitter(curPlayer.getCard().cardSeq, curPlayer.getCard().cardId);
                    if (record != null)
                    {
                        //키값이 있는 놈만 세팅
                        curPlayer.setBatterRecord(record);
                    }
                    else
                    {
                        curPlayer.setBatterRecord(null);
                    }
                }
                //투수 세팅
                for (int j = 0; j < NUM_PITCHER; j++)
                {
                    CPlayer curPlayer = SimulPlayerManager.GetPitcher(i, j);
                    if (curPlayer.getCard() != null)
                    {
                        GameRecordPitcher record = rInfo[i].GetGameRecordPitcher(curPlayer.getCard().cardSeq, curPlayer.getCard().cardId);

                        if (record != null)
                        {
                            //키값이 있는 놈만 세팅
                            curPlayer.setPitcherRecord(record);
                        }
                        else
                        {
                            curPlayer.setPitcherRecord(null);
                        }
                    }
                }

            }

            

#endif

        }

        /// <summary>
        /// 서버로부터 받아온 데이터를 로컬에 세팅하는 작업 수행
        /// </summary>
        private void initGameSetting()
        {
#if _Skill_Display
            //연출테스트용
            pitcherSkill_Display_test = InGameDebug.PitcherSkill;
            batterSkill_Display_test = InGameDebug.BitcherSkill;
#endif

            bool bHome = false;
            GameLineup lineup = null;

            if (Mode.gameMode == Mode.GamePlayMode.Season)
            {
                //시즌 모드
#if _Test_Local
                SimulPlayerManager.awayTeamIndex = Random.Range(1, 11);
                SimulPlayerManager.homeTeamIndex = Random.Range(1, 11);
                bHome = false;// InGameDebug.MYHOME;
                SimulPlayerManager.strAwayTeam = SimulPlayerManager.GetTeamName(false);
                SimulPlayerManager.strHomeTeam = SimulPlayerManager.GetTeamName(true);
#else
                //시즌모드 세팅
                // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;
                int myScheSeq = info.myScheNo;
                int[] teamNo = info.schedule[myScheSeq];
                bHome = info.home;

                //팀정보
                SeasonTeamInfo homeTeam = info.teamInfos[teamNo[0]];
                SeasonTeamInfo awayTeam = info.teamInfos[teamNo[1]];
                SimulPlayerManager.homeTeamIndex = (int)(homeTeam.team);
                SimulPlayerManager.awayTeamIndex = (int)(awayTeam.team);
                SimulPlayerManager.strHomeTeam = homeTeam.name;
                SimulPlayerManager.strAwayTeam = awayTeam.name;
                SimulPlayerManager.myTeamSeqNum = teamNo[bHome ? 0 : 1];
                SimulPlayerManager.cpuTeamSeqNum = teamNo[bHome ? 1 : 0];

                //라인업 세팅
                lineup = SimulManager.GetSeasonGameLineup(homeTeam, awayTeam);
                
#endif
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                //bHome = PhotonNetwork.isMasterClient ? true : false;
                SimulPlayerManager.awayTeamIndex = (int)(pvpmanager.Get().teamCode[bHome ? 1 : 0]);
                SimulPlayerManager.homeTeamIndex = (int)(pvpmanager.Get().teamCode[bHome ? 0 : 1]);
                SimulPlayerManager.strAwayTeam = pvpmanager.Get().UserID[bHome ? 1 : 0];
                SimulPlayerManager.strHomeTeam = pvpmanager.Get().UserID[bHome ? 0 : 1];
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                //PVP매치 모드
#if _Test_Local

                //매니저 로딩
                Util.Load("MainGame/prefabs/ballplayPrefab/PvpManager", transform, Vector3.zero);
                PvpManager.GetInstance().Init(this);

                bHome = InGameDebug.MYHOME;
                SimulPlayerManager.awayTeamIndex = 10;
                SimulPlayerManager.homeTeamIndex = 8;
                SimulPlayerManager.strAwayTeam = SimulPlayerManager.GetTeamName(false);
                SimulPlayerManager.strHomeTeam = SimulPlayerManager.GetTeamName(true);
#else
                //시즌모드 세팅
                //매니저 로딩
                // DISABLED_MGRS: LivePlayGameInfo info = Mgrs.userData.livePlayGmaeInfo;

                Util.Load("MainGame/prefabs/ballplayPrefab/PvpManager", transform, Vector3.zero);
                PvpManager.GetInstance().Init(this);                
                // DISABLED_MGRS: bHome = Mgrs.userData.LivePVP_Service.IsHome;

                //팀정보
                LivePlayTeamInfo homeTeam = info.homeTeam;
                LivePlayTeamInfo awayTeam = info.awayTeam;
                SimulPlayerManager.homeTeamIndex = (int)(homeTeam.team);
                SimulPlayerManager.awayTeamIndex = (int)(awayTeam.team);
                SimulPlayerManager.strHomeTeam = homeTeam.name;
                SimulPlayerManager.strAwayTeam = awayTeam.name;
                //SimulPlayerManager.myTeamSeqNum = teamNo[bHome ? 0 : 1];
                //SimulPlayerManager.cpuTeamSeqNum = teamNo[bHome ? 1 : 0];

                //라인업 세팅
                lineup = new GameLineup();
                lineup.homeTeam = homeTeam.lineup;
                lineup.awayTeam = awayTeam.lineup;
#endif
            }
            /*else if (Mode.gameMode == Mode.GamePlayMode.Ranking)
            {
                //랭킹 모드
#if _Test_Local
                SimulPlayerManager.awayTeamIndex = Random.Range(1, 11);
                SimulPlayerManager.homeTeamIndex = Random.Range(1, 11);
                SimulPlayerManager.strAwayTeam = SimulPlayerManager.GetTeamName(false);
                SimulPlayerManager.strHomeTeam = SimulPlayerManager.GetTeamName(true);
                //랭킹모드시 무조건 후공
                bHome = true;
#else
                //랭킹모드 세팅
                // DISABLED_MGRS: RankedPlayGameInfo info = Mgrs.userData.Ingame_rankInfo;
                bHome = true;
                RankedPlayTeamInfo homeTeam = info.homeTeam;
                RankedPlayTeamInfo awayTeam = info.awayTeam;

                SimulPlayerManager.homeTeamIndex = (int)(homeTeam.team);
                SimulPlayerManager.awayTeamIndex = (int)(awayTeam.team);
                SimulPlayerManager.strHomeTeam = homeTeam.name;
                SimulPlayerManager.strAwayTeam = awayTeam.name;

                lineup = SimulManager.GetRankGameLineup(homeTeam, awayTeam);
#endif
            }*/
            else if (Mode.gameMode == Mode.GamePlayMode.Race)
            {
                //쟁탈모드 
#if _Test_Local
                SimulPlayerManager.awayTeamIndex = Random.Range(1, 11);
                SimulPlayerManager.homeTeamIndex = Random.Range(1, 11);
                SimulPlayerManager.strAwayTeam = SimulPlayerManager.GetTeamName(false);
                SimulPlayerManager.strHomeTeam = SimulPlayerManager.GetTeamName(true);
                //랭킹모드시 무조건 후공
                bHome = MyMath.Half();
#else
                //쟁탈모드 세팅
                // DISABLED_MGRS: RacePlayGameInfo info = Mgrs.userData.raceInfo;
                bHome = info.home;

                // DISABLED_MGRS: RacePlayTeamInfo homeTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.homeTeamNo);
                // DISABLED_MGRS: RacePlayTeamInfo awayTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.awayTeamNo);

                SimulPlayerManager.homeTeamIndex = (int)(homeTeam.team);
                SimulPlayerManager.awayTeamIndex = (int)(awayTeam.team);
                SimulPlayerManager.strHomeTeam = homeTeam.name;
                SimulPlayerManager.strAwayTeam = awayTeam.name;

                lineup = SimulManager.GetRaceGameLineup(info.homeLineup, info.awayLineup);
#endif
            }
            else//if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                nineTwoRoundScore = new int[10];
                nineTwoRound = 1;
                nineTwoScore = 0;
                for (int i = 0; i < 10; i++) nineTwoRoundScore[i] = -1;
#if _Test_Local
                SimulPlayerManager.awayTeamIndex = Random.Range(1, 11);
                SimulPlayerManager.homeTeamIndex = Random.Range(1, 11);
                bHome = true;
                SimulPlayerManager.strAwayTeam = SimulPlayerManager.GetTeamName(false);
                SimulPlayerManager.strHomeTeam = SimulPlayerManager.GetTeamName(true);
                bNineTwoNextRound = false;
#else
                // DISABLED_MGRS: WalkoffPlayGameInfo info = Mgrs.userData.walkoffInfo;
                SimulPlayerManager.cpuTeamIndex = SimulPlayerManager.awayTeamIndex = (int)info.otherTeam;
                SimulPlayerManager.myTeamIndex = SimulPlayerManager.homeTeamIndex = (int)info.myTeam;
                bHome = true;
                SimulPlayerManager.strAwayTeam = SimulPlayerManager.GetTeamName(false);
                SimulPlayerManager.strHomeTeam = info.myName;
                bNineTwoNextRound = false;
#endif
            }
            bMyTurn = bHome ? false : true;
            StartCoroutine(initLineup(bHome, lineup));
            bCurrentChanceModeState = false;            
        }

        
        /// <summary>
        /// initGameSetting에서 호출되는 함수로서 서버로부터 받아온 라인업을 세팅한다
        /// </summary>
        private IEnumerator initLineup(bool bHome, GameLineup lineup)
        {
            //////UnityEngine.//Debug.Log("==================>>>>정식플레이는 여기서부터 시작");
            Mode.cameraView = CameraView.BatterLow;
            if (Mode.bPitchingViewActive == true)
            {
                Mode.cameraView = (bMyTurn == true ? CameraView.BatterLow : CameraView.PitcherCenter);
            }
            bPitcherChangeException = true;
            SimulPlayerManager.SetInit();
            bMyHome = bHome;
            bMyTurn = bHome ? false : true;

#if _Test_Local            
            SimulPlayerManager.strMyTeam = bMyHome ? SimulPlayerManager.strHomeTeam : SimulPlayerManager.strAwayTeam;
            SimulPlayerManager.strCPUTeam = bMyHome ? SimulPlayerManager.strAwayTeam : SimulPlayerManager.strHomeTeam;
            SimulPlayerManager.myTeamIndex = bMyHome ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex;
            SimulPlayerManager.cpuTeamIndex = bMyHome ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex;

            yield return new WaitForSeconds(0.1f);
            //로컬 테스트용
            for (int i = 0; i < 5; i++)
            {
                SimulPlayerManager.MakePlayerLocal(i);
                yield return new WaitForSeconds(0.1f);
            }
            SimulManager.InitGame(bMyHome, null);
#else
            SimulPlayerManager.strMyTeam = bMyHome ? SimulPlayerManager.strHomeTeam : SimulPlayerManager.strAwayTeam;
            SimulPlayerManager.strCPUTeam = bMyHome ? SimulPlayerManager.strAwayTeam : SimulPlayerManager.strHomeTeam;
            SimulPlayerManager.myTeamIndex = bMyHome ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex;
            SimulPlayerManager.cpuTeamIndex = bMyHome ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex;

            int myStarterOdr, otherStarterOdr;

            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                //otherStarterOdr = Random.Range(1, 6);
                //myStarterOdr = Random.Range(1, 6);
                // DISABLED_MGRS: WalkoffPlayGameInfo info = Mgrs.userData.walkoffInfo;
                SimulPlayerManager.MakePlayerWalkOff(info);
                SimulManager.InitGame(bMyHome, lineup);
            }
            else
            {
                if (Mode.gameMode == Mode.GamePlayMode.Season)
                {
                    //시즌 선발
                    // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;
                    otherStarterOdr = myStarterOdr = Mathf.Clamp(info.starterOdr,1,5);
                }
                else if (Mode.gameMode == Mode.GamePlayMode.Pvp)
                {
                    //라이브 매치 선발
                    // DISABLED_MGRS: otherStarterOdr = myStarterOdr = Mathf.Clamp(Mgrs.userData.livePlayGmaeInfo.starterOdr,1,5);//
                    //Debug.Log("=================>>> 라이브 매치 선발 =" + myStarterOdr);
                }
                else //if (Mode.gameMode == Mode.GamePlayMode.Ranking)
                {
                    //랭킹 선발
                    otherStarterOdr = Random.Range(1, 6);
                    myStarterOdr = Random.Range(1, 6);
                }
                SimulPlayerManager.MakePlayer(bMyHome, lineup, myStarterOdr, otherStarterOdr); 
                SimulManager.InitGame(bMyHome, lineup);
            }
#endif
            yield return null;
            
            //누적 기록 설정
#if !_Test_Local
            if (Mode.gameMode == Mode.GamePlayMode.Season)
            {
                //시즌 누적 기록
                // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;
                setRecordSetting(info.homeRecInfo, info.awayRecInfo, bHome);
            }
#endif
        }

        /// <summary>
        /// 모든 로딩과 세팅이 완료된후 게임을 실행한다
        /// </summary>
        public void startGame()
        {

            //최초 초기화
            initRule();
            saveGame2();
            playState = PlayState.PLAY_INIT_INNING;
            bStrike = bStrikeOut = bThreeOutChange = false;

            //보통게임의 초기화
            //이닝 초기화
            initInning(true);
            //UI초기화
            IngameUI.GetScoreBoard().Init(this);
            IngameUI.GetPlayerInfo().Init(this);
            IngameUI.GetInningChangeUI().Init();

            //결과 플래그 -> 결과 두번발생 방지
            bResultInit = false;

            if (Mode.bPvpMode433 == false)
            {
                //퀵시뮬레이터 초기화
                simulator.init(this);
                simulator.gameObject.SetActive(false);
            }
            //bUpdate = true;

        }


        //게임 룰 초기화 - 게임시 최초 1번 호출
        /// <summary>
        /// 룰 초기화 - 게임 플레이시 최초 1번 호출
        /// </summary>
        private void initRule()
        {
            //Debug.Log("InitRule");
            bTopInning = true; //이닝 플래그 초기화
            bGoodByeHitCall = false; //끝내기 플래그 초기화

            //홈 어웨이 여부에 따른 내 선공
            bMyTurn = bMyHome ? false : true;

            //오펜스 데펜스 인덱스 초기화
            offenseIndex = (bMyTurn ? 0 : 1);
            defenseIndex = (bMyTurn ? 1 : 0);

            //세이브
            //nSaveIndex = nLoserIndex = nWinnerIndex = nHoldIndex = -1;	//승리투수 조건 갖춘 사람 없음	

            bGameSet = false;
            bTopInningFinish = false;
            bPlayBall = true;
            nInningCount = 1;
            resetCount();

            for (int i = 0; i < 2; i++)
            {
                //각종 카운트 초기화
                nGameScore[i] = 0;
                nCurScore[i] = 0;
                nHitCount[i] = 0;
                nErrorCount[i] = 0;
                nFourballCount[i] = 0;
                nStrikeOutCount[i] = 0;
                nHomerunCount[i] = 0;
                nDPCount[i] = 0;
                nStealCount[i] = 0;
                //nPickOffOuntCount[i] = 0;
                //역전 플래그 초기화
                bTurnAroundFlag[i] = false; 
                //승패 인덱스 초기화
                winPitcherIndex[i] = -1;
                losePitcherIndex[i] = -1;
                //투수관련 초기화
                int starter = SimulPlayerManager.GetStarterIndex(i);
                pitcher.setPitcherChange(SimulPlayerManager.GetPitcher(i, starter), starter, i, true);
                pitcher.setChangeFlagInit(i);
            }

            for (int i = 0; i < SimulGameInfo.MAX_INNING; i++)
            {
                //이닝의 점수 초기화
                nInningScore[0, i] = 0;
                nInningScore[1, i] = 0;
            }

            //벤치 위치
            field.setBenchPosition(); //벤치 포지션 초기화
            bPitcherChangeFlag = false;
            bBatterChangeFlag = false;
            bFielderChangeFlag = false;
            bRunnerChangeFlag = false;

            if (Mode.bTieBreaker == true)
            {
                nInningCount = 10;
            }

            if(Mode.bPvpMode433 == true)
            {
                nInningCount = 9;
            }

        }



        //////////////////////////////////////////////////////////////////////////////
        // 이닝 초기화 (매이닝 호출)
        //////////////////////////////////////////////////////////////////////////////
        //이닝을 초기화 - 매 이닝 체인지시 호출
        private void initInning(bool bStartGame)
        {
            //UI 초기화
            IngameUI.GetControlRunner().Init(this);
            IngameUI.GetPitchingSelect().Init(this);
            IngameUI.GetScoreBoard().SetActive(false, false);
            IngameUI.GetPlayerInfo().SetActive(false, false);
            IngameUI.GetBattingCall().SetActive(false);
            IngameUI.GetFieldCall().SetActive(false);
            IngameUI.GetScoreShow().SetInit(this);
            IngameUI.GetFieldUI().SetActive(false);

            batter.lastBatterHand = -1;
            bPlayBall = false;
            if (nInningCount == 1 && bTopInning) bPlayBall = true;

            bBatterForceLoad = true; //타자 강제 로딩함ㅎ
            initInningParameter(false);

            if (bStartGame == true)
            {
                //게임 스타트시
                setFieldBack();                
                inningTime = 0;
#if _Local_Balance
                if (InGameDebug.EVENT_SKIP_MODE == true) 
                {
                    //이부분은 나중에 지울수도
                    setChangeInning(false);
                    field.ball.setActive(false);
                    field.ball.setFirstBound(false);
                }
                else 
#endif
                {
                    //playState = PlayState.PLAY_START_GAME;
                    inningTime = 0;
                    field.ball.setActive(false);
                    setChangeInning(false);                    
                }
            }
            else
            {
                //이닝 체인지시
                setChangeInning(false);
                field.ball.setActive(false);
                field.firstBound.SetActive(false);
            }
        }

        /// <summary>
        /// 타이브레이커 이닝 초기화
        /// </summary>
        private void tieBreakerInit()
        {
            ////UnityEngine.//Debug.Log("=======================================================>>tieBreakerInit :: bMyTurn = " + bMyTurn);            

            int curIndex = (bMyTurn ? 0 : 1);
            int pvpBatterOrder = 2;
            if (Mode.bPvpMode433 == true)
            {
                pvpBatterOrder = pvpmanager.Get().batterOrder[curIndex];
            }
            
            SimulPlayerManager.SetLineup(curIndex, pvpBatterOrder);  //3번타자 세팅

            int tb = ((pvpBatterOrder - 2) + 9) % 9;
            CPlayer secondRunner = SimulPlayerManager.GetFielder(curIndex, tb);   //1번타자 2루주자
            field.run.makeChanceRunner(secondRunner, FieldParm.THIRDBASE_INDEX);

            int fb = ((pvpBatterOrder - 1) + 9) % 9;
            CPlayer firstRunner = SimulPlayerManager.GetFielder(curIndex, fb);   //2번타자 1루주자
            field.run.makeChanceRunner(firstRunner, FieldParm.FIRSTBASE_INDEX);

            
            field.run.runnerRegenIndex = 2;
            field.run.nHitterRunnerIndex = 1;
            field.run.bHitterRunnerSafe = true;

            field.run.bOnBase[FieldParm.THIRDBASE_INDEX] = true;
            field.run.bOnBase[FieldParm.FIRSTBASE_INDEX] = true;

            nOutCount = 1;

        }


        /// <summary>
        /// 투아웃 만루모드 이닝 초기화
        /// </summary>
        private void twoOutBaseLoadModeInit()
        {
            ////UnityEngine.//Debug.Log("=======================================================>>tieBreakerInit :: bMyTurn = " + bMyTurn);            
            nInningCount = 9;
            nOutCount = 2;
            bTopInning = false;
            bMyTurn = true;

            IngameUI.GetFieldUI().SetCountUpdate(nOutCount);

            int curIndex = (bMyTurn ? 0 : 1);

            //임시
            int curLineup = 0;
            
            SimulPlayerManager.SetLineup(curIndex, curLineup);  //타자세팅

            int runnerIndex;

            runnerIndex = (curLineup + 9 - 3) % 9;
            CPlayer thirdRunner = SimulPlayerManager.GetFielder(curIndex, runnerIndex);   //3루주자
            field.run.makeChanceRunner(thirdRunner, FieldParm.THIRDBASE_INDEX);

            runnerIndex = (curLineup + 9 - 2) % 9;
            CPlayer secondRunner = SimulPlayerManager.GetFielder(curIndex, runnerIndex);   //2루주자
            field.run.makeChanceRunner(secondRunner, FieldParm.SECONDBASE_INDEX);

            runnerIndex = (curLineup + 9 - 1) % 9;
            CPlayer firstRunner = SimulPlayerManager.GetFielder(curIndex, runnerIndex);   //1루주자
            field.run.makeChanceRunner(firstRunner, FieldParm.FIRSTBASE_INDEX);
            

            field.run.runnerRegenIndex = 3;
            field.run.nHitterRunnerIndex = 1;
            field.run.bHitterRunnerSafe = true;

            field.run.bOnBase[FieldParm.THIRDBASE_INDEX] = true;
            field.run.bOnBase[FieldParm.SECONDBASE_INDEX] = true;
            field.run.bOnBase[FieldParm.FIRSTBASE_INDEX] = true;


            walkOffBatter = SimulPlayerManager.GetBatter(0);
            batter.initBatter(walkOffBatter, 0); //타자 초기화 및 타자 텍스쳐 로딩
            field.run.makeHitterRunner(walkOffBatter);      //타자주자 초기화
            batter.initPosition();

            //다시 한번 호출
            IngameUI.GetControlRunner().Init(this);

            this.offenseIndex = 0;
            this.defenseIndex = 1;
        }

        /// <summary>
        /// 이닝 초기화시 타자 초기화
        /// </summary>
        private void initInningBatter()
        {
            //이닝 초기화와 동시에 타자 초기화
            CPlayer nextBatter;
            //다음타자 정보를 얻어옴
            nextBatter = SimulPlayerManager.GetBatter(offenseIndex);
            batter.initBatter(nextBatter, offenseIndex); //타자 초기화 및 타자 텍스쳐 로딩
            field.run.makeHitterRunner(nextBatter);      //타자주자 초기화
        }


        /// <summary>
        /// 승부치기 최초 타자 초기화
        /// </summary>
        public void tieBreakPvPBatterInit()
        {
            if (bTieBreakPvpBatterInit == false)
            {
                Debug.Log("승부치기 PVP 타자 초기화 타자 로딩도 병행");
                initInningBatter();
                if (bMyTurn == true)
                {
                    //승부타자 초기화시 여기서 타자 로딩
                    battingview.gameObject.SetActive(true);
                    batter.LoadBatter();
                    batter.bLoadBatterFlag = false;
                    //Debug.Log("======================>>> batterLoadFlag = " + batter.bLoadBatterFlag);
                }

                bTieBreakPvpBatterInit = true;
            }
        }

        /// <summary>
        /// 이닝 시작시 관련 파라메터 초기화
        /// </summary>
        /// <param name="bContinue"></param>
        private void initInningParameter(bool bContinue)
        {
            
            //이닝 초기화 (순서 주의)
            field.bFielderTextureInit1 = false;
            field.bFielderTextureInit2 = false;
            field.bFielderTextureInit3 = false;
            field.run.bRunnerTextureInit = false;

            //플래그 초기화(순서 주의)
            bChangeFlag = true;
            offenseIndex = (bMyTurn ? 0 : 1);
            defenseIndex = (bMyTurn ? 1 : 0);

            bBattingPreUpdate = false;

            if (bContinue == false)
            {
                //카운트 초기화
                resetCount();   
            }

            //카운트 업데이트
            IngameUI.GetFieldUI().SetCountUpdate(nOutCount);


            //주자 초기화
            if (bContinue == false)
            {
                field.run.initRunner();
            }

            //승부치기시(봉인)
            if (Mode.bPvpMode433 == true) //if (Mode.bTieBreaker == true)
            {
                //승부치기 초기화                
                tieBreakerInit();
                /*bTieBreakPvpBatterInit = false;
                if (bMyTurn == false)
                {
                    //수비시에만 미리 초기화
                    tieBreakPvPBatterInit();   
                }*/
                
            }
            //else
            {
                bTieBreakPvpBatterInit = true;
                if (Mode.b2outBaseLoadedMode == true)
                {
                    //2사만루 초기화                
                    twoOutBaseLoadModeInit();
                }
                else
                {     
                    /*이건 승부치기시 다시 오픈
                    if (Mode.bPvpMode == false || bMyTurn == false || Mode.bOnlyChanceMode == true)
                    {
                        initInningBatter();
                    }
                    else
                    {
                        bTieBreakPvpBatterInit = false;
                    }*/
                    //2사만루 이외의 모드 초기화  --> 타이브레이크 모드 봉인에 의해서 이걸로 퉁치면 됨
                    initInningBatter();
                }
            }
            batter.bGangTa = false;
            
            //이닝 초기화와 동시에 야수 초기화
            //field.initFielder(); //문제문제 -> 야수 리소스 로딩은 여기말고 다른곳에서
            StartCoroutine(field.initFielder2());
                        

            //필드 플래그 초기화
            field.bUpdateStealOrPickOff = false;
            field.ball.bFoulCall = field.ball.bHomeRunCall = false;
            field.fieldShift = 0;
            field.setFieldShift(field.fieldShift, false, false);
            
            //기타 이닝 플래그 초기화
            nBatterCount = 0;
            pitcher.bSetPosition = false;            
            bThreeOutChange = false;
            outCountIF = 0;

            //카메라 뷰 초기화
            //CameraView curView = (bMyTurn == true ? CameraView.BatterLow : CameraView.PitcherCenter);
            //battingview.settingView(curView);   //카메라 세팅에 따른 뷰 설정
        }


        /// <summary>
        /// 이닝 체인지시 관련 파라메터 초기화
        /// 반드시 이 함수가 먼저 호출되고 나중에 initInningParameter()가 호출되어야함
        /// </summary>
        private void initChangeInningParm()
        {
            //Debug.Log("initChangeInningParm : 여기서 턴이 바뀜");           
            //주자 파괴 체크
            field.run.checkDestroyRunner();
            //인덱스 변화
            offenseIndex = (bMyTurn ? 0 : 1);
            defenseIndex = (bMyTurn ? 1 : 0);
            
            //이닝카운트 초말 변화 체크
            if (!bTopInning)
            {
                //말 공격인 경우
                nInningCount++;	//말공격인 경우 이닝 변함
                if (nInningCount == 6)
                {
                    //6회에 선발투수 승리여부 플래그를 활성화
                    if (nGameScore[0] < nGameScore[1])
                    {
                        //다른 방법으로 하자 ㅋㅋ
                        //nWinnerIndex = 50 + lineup.getPIndex(1, lineup.nCurrentPitcherIndex[1]);
                    }
                    else if (nGameScore[0] > nGameScore[1])
                    {
                        //nWinnerIndex = lineup.getPIndex(0, lineup.nCurrentPitcherIndex[0]);
                    }
                }
            }
            bTopInning = !bTopInning;		//**주의** 초말 변함
            bMyTurn = !bMyTurn;				//**주의** 나의 턴 변함
            field.setBenchPosition();            
        }


        //이닝 체인지 셋팅 - 상태
        public void setChangeInning(bool changeInning, bool bChangeUI = true)
        {
            int i;
            //field.nHitBallType = -1;	//포볼
            field.ball.bFairBall = true;
            
            if (playState == PlayState.PLAY_BATTING_VIEW || changeInning == false)
            {
                field.initFieldVector();
                field.initFieldWhenChangeInning();// field.initField();
            }
            else
            {
                // field.ball.changeBallToAbsCoordinates();
            }

            field.ball.state = BallState.BALL_DEAD;
            field.ball.step = BallStep.None;

            IngameUI.GetScoreBoard().SetActive(false);

            if (changeInning)
            {
                //필승의지 체크                
                if (SimulManager.GetGameInfo().allowChulu == 0)
                {
                    if (pitcher.pPitcher.setPiledupSkill(SkillIndex.WinSpirit, 1, true) == true)
                    {
                        instantSkillEffect(pitcher.pPitcher, SkillIndex.WinSpirit, true);
                    }
                    SimulManager.GetGameInfo().allowChulu = -1;
                }

                field.run.runnerInit();
                
                bool bChangeCheck = false;

                if (Mode.bAutoPlay == true || bMyTurn == false)
                {
                    bChangeCheck = true;
                }
                SimulManager.SimulChangeInning(bChangeCheck);
                simualtorChangeInning = false;
                
                int curIndex = (bMyTurn ? 0 : 1);
                SimulPlayerManager.SetLineupCount(curIndex);
                saveGame2();
                playState = PlayState.PLAY_CHANGE_INNING;

                if (Mode.bPvpMode433 == false) //if (Mode.bPvpMode == false)
                {
                    if (SimulManager.GetChangerIndex(ChangerIndex.OutFielder) != -1)
                    {
                        bFielderChangeFlag = true;
                    }
                }
                                
                changeInningInit(bChangeUI);
                field.setChangeInningCamera(12.0f);

                StartCoroutine(changeInningDelay());
            }
            else
            {
                bool bStart = (nInningCount <= 1 ? true : false);

                if (Mode.bTieBreaker == true)
                {
                    bStart = (nInningCount <= 10 ? true : false);
                }

                if (Mode.bPvpMode433 == true)
                {
                    bStart = (nInningCount <= 9 ? true : false);
                }

                if (bMyTurn)
                {
                    for (i = 0; i < 9; i++)
                    {
                        field.fielder[i].setDstPos(bMyHome ? FieldSize.getAwayBenchPosX() : FieldSize.getHomeBenchPosX());
                    }
                }
                else
                {
                    for (i = 0; i < 9; i++)
                    {
                        field.fielder[i].setDstPos((bMyHome ? FieldSize.getHomeBenchPosX() : FieldSize.getAwayBenchPosX()));
                    }
                }
                for (i = 0; i < 4; i++)
                {
                    field.judge.judge[i].setStartGame();
                }

                field.setZoom(0.7f);                                
                if (nInningCount > 1)
                {
                    if (bMyTurn)
                    {
                        CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + (bMyHome ? FieldSize.getThirdBasePosX() : FieldSize.getFirstBasePosX()), BallPlayManager.FIELDVIEW_CAMERA_INITY + FieldSize.getMoundPosY(), -200));
                    }
                    else
                    {
                        CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + (bMyHome ? FieldSize.getFirstBasePosX() : FieldSize.getThirdBasePosX()), BallPlayManager.FIELDVIEW_CAMERA_INITY + FieldSize.getMoundPosY(), -200));                        
                    }
                }
                playState = PlayState.PLAY_START_INNING;
                if (Mode.b2outBaseLoadedMode == true)
                {
                    StartCoroutine(startNineTwoDelay());
                }
                else
                {
                    StartCoroutine(startInningDelay(bStart));
                }

            }
            inningTime = 0;
        }

        /// <summary>
        /// 상태 스킵
        /// </summary>
        public void setInningChangeSkip()
        {
            StopCoroutine("startInningDelay");
            
            endStartInning();
            
        }

        /// <summary>
        /// 스타트 이닝 연출
        /// </summary>
        public bool bFielderLoadComp;
        private IEnumerator startInningDelay(bool bStart)
        {
            bUISkip = false;
            if (bStart == true)
            {
                setFieldBack();
                Debug_UI.SetNetwork(false);
                if (bTopInning == true)
                {   
                    playState = PlayState.NONE;
                    Debug_UI.SetLoading(true);
                    while (bFielderLoadComp == false || bUpdate == false)
                    {
                        yield return new WaitForSeconds(0.3f);
                    }
                    Debug_UI.SetLoading(false);
                    yield return new WaitForSeconds(1.5f);
                    //스타팅 라인업 이벤트
                    IngameUI.StartingLineup(this);
                    playState = PlayState.PLAY_START_INNING;

                }
                else
                {
                    playState = PlayState.NONE;

                    //스타팅 라인업 이벤트
                    IngameUI.StartingLineup(this);
                }
                //yield return new WaitForSeconds(10.0f);
                yield return new WaitForSeconds(1.0f);

                endStartInning();
            }
            else
            {
                //시간 업데이트
                BackGroundManager.UpdateTime(nInningCount, false);

                bFielderLoadComp = false;                
                while (true)
                {
                    yield return new WaitForSeconds(0.4f);
                    if (bFielderLoadComp == true)
                    {
                        endStartInning();
                        break;
                    }
                }
            }
            System.GC.Collect();
        }

        private IEnumerator startNineTwoDelay()
        {
            bFielderLoadComp = false;
            bUISkip = false;
            //Debug_UI.SetLoading(true);
            setFieldBack();            
            playState = PlayState.NONE;            
            while (bFielderLoadComp == false)
            {
                yield return new WaitForSeconds(0.3f);
            }
            IngameUI.GetInningChangeUI().InitWalkOff(this);
            Debug_UI.SetLoading(false);
            yield return new WaitForSeconds(0.5f);
            playState = PlayState.PLAY_START_INNING;
            walkOffPitcher = field.fielder[CPlayer._PITCHER].pFielder;
            IngameUI.GetInningChangeUI().WalkOffActive(this, -1, 0);
            yield return new WaitForSeconds(3.0f);
            
            endStartInning(); 
            System.GC.Collect();
        }

        

        /// <summary>
        /// 스타트 이닝 연출 종료후 상태 세팅
        /// </summary>
        private bool bUISkip = false;
        private void endStartInning()
        {
            if (bUISkip == false)
            {
                if (Mode.bTieBreaker == true ||
                    (Mode.bPvpMode == true && bMyTurn == true))
                {
                    tieBreakPvPBatterInit();
                }

                FieldCrowdManager.SetCrowdActiveAll(false);

                bThreeOutChange = false;

                /*
                //버그때문에 이렇게 처리한거 같은데 나중에 다시 살펴볼것
                if (bTopInning == true && Mode.bSimulationQuickPlay == true)
                {
                    returnBattingView();
                }
                else
                {*/                    
                    if (Mode.bSimulationQuickPlay == true)
                    {
                        //이모티콘 채팅창 초기화
                        if (Mode.gameMode == Mode.GamePlayMode.Pvp ||
                            Mode.gameMode == Mode.GamePlayMode.Pvp433)
                        {
                            IngameUI.GetEmoticonChatting().Init(this);
                        }
                        else
                        {
                            IngameUI.GetEmoticonChatting().gameObject.SetActive(false);
                        }
                        Mode.PlayTypeFlag = Mode.ModeFlag.Auto;
                        bCurrentChanceModeState = false;
                        SimulManager.SetBattingviewSkill();
                        StartCoroutine(backToSimulation(false));
                    }
                    else
                    {
                        if (Mode.gameMode == Mode.GamePlayMode.Pvp ||
                                Mode.gameMode == Mode.GamePlayMode.Pvp433)
                        {
                            IngameUI.GetEmoticonChatting().Init(this);
                        }
                        else
                        {
                            IngameUI.GetEmoticonChatting().gameObject.SetActive(false);
                        }

                        Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
                        if (Mode.bTieBreaker == true)
                        {
                            //버그 해결용 - 어디서 잘못들어온건 여기서 강제로 보정                            
                            field.run.bOnBase[FieldParm.SECONDBASE_INDEX] = true;
                            field.run.bOnBase[FieldParm.FIRSTBASE_INDEX] = true;
                        }
                        else if (Mode.b2outBaseLoadedMode == true)
                        {
                            //버그 해결용 - 어디서 잘못들어온건 여기서 강제로 보정
                            field.run.bOnBase[FieldParm.THIRDBASE_INDEX] = true;
                            field.run.bOnBase[FieldParm.SECONDBASE_INDEX] = true;
                            field.run.bOnBase[FieldParm.FIRSTBASE_INDEX] = true;
                        }
                        for (int i = 0; i < 9; i++) field.fielder[i].setStop();
                        field.setTimeScale(Field.INIT_TIME_SCALE);

                        if (changeBattingView() == true)
                        {
                            returnBattingView();
                        }
                    }
                //}
                System.GC.Collect();
                bUISkip = true;
            }
        }

        /// <summary>
        /// 체인지 이닝 초기화
        /// </summary>
        private void changeInningInit(bool bChangeUI = true)
        {
            bChangeInningSet = true;
            
            IngameUI.GetFieldUI().DestroyAllMinimapRunner();
            IngameUI.GetFieldUI().SetActive(false);

            field.setTimeScale(2.0f);
            inningTime = 0;
            field.setBenchFielding();
            resetCount();


            if (bGoodByeHitCall == true)
            {
                bGameSet = true;
                //bGoodByeHitCall = false;
            }
            else
            {
                bGameSet = checkEndGame();
            }

#if _Local_Balance
            if (InGameDebug.END_INNING_DIRECT_RESULT == true)
            {
                //로컬 밸런스  이닝 끝난후 게임셋
                bGameSet = true;
            }
#endif

            if (bGameSet == true)
            {
                //이부분 나중에 수정 - 게임셋                
                //SaveGame1(g_nGameType,2);
                //SaveGame2(g_nGameType);
                setResult();
                return;
            }


            if (bChangeUI == true)
            {
                IngameUI.GetInningChangeUI().SetActive(this);
            }

            bInningChange = true;
        }

        /// <summary>
        /// 체인지 이닝 연출
        /// </summary>        
        private IEnumerator changeInningDelay()
        {
            yield return new WaitForSeconds(4.0f);

            changeInningSetting();
        }

        private bool bChangeInningSet = false;
        public void changeInningSetting()
        {
            if (bChangeInningSet == true)
            {
                field.setTimeScale(Field.INIT_TIME_SCALE);
                inningTime = 0;
                if (bGameSet == true)
                {
                    //게임셋 처리
                    setResult();
                }
                else
                {
                    initChangeInningParm();
                    initInning(false);
                }
                bChangeInningSet = false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////
        // 결과
        //////////////////////////////////////////////////////////////////////////////
        //게임 셋 및 결과창 세팅 - 상태
        private bool bResultInit = false; //두번 발생하는거 방지
        public void setResult()
        {
            if (bResultInit == false)
            {
                bResultInit = true;
                Debug_UI.SetNetwork(true);
                //아웃게임 일부기능 활성화
                //loadManager.setOutGameManager(true);
                FieldCrowdManager.SetCrowdActiveAll(false);
                IngameUI.GetScoreBoard().SetActive(false);
                field.bFieldViewActive = false;
                field.run.checkDestroyRunner();
                //카메라 세팅
                setFieldBack();
                simulator.gameObject.SetActive(false);
                //Destroy(simulator.gameObject);

                //실제 게임용
                //신을 종료하고 리절트 화면으로
                playState = PlayState.NONE;
                //결과세팅
#if _Local_Balance
                if (InGameDebug.GOODBYE_HIT_DIRECT_RESULT == true || InGameDebug.END_INNING_DIRECT_RESULT == true)
                {
                    //강제 점수 설정
                    SimulManager.GameSimulate();
                    SimulManager.SyncData(this, true, false, false);
                    setInningScoreClose(bTopInning);
                }
#endif



#if _Test_Local
            SimulManager.SimulResultSetting(this, true);
            //아무처리 안함
            LoadGameResult(); 
#else
                //현재 게임 결과를 결과리스트에 추가
                // DISABLED_MGRS: DefineEnum.EGameMode mode = Mgrs.userData.GetUserGameMode();
                if (mode == DefineEnum.EGameMode.LiveMatch)
                {
                    //라이브매치
                    StartCoroutine(PvpResultSync());
                }
                else
                {
                    SimulManager.SimulResultSetting(this, true);
                    if (mode == DefineEnum.EGameMode.Season || mode == DefineEnum.EGameMode.SeasonConsecutive)
                    {
                        //시즌
                        // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;
                        SimulPlayerManager.AddSeasonResult(info.myScheNo, info.home);
                        SimulPlayerManager.AddPitcherResult();
                    }
                    else if (mode == DefineEnum.EGameMode.Rank)
                    {
                        //랭킹
                        // DISABLED_MGRS: RankedPlayGameInfo info = Mgrs.userData.Ingame_rankInfo;
                    }
                    else if (mode == DefineEnum.EGameMode.Walkoff)
                    {
                        //현재 처리 안해줌
                    }
                    LoadGameResult();
                }
#endif
            }
        }

        public void LoadGameResult()
        {
            MusicManager.Get().StopMusic();
            Util.Load("MainGame/prefabs/resultUI/resultPrefab", null, Vector3.zero).GetComponent<ResultUI>().Init(this);
        }

        /// <summary>
        /// PVP모드에서 호스트와 게스트의 결과를 동기화
        /// </summary>
        /// <returns></returns>
        private IEnumerator PvpResultSync()
        {
            bool bHost = simulator.IsHost();
                        
            if (bHost == true)
            {
                //호스트인 경우 결과를 세팅한후 동기화 정보 보냄
                SimulManager.SimulResultSetting(this, true);
                PvpManager.GetInstance().SendResultSyncInfo(true);
                PvpManager.rState = PvpManager.RecieveState.ResultSync;
            }

            //결과 동기화 대기
            while (true)
            {
                if (PvpManager.rState == PvpManager.RecieveState.ResultSync)
                {
                    break;
                }
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(0.3f);

            if (bHost == false)
            {
                //게스트인 경우 결과 정보를 받은후 결과 세팅 (단 투수의 기록은 세팅하지 않는다)
                PvpManager.GetInstance().SetResultSync();
                SimulManager.SimulResultSetting(this, false);
            }

            LoadGameResult();
            Debug.Log("rState를 None으로 세팅");
            PvpManager.rState = PvpManager.RecieveState.None;
        }


        //빠른 결과를 본다(과금)
        public IEnumerator setFastResult()
        {            
            Mode.bSimulationQuickPlay = false;            
            
            simulationRecordException();    //기록 예외 처리를 해준다

            for (int i = 0; i < 9; i++)
            {
                field.fielder[i].setStop();
                field.fielder[i].setInitPosition();
            }
            
            setChangeBack();

            yield return new WaitForSeconds(0.3f);

            Mode.bSimulationQuickPlay = true;
            simulator.gameObject.SetActive(true);

            yield return new WaitForSeconds(1.0f);

            playState = PlayState.PLAY_FAST_INNING_SIMUL;
        }

        //바로 1이닝 스킵 (그냥 스킵)
        public IEnumerator setFastNextInning()
        {
            playState = PlayState.NONE;
            simulationRecordException();    //기록 예외 처리를 해준다
            bool bChangeInningProcess = false; //한이닝만 체크할경우 이닝 체인지 프로세스를 하지 않는다 -> 외부 엔진에서 해줌
            SimulManager.SimulNextInning(bChangeInningProcess);
            SimulGameInfo gameInfo = SimulManager.GetGameInfo();

            yield return new WaitForSeconds(0.5f);
            
            bPitcherChangeException = false;
            //데이터 싱크
            SimulManager.SyncData(this, true, true, false); //시뮬레이션 -> 게임엔진 동기화

            bThreeOutChange = true; //이닝 체인지를 위해
            
            if (changeBattingView() == true)
            {
                returnBattingView();
            }            
            if (bGameSet == false)
            {
                CameraManager.SetZoomFactor(1);
                CameraManager.SetBatterCameraZoomFactor(1);
                ControlManager.CameraEnable(true);
                IngameUITrans.gameObject.SetActive(true);
            }
        }

        /*
        //빠른 다음 이닝을 본다 (전광판 나오는 버전)
        public IEnumerator setFastNextInning()
        {
            simulationRecordException();    //기록 예외 처리를 해준다
            
            for (int i = 0; i < 9; i++)
            {
                field.fielder[i].setStop();
                field.fielder[i].setInitPosition();
            }

            setChangeBack();

            yield return new WaitForSeconds(0.3f);

            ////UnityEngine.//Debug.Log("=====================================>>이닝 시뮬레이터");
            Util.Load("MainGame/prefabs/QuickUI/fastSimulInningPrefab", null, new Vector3(0, 0, 0));
            FastInningSimulator.FastSimulOneInning(this);

            yield return new WaitForSeconds(1.0f);

            playState = PlayState.PLAY_FAST_INNING_SIMUL;
        }*/



        //플레이 하지 않은 이닝을 클로즈해버린다.
        private void setInningScoreClose(bool topInning)
        {
            for (int i = (nInningCount); i < SimulGameInfo.MAX_INNING; i++)
            {
                nInningScore[0, i] = SimulParm.NOPLAY_INNING;// -2000;
                nInningScore[1, i] = SimulParm.NOPLAY_INNING;// -2000;
            }

            int bottom = bMyHome ? 0 : 1;
            if (topInning == true)
            {
                nInningScore[bottom, nInningCount - 1] = SimulParm.GAMEEND_INNING;// -1000;
            }
            else
            {
                nInningScore[bottom, nInningCount - 1] = -nInningScore[bottom, nInningCount - 1];
            }
        }

        //게임 종료 조건 검색 - 상태
        public bool checkEndGame()
        {
            int topScore, bottomScore;

            if (Mathf.Abs(getScoreGab(offenseIndex)) >= SimulGameInfo.ColdGame)
            {
                if (bTopInning == false)
                {
                    //콜드 게임 종료
                    //UnityEngine.Debug.Log("##################게임최종결과################################");
                    ////UnityEngine.//Debug.Log("====================>>콜드 게임으로 게임 종료");
                    ////UnityEngine.Debug.Log("MYTEAM " + gameInfo.run[MYTEAM] + "  :  " + "CPUTEAM " + gameInfo.run[CPUTEAM]);
                    //UnityEngine.Debug.Log("##################게임최종결과################################");
                    setInningScoreClose(bTopInning);
                    return true;
                }
            }

            if (nInningCount >= Mode.maxInning)  //연장포함
            {
                if (bTopInning == false)
                {
                    //연장 12회말끝나면 
                    //UnityEngine.Debug.Log("##################게임최종결과################################");
                    ////UnityEngine.//Debug.Log("====================>>연장 12회 종료로 게임 끝");
                    ////UnityEngine.Debug.Log("MYTEAM " + gameInfo.run[MYTEAM] + "  :  " + "CPUTEAM " + gameInfo.run[CPUTEAM]);
                    //UnityEngine.Debug.Log("##################게임최종결과################################");
                    setInningScoreClose(bTopInning);
                    return true;
                }
            }

            //정규이닝
            if (nInningCount >= Mode.finalInning)
            {
                if (bTopInning == true)
                {
                    //초공격 하는 팀의 스코어
                    topScore = nGameScore[offenseIndex];
                    //말공격 하는 팀의 스코어
                    bottomScore = nGameScore[defenseIndex];
                    if (bottomScore > topScore)
                    {
                        setInningScoreClose(bTopInning);
                        return true;
                    }
                }
                else
                {
                    if (nGameScore[0] != nGameScore[1])
                    {
                        //9회이후 두팀의 점수가 다른 경우 게임셋
                        setInningScoreClose(bTopInning);
                        return true;
                    }
                }
            }

            return false;
        }

        //굿바이 게임 조건 검색 - 상태
        public bool checkGoodByeGame()
        {
#if _Local_Balance
            if (InGameDebug.GOODBYE_HIT_DIRECT_RESULT == true)
            {
                //로컬밸런스로 무조건 굿바이 상황발생
                bGoodByeHitCall = true;
                return true;
            }
#endif

            if (nInningCount >= Mode.finalInning)
            {
                if (bTopInning == false)
                {
                    if (nGameScore[offenseIndex] > nGameScore[defenseIndex])
                    {
                        //말 이닝에서 필드상에서 점수가 틀리게 되는 경우 게임셋
                        setInningScoreClose(bTopInning);
                        bGoodByeHitCall = true;
                        SimulManager.SetGoodBye(true);
                        return true;
                    }
                }
            }
            return false;            
        }


        //스코어 차이를 구하기 - 상태
        public int getScoreGab(int team)
        {
            return (nGameScore[team] - nGameScore[1 - team]);
        }

        //공격이 얻은 점수 - 상태
        public int getOffeseScore()
        {
            return nGameScore[offenseIndex];
        }

        //수비가 얻은 점수 - 상태
        public int getDefenseScore()
        {
            return nGameScore[defenseIndex];
        }

        public int offenseWinningGab()
        {
            return (nGameScore[offenseIndex] - nGameScore[defenseIndex]); 
        }

        //잠재 허용 점수 - 상태
        public int potentialScoreLoss()
        {
            int score = 0;
            for (int i = 0; i < 3; i++)
            {
                if (field.run.bOnBase[i] == true) score++;
            }
            return score;
        }


        //////////////////////////////////////////////////////////////////////////////
        // 기록 관련 메쏘드
        //////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 투수기록 적립
        /// </summary>
        public void addPitcherRecord(int type, bool bCurrent = true, int num = 1)
        {
            if (Mode.bSimulationQuickPlay == false)
            {
                ////UnityEngine.Debug.Log("[엔진기록]=======================>>>투수: " + Param.debug_stat[type]);
                pitcher.addRecord(type, bCurrent, num);
            }
        }

        /// <summary>
        /// 타자기록 적립
        /// </summary>
        public void addBatterRecord(int type, int num = 1)
        {
            if (Mode.bSimulationQuickPlay == false)
            {
                //UnityEngine.Debug.Log("[엔진기록]=======================>>>타자: " + Param.debug_stat[type]);
                batter.addRecord(type, num);
            }
        }

        /// <summary>
        /// 야수기록 적립
        /// </summary>
        public void addFielderRecord(int type, int index, int num = 1)
        {

            if (Mode.bSimulationQuickPlay == false)
            {
                ////UnityEngine.Debug.Log("[엔진기록]=======================>>>야수: " + Param.debug_stat[type]);
                field.fielder[index].addRecord(type, num);
            }
        }

        /// <summary>
        /// 주자기록 적립
        /// </summary>
        public void addRunnerRecord(int type, int index, int num = 1)
        {
            if (Mode.bSimulationQuickPlay == false)
            {
                ////UnityEngine.Debug.Log("[엔진기록]=======================>>>주자: " + field.run.runner[index].pRunner.getName());
                field.run.runner[index].addRecord(type, num);
            }
        }

        //시뮬레이션으로 돌리는경우 기록 저장의 예외처리
        public void simulationRecordException()
        {
            addBatterRecord(Param.ST_PA, -1);           //타자 타수 감소
            addBatterRecord(Param.ST_AB, -1);           //타자 타석 감소
            addPitcherRecord(Param.ST_TBF, true, -1);         //투수 피타수 감소
        }

        /// <summary>
        /// 아웃 플래그 초기화
        /// </summary>
        public void setOutFlagInit()
        {
            outFlag = 0;
        }

        /// <summary>
        /// 특정 아웃 플래그로 세팅
        /// </summary>
        public void setOutFlag(int flag)
        {
            /*
            public const int _SO_FLAG = 0x01;   //삼진
        public const int _DP_FLAG = 0x02;   //병살    
        public const int _K_FLAG = 0x04;    //삼진(투수)    
        public const int _PO_FLAG = 0x08;   //자살
        public const int _A_FLAG = 0x10;    //보살
        public const int _CS_FLAG = 0x20;    //도루저지
        public const int _SBF_FLAG = 0x40;    //도루저지*/
            outFlag |= flag;
        }

        /// <summary>
        /// 아웃 카운트 적립
        /// </summary>
        public void addOutCount()
        {
            ////UnityEngine.//Debug.Log("==============>>addOutCount");
            pitcher.setPinchScoreReduce(3); //핀치스코어 -3
            pitcher.conHit = pitcher.conHR = 0;

            fieldOutCountNum++;
            if (fieldOutCountNum >= 2)
            {
                //병살 플래그            
                setOutFlag(_DP_FLAG);
            }

            //투수
            addPitcherRecord(Param.ST_IP);  //이닝 카운트

            if ((outFlag & _DP_FLAG) == _DP_FLAG)
            {
                ////UnityEngine.//Debug.Log("==============>>병살");
                strBatterResult = strFieldOutType + " 병살";
                nDPCount[offenseIndex]++;
                addBatterRecord(Param.ST_DP);  //병살
                //setAp(-2, offenseIndex);
            }
            else
            {
                ////UnityEngine.//Debug.Log("==============>>혼자죽음");
                //이것들은 한번만 카운트 해줘            
                if ((outFlag & _K_FLAG) == _K_FLAG)
                {
                    nStrikeOutCount[defenseIndex]++;
                    addPitcherRecord(Param.ST_PSO);  //삼진
                    //setAp(2, defenseIndex);
                }

                if ((outFlag & _SO_FLAG) == _SO_FLAG)
                {
                    strBatterResult = "삼진 아웃";
                    addBatterRecord(Param.ST_SO);  //삼진
                    batter.pBatter.setResultStr("삼진");
                    //setAp(-2, offenseIndex);
                }
                else
                {
                    if (batter.buntSuccess != SimulBuntType.NONE)
                    {
                        addBatterRecord(Param.ST_AB, -1);     //타자 타석 - 희생시 다시 뺴줄것
                        strBatterResult = (batter.buntSuccess == SimulBuntType.SQUEEZE ? "스퀴즈 번트" : "희생 번트");
                        batter.pBatter.setResultStr("희생");
                        batter.buntSuccess = SimulBuntType.NONE;                        
                    }
                    else
                    {
                        if (batter.bSacFly == true)
                        {
                            addBatterRecord(Param.ST_AB, -1);     //타자 타석 - 희생시 다시 뺴줄것
                            strBatterResult = "희생플라이";
                            batter.pBatter.setResultStr("희생");
                            batter.bSacFly = false;
                        }
                        else
                        {
                            strBatterResult = strFieldOutType + " 아웃"; 
                            batter.pBatter.setResultStr(field.bOutByFlyball?"뜬공":"땅볼");
                        }
                    }
                }

                //야수
                if ((outFlag & _CS_FLAG) == _CS_FLAG)
                {
                    ////UnityEngine.//Debug.Log("==============>>도루 저지 기록 카운트");
                    addFielderRecord(Param.ST_CS, CPlayer._CATCHER);  //도루저지
                }
                //주자
                if ((outFlag & _SBF_FLAG) == _SBF_FLAG)
                {
                    ////UnityEngine.//Debug.Log("==============>>도루자 기록 카운트 인덱스: " + sbfIndex);
                    //Runner stealRunner = field.run.runner[sbfIndex];
                    //int dstPos = (stealRunner.lastPos + 1);
                    //SimulManager.AddGameSummuryInfo("\n-" + (stealRunner.lastPos + 1) + "루주자 " + stealRunner.pRunner.getName() + ": " + (dstPos == FieldParm.HOMEBASE_INDEX?"홈스틸": (stealRunner.lastPos + 2) + "루 도루") +" 실패");
                    addRunnerRecord(Param.ST_SBF, sbfIndex);  //도루실패
                }

            }

            //야수
            if ((outFlag & _PO_FLAG) == _PO_FLAG) addFielderRecord(Param.ST_PO, poIndex);  //자살
            if ((outFlag & _A_FLAG) == _A_FLAG) addFielderRecord(Param.ST_A, aoIndex);  //보살

#if _Local_Balance
            if (InGameDebug._NO_OUT_COUNT == true)
            {
                //로컬 밸런스
                //아웃카운트를 해주지 않는다.
            }
            else
#endif
            {
                nOutCount++;
                outCountIF++;
            }
            
            if (nOutCount > 2)	//쓰리아웃 체인지 
            {                
                bStrikeOut = false;
                bThreeOutChange = true;
            }

            IngameUI.GetFieldUI().SetCountUpdate(nOutCount);

            setOutFlagInit();
        }

        /// <summary>
        /// 낫 아웃 상태 세팅
        /// </summary>
        public void setNotOutSituation(bool bNotOut)
        {
            nStrikeOutCount[defenseIndex]++;
            addPitcherRecord(Param.ST_PSO);  //삼진
            //setAp(2, defenseIndex);
            addBatterRecord(Param.ST_SO);  //삼진
            batter.pBatter.setResultStr("삼진");
            //setAp(-2, offenseIndex);

            if (bNotOut == false)
            {
                strBatterResult = "삼진 아웃";
                addPitcherRecord(Param.ST_IP);  //이닝 카운트

                nOutCount++;
                outCountIF++;
                if (nOutCount > 2)	//쓰리아웃 체인지 
                {
                    bStrikeOut = false;
                    bThreeOutChange = true;
                }
            }            
            setOutFlagInit();
        }

        public int displayEffectType = 0;
        /// <summary>
        /// 안타 카운트
        /// </summary>
        public void setHitCount(int curBase)
        {
            //////UnityEngine.//Debug.Log("=========================================>>newOutCount = " + newOutCount);
            //////UnityEngine.//Debug.Log("=========================================>>nOutCount = " + nOutCount);
            if (nOutCount > newOutCount)
            {
                strBatterResult = strFieldOutType+" (야수선택)";
                batter.pBatter.setResultStr(field.bOutByFlyball ? "뜬공" : "땅볼");
                newOutCount = nOutCount;
                return;
            }

            displayEffectType = 1;

            SimulManager.GetGameInfo().allowChulu++;
            pitcher.conHit++;

            int currentBase = curBase;
            //안타수를 카운트한다.
            nHitCount[offenseIndex]++;

            //안타증가
            addPitcherRecord(Param.ST_PH); //투수 피안트 증가
            addBatterRecord(Param.ST_H);   //타자 안타 증가
            


            if (currentBase > FieldParm.FIRSTBASE_INDEX)
            {
                //닥터K효과 해제
                pitcher.pPitcher.setPiledupSkill(SkillIndex.DoctorK, 3, false);

                //장타 증가
                if (currentBase == FieldParm.SECONDBASE_INDEX)
                {
                    strBatterResult = strHitType2 + " 2루타";
                    addPitcherRecord(Param.ST_P2B); //투수 피2루타 증가
                    addBatterRecord(Param.ST_2B);   //타자 2루타 증가
                    batter.pBatter.setResultStr("2루타");
                    //setAp(3, offenseIndex);
                }
                else if (currentBase == FieldParm.THIRDBASE_INDEX)
                {
                    strBatterResult = strHitType2 + " 3루타";
                    addPitcherRecord(Param.ST_P3B); //투수 피3루타 증가
                    addBatterRecord(Param.ST_3B);   //타자 3루타 증가
                    batter.pBatter.setResultStr("3루타");
                    //setAp(5, offenseIndex);
                }
                else if (currentBase == FieldParm.HOMEBASE_INDEX)
                {
                    strBatterResult = strHitType2 + " 홈런";
                    pitcher.conHR++;
                    addPitcherRecord(Param.ST_PHR); //투수 피홈런 증가
                    addBatterRecord(Param.ST_HR);   //타자 홈런 증가
                    batter.pBatter.setResultStr("홈런");
                    //setAp(7, offenseIndex);
                    //setAp(-3, defenseIndex);
                    displayEffectType = 0;
                }
            }
            else
            {
                strBatterResult = strHitType+" 안타";
                batter.pBatter.setResultStr("안타");
                //setAp(2, offenseIndex);
            }

        }

        /// <summary>
        /// 포볼 카운트
        /// </summary>
        public void setFourballCount(bool bHitByPitch = false)
        {
            SimulManager.GetGameInfo().allowChulu++;
            pitcher.conHit++;

            nFourballCount[offenseIndex]++; //팀의 포볼 증가

            //BB 혹은 HBP증가
            if (bHitByPitch == false)
            {
                strBatterResult = "베이스 온 볼";
                addPitcherRecord(Param.ST_PBB); //투수 포볼 증가
                addBatterRecord(Param.ST_BB);   //타자 포볼 증가
                batter.pBatter.setResultStr("포볼");
                displayEffectType = 2;
            }
            else
            {
                strBatterResult = "몸에 맞는 볼";
                addPitcherRecord(Param.ST_PHBP); //투수 힛바이피치 증가
                addBatterRecord(Param.ST_HBP);   //타자 힛바이피치 증가
                batter.pBatter.setResultStr("사구");
            }
            addBatterRecord(Param.ST_AB, -1);     //타자 타석 증가 - 포볼시 다시 뺴줄것

            bBaseOnBalls = true;

        }

        /// <summary>
        /// 에러 카운트
        /// </summary>
        public void setErrorCount(bool bOneHitOneError)
        {
            SimulManager.GetGameInfo().allowChulu++;
            nErrorCount[offenseIndex]++;
            outCountIF++;

            if (bOneHitOneError == false)
            {
                strBatterResult = "에러로 출루";
                batter.pBatter.setResultStr("에러");                
            }
            int eIndex = field.nErrorFielder;
            addFielderRecord(Param.ST_E, eIndex);

            //자책 비자책 관련 세팅은 여기서
        }

        /// <summary>
        /// 도루 카운트
        /// </summary>
        public void setStealCount(int index)
        {
                
            nStealCount[offenseIndex]++;

            addRunnerRecord(Param.ST_SBS, index);

            addFielderRecord(Param.ST_SBA, CPlayer._CATCHER);  //도루허용

            //setAp(2, offenseIndex);
            //setAp(-1, defenseIndex);
        }

        /// <summary>
        /// 실시간 승리투수 패전투수 인덱스 체크
        /// </summary>
        public void checkWinLoseIndex()
        {
            if (nGameScore[0] > nGameScore[1])
            {
                winPitcherIndex[1] = -1;
                losePitcherIndex[0] = -1;

                if (winPitcherIndex[0] == -1)
                {
                    //myTeam 승리투수 조건 이 없은 경우
                    winPitcherIndex[0] = SimulPlayerManager.GetPitcherIndex(0);
                }
                if (losePitcherIndex[1] == -1)
                {
                    //cpuTeam 패배투수 조건이 없는 경우
                    losePitcherIndex[1] = SimulPlayerManager.GetPitcherIndex(1);
                }

            }
            else if (nGameScore[0] < nGameScore[1])
            {
                winPitcherIndex[0] = -1;
                losePitcherIndex[1] = -1;

                if (winPitcherIndex[1] == -1)
                {
                    //cpuTeam 승리투수 조건 이 없은 경우
                    winPitcherIndex[1] = SimulPlayerManager.GetPitcherIndex(1);
                }
                if (losePitcherIndex[0] == -1)
                {
                    //myTeam 패배투수 조건이 없는 경우                
                    losePitcherIndex[0] = SimulPlayerManager.GetPitcherIndex(0);
                }
            }
            else
            {
                winPitcherIndex[0] = winPitcherIndex[1] = -1;
                losePitcherIndex[0] = losePitcherIndex[1] = -1;
            }
        }

        //카운트 세팅 (new관련) - 룰
        public void setNewCount()
        {
            newStrikeCount = nStrikeCount;
            newBallCount = nBallCount;
            newOutCount = nOutCount;
        }

        //카운트 리셋 - 룰
        public void resetCount()
        {
            newOutCount = nOutCount = 0;
            newStrikeCount = nStrikeCount = 0;
            newBallCount = nBallCount = 0;

#if _Local_Balance
            if (InGameDebug._TWO_OUT_TEST == true)
            {
                //로컬밸런스 무조건 2아웃
                newOutCount = nOutCount = 2;
            }
#endif

        }


        //////////////////////////////////////////////////////////////////////////////
        // 시뮬레이션과 sync관련
        //////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 주자 상태 시뮬레이션 상태와 싱크
        /// </summary>
        public void syncRunner()
        {
            for (int i = 0; i < 4; i++)
            {
                field.run.runnerActive[i] = false;
                field.run.bOnBase[i] = false;
            }

            foreach (Transform runner in field.run.transform)
            {
                Runner curRunner = runner.GetComponent<Runner>();
                if (curRunner != null)
                {
                    if (curRunner.runnerIndex != field.run.nHitterRunnerIndex)
                    {
                        Destroy(curRunner.gameObject);
                    }
                    else
                    {
                        field.run.runnerActive[curRunner.arrayIndex] = true;
                    }
                }
            }


            bool[] onBase = new bool[3] { false, false, false }; //중첩방지용
            for (int i = 0; i < 4; i++)
            {
                SimulRunner curRunner = SimulManager.GetRunner(i);
                if (curRunner != null)
                {
                    if (curRunner.curPos == SimulParm.FIRSTBASE_INDEX)
                    {
                        if (onBase[SimulParm.FIRSTBASE_INDEX] == false)
                        {
                            ////UnityEngine.//Debug.Log("==================================>>>1루주자 생성");
                            field.run.makeChanceRunner(curRunner.getRunner(), SimulParm.FIRSTBASE_INDEX);
                            onBase[SimulParm.FIRSTBASE_INDEX] = true;
                        }
                    }
                    else if (curRunner.curPos == SimulParm.SECONDBASE_INDEX)
                    {
                        if (onBase[SimulParm.SECONDBASE_INDEX] == false)
                        {
                            ////UnityEngine.//Debug.Log("==================================>>>2루주자 생성");
                            field.run.makeChanceRunner(curRunner.getRunner(), SimulParm.SECONDBASE_INDEX);
                            onBase[SimulParm.SECONDBASE_INDEX] = true;
                        }
                    }
                    else if (curRunner.curPos == SimulParm.THIRDBASE_INDEX)
                    {
                        if (onBase[SimulParm.THIRDBASE_INDEX] == false)
                        {
                            ////UnityEngine.//Debug.Log("==================================>>>3루주자 생성");
                            field.run.makeChanceRunner(curRunner.getRunner(), SimulParm.THIRDBASE_INDEX);
                            onBase[SimulParm.THIRDBASE_INDEX] = true;
                        }
                    }
                }
            }

        }

        //////////////////////////////////////////////////////////////////////////////
        //LOAD & SAVE 함수
        //////////////////////////////////////////////////////////////////////////////
        public void saveGame1()
        {
            //////////UnityEngine.//Debug.Log("=========================>>Save Game 1");
        }

        public void loadGame1()
        {
            // ////////UnityEngine.//Debug.Log("=========================>>Load Game 1");
        }

        public void saveGame2()
        {
            //////////UnityEngine.//Debug.Log("=========================>>Save Game 2");
        }

        public void loadGame2()
        {
            //////////UnityEngine.//Debug.Log("=========================>>Load Game 1");
        }


        ///////////////////////////////////////////////////////////////////////////////
        //메인 쓰레드
        ///////////////////////////////////////////////////////////////////////////////
        public float inningTime;

        //메인 프레임 함수
        void updateFrame()
        {
            if (Mode.bPauseGame == true)
            {
                return;
            }
            
            
            switch (playState)
            {
                case PlayState.PLAY_BATTING_VIEW_READY:
                    battingviewReadyFrame();
                    break;
                //case PlayState.PLAY_BATTING_VIEW_PRE:	//이미지 로딩할것....
                //    battingViewPreFrame();
                //    break;                
                case PlayState.PLAY_BATTING_VIEW:
                    battingViewFrame();
                    break;                
                case PlayState.PLAY_FAST_INNING_SIMUL:
                    fastInnintSimulFrame();
                    break;             
                
                    /*
                case PlayState.PLAY_AI_PITCHER_CHANGE:
                case PlayState.PLAY_AI_BATTER_CHANGE:
                case PlayState.PLAY_AI_FIELDER_CHANGE:
                case PlayState.PLAY_AI_RUNNER_CHANGE:
                    playerChangeFrame(playState);
                    break;*/
            }

            //skillEffectFrame();
        }


        /// <summary>
        /// 배팅뷰 스킬 처리
        /// </summary>
        public QuickSimulator.SimulPreCalled simulCalled;   //시뮬레이션에서 이미 호출된경우
        public void checkBattingviewSkill()
        {
            //Debug.Log("==================>>simulCalled = " + simulCalled);
            if (simulCalled == QuickSimulator.SimulPreCalled.None)
            {
                //Debug.Log("==================>>액션 엔진에서 호출하는 경우");
                //시뮬에서 호출했는데 또 호출하면 초기화 되므로 이런 루틴을 탐
                vsType = SimulManager.CheckBattingviewSkill(this);
            }
            else if(simulCalled == QuickSimulator.SimulPreCalled.VsType)
            {
                //Debug.Log("==================>>VS상태 선 호출시 -> 없어진 스킬을 복원해야함");
                vsType = true;
                int [] buffer = SimulManager.GetSkillBuff();
                if (SimulManager.GetBatterSkill() == null)
                {
                    //타자 스킬이 없어진 경우 타자스킬 복원하고 투수승리 세팅
                    SimulManager.SetVsBatterWin(false);
                    int id = buffer[0];
                    SkillIndex sIndex = SimulParm.GetSkillEffect((int)id);
#if _Test_Local
                    CSkill skill = new CSkill(id, sIndex, false);
                    skill.rank = buffer[1];
#else
                    CSkill skill = new CSkill(id, sIndex, buffer[1], false);
#endif
                    SimulManager.SetBatterSkill(skill);
                }
                if (SimulManager.GetPitcherSkill() == null)
                {
                    //투수 스킬이 없어진 경우 투수스킬 복원하고 타자승리 세팅
                    SimulManager.SetVsBatterWin(true);
                    int id = buffer[2];
                    SkillIndex sIndex = SimulParm.GetSkillEffect((int)id);
#if _Test_Local
                    CSkill skill = new CSkill(id, sIndex, true);
                    skill.rank = buffer[3];
#else
                    CSkill skill = new CSkill(id, sIndex, buffer[3], true);
#endif
                    SimulManager.SetPitcherSkill(skill);
                }
            }
            else
            {
                //Debug.Log("==================>>시뮬엔진에서 미리 호출된경우 배팅뷰 스킬 체크 스킵");
                //이부분은 대결스킬인지 아닌지를 체크
                vsType = false;
            }

            simulCalled = QuickSimulator.SimulPreCalled.None; //초기화

            if (SimulManager.GetBatterSkill() != null || SimulManager.GetPitcherSkill() != null)
            {
                //스킬연출
                bSkillEffectFlag = true;
            }
            else
            {
                bSkillEffectFlag = false;
            }
            //매니저에서 사용하기 위한 전역플래그 설정
            setSkillFlag();
        }


        /// <summary>
        /// 투수교체 세팅
        /// </summary>
        public void pitcherChangeSetting()
        {
            bPitcherChangeFlag = false;
            CPlayer inPlayer = SimulPlayerManager.GetPitcher(defenseIndex, pitcher.inPitcher);
            CPlayer outPlayer = SimulPlayerManager.GetPitcher(defenseIndex, pitcher.outPitcher);

            bool bMyTeam = !bMyTurn;
            IngameUI.GetChangeEventUI().InitPlayerChangeUI(bMyTeam, this, outPlayer, inPlayer, UIPlayerChange.PlayerChangeType.PitcherChange, 0);

        }

        /// <summary>
        /// 타자교체 세팅
        /// </summary>
        public void batterChangeSetting()
        {
            bBatterChangeFlag = false;
            CPlayer inPlayer = batter.pBatter;
            int outIndex = SimulManager.GetChangerIndex(ChangerIndex.OutBatter);
            CPlayer outPlayer = SimulPlayerManager.GetFielder(offenseIndex, outIndex);

            bool bMyTeam = bMyTurn;
            IngameUI.GetChangeEventUI().InitPlayerChangeUI(bMyTeam, this, outPlayer, inPlayer, UIPlayerChange.PlayerChangeType.BatterChange, 0);
        }

        /// <summary>
        /// 야수교체 세팅
        /// </summary>
        public void fielderChangeSetting()
        {
            bFielderChangeFlag = false;
            
            int inIndex = SimulManager.GetChangerIndex(ChangerIndex.InFielder);
            int outIndex = SimulManager.GetChangerIndex(ChangerIndex.OutFielder);

            CPlayer inPlayer = SimulPlayerManager.GetFielder(defenseIndex, inIndex);            
            CPlayer outPlayer = SimulPlayerManager.GetFielder(defenseIndex, outIndex);

            bool bMyTeam = !bMyTurn;
            IngameUI.GetChangeEventUI().InitPlayerChangeUI(bMyTeam, this, outPlayer, inPlayer, UIPlayerChange.PlayerChangeType.FielderChange, inPlayer.getCurPos());
        }

        public void runnerChangeSetting()
        {
            bRunnerChangeFlag = false;
                        
            int inIndex = SimulManager.GetChangerIndex(ChangerIndex.InRunner);
            int outIndex = SimulManager.GetChangerIndex(ChangerIndex.OutRunner);
            int baseIndex = 0;

            CPlayer inPlayer = SimulPlayerManager.GetFielder(offenseIndex, inIndex);
            CPlayer outPlayer = SimulPlayerManager.GetFielder(offenseIndex, outIndex);
            
            bool bMyTeam = bMyTurn;
            IngameUI.GetChangeEventUI().InitPlayerChangeUI(bMyTeam, this, outPlayer, inPlayer, UIPlayerChange.PlayerChangeType.FielderChange, baseIndex);
        }
                
        /// <summary>
        /// 배팅뷰 전처리
        /// </summary>
        public bool bBattingPreUpdate = false;
        public IEnumerator battingviewPreDelay()
        {
#if _Skill_Display
            //연출테스트용
            InGameDebug.PitcherSkill = pitcherSkill_Display_test;
            InGameDebug.BitcherSkill = batterSkill_Display_test;
#endif
            if (Mode.bSiumlSetting == true)
            {                
                Mode.bSiumlSetting = false;
                playState = PlayState.PLAY_FAST_INNING_SIMUL;
                bCurrentChanceModeState = false;
                StartCoroutine(backToSimulation(false));
                yield break;
            }

            //투수교체 플래그 들어온 경우
            if (bPitcherChangeFlag == true)
            {
                //코루틴 중단하고 투수교체 이벤트 처리
                pitcherChangeSetting();
                yield break;
            }
            //타자교체 플래그
            if (bBatterChangeFlag == true)
            {
                //코루틴 중단하고 투수교체 이벤트 처리
                batterChangeSetting();
                yield break;
            }
            //야수교체
            if (bFielderChangeFlag == true)
            {
                //코루틴 중단하고 투수교체 이벤트 처리
                fielderChangeSetting();
                yield break;
            }
            
            //주자교체 플래그
            if (bRunnerChangeFlag == true)
            {
                //코루틴 중단하고 투수교체 이벤트 처리
                runnerChangeSetting();
                yield break;
            }

            ////Debug.Log("======================>>battingviewPreDelay");
            if (bBattingPreUpdate == false)
            {
                ////Debug.Log("================>> camera mode " + );
                int cameraState = CameraManager.GetCameraState();
                if (cameraState != BallPlayManager._BATTINGVIEW)
                {
                    IngameUI.GetFieldUI().SetChangeView(cameraState);
                }

               // //Debug.Log("======================>>battingviewPreDelay!!");
                bBattingPreUpdate = true;
                playState = PlayState.PLAY_BATTING_VIEW_PRE;
                battingview.setInitAngle();
                CameraManager.SetZoomFactor(1);
                CameraManager.SetBatterCameraZoomFactor(1);
                field.bReturnCheck = false;
                field.bReturnBattingView = false;
                field.setFieldShift(0, true, true);

                if (displayEffectType != 0)
                {
                    BackGroundManager.SetDisplayEffect(displayEffectType == 1 ? "DOME_02" : "BASS_ON_BALLS");
                    displayEffectType = 0;
                }

                //카메라 트윈 제거
                UITweener tween = CameraManager.GetInstance().GetComponent<UITweener>();
                if (tween != null)
                {
                    tween.enabled = false;
                }

                FieldCrowdManager.SetCrowdActiveAll(false);
                battingview.setCameraOffset();
                CameraManager.ChangeCamera(_BATTINGVIEW, BATTINGVIEW_CAMERA_INITX + 640, BATTINGVIEW_CAMERA_INITY + 360);
                //yield return new WaitForEndOfFrame();

                //Debug.Log("===================>>배터 로드 해야하는지 체크 batter.bLoadBatterFlag = " + batter.bLoadBatterFlag);
                if (batter.bLoadBatterFlag == true)
                {
                    yield return new WaitForEndOfFrame();
                    batter.LoadBatter();
                    //Debug.Log("===================>>Load Batter1");
                    yield return new WaitForSeconds(1.0f);
                    batter.bReadyAnim = false;
                    batter.waitAnim();
                }

                pitcher.readyAnimCount = 0;
                pitcher.setSpositionString();
                pitcher.setReadyAnim();

                if (batter.bNewBatter == true)
                {
                    ControlManager.EraseFieldUI(batter.bNewBatterInfo, pitcher.bNewPitcher);
                }
                else
                {
                    ControlManager.EraseFieldUI(false, false);
                }

                pitcher.bNewPitcher = false;
                batter.bNewBatterInfo = false;
                IngameUI.GetScoreBoard().BoardUpdate();
                FieldCrowdManager.SetActive(false);
                field.setZoom(1.0f);


                field.setFieldShift(0, true, true); //야수위치 초기화
                field.run.setRunnerInitPos();       //주자위치 초기화
                field.judge.InitPosition();         //심판위치 초기화

                ////Debug.Log("===================>>batter.bNewBatter = " + batter.bNewBatter);

                if (batter.bNewBatter == true)
                {
                    // 배팅뷰 스킬 대결
                    if (Mode.bPvpMode433 == false)  //if (Mode.bPvpMode == false)
                    {
                        //대전모드가 아닌경우 스킬을 여기서 참조
                        checkBattingviewSkill();
                    }

                    //인포 화면으로
                    ControlManager.SetInfoUI();
                    battingview.setReadyState();
                    playState = PlayState.PLAY_BATTING_VIEW_INFO;
                }
                else
                {
                    ControlManager.SetReadyUI(1, true); //gameUI.setReadyUI(1, false, true); //[UI]레디 UI                
                    battingview.setReadyState(true);
                    playState = PlayState.PLAY_BATTING_VIEW_READY;
                }
                ////Debug.Log("===================>>Load Batter2");


                //initField에서 setZoomStop을 안하면 시작하자마자 줌아웃되는 버그발생테스트용
                //field.setZoomTo(0.75f, 0.5f); //테스트코드 지워지워 
                //안전빵으로 여기서도 줌스탑초기화해줌
                field.setZoomStop();
            }
        }

        /*
        public bool bInfoShow = false;
        private void battingViewPreFrame()
        {
            battingview.setInitAngle();
            CameraManager.SetZoomFactor(1);
            field.bReturnCheck = false;
            field.bReturnBattingView = false;
            field.setFieldShift(0, true, true);


            if (bPitcherChangeFlag == true)
            {
                //리와인드 플레이시 해당사항 없음
                setPitcherChangeView();
            }
         
            else if (bBatterChangeFlag == true)
            {
                //리와인드 플레이시 해당사항 없음
                setBatterChangeView();
            }
            else if (bRunnerChangeFlag == true)
            {
                //리와인드 플레이시 해당사항 없음
                setRunnerChangeView();
            }
            else if (bFielderChangeFlag == true)
            {
                //리와인드 플레이시 해당사항 없음
                setFielderChangeView();
            }
            else
            {
                if (displayEffectType != 0)
                {
                    BackGroundManager.SetDisplayEffect(displayEffectType == 1 ? "DOME_02" : "BASS_ON_BALLS");
                    displayEffectType = 0;
                }

                //CameraManager.FieldAlpahSetting(initZoom, field.gameObject);
                CameraManager.ChangeCamera(_BATTINGVIEW, BATTINGVIEW_CAMERA_INITX + 640, BATTINGVIEW_CAMERA_INITY + 360);
                //CameraManager.SetBatterCamera(true);
                battingview.setCameraOffset();

                if (bInfoShow == true)
                {
                    //UIPlayerInfo.SetDraw(true);
                    battingview.zoneUI.gameObject.SetActive(true);

                    if (Mode.bAutoPlay == true)
                    {
                        //자동 플레이시    
                        pitcher.setPitcherReadyState();
                        battingview.zoneUI.setZone(true, true, true); //피칭커서 세팅O, 스트라이크존type2 강제세팅O

                        StartCoroutine(setBattingViewState(1.0f));
                    }
                    else
                    {
                        ControlManager.SetReadyUI(1, true); //gameUI.setReadyUI(1, false, true); //[UI]레디 UI
                        playState = PlayState.PLAY_BATTING_VIEW_READY;
                    }
                }
                else
                {
                    if (batter.bNewBatter == true)
                    {
                        playState = PlayState.PLAY_BATTING_VIEW_INFO;
                        ControlManager.SetInfoUI(); //gameUI.setInfoUI(); //[UI] 인포 UI                        
                        battingview.setReadyState();
                        //pitcher.pitcherAnim(2, "TEMP_WAIT", false);
                    }
                    else
                    {
                        //UIPlayerInfo.SetDraw(true);
                        ControlManager.SetReadyUI(1, true); //gameUI.setReadyUI(1, false, true); //[UI]레디 UI
                        playState = PlayState.PLAY_BATTING_VIEW_READY;
                        battingview.setReadyState(true);
                    }
                    bInfoShow = true;
                }
            }
            //bool bNewBatter = batter.bNewBatter;
            ControlManager.EraseFieldUI(batter.bNewBatterInfo, pitcher.bNewPitcher);
            pitcher.bNewPitcher = false;
            batter.bNewBatterInfo = false;
            UIScoreBoard.BoardUpdate();
            FieldCrowdManager.SetActive(false);
            field.setZoom(1.0f);

            batter.LoadBatter();// //StartCoroutine( batter.LoadBatter());

            field.setFieldShift(0, true, true); //야수위치 초기화
            field.run.setRunnerInitPos();       //주자위치 초기화
            field.judge.InitPosition();         //심판위치 초기화
            //batter.readyAnim(true);

        }*/
                
        /// <summary>
        /// 배팅뷰 레디 
        /// </summary>
        private void battingviewReadyFrame()
        {
            //////UnityEngine.//Debug.Log("====================>>battingviewReadyFrame");
            if (Mode.bPauseReady == true)
            {
                //UIIngameMenu.SetPause();
                Mode.bPauseReady = false;
                return;
            }

            if (bSkillEffectFlag == true)
            {
                //스킬 연출 가능시
                //battingview.setCameraOffset();
                StartCoroutine(battingviewSkillEffect());
                bSkillEffectFlag = false;
            }
            else
            {
                if (bReadyFinish)
                {
                    if (Mode.bSiumlSetting == true)
                    {                        
                        Mode.bSiumlSetting = false;
                        playState = PlayState.PLAY_FAST_INNING_SIMUL;
                        bCurrentChanceModeState = false;
                        StartCoroutine(backToSimulation(false));
                        return;
                    }

                    BattingViewInitZoom = 1;// 0.9f;
                    readyZoom = 2;// 0.02f;
                    if (readyZoom > BattingViewInitZoom)//1)
                    {
                        //battingview.setCameraOffset();
                        readyZoom = BattingViewInitZoom;
                        if (Mode.cameraView == CameraView.PitcherCenter) battingview.transform.localScale = new Vector3(0.95f, 0.95f, 1);
                        else battingview.transform.localScale = new Vector3(0.9f, 0.9f, 1);
                        batter.waitAnim();
                        field.run.setStealInvalid();
                        field.setTimeScale(Field.INIT_TIME_SCALE);
                        bReadyFinish = false;
                        battingview._2ndRunner.set2ndRunnerLead();

                        /*
                        if (Mode.bAutoPlay == true)
                        {
                            //자동 플레이시
                            StartCoroutine(setBattingViewState(0.01f)); //playState = PlayState.PLAY_BATTING_VIEW;
                            battingview.zoneUI.setZone(true, true, true); //피칭커서 세팅O, 스트라이크존type2 강제세팅O
                            //battingview.zoneUI.setTrace(false, false);
                            bReadyFinish2 = false;
                            bReadyFinish3 = false;
                        }
                        else*/
                        {
                            ControlManager.SetReadyUI2(); //gameUI.setReadyUI2();   //[UI]레디2 상태로 
                            bReadyFinish3 = false;
                        }

                    }
                    else
                    {

                    }
                }
                else if (bReadyFinish2)
                {
                    if (batter.bNewBatter == true)
                    {                        
                        //시간변화
                        BackGroundManager.SetTime();
                        if (Mode.bAutoPlay == false)
                        {
                            if (bMyTurn == false)
                            {
                                if (Mode.bPvpMode433 == false) //if (Mode.bPvpMode == false)
                                {
                                    ////Debug.Log("=========>> 인공지능 타격시 배팅 시뮬레이션 한번 돔");
                                    //도루 인공지능
                                    //field.run.getAIStealResult();

                                    //타격 시뮬레이션
                                    batter.simulateBattingOnly();

                                    if (batterSkillFlag == SkillFlag.GodOfBunt)
                                    {
                                        //번트의 신 설정
                                        battingResultData.hitType = SimulHitType.Bunt;
                                        battingResultData.buntResultType = SpecificBuntType.DRAG_SUCCESS;
                                        if (MyMath.Percent() < 35)
                                        {
                                            battingResultData.fIndex = CPlayer._THIRDBASEMAN;
                                        }
                                        else if (MyMath.Percent() < 70)
                                        {
                                            battingResultData.fIndex = CPlayer._FIRSTBASEMAN;
                                        }
                                        else
                                        {
                                            battingResultData.fIndex = CPlayer._PITCHER;
                                        }
                                    }

                                    if (battingResultData.hitType == SimulHitType.Bunt)
                                    {
                                        //번트 체크
                                        batter.buntTryCheck();
                                    }
                                }
                            }
                            else
                            {
                                //내 수동 조작인 경우 번트 결과를 미리 뺴둠
                                batter.recheckBuntResult();
                            }
                        }
                        batter.bNewBatter = false;
                    }

                    //////UnityEngine.//Debug.Log("====================>>BATTING READY FRAME 3");
                    StartCoroutine(setBattingViewState(0.01f)); //playState = PlayState.PLAY_BATTING_VIEW;
                    ControlManager.SetBattingUI();////gameUI.setBattingUI();   //[UI]배팅 UI 세팅
                    bReadyFinish2 = false;
                    //gameUI.setSkillReady(false); //[UI]스킬 레디 상태 디액티브
                    //for (int i = 0; i < 3; i++) CameraManager.SetScreenOverlay(i, false);

                }
            }
        }
                
        /// <summary>
        /// 배팅뷰 메인 프레임
        /// </summary>
        private void battingViewFrame()
        {
            //만약 배팅뷰로 넘어오기 전에 초기 세팅 할것 있으면 여기서
            pitcher.pitchingFrame();
            batter.batterFrame();
        }

 
        ///////////////////////////////////////////////////////////////////////////////
        //상태 설정
        ///////////////////////////////////////////////////////////////////////////////
        //아무것도 없는 배팅뷰 배경설정
        private void setChangeBack()
        {            
            CameraManager.ChangeCamera(_BATTINGVIEW, BATTINGVIEW_CAMERA_INITX + 640, BATTINGVIEW_CAMERA_INITY + 360);
            CameraManager.SetBatterCamera(false);
            CameraManager.SetCameraLayer("BATTINGFIELD_LAYER");            
            IngameUI.GetScoreBoard().SetActive(false);
        }

        //아무것도 없는 필딩뷰 배경설정
        public void setFieldBack()
        {
            field.setZoom(0.7f);
            CameraManager.ChangeCamera(_FIELDVIEW, FIELDVIEW_CAMERA_INITX + FieldSize.getMoundPosX(), FIELDVIEW_CAMERA_INITY + 843);
            CameraManager.SetActiveCameraInitAngle(-15);                        
            FieldCrowdManager.SetActive(true);
            IngameUI.GetFieldUI().SetActive(false);
        }

        /*
        //강제로 오토모드 진입
        public void setForcedAutoState(Transform trans)
        {
            //////UnityEngine.//Debug.Log("=====================================>>강제로 오토모드 진입");
            if (playState == PlayState.PLAY_BATTING_VIEW_INFO)
            {                
                batter.simulateBattingOnly();
                batter.updateCountBySimulation();
                ControlManager.InfoGone();
                Mode.bAutoPlaySetting = Mode.bAutoPlay = true;      
            }
            else if (playState == PlayState.PLAY_BATTING_VIEW_READY)
            {
                batter.simulateBattingOnly();
                batter.updateCountBySimulation();
                Mode.bAutoPlaySetting = Mode.bAutoPlay = true;

                if (bMyTurn == true)
                {
                    //ControlBattingReady.SetActive(false, this);
                }
                else
                {
                    ControlPitchingSelect.SetActive(false);
                }
                StartCoroutine(setBattingViewState(0.01f)); //playState = PlayState.PLAY_BATTING_VIEW;
            }
            else
            {
                Mode.bAutoPlaySetting = !Mode.bAutoPlaySetting;
                //Util.LoadToast(("자동플레이 모드는 다은 투구부터 적용됩니다"), trans, new Vector3(0, 110, -8));
            }
        }

        //강제로 수동모드 진입
        public void setForcedControlState(Transform trans)
        {
            //////UnityEngine.//Debug.Log("=====================================>>강제로 수동모드 진입");
            if (playState == PlayState.PLAY_BATTING_VIEW_INFO)
            {
                Mode.bAutoPlaySetting = Mode.bAutoPlay = false;      
            }
            else
            {
                Mode.bAutoPlaySetting = !Mode.bAutoPlaySetting;
                //Util.LoadToast(("수동플레이 모드는 다은 투구부터 적용됩니다"), trans, new Vector3(0, 110, -8));
            }
        }*/

        /*
        public void setChangePlayer()
        {
            //bInfoShow = false;
            pitcher.bNewPitcher = true;
            batter.bNewBatter = true;
            batter.bNewBatterInfo = true;
            ControlManager.ResetUI();            
            pitcher.setPitch();
            //playState = PlayState.PLAY_BATTING_VIEW_PRE;
            StartCoroutine(battingviewPreDelay());
        }*/


        public IEnumerator setBattingViewState(float delay)
        {
            playState = PlayState.NONE;// PLAY_BATTING_VIEW;
            yield return new WaitForSeconds(delay);
            field.initPitcher(true);        //이리로 옯겨옴
            field.run.initRunner2();        //이리로 옮겨옴
            playState = PlayState.PLAY_BATTING_VIEW;
        }


        private IEnumerator setDelayState(float delay)
        {
            PlayState lastState = playState;
            playState = PlayState.NONE;
            yield return new WaitForSeconds(delay);
            playState = lastState;
        }

        /*
        //투수교체 UI설정
        private void setPitcherChangeView()
        {
            setChangeBack();
            playState = PlayState.PLAY_AI_PITCHER_CHANGE;
            ControlManager.ResetUI();
            inningTime = 0;

            CPlayer inPlayer = SimulPlayerManager.GetPitcher(defenseIndex, pitcher.inPitcher);
            CPlayer outPlayer = SimulPlayerManager.GetPitcher(defenseIndex, pitcher.outPitcher);

            //이부분에 교체 연출 프리팹
        }
        */

        /*
        //타자교체 UI설정
        private void setBatterChangeView()
        {
            setChangeBack();
            ////UnityEngine.//Debug.Log("================>>타자교체 여부?");
            playState = PlayState.PLAY_AI_BATTER_CHANGE;
            ControlManager.ResetUI();
            inningTime = 0;

            CPlayer inPlayer = batter.pBatter;
            int outIndex = SimulManager.GetChangerIndex(ChangerIndex.OutBatter);
            CPlayer outPlayer = SimulPlayerManager.GetFielder(offenseIndex, outIndex);

            //이부분에 교체 연출 프리팹
        }*/

        /*
        //주자교체 UI설정
        private void setRunnerChangeView()
        {
            setChangeBack();
            ////UnityEngine.//Debug.Log("================>>주자교체 여부?");
            playState = PlayState.PLAY_AI_RUNNER_CHANGE;
            ControlManager.ResetUI();
            inningTime = 0;

            CPlayer inRunner = null;
#if _Test_Local
            {
                int fIndex = SimulManager.GetChangerIndex(ChangerIndex.InRunner);// SimulManager.GetInstance().firstBase;
                if (fIndex != -1)
                {
                    SimulRunner _runner = SimulManager.GetRunner(fIndex);
                    if (_runner != null)
                    {
                        inRunner = _runner.getRunner();// GetInstance().runner[fIndex].getRunner();
                        field.run.runner[fIndex].setRunnerChange(inRunner);
                    }
                }
            }
#else
            {
                int inIndex = SimulManager.GetChangerIndex(ChangerIndex.InRunner);
                inRunner = SimulPlayerManager.GetFielder(offenseIndex, inIndex);
            }
#endif


            if (inRunner != null)
            {
                //////UnityEngine.//Debug.Log("================>>교체 들어온 주자 : "+inRunner.getName());
                //////UnityEngine.//Debug.Log("================>>교체 나간 타자 : " + SimulPlayerManager.GetFielder(offenseIndex, SimulManager.GetInstance().outRunner).getName());
                int outIndex = SimulManager.GetChangerIndex(ChangerIndex.OutRunner);
                CPlayer outPlayer = SimulPlayerManager.GetFielder(offenseIndex, outIndex);


                //이부분에 교체 연출 프리팹


            }

        }*/

        /*
        //야수교체 UI설정
        private void setFielderChangeView()
        {
            setChangeBack();
            ////UnityEngine.//Debug.Log("================>>대수비교체 여부?");
            playState = PlayState.PLAY_AI_FIELDER_CHANGE;
            ControlManager.ResetUI(); //gameUI.resetUI(); //[UI] 리셋UI

            int inIndex = SimulManager.GetChangerIndex(ChangerIndex.InFielder);
            CPlayer inPlayer = SimulPlayerManager.GetFielder(defenseIndex, inIndex);

            int outIndex = SimulManager.GetChangerIndex(ChangerIndex.OutFielder);
            CPlayer outPlayer = SimulPlayerManager.GetFielder(defenseIndex, outIndex);

            //bool bPositionChange = false;
            //if (inIndex < 9 && outIndex < 9) bPositionChange = true;

            inningTime = 0;
            


            //이부분에 교체 연출 프리팹

        }*/



        ///////////////////////////////////////////////////////////////////////////////
        //찬스모드
        ///////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 시뮬모드 배경 설정
        /// </summary>        
        private void simulationBackSetting()
        {
            field.bHomerunCeremony = false;
            field.setFireWorkDeActive();
            
            //배팅뷰 끔
            battingview.gameObject.SetActive(false);
            //필드에서 필요없는 부분 끔
            IngameUI.GetScoreBoard().SetActive(false);
            setFieldBack();
            FieldCrowdManager.SetCrowdActiveAll(false);
            for (int i = 0; i < 9; i++) field.fielder[i].gameObject.SetActive(false);
            field.run.gameObject.SetActive(false);
            field.judge.gameObject.SetActive(false);
            //시뮬레이터 액티브
            simulator.gameObject.SetActive(true);
            playState = PlayState.PLAY_FAST_INNING_SIMUL;
        }

        /// <summary>
        /// 액션모드로 설정
        /// </summary>
        private void backToActionSetting()
        {
            //배팅뷰 킴
            battingview.gameObject.SetActive(true);
            //필드에서 필요없는 부분 끔
            FieldCrowdManager.SetCrowdActiveAll(false);
            for (int i = 0; i < 9; i++) field.fielder[i].gameObject.SetActive(true);
            field.run.gameObject.SetActive(true);
            field.judge.gameObject.SetActive(true);
            //시뮬레이터 액티브
            simulator.gameObject.SetActive(false);
        }

        private bool simualtorChangeInning;
        /// <summary>
        /// 시뮬 모드로 설정
        /// </summary>
        private IEnumerator backToSimulation(bool inningChange, SimulResultState battingResult = SimulResultState.NONE)
        {
            //Debug_UI.SetNotice(false);

            if (Mode.bPvpMode == true)
            {
                IngameUI.GetEmoticonChatting().chattingDisable();   //채팅창 닫음
                //시작의 동기화를 위해
                //서로 이걸 받아야 동시에 시작함
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.Init);
                PvpManager.chanceState = PvpManager.ChanceState.None;
            }
            else if (Mode.bPvpMode433 == true)
            {
                IngameUI.GetEmoticonChatting().chattingDisable();   //채팅창 닫음
                
            }


            ControlBattingUI.SetActive(false, this);
            IngameUI.GetPlayerInfo().SetActive(false);            
            IngameUI.GetPitchingSelect().SetActive(false);
            IngameUI.GetControlRunner().SetActive(false, false);
            IngameUI.GetScoreShow().DeActive();

            SimulManager.SyncRunner(field.run);
            SimulManager.SyncData(this, false, false, bThreeOutChange); //게임엔진 -> 시뮬레이션으로 동기화
            yield return new WaitForSeconds(1.0f);
            simulationBackSetting();
            ////Debug.Log("=========================>>다음타자 세팅");
            if (battingResult != SimulResultState.NONE)
            {
                simulator.NextLineupFromGame(battingResult);
            }
            simulator.continueSimul(inningChange, simualtorChangeInning);
            
            Mode.bSimulationQuickPlay = true;
            Mode.PlayTypeFlag = Mode.ModeFlag.Auto;

            IngameUI.GetScoreBoard().setWait(false);
        }

        /// <summary>
        /// 찬스모드에서 캐릭터 포지션 초기화
        /// </summary>
        private void fieldCharacterInit()
        {
            //야수 초기화
            for (int i = 0; i < 9; i++)
            {
                field.fielder[i].transform.localPosition = Vector3.zero;
                field.fielder[i].bFielderActive = false;
            }

            //주자 초기화
            for (int i = 0; i < 4; i++)
            {
                if (field.run.runnerActive[i] == true)
                {
                    if (field.run.runner[i] != null)
                    {
                        field.run.runner[i].transform.localPosition = Vector3.zero;
                        field.run.runner[i].bRunnerActive = false;
                    }
                }
            }

            //심판 초기화
            field.judge.InitPosition();
        }

        /// <summary>
        /// 찬스모드 세팅
        /// </summary>
        public void setChanceMode()
        {
            Application.targetFrameRate = 60;
            Debug_UI.SetNetwork(false);
            setFieldBack();
            batter.bBatterFieldUpdate = true;
            IngameUI.GetInstance().gameObject.SetActive(true);
            if (Mode.bPvpMode == true)
            {
                IngameUI.GetEmoticonChatting().gameObject.SetActive(true); //인게임 채팅창 열음
                IngameUI.GetEmoticonChatting().toggleActive(true);         //토글 버튼 활성화 
                //Debug.Log("===================>> PvpManager.bWaitStateQuit false로 세팅");
                PvpManager.bWaitStateQuit = false;
                PvpManager.bGameReady = false;
                if (PvpManager.connectState != PvpManager.ConnectState.Connect)
                {
                    //Debug.Log("===================>> 찬스모드로 가는 순간 끊긴경우");
                    Mode.bPvpMode = false;
                }
            }

            //연속경기 팝업 체크
            IngameUI.GetScoreBoard().checkConsetiveGame();


            //필드캐릭터 초기화
            fieldCharacterInit();

            field.bHomerunCeremony = false; 
            Mode.bSimulationQuickPlay = false;
            Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            Mode.bSiumlSetting = false;            
            
            
            battingview.lastView = CameraView.None;            

            //화면 세팅
            backToActionSetting();

            Debug_UI.SetLoading(true);

            //데이터 싱크
            int lastOffenseIndex = offenseIndex;
            SimulManager.SyncData(this, true, false, false); //시뮬레이션 -> 게임엔진 동기화
            if (bMyHome == true)
            {
                bMyTurn = bTopInning ? false : true;
            }
            else
            {
                bMyTurn = bTopInning ? true : false;
            }


            if (Mode.bPitchingViewActive == true)
            {
                if (bMyTurn == true)
                {
                    Mode.cameraView = CameraView.BatterLow;
                }
                else
                {
                    Mode.cameraView = CameraView.PitcherCenter;
                }
            }
            else
            {
                Mode.cameraView = CameraView.BatterLow;
            }
            
            bFielderLoadComp = false;

            bBatterForceLoad = false;   //타자 강제 로딩 안함
            //파라메터 초기화
            initInningParameter(true);

            /*
            for (int i = 0; i < 4; i++)
            {
                if (field.run.runner[i] != null)
                {
                    field.run.runner[i].transform.localPosition = Vector3.zero;
                }
            }*/

            StartCoroutine(waitChanceModeToNextStep());
            
        }

        private IEnumerator waitChanceModeToNextStep()
        {
            while(bFielderLoadComp == false)
            {
                //대기탐
                yield return new WaitForSeconds(0.2f);
            }

            BackGroundManager.UpdateTime(nInningCount, true);

            //주자 싱크
            syncRunner();
            for (int i = 0; i < 4; i++)
            {
                if (field.run.runnerActive[i] == true)
                {
                    IngameUI.GetFieldUI().MakeMinimapRunner(this, field.run.runner[i], bTopInning ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex);
                }
            }
            IngameUI.GetControlRunner().Init(this);

            ///여기부터
            //뷰 초기화
            battingview.resetView();

            bCurrentChanceModeState = true;

            lastChanceScore = nGameScore[bMyTurn ? 0 : 1];
            chanceCount = 0;

            //카메라 세팅
            setBasicBattingViewCamera(false);
            batter.gameObject.SetActive(true);
            pitcher.gameObject.SetActive(true);

            simualtorChangeInning = true;
            ///여기까지

            //투수 세팅
            batter.bReadyAnim = true;
            pitcher.setPitch();

            
            //게임시작
            returnBattingView();

            //
            yield return new WaitForSeconds(0.2f);
            

            Debug_UI.SetLoading(false);
        }



        int lastChanceScore, chanceCount;
        /// <summary>
        /// 찬스모드 끝났는지 여부 체크
        /// </summary>
        /// <returns></returns>
        public bool checkChanceModeEnd(SimulResultState battingResult)
        {
            //Debug.Log("=========================================>>checkChanceModeEnd");
            if (Mode.bOnlyChanceMode == true)
            {
                if (bCurrentChanceModeState == true)
                {
                    //한타석만 체크
                    bCurrentChanceModeState = false;
                    StartCoroutine(backToSimulation(bThreeOutChange, battingResult));
                    return true;

                    /*이전버전
                    if (bMyTurn == false
                     || chanceCount > 0
                     || bThreeOutChange == true
                     || bStrikeOut == true
                     || (nGameScore[0] > lastChanceScore))
                    {
                        bCurrentChanceModeState = false;
                        StartCoroutine(backToSimulation(bThreeOutChange, true));
                        return true;
                    }

                    if (simulator != null)
                    {
                        //Debug.Log("=========================>>다음타자 세팅");
                        simulator.NextLineupFromGame(SimulResultState.Single);
                    }*/ //여기까지

                }
                chanceCount++;
            }
            return false;
        }
        

        //패스트 이닝 프레임중
        /// <summary>
        /// 두두두두 가는 게임 -> 나중에 살리면 됨
        /// </summary>
        private void fastInnintSimulFrame()
        {
            if (simulator.curState == QuickSimulator.SimulState.gameover)
            {
                SimulManager.SyncData(this, true, false, false);
                setInningScoreClose(bTopInning);
                setResult();
            }

            /*    FastInningSimulator.FastSimulState state = FastInningSimulator.CheckSimulState();

                if (state == FastInningSimulator.FastSimulState.gameover)
                {
                    SimulManager.SyncData(this, true, true, false); //시뮬레이션 -> 게임엔진 동기화
                    FastInningSimulator.Destroy();
                    setResult();
                }
                else if (state == FastInningSimulator.FastSimulState.simulover)
                {
                    bPitcherChangeException = false;
                    //nInningCount = Mode.simulatedInning + 1;
                    //데이터 싱크
                    SimulManager.SyncData(this, true, true, false); //시뮬레이션 -> 게임엔진 동기화

                    pitcher.pPitcher = SimulPlayerManager.GetPitcher(defenseIndex);
                    field.fielder[CPlayer._PITCHER].initParameter(pitcher.pPitcher, CPlayer._PITCHER);
                    batter.pBatter = SimulPlayerManager.GetBatter(offenseIndex);
                    batter.initBatter(batter.pBatter, offenseIndex);

                    for (int i = 0; i < 9; i++) field.fielder[i].setStop();
                    field.setTimeScale(Field.INIT_TIME_SCALE);
                    if (changeBattingView() == true)
                    {
                        returnBattingView();
                    }
                    FastInningSimulator.Destroy();
                }*/
        }

        

        ///////////////////////////////////////////////////////////////////////////////
        //화면 전환 관련 함수
        ///////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 선수교체 이벤트 화면 세팅
        /// </summary>
        public void setChangeEvent()
        {
            IngameUI.GetScoreBoard().SetActive(false);
            IngameUI.GetPlayerInfo().SetActive(false);
            ControlManager.ResetUI();
            setFieldBack();
            for (int i = 0; i < 9; i++) field.fielder[i].transform.localPosition = Vector3.zero;
            playState = PlayState.PLAY_CHANGE_PLAYER;
        }
        
        /// <summary>
        /// 선수교체 이벤트로부터 화면전환
        /// </summary>
        public void returnFromChangeEvent(UIPlayerChange.PlayerChangeType changeType)
        {
            battingview.lastView = CameraView.None;
            Mode.bPauseGame = false;
            /*if (changeType == UIPlayerChange.PlayerChangeType.PitcherChange || changeType == UIPlayerChange.PlayerChangeType.BatterChange)
            {
                //인포가 새로 갱신되는 경우
                if (bMyTurn == true) ControlBattingUI.CheckPauseState(false);
                pitcher.setPitch();
            }
            else
            {
                //인포가 새로 갱신되지 않는 경우는 투수 리슘상태로
                pitcher.setResume();
            }*/

            if (bMyTurn == true) ControlBattingUI.CheckPauseState(false);
            pitcher.setPitch();
            pitcher.bRelease = false; //이거때문에 버그났음

            bBattingPreUpdate = false;            
            returnBattingView();

            CameraView curView = CameraView.BatterLow;
            if (Mode.bPitchingViewActive == true)
            {
                curView = (bMyTurn == true ? CameraView.BatterLow : CameraView.PitcherCenter);
            }
            //타자 교체시 이전 타자 삭제
            battingview.settingView(curView, (changeType == UIPlayerChange.PlayerChangeType.BatterChange ? false : true));

            if (Mode.bPvpMode == true)
            {
                PvpManager.GetInstance().setChangeState();
            }
        }
       

        //필드뷰로 화면 전환을 한다
        public void changeFieldView(FieldState view)
        {
            //UnityEngine.//Debug.Log("========================================================>>changeFieldView");

            IngameUI.GetPitchUI().SetTrace(false);
            pitcher.setPitchSystemDraw(false);

            view = FieldState.NORMAL_FIELD;//ACTIVE_CENTER; // 

            //탑뷰
            float xPos = field.ball.transform.position.x;// //BallPlayManager.FIELDVIEW_CAMERA_INITX + FieldSize.getMoundPosX();
            float yPos = field.ball.transform.position.y;//BallPlayManager.FIELDVIEW_CAMERA_INITY + FieldSize.getMoundPosY(0.6f);
            CameraManager.ChangeCamera(_FIELDVIEW, xPos, yPos);

            battingview.gameObject.SetActive(false);

            field.bFieldViewActive = true;
            playState = PlayState.PLAY_FIELDING_VIEW;
            field.initField();
            

        }


        //필드로 넘어갈 때 조건에 따라 노멀 혹은 액티브 필드뷰를 리턴한다
        public FieldState getView()
        {
            return FieldState.NORMAL_FIELD;
        }

        //배팅뷰로 화면 전환을 한다 - FALSE를 리턴하는 경우 이닝 체인지(쓰리아웃)
        public bool changeBattingView()
        {
            IngameUI.GetPitchUI().SetTrace(true);
            field.ball.bFoulCall =
               field.ball.bHomeRunCall = false;

            if (bGoodByeHitCall == true)
            {
                setChangeInning(true);
                return false;
            }

            if (bThreeOutChange)
            {
                //안전빵
                field.initPitcher(false);
                setChangeInning(true);
                bThreeOutChange = false;
                ControlManager.ResetUI(); //gameUI.resetUI(); //[UI]리셋 UI
                return false;
            }

            //
            pitcher.setPitch();
           
            return true;
        }

        //배팅뷰로 돌아온다.
        public void returnBattingView()
        {            
            field.returnCheckInit();
            field.bFieldViewActive = false;
            battingview.gameObject.SetActive(true);
            battingview.setCenterView();
            //CameraManager.SetBloom(false);//setBloom(false);
            //CameraManager.SetBlur2(false); //setBlur(false);// setDepthOfField(false);
            //CameraManager.SetInvert(false);// setInvert(false);

            pitcher.initAnim();
            pitcher.setPitchSystemDraw(true);
            if (pitcher.bPvState == true)
            {
                pitchPv.battingSystemPv.setSystemDraw(false);
            }
            else
            {
                //처리됨
                pitch.battingSystem.setSystemDraw(false);/// pitcher.bball.setBallDraw(false, false);
            }

            battingview.setInitPosition();
            pitcher.initPosition();
            batter.initPosition();

            fieldSkillDisplayManager.InitSkill();
            battingview.setBVRunnerState(true);
            //playState = PlayState.PLAY_BATTING_VIEW_PRE;
            StartCoroutine(battingviewPreDelay());
            System.GC.Collect(); 
        }

        /// <summary>
        /// 9회2사 모드에서 다음라운드
        /// </summary>
        public IEnumerator nineTwoNextRound()
        {
            int getScore = nGameScore[0];
            if ( getScore > 0)
            {
                nineTwoRoundScore[nineTwoRound - 1] = getScore;
                yield return new WaitForSeconds(1.0f);
                IngameUI.GetScoreShow().DeActive();
            }

            field.setFireWorkDeActive();


            nineTwoRound++;
            if (nineTwoRound > 10 || getScore <= 0)
            {
                //결과화면
                nineTwoScore += nGameScore[0];
                nineTwoFinalScore = nineTwoScore;
                if (nineTwoRound > 10) nineTwoFinalRound = 10;
                else nineTwoFinalRound = nineTwoRound - 2;
                for (int i = 0; i < 9; i++) field.fielder[i].gameObject.SetActive(false);
                setResult();
            }
            else
            {
                BackGroundManager.UpdateTime(nineTwoRound, true);

                //field.run.checkDestroyRunner();
                foreach (Transform child in field.run.transform)
                {
                    Destroy(child.gameObject);
                }

                bThreeOutChange = false;
                nineTwoScore += getScore;
                bNineTwoNextRound = false;
                initRule();
                bMyTurn = true;
                bTopInning = false;

                
                //플래그 초기화(순서 주의)
                bChangeFlag = true;
                offenseIndex = 0;
                defenseIndex =1;
                bBattingPreUpdate = false;

                resetCount();
                //카운트 업데이트
                IngameUI.GetFieldUI().SetCountUpdate(nOutCount);
                
                field.run.initRunner();
                //2사만루 초기화
                twoOutBaseLoadModeInit();
                
                //이닝 초기화와 동시에 타자 초기화
                batter.initBatter(walkOffBatter, 0); //타자 초기화 및 타자 텍스쳐 로딩
                field.run.makeHitterRunner(walkOffBatter);      //타자주자 초기화
                batter.bLoadBatterFlag = false;
                batter.bGangTa = false;
                batter.readyAnim(true);
                batter.initPosition();

                
                //필드 플래그 초기화
                field.ball.bFoulCall = field.ball.bHomeRunCall = false;
                field.fieldShift = 0;
                field.setFieldShift(field.fieldShift, false, false);

                //기타 이닝 플래그 초기화
                nBatterCount = 0;
                pitcher.bSetPosition = false;
                bThreeOutChange = false;
                outCountIF = 0;


                for (int i = 0; i < 9; i++) field.fielder[i].setStop();
                field.setTimeScale(Field.INIT_TIME_SCALE);
                setFieldBack();
                yield return new WaitForEndOfFrame();

#if _Test_Local
                int count = 0; //서버에서
#else
                // DISABLED_MGRS: WalkoffPlayGameInfo info = Mgrs.userData.walkoffInfo;
                int count = info.outCounts[nineTwoRound-1]; //서버에서
#endif
                bNextRoundInit = true;
                IngameUI.GetInningChangeUI().WalkOffActive(this, getScore, count);
                yield return new WaitForSeconds(2.5f);
                nineTwoNextRoundSetting(count);
            }
        }

        private bool bNextRoundInit = false;

        public void nineTwoNextRoundSetting(int count)
        {
            if (bNextRoundInit == true)
            {
                bNextRoundInit = false;
                if (changeBattingView() == true)
                {
                    returnBattingView();
                }
                newStrikeCount = nStrikeCount = count;
                IngameUI.GetScoreBoard().BoardUpdate();
            }
        }


        public void foulCall()
        {
            //주자가 달리고 있으면 스톱시킴
            field.setRunnerStop(true);
            field.run.setStealInvalid();  //도루는 무효화          

            bStrikeCheck = true;
            bStrike = true;
            if (nStrikeCount < 2)
            {
                nStrikeCount++;
            }
            else
            {
                if (batter.bBunt == true || batter.bBuntHit == true)
                {
                    //스트라이크 아웃 사운드

                    bStrike = false;
                    bStrikeOut = true;
                    bStealStrikeOut = true;
                    IngameUI.GetBattingCall().Call(CALLTYPE.CALL_STRIKEOUT, (int)pitcher.selectedBallIndex, pitcher.getBallSpeed(), batter);
                    fieldOutCountNum = 0;
                    setOutFlag(BallPlayManager._K_FLAG | BallPlayManager._SO_FLAG);
                    addOutCount();                    
                    pitcher.initStateStep();
                    bChangeFlag = true;
                    pitcher.pState = PitcherState._GET_STRIKEOUT;
                    batter.bState = BatterState._NEXT_BATTER;
                    StartCoroutine(batter.nextBatterAfterStrikeOut(1.5f));
                    return;
                }
                else
                {
                    nStrikeCount = 2;
                }

            }
            IngameUI.GetBattingCall().Call(CALLTYPE.CALL_FOUL, (int)pitcher.selectedBallIndex, pitcher.getBallSpeed(), batter);
            IngameUI.GetScoreBoard().BoardUpdate();
        }


        private void setBasicBattingViewCamera(bool bCameraInit = true)
        {
            battingview.setInitPosition();
            pitcher.initPosition();
            //batter.readyAnim(true);
            battingview.zoneUI.setZone(false, false, false);    //존 디액티브

            if (bCameraInit == true)
            {
                //battingview.zoneUI.setTrace(false, false);
                CameraManager.ChangeCamera(_BATTINGVIEW, BATTINGVIEW_CAMERA_INITX + 640, BATTINGVIEW_CAMERA_INITY + 360);
            }
        }





        ///////////////////////////////////////////////////////////////////////////////
        //스킬매니저와 스킬 연출
        ///////////////////////////////////////////////////////////////////////////////        
        /// <summary>
        /// 스킬 플래그
        /// 매니저에서 스킬이 걸린지 여부를 알아내기 위한 플래그
        /// </summary>
        public SkillFlag batterSkillFlag, pitcherSkillFlag;
        
        //최신 스킬 연출        
        private bool bSkillEffectFlag; //배팅뷰 스킬 연출중인지 여부
        public bool vsType;            //1일반 2대결 (나중에 enum으로)

        
        //연출 -> 배팅뷰 즉석에서 연출 호출 나중에 바낄수도
        public void instantSkillEffect(CPlayer player, SkillIndex index, bool bDefenseSkill)
        {
            CSkill skill = player.getSkillValue(index);
            int ID = skill.ID;
            int rank = skill.rank;  

            if ((bDefenseSkill == false && bMyTurn == true) || (bDefenseSkill == true && bMyTurn == false))
            {
                IngameUI.GetMySkillUI().init(ID, rank);
            }
            else
            {
                IngameUI.GetCpuSkillUI().init(ID, rank);                
            }
        }






       



        /// <summary>
        /// 배팅뷰 스킬 연출
        /// </summary>
        /// <returns></returns>        
        public IEnumerator battingviewSkillEffect(bool bPitcherUI = false)
        {
            PlayState lastState = playState;
            playState = PlayState.NONE;

            if (vsType == true)
            {
                //대결연출
                //Debug.Log("================>>대결 연출");
                CSkill batterSkill = SimulManager.GetBatterSkill();
                CSkill pitcherSkill = SimulManager.GetPitcherSkill();
                bool bBatterWin = SimulManager.CheckVsBatterWin();
                //VS연출
                IngameUI.GetVsSkillUI().init(0, bMyTurn, (int)batterSkill.ID, batterSkill.rank, (int)pitcherSkill.ID, pitcherSkill.rank, bBatterWin);

                yield return new WaitForSeconds(2.0f);

                if (bBatterWin == true)
                {
                    IngameUI.GetPlayerInfo().SetBuffUI((SkillID)batterSkill.ID, (bMyTurn ? true : false));
                    SkillEffectDisplayManager.AddSkill(batterSkill);
                    float delay = SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Start, false, true);
                    yield return new WaitForSeconds(delay + 1.0f);
                    pitcherSkill = null;
                }
                else
                {
                    IngameUI.GetPlayerInfo().SetBuffUI((SkillID)pitcherSkill.ID, (bMyTurn ? false : true));
                    SkillEffectDisplayManager.AddSkill(pitcherSkill);
                    float delay = SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Start, true, true);
                    yield return new WaitForSeconds(delay + 1.0f);
                    batterSkill = null;
                }
            }
            else
            {
                CSkill batterSkill = SimulManager.GetBatterSkill();
                CSkill pitcherSkill = SimulManager.GetPitcherSkill();
                if (bMyTurn == true)
                {
                    if (batterSkill != null)
                    {
                        IngameUI.GetPlayerInfo().SetBuffUI((SkillID)batterSkill.ID, (bMyTurn ? true : false));
                        SkillEffectDisplayManager.AddSkill(batterSkill);
                        float delay = SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Start, false);
                        yield return new WaitForSeconds(delay + 1.0f);
                    }
                    if (pitcherSkill != null)
                    {
                        IngameUI.GetPlayerInfo().SetBuffUI((SkillID)pitcherSkill.ID, (bMyTurn ? false : true));
                        SkillEffectDisplayManager.AddSkill(pitcherSkill);
                        float delay = SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Start, true);
                        yield return new WaitForSeconds(delay + 1.0f);
                    }
                }
                else
                {
                    if (pitcherSkill != null)
                    {
                        IngameUI.GetPlayerInfo().SetBuffUI((SkillID)pitcherSkill.ID, (bMyTurn ? false : true));
                        SkillEffectDisplayManager.AddSkill(pitcherSkill);
                        float delay = SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Start, true);
                        yield return new WaitForSeconds(delay + 1.0f);
                        
                    }
                    if (batterSkill != null)
                    {
                        IngameUI.GetPlayerInfo().SetBuffUI((SkillID)batterSkill.ID, (bMyTurn ? true : false));
                        SkillEffectDisplayManager.AddSkill(batterSkill);
                        float delay = SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Start, false);
                        yield return new WaitForSeconds(delay + 1.0f);
                    }
                    
                }
            }

            //불꽃투혼 예외처리
            if (pitcher.pPitcher.checkSkillInvoke(SkillIndex.FrameFight) == true)
            {
                //발동시 핀치 극복
                pitcher.overComePinch();
            }
            
            playState = lastState;


            if (Mode.bPvpMode == true)
            {
                if (bPitcherUI == true)
                {
                    settingPitchingUIbyForce();
                }
            }
        }

        /// <summary>
        /// 투수 UI 강제세팅
        /// </summary>
        public void settingPitchingUIbyForce()
        {
            IngameUI.GetPitchingSelect().SetActive(true);
            if (Mode.cameraView == CameraView.PitcherCenter)
            {
                pitcher.catcher.setSign();
            }
        }



        /// <summary>
        /// 시뮬레이션 엔진단에서 검색되고 발동된 타자 스킬의 플래그를 세운다
        /// </summary>
        private void setSkillFlag()
        {
            batterSkillFlag = SkillFlag.None;
#if _Skill_Display
            //연출테스트용
            if (batterSkill_Display_test == bSkillDisplay.Gang_Seub_Ta_Gu)
            {
                batterSkillFlag = SkillFlag.AssaultBall;
                for (int i = 2; i < 6; i++)
                {
                    field.fielder[i].bCatchErrorFlag = true;
                }
            }
            else if (batterSkill_Display_test == bSkillDisplay.Bunt_Sin)
            {
                batterSkillFlag = SkillFlag.GodOfBunt;
            }
            else if (batterSkill_Display_test == bSkillDisplay.Tteun_Geum_Po)
            {
                batterSkillFlag = SkillFlag.Unexpected;
            }
#else         
            if (batter.pBatter.checkSkillInvoke(SkillIndex.AssaultBall) == true)
            {
                //강습타구
                SimulManager.SetPitchBatterSkill(null); //타자 피치 스킬 무효처리
                //Debug.Log("===================>>강습타구 걸려있음");
                batterSkillFlag = SkillFlag.AssaultBall;
                for (int i = 2; i < 6; i++)
                {
                    field.fielder[i].bCatchErrorFlag = true;
                }
            }
            else if (batter.pBatter.checkSkillInvoke(SkillIndex.GodOfBunt) == true)
            {
                //번트의신
                SimulManager.SetPitchBatterSkill(null); //타자 피치 스킬 무효처리
                //Debug.Log("===================>>번트신 걸려있음");
                batterSkillFlag = SkillFlag.GodOfBunt;
            }
            else if (batter.pBatter.checkSkillInvoke(SkillIndex.Unexpected) == true)
            {
                //뜬금포
                SimulManager.SetPitchBatterSkill(null); //타자 피치 스킬 무효처리
                //Debug.Log("===================>>뜬금포 걸려있음");
                batterSkillFlag = SkillFlag.Unexpected;
            }


#endif
        }

        /// <summary>
        /// 일부 타자스킬에 의해 일부 투수 스킬이 무효화 된경우 호출하거나
        /// 투수의 일부 스킬을 초기화 하기 위해 호출됨
        /// </summary>
        /// <param name="bInitPitch">true인 경우 초기화 하기 위해 사용</param>
        public void pitchSkillBatterWin(bool bInitPitch)
        {
            if (pitcherSkillFlag == SkillFlag.TenderStroke || pitcherSkillFlag == SkillFlag.Charm)
            {
                if (bInitPitch == false)
                {
                    //여기에 연출 때림
                    Debug.Log("회심의일격이나 매혹이 타자 스킬에 의해 무효화!!");
                }
                pitcherSkillFlag = SkillFlag.None;
                
            }
        }

        /// <summary>
        /// 일부 투수스킬에 의해 일부 타자 스킬이 무효화 된경우 호출
        /// </summary>
        public void pitchSkillPitcherWin()
        {
            //강습타구 뜬금포는 투수의 피치 스킬에 짐
            if (batterSkillFlag == SkillFlag.AssaultBall || batterSkillFlag == SkillFlag.Unexpected)
            {
                if (batterSkillFlag == SkillFlag.AssaultBall)
                {
                    for (int i = 2; i < 6; i++) field.fielder[i].bCatchErrorFlag = false;
                }
                else if (batterSkillFlag == SkillFlag.Unexpected)  //뜬금포 카운트 리셋
                {
                    CSkill curBatterSkill = SimulManager.GetBatterSkill();
                    if (curBatterSkill != null)
                    {
                        //뜬금포 계열 카운트 리셋
                        SimulManager.ResetSkillCount(offenseIndex, (SkillID)curBatterSkill.ID);
                    }
                }

                if (IngameUI.GetPlayerInfo()._active.activeSelf)
                {
                    //스킬 무효 연출
                    IngameUI.GetPlayerInfo().SetSkillInvalidity(bMyTurn);
                }

                //여기에 연출 때림
                //Debug.Log("강습타구나 뜬금포가 투수 스킬에 의해 무효화!!");
                batterSkillFlag = SkillFlag.None;
                SimulManager.SetBatterSkill(null);
                
            }
            
        }

#if _Skill_Display
        private CSkill setTestPitcherPitchSkill()
        {
            if (pitcherSkill_Display_test == pSkillDisplay.Hoe_Sim_Il_Gyeog)
            {
                return new CSkill(10007, SkillIndex.TenderStroke, true);
            }
            else if (pitcherSkill_Display_test == pSkillDisplay.Mea_Hog)
            {
                return new CSkill(10008, SkillIndex.Charm, true);
            }

            return null;
        }

        private CSkill setTestBatterPitchSkill()
        {
            if (batterSkill_Display_test == bSkillDisplay.Mea_Noon)
            {
                return new CSkill(20009, SkillIndex.FalconEye, true);
            }

            return null;
        }
#endif

        public void effectCheck(SkillEffectDisplayManager.DisplayStep timing)
        {
            //연출
            CSkill pitcherSkill = SimulManager.GetPitcherSkill();
            CSkill batterSkill = SimulManager.GetBatterSkill();
#if _Skill_Display
            //연출테스트용            
            CSkill pitchPitcherSkill = setTestPitcherPitchSkill();
            batter.tempPitchSkill = pitchPitcherSkill;
#else
            //투수 피치 스킬 플래그 설정
            CSkill pitchPitcherSkill = SimulManager.GetPitchPitcherSkill();
#endif
            if (pitcherSkill != null || pitchPitcherSkill != null)
            {
                SkillEffectDisplayManager.EffectDisplay(timing, true);
            }
#if _Skill_Display
            //연출테스트용
            CSkill pitchBatterSkill = setTestBatterPitchSkill();
#else
            //타자 피치 스킬 플래그 설정
            CSkill pitchBatterSkill = SimulManager.GetPitchBatterSkill();
#endif
            if (batterSkill != null || pitchBatterSkill != null)
            {
                SkillEffectDisplayManager.EffectDisplay(timing, false);
            }
        }
    }
}
