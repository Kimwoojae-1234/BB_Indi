#define _NO_CROWD
//#define _TIME_CHANGE_TEST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public enum CrowdState
    {
        Normal,
        Homerun
    }

    public enum BackGroundType
    {
        Jamsil = 1,
        Dome = 2,
        LionsPark = 3,
        ChamionsField = 4,
        Hanhwa = 5,
        HappyDream = 6
    }

    public class BackGroundManager : MonoBehaviour
    {
        public enum TimeState
        {
            Day = 0,
            Evening = 1,
            Night = 2,
        }


        private static BackGroundManager Instance_;
        
        private CrowdState lastState;
        private int animCount;
        private bool crowdAnimChange;

        //private List<SkeletonAnimation> crowdList = new List<SkeletonAnimation>();
        private List<tk2dSprite> crowdList = new List<tk2dSprite>();
        private int totalCrowdCount;

        public Transform crowdTransform;
        public SkeletonAnimation backEffectAnim;
        public SkeletonAnimation lightEffectAnim;
        public SkeletonAnimation solarLightEffectAnim;


        public tk2dSprite sky1, sky2;//, skyup;     
   
        //public tk2dSprite field1, field2, building;
        //public SkeletonAnimation balloon1, balloon2;
        public tk2dSprite[] fieldSpr;
        public tk2dSprite[] buildingSpr;
        public SkeletonAnimation[] skelObj;

        public GameObject leftLight, rightLight;

        private float gameTime;
        private TimeState curTimeState, visulTimeState;
        private bool bTimeChange;

        private BackGroundType groundType;
        private float solarX, solarY;

        private BallPlayManager manager;


        private const int dayTime = 0;
        private const int eveningTime = 5;
        private const int nightTime = 10;
        
        void Awake()
        {
            Instance_ = this;
            lastState = CrowdState.Normal;
            groundType = (BackGroundType)(Mode.stadiumType);
        }


        void OnDestroy()
        {
            Instance_ = null;
        }


        public void init(BallPlayManager _manager)
        {
            this.manager = _manager;

            groundType = (BackGroundType)(Mode.stadiumType);

#if GIRL_PLAY
            lightEffectAnim.gameObject.SetActive(false);
            solarLightEffectAnim.gameObject.SetActive(false);

#else
            if (groundType != BackGroundType.Dome)
            {
                solarX = solarLightEffectAnim.transform.localPosition.x;
                solarY = solarLightEffectAnim.transform.localPosition.y;
            }
            

            if (groundType != BackGroundType.LionsPark) //라이온즈 파크 예외처리
            {
                if (backEffectAnim != null)
                {
                    //backEffectAnim.skeleton.SetToSetupPose();
                    //backEffectAnim.state.SetAnimation(0, (groundType == BackGroundType.Dome ? "DOME_01" : "STADIUM_01"), true);
                }
            }





            if (Mode.crowdAnimMode == false)
            {
                if (crowdTransform != null)
                {
                    Destroy(crowdTransform.gameObject);
                }
            }
            else
            {
                crowdAnimChange = false;
                crowdList.Clear();
                totalCrowdCount = 0;

                if (crowdTransform != null)
                {
                    //스프라이트랑 섞인 경우
                    int crowdPer = Mode.crowdPer;
                    foreach (Transform trans in crowdTransform)
                    {
                        if (MyMath.Percent() < crowdPer)
                        {
                            float z = -0.05f - ((200 - trans.localPosition.y) * 0.001f);
                            trans.localPosition += new Vector3(0, 0, z);

                            tk2dSprite c = trans.gameObject.GetComponent<tk2dSprite>();
                            if (c != null)
                            {
                                crowdList.Add(c);
                                totalCrowdCount++;
                            }
                            else
                            {
                                Destroy(trans.gameObject);
                            }
                        }
                        else
                        {
                            Destroy(trans.gameObject);
                        }
                    }
                }
            }
#endif

        }

        void OnDisable()
        {
            setBackScreen(); 
            StopAllCoroutines();
        }

        void Update()
        {
            if (crowdAnimChange == true)
            {
                crowdStateUpdate();
            }

            if (bTimeChange == false)
            {
#if GIRL_PLAY
#else
                if (curTimeState == TimeState.Evening)
                {
                    updateEvening();
                }
                else if (curTimeState == TimeState.Night)
                {
                    updateNight();
                }
#endif
            }

        }


        public static void SetDisplayEffect(string effect)
        {
            Instance_.setDisplayEffect(effect);
        }

        public static void SetCrowdColor(Color color)
        {
            Instance_.setCrowdColor(color);
        }

        public static void SetInitTime(float curTime)
        {
            Instance_.setInitTime(curTime);
        }

        public static void SetTime(bool bSimul = false)
        {
            Instance_.setTime(bSimul);
        }

        public static TimeState GetTimeState()
        {
            return Instance_.curTimeState;
        }

        //백그라운드의 시간이 업데이트 중인지 체크
        public static bool IsUpdating()
        {
            return (!Instance_.bTimeChange);
        }

        //백그라운드 빌딩의 컬러 틴트값을 리턴
        public static Color GetBuildingColor()
        {
            return new Color(Instance_.r1, Instance_.g1, Instance_.b1);
        }

        //백그라운드 필드의 컬러 틴트값을 리턴
        public static Color GetFieldColor()
        {
            return new Color(Instance_.r2, Instance_.g2, Instance_.b2);
        }

        //백그라운드 업데이트 카운트값을 리턴
        public static int GetUpdateCount()
        {
            return Instance_.count;
        }


        //시간변화
        public static void SetTimeChange(float curTime)
        {
            Instance_.gameTime = curTime;
        }


        //시간 업데이트
        public static void UpdateTime(int inning, bool bChanceMode)
        {
            Instance_.updateTime(inning, bChanceMode);
        }



        private void setInitTime(float curTime)
        {
          
            groundType = (BackGroundType)(Mode.stadiumType);

            gameTime = curTime;
            bTimeChange = true;

            if(groundType != BackGroundType.Dome)
            {
                //돔구장이 아닌 경우 시간영향
                if (gameTime < eveningTime)
                {
                    setDayState();
                }
                else if (gameTime < nightTime)
                {
                    setEveningState();
                }
                else
                {
                    setNightState();
                }
            }
            else
            {
                //셰이더 초기화
                //manager.batter.changeShader("Spine/Skeleton");
            }
        }

        /// <summary>
        /// 업데이트 타임
        /// </summary>
        /// <param name="inning"></param>
        private void updateTime(int inning, bool bChanceMode)
        {
            if (groundType != BackGroundType.Dome)
            {
                if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
                {
                    //9회 투아웃 모드
                    if (inning == 4) BackGroundManager.SetTimeChange(eveningTime);
                    else if (inning == 7) BackGroundManager.SetTimeChange(nightTime);
                }
                else
                {
                    if (bChanceMode == false && manager.bMyTurn == false) return;

                    if (inning < 5) //if (inning == 1)//
                    {
                        BackGroundManager.SetTimeChange(dayTime);
                    }
                    else if (inning < 7) //else if (inning == 2)//
                    {
                        //5회에 저녁으로 접어들음
                        bTimeChange = true;
                        BackGroundManager.SetTimeChange(eveningTime);
                    }
                    else
                    {
                        //7회저녁
                        bTimeChange = true;
                        BackGroundManager.SetTimeChange(nightTime);
                    }

                    if (manager.bMyTurn == false)
                    {
                        //피처뷰 시간 바뀜처리
                        if (gameTime < eveningTime) curTimeState = TimeState.Day;
                        else if (gameTime < nightTime) curTimeState = TimeState.Evening;
                        else curTimeState = TimeState.Night;
                        manager.battingview.pitcherviewTimeCheck();
                    }
                }
            }
        }


        private void setTime(bool bSimul)
        {
            if (Mode.cameraView != CameraView.PitcherCenter)
            {
                if (bTimeChange == true)
                {
                    gameTime += 0.6f;// 0.2f;// 0.1f;// 0.2f;
                    if(groundType != BackGroundType.Dome)
                    {
                        if (visulTimeState != curTimeState)
                        {
                            if (curTimeState == TimeState.Night) setNightState();
                            else if (curTimeState == TimeState.Night) setEveningState();
                            else setDayState();
                        }
                        else
                        {
                            //돔구장이 아닌 경우 시간영향
                            if (curTimeState == TimeState.Day)
                            {
                                if (gameTime >= nightTime)
                                {
                                    setEveningState();
                                    curTimeState = TimeState.Night;
                                    visulTimeState = TimeState.Night;
                                    if (bSimul == false)
                                    {
                                        initNight();
                                    }
                                }
                                else if (gameTime >= eveningTime)
                                {
                                    curTimeState = TimeState.Evening;
                                    visulTimeState = TimeState.Evening;
                                    if (bSimul == false)
                                    {
                                        initEvening();
                                    }
                                }
                            }
                            else if (curTimeState == TimeState.Evening)
                            {
                                if (gameTime >= nightTime)
                                {
                                    curTimeState = TimeState.Night;
                                    visulTimeState = TimeState.Night;
                                    if (bSimul == false)
                                    {
                                        initNight();
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }


        private void setState(CrowdState state)
        {
            if (lastState != state)
            {
                lastState = state;
                animCount = 0;
                crowdAnimChange = true;
            }
        }

        private void crowdStateUpdate()
        {   
            if (Mode.cameraView == CameraView.PitcherCenter) return;

            /*
            if (Mode.crowdAnimMode == true)
            { 
                if (lastState == CrowdState.Normal)
                {
                    crowdList[animCount].setNormal();
                }
                else
                {
                    crowdList[animCount].setCheerUp();
                }
                if (++animCount >= totalCrowdCount)
                {
                    crowdAnimChange = false;
                    System.GC.Collect();
                }
            }*/
        }

        private IEnumerator setNormalBack()
        {
            yield return new WaitForSeconds(5.0f);
            setBackScreen();

            if (groundType == BackGroundType.Dome) 
            {
                //돔구장인 경우
                lightEffectAnim.state.ClearTracks();
                lightEffectAnim.skeleton.SetToSetupPose();
                lightEffectAnim.state.SetAnimation(0, "DOME_01", true);
            }

            yield return new WaitForSeconds(5.0f);            

            setState(CrowdState.Normal);
        }

        private void setBackScreen()
        {
            //라이온즈 파크 예외처리
            /*if (groundType == BackGroundType.LionsPark) return;
            if (backEffectAnim != null)
            {
                backEffectAnim.state.ClearTracks();
                backEffectAnim.skeleton.SetToSetupPose();
                backEffectAnim.state.SetAnimation(0, (groundType == BackGroundType.Dome ? "DOME_01" : "STADIUM_01"), true);
            }*/
        }


        private void setDisplayEffect(string effect)
        {
            if (Mode.cameraView != CameraView.PitcherCenter)
            {
#if GIRL_PLAY
#else

                //라이온즈 파크 예외처리
                if (groundType == BackGroundType.LionsPark) return;

                backEffectAnim.GetComponent<Renderer>().enabled = true;
                backEffectAnim.state.ClearTracks();
                backEffectAnim.skeleton.SetToSetupPose();
                backEffectAnim.state.SetAnimation(1, effect, false);

                if (groundType == BackGroundType.Dome) 
                {
                    //돔구장인 경우
                    lightEffectAnim.state.ClearTracks();
                    lightEffectAnim.skeleton.SetToSetupPose();
                    lightEffectAnim.state.SetAnimation(0, "DOME_02", true);
                }
                setState(CrowdState.Homerun);
                StartCoroutine(setNormalBack());
#endif
            }
        }


        private void setCrowdColor(Color color)
        {
            if (Mode.crowdAnimMode == true)
            {
                for (int i = 0; i < totalCrowdCount; i++)
                {
                    crowdList[i].color = color;// .skeleton.SetColor(color);
                }
            }
        }

        float r1, g1, b1, r2, g2, b2;
        float dr1, dg1, db1, dr2, dg2, db2;
        float alphagab;
        float curTime;
        int count;

        private void initEvening()
        {
            bTimeChange = false;
#if GIRL_PLAY
#else

            r1 = g1 = b1 = r2 = g2 = b2 = 1;

            //건물
            dr1 = (202.0f - 255.0f) / (255.0f * 60.0f);
            dg1 = (189.0f - 255.0f) / (255.0f * 60.0f);
            db1 = (170.0f - 255.0f) / (255.0f * 60.0f);

            dr2 = (255.0f - 255.0f) / (255.0f * 60.0f);
            dg2 = (230.0f - 255.0f) / (255.0f * 60.0f);
            db2 = (191.0f - 255.0f) / (255.0f * 60.0f);


            sky2.gameObject.SetActive(true);
            sky2.color = new Color(1, 1, 1, 1);

            

            sky1.spriteId = sky1.GetSpriteIdByName("sky0");
            sky2.spriteId = sky1.GetSpriteIdByName("sky1");

            //skyup.spriteId = skyup.GetSpriteIdByName("skyup1");

            alphagab = 1.0f / 60.0f;

            leftLight.gameObject.SetActive(true);
            rightLight.gameObject.SetActive(true);
            
            if (groundType == BackGroundType.Jamsil)
            {
                //잠실 예외처리
                lightEffectAnim.gameObject.SetActive(true);
                lightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 0));
            }
            else
            {
                leftLight.GetComponent<SkeletonAnimation>().skeleton.SetColor(new Color(1,1,1,0));
                rightLight.GetComponent<SkeletonAnimation>().skeleton.SetColor(new Color(1, 1, 1, 0));
            }
#endif
            curTime = 0;
            count = 0;
        }

        private void updateEvening()
        {
            curTime += Time.deltaTime;
            if (curTime >= 0.4f)
            {
                //구조물과 관중
                r1 += dr1;
                g1 += dg1;
                b1 += db1;
                Color curColor1 = new Color(r1, g1, b1);
                for (int i = 0; i < buildingSpr.Length; i++) buildingSpr[i].color = curColor1;
                setCrowdColor(curColor1);
                for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

                //필드
                r2 += dr2;
                g2 += dg2;
                b2 += db2;
                Color curColor2 = new Color(r2, g2, b2);
                for(int i = 0; i< fieldSpr.Length;i++) fieldSpr[i].color = curColor2;

                //하늘
                sky1.color = new Color(1, 1, 1, 1 - (alphagab * count));

                Color gradColoar = new Color(1, 1, 1, alphagab * count);
                if (groundType == BackGroundType.Jamsil)
                {
                    lightEffectAnim.skeleton.SetColor(gradColoar);
                }
                else
                {
                    leftLight.GetComponent<SkeletonAnimation>().skeleton.SetColor(gradColoar);
                    rightLight.GetComponent<SkeletonAnimation>().skeleton.SetColor(gradColoar);
                }
                
                //태양광
                if (count == 30)
                {
                    FieldCrowdManager.SetActive(true);
                    FieldCrowdManager.ChangeTime(TimeState.Evening);
                    FieldCrowdManager.SetActive(false);
                    //타자 셰이더 교환
                    //manager.batter.changeShader("Spine/SkeletonGraphic (Premultiply Alpha)");

                    solarLightEffectAnim.state.ClearTracks();
                    solarLightEffectAnim.skeleton.SetToSetupPose();
                    if (groundType == BackGroundType.LionsPark)
                    {
                        solarLightEffectAnim.state.SetAnimation(0, "SAMSUNG_LIGHT_04", true); //노을
                    }                    
                    else
                    {
                        if (groundType == BackGroundType.HappyDream)
                        {
                            solarX = 0;
                            solarY = -388;
                            solarLightEffectAnim.transform.localPosition = new Vector3(100, 100, 100);
                        }
                        solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_LIGHT_03", true); //노을
                    }
                    solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 0));
                    solarLightEffectAnim.transform.localPosition = new Vector3(solarX, solarY, -2.1f);

                }

                if (count < 30)
                {
                    solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1 - (alphagab * count * 2)));
                }
                else if (count > 30)
                {
                    solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, (alphagab * (count-30) * 2)));
                }


                if (++count >= 60)
                {
                    sky1.gameObject.SetActive(false);
                    bTimeChange = true;
                    StartCoroutine(manager.battingview.setSideTimeChange(TimeState.Evening));
                }
                curTime = 0;
            }
        }


        private void initNight()
        {
            bTimeChange = false;

            r1 = 0.792f;
            g1 = 0.741f;
            b1 = 0.667f;
            r2 = 1;
            g2 = 0.902f;
            b2 = 0.749f;

            //건물
            dr1 = (0.624f - r1) / (60.0f);
            dg1 = (0.690f - g1) / (60.0f);
            db1 = (1.0f - b1) / (60.0f);

            //필드
            dr2 = (1.0f - r2) / (60.0f);
            dg2 = (1.0f - g2) / (60.0f);
            db2 = (1.0f - b2) / (60.0f);


            sky1.gameObject.SetActive(true);
            sky1.color = new Color(1, 1, 1, 0);

            sky2.spriteId = sky1.GetSpriteIdByName("sky1");
            sky1.spriteId = sky1.GetSpriteIdByName("sky2");
            //skyup.spriteId = skyup.GetSpriteIdByName("skyup2");

            alphagab = 1.0f / 60.0f;

            
            lightEffectAnim.gameObject.SetActive(true);            

            //라팍
            //lightEffectAnim.gameObject.SetActive(true); //별
            if (groundType == BackGroundType.LionsPark)
            {
                lightEffectAnim.transform.localPosition = new Vector3(0, -460, 9);
                lightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 0));
            }
            else if (groundType == BackGroundType.Hanhwa || groundType == BackGroundType.ChamionsField || groundType == BackGroundType.HappyDream)
            {
                lightEffectAnim.transform.localPosition = new Vector3(0, -500, 9);
                lightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 0));
            }
            else
            {
                lightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1));
            }
            
            curTime = 0;
            count = 0;
        }


        private void updateNight()
        {
            curTime += Time.deltaTime;
            if (curTime >= 0.4f)
            {
                //구조물과 관중
                r1 += dr1;
                g1 += dg1;
                b1 += db1;
                Color curColor1 = new Color(r1, g1, b1);
                for (int i = 0; i < buildingSpr.Length; i++) buildingSpr[i].color = curColor1;
                setCrowdColor(curColor1);
                for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

                //필드
                r2 += dr2;
                g2 += dg2;
                b2 += db2;
                Color curColor2 = new Color(r2, g2, b2);
                for (int i = 0; i < fieldSpr.Length; i++) fieldSpr[i].color = curColor2;

                //하늘
                
                Color skyColor = new Color(1, 1, 1, alphagab * count);
                sky1.color = skyColor;
                if (groundType != BackGroundType.Jamsil)
                {
                    lightEffectAnim.skeleton.SetColor(skyColor);
                }

                if (count == 30)
                {
                    FieldCrowdManager.SetActive(true);
                    FieldCrowdManager.ChangeTime(TimeState.Night);
                    FieldCrowdManager.SetActive(false);
                    //타자 셰이더 교환
                    //manager.batter.changeShader("Spine/Skeleton");
                    if (groundType == BackGroundType.Jamsil)
                    {
                        //잠실처리
                        solarLightEffectAnim.state.ClearTracks();
                        solarLightEffectAnim.skeleton.SetToSetupPose();
                        solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_NIGHT_01", true);
                        solarLightEffectAnim.transform.localPosition = new Vector3(-50, -550, 2);
                    }
                    else
                    {
                        //기타 처리
                        solarLightEffectAnim.gameObject.SetActive(false);
                        //lightEffectAnim.gameObject.SetActive(true); //별
                        //lightEffectAnim.transform.localPosition = new Vector3(0, -460, 9);
                    }

                }

                if (count < 30)
                {
                    solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1 - (alphagab * count * 2)));
                }                
                else if (count > 30)
                {
                    solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, (alphagab * (count - 30) * 2)));
                    if (groundType == BackGroundType.LionsPark)
                    {
                        //라팍 특수 처리
                        buildingSpr[3].color = new Color(0.23f, 0.33f, 0.82f);
                    }
                }

                if (++count >= 60)
                {
                    sky1.color = new Color(1, 1, 1, 1);
                    //lightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1));
                    sky2.gameObject.SetActive(false);
                    bTimeChange = true;
                    StartCoroutine(manager.battingview.setSideTimeChange(TimeState.Night));
                }
                curTime = 0;
            }
        }



        private void setDayState()
        {
            curTimeState = TimeState.Day;
            visulTimeState = TimeState.Day;
            FieldCrowdManager.ChangeTime(curTimeState);

            //타자 쉐이더
            //manager.batter.changeShader("Spine/Skeleton");            

            //구조물과 관중
            Color curColor1 = new Color(1, 1, 1);
            for (int i = 0; i < buildingSpr.Length; i++) buildingSpr[i].color = curColor1;
            setCrowdColor(curColor1);
            for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

            //필드
            Color curColor2 = new Color(1, 1, 1);
            for (int i = 0; i < fieldSpr.Length; i++) fieldSpr[i].color = curColor2;

            //하늘
            sky1.gameObject.SetActive(true);
            sky1.spriteId = sky1.GetSpriteIdByName("sky0");
            sky1.color = new Color(1, 1, 1, 1);
            sky2.gameObject.SetActive(false);
            //skyup.spriteId = skyup.GetSpriteIdByName("skyup0");
            
            //하늘광
            solarLightEffectAnim.gameObject.SetActive(true);
            solarLightEffectAnim.state.ClearTracks();
            solarLightEffectAnim.skeleton.SetToSetupPose();
            if (groundType == BackGroundType.LionsPark)
            {   
                solarLightEffectAnim.state.SetAnimation(0, "SAMSUNG_LIGHT_01", true);
                //solarLightEffectAnim.transform.localPosition = new Vector3(solarX, solarY, 8);
                solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 0.35f));
            }
            else if (groundType == BackGroundType.ChamionsField)
            {
                solarLightEffectAnim.state.SetAnimation(0, "KIA_LIGHT_01", true);
            }
            else if (groundType == BackGroundType.Hanhwa)
            {
                solarLightEffectAnim.state.SetAnimation(0, "HANWHA_LIGHT__01", true);
            }
            else if (groundType == BackGroundType.HappyDream)
            {
                solarLightEffectAnim.transform.localPosition = new Vector3(-6, 129, -2.1f);
                solarLightEffectAnim.transform.localScale = new Vector3(188, 195, 1);
                solarLightEffectAnim.state.SetAnimation(0, "SK_LIGHT_01", true);
            }
            else
            {
                solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_LIGHT_01", true);
            }

            //기타
            lightEffectAnim.gameObject.SetActive(false);
            leftLight.gameObject.SetActive(false);
            rightLight.gameObject.SetActive(false);

            if (manager.bMyTurn == true)
            {
                StartCoroutine(manager.battingview.setSideTimeChange(TimeState.Day));
            }
        }

        private void setEveningState()
        {
            curTimeState = TimeState.Evening;
            visulTimeState = TimeState.Evening;
            FieldCrowdManager.ChangeTime(curTimeState);

            //타자 쉐이더
            //manager.batter.changeShader("Spine/SkeletonGraphic (Premultiply Alpha)"); //manager.batter.changeShader("Spine/Bones");

            //구조물과 관중
            Color curColor1 = new Color(0.792f, 0.741f, 0.667f);
            for (int i = 0; i < buildingSpr.Length; i++) buildingSpr[i].color = curColor1;
            setCrowdColor(curColor1);
            for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

            //필드
            Color curColor2 = new Color(1, 0.902f, 0.749f);
            for (int i = 0; i < fieldSpr.Length; i++) fieldSpr[i].color = curColor2;

            //하늘
            sky2.gameObject.SetActive(true);
            sky2.spriteId = sky1.GetSpriteIdByName("sky1");
            sky2.color = new Color(1, 1, 1, 1);
            sky1.gameObject.SetActive(false);

            //하늘광
            solarLightEffectAnim.gameObject.SetActive(true);
            solarLightEffectAnim.state.ClearTracks();
            solarLightEffectAnim.skeleton.SetToSetupPose();
            if (groundType == BackGroundType.LionsPark)
            {
                solarLightEffectAnim.state.SetAnimation(0, "SAMSUNG_LIGHT_04", true); //노을
            }
            else
            {
                if (groundType == BackGroundType.HappyDream)
                {
                    solarX = 0;
                    solarY = -388;
                    solarLightEffectAnim.transform.localPosition = new Vector3(100, 100, 100);
                }
                solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_LIGHT_03", true);
            }
            solarLightEffectAnim.transform.localPosition = new Vector3(solarX, solarY, -2.1f);

            //기타
            lightEffectAnim.gameObject.SetActive(false);            
            leftLight.gameObject.SetActive(true);
            rightLight.gameObject.SetActive(true);

            StartCoroutine(manager.battingview.setSideTimeChange(TimeState.Evening));

            
        }

        private void setNightState()
        {
            curTimeState = TimeState.Night;
            visulTimeState = TimeState.Night;
            FieldCrowdManager.ChangeTime(curTimeState);

            //타자 쉐이더
            //manager.batter.changeShader("Spine/Skeleton");


            //구조물과 관중
            Color curColor1 = new Color(0.624f, 0.690f, 1.0f);
            for (int i = 0; i < buildingSpr.Length; i++) buildingSpr[i].color = curColor1;
            setCrowdColor(curColor1);
            for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

            //필드
            Color curColor2 = new Color(1, 1, 1);
            for (int i = 0; i < fieldSpr.Length; i++) fieldSpr[i].color = curColor2;

            //하늘
            sky1.gameObject.SetActive(true);
            sky1.spriteId = sky1.GetSpriteIdByName("sky2");
            sky1.color = new Color(1, 1, 1, 1);
            sky2.gameObject.SetActive(false);
            //skyup.spriteId = skyup.GetSpriteIdByName("skyup2");

            //하늘광            
            if (groundType == BackGroundType.Jamsil)
            {
                solarLightEffectAnim.gameObject.SetActive(true);
                solarLightEffectAnim.state.ClearTracks();
                solarLightEffectAnim.skeleton.SetToSetupPose();
                solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_NIGHT_01", true);
                solarLightEffectAnim.transform.localPosition = new Vector3(-50, -480, 2);
                //기타
                lightEffectAnim.gameObject.SetActive(true);
                lightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1));
            }
            else
            {
                solarLightEffectAnim.gameObject.SetActive(false);
                lightEffectAnim.gameObject.SetActive(true);
                if (groundType == BackGroundType.LionsPark)
                {
                    lightEffectAnim.transform.localPosition = new Vector3(0, -460, 9);
                }
                else if (groundType == BackGroundType.Hanhwa || groundType == BackGroundType.ChamionsField || groundType == BackGroundType.HappyDream)
                {
                    lightEffectAnim.transform.localPosition = new Vector3(0, -500, 9);
                }
                if (groundType == BackGroundType.LionsPark)
                {
                    //라팍 특수처리                    
                    buildingSpr[3].color = new Color(0.23f, 0.33f, 0.82f);                    
                }
            }

            //라이트 켬
            leftLight.gameObject.SetActive(true);
            rightLight.gameObject.SetActive(true);

            StartCoroutine(manager.battingview.setSideTimeChange(TimeState.Night));

        }


    }
}
