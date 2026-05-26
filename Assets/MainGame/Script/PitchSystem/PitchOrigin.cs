//#define _CONTROL_TYPE1 //원래버전


using UnityEngine;
using System.Collections;
using Spine.Unity;


namespace BaseBall.BallPlay
{
    public class PitchOrigin : MonoBehaviour
    {
        public const float STRIKE_ZONE_WIDTH = 2.6f;
        public const float STRIKE_ZONE_HEIGHT = 3.143f; 
        

        //기본 물리 상수
        const float BASE_SPEED = 250;//300.0f;
        const float SPEED_GRADIENT = 2.875f; //140이상인 스피드에 추가적
        const float BASE_REAL_SPEED = 140.0f;
        const float FORK_ACC = 30;
        const float CIRCLE_ACC = 30;

        //중력 가속도
        const float GRAVITY_ACC_FAST = -40;     // -100;//중력가속도 -100을 -40으로 변경
        const float GRAVITY_ACC_CURVE = -70;    // -100;//중력가속도 -100을 -40으로 변경
        const float GRAVITY_ACC_SLIDER = -50;   // -100;//중력가속도 -100을 -40으로 변경
        const float GRAVITY_ACC_OFFSPEED = -80; // -100;//중력가속도 -100을 -40으로 변경
        const float GRAVITY_ACC_FASTBREAK = -45;// -100;//중력가속도 -100을 -40으로 변경


        //초기 위치 상수
        const int INITX = 0;
        const int INITY = 0;
        const int INITZ = 0;

        //클래식 뷰
        /*//오버
        const float OVER_INIT_BALL_HEIGHT = 30;// 15; //20
        const float OVER_INIT_BALL_GAB = 28;//16;
        //사이드
        const float SIDE_INIT_BALL_HEIGHT = 12;
        const float SIDE_INIT_BALL_GAB = 38;//25;
        //언더
        const float UNDER_INIT_BALL_HEIGHT = -5;
        const float UNDER_INIT_BALL_GAB = 32;//22;

        public float INIT_BALL_HEIGHT;// = 0;
        public float INIT_BALL_GAB;// = 22;*/

        //배팅뷰 롷우
        //오버
        const float OVER_INIT_BALL_HEIGHT = 25;
        const float OVER_INIT_BALL_GAB = 22;
        //사이드
        const float SIDE_INIT_BALL_HEIGHT = 12;
        const float SIDE_INIT_BALL_GAB = 38;//25;
        //언더
        const float UNDER_INIT_BALL_HEIGHT = -5;
        const float UNDER_INIT_BALL_GAB = 32;//22;

        public float INIT_BALL_HEIGHT;// = 0;
        public float INIT_BALL_GAB;// = 22;

        //존 관련 상수
        const float ZONE_HEIGHT = 5;
        const float ZONE_CALIBRATE_Y = -0.7f;
        const int ZONE_DISTANCE = -255;//55;
        const int ZONE_CHECK = -254;//55; 원래
        const int ZONE_CHECK2 = -249;//55; 원래
        const int HALF_CHECK = -130;
        const int END_DISTANCE = -300;//55;
        const int ORIGIN_POSZ = -50;
        const float OFFSET_UNIT = 150;

        //구질 관련 상수
        //커브
        const int CURVE_CURVATURE = 12;
        const int KNUCKLE_CURVE_CURVATURE = 6;
        const int _12_6_CURVE_CURVATURE = 0;
        const int POWER_CURVE_CURVATURE = 12;
        const int SLOW_CURVE_CURVATURE = 15;
        const int SCREW_CURVATURE = -12;

        //슬라이더
        const int SLIDER_CURVATURE = 5;
        const int H_SLIDER_CURVATURE = 4;
        const int V_SLIDER_CURVATURE = 2;
        const int SLUVE_CURVATURE = 7;
        const int SINKER_CURVATURE = -5;

        //오프스피드
        const int CIRCLE_CURVATURE = -2;


        //
        const int SINKING_FAST_CURVATURE = -2;
        const int TWOSEAM_FAST_CURVATURE = -2;
        const int RISING_FAST_CURVATURE = 5;
        const int CUT_FAST_CURVATURE = 4;

        public GameObject ball, ballObj,cursorBall, ballObjSpr;
        public GameObject shadow;
        public GameObject zoneCollider;
        public GameObject endCollider;
        public GameObject pPointer;

        public PitchIndicator indicator;
        private int indicatorLineNum;

        tk2dSprite cursorSpr;


        //투수
        int pHandSign;
        float pBallSpeed;
        float pMovement1, pMovement2, pMovement3, pMovement4;
        float speedGun;

        float speedRate;


        //공
        public bool bRelease;
        public bool bBallHit;
        bool bBreak;
        public bool bZoneCheck;
        public bool bFinish;
        public bool bWildPitch;
        bool bCursorCheck;
        bool bTip;
        bool bBigCurve;//큰 커브를 그리는 곡선
        bool bMissCase; //실투의 경우
        //bool bMyTurn;
        //bool bShowBallTrace;
        bool bBound;
        //float depth;

        //구질
        //public PitchType pitchType = PitchType.CHANGEUP;
        public PitchingArsenal ballType = PitchingArsenal.SLIDER;
        private BallMoveType moveType;

        float ballSpeed;
        float pitchTypeOffsetY;
        bool bRising;


        //물리
        float x, y, z;
        float dx, dy, dz;
        float addDY;
        float curvatureAX, curvatureAY;      //곡률 가속도
        float shadowY;             //그림자 Y
        float breakPointZ, breakPoint;         //break가 일어나는 z
        float movementAX, movementAY;
        float startOffsetX, startOffsetY;

        //상태
        public bool bHit;
        public bool bHitByPitched;

        //존
        float zoneX, zoneY;     //존 위치
        float zoneDX, zoneDY;   //존의 속도
        float zoneMoveX, zoneMoveY;
        float offsetX, offsetY; //변화구 목적지 조정값
        float pX, pY;
        float arriveX, arriveY;

        //중력 가속도
        float gravity_acc;

        //기타
        public float perfectTime;

        //투수 특수효과
        private bool bBlurEffect;
        //테일 이펙트
        public GameObject tail;


        // Use this for initialization
        void Start()
        {
            bRelease = false;
            bHitByPitched = false;

            pPointer.transform.localPosition = new Vector3(0, 0, 0);
            cursorSpr = pPointer.GetComponent<tk2dSprite>();

            
            ball.transform.localScale = new Vector3(2, 2, 2);           //배팅뷰 로우
            //ball.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);//클래식뷰
            //cursorBall.renderer.enabled = false;

            tail.SetActive(false);
        }

        // Update is called once per frame
        float rotationX, rotationY, rotationZ;
        float rDX, rDY, rDZ;
        void Update()//FixedUpdate()
        {
            if (bRelease == true)
            {
                //볼회전 표현 //3D시 되돌려
                /*rotationX += (rDX * Time.deltaTime);
                rotationY += (rDY * Time.deltaTime);
                rotationZ += (rDZ * Time.deltaTime);
                ballObj.transform.localEulerAngles = new Vector3(rotationX, rotationY, rotationZ);*/
                move();
            }


            if (bCursorMove == true)
            {
                cTime += Time.deltaTime;
                firstX += cDX * Time.deltaTime;
                firstY += cDY * Time.deltaTime;
                if (cTime >= CURSOR_WAIT_TIME)
                {
                    firstX = zoneX;
                    firstY = zoneY;
                    bCursorMove = false;
                    if (bMissCase == true) return;
                }
                movePointer(firstX, firstY);
            }

        }


        private void initRotate(PitchingArsenal type, int sign)
        {
            //ballObj.transform.localEulerAngles = new Vector3(0, 0, 0);//3D시 되돌려

            if (type == PitchingArsenal.KNUCKLE)
            {
                rotationY = 90;//
                rotationX =  rotationZ = 0;
                rDX = rDY = rDZ = 0;
            }
            else if (type == PitchingArsenal.FASTBALL || type == PitchingArsenal.RISING
             || type == PitchingArsenal.TWOSEAM )
            {
                rotationX = Random.Range(0, 359);
                rotationY = rotationZ = 0;
                rDX = 700 * 2;
                rDY = rDZ = 0;
            }
            else if (type == PitchingArsenal.CURVE || type == PitchingArsenal.POWER_CURVE
             || type == PitchingArsenal.SLOW_CURVE || type == PitchingArsenal.POKPOSU_CURVE
             || type == PitchingArsenal.KNUCKLE_CURVE || type == PitchingArsenal.GIRO_CURVE
             || type == PitchingArsenal.EEPHUS)
            {
                rotationX = Random.Range(0, 359);
                rotationY = 0;//
                rotationZ = -sign * 45;
                rDX = -400 * 2;
                rDZ = rDY = 0;
            }
            else if (type == PitchingArsenal.SLIDER || type == PitchingArsenal.H_SLIDER
             || type == PitchingArsenal.SLURVE || type == PitchingArsenal.CUT_FAST
             || type == PitchingArsenal.FRISBEE)
            {
                rotationX = 0;
                rotationY = Random.Range(0, 359);
                rotationZ = -sign * 80;
                rDY = sign * 400 * 2;
                rDX = rDZ = 0;
            }
            else //if (type == BallMoveType.Straight)
            {
                rotationX = Random.Range(0, 359);
                rotationY = rotationZ = 0;
                rDX = 200 * 2;
                rDY = rDZ = 0;
            }

        }

        //이펙트 애니메이션
        private void effectAnim(SkeletonAnimation effectAnim, int track, string strAnim, bool bLoop)
        {
            effectAnim.skeleton.SetToSetupPose();
            effectAnim.state.SetAnimation(track, strAnim, bLoop);
            effectAnim.timeScale = 1.0f;
        }


        bool bMyTurn;
        float firstX, firstY;
        float cTime;
        float cDX, cDY;
        bool bCursorMove = false;
        const float CURSOR_WAIT_TIME = 0.45f; //const float CURSOR_WAIT_TIME = 0.8f;

        public void setFirstPos()
        {
            bCursorMove = false;
            /*if (Mode.bPvpMode == true)
            {
                setPitchCursor(false);
                firstX = firstY = 0;
            }*/
            if (Mode.bPvpMode433 == true)
            {
                setPitchCursor(false);
                firstX = firstY = 0;
            }
            else
            {
                setPitchCursor(true);
                firstX = Random.Range(-2.5f, 2.5f);
                firstY = Random.Range(-2.5f, 2.5f);
            }
            movePointer(firstX, firstY);
        }

        public void setSecondPos()
        {
            //movePointer(zoneX, zoneY);
            float nextX = zoneX;
            float nextY = zoneY;

            if (bMissCase == true)
            {
                nextX = Random.Range(-1.0f, 1.0f);
                nextY = Random.Range(-1.0f, 1.0f);
            }
            
            cTime = 0;
            cDX = (nextX - firstX) / CURSOR_WAIT_TIME;
            cDY = (nextY - firstY) / CURSOR_WAIT_TIME;
            bCursorMove = true;
        }


        public void setPitcher(Pitcher pitcher)
        {
            bMyTurn = pitcher.manager.bMyTurn;
            //////UnityEngine.//Debug.Log("=============================>>SET PITCHER");
            //pitcher.m_nThrowHand = CPlayer._RIGHTHAND;
            if (pitcher.pitchingType == CPlayer._UNDERHAND)
            {
                //언더핸드
                INIT_BALL_GAB = UNDER_INIT_BALL_GAB;
                INIT_BALL_HEIGHT = UNDER_INIT_BALL_HEIGHT;
            }
            else if (pitcher.pitchingType == CPlayer._SIDEARM)
            {
                //사이드
                INIT_BALL_GAB = SIDE_INIT_BALL_GAB;
                INIT_BALL_HEIGHT = SIDE_INIT_BALL_HEIGHT;
            }
            else
            {                
                //오버핸드
                INIT_BALL_GAB = OVER_INIT_BALL_GAB;
                INIT_BALL_HEIGHT = OVER_INIT_BALL_HEIGHT;
            }

            bBlurEffect = false;
            if (bMyTurn == false)
            {
                if (pitcher.userControlValue == UserControlValue.Perfect)
                {
                    bBlurEffect = true;
                }
            }

            //setActiveSkill(pitcher);

            pHandSign = pitcher.nSign;// (pitcher.m_nThrowHand == CPlayer._RIGHTHAND ? 1 : -1);
            pBallSpeed = pitcher.curBallSpeed;// 150;

            setMoveMent(pitcher);

            bTip = false;

            //bMyTurn = pitcher.manager.bMyTurn;
            //pitchType = PitchType.CURVE;
            //ballType = PitchingArsenal.KNUCKLE_CURVE;//_12_6_CURVE;//CURVE;//POWER_CURVE;//  SLOW_CURVE;//   ;

            bBound = false;

            speedRate = 1;

            /*
            if (bBlackHole == true)
            {
                pBallSpeed = 300;
                ballType = PitchingArsenal.FASTBALL;
                setZone(0, 0, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
            }*/

            initRotate(ballType, pHandSign);

            //depth = 1.5f;

        }

        //public float TEST_MOVEMENT = 1.0f;

        private void setMoveMent(Pitcher pitcher)
        {
            float value = PitchingMechanism.GetBallMovemnet(pitcher, ballType);

            if (Mode.bPvpMode == true)
            {
                value = value * 0.6f;
            }

            //슬라이더
            pMovement1 = 0.6f + value * 0.011f;// 0.6f ~ 1.7
            
            //커브
            pMovement2 = 0.4f + value * 0.013f;//0.4f ~ 1.7
            
            //offspeed
            pMovement3 = 0.4f + value * 0.019f;//0.4f ~ 2.3
            
            //일부 오프스피드 볼 조정
            if (ballType == PitchingArsenal.PALM || ballType == PitchingArsenal.KNUCKLE || ballType == PitchingArsenal.VULCAN)
            {
                pMovement3 *= 0.6f;
            }

            //패스트 브레이크
            pMovement4 = 1.0f + value * 0.025f;//1.0f ~ 3.5
            
        }


        public void setMoveTypeAndGuwee(BallMoveType type)
        {
            moveType = type;
        }

        void setArrivePos(float speed)
        {
            ////UnityEngine.Debug.Log("#######################>>>speed = " + speed);
            ////UnityEngine.Debug.Log("#######################>>>breakPoint = " + breakPoint);

            float t = (ZONE_CHECK) / speed;
            float bt = t * (1 - breakPoint);
            float moveX = zoneDX * bt;
            float moveY = zoneDY * bt;

            arriveX = zoneX + moveX;
            arriveY = zoneY + moveY;

            ////UnityEngine.Debug.Log("#######################>>>moveX = " + (moveX * Zone.STRIKE_ZONE_WIDTH / STRIKE_ZONE_WIDTH));
            ////UnityEngine.Debug.Log("#######################>>>moveY = " + (moveY * Zone.STRIKE_ZONE_HEIGHT / STRIKE_ZONE_HEIGHT));

        }

        public void setTip(bool bUp)
        {
            bTip = true;
            if (bUp == true)
            {
                if (bBigCurve == false)
                {
                    dy = -Random.Range(0.6f, 1) * dy;
                }
                else
                {
                    dy = -Random.Range(0.0f, 0.4f) * dy;
                }
            }
            else
            {
                if (bBigCurve == false)
                {
                    dy = Random.Range(1.3f, 1.55f) * dy;
                }
                dz = 0.5f * dz;
            }
        }

        private float getSpeed(float speed)
        {
            float spd = (speed * BASE_SPEED) / BASE_REAL_SPEED;
            float spdDash = 0;

            if (speed > 140)
            {
                spdDash = SPEED_GRADIENT * (speed - 140);
            }
            spd += spdDash;

            //////////UnityEngine.//Debug.Log("======================>>speed = " + speed);
            //////////UnityEngine.//Debug.Log("======================>>spdDash = " + spdDash);
            //////////UnityEngine.//Debug.Log("======================>>spd = " + spd);
            return -spd;
        }

        private float getTime(float distance)
        {
            float t = (ZONE_DISTANCE) / distance;
            //////////UnityEngine.//Debug.Log("======================>>t = " + t);
            return t;
        }

        float getDX(float xPos, float dTime)
        {
            float gab = xPos + startOffsetX + pHandSign * INIT_BALL_GAB;
            return (gab / dTime);
        }

        float getDY(float yPos, float dTime)
        {
            float acc = 0.5f * gravity_acc * dTime * dTime; //float acc = 0.5f * GRAVITY_ACC * dTime * dTime;  //gravity_acc
            //////////UnityEngine.//Debug.Log("======================>>acc = " + acc);


            float gab = ((ZONE_HEIGHT + yPos + ZONE_CALIBRATE_Y + startOffsetY) - (INIT_BALL_HEIGHT + pitchTypeOffsetY));

            float dh = (gab - acc) / dTime;
            //////////UnityEngine.//Debug.Log("======================>>addDY = " + addDY + "  ===>>dh = " + dh);

            return (dh + addDY);
        }


        public float getCurrentZoneX(float maxX)
        {
            return pX * maxX / STRIKE_ZONE_WIDTH;
        }

        public float getCurrentZoneY(float maxY)
        {
            return pY * maxY / STRIKE_ZONE_HEIGHT;
        }



        public float getArriveZoneX(float maxX)
        {
            return arriveX * maxX / STRIKE_ZONE_WIDTH;
        }

        public float getArriveZoneY(float maxY)
        {
            return arriveY * maxY / STRIKE_ZONE_HEIGHT;
        }

        public void setPitchCursor(bool bActive)
        {
            pPointer.GetComponent<tk2dSprite>().GetComponent<Renderer>().enabled = bActive;
        }

        public void setVectorInit()
        {
            //////UnityEngine.//Debug.Log("==============>>setVectorInit");
            x = INITX - pHandSign * INIT_BALL_GAB;
            y = INITY + INIT_BALL_HEIGHT;
            z = END_DISTANCE;

            dz = 0;
            dx = 0; //15;
            dy = 0;

            bRelease = false;
            bBallHit = false;

            ball.transform.localPosition = new Vector3(x, y, z);
        }

        public void setZone(float x, float y, float maxX, float maxY)
        {
            zoneX = x * STRIKE_ZONE_WIDTH / maxX;
            zoneY = y * STRIKE_ZONE_HEIGHT / maxY;
        }

        /// <summary>
        /// PVP모드 전용 존세팅
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        public void setPvpZoneInit(float x, float y, float maxX, float maxY)
        {
            setZone(x, y, maxX, maxY);
            movePointer(zoneX, zoneY);
            setPitchCursor(true);
        }


        public void setVector()
        {
            //CPlayer pitcher =  new CPlayer();   //임시
            //setPitcher(pitcher);

            breakPointZ = -1000;
            x = INITX - pHandSign * INIT_BALL_GAB;
            y = INITY + INIT_BALL_HEIGHT;
            z = INITZ;

            ball.transform.localPosition = new Vector3(x, y, z);

            shadowY = INITY;
            ballSpeed = pBallSpeed;    //직구인 경우 얻어지는 스피드
            speedGun = pBallSpeed;
            zoneMoveX = zoneMoveY = 0;
            offsetX = offsetY = 0;
            movementAX = movementAY = 0;
            startOffsetX = startOffsetY = 0;
            addDY = 0;

            //타입별 디테일
            //////UnityEngine.//Debug.Log("================>> moveType = " + moveType);
            setBallDetail(moveType);

            y += pitchTypeOffsetY;
            dz = getSpeed(ballSpeed);
            float dTime = getTime(dz);

            setCurvatureAcc(moveType, dTime);

            //////UnityEngine.//Debug.Log("================>> zoneX = " + zoneX + "====> offsetX" + offsetX);
            //////UnityEngine.//Debug.Log("================>> zoneY = " + zoneY + "====> offsetY" + offsetY);

            dx = getDX(zoneX + offsetX, dTime); //15;
            dy = getDY(zoneY + offsetY, dTime);// 25;

            if (bMissCase == true)
            {
                //실투인 경우
                zoneDX = 0;
                zoneDY = 0;
            }

            bBreak = false;
            bRelease = true;
            bZoneCheck = false;
            bFinish = false;
            bCursorCheck = false;
            bBallHit = false;
            bWildPitch = false;

            setArrivePos(dz);

            //ballObj.renderer.enabled = true;  //3D시 되돌려
            ballObjSpr.GetComponent<Renderer>().enabled = true; //2D스프라이트시 되돌려
            bHit = false;
            

            perfectTime = dTime;
            //////UnityEngine.//Debug.Log("========================================>>[벡터] dtime =" + dTime);
            //////UnityEngine.//Debug.Log("========================================>>[벡터] dx =" + dx);
            //////UnityEngine.//Debug.Log("========================================>>[벡터] dy =" + dy);
            //////UnityEngine.//Debug.Log("========================================>>[벡터] dz =" + dz);
            //////UnityEngine.//Debug.Log("========================================>>[벡터] breakPointZ =" + breakPointZ);
            //////UnityEngine.//Debug.Log("========================================>>[벡터] acc =" + gravity_acc);

            //StartCoroutine(setTail(dTime * 0.7f));

            /*if (bStopBall == true)
            {
                StartCoroutine(setStopBall());
            }
            else if (bTomahawk == true)
            {
                StartCoroutine(setTomahawk());
            }*/

            /*
            if (bBlurEffect == true)
            {
                StartCoroutine(setBlurEffect());
            } */           
            
            StartCoroutine(setTailing());            
        }


        private IEnumerator setTailing()
        {
            yield return new WaitForSeconds(perfectTime * 0.7f);

            while (true)
            {
                Util.Load("MainGame/prefabs/BattingViewPrefab/ballTracePrefab", transform, ball.transform.localPosition);

                if (bZoneCheck == true || bHit == true)
                {
                    break;
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        /*
        //스톱볼 세팅
        private IEnumerator setStopBall()
        {
            float stopTime1 = perfectTime * Random.Range(0.65f, 0.85f);
            yield return new WaitForSeconds(stopTime1);
            bRelease = false;
            effectAnim(effectAnim1,10,"stopball" , true);
            effectAnim1.renderer.enabled = true;
            effectAnim1.transform.localPosition = Vector3.zero;
            float stopTime2 = Random.Range(0.5f, 1.5f);
            yield return new WaitForSeconds(stopTime2);
            effectAnim1.renderer.enabled = false;
            bRelease = true;
        }

        private IEnumerator setTomahawk()
        {
            float stopTime1 = perfectTime * 0.4f;
            yield return new WaitForSeconds(stopTime1);
            bRelease = false;
            float stopTime2 = 1.0f;
            yield return new WaitForSeconds(stopTime2);
            effectAnim1.renderer.enabled = false;
            tail.SetActive(true);
            bRelease = true;
        }*/

        /*
        private IEnumerator setBlurEffect()
        {
            //tail.SetActive(true);
            BallPlayManager.GetInstance().battingview.setJustMeet(true, "hitfocus2", 1.4f, 0.5f);
            CameraManager.SetBlur2(true, 2, 0, 1);

            float delay = perfectTime * 0.6f;
            yield return new WaitForSeconds(delay);

            BallPlayManager.GetInstance().battingview.setJustMeet(false);
            CameraManager.SetBlur2(false);
        }*/

        /*
        //광속구 테일
        private IEnumerator setTail(float delay)
        {
            yield return new WaitForSeconds(delay);
            tail.SetActive(true);            
        }*/


        private void setBallDetail(BallMoveType type)
        {
            //////UnityEngine.//Debug.Log("================>>setBallDetail");
            /*if (bActiveSkillOn == true)
            {
                setActiveBallDetail();
            }
            else*/
            {
                bBigCurve = false;
                gravity_acc = GRAVITY_ACC_FAST;

                if (type == BallMoveType.Straight)
                {
                    speedRate = 1.0f;
                    
                    gravity_acc = GRAVITY_ACC_FAST;
                    startOffsetX = 0;
                    startOffsetY = ballSpeed < OFFSET_UNIT ? -((OFFSET_UNIT - ballSpeed) / OFFSET_UNIT) : 0;// ;
                    pitchTypeOffsetY = 0;

                    setStraightDetail();
                    //////////UnityEngine.//Debug.Log("================>>startOffsetY = " + startOffsetY);                
                }
                else if (type == BallMoveType.Curve)
                {                    
                    speedRate = 1.0f;
                    gravity_acc = GRAVITY_ACC_CURVE;
                    setCurveDetail();
                    breakPointZ = ZONE_DISTANCE * breakPoint;
                    startOffsetX = 0;
                    startOffsetY = ballSpeed < OFFSET_UNIT ? -((OFFSET_UNIT - ballSpeed) / OFFSET_UNIT) : 0;// ;
                    pitchTypeOffsetY = 7;
                
                }
                else if (type == BallMoveType.Slide)
                {                    
                    speedRate = 1.0f;
                    gravity_acc = GRAVITY_ACC_SLIDER;
                    setSliderDetail();
                    breakPointZ = ZONE_DISTANCE * breakPoint;
                    startOffsetX = 0;
                    startOffsetY = ballSpeed < OFFSET_UNIT ? -((OFFSET_UNIT - ballSpeed) / OFFSET_UNIT) : 0;// ;
                    pitchTypeOffsetY = 5;
                
                }
                else if (type == BallMoveType.OffSpeed)
                {                    
                    speedRate = 1.0f;
                    gravity_acc = GRAVITY_ACC_OFFSPEED;
                    setOffspeedDetail();
                    breakPointZ = ZONE_DISTANCE * breakPoint;
                    startOffsetX = 0;
                    startOffsetY = ballSpeed < OFFSET_UNIT ? -((OFFSET_UNIT - ballSpeed) / OFFSET_UNIT) : 0;// ;                
                }
                //원래 직구변화구 계열 - 나중에 살려서 고쳐
                else if (type == BallMoveType.FastBreaking)
                {                    
                    speedRate = 1.0f;
                    gravity_acc = GRAVITY_ACC_FASTBREAK;
                    setFastbreakDetail();
                    breakPointZ = ZONE_DISTANCE * breakPoint;
                    startOffsetX = 0;
                    startOffsetY = ballSpeed < OFFSET_UNIT ? -((OFFSET_UNIT - ballSpeed) / OFFSET_UNIT) : 0;// ;
                    pitchTypeOffsetY = 0;
                }
            }
        }

        void setStraightDetail()
        {
            bRising = false;
            if (ballType == PitchingArsenal.UPSHOOT)
            {
                bRising = true;
                //breakPoint = 0.35f;
                setSpeed(0.85f * speedRate, 70);
                offsetX = 0;//
                movementAX = 0;
                movementAY = 8 * pMovement2;
                curvatureAY = -5;
                pitchTypeOffsetY = 0;
            }

        }

        //구종여기고쳐 - 커브 타입 재편성및 추가 삭제
        void setCurveDetail()
        {
            bBigCurve = true;
            breakPoint = 0.5f;
            if (ballType == PitchingArsenal.CURVE)
            {
                //일반 커브
                setSpeed(0.856f * speedRate, 100);
                offsetX = -1 * pHandSign * CURVE_CURVATURE; //왼손잡이 부호반대
                movementAX = 5 * pHandSign * pMovement2;
                movementAY = -10 * pMovement2;
                curvatureAY = 0;// type 1

            }
            else if (ballType == PitchingArsenal.POWER_CURVE)
            {
                setSpeed(0.92f * speedRate, 100);
                offsetX = -1 * pHandSign * POWER_CURVE_CURVATURE; //왼손잡이 부호반대
                movementAX = 1.5f * pHandSign * pMovement2;  //2 * pHandSign * pMovement2;
                movementAY = -15 * pMovement2;//-10 * pMovement2;
                curvatureAY = 12;// type 1
                //addDY = 50;    //type 2   
            }
            else if (ballType == PitchingArsenal.SLOW_CURVE)
            {
                setSpeed(0.72f * speedRate, 70);
                offsetX = -1 * pHandSign * SLOW_CURVE_CURVATURE; //왼손잡이 부호반대
                movementAX = 3 * pHandSign * pMovement2;
                movementAY = -10 * pMovement2;
                curvatureAY = 0;// type 1
            }
            else if (ballType == PitchingArsenal.POKPOSU_CURVE)
            {
                setSpeed(0.888f * speedRate, 100);
                offsetX = 0;// -1 * pHandSign * _12_6_CURVE_CURVATURE; //왼손잡이 부호반대
                movementAY = -10 * pMovement2;
                curvatureAY = 10;// type 1
                //addDY = 50;    //type 2   

            }
            else if (ballType == PitchingArsenal.KNUCKLE_CURVE)
            {
                setSpeed(0.736f * speedRate, 70);
                offsetX = -1 * pHandSign * KNUCKLE_CURVE_CURVATURE; //왼손잡이 부호반대
                movementAX = -5 * Random.RandomRange(-1.0f, 1.0f) * pHandSign * pMovement2;
                movementAY = -10 * pMovement2;
                curvatureAY = 7;// type 1
                //addDY = 50;    //type 2 
            }
            else if (ballType == PitchingArsenal.GIRO_CURVE)
            {
                setSpeed(0.736f * speedRate, 70);
                offsetX = -1 * pHandSign * SCREW_CURVATURE; //왼손잡이 부호반대
                movementAX = -5 * pHandSign * pMovement2;
                movementAY = -10 * pMovement2;
                curvatureAY = 20;// 20;// type 1
            }

            
        }


        //구종여기고쳐 - 슬라이더 타입 재편성및 추가 삭제
        void setSliderDetail()
        {
            breakPoint = 0.65f;
            if (ballType == PitchingArsenal.SLIDER)
            {
                setSpeed(0.9f * speedRate, 110);
                offsetX = -1 * pHandSign * SLIDER_CURVATURE; //왼손잡이 부호반대
                movementAX = 20 * pHandSign * pMovement1;
                movementAY = -4 * pMovement1;
                curvatureAY = 0;// type 1
            }
            
            else if (ballType == PitchingArsenal.H_SLIDER)
            {
                setSpeed(0.954f * speedRate, 110);
                offsetX = -1 * pHandSign * H_SLIDER_CURVATURE; //왼손잡이 부호반대
                movementAX = 25 * pHandSign * pMovement1;
                movementAY = 0;
                curvatureAY = 0;// type 1
            }
                /*
            else if (ballType == PitchingArsenal.V_SLIDER)
            {
                //일반 커브
                //speedGun = ballSpeed * 0.87f;
                //ballSpeed = speedGun;//ballSpeed * 0.85f;
                setSpeed(0.87f * speedRate, 125);
                offsetX = -1 * pHandSign * V_SLIDER_CURVATURE; //왼손잡이 부호반대
                movementAX = 0;//
                movementAY = -15 * pMovement1;
                curvatureAY = 0;// type 1
            }*/
            else if (ballType == PitchingArsenal.SLURVE)
            {
                bBigCurve = true;
                setSpeed(0.873f * speedRate, 110);
                offsetX = -1 * pHandSign * SLUVE_CURVATURE; //왼손잡이 부호반대
                movementAX = 12 * pHandSign * pMovement1;
                movementAY = -12 * pMovement1;
                curvatureAY = 0;// type 1
            }
            else if (ballType == PitchingArsenal.FRISBEE)
            {
                breakPoint = 0.5f;
                setSpeed(0.855f * speedRate, 110);
                offsetX = -4 * pHandSign * SLIDER_CURVATURE; //왼손잡이 부호반대
                movementAX = 18 * pHandSign * pMovement1;
                movementAY = 0;
                curvatureAY = 0;// type 1
            }
            else if (ballType == PitchingArsenal.SINKER)
            {
                setSpeed(0.954f * speedRate, 110);
                offsetX = -1 * pHandSign * SINKER_CURVATURE; //왼손잡이 부호반대
                movementAX = -10 * pHandSign * pMovement1;
                movementAY = -12 * pMovement1;
                curvatureAY = 0;// type 1

            }
        }

        //구종여기고쳐 - 일부는 직구류 일부는 다른 변화구류로 재편성
        void setFastbreakDetail()
        {
            breakPoint = 0.7f;
            curvatureAY = 0;
            if (ballType == PitchingArsenal.TWOSEAM)
            {
                //speedGun = ballSpeed * 0.96f;
                //ballSpeed = speedGun;//ballSpeed * 0.96f;
                setSpeed(0.96f * speedRate, 120);
                offsetX = -1 * pHandSign * TWOSEAM_FAST_CURVATURE; //왼손잡이 부호반대
                movementAX = -11 * pHandSign * pMovement4;
                movementAY = -3 * pMovement4;
            }
            else if (ballType == PitchingArsenal.RISING)
            {
                //RISING_FAST_CURVATURE
                setSpeed(speedRate, 130);
                offsetX = 0;// -1 * pHandSign * TWOSEAM_FAST_CURVATURE; //왼손잡이 부호반대
                movementAX = 0;
                movementAY = 6 * pMovement4;
            }
            else if (ballType == PitchingArsenal.SINKING_FAST)
            {
                setSpeed(0.98f * speedRate, 120);
                offsetX = -1 * pHandSign * SINKING_FAST_CURVATURE; //왼손잡이 부호반대
                movementAX = -4 * pHandSign * pMovement4;
                movementAY = -10 * pMovement4;
            }
            else if (ballType == PitchingArsenal.CUT_FAST)
            {
                setSpeed(0.98f * speedRate, 120);
                offsetX = -1 * pHandSign * CUT_FAST_CURVATURE; //왼손잡이 부호반대
                movementAX = 10 * pHandSign * pMovement4;
                movementAY = -4 * pMovement4;
            }
            
            else if (ballType == PitchingArsenal.UPSHOOT)
            {
                breakPoint = 0.35f;
                setSpeed(0.85f * speedRate, 70);
                offsetX = 0;//
                movementAX = 0;
                movementAY = 8 * pMovement2;
                curvatureAY = 0;
            }
        }

        //구종여기고쳐 - 체인지업과 포크류로 나눔
        void setOffspeedDetail()
        {
            if (ballType == PitchingArsenal.CHANGEUP)
            {
                breakPoint = 0.7f;
                setSpeed(0.82f * speedRate, 100);
                offsetX = 0;// 
                movementAX = 0;
                movementAY = -15 * pMovement3;
                curvatureAY = 0;
                addDY = -31;
                pitchTypeOffsetY = 0;
            }
            else if (ballType == PitchingArsenal.CIRCLE)
            {
                breakPoint = 0.7f;
                setSpeed(0.84f * speedRate, 100);
                offsetX = -1 * pHandSign * CIRCLE_CURVATURE; //왼손잡이 부호반대
                movementAX = -6 * pHandSign * pMovement3;
                movementAY = -12 * pMovement3;
                curvatureAY = 0;
                addDY = -31;
                pitchTypeOffsetY = 0;
            }
            else if (ballType == PitchingArsenal.VULCAN)
            {
                breakPoint = 0.7f;
                setSpeed(0.84f * speedRate, 100);
                offsetX = -1 * pHandSign * CIRCLE_CURVATURE; //왼손잡이 부호반대
                movementAX = -20 * pHandSign * pMovement3;
                movementAY = -12 * pMovement3;
                curvatureAY = 0;
                addDY = -31;
                pitchTypeOffsetY = 0;
            }            
            else if (ballType == PitchingArsenal.PALM)
            {
                bBigCurve = true;
                breakPoint = 0.5f;
                speedGun = ballSpeed * 0.77f;
                setSpeed(0.77f * speedRate, 70);
                offsetX = 0;// 
                movementAX = 0;
                movementAY = -7 * pMovement3;
                curvatureAY = 0;
                addDY = 0;
                pitchTypeOffsetY = 0;
            }
            else if (ballType == PitchingArsenal.KNUCKLE)
            {
                bBigCurve = true;
                breakPoint = 0.5f;
                speedGun = ballSpeed * 0.65f;
                ballSpeed = speedGun;//ballSpeed * 0.5f;
                setSpeed(0.65f * speedRate, 50);
                offsetX = Random.Range(-12.0f, 12.0f); //왼손잡이 부호반대
                movementAX = Random.Range(-3.0f, 3.0f) * pHandSign * pMovement3;
                movementAY = Random.Range(-3.0f, 1.0f) * pMovement3;
                curvatureAY = 0;
                addDY = -60;
                pitchTypeOffsetY = 0;
            }
            //포크류
            else if (ballType == PitchingArsenal.FORK)
            {
                breakPoint = 0.6f;
                setSpeed(0.87f * speedRate, 100);
                offsetX = 0;// 
                movementAX = 0;
                movementAY = -8 * pMovement3;
                curvatureAY = 0;
                addDY = -10;
                pitchTypeOffsetY = 5;
            }
            else if (ballType == PitchingArsenal.H_FORK)
            {
                breakPoint = 0.6f;
                setSpeed(0.96f * speedRate, 100);
                offsetX = 0;// 
                movementAX = 0;
                movementAY = -10 * pMovement3;
                curvatureAY = 0;
                addDY = -20;
                //pitchTypeOffsetY = 5;
            }
            else if (ballType == PitchingArsenal.SFF)
            {
                breakPoint = 0.75f;
                setSpeed(0.985f * speedRate, 100);
                offsetX = 0;// 
                movementAX = 0;
                movementAY = -14 * pMovement4;
                curvatureAY = 0;
                addDY = -25;
                pitchTypeOffsetY = 5;
            }
        }

        public void setMiss(bool bMiss)
        {
            bMissCase = bMiss;
        }

        /*
        //액티브 스킬에 의한 공 궤적
        private void setActiveBallDetail()
        {
            //////UnityEngine.//Debug.Log("================>>setBallDetail");
            bBigCurve = false;
            gravity_acc = GRAVITY_ACC_FAST;

            if (bTomahawk == true)
            {
                speedRate = 1.0f;
                gravity_acc = GRAVITY_ACC_OFFSPEED;
                bBigCurve = true;
                breakPoint = 0.3f;
                //speedGun = ballSpeed * 0.77f;
                //setSpeed(0.77f * speedRate, 70);
                movementAX = movementAY = 0;
                offsetX = 0;// 
                curvatureAY = 0;
                addDY = 250;
                pitchTypeOffsetY = 0;
                breakPointZ = ZONE_DISTANCE * breakPoint;
                startOffsetX = 0;
                startOffsetY = ballSpeed < OFFSET_UNIT ? -((OFFSET_UNIT - ballSpeed) / OFFSET_UNIT) : 0;// ;      
            }
        }*/


        /*
        float getZoneDX(float dTime)
        {
            float bPoint = (1 - breakPoint) * (1 - breakPoint);
            float bTime = dTime * breakPoint;
            float s = bPoint * movementAX * bTime * bTime;

            return (s / bTime);

        }
        float getZoneDY(float dTime)
        {
            float bPoint = (1 - breakPoint) * (1 - breakPoint);
            float bTime = dTime * breakPoint;
            float s = bPoint * movementAY * bTime * bTime;

            return (s / bTime);
        }
        */

        float getZoneDV(float movement, float dTime)
        {
            float bTime = dTime * (1 - breakPoint);
            float s = 0.5f * movement * bTime * bTime;

            return (s / bTime);
        }


        void setCurvatureAcc(BallMoveType type, float dTime)
        {
            float bPoint = (1 - breakPoint) * (1 - breakPoint);

            if (type == BallMoveType.Straight)
            {
                //원래 이거
                curvatureAX = 0;
                zoneDX = 0;
                zoneDY = 0;
            }
            else if (type == BallMoveType.Curve)
            {
                //float btime =
                curvatureAX = (-offsetX * 2.0f) / (bPoint * dTime * dTime);
                //curvatureAY = -addDY * 2.0f / dTime; //type 2
                //////////UnityEngine.//Debug.Log("=======================>>curvatureAY  = " + curvatureAY);
                zoneDX = getZoneDV(movementAX, dTime); // getZoneDX(dTime);
                zoneDY = getZoneDV(movementAY, dTime); //getZoneDY(dTime);
            }
            else if (type == BallMoveType.Slide)
            {
                curvatureAX = (-offsetX * 2.0f) / (bPoint * dTime * dTime);
                //curvatureAY = -addDY * 2.0f / dTime; //type 2
                //////////UnityEngine.//Debug.Log("=======================>>curvatureAY  = " + curvatureAY);
                zoneDX = getZoneDV(movementAX, dTime); // getZoneDX(dTime);
                zoneDY = getZoneDV(movementAY, dTime); // getZoneDY(dTime); 
            }

            else if (type == BallMoveType.OffSpeed)
            {
                curvatureAX = (-offsetX * 2.0f) / (bPoint * dTime * dTime);
                curvatureAY = -addDY * 2.0f / dTime; //type 2
                //////////UnityEngine.//Debug.Log("=======================>>curvatureAY  = " + curvatureAY);
                zoneDX = getZoneDV(movementAX, dTime); // getZoneDX(dTime);
                zoneDY = getZoneDV(movementAY, dTime); // getZoneDY(dTime); 
            }
            else if (type == BallMoveType.FastBreaking)
            {
                curvatureAX = (-offsetX * 2.0f) / (bPoint * dTime * dTime);
                zoneDX = getZoneDV(movementAX, dTime); // getZoneDX(dTime);
                zoneDY = getZoneDV(movementAY, dTime); // getZoneDY(dTime); 
            }
        }


        void move()
        {
            if (z <= END_DISTANCE)
            {
                if (bBallHit == false)
                {
                    setPitchCursor(false);
                    dx = dx = dz = 0;
                    bRelease = false;
                    bFinish = true;
                    //tail.SetActive(false);
                }
            }
            else
            {
                x += dx * Time.deltaTime;
                y += dy * Time.deltaTime;
                z += dz * Time.deltaTime;


                dy += gravity_acc * (Time.deltaTime); //dy += GRAVITY_ACC * (Time.deltaTime);// *Time.deltaTime);

                if (bMyTurn == false)
                {
                    float a = (z / END_DISTANCE);
                    cursorSpr.color = new Vector4(1, 1, 1, a);
                }

                if (z < HALF_CHECK && y < -3f)
                {
                    bWildPitch = true;
                    dy = -dy;
                }

                //존 체크
                if (bZoneCheck == false)
                {
                    if (z <= ZONE_CHECK2)//ZONE_CHECK)
                    {
                        //존을 지날때 힛바이 피치 체크
                        if (bHitByPitched == true)
                        {
                            //ballObj.renderer.enabled = false;//3D시 되돌려
                            ballObjSpr.GetComponent<Renderer>().enabled = false;//2D스프라이트시 되돌려
                            //tail.SetActive(false);
                            bHitByPitched = false;
                        }
                        setPitchCursor(false);
                        bZoneCheck = true;
                        removeBall();
                    }
                }

                //힛체크
                if (bHit == true)
                {
                    if (z <= ZONE_CHECK)
                    {
                      
                        setPitchCursor(false);
                        //ballObj.renderer.enabled = false;//3D시 되돌려
                        ballObjSpr.GetComponent<Renderer>().enabled = false;//2D스프라이트시 되돌려

                        //tail.SetActive(false);
                        bHit = false;
                    }
                }
                

                moveDetail();

                //if (bShowBallTrace == true)
                {
                    if (bCursorCheck == false)
                    {
                        setPitchCursor(true);
                        bCursorCheck = true;
                    }
                }

                /*
                //블랙홀 체크
                if (bBlackHole == true)
                {
                    if (z <= ZONE_CHECK + 10)
                    {
                        x = y = dz = 0;
                        z = ZONE_CHECK + 10;
                    }
                }*/

                ball.transform.localPosition = new Vector3(x, y, z);
                
            }
        }

        private void removeBall()
        {
            //yield return new WaitForSeconds(0.025f);  //클래식 뷰
            //yield return new WaitForSeconds(0.01f);     //배팅뷰 로우

            //ballObj.renderer.enabled = false;//3D시 되돌려
            ballObjSpr.GetComponent<Renderer>().enabled = false;//2D스프라이트시 되돌려
            tail.SetActive(false);
        }



        private void moveDetail()
        {
            /*if (bActiveSkillOn == true)
            {
                moveActiveBallDetail();                
            }
            else*/
            {
                if (moveType == BallMoveType.Straight)
                {
                    if (bRising == true)
                    {
                        if (z < breakPointZ)
                        {
                            dx += curvatureAX * (Time.deltaTime);

                            dx += (movementAX * Time.deltaTime);
                            dy += (movementAY * Time.deltaTime);

                            //y -= curvatureAY * Time.deltaTime; //type 1

                            if (bZoneCheck == false)
                            {
                                zoneMoveX += (zoneDX * Time.deltaTime);
                                zoneMoveY += (zoneDY * Time.deltaTime);
                            }
                        }
                    }
                }
                else if (moveType == BallMoveType.Curve)
                {
                    if (z < breakPointZ)
                    {
                        dx += curvatureAX * (Time.deltaTime);

                        dx += (movementAX * Time.deltaTime);
                        dy += (movementAY * Time.deltaTime);

                        y -= curvatureAY * Time.deltaTime; //type 1

                        if (bZoneCheck == false)
                        {
                            zoneMoveX += (zoneDX * Time.deltaTime);
                            zoneMoveY += (zoneDY * Time.deltaTime);
                        }
                    }
                    else
                    {
                        y += curvatureAY * Time.deltaTime; //type 1
                    }
                    //dy += curvatureAY * (Time.deltaTime); //type 2
                }
                else if (moveType == BallMoveType.Slide)
                {
                    if (z < breakPointZ)
                    {
                        dx += curvatureAX * (Time.deltaTime);

                        dx += (movementAX * Time.deltaTime);
                        dy += (movementAY * Time.deltaTime);

                        //y -= curvatureAY * Time.deltaTime; //type 1

                        if (bZoneCheck == false)
                        {
                            zoneMoveX += (zoneDX * Time.deltaTime);
                            zoneMoveY += (zoneDY * Time.deltaTime);
                        }
                    }
                }
                else if (moveType == BallMoveType.OffSpeed)
                {
                    if (z < breakPointZ)
                    {
                        dx += curvatureAX * (Time.deltaTime);

                        dx += (movementAX * Time.deltaTime);
                        dy += (movementAY * Time.deltaTime);


                        if (bZoneCheck == false)
                        {
                            zoneMoveX += (zoneDX * Time.deltaTime);
                            zoneMoveY += (zoneDY * Time.deltaTime);
                        }
                    }
                    dy += curvatureAY * (Time.deltaTime); //type 2
                }

                else if (moveType == BallMoveType.FastBreaking)
                {
                    if (z < breakPointZ)
                    {
                        dx += curvatureAX * (Time.deltaTime);

                        dx += (movementAX * Time.deltaTime);
                        dy += (movementAY * Time.deltaTime);

                        //y -= curvatureAY * Time.deltaTime; //type 1

                        if (bZoneCheck == false)
                        {
                            zoneMoveX += (zoneDX * Time.deltaTime);
                            zoneMoveY += (zoneDY * Time.deltaTime);
                        }
                    }
                }
            }
            //float px = zoneX + zoneMoveX;
            //float py = zoneY + zoneMoveY;
            //movePointer(px, py);
            pX = zoneX + zoneMoveX;
            pY = zoneY + zoneMoveY;
            movePointer(pX, pY);
        }

        /*
        private void moveActiveBallDetail()
        {
            if (bTomahawk == true)
            {
                if (z < breakPointZ)
                {
                    dx += curvatureAX * (Time.deltaTime);

                    dx += (movementAX * Time.deltaTime);
                    dy += (movementAY * Time.deltaTime);


                    if (bZoneCheck == false)
                    {
                        zoneMoveX += (zoneDX * Time.deltaTime);
                        zoneMoveY += (zoneDY * Time.deltaTime);
                    }
                }
                dy += curvatureAY * (Time.deltaTime); //type 2
            }
        }*/


        void movePointer(float x, float y)
        {
            pPointer.transform.localPosition = new Vector3(x, y, 0);
        }


        bool checkBreak()
        {
            if (bBreak == false)
            {
                if (z < breakPointZ)
                {
                    bBreak = true;
                    return true;
                }
            }
            return false;
        }

        void setSpeed(float rate, float minSpeed)
        {
            /*
            speedGun = ballSpeed * rate;
            if (ballSpeed > minSpeed)
            {
                if (speedGun < minSpeed)
                {
                    speedGun = (int)Random.Range(minSpeed, (minSpeed + 4));
                }
            }
            else
            {
                if (speedGun < 80)
                {
                    speedGun = (int)(Random.Range(80, 85));
                }
            }
            ballSpeed = speedGun;*/
            speedGun = ballSpeed;
        }

        public bool checkCount()
        {
            return bFinish;
        }

        public float getHitRate()
        {
            float gab = ((dz + BASE_SPEED) * 0.15f);//0.07f);
            //////////UnityEngine.//Debug.Log("======================>>spd = " + dz);
            //////////UnityEngine.//Debug.Log("======================>>gab = " + gab);
            return ((z + gab) / ZONE_CHECK);
        }

        public int getBallSpeed()
        {
            return (int)(speedGun);///speedRate);
        }


        ////
        public float curOffsetX, curOffsetY;

        public void getCurArrivePos(float maxX, float maxY, float curBallSpeed)
        {
            ballSpeed = curBallSpeed;
                        
            setBallDetail(moveType);

            float speed = getSpeed(ballSpeed);
            float dTime = getTime(speed);
            setCurvatureAcc(moveType, dTime);

            ////UnityEngine.Debug.Log("#######################>>>speed = " + speed);
            ////UnityEngine.Debug.Log("#######################>>>breakPoint = " + breakPoint);

            float t = (ZONE_CHECK) / speed;
            float bt = t * (1 - breakPoint);

            float moveX = zoneDX * bt;
            float moveY = zoneDY * bt;

            curOffsetX = moveX * maxX / STRIKE_ZONE_WIDTH;
            curOffsetY = moveY * maxY / STRIKE_ZONE_HEIGHT;

            ////////UnityEngine.//Debug.Log("===============>>curOffsetX = " + curOffsetX);
            ////////UnityEngine.//Debug.Log("===============>>curOffsetY = " + curOffsetY);

            perfectTime = dTime;

        }


        Vector3 cursorInitVector;

        public void releaseIndicator()
        {
            IngameUI.GetPitchUI().SetActive(false);
            indicator.active(false);
        }

        public void indicatorUpdate(float x, float y)
        {
            float curX = x * STRIKE_ZONE_WIDTH / Zone.STRIKE_ZONE_WIDTH;
            float curY = y * STRIKE_ZONE_HEIGHT / Zone.STRIKE_ZONE_HEIGHT;
            indicator.updateLine(curX, curY);
            cursorBall.transform.position = cursorInitVector + new Vector3(curX, curY, 0);
        }

        public void setIndicator(Pitcher pitcher)
        {
            if (pitcher.pitchingType == 2)
            {
                //언더핸드
                INIT_BALL_GAB = UNDER_INIT_BALL_GAB;
                INIT_BALL_HEIGHT = UNDER_INIT_BALL_HEIGHT;
            }
            else if (pitcher.pitchingType == 1)
            {
                //사이드
                INIT_BALL_GAB = SIDE_INIT_BALL_GAB;
                INIT_BALL_HEIGHT = SIDE_INIT_BALL_HEIGHT;
            }
            else
            {
                //오버핸드
                INIT_BALL_GAB = OVER_INIT_BALL_GAB;
                INIT_BALL_HEIGHT = OVER_INIT_BALL_HEIGHT;
            }
            pHandSign = pitcher.nSign;// (pitcher.m_nThrowHand == CPlayer._RIGHTHAND ? 1 : -1);
            pBallSpeed = pitcher.curBallSpeed;// 150;
            setMoveMent(pitcher);
            bTip = false;
            bBound = false;
            speedRate = 1;

            breakPointZ = -1000;
            x = INITX - pHandSign * INIT_BALL_GAB;
            y = INITY + INIT_BALL_HEIGHT;
            z = INITZ;
            shadowY = INITY;
            ballSpeed = pBallSpeed;    //직구인 경우 얻어지는 스피드
            speedGun = pBallSpeed;
            zoneMoveX = zoneMoveY = 0;
            offsetX = offsetY = 0;
            movementAX = movementAY = 0;
            startOffsetX = startOffsetY = 0;
            addDY = 0;

            //타입별 디테일
            //////UnityEngine.//Debug.Log("================>> moveType = " + moveType);
            setBallDetail(moveType);

            y += pitchTypeOffsetY;
            dz = getSpeed(ballSpeed);
            float dTime = getTime(dz);
            setCurvatureAcc(moveType, dTime);
           
            dx = getDX(zoneX + offsetX, dTime); 
            dy = getDY(zoneY + offsetY, dTime);
            bBreak = false;
            bZoneCheck = false;
            bFinish = false;
            bCursorCheck = false;
            bBallHit = false;
            setArrivePos(dz);

            //라인 만들기
            float limitTime = (ZONE_CHECK / dz) / PitchIndicator.MAX_LINE;
            Vector3 initVector = transform.position + new Vector3(x, y, z);
            pPointer.transform.localPosition = Vector3.zero;
            Vector3 finalVector = pPointer.transform.position;
            Vector3[] pos = makeIndicatorPosition(limitTime, initVector, finalVector);
            cursorBall.transform.position = finalVector;
            IngameUI.GetPitchUI().SetActive(true);
            IngameUI.GetPitchUI().InitRotate(moveType, ballType, pitcher.nSign);
            indicator.makeLine(pos, indicatorLineNum);
            cursorInitVector = finalVector;
        }

        private Vector3[] makeIndicatorPosition(float limitTime, Vector3 initVector, Vector3 finalVector)
        {
            float deltaTime = Time.deltaTime; 
            Vector3[] ballPos = new Vector3[PitchIndicator.MAX_LINE];
            ballPos[0] = initVector;
            float curTime = 0;
            int index = 0;
            cursorBall.transform.position = initVector;

            float gabX, gabY;

            while (true)
            {
                if (z <= ZONE_CHECK)
                {
                    indicatorLineNum = index;
                    ballPos[indicatorLineNum - 1] = cursorBall.transform.position;
                    gabX = finalVector.x - cursorBall.transform.position.x;
                    gabY = finalVector.y - cursorBall.transform.position.y;
                    //////UnityEngine.//Debug.Log("===========================>>finalVectorX = " + finalVector.x + "===>finalVectorY = " + finalVector.y);
                    //////UnityEngine.//Debug.Log("===========================>>gabX = " + gabX + "===>gabY = " + gabY);
                    break;
                }
                else
                {
                    if (curTime > limitTime)
                    {
                        index++;
                        ballPos[index] = cursorBall.transform.position;
                        curTime = 0;
                    }

                    curTime += deltaTime;
                    x += dx * deltaTime;
                    y += dy * deltaTime;
                    z += dz * deltaTime;
                    dy += gravity_acc * (deltaTime);
                    cursorBall.transform.localPosition = new Vector3(x, y, z);
                    moveIndicatorDetail(deltaTime);
                }
            }

            for (int i = 0; i < indicatorLineNum; i++)
            {
                float rate = (float)(i+1) / (float)indicatorLineNum;
                //////UnityEngine.//Debug.Log("===========================>>gabX = " + (gabX * rate) + "===>gabY = " + (gabY * rate));
                ballPos[i] += new Vector3(gabX * rate, gabY * rate, 0);
            }

            ballPos[indicatorLineNum - 1] = finalVector;    //

            return ballPos;
            
        }

        private void moveIndicatorDetail(float deltaTime)
        {
            /*if (bActiveSkillOn == true)
            {
                moveIndicatorActiveDetail(deltaTime);                
            }
            else*/
            {
                if (moveType == BallMoveType.Straight)
                {
                    if (bRising == true)
                    {
                        if (z < breakPointZ)
                        {
                            dx += curvatureAX * (deltaTime);

                            dx += (movementAX * deltaTime);
                            dy += (movementAY * deltaTime);
                        }
                    }
                }
                else if (moveType == BallMoveType.Curve)
                {
                    if (z < breakPointZ)
                    {
                        dx += curvatureAX * (deltaTime);

                        dx += (movementAX * deltaTime);
                        dy += (movementAY * deltaTime);

                        y -= curvatureAY * deltaTime; //type 1


                    }
                    else
                    {
                        y += curvatureAY * deltaTime; //type 1
                    }
                    //dy += curvatureAY * (deltaTime); //type 2
                }
                else if (moveType == BallMoveType.Slide)
                {
                    if (z < breakPointZ)
                    {
                        dx += curvatureAX * (deltaTime);

                        dx += (movementAX * deltaTime);
                        dy += (movementAY * deltaTime);


                    }
                }
                else if (moveType == BallMoveType.OffSpeed)
                {
                    if (z < breakPointZ)
                    {
                        dx += curvatureAX * (deltaTime);

                        dx += (movementAX * deltaTime);
                        dy += (movementAY * deltaTime);


                    }
                    dy += curvatureAY * (deltaTime); //type 2
                }

                else if (moveType == BallMoveType.FastBreaking)
                {
                    if (z < breakPointZ)
                    {
                        dx += curvatureAX * (deltaTime);

                        dx += (movementAX * deltaTime);
                        dy += (movementAY * deltaTime);


                    }
                }
            }
        }

        /*
        private void moveIndicatorActiveDetail(float deltaTime)
        {
            if (bTomahawk == true)
            {
                if (z < breakPointZ)
                {
                    dx += curvatureAX * (deltaTime);

                    dx += (movementAX * deltaTime);
                    dy += (movementAY * deltaTime);


                }
                dy += curvatureAY * (deltaTime); //type 2
            }
        }*/

        

    }
}