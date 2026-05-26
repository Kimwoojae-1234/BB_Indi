using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class ControlBattingUI : MonoBehaviour
    {
        public const float CURSOR_ZONE_MAX_X = 85 * 0.6f; // 65;// 82; //스트존 (피칭UI)
        public const float CURSOR_ZONE_MAX_Y = 95 * 0.6f; //73;//92; //스트존 (피칭UI)

        public GameObject _active;

        //인스턴스
        private static ControlBattingUI Instance_;
        public GameObject releaseType;//, pushType;


        private BallPlayManager manager;
        //릴리즈
        public GameObject cursor;
        public tk2dSprite cursorSpr;
        tk2dUIDragItem drag;
        tk2dUIItem pos;

        bool bCursorInvisible, curInvisible;
        bool bCursorMove;
        float cursorX, cursorY;
        float initX, initY;
        float uiX, uiY;


        public tk2dSprite bunt, power;
        public GameObject powerEffect, buntEffect;


        private static float screenW = (720 * Screen.width) / Screen.height;
        private static float screenWC = screenW / 2;


        void Awake()
        {
            Instance_ = this;
            bCursorMove = false;
            bPauseSetting = false;
        }

        void OnDestroy()
        {
            Instance_ = null;
        }

        // Use this for initialization
        void Start()
        {
            drag = cursor.GetComponent<tk2dUIDragItem>();
            pos = cursor.GetComponent<tk2dUIItem>();
            uiX = screenWC + transform.localPosition.x; //640 + transform.localPosition.x;
            uiY = 360 + transform.localPosition.y;

            bCursorInvisible = false;
            curInvisible = false;

            cursorSpr = cursor.transform.Find("spr").GetComponent<tk2dSprite>();
        }

        // Update is called once per frame
        void Update()
        {
            
            if (bCursorMove == true)
            {
                cursorFrame();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //외부 호출용 static함수
        //////////////////////////////////////////////////////////////////////////////////////
        public static void SetActive(bool bActive, BallPlayManager _manager)
        {            
            if (bActive == true)
            {
                if (Mode.bPvpMode433 == true) Debug_UI.SetNetwork(true);
                Instance_._active.SetActive(true);
                Instance_.manager = _manager;
                Instance_.activeType();
            }
            else
            {
                Instance_._active.SetActive(false);
            }
        }


        public static void SetPowerAndBuntUIActive(bool bActive)
        {
            Instance_.setPowerAndBuntUIActive(bActive);
        }

        public static void SetSqueezeButtonOn()
        {
            Instance_.setBuntButtonActive(true);
        }


        public static void CheckPauseState(bool bPause)
        {
            Instance_.checkPauseState(bPause);
        }
        //////////////////////////////////////////////////////////////////////////////////////
        //버튼 메세지
        //////////////////////////////////////////////////////////////////////////////////////

        private bool bButtonAvail;
        private bool bPower;

        //오른쪽 번트 (푸시, 릴리즈 공통)
        public void setPowerButton()
        {
            if (bButtonAvail == true)
            {
                manager.batter.bGangTa = !manager.batter.bGangTa;
                //bPower = manager.batter.bGangTa;
                //power.spriteId = power.GetSpriteIdByName("power_bt" + (bPower ? 1 : 0));
                manager.batter.zoneUI.setGangtaCursor();
                powerEffect.SetActive(manager.batter.bGangTa);

                if (Mode.bPvpMode == true)
                {
                    PvpManager.GetInstance().SendPowerBattingInfo();
                }
            }
            
        }

        //왼쪽 번트 (푸시, 릴리즈 공통)
        public void setBunt()
        {
            if (Mode.gameMode == Mode.GamePlayMode.Pvp || Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                //번트시 강제 종료(토글창은 안닫음)
                IngameUI.GetEmoticonChatting().forceChatDisable();
            }

            //bunt.spriteId = bunt.GetSpriteIdByName("batting_bunt2");
            manager.batter.buntAnim();
            manager.batter.buntDir = MyMath.Half() ? -1 : 1;
            buntEffect.SetActive(true);
        }

        //배팅 UI에서 번트자세 취소
        public void setBuntRelease()
        {
            //bunt.spriteId = bunt.GetSpriteIdByName("batting_bunt1");
            if (manager.pitcher.pState == PitcherState._PITCHING || manager.pitcher.pState == PitcherState._RELEASE)
            {
                manager.batter.lookingAnim(true, true);
            }
            else
            {
                manager.batter.bBunt = false;
                manager.batter.readyAnim(false);
            }
            buntEffect.SetActive(false);
        }


        //배팅 UI에서 스윙 이벤트 버튼(푸스 릴리즈 공통)
        public void swing()
        {
            if (manager.batter.bSwing == false && manager.batter.bForcedSwingPrevent == false)
            {
                if (manager.pitcher.pState == PitcherState._RELEASE ||
                   manager.pitcher.pState == PitcherState._FINISH)
                {
                    SetActive(false, manager);
                    if (manager.pitcher.hitByPitchStep != 0)
                    {
                        return;
                    }
                    if (manager.batterSkillFlag == SkillFlag.FalconEye)
                    {
                        //CameraManager.SetBlur2(0,false);
                        Time.timeScale = 1.0f;
                    }
                    StartCoroutine(manager.batter.startSwingAnim());
                    bCursorMove = false;
                }
            }
        }


        ///////////////////////////////////////////////////////////////////////////

        



        //릴리즈 타입 초기화 (릴리즈에서 투구 시작및 커서 초기화)
        bool bPadInit;
        bool bAuto = false;
        public void releaseTypeInit()
        {
            if (Mode.gameMode == Mode.GamePlayMode.Pvp ||
                Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                //타격시 강제 종료(토글창은 안닫음)
                IngameUI.GetEmoticonChatting().forceChatDisable();
            }
            releaseTypeInitAuto();
        }

        public void releaseTypeInitAuto()
        {
            if (bPadInit == false)
            {
                if (manager.pitcher.bRelease == false)
                {
                    if (manager.pitcher.pState == PitcherState._GET_SIGN)
                    {                        
                        //setButtonActive(false);
                        if(Mode.bPvpMode433 == true)
                        {

                        }
                        else if (Mode.bPvpMode == true)
                        {
                            PvpManager.GetInstance().WaitPitchingInfo();                            
                        }
                        else
                        {
                            //if (Mode.bPowerfulType == true)
                            {
                                StartCoroutine(manager.pitcher.startPichingAnim3());
                            }
                            /*else
                            {
                                manager.pitcher.startPichingAnim2();
                            }*/
                        }

                        if (bAuto == true)
                        {
                            initX = 0;
                            initY = 0;
                            bCursorMove = true;
                        }
                        else
                        {
                            setDown();
                        }
                    }
                }
                bPadInit = true;
            }
        }

        private IEnumerator StartPitchingAutomatically()
        {
            yield return new WaitForSeconds(1.0f);

            bAuto = true;
            releaseTypeInitAuto();
        }


        private void activeType()
        {
        /*    if (Mode.batControlType == BatControlType.PushType)
            {
                releaseType.SetActive(false);
                pushType.SetActive(true);
            }
            else*/
            {
                setPowerAndBuntUIActive(true);
                setBuntButtonActive(false);

                bCursorInvisible = false;
                bAuto = false;
                bPadInit = false;
                bPower = manager.batter.bGangTa;
                //bunt.spriteId = bunt.GetSpriteIdByName("batting_bunt1");
                buntEffect.SetActive(false);

                releaseType.SetActive(true);

                bCursorMove = false;
                manager.battingview.zoneUI.setBatCursorPos2(0, 0, CURSOR_ZONE_MAX_X, CURSOR_ZONE_MAX_Y);
                cursor.transform.localPosition = new Vector3(0, 0, -1);
                cursorSpr.gameObject.SetActive(true);

                //setButtonActive(true);

                if (manager.bMyTurn == true)
                {
                    initX = initY = 0;
                    StartCoroutine(StartPitchingAutomatically());
                }

            }
        }

        private void setPowerAndBuntUIActive(bool bActive)
        {
            bButtonAvail = bActive;
            if (bActive == true)
            {
                powerEffect.SetActive(manager.batter.bGangTa);
                power.color = new Color(1, 1, 1, 1);
            }
            else
            {
                StartCoroutine(powerAndBuntUIdeActive());
            }
        }

        private void setBuntButtonActive(bool bForced)
        {
            if (manager.field.run.bOnBase[FieldParm.THIRDBASE_INDEX] == false || bForced == true)
            {
                if (bForced == true)
                {
                    //번트 결과를 다시 체크해줘
                    manager.batter.recheckBuntResult();
                }
                bunt.gameObject.SetActive(true);
            }
            else
            {
                bunt.gameObject.SetActive(false);
            }
        }



        private IEnumerator powerAndBuntUIdeActive()
        {
            powerEffect.SetActive(false);
            float alpha = 1.0f;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                alpha -= 0.015f;
                power.color = new Color(1, 1, 1, alpha);
                if (alpha < 0)
                {
                    break;
                }
            }
        }

        public void setDown()
        {
            
            float sx = Screen.width;
            float sy = Screen.height;
            float rx = pos.Touch.position.x * screenW / sx; //pos.Touch.position.x *1280 / sx;
            float ry = pos.Touch.position.y *720 / sy;
            initX = (rx - uiX) - 463; // 420;// 410;
            initY = (ry - uiY) + 203; // 168;// 157;

            //////UnityEngine.//Debug.Log("========================InitX = " + initX);
            //////UnityEngine.//Debug.Log("========================initY = " + initY);
            
            bCursorMove = true;
            
        }

        //컨트롤 혹은 배트 커서 경계 설정 
        private void cursorFrame()
        {
            //if (cursorSpr.renderer.enabled == false) cursorSpr.renderer.enabled = true;
            
            if (Mode.bAutoPlay == false)
            {
                float x = cursor.transform.localPosition.x + initX;
                float y = cursor.transform.localPosition.y + initY;

                bCursorInvisible = false;

                if (x >= 85)//119)
                {
                    bCursorInvisible = true;
                    x = 85;// 119;
                }
                else if (x <= -85)//119)
                {
                    bCursorInvisible = true;
                    x = -85;//119;
                }

                if (y >= 95)//125)
                {
                    bCursorInvisible = true;
                    y = 95;// 125;
                }
                else if (y <= -95)//125)
                {
                    bCursorInvisible = true;
                    y = -95;//125;
                }

                if (manager.bMyTurn == true)
                {
                    //////UnityEngine.//Debug.Log("=================================>>X = " + x);
                    //////UnityEngine.//Debug.Log("=================================>>Y = " + y);
                    manager.battingview.zoneUI.setBatCursorPos2(x, y, CURSOR_ZONE_MAX_X, CURSOR_ZONE_MAX_Y);

                    if (curInvisible != bCursorInvisible)
                    {
                        cursorSpr.gameObject.SetActive(!bCursorInvisible);
                        curInvisible = bCursorInvisible;
                    }

                }
            }
            
        }

        /*
        private void setButtonActive(bool bActive)
        {
            
            bunt.color = new Color(1, 1, 1, bActive?1:0.5f);
            power.color = new Color(1, 1, 1, bActive?1:0.5f);

            bunt.GetComponent<BoxCollider2D>().enabled = bActive;
            power.GetComponent<BoxCollider2D>().enabled = bActive;
        }*/


        private bool bPauseSetting = false;
        private void checkPauseState(bool bPause)
        {
            if (bPause == true)
            {
                if (_active.activeSelf == true)
                {
                    bPauseSetting = true;
                    _active.SetActive(false);
                }
            }
            else
            {
                if (bPauseSetting == true)
                {
                    _active.SetActive(true);
                }
            }
        }

    }
   
}
