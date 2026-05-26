using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class BBall : MonoBehaviour
    {
        const int InitCenterStatndY = 226;

        const float _DNear = 300;
        const float _X_COEFF = 5.0f;// 8.0f; //4.0f   
        const float _Z_COEFF = 3.7f; //3.1f;//4.0f;//3.5f;
        const float _OFFSET_COEFF = 0.667f;
        const float _ROTATE_MISSOFFSET = 5.0f;  //회전 카메라 오차 옵셋

        //카메라 한계치 값
        const int rightCameraGabLimit = -140;//normal
        const int leftCameraGabLimit = 140;// 140;  //
        const int sideCameraGabLimit = 580;
        const int centerCameraGabLimit = 300;

        //
        const float _GRAVITY_ACCELERATION = -765.135f;
        const float _InitPosY = -159.0f;
        //////////////////////////////////////////////////////////////
        //PITCH BALL & HIT BALL 공통
        //////////////////////////////////////////////////////////////
        //public tk2dSpriteAnimator ballAnim;
        public tk2dSprite ballShadow;
        public GameObject ballAnim;

        private BallPlayManager manager;
        private Pitcher pitcher;
        private Field field;

        //기본 벡터
        private float screenX, screenY;
        private float nBallX, nBallY, nBallZ;
        private float nBallDX, nBallDY, nBallDZ;
        private float nScreenBallZ;
        private float limitDZ;  //파울 홈런 짜르는 수치
        private float rotationX;    //공의 회전
        private float xOffset;

        //카메라 연출 관련
        //private float cameraY, cameraDY;
        private bool bLeftView, bCenterView, bRightView;
        private bool bCameraWork;
        private float standZoom, standZoomDV;
                
        //field로 부터 얻어오는 힛볼관련 벡터값
        private float speed, nLastSpeed,
                      angleZ,
                      angle, nLastAngle;

        //관련 변수 및 플래그
        private float initHomeY;
        private int nBoundNum;
        private bool bBallStop = false;
        private bool bBound = false;
        //각도 보정 계수
        private float zCalibCoef = 1.0f;
        //기타
        private float remainTime;        
        private bool bBallMove;
        //수비수의 동작 여부
        private bool bFielderActionType;

        private GameObject center, left, right;


        //인스턴스 초기화 함수
        public void initInstance(BallPlayManager manager)
        {
            this.manager = manager;
            this.pitcher = manager.pitcher;
            this.field = manager.field;
            ballShadow = transform.Find("ballShadow").gameObject.GetComponent<tk2dSprite>();
            setBallDraw(false);

            center = manager.battingview.centerObj.transform.Find("stand").gameObject;// BackGroundManager.FindStand();
            //left = manager.battingview.leftObj.transform.Find("stand").gameObject;// //살려살려
            //right = manager.battingview.rightObj.transform.Find("stand").gameObject;// //살려살려
        }

        void Update()
        {
            if (bBallMove == true)
            {
                showHitBall();
            }
        }

        //볼의 액티브 여부
        public void setBallDraw(bool bDrawBall)//, bool bDrawShadow)
        {
            ballAnim.gameObject.SetActive(bDrawBall);
            ballShadow.gameObject.SetActive(bDrawBall);
            bBallMove = bDrawBall;
        }
        

        //힛볼 벡터 셋팅
        public void setHitVector()
        {
            lastY = 840;
            bBallMove = false;
            rotationX = Random.Range(0,300);
            float power = field.ballPower;
            speed = field.ball.speed;
            angleZ = field.ball.angleZ;  //0~3까지
            angle = field.ball.angle;  //-3~3까지
            zCalibCoef = getZCoef();

            
            xOffset = pitcher.getCurrentZoneX(Zone.STRIKE_ZONE_WIDTH); //0;
            nBallX = 0;
            nBallY = 0;
            initHomeY = nBallY;
            nBallZ = FieldParm.BALL_INIT_HEIGHT;			//처음 높이
            setVelocity();
            setVelocityZ(field.ballPower);
            bBallStop = false;
            bBound = false;
            nBoundNum = 0;
            
            bCameraWork = false;
            bCenterView = true;
            bLeftView = bRightView = false;
            limitDZ = nBallDZ * -0.5f;

            field.battingviewFieidingType = FieldParm.BattingViewFieldingType.None;

            field.setFieldHitState();

            if (field.battingviewFieidingType != FieldParm.BattingViewFieldingType.None && field.bGrounderAvailble == true)
            {
                //홈런 연출
                bCameraWork = true;
                bool bBatterClip = checkViewAngle();
                remainTime = field.fastRemainTime;
                
                StartCoroutine(changeFieldView(remainTime, bBatterClip));
                
            }
            else
            {
                remainTime = 0.3f;
                StartCoroutine(changeFieldView(remainTime,false));
            }
            
            

        }

        //필드뷰로 전환
        private IEnumerator changeFieldView(float delay, bool bBatterClip)
        {
            if (bBatterClip == true)
                manager.batter.setColor(new Color(1, 1, 1, 0.5f));

            yield return new WaitForSeconds(delay * 0.5f);

            if (bBatterClip == true)
                manager.batter.setColor(new Color(1, 1, 1, 0));

            manager.field.bFieldViewActive = true; //이걸 어디다 두느냐
            
            yield return new WaitForSeconds(delay * 0.5f);

            

            field.fieldState = FieldState.NORMAL_FIELD;
            manager.changeFieldView(field.fieldState);
            field.ball.setCameraPosInitAfterHit();
            setBallDraw(false);

            initBackView();

            /*
            //가짜
            yield return new WaitForSeconds(delay);
            manager.field.bFieldViewActive = true;
            field.fieldState = FieldState.NORMAL_FIELD;
            manager.changeFieldView(field.fieldState);
            setBallDraw(false);//*/
        }

        private void initBackView()
        {
            if (bLeftView == true)
            {
                left.transform.localScale = Vector3.one;
            }
            else if (bRightView == true)
            {
                right.transform.localScale = Vector3.one;
            }
            else
            {
                center.transform.localScale = Vector3.one;
                center.transform.localPosition = new Vector3(0, InitCenterStatndY, 0);
            }
            CameraManager.SetZoomFactor(1.0f);
        }

        //사이드뷰 혹은 정면뷰 체크
        private bool checkViewAngle()
        {
            bCenterView = true;
            float angleDash = field.ball.firstAngle;
            if (angleDash > 25)
            {
                zCalibCoef = zCalibCoef - 0.1f;
                bCenterView = false;
                bLeftView = true;
                return (manager.batter.sign == -1 && angleDash <40 ? true : false);
            }
            else if (angleDash < -25)
            {
                zCalibCoef = zCalibCoef - 0.1f;
                bCenterView = false;
                bRightView = true;
                return (manager.batter.sign == 1 && angleDash > -40 ? true : false);
            }
            else
            {
                return (manager.batter.sign * field.ball.firstAngle > 0 ? true : false);
            }
        }

        private float getZCoef()
        {
            float coef = 1.1f;

            if (angleZ > 45) coef = 0.75f;
            else if (angleZ > 35) coef = 0.85f;
            else if (angleZ > 25) coef = 1.1f;

            else if (angleZ < 10) coef = 1.3f;

            return coef;

        }

        //줌 프레임
        private void cameraMove()
        {
            
        }

        //속도 벡터 셋팅
        private void setVelocity()
        {
            if (angle < 0) angle = 360 + angle;
            float rad = angle * Mathf.Deg2Rad;

            nBallDX = -speed * Mathf.Sin(rad);
            nBallDY = speed * Mathf.Cos(rad);
            nLastSpeed = speed;
            nLastAngle = angle;
        }

        //Z축 속도 세팅
        private void setVelocityZ(float power)
        {
            ////////Debug.Log("============================>>Time.deltaTime = " + Time.deltaTime);
            nBallDZ = power * Mathf.Sin(angleZ * Mathf.Deg2Rad) / FBall.NORMAL_DELTATIME;
            nScreenBallZ = nBallZ * FBall._Z_AXIS_PROJECTION_COEFF;
        }

        //볼 임팩트후 다음과정 세팅
        public void setHitBallNextStep()
        {
            setBallDraw(true);

            if (bCameraWork == true)
            {
                standZoom = 1.0f;
                if (bLeftView == true)
                {
                    //좌측뷰 세팅
                    manager.battingview.setLeftView(0);
                    standZoomDV = -0.5f / remainTime;                    
                    //standZoomDV = -0.1f / remainTime;                    
                }
                else if (bRightView == true)
                {
                    //우측뷰 세팅
                    manager.battingview.setRightView(0);
                    standZoomDV = -0.5f / remainTime;                    
                    //standZoomDV = -0.1f / remainTime;                    
                }
                else
                {
                    standZoomDV = -0.4f / remainTime;                    
                }

                CameraManager.SetZoomTo(1.6f, remainTime);
                //CameraManager.SetZoomTo(1.16f, remainTime);
            }

            //pitcher.pitcherHitAnim();
        }

        //볼의 움직임 처리
        private void ballMove()
        {
            //bool bBoundEffect = false;
            nBallX += (nBallDX * Time.deltaTime);
            nBallY += (nBallDY * Time.deltaTime);

            nBallZ += (nBallDZ * Time.deltaTime);
            nBallDZ += (_GRAVITY_ACCELERATION * Time.deltaTime);
            
            if (nBallZ <= 0)
            {
                nBallZ = 0;
                nBoundNum++;
                nBallDZ = -(nBallDZ / 3.0f);
                //if (field.ball.bHomeRunGuess == false) bBoundEffect = true;

            }

            float changeX, changeY;
            float offset = 0;//xOffset * getX(changeY);

            if (bLeftView == true)
            {
                changeX = getXAxisChange(nBallX, nBallY, -45 + _ROTATE_MISSOFFSET);
                changeY = getYAxisChange(nBallX, nBallY, -45 + _ROTATE_MISSOFFSET);

                screenY = getY(changeY);// * BattingViewInitZoom;
                screenX = changeX * getX(changeY) * getSideOffsetX(field.ball.firstAngle);// * BattingViewInitZoom;

                nScreenBallZ = nBallZ * getZ(changeY) * 1.2f;
            }
            else if (bRightView == true)
            {
                changeX = getXAxisChange(nBallX, nBallY, 45 - _ROTATE_MISSOFFSET);
                changeY = getYAxisChange(nBallX, nBallY, 45 - _ROTATE_MISSOFFSET);

                screenY = getY(changeY);// * BattingViewInitZoom;
                screenX = changeX * getX(changeY) * getSideOffsetX(field.ball.firstAngle);// * BattingViewInitZoom;

                nScreenBallZ = nBallZ * getZ(changeY) * 1.2f;
            }
            else
            {
                changeX = nBallX;
                changeY = nBallY;
                offset = xOffset * getOffsetX(changeY);

                screenY = getY(changeY);// * BattingViewInitZoom;
                screenX = changeX * getX(changeY);// * BattingViewInitZoom;

                nScreenBallZ = nBallZ * getZ(changeY);
            }


            float depth = getHitBallDepth();

            float ballDepth = depth * 50.0f;
            //if (ballDepth > 50) ballDepth = 50;

            ballAnim.transform.localScale = new Vector3(ballDepth, ballDepth, ballDepth);
            ballShadow.transform.localScale = new Vector3(depth, depth, 1);

            float z = 0;// (field.ball.bHomeRunGuess == true && depth < 0.2f ? 0.5f : 0);

            if (screenY > 0) z = 0;
            else z = -1;

            //공의 스케일
            ballAnim.transform.localPosition = new Vector3(screenX + offset, screenY + nScreenBallZ, manager.pitcher._initZOrder + 3.1f + z);
            ballShadow.transform.localPosition = new Vector3(screenX + offset, screenY, manager.pitcher._initZOrder + 3.15f + z);

            //공의 회전
            rotationX += (1000 * Time.deltaTime);
            ballAnim.transform.localEulerAngles = new Vector3(rotationX, 0, 0);

            /*
            if (bBoundEffect == true)
            {
                if (pitcher.batter.sign * field.ball.firstAngle < 20)
                {
                    Vector3 pos = ballShadow.transform.position;// new Vector3(ballShadow.transform.position.x, ballShadow.transform.position.y, ballShadow.transform.position.z + 9);
                    GameObject boundEffect = Instantiate(Resources.Load("MainGame/prefabs/effectPrefab/batting/fx_ballbound_batting"), pos, Quaternion.identity) as GameObject;
                    float boundDepth = depth * 3.5f;// *600;
                    ////Debug.Log("=====================>>boundDepth = " + boundDepth);
                    boundEffect.transform.localScale = new Vector3(boundDepth, boundDepth, 1);
                    boundEffect.transform.parent = manager.pitcher.transform;
                    Destroy(boundEffect, 1.5f);
                    bBoundEffect = false;
                }
            }*/

            if (bBallStop == false)
            {                
                if (bBound == false)
                {
                    speed += (FBall._AIR_FRICTION_COEFF * Time.deltaTime);
                }
            
                if (speed != nLastSpeed || angle != nLastAngle)
                {
                    setVelocity();
                }
            }
        }

        private float lastY;
        //힛볼을 그래픽 처리
        private void showHitBall()
        {            
            ballMove();
            if (bCameraWork == true)
            {
                standZoom += standZoomDV * Time.deltaTime;

                if (bLeftView == true)
                {
                    float x = Mathf.Clamp(ballAnim.transform.position.x, 300, 980);
                    float y = ballShadow.transform.position.y + (250 / CameraManager.GetZoomFactor());
                    CameraManager.SetCameraPos(new Vector3(x, y, -200));

                    left.transform.localScale = new Vector3(standZoom, standZoom, 1);
                }
                else if (bRightView == true)
                {
                    float x = Mathf.Clamp(ballAnim.transform.position.x, 300, 980);
                    float y = ballShadow.transform.position.y + (250 / CameraManager.GetZoomFactor());
                    CameraManager.SetCameraPos(new Vector3(x, y, -200));

                    right.transform.localScale = new Vector3(standZoom, standZoom, 1);
                }
                else
                {
                    float x = Mathf.Clamp(ballAnim.transform.position.x, 300, 980);
                    float y = Mathf.Clamp(ballShadow.transform.position.y + (212 / CameraManager.GetZoomFactor())-100, lastY, 10000);
                    lastY = y;
                    CameraManager.SetCameraPos(new Vector3(x, y, -200));
                    
                    center.transform.localScale = new Vector3(standZoom, standZoom, 1);
                    float gabX = (x - (BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640)) * 0.5f;
                    center.transform.localPosition = new Vector3(gabX, InitCenterStatndY, 0);
                }

                
            }
        }

        //타구의 뎁스 구함
        private float getHitBallDepth()
        {
            //GROUND_SIZEH = 2400 * 0.25 = 600;
            float depth = (1 - (nBallY - initHomeY) / 450.0f);// *0.8f;
            return Mathf.Clamp(depth, 0.2f, 0.6f);
        }


        /*
        //2차방정식 근의 공식
        float getEquation(float a, float b, float c, bool bBig)
        {
            //2차방정식의 근의 공식 (bBig값이 true이면 큰값을 리턴)
            float val1;
            float _4abc = (bBig == true ? 1 : -1) * Mathf.Sqrt((b * b - 4 * a * c));
            val1 = (-b + _4abc) / (2 * a);
            return val1;
        }*/

        //x축 회전
        private float getXAxisChange(float x, float y, float degree)
        {
            float rad = degree * Mathf.Deg2Rad;
            return Mathf.Cos(rad) * x - Mathf.Sin(rad) * y;
        }

        //y축 회전
        private float getYAxisChange(float x, float y, float degree)
        {
            float rad = degree * Mathf.Deg2Rad;
            return Mathf.Sin(rad) * x + Mathf.Cos(rad) * y;
        }

        //위치벡터로부터 스크린 y값을 투영하는 함수
        private float getY(float originY)
        {

            float c = 2.23f * (originY * originY) + 151232.95f * originY;
            float b = -402.88f * originY - 101947.58f;
            float a = 148.22f;

            //볼 오브젝트의 의 위치로부터 홈까지의 거리(_initPosY)
            //배팅뷰 오브젝트의 시작 위치 -80, 홈베이스 위치 80 간격 160
            //따라서 도출된 값에 -_initPosY+160을 해준다 - 차후 조정 될시 참조 바람

            //float value;
            //value = (MyMath.getEquation(a, b, c, false) - Pitcher._initPosY + 160);            
            float value = (MyMath.getEquation(a, b, c, false) * 0.5f + _InitPosY);            

            return value;
        }

        //위치값으로부터 스크린 X값을 투영하는 함수
        private float getX(float originY)
        {
            //간단한 2차 투영공식을 통해 산출된 이값을 계수값으로 조정한후 x값에 곱해준다
            return _X_COEFF * BallPlayManager.m_lcdW / getCurW(originY);
        }

        private float getCurW(float originY)
        {
            //투영거리로부터 절두체 far까지의 거리 originY+_DNear
            float W = (_DNear + originY) * BallPlayManager.m_lcdW / _DNear;
            return W;
        }

        private float getOffsetX(float originY)
        {
            //간단한 2차 투영공식을 통해 산출된 이값을 계수값으로 조정한후 x값에 곱해준다
            return _OFFSET_COEFF * BallPlayManager.m_lcdW / getCurW(originY);
        }


        private float getSideOffsetX(float degree)
        {   
            //return (Mathf.Abs(degree) * (-0.05f)) + 3.25f;
            return (Mathf.Abs(degree) * (-0.025f)) + 2.125f;
        }

        private float getZ(float originY)
        {
            //간단한 2차 투영공식을 통해 산출된 이값을 계수값으로 조정한후 z값에 곱해준다           
            return _Z_COEFF * zCalibCoef * BallPlayManager.m_lcdH / getCurH(originY);
        }

        private float getCurH(float originY)
        {
            //투영거리로부터 절두체 far까지의 거리 originY+_DNear
            float H = (_DNear + originY) * BallPlayManager.m_lcdH / _DNear;
            return H;
        }


    }
}
