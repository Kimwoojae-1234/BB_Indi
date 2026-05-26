using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIScoreBoard : MonoBehaviour
    {
        //
        public GameObject _active;

        //보드
        public UISprite homeLogo, awayLogo;
        public UILabel homeName, awayName;
        public UILabel homeScore, awayScore;

        //베이스 상태
        public UISprite[] baseOn;

        //이닝 정보
        public UILabel inningInfo;
        public UISprite topBottom;
        public UISprite[] ballCount;
        public UISprite[] strikeCount;
        public UISprite[] outCount;

        //공격 인디케이터
        public GameObject [] indicator;

        //
        public GameObject board, topUI;

        public GameObject autoButton;
        public GameObject waitObj;


        //타이머
        public GameObject Timer;
        public UILabel timerLabel;
        public UISprite timerGauge;

        //타자 타이머
        public UILabel batterTimer;


        //연속경기
        public GameObject consectiveGame;
             
     
        private BallPlayManager manager;
        private bool bBoardInit = false;


        private bool bNoAutoButton;

        // Use this for initialization
        void Start()
        {
            bBoardInit = false;
            //board = _active.transform.FindChild("board").gameObject;
            board.transform.localPosition = new Vector3(-900, 0, 0);
            waitObj.SetActive(false);

            bNoAutoButton = false;

            /*if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                bNoAutoButton = true;
                Destroy(topUI);
            }
            else*/
            {
                if (Mode.gameMode == Mode.GamePlayMode.Pvp433 || 
                    Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
                {
                    bNoAutoButton = true;
                    Destroy(autoButton.gameObject);
                }
                topUI.transform.localPosition = new Vector3(700, 35, 0);

                if (Mode.gameMode != Mode.GamePlayMode.Pvp433)
                {
                    //타이머 obj 지움
                    Destroy(Timer.gameObject);
                }

                //연속경기 여부
                checkConsetiveGame();

            }

            batterTimer.gameObject.SetActive(false);
        }

       
        public void SetActive(bool bActive, bool bFade = false)
        {
            UIPanel panel = gameObject.GetComponent<UIPanel>();

            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                IngameUI.SetWalkOffActive(bActive);
                if (bActive == true)
                {
                    IngameUI.GetWalkOffUI().SetRound(manager.nineTwoRound);
                }
            }

            if (bActive == true)
            {
                TopUIActive(true);
                panel.alpha = 1.0f;
                _active.SetActive(true);


                //
                //bFade가 true가 되야 뿅하고 나타남
                
                if (bFade == false)
                {
                    board.transform.localPosition = new Vector3(-446, 0, 0);
                    
                    /*if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
                    {
                    }
                    else*/
                    {
                        if(bNoAutoButton == false) autoButton.SetActive(!Mode.bOnlyChanceMode);
                        topUI.transform.localPosition = new Vector3(480, 35, 0);                        
                    }
                }
                else
                {
                    UITweener tween1 = board.GetComponent<TweenPosition>();
                    tween1.ResetToBeginning();
                    tween1.PlayForward();

                    /*if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
                    {
                    }
                    else*/
                    {
                        if (bNoAutoButton == false) autoButton.SetActive(!Mode.bOnlyChanceMode);
                        UITweener tween2 = topUI.GetComponent<TweenPosition>();
                        tween2.ResetToBeginning();
                        tween2.PlayForward();
                    }
                }

            }
            else
            {
                if (bFade == false)
                {
                    board.transform.localPosition = new Vector3(-900, 0, 0);
                    /*if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
                    {
                    }
                    else*/
                    {
                        topUI.transform.localPosition = new Vector3(700, 35, 0);
                    }
                    _active.SetActive(false);
                }
                else
                {
                    StartCoroutine(fadeOut(panel));
                }
            }
        }

        private IEnumerator fadeOut(UIPanel panel)
        {
            float alpha = 1;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                alpha -= 0.1f;
                panel.alpha = alpha;

                if (alpha < 0)
                {
                    break;
                }
            }
            board.transform.localPosition = new Vector3(-900, 0, 0);
            /*if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
            }
            else*/
            {
                topUI.transform.localPosition = new Vector3(700, 35, 0);
            }
            _active.SetActive(false);

        }


        public void Init(BallPlayManager _manager)
        {
            this.manager = _manager;
            //팀정보
            Util.SetSpritePixelPerfect(homeLogo, "logo_" + SimulPlayerManager.homeTeamIndex);//homeLogo.spriteName = "logo_" + SimulPlayerManager.homeTeamIndex;
            Util.SetSpritePixelPerfect(awayLogo, "logo_" + SimulPlayerManager.awayTeamIndex);//awayLogo.spriteName = "logo_" + SimulPlayerManager.awayTeamIndex;
            homeName.text = SimulPlayerManager.strHomeTeam;
            awayName.text = SimulPlayerManager.strAwayTeam;

            //이닝정보
            inningInfo.text = "1";
            topBottom.spriteName = "scoreboard_top";

            
            //카운트 정보
            for (int i = 0; i < 3; i++) ballCount[i].spriteName = "scoreboard_round";
            for (int i = 0; i < 2; i++) strikeCount[i].spriteName = "scoreboard_round";
            for (int i = 0; i < 2; i++) outCount[i].spriteName = "scoreboard_round";

            //베이스정보
            for (int i = 0; i < 3; i++) baseOn[i].spriteName = "scoreboard_base";


        }

        public void BoardUpdate()
        {   
            //이닝 정보
            inningInfo.text = manager.nInningCount.ToString();
            topBottom.spriteName = manager.bTopInning ? "scoreboard_top" : "scoreboard_bottom";
            //indicator.transform.localPosition = new Vector3(-169.5f, (manager.bTopInning ? 1.6f : -44.3f), 0);

            indicator[0].gameObject.SetActive(manager.bTopInning);
            indicator[1].gameObject.SetActive(!manager.bTopInning);

            int homeIndex = manager.bMyHome ? 0 : 1;
            homeScore.text = (manager.nGameScore[homeIndex]).ToString();
            awayScore.text = (manager.nGameScore[1 - homeIndex]).ToString();

            //카운트 정보
            for (int i = 0; i < 3; i++)
            {
                ballCount[i].spriteName = (i < manager.nBallCount ? "scoreboard_ball" : "scoreboard_round");
            }
            for (int i = 0; i < 2; i++)
            {
                strikeCount[i].spriteName = (i < manager.nStrikeCount ? "scoreboard_strike" : "scoreboard_round");
            }
            for (int i = 0; i < 2; i++)
            {
                outCount[i].spriteName = (i < manager.nOutCount ? "scoreboard_out" : "scoreboard_round");
            }

            //베이스 정보
            for (int i = 0; i < 3; i++)
            {
                baseOn[i].spriteName = manager.field.run.bOnBase[i] ? "scoreboard_baseon" : "scoreboard_base";
            }

            IngameUI.GetPlayerInfo().SetPitchNum(manager.pitcher.pPitcher);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //탑 UI
        //////////////////////////////////////////////////////////////////////////////////////

        public void setPause()
        {
            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {                
                //Mode.bPauseGame = true;
                //Util.Load("MainGame/prefabs/gameUI/QuitPopupPrefab", transform.parent, Vector3.zero).GetComponent<UIQuit>().init(manager);
                manager.pitcher.setPause();
            }
            else
            {
                if (Mode.PlayTypeFlag == Mode.ModeFlag.Manual)
                {
                    if (manager.pitcher.setPause() == true)
                    {
                        if (IngameUI.GetPauseUI().SetPause(manager) == true)
                        {
                            Mode.bPauseGame = true;
                        }
                    }
                }
            }
        }


        public void setGameSpeedControl()
        {
            if (Mode.bSimulationQuickPlay == false)
            {
                if (Mode.PlayTypeFlag == Mode.ModeFlag.Manual)
                {
                    ////Debug.Log("=======>>>자동으로 전환");
                    Mode.PlayTypeFlag = Mode.ModeFlag.Auto;
                    //Debug_UI.SetNotice(true);
                    Mode.bSiumlSetting = true;
                    setWait(true);
#if !_Test_Local
                    // DISABLED_MGRS: Mgrs.LocalManager.SaveLastAutoGame("auto");
#endif
                }
            }
        }

        public void setWait(bool bActive)
        {
            if (bNoAutoButton == false)
            {
                waitObj.SetActive(bActive);
                autoButton.GetComponent<Collider>().enabled = !bActive;
            }
        }


        public void TopUIActive(bool bActive)
        {
            /*if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {                
            }
            else*/
            {
                topUI.SetActive(bActive);
            }
        }

        private bool bTimerActive;
        private IEnumerator timerSetting;
        /// <summary>
        /// PVP모드에서 자동 피치 타이머 게이지
        /// </summary>
        /// <param name="bActive"></param>
        public void SetPitchTimerActive(bool bActive)
        {
            if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                if (manager.bMyTurn == false)
                { 
                    if (Timer != null)
                    {
                        if (bActive == true)
                        {
                            bTimerActive = true;
                            timerSetting = timerStart();
                            StartCoroutine(timerSetting);
                        }
                        else
                        {
                            bTimerActive = false;
                            StopCoroutine(timerSetting);
                            TweenPosition.Begin(Timer, 0.2f, new Vector3(0, (155*1.5f), 0));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 피치 타이머 작동
        /// </summary>
        /// <returns></returns>
        private IEnumerator timerStart()
        {
            manager.pitcher.bPitchTimerOn = false;
            TweenPosition.Begin(Timer, 0.2f, new Vector3(0, 88, 0));
            yield return new WaitForSeconds(0.2f);

            float curTime = 8.0f;
            while (curTime > 0)
            {
                curTime -= Time.deltaTime;
                int w = (int)(146.0f * curTime / 8.0f);
                timerGauge.SetDimensions(w, 5);
                if (curTime < 0) curTime = 0;
                timerLabel.text = "TIMER   " + string.Format("{0:F2}", curTime); 
                yield return new WaitForEndOfFrame();
            }

            TweenPosition.Begin(Timer, 0.2f, new Vector3(0, 155*1.5f, 0));

            if (manager.bMyTurn == false)
            {
                if (bTimerActive == true)
                {
                    manager.pitcher.bPitchTimerOn = true;

                    IngameUI.GetEmoticonChatting().forceChatDisable(true);
                    
                    bTimerActive = false;
                    bool bBallSelect = IngameUI.GetPitchingSelect().autoBallSelect();
                    if (bBallSelect == true)
                    {
                        yield return new WaitForSeconds(1.0f);
                        IngameUI.GetPitchingSelect().SetActive(false);
                    }
                    manager.pitcher.aiCourseSelect();   //자동 코스 셀렉트
                    //PvpManager.GetInstance().SendPitchingInfo();
                    manager.pitcher.getSign();//0.5f);*/
                    StartCoroutine(manager.setBattingViewState(0.01f));
                    IngameUI.GetPitchUI().SetActive(false);
                    ControlPitchingUI.SetActive(false, manager);
                    ControlPitchingUI.TimerDeactive();

                    //인디케이터 릴리즈
                    if (Mode.cameraView == CameraView.PitcherCenter)
                    {
                        manager.pitchPv.pitchOriginPv.releaseIndicator();
                    }
                    else
                    {
                        manager.pitch.pitchOrigin.releaseIndicator();
                    }

                    pvpmanager.Get().SendPitchInfo(manager.pitcher);

                    //yield return new WaitForSeconds(1.0f);

                    //manager.pitcher.startPitchingAnim();
                }

            }
        }


        private bool bBatterTimerActive;
        private IEnumerator battertimerSetting;
        private int BatterWait = 8;
        private bool bSelectPitch = false;

        public void SetBatterTimer(bool bActive)
        {
            if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                if (manager.bMyTurn == true)
                {
                    Debug_UI.SetNetwork(false);
                    if (batterTimer.gameObject != null)
                    {
                        if (bActive == true)
                        {
                            batterTimer.gameObject.SetActive(true);
                            bBatterTimerActive = true;
                            battertimerSetting = batterTimerStart();
                            StartCoroutine(battertimerSetting);
                        }
                        else
                        {
                            bBatterTimerActive = false;
                            StopCoroutine(battertimerSetting);
                            batterTimer.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        public void selectPitch()
        {
            batterTimer.text = "[ff0000]상대가 코스를 선택 중입니다 : " + BatterWait + "초";
            bSelectPitch = true;
        }

        private IEnumerator batterTimerStart()
        {
            BatterWait = 8;
            bSelectPitch = false;
            batterTimer.text = "[ffffff]상대가 투구선택을 하는 중입니다 : 8초";

            while (BatterWait > 0)
            {
                yield return new WaitForSeconds(1.0f);
                BatterWait--;
                if(bSelectPitch == false)
                {
                    batterTimer.text = "[ffffff]상대가 투구선택을 하는 중입니다 : " + BatterWait + "초";
                }
                else
                {
                    batterTimer.text = "[ff0000]상대가 코스를 선택 중입니다 : " + BatterWait + "초";
                }
            }
            yield return new WaitForSeconds(1.0f);
            batterTimer.gameObject.SetActive(false);
        }



        public void pushConsectiveQuit()
        {
            // DISABLED_MGRS: Mgrs.userData.SetUserGameMode(DefineEnum.EGameMode.Season);
            consectiveGame.SetActive(false);
        }


        public void checkConsetiveGame()
        {
            if (consectiveGame != null)
            {
                
                {
                    consectiveGame.SetActive(false);
                }
            }
        }

    }
}
