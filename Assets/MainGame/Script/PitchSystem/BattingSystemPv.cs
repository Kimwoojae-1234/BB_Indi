//#define _Test_BattingSystem

using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class BattingSystemPv : MonoBehaviour
    {
        const int startZ = -250;

        //public Camera battingCamera;
        public Transform origin;
        public GameObject ballObj, ballSpr;
        public GameObject tail;

        private BallPlayManager manager;
        private Field field;

        //private bool bSideView;

        public float x, y, z;
        public float dx, dy, dz;
        public float aX, aY, aZ;


        const float Gravity_Accel = -200;// -700.00f;
        const float GROUND_Y = -20;
        const float PowerRate = 22.857f;
        const float XAxisRate = -6.667f;
        const float YAxisRateFly = 5.0f;
        const float YAxisRateLine = 7.0f;
        const float YAxisRateBound = 10.0f;
        


        public float BOUND_RATE = 1;
        public float BOUND_RATE_Y = 3;


        public bool bBallMove = false;

        private bool bGrounder = false;
        private bool bGoodHit = false;
        private bool bBound = false;
        private bool bTailOn = false;
        private bool changeFieldChecked = false;


        //35/800    y
        //15/-100   x
        //따볼  10/100
        //플라이 20/100

        
        public void initInstance(BallPlayManager _manager)
        {
            this.manager = _manager;
            this.field = _manager.field;

            initPosition();
            setSystemDraw(false);
        }


        public void initPosition()
        {
            origin.localPosition = new Vector3(0, 0, 0);
            ballObj.transform.localPosition = new Vector3(0, 0, startZ);
            tail.SetActive(false);
            bTailOn = false;            
        }


        // Use this for initialization
        void Start()
        {
            bBallMove = false;
        }

#if _Test_BattingSystem

        public void init(float initX, float initY)
        {
            x = initX;
            y = initY;
            z = startZ;
            ballObj.transform.localPosition = new Vector3(x, y, z);            
            dx = TESTX;
            dy = TESTY;
            dz = TESTZ;
            bBallMove = true;
            groundNum = 0;

            ballSpr.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            
        }

        public float TESTX = 0;
        public float TESTY = 20;
        public float TESTZ = 100;

        public float TEST_GA = -300;
        public float TEST_GROUND_Y = 0;

        int groundNum = 0;

        void Update()
        {
            if (bBallMove == true)
            {
                x += dx * Time.deltaTime;
                y += dy * Time.deltaTime;
                z += dz * Time.deltaTime;

                dy += TEST_GA * Time.deltaTime;

                ballObj.transform.localPosition = new Vector3(x, y, z);

                float scale = getBallScale();
                ballSpr.transform.localScale = new Vector3(scale, scale, scale);


                if (y < TEST_GROUND_Y)
                {
                    setBound();
                    groundNum++;
                }
            }
        }
#else

        void Update()//FixedUpdate()
        {            
            if (bBallMove == true)
            {

                x += dx * Time.deltaTime;
                y += dy * Time.deltaTime;
                z += dz * Time.deltaTime;
                dy += Gravity_Accel * Time.deltaTime;
                
                ballObj.transform.localPosition = new Vector3(x, y, z);

                float scale = getBallScale();
                ballSpr.transform.localScale = new Vector3(scale, scale, scale);

                if (y < GROUND_Y)
                {
                    setBound();
                }

                if (changeFieldChecked == false)
                {
                    if (bGrounder == true)// manager.hitBallType == HITBALLTYPE._GROUNDER || field.ball.firstAngleZ < 5)
                    {
                        if (bBound == true || field.ballPower < 20)
                        {
                            changeFieldChecked = true;
                            StartCoroutine(changeFieldView(0.4f));
                        }
                    }
                    else
                    {
                        if (bGoodHit == true)
                        {
                            StartCoroutine(changeFieldView(1.0f));
                        }
                        else
                        {
                            StartCoroutine(changeFieldView2(0.5f));
                        }
                        changeFieldChecked = true;
                    }
                }
            }
        }
#endif

        private float getBallScale()
        {
            if (z > 100) return 0.1f;
            else
            {
                return Mathf.Abs((z-100) / 350.0f) * 1.0f + 0.5f;
            }
        }

        private void setBound()
        {
            bBound = true;
#if _Test_BattingSystem
            y = TEST_GROUND_Y;
#else
            y = GROUND_Y;
#endif
            dy = -dy * BOUND_RATE_Y;
            //dz = dz;// *BOUND_RATE;
            //dx = dx;// *BOUND_RATE;
            if (bGrounder == true)
            {
                tail.SetActive(true);
                bTailOn = true;
            }
        }

        private IEnumerator changeFieldView(float delay)
        {
            yield return new WaitForSeconds(delay);


            activateField(); //임시


            field.fieldState = FieldState.NORMAL_FIELD;
            manager.changeFieldView(field.fieldState);
            field.ball.setCameraPosInitAfterHit();
            setSystemDraw(false);
            //yield return new WaitForSeconds(0.1f);

        }

        private IEnumerator changeFieldView2(float delay)
        {
            yield return new WaitForSeconds(delay);

            activateField(); //임시

            yield return new WaitForSeconds(0.2f);
            field.fieldState = FieldState.NORMAL_FIELD;
            manager.changeFieldView(field.fieldState);
            field.ball.setCameraPosInitAfterHit();
            setSystemDraw(false);

            //yield return new WaitForSeconds(0.1f);

        }




        public void setHitVector()
        {
            bBound = false;
            changeFieldChecked = false;

            Vector3 initPos = manager.pitchPv.pitchOriginPv.getBallHitPos(startZ);
            x = initPos.x;
            y = initPos.y;
            z = startZ;
            ballObj.transform.localPosition = initPos;
            ballSpr.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            dz = field.ballPower * PowerRate;
            dx = field.ball.firstAngle * XAxisRate;
            if (field.ball.firstAngleZ > 10) dy = field.ball.firstAngleZ * YAxisRateFly;
            else if (field.ball.firstAngleZ > 0) dy = field.ball.firstAngleZ * YAxisRateLine;
            else dy = field.ball.firstAngleZ * YAxisRateBound;
           
            if (manager.hitBallType == HITBALLTYPE._GROUNDER || field.ball.firstAngleZ < 1)
            {
                bGrounder = true;
            }
            else
            {
                bGrounder = false;
                if (manager.batter.bHitGood == true)
                {
                    bGoodHit = true;
                }
                else
                {
                    bGoodHit = false;
                }
            }

            //배팅뷰 수비 활성화 시키려면 이걸 액티브 시키면 됨
            //Invoke("activateField", waitTime);

            setHitBallNextStep();
        }

        private void activateField()
        {
            manager.field.setFieldHitState();
            manager.field.bFieldViewActive = true;
        }

        public void setHitBallNextStep()
        {
            setSystemDraw(true);
            if (bGoodHit == true)
            {
                //manager.battingview.justmeet.transform.localPosition = new Vector3(0, 474 + mainCameraPositionY, -0.5f);
            }
        }


        public void setSystemDraw(bool bActive)
        {
            bBallMove = bActive;

            if (bActive == true)
            {
                setBallDraw();
            }
            else
            {
                ballObj.SetActive(false);
            }
        }


        private void setBallDraw()
        {
            ballObj.SetActive(true);
            if (bGrounder == false)
            {
                tail.SetActive(true);
                bTailOn = true;
            }
        }
    }
}
