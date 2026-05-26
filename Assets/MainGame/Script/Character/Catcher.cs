//#define _NO_TEXTURE_LOADING       //지워지워

using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class Catcher : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Sign,
            SignWait,
            Ready,
            SideStep,
            Catch,
            BallHit,
            None,
        }

        private enum CatcherForm
        {
            Normal = 1,
            Down = 6,
            DownRight = 7,
            Up = 11,
            None,
        }

        private SkeletonAnimation anim;
        public SkeletonAnimation glove;

        public Transform glovePos, leftArmPos;


        private State state;
        private float curTime;
        private Pitcher pitcher;
        private int initPosX, initPosY;
        private int posX;
        private int gabX;

        private CatcherForm form;

        private bool bCatchResponse;

        void Start()
        {
            //투수는 여기 이전에 이미지 리소스 생성 필요
            //오직 시뮬레이션만 하는 경우에는 초기에 리소스를 설정해주지 않는다
            //배팅뷰용
            GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/pCatcher/pCatcherSkelPrefab", transform, Vector3.zero, "skeleton");
            skeleton.transform.localScale = new Vector3(80, 80, 1);
            anim = skeleton.GetComponent<SkeletonAnimation>();

            glove.transform.localPosition = Vector3.zero;
        }

        

        // Update is called once per frame
        void Update()
        {
            catcherFrame();
        }

        /// <summary>
        /// 캐처 프레임
        /// </summary>
        private void catcherFrame()
        {
            curTime += Time.deltaTime;
            if (state == State.Sign)
            {
                if (curTime > 2)
                {
                    setWait();
                }
            }
            else if (state == State.SignWait)
            {
                if (curTime > 2 && curTime <3)
                {
                    pitcher.signDisagreeAnim();
                    curTime = 3.5f;
                }
                else if (curTime > 5)
                {
                    setSign();
                }
            }
            else if (state == State.Ready)
            {
                gloveFrame();
            }
        }

        //투수 애니메이션
        public void catcherAnim(string strAnim, bool bLoop = false)
        {
            anim.state.ClearTracks();
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(0, strAnim, bLoop);
            anim.timeScale = 1.0f;
        }

        public void catcherAnim2(string strAnim, bool bLoop = false)
        {
            anim.state.ClearTrack(0);
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(1, strAnim, bLoop);
        }


        private void gloveAnim(string strAnim)
        {
            glove.state.ClearTracks();
            glove.skeleton.SetToSetupPose();
            glove.state.SetAnimation(0, strAnim, false);
            glove.timeScale = 1.0f;
        }

        private void gloveActive(bool bActive)
        {
            glove.GetComponent<MeshRenderer>().enabled = bActive; 
            leftArmPos.gameObject.SetActive(bActive);
        }


        //인스턴스 초기화
        public void initInstance(Pitcher _pitcher)
        {
            this.pitcher = _pitcher;
        }


        //텍스쳐 로딩
        private string lastPath = null;
        public void loadCatcher()
        {
#if _NO_TEXTURE_LOADING

#else
            int index = (pitcher.manager.bTopInning ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex);
                        
            string curPath = "MainGame/spineData/pitchingview/catcher/team/"+index+"/CATCHER";
            if (curPath != lastPath)
            {
                lastPath = curPath;
                AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
                Material[] materials = atlasdata.materials;
                materials[0].mainTexture = (Texture)Resources.Load(lastPath);
            }
#endif
        }


        /// <summary>
        /// 위치 초기화
        /// </summary>
        /// <param name="bLeftPitcher"></param>
        /// <param name="_parent"></param>
        public void initPosition(bool bLeftPitcher, GameObject _parent)
        {
            transform.parent = _parent.transform;
            transform.localScale = Vector3.one;
            initPosX = (bLeftPitcher ? -155 : 120);
            initPosY = 392;
            //int depth = -2;            
            setIdle();
        }

        
        /// <summary>
        /// 아이들 상태
        /// </summary>
        public void setIdle()
        {
            bFormChangeAvail = true;
            posX = initPosX;
            gabX = 0;
            transform.localPosition = new Vector3(initPosX, initPosY, -2);

            gloveInit();
            catcherAnim("IDLE_1", true);
            state = State.Idle;
            curTime = 0;
        }

        /// <summary>
        /// 사인 교환
        /// </summary>
        public void setSign()
        {
            posX = initPosX;
            gabX = 0;
            transform.localPosition = new Vector3(initPosX, initPosY, -2);

            gloveInit();
            catcherAnim("SIGN", false);
            state = State.Sign;
            curTime = 0;
        }

        /// <summary>
        /// 사인 응답 대기
        /// </summary>
        public void setWait()
        {
            catcherAnim("IDLE_1", true);
            state = State.SignWait;
            curTime = 0;
        }

        /// <summary>
        /// 포구 준비
        /// </summary>
        public void setReady()
        {
            gloveActive(false);
            catcherAnim("READY_1", false);
            Invoke("gloveAvailble", 1.4f);
        }

        float glovePosX, glovePosY;
        private void gloveAvailble()
        {
            catcherAnim2("CATCH_BODY_IMG_1", false);
            leftArmPos.localPosition = new Vector3(62, 87, -0.05f);
            form = CatcherForm.Normal;
            float dstX = pitcher.zoneUI.curX - gabX;
            float dstY = pitcher.zoneUI.curY;
            chageCatcherForm(dstX, dstY);
            gloveActive(true);            
            state = State.Ready;
        }

        private void gloveInit()
        {
            bCatchResponse = false;
            glovePosX = glovePosY = 0;
            gloveAnim("CATCH_GLOVE_IMG");
            gloveActive(false);
            glove.transform.localPosition = new Vector3(glovePosX, glovePosY, 0);
        }


        private void gloveFrame()
        {
            if (pitcher.bRelease == true)
            {
                if (bCatchResponse == true)
                {
                    setNormalBallCatch();
                }
            }
            else
            {
                float dstX = pitcher.zoneUI.curX - gabX;
                float dstY = pitcher.zoneUI.curY;

                float absX = Mathf.Abs(glovePosX - dstX);
                float absY = Mathf.Abs(glovePosY - dstY);

                chageCatcherForm(dstX, dstY);

                if (bFormChangeAvail == true)
                {
                    if (absX > 5 || absY > 5)
                    {
                        float dx = (dstX - glovePosX) * 3 * Time.deltaTime;
                        float dy = (dstY - glovePosY) * 3 * Time.deltaTime;

                        glovePosX += dx;
                        glovePosY += dy;

                        float rX = Mathf.Clamp(glovePosX, -40, 40);
                        float rY = Mathf.Clamp(glovePosY, -40, 40);

                        glove.transform.localPosition = new Vector3(rX, rY, 0);
                    }
                }

                if (pitcher.bCatcherMoveFlag == true)
                {
                    if (Mathf.Abs(dstX) > 30)
                    {
                        StartCoroutine(setSideStep((int)dstX));
                    }
                    pitcher.bCatcherMoveFlag = false;
                }
            }
            leftArmFrame();
        }

        private void leftArmFrame()
        {
            float gabX = glovePos.position.x - leftArmPos.position.x;
            float gabY = glovePos.position.y - leftArmPos.position.y;

            float angle = Mathf.Atan2(gabY, gabX) * Mathf.Rad2Deg;

            leftArmPos.localEulerAngles = new Vector3(0, 0, angle);
        }

        /*
        //68,108 // 0
        //37 ,92 // 6
        //18 ,92 // 7
        //9, 127 //11*/

        
        private IEnumerator catcherFormChangeAnim(CatcherForm nextForm, Vector3 newPos)
        {
            bFormChangeAvail = false;
            string ani = "CATCH_BODY_ANI_MOVE_" + (int)form + "_" + (int)nextForm;
            catcherAnim2(ani, false); //catcherAnim(2, "CATCH_BODY_IMG_7", false);
           
            //왼팔 움직임
            TweenPosition.Begin(leftArmPos.gameObject, 0.167f, newPos);

            //글러브 중점움직임
            float dstX = Mathf.Clamp(pitcher.zoneUI.curX - gabX, -40, 40);
            float dstY = Mathf.Clamp(pitcher.zoneUI.curY, -40, 40);
            Vector3 nextGlovePos = new Vector3(dstX, dstY, 0);
            TweenPosition.Begin(glove.gameObject, 0.167f, nextGlovePos);
            
            form = nextForm;
            yield return new WaitForSeconds(0.167f);

            bFormChangeAvail = true;
            glovePosX = nextGlovePos.x;
            glovePosY = nextGlovePos.y;
        }

        bool bFormChangeAvail = true;

        private void chageCatcherForm(float rX, float rY)
        {
            //bool bChangeForm = false;
            if (bFormChangeAvail == true)
            {
                if (rX < -15)
                {
                    if (rY < 0)
                    {
                        if (form != CatcherForm.DownRight)
                        {
                            StartCoroutine(catcherFormChangeAnim(CatcherForm.DownRight, new Vector3(17, 72, -0.05f)));
                        }
                    }
                    else
                    {
                        if (form != CatcherForm.Up)
                        {
                            StartCoroutine(catcherFormChangeAnim(CatcherForm.Up, new Vector3(10, 101, -0.05f)));
                        }
                    }
                }
                else
                {
                    if (rY < -20)
                    {
                        if (form != CatcherForm.Down)
                        {
                            StartCoroutine(catcherFormChangeAnim(CatcherForm.Down, new Vector3(34, 73, -0.05f)));
                        }
                    }
                    else
                    {
                        if (form != CatcherForm.Normal)
                        {
                            StartCoroutine(catcherFormChangeAnim(CatcherForm.Normal, new Vector3(62, 87, -0.05f)));
                        }
                    }
                }
            }
           
        }



        
        private IEnumerator setSideStep(int gab )
        {
            bool bLeft = gab > 0 ? true : false;

            state = State.SideStep;
            gloveInit();

            if (bLeft == true)
            {
                catcherAnim("MOVE_LEFT", false);
            }
            else
            {
                catcherAnim("MOVE_RIGHT", false);
            }
            gabX += gab;
            anim.timeScale = 1.61f;
            
            yield return new WaitForSeconds(0.7f);            
            yield return new WaitForEndOfFrame();            
            transform.localPosition = new Vector3(initPosX + gabX, initPosY, -2);            
            catcherAnim2("READY_1", false);
            anim.timeScale = 2;
            yield return new WaitForSeconds(0.7f);
            anim.timeScale = 1;
            gloveAvailble();
            
        }


        /// <summary>
        /// 글러브 위치를 포구 위치로 aim
        /// </summary>
        public void setGloveAim(float dstX, float dstY)
        {
            float aimX = -dstX - gabX;// Mathf.Clamp(-pitcher.preArriveX - gabX, -Zone.UI_ZONE_WIDTH, Zone.UI_ZONE_WIDTH);
            float aimY = Mathf.Clamp(dstY, -Zone.UI_ZONE_HEIGHT + 10, Zone.UI_ZONE_WIDTH + 10);

            chageCatcherForm(aimX, aimY);//, false);
            
            TweenPosition.Begin(glove.gameObject, 0.2f, new Vector3(aimX, aimY, 0));
        }

        /// <summary>
        /// 정상위치 포구
        /// </summary>
        float ballX, ballY;
        float catcherX, catcherY;
        float catchGabX, catchGabY;

        public void setNormalBallCatch()
        {
            if (pitcher.batter.bPreHitCheck == true)
            {
                state = State.BallHit;
                return;
            }

            ballX = -pitcher.preArriveX;
            ballY = pitcher.preArriveY;

            catcherX = glovePosX + gabX;
            catcherY = glovePosY;

            catchGabX = ballX - catcherX;
            catchGabY = ballY - catcherY;

            //Debug.Log("ballX / catcherX / catchGabX = " + ballX + " / " + catcherX + " / " + catchGabX);
            //Debug.Log("ballY / catcherY / catchGabY = " + ballY + " / " + catcherY + " / " + catchGabY);
            //Debug.Log("catchGabX = " + catchGabX);

            
            if (pitcher.bMissControl == true)
            {
                bool bMiss = pitcher.bWildPitch ? true : false;
                if (pitcher.hitByPitchStep != 0) bMiss = true;

                if (ballY < -Zone.UI_ZONE_HEIGHT)
                {
                    //밑으로 잡음
                    downWildCatch(bMiss);
                }
                else if (ballY > Zone.UI_ZONE_HEIGHT + 40)
                {
                    //위로 잡음
                    upWildCatch(bMiss);
                }
                else if (ballX > Zone.UI_ZONE_WIDTH)
                {
                    sideWildCatch(true, bMiss, ballY);
                }
                else if (ballX < -Zone.UI_ZONE_WIDTH)
                {
                    sideWildCatch(false, bMiss, ballY);
                }
                else
                {
                    normalCatch();
                }

                if (bMiss == false)
                {
                    Invoke("removeBall", 0.5f);
                }
            }
            else
            {
                if (ballY < -Zone.UI_ZONE_HEIGHT)
                {
                    //밑으로 잡음
                    downCatch(false);
                }
                else if (ballY > Zone.UI_ZONE_HEIGHT)
                {
                    //위로 잡음
                    upCatch(false);
                }
                else
                {
                    normalCatch();
                }
            }

            state = State.Catch;
        }

        private void normalCatch()
        {
            int type = (int)form;

            bool bHit = false;
            if (pitcher.batter.bDecideSwing == true)
            {
                if (pitcher.batter.aiContactPoint != BattingContact.HUT_SWING)
                {
                    if (pitcher.batter.aiTimingPoint > BattingTiming.TOO_EARLY && pitcher.batter.aiTimingPoint < BattingTiming.TOO_LATE)
                    {
                        bHit = true;
                    }
                }
            }
            
            //왼팔 위치
            if(form == CatcherForm.Down) leftArmPos.localPosition = new Vector3(34, 73, -0.05f);
            else if (form == CatcherForm.DownRight) leftArmPos.localPosition = new Vector3(17, 72, -0.05f);
            else if (form == CatcherForm.Up) leftArmPos.localPosition = new Vector3(10, 101, -0.05f);
            else leftArmPos.localPosition = new Vector3(62, 87, -0.05f);

            if (bHit == true)
            {
                catcherAnim2("CATCH_BODY_ANI_" + type, false);
                gloveAnim("CATCH_GLOVE_ANI_MISS");
            }
            else
            {
                if (pitcher.selectedBallIndex == PitchingArsenal.FASTBALL)
                {
                    catcherAnim2("CATCH_BODY_ANI_STRONG_" + type, false);
                    gloveAnim("CATCH_GLOVE_ANI_STRONG");
                }
                else
                {
                    catcherAnim2("CATCH_BODY_ANI_" + type, false);
                    gloveAnim("CATCH_GLOVE_ANI");
                }
            }
            
        }

        private void downCatch(bool bMiss)
        {
            gabX = 0;
            string strMiss = bMiss ? "_MISS" : "";
            string animStr;

            if (catchGabX > 20 || ballX > Zone.UI_ZONE_WIDTH)
            {
                transform.localPosition = new Vector3(initPosX + ballX - 50, initPosY, -2);
                animStr = "CATCH_LOW_LEFT";
            }
            else if (catchGabX < -20 || ballX < -Zone.UI_ZONE_WIDTH)
            {
                transform.localPosition = new Vector3(initPosX + ballX + 50, initPosY, -2);
                animStr = "CATCH_LOW_RIGHT";
            }
            else
            {
                transform.localPosition = new Vector3(initPosX + ballX - 5, initPosY, -2);
                animStr = "CATCH_LOW_CENTER";
            }

            catcherAnim(animStr + strMiss, false);
            gloveActive(false);
        }

        private void upCatch(bool bMiss)
        {
            //string animName = bVeryHigh ? "CATCH_OVER_HIGH_" : "CATCH_HIGH_";
            string strMiss = bMiss ? "_MISS" : "";
            if (catchGabX > 20 || ballX > Zone.UI_ZONE_WIDTH)
            {
                //위치좌표 퉁침
                catcherAnim("CATCH_HIGH_LEFT" + strMiss, false);
            }
            else if (catchGabX < -20 || ballX < -Zone.UI_ZONE_WIDTH)
            {
                //위치좌표 퉁침
                transform.localPosition = new Vector3(initPosX + 40 + catcherX, initPosY, -2);
                catcherAnim("CATCH_HIGH_RIGHT" + strMiss, false);
            }
            else
            {
                //위치좌표 계산 필요 (initPosX - 30 + catcherX)
                transform.localPosition = new Vector3(initPosX - 10 + catcherX, initPosY, -2);
                catcherAnim("CATCH_HIGH_CENTER" + strMiss, false);
            }
            gloveActive(false);
            
        }

        private void upWildCatch(bool bMiss)
        {
            string strMiss = bMiss ? "_MISS" : "";
            string side = "CENTER";
            gabX = 0;            
            if (ballX > Zone.UI_ZONE_WIDTH)
            {
                //위치좌표 퉁침
                side = "LEFT";
                transform.localPosition = new Vector3(initPosX + ballX - 55, initPosY, -2);
            }
            else if (ballX < -Zone.UI_ZONE_WIDTH)
            {
                side = "RIGHT";
                transform.localPosition = new Vector3(initPosX + ballX + 50, initPosY, -2);
            }
            else
            {
                transform.localPosition = new Vector3(initPosX + ballX - 10, initPosY, -2);
            }
            catcherAnim("CATCH_OVER_HIGH_" + side + strMiss, false);
            gloveActive(false);
        }

        private void downWildCatch(bool bMiss)
        {
            gabX = 0;
            string strMiss = bMiss ? "_MISS" : "";
            string animStr;

            if (catchGabX > 60)
            {
                transform.localPosition = new Vector3(initPosX + ballX - 125, initPosY, -2);
                animStr = "CATCH_OVER_LOW_LEFT";
            }
            else if (catchGabX > 30)
            {
                transform.localPosition = new Vector3(initPosX + ballX - 50, initPosY, -2);
                animStr = "CATCH_LOW_LEFT";
            }
            else if (catchGabX < -60)
            {
                transform.localPosition = new Vector3(initPosX + ballX + 120, initPosY, -2);
                animStr = "CATCH_OVER_LOW_RIGHT";
            }
            else if (catchGabX < -30)
            {
                transform.localPosition = new Vector3(initPosX + ballX + 50, initPosY, -2);
                animStr = "CATCH_LOW_RIGHT";
            }
            else
            {
                transform.localPosition = new Vector3(initPosX + ballX - 5, initPosY, -2);
                animStr = "CATCH_LOW_CENTER";
            }

            catcherAnim(animStr + strMiss, false);
            gloveActive(false);
        }


        private void sideWildCatch(bool bLeft, bool bMiss, float heightY)
        {
            gabX = 0;            
            string side = bLeft ? "_LEFT" : "_RIGHT";
            string strMiss = bMiss ? "_MISS" : "";
            string height = (heightY > 0 ? "MIDDLE" : "LOW");
            if (heightY > 0)
            {
                transform.localPosition = new Vector3(initPosX + ballX + (bLeft ? -149 : 149), initPosY, -2);
            }
            else
            {
                transform.localPosition = new Vector3(initPosX + ballX + (bLeft ? -149 : 115), initPosY, -2);
            }
            catcherAnim("CATCH_OVER_" + height + side + strMiss, false);
            gloveActive(false);

        }


        public IEnumerator setCatchResponse(float delay)
        {
            yield return new WaitForSeconds(delay);
            bCatchResponse = true;
        }

        private void removeBall()
        {
            pitcher.pitchPv.pitchOriginPv.removeBall();
        }

    }
}