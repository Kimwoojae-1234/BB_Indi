#define _NO_BASE_FOCUS      //베이스 포커스 없앰

using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    //카메라워크 타입
    public enum CameraWork
    {
        Default,
        Shadow_Chase,
        Ball_Chase,
        Ball_Throw,
        Popup,
        Make_Center,
        Move_Target,
        None
    };

    //볼 상태
    public enum BallState
    {
        BALL_PITCHING = 0,		//Pitching (Batting View)
        BALL_FLYING = 1,		//Fly Ball (Field View)
        BALL_FAIR = 2,		    //Fair Ball (Field View)
        BALL_FOUL = 10,         //Foul Ball (Field View)
        BALL_DEAD = 100	        //Ball Dead State
    }

    //볼 스텝
    public enum BallStep
    {
        None = -1,
        BALL_HIT = 0,		    // Ball Hitted by Batter
        BALL_THROW = 1,		    // Ball Throwed by Fielder
        BALL_CATCH = 2,		    // Ball Catched by Fielder
        BALL_CARRY = 3,		    // Ball Carried by Fielder who has the ball
        BALL_THROW_CATCH = 4,	// Ball Catched by Fielder
        BALL_WILD_PITCH = 5,
        BALL_DEAD_STATE = 11,
        BALL_ERROR_STATE = 20,
        BALL_STEAL = 30,		// 
        BALL_EVENT = 40,		//
        BALL_POLE_HIT = 50

    }

    //이벤트 스텝
    public enum BallEvent
    {
        //EVENT
        EVENT_NOTHING = 0,
        EVENT_HOMERUN = 10,
        EVENT_RUNNER_FOCUS = 20,
        EVENT_FIELDER_FOCUS = 30,
        EVENT_BASE_FOCUS = 40,		// 
        EVENT_JUDGE_FOCUS = 50,
        EVENT_FOCUS_MOVE = 60
    }

    public enum CameraRayCastType
    {
        Boarder,
        Crowd,
        None
    }

    public class FBall : MonoBehaviour
    {
        //오브젝트
        public GameObject ballObj;  //최상위
        public GameObject ballObj2, ballObj3;   //빈 오브젝트, 볼 모형
        public GameObject colliderObj;
        public GameObject particle, laserParticle;
        private TrailRenderer ballTrail;

        private Field field;
        private tk2dSprite spr;

        ////////////////////////////////////////////////////////////
        //물리 상수
        ////////////////////////////////////////////////////////////
        public const float _BALLSPEED_COEF = 50; //볼파워를 실 스피드로 치환시켜주는 계수 
        public const float NORMAL_DELTATIME = (0.01656688f);
        public const float _GRAVITY_ACCELERATION = -800;//
        public const float _GRAVITY_ACCELERATION_TOPSPIN = -1000;//-1000으로 체크체크
        public const float _GRAVITY_ACCELERATION_30 = -1000;// -1000.00f;
        public const float _GRAVITY_ACCELERATION_40 = -1200;// -1000.00f;
        public const float _GRAVITY_ACCELERATION_50 = -1400;// -1000.00f;
        public const float _GRAVITY_ACCELERATION_THROW = -600.00f;
        public const float _Z_AXIS_PROJECTION_COEFF = 0.85f;
        public const float _Z_AXIS_PROJECTION_COEFF2 = 1;//Z축 투영계수
        public const float _Z_AXIS_VECTOR_CAL_COEFF = 0.75f;//  z축 벡터 조정 계수

        public const float _AIR_FRICTION_COEFF = -100;// -150;      //타구
        public const float _AIR_FRICTION_COEFF2 = -150.0f;// -300.0f;   //송구
        public const float _BOUND_FRICTION_COEFF = -120;
        public const float _BOUND_SPEED_RATE = 0.8f;
        public const float _BOUND_SPEED_DECREASE = -3;
        //public const float _CAMERA1_Y_CALIBRATION = (float)(Field.SECOND_BASE_POSY - Field.HOME_BENCH_POSY) / (float)(Field.FIRST_BASE_POSX - Field.THIRD_BASE_POSX);
        //public const float _Y_AXIS_COEFF = (float)(Field.SECOND_BASE_POSY - Field.HOME_BENCH_POSY) / (float)(Field.FIRST_BASE_POSX - Field.THIRD_BASE_POSX);
                
        public const int initZOrder = 4;

        //상태값
        public BallState state;
        public BallStep step;
        public BallEvent eventStep;

        //시간
        public float curTime; 

        //벡터
        public float nBallX, nBallY;
        public float nBallDX, nBallDY, nBallDZ,
                     nBallAX, nBallAY,//, nBallY,
                     nScreenX, nScreenY,
                     screenDX, screenDY,
                     nDY,
                     OffsetX, OffsetY;

        public float nBallZ;		//공의 높이	좌표계와 상관없는 상대적인 값.	
        public float nScreenBallZ;	//화면에 투영되는 픽셀단위의 공높이..
        public float nBallDepth;//,lastBallDepth;	//공이 화면에 투영되는 뎁스..

        public float screenX, screenY;

        public float speed, nLastSpeed, firstSpeed;
        public float angle, nLastAngle, firstAngle;
        public float angleZ, nLastAngleZ, firstAngleZ;
        public float decreaseSpeed;
        public float throwWrist;    //손목힘        
        public float throwingMaxTime;

        private float rotationX, rotateSpeed;

        //플래그
        public bool bBallActive;
        public bool bBallStop,
                    bBallStopCheck,
                    bBallCatched,
                    bHighDivingCatched,
                    bBallHidden,
                    bBound;

        public bool bRotate;

        public bool bStunBall;
        
        //첫바운드 & 바운드
        public float nFirstBoundX, nFirstBoundY,
                     nSecondBoundX, nSecondBoundY,
                     screenFirstBoundX, screenFirstBoundY,
                     boundX2, boundY2;        

        public float firstBoundDistance;
        public float firstBoundTime;
        public float baseDeltaTime;//, fpsCalCoff;
        public float hitBallMaxHeight; //타구의 최대 높이

        public int nBoundNum, nBoundNum2;

        //훅과 슬라이스
        public bool bHookorSlice;
        public float angleHookSlice;

        //탑스핀
        public bool bTopSpin;
        public float curGravityAccel;//현재 중력 가속도
        public float curThrowGravityAccel;//현재 중력 가속도

        //펜스
        public bool bFenceOver, bFenceCol, bSideFenceCol, bSideFenceBallDraw;
        float angleOfReflect; //펜스 반사각        
        

        //판정
        public bool bNoFoulCheck;   //true인 경우 파울 체크를 하지 않는다.
        public bool bFairBall;
        public bool bFoulCall;
        public bool bHomeRunCall;
        public bool bPoleCol;
                
        public bool bBallScroll;
        public bool bThrowStart;
        public float throwingApprochTime;

        
        //예상 예측
        public bool bFairBallGuess, bHomeRunGuess, bFenceMeetGuess, bFoulHomerunGuess;			//페어볼인 경우	, 페어일 것이라고 예상됨
        bool bFoulHomerunCheck;


        //이벤트 & 카메라
        public CameraWork cameraWork;
        public bool bBallOutofCamera;
        public float nEventX, nEventY;
        public bool bCameraBallMove;   //카메라가 볼을 따라감        
        public bool bNoEventCamera = false;
        private int focusBase;  //포커스 베이스

        private float curCameraX, curCameraY;
        private float curCameraDX, curCameraDY;
        private float cameraTime;
        private float cameraDV;
        

        private float lastCameraX, lastCameraY;

        public bool bForceDefaultCameraWork;    //강제 디폴트 카메라워크
        

        //기타 
        public bool bRounding; //라운딩여부
        public float throwingTime, throwingDistance;
        int focusRunnerIndex, focusFielderIndex;

        public bool bLaserThrowFlag;


        //볼데드 상태
        public bool bBallDeadState;

        public void initInstance(BallPlayManager main)
        {
            //bLaserThrowFlag = false;

            spr = gameObject.GetComponent<tk2dSprite>();
            //anim = gameObject.GetComponent<tk2dSpriteAnimator>();
            //transform.parent = GameObject.FindWithTag("FIELDINGVIEW_TAG").transform;
            transform.position = new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX, BallPlayManager.FIELDVIEW_CAMERA_INITY, initZOrder);

            bBallActive = false;
            bStunBall = false;
            bForceDefaultCameraWork = false;
            bBallDeadState = false;

            ballTrail = particle.gameObject.GetComponent<TrailRenderer>();

            //cameraWork = CameraWork.Shadow_Chase;

            this.field = main.field;
            //camera = main.camera;
        }

        // Update is called once per frame
        void Update()//FixedUpdate() //  
        {
            if (bBallActive == true)
            {
                //if (field.bInputWait == true) return;
                float deltaTime = field.getDeltaTime();// Time.deltaTime;
                move(deltaTime);
                if (bRotate == true)
                {
                    rotationX += (rotateSpeed * Time.deltaTime);
                    ballObj3.transform.localEulerAngles = new Vector3(rotationX, 0, 0);
                }

            }
        }
        ///////////////////////////////////////////////////////
        //초기화  함수
        ///////////////////////////////////////////////////////
        public void setBallInit()
        {
            ////Debug.Log("====================================================================================>>setBallInit");
            //bHookorSlice = false;
            bNoFoulCheck = false;

            bNoEventCamera = false;
            bCameraBallMove = true;
            bBound = false;
            bBallStop = false;
            bBallStopCheck = false;
            bBallCatched = false;
            bHighDivingCatched = false;
            nBoundNum = 0;
            nBoundNum2 = 0;
            curTime = 0;

            bFenceCol = false;
            bSideFenceCol = false;
            bFoulCall = false;
            bHomeRunCall = false;
            bPoleCol = false;
            bFairBall = false;
            bFenceOver = false;
            bRounding = false;

            bFairBallGuess = false;
            bHomeRunGuess = false;
            bFenceMeetGuess = false;
            bFoulHomerunGuess = false;
            bFoulHomerunCheck = false;

            nBallAY = 0;
            nBallAX = 0;

            nFirstBoundX = 0;
            nFirstBoundY = 0;

            eventStep = BallEvent.EVENT_NOTHING;
            bBallOutofCamera = false;

            bRightBound = bLeftBound = false;

            bRotate = true;

            bLaserThrowFlag = false;
            laserParticle.SetActive(false);
            setHookorSlice(false, 0);

            //for (int i = 0; i < 4; i++) bCrowdActive[i] = false;

            //anim.Play("depth");
        }

        ///////////////////////////////////////////////////////
        //Get 함수
        ///////////////////////////////////////////////////////
        //public int speedDecreaseCount;
        //first bound 좌표를 구한다 - pre simulating
        public void getFirstBound()
        {
            //이함수 안 씀
            float fX, fY, fZ;
            float dx, dy, dz;
            float spd = speed;
            float lastSpd = spd;
            int frame = 0;
            bool bFriction = true;// bAirFriction;
            float nFriction = _AIR_FRICTION_COEFF;
            float ang = angle;
            float lastAng = ang;

            int speedDecreaseCount = 0;
            bHomeRunGuess = false;
            bFoulHomerunGuess = false;
            bFenceMeetGuess = false;
            bFairBallGuess = false;

            fX = field.getOriginX(FieldSize.getHomePosX());
            fY = field.getOriginY(FieldSize.getHomePosY());
            fZ = FieldParm.BALL_INIT_HEIGHT;


            baseDeltaTime = Time.fixedDeltaTime;

            firstBoundTime = baseDeltaTime;

            dx = nBallDX;
            dy = nBallDY;
            dz = nBallDZ;

            bool bHomerunChecked = false;

            float gravityAccel = (bTopSpin ? _GRAVITY_ACCELERATION_TOPSPIN : _GRAVITY_ACCELERATION);

            while (true)
            {
                frame++;
                fX += (dx * baseDeltaTime);
                fY += (dy * baseDeltaTime);

                fZ += (dz);
                dz += (gravityAccel);
                
                firstBoundTime += baseDeltaTime;

                if (bHomerunChecked == false)
                {
                    float sY = field.getScreenY(fY);
                    if (sY >= FieldSize.getFenceOriginY())
                    {
                        //////////UnityEngine.//Debug.Log("====================>>CHECK FENCE");
                        float sX = field.getScreenX(fX);
                        if (fenceEquation(sX, sY))
                        {
                            if (fZ >= FieldSize._FENCE_HEIGHT)
                            {
                                bHomeRunGuess = true;
                            }
                            else
                            {
                                bFenceMeetGuess = true;
                            }
                            bHomerunChecked = true;
                        }
                    }
                }

                if (fZ <= 0)
                {
                    bHomerunChecked = true;
                    nFirstBoundX = fX;
                    nFirstBoundY = fY;


                    screenFirstBoundX = field.getScreenX(nFirstBoundX);
                    screenFirstBoundY = field.getScreenY(nFirstBoundY);

                    field.checkFlyCatchCount();

                    if (checkFoul(screenFirstBoundX, screenFirstBoundY) == true)//if (field.checkFairBallGuess(nAbsBallX, nAbsBallY) == false)
                    {
                        //////////UnityEngine.//Debug.Log("===============>> GUESS FOUL!!!!!!");
                        bFairBallGuess = false;
                        if (bHomeRunGuess == false)
                        {
                            if (poleEquation(screenFirstBoundX, screenFirstBoundY) == true)
                            {
                                //////////UnityEngine.//Debug.Log("===============>> FOUL HOMERUN!!!!!!");
                                bFoulHomerunGuess = true;
                            }
                        }
                    }
                    else
                    {
                        //////////UnityEngine.//Debug.Log("===============>> GUESS FAIR BALL!!!!!!");
                        bFairBallGuess = true;
                        if (bHomerunChecked == false)
                        {
                            /*    if (fenceEquation(screenFirstBoundX, screenFirstBoundY))
                                {
                                    bHomeRunGuess = true;
                                }*/
                            bHomeRunGuess = false;
                            bHomerunChecked = true;
                        }
                    }

                    //////////UnityEngine.//Debug.Log("===============>> speedDecreaseCount = " + speedDecreaseCount);
                    if (bHomeRunGuess == false && bFenceMeetGuess == false && bFairBallGuess == true)
                    {
                        setFirstBound(true);
                    }



                    //////////UnityEngine.//Debug.Log("===============>> speed = " + speed);
                    decreaseSpeed = speed - (speedDecreaseCount * 2) / 3;
                    //////////UnityEngine.//Debug.Log("===============>> decreaseSpeed = " + decreaseSpeed);
                    return;
                }

                if (bFriction)
                {
                    spd -= nFriction;
                    speedDecreaseCount++;
                    //dz -= nFriction;
                    if (spd <= 0)
                    {
                        spd = 0;
                        bFriction = false;
                    }
                }

                if (bHookorSlice == true)
                {
                    ang += angleHookSlice;
                }

                if (spd != lastSpd || ang != lastAng)
                {
                    if (ang < 0) ang = 360 + ang;
                    float rad = ang * Mathf.Deg2Rad;
                    dx = -spd * Mathf.Sin(rad);
                    dy = spd * Mathf.Cos(rad);
                    lastSpd = spd;
                    lastAng = ang;
                }
            }
        }

        public void getFirstBound2() 
        {
            float realBoundTime;
            float a = 0.5f * (bTopSpin ? _GRAVITY_ACCELERATION_TOPSPIN : _GRAVITY_ACCELERATION); 
            float b = nBallDZ;
            float c = FieldParm.BALL_INIT_HEIGHT;

            firstBoundTime = MyMath.getEquation(a, b, c, false);
            realBoundTime = firstBoundTime;
            firstSpeed = speed;

            float limitTime = -firstSpeed / _AIR_FRICTION_COEFF;
            if (realBoundTime > limitTime) realBoundTime = limitTime;
            /*//////UnityEngine.//Debug.Log("=======================>>limitTime = " + (MyMath.getEquation(0.5f * _AIR_FRICTION_COEFF, speed, 0, false) / 2.0f));
            //////UnityEngine.//Debug.Log("=======================>>limitTimeR = " + limitTime);
            //////UnityEngine.//Debug.Log("=======================>>firstBoundTime = " + firstBoundTime);*/
            //가장 좋은 방법
            //이동한 거리
            firstBoundDistance = speed * realBoundTime + 0.5f * _AIR_FRICTION_COEFF * realBoundTime * realBoundTime;

            float highTime = realBoundTime / 2;
            hitBallMaxHeight = (b * highTime + a * (highTime * highTime)) + c;
            ////////UnityEngine.//Debug.Log("=======================>>high = " + hitBallMaxHeight);

            //각계산
            float rad = (angle) * Mathf.Deg2Rad;// -addAngle;
            //첫바운드 계산
            nFirstBoundX = -firstBoundDistance * Mathf.Sin(rad) + field.homeX;
            nFirstBoundY = firstBoundDistance * Mathf.Cos(rad) + field.homeY;

            float hsX = 0;
            float hsY = 0;

            if (bHookorSlice == true)
            {
                ////////UnityEngine.//Debug.Log("========================>>HOOK SLICE!!!!!!!!!!!!!!!!!!!!");
                float time = 0;
                float x = field.homeX;
                float y = field.homeY;
                float spd = speed;
                float ang = angle;// *Mathf.Deg2Rad;
                float angHS = angleHookSlice * 2;// *Mathf.Deg2Rad;
                float dx = nBallDX;
                float dy = nBallDY;

                float baseTime = 0.08f;//

                while (true)
                {
                    if (time >= firstBoundTime)
                    {
                        break;
                    }
                    x += (dx * baseTime);
                    y += (dy * baseTime);
                    spd += (_AIR_FRICTION_COEFF * baseTime);
                    ang += (angHS * baseTime);
                    if (ang < 0) ang = 360 + ang;
                    float radAng = ang * Mathf.Deg2Rad;
                    dx = -spd * Mathf.Sin(radAng);
                    dy = spd * Mathf.Cos(radAng);
                    time += baseTime;
                }
                hsX = (x - nFirstBoundX);
                hsY = (y - nFirstBoundY);
            }

            nFirstBoundX += hsX;
            nFirstBoundY += hsY;

            screenFirstBoundX = field.getScreenX(nFirstBoundX);
            screenFirstBoundY = field.getScreenY(nFirstBoundY);


            bHomeRunGuess = false;
            if (fenceEquation(screenFirstBoundX, screenFirstBoundY, true))
            {
                bHomeRunGuess = true;
                /*   if (fZ >= FieldSize._FENCE_HEIGHT)
                   {
                       bHomeRunGuess = true;
                   }
                   else
                   {
                       bFenceMeetGuess = true;
                   }*/
            }

            field.checkFlyCatchCount();

            if (checkFoul(screenFirstBoundX, screenFirstBoundY) == true)//if (field.checkFairBallGuess(nAbsBallX, nAbsBallY) == false)
            {
                bFairBallGuess = false;
                if (bHomeRunGuess == true)
                {
                    bHomeRunGuess = false;
                    bFoulHomerunGuess = true;
                }
                else
                {
                    if (poleEquation(screenFirstBoundX, screenFirstBoundY) == true)
                    {
                        bFoulHomerunGuess = true;
                    }
                }
            }
            else
            {
                bFairBallGuess = true;
            }

            if (bHomeRunGuess == false && bFenceMeetGuess == false && bFairBallGuess == true)
            {
                setFirstBound(true);
            }

        }

        //사용안함
        public float getThrowAngleZ(float targetX, float targetY)//,bool bToss)
        {
            float fX, fY, fZ;
            float dx, dy, dz;
            float spd = speed;
            float lastSpd = spd;
            int frame = 0;
            int maxFrame;
            bool bFriction = true;// bAirFriction;
            float nFriction = _AIR_FRICTION_COEFF2;
            float ang = angle;
            float lastAng = ang;

            fX = nBallX;
            fY = nBallY;
            fZ = nBallZ;

            //dz = nBallDZ;

            float baseDelta = Time.fixedDeltaTime;

            float boundTime = baseDelta;//baseDeltaTime;

            dx = nBallDX;
            dy = nBallDY;

            while (true)
            {
                frame++;
                fX += (dx * baseDelta);
                fY += (dy * baseDelta);

                boundTime += baseDelta;

                if (Mathf.Abs(fX - targetX) < (40) && Mathf.Abs(fY - targetY) < (40))
                {
                    maxFrame = frame;
                    break;
                }

                if (bFriction)
                {
                    spd -= nFriction;
                    if (spd <= 0)
                    {
                        spd = 0;
                        bFriction = false;
                        maxFrame = frame;
                        break;
                    }
                }


                if (spd != lastSpd || ang != lastAng)
                {
                    if (ang < 0) ang = 360 + ang;
                    float rad = ang * Mathf.Deg2Rad;
                    dx = -spd * Mathf.Sin(rad);
                    dy = spd * Mathf.Cos(rad);
                    lastSpd = spd;
                    lastAng = ang;
                }

                if (frame > 200)
                {
                    maxFrame = 100;
                    break;
                }
            }

            throwingMaxTime = boundTime;
            field.throwRemainTime = throwingMaxTime * 0.3f;
            //////////UnityEngine.//Debug.Log("================>>maxFrame = " + maxFrame);
            //////////UnityEngine.//Debug.Log("================>>boundTime = " + boundTime);
            //////////UnityEngine.//Debug.Log("================>>throwWrist = " + throwWrist);
            dz = (maxFrame / 2) * (_GRAVITY_ACCELERATION_THROW * throwWrist);

            float maxdZ = 250;// (field.nThrowIndex < CPlayer._LEFTFIELDER ? 5 : 5);

            if (dz < maxdZ)
            {
                //////////UnityEngine.//Debug.Log("========================>> SHOW ME THE FUCKING DZ = " + dz+"  ====> and MAX DZ = "+maxdZ);
                dz = maxdZ;
            }
            ////////UnityEngine.//Debug.Log("========================>> RETURN DZ = " + dz);
            //return dz;
            return 400;
        }

        public float getThrowAngleZ2(float targetX, float targetY)//,bool bToss)
        {
            float dz = 0;
            float _time;
            throwingDistance = MyMath.getDistance(nBallX, targetX, nBallY, targetY);
            float gab = (nBallZ - FieldParm.BALL_INIT_HEIGHT_END);
            ////////UnityEngine.//Debug.Log("====================>>> gab = " + gab);

            _time = MyMath.getEquation(0.5f * _AIR_FRICTION_COEFF2, speed, -throwingDistance, true);

            if (_time <= 0 || _time > 10)
            {
                _time = throwingDistance / (speed * 0.9f);
            }
            ////////UnityEngine.//Debug.Log("====================>>> time = " + time);
            dz = -(gab + 0.5f * _GRAVITY_ACCELERATION_THROW * _time * _time) / _time;
            ////////UnityEngine.//Debug.Log("====================>>> dz = " + dz);

            throwingTime = _time;

            if (dz > 400) dz = 400;
            return dz;
        }

        /*
        public float getThrowingTime(float distance, float throwSpeed)
        {
            float _time = MyMath.getEquation(0.5f * _AIR_FRICTION_COEFF2, throwSpeed, -throwingDistance, true);
            if (_time <= 0 || _time > 10)
            {
                _time = throwingDistance / (speed * 0.9f);
            }
            return _time;
        }*/

        public bool checkClosePlay(float baseTIme = 0.65f)
        {
            //////UnityEngine.//Debug.Log("=============>>curTime / throwingTime " + curTime + " / " + throwingTime);
            if ((curTime / throwingTime) > baseTIme) return true;
            else return false;
        }

        ///////////////////////////////////////////////////////
        //Set 함수
        ///////////////////////////////////////////////////////
        //볼을 화면에 나타내거나 사라지게 함
        public void setActive(bool bDraw)
        {
            gameObject.SetActive(bDraw);
            ballObj3.gameObject.SetActive(bDraw);
            if (bDraw == true)
            {
                //spr.scale = new Vector3(0.12f, 0.06f, 1);
                spr.scale = new Vector3(0.15f, 0.075f, 1);
                if (bLaserThrowFlag == false)
                {
                    laserParticle.SetActive(false);
                    particle.SetActive(true);
                }
                else
                {
                    laserParticle.SetActive(true);
                }
            }
        }

        public void setDraw(bool bDraw)
        {
            gameObject.GetComponent<Renderer>().enabled = bDraw;
            ballObj3.gameObject.GetComponent<Renderer>().enabled = bDraw;
        }

        public void setParticleDraw(bool bDraw, float time = 0.1f)
        {
            if (bLaserThrowFlag == false)
            {
                particle.GetComponent<Renderer>().enabled = bDraw;
                if (bDraw == true)
                {
                    ballTrail.time = time;
                }
            }
            else
            {
                laserParticle.SetActive(bDraw);
            }
        }

        public void setBallRotation(float dx, float dy, bool bThrow, bool bReverse = true)
        {
            bRotate = true;

            if (bThrow == true)
            {
                rotateSpeed = 1400;
            }
            else
            {
                rotateSpeed = (bReverse ? -1200 : 1200);
            }

            float initAngle = (Mathf.Atan2(dy, dx) * Mathf.Rad2Deg) + 90;

            ballObj2.transform.localEulerAngles = new Vector3(0, 0, initAngle);
        }

        //볼을 정지 상태로 세팅
        public void setBallStop()
        {
            //anim.Stop();
            nBallDX = 0;
            nBallDY = 0;
            bBallStop = true;
            speed = 0;
            bBallScroll = false;
            bRotate = false;

        }

        //볼의 상태를 와일드 피치 상태로 만듬
        public void setWildPitch(bool bBlock)
        {
            setBallInit();
            setActive(true);
            setDraw(true);
            setParticleDraw(false);
            bBallStop = false;
            bBallStopCheck = true;
            bNoFoulCheck = true;
            step = BallStep.BALL_WILD_PITCH;

            if (bBlock == false)
            {
                //블록 실패
                speed = _BALLSPEED_COEF * Random.Range(7, 10); //pvp랜덤체크
                angle = Random.Range(160.0f, 200.0f);          //pvp랜덤체크
                angleZ = 10;
            }
            else
            {
                //블록 성공
                speed = _BALLSPEED_COEF * Random.Range(3.0f, 4.0f); //pvp랜덤체크
                angle = Random.Range(-15.0f, 15.0f);                //pvp랜덤체크
                angleZ = 30;
            }
            
            setVelocity();
            setFielderFocus(CPlayer._CATCHER);
            nBallX = field.fielder[CPlayer._CATCHER].posX;
            nBallY = field.fielder[CPlayer._CATCHER].posY;
            nBallZ = 20;
            nBallDZ = -100;
            curGravityAccel = _GRAVITY_ACCELERATION;
            checkScroll();
            gameObject.transform.localPosition = new Vector3(screenX, screenY, 0);
        }


        //볼의 상태를 BALL_THROW상태로
        public void setBallThrow()
        {
            for (int i = 0; i < 4; i++) field.run.bBallOnBase[i] = false;
            //bFenceOver = false;
            bFenceCol = false;
            bSideFenceCol = false;
            bBallStop = false;
            step = BallStep.BALL_THROW;
            //anim.Play("depth");
            setBallRotation(nBallDX, nBallDY, true);

            bRightBound = bLeftBound = false;

        }

        //볼의 상태를 BALL_CATCH상태로
        public void setBallCatched(int myIndex, float posX, float posY)
        {
            fieldSkillDisplayManager.EffectDisplay(fieldSkillDisplayManager.FieldDisplayStep.Catch);

            if (myIndex > -1)
            {
                int teamIndex = (field.manager.bTopInning ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex);
                IngameUI.GetFieldUI().SetName(true, field.fielder[myIndex].pFielder, teamIndex, field.manager.bMyTurn);
            }
            //if (field.bOnceWildThrow == true) field.bOnceWildThrow = false;
            //setCenterCamera(field.getScreenX(posX), field.getScreenY(posY));
            ////////UnityEngine.//Debug.Log("======================>setBallCatched");
            if (bBound == false)
            {
                //////////UnityEngine.//Debug.Log("======================>>bFairBall = TRUE");
                bFairBall = true;
            }
            step = BallStep.BALL_CATCH;
            field.bThrowing = false;
            bBallCatched = true;
            setBallStop();
            //setActive(false);
            setDraw(false);
            setFirstBound(false);

            field.setCatched(myIndex);
            field.nThrowIndex = field.nCarrierIndex = myIndex;
            if (field.nFirstThrowIndex == -1)
            {
                field.nFirstThrowIndex = myIndex;
            }
            field.returnCheck(-2);

            nBallX = posX;
            nBallY = posY;
            //curCameraX = field.getScreenX(posX);
            //curCameraY = field.getScreenY(posY);
            bRotate = false;
        }

        public void setThrowBallCatched(int myIndex, bool bOnBase)
        {
            if (myIndex > -1)
            {
                int teamIndex = (field.manager.bTopInning ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex);
                IngameUI.GetFieldUI().SetName(true, field.fielder[myIndex].pFielder, teamIndex, field.manager.bMyTurn);
            }
            //////////UnityEngine.//Debug.Log("======================>setThrowBallCatched");
            field.bThrowing = false;
            field.bRelaying = false;
            field.bThrowBallCatched = true;

            if (bOnBase == true)
            {
                if (field.nTargetIndex >= FieldParm.FIRSTBASE_INDEX && field.nTargetIndex <= FieldParm.HOMEBASE_INDEX)
                {
                    //중요!!!!!!!!!!!!!!!
                    //태그인 경우 이 플래그를 태그 딜레이에 따라 변경해주어야 함 throwingCatchFrame()에서 처리
                    field.run.bBallOnBase[field.nTargetIndex] = true;

                }
            }
            //ballObj.transform.localPosition = new Vector3(0, 3.5f + nScreenBallZ, 0.1f);
            step = BallStep.BALL_THROW_CATCH;// BALL_CATCH;
            setBallStop();
            //setActive(false);
            setDraw(false);
            setParticleDraw(false);
            

            field.nThrowIndex = field.nCarrierIndex = myIndex;

            /*
            if (myIndex != -1)
            {
                setCenterCamera(field.getScreenX(field.fielder[myIndex].posX), field.getScreenY(field.fielder[myIndex].posY));
            }*/

            bRotate = false;

            /*
            if (myIndex != -1)
            {
                curCameraX = field.getScreenX(field.fielder[myIndex].posX);
                curCameraY = field.getScreenY(field.fielder[myIndex].posY);
            }*/
            /*
            if (field.fielder[myIndex].setRecheckThrow2() == false)
            {
                field.returnCheck(-2);
            }*/
        }

        public void setThrowBallCatchedTagReady(int myIndex)
        {
            //////////UnityEngine.//Debug.Log("======================>setThrowBallCatched");
            field.bThrowing = false;
            field.bRelaying = false;

            /*   if (bOnBase == true)
               {
                   if (field.nTargetIndex >= FieldParm.FIRSTBASE_INDEX && field.nTargetIndex <= FieldParm.HOMEBASE_INDEX)
                   {
                       //중요!!!!!!!!!!!!!!!
                       //태그인 경우 이 플래그를 태그 딜레이에 따라 변경해주어야 함 throwingCatchFrame()에서 처리
                       field.run.bBallOnBase[field.nTargetIndex] = true;
                   }
               }*/
            step = BallStep.BALL_THROW_CATCH;// BALL_CATCH;
            setBallStop();
            //setActive(false);
            setDraw(false);
            

            field.nThrowIndex = field.nCarrierIndex = myIndex;
        }

        //러너 포커스 상태로 만든다
        public void setRunnerFocus(int index)
        {
            if (bNoEventCamera == false)
            {
                bCameraBallMove = false;
                eventStep = BallEvent.EVENT_RUNNER_FOCUS;
                focusRunnerIndex = index;
                setParticleDraw(false);
            }
        }

        public void setFielderFocus(int index)
        {
            if (bNoEventCamera == false)
            {
                bCameraBallMove = false;
                eventStep = BallEvent.EVENT_FIELDER_FOCUS;
                focusFielderIndex = index;
                //setParticleDraw(false);
            }
        }

        public void setFielderFocus2(int index, float duration)
        {
            float posX = field.getScreenX(field.fielder[index].posX);
            float posY = field.getScreenY(field.fielder[index].posY);
            CameraManager.SetPositionTo(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + posX, BallPlayManager.FIELDVIEW_CAMERA_INITY + posY, -200), duration);
        }

        public void setBaseFocus(int index)
        {
            if (bNoEventCamera == false)
            {
                ////UnityEngine.Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&이게문제??");
                bCameraBallMove = false;
                eventStep = BallEvent.EVENT_BASE_FOCUS;
                focusBase = index;
                setParticleDraw(false);
            }
        }

        public void setJudgeFocus(int index)
        {
            if (bNoEventCamera == false)
            {
                bCameraBallMove = false;
                eventStep = BallEvent.EVENT_JUDGE_FOCUS;
                focusFielderIndex = index;
            }
        }


        float eventCurX, eventCurY;
        float eventDX, eventDY;
        float eventMoveTime, eventRemainTime;
        BallEvent nextStep;
        int nextIndex;

        public void setFocusMove(float sx, float sy, float dx, float dy, BallEvent nStep, int nIndex, float remaintime = 0.5f)
        {
            if (bNoEventCamera == false)
            {
                eventRemainTime = remaintime;
                bCameraBallMove = false;
                eventCurX = sx;
                eventCurY = sy;
                eventDX = (dx - sx) / eventRemainTime;
                eventDY = (dy - sy) / eventRemainTime;
                eventStep = BallEvent.EVENT_FOCUS_MOVE;
                eventMoveTime = 0;
                nextStep = nStep;
                nextIndex = nIndex;
            }
        }


        //볼의 훅 혹은 슬라이스 상태를 세팅
        public void setHookorSlice(bool bAvail, float angle)
        {
            //////UnityEngine.//Debug.Log("====================================================================>>setHookorSlice angle = " + angle);
            bHookorSlice = bAvail;
            angleHookSlice = angle;
        }

        public void setTopSpin()
        {
            bTopSpin = MyMath.Percent() < 30 ? true : false;
            ////Debug.Log("====================================================================>>TopSpin = " + bTopSpin);
            curGravityAccel = bTopSpin ? _GRAVITY_ACCELERATION_TOPSPIN : _GRAVITY_ACCELERATION;
        }

        public void setNormalGraivty()
        {
            bTopSpin = false;
            curGravityAccel = _GRAVITY_ACCELERATION;
        }



        //볼의 속력과 방향을 통해 x,y 벡터의 속도값을 구해온다.
        public void setVelocity()
        {
            if (angle < 0) angle = 360 + angle;
            float rad = angle * Mathf.Deg2Rad;

            //float newSpeed = speed * 50;

            nBallDX = -speed * Mathf.Sin(rad);
            nBallDY = speed * Mathf.Cos(rad);
            nLastSpeed = speed;
            nLastAngle = angle;

        }

        private float getGravityAccelChangeAfterBound(float angle)
        {
            if (angle < -50)
            {
                return _GRAVITY_ACCELERATION_50;
            }
            else if (angle < -35)
            {
                return _GRAVITY_ACCELERATION_40;
            }
            else if (angle < -20)
            {
                return _GRAVITY_ACCELERATION_30;
            }

            return _GRAVITY_ACCELERATION;
        }


        //볼의 각도를 통해 z 벡터의 속도값을 구해온다.
        public void setVelocityZ(float power)
        {
            nBallDZ = power * Mathf.Sin(angleZ * Mathf.Deg2Rad) / FBall.NORMAL_DELTATIME;
            nScreenBallZ = nBallZ * _Z_AXIS_PROJECTION_COEFF;            
        }

        //타구의 바운드 - 바운드시 세팅되는 값을 설정
        private void setBound()
        {
            ////////UnityEngine.//Debug.Log("========================>>SET BOUND");
            if (bBound == false)
            {
                //Debug.Log("curTime = " + curTime + " ======>> firstBoundTime = " + firstBoundTime);
                field.firstBound.SetActive(false);
                if (cameraWork != CameraWork.Default)
                {
                    cameraWork = CameraWork.Shadow_Chase;
                }
                //////UnityEngine.//Debug.Log("==============================================>> bOutofInfield = " + field.bOutofInfield);
                //////UnityEngine.//Debug.Log("==============================================>> earlygrounder, grounder = " + field.earlygrounder + ", " + field.grounder);
                if (field.bOutofInfield == true)
                {
                    if (field.earlygrounder == false && field.grounder == false)
                    {
                        field.run.runnerFullAccell(FieldParm.FIRSTBASE_INDEX);
                    }
                }

                if (field.bCollisionFlag == true)
                {
                    field.bCollisionFlag = false;
                }

                field.setHitStringType();

                setParticleDraw(false);
                bBound = true;

                //첫바운드시 속도 감속
                speed = speed * _BOUND_SPEED_RATE;
                //첫바운드 튀고 상황에 따른 중력가속도 세팅
                curGravityAccel = getGravityAccelChangeAfterBound(firstAngleZ);

                bBallScroll = false;
                field.run.setRunnerMoveAfterBound();   //타구시 바운드됨에 따른 주자들의 움직임

                if (bFairBallGuess == true)
                {
                    bFairBall = true; //guess는 확신으로 바꿈
                }

                if (bHomeRunGuess == true)
                {
                    cameraWork = CameraWork.None;
                    bFairBall = true;
                    bHomeRunCall = true;
                    speed = speed * 0.5f;
                    setDraw(false);
                    nBoundNum = 400;
                }

                if (field.run.bRunnerFoul == false && field.bOnceWildThrow == false && bFenceCol == false)
                {
                    state = BallState.BALL_FAIR;
                    if (bHomeRunGuess == false)
                    {
                        //파울체크
                        if (checkFoul(screenX, screenY) == true)//if (field.checkFairBallGuess(nAbsBallX, nAbsBallY) == false)
                        {
                            //setDraw(false);
                            field.setFoulCall();
                            nBoundNum = 400;
                        }
                        else
                        {
                            if (field.judge.bFairCheck == true)
                            {
                                field.judge.setCall(0, CallType._LINECALL);
                            }
                            bFairBall = true;
                        }
                    }
                }
            }
           
            
            if (nBoundNum < 4 && bFenceCol == false)
            {
                makeBoundEffect(transform.position);
            }
            field.checkCatcherFielding(speed);

            if (bBallStop == true)
            {
                nBallZ = 0;
                nScreenBallZ = 0;
                nBallDZ = 0;
                if (bHomeRunCall == true)
                {
                    //홈런 콜이 안낫는데.. 펜스는 넘어가고 페어인경우 다시 홈런 처리를 여기서 해준다.                     
                    field.setHomerunCall();
                }
                return;
            }
            else
            {
                if (nBoundNum == 401)
                {
                    if (bHomeRunCall == true)
                    {
                        //홈런 콜이 안낫는데.. 펜스는 넘어가고 페어인경우 다시 홈런 처리를 여기서 해준다.                     
                        field.setHomerunCall();
                        //홈런 사운드
                        soundmanager.Get().PlaySound(soundmanager.SoundID.HomerunCall);
                    }
                    
                }
            }
            

            nBallZ = 0;
            nBallDZ = -nBallDZ / 2;
            nBoundNum++;
            //불규칙 바운드 생성 - 옵션
        } //bBigChop

        public void setPoleCollision()
        {
            if (bBound == true) return;
            if (step == BallStep.BALL_HIT)
            {
                if (bFoulHomerunGuess == false)//if (field.pitcher.bball.bBigHomerun == false && bFoulHomerunGuess == false)
                {
                    if (bPoleCol == false)
                    {
                        step = BallStep.BALL_POLE_HIT;
                        bPoleCol = true;
                        bFairBall = true;
                        speed = speed / 7.0f;
                        angle = angle + (firstAngle > 0 ? Random.Range(160, 220) : Random.Range(140, 195)); 
                        setVelocity();
                        nBallDZ = nBallDZ / 5.0f;
                        setParticleDraw(false);
                        for (int i = 0; i < 9; i++)
                        {
                            field.fielder[i].setStop();
                        }
                    }
                }
            }
        }

        //송구의 바운드
        void setThrowingBound()
        {
            if (bBound == false)
            {
                cameraWork = CameraWork.Shadow_Chase;
                setParticleDraw(false);
                bBound = true;
                
                ////////UnityEngine.//Debug.Log("========================>>송구 첫바운드 처리");
                if (field.nThrowIndex < CPlayer._LEFTFIELDER)
                {
                    speed = speed * 0.85f;
                }
                else
                {
                    speed = speed * 0.7f;
                }
                ////////UnityEngine.//Debug.Log("========================>>송구 첫바운드 처리 끝");
            }

            nBoundNum2++;
            if (nBoundNum2 < 2)
            {
                makeBoundEffect(transform.position);
            }

            /*
            if (bCameraBallMove == true)
            {
                if (field.bOnceWildThrow == false)
                {
                    if (nBoundNum2 == 20)
                    {
                        StartCoroutine(field.updateFieldScene(0.1f, true));
                    }
                }
            }*/

            if (bBallStop == true)
            {
                if (bBallStopCheck == false)
                {
                    if (field.bOnceWildThrow == false || field.errorType == FieldParm.ErrorType.WildWrongPlace)
                    {
                        StartCoroutine(field.updateFieldScene(3.0f, true));
                        int closeIndex = field.getCloseFielderIndexError(-1, false, nBallX, nBallY);
                        field.fielder[closeIndex].setBallChase();
                        bBallStopCheck = true;
                    }
                }
                nBallZ = 0;
                nScreenBallZ = 0;
                nBallDZ = 0;
                return;
            }

            nBallZ = 0;
            nBallDZ = -(nBallDZ / 3.0f);

            if (speed <= 0)
            {
                setBallStop();
            }
        }

        //외야 펜스 충돌시 값 조정
        void setFenceCol()
        {
            float limit = 3 * _BALLSPEED_COEF;
            speed = speed / 5;
            if (speed > limit)
            {
                speed = limit;
            } angle = angleOfReflect;
            bFenceCol = true;

            if (bBound == false)
            {
                //충격파
                //CameraManager.FieldShockWave(transform.position, 0.5f, 10, 0.05f);
                state = BallState.BALL_FAIR;
                //파울체크
                if (checkFoul(screenX, screenY) == true)//if (field.checkFairBallGuess(nAbsBallX, nAbsBallY) == false)
                {
                    field.setFoulCall();
                }
            }

            field.setCatchErrorInit();  //에러플래그 초기화 해줌- 여기서 에러하면 바보같음
            //field.setActiveBattingFence();

            //중계 재 조정
            int relay = (firstAngle>0?CPlayer._SHORTSTOP:CPlayer._SECONDBASEMAN);
            field.fielder[relay].setRelayPosition(FieldParm.HOMEBASE_INDEX, nBallX, nBallY);

        }

        /*
        public float _radius = 0.5f;
        public float _speed = 1;
        public float _amp = 0.1f;*/


        //사이드 펜스 충돌시 값 조정
        public void setSideFenceCol()
        {
            if (bSideFenceCol == false)
            {
                if (field.bFieldPickOffFlag == true || field.bThrowing == true)
                {
                    //공을 던진 경우
                    speed = speed / 4.0f;
                    angle = angle + 180;
                    setVelocity();
                }
                else
                {
                    //공을친경우
                    speed = speed / 4.0f;
                    //float curAngle = Mathf.Atan2(nBallDY, nBallDX);
                    if (firstAngle > 0)
                    {
                        angle = firstAngle - 36;
                    }
                    else
                    {
                        angle = firstAngle + 36;
                    }
                    setVelocity();
                }
                bSideFenceBallDraw = true;
                nBoundNum = 100;
                bSideFenceCol = true;
            }
        }

        public void setCameraPosInit(float sX, float sY)
        {
            curCameraX = sX;
            curCameraY = sY;

            curCameraDX = 0;
            curCameraDY = 0;

            cameraDV = 4.5f;
            //cameraStep = 0;

            cameraWork = CameraWork.Default;
            
        }

        public void setCameraPosInitAfterHit()
        {
            //Debug.Log("setCameraPosInitAfterHit");
            curCameraDX = 0;
            curCameraDY = 0;
            cameraDV = 4.5f;

            screenX = field.getScreenX(nBallX);
            screenY = field.getScreenY(nBallY);

            gameObject.transform.localPosition = new Vector3(screenX, screenY, -3.99999f + (nBallY * 0.0002f));
            ballObj.transform.localPosition = new Vector3(0, 3.5f + nScreenBallZ, 0.1f);

#if _OrthoCamera
            bool bFielderFocusView = (MyMath.Percent() < 65 ? true : false);

            if(bFielderFocusView == true)
            {
                if ((field.bGrounderAvailble == true && field.groundCatchFielder > CPlayer._CATCHER) ||
                    (field.flyCatchAvaiableCount > 0 && firstAngleZ > 35 && field.flyCatchFielder > CPlayer._PITCHER))
                {
                    cameraWork = CameraWork.Default;

                    if (field.flyCatchAvaiableCount > 0)
                    {
                        setFielderFocus(field.flyCatchFielder);                    
                        field.setZoom(0.45f);
                        field.setZoomTo(1.0f, firstBoundTime); 
                    }
                    else
                    {
                        setFielderFocus(field.groundCatchFielder);
                        field.setZoom(0.5f);
                        field.setZoomTo(2.0f, 1.5f);   
                    }
   
                    curCameraX = nEventX;
                    curCameraY = nEventY;
                    CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + curCameraX, BallPlayManager.FIELDVIEW_CAMERA_INITY + curCameraY, -200));
                }
                else
                {
                    bFielderFocusView = false;
                }
            }

            if(bFielderFocusView == false)
            {
                if (field.batter.bHitGood == false || Mathf.Abs(firstAngle) > 25)
                {
                    cameraWork = CameraWork.Default;
                    curCameraX = screenX;
                    curCameraY = screenY;
                }
                else
                {
                    cameraWork = CameraWork.Ball_Chase;
                    curCameraX = screenX;
                    curCameraY = screenY + nScreenBallZ;
                }                
                CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + curCameraX, BallPlayManager.FIELDVIEW_CAMERA_INITY + curCameraY, -200));
            }

#else
            cameraWork = CameraWork.Default;
            curCameraX = screenX;
            curCameraY = screenY;
            CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + curCameraX, BallPlayManager.FIELDVIEW_CAMERA_INITY + curCameraY, -200));
            
#endif

            if (bBound == false)
            {
                setParticleDraw(true);
            }




        }

        ///////////////////////////////////////////////////////
        //Check 함수
        ///////////////////////////////////////////////////////
        

        //펜스를 체크한다.
        public void checkFence()
        {
            if (bHomeRunCall == true || bFenceOver == true) return;

            if (bFenceCol == false)
            {
                if (checkOutFenceCol())
                {
                    setFenceCol();
                }
            }
            /*
            if (bSideFenceCol == false)
            {
                if (checkSideFenceCol())
                {
                    //////////UnityEngine.//Debug.Log("==================>>side fence col");
                    setSideFenceCol();
                }
            }*/
        }

        //외야펜스와의 충돌을 체크한다.
        bool checkOutFenceCol()
        {
            if (bHomeRunGuess == false && bFoulHomerunGuess == false)
            {
                if (screenY >= FieldSize.getFenceOriginY())//field.fRatio))
                {
                    float nX = screenX;// nBallX;
                    float nY = screenY;// nBallY;

                    if (fenceEquation(nX, nY))
                    {
                        //if ((Mathf.Abs(nBallZ) > FieldSize._FENCE_HEIGHT) || bHomeRunGuess == true)
                        if (bHomeRunGuess == true)
                        {
                            if (bHomeRunCall == false && bFairBallGuess == true)
                            {
                                //ballObj.gameObject.renderer.enabled = false; 
                                //ballObj.gameObject.GetComponent<tk2dSprite>().enabled = false;//그림자 지움
                                bFenceOver = true;
                                bHomeRunCall = true;
                            }
                        }
                        else
                        {
                            angleOfReflect = angle + 180;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //사이드펜스와의 충돌을 체크한다.
        bool checkSideFenceCol()
        {
            //이거 완전 다시 만들 필요 있음
            float slope;
            //float ratio = field.fRatio;

            if (bBound || (Mathf.Abs(nScreenBallZ) < FieldSize._FENCE_HEIGHT))
            {
                if (angle > 40 && angle < 90)
                {
                    if (bFairBallGuess == false && screenY > FieldSize.FENCE_LEFT_POLE_Y)//ratio))
                    {
                        angle += 180;
                        return true;
                    }
                    else
                    {
                        slope = ((float)(FieldSize.getSideFenceY2() - screenY) / (float)(FieldSize.getLeftSideFenceX2() - screenX));
                        if (slope < 0)
                        {
                            if (slope >= FieldSize.getLeftFenceSlope())
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (angle < 320 && angle > 270)
                {
                    if (bFairBallGuess == false && screenY > FieldSize.FENCE_LEFT_POLE_Y)
                    {
                        angle += 180;
                        return true;
                    }
                    else
                    {
                        slope = ((float)(FieldSize.getSideFenceY2() - screenY) / (float)(FieldSize.getRightSideFenceX2() - screenX));
                        if (slope > 0)
                        {
                            if (slope <= FieldSize.getRightFenceSlope())
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        //파울을 체크한다.
        public bool checkFoul(float x, float y)
        {
            if (bNoFoulCheck == true) return false;

            if (firstAngle >= 45 || firstAngle <= -45)
            {
                //Debug.Log("CASE 1 ==> x:" + x + "     y" + y + " /// firstAngle: " + firstAngle);
                return true;
            }

            if (angle < 0 || angle > 180)
            {
                if (foulEquation(x, y, FieldSize.getHomePosX(), FieldSize.getHomePosY(), FieldSize.FENCE_RIGHT_POLE_X, FieldSize.FENCE_RIGHT_POLE_Y))
                {
                    //Debug.Log("CASE 2 ==> x:" + x + "      y" + y + " /// angle: " + angle);
                    return true;
                }
            }
            else
            {
                if (foulEquation(x, y, FieldSize.getHomePosX(), FieldSize.getHomePosY(), FieldSize.FENCE_LEFT_POLE_X, FieldSize.FENCE_LEFT_POLE_Y))
                {
                    //Debug.Log("CASE 3 ==> x:" + x + "        y" + y + " /// angle: " + angle);
                    return true;
                }
            }
            return false;
        }


        //현재 지정된 베이스로 던져지는 지 여부 체크
        public bool checkThrowingToThisBase(int baseIndex)
        {
            if (step == BallStep.BALL_THROW && field.nTargetIndex == baseIndex)
            {
                return true;
            }
            return false;
        }

        ///////////////////////////////////////////////////////
        //Update 함수
        ///////////////////////////////////////////////////////
        private void getBallDepth()
        {
            //이전
            nBallDepth = 8.0f + Mathf.Clamp(nBallZ * 0.01f, 0.0f, 20.0f);

            //옛날
            //nBallDepth = nBallZ * 0.0525f; //nBallZ * 0.0825f;
            //if (nBallDepth < 13.2f) nBallDepth = 13.2f;
            //else if (nBallDepth > 40) nBallDepth = 40;//

            ballTrail.startWidth = nBallDepth;
        }

        private float getBallZPos()
        {
            if (step == BallStep.BALL_HIT)
            {
                if (bBound == false)
                {
                    return bHomeRunGuess ? -150 : -30;
                }                
            }
            
            return 0;
        }

        void recheckFoulHomerun()
        {
            if (bFoulHomerunGuess == true)
            {
                if (bFoulHomerunCheck == false)
                {
                    if (screenY >= FieldSize.FENCE_LEFT_POLE_Y)
                    {
                        bool bHomeRun = false;
                        if (firstAngle > 0)
                        {
                            if (screenX > FieldSize.FENCE_LEFT_POLE_X)
                            {
                                bHomeRun = true;
                            }
                        }
                        else
                        {
                            if (screenX < FieldSize.FENCE_RIGHT_POLE_X)
                            {
                                bHomeRun = true;
                            }
                        }

                        if (bHomeRun == true)
                        {
                            //////UnityEngine.//Debug.Log("====================>> 다시 체크 해보니 파울홈런 아닐세");
                            bHomeRunGuess = true;
                            bFoulHomerunGuess = false;
                            bFoulHomerunCheck = true;
                            field.judge.setCall(0, CallType._HOMERUN);
                        }
                    }
                }
            }
        }

        private float getBallZRatio()
        {
            if (bBound == false) return 1;
            else
            {
                float ratio = 1 - screenY * 0.0004f;
                if (ratio < 0.3f) ratio = 0.3f;

                return ratio;
            }
        }

        void ballMove(float deltaTime)
        {
            field.checkFieldRatio(nBallY - field.baseHomePosY);//, true);


            nBallX += (nBallDX * deltaTime);
            nBallY += (nBallDY * deltaTime);

            screenX = field.getScreenX(nBallX);
            screenY = field.getScreenY(nBallY);

            nBallZ += (nBallDZ * deltaTime);
            nBallDZ += (curGravityAccel * deltaTime);
            nScreenBallZ = nBallZ * _Z_AXIS_PROJECTION_COEFF * getBallZRatio();

            recheckFoulHomerun();

            //getBallDepth();
        }

        void ballMoveThrow(float deltaTime)
        {

            field.checkFieldRatio(nBallY - field.baseHomePosY);//,false);

            nBallX += (nBallDX * deltaTime);
            nBallY += (nBallDY * deltaTime);


            screenX = field.getScreenX(nBallX);
            screenY = field.getScreenY(nBallY);

            nBallZ += (nBallDZ * deltaTime);
            //nBallDZ += (_GRAVITY_ACCELERATION_THROW * deltaTime); //* throwWrist
            nBallDZ += (curThrowGravityAccel * deltaTime);
            nScreenBallZ = nBallZ * _Z_AXIS_PROJECTION_COEFF2;// *(field.fRatio / FieldParm.InitRatio);

            //getBallDepth();

        }

        void ballScroll(bool bThrow)
        {
            if (speed <= 0)
            {
                speed = 0;
            }
            else
            {
                if (bThrow == true)
                {
                    nScreenX = screenX;
                    nScreenY = screenY;
                }
                else
                {
                    nScreenX = screenX;
                    nScreenY = screenY;
                }
            }
        }

        //타구 움직임
        private void ballHitMove(float deltaTime)
        {
            ballMove(deltaTime);

            firstBoundUpdate();

            if (nBallZ <= 0)
            {
                nBallZ = 0;
                nScreenBallZ = 0;
                setBound();
            }

            if (bBound && bFenceOver)
            {
                if (nBoundNum > 20)
                {
                    setBallStop();
                }
            }

            if (bBallStop == false)
            {
                checkFence();
                if (bBound == false)//bAirFriction)
                {
                    speed += (_AIR_FRICTION_COEFF * deltaTime);
                    if (bHookorSlice == true)
                    {
                        angle += (angleHookSlice * 2 * deltaTime);
                    }
                    //바람 처리도 여기서 해줘 만약 옵션에 들어가면....
                }
                else
                {
                    speed += (_BOUND_FRICTION_COEFF * deltaTime);
                    if (speed <= 0)//&& nBoundNum>5)
                    {
                        setBallStop();
                        if (bBallCatched == false)
                        {
                            field.checkNearFielderActive();
                        }
                    }
                }

                if (speed != nLastSpeed || angle != nLastAngle)
                {
                    setVelocity();
                }
                ballScroll(false);

            }
        }

        //송구 움직임
        private void ballThrowMove(float deltaTime)
        {
            if (bBallHidden == true)
            {
                if (curTime > 0.05f)
                {
                    setDraw(true);
                    bBallHidden = false;
                }
            }

            ballMoveThrow(deltaTime);
            if (nBallZ <= 0)
            {
                nBallZ = 0;
                nScreenBallZ = 0;
                setThrowingBound();
            }
            if (bBallStop == false)
            {
                //checkFence();
                if (bBound == false)//bAirFriction)
                {
                    speed += (_AIR_FRICTION_COEFF2 * deltaTime); 
                }
                else
                {
                    checkFence();
                    speed += _BOUND_SPEED_DECREASE;
                    if (speed < 0)
                    {
                        setBallStop();
                    }
                }

                if (speed != nLastSpeed || angle != nLastAngle)
                {
                    setVelocity();
                }


                if (bRounding == false)
                {
                    field.throwFrame++;
                    if (field.throwFrame > 10) //나중에 토털 시간을 계산에서 dx, dy를 구하는 방법으로 바꾸자
                    {
                        ballScroll(true);
                    }
                    else
                    {
                        screenDX = 2 * (screenX - nScreenX) / 10.0f;
                        screenDY = 2 * (screenY - nScreenY) / 10.0f;
                    }
                }
            }
        }

        //와일드 피치시 볼 움직임
        private void ballWildPitchMove(float deltaTime)
        {
            ballMove(deltaTime);

            if (nBallZ <= 0)
            {
                nBallZ = 0;
                nScreenBallZ = 0;
                if (bBound == false)
                {
                    cameraWork = CameraWork.Shadow_Chase;
                    setParticleDraw(false);                    
                    bFairBall = true; //guess는 확신으로 바꿈
                    state = BallState.BALL_FAIR;
                    speed = speed * 0.9f;
                    bBound = true;
                }
                /*
                if (nBoundNum < 4)
                {
                    makeBoundEffect(transform.position);
                }*/
 
                if (bBallStop == true)
                {
                    nBallZ = 0;
                    nScreenBallZ = 0;
                    nBallDZ = 0;
                    return;
                }
                nBallDZ = -(nBallDZ / 3.0f);     
                nBoundNum++;
            }

            if (bBound && bFenceOver)
            {
                if (nBoundNum > 20)
                {
                    setBallStop();
                }
            }

            if (bBallStop == false)
            {
                checkFence();                
                if (speed != nLastSpeed || angle != nLastAngle)
                {
                    setVelocity();
                }
                ballScroll(false);
            }
        }

        //폴대 강타
        private void ballPoleHit(float deltaTime)
        {
            if (nBoundNum > 10)
            {
                step = BallStep.None;
                return;
            }

            nBallX += (nBallDX * deltaTime);
            nBallY += (nBallDY * deltaTime);
            screenX = field.getScreenX(nBallX);
            screenY = field.getScreenY(nBallY);
            nBallZ += (nBallDZ * deltaTime);
            nBallDZ += (curGravityAccel * deltaTime);
            nScreenBallZ = nBallZ * _Z_AXIS_PROJECTION_COEFF * getBallZRatio();

            if (nBallZ <= 0)
            {
                if (bBound == false)
                {
                    setParticleDraw(false);
                    bBound = true;
                    if (bPoleCol == true)
                    {
                        bFairBall = true; //guess는 확신으로 바꿈
                        bHomeRunCall = true;
                    }
                }
                 
                ////////UnityEngine.//Debug.Log("=================>>>nBallDZ = " + nBallDZ);
                nBallDZ = -nBallDZ * 0.5f;
                nBallZ = 0;
                nScreenBallZ = 0;
                speed = speed * 0.2f;
                nBoundNum++;

                setVelocity();

                if (nBoundNum == 10)
                {
                    setBallStop();
                    if (bHomeRunCall == true)
                    {
                        //홈런 콜이 안낫는데.. 펜스는 넘어가고 페어인경우 다시 홈런 처리를 여기서 해준다.                     
                        field.setHomerunCall(bPoleCol);
                    }
                    //홈런 사운드
                    soundmanager.Get().PlaySound(soundmanager.SoundID.HomerunCall);
                }
            }
        }

        //운반 움직임
        void ballCarryMove()
        {            
            nBallX = field.fielder[field.nCarrierIndex].posX;
            nBallY = field.fielder[field.nCarrierIndex].posY;
            checkScroll();
            //setBoundary(20, 57);
        }

        void ballCatchMove()
        {
            if (field.nCatchIndex >= 0)
            {
                nBallX = field.fielder[field.nCatchIndex].posX;
                nBallY = field.fielder[field.nCatchIndex].posY;
                checkScroll();
                //setBoundary(20, 57);
            }
        }

        void eventRunnerFocus()
        {
            float xPos = field.run.runner[focusRunnerIndex].posX;
            float yPos = field.run.runner[focusRunnerIndex].posY;
            checkScroll(xPos, yPos);
        }

        void eventFielderFocus()
        {
            float xPos = field.fielder[focusFielderIndex].posX;
            float yPos = field.fielder[focusFielderIndex].posY;
            checkScroll(xPos, yPos);
        }

        void eventJudgeFocus()
        {
            float xPos = field.judge.judge[focusFielderIndex].posX;
            float yPos = field.judge.judge[focusFielderIndex].posY;
            checkScroll(xPos, yPos);
        }
        void eventBaseFocus()
        {
            float xPos, yPos;
            if (focusBase == -1)
            {
                xPos = field.getOriginX(FieldSize.getMoundPosX());
                yPos = field.getOriginY(FieldSize.getMoundPosY());
            }
            else
            {
                xPos = field.getOriginX(FieldSize.getBasePosX(focusBase));
                yPos = field.getOriginY(FieldSize.getBasePosY(focusBase));
            }
            checkScroll(xPos, yPos);
        }

        void eventFoucsMove()
        {
            eventMoveTime += Time.deltaTime; 

            eventCurX += (eventDX * Time.deltaTime); 
            eventCurY += (eventDY * Time.deltaTime); 

            checkScroll(eventCurX, eventCurY);

            if (eventMoveTime > eventRemainTime)
            {
                if (nextStep == BallEvent.EVENT_RUNNER_FOCUS)
                {
                    setRunnerFocus(nextIndex);
                }
                else if (nextStep == BallEvent.EVENT_FIELDER_FOCUS)
                {
                    setFielderFocus(nextIndex);
                }
                else if (nextStep == BallEvent.EVENT_JUDGE_FOCUS)
                {
                    setJudgeFocus(nextIndex);
                }
                else if (nextStep == BallEvent.EVENT_BASE_FOCUS)
                {
                    setBaseFocus(nextIndex);
                }
            }
        }


        //홈런시 화면 처리
        public int homerunFrame;
        public float hDX, hDY;

        public void setHomerunEvent()
        {
            bCameraBallMove = false;
            field.bFieldPerspectiveZoom = false;
            eventStep = BallEvent.EVENT_HOMERUN;

            for (int i = 0; i < CPlayer._LEFTFIELDER; i++)
            {
                field.fielder[i].setInitPosition();
            }
            nEventY = 0;
        }


        //볼의 움직임을 매프레임 업데이트 하는 함수
        public void move(float deltaTime) //FIELD_FieldBallMove
        {
            //////////UnityEngine.//Debug.Log("==================>>실제 speed = " +speed);
            //////////UnityEngine.//Debug.Log("==================>>실제 angle = " + angle);
            //////////UnityEngine.//Debug.Log("==================>>실제 angleZ = " + angleZ);

            if (field.bFieldViewActive == false) return; //if (field.manager.playState != PlayState.PLAY_FIELDING_VIEW) return;

            curTime += deltaTime;
            //fpsCalCoff = deltaTime / baseDeltaTime;

            //////////UnityEngine.//Debug.Log("==================>>nBallStep= " + nBallStep);

            if (step == BallStep.BALL_HIT)
            {
                ballHitMove(deltaTime);
            }
            else if (step == BallStep.BALL_THROW || step == BallStep.BALL_ERROR_STATE)
            {
                ballThrowMove(deltaTime);
            }
            else if (step == BallStep.BALL_CARRY)
            {
                ballCarryMove();
            }
            else if (step == BallStep.BALL_CATCH)
            {
                ballCatchMove();
            }
            else if (step == BallStep.BALL_WILD_PITCH)
            {
                ballWildPitchMove(deltaTime);
            }
            else if (step == BallStep.BALL_POLE_HIT)
            {
                ballPoleHit(deltaTime);
            }
            //바운더리 체크
            //setBoundary(20, 57);


            if (field.manager.playState == PlayState.PLAY_FIELDING_VIEW)
            {
                if (bCameraBallMove == false)
                {
                    if (eventStep == BallEvent.EVENT_HOMERUN)
                    {
                        CameraManager.SetCameraPos(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, 950, -200));
                        return;
                    }
                    else if (eventStep == BallEvent.EVENT_RUNNER_FOCUS)
                    {
                        eventRunnerFocus();
                    }
                    else if (eventStep == BallEvent.EVENT_FIELDER_FOCUS)
                    {
                        eventFielderFocus();
                    }
                    else if (eventStep == BallEvent.EVENT_JUDGE_FOCUS)
                    {
                        eventJudgeFocus();
                    }
                    else if (eventStep == BallEvent.EVENT_BASE_FOCUS)
                    {
                        eventBaseFocus();
                    }
                    else if (eventStep == BallEvent.EVENT_FOCUS_MOVE)
                    {
                        eventFoucsMove();
                    }
                }

#if _OrthoCamera
                float depthZ = -3.99999f + (nBallY * 0.0002f);                
                gameObject.transform.localPosition = new Vector3(screenX, screenY, depthZ);                
                if (step == BallStep.BALL_THROW)
                {
                    ballObj.transform.localPosition = new Vector3(0, 3.5f + nScreenBallZ, curTime < (throwingTime * 0.7f) ? -1 : 0.1f);// (bBallStop?0.1f:-1));//
                }
                else
                {
                    ballObj.transform.localPosition = new Vector3(0, 3.5f + nScreenBallZ, 0.1f);//
                }
#else
                gameObject.transform.localPosition = new Vector3(screenX, screenY, -1);
                ballObj.transform.localPosition = new Vector3(0, 3.5f + nScreenBallZ, getBallZPos());//
#endif
                getBallDepth();
                ballObj.transform.localScale = new Vector3(nBallDepth, nBallDepth, nBallDepth);
                //spr.scale = new Vector3(0.015f * nBallDepth, 0.0075f * nBallDepth, 1);
                colliderObj.transform.localPosition = new Vector3(0, 0, -7 - (nScreenBallZ / 2));

                if (bBallDeadState == true) return;

                if (bCameraBallMove == true)
                {
                    if (cameraWork == CameraWork.Shadow_Chase)
                    {
                        cameraDV += 0.2f;
                        if (cameraDV > 7) cameraDV = 7;
                        curCameraDX = (screenX - curCameraX) * cameraDV;
                        curCameraDY = (screenY - curCameraY) * cameraDV;
                        curCameraX += (curCameraDX * deltaTime);
                        curCameraY += (curCameraDY * deltaTime);
                    }
                    else if (cameraWork == CameraWork.Default)
                    {
                        curCameraX = screenX;
                        curCameraY = screenY;
                    }
                    else if (cameraWork == CameraWork.Ball_Chase)
                    {
                        cameraDV += 0.3f;
                        if (cameraDV > 10) cameraDV = 10;
                        curCameraDX = (screenX - curCameraX) * cameraDV;
                        curCameraDY = (screenY + nScreenBallZ - curCameraY) * cameraDV;
                        curCameraX += (curCameraDX * deltaTime);
                        curCameraY += (curCameraDY * deltaTime);

                        if (bBallOutofCamera == true)
                        {
                            if (nBallDZ < 0)
                            {
                                setTargetCamera(field.getScreenX(nFirstBoundX), field.getScreenY(nFirstBoundY) + 50, firstBoundTime * 0.5f);
                            }
                        }

                    }
                    else if (cameraWork == CameraWork.Popup)
                    {
                        curCameraX = screenX;
                        curCameraY = screenY + nScreenBallZ * 0.3f;

                        if (nBallDZ < 0)
                        {
                            setTargetCamera(field.getScreenX(nFirstBoundX), field.getScreenY(nFirstBoundY) + 50, firstBoundTime * 0.5f);
                            cameraTime = 0;
                        }

                    }
                    else if (cameraWork == CameraWork.Ball_Throw)
                    {
                        if (bThrowStart == true)
                        {
                            cameraTime += deltaTime;
                            if (cameraTime > throwingApprochTime)//0.15f)
                            {
                                cameraDV += 0.2f;
                                if (cameraDV > 7) cameraDV = 7;
                                curCameraDX = (screenX - curCameraX) * 5;
                                curCameraDY = (screenY - curCameraY) * 5;
                                curCameraX += (curCameraDX * deltaTime);
                                curCameraY += (curCameraDY * deltaTime);
                            }
                        }
                    }
                    else if (cameraWork == CameraWork.Make_Center)
                    {
                        cameraTime += deltaTime;
                        if (cameraTime < 1)
                        {
                            curCameraDX *= 0.9f;
                            curCameraDY *= 0.9f;
                            curCameraX += (curCameraDX * deltaTime);
                            curCameraY += (curCameraDY * deltaTime);
                        }
                    }
                    else if (cameraWork == CameraWork.Move_Target)
                    {
                        cameraTime += deltaTime;
                        curCameraX += (curCameraDX * deltaTime);
                        curCameraY += (curCameraDY * deltaTime);
                    }
                    
                    //바운더리 체크
                    checkBound();
                    CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + curCameraX, BallPlayManager.FIELDVIEW_CAMERA_INITY + curCameraY, -200));
                    
                }
                else
                {
                    curCameraX = nEventX;
                    curCameraY = nEventY;
                    //checkBound();
                    CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + curCameraX, BallPlayManager.FIELDVIEW_CAMERA_INITY + curCameraY, -200));
                }

                lastCameraX = curCameraX;
                lastCameraY = curCameraY;
            }

            //크라우드 범위 체크 비활성화 처리
            checkCrowdActive();
        }

        public void eventCameraMove()
        {
            if (bCameraBallMove == false)
            {
                if (eventStep == BallEvent.EVENT_FOCUS_MOVE)
                {
                    eventFoucsMove();
                }

                curCameraX = nEventX;
                curCameraY = nEventY;
                //checkBound();
                CameraManager.SetCameraPos(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + curCameraX, BallPlayManager.FIELDVIEW_CAMERA_INITY + curCameraY, -200));
            }
        }


        public bool bRightBound, bLeftBound;

        private void checkBound()
        {
            float wc = BallPlayManager.m_lcdWC * (1 / field.curZoom);// field._ZOOM_SIZE);
            float hc = BallPlayManager.m_lcdHC * (1 / field.curZoom);//field._ZOOM_SIZE);

            //우측경계
            if (bRightBound == false)
            {
                if (curCameraX + wc > field._FIELD_SIZE_X - 300)
                {
                    CameraRayCastType rayCastType = rayCasting(CameraManager.FieldCameray(false));
                    if (rayCastType == CameraRayCastType.Boarder)
                    {
                        ////Debug.Log("===============================>>우측 경계");
                        curCameraX = lastCameraX;
                        if (step == BallStep.BALL_HIT) bRightBound = true;

                        if (bHomeRunGuess) Invoke("setCameraworkNone", 0.8f);
                        else
                        {
                            if (bBallDeadState == false)
                            {
                                bBallDeadState = true;
                                if (bFairBallGuess == true)
                                {
                                    checkBallDeadCatch();
                                }
                                else
                                {
                                    field.setZoomStop();
                                }
                            }
                        }

                        /*이전
                        if (bHomeRunGuess || firstAngle < -45)
                        {
                            Invoke("setCameraworkNone", 0.8f);    
                        }*/
                    }
                }
            }
            else
            {
                curCameraX = lastCameraX;
            }

            //좌측경계
            if (bLeftBound == false)
            {
                if (curCameraX - wc < 300)
                {
                    CameraRayCastType rayCastType = rayCasting(CameraManager.FieldCameray(true));
                    if (rayCastType == CameraRayCastType.Boarder)
                    {
                        ////Debug.Log("===============================>>좌측 경계");
                        curCameraX = lastCameraX;
                        if (step == BallStep.BALL_HIT) bLeftBound = true;

                        if (bHomeRunGuess) Invoke("setCameraworkNone", 0.8f);
                        else
                        {
                            if (bBallDeadState == false)
                            {
                                bBallDeadState = true;
                                if (bFairBallGuess == true)
                                {
                                    checkBallDeadCatch();
                                }
                                else
                                {
                                    field.setZoomStop();
                                }

                            }
                        }
                        
                        //이전
                        /*if (bHomeRunGuess || firstAngle > 45)
                        {
                            Invoke("setCameraworkNone", 0.8f);    
                        }*/
                    }
                }
            }
            else
            {
                curCameraX = lastCameraX;
            }


            //상 경계
            if (curCameraY + hc > (field._FIELD_SIZE_Y - 300))
            {
                curCameraY = field._FIELD_SIZE_Y - 300 - hc;
                cameraWork = CameraWork.None;
            }

            //하 경계
            if (curCameraY - hc < 0)
            {
                curCameraY = hc;
            }

            //return false;
        }

        /// <summary>
        /// 카메라에 비치는 관중 활성화
        /// </summary>
        //bool[] bCrowdActive = new bool[4]{false,false,false,false};
        private void checkCrowdActive()
        {
            //
            for (int i = 0; i < 4; i++)
            {                
                Ray ray = CameraManager.FieldCameray(i);
                RaycastHit hitObj;
                if (Physics.Raycast(ray, out hitObj, Mathf.Infinity) == true)
                {
                    if (hitObj.transform.CompareTag("CROWD_TAG") == true)
                    {                        
                        int index = System.Convert.ToInt32(hitObj.transform.name);
                        FieldCrowdManager.SetCrowdActive(true, index);
                        //bCrowdActive[index] = true;
                    }
                }
            }
            
            /*
            for (int i = 0; i < 4; i++)
            {
                FieldCrowdManager.SetCrowdActive(bCrowdActive[i], i);
            }*/
        }

        private void setCameraworkNone()
        {
            cameraWork = CameraWork.None;
        }

        private CameraRayCastType rayCasting(Ray ray)
        {
            RaycastHit hitObj;
            if (Physics.Raycast(ray, out hitObj, Mathf.Infinity) == true)
            {
                if (hitObj.transform.CompareTag("BOARDER_TAG") == true)
                {
                    return CameraRayCastType.Boarder;
                }
            }
            return CameraRayCastType.None;
        }


        public void checkScroll(float xPos, float yPos)
        {
            nEventX = field.getScreenX(xPos); //screenX;
            nEventY = field.getScreenY(yPos); //screenY;
        }

        public void checkScroll()
        {
            screenX = field.getScreenX(nBallX);
            screenY = field.getScreenY(nBallY);
            nScreenX = screenX;
            nScreenY = screenY;
        }

        public void setThrowingCamera(float x, float y)
        {
            bThrowStart = false;
            cameraWork = CameraWork.Ball_Throw;
            cameraTime = 0;
            cameraDV = 5.0f;
        }

        public void setCenterCamera(float x, float y)
        {
            cameraWork = CameraWork.Make_Center;
            cameraTime = 0;
            /*curCenterX = x;
            curCenterY = y;

            curCameraDX = (curCenterX - curCameraX) / 120.0f;
            curCameraDY = (curCenterY - curCameraY) / 120.0f;*/
        }

        private void setTargetCamera(float x, float y, float remainTime)
        {
            ////UnityEngine.//Debug.Log("======================================>>setTargetCamera");
            cameraWork = CameraWork.Move_Target;
            cameraTime = 0;
            curCameraDX = (x - curCameraX) / remainTime;
            curCameraDY = (y - curCameraY) / remainTime;
            ////UnityEngine.//Debug.Log("======================================>>curCameraDX = " + curCameraDX);
            ////UnityEngine.//Debug.Log("======================================>>curCameraDY = " + curCameraDY);

        }


        public void setThrowingCamera()
        {
            bThrowStart = true;

            if (bLaserThrowFlag == true)
            {
                throwingApprochTime = 0.3f;                
            }
            else
            {
                throwingApprochTime = 0.15f;
            }
            //CameraManager.SetMotionBlurDelay(throwingTime*0.4f);
        }

        ///////////////////////////////////////////////////////
        //기타 계산 함수
        ///////////////////////////////////////////////////////

        static int[] _fence_height = new int[7] 
        {
            143, 125, 98, 80, 68, 65, 64
        };

        //펜스 방정식
        public bool fenceEquation(float x, float y, bool bHomerunCheck = false)
        {
            //float ratio = field.fRatio;
            float slope1, slope2;

            if (x < FieldSize.FENCE_LEFT_POLE_X)
            {
                if (y > FieldSize.FENCE_LEFT_POLE_Y)
                {
                    return true;
                }
            }
            else if (x < FieldSize.FENCE_LEFT_POINT_X1)
            {
                int wall1 = (bHomerunCheck ? _fence_height[0] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[1] : 0);
                slope1 = MyMath.getSlope(FieldSize.FENCE_LEFT_POLE_X, x, FieldSize.FENCE_LEFT_POLE_Y + wall1, y);
                slope2 = MyMath.getSlope(FieldSize.FENCE_LEFT_POLE_X, FieldSize.FENCE_LEFT_POINT_X1, FieldSize.FENCE_LEFT_POLE_Y + wall1, FieldSize.FENCE_LEFT_POINT_Y1 + wall2);
                if (slope1 > slope2) return true;
            }
            else if (x < FieldSize.FENCE_LEFT_POINT_X2)
            {
                int wall1 = (bHomerunCheck ? _fence_height[1] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[2] : 0);
                slope1 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X1, x, FieldSize.FENCE_LEFT_POINT_Y1 + wall1, y);
                slope2 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X1, FieldSize.FENCE_LEFT_POINT_X2, FieldSize.FENCE_LEFT_POINT_Y1 + wall1, FieldSize.FENCE_LEFT_POINT_Y2 + wall2);
                if (slope1 > slope2) return true;
            }
            else if (x < FieldSize.FENCE_LEFT_POINT_X3)
            {
                int wall1 = (bHomerunCheck ? _fence_height[2] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[3] : 0);
                slope1 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X2, x, FieldSize.FENCE_LEFT_POINT_Y2 + wall1, y);
                slope2 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X2, FieldSize.FENCE_LEFT_POINT_X3, FieldSize.FENCE_LEFT_POINT_Y2 + wall1, FieldSize.FENCE_LEFT_POINT_Y3 + wall2);
                if (slope1 > slope2) return true;
            }
            /*else if (x < FieldSize.FENCE_LEFT_POINT_X4)
            {
                int wall1 = (bHomerunCheck ? _fence_height[3] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[4] : 0);
                slope1 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X3, x, FieldSize.FENCE_LEFT_POINT_Y3 + wall1, y);
                slope2 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X3, FieldSize.FENCE_LEFT_POINT_X4, FieldSize.FENCE_LEFT_POINT_Y3 + wall1, FieldSize.FENCE_LEFT_POINT_Y4 + wall2);
                if (slope1 > slope2) return true;
            }*/
            else if (x < FieldSize.FENCE_LEFT_POINT_X5)
            {
                int wall1 = (bHomerunCheck ? _fence_height[4] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[5] : 0);
                //slope1 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X4, x, FieldSize.FENCE_LEFT_POINT_Y4 + wall1, y);
                //slope2 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X4, FieldSize.FENCE_LEFT_POINT_X5, FieldSize.FENCE_LEFT_POINT_Y4 + wall1, FieldSize.FENCE_LEFT_POINT_Y5 + wall2);
                slope1 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X3, x, FieldSize.FENCE_LEFT_POINT_Y3 + wall1, y);
                slope2 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X3, FieldSize.FENCE_LEFT_POINT_X5, FieldSize.FENCE_LEFT_POINT_Y3 + wall1, FieldSize.FENCE_LEFT_POINT_Y5 + wall2);
                if (slope1 > slope2) return true;
            }
            else if (x < FieldSize.GROUND_SIZEWC)
            {
                int wall1 = (bHomerunCheck ? _fence_height[5] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[6] : 0);
                slope1 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X5, x, FieldSize.FENCE_LEFT_POINT_Y5 + wall1, y);
                slope2 = MyMath.getSlope(FieldSize.FENCE_LEFT_POINT_X5, FieldSize.GROUND_SIZEWC, FieldSize.FENCE_LEFT_POINT_Y5 + wall1, FieldSize.getCencterFenceY() + wall2);
                if (slope1 > slope2) return true;
            }
            else if (x > FieldSize.FENCE_RIGHT_POLE_X)
            {
                if (y > FieldSize.FENCE_RIGHT_POLE_Y)
                {
                    return true;
                }
            }
            else if (x > FieldSize.FENCE_RIGHT_POINT_X1)
            {
                int wall1 = (bHomerunCheck ? _fence_height[6] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[5] : 0);
                slope1 = MyMath.getSlope(x, FieldSize.FENCE_RIGHT_POLE_X, y, FieldSize.FENCE_RIGHT_POLE_Y + wall1);
                slope2 = MyMath.getSlope(FieldSize.FENCE_RIGHT_POINT_X1, FieldSize.FENCE_RIGHT_POLE_X, FieldSize.FENCE_RIGHT_POINT_Y1 + wall2, FieldSize.FENCE_RIGHT_POLE_Y + wall1);
                if (slope1 < slope2) return true;
            }
            else if (x > FieldSize.FENCE_RIGHT_POINT_X2)
            {
                int wall1 = (bHomerunCheck ? _fence_height[5] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[4] : 0);
                slope1 = MyMath.getSlope(x, FieldSize.FENCE_RIGHT_POINT_X1, y, FieldSize.FENCE_RIGHT_POINT_Y1 + wall1);
                slope2 = MyMath.getSlope(FieldSize.FENCE_RIGHT_POINT_X2, FieldSize.FENCE_RIGHT_POINT_X1, FieldSize.FENCE_RIGHT_POINT_Y2 + wall2, FieldSize.FENCE_RIGHT_POINT_Y1 + wall1);
                if (slope1 < slope2) return true;
            }
            else if (x > FieldSize.FENCE_RIGHT_POINT_X3)
            {
                int wall1 = (bHomerunCheck ? _fence_height[4] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[3] : 0);
                slope1 = MyMath.getSlope(x, FieldSize.FENCE_RIGHT_POINT_X2, y, FieldSize.FENCE_RIGHT_POINT_Y2 + wall1);
                slope2 = MyMath.getSlope(FieldSize.FENCE_RIGHT_POINT_X3, FieldSize.FENCE_RIGHT_POINT_X2, FieldSize.FENCE_RIGHT_POINT_Y3 + wall2, FieldSize.FENCE_RIGHT_POINT_Y2 + wall1);
                if (slope1 < slope2) return true;
            }
            /*else if (x > FieldSize.FENCE_RIGHT_POINT_X4)
            {
                int wall1 = (bHomerunCheck ? _fence_height[3] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[2] : 0);
                slope1 = MyMath.getSlope(x, FieldSize.FENCE_RIGHT_POINT_X3, y, FieldSize.FENCE_RIGHT_POINT_Y3 + wall1);
                slope2 = MyMath.getSlope(FieldSize.FENCE_RIGHT_POINT_X4, FieldSize.FENCE_RIGHT_POINT_X3, FieldSize.FENCE_RIGHT_POINT_Y4 + wall2, FieldSize.FENCE_RIGHT_POINT_Y3 + wall1);
                if (slope1 < slope2) return true;
            }*/
            else if (x > FieldSize.FENCE_RIGHT_POINT_X5)
            {
                int wall1 = (bHomerunCheck ? _fence_height[2] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[1] : 0);
                //slope1 = MyMath.getSlope(x, FieldSize.FENCE_RIGHT_POINT_X4, y, FieldSize.FENCE_RIGHT_POINT_Y4 + wall1);
                //slope2 = MyMath.getSlope(FieldSize.FENCE_RIGHT_POINT_X5, FieldSize.FENCE_RIGHT_POINT_X4, FieldSize.FENCE_RIGHT_POINT_Y5 + wall2, FieldSize.FENCE_RIGHT_POINT_Y4 + wall1);
                slope1 = MyMath.getSlope(x, FieldSize.FENCE_RIGHT_POINT_X3, y, FieldSize.FENCE_RIGHT_POINT_Y3 + wall1);
                slope2 = MyMath.getSlope(FieldSize.FENCE_RIGHT_POINT_X5, FieldSize.FENCE_RIGHT_POINT_X3, FieldSize.FENCE_RIGHT_POINT_Y5 + wall2, FieldSize.FENCE_RIGHT_POINT_Y3 + wall1);
                if (slope1 < slope2) return true;
            }
            else if (x >= FieldSize.GROUND_SIZEWC)
            {
                int wall1 = (bHomerunCheck ? _fence_height[1] : 0);
                int wall2 = (bHomerunCheck ? _fence_height[0] : 0);
                slope1 = MyMath.getSlope(x, FieldSize.FENCE_RIGHT_POINT_X5, y, FieldSize.FENCE_RIGHT_POINT_Y5 + wall1);
                slope2 = MyMath.getSlope(FieldSize.GROUND_SIZEWC, FieldSize.FENCE_RIGHT_POINT_X5, FieldSize.getCencterFenceY() + wall2, FieldSize.FENCE_RIGHT_POINT_Y5 + wall1);
                if (slope1 < slope2) return true;
            }

            else
            {
                if (y >= FieldSize.FENCE_LEFT_POINT_Y5) return true;
            }


            if (bFairBallGuess == true)
            {
                return poleEquation(x, y);
            }
            return false;
        }

        public bool poleEquation(float x, float y)
        {
            //float ratio = field.fRatio;
            if (y >= FieldSize.FENCE_LEFT_POLE_Y)
            {
                if (x < FieldSize.FENCE_LEFT_POLE_X)
                {
                    ////////UnityEngine.//Debug.Log("================>> 여기서 헤헤헤");
                    return true;
                }

                else if (x > FieldSize.FENCE_RIGHT_POLE_X)
                {
                    ////////UnityEngine.//Debug.Log("================>> 아님 여기서 헤헤헤");
                    return true;
                }
            }
            return false;
        }


        //파울 방정식
        public bool foulEquation(float xPoint, float yPoint, float X1, float Y1, float X2, float Y2)
        {
            float slope, curslope;
            bool bFoul = true;
            if (speed < 0) return true;

            slope = Mathf.Abs((Y2 - Y1) / (X2 - X1));
            curslope = Mathf.Abs((yPoint - Y1) / (xPoint - X1));

            if (curslope >= slope) bFoul = false;
            return bFoul;
        }



        public Vector2 getFencePosition()
        {
            float hrX = nFirstBoundX;
            float hrY = nFirstBoundY * 0.6f;
            float homeX = FieldSize.getHomePosX();
            float homeY = FieldSize.getHomePosY();

            float slope = (hrY - homeY) / (hrX - homeX);

            if (slope == 0)
            {
                return new Vector2(hrX, hrY);
            }

            float b = homeY - slope * homeX;
            float initY = FieldSize.FENCE_LEFT_POLE_Y;
            float xPos;
            float yPos = initY;

            while (true)
            {
                xPos = (yPos - b) / slope;
                if (fenceEquation(xPos, yPos) == true)
                {
                    break;
                }
                yPos += 10;
            }

            return new Vector2(xPos, yPos / 0.6f);
        }

        
        private void makeBoundEffect(Vector3 pos)
        {
            GameObject obj = Util.Load("MainGame/prefabs/skeleton/effect/fieldGroundEffectPrefab", null, pos);
            obj.GetComponent<SkeletonAnimation>().state.SetAnimation(0, "GROUND_BALL_DUST_1", false);
            Destroy(obj, 0.5f);
        }



        private bool bFirstBoundActive;

        public void setFirstBound(bool bActive)
        {
            bFirstBoundActive = bActive;
            field.firstBound.SetActive(bActive);
            if (bActive == true)
            {
                field.firstBound.transform.localPosition = new Vector3(screenFirstBoundX, screenFirstBoundY, -0.01f);
            }
        }

        private void firstBoundUpdate()
        {
            if (bFirstBoundActive == true)
            {
                //float depth = 120;
                //if (nBallDZ < 0)
                //{
                float depth = Mathf.Clamp(nBallZ * 0.3f, 20, 120);
                    //depth = Mathf.Clamp(nBallZ * 0.15f, 40, 120);
                //}
                field.firstBound.transform.localScale = new Vector3(depth, depth*0.6f, 1);
            }
        }


        /// <summary>
        /// 볼데드시 송구 예외 처리해줌
        /// </summary>
        public void checkBallDeadThrow(int targetIndex)
        {
            if (bBallDeadState == true)
            {
                StartCoroutine(ballDeadThrowStep(targetIndex));
            }
        }


        private IEnumerator ballDeadThrowStep(int targetIndex)
        {
            float posX = 0;
            float posY = 0;

            if (targetIndex != -1)
            {
                posX = FieldSize.getBasePosX(targetIndex);
                posY = FieldSize.getBasePosY(targetIndex);
            }
            else
            {
                posX = field.getScreenX(field.fielder[field.nRelayFielderIndex].posX);
                posY = field.getScreenY(field.fielder[field.nRelayFielderIndex].posY);
            }

            yield return new WaitForSeconds(0.1f);            
            CameraManager.SetPositionTo(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + posX, BallPlayManager.FIELDVIEW_CAMERA_INITY + posY, -200), throwingTime);
            yield return new WaitForSeconds(throwingTime);
            cameraWork = CameraWork.Default;
            bCameraBallMove = true;
            bBallDeadState = false;
        }

        /// <summary>
        /// 볼데드시 포구 예외 처리해줌
        /// </summary>
        public void checkBallDeadCatch()
        {
            if (bBallDeadState == true)
            {
                StartCoroutine(ballDeadCatchStep());
            }
        }

        private IEnumerator ballDeadCatchStep()
        {
            int count = 0;
            int posIndex = 0;
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                posIndex = field.getCloseFielderIndex();
                if (posIndex >= CPlayer._LEFTFIELDER || count > 9)
                {
                    break;
                }                
                count++;
            }

            if (posIndex >= CPlayer._LEFTFIELDER)
            {
                float posX = field.getScreenX((field.fielder[posIndex].posX + nBallX)/2);
                float posY = field.getScreenY((field.fielder[posIndex].posY + nBallY)/2);
                CameraManager.SetPositionTo(new Vector3(BallPlayManager.FIELDVIEW_CAMERA_INITX + posX, BallPlayManager.FIELDVIEW_CAMERA_INITY + posY, -200), 1.5f);
                field.setZoomTo(1.0f, 1.5f);
                yield return new WaitForSeconds(1.5f);

                bRightBound = false;
                bLeftBound = false;
            }

        }
    }
}
