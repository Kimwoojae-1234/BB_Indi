//#define _TEST_RUNNER
//#define _TEST_NOSKILL
//#define _TEST_ONEMOREBASE
//#define _TEST_FIRSTBASE_OVERRUN
//#define _NOMOREBASE
//#define _INPUT_SKILL
//#define _TEST_HOMERUSH

using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public enum RunState
    {
        //주자 상태
        ADD_SCORE = 97,
        NOT_EXIST = 98,
        GO_BENCH = 99,
        STANDBY = 100,
        MOVE = 101,
        WAIT = 102,
        SLIDING = 103,
        CHECK = 104,
        RUSH = 105,
        BLOCKED = 106,
        DOUBLEPLAY = 107,
        STEAL = 108,
        PICKOFF = 109,
        GOODBYEHIT = 110,
        DO_NOTHING = 111,
        FIRSTBASE_SAFE = 112,
        SECOND_THIRD_SAFE = 113,
        REVERSE_DELAY = 114,    //역동작에 따른 딜레이
        NONE = -1
    }


    public class Runner : MonoBehaviour
    {   
        //테스트
#if _TEST_ONEMOREBASE
        public bool bTestOnemoreBase = false;
#endif
#if _TEST_FIRSTBASE_OVERRUN
        public bool bTestOverRun = true;
#endif
#if _TEST_RUNNER
        public const int testSpeed = 700;
#endif
        //상수
        //const float _RUNNER_MULTI = 1.15f;
        const float INIT_RUNNER_TIME_SCALE = 1.35f;// //타임스케일 상수

        //오브젝트
        public tk2dCamera _camera;       //카메라
        Field field;                    //필드    
        runnerManager run;              //주루 매니저

        public GameObject minimapRunner;

        //애니메이션
        public SkeletonAnimation anim;              //애니메이션 오브젝트
        public TextMesh runnerNameObj;
        public tk2dSprite errorMark;
        public string strID, lastStrID, _strID;     //애니메이션 이름
        float timeScale, timeScaleRate;
        bool loop;                                  //루프여부
        //모션 관련
        int nMotionStep;            //모션 스텝
        SlidingType slidingType;    //슬라이딩 관련
        RunnerOutMotion outMotion; //아웃되는 모션

        //주자 데이터
        public CPlayer pRunner;     //오브젝트
        public string runnerName;   //이름
        public int runnerRating;    //레이팅        
        public int runnerIndex;     //생성시마다 카운트 되는 인덱스
        public int arrayIndex;      //생성되는 배열의 인덱스
        public int lineupCount;

        //상태
        //public int nState, nLastState, nNextState;	    //주자의 상태	
        public RunState state, lastState, nextState;
        public int nDir, nLastDir;			            //러너의 방향
        public bool destroyCall;                        //파괴여부 : true 시 본 오브젝트를 파괴한다
        public bool bRunnerActive;                      //액티브 여부
        public bool bMoving;
        

        //Physics//////////////////////////////////////////////////////
        //Time
        public int nFrame;	//프레임		
        public float curTime, maxTime, deltaTime;        
        //벡터
        float startX, startY;
        public float posX, posY;		//주자 X,Y Pos
        public float dstX, dstY;        //목적 위치
        public float dX;				//주자 X 속도
        public float dY;				//주자 Y 속도
        public float curDX, curDY;
        public float turnDX, turnDY, turnSpeed;
        public float aX, aY, turnAccel;            //가속도    
        public float deadPosX;		    //주자가 죽은 X위치
        public float deadPosY;		    //주자가 죽은 Y위치	
        float screenX, screenY;         //스크린상 위치
        float baseDistance, baseAngle;  //베이스간 거리및 각도

        //능력치//////////////////////////////////////////////////////
        //1차 능력치
        public int runnerAbil;
        public float RUNNER_SPEED;	//주자 스피드 난이도 밸런스	
        public float RUNNER_DELAY;    //주자 대기
        public int RUNNER_AI;		//주AI 레벨 0,1,2	//0:최저, 1:중간: 2 최고
        //2차 능력치
        public float curSpeed;
        public float curDelay;
        public int curAI;
        float stealDelay;
        float pickOffDelay;
        public float accelRate;
        float accelTimeRate;
        float needMoreTime; //한베이스 스킬발동시 더 필요한 시간

        //베이스러닝//////////////////////////////////////////////////////
        //베이스 터닝 관련
        bool bBaseTurning;      //베이스 터닝 여부
        float turningDir;       //베이스 터닝 방향

        //베이스 관련 인덱스
        //생성 순서에 관련된 인덱스이지만 베이스 인덱스와연결되는 변수들
        public int currentPos;		//현재 포지션 0:홈 1:1루 2:2루 3:3루//-->현재 머무르는 루
        public int lastPos;         //-->최종적으로 머무른 루
        public int destPos;		    //목적 포지션 0:홈 1:1루 2:2루 3:3루
        public int destPos2;
        public bool bMoveForward;
        public bool bForcedOutBackMove; //뒤로 돌아가는 경우 포스 아웃이 걸림

        //도루 관련
        public bool bStealFlag;     //true인 경우 도루를 감행

        //주루 능력 관련
        public int baseRunningValue;

        //픽오프 관련
        public bool bPickOffFlag;

        //리드 관련
        public bool bLead;
        public int leadStep; //1~3

        //기타
        public bool bSlidingMotion; //슬라이딩 모션으로 들어왔는지 여부

        

        //플래그//////////////////////////////////////////////////////
        //주루 플래그
        public bool bRunFoward;         //진루여부
        public bool bOneBaseMore;       //한베이스 더 여부
        public bool bReCheckMoreBase;   //주루 지능과 연관 - 재판단...
        public bool bGrounderWaitFlag;  //땅볼시 대기 상황
        public bool baseTagPrepare;     //베이스택 상황
        public bool bClosePlay;         //접전 상황
        public bool bMoveEffect;        //move상황시 이펙트 붙여주는 여부
        public bool bLastPitcher;       //승계주자 여부
        public bool bForcedOneMoreBase, bForcedOneMoreBase2;        //강제 한베이스 더 진루     
        SimulOverrunState oneMoreSkillSense;        //한베이스 더 훔치는 주루 스킬
        //기록
        public bool bErrorRunner;       //에러로 출루 여부
        public bool bChangedRunner;     //바뀌었는지 여부
        //public bool bNotOutFlag;        //낫아웃으로 출루여부

        private bool bErrorCheck, bHitCheck;    //한번만 체크
         
        //오버런
        public SimulOverrunState overRunFlag;        //오버런 해서 아웃되는지 여부

        //스킬//////////////////////////////////////////////////////
        //이펙트
        GameObject skillEffect1, skillEffect2, lineEffect;
        bool bSkillEffectOn = false;
        int targetBase; //연출이 일어나는 베이스

        //스킬 - 패시브
        public byte sRunningSense;             //주루딜레이 감소, 판단 증가
        public byte sLead;                     //주자의 리드폭을 증가   : 지속 -> done
        public byte sSliding;                  //슬라이딩 스킬을 증가   : 지속 -> done
        
        //스킬 발동 플래그
        public bool bSlidingSkillOn;       //슬라이딩 스킬로 세잎이 되는 경우   //최대 7%    (접전시만 유용)
        public bool bRushSkillOn, bRushMustHappen;          //홈돌진 스킬로 세잎이 되는 경우     //최대 50%    
        public bool bTurboSkillOn;         //터보 스킬 온
        public bool bDoublePlaySkillOn;    //병살저지 스킬 온
        //public bool bDelayStealSkillOn;         //도루터보 스킬 온
        float slidingAddTagDelay;

        

        //리와인드 플레이//////////////////////////////////////////////////////
        public bool bRewindOverRunOut;
        bool bRewindSpeedSet;
        float rewindSetSpeed;
        bool bRewindBaseTagWait;


        //카메라
        public bool bCameraOn;



        ///////////////////////////////////////////////////////////////////
        //초기화 및 destroy
        ///////////////////////////////////////////////////////////////////
        void Awake()
        {
            bRunnerActive = false;
        }

        // Use this for initialization
        void Start()
        {
            bThrowErrorHappen = false;
            bErrorCheck = false;
            bHitCheck = false;
        }

        // Update is called once per frame
        void Update()//FixedUpdate()
        {
            if (bRunnerActive == true)
            {
                //if (field.bInputWait == true) return;
                deltaTime = field.getDeltaTime();
                nextFrame();
            }
        }

        //스켈레톤 데이터를 로딩
        string lastPath = null;
        public void loadRunner(CPlayer player, bool bTopInning)
        {
            GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/runner/runnerSkelPrefab", transform, new Vector3(0, 0, -0.01f), "skeleton");
            skeleton.transform.localScale = new Vector3(100, 100, 100);
            skeleton.transform.localEulerAngles = Vector3.zero;
            anim = skeleton.GetComponent<SkeletonAnimation>();

            int index = bTopInning ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex;
            int skin = (int)player.getSkin();
            AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
            Material[] materials = atlasdata.materials;

            string curPath = "MainGame/spineData/fieldingview/runner/team/" + index + "/" + skin + "/runnerAnim";
            if (curPath != lastPath)
            {
                lastPath = curPath;
                materials[0].mainTexture = (Texture)Resources.Load(curPath);
            }

            runnerNameObj.text = player.getName();
        }


        //배팅뷰로 돌아가기전 달라진 상태를 업데이트 해줌
        public void setUpdate()
        {
            ////UnityEngine.Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ RUNNER SETUPDATE  runnerIndex = " + runnerIndex);
            //////UnityEngine.//Debug.Log("==============>>runnerIndex = " + runnerIndex);
            //////UnityEngine.//Debug.Log("==============>>nHitterRunnerIndex = " + run.nHitterRunnerIndex);
            //배팅뷰로 돌아가기전에 함 호출함
            if (runnerIndex != run.nHitterRunnerIndex)
            {
                ////////UnityEngine.//Debug.Log("======================>> runner Set InitPos");
                if (currentPos == FieldParm.HOMEBASE_INDEX)
                {
                    //////UnityEngine.//Debug.Log("======================>> 홈인한 주자가 계속존재하는 경우 Destroy");
                    destroyRunner();
                    currentPos = -1;
                    return;
                }

                if (state == RunState.GO_BENCH) //if (nState == RunningMechnism.GO_BENCH)
                {
                    //////UnityEngine.//Debug.Log("======================>> 벤치로 향하고 있는 경우 Destroy");
                    destroyRunner();
                    currentPos = -1;
                    return;
                }

                /*
                setInitPos(currentPos, false, true);
                state = RunState.STANDBY;
                nDir = RunningMechnism.RunnerLeadDir[currentPos];
                setStopAnim();
                drawRunner();*/
            }

            bForcedOutBackMove = false;
            lastPos = currentPos;
            bLead = false;
            baseTagPrepare = false;
            //bForcedOut = false; //->필요없음 필더가 체크해줌
            bGrounderWaitFlag = false;
            outMotion = RunnerOutMotion._NORMAL;
            bRunnerActive = false;
            bStealFlag = false;
            
            bErrorCheck = false;
            bHitCheck = false;
            
            initSpecialFlag();

            setCamera(false);
            errorMark.gameObject.SetActive(false);
            bBenchAlready = false;
            //Util.ChangeLayersRecursively(transform, "FIELDINGVIEW_LAYER");

            overRunFlag = SimulOverrunState.NONE;

        }

        public void setRunnerInitPos()
        {
            if (runnerIndex != run.nHitterRunnerIndex)
            {
                setInitPos(currentPos, false, true);
                state = RunState.STANDBY;
                nDir = RunningMechnism.RunnerLeadDir[currentPos];
                setStopAnim();
                drawRunner();
            }
        }


        //시뮬레이션과 주자를 싱크 시키기
        public void setSync(int curPos)
        {
            currentPos = curPos;
            destroyCall = false;
            curTime = 0;
            setStandby(BaseArriveMotion._NORMAL);
        }


        //초기 세팅
        public void initSetting(CPlayer player, Field field, int index, int team, int arrayIndex)
        {
            timeScaleRate = Field.INIT_TIME_SCALE * 0.6f;
            timeScale = 1;
            lastTrack = track = 1;

            pRunner = player;
            this.field = field;
            this.run = field.run;

            initSetting2();
            initSkill(pRunner);

            if (index != -1) runnerIndex = index;
            this.arrayIndex = arrayIndex;

            destroyCall = false;

            //bForcedOut = false;//->필요없음 필더가 체크해줌
            bErrorRunner = false; //에러로 출루한 주자
            
            bRewindSpeedSet = false;

            curTime = 0;

            setCamera(false);
            errorMark.gameObject.SetActive(false);
            bBenchAlready = false;
            overrunState =  SimulOverrunState.NONE;
            overRunFlag = SimulOverrunState.NONE;
        }

        //초기 세팅2
        public void initSetting2()
        {
#if _OrthoCamera
            transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            transform.localEulerAngles = new Vector3(0, 0, 0);
#else
            transform.localScale = new Vector3(FieldParm.CHAR_SIZE_RATE, FieldParm.CHAR_SIZE_RATE, FieldParm.CHAR_SIZE_RATE);
            transform.localEulerAngles = new Vector3(CameraManager.FieldActiveAngleX, 0, 0);
#endif
            curTime = 0;
        }

        public void initSpecialFlag()
        {
            bBaseTurning = false;
            outMotion = RunnerOutMotion._NORMAL;
            bStealFlag = false;
            bPickOffFlag = false;
            bRushMustHappen = false;
            bForcedOneMoreBase = bForcedOneMoreBase2 =false;
            oneMoreSkillSense = SimulOverrunState.NONE;
            overrunState = SimulOverrunState.NONE;
        }

        //러너 디스트로이
        public void destroyRunner()
        {
            //리와인드인 경우 이곳으로 들어오지 않는다.
            ////UnityEngine.//Debug.Log("=================>>Runner :: destroyRunner 체크");
            run.runnerActive[arrayIndex] = false;
            TweenAlpha.Begin(minimapRunner, 0.3f, 0);
            Destroy(minimapRunner.gameObject,0.3f); //미니맵러너 삭제
            Destroy(gameObject);
        }


        public void setRunnerChange(CPlayer player)
        {
            pRunner = player;
            initSkill(pRunner);
            bChangedRunner = true;

            runnerName = player.getName();
            runnerRating = (int)(player.getSpeed());// / 10);

        }




        //리와인드 플레이시 주자 아웃 세잎
        public void setRewindRunnerSpeed(bool bOut, float timeLeft, int targetIndex)
        {
            //////UnityEngine.//Debug.Log("============================>>setRewindRunnerSpeed curSpeed= " + curSpeed);
            float distance = 0;
            float targetX = field.getOriginX(FieldSize.getBasePosX(targetIndex));
            float targetY = field.getOriginY(FieldSize.getBasePosY(targetIndex));

            if(targetIndex == destPos)
            {
                //////UnityEngine.//Debug.Log("============================>>현재 해당 타겟 베이스로 달리고 있는 중인 경우");
                bRewindSpeedSet = false;
                distance = RunningMechnism.getDistance(targetX - posX, targetY - posY);
                //////UnityEngine.//Debug.Log("============================>>주자 거리 : "+distance +"    주자 속도 "+curSpeed);
            }
            else
            {
                //////UnityEngine.//Debug.Log("============================>>현재 해당 타겟 베이스가 송구가 향하는 베이스보다 한베이스 적은 경우");
                bRewindSpeedSet = true;                
                distance = RunningMechnism.getDistance(targetX - dstX, targetY - dstY);
            }
                        
            if (bOut == true)
            {
                curSpeed = distance / (timeLeft+1);
                RUNNER_SPEED = curSpeed;
            }
            else
            {
                curSpeed = distance / (timeLeft-0.1f);
                curSpeed *= 1.2f;
            }
            //////UnityEngine.//Debug.Log("============================>>setRewindRunnerSpeed 변환후 curSpeed= " + curSpeed);

            dX = curSpeed * Mathf.Cos(baseAngle);
            dY = curSpeed * Mathf.Sin(baseAngle);

            bRewindOverRunOut = false;
            rewindSetSpeed = curSpeed;

        }

        //카메라
        public void setCamera(bool bActive)
        {
            _camera.gameObject.SetActive(bActive);
            bCameraOn = bActive;
        }

        private bool checkRunnerOnScreen()
        {
            if (field.ball.step == BallStep.BALL_HIT) return false;

            float x = field.ball.nBallX;
            float y = field.ball.nBallY;

            if (Mathf.Abs(posX - x) < BallPlayManager.m_lcdWC && Mathf.Abs(posY - y) < BallPlayManager.m_lcdHC)
            {
                return true;
            }

            return false;
        }


        ///////////////////////////////////////////////////////
        //Runner Frame 함수
        ///////////////////////////////////////////////////////
        //Standby 상황에서 액티브 스킬 체크
        /*private void checkActiveSkillOnStandBy()
        {            
            if (bDelayStealSkillOn == true)
            {
                if (field.bFieldStealFlag == true)
                {
                    if (field.ball.step == BallStep.BALL_THROW)
                    {
                        if (currentPos == FieldParm.THIRDBASE_INDEX)
                        {
                            if (run.bHomeSteal == false && run.bSecondBaseSteal == true)
                            {
                                if (field.bFieldViewActive == true)//if (field.manager.playState == PlayState.PLAY_FIELDING_VIEW)
                                {
                                    //UnityEngine.Debug.Log("############### 스킬 딜레이 스틸 SKILL 여기서 발동");
                                    Fielder Cover = field.fielder[CPlayer._CATCHER];
                                    Cover.posX = field.getOriginX(FieldSize.getBasePosX(FieldParm.HOMEBASE_INDEX));
                                    Cover.setStop();

                                    bDelayStealSkillOn = false;
                                    float delay = 0.3f;
                                    StartCoroutine(activeDelaySteal(delay));
                                }
                            }
                        }
                    }
                }
            }
        }*/

        //Runner Standby
        void standbyFrame()
        {
            //리와인드시 작동 안함
            //checkActiveSkillOnStandBy();
        }

        //Runnner Check
        void checkFrame()
        {
            //////////UnityEngine.//Debug.Log("================>> runner checkFrame curTime = " + curTime + "    RUNNER_DELAY = " + RUNNER_DELAY);
            if (curTime >= curDelay)
            {
                //////////UnityEngine.//Debug.Log("================>> curTime>RUNNER_DELAY :: nNextState = " + nNextState);
                switch (nextState)
                {
                    case RunState.MOVE:
                        setMoveAnim(RunState.MOVE);
                        curTime = 0;
                        break;
                    case RunState.WAIT:
                        setMoveAnim(RunState.WAIT);
                        //setStopAnim();
                        curTime = 0;
                        break;

                }
            }
        }

        //Move 상황에서 액티브 스킬 체크
        private void checkActiveSkillOnMoving()
        {
            if (field.bFieldStealFlag == true)
            {
                if(SimulSteal.runnerStealMarster == FieldSkillUse.Active)
                {
                    if (run.bHomeSteal == false)
                    {
                        if (field.bFieldViewActive == true)//if (field.manager.playState == PlayState.PLAY_FIELDING_VIEW)
                        {
                            if (field.stealSuccess == false)
                            {
                                posX -= (dX * 0.4f);
                                posY -= (dY * 0.4f);
                                SimulSteal.runnerStealMarster = FieldSkillUse.Fail;
                            }
                            else
                            {
                                posX -= (dX * 0.3f);
                                posY -= (dY * 0.3f);
                                SimulSteal.runnerStealMarster = FieldSkillUse.Success;
                            }
                            setTurboOn(true);
                        }
                    }
                }
            }
            else
            {
                if (bTurboSkillOn == true)
                {
                    if (field.bGrounderAvailble == true
                    //&& field.batter.bSpecialBattingOn == false
                    && (run.bOnRunning[FieldParm.SECONDBASE_INDEX] == false && run.bOnRunning[FieldParm.HOMEBASE_INDEX] == false)
                    && field.bFirstThrow == false
                    && field.groundCatchFielder != CPlayer._PITCHER
                    && field.ball.firstAngle > -10
                    && field.manager.fieldOutCountNum == 0)
                    {
                        if (curTime > 0.0f && curTime < 0.5f)
                        {
                            setTurboOn();
                            bTurboSkillOn = false;
                        }
                    }
                }
            }
        }

        //Runnner Move
        void moveFrame()
        {
            //Debug.Log("@@@@@@@@4");
            field.bReturnBattingView = false;
            if (bBaseTurning == true)
            {
                if (currentPos == FieldParm.SECONDBASE_INDEX)
                {
                    //리와인드시 터닝 없음
                    setDestinationTurning();
                    bBaseTurning = false;
                }
                else
                {
                    //리와인드가 아닌 경우 터닝 처리
                    turningDir += (60 * Mathf.Deg2Rad) * deltaTime;

                    nDir = FieldParm.getDir(turningDir);
                    turnDX = turnSpeed * Mathf.Cos(turningDir);
                    turnDY = turnSpeed * Mathf.Sin(turningDir);

                    if (field.bInputWait == false)
                    {
                        accelTimeRate = timeScaleRate / INIT_RUNNER_TIME_SCALE;

                        turnSpeed += (turnAccel * accelTimeRate);
                        if (turnSpeed > curSpeed) turnSpeed = curSpeed;
                    }

                    posX += (turnDX * deltaTime); //posX += (dX * deltaTime);
                    posY += (turnDY * deltaTime); //posY += (dY * deltaTime);

                    if (curTime > maxTime)
                    {
                        setDestinationTurning();
                        bBaseTurning = false;
                    }
                }
            }
            else
            {
                checkActiveSkillOnMoving();

                if (field.bInputWait == false)
                {
                    ////UnityEngine.Debug.Log("timeScaleRate = " + timeScaleRate);
                    //아닌 경우 가속 있음
                    accelTimeRate = timeScaleRate / INIT_RUNNER_TIME_SCALE;
                    curDX += (aX * accelTimeRate);
                    curDY += (aY * accelTimeRate);

                    if (Mathf.Abs(curDX) > Mathf.Abs(dX)) curDX = dX;
                    if (Mathf.Abs(curDY) > Mathf.Abs(dY)) curDY = dY;                   
                }

                posX += (curDX * deltaTime); //posX += (dX * deltaTime);
                posY += (curDY * deltaTime); //posY += (dY * deltaTime);

                //bool ballOnBase = checkBallOnDestBase();
                //out 체크 true시 return - 봉살 태그
                if (checkForcedOut() == true ||
                    checkForceBackOut() == true)
                {
                    //////UnityEngine.//Debug.Log("======================>>이중 체키 ㅋㅋㅋ 봉살 아웃");
                    //outText();
                    setRunnerBench(true, false, true);
                    field.setGroundOut();
                    field.setThrowAvailabe(); //포스 아웃인 경우 다시 던짐 가능
                    return;
                }
                if (checkSlidingRange(posX, dstX) == true)//  if (Mathf.Abs(posX - dstX) <= SLIDING_RANGE)
                {
                    //bool ballToBase = checkBallToDestBase();
                    //ballToBase = field.ball.checkThrowingToThisBase(destPos);
                    //////UnityEngine.//Debug.Log("======================>>슬라이딩 레인지 runner index = " + runnerIndex);
                    if (checkSlidingNeeded() == true)//ballOnBase, ballToBase) == true)
                    {
                        if (field.bInputWait == true) return; //인풋 대기시 슬라이딩 보류

                        if (bRushSkillOn == true &&  field.bFieldStealFlag == false)// && bForcedOut == false)
                        {
                            //UnityEngine.Debug.Log("#############홈 돌진 체크#############");
                            setRushAnim();
                        }
                        else if (bDoublePlaySkillOn == true && field.bFieldStealFlag == false)
                        {
                            //UnityEngine.Debug.Log("#############병살저지 체크#############");
                            setDoblePlayAnim();
                        }
                        else
                        {
                            setSlidingAnim(slidingType);
                        }
                    }
                    else
                    {
                        //////////UnityEngine.//Debug.Log("======================>>슬라이딩 안함 runner index = " + runnerIndex);
                        if (checkBaseRange(posX, dstX) == true)
                        {                            
                            if (checkRunnerOut() == true)
                            {
                                //outText();
                                setRunnerBench(false, true, true);
                                field.setGroundOut();
                                checkBaseHitButOut();
                                return;
                            }
                            else
                            {
                                setArriveBase();
                            }
                        }

                    }
                }

            }
        }

        //Runnner Wait
        void waitFrame()
        {
            ////////UnityEngine.//Debug.Log("=================waitFrame : curTime = " + curTime+"    maxTime = "+maxTime);
            if (curTime < maxTime)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);
            }
            else
            {
                if (_strID != RunningMechnism._HOLD)
                {
                    //////UnityEngine.//Debug.Log("============>HOLD");
                    setStopAnim();
                    playAnim();
                }

                baseAngle = RunningMechnism.getAngle(field.ball.nBallX - posX, field.ball.nBallY - posY);
                nDir = FieldParm.getDir(baseAngle);



                ////////UnityEngine.//Debug.Log("============>nDir = " + nDir);
            }
        }

        //Runnner Sliding
        SimulOverrunState overrunState;
        float slidingTime;
        void slidingFrame(bool bDoublePlaySkillActive = false)
        {
            //Debug.Log("@@@@@@@@5");
            slidingTime += deltaTime;
            //bool bForceOut = false;
            field.bReturnBattingView = false;

            if (bDoublePlaySkillActive == true)
            {
                posX += (dX * 1.5f * deltaTime);
                posY += (dY * 1.5f * deltaTime);
            }
            else
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);
            }

            if (overrunState != SimulOverrunState.NONE )
            {
                //오버런 주자         
                if (overrunState == SimulOverrunState.OUT)
                {
                    StartCoroutine(setErrorMark(0.001f));
                }
                else
                {
                    errorMark.gameObject.SetActive(false);
                }
                overrunState = SimulOverrunState.NONE;
            }

            if (bClosePlay == true && destPos == FieldParm.HOMEBASE_INDEX)
            {
                //////UnityEngine.//Debug.Log("===============>>홈 접전 슬라이딩");
                posY += (0.3f * dY * deltaTime);
            }

            if (bSlidingSkillOn == true)
            {
                ////UnityEngine.//Debug.Log("===============>>슬라이딩 스킬 발동시");
                if (bClosePlay == true)
                {
                    //field.manager.gameUI.setSkillBox(true, "아크로베틱"); //[UI]주루 스킬 슬라이등 네임박스
                }
                int coverIndex = field.getBaseCoverIndex(destPos);
                if (coverIndex != -1)
                {
                    field.fielder[coverIndex].addTaggingDelay(slidingAddTagDelay);
                }
                bSlidingSkillOn = false;
            }


            if (checkBaseRange(posX, dstX) == true) 
            {
                /*
                if (bClosePlay == true && bStealFlag == false)
                {
                    ////UnityEngine.Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&젠장 여기");
                    //field.ball.setBaseFocus(targetBase);
                }*/

                
                //새버전 : 병살저지는 무조건 아웃되게 고친 버전
                if (bDoublePlaySkillActive == true)
                {
                    //outText();
                    setSpecialAnim("2030_SLIDING_DOUBLEPLAY", false);
                    setRunnerBench(false, true, true, true);
                    field.setGroundOut();
                    checkBaseHitButOut();
                    return;
                }
                else
                {
                    if (checkRunnerOut() == true)
                    {
                        //////UnityEngine.//Debug.Log("=======================================================>>주자 슬라이딩하다 아웃");
                        if (destPos == FieldParm.HOMEBASE_INDEX && run.bHomeSteal == true && field.bFieldStealFlag == true)
                        {
                            field.fielder[CPlayer._CATCHER].setCatcherCollision(true);
                            setRunnerCrushFail();
                        }
                        else
                        {
                            //////UnityEngine.//Debug.Log("=======================================================>>주자 슬라이딩하다 아웃 최종여기");
                            //outText();
                            setRunnerBench(false, true, true);
                            field.setGroundOut();
                            checkBaseHitButOut();
                        }
                        return;
                    }
                    else
                    {
                        //////UnityEngine.//Debug.Log("=======================================================>>주자 슬라이딩하다 세잎");
                        setArriveBase(true);
                    }
                }//여기까지
            }
            /*else
            {
                //슬라이딩 먼지
                if (bDoublePlaySkillActive == true)
                {
                    if (slidingTime > 0.25f)
                    {
                        Vector3 pos = new Vector3(transform.position.x - 73, transform.position.y + 38, transform.position.z + 0.1f);
                        GameObject slidingEffect = Instantiate(Resources.Load("MainGame/prefabs/effectPrefab/field/dpSkidPrefab"), pos, Quaternion.identity) as GameObject;
                        Destroy(slidingEffect, 1.0f);
                        slidingTime = 0;
                    }
                }
                else
                {
                    makeDust(0.1f);
                }
            }*/
        }

        //Runnner Crush Fail
        public void setRunnerCrushFail(bool bBlockCase = true)
        {
            //field.fielder[CPlayer._CATCHER].checkCatcherBlockActive();

            int percent = MyMath.Percent();
            ////Debug.Log("======================>>덤블링 퍼센트 " + percent);
            bool bBlockedDumbling = (percent > 70 ? true : false);

            bBenchAlready = true;
            state = RunState.BLOCKED;
            curTime = 0;
            if (bBlockedDumbling == true)
            {
                setSpecialAnim("2020_SLIDING_HEAD_HOMEOUT2", false);
                dX = curSpeed / 5;
                dY = -curSpeed / 5;
                nMotionStep = 1;
            }
            else
            {
                setSpecialAnim("2040_SLIDING_CRASH_FAIL", false);

                if (bBlockCase == true)
                {
                    nMotionStep = 1;
                    //nDir = FieldParm._NORTHEAST;
                    dX = 0;// -0.5f * RUNNER_SPEED * Mathf.Cos(FieldParm._angleDir[nDir] * Mathf.Deg2Rad);
                    //dY = -0.5f * RUNNER_SPEED * Mathf.Sin(FieldParm._angleDir[nDir] * Mathf.Deg2Rad);
                }
                else
                {
                    nMotionStep = 100;
                    dX = 0;
                    dY = -0.5f * RUNNER_SPEED;
                }
            }
        }

        //Runnner Crush Success
        public void setRunnerCurshSuccess()
        {
            setSpecialAnim("2040_SLIDING_CRASH_SUCCESS", false);
            state = RunState.BLOCKED;
            dX = 0.4f * dX;
            dY = 0.6f * dY;
            nMotionStep = 2;
            curTime = 0;
        }

        //Runner Rush
        void rushFrame()
        {
            //Debug.Log("@@@@@@@@6");
            field.bReturnBattingView = false;
            slidingTime += deltaTime;
            //bool bForceOut = false;
            //field.bReturnBattingView = false;

            posX += (dX * deltaTime);
            posY += (dY * deltaTime);


            if (nMotionStep == 0)
            {
                if ((Mathf.Abs(posX - dstX) <= RunningMechnism.SLIDING_RANGE))
                {
                    setSpecialAnim("2040_SLIDING_CRASH", false);
                    nMotionStep = 1;
                }
            }

            if (checkBaseRange(posX, dstX) == true) 
            {
                destroyEffect();
                
                //블로킹 충돌 연출

                if (checkRunnerOut() == true) //포수가 블록킹을 실행한경우
                {
                    ////UnityEngine.//Debug.Log("=============================>>>블럭 돌진 대결");
                    field.runnerHomeRush = FieldSkillUse.Fail;
                    Fielder catcher = field.fielder[CPlayer._CATCHER];

                    if(catcher.pFielder.skillAvailable(SkillIndex.CatcherRunnerBlocking) == true) 
                    {
                        //홈돌진 대항                        
                        bool bOffenseWin = bHomeRushWin;

                        if (bOffenseWin == true)
                        {
                            //주자블록 vs 홈돌진
                            field.runnerHomeRush = FieldSkillUse.Success;
                        }
                        else
                        {
                            field.runnerHomeRush = FieldSkillUse.Fail;
                        }
                    }
                    else
                    {
                        field.runnerHomeRush = FieldSkillUse.Success;
                    }

                    if (field.runnerHomeRush == FieldSkillUse.Fail)
                    {
                        catcher.setCatcherCollision(true);
                        setRunnerCrushFail();
                    }
                    else
                    {
                        //러쉬 아웃 포수 튕겨냄
                        catcher.setCatcherCollision(false);
                        setRunnerCurshSuccess();
                    }

                    CameraManager.CameraShake(0.5f, 20);

                    return;
                }
                else
                {
                    //무조건 주자가 이긴경우로 처리
                    ////Debug.Log("====================>>> 이건 원래 세이프이므로 주자 블럭은 발생하지 않는다");
                    //러쉬 아웃 포수 튕겨냄
                    field.fielder[CPlayer._CATCHER].setCatcherCollision(false);
                    setSpecialAnim("2040_SLIDING_CRASH_SUCCESS", false);
                    state = RunState.BLOCKED;
                    dX = 0.4f * dX;
                    dY = 0.6f * dY;
                    nMotionStep = 2;
                    curTime = 0;

                    CameraManager.CameraShake(0.5f, 20);
                }
            }
        }

        //Runner Blocked
        void blockFrame()
        {
            //Debug.Log("@@@@@@@@7");
            field.bReturnBattingView = false;
            //curTime += field.getDeltaTime();

            if (nMotionStep == 1)
            {
                //홈돌진 아웃
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);
                dX *= 0.98f;
                dY *= 0.98f;
                if (curTime > 1.25f)
                {
                    setRunnerBench(false, true, true);
                    field.setGroundOut();
                    checkBaseHitButOut();
                }
            }
            else if (nMotionStep == 2)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);
                dX *= 0.93f;
                dY *= 0.93f;
                //홈돌진 성공
                if (curTime > 2.0f)
                {
                    field.checkSafeCall(FieldParm.HOMEBASE_INDEX);
                    setArriveBase(true);
                }
            }
            //기타 처리
            else if (nMotionStep == 100)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);
                dX *= 0.98f;
                dY *= 0.98f;
                if (curTime > 1.0f)
                {
                    setRunnerBench(false, true, true);
                }
            }
        }

        //Runner Bench
        void benchFrame()
        {
            //////UnityEngine.//Debug.Log("===============================>>benchFrame~~!!! curTime = "+curTime+" / maxTime = "+maxTime);
            posX += (dX * deltaTime);
            posY += (dY * deltaTime);

            if (outMotion == RunnerOutMotion._FIRSTBASE_RUN_OUT)
            {
                if (nMotionStep == 0)
                {
                    if (field.runnerTurbo == FieldSkillUse.Active)
                    {
                        dX *= 1.02f;
                        dY *= 1.02f;
                    }

                    if (checkBaseRange(posX, dstX) == true) 
                    {
                        //UnityEngine.//Debug.Log("======================>>죽을 똥을 다해 살려고 발버둥치는 애니메이션 추가");
                        nMotionStep = 1;
                        
                    }
                    //makeDust(0.2f);
                }
                else
                {
                    dX *= 0.975f;
                    dY *= 0.975f;
                }
                
            }
            else if (outMotion == RunnerOutMotion._SECONDBASE_SKILL_OUT)
            {
                if (nMotionStep == 0)
                {
                    if (checkBaseRange(posX, dstX) == true) 
                    {
                        nMotionStep = 1;
                    }
                    //makeDust(0.2f);
                }
                else
                {
                    dX *= 0.94f;
                    dY *= 0.94f;
                }
            }
            else if (outMotion == RunnerOutMotion._SECONDBASE_SLIDING_OUT || outMotion == RunnerOutMotion._FIRSTBASE_SLIDING_OUT || outMotion == RunnerOutMotion._THIRDBASE_SLIDING_OUT)
            {
                if (nMotionStep == 0)
                {
                    if (checkBaseRange(posX, dstX) == true) 
                    {
                        dX = dY = 0;
                        nMotionStep = 1;
                        curTime = 0;
                    }
                    //makeDust(0.2f);
                }
                else if (nMotionStep == 1)
                {
                    if (curTime > 0.5f)
                    {
                        //추후 일어나는 모션으로 수정
                        setSpecialAnim(RunningMechnism._HOLD + FieldParm._dir[nDir], false);
                        nMotionStep = 2;
                    }
                }
            }
            else if (outMotion == RunnerOutMotion._HOMEBASE_SLIDING_OUT)
            {
                if (nMotionStep == 0)
                {
                    if (checkBaseRange(posX, dstX) == true)
                    {
                        dX = dY = 0;
                        nMotionStep = 1;
                    }
                    //makeDust(0.2f);
                }
                else if (nMotionStep == 99)
                {
                    dX *= 0.98f;
                    dY *= 0.98f;
                }
            }

            if (curTime > maxTime)
            {
                //////UnityEngine.//Debug.Log("===============================================>>bNotBenchYet = " + bNotBenchYet);
                if (bNotBenchYet == true)
                {
                    bNotBenchYet = false;
                    setBench(RunState.GO_BENCH, RunnerOutMotion._NORMAL, 1.0f);
                }
                else
                {
                    //////UnityEngine.//Debug.Log("===============>>1");
                    if (destroyCall == false)
                    {
                        //////UnityEngine.//Debug.Log("===============>>2");
                        if (field.forcedSetBattingView(0.7f) == false)
                        {
                            if (run.bPickOff == true || run.bStealBase == true)
                            {
                                field.returnCheck_Steal_Pickoff(0);
                            }
                            else
                            {
                                //////UnityEngine.//Debug.Log("===============>>3");
                                field.returnCheck(0);
                            }
                        }
                        destroyCall = true;
                        destroyRunner();
                    }
                }
            }
        }

        //Runner FirstBase Safe
        void firstBaseSafeFrame()
        {
            //offsetY = -30;
            posX += (dX * deltaTime);
            posY += (dY * deltaTime);

            if (nMotionStep == 0)
            {
                if (field.runnerTurbo == FieldSkillUse.Success)
                {
                    dX = RUNNER_SPEED * 1.35f * Mathf.Cos(baseAngle);
                    dY = RUNNER_SPEED * 1.35f * Mathf.Sin(baseAngle);
                    field.runnerTurbo = FieldSkillUse.Init;
                }

                if (field.bOutCalled == true || field.ball.bFairBall == false)
                {
                    //살았는데 별로 안 좋은 상황
                    //setSpecialAnim(RunningMechnism._FIRSTOUT_TYPE1, false);
                    setSpecialAnim(RunningMechnism._RUN + FieldParm._dir[FieldParm._NORTHEAST], true);
                    nMotionStep = 2;
                    curTime = 0; ;
                }
                else
                {
                    //살았는데 별로 안 좋은 상황
                    /*if (field.nTargetIndex == FieldParm.FIRSTBASE_INDEX
                    && (run.bBallOnBase[FieldParm.FIRSTBASE_INDEX] == true || field.ball.nBallStep == FBall.BALL_THROW_CATCH))
                    {
                        ////////UnityEngine.//Debug.Log("======================>>공이 1루로 뿌려지고 있는 경우");
                        setSpecialAnim(RunningMechnism._FIRSTSAFT_TYPE1, false);
                    }
                    else
                    {
                        ////////UnityEngine.//Debug.Log("======================>>공이 1루로 뿌려지지 않는 경우");
                        setSpecialAnim(RunningMechnism._FIRSTSAFT_TYPE1, false);
                    }*/
                    //////UnityEngine.//Debug.Log("======================>>1루에서 좋아 죽음");
                    setSpecialAnim(RunningMechnism._FIRSTSAFT_TYPE1, false);
                    nMotionStep = 1;
                }
                //makeDust(0.2f);
            }

            dX *= 0.98f;
            dY *= 0.98f;

            if (nMotionStep >= 1)
            {
                if (curTime > (nMotionStep == 1 ? 1.3f : 0.5f))
                {
                    //////UnityEngine.//Debug.Log("======================>>이제그만");
                    dX = dY = 0;
                    setSpecialAnim(RunningMechnism._HOLD + FieldParm._dir[FieldParm._NORTHEAST], true);
                    nMotionStep = 3;
                    if (run.bWildPitchRunning == true && field.wildPitchCase == FieldParm.WildPitchCase.NotOut)
                    {
                        field.returnCheck(2.5f);
                    }
                }
            }
        }

        //Runner SecondBase
        void secondBaseFrame()
        {
            if (nMotionStep == 0)
            {
                setSpecialAnim(currentPos == FieldParm.SECONDBASE_INDEX ? RunningMechnism._SECOND_ARRIVE : RunningMechnism._THIRD_ARRIVE, false);
                nMotionStep = 1;
            }
        }

        //Runner AddScore
        void addScoreFrame()
        {
            //정확히 벤치로 향하거나 혹은 향하는 도중 쓱 없어지게 처리할 것~~~

            posX += (dX * deltaTime);
            posY += (dY * deltaTime);

            if (curTime > maxTime)
            {
                /* destroyCall이 들어오기 전에 주자의 득점 기록 처리*/
                if (destroyCall == false)
                {
                    field.returnCheck(field.bHomerunCeremony?4:3.5f);
                    //////UnityEngine.//Debug.Log("===========================>>여기여기");
                    destroyCall = true;
                    destroyRunner();
                }
            }


        }

        //Runner Steal
        public void stealFrame()
        {            
            if (curTime > stealDelay)
            {
                //////UnityEngine.//Debug.Log("========================>>도루 능력치가 먼저냐?");
                curTime = 0;
                RUNNER_SPEED = RunningMechnism.getRunnerSpeed(RunningMechnism.STEAL_SPEED);// *Runner._RUNNER_MULTI;
                curSpeed = RUNNER_SPEED;
                accelRate = RunningMechnism.STEAL_ACCEL;// / Runner._RUNNER_MULTI;
                setDestination(true);
                state = RunState.MOVE;
                setMoveAnim(RunState.MOVE);
            }
        }

        //pickOffFrame
        public void pickoffFrame()
        {
            if (nMotionStep == 0)
            {
                if (curTime > pickOffDelay)
                {
                    curTime = 0;
                    setMoveAnim(RunState.PICKOFF);
                    nMotionStep = 1;
                    playAnim();
                }
            }
            else
            {
                if (field.bInputWait == false)
                {
                    curDX = dX;
                    curDY = dY;
                }

                posX += (curDX * deltaTime); //posX += (dX * deltaTime);
                posY += (curDY * deltaTime); //posY += (dY * deltaTime);

                if (nMotionStep == 1)
                {
                    //뛰는 상태 
                    if (Mathf.Abs(posX - dstX) <= (RunningMechnism.SLIDING_RANGE * 0.66f))
                    {
                        //////UnityEngine.//Debug.Log("================>>여기로 들어오냐?");
                        nMotionStep = 2;
                        _strID = RunningMechnism._HEADSLIDING;
                        strID = _strID + FieldParm._dir[nDir];
                        loop = false;
                        playAnim();
                    }
                }
                else if (nMotionStep == 2)
                {
                    slidingTime += field.getDeltaTime();
                    //슬라이딩 상태
                    if (checkBaseRange(posX, dstX) == true)
                    {
                        //##연출 주자 견제 아웃                        
                        if (checkRunnerOut() == true)
                        {
                            //SimulManager.AddGameSummuryInfo("\n-" + (currentPos + 1) + "루주자 " + pRunner.getName() + ": 견제 아웃");
                            setRunnerBench(false, true, true);
                            field.setGroundOut();
                            
                            return;
                        }
                        else
                        {
                            //setArriveBase(true);
                            StartCoroutine(setBaseHoldAfterPickOff(currentPos, 0.5f));
                        }
                        nMotionStep = 3;
                        curDX = dX = curDY = dY = 0;
                    }
                    //makeDust(0.1f);
                }
            }
        }


        //reverseDelay Frame
        private void reverseDelayFrame()
        {
            if (curTime > maxTime)
            {
                setDestination(false, false);
                setMoveAnim(RunState.MOVE);
            }
            else
            {
                field.returnCheckInit();
            }

        }

        //Runner Frame
        public void nextFrame()
        {
            nFrame++;
            curTime += deltaTime;
            bMoving = false;
            ////UnityEngine.//Debug.Log("===================>>runner nextFrame");

            if (bCameraOn == true)
            {
                if (checkRunnerOnScreen() == true)
                {
                    //UIFieldCamera.SetActive(false, this, field.manager);              
                    setCamera(false);
                }
            }


            switch (state)
            {
                //case RunState.STANDBY:// = 100;
                //    standbyFrame();
                //    break;
                case RunState.CHECK:
                    //////////UnityEngine.//Debug.Log("==============>>RUNNER_CHECK : index"+runnerIndex);
                    bMoving = (curTime<5.0f?true:false);//bMoving = true;
                    checkFrame();
                    break;
                case RunState.MOVE:
                    // //////////UnityEngine.//Debug.Log("==============>>RUNNER_MOVE");
                    bMoving = true;
                    moveFrame();
                    break;
                case RunState.WAIT:
                    ////////////UnityEngine.//Debug.Log("==============>>RUNNER_WAIT");
                    bMoving = (curTime<5.0f?true:false);
                    waitFrame();
                    break;
                case RunState.SLIDING:
                    ////////////UnityEngine.//Debug.Log("==============>>RUNNER_SLIDING");
                    bMoving = true;
                    slidingFrame();
                    break;
                case RunState.RUSH:
                    bMoving = true;
                    rushFrame();
                    break;
                case RunState.BLOCKED:
                    blockFrame();
                    break;
                case RunState.DOUBLEPLAY:
                    bMoving = true;
                    slidingFrame(true);
                    break;
                case RunState.STEAL:
                    stealFrame();
                    break;
                case RunState.PICKOFF:
                    pickoffFrame();
                    break;
                case RunState.GOODBYEHIT:
                    break;
                case RunState.GO_BENCH:
                    benchFrame();
                    break;
                case RunState.ADD_SCORE:
                    ////////////UnityEngine.//Debug.Log("==============>>RUNNER_ADD_SCORE");
                    addScoreFrame();
                    break;
                case RunState.DO_NOTHING:
                    break;
                case RunState.FIRSTBASE_SAFE:
                    firstBaseSafeFrame();
                    break;
                case RunState.SECOND_THIRD_SAFE:
                    secondBaseFrame();
                    break;
                case RunState.REVERSE_DELAY:
                    bMoving = true;
                    reverseDelayFrame();
                    break;
            }

            drawRunner();
        }
        ///////////////////////////////////////////////////////
        //Transform 관련 함수
        ///////////////////////////////////////////////////////
        //Y축 옵셋
        float getOffsetY()
        {
            if (state == RunState.FIRSTBASE_SAFE)
            {
                return RunningMechnism.offsetY[0];
            }
            else
            {
                return RunningMechnism.offsetY[destPos];
            }
        }

        //러너 포지션
        public void setPosition()
        {

            screenX = field.getScreenX(posX);
            screenY = field.getScreenY(posY);// +getOffsetY();
            float depthZ = 0;// 
#if OrthoCamera
            depthZ = -4 + (posY * 0.0002f);
#endif
            //float scale = field.getScale(posX, posY);
            transform.localPosition = new Vector3(screenX, screenY, depthZ);
            //transform.localScale = new Vector3(scale, scale, 1);
        }

        //비율
        private void setRatio()
        {
#if _OrthoCamera
            float ratio = 0.45f - screenY * 0.0001f;
            if (ratio < 0.4f) ratio = 0.4f;
            transform.localScale = new Vector3(ratio, ratio, ratio);
#else
            transform.localEulerAngles = new Vector3(CameraManager.FieldActiveAngleX, 0, 0);
#endif
        }

        //Draw Runner
        void drawRunner()
        {
            
            if (lastState != state)
            {
                strID = _strID + FieldParm._dir[nDir];
                playAnim();
                lastState = state;
            }
            else
            {
                if (nLastDir != nDir)
                {
                    strID = _strID + FieldParm._dir[nDir];
                    playAnim();
                    nLastDir = nDir;
                }
            }

            setPosition();
            setRatio();
        }

        //Play Animation
        int track = 1;
        int lastTrack;

        void playAnim()
        {
            if (lastStrID != strID)
            {
                if (anim.state.Data.skeletonData.FindAnimation(strID) != null)
                {
                    //if (track != lastTrack)anim.state.ClearTrack(lastTrack);
                    //anim.skeleton.SetSlotsToSetupPose();
                    anim.state.ClearTracks();
                    anim.skeleton.SetToSetupPose();//.SetSlotsToSetupPose();
                    anim.state.SetAnimation(0, strID, loop);

                    //lastTrack = track;
                    //track = 1 - track;

                    anim.timeScale = 1;
                    lastStrID = strID;
                }
            }

            //anim.timeScale = (timeScale * timeScaleRate);
            anim.timeScale = timeScaleRate;
        }

        //Play Special Animtion
        void setSpecialAnim(string strAnim, bool bLoop)
        {
            //////UnityEngine.//Debug.Log("==========>>runnerIndex " + runnerIndex + "     setSpecialAnim!!!!");
            //if (track != lastTrack) anim.state.ClearTrack(lastTrack);
            //anim.skeleton.SetSlotsToSetupPose();

            anim.state.ClearTracks();
            anim.skeleton.SetSlotsToSetupPose();
            anim.state.SetAnimation(0, strAnim, bLoop);

            //lastTrack = track;
            //track = 1 - track;
            anim.timeScale = 1;
        }

        //Time Scale
        public void setTimeScale(float scale)
        {
            timeScaleRate = scale;
            anim.timeScale = timeScaleRate;// (timeScale * timeScaleRate);*/
        }

        ///////////////////////////////////////////////////////
        //애니메이션
        ///////////////////////////////////////////////////////    

        //Stop Anim 세팅
        public void setStopAnim()
        {
            //////UnityEngine.//Debug.Log("==========>>runnerIndex "+ runnerIndex +"     setStopAnim()");
            _strID = RunningMechnism._HOLD;
            strID = _strID + FieldParm._dir[nDir];
            loop = false;
        }

        //Move Anim 세팅
        public void setMoveAnim(RunState _state)
        {
            //////UnityEngine.//Debug.Log("==========>>runnerIndex " + runnerIndex + "     setMoveAnim()");
            state = _state;// RUNNER_MOVE;
            _strID = RunningMechnism._RUN;
            strID = _strID + FieldParm._dir[nDir];
            loop = true;
            curTime = 0;
        }

        //Sliding Anim세팅
        void setSlidingAnim(SlidingType type, bool bCheckClosePlay = true)
        {
            if (type != SlidingType._NO_SLIDING)
            {
                if (bCheckClosePlay == true)
                {
                    //CameraManager.SetFieldMotionBlur(0.5f);
                    if (bStealFlag == true)
                    {
                        //##연출 주자 도루 접전
                    }
                    else
                    {
                        if (field.ball.step == BallStep.BALL_THROW)
                        {
                            if (destPos == field.nTargetIndex && field.ball.checkClosePlay() == true)
                            {
                                //////UnityEngine.//Debug.Log("===============>>클로즈 플레이!!!!!!!!");
                                targetBase = destPos;
                                bClosePlay = true;
                            }
                        }
                    }
                }

                state = RunState.SLIDING;
                if (type == SlidingType._DOUBLEPLAY_SLIDING)
                {
                    //setSpecialAnim("2030_SLIDING_DOUBLEPLAY", false);
                    //setSpecialAnim("2000_SLIDING_LEG_NW_KILL", false); //이거 어디감?
                    setSpecialAnim("2000_SLIDING_LEG_NW", false);
                }
                else
                {
                    if (type == SlidingType._HEAD_FIRST || bMoveForward == false)
                    {
                        _strID = RunningMechnism._HEADSLIDING;
                    }
                    else if (type == SlidingType._NORMAL)
                    {
                        _strID = RunningMechnism._SLIDING;
                    }
                    strID = _strID + FieldParm._dir[nDir];
                    loop = false;

                    if (destPos == FieldParm.HOMEBASE_INDEX)
                    {
                        run.homeShobu = HomeShobu._SLIDING;
                    }
                }

            }
        }

        //홈돌진 anim세팅
        void setRushAnim()
        {
            if ((field.ball.step == BallStep.BALL_THROW && destPos == field.nTargetIndex)
             || (field.ball.step == BallStep.BALL_THROW_CATCH && field.nCatchIndex == CPlayer._CATCHER))
            {
                //////UnityEngine.//Debug.Log("===============>>클로즈 플레이!!!!!!!!");
                targetBase = destPos;
                bClosePlay = true;
            }

            if (bClosePlay == true)
            {
                if (bRushMustHappen == true)
                {
                    //AI컨트롤                    
                    setHomeRushOn();
                    /*
                    //블럭 특수효과
                    Fielder catcher = field.fielder[CPlayer._CATCHER];
                    if (catcher.checkSkillOn(SkillIndex.CatcherRunnerBlocking) == true)
                    {
                        //수비형포수 - 주자블럭 연출
                        fieldSkillDisplayManager.AddSkill(catcher.gameObject, SkillIndex.CatcherRunnerBlocking);
                    }*/
                }
                else
                {
                    //UnityEngine.Debug.Log("러시 무효화!!!!!!!!!!!!!!!!!!!!");
                    bRushSkillOn = false;
                }
            }
            
        }

        //병살저지 anim세팅
        void setDoblePlayAnim()
        {
            if (field.ball.step == BallStep.BALL_THROW)
            {
                if (destPos == field.nTargetIndex && field.ball.checkClosePlay(0.5f) == true && field.bOutofInfield == false)
                {
                    //////UnityEngine.//Debug.Log("===============>>클로즈 플레이!!!!!!!!");
                    targetBase = destPos;
                    bClosePlay = true;
                }
            }

            if (bClosePlay == true)
            {        //AI컨트롤
                setDoublePlayStopOn();
            }
            else
            {
                //UnityEngine.Debug.Log("병살저지 무효화!!!!!!!!!!!!!!!!!!!!");
                bDoublePlaySkillOn = false;
                setSlidingAnim(slidingType);
            }
        }

        ///////////////////////////////////////////////////////
        //주루 관련 상태 판단 함수
        ///////////////////////////////////////////////////////
        //주자의 active여부 확인한다
        public bool checkActive()
        {
            if ((int)state > (int)RunState.GO_BENCH)
            {
                return true;
            }
            return false;
        }

        //주자의 AI가 올바른 판단 여부를 체크
        //미완성
        bool checkRunnerAI()
        {
            //옯바른 판단시 true 리턴
            return true;
        }

        //슬라이딩 범위 체크해준다
        bool checkSlidingRange(float myPosX, float basePosX)
        {
            float range = ((bRushSkillOn || bDoublePlaySkillOn) ? RunningMechnism.RUSH_RANGE : RunningMechnism.SLIDING_RANGE);

            if (Mathf.Abs(posX - dstX) <= range)
            {
                return true;
            }

            return false;
        }

        //베이스 도착 여부를 체크해준다
        bool checkBaseRange(float myPosX, float basePosX)
        {
            if (destPos == FieldParm.FIRSTBASE_INDEX || destPos == FieldParm.HOMEBASE_INDEX)
            {
                //1루, 홈
                if (myPosX >= basePosX - RunningMechnism.BASE_ARRIVE_RANGE)
                {
                    return true;
                }
            }
            else
            {
                //2루, 3루
                if (bMoveForward == false && destPos == FieldParm.SECONDBASE_INDEX)
                {
                    if (myPosX >= basePosX + RunningMechnism.BASE_ARRIVE_RANGE)
                    {
                        return true;
                    }
                }
                else
                {
                    if (myPosX <= basePosX + RunningMechnism.BASE_ARRIVE_RANGE)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //접전 상황 여부를 체크해준다
        bool checkClosePlay()
        {
            if (Mathf.Abs(posX - dstX) < RunningMechnism.CLOSE_PLAY_RANGE)
            {
                return true;
            }
            return false;
        }

        //public float oneMoreBaseCheckValue = -1;

        //한 베이스 더 갈 수 있는지 여부를 체크해준다 
        public bool checkOneMoreBase(bool bAfterSliding)
        {            

            overRunFlag = SimulOverrunState.NONE;
#if _TEST_HOMERUSH
            int next2 = (destPos + 1) % 4;
            if (next2 == FieldParm.HOMEBASE_INDEX)
            {
                return true;
            }
#endif


#if _NOMOREBASE
            return false;
#endif
            //////UnityEngine.//Debug.Log("=====================>> checkOneMoreBase run.bRunnerFoul : " + run.bRunnerWalk);

            //리와인드 플레이가 아닌 경우 실시간 계산
            if (destPos == FieldParm.HOMEBASE_INDEX || bMoveForward == false)
            {
                return false;
            }
            else
            {
#if _TEST_ONEMOREBASE
                    if (bTestOnemoreBase == true)
                    {
                        bTestOnemoreBase = false;
                        return true;
                    }
#endif

#if _TEST_FIRSTBASE_OVERRUN
                if (bTestOverRun == true && destPos == FieldParm.FIRSTBASE_INDEX) return true;
#endif

                int backIndex = -1;
                int nextBase = (destPos + 1) % 4;


                if (field.ball.bHomeRunCall == true)
                {
                    //홈런인 경우
                    return true;
                }

                if (run.bRunnerWalk == true || run.bWildPitchRunning == true || run.bPickOff == true)
                {
                    //Debug.Log("=====================>> 견제, 폭투, 포볼시 강제 한베이스만....");
                    return false;
                }

                ////Debug.Log("=====================>> run.bHomeSteal = " + run.bHomeSteal + "===field.errorType = " + field.errorType);

                if (run.bStealBase == true || run.bHomeSteal == true || field.bBuntFielding == true || run.bOnlyOneBaseFlag == true)
                {
                    if (field.errorType == FieldParm.ErrorType.None || field.errorType == FieldParm.ErrorType.Juggle)
                    {
                        //포볼인 경우 강제 한베이스만....
                        //Debug.Log("=====================>> 포볼/도루/번트인 경우 강제 한베이스만....");
                        return false;
                    }
                }

                if (field.runnerTurbo == FieldSkillUse.Success)
                {
                    if (runnerIndex == run.nHitterRunnerIndex && nextBase == FieldParm.SECONDBASE_INDEX)
                    {
                        if (field.errorType == FieldParm.ErrorType.None)
                        {
                            ////UnityEngine.//Debug.Log("=====================>> 터보스킬이 켜진 경우 강제 한베이스만....");
                            return false;
                        }
                    }
                }


                if (bForcedOneMoreBase == true)
                {
                    UnityEngine.Debug.Log("###############강제 oneMoreBase 플래그 on시 강제로 진루한다");
                    bForcedOneMoreBase = false;
                    return true;
                }

                if (nextBase != FieldParm.HOMEBASE_INDEX)
                {
                    if (run.bOnRunning[nextBase] == true)
                    {
                        //UnityEngine.Debug.Log("###############앞에 주자가 달리고 있는경우 무조건 FALSE");
                        return false;
                    }
                }

                if (runnerIndex != run.nHitterRunnerIndex)
                {
                    backIndex = run.getBackRunnerRunningStateIndex(currentPos, runnerIndex);
                }

                if (backIndex != -1)
                {
                    //Debug.Log("==========>>> 뒷주자에 밀려 강제 진루");
                    return true;
                }
                else
                {
                    if (overrunState != SimulOverrunState.NONE)
                    {
                        //Debug.Log("$##################################################이전에 오버런 한 경우강제스탑");
                        overrunState = SimulOverrunState.NONE;
                        return false; //
                    }

                    float delayGab = field.getThrowingTotalTime(nextBase) - getNextDstTime(false);
                    float tagDelay = (FieldingMechanism.DELAY_TAGGING - 0.1f); //여기에 태그 딜레이를 더해봤다.
                    float oneMoreValue = delayGab + tagDelay;
                    

                    if (oneMoreValue > 0)
                    {
                        //진루 가능:최대한 보수적
                        ////Debug.Log("======================================>> oneMoreValue = " + oneMoreValue + "   " + (destPos + 1) + "루 체크");
                        if (nextBase == FieldParm.HOMEBASE_INDEX) field.bRushCounterHappen = true;
                        field.setRecheckThrow(nextBase);
                        return true;
                    }
                    else
                    {
                        if (bAfterSliding == true)
                        {
                            ////UnityEngine.//Debug.Log("======================================>> 슬라이딩 후 무조건 false");
                            return false;
                        }
                        else
                        {
                            return chekcOverrunCheck(oneMoreValue, nextBase);
                        }
                    }
                }
            }
        }

        //홈돌진 여부를 체크해준다 (홈돌진 기본 발동)
        private bool checkHomeRush(float timeValue, int nextBase)
        {
            //Debug.Log("==================================>> 홈돌진 랜덤테스트 " + Random.Range(0, 1000) + "       timeValue = " + timeValue);   
            if(pRunner.fieldSkillSuccess(SkillIndex.RunnerHomeRush) == true) 
            {
                //
                if (timeValue > RunningMechnism.OVERRUN_HOMERUSH_LIMIT && timeValue < 0)
                {
                    if (nextBase == FieldParm.HOMEBASE_INDEX)
                    {
                        if (MyMath.Percent() < 40)
                        {
                            //홈돌진 체크
                            if (field.bOutofInfield == true && field.bNoMoreHomeRushFlag == false)
                            {
                                if (field.bFieldStealFlag == false && runnerIndex != run.nHitterRunnerIndex)
                                {
                                    //발동확률
                                    bRushMustHappen = true;
                                    field.bNoMoreHomeRushFlag = true;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            //if (nextBase == FieldParm.HOMEBASE_INDEX) //UnityEngine.//Debug.Log("================================================================================>>홈돌진을 발동하지 않는다");
            return false;
        }

        //홈돌진 카운터 발동여부를 체크해준다 (홈돌진 카운터 발동)
        public bool checkCounterHomeRush()
        {
            if (pRunner.fieldSkillSuccess(SkillIndex.RunnerHomeRush) == true) 
            {
                if (MyMath.Percent() < 40)
                {
                    //카운터 발동
                    if (field.bFieldStealFlag == false && runnerIndex != run.nHitterRunnerIndex)
                    {
                        nDir = FieldParm._SOUTHEAST;
                        //홈돌진 카운터 발동확률
                        bRushMustHappen = true;
                        field.bNoMoreHomeRushFlag = true;
                        //field.bBlockBonusByLaser = true;
                        return true;
                    }
                }
            }
            return false;
        }

        //오버런 주루 여부를 체크해준다
        public bool chekcOverrunCheck(float timeValue,int nextBase)
        {
            SimulOverrunState state = SimulOverrunState.NONE;

            //Debug.Log("중요중요 PVP모드에서 주루관련 다 어긋나는건 이놈 때문임 오버런 타임밸류 체크 : timeValue = " + timeValue);

            if (checkHomeRush(timeValue, nextBase) == true)
            {
                state = SimulOverrunState.OUT;
            }
            else
            {
                //////UnityEngine.//Debug.Log("================================================================================>>" + (nextBase) + "루 주자 한베이스 더!! 체크한다");

                int tIndex;
                if (field.throwFielder >= 0)
                {
                    tIndex = field.throwFielder;
                }
                else
                {
                    tIndex = field.getCloseFielderIndex();
                }
                Fielder throwFielder = field.fielder[tIndex];

                if (timeValue > RunningMechnism.OVERRUN_SAFE_LIMIT)
                {
                    state = SimulBaseRunning.checkGetOneMoreBase(pRunner, throwFielder.pFielder, true);

                    if (state == SimulOverrunState.SAFE)
                    {
                        if(nextBase==FieldParm.HOMEBASE_INDEX) field.bRushCounterHappen = true;
                        needMoreTime = Mathf.Abs(timeValue) + 0.1f;
                    }
                    oneMoreSkillSense = state;
                    //if (state != SimulOverrunState.NONE) //UnityEngine.//Debug.Log("======================================================================================================>>" + (nextBase) + "루 주자 안전안전  timeValue = " + timeValue);
                }
                else if (timeValue > RunningMechnism.OVERRUN_DANGER_LIMIT)// - 0.5f)
                {
                    state = SimulBaseRunning.checkGetOneMoreBase(pRunner, throwFielder.pFielder, false);

                    if (state == SimulOverrunState.SAFE)
                    {
                        if (nextBase == FieldParm.HOMEBASE_INDEX) field.bRushCounterHappen = true;
                        needMoreTime = Mathf.Abs(timeValue) + RunningMechnism.OVERRUN_SAFE_LIMIT + 0.1f;
                    }
                    oneMoreSkillSense = state;
                    //if (state != SimulOverrunState.NONE) ////UnityEngine.//Debug.Log("===========================================================================================================>>" + (nextBase) + "루 주자 위험위험한베이스 더!!!!!!!!!!!!!!!!!!!!!  timeValue = " + timeValue);
                }

                //뇌주루 설정
                if(state == SimulOverrunState.OUT)
                {                    
                    overRunningMark(true);
                }
                else if (state == SimulOverrunState.SAFE)
                {
                    if (pRunner.skillAvailable(SkillIndex.RunnerSense) == true)
                    {
                        //주루센스 - 상황판단 연출
                        fieldSkillDisplayManager.AddSkill(gameObject, pRunner, SkillIndex.RunnerSense);
                        overRunningMark(false);
                    }
                }
                overRunFlag = state;
            }

            overrunState = state;


            if (state != SimulOverrunState.NONE)
            {                
                //Debug.Log("================================================================================>>" + (nextBase) + "루 주자 한베이스 더진루를 시도한다 state = " + state);                
                field.setRecheckThrow(nextBase);
                return true;
            }
            //////UnityEngine.//Debug.Log("================================================================================>>" + (nextBase) + "루 주자 한베이스 더진루를 시도하지 않는다");
            return false;
        }

        //송구 에러 후 한베이스 더 갈 수 있는 지여부를 체크
        public bool checkOneMoreBaseAfterError(Fielder fielder, float ballX, float ballY)
        {
            float runnerTime, fielderTime;
            
            float disTime = fielder.getRemainTime(ballX, ballY);
            float throwTime = FieldingMechanism.getDistance(posX, ballX, posY, ballY) / fielder.THROW_SPEED;
            
            float addTime = 0.8f;
            if (fielder.fieldingAbil > 500)
            {
                addTime = 0.8f - ((fielder.fieldingAbil - 500) * 0.0008f);
            }

            fielderTime = disTime + throwTime + fielder.THROW_DELAY + addTime;
            runnerTime = getNextDstTime(true) + RUNNER_DELAY;

            //////UnityEngine.//Debug.Log("============================================>> 주자 시간 :" + runnerTime);
            //////UnityEngine.//Debug.Log("============================================>> 야수 시간 :" + fielderTime);

            if (runnerTime < fielderTime)
            {
                return true;
            }

            return false;
        }

        //베이스택 가능한지 여부를 체크해준다
        public bool checkBaseTagPossibe()
        {
            int fIndex;
            //리와인드 플레이가 아닌 경우 실시간 계산
            if (field.flyCatchFielder != -1)
            {
                fIndex = field.flyCatchFielder;
            }
            else
            {
                fIndex = field.getCloseFielderIndex();
                if (field.fielder[fIndex].bOverHead == false)
                {
                    //아직 공을 안잡았으나 오버해드가 아니면 뛰지마
                    //////UnityEngine.//Debug.Log("===========>>여기 체크2");
                    return false;
                }
            }
            if (fIndex < CPlayer._LEFTFIELDER) return false;            

            int nextBase = (currentPos + 1) % 4;
            float nextX = field.getOriginX(FieldSize.getBasePosX(nextBase));
            float nextY = field.getOriginY(FieldSize.getBasePosY(nextBase));
            float distance = MyMath.getDistance(field.fielder[fIndex].posX, nextX, field.fielder[fIndex].posY, nextY);

            float delayTime = field.fielder[fIndex].getThrowingDelay(nextBase, false) + field.fielder[fIndex].getCatchingDelay(true);
            float throwingTime = (distance / field.fielder[fIndex].THROW_SPEED);
            float totalThrowingTime = (delayTime + throwingTime);

            //Debug.Log("delayTime = " + delayTime);
            //Debug.Log("구방법 throwingTime = " + throwingTime);
            //Debug.Log("신방법 throwingTime = " + field.ball.getThrowingTime(distance, field.fielder[fIndex].THROW_SPEED));
            ////Debug.Log("=======================>>checkBaseTagPossibe :: runnerIndex = " + runnerIndex + "::nextBase = " + nextBase);
            ////Debug.Log("==================>>totalThrowingTime = " + totalThrowingTime);

            if (totalThrowingTime > getNextDstTime(true))
            {
                if (nextBase == FieldParm.HOMEBASE_INDEX)
                {
                    field.bRushCounterHappen = true;
                    field.batter.bSacFly = true;
                }
                return true;
            }
            else
            {
                return false;
            }
           
        }


        //내가 제일 선행주자인지 여부를 확인 해준다
        bool checkMeFirstRunner()
        {
            //내가 선행주자인지 여부

            for (int i = 0; i < 4; i++)
            {
                if (run.runnerActive[i] == true)
                {
                    if (runnerIndex != run.runner[i].runnerIndex)
                    {
                        if (run.runner[i].checkActive() == true)//nState > RunningMechnism.GO_BENCH)
                        {
                            if (run.runner[i].destPos > destPos)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        //내가 가는 베이스로 볼이 오고 있는지 여부를 확인 해준다
        bool checkBallOnDestBase()
        {
            if (field.ball.checkThrowingToThisBase(destPos) == true //볼이 해당 베이스로 던져지고 있는 경우
                || run.bBallOnBase[destPos] == true //볼이 해당 베이스에 도착해 있는 경우
                || (field.bThrowing == true && destPos == FieldParm.HOMEBASE_INDEX && field.ball.bHomeRunGuess == false) //볼이 던져지고 있는 경우, 홈베이스, 홈런이 아닌경우
                )
            {
                return true;
            }

            return false;
        }

        //슬라이딩이 필요한지 여부를 확인 해준다
        bool checkSlidingNeeded()//bool ballOnBase, bool ballToBase)
        {
            //해드 슬라이딩 특능이 아닌 경우 
            if (destPos == FieldParm.FIRSTBASE_INDEX && bMoveForward == true) // && 해드 슬라이딩 특능 없는 경우
            {
                return false;
            }

            if (destPos == FieldParm.HOMEBASE_INDEX && bRushSkillOn == true && field.nTargetIndex != FieldParm.HOMEBASE_INDEX) // && 해드 슬라이딩 특능 없는 경우
            {
                return false;
            }

            if (destPos == FieldParm.HOMEBASE_INDEX && run.bHomeSteal == true && field.bFieldStealFlag == true)
            {
                ////UnityEngine.//Debug.Log("====================>>홈스틸 특수조건 !!!! 이조건에 걸리냐");
                return true;
            }

            return checkBallOnDestBase();
        }

        //포스아웃 체크
        private bool checkForcedOut()
        {
            if (run.bStealBase == false)
            {
                if (bMoveForward == true)
                {
                    if (run.bForceOutFlag[destPos] == true)
                    {
                        if (run.bBallOnBase[destPos] == true)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                }
            }
            return false;
        }

        private bool checkForceBackOut()
        {
            if (run.bStealBase == false)
            {
                if (bMoveForward == false && bForcedOutBackMove == true)
                {
                    if (run.bBallOnBase[destPos] == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //태그 또는 포스 아웃 체크
        bool checkRunnerOut()
        {
            ////UnityEngine.Debug.Log("##########################포스 또는 태극 아웃 체크 러너");
            if (bMoveForward == true)
            {
                if (run.bBallOnBase[destPos] == true)
                {
                    field.nCheckBaseNum = -1;
                    return true;
                }
            }
            else
            {
                if (run.bBallOnBase[destPos] == true)
                {
                    field.nCheckBaseNum = -1;
                    return true;
                }
            }
            return false;
        }

        ///////////////////////////////////////////////////////
        //주자의 능력치
        ///////////////////////////////////////////////////////
        
        //주자의 스피드를 얻어온다
        public float getRunnerSpeed()
        {
#if _TEST_RUNNER
            return testSpeed;       //홈돌진, 병살저지
#else
            return (pRunner.getSpeed());// + pRunner.getSpeedBonus());
#endif            
        }

        //////////////////////////////////////////////////////////////////
        //기록
        //////////////////////////////////////////////////////////////////

        //득점시 관련 상태 업데이트를 반영한다.
        public void setAddScore()
        {
            //Debug.Log("@@@@@@@@8");
            field.bReturnBattingView = false;

            field.getAddScore++;
            if (bLastPitcher == true)
            {
                //승계주자인 경우
                field.lastPitcherAddScore++;
            }

            if (bErrorRunner == false)
            {
                field.getAddErrScore++;
                if (bLastPitcher == true)
                {
                    //승계주자인 경우
                    field.lastPitcherAddErrScore++;
                }
            }

            //득점 플래그 온
            run.runnerLastPos[arrayIndex] = lastPos;
            run.runnerRunScore[arrayIndex] = true;
            run.runnerData[arrayIndex] = this.pRunner;

            //setBench(RunState.ADD_SCORE, RunnerOutMotion._NORMAL);

            //Debug.Log("==================>> lastPos = " + lastPos);

            if (lastPos == FieldParm.THIRDBASE_INDEX && field.bOutofInfield == true)
            {
                setBench(RunState.ADD_SCORE, RunnerOutMotion._NORMAL, 1);
            }
            else
            {
                setBench(RunState.ADD_SCORE,MyMath.Half() ? RunnerOutMotion._HOME_CEREMONY1 : RunnerOutMotion._HOME_CEREMONY2);
            }

            
            if (bCameraOn == true)
            {
                //UIFieldCamera.SetActive(false, this, field.manager);
                setCamera(false);
            }

        }

        //해당 주루 기록을 더해준다
        public void addRecord(int type, int num = 1)
        {
            if (pRunner != null)
            {
                //Debug.Log((currentPos + 1) + "루 주자 기록===============>>> " + pRunner.getName() + "의 " + Param.debug_stat[type] + " 가산");
                pRunner.setRecord(type, num);
            }
        }

        //안타(혹은 에러)를 체크해준다
        private void checkBaseHit()
        {
            //안타 체크 - (베이스에 도착시)
            if (runnerIndex == field.run.nHitterRunnerIndex)
            {
                if (run.bStealBase == false && run.bRunnerWalk == false)
                {
                    if (run.bNotOutRunning == true)
                    {
                        //Debug.Log("=========================================>>>스트라이크 아웃 낫아웃");
                        field.chanceResult = SimulResultState.StrikeOut;
                        field.manager.strBatterResult = "삼진 (낫아웃)";
                    }
                    else
                    {
                        if (field.bErrorFlag == false)
                        {
                            //안타 카운트            
                            bErrorRunner = false;
                            if (bHitCheck == false)
                            {
                                field.chanceResult = SimulResultState.Single + currentPos;
                                field.manager.setHitCount(currentPos);
                                //Debug.Log("=========================================>>>안타체크");
                                bHitCheck = true;
                            }
                        }
                        else
                        {
                            //에러 카운트                        
                            if (currentPos >= FieldParm.SECONDBASE_INDEX)
                            {
                                //Debug.Log("=========================================>>>원힛 원에러로 계산 체크?");
                                if (bErrorRunner == false)
                                {
                                    ////UnityEngine.//Debug.Log("=========================================>>>원힛 원에러로 계산");
                                    bErrorRunner = false;

                                    if (field.bInfieldThrowErrorFlag == true)
                                    {
                                        //악송구에 진루
                                        if (bErrorCheck == false)
                                        {
                                            field.chanceResult = SimulResultState.ThrowError;
                                            field.manager.setErrorCount(true);
                                            bErrorCheck = true;
                                        }
                                    }
                                    else
                                    {
                                        if (bHitCheck == false)
                                        {
                                            field.chanceResult = SimulResultState.SingleOneError + (currentPos - 1);
                                            field.manager.setHitCount(currentPos - 1);
                                            bHitCheck = true;
                                        }
                                        if (bErrorCheck == false)
                                        {
                                            field.chanceResult = SimulResultState.CatchError;
                                            field.manager.setErrorCount(true);
                                            bErrorCheck = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Debug.Log("=========================================>>>에러에 의한 출루로 계산");
                                bErrorRunner = true;
                                if (bErrorCheck == false)
                                {
                                    field.chanceResult = SimulResultState.CatchError;
                                    field.manager.setErrorCount(false);
                                    bErrorCheck = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        //안타(혹은 에러)지만 주루사를 체크해준다
        void checkBaseHitButOut() //무리한 주루로 아웃
        {
            if (runnerIndex == field.run.nHitterRunnerIndex)
            {
                //안타지만 오버런 아웃 체크
                //////UnityEngine.//Debug.Log("================>>타자주자의 오버런 체크 destPos :: " + destPos);
                if (destPos >= FieldParm.SECONDBASE_INDEX)
                {
                    if (run.bStealBase == false && run.bRunnerWalk == false)
                    {
                        if (field.bErrorFlag == false)
                        {
                            //안타 카운트  
                            bErrorRunner = false;
                            if (bHitCheck == false)
                            {
                                field.manager.setHitCount(destPos - 1);
                                bHitCheck = true;
                            }
                        }
                        else
                        {
                            //에러 카운트
                            bErrorRunner = true;
                            if (bErrorCheck == false)
                            {
                                field.manager.setErrorCount(true);
                                bErrorCheck = true;
                            }
                        }
                    }
                }
            }
        }


        ///////////////////////////////////////////////////////
        //주루 관련  (before fielding)
        ///////////////////////////////////////////////////////
        //스킵동작에 이은 주자의 위치 지정
        public void setRunnerSkipMove()
        {
            if (bStealFlag == true) return;
            ////UnityEngine.Debug.Log("##########################필딩시 setRunnerSkipMove 호출?????");
            //1차 리드 후 볼을 던지는 스킵동작
            //러너의 능력치 별로 차별을 둘것!!!!
            //리드 스킬이 이것에 영향
            int leadLevel = (int)sLead;// -field.pitcherQuickMotionLevel;
            
            float rate = 0.7f + (leadLevel * 0.01143f);//MAX: 1.5f; 
            float rate2 = 1.0f + (leadLevel * 0.01715f);//MAX: 2.2f; 
            float leadRange = Random.Range(0.5f, 1.0f);// leadLevel이 0보다 큰경우 투수가 묶어놓는 범위

            posX = field.getOriginX(FieldSize.getRunnerInitPosX(currentPos));	//HOME_POSX;
            posY = field.getOriginY(FieldSize.getRunnerInitPosY(currentPos));
            if (currentPos == FieldParm.FIRSTBASE_INDEX)
            {
                //UnityEngine.Debug.Log("현재 1루주자 " + pRunner.getName() + "의 리드 스킬에 의한 스킵무브");
                if (leadLevel >= 0)
                {
                    posX -= (140 * rate * 0.5f);
                    posY += (100 * rate * 0.5f);
                }
                else
                {
                    posX -= (49 + leadLevel * 0.43f * leadRange);
                    posY += (50 + leadLevel * 0.43f * leadRange);
                }

            }
            else if (currentPos == FieldParm.SECONDBASE_INDEX)
            {
                //UnityEngine.Debug.Log("현재 2루주자 " + pRunner.getName() + "의 리드 스킬에 의한 스킵무브");
                if (leadLevel >= 0)
                {
                    posX -= (160 * rate2 * 0.5f);
                    posY -= (20 * rate2 * 0.5f);
                }
                else
                {
                    posX -= (80 + leadLevel * 0.57f * leadRange);
                    posY -= (10 + leadLevel * 0.071f * leadRange);
                }
            }
            else if (currentPos == FieldParm.THIRDBASE_INDEX)
            {
                //UnityEngine.Debug.Log("현재 3루주자 " + pRunner.getName() + "의 리드 스킬에 의한 스킵무브");
                if (leadLevel >= 0)
                {
                    posX += (40 * rate * 0.5f);
                    posY -= (180 * rate * 0.5f);
                }
                else
                {
                    posX += (14 + leadLevel * 0.1f * leadRange);
                    posY -= (90 + leadLevel * 0.643f * leadRange);
                }
            }
        }

        //초기 위치 설정
        public void setInitPos(int curPos, bool bAfterSlide = false, bool bLead = false)
        {
            if (run.bPickOff == true)
            {
                if (curPos == FieldParm.FIRSTBASE_INDEX)
                {
                    nDir = FieldParm._SOUTHEAST;
                }
                else if (curPos == FieldParm.THIRDBASE_INDEX)
                {
                    nDir = FieldParm._NORTHWEST;
                }
                else
                {
                    nDir = FieldParm._NORTHEAST;
                }
                return;
            }


            posX = field.getOriginX(FieldSize.getRunnerInitPosX(curPos));	//HOME_POSX;
            posY = field.getOriginY(FieldSize.getRunnerInitPosY(curPos));	//HOME_POSY;

            dX = 0;
            dY = 0;

            if (bLead == false)
            {
                if (bAfterSlide == true)
                {
                    StartCoroutine(setBaseHoldAnim(curPos, 0.6f, true));
                }
                else
                {
                    StartCoroutine(initPos(curPos, 0.1f));
                }
            }
            else
            {
                setRunnerSkipMove();
            }


        }

        //초기 위치
        IEnumerator initPos(int curPos, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (curPos == FieldParm.FIRSTBASE_INDEX)
            {
                posX -= 20;
                posY += 20;
                nDir = FieldParm._SOUTHWEST;
            }
            else if (curPos == FieldParm.THIRDBASE_INDEX)
            {
                posX += 10;
                posY -= 30;
                nDir = FieldParm._NORTHEAST;
            }
            else
            {
                posX -= 30;
                nDir = FieldParm._SOUTH;
            }
            setStopAnim();            
        }

        IEnumerator setBaseHoldAnim(int curPos, float delay, bool afterSliding)
        {
            //////UnityEngine.//Debug.Log("=======================>>setBaseHoldAnim");
            yield return new WaitForSeconds(delay);
            //////UnityEngine.//Debug.Log("===============================================================>>runnerIndex : " + runnerIndex + "  setBaseHoldAnim!!!!!!!!!=====>>> bForcedOneMoreBase2 = " + bForcedOneMoreBase2);
            if (bForcedOneMoreBase2 == true)
            {
                //도데체 왜 이렇게 해야하는지 모르겄다... 
                bForcedOneMoreBase2 = false;
                setDestination(true);
                state = RunState.MOVE;
                _strID = RunningMechnism._RUN;
                strID = _strID + FieldParm._dir[nDir];
                loop = true;
                curTime = 0;
                
                anim.state.ClearTracks();
                anim.skeleton.SetSlotsToSetupPose();
                anim.state.SetAnimation(0, strID, loop);
                anim.timeScale = 1;
                lastStrID = strID;
            }
            else
            {
                //슬라이딩 일어나는 애니메이션
                if (afterSliding == true)
                {
                    anim.state.SetAnimation(0, (slidingType == SlidingType._HEAD_FIRST ? "TEMP_SLIDING_HEAD_" : "GETUP_AFTER_SLIDING_") + FieldParm._dir[nDir], false);
                    yield return new WaitForSeconds(0.7f);
                }

                ////Debug.Log("=======================>>함성이벤트");
                if ((field.bNormalFielding == true && bHitCheck == true)
                  && runnerIndex == run.nHitterRunnerIndex
                  && (curPos == FieldParm.SECONDBASE_INDEX || curPos == FieldParm.THIRDBASE_INDEX))
                {
                    state = RunState.STANDBY;
                    setSpecialAnim("RUNNER_ROAR", false);
                    field.returnWaitTime = 2.7f;//
                }
                else
                {
                    state = RunState.STANDBY;
                    _strID = "0200_HOLD_TYPE" + Random.Range(1, 3) + "_";
                    //////UnityEngine.//Debug.Log("=======================>>_strID = " + _strID);
                    if (curPos == FieldParm.FIRSTBASE_INDEX)
                    {
                        nDir = FieldParm._WEST;
                    }
                    else if (curPos == FieldParm.THIRDBASE_INDEX)
                    {
                        nDir = FieldParm._EAST;
                    }
                    else
                    {
                        nDir = FieldParm._SOUTH;
                    }
                }
            }
        }



        private IEnumerator setBaseHoldAfterPickOff(int curPos, float delay)
        {           
            yield return new WaitForSeconds(delay);

            if (bThrowErrorHappen == true)
            {
                bThrowErrorHappen = false;
                yield break;
            }

            setArriveBase(true);
            posX = field.getOriginX(FieldSize.getRunnerInitPosX(curPos));
            posY = field.getOriginY(FieldSize.getRunnerInitPosY(curPos));
            dX = 0;
            dY = 0;

            state = RunState.STANDBY;            
            //////UnityEngine.//Debug.Log("=======================>>_strID = " + _strID);
            anim.state.ClearTracks();
            anim.skeleton.SetToSetupPose();
            if (curPos == FieldParm.FIRSTBASE_INDEX)
            {
                posY += 10;
                nDir = FieldParm._WEST;
                field.fielder[CPlayer._FIRSTBASEMAN].setStop();
                anim.state.SetAnimation(0, "TEMP_SLIDING_HEAD_SE", false);
                anim.state.AddAnimation(1, "0200_HOLD_TYPE1_W", true, 0.7f);
            }
            else if (curPos == FieldParm.THIRDBASE_INDEX)
            {                
                nDir = FieldParm._EAST;
                anim.state.SetAnimation(0, "TEMP_SLIDING_HEAD_NW", false);
                anim.state.AddAnimation(1, "0200_HOLD_TYPE1_E", true, 0.7f);
            }
            else
            {
                posX -= 15;
                nDir = FieldParm._SOUTH;
                anim.state.SetAnimation(0, "TEMP_SLIDING_HEAD_NE", false);
                anim.state.AddAnimation(1, "0200_HOLD_TYPE2_S", true, 0.7f);
                
            }
            //_strID = "0200_HOLD_TYPE" + Random.Range(1, 3) + "_";
            //strID = _strID + FieldParm._dir[nDir];            
            //playAnim();

            field.returnCheck_Steal_Pickoff(0);
        }

        //Standby상태 셋팅
        public void setStandby(BaseArriveMotion motion, bool bAfterSliding = false, bool forward = true)
        {            
            int curPos;//,rr;

            curPos = currentPos;
            destPos = (curPos + 1) & 0x03;



            posX = field.getOriginX(FieldSize.getRunnerInitPosX(curPos));	//HOME_POSX;
            posY = field.getOriginY(FieldSize.getRunnerInitPosY(curPos));	//HOME_POSY;

            startX = posX;
            startY = posY;

            bSlidingMotion = false; //초기화

            if (MyMath.Percent() < 80)
            {
                slidingType = SlidingType._NORMAL;	//레그 슬라이딩
            }
            else
            {
                slidingType = SlidingType._HEAD_FIRST; //헤드 퍼스트 슬라이딩	
            }

            curTime = 0;

            if (runnerIndex == run.nHitterRunnerIndex)
            {
                posX -= (run.batter.sign * 5);
            }

            bForcedOutBackMove = false;
            bStealFlag = false;
            bBaseTurning = false;
            //bForcedOut = false;//->필요없음 필더가 체크해줌
            bClosePlay = false;
            bMoveEffect = false;
            bSlidingSkillOn = false;  //슬라이딩 스킬 발동여부
            slidingAddTagDelay = 0;  //슬라이딩으로 발생하는 태그 딜레이
            outMotion = RunnerOutMotion._NORMAL;

            if (forward == false)
            {
                motion = BaseArriveMotion._NORMAL;
            }

            //////UnityEngine.//Debug.Log("========================>>능력치 세팅이 먼저냐?");
            //리와인드 플레이가 아닌경우 
            //주루 딜레이 -> 딜레이는 특능에 의해 변동 가능 //최소 0.1이고 타자주자시는 0.25
            ////UnityEngine.Debug.Log("주루 딜레이시 #######################################################>>sRunningSense = " + sRunningSense);
            RUNNER_DELAY = RunningMechnism.getRunnerDelay(sRunningSense);
            //주루 스피드
            RUNNER_SPEED = RunningMechnism.getRunnerSpeed(getRunnerSpeed());                        
            //RUNNER_SPEED *= Runner._RUNNER_MULTI;

            //주루 가속 -> 가속 특능에 의해 변동 가능
            float accelLevel = getRunnerSpeed() / 10;
            accelRate = RunningMechnism.getAccelRate(accelLevel);

            //각종 상태에 따른 2차 능력치
            curSpeed = RUNNER_SPEED;
            curDelay = RUNNER_DELAY;

            if (runnerIndex == run.nHitterRunnerIndex && destPos == FieldParm.FIRSTBASE_INDEX)
            {
                //타자주자 인덱스  더 딜레이
                curDelay = curDelay + RunningMechnism.HITTERRUNNER_DELAY_RATE; //2
            }

            if (motion == BaseArriveMotion._NORMAL || bAfterSliding == true)
            {
                bSlidingMotion = true;
                setInitPos(curPos, bAfterSliding);
                state = RunState.STANDBY;
            }
            else
            {
                nDir = RunningMechnism.RunnerDir[curPos];
                if (motion == BaseArriveMotion._FIRSTBASE_RUN_ARRIVE)
                {
                    state = RunState.FIRSTBASE_SAFE;
                    nMotionStep = 0;
                    curTime = 0;
                }
                else if (motion == BaseArriveMotion._SENCOND_THIRD_ARRIVE)
                {
                    //////UnityEngine.//Debug.Log("========================>>BaseArriveMotion._SENCOND_THIRD_ARRIVE");
                    state = RunState.SECOND_THIRD_SAFE;
                    nMotionStep = 0;
                    curTime = 0;
                    StartCoroutine(setBaseHoldAnim(curPos, 0.6f, false));                    
                }
            }

            
            if (forward == true)
            {
                //스킬 액티브 여부
                setSkillActive(curPos);
            }
            else
            {
                if (run.bPickOff == true)
                {
                    anim.state.ClearTracks();
                    anim.skeleton.SetToSetupPose();
                    if (currentPos == FieldParm.FIRSTBASE_INDEX)
                    {
                        posX -= 40;
                        posY += 25;
                        anim.state.SetAnimation(0, "TEMP_SLIDING_HEAD_SE", false);
                        anim.state.AddAnimation(1, "0200_HOLD_TYPE1_W", true, 0.7f);
                    }
                    else if (currentPos == FieldParm.SECONDBASE_INDEX)
                    {
                        posX -= 60;
                        posY -= 30;
                        anim.state.SetAnimation(0, "TEMP_SLIDING_HEAD_NE", false);
                        anim.state.AddAnimation(1, "0200_HOLD_TYPE2_S", true, 0.7f);
                    }
                    else
                    {
                        anim.state.SetAnimation(0, "TEMP_SLIDING_HEAD_NW", false);
                        anim.state.AddAnimation(1, "0200_HOLD_TYPE1_E", true, 0.7f);
                    }
                }
            }
        }

        /// <summary>
        /// 번트시 스피드 설정
        /// </summary>
        public void setBuntSpeed()
        {
            if (runnerIndex != run.nHitterRunnerIndex)
            {
                RUNNER_DELAY = RunningMechnism.STEAL_DELAY;
            }
            //주루 스피드
            RUNNER_SPEED = RunningMechnism.getRunnerSpeed(RunningMechnism.BUNT_SPEED);
            curSpeed = RUNNER_SPEED;
            accelRate = RunningMechnism.STEAL_ACCEL;

            if (currentPos == FieldParm.THIRDBASE_INDEX)
            {
                //스퀴즈 플래그 켜져 있지 않으면 실패하게
                if (field.batter.buntResult == SpecificBuntType.SQUEEZ_SUCCESS)
                {
                    RUNNER_DELAY = 0;
                    RUNNER_SPEED = RunningMechnism.getRunnerSpeed(RunningMechnism.BUNT_SPEED * 1.4f);
                    curSpeed = RUNNER_SPEED;
                    accelRate = 1;
                }
                else
                {
                    RUNNER_DELAY = 1.0f;
                }                
            }

            if (runnerIndex == run.nHitterRunnerIndex)
            {
                if (field.batter.buntResult == SpecificBuntType.DRAG_SUCCESS)
                {
                    //Debug.Log("=====================>> 드래그 번트 성공시 조정");
                    RUNNER_DELAY = 0;
                }
            }

            curDelay = RUNNER_DELAY;
        }


        //DO_NOTHING 상태 셋팅
        public void setDoNothing(bool bInitPos = false)
        {

            state = RunState.DO_NOTHING;
            dX = 0;
            dY = 0;
            setStopAnim();
            if (bInitPos == true)
            {
                setUpdate();
            }
        }

        //목적지 셋팅
        public void setDestination(bool bForward, bool bSlowMove = false)
        {
            //Debug.Log(arrayIndex + "번 주자========================>>curSpeed " + curSpeed+ "  재체크!!!!!!");

            
            int cur, next;

            cur = currentPos;
            if (bForward == true)
            {
                bMoveForward = true;
                next = (cur + 1) & 0x03;
                nDir = RunningMechnism.RunnerDir[cur];
                run.bOnBase[cur] = false;
                run.bOnRunning[next] = true;
                run.bOnBackRunning[cur] = false;

            }
            else
            {
                bMoveForward = false;
                next = cur;
                nDir = RunningMechnism.RunnerDir[(cur + 2) % 4];
                run.bOnBase[cur] = false;
                run.bOnRunning[next] = false;
                run.bOnBackRunning[cur] = true;
            }
            destPos = next;

            //bGrounderWaitFlag = false;


            dstX = field.getOriginX(FieldSize.getBasePosX(destPos));
            dstY = field.getOriginY(FieldSize.getBasePosY(destPos));


            float distanceX = dstX - posX;
            float distanceY = dstY - posY;

            baseDistance = RunningMechnism.getDistance(distanceX, distanceY);
            baseAngle = RunningMechnism.getAngle(distanceX, distanceY);

            if (bSlowMove == true)
            {
                curSpeed = curSpeed * 0.3f;
            }

            //병살저지를 그럴듯하게 하기 위해 호출해줌
            setDpBreakSpeed();

            dX = curSpeed * Mathf.Cos(baseAngle);
            dY = curSpeed * Mathf.Sin(baseAngle);

            curDX = 0;
            curDY = 0;
            aX = dX / accelRate;
            aY = dY / accelRate;

            //Debug.Log(arrayIndex + "번 주자========================>>curSpeed " + curSpeed + " 파이널 재체크!!!!!!");
            if (bForward == false)
            {
                setBackMoveException();
            }
            else
            {
                /*
                if (next == FieldParm.FIRSTBASE_INDEX && runnerIndex == run.nHitterRunnerIndex)
                {
                    if (field.batter.bDragBuntOn == true)
                    {
                        //##연출 타자주자 드래그번트 시전
                        field.batter.bDragBuntOn = false;
                    }
                }*/

                if (next == FieldParm.HOMEBASE_INDEX)
                {
                    if ((field.bOutofInfield == true && field.bNormalFielding == true)
                        || field.bOnceWildThrow == true)
                    {
                        //UIFieldCamera.SetActive(true, this, field.manager);
                        setCamera(true);
                    }
                }

            }
        }

        private void setBackMoveException()
        {
            bool xException = false;
            bool yException = false;
            if (destPos == FieldParm.FIRSTBASE_INDEX)
            {
                if (dX < 0) xException = true;
                if (dY > 0) yException = true;
            }
            else if (destPos == FieldParm.SECONDBASE_INDEX)
            {
                if (dX < 0) xException = true;
                if (dY < 0) yException = true;
            }
            else if (destPos == FieldParm.THIRDBASE_INDEX)
            {
                if (dX > 0) xException = true;
                if (dY < 0) yException = true;
            }

            if (xException == true)
            {
                Debug.Log(arrayIndex + "번 주자========================>>백무브 x축 익셉션 들어옴");
                dX = -dX;
                aX = -aX;
            }
            if (yException == true)
            {
                Debug.Log(arrayIndex + "번 주자========================>>백무브 y축 익셉션 들어옴");
                dY = -dY;
                aY = -aY;
            }
        }

        //병살저지를 그럴듯하게 보여주기 위해 curSpeed를 조절해줌
        private void setDpBreakSpeed()
        {
            if (bDoublePlaySkillOn == true)
            {
                if (field.bGrounderAvailble == true)
                {
                    if (destPos == FieldParm.SECONDBASE_INDEX && bMoveForward == true)
                    {
                        if( field.groundCatchFielder == CPlayer._PITCHER ||
                           (field.groundCatchFielder > CPlayer._FIRSTBASEMAN && field.ballPower > 24) )
                        {
                            float lastCurSpeed = curSpeed;
                            float newSpeed = baseDistance / (field.fastRemainTime + 1.5f);
                            if (newSpeed > lastCurSpeed)
                            {
                                curSpeed = newSpeed;
                                if (curSpeed > lastCurSpeed * 1.1f)
                                {
                                    curSpeed = lastCurSpeed * 1.1f;
                                }
                            }
                            //////UnityEngine.//Debug.Log("=====================================================================>> bGrounderAvail = " + field.bGrounderAvailble);
                            //////UnityEngine.//Debug.Log("=====================================================================>> bDoublePlaySkillOn = " + bDoublePlaySkillOn);
                            ////UnityEngine.//Debug.Log("=====================================================================>> lastCurSpeed = " + lastCurSpeed);
                            ////UnityEngine.//Debug.Log("=====================================================================>> curSpeed = " + curSpeed);
                        }
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////
        //주루 관련 (after fielding)
        ///////////////////////////////////////////////////////
        //플라이볼을 주자가 판단 대기시 리드하는 비율(베이스 대 베이스간 거리 기준)
        private float getFlyballLeadRatio()
        {
            float ratio = 0.1f;
            float baseDis;// = getDistance(
            float ballDis;//

            if (currentPos == FieldParm.FIRSTBASE_INDEX)
            {
                baseDis = RunningMechnism.getDistance(FieldSize.getFirstBasePosX() - FieldSize.getSecondBasePosX(), FieldSize.getFirstBasePosY() - FieldSize.getSecondBasePosY());
                ballDis = RunningMechnism.getDistance(FieldSize.getFirstBasePosX() - field.ball.nFirstBoundX, FieldSize.getFirstBasePosY() - field.ball.nFirstBoundY) / 7;

                ratio = ballDis / baseDis;
                if (ratio > 0.6f) ratio = 0.6f;
            }
            else if (currentPos == FieldParm.SECONDBASE_INDEX)
            {
                baseDis = RunningMechnism.getDistance(FieldSize.getSecondBasePosX() - FieldSize.getThirdBasePosX(), FieldSize.getSecondBasePosY() - FieldSize.getThirdBasePosY());
                ballDis = RunningMechnism.getDistance(FieldSize.getSecondBasePosX() - field.ball.nFirstBoundX, FieldSize.getSecondBasePosY() - field.ball.nFirstBoundY) / 7;

                ratio = ballDis / baseDis;
                if (ratio > 0.4f) ratio = 0.4f;
            }
            //////////UnityEngine.//Debug.Log("===================>>currentPos = " + currentPos + "============>>ratio = " + ratio);

            return ratio;
        }

        //다음 베이스까지 도달하는 시간(딜레이 포함)
        private float getNextDstTime(bool bFromBase)
        {
            //////////UnityEngine.//Debug.Log("===================>>getNextDstFrame :: runnerIndex = "+runnerIndex);
            //////////UnityEngine.//Debug.Log("===================>>currentPos = " + currentPos + " :: destPos = " + destPos);
            int nextPos = (currentPos + 1) % 4;
            float nextX = field.getOriginX(FieldSize.getBasePosX(nextPos));
            float nextY = field.getOriginY(FieldSize.getBasePosY(nextPos));

            float curX, curY;

            if (bFromBase == true)
            {
                curX = field.getOriginX(FieldSize.getBasePosX(currentPos));
                curY = field.getOriginY(FieldSize.getBasePosY(currentPos));
            }
            else
            {
                curX = posX;
                curY = posY;
            }

            float distanceX = nextX - curX;
            float distanceY = nextY - curY;

            float dis = RunningMechnism.getDistance(distanceX, distanceY);

            float _time = (dis / curSpeed);// +(curDelay);

            //////////UnityEngine.//Debug.Log("=====================================>>getNextDstFrame = " + frame);

            return _time;
        }


        //목적 베이스에서 다음 목적 베이스로 진행하면서 터닝을 한다 
        private void setDestinationTurning()
        {
            //////////UnityEngine.//Debug.Log("===================>>setDestination");   
            if (field.ball.bHomeRunCall == false)
            {
                if (bMoveForward == true)
                {
                    if (destPos == FieldParm.SECONDBASE_INDEX || destPos == FieldParm.THIRDBASE_INDEX)
                    {
                        if (run.bOnBase[destPos] == true)
                        {
                            ////UnityEngine.//Debug.Log("===================>>앞에 주자가 있네 ㅋㅋ");
                            setMoveBack();
                            return;
                        }
                    }
                }
            }

            dstX = field.getOriginX(FieldSize.getBasePosX(destPos));
            dstY = field.getOriginY(FieldSize.getBasePosY(destPos));

            float distanceX = dstX - posX;
            float distanceY = dstY - posY;

            baseDistance = RunningMechnism.getDistance(distanceX, distanceY);
            baseAngle = RunningMechnism.getAngle(distanceX, distanceY);

            nDir = FieldParm.getDir(baseAngle);

            //////UnityEngine.//Debug.Log("===================================>>다시 curSpeed체크: "+curSpeed);
            dX = curSpeed * Mathf.Cos(baseAngle);
            dY = curSpeed * Mathf.Sin(baseAngle);

            curDX = dX;
            curDY = dY;

            strID = _strID + FieldParm._dir[nDir];

        }

        //터닝 상태를 세팅해준다
        private void setBaseTurning()
        {
            //리와인드 플레이가 아닌 경우 터닝상태 세팅
            turningDir = baseAngle + (40 * Mathf.Deg2Rad);  //45도 더해줘
            turnDX = curDX / 2;
            turnDY = curDY / 2;
            turnSpeed = curSpeed / 2;
            turnAccel = curSpeed / accelRate;
            curTime = 0;
            maxTime = 1;            
            bBaseTurning = true;
        }

        //Move상태로 세팅한다
        public void setMoveOnBase()
        {
            //////////UnityEngine.//Debug.Log("=======================>>setMoveOnBase() runnerIndex :" + runnerIndex);

            if (bStealFlag) return;

            setDestination(true);
            curTime = 0;

            state = RunState.CHECK;
            nextState = RunState.MOVE;

            //내야안타 특능
            if (runnerIndex == run.nHitterRunnerIndex)
            {

            }

        }

        /// <summary>
        /// 에러 세팅
        /// </summary>
        /// <param name="curPos"></param>
        public bool bThrowErrorHappen;
        public void setThrowErrorSetting(int curPos)
        {
            bStealFlag = false;
            currentPos = curPos;
            bMoveForward = true;
            bThrowErrorHappen = true;
        }

        //Move상태로 세팅한다(위함수와 케이스가 약간 틀림)
        public void setMove()
        {
            //////UnityEngine.//Debug.Log("=======================>>setMove() bStealFlag = " + bStealFlag +" nState = "+nState);
            if (bStealFlag) return;
            ////UnityEngine.//Debug.Log("=======================>>setMove()");
            //if (nState > RunningMechnism.GO_BENCH && nState != RUNNER_MOVE)
            if (checkActive() == true && state != RunState.MOVE)
            {
                ////UnityEngine.//Debug.Log("=======================>>set Move!!! OK");
                setDestination(true);
                setMoveAnim(RunState.MOVE); //nState = RUNNER_MOVE;
            }
        }

        //땅볼시 움직임. 주루 상태를 판단하고 세팅한다.
        public void setMoveWhenGrounder()
        {
            ////UnityEngine.//Debug.Log("=======================================================================>>setMoveWhenGrounder");
            ////UnityEngine.//Debug.Log("====================>>>setMoveWhenGrounder :: groundCatchFielder = " + field.groundCatchFielder + "  ::bGrounderAvailble = " + field.bGrounderAvailble);
            if (field.bGrounderAvailble == true)
            {
                //그라운더가 잡힐것이라 예상하는 경우
                if (currentPos == FieldParm.FIRSTBASE_INDEX)
                {
                    //일루에 있는 경우 봉살 상태이기 때문에 무조건 뛰어
                    setMoveOnBase();
                }
                else if (currentPos == FieldParm.SECONDBASE_INDEX)
                {
                    //2루에 있는 경우 봉살이거나 볼이 1-2루사이이면 무조건 뛰어
                    //if (run.check2ndBaseForceOut() == true
                    //|| (field.groundCatchFielder == CPlayer._FIRSTBASEMAN || field.groundCatchFielder == CPlayer._SECONDBASEMAN))
                    if ((field.groundCatchFielder == CPlayer._FIRSTBASEMAN || field.groundCatchFielder == CPlayer._SECONDBASEMAN)
                      || run.check2ndBaseForceOut() == true
                      || bStealFlag == true)
                    //여기서 중요한거 --->>슬라이딩시 미스 나는 경우 뛰게 만들자
                    {
                        //////////UnityEngine.//Debug.Log("==================>>runnerIndex : "+runnerIndex+"=======>>3루로 진루");
                        setMoveOnBase();
                    }
                    else
                    {
                        //////////UnityEngine.//Debug.Log("==================>>2루에서 땅볼시 대기!!!!!!!!!!!!!!!!");
                        bGrounderWaitFlag = true;
                        setMoveBack();
                    }
                }
                else if (currentPos == FieldParm.THIRDBASE_INDEX)
                {
                    //3루에 있는 경우 봉살 이거나 2루 유격으로 공이 향하면 무조건 뛰어
                    //if (run.check3rdBaseForceOut() == true
                    //|| (field.groundCatchFielder == CPlayer._SHORTSTOP || field.groundCatchFielder == CPlayer._SECONDBASEMAN))
                    if ((field.groundCatchFielder == CPlayer._SHORTSTOP || field.groundCatchFielder == CPlayer._SECONDBASEMAN)
                    || run.check3rdBaseForceOut() == true
                    || bStealFlag == true)
                    //여기도 마찬가지 --->>슬라이딩시 미스 나는 경우 뛰게 만들자
                    {
                        //////////UnityEngine.//Debug.Log("==================>>runnerIndex : " + runnerIndex + "=======>>홈으로 진루");
                        setMoveOnBase();
                    }
                    else
                    {
                        ////UnityEngine.//Debug.Log("==================>>3루에서 땅볼시 대기!!!!!!!!!!!!!!!!");
                        bGrounderWaitFlag = true;
                        setMoveBack();
                    }
                }
            }
            else
            {
                //아닌경우 그냥 뛰어
                setMoveOnBase();
            }
        }

        //플라이볼 대기. 상태를 판단하고 세팅한다.
        public void setFlyBallWait()
        {
            //bool bCheckGo = false;
            float nDis = 1;

            if (field.ball.bFairBallGuess == false || field.ball.bFoulHomerunGuess == true)
            {
                nDis = getFlyballLeadRatio() / 2;
            }
            else
            {
                if (field.infieldFlyOut)
                {
                    setMoveBack();
                    return;
                }
                else
                {
                    if (field.flyCatchAvaiableCount > 0)
                    {
                        //베이스텍 처리
                        if (currentPos == FieldParm.THIRDBASE_INDEX || currentPos == FieldParm.SECONDBASE_INDEX)
                        {
                            if (checkBaseTagPossibe() == true)
                            {
                                setMoveBack();
                                baseTagPrepare = true;

                                if (run.bThirdBaseTag == false)
                                {
                                    //run.bThirdBaseTag: 3루 2루 겹칠경우 3루만 나타내주기 위한 플래그
                                    //UIFieldCamera.SetActive(true, this, field.manager);
                                    setCamera(true);
                                    if (currentPos == FieldParm.THIRDBASE_INDEX)
                                    {
                                        run.bThirdBaseTag = true;
                                    }
                                }

                                return;
                            }
                        }
                        //웨이트 처리          
                        //RUNNER_AI = 30;
                        //if (Random.Range(0, RUNNER_AI) < field.minFlyGab) ->이런식으로
                        nDis = getFlyballLeadRatio();
                        if (field.deepFlyOut == false)
                        {
                            nDis /= 2;
                        }
                    }
                    else
                    {
                        //잡는 놈 없으면 무조건 뛰어 -> 인공지능 최대
                        if (checkRunnerAI() == true)
                        {
                            setMove();
                            return;
                        }
                        else
                        {
                            nDis = getFlyballLeadRatio();
                        }
                    }
                }
            }


            //웨이트시 목표 재계산
            setDestination(true);
            //dest 재 계산
            float distanceX = (dstX - posX) * nDis;
            float distanceY = (dstY - posY) * nDis;

            dstX = posX + distanceX;
            dstY = posY + distanceY;
            curTime = 0;
            maxTime = distanceX / dX;
            state = RunState.CHECK;
            nextState = RunState.WAIT;


        }

        //베이스 택 세팅
        public void setBaseTag()
        {
            //////////UnityEngine.//Debug.Log("=================>>SET BASE TAG");            
            setMove();            
            baseTagPrepare = false;
            run.bOnlyOneBaseFlag = true;
        }

        //귀루 상태 세팅
        public void setMoveBack(bool bSlowMove = false)
        {
            ////UnityEngine.//Debug.Log("==================>>runnerIndex : "+runnerIndex +" setMoveBack!!!!!!!!!!!!!!!");
            if (checkActive() == true)//if (nState > RunningMechnism.GO_BENCH)
            {
                if (run.bHitAndRun == true)
                {
                    //히트앤드런시 딜레이 후 돌아가게 한다.
                    bForcedOutBackMove = true;
                    _strID = RunningMechnism._HOLD;
                    strID = _strID + FieldParm._dir[nDir];
                    loop = false;
                    state = RunState.REVERSE_DELAY;
                    curTime = 0;
                    maxTime = RunningMechnism.RUNNER_BACK_DELAY;
                    destPos = currentPos;
                    //StartCoroutine(reverseDelay(3.0f));
                    field.returnCheckInit();
                }
                else
                {
                    //////////UnityEngine.//Debug.Log("=======================>>set Move");
                    setDestination(false, bSlowMove);
                    setMoveAnim(RunState.MOVE);
                }
            }
        }

        /*
        private IEnumerator reverseDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            setDestination(false, false);
            setMoveAnim(RunState.MOVE);
        }*/

        //베이스 도착시 관련 상태와 이에 따른 상태업데이트를 반영한다
        public void setArriveBase(bool bAfterSliding = false)
        {
            //////////UnityEngine.//Debug.Log("======================>> setArriveBase");
            //bForcedOut = false;//->필요없음 필더가 체크해줌

            bForcedOneMoreBase2 = false;

            if (bSkillEffectOn == true)
            {
                //이펙트가 켜진경우 제거
                destroyEffect();
            }

            run.bOnBase[destPos] = true;
            run.bOnRunning[destPos] = false;

            run.bOnBackRunning[currentPos] = false;
            currentPos = destPos;


            if (destPos == FieldParm.HOMEBASE_INDEX)
            {
                //////////UnityEngine.//Debug.Log("======================>> destPos == FieldParm.HOMEBASE_INDEX");
                checkBaseHit();
                
                if (field.bErrorFlag == true)
                {
                    bErrorRunner = true;
                }
                setAddScore();
                field.returnCheckNC(-1.5f);
            }
            else
            {
                // ////////UnityEngine.//Debug.Log("======================>> checkOneMoreBase");
                bool bOneMoreBaseResult = false;
                
                if(Mode.bPvpMode433 == true)
                {
                    bool bEvent = false;
                    oneMoreSkillSense = SimulOverrunState.NONE;
                    if (field.ball.bHomeRunCall == true)
                    {
                        //홈런인 경우
                        bEvent = true;
                        bOneMoreBaseResult = true;
                    }

                    if (run.bRunnerWalk == true || run.bWildPitchRunning == true || run.bPickOff == true)
                    {
                        //Debug.Log("=====================>> 견제, 폭투, 포볼시 강제 한베이스만....");
                        bEvent = true;
                        bOneMoreBaseResult = false;
                    }

                    if (bEvent == false)
                    {
                        //PVP
                        if (field.manager.bMyTurn == true)
                        {
                            bOneMoreBaseResult = checkOneMoreBase(bAfterSliding);
                            //값 송신
                            pvpmanager.Get().SendOnemorebaseInfo(destPos, bOneMoreBaseResult, oneMoreSkillSense);
                        }
                        else
                        {
                            //네트워크에서 받아옴
                            bOneMoreBaseResult = field.manager.Pvp_OneMore[destPos];
                            oneMoreSkillSense = field.manager.Pvp_moreSkillSense[destPos];
                        }
                    }
                }
                else
                {
                    //일반
                    bOneMoreBaseResult = checkOneMoreBase(bAfterSliding);
                }

                if (bOneMoreBaseResult == true)
                {
                    startX = posX;
                    startY = posY;
                    //setMove();
                    setSkillActive(currentPos, true);
                    int foreIndex = run.getForeRunnerStandbyStateIndex(currentPos, runnerIndex);
                    if (foreIndex != -1)
                    {
                        Runner foreRunner = run.runner[foreIndex];
                        if (foreRunner.currentPos != FieldParm.HOMEBASE_INDEX)
                        {
                            ////Debug.Log("======================>> foreIndex :: 선행주자 강제 진루 = " + foreIndex);
                            ////Debug.Log("======================>> runnerIndex = " + runnerIndex + " =====> hitterRunner = " + run.nHitterRunnerIndex);
                            if (foreRunner.state == RunState.STANDBY ||  //스탠바이 상태이거나
                                foreRunner.state == RunState.WAIT ||     //웨이트 상태이거나
                                (foreRunner.state == RunState.MOVE && bMoveForward == false)) //뒤로 움직이는 상태인경우
                            {
                                ////Debug.Log("======================>> 서있는 경우 강제 진루");
                                foreRunner.setDestination(true);
                                foreRunner.setMoveAnim(RunState.MOVE);
                            }
                            else
                            {
                                ////Debug.Log("======================>> 아닌 경우 강제 진루");
                                //아직 베이스에 도착 안한 경우 oneMoreBase 플래그 온
                                //////UnityEngine.//Debug.Log("==============>>현재주자 destPos / 다음 주자 destPos = " + destPos + " / " + run.runner[foreIndex].destPos);
                                //////UnityEngine.//Debug.Log("==============>>현재주자 currentPos / 다음 주자 currentPos = " + currentPos + " / " + run.runner[foreIndex].currentPos);
                                //if (foreRunner.currentPos != FieldParm.HOMEBASE_INDEX)
                                {
                                    if (destPos == foreRunner.destPos)
                                    {
                                        ////Debug.Log("==============>>다음주자와 목적지가 같은 경우");
                                        if (foreRunner.state == RunState.SECOND_THIRD_SAFE)
                                        {
                                            ////Debug.Log("=================================================>>다음주자와 목적지가 같은 경우");
                                            foreRunner.bForcedOneMoreBase2 = true;
                                        }
                                        else
                                        {
                                            ////Debug.Log("=================================================>>다음주자와 목적지가 같은 경우가 아닌때");
                                            foreRunner.bForcedOneMoreBase = true;
                                        }
                                    }
                                }
                            }
                        }

                    }
                    setBaseTurning();
                    setDestination(true);
                                        
                    if (oneMoreSkillSense == SimulOverrunState.SAFE)
                    {                        
                        //needMoreTime
                        float basictime = (baseDistance / curSpeed);
                        curSpeed = baseDistance / (basictime - needMoreTime);
                        dX = curSpeed * Mathf.Cos(baseAngle);
                        dY = curSpeed * Mathf.Sin(baseAngle);                        
                                          
                        ////UnityEngine.//Debug.Log("================================================>>bOneMoreSkillSense 사용  새로 계산한 curSpeed = " + curSpeed);
                        ////UnityEngine.//Debug.Log("==================================================>>>한베이스 더계산 runnerIndex: " +runnerIndex + "   dX = " + dX+"   dY = "+dY);                        
                        curDX = dX;
                        curDY = dY;

                        //턴을 살릴경우
                        turnDX = curDX;
                        turnDY = curDY;
                        turnSpeed = curSpeed;

                        //턴을 죽일경우
                        //bBaseTurning = false;          
                        //if (destPos == FieldParm.HOMEBASE_INDEX) nDir = FieldParm._SOUTHEAST;
                        //else if (destPos == FieldParm.SECONDBASE_INDEX) nDir = FieldParm._NORTHWEST;
                        //else if (destPos == FieldParm.THIRDBASE_INDEX) nDir = FieldParm._SOUTHWEST;

                        oneMoreSkillSense = SimulOverrunState.NONE;
                    }
                    else if (oneMoreSkillSense == SimulOverrunState.OUT)
                    {
                        ////UnityEngine.//Debug.Log("=======================================================>>아웃이전 curSpeed = " + curSpeed);
                        curSpeed = curSpeed * 0.9f;
                        ////UnityEngine.//Debug.Log("=======================================================>>변경 curSpeed = " + curSpeed);
                        dX = dX * 0.9f;
                        dY = dY * 0.9f;
                        aX = aX * 0.7f;
                        aY = aY * 0.7f;
                        oneMoreSkillSense = SimulOverrunState.NONE;
                    }

                    setMoveAnim(RunState.MOVE);
                    field.returnCheckInit();
                    targetBase = destPos;
                    if (destPos > field.nCheckBaseNum)
                    {
                        field.nCheckBaseNum = destPos;
                    }


                }
                else
                {
                    ////////UnityEngine.//Debug.Log("======================>> setArriveBase bAfterSliding = " + bAfterSliding);

                    if (runnerIndex == run.nHitterRunnerIndex)
                    {
                        //////////UnityEngine.//Debug.Log("======================>> HITTER RUNNER SAFE");
                        checkBaseHit();
                        run.bHitterRunnerSafe = true;
                    }
                    else
                    {
                        //////UnityEngine.//Debug.Log("======================>> Steal Check = " + field.bFieldStealFlag);
                        if (field.bFieldStealFlag == true || field.bFieldDelayStealFlag == true)
                        {
                            field.manager.setStealCount(arrayIndex);
                            
                            if (currentPos > lastPos)
                            {     
                                /*
                                if (field.bFieldDelayStealFlag == true)
                                    SimulManager.AddGameSummuryInfo("\n-" + (lastPos + 1) + "루주자 " + pRunner.getName() + ": 홈스틸 성공");
                                else
                                    SimulManager.AddGameSummuryInfo("\n-" + (lastPos + 1) + "루주자 " + pRunner.getName() + ": " + (currentPos + 1) + "루 도루 성공");
                                */
                                lastPos = currentPos;
                            }
                        }
                        else
                        {
                            if (currentPos > lastPos)
                            {                                
                                //SimulManager.AddGameSummuryInfo("\n-" + (lastPos + 1) + "루주자 " + pRunner.getName() + ": " + (currentPos + 1) + "루까지 진루");
                                lastPos = currentPos;
                            }
                        }
                    }

                    //여기가 다이나믹한 애니메이션의 총집합체임  ㅋㅋㅋ
                    if (run.bRunnerWalk == false && bMoveForward == true)
                    {                        
                        if (currentPos == FieldParm.FIRSTBASE_INDEX)
                        {
                            ////////UnityEngine.//Debug.Log("======================>> 11111111111111");
                            //일루에서 죽을 똥을 싸는
                            setStandby(BaseArriveMotion._FIRSTBASE_RUN_ARRIVE);
                        }
                        else if (currentPos == FieldParm.SECONDBASE_INDEX || currentPos == FieldParm.THIRDBASE_INDEX)
                        {
                            //_SENCOND_THIRD_ARRIVE
                            //////UnityEngine.//Debug.Log("======================>> 2루 서서 도착!!!!!!!!!!!");
                            setStandby(BaseArriveMotion._SENCOND_THIRD_ARRIVE, bAfterSliding);
                        }
                        else
                        {
                            setStandby(BaseArriveMotion._NORMAL, bAfterSliding);
                        }
                    }
                    else
                    {
                        //노멀하게
                        ////////UnityEngine.//Debug.Log("======================>> 33333333333333");
                        setStandby(BaseArriveMotion._NORMAL, bAfterSliding, bMoveForward);
                    }
                                        
                    if (field.forcedSetBattingView(0.7f) == false)
                    {
                        field.returnCheck(-1.3f);
                    }
                }
            }
        }

        //주자가 아웃되어 벤치로 가는 상태 셋팅
        bool bNotBenchYet;
        bool bBenchAlready;
        public void setBench(RunState nextState, RunnerOutMotion motion, float _time = 1.5f)
        {
            
            bool bAnimChange = true;
            outMotion = motion;

            if (bSkillEffectOn == true)
            {
                destroyEffect();
            }

            //////UnityEngine.//Debug.Log("=========================>>setBench");

            //조정스피드를 현재스피드로 돌려놈
            curSpeed = RUNNER_SPEED; 
            curDX = dX = curSpeed * Mathf.Cos(baseAngle);
            curDY = dY = curSpeed * Mathf.Sin(baseAngle);
            //여기까지

            if (bForcedOutBackMove == true)
            {
                outMotion = RunnerOutMotion._NORMAL;
            }
            

            if (outMotion == RunnerOutMotion._FIRSTBASE_RUN_OUT)
            {
                bAnimChange = false;
                maxTime = 1.5f;
                if (checkClosePlay() == true)//  Mathf.Abs(posX - dstX) < RunningMechnism.CLOSE_PLAY_RANGE)
                {
                    ////////UnityEngine.//Debug.Log("=========================>>여기");
                    bNotBenchYet = true;
                    if (MyMath.Percent() < 60)
                        setSpecialAnim(RunningMechnism._FIRSTOUT_TYPE1, false);
                    else
                    {
                        setSpecialAnim(RunningMechnism._FIRSTOUT_TYPE2, false);
                        maxTime = 3.1f;
                    }

#if _OrthoCamera
                    field.ball.setRunnerFocus(arrayIndex);
                    //##연출 타자주자 1루 저번 아웃
                    field.setZoomTo(1.5f, 1);
#endif

                }
                else
                {
                    setSpecialAnim(RunningMechnism._FIRSTOUT_TYPE1, false);
                }
                dstX = field.getOriginX(FieldSize.getBasePosX(FieldParm.FIRSTBASE_INDEX));
                dstY = field.getOriginY(FieldSize.getBasePosY(FieldParm.FIRSTBASE_INDEX));

            }
            else if (outMotion == RunnerOutMotion._SECONDBASE_SLIDING_OUT || outMotion == RunnerOutMotion._FIRSTBASE_SLIDING_OUT || outMotion == RunnerOutMotion._THIRDBASE_SLIDING_OUT)
            {
                ////////UnityEngine.//Debug.Log("=========================>>여기2");
                bNotBenchYet = true;
                bAnimChange = false;
                //dX = 0;
                //dY = 0;
                maxTime = 1.0f;

            }
            else if (outMotion == RunnerOutMotion._SECONDBASE_SKILL_OUT)
            {
                ////////UnityEngine.//Debug.Log("=========================>>여기2");
                bNotBenchYet = true;
                bAnimChange = false;
                //dX = 0;
                //dY = 0;
                maxTime = 3.0f;

            }
            else if (outMotion == RunnerOutMotion._HOMEBASE_SLIDING_OUT)
            {
                //UnityEngine.//Debug.Log("=========================>>홈 슬라이딩 아웃 state = "+ state);
                bNotBenchYet = true;
                bAnimChange = false;
                //dX = 0;
                //dY = 0;
                maxTime = 3.0f;

                if (bBenchAlready == false)
                {
                    bBenchAlready = true;

                    int percent = MyMath.Percent();
                    ////Debug.Log("======================>>덤블링 퍼센트 " + percent);
                    bool bBlockedDumbling = (percent > 70 ? true : false);

                    if (bBlockedDumbling == false)
                    {
                        setSpecialAnim("2020_SLIDING_HEAD_HOMEOUT1", false);
                    }
                    else
                    {
                        dX = curSpeed / 5;
                        dY = -curSpeed / 5;
                        setSpecialAnim("2020_SLIDING_HEAD_HOMEOUT2", false);
                        state = nextState;                        
                        curTime = 0;
                        nMotionStep = 99;
                        return;
                    }
                }
                
                //dX = dY = 0;
            }
            else if (outMotion == RunnerOutMotion._HOME_CEREMONY1)
            {
                //Debug.Log("distance = " + RunningMechnism.getDistance(posX - field.ball.nBallX, posY - field.ball.nBallY));
                if(RunningMechnism.getDistance(posX - field.ball.nBallX, posY - field.ball.nBallY) <  180)
                {
                    setSpecialAnim("2021_SLIDING_HOMESAFE1", false);
                    maxTime = 3;
                    posY -= 20;
                }
                else
                {
                    setSpecialAnim(MyMath.Half() ? "RUNNER_ROAR" : "2021_SLIDING_HOMESAFE2", false);
                    maxTime = 3;
                    posY -= 60;
                }
                bNotBenchYet = false;
                bAnimChange = false;                
                //maxTime = _time;
                dX = 0;
                dY = 0;
            }
            else if (outMotion == RunnerOutMotion._HOME_CEREMONY2)
            {
                setSpecialAnim(MyMath.Half() ? "2021_SLIDING_HOMESAFE3" : "2021_SLIDING_HOMESAFE4", true);
                bNotBenchYet = false;
                bAnimChange = false;    
                dX = 0;
                dY = -(curSpeed / 8);
                maxTime = 2.5f;
                posY -= 20;
            }
            else
            {
                bNotBenchYet = false;
                //            //////UnityEngine.//Debug.Log("=========================>>여기3");
                dstX = field.getOriginX(field.nBenchPosX);
                dstY = field.getOriginY(field.nBenchPosY);
                maxTime = _time;

                if (outMotion == RunnerOutMotion._NORMAL)
                {
                    float angleDir = Mathf.Atan2(dstY - posY, dstX - posX);
                    nDir = FieldParm.getDir(angleDir);

                    dX = curSpeed * 0.4f * Mathf.Cos(angleDir);
                    dY = curSpeed * 0.4f * Mathf.Sin(angleDir);
                }
            }

            state = nextState;
            if (bAnimChange == true)
            {
                _strID = RunningMechnism._WALK;// +
                strID = _strID + FieldParm._dir[nDir];
                loop = true;
            }
            curTime = 0;
            nMotionStep = 0;
            
        }

        //주자가 아웃되어 벤치로 가는 상태 셋팅 - 위랑 비슷하나 좀 틀림
        public void setRunnerBench(bool forceOut, bool slidingOut, bool bMotionAble, bool skillOut = false)	//주자가 죽어서 벤치로 향하게 함
        {
            run.bOnRunning[destPos] = false;

            run.bOnBase[currentPos] = false;


            RunnerOutMotion _motion = RunnerOutMotion._NORMAL;


            if (bMotionAble == true)
            {
                _motion = RunningMechnism.getOutMotion(forceOut, slidingOut, destPos, skillOut);
            }

            setBench(RunState.GO_BENCH, _motion);

            curTime = 0;
        }

        //주자가 포스 아웃 상태 세팅
        public void setForceOut()
        {
            if (state == RunState.MOVE || state == RunState.SLIDING)
            {
                if (checkForcedOut() == true)
                {
                    //outText();
                    setRunnerBench(true, (state == RunState.SLIDING ? true : false), true);
                    field.setGroundOut();
                    checkBaseHitButOut();
                    field.setThrowAvailabe(); //포스 아웃인 경우 다시 던짐 가능
                }
            }
        }


        public float basePositionRate()
        {
            return RunningMechnism.getDistance(posX - startX, posY - startY) / baseDistance;
        }

        
        //스킬에 의한 승부 가능 체크
        public int checkShobuPossible(bool bInfield)
        {
            if (bMoveForward == true && bMoving == true)
            {
                //float rate = RunningMechnism.getDistance(posX - startX, posY - startY) / baseDistance;
                float rate = basePositionRate();
                ////UnityEngine.//Debug.Log("===========================>>rate = " + rate);
                if (rate < (bInfield ? 0.4f : 0.5f))
                {
                    return 1;
                }
                else if (rate > 0.85f)
                {
                    return 2;
                }
            }

            return 0;
        }

        public bool checkShobuLose(int value)
        {
            //true인 경우 주자 패배
            //false인 경우 주자 승리
            return false;
        }

        //리와인드 플레이시 주자 아웃 세잎
        public void setShobuRunnerSpeed(bool bOut, float timeLeft, float limit, bool bFirstBaseGrounder = false)
        {
            if (field.errorType != FieldParm.ErrorType.None) return; //유심히 살펴볼것 (noShobuFlag를 만드는게 나을지도)
            
            //Debug.Log("============================>> bOut: "+ bOut+ "   setRewindRunnerSpeed curSpeed= " + curSpeed);
            float distance =  RunningMechnism.getDistance(dstX - posX, dstY - posY);
            baseAngle = RunningMechnism.getAngle(dstX - posX, dstY - posY);

            float lastSpeed = curSpeed;

            if (bOut == true)
            {
                float newSpeed = distance / (timeLeft + Random.Range(0.25f, 0.29f));
                if (newSpeed < curSpeed)
                {
                    curSpeed = newSpeed;
                    RUNNER_SPEED = curSpeed;
                }
            }
            else
            {
                if (bFirstBaseGrounder == true)
                {
                    curSpeed = curSpeed * 1.5f;
                }
                else
                {
                    float limitSpeed = curSpeed * limit;// 2.2f;
                    curSpeed = distance / (timeLeft - 0.05f);
                    if (curSpeed > limitSpeed)
                    {
                        curSpeed = limitSpeed;
                    }
                }
                if (curSpeed < lastSpeed) curSpeed = lastSpeed;
            }
            //Debug.Log("============================>>setRewindRunnerSpeed 변환후 curSpeed= " + curSpeed);
            

            dX = curSpeed * Mathf.Cos(baseAngle);
            dY = curSpeed * Mathf.Sin(baseAngle);

            curDX = dX;
            curDY = dY;

            if (bBaseTurning == true)
            {
                if (destPos == FieldParm.FIRSTBASE_INDEX) nDir = FieldParm._NORTHEAST;
                else if (destPos == FieldParm.SECONDBASE_INDEX) nDir = FieldParm._NORTHWEST;
                else if (destPos == FieldParm.SECONDBASE_INDEX) nDir = FieldParm._SOUTHWEST;
                else nDir = FieldParm._SOUTHEAST;
            }

            bBaseTurning = false;

        }


        ///////////////////////////////////////////////////////
        //도루 관련
        ///////////////////////////////////////////////////////    
        //도루 상태 셋팅
        public void set_Steal_Pickoff(bool bActive, bool bPickOff = false)
        {
            if (bPickOff == false)
            {
                //도루인 경우
                bStealFlag = bActive;
                field.bFieldStealFlag = bActive;
                stealDelay = RunningMechnism.STEAL_DELAY;
            }
            else
            {
                bPickOffFlag = bActive;
                field.bFieldPickOffFlag = bActive;                
                pickOffDelay = RunningMechnism.STEAL_DELAY;
            }
            
        }

        //도루 플래그 On여부를 판단
        public bool checkSteal()
        {
            if (bStealFlag == true)
            {
                //setMoveOnBase();
                setDestination(true);
                state = RunState.STEAL;
                nextState = RunState.MOVE;
                curTime = 0;
                bRunnerActive = true;
                return true;
            }
            return false;
        }


        //픽오프 상태로 세팅
        public void setPickOff()
        {
            if (bPickOffFlag == true)
            {
                if (field.bPickOffOut == false)// SimulSteal.pitcherLaserPickoff == FieldSkillUse.Fail)
                {
                    //견제시 살은 경우
                    curSpeed = RunningMechnism.PICKOFF_SAFE_SPEED;// 250;
                    pickOffDelay = RunningMechnism.PICKOFF_SAFE_DELAY;
                }
                else
                {
                    //견제시 죽은 경우
                    curSpeed = RunningMechnism.PICKOFF_OUT_SPEED;// 150;
                    pickOffDelay = RunningMechnism.PICKOFF_OUT_DELAY;
                    StartCoroutine(setErrorMark(0.3f));//, 0.7f));
                }                
                
                setDestination(false);

                float leadRate = 1;
                if (currentPos == FieldParm.FIRSTBASE_INDEX)
                {
                    posX -= (60 + 40 * leadRate); //100
                    posY += (90 + 25 * leadRate); //115;
                    dstX -= 40;
                    dstY += 25;
                    nDir = FieldParm._SOUTHEAST;
                }
                else if (currentPos == FieldParm.THIRDBASE_INDEX)
                {
                    nDir = FieldParm._NORTHWEST;
                }
                else
                {
                    posX -= (80 + 50 * leadRate);//130;
                    posY -= (40 + 25 * leadRate);// 65;
                    dstX -= 50; //60
                    dstY -= 25; // 30;
                    nDir = FieldParm._NORTHEAST;
                }

                state = RunState.PICKOFF;
                curTime = 0;
                nMotionStep = 0;

            }
        }


        ///////////////////////////////////////////////////////
        //리드 관련
        ///////////////////////////////////////////////////////    
        //리드 상태 셋팅
        public void setLead()
        {
            /*    if (runnerIndex != run.nHitterRunnerIndex)
                {
                    //if (nState > RUNNER_GO_BENCH && nState != RUNNER_MOVE)
                    {
                        posX = field.getOriginX(FieldSize.getBasePosX(currentPos));
                        posY = field.getOriginY(FieldSize.getBasePosY(currentPos,field.fRatio));
                        bLead = true;
                        setMoveAnim(nState);
                    }
                }*/
        }



        ///////////////////////////////////////////////////////
        //주루 스킬
        ///////////////////////////////////////////////////////
        /// <summary>
        /// 스킬 발동 여부를 체크
        /// </summary>
        /// <returns>true를 리턴한경우 스킬이 발동함</returns>
        public bool checkSkillOn(SkillIndex index)
        {
            return pRunner.fieldSkillSuccess(index);
        }

        /// <summary>
        /// 스킬 초기화
        /// </summary>
        /// <param name="runner"></param>
        public void initSkill(CPlayer runner)
        {
            runnerAbil = pRunner.getSpeed() + pRunner.getFieldBonusValue();
            //값을 초기화
            sRunningSense = 0;
            sLead = 0;
            sSliding = 0;
        }
        
        /// <summary>
        /// 스킬을 활성화 한다 (조건을 검색후)
        /// </summary>
        /// <param name="curPos"></param>
        /// <param name="bOneMoreBase"></param>
        private void setSkillActive(int curPos, bool bOneMoreBase = false)
        {            
            //슬라이딩 스킬
#if _TEST_SKILL_BALANCE
        //sSliding = (byte)field._TEST_SLIDING;
#endif
            //Debug.Log("매우중요매우중요=================================>>>주루 스킬 발동여부 체크");
            //Debug.Log(curPos + " 루주자 테스트 랜덤 " + Random.Range(0, 1000));
           
            //홈돌진
            bRushSkillOn = false;
            if (curPos == FieldParm.THIRDBASE_INDEX && runnerIndex != run.nHitterRunnerIndex)
            {
                if(pRunner.skillAvailable(SkillIndex.RunnerHomeRush) == true) 
                {
                    //발동확률은 이전에 계산함
                   bRushSkillOn = true;
                }
            }

            //터보
            bTurboSkillOn = false;
            if (destPos == FieldParm.FIRSTBASE_INDEX && runnerIndex == run.nHitterRunnerIndex)
            {
                if(pRunner.fieldSkillSuccess(SkillIndex.RunnerTurbo) == true && MyMath.Percent() < 20)
                {
                    //발동확률
                    bTurboSkillOn = true;
                }
            }

            //병살저지
            bDoublePlaySkillOn = false;
            if (curPos == FieldParm.FIRSTBASE_INDEX && bOneMoreBase == false && field.manager.nOutCount < 2)
            {
                if (runnerIndex != run.nHitterRunnerIndex)
                {
                    if (MyMath.Percent() < 40)
                    {
                        if (pRunner.fieldSkillSuccess(SkillIndex.RunnerDoublePlayBreaker) == true)
                        {
                            //상황시 100%발생
                            slidingType = SlidingType._NORMAL;  //레그 슬라이딩
                            bDoublePlaySkillOn = true;
                        }
                    }
                }
            }

            /*
            //딜레이 스틸
            bDelayStealSkillOn = false;
            if (destPos == FieldParm.HOMEBASE_INDEX)
            {
                if(pRunner.skillAvailable(SkillIndex.RunnerDelayedHomeSteal) == true) 
                {
                    bDelayStealSkillOn = true;
                }
            }*/

        }

        /// <summary>
        /// 터보 엔진 스킬을 세팅한다
        /// </summary>
        /// <param name="bSteal"></param>
        public void setTurboOn(bool bSteal = false)
        {
            bMoveEffect = true;
            bClosePlay = true;
            ////////UnityEngine.//Debug.Log("===========>> 현재 스피드 curSpeed / RUNNER_SPEED: " + curSpeed + " / " + RUNNER_SPEED);
            //////UnityEngine.//Debug.Log("===========>> 현재 스피드 curSpeed / RUNNER_SPEED: " + (curDX / dX) + " / " + (curDY / dY));
            curSpeed = RUNNER_SPEED;

            if (bSteal == false)
            {
                //터보엔진인 경우
                curDX = dX;
                curDY = dY;
                field.runnerTurbo = FieldSkillUse.Active;
            }
            else
            {
                //스틸터보인 경우
                curDX = dX;
                curDY = dY; 
            }
            //////UnityEngine.//Debug.Log("===========>> 보정 스피드 curSpeed / RUNNER_SPEED: " + (curDX / dX) + " / " + (curDY / dY));        
            //////UnityEngine.//Debug.Log("===========>> 터보 스피드 curSpeed / RUNNER_SPEED: " + curSpeed + " / " + RUNNER_SPEED);

            bSkillEffectOn = true;
            

            if (bSteal == false)
            {
                if (destPos == FieldParm.FIRSTBASE_INDEX)
                {
                    setSpecialAnim("0101_FASTRUN_NE", true);
                }

                //대결효과 여부 체크
                field.bVsShow = false;
                if (field.groundCatchFielder > 0)
                {
                    Fielder vsFielder = field.fielder[field.groundCatchFielder];
                    if (vsFielder.checkSpecialThrowing() == true)
                    {
                        field.bVsShow = true;
                        fieldSkillDisplayManager.RemoveSkill(vsFielder.pFielder, SkillIndex.SpecialThrow);
                    }
                }

                if (field.bVsShow == false)
                {
                    //주루센스 - 터보 연출
                    fieldSkillDisplayManager.AddSkill(gameObject, pRunner, SkillIndex.RunnerTurbo);
                }
            }
            else
            {
                if (field.bVsShow == false)
                {
                    if (field.stealSuccess == true)
                    {
                        //질주본능 - 대도 연출
                        fieldSkillDisplayManager.AddSkill(gameObject, pRunner, SkillIndex.RunnerStealMaster);
                    }
                }
            }
        }

        /// <summary>
        /// 병살저지 스킬을 세팅한다
        /// </summary>
        public void setDoublePlayStopOn()
        {
            if (state != RunState.DOUBLEPLAY)
            {
                setSlidingAnim(SlidingType._DOUBLEPLAY_SLIDING, false);
                state = RunState.DOUBLEPLAY;

                //Debug.Log("=====================>> 병살저지 체크 랜덤테스트 = " + Random.Range(0,1000));
                bool bOffenseWin = true;
                int posIndex = field.getBaseCoverIndex(FieldParm.SECONDBASE_INDEX);
                //Debug.Log("=====================>> 1 ==> posIndex = " + posIndex);
                if (posIndex != -1)
                {                    
                    Fielder vsFielder = field.fielder[posIndex];
                    //Debug.Log("=====================>> 2  ===> specialthrow = " + vsFielder.checkSpecialThrowing());
                    if (vsFielder.checkSpecialThrowing() == true)
                    {
                        //Debug.Log("=====================>> 3");
                        bOffenseWin = SimulParm.checkOffenseSkillWin(pRunner.getSkillRank(SkillIndex.RunnerDoublePlayBreaker), vsFielder.pFielder.getSkillRank(SkillIndex.SpecialThrow));
                        //Debug.Log("=====================>> 병살저지 체크 bOffenseWin = " + bOffenseWin);
                    }
                }      
                
                if(bOffenseWin == true)
                {
                    field.runnerDPStop = FieldSkillUse.Success;
                }
                else
                {
                    field.runnerDPStop = FieldSkillUse.Fail;
                }                                
                
                bSkillEffectOn = true;                
                slidingTime = 1;

                if (bOffenseWin == true)
                {
                    //주루센스 - 병살저지 연출
                    fieldSkillDisplayManager.AddSkill(gameObject, pRunner, SkillIndex.RunnerDoublePlayBreaker);
                }
                else
                {

                }
            }
        }

                
        /// <summary>
        /// 홈돌진 스킬을 세팅한다.
        /// </summary>
        private bool bHomeRushWin;
        public void setHomeRushOn()
        {
            if (state != RunState.RUSH)
            {
                state = RunState.RUSH;
                //field.ball.setRunnerFocus(arrayIndex);
                run.homeShobu = HomeShobu._RUSH;
                nMotionStep = 0;
                nDir = FieldParm._SOUTHEAST;

                bSkillEffectOn = true;
                                
                field.runnerHomeRush = FieldSkillUse.Active;
                                
                Fielder catcher = field.fielder[CPlayer._CATCHER];
                if (catcher.pFielder.skillAvailable(SkillIndex.CatcherRunnerBlocking) == true)
                {
                    bHomeRushWin = field.setVsSkill(pRunner, catcher.pFielder, SkillIndex.RunnerHomeRush, SkillIndex.CatcherRunnerBlocking, 0.01f, 0.01f);
                    ////Debug.Log("====================>>> 대결연출 : 질주본능(홈돌진) vs 수비형포수(주자블럭)");
                }
                else
                {
                    //질주본능 - 홈돌진 연출
                    fieldSkillDisplayManager.AddSkill(gameObject, pRunner, SkillIndex.RunnerHomeRush);
                }
                //field.ball.setRunnerFocus(arrayIndex);

            }
        }

        /*
        //딜레이 스틸을 시전
        public void setDelayStealOn()
        {
            RUNNER_SPEED = RunningMechnism.getRunnerSpeed(RunningMechnism.STEAL_SPEED);// *Runner._RUNNER_MULTI;
            curSpeed = RUNNER_SPEED;
            accelRate = RunningMechnism.STEAL_ACCEL;// / Runner._RUNNER_MULTI;
            setMove();
            
            field.run.setStealInvalid(); //도루무효화
            field.runnerDelaySteal = FieldSkillUse.Active;//딜레이 스틸 온
            field.bFieldDelayStealFlag = true;  //현재 필드는 딜레이 스틸 상태

            //딜레이 홈스틸 연출
            field.manager.playingCount = 0; //강조를 위해 초기화
            field.setSkillEffect(false, pRunner, SkillIndex.RunnerDelayedHomeSteal);            
            field.ball.setRunnerFocus(arrayIndex);
            field.setZoomTo(1.3f, 1);
            run.bHomeSteal = true;
            
        }*/

        /*
        IEnumerator activeDelaySteal(float delay)
        {
            yield return new WaitForSeconds(delay);

            field.fielder[CPlayer._CATCHER].posX = field.getOriginX(FieldSize.getBasePosX(FieldParm.HOMEBASE_INDEX));
            field.fielder[CPlayer._CATCHER].posY = field.getOriginY(FieldSize.getBasePosY(FieldParm.HOMEBASE_INDEX));
            field.fielder[CPlayer._CATCHER].setBaseCover(FieldParm.HOMEBASE_INDEX);

            setDelayStealOn();
        }*/



        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //연출관련
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        //카메라를 세팅해준다
        public void setBvCamera(bool bActive)
        {

        }

        //이펙트를 디스트로이
        void destroyEffect()
        {
            bSkillEffectOn = false;
            if (skillEffect1 != null)
            {
                Destroy(skillEffect1);
            }
            if (skillEffect2 != null)
            {
                Destroy(skillEffect2);
            }
            if (lineEffect != null)
            {
                Destroy(lineEffect);
                lineEffect = null;
            }
        }

        //
        public void destroyLineEffect()
        {
            if (lineEffect != null)
            {
                Destroy(lineEffect);
                lineEffect = null;
            }
        }

        

        /*
        void makeDust(float delay)
        {
            slidingTime += deltaTime;
            if (slidingTime > delay)
            {
                Vector3 pos = transform.position;
                GameObject slidingEffect = Instantiate(Resources.Load("MainGame/prefabs/effectPrefab/field/fx_rundust"), pos, Quaternion.identity) as GameObject;
                Destroy(slidingEffect, 1.0f);
                slidingTime = 0;
            }
        }*/

        /*
        //진루시 아웃 텍스트
        private void outText()
        {
            if (runnerIndex != run.nHitterRunnerIndex)
            {
                if (run.bStealBase == false)
                {
                    SimulManager.AddGameSummuryInfo("\n-" + (lastPos + 1) + "루주자 " + pRunner.getName() + ": " + (destPos == FieldParm.HOMEBASE_INDEX ? "홈" : (destPos + 1) + "루") + "에서 아웃");
                }

            }
        }*/

        /// <summary>
        /// 에러마크
        /// </summary>
        /// <param name="delay1"></param>
        /// <returns></returns>
        private IEnumerator setErrorMark(float delay1)//, float delay2)
        {
            yield return new WaitForSeconds(delay1);

            errorMark.spriteId = errorMark.GetSpriteIdByName("hiticon");
            errorMark.gameObject.SetActive(true);
            errorMark.transform.localScale = Vector3.one;
            UITweener tween = TweenScale.Begin(errorMark.gameObject, 0.25f, new Vector3(2, 2, 1));
            tween.style = UITweener.Style.PingPong;

            yield return new WaitForSeconds(0.5f);

            errorMark.gameObject.SetActive(false);
        }

        /// <summary>
        /// 주루실수 마크
        /// </summary>
        /// <param name="bOut"></param>
        private void overRunningMark(bool bOut)
        {
            errorMark.spriteId = errorMark.GetSpriteIdByName(bOut?"nobrainicon":"onemoreicon");
            errorMark.gameObject.SetActive(true);
            errorMark.transform.localScale = Vector3.one;
            UITweener tween = TweenScale.Begin(errorMark.gameObject, 0.25f, new Vector3(2, 2, 1));
            tween.style = UITweener.Style.PingPong;
        }




        //충돌

        //OnTriggerStay
        private void OnTriggerStay(Collider col)
        {
            //if (col.gameObject.tag == "FIELDER_TAG")
            if (col.gameObject.CompareTag("FIELDER_TAG") == true)
            {
                if (state == RunState.STANDBY)
                {
                    Fielder colFielder = col.gameObject.GetComponent<Fielder>();
                    colFielder.collideWithRunner();
                }
            }
        }


    }
}


