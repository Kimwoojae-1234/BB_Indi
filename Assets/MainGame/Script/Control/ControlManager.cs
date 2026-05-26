using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class ControlManager : MonoBehaviour
    {
        //카메라 오브젝트
        public GameObject _camera; 

        //인스턴스
        private static ControlManager Instance_;
        //
        private BallPlayManager manager;
        private Batter batter;
        private Pitcher pitcher;
        private Zone zoneUI;

        //
        private ControlStep uiStep;

        
        


        public static bool bInitPopup;

        void Awake()
        {
            Instance_ = this;
        }

        void OnDestroy()
        {
            Instance_ = null;
        }

        // Use this for initialization
        void Start()
        {
            uiStep = ControlStep.Info;
            bInitPopup = false; 
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        /*
        void OnMouseDown()
        {
            if (manager.playState == PlayState.PLAY_BATTING_VIEW_INFO)
            {
                if (uiStep == ControlStep.Info)
                {
                    infoGone();
                }
            }
        }*/

        /*
        void OnMouseDown()
        {
            if (manager.playState == PlayState.PLAY_FIELDING_VIEW)
            {
                ////Debug.Log("=======>터치 먹냐");
                manager.field.checkFieldEnd();
            }
        }*/

        //////////////////////////////////////////////////////////////////////////////////////
        //외부 호출용 static함수
        //////////////////////////////////////////////////////////////////////////////////////
        public static ControlManager GetInstance()
        {
            return Instance_;
        }
        
        public static void InitInstance(BallPlayManager manager)
        {
            Instance_.initInstance(manager);
        }

        public static void CameraEnable(bool bEnable)
        {
            Instance_._camera.SetActive(bEnable); 
        }

        public static void ResetUI()
        {
            Instance_.resetUI();
        }

        public static void SetInfoUI()
        {
            Instance_.setInfoUI();
        }

        public static void SetReadyUI(float startZoom, bool bReady2 = true)
        {
            Instance_.setReadyUI(startZoom, bReady2);
        }

        public static void SetReadyUI2()
        {
            Instance_.setReadyUI2();
        }

        public static void SetBattingUI()
        {
            Instance_.setBattingUI();
        }


        public static void EraseBattingUI()
        {
            IngameUI.GetScoreBoard().SetActive(false, true);
            IngameUI.GetPlayerInfo().SetActive(false, true);
        }


        public static void EraseFieldUI(bool bNewBatter, bool bNewPitcher)
        {
            IngameUI.GetScoreShow().DeActive();
            IngameUI.GetScoreBoard().SetActive(true, bNewBatter);

            if (bNewBatter == false && bNewPitcher == false)
            {
                IngameUI.GetPlayerInfo().Active();
            }
            else
            {
                IngameUI.GetPlayerInfo().SetActive(true, bNewBatter, bNewPitcher);
            }
        }

        public static void InfoGone()
        {
            Instance_.infoGone();
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //private 함수
        //////////////////////////////////////////////////////////////////////////////////////
        private void initInstance(BallPlayManager _manager)
        {
            manager = _manager;
            batter = manager.batter;
            pitcher = manager.pitcher;
            zoneUI = manager.battingview.zoneUI;
        }


        private void resetUI()
        {
            ControlBattingUI.SetActive(false, manager);
            IngameUI.GetPitchingSelect().SetActive(false);//

            //pitchingUI.GetComponent<pitchControlUI>()._active.SetActive(false);// pitchingUI.SetActive(false);            
        }

        private void setInfoUI()
        {
            //manager.batter.readyAnim(true);
            if (manager.bInningChange == true)
            {                
                CameraManager.ChangeCamera(BallPlayManager._BATTINGVIEW, BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY + 360);
                manager.bInningChange = false;
            }

            IngameUI.GetControlRunner().SetActive(false, false);
            IngameUI.GetPlayerInfo().SetActive(true, false, false);     

            
            uiStep = ControlStep.Info;
            zoneUI.setZone(true, false, false); //피칭커서 세팅X, 스트라이크존type2 강제세팅X
            pitcher.setPitcherReadyState();
            
            ControlBattingUI.SetActive(false, manager);
            _camera.SetActive(true);
            
        }

        /*
        private IEnumerator setInfoGone(float delay)
        {
            yield return new WaitForSeconds(delay); 김상수 16 
            if (uiStep == ControlStep.Info)
            {
                //////UnityEngine.//Debug.Log("========================>>정상적인 사라짐");
                infoGone();
            }
            else
            {
                //////UnityEngine.//Debug.Log("========================>>스킵에 의해 사라짐");
            }
        }*/

        private void infoGone()
        {
            if (Mode.bAutoPlay == false)
            {
                IngameUI.GetControlRunner().SetActive(true, true);
            }         
            float startZoom = 0.75f;
            setReadyUI(startZoom, false);
            manager.playState = PlayState.PLAY_BATTING_VIEW_READY;
            manager.bReadyFinish = true;
        }

        //준비 상태 UI 셋팅
        private void setReadyUI(float startZoom, bool bReady2)
        {
            uiStep = ControlStep.Ready;
            if (Mode.bAutoPlay == true)
            {
                //자동 플레이시
                //startZoom = 1.0f; //오토줌 고친곳
            }

            zoneUI.setZone(true, false, false); //피칭커서 세팅X, 스트라이크존type2 강제세팅X
            if (Mode.bAutoPlay == false)
            {
                pitcher.setPitcherReadyState();
            }
            manager.bReadyFinish = false;
            manager.bReadyFinish2 = false;
            manager.bReadyFinish3 = false;


            if (Mode.cameraView == CameraView.BatterLow)
            {
                if (startZoom > 0.9f) startZoom = 0.9f;
            }
            else
            {
                startZoom = 1;
            }

            manager.readyZoom = startZoom;// 0.7f;            
            if (bReady2 == true) setReadyUI2();
            _camera.SetActive(true);

            
        }

        private void setReadyUI2()
        {
            if (manager.bMyTurn == true)
            {
                //레디 없는 버전
                manager.bReadyFinish2 = true;
                //if (Mode.bPowerfulType == true)
                {
                    //처리됨
                    manager.pitch.pitchOrigin.setFirstPos(); //투수뷰에서는 필요없는 
                }
                ControlBattingUI.SetActive(true, manager);

                //레디 있는 버전
                //ControlBattingUI.SetActive(false, manager);                
                //manager.batter.setBatterSkillAnim(true);

                if(Mode.bPvpMode433 == true)
                {
                    pvpmanager.Get().SendBatterSync(manager);
                }
            }
            else
            {
                /*if (Mode.bPvpMode == true && manager.field.bUpdateStealOrPickOff == false)
                {
                    //견제나 도루 후 이곳으로 들어오지 않는다
                    PvpManager.GetInstance().WaitNewBatterInfo();
                }*/
                if (Mode.bPvpMode433 == true && manager.field.bUpdateStealOrPickOff == false)
                {
                    //견제나 도루 후 이곳으로 들어오지 않는다
                }
                else
                {                    
                    IngameUI.GetPitchingSelect().SetActive(true);
                    if (Mode.cameraView == CameraView.PitcherCenter)
                    {
                        manager.pitcher.catcher.setSign();
                    }
                    manager.field.bUpdateStealOrPickOff = false;
                }
            }
            
            //setRunningControlUI();
        }


        private void setBattingUI()
        {
            zoneUI.setZone(true, true, true); //피칭커서 세팅O, 스트라이크존type2 강제세팅O
            
            /*if (batter.resetTrace == true)
            {
                zoneUI.resetTrace();
                batter.resetTrace = false;
            }*/

            //zoneUI.setTrace(false, false);
            if (manager.bMyTurn == true)
            {
                ControlBattingUI.SetActive(true, manager);
                //manager.batter.setBatterSkillAnim(false);
            }
            else
            {
                ControlPitchingUI.SetActive(true, manager, true); //pitchingUI.GetComponent<pitchControlUI>().init(bPowerPitch);// pitchingUI.SetActive(true);
            }
        }



    }
}