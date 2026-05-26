//#define _NO_FIELD_CONTROL     //특능 자동으로 컨트롤 되게 설정 - 지워지워
//#define _NO_FIELD_ZOOM                                         - 지워지워
//#define _RANDOM_HIT_TEST      //와일드 피치 테스트시 킴        - 지워지워
//#define _NOHOOKSLICE
//#define _TEST_SKILL_BALANCE                                    - 지워지워
//#define _INPUT_SKILL                                           - 지워지워
//#define _NO_FIELDING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace BaseBall.BallPlay
{
    public class Field : MonoBehaviour
    {
#if _TEST_SKILL_BALANCE
        //public int _TEST_DECISION = 0;
        //public int _TEST_RANGE = 0;
        public int _TEST_SONIC = 0;
        public int _TEST_TIEUP = 0;
        public int _TEST_LEAD = 0;
        public int _TEST_QUICKMOTION = 0;
        public int _TEST_SLIDING = 70;
#endif
        public GameObject [] tempClone;

        //타임 스케일 상수
        public const float INIT_TIME_SCALE = 1f;// 1.10f;// 1.35f;
        public const float DECISIVE_TIME_SCALE = 1;// 1.10f;//1.35f;//1.5f;
        
        //Field상수
        public const int _FielderZOrder = 4; //필드 Z order        

        //Object
        public Transform fieldTransform;
        public BallPlayManager manager;
        public Pitcher pitcher;
        public Batter batter;
        public FBall ball;
        public runnerManager run;
        public judgeManager judge;
        public Fielder[] fielder;// = new Fielder[9];
        public Batting battingview;
        public FieldState fieldState;   //필드 View 상태
        public GameObject firstBound;   //첫바운드 오브젝트
        public GameObject tvChase;
        

        //필드 벡터
        //1.포지션
        public float _FIELD_SIZE_X; //맵 픽셀 사이즈 가로
        public float _FIELD_SIZE_Y; //맵 픽셀 사이즈 세로
        public float baseHomePosY;
        public float homeX, homeY;
        public float nBenchPosX, nBenchPosY, nBenchFielderPosX, nBenchFielderPosY;
        public float throwW, throwH; //송구시 화면 중점에 두기 위한 변수
        public float ballPower, firstBallSpeed;     //타구파워
        //public float batXOffsetRate, batYOffsetRate;
        //public float throwingX, throwingY;  //송구좌표
        public float groundingDstX, groundingDstY;// 그라운더 포구 좌표
        //2.타임
        float curTime;
        float timeScale;

        //카메라        
        //1.줌
        //public float fRatio; //current View
        public float _ZOOM_SIZE = 1;//나중에 변수로 바꿔
        public bool bZoomInState, bZoomOutState; //현재 zoom 여부
        public float destZoom, zoomSpeed, curZoom;
        public bool bFieldZoom, bThrowZoom;
        public bool bFieldPerspectiveZoom;        
        //public bool bFieldEffectActive;
        float zoomRemainTime;
        //public int bFiedZoomStep;        
        
        //필딩에 관련되는 정수값들
        public int flyCatchFielder;         //플라이를 처리하는 야수 인덱스
        public int groundCatchFielder;//, groundCatchFielder2;      //그라운드 처리하는 야수 인덱스
        public int nCatchIndex;             //현재 볼을 잡은 야수 인덱스
        public int fieldShift;              //필드쉬프트 인덱스
        public int nThrowIndex, nFirstThrowIndex, curThrowIndex; //던지는 야수 인덱스, 첫 송구를 한 야수 인덱스
        public int nTargetIndex;            //받는 야수 인덱스
        public int nCarrierIndex;           //공을 운반하는 야수 인덱스
        public int nRelayFielderIndex;      //릴레이하는 야수 인덱스
        public int nFirstThrower;           //처음 공을 던진 야수 인덱스        
        public int nRecheckTarget;
        public int nCheckBaseNum;           //베이스를 돌때마다 체크해줌    - 태그 아웃 전용
        public int flyCatchAvaiableCount;               //플라이 캐치를 할수 있는 야수 수 2명이상이면 최단 2인만 available처리 나머지는 익셉션
        public float fastRemainTime, throwRemainTime;   //남은 시간        
        public int doublePlayType;                      //더블플레이 타입        
        public int getAddScore, lastAddScore;           //실점 스코어    
        public int lastPitcherAddScore;                     //승계실점
        public int getAddErrScore, lastPitcherAddErrScore;  //자책스코어        
        public int pickOffCount;                            //견제 수 / 매 이닛배터시 0으로 세팅
        public int stealBaseTarget;
                
        //필딩에서 사용되는 각종 플래그
        public bool bFirstThrow;                    //공을 처음 던졌는지 여부
        public bool bTossThrow;                     //토스 송구
        public bool bOnceWildThrow;                 //악송구 여부
        public bool bFieldOut;			            //아웃이 된경우    
        public bool bGrounderAvailble;              //그라운더 처리 가능 플래그
        public bool bAssist, bPutOut;                               //보살, 자살 플래그
        public bool[] bBaseCoverd;// = new bool[4];                 //베이스 커버 여부
        public bool bOutByFlyball;                                  //플라이 아웃 플래그
        public bool earlygrounder, grounder;   //타구 타입
        public bool infieldFlyOut, shallowFlyOut, deepFlyOut;       //플라이 타입
        public bool bRelaying, bThrowing, bThrowBallCatched;
        bool bCatcherFieldChecked;                                  //포수의 필딩 여부
        public bool bPitcherException;                              //피처 필딩 예외 여부
        public bool bFieldersChoice;                                //야수초이스 여부
        public bool bHomeRunGangTa;
        public bool bPitcherKill;                                   //피처죽이기 여부        
        public bool bGrounderSpecial, bFlyballSpecial;
        public bool bSpecialMoveActivte;        
        public bool bDelayedCall;        
        public bool bStealThrow;
        public bool bOutCalled;                                 //아웃 콜이 불려진 경우
        public bool bHomerunStealTry = false;
        public bool bFieldStealFlag, bFieldPickOffFlag, bFieldDelayStealFlag; //스틸/견제/딜레이스틸 상태 플래그
        public bool bOutofInfield;
        public bool bCrushDelay;
        public bool bNormalFielding;    //보통 수비 상황: 도루,포볼,픽오프등의 상황이 아닌경우
        public bool bNoCheckReThrow = false;
        public bool bBaseCoverAfterLiner = false;
        public bool bFoulFlyOut;

        //장타 체크
        public bool bMoreDouble;

        //도루 관련 플래그
        public bool stealSuccess; //도루성공여부

        //견제관련
        public bool bPickOffOut;       //견제사 여부

        //에러
        public int nErrorFielder;                       //에러하는 야수 인덱스
        public FieldParm.ErrorType errorType;     //포구 에러타입
        public bool bErrorFlag;                         //에러,안타 플래그
        public bool bThrowErrorFlag;
        public bool bInfieldThrowErrorFlag;

        //와일드
        public FieldParm.WildPitchCase wildPitchCase;
        private bool bWildPitchCatcherBlock;

        //충돌
        public bool bCollisionFlag;
        public bool bFielderCrushEffect;

        //번트 관련 플래그
        public bool bBuntFielding;  //번트수비 여부
        public bool bBuntSuccess;//, bSqeezAreadyFail;
        public bool bSqueezeFieldOut; //피치아웃으로인한 스퀴즈 실패
        //public bool bSqueezeFlagOn; //bSqueezePitchedOut
        
        //기타 필딩에 필요한 변수
        public float delayedCallTime;        //아웃 콜을 약간 늦게
        public bool bBallTail;              //false인 경우 볼테일링을 없앤다.        
        public int throwFrame;

        public bool b2ndLeadCheck;

        //찬스모드 견과
        public SimulResultState chanceResult = SimulResultState.Grounder;

        //스킬 시전
        //1. 투수
        
        
        //3. 주자
        public FieldSkillUse runnerHomeRush;
        public FieldSkillUse runnerTurbo;
        public FieldSkillUse runnerDPStop;
        //public FieldSkillUse runnerDelaySteal;

        //스킬 플래그
        public bool bNoMoreHomeRushFlag; // true시 더이상 홈돌진 없음
        public bool bRushCounterHappen;  //레이저에 대항하는 홈돌진 카운터 발생여부
        //public bool bBlockBonusByLaser;  //레이저에 의한 블록 보너스
        public bool bFieldVsSkillOffenseWin;


        //미사용 변수
        //public bool bActiveField; //액티브 필드 여부
        //조작
        public bool bInputWait;
        public bool bRunnerControlPossible, bFielderControlPossible;
        public bool bMustSlowMove;

        
        //텍스쳐 초기화
        public bool bFielderTextureInit1, bFielderTextureInit2, bFielderTextureInit3;
        //배팅 데이터
        public SimulBattingData battingResult;
        //현재타자 파울수
        public int curBatterFoulNum;


        //public bool bMotionBlur = false;
        //private bool bBlurSetting = false;
        //public float blurDV;


        public bool bFieldViewActive;
        public bool b2DBattingSystem;       //2D배팅 시스템 여부
        public FieldParm.BattingViewFieldingType battingviewFieidingType; //2D배팅뷰에서  


        //필드 vs연출
        public bool bVsShow;
        public SkillID vsSkillID;
        public int vsSkillRank;


        //네트워크 버퍼
        public int[] netTarget;// = new int[9];
        public float[] netOneMoreValue;// = new float[4];
        public bool[] netBaseSafe;// = new bool[4];
        public bool bFieldSyncCheck;

        
        /////////////////////////////////////////////////////////////
        //초기화 함수
        /////////////////////////////////////////////////////////////
        void Awake()
        {
            
        }

        // Use this for initialization
        void Start()
        {
            fireWorkObj.SetActive(false);
            returnCheckInit();
        }

        void Update()//FixedUpdate()
        {
            if (bFieldViewActive == true)
            {
                nextFrame();
            }
        }

        //인스턴스 초기화 함수
        public void initInstance(BallPlayManager manager)
        {
            fieldState = FieldState.NORMAL_FIELD;
            this.manager = manager;
            pitcher = manager.pitcher;
            batter = manager.batter;
            battingview = manager.battingview;
            //camera = manager.camera;

            bInputWait = false;
            bMustSlowMove = false;

            //스타디움 로드
            loadStadium(Mode.stadiumNum);
            setFieldRatio(FieldParm.InitRatio);//, false);

            //fielder = new Fielder[9];
            bBaseCoverd = new bool[4];
            transform.position = new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX, BallPlayManager.FIELDVIEW_CAMERA_INITY, 0);
            //필드
            for (int i = 0; i < 9; i++)
            {
                //fielder[i] = Util.Load("MainGame/prefabs/FieldViewPrefab/fielderPrefab", transform, new Vector3(0, 0, _FielderZOrder)).GetComponent<Fielder>();
                fielder[i].initInstance(manager);
            }

            //심판
            //judge = Util.Load("MainGame/prefabs/FieldViewPrefab/judgeManagerPrefab", transform, Vector3.zero).GetComponent<judgeManager>();
            judge.InitInstance(this);

            bZoomInState = false;
            bZoomOutState = false;
            bHomerunCeremony = false;
            
            //연출관련
            //fieldSkillFlag.Clear();


            //Debug.Log("================>>네트워크 필드버퍼 초기화");
            netTarget = new int[9];
            netOneMoreValue = new float[4];
            netBaseSafe = new bool[4];
            setNetBufferInit();
            
        }

        //스타디움 로드 - 로딩 개선에 반드시 포함
        private void loadStadium(int stadimNum)
        {
            fieldTransform = Util.Load(("MainGame/prefabs/FieldViewPrefab/stadium/stadiumPrefab" + stadimNum), transform, Vector3.zero).transform;
        }

        /*
        public void setActiveField(bool bActive)
        {
            //fieldTransform.gameObject.SetActive(bActive);
        }*/


        //필드 벡터 초기화
        public void initFieldVector(float x = 0, float y = 0)
        {
            //Debug.Log("필드벡터초기화");
            ball.nBallX = 0;	//공좌표계 기준
            ball.nBallY = 0;	//공좌표계 기준	
            bZoomInState = false;
            bZoomOutState = false;
            ball.nScreenX = ball.nBallX * _ZOOM_SIZE - BallPlayManager.m_lcdWC;
            ball.nScreenY = ball.nBallY * _ZOOM_SIZE - BallPlayManager.m_lcdHC;

        }

        //필드 조건 초기화
        public void initFieldCondition()
        {
            //int i;
            //int errorper;

            ball.nScreenX = ball.nBallX * _ZOOM_SIZE - throwW;
            ball.nScreenY = ball.nBallY * _ZOOM_SIZE - throwH;

            bFieldOut = false;

            //run.bHitterRunnerBackAndSafe = false;
            run.bRunnerHomeRun = run.bHomeRunEventOver = run.bRunnerFoul = false;


        }

        //필드 초기화 (*매우중요 :  즉 어떤 상태에서든지 필드로 가는 경우는 반드시 이 함수를 호출)
        public bool initField()
        {
            if(Mode.bPvpMode433 == true)
            {
                //PVP용 랜덤 초기화
                Random.InitState(manager.Pvp_RandonSeed);
            }

#if _RANDOM_HIT_TEST
#if _Local_Balance
            //InGameDebug._ALWAYS_CATCH_ERROR = bCatchErrorFlagOn;
            //InGameDebug._ALWAYS_THROW_ERROR = bThrowErrorFlagOn;
#endif
#endif
            Time.timeScale = 1.0f;
            returnWaitTime = 1.2f;

#if _INPUT_SKILL
            //조작 관련 초기화
            if (Mode.bAutoPlay == true)
            {
                //자동 플레이시
                bRunnerControlPossible = false;
                bFielderControlPossible = false;
            }
            else
            {
                bRunnerControlPossible = (manager.bMyTurn ? true : false);
                bFielderControlPossible = (manager.bMyTurn ? false : true);
            }
#else
            bRunnerControlPossible = false;
            bFielderControlPossible = false;
#endif
            //manager.playingCount = 0;
            //setActiveField(true);

            //카메라
            CameraManager.CameraPositionInit();
            //CameraManager.SetBloom(false);
            //CameraManager.SetBlur2(false);
            //CameraManager.SetInvert(false);
            run.setRunnerCamera(false);
            FieldCrowdManager.SetActive(true);

            //returnCheckInit();

            getAddErrScore = getAddScore = lastAddScore = 0;
            lastPitcherAddErrScore = lastPitcherAddScore = 0;
            

            homeX = getOriginX(FieldSize.getHomePosX());
            homeY = getOriginY(FieldSize.getHomePosY());

            setFieldRatio(FieldParm.InitActiveRatio);//, false);
            //
            setZoom(FieldParm.InitZoom);

            baseHomePosY = getOriginY(FieldSize.getHomePosY());

            ball.bBallDeadState = false;

            if (run.bRunnerWalk == true)
            {
                setZoomStop(); //살려살려 -> 이거 안하면 도루시 구장바깥이 보이는 크리티컬 버그 발생
                //랜덤시드 싱크
                manager.RandomSeedSync(); //setRandomSeedSync();
                bFieldSyncCheck = false;

                bNormalFielding = false;
                ball.setActive(true);
                ball.setDraw(false);
                ball.setParticleDraw(false);
                setWalkFielding();

                /*
                ball.setCameraPosInit(FieldSize.getHomePosX(), FieldSize.getHomePosY());
                setZoom(1.4f);
                CameraManager.SetActiveCameraDstAngle(-15, 2.0f);
                setZoomTo(0.7f, 2.0f);
                ball.setFocusMove(FieldSize.getHomePosX(), FieldSize.getHomePosY(), getOriginX(FieldSize.getMoundPosX()), getOriginY(FieldSize.getMoundPosY()), BallEvent.EVENT_BASE_FOCUS, -1, 2.0f); 
                */

                //ball.setCameraPosInit(FieldSize.getHomePosX(), FieldSize.getMoundPosY());
                CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + FieldSize.getHomePosX(), BallPlayManager.FIELDVIEW_CAMERA_INITY + FieldSize.getMoundPosY(), -200));
                setZoom(0.9f);
                CameraManager.SetActiveCameraInitAngle(-15);
                setZoomTo(0.7f, 3.0f);
                returnWaitTime = 0;
            }
            else if (run.bPickOff == true)
            {
                setZoomStop(); //살려살려 -> 이거 안하면 도루시 구장바깥이 보이는 크리티컬 버그 발생
                //랜덤시드 싱크
                manager.RandomSeedSync(); //setRandomSeedSync();
                bFieldSyncCheck = false;

                bNormalFielding = false;
                //ball.setCameraPosInit(FieldSize.getMoundPosX(), FieldSize.getMoundPosY());
                CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + FieldSize.getHomePosX(), BallPlayManager.FIELDVIEW_CAMERA_INITY + FieldSize.getMoundPosY(), -200));
                setZoom(1.0f);
                ball.setActive(true);
                ball.setDraw(false);
                ball.setParticleDraw(false);
                setPickOffFielding();
                judge.setJudgeStealFieding();
                for (int i = 0; i < 9; i++) fielder[i].bObjectInit = true;
                setZoomTo(0.75f, 1.5f);

            }
            else if (run.bStealBase == true)
            {
                setZoomStop(); //살려살려 -> 이거 안하면 도루시 구장바깥이 보이는 크리티컬 버그 발생
                //랜덤시드 싱크
                manager.RandomSeedSync(); //setRandomSeedSync();
                bFieldSyncCheck = false;

                bNormalFielding = true;
                IngameUI.GetPitchUI().SetPitchCursor(false,0,0);
                IngameUI.GetBattingCall().SetActive(false);                
                ball.setActive(true);
                ball.setDraw(false);
                ball.setParticleDraw(false);
                setStealFielding();
                judge.setJudgeStealFieding();
                for (int i = 0; i < 9; i++) fielder[i].bObjectInit = true;

                //ball.setCameraPosInit(FieldSize.getHomePosX(), FieldSize.getHomePosY());
                CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + FieldSize.getHomePosX(), BallPlayManager.FIELDVIEW_CAMERA_INITY + FieldSize.getHomePosY(), -200));
                setZoom(1.4f);
                CameraManager.SetActiveCameraInitAngle(-15);

            }
            else if (run.bWildPitchRunning == true)
            {
                setZoomStop(); //살려살려 -> 이거 안하면 도루시 구장바깥이 보이는 크리티컬 버그 발생

                //랜덤시드 싱크                
                manager.RandomSeedSync(); //setRandomSeedSync();

                bFieldSyncCheck = false;

                bNormalFielding = true;
                IngameUI.GetPitchUI().SetPitchCursor(false, 0, 0);
                bWildPitchCatcherBlock = pitcher.checkCatcherBlock();
                ball.setWildPitch(bWildPitchCatcherBlock);
                setWildPitchFielding(bWildPitchCatcherBlock);
                if (wildPitchCase == FieldParm.WildPitchCase.NoRunner)
                {
                    StartCoroutine(updateFieldScene2(1.8f));
                }
                else if (wildPitchCase == FieldParm.WildPitchCase.BaseOnBall)
                {                    
                }

                if (bWildPitchCatcherBlock == true)
                {
                    if (wildPitchCase != FieldParm.WildPitchCase.BaseOnBall)
                    {
                        Fielder catcher = fielder[CPlayer._CATCHER];
                        if (catcher.pFielder.skillAvailable(SkillIndex.CatcherBallBlocking) == true)
                        {
                            //수비형포수 - 투구블럭 연출
                            fieldSkillDisplayManager.AddSkill(catcher.gameObject, catcher.pFielder, SkillIndex.CatcherBallBlocking);
                        }
                    }
                }
                for (int i = 0; i < 9; i++) fielder[i].bObjectInit = true;

                CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + FieldSize.getHomePosX(), BallPlayManager.FIELDVIEW_CAMERA_INITY + FieldSize.getHomePosY() - 32, -200));

                //
                ball.bBallDeadState = true;

            }
            else
            {
                /*
                bNormalFielding = true;
                bStealThrow = false;
                ball.setActive(true);
                ball.setDraw(true);
                initFieldCondition();
                setFielding();
                run.checkRunning();
                judge.setJudgeFieding(ball.firstAngle);*/
                //ball.setCameraPosInit(FieldSize.getMoundPosX(), BallPlayManager.m_lcdHC, true);
                manager.RandomSeedSync(); //

                bNormalFielding = true;
                //float posX = ball.transform.position.x;
                //float posY = ball.transform.position.y;
                //CameraManager.SetCameraPos(new Vector3(posX, posY, -200));
                CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + FieldSize.getHomePosX(), BallPlayManager.FIELDVIEW_CAMERA_INITY + FieldSize.getHomePosY(), -200));

                /*
                bBlurSetting = true;
                blurDV = -3.0f / 0.4f;
                CameraManager.SetFieldBlur(true, 2);*/

                judge.setJudgeFieding(ball.firstAngle);
                FieldCrowdManager.SetfieldBackPosition(ball.firstAngle , ball.angleHookSlice);

            }

            if (Mode.gameMode == Mode.GamePlayMode.Pvp ||
                Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                //필드에서 채팅창 설정
                IngameUI.GetEmoticonChatting().fieldviewSetting();
            }
            
            return true;

        }

        //찬스모드의 필드 초기화
        public void initFieldChance()
        {
            getAddErrScore = getAddScore = lastAddScore = 0;
            lastPitcherAddErrScore = lastPitcherAddScore = 0;

            homeX = getOriginX(FieldSize.getHomePosX());
            homeY = getOriginY(FieldSize.getHomePosY());
            setFieldRatio(FieldParm.InitActiveRatio);
            baseHomePosY = getOriginY(FieldSize.getHomePosY());
        }

        public void setFieldHitState()
        {
            //랜덤시드 싱크
            setRandomSeedSync();

            //returnCheckInit();            
            bStealThrow = false;
            ball.setActive(true);
            ball.setDraw(true);
            initFieldCondition();
            setFielding();
            run.checkRunning();
            //judge.setJudgeFieding(ball.firstAngle);
            //bFieldViewActive = true;    
            ball.bBallDeadState = false;

            if(Mode.bPvpMode433 == true)
            {
                Debug_UI.SetNetwork(false);
                if(manager.bMyTurn == true)
                {
                    pvpmanager.Get().SendFieldingSyncInfo(manager);
                }
            }

        }


        public void setRandomSeedSync()
        {
            /*if (Mode.bPvpMode == true)
            {
                Random.InitState(PvpManager.RandomSeed);
                Debug.Log("테스트 랜덤 ======>>>>> " + Random.Range(0.0f, 100.0f) + "   시드 : " + PvpManager.RandomSeed);
            }*/
        }



        //공수교대시 필드 초기화
        public bool initFieldWhenChangeInning()
        {
            bRunnerControlPossible = false;
            bFielderControlPossible = false;
            //카메라
            //CameraManager.SetBloom(false);
            //CameraManager.SetBlur2(false);
            //CameraManager.SetInvert(false);
            run.setRunnerCamera(false);

            homeX = getOriginX(FieldSize.getHomePosX());
            homeY = getOriginY(FieldSize.getHomePosY());

            setFieldRatio(FieldParm.InitRatio);//, false);
            setZoom(FieldParm.InitZoom);
            bNormalFielding = false;
            baseHomePosY = getOriginY(FieldSize.getHomePosY());


            bStealThrow = false;
            ball.setActive(true);
            ball.setDraw(true);
            initFieldCondition();

            judge.setJudgeFieding(0);


            ball.setCameraPosInit(FieldSize.getMoundPosX(), BallPlayManager.m_lcdHC);

            return true;

        }

        /*
        //선수데이터로부터 각각의 포지션의 야수를 초기화 한다
        public void initFielder()
        {
            int team = (manager.defenseIndex);

            //야수 초기화
            for (int i = 0; i < BallPlayManager.NUM_LINEUP; i++)
            {
                int pos = SimulPlayerManager.GetCurPosition(team, i);// manager.currentPosition[team, i];

                if (pos != CPlayer._DH)
                {
                    if (pos == CPlayer._PITCHER)//   b_pPlayer[team][index].m_bPitcher) //투수인경우 플레이에 관한 초기화
                    {
                        CPlayer player = SimulPlayerManager.GetPitcher(team);// manager.pPithcer[team, manager.pitcherIndex[team]];
                        //pitcher.initPitcher(player, team);		    //피처 클래스에 	
                        fielder[pos].initParameter(player,  pos);
                    }
                    else
                    {
                        CPlayer player = SimulPlayerManager.GetFielder(team, i);// manager.pFielder[team, i];
                        fielder[pos].initParameter(player,  pos);
                    }

                    fielder[pos].loadFielder(manager.bTopInning);
                }
            }

            //투수 초기화
            CPlayer _player = SimulPlayerManager.GetPitcher(team);
            pitcher.initPitcher(_player, manager.defenseIndex, true);
        }*/


        public IEnumerator initFielder2()
        {
            manager.bFielderLoadComp = false;
            ////Debug.Log("==============================>>initFielder2");
            int team = (manager.defenseIndex);

            //야수 초기화
            for (int i = 0; i < BallPlayManager.NUM_LINEUP; i++)
            {
                int pos = SimulPlayerManager.GetCurPosition(team, i);// manager.currentPosition[team, i];

                if (pos < CPlayer._DH && pos >= CPlayer._PITCHER)
                {
                    if (pos == CPlayer._PITCHER)//   b_pPlayer[team][index].m_bPitcher) //투수인경우 플레이에 관한 초기화
                    {
                        CPlayer player = SimulPlayerManager.GetPitcher(team);// manager.pPithcer[team, manager.pitcherIndex[team]];
                        //pitcher.initPitcher(player, team);		    //피처 클래스에 	
                        fielder[pos].initParameter(player, pos);
                    }
                    else
                    {
                        CPlayer player = SimulPlayerManager.GetFielder(team, i);// manager.pFielder[team, i];
                        fielder[pos].initParameter(player, pos);
                    }

                    fielder[pos].loadFielder(manager.bTopInning);
                    fielder[pos].transform.localPosition = Vector3.zero;

                    yield return new WaitForSeconds(0.1f);
                }
            }

            //투수 초기화
            CPlayer _player = SimulPlayerManager.GetPitcher(manager.defenseIndex);
            pitcher.initPitcher(_player, manager.defenseIndex, true);

            yield return new WaitForSeconds(0.1f);
            //카메라 뷰 초기화
            battingview.lastView = CameraView.None;
            CameraView curView = CameraView.BatterLow;
            if (Mode.bPitchingViewActive == true)
            {
                curView = (manager.bMyTurn == true ? CameraView.BatterLow : CameraView.PitcherCenter);
            }
            battingview.settingView(curView);   //카메라 세팅에 따른 뷰 설정

            if (manager.bBatterForceLoad == true)
            {
                //Debug.Log("======================>>> 이닝초에서는 여기서 타자 강제 로딩");
                manager.bBatterForceLoad = false;
                manager.battingview.gameObject.SetActive(true);
                yield return new WaitForEndOfFrame();                
                batter.LoadBatter();
                yield return new WaitForSeconds(1.0f);
            }

            ////Debug.Log("======================>>> batterLoadFlag = " + manager.batter.bLoadBatterFlag);

            manager.bFielderLoadComp = true;

        }

#if _RewindMode
        //선수데이터로부터 각각의 포지션의 야수를 초기화 한다(리와인드 모드)
        public void initRewindFielder(SimulCurrentPlayerData playerData, CPlayer rewindPitcher)
        {
            int team = (manager.defenseIndex);
          
            //리와인드 모드 야수 초기화
            for (int i = 0; i < 9; i++)
            {
                if (i == CPlayer._PITCHER)
                {
                    CPlayer player = rewindPitcher;
                    fielder[i].initParameter(player,  i);
                    fielder[i].loadFielder(manager.bTopInning);
                }
                else
                {
                    CPlayer player = getRewindFielder(team, playerData.fielderSeq[i]);
                    ////UnityEngine.//Debug.Log("============================>>playerData.fielderSeq[i] = " + playerData.fielderSeq[i] + " 이름: " + player.getName());
                    fielder[i].initParameter(player,  i);
                    fielder[i].loadFielder(manager.bTopInning);
                    
                }
            }
            //리와인드 투수 초기화
            ////UnityEngine.//Debug.Log("============================>> 투수 이름: " + rewindPitcher.getName());
            pitcher.initPitcher(rewindPitcher, team, true);
        }

        //리와인드 모드에서 시퀀스를 참조하여 현재 야수 얻어오기
        private CPlayer getRewindFielder(int team, long seq)
        {
            CPlayer _fielder = null;

            for (int i = 0; i < SimulPlayer.NUM_FIELDER; i++)
            {
                _fielder = SimulPlayerManager.GetFielder(team, i, false);
#if _Test_Local
                {
                    if (_fielder.picIndex == seq)
                    {
                        return _fielder;
                    }
                }
#else
                {
                    if (_fielder.getCard().cardSeq == seq)
                    {
                        return _fielder;
                    }
                }
#endif
            }
            return null;
        }
#endif

        //선수데이터로부터 투수 포지션을 초기화 한다
        public void initPitcher(bool positionInit)
        {
            ////UnityEngine.//Debug.Log("======================================================>>INIT PITCHER 여기서 야수위치 초기화");
            int i;

            ball.state = BallState.BALL_PITCHING;	//볼의 상태는 피칭			
            //	m_bBallSeen = true;
            ball.setBallInit();
            // ball.bBallThrowing = false;



            throwW = (BallPlayManager.m_lcdWC);// / 2);
            throwH = (BallPlayManager.m_lcdHC);// / 2);

            /*if (Mode.bPvpMode == true)
            {
                Random.InitState(PvpManager.RandomSeed2);
                //Debug.Log("필드 스킬용 테스트 랜덤 ======>>>>> " + Random.Range(0, 1000) + "   시드2 : " + PvpManager.RandomSeed2);
            }*/

            //Debug.Log("필드 스킬전 테스트 랜덤 ======>>>>> " + Random.Range(0, 1000) + "   시드 : " + PvpManager.RandomSeed);
            //Debug.Log("매우중요매우중요=================================>>>필드 스킬 발동여부 체크");

            setSkillFlagInit();

            for (i = 0; i < 9; i++)
            {
                fielder[i].initSetting(i, positionInit);
            }

            //nWildThrowFalg = 0;
            bOnceWildThrow = false;
            bPitcherKill = false;


        }


        /////////////////////////////////////////////////////////////
        //필드의 시간 관련 함수
        /////////////////////////////////////////////////////////////
        //필드의 deltaTime을 얻어온다
        public float getDeltaTime()
        {
            return Time.deltaTime * timeScale;
        }

        //필드상의 모든 오브젝트에 대한 timeScale을 설정한다
        public void setFielderTimeScale()
        {
            for (int i = 0; i < 9; i++)
            {
                fielder[i].setTimeScale(timeScale);
            }

            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    run.runner[i].setTimeScale(timeScale);
                }
                judge.judge[i].setTimeScale(timeScale);
            }
        }

        //타임 스케일 세팅
        public void setTimeScale(float scale, bool once = false)
        {
            if (bInputWait == true) return;
            if (bMustSlowMove == true) return;

            /*if (Mode.bAutoPlay == true)
            {
                //자동 플레이시
                if (scale < DECISIVE_TIME_SCALE)
                {
                    timeScale = DECISIVE_TIME_SCALE;
                }
                else
                {
                    timeScale = scale;
                }
                setFielderTimeScale();
            }
            else*/
            {
                if (once == true)
                {
                    if (timeScale == INIT_TIME_SCALE) return;
                }

                timeScale = scale;
                setFielderTimeScale();
            }

        }

        //현재 타임스케일 얻어온다
        public float getTimeScale()
        {
            return timeScale;
        }


        /////////////////////////////////////////////////////////////
        //타구 벡터 함수
        /////////////////////////////////////////////////////////////
        //타구 발생시 벡터값을 계산하여 세팅한다
        public void setHitVector(bool bBunt = false)
        {
            if (Mode.bPvpMode433 == true && manager.bMyTurn == false) //if (Mode.bPvpMode == true && manager.bMyTurn == false)
            {
                ball.setNormalGraivty();

                //pvp에서 얻어온 타격 정보 세팅
                manager.setPvpBattingInfo();

                bBuntFielding = manager.Pvp_bBunt;
                if (bBuntFielding == true)
                {
                    bBuntSuccess = false;
                    batter.buntResult = manager.Pvp_BuntResult;

                    if (batter.buntResult == SpecificBuntType.DRAG_SUCCESS ||
                       batter.buntResult == SpecificBuntType.SAC_FIELDER_CHOICE || batter.buntResult == SpecificBuntType.SAC_SUCCESS ||
                       batter.buntResult == SpecificBuntType.SQUEEZ_SUCCESS || batter.buntResult == SpecificBuntType.SQUEEZ_FIELDER_CHOICE)
                    {
                        bBuntSuccess = true;
                        batter.buntSuccess = batter.buntType;
                    }
                    setBuntShift();
                    run.setBuntSpeed();
                }
            }
            else
            {
                ball.setTopSpin();
                bBuntFielding = bBunt;

                if (bBunt == false)
                {
                    //타격
                    if (manager.batterSkillFlag == SkillFlag.Unexpected)
                    {
                        //뜬금포
                        setUnexpectedHit();
                    }
                    else if (manager.batterSkillFlag == SkillFlag.AssaultBall)
                    {
                        //강습타구
                        setAssultBallHit();
                    }
                    else
                    {
#if _Skill_Display
                        //연출테스트용
                        setSkillVector();
#else
                        //일반타격
                        ball.setNormalGraivty();
                        ballPower = getBallPower();
                        ball.angleZ = getBallAngle();  //0~3까지
                        ball.angle = getBallDirection();  //-3~3까지                        
#endif
                    }
                    setAdjustHitType();
                    setHookSlice();
                }
                else
                {
                    //번트 체크
                    setBuntVector();
                    setBuntShift();
                    run.setBuntSpeed();
                }
            }

            ball.firstAngleZ = ball.angleZ;
            ball.firstAngle = ball.angle;

            if (Mode.bPvpMode433 == true)
            {
                if (manager.bMyTurn == true)
                {
                    //파워 한계설정 -> pvp인 경우 내턴에만
                    setPowerLimit();
                }
            }
            else
            {
                //파워 한계설정 -> 각종 이상한 타구 및 장외홈런 제한
                setPowerLimit();
            }
            

            //ball.angleZ이 30보다 작으면 땅볼 파울
            //ball.realSpeed = ballPower * Mathf.Cos(ball.angleZ * Mathf.Deg2Rad);
            ball.speed = FBall._BALLSPEED_COEF * ballPower * Mathf.Cos(ball.angleZ * Mathf.Deg2Rad);
            firstBallSpeed = ball.speed;

            ////Debug.Log("==============================>> FirstBallSpeed = " + firstBallSpeed);
            ball.nBallX = homeX;// getOriginX(FieldSize.getHomePosX());
            ball.nBallY = homeY;// getOriginY(FieldSize.getHomePosY(Field.InitRatio));
            ball.nBallZ = FieldParm.BALL_INIT_HEIGHT;		//처음 높이
            //Debug.Log("=====>> nBallX = " + ball.nBallX+ "   nBallY = "+ ball.nBallY);
            ball.state = BallState.BALL_FLYING;
            ball.setVelocity();
            ball.setVelocityZ(ballPower);
            ball.getFirstBound2();

            //Debug.Log("======>>ball.angle = " + ball.angle + "         ball.firstAngle = " + ball.firstAngle);

            if (Mode.bPvpMode433 == true)
            {
                if (manager.bMyTurn == true)
                {
                    pvpmanager.Get().SendBattingInfo(this);
                }
            }

            

            /*if (Mode.bPvpMode == true)
            {
                if (manager.bMyTurn == true)
                {
                    PvpManager.GetInstance().SendBattingInfo();
                }
            }*/
        }

        //이상한 타구및 장외홈런 제한
        private void setPowerLimit()
        {
            //angleZ에 따른 예외 처리
            if (ball.angleZ < -40)
            {
                //-40이하인경우 볼파워 30제한
                if (ballPower > 30) ballPower = 30;
            }
            else if (ball.angleZ >= 55)
            {
                if (ballPower < 20)
                {
                    //55이상인 경우 투포13 딜레이 0.05
                    fielder[CPlayer._PITCHER].FIELD_DELAY = 0.05f;// 0.2f;
                    fielder[CPlayer._CATCHER].FIELD_DELAY = 0.05f;
                    fielder[CPlayer._FIRSTBASEMAN].FIELD_DELAY = 0.05f;
                    fielder[CPlayer._THIRDBASEMAN].FIELD_DELAY = 0.05f;
                }
            }
            else
            {
                //장외 홈런 처리
                if (MyMath.Percent() < 90)
                {
                    if (ball.angleZ >= 25) //홈런각
                    {
                        if (ballPower > 32.5f)
                        {
                            if (ball.angleZ > 36) //고각
                            {
                                ballPower = Random.Range(31.5f, 33.0f); //pvp랜덤체크
                            }
                            else //저각
                            {
                                ballPower = Random.Range(32.5f, 33.5f); //pvp랜덤체크
                            }
                        }
                    }

                }
            }
        }


        //뜬금포
        private void setUnexpectedHit()
        {
            ballPower = Random.Range(32.0f, 35.0f);    
            ball.angleZ = Random.Range(35.0f, 45.0f);  
            ball.angle = getBallDirection();  //-3~3까지   
            ball.setNormalGraivty();
        }

        //강습타구
        private void setAssultBallHit()
        {
            ballPower = Random.Range(45.0f, 50.0f);
            ball.angleZ = Random.Range(-8.0f, -1.0f);
            float _angle = getBallDirection();  //-3~3까지   
            if (_angle > 0)
            {
                if (_angle > 23) ball.angle = 36f;
                else ball.angle = 15;
            }
            else
            {
                if (_angle < -23) ball.angle = -36f;
                else ball.angle = -15;
            }
            ball.setNormalGraivty();
        }

        //빠따 뿌러질 경우 볼 벡터 재계산
        public void setBrokenBat()
        {
            //초기 물리값과 위치 첫바운드 위치 재계산
            if (ballPower > 22)
            {
                ballPower = Random.Range(17.0f, 22.0f); //pvp랜덤체크
                ball.speed = FBall._BALLSPEED_COEF * ballPower * Mathf.Cos(ball.angleZ * Mathf.Deg2Rad);
                firstBallSpeed = ball.speed;

                ball.nBallX = homeX;
                ball.nBallY = homeY;
                ball.nBallZ = FieldParm.BALL_INIT_HEIGHT;		//처음 높이
                ball.state = BallState.BALL_FLYING;
                ball.setVelocity();
                ball.setVelocityZ(ballPower);
                ball.getFirstBound2();
            }
        }

#if _Skill_Display
        //연출테스트용
        private void setSkillVector()
        {
            //일반타격
            ballPower = getBallPower();
            ball.angleZ = getBallAngle();  //0~3까지
            ball.angle = getBallDirection();  //-3~3까지     
        }
#endif

#if _RANDOM_HIT_TEST
        int count = 0;
        public float _Power = 1000; 
        public float _AngleZ = 1000;
        public float _AngleX = 1000;
        public float _HOOKSLICE = 1000;
        public float _BUNTPOWER = 1000;
        public float _BUNTANGLE = 1000;

        public bool bCatchErrorFlagOn = false;
        public bool bThrowErrorFlagOn = false;
        public bool bFirstHitCase = false;

#endif
                
        //타구의 파워를 얻어옴
        private float getBallPower()
        {
#if _RANDOM_HIT_TEST
            //40~50을 max 10~15 min            
            //float val = (run.bOnBase[1] == false ? 30 : 30);  //홈돌진  주자 스피드 450 야수 어꺠 800으로 테스트
            //float val = (run.bOnBase[0] == false?35:32);  //딜레이드 스틸, 레이저
            //float val = (run.bOnBase[0] == false ? 35 : 24);//병살저지
            //float val = (run.bOnBase[1] == false ? 35 : 32);//레이저 vs 홈돌진
            //float val = (run.bOnBase[0] == true ? 25 : 35);
            float val = 35;// Random.Range(20, 32);//
            if (_Power != 1000) val = _Power;//
            if (bFirstHitCase == true) val = 30;
            Debug.Log("################ SET HIT VECTOR Power = " + val);
            return val;
#else
            float val = batter.getBatterBasicPower();
            return val;
#endif
        }

        //타구의 방향을 얻어옴
        private float getBallDirection()
        {
#if _RANDOM_HIT_TEST
            //float val = (run.bOnBase[1] == false ? 15 : 0);  //홈돌진
            //float val = (run.bOnBase[0] == false ? 0 : -5); //딜레이드 스틸, 레이저
            //float val = (run.bOnBase[0] == false ? 0 : 35); //병살저지
            //float val = (run.bOnBase[1] == false ? 15 : -5); //레이저 vs 홈돌진
            //float val = (run.bOnBase[0] == true ? 15 : 0);
            ball.setHookorSlice(false, 0);
            float val = 0;// Random.Range(-35, 35);
            if (_AngleX != 1000) val = _AngleX;
            if (bFirstHitCase == true) val = 15;    
            if (_HOOKSLICE != 1000)
            {
                ball.setHookorSlice(true, _HOOKSLICE);
            }
            UnityEngine.Debug.Log("################ SET HIT VECTOR AngleX = " + val);
            if (val == 0) val = 0.001f;
            count++;
            return val;
#else            
            //ball.setHookorSlice(false, 0); //훅 슬라이스에 문제 생기면 이거 다시 켜
            float val = batter.getBatterBasicDirection();
            if (curBatterFoulNum > 2)
            {
                if (val > 40) val = Random.Range(0.0f, 40.0f);
                else if (val < -40) val = Random.Range(0.0f, -40.0f);
            }
            //UnityEngine.Debug.Log("################ SET HIT VECTOR Direction = " + val);
            if (val == 0) val = 0.001f;
            return val;

#endif
        }

        //타구의 앵글을 얻어옴
        private float getBallAngle()
        {
#if _RANDOM_HIT_TEST
            //60 맥스, -50 최저
            //float val = (run.bOnBase[1] == false ? 20 : 12);//홈돌진
            //float val = (run.bOnBase[0] == false ? 0 : -20);//딜레이드 스틸 , 레이저
            //float val = (run.bOnBase[0] == false ? 0 : -20);//병살저지
            //float val = (run.bOnBase[1] == false ? 20 : -20);//레이저 vs 홈돌진
            //float val = (run.bOnBase[0] == true ? -10 : 5);
            float val = 35;// Random.Range(25, 45);            
            if(_AngleZ!=1000)   val = _AngleZ;
            if (bFirstHitCase == true) val = 20;
            Debug.Log("################ SET HIT VECTOR AngleZ = " + val);

            return val;
#else            
            float val = batter.getAngleZ();
            //UnityEngine.Debug.Log("################ SET HIT VECTOR AngleZ = " + val);
            return val;
#endif
        }


        //타구 타임을 조정
        public void setAdjustHitType()
        {
            //최저파워 설정
            if (ball.angleZ < 30) 
            {
                if (ballPower < 12)
                {
                    ballPower = Random.Range(12.0f, 14.0f); //pvp랜덤체크
                }
            }            
            else 
            {
                if (ballPower < 15)
                {
                    //플라이볼의 최저파워
                    ballPower = Random.Range(15.0f, 18.0f); //pvp랜덤체크
                }
            }

            //
            if (ball.angleZ > 60)
            {
                manager.hitBallType = HITBALLTYPE._POPUP;
            }
            else
            {
                if (ballPower >= 32)
                {
                    if (ball.angleZ > 30) manager.hitBallType = HITBALLTYPE._STRONG_FLY;
                    else if (ball.angleZ > 10) manager.hitBallType = HITBALLTYPE._LINEDRIVE;
                    else manager.hitBallType = HITBALLTYPE._GROUNDER;
                }
                else if (ballPower >= 28)
                {
                    if (ball.angleZ > 30) manager.hitBallType = (ballPower >= 30 ? HITBALLTYPE._STRONG_FLY : HITBALLTYPE._FLYBALL);
                    else if (ball.angleZ > 15) manager.hitBallType = HITBALLTYPE._LINEDRIVE;
                    else manager.hitBallType = HITBALLTYPE._GROUNDER;
                }
                else if (ballPower > 25)
                {
                    if (ball.angleZ > 40) manager.hitBallType = HITBALLTYPE._FLYBALL;
                    else if (ball.angleZ > 20) manager.hitBallType = HITBALLTYPE._LINEDRIVE;
                    else manager.hitBallType = HITBALLTYPE._GROUNDER;
                }
                else
                {
                    manager.hitBallType = HITBALLTYPE._GROUNDER;
                }

                if (manager.hitBallType == HITBALLTYPE._GROUNDER && ball.angleZ > 30)
                {
                    manager.hitBallType = HITBALLTYPE._POPUP;
                }
            }
        }


        private void setHookSlice()
        {
#if _NOHOOKSLICE
            //훅없음
#else
            if (ball.angleZ < 50 && ballPower > 25)
            {
                int per = MyMath.Percent();
                float angleValue = Mathf.Abs(ball.angle);
                float sign = Mathf.Sign(ball.angle);
                if (angleValue > 35)
                {
                    if (per < 80 && ball.angleZ > 15)
                    {
                        ball.setHookorSlice(true, sign * Random.Range(3, 8));
                    }
                }
                else if (angleValue > 20)
                {
                    if (per < 60 && ball.angleZ > 25)
                    {
                        ball.setHookorSlice(true, sign * Random.Range(3, 6));
                    }
                }
                else
                {
                    if (per < 40 && ball.angleZ > 35)
                    {
                        ball.setHookorSlice(true, sign * Random.Range(3, 5));
                    }
                }
            }
#endif
        }


        //번트시 타구의 벡터를 세팅
        private void setBuntVector()
        {
            ////UnityEngine.//Debug.Log("================================>>buntType = " + batter.buntType);
            ////UnityEngine.//Debug.Log("================================>>buntFielder = " + batter.buntFielder);

            bool bBuntFly = false;
            bBuntSuccess = false;
            if (batter.buntResult == SpecificBuntType.DRAG_SUCCESS ||
               batter.buntResult == SpecificBuntType.SAC_FIELDER_CHOICE || batter.buntResult == SpecificBuntType.SAC_SUCCESS ||
               batter.buntResult == SpecificBuntType.SQUEEZ_SUCCESS || batter.buntResult == SpecificBuntType.SQUEEZ_FIELDER_CHOICE)
            {
                bBuntSuccess = true;
                batter.buntSuccess = batter.buntType;
            }
            else
            {
                //if (batter.buntResult == SpecificBuntType.SQUEEZ_FAIL) bSqeezAreadyFail = true;
                if (MyMath.Percent() < 7)
                {
                    //실패시 번트 플라이 발생률 7%
                    bBuntFly = true;
                }
            }

            ////UnityEngine.//Debug.Log("============================>>>bBuntSuccess = " + bBuntSuccess);
            //batter.buntFielder = CPlayer._CATCHER; //테스트용 -> 반드시 지울것
            //bSqueezeFieldOut = true;  //테스트용 -> 반드시 지울것
            ballPower = FieldParm.GetBuntPower(batter.buntType, bBuntSuccess, batter.buntFielder, bBuntFly);
            ball.angleZ = FieldParm.GetBuntAngleZ(batter.buntType, bBuntSuccess, batter.buntFielder, bBuntFly);
            ball.angle = FieldParm.GetBuntAngleX(batter.buntType, bBuntSuccess, batter.buntFielder, bBuntFly, bSqueezeFieldOut);

            if (bBuntFly == true)
            {
                ball.angle *= batter.buntDir;
            }
        }

        //번트 타구시 야수 쉬프트
        private void setBuntShift()
        {
            //Debug.Log("=============>>번트 쉬프트 체크");
            if (batter.buntType == SimulBuntType.DRAG) 
            {
                //Debug.Log("=============>>드래그 번트 체크");
                //기습번트는 2루수 위치만
                fielder[CPlayer._SECONDBASEMAN].posX = FieldSize.getBuntPosX(CPlayer._SECONDBASEMAN) + 200;
                fielder[CPlayer._SECONDBASEMAN].posY = FieldSize.getBuntPosY(CPlayer._SECONDBASEMAN) / 0.6f - 200;

                //3루수 특급송구 체크
                if (fielder[CPlayer._THIRDBASEMAN].skillDashThrowLevel > 0)
                {
                    fielder[CPlayer._THIRDBASEMAN].posX = FieldSize.getBuntPosX(CPlayer._THIRDBASEMAN);
                    fielder[CPlayer._THIRDBASEMAN].posY = FieldSize.getBuntPosY(CPlayer._THIRDBASEMAN) / 0.6f;
                }

                //1루수 특급송구 체크
                if (fielder[CPlayer._FIRSTBASEMAN].skillDashThrowLevel > 0)
                {
                    fielder[CPlayer._FIRSTBASEMAN].posX = FieldSize.getBuntPosX(CPlayer._FIRSTBASEMAN);
                    fielder[CPlayer._FIRSTBASEMAN].posY = FieldSize.getBuntPosY(CPlayer._FIRSTBASEMAN) / 0.6f;
                }

            }
            else
            {
                //Debug.Log("=============>>기타 번트 체크 buntFielder : " + batter.buntFielder);
                //필더 위치 재조정
                for (int i = 0; i < CPlayer._LEFTFIELDER; i++)
                {
                    fielder[i].posX = FieldSize.getBuntPosX(i);
                    fielder[i].posY = FieldSize.getBuntPosY(i) / 0.6f;

                    if (i == CPlayer._SECONDBASEMAN)
                    {
                        if (batter.buntFielder == CPlayer._PITCHER || batter.buntFielder == CPlayer._CATCHER || batter.buntFielder == CPlayer._FIRSTBASEMAN)
                        {
                            fielder[i].posX += 200;
                            fielder[i].posY -= 200;
                        }
                    }
                    else if (i == CPlayer._SHORTSTOP)
                    {
                        if (batter.buntFielder == CPlayer._THIRDBASEMAN)
                        {
                            fielder[i].posX -= 250;
                            fielder[i].posY -= 250;
                        }
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////
        //필드 카메라워크 관련 함수
        /////////////////////////////////////////////////////////////
        //필드 비율 세팅
        public void setFieldRatio(float ratio)//, bool bZoom)
        {
            //fRatio = FieldParm.InitRatio;// ratio;
            fieldTransform.localScale = Vector3.one;// new Vector3(1 * 2.0f, fRatio * 2.0f, 1);
            _FIELD_SIZE_X = FieldSize.getWidth();
            _FIELD_SIZE_Y = FieldSize.getHeight();
            /*if (ball != null)
            {
                float sX = getScreenX(ball.nFirstBoundX);
                float sY = getScreenY(ball.nFirstBoundY);
                firstBound.transform.localPosition = new Vector3(sX, sY, -0.01f);
            }*/

        }

        //현재 필드 비율을 체크
        public void checkFieldRatio(float yPos)//,bool bZoom)
        {
           
        }

        //필드 줌 세팅
        public void setZoom(float zoomSize)
        {
            //줌 
            _ZOOM_SIZE = zoomSize;//나중에 변수로 바꿔
            //camera.ZoomFactor = _ZOOM_SIZE;
            CameraManager.SetFieldZoomFactor(_ZOOM_SIZE);
        }

        //다음 줌 비율 세팅
        public void setZoomTo(float destSize, float remainTime)
        {
            if (_ZOOM_SIZE != destSize)
            {
                curZoom = _ZOOM_SIZE;
                destZoom = destSize;
                zoomSpeed = ((destZoom - curZoom) / remainTime) * getDeltaTime();
                bZoomInState = false;
                bZoomOutState = false;
                if (curZoom > destSize) bZoomOutState = true;
                else bZoomInState = true;
            }
        }

        public void setThrowingZoom(int posIndex)
        {
            /*
            if (run.bWildPitchRunning)
            {
                return;
            }*/
            if (run.bPickOff || run.bStealBase == true)
            {
                float remainTime = (ball.throwingTime * 0.95f);
                setZoomTo(1.0f, remainTime);
            }
            else
            {
                float remainTime = Mathf.Clamp((ball.throwingTime * 0.95f), 0.8f, 5.0f);
                if (posIndex >= CPlayer._LEFTFIELDER || posIndex == nRelayFielderIndex)
                {
                    if (nTargetIndex == FieldParm.HOMEBASE_INDEX || nTargetIndex == FieldParm.THIRDBASE_INDEX)
                    {
                        setZoomTo(1.05f, remainTime);
                    }
                    else
                    {
                        setZoomTo(0.75f, remainTime);
                    }
                }
                else
                {
                    if (nTargetIndex == FieldParm.FIRSTBASE_INDEX)// && posIndex > CPlayer._FIRSTBASEMAN)
                    {
                        CameraManager.SetActiveCameraDstAngle(-15, remainTime);
                        setZoomTo(0.75f, remainTime);
                    }
                    else if (nTargetIndex == FieldParm.SECONDBASE_INDEX)
                    {
                        CameraManager.SetActiveCameraDstAngle(-15, remainTime);
                        setZoomTo(0.8f, remainTime);
                    }
                    else
                    {
                        setZoomTo(1.05f, remainTime);
                    }
                }
            }
        }


        //줌 스탑
        public void setZoomStop()
        {
            bZoomInState = false;
            bZoomOutState = false;
        }

        //줌 프레임
        public void nextZoom()
        {
            if (bZoomOutState == true)
            {
                curZoom += (zoomSpeed);// * getDeltaTime());
                if (curZoom < destZoom)
                {
                    curZoom = destZoom;
                    bZoomOutState = false;
                }
                setZoom(curZoom);
                ball.checkScroll();
            }
            else if (bZoomInState == true)
            {
                curZoom += (zoomSpeed);// * getDeltaTime());
                if (curZoom > destZoom)
                {
                    curZoom = destZoom;
                    bZoomInState = false;
                }
                setZoom(curZoom);
                ball.checkScroll();
            }

        }

        //카메라 체인지 여부 체크
        public bool checkCameraChange()
        {
            bool bChange = false;
            if (nThrowIndex == CPlayer._FIRSTBASEMAN)
            {
                if (nTargetIndex == FieldParm.FIRSTBASE_INDEX) bChange = true;
            }
            else if (nThrowIndex == CPlayer._THIRDBASEMAN)
            {
                if (nTargetIndex == FieldParm.THIRDBASE_INDEX) bChange = true;
            }
            else if (nThrowIndex == CPlayer._LEFTFIELDER)
            {
                if (nTargetIndex == FieldParm.FIRSTBASE_INDEX) bChange = true;
            }
            else if (nThrowIndex == CPlayer._RIGHTFIELDER)
            {
                if (nTargetIndex == FieldParm.THIRDBASE_INDEX) bChange = true;
            }
            return bChange;
        }



        /////////////////////////////////////////////////////////////
        //필드의 각종 위치 세팅 함수
        /////////////////////////////////////////////////////////////
        //벤치 포지션을 세팅
        public void setBenchPosition()
        {
            if (manager.bMyHome == true)
            {
                if (manager.bMyTurn == true)
                {
                    nBenchPosX = FieldSize.getHomeBenchPosX();
                    nBenchPosY = FieldSize.getHomeBenchPosY();
                    nBenchFielderPosX = FieldSize.getAwayBenchPosX();
                    nBenchFielderPosY = FieldSize.getAwayBenchPosY();
                }
                else
                {
                    nBenchPosX = FieldSize.getAwayBenchPosX();
                    nBenchPosY = FieldSize.getAwayBenchPosY();
                    nBenchFielderPosX = FieldSize.getHomeBenchPosX();
                    nBenchFielderPosY = FieldSize.getHomeBenchPosY();
                }
            }
            else
            {
                if (manager.bMyTurn == true)
                {
                    nBenchPosX = FieldSize.getAwayBenchPosX();
                    nBenchPosY = FieldSize.getAwayBenchPosY();
                    nBenchFielderPosX = FieldSize.getHomeBenchPosX();
                    nBenchFielderPosY = FieldSize.getHomeBenchPosY();
                }
                else
                {
                    nBenchPosX = FieldSize.getHomeBenchPosX();
                    nBenchPosY = FieldSize.getHomeBenchPosY();
                    nBenchFielderPosX = FieldSize.getAwayBenchPosX();
                    nBenchFielderPosY = FieldSize.getAwayBenchPosY();
                }
            }
        }

        //필드 쉬프트 세팅
        public void setFieldShift(int step, bool bSetPos, bool shift)
        {
            int i;
            /*
             * _FIRSTBASEMAN_SHIFT_POS = new int[2] { GROUND_SIZEW - 1315, GROUND_SIZEH - 1482 };
        public static int[] _SECONDBASEMAN_SHIFT_POS = new int[2] { 0, 0 };
        public static int[] _SHORTSTOP_SHIFT_POS = new int[2] { 0, 0 };
        public static int[] _DOUBLEPLAY_OFFSET*/

            for (i = 0; i < 9; i++)
            {
                //이부분 나중에 수정 - 타일좌표에서 절대좌표로
                fielder[i].originX = getOriginX(FieldSize.getFielderPosX(i));// *TILE_WIDTH;
                fielder[i].originY = getOriginY(FieldSize.getFielderPosY(i));// *TILE_WIDTH;


                if (shift == true)
                {
                    if (i == CPlayer._FIRSTBASEMAN)
                    {
                        if (run.bOnBase[FieldParm.SECONDBASE_INDEX] == false && run.bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
                        {
                            //1루 견제 쉬프트 
                            fielder[i].originX = getOriginX(FieldSize.getFirstBasemanSiftPosX());
                            fielder[i].originY = getOriginY(FieldSize.getFirstBasemanSiftPosY());
                        }
                    }

                    if (i == CPlayer._SECONDBASEMAN)
                    {
                        /*if (run.bOnBase[FieldParm.SECONDBASE_INDEX] == true)
                        {
                            if (batter.nBatterHand == CPlayer._RIGHTHAND)
                            {
                                //2루 견제 쉬프트
                                fielder[i].originX = getOriginX(_SECONDBASEMAN_SHIFT_POS[0]);
                                fielder[i].originY = getOriginX(_SECONDBASEMAN_SHIFT_POS[1]);
                            }
                        }
                        else*/
                        if (run.bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
                        {
                            if (manager.nOutCount < 2)
                            {
                                //더블플레이 쉬프트
                                // fielder[i].originX -= 50;// _DOUBLEPLAY_OFFSET[0];
                                // fielder[i].originY -= 50;//_DOUBLEPLAY_OFFSET[1];
                            }
                        }

                    }


                    if (i == CPlayer._SHORTSTOP)
                    {
                        /*
                        if (run.bOnBase[FieldParm.SECONDBASE_INDEX] == true)
                        {
                            if (batter.nBatterHand == CPlayer._LEFTHAND)
                            {
                                //2루 견제 쉬프트
                                fielder[i].originX = getOriginX(_SHORTSTOP_SHIFT_POS[0]);
                                fielder[i].originY = getOriginX(_SHORTSTOP_SHIFT_POS[1]);
                            }
                        }
                        else*/
                        if (run.bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
                        {
                            if (manager.nOutCount < 2)
                            {
                                //더블플레이 쉬프트
                                //fielder[i].originX += 50;//_DOUBLEPLAY_OFFSET[0];
                                //fielder[i].originY -= 50;//_DOUBLEPLAY_OFFSET[1];
                            }
                        }
                    }

                }


                fielder[i].posX = fielder[i].originX;
                fielder[i].posY = fielder[i].originY;
                //fielder[i].lineY = fielder[i].originY;

                if (bSetPos == true)
                {
                    fielder[i].setPosition();
                }
            }

        }

        /* //필딩 결과 세팅
         public void setFieldingResult(int team, int index, bool errorAdd, bool stealAdd, bool runAdd, bool reverseAdd)
         {
             int error, steal, run, reverse;//,temp;

             int point = batter.nFieldResult[team, index];

             reverse = (point >> 24) & 0x0f;
             error = (point >> 16) & 0xff;
             steal = (point >> 8) & 0xff;
             run = (point) & 0xff;

             reverse += (reverseAdd ? 1 : 0);
             if (reverse >= 14) reverse = 14;
             error += (errorAdd ? 1 : 0);
             steal += (stealAdd ? 1 : 0);
             run += (runAdd ? 1 : 0);

             batter.nFieldResult[team, index] = ((reverse & 0x0f) << 24) | ((error & 0xff) << 16) | ((steal & 0xff) << 8) | (run & 0xff);

             //f_RunnerFieldIndex[FIELD_GetRunnerIndex(i)]<--참조	베이스 기준
             //f_RunnerFieldIndex[i]<--참조	인덱스 기준

         }*/

        //필더 전체를 Stop 세팅
        public void setFielderStop(int notThisIndex)
        {
            for (int i = 0; i < 9; i++)
            {
                if (i != notThisIndex)
                {
                    fielder[i].setStop();
                }
            }
        }

        public void setRunnerStop(bool bInitPos = false)
        {
            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    run.runner[i].setDoNothing(bInitPos);
                }
            }

            battingview._1stRunner.setBvRunnerMove(false, true);
            battingview._2ndRunner.setBvRunnerMove(false, true);
        }

        /////////////////////////////////////////////////////////////
        //필드의 상태 변환 세팅
        /////////////////////////////////////////////////////////////
        //파울 세팅
        public void setFoulCall()//float delay)
        {
            //yield return new WaitForSeconds(delay);
            //Debug.Log("===============>>FOUL CALL!!!!!!!!!!!!!");
            bool bFoulCallSign = true;
            curBatterFoulNum++;
            //bFiedZoomStep = 0;
            setFielderStop(-1);
            setRunnerStop();

            returnCheckNC(-0.3f, !batter.bBunt);


            if (ball.state == BallState.BALL_FOUL || ball.bHomeRunCall)
            {
                return;
            }
            else
            {
                if (run.bRunnerFoul == false)
                {
                    run.bRunnerFoul = true;
                    ball.state = BallState.BALL_FOUL;
                    ball.bFoulCall = true;
                    ball.bFairBall = false;
                    run.bHitterRunnerSafe = false;

                    if (manager.nStrikeCount >= 2)
                    {
                        if (batter.bBunt)
                        {
                            bFoulCallSign = false;
                            ball.bFoulCall = false;
                            ball.bFairBall = true;
                            ball.state = BallState.BALL_FOUL;
                            setOutCondition(false, true); //쓰리번트 아웃                         
                        }
                       
                    }
                    else //if(b_nStrikeCount<2)
                    {
                        manager.nStrikeCount++;
                    }
                }


                if (bFoulCallSign == true)
                {
                    judge.setCall(0, CallType._FOUL);
                    IngameUI.GetFieldCall().Call("foul");
                }
            }
        }

        public GameObject fireWorkObj = null;
        public bool bHomerunCeremony = false;

        private CameraView fireWorkView;

        //홈런 콜 세팅
        public void setHomerunCall(bool bPoleCol = false)
        {
            manager.nHomerunCount[manager.offenseIndex]++;
            bErrorFlag = false;
            setFielderStop(-1);

            //임시 방편
            //////UnityEngine.//Debug.Log("===============>>HOMERUN CALL!!!!!!!!!!!!!");
            int num = 0;
            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    num++;
                    run.runner[i].setMove(); //makeRunnerMove(i, false);
                }
            }

            if (bPoleCol == false)
            {
                ball.setDraw(false);
                ball.setBallStop();
            }

            battingview.gameObject.SetActive(true);
            battingview.setInitAngle();
            battingview.setCenterView();
            battingview._runnerPosition.SetActive(false);

            BackGroundManager.SetDisplayEffect("HOMERUN");
            manager.displayEffectType = 0;

            IngameUI.GetFieldCall().HomeRun(num);


            if (bHomerunStealTry == true)
            {
                //홈런사운드
                judge.setCall(0, CallType._HOMERUN);
            }

            bHomerunStealTry = false;

            setZoom(1.0f);

            ball.setHomerunEvent();

            setTimeScale(2.5f);

            //살려살려
            fireWorkView = Mode.cameraView;
            if (fireWorkView == CameraView.PitcherCenter)
            {
                //현재카메라가 피칭뷰인 경우 바꿈
                battingview.settingView(CameraView.BatterLow, true); //타자 안지움
                battingview.setCameraOffset();
            }

            CameraManager.ChangeCamera(BallPlayManager._BATTINGVIEW, 0, 0);
            batter.gameObject.SetActive(false);
            pitcher.setPitcherPosOffset(0, 0);

            manager.battingview.setInitPosition();
            manager.battingview.setBattingviewRunnerDelete();

            //살려살려 폭죽 이상해짐
            //fireWorkObj = Util.Load("MainGame/prefabs/firework/fireworksPrefab", null, new Vector3(0, 0, 0));
            fireWorkObj.SetActive(true);
            fireWorkObj.GetComponent<fireWork>().Init();

            bHomerunCeremony = true;

            pitcher.pitcherHomerunAnim();

            FieldCrowdManager.SetCrowdActiveAll(false);

            ball.step = BallStep.BALL_DEAD_STATE;
        }

        //포볼 콜 세팅
        public void setFourBallCall()
        {
            Debug.Log("====================>>set Four ball");
            //도루시 상태 무효화
            run.setStealInvalid();

            manager.bChangeFlag = true;
            pitcher.pState = PitcherState._ALLOW_FOURBALL;
            //	g_nBatterState  = _GET_FOURBALL;
            //manager.nFrame = -1;
            manager.bBall = false;
            ball.nFirstBoundX = 0;
            initFieldVector();

            run.bRunnerWalk = true;
            ball.state = BallState.BALL_DEAD;
            ball.step = BallStep.BALL_DEAD_STATE;

            manager.playState = PlayState.PLAY_FIELDING_VIEW;	//나중에 이걸로 바꿔

            ball.bFairBallGuess = true;
            manager.changeFieldView(FieldState.NORMAL_FIELD);

            setTimeScale(2.5f);

        }

        //도루 상황
        public void setStealState()
        {
            //manager.nFrame = -1;
            ball.nFirstBoundX = 0;
            nThrowIndex = nCatchIndex = CPlayer._CATCHER;
            ball.state = BallState.BALL_FAIR;
            ball.step = BallStep.BALL_DEAD_STATE;
            manager.playState = PlayState.PLAY_FIELDING_VIEW;	//나중에 이걸로 바꿔
            ball.bFairBallGuess = true;
            manager.changeFieldView(FieldState.NORMAL_FIELD);
            run.bStealBase = true;
        }

        //역전플래그 체크
        private void checkTurnAround()
        {
            if (manager.nInningCount >= 3)
            {
                if (manager.bMyTurn == true)
                {
                    if (manager.bTurnAroundFlag[0] == false)
                    {
                        if ((manager.nGameScore[0] <= manager.nGameScore[1])
                         && (manager.nGameScore[0] + getAddScore > manager.nGameScore[1]))
                        {
                            manager.bTurnAroundFlag[0] = true;
                            manager.bTurnAroundFlag[1] = false;
                        }
                    }
                }
                else
                {
                    if (manager.bTurnAroundFlag[1] == false)
                    {
                        if ((manager.nGameScore[1] <= manager.nGameScore[0])
                         && (manager.nGameScore[1] + getAddScore > manager.nGameScore[0]))
                        {
                            manager.bTurnAroundFlag[1] = true;
                            manager.bTurnAroundFlag[0] = false;
                        }
                    }
                }
            }
        }

        //스코어 발생 상황 세팅
        private bool bAddScore;
        private void setAddScore()
        {
            bAddScore = false;
            if (getAddScore > 0 && manager.bThreeOutChange == false)
            {
                pitcher.conRun += getAddScore;

                //기세
                //manager.setAp(2 * getAddScore, manager.offenseIndex);
                //manager.setAp(-2 * getAddScore, manager.defenseIndex);

                //역전플래그 체크
                checkTurnAround();

                //아닌 경우 실시간 점수 add
                if (manager.bMyTurn == true)
                {
                    manager.nGameScore[0] += getAddScore;
                    manager.nInningScore[0, (manager.nInningCount - 1)] += getAddScore;
                    manager.pitcher.allowRun[1] += getAddScore;
                }
                else
                {
                    manager.nGameScore[1] += getAddScore;
                    manager.nInningScore[1, (manager.nInningCount - 1)] += getAddScore;
                    manager.pitcher.allowRun[0] += getAddScore;
                }
                manager.checkWinLoseIndex();

                //투수 실점 계산            
                if (lastPitcherAddScore > 0)
                {
                    //승계주자 관련
                    manager.addPitcherRecord(Param.ST_PR, false, lastPitcherAddScore); //승계주자 실점
                    if (manager.outCountIF < 3 && lastPitcherAddErrScore > 0)
                    {
                        manager.addPitcherRecord(Param.ST_PER, false, lastPitcherAddErrScore); //승계주자 자책
                    }
                }

                //본인 실점 관련
                manager.addPitcherRecord(Param.ST_PR, true, (getAddScore - lastPitcherAddScore)); //실점

                if (manager.outCountIF < 3 && (getAddErrScore - lastPitcherAddErrScore) > 0)
                {
                    ////UnityEngine.//Debug.Log("========================================>>자책으로 계산");
                    manager.addPitcherRecord(Param.ST_PER, true, (getAddErrScore - lastPitcherAddErrScore)); //자책
                }
                else
                {
                    ////UnityEngine.//Debug.Log("========================================>>비자책으로 계산");
                }

                pitcher.pPitcher.setPiledupSkill(SkillIndex.WinSpirit, 1, false); //필승의지 효과제거

                //타점 계산
                if (manager.fieldOutCountNum < 2 && bErrorFlag == false)
                {
                    //더블 플레이가 아닌 경우 타자 타점 가산
                    manager.strBatterResult += " (타점 " + getAddScore + ")";
                    manager.addBatterRecord(Param.ST_RBI, getAddScore);
                }

                //득점 계산
                run.addRunnerRunStat();

                manager.nCurScore[0] = manager.nGameScore[0];
                manager.nCurScore[1] = manager.nGameScore[1];
               


                //점수 디스플레이
                
                IngameUI.GetScoreShow().ShowBoard(manager, (manager.bMyTurn ? 0 : 1), getAddScore);
                lastPitcherAddScore = getAddScore = 0;
                if (ball.bHomeRunGuess == false && run.bRunnerWalk == false)
                {
                    CameraManager.SetActiveCameraDstAngle(-15, 1.2f);
                    setZoomTo(0.65f, 1.2f);
                    ball.setFocusMove(ball.nBallX, ball.nBallY, getOriginX(FieldSize.getMoundPosX()), getOriginY(FieldSize.getMoundPosY()), BallEvent.EVENT_BASE_FOCUS, -1, 1.2f); 
                }
                bAddScore = true;
                //점수 사운드
                soundmanager.Get().PlaySound(soundmanager.SoundID.ScoreSound);
            }
        }

        //견제 세팅
        public void setPickOff(int _base)
        {
            if (run.bOnBase[_base] == true)
            {
                run.bPickOff = true;
                //manager.nFrame = -1;
                ball.nFirstBoundX = 0;
                nThrowIndex = nCatchIndex = CPlayer._PITCHER;
                nTargetIndex = _base;
                ball.state = BallState.BALL_FAIR;
                ball.step = BallStep.BALL_DEAD_STATE;
                manager.playState = PlayState.PLAY_FIELDING_VIEW;	//나중에 이걸로 바꿔
                ball.bFairBallGuess = true;
                manager.changeFieldView(FieldState.NORMAL_FIELD);
            }
        }


        //와일드 피치 사항
        public void setWildPitchState(FieldParm.WildPitchCase _case)
        {
            wildPitchCase = _case;
            run.bWildPitchRunning = true;
            //manager.nFrame = -1;
            ball.nFirstBoundX = 0;
            ball.state = BallState.BALL_FAIR;
            ball.step = BallStep.BALL_DEAD_STATE;
            manager.playState = PlayState.PLAY_FIELDING_VIEW;	//나중에 이걸로 바꿔
            ball.bFairBallGuess = true;
            manager.changeFieldView(FieldState.NORMAL_FIELD);

        }


        /////////////////////////////////////////////////////////////
        //필드로부터 배팅뷰로 돌아오는 것에 대한 관련 함수
        /////////////////////////////////////////////////////////////
        public bool bReturnBattingView;
        public float returnCheckTime;
        public float returnCheckTime2;
        public bool bReturnCheck;
        public bool bReturnException;
        public float returnWaitTime = 1.2f;

        //배팅뷰 리턴체크
        public void returnBattingViewCheck()
        {
            //if (bDelayedCall == true) return;            

            if (bReturnBattingView == true)
            {
                if (getAddScore > 0)
                {
                    if (getAddScore != lastAddScore)
                    {
                        lastAddScore = getAddScore;
                    }

                    if (bReturnCheck == false)
                    {
                        returnCheckTime += Time.deltaTime;
                        if (returnCheckTime > returnWaitTime)
                        {
                            //이벤트 발생시킴
                            ////////UnityEngine.//Debug.Log("==================>>>Add Score Event 발생시킴");
                            StartCoroutine(updateFieldScene(2.0f, true));
                            bReturnCheck = true;
                        }
                    }
                }
                else
                {
                    if (bReturnCheck == false)
                    {
                        returnCheckTime += Time.deltaTime;
                        if (returnCheckTime > returnWaitTime)
                        {
                            getAddScore = 0;
                            StartCoroutine(updateFieldScene(0.2f, true));
                            bReturnCheck = true;
                        }
                    }
                }
            }
            else
            {
                if (bReturnException == true) //예외처리
                {
                    bReturnException = false;
                    StartCoroutine(updateFieldScene(1.5f, true));
                }
                else
                {
                    returnCheckTime2 += Time.deltaTime; 
                    if (returnCheckTime2 > 15.0f)
                    {
                        //30초 강제 배팅뷰
                        returnCheckTime2 = 0;
                        bReturnException = true;
                    }
                }
            }
        }

        //리턴 체크 초기화
        public void returnCheckInit()
        {
            //Debug.Log("@@@@@@@@1");
            bReturnBattingView = false;
            returnCheckTime = 0;
        }

        //리턴 체크 : 해당 콘디션일때만
        public void returnCheck(float startFrame)
        {
            if (ball.step == BallStep.BALL_CATCH || ball.step == BallStep.BALL_DEAD_STATE || ball.step == BallStep.BALL_THROW_CATCH)
            {
                returnCheckNC(startFrame);
            }
        }

        //리턴 체크 : 노 콘디션
        public void returnCheckNC(float startFrame, bool bNoCondtion = false)
        {
            ////UnityEngine.//Debug.Log("==================>>returnCheckNC startFrame = " + startFrame);
            if (bFieldPickOffFlag == false && bFieldStealFlag == false)
            {
                if (noRunnerMove() == true || bNoCondtion == true || run.bRunnerWalk == true)
                {
                    bReturnBattingView = true;
                    returnCheckTime = startFrame;
                }
            }
        }

        private bool noRunnerMove()
        {
            if (manager.bThreeOutChange == true)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (run.runnerActive[i] == true)
                    {
                        if (run.runner[i].state > RunState.STANDBY && run.runner[i].state < RunState.GOODBYEHIT)
                        {
                            if (run.runner[i].bMoving == true)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }

        //리턴 체크 : 견제나 도루 상황으로 부터
        public void returnCheck_Steal_Pickoff(float startFrame)
        {
            bReturnBattingView = true;
            returnCheckTime = startFrame;
        }

        /// <summary>
        /// 필드 상태 종료
        /// </summary>
        public void checkFieldEnd()
        {
            if (ball.step == BallStep.BALL_HIT) return;
            if (noRunnerMove() == false) return;
            if (getAddScore > 0) return;
            if (run.bPickOff == true || run.bStealBase == true || run.bWildPitchRunning == true) return;

            bReturnBattingView = true;
            returnCheckTime = 0;
        }


        public bool forcedSetBattingView(float delay = 1.5f)
        {
        /*    if (run.bPickOff == true || run.bStealBase == true || bOnceWildThrow == true)
            {
                return false;
            }

            bool bNoRunningState = true;
            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    if ((int)run.runner[i].state > (int)RunState.STANDBY ||           //움직이고 있거나
                        run.runner[i].state == RunState.ADD_SCORE ||        //득점상황
                        run.runner[i].destPos == FieldParm.HOMEBASE_INDEX)          //도착점이 홈베이스
                    {
                        bNoRunningState = false;
                        break;
                    }
                }
            }

            if (bNoRunningState == true)
            {
                bReturnBattingView = false;
                bReturnException = false;
                StartCoroutine(updateFieldScene(delay, true));
                return true;
            }*/

            return false;
        }

        //다른 모드와 주자 상태를 동기화
        private void syncRunner()
        {
            SimulManager.SyncRunner(run); 
        }

        private bool bRunnerUpdateFlag;

        public bool bUpdateStealOrPickOff;

        //필드의 상태를 업데이트 한다
        public IEnumerator updateFieldScene(float delay, bool bSetBattingView)
        {
            //Debug.Log("=========================================>>updateFieldScene");

            //타자 대기 애니메이션 초기화 안되는 버그 관련
            batter.bBatterFieldUpdate = true;
            batter.bReadyAnim = false;

            bUpdateStealOrPickOff = false;
            setAddScore();

            if (bAddScore == true)
            {
                yield return new WaitForSeconds(delay + 0.5f);
                IngameUI.GetScoreShow().DeActive();
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }

            /*
            if (Mode.bPvpMode == true)
            {
                PvpManager.bBattingResultUpdate = true;
                setNetBufferInit();
            }*/

            //setAddScore();
            //////UnityEngine.//Debug.Log("==================>>혹시 이게 문제인가 현재 필드 상태 state "+manager.playState);
            b2ndLeadCheck = false;
            bool bBuntUpdate = false;

            if (bFieldViewActive == true)// if (manager.playState == PlayState.PLAY_FIELDING_VIEW)
            {
                if (run.bWildPitchRunning == true)
                {
                    if (wildPitchCase == FieldParm.WildPitchCase.RunnerOnBase)
                    {
                        b2ndLeadCheck = true;
                        run.checkUpdateOnBaseAfterSteal();
                    }
                    else
                    {
                        //낫아웃상황
                        if (manager.checkChanceModeEnd(SimulResultState.StrikeOut) == true)
                        {
                            //run.bRunnerUpdateFlag = true;// 
                            run.updateRunner();
                            yield break;
                        }
                        syncRunner();
                        batter.nextBatter();
                    }
                }
                else
                {
                    if (run.bPickOff || run.bStealBase)//||f_bPickOffState)
                    {
                        bUpdateStealOrPickOff = true;
                        b2ndLeadCheck = true;
                        if (manager.bStealStrikeOut == true)    //오토 경기에서는 이거 안나오도록 하자
                        {
                            ////UnityEngine.//Debug.Log("==================>>도루 아웃 여기서 처리");
                            syncRunner();
                            run.bHitterRunnerSafe = false;
                            batter.nextBatter();
                            manager.bStealStrikeOut = false;
                        }
                        else
                        {
                            batter.readyAnim(false);
                            //견제 혹은 도루후
                            if (Mode.bAutoPlay == true || manager.bMyTurn == false)
                            {
                                //오토진행 혹은 내 수비시
                                //번트 결과 업데이트
                                if (run.bStealBase == true || bPickOffOut == true)
                                {                                    
                                    //도루 혹은 견제사
                                    if (bPickOffOut == true)
                                    {
                                        //견제사시 도루작전 취소
                                        run.stealResult = SimulStealState.NONE;
                                        batter.bNewBatter = false;
                                    }
                                    //모든 번트 작전 취소
                                    batter.buntType = SimulBuntType.NONE;
                                    batter.buntResult = SpecificBuntType.NONE;
                                    bPickOffOut = false;
                                }
                                else
                                {
                                    bBuntUpdate = true; //batter.buntResult = batter.getBuntDynamicResult();
                                    if (run.bPickOff == true)
                                    {
                                        batter.bNewBatter = false;
                                    }
                                }

                                ////UnityEngine.//Debug.Log("=================++++++++++++++++++++++++++++++++++++++++++++++++++++++>> 내 수동 조작인 경우 buntResult = " + batter.buntResult);
                            }
                        }

                        if (manager.bThreeOutChange == true)
                        {
                            ////UnityEngine.//Debug.Log("============================================>>도루 아웃 쓰리아웃 찬스모드 엔드!!!!!!!!!!!!!!!!!!!!!");
                            //도루에 의한 기회 무산
                            if (manager.checkChanceModeEnd(SimulResultState.NONE) == true)
                            {
                                ////UnityEngine.//Debug.Log("============================================>>찬스모드 엔드!!!!!!!!!!!!!!!!!!!!!");
                                //run.bRunnerUpdateFlag = true;// 
                                run.updateRunner();
                                yield break;
                            }
                        }

                        run.checkUpdateOnBaseAfterSteal();
                    }
                    else
                    {
                        if (ball.bFairBall == true ||
                            ball.bHomeRunCall == true)
                        {
                            if (manager.checkChanceModeEnd(chanceResult) == true) // (PVP테스트_반드시_복구)
                            {
                                ////UnityEngine.//Debug.Log("============================================>>찬스모드 엔드!!!!!!!!!!!!!!!!!!!!!");
                                //run.bRunnerUpdateFlag = true;// 
                                run.updateRunner();
                                yield break;
                            } // (PVP테스트_반드시_복구)
                            syncRunner();
                            batter.nextBatter();
                        }
                        else
                        {                            
                            run.bHitterRunnerSafe = false;
                            if (Mode.bAutoPlay == false && manager.bMyTurn == true)
                            {
                                //수동진행 && 내 공격시
                                //파울시 번트 결과 업데이트
                                bBuntUpdate = true; //batter.buntResult = batter.getBuntDynamicResult();
                                ////UnityEngine.//Debug.Log("=================++++++++++++++++++++++++++++++++++++++++++++++++++++++>> 내 수동 조작인 경우 buntResult = " + batter.buntResult);
                            }
                            else
                            {
                                if (batter.buntType == SimulBuntType.DRAG || batter.buntType == SimulBuntType.SQUEEZE)
                                {
                                    //스퀴즈나 기습 후 파울등이 나오면 취소
                                    batter.buntType = SimulBuntType.NONE;
                                }
                            }
                            run.checkUpdateOnBaseAfterSteal();
                        }
                    }
                }

                manager.bStrike = manager.bBall = manager.bStrikeOut
                    = manager.bBaseOnBalls = false;

                batter.bHitted = false;
                batter.bHitChecked = false;
                
                getAddScore = lastAddScore = 0;
                lastPitcherAddScore = 0;

                /////////////////////////////////////////////////////////////////////////////
                //동기화 정보 보냄
                if (Mode.bPvpMode433 == true)
                {
                    if (manager.bMyTurn == true)
                    {
                        //동기화 정보 송신
                        //주자정보
                        //아웃정보
                        //쓰리아웃여부
                        //점수정보
                        pvpmanager.Get().SendFieldResultSync(manager);
                    }
                    else
                    {
                        //수신된 동기화 정보
                        if(manager.Pvp_FiendResultSync == false)
                        {
                            Debug_UI.SetNetwork(true);
                        }
                        while(manager.Pvp_FiendResultSync == false)
                        {
                            yield return new WaitForEndOfFrame();
                        }

                        FieldResultSync();

                        Debug_UI.SetNetwork(false);
                    }
                }
                /////////////////////////////////////////////////////////////////////////////

                run.updateRunner();    //run.bRunnerUpdateFlag = true;//             
                updateFielder();

                if (bBuntUpdate == true)
                {
                    batter.buntResult = batter.getBuntDynamicResult();
                    bBuntUpdate = false;
                }

                if (Mode.bAutoPlay == true || manager.bMyTurn == false)
                {
                    if (manager.bChangeFlagRunner == false)
                    {
                        manager.bRunnerChangeFlag = SimulManager.CheckChangeRunner();
                        manager.bChangeFlagRunner = true;
                    }
                }

                ball.bBallActive = false;
                ball.bStunBall = false;
                
                manager.saveGame1();
                setFieldRatio(FieldParm.InitRatio);//, false);
                //setZoom(1.0f);
                
                //카메라 트윈 제거
                UITweener tween = CameraManager.GetInstance().GetComponent<UITweener>();
                if (tween != null)
                {
                    tween.enabled = false;
                }

                if (bSetBattingView)
                {
                    if (Mode.b2outBaseLoadedMode == true && manager.bNineTwoNextRound == true)
                    {
                        //넥스트 라운드
                        //Debug.Log("================>>>다음 라운드");
                        StartCoroutine(manager.nineTwoNextRound());
                    }
                    else
                    {                        
                        returnBattigView();
                    }
                }
                manager.bThreeOutChange = false;

                IngameUI.GetControlRunner().bUpdateNeed = true;

                manager.Pvp_FiendResultSync = false;
            }
            ////UnityEngine.//Debug.Log("============================================>>UpdateFieldScene End~~~!!!!!");
        }


        private void FieldResultSync()
        {
            bool bCleanRunner = false;
            if(run.bOnBase[0] !=  manager.Pvp_bOnBase[0])
            {
                Debug.Log("============>> 1루주자 오류 동기화");
                bCleanRunner = true;
            }

            if (run.bOnBase[1] != manager.Pvp_bOnBase[1])
            {
                Debug.Log("============>> 2루주자 오류 동기화");
                bCleanRunner = true;
            }

            if (run.bOnBase[2] != manager.Pvp_bOnBase[2])
            {
                Debug.Log("============>> 3루주자 오류 동기화");
                bCleanRunner = true;
            }

            if(bCleanRunner == true)
            {
                run.DestroyRunnerExceptHitterRunner(); //타자주자 제외한 모든 주자 삭제
                int curIndex = (manager.bMyTurn ? 0 : 1);
                if (manager.Pvp_bOnBase[2] == true)
                {
                    CPlayer runner3 = SimulPlayerManager.GetFielder(curIndex, 2); 
                    run.makeChanceRunner(runner3, FieldParm.THIRDBASE_INDEX);
                }
                if (manager.Pvp_bOnBase[1] == true)
                {
                    CPlayer runner2 = SimulPlayerManager.GetFielder(curIndex, 1);
                    run.makeChanceRunner(runner2, FieldParm.SECONDBASE_INDEX);
                }
                if (manager.Pvp_bOnBase[0] == true)
                {
                    CPlayer runner1 = SimulPlayerManager.GetFielder(curIndex, 0);
                    run.makeChanceRunner(runner1, FieldParm.FIRSTBASE_INDEX);
                }

            }



            if (manager.nGameScore[0] != manager.Pvp_myScore)
            {
                Debug.Log("============>> 내 점수 오류 동기화");
                manager.nGameScore[0] = manager.Pvp_myScore;
            }

            if (manager.nGameScore[1] != manager.Pvp_otherScore)
            {
                Debug.Log("============>> 상대 점수 오류 동기화");
                manager.nGameScore[1] = manager.Pvp_otherScore;
            }

            if (manager.nOutCount != manager.Pvp_outCount)
            {
                Debug.Log("============>> 아웃카운트 오류 동기화");
                manager.nOutCount = manager.Pvp_outCount;
            }

            if(manager.bThreeOutChange != manager.Pvp_bThreeOut)
            {
                Debug.Log("============>> 공수교대 플래그 오류 동기화");
                manager.bThreeOutChange = manager.Pvp_bThreeOut;
            }

            if (manager.bGoodByeHitCall != manager.Pvp_bGoodBye)
            {
                Debug.Log("============>> 끝내기 플래그 오류 동기화");
                manager.bGoodByeHitCall = manager.Pvp_bGoodBye;
            }
        }


        //조건 검색 안하고 바로 배팅뷰로
        public IEnumerator updateFieldScene2(float delay)
        {
            //Debug.Log("=================================>>updateFieldScene2");
            yield return new WaitForSeconds(delay);
            //타자 대기 애니메이션 초기화 안되는 버그 관련
            batter.bBatterFieldUpdate = true;
            batter.bReadyAnim = false;

            if(Mode.bPvpMode == true)
            {
                PvpManager.bBattingResultUpdate = true;
            }

            manager.bStrike = manager.bBall = manager.bStrikeOut
                    = manager.bBaseOnBalls = false;

            batter.bHitted = false;
            batter.bHitChecked = false;

            getAddScore = lastAddScore = 0;
            lastPitcherAddScore = 0;
                        
            run.updateRunner();  //bRunnerUpdateFlag = true;// 
            judge.InitPosition();
            updateFielder();

            ball.bBallActive = false;
            ball.bStunBall = false;

            manager.saveGame1();
            setFieldRatio(FieldParm.InitRatio);//, false);
            //setZoom(1.0f);

            //카메라 트윈 제거
            UITweener tween = CameraManager.GetInstance().GetComponent<UITweener>();
            if (tween != null)
            {
                tween.enabled = false;
            }

            returnBattigView();
            manager.bThreeOutChange = false;

            manager.Pvp_FiendResultSync = false;
        }

        public void setFireWorkDeActive()
        {
            if (fireWorkObj.activeSelf) //if (fireWorkObj != null)
            {
                fireWorkObj.GetComponent<fireWork>().setBattingview();
                fireWorkObj.SetActive(false);// Destroy(fireWorkObj);
            }
        }
        
        //배팅뷰로 돌아감
        public void returnBattigView()
        {
            manager.bBattingPreUpdate = false;
            doublePlayCheckCount = 0;
            if (bHomerunCeremony == true)
            {
                setFireWorkDeActive();

                bHomerunCeremony = false;
                if (fireWorkView == CameraView.PitcherCenter)
                {
                    //다시 피칭뷰로 돌림                    
                    manager.battingview.settingView(fireWorkView);
                    if (batter.pAnim != null)
                    {
                        batter.pAnim.state.ClearTracks();
                        batter.readyAnim(true);
                        //pitcher.setPitcherReadyState()
                    }
                }
            }

            //batter.gameObject.SetActive(true);
            //for(int i=0;i<3;i++) CameraManager.SetScreenOverlay(i,false);
            //CameraManager.SetFocus(false);
            //yield return new WaitForSeconds(2.0f);
            //manager.gameUI.setScorePanel(false); //[UI]스코어 패널 디액티브

            
            returnCheckInit();
            //UnityEngine.Debug.Log("1. ==========================>> returnBattigView에서 changeBattingView()를 호출 한다");
            if (manager.changeBattingView() == true)
            {
                //UnityEngine.Debug.Log("2. ==========================>> returnBattigView에서 changeBattingView()를 호출 해서 TRUE인 경우");
                for (int i = 0; i < 9; i++) fielder[i].bObjectInit = false;                
                manager.returnBattingView();//임시
            }
            //연출관련 이전버전
            //fieldSkillFlag.Clear();

        }

        //야수 상태 업데이트
        private void updateFielder()
        {            
            for (int i = 0; i < 9; i++)
            {
                fielder[i].setUpdate(false);
            }
        }



        /////////////////////////////////////////////////////////////
        //FRAME 함수
        /////////////////////////////////////////////////////////////
        //메인 프레임 프레임
        public void nextFrame()
        {  
            nextZoom();
            returnBattingViewCheck();
            run.checkForcedOut();
            run.chekcOnRunning();
            curTime += getDeltaTime();//Time.deltaTime;

            /*
            if (bBlurSetting == true)
            {
                if (CameraManager.SetBlurSize2(blurDV * Time.deltaTime) == false)
                {
                    bBlurSetting = false;
                }
            }*/

        }


        //////////////////////////////////////////////////////////////////
        //필드의 각종값 얻어오는 함수
        //////////////////////////////////////////////////////////////////

        //현 필드뷰에서 스크린 위치 조정 함수
        public float getScreenX(float orginalX)
        {
            return orginalX;
        }

        //오리지널 좌표로부터 스크린 좌표 얻어옴 : X축
        public float getOriginX(float screenX)
        {
            return screenX;
        }

        //오리지널 좌표로부터 스크린 좌표 얻어옴 : Y축
        public float getScreenY(float orginalY)
        {
            return (orginalY * FieldParm.InitRatio);// FieldSize.getYAxisCoeff());

        }

        //오리지널 좌표로부터 스크린 좌표 얻어옴 : Y축
        public float getOriginY(float screenY)
        {
            return (screenY * FieldParm.InverseRatio);/// FieldParm.InitRatio);//   FieldSize.getYAxisCoeff(fRatio));
        }

        //해당 베이스에서 필더의 인덱스를 리턴하는 함수
        public int getBaseCoverIndex(int baseIndex)
        {

            for (int i = 0; i < 9; i++)
            {
                if (fielder[i].bBaseCovering == true)
                {
                    if (fielder[i].nCoveringIndex == baseIndex)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        //릴레이 포지션인 필더의 인덱스 리턴
        public int getRelayPositionIndex()
        {
            for (int i = 2; i < 6; i++)
            {
                if (fielder[i].bRelayPositioning == true || fielder[i].bRelayStart == true)
                {
                    return i;
                }
            }

            return -1;
        }

        public void setRelayOffsetMove(float x, float y)
        {
            for (int i = CPlayer._FIRSTBASEMAN; i < CPlayer._LEFTFIELDER; i++)
            {
                if (fielder[i].bRelayStart == true)
                {
                    float dstX = fielder[i].posX + x;
                    float dstY = fielder[i].posY + y;
                    fielder[i].setBackupPosition(dstX, dstY);
                }
            }
        }

        //강제!! 릴레이 포지션이 없는 경우 강제로 릴레이 인덱스 리턴
        public int getRelayIndex()
        {

            //임시 
            if (fielder[CPlayer._SHORTSTOP].bBaseCovering == true)
            {
                return CPlayer._SECONDBASEMAN;
            }
            else if (fielder[CPlayer._SECONDBASEMAN].bBaseCovering == true)
            {
                return CPlayer._SHORTSTOP;
            }
            else
            {
                if (ball.firstAngle > 0)
                {
                    return CPlayer._SHORTSTOP;
                }
                else
                {
                    return CPlayer._SECONDBASEMAN;
                }
            }
        }

        /*
        //외야에서 송과 좌표 구하기
        public void getOutFieldThrowingPosition()
        {
            throwingX = ball.nFirstBoundX;
            throwingY = ball.nFirstBoundY;
            for (int i = CPlayer._LEFTFIELDER; i <= CPlayer._RIGHTFIELDER; i++)
            {
                if (fielder[i].bGrounderAvail == true)
                {
                    throwingX = fielder[i].dstX;
                    throwingY = fielder[i].dstY;
                    return;
                }
            }
        }*/

        //가장 가까운 야수 인덱스 구하기
        public int getCloseFielderIndex()
        {
            int index = 0;
            float dis = 1000000000;
            float px, py;
            if (ball.bBound == true)
            {
                px = ball.nBallX;
                py = ball.nBallY;
            }
            else
            {
                px = ball.nFirstBoundX;
                py = ball.nFirstBoundY;
            }

            for (int i = 0; i < 9; i++)
            {
                float curDis = MyMath.getDistance(px, fielder[i].posX, py, fielder[i].posY);
                if (curDis < dis)
                {
                    dis = curDis;
                    index = i;
                }
            }

            return index;
        }


        public int getCloseFielderIndexError(int curFielder, bool bOnlyInfield, float px, float py)
        {
            int max = (bOnlyInfield ? CPlayer._SHORTSTOP : CPlayer._RIGHTFIELDER);            
            int index = 0;
            float dis = 1000000000;
            
            for (int i = 0; i <= max; i++)
            {
                if (i != curFielder)
                {
                    if (fielder[i].actState != FielderAction._COLLISION)
                    {
                        float curDis = MyMath.getDistance(px, fielder[i].posX, py, fielder[i].posY);
                        if (curDis < dis)
                        {
                            dis = curDis;
                            index = i;
                        }
                    }
                }
            }

            return index;
        }


        //////////////////////////////////////////////////////////////////
        //필딩시 (타구, 포볼, 도루, 이닝체인지) 각종 세팅
        //////////////////////////////////////////////////////////////////
        //필딩의 여러 플래그를 초기화
        private void setFieldFlagInit()
        {
            bVsShow = false;
            bCollisionFlag = false;
            bReturnException = false;
            bReturnCheck = false;

            bGrounderAvailble = false;
            timeScale = INIT_TIME_SCALE;

            bCatcherFieldChecked = false;
            bPitcherException = false;
            bAssist = false;
            bPutOut = false;
            bFirstThrow = false;
            nFirstThrower = -100;
            bOutByFlyball = false;
            //bGrounderCoverReady = false;
            //bGrounderCoverStart = false;
            bRelaying = false;
            bThrowing = false;
            bThrowBallCatched = false;
            bThrowZoom = false;
            bFieldZoom = false;
            //bBlur = false;
            bFieldPerspectiveZoom = true;
            nCatchIndex = -1;
            nRecheckTarget = -1;
            nCheckBaseNum = -1;
            nFirstThrowIndex = -1;
            flyCatchFielder = -1;
            groundCatchFielder = -1;
            nThrowIndex = -1;
            //groundCatchFielder2 = -1;
            doublePlayType = FieldParm.NOT_CHECKED;
            bGrounderSpecial = false;
            bFlyballSpecial = false;
            bSpecialMoveActivte = false;
            bBallTail = true;
            bDelayedCall = false;
            delayedCallTime = 0.01f;
            //bFieldEffectActive = false;
            bOutCalled = false;
            nRelayFielderIndex = -1;
            bFoulFlyOut = false;
            bMoreDouble = false;

            //nReThrowCheckIndex = -1;
            //bHomerunStealTry = false;
            //bThrowSlow = false;
            //specialIndex = -1;
            bErrorFlag = false;            
            errorType = FieldParm.ErrorType.None;
            bThrowErrorFlag = false;
            bInfieldThrowErrorFlag = false;
            
            bCollisionFlag = false;
            bFielderCrushEffect = false;

            bOutofInfield = false;
            bCrushDelay = false;
            bFieldDelayStealFlag = false;

            bInputWait = false;
            
            manager.fieldOutCountNum = 0;   //병살 카운트
            run.homeShobu = HomeShobu._NONE;

            groundingDstX = ball.nFirstBoundX;
            groundingDstY = ball.nFirstBoundY;

            bZoomCameraSetting = false;
            //
            returnCheckTime2 = 0.0f;


            bFieldVsSkillOffenseWin = false;

        }

        //필딩의 스킬 플래그를 초기화
        private void setSkillFlagInit()
        {            
            //UnityEngine.Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@==================>>필드에 발동된 스킬 플래그 초기화");
            //주자
            runnerHomeRush = FieldSkillUse.Init;
            runnerTurbo = FieldSkillUse.Init;
            runnerDPStop = FieldSkillUse.Init;
            //runnerDelaySteal = FieldSkillUse.Init;
            
            //투수
            //pitcherLaserPickoff = FieldSkillUse.Init;            
                        
            bFieldDelayStealFlag = false;
            bNoMoreHomeRushFlag = false;
            bRushCounterHappen = false;
            //bBlockBonusByLaser = false;

        }
        
        //필딩시 필딩캐릭터를 액티베이트
        public void setCharActive()
        {
            //필드 캐릭터를 액티베이트 시킨다
            ball.bBallActive = true;
            for (int i = 0; i < 9; i++)
            {
                fielder[i].setUpdate(true);
            }

            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    run.runner[i].bRunnerActive = true;
                    run.runner[i].curTime = 0;
                }
            }
        }

        //필더를 이닝 체인지시 벤치로 보낸다
        public void setBenchFielding()
        {
            ball.setDraw(false);// setActive(false);
            ball.setFirstBound(false);

            setCharActive();

            for (int i = 0; i < 9; i++)
            {
                fielder[i].setBench();
            }

            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    if (run.runner[i].checkActive() == true)//nState > RunningMechnism.GO_BENCH)
                    {
                        run.runner[i].setRunnerBench(false, false, false);//   .nState = Runner.RUNNER_GO_BENCH;
                    }
                }
            }
        }

        //도루나 견제시 베이스 커버
        public void setBaseCover_Steal_PickOff()
        {
            //1루
            fielder[CPlayer._FIRSTBASEMAN].posX = FieldSize.getFirstBasePosX() - 10;
            fielder[CPlayer._FIRSTBASEMAN].posY = FieldSize.getFirstBasePosY() / 0.6f + 20;
            fielder[CPlayer._FIRSTBASEMAN].setBaseCover(FieldParm.FIRSTBASE_INDEX);

            //3루
            fielder[CPlayer._THIRDBASEMAN].posX = FieldSize.getThirdBasePosX() + 10;
            fielder[CPlayer._THIRDBASEMAN].posY = FieldSize.getThirdBasePosY() / 0.6f + 20;
            fielder[CPlayer._THIRDBASEMAN].setBaseCover(FieldParm.THIRDBASE_INDEX);

            //2루
            if (batter.sign == 1 || run.bPickOff == true)
            {
                fielder[CPlayer._SECONDBASEMAN].posY = FieldSize.getSecondBasePosY() / 0.6f + 20;
                fielder[CPlayer._SECONDBASEMAN].posX = FieldSize.getSecondBasePosX() + 30;
                fielder[CPlayer._SECONDBASEMAN].setBaseCover(FieldParm.SECONDBASE_INDEX);
            }
            else
            {
                fielder[CPlayer._SHORTSTOP].posY = FieldSize.getSecondBasePosY() / 0.6f + 20;
                fielder[CPlayer._SHORTSTOP].posX = FieldSize.getSecondBasePosX() - 10;
                fielder[CPlayer._SHORTSTOP].setBaseCover(FieldParm.SECONDBASE_INDEX);
            }
        }

        //도루시 상태를 세팅
        public void setStealFielding()
        {
            //버그 방지용
            run.bRunnerWalk = false;
            run.bPickOff = false;
            //run.bStealBase = true;
            run.bWildPitchRunning = false;
            //
 
            pitcher.setPitchSystemDraw(false);
            ControlManager.EraseBattingUI();//manager.gameUI.eraseBattingUI(); //[UI]모든 배팅 UI 화면에서 지우기
            run.setRunnerCamera(false);

            setFieldFlagInit();
            // setSkillFlagInit();
            setCharActive();

            bFieldPickOffFlag = false;

            //도루나 견제시 베이스 커버 상태 설정
            setBaseCover_Steal_PickOff();

            if (run.bHomeSteal == true)
            {
                fielder[CPlayer._CATCHER].setBaseCover(FieldParm.HOMEBASE_INDEX);
                run.bBallOnBase[FieldParm.HOMEBASE_INDEX] = true;
            }
            else
            {
                fielder[CPlayer._CATCHER].setLongTagState();
                bStealThrow = true;
            }

            run.bForceOutFlag[FieldParm.SECONDBASE_INDEX] = false;

            ball.setFielderFocus(CPlayer._CATCHER);

            ball.nBallX = fielder[CPlayer._CATCHER].posX;// FieldSize.getMoundPosX();
            ball.nBallY = fielder[CPlayer._CATCHER].posY;// FieldSize.getMoundPosY(fRatio) + 430;
            ball.checkScroll();
            ball.gameObject.transform.localPosition = new Vector3(ball.screenX, ball.screenY, 0);


            if (run.stealResult == SimulStealState.Success || run.stealResult == SimulStealState.Success_Skill)
            {
                //도루 성공 세팅
            }
            else if (run.stealResult == SimulStealState.Fail || run.stealResult == SimulStealState.Fail_Skill)
            {
                //도루 실패 세팅
            }

            //포수의 송구 속도
            //주자의 속도

            run.setStealInit(); //도루 초기화

            run.setHitterRunnerStealPickOffSetting();//타자주자 세팅

        }

        //포볼시 상태 세팅
        public void setWalkFielding()
        {
            //버그 방지용
            //run.bRunnerWalk = true;
            run.bPickOff = false;
            run.bStealBase = false;
            run.bWildPitchRunning = false;
            //

            run.setStealInvalid();
            pitcher.setPitchSystemDraw(false);
            ControlManager.EraseBattingUI();////manager.gameUI.eraseBattingUI(); //[UI]모든배팅UI 화면에서 지우기
            run.setRunnerCamera(false);

            setFieldFlagInit();
            setCharActive();

            bFieldStealFlag = false;
            bFieldPickOffFlag = false;
            manager.bStealStrikeOut = false;
            run.bPickOff = false;
            run.bStealBase = false;


            ball.setFielderFocus(CPlayer._PITCHER);

            ball.nBallX = fielder[CPlayer._PITCHER].posX;// FieldSize.getMoundPosX();
            ball.nBallY = fielder[CPlayer._PITCHER].posY;// FieldSize.getMoundPosY(fRatio) + 430;
            ball.checkScroll();
            ball.gameObject.transform.localPosition = new Vector3(ball.screenX, ball.screenY, 0);

            for (int i = 0; i < 9; i++)
            {
                fielder[i].setDeadBall();
            }
            run.setRunnerWalkMove();
        }

        //견제상태 세팅
        private void setPickOffFielding()
        {
            for (int i = 0; i < 9; i++)
            {
                fielder[i].initSetting(i, false);
            }

            //버그 방지용
            run.bRunnerWalk = false;
            run.bStealBase = false;
            run.bWildPitchRunning = false;
            //

            run.setStealInvalid();
            pitcher.setPitchSystemDraw(false);
            ControlManager.EraseBattingUI();
            ControlBattingUI.SetActive(false, manager);
            IngameUI.GetPitchingSelect().SetActive(false);//, manager,false);            
            run.setRunnerCamera(false);

            setFieldFlagInit();
            setCharActive();

            bFieldStealFlag = false;

            //도루나 견제시 베이스 커버 상태 설정
            setBaseCover_Steal_PickOff();

            ball.setFielderFocus(CPlayer._PITCHER);

            ball.nBallX = fielder[CPlayer._PITCHER].posX;// FieldSize.getMoundPosX();
            ball.nBallY = fielder[CPlayer._PITCHER].posY;// FieldSize.getMoundPosY(fRatio) + 430;
            ball.checkScroll();
            ball.gameObject.transform.localPosition = new Vector3(ball.screenX, ball.screenY, 0);
            
            ////UnityEngine.//Debug.Log("============================>>>target = " + nTargetIndex);
            //투수세팅
            fielder[CPlayer._PITCHER].setPickOffState(nTargetIndex);
            run.runnerCheckPickOff();

            //포수 세팅
            fielder[CPlayer._CATCHER].setBaseCover(FieldParm.HOMEBASE_INDEX);

            //타자주자 세팅
            run.setHitterRunnerStealPickOffSetting();

            //픽오프 넘버
            pickOffCount++;

            //픽오프 초기화
            run.pickoffState = SimulPickOffState.NONE;
        }

        //와일드 피치 세팅
        public void setWildPitchFielding(bool bBlock)
        {
            //버그 방지용
            run.bRunnerWalk = false;
            run.bPickOff = false;
            run.bStealBase = false;
            //run.bWildPitchRunning = true;
            //

            run.setStealInvalid();
            pitcher.setPitchSystemDraw(false);
            ControlManager.EraseBattingUI();//manager.gameUI.eraseBattingUI(); //[UI]모든 배팅 UI 화면에서 지우기
            ControlBattingUI.SetActive(false, manager);
            IngameUI.GetPitchingSelect().SetActive(false);
            run.setRunnerCamera(false);

            setFieldFlagInit();
            setCharActive();

            bFieldStealFlag = false;

            //심판
            judge.setJudgeFieding(10);

            //야수상태
            if (bBlock == false && wildPitchCase != FieldParm.WildPitchCase.BaseOnBall)
            {
                //SimulManager.AddGameSummuryInfo("\n-투수 " + pitcher.pPitcher.getName()+"의 폭투");
                fielder[CPlayer._PITCHER].setBaseCover(FieldParm.HOMEBASE_INDEX);
            }
            fielder[CPlayer._FIRSTBASEMAN].setBaseCover(FieldParm.FIRSTBASE_INDEX);
            fielder[CPlayer._SHORTSTOP].setBaseCover(FieldParm.SECONDBASE_INDEX);
            fielder[CPlayer._THIRDBASEMAN].setBaseCover(FieldParm.THIRDBASE_INDEX);


            //주자 상태            
            run.runnerWildPitch(wildPitchCase, bBlock);
            //타자주자 세팅                
            run.setHitterRunnerWildPitchSetting(wildPitchCase, bBlock);
            //포수 세팅            
            float serchDelay = (bBlock == true ? 0.4f : 0.2f);
            StartCoroutine(fielder[CPlayer._CATCHER].setSerchBall(true, serchDelay, bBlock)); //와일드 피치 볼 찾기
        }

        //타구시 상태 세팅(중요)
        public void setFielding()
        {
            bFieldSyncCheck = true;

            Time.timeScale = 1.0f;
            curZoom = 1.0f;
            chanceResult = SimulResultState.Grounder; //찬스모드시 결과초기화
            setFieldFlagInit();
            setCharActive();

            bFieldStealFlag = false;
            bFieldPickOffFlag = false;
            manager.bStealStrikeOut = false;
            setBaseCoverInit();
            fastRemainTime = 10000;

            for (int i = 0; i < 9; i++)
            {
                fielder[i].setFieldingReady();
            }

            checkDistance();

            if (flyCatchAvaiableCount > 0)
            {
                if (ball.firstAngleZ > 25)
                {
                    bCollisionFlag = true;
                }

                if (ball.firstAngle > 45 || ball.firstAngle < -45)
                {
                    bFoulFlyOut = true;
                }

                //setSpecialFlyMoveInvalid();                
                //반드시 플라이볼을 잡는 경우 - 오판의 경우도 발생할 수 있다.
                float dis = 1000000;
                int index = -1;
                for (int i = 0; i < 9; i++)
                {
                    if (fielder[i].bFlyCatchAvail == true)// && fielder[i].flyballCatchType != FlyCatch.FLYCATCH_HOMERUNSTEAL)
                    {
                        if (fielder[i].distanceToBall < dis)
                        {
                            if (index != -1)
                            {
                                //////UnityEngine.//Debug.Log("==================>>bFlyCatchAvail invalid index = " + index);
                                fielder[index].bFlyCatchAvail = false;
                            }
                            dis = fielder[i].distanceToBall;
                            index = i;
                        }
                        else
                        {
                            fielder[i].bFlyCatchAvail = false;
                            //////UnityEngine.//Debug.Log("==================>>bFlyCatchAvail invalid index = " + i);
                            //fielder[i].bDeepFlyChase = true;
                        }
                    }

                }

                flyCatchFielder = index;

                ////////UnityEngine.//Debug.Log("==================>>#########index = " + index);

                for (int i = CPlayer._LEFTFIELDER; i < 9; i++)
                {
                    if (fielder[i].bFlyCatchAvail == false)
                    {
                        if (fielder[i].getGrounderDstPos() == false)
                        {
                            ////////UnityEngine.//Debug.Log("==================>>bFlyCoverChase setting index = " + i);

                            if (i == CPlayer._LEFTFIELDER)
                            {
                                if (ball.angle < 180)
                                {
                                    fielder[i].setBallChase();
                                    //fielder[i].bFlyCoverChase = true;
                                }
                            }
                            else if (i == CPlayer._CENTERFIELDER)
                            {
                                fielder[i].setBallChase();
                                //fielder[i].bFlyCoverChase = true;
                            }
                            else if (i == CPlayer._RIGHTFIELDER)
                            {
                                if (ball.angle > 180)
                                {
                                    fielder[i].setBallChase();
                                    //fielder[i].bFlyCoverChase = true;
                                }
                            }
                        }

                    }
                }
            }
            else
            {
                if (ball.firstAngle >= 45 || ball.firstAngle <= -45)
                {
                    ////UnityEngine.//Debug.Log("==========================================>>파울 콜");
                    float hookSlice = Random.Range(1.0f, 5.0f);
                    if (ball.firstAngle < 0) hookSlice = -hookSlice;
                    ball.setHookorSlice(true, hookSlice);

                    //float remainTime = Mathf.Clamp(ball.firstBoundTime, 0.5f, 2.0f);
                    //CameraManager.SetActiveCameraDstAngle(-15, remainTime);
                    CameraManager.SetActiveCameraInitAngle(-15);
                    setZoomTo(0.8f, Mathf.Clamp(ball.firstBoundTime, 0.5f, 1.0f));

                    return;
                }

                //반드시 그라운드 볼이 나오는 경우
                if (grounder == true)
                {
                    int count = 0;
                    int outFieldCount = 0;

                    if (firstBallSpeed < 250.0f)
                    {
                        //포수 수비 세팅
                        fielder[CPlayer._CATCHER].FIELD_DELAY = 0.2f;
                        fielder[CPlayer._CATCHER].grounderCatchType = GrounderCatch.GROUNDERCATCH_DASH_FIRST;
                        setFastFieldTime(0.2f);
                        groundCatchFielder = CPlayer._CATCHER;
                        count = 1;
                    }
                    else
                    {
                        
                        for (int i = 0; i < 9; i++)
                        {
                            if (fielder[i].getGrounderDstPos())
                            {
                                if (i < CPlayer._LEFTFIELDER)
                                {
                                    //nCurrentGroundFielder = i;
                                    groundCatchFielder = i;
                                    count++;
                                }
                                else
                                {
                                    outFieldCount++;
                                }
                            }
                        }
                    }

                    if (count > 0)
                    {
                        ball.setHookorSlice(false, 0); //인필드에서 그라운더 발생시 훅 무효화      
                        //평범하게 잡는 놈...
                        bGrounderAvailble = true;
                        //setSpecialGrounderMoveInvalid();
                        //////UnityEngine.//Debug.Log("============================>>땅볼 수비 count = " + count);
                        if (count >= 2)  //원래 count>2였는데 혹시 모르니 체크
                        {
                            if (bPitcherException == true)
                            {
                                setPitcherException();
                            }
                            else
                            {
                                setGrounderReform();
                            }
                        }
                    }
                    else
                    {
                        bOutofInfield = true;
                        bGrounderAvailble = false;
                        //setGrounderTry();
                    }

                    if (outFieldCount > 0)
                    {
                        ball.setHookorSlice(false, 0); //아웃필드에서 그라운더 발생시 훅 무효화      
                    }
                    else
                    {
                        bMoreDouble = true;
                    }
                    ///bGrounderAvailble = bCheckGrounderAvail();
                    //if (bGrounderAvailble == false) setGrounderTry();
                }
                else
                {
                    bOutofInfield = true;
                    setFlyCatchReform();
                }
            }

            if (flyCatchAvaiableCount > 0 || groundCatchFielder == -1)
            {
                //공이 외야로 나가거나 플라이볼인경우
                CameraManager.SetActiveCameraInitAngle(-25);
                float remainTime2 = Mathf.Clamp(ball.firstBoundTime, 0.7f, 1.5f);
                if (Mathf.Abs(ball.firstAngle) > 30) remainTime2 *= 0.5f;
                CameraManager.SetActiveCameraDstAngle(-15, remainTime2);
                setZoomTo(0.7f, remainTime2);
            }
            else
            {
                //그외.. 땅볼
                setZoom(1.2f);
                float remainTime2 = Mathf.Clamp(fastRemainTime, 0.2f, 1.0f);
                /*if (ball.firstAngleZ > -20 && ballPower > 20)
                {
                    CameraManager.SetActiveCameraInitAngle(-25);
                    CameraManager.SetActiveCameraDstAngle(-15, remainTime2);
                    setZoomTo(0.9f, remainTime2);
                }
                else
                {
                    CameraManager.SetActiveCameraInitAngle(-30);
                    CameraManager.SetActiveCameraDstAngle(-25, remainTime2);
                }*/
                if (groundCatchFielder <= CPlayer._CATCHER)
                {
                    CameraManager.SetActiveCameraInitAngle(-25);
                    setZoomTo(1.4f, 0.5f);
                }
                else
                {
                    CameraManager.SetActiveCameraInitAngle(-30);
                    CameraManager.SetActiveCameraDstAngle(-25, remainTime2);
                }
                
            }

            //UnityEngine.//Debug.Log("=====================================>> 장타여부 =" + bMoreDouble);

            //fastRemainTime = fastRemainTime * 0.75f;


            //베이스 커버
            checkCover();

            //ball.setCameraOffset(false);
            //bFiedZoomStep = 0;
            //UnityEngine.Debug.Log("###############################################>>bGrounderAvailble = " + bGrounderAvailble + " ball.angleZ = " + ball.angleZ);

            setFieldRatio(FieldParm.InitRatio);
            
            curTime = 0;

            ball.setBallRotation(ball.nBallDX, ball.nBallDY, false, (ball.firstAngle>25?true:false));

            //투수 액티브 스킬이 필드에 미치는 영향
            //checkPitcherActiveSkillOnField();

            if (ball.bHomeRunGuess == true && bHomerunStealTry == false)
            {
                ////UnityEngine.//Debug.Log("================>>홈런 콜 체크 bHomerunStealTry = " + bHomerunStealTry);
                StartCoroutine(homerunDelay());
            }

            /*
            if (Mode.bAutoPlay == true)
            {
                //자동 플레이시
                setTimeScale(DECISIVE_TIME_SCALE);
            }*/
        }


        //홈런 콜 딜레이
        private IEnumerator homerunDelay()
        {
            yield return new WaitForSeconds(1.00f);
            //홈런 사운드
            judge.setCall(0, CallType._HOMERUN);
        }

        //////////////////////////////////////////////////////////////////
        //수비(FIELDING) 관련 메쏘드
        //////////////////////////////////////////////////////////////////    
        //공을 잡은후 이후 결과를 처리하는 함수
        public void setCatched(int myIndex)
        {
            for (int i = 0; i < 9; i++)
            {
                if (i != myIndex)
                {
                    if (fielder[i].actState == FielderAction._FIELDING)
                    {
                        fielder[i].setStop();
                    }
                }
            }
        }

        //베이스 커버 스테이트를 초기화
        public void setBaseCoverInit()
        {
            for (int i = 0; i < 4; i++)
            {
                bBaseCoverd[i] = false;

                if (run.runnerActive[i] == true)
                {
                    run.runner[i].setRunnerSkipMove();
                }
            }
        }

        //해당 베이스가 커버된 상태
        public void setBaseCover(int baseIndex)
        {
            bBaseCoverd[baseIndex] = true;
        }

        //플라이 아웃 처리
        public void setFlyOut()
        {
            ball.setParticleDraw(false);
            bOutByFlyball = true;
            //러너 처리
            run.setRunnerAfterFlyCatch();
            bPutOut = true;

            //공을 잡는 야수 자살 카운트 업

            setOutCondition(true);
        }

        //그라운드 아웃처리
        public void setGroundOut()
        {
            //UnityEngine.Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%SET GROUND OUT!!!!!!!!!!!! nTargetIndex" + nTargetIndex);
            bOutByFlyball = false;
            bAssist = true;

            //공을 잡는 야수 자살 카운트 업
            //공을 던지는 야수 보살 카운트 업



            setOutCondition();
        }


        public void setThrowAvailabe()
        {
            for (int i = 0; i < 9; i++)
            {
                fielder[i].throwAgainState = FieldParm.ThrowAgain.Available;
            }
        }


        //보살 아웃 처리
        public void setAssistedOut(bool bTaged)
        {

            //던지는놈 보살카운트
            //받는놈 자살카운트
        }

        //필드에서의 아웃 조건
        public void setOutCondition(bool bFly = false, bool bBuntOut = false)
        {
            ////UnityEngine.//Debug.Log("=============================>>SetOutCondition");
            int offsetY = 0;
            bOutCalled = true;

            //아웃 사운드
            soundmanager.Get().PlaySound(soundmanager.SoundID.OutCall);

            if (bBuntOut == true)
            {
                manager.strFieldOutType = "쓰리번트";
                int index = FieldParm.FIRSTBASE_INDEX;
                if (ball.firstAngle > 0) index = FieldParm.THIRDBASE_INDEX;
                judge.setCall(index, CallType._OUT);
                manager.setOutFlag(BallPlayManager._K_FLAG | BallPlayManager._SO_FLAG); //쓰리번트 아웃인 경우 삼진 처리
            }
            else
            {

                if (bFly == true)
                {
                    ////UnityEngine.//Debug.Log("===========>>자살 인덱스:flyballCatchIndex = " + flyCatchFielder);
                    doublePlayCheckCount = 0;
                    judge.setCall(flyCatchFielder, CallType._FLYOUT);
                    StartCoroutine(callUI("out", offsetY));
                    if (flyCatchFielder != -1)
                    {
                        manager.poIndex = flyCatchFielder;      //오직 자살처리
                        manager.setOutFlag(BallPlayManager._PO_FLAG);
                        manager.strFieldOutType = Util.GetPositionString(flyCatchFielder)+" 뜬공";
                    }
                    else
                    {
                        manager.strFieldOutType = "뜬공";
                    }
                }
                else
                {
                    doublePlayCheckCount++;
                    int index = run.getCurBallBase();
                    if (index == -1) index = nTargetIndex;
                    judge.setCall(index, CallType._OUT);

                    ////UnityEngine.//Debug.Log("==============>>index/catchIndex " + index + "/" + nCatchIndex);
                    //각종 아웃의 특수 애니메이션 처리
                    if (nCatchIndex == CPlayer._CATCHER && index == FieldParm.HOMEBASE_INDEX)
                    {
                        ////UnityEngine.//Debug.Log("==============>>태그 승리 애니메이션 호출");
                        fielder[nCatchIndex].afterTagAnim();
                    }

                    if (nTargetIndex != -1)
                    {
                        manager.poIndex = getBaseCoverIndex(nTargetIndex);  //자살은 잡는놈
                    }
                    else
                    {
                        manager.poIndex = getBaseCoverIndex(fielder[nCatchIndex].nCoveringIndex);
                        ////UnityEngine.//Debug.Log("============>>자살 인덱스가 여기 걸리는 경우: manager.poIndex = " + manager.poIndex);
                    }
                    manager.aoIndex = curThrowIndex;                    //보살은 던지는놈

                    if (bFieldStealFlag == true)
                    {
                        //도루자 관련 아웃플래그 세팅
                        if (run.bHomeSteal == true)
                        {
                            manager.setOutFlag(BallPlayManager._PO_FLAG); //홈스틸시 자살만
                            manager.sbfIndex = run.getDestRunner(FieldParm.HOMEBASE_INDEX).arrayIndex; //run.getRunnerDestIndex(FieldParm.HOMEBASE_INDEX);
                            manager.setOutFlag(BallPlayManager._SBF_FLAG); //도루실패만
                        }
                        else
                        {
                            manager.setOutFlag(BallPlayManager._PO_FLAG | BallPlayManager._A_FLAG); //보살은 자살을 반드시 동반
                            manager.sbfIndex = run.getDestRunner(nTargetIndex).arrayIndex;
                            manager.setOutFlag(BallPlayManager._SBF_FLAG | BallPlayManager._CS_FLAG); //도루저지, 도루실패
                        }
                    }
                    else
                    {
                        manager.setOutFlag(BallPlayManager._PO_FLAG | BallPlayManager._A_FLAG); //보살은 자살을 반드시 동반
                        if (bFieldDelayStealFlag == true)
                        {
                            ////UnityEngine.//Debug.Log("============>>딜레이 스틸은 여기 걸리는가");
                        }
                        else
                        {
                            if (groundCatchFielder != -1)
                            {
                                manager.strFieldOutType = Util.GetPositionString(groundCatchFielder) + " 땅볼";
                            }
                        }
                    }
                }
            }

            if (bFieldPickOffFlag == true || bFieldStealFlag == true)
            {
                returnCheck_Steal_Pickoff(-1);
            }
            manager.addOutCount();
        }
                

        //콜 관련 UI
        public int doublePlayCheckCount = 0;
        public IEnumerator callUI(string call, float offsetY)
        {
            yield return new WaitForSeconds(delayedCallTime);
            IngameUI.GetFieldCall().Call("out", doublePlayCheckCount);
            delayedCallTime = 0.01f;
            bDelayedCall = false;
            if (bReturnBattingView == true)
            {
                returnCheckNC(-1);
            }
        }

        //fastRemainTime 구하는 함수(가장 가까운 야수를 구하기 위해 필요함)
        public void setFastFieldTime(float curFieldTime)
        {
            bFieldZoom = true;
            if (curFieldTime < fastRemainTime)
            {
                fastRemainTime = curFieldTime;
            }
        }

        //피처 익셉션 처리 - 투수가 처리할수 없는 영역을 설정
        private void setPitcherException()
        {
            if (ball.firstAngle > 0)
            {
                if (ball.firstAngle > 20)
                {
                    fielder[CPlayer._THIRDBASEMAN].bGrounderAvail = true;
                    fielder[CPlayer._SHORTSTOP].bGrounderAvail = false;
                }
                else
                {
                    fielder[CPlayer._THIRDBASEMAN].bGrounderAvail = false;
                    fielder[CPlayer._SHORTSTOP].bGrounderAvail = true;
                }
            }
            else
            {
                if (ball.firstAngle < -20)
                {
                    fielder[CPlayer._FIRSTBASEMAN].bGrounderAvail = true;
                    fielder[CPlayer._SECONDBASEMAN].bGrounderAvail = false;
                }
                else
                {
                    fielder[CPlayer._FIRSTBASEMAN].bGrounderAvail = false;
                    fielder[CPlayer._SECONDBASEMAN].bGrounderAvail = true;
                }
            }
            groundCatchFielder = 0;
        }

        //그라운더 조건 리폼 : 가중치에 따른 재 정비
        private void setGrounderReform()
        {
            ////UnityEngine.//Debug.Log("============================>>setGrounderReform");
            /*if (ball.firstAngleZ > -20)
            {
                float dis = 1000000;
                int index = -1;
                for (int i = 0; i < CPlayer._SHORTSTOP; i++)
                {
                    ////////UnityEngine.//Debug.Log("============================>>i = " + i + "avail = " + fielder[i].bGrounderAvail);
                    if (fielder[i].bGrounderAvail == true)
                    {
                        float curDis = fielder[i].distanceToBall;
                        if (curDis < dis)
                        {
                            if (index != -1)
                            {
                                ////UnityEngine.//Debug.Log("============================>>i = "+ index);
                                fielder[index].bGrounderAvail = false;
                            }
                            dis = curDis;// fielder[i].distanceToBall;
                            ////UnityEngine.//Debug.Log("============================>>i = " + index);
                            index = i;
                        }
                        else
                        {
                            fielder[i].bGrounderAvail = false;
                        }
                    }
                }
                groundCatchFielder = index;
            }*/
            //groundCatchFielder2 = index;
            for (int i = 0; i < CPlayer._SHORTSTOP; i++)
            {
                if (fielder[i].bGrounderAvail == true)
                {
                    groundCatchFielder = i;
                    break;
                }
            }
        }

        //플라이볼 조건 리폼 : 가중치에 따른 재 정비
        private void setFlyCatchReform()
        {
            for (int i = CPlayer._LEFTFIELDER; i < 9; i++)
            {
                //안걸리는 경우가 있음..
                if (fielder[i].bDeepFlyChase == false && fielder[i].flyballCatchType == FlyCatch.FLYCATCH_NORMAL)
                {
                    ////////UnityEngine.//Debug.Log("===============>>>여기여기여기 index = "+i);
                    if (fielder[i].bOverHead == true) //if (fielder[i].checkDeepFly() == true)
                    {
                        if (i == CPlayer._CENTERFIELDER
                         || (i == CPlayer._LEFTFIELDER && ball.angle < 180)
                         || (i == CPlayer._RIGHTFIELDER && ball.angle > 180))
                        {
                            fielder[i].bDeepFlyChase = true;
                            bMoreDouble = true;
                        }
                    }
                    else
                    {
                        if (fielder[i].getGrounderDstPos() == false)
                        {
                            bMoreDouble = true;
                        }
                    }
                }
            }
            //bGrounderAvailble = bCheckGrounderAvail();
            //if (bGrounderAvailble==true) setSpecialGrounderMoveInvalid();
        }

        //포수 필딩 여부 체크
        public void checkCatcherFielding(float ballSpeed)
        {
            if (bCatcherFieldChecked == false)
            {
                if (earlygrounder == true)
                {
                    if (fielder[CPlayer._CATCHER].bGrounderAvail == false)
                    {
                        if (ball.bBallCatched == false)
                        {
                            if (ballSpeed < 100)//60)
                            {
                                bCatcherFieldChecked = true;

                                //캐처의 필딩여부 체크        
                                float bX = ball.nBallX;
                                float bY = ball.nBallY;
                                float disP = FieldingMechanism.getDistance(fielder[0].posX, bX, fielder[0].posY, bY);// fielder[CPlayer._PITCHER].getDistance(bX, bY);
                                float disC = FieldingMechanism.getDistance(fielder[1].posX, bX, fielder[1].posY, bY);//fielder[CPlayer._CATCHER].getDistance(bX, bY);
                                float dis1 = FieldingMechanism.getDistance(fielder[2].posX, bX, fielder[2].posY, bY);//fielder[CPlayer._FIRSTBASEMAN].getDistance(bX, bY);
                                float dis3 = FieldingMechanism.getDistance(fielder[4].posX, bX, fielder[4].posY, bY);//fielder[CPlayer._THIRDBASEMAN].getDistance(bX, bY);
                                if (disC < disP && disC < dis1 && disC < dis3)
                                {
                                    //////UnityEngine.//Debug.Log("========================>>캐처 필딩 체크");
                                    fielder[CPlayer._CATCHER].setBallChase();
                                    fielder[CPlayer._PITCHER].setBaseCover(FieldParm.HOMEBASE_INDEX);

                                    if (fielder[CPlayer._FIRSTBASEMAN].bBaseCovering == false)
                                    {
                                        fielder[CPlayer._FIRSTBASEMAN].setBaseCover(FieldParm.FIRSTBASE_INDEX);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //가장 가까운 야수 필딩 액티브 체크
        public void checkNearFielderActive()
        {
            if (ball.bHomeRunCall == false)
            {
                //////UnityEngine.//Debug.Log("=========================>>checkNearFielderActive");
                //가장 가까운 필더 액티브 시키기
                float bx = ball.nBallX;
                float by = ball.nBallY;

                int fielderIndex = 0;
                float dis = 1000000;

                for (int i = 0; i < 9; i++)
                {
                    if (fielder[i].actState != FielderAction._COLLISION && fielder[i].actState != FielderAction._ERROR_PANIC)
                    {
                        float d = FieldingMechanism.getDistance(fielder[i].posX, bx, fielder[i].posY, by);// fielder[i].getDistance(bx, by);
                        if (d < dis)
                        {
                            dis = d;
                            fielderIndex = i;
                        }
                    }
                }

                fielder[fielderIndex].setBallChase();
            }

        }

        //더블 플레이 타입 체크
        public int checkDoublePlayType(int posIndex)
        {
            //int startIndex = nThrowIndex;
            //int curIndex = index;

            if (posIndex < CPlayer._LEFTFIELDER)
            {
                /*    //////UnityEngine.//Debug.Log("==================>>nTargetIndex = " + nTargetIndex);
                    //////UnityEngine.//Debug.Log("==================>>bOnRunning[FieldParm.SECONDBASE_INDEX] = " + run.bOnRunning[FieldParm.SECONDBASE_INDEX] + "=====>forceOut = " + run.bForceOutFlag[FieldParm.SECONDBASE_INDEX]);
                    //////UnityEngine.//Debug.Log("==================>>bOnRunning[FieldParm.THIRDBASE_INDEX] = " + run.bOnRunning[FieldParm.THIRDBASE_INDEX] + "=====>forceOut = " + run.bForceOutFlag[FieldParm.THIRDBASE_INDEX]);
                    //////UnityEngine.//Debug.Log("==================>>bOnRunning[FieldParm.HOMEBASE_INDEX] = " + run.bOnRunning[FieldParm.HOMEBASE_INDEX] + "=====>forceOut = " + run.bForceOutFlag[FieldParm.HOMEBASE_INDEX]);
                */
                /*
                 * public const int NO_DOUBLEPLAY = -1,
                      DOUBLEPLAY_163 = 0,   //투->유(이)->일
                      DOUBLEPLAY_153 = 1,   //투->삼->일
                      DOUBLEPLAY_123 = 2,   //투->포->일
                      DOUBLEPLAY_263 = 10,  //포->유->일
                      DOUBLEPLAY_253 = 11,  //포->삼->일
                      DOUBLEPLAY_361 = 20,  //일->유->투
                      DOUBLEPLAY_351 = 21,  //일->삼->투   (대희박)
                      DOUBLEPLAY_321 = 22,  //일->포->투
                      DOUBLEPLAY_463 = 30,  //이->유->일
                      DOUBLEPLAY_453 = 31,  //이->삼->일   (대희박)
                      DOUBLEPLAY_421 = 32,  //이->포->투   (희박)
                      DOUBLEPLAY_543 = 40,  //삼->이->일                
                      DOUBLEPLAY_553 = 41,  //삼->삼찍고->일   (멋있음 구현하고 싶어)
                      DOUBLEPLAY_523 = 42,  //삼->포->일
                      DOUBLEPLAY_643 = 50,  //유->이->일
                      DOUBLEPLAY_653 = 51,  //유->이->일   (대희박)
                      DOUBLEPLAY_621 = 52,  //유->포->투   (희박)
                      DOUBLEPLAY_ETC = 100,
                      NOT_CHECKED = -2;*/

                if (nTargetIndex == FieldParm.SECONDBASE_INDEX)
                {
                    if (run.bOnRunning[FieldParm.SECONDBASE_INDEX] == true)
                    {
                        ////UnityEngine.//Debug.Log("==================>>2루찍고 병살시도");
                        ////UnityEngine.//Debug.Log("============================>> doublePlayType = " + (nFirstThrowIndex * 10));
                        return (nFirstThrowIndex * 10);
                    }
                }
                else if (nTargetIndex == FieldParm.THIRDBASE_INDEX)
                {
                    if (run.bOnRunning[FieldParm.THIRDBASE_INDEX] == true && run.bForceOutFlag[FieldParm.THIRDBASE_INDEX] == true)
                    {
                        //1 2루인 경우 삼루찍고 병살(2 혹은 1루)
                        ////////UnityEngine.//Debug.Log("==================>>3루찍고 병살시도");
                        ////////UnityEngine.//Debug.Log("============================>> doublePlayType = " + ((nFirstThrowIndex * 10) + 1));
                        return (nFirstThrowIndex * 10) + 1;
                    }
                }
                else if (nTargetIndex == FieldParm.HOMEBASE_INDEX)
                {
                    if (run.bOnRunning[FieldParm.HOMEBASE_INDEX] == true && run.bForceOutFlag[FieldParm.HOMEBASE_INDEX] == true)
                    {
                        //만루인경우 홈찍고 병살
                        ////////UnityEngine.//Debug.Log("==================>>홈 찍고 병살시도");
                        ////////UnityEngine.//Debug.Log("============================>> doublePlayType = " + ((nFirstThrowIndex * 10) + 2));
                        return (nFirstThrowIndex * 10) + 2;
                    }
                }
            }
            ////////UnityEngine.//Debug.Log("============================>> doublePlayType = NO_DOUBLEPLAY");
            return FieldParm.NO_DOUBLEPLAY;
        }

        //베이스 커버가 필요한지 여부 체크
        public bool checkCoverNeeded(int baseIndex, int posIndex)
        {
            bool bNeeded = false;
            if (bBaseCoverd[baseIndex] == false)
            {
                bNeeded = true;
                for (int i = 0; i < CPlayer._LEFTFIELDER; i++)
                {
                    if (i != posIndex)
                    {
                        if (fielder[i].nCoveringIndex == baseIndex)
                        {
                            if (fielder[i].bBaseCovering == true)
                            {
                                bNeeded = false;
                            }
                        }
                    }
                }
            }
            else
            {
                bNeeded = false;
            }
            ////////UnityEngine.//Debug.Log("=====================>> checkCoverNeeded posIndex = " + posIndex + " ==>> " + bNeeded);
            return bNeeded;
        }

        //릴레이가 필요한지 여부 체크
        public bool checkRelayNeeded()
        {
            if (getRelayPositionIndex() == -1)
            {
                ////////UnityEngine.//Debug.Log("=====================>> checkRelayNeeded TRUE!!! posIndex = " + posIndex);
                return true;
            }
            ////////UnityEngine.//Debug.Log("=====================>> checkRelayNeeded!! FALSE!!! posIndex = "+posIndex);
            return false;
        }


        //플라이 캐치가 가능한 야수의 수 카운트 : 0보다 큰경우 에러가 발생하지 않으면 무조건 플라이 아웃
        public void checkFlyCatchCount()
        {
            flyCatchAvaiableCount = 0;

            for (int i = 0; i < 9; i++)
            {
                if (fielder[i].checkFlyCatchAvailable() == true)
                {
                    ////////UnityEngine.//Debug.Log("=============>>avaible fielder index = " + i);
                    flyCatchAvaiableCount++;
                }
            }
        }

        //볼의 비거리와 타구의 타입을 체크
        private void checkDistance()
        {
            earlygrounder = false;
            grounder = false; //내야수 한명 움직임 , 외야수 포지션 움직임            
            infieldFlyOut = false;
            shallowFlyOut = false;
            deepFlyOut = false;

            float a = ball.nFirstBoundX - homeX;
            float b = ball.nFirstBoundY;
            if (b > homeY)
            {
                //앞으로 가는 타구
                float dis = Mathf.Sqrt(a * a + b * b);
                float infieldDis = getOriginY(FieldSize.getInfieldFlyDistance());
                if (dis < infieldDis)
                {
                    if (flyCatchAvaiableCount > 0)
                    {
                        infieldFlyOut = true;
                    }
                    else
                    {
                        grounder = true;
                        if (dis < getOriginY(FieldSize.getEarlyGrounderDistance()))
                        {
                            earlygrounder = true;
                        }
                    }
                }
                else
                {
                    if (checkDeepFly() == false)//dis < SHALLOW_OUTFIED_FLY_DISTANCE)
                    {
                        if (flyCatchAvaiableCount > 0)
                        {
                            // //////UnityEngine.//Debug.Log("==================>> SHALLOW FLY OUT");
                            shallowFlyOut = true;
                        }
                    }
                    else
                    {
                        if (flyCatchAvaiableCount > 0)
                        {
                            ////////UnityEngine.//Debug.Log("==================>> DEEP FLY OUT");
                            deepFlyOut = true;
                        }
                    }
                }
            }

        }

        //딥 플라이 여부 체크
        private bool checkDeepFly()
        {
            //외야 딥플라이
            for (int i = CPlayer._LEFTFIELDER; i < 9; i++)
            {
                if (fielder[i].bOverHead == true)
                {
                    return true;
                }
            }
            return false;
        }

        //외야수가 땅볼 처리 하는 여부 체크
        public bool checkOutFielderGrounderAvail()
        {
            for (int i = CPlayer._LEFTFIELDER; i <= CPlayer._RIGHTFIELDER; i++)
            {
                if (fielder[i].bGrounderAvail == true) return true;
            }
            return false;
        }

        //야수의 커버 처리
        public void checkCover()
        {
            for (int i = 0; i < 9; i++)
            {
                fielder[i].getBackupPos();
            }
        }


        //////////////////////////////////////////////////////////////////
        //필딩 에러 관련 메쏘드
        //////////////////////////////////////////////////////////////////    
        //필딩 에러 세팅 - initBatter에서 호출
        public void setCatchError()
        {
            //PVP모드에서 타자정보를 수신해야하는 투수컨트롤 부분에서는 이부븐을 스킵한다.
            //if (Mode.bPvpMode == true && manager.bMyTurn == false) return;
            //Debug.Log("=================================================>>포구 에러 계산");
#if _Local_Balance
            /*if (InGameDebug._ALWAYS_CATCH_ERROR == true)
            {
                //일부로 포구에러 발생
                for (int i = 0; i < 9; i++)
                {
                    fielder[i].bCatchErrorSpeicalAnimation = false;
                    fielder[i].bCatchErrorFlag = true;// (MyMath.Percent() < 70 ? true:false);
                }
            }
            else*/
#endif
            {
                //Debug.Log("================>> error check!!!");
                if (manager.nInningCount == 1 && manager.nOutCount == 0) return; //1회 노아웃 예외처리
                for (int i = 0; i < 9; i++)
                {
                    fielder[i].bCatchErrorSpeicalAnimation = false;
                    int abil = (i==CPlayer._PITCHER?800:fielder[i].fieldingAbil);
                    bool bInfielder = (i < CPlayer._LEFTFIELDER ? true : false);
                    fielder[i].bCatchErrorFlag = SimulParm.checkCatchError(abil, bInfielder); 
                }
            }

        }

        //필딩 에러 초기화 - 필딩이 일어나면 호출한다
        public void setCatchErrorInit()
        {
            for (int i = 0; i < 9; i++)
            {
                fielder[i].bCatchErrorFlag = false;
            }
        }



        //////////////////////////////////////////////////////////////////
        //송구(THROWING) 관련 메쏘드
        //////////////////////////////////////////////////////////////////
        //송구시 걸리는 총시간 구하기
        public int throwFielder;
        public float getThrowingTotalTime(int nextBase)
        {
            ////UnityEngine.Debug.Log("########################################### next base: "+nextBase);

            //field.nRelayFielderIndex
            //field.nTargetIndex
            //nThrowIndex

            float nextX = getOriginX(FieldSize.getBasePosX(nextBase));
            float nextY = getOriginY(FieldSize.getBasePosY(nextBase));

            //int throwFielder;


            float delayTime = 0;
            float throwingTime = 0;
            float relayingTime = 0;
            float distanceTime = 0;
            float fenceDelay = (ball.bFenceCol ? FieldParm.FENCE_DELAY : 0) + (bCrushDelay ? FieldParm.FENCE_DELAY : 0);

            if (bRelaying == true)
            {
                ////UnityEngine.//Debug.Log("===================>> bRelaying!!!  nThrowIndex = " + nThrowIndex);
                relayingTime = (MyMath.getDistance(ball.nBallX, fielder[nRelayFielderIndex].posX, ball.nBallY, fielder[nRelayFielderIndex].posY) / fielder[nRelayFielderIndex].THROW_SPEED);
                delayTime = FieldParm.RELAY_DELAY;//릴레이 딜레이 fielder[nRelayFielderIndex].THROW_DELAY + 0.4f; //릴레이 딜레이!!
                throwingTime = (MyMath.getDistance(fielder[nRelayFielderIndex].posX, nextX, fielder[nRelayFielderIndex].posY, nextY) / fielder[nRelayFielderIndex].THROW_SPEED);
            }

            else if (bThrowing == true)
            {
                ////UnityEngine.//Debug.Log("===================>> bThrowing!!! nThrowIndex = " + nThrowIndex);
                relayingTime = 0;
                delayTime = 0;

                float throwSpeed;
                if (nThrowIndex != -1) throwSpeed = fielder[nThrowIndex].THROW_SPEED;
                else throwSpeed = fielder[0].THROW_SPEED;
                throwingTime = (MyMath.getDistance(ball.nBallX, nextX, ball.nBallY, nextY) / throwSpeed);
            }
            else
            {                
                relayingTime = 0;
                if (ball.step == BallStep.BALL_HIT)
                {
                    //캐칭 딜레이 추가...
                    throwFielder = getCloseFielderIndex();
                    ////UnityEngine.//Debug.Log("===================>> 아직 볼 못잡음!!! throwFielder = " + throwFielder);
                    delayTime = fielder[throwFielder].THROW_DELAY + fielder[throwFielder].getCatchingDelay(false); //캐칭 딜레이.. 
                    throwingTime = (MyMath.getDistance(ball.nBallX, nextX, ball.nBallY, nextY) / fielder[throwFielder].THROW_SPEED);
                    distanceTime = (MyMath.getDistance(fielder[throwFielder].posX, ball.nBallX, fielder[throwFielder].posY, ball.nBallY) / fielder[throwFielder].FIELDER_SPEED);
                    if (fielder[throwFielder].actState != FielderAction._CATCHING)
                    {
                        throwingTime *= 1.2f;
                        distanceTime *= 2;
                    }

                    ////UnityEngine.//Debug.Log("===================>> throwingTime: " + throwingTime + "===>>THROW_SPEED: " + fielder[throwFielder].THROW_SPEED);

                }
                else
                {
                    //캐칭 딜레이 추가... 매우 중요
                    if (nThrowIndex >= 0)
                    {
                        throwFielder = nThrowIndex;
                    }
                    else
                    {
                        throwFielder = getCloseFielderIndex();
                    }

                    ////UnityEngine.//Debug.Log("===================>> 볼을 캐치중!!!! throwFielder = " + throwFielder);
                    delayTime = fielder[throwFielder].THROW_DELAY + (fielder[throwFielder].getCatchingDelay(false) * 0.7f); //캐칭 딜레이.. 어떤 캐칭이냐에 따라..
                    throwingTime = (MyMath.getDistance(fielder[throwFielder].posX, nextX, fielder[throwFielder].posY, nextY) / fielder[throwFielder].THROW_SPEED);
                }

            }


            ////////UnityEngine.//Debug.Log("========================>>relayingTime = " + relayingTime);
            ////////UnityEngine.//Debug.Log("========================>>delayTime = " + delayTime);
            ////////UnityEngine.//Debug.Log("========================>>throwingTime = " + throwingTime);
            ////////UnityEngine.//Debug.Log("========================>>distanceTime = " + distanceTime);

            float _time = (delayTime + throwingTime + relayingTime + distanceTime + fenceDelay);


            if (errorType == FieldParm.ErrorType.Fumble)
            {
                if (bOutofInfield == false)
                {
                    ////UnityEngine.//Debug.Log("========================>>펌블이 인필드에서 나온경우 보수적으로 계산!!");
                    _time = _time * Random.Range(0.5f, 0.8f);
                }
            }
            else if (errorType == FieldParm.ErrorType.Drop || errorType == FieldParm.ErrorType.Tunnel)
            {
                if (bOutofInfield == true)
                {
                    ////UnityEngine.//Debug.Log("========================>>드랍 또는 알까기가 외야에서 나온경우 공격적으로 계산!!");
                    _time = _time * Random.Range(1.2f, 1.5f);
                }
            }

            ////////UnityEngine.//Debug.Log("========================>>getThrowingTotalFrame = " + frame);
            ////UnityEngine.Debug.Log("###########################################################");
            return _time;
        }

        //던지는 상황이 avaliable하지 않은 경우 다시 한번 throw 체크
        //checkOneMoreBase시 호출
        public void setRecheckThrow(int nextBase)
        {
            ////UnityEngine.//Debug.Log("========================>>다시 던지기 체크 ball.nBallStep = " + ball.nBallStep);
            ////UnityEngine.//Debug.Log("========================>>nCatchIndex/nCarrierIndex/nTargetIndex " +nCatchIndex+"/"+nCarrierIndex+"/"+nTargetIndex);
            if (nextBase > nRecheckTarget)
            {
                if (ball.step == BallStep.BALL_CATCH || ball.step == BallStep.BALL_CARRY)// || ball.nBallStep == FBall.BALL_THROW_CATCH)
                {
                    if (nCatchIndex != -1)
                    {
                        ////UnityEngine.//Debug.Log("========================>>다시 던지기 체크 nCatchIndex: " + nCatchIndex);
                        fielder[nCatchIndex].setRecheckThrow(nextBase);
                    }
                    else if (nCarrierIndex != -1)
                    {
                        ////UnityEngine.//Debug.Log("========================>>다시 던지기 체크 nCarrierIndex: " + nCarrierIndex);
                        fielder[nCarrierIndex].setRecheckThrow(nextBase);
                    }
                }
            }
        }

        //던지는 상황이 avaliable하지 않은 경우 다시 한번 throw 체크 타입 2
        //throwBallCatch시 호출
        public void setRecheckThrow2(int posIndex)
        {
            ////UnityEngine.//Debug.Log("========================>>다시 던지기 체크 nCatchIndex: " + nCatchIndex);
            int nextBase = -1;

            for (int i = FieldParm.HOMEBASE_INDEX; i > FieldParm.FIRSTBASE_INDEX; i--)
            {
                if (run.bOnRunning[i] == true)
                {
                    nextBase = i;
                    break;
                }
            }

            if (nextBase != -1)
            {
                fielder[posIndex].setRecheckThrow(nextBase);
            }
        }

        //송구시 능력치, 초기 위치, 목적지에 따른 벡터값을 구한다
        public void setThrowingVector(float startX, float startY, float startZ, float endX, float endY, float throwSpeed, float wrist, int dir, bool bThrowError)//, bool bToss)
        {
            ball.bBound = false;
            ball.nBallX = startX;// +dirOffset[dir, 0];
            ball.nBallY = startY;// +dirOffset[dir, 1];
            ball.nBallZ = startZ;// FieldParm.BALL_INIT_HEIGHT * 2;

            if (throwSpeed == 0)
            {
                throwSpeed = FieldingMechanism.BASIC_THROW_SPEED;
            }

            if (bTossThrow == true)
            {
                //토스시 시작 z위치 낮게해줌
                ball.nBallZ = ball.nBallZ / 5;
                ball.curThrowGravityAccel = FBall._GRAVITY_ACCELERATION_THROW * 1.35f;
            }
            else
            {
                ball.curThrowGravityAccel = FBall._GRAVITY_ACCELERATION_THROW;
            }

            ball.throwWrist = wrist;
            ball.speed = throwSpeed;
            ball.angle = (Mathf.Atan2(endY - ball.nBallY, endX - ball.nBallX) * Mathf.Rad2Deg) - 90;
            ////UnityEngine.//Debug.Log("=======================================================>> correctAngle = "+(ball.angle*Mathf.Rad2Deg));
            ball.angleZ = ball.getThrowAngleZ2(endX, endY);//, bToss);

                        
            if (bThrowError == true)
            {
                float curAngle = (ball.angle + 90.0f + (MyMath.Half()?-90:90)) * Mathf.Deg2Rad;    //degree                
                int cover = getBaseCoverIndex(nTargetIndex);

                if (curThrowIndex < CPlayer._LEFTFIELDER)
                {
                    //if(Mode.bPvpMode == true) Debug.Log("악송구 테스트 랜덤 ======>>>>> " + Random.Range(0.0f, 100.0f) + "   시드 : " + PvpManager.RandomSeed);

                    int gab = Random.Range(50, 100);
                    float gabY = gab * Mathf.Sin(curAngle);
                    float gabX = gab * Mathf.Cos(curAngle);
                    
                    if (MyMath.Half())
                    {
                        //바운드 송구 에러시
                        ball.angleZ = ball.angleZ * 0.3f;
                        errorType = FieldParm.ErrorType.WildThrow;
                    }
                    else
                    {
                        //머리위 악송구
                        ball.angleZ = ball.angleZ * 1.3f;
                        errorType = FieldParm.ErrorType.WildThrowUp;
                    }
                    if (cover != -1)
                    {
                        fielder[cover].setErrorException(gabX, gabY + 50);
                        ball.angle = (Mathf.Atan2((endY + gabY) - ball.nBallY, (endX + gabX) - ball.nBallX) * Mathf.Rad2Deg) - 90;
                        ////UnityEngine.//Debug.Log("=======================================================>> errorAngle = " + (ball.angle * Mathf.Rad2Deg));
                    }

                    if ((run.bStealBase == true || run.bPickOff == true) && nTargetIndex == FieldParm.SECONDBASE_INDEX)
                    {
                        //도루시 중견수 위치 보정
                        fielder[CPlayer._CENTERFIELDER].posX += (gabX*2);
                    }
                }
                else
                {
                    //외야는 펌블이나 잘못된 쪽으로의 송구로 가닥을 잡을것
                    int gab = Random.Range(50, 200);
                    float gabY = gab * Mathf.Sin(curAngle);
                    float gabX = gab * Mathf.Cos(curAngle);
                    errorType = FieldParm.ErrorType.WildWrongPlace;
                    if (cover != -1)
                    {
                        fielder[cover].posX += gabX;
                        fielder[cover].posY += gabY;
                        ball.angle = (Mathf.Atan2((endY + gabY) - ball.nBallY, (endX + gabX) - ball.nBallX) * Mathf.Rad2Deg) - 90;
                        ////UnityEngine.//Debug.Log("=======================================================>> errorAngle = " + (ball.angle * Mathf.Rad2Deg));
                    }
                }

            }

            ////////UnityEngine.//Debug.Log("==================>>ball.angleZ = " + ball.angleZ);

            ball.setVelocity();
            //ball.setVelocityZ(ball.speed);
            ball.nBallDZ = ball.angleZ;
            ball.nScreenBallZ = ball.nBallZ * FBall._Z_AXIS_PROJECTION_COEFF2;

            ball.bBallHidden = true;
            ball.curTime = 0;

            //nBallDX = 0;
            //nBallDY = 0;
            //nBallDZ = 0;
        }

        //해당 야수(posIndex)가 해당 베이스(baseIndex)로 던지는 경우 
        //아웃 혹은 세이프가 될 수 있는 지 여부를 true or false값으로 리턴
        //주자의 능력치, 딜레이 
        //야수의 능력치, 딜레이, 상황에 따른 딜레이(펜스, 충돌등)
        //모두를 고려함
        private bool checkThrowFrameOffset(int posIndex, int baseIndex, bool bOutFielder)
        {
            ////////UnityEngine.//Debug.Log("=======================>>checkThrowFrameOffset 함수 체크");
            if (baseIndex == FieldParm.RELAY_INDEX)
            {
                //릴레이 포지션으로 타겟시 무조건 true 리턴
                return true;
            }

            if (posIndex < CPlayer._LEFTFIELDER && baseIndex == FieldParm.FIRSTBASE_INDEX)
            {
                //릴레이 포지션 상태가 아닌 내야수가 일루 타겟 뜨면 무조건 리턴
                if (fielder[posIndex].bRelayPositioning == false)
                {
                    return true;
                }
            }

            int runnerIndex = run.getRunnerDestIndex(baseIndex);
            if (runnerIndex == -1) return false;

            Runner checkRunner = run.runner[runnerIndex];

            //Throw 딜레이 적용 (안전빵 갭 0.015초 추가)
            float throwDelay = fielder[posIndex].getThrowingDelay(baseIndex, true, run.runner[runnerIndex].getRunnerSpeed()) + 0.015f; 


            //딜레이 적용된 러너의 포지션
            float runnerX = checkRunner.posX;// +(runner[runnerIndex].dX * throwDelay);
            float runnerY = checkRunner.posY;// +(runner[runnerIndex].dY * throwDelay);

            //해당 베이스까지 거리
            float basePosX = getOriginX(FieldSize.getBasePosX(baseIndex));
            float basePosY = getOriginY(FieldSize.getBasePosY(baseIndex));

            //주자 베이스간 거리및 주자가 베이스에 도착하기까지 시간
            float distanceRunnerBase = MyMath.getDistance(runnerX, basePosX, runnerY, basePosY);
            float maxRunnerTime = (distanceRunnerBase / (checkRunner.curSpeed * 0.8f)) - throwDelay;

            //야수 베이스간 거리및 송구가 베이스에 도착하기까지 시간 + 딜레이
            float distanceFielderBase = MyMath.getDistance(fielder[posIndex].posX, basePosX, fielder[posIndex].posY, basePosY);
            float maxFielderTime = (distanceFielderBase / fielder[posIndex].THROW_SPEED) + throwDelay;

            //이부분이 수비수의 AI와 난수값을 통해서 베리에이션을 둔다
            //UnityEngine.Debug.Log("##################################################");
            //////UnityEngine.//Debug.Log("=======================>>baseIndex = " + baseIndex);
            //////UnityEngine.//Debug.Log("=======================>>throwDelay = " + throwDelay);
            ////UnityEngine.//Debug.Log("==================>>이값이 작으면 던짐 maxFielderTime = " + maxFielderTime);
            //////UnityEngine.//Debug.Log("==================>>                   maxRunnerTime = " + maxRunnerTime);
            //UnityEngine.Debug.Log("##################################################");
            ////UnityEngine.//Debug.Log("==================>>이값이 작으면 던짐 maxFielderTime = " + maxFielderTime);
            ////UnityEngine.//Debug.Log("==================>>                   maxRunnerTime = " + maxRunnerTime);

            if (bOutFielder == true)
            {
                if (ball.bFenceCol == false)
                {
                    maxRunnerTime += 0.5f;
                    ////UnityEngine.//Debug.Log("==================>>외야 재계산    maxRunnerTime = " + maxRunnerTime);
                }
            }

            if (maxFielderTime < maxRunnerTime)
            {
                //////UnityEngine.//Debug.Log("=======================>>baseIndex = " + baseIndex);
                //////UnityEngine.//Debug.Log("=======================>>throwDelay = " + throwDelay);
                //////UnityEngine.//Debug.Log("==================>>이값이 작으면 던짐 maxFielderTime = " + maxFielderTime);
                //////UnityEngine.//Debug.Log("==================>>                   maxRunnerTime = " + maxRunnerTime);
                //////UnityEngine.//Debug.Log("=======================>>checkThrowFrameOffset 함수 return true");
                ////UnityEngine.//Debug.Log("=======================>>baseIndex = " + baseIndex + "===>>maxFielderTime = " + maxFielderTime + "====>>maxRunnerTime = " + maxRunnerTime);
                return true;
            }
            return false;
        }

        //공을 던져 아웃시킬수 있는지 여부를 판단하는 함수
        //수비 AI의 핵심이다.
        public bool checkGrounderOutPossible(int posIndex, int thisBase)
        {
            if (posIndex >= CPlayer._LEFTFIELDER)
            {
                //외야수인 경우
                if (thisBase == FieldParm.RELAY_INDEX)
                {
                    //릴레이 포지션에서 필수 리턴
                    return true;
                }
                else
                {
                    if (checkThrowFrameOffset(posIndex, thisBase, true) == true)
                    {
                        if ((run.getDestRunner(thisBase) == true) 
                         && (run.bOnRunning[thisBase] || run.bOnBackRunning[thisBase]))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            else
            {
                //내야수인 경우
                if (thisBase == FieldParm.FIRSTBASE_INDEX)
                {
                    ////////UnityEngine.//Debug.Log("==================>> FIRST BASE CHECK");
                    //릴레이 포지션에서 필수 리턴
                    if (run.bBallOnBase[thisBase] == true)
                    {
                        ////////UnityEngine.//Debug.Log("==================>> 여기서 false냐??/");
                        ////////UnityEngine.//Debug.Log("==================>> field.nTargetIndex = " + field.nTargetIndex);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    //아웃카운트 포지션 베이스에 따른 가중치
                    if (run.checkOutWeightWithPositionAndOutcount(posIndex, thisBase, manager.nOutCount) == true)
                    {
                        //아웃 시킬 수 있는지 ...
                        if (checkThrowFrameOffset(posIndex, thisBase, false) == true)
                        {
                            if ((run.getDestRunner(thisBase) == true) 
                             && (run.bOnRunning[thisBase] || run.bOnBackRunning[thisBase]))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
        }

        //다시한번 공을 던져 아웃시킬수 있는지 여부를 판단한다
        public bool checkRecheckOutPossible(int posIndex, int baseIndex)
        {
            int runnerIndex = run.getRunnerDestIndex(baseIndex);
            if (runnerIndex == -1) return false;
            Runner checkRunner = run.runner[runnerIndex];

            //딜레이 적용된 러너의 포지션
            float runnerX = checkRunner.posX;
            float runnerY = checkRunner.posY;

            //해당 베이스까지 거리
            float basePosX = getOriginX(FieldSize.getBasePosX(baseIndex));
            float basePosY = getOriginY(FieldSize.getBasePosY(baseIndex));

            //주자 베이스간 거리및 주자가 베이스에 도착하기까지 시간
            float distanceRunnerBase = MyMath.getDistance(runnerX, basePosX, runnerY, basePosY);
            float maxRunnerTime = (distanceRunnerBase / checkRunner.curSpeed * 0.8f);

            //야수 베이스간 거리및 송구가 베이스에 도착하기까지 시간 + 딜레이
            float distanceFielderBase = MyMath.getDistance(fielder[posIndex].posX, basePosX, fielder[posIndex].posY, basePosY);
            float maxFielderTime = (distanceFielderBase / fielder[posIndex].THROW_SPEED);


            ////UnityEngine.//Debug.Log("==================>>이값이 작으면 던짐 maxFielderTime = " + maxFielderTime);
            ////UnityEngine.//Debug.Log("==================>>                   maxRunnerTime = " + maxRunnerTime);

            if (maxFielderTime < maxRunnerTime)
            {
                return true;
            }

            return false;

        }

        //던지는것이 과연 유효할지를 판단하는 함수 : 더불어 타겟 베이스도 여기서 구한다.
        public bool checkThrowAvailable(int posIndex)
        {            
#if THROW_TEST
        nTargetIndex = FieldParm.HOMEBASE_INDEX;
        return true;
#else
            bBaseCoverAfterLiner = false;

            //견제 처리
            if (bFieldPickOffFlag == true)
            {
                if (errorType == FieldParm.ErrorType.WildThrow || errorType == FieldParm.ErrorType.WildThrowUp)
                {
                    if (bFirstThrow == false)
                    {
                        //견제시 송구 에러 처리
                        for (int i = FieldParm.HOMEBASE_INDEX; i > FieldParm.FIRSTBASE_INDEX; i--)
                        {
                            if (run.bOnBase[i] || run.bOnRunning[i])
                            {
                                bFirstThrow = true;
                                nTargetIndex = i;
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            //도루 확인
            if (bStealThrow == true)
            {
                nTargetIndex = run.getStealDest();
                bStealThrow = false;
                return true;
            }

            //치고 달리기시 직선타에 의한 귀루 체크
            if (checkLinerOutAndRunnerBack(posIndex) == true)
            {
                ////UnityEngine.//Debug.Log("=========================================================>>치고달리기시 직선타 아웃");
                bNoCheckReThrow = true;
                return true;
            }

            //번트 필딩 확인
            if (checkBuntFieldTarget(posIndex) == true)
            {
                ////UnityEngine.//Debug.Log("=========================================================>>번트 송구 검색");
                bFirstThrow = true;
                return true;
            }
            
            //와일드피칭
            if (checkWildPitchingTarget(posIndex) == true)
            {
                ////UnityEngine.//Debug.Log("=========================================================>>와일드 피칭 검색");
                return wildPitchTarget(posIndex);
            }

            //3아웃 상황시 처리
            if (manager.nOutCount > 2 || manager.bThreeOutChange == true)
            {
                return false;
            }

            //달리고 있는 주자 있는지 여부 체크
            bool bNoThrowing = true;
            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    int _state = (int)run.runner[i].state;
                    if (_state > (int)RunState.STANDBY &&
                        _state < (int)RunState.FIRSTBASE_SAFE)
                    {
                        bNoThrowing = false;
                        break;
                    }
                }
            }

            if (bNoThrowing == true)
            {
                if (posIndex < CPlayer._LEFTFIELDER)
                {
                    return false;
                }
                else
                {
                    nTargetIndex = FieldParm.RELAY_INDEX;
                    return true;
                }
            }

            if (posIndex == nRelayFielderIndex)
            {
                //중계처리
                if (checkRelayTarget(posIndex) == false)
                {
                    //중계 아이들동작 처리
                }
                else
                {
                    //Debug.Log("==============================================>> 중계자 vs 주자 오버런 체크 여기인가?");
                    run.checkOverrunRunner(posIndex, nTargetIndex);
                    return true;
                }
            }
            else
            {
                if (posIndex < CPlayer._LEFTFIELDER)
                {
                    return checkInfielderTarget(posIndex);
                }
                else
                {
                    return checkOutfielderTarget(posIndex);
                }
            }

            return false;
#endif
        }

        private bool checkRelayTarget(int posIndex)
        {
            int notThisBase = 1000;
            int count = 0;
            while (true)
            {
                int throwBase = run.getFirstRunnerDest(posIndex, notThisBase);

                if (throwBase == FieldParm.RELAY_INDEX)
                {
                    return false;
                }
                else
                {
                    //if (checkGrounderOutPossible(posIndex, throwBase) == true)
                    if ((run.getDestRunner(throwBase) == true) && (run.bOnRunning[throwBase] || run.bOnBackRunning[throwBase]))
                    {
                        nTargetIndex = throwBase;//
                        if (nTargetIndex == FieldParm.FIRSTBASE_INDEX)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        notThisBase = throwBase;
                    }

                    if (++count > 10)
                    {
                        return false;
                    }
                }
            }
        }

        private bool checkInfielderTarget(int posIndex)
        {
            int notThisBase = 1000;
            int count = 0;
            while (true)
            {
                int throwBase = run.getFirstRunnerDest(posIndex, notThisBase);

                if (throwBase == FieldParm.RELAY_INDEX)
                {
                    return false;
                }
                else
                {
                    if (checkGrounderOutPossible(posIndex, throwBase) == true)
                    {
                        nTargetIndex = throwBase;//
                        return true;
                    }
                    else
                    {
                        if (throwBase == FieldParm.FIRSTBASE_INDEX)
                        {
                            nTargetIndex = FieldParm.FIRSTBASE_INDEX;
                            return true;
                        }
                        else
                        {
                            notThisBase = throwBase;
                        }
                    }
                    if (++count > 10)
                    {
                        int firstRunner = run.getFirstRunnerDest(posIndex, 1000);

                        return false;
                    }
                }
            }
        }

        private bool checkOutfielderTarget(int posIndex)
        {
            int notThisBase = 1000;
            int count = 0;
            while (true)
            {
                int throwBase = run.getFirstRunnerDest(posIndex, notThisBase);

                if (throwBase == FieldParm.RELAY_INDEX)
                {
                    nTargetIndex = FieldParm.RELAY_INDEX;
                    return true;
                }
                else
                {
                    if (checkGrounderOutPossible(posIndex, throwBase) == true)
                    {
                        nTargetIndex = throwBase;
                        if (nTargetIndex == FieldParm.HOMEBASE_INDEX)
                        {
                            if (bMoreDouble == true || ball.bFenceCol == true)
                            {
                                nTargetIndex = FieldParm.RELAY_INDEX;
                            }
                        }
                        else if (nTargetIndex == FieldParm.THIRDBASE_INDEX)
                        {
                            if (bMoreDouble == true || ball.bFenceCol == true)
                            {
                                if (ball.firstAngle < -12)
                                {
                                    nTargetIndex = FieldParm.RELAY_INDEX;
                                }
                            }
                        }

                        //Debug.Log("==============================================>> 외야수 vs 주자 오버런 체크 여기인가?");
                        run.checkOverrunRunner(posIndex, nTargetIndex);

                        return true;
                    }
                    else
                    {
                        if (throwBase == FieldParm.FIRSTBASE_INDEX)
                        {
                            nTargetIndex = FieldParm.RELAY_INDEX;
                            return true;
                        }
                        else
                        {
                            notThisBase = throwBase;
                        }
                    }

                    if (++count > 10)
                    {
                        nTargetIndex = FieldParm.RELAY_INDEX;
                        return true;
                    }
                }
            }
        }




        //직선타 아웃시
        private bool checkLinerOutAndRunnerBack(int posIndex)
        {            
            if (run.bHitAndRun == true && bOutByFlyball == true)
            {
                int index = -1;
                float distance = 10000000;
                for (int i = 0; i < 4; i++)
                {
                    if (run.runnerActive[i] == true)
                    {
                        Runner curRunner = run.runner[i];
                        if (curRunner.state == RunState.REVERSE_DELAY || curRunner.bForcedOutBackMove == true)
                        {
                            int curBase = curRunner.currentPos;

                            float dstBaseX = getOriginX(FieldSize.getBasePosX(curBase));
                            float dstBaseY = getOriginY(FieldSize.getBasePosY(curBase));

                            float curDistance = MyMath.getDistance(fielder[posIndex].posX, dstBaseX, fielder[posIndex].posY, dstBaseY);
                            
                            if(curDistance < distance)
                            {
                                distance = curDistance;
                                index = curRunner.currentPos;
                            }
                        }
                    }
                }

                if (index != -1)
                {
                    nTargetIndex = index;
                    if (fielder[posIndex].nCoveringIndex == index)
                    {
                        if (fielder[posIndex].bBaseCovering == false)
                        {
                            bBaseCoverAfterLiner = true;    //베이스 커버를 한다
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

            }

            return false;
        }

        //번트시
        private bool checkBuntFieldTarget(int posIndex)
        {
            if (bBuntFielding == true && bFirstThrow == false)
            {
                if (posIndex < CPlayer._LEFTFIELDER)
                {
                    ////UnityEngine.//Debug.Log("==========================>>checkBuntFieldTarget 번트상황");
                    bool bRunnerSafe = false;
                    bool destRunnerSpeedChange = true;
                    float tagDelay = 0;
                    //float outTime = 0.8f;
                    if (batter.buntType == SimulBuntType.SQUEEZE)
                    {
                        //스킬발동인 경우
                        if (fielder[posIndex].skillDashThrowLevel > 0)
                        {
                            //Debug.Log("===========================>>스퀴즈시 스킬 발동시 홈승부 아웃");
                            //특급송구 발동하면 해당 러너는 무조건 아웃
                            nTargetIndex = FieldParm.HOMEBASE_INDEX;
                            bRunnerSafe = false;
                        }
                        else
                        {
                            /*
                            if (manager.bMyTurn && bSqueezeFlagOn == false)
                            {
                                bBuntSuccess = false;
                                batter.buntResult = SpecificBuntType.SQUEEZ_FAIL;
                            }*/

                            if (bBuntSuccess == true || batter.buntResult == SpecificBuntType.SQUEEZ_SUCCESS)
                            {
                                nTargetIndex = FieldParm.FIRSTBASE_INDEX;
                            }
                            else
                            {
                                nTargetIndex = FieldParm.HOMEBASE_INDEX;
                                bRunnerSafe = false;
                                tagDelay = 0.2f;
                            }
                        }
                    }
                    else if (batter.buntType == SimulBuntType.DRAG)
                    {
                        ////UnityEngine.//Debug.Log("=========================================================>>checkBuntFieldTarget : Drag");
                        bRunnerSafe = bBuntSuccess;
                        if (batter.buntFielder == CPlayer._FIRSTBASEMAN && bRunnerSafe==false)
                        {
                            destRunnerSpeedChange = false;
                        }
                        nTargetIndex = FieldParm.FIRSTBASE_INDEX;
                    }
                    else //희생
                    {
                        ////UnityEngine.//Debug.Log("==============================>>bOnRunning[2] = " + run.bOnRunning[2]);
                        ////UnityEngine.//Debug.Log("==============================>>bOnRunning[1] = " + run.bOnRunning[1]);
                        //번트앤드 런에 의한 예외 처리
                        Runner secondRunner = run.getDestRunner(FieldParm.THIRDBASE_INDEX);
                        Runner firstRunner = run.getDestRunner(FieldParm.SECONDBASE_INDEX);
                        bool bException = false;

                        if (secondRunner != null)
                        {
                            if (secondRunner.bStealFlag == true)
                            {
                                bException = true;
                            }
                        }

                        if (firstRunner != null)
                        {
                            if (firstRunner.bStealFlag == true)
                            {
                                bException = true;
                            }
                        }

                        if (run.bOnRunning[FieldParm.THIRDBASE_INDEX] == false && run.bOnRunning[FieldParm.SECONDBASE_INDEX] == false)
                        {
                            bException = true;
                        }

                        if (bException == true)
                        {
                            ////UnityEngine.//Debug.Log("==============================>>번트앤드런 발생혹은 주자 이미 도착!!! -> 예외처리");
                            nTargetIndex = FieldParm.FIRSTBASE_INDEX;
                            bRunnerSafe = false;
                        }
                        else
                        {
                            if (bBuntSuccess == true || batter.buntResult == SpecificBuntType.SAC_SUCCESS)
                            {
                                nTargetIndex = FieldParm.FIRSTBASE_INDEX;
                                //destRunnerSpeedChange = false; //보내기시 1루에서 계속 살음
                                bRunnerSafe = false;             //처리   
                            }
                            else
                            {
                                nTargetIndex = run.bOnRunning[FieldParm.THIRDBASE_INDEX] == true ? FieldParm.THIRDBASE_INDEX : FieldParm.SECONDBASE_INDEX;
                                bRunnerSafe = false;
                            }
                        }
                    }

                    if (bBuntSuccess == true)
                    {
                        //번트 성공인 경우
                        if (fielder[posIndex].skillDashThrowLevel > 0 && bFieldVsSkillOffenseWin == false)
                        {
                            //Debug.Log("===========================>>특급송구 발동하면 해당 러너는 무조건 아웃");
                            //특급송구 발동하면 해당 러너는 무조건 아웃
                            bRunnerSafe = false;
                        }
                    }

                    if (destRunnerSpeedChange == true)
                    {
                        Runner destRunner = run.getDestRunner(nTargetIndex);
                        if (destRunner != null)
                        {
                            float rate = destRunner.basePositionRate();
                            //Debug.Log("=======================================================================================================>>Rate = " + rate);
                            if (rate > 0.45f)
                            {
                                float timeLeft = getTimeLeftforThrow(nTargetIndex, fielder[posIndex].posX, fielder[posIndex].posY, 0.25f, fielder[posIndex].THROW_SPEED);
                                //Debug.Log("=======================================================================================================>>timeLeft = " + timeLeft);
                                destRunner.setShobuRunnerSpeed(!bRunnerSafe, timeLeft - tagDelay, 1.6f);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        //와일드 피치 시
        private bool checkWildPitchingTarget(int posIndex)
        {
            if (run.bWildPitchRunning == true)
            {                
                return true;
            }
            return false;
        }

        //와일드 피치시 송구 타겟
        private bool wildPitchTarget(int posIndex)
        {
            int weight = -1;
            nTargetIndex = -1;
            
            if (bFirstThrow == false)
            {
                bFirstThrow = true;

                if (wildPitchCase == FieldParm.WildPitchCase.BaseOnBall)
                {
                    //던질 필요없음
                }
                else
                {
                    //가중치 계산
                    for (int i = 3; i >= 0; i--)
                    {
                        /*Runner dstRunner = run.getDestRunner(i);
                        if (dstRunner != null)
                        {
                            if (i > weight)
                            {
                                weight = i - (dstRunner.bMoving ? 0 : 1);
                                if (weight < 0) weight = 0;
                            }
                        }*/
                        if (run.bOnRunning[i] == true)
                        {
                            weight = i;
                        }

                    }

                    if (bWildPitchCatcherBlock == false)
                    {
                        //블록 실패시 가중치 높은 곳으로 송구
                        if (weight == -1)
                        {
                            return false;
                        }
                        else
                        {
                            nTargetIndex = weight;
                            return true;
                        }
                    }
                    else
                    {
                        //블록성공시
                        if (wildPitchCase == FieldParm.WildPitchCase.NotOut)
                        {
                            if (run.bNotOutRunning == true)
                            {
                                //낫아웃상태가 활성화되면 1루 던짐
                                nTargetIndex = FieldParm.FIRSTBASE_INDEX;
                                return true;
                            }
                        }
                    }
                }
            }
            fielder[posIndex].throwAgainState = (posIndex == CPlayer._CATCHER ? FieldParm.ThrowAgain.BackToPosition : FieldParm.ThrowAgain.NoThrow);
            fielder[posIndex].bForcedThrow = false;
            return false;
        }



        //스킬 승부 관련
        public float getTimeLeftforThrow(int target, float posX, float posY, float delay, float speed)
        {
            float targetX = getOriginX(FieldSize.getBasePosX(target));
            float targetY = getOriginY(FieldSize.getBasePosY(target));

            return delay + (RunningMechnism.getDistance(targetX - posX, targetY - posY) / speed);
        }

        //리와인드 관련
        public float getTimeLeftforThrow(int posIndex, int target)
        {
            Fielder player = fielder[posIndex];
            float targetX = getOriginX(FieldSize.getBasePosX(target));
            float targetY = getOriginY(FieldSize.getBasePosY(target));

            ////UnityEngine.//Debug.Log("============================>>송구 거리 : " + RunningMechnism.getDistance(targetX - player.posX, targetY - player.posY));

            return (player.THROW_DELAY + RunningMechnism.getDistance(targetX - player.posX, targetY - player.posY) / player.THROW_SPEED);
        }



        //다시 던질지 여부를 확인
        public int checkReThrow()
        {
            ////UnityEngine.//Debug.Log("========================>>checkReThrow");
            if (nTargetIndex != -1)
            {
                if (run.bOnRunning[nTargetIndex] == false)
                {
                    if (run.bOnBase[nTargetIndex] == false)
                    {
                        int next = nTargetIndex + 1;
                        if (next <= FieldParm.HOMEBASE_INDEX)
                        {
                            if (run.bOnRunning[next] == true)
                            {
                                ////UnityEngine.//Debug.Log("==============>>recheck next/nTargetIndex :" + next + "/" + nTargetIndex);
                                if (next > nTargetIndex)
                                {
                                    return next;
                                }
                            }
                        }
                    }
                }
            }
            return -1;
        }

        //태그시는 throwReady상태에 들어와도 태그 상태로 대기하여야 하는데
        //태그가 끝나면 그 대기 상태를 끝내고 다시 Throw Ready상태로 전환하는함수
        public bool checkTagWaitEnd()
        {
            //태그를 웨이트 하는데 다른 주자가 뛰는 경우 던질까 말까 결정

            int curBase = -1; // nTargetIndex;
            int getFinal = -10;

            if (run.bOnRunning[nTargetIndex] == true)
            {
                curBase = nTargetIndex;
            }

            for (int i = FieldParm.SECONDBASE_INDEX; i <= FieldParm.HOMEBASE_INDEX; i++)
            {
                if (run.bOnRunning[i] == true)
                {
                    getFinal = i;
                }
            }

            //////UnityEngine.//Debug.Log("==========>>curBase / getFinal ::" + curBase + " / " + getFinal);
            if (getFinal > curBase)
            {
                nTargetIndex = getFinal;
                return true;
            }

            return false;
        }


        //////////////////////////////////////////////////////////////////
        //송구 에러 관련 메쏘드
        //////////////////////////////////////////////////////////////////    
        //송구 에러 세팅 - initBatter에서 호출
        public void setThrowError()
        {
            //PVP모드에서 타자정보를 수신해야하는 투수컨트롤 부분에서는 이부븐을 스킵한다.
            //if (Mode.bPvpMode == true && manager.bMyTurn == false) return;
            //Debug.Log("=================================================>> 송구 에러 계산");

#if _Local_Balance
            /*if (InGameDebug._ALWAYS_THROW_ERROR == true && MyMath.Percent() < tempSelectPage.ERROR_PER)
            {
                //일부러 송구에러 발생
                for (int i = 0; i < 9; i++) fielder[i].bThrowErrorFlag = true;// MyMath.Half() ? true : false;// true;
            }
            else*/
#endif
            {
                if (manager.nInningCount == 1 && manager.nOutCount == 0) return;
                for (int i = 0; i < 9; i++)
                {
                    bool bCore = false;
                    if (i == CPlayer._CATCHER || i == CPlayer._THIRDBASEMAN || i == CPlayer._SHORTSTOP) bCore = true;
                    fielder[i].bThrowErrorFlag = SimulParm.checkThrowError(fielder[i].throwingAblil, bCore);  
                }
            }

        }

        public void setThrowErrorInit()
        {
            for (int i = 0; i < 9; i++)
            {
                fielder[i].bThrowErrorFlag = false;
            }
        }

        public void setBaseCoverAfterThrowError(int coverIndex)
        {
            int baseCover;
            int curCovering = -1;
            if (bBaseCoverd[FieldParm.FIRSTBASE_INDEX] == false)
            {
                baseCover = (coverIndex == CPlayer._FIRSTBASEMAN ? CPlayer._PITCHER : CPlayer._FIRSTBASEMAN);
                curCovering = baseCover;
                ////UnityEngine.//Debug.Log("=================================================>> 1루 베이스 커버 = " + baseCover);
                fielder[baseCover].setBaseCover(FieldParm.FIRSTBASE_INDEX);
            }
            if (bBaseCoverd[FieldParm.SECONDBASE_INDEX] == false)
            {
                if (coverIndex == CPlayer._SECONDBASEMAN) baseCover = CPlayer._SHORTSTOP;
                else if (coverIndex == CPlayer._SHORTSTOP) baseCover = CPlayer._SECONDBASEMAN;
                else
                {
                    if (fielder[coverIndex].posX < FieldSize.getMoundPosX())
                    {
                        baseCover = CPlayer._SECONDBASEMAN;
                        fielder[CPlayer._SHORTSTOP].bBaseCovering = false;
                        fielder[CPlayer._SHORTSTOP].setBackupPosition(0, 0, true);
                    }
                    else
                    {
                        baseCover = CPlayer._SHORTSTOP;
                        fielder[CPlayer._SECONDBASEMAN].bBaseCovering = false;
                        fielder[CPlayer._SECONDBASEMAN].setBackupPosition(0, 0, true);
                    }
                }                
                ////UnityEngine.//Debug.Log("=================================================>> 2루 베이스 커버 = " + baseCover);
                fielder[baseCover].setBaseCover(FieldParm.SECONDBASE_INDEX);
            }
            if (bBaseCoverd[FieldParm.THIRDBASE_INDEX] == false)
            {
                baseCover = (coverIndex == CPlayer._THIRDBASEMAN ? CPlayer._PITCHER : CPlayer._THIRDBASEMAN);
                if (curCovering == baseCover)
                {
                    baseCover = CPlayer._SHORTSTOP;
                    curCovering = baseCover;
                }
                ////UnityEngine.//Debug.Log("=================================================>> 3루 베이스 커버 = " + baseCover);
                fielder[baseCover].setBaseCover(FieldParm.THIRDBASE_INDEX);
            }
            if (bBaseCoverd[FieldParm.HOMEBASE_INDEX] == false)
            {
                baseCover = (coverIndex == CPlayer._CATCHER ? CPlayer._PITCHER : CPlayer._CATCHER);
                if (curCovering == baseCover)
                {
                    baseCover = CPlayer._FIRSTBASEMAN;
                }
                ////UnityEngine.//Debug.Log("=================================================>> 홈 베이스 커버 = " + baseCover);
                fielder[baseCover].setBaseCover(FieldParm.HOMEBASE_INDEX);
            }
        }


        /// <summary>
        /// 에러 플래그를 pvp모드에서 동기화
        /// </summary>
        /// <param name="catchError"></param>
        /// <param name="throwError"></param>
        public void setErrorFlagSync(bool[] catchError, bool[] throwError)
        {
            //Debug.Log("=================================================>> 에러 동기화");
            for (int i = 0; i < 9; i++)
            {
                fielder[i].bCatchErrorFlag = catchError[i];
                fielder[i].bThrowErrorFlag = throwError[i];
            }

        }

        

        //스킬 연출시 느려짐
        public void setSkillSlow()
        {
            setTimeScale(0.001f);
            StartCoroutine(setTimeInit(1.0f));
        }

        IEnumerator setTimeInit(float delay)
        {
            yield return new WaitForSeconds(delay);
            setTimeScale(1.0f);// Mode.bAutoPlay ? Field.DECISIVE_TIME_SCALE : Field.INIT_TIME_SCALE);
        }
        

        /////////////////////////////////////////////////////////////
        //필드 상 연출 및 UI 관련 메쏘드
        /////////////////////////////////////////////////////////////
        
        //세이프 콜 
        public void checkSafeCall(int curBase)
        {
            if (curBase != FieldParm.RELAY_INDEX)
            {
                if (run.bOnBase[curBase] == true)
                {
                    judge.setCall(curBase, CallType._SAFE);
                    IngameUI.GetFieldCall().Call("safe");
                    returnWaitTime = 2.2f;
                    if (bFieldPickOffFlag == true || bFieldStealFlag == true)
                    {
                        returnCheck_Steal_Pickoff(-1);
                    }
                    //세이프콜 사운드
                    soundmanager.Get().PlaySound(soundmanager.SoundID.SafeCall);
                }
            }
        }

        //세이프 콜 - type2
        public void checkSafeCall2(int curBase)
        {
            
            judge.setCall(curBase, CallType._SAFE);
            IngameUI.GetFieldCall().Call("safe");
            returnWaitTime = 2.2f;
            //세잎콜 사운드
            soundmanager.Get().PlaySound(soundmanager.SoundID.ScoreSound);
        }




        public void setHitStringType()
        {
            if (groundCatchFielder != -1)
            {
                if (groundCatchFielder < CPlayer._LEFTFIELDER)
                {
                    manager.strHitType = Util.GetPositionString(groundCatchFielder) + "앞 내야";
                    return;
                }                
            }

            if (ball.firstAngle > 25) manager.strHitType = "좌익수앞 ";
            else if(ball.firstAngle < -25) manager.strHitType = "우익수앞 ";
            else manager.strHitType = "중견수앞 ";

            if (ball.firstAngle > 40) manager.strHitType2 = "좌익선상 ";
            else if (ball.firstAngle > 30) manager.strHitType2 = "좌월 ";
            else if (ball.firstAngle > 10) manager.strHitType2 = "좌중월 ";
            else
            {
                if (ball.firstAngle < -40) manager.strHitType2 = "우익선상 ";
                else if (ball.firstAngle < -30) manager.strHitType2 = "우월 ";
                else if (ball.firstAngle < -10) manager.strHitType2 = "우중월 ";
                else manager.strHitType2 = "중월 ";
            }

        }






        //연출

        bool bZoomCameraSetting = false;
        /*
        public void setZoomCameraSetting(bool bActive, CPlayer player = null, SkillIndex skill = SkillIndex.BaesuJin, int focusIndex = -1, float dstFactor = 1, int focusType = 0, float timeRate = 0.8f)
        {
            if (bActive == true)
            {
                if (bZoomCameraSetting == false)
                {
                    if (player != null)
                    {
                        manager.instantFieldSkillEffect(player.getSkillValue(skill), true, player.getName());
                    }
                    setZoomTo(2.0f, 0.5f);

                   
                    //setFielderCharZoomRate(FieldParm.CHAR_ZOOM_SIZE_RATE);

                    //if (focusType == 1)
                    //{
                        //야수 포커스
                        //ball.setFielderFocus(focusIndex);
                    //}
                    //else if (focusType == 2)
                    //{
                        //주자 포커스
                        //ball.setRunnerFocus(focusIndex);
                    //}
                    //setTimeScale(Field.INIT_TIME_SCALE * timeRate);
                    //CameraManager.SetZoomActive(focusIndex, dstFactor);
                    bZoomCameraSetting = true;
                }
            }
            else
            {
                if (bZoomCameraSetting == true)
                {                    
                    //curZoom = 1.8f;
                    //setZoom(curZoom);
                    //setFielderCharZoomRate(FieldParm.CHAR_SIZE_RATE);
                    //setTimeScale(Field.INIT_TIME_SCALE);
                    //CameraManager.SetZoomDeActive();
                    bZoomCameraSetting = false;
                }
            }
        }*/


        private void setFielderCharZoomRate(float rate)
        {

            for (int i = 0; i < 9; i++)
            {
                fielder[i].transform.localScale = new Vector3(rate, rate, 1);
            }

            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    run.runner[i].transform.localScale = new Vector3(rate, rate, 1);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                judge.judge[i].transform.localScale = new Vector3(rate, rate, 1);
            }
        }

        public void setFieldCollisionEffect(float cx, float cy)
        {            
            setZoomTo(1.0f, 0.5f);
            Destroy(Util.Load("MainGame/prefabs/effect/fieldCrashPrefab", null, new Vector3(cx, cy, transform.position.z - 0.001f)), 1.2f);
            //야수간 충돌 연출            
            CameraManager.FieldCameraShake(0.2f, 10);
        }

        public void setErrorCollisionEffect(Vector3 pos, float gabX)
        {
            //GameObject obj = Util.Load("MainGame/prefabs/effect/errorCrashPrefab", transform, pos + new Vector3(gabX,0,0));
            //Vector3 ballPos = ball.transform.localPosition;
            //float x = pos.x + gabX;// (ballPos.x - pos.x) * 0.5f;
            //float y = pos.y;// +(ballPos.y - pos.y) * 0.5f;
            //obj.transform.localPosition = new Vector3(x, y, -0.0001f);
            //Destroy(obj, 1.2f);
            Destroy(Util.Load("MainGame/prefabs/effect/errorCrashPrefab", transform, pos + new Vector3(gabX, 0, 0)), 1.2f);
        }



        public bool setVsSkill(CPlayer offense, CPlayer defense, SkillIndex oIndex, SkillIndex dIndex, float delay1, float delay2)
        {
            /*CSkill offeseSkill = offense.getSkillValue(oIndex);
            CSkill defenseSkill = defense.getSkillValue(dIndex);
            bool bOffenseWin = SimulParm.checkOffenseSkillWin(offeseSkill.rank, defenseSkill.rank);
            StartCoroutine(setDelay(bOffenseWin, offeseSkill, defenseSkill, delay1, delay2));

            return bOffenseWin;*/

            return MyMath.Half();
        }

        private IEnumerator setDelay(bool bOffenseWin, CSkill offeseSkill, CSkill defenseSkill,  float delay1, float delay2)
        {
            IngameUI.GetVsSkillUI().init(0, manager.bMyTurn, offeseSkill.ID, offeseSkill.rank, defenseSkill.ID, defenseSkill.rank, bOffenseWin);
            yield return new WaitForSeconds(delay1);
            setTimeScale(0.01f);
            yield return new WaitForSeconds(delay2);
            setTimeScale(Field.INIT_TIME_SCALE);
        }
        



        //이닝 체인지 카메라
        public void setChangeInningCamera(float _remainTime)
        {
            if (bZoomCameraSetting == true)
            {
                setZoom(1.0f);
                setFielderCharZoomRate(FieldParm.CHAR_SIZE_RATE);
                setTimeScale(Field.INIT_TIME_SCALE);
                CameraManager.SetZoomDeActive();
                bZoomCameraSetting = false;
            }
            setZoomTo(0.7f, 1);

            int benchX = (int)(manager.bTopInning == false ? FieldSize.getAwayBenchPosX() : FieldSize.getHomeBenchPosX());

            if(ball.nBallX == 0 || ball.nBallY < getOriginY(FieldSize.getMoundPosY()))
            {
                ball.nEventX = ball.nBallX = FieldSize.getMoundPosX();
                ball.nEventY = ball.nBallY = getOriginY(FieldSize.getMoundPosY());
                setZoomTo(1, 1);
            }

            ball.setFocusMove(ball.nBallX, ball.nBallY, benchX, FieldSize.getMoundPosY(), BallEvent.EVENT_BASE_FOCUS, -1, _remainTime);
        }


        /////////////////////////////////////////////////////////////////////////
        //네트워크 필드 동기화
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 어떤쪽이 먼저 필드로 진입했는지 여부
        /// </summary>
        public bool bFieldFirst;

        /// <summary>
        /// 수비수 동기화
        /// 정확히 말하면 수비수의 송구방향을 동기화 한다
        /// </summary>
        /// <param name="fielder"></param>
        public void fielderSync(Fielder fielder, int oldTarget)
        {
            if (bFieldFirst == true)
            {
                //네트워크 전송
                PvpManager.GetInstance().SendFieldSyncInfo(FieldSyncType.Target, fielder.posIndex, nTargetIndex);
                fielder.throwTargetExceptioin();
                fielder.setState(FielderAction._MOTION, ActionStep._THROW_READY); //-> 캐치 딜레이의 타입을 여기서 결정
                fielder.curTime = 0;
            }
            else
            {
                //테스트 -->> 이부분만 남기면 끊어지는 테스트용
                //StartCoroutine(askGameSyncUpdateFieldScene());

                //진짜
                StartCoroutine(waitFielderSync(fielder, oldTarget));
            }
        }

        /// <summary>
        /// 송구방향 동기화를 위한 코루틴 프로세스
        /// </summary>
        /// <param name="fielder"></param>
        /// <param name="oldTarget"></param>
        /// <returns></returns>
        private IEnumerator waitFielderSync(Fielder fielder, int oldTarget)
        {
            if (netTarget[fielder.posIndex] == FieldParm.NoLink)//  fielder.nNetTargetIndex == -100)
            {
                Debug_UI.SetNetwork(true);
                setTimeScale(0);
            }
            int count1 = 0;
            //Debug.Log("===============================>> 네트워크로부터 던지는 방향 대기 fielder.posIndex = " + fielder.posIndex + "==== netTarget[fielder.posIndex] = " + netTarget[fielder.posIndex]);
            while (netTarget[fielder.posIndex] == FieldParm.NoLink) //fielder.nNetTargetIndex == -100)
            {
                ////Debug.Log("=========================wait");
                if (++count1 > 25)
                {
                    //수신중으로 표시하고 호스트와 완전 동기화로 처리
                    Debug.Log("필드 싱크 끊어짐 처리 -> 호스트의 결과를 게스트로 치환하던가");
                    netTarget[fielder.posIndex] = oldTarget;//fielder.nNetTargetIndex = oldTarget;
                    StartCoroutine(askGameSyncUpdateFieldScene());
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            Debug_UI.SetNetwork(false);
            setTimeScale(INIT_TIME_SCALE);

            //정상 루틴
            Debug.Log(" 네트워크로부터 받아온 송구 정보 = nNetTargetIndex = " + netTarget[fielder.posIndex]);//fielder.nNetTargetIndex);
            nTargetIndex = netTarget[fielder.posIndex];// fielder.nNetTargetIndex;
            netTarget[fielder.posIndex] = FieldParm.NoLink;// fielder.nNetTargetIndex = -100;

            fielder.throwTargetExceptioin();
            fielder.setState(FielderAction._MOTION, ActionStep._THROW_READY); //-> 캐치 딜레이의 타입을 여기서 결정
            fielder.curTime = 0;
            
        }

        /// <summary>
        /// 주자 동기화
        /// 엄밀히 말하면 베이스 도착시 한베이스 더가기 위해 알아야 하는 timeValue값을 동기화한다
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="oneMoreValue"></param>
        /// <returns></returns>
        public float runnerSync(Runner runner, float oneMoreValue)
        {
            //이건 방법이 없다. 슬립을 쓰자
            if (bFieldFirst == true)
            {
                PvpManager.GetInstance().SendFieldSyncInfo(FieldSyncType.OneMoreValue, runner.arrayIndex, oneMoreValue);
                return oneMoreValue;
            }
            else
            {
                /*//테스트 -->> 이부분만 남기면 끊어지는 테스트용
                StartCoroutine(askGameSyncUpdateFieldScene());
                return oneMoreValue;*/

                //진짜 
                int count1 = 0;

                while (netOneMoreValue[runner.arrayIndex] == FieldParm.NoLink)// runner.oneMoreBaseCheckValue == -100)
                {
                    if (++count1 > 25)
                    {
                        //수신중으로 표시하고 호스트와 완전 동기화로 처리
                        Debug.Log("주루 싱크 끊어짐 처리 -> 호스트의 결과를 게스트로 치환하던가");
                        StartCoroutine(askGameSyncUpdateFieldScene());
                        return oneMoreValue;
                    }
                    System.Threading.Thread.Sleep(100);
                }
                Debug.Log(" 네트워크로부터 받아온 한베이스 더 시간 = oneMoreBaseCheckValue = " + netOneMoreValue[runner.arrayIndex]);
                float value = netOneMoreValue[runner.arrayIndex];// runner.oneMoreBaseCheckValue;
                netOneMoreValue[runner.arrayIndex] = FieldParm.NoLink;// runner.oneMoreBaseCheckValue = -100;
                return value;//*/
            }
        }

        /*
        /// <summary>
        ///  ㅋㅋㅋ
        /// </summary>
        /// <returns></returns>
        private IEnumerator waitRunnerSync()
        {
            Debug_UI.SetNetwork(true);
            yield return new WaitForSeconds(1.0f);
            Debug_UI.SetNetwork(false);
        }*/

        /// <summary>
        /// 게임 동기화를 요청한후 동기화 완료시 업데이트 필드신을 호출한다
        /// </summary>
        /// <returns></returns>
        private IEnumerator askGameSyncUpdateFieldScene()
        {
            //Debug.Log("=================================>>askGameSyncUpdateFieldScene");
            setNetBufferInit();

            manager.simulator.setReSync();

            if (Mode.bOnlyChanceMode == true)
            {
                //찬스모드(개입)플레이시
                setTimeScale(0);
                Debug_UI.SetNetwork(true);
                
                yield return new WaitForSeconds(2.0f);

                manager.simulator.nextLineup(manager.offenseIndex, SimulResultState.NONE);
                if (manager.simulator.IsHost() == false)
                {
                    //재접속     호스트가 아닌 경우                    
                    manager.checkChanceModeEnd(SimulResultState.NONE);
                }
                else
                {
                    //호스트 인경우
                    //Debug.Log("==============================================>>호스트인데 접속이 끊김");
                    manager.simulator.setHost(false);   //호스트를 바꿈
                    PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.HostChange);
                    yield return new WaitForSeconds(2.0f);
                    manager.checkChanceModeEnd(SimulResultState.NONE);
                }

                yield return new WaitForSeconds(2.0f);
                Debug_UI.SetNetwork(false);
                setTimeScale(INIT_TIME_SCALE);
            }
            else
            {
                //계속된 직접 플레이시
                //동기화 요청 SendAskSync
                PvpManager.GetInstance().SendAskSync();

                setTimeScale(0);
                Debug_UI.SetNetwork(true);

                bool bLastMyTurn = manager.bMyTurn;

                yield return new WaitForSeconds(1.0f);

                int count1 = 0;
                while (PvpManager.syncState != PvpManager.SyncAskState.Done)
                {
                    if (++count1 > 30)
                    {
                        //접속 끊김 팝업 호출
                        Debug_UI.SetNetwork(false);
                        Debug.Log("접속 끊김 팝업 호출 -> 재접속루틴을 타야함");
                        if (manager.simulator.IsHost() == false)
                        {
                            //재접속     
                            manager.checkChanceModeEnd(SimulResultState.NONE);
                        }
                        yield break;
                    }
                    yield return new WaitForSeconds(0.3f);
                }
                //이부분에서 호스트와 동기화 시킴

                if (manager.bMyTurn != bLastMyTurn || manager.nOutCount >= 3)
                {
                    manager.bThreeOutChange = true;
                }

                manager.bMyTurn = bLastMyTurn;

                Debug.Log("동기화가 완료되면 아래부분을 실행");
                Debug_UI.SetNetwork(false);
                PvpManager.syncState = PvpManager.SyncAskState.None;
                setTimeScale(INIT_TIME_SCALE);
                StartCoroutine(updateFieldScene(0, true));
            }

        }


        private void setNetBufferInit()
        {
            for (int i = 0; i < 9; i++) netTarget[i] = FieldParm.NoLink;
            for (int i = 0; i < 4; i++) netOneMoreValue[i] = FieldParm.NoLink;
            for (int i = 0; i < 4; i++) netBaseSafe[i] = false;
        }
    }
}
