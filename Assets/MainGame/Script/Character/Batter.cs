//#define _NO_CONTOL_BATTING_TYPE //컨트롤에 의한 배팅 타입 무효화 ㅠㅠ //->이건 지우면 안됨 확정
//#define _TEST_VERSION
//#define _BATTER_NO_AI             //와일7드 피치 테스트시 킴
//#define _VARIATION_TEST
//#define _NO_TEXTURE_LOADING       //지워지워  
//#define _BATTER_TEXTURE_TEST


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class Batter : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////
        //오브젝트
        ////////////////////////////////////////////////////////////////////
        public BallPlayManager manager;
        Pitcher pitcher;
        Field field;
        runnerManager run;
        Batting battingview;
        public Zone zoneUI;
        public tk2dSprite ballImage;
        

        //애니메이션
        public SkeletonAnimation anim = null;
        public SkeletonAnimation pAnim = null;

        //이펙트 애님
        public SkeletonAnimation effectAnim1, batEffectAnim;

        //타자 데이터
        public CPlayer pBatter, pLastBatter;
        
        //Trasnform 초기화 데이터
        int initPosY = (125+15);//144;
        int initPosX;
        int initZDepth = -100;

        const int _LeftPitcherGab = -260;


        //상태
        public BatterState bState;
        public bool bNewBatter, bNewBatterInfo;//, resetTrace;        
        public bool bHitted, bHitChecked;	//공이 맞었는지 여부
        public bool bPreHitCheck;
        public bool bSwing;	                //스윙여부
        public bool bForcedSwingPrevent;    //강제 스윙 못하는 상태    
        public bool bGangTa;	            //강타여부
        public bool bBunt, bBuntHit;		                    //현재 번트를 댄 경우인지 여부
        public SimulBuntType buntType, buntTypePre;      //번트 지시여부    
        public SimulBuntType buntSuccess;
        public bool bSacFly;
        public int buntDir, buntFielder;    //번트방향과 번트처리야수
        public bool bTipped;
        public int aiCheckStep;
        public bool onTimingSwing;
        float swingWaitDelay;       //스윙 웨이트 딜레이

        //타자정보
        public string strName;		        //현재 타자 이름
        public int sign;		            //타자의 좌우
        public int condition;               //컨디션
        public int position, secondPosition;    //포지션
        public int batterHand, lastBatterHand;  //손
        public int battingType;             //타격타입

        //타구정보 //배팅시스템
        public BattingTiming timing, aiTimingPoint;     //타구타이밍
        public BattingContact contact, aiContactPoint;  //컨택값
        public int timingAddPoint, contactAddPoint;
        public SpecificBuntType buntResult; //번트결과
        
        //파생 능력치
        public int bEye, bContact, bPower, bTando;
        private int arrivePointGuessPer;//, checkSwingPer;   //예측 (percent)
        public bool bArrivePointGuess;
        public bool bCheckSwing, bCheckSwingActivate;
        public bool bMustCheckSwing;    //100%확률로 체크스윙
        public bool bCheckSwinged;

        //산출 능력치
        //private float meetPoint;    //타격강도
        //private float timingPoint;  //타이밍강도
        //private float powerCoef;    //파워계수

        //미트커서 관련
        public float cursorX, cursorY;
        public float cursorDX, cursorDY;	        //배터가 움직이는 위치
        public float battingOffsetX, battingOffsetY;	    //타격 미스 옵셋
        float calibrationValue;                     // 옵셋 조정값
        public bool bStrkeSwing, bBadballSwingLow, bBadballSwingHigh, bBadballSwingFar, bBadballSwingNear;    //볼을 칠경우 마이너스 요인
        public bool bFarBatting, bNearBatting, bHighBatting, bLowBatting;

        //배트 사이즈
        public float batRealSizeX;
        public float batRealSizeY;
        public float batGangtaSize;
        //public float batFlyGrounderGab;    //배트의 플라이 그라운더 갭 -> 이영역을 넘어가면 강제 플라이 혹은 그라운더
        
        //타격 AI
        bool bAiCheck;
        public bool bReleaseCheck;
        public bool bGuessCorrect, bStrikeCheck;
        //public float _aiAutoPower, _aiAutoAngle, _aiAutoDir, _aiAutoHoolSlice;
        public bool aiHitandRunDecide;

        //AI로부터 나온 타이밍을 통해 조정되는 파워계수
        public float powerCoef1,    //컨택 파워조정계수
                     powerCoef2,    //타이밍 파워조정계수
                     powerCoef3;    //강태 파워 조정계수    

        //기타
        public int outBatter;   //교체 아웃되는 타자
        string batPos, batCourse, hitType;
        public int curLineupCount;
        int skillOrder;
        public bool bHitGood, bHitHomeRun;

        private bool bBatFlipEvent, bBatFlipEffect;


        //ai 오토모드 타격 특수
        public AutoModeBatting autoModeBatting;
        private bool autoModeSwing;
        //bool bNeverContact; //true인경우 컨택이 일어나지 않는다.

        //float ballDrawDelay;    //배팅뷰에서 볼이 나오는 딜레이

        //배팅 타이밍영
        public float batterPerfectTime;
        public float perfectTimingGabRate;

        //다양한 타구 생산
        //public SpecialHitType specialHitType;
        //public bool bSpecialBattingOn;


        public bool bPvState;


        //타이밍 밸류
        public float _PERFECT_FAST;
        //정타 타이밍(늦은쪽)
        public float _PERFECT_LATE;
        //퍼펙트 타이밍
        public float _PERFECT_TIMING;

        private float justEarlyGab, earlyGab, justLateGab, lateGab;




        // Use this for initialization
        void Start()
        {
            
        }

        private void Update()
        {
            //교착상태에 빠진경우 고려해보자
            /*if (Input.GetKeyUp(KeyCode.A))
            {
                if (manager.bMyTurn == true)
                {
                    if (Mode.bPvpMode433 == true)
                    {
                        pvpmanager.Get().SendBatterSync(manager);
                    }
                }
            }*/
        }

        //인스턴스 초기화
        public void initInstance(BallPlayManager manager)
        {
            anim = null;
            pAnim = null;

            lastBody = -1;
            bPvState = false;
            //transform.parent = GameObject.FindWithTag("BATTINGVIEW_TAG").transform;
            ballImage.gameObject.SetActive(false);

            this.manager = manager;
            pitcher = manager.pitcher;
            field = manager.field;
            run = field.run;
            battingview = manager.battingview;            
            //master = manager.master;
            //shadow.renderer.enabled = false;

            effectAnim1.transform.localPosition = new Vector3(0, 0, -0.01f);
            effectAnim1.transform.localScale = new Vector3(100.0f, 100.0f, 1);
            effectAnim1.gameObject.SetActive(false);
        }
        
        //스파인 이벤트 처리
        public void Event(Spine.AnimationState state, int trackIndex, Spine.Event e)
        {
            Debug.Log(trackIndex + " " + state.GetCurrent(trackIndex) + ": event " + e + ", " + e.Int);
            string strEvent = e.ToString();
            if (strEvent == "hit")
            {
                if (bSwing == true)
                {
                    //////UnityEngine.//Debug.Log("===================>>피치 시스템 지워");
                    pitcher.setPitchSystemDraw(false);
                }
            }
        }

        


        public void changeShader(string shaderName)
        {
            AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
            Material[] materials = atlasdata.materials;

            Shader curShader = Shader.Find(shaderName);

            materials[0].shader = curShader;
        }
        
        //그래픽 관련변화시 이미지 로딩을 담당
        private string teamPath = null,
                       facePath = null,
                       logoPath = null;
                       
        private int lastBody = -1;

#if _BATTER_TEXTURE_TEST
        public int nextBody = 1;
        public int nextFace = 1;
        public int nextTeam = 1;
#endif

        public bool bLoadBatterFlag;
        
        public void LoadBatter()
        {
            //Debug.Log("진짜 배터 로딩");
            StartCoroutine(loadBatter(pBatter));                            
        }


        private readonly string batterDefault = "MainGame/spineData/battingview/batter/anim";

        private IEnumerator loadBatter(CPlayer player)
        {       

            if(manager.bTopInning == true)
            {
                Transform bottom = transform.Find("bottom");
                if(bottom != null)
                {
                    //Debug.Log("탑이닝 배터 체크");
                    Destroy(bottom.gameObject);
                }
            }
            else
            {
                Transform top = transform.Find("top");
                if (top != null)
                {
                    //Debug.Log("바텀이닝 배터 체크");
                    Destroy(top.gameObject);
                }
            }
          
#if _BATTER_TEXTURE_TEST
            int index = nextTeam;
#else
            int index = manager.bTopInning ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex;
#endif
            //0:배트 1:몸통 2:스파이크 3:글러브 4:스타일 5:로고 6:헬멧 7:암 8: 레그
            if (player != null)
            {                
                if(manager.bMyTurn == true || Mode.bPitchingViewActive == false)// if (bPvState == false)
                {
#if _BATTER_TEXTURE_TEST
                    int body = nextBody;
#else
                    DefineEnum.EPlayerBody bodyType = player.getBody();
                    int body = Mathf.Clamp((int)bodyType, 1, 3);
#endif
                    //타자뷰 배터
                    if (anim == null || body != lastBody)
                    {
#if GIRL_PLAY
                        GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/batter/batterSkelPrefab_girl", transform, Vector3.zero, manager.bTopInning ? "top" : "bottom");
                        anim = skeleton.GetComponent<SkeletonAnimation>();
                        anim.transform.localScale = new Vector3(30, 30, 1);
#else

                        GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/batter/batterSkelPrefab" + body, transform, Vector3.zero, manager.bTopInning?"top":"bottom");
                        anim = skeleton.GetComponent<SkeletonAnimation>();
                        
                        batEffectAnim = anim.transform.Find("SkeletonUtility-Root").Find("root").Find("master").Find("body_01").Find("R_arm_01").Find("R_arm_02").Find("hand_02").Find("bat_01").Find("batAura").GetComponent<SkeletonAnimation>();
                        batEffectAnim.gameObject.SetActive(false);

                        if (bodyType == DefineEnum.EPlayerBody.NORMAL)
                        {
                            //꼬마
                            anim.transform.localScale = new Vector3(120, 120, 1);
                        }
                        else if (bodyType == DefineEnum.EPlayerBody.MUSCLE)
                        {
                            //근육
                            anim.transform.localScale = new Vector3(125, 125, 1);
                        }
                        else if (bodyType == DefineEnum.EPlayerBody.FAT)
                        {
                            //뚱
                            anim.transform.localScale = new Vector3(125, 125, 1);
                        }
#endif
                        teamPath = facePath = logoPath = null;

                        lastBody = body;

                        anim.gameObject.SetActive(true);
                        yield return new WaitForEndOfFrame();
                    }

#if GIRL_PLAY

#else
                    
                    //타자뷰
                    AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
                    Material[] materials = atlasdata.materials;

                    string texturePath = "MainGame/spineData/battingview/batter/";

                        //장비 & 몸통
                     string curTeamPath = texturePath + "team" + body + "/" + index;
                     if (curTeamPath != teamPath)
                     {
                            //Debug.Log("장비 몸통 재로딩 " + curTeamPath);
                         teamPath = curTeamPath;
                         Texture curTexture1 = (Texture)Resources.Load(teamPath + "/batterAnim");
                         if (curTexture1 == null)
                         {
                             curTexture1 = (Texture)Resources.Load(batterDefault + body + "/batterAnim");
                         }
                         materials[0].mainTexture = curTexture1;

                         Texture curTexture2 = (Texture)Resources.Load(teamPath + "/batterAnim2");
                         if (curTexture2 == null)
                         {
                             curTexture2 = (Texture)Resources.Load(batterDefault + body + "/batterAnim2");
                         }
                         materials[1].mainTexture = curTexture2;
                     }
                  
                    //얼굴
#if _BATTER_TEXTURE_TEST
                    int faceIndex = nextFace;
#else
                     int faceIndex = player.getFace();
#endif

                    ////Debug.Log("==========>>faceIndex = " + faceIndex);

                    string curFacePath = texturePath + "face/" + faceIndex + "/batterAnim3";
                    if (curFacePath != facePath)
                    {
                        //Debug.Log("얼굴 재 로딩 " + curFacePath);
                        facePath = curFacePath;
                        Texture curTexture = (Texture)Resources.Load(facePath);
                        if(curTexture == null)
                        {
                            curTexture = (Texture)Resources.Load(batterDefault+body+"/batterAnim3");
                        }
                        materials[2].mainTexture = curTexture;
                    }
                    //로고
                    string curLogoPath = texturePath + (sign == 1 ? "logoRight/" : "logoLeft/") + index + "/batterAnim4";
                    if (curLogoPath != logoPath)
                    {
                        //Debug.Log("로고 재 로딩 " + curLogoPath);
                        logoPath = curLogoPath;
                        Texture curTexture = (Texture)Resources.Load(logoPath);
                        if (curTexture == null)
                        {
                            curTexture = (Texture)Resources.Load(batterDefault + body + "/batterAnim4");
                        }
                        materials[3].mainTexture = curTexture;
                    }
#endif

                }
                else
                {
                    
#if _BATTER_TEXTURE_TEST
                    int body2 = nextBody;
#else
                    DefineEnum.EPlayerBody bodyType2 = player.getBody();
                    int body2 = Mathf.Clamp((int)bodyType2,1,3);
#endif
                    ////Debug.Log("==============>>body2 = " + body2);
                    ////Debug.Log("==============>>player.getBody() = " + player.getBody());


                    if (pAnim == null || lastBody != body2)
                    {                        
                        GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/pBatter/pBatterSkelPrefab" + body2, transform, Vector3.zero, "skeleton2");
                        pAnim = skeleton.GetComponent<SkeletonAnimation>();

                        batEffectAnim = null; //임시

                        if (bodyType2 == DefineEnum.EPlayerBody.NORMAL)
                        {
                            //꼬마
                            pAnim.transform.localScale = new Vector3(80, 80, 1);
                        }
                        else if (bodyType2 == DefineEnum.EPlayerBody.MUSCLE)
                        {
                            //근육
                            pAnim.transform.localScale = new Vector3(95, 95, 1);
                        }
                        else if (bodyType2 == DefineEnum.EPlayerBody.FAT)
                        {
                            //뚱
                            pAnim.transform.localScale = new Vector3(90, 90, 1);
                        }
                        pAnim.gameObject.SetActive(true);
                        teamPath = facePath = logoPath = null;
                        lastBody = body2;
                        yield return new WaitForEndOfFrame();
                    }
#if _NO_TEXTURE_LOADING

#else
                    
                    //투수뷰
                    //타자뷰
                    AtlasAsset atlasdata = pAnim.skeletonDataAsset.atlasAssets[0];
                    Material[] materials = atlasdata.materials;

                    string texturePath = "MainGame/spineData/pitchingview/batter/";

                    //장비 & 몸통
                    string curTeamPath = texturePath + "team" + body2 + "/" + index;
                    if (curTeamPath != teamPath)
                    {
                        //Debug.Log("장비 몸통 재로딩 " + curTeamPath);
                        teamPath = curTeamPath;
                        materials[0].mainTexture = (Texture)Resources.Load(teamPath + "/pvBatterAnim");
                        materials[1].mainTexture = (Texture)Resources.Load(teamPath + "/pvBatterAnim2");
                    }

                    //얼굴
#if _BATTER_TEXTURE_TEST
                    int faceIndex = nextFace;
#else
                    int faceIndex = player.getFace();
#endif
                    string curFacePath = texturePath + "face/" + faceIndex + "/pvBatterAnim3";
                    if (curFacePath != facePath)
                    {
                        //Debug.Log("얼굴 재 로딩 " + curFacePath);
                        facePath = curFacePath;
                        materials[2].mainTexture = (Texture)Resources.Load(facePath);
                    }

                    //로고
                    string curLogoPath = texturePath + (sign == 1 ? "logoRight/" : "logoLeft/") + index + "/pvBatterAnim4";
                    if (curLogoPath != logoPath)
                    {
                        //Debug.Log("로고 재 로딩 " + curLogoPath);
                        logoPath = curLogoPath;
                        materials[3].mainTexture = (Texture)Resources.Load(logoPath);
                    }

#endif
                    
                }
            }
            readyAnim(true);
            yield return new WaitForSeconds(0.1f);
            bLoadBatterFlag = false;
            //System.GC.Collect();
            

        }

        public void setColor(Color color)
        {
            //anim.skeleton.SetColor(color);
        }

        /////////////////////////////////////////////////////////////////
        //위치값 세팅 함수
        /////////////////////////////////////////////////////////////////
        //타자 위치 초기화
        public void initPosition()
        {
            int batterSign;
            if (Mode.cameraView == CameraView.PitcherCenter)
            {   
                //center 123
                batterSign = sign; //
                initPosY = 375;
                initPosX = (sign == -1 ?-94:340) + (pitcher.pitchHand==CPlayer._LEFTHAND?_LeftPitcherGab:0);
                initZDepth = -5;                
            }
            else //if (Mode.cameraView == CameraView.BatterLow)
            {
                batterSign = -sign; //타자뷰에서 왼손잡이로 작업되어서
                initPosY = 145;
                initPosX = batterSign * 326;
                initZDepth = -100;                
            }


            transform.localScale = new Vector3(batterSign, 1, 1);
            transform.localPosition = new Vector3(initPosX, initPosY, initZDepth);

        }

        //좌우 타자에 따른 위치옵셋 세팅
        public void setBatterOffset(float x, float y)
        {
            if (sign == -1)
            {
                transform.localPosition = new Vector3(initPosX + x, initPosY + y, initZDepth);
            }
            else
            {
                transform.localPosition = new Vector3(-initPosX + x, initPosY + y, initZDepth);
            }
        }
                
        //볼을 맞춘 포지션에 따른 배트 포지션 위치
        public float effectX, effectY;
        float ballImageX, ballImageY;
        int courseIndex;
        public void getBatPosition()
        {            
            courseIndex = 0;
            batCourse = "_CENTER";
            batPos = "_MIDDLE";
            hitType = "_NORMAL";

            float px = sign * pitcher.preArriveX;
            float bX = pitcher.preArriveX;
            float bY = pitcher.preArriveY;

            ballImageX = -326;/// -270;
            ballImageY = 173;// 140;// 155;

            if (px > 35)
            {                
                batCourse = "_OUT";
                ballImageX = -385;// -310;
            }
            else if (px < -35)
            {                
                batCourse = "_IN";
                ballImageX = -271;// -230;
            }

            if (px > 25)
            {
                courseIndex = 2;
                hitType = "_PUSH";
            }
            else if (px < -25)
            {
                courseIndex = 1;
                hitType = "_PULL";
            }


            if (bY > 35)
            {
                batPos = "_UP";
                ballImageY = 210;// 170;// 185;
            }
            else if (bY < -35)
            {
                batPos = "_LOW";
                ballImageY = 116;// 90;// 105;
            }
            if (bX > 100) bX = 100;
            if (bX < -100) bX = -100;


            if (bPvState == true)
            {
                effectX = bX;
                effectY = bY;
            }
            else
            {
                effectX = bX;
                effectY = bY - 40;
                ballImage.transform.localPosition = new Vector3(ballImageX, ballImageY, 1.0f);
            }
        }
        
        //타자의 배트 커서 위치를 정해준다
        public void setBatterCursorPos(float x, float y, float maxX, float maxY)
        {
            cursorX = x * Zone.STRIKE_ZONE_WIDTH / maxX;
            cursorY = y * Zone.STRIKE_ZONE_HEIGHT / maxY;
            zoneUI.setBatCursorPos();
        }


        /////////////////////////////////////////////////////////////////
        //카메라 설정에 따른 타자 카메라 세팅
        /////////////////////////////////////////////////////////////////
        /// <summary>
        /// 카메라 세팅에 따른 설정
        /// </summary>
        public void setCameraSetting(bool bDontDestroyBatter = false)
        {
            if (Mode.cameraView == CameraView.PitcherCenter && Mode.bPitchingViewActive == true)
            {
                bPvState = true;
                if (bDontDestroyBatter == false)
                {
                    if (anim != null)
                    {
                        Destroy(anim.gameObject);
                        anim = null;
                    }
                    if (pAnim != null)
                    {
                        Destroy(pAnim.gameObject);
                        pAnim = null;
                    }
                    lastBody = -1;
                }
                
                //타이밍 밸류                
                _PERFECT_TIMING = BattingMechanism.PERFECT_TIMING_PV;
                _PERFECT_FAST = _PERFECT_TIMING - BattingMechanism.TIMING_GAB;
                _PERFECT_LATE = _PERFECT_TIMING + BattingMechanism.TIMING_GAB;
                Debug.Log("p = " + _PERFECT_TIMING + " _PERFECT_FAST = " + _PERFECT_FAST + "_PERFECT_LATE = " + _PERFECT_LATE);
                justEarlyGab = 0.025f;
                earlyGab = 0.090f;
                justLateGab = 0.020f;
                lateGab = 0.070f;

                //이펙트 설정
                effectAnim1.transform.localPosition = new Vector3(-30, 0, -0.01f);
                effectAnim1.transform.localScale = new Vector3(150, 150, 1);
                effectAnim1.gameObject.layer = LayerMask.NameToLayer("BATTINGVIEW_LAYER");

            }
            else
            {
                bPvState = false;
                if (bDontDestroyBatter == false)
                {
                    if (anim != null)
                    {
                        Destroy(anim.gameObject);
                        anim = null;
                    }
                    if (pAnim != null)
                    {
                        Destroy(pAnim.gameObject);
                        pAnim = null;
                    }
                    lastBody = -1;
                }

                //타이밍 밸류                
                _PERFECT_TIMING = BattingMechanism.PERFECT_TIMING;
                _PERFECT_FAST = _PERFECT_TIMING - BattingMechanism.TIMING_GAB;
                _PERFECT_LATE = _PERFECT_TIMING + BattingMechanism.TIMING_GAB;
                //Debug.Log("p = " + _PERFECT_TIMING + " _PERFECT_FAST = " + _PERFECT_FAST + "_PERFECT_LATE = " + _PERFECT_LATE);
                justEarlyGab = 0.025f;
                earlyGab = 0.090f;
                justLateGab = 0.020f;
                lateGab = 0.070f;


                //이펙트 설정
                effectAnim1.transform.localPosition = new Vector3(0, 0, -0.01f);
                effectAnim1.transform.localScale = new Vector3(180, 180, 1);
                effectAnim1.gameObject.layer = LayerMask.NameToLayer("BATTER_LAYER");
            }
            initPosition();
            //setCameraOffset();
            System.GC.Collect();
        }

        

        /////////////////////////////////////////////////////////////////
        //기초 애니매이션
        /////////////////////////////////////////////////////////////////
        //초기화 후 애니메이션
        int lastTrack;
        public string strID = "";
        public void batterAnim(int track, string strAnim, bool bLoop, float timeScale = 1.0f)
        {
            if (strID != strAnim)
            {
                if (Mode.cameraView == CameraView.PitcherCenter)
                {
                    if (pAnim != null)
                    {
                        pAnim.state.ClearTracks();
                        pAnim.skeleton.SetToSetupPose();
                        pAnim.state.SetAnimation(track, strAnim, bLoop);
                        pAnim.timeScale = timeScale;
                    }
                }
                else
                {
                    if (anim != null)
                    {
                        anim.state.ClearTracks();
                        anim.skeleton.SetToSetupPose();
                        anim.state.SetAnimation(track, strAnim, bLoop);
                        anim.timeScale = timeScale;
                    }
                }
                lastTrack = track;
                strID = strAnim;
            }
        }

        //블렌딩 애니메이션
        private void batterAnimBlend(string strAnim, bool bLoop, float timeScale = 1.0f)
        {
            if (Mode.cameraView == CameraView.PitcherCenter)
            {
                pAnim.skeleton.SetToSetupPose();
                pAnim.state.SetAnimation(lastTrack, strAnim, bLoop);
                pAnim.timeScale = timeScale;
            }
            else
            {
                anim.skeleton.SetToSetupPose();
                anim.state.SetAnimation(lastTrack, strAnim, bLoop);
                anim.timeScale = timeScale;
            }
            strID = strAnim;
        }

        //이펙트 애니메이션

        /// <summary>
        /// 외부에서 제어
        /// </summary>
        /// <param name="bActive"></param>
        /// <param name="strAnim"></param>
        public void AuraEffect(bool bActive, string strAnim, Vector3 pos, Vector3 scale)//, Color color)
        {
            if (bActive == true)
            {
                effectAnim1.transform.localPosition = pos;
                effectAnim1.transform.localScale = scale * 100;
                effectAnim1.gameObject.SetActive(true);
                if (effectAnim1.state.Data.skeletonData.FindAnimation(strAnim) != null)
                {
                    //if (color != Color.white) effectAnim1.skeleton.SetColor(color);
                    effectAnim1.state.ClearTracks();
                    effectAnim1.skeleton.SetToSetupPose();
                    effectAnim1.state.SetAnimation(0, strAnim, false);
                }
            }
            else
            {
                effectAnim1.gameObject.SetActive(false);
                effectAnim1.skeleton.SetColor(new Color(1,1,1,1));
            }
        }

        public void BatAuraEffect(bool bActive, string strAnim, Vector3 pos, Vector3 scale)
        {
            if (batEffectAnim != null)
            {
                if (bActive == true)
                {
                    batEffectAnim.transform.localPosition = pos;
                    batEffectAnim.transform.localScale = scale;
                    batEffectAnim.gameObject.SetActive(true);
                    if (batEffectAnim.state.Data.skeletonData.FindAnimation(strAnim) != null)
                    {
                        batEffectAnim.state.ClearTracks();
                        batEffectAnim.skeleton.SetToSetupPose();
                        batEffectAnim.state.SetAnimation(0, strAnim, false);
                    }
                }
                else
                {
                    batEffectAnim.gameObject.SetActive(false);
                }
            }
        }


        /// <summary>
        /// 외부에서 애니메이션 제어
        /// </summary>
        private string lastAnim;
        private bool bLastLoop;
        private int lastTrack2;
        public void AnimEffect(bool bActive, string strAnim)
        {
            if (bActive == true)
            {
                lastTrack2 = lastTrack;
                lastAnim = strID;
                bLastLoop = (bPvState ? pAnim.loop : anim.loop);
                batterAnim(0, strAnim, true);
            }
            else
            {
                batterAnim(lastTrack2, lastAnim, bLastLoop);
            }
        }
        /////////////////////////////////////////////////////////////////
        //타자 초기화 (InitBatter)
        /////////////////////////////////////////////////////////////////
        //long curSeq = -1;
        //타자 초기화 - 다음 타자로 바뀔떄(혹은 이어하기시) 한번 호출 된다
        public void initBatter(CPlayer player, int team, bool bPinchHitter = false)
        {
            //Debug.Log("타자 초기화");
            /*Debug.Log("batter contact = " + player.getContact());
            Debug.Log("batter power = " + player.getPower());
            Debug.Log("batter eye = " + player.getEye());
            Debug.Log("batter fielding = " + player.getFielding());
            Debug.Log("batter throwing = " + player.getThrowing());
            Debug.Log("batter running = " + player.getSpeed());*/
#if _Test_Local
            //if(pitcher.pPitcher!=null) Debug_UI.SetPitcher(pitcher.pPitcher);  //지워지워
            //localBalance.SetBattingBalance();
            //localBalance.SetPitchingBalance();
            //localBalance.SetFieldingBalance();
            //localBalance.SetRunningBalance();
#endif
            manager.batterSkillFlag = SkillFlag.None;
            manager.bBattingPreUpdate = false;

            //스킬 연출 초기화            
            SimulSteal.catcherSitThrow = FieldSkillUse.Init;
            SimulSteal.runnerStealMarster = FieldSkillUse.Init;

            //effectAnim1.renderer.enabled = false;
            //effectAnim2.renderer.enabled = false;
            ////UnityEngine.//Debug.Log("=============================>>initBatter");
            //manager.bInfoShow = false;   //인포가 보여줬는지 여부
            field.ball.cameraWork = CameraWork.Default;


            autoModeBatting = AutoModeBatting.Normal;
            buntType = SimulBuntType.NONE;
            buntSuccess = SimulBuntType.NONE;
            bSacFly = false;
            
            pBatter = player;

            //스킬 초기화
            manager.nBatterCount++;
            skillOrder = 0;
            pBatter.setBonusInit();
            //manager.gameUI.removeBvSkillIcon(); //[UI]스킬 아이콘 초기화

            //아웃플래그 초기화
            manager.setOutFlagInit();

            //bool bInjury = false;

            field.fieldShift = 0;	//필딩 초기화

            field.run.setStealInit(); //도루 초기화


            //이부분 나중에 수정 - 심판 액션관련
#if _NO_JUDGE
             for(int kkk=0;kkk<4;kkk++) field.jugde[kkk].setJugdeInit();//심판 초기화 필요있으면 활성화 시켜
#endif


            //manager.nDoublePlayCount = 0;	//더블플레이 카운트
            
            manager.nLastScore = manager.nGameScore[manager.offenseIndex];	//타점 관련		

            


            //타자 정보
            curLineupCount = SimulPlayerManager.GetLineupCount(team); //manager.lineupCount[team];
            //이름
            strName = pBatter.getName();
            //포지션 얻어오기
            position = pBatter.getPosition();	//타자의 포지션
            secondPosition = position;          // lineup.pPlayer[team, index].m_nSecondPosition;//
            //컨디션 얻어오기
            condition = 0;// lineup.pPlayer[team, index].m_nCondition;
            //타격 자세
#if _Test_Local
            battingType = ((curLineupCount + 3) % 7);// pBatter.getBattingType();  //타격타입
#else
            battingType = pBatter.getBattingType();  //타격타입
#endif
            //타격 손
            batterHand = pBatter.getHitHand();//0왼손 1오른손 여기여기
            sign = (batterHand == CPlayer._LEFTHAND ? -1 : 1);
            if (batterHand == CPlayer._SWITCHHAND)	//양타인 경우
            {
                sign = (pitcher.pitchHand == CPlayer._LEFTHAND ? 1 : -1);
            }

            
            /*
            //부상여부 
            bInjury = false;

            if (bInjury)
            {
                //능력치 하락
            }
            else
            {
                //능력치 적용
            }


            if (manager.bMyTurn == false)
            {
                //말로 갈수록 컴퓨터 유리해짐
                if (manager.nInningCount > 13)
                {
                    //이닝이 뒤로 갈수록 컴퓨터 유리 설정
                }
            }*/

            //등장관련 초기화
            bNewBatter = true;
            //resetTrace = true;
            bNewBatterInfo = true;


            //배트 사이즈 매 타석 초기화
            cursorX = cursorY = 0;
            cursorDX = cursorDY = 0;
            bGangTa = false;
            zoneUI.setGangtaCursor();
            

            
            //배트 플립 여부
            bBatFlipEvent = BattingMechanism.checkBatFlip();

            //타자관련 UI 초기화
            //manager.gameUI.setBatterInfo(team, pBatter, curLineupCount); //[UI]타자 정보 초기화


            conStrike = 0;
            conSameBall = 0;


            //견제 카운트를 0으로 세팅
            field.pickOffCount = 0;
            field.bUpdateStealOrPickOff = false;

            if (Mode.bPvpMode433 == false)//if (Mode.bPvpMode == false)
            {
                if (manager.bMyTurn == false)
                {
                    //AI의 도루
                    field.run.getAIStealResult();
                }
            }

            field.curBatterFoulNum = 0;
            //field.bSqueezePitchedOut = false;
            field.bSqueezeFieldOut = false;
            //field.bSqeezAreadyFail = false;
            
            //에러 초기화
            field.setCatchError();
            field.setThrowError();

            //for (int i = 0; i < 9; i++) Debug.Log("원래 bCatchErrorFlag : " + field.fielder[i].bCatchErrorFlag + "     bThrowErrorFlag : " + field.fielder[i].bThrowErrorFlag);

            if (bPinchHitter == false)
            {
                manager.addBatterRecord(Param.ST_PA);           //타자 타수 증가
                manager.addBatterRecord(Param.ST_AB);           //타자 타석 증가 - 포볼시 다시 뺴줄것
                manager.addPitcherRecord(Param.ST_TBF);         //투수 피타수 증가
                ////UnityEngine.Debug.Log("[엔진기록]&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&>>투수피타수 현재타자 " + pBatter.getName());
            }
            else
            {
                //대타 특능 부여
                manager.addBatterRecord(Param.ST_PA);           //타자 타수 증가
                manager.addBatterRecord(Param.ST_AB);           //타자 타석 증가 - 포볼시 다시 뺴줄것
                pLastBatter.setRecord(Param.ST_PA, -1);
                pLastBatter.setRecord(Param.ST_AB, -1);
            }
            pLastBatter = pBatter;
            bLoadBatterFlag = true;


            if (pBatter != null)
            {
                /*
                if (Mode.bPvpMode == true)
                {
                    PvpManager.GetInstance().InitBatter();
                }*/

                if(Mode.bPvpMode433 == true)
                {
                    //pvpmanager.Get().SendBatterSync(manager);
                }
            }

            IngameUI.GetControlRunner().bActiveAvailble = true;

            
        }

        //번트 결과를 리체크 한다 -> 버그 방지용
        public void recheckBuntResult()
        {
            buntResult = getBuntDynamicResult();
            ////Debug.Log("=================++++++++++++++++++++++++++++++++++++++++++++++++++++++>> 내 수동 조작인 경우 buntResult = " + buntResult);
            ////Debug.Log("=================++++++++++++++++++++++++++++++++++++++++++++++++++++++>> 내 수동 조작인 경우 buntTypePre = " + buntTypePre);
        }


        //타순 변환에 따른 다음 타자 세팅
        //이함수 안에는 기본적으로 initBatter()가 호출되고 타순의 변화까지 고려한것
        public void nextBatter()
        {
            if (Mode.b2outBaseLoadedMode == true)
            {
                manager.bNineTwoNextRound = true;                
                return;
            }

            manager.checkGoodByeGame();            
            //SimulManager.AddGameSummuryInfo("\n[ffde00]" + pBatter.getName() + ": " + manager.strBatterResult + "[-]");
            if (manager.bGoodByeHitCall == true)
            {
                //SimulManager.AddGameSummuryInfo("[ff3a3a] (끝내기)[-]");
                return;    //끝내기
            }

            //스태미너 처리
            pitcher.setStaminaChange();


            manager.strFieldOutType = "";
            manager.strHitType = "";
            manager.strHitType2 = "";
            //////UnityEngine.//Debug.Log("=======================>>엔진 NEXT BATTER");
            int currentIndex;

            manager.bChangeFlagBatter = manager.bChangeFlagRunner = false;

            //볼카운트 정리
            manager.newStrikeCount = manager.nStrikeCount = 0;
            manager.newBallCount = manager.nBallCount = 0;

#if _Local_Balance
            if (InGameDebug._BALL_COUNT_SETTING == true)
            {
                //로컬 밸런스로 임의 볼카운트 세팅
                manager.newStrikeCount = manager.nStrikeCount = 2;
                manager.newBallCount = manager.nBallCount = 0;
            }
#endif


            //쓰리아웃시 더이상 진행하지 않음
            if (manager.bThreeOutChange == true) return;    // 쓰리 아웃인 경우 다음

            //인덱스 정리
            currentIndex = manager.offenseIndex;// (manager.bMyTurn ? Lineup._1P : Lineup._2P);        

            SimulPlayerManager.SetLineupCount(currentIndex);

            CPlayer next = null;

            /*타자교체
            if (Mode.bPvpMode == false)
            {
                if (Mode.bAutoPlay == true || manager.bMyTurn == false)
                {
                    if (manager.bChangeFlagBatter == false)
                    {
                        manager.bBatterChangeFlag = SimulManager.CheckChangeBatter();
                        manager.bChangeFlagBatter = true;
                    }
                }
            }*/

            //오더와 라인 오더가 일치하는 경우      
            SimulManager.SyncGameRecord(manager);
            SimulManager.SimulInitBatter();
            next = SimulPlayerManager.GetBatter(currentIndex);
            //////UnityEngine.//Debug.Log("===========================================================>> 타자 = " + next.getName());
            

            initBatter(next, currentIndex);  //타자 초기화
            run.makeHitterRunner(next);	    //타자 주자 초기화
            run.setRunnerStandBy();         //루상의 주자 스탠바이

            //상태        
            pitcher.pState = PitcherState._GET_SIGN;    //투수상태 초기화
            bState = BatterState._WAITING;              //타자 상태 초기화
            
            //게임 저장
            manager.saveGame1();
            manager.saveGame2();


            //기타 설정            
            bNewBatter = true;
            IngameUI.GetPlayerInfo().InfoInitPos(); //UI위치 초기화
            ////////UnityEngine.//Debug.Log("================>>여기 들어오냐?");
            battingview.setBVRunnerState();

            //투수교체 플래그
            manager.bChangeFlag = true;
            manager.bPitcherChangeException = false;

            int curBodyIndex = (int)pBatter.getBody();
            if (curBodyIndex != lastBody)
            {
                if (Mode.bPitchingViewActive == true)
                {
                    if (manager.bMyTurn == true)
                    {
                        if (anim != null)
                        {
                            Destroy(anim.gameObject);
                            anim = null;
                        }
                    }
                    else
                    {
                        if (pAnim != null)
                        {
                            Destroy(pAnim.gameObject);
                            pAnim = null;
                        }
                    }
                }
                else
                {
                    if (anim != null)
                    {
                        Destroy(anim.gameObject);
                        anim = null;
                    }
                }
            }

        }

        
        //배팅뷰에서 삼진후 다음 타자 세팅
        //일정 딜레이 후 nextBatter를 호출하여 다음타순으로 넘어가며 타자를 초기화 한다
        public IEnumerator nextBatterAfterStrikeOut(float delay)
        {
            //삼진시 미니맵 타자주자 지움
            IngameUI.GetFieldUI().DestroyHitterRunner();

            yield return new WaitForSeconds(delay);


            field.setFieldShift(0, true, true);            
            initPosition();

            /*
            if (Mode.bPvpMode == false) //주자교체 관련
            {
                if (Mode.bAutoPlay == true || manager.bMyTurn == false)
                {
                    if (manager.bChangeFlagRunner == false)
                    {
                        manager.bRunnerChangeFlag = SimulManager.CheckChangeRunner();
                        manager.bChangeFlagRunner = true;
                    }
                }
            }*/
           

            if (manager.bBatterChangeFlag == true || manager.bRunnerChangeFlag == true)
            {
                //타자교체 혹은 투수교체
                //manager.playState = PlayState.PLAY_BATTING_VIEW_PRE;
                nextBatter();
                bLoadBatterFlag = true;
                StartCoroutine(manager.battingviewPreDelay());
            }
            else
            {
                if (bPvState == true)
                {
                    pAnim.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.5f);                    
                }
                else
                {
                    //IngameUI.GetFieldUI().SetChangeView(BallPlayManager._BATTERCAMERA);
                    anim.gameObject.SetActive(false);
                }
                
                nextBatter();
                bLoadBatterFlag = true;
                LoadBatter();// StartCoroutine(LoadBatter());
                yield return new WaitForSeconds(1.5f);
                if (bPvState == true)
                    pAnim.gameObject.SetActive(true);
                else
                    anim.gameObject.SetActive(true);
                manager.playState = PlayState.PLAY_BATTING_VIEW_INFO;
                pitcher.setPitch();

                //투수교체
                if (manager.bPitcherChangeFlag == true)
                {
                    manager.pitcherChangeSetting();
                    yield break;
                }
                //타자교체
                if (manager.bBatterChangeFlag == true)
                {
                    manager.batterChangeSetting();
                    yield break;
                }
                
                ControlManager.SetInfoUI(); 
                IngameUI.GetPlayerInfo().SetActive(true, true, false);
                IngameUI.GetScoreBoard().BoardUpdate();//manager.gameUI.boardUpdate(); //[UI]스코어 보드 업데이트                
            }
            
            
            if (Mode.cameraView == CameraView.PitcherCenter)
            {
                //battingview.transform.localScale = Vector3.one;
                battingview.transform.localScale = new Vector3(0.95f, 0.95f, 1);
            }
            else //if (Mode.cameraView == CameraView.BatterLow)
            {
                battingview.transform.localScale = new Vector3(0.9f, 0.9f, 1);
            }

            readyAnim(true);
        }


        /////////////////////////////////////////////////////////////////
        //시뮬레이션 엔진과의 연계
        /////////////////////////////////////////////////////////////////
        //시뮬엔진으로부터 타석의 결과와 디테일을 얻어옴
        //오토모드와 오토모드가 아닐경우 호출되는 영역이 다르다
        //오토모드는 initBatter에서 호출
        //수동모든는 wait -> looking 상태로 들어가면서 호출
        public void simulateBattingOnly()
        {
            /////////////////////////////////////////////////////////////////////////
            //스텝 타구 자체만 계산하는 시뮬로직
            SimulManager.SimulationBattingOnly();
            manager.battingResultData = SimulManager.GetBattingResult();
            /////////////////////////////////////////////////////////////////////////
            
            /*if (manager.battingResultData.result == SimulResultState.NONE) //원래 이부분 있었는데 삼진이 너무 많이 발생하는거 같아서 지움
            {
                manager.battingResultData.result = SimulResultState.StrikeOut;
                manager.battingResultData.curStrikeCount = 2;
            }*/

            //_aiAutoHoolSlice = 0;
            
            
            if (manager.battingResultData.result == SimulResultState.StrikeOut)
            {
                autoModeBatting = AutoModeBatting.StrikeOut;
                manager.battingResultData.curStrikeCount = 2;
                //bPerfectTiming = false;
            }
            else if (manager.battingResultData.result == SimulResultState.FourBall)
            {
                autoModeBatting = AutoModeBatting.BaseOnBall;
                manager.battingResultData.curBallCount = 3;
                //bPerfectTiming = false;
            }

            //번트신공 처리
            if (field.run.getHowManyRunners() == 0 && pBatter.checkSkillInvoke(SkillIndex.GodOfBunt) == true)
            {
                manager.battingResultData.hitType = SimulHitType.Bunt;                
                manager.battingResultData.buntResultType = SpecificBuntType.DRAG_SUCCESS;
                buntFielder = MyMath.Half() ? CPlayer._PITCHER : CPlayer._THIRDBASEMAN;
                buntType = buntTypePre = SimulBuntType.DRAG;
            }

        }

        //시뮬레이션 후 카운트를 시뮬레이션 결과에 맞게 업데이트 해줌
        public void updateCountBySimulation()
        {
            manager.nStrikeCount = manager.battingResultData.curStrikeCount;
            manager.nBallCount = manager.battingResultData.curBallCount;
            manager.nCurPitcherPitchNum[manager.defenseIndex] += manager.battingResultData.pitchNum;

            IngameUI.GetScoreBoard().BoardUpdate();
        }

#if _RewindMode
        /////////////////////////////////////////////////////////////////
        //리와인드 데이터와의 연계
        /////////////////////////////////////////////////////////////////

        //리와인드 데이터로부터 타석의 결과와 디테일을 얻어옴
        //즉 리와인드 모드의 simulateBattingOnly()함수 역활
        int count = 0;
        public void rewindBattingOnly()
        {   
            ///////////////////////////////////////////////////////////////////////////////////////////
            //리스트에 저장된 리와인드 데이터를 불러온다.
            manager.battingResultData = SimulManager.GetRewindBattingData();
            ////UnityEngine.//Debug.Log("===============>>>> 결과 : " + manager.battingResultData.result);
            ///////////////////////////////////////////////////////////////////////////////////////////

            //bool bPerfectTiming = false;
            _aiAutoHoolSlice = 0;
       

            for (int i = 0; i < 4; i++) //UnityEngine.Debug.Log("##############################===============>>>> runnerCurPos : " + manager.battingResultData.runnerCurPos[i] + " /  runnerValue = " + manager.battingResultData.runnerValue[i]);                

            if (manager.battingResultData.result == SimulResultState.StrikeOut)
            {
                bNeverContact = true;
                //bPerfectTiming = false;
            }
            else
            {
                if (manager.battingResultData.hitType == SimulHitType.Grounder)
                {
                    ////UnityEngine.//Debug.Log("========================================>>땅볼 타입 = " + manager.battingResultData.grounderType);
                    if (manager.battingResultData.grounderType == SpecificGrounderType.Solid ||
                        manager.battingResultData.grounderType == SpecificGrounderType.VerySolid)
                    {
                        //manager.preHitBallType = HITBALLTYPE._GROUNDER;
                        field.ball.cameraWork = CameraWork.Shadow_Chase;
                    }
                    else
                    {
                        //manager.preHitBallType = HITBALLTYPE._PPICSAL;
                        field.ball.cameraWork = CameraWork.Default;
                    }

                }
                else if (manager.battingResultData.hitType == SimulHitType.Fly)
                {
                    ////UnityEngine.//Debug.Log("========================================>>플라이 타입 = " + manager.battingResultData.flyType);
                    if (manager.battingResultData.flyType == SpecificFlyType.OutfieldHomerun)
                    {
                        //manager.preHitBallType = HITBALLTYPE._HOMERUN;
                        //bPerfectTiming = true;
                        field.ball.cameraWork = CameraWork.Ball_Chase;
                    }
                    else if (manager.battingResultData.flyType == SpecificFlyType.OutfieldOverHead)
                    {
                        //manager.preHitBallType = HITBALLTYPE._STRONG_FLY;
                        //bPerfectTiming = true;
                        field.ball.cameraWork = CameraWork.Ball_Chase;
                    }
                    else if (manager.battingResultData.flyType == SpecificFlyType.OutfieldHighFly ||
                            manager.battingResultData.flyType == SpecificFlyType.OutfieldShort)
                    {
                        //manager.preHitBallType = HITBALLTYPE._FLYBALL;
                        field.ball.cameraWork = CameraWork.Ball_Chase;
                    }
                    else
                    {
                        //manager.preHitBallType = HITBALLTYPE._POPUP;
                        field.ball.cameraWork = CameraWork.Popup;
                    }

                }
                else if (manager.battingResultData.hitType == SimulHitType.Liner)
                {
                    //////UnityEngine.//Debug.Log("========================================>>라이너 타입 = " + manager.battingResultData.linerType);
                    if (manager.battingResultData.linerType == SpecificLinerType.Solid ||
                       manager.battingResultData.linerType == SpecificLinerType.VerySolid)
                    {
                        //manager.preHitBallType = HITBALLTYPE._LINEDRIVE;
                        //bPerfectTiming = true;
                        field.ball.cameraWork = CameraWork.Ball_Chase;
                    }
                    else
                    {
                        //manager.preHitBallType = HITBALLTYPE._PPICSAL;
                        field.ball.cameraWork = CameraWork.Default;
                    }

                }
            }
           
            //요놈을 나중에 수정
            _aiAutoPower = BattingMechanism.GetBallPowerAuto(manager.battingResultData);
            _aiAutoAngle = BattingMechanism.GetBallAngleAuto(manager.battingResultData);
            _aiAutoDir = BattingMechanism.GetBallAngleDir(manager.battingResultData);
        }

#endif

        /////////////////////////////////////////////////////////////////
        //타자와 관련된 상태를 업데이트 해주는 함수
        /////////////////////////////////////////////////////////////////
        
        //매구 타자의 상태를 업데이트 해주는 함수
        public bool bBatterFieldUpdate;
        public void setBatter()
        {
            //strID = "";

            //컬러 초기화
            setColor(new Color(1, 1, 1, 1));

            bCheckSwinged = false;
            bCheckSwing = false;
            bDecideSwing = false;
            aiHitandRunDecide = (MyMath.Percent() < 90 ? false : true); //AI의 치고 달리기
            bHitGood = false;
            bAiCheck = false;
            bBunt = false;
            bBuntHit = false;
            bHitted = false;
            bHitChecked = false;
            bSwing = false;
            bForcedSwingPrevent = false;
            bReleaseCheck = false;
            aiCheckStep = CHECK_WAIT;
            bPreHitCheck = false;
            bTipped = false;
            bState = BatterState._WAITING;
            contact = BattingContact.NO_SWING;
            bBadballSwingLow = bBadballSwingHigh = bBadballSwingFar = bBadballSwingNear = false;
            bFarBatting = bNearBatting = bHighBatting = bLowBatting = false;
            bMustCheckSwing = false;

            zoneUI.setBatCursorPos(0, 0, 115, 146);//CURSOR_MAX_X, CURSOR_MAX_Y);

            cursorDX = cursorDY = 0;
            powerCoef1 = 1.0f;
            powerCoef2 = 1.0f;
            powerCoef3 = 1.0f;

            
            //선구안 영역
            bArrivePointGuess = true;            
            

            //1차로 타자 능력치 세팅
            setBatterFinalSetting();
            //타구 퀄리티 미리 세팅
            setTando();
            setBattingValue();
            setDirection();

            //초구 관련 특능 설정
            //-->여기에

            //투스트라이크 관련 디버프 특능 설정
            //-->여기에

            //쓰리볼 관련 버프 특능 설정
            //-->여기에

            //풀 카운트 관련 버프 디버프 특능 설정
            //-->여기에


            //스윙 애니메이션 초기화
            bool bReadyInit = true;
            if((manager.nStrikeCount == 0 && manager.nBallCount == 0) || bBatterFieldUpdate == true)
            {
                //해당 경우 스윙 애니메이션 초기화
                bReadyInit = false;
            }

            initSwingAnim(bReadyInit);

            bBatterFieldUpdate = false;

            zoneUI.setBatCursorActive(true);//

            if (bPvState == true)
            {
                pitcher.pitchPv.battingSystemPv.initPosition();
            }
            else
            {
                //처리됨
                pitcher.pitch.battingSystem.initPosition();
            }

            field.b2DBattingSystem = false;
            //field.bSqueezeFlagOn = false;

                        
            //직접 플레이 처리
            if (Mode.bAutoPlay == false)
            {
                //자동 플레이시 해당 사항 없음
                if (manager.bMyTurn == false)
                {
                    //ai 공격인 경우 배트커서 사라져
                    zoneUI.setBatCursorActive(false);//
                }
            }
            
            //오토 플레이 혹은 CPU타격 플레이시 모든 번트작전 포기
            if (Mode.bAutoPlay == true || manager.bMyTurn == false)
            {                
                if (manager.nStrikeCount == 2)
                {
                    //모든 번트 작전 취소
                    ////UnityEngine.//Debug.Log("===============================>>buntType = " + buntType);
                    buntType = SimulBuntType.NONE;
                    buntResult = SpecificBuntType.NONE;
                }
            }

            

            setBatRealSize();
            IngameUI.GetControlRunner().UpdateState();

        }
        

        //타자가 공을 맞춘후 관련 상태를 세팅한다.
        public void setHit()
        {
            ControlBattingUI.SetActive(false, manager); //manager.gameUI.battingUI.GetComponent<battingUI>()._active.SetActive(false); //[UI]배팅UI를 디액티브
            pitcher.setPitchSystemDraw(false);
            ControlManager.EraseBattingUI();//manager.gameUI.eraseBattingUI(); //[UI]배팅UI를 디액티브            
            field.run.setRunnerCamera(false);

            field.ball.step = BallStep.BALL_HIT;
            bHitted = true;
            bBuntHit = false;
                        
            pitcher.setBattingSystem();

            if (manager.batterSkillFlag != SkillFlag.None)
            {
                if (manager.batterSkillFlag != SkillFlag.GodOfBunt)
                {
                    SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Batting, false);
                }
                else
                {
                    manager.batterSkillFlag = SkillFlag.None;
                }
            }

            /*
            if (bFirstBallOn == true)
            {
                //StartCoroutine(loadPassiveSkill("bskill_skill" + SkillParm.CHOGU, 0.1f, 0));
                bFirstBallOn = false;
            }

            if (bWideAngleOn == true)
            {
                if (field.ball.firstAngle > 35 || field.ball.firstAngle < -35)
                {
                    StartCoroutine(loadPassiveSkill("bskill_skill" + SkillParm.WIDE_ANGLE, 0.1f, 0));
                }
                bWideAngleOn = false;
            }

            if (bSpecialBattingOn == true)
            {
                if (specialHitType == SpecialHitType.YasuAttack)
                {
                    StartCoroutine(loadPassiveSkill("bactive_skill6", 0.1f, 0,0.84f));
                }
                else if (specialHitType == SpecialHitType.DropBatting)
                {
                    StartCoroutine(loadPassiveSkill("bactive_skill2", 0.1f, 0, 0.84f));
                }
                else if (specialHitType == SpecialHitType.FenceAttack)
                {
                    StartCoroutine(loadPassiveSkill("bactive_skill7", 0.1f, 0, 0.84f));
                }
            }*/

        }

        //타자가 번트를 댄 후 관련 상태를 세팅한다
        public void setBunt()
        {
            ControlBattingUI.SetActive(false, manager); //manager.gameUI.battingUI.GetComponent<battingUI>()._active.SetActive(false);//[UI]배팅UI를 디액티브
            pitcher.setPitchSystemDraw(false);
            ControlManager.EraseBattingUI();//manager.gameUI.eraseBattingUI(); //[UI]배팅UI를 디액티브
            field.run.setRunnerCamera(false);
            IngameUI.GetPitchUI().SetPitchCursor(false, 0, 0);
            IngameUI.GetPitchUI().SetActive(false);
            if (bPvState == false)
            {
                HitEffect.SetHitEffect("SMALL_HIT", effectX, 0, 150);
            }
            else
            {
                setHitEffectPv(true, false);
            }

            field.ball.step = BallStep.BALL_HIT;
            bHitted = true;
            bBuntHit = true;
            field.setHitVector(true);

            if (manager.batterSkillFlag == SkillFlag.GodOfBunt)
            {
                if (manager.bMyTurn == false)
                {
                    //SkillEffectDisplayManager.EffectDisplay(SkillEffectDisplayManager.DisplayStep.Batting, false);
#if _Test_Local
                    int rank = 3;
#else
                    int rank = SimulManager.GetBatterSkill().rank;
#endif
                    IngameUI.GetCpuSkillUI().init((int)SkillID.bunt_sin, rank);
                }
            }
            else
            {
                manager.batterSkillFlag = SkillFlag.None;
            }
            
            //이거
            Invoke("buntFieldState", 0.5f);
        }

        //
        private void buntFieldState()
        {
            //이거
            field.setFieldHitState();
            field.bFieldViewActive = true;
            field.fieldState = FieldState.NORMAL_FIELD;
            manager.changeFieldView(field.fieldState);
            field.ball.setCameraPosInitAfterHit();
        }

                
        //타자의 기록 관련 상태를 세팅한다.
        public void addRecord(int type, int num = 1)
        {
            if (pBatter != null)
            {
                ////UnityEngine.Debug.Log("타자 기록===============>>> " + pBatter.getName() + "의 " + Param.debug_stat[type] + " 가산");
                pBatter.setRecord(type, num);
            }
        }

        //타격후 이펙트를 처리해주는 함수
        public void setHitEffect(bool bActive, bool bBroken = false, bool earlyTiming = false)
        {
            //////UnityEngine.//Debug.Log("===============================>> !!!!!!!!!!!!!!!!!!!!!!!!!!! setHitEffect");
            if (bActive == true)
            {
                if (earlyTiming == true || bBroken == true)
                {
                    HitEffect.SetHitEffect("SMALL_HIT", effectX, effectY, 150);// 100);//, 2.5f);                    
                }
                else
                {
                    if (bHitGood == true)
                    {
                        ballImage.gameObject.SetActive(true);
                    }

                    if (bHitHomeRun == true) //if (type == HITBALLTYPE._HOMERUN)
                    {
                        //홈런타
                        HitEffect.SetHitEffect("HOMERUN_HIT", effectX, effectY, 130);    //100                     
                    }
                    else
                    {
                        HitEffect.SetHitEffect("MIDDLE_HIT", effectX, effectY, 70);  //100
                    }
                }
            }
            else
            {
                ballImage.gameObject.SetActive(false);
                if (bHitHomeRun == true)
                {
                    //CameraManager.SetScreenOverlay(false);                    
                    bHitHomeRun = false;
                }
            }
        }
        
        
        //체크스윙을 세팅
        public void setCheckSwing(int finalGuwee)
        {
            //433버전에서는 체크스윙 없음
            /*if (manager.bMyTurn == false && Mode.bPvpMode == true)
            {
                //미리 세팅됨 -> 아래는 랜덤시드 동일화 하기 위한꼼수
                int range = Random.Range(0, finalGuwee * 2);
            }*/
            /*if (manager.bMyTurn == false && Mode.bPvpMode433 == true)
            {
                
            }
            else
            {
                //체크스윙 세팅
                //bCheckSwing = BattingMechanism.checkSwing(finalGuwee, bEye); 
            }*/ //체크스윙 임시 없앰
        }

        /////////////////////////////////////////////////////////////////
        //컨택과 타이밍
        /////////////////////////////////////////////////////////////////

        //공과 배트의 충돌에 관련된 여러가지 조건들을 체크하는 함수 - (스윙과 같은 노멀한 케이스)
        //진짜
        private void checkNormalHit()
        {
            if (pitcher.bRelease == true)
            {
                if (bHitChecked == false)
                {
                    if (hitCheck())
                    {
                        //timingAdjustPower();
                        bHitChecked = true;
                        bPreHitCheck = true;
                        run.bStealBase = false;
                    }
                    /*else
                    {                        
                        if (Mode.bPvpMode433 == true)
                        {
                            if (manager.bMyTurn == true)
                            {
                                if (bCheckSwingActivate == true)
                                {
                                    Debug.Log("체크 스윙 정보 송신");
                                    pvpmanager.Get().SendNoHitInfo(manager, NoHitStatus.CheckSwing);
                                }
                                else
                                { 
                                    Debug.Log("일반 헛스윙 정보 송신");
                                    pvpmanager.Get().SendNoHitInfo(manager, NoHitStatus.HutSwing);
                                }
                            }
                        }
                    }*/
                }
            }
        }

        ///테스트용
        /*private void checkNormalHit()
        {
            if (pitcher.bRelease == true)
            {
                if (bHitChecked == false)
                {
                    if (hitCheck())
                    {
                        //UnityEngine.//Debug.Log("======================================>>타자 RAW 타이밍 : " + timing);
                        //timingAdjust();
                        //UnityEngine.//Debug.Log("======================================>>perfectTimingGabRate = " + (perfectTimingGabRate * batterPerfectTime));
                        bHitChecked = true;
                        bPreHitCheck = true;
                        run.bStealBase = false;
                        //////////UnityEngine.//Debug.Log("=================++++++++++++++++++++++++++++++++++++++++++++++++++++++>> hitCheck()");
                        ////UnityEngine.//Debug.Log("======================================>>타자 타이밍 : " + timing);
                        ////UnityEngine.//Debug.Log("======================================>>타자 컨택 : " + contact);
                    }
                }
            }
        }*/

        //공과 배트의 충돌에 관련된 여러가지 조건들을 체크하는 함수 - (번트 케이스)
        private void checkBuntHit()
        {
            //번트 충돌 체크
            if (pitcher.bRelease == true)
            {
                if (bHitChecked == false)
                {
                    if (bBunt == true)
                    {
                        float hitRate = pitcher.getHitRate();
                        if (hitRate >= (_PERFECT_FAST))
                        {
                            bool bHit = false;
                            
                            if (Mode.bPvpMode433 == true && manager.bMyTurn == false)
                            {
                                bHit = manager.Pvp_bBuntContact;
                            }
                            else
                            {
                                float px = pitcher.preArriveX;
                                float py = pitcher.preArriveY;
                                if (Mathf.Abs(px) < (Zone.UI_ZONE_WIDTH + 15) && Mathf.Abs(py) < (Zone.UI_ZONE_HEIGHT + 15))
                                {
                                    bHit = true;
                                }
                            }

                            if(bHit == true)
                            {
                                /*
                                //파울팁 제거
                                if (Mode.bAutoPlay == false && manager.bMyTurn == false 
                                && (MyMath.Percent() < BattingMechanism.AI_BUNT_CONTACT_FAIL))
                                {
                                    //AI플레이어의 번트실패 (실플레이에서만 적용)
                                    battingOffsetY = Random.Range(-15, 30);
                                    bHitChecked = true;
                                    bBuntHit = true;
                                    contact = BattingContact.TIP;
                                    StartCoroutine(tipCheckDelay(0.1f));
                                }
                                else*/
                                {
                                    //////UnityEngine.//Debug.Log("=================++++++++++++++++++++++++++++++++++++++++++++++++++++++>> checkBuntHit() : true");
                                    if (buntTypePre != SimulBuntType.NONE)
                                    {
                                        buntType = buntTypePre;
                                    }

                                    //Debug.Log("CheckBuntHit=======================>>buntType = " + buntType);
                                    //Debug.Log("CheckBuntHit=======================>>buntTypePre = " + buntTypePre);
                                    field.bSqueezeFieldOut = MyMath.Half();

                                    //번트 타구 방향은 실시간 외에 답이 없다.!
                                    int range = MyMath.Percent();
                                    if (range < 70)
                                    {
                                        buntFielder = (buntDir == -1 ? CPlayer._FIRSTBASEMAN : CPlayer._THIRDBASEMAN);
                                    }
                                    else if (range > 93)
                                    {
                                        buntFielder = CPlayer._CATCHER;
                                    }
                                    else
                                    {
                                        buntFielder = CPlayer._PITCHER;
                                    }

                                    if (buntFielder == CPlayer._CATCHER && (buntType == SimulBuntType.DRAG || buntType == SimulBuntType.SQUEEZE))
                                    {
                                        //캐처시 예외처리
                                        buntFielder = CPlayer._THIRDBASEMAN;
                                    }

                                    setBunt();
                                    StartCoroutine(buntCheckDelay());
                                    bHitChecked = true;
                                    run.bStealBase = false;
                                }
                            }
                            else
                            {
                                
                                if (Mode.bPvpMode433 == true)
                                {
                                    if (manager.bMyTurn == true)
                                    {
                                        //번트 헛스윙 정보
                                        Debug.Log("번트 헛스윙 정보 발신");
                                        pvpmanager.Get().SendNoHitInfo(manager, NoHitStatus.BuntSwing);
                                    }
                                }
                                bSwing = true;
                                bHitChecked = true;
                            }
                            //파울팁 제거
                            /*else
                            {
                                if (Mode.bAutoPlay == false)
                                {
                                    float px = pitcher.preArriveX;
                                    float py = pitcher.preArriveY;
                                    //파울팁 체크
                                    if (Mathf.Abs(px) < (Zone.UI_ZONE_WIDTH + 20) && Mathf.Abs(py) < (Zone.UI_ZONE_HEIGHT + 20))
                                    {
                                        battingOffsetY = Random.Range(-15, 30);
                                        bHitChecked = true;
                                        bBuntHit = true;
                                        contact = BattingContact.TIP;
                                        StartCoroutine(tipCheckDelay(0.1f));
                                    }
                                }
                            }*/
                        }
                        else
                        {
                            if (Mode.bPvpMode433 == false)//if (Mode.bPvpMode == false)
                            {
                                //일반모드에서 택돌이 번트 AI가 안하는루틴 - PVP해당사항 없음
                                if (Mode.bAutoPlay == true || manager.bMyTurn == false)
                                {
                                    if (hitRate >= (_PERFECT_FAST - 0.3f))
                                    {
                                        if (bStrikeCheck == false && buntType != SimulBuntType.SQUEEZE)
                                        {
                                            readyAnim(false);// lookingAnim(true);
                                            bState = BatterState._BUNT_BACK;
                                            bBunt = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void setBatterFinalSetting()
        {
            int bonus = pBatter.getBonusValue();
            bEye = getEyeValue() + bonus;
            bContact = getContactValue() + bonus;
            bPower = getPowerValue() + bonus;
            bTando = getTandoValue() + bonus;
#if _Test_Local
            //Debug_UI.SetBatter(pBatter);
#endif
        }


        public void setPreTimingAndContact()
        {
            //배팅시스템
            if (Mode.bAutoPlay == true || manager.bMyTurn == false)
            {
                if (Mode.bAutoPlay == true && autoModeBatting != AutoModeBatting.Normal)
                {
                    //Debug.Log("===========================>>> 오토모드 예외 사항 컨택 타이밍 계산");
                    if (autoModeBatting == AutoModeBatting.StrikeOut && MyMath.Percent() < 75)
                    {
                        autoModeSwing = true;
                        aiContactPoint = BattingContact.HUT_SWING;
                        aiTimingPoint = MyMath.Half() ? BattingTiming.VERY_EARLY : BattingTiming.VERY_LATE;                        
                    }
                    else
                    {
                        autoModeSwing = false;
                        aiContactPoint = BattingContact.NO_SWING;
                        aiTimingPoint = BattingTiming.NOSWING;
                    }

                    //이다음에 타석이 계속된다면 정상으로 돌림
                    autoModeBatting = AutoModeBatting.Normal;

                }
                else
                {
                    autoModeSwing = true;
                    ////Debug.Log("===========================>>> 일반적인 오토모드 혹은 AI타격 컨택 타이밍 계산");
                    //AI 컨택점
                    aiContactPoint = BattingMechanism.getConctactResult(Mode.bAutoPlay, bStrikeCheck, 
                                                                        pitcher.pFinalGuwee,            //투수구위
                                                                        bContact);                      //타자컨택

                    ////Debug.Log("===========================>>> AI 컨택점 aiContactPoint = " + aiContactPoint);

                    bool bFastBall = (pitcher.fastballCon >= 0 ? true : false);
                    aiTimingPoint = BattingMechanism.getTimingResult(Mode.bAutoPlay, bFastBall, 
                                                                     pitcher.pFinalGuwee,
                                                                     bEye);

                    ////Debug.Log("===========================>>> AI 타이밍점 aiTimingPoint = " + aiTimingPoint);
                    float value = Mathf.Clamp(10 + ((int)aiContactPoint * 10), 10.0f, 40.0f);
                    ////Debug.Log("===========================>>> 최대 미스점 = " + value);

                    pitcher.aiMissOffsetX = Random.Range(-value, value);
                    pitcher.aiMissOffsetY = Random.Range(-value, value);

                    //밸런스 매칭 시스템
                    int offenseScoreGab = manager.offenseWinningGab();
                    ////Debug.Log("===========================>>> offenseScoreGab = " + offenseScoreGab);
                    if (offenseScoreGab > 2)
                    {
                        //공격자가 이기고 있는경우 공격력을 다운 시킴
                        //-1~0
                        timingAddPoint = Random.Range(-1, 1);   //끝에 랜덤요소 섞음
                        contactAddPoint = Random.Range(-1, 1);  //끝에 랜덤요소 섞음
                    }
                    else if (offenseScoreGab < 2)
                    {
                        //공격팀이 지고 있는경우 공격력을 업 시킴
                        //0~1
                        timingAddPoint = Random.Range(0, 2);   //끝에 랜덤요소 섞음
                        contactAddPoint = Random.Range(0, 2);  //끝에 랜덤요소 섞음
                    }
                    else
                    {
                        /*
                        if (Mode.bAutoPlay == true)
                        {
                            //오토시 공평
                            //-1 ~ 1
                            timingAddPoint = Random.Range(-1, 2);   //끝에 랜덤요소 섞음
                            contactAddPoint = Random.Range(-1, 2);  //끝에 랜덤요소 섞음
                        }
                        else*/
                        {
                            //직접플레이시 CPU잇점 -> 폐기되고 공평하게 갈수도 있음
                            //0 ~ 1 
                            timingAddPoint = Random.Range(0, 2);   //끝에 랜덤요소 섞음
                            contactAddPoint = Random.Range(0, 2);  //끝에 랜덤요소 섞음
                        }
                    }
                    ////Debug.Log("===========================>>> AI contactAddPoint = " + contactAddPoint);
                    ////Debug.Log("===========================>>> AI timingAddPoint = " + timingAddPoint);
                }
            }
            else
            {
                //Debug.Log("===========================>>> 직접조작시 타이밍과 컨택 보정");
                pitcher.aiMissOffsetX = pitcher.aiMissOffsetY = 0;

                /*if (Mode.bPvpMode == true)
                {
                    ////Debug.Log("===========================>>> PVP 찬스모드 타격 보너스 포인트 연산");
                    timingAddPoint = Random.Range(1,3);
                    contactAddPoint = Random.Range(1,3);
                }
                else*/
                {
                    if (Mode.bOnlyChanceMode == true && manager.bMyTurn == true)
                    {
                        ////Debug.Log("===========================>>> 일반 찬스모드 타격 보너스 포인트 연산");
                        timingAddPoint = Random.Range(1, 2);
                        contactAddPoint = Random.Range(1, 2);
                    }
                    else
                    {
                        ////Debug.Log("===========================>>> 기본 타격 보너스 포인트 연산");
                        //조작 시스템 -> 능력치에 따른 add 포인트
                        timingAddPoint = BattingMechanism.getAddValue(pitcher.pFinalGuwee, bEye);     //선구+파워 vs 최종구위 // 양수면 좋음, 음수면 나쁨
                        contactAddPoint = BattingMechanism.getAddValue(pitcher.pFinalGuwee, bContact);    //컨택 vs 최종구위
                    }
                }

            }
          
        }
        

        //컨택 여부와 컨택 밸류를 얻어오는 함수 (오토모드 관련 체크해줌)       
        private bool contactCheck()
        {
            float r;// = MyMath.getEllipseEquation(battingOffsetX, battingOffsetY, batRealSizeX, batRealSizeY);

            if (bGangTa == false)
            {
                //일반타
                r = MyMath.getEllipseEquation(battingOffsetX, battingOffsetY, batRealSizeX, batRealSizeY);
                ////Debug.Log("========================>> contact R = " + r + "    일반타 = " + bGangTa);
            }
            else
            {
                //강타
                r = MyMath.getEllipseEquation(battingOffsetX, battingOffsetY, batGangtaSize, batGangtaSize);
                ////Debug.Log("========================>> contact R = " + r + "    강타 = " + bGangTa);
            }

            if (r < 1) //커서안에 들어가는 경우
            {                
                if (r < BattingMechanism.PERFECT_CONTACT_COEF)
                {
                    powerCoef3 = (bGangTa == false ? 1.0f : 1.1f);
                    contact = BattingContact.SOLID;
                    return true;
                }
                else if (r < BattingMechanism.GOOD_CONTACT_COEF)/// 0.15f)
                {
                    powerCoef3 = (bGangTa == false ? 1.0f : 1.025f);
                    contact = BattingContact.GOOD;
                    return true;
                }
                else if (r < BattingMechanism.NORMAL_CONTACT_COEF)/// 0.4f)
                {
                    powerCoef3 = (bGangTa == false ? 1.0f : 0.9f);
                    contact = BattingContact.NORMAL;
                    return true;
                }
                else
                {
                    //팁 없앰
                    powerCoef3 = (bGangTa == false ? 1.0f : 0.7f);
                    contact = (battingOffsetX > 0 ? BattingContact.BAD : BattingContact.JAMMED);
                    return true;
                }
            }

            contact = BattingContact.HUT_SWING;
            return false;

        }



        //타이밍 밸류를 얻어오는 함수
        //float swingAnimScale;
        private bool timingCheck(float hitRate)
        {
            if (hitRate >= (_PERFECT_FAST) && hitRate <= (_PERFECT_LATE))
            {
                //퍼펙트
                timing = BattingTiming.PERFECT;
                ////UnityEngine.//Debug.Log("====================>>배트 타이밍 :" + timing);
                return true;
            }
            else
            {
                if (hitRate < (_PERFECT_FAST))
                {
                    if (hitRate > ((_PERFECT_FAST - justEarlyGab)))
                    {
                        //just early
                        timing = BattingTiming.JUST_EARLY;
                        ////UnityEngine.//Debug.Log("====================>>배트 타이밍 :" + timing);
                        return true;
                    }
                    else if (hitRate > ((_PERFECT_FAST - earlyGab)))
                    {
                        //early
                        timing = BattingTiming.EARLY;
                        ////UnityEngine.//Debug.Log("====================>>배트 타이밍 :" + timing);
                        return true;
                    }
                    else
                    {
                        //very early
                        timing = BattingTiming.VERY_EARLY;
                        ////UnityEngine.//Debug.Log("====================>>배트 타이밍 :" + timing);
                        contact = BattingContact.HUT_SWING;
                        //swingAnimScale = 1.0f;
                        //return false;
                    }
                }
                else //if (hitRate > (_PERFECT_LATE))
                {
                    if (hitRate < ((_PERFECT_LATE + justLateGab)))
                    {
                        timing = BattingTiming.JUST_LATE;
                        ////UnityEngine.//Debug.Log("====================>>배트 타이밍 :" + timing);
                        return true;
                    }
                    else if (hitRate < ((_PERFECT_LATE + lateGab)))
                    {
                        timing = BattingTiming.LATE;
                        ////UnityEngine.//Debug.Log("====================>>배트 타이밍 :" + timing);
                        return true;
                    }
                    else
                    {
                        timing = BattingTiming.TOO_LATE;
                        ////UnityEngine.//Debug.Log("====================>>배트 타이밍 :" + timing);
                        contact = BattingContact.HUT_SWING;
                        //swingAnimScale = 1.0f;
                        //return false;
                    }
                }
            }
            
            return false;
        }


        //AI 타이밍 밸류를 얻어오는 함수
        private bool timingCheckAI(BattingTiming curTiming)
        {
            //swingAnimScale = 1.1f;// 1.2f;  //원래 1

                if (curTiming == BattingTiming.VERY_EARLY || curTiming == BattingTiming.VERY_LATE)
                {
                    //////UnityEngine.//Debug.Log("===========================>> AI 타이밍 못맞춤");
                    return false;
                }
                else
                {
                    //////UnityEngine.//Debug.Log("===========================>> AI 타이밍 맞춤 = "+curTiming);
                    return true;
                }

            
        }
        public float userHitRate;

        //pvp 헛스윙용 타이밍 얻어오기
        public BattingTiming timingCheckPVP()
        {
            if (userHitRate >= (_PERFECT_FAST) && userHitRate <= (_PERFECT_LATE))
            {
                //퍼펙트
                return BattingTiming.PERFECT;
            }
            else
            {
                if (userHitRate < (_PERFECT_FAST))
                {
                    if (userHitRate > ((_PERFECT_FAST - justEarlyGab)))
                    {
                        //just early
                        return BattingTiming.JUST_EARLY;
                    }
                    else if (userHitRate > ((_PERFECT_FAST - earlyGab)))
                    {
                        //early
                        return BattingTiming.EARLY;
                    }
                    else
                    {
                        //very early
                        return BattingTiming.VERY_EARLY;
                    }
                }
                else //if (hitRate > (_PERFECT_LATE))
                {
                    if (userHitRate < ((_PERFECT_LATE + justLateGab)))
                    {
                        return BattingTiming.JUST_LATE;
                    }
                    else if (userHitRate < ((_PERFECT_LATE + lateGab)))
                    {
                        return BattingTiming.LATE;
                    }
                    else
                    {
                        return BattingTiming.TOO_LATE;
                    }
                }
            }

            //return BattingTiming.PERFECT;
        }


        //공과 배트의 충돌영역 체크
        public bool hitCheck()
        {
            float hitRate = pitcher.getHitRate();
            userHitRate = hitRate;

            float px = pitcher.preArriveX;
            float py = pitcher.preArriveY;

            bStrkeSwing = true;
            float absX = Mathf.Abs(px);
            float absY = Mathf.Abs(py);


            if (absX > Zone.UI_ZONE_WIDTH)
            {
                //볼을 치는 경우 체크
                bStrkeSwing = false;
                if ((px * sign) > 0)
                {
                    //바깥쪽 나쁜공
                    bBadballSwingFar = true;
                }
                else
                {
                    //안쪽 나쁜공
                    bBadballSwingNear = true;
                }
            }
            else
            {
                if (absX > Zone.UI_ZONE_WIDTHC)
                {
                    if ((px * sign) > 0)
                    {
                        //바깥쪽
                        bFarBatting = true;
                    }
                    else
                    {
                        //안쪽공
                        bNearBatting = true;
                    }
                }
            }


            if (absY > Zone.UI_ZONE_HEIGHT)
            {
                bStrkeSwing = false;
                if ((py) > 0)
                {
                    //위쪽 나쁜공
                    bBadballSwingHigh = true;
                }
                else
                {
                    //아래쪽 나쁜공
                    bBadballSwingLow = true;
                }
            }
            else
            {
                if (absY > Zone.UI_ZONE_HEIGHTC)
                {
                    if ((py) > 0)
                    {
                        //위쪽공
                        bHighBatting = true;
                    }
                    else
                    {
                        //아래공
                        bLowBatting = true;
                    }
                }
            }


            battingOffsetX = sign * (px - cursorX);
            battingOffsetY = (py - cursorY);

            if (manager.pitcherSkillFlag == SkillFlag.Charm)
            {
                //매혹 처리 -> 헛스윙
                manager.pitchSkillPitcherWin(); //일부 타자 스킬 무효화 처리
                manager.pitcherSkillFlag = SkillFlag.None;
                contact = BattingContact.HUT_SWING;
                return false;
            }
            else
            {
                if (Mode.bAutoPlay == true || manager.bMyTurn == false)
                {
                    //////UnityEngine.//Debug.Log("=============================================>>hitCheck HIT CHECK!!");
                    contact = aiContactPoint;// preContact;     
                    //Debug.Log("eeeeeeeeeeeeeeeee  contact =" + contact);
                    if (contact != BattingContact.HUT_SWING)
                    {
                        if (checkSwingCondition() == true)
                        {
                            return false;
                        }
                        else
                        {
                            //컨택조정
                            //Debug.Log("fffffffffff  timing =" + timing);
                            adjustContact();
                            setBattingMissOffset(px, py);
                            perfectTimingGabRate = (_PERFECT_TIMING - hitRate);
                            bool bHit = timingCheckAI(timing);
                            if (bHit == true)
                            {
                                //타이밍 조정
                                adjustTiming();
                            }
                            return bHit;
                        }
                    }
                }
                else
                {
                    if (contactCheck() == true)
                    {
                        if (checkSwingCondition() == true)
                        {
                            return false;
                        }
                        else
                        {
                            ////Debug.Log("======================>> 원래 컨택 contact = " + contact + "=============>> contactAddPoint = " + contactAddPoint);
                            //컨택 조정
                            adjustContact();
                            setBattingMissOffset(px, py); //다시 만들어 보자
                            perfectTimingGabRate = (_PERFECT_TIMING - hitRate);
                            if (manager.batterSkillFlag == SkillFlag.FalconEye)
                            {
                                //매의눈 타이밍 세팅
                                if (hitRate > 0.45f)
                                {
                                    //팰콘아이 타이밍 보정
                                    noTandoEffect = true;
                                    timing = BattingTiming.PERFECT;
                                    timingAddPoint = 2;
                                    adjustTiming();
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                bool bHit = timingCheck(hitRate);
                                //Debug.Log("타이밍 체크");
                                ////Debug.Log("======================>> 원래 타이밍 timing = " + timing + "=============>> timingAddPoint = " + timingAddPoint);
                                //배팅시스템
                                if (bHit == true)
                                {
                                    //타이밍 조정
                                    adjustTiming();
                                }
                                return bHit;
                            }
                        }
                    }
                }
            }
            //체크스윙 체크
            if (checkSwingCondition() == true)
            {
                return false;
            }
            else
            {
                //헛스윙
                contact = BattingContact.HUT_SWING;
                return false;
            }
        }

        /// <summary>
        /// 체크스윙 컨디션
        /// </summary>
        /// <returns></returns>
        private bool checkSwingCondition()
        {
            /* //체크스윙 임시 없앰
            if (bCheckSwing == true || bMustCheckSwing == true)
            {
                if (bStrikeCheck == false && field.bFieldStealFlag == false)
                {
                    //체크스윙
                    bCheckSwingActivate = true;
                    contact = BattingContact.NO_SWING;
                    return true;
                }
            }*/
            return false;
        }

        /// <summary>
        /// contactAddPoint 밸류에 의해 원 contact값을 조정한다
        /// contactAddPoint 값이 양수이고 클수록 contact밸류가 좋아진다
        /// 음수이고 작을수록 contact밸류가 나빠진다.
        /// </summary> //배팅시스템
        private void adjustContact()
        {   
            if (bGangTa == false)
            {
                if (contactAddPoint != 0)
                {
                    contact -= contactAddPoint;
                    if (contact > BattingContact.BAD) contact = BattingContact.BAD;
                    else if (contact < BattingContact.SOLID) contact = BattingContact.SOLID;
                        ////Debug.Log("======================>> 조정 컨택 contact = " + contact);
                }
            }

            powerCoef1 = BattingMechanism.getContactPowerCoef(contact);    //setContactPowerCoef();            
        }

        /*
        private void setContactPowerCoef()
        {
            if (contact == BattingContact.SOLID)
            {
                powerCoef1 = 1.0f;
            }
            else if (contact == BattingContact.GOOD)
            {
                powerCoef1 = Random.Range(0.9f, 0.95f);
            }
            else if (contact == BattingContact.NORMAL)
            {
                powerCoef1 = Random.Range(0.8f, 0.9f);
            }
            else
            {
                powerCoef1 = Random.Range(0.65f, 0.75f);
            }
        }*/

        /// <summary>
        /// timingAddPoint 밸류에 의해 원 timing값을 조정한다
        /// timingAddPoint 값이 양수이고 클수록 timing밸류가 좋아진다
        /// 음수이고 작을수록 timing밸류가 나빠진다.
        /// </summary> //배팅시스템
        private void adjustTiming()
        {
            if (timingAddPoint != 0) // 양수면 좋음, 음수면 나쁨
            {
                if (timing < BattingTiming.PERFECT)
                {
                    //timingAddPoint가 양수이고클수록 퍼펙트에 가까와지고, 음수이고 작을수록 too early에 가까워짐
                    timing += timingAddPoint;
                    if (timing > BattingTiming.PERFECT) timing = BattingTiming.PERFECT;
                    else if (timing < BattingTiming.TOO_EARLY) timing = BattingTiming.TOO_EARLY;
                }
                else
                {
                    //timingAddPoint가 양수이고클수록 퍼펙트에 가까와지고, 음수이고 작을수록 too late에 가까워짐
                    timing -= timingAddPoint;
                    if (timing < BattingTiming.PERFECT) timing = BattingTiming.PERFECT;
                    else if (timing > BattingTiming.TOO_LATE) timing = BattingTiming.TOO_LATE;
                }
                ////Debug.Log("======================>> 조정 타이밍 timing = " + timing);
            }
            powerCoef2 = BattingMechanism.getTimingPowerCoef(timing); //setTimingPowerCoef();
        }

        /*
        private void setTimingPowerCoef()
        {
            if (timing == BattingTiming.PERFECT)
            {
                powerCoef2 = 1.1f;
            }
            else if (timing == BattingTiming.JUST_EARLY || timing == BattingTiming.JUST_LATE)
            {
                powerCoef2 = Random.Range(0.95f, 1.05f);
            }
            else if (timing == BattingTiming.EARLY || timing == BattingTiming.LATE)
            {
                powerCoef2 = Random.Range(0.85f, 0.95f);
            }
            else
            {
                powerCoef2 = Random.Range(0.65f, 0.75f);
            }
        }*/



        //배팅 커서와 탄착접과의 offset을 체크한후 추후 타구질을 결정하기 위한 값을 산출함
        private void setBattingMissOffset(float px, float py)
        {
            //////UnityEngine.//Debug.Log("=============================>>> manager.battingResultData.hitType = " + manager.battingResultData.hitType);
            int xSign = battingOffsetX < 0 ? -1 : 1;
            float offsetX = Mathf.Abs(battingOffsetX);  //양수
            float offsetY = Mathf.Abs(battingOffsetY);  //양수

            //////UnityEngine.//Debug.Log("=====================>>offsetX = " + offsetX);
            //////UnityEngine.//Debug.Log("=====================>>offsetY = " + offsetY);

            float finalX = 0;
            float finalY = 0;

            bool bNear = ((px * sign) > 0 ? true : false);
            //SimulHitType hitType = manager.battingResultData.hitType;

            if (contact == BattingContact.SOLID)
            {
                //X축
                finalX = offsetX;
                //Y축
                finalY = Random.Range(offsetY, offsetY + 5);
            }
            else if (contact == BattingContact.GOOD)
            {
                //X축
                finalX = offsetX;
                //Y축
                finalY = Random.Range(offsetY, offsetY + 10);
            }
            else if (contact == BattingContact.NORMAL)
            {
                //X축
                finalX = Random.Range(offsetX, offsetX + 5);
                //Y축
                finalY = Random.Range(offsetY + 5, offsetY + 15);
            }
            else if (contact == BattingContact.BAD)
            {
                if (bNear == true)
                {
                    //안쪽으로 맞은 경우
                    contact = BattingContact.JAMMED;
                }

                //X축
                finalX = Random.Range(offsetX + 5, offsetX + 10);
                //Y축
                finalY = Random.Range(offsetY + 15, offsetY + 30);
            }

            battingOffsetX = xSign * finalX;
            battingOffsetY = (battingOffsetY > 0 ? 1 : -1) * finalY;


            //////UnityEngine.//Debug.Log("=====================>>battingOffsetX = " + battingOffsetX);
            //////UnityEngine.//Debug.Log("=====================>>battingOffsetY = " + battingOffsetY);
        }


        //현재 휘두르는 타이밍을 간략화 해서 -1, 0, 1값으로 리턴한다
        public int getTiming()
        {
            if (bSwing == true && manager.bMyTurn == true)
            {
                if (timing == BattingTiming.EARLY || timing == BattingTiming.TOO_EARLY || timing == BattingTiming.VERY_EARLY)
                {
                    return -1;
                }
                else if (timing == BattingTiming.LATE || timing == BattingTiming.TOO_LATE || timing == BattingTiming.VERY_LATE)
                {
                    return 1;
                }
            }

            return 0;
        }
        //////////////////////////////////////////////////////////////
        //타격의 AI
        //기본적으로 타구의 결과는 시뮬엔진을 통해 나와 있으나
        //예외를 타는 경우 이 로직으로 들어와서 처리 된다.
        //////////////////////////////////////////////////////////////
        private const int CHECK_WAIT = 0;
        private const int CHECK_SWING = 1;
        private const int CHECK_END = 2;

        private float aiTIming;
        private int conStrike;
        private int conSameBall;
        private int lastSelectBall;

        public bool bDecideSwing;

        //AI 메인 프레임
        private void aiBatting()//int frame)
        {
            if (bReleaseCheck == true) //pitcher.pState == PitcherState._RELEASE)
            {
                if (bAiCheck == false)
                {
                    if (aiCheckStep == CHECK_WAIT)
                    {                        
                        if(Mode.bPvpMode433 == true)
                        {
                            //Debug.Log("==========================>>aiCheckStep == CHECK_WAIT");
                            if(manager.Pvp_bSwing == true)
                            {
                                bDecideSwing = true;
                                aiCheckStep = CHECK_SWING;
                                if (manager.Pvp_bContact == true)
                                {
                                    //Debug.Log("여기 체크!! ==========================>>Pvp_bContact == CHECK_WAIT");
                                    aiContactPoint = BattingContact.SOLID;
                                    aiTIming = _PERFECT_TIMING;
                                    timing = BattingTiming.PERFECT;
                                }
                                else
                                {
                                    aiContactPoint = BattingContact.HUT_SWING;
                                    aiTimingPoint = manager.Pvp_TimingPoint;
                                    //Debug.Log("실제 적용되는 헛스윙 타이밍==============>>" + aiTimingPoint);
                                    aiTIming = getAiTiming();
                                    //aiTIming = _PERFECT_TIMING + Random.Range(-0.12f, 0.10f);
                                }
                            }
                            else if(manager.Pvp_bBunt == true)
                            {
                                buntAnim();
                                aiCheckStep = CHECK_END;
                            }
                        }
                        else
                        {
                            //일반모드에서 AI배팅
                            if (aiDecideSwing() == true)
                            {
                                bDecideSwing = true;
                                aiCheckStep = CHECK_SWING;
                                aiTIming = getAiTiming();
                            }
                            else
                            {
                                aiCheckStep = CHECK_END;
                            }
                        }
                    }
                    else if (aiCheckStep == CHECK_SWING)
                    {
                        if (bSwing == false)
                        {
                            float hitRate = pitcher.getHitRate();
                            if (hitRate >= aiTIming)
                            {
                                //////UnityEngine.//Debug.Log("====================================>> hitRate = " + hitRate + "=====>> aiTiming = " + aiTIming);
                                if (manager.batterSkillFlag == SkillFlag.FalconEye) Time.timeScale = 1.0f;

                                if (bPvState == true)
                                {
                                    StartCoroutine(startSwingAnimPv());
                                }
                                else
                                {
                                    StartCoroutine(startSwingAnim());
                                }
                                bAiCheck = true;
                                aiCheckStep = CHECK_END;
                            }
                        }
                    }
                }
            }
        }

        /*
        public void setSyncHit()
        {
            aiCheckStep = CHECK_SWING;
            aiContactPoint = BattingContact.SOLID;
            aiTIming = _PERFECT_TIMING;
        }*/

        //선구 및 배팅 결정
        private bool aiDecideSwing()
        {
#if _BATTER_NO_AI
            //return bStrikeCheck;   //스트라이크만 스윙
            return false; //true인경우 무조건 스윙 false인 경우 무조건 스윙안함
#else
                        
            if (Mode.bPvpMode433 == true)
            {
                //pvp에서는 ai가 결정하지 않는다
                return false;
            }
            else
            {
                ////Debug.Log("================>>> AI 스윙선택 run.stealResult = " + run.stealResult + "        히트앤드런 플래그 " + aiHitandRunDecide);

                if (run.stealResult != SimulStealState.NONE && aiHitandRunDecide == false)
                {
                    //도루시 스윙안함, 근데 힛앤런시는 함
                    return false;
                }

                /*
                if (Mode.bAutoPlay == true)
                {
                    return autoModeSwing;
                }
                else*/
                {
                    if (pitcher.bMissControl == true)
                    {
                        //실투시 스트라이크면 휘두름
                        return bStrikeCheck;
                    }
                    else
                    {
                        //직접플레이시 AI의 스윙 판단여부
                        return BattingMechanism.checkSwingDecide(pitcher.controlValue,
                                                                 pitcher.pGuwee,
                                                                 bEye,
                                                                 bStrikeCheck,
                                                                 manager.nBallCount,
                                                                 manager.nStrikeCount,
                                                                 false);
                    }
                }
            }
#endif
        }

        //AI의 배팅 타이밍을 얻어오는 함수
        private float getAiTiming()
        {
#if _BATTER_NO_AI
            return Random.Range(_PERFECT_FAST, _PERFECT_LATE); //랜덤
#else
            float value;

            timing = aiTimingPoint;// BattingTiming.PERFECT;
            
            if (timing == BattingTiming.PERFECT)
            {
                value = _PERFECT_TIMING;
            }
            else if (timing == BattingTiming.JUST_EARLY || timing == BattingTiming.EARLY)
            {
                value = _PERFECT_FAST - 0.01f;
            }
            else if (timing == BattingTiming.JUST_LATE || timing == BattingTiming.LATE)
            {
                value = _PERFECT_LATE + 0.01f;
            }
            else if (timing == BattingTiming.VERY_EARLY)
            {
                value = _PERFECT_FAST - 0.071f;
            }
            else //if (timing == BattingTiming.VERY_LATE)
            {
                value = _PERFECT_LATE + 0.06f;
            }

            return (value);
#endif
        }


        /////////////////////////////////////////////////////////////////
        //번트 및 작전
        //번트나 작전은 수동모드 내공격을 제외한 다른 모드에서는 무조건 시뮬에서 결과값을 얻어온다
        //하지만 유저공격의 수동조작은 실시간으로 결과를 계산한다.
        /////////////////////////////////////////////////////////////////
        
        //번트 작전여부를 시뮬 엔진으로부터 얻어옴
        //simulateBattingOnly를 호출후 처리하는 프로세스의 가장 마지막에 호출
        public void buntTryCheck()
        {
            ////UnityEngine.//Debug.Log("=============================================================================================>> buntTryCheck 호출!!");
            buntResult = manager.battingResultData.buntResultType;
            if (buntResult != SpecificBuntType.NONE)
            {
                buntDir = 0;
                buntFielder = manager.battingResultData.fIndex;
                if (buntResult == SpecificBuntType.DRAG_FAIL ||
                   buntResult == SpecificBuntType.DRAG_SUCCESS)
                {
                    buntTypePre = buntType = SimulBuntType.DRAG;

                }
                else if (buntResult == SpecificBuntType.SQUEEZ_FAIL ||
                    buntResult == SpecificBuntType.SQUEEZ_FIELDER_CHOICE ||
                    buntResult == SpecificBuntType.SQUEEZ_SUCCESS)
                {
                    //스퀴즈 감행할 카운트 세팅
                    field.run.setStealCount(Random.Range(0, 2));
                    buntTypePre = buntType = SimulBuntType.SQUEEZE;
                    if (buntFielder == CPlayer._CATCHER) buntFielder = CPlayer._PITCHER;
                }
                else
                {
                    buntTypePre = buntType = SimulBuntType.SACRIFY;
                }


                if (buntFielder == CPlayer._THIRDBASEMAN) buntDir = 1;
                else if (buntFielder == CPlayer._FIRSTBASEMAN) buntDir = -1;
                else
                {
                    if (buntType == SimulBuntType.DRAG) buntDir = 1;
                    else
                    {
                        buntDir = Random.Range(0, 10) < 5 ? -1 : 1;
                    }
                }
            }
            ////UnityEngine.//Debug.Log("==============================================>>번트 타입 = " + buntType + "======>> 번트결과 = " + buntResult);
        }

        //동적으로 번트의 결과를 얻어오는 함수
        public SpecificBuntType getBuntDynamicResult()
        {
            //buntDir = -1 : 1루쪽
            //buntDir = 1 : 3루쪽
            //동적으로 번트타입 얻어옴            
            //Debug.Log("번트 동적 체크");
            if (SimulBunt.checkSqueezeBuntCase(manager.nOutCount, 0, field.run.bOnBase) == true)
            {
                //Debug.Log("스퀴즈 케이스");
                buntTypePre = SimulBuntType.SQUEEZE;
                //30%확률로 피치아웃!!
                //field.bSqueezePitchedOut = true; 이건 차후 다른 부분에서 호출
            }
            else
            {
                if (SimulBunt.checkSacBuntCase(manager.nOutCount, 0, field.run.bOnBase) == true)
                {
                    //Debug.Log("희생 케이스");
                    buntTypePre = SimulBuntType.SACRIFY;
                }
                else
                {
                    if (SimulBunt.checkDragBunt(0, field.run.bOnBase) == true)
                    {
                        //Debug.Log("드래그 케이스");
                        buntTypePre = SimulBuntType.DRAG;

                        if (manager.batterSkillFlag == SkillFlag.GodOfBunt) 
                        {
                            //번트 신 발생시 무조건 성공
                            return SpecificBuntType.DRAG_SUCCESS;
                        }
                    }
                }
            }

            
            if (buntTypePre == SimulBuntType.SQUEEZE)
            {
                //스퀴즈를 댈경우 성공여부
                return SimulBunt.getSqueezeSuccessResult(pitcher.pPitcher, pBatter);
            }
            else if (buntTypePre == SimulBuntType.SACRIFY)
            {
                //희생을 댈경우 성공여부
                return SimulBunt.getSacSuccessResult(pitcher.pPitcher, pBatter);
            }
            else if (buntTypePre == SimulBuntType.DRAG)
            {
                //드래그를 될경우 성공여부
                return SimulBunt.getDragBuntSuccessResult(pitcher.pPitcher, pBatter);
            }
            else
            {
                return SpecificBuntType.NONE;
            }

        }

        //////////////////////////////////////////////////////////////
        //FRAME 함수
        //////////////////////////////////////////////////////////////
        //배팅 메인 프레임
        public void batterFrame()
        {
            switch (bState)
            {
                case BatterState._WAITING:
                    waitingFrame();
                    break;
                case BatterState._LOOKING:
                    lookingFrame();
                    break;
                case BatterState._SWING:
                    swingFrame();
                    break;
                case BatterState._SWING_BACK:

                    break;
                case BatterState._BUNT:
                    checkBuntHit();
                    break;
            }
        }

        //웨이팅 프레임
        private void waitingFrame()
        {
            /* //필요없음
            if (Mode.bPvpMode == true)
            {
                if (manager.bMyTurn == true)
                {
                    if (PvpManager.bGameReady == true)
                    {
                        //PVP모드에서 배팅 대기시 game ready 플래그가 들어오면 시뮬로 강제전환
                        if (manager.checkChanceModeEnd(SimulResultState.NONE) == true)
                        {
                            bState = BatterState._NONE;
                        }
                    }
                }
            }*/
        }

        //루킹 프레임
        private void lookingFrame()
        {
            if (manager.bMyTurn == false || Mode.bAutoPlay == true)
            {
                if (buntType != SimulBuntType.SQUEEZE)
                {
                    aiBatting();
                }
            }

            if (pitcher.hitByPitchStep == 1)
            {
                if (pitcher.bRelease == true)
                {
                    float hitRate = pitcher.getHitRate();
                    if (hitRate >= _PERFECT_FAST - 0.2f)
                    {
                        pitcher.hitByPitchStep = 2;                        
                        StartCoroutine(hitByPitchAnim());
                    }
                }
            }
        }

        //스윙 프레임
        private void swingFrame()
        {
        }

        //////////////////////////////////////////////////////////////
        //타자의 애니메이션
        //////////////////////////////////////////////////////////////        
        //스윙 애니메이션의 초기화
        public void initSwingAnim(bool bReady)
        {
            //대기 상태에 따른 종류 선별

            //Debug.Log("=========================>> initSwingAnim " + bReady);

            if (bReady == false) waitAnim();

            bState = BatterState._WAITING;
            bSwing = false;
            bAiCheck = false;
            bPreHitCheck = false;

            //shadow.renderer.enabled = false;
            //shadow.gameObject.transform.localScale = new Vector3(1, 1, 1);

        }

        //스윙 딜레이 조정
        private void checkSwingDelayAdjust(float timingGab)
        {
            if (bPreHitCheck == true)
            {
                /*
                if (Mode.bAutoPlay == false && manager.bMyTurn == true)
                {
                    zoneUI.timerObj.SetActive(false);
                }*/

                ////UnityEngine.//Debug.Log("===========================================>>timing = " + timing);
                if (pitcher.checkPitchSystemZoneCheck() == true)
                {
                    ////UnityEngine.//Debug.Log("===========================================>>111111");
                    swingWaitDelay = 0;
                }
                else
                {
                    //swingWaitDelay += timingGab;
                    if (timing > BattingTiming.PERFECT)
                    {
                        ////UnityEngine.//Debug.Log("===========================================>>22222");
                        swingWaitDelay = 0.03f + timingGab;
                        if (swingWaitDelay > 0.03f) swingWaitDelay = 0.03f;
                    }
                    else if (timing == BattingTiming.PERFECT)
                    {
                        swingWaitDelay = 0.04f + timingGab;
                        if (swingWaitDelay > 0.053f) swingWaitDelay = 0.053f;
                    }
                    else
                    {
                        ////UnityEngine.//Debug.Log("===========================================>>33333");
                        swingWaitDelay = 0.045f + timingGab;
                        if (swingWaitDelay > 0.09f) swingWaitDelay = 0.09f;
                    }
                    if (swingWaitDelay < 0) swingWaitDelay = 0;

                }
                ////UnityEngine.//Debug.Log("===========================================>>swingWaitDelay = " + swingWaitDelay);
            }
        }

        /*//액티브 스킬 컨택 타이밍 보정
        private void checkActiveSkillAdjust(float timingGab)
        {
            if (bSpecialBattingOn == true)
            {
                if (specialHitType == SpecialHitType.CurtSingong)
                {
                    if (timing < BattingTiming.TOO_LATE)
                    {
                        if (bPreHitCheck == false)
                        {
                            if (timing > BattingTiming.PERFECT)
                            {
                                swingWaitDelay = 0;
                            }
                            else
                            {
                                swingWaitDelay = 0.04f + timingGab;
                            }
                            bPreHitCheck = true;
                            timing = BattingTiming.PERFECT;
                            contact = BattingContact.TIP;
                        }
                    }
                }
                else if (specialHitType == SpecialHitType.BlackHole)
                {
                    bPreHitCheck = true;
                    timing = BattingTiming.PERFECT;
                    contact = BattingContact.SOLID;
                    swingWaitDelay = 0.01f;
                }
                else if (specialHitType == SpecialHitType.Irregular)
                {
                    //해당사항없음
                }
                else //if (bActiveSkillFlag[SkillParm.YASU_ATTACK] == true)
                {
                    //그외 액티브는 타격 & 스트라이크 스윙 & 정타이밍시 발동 솔리드 컨택으로 발동
                    if (bPreHitCheck == true)
                    {
                        if (bStrkeSwing == true && (timing >= BattingTiming.JUST_EARLY && timing <= BattingTiming.JUST_LATE))
                        {
                            contact = BattingContact.SOLID;
                        }
                        else
                        {
                            //그렇지 않은경우 액티브 취소
                            bSpecialBattingOn = false;
                        }
                    }
                }
            }   
        }*/


#if _Skill_Display
        //연출테스트용
        public CSkill tempPitchSkill = null;

        
#endif

        


        //스윙 애니메이션 시작 
        //[중요]볼을 맞추는 메커니즘을 여기서 담당
        public IEnumerator startSwingAnim()
        {
            //스윙시 스킬 연출
            manager.effectCheck(SkillEffectDisplayManager.DisplayStep.Swing);  

            bDecideSwing = true;
            float timingGab = (perfectTimingGabRate * batterPerfectTime );
            //ballDrawDelay = 0.001f;
            swingWaitDelay = 0.04f;
            if (pitcher.bRelease == false)
            {
                batterAnim(1, BattingMechanism.NORMAL_FULL_SWING, false);
            }
            else
            {
                //Debug.Log("aaaaaaaaaaaaaaaaa");
                bHitHomeRun = false;

                zoneUI.setBatCursorActive(false);//

                bState = BatterState._SWING;
                bSwing = true;
                onTimingSwing = false;
                checkNormalHit();
                //pitcher.bball.bCameraMove = false;

                //스윙 딜레이 보정
                checkSwingDelayAdjust(timingGab);   //swingWaitDelay값이 여기서 보정됨


                yield return new WaitForSeconds(swingWaitDelay);

                if (bPreHitCheck == true)
                {
                    //실투 보정
                    if (pitcher.bMissControl == true)
                    {
                        timing = BattingTiming.PERFECT;
                    }
                    
                    field.setHitVector();

                    IngameUI.GetPitchUI().SetPitchCursor(false, 0, 0);
                    manager.hitBallType = HITBALLTYPE._NONE;
                    pitcher.pitch.pitchOrigin.bBallHit = true; //처리됨 타격뷰에서만 불리어지는 함수이므로 

                    
                    int timVal = (int)timing;

                    if (contact == BattingContact.TIP)
                    {
#if GIRL_PLAY
                        batterAnim(1, BattingMechanism.NORMAL_FULL_SWING, false);
#else
                        batterAnim(1, (bStrkeSwing ? BattingMechanism.NORMAL_FULL_SWING : BattingMechanism.EARLY_TIMING_SWING), false);                        
#endif
                        StartCoroutine(tipCheckDelay(0.05f));
                    }
                    else
                    {
                        /*if ((bBadballSwingNear == true && timVal >= 4) || manager.pitcherSkillFlag == SkillFlag.TenderStroke)
                        {
                            //배트 브로큰 or 회심의 일격
                            manager.pitchSkillPitcherWin();//일부 타자 스킬 무효화 처리
                            manager.pitcherSkillFlag = SkillFlag.None;
                            field.setBrokenBat();
                            manager.hitBallType = HITBALLTYPE._BROKEN; 
                            brokenSwingAnim();
                        }
                        else*/
                        {                            
                            if (checkHitGood() == true)
                            {
                                //온타임 스윙
                                bHitGood = true;
                                onTimingSwingAnim();
                            }
                            else
                            {
                                //오프타이밍이지만 노멀스윙
                                bHitGood = false;
                                if (MyMath.Percent() < 60 || field.ballPower > 28)
                                {
                                    onTimingSwingAnimTwoStep();// 
                                }
                                else
                                {
                                    offTimingSwingAnim(false);
                                }
                            }
                        }
                        pitcher.setPitchSystemHit();
                    }

                }
                else
                {
                    if (bCheckSwingActivate == true)
                    {
                        //체크스윙
                        checkSwingAnim();
                    }
                    else
                    {
                        hutSwingAnim();
                    }
                }
            }
        }//conVal


        //정타 여부 확인
        private bool checkHitGood()
        {
            if (field.ball.bHomeRunGuess == true || field.ball.bFoulHomerunGuess == true || field.ball.bFenceMeetGuess == true)
            {
                return true;
            }
            else
            {
                if (Mathf.Abs(field.ball.firstAngle) < 45)
                {
                    if (field.ballPower > 26 && (field.ball.firstAngleZ > 19 && field.ball.firstAngleZ < 47))
                    {
                        return true;
                    }
                    else
                    {
                        if (field.ballPower > 30 && (field.ball.firstAngleZ > 5 && field.ball.firstAngleZ < 60))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //헛 스윙후 딜레이후 스윙 초기화
        IEnumerator swingDelay()
        {
            yield return new WaitForSeconds(0.4f);
            initSwingAnim(false);
            //manager.nFrame = 0;
        }

        //스윙후 필드뷰로 바뀌기전 딜레이
        private IEnumerator fieldDelay()
        {
            float delay = 0.001f;
            //FieldState view = manager.getView();
            if (field.fieldState == FieldState.NORMAL_FIELD)
            {
                if (field.ball.angleZ > 35) delay = 0.4f;
                else delay = 0.1f;
            }
            else
            {
                delay = 0.25f;
            }
            //////////UnityEngine.//Debug.Log("==============>>delay = " + delay);
            //field.ball.bBallActive = true;
            yield return new WaitForSeconds(delay);
            if (field.bFieldViewActive == false)// if (manager.playState != PlayState.PLAY_FIELDING_VIEW)
            {
                manager.changeFieldView(field.fieldState);
            }

        }

        //정상 타이밍에 휘두르는 스윙 애니메이션
        private void onTimingSwingAnim()//int conVal)
        {
            onTimingSwing = true;
            
            //pitcher.bball.bCameraMove = true;
            bHitGood = true;

            //각기
            batterAnim(1, BattingMechanism.CORRECT_TIMING_SWING + batPos + batCourse, false);
            //블렌딩
            //batterAnimBlend(BattingMechanism.CORRECT_TIMING_SWING + batPos + batCourse, false);


            bHitHomeRun = field.ball.bHomeRunGuess;
            if (bHitHomeRun == true)
            {
                battingview.setJustMeet(true, "hitfocus", 1.2f, 0.5f);
            }
            else
            {
                battingview.setJustMeet(true, "hitfocus2", 1.2f, 0.8f);
            }

            StartCoroutine(hitCheckDelay(true, BattingMechanism._ONTIMING_HIT_DELAY));

        }


        //onTimingSwingAnim  스윙후 체크 딜레이
        private IEnumerator hitCheckDelay(bool bEffect, float delay)
        {            
            yield return new WaitForSeconds(delay);//BattingMechanism._NORMAL_HIT_DELAY);

            setHit();
                        

            setHitEffect(true);//, manager.hitBallType);

            if (bHitHomeRun == true && onTimingSwing == true)
            {
                //정타 사운드
                if(manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(500);
                soundmanager.Get().PlaySound(soundmanager.SoundID.HitBest);
                field.bFieldFirst = !manager.bMyTurn;
                yield return new WaitForSeconds(0.03f);
                CameraManager.SetZoomTo(1.1f, 0.1f);
                CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360 - 20, -200), 0.1f); 
                CameraManager.CameraShake(0.3f, 15);
                yield return new WaitForSeconds(0.4f);                
                CameraManager.SetZoomTo(1.0f, 0.2f);
                CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360, -200), 0.2f);          
                yield return new WaitForSeconds(0.2f);
                StartCoroutine(battingview.justMeetHomerunEffect2());
                //CameraManager.SetInvert(true, true); 
                nextStepDelay(bBatFlipEvent);
                yield return new WaitForSeconds(0.2f);
                pitcher.pitcherStartledLookingAnim();
            }
            else
            {
                //정타 사운드
                if (manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(300);
                soundmanager.Get().PlaySound(soundmanager.SoundID.HitGood);
                if (bHitGood == true)
                {
                    
                    field.bFieldFirst = !manager.bMyTurn;
                    yield return new WaitForSeconds(0.03f);                    
                    CameraManager.SetZoomTo(1.1f, 0.1f);
                    CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360 - 20, -200), 0.1f);          
                    CameraManager.CameraShake(0.175f, 15);
                    yield return new WaitForSeconds(0.25f);                    
                    CameraManager.SetZoomTo(1.0f, 0.15f);
                    CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360, -200), 0.15f);          
                    yield return new WaitForSeconds(0.15f);
                    StartCoroutine(battingview.justMeetHomerunEffect2());
                    nextStepDelay(bBatFlipEvent);
                    if (field.ball.firstAngleZ > 25)
                    {
                        yield return new WaitForSeconds(0.2f);
                        pitcher.pitcherStartledLookingAnim();
                    }
                }
                else
                {
                    ////Debug.Log("======>>여기냐?");
                    CameraManager.CameraShake(0.2f, 10); //완료 (개선여지있음)
                    yield return new WaitForSeconds(0.01f);
                    nextStepDelay();                    
                }
            }

        }


        //배트가 부러지는 애니메이션
        private void brokenSwingAnim()
        {
            batterAnim(1, BattingMechanism.BROKEN_SWING, false);            
            StartCoroutine(hitCheckDelay2(true));
        }

        //brokenSwingAnim 스윙후 체크 딜레이
        private IEnumerator hitCheckDelay2(bool bBroken = false)
        {
            //yield return new WaitForSeconds(delay);            
            yield return new WaitForSeconds(BattingMechanism._BROKEN_HIT_DELAY);            
            setHit();
            CameraManager.CameraShake(0.1f, 10);    //완료

            //약타 사운드
            if (manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(150);
            soundmanager.Get().PlaySound(soundmanager.SoundID.HitWeak);

            battingview.zone.SetActive(false);
            setHitEffect(true,  bBroken);

            yield return new WaitForSeconds(0.015f);
            nextStepDelay2(bBroken);            
        }


        //늦거나 빠른 타이밍에 휘두르는 스윙 애니메이션
        private void offTimingSwingAnim(bool bNormalSwing)
        {
            //////UnityEngine.//Debug.Log("===================>>offTimingSwingAnim");
#if GIRL_PLAY
            batterAnim(1, BattingMechanism.NORMAL_FULL_SWING, false);
#else
            if (bNormalSwing == true)
            {
                //////UnityEngine.//Debug.Log("===================>>여기로 오냐?");
                batterAnim(1, BattingMechanism.NORMAL_FULL_SWING, false);
            }
            else
            {
                batterAnim(1, BattingMechanism.EARLY_TIMING_SWING, false);
            }            
#endif
            StartCoroutine(hitCheckDelay3(0.1f));
        }

        //offTimingSwingAnim 스윙 후 체크 딜레이
        private IEnumerator hitCheckDelay3(float delay)
        {            
            yield return new WaitForSeconds(delay);
            setHit();
            CameraManager.CameraShake(0.08f, 7);    //완료
            if (manager.hitBallType == HITBALLTYPE._HOMERUN)
            {
                //정타 사운드
                if (manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(300);
                soundmanager.Get().PlaySound(soundmanager.SoundID.HitGood);
            }
            else
            {
                //약타 사운드
                if (manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(150);
                soundmanager.Get().PlaySound(soundmanager.SoundID.HitWeak);
            }
            
            
            battingview.zone.SetActive(false);

            setHitEffect(true, false, true);

            yield return new WaitForSeconds(0.015f);
            nextStepDelay2();            
        }



        private void onTimingSwingAnimTwoStep()
        {
            //각기
            batterAnim(1, BattingMechanism.CORRECT_TIMING_SWING + batPos + batCourse, false);
            //블렌딩
            //batterAnimBelnd(BattingMechanism.CORRECT_TIMING_SWING + batPos + batCourse, false);
            //ballDrawDelay = 0.04f;


            StartCoroutine(hitCheckDelay4(0.12f));//
        }

        private IEnumerator hitCheckDelay4(float delay)
        {
            if (field.ballPower < 31)
            {
                CameraManager.CameraShake(0.08f, 7);    //완료
                setHitEffect(true, false, true);
            }
            else
                setHitEffect(true);

            yield return new WaitForSeconds(delay);

            float timeScale = 1.0f;
            string strength = "_STRONG";
            if (field.ballPower < 22)
            {
                strength = "_MISS";
                timeScale = 0.8f;
            }
            else if (field.ballPower < 28)
            {
                strength = "_WEAK";
                timeScale = 0.9f;
            }

            string direction = "_C";
            bool bLeftHand = batterHand == CPlayer._LEFTHAND ? true : false;

            if (bHitGood == false || Mathf.Abs(field.ball.firstAngle) < 25)
            {
                if (field.ball.firstAngle > 10) direction = bLeftHand ? "_L" : "_R";
                else if (field.ball.firstAngle < -10) direction = bLeftHand ? "_R" : "_L";
            }

            //각기
#if GIRL_PLAY
            batterAnim(2, "FOLLOWTHROW_NORMAL_STRONG_C", false, timeScale);
#else
            batterAnim(2, BattingMechanism.FOLLOW_THROW + hitType + strength + direction, false, timeScale);
#endif
            //블렌딩
            //batterAnimBlend(BattingMechanism.FOLLOW_THROW + hitType + strength + direction, false);

            setHit();
            
            //CameraManager.CameraShake(0.12f, 12);    //완료

            if (manager.hitBallType == HITBALLTYPE._HOMERUN)
            {
                //정타 사운드
                if (manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(300);
                soundmanager.Get().PlaySound(soundmanager.SoundID.HitGood);
            }
            else
            {
                //약타 사운드
                if (manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(150);
                soundmanager.Get().PlaySound(soundmanager.SoundID.HitWeak);
            }
                        
            battingview.zone.SetActive(false);

            

            yield return new WaitForSeconds(0.015f); 
            nextStepDelay2();
        }


               
        //번트 체크 딜레이
        private IEnumerator buntCheckDelay()
        {
            /*
             *번트 신공
             */

            yield return new WaitForSeconds(0.1f);

            CameraManager.CameraShake(0.1f, 5); //완료됨

            yield return new WaitForSeconds(0.33f);  //yield return new WaitForSeconds(0.43f);

            //약타 사운드
            if (manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(150);
            soundmanager.Get().PlaySound(soundmanager.SoundID.HitWeak);

            battingview.zone.SetActive(false);            
                        
            field.fieldState = FieldState.NORMAL_FIELD;
            manager.changeFieldView(field.fieldState);



        }

        //파울 팁 딜레이
        private IEnumerator tipCheckDelay(float delay)//int timVal)
        {
            yield return new WaitForSeconds(delay); 
            CameraManager.CameraShake(0.06f, 3);    //완료
            bTipped = true;

            //팁 사운드
            if (manager.bMyTurn) ToolShed.Android.OS.Vibrator.Vibrate(100); 
            soundmanager.Get().PlaySound(soundmanager.SoundID.HitTip);
        }

        //정상적인 타격 시작 후 컨택까지의 딜레이
        private void nextStepDelay(bool bFlip = false) //private IEnumerator nextStepDelay(float delay)
        {
            string strength = "_STRONG";
            string direction = "_C";
            bool bLeftHand = batterHand==CPlayer._LEFTHAND?true:false;

            if (bHitGood == false || Mathf.Abs(field.ball.firstAngle) < 25 || field.ball.firstAngleZ < 20.001f) //
            {
                if (field.ball.firstAngle > 10) direction = bLeftHand ? "_L" : "_R";
                else if (field.ball.firstAngle < -10) direction = bLeftHand ? "_R" : "_L";
            }

#if GIRL_PLAY
            batterAnim(1, "FOLLOWTHROW_NORMAL_STRONG_C", false);
#else
            if (bFlip == true)
            {
                //각기
                batterAnim(1, "BAT_FLIP" + Random.Range(1, (bHitHomeRun ? 4 : 3)), false);
                //블렌딩
                //batterAnimBlend("BAT_FLIP" + Random.Range(1, (bHitHomeRun ? 4 : 3)), false);
            }
            else
            {   
                //각기
                batterAnim(1, BattingMechanism.FOLLOW_THROW + hitType + strength + direction, false);
                //블렌딩
                //batterAnimBlend(BattingMechanism.FOLLOW_THROW + hitType + strength + direction, false);
            }
#endif

            pitcher.setHitBallNextStep();

            setHitEffect(false);//, manager.hitBallType);
        }

        //컨택 후 팔로스로우까지의 딜레이
        private void nextStepDelay2(bool bBroken = false) 
        {
            //yield return new WaitForSeconds(delay);
            pitcher.setHitBallNextStep();

            setHitEffect(false,  bBroken);
        }


        //볼을 맞춘후 실제 던진공은 감춰주는 메쏘드
        private IEnumerator setPitchBallErase(float delay)
        {
            yield return new WaitForSeconds(delay);
            pitcher.setPitchSystemDraw(false);
        }

        //체크스윙 애니메이션
        private void checkSwingAnim()
        {
            pitcher.bCheckSwingAsk = true;
            bCheckSwingActivate = false;
            bCheckSwing = false;
            bSwing = false;
            bCheckSwinged = true;

            batterAnim(1, BattingMechanism.CHECK_SWING, false);
        }

        //헛스윙 애니메이션
        public void hutSwingAnim()
        {
#if GIRL_PLAY
            batterAnim(1, "EX_SWING_NORMAL", false, 0.75f);
#else
            if (timing <= BattingTiming.TOO_EARLY && MyMath.Percent() < 25)
            {
                batterAnim(1, MyMath.Half()?"2010_EARLYSWING_02":"2010_EARLYSWING_OUT", false);
            }
            else
            {
                if (courseIndex == 0)
                {
                    batterAnim(1, manager.nStrikeCount >= 2 ? "2010_EARLYSWING_OUT" : "2010_EARLYSWING_01", false);
                }
                else if (courseIndex == 1)
                {
                    batterAnim(1, "INCOURSE_SWING_HEADUP", false, 0.75f);                    
                }
                else
                {
                    batterAnim(1, "OUTCOURSE_SWING_HEADUP", false);
                }
            }
#endif
        }

        

        //릴리즈 된 공을 쳐다보는 애니메이션
        public void lookingAnim(bool bBuntControl, bool bAnimInit = false)
        {            
            if (bState == BatterState._WAITING || bBuntControl == true)
            {
                //볼 선구관련 애니메이션
                string takeBack = (bGangTa ? "TAKEBACK_POWERSWING" : "1000_TAKEBACK_NORMAL");
                if (bAnimInit == true)
                {
                    batterAnim(0, takeBack, false);
                }
                else
                {
                    //추후 수정
                    //블렌딩
                    if (bPvState == true)
                    {
                        pAnim.state.SetAnimation(0, takeBack, false);
                    }
                    else
                    {
                        anim.state.SetAnimation(0, takeBack, false);
                    }
                }
                strID = takeBack;
                bState = BatterState._LOOKING;
                bBunt = false;
                //anim.state.End += EndEvent;
            }
        }


        //타격 대기및 대기 상태로의 전환
        public void waitAnim()
        {
            //Debug.Log("타자 웨이트 애님");
            if (bReadyAnim == false)
            {
                strID = "";
                initPosition();
#if GIRL_PLAY
                batterAnim(0, "0002_WAIT_CLOSE_01", true);
#else
                batterAnim(0, "0002_WAIT_CLOSE_0" + (battingType + 1), true);
#endif
            }
            bReadyAnim = false;
        }

        //타격 준비 애니메이션 및 준비 상태로의 전환
        public bool bReadyAnim;
        public void readyAnim(bool newBatter)
        {
            //Debug.Log("타자 레디 애님");
            initPosition();
#if GIRL_PLAY
            batterAnim(0, "0002_WAIT_CLOSE_01", true);
#else
            batterAnim(0, "0002_WAIT_CLOSE_0" + (battingType + 1), true);
#endif
            bReadyAnim = true;
        }

        //번트 작전이 걸렸는지 여부 체크 -(오토모드 혹은 AI공격시)
        public bool checkCurrentBuntTryOn()
        {
            if (Mode.bAutoPlay == true || manager.bMyTurn == false)
            {
                if (buntType == SimulBuntType.SQUEEZE)
                {
                    if (run.stealCount == (manager.nStrikeCount + manager.nBallCount))
                    {
                        //스퀴즈 감행 카운트
                        buntAnim();
                        return true;
                    }
                }
                else
                {
                    if (buntType != SimulBuntType.NONE)
                    {
                        buntAnim();
                        return true;
                    }
                }
            }
            return false;
        }

        //번트 애니메이션 및 번트 상태로의 전환
        public void buntAnim()
        {
            initPosition();
            batterAnim(2, "7000_BUNT_NORMAL", false);
            bState = BatterState._BUNT;
            bBunt = true;
        }

        //스트라이크 아웃 루킹 애니메이션
        public void strikeOutLooking()
        {
            if (bSwing == false)
            {
                if (bPvState == true)
                {
                    //투수뷰 스트라이크 아웃루킹
                    pAnim.state.ClearTrack(lastTrack);
                    batterAnim(4, "5000_EVENT_LOOKING6", false);
                }
                else
                {
                    //타자뷰 스트라이크 아웃루킹
                    anim.state.ClearTrack(lastTrack);
                    batterAnim(4, "5000_EVENT_LOOKING" + Random.Range(3, 5), false);
                }
                
            }
        }

        //안쪽으로 공이 파고 들때 배터가 공을 참아내는
        public IEnumerator plateDiciplineAnim()
        {
            yield return new WaitForSeconds(batterPerfectTime * 0.7f);// * 0.85f);

#if GIRL_PLAY
#else

            if (bBunt == true || bBuntHit == true)
            {
            }
            else
            {
                if (bState == BatterState._LOOKING)
                {
                    if (bBunt == true || bBuntHit == true || bDecideSwing == true)
                    {
                    }
                    else
                    {
                        if (bSwing == false)
                        {
                            if (pitcher.hitByPitchStep == 0)
                            {
                                if ((pitcher.preArriveX * sign) < 0)
                                {
                                    bForcedSwingPrevent = true; //움찔시 강제 노 스윙
                                    if (MyMath.Half() == true)
                                        batterAnim(3, "STARTLED1", false);
                                    else
                                        batterAnim(3, MyMath.Half() ? "STARTLED3" : "STARTLED4", false);
                                }
                            }
                        }
                    }
                }
            }
#endif
        }

        //스트라이크 루킹 애니메이션
        public void strikeLooking()
        {
            if (bSwing == false)
            {
                //타자뷰 스트라이크 루킹
                batterAnim(4, "5000_EVENT_LOOKING" + Random.Range(1, 3), false);
            }
        }

        //공을 안치지만 몸은 반응하는
        public IEnumerator tryToSwing()
        {
            yield return new WaitForSeconds(batterPerfectTime * 0.7f);// * 0.85f);

#if GIRL_PLAY
#else
            if (bBunt == true || bBuntHit == true || bDecideSwing == true)
            {
            }
            else
            {
                bool bMove = false;
                if (bSwing == false)
                {
                    float course = (pitcher.preArriveX * sign);

                    if ( (course < 0 && Mathf.Abs(pitcher.preArriveX) >= 25)
                       ||(pitcher.bMissControl && pitcher.preArriveY >60))
                    {
                        if (bPvState == false)
                        {
                            bMove = true;
                            //안쪽공 움찔
                            if (pitcher.preArriveY < -40)
                            {
                                batterAnim(3, "STARTLED1", false, 0.5f);
                            }
                            else
                            {
                                batterAnim(3, MyMath.Half() ? "STARTLED3" : "STARTLED4", false, 0.5f);
                            }
                        }
                        else
                        {
                            if (bStrikeCheck == false)
                            {
                                bMove = true;
                                if (pitcher.preArriveY < -15)
                                {
                                    batterAnim(3, "STARTLED1", false, 0.5f);
                                }
                                else
                                {
                                    if (pitcher.bBreakingBallType == true && batterHand == pitcher.pitchHand)
                                    {
                                        batterAnim(3, "STARTLED3", false);
                                    }
                                    else
                                    {
                                        batterAnim(3, MyMath.Half() ? "STARTLED3" : "STARTLED4", false, 0.5f);
                                    }
                                }
                            }
                            /*else
                            {
                                if (pitcher.bBreakingBallType == true && batterHand == pitcher.pitchHand)
                                {
                                    batterAnim(3, "5000_EVENT_LOOKING5", false ,2);
                                }
                            }*/
                        }
                    }
                    else
                    {
                        if (pitcher.preArriveY < 5)
                        {
                            if ((course > 0 && Mathf.Abs(pitcher.preArriveX) >= 35)
                               || pitcher.preArriveY < -40)
                            {
                                bMove = true;
                                //바깥쪽볼 움찔
                                batterAnim(3, "STARTLED2", false, 0.5f);
                            }
                        }
                        else if (pitcher.preArriveY > Zone.UI_ZONE_HEIGHTC)
                        {
                            if (bPvState == true)
                            {
                                //if (MyMath.Percent() < 40)
                                {
                                    //투수뷰에서만 적용
                                    bMove = true;
                                    //높은공 움찔
                                    batterAnim(3, "STARTLED2", false);
                                }
                            }
                        }
                    }

                    float nextDelay;
                    if (bPvState == true) nextDelay = (bMove ? 0.8f : 0.5f);
                    else nextDelay = (bMove ? 1.3f : 1.0f);

                    yield return new WaitForSeconds(nextDelay);

                    if (manager.bStrikeCheck == true)
                    {
                        if (manager.nStrikeCount > 2 || manager.bStrikeOut == true)
                        {
                            strikeOutLooking();
                        }
                        else
                        {
                            //보더라인
                            if (bMove == false)
                            {
                                strikeLooking();
                                bReadyAnim = true;
                            }
                        }
                    }

                }
            }
#endif
        }

        //스윙 end 애니메이션
        public void endSwingAnim(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip)
        {

        }

        //힛바이 피치
        public IEnumerator hitByPitchAnim()
        {
            batterAnim(3, "DEADBALL" + Random.Range(1, 4), false);
            
            yield return new WaitForSeconds(0.12f);

            //힛바이 피치 사운드
        }


        //피칭뷰에서만 쓰이는 애니메이션
        /// <summary>
        ///피칭뷰에서 스타트 스윙
        /// </summary>
        /// <returns></returns>
        public IEnumerator startSwingAnimPv()
        {
            //스윙시 스킬 연출
            manager.effectCheck(SkillEffectDisplayManager.DisplayStep.Swing);

            float timingGab = (perfectTimingGabRate * batterPerfectTime);
            //ballDrawDelay = 0.001f;
            swingWaitDelay = 0.04f;
            if (pitcher.bRelease == false)
            {
                batterAnim(1, "SWING_NORMAL_1", false);
            }
            else
            {
                bHitHomeRun = false;

                zoneUI.setBatCursorActive(false);//

                bState = BatterState._SWING;
                bSwing = true;
                onTimingSwing = false;
                checkNormalHit();

                //스윙 딜레이 보정
                checkSwingDelayAdjust(timingGab);

                yield return new WaitForSeconds(swingWaitDelay);


                if (bPreHitCheck == true)
                {
                    //실투 보정
                    if (pitcher.bMissControl == true)
                    {
                        timing = BattingTiming.PERFECT;
                    }
                    
                    field.setHitVector();

                    IngameUI.GetPitchUI().SetPitchCursor(false, 0, 0);
                    manager.hitBallType = HITBALLTYPE._NONE;
                    //피칭뷰용 버전으로 고쳐
                    pitcher.pitchPv.pitchOriginPv.bBallHit = true;

                    int timVal = (int)timing;

                    if (contact == BattingContact.TIP)
                    {
#if GIRL_PLAY
                        batterAnim(1, "SWING_NORMAL_1", false);
#else
                        batterAnim(1, (bStrkeSwing ? "SWING_NORMAL_1" : "2010_EARLYSWING_01"), false);
#endif
                        StartCoroutine(tipCheckDelay(0.05f));
                    }
                    else
                    {
                        if ((bBadballSwingNear == true && timVal >= 4) || manager.pitcherSkillFlag == SkillFlag.TenderStroke)
                        {
                            //타이밍 늦고 안쪽 볼을 치면 
                            //bMustFoul = true;
                            manager.pitchSkillPitcherWin();//일부 타자 스킬 무효화 처리
                            manager.pitcherSkillFlag = SkillFlag.None;
                            field.setBrokenBat();
                            manager.hitBallType = HITBALLTYPE._BROKEN;
                            bHitGood = false;
                            swingAnimPv(false);
                        }
                        else
                        {
                            if (checkHitGood() == true)
                            {
                                field.bFieldFirst = !manager.bMyTurn;
                                //온타임 스윙
                                bHitGood = true;
                                swingAnimPv(true);
                            }
                            else
                            {
                                //오프타이밍이지만 노멀스윙
                                bHitGood = false;
                                swingAnimPv(false);
                                //onTimingSwingAnimTwoStep();// 
                            }
                        }
                        pitcher.setPitchSystemHit();
                    }

                }
                else
                {
                    if (bCheckSwingActivate == true)
                    {
                        checkSwingAnim();
                    }
                    else
                    {
                        hutSwingAnimPv();
                    }
                }
            }
        }

        /// <summary>
        /// 피칭뷰에서 공을 맞추는 경우
        /// </summary>
        /// <param name="bOnTiming"></param>
        private void swingAnimPv(bool bOnTiming)
        {
            onTimingSwing = bOnTiming;

            //공의 위치와 타구각에 의한 방망이 방향 설정
            float gabY = pitcher.preArriveY + Mathf.Clamp(-field.ball.angleZ * 0.33f, -15.0f, 15.0f); //기본 gab구하는 공식
            int batPosIndex = 0;             //방망이 중간으로 스윙   
            if (gabY < -25) batPosIndex = 2; //방망이 아래로 스윙
            else if (gabY > 35) batPosIndex = 4; //방망이 위로 스윙
            ////Debug.Log("================>> field.angleZ = " + field.ball.angleZ + "=======>> preArriveY = " + pitcher.preArriveY);
            ////Debug.Log("================>> gabY = " + gabY + "====> batPosIndex = " + batPosIndex);

            bool bFlip = false;
            if (bOnTiming == true)
            {
                
                batterAnim(1, "SWING_NORMAL_" + (batPosIndex + Random.Range(1, 3)), false);
                bHitHomeRun = field.ball.bHomeRunGuess;
                bFlip = bBatFlipEvent;// (MyMath.Percent() < 50 ? true : false);
            }
            else
            {
#if GIRL_PLAY
                batterAnim(1, "SWING_NORMAL_" + (batPosIndex + Random.Range(1, 3)), false);
#else
                if (manager.hitBallType == HITBALLTYPE._BROKEN)
                {
                    batterAnim(1, BattingMechanism.BROKEN_SWING, false);     
                }
                else
                {
                    if ((MyMath.Percent() < 60 || field.ballPower > 28) || batPosIndex == 2)
                    {
                        batterAnim(1, "SWING_NORMAL_" + (batPosIndex + Random.Range(1, 3)), false);
                    }
                    else
                    {
                        batterAnim(1, "2010_EARLYSWING_0" + Random.Range(1, 3), false);
                    }
                }
#endif
            }

            StartCoroutine(hitCheckDelayPv(bFlip, BattingMechanism._ONTIMING_HIT_DELAY, bOnTiming));

        }

        /// <summary>
        /// 피칭뷰에서 공을 맞춘후 hitcheck delay
        /// </summary>
        /// <param name="bFlip"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator hitCheckDelayPv(bool bFlip, float delay, bool bHitGood)
        {
            yield return new WaitForSeconds(delay);

            setHit();

            setHitEffectPv(true, bHitGood);

            CameraManager.CameraShake(0.2f, 10); //완료 (개선여지있음)
            yield return new WaitForSeconds(0.01f);
            if (bFlip == true)
            {
                Invoke("batFlip", 0.4f);
            }

            pitcher.setHitBallNextStep();

            yield return new WaitForSeconds(0.5f);

            setHitEffectPv(false, bHitGood);

        }

        //배트플립
        private void batFlip()
        {
#if GIRL_PLAY
#else
            batterAnim(1, "BAT_FLIP_" + Random.Range(1, 5), false);
#endif
        }

        /// <summary>
        /// 피칭뷰 헛스윙 애니메이션
        /// </summary>
        private void hutSwingAnimPv()
        {
            if (timing <= BattingTiming.TOO_EARLY && MyMath.Percent() < 25)
            {
                batterAnim(1, MyMath.Half()?"2010_EARLYSWING_02":"2010_EARLYSWING_OUT", false);                
            }
            else
            {
                if (courseIndex == 0)
                {
                    batterAnim(1, manager.nStrikeCount >= 2 ? "2010_EARLYSWING_OUT" : "2010_EARLYSWING_0"+Random.Range(1,3), false);
                }
                else if (courseIndex == 1)
                {
                    batterAnim(1, "INCOURSE_SWING_HEADUP", false);
                }
                else
                {
                    batterAnim(1, "OUTCOURSE_SWING_HEADUP", false);
                }
            }

            if (manager.nStrikeCount >= 2)
            {
                pAnim.state.AddAnimation(1, "5000_EVENT_LOOKING6", false, 1.5f);
            }
            strID = "";
        }


        /// <summary>
        /// 피칭뷰에서 타격 이펙트 처리
        /// </summary>
        /// <param name="bActive"></param>
        /// <param name="type"></param>
        /// <param name="bBroken"></param>
        /// <param name="earlyTiming"></param>
        public void setHitEffectPv(bool bActive, bool bGoodHit)
        {
            //////UnityEngine.//Debug.Log("===============================>> !!!!!!!!!!!!!!!!!!!!!!!!!!! setHitEffect");
            if (bActive == true)
            {
                float offsetX = 0;
                float offsetY = 0;

                if (pitcher.missType != 1)
                {
                    offsetX = -pitcher.preArriveX;
                    offsetY = pitcher.preArriveY;
                }

                effectX = (pitcher.pitchHand == CPlayer._LEFTHAND ? -136 : 124) + offsetX;
                effectY = 203 + offsetY;
                if (bGoodHit == false)
                {
                    HitEffect.SetHitEffect("SMALL_HIT", effectX, effectY, 110);
                }
                else
                {
                    if (bHitHomeRun == true)
                    {
                        //홈런타
                        HitEffect.SetColor(new Color(1,1,1,0.8f)); //0.5f
                        HitEffect.SetHitEffect("HOMERUN_HIT", effectX, effectY,80);// 60);
                        battingview.setJustMeet(true, "hitfocus", 1.2f, 0.5f);
                    }
                    else
                    {
                        HitEffect.SetHitEffect("MIDDLE_HIT", effectX, effectY, 60); //90
                        battingview.setJustMeet2(true, "hitfocus2", new Vector3(1.3f, 1.5f, 1), new Vector3(0, 600, -0.5f), new Color(1, 1, 1, 0.8f));
                    }

                }
            }
            else
            {
                battingview.setJustMeet(false);
                if (bHitHomeRun == true)
                {
                    //CameraManager.SetScreenOverlay(false);                    
                    bHitHomeRun = false;
                }
            }
        }
        
        
        
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        //타자의 능력치
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 기본능력치 - 타자의 컨택값을 얻어온다.
        /// </summary>
        private int getContactValue()
        {
            return (int)(pBatter.getContact());// + pBatter.getConBonus()));
        }

        /// <summary>
        /// 기본능력치 - 타자의 파워값을 얻어온다.
        /// </summary>
        private int getPowerValue()
        {
            return (int)(pBatter.getPower());// + pBatter.getPowerBonus()));
        }

        /// <summary>
        /// 기본능력치 - 타자의 선구값을 얻어온다.
        /// </summary>
        private int getEyeValue()
        {
            return (int)(pBatter.getEye());// + pBatter.getEyeBonus()));
        }

        /// <summary>
        /// 기본능력치 - 타자의 탄도값을 얻어온다.
        /// </summary>
        private int getTandoValue()
        {
            return pBatter.getTando();// + pBatter.getTandoBonus();
        }

        /// <summary>
        /// 파생능력치 - 배트 사이즈 얻어오기
        /// </summary>
        public void setBatRealSize()
        {
            float batRealSizeCoef = 0.5f + (bContact * 0.0006f); //float batRealSizeCoef = 0.5f + (getContactValue() * 0.0006f);            

            batRealSizeX = batRealSizeCoef * BattingMechanism.BAT_SIZEX;
            batRealSizeY = batRealSizeCoef * BattingMechanism.BAT_SIZEY;

            batGangtaSize = batRealSizeCoef * BattingMechanism.GANGTA_BATSIZE;

            //0~150
            //0.5 ~ 1.4
            if (manager.bMyTurn == true)
            {
                zoneUI.setBatterSize(batRealSizeCoef);
            }

        }

        ///////////////////////////////////////////////////////////////////////
        //타구의 벡터
        ////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 타구속도
        /// </summary>        
        public float getBatterBasicPower()
        {
            //맥스파워
            float curPower = BattingMechanism.getBatterMaxPower(bPower, pitcher.pFinalGuwee);
            //맥스 파워 설정
            if (curPower > maxPower) curPower = maxPower;
            //Debug.Log("===> curPower = " + curPower + "  powerCoef1 = " + powerCoef1 + "   powerCoef2 = " + powerCoef2 + "           powerCoef3 = " + powerCoef3);            
            //파워 계수 곱함
            curPower = curPower * powerCoef1 * powerCoef2 * powerCoef3;
            //Debug.Log("===> FinalPower = " + curPower);            
            return curPower;
        }

        /// <summary>
        /// 타구의 최종 각도 (Z앵글)값을 얻어온다.
        /// </summary>
        public float getAngleZ()
        {
            float angleZ = 0;

            if (bBadballSwingHigh == true)
            {
                return Random.Range(50.0f, 88.0f);
            }
            else if (bBadballSwingLow == true)
            {
                return Random.Range(-40.0f, -30.0f);
            }
                        

            //신버전
            if (battingOffsetY >= 5.0f)
            {    
                //플라이볼 형태
                //배트위에 맞은 경우 -> 탄도가 높을수록 더 뜸
                if (timing <= BattingTiming.EARLY)
                {
                    angleZ = 10.0f + (battingOffsetY * 0.7f * flyTandoRate);
                }
                else if (timing == BattingTiming.JUST_EARLY)
                {
                    angleZ = 20.0f + (battingOffsetY * 0.7f * flyTandoRate);
                }
                else if (timing == BattingTiming.JUST_LATE)
                {
                    angleZ = 35.0f + (battingOffsetY * 0.7f * flyTandoRate * 1.35f);
                }
                else if (timing >= BattingTiming.LATE)
                {
                    angleZ = 35.0f + (battingOffsetY * 0.7f * flyTandoRate * 2f);
                }
                else //퍼펙트
                {
                    angleZ = 35.0f + (battingOffsetY * 0.7f * flyTandoRate);
                }
            }
            else if (battingOffsetY >= -5.0f)
            {
                //그라운드볼 형태
                //배트위에 맞은 경우 -> 탄도가 높을수록 더 뜸
                if (timing <= BattingTiming.EARLY)
                {
                    angleZ = -10 + (battingOffsetY * linerTandoRate);
                }
                else if (timing == BattingTiming.JUST_EARLY)
                {
                    angleZ = 0 + (battingOffsetY * linerTandoRate);
                }
                else if (timing == BattingTiming.JUST_LATE)
                {
                    angleZ = 15 + (battingOffsetY * linerTandoRate* 1.4f);
                }
                else if (timing >= BattingTiming.LATE)
                {
                    angleZ = 15 + (battingOffsetY * linerTandoRate * 2.0f);
                }
                else
                {
                    angleZ = 15.0f + (battingOffsetY * linerTandoRate);
                }
            }
            else
            {
                if (timing <= BattingTiming.EARLY)
                {
                    angleZ = -25.0f + (battingOffsetY * 0.3f);
                }
                else if (timing == BattingTiming.JUST_EARLY)
                {
                    angleZ = -15.0f + (battingOffsetY * 0.3f);
                }
                else if (timing == BattingTiming.JUST_LATE)
                {
                    angleZ = -10.0f + (battingOffsetY * 0.6f);
                }
                else if (timing >= BattingTiming.LATE)
                {
                    angleZ = -20.0f + (battingOffsetY * 0.6f);
                }
                else
                {
                    angleZ = -10.0f + (battingOffsetY * 0.2f);
                }
            }

            if (bHighBatting == true)
            {
                angleZ += Random.Range(5, 15);
            }
            else if (bLowBatting == true)
            {
                angleZ -= Random.Range(5, 20);
            }

            // 범위 제한
            if (angleZ > 70)
            {
                float gabZ = (angleZ - 70) * 0.333334f;
                angleZ = 70.0f + gabZ;
                if (angleZ > 89) angleZ = 89;
            }
            else if (angleZ < -30.0f)
            {
                float gabZ = (angleZ + 30) * 0.333334f;
                angleZ = -30 + gabZ;
                if (angleZ < -45) angleZ = -45;
            }
            ////UnityEngine.//Debug.Log("========================================>> angleZ = " + angleZ);
            return getTandoAdust(angleZ);
        }

        /// <summary>
        /// 탄도의 힘을 느끼게 해주는 아주 중요한 조정함수
        /// </summary>
        /// <param name="curAngle"></param>
        /// <returns></returns>
        private float getTandoAdust(float curAngle)
        {
            float adjustAngleValue = curAngle;

            ////Debug.Log("========================================>> 초기앵글 = " + curAngle);

            if (noTandoEffect == false)
            {
                if (field.ballPower > 30 &&
                   (curAngle > 24 && curAngle <= 55))
                {
                    //이영역에 들어가면 홈런 기회 박탈 당함
                    if (curAngle < 38 || battingOffsetY < 10)
                    {
                        if (battingOffsetY < 7.0f)
                        {
                            adjustAngleValue = (-15.0f + battingOffsetY);
                        }
                        else
                        {
                            adjustAngleValue = (10.0f + battingOffsetY);
                        }
                    }
                    else
                    {
                        adjustAngleValue = (48.0f + battingOffsetY);
                    }
                    ////Debug.Log("========================================>> setTandoAdust Case  배팅커서옵셋Y = " + battingOffsetY + "    새로운 값 = " + adjustAngleValue);
                }
            }


            if (field.ballPower > 35.0f && adjustAngleValue > 50.0f)
            {
                //강력한 고각 하이 플라이볼 파워 제한
                adjustAngleValue = Random.Range(32.0f, 35.0f);
            }

            return adjustAngleValue;
        }


        /// <summary>
        /// angleZ에 영향을 주는 탄도 초기 값들을 세팅한다
        /// </summary>
        private bool noTandoEffect; //탄도값이 높아야 true값이 되고 true값이 되야 홈런 방해 안받음
        private float linerTandoRate, linerTandoRate2, flyTandoRate, flyTandoRate2;
        private void setTando()
        {
            int value = bTando;// getTandoValue();
            //탄도가 높을 수록 높음
            linerTandoRate = 0.2f + (value * 0.000375f);          //기준값 0.5
            flyTandoRate = 0.7f + (value * 0.000375f);            //기준값 1.0

            //탄도가 높을수록 낮음
            linerTandoRate2 = 0.45f - (value * 0.0001875f);         //기준값 0.3
            flyTandoRate2 = 0.75f - (value * 0.0003125f);           //기준값 0.5


            int range = Random.Range(0, BattingMechanism.TANDO_RANGE);  //0~3000
            noTandoEffect = (range < value ? true : false); 
            ////Debug.Log("==============>> 탄도값 " + value + "  탄도 레인지 " + range + "  noTandoEffect = " + noTandoEffect);

            if (Mode.b2outBaseLoadedMode == true)
            {
                //탄도방해 안받음
                noTandoEffect = true;
            }

            ////UnityEngine.//Debug.Log("========================================>> 라이너 탄도 관련   linerTandoRate = " + linerTandoRate + " ====>> linerTandoRate2 = " + linerTandoRate2);
            ////UnityEngine.//Debug.Log("========================================>> 라이너 탄도 관련   flyTandoRate = " + flyTandoRate + " ====>> flyTandoRate2 = " + flyTandoRate2);
        }


        //타구의 타이밍 보너스 값 세팅 
        private float maxPower = 45;
        private void setBattingValue()
        {
            //배팅 값을 미리 세팅한다.
        
            int value = bPower;// getPowerValue();
            maxPower = 26.0f + (0.01f * value) + Random.Range(-2.0f, 2.0f);            
        }

        //타구의 기본 방향 (X앵글) 설정
        private bool babibSin;          //바빕신이 true이면 유리한쪽으로
        private float[] _basicFlyAngle = new float[8];
        private float[] _basicGrounderAngle = new float[8];
        private void setDirection()
        {
            babibSin = (MyMath.Percent() < BattingMechanism.BABIB_SIN ? true : false);
            ////UnityEngine.//Debug.Log("======================================>>바빕신 " + babibSin);
            bool bWrist = (MyMath.Percent() < BattingMechanism.WRIST_USE ? true : false);   //손목 사용
            //바운드 앵글
            if (babibSin == true)
            {
                //바빕신의 가호
                _basicGrounderAngle[(int)BattingTiming.PERFECT] = Random.Range(-13.0f, 13.0f);
                _basicGrounderAngle[(int)BattingTiming.JUST_EARLY] = sign * (bWrist == false ? Random.Range(10.0f, 30.0f) : Random.Range(40.0f, 45.0f));
                _basicGrounderAngle[(int)BattingTiming.JUST_LATE] = -sign * (bWrist == false ? Random.Range(10.0f, 30.0f) : Random.Range(40.0f, 45.0f));
                _basicGrounderAngle[(int)BattingTiming.EARLY] = sign * (bWrist == false ? Random.Range(15.0f, 42.0f) : Random.Range(0.0f, 15.0f));
                _basicGrounderAngle[(int)BattingTiming.LATE] = -sign * (bWrist == false ? Random.Range(15.0f, 42.0f) : Random.Range(0.0f, 15.0f));
            }
            else
            {
                //바빕신의 가호 못받음
                _basicGrounderAngle[(int)BattingTiming.PERFECT] = (MyMath.Half() ? 1 : -1) * Random.Range(22.0f, 6.0f);
                _basicGrounderAngle[(int)BattingTiming.JUST_EARLY] = sign * (bWrist == false ? Random.Range(10.0f, 22.0f) : Random.Range(30.0f, 41.0f));
                _basicGrounderAngle[(int)BattingTiming.JUST_LATE] = -sign * (bWrist == false ? Random.Range(10.0f, 22.0f) : Random.Range(30.0f, 41.0f));
                _basicGrounderAngle[(int)BattingTiming.EARLY] = sign * (bWrist == false ? Random.Range(28.0f, 43.0f) : Random.Range(9.0f, 22.0f));
                _basicGrounderAngle[(int)BattingTiming.LATE] = -sign * (bWrist == false ? Random.Range(28.0f, 43.0f) : Random.Range(9.0f, 22.0f));
            }
            _basicGrounderAngle[(int)BattingTiming.TOO_EARLY] = sign * (bWrist == false ? Random.Range(35.0f, 48.0f) : Random.Range(14.0f, 16.0f));  //too early
            _basicGrounderAngle[(int)BattingTiming.TOO_LATE] = -sign * (bWrist == false ? Random.Range(35.0f, 48.0f) : Random.Range(14.0f, 16.0f)); //too late
            _basicGrounderAngle[(int)BattingTiming.VERY_LATE] = Random.Range(43.0f, 50.0f);


            //플라이 앵글
            if (babibSin == true)
            {
                //바빕신의 가호
                _basicFlyAngle[(int)BattingTiming.PERFECT] = (bWrist == false ? Random.Range(-22.0f, 22.0f) : (MyMath.Half() ? 1 : -1) * Random.Range(30.0f, 42.0f));        //perfect  
                _basicFlyAngle[(int)BattingTiming.JUST_EARLY] = sign * (bWrist == false ? Random.Range(0, 22) : Random.Range(30.0f, 45.0f));    //just early
                _basicFlyAngle[(int)BattingTiming.JUST_LATE] = -sign * (bWrist == false ? Random.Range(0, 22) : Random.Range(30.0f, 45.0f));    //just late
                _basicFlyAngle[(int)BattingTiming.EARLY] = sign * (bWrist == false ? Random.Range(20, 45) : Random.Range(10, 25));              //early -> 파워 28이상 hook
                _basicFlyAngle[(int)BattingTiming.LATE] = -sign * (bWrist == false ? Random.Range(20, 45) : Random.Range(10, 25));              //late -> 파워 28이상 slice
            }
            else
            {
                //바빕신의 가호 못받음
                _basicFlyAngle[(int)BattingTiming.PERFECT] = (bWrist == false ? Random.Range(-12.0f, 12.0f) : (MyMath.Half() ? 1 : -1) * Random.Range(20.0f, 33.0f));        //perfect  
                _basicFlyAngle[(int)BattingTiming.JUST_EARLY] = sign * (bWrist == false ? Random.Range(13, 33) : Random.Range(25.0f, 37.0f));   //just early
                _basicFlyAngle[(int)BattingTiming.JUST_LATE] = -sign * (bWrist == false ? Random.Range(13, 33) : Random.Range(25.0f, 37.0f));   //just late
                _basicFlyAngle[(int)BattingTiming.EARLY] = sign * (bWrist == false ? Random.Range(23, 33) : Random.Range(35.0f, 45.0f));        //early -> 파워 28이상 hook
                _basicFlyAngle[(int)BattingTiming.LATE] = -sign * (bWrist == false ? Random.Range(23, 33) : Random.Range(35.0f, 45.0f));        //late -> 파워 28이상 slice
            }

            _basicFlyAngle[(int)BattingTiming.TOO_EARLY] = sign * Random.Range(15, 45);   //too early  //강제 바운드
            _basicFlyAngle[(int)BattingTiming.TOO_LATE] = -sign * Random.Range(-45, 0);   //too late //강제 팝업
            _basicFlyAngle[(int)BattingTiming.VERY_LATE] = -sign * Random.Range(-60, -20);   //too late //강제 팝업Random.Range(-50.0f, 50.0f);
        }

        /// <summary>
        /// 타구의 최종 방향 (X앵글)값을 얻어온다.
        /// </summary>   

        //bool bMustFoul = false;
        public float getBatterBasicDirection()
        {
            if (MyMath.Percent() < 3 && timing != BattingTiming.PERFECT)
            {
                field.ball.angleZ = Mathf.Clamp(field.ball.angleZ += 30, 25, 50);
                return -sign * Random.Range(130.0f, 180.0f);
            }

            if (bBadballSwingFar == true)
            {
                //return -sign * Random.Range(30.0f, 180.0f);
                return -sign * Random.Range(20.0f, 80.0f);
            }
            /*else if (bBadballSwingNear == true)
            {
                //return sign * Random.Range(50.0f, 90.0f);
                return sign * Random.Range(40.0f, 80.0f);
            }*/

            int _timing = (int)timing;

            int foulPer = MyMath.Percent();
            if (timing <= BattingTiming.EARLY)
            {
                if (foulPer < 15)
                {
                    return sign * Random.Range(55.0f, 90.0f);
                }
            }
            else if (timing >= BattingTiming.LATE)
            {
                if (foulPer < 25)
                {
                    return -sign * Random.Range(55.0f, 120.0f);
                }
            }

            float angleX;
            if(battingOffsetY < 0)  //(manager.battingResultData.hitType == SimulHitType.Grounder)
            {
                angleX = _basicGrounderAngle[(int)timing];
            }
            else
            {
                angleX = _basicFlyAngle[(int)timing];
            }

            float value = 0;
            if (babibSin == true)
            {
                value = (battingOffsetX / 8.0f);
                angleX += value;
            }
            else
            {
                if (battingOffsetX > 0)
                {
                    value = (15.0f - battingOffsetX) / 5.0f;
                    angleX += Random.Range(0, value);
                }
                else if (battingOffsetX < 0)
                {
                    value = (-15 - battingOffsetX) / 5.0f;
                    angleX += Random.Range(0, value);
                }
                else
                {
                    value = Random.Range(-4.0f, 4.0f);
                    angleX += value;
                }
            }

            if (bBadballSwingNear == true)
            {
                //배트 안쪽에 맞는 경우 무조건 밀리게
                angleX = Mathf.Abs(angleX) * -sign;
            }
            else
            {
                if (courseIndex == 1)
                {
                    //안쪽공
                    if (sign == 1)
                    {
                        if (angleX < 0) angleX = -angleX;
                    }
                    else
                    {
                        if (angleX > 0) angleX = -angleX;
                    }
                }
                else if (courseIndex == 2)
                {
                    //바깥
                    if (sign == 1)
                    {
                        if (angleX > 0) angleX = -angleX;
                    }
                    else
                    {
                        if (angleX < 0) angleX = -angleX;
                    }
                }
            }

            if (bFarBatting == true)
            {
                angleX -= (sign * Random.Range(8, 15));
            }
            else if (bNearBatting == true)
            {
                angleX += (sign * Random.Range(8, 15));
            }

            return angleX;
        }

#if DEATHMATCH_OLD_VERSION
        public void batterSkip()
        {
            bState = BatterState._NONE;
        }

#endif


    }
}
