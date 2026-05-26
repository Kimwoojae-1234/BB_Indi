//#define NO_SPECIAL_MOVE
//#define NO_DASH_MOVE
//#define NO_SPECIAL_THROW
//#define NO_SPECIAL_FLY_MOVE
//#define NO_FIELDING
//#define _SCENARIO
//#define _TEST_FIELDER
//#define _TEST_SKILL_BALANCE
//#define _INPUT_SKILL
//#define _NO_TEXTURE_LOADING       //지워지워
//#define _THROWING_TEST

using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class Fielder : MonoBehaviour
    {     
#if _THROWING_TEST
        public int test_throwing = 800;
#endif

        //public const float _FIELDER_MULTI = 1.15f;

        //스파인 애니메이션
        public SkeletonAnimation anim;
        tk2dSprite shadow;
        public string strID, lastStrID, _strID;
        public float timeScale, timeScaleRate;
        bool loop;
        //bool bChangeDir, lastChangeDir;
        bool bSpecialAnim;
        public FieldParm.ThrowAgain throwAgainState;

        //선수 데이터
        public CPlayer pFielder;    //야수 데이터
        public TextMesh fielderName;       //이름 오브젝트

        //에러 마크
        public tk2dSprite errorMark;

        //필드 데이터
        Field field;                //필드 오브젝트

        //충돌체
        BoxCollider _collider;       //충돌체 오브젝트

        //오브젝트 초기화
        public bool bObjectInit = false;
        public bool bFielderActive = false;

        // 인덱스
        public int posIndex;//,ind;
        public int nCoveringIndex;

        //상태
        public FielderAction actState, lastActState;    //상태
        public ActionStep aStep;                        //액션
        public NextAction nextMove;                     //넥스트 무브
        public FlyCatch flyballCatchType;               //플라이 캐치 타입
        public GrounderCatch grounderCatchType;         //그라운더 캐치 타입
        ThrowType throwType, throwTypeFirstBase;        //쓰로우 타입
        public int tagState;
        //미스매치 여부
        public bool bMisMatch;


        //프레임
        public float curTime, deltaTime;   //타임 베이스
        float remainTime;   //수비수에게 남은 시간
        float fieldingTime; //필딩 시간
        float flyRemainingTime; //플라이볼 남은 시간
        float flyTimeGab; //주자 판단AI를 
        float throwDelayTime;

        //벡터
        public float posX, posY;
        public float offsetX, curOffsetX, offsetDV;
        bool bOffsetLinear;
        public float originX, originY;
        float screenX, screenY;
        int nFielderDir, nLastDir, catchDir;
        float tryDstX, tryDstY;
        public float dX, dY;   //이동하면서 움직이는
        float tdX, tdY; //던지면서 움직이는 
        float tStartX, tStartY, tStartZ; //던지기 시작하는 위치
        float tSpeed;
        float tDir;       //던지면서 움직이는 방향
        float throwMoveRate;    //던지면서 이동시 릴리즈시 움직이는 비율
        float throwStopRate;    //(비율) 움직이는게 중단까지의..
        public float dstX, dstY;
        public float distanceToBall;
        public float speed, angleDir;
        public float grounderDistance, grounderRemainTime;

        //프로퍼티
        //public int nFielderSkin;
        //public string nFielderName;

        //능력치
        public int fieldingAbil, throwingAblil, runningAbil;
        //실게임 적용 능력치
        public float THROW_SPEED;   //(속도 m/s) 기본 송구 속도
        public float THROW_DELAY;   //(딜레이 s) 기본 송구 딜레이
        public float FIELD_DELAY;	//(딜레이 s) 기본 필딩 딜레이
        public float FIELDER_SPEED; //(딜레이 s) 기본 이동 속도

        //가변 비율 능력치 // 특정모션에 따른 능력치 조정 비율
        float catchDelayRate;// (0.1 ~ 1) 기본 캐치 딜레이에 곱해주는 비율. 능력치가 좋을 수록 비율이 줄어든다
        float throwDelayRate;// (0.1 ~ 1) 기본 송구 딜레이에 곱해주는 비율. 능력치가 좋을 수록 비율이 줄어든다
        float throwSpeedRate;// (0.7 ~ 1.5) 기본 송구 스피드에 곱해주는 비율. 능력치가 좋을 수록 비율이 늘어난다

        //기타 능력치 (1차능력치와 관계없는...)
        float taggingDelay; //송구의 정확도와 상관있는
        float longTagDelay; //포수의 도루 송구 딜레이
        float specialThrowCatchDelayRate; //(0.1~1) 특수모션 뒤 특수 던지기 발동하면 적용되는 캐치 딜레이 비율. 능력치가 좋으면 발동확률이 높아지고 일단 발동되면 모션에 따른 비율은 일정하다. 즉 비율은 모션에 따라 다를뿐 능력치랑 상관없다.
        float throwReadyDelay;  //(초단위)) //각 던지기 모션에 따른 릴리즈까지의 시간    
        public float THROW_WRIST; //아직 적용 안됨    //1최소 ~ 최대(0.5정도) (0보다 반드시 커야함)
        float specialTimeGabX;  //특수동작시 시간차 때문에 발생하는 거리 조정값

        //동작
        public int moveStep;

        //필딩 플래그
        public bool bCarrier;                        //볼을 가지고 나르는지 여부
        public bool bFlyCatchAvail, bFlyCatchTry;
        public bool bFlySlowMove, bFlyFastMove;
        public bool bGrounderAvail, bGrounderTry, bVeryShortGrounder;
        public bool bDeepFlyChase;
        public bool bFlyCoverChase;
        public bool bBaseCovering, bRunnerArrivedOnBase, bTossTaking, bTossTaked, bThrowErrorCoverd;
        bool bBaseLeave;//bHoldAnim;,
        public bool bPositionCovering;
        public bool bRelayPositioning, bRelayStart;
        public bool bOverHead;
        bool bThrow, bThrowAvailable;
        public bool bForcedThrow;
        bool bThrowAddDelay;
        bool bThrowableChecked;
        public bool bFenceReady;
        bool bRoundingReady;
        bool bDashCatched;
        bool bDashCatchTry;
        bool bDashQuickThrow;   //특능여부 체크
        bool bSecondMove;
        bool bFlyballCatch;
        bool bDoublePlayAction;
        bool bDiveSuccess;
        bool bSecondDelayCover; //2루수의 딜레이 커버 //2루수 특수
        public bool bSpecialEnable;
        bool bNoCatchFlag;
        bool bDustOn;
        bool bSideChase;
        bool bLaserThrowCheck;
        public bool bChaseColliderCheck;

        //에러플레그
        public bool bCatchErrorFlag;
        public bool bThrowErrorFlag;
        public bool bCatchErrorSpeicalAnimation;    //스페셜 엔메이션으로 포구 에러 -> 이경우 연결된 동작이 아닌 홀드 상태로

        public bool bStun;

        //스킬 변수
        public int skillRangeLevel,                                //범위
                    skillSlidingCatchLevel, skillJumpCatchLevel,    //스페셜 캐치
                    skillSpinThrowLevel, skillQuickThrowLevel,skillJumpThrowLevel,skillDashThrowLevel,  //스페셜 송구
                    skillDivingLevel;                               //외야 다이빙캐치

        private bool specialCatchSuccess, specialThrowingSuccess,   //스페셜 캐치, 스페설 송구 성공여부
                     pitcherReactSuccess;                           //투수 리액션

        private bool homerunStealActive, homerunstealSuccess;
        

        //버그 수정용
        bool bNoDirChange, bNoDirChange2;


        //기타 dashRatio
        float dashRatio;

        //이벤트
        bool bThrowAnimFlag = false;

        //네트워크 동기화
        //public int nNetTargetIndex;

        //Start
        void Start()
        {

            anim = null;

            //야수는 여기 이전에 이미지 리소스 생성 필요
            _collider = gameObject.GetComponent<BoxCollider>();
            shadow = gameObject.GetComponent<tk2dSprite>();
            lastStrID = "null";

            bObjectInit = false;
            bFielderActive = false;

            transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            //shadow.scale = new Vector3(1.6f, 1, 1);
            //shadow.color = new Color(1, 1, 1, 0.67f);
            /*
            GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/fielder/fielderSkelPrefab", transform, new Vector3(0, 0, -0.01f), "skeleton");
            skeleton.transform.localScale = new Vector3(100, 100, 100);
            anim = skeleton.GetComponent<SkeletonAnimation>();*/

            //field.netTarget[posIndex] = FieldParm.NoLink;// nNetTargetIndex = -100;
        }

        //스파인 이벤트 처리
        public void HandleEvent(Spine.TrackEntry trackEntry, Spine.Event e)//HandleEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
        {
            //Debug.Log(trackIndex + " " + state.GetCurrent(trackIndex) + ": event " + e + ", " + e.Int);
            string strEvent = e.ToString();

            if (strEvent == "throw")
            {
                if (bThrowAnimFlag == false)
                {
                    bThrowAnimFlag = true;
                }
            }
        }

        //Update
        void Update()//FixedUpdate()
        {
#if! NO_FIELDING
            if (bFielderActive == true)
            {
                //if (field.bInputWait == true) return;
                deltaTime = field.getDeltaTime();
                nextFrame();
            }
#endif
        }

        //////////////////////////////////////////////////////////////////
        //초기화 관련 함수
        //////////////////////////////////////////////////////////////////
        //텍스쳐를 로딩한다
        public void loadFielder(bool bTopInning)
        {            
            int index = bTopInning ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex;

            int skin = (int)pFielder.getSkin();
            if (posIndex == 1) skin = 1; //포수 예외처리

            if (anim != null)
            {
                Destroy(anim.gameObject);
            }

            /*
            if (field.tempClone[skin - 1] == null)
            {
                field.tempClone[skin - 1] = Util.Load("MainGame/prefabs/skeleton/fielder/fielderSkelPrefab_" + skin, field.transform, new Vector3(10000, 10000, 10000), "temp");
            }*/
            //GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/fielder/fielderSkelPrefab_" + skin, transform, new Vector3(0, 0, -0.01f), "skeleton");
            GameObject skeleton = GameObject.Instantiate(field.tempClone[skin-1], Vector3.zero, Quaternion.identity) as GameObject;
            skeleton.transform.parent = transform;
            skeleton.transform.localPosition = new Vector3(0, 0, -0.01f);
            skeleton.name = "skeleton";
            skeleton.transform.localScale = new Vector3(100, 100, 100);
            skeleton.transform.localEulerAngles = Vector3.zero;
            anim = skeleton.GetComponent<SkeletonAnimation>();

            fielderName.gameObject.SetActive(true);
            shadow.gameObject.SetActive(true);
            anim.gameObject.SetActive(true);


            anim.state.Event += HandleEvent;

#if GIRL_PLAY       //지워지워

#else
            if (skin == 1)
            {                
                if (field.bFielderTextureInit1 == false) 
                {
                    ////Debug.Log("=================>> 황인 텍스쳐 path = " +("MainGame/spineData/fieldingview/fielder/team/" + index + "/fielderAnim3"));
                    AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
                    Material[] materials = atlasdata.materials;
                    materials[1].mainTexture = (Texture)Resources.Load("MainGame/spineData/fieldingview/fielder/face/" + index + "/1/fielderAnim2");                    
                    materials[2].mainTexture = (Texture)Resources.Load("MainGame/spineData/fieldingview/fielder/team/" + index + "/fielderAnim3");
                    field.bFielderTextureInit1 = true;
                }
            }
            else if (skin == 2)
            {                
                if (field.bFielderTextureInit2 == false)
                {
                    ////Debug.Log("=================>> 백인 텍스쳐");
                    AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
                    Material[] materials = atlasdata.materials;
                    materials[1].mainTexture = (Texture)Resources.Load("MainGame/spineData/fieldingview/fielder/face/" + index + "/2/fielderAnim2");
                    materials[2].mainTexture = (Texture)Resources.Load("MainGame/spineData/fieldingview/fielder/team/" + index + "/fielderAnim3");
                    field.bFielderTextureInit2 = true;
                }
            }
            else //if (skin == 2)
            {                
                if (field.bFielderTextureInit3 == false)
                {
                    ////Debug.Log("=================>> 흑인 텍스쳐");
                    AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
                    Material[] materials = atlasdata.materials;
                    materials[1].mainTexture = (Texture)Resources.Load("MainGame/spineData/fieldingview/fielder/face/" + index + "/3/fielderAnim2");
                    materials[2].mainTexture = (Texture)Resources.Load("MainGame/spineData/fieldingview/fielder/team/" + index + "/fielderAnim3");
                    field.bFielderTextureInit3 = true;
                }
            }
#endif
        }

        //각종 플래그를 초기화 하고 initSkillActivate와 setAbility를 호출한다.
        public void initSetting(int index, bool positionInit)
        {
            bObjectInit = true;

            bBlending = false;
            bThrowBlending = false;
            bThrowAnimFlag = false;
            noDelayAnim = false;

            track = 1;
            lastTrack = 1;
            curOffsetX = offsetX = 0;
            timeScaleRate = Field.INIT_TIME_SCALE;
            lastStrID = "null";
            //shadow.renderer.enabled = true;
            setCollider();//콜라이더 초기화
#if _OrthoCamera
            transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            anim.transform.localScale = new Vector3(100, 100, 100);
            //shadow.scale = new Vector3(1.6f, 1, 1);
            shadow.color = new Color(1, 1, 1, 0.67f);
            transform.localEulerAngles = new Vector3(0, 0, 0);
#else
            transform.localScale = new Vector3(FieldParm.CHAR_SIZE_RATE, FieldParm.CHAR_SIZE_RATE, FieldParm.CHAR_SIZE_RATE);
            transform.localEulerAngles = new Vector3(CameraManager.FieldActiveAngleX, 0, 0);
            //shadow.scale = new Vector3(1.6f, 1, 1);
            //shadow.color = new Color(1, 1, 1, 0.67f);
#endif
            errorMark.gameObject.SetActive(false);
            
            //////////UnityEngine.//Debug.Log("===============setInit");
            actState = FielderAction._STANDBY;
            nextMove = NextAction._NONE;
            aStep = ActionStep._NONE;
            nFielderDir = FieldingMechanism._InitFielderDir[index];
            dashRatio = FieldingMechanism._InitDashRatio[index];
            posX = originX;
            posY = originY;
            //lineY = originY;
            //nFieldState = 0xff;
            curTime = 0;


            bSpecialAnim = false;

            bFlyCatchAvail = false;
            bGrounderAvail = false;
            bDeepFlyChase = false;
            //bHoldAnim = false;
            bFlyCatchTry = false;
            bGrounderTry = false;
            bBaseCovering = false;
            bBaseLeave = false;
            bRunnerArrivedOnBase = false;
            bTossTaking = false;
            bTossTaked = false;
            bThrowErrorCoverd = false;
            bOverHead = false;
            bPositionCovering = false;
            bRelayPositioning = false;
            bRelayStart = false;
            bFlyballCatch = false;
            bThrow = false;
            bFenceReady = false;
            bThrowAddDelay = false;
            bRoundingReady = false;
            bDashCatched = false;
            bVeryShortGrounder = false;
            bThrowAvailable = false;
            bForcedThrow = false;
            bThrowableChecked = false;
            throwAgainState = FieldParm.ThrowAgain.Available;   //재차 던질수 있음

            bDoublePlayAction = false;
            bLaserThrowCheck = false;
            bChaseColliderCheck = false;

            bNoCatchFlag = false;   //활성화시 공을 못잡음
            bDustOn = false;
            bSideChase = false;
            //특수캐치 관련 초기화
            //특수캐치 플라이볼
            flyballCatchType = FlyCatch.FLYCATCH_NORMAL;


            bStun = false;

            //특수캐치 그라운더
            grounderCatchType = GrounderCatch.GROUNDERCATCH_NORMAL;
            bDiveSuccess = false;

            throwSpeedRate = 1.0f;
            catchDelayRate = 1.0f;


            moveStep = 0;
            catchDir = -100;    //캐치방향 초기화

            //이차 움직임
            bSecondMove = false;

            //
            throwType = throwTypeFirstBase = ThrowType._NORMAL;
            specialThrowCatchDelayRate = 1000;
            throwReadyDelay = FieldingMechanism.DELAY_THROW_NORMAL;
            tagState = -1;

            bSecondDelayCover = false;

            bSpecialEnable = false;
            bDashQuickThrow = false;
            bFlySlowMove = false;
            bFlyFastMove = false;
            positionOffsetX = positionOffsetY = 0;
            dstOffsetX = dstOffsetY = 0;

            //
            bThrowErrorException = false;
            //능력치 세팅
            setAbility();

            bNoDirChange = false;
            bNoDirChange2 = false;
  
            //투수 딜레이 처리
            //if (posIndex == CPlayer._PITCHER) FIELD_DELAY = 1.0f;
            
            //스킬 발동 여부 
            initSkillActivate();
       

        }

        //스킬을 초기화 한다
        private void initSkillActivate()
        {
            skillRangeLevel = 0;
            skillJumpCatchLevel = skillSlidingCatchLevel = 0;
            skillSpinThrowLevel = skillQuickThrowLevel = skillJumpThrowLevel = skillDashThrowLevel = 0;
            skillDivingLevel = 0;

            specialCatchSuccess = false;
            pitcherReactSuccess = false;
            homerunStealActive = false;
            homerunstealSuccess = false;

            if (posIndex == CPlayer._PITCHER)
            {
                //투수 필드 스킬 
                int level = 0;  //1레벨로 제한                
                if (MyMath.Percent() < 30)
                {
                    //번트수비 체크
                    if (pFielder.fieldSkillSuccess(SkillIndex.PitcherBuntFielding) == true)
                    {
                        errorForceInit();
                        level = pFielder.getSkillRank(SkillIndex.PitcherBuntFielding) * 10;
                        skillQuickThrowLevel = skillDashThrowLevel = level;
                    }

                    //투수점프캐치 체크
                    if (pFielder.fieldSkillSuccess(SkillIndex.PitcherJumpCatch) == true)
                    {
                        errorForceInit();
                        level = 20 + (pFielder.getSkillRank(SkillIndex.PitcherJumpCatch) * 10);
                        skillJumpCatchLevel = level;
                        specialCatchSuccess = true;
                    }

                    //투수 반사신경 체크
                    if (pFielder.fieldSkillSuccess(SkillIndex.PitcherReaction) == true)
                    {
                        errorForceInit();
                        pitcherReactSuccess = true;
                        FIELD_DELAY = 0;
                    }
                }
                
            }
            else if (posIndex == CPlayer._CATCHER)
            {
                //포수 필드 스킬 
                
            }
            else if (posIndex >= CPlayer._FIRSTBASEMAN && posIndex <= CPlayer._SHORTSTOP)
            {
                //내야수 필드 스킬 
                int level = 0;
                //스페셜 캐치 체크                
                //if (pFielder.fieldSkillSuccess(SkillIndex.SpecialCatch) == true)
                if (pFielder.fieldSkillSuccessPVP(SkillIndex.SpecialCatch, field.manager.Pvp_spcatch) == true)
                {
                    if(field.manager.batterSkillFlag != SkillFlag.AssaultBall) errorForceInit();
                    level = 50 + 12;// (pFielder.getSkillRank(SkillIndex.SpecialCatch) * 6);
                    skillJumpCatchLevel = skillSlidingCatchLevel = level;
                    specialCatchSuccess = true;
                }

                //스페셜 송구 체크      
                ////Debug.Log("=============>>스페셜 송구 체크");
                //if (pFielder.fieldSkillSuccess(SkillIndex.SpecialThrow) == true)
                if (pFielder.fieldSkillSuccessPVP(SkillIndex.SpecialThrow, field.manager.Pvp_spthrow) == true)
                {
                    if (field.manager.batterSkillFlag != SkillFlag.AssaultBall) errorForceInit();
                    level = 10 + 20;// pFielder.getSkillRank(SkillIndex.SpecialThrow) * 10;
                    skillSpinThrowLevel = skillQuickThrowLevel = skillJumpThrowLevel = skillDashThrowLevel = level;                    
                }

                /* pvp 임시 제거
                //내야수 수비 반경
                if (pFielder.skillAvailable(SkillIndex.InfieldRange) == true) 
                {
                    level = 10 + 20;// pFielder.getSkillRank(SkillIndex.InfieldRange) * 10;
                    skillRangeLevel = level;
                }*/
            }
            else
            {
                int level = 0;

                //외야수 다이빙 캐치
                //if (pFielder.fieldSkillSuccess(SkillIndex.DivingCatch) == true) 
                if (pFielder.fieldSkillSuccessPVP(SkillIndex.DivingCatch, field.manager.Pvp_diving) == true)
                {
                    errorForceInit();
                    level = 50 + 12;// (pFielder.getSkillRank(SkillIndex.DivingCatch) * 6); 
                    skillDivingLevel = level;
                    specialCatchSuccess = true;
                }


                //외야수 홈런 스틸
                //if (pFielder.fieldSkillSuccess(SkillIndex.HomerunSteal) == true)
                if (pFielder.fieldSkillSuccessPVP(SkillIndex.HomerunSteal, field.manager.Pvp_hrsteal) == true)
                {
                    errorForceInit();
                    homerunStealActive = true;
                    homerunstealSuccess = true;
                }

                /* pvp 임시 제거
                //외야수 수비 반경
                if (pFielder.skillAvailable(SkillIndex.OutfieldRange) == true)
                {
                    level = 10 + 20;// pFielder.getSkillRank(SkillIndex.OutfieldRange) * 10;
                    skillRangeLevel = level;
                }*/

            }
        }

        //능력치를 초기화 한다
        public void setAbility()
        {
            //매피치마다 재설정
            int fieldBonus = pFielder.getFieldBonusValue();
            fieldingAbil = pFielder.getFielding() + fieldBonus;
            throwingAblil = pFielder.getThrowing() + fieldBonus;
            runningAbil = pFielder.getSpeed() + fieldBonus;
            if (runningAbil < 500) runningAbil = 500;

            ////UnityEngine.//Debug.Log("=====================>>야수 능력 매번 세팅????");
            //리와인드가 아닌경우 능력치별 배정
            FIELD_DELAY = FieldingMechanism.getFieldDelay(fieldingAbil);
            THROW_DELAY = FieldingMechanism.getThrowDelay();
            THROW_SPEED = FieldingMechanism.getThrowSpeed(throwingAblil);
            FIELDER_SPEED = FieldingMechanism.getFieldSpeed(runningAbil, fieldingAbil);
            
            THROW_WRIST = 1;
        }

        public void initInstance(BallPlayManager manager)
        {
            this.field = manager.field;
        }
                

        //파라메터 셋팅
        public void initParameter(CPlayer player, int position)
        {
            this.pFielder = player;
            //this.field = manager.field;

            lastActState = FielderAction._NOTHING_STATE;
            nLastDir = -100;
            posIndex = position;
            fielderName.text = player.getName();
            //nFielderName = "김우재";// player.m_strName;


            //미스매치 체크
            if (pFielder.getMissMatch() == true) Debug.Log("========================>>>> " + posIndex + " 포지션 미스매치 상태");
            
            int fieldBonus = pFielder.getFieldBonusValue();
            fieldingAbil = pFielder.getFielding() + fieldBonus;
            throwingAblil = pFielder.getThrowing() + fieldBonus;
            runningAbil = pFielder.getSpeed() + fieldBonus;
            if (runningAbil < 500) runningAbil = 500;


            //레벨은 작을 수록 우수
            //fieldLevel = FieldingMechanism.getFieldLevel(fieldingAbil);
            //throwLevel = FieldingMechanism.getFieldLevel(throwingAblil);
            //runningLevel = FieldingMechanism.getFieldLevel(runningAbil);

            //name.text = pFielder.getName(); 
        }
        
        //위치 초기화
        public void setInitPosition()
        {
            posX = field.getOriginX(FieldSize.getFielderPosX(posIndex));// *TILE_WIDTH;
            posY = field.getOriginY(FieldSize.getFielderPosY(posIndex));// *TILE_WIDTH;
            nFielderDir = FieldingMechanism._InitFielderDir[posIndex];
            nLastDir = -1;
        }


        //////////////////////////////////////////////////////////////////
        //기본적인 get set함수
        //////////////////////////////////////////////////////////////////
        
        //달리기 애니메이션 이름 가져오기
        private string getRunIndexStr()
        {
            if (posIndex == CPlayer._CATCHER) return FieldingMechanism._CATCHER_RUN;
            else return FieldingMechanism._RUN;
        }

        //센터캐치 애니메이션 이름 가져오기
        private string getCenterCatchStr()
        {
            string str = FieldingMechanism._GROUNDBALL_CENTER;
            int rand = MyMath.Percent();
            if (rand < 25) str = FieldingMechanism._GROUNDBALL_CENTER10;
            else if (rand < 50) str = FieldingMechanism._GROUNDBALL_CENTER20;

            return str;
        }

        //홀드 애니메이션 이름 가져오기
        private string getHoldIndexStr()
        {
            if (posIndex == CPlayer._CATCHER)
            {
                if (nFielderDir >= FieldParm._EAST && nFielderDir <= FieldParm._WEST)
                {
                    nFielderDir = FieldParm._NORTH;
                }                
                return FieldingMechanism._CATCHER_HOLD;
            }
            else return FieldingMechanism._HOLD;
        }
        
        //posX를 리턴
        public float getX()
        {
            return posX;
        }

        //posY를 리턴
        public float getY()
        {
            return posY;
        }

        //nFielderDir를 리턴
        public int getFielderDir()
        {
            return nFielderDir;
        }

        //필딩값 얻어오기
        public int getFieldPower()
        {
            return fieldingAbil;
        }

        //송구값 얻어오기
        public int getThrowingPower()
        {
            return throwingAblil;
        }


        //////////////////////////////////////////////////////////////////
        //핵심 수비 로직
        //1. 땅볼 수비 로직
        /////////////////////////////////////////////////////////////////
        //목적지까지 걸리는 시간
        public float getRemainTime(float dstX, float dstY)
        {
            //해당 거리까지 남은 시간
            float time = FieldingMechanism.getDistance(posX, dstX, posY, dstY) / FIELDER_SPEED; //getDistance(posX, dstX, posY, dstY) / FIELDER_SPEED;

            return time + 0.1f; //0.1초의 옵셋을 붙여줌
        }
         
        /*
        //타구별 바운드에 의해 감소되는 스피드를 비율로 환산하여 땅볼수비에 참조함
        private float getSpeedRatio(float distance)
        {            
            float spdRatio = 1.0f;

            if (field.ball.firstAngleZ <= -50) return 0.5f; //찹퍼 처리

            float dz = (field.ball.nBallDZ - 500);
            if (dz < 0)
            {
                spdRatio = (dz * (field.ball.firstBoundDistance / distance)) / -100.0f;
                if (spdRatio > 1) spdRatio = 1;
                else if (spdRatio < 0.4f) spdRatio = 0.4f;
            }
            if (posIndex == CPlayer._SECONDBASEMAN || posIndex == CPlayer._SHORTSTOP)
            {
                if (field.ballPower < 15) spdRatio *= 0.6f;
                else if (field.ballPower < 20) spdRatio *= 0.8f;
                if (spdRatio < 0.5f) spdRatio = 0.5f;
            }
            else if (posIndex >= CPlayer._LEFTFIELDER)
            {
                if (field.ball.angleZ > 35)
                {
                    if (field.ball.nFirstBoundY < (posY - 100))
                    {
                        spdRatio = 1 + (25.0f - field.ball.angleZ) / 45.0f;
                        if (spdRatio < 0.4f) spdRatio = 0.4f;
                    }
                    else
                    {
                        spdRatio = 0.7f;
                    }
                }
                else
                {
                    spdRatio = 0.8f;
                }
            }
            return spdRatio;
        }*/

   
        //땅볼수비의 핵심 함수
        //땅볼을 처리할 수 있는지 여부를 결정
        //땅볼관련 스페셜 캐치를 할수 있는지를 결정
        //땅볼을 잡는 목적좌표를 구함
        public bool getGrounderDstPos()
        {
#if NO_FIELDING
        return false;
#else
            if (bOverHead == true) return false;
            float angle = field.ball.firstAngle;
            float speed = field.ball.firstSpeed; //field.ball.speed;

            if (speed == 0) return false;


            float dx = field.ball.nBallDX;
            float dy = field.ball.nBallDY;

            float px = posX;
            float py = posY;

            float hx = field.homeX;// field.getOriginX(FieldSize.getHomePosX());
            float hy = field.homeY;// field.getOriginY(FieldSize.getHomePosY(Field.InitRatio));


            float a = (dx == 0 ? a = 0 : dy / dx);
            float b = hy - a * hx;

            float dsty = py;
            float dstx = (a == 0 ? a : (py - b) / a);


            tryDstX = dstx;
            tryDstY = dsty;

            if (angle > 0) //좌측 방향
            {
                if (FieldingMechanism.checkRightCornerFielder(posIndex) == true)
                {
                    return false;
                }
            }
            else           //우측 방향
            {
                if (posIndex == CPlayer._FIRSTBASEMAN && angle >= FieldingMechanism.FIRSTBASE_COVER_LIMIT_ANGLE && field.bBuntFielding == false)
                {
                    return false; //1루수 예외
                }

                if (FieldingMechanism.checkLeftCornerFielder(posIndex) == true)
                {
                    return false;
                }
            }

            if (posIndex >= CPlayer._LEFTFIELDER)
            {
                //외야수 땅볼 수비로 전환
                return getGrounderDstPosOutFielder(px, py, hx, hy);
            }

            if (field.bBuntFielding == true)
            {
                if (posIndex == CPlayer._SHORTSTOP || posIndex == CPlayer._SECONDBASEMAN)
                {
                    return false;
                }
            }

            float timeH = 0;// field.ball.firstBoundTime + FieldingMechanism.getGroundTime(field.ball.nFirstBoundX, dstx, field.ball.nFirstBoundY, dsty, speed);
            float timeF = 0;// FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);

            if(Mode.bPvpMode433 == true)
            {
                if(field.manager.bMyTurn == true)
                {
                    timeH = field.ball.firstBoundTime + FieldingMechanism.getGroundTime(field.ball.nFirstBoundX, dstx, field.ball.nFirstBoundY, dsty, speed);
                    timeF = FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);
                    field.manager.Pvp_GroundTimeH[posIndex] = timeH;
                    field.manager.Pvp_GroundTimeF[posIndex] = timeF;                    
                }
                else
                {
                    timeH = field.manager.Pvp_GroundTimeH[posIndex];
                    timeF = field.manager.Pvp_GroundTimeF[posIndex];
                }
            }
            else
            {
                timeH = field.ball.firstBoundTime + FieldingMechanism.getGroundTime(field.ball.nFirstBoundX, dstx, field.ball.nFirstBoundY, dsty, speed);
                timeF = FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);
            }


            grounderRemainTime = timeF;    //수비 성공 실패 관계없이         
            grounderDistance = Mathf.Abs(dstx - px);

            catchDelayRate = 1.0f;
            throwDelayRate = 1.0f;

            
            //int count = 0;
            if (timeF < timeH)//if (frameF < frameH)
            {
                float dsty2 = dsty;
                float dstx2 = dstx;
                float timeH2 = timeH;  //int frameH2 = frameH;
                float timeF2 = timeF;  //int frameF2 = frameF;
                                                
                float grounderRatio = timeF / timeH;
                
                //0.7, 0.55

                if (field.ballPower < 22 && field.ball.firstAngleZ < 0)
                {
                    if (grounderRatio < dashRatio)
                    {
                        ////Debug.Log("===================>>대쉬수비로 바꿔");
                        FIELD_DELAY = 0;
                        grounderCatchType = GrounderCatch.GROUNDERCATCH_DASH_FIRST;
                        grounderRemainTime = timeH2;   
                        field.setFastFieldTime(timeH2);
                        bGrounderAvail = true;
                        return bGrounderAvail;
                    }
                }


                if (grounderRatio < 0.85f || field.ballPower < 20 || posIndex == CPlayer._FIRSTBASEMAN)
                {
                    grounderCatchType = GrounderCatch.GROUNDERCATCH_NORMAL;

                    if (posIndex >= CPlayer._FIRSTBASEMAN && posIndex <= CPlayer._SHORTSTOP)
                    {
                        field.battingviewFieidingType = FieldParm.BattingViewFieldingType.NormalGrounder;
                    }
                }
                else
                {
                    grounderCatchType = GrounderCatch.GROUNDERCATCH_MOVING_NORMAL;
                }



                throwDelayRate = 0.1f;  // 쓰로우 딜레이가 없다.

                bDiveSuccess = true;
                bGrounderAvail = true;
                dstX = dstx;
                dstY = dsty;

                field.groundingDstX = dstX;
                field.groundingDstY = dstY;

                remainTime = timeF - FIELD_DELAY;
                distanceToBall = FieldingMechanism.getDistance(posX, dstX, posY, dstY);// getDistance(dstX, dstY);

                field.setFastFieldTime(timeH2);

                if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING_NORMAL)
                {
                    remainTime *= 1.3f;
                    float s = (distanceToBall / remainTime);
                    if (s > FIELDER_SPEED) s = FIELDER_SPEED;
                    specialTimeGabX = (timeF - timeH) * s;
                    field.bGrounderSpecial = true;
                    bSpecialEnable = true;
                    setCollider(grounderCatchType);
                }
            }
            else
            {
#if !NO_SPECIAL_MOVE
                if (posIndex < CPlayer._LEFTFIELDER && posIndex > CPlayer._CATCHER &&
                    field.firstBallSpeed > FieldingMechanism.SPECIAL_GROUNDER_BALLSPEED)
                {
                    //float range = 1.05f + FieldingMechanism.getCornerInfielderRangeOffset(posIndex, field.ballPower, field.ball.firstAngle);
                    float range = FieldingMechanism.SPECIAL_GROUNDER_MIN_VALUE + FieldingMechanism.getCornerInfielderRangeOffset(posIndex, field.ballPower, field.ball.firstAngle);
                    if (skillRangeLevel > 0) 
                    {
                        range += SkillParm.getInfieldRangeOffset(skillRangeLevel); //최대 1.45
                    }

                    //float divingRange = (range + 0.1f);
                    float divingRange = (range + FieldingMechanism.SLIDING_CATCH_OFFSET);
                    if (skillSlidingCatchLevel > 0) //다이빙 캐치 레인지세팅
                    {
                        divingRange += SkillParm.getSlidingRangeOffset(skillSlidingCatchLevel);
                    }

                    float addRangeFore, addRangeBack;
                    addRangeFore = SkillParm.getAddRangeFore(posX, dstx, skillSpinThrowLevel);
                    addRangeBack = SkillParm.getAddRangeBack(posX, dstx, skillJumpThrowLevel);
                    range += (addRangeFore + addRangeBack);

                    if (range > 1.45f) range = 1.45f;
                    float grounderRatio = (timeF / timeH);


                    float limit = range + (posIndex == CPlayer._SECONDBASEMAN || posIndex == CPlayer._SHORTSTOP ? 1.0f : 0.5f);

                    if (grounderRatio < limit)
                    {
                        //그라운더 트라이 할 수 있는지 여부 판단
                        if (posIndex < CPlayer._LEFTFIELDER)
                        {
                            //Debug.Log("posIndex : " + posIndex + "====>>헛글러브질 모션");
                            dstX = dstx;
                            dstY = dsty;
                            bGrounderTry = true;
                            //트라이트라이 -> 트라이 디펜스 제거
                            /*if (MyMath.Percent() < FieldingMechanism.TEAM_DEFENSE) //체크체크
                            {
                                //팀디펜스의 확률로 try시공 잡을 확률 늘어남
                                setCollider(GrounderCatch.GROUNDERCATCH_TRY);
                            }
                            else*/
                            {
                                setNoCatchCollider();
                            }
                            
                        }
                    }

                    //포핸드, 백핸드 캐치
                    if (grounderRatio < range)//1.38f)
                    {
                        field.setFastFieldTime(timeH);
                        bGrounderAvail = true;
                        dstX = dstx;
                        dstY = dsty;

                        field.groundingDstX = dstX;
                        field.groundingDstY = dstY;

                        throwDelayRate = 1.0f;


                        grounderCatchType = GrounderCatch.GROUNDERCATCH_MOVING;

                        if (posIndex >= CPlayer._SECONDBASEMAN && posIndex <= CPlayer._SHORTSTOP)
                        {
                            if ( (skillSpinThrowLevel > 0 && (posX < dstx))
                             ||  (skillJumpThrowLevel > 0 && (posX > dstx)))  //스핀 스로 혹은 점핑 쓰로가 있는 경우
                            {
                                //UnityEngine.Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>> 1");
                                field.bSpecialMoveActivte = true;////스핀 점핑 미리계산 미리 슬로우

                            }
                            bSpecialEnable = true;
                        }

                        setCollider(grounderCatchType);

                        bDiveSuccess = true;
                        remainTime = (timeH - FIELD_DELAY);// *1.5f;
                        remainTime *= 1.3f;
                        distanceToBall = FieldingMechanism.getDistance(posX, dstX, posY, dstY);// getDistance(dstX, dstY);

                        specialTimeGabX = (timeF - timeH) * FIELDER_SPEED * 0.5f;

                        if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING)
                        {
                            dstOffsetX = (nFielderDir == FieldParm._EAST ? (FieldingMechanism.MOVING_GROUNDER_OFFSET_FORE + specialTimeGabX) : (FieldingMechanism.MOVING_GROUNDER_OFFSET_BACK - specialTimeGabX)); //옵셋                                
                        }
                        else
                        {
                            dstOffsetX = 0;
                        }
                        dstOffsetY = 0;
                        posX += dstOffsetX;
                        field.bGrounderSpecial = true;
                        bGrounderTry = false;
                        return bGrounderAvail;
                    }
                    //슬라이딩 캐치
                    else if (grounderRatio < divingRange) 
                    {
                        if (skillSlidingCatchLevel > 0)  
                        {
                            ////UnityEngine.//Debug.Log("======================>> 내야수 다이빙 가능");                                
                            field.setFastFieldTime(timeH);                            
                            dstX = dstx;
                            dstY = dsty;
                            field.groundingDstX = dstX;
                            field.groundingDstY = dstY;
                            grounderCatchType = GrounderCatch.GROUNDERCATCH_DIVING;

                            throwDelayRate = 1.0f;
                            bGrounderAvail = true;

                            if(specialCatchSuccess == true)
                            {
                                ////UnityEngine.//Debug.Log("======================>> 내야수 스페셜 캐치 성공");                                
                                setCollider(grounderCatchType);
                                bDiveSuccess = true;
                            }
                            else
                            {
                                ////UnityEngine.//Debug.Log("======================>> 내야수 다이빙 실패");                                
                                setCollider();// setNoCatchCollider();
                                bDiveSuccess = false;
                            }
                            //UnityEngine.Debug.Log("specialIndex ================>> " + posIndex);
                            //field.bSpecialGrounderMoveOn = true;

                            remainTime = (timeH - FIELD_DELAY);// *1.3f;
                            remainTime *= 1.3f;

                            distanceToBall = FieldingMechanism.getDistance(posX, dstX, posY, dstY);// getDistance(dstX, dstY);

                            specialTimeGabX = (timeF - timeH) * FIELDER_SPEED;// *0.5f;

                            //posX += (posX < dstX ? (FieldingMechanism.DIVING_GROUNDER_OFFSET_FORE + specialTimeGabX) : (FieldingMechanism.DIVING_GROUNDER_OFFSET_BACK - specialTimeGabX)); //옵셋

                            dstOffsetY = 0;

                            if (bDiveSuccess == true)
                            {
                                dstOffsetX = -(posX < dstX ? (FieldingMechanism.DIVING_GROUNDER_OFFSET_FORE + specialTimeGabX) : (FieldingMechanism.DIVING_GROUNDER_OFFSET_BACK - specialTimeGabX));
                                posX -= dstOffsetX; //위치 옵셋
                                field.bGrounderSpecial = true;
                                bSpecialEnable = true;
                                field.bSpecialMoveActivte = true;
                            }
                            else
                            {
                                dstOffsetX = 0;
                            }

                            //field.setFieldSkill(FieldSkillType.INFIELD_SLIDING, posIndex, "슬라이딩 캐치");
                            bGrounderTry = false;
                            return bGrounderAvail;
                        }
                    }
                }
#endif
                bGrounderAvail = false;
            }
            return bGrounderAvail;
#endif            
        }

        //외야수의 땅볼수비
        private bool getGrounderDstPosOutFielder(float px, float py, float hx, float hy)
        {
            bool bDestCheck = false;
            bool bCornerDefense = false;
            bool bRecheck = false;

            float a = (field.ball.nFirstBoundY - hy) / (field.ball.nFirstBoundX - hx);
            float b = hy - (a * hx);

            float dstx = 0, dsty = 0;
            float distance;
            float timeF = 0,timeH = 0;
            float boundBallDistance;

            float speedBoundFirst = field.ball.speed * 0.7f;
            

            if (a != 0)
            {
                float _dstX = (posY - b) / a;
                if (posIndex == CPlayer._LEFTFIELDER)
                {
                    if (_dstX < posX) bCornerDefense = true;
                }
                else if (posIndex == CPlayer._RIGHTFIELDER)
                {
                    if (_dstX > posX) bCornerDefense = true;
                }
                else
                {
                    bCornerDefense = true;
                }
            }

            if (bCornerDefense == true)
            {
                float positionAngle = Mathf.Atan2(posY - hy, posX - hx);// * Mathf.Rad2Deg;
                float boundAngle = Mathf.Atan2(field.ball.nFirstBoundY - hy, field.ball.nFirstBoundX - hx);// * Mathf.Rad2Deg;
                float angleGab = (positionAngle - boundAngle);// * Mathf.Deg2Rad;

                float myDistance = FieldingMechanism.getDistance(hx, posX, hy, posY);
                float idealDistance = Mathf.Abs(myDistance * Mathf.Cos(angleGab));
                float firstboundDistance = FieldingMechanism.getDistance(hx, field.ball.nFirstBoundX, hy, field.ball.nFirstBoundY);

                if (firstboundDistance < idealDistance)
                {
                    ////UnityEngine.//Debug.Log("======================>> 이곳에 들어가는가?");
                    dstx = idealDistance * Mathf.Cos(boundAngle) + hx;
                    dsty = idealDistance * Mathf.Sin(boundAngle) + hy;
                    float gab = Mathf.Abs((posY-dsty)/10);
                    float fSpeed = FIELDER_SPEED;
                    int count = 0;
                    while(true)
                    {
                        distance = FieldingMechanism.getDistance(hx, dstx, hy, dsty);
                        boundBallDistance = distance - field.ball.firstBoundDistance;                        
                        if (count >= 10 || boundBallDistance < 0)
                        {
                            ////UnityEngine.//Debug.Log("=================================>>>답안나옴 = " + posIndex);
                            bDestCheck = true;
                            dsty = py;
                            dstx = (a == 0 ? a : (py - b) / a);
                            break;
                        }
                        if (Mode.bPvpMode433 == true)
                        {
                            if (field.manager.bMyTurn == true)
                            {
                                timeF = FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);
                                timeH = field.ball.firstBoundTime + (boundBallDistance / speedBoundFirst);
                                field.manager.Pvp_GroundTimeF[posIndex] = timeF;
                                field.manager.Pvp_GroundTimeH[posIndex] = timeH;
                            }
                            else
                            {
                                timeF = field.manager.Pvp_GroundTimeF[posIndex];
                                timeH = field.manager.Pvp_GroundTimeH[posIndex];                                
                            }

                        }
                        else
                        {
                            timeF = FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);
                            timeH = field.ball.firstBoundTime + (boundBallDistance / speedBoundFirst);
                        }

                        if (timeF < timeH)
                        {                            
                            ////UnityEngine.//Debug.Log("=================================>>>답나옴 = " +posIndex);
                            bDestCheck = true;
                            break;
                        }
                        dsty += gab;
                        dstx = (a == 0 ? a : (dsty - b) / a);
                        fSpeed *= 1.1f;
                        count++;
                    }                    
                }
                else
                {
                    ////UnityEngine.//Debug.Log("======================>> 코너 디펜스가 아닌 경우로 재검색?");
                    bRecheck = true;
                }
            }


            if (bCornerDefense == false || bRecheck == true)
            {
                //  //UnityEngine.//Debug.Log("======================>> 코너 디펜스가 아닌 경우");
                dsty = py;
                dstx = (a == 0 ? a : (py - b) / a);
                distance = FieldingMechanism.getDistance(hx, dstx, hy, dsty);
                boundBallDistance = distance - field.ball.firstBoundDistance;
                if (boundBallDistance >= 0)
                {
                    if (Mode.bPvpMode433 == true)
                    {
                        if (field.manager.bMyTurn == true)
                        {
                            timeF = FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);
                            timeH = field.ball.firstBoundTime + (boundBallDistance / speedBoundFirst);
                            field.manager.Pvp_GroundTimeF[posIndex] = timeF;
                            field.manager.Pvp_GroundTimeH[posIndex] = timeH;
                        }
                        else
                        {
                            timeF = field.manager.Pvp_GroundTimeF[posIndex];
                            timeH = field.manager.Pvp_GroundTimeH[posIndex];
                        }
                    }
                    else
                    {
                        timeF = FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);
                        timeH = field.ball.firstBoundTime + (boundBallDistance / speedBoundFirst);
                    }

                    if (timeF < timeH)
                    {                       
                        bDestCheck = true;
                    }
                }
            }


            if (bDestCheck == true)
            {
                grounderCatchType = GrounderCatch.GROUNDERCATCH_NORMAL;
                throwDelayRate = 0.1f;  // 쓰로우 딜레이가 없다.

                bDiveSuccess = true;
                bGrounderAvail = true;
                dstX = dstx;
                dstY = dsty;

                remainTime = timeF - FIELD_DELAY;
                distanceToBall = FieldingMechanism.getDistance(posX, dstX, posY, dstY);

                if (Mathf.Abs(posX - dstX) > 300)
                {
                    if (bCornerDefense == true && posIndex != CPlayer._CENTERFIELDER)
                    {
                        ////Debug.Log("===============================>>외야 코너 땅볼처리 timeH = " + timeH);
                        StartCoroutine(setCornerGrounder(timeH * 0.8f, dstX));
                    }
                }

                return true;
            }
            
            getGrounderReverseSlope();
            return false;
        }


        private IEnumerator setCornerGrounder(float delay, float dstX)
        {
            //Debug.Log("이전 ==============>> gab = " + (posX - dstX));
            yield return new WaitForSeconds(delay);
            //Debug.Log("이후 ==============>> gab = " + (posX - dstX));
            if (Mathf.Abs(posX - dstX) > 150)
            {                
                setBallChase(false);
                nextDashTime = 1.5f;// delay * 3;
            }
        }

        //외야수가 땅볼을 잡을 수 없는경우 리버스 슬로프로 계산하여 땅볼처리
        public void getGrounderReverseSlope()
        {
            //Debug.Log("===========================>>getGrounderReverseSlope posIndex :: "+posIndex);
            if(field.ball.firstAngle > 0)
            {
                if(posIndex == CPlayer._RIGHTFIELDER) return;
            }
            else
            {
                if(posIndex == CPlayer._LEFTFIELDER) return;
            }


            if (bDeepFlyChase == true) return;

            float speed = field.ball.speed;
            float dx = field.ball.nBallDX;
            float dy = field.ball.nBallDY;

            float px = posX;
            float py = posY;

            float hx = field.homeX;// field.getOriginX(FieldSize.getHomePosX());
            float hy = field.homeY;// field.getOriginY(FieldSize.getHomePosY(Field.InitRatio));

            float a = dy / dx;
            float b = hy - a * hx;

            float dsty = py;
            float dstx = (py - b) / a;

            float timeH = FieldingMechanism.getTime(hx, hy, dstx, dsty, speed, 0);
            float timeF = FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);

            float maxSlope = 0.4f;
            if (posIndex == CPlayer._LEFTFIELDER)
            {
                if (px > dstx)
                {
                    maxSlope = 0;
                    field.fielder[CPlayer._CENTERFIELDER].bGrounderAvail = false;
                }
                else maxSlope = 1.2f;
            }
            else if (posIndex == CPlayer._RIGHTFIELDER)
            {
                if (px > dstx) maxSlope = 1.2f;
                else
                {
                    maxSlope = 0;
                    field.fielder[CPlayer._CENTERFIELDER].bGrounderAvail = false;
                }
            }

            ////////UnityEngine.//Debug.Log("===============>>posIndex = " + posIndex + "   maxSlope = " + maxSlope);
            int count = 0; //테스트용

            if (timeF > timeH)//if (frameF > frameH)
            {
                while (true)
                {
                    dsty += 2 * (dy * deltaTime);
                    dstx = (dsty - b) / a;

                    timeH = FieldingMechanism.getTime(hx, hy, dstx, dsty, speed, 0);
                    timeF = FieldingMechanism.getTime(px, py, dstx, dsty, FIELDER_SPEED, FIELD_DELAY);
                    //speed -= (2.0f * Field._TIME_CAL); //도데체 이유를 모르겠음
                    if (timeF <= timeH || ++count> 10)
                    {
                        break; 
                    }
                    else
                    {
                        float slope = Mathf.Abs(dsty - py) / Mathf.Abs(dstx - px);
                        if (slope >= maxSlope)
                        {
                            break;
                        }
                    }
                }

                ////////UnityEngine.//Debug.Log("===============>>posIndex = "+posIndex+"   count = "+count);

                bGrounderAvail = true;
                dstX = dstx;
                dstY = dsty;
                remainTime = timeF - FIELD_DELAY;
                distanceToBall = FieldingMechanism.getDistance(posX, dstX, posY, dstY);// getDistance(dstX, dstY);

                ////////UnityEngine.//Debug.Log("===============>>remainTime = " + remainTime);
                ////UnityEngine.Debug.Log("REVERSE SLOPE posIndex = " + posIndex + "  =======>>dstX = " + dstX + "  =======>>dstY = " + dstY);
            } //dx speed dy
        }

        /////////////////////////////////////////////////////////////////
        //3. 딜레이 관련 수비 로직
        /////////////////////////////////////////////////////////////////
        //능력치와 AI 어그레시브에 따른 딜레이 산출
        public float getThrowingDelay(int baseIndex, bool bOutFielderDelayOffset, float runnerSpeed = -1.0f)
        {
            float throwDelay = THROW_DELAY;            
            if (field.run.bForceOutFlag[baseIndex] == false)
            {
                throwDelay += FieldingMechanism.DELAY_TAGGING;
            }
            throwDelay += 0.1f; //던지는데 걸리는 물리적인 시간 더해줌
            if (bOutFielderDelayOffset == true)
            {
                if (posIndex >= CPlayer._LEFTFIELDER)
                {
                    //외야수의 어그레시브에 따른
                    throwDelay -= 0.75f;    //노멀 
                    //throwDelay -= 1.5f;   //어그레시브
                    //throwDelay -= 2.0f;   //베리 어그레시브
                }
            }
            if (runnerSpeed > 500)
            {
                if (posIndex < CPlayer._LEFTFIELDER)
                {
                    //내야수인 경우
                    throwDelay += ((runnerSpeed - 500) / 3000.0f);
                }
            }
            return throwDelay;
        }

        //캐칭 타입에 따른 딜레이값을 산출
        public float getCatchingDelay(bool bFly)
        {
            float delay = 0.1f;

            if (bFly == true)
            {
                if (flyballCatchType == FlyCatch.FLYCATCH_SLOWMOVE)
                {
                    delay += FieldingMechanism.TIME_SLOWMOVE_END;
                }
                else if (flyballCatchType == FlyCatch.FLYCATCH_FULLSPEED)
                {
                    delay += FieldingMechanism.TIME_RUNNING_END;
                }
                else if (flyballCatchType == FlyCatch.FLYCATCH_DIVING)
                {
                    delay += FieldingMechanism.TIME_SLIDING_END;
                }
            }
            else
            {
                if (posIndex >= CPlayer._LEFTFIELDER)
                {
                    //어떤 캐칭이냐에 따라.. 좀더 복잡하게
                    if (bOverHead || field.ball.bFenceCol)
                    {
                        delay += 0.7f;
                    }
                    else
                    {
                        delay += 0.2f;
                    }

                    if (bGrounderAvail == false)
                    {
                        ////////UnityEngine.//Debug.Log("================>>@@@@@@@@@@@@@@@@여기체크@@@@@@@@@!!!");
                        //delay += 2.0f;
                        //delay += 1.0f;
                    }

                }
            }

            return delay;
        }

        /////////////////////////////////////////////////////////////////
        //4. 송구 관련 수비 로직
        /////////////////////////////////////////////////////////////////
        //기본 송구 세팅
        float dstBaseX, dstBaseY;
        float dstOffsetX, dstOffsetY;
        float positionOffsetX, positionOffsetY;

        public void setThrow(int coverIndex, bool bLaser = false, bool bRecheck = false)
        {
            if (throwAgainState != FieldParm.ThrowAgain.Available)
            {
                //재차 던질수 없는 상황인경우 setThrow를 생략
                return;
            }

            if (field.bThrowing == true ||
                field.nThrowIndex == coverIndex)
            {
                setStop();
                return;
            }

            ////UnityEngine.//Debug.Log("===================>>setThrow");
            //각종 플래그 초기화
            field.bCollisionFlag = false;   //충돌 무효
            field.bCrushDelay = false;
            bThrowableChecked = false;
            bThrowAvailable = false;
            bDashCatched = bDashCatchTry = false;
            bThrowAddDelay = false;
            //field.bFieldEffectActive = false;
            field.bFieldPerspectiveZoom = false;
            field.bThrowBallCatched = false;
            field.bThrowZoom = false;
            field.bTossThrow = false;
            bool bToss = false;

            //물리 초기화
            float delayTime = 0;
            float throwSpeed = THROW_SPEED * throwSpeedRate;

            if (bLaser == true)
            {
                //레이저 특능 발동
                throwSpeed = SkillParm.getLaserThrow();//  SkillParm.getLaserThrow(sLaserActive);
            }

            if (posIndex >= CPlayer._FIRSTBASEMAN && posIndex <= CPlayer._SHORTSTOP)
            {
                if (field.runnerTurbo == FieldSkillUse.Active)
                {
                    checkRunnerTurbo(throwSpeed, 0.01f, 2.0f);
                }
                else
                {
                    checkQuickThrow();
                }
            }

            //임시
            /*if (field.nTargetIndex == FieldParm.SECONDBASE_INDEX)
            {
                field.run.getDestRunner(FieldParm.SECONDBASE_INDEX).setDoublePlayStopOn();
            }*/

            //


            float wrist = THROW_WRIST;
            //송구의 정확도 개념
            float curTaggingDelay = FieldingMechanism.getTaggingDelay(fieldingAbil, throwingAblil);

            if (field.nTargetIndex == FieldParm.RELAY_INDEX)    //던지는 타겟이 릴레이 인경우
            {
                //릴레이 인경우의             
                field.nRelayFielderIndex = field.getRelayIndex();
                dstBaseX = field.fielder[field.nRelayFielderIndex].posX;
                dstBaseY = field.fielder[field.nRelayFielderIndex].posY;
                field.ball.setThrowingCamera(dstBaseX, dstBaseY);

                //////UnityEngine.//Debug.Log("=======>>DIS = " + FieldingMechanism.getDistance(posX, dstBaseX, posY, dstBaseY));
                if (FieldingMechanism.getDistance(posX, dstBaseX, posY, dstBaseY) < FieldingMechanism.RANGE_MINIMUM_RELAY)
                {
                    //너무 가까운 경우 2루로
                    field.fielder[field.nRelayFielderIndex].setBackupPosition(0, 0, true);                    
                    coverIndex = (field.nRelayFielderIndex == CPlayer._SHORTSTOP ? CPlayer._SECONDBASEMAN : CPlayer._SHORTSTOP);

                    int target2 = FieldParm.SECONDBASE_INDEX;

                    Runner thirdRunner = field.run.getDestRunner(FieldParm.THIRDBASE_INDEX);
                    if (thirdRunner != null)
                    {
                        if (thirdRunner.state == RunState.MOVE && thirdRunner.basePositionRate() < 0.7f)
                        {
                            target2 = FieldParm.THIRDBASE_INDEX;
                            coverIndex = CPlayer._THIRDBASEMAN;
                        }
                    }

                    Runner homeRunner = field.run.getDestRunner(FieldParm.HOMEBASE_INDEX);
                    if (homeRunner != null)
                    {
                        if (homeRunner.state == RunState.MOVE && homeRunner.basePositionRate() < 0.6f)
                        {
                            target2 = FieldParm.HOMEBASE_INDEX;
                            coverIndex = CPlayer._CATCHER;
                        }
                    }
                    

                    field.nTargetIndex = target2;
                }
                else
                {
                    field.bThrowing = true;
                    field.nFirstThrower = posIndex;
                    field.fielder[field.nRelayFielderIndex].bRelayPositioning = true;
                    field.bRelaying = true;
                    //if (field.fielder[field.nRelayFielderIndex].actState != FielderAction._STANDBY)
                    {
                        //타겟의 현재 상태를 스탑해버린다.
                        field.fielder[field.nRelayFielderIndex].setStop();
                    }
                }
            }

            if (field.nTargetIndex != FieldParm.RELAY_INDEX)    //던지는 타겟이 릴레가 아닌 경우
            {
                //베이스로 던지는 경우

                //각종 플래그 초기화
                field.bThrowZoom = true;
                field.bThrowing = true;
                field.nFirstThrower = posIndex;

                //목표 베이스의 좌표를 구한다
                dstBaseX = field.getOriginX(FieldSize.getBasePosX(field.nTargetIndex)) +FieldingMechanism.baseOffset[field.nTargetIndex, 0]; 
                dstBaseY = field.getOriginY(FieldSize.getBasePosY(field.nTargetIndex)) +FieldingMechanism.baseOffset[field.nTargetIndex, 1];

                field.ball.setThrowingCamera(dstBaseX, dstBaseY);

                if (field.bBaseCoverd[field.nTargetIndex] == false)
                {
                    ///////////////////////////////////////////////////////////////////////
                    //UnityEngine.Debug.Log("현재 베이스 커버가 되어 있지 않은 경우");
                    ///////////////////////////////////////////////////////////////////////
                    //거리계산
                    float distanceFielderBase = FieldingMechanism.getDistance(posX, dstBaseX, posY, dstBaseY); //던지는 자와 목표 베이스간의 거리
                    float maxFielderTime = (distanceFielderBase / throwSpeed);//THROW_SPEED);            //던지는 공이 목표 베이스까지 걸리는 시간

                    //현재 아무도 커버를 하고 있지 않은 경우
                    if (coverIndex == -1 || coverIndex >= CPlayer._LEFTFIELDER)
                    {
                        //익셉션 처리
                        coverIndex = FieldingMechanism.getCoverIndexException(posIndex, field.ball.firstAngle);
                    }

                    Fielder coverFielder = field.fielder[coverIndex]; //커버 필더의 정보를 얻어옴

                    //스퀴즈 번트 예외처리
                    if (posIndex == CPlayer._CATCHER)
                    {
                        if (field.bBuntFielding == true && field.batter.buntType == SimulBuntType.SQUEEZE)
                        {
                            //스퀴즈번트인경우 포수가 잡았는데 루가 빔
                            coverFielder.bBaseCovering = false;
                            coverFielder.setStop();
                            setCarry(field.nTargetIndex);
                            field.run.bHomeSteal = true;
                            field.bFieldStealFlag = true;
                            return; //리턴을 해버림으로서 함수에서 빠져나온다
                        }
                    }


                    //2루 베이스를 두고 유격수나 2루수가 직접 베이스를 찍는지 여부를 판단하는 함수
                    if ((field.nTargetIndex == FieldParm.SECONDBASE_INDEX) &&
                        (posIndex == CPlayer._SHORTSTOP || posIndex == CPlayer._SECONDBASEMAN))
                    {
                        int coverIndex2 = (posIndex == CPlayer._SECONDBASEMAN ? CPlayer._SHORTSTOP : CPlayer._SECONDBASEMAN);

                        if ((field.fielder[coverIndex2].bBaseCovering == false) ||  //상대 (유격수 혹은 2루수)가 아직 베이스를 커버 안한 경우나
                            (posIndex == CPlayer._SHORTSTOP && throwType == ThrowType._INFIELD_SIDE_SPIN) || //내가 유격수인데 스핀 쓰로우가 걸린 경우나
                            (distanceFielderBase * 0.8f < FieldingMechanism.getDistance(field.fielder[coverIndex2].posX, dstBaseX, field.fielder[coverIndex2].posY, dstBaseY))) // 커버맨보다 어느정도 가까운 경우이면
                        {
                            //베이스를 직접 가서 찍는 루틴
                            field.fielder[coverIndex2].bBaseCovering = false;
                            field.fielder[coverIndex2].setStop();
                            setCarry(field.nTargetIndex);
                            return; //리턴을 해버림으로서 함수에서 빠져나온다
                        }
                        else
                        {
                            //그렇지 않은 경우
                            //2루에 특수 던지기에 대한 익셉션 처리
                            throwType = FieldingMechanism.getDPThrowException(posIndex, throwType);

                            //익셉션 처리 후에도 점핑쓰로우나 스핀 쓰로우가 아닌 경우에
                            if (throwType != ThrowType._INFIELD_OVER_JUMPING && throwType != ThrowType._INFIELD_SIDE_SPIN)
                            {
                                //너무 가까우면 기존 쓰로우타입 무시하고 토스로 변경
                                if (Mathf.Abs(distanceFielderBase) < (FieldParm.TOSS_DISTANCE - 100))
                                {
                                    //조건 만족시 토스 변경
                                    bToss = true;
                                    field.bTossThrow = true;
                                    throwSpeed = 0.5f * throwSpeed;
                                }
                            }

                        }
                    }


                    //1루 베이스를 두고 1,2,3,유가 판단하는 함수
                    float distanceCoverBase = FieldingMechanism.getDistance(coverFielder.posX, dstBaseX, coverFielder.posY, dstBaseY); //커버맨과 베이스간 거리
                    float maxCoverTime = (distanceCoverBase / coverFielder.FIELDER_SPEED);                                       //커버맨이 베이스를 커버하는 시간

                    if (maxFielderTime < maxCoverTime) // 커버맨보다 공이 도착하는 시간이 빠를 경우
                    {
                        if ((posIndex == CPlayer._FIRSTBASEMAN && field.nTargetIndex == FieldParm.FIRSTBASE_INDEX) ||           //1루수가 1루를 목표로 하는 경우
                            (posIndex == CPlayer._SECONDBASEMAN && field.nTargetIndex == FieldParm.SECONDBASE_INDEX) ||         //2루수가 2루를 목표로 하는 경우
                            (posIndex == CPlayer._SHORTSTOP && field.nTargetIndex == FieldParm.SECONDBASE_INDEX && throwType != ThrowType._INFIELD_OVER_JUMPING)) //유격수가가 2루를 목표로 하는 경우 && 점핑쓰로우가 아닌경우
                        {
                            bool bTossThrow = false;
                            float rate = maxFielderTime / maxCoverTime; //시간을 비교한 비율을 구함

                            if (posIndex == CPlayer._FIRSTBASEMAN)
                            {
                                if(distanceFielderBase > distanceCoverBase + 50 && rate > 0.13f)
                                {
                                    throwSpeed = Mathf.Clamp(0.9f * (distanceFielderBase * FIELDER_SPEED) / distanceCoverBase , 500, 1500);
                                    bTossThrow = true;
                                }
                            }
                            else
                            {
                                throwSpeed = 0.9f * (distanceFielderBase * FIELDER_SPEED) / distanceCoverBase;
                                bTossThrow = true;
                            }


                            if (bTossThrow == false)
                            {
                                //수비수가 직접 베이스를 찍음
                                coverFielder.bBaseCovering = false;
                                coverFielder.setStop();
                                setCarry(field.nTargetIndex);
                                return;
                            }
                            else
                            {
                                //그렇지 않은 경우 토스
                                //토스를 하기 위해 산출되는 물리 공식을 여기서 계산
                                bToss = true;
                                field.bTossThrow = true;                                
                                float newDestX = coverFielder.posX + 0.75f * (dstBaseX - coverFielder.posX);
                                float newDestY = coverFielder.posY + 0.75f * (dstBaseY - coverFielder.posY);
                                dstBaseX = newDestX;
                                dstBaseY = newDestY;
                                wrist = 1;
                                coverFielder.bTossTaking = true;
                            }
                        }
                        else
                        {
                            //위의 조건에 만족하지 않는 경우
                            //딜레이를 주어 던지게 하는데                        
                            delayTime = (maxCoverTime - maxFielderTime); //딜레이 타임
                            bThrowAddDelay = FieldingMechanism.getThrowAddDelayException(posIndex, throwType); //특정 상황에서는 딜레이가 무시되는 익셉션이 발생, 디폴트는 true임
                        }
                    }

                    coverFielder.taggingDelay = curTaggingDelay;
                }
                else
                {
                    ////UnityEngine.//Debug.Log("====================>>coverIndex = " + coverIndex);
                    ////UnityEngine.//Debug.Log("====================>>nTargetIndex = " + field.nTargetIndex);
                    //던지려고 하는 경우 커버맨이 on base인 경우 목표에 던진다
                    //포스아웃 관련 오프셋
                    if (coverIndex == -1) return;

                    dstBaseX = field.fielder[coverIndex].posX + FieldingMechanism.baseOffset[field.nTargetIndex, 0];
                    dstBaseY = field.fielder[coverIndex].posY + FieldingMechanism.baseOffset[field.nTargetIndex, 1];

                    field.ball.setThrowingCamera(dstBaseX, dstBaseY);
                    field.fielder[coverIndex].setCatchDir(dstBaseX, dstBaseY);

                    if (field.bFieldStealFlag == true && field.nTargetIndex == FieldParm.SECONDBASE_INDEX)
                    {
                        ////UnityEngine.//Debug.Log("====================>>스틸 플래그 옵셋");
                        dstBaseX -= 40;
                    }

                    //이경우에도 목표가 너무 가까우면 토스를 한다
                    float distanceFielderBase = FieldingMechanism.getDistance(posX, dstBaseX, posY, dstBaseY);

                    if (field.nTargetIndex == FieldParm.SECONDBASE_INDEX && distanceFielderBase < FieldParm.CARRY_DISTANCE)
                    {
                        /*field.fielder[coverIndex].setBackupPosition(0,0,true);
                        field.fielder[coverIndex].curTime = 1000;*/
                        setCarry(field.nTargetIndex);
                        return; //리턴을 해버림으로서 함수에서 빠져나온다
                    }
                    else if (distanceFielderBase < FieldParm.TOSS_DISTANCE)
                    {
                        if (posIndex == field.nRelayFielderIndex) return;
                        //너무가까우면 토스
                        bToss = true;
                        field.bTossThrow = true;
                        throwSpeed = 1.6f * FIELDER_SPEED;// (distanceFielderBase * FIELDER_SPEED);
                    }

                    //송구 정확도 관련
                    field.fielder[coverIndex].taggingDelay = curTaggingDelay;
                }
            }

            //던지는 방향을 구하는 공식들
            angleDir = Mathf.Atan2(dstBaseY - posY, dstBaseX - posX);


            curTime = 0;
            int lastDir = nFielderDir;
            nFielderDir = FieldParm.getDir(angleDir);//getDir();

            //각종 애니메이션에 따라 바뀔수 있는 물리 값들을 저장 해둔다
            tStartX = posX;             //x 포지션저장
            tStartY = posY;             //y 포지션 저장
            tStartZ = FieldParm.BALL_INIT_HEIGHT;   //z 포지션 저장
            tSpeed = throwSpeed;        //던지는 속도 저장    
            
            field.curThrowIndex = posIndex;

            //송구에러 예외처리
            if (field.nTargetIndex == FieldParm.RELAY_INDEX ||
                   bToss == true ||
                   (field.nTargetIndex == FieldParm.SECONDBASE_INDEX && posIndex == field.nRelayFielderIndex))
            {                
                bThrowErrorFlag = false;
            }

            if (bToss == true)
            {
                if (tSpeed < (THROW_SPEED * 0.34f))
                {
                    tSpeed = (THROW_SPEED * 0.34f);
                }
            }

            bSpecialAnim = false;
            if (bThrowAddDelay == true)    //베이스 커버가 느려서 딜레이가 발생할 경우
            {
                if (bRecheck == false)
                {
                    //해당 딜레이 만큼 기다렸다가 쓰로우 애니메이션을 호출
                    StartCoroutine(setThrowAnimationDelay(delayTime, field.bTossThrow, field.nTargetIndex, lastDir));
                }
            }
            else //딜레이가 발생하지 않은 경우
            {
                if (bRecheck == false)
                {
                    //해당 애니메이션을 호출
                    setThrowAnimation(bThrowAddDelay, field.bTossThrow, field.nTargetIndex, lastDir);
                }
                
                //물리값을 넣어서 던진는 공의 vector값을 구한다
                field.setThrowingVector(tStartX, tStartY, tStartZ, dstBaseX, dstBaseY, tSpeed, wrist, nFielderDir, bThrowErrorFlag);
                //토스시
                if (bToss == true)
                {
                    //토스시 토스를 테이크 하는 커버맨의 움직이는 속도 재설정
                    field.bBallTail = false;
                    field.fielder[coverIndex].setNewSpeedByFrame(field.ball.throwingTime + THROW_DELAY, dstBaseX, dstBaseY);

                    //글러브 토스 조건
                    if (skillQuickThrowLevel > 0 && posIndex == CPlayer._SECONDBASEMAN)
                    {
                        if (posX < originX)
                        {
                            if (grounderCatchType == GrounderCatch.GROUNDERCATCH_NORMAL
                             || grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING
                             || grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING_NORMAL)
                            {
                                //글러브 토스 컴비네이션 플레이
                                StartCoroutine(globeTossCombination());
                            }
                        }
                    }
                }
            }
        }

        /*
        public void setForcedThrow()
        {
            bForcedThrow = true;
        }*/

        private void checkRunnerTurbo(float throwSpeed, float delay1, float delay2)
        {
            bool bOut = true;            
            Runner destRunner = field.run.getDestRunner(FieldParm.FIRSTBASE_INDEX);

            /*
            int turboBonus = 0;            
            //터보 성공 보너스 주자 스킬 '수퍼소닉'
            if(destRunner.pRunner.skillAvailable(SkillIndex.RunnerSuperSonic) == true)
            {                
                turboBonus = destRunner.pRunner.getSkillValue(SkillIndex.RunnerSuperSonic).arg_vale[3];
            }*/

            field.ball.bNoEventCamera = false;
            field.ball.setFielderFocus(posIndex);

            if (pFielder.skillAvailable(SkillIndex.SpecialThrow) == true)
            {
                //야수가 스페셜 송구 보유시 터보에 대한 카운터 발생
                if (throwType == ThrowType._NORMAL)
                {
                    //터보 발동시 퀵스로우 특능 가지고 있고 송구타입이 노멀인 경우
                    catchDelayRate = 0.40f;
                    throwType = ThrowType._INFIELD_SIDE_QUICK;
                }

                //주루센스 vs 특급송구                
                bool bOffenseWin = field.setVsSkill(destRunner.pRunner, pFielder, SkillIndex.RunnerTurbo, SkillIndex.SpecialThrow, delay1, delay2);
                //Debug.Log("====================>>> 대결연출 임시 : 주루센스(터보) vs 특급송구");                  
                if (bOffenseWin == true)
                {
                    field.runnerTurbo = FieldSkillUse.Success;
                    bOut = false;
                }
            }
            else
            {
                //터보의 성공여부
                if(destRunner.checkSkillOn(SkillIndex.RunnerTurbo) == true)
                {
                    field.runnerTurbo = FieldSkillUse.Success;
                    bOut = false;
                }
            }

            if (bOut == false)
            {
                if (posIndex == CPlayer._FIRSTBASEMAN)
                {
                    ////UnityEngine.//Debug.Log("=================================>>1루땅볼 세팅 처리");
                    destRunner.setShobuRunnerSpeed(bOut, 2.0f, 2.2f, true);
                }
                else
                {
                    float rate = destRunner.basePositionRate();
                    float rateLimit = (posIndex <= CPlayer._SECONDBASEMAN ? 0.35f : 0.1f);
                    if (rate < rateLimit)
                    {
                        ////UnityEngine.//Debug.Log("=======================================================================================================>>커트라인 통과 못함");
                        field.runnerTurbo = FieldSkillUse.Active;
                        bOut = true;
                    }
                    float timeLeft = field.getTimeLeftforThrow(FieldParm.FIRSTBASE_INDEX, posX, posY, 0.1f, throwSpeed);
                    destRunner.setShobuRunnerSpeed(bOut, timeLeft, 2.2f);   
                }
            }
        }

        private void checkQuickThrow()
        {
            if (skillQuickThrowLevel > 0)
            {
                if (throwType == ThrowType._NORMAL)
                {
                    if (posIndex != field.nRelayFielderIndex)
                    {
                        if (Random.Range(0, 1000) < throwingAblil && MyMath.Percent() <35)
                        {
                            Runner targetRunner = field.run.getDestRunner(field.nTargetIndex);
                            if (targetRunner != null)
                            {
                                float rate = targetRunner.basePositionRate();
                                float rateMinimum = (posIndex <= CPlayer._SECONDBASEMAN ? 0.5f : 0.25f);
                                ////UnityEngine.//Debug.Log("====================================================>>rate = " + rate);
                                if (rate > rateMinimum)
                                {
                                    catchDelayRate = 0.22f;
                                    throwType = ThrowType._INFIELD_SIDE_QUICK;
                                }
                            }
                        }
                    }
                }
            }
        }


        //특수 던지기 모션을 세팅하고 캐치 딜레이 rate를 구한다
        public void setSpecialThrowType(ThrowState state)
        {
            ////UnityEngine.//Debug.Log("====================>>setSpecialThrowType: " + state);

            throwType = ThrowType._NORMAL;

            if (field.bSpecialMoveActivte == true)
            {
                //스핀 점핑 케이스인 경우
                throwType = FieldingMechanism.getThrowType(state);
            }


            throwReadyDelay = FieldingMechanism.getThrowReadyDelay(throwType);

            if (throwType != ThrowType._NORMAL)
            {
                catchDelayRate = FieldingMechanism.getCatchDelayRate(throwType, fieldingAbil, (checkDoublePlay() && posIndex == CPlayer._SECONDBASEMAN));
            }
            else
            {
                catchDelayRate = 1.0f;
                field.setTimeScale(Field.INIT_TIME_SCALE);
            }
        }

        //재송구 세팅
        public void setRecheckThrow(int nextPos)
        {
            ////Debug.Log("====================>>setRecheckThrow: " + nextPos);
            if (field.ball.step == BallStep.BALL_THROW) return;
            if (posIndex >= CPlayer._LEFTFIELDER) return;                       //외야수인 경우 리체크 안함
            if (field.doublePlayType >= FieldParm.DOUBLEPLAY_163) return;       //병살인 경우 리체크 안함
            if (bBaseCovering == true && nCoveringIndex == nextPos) return;
            if (field.manager.nOutCount >= 3 || field.manager.bThreeOutChange == true) return;
            if (actState == FielderAction._THROWING_CATCH && aStep == ActionStep._TAGGING) return;  //태그시 리체크 안함

            Runner runner = field.run.getDestRunner(nextPos);
            if (runner != null)
            {
                if (field.run.bOnBase[nextPos] == false && runner.basePositionRate() < 0.9f)
                {
                    if (runner.state == RunState.MOVE && runner.bMoveForward == true)
                    {
                        if (field.checkRecheckOutPossible(posIndex, nextPos) == true)
                        {
                            field.nTargetIndex = nextPos;
                            ////UnityEngine.//Debug.Log("=================>>@@@@ nTargetIndex " + field.nTargetIndex);
                            int coverIndex = field.getBaseCoverIndex(field.nTargetIndex);
                            setThrow(coverIndex);
                        }
                    }
                }
            }
        }

        //재송구 세팅 - 타입2
        public bool setRecheckThrow2()
        {
            ////UnityEngine.//Debug.Log("========================>>다시 던지기 체크 posIndex "+posIndex);
            int nextBase = -1;

            if (posIndex >= CPlayer._LEFTFIELDER) return false;
            if (bTossTaked || bTossTaking) return false;

            for (int i = FieldParm.HOMEBASE_INDEX; i > FieldParm.FIRSTBASE_INDEX; i--)
            {
                if (field.run.bOnRunning[i] == true)
                {
                    ////UnityEngine.//Debug.Log("========================>>nextBase = " + nextBase);
                    nextBase = i;
                    break;
                }
            }

            if (nextBase != -1)
            {
                ////UnityEngine.//Debug.Log("========================>>nextBase = " + nextBase);
                setRecheckThrow(nextBase);
                return true;
            }

            return false;
        }

        

        //////////////////////////////////////////////////////////////////
        //메인 프레임과 상태 변화
        //1. 메인 프레임
        //////////////////////////////////////////////////////////////////
        //Fielder의 Main Frame함수
        public void nextFrame()
        {
            curTime += deltaTime;

            if (bStun == true)
            {
                if (curTime > 1.5f)
                {
                    actState = FielderAction._COLLISION;
                    bStun = false;
                    curTime = 0;
                }
                return;
            }

            switch (actState)//    f_nActState[i])
            {

                case FielderAction._STANDBY:
                    standbyFrame();
                    break;

                case FielderAction._MOVE:
                    moveFrame();
                    break;

                case FielderAction._FIELDING:
                    fieldFrame();
                    break;

                case FielderAction._CATCHING:
                    catchFrame();
                    break;

                case FielderAction._MOTION:
                    motionFrame();
                    break;

                case FielderAction._THROWING:
                    throwingFrame();
                    break;

                case FielderAction._THROWING_CATCH:
                    throwingCatchFrame();
                    break;
                case FielderAction._CATCHER_LONG_TAG:
                    longTagFrame();
                    break;

                case FielderAction._PICKOFF:
                    pickOffFrame();
                    break;

                case FielderAction._COLLISION:
                    collisionFrame();
                    break;

                case FielderAction._ERROR_PANIC:
                    errorPanicFrame();
                    break;

            }

            drawFielder();
        }

        //상태 혹은 방향에 따라 야수의 애니메이션을 바꾸어준다
        private void drawFielder()
        {
            if (bSpecialAnim == false)
            {
                if (bNoDirChange2 == false)
                {
                    if (lastActState != actState)
                    {                        
                        strID = _strID + FieldParm._dir[nFielderDir];
                        playAnim();
                        lastActState = actState;
                    }
                    else
                    {
                        if (nLastDir != nFielderDir)// && bChangeDir == true)
                        {
                            strID = _strID + FieldParm._dir[nFielderDir];
                            playAnim();
                            nLastDir = nFielderDir;
                        }
                    }
                }

                if (bNoDirChange == true)
                {
                    bNoDirChange2 = true;
                    bNoDirChange = false;
                }
            }

            setPosition();
        }
                
        //타임스케일을 조정한다.
        public void setTimeScale(float scale)
        {
            /*if (Mode.bAutoPlay == true)
            {
                anim.timeScale = 1;
            }
            else*/
            {
                timeScaleRate = scale;
                anim.timeScale = (timeScale * timeScaleRate);
            }
        }

        //필드뷰 <-> 배팅뷰를 오고갈때 상태 업데이트
        public void setUpdate(bool bAvailable)
        {
            ////////UnityEngine.//Debug.Log("======================>> fielder Set InitPos : posIndex : "+posIndex);
            //setInitPos(posIndex);
            nFielderDir = FieldingMechanism._InitFielderDir[posIndex];
            setStop();
            drawFielder();

            bStun = false;
            //배팅뷰로 돌아가기 전에 한번 호출해준다.
            bFielderActive = bAvailable;// false;
            if (bAvailable == true) curTime = 0;
            Util.ChangeLayersRecursively(transform, "FIELDINGVIEW_LAYER");
        }

        //다음 애니메이션을 세팅
        float nextDelayAniTime = 1.5f;
        private bool noDelayAnim = false;
        //string delayNextMoveString;
        //다음 상태를 세팅
        private void setNextMove(string next, bool bLoop, float tScale, bool bShodow = true, bool changeDir = true)
        {
            bNoDirChange = false;
            bNoDirChange2 = false;

            if (bSpecialAnim == true)
            {
                if (noDelayAnim == false)
                {
                    StartCoroutine(delaySetNextMove(nextDelayAniTime, next, bLoop, tScale, bShodow, changeDir));
                    nextDelayAniTime = 1.5f; //디폴트 세팅
                }
            }
            else
            {
                //if (posIndex == CPlayer._FIRSTBASEMAN)  UnityEngine.Debug.Log("####### tScale = " + tScale);                
                bSpecialAnim = false;
                offsetX = 0;
                setOffetDV();
                nLastDir = -1;
                _strID = next;
                loop = bLoop;
                timeScale = tScale;
                shadow.GetComponent<Renderer>().enabled = bShodow;
                //lastChangeDir = changeDir;
            }
            //nLastDir = -1;
        }

        IEnumerator delaySetNextMove(float delay, string delayNextMoveString, bool bLoop, float tScale, bool bShodow = true, bool changeDir = true)
        {
            yield return new WaitForSeconds(delay);
            bSpecialAnim = false;
            setNextMove(delayNextMoveString, bLoop, tScale, bShodow, changeDir);
        }

        //위치를 업데이트
        public void setPosition()
        {
            screenX = field.getScreenX(posX);
            screenY = field.getScreenY(posY);

            float depthZ = 0;// 
#if _OrthoCamera
            depthZ = -4 + (posY * 0.0002f);
#endif
            //float scale = field.getScale(posX, posY);
            checkOffsetX();
            transform.localPosition = new Vector3(screenX + curOffsetX, screenY, depthZ);

            setRatio();
            //transform.localScale = new Vector3(scale, scale, 1);

        }
                
        //야수의 상태를 파라메터로 넘긴다
        public void setState(FielderAction state, ActionStep step)        
        {            
            actState = state;
            aStep = step;
            lastActState = FielderAction._NOTHING_STATE;            
        }

        //////////////////////////////////////////////////////////////////
        //2. 애니메이션과 상태 변화
        //////////////////////////////////////////////////////////////////
        //조건 만족시 해당 애님을 호출하기 위해 drawFielder() 함수내에서 호출
        int track = 1;
        int lastTrack;
        bool bBlending = false;
        bool bThrowBlending = false;
        private void playAnim()
        {
            if (strID != lastStrID)
            {                
                if (anim.state.Data.skeletonData.FindAnimation(strID) != null)
                {
                    if (bBlending == false)
                    {
                        anim.state.ClearTracks();
                        anim.skeleton.SetToSetupPose();
                    }
                    anim.state.SetAnimation(0, strID, loop);
                    bBlending = false;
                    lastStrID = strID;
                    
                }
            }
            anim.timeScale = (timeScale * timeScaleRate);
        }

        //강제로 애니메이션 세팅
        private void setForcedAnim(string str)
        {
            _strID = str;
            strID = _strID + FieldParm._dir[nFielderDir];            
            playAnim();
        }

        //특수한 애니메이션을 호출하기 위한 함수
        private void playSpecialAnim(string str, bool bLoop, float _timeScale = 1.0f, bool bShadow = true)
        {
            bSpecialAnim = true;
            timeScale = 1;

            //if (track != lastTrack) anim.state.ClearTrack(lastTrack);
            anim.state.ClearTracks();
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(0, str, bLoop);

            //lastTrack = track;
            //track = 1 - track;
            

            anim.timeScale = (_timeScale * timeScaleRate);
            shadow.GetComponent<Renderer>().enabled = bShadow;
        }

        //해당 딜레이후 playSpecialAnim()을 호출함
        private IEnumerator nextSpecialAnim(float delay, string str, bool bLoop, float timeScale = 1.0f, bool bShadow = true)
        {
            yield return new WaitForSeconds(delay);
            playSpecialAnim(str, bLoop, timeScale, bShadow);
        }

        //쓰로잉 애니메이션의 상태를 설정
        string strThrowAfterDelay;

        private IEnumerator setThrowAnimationDelay(float delay, bool bToss, int target, int lastDir)
        {
            yield return new WaitForSeconds(delay);
            setThrowAnimation(false, bToss, target, lastDir);
            field.setThrowingVector(tStartX, tStartY, tStartZ, dstBaseX, dstBaseY, tSpeed, 1, nFielderDir, bThrowErrorFlag);
        }

        
        private void setThrowAnimation(bool bDelay, bool bToss, int target, int lastDir)
        {
            throwGabX = throwGabY = 0;
            float tdV = 0; //던지면서 이동
            float secondSpinOffsetX = 0;
            bool bShadow = true;
            bool delayNeeded = false;
            float speedRate = 1;
            dX = 0;
            dY = 0;
            tdX = 0;
            tdY = 0;
            throwStopRate = 1.0f;

            bool bPosOffset = false;
            bool bYAxisMove = false;

            
            if (posIndex == CPlayer._CATCHER)
            {
                //포수 예외처리
                throwType = ThrowType._NORMAL;
                string strThrow;// = FieldingMechanism._THROW_CATCHER_NORMAL;
                if (SimulSteal.catcherSitThrow != FieldSkillUse.Init)
                {
                    //앉아쏴
                    strThrow = FieldingMechanism._THROW_CATCHER_SIT;
                    throwReadyDelay = FieldingMechanism.DELAY_SIT_THROW;
                    tdV = 20;
                    throwMoveRate = 0.9f;
                }
                else
                {
                    //일반
                    strThrow = FieldingMechanism._THROW_CATCHER_NORMAL;
                    throwReadyDelay = FieldingMechanism.DELAY_THROW_NORMAL;
                    tdV = 40;
                    throwMoveRate = 0.9f;
                }
                bYAxisMove = true;
                tDir = FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad;//
                setNextMove(strThrow, false, speedRate, bShadow);
                actState = FielderAction._THROWING;
            }
            else
            {
                if (bToss == true)
                {
                    ////UnityEngine.//Debug.Log("==========>>토스 애니메이션 호출해야 하는데 일반");
                    setNextMove(FieldingMechanism._TOSS_INFIELD, false, 1);

                    throwReadyDelay = FieldingMechanism.DELAY_THROW_TOSS;
                    tdV = 40;
                    throwMoveRate = 0.9f;
                    tDir = FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad;// angleDir;
                }
                else
                {
                    if (bDelay == true)
                    {
                        ////UnityEngine.//Debug.Log("==========>>DELAY가 들어감");
                        setNextMove(FieldingMechanism._THROW_NORMAL, false, 1);
                        tdV = 40;
                        throwMoveRate = 0.9f;
                        throwDelayTime = FieldingMechanism.DELAY_THROW_NORMAL;
                        tDir = FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad;// angleDir;
                    }
                    else
                    {
                        //////UnityEngine.//Debug.Log("==========>>DELAY가 안들어감");
                        //bMoveThrow = false;
                        string strThrow = FieldingMechanism._THROW_NORMAL;// _SIDETHROW_INFIELD_NOSTEP_QUICK;
                        tdV = 40;
                        throwMoveRate = 0.9f;

                        if (field.bOnceWildThrow == true)
                        {
                            //와일드 송구가 한번 발생하면 그다음부터는 무조건 노멀송구
                            throwType = ThrowType._NORMAL;
                        }

                        if (throwType == ThrowType._NORMAL)
                        {
                            if (checkStrongThrow() == true)// posIndex >= CPlayer._LEFTFIELDER)// && field.nTargetIndex != FieldParm.RELAY_INDEX)
                            {
                                throwType = ThrowType._NORMAL_STRONG;
                                strThrow = FieldingMechanism._THROW_OUTFIELDER_NORMAL;
                                throwReadyDelay = FieldingMechanism.DELAY_THROW_ONESTEP;
                                tdV = 55;
                                throwMoveRate = 0.25f;
                                throwStopRate = 0.8f;
                                if (nFielderDir == FieldParm._EAST) nFielderDir = FieldParm._SOUTHEAST;
                                else if (nFielderDir == FieldParm._WEST) nFielderDir = FieldParm._SOUTHWEST;
                                tDir = FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad;//angleDir;
                                bYAxisMove = true;

                            }
                        }

                        //  ////UnityEngine.//Debug.Log("==========>>throwType = " + throwType);

                        if (throwType == ThrowType._INFIELD_SIDE_QUICK)
                        {
                            //##연출 내야수 퀵스로우
                            strThrow = FieldingMechanism._SIDETHROW_INFIELD_NOSTEP_QUICK;
                            tdV = 40;
                            throwMoveRate = 0.9f;
                            tSpeed *= SkillParm.getQuickThrowRate(skillQuickThrowLevel);// 0.9f;

                        }
                        
                        else if (throwType == ThrowType._INFIELD_SIDE_DASH)
                        {
                            //////UnityEngine.//Debug.Log("====================>>nFielderDir = " + nFielderDir);
                            if (nFielderDir == FieldParm._EAST || nFielderDir == FieldParm._NORTHEAST || nFielderDir == FieldParm._SOUTHEAST)
                            {
                                if (MyMath.Percent() < 40) //체크체크
                                {
                                    //험하게 던지기
                                    field.setZoomTo(2.0f, 0.2f);
                                    strThrow = "FORWARD_DASH_CATCH_THROW_";
                                    if (nFielderDir == FieldParm._EAST)
                                    {
                                        throwGabX = -100;
                                        throwGabY = 0;
                                    }
                                    else if (nFielderDir == FieldParm._NORTHEAST)
                                    {
                                        throwGabX = -85;
                                        throwGabY = -45;
                                    }
                                    else //if (nFielderDir == FieldParm.SE)
                                    {
                                        throwGabX = -90;
                                        throwGabY = 40;
                                    }
                                }
                                else
                                {
                                    strThrow = FieldingMechanism._SIDETHROW_INFIELD_DASH;
                                    throwGabX = -35;
                                    throwGabY = (nFielderDir == FieldParm._SOUTHEAST ? 5 : -5);
                                }
                                //tDir = Mathf.Atan2(-100, 0);
                                tDir = FieldParm._angleDir[FieldParm._SOUTH] * Mathf.Deg2Rad;//
                                tdV = 40;
                                bShadow = false;
                                throwReadyDelay = FieldingMechanism.DELAY_THROW_SIDEDASH;
                                throwMoveRate = 0.99f;
                                tSpeed *= SkillParm.getQuickThrowRate(skillDashThrowLevel);// 0.9f;
                                bYAxisMove = true;
                            }
                            else
                            {
                                //예외 처리
                                strThrow = FieldingMechanism._SIDETHROW_INFIELD_NOSTEP_QUICK;
                                tdV = 40;
                                throwReadyDelay = FieldingMechanism.DELAY_THROW_SIDEQUICK;
                                throwMoveRate = 0.9f;
                                tSpeed *= SkillParm.getQuickThrowRate(skillDashThrowLevel);//0.9f;
                            }

                        }
                        else if (throwType == ThrowType._INFIELD_SIDE_SPIN) //나중에 고쳐
                        {
                            if (target == FieldParm.FIRSTBASE_INDEX ||
                                target == FieldParm.SECONDBASE_INDEX && (posIndex == CPlayer._THIRDBASEMAN || posIndex == CPlayer._SHORTSTOP))
                            {
                                bThrowBlending = true;
                                strThrow = FieldingMechanism._SIDETHROW_INFIELD_SPIN;
                                tdV = (nFielderDir == FieldParm._EAST ? 100 : 80);
                                throwMoveRate = 0.25f;
                                throwStopRate = 0.8f;
                                tSpeed *= SkillParm.getSpinThrowRate(skillSpinThrowLevel);// 최소 0.85f; 최대 1.2
                                //tDir = Mathf.Atan2(0, 100);// //dstBaseX - posX);
                                tDir = FieldParm._angleDir[FieldParm._EAST] * Mathf.Deg2Rad;//

                                //포지션 옵셋
                                bPosOffset = true;
                                if (posIndex == CPlayer._SECONDBASEMAN)
                                {
                                    secondSpinOffsetX = -50;
                                }
                            }
                            else
                            {
                                //////UnityEngine.//Debug.Log("====================>>setSpecialThrowType 수정");
                                throwType = ThrowType._NORMAL;
                                throwReadyDelay = FieldingMechanism.DELAY_THROW_NORMAL;
                                strThrow = FieldingMechanism._THROW_NORMAL;// _SIDETHROW_INFIELD_NOSTEP_QUICK;
                                tdV = 40;
                                throwMoveRate = 0.9f;
                            }
                        }

                        else if (throwType == ThrowType._INFIELD_OVER_JUMPING) //나중에 고쳐
                        {
                            if (target == FieldParm.FIRSTBASE_INDEX ||
                                target == FieldParm.SECONDBASE_INDEX && (posIndex == CPlayer._THIRDBASEMAN || posIndex == CPlayer._SHORTSTOP))
                            {
                                bThrowBlending = true;
                                strThrow = FieldingMechanism._OVERTHROW_INFIELD_JUMPING;
                                tdV = -100;//(nFielderDir == FieldParm._EAST ? 100 : 80);
                                throwMoveRate = 0.25f;
                                throwStopRate = 0.7f;
                                //tDir = Mathf.Atan2(0, 100);
                                tDir = FieldParm._angleDir[FieldParm._EAST] * Mathf.Deg2Rad;//
                                tSpeed *= SkillParm.getJumpingThrowRate(skillJumpThrowLevel);// 최소 0.65f; 최대 1
                                tStartZ *= 1.5f;
                                speedRate = 1.5f;
                            }
                            else
                            {
                                //////UnityEngine.//Debug.Log("====================>>setSpecialThrowType 수정");
                                throwType = ThrowType._NORMAL;
                                throwReadyDelay = FieldingMechanism.DELAY_THROW_NORMAL;
                                strThrow = FieldingMechanism._THROW_NORMAL;// _SIDETHROW_INFIELD_NOSTEP_QUICK;
                                tdV = 40;
                                throwMoveRate = 0.9f;
                            }
                        }
                        else if (throwType == ThrowType._INFIELD_DODGE)
                        {
                            if (target == FieldParm.FIRSTBASE_INDEX ||
                                target == FieldParm.SECONDBASE_INDEX && (posIndex == CPlayer._THIRDBASEMAN || posIndex == CPlayer._SHORTSTOP))
                            {
                                nFielderDir = FieldParm._SOUTHEAST;
                                if (MyMath.Half())
                                {
                                    strThrow = "5050_SS_BAREHANDCATCH_JUMPTHROW_";// FieldingMechanism._OVERTHROW_INFIELD_JUMPING;
                                    tdV = -100;//(nFielderDir == FieldParm._EAST ? 100 : 80);
                                }
                                else
                                {
                                    strThrow = "5051_2B_DODGE2_THROW_";
                                    tdV = 0;
                                }
                                throwMoveRate = 0.25f;
                                throwStopRate = 0.7f;
                                tDir = FieldParm._angleDir[FieldParm._EAST] * Mathf.Deg2Rad;// tDir = Mathf.Atan2(0, 100);
                                tSpeed *= 1;
                                tStartZ *= 1.5f;
                                speedRate = 1.5f;
                            }
                            else
                            {
                                strThrow = FieldingMechanism._SIDETHROW_INFIELD_NOSTEP_QUICK;
                                tdV = 40;
                                throwReadyDelay = FieldingMechanism.DELAY_THROW_SIDEQUICK;
                                throwMoveRate = 0.9f;
                                tSpeed *= 1;//
                            }
                        }

                        else if (throwType == ThrowType._INFIELD_DOUBLE_PLAY1)
                        {
                            if (field.nTargetIndex == FieldParm.SECONDBASE_INDEX)
                            {
                                if (posIndex == CPlayer._SECONDBASEMAN)
                                {
                                    nFielderDir = FieldParm._WEST;
                                    strThrow = FieldingMechanism._GLOVE_TOSS_INFIELD;
                                    tDir = FieldParm._angleDir[FieldParm._WEST] * Mathf.Deg2Rad;// tDir = Mathf.Atan2(0, -100);
                                    tdV = 20;
                                    throwMoveRate = 0.9f;
                                    throwDelayTime = 0;// FieldingMechanism.DELAY_THROW_TOSS;
                                    tSpeed *= 0.5f;
                                }
                                else
                                {
                                    strThrow = FieldingMechanism._SIDETHROW_INFIELD_NOSTEP_QUICK;
                                    tdV = 40;
                                    throwMoveRate = 0.9f;
                                    tSpeed *= 0.9f;
                                }
                            }
                        }

                        setNextMove(strThrow, false, speedRate, bShadow);
                    }
                }
                if (delayNeeded == false)
                {
                    actState = FielderAction._THROWING;    
                }
                else
                {
                    setState(FielderAction._MOTION, ActionStep._THROW_DELAY);
                }
            }

            curTime = 0;

            tdX = tdV * Mathf.Cos(tDir);
            tdY = (bYAxisMove == false ? 0 : (tdV * Mathf.Sin(tDir)));


            float offsetX = (throwDelayTime * throwStopRate) * tdX; //throw offset X
            float offsetY = (throwDelayTime * throwStopRate) * tdY; //throw offset Y

            if (bPosOffset == true)
            {
                positionOffsetX += (tdX * throwMoveRate);
                positionOffsetY += (tdY * throwMoveRate);
            }

            positionOffsetX = positionOffsetY = 0;  //우선 무효
            dstOffsetX = dstOffsetX = 0;            //우선 무효

            //던지는 시작점에 포지션 offset 더해줌
            tStartX += (offsetX + positionOffsetX);
            tStartY += (offsetY + positionOffsetY);

            //던지는 도착점에 포지션 offset을 더해줌
            dstBaseX += (offsetX + dstOffsetX + secondSpinOffsetX);
            dstBaseY += (offsetY + dstOffsetY);


            if (bThrowBlending == true)
            {
                bBlending = true;
                bThrowBlending = false;
            }

            fieldSkillDisplayManager.EffectDisplay(fieldSkillDisplayManager.FieldDisplayStep.Throwing);

            //////UnityEngine.//Debug.Log("====================>>tdX = " + tdX);
            //////UnityEngine.//Debug.Log("====================>>tdY = " + tdY);

        }

        
        //태그 애니메이션
        private void setTagAnim()
        {
            bSpecialAnim = true;
            timeScale = 1;
            string strName = null;// = "6100_TAG_2B_TYPE3";
            if (field.nTargetIndex == FieldParm.FIRSTBASE_INDEX)
            {
                strName = "6100_TAG_1B_TYPE1";
            }
            else if (field.nTargetIndex == FieldParm.SECONDBASE_INDEX)
            {
                if (field.curThrowIndex <= CPlayer._LEFTFIELDER)
                {
                    if (field.bFieldPickOffFlag == true)
                    {
                        strName = "6100_TAG_1B_TYPE1";
                    }
                    else
                    {
                        strName = "6100_TAG_2B_TYPE3";
                    }
                   
                }
                else
                {
                    if (field.ball.firstAngle > 0)
                    {
                        strName = "6100_TAG_2B_TYPE3";
                    }
                    else if (field.ball.firstAngle > -15)
                    {
                        strName = "6100_TAG_2B_TYPE1";
                    }
                    else
                    {
                        strName = "6100_TAG_2B_TYPE2";
                    }
                }
            }
            else if (field.nTargetIndex == FieldParm.THIRDBASE_INDEX)
            {
                if (field.curThrowIndex < CPlayer._SECONDBASEMAN)
                {
                    strName = "6100_TAG_3B_TYPE3";
                }
                else
                {
                    if (field.curThrowIndex == CPlayer._LEFTFIELDER)
                    {
                        strName = "6100_TAG_3B_TYPE1";
                    }
                    else if (field.curThrowIndex == CPlayer._CENTERFIELDER)
                    {
                        strName = field.ball.firstAngle > 0 ? "6100_TAG_3B_TYPE1" : "6100_TAG_3B_TYPE2";
                    }
                    else
                    {
                        strName = "6100_TAG_3B_TYPE2";
                    }
                }
            }
            else if (field.nTargetIndex == FieldParm.HOMEBASE_INDEX)
            {
                if (posIndex != CPlayer._CATCHER)
                {
                    strName = "6100_TAG_1B_TYPE1";
                }
                else
                {
                    if (field.run.homeShobu == HomeShobu._SLIDING)
                    {
                        //다이나믹 태그
                        posX += 15;
                        strName = FieldingMechanism._CATCHER_TAG + FieldParm._dir[nFielderDir];
                        tagState = 100; //포수의 태그
                    }
                    else
                    {
                        //블록킹
                        strName = FieldingMechanism._CATCHER_BLOCK + FieldParm._dir[nFielderDir];
                        tagState = 200; //포수의 블록킹
                    }
                    //StartCoroutine(nextSpecialAnim(1.8f, "9000_CATCHER_HOLD_S", false));
                }

            }

            ////UnityEngine.//Debug.Log("=====>>strName = " + strName);
            playSpecialAnim(strName, false, 1);

        }

        //특수 애니메이션 연출 관련
        public void afterTagAnim()
        {
            if (tagState == 100)
            {
                //포수의 태그승
                playSpecialAnim("9700_CATCHER_TAG_WIN", false, 1);
                throwAgainState = FieldParm.ThrowAgain.NoThrow;
                tagState = -1;
            }
            else if (tagState == 200)
            {
                //포수의 블록승
                playSpecialAnim("9702_CATCHER_BLOCK_WIN", false, 1);
                throwAgainState = FieldParm.ThrowAgain.NoThrow;
                field.run.getDestRunner(FieldParm.HOMEBASE_INDEX).setRunnerCrushFail(false);
                tagState = -1;
            }
        }


        //////////////////////////////////////////////////////////////////
        //야수의 각종 상태 세팅
        //////////////////////////////////////////////////////////////////

        //1. 기본 세팅
        
        //송구된 공을 잡는 위치 세팅
        public void setCatchDir(float dX, float dY)
        {
            float aX = Mathf.Abs(dX - posX);
            float aY = Mathf.Abs(dY - posY);
            float signY = Mathf.Sign(dY - posY);

            if (dX > posX)
            {
                if (aY < 0.2f * aX) catchDir = FieldParm._EAST;
                else if (aY < 0.2f * aX) catchDir = signY > 0 ? FieldParm._NORTHEAST : FieldParm._SOUTHEAST;
                else if (aY < 0.2f * aX) catchDir = signY > 0 ? FieldParm._NORTH : FieldParm._SOUTH;
            }
            else
            {
                if (aY < 0.2f * aX) catchDir = FieldParm._WEST;
                else if (aY < 0.2f * aX) catchDir = signY > 0 ? FieldParm._NORTHWEST : FieldParm._SOUTHWEST;
                else if (aY < 0.2f * aX) catchDir = signY > 0 ? FieldParm._NORTH : FieldParm._SOUTH;
            }

        }

        //정지 상태 세팅
        public void setStop(bool nohold = false)
        {
            //if (posIndex == CPlayer._SHORTSTOP) ////UnityEngine.//Debug.Log("==================>>setStop호출");
            //if (nFieldState != _BASE_COVER)
            {
                //if (posIndex == 0) //UnityEngine.Debug.Log("pitcher stop");
                //if (posIndex == 3) //UnityEngine.Debug.Log("second stop");
                //if (posIndex == 4) //UnityEngine.Debug.Log("third stop");
                //if (posIndex == 5) //UnityEngine.Debug.Log("short stop");

                if (nohold == false)
                {
                    setNextMove(getHoldIndexStr(), true, 1);
                }

                speed = 0;
                dX = 0;
                dY = 0;
                actState = FielderAction._STANDBY;
                //nFieldState = 0xff;

                //nFielderDir = getDir();
                //strID = _strID + FieldParm._dir[nFielderDir];

                bFenceReady = false;
                //bBlending = false;
            }

        }

        //정지상태 세팅: deadball 상태
        public void setDeadBall()
        {
            setNextMove(getHoldIndexStr(), true, 1);

            dX = 0;
            dY = 0;
            actState = FielderAction._STANDBY;

            nFielderDir = FieldingMechanism._InitFielderDir[posIndex];
            //strID = _strID + FieldParm._dir[nFielderDir];

        }

        private void setCatchMotionException()
        {
            if (_strID == FieldingMechanism._RUN)
            {
                if (field.ball.bBound == true)
                {                    
                    if (nFielderDir == FieldParm._SOUTH || nFielderDir == FieldParm._SOUTHEAST || nFielderDir == FieldParm._SOUTHWEST)
                    {
                        dX = dY = 0;
                        _strID = FieldingMechanism._GROUNDBALL_CENTER;                        
                    }
                    else if(nFielderDir == FieldParm._NORTH)
                    {
                        dX = dY = 0;
                        _strID = FieldingMechanism._HOLD;
                    }
                    else if (nFielderDir == FieldParm._EAST || nFielderDir == FieldParm._NORTHEAST)
                    {
                        dY = 0;
                        nFielderDir = FieldParm._EAST;
                        _strID = FieldingMechanism._GROUNDBALL_FORE_BACK_HIGH;
                    }
                    else
                    {
                        dY = 0;
                        nFielderDir = FieldParm._WEST;
                        _strID = FieldingMechanism._GROUNDBALL_FORE_BACK_HIGH;
                    }

                }
                else
                {

                }
            }
        }



        //볼을 잡는 상태를 세팅
        public void setCatch(bool bFlyCatch, bool bCenterReadyCatch)
        {
            //SetCatch
            field.groundingDstX = posX;
            field.groundingDstY = posY;
            if (posIndex == CPlayer._PITCHER)
            {
                if (pitcherReactSuccess == true)
                {
                    if (FieldingMechanism.checkPitcherActRange(posY, originY, field.ballPower, field.ball.firstAngle, field.ball.firstAngleZ) == true)
                    {
                        //제5의내야수 - 투수반응 연출
                        fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.PitcherReaction);
                        skillJumpCatchLevel = 0; //점프캐치와 중첩방지
                        int index = 3;
                        if (Random.Range(0, 10) < 7) index = (field.ball.firstAngle > 1 ? 2 : 1);
                        playSpecialAnim("4400_PITCHER_SPECIAL_" + index, false);
                    }
                    else
                    {
                        pitcherReactSuccess = false;
                    }
                }
            }

            //////UnityEngine.//Debug.Log("==================>>어디서 불렸냐");
            if (bFlyCatch == true)
            {
                field.ball.nBallDZ = 0;
                field.ball.nBallZ = FieldParm.BALL_INIT_HEIGHT;
                field.setFlyOut();
                field.bBuntFielding = false;    // 번트필딩 무효화
            }

            field.nCatchIndex = posIndex;
            field.ball.setBallCatched(posIndex, posX, posY);

            if (bCenterReadyCatch == true)
            {
                //가운데서 캐칭 준비상태
                //setState(FielderAction._MOTION, ActionStep._MOTION_SET);//actState = FielderAction._THROW_READY;
                //////UnityEngine.//Debug.Log("==================>>posIndex = " + posIndex+"  ===>>setCatch!!!!!!!!!!!!!!!!!");

                setState(FielderAction._CATCHING, bDashCatched == false ? ActionStep._CATCHING : ActionStep._CATCHING_DASH);
                curTime = 0;

                if (bFlyCatch == true)
                {
                    if (field.ball.bRightBound == false && field.ball.bLeftBound == false)
                    {
                        //플라이볼 잡기
                        field.setZoomTo(field.curZoom * 1.3f, 0.5f);
                    }
                    throwType = ThrowType._NORMAL;
                    catchDelayRate = 1;
                }
                else
                {

                    if (bDashCatched == true)
                    {
                        if (posIndex < CPlayer._LEFTFIELDER)
                        {
                            field.setZoomTo(1.5f, 0.1f); //field.setZoomTo(field.curZoom * 1.3f, 0.3f);
                            if (bDashQuickThrow == true)
                            {
                                //내야수 특능인 경우
                                throwType = ThrowType._INFIELD_SIDE_DASH;
                                catchDelayRate = SkillParm.getDashThrowDelayRate(skillDashThrowLevel);//0.2f;// 0.5f;
                                //CameraManager.SetBlur2(false);// field.manager.setBlur(false);
                                //field.setTimeScale(Field.INIT_TIME_SCALE);                                
                                ////UnityEngine.//Debug.Log("===============>> 대쉬 쓰로우 딜레이 레이트: " + catchDelayRate);
                            }
                            else
                            {                                
                                throwType = ThrowType._NORMAL;
                                catchDelayRate = FieldingMechanism.getNormalThrowDelayRate(throwingAblil, fieldingAbil) + 0.35f;// 1.5f;
                                ////UnityEngine.//Debug.Log("===============>> 대쉬시 노멀 쓰로우 딜레이 레이트: " + catchDelayRate);
                                bDustOn = true;
                            }

                        }
                        else
                        {
                            //외야수 일반
                            if (field.ball.bRightBound == false && field.ball.bLeftBound == false)
                            {
                                if (field.ball.bBallDeadState == true)
                                {
                                    field.setZoomTo(1.5f, 0.7f);
                                    field.ball.setFielderFocus2(posIndex, 0.7f);
                                }
                                else
                                {
                                    field.setZoomTo(field.curZoom * 1.2f, 0.3f);
                                }
                            }
                            throwType = ThrowType._NORMAL;
                            catchDelayRate = 1;
                        }
                    }
                    else
                    {
                        if (posIndex < CPlayer._LEFTFIELDER)
                        {
                            field.setZoomTo(1.5f, 0.5f); //field.setZoomTo(field.curZoom * 1.2f, 0.3f);
                            //내야수 처리
                            if (checkDoublePlay() == true)
                            {
                                ////UnityEngine.//Debug.Log("==================>>더블플레이 특수");
                                //더블플레이 특수
                                field.bBallTail = false;
                                //setDoublePlayThrowTye();
                                throwType = ThrowType._NORMAL;
                                catchDelayRate = 0.65f;

                                if (field.ball.nBallZ > FieldParm.JUMPCATCH_HEIGHT)
                                {
                                    throwType = ThrowType._NORMAL;
                                    catchDelayRate = 1.5f;
                                }
                                else
                                {
                                    if (throwType == ThrowType._NORMAL)
                                    {
                                        catchDelayRate = 0.7f;
                                    }
                                }
                            }
                            else
                            {                                
                                if (field.ball.nBallZ > FieldParm.JUMPCATCH_HEIGHT)
                                {
                                    throwType = ThrowType._NORMAL;
                                    catchDelayRate = 1.5f;
                                }
                                else
                                {
                                    throwType = ThrowType._NORMAL;
                                    catchDelayRate = FieldingMechanism.getNormalThrowDelayRate(throwingAblil, fieldingAbil);
                                }
                            }
                        }
                        else
                        {
                            if (field.ball.bRightBound == false && field.ball.bLeftBound == false)
                            {
                                field.setZoomTo(field.curZoom * 1.3f, 0.3f);
                            }
                            //외야수 일반
                            throwType = ThrowType._NORMAL;
                            catchDelayRate = 1;
                        }
                    }
                }
            }

            if (pitcherReactSuccess == true)
            {
                //투수 반응 성공시
                catchDelayRate = 2;
            }
            else
            {
                if (field.run.bRunnerWalk == false)
                {
                    if (bFlyCatch == true)
                    {
                        if (flyballCatchType != FlyCatch.FLYCATCH_NORMAL) field.ball.setFielderFocus(posIndex);
                    }
                    else
                    {
                        if (grounderCatchType != GrounderCatch.GROUNDERCATCH_NORMAL) field.ball.setFielderFocus(posIndex);
                    }
                }
                
            }
        }


        //볼을 잡기위해 대기하는 상태를 세팅
        public void setCatchReady(bool bFlyball, bool bChase, bool bAnimation)
        {
            //bBlending = false;
            bDashCatched = false;

            bFlyballCatch = bFlyball;

            if (bFlyball == true)
            {
                if (bCatchErrorFlag == true)
                {
                    StartCoroutine(flyCatchMiss());
                }
                else
                {
                    if (posIndex == CPlayer._CATCHER)
                    {
                        nFielderDir = FieldingMechanism.getFlyballDirException(nFielderDir);
                        setNextMove(FieldingMechanism._CATCHER_FLYBALL, false, bAnimation ? 1 : 0);
                    }
                    else
                    {
                        if (posIndex == CPlayer._FIRSTBASEMAN || posIndex == CPlayer._RIGHTFIELDER)
                        {
                            nFielderDir = FieldParm._SOUTHWEST;
                        }
                        else if (posIndex == CPlayer._THIRDBASEMAN || posIndex == CPlayer._LEFTFIELDER)
                        {
                            nFielderDir = FieldParm._SOUTHEAST;
                        }
                        else
                        {
                            nFielderDir = FieldParm._SOUTH;
                        }
                        setNextMove(FieldingMechanism._FLYBALL_CENTER, false, bAnimation ? 1 : 0);
                    }
                }
            }
            else
            {
                if (posIndex == CPlayer._CATCHER)
                {
                    if (bChase == false)
                    {
                        setNextMove(FieldingMechanism._CATCHER_GROUNDBALL, false, (bAnimation ? 1 : 0));
                    }
                    else
                    {
                        setNextMove(FieldingMechanism._CATCHER_GROUNDBALL, false, 1);
                    }
                    bDashCatched = false;
                    bChase = false;
                }
                else
                {
                    //if(posIndex == CPlayer._SECONDBASEMAN) //UnityEngine.//Debug.Log("===============>>111111111111111?");
                    if (field.ball.nBallZ < 100)
                    {
                        ////////UnityEngine.//Debug.Log("===============>>111111111111111?");
                        if (bChase == false)
                        {
                            //////UnityEngine.//Debug.Log("===============>>2222222222222222222?  bAnimation = " + bAnimation);
                            ////UnityEngine.//Debug.Log("====================>>ballZ = " + field.ball.nBallZ);
                            setNextMove(getCenterCatchStr(), false, (bAnimation ? 1 : 0));
                        }
                        else
                        {
                            if (bDashCatchTry == true)///|| (posIndex < CPlayer._LEFTFIELDER && posIndex > CPlayer._CATCHER))
                            {
                                //////UnityEngine.//Debug.Log("===============>>333333333333333?");                    
                                nFielderDir = FieldingMechanism.getDashDir(posIndex);// getDashDirException(nFielderDir);
                                if (posIndex < CPlayer._LEFTFIELDER)
                                {
                                    setNextMove(FieldingMechanism._GROUNDBALL_DASHCATCH, false, 1);
                                }
                                else
                                {
                                    setNextMove(FieldingMechanism._GROUNDBALL_DASHCATCH2, false, 1);
                                    //playSpecialAnim(FieldingMechanism._GROUNDBALL_DASHCATCH2+FieldParm._dir[nFielderDir],false,1);
                                }
                                bNoDirChange = true;
                                setOffset(-18, false);
                                bDashCatched = true;
                                bDashCatchTry = false;
                                curTime = 0;
                            }
                            else
                            {

                                //////UnityEngine.//Debug.Log("===============>>44444444444444444444?");
                                //각 상황에 맞는 거 만들어서 이럴 뻘짓 하지 말것
                                if (nFielderDir == FieldParm._SOUTH || nFielderDir == FieldParm._SOUTHEAST || nFielderDir == FieldParm._SOUTHWEST)
                                {
                                    nFielderDir = FieldingMechanism.getDashDir(posIndex);//getDashDirException(nFielderDir);
                                    setNextMove(FieldingMechanism._GROUNDBALL_DASHCATCH2, false, 1);
                                    bNoDirChange = true;
                                }
                                else if (nFielderDir == FieldParm._NORTH)
                                {
                                    setNextMove(getRunIndexStr(), false, Mode.bAutoPlay ? 0.4f : 0.72f);
                                }
                                else
                                {
                                    //나중에 고쳐
                                    dstOffsetX = (nFielderDir == FieldParm._EAST ? (FieldingMechanism.MOVING_GROUNDER2_OFFSET_FORE) : (FieldingMechanism.MOVING_GROUNDER2_OFFSET_BACK)); //옵셋
                                    nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                                    setNextMove((field.ball.nBallZ > FieldParm.STANDINGCATCH_HEIGHT ? FieldingMechanism._GROUNDBALL_FORE_BACK_HIGH : FieldingMechanism._GROUNDBALL_FORE_BACK2), false, 0.6f, true, true);
                                }

                                bDashCatched = true;
                                curTime = 0;
                            }
                        }
                    }
                    else
                    {
                        ////UnityEngine.//Debug.Log("====================>>여기여기여기111111111111 bAnimation = " + bAnimation);
                        if (bAnimation == false)
                        {
                            ////UnityEngine.//Debug.Log("====================>>ballZ = " + field.ball.nBallZ);
                            setNextMove(getCenterCatchStr(), false, 0);
                        }
                        else
                        {
                            //높은 볼 //정면 땅볼잡기
                            if (field.ball.nBallZ > FieldParm.JUMPCATCH_HEIGHT)
                            {
                                if (checkBoundJump() == true) ////DONE
                                {
                                    if (skillJumpCatchLevel > 0) //점핑캐치
                                    {
                                        if (specialCatchSuccess == true)
                                        {
                                            //제5의내야수 or 철벽수비 연출 (점프캐치)
                                            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, posIndex == CPlayer._PITCHER ? SkillIndex.PitcherJumpCatch : SkillIndex.SpecialCatch);

                                            //내야수 스페셜 캐치 성공
                                            setCatch(false, true);
                                            catchDelayRate = 1.5f;                                            
                                        }
                                        else
                                        {
                                            setNoCatchCollider();
                                        }
                                        nFielderDir = FieldingMechanism.getJumpCatchException(nFielderDir, posIndex);
                                        setNextMove(FieldingMechanism._FLYBALL_JUMPING, false, 1);
                                    }
                                }
                                else
                                {
                                    setNextMove(FieldingMechanism._GROUNDBALL_CENTER_HIGH, false, 1);
                                }
                            }
                            else if (field.ball.nBallZ > FieldParm.STANDINGCATCH_HEIGHT)
                            {
                                //추후수정
                                ////UnityEngine.//Debug.Log("====================>>서서 땅볼 잡기 인데 현재는 점핑캐치 애니메이션 들어가 있음");
                                setNextMove(FieldingMechanism._GROUNDBALL_CENTER_HIGH, false, 1);
                            }
                            else
                            {
                                //UnityEngine.//Debug.Log("====================>>ballZ = " + field.ball.nBallZ);
                                setNextMove(getCenterCatchStr(), false, 1);
                            }
                        }
                        bDashCatched = false;
                        bChase = false;
                    }
                }
            }

            //다른 플레이어 상황에 따른 정지
            for (int i = 0; i < 9; i++)
            {
                if (field.fielder[i].actState == FielderAction._FIELDING)
                {
                    if (i != posIndex)
                    {
                        if (posIndex < CPlayer._LEFTFIELDER)
                        {
                            if (i < CPlayer._LEFTFIELDER)
                            {
                                field.fielder[i].setSecondMove();
                            }
                        }
                        else
                        {
                            //////UnityEngine.//Debug.Log("===============>>posIndex = " + posIndex);
                            if (i >= CPlayer._LEFTFIELDER)
                            {
                                field.fielder[i].setStop();
                            }
                        }
                    }
                }
            }


            if (bDashCatched == false)
            {
                ////////UnityEngine.//Debug.Log("===============>>여기냐2222?");
                setState(FielderAction._CATCHING, ActionStep._CATCHING_READY);// actState = FielderAction._CATCH_READY;
                curTime = 0;
                dX = 0;
                dY = 0;

                if (bChase == false)
                {
                    if (bFlyball == true)
                    {
                        nFielderDir = FieldingMechanism.getFlyballDirException(nFielderDir);
                    }
                    else
                    {
                        nFielderDir = FieldingMechanism.getFielderCatchReadyDir(posIndex, field.ball.firstAngle);
                    }
                }
            }
            else
            {
                //////UnityEngine.//Debug.Log("===============>>여기냐? zzzzzzz = nFielderDir = " + nFielderDir);
                setState(FielderAction._CATCHING, ActionStep._CATCHING_DASH);// //actState = FielderAction._CATCH_DELAY;
                curTime = 0;

                dX = speed * Mathf.Cos(FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad);
                dY = speed * Mathf.Sin(FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad);
            }


            //strID = _strID + FieldParm._dir[nFielderDir];
        }

        
        //////////////////////////////////////////////////////////////////
        //2. 투수 세팅
        //////////////////////////////////////////////////////////////////
        //투수의 견제 상태를 세팅
        public void setPickOffState(int target)
        {
            //Debug.Log("===================>>견제용 테스트용 랜덤 : " + Random.Range(0.0f, 100.0f));

            actState = FielderAction._PICKOFF;
            Runner pickOffRunner = null;
            if (target != -1)
            {
                pickOffRunner = field.run.getRunner(target);
            }

            field.bPickOffOut = false;
            if (pickOffRunner != null)
            {
                //////
                /*if ((field.manager.bMyTurn == false && Mode.bAutoPlay == false) ||
                    (Mode.bPvpMode433 == true))//(Mode.bPvpMode == true))
                {
                    if (field.pickOffCount < 3)
                    {
                        //수동은 여기서 체크해줌
                        //Debug.Log("===================>>견제용 테스트용 랜덤 한번더: " + Random.Range(0.0f, 100.0f));
                        field.run.pickoffState = SimulSteal.getPickOffResultMyControl(pickOffRunner.pRunner, field.pitcher.pPitcher, field.run.stealResult);
                    }
                }*/                

                /*//총체적인 결과는 이 아래에서 체크
                if (field.run.pickoffState == SimulPickOffState.VsSkill)
                {
                    //VS모드                    
                    errorForceInit();
                    //주루센스 vs 광속견제
                    //Debug.Log("====================>>> 대결연출 임시 : 주루센스(리드) vs 광속견제");
                    bool bOffenseWin = field.setVsSkill(pickOffRunner.pRunner, field.pitcher.pPitcher, SkillIndex.RunnerLead, SkillIndex.LaserPickOff, 0.01f, 2.0f);                    
                    field.bPickOffOut = !bOffenseWin;
                }
                else if(field.run.pickoffState == SimulPickOffState.LaserPickOff)
                {
                    //광속견제
                    errorForceInit();
                    field.bPickOffOut = true;
                    //견제왕 - 광속견제 연출
                    fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.LaserPickOff);
                }
                else if (field.run.pickoffState == SimulPickOffState.LeadDefense)
                {
                    //리드 디펜스
                    field.bPickOffOut = false;
                    //주루센스 - 리드 연출
                    fieldSkillDisplayManager.AddSkill(pickOffRunner.gameObject, pFielder, SkillIndex.RunnerLead);
                }
                else if (field.run.pickoffState == SimulPickOffState.Success)
                {
                    field.bPickOffOut = true;
                }
                else
                {
                    field.bPickOffOut = false;
                }*/

                //pvp모드에서 작동안함
                field.run.pickoffState = SimulPickOffState.Fail;
                field.bPickOffOut = false;
            }


            if (field.manager.bMyTurn == true || Mode.bPvpMode433 == true)
            {
                field.run.stealResult = SimulStealState.NONE;
            }

            curTime = 0;
        }

        //////////////////////////////////////////////////////////////////
        //3. 포수 세팅
        //////////////////////////////////////////////////////////////////
        //포수의 홈 충돌 상태 세팅
        public void setCatcherCollision(bool bSuccess)
        {            
            if (bSuccess)
            {
                //블록 성공
                setState(FielderAction._COLLISION, ActionStep._CATCHER_BLOCK);
                nFielderDir = FieldParm._WEST;
                //setNextMove(getHoldIndexStr(), true, 1);
                playSpecialAnim("9702_CATCHER_BLOCK_WIN", false);
                throwAgainState = FieldParm.ThrowAgain.NoThrow;
                tagState = -1;
                field.returnCheck(-4);
            }
            else
            {
                //블록 실패
                setState(FielderAction._COLLISION, ActionStep._CATCHER_CRUSHED);
                nFielderDir = FieldParm._NORTHWEST;
                if (Util.GetPercent(50) == true)
                {
                    dX = -1 * FIELDER_SPEED * Mathf.Cos(FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad);
                    dY = -1 * FIELDER_SPEED * Mathf.Sin(FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad);
                    playSpecialAnim("9702_CATCHER_BLOCK_LOSE1", false);
                }
                else
                {
                    dX = -0.9f * FIELDER_SPEED * Mathf.Cos(FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad);
                    dY = -0.3f * FIELDER_SPEED * Mathf.Sin(FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad);
                    playSpecialAnim("9702_CATCHER_BLOCK_LOSE2", false, 1, false);
                }
                throwAgainState = FieldParm.ThrowAgain.NoThrow;
                field.returnCheck(-4);
            }
            curTime = 0;
        }

        //포수의 도루 송구 상태를 세팅
        public void setLongTagState()
        {            
            actState = FielderAction._CATCHER_LONG_TAG;
            longTagDelay = FieldingMechanism.DELAY_LONGTAG;

            //bool bDelayHomeStealHappen = field.run.checkDelayHomeSteal();
            //UnityEngine.//Debug.Log("=======================>>딜레이 홈스틸 여부 :" + bDelayHomeStealHappen);

            if (field.run.stealResult == SimulStealState.VsSkill)
            {
                //질주본능 vs 수비형포수
                //Debug.Log("====================>>> 대결연출 임시 질주본능(대도) vs 수비형포수(앉아쏴)");                
                bool bOffenseWin = field.setVsSkill(field.run.stealRunner.pRunner, pFielder, SkillIndex.RunnerStealMaster, SkillIndex.CatcherSitThrow, 0.01f, 2.0f);
                field.bVsShow = true;
                field.run.stealResult = (bOffenseWin ? SimulStealState.Success : SimulStealState.Fail);
            }

            if (field.run.stealResult == SimulStealState.Success || field.run.stealResult == SimulStealState.Success_Skill)// && bDelayHomeStealHappen == false)
            {
                field.stealSuccess = true;
                longTagDelay = FieldingMechanism.DELAY_LONGTAG + 0.07f;
            }
            else
            {
                field.stealSuccess = false;                
            }

            curTime = 0;
        }

        //홈스틸 세팅
        private void setHomeStealTag()
        {
            //yield return new WaitForSeconds(delay);

            field.nTargetIndex = FieldParm.HOMEBASE_INDEX;
            field.ball.setThrowBallCatchedTagReady(posIndex);
            setState(FielderAction._THROWING_CATCH, ActionStep._TAGGING);// actState = FielderAction._TAGGING;

            nFielderDir = FieldParm._NORTH;
            setTagAnim();
            nextDelayAniTime = 2.0f;

            moveStep = 0;
            curTime = 0;
        }


        //////////////////////////////////////////////////////////////////
        //4. 야수 세팅
        //////////////////////////////////////////////////////////////////
        //공 잃어 버림
        public IEnumerator setSerchBall(bool bWildPitch, float delay, bool bCatcherBlockSuccess)
        {
            bCatchErrorFlag = false;    //캐치에러 무효화시킴
            if (bWildPitch == true)
            {
                if (field.wildPitchCase != FieldParm.WildPitchCase.BaseOnBall)
                {
                    field.setZoomTo(1, 1); //포수의 폭투 놓친 경우
                }
            }
            else
            {
                bErrorFieldingFirstCheck = false;
                field.ball.step = BallStep.BALL_ERROR_STATE;
                StartCoroutine(setErrorCaseFormation2(1.0f));
            }

            //errorMark.GetComponent<Renderer>().enabled = true;

            StartCoroutine(setErrorMark(0.5f));

            actState = FielderAction._COLLISION;


            //여기부터
            if (posIndex == CPlayer._CATCHER)
            {
                nFielderDir = FieldParm._NORTH;
            }
            setForcedAnim(getHoldIndexStr());

            yield return new WaitForSeconds(delay);

            if (bCatcherBlockSuccess == false)
            {
                nFielderDir = FieldParm._EAST;

                yield return new WaitForSeconds(delay);
                nFielderDir = FieldParm._WEST;

                yield return new WaitForSeconds(delay);
                nFielderDir = FieldParm._SOUTH;

                yield return new WaitForSeconds(delay);
            }
            //여기까지는 -> 나중에 애니메이션으로 대체

            errorMark.gameObject.SetActive(false);// errorMark.GetComponent<Renderer>().enabled = false;

            if (bWildPitch == true)
            {
                setBallChase();
                if (field.wildPitchCase == FieldParm.WildPitchCase.BaseOnBall)
                {
                    
                }
                else
                {
                    
                }
            }
            else
            {
                field.ball.nFirstBoundX = field.ball.nBallX + (0.3f * field.ball.nBallDX);
                field.ball.nFirstBoundY = field.ball.nBallY + (0.3f * field.ball.nBallDY);
                setBallChase();
            }

        }


        //강한 송구를 해야 할지 여부를 체크
        private bool checkStrongThrow()
        {
            if (posIndex >= CPlayer._LEFTFIELDER)  // && field.nTargetIndex != FieldParm.RELAY_INDEX)
            {
                if (field.nTargetIndex != FieldParm.RELAY_INDEX) return true;
                if (bOverHead == true) return true;
                if (field.ball.bFenceCol == true) return true;
            }
            else
            {
                if (posIndex == field.nRelayFielderIndex && field.nTargetIndex == FieldParm.HOMEBASE_INDEX)
                {
                    return true;
                }
            }

            return false;
        }

        //더블 플레이인지 여부를 체크
        private bool checkDoublePlayStop()
        {
            if (posIndex == CPlayer._SECONDBASEMAN || posIndex == CPlayer._SHORTSTOP)
            {
                //발생하여
                if(field.runnerDPStop == FieldSkillUse.Success)
                {
                    //주자가 병살저지 성공
                    ////UnityEngine.//Debug.Log("====================>>주자가 병살저지 성공");
                    setDoublePlayStop();
                    CameraManager.FieldCameraShake(0.2f, 10);
                    return true;
                }
                else if (field.runnerDPStop == FieldSkillUse.Fail)
                {
                    //주자가 병살저지 실패
                    ////UnityEngine.//Debug.Log("====================================================================>>주자가 병살저지 실패");
                    //병살회피
                    if (skillDashThrowLevel > 0)
                    {
                        throwType = ThrowType._INFIELD_SIDE_DASH;
                        throwReadyDelay = FieldingMechanism.DELAY_THROW_SIDEDASH;
                        //주루센스 - 병살저지 연출
                        fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.SpecialThrow);
                    }
                    else
                    {
                        throwType = ThrowType._INFIELD_DODGE;
                        throwReadyDelay = FieldingMechanism.DELAY_THROW_JUMPING;
                    }

                }
            }
            return false;
        }

        //더블 플레이인지 여부를 체크 - type2
        private bool checkDoublePlayStop2()
        {
            if (posIndex == CPlayer._SECONDBASEMAN || posIndex == CPlayer._SHORTSTOP)
            {
                //발생하여
                if (field.runnerDPStop == FieldSkillUse.Success)
                {
                    //주자가 병살저지 성공
                    ////UnityEngine.//Debug.Log("====================>>주자가 병살저지 성공");
                    setDoublePlayStop();
                    return true;
                }
                else if (field.runnerDPStop == FieldSkillUse.Fail)
                {
                    //주자가 병살저지 실패
                    ////UnityEngine.//Debug.Log("====================>>주자가 병살저지 실패");
                    //병살회피
                    //field.manager.gameUI.setSkillBox(true, "회피"); //[UI]필드에서 병살회피 스킬 이름 박스
                    setState(FielderAction._MOTION, ActionStep._MOTION_SET);
                    return true;
                }
            }
            return false;
        }
        
        //병살저지 스킬에 의해 2루수 혹은 유격수가 저지되는 상태를 세팅
        public void setDoublePlayStop()
        {
            //병살저지 회피 실패
            ////UnityEngine.//Debug.Log("=====================>>병살저지에의한 야수 충돌 애니메이션 posIndex = " + posIndex);
            field.runnerDPStop = FieldSkillUse.Init;
            setState(FielderAction._COLLISION, ActionStep._DOUBLEPLAY_STOP);
            playSpecialAnim("0301_2B_CRASH", false, 1, true);
            throwAgainState = FieldParm.ThrowAgain.NoThrow;
            nFielderDir = FieldParm._EAST;
            dX = -0.5f * FIELDER_SPEED * Mathf.Cos(FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad);
            dY = -0.5f * FIELDER_SPEED * Mathf.Sin(FieldParm._angleDir[nFielderDir] * Mathf.Deg2Rad);


        }

        //스킬 발동에 의한 동작후 그것을 끝내는 상태를 세팅
        private void setSpecialMoveEnd(float setTime)
        {
            if (field.nCatchIndex == posIndex)
            {
                //////UnityEngine.//Debug.Log("==================>>여기로 들어옴?   " + posIndex);
                //setStop(true);
                setStop();
                setState(FielderAction._MOTION, ActionStep._MOTION_SET);// actState = FielderAction._THROW_READY;
                curTime = setTime;

            }
            else
            {
                setStop();
            }
            //field.setTimeScale(Field.INIT_TIME_SCALE);

        }

        //토스캐칭시 스피드 재설정
        public void setNewSpeedByFrame(float time, float dstPosX, float dstPosY)
        {
            float dXDash = (dstPosX - posX) / time;
            float dYDash = (dstPosY - posY) / time;

            float speedDash = Mathf.Sqrt(dXDash * dXDash + dYDash * dYDash);
            float ratio = FIELDER_SPEED / speedDash;

            dX = dXDash * ratio;
            dY = dYDash * ratio;

            setThrowingCatchCollider(true);
        }

        //볼운반 상태 세팅
        public void setCarry(int baseIndex)
        {
            field.run.bOnlyOneBaseFlag = true;
            field.setTimeScale(Field.INIT_TIME_SCALE);
            field.ball.step = BallStep.BALL_CARRY;
            field.ball.setParticleDraw(false);
            field.bThrowing = false;
            field.bRelaying = false;
            field.nCarrierIndex = posIndex;

            setBaseCover(baseIndex);
            bTossTaked = true;
            curTime = 100;            
            field.ball.cameraWork = CameraWork.Default;

            //field.setZoomCameraSetting(false);
            field.setZoomTo(1, 0.5f); //야수의 볼 운반시
        }
        
        //필딩 타임을 세팅
        void setFiedingTime()
        {
            fieldingTime = (FieldingMechanism.getDistance(posX, dstX, posY, dstY) / speed); //(getDistance(posX, dstX, posY, dstY) / speed);
            if (bGrounderAvail == true)
            {
                if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING)
                {
                    fieldingTime -= FieldingMechanism.TIME_FORE_BACK_CATCH;
                }
                else if (grounderCatchType == GrounderCatch.GROUNDERCATCH_DIVING)
                {
                    if (posIndex == CPlayer._SHORTSTOP || posIndex == CPlayer._SECONDBASEMAN) fieldingTime -= FieldingMechanism.TIME_SLIDING_CATCH2;
                    else fieldingTime -= FieldingMechanism.TIME_SLIDING_CATCH;
                }

                else if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING_NORMAL)
                {
                    fieldingTime -= FieldingMechanism.TIME_GROUNDER_MOVING_CATCH;
                }
            }
            else if (bFlyCatchAvail)
            {
                if (flyballCatchType == FlyCatch.FLYCATCH_SLOWMOVE)
                {
                    ////////UnityEngine.//Debug.Log("==============>>여기??");
                    fieldingTime -= FieldingMechanism.TIME_SLOWMOVE_START;
                }
                else if (flyballCatchType == FlyCatch.FLYCATCH_FULLSPEED)
                {
                    fieldingTime -= FieldingMechanism.TIME_RUNNING_CATCH;
                }
                else if (flyballCatchType == FlyCatch.FLYCATCH_DIVING)
                {
                    fieldingTime -= FieldingMechanism.TIME_SLIDING_CATCH;
                }
                else if (flyballCatchType == FlyCatch.FLYCATCH_JUMPING)
                {
                    fieldingTime = 0;
                }
                else if (flyballCatchType == FlyCatch.FLYCATCH_DASH_FOR_ASSIST)
                {
                }

            }
        }
        
        //필딩상태: ready
        public void setFieldingReady()
        {
            setState(FielderAction._FIELDING, ActionStep._FIELDING_READY); //actState = FielderAction._FIELDING_READY;
            setNextMove(getHoldIndexStr(), true, 1);
            nFielderDir = FieldParm.getDir(angleDir);//getDir();
            //strID = _strID + FieldParm._dir[nFielderDir];
            //checkOverHead();
        }
        
        //필딩상태 : 플라이볼 상태
        public void setFlyBallFielding()
        {
            //UnityEngine.//Debug.Log("===================>>setFlyBallCatchMove Index: " + posIndex);

            setState(FielderAction._FIELDING, ActionStep._FIELDING_MOVE); //actState = FielderAction._FIELDING;

            dstX = field.ball.nFirstBoundX;
            dstY = field.ball.nFirstBoundY;// -offset;

            if (flyballCatchType == FlyCatch.FLYCATCH_FULLSPEED
             || flyballCatchType == FlyCatch.FLYCATCH_DIVING
             || flyballCatchType == FlyCatch.FLYCATCH_SLOWMOVE)
             //|| flyballCatchType == FlyCatch.FLYCATCH_HOMERUNSTEAL)
            {
                //////UnityEngine.//Debug.Log("===================>>홈런체크 여기2!!!!!");
                flyRemainingTime = (field.ball.firstBoundTime - field.ball.curTime) - 0.05f;// -0.1f;
                speed = distanceToBall / flyRemainingTime;
            }
            else
            {
                ////////UnityEngine.//Debug.Log("==================>>>setFlyBallCatchMove Index: " + posIndex);
                speed = (distanceToBall / (remainTime)) * 1.2f;
                if (speed > FIELDER_SPEED) speed = FIELDER_SPEED;
                else if (speed < FIELDER_SPEED * 0.5f) speed = FIELDER_SPEED * 0.5f;
            }


            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);


            dY = speed * Mathf.Sin(angleDir);
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);//getDir();

            if (bFlySlowMove == true)
            {
                //방향 예외처리
                setNextMove(getRunIndexStr(), true, 0.72f);//Mode.bAutoPlay ? 0.4f : 0.72f);
            }
            else if (bFlyFastMove == true)
            {
                setNextMove(FieldingMechanism._FLYBALL_FASTMOVE, true, 0.6f);
                if (flyballCatchType == FlyCatch.FLYCATCH_DIVING)
                {
                    setFlyDivingOffsetX(nFielderDir);
                }
            }
            else
            {
                setNextMove(getRunIndexStr(), true, 0.72f);//Mode.bAutoPlay ? 0.4f : 0.72f);
            }

            /*
            if (bSpecialCase == true)
            {
                StartCoroutine(specialFlyCase());
            }*/

            //strID = _strID + FieldParm._dir[nFielderDir];
        }

        //필딩상태 : 딥 플라이볼 상태
        public void setDeepFlyFielding(bool bFlyCover)
        {
            ////////UnityEngine.//Debug.Log("===================>>setFlyBallCatchMove Index: " + posIndex);

            setState(FielderAction._FIELDING, ActionStep._FIELDING_MOVE); //actState = FielderAction._FIELDING;

            dstX = field.ball.nFirstBoundX;
            dstY = field.ball.nFirstBoundY;


            if (bFlyCover == true)
            {
                speed = FIELDER_SPEED * 0.8f;
                setNextMove(getRunIndexStr(), true, 0.72f);//Mode.bAutoPlay ? 0.4f : 0.72f);
            }
            else
            {
                speed = FIELDER_SPEED;
                setNextMove(getRunIndexStr(), true, 0.72f);//Mode.bAutoPlay ? 0.4f : 0.72f);
            }
            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            dY = speed * Mathf.Sin(angleDir);
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);//getDir();
            //strID = _strID + FieldParm._dir[nFielderDir];
        }


        private IEnumerator setGrounderTry(float delay)
        {
            setStop();
            yield return new WaitForSeconds(delay);
            bGrounderTry = true;
            setGrounderFielding(true);
        }
        
        //필딩상태 : 그라운더 필딩
        public void setGrounderFielding(bool bTry)
        {
            ////////UnityEngine.//Debug.Log("===================>>setFlyBallCatchMove Index: " + posIndex);

            setState(FielderAction._FIELDING, ActionStep._FIELDING_MOVE); //actState = FielderAction._FIELDING;
            if (bTry)
            {
                dstX = tryDstX;
                dstY = tryDstY;
                speed = FIELDER_SPEED * 0.8f;
            }
            else
            {
                if (grounderCatchType == GrounderCatch.GROUNDERCATCH_DASH_FIRST)
                {
                    setBallChase(false);
                    //Debug.Log("posIndex = " + posIndex + " ==== grounderRemainTime = " + grounderRemainTime);
                    float timeValue = Mathf.Clamp(field.fastRemainTime * 0.7f, 1.0f, 2.2f);
                    nextDashTime = timeValue;
                    if (skillDashThrowLevel > 0)
                    {
                        setDashThrowSkillOn();
                    }                        
                    return;
                }
                else
                {
                    if (grounderCatchType == GrounderCatch.GROUNDERCATCH_NORMAL)// || grounderCatchType == GROUNDERCATCH_MOVING_NORMAL)
                    {
                        speed = (distanceToBall / remainTime) * 1.2f;
                        if (speed > FIELDER_SPEED) speed = FIELDER_SPEED;
                        else if (speed < FIELDER_SPEED * 0.7f) speed = FIELDER_SPEED * 0.7f;
                    }
                    else
                    {
                        speed = (distanceToBall / remainTime);
                    }
                }

            }



            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            dY = speed * Mathf.Sin(angleDir);
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);//getDir();
            //strID = _strID + FieldParm._dir[nFielderDir];

            /*     if (posIndex< CPlayer._LEFTFIELDER && (nFielderDir == FieldParm._EAST || nFielderDir == FieldParm._WEST))
                 {
                     //////UnityEngine.//Debug.Log("===================>>여기냐 posIndex = "+posIndex+"   dir = "+nFielderDir);
                     setNextMove(FieldingMechanism._GROUNDBALL_RUN, true, 1);
                 }
                 else*/
            {
                setNextMove(getRunIndexStr(), true, 0.72f);// Mode.bAutoPlay ? 0.4f : 0.72f);
            }
        }

        //필딩상태 : chase 상태
        public void setBallChase(bool bDash = false)
        {
            nextDashTime = 0;
            ////UnityEngine.//Debug.Log("====================>>setBallChase()   posIndex = " + posIndex);
            setState(FielderAction._FIELDING, ActionStep._CHASE);// actState = FielderAction._BALL_CHASE;
            bForcedThrow = true;
            if (bDash == false)
            {
                if (_strID != getRunIndexStr())
                {
                    setNextMove(getRunIndexStr(), true, 1);
                    nFielderDir = FieldParm.getDir(angleDir);
                }
            }
            else
            {                
                if (posIndex == CPlayer._CATCHER)
                {
                    setNextMove(FieldingMechanism._CATCHER_GROUNDBALL, false, 1);
                }
                else
                {
                    ////UnityEngine.//Debug.Log("===============>>여기냐?");
                    nFielderDir = FieldingMechanism.getDashDir(posIndex);// FieldingMechanism.getDashDirException(nFielderDir);
                    setNextMove(FieldingMechanism._GROUNDBALL_DASH, true, 1);// 0.6f);
                    setOffset(-18, false);
                    bNoDirChange = true;
                }
                //curOffsetX = -25.0f;
            }
        }

        //쓰로잉 캐치 상태 Setting
        int throwCatchDir;
        private void setThrowingCatchReady(ActionStep step)
        {
            setThrowingCatchCollider();

            if (posIndex == CPlayer._CATCHER)
            {
                //우선 임시
                //setNextMove(FieldingMechanism._CATCHER_HOLD, false, 0);
                nFielderDir = FieldingMechanism.getCatcherCatchException(field.curThrowIndex, field.ball.firstAngle);
                setNextMove(FieldingMechanism._CATCHER_BALL_CATCH, false, 0);
            }
            else
            {
                /*
                if (posIndex == CPlayer._CATCHER)
                {
                    nFielderDir = FieldingMechanism.getCatcherCatchException(field.curThrowIndex, field.ball.firstAngle);
                    setNextMove(FieldingMechanism._CATCHER_TAG, false, 0);
                }
                else*/
                {
                    if (step == ActionStep._FORCE_OUT)
                    {
                        setNextMove(FieldingMechanism._BALLCATCH_FORCEOUT, false, 0);
                    }
                    else if (step == ActionStep._TAG_OUT)
                    {
                        setNextMove(FieldingMechanism._BALLCATCH_NORMAL, false, 0);
                    }
                    else
                    {
                        setNextMove(FieldingMechanism._BALLCATCH_NORMAL, false, 0);
                    }
                }
            }

            dX = 0;
            dY = 0;
            setState(FielderAction._THROWING_CATCH, step);// actState = FielderAction._THROWING_CATCH;

            //nFieldState = 0xff;
            if (catchDir != -100)
            {
                nFielderDir = catchDir;
            }
            else
            {
                angleDir = Mathf.Atan2(field.ball.nBallY - posY, field.ball.nBallX - posX);
                nFielderDir = FieldParm.getDir(angleDir);//getDir();
            }
            strID = _strID + FieldParm._dir[nFielderDir];
            throwCatchDir = nFielderDir;

            //offset
            if (step == ActionStep._FORCE_OUT)
            {
                //포스아웃 옵셋
                posX += FieldParm.forceoutOffset[nFielderDir, 0];
                posY += FieldParm.forceoutOffset[nFielderDir, 1];
            }
            else if (step == ActionStep._TAG_OUT)
            {
                if (nCoveringIndex == FieldParm.SECONDBASE_INDEX)
                {
                    posY -= 23;
                    posX += (field.bFieldPickOffFlag == true ? 20 : -30);
                }
                else if (nCoveringIndex == FieldParm.THIRDBASE_INDEX)
                {
                    posY -= 45;
                }
                else if (nCoveringIndex == FieldParm.HOMEBASE_INDEX)
                {
                    posY -= 20;
                }
                else
                {
                    posX += FieldParm.forceoutOffset[nFielderDir, 0];
                    posY += FieldParm.forceoutOffset[nFielderDir, 1];
                }
            }

            curTime = 0;
        }

        //////////////////////////////////////////////////////////////////
        //5. 커버 백업 릴레이 Setting
        //////////////////////////////////////////////////////////////////
        //라운딩 레디
        private bool setRoundingReady(bool bPitcher)
        {
            //if (field.nFirstThrower == -100 || field.nFirstThrower >= CPlayer._LEFTFIELDER) return false;
            if (posIndex == CPlayer._PITCHER || field.nFirstThrower == -100) return false;



            if (bPitcher == true)
            {
                field.ball.bRounding = false;
                field.nFirstThrower = CPlayer._PITCHER;

            }
            else
            {
                field.ball.bRounding = true;
                if (posIndex == CPlayer._FIRSTBASEMAN)
                {
                    if (field.nFirstThrower == posIndex)
                    {
                        field.nFirstThrower = CPlayer._SECONDBASEMAN;
                    }
                }


            }

            setState(FielderAction._MOVE, ActionStep._MOVE_READY);

            setNextMove(getRunIndexStr(), true, 0.7f); //Mode.bAutoPlay ? 0.4f : 0.7f); 

            dstX = field.fielder[field.nFirstThrower].posX;
            dstY = field.fielder[field.nFirstThrower].posY;

            speed = FIELDER_SPEED * 0.5f;
            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            dY = speed * Mathf.Sin(angleDir);
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);//getDir();
            //strID = _strID + FieldParm._dir[nFielderDir];

            curTime = 20;

            bRoundingReady = true;
            field.returnCheckNC(0);
            return true;
        }
        
        //라운딩
        private void setRounding()
        {
            //라운딩용 애니메이션 필요
            float dstBaseX, dstBaseY;
            field.bThrowing = true;

            //////UnityEngine.//Debug.Log("=======================>>>nFirstThrower = " + field.nFirstThrower);
            dstBaseX = field.fielder[field.nFirstThrower].posX;
            dstBaseY = field.fielder[field.nFirstThrower].posY;
            field.fielder[field.nFirstThrower].setThrowingCatchReady(ActionStep._CATCH_NORMAL);
            angleDir = Mathf.Atan2(dstBaseY - posY, dstBaseX - posX);
            curTime = 0;

            dX = 0;
            dY = 0;
            setNextMove(FieldingMechanism._THROW_NORMAL, false, 1);
            actState = FielderAction._THROWING;

            nFielderDir = FieldParm.getDir(angleDir);//getDir();
            //strID = _strID + FieldParm._dir[nFielderDir];
            tStartZ = FieldParm.BALL_INIT_HEIGHT;
            bThrowErrorFlag = false;
            field.setThrowingVector(posX, posY, tStartZ, dstBaseX, dstBaseY, THROW_SPEED * 0.63f, 1, nFielderDir,false);

        }

        //이미 베이스를 차지하고 있는 경우 야수는 베이스를 비켜줌
        /*public bool setBaseLeave(int baseIndex)
        {
            if (baseIndex != FieldParm.HOMEBASE_INDEX)
            {
                if (actState != FielderAction._THROWING_CATCH)
                {
                    Runner destRunner = field.run.getDestRunner(baseIndex);
                    if (destRunner != null)
                    {
                        if ((destRunner.state == RunState.STANDBY && destRunner.currentPos == baseIndex) ||   //if (field.run.bOnBase[baseIndex] == true || (field.run.bOnRunning[baseIndex] == true && field.bOutofInfield == true))
                           (destRunner.state == RunState.MOVE && destRunner.basePositionRate() > 0.85f && field.bOutofInfield == true))
                        {
                            if (field.ball.step == BallStep.BALL_THROW && field.nTargetIndex == baseIndex)
                            {
                                return false;
                            }
                            else
                            {
                                if (bBaseLeave == false)
                                {
                                    ////UnityEngine.//Debug.Log("=========================================>> setBaseLeave 적용!!!");
                                    float angle = Mathf.Atan2(field.ball.nBallY - posY, field.ball.nBallX - posX);
                                    float dstX = posX + 40 * Mathf.Cos(angle);
                                    float dstY = posY + 40 * Mathf.Sin(angle);
                                    setBackupPosition(dstX, dstY, false);
                                    nCoveringIndex = baseIndex;
                                    bBaseLeave = true;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }*/

        //이미 베이스를 차지하고 있는 경우 야수는 주자와의 겹침을 피해 비켜줌
        private IEnumerator setBaseLeave(int baseIndex)
        {            
            bBaseLeave = true;

            yield return new WaitForSeconds(1.0f);

            bool bLeave = true;
            float dstX, dstY; 
            if (baseIndex == FieldParm.THIRDBASE_INDEX)
            {
                dstX = posX + 100;
                dstY = posY;
            }
            else if (baseIndex == FieldParm.SECONDBASE_INDEX)
            {
                dstX = posX + (posIndex == CPlayer._SECONDBASEMAN ? 100 : -50);
                dstY = posY;
            }
            else
            {
                dstX = posX;
                dstY = posY;
                bLeave = false;
            }

            if (bLeave == true)
            {
                setForcedAnim(getHoldIndexStr());
                setBackupPosition(dstX, dstY, false);
                nCoveringIndex = baseIndex;
                bSpecialAnim = false;
            }
        }



        //주자와의 충돌 상황 처리
        public void collideWithRunner()
        {
            if (bObjectInit == true)
            {
                if (bBaseLeave == false)
                {
                    if (bBaseCovering == true)
                    {
                        if (field.ball.step == BallStep.BALL_THROW_CATCH && posIndex == field.nCarrierIndex)
                        {
                            if (posIndex == CPlayer._THIRDBASEMAN ||
                                posIndex == CPlayer._SECONDBASEMAN ||
                                posIndex == CPlayer._SHORTSTOP)
                            {
                                StartCoroutine(setBaseLeave(nCoveringIndex));
                            }
                        }
                    }
                }
            }
        }



        //베이스 커버
        public void setBaseCover(int baseIndex)
        {
            ////UnityEngine.//Debug.Log("==============================>>>> SET BASE COVER");
            for (int i = CPlayer._PITCHER; i < CPlayer._SHORTSTOP; i++)
            {
                if (posIndex != i)
                {
                    if (field.fielder[i].bBaseCovering == true && field.fielder[i].nCoveringIndex == baseIndex)
                    {
                        field.fielder[i].bBaseCovering = false;
                        field.fielder[i].nCoveringIndex = -1;
                        field.fielder[i].setBackupPosition(0, 0, true);// setStop();
                    }
                }
            }

            setThrowingCatchCollider();
            bBaseCovering = true;
            nCoveringIndex = baseIndex;
            setState(FielderAction._MOVE, ActionStep._MOVE_READY);

            //setNextMove(FieldingMechanism._RUN, true, 1); //setNextMove(_WALK, true, 1);

            dstX = field.getOriginX(FieldSize.getBasePosX(baseIndex)) + FieldingMechanism.baseOffset[baseIndex, 0];
            dstY = field.getOriginY(FieldSize.getBasePosY(baseIndex)) + FieldingMechanism.baseOffset[baseIndex, 1];

            /*            
            float gabAngle = Mathf.Atan2(field.groundingDstY - dstY, field.groundingDstX - dstX);
            dstX += (20 * Mathf.Cos(gabAngle));
            dstY += (20 * Mathf.Sin(gabAngle));*/

            if (FIELDER_SPEED == 0)
            {
                FIELDER_SPEED = FieldingMechanism.BASIC_FIELDER_SPEED;
            }

            speed = FIELDER_SPEED * 1.2f;

            if (posIndex == CPlayer._PITCHER || posIndex == CPlayer._FIRSTBASEMAN)
            {
                //투수인 경우 빠르게
                speed = FIELDER_SPEED * 1.5f;
            }

            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            dY = speed * Mathf.Sin(angleDir);
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);//getDir();
            //strID = _strID + FieldParm._dir[nFielderDir];

            curTime = 0;
            fieldingTime = (FieldingMechanism.getDistance(posX, dstX, posY, dstY) / speed); //(getDistance(posX, dstX, posY, dstY) / speed);
        }
        //릴레이 
        public void setRelayPosition(int dstBase, float _xPos, float _yPos)
        {
            if (field.bGrounderAvailble == true) return;

            ////////UnityEngine.//Debug.Log("==================>>setRelayPosition Start dstBase = "+dstBase);
            if (dstBase <= FieldParm.SECONDBASE_INDEX) dstBase = FieldParm.SECONDBASE_INDEX;
            else if (dstBase > FieldParm.HOMEBASE_INDEX) dstBase = FieldParm.HOMEBASE_INDEX;

            if (bFlyCatchAvail == true ||
                flyballCatchType == FlyCatch.FLYCATCH_FULLSPEED ||
                flyballCatchType == FlyCatch.FLYCATCH_DIVING)
            {
                return;
            }

            if (bGrounderAvail == true ||
               grounderCatchType == GrounderCatch.GROUNDERCATCH_DIVING ||
               grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING)
            {
                return;
            }

            ////////UnityEngine.//Debug.Log("==================>>setRelayPosition posIndex = " + posIndex);
            setState(FielderAction._MOVE, ActionStep._MOVE_READY);

            if (dstBase == FieldParm.THIRDBASE_INDEX && field.ball.firstAngle > -20)
            {
                dstBase = FieldParm.HOMEBASE_INDEX;
            }


            float baseX = field.getOriginX(FieldSize.getBasePosX(dstBase));
            float baseY = field.getOriginY(FieldSize.getBasePosY(dstBase));


            float throwX = _xPos;// field.ball.nFirstBoundX;// field.throwingX;
            float throwY = _yPos;// field.ball.nFirstBoundY;//field.throwingY;

            float minPosY = field.getOriginY(FieldSize.getFielderPosY(CPlayer._LEFTFIELDER));
            if (throwY < minPosY)
            {
                throwY = minPosY;
                float hX = field.getOriginX(FieldSize.getBasePosX(FieldParm.HOMEBASE_INDEX));
                float hY = field.getOriginX(FieldSize.getBasePosY(FieldParm.HOMEBASE_INDEX));
                if (hX == throwX)
                {
                    throwX = hX;
                }
                else
                {
                    float rx = _xPos - hX;
                    float ry = _yPos - hY;

                    throwX = ((rx / ry) * (throwY - hY)) + hX;
                }
            }


                        
            float rate = 0.4f;
            if (dstBase != FieldParm.SECONDBASE_INDEX) rate = 0.6f;// (field.bMoreDouble ? 0.7f : 0.6f);
            

            dstX = baseX + (throwX - baseX) * rate;
            dstY = baseY + (throwY - baseY) * rate;


            speed = FIELDER_SPEED*1.3f;//
            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            dY = speed * Mathf.Sin(angleDir);
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);
            curTime = 0;

            bRelayStart = true;
        }
        
        //백업 포지션 (포지션 기준)
        public void setBackupPosition(float x, float y, bool bInitPos = false)
        {
            bPositionCovering = true;
            setState(FielderAction._MOVE, ActionStep._MOVE_READY);

            //setNextMove(FieldingMechanism._RUN, true, 1); //setNextMove(_WALK, true, 1);
            if (bInitPos == false)
            {
                dstX = x;// field.getOriginX(x);
                dstY = y;// field.getOriginY(y);
            }
            else
            {
                dstX = originX;
                dstY = originY;
            }

            speed = FIELDER_SPEED;// *1.5f;
            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            dY = speed * Mathf.Sin(angleDir);
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);//getDir();
            //strID = _strID + FieldParm._dir[nFielderDir];

            curTime = 0;

            //field.bGrounderCoverReady = true;
        }
        
        //백업 포지션 (베이스 기준)
        public void setBackupPosition(int baseIndex)
        {
            /*    int fIndex;
                if (field.flyCatchFielder != -1)
                    fIndex = field.flyCatchFielder;
                else
                    fIndex = field.getCloseFielderIndex();

                float pX = field.fielder[fIndex].posX;
                float pY = field.fielder[fIndex].posY;*/

            float baseX = field.getOriginX(FieldSize.getBasePosX(baseIndex));
            float baseY = field.getOriginY(FieldSize.getBasePosY(baseIndex));


            float gabX = 0;// (pX - baseX) * 1.35f;
            float gabY = 0;// (pY - baseY) * 1.35f;


            if (baseIndex == FieldParm.FIRSTBASE_INDEX)
            {
                gabX = 130;
                gabY = -280;
            }
            else if (baseIndex == FieldParm.THIRDBASE_INDEX)
            {
                gabX = -130;
                gabY = -280;
            }
            else if (baseIndex == FieldParm.HOMEBASE_INDEX)
            {
                gabX = (field.ball.firstAngle > 0 ? 150 : -150);
                gabY = -355;
            }


            float x = baseX + gabX;
            float y = baseY + gabY;

            FIELDER_SPEED = FIELDER_SPEED * 1.5f;

            setBackupPosition(x, y);
        }
        
        //세컨드 무브 세팅 (백업, 커버, 등등)
        void setSecondMove()
        {
            //루상에 주자가 있는 경우 이게 필요없음
            //if (field.run.checkRunnerOnBase() == false && field.bGrounderAvailble == true) return;

            if (bSecondMove == false)
            {
                if (posIndex == CPlayer._FIRSTBASEMAN)
                {
                    ////UnityEngine.//Debug.Log("===========================>>aaaaaaaaaaaaaaaaaaa");
                    if (field.checkCoverNeeded(FieldParm.FIRSTBASE_INDEX, posIndex) == true)
                    {
                        setBaseCover(FieldParm.FIRSTBASE_INDEX);
                        return;
                    }
                }
                else if (posIndex == CPlayer._THIRDBASEMAN)
                {
                    if (field.checkCoverNeeded(FieldParm.THIRDBASE_INDEX, posIndex) == true)
                    {
                        setBaseCover(FieldParm.THIRDBASE_INDEX);
                        return;
                    }
                }
                else if (posIndex == CPlayer._SECONDBASEMAN || posIndex == CPlayer._SHORTSTOP)
                {
                    if (field.checkCoverNeeded(FieldParm.SECONDBASE_INDEX, posIndex) == true)
                    {
                        if (bSecondDelayCover == false)
                        {
                            setBaseCover(FieldParm.SECONDBASE_INDEX);
                            return;
                        }
                    }
                    else if (field.checkRelayNeeded() == true)
                    {
                        int curBase = field.run.getFirstRunnerDest(0, 10000) + (field.flyCatchAvaiableCount == 0 ? 1 : 0);
                        setRelayPosition(curBase, field.ball.nFirstBoundX, field.ball.nFirstBoundY);
                        return;
                    }
                }
                else if (posIndex == CPlayer._CATCHER)
                {
                    if (field.firstBallSpeed >= FieldingMechanism.CATCHER_FIELDING_BALLSPEED)
                    {
                        if (field.checkCoverNeeded(FieldParm.HOMEBASE_INDEX, posIndex) == true)
                        {
                            setBaseCover(FieldParm.HOMEBASE_INDEX);
                            return;
                        }
                    }
                }
                else if (posIndex == CPlayer._PITCHER)
                {
                    ////////UnityEngine.//Debug.Log("============================>> PITCHER setSecondMove");
                    if (field.checkCoverNeeded(FieldParm.FIRSTBASE_INDEX, posIndex) == true && field.ball.firstAngle < -25)
                    {
                        ////////UnityEngine.//Debug.Log("============================>> PITCHER 일루커버");
                        setBaseCover(FieldParm.FIRSTBASE_INDEX);
                        return;
                    }
                    else if (field.checkCoverNeeded(FieldParm.HOMEBASE_INDEX, posIndex) == true)
                    {
                        ////////UnityEngine.//Debug.Log("============================>> PITCHER 홈 커버");
                        if (field.bFoulFlyOut == false)
                        {
                            setBaseCover(FieldParm.HOMEBASE_INDEX);
                        }
                        return;
                    }
                    /*else if (field.checkCoverNeeded(FieldParm.THIRDBASE_INDEX, posIndex) == true && field.ball.firstAngle > 25)
                    {
                        ////////UnityEngine.//Debug.Log("============================>> PITCHER 삼루 커버");
                        setBaseCover(FieldParm.THIRDBASE_INDEX);
                        return;
                    }*/
                    else
                    {
                        ////////UnityEngine.//Debug.Log("============================>> PITCHER 그밖의 처리");
                        if (field.bGrounderAvailble == false)
                        {
                            int backupBase = field.run.getFirstRunnerDest(0, 10000) + (field.flyCatchAvaiableCount == 0 ? 1 : 0);
                            if (backupBase > FieldParm.SECONDBASE_INDEX)
                            {
                                if (backupBase >= FieldParm.HOMEBASE_INDEX)
                                {
                                    backupBase = FieldParm.HOMEBASE_INDEX;
                                }
                                else
                                {
                                    backupBase = FieldParm.THIRDBASE_INDEX;
                                }
                                setBackupPosition(backupBase);
                                //field.bGrounderCoverStart = true;
                                ////////UnityEngine.//Debug.Log("============================>> 백업할 베이스 = " + curBase);
                                return;
                            }
                        }
                    }
                }
                bSecondMove = true;
            }

            setStop();
        }


        //////////////////////////////////////////////////////////////////
        //6. CHECK 함수
        //////////////////////////////////////////////////////////////////

        private bool checkBoundJump()
        {
            if (Mathf.Abs(posY - originY) < 20 &&
                field.ball.firstAngleZ <= 30 &&
               (posIndex == CPlayer._PITCHER || posIndex == CPlayer._FIRSTBASEMAN || posIndex == CPlayer._THIRDBASEMAN))
            {
                return true;
            }
            return false;
        }

        //NextAction값 리턴
        public NextAction checkNextMove()
        {
            int outCount = field.manager.nOutCount;
            if (posIndex == CPlayer._FIRSTBASEMAN)
            {
                /*라운딩 조건 주석 처리 - 이상함
                if (outCount <3)
                {
                    if (field.bAssist == true)
                    {
                        if (field.run.checkActiveRunnerOnBase() == false)
                        {
                            //1루수 라운딩 조건
                            //return NextAction._ROUNDING;
                        }
                    }
                }
                */
            }


            /*    if (posIndex <= CPlayer._SHORTSTOP)
                {
                    if (outCount < 3)
                    {
                        return NextAction._TO_PITCHER;
                    }
                }*/


            return NextAction._NONE;
        }

        //첫바운드포지션 기준으로 플라이볼을 잡을 수 있는지 여부 계산
        public bool checkFlyCatchAvailable()
        {
            checkOverHead();

            remainTime = field.ball.firstBoundTime - FIELD_DELAY;

            float possibleDis = 0;

            if (Mode.bPvpMode433 == true)
            {
                if (field.manager.bMyTurn == true)
                {
                    possibleDis = FIELDER_SPEED * remainTime;
                    distanceToBall = FieldingMechanism.getDistance(posX, field.ball.nFirstBoundX, posY, field.ball.nFirstBoundY - (field.ball.bHookorSlice ? 30 : 0)); //getDistance(field.ball.nFirstBoundX, field.ball.nFirstBoundY);

                    field.manager.Pvp_possibleDis[posIndex] = possibleDis;
                    field.manager.Pvp_distanceToBall[posIndex] = distanceToBall;
                }
                else
                {
                    possibleDis = field.manager.Pvp_possibleDis[posIndex];
                    distanceToBall = field.manager.Pvp_distanceToBall[posIndex];
                }
            }
            else
            {
                possibleDis = FIELDER_SPEED * remainTime;
                distanceToBall = FieldingMechanism.getDistance(posX, field.ball.nFirstBoundX, posY, field.ball.nFirstBoundY - (field.ball.bHookorSlice ? 30 : 0)); //getDistance(field.ball.nFirstBoundX, field.ball.nFirstBoundY);
            }


            flyTimeGab = ((distanceToBall / FIELDER_SPEED) - remainTime);

            catchDelayRate = 1.0f;
            throwDelayRate = 1;
            throwSpeedRate = 1;

            float flyCatchRatio = (distanceToBall / possibleDis);
            if (possibleDis >= distanceToBall)
            {
#if !NO_SPECIAL_MOVE
                if (field.ball.bHomeRunGuess == true)
                {
                    if (homerunStealActive == true)
                    {
                        //Debug.Log("===================>>홈런스틸 트라이 선체크 주자 스톱");
                        field.bHomerunStealTry = true;
                        bFlyCatchAvail = true;
                        return true;
                    }
                    return false;
                }
                else
                {
                    if (flyCatchRatio > 0.75f)
                    {
                        if (posIndex > CPlayer._CATCHER)
                        {
                            flyballCatchType = FlyCatch.FLYCATCH_SLOWMOVE;
                        }
                    }
                }
#endif
                //field.setFastFieldTime(field.ball.firstBoundTime);
                bFlyCatchAvail = true;
                bFlySlowMove = true;                
                return true;
            }
            else
            {
                //bool bTry = false;
                if (posIndex > CPlayer._CATCHER)
                {
#if !NO_SPECIAL_FLY_MOVE
                    //능력치
                    if (flyCatchRatio > 1)
                    {
                        //float range = 1.05f;
                        float range = FieldingMechanism.SPECIAL_FLYCATCH_MIN_VALUE;
                        if (skillRangeLevel > 0)
                        {
                            range += SkillParm.getOutfieldRangeOffset(skillRangeLevel); //1.05 ~ 1.35
                        }    

                        if (field.ball.bHomeRunGuess == true)
                        {
                            //홈런스틸 체크
                            if (homerunStealActive == true)
                            {
                                if (flyCatchRatio < range)
                                {
                                    //Debug.Log("===================>>홈런스틸 트라이 선체크 주자 스톱");
                                    field.bHomerunStealTry = true;
                                    bFlyCatchAvail = true;
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            
                            float divingRange = (range + FieldingMechanism.DIVING_CATCH_OFFSET); //기본 다이빙 레인지
                            if (skillDivingLevel > 0) 
                            {
                                divingRange += SkillParm.getSlidingRangeOffset(skillDivingLevel);
                            }

                            if (flyCatchRatio < range)
                            {
                                flyballCatchType = FlyCatch.FLYCATCH_FULLSPEED;
                                bFlyCatchAvail = true;
                                bFlyFastMove = true;
                                return true;
                            }
                            else if (flyCatchRatio < divingRange)//1.35f)
                            {
                                if (skillDivingLevel > 0) 
                                {                                    
                                    bDiveSuccess = specialCatchSuccess;
                                    flyballCatchType = FlyCatch.FLYCATCH_DIVING;
                                    bFlyCatchAvail = true;
                                    bFlyFastMove = true;
                                    return true;
                                }
                            }
                        }
                    }
#endif
                }
            }

            if (skillJumpCatchLevel > 0) //점핑캐치
            {   
                if (bOverHead == true)
                {
                    bool bJumpCatchSuccess = false;
                    if(posIndex == CPlayer._PITCHER)
                    {
                        //투수 점핑 캐치 체크
                        if (field.ballPower < 32 && Mathf.Abs(field.ball.firstAngle) < 3.1f && field.ball.firstAngleZ < 12.0f)
                        {
                            bJumpCatchSuccess = true;
                        }
                    }
                   /* else if(posIndex >= CPlayer._FIRSTBASEMAN && posIndex <= CPlayer._SHORTSTOP)
                    {
                        //내야수 점핑캐치 체크
                        bJumpCatchSuccess = checkJumpCatch();
                    }*/

                    if (bJumpCatchSuccess == true)
                    {
                        flyballCatchType = FlyCatch.FLYCATCH_JUMPING;
                        bFlyCatchAvail = true;
                        FIELD_DELAY = 0;
                        return true;
                    }
                }
            }


            if (bFlyCatchAvail == false)
            {
                if ((posIndex == CPlayer._SHORTSTOP && field.ball.firstAngle > 0)
                 || (posIndex == CPlayer._SECONDBASEMAN && field.ball.firstAngle < 0))
                {
                    if (field.ball.nFirstBoundY > posY)
                    {
                        if (possibleDis >= (distanceToBall * 0.5f))
                        {
                            bFlyCatchTry = true;
                        }
                    }
                }
            }

            return false;
        }

        private bool checkJumpCatch()
        {
            //////UnityEngine.//Debug.Log("============================>>hitBallMaxHeight = " + field.ball.hitBallMaxHeight);
            int heghtRange = SkillParm.getJumpHeightRange(skillJumpCatchLevel);    //max 70
            float slopeRange = SkillParm.getJumpSlopeRange(skillJumpCatchLevel);   //max 0.4

            //////UnityEngine.//Debug.Log("============================>>heght/max = " + field.ball.hitBallMaxHeight + "/" + (FieldingMechanism.RANGE_JUMPING_CHTCH_HEIGHT + heghtRange));

            if (field.ball.hitBallMaxHeight < (FieldingMechanism.RANGE_JUMPING_CHTCH_HEIGHT + heghtRange) && field.ballPower < 35)
            {
                float myDX = posX - field.homeX;
                float ballDX = field.ball.nFirstBoundX - field.homeX;

                if (Mathf.Sign(ballDX) != Mathf.Sign(myDX)) return false;
                if (myDX == 0) return false;
                if (ballDX == 0) return false;

                float myDY = posY - field.homeY;
                float ballDY = field.ball.nFirstBoundY - field.homeY;

                float mySlope = myDY / myDX;
                float ballSlope = ballDY / ballDX;

                //////UnityEngine.//Debug.Log("============================>>slope/max = " + Mathf.Abs(mySlope - ballSlope) + "/" + (FieldingMechanism.RANGE_JUMPING_CHTCH_SLOPE + slopeRange));

                if (Mathf.Abs(mySlope - ballSlope) < (FieldingMechanism.RANGE_JUMPING_CHTCH_SLOPE + slopeRange))
                {
                    //////UnityEngine.//Debug.Log("============================>>index = " + posIndex);
                    float b = field.homeY - (ballSlope * field.homeX);
                    posX = (posY - b) / ballSlope; //x포지션 재조정
                    return true;
                }

                //////UnityEngine.//Debug.Log("============================>>index = " + posIndex);
                //////UnityEngine.//Debug.Log("============================>>mySlope = " + mySlope);
                //////UnityEngine.//Debug.Log("============================>>ballSlope = " + ballSlope);
            }

            return false;
        }


        //첫바운드가 내 머리를 넘어갈지 여부
        public void checkOverHead()//DeepFly()
        {
            if (posIndex == CPlayer._CATCHER)
            {
                bOverHead = true;
            }
            else
            {
                float firstDis = FieldingMechanism.getDistance(field.ball.nFirstBoundX, field.homeX, field.ball.nFirstBoundY, field.homeY); //getDistance(field.ball.nFirstBoundX, field.homeX, field.ball.nFirstBoundY, field.homeY);
                float myDis = FieldingMechanism.getDistance(posX, field.homeX, posY, field.homeY); //getDistance(posX, field.homeX, posY, field.homeY);
                if (firstDis > myDis + 20) bOverHead = true;// return true;
                else bOverHead = false;// return false;
            }
        }

        /// <summary>
        /// 특급송구 여부
        /// </summary>
        /// <returns></returns>
        public bool checkSpecialThrowing()
        {
            if (skillQuickThrowLevel > 0 || skillSpinThrowLevel > 0 || skillJumpThrowLevel > 0 || skillDashThrowLevel > 0)
            {
                return true;
            }
            return false;
        }

        //펜스와 충돌 여부..
        //Moving, FIeldingMoving, Chasing경우
        private void checkFence()
        {
            //if (field.bInputWait == true) return;

            if (field.ball.step == BallStep.BALL_HIT && posIndex >= CPlayer._LEFTFIELDER)
            {
                //if (field.ball.bHomeRunGuess == true)
                {
                    if (screenY >= FieldSize.getFenceOriginY())
                    {
                        float nX = field.getScreenX(posX);
                        float nY = field.getScreenY(posY);

                        if (field.ball.fenceEquation(nX, nY + 15))
                        {
                            float distance = FieldingMechanism.getDistance(posX, field.ball.nFirstBoundX, posY, field.ball.nFirstBoundY);
                            //Debug.Log("야수인덱스 "+ posIndex +" :  첵크펜스 1 ====================>>홈런캐치 거리 측정 = "+distance);

                            if (distance < FieldingMechanism.HOMERUN_STEAL_DISTANCE &&
                                field.ball.bBound == false &&
                                (field.bHomerunStealTry == true || (homerunStealActive == true && bFlyCatchAvail == true)))
                            {
                                //Debug.Log("====================>>홈런 충돌시");
                                Debug.Log("Remain Time = " + (field.ball.firstBoundTime - field.ball.curTime));
                                float remainT = (field.ball.firstBoundTime - field.ball.curTime) - 0.18f;
                                StartCoroutine(setHomerunStealDelay(remainT));                                
                            }
                            else
                            {
                                setStop();
                                bFenceReady = true;
                            }
                        }
                    }
                }
            }
        }


        //_FLYBALL_SPECIAL
        //catchingDash
        private void checkFence2(bool bBoundBall = false)
        {
            if (posIndex >= CPlayer._LEFTFIELDER)
            {
                float nX = field.getScreenX(posX);
                float nY = field.getScreenY(posY);
                if (field.ball.fenceEquation(nX, nY + 15))
                {
                    float distance = FieldingMechanism.getDistance(posX, field.ball.nFirstBoundX, posY, field.ball.nFirstBoundY);
                    Debug.Log("야수인덱스 "+ posIndex + " :  첵크펜스 2 ====================>>홈런캐치 거리 측정 = " + distance);
                    if (distance < FieldingMechanism.HOMERUN_STEAL_DISTANCE &&
                        field.ball.bBound == false &&
                        (field.bHomerunStealTry == true || (homerunStealActive == true && bFlyCatchAvail == true)))
                    {
                        
                        //Debug.Log("====================>>홈런 충돌시");
                        Debug.Log("Remain Time = " + (field.ball.firstBoundTime - field.ball.curTime));
                        float remainT = (field.ball.firstBoundTime - field.ball.curTime) - 0.18f;
                        StartCoroutine(setHomerunStealDelay(remainT));
                    }
                    else
                    {
                        //Debug.Log("====================>>펜스 충돌 처리");
                        if (field.ball.bBallCatched == true && field.nThrowIndex == posIndex) //if (field.ball.step == BallStep.BALL_CATCH)
                        {
                            //공 잡은후 펜스와 충돌                            
                            StartCoroutine(setFenceColAfterCatch());
                        }
                        else
                        {
                            if (MyMath.Percent() < FieldingMechanism.TEAM_DEFENSE)
                            {
                                //충돌 피함 ㅋㅋ
                                StartCoroutine(setFencePlayFieder());
                            }
                            else
                            {
                                //충돌 못 피함 ..ㅋㅋ 등신
                                //펜스 충돌 처리
                                setNoCatchCollider();
                                field.bCrushDelay = true;
                                float cx = transform.position.x + (dX * 0.25f);
                                float cy = transform.position.y + (dY * 0.25f);
                                field.setFieldCollisionEffect(cx, cy);
                                setCollisionFielders(nFielderDir);
                            }
                        }
                    }
                }
            }
        }


        bool checkDoublePlay()
        {
            ////UnityEngine.//Debug.Log("====================>>checkDoublePlay");
            if (posIndex >= CPlayer._SECONDBASEMAN && posIndex <= CPlayer._SHORTSTOP)
            {
                if (field.run.bOnRunning[FieldParm.SECONDBASE_INDEX] == true && field.run.bOnRunning[FieldParm.FIRSTBASE_INDEX] == true)
                {
                    ////UnityEngine.//Debug.Log("====================>>checkDoublePlay TRUE");
                    return true;
                }
            }

            ////UnityEngine.//Debug.Log("====================>>checkDoublePlay FALSE");
            return false;
        }


        //////////////////////////////////////////////////////////////////
        //7. 기타 세팅
        //////////////////////////////////////////////////////////////////
        //죽어서 벤치감
        public void setBench()
        {
            setInningChange(false);
        }

        //이닝 체인지
        public void setInningChange(bool bDefence)
        {
            int[] fspeed = new int[9] { 3, 3, 4, 5, 5, 5, 6, 6, 6 };
            ////////UnityEngine.//Debug.Log("===================>>setInningChange");
            setState(FielderAction._MOVE, ActionStep._MOVING);
            setNextMove(getRunIndexStr(), true, posIndex <= 2 ? 0.7f : 1); //
            if (bDefence)
            {
                posX = field.nBenchFielderPosX;
                posY = field.nBenchFielderPosY;
                dstX = originX;
                dstY = originY;
            }
            else
            {
                dstX = field.nBenchFielderPosX;
                dstY = field.nBenchFielderPosY;
            }

            speed = fspeed[posIndex] * FBall._BALLSPEED_COEF;
            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            dY = speed * Mathf.Sin(angleDir);
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);// getDir();
            //strID = _strID + FieldParm._dir[nFielderDir];

            curTime = 0;
            fieldingTime = (FieldingMechanism.getDistance(posX, dstX, posY, dstY) / speed); //(getDistance(posX, dstX, posY, dstY) / speed);
        }

        //벤치상태: 목적지 설정
        public void setDstPos(float startX)
        {
            posX = startX;
            posY = FieldSize.getHomeBenchPosY();
            //lineX = posX;
            //lineY = posY;
        }



        //////////////////////////////////////////////////////////////////
        // StandBy State
        //////////////////////////////////////////////////////////////////
        //스탠바이 처리
        private void standbyFrame()
        {
            if (posIndex == field.nCatchIndex)
            {
                if (field.ball.step == BallStep.BALL_THROW_CATCH)
                {
                    if (aStep != ActionStep._TAGGING)
                    {
                        if (setRecheckThrow2() == true) return;
                    }
                }
            }

            if (bBaseCovering == true)
            {
                //이거정말
                if (nCoveringIndex == FieldParm.SECONDBASE_INDEX)
                {
                    if (posIndex == field.nCatchIndex)
                    {
                        if (checkDoublePlayStop2()) return;
                    }
                }

                baseCoverStandby();
            }
            else if (bRelayPositioning == true)
            {
                relayStandby();
            }
            
            else if (bPositionCovering == true)
            {
                if (bBaseLeave == true)
                {
                    if (nCoveringIndex >= 0)
                    {
                        if (field.run.bOnBase[nCoveringIndex] == false)
                        {
                            setBaseCover(nCoveringIndex);
                            bBaseLeave = false;
                        }
                    }
                }
            }



            if (bFenceReady == true)
            {
                if (field.ball.bFenceCol == true && field.ball.bBound == true)
                {
                    setBallChase();// actState = Fielder._BALL_CHASE;
                    for (int i = 0; i < 9; i++)
                    {
                        field.fielder[i].bFenceReady = false;
                    }
                }
            }
            else
            {
                if (field.bFieldViewActive == true)//if (field.manager.playState == PlayState.PLAY_FIELDING_VIEW)
                {
                    if (field.ball.eventStep != BallEvent.EVENT_HOMERUN)
                    {
                        if (posIndex != field.nCarrierIndex && bRelayPositioning == false)
                        {
                            angleDir = Mathf.Atan2(field.ball.nBallY - posY, field.ball.nBallX - posX);
                            nFielderDir = FieldParm.getDir(angleDir);//getDir();
                            strID = _strID + FieldParm._dir[nFielderDir];
                        }
                    }
                }
            }

        }
        //베이스커버 레디
        void baseCoverStandby()
        {
            if (field.ball.checkThrowingToThisBase(nCoveringIndex) == true)
            {
                ////UnityEngine.//Debug.Log("==============>> posIndex: " + posIndex + "=======>> catchReady");
                setThrowingCatchReady(field.run.bForceOutFlag[nCoveringIndex] ? ActionStep._FORCE_OUT : ActionStep._TAG_OUT);
                //setThrowingCatchReady(ActionStep._FORCE_OUT); // (field.run.bForceOutFlag[nCoveringIndex]?ActionStep._FORCE_OUT: ActionStep._TAG_OUT)

            }
            else
            {
                //if (setBaseLeave(nCoveringIndex) == false)
                //{

                    ////////UnityEngine.//Debug.Log("==============>> posIndex: " + posIndex + "=======>> nCoveringIndex = " + nCoveringIndex);
                    if (nCoveringIndex == FieldParm.FIRSTBASE_INDEX)
                    {
                        nFielderDir = FieldParm._WEST;
                    }
                    else if (nCoveringIndex == FieldParm.SECONDBASE_INDEX)
                    {
                        nFielderDir = FieldParm._SOUTH;
                    }
                    else if (nCoveringIndex == FieldParm.THIRDBASE_INDEX)
                    {
                        nFielderDir = FieldParm._EAST;
                    }
                    else //if (nCoveringIndex == FieldParm.HOMEBASE_INDEX)
                    {
                        nFielderDir = FieldParm._NORTH;
                    }
                    //strID = _strID + FieldParm._dir[nFielderDir];
                //}
            }
        }
        //릴레이 레디
        void relayStandby()
        {
            if (field.ball.checkThrowingToThisBase(FieldParm.RELAY_INDEX) == true)
            {
                ////////UnityEngine.//Debug.Log("==============>> posIndex: " + posIndex + "=======>> catchReady");
                setThrowingCatchReady(ActionStep._CATCH_NORMAL);
            }
        }
        

        //////////////////////////////////////////////////////////////////
        // MOVE State
        //////////////////////////////////////////////////////////////////
        //무브 상태 처리
        void moveFrame()
        {
            if (aStep == ActionStep._MOVING)
            {
                moving();
            }
            else if (aStep == ActionStep._MOVE_READY)
            {
                moveReady();
            }
            else if (aStep == ActionStep._AFTER_FORCEOUT)
            {
                afterForceout();
            }
        }
        //ActionStep._MOVING 스텝처리
        void moving()
        {
            posX += (dX * deltaTime);
            posY += (dY * deltaTime);

            if (posIndex == field.nCarrierIndex)
            {
                if (bBaseCovering == false)
                {
                    if (field.ball.step == BallStep.BALL_CARRY)
                    {
                        if (setRecheckThrow2() == true) return;
                    }
                }
            }

            if (bRoundingReady == true)
            {
                if (curTime >= (bRelayPositioning == true ? 1.0f : 0.5f))
                {
                    setRounding();
                }
            }
            else
            {
                checkFence();
                if (curTime >= fieldingTime)
                {
                    posX = dstX;
                    posY = dstY;
                    if (bBaseCovering == true)
                    {
                        field.setBaseCover(nCoveringIndex);
                        
                        if (posIndex == CPlayer._CATCHER && field.run.bHomeSteal == true && field.bFieldStealFlag == true)
                        {                            
                            setHomeStealTag();
                            return;
                        }
                        else if (bTossTaked == true || bThrowErrorCoverd == true)
                        {
                            field.ball.step = BallStep.BALL_THROW_CATCH;// BALL_CATCH;
                            field.run.bBallOnBase[field.nTargetIndex] = true;

                            if (bTossTaked == true) bTossTaked = false;
                            if (bThrowErrorCoverd == true) bThrowErrorCoverd = false;
                            
                            if (nCoveringIndex == FieldParm.FIRSTBASE_INDEX || field.manager.nOutCount >= 2)
                            {
                                setNextMove(getHoldIndexStr(), true, 1);
                            }

                            setNextMove(getHoldIndexStr(), true, 1); //이놈 주의
                            setState(FielderAction._MOTION, ActionStep._MOTION_SET);
                            return;
                        }
                    }
                    else if (bPositionCovering == true)
                    {
                        nFielderDir = FieldParm.getDir(angleDir);//getDir();
                    }
                    setStop();
                }

            }

        }
        //ActionStep._MOVE_READY 스텝처리
        void moveReady()
        {
            
            if (bPositionCovering == true)
            {
                if (curTime >= FieldingMechanism.DELAY_COVER)// && field.ball.nBallY > 2000)// FieldSize.getSecondBasePosY(field.fRatio) + 100)
                {
                    //////UnityEngine.//Debug.Log("========================>>posIndex = " + posIndex + "=================>>포지션 커버링 field.ball.nBallY = " + field.ball.nBallY);
                    //if (field.bGrounderCoverStart == true)
                    {
                        setNextMove(getRunIndexStr(), true, 0.72f); // Mode.bAutoPlay ? 0.4f : 0.72f); 
                        setState(FielderAction._MOVE, ActionStep._MOVING);
                        curTime = 0;
                        fieldingTime = (FieldingMechanism.getDistance(posX, dstX, posY, dstY) / speed);  //(getDistance(posX, dstX, posY, dstY) / speed);
                    }
                }
            }
            else if (bBaseCovering == true)
            {
                if (curTime >= FieldingMechanism.DELAY_COVER + (posIndex == CPlayer._PITCHER ? 0.2f : 0))
                {
                    setNextMove(getRunIndexStr(), true,  0.72f);//Mode.bAutoPlay ? 0.4f : 0.72f);
                    setState(FielderAction._MOVE, ActionStep._MOVING);
                    curTime = 0;
                }
            }
            else
            {
                if (curTime >= FIELD_DELAY)
                {
                    setNextMove(getRunIndexStr(), true, 0.72f);//Mode.bAutoPlay ? 0.4f : 0.72f);
                    setState(FielderAction._MOVE, ActionStep._MOVING);
                    curTime = 0;
                    fieldingTime = (FieldingMechanism.getDistance(posX, dstX, posY, dstY) / speed); //(getDistance(posX, dstX, posY, dstY) / speed);

                }
            }
        }
        //ActionStep._AFTER_FORCEOUT 스텝 처리
        void afterForceout()
        {
            if (moveStep == 0)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);

                if (curTime > 0.5f)
                {
                    curTime = 0;
                    dX = dY = 0;
                    setNextMove(FieldingMechanism._THROW_NORMAL, false, 1);
                    moveStep = 1;
                }
            }
            else if (moveStep == 1)
            {
                if (curTime > 0.5f)
                {
                    setStop();
                }
            }
        }

        //////////////////////////////////////////////////////////////////
        // Fielding State
        //////////////////////////////////////////////////////////////////
        //필딩 상태 처리
        void fieldFrame()
        {
            if (aStep == ActionStep._FIELDING_READY)
            {
                fieldReady();
            }
            else if (aStep == ActionStep._FIELDING_MOVE)
            {
                fieldingMove();
            }
            else if (aStep == ActionStep._CHASE)
            {
                chasingMove();
            }
            else if (aStep == ActionStep._GROUNDER_SPECIAL)
            {
                grounderSpecial();
            }
            else if (aStep == ActionStep._FLYBALL_SPECIAL)
            {
                flyballSpecial();
            }
        }
        //ActionStep._FIELDING_READY 스텝처리
        void fieldReady()//fieldReadyFrame()
        {
            nFielderDir = FieldingMechanism._InitFielderDir[posIndex];

            if (curTime >= FIELD_DELAY)
            {
                if (bFlyCatchAvail == true)
                {
                    setFlyBallFielding();
                }
                else if (bGrounderAvail == true)
                {
                    setGrounderFielding(false);
                }
                else if (bDeepFlyChase == true)
                {
                    setDeepFlyFielding(false);
                }
                else if (bFlyCoverChase == true)
                {
                    setDeepFlyFielding(true);
                }
                else if (bGrounderTry == true)
                {
                    if (posIndex == CPlayer._CATCHER)
                    {
                        setBallChase();
                    }
                    else if (posIndex == CPlayer._FIRSTBASEMAN && field.fielder[CPlayer._SECONDBASEMAN].bGrounderAvail == true)
                    {
                        field.fielder[CPlayer._PITCHER].setStop();
                        field.fielder[CPlayer._PITCHER].bBaseCovering = false;
                        setBaseCover(FieldParm.FIRSTBASE_INDEX);
                    }
                    else
                    {
                        setGrounderFielding(true);
                    }
                }
                else if (bFlyCatchTry == true)
                {
                    setDeepFlyFielding(false);
                }
                else //아무것도 안걸릴 경우
                {
                    if (posIndex > CPlayer._CATCHER)
                    {
                        setGrounderFielding(true);
                    }
                    else
                    {
                        setSecondMove();
                    }
                }

                curTime = 0;
                setFiedingTime();
            }
        }

        bool checkInfieldDash(float dis)
        {
            if (field.ball.firstAngleZ < -35 && dis > (FieldingMechanism.RANGE_OUTFIELDER_WAITING + 50))
            {
                bDashCatchTry = true;
                float deltaSpeed = field.ball.speed * deltaTime;
                if (deltaSpeed < FieldingMechanism.RANGE_IN_DASH_CHECK_SPEED)
                {
                    ////UnityEngine.//Debug.Log("=====================>>내야 DASH로 바뀜");
                    nFielderDir = FieldingMechanism.getDashDir(posIndex);// FieldParm.getDir((int)(field.ball.nBallX - posX), (int)(field.ball.nBallY - posY), 15);
                    nLastDir = -1;

                    setBallChase((skillSlidingCatchLevel > 0 || skillDashThrowLevel > 0) ? true : false);

                    bDashQuickThrow = false;

                    if (field.groundCatchFielder == posIndex && (posIndex >= CPlayer._SECONDBASEMAN || posIndex == CPlayer._PITCHER))
                    {
                        //대쉬앤 퀵 스로우 발동
                        if (skillDashThrowLevel > 0)
                        {
                            setDashThrowSkillOn();
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        //ActionStep._FIELDING_MOVE 스텝처리
        void fieldingMove()
        {
            posX += (dX * deltaTime);
            posY += (dY * deltaTime);

            checkFence();
            if (bFlyCatchAvail)
            {
                if (flyballCatchType == FlyCatch.FLYCATCH_NORMAL)
                {
                    if (moveStep == 0)
                    {
                        if (curTime >= fieldingTime - FieldingMechanism.TIME_SLOWMOVE_START)
                        {
                            nFielderDir = FieldingMechanism.getDirException(nFielderDir, dX, dY);//setDirException();

                            if (posIndex == CPlayer._CATCHER)
                            {
                                setNextMove(FieldingMechanism._CATCHER_FLYBALL_SLOWMOVE, true, 1);
                            }
                            else
                            {
                                setNextMove(FieldingMechanism._FLYBALL_SLOWMOVE, true, 1);
                            }
                            moveStep = 1;
                            nLastDir = -1;
                        }
                    }
                }

                if (curTime >= fieldingTime)
                {
                    if (flyballCatchType == FlyCatch.FLYCATCH_SLOWMOVE)
                    {
                        setFlyCollider();
                        bCatchErrorSpeicalAnimation = true;
                        setState(FielderAction._FIELDING, ActionStep._FLYBALL_SPECIAL);//actState = FielderAction._FLYBALL_SPECIAL;

                        nFielderDir = FieldingMechanism.getDirException(nFielderDir, dX, dY);//setDirException();
                        if (posIndex == CPlayer._CATCHER)
                        {
                            setNextMove(FieldingMechanism._CATCHER_FLYBALL_SLOWMOVE, true, 1);
                        }
                        else
                        {
                            setNextMove(FieldingMechanism._FLYBALL_SLOWMOVE, true, 1);
                        }
                        setFlyMoveOffsetX(nFielderDir);

                        nLastDir = -1;
                    }
                    else if (flyballCatchType == FlyCatch.FLYCATCH_FULLSPEED)// || flyballCatchType == FlyCatch.FLYCATCH_BACKWARD_JUMPING)
                    {
                        setFlyCollider();
                        bCatchErrorSpeicalAnimation = true;
                        setState(FielderAction._FIELDING, ActionStep._FLYBALL_SPECIAL);//actState = FielderAction._FLYBALL_SPECIAL;                        
                        
                        if (nFielderDir == FieldParm._SOUTH || nFielderDir == FieldParm._SOUTHEAST || nFielderDir == FieldParm._SOUTHWEST)
                        {
                            moveStep = 0;// 정상
                        }
                        else
                        {
                            //외야수 점프 캐치 애니메이션 나올 확률
                            if (MyMath.Percent() < 35 && posIndex >= CPlayer._LEFTFIELDER)
                            {
                                moveStep = 99;//체크체크 점프
                            }
                            else
                            {
                                moveStep = 0;
                            }
                        }

                    }
                    else if (flyballCatchType == FlyCatch.FLYCATCH_DIVING)
                    {
                        setState(FielderAction._FIELDING, ActionStep._FLYBALL_SPECIAL);//actState = FielderAction._FLYBALL_SPECIAL;//
                        //setNextMove(FieldingMechanism._FLYBALL_SLIDING, false, 1);
                        if (bDiveSuccess == true)
                        {
                            setFlyCollider(flyballCatchType);
                        }
                        else
                        {
                            setNoCatchCollider();
                        }
                        moveStep = 0;
                        curTime = 0;
                        nLastDir = -1;

                    }
                    else if (flyballCatchType == FlyCatch.FLYCATCH_JUMPING)
                    {
                        if (specialCatchSuccess == true) 
                        {
                            //내야수 스페셜 캐치 성공
                            setFlyCollider(flyballCatchType);
                        }
                        else
                        {
                            setNoCatchCollider();
                        }
                        dX = dY = 0;
                        setState(FielderAction._FIELDING, ActionStep._FLYBALL_SPECIAL);
                        nFielderDir = FieldingMechanism._InitFielderDir[posIndex];
                        curTime = 0;
                        moveStep = 0;

                    }
                    else//if (flyballCatchType == FlyCatch.FLYCATCH_NORMAL)
                    {
                        setFlyCollider();
                        setCatchReady(true, false, false);
                    }
                }
            }
            else if (bGrounderAvail)
            {
                if (curTime >= fieldingTime)
                {
                    float dis = posY - field.ball.nBallY;
                    if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING)
                    {
                        if (checkInfieldDash(dis) == false)
                        {
                            ////////UnityEngine.//Debug.Log("=====================>>GROUNDERCATCH_MOVE posIndex = " + posIndex);
                            ////////UnityEngine.//Debug.Log("=====================>>여기서 변화 = " + posIndex + "curTime = " + curTime + "===>fieldingTime = " + fieldingTime);
                            setState(FielderAction._FIELDING, ActionStep._GROUNDER_SPECIAL);//actState = FielderAction._GROUNDER_SPECIAL;
                            nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                            setNextMove(FieldingMechanism._GROUNDBALL_FORE_BACK, true, 0.6f, true, true);
                            moveStep = 0;
                            nLastDir = -1;

                            
                            //dstOffsetX = (nFielderDir == FieldParm._EAST ? (FieldingMechanism.MOVING_GROUNDER_OFFSET_FORE + specialTimeGabX) : (FieldingMechanism.MOVING_GROUNDER_OFFSET_BACK - specialTimeGabX)); //옵셋
                            //dstOffsetY = 0;
                            //posX += dstOffsetX;
                        }
                        return;
                    }
                    else if (grounderCatchType == GrounderCatch.GROUNDERCATCH_DIVING)
                    {
                        if (checkInfieldDash(dis) == false)
                        {
                            setSlidingSkillOn();
                            if (bDiveSuccess == true)
                            {
                                //##연출 내야수 다이빙캐치
                            }
                        }
                        return;
                    }

                    else if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING_NORMAL)
                    {
                        ////////UnityEngine.//Debug.Log("=====================>>GROUNDERCATCH_MOVING_NORMAL posIndex = " + posIndex);
                        //if (checkInfieldDash(dis) == false)
                        {
                            setState(FielderAction._FIELDING, ActionStep._GROUNDER_SPECIAL);//actState = FielderAction._GROUNDER_SPECIAL;//
                            if (field.ball.nBallZ > FieldParm.JUMPCATCH_HEIGHT)
                            {
                                if (checkBoundJump() == true) //DONE
                                {
                                    //setCollider(GrounderCatch.GROUNDERCATCH_JUMP); //컬라이더 재정의 
                                    //UnityEngine.//Debug.Log("====================>>이건 공중 다이빙 캐치로 바꿔");
                                    if (skillJumpCatchLevel > 0) // 점핑캐치 체크
                                    {
                                        if (specialCatchSuccess == true)
                                        {
                                            //제5의내야수 or 철벽수비 연출 (점프캐치)
                                            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, posIndex == CPlayer._PITCHER ? SkillIndex.PitcherJumpCatch : SkillIndex.SpecialCatch);

                                            //내야수 스페셜 캐치 성공
                                            setCatch(false, true);
                                            catchDelayRate = 1.5f;
                                        }
                                        else
                                        {
                                            setNoCatchCollider();
                                        }
                                        nFielderDir = FieldingMechanism.getJumpCatchException(nFielderDir, posIndex);
                                        setNextMove(FieldingMechanism._FLYBALL_JUMPING, false, 1);
                                    }
                                    else
                                    {
                                        setStop();
                                    }
                                }
                                else
                                {
                                    nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                                    setNextMove(FieldingMechanism._GROUNDBALL_CENTER2_HIGH, false, 1, true, false);
                                }
                            }
                            else
                            {
                                nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                                setNextMove(FieldingMechanism._GROUNDBALL_CENTER2, false, 1, true, false);
                            }
                            moveStep = 0;
                            curTime = 0;
                            nLastDir = -1;
                            //dstOffsetX = (nFielderDir == FieldParm._EAST ? (FieldingMechanism.MOVING_GROUNDER2_OFFSET_FORE + specialTimeGabX) : (FieldingMechanism.MOVING_GROUNDER2_OFFSET_BACK - specialTimeGabX)); //옵셋
                            //dstOffsetY = 0;
                            //posX += dstOffsetX;
                        }
                        return;
                    }
                    else
                    {
                        //float dis = posY - field.ball.nBallY;// getDistance(posX, field.ball.nBallX, posY, field.ball.nBallY);
                        float spdRatio = (field.ball.speed / field.ball.firstSpeed);
                        bool bCatchReady = true;

#if !NO_DASH_MOVE
                        ////UnityEngine.//Debug.Log("=====================>>DASH 체킹 posIndex = " + posIndex + "==>>DIS = " + dis + "====>bBuntHit = " + field.batter.bBuntHit);
                        if (dis > FieldingMechanism.RANGE_OUTFIELDER_WAITING || field.bPitcherException == true || field.batter.bBuntHit == true || field.ball.firstAngleZ <= -50)
                        {
                            bDashCatchTry = true;
                            ////UnityEngine.//Debug.Log("=====================>>DASH로 바뀜 posIndex = "+posIndex + "==>>DIS = "+dis);
                            float deltaSpeed = field.ball.speed * deltaTime;
                            if (posIndex >= CPlayer._LEFTFIELDER)
                            {
                                ////////UnityEngine.//Debug.Log("=====================>>deltaSpeed = " + deltaSpeed);
                                if (deltaSpeed < FieldingMechanism.RANGE_OUT_DASH_CHECK_SPEED)//25)
                                {
                                    //////UnityEngine.//Debug.Log("=====================>>외야 DASH로 바뀜");
                                    nFielderDir = FieldParm.getDir((int)(field.ball.nBallX - posX), (int)(field.ball.nBallY - posY), 15);
                                    nLastDir = -1;
                                    ////////UnityEngine.//Debug.Log("=====================2");
                                    setBallChase(true);
                                    bCatchReady = false;
                                }
                            }
                            else
                            {
                                ////UnityEngine.//Debug.Log("=====================>>여기까지는 들어옴 field.ball.firstAngleZ = " + field.ball.firstAngleZ);
                                if (field.bInputWait == true) return;

                                if (deltaSpeed < FieldingMechanism.RANGE_IN_DASH_CHECK_SPEED || field.ballPower < 22 || field.ball.firstAngleZ <= -50)
                                {
                                    ////UnityEngine.//Debug.Log("=====================>>내야 DASH로 바뀜");
                                    nFielderDir = FieldingMechanism.getDashDir(posIndex); //FieldParm.getDir((int)(field.ball.nBallX - posX), (int)(field.ball.nBallY - posY), 15);
                                    nLastDir = -1;

                                    setBallChase((skillSlidingCatchLevel > 0 || skillDashThrowLevel > 0) ? true : false);   //setBallChase(true);

                                    bCatchReady = false;
                                    bDashQuickThrow = false;

                                    if (field.groundCatchFielder == posIndex && (posIndex >= CPlayer._SECONDBASEMAN || posIndex == CPlayer._PITCHER))
                                    {
                                        ////UnityEngine.//Debug.Log("=====================================>> skillDashThrowLevel = " + skillDashThrowLevel);
                                        //대쉬앤 퀵 스로우 발동
                                        if (skillDashThrowLevel > 0)
                                        {
                                            setDashThrowSkillOn();
                                        }
                                    }

                                }
                            }
#endif
                        }
                        if (bCatchReady == true)
                        {
                            nFielderDir = FieldParm.getDir((int)(field.homeX - posX), (int)(field.homeY - posY), 15);
                            nLastDir = -1;
                            setCatchReady(false, false, false);
                        }
                    }
                }

                if (posIndex >= CPlayer._LEFTFIELDER)
                {
                    if (field.ball.nBallY > posY
                     || field.ball.bFenceCol == true
                     || field.ball.bBallStop == true)
                    {
                        ////////UnityEngine.//Debug.Log("=====================4");
                        setBallChase();// actState = _BALL_CHASE;
                    }
                }
                else
                {
                    if (bVeryShortGrounder == true)
                    {
                        if (field.ball.bBound == true)
                        {
                            setBallChase();
                        }
                    }
                }



            }
            else if (bDeepFlyChase)
            {
                if (Mathf.Abs(posX - dstX) < FieldingMechanism.RANGE_FIELDING
                 && Mathf.Abs(posY - dstY) < FieldingMechanism.RANGE_FIELDING)
                {
                    if (field.ball.bBound == true) //|| 
                    {
                        nFielderDir = FieldParm.getDir((int)(field.homeX - posX), (int)(field.homeY - posY), 15);
                        setStop();
                    }
                }

                if (posIndex >= CPlayer._LEFTFIELDER)
                {
                    if (field.ball.nBallY > posY
                     || field.ball.bFenceCol == true
                     || field.ball.bBallStop == true)
                    {
                        ////////UnityEngine.//Debug.Log("=====================5");
                        setBallChase();// actState = _BALL_CHASE;
                    }

                }
            }
            else if (bFlyCoverChase == true)
            {

            }
            else if (bGrounderTry)
            {
                if (field.ball.nBallY > posY - 40)
                {
                    //TRY 포기
                    //setStop();
                    //setSecondMove();
                    grounderCatchType = GrounderCatch.GROUNDERCATCH_TRY;
                    setState(FielderAction._FIELDING, ActionStep._GROUNDER_SPECIAL);//actState = FielderAction._GROUNDER_SPECIAL;
                    nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                    setNextMove(FieldingMechanism._GROUNDBALL_FORE_BACK_NOCATCH, true, 1, true, true);
                    bNoDirChange = true;
                    moveStep = 0;
                    curTime = 0;
                    nLastDir = -1;
                }
            }
            else if (bFlyCatchTry)
            {
                if (field.ball.bBound == true)
                {
                    //////UnityEngine.//Debug.Log("=====================>>setSecondMove");

                    //TRY 포기
                    //setStop();
                    setSecondMove();
                    //bFlyCatchTry = false;
                }
            }
            else
            {
                //투수커버처리
                /*    if (posIndex == CPlayer._PITCHER && field.flyCatchAvaiableCount == 0)
                    {
                        //////UnityEngine.//Debug.Log("==================>>투수이며 안타인 경우");
                        setSecondMove();
                    }
                    else
                    {
                        setStop();
                    }*/

                setSecondMove();
            }
        }

        //ActionStep._CHASE 스텝처리
        float nextDashTime;
        void chasingMove()
        {
            if (bChaseColliderCheck == false && (field.ball.nBoundNum > 2 || field.ball.bFenceCol == true))
            {
                if (posIndex >= CPlayer._LEFTFIELDER)
                {
                    setCollider();
                }
                bChaseColliderCheck = true;
            }

            if (field.ball.bBound == false)
            {
                dstX = field.ball.nFirstBoundX;
                dstY = field.ball.nFirstBoundY;
            }
            else
            {
                dstX = field.ball.nBallX + (field.ball.nBallDX * nextDashTime);
                dstY = field.ball.nBallY + (field.ball.nBallDY * nextDashTime);

                if (nextDashTime > 0)
                {
                    nextDashTime -= 0.04f;
                    if (nextDashTime < 0) nextDashTime = 0;
                }

                

            }

            speed = FIELDER_SPEED;
            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            dY = speed * Mathf.Sin(angleDir);
            if (bSideChase == true) dY = 0;
            dX = speed * Mathf.Cos(angleDir);

            nFielderDir = FieldParm.getDir(angleDir);//getDir();

            posX += (dX * deltaTime);
            posY += (dY * deltaTime);

            if (nextDashTime != 0)
            {
                if (actState == FielderAction._FIELDING)
                {
                    if (posIndex <= CPlayer._CATCHER)
                    {
                        posX = field.ball.nBallX;
                    }
                }
            }
            checkFence();
        }
        
        //ActionStep._GROUNDER_SPECIAL 스텝처리
        void grounderSpecial()//Frame()
        {
            if (grounderCatchType == GrounderCatch.GROUNDERCATCH_CATCH_AND_THROW)
            {
                posX += (dX * deltaTime);
                dX = dX * 0.90f;
                if (moveStep == 0)
                {
                    if (bCatchErrorFlag == true)
                    {
                        //에러 처리한곳
                        playSpecialAnim("GROUNDBALL_MISS_CATCH_S", false, 1, false);
                        moveStep = 99;
                    }
                    else
                    {
                        //setNextMove(FieldingMechanism._GROUNDBALL_CENTER2_CATCH, false, 1, true, false);
                        playSpecialAnim(FieldingMechanism._GROUNDBALL_CENTER2_CATCH + FieldParm._dir[nFielderDir], false, 1);
                        bSpecialAnim = false;
                        if (field.ball.bBallCatched == true)
                        {
                            moveStep = 1;
                            curTime = 0;
                        }
                        else
                        {
                            if (curTime > 0.1f)
                            {
                                moveStep = 1;
                            }
                        }
                    }
                }
                else if (moveStep == 1)
                {
                    //if (curTime < 0.4f) makeDust(0.2f);
                    if (curTime >= (FieldingMechanism.TIME_GROUNDER_MOVING_END))
                    {
                        setState(FielderAction._MOTION, ActionStep._MOTION_SET);
                    }
                }
            }
            else if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING_NORMAL)
            {
                if (moveStep == 0)
                {
                    posX += (dX * deltaTime);
                    dY = 0;
                    posY += (dY * deltaTime);

                    //if (bNoCatchFlag == false) makeDust(0.2f);

                    if (field.ball.bBallCatched == true)
                    {
                        //살짝 움직이며 그라운드볼 잡기
                        field.setZoomTo(1.5f, 0.1f); //field.setZoomTo(field.curZoom * 1.2f, 0.3f);
                        moveStep = 1;
                        if (field.ball.nBallZ > FieldParm.JUMPCATCH_HEIGHT)
                        {
                            curTime = FieldingMechanism.TIME_GROUNDER_MOVING_END - 1.5f;
                            throwType = ThrowType._NORMAL;
                        }
                        else
                        {
                            catchDelayRate = 1;
                            curTime = 0;
                            if (posIndex >= CPlayer._SECONDBASEMAN && posIndex <= CPlayer._SHORTSTOP)
                            {
                                setSpecialThrowType(ThrowState.CENTER_MOVING);//nFielderDir == FieldParm._WEST ? ThrowState.BACK_HAND_CATCH : ThrowState.FORE_HAND_CATCH);
                                if (throwType == ThrowType._INFIELD_SIDE_QUICK)
                                {
                                    //칼날송구시 딜레이 레이트 결정
                                    catchDelayRate = SkillParm.getQuickThrowDelayRate(skillQuickThrowLevel, false);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (bNoCatchFlag == true)
                        {
                            dX = dX * 0.90f;
                            dY = dY * 0.90f;
                        }

                        if (curTime >= 2.0f)
                        {
                            setStop();
                        }
                    }
                }
                else
                {
                    posX += (dX * deltaTime);
                    posY += (dY * deltaTime);

                    dX = dX * 0.90f;
                    dY = dY * 0.90f;

                    //makeDust(0.2f);

                    if (throwType == ThrowType._INFIELD_SIDE_QUICK)
                    {
                        if (moveStep == 1)
                        {
                            if (curTime >= (FieldingMechanism.TIME_GROUNDER_MOVING_END * catchDelayRate))
                            {
                                moveStep = 2;
                            }
                        }
                    }

                    if (curTime >= (FieldingMechanism.TIME_GROUNDER_MOVING_END)
                     || curTime > specialThrowCatchDelayRate)
                    {
                        setSpecialMoveEnd(1000);
                    }
                }


            }
            else if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);


                if (moveStep == 0)
                {
                    //UnityEngine.//Debug.Log("======================>>skillSpinThrowLevel = " + skillSpinThrowLevel + "======================>>skillJumpThrowLevel = " + skillJumpThrowLevel);
                    if (skillSpinThrowLevel > 0 || skillJumpThrowLevel > 0)
                    {
                        if (bCatchErrorFlag == false)
                        {
                            if (field.bVsShow == false)
                            {
                                //특급송구 연출
                                fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.SpecialThrow);
                            }
                        }
                    }
                    //makeDust(0.2f);

                    if (field.ball.bBallCatched == true)
                    {
                        if (skillSpinThrowLevel == 0 && skillJumpThrowLevel == 0)
                        {
                            field.setZoomTo(1.5f, 0.5f); //field.setZoomTo(field.curZoom * 1.2f, 0.5f);
                        }

                        if (field.ball.bHighDivingCatched == true) //공중 다이빙  : 후에 추가
                        {
                            //성공 확률 추가
                            moveStep = 1;
                            nFielderDir = FieldParm._SOUTH;
                            setNextMove(FieldingMechanism._FLYBALL_JUMPING, false, 1);
                            curTime = FieldingMechanism.TIME_GROUNDER_MOVING_END - 1.0f;
                            throwType = ThrowType._NORMAL;
                        }
                        else
                        {
                            //////UnityEngine.//Debug.Log("======================>>nFielderDir = " + nFielderDir);
                            nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                            setNextMove((field.ball.nBallZ > FieldParm.STANDINGCATCH_HEIGHT ? FieldingMechanism._GROUNDBALL_FORE_BACK_HIGH : FieldingMechanism._GROUNDBALL_FORE_BACK2), false, 0.6f, true, true);


                            if (posIndex >= CPlayer._SECONDBASEMAN && posIndex <= CPlayer._SHORTSTOP)
                            {
                                setSpecialThrowType(nFielderDir == FieldParm._WEST ? ThrowState.BACK_HAND_CATCH : ThrowState.FORE_HAND_CATCH);

                                if (throwType == ThrowType._INFIELD_SIDE_QUICK)
                                {
                                    //UnityEngine.//Debug.Log("===================>>깊은 백핸드 포핸드시 칼날 송구 예외 처리");
                                    catchDelayRate = 0.85f;
                                    throwType = ThrowType._NORMAL;
                                }
                            }

                            moveStep = 1;
                            curTime = 0;

                            /*
                            if (throwType == ThrowType._INFIELD_SIDE_SPIN || throwType == ThrowType._INFIELD_OVER_JUMPING)
                            {
                                //##연출 내야수 스페셜 송구
                                setTimeScale(Field.INIT_TIME_SCALE * 0.5f);
                            }*/
                        }
                    }
                    else
                    {
                        if (curTime >= 2.0f)
                        {
                            setStop();
                        }
                    }
                }
                else if (moveStep == 1)
                {
                    if (field.bInputWait == true) return;

                    //if (curTime < 0.2f) 
                    //makeDust(0.2f);

                    dX = dX * 0.965f;
                    dY = dY * 0.965f;


                    if (curTime >= (FieldingMechanism.TIME_FORE_BACK_END * catchDelayRate)
                         || curTime > specialThrowCatchDelayRate)
                    {
                        //트라이트라이
                        if (bGrounderTry == true)
                        {
                            setNextMove(FieldingMechanism._GROUNDBALL_FORE_BACK_HIGH, false, 1.0f, true, true);
                            moveStep = 99;
                            curTime = 0;
                        }
                        else
                        {
                            setSpecialMoveEnd(0);
                        }
                    }
                }
                else if (moveStep == 99)
                {
                    //트라이트라이
                    if (curTime > 0.8f)
                    {
                        setSpecialMoveEnd(0);
                    }
                }                
            }
            else if (grounderCatchType == GrounderCatch.GROUNDERCATCH_DIVING)
            {
                if (moveStep == 0)
                {                    
                    posX += 0.6f * (dX * deltaTime);
                    posY += 0.6f * (dY * deltaTime);

                    //if (bNoCatchFlag == false) makeDust(0.2f);

                    //if (curTime >= FieldingMechanism.TIME_SLIDING_CATCH || field.ball.bBallCatched == true)
                    if (field.ball.bBallCatched == true)
                    {
                        if (field.ball.bHighDivingCatched == true) //공중 다이빙 : 후에 추가
                        {
                            //성공 확률 추가
                            moveStep = 1;
                            nFielderDir = FieldParm._SOUTH;
                            setNextMove(FieldingMechanism._FLYBALL_JUMPING, false, 1);
                            curTime = FieldingMechanism.TIME_GROUNDER_MOVING_END - 1.0f;
                            throwType = ThrowType._NORMAL;
                        }
                        else
                        {
                            nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                            setNextMove(FieldingMechanism._GROUNDBALL_SLIDING2, false, 1, false);
                            moveStep = 1;
                            curTime = 0;
                            if (posIndex >= CPlayer._SECONDBASEMAN && posIndex <= CPlayer._SHORTSTOP)
                            {
                                setSpecialThrowType(nFielderDir == FieldParm._WEST ? ThrowState.BACK_DIVING_CATCH : ThrowState.FORE_DIVING_CATCH);
                            }
                        }
                    }
                    else
                    {
                        if (field.ball.nBallY > posY)
                        {
                            if (field.ball.bCameraBallMove == false)
                            {
                                field.ball.bCameraBallMove = true;
                            }
                        }

                        if (bNoCatchFlag == true)
                        {
                            //makeDust(0.2f);
                            
                            dX = dX * 0.992f;
                            dY = dY * 0.992f;
                        }

                        if (curTime >= 2.0f)
                        {
                            setStop();
                        }
                    }
                }
                else if (moveStep == 1)
                {
                    //makeDust(0.2f);

                    posX += (dX * deltaTime);
                    posY += (dY * deltaTime);

                    field.ball.nBallX += (dX * deltaTime);
                    field.ball.nBallY += (dY * deltaTime);
                    dX = dX * 0.965f;
                    dY = dY * 0.965f;
                    if (curTime >= (FieldingMechanism.TIME_GRONDER_SLIDING_END)
                     || curTime > specialThrowCatchDelayRate)
                    {
                        setSpecialMoveEnd(0);
                    }
                }
            }
            else if (grounderCatchType == GrounderCatch.GROUNDERCATCH_TRY)
            {
                if (moveStep == 0)
                {
                    //makeDust(0.2f);

                    posX += (dX * deltaTime);
                    posY += (dY * deltaTime);
                    dX = dX * 0.98f;
                    dY = dY * 0.98f;

                    if (curTime >= 2.0f)
                    {
                        moveStep = 1;
                        setStop();
                        setSecondMove();
                    }
                }
            }
        }
   
        //ActionStep._FLYBALL_SPECIAL 스텝처리
        float limitDX, limitDY;
        void flyballSpecial()//Frame()
        {
            posX += (dX * deltaTime);
            posY += (dY * deltaTime);

            checkFence2();

            if (flyballCatchType == FlyCatch.FLYCATCH_FULLSPEED)
            {
                if (moveStep == 0)
                {
                    //패스트 런 캐치
                    //makeDust(0.2f, "2");
                    if (field.ball.bBallCatched || field.ball.bBound)
                    {
                        field.setZoomTo(1.2f, 0.5f);                                                
                        curTime = 0;
                        offsetX = 0;
                        field.setTimeScale(Field.INIT_TIME_SCALE);
                        if (posIndex >= CPlayer._LEFTFIELDER)
                        {
                            field.setRelayOffsetMove(dX * 0.4f, dY * 0.4f);
                        }                                                                            
                        setNextMove(FieldingMechanism._FLYBALL_FASTCATCH, false, 0.65f); 
                        moveStep = 1;
                        
                    }
                }
                else if(moveStep == 1)
                {    
                    //Fast Run Catch 마무리
                    if (curTime > 1.2f)
                    {                       
                        nFielderDir = FieldParm._SOUTH;
                        setStop();
                        setState(FielderAction._MOTION, ActionStep._MOTION_SET);// actState = FielderAction._THROW_READY;
                        curTime = 0;      
                    }
                    else
                    {
                        dX = dX * 0.985f;
                        dY = dY * 0.985f;
                    }
                }
                else if (moveStep == 99 || moveStep == 100)
                {
                    if (moveStep == 99)
                    {
                        float tScale = 0.6f;
                        float delayGab = 0.5f;
                        if (nFielderDir == FieldParm._EAST || nFielderDir == FieldParm._WEST)
                        {
                            tScale = 1.0f;
                            delayGab = 1.2f;
                        }
                        if (field.ball.curTime > field.ball.firstBoundTime - delayGab)
                        {                                              
                            playSpecialAnim("RUNNING_JUMP_CATCH_" + FieldParm._dir[nFielderDir], false, tScale);
                            moveStep++;
                            bSpecialAnim = false;
                            dX *= 0.8f;
                            dY *= 0.8f;
                        }
                    }
                    //패스트 런 점프캐치
                    if (field.ball.bBallCatched || field.ball.bBound)
                    {
                        field.setZoomTo(1.2f, 0.5f);          
                        curTime = 0;
                        offsetX = 0;
                        field.setTimeScale(Field.INIT_TIME_SCALE);
                        if (posIndex >= CPlayer._LEFTFIELDER)
                        {
                            field.setRelayOffsetMove(dX * 0.4f, dY * 0.4f);
                        }                                                                            //패스트 런 캐치
                        moveStep = 1;
                    }
                }
            }
            else if (flyballCatchType == FlyCatch.FLYCATCH_SLOWMOVE)
            {
                if (moveStep == 0)
                {
                    if (field.ball.bBallCatched || field.ball.bBound)
                    {
                        field.setZoomTo(field.curZoom * 1.2f, 0.5f);
                        if (posIndex == CPlayer._CATCHER)
                        {
                            nFielderDir = FieldingMechanism.getFlyballDirException(nFielderDir);
                            setNextMove(FieldingMechanism._CATCHER_FLYBALL, false, 1);
                            moveStep = 2;
                            curTime = 0;
                        }
                        else
                        {
                            setNextMove(FieldingMechanism._FLYBALL_SLOWCATCH, false, 1);
                            moveStep = 1;
                            curTime = 0;
                            if (posIndex >= CPlayer._LEFTFIELDER)
                            {
                                field.setRelayOffsetMove(dX * 0.2f, dY * 0.2f);
                            }
                        }                        
                    }
                }
                else if (moveStep == 1)
                {
                    if (curTime > FieldingMechanism.TIME_SLOWMOVE_END)//(FieldingMechanism.TIME_SLOWMOVE_END * catchDelayRate))
                    {                        
                        nFielderDir = FieldParm._SOUTH;
                        setStop();
                        setState(FielderAction._MOTION, ActionStep._MOTION_SET);// actState = FielderAction._THROW_READY;
                        curTime = 0;                        
                    }
                    else
                    {
                        dX = dX * 0.97f;//0.985f;
                        dY = dY * 0.97f;//0.985f;
                    }
                }
                else if (moveStep == 2)
                {
                    dX = 0;
                    dY = 0;
                    if (curTime > FieldingMechanism.TIME_SLOWMOVE_END)//(FieldingMechanism.TIME_SLOWMOVE_END * catchDelayRate))
                    {
                        //setSpecialMoveEnd(0);
                        setStop(true);                        
                        nFielderDir = FieldParm._SOUTH;
                        curTime = 0;
                    }
                }

            }
            else if (flyballCatchType == FlyCatch.FLYCATCH_DIVING)
            {
                if (moveStep == 0)
                {
                    //쇠그물수비 - 다이빙캐치 연출
                    fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.DivingCatch);
                    moveStep = 1;
                }
                else if (moveStep == 1)
                {
                    //makeDust(0.2f, "2");
                    if (bDiveSuccess == true)
                    {   
                        if (field.ball.bBallCatched == true)
                        {
                            //nFielderDir = FieldingMechanism.getDirException(nFielderDir, dX, dY);
                            setNextMove(FieldingMechanism._FLYBALL_SLIDING, false, 1);
                            moveStep = 2;
                            curTime = 0;
                            field.setTimeScale(Field.INIT_TIME_SCALE);
                            if (posIndex >= CPlayer._LEFTFIELDER)
                            {
                                field.setRelayOffsetMove(dX * 0.3f, dY * 0.3f);
                            }
                        }
                    }
                    else
                    {
                        if (curTime > 0.2f)
                        {
                            setNextMove(FieldingMechanism._FLYBALL_SLIDING, false, 1);
                            moveStep = 3;
                            curTime = 0;
                        }

                    }
                }
                else if (moveStep == 2)
                {
                    //if (curTime < 0.4f) makeDust(0.2f, "2");

                    dX = dX * 0.98f;
                    dY = dY * 0.98f;

                    if (curTime >= FieldingMechanism.TIME_SLIDING_END * 0.5f)
                    {
                        dX = dY = 0;

                    }

                    if (curTime >= FieldingMechanism.TIME_SLIDING_END)// * catchDelayRate)
                    {
                        setSpecialMoveEnd(0);                        
                        //setStop();
                    }
                }
                else if (moveStep == 3 || moveStep == 4)
                {
                    if (field.ball.bBound == true)
                    {
                        if (field.ball.bCameraBallMove == false)
                        {
                            field.ball.bCameraBallMove = true;
                        }
                    }

                    if (moveStep == 3)
                    {
                        if (curTime > 1.0f)
                        {
                            moveStep = 4;
                            anim.timeScale = 0;
                        }
                    }

                    //if (curTime < 0.4f) makeDust(0.2f, "2");

                    dX = dX * 0.98f;
                    dY = dY * 0.98f;

                    if (curTime >= FieldingMechanism.TIME_SLIDING_END * 0.5f)
                    {
                        dX = dY = 0;
                    }
                    if (curTime >= FieldingMechanism.TIME_SLIDING_END)// * catchDelayRate)
                    {
                        setStop();
                    }
                }
            }
            else if (flyballCatchType == FlyCatch.FLYCATCH_JUMPING) //DONE
            {
                if (moveStep == 0)
                {
                    //제5의내야수 or 철벽수비 연출 (점프캐치)
                    fieldSkillDisplayManager.AddSkill(gameObject, pFielder, posIndex == CPlayer._PITCHER ? SkillIndex.PitcherJumpCatch : SkillIndex.SpecialCatch);

                    moveStep = 1;
                }
                else if (moveStep == 1)
                {
                    if (bNoCatchFlag == true)
                    {
                        //못잡음
                        nFielderDir = FieldingMechanism.getJumpCatchException(nFielderDir, posIndex);
                        setNextMove(FieldingMechanism._FLYBALL_JUMPING, false, 1);
                        moveStep = 3;
                        curTime = 0;
                    }
                    else
                    {
                        if (field.ball.bBallCatched || field.ball.bBound)
                        {
                            //제5의 내야수 - 점프캐치효과
                            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.PitcherJumpCatch);
                            nFielderDir = FieldingMechanism.getJumpCatchException(nFielderDir, posIndex);
                            setNextMove(FieldingMechanism._FLYBALL_JUMPING, false, 1);
                            moveStep = 2;
                            curTime = 0;
                        }
                    }
                }
                else if (moveStep == 2)
                {
                    if (curTime > FieldingMechanism.TIME_JUMPING_END)//(FieldingMechanism.TIME_SLOWMOVE_END * catchDelayRate))
                    {
                        //잡음
                        setSpecialMoveEnd(0);
                        nFielderDir = FieldingMechanism._InitFielderDir[posIndex];
                    }
                }
                else if (moveStep == 3)
                {
                    if (curTime > FieldingMechanism.TIME_JUMPING_END)//(FieldingMechanism.TIME_SLOWMOVE_END * catchDelayRate))
                    {
                        //못잡음
                        setStop();
                    }
                }
            }
        }


        //////////////////////////////////////////////////////////////////
        // CATCH State
        //////////////////////////////////////////////////////////////////
        //캐치 상태 처리
        void catchFrame()
        {
            if (aStep == ActionStep._CATCHING)
            {
                catching();
            }
            else if (aStep == ActionStep._CATCHING_READY)
            {
                catchReady();
            }
            else if (aStep == ActionStep._CATCHING_DASH)
            {
                catchDash();
            }
        }
        //ActionStep._CATCHING 스텝처리
        void catching()
        {
            float catchDelayTime = FieldingMechanism.TIME_NORMAL_CATCH * catchDelayRate * 0.7f;
            if(catchDelayTime < 0.4f) catchDelayTime = 0.4f;

            if (curTime > catchDelayTime)
            {
                ////UnityEngine.//Debug.Log("===============>>캐칭 처리");
                setState(FielderAction._MOTION, ActionStep._MOTION_SET);
                curTime = 0;

                if (pitcherReactSuccess == true)
                {
                    //투구반응 예외처리 해줘
                    bSpecialAnim = false;
                }
            }
        }
        //ActionStep._CATCHING_READY 스텝처리
        void catchReady()
        {
            if (posIndex >= CPlayer._LEFTFIELDER)
            {
                if (field.ball.bBallCatched == true)
                {
                    if (curTime > 2.0f)
                    {
                        setState(FielderAction._MOTION, ActionStep._MOTION_SET);
                        curTime = 0;
                    }
                }
            }
        }
        //ActionStep._CATCHING_DELAY 스텝처리
        void catchDash()
        {
            if (bDustOn)
            {
                //makeDust(0.2f);
            }

            if (curTime > 0.5f)
            {
                bDustOn = false;
            }

            if (bDashCatched == true)
            {
                checkFence2(true);

                posX += (dX * deltaTime);
                posY += (dY * deltaTime);


                if (curTime < FieldingMechanism.DELAY_DASHCATCH)// * 0.8f)
                {
                    field.ball.nBallX += (dX * deltaTime);
                    field.ball.nBallY += (dY * deltaTime);
                    dX *= 0.975f;
                    dY *= 0.975f;
                }
                else
                {
                    dX = dY = 0;
                }



                //////UnityEngine.//Debug.Log("======================>>catchReadyFrame catchDelayRate = " + catchDelayRate);
                if (curTime >= (FieldingMechanism.DELAY_DASHCATCH * catchDelayRate)
                    || curTime > specialThrowCatchDelayRate)
                {
                    bDustOn = false;
                    setState(FielderAction._MOTION, ActionStep._MOTION_SET);//actState = FielderAction._THROW_READY;
                    curTime = 0;
                    posX -= 25;
                    offsetX = curOffsetX = 0;
                }
            }
        }


        //////////////////////////////////////////////////////////////////
        // MOTION State
        //////////////////////////////////////////////////////////////////
        //모션 상태 처리
        void motionFrame()
        {
            if (throwAgainState != FieldParm.ThrowAgain.Available)
            {
                if (throwAgainState == FieldParm.ThrowAgain.BackToPosition)
                {
                    //백투포지션 상태인경우 원래 포지션으로 돌아감
                    setBackupPosition(0, 0, true);
                }
                return;
            }

            if (aStep == ActionStep._MOTION_SET)
            {
                motionSet();
            }
            else if (aStep == ActionStep._THROW_READY)
            {
                throwReady();
            }
            else if (aStep == ActionStep._DOUBLEPLAY)
            {
                doublePlay();
            }
            else if (aStep == ActionStep._THROW_DELAY)
            {
                throwDelay();
            }
            
        }

        public void throwTargetExceptioin()
        {
            if (field.nTargetIndex == FieldParm.HOMEBASE_INDEX && field.ball.bFenceCol == true)
            {
                //충돌시 무조건 리레이 포지션으로 예외 처리
                field.nTargetIndex = FieldParm.RELAY_INDEX;
            }
            else
            {
                if (field.nTargetIndex != FieldParm.RELAY_INDEX)
                {
                    //릴레이 포지션이 아닐때 익셉션
                    if (field.bStealThrow == false && field.bFieldPickOffFlag == false)
                    {
                        throwExceptionCheck(field.nTargetIndex);
                    }
                }
                else
                {
                    //릴레이 포지션인 경우
                    if ((posIndex == CPlayer._LEFTFIELDER && posY < field.fielder[CPlayer._SHORTSTOP].posY) && (posIndex == CPlayer._RIGHTFIELDER && posY < field.fielder[CPlayer._SECONDBASEMAN].posY)
                      || field.bOnceWildThrow == true)
                    {
                        //해당 사항에서 2루로
                        field.nTargetIndex = FieldParm.SECONDBASE_INDEX;
                    }
                }
            }
        }

        //ActionStep._MOTION_SET 스텝처리
        void motionSet()
        {
            if (bSpecialAnim == true) return;
            if (field.bInputWait == true) return;

            if (bThrowableChecked == false)
            {
                field.manager.RandomSeedSync();

                bool bLaserThrow = false;
                if (bLaserThrowCheck == false)
                {
                    //레이저 체크
                    bLaserThrow = checkLaserThrowPossible();                    
                    //레이저레이저 - 추가됨
                    if (bLaserThrow == true)
                    {
                        //bLaserThrow가 true인 경우 코루틴으로 처리하고 update에서 빠져나옴
                        return;
                    }//
                }

                if (bLaserThrow == false)
                {
                    //던질 방향을 미리 계산
                    throwDelayTime = (THROW_DELAY * throwDelayRate);
                    bThrowAvailable = field.checkThrowAvailable(posIndex);

                    //여기여기체크
                    if(Mode.bPvpMode433 == true && field.manager.bMyTurn == false)
                    {
                        //수신
                        if(field.manager.Pvp_throwTarget[posIndex]!=-100)
                        {
                            bThrowAvailable = true;
                            field.nTargetIndex = field.manager.Pvp_throwTarget[posIndex];
                        }
                    }

                    bThrowableChecked = true;

                    if (bThrowAvailable == true)
                    {   
                        throwTargetExceptioin();
                    }

                    if (posIndex == CPlayer._CATCHER)
                    {
                        if (bThrowAvailable == false)
                        {
                            if (field.run.bOnRunning[FieldParm.HOMEBASE_INDEX] == true && bBaseCovering == true)
                            {
                                StartCoroutine(nextSpecialAnim(0.2f, "9700_CATCHER_TAG_" + FieldParm._dir[nFielderDir], false));
                            }
                        }
                    }
                    else
                    {
                        if (bThrowAvailable == false)
                        {
                            if (posIndex > CPlayer._FIRSTBASEMAN)
                            {
                                if (posIndex >= CPlayer._LEFTFIELDER)
                                {
                                    //Debug.Log("=============================>>외야수는 무조건 릴레이 포지션으로");
                                    bThrowAvailable = true;
                                    field.nTargetIndex = FieldParm.RELAY_INDEX;
                                    field.returnCheck_Steal_Pickoff(-3);
                                }
                                else
                                {
                                    //UnityEngine.//Debug.Log("=============================>>백업 포지션");
                                    StartCoroutine(idleAction());
                                    return;
                                }
                            }                            
                        }
                    }

                    //여기여기체크
                    if (Mode.bPvpMode433 == true && field.manager.bMyTurn == true)
                    {
                        //발신
                        if (bThrowAvailable == true)
                        {
                            if (field.nTargetIndex != -1)
                            {
                                pvpmanager.Get().SendThrowingSyncInfo(posIndex, field.nTargetIndex);
                            }
                        }
                    }

                    setState(FielderAction._MOTION, ActionStep._THROW_READY); //-> 캐치 딜레이의 타입을 여기서 결정
                }
                curTime = 0;
            }
            else
            {
                setStop();
            }
        }

        private IEnumerator idleAction()
        {
            if (field.bReturnBattingView == false)
            {
                //yield return new WaitForSeconds(0.3f);
#if _OrthoCamera
            field.setZoomTo(1, 1.0f);
#else
                //CameraManager.SetFieldActiveCameraAngle(-30, 1.0f);
                //field.setZoomTo(1.2f, 1.0f);
#endif
                //field.returnWaitTime = -20;
                field.returnCheckNC(0.5f);
                setStop();
                yield return new WaitForSeconds(0.2f);
                if (field.manager.playState != PlayState.PLAY_CHANGE_INNING)
                {
                    setBackupPosition(0, 0, true);
                }
            }
        }

        private void throwExceptionCheck(int target)
        {
            bool bNoThrow = false;
            bool bRelay = false;

            Runner targetRunner = field.run.getDestRunner(target);

            if (targetRunner != null)
            {
                float rate = targetRunner.basePositionRate();
                ////UnityEngine.//Debug.Log("===========================================>>rate = " + rate);

                if (target == FieldParm.FIRSTBASE_INDEX)
                {
                    if (posIndex >= CPlayer._LEFTFIELDER)
                    {
                        bRelay = true;
                    }
                    else
                    {
                        if (posIndex == field.nRelayFielderIndex)
                        {
                            bNoThrow = true;
                        }
                    }
                }
                else
                {
                    if (posIndex >= CPlayer._LEFTFIELDER)
                    {
                        //외야
                        float limit = 0.5f + 0.3f - (target * 0.1f);
                        ////UnityEngine.//Debug.Log("===========================================>>limit = " + limit);
                        if (rate > limit)
                        {
                            bRelay = true;
                        }
                    }
                    else
                    {
                        //내야
                        if (target == FieldParm.HOMEBASE_INDEX) return; //홈이면 무조건 던져

                        if (rate > 0.9f)
                        {
                            //기타 베이스는 살거 같으면 안던져
                            bNoThrow = true;
                        }
                    }
                }

                if (bNoThrow == true)
                {
                    //안던지고 가만히 있음
                    bThrowAvailable = false;
                    nFielderDir = FieldParm._SOUTH;
                    setStop();
                }
                else if (bRelay == true)
                {
                    //릴레이로 전환
                    field.nTargetIndex = FieldParm.RELAY_INDEX;
                }
            }
        }

        //ActionStep._DOUBLEPLAY 스텝처리
        private void doublePlay()
        {
            if (curTime > 0.1f) //한계시간
            {
                ////UnityEngine.//Debug.Log("====================>>doublePlayFrame");
                //더블플레이 모션

                if (field.doublePlayType % 10 == 0) //2루에서 이루어지는
                {
                    ////UnityEngine.//Debug.Log("====================>>2루에서 이루어지는 doublePlayFrame");                    
                    bDoublePlayAction = true;

                    if (field.runnerDPStop == FieldSkillUse.Init)
                    {
                        Runner secondRunner = field.run.getDestRunner(FieldParm.SECONDBASE_INDEX);
                        if (secondRunner.bDoublePlaySkillOn == true)
                        {
                            if (MyMath.Percent() < 40)
                            {
                                if (secondRunner.pRunner.skillAvailable(SkillIndex.RunnerDoublePlayBreaker) == true)
                                {
                                    //2루에서 이루어지는 dpstop 재 체크
                                    secondRunner.setDoublePlayStopOn();
                                }
                            }
                        }
                        else
                        {
                            if (secondRunner.basePositionRate() > 0.78f)
                            {
                                if (skillDashThrowLevel > 0)
                                {
                                    ////UnityEngine.//Debug.Log("=====================================================================>>대쉬킥");
                                    throwType = ThrowType._INFIELD_SIDE_DASH;
                                    throwReadyDelay = FieldingMechanism.DELAY_THROW_SIDEDASH;
                                    //##연출 내야수 대쉬(송구)
                                }
                            }
                        }
                        ////여기까지 원래 없던부분
                    }
                }
                else
                {
                    //기타
                    throwType = ThrowType._NORMAL;
                    throwReadyDelay = FieldingMechanism.DELAY_THROW_NORMAL;
                }
                throwSpeedRate = 1;

                setState(FielderAction._MOTION, ActionStep._THROW_READY);// actState = FielderAction._THROW_READY;
                field.bThrowBallCatched = false;
                bThrowableChecked = false;
            }
        }
        //ActionStep._THROW_READY 스텝처리
        private void throwReady()
        {
            if (field.bThrowBallCatched == true)
            {
                //////UnityEngine.//Debug.Log("====================>>111 throwDelayTime = " + throwDelayTime);
                if (field.doublePlayType != FieldParm.NO_DOUBLEPLAY)
                {
                    ////UnityEngine.//Debug.Log("==================>>더블 플레이 조건계산");
                    field.bThrowBallCatched = false;
                    curTime = 0;
                    setState(FielderAction._MOTION, ActionStep._DOUBLEPLAY); //actState = FielderAction._DOUBLEPLAY_MOTION;
                }
                else
                {
                    //글러브 잡는 프레임 계산
                    if (curTime > (bRelayPositioning ? 0.3f : 0.5f)) //수비 능력치 밸런스 고려될수 있음
                    {
                        ////UnityEngine.//Debug.Log("==================>>릴레이 포지셔닝??  curTime:: "+curTime);
                        field.bThrowBallCatched = false;
                        curTime = 1000;

                        if (bRelayPositioning == true)
                        {
                            ////UnityEngine.//Debug.Log("=======================>> 릴레이 하는 놈!!");
                            ////UnityEngine.//Debug.Log("=======================>> throwAvailble = " + bThrowAvailable);
                            if (bThrowAvailable == true)
                            {
                                field.nTargetIndex = FieldParm.RELAY_INDEX;
                                if (field.run.bOnRunning[FieldParm.HOMEBASE_INDEX] == true)
                                {
                                    ////UnityEngine.//Debug.Log("=======================>> 홈으로 주자가 달림");
                                    field.nTargetIndex = FieldParm.HOMEBASE_INDEX;
                                }
                                else if (field.run.bOnRunning[FieldParm.THIRDBASE_INDEX] == true)
                                {
                                    ////UnityEngine.//Debug.Log("=======================>> 3루로 주자가 달림");
                                    field.nTargetIndex = FieldParm.THIRDBASE_INDEX;
                                }
                                ////UnityEngine.//Debug.Log("=======================>> 릴레이 하는 놈이 그냥 " +(field.nTargetIndex+1)+ "루 베이스로 던짐");
                                int coverIndex = field.getBaseCoverIndex(field.nTargetIndex);
                                setThrow(coverIndex);
                            }
                        }
                    }
                }
            }
            else
            {
                if (bThrowAvailable == true)
                {
                    //병살저지 발생 여부                
                    if (checkDoublePlayStop()) return;   //던지는걸 포기
                    
                    int coverIndex = field.getBaseCoverIndex(field.nTargetIndex);
                    if (coverIndex != -1 || posIndex >= CPlayer._LEFTFIELDER)
                    {
                        setThrow(coverIndex);
                    }
                    else
                    {
                        if (field.bBaseCoverAfterLiner == true)
                        {
                            bThrowErrorCoverd = true;
                            setBaseCover(field.nTargetIndex);
                            field.bBaseCoverAfterLiner = false;
                        }
                    }
                }
                else
                {
                    field.ball.setParticleDraw(false);
                    field.bReturnBattingView = true;
                    //정상적인 송구 케이스가 아닌 경우 setThrowExceptionCase()에서 처리해줌
                    setThrowExceptionCase();
                }
            }

        }

        //던지기의 각종 예외 사항 처리
        private void setThrowExceptionCase()
        {
            if (bForcedThrow == true) //강제던지기 플래그
            {
                bThrowAvailable = true;
                bForcedThrow = false;

                if (field.bOnceWildThrow == true) //악송구 발생시
                {
                    field.nTargetIndex = FieldParm.FIRSTBASE_INDEX;
                    for (int i = FieldParm.HOMEBASE_INDEX; i > FieldParm.FIRSTBASE_INDEX; i--)
                    {
                        if (field.run.bOnRunning[i] == true)
                        {
                            field.nTargetIndex = i;
                            break;
                        }
                    }

                    if (field.nTargetIndex == FieldParm.FIRSTBASE_INDEX)
                    {
                        bThrowAvailable = false;
                    }
                }
                else
                {
                    //field.nTargetIndex = FieldParm.SECONDBASE_INDEX;
                    if (posIndex >= CPlayer._LEFTFIELDER)
                    {
                        bThrowAvailable = true;
                        field.nTargetIndex = FieldParm.RELAY_INDEX;
                    }
                    else
                    {
                        bThrowAvailable = false;
                        for (int i = FieldParm.HOMEBASE_INDEX; i >= FieldParm.FIRSTBASE_INDEX; i--)
                        {
                            if (field.run.bOnRunning[i] == true)
                            {
                                field.nTargetIndex = i;
                                bThrowAvailable = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (posIndex == field.nRelayFielderIndex)
                {
                    bThrowAvailable = false;
                    /*
                    if (bBaseCovering == false)
                    {
                        bThrowAvailable = true;
                        ////UnityEngine.//Debug.Log("=================>>33 bThrowAvailable = " + bThrowAvailable);
                        field.nTargetIndex = FieldParm.SECONDBASE_INDEX;
                    }
                    else
                    {
                        bThrowAvailable = false;
                    }*/
                }
                else
                {

                    //nFielderDir = FieldParm._SOUTH;
                    bThrowAvailable = false;
                    setNextMove(getHoldIndexStr(), true, 1);
                    dX = 0;
                    dY = 0;
                    actState = FielderAction._STANDBY;
                }
            }
            if (bThrowAvailable == true)
            {
                ////UnityEngine.//Debug.Log("=================>>33 bThrowAvailable = " + bThrowAvailable);
                int coverIndex = field.getBaseCoverIndex(field.nTargetIndex);
                if (coverIndex != -1 || posIndex >= CPlayer._LEFTFIELDER)
                {
                    setThrow(coverIndex);
                    field.throwFrame = 100;
                }
                else
                {
                    if (posIndex < CPlayer._LEFTFIELDER && field.errorType != FieldParm.ErrorType.None)
                    {
                        //에러상황 내야수 베이스 빈경우 자기가 직접 베이스 커버
                        setBaseCover(field.nTargetIndex);
                    }
                    else
                    {
                        nFielderDir = FieldParm._SOUTH;
                        setStop();
                    }
                }
            }
            else
            {
                field.forcedSetBattingView(0.7f);
            }

        }



        //ActionStep._THROW_DELAY 스텝처리
        void throwDelay()
        {
            if (curTime >= 0.35f)
            {
                setNextMove(strThrowAfterDelay, false, 1);
                actState = FielderAction._THROWING;
                curTime = 0;
            }
        }


        //////////////////////////////////////////////////////////////////
        // THROW State
        //////////////////////////////////////////////////////////////////
        //쓰로잉 상태 처리
        bool bTimeScaleForThrow = false;
        private void throwingFrame()
        {
            int throwDelayCoef = 1;

            if (curTime < throwReadyDelay * throwStopRate * throwDelayCoef)
            {
                posX += (tdX * deltaTime);
                posY += (tdY * deltaTime);
                //field.ball.nBallX += (tdX * deltaTime);
                //field.ball.nBallY += (tdY * deltaTime);
            }

            if(bTimeScaleForThrow == false) //if (throwType == ThrowType._INFIELD_OVER_JUMPING || throwType == ThrowType._INFIELD_SIDE_SPIN)
            {
                if (curTime > throwReadyDelay * 0.5f * throwDelayCoef)
                {
                    field.setTimeScale(Field.INIT_TIME_SCALE, true);
                    bTimeScaleForThrow = true;
                }
            }

            if (curTime >= throwReadyDelay * throwDelayCoef)
            {
                //bMoveThrow = false;
                if (bThrow == false)
                {
                    if (bThrowErrorFlag == true)
                    {
                        field.bThrowErrorFlag = true;
                        bThrowErrorFlag = false;
                    }

                    if (field.bNoCheckReThrow == false)
                    {
                        int next = field.checkReThrow();
                        if (next != -1)
                        {
                            field.nTargetIndex = next;
                            int coverIndex = field.getBaseCoverIndex(field.nTargetIndex);
                            setThrow(coverIndex, false, true);
                        }                        
                    }


                    field.bNoCheckReThrow = false;
                    field.ball.bCameraBallMove = true;
                    
                    
                    posX += (tdX * throwMoveRate);
                    posY += (tdY * throwMoveRate);
                    //field.ball.nBallX += (tdX * throwMoveRate);
                    //field.ball.nBallY += (tdY * throwMoveRate);

                    tdX = tdY = 0;
                    throwTypeFirstBase = ThrowType._NORMAL;
                    field.ball.setActive(true);
                    field.ball.setBallThrow();// nBallStep = FBall.BALL_THROW;
                    field.ball.bBallHidden = true;
                    field.ball.setDraw(false);
                    StartCoroutine(throwingStop());
                    bThrow = true;                    
                    field.throwFrame = getThrowFrame();
                    
                    //연출관련 삭제
                    //field.setZoomCameraSetting(false);
                    //if (posIndex >= CPlayer._LEFTFIELDER)// || field.nTargetIndex != FieldParm.SECONDBASE_INDEX)
                    {
                        //송구시 줌 아웃
                        field.setThrowingZoom(posIndex);
                    }
                    field.setTimeScale(Field.INIT_TIME_SCALE);
                    field.ball.setParticleDraw(field.bBallTail);
                    field.ball.setThrowingCamera();
                    //if (field.bBallTail == true) CameraManager.SetFieldMotionBlur(0.25f);
                    IngameUI.GetFieldUI().SetName(false, null, 0, field.manager.bMyTurn);

                    bTimeScaleForThrow = false;
                    field.bFirstThrow = true;

                    //볼 데드시 처리
                    field.ball.checkBallDeadThrow(field.nTargetIndex);

                    //릴리즈 사운드
                    //soundmanager.Get().PlaySound(soundmanager.SoundID.Release);
                }
            }
        }

        private int getThrowFrame()
        {
            if (field.bFieldPickOffFlag == true) return 1000;   //광속견제인 경우
            if (SimulSteal.catcherSitThrow != FieldSkillUse.Init) return 1000;// 앉아쏴인 경우

            if (posIndex != field.nRelayFielderIndex)
            {
                int coverIndex = field.getBaseCoverIndex(field.nTargetIndex);

                if (coverIndex >= 0)
                {
                    if (Mathf.Abs(field.fielder[coverIndex].posX - posX) > (BallPlayManager.m_lcdWC * field._ZOOM_SIZE * 0.8f) ||
                       Mathf.Abs(field.fielder[coverIndex].posY - posY) > (BallPlayManager.m_lcdHC * field._ZOOM_SIZE * 0.8f))
                    {
                        return 0;
                    }
                }
            }

            return 1000;
        }

        float throwGabX, throwGabY;

        private IEnumerator throwingStop()
        {
            yield return new WaitForSeconds(1.5f);            
            ////////UnityEngine.//Debug.Log("================>>스탑");            
            speed = 0;
            dX = 0;
            dY = 0;
            actState = FielderAction._STANDBY;
            _strID = "0000_HOLD_";
            strID = _strID + FieldParm._dir[nFielderDir];
            playSpecialAnim(strID, false, 1.0f, false);
            bSpecialAnim = false;
            posX += throwGabX;
            posY += throwGabY;
            //setPosition();
            throwGabX = throwGabY = 0;
            bThrow = false;
            yield return new WaitForEndOfFrame();
            setStop();
            yield return new WaitForEndOfFrame();
            if (posIndex == CPlayer._THIRDBASEMAN && field.checkCoverNeeded(FieldParm.THIRDBASE_INDEX, posIndex) == true)
            {
                setBaseCover(FieldParm.THIRDBASE_INDEX);
            }
            else if (posIndex == CPlayer._SECONDBASEMAN && field.checkCoverNeeded(FieldParm.SECONDBASE_INDEX, posIndex) == true)
            {
                setBaseCover(FieldParm.SECONDBASE_INDEX);
            }
            else if (posIndex == CPlayer._SHORTSTOP && field.checkCoverNeeded(FieldParm.SECONDBASE_INDEX, posIndex) == true)
            {
                setBaseCover(FieldParm.SECONDBASE_INDEX);
            }
        }

        //////////////////////////////////////////////////////////////////
        // THROWING Catching State
        //////////////////////////////////////////////////////////////////
        //쓰로잉 캐치 상태 처리
        private void throwingCatchFrame()
        {
            /*
            if (aStep == ActionStep._FORCE_OUT)
            {

            }
            else if (aStep == ActionStep._TAG_OUT)
            {

            }
            else if (aStep == ActionStep._CATCH_NORMAL)
            {
            }*/
            if (aStep == ActionStep._TAGGING)
            {
                tagging();
            }
            else
            {
                if (nFielderDir != throwCatchDir)
                {
                    nFielderDir = throwCatchDir;
                }
            }
        }

        /*
        private bool checkDelayStealStateOnTagging()
        {
            if (field.runnerDelaySteal == FieldSkillUse.Active)
            {
                if (curTime > FieldingMechanism.DELAY_HOMESTEAL_WAIT)
                {
                    bool bSpecialThrowCount = false;
                    ////UnityEngine.//Debug.Log("==========================>>딜레이드 스틸 플래그 발생 = " + curTime);
                    setStop();
                    field.ball.bNoEventCamera = false;
                    field.ball.setFielderFocus(posIndex);                    
                    bSpecialAnim = false;
                    
                    field.nTargetIndex = FieldParm.HOMEBASE_INDEX;
                    bThrowAvailable = true;
                    int coverIndex = CPlayer._CATCHER;// field.getBaseCoverIndex(field.nTargetIndex);                   
                    if (pFielder.fieldSkillSuccess(SkillIndex.SpecialThrow) == true)
                    {
                        //야수가 스페셜 송구 보유시 딜레이스틸에 대한 카운터 발생
                        bSpecialThrowCount = true;
                        skillQuickThrowLevel = 30;
                        catchDelayRate = 0.22f;
                        throwType = ThrowType._INFIELD_SIDE_QUICK;
                    }
                    setThrow(coverIndex);
                    

                    Runner homeStealer = field.run.getDestRunner(FieldParm.HOMEBASE_INDEX);
                    if (homeStealer != null)
                    {
                        bool bHomeStealSuccess = SimulSteal.getHomeStealResult(pFielder, homeStealer.pRunner);
                        float timeLeft = field.getTimeLeftforThrow(FieldParm.HOMEBASE_INDEX, posX, posY, THROW_DELAY + 0.15f, THROW_SPEED);
                        if (bHomeStealSuccess == true)
                        {
                            homeStealer.setShobuRunnerSpeed(false, timeLeft, 2);
                        }
                        else
                        {
                            homeStealer.setShobuRunnerSpeed(true, timeLeft-0.15f, 1);
                        }

                        if (bSpecialThrowCount == true)
                        {
                            //딜레이 홈스틸 카운터 스페셜송구 연출
                            field.setSkillEffect(true, pFielder, SkillIndex.SpecialThrow);
                        }

                    }
                    field.runnerDelaySteal = FieldSkillUse.Init;
                }
                //태그동작을 하지 않음을 의미
                return true;
            }            
            return false;
        }*/

        public void addTaggingDelay(float addDelay)
        {
            taggingDelay += addDelay;
        }

        private void tagging()
        {
            //블록킹 스킬을 체크한다
            //checkBlockSkillOnTagging();

            //딜레이 스킬에 반응한다
            //if (checkDelayStealStateOnTagging() == true) return;


            //정상적인 태그하는 동작
            ////UnityEngine.//Debug.Log("==========================>>태그하는 중!! taggingDelay = " + taggingDelay);
            if (curTime >= taggingDelay)// FieldingMechanism.DELAY_TAGGING) 
            {
                if (moveStep == 0)
                {
                    //UnityEngine.Debug.Log("##########################포스아웃 체크 필더 STEP 2 볼도착");
                    field.run.bBallOnBase[field.nTargetIndex] = true;
                    moveStep = 1;
                }
                else if (moveStep == 1)
                {
                    //////UnityEngine.//Debug.Log("==========================>>field.run.bOnRunning[field.nTargetIndex] = " + field.run.bOnRunning[field.nTargetIndex]);
                    if (field.run.bOnRunning[field.nTargetIndex] == false)
                    {
                        field.checkSafeCall(field.nTargetIndex);

                        /*if (posIndex == CPlayer._CATCHER)
                        {
                            if (tagState == 100)
                            {
                                ////UnityEngine.//Debug.Log("========================>> 태그 했으나 포수가 짐");
                            }
                            else if (tagState == 200)
                            {
                                ////UnityEngine.//Debug.Log("========================>> 블록 했으나 포수가 짐");
                            }
                        }*/
                    }
                    field.returnCheck(-3);
                    moveStep = 2;
                    //}
                }
                else if (moveStep == 2)
                {
                    //bTaggingAnim = false;
                    //////UnityEngine.//Debug.Log("==========================>>여기로 들어오는가!!");
                    if (field.run.bOnBase[field.nTargetIndex] == true)
                    {
                        setState(FielderAction._MOTION, ActionStep._MOTION_SET);//actState = FielderAction._THROW_READY;
                        curTime = 0;
                    }

                    if (field.checkTagWaitEnd() == true)
                    {
                        if (field.manager.nOutCount < 2 && field.manager.bThreeOutChange == false)
                        {
                            int coverIndex = field.getBaseCoverIndex(field.nTargetIndex);
                            ////UnityEngine.//Debug.Log("==========================>>nTargetIndex = "+field.nTargetIndex);
                            Runner runner = field.run.getDestRunner(field.nTargetIndex);
                            if (runner != null)
                            {
                                ////UnityEngine.//Debug.Log("==========================>>runner = " + runner.nState);
                                if (runner.state == RunState.MOVE && runner.bMoveForward == true)
                                {
                                    bSpecialAnim = false;
                                    bThrowAvailable = true;
                                    setThrow(coverIndex);
                                }
                            }
                        }
                    }

                }
            }
        }



        //////////////////////////////////////////////////////////////////
        // LongTag State
        //////////////////////////////////////////////////////////////////
        private void longTagFrame()
        {
            if (curTime > longTagDelay)
            {
                float delay = FieldingMechanism.DELAY_THROW_NORMAL;
                if (SimulSteal.catcherSitThrow == FieldSkillUse.Active)
                {
                    //앉아쏴 세팅
                    setSitThrowOn();
                    delay = FieldingMechanism.DELAY_SIT_THROW;                
                }
                                
                throwSpeedRate = 1;
                ////UnityEngine.//Debug.Log("========================================================>>field.stealBaseTarget: " + field.stealBaseTarget + " 송구속도: " + THROW_SPEED);
                if (field.stealBaseTarget >= FieldParm.SECONDBASE_INDEX)
                {
                    float timeLeft = field.getTimeLeftforThrow(field.stealBaseTarget, posX, posY, delay + 0.3f, THROW_SPEED);
                    Runner destRunner = field.run.getDestRunner(field.stealBaseTarget);
                    if (destRunner != null)
                    {
                        if (field.stealSuccess == true)
                        {
                            destRunner.setShobuRunnerSpeed(false, timeLeft - Random.Range(0.0f, 0.2f), 2);
                        }
                        else
                        {                            
                            //터보발동해서 죽은경우 일반의 경우와 느낌을 다르게 하기 위해...
                            if(SimulSteal.runnerStealMarster == FieldSkillUse.Fail)
                            {
                                //터보스틸이 발동했으나 죽은 경우                                
                                destRunner.setShobuRunnerSpeed(false, timeLeft + Random.Range(0.5f,0.6f), 2);
                            }
                            else
                            {
                                //그냥 죽은 경우
                                destRunner.setShobuRunnerSpeed(true, timeLeft + Random.Range(0.2f, 0.6f), 2);
                            }
                        }
                    }
                }

                setState(FielderAction._MOTION, ActionStep._MOTION_SET);
            }
        }

        //////////////////////////////////////////////////////////////////
        // PickOff State
        //////////////////////////////////////////////////////////////////
        private void pickOffFrame()
        {
            if (curTime > FieldingMechanism.DELAY_PICKOFF)
            {
                bThrowAvailable = true;
                int coverIndex = field.getBaseCoverIndex(field.nTargetIndex);
                if (coverIndex != -1 || posIndex >= CPlayer._LEFTFIELDER)
                {
                    throwSpeedRate = 1.0f;
                    setThrow(coverIndex);

                    Runner pickOffRunner = field.run.getRunner(field.nTargetIndex);
                    if (pickOffRunner != null)
                    {
                        /*
                        if (SimulSteal.pitcherLaserPickoff == FieldSkillUse.Success)
                        {
                            //견제 아웃
                        }
                        else // FieldSkillUse.Fail
                        {
                            //견제 세입
                        }*/
                    }
                }

            }
        }

        //////////////////////////////////////////////////////////////////
        // Collision State
        //////////////////////////////////////////////////////////////////
        private void collisionFrame()
        {
            if (aStep == ActionStep._CATCHER_BLOCK)
            {
                catcherBlock();
            }
            else if (aStep == ActionStep._CATCHER_CRUSHED)
            {
                catcherCrushed();
            }
            else if (aStep == ActionStep._DOUBLEPLAY_STOP)
            {
                doublePlayStop();
            }          
            else if (aStep == ActionStep._FIELDER_COLLISION)
            {
                fielderCollision();
            }


        }

        //Collision - 캐처블럭
        private void catcherBlock()
        {
            if (curTime > FieldingMechanism.DELAY_CATCHER_BLOCK)
            {
                setState(FielderAction._MOTION, ActionStep._MOTION_SET);
            }
        }

        //Collision - 캐처충돌
        private void catcherCrushed()
        {
            posX += (dX * deltaTime);
            posY += (dY * deltaTime);

            dX *= 0.98f;
            dY *= 0.98f;

            if (curTime > FieldingMechanism.DELAY_CATCHER_CRUSH)
            {
                bSpecialAnim = false;
                field.judge.setCall(FieldParm.HOMEBASE_INDEX, CallType._SAFE);
                IngameUI.GetFieldCall().Call("safe");
                setBaseCover(FieldParm.HOMEBASE_INDEX);
            }
        }

        //Collision - 병살저지
        private void doublePlayStop()
        {
            if (curTime > FieldingMechanism.DELAY_DP_STOP)
            {
                setState(FielderAction._MOTION, ActionStep._MOTION_SET);
            }
        }



        //야수간 충돌
        private void fielderCollision()
        {
            if (moveStep == 0)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);

                dX *= 0.97f;
                dY *= 0.97f;

                if (curTime > FieldingMechanism.DELAY_FIELDER_COLLISION)
                {
                    curTime = 0;
                    moveStep = 1;

                    playSpecialAnim("FIELDER_CRASH_STUN_" + FieldParm._dir[nFielderDir], true);
                }
            }
            else
            {
                if (curTime > FieldingMechanism.DELAY_FIELDER_COLLISION)
                {
                    //CameraManager.CameraPositionInit();
                    bSpecialAnim = false;
                    if (posIndex < CPlayer._LEFTFIELDER)
                    {
                        if (field.ball.step == BallStep.BALL_HIT)
                        {
                            StartCoroutine(setErrorCaseFormation());
                        }
                        else
                        {
                            setSecondMove();
                        }
                    }
                    else
                    {   
                        if (field.ball.step == BallStep.BALL_HIT)
                        {
                            setBallChase();
                        }
                        else
                        {
                            if (field.ball.bBallCatched == true && field.nThrowIndex == posIndex)
                            {
                                setState(FielderAction._MOTION, ActionStep._MOTION_SET);
                                curTime = 0;
                            }
                            else
                            {
                                setStop();
                            }
                        }
                    }
                    moveStep = 2;
                }
            }
        }


        //////////////////////////////////////////////////////////////////
        //POSITION 백업
        //////////////////////////////////////////////////////////////////
        public void getBackupPos()
        {
            int runner = field.run.getRunnerOnGround();// 0;// 0;
            int outCount = field.manager.nOutCount;

            //1 : 1루
            //11: 12루
            //101:: 13루
            //111: 만루

            //10 : 2루
            //110 : 23루

            //100: 3루

            if (posIndex == CPlayer._PITCHER)
            {
                pitcherCover(outCount, runner);
            }
            else if (posIndex == CPlayer._CATCHER)
            {
                catcherCover(outCount, runner);
            }
            else if (posIndex == CPlayer._FIRSTBASEMAN)
            {
                firstCover(outCount, runner);
            }
            else if (posIndex == CPlayer._SECONDBASEMAN)
            {
                secondCover(outCount, runner);
            }
            else if (posIndex == CPlayer._THIRDBASEMAN)
            {
                thirdCover(outCount, runner);
            }
            else if (posIndex == CPlayer._SHORTSTOP)
            {
                ////////UnityEngine.//Debug.Log("================>>SHORT COVER");
                shortCover(outCount, runner);
            }

            //노주자   

        }

        //투수 커버플레이
        private void pitcherCover(int outCount, int runner)
        {
            if (field.bFoulFlyOut == true) return;
            if (field.ball.bHomeRunGuess == true) return;
            if (field.flyCatchFielder == CPlayer._CATCHER) return;
            if (field.groundCatchFielder == CPlayer._CATCHER) return;

            if (field.manager.nOutCount == 2 || runner == 0)
            {
                //2아웃 또는 주자 없는 경우
                if (field.fielder[CPlayer._FIRSTBASEMAN].bGrounderAvail
                 || field.fielder[CPlayer._FIRSTBASEMAN].bGrounderTry)
                {
                    if (field.ball.firstAngle < FieldingMechanism.FIRSTBASE_COVER_LIMIT_ANGLE)
                    {
                        if (bGrounderAvail == false)
                        {
                            setBaseCover(FieldParm.FIRSTBASE_INDEX);
                        }
                        else
                        {
                            //투수 자신도 수비하는 경우 2루수한테 커버 시켜
                            field.fielder[CPlayer._SECONDBASEMAN].setBaseCover(FieldParm.FIRSTBASE_INDEX);
                        }
                    }
                }
                else
                {

                }
            }
            else
            {
                if (field.fielder[CPlayer._FIRSTBASEMAN].bGrounderAvail
                 || field.fielder[CPlayer._FIRSTBASEMAN].bGrounderTry)
                {
                    if (field.ball.firstAngle < -25)
                    {
                        if (bGrounderAvail == false)
                        {
                            setBaseCover(FieldParm.FIRSTBASE_INDEX);
                        }
                        else
                        {
                            //투수 자신도 수비하는 경우 2루수한테 커버 시켜
                            field.fielder[CPlayer._SECONDBASEMAN].setBaseCover(FieldParm.FIRSTBASE_INDEX);
                        }
                    }
                }
                else
                {
                    //안타시
                    //주자 1루에 있는 경우 ->3루 백업
                    //주자 2루에 있는 경우 ->홈 백업
                }
            }
        }

        //포수 커버 플레이
        private void catcherCover(int outCount, int runner)
        {
            if (field.ball.firstAngleZ > 60 || field.firstBallSpeed < FieldingMechanism.CATCHER_FIELDING_BALLSPEED)
            {
                return;
            }

            if (field.batter.bBunt == true)
            {
                if (field.ballPower < 6.5f && field.run.bOnBase[FieldParm.THIRDBASE_INDEX] == false)
                {
                    //setBallChase();
                    setFieldingReady();
                    curTime = 0;
                    bGrounderTry = true;
                    return;
                }

            }
            if (field.manager.nOutCount == 2 || runner == 0)
            {
                //2아웃 또는 주자 없는 경우
                if (field.bGrounderAvailble == true)
                {
                    /*    if (runner == 0)
                        {
                            if (field.fielder[CPlayer._SECONDBASEMAN].bGrounderAvail == true //2루수
                                    || field.fielder[CPlayer._THIRDBASEMAN].bGrounderAvail == true //3루수
                                    || field.fielder[CPlayer._SHORTSTOP].bGrounderAvail == true) //유격수
                            {
                                //1루 백업
                                setBackupPosition(Field._CATCHER_FIRST_BACKUP_POSX, Field._CATCHER_FIRST_BACKUP_POSY);
                            }
                        }
                        else*/
                    {
                        //홈베이스 커버
                        setBaseCover(FieldParm.HOMEBASE_INDEX);
                    }

                }
                else
                {
                    if (bFlyCatchAvail == false)
                    {
                        //홈베이스 커버
                        setBaseCover(FieldParm.HOMEBASE_INDEX);
                    }
                }
            }
            else
            {
                setBaseCover(FieldParm.HOMEBASE_INDEX);
            }
        }

        //1루수의 커버 플레이
        private void firstCover(int outCount, int runner)
        {
            if (field.bGrounderAvailble == true)
            {
                if (bGrounderAvail == false
                     && bGrounderTry == false
                     && bFlyCatchAvail == false)
                {
                    setBaseCover(FieldParm.FIRSTBASE_INDEX);
                }
                else
                {
                    //주자 2or3루에 있는 경우 안타시
                    //홈베이스 릴레이 포지션
                }
            }
            else
            {
                if (bFlyCatchAvail == true || bDeepFlyChase == true || bGrounderTry == true || bFlyCatchTry == true)
                {
                    //액션없음
                }
                else
                {
                    if (field.ball.firstAngleZ < 10 && posX < tryDstX) 
                    {
                        //1루수 선상 멀뚱 고침
                        StartCoroutine(setGrounderTry(grounderRemainTime * 0.5f));
                    }
                    else
                    {
                        setBaseCover(FieldParm.FIRSTBASE_INDEX);
                    }
                }
            }
        }

        //2루수의 커버 플레이
        private void secondCover(int outCount, int runner)
        {
            //번트 수비 익셉션
            if (field.bBuntFielding == true && field.batter.buntFielder == CPlayer._FIRSTBASEMAN)
            {
                if (bGrounderAvail == false)
                {
                    setBaseCover(FieldParm.FIRSTBASE_INDEX);
                    return;
                }
            }
            //번트 수비 익셉션

            //2아웃 또는 주자 없는 경우
            if (field.bGrounderAvailble == true) //if (field.grounder == true)
            {
                if (bGrounderAvail == false)
                {
                    if (bGrounderTry == true)
                    {
                        nextMove = NextAction._NEXT_FIRSTBASE_BACKUP;
                    }
                    else if (bFlyCatchTry == true)
                    {
                        nextMove = NextAction._RELAY_POSITION;
                    }
                    else
                    {
                        //2루 커버
                        if (field.fielder[CPlayer._THIRDBASEMAN].bGrounderAvail == true) //3루수
                        {
                            setBaseCover(FieldParm.SECONDBASE_INDEX);
                        }
                        else if (field.fielder[CPlayer._SHORTSTOP].bGrounderAvail == true) //유격수
                        {
                            //if (field.run.bOnRunning[FieldParm.SECONDBASE_INDEX] == true)
                            if (field.run.bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
                            {
                                setBaseCover(FieldParm.SECONDBASE_INDEX);
                            }
                            else
                            {
                                //딜레이 커버 플래그 온
                                bSecondDelayCover = true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (bFlyCatchAvail == true || bDeepFlyChase == true || bGrounderTry == true || bFlyCatchTry == true)
                {
                }
                else
                {
                    if (field.ball.angle < 180) //좌측 플라이 타구
                    {
                        //2루 백업
                        setBaseCover(FieldParm.SECONDBASE_INDEX);
                    }
                    else //우측 타구
                    {
                        if (field.ball.firstAngleZ > 20 && field.infieldFlyOut == false && field.ball.bHomeRunGuess == false)
                        {
                            //2루 릴레이 포인트
                            int curBase = field.run.getFirstRunnerDest(0, 10000) + (field.flyCatchAvaiableCount == 0 ? 1 : 0);
                            setRelayPosition(curBase, field.ball.nFirstBoundX, field.ball.nFirstBoundY);
                        }
                        else
                        {
                            //2루수 중계 멀뚱 고침
                            StartCoroutine(setGrounderTry(grounderRemainTime*0.3f));
                        }

                    }
                }
            }
        }

        //3루수의 커버 플레이
        private void thirdCover(int outCount, int runner)
        {
            if (field.bGrounderAvailble == true)
            {
                //2아웃 또는 주자 없는 경우
                if (bGrounderAvail == false
                 && bGrounderTry == false
                 && bFlyCatchAvail == false)
                {
                    if (field.bGrounderAvailble == false || runner != 0)
                    {
                        setBaseCover(FieldParm.THIRDBASE_INDEX);
                    }
                }
                else
                {
                }
            }
            else
            {
                if (bFlyCatchAvail == true || bDeepFlyChase == true || bGrounderTry == true || bFlyCatchTry == true)
                {
                    //액션없음
                }
                else
                {
                    if (field.ball.firstAngleZ < 10 && posX > tryDstX)
                    {
                        //3루수 선상 멀뚱 고침
                        StartCoroutine(setGrounderTry(grounderRemainTime * 0.5f));
                    }
                    else
                    {
                        setBaseCover(FieldParm.THIRDBASE_INDEX);
                    }
                }
            }
         
        }

        //유격수의 커버 플레이
        private void shortCover(int outCount, int runner)
        {
            //번트 수비 익셉션
            if (field.bBuntFielding == true && field.batter.buntFielder == CPlayer._THIRDBASEMAN)
            {
                //무조건
                setBaseCover(FieldParm.THIRDBASE_INDEX);
                return;
            }//번트 수비 익셉션

            if (field.bGrounderAvailble == true)
            {
                if (bGrounderAvail == false)
                {
                    if (bGrounderTry == true)
                    {
                        nextMove = NextAction._NEXT_SECONDBASE_COVER;
                    }
                    else if (bFlyCatchTry == true)
                    {
                        nextMove = NextAction._RELAY_POSITION;
                    }
                    else
                    {
                        if (field.fielder[CPlayer._PITCHER].bGrounderAvail == true //투수
                             || field.fielder[CPlayer._CATCHER].bGrounderAvail == true //포수
                            || field.fielder[CPlayer._FIRSTBASEMAN].bGrounderAvail == true //1루수
                            || field.fielder[CPlayer._SECONDBASEMAN].bGrounderAvail == true) //2루수
                        {
                            setBaseCover(FieldParm.SECONDBASE_INDEX);
                        }
                    }
                }
            }
            else
            {
                if (bFlyCatchAvail == true || bDeepFlyChase == true || bGrounderTry == true || bFlyCatchTry == true)
                {
                    //액션 없음
                }
                else
                {
                    if (field.ball.angle < 180) //좌측 플라이 타구
                    {
                        if (field.ball.firstAngleZ > 20 && field.infieldFlyOut == false && field.ball.bHomeRunGuess == false)
                        {
                            //2루 릴레이 포인트
                            ////////UnityEngine.//Debug.Log("=====================>>flyCatchAvaiableCount = " + field.flyCatchAvaiableCount);
                            int curBase = field.run.getFirstRunnerDest(0, 10000) + (field.flyCatchAvaiableCount == 0 ? 1 : 0);
                            setRelayPosition(curBase, field.ball.nFirstBoundX, field.ball.nFirstBoundY);
                        }
                        else
                        {
                            //유격수 중계 멀뚱 고침
                            StartCoroutine(setGrounderTry(grounderRemainTime * 0.3f));
                        }
                    }
                    else //우측 타구
                    {
                        //2루 백업
                        setBaseCover(FieldParm.SECONDBASE_INDEX);
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////
        //에러처리
        /////////////////////////////////////////////////////////////////
        //에러 플래그 강제 초기화 -> 스킬발동시
        private void errorForceInit()
        {
            bCatchErrorFlag = false;
            bThrowErrorFlag = false;
        }

        //에러 프레임
        private void errorPanicFrame()
        {
            if (moveStep == 0)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);
                //makeDust(0.2f, "2");
                dX = dX * 0.98f;
                dY = dY * 0.98f;

                if (curTime > FieldingMechanism.DELAY_CATCH_ERROR)
                {
                    dX = dY = 0;
                    field.bDelayedCall = false; //이놈이 true인경우 넘어가지 않는 에러가 생기곤함
                    bSpecialAnim = false;

                    if (field.manager.batterSkillFlag == SkillFlag.AssaultBall)
                    {
                        //강습타구 예외처리
                        playSpecialAnim("FIELDER_CRASH_STUN_" + FieldParm._dir[nFielderDir], true);
                        curTime = 0;
                        moveStep = 1;
                    }
                    else
                    {
                        setBallChase();
                    }
                }
            }
            else if (moveStep == 1)
            {
                if (curTime > 3.0f)
                {
                    if (field.ball.step == BallStep.BALL_HIT) setBallChase();
                    else setSecondMove();
                }
            }
        }



        /// <summary>
        /// 포구시 에러 세팅
        /// </summary>
        /// <param name="bThrowState"></param>
        /// <param name="bFlyState"></param>
        /// <param name="bDashCase"></param>
        /// <returns></returns>
        public IEnumerator setErrorEvent(bool bThrowState, bool bFlyState, bool bDashCase = false)//, bool bFumbleWithNoError = false)
        {
            //강습타구가 철벽수비를 만난경우
            if (field.manager.batterSkillFlag == SkillFlag.AssaultBall)
            {
                if(skillSlidingCatchLevel > 0)
                {
                    if (posIndex >= CPlayer._FIRSTBASEMAN && posIndex <= CPlayer._SHORTSTOP)
                    {
                        
                        //Debug.Log("====================>> 강습타구를 철벽수비로 무효화");
                        fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.SpecialCatch);
                        bCatchErrorFlag = false;
                        catchDelayRate = 2;
                        nFielderDir = FieldParm._SOUTH;
                        playSpecialAnim((MyMath.Half() ? "4400_PITCHER_SPECIAL_1" : "4400_PITCHER_SPECIAL_2"), false);
                        setCatch(false, true);
                        yield return new WaitForSeconds(1.5f);
                        setState(FielderAction._MOTION, ActionStep._MOTION_SET);
                        bSpecialAnim = false;
                        yield break;
                    }
                }
            }

            //예외처리
            if (posIndex == CPlayer._SHORTSTOP)
            {
                if (field.bBaseCoverd[FieldParm.SECONDBASE_INDEX] == false)
                {
                    field.fielder[CPlayer._SECONDBASEMAN].setBaseCover(FieldParm.SECONDBASE_INDEX);
                }
            }

            //field.setZoomCameraSetting(false);

            ////UnityEngine.//Debug.Log("========================================================================>>setErrorEvent !!!");
            field.run.bHitAndRun = false;
            field.ball.bCameraBallMove = true;  //카메라 공 움직임 따라

            CameraManager.SetActiveCameraDstAngle(-15, 1.0f);
            field.setZoomTo(1.0f, 0.5f);        //에러 발생시 줌
            CameraManager.FieldCameraShake(0.1f, 5);

            if (bFlyState == true)
            {
                if (field.ball.firstAngle > -70 && field.ball.firstAngle < 70)
                {
                    float screenX = field.getScreenX(posX);
                    float screenY = field.getScreenY(posY);
                    if (field.ball.foulEquation(screenX, screenY, FieldSize.getHomePosX(), FieldSize.getHomePosY(), FieldSize.FENCE_RIGHT_POLE_X, FieldSize.FENCE_RIGHT_POLE_Y) == false)
                    {
                        field.ball.bNoFoulCheck = true;
                    }
                }
            }

            field.nErrorFielder = posIndex;
            setNoCatchCollider();

            yield return new WaitForEndOfFrame();

            if (bCatchErrorSpeicalAnimation == true)
            {
                dX = dY = 0;
                setNextMove(getHoldIndexStr(), true, 1);
                bCatchErrorSpeicalAnimation = false;
            }


            //if (bFumbleWithNoError == false)
            {
                field.bErrorFlag = true;
                StartCoroutine(setErrorMark(0.5f));// //errorMark.GetComponent<Renderer>().enabled = true;
            }
                        
            actState = FielderAction._ERROR_PANIC;
            curTime = 0;
            moveStep = 0;
            field.setCatchErrorInit();

            if (bThrowState == true)
            {
                if (field.run.bPickOff == true || field.run.bStealBase == true)
                {
                    //도루 초기화
                    field.run.stealResult = SimulStealState.NONE; 
                    field.run.stealCount = -1;

                    //송구에러시 도루 혹은 견제인 경우 2중 에러 나오지 않게 한다 : 버그의 위험성이 너무큼
                    field.setThrowErrorInit();
                    //SimulManager.AddGameSummuryInfo("\n-" + (field.run.bPickOff ? "견제" : "도루") + "시 송구 에러");

                    if (field.run.bPickOff == true)
                    {
                        //견제인 경우 따로 처리
                        for (int i = 0; i <= FieldParm.THIRDBASE_INDEX; i++)
                        {
                            Runner runner = field.run.getRunner(i);
                            if (runner != null)
                            {
                                Debug.Log("견제 에러시 =====================>> " + (i+1) +"루 주자 강제 이동");
                                runner.setThrowErrorSetting(i);
                                runner.setMoveOnBase();
                            }
                        }
                    }
                }
                /*else
                {
                    if (field.nThrowIndex != -1)
                    {
                        Fielder Thrower = field.fielder[field.nThrowIndex];
                        SimulManager.AddGameSummuryInfo("\n-" + Util.GetPositionString(field.nThrowIndex) + " " + Thrower.pFielder.getName() + "의 송구 에러");
                    }
                }*/

                StartCoroutine(setThrowErrorType());
            }
            else
            {
                if (bFlyState == true)
                {
                    //SimulManager.AddGameSummuryInfo("\n-" + Util.GetPositionString(posIndex) + " " + pFielder.getName() + "의 포구 에러");
                    setFlyErrorType();
                }
                else
                {
                    //SimulManager.AddGameSummuryInfo("\n-" + Util.GetPositionString(posIndex) + " " + pFielder.getName() + "의 포구 에러");
                    setGrounderErrorType(bDashCase);//, bFumbleWithNoError);
                }
            }
            float delay = FieldingMechanism.DELAY_CATCH_ERROR;
            yield return new WaitForSeconds(delay);

            //CameraManager.CameraPositionInit();

            bSpecialAnim = false;
            setCollider();
        }

        /// <summary>
        /// 플라이 에러타입
        /// </summary>
        private void setFlyErrorType()
        {
            int limit = 50;
            bool bFumble = (MyMath.Percent() < limit || bStun == true) ? true : false;

#if _Local_Balance
            if (InGameDebug._ALWAYS_ERROR_FUMBLE == true && MyMath.Percent() < tempSelectPage.ERROR_PER)
            {
                //로컬 밸런스로 언제나 펌블 에러 발생하게
                bFumble = true;
            }
#endif
            if (bFumble == true)
            {
                setFumble(true);
                field.errorType = FieldParm.ErrorType.Fumble;
            }
            else
            {
                field.errorType = FieldParm.ErrorType.Drop;
            }
        }

        private void setGrounderErrorType(bool bDashCase)//, bool bFumbleWithNoError = false)
        {
            int limit = (posIndex < CPlayer._LEFTFIELDER ? 50 : 75);
            bool bFumble = (MyMath.Percent() < limit || bDashCase == true || bStun == true) ? true : false;
            
            bool bShortFumble = false;
            if (field.manager.batterSkillFlag == SkillFlag.AssaultBall)
            {
                //강습타구 예외처리
                field.bErrorFlag = false; //에러로 기록 안한다
                bShortFumble = true;      //숏펌블로 처리  
                CameraManager.FieldCameraShake(0.3f, 20); //카메라 연출
                field.setZoomTo(1.3f, 0.3f);              //줌처리
            }

            if (bFumble == true || bShortFumble == true)// || 
            {
                field.setErrorCollisionEffect(transform.localPosition, (dX * 0.3f)); //펌블 이펙트 처리
                setFumble(bShortFumble);//, false);
                field.errorType = FieldParm.ErrorType.Fumble;
                bErrorFieldingFirstCheck = true;
             
                StartCoroutine(setErrorCaseFormation());
                
                if (posIndex < CPlayer._LEFTFIELDER)
                {
                    if (posIndex == CPlayer._FIRSTBASEMAN || posIndex == CPlayer._SECONDBASEMAN)
                    {
                        field.fielder[CPlayer._RIGHTFIELDER].setBallChase();                        
                    }
                    else if (posIndex == CPlayer._THIRDBASEMAN || posIndex == CPlayer._SHORTSTOP)
                    {
                        field.fielder[CPlayer._LEFTFIELDER].setBallChase();
                    }
                    field.fielder[CPlayer._CENTERFIELDER].setBallChase();
                }
            }
            else
            {
                field.errorType = FieldParm.ErrorType.Tunnel;
            }
        }

        private void setFumble(bool bFly)//, bool bShortFumble = false)
        {
            //펌블의 경우
            FBall ball = field.ball;            

            /*
            float initBallPower;
            if (bShortFumble == false)
                initBallPower = MyMath.SetMinMax(field.ballPower, 8, 26);
            else
                initBallPower = 10;// MyMath.SetMinMax(field.ballPower, 7, 9);*/

            float initBallPower = MyMath.SetMinMax(field.ballPower, 8, 26);
            ball.speed = FBall._BALLSPEED_COEF * initBallPower * 0.2f * (posIndex < CPlayer._LEFTFIELDER ? 1 : 0.4f);
            ball.angle += Random.Range(0.0f, 360.0f);
            ball.nBallDZ = bFly ? Random.Range(0.0f, 100.0f) : Random.Range(250.0f, 300.0f);

            ball.setVelocity();
        }

        private int errorCoverFielder;              //에러 커버하는 야수 인덱스
        private bool bErrorFieldingFirstCheck;      //처음 에러 커버하는지 여부

        //포구에러시 에러 포메이션
        private IEnumerator setErrorCaseFormation(float nextTime = 0.3f) //진짜
        {
            float px = field.ball.nBallX + (field.ball.nBallDX * nextTime); //0.3초후의 거리를 체크한다
            float py = field.ball.nBallY + (field.ball.nBallDY * nextTime);

            int coverIndex;
            int closeIndex = field.getCloseFielderIndexError((bErrorFieldingFirstCheck ? posIndex : -1), (posIndex < CPlayer._LEFTFIELDER ? true : false), px, py);
            if (bErrorFieldingFirstCheck == true)
            {
                coverIndex = closeIndex;
                errorCoverFielder = coverIndex;
            }
            else
            {
                coverIndex = closeIndex;
            }

            if (bErrorFieldingFirstCheck == true || coverIndex != errorCoverFielder)
            {
                ////UnityEngine.//Debug.Log("====================================================================>> 새로운 야수 " + coverIndex + " 야수에 의해 커버 플레이");
                Fielder coverFielder = field.fielder[coverIndex];
                if (coverFielder.bBaseCovering == true)
                {
                    ////UnityEngine.//Debug.Log("====================================================================>> 새로운 야수 " + coverIndex + " 베이스 커버중이면 무효화");
                    //에러커버 야수에 의해 베이스 커버된 경우 - 베이스 커버의 무효화
                    field.bBaseCoverd[coverFielder.nCoveringIndex] = false;
                    coverFielder.bBaseCovering = false;
                }

                //에러커버 야수는 공을 쫒음
                coverFielder.setBallChase();
                                
                if (coverIndex != errorCoverFielder)
                {
                    ////UnityEngine.//Debug.Log("====================================================================>> 이전 야수는 " + errorCoverFielder + " 제2동작");
                    field.fielder[errorCoverFielder].bSecondMove = false;
                    field.fielder[errorCoverFielder].setSecondMove();
                    errorCoverFielder = coverIndex;
                }                
            }

            float delay = (bErrorFieldingFirstCheck ? FieldingMechanism.DELAY_CATCH_ERROR : FieldingMechanism.DELAY_ERROR_CHECK);

            if (field.manager.batterSkillFlag == SkillFlag.AssaultBall)
            {
                //강습타구 예외처리
                delay = 3.0f;
            }

            yield return new WaitForSeconds(delay);

            if (field.ball.step == BallStep.BALL_HIT)
            {
                ////UnityEngine.//Debug.Log("====================================================================>> 1초후 재검색");
                bErrorFieldingFirstCheck = false;
                StartCoroutine(setErrorCaseFormation(nextTime));
            }
            else
            {
                field.setBaseCoverAfterThrowError(coverIndex);
            }
        }

        //송구에러시 에러 포메이션
        private IEnumerator setErrorCaseFormation2(float nextTime = 1.0f) 
        {

            float px = field.ball.nBallX + (field.ball.nBallDX * nextTime); //0.3초후의 거리를 체크한다
            float py = field.ball.nBallY + (field.ball.nBallDY * nextTime);

            int coverIndex;
            int closeIndex = field.getCloseFielderIndexError(-1, false, px, py);
            
            if (bErrorFieldingFirstCheck == true)
            {
                coverIndex = closeIndex;
                errorCoverFielder = coverIndex;
            }
            else
            {
                coverIndex = closeIndex;
            }

            
            if (bErrorFieldingFirstCheck == true || coverIndex != errorCoverFielder)
            {
                Fielder coverFielder = field.fielder[coverIndex];
                
                if (coverFielder.bBaseCovering == true)
                {
                    ////UnityEngine.//Debug.Log("====================================================================>> 새로운 야수 " + coverIndex + " 베이스 커버중이면 무효화");
                    //에러커버 야수에 의해 베이스 커버된 경우 - 베이스 커버의 무효화
                    field.bBaseCoverd[coverFielder.nCoveringIndex] = false;
                    coverFielder.bBaseCovering = false;            
                }

                field.bBaseCoverd[FieldParm.SECONDBASE_INDEX] = false;
                field.fielder[CPlayer._SECONDBASEMAN].bBaseCovering = false;
                field.fielder[CPlayer._SHORTSTOP].bBaseCovering = false;

                //에러커버 야수는 공을 쫒음
                coverFielder.setBallChase();

                //송구 실책후 베이스 커버
                field.setBaseCoverAfterThrowError(coverIndex);

                if (bErrorFieldingFirstCheck == true)
                {
                    float ballX = field.ball.nBallX + (field.ball.nBallDX * 2.0f); //2초후 볼위치
                    float ballY = field.ball.nBallY + (field.ball.nBallDY * 2.0f); //2초후 볼위치
                    for(int i =0; i< 4; i++)
                    {
                        if (field.run.runnerActive[i] == true)
                        {
                            bool bMovable = true;
                            Runner dstRunner = field.run.runner[i];

                            if (dstRunner != null)
                            {
                                //타자 주자가 뛰지 말아야 할 경우 체크
                                if (((field.run.bPickOff == true || field.run.bStealBase == true) && dstRunner.destPos == FieldParm.FIRSTBASE_INDEX)    //도루 픽오프 에러시 타자 주자 안움직이게
                                    || (field.run.bWildPitchRunning == true && field.wildPitchCase == FieldParm.WildPitchCase.RunnerOnBase && dstRunner.destPos == FieldParm.FIRSTBASE_INDEX)) //와일드피치시 타자주자 안움직이게
                                {
                                    ////UnityEngine.//Debug.Log("===========================================>> " + dstRunner.runnerIndex + " 타자주자 도루 견제 송구 에러시 안움직임");
                                    bMovable = false;
                                }

                                if (field.run.bPickOff == true)
                                {
                                    ////UnityEngine.//Debug.Log("===========================================>> " + dstRunner.runnerIndex + " 타자주자 도루 견제 송구 에러시 안움직임");
                                    bMovable = false;
                                }

                                if (bMovable == true)
                                {
                                    ////UnityEngine.//Debug.Log("===========================================>> " + dstRunner.runnerIndex + "주자 에러로 움직임!! = dstRunner.nState = " + dstRunner.nState);
                                    if (dstRunner.checkOneMoreBaseAfterError(coverFielder, ballX, ballY) == true)
                                    {
                                        if (dstRunner.state == RunState.STANDBY ||
                                            dstRunner.state == RunState.WAIT ||
                                            dstRunner.state == RunState.CHECK ||
                                            dstRunner.state == RunState.FIRSTBASE_SAFE)
                                        {
                                            if (dstRunner.bSlidingMotion == true)
                                            {
                                                dstRunner.bForcedOneMoreBase2 = true;
                                            }
                                            dstRunner.lastState = RunState.NONE;
                                            dstRunner.lastStrID = "";
                                            dstRunner.setMove();
                                        }
                                        else if (dstRunner.state == RunState.SECOND_THIRD_SAFE)
                                        {
                                            dstRunner.bForcedOneMoreBase2 = true;                                            
                                        }
                                        else
                                        {
                                            dstRunner.bForcedOneMoreBase = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            float delay = FieldingMechanism.DELAY_ERROR_CHECK;

            yield return new WaitForSeconds(delay);

            if (field.ball.step == BallStep.BALL_ERROR_STATE)
            {
                ////UnityEngine.//Debug.Log("====================================================================>> 1초후 재검색");
                bErrorFieldingFirstCheck = false;
                StartCoroutine(setErrorCaseFormation2(nextTime));
            }
        }

        //에러처리중 야수간 충돌
        public void setCollisionFielders(int dir)
        {
            if (posIndex != CPlayer._CATCHER)
            {
                //포수는 예외처리
                setState(FielderAction._COLLISION, ActionStep._FIELDER_COLLISION);                
                float[,] sign = new float[8, 2] { { 0, 1 }, { 0.7f, 0.7f }, { 1, 0 }, { 0.7f, -0.7f }, { 0, -1 }, { -0.7f, -0.7f }, { -1, 0 }, { -0.7f, 0.7f } };
                dX = -sign[dir, 0] * Mathf.Abs(dX);
                dY = -sign[dir, 1] * Mathf.Abs(dY);
                //nFielderDir = (dir+4) % 8;
                setNextMove(FieldingMechanism._CRASH, false, 1);
                curTime = 0;
                moveStep = 0;
                StartCoroutine(setErrorMark(0.5f));
            }
        }


        //송구에러 타입
        private IEnumerator setThrowErrorType()
        {
            field.setCatchErrorInit();
            //Debug.Log("@@@@@@@@3");
            field.bReturnBattingView = false;
            //field.errorType = FieldParm.ErrorType.WildThrow;
            field.bOnceWildThrow = true;
            field.ball.step = BallStep.BALL_ERROR_STATE;
            yield return new WaitForSeconds(0.5f);

            bErrorFieldingFirstCheck = true;
            StartCoroutine(setErrorCaseFormation2(1.0f));

        }


        //잘못된 곳으로 송구했을 경우
        private IEnumerator setWrongPlaceThrow()
        {
            setBaseCover(field.nTargetIndex);
            int dir = nFielderDir;

            yield return new WaitForSeconds(0.3f);

            nFielderDir = dir;
            bThrowErrorCoverd = true;
        }

        //////////////////////////////////////////////////////////////////
        //충돌 처리
        //////////////////////////////////////////////////////////////////
        //그라운더시 충돌체 세팅
        public void setCollider(GrounderCatch type = GrounderCatch.GROUNDERCATCH_NORMAL)
        {
            //float rate = 1.4f;
            //////UnityEngine.//Debug.Log("==================>>type = " + type);
            _collider.center = new Vector3(0, 30, 0);
            if (type == GrounderCatch.GROUNDERCATCH_MOVING)
            {
                //_collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(700, 200, 600); //new Vector3(500, 140, 400);
            }
            else if (type == GrounderCatch.GROUNDERCATCH_DIVING)
            {
                //_collider.center = new Vector3(0, -80, 0);
                _collider.size = new Vector3(850, 200, 600); //new Vector3(600, 200, 400);
            }
            else if (type == GrounderCatch.GROUNDERCATCH_MOVING_NORMAL)
            {
                //_collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(700, 200, 600); //new Vector3(500, 140, 400);
            }
            else if (type == GrounderCatch.GROUNDERCATCH_JUMP)
            {
                //////UnityEngine.//Debug.Log("==================>>setCollider GROUNDERCATCH_JUMP");
                //_collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(840, 280, 840); //new Vector3(600, 200, 400);
            }
            else if (type == GrounderCatch.GROUNDERCATCH_TRY)
            {
                //_collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(420, 280, 560);  //new Vector3(300, 200, 400);
            }
            else
            {
                _collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(280, 200, 600); //new Vector3(200, 140, 400);
            }
        }

        //플라이시 충돌체 세팅
        private void setFlyCollider(FlyCatch type = FlyCatch.FLYCATCH_NORMAL)
        {
            if (type == FlyCatch.FLYCATCH_JUMPING)
            {
                _collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(420, 340, 700);// new Vector3(300, 240, 600);
            }
            else if (type == FlyCatch.FLYCATCH_DIVING)
            {
                _collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(420, 340, 300);//new Vector3(300, 240, 300);
            }
            /*else if (type == FlyCatch.FLYCATCH_HOMERUNSTEAL)
            {
                _collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(560, 476, 800);//new Vector3(400, 400, 700);
            }*/
            else
            {
                _collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(350, 350, 350);//new Vector3(250, 250, 300);
            }
        }

        //송구시 충돌체 세팅
        private bool bThrowErrorException;
        private void setThrowingCatchCollider(bool bToss = false, bool bLaser = false)
        {
            if (bThrowErrorException == true)
            {
                bThrowErrorException = false;
                return;
            }
            float rate = 1;

            if (bToss == true)
            {
                _collider.center = new Vector3(0, 0, 0);
                _collider.size = new Vector3(rate * 240, rate * 140, rate * 300);
            }
            else
            {
                if (bLaser == true)
                {
                    rate = 1.8f;
                }

                _collider.center = new Vector3(0, 50, 0);
                _collider.size = new Vector3(rate * 300, rate * 300, rate * 300);
            }
        }

        //에러 처리용 충돌체 세팅
        public void setErrorException(float x, float y)
        {
            setThrowingCatchCollider(false, true);
            _collider.center = new Vector3(x, y, 0);
            bThrowErrorException = true;
        }

        //충돌체를 없앤다.
        public void setNoCatchCollider(bool bFlagOn = true)
        {
            bNoCatchFlag = bFlagOn;
            _collider.center = new Vector3(0, 0, 0);
            _collider.size = new Vector3(0, 0, 0);
        }
        
        //볼과의 충돌처리
        private void collideWithBall(Collider col)
        {
            ////UnityEngine.//Debug.Log("=================>>>OnTriggerStay2D ::: field.ball.step = " + field.ball.step);
            if (field.ball.step == BallStep.BALL_CATCH
              || field.ball.step == BallStep.BALL_THROW_CATCH
              || field.ball.step == BallStep.BALL_CARRY)
            {
                ////////UnityEngine.//Debug.Log("=================>>>OnTriggerStay2D Exception");
                return;
            }

            //////UnityEngine.//Debug.Log("=================>>>OnTriggerStay2D actState = " + actState);
            if (field.ball.step == BallStep.BALL_HIT)
            {
                ////Debug.Log("=================>>>OnTriggerStay actState = " + actState);
                setStunCheck();
                if (actState == FielderAction._CATCHING)
                {
                    catchingCatchCheck();
                }
                else if (actState == FielderAction._FIELDING)
                {
                    if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING_NORMAL)
                    {
                        field.setZoomTo(1.5f, 0.1f); //field.setZoomTo(field.curZoom * 1.2f, 0.3f);
                        grounderCatchType = GrounderCatch.GROUNDERCATCH_CATCH_AND_THROW;
                        moveStep = 0;
                        curTime = 0;
                        nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                        dX = (nFielderDir == FieldParm._EAST ? 1 : -1) * FIELDER_SPEED * 0.8f;                        
                    }
                    fieldingCatchCheck();
                }
                /*
                else if (actState == FielderAction._STANDBY)
                {
                    //홈런캐치 여기서
                    standbyCatchCheck();
                }*/                    
                else if (actState == FielderAction._SPECIAL_CATCH_THROW)
                {
                    //존나 특수
                    field.ball.setBallCatched(posIndex, posX, posY);
                    actState = FielderAction._NOTHING_STATE;
                }
            }
            else if (field.ball.step == BallStep.BALL_THROW)
            {
                if (field.nThrowIndex != posIndex)
                {
                    if (actState == FielderAction._THROWING_CATCH)
                    {
                        throwCatchCheck();
                    }
                    else if (actState == FielderAction._MOVE)
                    {
                        if (aStep == ActionStep._MOVING)
                        {
                            movingCatchCheck();
                        }
                    }
                    else if (actState == FielderAction._FIELDING)
                    {
                        if (aStep == ActionStep._CHASE)
                        {
                            field.ball.setThrowBallCatched(posIndex, false);
                            setStop();
                        }
                    }
                    else if (actState == FielderAction._SPECIAL_CATCH_THROW)
                    {
                        //존나 특수
                        field.ball.setThrowBallCatched(posIndex, false);
                    }
                }
            }
            else if (field.ball.step == BallStep.BALL_ERROR_STATE)
            {
                if (actState == FielderAction._FIELDING)
                {
                    ////UnityEngine.//Debug.Log("==========================================>> ERROR STATE CATCH posIndex = " + posIndex);
                    setCatchReady(false, true, true);
                    setCatch(false, false);
                }
            }
            else if (field.ball.step == BallStep.BALL_WILD_PITCH)
            {
                ////UnityEngine.//Debug.Log("=================>>>OnTriggerStay2D actState = " + actState);
                setStunCheck();
                if (actState == FielderAction._FIELDING)
                {
                    fieldingCatchCheck();
                }
            }
        }

        private void setStunCheck()
        {
            if(field.ball.bStunBall == true)
            {
                field.ball.bStunBall = false;
                bStun = true;
                bCatchErrorFlag = true;
                curTime = 0;
            }
        }


        //볼과의 충돌처리시 야수가 Catching 상태에서 공과의 충돌체크
        private void catchingCatchCheck()
        {            
            if (aStep == ActionStep._CATCHING_READY)
            {
                if (bGrounderAvail == true
                 || bFlyCatchAvail == true)
                {
                    if (field.ball.bBound == false)
                    {
                        if ((field.ball.nBallZ + field.ball.nBallDZ) < 100)
                        {
                            if (bCatchErrorFlag == true)
                            {
                                if ((field.ball.nBallZ) < 50)
                                {
                                    //플라이 경우
                                    StartCoroutine(setErrorEvent(false, true));
                                }
                                return;
                            }
                            else
                            {
                                if (posIndex == CPlayer._CATCHER)
                                {
                                    nFielderDir = FieldingMechanism.getFlyballDirException(nFielderDir);
                                    setNextMove(FieldingMechanism._CATCHER_FLYBALL, false, 1);
                                }
                                else
                                {
                                    setNextMove(FieldingMechanism._FLYBALL_CENTER, false, 1);
                                }
                                setCatch(true, true);
                            }
                        }
                    }
                    else
                    {
                        if (posIndex == CPlayer._CATCHER)
                        {
                            setNextMove(FieldingMechanism._CATCHER_GROUNDBALL, false, 1);
                        }
                        else
                        {
                            //정면 땅볼잡기
                            ////UnityEngine.//Debug.Log("====================>>ballZ = " + field.ball.nBallZ);
                            if (field.ball.nBallZ > FieldParm.JUMPCATCH_HEIGHT)
                            {
                                if (checkBoundJump() == true)   //DONE
                                {
                                    //점프해서 땅볼 잡기
                                    if (skillJumpCatchLevel > 0)//점핑캐치 체크
                                    {
                                        //if (field.bAiActiveControl() == true)
                                        {
                                            if (bCatchErrorFlag == true)
                                            {
                                                //바운드된볼을 점프캐치
                                                //에러 애니메이션 처리 필요없을듯
                                                StartCoroutine(setErrorEvent(false, false));
                                                return;
                                            }
                                            else
                                            {
                                                if (specialCatchSuccess == true)
                                                {
                                                    //제5의내야수 or 철벽수비 연출 (점프캐치)
                                                    fieldSkillDisplayManager.AddSkill(gameObject, pFielder, posIndex == CPlayer._PITCHER ? SkillIndex.PitcherJumpCatch : SkillIndex.SpecialCatch);

                                                    //내야수 스페셜 캐치 성공
                                                    setCatch(false, true);
                                                    catchDelayRate = 1.5f;
                                                }
                                                else
                                                {
                                                    setNoCatchCollider();
                                                }
                                            }
                                            nFielderDir = FieldingMechanism.getJumpCatchException(nFielderDir, posIndex);
                                            setNextMove(FieldingMechanism._FLYBALL_JUMPING, false, 1);
                                        }
                                    }
                                }
                            }
                            else if (field.ball.nBallZ > FieldParm.STANDINGCATCH_HEIGHT)
                            {
                                //추후수정 - 서서 땅볼 잡기
                                if (bCatchErrorFlag == true)
                                {
                                    //서서 땅볼
                                    //에러 애니메이션 처리함
                                    //에러 처리한곳
                                    playSpecialAnim("GROUNDBALL_MISS_CATCH_" + FieldParm._dir[nFielderDir], false, 1, false); 
                                    StartCoroutine(setErrorEvent(false, false));
                                    return;
                                }
                                else
                                {
                                    setNextMove(FieldingMechanism._GROUNDBALL_CENTER_HIGH, false, 1);
                                    setCatch(false, true);
                                }
                            }
                            else
                            {
                                //일반 땅볼
                                ////UnityEngine.//Debug.Log("====================>>ballZ = " + field.ball.nBallZ);
                                if (bCatchErrorFlag == true)
                                {
                                    //일반땅볼 (정면)
                                    //에러 애니메이션 처리함
                                    //에러 처리한곳
                                    //저글
                                    if (checkJuggle() == true)
                                    {
                                        StartCoroutine(setJuggleCatch(false,0));
                                    }
                                    else
                                    {
                                        //알까기,펌블
                                        playSpecialAnim("GROUNDBALL_MISS_CATCH_" + FieldParm._dir[nFielderDir], false,1,false);
                                        StartCoroutine(setErrorEvent(false, false));
                                    }
                                    return;
                                }
                                else
                                {
                                    setNextMove(getCenterCatchStr(), false, 1);
                                    setCatch(false, true);
                                }
                            }
                        }

                    }
                }
            }
        }


        private void grounderSideCatch()//bool bFumbleWithNoError = false)
        {
            if (bCatchErrorFlag == true)// || bFumbleWithNoError == true)
            {
                if (checkJuggle() == true)
                {
                    StartCoroutine(setJuggleCatch(true, 0.975f));
                    return;
                }
                else
                {
                    //if (bCatchErrorFlag == true) bFumbleWithNoError = false;
                    //에러 애니메이션 처리함 (grounderSideCatch함수안에 있음)
                    StartCoroutine(setErrorEvent(false, false, false));//, bFumbleWithNoError));
                }
            }
            else
            {
                setCatch(false, true);
            }
            grounderCatchType = GrounderCatch.GROUNDERCATCH_MOVING;
            setState(FielderAction._FIELDING, ActionStep._GROUNDER_SPECIAL);//actState = FielderAction._GROUNDER_SPECIAL;//
            nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
            if (bCatchErrorFlag == true)
            {
                //에러 처리한곳
                setNextMove(FieldingMechanism._GROUNDBALL_FORE_BACK_NOCATCH, false, 1.0f);
            }
            else
            {
                //트라이트라이
                if (bGrounderTry == false)
                {
                    setNextMove((field.ball.nBallZ > FieldParm.STANDINGCATCH_HEIGHT ? FieldingMechanism._GROUNDBALL_FORE_BACK_HIGH : FieldingMechanism._GROUNDBALL_FORE_BACK2), false, 0.6f, true, true); //setNextMove(FieldingMechanism._GROUNDBALL_CENTER2, false, 1);
                }
            }
            moveStep = 1;
            curTime = 0;
            nLastDir = -1;
        }

        //볼과의 충돌처리시 야수가 Fielding 상태에서 공과의 충돌체크
        private void fieldingCatchCheck()
        {
            ////Debug.Log("=======================>>aStep = " + aStep);
            if (aStep == ActionStep._FIELDING_MOVE)
            {
                if (bGrounderAvail == true
                 || bFlyCatchAvail == true)
                {
                    if (field.ball.bBound == false)
                    {
                        if (flyballCatchType == FlyCatch.FLYCATCH_NORMAL)
                        {
                            if ((field.ball.nBallZ + field.ball.nBallDZ) < 100)
                            {
                                if (bCatchErrorFlag == true)
                                {
                                    if ((field.ball.nBallZ) < 50)
                                    {
                                        //플라이 경우
                                        StartCoroutine(setErrorEvent(false, true));
                                    }
                                    return;
                                }
                                else
                                {
                                    setCatchReady(true, false, true);
                                    setCatch(true, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (grounderCatchType == GrounderCatch.GROUNDERCATCH_NORMAL)
                        {                            
                            if ((nFielderDir == FieldParm._EAST || nFielderDir == FieldParm._WEST)  //좌우움직임과
                              && field.ball.nBallX < FieldParm.STANDINGCATCH_HEIGHT)    //일반땅볼캐치의 높이인 경우
                            {
                                ////UnityEngine.//Debug.Log("=======================>>여기여기여기1111");
                                grounderSideCatch();
                                //능력치에 따른
                                if (posIndex >= CPlayer._SECONDBASEMAN && posIndex <= CPlayer._SHORTSTOP)
                                {
                                    setSpecialThrowType(nFielderDir == FieldParm._WEST ? ThrowState.BACK_HAND_CATCH : ThrowState.FORE_HAND_CATCH);
                                }


                            }
                            else
                            {
                                ////UnityEngine.//Debug.Log("=======================>>여기여기여기2222");
                                if (bCatchErrorFlag == true)
                                {
                                    StartCoroutine(setErrorEvent(false, false));
                                    return;
                                }
                                else
                                {
                                    setCatchReady(false, false, true);
                                    setCatch(false, true);
                                }
                            }

                        }
                    }
                }
                else if (bGrounderTry == true)
                {
                    if (field.ball.bBound == true)
                    {
                        //25%의 저글 확률
                        bool bJuggle = (MyMath.Percent() < 25 ? true : false); //트라이트라이
                        if (bJuggle == true)
                        {
                            StartCoroutine(setJuggleCatch(true, 0.98f));
                            return;
                        }
                        else
                        {
                            grounderSideCatch();
                        }
                        field.bOutofInfield = false;
                    }
                }
            }

            else if (aStep == ActionStep._CHASE)
            {
                if (field.ball.bBound == true)
                {
                    ////UnityEngine.//Debug.Log("=======================>> 여기??");
                    //수정 
                    //좌우(E,W NE,NW,SE,SW) 로 잡을때는 관성 딜레이)
                    if (bCatchErrorFlag == true)
                    {
                        //에러 처리함
                        //에러 처리한곳
                        nFielderDir = FieldingMechanism.getDashDirException(nFielderDir);
                        playSpecialAnim("GROUNDBALL_MISS_CATCH_" + FieldParm._dir[nFielderDir], false, 1, false);                        
                        //chase 형태                        
                        bool bDashState = (posIndex < CPlayer._LEFTFIELDER & bDashCatchTry);
                        StartCoroutine(setErrorEvent(false, false, bDashState));
                    }
                    else
                    {
                        if (posIndex < CPlayer._LEFTFIELDER)
                        {
                            if (skillSlidingCatchLevel > 0 || skillDashThrowLevel > 0)
                            {
                                setCatchReady(false, true, true);
                            }
                            else
                            {
                                setCatchReady(false, false, true);
                            }
                            setCatch(false, true);
                        }
                        else
                        {
                            if (field.ball.bFenceCol == true)
                            {
                                setStop();
                            }
                            setCatchReady(false, true, true);
                            setCatchMotionException();
                            setCatch(false, true);
                        }
                    }
                }
            }
            else if (aStep == ActionStep._GROUNDER_SPECIAL)
            {
                if (field.ball.bBound == true)
                {
                    ////////UnityEngine.//Debug.Log("==============>> ㅋㅋㅋㅋ posIndex = "+posIndex);
                    if (bDiveSuccess == true)
                    {
                        if (bCatchErrorFlag == true)
                        {
                            //에러 애니메이션 처리함
                            if (checkJuggle() == true &&
                                (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING || grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING_NORMAL || grounderCatchType == GrounderCatch.GROUNDERCATCH_TRY || grounderCatchType == GrounderCatch.GROUNDERCATCH_CATCH_AND_THROW))
                            {
                                //저글
                                if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING || grounderCatchType == GrounderCatch.GROUNDERCATCH_TRY)
                                {
                                    //포어, 백 캐치
                                    StartCoroutine(setJuggleCatch(true, 0.975f));
                                }
                                else
                                {
                                    //정면 무빙 캐치 등등
                                    nFielderDir = FieldParm._SOUTH;
                                    StartCoroutine(setJuggleCatch(true, 0.96f));
                                }
                                return;
                            }
                            else
                            {
                                if (grounderCatchType == GrounderCatch.GROUNDERCATCH_MOVING)
                                {
                                    //에러 처리한곳
                                    playSpecialAnim("GROUNDBALL_MISS_CATCH_S", false, 1, false);
                                }

                                //특수 캐치 형태에서는 펌블유도
                                //bool bDashState = false;                                                                                        
                                StartCoroutine(setErrorEvent(false, false, true));
                            }
                        }
                        else
                        {
                            setCatch(false, false);
                        }
                    }
                }
            }
            else if (aStep == ActionStep._FLYBALL_SPECIAL)
            {
                if (field.ball.bBound == false)
                {
                    if ((field.ball.nBallZ + field.ball.nBallDZ) < 100)
                    {
                        if (bCatchErrorFlag == true)
                        {
                            if ((field.ball.nBallZ) < 50)
                            {
                                //플라이 스페셜인경우
                                StartCoroutine(setErrorEvent(false, true));
                            }
                        }
                        else
                        {
                            if (flyballCatchType == FlyCatch.FLYCATCH_SLOWMOVE)
                            {
                                field.bDelayedCall = true;
                                field.delayedCallTime = 0.5f;
                                setNextMove(getRunIndexStr(), true, 0.72f);//Mode.bAutoPlay ? 0.4f : 0.72f);
                                setCatch(true, false);
                            }
                            else if (flyballCatchType == FlyCatch.FLYCATCH_FULLSPEED)
                            {
                                field.bDelayedCall = true;
                                field.delayedCallTime = 0.5f;
                                setCatch(true, false);
                            }
                            else if (flyballCatchType == FlyCatch.FLYCATCH_DIVING)
                            {
                                field.bDelayedCall = true;
                                field.delayedCallTime = 0.7f;
                                setCatch(true, false);
                            }
                            else if (flyballCatchType == FlyCatch.FLYCATCH_JUMPING)
                            {
                                field.bDelayedCall = true;
                                field.delayedCallTime = 1.0f;
                                setCatch(true, false);
                            }
                        }
                    }
                }
            }
        }

        //볼과의 충돌처리시 야수가 Moving 상태에서 공과의 충돌체크
        private void movingCatchCheck()
        {
            if (bTossTaking == true)
            {
                if (field.ball.nBallZ < 25)
                {
                    ////////UnityEngine.//Debug.Log("=================>>nBallZ = " + field.ball.nBallZ);
                    ////////UnityEngine.//Debug.Log("=================>>bBound = " + field.ball.bBound);

                    field.doublePlayType = field.checkDoublePlayType(posIndex);
                    field.nCatchIndex = posIndex;
                    field.ball.setThrowBallCatched(posIndex, false);

                    bTossTaking = false;
                    bTossTaked = true;

                    setNewSpeedByFrame((fieldingTime - curTime), dstX, dstY);

                }
                else
                {
                    if (field.ball.bBound == true)
                    {
                        //////UnityEngine.//Debug.Log("=================>>WILD THROW - Ball Over");
                    }
                }
            }
        }

        //볼과의 충돌처리시 야수가 ThrowingCatch 상태에서 공과의 충돌체크
        private void throwCatchCheck()
        {
            if (field.ball.nBallZ < 300)
            {
                if (field.bThrowErrorFlag == true)
                {                    
                    bThrowErrorFlag = false;
                    field.bThrowErrorFlag = false;
                    //송구에러시

                    if (field.errorType == FieldParm.ErrorType.WildWrongPlace)
                    {
                        field.errorType = FieldParm.ErrorType.None;
                        field.bBallTail = true;
                        field.nCatchIndex = posIndex;
                        field.ball.setThrowBallCatched(posIndex, false);                                                
                        StartCoroutine(setWrongPlaceThrow());
                        nFielderDir = throwCatchDir;
                    }
                    else
                    {
                        if (field.nThrowIndex < CPlayer._LEFTFIELDER && field.bFirstThrow == true)
                        {
                            //내야에서 송구에러 발생
                            field.bInfieldThrowErrorFlag = true;
                        }

                        if (field.errorType == FieldParm.ErrorType.WildThrowUp)
                        {
                            nFielderDir = FieldingMechanism.getJumpCatchException(nFielderDir, posIndex);
                            setNextMove(FieldingMechanism._FLYBALL_JUMPING, false, 1);
                        }
                        //송구 에러임
                        StartCoroutine(setErrorEvent(true, false));
                    }
                }
                else
                {
                    if (field.ball.bLaserThrowFlag == true)
                    {
                        //CameraManager.FieldShockWave(field.ball.transform.position, 0.7f, 1, 0.1f);
                        //충돌연출
                        field.ball.bLaserThrowFlag = false;
                    }

                    field.bBallTail = true;
                    field.nCatchIndex = posIndex;
                    int curBase = field.nTargetIndex;
                    bool bTagNeeded = false;
                    if (curBase == FieldParm.RELAY_INDEX)
                    {
                        bTagNeeded = false;
                    }
                    else
                    {
                        if (field.run.bForceOutFlag[curBase] == true)
                        {
                            ////UnityEngine.//Debug.Log("===============>>1");
                            bTagNeeded = false;
                        }
                        else
                        {
                            //현재 베이스로 달려오는 중
                            if (field.run.bOnRunning[curBase] == true
                              || field.run.bOnBackRunning[curBase] == true)
                            {
                                bTagNeeded = true;
                            }
                            else
                            {
                                //아니면
                                ////UnityEngine.//Debug.Log("===============>>2");
                                bTagNeeded = false;
                            }
                        }
                    }

                    ////UnityEngine.//Debug.Log("===============>>bTagNeeded = " + bTagNeeded);

                    if (bTagNeeded == false)
                    {
                        //UnityEngine.Debug.Log("##########################포스아웃 체크 필더");
                        
                        //미트 사운드

                        //봉살인 경우
                        //field.run.checkForceOutRunner(curBase);

                        if (posIndex == CPlayer._CATCHER)
                        {
                            setNextMove(FieldingMechanism._CATCHER_BALL_CATCH, false, 1);
                        }
                        else
                        {
                            if (aStep == ActionStep._FORCE_OUT)
                            {
                                if (posIndex == CPlayer._FIRSTBASEMAN && nFielderDir == FieldParm._WEST)
                                {
                                    playSpecialAnim("TEST_5_BASECATCH", false);
                                    bSpecialAnim = false;
                                }
                                else
                                {
                                    setNextMove(FieldingMechanism._BALLCATCH_FORCEOUT, false, 1);
                                }
                            }
                            else
                            {
                                //UnityEngine.Debug.Log("##########################여기 들어옴?");
                                setNextMove(FieldingMechanism._BALLCATCH_NORMAL, false, 1);
                            }
                        }

                        field.doublePlayType = field.checkDoublePlayType(posIndex);
                        field.checkSafeCall(curBase);
                        field.ball.setThrowBallCatched(posIndex, true);
                        setState(FielderAction._MOTION, ActionStep._MOTION_SET);//actState = FielderAction._THROW_READY;

                        field.run.setForceOutRunner(curBase);

                        nFielderDir = throwCatchDir;

                    }
                    else
                    {
                        //UnityEngine.Debug.Log("##########################포스아웃 체크 필더 STEP 1");
                        //태그인 경우
                        field.ball.setThrowBallCatchedTagReady(posIndex);
                        setState(FielderAction._THROWING_CATCH, ActionStep._TAGGING);// actState = FielderAction._TAGGING;
                        setTagAnim();
                        moveStep = 0;
                        nFielderDir = throwCatchDir;
                    }
                }
                curTime = 0;

            }
            else
            {
                //////UnityEngine.//Debug.Log("=================>>WILD THROW - Ball Over");
            }
        }

        //필더간의 충돌처리
        private void collideWithFielder(Collider col)
        {
            ////Debug.Log("========================================================================>> 필더간의 충돌 체크 :: 에러체크시");
            if (field.bCollisionFlag == true)
            {
                if (actState != FielderAction._COLLISION && actState == FielderAction._FIELDING)
                {
                    if (field.flyCatchFielder == posIndex && field.ball.bBound == false) //
                    {
                        //if (Mathf.Abs(transform.position.x - col.transform.position.x) < 100 && Mathf.Abs(transform.position.y - col.transform.position.y) < 100)
                        {
                            ////UnityEngine.//Debug.Log("========================================================================>> 필더간의 충돌 체크 :: 에러체크시");
                            if (field.bFielderCrushEffect == false)
                            {
                                //if (MyMath.Percent() < FieldingMechanism.TEAM_DEFENSE) //이게 진짜                                
                                {
                                    //충돌피함 --> 영리하게 ㅋㅋㅋ
                                    if (bFlyCatchAvail == true)
                                    {
                                        Fielder otherFieler = col.gameObject.GetComponent<Fielder>();
                                        if (posIndex < CPlayer._LEFTFIELDER) otherFieler.setStop();
                                        else otherFieler.setSecondMove();
                                    }
                                    else
                                    {
                                        if (posIndex < CPlayer._LEFTFIELDER) setStop();
                                        else setSecondMove();
                                    }
                                }
                                /*else
                                {
                                    //충돌 못피함 --> 등신 ㅋㅋ                            
                                    setCollisionFielders(nFielderDir);
                                    int dir = (nFielderDir + 4) % 8;
                                    Fielder colFielder = col.gameObject.GetComponent<Fielder>();
                                    colFielder.setCollisionFielders(dir);
                                    float cx = transform.position.x + (col.transform.position.x - transform.position.x) / 2;
                                    float cy = transform.position.y + (col.transform.position.y - transform.position.y) / 2;
                                    field.setFieldCollisionEffect(cx, cy);
                                    //야수간 충돌 연출
                                }*/
                                field.bFielderCrushEffect = true;
                            }

                        }
                    }
                }
            }

            /*
            if (field.ball.nBallStep == FBall.BALL_HIT)
            {
                ////////UnityEngine.//Debug.Log("==========================>> Set Col");
                ////////UnityEngine.//Debug.Log("==========================>> actState = " + actState);
                ////////UnityEngine.//Debug.Log("==========================>> colFielder.actState = " + colFielder.actState);
                

                
                if (actState == FielderAction._FIELDING)
                {
                    
                    if ((aStep == ActionStep._FIELDING_MOVE && bFlyCatchAvail == true)
                       || (aStep == ActionStep._FLYBALL_SPECIAL)
                       || (aStep == ActionStep._CHASE && field.ball.bBound == false))
                    {
                        //나중에
                        //setStop();
                        //actState = FielderAction._CRASH_PLAYERS;
                    }
                }
            }*/
        }

        private void collideWithInfieldFence(Collider col)
        {            
            if (actState == FielderAction._FIELDING)
            {
                if (field.flyCatchFielder == posIndex && field.ball.bBound == false) //
                {
                    //Debug.Log("========================================================================>> 수비시 내야 펜스랑 충돌");
                    setStop();
                    actState = FielderAction._NOTHING_STATE; //아무것도 아닌 상태로 세팅
                }
            }
        }


        //필더간, 혹은 펜스 충돌관련
        private void OnTriggerEnter(Collider col)
        {
            if (bObjectInit == true)
            {
                //수비 능력치가 낮으면 발생 능력치 좋으면 싹피함.. 
                //if (col.gameObject.tag == "FIELDER_TAG")
                if (col.gameObject.CompareTag("FIELDER_TAG") == true)
                {
                    collideWithFielder(col);
                }
                else if (col.gameObject.CompareTag("INFIELD_FENCE_TAG") == true)
                {
                    if (field.manager.playState == PlayState.PLAY_CHANGE_INNING)
                    {
                        ////Debug.Log("=====>>벤치 들어감");
                        fielderName.gameObject.SetActive(false);
                        shadow.gameObject.SetActive(false);
                        anim.gameObject.SetActive(false);
                    }
                    else if (field.manager.playState == PlayState.PLAY_FIELDING_VIEW)
                    {
                        collideWithInfieldFence(col);
                    }
                }
                /*else if (col.gameObject.tag == "BASE_TAG")
                {
                }*/
            }
        }

        //OnTriggerStay
        private void OnTriggerStay(Collider col)
        {
            if (bObjectInit == true)
            {
                //if (col.gameObject.tag == "BALL_COLLIDER_TAG")
                if (col.gameObject.CompareTag("BALL_COLLIDER_TAG") == true)
                {   
                    collideWithBall(col);
                }
                
            }
        }
        
        
        //////////////////////////////////////////////////////////////////
        //필드 스킬 활성화 및 관련 애니메이션과 이펙트 효과
        //////////////////////////////////////////////////////////////////
        /// <summary>
        /// 스킬 발동 여부를 체크
        /// </summary>
        /// <returns>true를 리턴한경우 스킬이 발동함</returns>
        public bool checkSkillOn(SkillIndex index)
        {
            return pFielder.fieldSkillSuccess(index);
        }

        /// <summary>
        /// 포수 앉아쏴 스킬 온        
        /// </summary>
        private void setSitThrowOn()
        {
            errorForceInit();            
            if (field.stealSuccess == false)
            {
                if (field.bVsShow == false)
                {
                    //수비형포수 - 앉아쏴 연출
                    fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.CatcherSitThrow);
                }
            }
        }

        /// <summary>
        /// 스페셜 캐치 발동시 땅볼을 슬라이딩 캐치하는 애니메이션과 효과 및 충돌체 재설정
        /// </summary>
        private void setSlidingSkillOn()
        {
            //철벽수비 연출
            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.SpecialCatch);

            if (checkSlidingQuickThrow() == true)
            {
                StartCoroutine(setSlidingQuickThrow());
            }
            else
            {
                setState(FielderAction._FIELDING, ActionStep._GROUNDER_SPECIAL);

                if (field.ball.nBallZ > FieldParm.JUMPCATCH_HEIGHT)
                {
                    /*if (checkBoundJump() == true) //공중 다이빙
                    {
                        field.ball.bHighDivingCatched = true;
                        setCollider(GrounderCatch.GROUNDERCATCH_JUMP); //컬라이더 재정의 
                    }
                    else*/
                    {
                        nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                        setNextMove(FieldingMechanism._GROUNDBALL_SLIDING, false, 1, false);
                    }
                }
                else
                {
                    nFielderDir = FieldingMechanism.getGrounderDir(nFielderDir);
                    setNextMove(FieldingMechanism._GROUNDBALL_SLIDING, false, 1, false);
                }
                moveStep = 0;
                curTime = 0;
                nLastDir = -1;
            }
        }

        /// <summary>
        /// 스페셜 송구의 대쉬 송구 발동시 관련 애니메이션과 효과
        /// </summary>
        private void setDashThrowSkillOn()
        {
            bDashQuickThrow = true;

            for (int i = 0; i < CPlayer._LEFTFIELDER; i++)
            {
                //다른 야수들 스킬 발동 무효처리
                if (i != posIndex) field.fielder[i].skillDashThrowLevel = 0;
            }

            SkillIndex curSkill = (posIndex == CPlayer._PITCHER ? SkillIndex.PitcherBuntFielding : SkillIndex.SpecialThrow);

            if (field.manager.batterSkillFlag == SkillFlag.GodOfBunt)
            {
                //번트신공 vs 제5의 내야수 혹은 특급송구
                int offenseRank = field.batter.pBatter.getSkillRank(SkillIndex.GodOfBunt);
                int defenseRank = pFielder.getSkillRank(curSkill);
                bool bOffenseWin = SimulParm.checkOffenseSkillWin(offenseRank, defenseRank);
                //Debug.Log("====================>>> 대결연출 임시 : 번트신공 vs 제5의 내야수 혹은 특급송구");
                //번트신공이 배팅뷰 스킬이므로 예외처리로 할것...

                if (bOffenseWin == true)
                {
                    //Debug.Log("============>>번트신공 승리");
                    field.bFieldVsSkillOffenseWin = true;
                    field.bBuntSuccess = true;
                }
                else
                {
                    //Debug.Log("============>>특급송구 승리");
                    field.bFieldVsSkillOffenseWin = false;
                    field.bBuntSuccess = false;
                    fieldSkillDisplayManager.AddSkill(gameObject, pFielder, curSkill);
                }
            }
            else
            {
                if (field.bVsShow == false)
                {
                    //대쉬송구
                    field.bFieldVsSkillOffenseWin = false;
                    //제5의 내야수 또는 특급송구 연출 (대쉬상황)
                    fieldSkillDisplayManager.AddSkill(gameObject, pFielder, curSkill);
                }
            }
            
        }

        /// <summary>
        /// 레이저 송구를 발동 가능한지를 체크
        /// </summary>
        private bool checkLaserThrowPossible()
        {
            //레이저
            if (posIndex >= CPlayer._LEFTFIELDER)
            {
                if(pFielder.fieldSkillSuccess(SkillIndex.Laser) == true && MyMath.Percent() <30)
                {
                    errorForceInit();
                    if (field.manager.nOutCount == 3 || field.manager.bThreeOutChange == true)
                    {
                        //해당 사항 없음
                        return false;
                    }
                    else
                    {
                        int throwBase = field.run.getFirstRunnerDest(-1, 1000);
                        if (throwBase == FieldParm.THIRDBASE_INDEX || throwBase == FieldParm.HOMEBASE_INDEX)
                        {
                            //레이저 발동
                            Runner destRunner = field.run.getDestRunner(throwBase);
                            if (destRunner != null)
                            {
                                int shobutype = destRunner.checkShobuPossible(false); //0상황없음 1:베이스 턴 후 2: 베이스 턴전
                                if (shobutype == 1 || (throwBase == FieldParm.THIRDBASE_INDEX && shobutype == 2))
                                {
                                    //레이저레이저 - 추가됨
                                    StartCoroutine(laserThrowDelay(destRunner, throwBase, shobutype));
                                    return true; //
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////
        //기타
        //////////////////////////////////////////////////////////////////
        //먼지 만들기
        float dustTime;
        private void makeDust(float delay, string type = "")
        {
        /*    dustTime += deltaTime;// field.getDeltaTime();
            if (dustTime > delay)
            {
                Vector3 pos = transform.position;
                GameObject dustEffect = Instantiate(Resources.Load("MainGame/prefabs/effectPrefab/field/fx_rundust" + type), pos, Quaternion.identity) as GameObject;
                Destroy(dustEffect, 1.0f);
                dustTime = 0;
            }*/
        }

        //야수 기록을 체크한다
        public void addRecord(int type, int num = 1)
        {
            if (pFielder != null)
            {
                //Debug.Log((posIndex + 1) + "번 야수 기록===============>>> " + pFielder.getName() + "의 " + Param.debug_stat[type] + " 가산");
                pFielder.setRecord(type, num);
            }
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

        //x좌표의 옵셋값을 체크한다
        private void checkOffsetX()
        {
            if (curOffsetX != offsetX)
            {
                if (bOffsetLinear == true)
                {
                    if (offsetDV > 0)
                    {
                        curOffsetX += offsetDV * deltaTime;
                        if (curOffsetX > offsetX)
                        {
                            curOffsetX = offsetX;
                        }
                    }
                    else
                    {
                        curOffsetX += offsetDV * deltaTime;
                        if (curOffsetX < offsetX)
                        {
                            curOffsetX = offsetX;
                        }
                    }
                }
                else
                {
                    curOffsetX = offsetX;
                }
            }
        }


        //플라이 볼 수비시 x좌표 옵셋값을 세팅
        private void setFlyMoveOffsetX(int dir)
        {
            if (dir == FieldParm._SOUTH || dir == FieldParm._NORTH)
            {
                setOffset(0);
            }
            else if (dir == FieldParm._NORTHEAST || dir == FieldParm._EAST || dir == FieldParm._SOUTHEAST)
            {
                setOffset(-25); //offsetX = -25;
            }
            else
            {
                setOffset(25);
            }
            //setOffetDV();
        }

        //다이빙 캐치 수비시 x좌표 옵셋값을 세팅
        private void setFlyDivingOffsetX(int dir)
        {
            if (dir == FieldParm._SOUTH || dir == FieldParm._NORTH)
            {
                setOffset(0);
            }
            else if (dir == FieldParm._NORTHEAST || dir == FieldParm._EAST || dir == FieldParm._SOUTHEAST)
            {
                setOffset(-50); //offsetX = -25;
            }
            else
            {
                setOffset(50);
            }
            //setOffetDV();
        }

        //옵셋을 세팅
        private void setOffset(float offset, bool bLinear = true)
        {
            offsetX = offset;
            dstOffsetX = offsetX;
            bOffsetLinear = bLinear;
            setOffetDV();
        }

        //옵셋 dv를 세팅
        public void setOffetDV()
        {
            /*    if (offsetX > curOffsetX)
                {
                    offsetDV = 25;
                }
                else if (offsetX < curOffsetX)
                {
                    offsetDV = -25;
                }
                else
                {
                    offsetDV = 0;
                }*/
            offsetDV = (offsetX - curOffsetX);
        }

        /// <summary>
        /// 에러마크
        /// </summary>
        public IEnumerator setErrorMark(float delay)
        {
            errorMark.spriteId = errorMark.GetSpriteIdByName("hiticon");
            errorMark.gameObject.SetActive(true);
            errorMark.transform.localScale = Vector3.one;
            UITweener tween = TweenScale.Begin(errorMark.gameObject, 0.25f, new Vector3(2, 2, 1));
            tween.style = UITweener.Style.PingPong;

            yield return new WaitForSeconds(0.5f);

            errorMark.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 상태를 NOT State로 변환후 코루틴에서 야수의 펜스플레이 처리
        /// </summary>
        /// <returns></returns>
        private IEnumerator setFencePlayFieder()
        {
            ////Debug.Log("=============>>펜스플레이");
            setNoCatchCollider();
            setStop();
            nFielderDir = FieldingMechanism.getFenceColException(nFielderDir);
            playSpecialAnim("FENCE_PLAY_" + FieldParm._dir[nFielderDir], true, 1);
            actState = FielderAction._NOTHING_STATE;
            yield return new WaitForSeconds(1.0f);
            while (field.ball.bBound == false)
            {
                yield return new WaitForSeconds(0.25f);
            }
            setCollider();
            bSpecialAnim = false;
            if (field.ball.bBallCatched == true && field.nThrowIndex == posIndex)
            {
                setState(FielderAction._MOTION, ActionStep._MOTION_SET);
                curTime = 0;
            }
            else
            {
                setBallChase();
            }
        }

        /// <summary>
        /// 상태를 NOT STATE를 변환후 코루틴에서 야수간 충돌처리
        /// </summary>
        /// <returns></returns>
        private IEnumerator setFenceColAfterCatch()
        {
            float cx = transform.position.x + (dX * 0.25f);
            float cy = transform.position.y + (dY * 0.25f);
            field.setFieldCollisionEffect(cx, cy);

            setStop();
            playSpecialAnim("0300_CRASH_" + FieldParm._dir[nFielderDir], false, 1);
            actState = FielderAction._NOTHING_STATE;
            yield return new WaitForSeconds(1.0f);
            bSpecialAnim = false;
            setState(FielderAction._MOTION, ActionStep._MOTION_SET);
            curTime = 0;
            //CameraManager.CameraPositionInit();
        }


        /// <summary>
        /// 상태를 NOT STATE를 변환후 코루틴에서 야수의 저글 처리
        /// </summary>
        /// <param name="bMove"></param> 움직이면서 저글하는지 여부
        /// <param name="accel"></param> 저글시 가(감)속도
        /// <returns></returns>
        private IEnumerator setJuggleCatch(bool bMove, float accel)
        {
            //에러타입은 저글
            field.errorType = FieldParm.ErrorType.Juggle;
            for (int i = CPlayer._LEFTFIELDER; i <= CPlayer._RIGHTFIELDER; i++)
            {
                field.fielder[i].setStop();
            }
            setNoCatchCollider();
            field.setCatchErrorInit();
            float delay = Random.Range(0.7f, 3.0f);            
            StartCoroutine(setErrorMark(0.5f));
            if (bMove == false)
            {
                setStop();
            }
            nFielderDir = FieldingMechanism.getDashDirException(nFielderDir);
            playSpecialAnim("JUGGLE_STYLE1_" + FieldParm._dir[nFielderDir], false, 1,false);
            actState = FielderAction._NOTHING_STATE;

            //저글시 한베이스만...
            field.run.bOnlyOneBaseFlag = true;
            float ballGabX = 0;
            if (delay > 2.3f)
            {
                //완전히 놓치는 저글
                field.setZoomTo(1.5f, 2.0f);
                field.ball.gameObject.SetActive(false);                
                if (bMove == false)
                {
                    yield return new WaitForSeconds(3.2f);
                }
                else
                {
                    for (int i = 0; i < 60; i++)
                    {
                        posX += (dX * deltaTime);
                        ballGabX += (dX * deltaTime);
                        dX = dX * accel;// 0.96f;
                        yield return new WaitForSeconds(deltaTime);
                    }
                    float lastDelay = 3.2f - 1.0f;
                    yield return new WaitForSeconds(lastDelay);
                }
                field.ball.gameObject.SetActive(true);
                field.ball.nBallX += ballGabX;
                setCollider();
                bSpecialAnim = false;
                posX += (nFielderDir == FieldParm._SOUTHWEST ? -30 : 30);
                posY -= 30;
                bBlending = true;
                setBallChase();        
            }
            else
            {
                //중간에 잡는 저글
                field.setZoomTo(1.5f, delay);
                field.ball.setBallCatched(posIndex, posX, posY);
                if (bMove == false)
                {
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    for (int i = 0; i < 60; i++)
                    {
                        posX += (dX * deltaTime);
                        ballGabX += (dX * deltaTime);
                        dX = dX * accel;// 0.96f;
                        yield return new WaitForSeconds(deltaTime);
                    }
                    float lastDelay = delay - 1.0f;
                    if (lastDelay > 0)
                    {
                        yield return new WaitForSeconds(lastDelay);
                    }
                }
                field.ball.nBallX += ballGabX;
                setCollider();
                bSpecialAnim = false;
                bBlending = true;
                bThrowBlending = true;
                setState(FielderAction._MOTION, ActionStep._MOTION_SET);

                if (skillDashThrowLevel > 0 && delay < 1.5f)
                {
                    //저글후 스페셜 송구 -> 따로 연출 안함
                    throwType = ThrowType._INFIELD_SIDE_DASH;
                }
                else
                {
                    throwType = ThrowType._NORMAL;
                }
                
            }
        }


        /// <summary>
        /// 저글확률 체크
        /// </summary>
        /// <returns></returns>
        private bool checkJuggle()
        {
            if (field.manager.batterSkillFlag == SkillFlag.AssaultBall)
            {
                //강습타구 예외처리
                return false;
            }
            else
            {
                if (MyMath.Percent() < FieldingMechanism.JUGGLE_PER && posIndex < CPlayer._LEFTFIELDER)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        int slidingQuickTarget; //퀵슬라이딩 캐치시 던지는 방향
        /// <summary>
        /// 퀵스라이딩 쓰로우 발생 확률 체크
        /// 스페셜쓰로우, 스페셜캐치 모두 발동시 발동가능
        /// </summary>
        /// <returns></returns>
        private bool checkSlidingQuickThrow()
        {
            if (skillSlidingCatchLevel > 0 && skillQuickThrowLevel > 0)
            {
                if (posIndex == CPlayer._THIRDBASEMAN   //3루수
                 || posIndex == CPlayer._SHORTSTOP     //유격수
                 || (posIndex == CPlayer._SECONDBASEMAN && nFielderDir == FieldParm._WEST)) //2루수 서쪽방향만
                {
                    if (field.run.checkRunnerOnBase() == false)
                    {
                        //주자 올스톱에 타자주자 1루로 가는 경우
                        slidingQuickTarget = FieldParm.FIRSTBASE_INDEX;
                        return true;
                    }
                    else
                    {
                        if (posIndex != CPlayer._SECONDBASEMAN)
                        {
                            if (field.run.bOnRunning[FieldParm.HOMEBASE_INDEX] == false)
                            {
                                //홈쇄도 안하는 경우 2루에 주자가 오는 경우
                                if (field.run.bOnRunning[FieldParm.SECONDBASE_INDEX] == true)
                                {
                                    slidingQuickTarget = FieldParm.SECONDBASE_INDEX;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 특수 쓰로잉에 쓰이는 던지기전 조건 선세팅
        /// </summary>
        private void setThrowPreCondition(float _throwRate = 1.0f)
        {
            fieldSkillDisplayManager.EffectDisplay(fieldSkillDisplayManager.FieldDisplayStep.Throwing);
            float _throwPower = THROW_SPEED * _throwRate;
            //각종 플래그 초기화
            field.bCollisionFlag = false;   //충돌 무효
            field.bCrushDelay = false;
            bThrowableChecked = false;
            bThrowAvailable = false;
            bDashCatched = bDashCatchTry = false;
            bThrowAddDelay = false;
            field.bFieldPerspectiveZoom = false;
            field.bThrowBallCatched = false;
            field.bThrowZoom = false;
            field.bTossThrow = false;

            /*
            //터보 체크 -> 임시봉인
            if (posIndex >= CPlayer._FIRSTBASEMAN && posIndex <= CPlayer._SHORTSTOP)
            {
                if (field.runnerTurbo == FieldSkillUse.Active)
                {
                    checkRunnerTurbo(_throwPower, 1.0f, 0.01f);
                }                
            }*/

            tStartX = posX;             //x 포지션저장
            tStartY = posY;             //y 포지션 저장
            tStartZ = FieldParm.BALL_INIT_HEIGHT;   //z 포지션 저장            
            field.curThrowIndex = posIndex;
            bThrowErrorFlag = false;

            //각종 플래그 초기화
            field.bThrowZoom = true;
            field.bThrowing = true;
            field.nFirstThrower = posIndex;
            //목표 베이스의 좌표를 구한다
            dstBaseX = field.getOriginX(FieldSize.getBasePosX(field.nTargetIndex)) + FieldingMechanism.baseOffset[field.nTargetIndex, 0];
            dstBaseY = field.getOriginY(FieldSize.getBasePosY(field.nTargetIndex)) + FieldingMechanism.baseOffset[field.nTargetIndex, 1];
            field.ball.setThrowingCamera(dstBaseX, dstBaseY);

            //물리값을 넣어서 던진는 공의 vector값을 구한다
            field.setThrowingVector(tStartX, tStartY, tStartZ, dstBaseX, dstBaseY, _throwPower, THROW_WRIST, nFielderDir, bThrowErrorFlag);

        }

        /// <summary>
        /// 특수 쓰로잉에 쓰이는 던지기전 조건 공을 잡은 후 세팅
        /// </summary>
        private void setThrowConditionAfterCatch()
        {
            field.ball.nBallX = posX;
            field.ball.nBallY = posY;
            field.ball.transform.localPosition = transform.localPosition;

            throwTypeFirstBase = ThrowType._NORMAL;
            field.ball.setActive(true);
            field.ball.setBallThrow();// nBallStep = FBall.BALL_THROW;
            field.ball.bBallHidden = true;
            field.ball.setDraw(false);
            StartCoroutine(throwingStop());
            bThrow = true;
            field.throwFrame = getThrowFrame();
            //field.setZoomCameraSetting(false);
            //송구시 줌 아웃
            field.setThrowingZoom(posIndex);
            field.setTimeScale(Field.INIT_TIME_SCALE);
            field.ball.setParticleDraw(field.bBallTail);
            field.ball.setThrowingCamera();
            IngameUI.GetFieldUI().SetName(false, null, 0, field.manager.bMyTurn);

            //연출관련 삭제
            //if (field.bFirstThrow == false) field.checkRunnerSkillAfterFirstThrow();

            field.bFirstThrow = true;

            field.ball.checkBallDeadThrow(field.nTargetIndex);

        }


        private void setFieldingCatchCondition(bool bFlyCatch)
        {
            if (bFlyCatch == true)
            {
                field.ball.nBallDZ = 0;
                field.ball.nBallZ = FieldParm.BALL_INIT_HEIGHT;
                field.setFlyOut();
                field.bBuntFielding = false;    // 번트필딩 무효화
            }
            field.nCatchIndex = posIndex;
            field.ball.setBallCatched(posIndex, posX, posY);
        }

        /// <summary>
        /// 슬라이딩 후 퀵스로우
        /// </summary>
        /// <returns></returns>
        private IEnumerator setSlidingQuickThrow()
        {            
            //철벽수비 - 특급송구 연속 연출 (슬라이딩후 바로 송구)
            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.SpecialCatch);
            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.SpecialThrow);


            actState = FielderAction._SPECIAL_CATCH_THROW;
            field.nTargetIndex = slidingQuickTarget;

            float curThrowRate = 1.0f;
            
            string animStr = "SLIDING_CATCH_AND_QUICK" + (slidingQuickTarget + 1) + "_";
            string throwDir = "";

            if (posIndex == CPlayer._THIRDBASEMAN)
            {
                throwDir = (slidingQuickTarget == FieldParm.FIRSTBASE_INDEX ? "_E" : "_NE");                
            }
            else if (posIndex == CPlayer._SHORTSTOP)
            {
                throwDir = (slidingQuickTarget == FieldParm.FIRSTBASE_INDEX ? "_SE" : "_E");                
            }
            else if (posIndex == CPlayer._SECONDBASEMAN)
            {
                throwDir = "_SE";
            }
            else
            {
                //예외처리
                setStop();
                yield break;
            }

            playSpecialAnim(animStr + FieldParm._dir[nFielderDir] + throwDir, false, 1, false);
            if (nFielderDir == FieldParm._WEST) curThrowRate = 0.9f;

            while (field.ball.bBallCatched == false)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);                
                dX = dX * 0.965f;
                dY = dY * 0.965f;
                yield return new WaitForSeconds(deltaTime);
            }

            /*
            //Debug.Log("==================== bVsShow = " + field.bVsShow);            
            if (field.nTargetIndex == FieldParm.FIRSTBASE_INDEX)
            {
                if (field.bVsShow == true && field.runnerTurbo == FieldSkillUse.Active)
                {
                    Runner destRunner = field.run.getDestRunner(field.nTargetIndex);
                    bool bOffenseWin = 
                }
            }*/

            field.ball.setFielderFocus(posIndex);
            setThrowPreCondition(curThrowRate);


            while (bThrowAnimFlag == false)// _time < delay)
            {
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);                
                dX = dX * 0.965f;
                dY = dY * 0.965f;
                yield return new WaitForSeconds(deltaTime);
            }
            dX = dY = 0;
            
            //field.setSkillEffect(true, pFielder, SkillIndex.SpecialThrow);
            setThrowConditionAfterCatch();
            field.ball.bCameraBallMove = true;
            bThrowAnimFlag = false;
            bSpecialAnim = false;
        }


        /// <summary>
        /// 글러브 토스 컴비네이션
        /// </summary>
        /// <returns></returns>
        private IEnumerator globeTossCombination()
        {
            //특급송구 연출 (글러브토스)
            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.SpecialThrow);
            fieldSkillDisplayManager.EffectDisplay(fieldSkillDisplayManager.FieldDisplayStep.Throwing);
            
            noDelayAnim = true;
            actState = FielderAction._NOTHING_STATE;            
            playSpecialAnim("5031_GLOVE_TOSS_INFIELD_W", false);
            while (bThrowAnimFlag == false)
            {
                yield return new WaitForEndOfFrame();
            }
            dX = dY = 0;
            //field.setSkillEffect(true, pFielder, SkillIndex.SpecialThrow);
            setThrowConditionAfterCatch();
            field.ball.bCameraBallMove = true;
            bThrowAnimFlag = false;

            if (field.manager.nOutCount < 2)
            {
                Fielder shortStop = field.fielder[CPlayer._SHORTSTOP];
                if (shortStop.skillQuickThrowLevel > 0)
                {
                    StartCoroutine(shortStop.bareHandCatchCombination());
                }
            }

            yield return new WaitForSeconds(3.0f);
            bSpecialAnim = false;
            setStop();
            noDelayAnim = false;
        }

        /// <summary>
        /// 글러브 토스랑 연결되는 맨손 캐치 컴비네이션
        /// 20%확률로 스핀 쓰로우
        /// 이게 발동시 100% 확률로 병살저지 방어
        /// </summary>
        /// <returns></returns>
        public IEnumerator bareHandCatchCombination()
        {
            //특급송구 (배어핸드 캐치) 연출
            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.SpecialThrow);

            noDelayAnim = true;
            bSpecialAnim = true;
            float baseX = field.getOriginX(FieldSize.getBasePosX(field.nTargetIndex)); 
            float baseY = field.getOriginY(FieldSize.getBasePosY(field.nTargetIndex));
            float _time = field.ball.throwingTime*0.5f;
            dX = Mathf.Clamp((baseX - posX) / _time, (dX * 0.7f), dX);
            dY = Mathf.Clamp((baseY - posY) / _time, (dY * 0.7f), dY);

            while (field.bThrowing)
            {
                yield return new WaitForSeconds(deltaTime);
            }

            actState = FielderAction._NOTHING_STATE;
            field.setZoomTo(2.0f, 0.5f);

            int per = MyMath.Percent();
            if (per < 80)
            {
                playSpecialAnim("5050_SS_BAREHANDCATCH_JUMPTHROW_SE", false);
                dX = 150;
            }
            else
            {
                playSpecialAnim("5020_SIDETHROW_INFIELD_SPIN_SE", false, 1.5f);
                dX = 150;
            }
            

            field.nTargetIndex = FieldParm.FIRSTBASE_INDEX;
            setThrowPreCondition(1.0f);            
            while (bThrowAnimFlag == false)
            {
                posX += (dX * deltaTime);
                yield return new WaitForSeconds(deltaTime);
            }
            dX = dY = 0;
            //field.setSkillEffect(true, pFielder, SkillIndex.SpecialThrow);
            setThrowConditionAfterCatch();
            field.ball.bCameraBallMove = true;
            bThrowAnimFlag = false;

            yield return new WaitForSeconds(3.0f);
            bSpecialAnim = false;
            setStop();
            noDelayAnim = false;
        }


        /// <summary>
        /// 홈런스틸 코루틴
        /// </summary>
        /// <param name="delayT"></param>
        /// <returns></returns>
        private IEnumerator setHomerunStealDelay(float delayT)
        {
            //쇠그물수비 (홈런스틸) 연출
            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.HomerunSteal);

            noDelayAnim = true;
            float limit = (field.ball.bHomeRunGuess ? -1000 : 200);
            if (delayT < 0.1f) delayT = 0.1f;
            Vector2 fensPos = field.ball.getFencePosition();            
            actState = FielderAction._NOTHING_STATE;

            dX = (fensPos.x - posX) / 0.5f;
            dY = (fensPos.y - posY) / 0.5f;

            float fenseX = fensPos.x + (dX > 0 ? 30 : -30);
            float fenseY = fensPos.y;
            

            //playSpecialAnim((dX > 0 ? "0100_RUN_E" : "0100_RUN_W"), true, 0.6f);

            ////Debug.Log("=================>> field.ball.nBallZ = " + field.ball.nBallZ);

            while (field.ball.nBallZ > 600)
            {   
                posX += (dX * deltaTime);
                posY += (dY * deltaTime);
                if (dX != 0)
                {
                    if (Mathf.Abs(fensPos.x - posX) < 5) dX = 0;
                }
                if (dY != 0)
                {
                    if (Mathf.Abs(fensPos.y - posY) < 5) dY = 0;
                }

                if (dX == 0 && dY == 0)
                {
                    if (bSpecialAnim == true)
                    {                        
                        playSpecialAnim("0000_HOLD_S", false);
                        bSpecialAnim = false;
                    }
                }

                yield return new WaitForSeconds(deltaTime);
                delayT-= deltaTime;
            }


            float gabX = (fenseX - posX);
            float gabY = (fenseY - posY);
            if (Mathf.Abs(field.ball.firstAngle) < 18)
            {
                nFielderDir = FieldParm._NORTH;
            }
            else
            {
                if (field.ball.firstAngle < 0)
                {
                    nFielderDir = FieldParm._NORTHEAST;
                }
                else
                {
                    nFielderDir = FieldParm._NORTHWEST;
                }
            }

            float _curTime = 0;
            dX = gabX / delayT;
            dY = gabY / delayT;

            /*특수하게 여기서 하드코딩으로 연출할 수도 있음
            field.ball.setFocusMove(field.ball.nBallX, field.ball.nBallY, posX, posY, BallEvent.EVENT_FIELDER_FOCUS, posIndex, 0.5f);
            field.setZoomTo(1.0f, 0.5f);
            */

            //해당애니메이션
            playSpecialAnim("FENCE_JUMP_CATCH_" + FieldParm._dir[nFielderDir], false);


            bool bBallDraw = true;
            bool bBallCatched = false;

            while (field.ball.bBallCatched == false)
            {                
                if (bBallDraw == true)
                {
                    if (field.ball.nBallZ + (field.ball.nBallDZ * deltaTime) < limit)
                    {
                        field.ball.setDraw(false);
                        field.ball.setParticleDraw(false);
                        bBallDraw = false;
                    }
                }

                posX += (dX * deltaTime);
                posY += (dY * deltaTime);
                yield return new WaitForSeconds(deltaTime);
                _curTime += deltaTime;
                if (_curTime > delayT)
                {
                    bBallCatched = true;
                    setFieldingCatchCondition(true);
                    break;
                }
            }

            if (bBallCatched == false)
            {
                bBallCatched = true;
                setFieldingCatchCondition(true);
            }

            dX = dY = 0;

            field.setZoomTo(1.5f, 0.5f);
            yield return new WaitForSeconds(1.0f);
            bSpecialAnim = false;
            setState(FielderAction._MOTION, ActionStep._MOTION_SET);
            
            
        }

        /// <summary>
        /// 어리버리 플라이 캐치 에러
        /// </summary>
        /// <returns></returns>
        private IEnumerator flyCatchMiss()
        {
            noDelayAnim = true;
            float delay = field.ball.firstBoundTime - field.ball.curTime;            
            actState = FielderAction._NOTHING_STATE;
            nFielderDir = FieldParm._SOUTH;

            if (delay > 2.0f)
            {
                playSpecialAnim((posIndex == CPlayer._CATCHER ? "9300_CATCHER_FLY_CATCH_S" : "3000_FLYBALL_CENTER_S"), false, 0);
                yield return new WaitForSeconds(delay - 2.0f);
            }
            playSpecialAnim((posIndex == CPlayer._CATCHER ? "FLYBALL_MISS_CATCH3" : "FLYBALL_MISS_CATCH1"), false,1,false);

            while ((field.ball.nBallZ) > 50) //while (field.ball.bBound == false)
            {
                yield return new WaitForEndOfFrame();
            }

            StartCoroutine(setErrorEvent(false, true));

            yield return new WaitForSeconds(1.0f);

            //플라이 경우
            bSpecialAnim = false;
            noDelayAnim = false;
        }


        /// <summary>
        /// 레이저 연출
        /// </summary>
        /// <param name="destRunner">잡으려고 하는 주자</param>
        /// <param name="throwBase">던지는 베이스</param>
        /// <param name="shobutype">승부 타입</param>
        /// <returns></returns>
        private IEnumerator laserThrowDelay(Runner destRunner, int throwBase, int shobutype)
        {
            fieldSkillDisplayManager.AddSkill(gameObject, pFielder, SkillIndex.Laser);

            //shobutype 0상황없음 1:베이스 턴 후 2: 베이스 턴전
            actState = FielderAction._NOTHING_STATE;
            noDelayAnim = true;                        
            
            bLaserThrowCheck = true;
            int laserBase = throwBase +(shobutype == 2 ? 1 : 0);
            bThrowAvailable = true;
            field.nTargetIndex = laserBase;
            bThrowableChecked = true;
            float ddx = field.getOriginX(FieldSize.getBasePosX(field.nTargetIndex));
            float ddy = field.getOriginY(FieldSize.getBasePosY(field.nTargetIndex));
            angleDir = Mathf.Atan2(ddy - posY, ddx - posX);
            nFielderDir = FieldParm.getDir(angleDir);

            //Debug.Log("===================>>주자 상태 " + destRunner.state);

            float checkTime = 0;
            while (laserBase != destRunner.destPos)
            {
                //문제 많음
                yield return new WaitForSeconds(deltaTime);
                checkTime += deltaTime;
                if (checkTime > 1.0f)
                {
                    yield break;
                }
            }


            //##연출 외야수 레이저 송구
            float laserDelay = 0;//
            float laserSpeed = SkillParm.getLaserThrow();

            //field.setZoomTo(2.0f, 1.5f);
            //field.setSkillEffect(true, pFielder, SkillIndex.Laser);
                        
            playSpecialAnim("SSS_THROW_" + FieldParm._dir[nFielderDir], false,1,false);
            
            //볼 데드 상태 예외 처리
            if (field.ball.bBallDeadState == true)
            {
                field.ball.setFielderFocus2(posIndex, 1.5f);
                field.setZoomTo(2.5f, 1.5f);
            }


            field.ball.setFielderFocus(posIndex);
            dX = dY = 0;
            setThrowPreCondition(laserSpeed / THROW_SPEED);

            field.setTimeScale(0.5f); //모두 정지
            setTimeScale(Field.INIT_TIME_SCALE); //필더만 움직임
            while (bThrowAnimFlag == false)
            {
                yield return new WaitForSeconds(deltaTime);
            }
            field.setTimeScale(Field.INIT_TIME_SCALE);

            ////Debug.Log("===================>>주자 상태2 " + destRunner.state);

            if (laserBase == FieldParm.THIRDBASE_INDEX)
            {
                //레이저 카운터용 주자 '슬라이딩' 스킬
                bool bOffenseWin = false;
                if (destRunner.pRunner.fieldSkillSuccess(SkillIndex.RunnerSliding) == true)
                {
                    int offenseRank = destRunner.pRunner.getSkillRank(SkillIndex.RunnerSliding);
                    int defenseRank = pFielder.getSkillRank(SkillIndex.Laser);
                    bOffenseWin = SimulParm.checkOffenseSkillWin(offenseRank, defenseRank);
                }
                //레이저 여기 고쳐
                float timeLeft = field.getTimeLeftforThrow(laserBase, posX, posY, laserDelay, laserSpeed);
                if (bOffenseWin == true)
                {
                    fieldSkillDisplayManager.AddSkill(destRunner.gameObject, destRunner.pRunner, SkillIndex.RunnerSliding);
                }
                else
                {
                    timeLeft += 1;
                }                
                destRunner.setShobuRunnerSpeed(false, timeLeft, 2.5f);
            }
            else
            {                
                if (destRunner.bRushSkillOn == true && field.bRushCounterHappen == true)
                {
                    destRunner.checkCounterHomeRush();
                }
                float timeLeft = field.getTimeLeftforThrow(laserBase, posX, posY, laserDelay, laserSpeed) + 1;
                destRunner.setShobuRunnerSpeed(false, timeLeft, 2.5f);
            }



            field.ball.bLaserThrowFlag = true;

            setThrowConditionAfterCatch();
            field.ball.bCameraBallMove = true;
            bThrowAnimFlag = false;
            bSpecialAnim = false;


            fieldSkillDisplayManager.RemoveSkill(pFielder, SkillIndex.Laser);
            
        }

    }
}