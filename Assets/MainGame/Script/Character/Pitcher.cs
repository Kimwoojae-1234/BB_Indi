//#define _AI_ONLYFASTBALL        //지워지워
//#define _COURSE_NO_AI           //지워지워
//#define _PITCHER_CHANGE_TEST      //지워지워
#define _NOT_YET_GROUNDERVIEW   //아직냅둬 그라운더뷰 완성안됨
//#define _WILD_PITCH_TEST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace BaseBall.BallPlay
{

    public class Pitcher : MonoBehaviour
    {        
        //초기 포지셔닝
        public int _initPosX = 0;
        public int _initZOrder = -2;
        public int _initPosY = 422;
        //private float initScale = 1;    


        //오브젝트
        public PitchSystem pitch;
        public PitchSystem pitchPv;

        public BallPlayManager manager;
        public Batter batter;        
        public BBall bball;
        public bvFielder[] bvfielder;
        private Field field;
        private runnerManager run;
        private Batting battingview;
        public Zone zoneUI;
        //private GameObject _ball;
        private GameObject pitchingEffect;
        
        //애니메이션 오브젝트
        public SkeletonAnimation anim;
        public SkeletonAnimation pAnim;
        //이펙트 애니메이션
        public SkeletonAnimation effectAnim1;
        //먼지
        public SkeletonAnimation munjiAnim;

        
        //투수데이터
        public CPlayer pPitcher, pLastPitcher;


        //이벤트 애니메이션
        public tk2dSprite icon; //나중에 지울것
        bool bEventAnim;
        
        
        //상태
        private float curTime;                     //시간 
        private int stateStep;                     //스텝구분
        public PitcherState pState, lastState;     //투수 상태        
        public FatigueStep fatigueStep;
        private int curStamina;
                
        //구종
        public PitchingArsenal selectedBallIndex;           //선택된 구종
        //public float calibrationCoef;	        //구종별 파워 보정 계수
        public float curBallSpeed;              //현재 볼 스피드 (직구를 던질시)  

        public bool bBreakingBallType;
        
        //존 관련
        public float courseX, courseY, courseX2, courseY2;
        public float coursePvpX, coursePvpY;
        public float arriveX, arriveY;                          //현재 계산되는
        public float preArriveX, preArriveY;                    //미리 계산되는
        public float preHenkaX, preHenkaY;                      //미리 계산되는 변화량
        public float aiMissOffsetX, aiMissOffsetY;              //구위에 의해 계산되는
        
        //투구 플래그
        public bool bGetSign;           //사인을 주고 받았는지 여부
        public bool bSetPosition;		//현재 셋포지션 여부
        public bool bMissControl;       //컨트롤 미스 여부    
        public bool bPowerPitch;        //전력 투구 여부
        public bool bStartPitch;

        //현재 출격중인 투수 정보
        public string strName;
        public string strStat;
        public int condition;
        public int pitchHand;                           //투구 손
        public int pitchingType, pitchingType2;         //피칭 타입1: 오버 사이드 언더 /// 피칭타입2: 세분화

        
        //피칭 상태 플래그
        public bool bRelease;
        public int nSign;			            //좌우 부호	좌-1 우+1    
        private bool positionInit;
        public bool bTipHappen;
        public bool bCheckSwingAsk;

        public int hitByPitchStep;
        
        //public bool bPitchedOut;    //피치아웃
        public ControlValue controlValue;
        public UserControlValue userControlValue;
        public int pControl;
        public int pGuwee, pFinalGuwee;


        //폭투관련
        public bool bWildPitch;
        private float catcherBlockRate;
        private int pitcherWildRate;
        public int missType; //0:정상 1:한가운데 실투 2: 폭투

        
        //투수 교체 
        private ChangeType pitcherChangeType;   //투수 교체타입
        public int inPitcher, outPitcher;       //바뀌는 투수와 나오는 투수
        private CPlayer nextBatter, nextBatter2;        //다음 타자와 다음다음 타자까지 고려
        public bool bInningStart;               //이닝 시작 여부 -> 이때 교체??        
        

        //투수 기록 관련 (교체시 참조하기도 함)
        public int[] allowRun;      //실점허용
        public int[] pitchCount;    //투구수    
        public int[] startInning;// 시작이닝

        //투수교체 보직 관련 플래그
        public bool[] bLongReliefOn; //롱릴리프 출격
        public bool[] bChaseOn;      //추격조 출격
        public bool[] bSetupOn;      //셋업 출격   
        public bool[] bSaveOn;       //마무리 출격   

        //자책 관련
        public bool bEarnedRunFlag;
        public bool bNoMoreEarnRun;

        //핀치,위기, 대위기
        public PinchStep pinchState;        
        public int conHit,  //연속안타
                   conHR,   //연속홈런
                   conRun;   //연속실점
        //public int allowChulu;  //출루허용
        

        //스킬
        private int skillOrder;


        //피칭뷰 포수
        public Catcher catcher;
        public bool bCatcherMoveFlag;
         
        //피칭뷰 주심
        public pvJudge pvjudge;


        //PVP 피치 타이머
        public bool bPitchTimerOn;

    
        //////////////////////////////////////////////////////////////////////////////////////////
        //초기화
        //////////////////////////////////////////////////////////////////////////////////////////       
        //인스턴스 초기화
        public void initInstance(BallPlayManager manager)
        {
            //Debug.Log("투수 인스턴스 초기화");
            //투수는 여기 이전에 이미지 리소스 생성 필요
            //오직 시뮬레이션만 하는 경우에는 초기에 리소스를 설정해주지 않는다
            //배팅뷰용

#if GIRL_PLAY
            GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/pitcher/pitcherSkelPrefab_girl", transform, Vector3.zero, "skeleton");
            skeleton.transform.localScale = new Vector3(12, 12, 1);
#else
            GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/pitcher/pitcherSkelPrefab", transform, Vector3.zero, "skeleton");
            skeleton.transform.localScale = new Vector3(60, 60, 1);
#endif
            anim = skeleton.GetComponent<SkeletonAnimation>();


            GameObject skeleton2 = Util.Load("MainGame/prefabs/skeleton/pPitcher/pPitcherSkelPrefab", transform, Vector3.zero, "skeleton2");
            skeleton2.transform.localScale = new Vector3(100, 100, 1);
            pAnim = skeleton2.GetComponent<SkeletonAnimation>();


            effectAnim1.transform.localPosition = new Vector3(0, 0, -0.01f);
            effectAnim1.transform.localScale = new Vector3(100.0f, 100.0f, 1);
            effectAnim1.gameObject.SetActive(false);

            //
            munjiAnim = transform.Find("munji").gameObject.GetComponent<SkeletonAnimation>();
            munjiAnim.transform.localPosition = new Vector3(-90, 170, -0.01f); //정통파 기준
            munjiAnim.transform.localScale = new Vector3(100.0f, 100.0f, 1);
            munjiAnim.gameObject.SetActive(false);

            initStateStep();

            anim.state.Event += HandleEvent;
            pAnim.state.Event += HandleEvent;
            lastState = PitcherState._NONE;

            bReadyPossible = true;

            this.manager = manager;
            batter = manager.batter;
            field = manager.field;
            run = field.run;
            battingview = manager.battingview;
            //camera = manager.camera;    
            //피치시스템 초기화
            pitch = manager.pitch;
            pitch.battingSystem.initInstance(manager);
            //투수뷰 피치시스템 초기화
            pitchPv = manager.pitchPv;
            pitchPv.battingSystemPv.initInstance(manager);


            GameObject _ball = Util.Load("MainGame/prefabs/BattingViewPrefab/bballPrefab", transform, Vector3.zero);
            bball = _ball.GetComponent<BBall>();

            catcher = Util.Load("MainGame/prefabs/BattingViewPrefab/catcherPrefab", transform, Vector3.zero).GetComponent<Catcher>();
            catcher.initInstance(this);

            pvjudge = Util.Load("MainGame/prefabs/BattingViewPrefab/pvJudgePrefab", transform, Vector3.zero).GetComponent<pvJudge>();

            parmInit();

        }

        //파라미터 초기화
        private void parmInit()
        {
            //기록
            allowRun = new int[2];  //실점허용
            pitchCount = new int[2];      //투구수

            //투수교체 플래그 ->저장정보임
            bLongReliefOn = new bool[2];
            bChaseOn = new bool[2];
            bSetupOn = new bool[2];
            bSaveOn = new bool[2];

            //기록-이닝 관련 -> 저장정보임
            startInning = new int[2];
            //totalInning = new int[2];	

        }

        //시간및 스텝 초기화
        public void initStateStep()
        {
            curTime = 0;
            stateStep = 0;
        }

        //위치 초기화
        public void initPosition()
        {
            if (Mode.cameraView == CameraView.PitcherCenter)  //if (manager.bMyTurn == false)// 
            {
                _initPosX = -nSign * 210;
                _initPosY = 80;
                _initZOrder = -100;
                pAnim.gameObject.GetComponent<Renderer>().enabled = true; //투수 그림
                catcher.setIdle();
                pvjudge.setIdle();
            }
            else //if (Mode.cameraView == CameraView.BatterLow)
            {
                _initPosX = 0;
                _initPosY = 336;
                _initZOrder = -2;
                anim.gameObject.GetComponent<Renderer>().enabled = true; //투수 그림
            }
                        
            if (battingview != null)
            {
                transform.parent = battingview.transform;
                transform.localPosition = new Vector3(_initPosX, _initPosY, _initZOrder);
            }                        
            transform.localScale = new Vector3(nSign, 1, 1);
            
            
        }

        //투수의 그래픽적 초기화

        private string teamPath = null,
                       facePath = null,
                       logoPath = null;

        public void loadPitcher(CPlayer player)
        {
            int index = manager.bTopInning ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex;
            
            //0:글러브 1:몸통 2:스타일 3:데칼 4:스파이크 5:모자
            if (player != null)
            {
                if(manager.bMyTurn == true || Mode.bPitchingViewActive == false) // if(Mode.cameraView == CameraView.BatterLow)// if (bPvState == false)
                {
#if GIRL_PLAY
#else
                    //타자뷰
                    AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
                    Material[] materials = atlasdata.materials;

                    string strPath = "MainGame/spineData/battingview/pitcher/";
                    //string itemPath;

                    //팀
                    string curTeamPath = strPath + "team/" + index + "/pitcherAnim";  //팀디폴트
                    if (curTeamPath != teamPath)
                    {
                        teamPath = curTeamPath;
                        materials[0].mainTexture = (Texture)Resources.Load(teamPath);
                    }
                                        
                    //스타일
                    int faceIndex = player.getFace();
                    string curFacePath = strPath + "face/" + faceIndex + "/pitcherAnim2";
                    if (curFacePath != facePath)
                    {
                        facePath = curFacePath;
                        materials[1].mainTexture = (Texture)Resources.Load(facePath);
                    }

                    //로고
                    string curLogoPath = strPath + (nSign == 1 ? "logoRight/" : "logoLeft/") + index + "/pitcherAnim3";
                    if (curLogoPath != logoPath)
                    {
                        logoPath = curLogoPath;
                        materials[2].mainTexture = (Texture)Resources.Load(logoPath);
                    }
#endif
                    
                }
                else ///if (Mode.cameraView == CameraView.PitcherCenter)//
                {
                    //투수뷰
                    AtlasAsset atlasdata = pAnim.skeletonDataAsset.atlasAssets[0];
                    Material[] materials = atlasdata.materials;

                    string strPath = "MainGame/spineData/pitchingview/pitcher/";

                    string curTeamPath = strPath + "team/" + index + "/pvPitcherAnim";  //팀디폴트
                    if (curTeamPath != teamPath)
                    {
                        teamPath = curTeamPath;
                        materials[0].mainTexture = (Texture)Resources.Load(teamPath);
                    }
                                        
                    //스타일
                    int faceIndex = player.getFace();
                    string curFacePath = strPath + "face/" + faceIndex + "/pvPitcherAnim2";
                    if (curFacePath != facePath)
                    {
                        facePath = curFacePath;
                        materials[1].mainTexture = (Texture)Resources.Load(facePath);
                    }
                }
            }

            
        }

        
        //투수의 위치 옵셋
        public void setPitcherPosOffset(float x, float y)
        {
            transform.localPosition = new Vector3(_initPosX + x, _initPosY + y, _initZOrder);
        }

        //볼의 옵셋
        public void setBallPosOffset(float x, float y)
        {
            //_ball.transform.localPosition = new Vector3(_initPosX + x, _initPosY + y, _initZOrder - 1);
        }

        //배팅뷰 상의 야수 세팅
        public void setBVFielder()
        {
            ////Debug.Log("================================>>배팅뷰 주자 텍스쳐 로딩");
            bvfielder = new bvFielder[2];

            GameObject second = Util.Load("MainGame/prefabs/BattingViewPrefab/bvFielderPrefab", battingview.transform, Vector3.zero);
            GameObject shortstop = Util.Load("MainGame/prefabs/BattingViewPrefab/bvFielderPrefab", battingview.transform, Vector3.zero);

            int initPosY1 = 205;//292;
            int initPosX = 400;
            float scale1 = 0.35f;


            bvfielder[0] = second.GetComponent<bvFielder>();
            bvfielder[1] = shortstop.GetComponent<bvFielder>();

            second.transform.parent = battingview._runnerPosition.transform;
            shortstop.transform.parent = battingview._runnerPosition.transform;


            second.transform.localPosition = new Vector3(initPosX, initPosY1, -1.8f);
            shortstop.transform.localPosition = new Vector3(-initPosX, initPosY1, -1.8f);

            second.transform.localScale = new Vector3(scale1, scale1, 1);
            shortstop.transform.localScale = new Vector3(scale1, scale1, 1);

            //텍스쳐 로딩 - 한번만
            bvfielder[0].loadTexture();
            //주자 텍스쳐 로딩 - 한번만
            battingview._1stRunner.loadTexture();
        }


        /// <summary>
        /// 카메라 세팅에 따른 설정
        /// </summary>
        /// 
        public bool bPvState;
        public void setCameraSetting()
        {
            //Debug.Log("==========================>> 투수의 카메라 세팅 pitchHand = " + pitchHand);
            if (Mode.cameraView == CameraView.PitcherCenter) //if (manager.bMyTurn == false)//
            {
                bPvState = true;
                pitchPv.gameObject.SetActive(true); //피칭뷰 피치 시스템 활성화
                float scale = (1.0f / 0.9f);
                pitchPv.transform.localScale = new Vector3(scale, scale, 1);
                pitch.gameObject.SetActive(false);  //타격뷰 피치 시스템 비활성화
                pAnim.gameObject.SetActive(true);
                anim.gameObject.SetActive(false);
                effectAnim1.transform.localScale = new Vector3(150, 150, 1);
                effectAnim1.gameObject.layer = LayerMask.NameToLayer("BATTER_LAYER");
                //피칭뷰 좌우
                manager.battingview.setPitchingView(nSign);

                //피칭뷰 포수 주심
                bool bLeftPitcher = (pitchHand == CPlayer._LEFTHAND ? true : false);
                catcher.gameObject.SetActive(true);
                catcher.loadCatcher();
                catcher.initPosition(bLeftPitcher, battingview.gameObject);
                pvjudge.gameObject.SetActive(true);
                pvjudge.initPosition(bLeftPitcher, battingview.gameObject);

                munjiAnim.transform.localPosition = new Vector3(160, 350, -0.01f); //정통파 기준
                munjiAnim.transform.localScale = new Vector3(80.0f, 80.0f, 1);

                IngameUI.GetPitchUI().SetPitchUIInitPos(true, bLeftPitcher);

                //pitcherAnim("WAIT_01", true, true);
            }
            else
            {
                bPvState = false;
                pitchPv.gameObject.SetActive(false); //피칭뷰 피치 시스템 비활성화
                pitch.gameObject.SetActive(true);    //타격뷰 피치 시스템 활성화
                float scale = (1.0f / 0.9f);
                pitch.transform.localScale = new Vector3(scale, scale, 1);
                pAnim.gameObject.SetActive(false);
                anim.gameObject.SetActive(true);
                effectAnim1.transform.localScale = new Vector3(100, 100, 1);
                effectAnim1.gameObject.layer = LayerMask.NameToLayer("BATTINGVIEW_LAYER");
                catcher.gameObject.SetActive(false);
                pvjudge.gameObject.SetActive(false);

                //먼지위치
                munjiAnim.transform.localPosition = new Vector3(-90, 170, -0.01f); //정통파 기준
                munjiAnim.transform.localScale = new Vector3(100.0f, 100.0f, 1);

                //실투 아이콘 위치
                icon.transform.localPosition = new Vector3(55, 120, -0.1f);

                IngameUI.GetPitchUI().SetPitchUIInitPos(false, false);
            }
            setSpositionString();
            initPosition();
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //기본 애니메이션
        //////////////////////////////////////////////////////////////////////////////////////////    

        //스파인 이벤트 처리
        public void HandleEvent(Spine.TrackEntry trackEntry, Spine.Event e)//HandleEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
        {
            //Debug.Log(trackIndex + " " + state.GetCurrent(trackIndex) + ": event " + e + ", " + e.Int);
            string strEvent = e.ToString();

            if (strEvent == "release")
            {
                if (bRelease == false)
                {
                    StartCoroutine(releaseDelay());
                }
            }
        }

        //스파인 이벤트 엔드 처리
        private void EndEvent(Spine.AnimationState state, int trackIndex)
        {
            //Debug.Log(trackIndex + " " + state.GetCurrent(trackIndex));
        }

        //투수 애니메이션
        //int lastTrack = 0;
        string strID ="";
        public void pitcherAnim(string strAnim, bool bLoop, bool bAnimInit = false)
        {
            ////Debug.Log("================>>pitcherAnim");
            if (strAnim != strID)
            {
#if GIRL_PLAY
                if (anim.state.Data.skeletonData.FindAnimation(strAnim) != null)
                {
                    if (bPvState == true)
                    {
                        pAnim.state.ClearTracks();
                        pAnim.skeleton.SetToSetupPose();//.SetSlotsToSetupPose();// .SetToSetupPose();
                        pAnim.state.SetAnimation(0, strAnim, bLoop);
                        pAnim.timeScale = 1.0f;
                    }
                    else
                    {
                        anim.state.ClearTracks();
                        anim.skeleton.SetToSetupPose(); //.SetSlotsToSetupPose();//.SetToSetupPose();
                        anim.state.SetAnimation(0, strAnim, bLoop);
                        anim.timeScale = 1.0f;
                    }
                }
                else
                {
                    Debug.LogError("투수 에러 " + strAnim);
                }
#else
                    if (bPvState == true)
                    {
                        pAnim.state.ClearTracks();
                        pAnim.skeleton.SetToSetupPose();//.SetSlotsToSetupPose();// .SetToSetupPose();
                        pAnim.state.SetAnimation(0, strAnim, bLoop);
                        pAnim.timeScale = 1.0f;
                    }
                    else
                    {
                        anim.state.ClearTracks();
                        anim.skeleton.SetToSetupPose(); //.SetSlotsToSetupPose();//.SetToSetupPose();
                        anim.state.SetAnimation(0, strAnim, bLoop);
                        anim.timeScale = 1.0f;
                    }                
#endif
                strID = strAnim;
            }
            //lastTrack = track;
        }

        //스파인 애니메이션 초기화
        public void initAnim()
        {
            bEventAnim = false;
            if (bPvState == true)
            {
                pAnim.state.ClearTracks();
                pAnim.skeleton.SetToSetupPose();
            }
            else
            {
                anim.state.ClearTracks();
                anim.skeleton.SetToSetupPose();
            }
        }

        //이펙트 애니메이션
        private void effectAnim(SkeletonAnimation effectAnim, int track, string strAnim, bool bLoop)
        {
            
                effectAnim.gameObject.SetActive(true);
                if (effectAnim.state.Data.skeletonData.FindAnimation(strAnim) != null)
                {
                    effectAnim.state.ClearTracks();
                    effectAnim.skeleton.SetToSetupPose();
                    effectAnim.state.SetAnimation(track, strAnim, bLoop);
                    effectAnim.timeScale = 1.0f;
                }
        }

        /// <summary>
        /// 외부에서 제어
        /// </summary>
        /// <param name="bActive"></param>
        /// <param name="strAnim"></param>
        /// <param name="duration"></param>
        public void AuraEffect(bool bActive, string strAnim, Vector3 pos, Vector3 scale)//, Color color)
        {
            if (bActive == true)
            {
                effectAnim1.transform.localPosition = pos;
                effectAnim1.transform.localScale = scale * 100;
                //if (effectAnim1.state.Data.skeletonData.FindAnimation(strAnim) != null)
                {                    
                    effectAnim(effectAnim1, 0, strAnim, false);
                    //if (color != Color.white) effectAnim1.skeleton.SetColor(color);
                }
            }
            else
            {
                effectAnim1.skeleton.SetColor(new Color(1, 1, 1, 1));
                effectAnim1.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 외부에서 애니메이션 제어
        /// </summary>
        private string lastAnim;
        private bool bLastLoop;
        public void AnimEffect(bool bActive, string strAnim)
        {
            if (bActive == true)
            {
                lastAnim = strID;
                bLastLoop = (bPvState ? pAnim.loop : anim.loop);
                pitcherAnim(strAnim, true);
            }
            else
            {
                pitcherAnim(lastAnim, bLastLoop);
            }
        }

        /////////////////////////////////////////////////////////////////////
        //투수 초기화 함수 - 매이닝 혹은 교체시 호출 된다.
        /////////////////////////////////////////////////////////////////////
        //투수의 초기화 - 매이닝시작, 혹은 교체시 호출
        public bool bNewPitcher;
        public void initPitcher(CPlayer player, int team, bool bNewInning = false)
        {
            //Debug.Log("=================================>>> Init Pitcher");
            ////UnityEngine.Debug.Log("[INIT PITCHER]====>>이름: " + player.getName());
            ////UnityEngine.Debug.Log("[INIT PITCHER]====>>구속: " + player.getBallSpeed() + "====>제구: " + player.getControl() + "====>체력: " + player.getStamina());
            bPauseQue = false;
            bNewPitcher = true;
            bPitchCeremony = false;
            bReadyPossible = true;
            bool bInjury;

            //새이닝에 이전 투수는 없음
            if (bNewInning == true)
            {
                this.pLastPitcher = null;
            }
            //지금 투수 설정
            this.pPitcher = player;

            //스킬 초기화
            pPitcher.setBonusInit();

            /*
            if (field.fielder[CPlayer._PITCHER].pFielder != null)
            {
                //필더 이름 설정
                field.fielder[CPlayer._PITCHER].name.text = pPitcher.getName();
            }*/

             
            bEarnedRunFlag = false;
            bNoMoreEarnRun = false;


            //투수의 교체 타입 설정
            setChangeType(pPitcher);
            

            //기본 프로퍼티
            strName = pPitcher.getName();           //이름 얻어오기
            pitchHand = pPitcher.getThrowHand();	//현재 투수 던지는 어깨 설정

            nSign = (pitchHand == CPlayer._LEFTHAND ? -1 : 1); //좌우 사인
            pitchingType = player.getPitchingType();	//현재 투수 던지는 타입 (투구폼)     

            
            //컨디션
            condition = 0;// pitcher.m_nCondition;

            //부상 여부
            bInjury = false;
            if (bInjury)
            {
                //부상시 상태 설정
                setInjury();
            }


            //셋 피치
            //setPitch();

            
            //출루 카운트 초기화
            SimulManager.GetGameInfo().allowChulu = 0;

            //투수 UI 초기화
            //manager.gameUI.setPitcherInfo(team, player); //[UI]투수 UI 초기화

            //와일드 피치
            setWildParm();


            //투수 리소스 로딩
            loadPitcher(player);

            bEventAnim = false;

            
            fastballCon = 0;


            //스태미너 관련 상태 초기화
            setStaminaTotalUpdate();
            conHit = conHR = conRun = 0; 
            
        }


        /////////////////////////////////////////////////////////////////////
        //투구 프로세스에 관련된 각종 세팅
        /////////////////////////////////////////////////////////////////////
        
        //Set Pitch (피칭 세팅)
        //매 피치 초기때마다 호출한다
        public void setPitch()
        {
            Debug.Log("SetPitch");
            /*if (Mode.bPvpMode == true)
            {
                PvpManager.GetInstance().SetPitch();
                field.bFieldFirst = manager.bMyTurn;
            }*/
                        

            bPitchTimerOn = false;

            bPitchStart = false;
            //Debug.Log("SET PITCH");
            //setPitchSystemDraw(true);
            //bPitchFinish = false;
            //manager.nDeadBallStep = 0;	//데드볼 초기화

            //스킬 연출 초기화
            SkillEffectDisplayManager.InitSkill();

            //뜬금포, 강습타구같이 타석초기 발생하지만 매구 연출을 유지하는 스킬(CPU타석의 경우만) 예외처리
            if (manager.bMyTurn == false)
            {                
                if (manager.batterSkillFlag == SkillFlag.Unexpected)
                {
                    CPlayer pBatter = batter.pBatter;
                    if (pBatter.skillAvailable(SkillIndex.Unexpected) == true)
                    {
                        //뜬금포 연출 재장전
                        CSkill curSkill = pBatter.getSkillValue(SkillIndex.Unexpected);
                        if (curSkill != null) SkillEffectDisplayManager.AddSkill(curSkill);
                    }
                }
                else if (manager.batterSkillFlag == SkillFlag.AssaultBall)
                {
                    CPlayer pBatter = batter.pBatter;
                    if (pBatter.skillAvailable(SkillIndex.AssaultBall) == true)
                    {
                        //강습타구 연출 재장전
                        CSkill curSkill = pBatter.getSkillValue(SkillIndex.AssaultBall);
                        if (curSkill != null) SkillEffectDisplayManager.AddSkill(curSkill);
                    }
                }
            }

            //특정 플래그가 서있는 경우 초기화
            manager.pitchSkillBatterWin(true); //초기화겸용 함수임

            
            manager.bSqueeze = false;
            manager.setNewCount();
            
            /*if (manager.bChangeFlag) //교체 플래그 활성화 여부
            {
                manager.bChangeFlag = false;

                if (Mode.bPvpMode == false && Mode.b2outBaseLoadedMode == false)
                {
                    if (Mode.bAutoPlay == true || manager.bMyTurn == true)
                    {
                        if (checkPitcherChanged() == true)
                        {
                            manager.bPitcherChangeFlag = true;
                            //////UnityEngine.//Debug.Log("==================>>피처체인지 체크 여기 먼저?");
                            return;
                        }
                    }
                }
            }*/

            manager.saveGame1();
            if (manager.bSaveGame2)
            {
                manager.saveGame2();
                manager.bSaveGame2 = false;
            }
            bMissControl = false;



            //볼 관련 초기화
            pState = PitcherState._GET_SIGN;
            bRelease = false;
            bGetSign = false;
            bPowerPitch = false;
            positionInit = false;
            bTipHappen = false;
            bCheckSwingAsk = false;
            bStartPitch = false;
            bWildPitch = false;
            missType = 0; //정상투구

            hitByPitchStep = 0;

            //필드 관련 초기화
            //field.initPitcher(true);  //원래 여기있었음
            //field.run.initRunner2();  //원래 여기있었음

            //커서 관련 초기화
            if (bPvState == true)
            {
                pitchPv.pitchOriginPv.setPitchCursor(false);
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.setPitchCursor(false);
            }
            
            //실투 아이콘 숨기기
            icon.gameObject.SetActive(false);
            //먼지 숨기기
            munjiAnim.gameObject.SetActive(false);

            //타자 관련 초기화
            batter.setBatter();

            //존 관련
            zoneUI.setZone(true, true, false); //피칭커서 세팅O, 스트라이크존type2 강제세팅X
            zoneUI.setBatCursorAlpha(1);

            //포수
            bCatcherMoveFlag = false;

            run.setRunnerLead();

            initStateStep();


            //피치단위로 스킬 검색
            SimulManager.CheckSkillByPitch();


            System.GC.Collect();
        }

        //Select Ball (구종선택)
        //볼을 선택하고 해당 관련된 프로퍼티로 세팅해주는 함수
        public void setBallSelect(PitchingArsenal selectBallType)
        {
            if (bPvState == true)
            {                
                pitcherAnim("SIGN_02", false, !bReadyPossible);
                catcher.setIdle();
                //if (bSetPosition == true)
                {
                    Invoke("setPitchPosition", 1);
                }
                bReadyPossible = false;
            }

            //bSignDisAgree = false;

            ////UnityEngine.//Debug.Log("========================>> 내가 구종 선택 = _selected = " + _selected);
            setFastballCon(selectBallType <= PitchingArsenal.RISING ? true : false);
            //나의 직구 스피드 산출
            setCurBallSpeed(selectBallType);
            //내가 구종 선택
            setBallAndGuwee(selectBallType); 
            setBatterMeet();

            setMiss(false);
           
        }


        //Set Ball Speed
        //얻어온 구종과 나의 능력치를 통해 구속을 세팅한다
        private void setCurBallSpeed(PitchingArsenal selectBall)
        {
            curBallSpeed = getBallSpeedValue(selectBall) + Random.Range(-3, 3);    //직구스피드는 능력치별로 산정할것
        }

        //계속해서 직구류 혹은 변화구류를 연속으로 던질경우에 대한 디스어드벤티지를 적용하는 세팅
        public int fastballCon;
        private void setFastballCon(bool bFastBall)
        {
            if (fastballCon > 0)
            {
                if (bFastBall) fastballCon++;
                else fastballCon = 0;
            }
            else if (fastballCon < 0)
            {
                if (bFastBall) fastballCon = 0;
                else fastballCon--;
            }
            else
            {
                if (bFastBall) fastballCon = 1;
                else fastballCon = -1;
            }
        }


        //투수의 선택된 구종으로부터 최종 구위를 구함 (궁극적인 중요)
        public void setBallAndGuwee(PitchingArsenal selectBallType)
        {
            ////Debug.Log("=======================>>setBallAndGuwee ::          curBallSpeed = " + curBallSpeed);
            selectedBallIndex = selectBallType;
            //bool henkaActive = true;
            float henkaAngle = 0;
            //int henkaNum = 4;
            float movementAX = 0;
            float movementAY = 0;
            int pHandSign = nSign;

            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.ballType = selectBallType;
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.ballType = selectBallType;
            }

            //현재 구위 구하기
            int curIndex = (int)PitchingMechanism.getBallType(selectBallType);
            int value = (int)(pPitcher.getBallValue2(curIndex) + pPitcher.getGuweeBonus());
            ////Debug.Log("============>>원래 구위 = " + value);
            pGuwee = PitchingMechanism.reductionByFatigue(value, fatigueStep);
            ////Debug.Log("============>>체력 감안 구위 = " + pGuwee);

            //인덱스에 의해 볼 판별
            if (selectBallType == PitchingArsenal.FASTBALL)
            {
                setMoveTypeAndGuwee(BallMoveType.Straight);
                //직구
                //henkaActive = false;
            }
            else if (selectBallType == PitchingArsenal.RISING)
            {
                //라이징
                setMoveTypeAndGuwee(BallMoveType.FastBreaking);
                //henkaNum = 1;
                movementAX = 0;
                movementAY = 6;
            }
            else if (selectBallType == PitchingArsenal.TWOSEAM)
            {
                //투심
                setMoveTypeAndGuwee(BallMoveType.FastBreaking);
                //henkaNum = 1;
                movementAX = -11 * pHandSign;
                movementAY = -3;
            }
            //커브류
            else if (selectBallType == PitchingArsenal.CURVE)
            {
                setMoveTypeAndGuwee(BallMoveType.Curve);
                //henkaNum = 3;
                movementAX = 5 * pHandSign;
                movementAY = -10;
            }
            else if (selectBallType == PitchingArsenal.SLOW_CURVE)
            {
                setMoveTypeAndGuwee(BallMoveType.Curve);
                movementAX = 4 * pHandSign;
                movementAY = -10;
                //henkaNum = 4;
            }
            else if (selectBallType == PitchingArsenal.POWER_CURVE)
            {
                setMoveTypeAndGuwee(BallMoveType.Curve);
                //henkaNum = 3;
                movementAX = 2 * pHandSign;
                movementAY = -10;
            }
            else if (selectBallType == PitchingArsenal.POKPOSU_CURVE)
            {
                setMoveTypeAndGuwee(BallMoveType.Curve);
                //henkaNum = 4;
                movementAY = -10;
            }
            else if (selectBallType == PitchingArsenal.GIRO_CURVE)
            {
                setMoveTypeAndGuwee(BallMoveType.Curve);
                //henkaNum = 4;
                movementAX = -5 * pHandSign;
                movementAY = -10;
            }
            else if (selectBallType == PitchingArsenal.KNUCKLE_CURVE)
            {
                setMoveTypeAndGuwee(BallMoveType.Curve);
                //henkaNum = 3;
                movementAX = 0;
                movementAY = -10;
            }
            else if (selectBallType == PitchingArsenal.UPSHOOT)
            {
                setMoveTypeAndGuwee(BallMoveType.Straight);
                //henkaNum = 2;
                movementAX = 0;
                movementAY = 10;
            }

            //체인지업류
            else if (selectBallType == PitchingArsenal.CHANGEUP)
            {
                setMoveTypeAndGuwee(BallMoveType.OffSpeed);
                //henkaNum = 2;
                movementAX = 0;// 0 * pHandSign * pMovement1;
                movementAY = -15;
            }
            else if (selectBallType == PitchingArsenal.CIRCLE)
            {
                setMoveTypeAndGuwee(BallMoveType.OffSpeed);
                //henkaNum = 2;
                movementAX = -6 * pHandSign;
                movementAY = -12;
            }
            else if (selectBallType == PitchingArsenal.VULCAN)
            {
                setMoveTypeAndGuwee(BallMoveType.OffSpeed);
                //henkaNum = 2;
                movementAX = -20 * pHandSign;
                movementAY = -12;
            }
            else if (selectBallType == PitchingArsenal.PALM)
            {
                setMoveTypeAndGuwee(BallMoveType.OffSpeed);
                movementAX = 0;// 0 * pHandSign * pMovement1;
                movementAY = -7;
                //henkaNum = 3;
            }
            
            else if (selectBallType == PitchingArsenal.KNUCKLE)
            {
                setMoveTypeAndGuwee(BallMoveType.OffSpeed);
                movementAX = 0;// 0 * pHandSign * pMovement1;
                movementAY = -7;
                //henkaNum = 4;
            }

            //슬라이더류
            else if (selectBallType == PitchingArsenal.SLIDER)
            {
                setMoveTypeAndGuwee(BallMoveType.Slide);
                movementAX = 20 * pHandSign;
                movementAY = -4;
                //henkaNum = 2;
            }            
            else if (selectBallType == PitchingArsenal.H_SLIDER)
            {
                setMoveTypeAndGuwee(BallMoveType.Slide);
                movementAX = 25 * pHandSign;
                movementAY = 0;
                //henkaNum = 2;
            }
            else if (selectBallType == PitchingArsenal.SLURVE)
            {
                setMoveTypeAndGuwee(BallMoveType.Slide);
                movementAX = 12 * pHandSign;
                movementAY = -12;
                //henkaNum = 3;
            }
            else if (selectBallType == PitchingArsenal.CUT_FAST)
            {
                //커터
                setMoveTypeAndGuwee(BallMoveType.FastBreaking);
                movementAX = 10 * pHandSign;
                movementAY = -4;
                //henkaNum = 1;
            }
            else if (selectBallType == PitchingArsenal.FRISBEE)
            {
                setMoveTypeAndGuwee(BallMoveType.Slide);
                movementAX = 18 * pHandSign;
                movementAY = 0;
                //henkaNum = 5;
            }

            //포크류
            else if (selectBallType == PitchingArsenal.FORK)
            {
                setMoveTypeAndGuwee(BallMoveType.OffSpeed);
                //henkaNum = 2;
                movementAX = 0;// 0 * pHandSign * pMovement1;
                movementAY = -8;
            }
            else if (selectBallType == PitchingArsenal.H_FORK)
            {
                setMoveTypeAndGuwee(BallMoveType.OffSpeed);
                //henkaNum = 2;
                movementAX = 0;// 0 * pHandSign * pMovement1;
                movementAY = -10;
            }
            else if (selectBallType == PitchingArsenal.SFF)
            {
                //SFF
                setMoveTypeAndGuwee(BallMoveType.OffSpeed);
                //henkaNum = 1;
                movementAX = 0;
                movementAY = -14;
            }
            else if (selectBallType == PitchingArsenal.SINKER)
            {
                setMoveTypeAndGuwee(BallMoveType.Slide);
                //henkaNum = 2;
                movementAX = -10 * pHandSign;
                movementAY = -12;
            }
            
            else if (selectBallType == PitchingArsenal.SINKING_FAST)
            {
                //하드싱커
                setMoveTypeAndGuwee(BallMoveType.FastBreaking);
                //henkaNum = 1;
                movementAX = -8 * pHandSign;
                movementAY = -15;
            }
            
            henkaAngle = Mathf.Atan2(movementAY, movementAX) * Mathf.Rad2Deg;
            //zoneUI.setHenka(henkaActive, henkaAngle, henkaNum);          
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.getCurArrivePos(Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT, curBallSpeed);
                preHenkaX = -pitchPv.pitchOriginPv.curOffsetX;
                preHenkaY = pitchPv.pitchOriginPv.curOffsetY;
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.getCurArrivePos(Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT, curBallSpeed);
                preHenkaX = pitch.pitchOrigin.curOffsetX;
                preHenkaY = pitch.pitchOrigin.curOffsetY;
            }
        }

        

        //Set Batter Meet
        //투타의 컨택관련 사전 데이터를 설정
        private void setBatterMeet()
        {
            //파워풀 버전에서는 핵심적인 역활을 했으나 지금은 글쎼?
            //투수의 구위와 타자의 능력치 간에 상대적 역학관계를 설정한다.
            //결국에는 이 함수가 투타 밸런스의 핵심
            //이전 작업에서도 가장 많이 손을 댄 함수이기도 함
        }


        //릴리즈 하면서 생기는 미세 컨트롤 미스
        public void setReleaseContolMiss()
        {
            //Debug.Log("정상루트이거나 PVP투수");
            if (bMissControl == false)
            {
                if (Mode.bPvpMode433 == false)//if (Mode.bPvpMode == false)
                {
                    courseX += Random.Range(-7.0f, 7.0f);
                    courseY += Random.Range(-7.0f, 7.0f);
                }
                courseX2 = courseX;
                courseY2 = courseY;
                pitchSetZone(courseX2, courseY2, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
            }
        }

        public void setReleasePosPvp()
        {
            //Debug.Log("PVP대전에서 타자인경우");
            courseX = coursePvpX;
            courseY = coursePvpY;
            courseX2 = courseX;
            courseY2 = courseY;
            pitchSetZone(courseX2, courseY2, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
            //Debug.Log("실제찍히는값===============>> courseX = " + manager.pitcher.coursePvpX);
            //Debug.Log("실제찍히는값===============>> courseY = " + manager.pitcher.coursePvpY);
        }

        //유저 컨트롤 하면서 생기는 컨트롤 미스값을 설정 (퍼펙트:완벽히 컨트롤 -> 배드 :나쁜 컨트롤 -> 폭투 : 제멋대로 컨트롤)
        public void setUserControlValue2(UserControlValue value)
        {
            //주목
            //실투 계산
            userControlValue = value;
            if (userControlValue == UserControlValue.Miss)
            {
                setMiss(true);
                bMissControl = true;
                controlValue = ControlValue.Miss;
                zoneUI.setBatCursorPos(0, 0, 100, 130);
                //bool bStrkie = true;
                //if (Mathf.Abs(courseX) > Zone.STRIKE_ZONE_WIDTH || Mathf.Abs(courseY) > Zone.STRIKE_ZONE_HEIGHT) bStrkie = false;
                setMissControl();
                courseX2 = courseX;
                courseY2 = courseY;

            }
            else
            {
                float offsetX, offsetY;
                float signX = MyMath.Half() ? -1 : 1;
                float signY = MyMath.Half() ? -1 : 1;
                if (userControlValue == UserControlValue.Perfect)
                {
                    offsetX = Random.Range(0, 3);
                    offsetY = Random.Range(0, 3);
                }
                else if (userControlValue == UserControlValue.Good)
                {
                    offsetX = Random.Range(3, 7);
                    offsetY = Random.Range(3, 7);
                }
                else
                {
                    offsetX = Random.Range(7, 20);
                    offsetY = Random.Range(7, 20);
                }
                courseX2 = courseX + (signX * offsetX);
                courseY2 = courseY + (signY * offsetY);

                courseX = courseX2;
                courseY = courseY2;
              
            }
            //Debug.Log("코스값 확정!!!");
            pitchSetZone(courseX2, courseY2, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
        }



        //userControlValue와 최종 탄착점을 통해 최종 컨트롤 밸류를 정한다
        private void setControlValue(float x, float y)
        {
            if (userControlValue == UserControlValue.Miss)
            {
                controlValue = ControlValue.Miss;
                pFinalGuwee = pGuwee / 2;
                return;
            }


            float gabX = Mathf.Abs(x);
            float gabY = Mathf.Abs(y);

            float reverseGabX = Mathf.Abs((Mathf.Abs(x) - Zone.UI_ZONE_WIDTH));
            float reverseGabY = Mathf.Abs((Mathf.Abs(y) - Zone.UI_ZONE_HEIGHT));

            ////Debug.Log("====================>>gabX = " + gabX + "======>reverseGabX = " + reverseGabX);
            ////Debug.Log("====================>>gabY = " + gabY + "======>reverseGabY = " + reverseGabY);


            if (gabX > (Zone.UI_BADZONEX) || gabY > (Zone.UI_BADZONEY))
            {
                ////Debug.Log("====================>>택돌이 볼로 제구");
                controlValue = ControlValue.Bad;
            }
            else
            {
                if (reverseGabX < (Zone.UI_PINPOINTX) && reverseGabY < (Zone.UI_PINPONITY))
                {
                    ////Debug.Log("====================>>핀포인트 제구");
                    //핀포인트 제구시
                    if (userControlValue <= UserControlValue.Good)
                    {
                        //노멀이하 무조건 핀포인트제구값 설정
                        controlValue = (ControlValue)userControlValue;
                    }
                    else
                    {
                        //2단계 위 값 설정
                        controlValue = (ControlValue)((int)userControlValue - 1);
                    }
                }
                else
                {
                    if (reverseGabX < (Zone.UI_PINPOINTX) || reverseGabY < (Zone.UI_PINPONITY))
                    {
                        ////Debug.Log("====================>>굿 제구");
                        //굿 제구
                        if (userControlValue <= UserControlValue.Good)
                        {
                            //최고는 굿
                            controlValue = ControlValue.Good;
                        }
                        else
                        {
                            //1단계 위 값 설정
                            controlValue = (ControlValue)((int)userControlValue - 1);
                        }
                    }
                    else
                    {
                        if (gabX < (Zone.UI_CENTERZONEX) && gabY < (Zone.UI_CENTERZONEY))
                        {
                            ////Debug.Log("====================>>가운데 몰림 제구");
                            controlValue = ControlValue.Bad;
                        }
                        else
                        {
                            ////Debug.Log("====================>>기타 제구");
                            if (userControlValue <= UserControlValue.Normal)
                            {
                                controlValue = ControlValue.Normal;
                            }
                            else
                            {
                                controlValue = (ControlValue)userControlValue;
                            }
                        }
                    }
                }
            }

            ////Debug.Log("====================>>controlValue = " + controlValue + "======>>userControlValue = " + userControlValue);

            //setFinalGuwee();

            if (Mode.b2outBaseLoadedMode == true)
            {
                pFinalGuwee = PitchingMechanism.GetFinalGuweeNineTwo(manager.nineTwoRound);
            }
            else
            {
                pFinalGuwee = PitchingMechanism.GetFinalGuwee(pGuwee, controlValue);
                if (Mode.bPvpMode433 == true) //if (Mode.bPvpMode == true)
                {                    
                    pFinalGuwee = (pFinalGuwee * 8) / 10;
                    //Debug.Log("PVP모드 최종구위 80%감소      pFinalGuwee : "+ pFinalGuwee);
                }
                else
                {
                    if (Mode.bOnlyChanceMode == true)
                    {
                        //Debug.Log("=====================================>>> 찬스모드시 구위 보너스 이전 pFinalGuwee = " + pFinalGuwee);
                        if (manager.bMyTurn == true) pFinalGuwee = (pFinalGuwee * 9) / 10;
                        else pFinalGuwee = (pFinalGuwee * 11) / 10;
                        //Debug.Log("=====================================>>> 찬스모드시 구위 보너스 이후 pFinalGuwee = " + pFinalGuwee);
                    }
                }
            }
            //체크스윙 세팅
            batter.setCheckSwing(pFinalGuwee);

        }
        
        //컨트롤 미스세팅
        private void setMissControl()
        {
            int limit = 50;
            
            /*//행운아 세팅
            if (bLuckyGuyOn == true)
            {
                limit = -10;
            }
            */

#if _WILD_PITCH_TEST
            limit =  -10;    //지워지워
#endif

            if (MyMath.Percent() < limit)
            {
                missType = 1; //한가운데 실투
                //한가운데로 몰리는
                courseX = Random.Range(-10, 10);
                courseY = Random.Range(-10, 10);
                controlValue = ControlValue.Miss;
                batter.cursorX = batter.cursorY = 0;                
            }
            else
            {
                /*
                if (Mode.bAutoPlay == true)
                {
                    //오토모드에서는 빠지는 실투는 안걸들게
                    batter.autoModeBatting = AutoModeBatting.BaseOnBall;
                }*/

                controlValue = ControlValue.Normal; //임시
                missType = 2; //폭투
                //공이 빠지는
                bool bSideMiss = MyMath.Half();
                bool bFarAndUp = MyMath.Half();

#if _WILD_PITCH_TEST
                bSideMiss = false;    //지워지워
                bFarAndUp = false;    //지워지워
#endif

                if (bSideMiss == true)
                {
                    //좌우 미스
                    //bFarAndUp 가 false인 경우 힛바이 피치 발생
                    courseY = Random.Range(-50, 50);
                    int sideOffset = Random.Range(0,30) + (bPvState==true?100:170);
                    if (bFarAndUp == true)
                    {
                        //외각으로 빠짐
                        int sign = (bPvState ? -1 : 1);
                        courseX = sign * (Zone.UI_ZONE_WIDTH + sideOffset);
                    }
                    else
                    {
                        //힛바이 피치 발생
                        int sign = (bPvState ? 1 : -1);
                        hitByPitchStep = 1;
                        setPitchSystemHitByPitched();
                        courseX = sign * (Zone.UI_ZONE_WIDTH + sideOffset);
                    }
                    courseX = batter.sign * courseX;
                }
                else
                {
                    // 상하 미스
                    courseX = Random.Range(-100, 100);
                    int upDownOffset = Random.Range(0, 30) + (bPvState == true ? 60 : 120);
                    if (bFarAndUp == true)
                    {
                        //위로 미스
                        courseY = (Zone.UI_ZONE_HEIGHT + upDownOffset);
                    }
                    else
                    {
                        //아래로 미스
                        courseY = -(Zone.UI_ZONE_HEIGHT + upDownOffset);
                        if (bPvState == true)
                        {
                            courseY = Mathf.Clamp(courseY, -80, -110);
                        }
                    }
                }
            }
            
        }

        //스트라이크 볼여부를 세팅해줌
        //투구된 볼이 존을 지나면 호출됨 -> 타구가 발생되면 무시된다.
        public void setCount()
        {
            if (batter.bHitted == true) return;

            zoneUI.setZone(true, false, false); //피칭커서 세팅X, 스트라이크존type2 강제세팅X

            manager.bStrikeCheck = true;

            arriveX = getCurrentZoneX(Zone.STRIKE_ZONE_WIDTH);
            arriveY = getCurrentZoneY(Zone.STRIKE_ZONE_HEIGHT);


            ////Debug.Log("==========>> arriveX = " + arriveX + " ====>preArriveX = " + preArriveX);
            ////Debug.Log("==========>> arriveY = " + arriveY + " ====>preArriveY = " + preArriveY);

            //미트질 특능
            
            //스트라이크 체크
            if ((Mathf.Abs(arriveX) > Zone.UI_ZONE_WIDTH || Mathf.Abs(arriveY) > Zone.UI_ZONE_HEIGHT)
                && batter.bSwing == false && batter.bBunt == false)
            {
                manager.bStrikeCheck = false;
            }
                        
            //batter.tryToSwing();
            
            //와일드 피치 체크
            /*if (arriveY < -(Zone.UI_ZONE_HEIGHT + 20))
            {
                if (run.stealResult == SimulStealState.NONE) //도루경우 무효화
                {
                    if(checkWildPitch() == true)
                    {
                        int range = MyMath.Percent();
                        if (bMissControl == true ||  range > pitcherWildRate)
                        {
                            bWildPitch = true;
                        }
                    }
                }
            }*/

            //탄착점 나타내줘
            if (manager.bStrikeCheck == true)
            {
                zoneUI.setArriveCursorPos(true);
            }
            else
            {
                zoneUI.setArriveCursorPos(false);
            }

            initStateStep();
        }


        //기타
        //파울팁 처리
        private IEnumerator setTipInfo()
        {
            yield return new WaitForSeconds(0.8f);

            zoneUI.setZone(true, false, false); //피칭커서 세팅X, 스트라이크존type2 강제세팅X
            manager.bStrikeCheck = true;

            arriveX = getCurrentZoneX(Zone.STRIKE_ZONE_WIDTH);
            arriveY = getCurrentZoneY(Zone.STRIKE_ZONE_HEIGHT);

            //탄착점 나타내줘
            zoneUI.setArriveCursorPos(true);


        }

        ///////////////////////////////////////////////////////////////
        //피칭의 AI 함수
        //인플레이시 적용된다
        ///////////////////////////////////////////////////////////////
        public bool bLastFastBall;

        //AI가 낮게 제구하는지 여부
        private bool checkAILowControlBall(PitchingArsenal ballType)
        {
            //구종여기고쳐
            PitchType type = PitchingMechanism.getBallType(ballType);
            int per = MyMath.Percent();

            if(type == PitchType.CURVE)
            {
                if (ballType == PitchingArsenal.UPSHOOT) return false;
                else
                {
                    if (per < 80) return true;
                }
            }
            else if (type == PitchType.SLIDER)
            {
                if (per < 80) return true;
            }
            else if (type == PitchType.CHANGEUP)
            {
                if (per < 80) return true;
            }
            else if (type == PitchType.FORK)
            {
                if (per < 80) return true;
            }
            else
            {
                if (per < 55) return true;
            }

            return false;
        }

        /*
        //피치아웃 여부를 체크 (현재는 스퀴즈 실패시만 피치아웃)
        public bool checkPitchedOut()
        {
            //////UnityEngine.//Debug.Log("=========================================>>>피치아웃 체크11111");
            if (batter.buntTypePre == SimulBuntType.SQUEEZE
                 && batter.buntResult == SpecificBuntType.SQUEEZ_FAIL)
            {
                //////UnityEngine.//Debug.Log("=========================================>>>피치아웃 체크222222");
                //////UnityEngine.//Debug.Log("=========================================>>>field.bSqueezePitchedOut = " + field.bSqueezePitchedOut);
                if (field.bSqueezePitchedOut == true)
                {
                    Runner squeezRunner = field.run.getDestRunner(FieldParm.HOMEBASE_INDEX);
                    if (squeezRunner != null)
                    {
                        if (squeezRunner.bStealFlag == true)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }*/

        //AI가 코스를 셀렉트 한다.
        public void aiCourseSelect()
        {
#if _COURSE_NO_AI
            courseX = 0;// Random.Range(-60, 60);
            courseY = 0;// Random.Range(-60, 60);
            
            courseX2 = courseX;
            courseY2 = courseY;
            pitchSetZone(courseX, courseY, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
#else
            if (Mode.b2outBaseLoadedMode == true)
            {
                bMissControl = false;
                int gab = manager.nineTwoRound * 7;
                courseX = Random.Range(-gab, gab);
                courseY = Random.Range(-gab, gab);

                courseX2 = courseX;
                courseY2 = courseY;
                pitchSetZone(courseX, courseY, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
            }
            else
            {
                pControl = pGuwee;
                ////Debug.Log("========================>> ai 코스 선택");     
                //스트라이크 던지는 여부
                //////UnityEngine.//Debug.Log("========================>> manager.battingResultData.result = " + manager.battingResultData.result);
                /*bPitchedOut = checkPitchedOut();
                if (bPitchedOut == true)
                {
                    courseX = batter.batterHand == CPlayer._LEFTHAND ? -150 : 150;
                    courseY = 100;
                    controlValue = ControlValue.Normal;
                    batter.autoModeBatting = AutoModeBatting.Normal; //가운데 실투시 노멀 배팅체재로 바꿈
                }
                else*/
                {
                    //실투율
                    bool bMiss = PitchingMechanism.getMiss(pGuwee, fatigueStep); //피칭밸류

#if _WILD_PITCH_TEST
                    //if (run.bOnBase[0] == true) //조건부 
                        bMiss = true;   //지워지워
#endif
                    if (Mode.bAutoPlay == true && batter.autoModeBatting != AutoModeBatting.Normal)
                    {
                        //타자 삼진 혹은 포볼 케이스가 오토모드에서 나왔을시 실투는 없는걸로 처리
                        bMiss = false;
                    }

                    bMissControl = bMiss;
                    setMiss(bMiss);
                    if (bMiss == true)
                    {
                        controlValue = ControlValue.Miss;
                        userControlValue = UserControlValue.Miss;
                        setMissControl();
                    }
                    else
                    {
                        bool bStrike;
                        int batterPower = (batter.pBatter.getContact() + batter.pBatter.getPower());
                        int pitcherPower = (pGuwee * 2);
                        /*if (Mode.bAutoPlay == true)
                        {
                            ////Debug.Log("========================>> 오토모드 스트라이크 볼 선택 // autoModeBatting = " + batter.autoModeBatting);  
                            bStrike = (batter.autoModeBatting == AutoModeBatting.BaseOnBall ? false : true);
                        }
                        else*/
                        {
                            ////Debug.Log("========================>> ai 스트라이크 볼 선택");  
                            /*bStrike = PitchingMechanism.pitchStrike(manager.nBallCount, manager.nStrikeCount, bLastFastBall,
                                                               batterPower,   //배팅 밸류
                                                               pitcherPower,  //피칭 밸류
                                                               false);*/

                            bStrike = MyMath.Percent() < 80 ? true : false;
                        }
                        controlValue = ControlValue.Normal;
                        userControlValue = PitchingMechanism.getControlValue(pControl);
                        //////UnityEngine.//Debug.Log("===============>>getGujongType(_selected) = " + getGujongType(_selected));
                        ////UnityEngine.//Debug.Log("================>>controlValue = " + controlValue);
                        //bool bLowControl = checkAILowControlBall(selectedBallIndex);
                        bool bLowControl = false;
                        //Vector2 pos = PitchingMechanism.getCourse(userControlValue, preHenkaX, preHenkaY, bStrike, bLowControl, (Mode.gameMode == Mode.GamePlayMode.Ranking ? true : false));
                        Vector2 pos = PitchingMechanism.getCourse(UserControlValue.Normal, preHenkaX, preHenkaY, bStrike, bLowControl, false);
                        ////////UnityEngine.//Debug.Log("================>>체크 2");
                        courseX = pos.x;
                        courseY = pos.y;

                    }
                }
                courseX2 = courseX;
                courseY2 = courseY;
                pitchSetZone(courseX, courseY, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
            }
#endif
        }

        /// <summary>
        /// 현재 투수가 가지고 있는 공중에 해당 타입의 구종의 값을 리턴해줌
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int getTypeValue(PitchType type)
        {
            for (int i = 0; i < 5; i++)
            {
                PitchingArsenal cur = pPitcher.getBallType()[i];
                if (type == PitchingMechanism.getBallType(cur))
                {
                    return pPitcher.getBallValue2(i);
                }
            }

            return -1;
        }

        //AI가 구종을 선택한다
        public void aiBallSelect()
        {

#if _AI_ONLYFASTBALL
            setCurBallSpeed(PitchingArsenal.FASTBALL);
            setBallAndGuwee(PitchingArsenal.FASTBALL);
#else
            PitchingArsenal curSelect;// = PitchingMechanism.getSelectedBall(pPitcher, manager.nBallCount, manager.nStrikeCount, bLastFastBall);
            
            if (Mode.b2outBaseLoadedMode == true)
            {
                curSelect = PitchingMechanism.getSelectedBallNineTwoMode(pPitcher, manager.nineTwoRound);
            }
            else
            {
                curSelect = PitchingMechanism.getSelectedBall(pPitcher, manager.nBallCount, manager.nStrikeCount, bLastFastBall);                
            }

            
            fastballCon = 0;//오토플레이에서는 필요없을듯
            //AI 직구 스피드 산출
            //구속 산출
            setCurBallSpeed(curSelect);
            //구위 산출
            setBallAndGuwee(curSelect);
            ////////UnityEngine.//Debug.Log("==============>> gujong = " + getGujongType(select));
            selectedBallIndex = curSelect;
            bLastFastBall = (PitchingMechanism.getBallType(curSelect) == PitchType.FASTBALL ? true : false);


            
#endif

        }
        

        ///////////////////////////////////////////////////////////
        //투구의 메인 프레임
        ///////////////////////////////////////////////////////////
        //투구 메인 프레임
        public void pitchingFrame()
        {
            curTime += Time.deltaTime;

            switch (pState)
            {
                case PitcherState._GET_SIGN:
                    getSignFrame();
                    break;
                case PitcherState._PITCHING:
                    if (curTime > 5)
                    {
                        if (bPitchStart == false)
                        {
                            //Debug.Log("=====================>>>>피칭 에러 뜬경우 강제 처리");
                            StartCoroutine(startPichingAnim3());
                            curTime = 0;
                        }
                    }
                    break;
                case PitcherState._RELEASE:
                    releaseFrame();
                    break;
                case PitcherState._FINISH:
                    finishFrame();
                    break;
                case PitcherState._CHECKCOUNT:
                    checkCountFrame();
                    break;
                case PitcherState._TIPCOUNT:
                    tipCountFrame();
                    break;
                case PitcherState._CHECKSWING_ASK:
                    checkSwingFrame();
                    break;
                //case PitcherState._WILD_PITCH:
                //    wildPitchFrame();
                //    break;
            }
        }

        ///////////////////////////////////////////////////////////////////
        //공이 타격되거나 미트에 닿기 전까지의 과정
        ///////////////////////////////////////////////////////////////////
        private float waitTime;
        //투수 포수간 사인을 주고받는 이벤트
        //AI는 여기서 코스및 구종을 선택, 플레이어는 조작에 따라 마찬가지로 코스와 구종을 선택
        private void getSignFrame()
        {
            pvp_wait_time = 0;
            if (bPauseQue == true)
            {
                if (IngameUI.GetPauseUI().SetPause(manager) == true)
                {
                    Mode.bPauseGame = true;
                    ControlBattingUI.CheckPauseState(true);
                    //Util.Load("MainGame/prefabs/gameUI/QuitPopupPrefab", IngameUI.GetInstance().transform, Vector3.zero).GetComponent<UIQuit>().init(manager);                
                }
                bPauseQue = false;
            }

            if (positionInit == false)
            {
                //if (Mode.bAutoPlay == true) initAnim();

                waitTime = 0.167f;// waitFrame = 10;//

                if (bPitchCeremony == true)
                {
                    waitTime = 2.0f;
                    bPitchCeremony = false;
                }


                //Debug.Log("셋포지션 상태 세팅1");
                if (Mode.bPvpMode433 == true)
                {
                    bSetPosition = manager.bMyTurn ? true : false;
                }
                else
                {
                    if (run.bOnBase[FieldParm.FIRSTBASE_INDEX] || run.bOnBase[FieldParm.SECONDBASE_INDEX] || run.bOnBase[FieldParm.THIRDBASE_INDEX])
                    {
                        bSetPosition = true;
                    }
                    else bSetPosition = false;
                }



                bvfielder[0].setReady();
                bvfielder[1].setReady();
                setReadyAnim();

                positionInit = true;

                if (Mode.bPvpMode433 == true)
                {
                    
                }
                else
                {
                    run.setAiStealControl();
                }

            }
            


            //타자가 진행 불가
            //if (batter.state != BatterState._WAITING && batter.state != BatterState._BUNT && batter.state != BatterState._LOOKING) return;
            //if ( (manager.bMyTurn && Mode.batControlType == BatControlType.PushType)
            /*
            if(Mode.bAutoPlay == true)
            {
                if (bGetSign == false)
                {
                    if (curTime > waitTime)//
                    {   
                        aiBallSelect();
                        aiCourseSelect();
                        getSign();//0.5f);
                        bGetSign = true;
                    }
                }
            }*/
        }

        //릴리즈 프레임
        private void releaseFrame()
        {
            if (batter.bTipped == false)
            {
                if (checkPitchFinish() == true)//
                {
                    ////////UnityEngine.//Debug.Log("==================>>투구종료");
                    if (bCheckSwingAsk == true)
                    {
                        curCheckTime = 0;
                        checkStep = 0;
                        pState = PitcherState._CHECKSWING_ASK;
                        bCheckSwingAsk = false;
                    }
                    else
                    {
                        batter.bPreHitCheck = false;
                        setCount();
                        if (batter.bSwing == false)
                        {
                            //볼 들어간 뒤는 강제 스윙 못하게함
                            batter.bForcedSwingPrevent = true;
                        }
                        pState = PitcherState._FINISH;
                    }

                    if (batter.bHitted == false)
                    {
                        
                        if (hitByPitchStep == 0)
                        {
                            //catcher.setNormalBallCatch();
                            if (bPvState == false)
                            {
                                //타자뷰에서만 먼지 가동
                                StartCoroutine(zoneUI.setMunji(preArriveX, preArriveY));
                            }

                            //미트 사운드
                            soundmanager.Get().PlaySound(soundmanager.SoundID.SoundCatch);
                        }
                        ControlBattingUI.SetActive(false, manager); //manager.gameUI.battingUI.GetComponent<battingUI>()._active.SetActive(false); //[UI]배티UI 컴포넌트를 디액티브
                    }
                }
                else
                {
                    cursorFrame();
                }
            }
            else
            {
                if (bTipHappen == false)
                {
                    //팁처리
                    tipCount();
                    bTipHappen = true;
                }
            }
        }

        float pvp_wait_time;

        //투구 완료 프레임
        private void finishFrame()
        {
            if (curTime > 0.2f)//if (pFrame == _MaxFrame)
            {
                //if (curTime > 0.2f)//if (pFrame == _MaxFrame)
                //{
                if (Mode.bPvpMode433 == true)
                {
                    if (manager.bMyTurn == true)
                    {
                        if (batter.bCheckSwinged == false)
                        {
                            if (batter.bSwing == true)
                            {
                                //Debug.Log("일반 헛스윙 정보 송신");
                                pvpmanager.Get().SendNoHitInfo(manager, NoHitStatus.HutSwing);
                            }
                            else
                            {
                                //노스윙 정보 보내기
                                //Debug.Log("노스윙 정보 보내기");
                                pvpmanager.Get().SendNoHitInfo(manager, NoHitStatus.NoSwing);
                            }
                        }
                    }
                    else
                    {                        
                        if (manager.bPVPHitInfoCheck == false)
                        {
                            //Debug.Log("=======================>>스윙정보를 수신 못한 경우");
                            pvp_wait_time += 0.1f;                            
                            if(pvp_wait_time >=10)
                            {
                                pState = PitcherState._CHECKCOUNT;
                                //PhotonManager.Get().Disconnect();
                            }
                            else
                            {
                                curTime = 0.1f;
                                Debug_UI.SetNetwork(true);
                            }
                            return;
                        }
                        else
                        {
                            if (manager.nohitType == NoHitStatus.NoSwing ||
                                manager.nohitType == NoHitStatus.CheckSwing)
                            {
                                //노스윙의 상태 체크
                                //Debug.Log("=======================>>수신된 노스윙 정보 세팅");
                                bWildPitch = manager.Pvp_bWildPitch;
                                manager.bStrikeCheck = manager.Pvp_bStrikeCheck;
                            }
                        }
                    }
                }


                if (batter.bHitted == false)
                {
                    pState = PitcherState._CHECKCOUNT;
                    checkCount();
                }
                initStateStep();
                Debug_UI.SetNetwork(false);
                //}

            }
        }
               


        //배팅 커서 프레임
        private void cursorFrame()
        {
            //릴리즈배팅 타입인경우 작용안한다.
            if (Mode.bAutoPlay == false && manager.bMyTurn == true)
            {
                //수동 타겟팅
                if (Mode.batControlType == BatControlType.ReleaseType && batter.bBunt == false)
                {
                    //if (Mode.bPowerfulType == true)
                    {
                        float px = getCurrentZoneX(Zone.STRIKE_ZONE_WIDTH);
                        float py = getCurrentZoneY(Zone.STRIKE_ZONE_HEIGHT);
                        zoneUI.setTimerPos(px, py);
                    }
                }
            }
            else
            {
                //오토 타겟팅
                if (pState == PitcherState._RELEASE && batter.bSwing == false)
                {
                    bool bMove = false;

                    float px, py;
                    if (batter.bArrivePointGuess == true)
                    {
                        px = (preArriveX + aiMissOffsetX);
                        py = (preArriveY + aiMissOffsetY);
                    }
                    else
                    {
                        px = (getCurrentZoneX(Zone.STRIKE_ZONE_WIDTH) + aiMissOffsetX);   //투수 능력치 오프셋 오차 필요
                        py = (getCurrentZoneY(Zone.STRIKE_ZONE_HEIGHT) + aiMissOffsetY);  //투수 능력치 오프셋 오차 필요
                    }

                    if (bPvState == true) px = -px;

                                        
                    if (px != batter.cursorX)
                    {
                        if (px > batter.cursorX) batter.cursorDX = 150;//cursorSpeed;
                        else batter.cursorDX = -150;//cursorSpeed;
                        bMove = true;
                    }

                    if (py != batter.cursorY)
                    {
                        if (py > batter.cursorY) batter.cursorDY = 150;//cursorSpeed;
                        else batter.cursorDY = -150;//cursorSpeed;
                        bMove = true;
                    }

                    if (bMove == true)
                    {
                        batter.cursorX += batter.cursorDX * Time.deltaTime;
                        if (batter.cursorX > Zone.UI_ZONE_WIDTH - 10) batter.cursorX = Zone.UI_ZONE_WIDTH - 10;
                        else if (batter.cursorX < -(Zone.UI_ZONE_WIDTH - 10)) batter.cursorX = -(Zone.UI_ZONE_WIDTH + 10);

                        batter.cursorY += batter.cursorDY * Time.deltaTime;
                        if (batter.cursorY > Zone.UI_ZONE_HEIGHT - 10) batter.cursorY = Zone.UI_ZONE_HEIGHT - 10;
                        else if (batter.cursorY < -(Zone.UI_ZONE_HEIGHT - 10)) batter.cursorY = -(Zone.UI_ZONE_HEIGHT + 10);

                        zoneUI.setBatCursorPos();
                    }
                }
            }

        }
       
        ///////////////////////////////////////////////////////////
        //공이 배트에 닿지 않은 경우 관련 메쏘드
        ///////////////////////////////////////////////////////////
        //스트라이크 볼을 체크
        private void checkCount()
        {
            bPitchCeremony = false;
            manager.bStrike
                = manager.bBall
                = manager.bStrikeOut
                = manager.bStealStrikeOut
                = manager.bBaseOnBalls
                = manager.bThreeOutChange = false;

            /*
            if (Mode.bPvpMode433 == true)
            {
                if (manager.bMyTurn == true)
                {
                    if (batter.bCheckSwinged == false)
                    {
                        if (batter.bSwing == true)
                        {
                            Debug.Log("일반 헛스윙 정보 송신");
                            pvpmanager.Get().SendNoHitInfo(manager, NoHitStatus.HutSwing);
                        }
                        else
                        {
                            //노스윙 정보 보내기
                            Debug.Log("노스윙 정보 보내기");
                            pvpmanager.Get().SendNoHitInfo(manager, NoHitStatus.NoSwing);
                        }
                    }
                }
            }*/


            if (bWildPitch == true)
            {                
                StartCoroutine(wildPitchDelay());
            }
            else
            {
                if (manager.bStrikeCheck == true)
                {
                    ////////UnityEngine.//Debug.Log("==============>> call Strke");
                    manager.bStrike = true;
                    manager.nStrikeCount++;
                                        
                    if (manager.nStrikeCount > 2)
                    {
                        BackGroundManager.SetDisplayEffect("STRIKE OUT");

                        //스트라이크아웃콜 사운드
                        soundmanager.Get().PlaySound(soundmanager.SoundID.StrikeOutCall);
#if _Skill_Display
                        //연출테스트용
                        if (manager.pitcherSkill_Display_test == pSkillDisplay.Doctor_K)
                        {
                            manager.instantSkillEffect(pPitcher, SkillIndex.DoctorK, true);
                        }

#else
                        //닥터 K 중첩효과
                        if (pPitcher.setPiledupSkill(SkillIndex.DoctorK, 3, true) == true)
                        {
                            manager.instantSkillEffect(pPitcher, SkillIndex.DoctorK, true);
                        }
#endif

                        //nextBatter를 호출 한다
                        manager.bStrike = false;
                        manager.bStrikeOut = true;
                        manager.bStealStrikeOut = true;
                                                
                        StartCoroutine(pitcherCeremony());

                        IngameUI.GetBattingCall().Call(CALLTYPE.CALL_STRIKEOUT, (int)selectedBallIndex, getBallSpeed(), batter);
                        if (bPvState == true)
                        {
                            pvjudge.setStrikeOut(batter.batterHand==CPlayer._LEFTHAND?true:false);
                        }

                        manager.fieldOutCountNum = 0;
                        manager.setOutFlag(BallPlayManager._K_FLAG | BallPlayManager._SO_FLAG);
                        manager.addOutCount();
                        setPinchScoreReduce(1); //삼진 추가 핀치스코어 -1

                        /*
                        if (manager.nOutCount > 2)
                        {
                            //예외처리
                            SimulManager.AddGameSummuryInfo("\n[ffde00]" + batter.pBatter.getName() + ": 삼진 아웃[-]");
                        }*/

                     
                        initStateStep();
                    }
                    else
                    {                        
                        //스트라이크콜 사운드
                        IngameUI.GetBattingCall().Call(CALLTYPE.CALL_STRIKE, (int)selectedBallIndex, getBallSpeed(), batter);
                        if (bPvState == true)
                        {
                            pvjudge.setStrike();
                        }
                        soundmanager.Get().PlaySound(soundmanager.SoundID.StrikeCall);
                    }
                }
                else
                {
                    if (hitByPitchStep != 0)
                    {
                        //힛바이 피치 처리
                        manager.setFourballCount(true);
                        IngameUI.GetBattingCall().Call(CALLTYPE.CALL_DEADBALL, (int)selectedBallIndex, getBallSpeed(), null);
                    }
                    else
                    {
                        //볼 처리
                        manager.bBall = true;
                        manager.nBallCount++;
                        if (manager.nBallCount > 3)
                        {
#if _NO_FIELD
                            manager.resetCount();
                   
#else
                            //포볼                            
                            manager.setFourballCount();                            
                            
                            //포볼 사운드

                            IngameUI.GetBattingCall().Call(CALLTYPE.CALL_FOURBALL, (int)selectedBallIndex, getBallSpeed(), null);
#endif
                        }
                        else
                        {                            
                            //볼 콜 사운드
                            IngameUI.GetBattingCall().Call(CALLTYPE.CALL_BALL, (int)selectedBallIndex, getBallSpeed(), null);
                        }
                        soundmanager.Get().PlaySound(soundmanager.SoundID.BallCall);
                        //batter.tryToHit();
                    }

                    if (bPvState == true)
                    {
                        pvjudge.setReadyBack();
                    }
                                        
                    StartCoroutine(pitcherAngry());  

                }
            }
            IngameUI.GetScoreBoard().BoardUpdate();

            
        }




        private bool bPitchCeremony;
        private IEnumerator pitcherCeremony()
        {
            IngameUI.GetPlayerInfo().SetActive(false, true);
            yield return new WaitForSeconds(0.5f);

            zoneUI.setZone(false, false, false);
            zoneUI.setBatCursorActive(false);

            if (bPvState == true)
            {
                pitcherAnim("PITCHER_SMILE_0" + Random.Range(1, 6), false);
                //bPitchCeremony = true;
                yield return new WaitForSeconds(2.0f);
                catcher.setIdle();
            }
            else
            {   
                if (manager.nOutCount > 2)
                {
                    CameraManager.SetZoomTo(2, 0.3f);
                    yield return new WaitForSeconds(0.3f);
                    batter.anim.GetComponent<Renderer>().enabled = false;
                }
                pitcherAnim("PITCHER_SMILE_0" + Random.Range(1, 6), false);
                //bPitchCeremony = true;
                yield return new WaitForSeconds(3.4f);
                batter.anim.GetComponent<Renderer>().enabled = true;
                CameraManager.SetZoomTo(1.0f, 0.1f);
            }
        }

        private IEnumerator pitcherAngry()
        {
            bReadyPossible = false;
            if (manager.bBaseOnBalls == true || hitByPitchStep != 0)
            {
                zoneUI.setZone(false, false, false);
                zoneUI.setBatCursorActive(false);
                pitcherAnim("PITCHER_INCONVENENCE_0" + Random.Range(1, 4), false);
                //bPitchCeremony = true;
                yield return new WaitForSeconds(2.5f);
                bReadyPossible = true;
            }
            else
            {
                if (pinchState != PinchStep.Pinch && fatigueStep == FatigueStep.STAMINA_NORMAL)
                {
                    if (MyMath.Percent() < ((manager.nBallCount + 1) * 10)) 
                    {
                        yield return new WaitForSeconds(0.3f);
                        pitcherAnim("PITCHER_ANGRY_0" + Random.Range(1, 3), false);
                        bPitchCeremony = true;
                        yield return new WaitForSeconds(2.0f);
                        bReadyPossible = true;
                        setReadyAnim();
                    }
                    else
                    {
                        bReadyPossible = true;
                    }
                }
            }


        }


        //와일드 피치 프레임
        private IEnumerator wildPitchDelay()
        {
            FieldParm.WildPitchCase wildCase = FieldParm.WildPitchCase.NoRunner;
            
            CameraManager.CameraShake(0.2f, 10);
            
            pState = PitcherState._WILD_PITCH;
            
            if (run.bOnBase[0] || run.bOnBase[1] || run.bOnBase[2])
            {
                wildCase = FieldParm.WildPitchCase.RunnerOnBase;
            }

            if (manager.bStrikeCheck == true)
            {
                manager.bStrike = true;
                manager.nStrikeCount++;
                if (manager.nStrikeCount > 2)
                {
                    //낫아웃 상태
                    //스트라이크 아웃 콜 사운드

                    IngameUI.GetBattingCall().Call(CALLTYPE.CALL_STRIKEOUT, (int)selectedBallIndex, getBallSpeed(), batter);
                    wildCase = FieldParm.WildPitchCase.NotOut;

                    soundmanager.Get().PlaySound(soundmanager.SoundID.StrikeOutCall);
                }
            }
            else
            {
                manager.bBall = true;
                manager.nBallCount++;
                if (manager.nBallCount > 3)
                {
                    //포볼                    
                    //볼 콜 사운드
                    IngameUI.GetBattingCall().Call(CALLTYPE.CALL_FOURBALL, (int)selectedBallIndex, getBallSpeed(), null);
                    wildCase = FieldParm.WildPitchCase.BaseOnBall;

                    soundmanager.Get().PlaySound(soundmanager.SoundID.BallCall);
                }
            }
            //0.5초의 딜레이
            yield return new WaitForSeconds(0.5f);

            field.setWildPitchState(wildCase);
            pState = PitcherState._NONE;

        }

        //체크 상태 프레임 함수
        private void checkCountFrame()
        {            
            float limitTime = 1.333f;
            if (run.bStealBase == true)
            {
                limitTime = 0;
            }
            else
            {
                if (manager.bStrikeOut || manager.bThreeOutChange)
                {
                    limitTime = (manager.bThreeOutChange ? 4.5f : 2.0f);// 0.667f;
                }
                else if (manager.bBaseOnBalls == true)
                {
                    limitTime = 3;
                }
            }

            if (stateStep == 0)
            {
                if (manager.bBall)
                {

                }

                /*
                if (manager.bStrikeOut || manager.bThreeOutChange)
                {
                    if (batter.bSwing == false)
                    {
                        StartCoroutine(batter.strikeOutLooking());
                    }
                }
                else if (manager.bStrike)
                {
                    if (batter.bSwing == false && manager.nStrikeCount >= 2)
                    {
                        if (Mathf.Abs(preArriveX) > Zone.STRIKE_ZONE_WIDTH - 30 || Mathf.Abs(preArriveY) > Zone.STRIKE_ZONE_HEIGHT - 30)
                        {
                            batter.strikeLooking();
                        }
                    }
                }*/

                stateStep = 1;
            }
            else
            {
                if(curTime > limitTime)
                {
                    //bPitchFinish = true;
                    
                    if (field.run.bOnBase[FieldParm.FIRSTBASE_INDEX] == false)
                    {
                        battingview._1stRunner.set1stRunnerInit(false,0.3f);
                    }
                    if (field.run.bOnBase[FieldParm.SECONDBASE_INDEX] == false)
                    {
                        battingview._2ndRunner.set2ndRunnerInit(false,0.3f);
                    }

                    if (manager.bThreeOutChange == true)
                    {
                        if (Mode.b2outBaseLoadedMode == true)
                        {
                            pState = PitcherState._NONE;
                            manager.playState = PlayState.NONE;
                            StartCoroutine(manager.nineTwoNextRound());
                        }
                        else
                        {                            
                            pState = PitcherState._GET_STRIKEOUT;
                            initStateStep();
                            bool bChangeUI = true;
                            if (manager.checkChanceModeEnd(SimulResultState.StrikeOut) == true)
                            {
                                //시뮬모드로 돌아감
                                bChangeUI = false;
                            }
                            else
                            {
                                manager.setFieldBack();
                                manager.setChangeInning(true, bChangeUI);
                                ControlManager.ResetUI();
                            }
                        }
                    }
                    else
                    {
                        if (run.bStealBase)
                        {
                            //도루시도시 포볼이 우선
                            if (manager.bBaseOnBalls == true)
                            {
                                field.setFourBallCall();
                            }
                            else
                            {
                                field.setStealState();
                            }
                        }
                        else
                        {
                            if (manager.bStrikeOut == true)
                            {
                                manager.bChangeFlag = true;
                                pState = PitcherState._GET_STRIKEOUT;
                                batter.bState = BatterState._NEXT_BATTER;
                                initStateStep();

                                if (Mode.b2outBaseLoadedMode == true)
                                {
                                    pState = PitcherState._NONE;
                                    manager.playState = PlayState.NONE;
                                    StartCoroutine(manager.nineTwoNextRound());
                                }
                                else
                                {
                                    if (manager.checkChanceModeEnd(SimulResultState.StrikeOut) == false)
                                    {
                                        StartCoroutine(batter.nextBatterAfterStrikeOut(2.0f));// 0.8f));
                                        //StartCoroutine(batter.nextBatterAfterStrikeOut(Mode.bAutoPlay == false ? 3.5f : 0.4f));
                                    }
                                }
                            }
                            else if (manager.bBaseOnBalls == true)
                            {                                
                                field.setFourBallCall();
                            }
                            else
                            {
                                //타자 애니메이션 버그 관련
                                batter.bBatterFieldUpdate = true;
                                batter.bReadyAnim = false;

                                setPitch();

                                //if (setting.battingUI == 0 || manager.bMyTurn == false)
                                {
                                    if (Mode.bAutoPlay == false)
                                    {
                                        //자동 플레이시 해당 사항 없음
                                        ControlManager.SetReadyUI(1, false); //manager.gameUI.setReadyUI(1, false, false); //[UI]UI 상태가 타격 준비로
                                    }
                                    manager.playState = PlayState.PLAY_BATTING_VIEW_READY;
                                    manager.bReadyFinish = true;
                                    manager.readyZoom = 1;
                                    IngameUI.GetScoreBoard().TopUIActive(true);
                                    IngameUI.GetControlRunner().SetActive(true, true);

                                    /*if (Mode.gameMode == Mode.GamePlayMode.Pvp ||
                                        Mode.gameMode == Mode.GamePlayMode.Pvp433)
                                    {
                                        //강제 채팅창이 닫혀진 경우 다시 강제로 열음
                                        IngameUI.GetEmoticonChatting().forceChatEnable();
                                    }*/
                                }
                            }
                        }
                    }
                    manager.bStrike
                        = manager.bBall
                        = manager.bStrikeOut
                        = manager.bBaseOnBalls
                        = manager.bThreeOutChange = false;

                }
            }
        }

        //파울팁시 카운트 및 상태 세팅
        private void tipCount()
        {
            if (bPvState == true)
            {
                pvjudge.setFoul();
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.setTip(batter.battingOffsetY > 0 ? true : false);
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.setTip(batter.battingOffsetY > 0 ? true : false);
            }
            StartCoroutine(setTipInfo());
            bTipHappen = true;
            pState = PitcherState._TIPCOUNT;
            initStateStep();
        }

        //팁카운트 프레임 함수
        private void tipCountFrame()
        {
            float timeLimit = 1.333f;// int limit = 80;
            if(stateStep == 0)
            {
                if(curTime > 0.667f)
                {
                    manager.foulCall();
                    //주자가 달리고 있으면 스톱시킴
                    /*field.setRunnerStop(true);
                    run.setStealInvalid();  //도루는 무효화          

                    manager.bStrikeCheck = true;
                    manager.bStrike = true;
                    if (manager.nStrikeCount <= 2) manager.nStrikeCount++;
                    //manager.gameUI.showCall(JUDGE_CALL._FOUL);
                    manager.gameUI.setBattingViewCall(CALLTYPE.CALL_FOUL, getBallSpeed(), selectedBallIndex, batter.getTiming());
                    UIScoreBoard.BoardUpdate();*/
                    stateStep = 1;
                }
            }
            else
            {
                if(curTime > timeLimit)
                {
                    //타자 애니메이션 버그 관련
                    batter.bBatterFieldUpdate = true;
                    batter.bReadyAnim = false;

                    //run.bRunnerUpdateFlag = true;// 
                    run.updateRunner(); //업데이트 러너           
                    setPitch();

                    /*
                    //if (setting.battingUI == 0 || manager.bMyTurn == false)
                    {        //디폴트 배팅 UI
                        manager.gameUI.setReadyUI(1, false);
                        manager.playState = PlayState.PLAY_BATTING_VIEW_READY;
                    }*/

                    manager.bStrike
                        = manager.bBall
                        = manager.bStrikeOut
                        = manager.bBaseOnBalls
                        = manager.bThreeOutChange = false;

                    ControlManager.SetReadyUI(1); //manager.gameUI.setReadyUI(1, false); //[UI]레디 상태로 세팅
                    manager.playState = PlayState.PLAY_BATTING_VIEW_READY;
                    manager.bReadyFinish = true;
                    manager.readyZoom = 1;
                }
            }
        }

        //체크 스윙 프레임 함수
        private float curCheckTime;
        private int checkStep;
        private void checkSwingFrame()
        {
            curCheckTime += Time.deltaTime;
            if (checkStep == 0)
            {
                if (curCheckTime > 0.3f)
                {
                    //manager.gameUI.setSkillBox(true, "체크스윙"); //[UI] 체크 스윙 스킬 이름 박스 
                    checkStep = 1;
                }
            }
            else if (checkStep == 1)
            {
                if (curCheckTime > 0.7f)//5f)
                {
                    setCount();
                    pState = PitcherState._FINISH;
                }
            }
        }



        ///////////////////////////////////////////////////////////
        //각종 상태와 상황에 따른 애니메이션 설정
        ///////////////////////////////////////////////////////////

        //인자값 arg에 들어온 값에 의해 견제 혹은 투구를 결정하는 함수
        public void getSign()//float delay)
        {
            if (manager.bMyTurn == true)
            {
                IngameUI.GetControlRunner().SetActive(false, true);
                ControlBattingUI.SetPowerAndBuntUIActive(false);
            }

            run.bStealBase = false;
            run.bHomeSteal = false;
            run.bPickOff = false;

            //yield return new WaitForSeconds(delay);
            startPitch();
        }

#if _Skill_Display
        //연출테스트용
        CSkill tempPitchSkill = null;

        private CSkill setTestPitcherPitchSkill()
        {
            if (manager.pitcherSkill_Display_test == pSkillDisplay.Hoe_Sim_Il_Gyeog)
            {
                return new CSkill(10007, SkillIndex.TenderStroke, true);
            }
            else if (manager.pitcherSkill_Display_test == pSkillDisplay.Mea_Hog)
            {
                return new CSkill(10008, SkillIndex.Charm, true);
            }

            return null;
        }

        private CSkill setTestBatterPitchSkill()
        {
            if (manager.batterSkill_Display_test == bSkillDisplay.Mea_Noon)
            {
                return new CSkill(20009, SkillIndex.FalconEye, true);
            }

            return null;
        }
#endif

        //투구 시작 
        private bool bPitchStart;
        public void startPitch()
        {
            //if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut) IngameUI.GetScoreBoard().TopUIActive(false);
            if (Mode.bPauseGame == true) return;
            if (run.setAiPickoffControl() == true) return;

            /*
            if (Mode.bAutoPlay == false)
            {
                if (userControlValue == UserControlValue.Perfect && manager.bMyTurn == false)
                {
                    effectAnim(effectAnim1, 0, "cutdivine_cha", false);
                }
            }*/

            //연출
            CSkill pitcherSkill = SimulManager.GetPitcherSkill();
            if (pitcherSkill != null)
            {
                SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Pitch, true);
            }

            CSkill batterSkill = SimulManager.GetBatterSkill();
            if (batterSkill != null)
            {
                SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Pitch, false);
            }

#if _Skill_Display
            //연출테스트용            
            CSkill pitchPitcherSkill = setTestPitcherPitchSkill();
            tempPitchSkill = pitchPitcherSkill;
#else
            //투수 피치 스킬 플래그 설정
            CSkill pitchPitcherSkill = SimulManager.GetPitchPitcherSkill();
#endif
            if (pitchPitcherSkill != null)
            {
                SkillEffectDisplayManager.AddSkill(pitchPitcherSkill);
                SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Pitch, true);
                manager.pitcherSkillFlag = (SkillFlag)pitchPitcherSkill.effectIndex;

                /*//의미가 없다.. 무슨수를 써도 가리게 됨 게다가 별 효과도 없음 특수능력이기 때문에
                if (manager.bMyTurn == false)
                {
                    //회심의 일격, 매혹 효과 나타내줌
                    SkillID id = (SkillID)SimulParm.GetSkillID(pitchPitcherSkill.effectIndex);
                    IngameUI.GetPlayerInfo().SetBuffUIDirect(id, true);
                }*/

                if (Mode.bPvpMode433 == false) 
                {
                    if (manager.bMyTurn == true)
                    {
                        //내공격이 아닌 경우는 AI가 일부러 스트라이크를 던지게 만듬
                        if (bMissControl == false)
                        {
                            //강제 스트라이크 던져
                            courseX = Random.Range(-Zone.STRIKE_ZONE_WIDTH * 0.7f, Zone.STRIKE_ZONE_WIDTH * 0.7f);
                            courseY = Random.Range(-Zone.STRIKE_ZONE_HEIGHT * 0.7f, Zone.STRIKE_ZONE_HEIGHT * 0.7f);
                            courseX2 = courseX;
                            courseY2 = courseY;
                            pitchSetZone(courseX2, courseY2, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
                        }
                    }
                }
            }
#if _Skill_Display
            //연출테스트용
            CSkill pitchBatterSkill = setTestBatterPitchSkill();
#else
            //타자 피치 스킬 플래그 설정
            CSkill pitchBatterSkill = SimulManager.GetPitchBatterSkill();
#endif
            if (pitchBatterSkill != null)
            {
                manager.batterSkillFlag = (SkillFlag)pitchBatterSkill.effectIndex;
                SkillEffectDisplayManager.AddSkill(pitchBatterSkill);
                SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Pitch, false);
            }



            bEventAnim = false;
            zoneUI.setZone(false, false, false); //존 디액티브
            zoneUI.setBatCursorAlpha(0.6f);
            
            batter.setBatterFinalSetting();


            setPitchSystem();
            pState = PitcherState._PITCHING;

            

            if (bPvState == true)
            {

            }
            else
            {
                //배팅뷰 필더 세팅
                bvfielder[0].setFielding(); //2루수
                bvfielder[1].setFielding(); //유격수      

                /*if (Mode.gameMode == Mode.GamePlayMode.Pvp)
                {
                    if (manager.bMyTurn == true)
                    {
                        IngameUI.GetScoreBoard().SetPitchTimerActive(false);
                    }
                    //투수가 피칭을 하면 타자 채팅창 강제닫음                    
                    IngameUI.GetEmoticonChatting().forceChatDisable(true);    
                }
                else*/ 
                if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
                {
                    if (manager.bMyTurn == true)
                    {
                        IngameUI.GetScoreBoard().SetPitchTimerActive(false);
                    }
                    //투수가 피칭을 하면 타자 채팅창 강제닫음                    
                    IngameUI.GetEmoticonChatting().forceChatDisable(true);
                }

            }
            
            pitchingType2 = Random.Range(1, 3); //임시

            /*포즈 문제때문에 지우고 아래의 코루틴으로 대체
            if (bSetPosition)
            {
                setPitchingAnim();
            }
            else
            {
                StartCoroutine(windupPitchAnim());
            }
            curTime = 0;*/
            StartCoroutine(startPitchDelay());            
        }


        private IEnumerator startPitchDelay()
        {
            bPitchStart = true;
            while (Mode.bPauseGame == true)
            {
                yield return new WaitForEndOfFrame();
            }

            //도발꾼연출
            CSkill catcherSkill = SimulManager.GetPitchCatcherSkill();
            if (catcherSkill != null)
            {
                if (catcherSkill.effectIndex == SkillIndex.CatcherProvoke)
                {
                    if (manager.bMyTurn == true)
                    {
                        IngameUI.GetCpuSkillUI().init(catcherSkill.ID, 3);
                    }
                    else
                    {
                        IngameUI.GetMySkillUI().init(catcherSkill.ID, 3);
                    }
                    IngameUI.GetPlayerInfo().SetBuffUIDirect(SkillID.do_bal_ggun, !manager.bMyTurn);

                    yield return new WaitForSeconds(0.5f);
                    batter.batterAnim(4, bPvState ? "5000_EVENT_LOOKING6" : "5000_EVENT_LOOKING1", false);
                    yield return new WaitForSeconds(2.5f);
                    batter.readyAnim(false);
                }
            }
            //여기까지

#if GIRL_PLAY
            setPitchingAnim();
#else
            if (bSetPosition)
            {
                setPitchingAnim();
            }
            else
            {
                StartCoroutine(windupPitchAnim());
            }
#endif
            curTime = 0;

        }


        private string getPitchAnimName()
        {
#if GIRL_PLAY
            return "pitching";
#else
            if (pitchingType == CPlayer._OVERTHROW)
            {
                //오버핸드
                /*if (userControlValue == UserControlValue.Perfect && Mode.bPvpMode == false)
                {
                    //PVP모드에서는 동기화 이슈때문에 스트롱 피치 애니메이션 출력 안함
                    return ("2000_PITCHING_OVER_STRONG_0" + Random.Range(3, (bPvState == true ? 4 : 5)));
                }
                else*/
                {
                    return ("2000_PITCHING_OVER_STRONG_0" + Random.Range(1, 3));
                }
            }
            else if (pitchingType == CPlayer._SIDEARM)
            {
                //사이드암
                return ("2000_PITCHING_SIDE_0" + Random.Range(1, 3));
            }
            else
            {
                //언더핸드
                return ("2000_PITCHING_UNDER_0" + Random.Range(1, 3));
            }
#endif
        }

        //투구 애니메이션
        private void setPitchingAnim()
        {
            
            if (bPvState == true)
            {
                pAnim.state.SetAnimation(2, getPitchAnimName(), false); 
            }
            else
            {
                pitcherAnim(getPitchAnimName(), false, true);
            }

            if (batter.checkCurrentBuntTryOn() == false)
            {
                batter.lookingAnim(false);
            }
        }
        
        //와인드업 딜레이
        private IEnumerator windupPitchAnim()
        {            
            if (bPvState == true)
            {
                pAnim.state.SetAnimation(1, "SETPOSITION_01", false);  //pitcherAnim("SETPOSITION_01", false);
                yield return new WaitForSeconds(0.6f);
                pAnim.state.SetAnimation(2, getPitchAnimName(), false);  //pitcherAnim(getPitchAnimName(), false);
            }
            else
            {
                pitcherAnim("1000_WINDUP_TYPE" + (pitchingType2 + 1), false);
                yield return new WaitForSeconds(0.6f);
                pitcherAnim(getPitchAnimName(), false);
            }

            

            if (batter.checkCurrentBuntTryOn() == false)
            {
                batter.lookingAnim(false);
            }
        }

        private bool bReadyPossible;
        public int readyAnimCount = 0;
        private string setPositionStr;

        public void setSpositionString()
        {
            if (readyAnimCount == 0)
            {
                //Debug.Log("셋포지션 상태 세팅2");
                if (Mode.bPvpMode433 == true)
                {
                    bSetPosition = manager.bMyTurn ? true : false;
                }
                else
                {
                    if (run.bOnBase[FieldParm.FIRSTBASE_INDEX] || run.bOnBase[FieldParm.SECONDBASE_INDEX] || run.bOnBase[FieldParm.THIRDBASE_INDEX])
                    {
                        bSetPosition = true;
                    }
                    else bSetPosition = false;
                }

                strID = "";


#if GIRL_PLAY
                setPositionStr = "wait";
#else

                bool bHalf = MyMath.Half();
                if (bPvState == true)
                {
                    setPositionStr = (bHalf ? "WAIT_01" : "WAIT_02");
                }
                else
                {
                    if (bSetPosition)
                    {
                        setPositionStr = "0001_WAIT_SETPOSITION2";//(bHalf ? "0001_WAIT_SETPOSITION1" : "0001_WAIT_SETPOSITION2");
                    }
                    else
                    {
                        setPositionStr = "0000_WAIT_WINDUP";
                    }
                }
#endif
            }
        }
        
        //투구 준비 애니메이션 (단순히 와인드업 or 셋포지션)
        public void setReadyAnim()
        {
            if (bReadyPossible == true)
            {
                if (readyAnimCount == 0)
                {
                    if (strID != setPositionStr)
                    {
                        pitcherAnim(setPositionStr, (bPvState ? true : false));
                    }
                    readyAnimCount++;
                }
            }
        }


        public void overComePinch()
        {
            setPinchInit();
            setStaminaTotalUpdate();
            strID = "";
            bReadyPossible = true;
            readyAnimCount = 0;
            setSpositionString();
            setReadyAnim();
        }




        //투구 준비 애니메이션 (상황에 따른 지친, 혹은 멘붕 상황까지.. 
        //아니라면 이 함수내에서 setReadyAnim호출
        private void setReadyAnim2()
        {
            if (pinchState == PinchStep.Pinch)
            {
                if (bPvState == true)
                {
                    pitcherAnim("4000_EVENT_03", true, true);
                }
                else
                {
                    pitcherAnim(bSetPosition ? "4000_EVENT_06" : "4000_EVENT_03", true, true);
                }
                bReadyPossible = false;
            }
            else
            {
                if (fatigueStep == FatigueStep.STAMINA_EXUSTED)
                {
                    pitcherAnim((bSetPosition && !bPvState) ? "4000_EVENT_05" : "4000_EVENT_02", true, true);
                    bReadyPossible = false;
                }
                else if (fatigueStep == FatigueStep.STAMINA_VERY_FATIGUE || fatigueStep == FatigueStep.STAMINA_FATIGUE)
                {
                    pitcherAnim((bSetPosition && !bPvState) ? "4000_EVENT_04" : "4000_EVENT_01", true, true);
                    bReadyPossible = false;
                }
                else
                {
                    setReadyAnim();
                }
            }
        }

        //사인에 동의 안하는 에니메이션 ->투수뷰 모드에서만 가능
        public void signDisagreeAnim()
        {
            if (bReadyPossible == true)
            {
                pAnim.state.SetAnimation(2, "SIGN_01", false);
            }
        }

        //투수뷰 모드에서 투수, 포수, 주심의 애니매이션을 초기화 한다
        private void setPitchPosition()
        {
            catcher.setReady();
            pvjudge.setReady();

            if (bSetPosition == true)
            {
                //초기화
                pitcherAnim("SETPOSITION_02", false, true);
            }
            else
            {
                //블렌딩
                pAnim.state.SetAnimation(0, "WINDUP1", false);
            }
            
        }

        //투수 준비 상황 설정
        public void setPitcherReadyState()
        {
            //////UnityEngine.//Debug.Log("====================>>PITCHER SET READY ANIM");
            

            setSpositionString();

            //테스트용
            //pinchState = PinchStep.Pinch;//지워
            //fatigueStep = FatigueStep.STAMINA_EXUSTED;//지워

            if (bEventAnim == false)
            {
                //bSignDisAgree = true;                
                setReadyAnim2();   
                bEventAnim = true;
            }

            bvfielder[0].setReady();
            bvfielder[1].setReady();
            //bvfielder[2].setReady(false);
        }

        //투구 시작 애니메이션
        public void startPitchingAnim()
        {
            //key값은 arg로 물려 받은 후
            bReadyPossible = true;
            bStartPitch = true;
            if (pState == PitcherState._GET_SIGN)
            {
                if (bGetSign == false)
                {
                    getSign();//0.1f);
                    bGetSign = true;
                }
            }
        }

        //투구 시작 애니메이션(AI)
        public IEnumerator startPichingAnim2()
        {
            bReadyPossible = true;
            aiBallSelect();
            aiCourseSelect();
            getSign();//0.5f);
            bGetSign = true;

            bReadyPossible = true;
            aiBallSelect();
            aiCourseSelect();
            yield return new WaitForSeconds(0.2f);
            getSign();
            bGetSign = true;
            //yield return new WaitForSeconds(bSetPosition ? 0.2f : 0.5f);
            //pitch.pitchOrigin.setSecondPos();
        }

        //투구 시작 애니메이션(AI type에 따라)
        public IEnumerator startPichingAnim3()
        {
            Debug.Log("Start Pitch");
            if (Mode.bPauseGame == false)
            {
                bReadyPossible = true;
                aiBallSelect();
                aiCourseSelect();
                yield return new WaitForSeconds(0.5f);
                getSign();//0.5f);
                bGetSign = true;
                yield return new WaitForSeconds(bSetPosition ? 0.2f : 0.5f);
                pitch.pitchOrigin.setSecondPos(); //피칭뷰에서는 호출될일 없는 함수이므로 냅둬도 됨 -> //처리됨
            }
        }

        //릴리즈 딜레이
        private IEnumerator releaseDelay()
        {
            /////////////////////////////////////////////////////////
            //스킬연출
            /////////////////////////////////////////////////////////
            manager.effectCheck(SkillEffectDisplayManager.DisplayStep.Release);

            //릴리즈 딜레이 설정
            while (Mode.bPauseGame == true)
            {
                anim.timeScale = 0;
                yield return new WaitForEndOfFrame();
            }

            anim.timeScale = 1.0f;

            readyAnimCount = 0;
            

            if (Mode.bPvpMode433 == true && manager.bMyTurn == true)//if (Mode.bPvpMode == true && manager.bMyTurn == true)
            {
                //PVP모드시
                setReleasePosPvp();
            }
            else
            {
                //일반시
                //릴리즈시 생기는 컨트롤 미스
                setReleaseContolMiss();
            }

            effectAnim1.gameObject.SetActive(false);
            effectAnim(munjiAnim, 0, "BALLSMOG_0" + Random.Range(1, 3), false);

            
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.startPitchPv(pitchHand==CPlayer._LEFTHAND?true:false);
                preArriveX = pitchPv.pitchOriginPv.getArriveZoneX(Zone.STRIKE_ZONE_WIDTH);
                preArriveY = pitchPv.pitchOriginPv.getArriveZoneY(Zone.STRIKE_ZONE_HEIGHT);
            }
            else
            {
                //처리됨
                pitch.startPitch();
                preArriveX = pitch.pitchOrigin.getArriveZoneX(Zone.STRIKE_ZONE_WIDTH);
                preArriveY = pitch.pitchOrigin.getArriveZoneY(Zone.STRIKE_ZONE_HEIGHT);
            }
            bRelease = true;
            pState = PitcherState._RELEASE;
            ////////UnityEngine.//Debug.Log("===================>>preArriveX = " + preArriveX);
            ////////UnityEngine.//Debug.Log("===================>>preArriveY = " + preArriveY);
            setControlValue(preArriveX, preArriveY);

            batter.bGuessCorrect = false;
            batter.bStrikeCheck = false;
            batter.batterPerfectTime = getPerfectTime() * 0.9f;

            StartCoroutine(run.runnerCheckSteal(batter.batterPerfectTime * 0.5f));

            //int curCourseIndex = PitchingMechanism.getSelectedIndex(preArriveX, preArriveY);

            float absX = Mathf.Abs(preArriveX);
            float absY = Mathf.Abs(preArriveY);
            if (absX <= Zone.UI_ZONE_WIDTH && absY <= Zone.UI_ZONE_HEIGHT)
            {
                batter.bStrikeCheck = true;
                StartCoroutine(batter.tryToSwing());
            }
            else
            {
                if (absX > (bPvState == true ? 70 :100))
                {
                    StartCoroutine(batter.plateDiciplineAnim());
                }
                else
                {
                    StartCoroutine(batter.tryToSwing());
                }
            }

            float perfectTime = batter._PERFECT_TIMING * getPerfectTime() * 0.95f; // *1.05f;
            if (manager.bMyTurn == false)
            {
                //존 상태 
                zoneUI.setBatCursorActive(true);
            }
            else //if(manager.bMyTurn == true)
            {
                if (Mode.batControlType == BatControlType.ReleaseType)
                {
                    //타이머 인디케이터를 생성해준다                    
                    StartCoroutine(zoneUI.setTimingIndicator(perfectTime));
                }
            }
                        
            batter.getBatPosition();            

            manager.addPitcherRecord(Param.ST_PNP);
            manager.nCurPitcherPitchNum[manager.defenseIndex]++;


            if (bMissControl == true)
            {    
                /*
                if (bLuckyGuyOn == true)
                {
                    StartCoroutine(loadPassiveSkill("pskill_skill" + SkillParm.LUCKY_GUY, 0.1f, 0));
                }*/
                //batter.bMustCheckSwing = true; //-->이걸 집어 넣으면 스트라이크 낫아웃이 안나옴
                StartCoroutine(setIcon(false)); //살려살려
                //와일드 피치 체크
                if (batter.bStrikeCheck == false && hitByPitchStep == 0)
                {
                    if (run.stealResult == SimulStealState.NONE) //도루경우 무효화
                    {
                        //if (checkWildPitch() == true)
                        {
#if _WILD_PITCH_TEST
                            if (MyMath.Percent() > 0)// pitcherWildRate)
#else
                            if (MyMath.Percent() > pitcherWildRate)
#endif

                            {
                                bWildPitch = true;
                            }
                        }
                    }
                }
            }


            if (bPvState == true)
            {
                //포수의 포구 반응
                if (missType != 2) //폭투 타입이 아닌경우
                {
                    if (missType == 0)
                    {
                        catcher.setGloveAim(preArriveX, preArriveY); //정상투구  
                    }
                    else
                    {
                        batter.bStrikeCheck = true;
                        catcher.setGloveAim(0, 0);  //가운데 몰림
                    }
                    float catchDelay = batter.batterPerfectTime - 0.07f;
                    if (preArriveY < -Zone.UI_ZONE_HEIGHT) catchDelay = batter.batterPerfectTime - 0.2f;
                    else if (preArriveY > Zone.UI_ZONE_HEIGHT) catchDelay = batter.batterPerfectTime - 0.15f;
                    StartCoroutine(catcher.setCatchResponse(catchDelay));
                }
                else
                {
                    //폭투
                    batter.bStrikeCheck = false;
                    float catchDelay = batter.batterPerfectTime - 0.2f;
                    StartCoroutine(catcher.setCatchResponse(catchDelay));
                }
            }


            batter.bReleaseCheck = true;
            field.bReturnBattingView = false;

            batter.setPreTimingAndContact();

            //매의 눈 스킬 연출및 컨택보정
            if (manager.batterSkillFlag == SkillFlag.FalconEye)
            {
                batter.contactAddPoint = 2; //컨택보정
                Mode.bPauseGame = false;
                if (manager.bMyTurn == true)
                {
                    StartCoroutine(falconEyeEffect(perfectTime));
                }
            }

            //Debug.Log("=========================================>> 파이널 구위 = " + pFinalGuwee);

            //릴리즈 사운드
            soundmanager.Get().PlaySound(soundmanager.SoundID.Release);
#if _Test_Local
            //Debug_UI.SetPitcher(pPitcher);  //지워지워
#endif

        }

        //투수가 쳐맞을때 애니메이션
        public void pitcherHitAnim()
        {
            if (field.ballPower > 28 && field.ball.angleZ > 15)
            {
                if (field.ball.firstAngle > 7)
                {
                    pitcherAnim((nSign > 0 ? "4001_LOOK_LEFTHIGH_01" : "4001_LOOK_LIGHTHIGH_01"), false);
                }
                else if (field.ball.firstAngle < -7)
                {
                    pitcherAnim((nSign > 0 ? "4001_LOOK_LIGHTHIGH_01" : "4001_LOOK_LEFTHIGH_01"), false);
                }
                else
                {
                    pitcherAnim("4001_LOOK_MIDHIGH_01", false);
                }
            }
        }

        //투수가 홈런 쳐맞을때 애니메이션
        public void pitcherHomerunAnim()
        {
            if (bPvState == true)
            {
            }
            else
            {
                anim.gameObject.GetComponent<Renderer>().enabled = true; //투수 그림
                pitcherAnim("PITCHER_IRRITATION", false);
            }
        }

        public void pitcherStartledLookingAnim()
        {
#if GIRL_PLAY
#else
            if (bPvState == false)
            {
                if (field.ball.firstAngle > 8)
                {
                    pitcherAnim(nSign == 1 ? "4001_LOOK_LEFTHIGH_02" : "4001_LOOK_LIGHTHIGH_02", false);
                }
                else if (field.ball.firstAngle < -9)
                {
                    pitcherAnim(nSign == 1 ? "4001_LOOK_LIGHTHIGH_02" : "4001_LOOK_LEFTHIGH_02", false);
                }
                else
                {
                    pitcherAnim("4001_LOOK_MIDHIGH_02", false);
                }
            }
#endif
        }

        /////////////////////////////////////////////////////////////////////
        //상태변화
        /////////////////////////////////////////////////////////////////////
        //부상시 능력치 세팅
        public void setInjury()
        {
            //부상시 능력치 설정
        }

        //투수의 보직 타입 관련 세팅
        public void setChangeType(CPlayer player)
        {

        }


        // 스테미나 변화 세팅
        public void setStaminaChange()
        {
            //int team = manager.defenseIndex;
            if (pPitcher != null)
            {
                //////UnityEngine.//Debug.Log("==================>>STAMINA CHANGE");
                //스태미너 변동
                pPitcher.setStamina();

                setStaminaTotalUpdate();
            }
        }

        public void setStaminaTotalUpdate()
        {
            //스태미나
            curStamina = pPitcher.getCurrentStamina();
            fatigueStep = pPitcher.getFatigueStep();

            //핀치상태
            setPinchState();
            pinchState = pPitcher.getPinchState();

            //Debug_UI.SetPitcher(pPitcher);  //지워지워

            //Debug.Log("===============================>> 투수 curStamina : " + curStamina + "  =====>> pinchState = " + pinchState);
        }


        //핀치 상황 초기화
        public void setPinchInit()
        {
            if (pPitcher != null)
            {
                conHit = conHR = conRun = 0;
                pPitcher.setPinchState(PinchStep.Normal);
                pPitcher.setPinchScore(-10);
            }
        }


        //핀치상태 세팅
        public void setPinchState()
        {
            /*int pinchScore = pPitcher.getPinchScore();            
            if (pinchScore > 0)
            {
                pPitcher.setPinchState(PinchStep.Pinch);
            }
            else
            {
                if (conHit >= 3 || conHR >= 2 || conRun >= 4) 
                {
                    conHit = conHR = conRun = 0;
                    pPitcher.setPinchScore(PitchingMechanism.PINCH_SCORE);
                    pPitcher.setPinchState(PinchStep.Pinch);
                }
                else
                {             
                    pPitcher.setPinchState(PinchStep.Normal);
                }
            }*/
            //핀치스테이트 삭제
            pPitcher.setPinchState(PinchStep.Normal);
        }

        //핀치 스코어 가감
        public void setPinchScoreReduce(int parm)
        {
            if (Mode.b2outBaseLoadedMode == false)
            {
                pPitcher.setPinchScoreReduce(parm);
            }
        }

        
        //투수의 기록을 세팅
        public void addRecord(int type, bool bCurrent, int num = 1)
        {
            if (bCurrent == true)
            {
                if (pPitcher != null)
                {
                    ////UnityEngine.Debug.Log("투수 기록===============>>> " + pPitcher.getName() + "의 " + Param.debug_stat[type] + " 가산");
                    pPitcher.setRecord(type, num);
                }
            }
            else
            {
                if (pLastPitcher != null)
                {
                    ////UnityEngine.Debug.Log("승계주자 관련 기록===============>>> " + pLastPitcher.getName() + "의 " + Param.debug_stat[type] + " 가산");
                    pLastPitcher.setRecord(type, num);
                }
            }

        }

        /////////////////////////////////////////////////////////////////////
        //투수 교체 
        /////////////////////////////////////////////////////////////////////
        //투수교체후 각종 파라메터를 세팅
        public void setPitcherChange(CPlayer player, int pIndex, int team, bool bStart)
        {
            manager.nCurPitcherPitchNum[team] = 0;
            ////////UnityEngine.//Debug.Log("====================>>setPitcherChange 이게먼저");
            //선발포함 투수가 세팅할때 단 한번 불려지는 함수이다

            //매우매우 중요
            //승계주자 관련 실점 부분 살펴봐야함

            //컨디션
            //int conAdd = 0;// (lineup.pPlayer[team, index].m_nCondition - 2) * 12;


            //투수 인덱스 설정해줌
            SimulPlayerManager.SetCurrentPitcherIndex(team, pIndex, false); //manager.pitcherIndex[team] = pIndex;

            //나간놈 플래그
            //manager.pitcherOut[team, pIndex] = true;
            SimulPlayerManager.SetPitcherOut(team, pIndex, true);

                        
            //시작이닝과 끝이닝        
            startInning[team] = manager.nInningCount * 10 + manager.nOutCount;
            //totalInning[team] = 0;

            //현재투수 실점 초기화
            allowRun[team] = 0;   //실점허용

            //핀치 초기화
            setPinchInit();

        }

        //게임 종료후 투수의 업적 세팅 -> 교체랑 상관없지만 상당히 유사한점이 있어 이 카테고리로
        public void setPitcherResult(CPlayer player, int team)
        {
            //최후 결과에 부르는것
            if (SimulPlayerManager.GetPitcherIndex(team) == SimulPlayerManager.GetStarterIndex(team))
            {
                //UnityEngine.Debug.Log("((투수업적))==============>> 선발투수가 끝까지 던진경우 team: " + team + "   투수이름: " + player.getName());
                //완투 처리
                //UnityEngine.Debug.Log("((투수업적))==============>> 완투처리   투수이름: " + player.getName());
                player.setPitcherAchieve(Param.ST_CG, Param.P_ACHIEVE_COMPLETE);
                //완봉  처리
                if (manager.nGameScore[1 - team] == 0 && manager.nGameScore[team] > 0)
                {
                    //UnityEngine.Debug.Log("((투수업적))==============>> 완봉처리   투수이름: " + player.getName());
                    player.setPitcherAchieve(Param.ST_SHO, Param.P_ACHIEVE_COMPLETE);
                }

                //승 패 처리
                if (manager.nGameScore[team] > manager.nGameScore[1 - team])
                {
                    //UnityEngine.Debug.Log("((투수업적))==============>> 승리처리   투수이름: " + player.getName());
                    player.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_COMPLETE);
                }
                else if (manager.nGameScore[team] < manager.nGameScore[1 - team])
                {
                    //UnityEngine.Debug.Log("((투수업적))==============>> 패배처리   투수이름: " + player.getName());
                    player.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_COMPLETE);
                }
            }
            else
            {
                if (manager.nGameScore[team] > manager.nGameScore[1 - team])
                {
                    //UnityEngine.Debug.Log("((투수업적))==============>> team " + team + "이 승리한 경우");
                    if (manager.winPitcherIndex[team] == -1)
                    {
                        //마지막 투수가 승리투수
                        //UnityEngine.Debug.Log("((투수업적))==============>> 마지막에 던진 투수가 승리하는 경우 // 투수이름: " + player.getName());
                        player.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_COMPLETE);
                    }
                    else
                    {
                        //승리투수는 
                        //manager.winPitcherIndex 이놈
                        //UnityEngine.Debug.Log("((투수업적))==============>> 승리투수 // 투수이름: " + SimulPlayerManager.GetPitcher(team, manager.winPitcherIndex[team]).getName());
                        SimulPlayerManager.GetPitcher(team, manager.winPitcherIndex[team]).setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_COMPLETE);

                        //세이브 투수는
                        if (player.getStat(Param.ST_SV) == Param.P_ACHIEVE_TRY)
                        {
                            //UnityEngine.Debug.Log("((투수업적))==============>> 세이브 조건 만족시 세이브 투수 // 투수이름: " + player.getName());
                            player.setPitcherAchieve(Param.ST_SV, Param.P_ACHIEVE_COMPLETE);
                        }
                    }
                }
                else if (manager.nGameScore[team] < manager.nGameScore[1 - team])
                {
                    if (manager.losePitcherIndex[team] == -1)
                    {
                        //마지막 투수가 패전투수
                        //UnityEngine.Debug.Log("((투수업적))==============>> 마지막에 던진 투수가 패배 // 투수이름: " + player.getName());
                        player.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_COMPLETE);

                        //블론 투수는
                        if (player.getStat(Param.ST_SV) == Param.P_ACHIEVE_TRY)
                        {
                            //UnityEngine.Debug.Log("((투수업적))==============>> 블론 조건 만족시 블론투수 // 투수이름: " + player.getName());
                            player.setPitcherAchieve(Param.ST_BS, Param.P_ACHIEVE_COMPLETE);
                        }
                    }
                    else
                    {
                        //manager.losePitcherIndex 이놈
                        //UnityEngine.Debug.Log("((투수업적))==============>> 패배투수 // 투수이름: " + SimulPlayerManager.GetPitcher(team, manager.losePitcherIndex[team]).getName());
                        SimulPlayerManager.GetPitcher(team, manager.losePitcherIndex[team]).setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_COMPLETE);
                    }
                }

            }
        }
       
        //수동교체
        public void setManualPitcherChange(int inPitcherIndex,int outPitcherIndex)
        {
            inPitcher = inPitcherIndex;
            outPitcher = outPitcherIndex;
            setChangeFlagInit(manager.defenseIndex);            
            if (checkSavePitcher(manager.defenseIndex) == true)
            {
                if (manager.nInningCount >= Mode.finalInning)
                {
                    pitcherChangeType = ChangeType.SAVE;
                    bSaveOn[manager.defenseIndex] = true;
                }
                else
                {
                    pitcherChangeType = ChangeType.MUST_WIN;
                    bSetupOn[manager.defenseIndex] = true;
                }
            }
            else
            {
                if (manager.nInningCount < 6)
                {
                    pitcherChangeType = ChangeType.LONGRIELEF;
                    bLongReliefOn[manager.defenseIndex] = true;
                }
                else
                {
                    int gab = manager.getScoreGab(manager.defenseIndex);
                    if (gab>=0 && gab<5)
                    {
                        pitcherChangeType = ChangeType.MUST_WIN;
                        bSetupOn[manager.defenseIndex] = true;
                    }
                    else
                    {
                        pitcherChangeType = ChangeType.CHASE;
                        bChaseOn[manager.defenseIndex] = true;
                    }
                }
            }

            pLastPitcher = pPitcher;
            int curIndex = SimulPlayerManager.GetPitcherIndex(manager.defenseIndex);

            if (pLastPitcher.getStat(Param.ST_SV) == Param.P_ACHIEVE_TRY)
            {
                //홀드 조건
                //UnityEngine.Debug.Log("[투수교체]==============>> 이전 투수가 홀드/세이브 상황에서 등판 // 투수이름: " + pLastPitcher.getName());
                if (manager.nGameScore[manager.defenseIndex] > manager.nGameScore[1 - manager.defenseIndex])
                {
                    //UnityEngine.Debug.Log("((투수업적))==============>> 홀드조건 만족하고 강판 // 투수이름: " + pLastPitcher.getName());
                    pLastPitcher.setPitcherAchieve(Param.ST_HLD, Param.P_ACHIEVE_COMPLETE);
                }
                else
                {
                    //UnityEngine.Debug.Log("((투수업적))==============>> 블론을 저지르고 강판 // 투수이름: " + pLastPitcher.getName());
                    pLastPitcher.setPitcherAchieve(Param.ST_BS, Param.P_ACHIEVE_COMPLETE);
                }
            }
            else
            {
                //승패조건
                //UnityEngine.Debug.Log("[투수교체]==============>> 강판시 승패 조건을 체크");
                if (checkWinPitcher(manager.defenseIndex, curIndex) == true)
                {
                    //UnityEngine.Debug.Log("[투수교체]==============>> 승리투수 요건 만족하고 강판 // 투수이름: " + pLastPitcher.getName());
                    pLastPitcher.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_TRY);
                }
                else if (checkLosePitcher(manager.defenseIndex, curIndex) == true)
                {
                    //UnityEngine.Debug.Log("[투수교체]==============>> 패배상황에서 강판 // 투수이름: " + pLastPitcher.getName());
                    pLastPitcher.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_TRY);
                }
                else
                {
                    //UnityEngine.Debug.Log("[투수교체]==============>> 승패 상관 없이 강판 // 투수이름: " + pLastPitcher.getName());
                    pLastPitcher.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_NONE);
                    pLastPitcher.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_NONE);
                }
            }

            //Debug.Log("===============>> 투수교체 액션상태에서 실행");

            run.setLastPitcher();
            SimulPlayerManager.SetCurrentPitcherIndex(manager.defenseIndex, inPitcherIndex);
            setPitcherChange(SimulPlayerManager.GetPitcher(manager.defenseIndex), inPitcherIndex, manager.defenseIndex, false);
            initPitcher(SimulPlayerManager.GetPitcher(manager.defenseIndex), manager.defenseIndex);

            //세이브 혹은 홀드
            //UnityEngine.Debug.Log("[투수교체]==============>> 등판시 홀드/세이브 조건을 체크");
            if (checkSavePitcher(manager.defenseIndex) == true)
            {
                //UnityEngine.Debug.Log("[투수교체]==============>> 세이브 조건에서 출격 // 투수이름: " + pPitcher.getName());
                pPitcher.setPitcherAchieve(Param.ST_SV, Param.P_ACHIEVE_TRY);
            }

            //pPitcher.changedOrder = (pLastPitcher.changedOrder + 1);
                        
        }
        //inPitcher


        //투수교체 플래그의 초기화
        public void setChangeFlagInit(int team)
        {
            bLongReliefOn[team] = false; //롱리리프 상황
            bChaseOn[team] = false;      //추격조 상황
            bSetupOn[team] = false;      //셋업 상황
            bSaveOn[team] = false;       //세이브 상황
        }

        //checkChangePitcher를 호출하여 투수가 교체 되었는지 여부를 리턴해줌
        public bool checkPitcherChanged()
        {
#if _Local_Balance
            //로컬밸런스에 의한
            if (InGameDebug._NO_CHANGE_PLAYER == true)
            {
                return false;
            }
#endif

            if (pitcherChanging(manager.defenseIndex, manager.offenseIndex))
            {
                ////UnityEngine.Debug.Log("##############################투수교체 pitcherChangeType = "+pitcherChangeType);
                if (getChangePitcher(manager.defenseIndex, pitcherChangeType) != -1)   //교체할 투수의 인덱스를 얻어옴
                {
                    setChangeFlagInit(manager.defenseIndex);
                    if (pitcherChangeType == ChangeType.CHASE)
                    {
                        //추격조 출전
                        bChaseOn[manager.defenseIndex] = true;
                    }
                    else if (pitcherChangeType == ChangeType.MUST_WIN)
                    {
                        //필승조 출전
                        bSetupOn[manager.defenseIndex] = true;
                    }
                    else if (pitcherChangeType == ChangeType.SAVE)
                    {
                        //마무리 출전
                        bSaveOn[manager.defenseIndex] = true;
                    }
                    else if (pitcherChangeType == ChangeType.LONGRIELEF)
                    {
                        //롱릴리프
                        bLongReliefOn[manager.defenseIndex] = true;
                    }

                    pLastPitcher = pPitcher;

                    int curIndex = SimulPlayerManager.GetPitcherIndex(manager.defenseIndex);


                    if (pLastPitcher.getStat(Param.ST_SV) == Param.P_ACHIEVE_TRY)
                    {
                        //홀드 조건
                        //UnityEngine.Debug.Log("[투수교체]==============>> 이전 투수가 홀드/세이브 상황에서 등판 // 투수이름: " + pLastPitcher.getName());
                        if (manager.nGameScore[manager.defenseIndex] > manager.nGameScore[1 - manager.defenseIndex])
                        {
                            //UnityEngine.Debug.Log("((투수업적))==============>> 홀드조건 만족하고 강판 // 투수이름: " + pLastPitcher.getName());
                            pLastPitcher.setPitcherAchieve(Param.ST_HLD, Param.P_ACHIEVE_COMPLETE);
                        }
                        else
                        {
                            //UnityEngine.Debug.Log("((투수업적))==============>> 블론을 저지르고 강판 // 투수이름: " + pLastPitcher.getName());
                            pLastPitcher.setPitcherAchieve(Param.ST_BS, Param.P_ACHIEVE_COMPLETE);
                        }
                    }
                    else
                    {
                        //승패조건
                        //UnityEngine.Debug.Log("[투수교체]==============>> 강판시 승패 조건을 체크");
                        if (checkWinPitcher(manager.defenseIndex, curIndex) == true)
                        {
                            //UnityEngine.Debug.Log("[투수교체]==============>> 승리투수 요건 만족하고 강판 // 투수이름: " + pLastPitcher.getName());
                            pLastPitcher.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_TRY);
                        }
                        else if (checkLosePitcher(manager.defenseIndex, curIndex) == true)
                        {
                            //UnityEngine.Debug.Log("[투수교체]==============>> 패배상황에서 강판 // 투수이름: " + pLastPitcher.getName());
                            pLastPitcher.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_TRY);
                        }
                        else
                        {
                            //UnityEngine.Debug.Log("[투수교체]==============>> 승패 상관 없이 강판 // 투수이름: " + pLastPitcher.getName());
                            pLastPitcher.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_NONE);
                            pLastPitcher.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_NONE);
                        }
                    }


                    run.setLastPitcher();
                    SimulPlayerManager.SetCurrentPitcherIndex(manager.defenseIndex, inPitcher);
                    setPitcherChange(SimulPlayerManager.GetPitcher(manager.defenseIndex), inPitcher, manager.defenseIndex, false);
                    initPitcher(SimulPlayerManager.GetPitcher(manager.defenseIndex), manager.defenseIndex);

                    //Debug.Log("===============>> 투수교체 액션상태에서 실행");

                    //세이브 혹은 홀드
                    //UnityEngine.Debug.Log("[투수교체]==============>> 등판시 홀드/세이브 조건을 체크");
                    if (checkSavePitcher(manager.defenseIndex) == true)
                    {
                        //UnityEngine.Debug.Log("[투수교체]==============>> 세이브 조건에서 출격 // 투수이름: " + pPitcher.getName());
                        pPitcher.setPitcherAchieve(Param.ST_SV, Param.P_ACHIEVE_TRY);
                    }

                    //pPitcher.changedOrder = (pLastPitcher.changedOrder + 1);

                   
                    return true;
                }
            }
            return false;
        }

        //승리투수 요건 체크
        private bool checkWinPitcher(int team, int index)
        {
            if (manager.nGameScore[team] > manager.nGameScore[1 - team])
            {
                if (index == SimulPlayerManager.GetStarterIndex(team) && manager.nInningCount < 6)
                {
                    //UnityEngine.Debug.Log("[투수교체]==============>>선발투수 이기고 있지만 5이닝 못채우면");
                    manager.winPitcherIndex[team] = -1;
                    return false;
                }

                if (manager.winPitcherIndex[team] == index)
                {
                    return true;
                }
            }
            return false;
        }

        //패전투수 요건 체크
        private bool checkLosePitcher(int team, int index)
        {
            if (manager.nGameScore[team] < manager.nGameScore[1 - team])
            {
                if (index == SimulPlayerManager.GetStarterIndex(team))
                {
                    //UnityEngine.Debug.Log("[투수교체]==============>>선발투수 지고 있을 떄 강판 되면 무조건 패전 인덱스");
                    manager.losePitcherIndex[team] = index;
                    return true;
                }

                if (manager.losePitcherIndex[team] == index)
                {
                    return true;
                }
            }
            return false;
        }

        //세이브 투수 요건 체크
        private bool checkSavePitcher(int team)
        {
            int gab = manager.nGameScore[team] - manager.nGameScore[1 - team];
            if (manager.nInningCount >= 7 && (gab > 0 && gab <= 3))
            {
                return true;
            }
            return false;
        }

        //지침서 바꿔야 되는지를 체크
        private bool checkFatigueCase(bool bStart, int scoreGab, int allowrun)
        {
            if (curStamina < 10 || allowrun >= 7)  //스태미너 10%이하
            {
                if (bStart == true && manager.nInningCount >= 9 && scoreGab > 0)
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
            if (manager.nInningCount <= 7)
            {
                ////////UnityEngine.//Debug.Log("================>>7이닝 이전이므로 마무리 안나옴");
                return false;        //세이브 조건 1
            }
            if (scoreGab <= 0)
            {
                ////////UnityEngine.//Debug.Log("================>>점수가 뒤지고 있으므로 안나옴");
                return false;    //세이브 조건 2
            }

            int realScoreGab = (scoreGab - manager.potentialScoreLoss());

            ////////UnityEngine.//Debug.Log("===============>>본격적인 세이브 조건 체크 realScoreGab = " + realScoreGab);

            if (bStart == true)
            {
                if (manager.getOffeseScore() == 0)
                {
                    ////////UnityEngine.//Debug.Log("================>>선발이 완봉을 노리고 있으므로 마무리 안나옴");
                    return false; //조건1 불만족: 9이닝 선발이 완투 완봉을 노리지 않는 상태에서 세이브 상황
                }
                if (manager.nInningCount >= 9 && realScoreGab > 0)
                {
                    ////////UnityEngine.//Debug.Log("================>>선발이 완투승을 노리고 동점주자가 루상에 없으므로 안바꿈");
                    return false;//조건2 불만족: 9이닝 완투 완봉을 노리지 않는 상태에서 루상의 주자로 인해 세이브가 된 상황
                }

                //조건1,2 만족시
                if (manager.nInningCount >= 9 && realScoreGab <= 3)
                {
                    //////UnityEngine.//Debug.Log("================>>정석적인 세이브 상황");
                    return true;    //정석적인 세이브 상황
                }
                if (manager.nInningCount == 8 && realScoreGab <= 0)
                {
                    //////UnityEngine.//Debug.Log("================>>8회 동점 주자로 인한 세이브 상황");
                    return true;    //조건3: 8회 세이브 상황 (주자가 동점을 허용할 수 있는 상황)ㄴ            
                }
            }
            else
            {
                if (manager.nInningCount >= 9 && realScoreGab <= 3)
                {
                    //////UnityEngine.//Debug.Log("================>>중계의 정석적인 세이브 상황");
                    return true;    //정석적인 세이브 상황
                }
                if (manager.nInningCount == 8 && realScoreGab <= 0)
                {
                    //////UnityEngine.//Debug.Log("================>>중계의 8회 동점 주자로 인한 세이브 상황");
                    return true;    //조건3: 8회 세이브 상황 (주자가 동점을 허용할 수 있는 상황)
                }
            }


            return false;
        }

        //롱릴리프 조건 체크
        private bool checkLongReliefCondition(bool bStart, int scoreGab, int allowrun, ChangeType type = ChangeType.NA)
        {
            //5회 이전
            if (manager.nInningCount > 5)
            {
                ////////UnityEngine.//Debug.Log("================>>롱릴리프 6회 이후에 나오지 않는다");
                return false;
            }

            ////////UnityEngine.//Debug.Log("===============>>롱릴리프 조건 체크");
            
            if (bStart)
            {
                if (manager.nInningCount >= 3)
                {
                    if (scoreGab <= -5)
                    {
                        //////UnityEngine.//Debug.Log("================>>5점이상 뒤지고 있는경우 롱릴리프로 교체");
                        return true;
                    }
                    if (curStamina < 50 && allowrun >= 5)   //스태미너 50%이하
                    {
                        //////UnityEngine.//Debug.Log("================>>5점이상 이기고 지침인 경우 롱릴리프로 교체");
                        return true;
                    }
                    if (curStamina < 20 && allowrun >= 3) //스태미너 20%이하
                    {
                        //////UnityEngine.//Debug.Log("================>>3점이상 이기고 스태미나 방전 있는경우 롱릴리프로 교체");
                        return true;
                    }
                }
                else if (allowrun >= 7)
                {
                    //////UnityEngine.//Debug.Log("================>>7점이상 이기고 있는경우 롱릴리프로 교체");
                    return true;
                }
            }

            return false;
        }

        //추격 조건 체크
        private bool checkChaseCondition(bool bStart, int scoreGab, int allowrun, ChangeType type = ChangeType.NA)
        {
            //6,7,8,9회
            if (manager.nInningCount <= 5)
            {
                ////////UnityEngine.Debug.Log("====>>6회이전 패전조가 나오지 않는다");
                return false;
            }

            int realScoreGab = (scoreGab - manager.potentialScoreLoss());
            
            if (bStart)
            {
                if ((curStamina < 25 || allowrun >= 3) && scoreGab < 0) //스태미너 25%이하
                {
                    //조건3: 6이닝 이후 게임이 지고 있는 경우 선발 체력이 매우 지침이 된 경우
                    //////UnityEngine.Debug.Log("====>>6이닝 이후 게임이 지고 있는 경우 선발 체력이 매우 지침 패전조 출격");
                    return true;
                }

                if ((curStamina < 25 || allowrun >= 3) && scoreGab >= 5 && manager.getOffeseScore() != 0) //스태미너 25%이하
                {
                    //조건4: 6이닝 이후 5점차 이상으로 이기고 있는 경우 선발의 체력이 매우지침 & 완투 완봉을 노리는 상태가 아닌 경우
                    //////UnityEngine.Debug.Log("====>>6이닝 이후 선발이 완봉을 노리고 있지 않으면서 게임을 5점차이상으로 이기고 있는 경우 선발 체력이 매우 지침 패전조 출격");
                    return true;
                }

                if (realScoreGab <= -3)
                {
                    //조건1: 6이닝 이후 체력과 관계없이 선발이 3점차 이상으로 경기를 뒤지고 있는 경우
                    //조건2: 6이닝 이후 체력과 관계없이 선발이 3점차 이상으로 지게 만들 주자를 루상에 허용한 경우
                    //////UnityEngine.Debug.Log("====>>6이닝 이후 체력과 관계없이 선발이 (주자포함) 3점차 이상으로 뒤지고 있는 경우 패전조 출격");
                    return true;
                }

                if ((curStamina < 25 || allowrun >= 3) && realScoreGab < 0 && manager.nInningCount == 6 && manager.getOffeseScore() != 0)   //스태미너 25이하
                {
                    //조건5: 정확히 6이닝 이기고 있으나 역전주자 허용 스태미너 레이트 0.25이하 완봉조건이 아닌경우
                    //////UnityEngine.Debug.Log("====>>정확히 6이닝 이기고 있으나 역전주자 허용 스태미너 레이트 0.2이하 완봉조건이 아닌경우 패전조 출격");
                    return true;
                }

            }
            else
            {
                if (type == ChangeType.SAVE)
                {
                    //세이브 투수에 이은 또는 필승조 투수에 이은
                    //////UnityEngine.Debug.Log("====>>마무리 블론시 역전당하면 바로 패배조 투입");
                    return true;
                }
                else if (type == ChangeType.MUST_WIN)
                {
                    //필승조에 이은
                    if (curStamina < 50 || allowrun >= 2)   //50이하
                    {
                        //////UnityEngine.Debug.Log("====>>역전당하면 승리조의 체력은 50%소진후");
                        return true;
                    }
                }
                else //if (type == ChangeType.CHASE)
                {
                    if (curStamina < 30 || allowrun >= 3)   //30이하
                    {
                        //////UnityEngine.Debug.Log("====>>여전히 지고 있는 경우 70프로 소진후");
                        return true;
                    }
                }

            }

            return false;
        }

        //셋업 조건 체크
        private bool checkSetupCondition(bool bStart, int scoreGab, int allowrun, ChangeType type = ChangeType.NA)
        {
            //6,7,8,9회
            if (manager.nInningCount <= 5)
            {
                ////////UnityEngine.Debug.Log("====>>6회이전 승리조가 나오지 않는다");
                return false;
            }

            if (scoreGab >= 5 || scoreGab < 0)
            {
                ////////UnityEngine.Debug.Log("====>>5점이상으로 이기고 있거나 지고 있는경우 승리조는 나오지 않는다.");
                return false;
            }

            int realScoreGab = (scoreGab - manager.potentialScoreLoss());
            
            if (bStart)
            {
                if (manager.nInningCount == 7)
                {
                    //7이닝 조건
                    if (curStamina < 25 || allowrun >= 3)   //스태미너 25이하
                    {
                        //조건3: 7이닝에 선발 체력이 매우 지침이된 경우
                        //////UnityEngine.Debug.Log("====>>7이닝에 선발 체력이 매우 지침이된 경우 승리조 출격");
                        return true;
                    }
                    if ((curStamina < 40 || allowrun >= 3) && realScoreGab <= 0) //스태미너 40이하
                    {
                        //조건3: 7이닝에 선발 체력이 매우 지침이된 경우
                        //////UnityEngine.Debug.Log("====>>7이닝에 선발 체력이 지침이 되고 동점주자를 허용한 경우 승리조 출격");
                        return true;
                    }
                }
                else if (manager.nInningCount == 8)
                {
                    //8이닝 조건
                    if ((curStamina < 25 || allowrun >= 3) && manager.getOffeseScore() != 0)  //스태미너 25이하
                    {
                        //조건5: 8이닝에 선발이 완봉을 노리지 않는 상태에서 체력이 매우 지침이 된 경우 (2순위 구원)
                        //////UnityEngine.Debug.Log("====>>8이닝에 선발이 완봉을 노리지 않는 상태에서 체력이 매우 지침이 된 경우 승리조 출격");
                        return true;
                    }
                    if ((curStamina < 40 || allowrun >= 3) && manager.getOffeseScore() != 0 && realScoreGab <= 0) //스태미너 40이하
                    {
                        //조건6: 8이닝에 선발이 완봉을 노리지 않는 상태에서 체력이 지침 이하이며 루상에 동점주자를 허용한 경우 (2순위 구원)
                        //////UnityEngine.Debug.Log("====>>8이닝에 선발이 완봉을 노리지 않는 상태에서 체력이 지침 이하이며 루상에 동점주자를 허용한 경우 승리조 출격");
                        return true;
                    }
                }
                else if (manager.nInningCount >= 9)
                {
                    //9이닝 이후
                    if ((curStamina < 10 || allowrun >= 3) && manager.getOffeseScore() != 0 && scoreGab > 3)    //스태미너 10이하
                    {
                        //조건7: 9이닝에 세이브 조건이 아니며 완봉을 노리지 않는 상태에서 체력이 방전된 경우
                        //////UnityEngine.Debug.Log("====>>9이닝에 세이브 조건이 아니며 완봉을 노리지 않는 상태에서 체력이 방전된 경우 경우 승리조 출격");
                        return true;
                    }
                }

                if ((curStamina < 25 || allowrun >= 3) && realScoreGab < 0 && manager.nInningCount == 7 && manager.getOffeseScore() != 0)  //스태미너 25이하
                {
                    //조건8: 정확히 7이닝 이기고 있으나 역전주자 허용 스태미너 레이트 0.2이하 완봉조건이 아닌경우
                    //////UnityEngine.Debug.Log("====>>정확히 7이닝 이기고 있으나 역전주자 허용 스태미너 레이트 0.2이하 완봉조건이 아닌경우 승리조 출격");
                    return true;
                }

            }
            else
            {
                if (type == ChangeType.SAVE)
                {
                    //세이브 투수에 이은 또는 필승조 투수에 이은
                    //////UnityEngine.Debug.Log("====>>마무리 블론시 최소 동점이면 바로 승리조 투입");
                    return true;
                }
                else if (type == ChangeType.MUST_WIN)
                {
                    //필승조에 이은
                    //////UnityEngine.Debug.Log("====>>다음 승리조 투입");
                    return true;
                }
                else// if (type == ChangeType.CHASE)
                {
                    //////UnityEngine.Debug.Log("====>>추격시 역전시 승리조 투입");
                    return true;
                }
            }

            return false;
        }

        //투수를 바꾸어야 하는지 여부를 체크 // 매 이닝과 addScore마다 호출
        public bool pitcherChanging(int def, int off)	//매 AddScore 마다 호출
        {
            pitcherChangeType = ChangeType.NA;

            nextBatter = SimulPlayerManager.GetNextBatter(off, 1);// manager.pFielder[off, (curLineupCount + 1) % 9]; //다음타자
            nextBatter2 = SimulPlayerManager.GetNextBatter(off, 2);//manager.pFielder[off, (curLineupCount + 2) % 9]; //다다음타자

            //실점에 따른 투수교체 타이밍 조절
            int scoreGab = manager.getScoreGab(def);
            int allowrun = allowRun[def];

            if (SimulPlayerManager.IsStartPitcher(def) == true)//if (manager.starterIndex[def] == manager.pitcherIndex[def])
            {
#if _PITCHER_CHANGE_TEST
                //투수교체 테스트용
                if (pPitcher.getStat(Param.ST_PNP) > 1)
                {
                    pitcherChangeType = ChangeType.LONGRIELEF;
                    return true;
                }//지워지워
#endif

                //선발투수 교체
                //세이브 조건
                if (checkSaveCondition(true, scoreGab) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>세이브 투수 출현");
                    pitcherChangeType = ChangeType.SAVE;
                    return true;
                }

                //롱릴리프
                if (checkLongReliefCondition(true, scoreGab, allowrun) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>롱리리프 투수 출현");
                    pitcherChangeType = ChangeType.LONGRIELEF;
                    return true;
                }

                //추격 패전
                if (checkChaseCondition(true, scoreGab, allowrun) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>추격조 투수 출현");
                    pitcherChangeType = ChangeType.CHASE;
                    return true;
                }
                //필승
                if (checkSetupCondition(true, scoreGab, allowrun) == true)
                {
                    ////UnityEngine.//Debug.Log("===============>>필승조 투수 출현");
                    pitcherChangeType = ChangeType.MUST_WIN;
                    return true;
                }

                //방전시 바꿈
                if (checkFatigueCase(true, scoreGab, allowrun) == true)
                {
                    //////UnityEngine.//Debug.Log("===============>>방전으로 인한 투수 바꿈");
                    if (manager.nInningCount <= 5)
                    {
                        ////UnityEngine.//Debug.Log("===============>>5회 이전 롱릴리프 투수 바꿈");
                        pitcherChangeType = ChangeType.LONGRIELEF;
                    }
                    else if (manager.nInningCount <= 6)
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
                int curInning = (manager.nInningCount * 10 + manager.nOutCount) - startInning[def];

                if (bSaveOn[def] == true)
                {
                    //세이브 투수 
                    //1이닝 이상을 던진 경우
                    if (curInning >= 10)
                    {
                        //////UnityEngine.//Debug.Log("==============>>마무리 투수가 1이닝 이사을 던진경우");
                        if (scoreGab < 0)
                        {
                            //////UnityEngine.//Debug.Log("==============>>마무리 투수가 던져서 지고 있을떄");
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
                            //////UnityEngine.//Debug.Log("==============>>마무리 투수가 던져서 최소 동점 허용");
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
                else if (bChaseOn[def] == true)
                {
                    //추격조 투수 

                    //1이닝 이상을 던지고 
                    //세이브 조건
                    if (curInning >= 10)
                    {
                        //////UnityEngine.//Debug.Log("==============>>추격조 투수가 1이닝 이사을 던진경우");
                        if (scoreGab >= 0)
                        {
                            //////UnityEngine.//Debug.Log("==============>>추격조 투수가 던져 최소 동점이상이 되었을때");
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
                            //////UnityEngine.//Debug.Log("==============>>추격조 투수가 던져 여전히 지고 있는 경우");
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
                else if (bSetupOn[def] == true)
                {
                    //필승 투수 
                    if (curInning >= 10)
                    {
                        //////UnityEngine.//Debug.Log("==============>>필승조 투수가 1이닝 이사을 던진경우");
                        if (scoreGab > 0)
                        {
                            //////UnityEngine.//Debug.Log("==============>>이기고 있는 경우 세이브 조건 검색");
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
                            //////UnityEngine.//Debug.Log("==============>>동점이상 5점차 이하 필승조 가동 조건 검색");
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
                            //////UnityEngine.//Debug.Log("==============>>필승조가 나왔으나 지고 있거나 5점이상으로 이기고 있는 경우");
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
                else if (bLongReliefOn[def] == true)
                {
                    //6이닝 이후인 경우 선발 조건이랑 동일시
                    if (manager.nInningCount >= 6)
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
                            if (checkSetupCondition(true, allowrun, scoreGab) == true)
                            {
                                ////UnityEngine.//Debug.Log("===============>>롱릴리프가 던지다가 필승조 조건시 투수 바꿈");
                                pitcherChangeType = ChangeType.MUST_WIN;
                                return true;
                            }
                        }
                        else
                        {
                            //추격 패전
                            if (checkChaseCondition(true, allowrun, scoreGab) == true)
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
                    if (manager.nInningCount <= 6)
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

        //AI가 현재 투수를 바꿔야하는지를 판단
        public int getChangePitcher(int team, ChangeType changeType)
        {
            int inIndex = -1;
            inIndex = getReiefIndex(team, changeType);

            if (inIndex != -1)
            {
                inPitcher = inIndex;
                outPitcher = SimulPlayerManager.GetPitcherIndex(team);// manager.pitcherIndex[team];
            }

            return inIndex;
        }

        //투수 가치값을 리턴
        public int getPitcherValue(CPlayer player)
        {
            //나중에 특수 능력 고려해라~~
            int value = 0;
            
            PitchingArsenal[] ballType = player.getBallType();

            for (int i = 0; i < 5; i++)
            {
                if(ballType[i] != PitchingArsenal.NONE)  //구종이 있는 경우
                {
                    value += player.getBallValue(ballType[i]);
                }
            }

            return (value);

        }

        //중계 투수의 인덱스를 리턴
        private int getReiefIndex(int team, ChangeType changeType)
        {
            // 상황에 맞는 밸류를 
            int inIndex = -1;
            int scoreGab = manager.getScoreGab(team);
            int[] changeValue = new int[BallPlayManager.NUM_PITCHER];
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
                if (manager.nInningCount >= 8)
                {
                    //8회 이후
                    for (int i = 0; i < PitchingMechanism.TYPE_NUM; i++)
                    {
                        _weight[i] = PitchingMechanism.setupValueOver8[i];
                    }
                    bDescendingOrder = true;
                }
                else if (manager.nInningCount == 7)
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



            for (int i = 0; i < BallPlayManager.NUM_PITCHER; i++)
            {
                int rating = getPitcherValue(SimulPlayerManager.GetPitcher(team, i)); //getPitcherValue(manager.pPithcer[team, i]);
                int outWeight = (SimulPlayerManager.GetPitcherOut(team, i) == true ? -1 : 1);//manager.pitcherOut[team,i]==true?-1:1;
                int pitcherPosition = SimulPlayerManager.GetPitcher(team, i).getPitcherPosition();// manager.pPithcer[team, i].getPitcherPosition();
                //////UnityEngine.//Debug.Log("============>>pitcherPosition: " + pitcherPosition);

                if (pitcherPosition == 0 || SimulPlayerManager.GetPitcherOut(team, i) == true)
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

            for (int i = 0; i < BallPlayManager.NUM_PITCHER; i++)
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


        ////////////////////////////////////////////////////////////////////////////    
        //투수 능력치 얻어오기
        ////////////////////////////////////////////////////////////////////////////

        //현재 볼스피드 얻어오기
        public int getBallSpeedValue(PitchingArsenal selectBall)
        {
            float curSpeed = PitchingMechanism.GetBallSpeed(this, selectBall);

            if (fatigueStep == FatigueStep.STAMINA_FATIGUE) curSpeed *= 0.98f;
            else if (fatigueStep == FatigueStep.STAMINA_VERY_FATIGUE) curSpeed *= 0.955f;
            else if (fatigueStep == FatigueStep.STAMINA_EXUSTED) curSpeed *= 0.935f;

            return (int)curSpeed;
        }



        //선택한 구종타입 얻어오기
        public PitchingArsenal getGujongType(int select)
        {
            return pPitcher.getBallType()[select];
        }
               

        //와일드 피치 파라메터 세텅
        public void setWildParm()
        {
            //포수 블록율
            CPlayer catcher = field.fielder[CPlayer._CATCHER].pFielder;
            int catcherValue = catcher.getFielding();
            if (catcher.getPosition() != CPlayer._CATCHER)
            {
                //주포지션이 포수가 아닌경우 블록율 제로
                catcherBlockRate = 0;

                //투수 폭투율
                pitcherWildRate = 40;   //주포지션이 아닌경우 투수 폭투율 60;
            }
            else
            {
                /*
                if (catcherValue < 200) catcherValue = 200;
                float sinValue = (catcherValue - 200.0f) * 0.07f;
                catcherBlockRate = Mathf.Sin(sinValue * Mathf.PI / 180) * Mathf.Pow(35.0f, 1.2f) + 18.0f;*/


                catcherBlockRate = catcherValue;

                if (catcher.skillAvailable(SkillIndex.CatcherBallBlocking) == true)
                {
                    catcherBlockRate *= 1.5f;
                }

                //투수 폭투율
                pitcherWildRate = 89;
            }
        }

        //캐처의 블록 여부
        public bool checkCatcherBlock()
        {
#if _WILD_PITCH_TEST
            catcherBlockRate = 750;
#endif
            //true이면 블록
            //false이면 와일드 캐치
            //catcherBlockRate은 포수수비가 높을수록 높아짐
            //catcherBlockRate가 높을수록 블록확률 커짐
            if (Random.Range(0.0f, 1500) < catcherBlockRate)
            {
                return true;
            }
            return false;

        }
        
        
        ////////////////////////////////////////////////////////////////////////////    
        //기타 연출
        ////////////////////////////////////////////////////////////////////////////
        //아이콘 세팅
        public IEnumerator setIcon(bool bHomeRun = true, string sprName = "hiticon")
        {      
            icon.gameObject.SetActive(true);
            if (bPvState == true)
            {
                icon.scale = new Vector3(nSign, 1, 1);
                icon.transform.localPosition = new Vector3(44, 460, 0);
            }
            else
            {
                icon.scale = new Vector3(nSign*0.8f, 0.8f, 1);
                icon.transform.localPosition = new Vector3(90, 137, -0.1f);
            }            
            UITweener tween = icon.GetComponent<UITweener>();
            tween.ResetToBeginning();
            tween.PlayForward();
            yield return new WaitForSeconds(0.5f);
            icon.gameObject.SetActive(false);
        }


        
#if DEATHMATCH_OLD_VERSION
        public void pitcherSkip()
        {
            pState = PitcherState._NONE;
            setPitchSystemDraw(false);
        }
#endif

        
        ////////////////////////////////////////////////////////////////////////////    
        //3D 피칭 & 배팅 시스템
        ////////////////////////////////////////////////////////////////////////////

        //타구 발생시 배팅 시스템 설정
        public void setBattingSystem()
        {
            if (bPvState == true)
            {
                field.b2DBattingSystem = false;
                pitchPv.battingSystemPv.setHitVector();
            }
            else
            {
                //처리됨
#if _NOT_YET_GROUNDERVIEW
                field.b2DBattingSystem = false;
                pitch.battingSystem.setHitVector();
#else
                if (field.ballPower > 25 && field.ball.firstAngleZ < 0 && Mathf.Abs(field.ball.firstAngle) < 44.5f) //if (field.b2DBattingSystem == true)
                {
                    field.b2DBattingSystem = true;
                    bball.setHitVector();
                }
                else
                {
                    field.b2DBattingSystem = false;
                    pitch.battingSystem.setHitVector();
                }
#endif
            }
        }

        //설정된 벡터값을 바탕으로 배팅시스템 구현
        public void setHitBallNextStep()
        {
            if (bPvState == true)
            {
                //pitchPv.battingSystemPv.setHitBallNextStep();
            }
            else
            {
                //처리됨
                if (field.b2DBattingSystem == true)
                {
                    bball.setHitBallNextStep();
                }
                else
                {
                    pitch.battingSystem.setHitBallNextStep();
                }
            }
        }

        //피칭 시스템 설정
        private void setPitchSystem()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.setPitcher(this);
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.setPitcher(this);
            }
        }

                
        /// <summary>
        /// //피치 시스템을 화면에 그려줌
        /// </summary>
        /// <param name="bDraw"></param>
        public void setPitchSystemDraw(bool bDraw)
        {
            int layerMask;

            if (bPvState == true)
            {
                //투수뷰
                if (bDraw == true)
                {
                    //layerMask = 1 << LayerMask.NameToLayer("PITCH_LAYER");
                    pitchPv.origin.gameObject.SetActive(bDraw);
                    pitchPv.pitchOriginPv.setVectorInit();
                }
                else ///_FIELDVIEW
                {
                    //layerMask = 0;
                }
                //Camera unityCam = pitchPv.camera.GetComponent<Camera>();
                //unityCam.cullingMask = layerMask;
            }
            else
            {
                //타자뷰
                if (bDraw == true)
                {
                    layerMask = 1 << LayerMask.NameToLayer("PITCH_LAYER");
                    pitch.origin.gameObject.SetActive(bDraw);
                    pitch.pitchOrigin.setVectorInit();
                }
                else ///_FIELDVIEW
                {
                    layerMask = 0;
                }
                Camera unityCam = pitch._camera.GetComponent<Camera>();// pitch.GetComponent<Camera>().GetComponent<Camera>();
                unityCam.cullingMask = layerMask;
            }

        }

        /// <summary>
        /// 피치시스템에서 공이 맞은 경우
        /// </summary>
        public void setPitchSystemHit()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.bHit = true;
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.bHit = true;
            }
        }

        //피칭시스템에서 힛바이피치가 발생한경우
        public void setPitchSystemHitByPitched()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.bHitByPitched = true;
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.bHitByPitched = true;
            }
        }

        //피칭 시스템에서 공이 화면에서 사라지게
        public void setPitchSystemBallErase()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.ballObj.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.ballObj.GetComponent<Renderer>().enabled = false;
            }
        }

        //피칭 시스템에서 볼이 존을 지나갔는지 여부를 체크
        public bool checkPitchSystemZoneCheck()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                return pitchPv.pitchOriginPv.bZoneCheck;
            }
            else
            {
                //처리됨
                return pitch.pitchOrigin.bZoneCheck;
            }
        }
        
        /// <summary>
        /// 피칭시스템으로스터 타이밍을 잡기위한 hitrate얻어옴
        /// </summary>
        /// <returns></returns>
        public float getHitRate()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                return pitchPv.pitchOriginPv.getHitRate();
            }
            else
            {
                //처리됨
                return pitch.pitchOrigin.getHitRate();
            }

        }

        /// <summary>
        /// 선택된 구종의 구위 밸류를 피치시스템에 적용
        /// </summary>
        /// <param name="type"></param>
        /// <param name="guweeValue"></param>
        private void setMoveTypeAndGuwee(BallMoveType type)
        {
            if (type == BallMoveType.Curve || type == BallMoveType.Slide)
            {
                bBreakingBallType = true;
            }
            else
            {
                bBreakingBallType = false;
            }

            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.setMoveTypeAndGuwee(type);
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.setMoveTypeAndGuwee(type);
            }

        }

        //피칭스스템으로 부터 zoneX 얻어옴
        public float getCurrentZoneX(float maxX)
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                return pitchPv.pitchOriginPv.getCurrentZoneX(Zone.STRIKE_ZONE_WIDTH);
            }
            else
            {
                //처리됨
                return pitch.pitchOrigin.getCurrentZoneX(Zone.STRIKE_ZONE_WIDTH);
            }
        }

        //피칭스스템으로 부터 zoneY 얻어옴
        public float getCurrentZoneY(float maxY)
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                return pitchPv.pitchOriginPv.getCurrentZoneY(Zone.STRIKE_ZONE_HEIGHT);
            }
            else
            {
                //처리됨
                return pitch.pitchOrigin.getCurrentZoneY(Zone.STRIKE_ZONE_HEIGHT);
            }
        }

        //피칭시스템에 폭투 설정
        private void setMiss(bool bMiss)
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.pitchOriginPv.setMiss(bMiss);
            }
            else
            {
                //처리됨
                pitch.pitchOrigin.setMiss(bMiss);
            }
        }

        //피칭 시스템 상에서 퍼펙트 타이밍값 얻어옴
        private float getPerfectTime()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                return pitchPv.pitchOriginPv.perfectTime;
            }
            else
            {
                //처리됨
                return pitch.pitchOrigin.perfectTime;
            }
        }

        //피칭 시스템에 설정된 볼 구속 얻어옴
        public int getBallSpeed()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                return pitchPv.pitchOriginPv.getBallSpeed();
            }
            else
            {
                //처리됨
                return pitch.pitchOrigin.getBallSpeed();
            }
        }

        //피칭 시스템에서 폭투 체크 -> 필요없을지도
        private bool checkWildPitch()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                return pitchPv.pitchOriginPv.bWildPitch;
            }
            else
            {
                //처리됨
                return pitch.pitchOrigin.bWildPitch;
            }
        }

        //피칭 시스템에서 투구 완료 여부 체크
        private bool checkPitchFinish()
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                return pitchPv.pitchOriginPv.checkCount();
            }
            else
            {
                //처리됨
                return pitch.pitchOrigin.checkCount();
            }
        }

        //피칭 시스템에서 존을 세팅함
        private void pitchSetZone(float x, float y, float maxX, float maxY)
        {
            if (bPvState == true)
            {
                //피칭뷰용 버전으로 고쳐
                pitchPv.setZonePv(x, y, maxX, maxY);
            }
            else
            {
                //처리됨
                pitch.setZone(x, y, maxX, maxY);
            }
        }






        //
        private bool bPauseQue = false;
        public bool setPause()
        {
            if (manager.bMyTurn == true)
            {                
                bPauseQue = true;
                return false;
            }
            else
            {                
                bPauseQue = false;
                return true;
            }
        }


        public void setResume()
        {
            Mode.bPauseGame = false;
            if (manager.bMyTurn == true)
            {
                ControlBattingUI.CheckPauseState(false);
                if (Mode.bPvpMode433 == false)//Mode.bPvpMode == false )
                {
                    pState = PitcherState._GET_SIGN;
                    //if (Mode.bPowerfulType == true)                
                    {
                        StartCoroutine(startPichingAnim3());
                    }
                    /*else
                    {
                        //manager.pitcher.startPichingAnim2();
                    }*/
                }
            }
        }

        
        /// <summary>
        /// 매의눈 연출
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator falconEyeEffect(float delay)
        {
            yield return new WaitForSeconds(delay * 0.25f);            
            Time.timeScale = 0.85f;
            yield return new WaitForSeconds(delay);
            Time.timeScale = 1.0f;
            //CameraManager.SetBlur2(0, false);
            manager.batterSkillFlag = SkillFlag.None;
        }


        public void syncHand(CPlayer pitcher)
        {
            batter.bPvState = false;
            if (Mode.bPitchingViewActive == true)
            {
                batter.bPvState = bPvState = (manager.bMyTurn ? false : true);
            }
            pitchHand = pitcher.getThrowHand();	//현재 투수 던지는 어깨 설정
            nSign = (pitchHand == CPlayer._LEFTHAND ? -1 : 1); //좌우 사인
            pitchingType = pitcher.getPitchingType();	//현재 투수 던지는 타입 (투구폼)  

            //Debug.Log("===========================================>> 투수의 손 동기화 시킴 pitchHand = " + pitchHand);

            pPitcher = pitcher;
        }

    }
}