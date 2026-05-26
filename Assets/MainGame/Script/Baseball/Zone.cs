//#define _CONTROL_TYPE1    - 지워지워
//#define _TRACE_HISTORY  //  트레이스 히스토리를 보여준다 - 지워지워

using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class Zone : MonoBehaviour
    {
        int ZonePositionX = 0;
        int ZonePositionY = 277;
        int ZoneDepth = -6;
        const int batAngleGabY = 277;

        public const float BatterOrigineY = 49;
        public const float CURSOR_ZONE_MAX_X = 82; //스트존 (피칭UI)
        public const float CURSOR_ZONE_MAX_Y = 92; //스트존 (피칭UI)

        private BallPlayManager manager;

        public const float STRIKE_ZONE_WIDTH = 67;// 69;
        public const float STRIKE_ZONE_HEIGHT = 81;// 76;
        public const float STRIKE_ZONE_WIDTH_PV = 50;//  57;
        public const float STRIKE_ZONE_HEIGHT_PV = 61;// 70;

        public static float UI_ZONE_WIDTHC = 24;
        public static float UI_ZONE_HEIGHTC = 35;
        public static float UI_ZONE_WIDTH = STRIKE_ZONE_WIDTH;
        public static float UI_ZONE_HEIGHT = STRIKE_ZONE_HEIGHT;

        public static float UI_PINPOINTX, UI_PINPONITY; //핀포인트
        public static float UI_CENTERZONEX, UI_CENTERZONEY; //가운도 몰림
        public static float UI_BADZONEX, UI_BADZONEY;   //볼빠짐    


        public GameObject zoneObj;//, zoneObj2;
        public GameObject pTimerObj;//,timerObj;
        //public GameObject pCursorObj; 
        public GameObject bCursorObj;
        public tk2dSprite zoneSpr, bPointSpr, sweetSpot;//zoneSpr2,, pPointSpr;
        //public tk2dSprite trace1, trace2;


        //방망이 크기
        private float batScale = 1;
        private bool bBatCursorChange;
        public float batSizeCoef = 1.0f;


        //피칭 조작
        public bool controlActive;         
        public float curX, curY, lastX, lastY;
        float curDX, curDY;
        float offsetX, offsetY;


        public SkeletonAnimation munjiAnim;



        private bool bPvState;

        /// <summary>
        /// 존 인스턴스 초기화
        /// </summary>
        public void initInstance(BallPlayManager manager)
        {
            ZonePositionX = 0;
            ZonePositionY = 277;
            ZoneDepth = -6;

            this.manager = manager;
            transform.localPosition = new Vector3(0, ZonePositionY, -6);  //197
            zoneSpr = zoneObj.GetComponent<tk2dSprite>();
            
            float scale = 1.125f;// 0.65f;// 1.0f / 0.9f;
            zoneSpr.transform.localScale = new Vector3(scale, scale, 1);

            bBatCursorChange = false;


            UI_ZONE_WIDTH = STRIKE_ZONE_WIDTH;
            UI_ZONE_HEIGHT = STRIKE_ZONE_HEIGHT;
            UI_ZONE_WIDTHC = UI_ZONE_WIDTH / 2;
            UI_ZONE_HEIGHTC = UI_ZONE_HEIGHT / 2;


            UI_PINPOINTX = (Zone.UI_ZONE_WIDTH * 0.23f);
            UI_PINPONITY = (Zone.UI_ZONE_HEIGHT * 0.2f);
            UI_CENTERZONEX = (Zone.UI_ZONE_WIDTH * 0.32f);
            UI_CENTERZONEY = (Zone.UI_ZONE_HEIGHT * 0.26f);
            UI_BADZONEX = (Zone.UI_ZONE_WIDTH * 1.4f);
            UI_BADZONEY = (Zone.UI_ZONE_HEIGHT * 1.3f);
        }

        /// <summary>
        /// 존 업데이트
        /// </summary>
        void Update()
        {
            if (Mode.pitchControlType == PicthControlType.IndicatorType)
            {
                if (controlActive == true)
                {
                    if (manager.bMyTurn == false)
                    {
                        if (Mode.cameraView == CameraView.PitcherCenter)
                        {
                            manager.pitchPv.pitchOriginPv.indicatorUpdate(curX, curY);
                        }
                        else
                        {
                            //처리됨
                            manager.pitch.pitchOrigin.indicatorUpdate(curX, curY);
                        }
                        IngameUI.GetPitchUI().SetMove(curX, curY);
                    }
                }
            }
        }

        /// <summary>
        /// 존 세팅
        /// </summary>
        public void setZone(bool bActive, bool pCursor, bool bForceZone2)
        {
            if (bActive == true)
            {                
                //존 액티브
                initPosition();
                if (Mode.bAutoPlay == true)
                {
                    //자동 플레이시                    
                    //setBatCursorActive(true);
                    //controlActive = false;

                    //trace1.renderer.enabled = false;
                    //trace2.renderer.enabled = false;                    
                    //pPointSpr.renderer.enabled = false;                    
                }
                else
                {
                    //수동 플레이시
                    if (bForceZone2 == true)
                    {
                        //zoneObj.SetActive(false);
                        //zoneObj2.SetActive(true);
                    }
                    else
                    {
                        //zoneObj.SetActive(manager.bMyTurn);
                        //zoneObj2.SetActive(!manager.bMyTurn);
                        zoneObj.SetActive(true);
                    }

                    setCursorWeightInit();
                    if (pCursor == true)
                    {
                        //pPointSpr.renderer.enabled = !manager.bMyTurn;
                        //pCursorObj.transform.localPosition = new Vector3(curX, curY, -0.1f);
                        //trace1.renderer.enabled = false;
                        //trace2.renderer.enabled = false;
                        //setPitchCursorIndex(1);
                        if (manager.bMyTurn == false)
                        {
                            if (Mode.pitchControlType == PicthControlType.IndicatorType)
                            {
                                //pPointSpr.renderer.enabled = false;
                                controlActive = true;
                            }
                            else
                            {
                                
                            }
                        }

                    }
                    else
                    {
                        controlActive = false;
                        //pPointSpr.renderer.enabled = false;   
                    }
                }
            }
            else
            {
                //존 디액티브
                zoneObj.SetActive(false);
                //zoneObj2.SetActive(false);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////
        //피칭 UI
        //////////////////////////////////////////////////////////////////////////////////////////////////        

        /// <summary>
        /// 유저 컨트롤시 투구 릴리즈시 존 상태
        /// </summary>
        public void setUserControlPitchRelease()
        {
            if (Mode.pitchControlType == PicthControlType.IndicatorType)
            {
                IngameUI.GetPitchUI().SetPitchCursor(true, curX, curY); //유저가 세팅한 탄착점
                offsetX = 0;
                offsetY = 0;
                manager.pitcher.courseX = curX + offsetX - manager.pitcher.preHenkaX;
                manager.pitcher.courseY = curY + offsetY - manager.pitcher.preHenkaY;
                //Debug.Log("보내는 course??===========>>cursorX = " + curX + "=======>>cursorY = " + curY);
            }
                        
            controlActive = false;
            //pPointSpr.renderer.enabled = false;

        }

        /// <summary>
        /// 피처 커서 위치 초기화 ( 초기에는 가중치 초기화 였는데 함수의 기능이 바뀜)
        /// </summary>
        public void setCursorWeightInit()
        {
            curX = 0;
            curY = 0;
            lastX = 0;
            lastY = 0;
            curDX = 0;
            curDY = 0;
        }

        /*
        /// <summary>
        /// 피처 커서의 스프라이트의 인덱스를 설정하고 위치를 초기화 한다.
        /// </summary>
        public void setPitchCursorIndex(int index)
        {
            pPointSpr.transform.localPosition = new Vector3(0, 0, 0);
            pPointSpr.spriteId = pPointSpr.GetSpriteIdByName("ballcursor" + index);
            pPointSpr.color = new Vector4(1, 1, 1, 1);
        }*/

        /*
        /// <summary>
        /// 피처 커서의 알파값을 조절
        /// </summary>
        public void setPitchCursorAlpha(float a)
        {
            pPointSpr.color = new Vector4(1, 1, 1, a);
        }*/

        /// <summary>
        /// 피처 커서의 위치를 설정
        /// </summary>
        public void setPitchCursorPos(float x, float y, float maxX, float maxY)
        {
            if (Mode.pitchControlType == PicthControlType.IndicatorType) //인디케이터 타입
            {
                float cx = x * Zone.STRIKE_ZONE_WIDTH / maxX;
                float cy = y * Zone.STRIKE_ZONE_HEIGHT / maxY;
                curX = cx;
                curY = cy;

                //pCursorObj.transform.localPosition = new Vector3(curX, curY, -0.15f);
                
            }
           
        }

        /// <summary>
        /// 볼이 스트라이크 존을 지날시 탄착점을 설정해준다
        /// </summary>
        public void setArriveCursorPos(bool bStrike)
        {
            ////////UnityEngine.//Debug.Log("================>>setCursorPos");
            if (Mode.bAutoPlay == false)
            {
                //자동 플레이시 해당 사항 없음
                //float courseX, courseY;
                float arriveX, arriveY;

                arriveX = manager.pitcher.arriveX;
                arriveY = manager.pitcher.arriveY;

                /*pPointSpr.renderer.enabled = true;

                setPitchCursorAlpha(1);
                setPitchCursorIndex(bStrike ? 3 : 4);
                pCursorObj.transform.localPosition = new Vector3(arriveX, arriveY, -0.15f);

                courseX = manager.pitcher.courseX2;
                courseY = manager.pitcher.courseY2;

                trace1.renderer.enabled = true;
                trace2.renderer.enabled = true;

                trace1.transform.localPosition = new Vector3(courseX - arriveX, courseY - arriveY, 0.0001f);
                trace2.transform.localPosition = new Vector3((courseX - arriveX) / 2, (courseY - arriveY) / 2, 0.0002f);*/

                if (Mode.cameraView == CameraView.PitcherCenter)
                {
                    IngameUI.GetPitchUI().SetArrivePos(manager, -arriveX, arriveY);
                }
                else
                {
                    IngameUI.GetPitchUI().SetArrivePos(manager, arriveX, arriveY);
                }

                //ballPitchedNum++;
            }

        }



        //////////////////////////////////////////////////////////////////////////////////////////////////
        //타격 UI
        //////////////////////////////////////////////////////////////////////////////////////////////////  
      
        /// <summary>
        /// 타격시 볼을 칠수 있는 탄착점과 타이밍 인디케이터를 설정해준다
        /// </summary>
        public IEnumerator setTimingIndicator(float arriveTime)
        {
            if (Mode.bAutoPlay == false)
            {
                //자동 플레이시 해당 사항 없음
                float arriveX, arriveY;

                //if (Mode.bPowerfulType == true)
                {
                    arriveX = manager.pitcher.courseX;
                    arriveY = manager.pitcher.courseY;
                }
                /*else
                {
                    arriveX = manager.pitcher.preArriveX;
                    arriveY = manager.pitcher.preArriveY;
                }*/

                pTimerObj.SetActive(true);
                pTimerObj.transform.localPosition = new Vector3(arriveX, arriveY, 0);
                pTimerObj.GetComponent<perfectTimer>().init(arriveTime);
                frame = 0;

                yield return new WaitForSeconds(arriveTime);
                pTimerObj.SetActive(false); 
                
            }
        }

        int frame;

        public void setTimerPos(float x, float y)
        {
            if (pTimerObj != null)
            {
                if (++frame > 20)
                {

                    pTimerObj.transform.localPosition = new Vector3(x, y, 0);
                }
            }
        }

        /// <summary>
        /// 타격 커서의 위치를 설정해줌
        /// </summary>
        public void setBatCursorPos(float x, float y, float maxX, float maxY)
        {
            manager.batter.cursorX = x * Zone.STRIKE_ZONE_WIDTH / maxX;
            manager.batter.cursorY = y * Zone.STRIKE_ZONE_HEIGHT / maxY;
            bCursorObj.transform.localPosition = new Vector3(manager.batter.cursorX, manager.batter.cursorY, -0.16f);

            float angle = 0;
            if (manager.batter.bGangTa == false)
            {
                angle = 90 + Mathf.Atan2(BatterOrigineY - manager.batter.cursorY, (-manager.batter.sign * batAngleGabY) - manager.batter.cursorX) * Mathf.Rad2Deg;
            }

            //float size = (manager.batter.bGangTa ? 0.5f : 1) * batSizeCoef;
            if (bPvState == true)
            {
                angle = -angle;
                bCursorObj.transform.localScale = new Vector3(0.8f, 0.8f, 1);
            }
            else
            {
                bCursorObj.transform.localScale = new Vector3(1, 1, 1);
            }


            bCursorObj.transform.localEulerAngles = new Vector3(0, 0, angle);
        }

        public void setGangtaCursor()
        {
            if (manager.bMyTurn == true)
            {
                if (manager.batter.bGangTa == true)
                {
                    bPointSpr.gameObject.SetActive(false);
                    sweetSpot.spriteId = sweetSpot.GetSpriteIdByName("powerbat");
                    //sweetSpot.transform.localScale = new Vector3(0.7f, 0.7f, 1);
                    bCursorObj.transform.localEulerAngles = Vector3.zero;
                }
                else
                {
                    bPointSpr.gameObject.SetActive(true);
                    sweetSpot.spriteId = sweetSpot.GetSpriteIdByName("plus");
                    //sweetSpot.transform.localScale = Vector3.one;
                    bPointSpr.transform.localPosition = new Vector3(0, -47, 0);
                    bPointSpr.spriteId = bPointSpr.GetSpriteIdByName("bat");
                }
            }
        }

        /// <summary>
        /// 타격 커서의 위치를 설정해줌
        /// 터치패드에서 입력받은 위치를 존에 알맞게 변환하여 입력해줄때 사용
        /// </summary>
        public void setBatCursorPos2(float x, float y, float maxX, float maxY)
        {
            manager.batter.cursorX = x * Zone.STRIKE_ZONE_WIDTH / maxX;
            manager.batter.cursorY = y * Zone.STRIKE_ZONE_HEIGHT / maxY;

            setBatCursorPos();
        }

        public void setBatCursorPos()
        {
            bCursorObj.transform.localPosition = new Vector3(manager.batter.cursorX, manager.batter.cursorY, -0.14f);

            float angle = 0;
            if (manager.batter.bGangTa == false)
            {
                angle = 90 + Mathf.Atan2(BatterOrigineY - manager.batter.cursorY, (-manager.batter.sign * batAngleGabY) - manager.batter.cursorX) * Mathf.Rad2Deg;
            }

            if (manager.batter.sign == -1)
            {
                if (angle < 90) angle = 90;
            }
            else
            {
                if (angle < 0) angle = 270;
            }
            if (bPvState == true) angle = -angle;
            bCursorObj.transform.localEulerAngles = new Vector3(0, 0, angle);

        }

        /// <summary>
        /// 타격 커서의 활성화 /  비활성화
        /// </summary>
        public void setBatCursorActive(bool bActive)
        {
            if (manager.bMyTurn == true && Mode.bAutoPlay == false)
            {
                bCursorObj.SetActive(bActive);
            }
            else
            {
                bCursorObj.SetActive(false);
            }
        }

        /// <summary>
        /// 타격 커서의 알파값을 설정해줌
        /// </summary>
        public void setBatCursorAlpha(float a)
        {
            bPointSpr.color = new Vector4(1, 1, 1, a);
        }

        public void setBatterSize(float coef)
        {
            bPointSpr.transform.localScale = new Vector3(coef, coef);
            bPointSpr.transform.localPosition = new Vector3(0, -47.0f * coef, 0);
        }



        public IEnumerator setMunji(float x, float y)
        {
            //munjiAnim.transform.localPosition = new Vector3(x, -50 + y, -99);
            CameraManager.CameraShake(0.1f, 10);
            munjiAnim.transform.localPosition = new Vector3(x, y - 20, -99);
            munjiAnim.GetComponent<Renderer>().enabled = true;
            munjiAnim.skeleton.SetToSetupPose();
            munjiAnim.state.SetAnimation(0, "BALLSMOG_0"+Random.Range(1,3), false);
            yield return new WaitForSeconds(2.0f);
            munjiAnim.GetComponent<Renderer>().enabled = false;
        }




        public void initPosition()
        {
            float scale;// = 1.125f;

            if (Mode.cameraView == CameraView.PitcherCenter && Mode.bPitchingViewActive == true)
            {
                bPvState = true;
                int _LeftPitcherGab = (manager.pitcher.pitchHand == CPlayer._LEFTHAND? -260:0);
                scale = 1;
                ZonePositionX = 124 + _LeftPitcherGab;
                ZonePositionY = 480;
                ZoneDepth = -6;
                zoneSpr.spriteId = zoneSpr.GetSpriteIdByName("ball_zone2");

                UI_ZONE_WIDTH = STRIKE_ZONE_WIDTH_PV;
                UI_ZONE_HEIGHT = STRIKE_ZONE_HEIGHT_PV;
                UI_ZONE_WIDTHC = UI_ZONE_WIDTH / 2;
                UI_ZONE_HEIGHTC = UI_ZONE_HEIGHT / 2;
            }
            else
            {
                bPvState = false;
                scale = 1.125f;
                ZonePositionX = 0;
                ZonePositionY = 277;
                ZoneDepth = -6;
                zoneSpr.spriteId = zoneSpr.GetSpriteIdByName("ball_zone");
                                
                UI_ZONE_WIDTH = STRIKE_ZONE_WIDTH;
                UI_ZONE_HEIGHT = STRIKE_ZONE_HEIGHT;
                UI_ZONE_WIDTHC = UI_ZONE_WIDTH / 2;
                UI_ZONE_HEIGHTC = UI_ZONE_HEIGHT / 2;
            }

            UI_PINPOINTX = (Zone.UI_ZONE_WIDTH * 0.2f);
            UI_PINPONITY = (Zone.UI_ZONE_HEIGHT * 0.18f);
            UI_CENTERZONEX = (Zone.UI_ZONE_WIDTH * 0.45f);
            UI_CENTERZONEY = (Zone.UI_ZONE_HEIGHT * 0.42f);
            UI_BADZONEX = (Zone.UI_ZONE_WIDTH * 1.4f);
            UI_BADZONEY = (Zone.UI_ZONE_HEIGHT * 1.3f);

            transform.localPosition = new Vector3(ZonePositionX, ZonePositionY, ZoneDepth);
            zoneSpr.transform.localScale = new Vector3(scale, scale, 1);
        }

    }
}