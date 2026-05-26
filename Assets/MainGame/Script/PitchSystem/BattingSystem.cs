//#define _Test_BattingSystem

using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class BattingSystem : MonoBehaviour
    {
        public Camera battingCamera;
        public Transform origin, cameraOrigin;
        public GameObject ballObj;
        public GameObject tail;
        public GameObject ballLine;
        
        private BallPlayManager manager;
        private Field field;

        //private bool bSideView;

        public float x, y, z;
        public float dx, dy, dz;
        public float aX, aY, aZ;

        
        const float Gravity_Accel = -550;// -700.00f;
        const float TEST_GROUND_Y = -20;


        public float BOUND_RATE = 1;
        public float BOUND_RATE_Y = 0.5f;


        public bool bBallMove = false;

        private bool bGrounder = false;
        private bool bGoodHit = false;
        private bool bBound = false;
        private bool bTailOn = false;
        private bool changeFieldChecked = false;

        private bool bHookSlice;
        private float hookSliceAngle;

        private bool bBlurSetting = false;
        private float blurDV;


        private bool bLowAngle;


        float changeFieldTime;

        public void initInstance(BallPlayManager _manager)
        {
            this.manager = _manager;
            this.field = _manager.field;

            initPosition();
            setSystemDraw(false);
        }


        public void initPosition()
        {
            Vector3 originPos = new Vector3(0, 2, 450);
            Vector3 cameraOriginPos = new Vector3(0, 0, 450);
            Vector3 cameraPos = new Vector3(0, 15, -290);
            Vector3 initCameraRotation = new Vector3(-3, 0, 0);
            
            origin.localPosition = originPos;
            cameraOrigin.localPosition = cameraOriginPos;
            cameraOrigin.localEulerAngles = Vector3.zero;
            battingCamera.transform.localPosition = cameraPos;
            battingCamera.transform.localEulerAngles = initCameraRotation;
            bTailOn = false;
            bRotateAngle = false;
            ballLine.SetActive(false);
        }


        // Use this for initialization
        void Start()
        {
            bBallMove = false;

            //테스트용
#if _Test_BattingSystem
            bTestBallMove = false;
            dx = TESTX;
            dy = TESTY;
            dz = TESTZ;
#endif

        }

#if _Test_BattingSystem
        public float TESTX = 30;
        public float TESTY = 300;
        public float TESTZ = 1500;
        public bool bTestBallMove = false;
        public void testUpdate()
        {
            if (bTestBallMove == true)
            {
                x += dx * Time.deltaTime;
                y += dy * Time.deltaTime;
                z += dz * Time.deltaTime;

                dy += Gravity_Accel * Time.deltaTime;

                ballObj.transform.localPosition = new Vector3(x, y, z);

                if (y < TEST_GROUND_Y)
                {
                    bTestBallMove = false;
                    x = 0;
                    y = 0;
                    z = 0;
                    dx = TESTX;
                    dy = TESTY;
                    dz = TESTZ;
                    ballObj.transform.localPosition = new Vector3(x, y, z);
                }
            }
        }
#endif

        // Update is called once per frame
#if _Test_BattingSystem
        void Update()
        {
            rotateUpdate();


            testUpdate();

            if (bTestBallMove == false)
            {
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    bTestBallMove = true;
                    dx = TESTX;
                    dy = TESTY;
                    dz = TESTZ;
                }
            }
        }

#else
        void Update()//FixedUpdate()
        {
            rotateUpdate();
            
            if (bBlurSetting == true)
            {
                /*if (CameraManager.SetBlurSize(0,blurDV*Time.deltaTime) == false)
                {
                    bBlurSetting = false;
                }*/
            }

            if (bBallMove == true)
            {
                
                x += dx * Time.deltaTime;
                y += dy * Time.deltaTime;
                z += dz * Time.deltaTime;

                dy += Gravity_Accel * Time.deltaTime;
                if (bHookSlice == true)
                {
                    dx += hookSliceAngle;
                }

                ballObj.transform.localPosition = new Vector3(x, y, z);

                if (y < TEST_GROUND_Y)
                {
                    setBound();
                }

                if (changeFieldChecked == false)
                {
                    if (bGrounder == true)// manager.hitBallType == HITBALLTYPE._GROUNDER || field.ball.firstAngleZ < 5)
                    {
                        if (bBound == true)
                        {
                            changeFieldChecked = true;
                            StartCoroutine(changeFieldView(0.4f));// changeFieldTime));// 0.4f));                            
                        }
                    }
                    else
                    {
                        if (bGoodHit == true)
                        {
                            if (dy < 0)
                            {
                                CameraManager.SetMotionBlur(false);
                                changeFieldChecked = true;
                                StartCoroutine(changeFieldView(0.1f));//changeFieldTime));//0.1f));
                            }
                        }
                        else
                        {
                            changeFieldChecked = true;
                            StartCoroutine(changeFieldView2(0.2f));//changeFieldTime)); //0.2f));
                        }
                    }
                }
            }
        }
#endif

        private void setBound()
        {
            bBound = true;
            y = TEST_GROUND_Y;
            dy = -dy * BOUND_RATE_Y;
            dz = dz * BOUND_RATE;
            dx = dx * BOUND_RATE;
            if (bTailOn == true)
            {
                bTailOn = false;
                tail.SetActive(false);
            }
        }

        private IEnumerator changeFieldView(float delay)
        {
            Debug.Log("=================================================aaaaaaaaaaaaaaaaaaaaaaaaaa");

            yield return new WaitForSeconds(delay);


            if (Mode.bPvpMode433 == true)
            {
                if (manager.bMyTurn == false)
                {
                    while (manager.Pvp_FiendSync == false)
                    {
                        //필드 싱크 인포가 들어올때까지 대기
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

            activateField(); //임시


            field.fieldState = FieldState.NORMAL_FIELD;
            manager.changeFieldView(field.fieldState);
            field.ball.setCameraPosInitAfterHit();
            setSystemDraw(false);
            //yield return new WaitForSeconds(0.1f);
            
        }

        private IEnumerator changeFieldView2(float delay)
        {
            Debug.Log("=================================================bbbbbbbbbbbbbbbbbbbbbbbb");
            yield return new WaitForSeconds(delay);
            
            if (Mode.bPvpMode433 == true)
            {
                if (manager.bMyTurn == false)
                {
                    while (manager.Pvp_FiendSync == false)
                    {
                        //필드 싱크 인포가 들어올때까지 대기
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

            activateField(); //임시

            setRotateView(0.15f, false);
            yield return new WaitForSeconds(0.2f);
            field.fieldState = FieldState.NORMAL_FIELD;
            manager.changeFieldView(field.fieldState);
            field.ball.setCameraPosInitAfterHit();
            setSystemDraw(false);

            //yield return new WaitForSeconds(0.1f);
            
        }




        public void setHitVector()
        {
            bHookSlice = false;
            bBound = false;
            changeFieldChecked = false;

            z = 0;
            x = manager.batter.effectX * 0.2f;
            y = (manager.batter.effectY) * 0.2f;
            ballObj.transform.localPosition = new Vector3(x, y, z);


            ////Debug.Log("==================>> dx = " + field.ball.nBallDX);
            ////Debug.Log("==================>> dy = " + field.ball.nBallDZ);
            ////Debug.Log("==================>> dz = " + field.ball.nBallDY);

            if (field.ball.firstAngleZ < 25.1f)
            {
                bLowAngle = true;
                dy = field.ball.nBallDZ * 1.1f;// TESTY;
            }
            else
            {
                bLowAngle = false;
                dy = field.ball.nBallDZ * 0.85f;// TESTY;
            }

            dx = field.ball.nBallDX * 1.5f;// TESTX;            
            dz = field.ball.nBallDY * 1.8f; //1.5f;// TESTZ;

            float waitTime = 0;
            if (manager.hitBallType == HITBALLTYPE._GROUNDER || field.ball.firstAngleZ < 1)
            {
                bGrounder = true;
                waitTime = 0.3f;
                if (field.bGrounderAvailble == true)
                {
                    changeFieldTime = Mathf.Clamp(field.fastRemainTime * 0.5f,0.02f,0.4f);                    
                }
                else
                {
                    changeFieldTime = 0.4f;
                }
            }
            else
            {
                bGrounder = false;
                if (manager.batter.bHitGood == true)
                {
                    bGoodHit = true;
                    waitTime = field.ball.firstBoundTime * 0.8f;
                    changeFieldTime = 0.1f;
                    Invoke("activeBallLine", waitTime * 0.4f);

                    CameraManager.SetMotionBlur(true);
                }
                else
                {
                    dy = field.ball.nBallDZ * 1.1f;// TESTY;
                    bGoodHit = false;
                    waitTime = 0.3f;
                    changeFieldTime = 0.2f;
                }
            }

            //배팅뷰 수비 활성화 시키려면 이걸 액티브 시키면 됨
            //Invoke("activateField", waitTime);
            

        }

        private void activateField()
        {
            manager.field.setFieldHitState();
            manager.field.bFieldViewActive = true;
        }

        private void activeBallLine()
        {
            if (field.ball.firstAngleZ > 5)
            {
                ballLine.SetActive(true);
            }
        }

        //private float rotateAngle;
        //테스트용
        /*public float radius = 2;
        public float speed = 0.8f;
        public float amp = 0.15f;*/

        public void setHitBallNextStep()
        {
            bRotateAngle = false;
            setSystemDraw(true);
            if (manager.batter.bHitGood == true)
            {
                //블러 효과
                //CameraManager.SetBlur2(true, 3,0,1);
                //blurDV = -3.0f / 0.4f;
                //bBlurSetting = true;


                if (bLowAngle == false)
                {
                    if (manager.batter.bHitHomeRun == true)
                    {
                        //ShockWave.Get().StartIt(manager.batter.zoneUI.transform.position, 2.5f, 1.2f, 0.1f);
                    }
                }
                

                float timeSet = field.ball.firstBoundTime * 0.3f;

                if (field.ball.firstAngleZ > 10)
                {
                    if (field.ball.firstAngle > 22)
                    {
                        //좌측 펜스 사이드
                        if (bLowAngle == false)
                        {
                            manager.battingview.setLeftBigHomerunAngle();
                        }
                        

                        setSideView(true, timeSet);
                    }
                    else if (field.ball.firstAngle < -22)
                    {
                        //우측 펜스 사이드
                        if (bLowAngle == false)
                        {
                            manager.battingview.setRightBigHomerunAngle();
                        }
                        
                        setSideView(false, timeSet);
                    }
                    else
                    {
                        //센터

                        if (bLowAngle == false)
                        {
                            manager.battingview.setBigHomerunAngle();
                        }

                        setRotateView(timeSet, true);
                    }
                }
                else
                {
                    //센터
                    setRotateView(timeSet, true);
                }

            }
        }

        private void setRotateView(float timeRemain, bool bHitGood)
        {
            Debug.Log("==============================================>> setRotate view");
            float rotateTime = timeRemain;
            float rotateAngle = Mathf.Clamp(field.ball.firstAngle, -15.0f, 15.0f);            
            float rotateAngleY = Mathf.Clamp(field.ball.firstAngleZ - 30.0f , 0, 15);

            float mainCameraPositionX = -rotateAngle * 12;
            float mainCameraPositionY = rotateAngleY * 8.9f;

            //훅 & 슬라이스
            bHookSlice = field.ball.bHookorSlice;
            hookSliceAngle = -field.ball.angleHookSlice *0.7f;

            //2D 카메라 움직임
            //CameraManager.SetCameraPos(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360 + mainCameraPositionY, -200));            
            //CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640 + mainCameraPositionX, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360 + mainCameraPositionY, -200), rotateTime);                        
            //앵글체인지
            //CameraManager.SetCameraPos(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 395, -200));
            //CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640 + mainCameraPositionX, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 395, -200), rotateTime);

            if (bHitGood == true)
            {
                manager.battingview.justmeet.transform.localPosition = new Vector3(0, 474 + mainCameraPositionY, -0.5f);
                TweenPosition.Begin(manager.battingview.justmeet.gameObject, rotateTime, new Vector3(mainCameraPositionX * 1.5f, 474 + mainCameraPositionY, -0.5f));

                if (bLowAngle == true)
                {
                    CameraManager.SetCameraPos(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360 + mainCameraPositionY, -200));
                    CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640 + mainCameraPositionX, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360 + mainCameraPositionY, -200), rotateTime);            
                }
                else
                {
                    CameraManager.SetCameraPos(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 395, -200));
                    CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640 + mainCameraPositionX, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 395, -200), rotateTime);
                }
            }
            else
            {
                //2D 카메라 움직임
                CameraManager.SetCameraPos(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360 + mainCameraPositionY, -200));            
                CameraManager.SetPositionTo(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640 + mainCameraPositionX, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360 + mainCameraPositionY, -200), rotateTime);            
            }

            //3D카메라 회전
            targetAngleY = -rotateAngle * 0.5f;
            targetAngleX = -rotateAngleY * 0.6f;
            angleDY = targetAngleY / rotateTime;
            //angleDX = targetAngleX / rotateTime;
            if (targetAngleY < 0) angleDY = -angleDY;
            //if (targetAngleX < 0) angleDX = -angleDX;

            bRotateAngle = true;
        }


        private void setSideView(bool bLeft, float timeRemain)
        {
            float rotateTime = timeRemain;            
            float gabX = 0;
            float curAngle = field.ball.firstAngle;

            if (bLowAngle == false)
            {
                dy = field.ball.nBallDZ * 0.85f;
            }
            else
            {
                dy = field.ball.nBallDZ * 1.1f;// *0.72f;//dy 재계산
            }

            if (bLeft == true)
            {
                //좌측펜스
                    if (curAngle > 35)
                    {
                        dx = -20.0f * curAngle + 800.0f;
                    }
                    else
                    {
                        dx = 100.0f;
                        gabX = 20.0f * curAngle - 700.0f;
                    }
                    manager.battingview.setLeftView(-50);//bLowAngle ? -50 : -80);
            }
            else
            {
                //if (manager.batter.sign == -1)
                {
                    //좌타자 우측펜스
                    if (curAngle < -35)
                    {
                        dx = -20.0f * curAngle - 800.0f;
                    }
                    else
                    {
                        dx = -100.0f;
                        gabX = (20.0f * curAngle + 700.0f) * 0.8f;
                    }
                }
                /*else
                {
                    //우타자 우측펜스
                    if (curAngle < -45)
                    {
                        dx = -10.0f * curAngle - 350.0f;
                    }
                    else
                    {
                        dx = 100.0f;
                        gabX = (15.0f * curAngle + 675.0f) * 0.8f;
                    }
                }*/
                manager.battingview.setRightView(-50);//bLowAngle ? -50 : -80);
            }

            //훅 & 슬라이스
            bHookSlice = field.ball.bHookorSlice;
            hookSliceAngle = -field.ball.angleHookSlice *0.7f;// *Time.deltaTime;

            //2D카메라 움직임
            CameraManager.SetCameraPos(new Vector3((bLeft ? 730 : 550), 915, -200));
            //float yMax = Mathf.Clamp(1000 + (field.ball.firstAngleZ-20), 1000, 1030);
            CameraManager.SetPositionTo(new Vector3((bLeft ? 730 : 550), 915, -200), rotateTime);

            //3D카메라 회전
            float rotateAngleY = Mathf.Clamp(field.ball.firstAngleZ - 20.0f, 0, 15);
            targetAngleY = gabX / -70.0f;// angleDevide;
            targetAngleX = -rotateAngleY * 0.6f;
            angleDY = targetAngleY / rotateTime;
            if (targetAngleY < 0) angleDY = -angleDY;

            bRotateAngle = true;
        }

        //public float angleDevide = 100.0f;
        //public float gabRate = 0.8f;


        bool bRotateAngle = false;
        float targetAngleX, targetAngleY;
        float angleDX, angleDY;
        private void rotateUpdate()
        {
            if (bRotateAngle == true)
            {
                float angleX = targetAngleX;// Mathf.MoveTowardsAngle(cameraOrigin.eulerAngles.x, targetAngleX, angleDX * Time.deltaTime);
                float angleY = Mathf.MoveTowardsAngle(cameraOrigin.eulerAngles.y, targetAngleY, angleDY * Time.deltaTime);
                cameraOrigin.eulerAngles = new Vector3(angleX, angleY, 0);
            }

        }



        public void setSystemDraw(bool bActive)
        {
            battingCamera.gameObject.SetActive(bActive);
            bBallMove = bActive;

            if (bActive == true)
            {
                StartCoroutine(setBallDraw(0.04f));
            }
            else
            {
                ballObj.SetActive(false);
            }
        }


        private IEnumerator setBallDraw(float delay)
        {
            yield return new WaitForSeconds(delay);
            ballObj.SetActive(true);
            yield return new WaitForSeconds(0.05f);
            tail.SetActive(true);
            bTailOn = true;
        }
    }
}
