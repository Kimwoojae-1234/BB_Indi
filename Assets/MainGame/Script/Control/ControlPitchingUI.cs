using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{

    public class ControlPitchingUI : MonoBehaviour
    {
        private bool bPitchingView = false; //true인 경우 피칭뷰

        public GameObject _active;
        protected BallPlayManager manager = null;

        public float CURSOR_ZONE_MAX_X = 65;// 82; //스트존 (피칭UI)
        public float CURSOR_ZONE_MAX_Y = 73;//92; //스트존 (피칭UI)

        public GameObject cursor;
        public tk2dSprite cursorSpr;
        tk2dUIDragItem drag;
        tk2dUIItem pos;


        //private int selectedGuwee;
        bool bCursorMove;
        float cursorX, cursorY;
        float initX, initY;
        float uiX, uiY;

        public tk2dSprite dragLight;

        public GameObject timerObj, timerOrigin;

        //인스턴스
        private static ControlPitchingUI Instance_;


        private static float screenW = (720 * Screen.width) / Screen.height;
        private static float screenWC = screenW / 2;


        void Awake()
        {
            Instance_ = this;
            manager = null;
        }

        void OnDestroy()
        {
            Instance_ = null;
        }
        // Use this for initialization
        void Start()
        {
            //base.fadeInit();
            drag = cursor.GetComponent<tk2dUIDragItem>();
            pos = cursor.GetComponent<tk2dUIItem>();
            uiX = screenWC + transform.localPosition.x;  //640 + transform.localPosition.x;
            uiY = 360 + transform.localPosition.y;
        }

        // Update is called once per frame
        void Update()
        {
            //base.update();

            if (bCursorMove == true)
            {
                cursorFrame();
            }


            if (bTimerOn == true)
            {
                timerCursorFrame();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //외부 호출용 static함수
        //////////////////////////////////////////////////////////////////////////////////////
        public static void SetActive(bool bActive, BallPlayManager _manager, bool fade = false)
        {
            //Instance_.initInstance(_manager);
            if (bActive == true)
            {
                if (_manager.pitcher.bPitchTimerOn == true) return;
                //Instance_.active(fade, 8);
                Instance_._active.SetActive(true);
                Instance_.manager = _manager;
                Instance_.init();
            }
            else
            {
                //Instance_.deActive(fade, 5);
                Instance_._active.SetActive(false);
                
            }
        }

        public static void TimerDeactive()
        {
            Instance_.timerObj.SetActive(false);
        }

        private void init()
        {            
            cursor.transform.localPosition = new Vector3(0, 0, -1);
            cursorSpr.GetComponent<Renderer>().enabled = true;
            //manager.battingview.zoneUI.setControlUI(true);
            bCursorMove = false;
            cursorX = cursorY = 0;

            setPitchCursor(0, 0);

            dragLight.gameObject.SetActive(false);

            //int balltype = (int)ControlPitchingSelect.GetSelectBall();
            //ballTxt.spriteId = ballTxt.GetSpriteIdByName("balltype"+balltype);

            if (Mode.cameraView == CameraView.PitcherCenter)
            {
                manager.pitchPv.pitchOriginPv.setIndicator(manager.pitcher);
            }
            else
            {
                //처리됨
                manager.pitch.pitchOrigin.setIndicator(manager.pitcher);
            }
            
        }


        //컨트롤 혹은 배트 커서 경계 설정 
        private void cursorFrame()
        {
            if (cursorSpr.GetComponent<Renderer>().enabled == false) cursorSpr.GetComponent<Renderer>().enabled = true;

            float x = cursor.transform.localPosition.x + initX;
            float y = cursor.transform.localPosition.y + initY;

            if (x >= 85)
            {
                x = 85;
                cursorSpr.GetComponent<Renderer>().enabled = false;
            }
            else if (x <= -85)
            {
                x = -85;
                cursorSpr.GetComponent<Renderer>().enabled = false;
            }

            if (y >= 95)
            {
                y = 95;
                cursorSpr.GetComponent<Renderer>().enabled = false;
            }
            else if (y <= -95)
            {
                y = -95;
                cursorSpr.GetComponent<Renderer>().enabled = false;
            }

            if (manager.bMyTurn == true)
            {
                
            }
            else
            {
                setPitchCursor(x, y);
            }
        }

        private void setPitchCursor(float x, float y)
        {
            manager.battingview.zoneUI.setPitchCursorPos(x, y, CURSOR_ZONE_MAX_X, CURSOR_ZONE_MAX_Y);
        }

        public void setDown()
        {
            dragLight.gameObject.SetActive(true);

            float sx = Screen.width;
            float sy = Screen.height;
            float rx = pos.Touch.position.x * screenW / sx; //pos.Touch.position.x * 1280 / sx;
            float ry = pos.Touch.position.y * 720 / sy;
            //initX = rx - uiX;
            //initY = ry - uiY;
            initX = (rx - uiX);// -463; 
            initY = (ry - uiY);// +203; 
            //cursorSpr.transform.localPosition = new Vector3(initX, initY, -0.1f);// pos.transform.position.z - 0.1f);
            //////Debug.Log("===========>> rx = "+rx);
            //////Debug.Log("===========>> ry = " + ry);initX, initY;

            bCursorMove = true;

            if (manager.bMyTurn == false)
            {
                manager.battingview.zoneUI.controlActive = true;
                manager.battingview.zoneUI.setCursorWeightInit();
            }
        }

        public void setRelease()
        {
            dragLight.gameObject.SetActive(false);
            //int index = manager.bMyTurn ? 0 : 1;
            bCursorMove = false;

            if (manager.bMyTurn == true)
            {
            }
            else
            {
                if (manager.pitcher.bRelease == false)
                {
                    if (manager.pitcher.bPitchTimerOn == true) return;

                    if (Mode.cameraView == CameraView.PitcherCenter)
                    {
                        manager.pitchPv.pitchOriginPv.releaseIndicator();
                    }
                    else
                    {
                        manager.pitch.pitchOrigin.releaseIndicator();
                    }
                    manager.battingview.zoneUI.setUserControlPitchRelease();

                    /* //기존 버전
                    _active.SetActive(false);
                    timerObj.SetActive(true);
                    initTimer();*/

                    //개선버전
                    _active.SetActive(false);
                    releasePitching();


                    manager.pitcher.bCatcherMoveFlag = true;
                }
            }
        }


        public GameObject curSpr;
        public tk2dSprite perfectLight;//, perfectLightBack;
        public tk2dSprite missSpr;        
        

        private float badX, normalX, goodX, perfectX;


        private float curPosX, curDX;
        private float initDX;

        private bool bTimerOn;

        private int lightNum, count;


        private const float InitPos = -117.0f;
        private const float FinalPos = 117.0f;

        private const float NormalPos = 26.16693f;
        private const float GoodPos = 57f;
        private const float PerfectPos = 60f;

        private const float NormalWidth = 36;
        private const float GoodWidth = 30;
        private const float PerfectWidth = 6;

        /*
        private void setGauge(float guwee)
        {
            float rate = 0.2f + (guwee / 1200.0f);
            if (rate > 1.3f) rate = 1.3f; //PerfectWidth 0.2
            float rate2 = rate;
            if (rate2 < 0.5f) rate2 = 0.5f;//NormalWidth 최저0.5, //GoodWidth 최저0.5
                        
            badX = NormalPos - (NormalWidth * rate2);
            normalX = GoodPos - (GoodWidth * rate2);
            goodX = PerfectPos - (PerfectWidth * rate);
            perfectX = PerfectPos + (PerfectWidth * rate);

        }*/

        public static void InitTimer()
        {
            Instance_.initTimer();
        }

        private void initTimer()
        {
            timerObj.SetActive(true);
            if (Mode.cameraView == CameraView.PitcherCenter)
            {
                //투수뷰인 경우
                int gab = (manager.pitcher.pitchHand == CPlayer._RIGHTHAND ? 40 : -5);
                timerOrigin.transform.localPosition = new Vector3(gab, 140, 0);
            }
            else
            {
                //타자뷰인 경우
                timerOrigin.transform.localPosition = new Vector3(0, 0+30, 0);
            }

            missSpr.gameObject.SetActive(false);
            perfectLight.GetComponent<Renderer>().enabled = false;

            int staminaLoss = (int)(5 * (100 - manager.pitcher.pPitcher.getCurrentStamina()));
            int control = (1100 - staminaLoss);            
            //Debug.Log("control = " + control + "     ===>>staminaLoss = " + staminaLoss);
            //setGauge(control - staminaLoss);

            badX = -48;
            normalX = 49;
            goodX = 64;
            perfectX = 74;

            curPosX = InitPos;
            initDX = FinalPos * PitchingMechanism.USER_CONTROL_SPEED * (800.0f / (float)control);  //initDX = FinalPos * 2.0f * (800.0f / (float)control);
            curDX = initDX;
            bTimerOn = true;
            curSpr.SetActive(true);
        }

        public void releaseTimer()
        {
            ////Debug.Log("========================>>releaseTimer");
            bTimerOn = false;
            checkPitchValue();

            StartCoroutine(cursorFlip());
            StartCoroutine(release());
        }

        private IEnumerator release()
        {
            //타이머 무효처리
            IngameUI.GetScoreBoard().SetPitchTimerActive(false); //PVP모드에서 피치 타이머

            if (value == UserControlValue.Perfect)
            {
                for (int i = 0; i < 8; i++)
                {
                    yield return new WaitForSeconds(0.10f);
                    perfectLight.GetComponent<Renderer>().enabled = !perfectLight.GetComponent<Renderer>().enabled;
                    //perfectLightBack.GetComponent<Renderer>().enabled = !perfectLightBack.GetComponent<Renderer>().enabled;
                }
            }
            else
            {
                if (value == UserControlValue.Miss)
                {
                    missSpr.gameObject.SetActive(true);
                    UITweener tween = missSpr.GetComponent<UITweener>();
                    tween.ResetToBeginning();
                    tween.PlayForward();
                }
                yield return new WaitForSeconds(0.50f);
            }
            
            cursor.transform.localPosition = new Vector3(0, 0, -0.1f);
            timerObj.SetActive(false);

            /*if (Mode.bPvpMode == true)
            {
                PvpManager.GetInstance().SendPitchingInfo();
                yield return new WaitForSeconds(1.0f); //투수쪽 동기화 테스트시 비활성화(PVP테스트_반드시_복구)
                manager.pitcher.startPitchingAnim();
            }*/
            if (Mode.bPvpMode433 == true)
            {
                pvpmanager.Get().SendPitchInfo(manager.pitcher);//
                //yield return new WaitForSeconds(1.0f); //투수쪽 동기화 테스트시 비활성화(PVP테스트_반드시_복구)
                //manager.pitcher.startPitchingAnim();
            }
            else
            {
                manager.pitcher.startPitchingAnim();
            }
            _active.SetActive(false);
            //deActive(true, 8);
        }

        private void timerCursorFrame()
        {
            
            if (curPosX < InitPos || curPosX > FinalPos)
            {
                curDX = initDX;
                curPosX = InitPos;
            }/*
            else if (curPosX > FinalPos)
            {
                curDX = -88;
                curPosX = FinalPos;
            }*/

            float addGab = IngameUI.GetPitchUI().size * 0.08f;
            //Debug.Log("===================>> addGab = " + addGab);

            curPosX += (curDX * Time.deltaTime);
            //curDX += (curDX > 0 ? 4 : -4);
            curDX += (curDX > 0 ? 1 : -1) * (4 + addGab);

            curSpr.transform.localPosition = new Vector3(curPosX, 3, -2);
        }

        private UserControlValue value;
        private void checkPitchValue()
        {
            ////Debug.Log("===============================>>> curPosX = " + curPosX);
            if (curPosX < badX)
            {
                value = UserControlValue.Bad;
                lightNum = 0;
            }
            else if (curPosX < normalX)
            {
                value = UserControlValue.Normal;
                lightNum = 0;
            }
            else if (curPosX < goodX)
            {
                value = UserControlValue.Good;
                lightNum = 1;
            }
            else if (curPosX < perfectX)
            {                
                value = UserControlValue.Perfect;
                lightNum = 2;
            }
            else
            {
                value = UserControlValue.Miss;                
                lightNum = 3;
            }
            ////Debug.Log("===============================>>> value = " + value);
            manager.pitcher.setUserControlValue2(value);
        }

        
        private IEnumerator cursorFlip()
        {
            //yield return new WaitForSeconds(0.3f);
            bool bSetActive = false;
            for (int i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(0.05f);
                curSpr.SetActive(bSetActive);
                bSetActive = !bSetActive;
            }
            curSpr.SetActive(false);
        }


        /// <summary>
        /// 개선 피칭
        /// </summary>
        public void releasePitching()
        {
            bTimerOn = false;
            ////Debug.Log("========================>>releaseTimer");
            /*float scaleValue = IngameUI.GetPitchUI().timerObj.release();

            if(scaleValue < 0.1f) value = UserControlValue.Perfect;
            else if (scaleValue < 0.2f) value = UserControlValue.Good;
            else if (scaleValue < 0.35f) value = UserControlValue.Normal;
            else if (scaleValue < 0.6f) value = UserControlValue.Bad;
            else value = UserControlValue.Miss;*/

            //timerObj.SetActive(false);
            checkPitchValue();
            //value = UserControlValue.Perfect;
            //Debug.Log("========================>>scaleValue = " + scaleValue+"         value = " + value);
            //manager.pitcher.setUserControlValue2(value);

            StartCoroutine(cursorFlip());
            StartCoroutine(release());
        }
    }


 

}
