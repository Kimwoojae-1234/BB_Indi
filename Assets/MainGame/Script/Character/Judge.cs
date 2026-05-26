using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{

    public class Judge : MonoBehaviour
    {
        //INDEX
        const int HOME_JUDGE = 3;
        const int FIRST_JUDGE = 0;
        const int SECOND_JUDGE = 1;
        const int THIRD_JUDGE = 2;

        //기본 상수값
        const float BASIC_SPEED = 200.0f;

        //애니메이션 이름
        const string HOLD = "0000_HOLD_";
        const string WALK = "0001_WALK_";
        const string RUN = "0100_RUN_";
        const string SAFE = "1000_SAFE_";
        const string OUT = "1100_OUT_";
        const string FAIR = "1200_FAIR_";
        const string FOUL = "1300_FOUL_";
        const string HOMERUN = "1400_HOMERUN_";

        //상태
        const int JUDGE_HOLD = 0,
              JUDGE_MOVE = 1,
              JUDGE_CALL = 2,
              JUDGE_LINEWAIT = 3,
              JUDGE_WAIT = 4;



        Field field;

        public SkeletonAnimation anim;
        tk2dSprite shadow;

        //상태
        int lastState, state;
        int lastDir, nDir;
        int originDir;          //원래 방향
        int dstDir;             //목적지에서의 방향
        int basecallDir;        //베이스에서 콜하는 방향
        //string strID, _strID;

        //벡터
        public float posX, posY;
        public float dstX, dstY;
        public float firstX, firstY, originX, originY; //시작점, 그리고 심판이 경기중 위치하는 원점
        public float screenX, screenY;
        float dX, dY;
        float curTime, deltaTime, maxTime;   //타임 베이스
        float angleDir;

        //포지션
        int posIndex;

        //필딩
        int catchFielder;
        float firstAngle;
        bool bOnPlaying;
        bool bReturnPosition;
        bool bDelayCall;
        bool bOnFocus;

        void Awake()
        {
            shadow = gameObject.GetComponent<tk2dSprite>();
            posX = posY = 0;
            bOnPlaying = false;
        }

        // Use this for initialization
        void Start()
        {


        }

        public void InitInstance(Field _field, int index)
        {
#if _OrthoCamera
            transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            shadow.scale = new Vector3(1.6f, 1, 1);
            shadow.color = new Color(1, 1, 1, 0.67f);
#else
            transform.localScale = new Vector3(FieldParm.JUDGE_SIZE_RATE, FieldParm.JUDGE_SIZE_RATE, FieldParm.JUDGE_SIZE_RATE);
            transform.localEulerAngles = new Vector3(CameraManager.FieldActiveAngleX, 0, 0);
            //shadow.scale = new Vector3(1.6f, 1, 1);
            //shadow.color = new Color(1, 1, 1, 0.67f);            
#endif

            field = _field;
            posIndex = index;

            //field.fRatio = 0.6f;

            initPosition();
        }

        public void initPosition()
        {
            transform.localScale = new Vector3(FieldParm.JUDGE_SIZE_RATE, FieldParm.JUDGE_SIZE_RATE, FieldParm.JUDGE_SIZE_RATE);
            bOnFocus = false;
            bDelayCall = false;
            bReturnPosition = false;
            bOnPlaying = false;
            originX = field.getOriginX(FieldSize.getJudgePosX(posIndex));
            originY = field.getOriginY(FieldSize.getJudgePosY(posIndex));

            posX = originX;
            posY = originY;

            int[] dir = new int[4] { FieldParm._SOUTHWEST, FieldParm._SOUTH, FieldParm._SOUTHEAST, FieldParm._NORTH };
            nDir = originDir = dir[posIndex];

            state = JUDGE_HOLD;
            playAnim(HOLD, false);

            setPosition();
        }


        // Update is called once per frame
        void Update()
        {
            deltaTime = field.getDeltaTime();//Time.deltaTime;
            nextFrame();
        }



        public void setStartGame()
        {
            //게임 스타트시 불러줘
            posX = originX;
            posY = originY;
            lastDir = nDir = originDir;
        }

        void setMove()
        {

            float dis = MyMath.getDistance(posX, dstX, posY, dstY);

            maxTime = dis / BASIC_SPEED;
            angleDir = Mathf.Atan2(dstY - posY, dstX - posX);

            //Debug.Log("########## JUDGE SET FIELDING  maxTime = " + maxTime);


            dY = BASIC_SPEED * Mathf.Sin(angleDir);
            dX = BASIC_SPEED * Mathf.Cos(angleDir);
            nDir = FieldParm.getDir(angleDir);// getDir();
            lastDir = nDir;

            dstDir = originDir;

            state = JUDGE_MOVE;
            playAnim(RUN, true,0.6f);

            bOnPlaying = true;
            curTime = 0;
        }

        public void setGrounder(int fielder, float firstAngle, bool bLine)
        {
            this.catchFielder = fielder;
            this.firstAngle = firstAngle;

            if (posIndex == FieldParm.HOMEBASE_INDEX)
            {
                dstX = originX + 80;
                dstY = originY;
                basecallDir = FieldParm._NORTHWEST;
            }
            else if (posIndex == FieldParm.FIRSTBASE_INDEX)
            {
                if (field.ball.firstAngle < -30 && (field.ball.bHomeRunGuess || field.ball.bFoulHomerunGuess))
                {
                    posX = originX + 707;
                    posY = originY + 670;
                    return;
                }
                else
                {
                    if (bLine && firstAngle < 0)
                    {
                        //라인 판정 대기
                        state = JUDGE_LINEWAIT;
                        return;
                    }
                    else
                    {
                        if (catchFielder == CPlayer._SHORTSTOP || catchFielder == CPlayer._THIRDBASEMAN)
                        {
                            dstX = originX - 189 - 100;
                            dstY = originY + 60 + 66;   //68
                            basecallDir = FieldParm._SOUTH;
                        }
                        else
                        {
                            dstX = originX - 153 - 39;  //-189
                            dstY = originY - 288 + 37;  //68 
                            basecallDir = FieldParm._NORTHWEST;

                        }
                    }
                }
            }
            else if (posIndex == FieldParm.SECONDBASE_INDEX)
            {
                dstX = originX + (firstAngle > 0 ? 126 : -126);
                dstY = originY;
                basecallDir = (firstAngle > 0 ? FieldParm._SOUTHWEST : FieldParm._SOUTHEAST);
            }
            else if (posIndex == FieldParm.THIRDBASE_INDEX)
            {
                if (field.ball.firstAngle > 30 && (field.ball.bHomeRunGuess || field.ball.bFoulHomerunGuess))
                {
                    posX = originX - 707;
                    posY = originY + 670;
                    return;
                }
                else
                {
                    if (bLine && firstAngle < 0)
                    {
                        //라인 판정 대기
                        state = JUDGE_LINEWAIT;
                        return;
                    }
                    else
                    {
                        dstX = originX + 96;
                        dstY = originY - 136;
                        basecallDir = FieldParm._EAST;
                    }
                }
            }
            setMove();
        }

        public void setFlyball(int fielder, float firstAngle)
        {
            this.catchFielder = fielder;
            this.firstAngle = firstAngle;

            bool bCheckMove = false;

            if (fielder < CPlayer._LEFTFIELDER)
            {
                if (fielder <= CPlayer._CATCHER)
                {
                    if (posIndex == FieldParm.HOMEBASE_INDEX) bCheckMove = true;
                }
                else if (fielder == CPlayer._FIRSTBASEMAN)
                {
                    if (posIndex == FieldParm.FIRSTBASE_INDEX) bCheckMove = true;
                }
                else if (fielder == CPlayer._THIRDBASEMAN)
                {
                    if (posIndex == FieldParm.THIRDBASE_INDEX) bCheckMove = true;
                }
                else
                {
                    if (posIndex == FieldParm.SECONDBASE_INDEX) bCheckMove = true;
                }
            }
            else
            {
                if (firstAngle > 30)
                {
                    if (posIndex == FieldParm.THIRDBASE_INDEX) bCheckMove = true;
                }
                else if (firstAngle < -30)
                {
                    if (posIndex == FieldParm.FIRSTBASE_INDEX) bCheckMove = true;
                }
                else
                {
                    if (posIndex == FieldParm.SECONDBASE_INDEX) bCheckMove = true;
                }
            }


            if (bCheckMove == true)
            {
                field.judge.flyOutCallIndex = posIndex;
                bReturnPosition = true;
                dstX = originX + (field.ball.nFirstBoundX - originX) / 3;
                dstY = originY + (field.ball.nFirstBoundY - originY) / 3;

                if (posIndex == FieldParm.SECONDBASE_INDEX)
                {
                    //이루심 특수 상황
                    dstX = originX + (firstAngle > 0 ? 126 : -126);
                    if (fielder < CPlayer._LEFTFIELDER)
                    {
                        dstY = originY;
                    }
                }
                else if (posIndex == FieldParm.HOMEBASE_INDEX)
                {
                    dstX = originX;
                    dstY = originY;
                }


                setMove();
            }
            else
            {
                setGrounder(fielder, firstAngle, false);
            }

        }


        public void judgeWait()
        {
            state = JUDGE_LINEWAIT;
        }

        public float callOutStrong()
        {
            //judgeWait();
            float delay = 0.01f;
            nDir = basecallDir;
            state = JUDGE_CALL;

            bOnFocus = true;
            bDelayCall = true;
            maxTime = 1;// 0.3f;
            //playAnimRow("1101_OUT_BIG_S" + Random.Range(1, 3));
            delay = 1.5f;// 0.8f;        
            curTime = 0;
            return delay;
        }

        public float callOut(CallType type = CallType._OUT)
        {
            float delay = 0.01f;
            //bOnPlaying = false;
            //Debug.Log("#######################JUDGE :: callOut()");
            if (type == CallType._FLYOUT)
            {
                //그대로 유지
            }
            else
            {
                nDir = basecallDir;
            }

            state = JUDGE_CALL;

            if (posIndex == FieldParm.FIRSTBASE_INDEX && nDir == FieldParm._SOUTH && type == CallType._OUT)
            {
                bDelayCall = true;
                maxTime = (Random.Range(0, 2) == 1 ? 0.3f : 0.8f);// 1.0f;
                //playAnimRow("1101_OUT_BIG_S" + Random.Range(1, 3));
                delay = 0.8f;
            }
            else
            {
                playAnim(OUT, false);
                StartCoroutine(field.callUI("out", 0));
                maxTime = 2;
            }

            curTime = 0;

            return delay;

        }

        public void callSafe(CallType type = CallType._SAFE)
        {
            //bOnPlaying = false;
            nDir = basecallDir;
            state = JUDGE_CALL;
            playAnim(SAFE, false);

            curTime = 0;
            maxTime = 2;
        }

        public void callFoul(CallType type)
        {
            state = JUDGE_CALL;

            if (type == CallType._FOUL)
            {
                /*if (field.ball.angleZ > 10 && field.ball.nFirstBoundY > posY+200)
                    nDir = FieldParm._NORTH;
                else*/
                nDir = FieldParm._SOUTH;
                playAnim("1300_FOUL_", true);
            }
            else
            {
                nDir = (posIndex == FieldParm.FIRSTBASE_INDEX ? FieldParm._SOUTHWEST : FieldParm._SOUTHEAST);
                playAnim("1200_FAIR_", false);//  SE);
            }

            curTime = 0;
            maxTime = 2;

            bReturnPosition = true;
        }


        public void callHomerun()
        {
            nDir = FieldParm._NORTH;
            state = JUDGE_CALL;
            playAnim(HOMERUN, true);

            curTime = 0;
            maxTime = 20;
        }



        float timeScale, timeScaleRate;
        //int track = 1;
        //int lastTrack;

        void playAnim(string ani, bool bLoop, float tScale = 1.0f)
        {

            string strID = ani + FieldParm._dir[nDir];
            anim.state.ClearTracks();
            anim.skeleton.SetToSetupPose();//.SetSlotsToSetupPose();
            anim.state.SetAnimation(0, strID, bLoop);

            
            timeScale = tScale;
            anim.timeScale = (timeScale * timeScaleRate);
        }


        void playAnimRow(string ani, bool bLoop = false, float tScale = 1.0f)
        {
            //anim.skeleton.SetSlotsToSetupPose();
            //anim.state.SetAnimation(0, ani, bLoop);

            
            anim.state.ClearTracks();
            anim.skeleton.SetToSetupPose();// .SetSlotsToSetupPose();
            anim.state.SetAnimation(0, ani, bLoop);

            
            timeScale = tScale;
            anim.timeScale = (timeScale * timeScaleRate);
        }

        public void setTimeScale(float scale)
        {
            timeScaleRate = scale;

            anim.timeScale = (timeScale * timeScaleRate);
        }

        public void setPosition()
        {
            screenX = field.getScreenX(posX);
            screenY = field.getScreenY(posY);

            float depthZ = 0;// 
#if OrthoCamera
            depthZ = -4 + (posY * 0.0002f);
#endif

            transform.localPosition = new Vector3(screenX, screenY, depthZ);
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

        void drawJudge()
        {
            setPosition();
            setRatio();
        }

        void nextFrame()
        {
            curTime += deltaTime;

            switch (state)
            {
                case JUDGE_HOLD:
                    hold();
                    break;
                case JUDGE_MOVE:
                    move();
                    break;
                case JUDGE_CALL:
                    call();
                    break;
            }

            drawJudge();
        }

        void hold()
        {
            if (bOnPlaying == true)
            {
                float x = field.ball.nBallX;
                float y = field.ball.nBallY;
                angleDir = Mathf.Atan2(y - posY, x - posX);
                nDir = FieldParm.getDir(angleDir);// getDir();

                if (lastDir != nDir)
                {
                    lastDir = nDir;
                    playAnim(HOLD, false);
                }
            }
        }

        void move()
        {
            posX += (dX * deltaTime);
            posY += (dY * deltaTime);
            if (curTime > maxTime)
            {
                nDir = dstDir;
                state = JUDGE_HOLD;
                playAnim(HOLD, false);
            }
        }

        void call()
        {
            if (bDelayCall == true)
            {
                if (curTime > maxTime)
                {
                    field.delayedCallTime = 0.3f;
                    playAnimRow("1101_OUT_BIG_S" + (maxTime == 0.3f ? 2 : 1));//  Random.Range(1, 3));
                    StartCoroutine(field.callUI("out", 0));
                    bDelayCall = false;
                    curTime = 0;
                    maxTime = 2.0f;
                    if (bOnFocus == true)
                    {
                        field.ball.setJudgeFocus(posIndex);
                        bOnFocus = false;
                    }
                }
            }
            else
            {
                if (curTime > maxTime)
                {
                    if (bReturnPosition == true)
                    {
                        setGrounder(catchFielder, firstAngle, false);
                    }
                    else
                    {
                        //nDir = dstDir;
                        state = JUDGE_HOLD;
                        playAnim(HOLD, false);
                    }
                }
            }
        }



        //OnTriggerStay
        private void OnTriggerStay(Collider col)
        {
            //if (col.gameObject.tag == "FIELDER_TAG")
            if (col.gameObject.CompareTag("FIELDER_TAG") == true)
            {
                if (field.ball.step == BallStep.BALL_HIT)
                {
                    if (state != JUDGE_CALL)
                    {
                        if (posIndex == FieldParm.SECONDBASE_INDEX || posIndex == FieldParm.HOMEBASE_INDEX)
                        {
                            dstX = posX + (field.ball.firstAngle > 0 ? 50 : -50);
                            dstY = posY;
                            setMove();
                        }
                        else
                        {
                            dstX = posX;
                            dstY = posY + 75;
                            basecallDir = FieldParm._SOUTH;
                            setMove();
                        }
                    }
                }
            }
        }
    }
}