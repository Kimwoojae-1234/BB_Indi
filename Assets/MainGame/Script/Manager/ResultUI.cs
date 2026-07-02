#define _EFFECT_TEST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class ResultUI : MonoBehaviour
    {
        private static ResultUI Instance_;

#if _EFFECT_TEST
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) == true)
            {
                resultMain.setEffectStart(win, leftTitle, rightTitle);
            }
        }
#endif
        

        void Awake()
        {
            Instance_ = this;
        }

        void OnDestroy()
        {
            Instance_ = null;
        }

        public enum ResultStep{
            Main,
            Grow,
            Reward,
            Stat,
            OtherTeam,            
            WalkOff,
            OutGame
        }

        public enum ResultType
        {
            Season,
            Ranking,
            Race,
            WalkOff,
            LiveMatch
        }


        public UIResultMain resultMain;
        public UIResultGrow resultGrow;
        public UIResultReward resultReward;
        public UIResultPlayerRecord playerRecord;
        public UIResultOtherTeam otherResult;
        public UIWalkOffResult walkOffResult;
        public UILeagueUpDown liveLeagueUpDown;

        //
        //public UI_PopupConfirm confirmPopup;


        public ResultType resultType;

        //뒷배경
        public GameObject back, front;
        public GameObject leftTitle, rightTitle;
        public GameObject win, draw, lose;
        public GameObject statButonObj, otherResultButton, getCoinObj, stringObj;

        //
        //public GameObject winBack, normalBack;
        
        //
        private SeasonGameEndInfo resultInfo;
        //private RankedPlayGameEndInfo rankResultInfo;
        private LivePlayGameEndInfo liveResultInfo;
        private RacePlayEndInfo raceResultInfo;
        private WalkoffPlayEndInfo walkoffResultInfo;
        private BallPlayManager manager;

        //스텝
        private ResultStep curStep, lastStep;

        private bool otherResultInit = false;
        private bool playerStatInit = false;

        private bool bFinishButton = false;
        
        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="_manager"></param>
        public void Init(BallPlayManager _manager)
        {
            bFinishButton = false;
            //Application.targetFrameRate = 45;
            this.manager = _manager;
            this.curStep = ResultStep.Main;
            otherResultInit = false;
            playerStatInit = false;
            
            //confirmPopup.gameObject.SetActive(false);


            TweenAlpha.Begin(front, 0.3f, 1);

#if _Test_Local
            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                // DISABLED_MGRS: Mgrs.userData.SetUserGameMode(DefineEnum.EGameMode.Walkoff);
                walkOffResult.Init(manager);
                TweenAlpha.Begin(walkOffResult.gameObject, 1, 1);
            }
            else
            {
                // DISABLED_MGRS: Mgrs.userData.SetUserGameMode(DefineEnum.EGameMode.Season);
                resultMain.initSeason(manager);
                TweenAlpha.Begin(resultMain.gameObject, 1, 1);
            }
            setBack();
#else       
            
            //결과 보내기
            sendResult();
            
            /*
            //네트워크 에러테스트 
            sendRankGameResult();
            Invoke("networkErrorPopup", 2);*/
#endif
        }

        private void sendResult()
        {
            DefineEnum.EGameMode mode = DefineEnum.EGameMode.Season;
            if (mode == DefineEnum.EGameMode.SeasonInstant)
            {
                GetComponent<BoxCollider>().enabled = true;
                sendSeasonGameResult();
            }
            else if (mode == DefineEnum.EGameMode.LeagueRaceInstant)
            {
                GetComponent<BoxCollider>().enabled = true;
                sendRaceGameResult();
            }
            else
            {
                GetComponent<BoxCollider>().enabled = false;
                if (mode == DefineEnum.EGameMode.Season || mode == DefineEnum.EGameMode.SeasonConsecutive)
                {
                    sendSeasonGameResult();
                }
                /*else if (mode == DefineEnum.EGameMode.Rank)
                {
                    sendRankGameResult();
                }*/
                else if (mode == DefineEnum.EGameMode.LiveMatch)
                {
                    sendLiveMatchGameResult();
                }
                else if (mode == DefineEnum.EGameMode.LeagueRace)
                {
                    sendRaceGameResult();
                }
                else if (mode == DefineEnum.EGameMode.Walkoff)
                {
                    curStep = ResultStep.WalkOff;
                    sendWalkOffResult();
                }
            }
        }

        

        /// <summary>
        /// 시즌 인포 인스턴스
        /// </summary>
        /// <returns></returns>
        public static SeasonGameEndInfo GetSeasonEndInfo()
        {
            return Instance_.resultInfo;
        }

        /// <summary>
        /// 랭크 인포 인스턴스
        /// </summary>
        /// <returns></returns>
        /*public static RankedPlayGameEndInfo GetRankPlayEndInfo()
        {
            return Instance_.rankResultInfo;
        }*/

        /// <summary>
        /// 쟁탈 결과 인스턴스
        /// </summary>
        /// <returns></returns>
        public static RacePlayEndInfo GetRaceEndInfo()
        {
            return Instance_.raceResultInfo;
        }

        /// <summary>
        /// 쟁탈 결과 인스턴스
        /// </summary>
        /// <returns></returns>
        public static LivePlayGameEndInfo GetLiveEndInfo()
        {
            return Instance_.liveResultInfo;
        }


        /// <summary>
        /// 9회투아웃 인포 인스턴스
        /// </summary>
        /// <returns></returns>
        public static WalkoffPlayEndInfo GetWalkoffEndInfo()
        {
            return Instance_.walkoffResultInfo;
        }

        /// <summary>
        /// 게임 매니저 인스턴스
        /// </summary>
        /// <returns></returns>
        public static BallPlayManager GetGameManager()
        {
            return Instance_.manager;
        }

        /// <summary>
        /// 팝업창으로 부터 현재 상태로 돌아옴
        /// </summary>
        public static void BackFromPopup()
        {
            Instance_.curStep = Instance_.lastStep;
        }


        public static void GotoUpgrade()
        {
            Instance_.gotoUpgrade();
        }


        private void callBack()
        {
            //Debug.Log("===================callBack");
            // DISABLED_MGRS: Mgrs.ManagerSupervise(false);
            // DISABLED_MGRS: Mgrs.userData.UserLobbyReason = UserData.EReason.OutGame_Lobby;            
            SkillEffectDisplayManager.Destroy();
            Destroy(GameObject.FindWithTag("SIMUL_TAG").gameObject);
            // DISABLED_MGRS: Mgrs.SceneLoad.LoadScene(SceneID.Lobby);       
        }


        private void networkErrorPopup()
        {
            //gameObject.SetActive(false);
            DefineEnum.EGameMode mode = DefineEnum.EGameMode.Season;
            if (mode == DefineEnum.EGameMode.SeasonInstant || mode == DefineEnum.EGameMode.LeagueRaceInstant)
            {
                
            }
            else
            {
                // DISABLED_MGRS: Mgrs.ManagerSupervise(false);
            }
            //confirmPopup.gameObject.SetActive(true);
            //confirmPopup.SetPopup_OneBtn("서버 연결 실패", "서버와의 연결을 실패하였습니다.", "확인", callBack);
        }
        
        
        /// <summary>
        /// 시즌게임 결과를 서버에 전송
        /// </summary>
        private void sendSeasonGameResult()
        {
            resultType = ResultType.Season;
            
            //UnityEngine.Debug.Log("=====================================>>>sendGameResult");           
            //서버에 전송
            List<SeasonGameResult> gameResults = SimulPlayerManager.GetGameResults();
            string summary = SimulPlayerManager.GetGameSummary();
            // DISABLED_MGRS: Mgrs.RestService.SeasonPlay_Results(gameResults, summary,
                // DISABLED_MGRS_CONT: (SeasonGameEndInfo info) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("sendTestResult Success");
                    //bNetworkComplete = true;
                    // DISABLED_MGRS_CONT: resultInfo = info;                    
                    // DISABLED_MGRS_CONT: resultMain.initSeason(manager);
                    // DISABLED_MGRS_CONT: setBack();
                    
                    //TweenAlpha.Begin(resultMain.gameObject, 1, 1);
                    //결과전송 끝
                // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                // DISABLED_MGRS_CONT: {
                    //Debug.Log("sendTestResult Fail : " + er.code);
                    //결과전송 에러 팝업
                    // DISABLED_MGRS_CONT: networkErrorPopup();
                // DISABLED_MGRS_CONT: }));
            
        }

        /// <summary>
        /// 랭크게임 결과를 서버에 전송
        /// </summary>
        /*private void sendRankGameResult()
        {
            resultType = ResultType.Ranking;
            RankedPlayGameResult gameResult = SimulPlayerManager.GetGameRankedPlayResults();
            // DISABLED_MGRS: Mgrs.RestService.RankedPlay_Result(gameResult,
                // DISABLED_MGRS_CONT: (RankedPlayGameEndInfo info) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("sendTestResult Success");
                    //bNetworkComplete = true;
                    // DISABLED_MGRS_CONT: rankResultInfo = info;
                    // DISABLED_MGRS_CONT: resultMain.initRank(manager);
                    // DISABLED_MGRS_CONT: TweenAlpha.Begin(resultMain.gameObject, 1, 1);
                    // DISABLED_MGRS_CONT: setBack();
                // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                // DISABLED_MGRS_CONT: {
                    //Debug.Log("sendTestResult Fail : " + er.code);
                    //결과전송 에러 팝업
                    // DISABLED_MGRS_CONT: networkErrorPopup();
                // DISABLED_MGRS_CONT: }));
        }*/


        private void sendLiveMatchGameResult()
        {
            resultType = ResultType.LiveMatch;
            LivePlayResult gameResult = SimulPlayerManager.GetLivePlayResults(manager.bMyHome);
            // DISABLED_MGRS: Mgrs.RestService.LivePlay_Result(gameResult, null,
                // DISABLED_MGRS_CONT: (LivePlayGameEndInfo info) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("sendTestResult Success");
                    //bNetworkComplete = true;
                    // DISABLED_MGRS_CONT: liveResultInfo = info;                    
                    // DISABLED_MGRS_CONT: resultMain.initLiveMath(manager);
                    // DISABLED_MGRS_CONT: setBack();                    
                // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                // DISABLED_MGRS_CONT: {
                    //Debug.Log("sendTestResult Fail : " + er.code);
                    //결과전송 에러 팝업
                    // DISABLED_MGRS_CONT: networkErrorPopup();
                // DISABLED_MGRS_CONT: }));

        }


        private void sendRaceGameResult()
        {
            resultType = ResultType.Race;
            RacePlayResult gameResult = SimulPlayerManager.GetGameRaceResults(manager.bMyHome);
            // DISABLED_MGRS: Mgrs.RestService.RacePlay_Results(gameResult, null,
                    // DISABLED_MGRS_CONT: (RacePlayEndInfo info) =>
                    // DISABLED_MGRS_CONT: {
                        // DISABLED_MGRS_CONT: Debug.Log("sendTestResult Success");
                        //bNetworkComplete = true;
                        // DISABLED_MGRS_CONT: raceResultInfo = info;                        
                        // DISABLED_MGRS_CONT: resultMain.initRace(manager);
                        // DISABLED_MGRS_CONT: setBack();
                        
                    // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                    // DISABLED_MGRS_CONT: {
                        //Debug.Log("sendTestResult Fail : " + er.code);
                        //결과전송 에러 팝업
                        // DISABLED_MGRS_CONT: networkErrorPopup();
                    // DISABLED_MGRS_CONT: }));
        }

        /// <summary>
        /// 9회2아웃 결과를 서버에 전송
        /// </summary>
        private void sendWalkOffResult()
        {
            //
            resultType = ResultType.WalkOff;
            int finalRound = manager.nineTwoFinalRound;
            int finalScore = manager.nineTwoFinalScore;
            // DISABLED_MGRS: Mgrs.RestService.WalkoffPlay_Result(finalRound, finalScore,
                    // DISABLED_MGRS_CONT: (WalkoffPlayEndInfo info) =>
                    // DISABLED_MGRS_CONT: {
                        // DISABLED_MGRS_CONT: Debug.Log("sendTestResult Success");
                        //bNetworkComplete = true;
                        // DISABLED_MGRS_CONT: walkoffResultInfo = info;                        
                        // DISABLED_MGRS_CONT: walkOffResult.Init(manager);
                        // DISABLED_MGRS_CONT: setBack();                        
                    // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                    // DISABLED_MGRS_CONT: {
                        //Debug.Log("sendTestResult Fail : " + er.code);
                        //결과전송 에러 팝업
                        // DISABLED_MGRS_CONT: networkErrorPopup();
                    // DISABLED_MGRS_CONT: }));

            
        }


        /// <summary>
        /// 뒷배경 세팅
        /// </summary>
        private void setBack()
        {
            front.GetComponent<UIPanel>().alpha = 1;
            TweenAlpha.Begin(front, 0.5f, 0);

            DefineEnum.EGameMode mode = DefineEnum.EGameMode.Season;

            if (mode == DefineEnum.EGameMode.Walkoff)
            {
                //winBack.SetActive(true);
                //normalBack.SetActive(true);
                // DISABLED_MGRS: Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_WalkoffPlay;
                leftTitleSetting("9회말 2아웃");
                //lose.SetActive(true);
                lose.transform.Find("result").GetComponent<UISprite>().spriteName = "walkoff_result_title";
                statButonObj.SetActive(false);
                otherResultButton.SetActive(false);
                getCoinObj.SetActive(false);
                stringObj.SetActive(true);
                //연출
                walkOffResult.setEffectStart(lose, leftTitle, rightTitle);
            }
            else
            {
                //결과
                if (manager.nGameScore[0] > manager.nGameScore[1])
                {
                    winSetting(mode);
                    //연출
                    resultMain.setEffectStart(win, leftTitle, rightTitle);
                }
                else if (manager.nGameScore[0] < manager.nGameScore[1])
                {
                    loseSetting(mode);
                    //연출
                    resultMain.setEffectStart(lose, leftTitle, rightTitle);
                }
                else
                {
                    drawSetting(mode);
                    //연출
                    resultMain.setEffectStart(draw, leftTitle, rightTitle);
                }
            

            //타이틀
#if _Test_Local
                leftTitle.SetActive(true);
                rightTitle.SetActive(true);
#else

                if (mode == DefineEnum.EGameMode.Season || mode == DefineEnum.EGameMode.SeasonInstant || mode == DefineEnum.EGameMode.SeasonConsecutive)
                {
                    if (mode == DefineEnum.EGameMode.SeasonConsecutive)
                    {
                        GameObject obj = Util.Load("MainGame/prefabs/gameUI/consectiveGamePrefab", transform, new Vector3(0,361,0));
                        obj.GetComponent<consectiveGameUI>().Init();
                    }

                    //시즌인 경우
                    // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo; 
                
                    //왼쪽 타이틀
                    string titleName;
                    if (info.gameType == SeasonGameType.KoreaSeries) titleName = "정규시즌 한국시리즈";
                    else if (info.gameType == SeasonGameType.PlayOff) titleName = "정규시즌 플레이오프";
                    else if (info.gameType == SeasonGameType.SemiPlayOff) titleName = "정규시즌 준플레이오프";
                    else if (info.gameType == SeasonGameType.WildCard) titleName = "정규시즌 와일드카드";
                    else
                    {
                        titleName = "정규시즌 페넌트레이스";
                        //타팀결과
                        otherResultButtonSetting();
                        //오른쪽
                        // DISABLED_MGRS: rightTitleSetting(Mgrs.userData.seasonLobbyInfo.roundNo + "일차 (총" + Mgrs.userData.seasonLobbyInfo.schedules.Count + "일차)");
                    }
                    //타이틀
                    leftTitleSetting(titleName);

                    //기록정보
                    statButonObj.SetActive(true);
                    // DISABLED_MGRS: if (Mgrs.UI.GetWindow(WindowID.UI_PopupInstancePlay) != null)
                    {
                        // DISABLED_MGRS: UI_PopupInstancePlay popup = Mgrs.UI.GetWindow(WindowID.UI_PopupInstancePlay).GetComponent<UI_PopupInstancePlay>();
                        popup.CloseWindow();
                    }


                    //
                    if (mode == DefineEnum.EGameMode.SeasonConsecutive)
                    {
                        autoNextCoroutione = autoNext();
                        StartCoroutine(autoNextCoroutione);
                    }

                }
                /*else if (mode == DefineEnum.EGameMode.Rank)
                {
                    //코인획득정보
                    leftTitleSetting("랭킹전");
                    getCoinSetting(rankResultInfo.coin);

                    //기록정보
                    statButonObj.SetActive(true);
                }*/
                else if (mode == DefineEnum.EGameMode.LeagueRace || mode == DefineEnum.EGameMode.LeagueRaceInstant)
                {
                    //임시
                    leftTitleSetting("쟁탈전");
                    
                    //기록정보
                    statButonObj.SetActive(true);
                }
                else if (mode == DefineEnum.EGameMode.LiveMatch)
                {
                    //왼쪽
                    leftTitleSetting("라이브매치");
                    //오른쪽
                    rightTitleSetting(string.Format("{0} 포인트 ({1:+0;-0})", liveResultInfo.point, liveResultInfo.chgPoint));

                    //코인정보
                    getCoinSetting(liveResultInfo.coin);
                    //기록정보
                    statButonObj.SetActive(true);
                }
#endif
            }
            if (mode != DefineEnum.EGameMode.SeasonInstant && mode != DefineEnum.EGameMode.LeagueRaceInstant) Debug_UI.SetNetwork(false);
            back.SetActive(true);
            //TweenAlpha.Begin(back.gameObject, 1, 1);

        }

        //왼쪽 타이틀
        private void leftTitleSetting(string text)
        {
            UILabel leftLabel = leftTitle.transform.Find("leftLabel").GetComponent<UILabel>();
            leftTitle.SetActive(true);
            leftLabel.text = text;
        }

        //오른쪽 타이틀
        private void rightTitleSetting(string text)
        {
            UILabel rightLabel = rightTitle.transform.Find("rightLabel").GetComponent<UILabel>();
            rightTitle.SetActive(true);
            rightLabel.text = text;
        }


        //타팀 경기 버튼 세팅
        private void otherResultButtonSetting()
        {
            otherResultButton.SetActive(true);
        }

        private void getCoinSetting(int coin)
        {
            getCoinObj.SetActive(true);
            getCoinObj.transform.Find("coinLabel").GetComponent<UILabel>().text = string.Format("{0:N0}", coin); 
        }

        //이긴경우
        private void winSetting(DefineEnum.EGameMode mode)
        {
            //연속경기인 경우 부하를 줄이기 위해 일반뒷배경
            //if (mode == DefineEnum.EGameMode.SeasonConsecutive) normalBack.SetActive(true);
            //else winBack.SetActive(true);

            // DISABLED_MGRS: if (mode == DefineEnum.EGameMode.Season || mode == DefineEnum.EGameMode.SeasonInstant || mode == DefineEnum.EGameMode.SeasonConsecutive) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_SeasonWin;
            // DISABLED_MGRS: else if (mode == DefineEnum.EGameMode.LiveMatch) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_LiveMatchWin;
            // DISABLED_MGRS: else if (mode == DefineEnum.EGameMode.LeagueRace || mode == DefineEnum.EGameMode.LeagueRaceInstant) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_LeagueRaceWin;
            //win.SetActive(true);
        }

        //진경우
        private void loseSetting(DefineEnum.EGameMode mode)
        {
            //normalBack.SetActive(true);
            // DISABLED_MGRS: if (mode == DefineEnum.EGameMode.Season || mode == DefineEnum.EGameMode.SeasonInstant || mode == DefineEnum.EGameMode.SeasonConsecutive) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_SeasonLose;
            // DISABLED_MGRS: else if (mode == DefineEnum.EGameMode.LiveMatch) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_LiveMatchLose;
            // DISABLED_MGRS: else if (mode == DefineEnum.EGameMode.LeagueRace || mode == DefineEnum.EGameMode.LeagueRaceInstant) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_LeagueRaceLose;
            //lose.SetActive(true);
        }

        //비긴경우
        private void drawSetting(DefineEnum.EGameMode mode)
        {
            //normalBack.SetActive(true);
            // DISABLED_MGRS: if (mode == DefineEnum.EGameMode.Season || mode == DefineEnum.EGameMode.SeasonInstant || mode == DefineEnum.EGameMode.SeasonConsecutive) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_SeasonDraw;
            // DISABLED_MGRS: else if (mode == DefineEnum.EGameMode.LiveMatch) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_LiveMatchDraw;
            // DISABLED_MGRS: else if (mode == DefineEnum.EGameMode.LeagueRace || mode == DefineEnum.EGameMode.LeagueRaceInstant) Mgrs.userData.UserLobbyReason = UserData.EReason.InGame_LeagueRaceDraw;
            //draw.SetActive(true);
        }


        /// <summary>
        /// 외부에서 컨트롤
        /// </summary>
        public void gotoOutGame()
        {
            StartCoroutine(outGame());
        }


        /// <summary>
        /// 아웃게임으로
        /// </summary>
        private IEnumerator outGame()
        {
            curStep = ResultStep.OutGame;

            
            DefineEnum.EGameMode mode = DefineEnum.EGameMode.Season;

            bool bConcectiveGame = false;

            if (mode == DefineEnum.EGameMode.SeasonConsecutive)
            {                
                
            }

            if (bConcectiveGame == false)
            {                
                if (mode == DefineEnum.EGameMode.SeasonInstant)
                {
                    leftTitle.gameObject.SetActive(false);
                    rightTitle.gameObject.SetActive(false);
                    TweenAlpha.Begin(front, 0.5f, 1);
                    yield return new WaitForSeconds(0.5f);
                    //정규시즌 화면 업데이트                    
                    // DISABLED_MGRS: Mgrs.NM.C2S_GetSeasonLobbyInfo();
                    Destroy(gameObject);
                }
                else if (mode == DefineEnum.EGameMode.LeagueRaceInstant)
                {
                    leftTitle.gameObject.SetActive(false);
                    rightTitle.gameObject.SetActive(false);
                    TweenAlpha.Begin(front, 0.5f, 1);
                    yield return new WaitForSeconds(0.5f);
                    //쟁탈전 화면 업데이트                    
                    // DISABLED_MGRS: Mgrs.NM.C2S_RacePlay_GetLobbyInfo();
                    Destroy(gameObject);
                }
                else
                {
                    TweenAlpha.Begin(back.gameObject, 0.5f, 0);
                    yield return new WaitForSeconds(0.5f);
                    //로비행
                    gotoLobby();
                }
            }
            else
            {
                //연속 경기
                TweenAlpha.Begin(back.gameObject, 0.5f, 0);
                yield return new WaitForSeconds(0.5f);

                
            }
        }

        //로비로 감
        private void gotoLobby()
        {
            // DISABLED_MGRS: Mgrs.ManagerSupervise(false);
            SkillEffectDisplayManager.Destroy();
            GameObject simulator = GameObject.FindWithTag("SIMUL_TAG");
            if (simulator != null) Destroy(simulator.gameObject);
            // DISABLED_MGRS: Mgrs.SceneLoad.LoadScene(SceneID.Lobby);
        }

        //연속경기 진행
        private void goConsecutiveGame()
        {
            
        }


        //업그레이드로 감
        private void gotoUpgrade()
        {
            // DISABLED_MGRS: Mgrs.userData.UserLobbyReason = UserData.EReason.OutGame_TeamManage;
            DefineEnum.EGameMode mode = DefineEnum.EGameMode.Season;
            if (mode == DefineEnum.EGameMode.SeasonInstant)
            {
                
            }
            else
            {
                // DISABLED_MGRS: Mgrs.ManagerSupervise(false);
                SkillEffectDisplayManager.Destroy();
                Destroy(GameObject.FindWithTag("SIMUL_TAG").gameObject);
                // DISABLED_MGRS: Mgrs.SceneLoad.LoadScene(SceneID.Lobby);
            }            
        }

        

        /// <summary>
        /// 성장결과 세팅
        /// </summary>
        private void growSetting()
        {
            resultGrow.init();
            stringObj.SetActive(true);
            curStep = ResultStep.Grow;
        }

        /// <summary>
        /// 보상 세팅
        /// </summary>
        private void rewardSetting()
        {
            resultReward.init();            
            stringObj.SetActive(true);
            curStep = ResultStep.Reward;
        }

        /// <summary>
        /// 버튼 이벤트 다음 버튼 누른경우
        /// </summary>
        public void pressNext()
        {
#if _Test_Local
            
#else
            if (curStep == ResultStep.OtherTeam || curStep == ResultStep.Stat || curStep == ResultStep.OutGame) return;

            autoNextTime = 0; 
            // DISABLED_MGRS: DefineEnum.EGameMode mode = Mgrs.userData.GetUserGameMode();
            lastStep = curStep;
            if (curStep == ResultStep.WalkOff)
            {
                //9회 투아웃
                walkOffResult.deActive();
                StartCoroutine(outGame());
            }
            else if (curStep == ResultStep.Main)
            {
                //결과 메인창
                resultMain.deActive();
                statButonObj.SetActive(false);
                otherResultButton.SetActive(false);
                if (mode == DefineEnum.EGameMode.Season ||
                    mode == DefineEnum.EGameMode.SeasonInstant || 
                    mode == DefineEnum.EGameMode.SeasonConsecutive || 
                    mode == DefineEnum.EGameMode.LeagueRace ||
                    mode == DefineEnum.EGameMode.LeagueRaceInstant)
                {
                    //해당 모드에서는 보상 창으로 이동
                    rewardSetting(); 
                }
                else if (mode == DefineEnum.EGameMode.LiveMatch)
                {
                    //라이브 매치
                    if (liveResultInfo.chgLeagueLev[0] != liveResultInfo.chgLeagueLev[1])
                    {
                        //승강 발생시 승강 이벤트
                        back.transform.FindChild("button").gameObject.SetActive(false);
                        liveLeagueUpDown.gameObject.SetActive(true);
                        liveLeagueUpDown.InitLivematchUpDown(liveResultInfo);
                    }
                    else
                    {
                        //아니면 아웃게임
                        StartCoroutine(outGame());
                    }
                }
                else //if (mode == DefineEnum.EGameMode.Rank)
                {
                    //그외 모드에서는 바로 아웃게임
                    StartCoroutine(outGame());
                }
            }
            else if (curStep == ResultStep.Reward)
            {
                //보상
                resultReward.deActive();
                if (mode == DefineEnum.EGameMode.LeagueRace || mode == DefineEnum.EGameMode.LeagueRaceInstant)
                {                    
                    StartCoroutine(outGame());
                }
                else
                {                    
                    growSetting();
                }
            }
            else if (curStep == ResultStep.Grow)
            {
                //성장
                resultGrow.deActive();
                StartCoroutine(outGame());
            }
#endif
        }


        private void gotoLogin()
        {
            //PhotonManager.Get().Disconnect();
            MusicManager.Get().StopMusic();

            GameObject obj1 = GameObject.Find("skillEffectDisplayManager").gameObject;
            if (obj1 != null) Destroy(obj1);
            GameObject obj2 = GameObject.Find("simulator").gameObject;
            if (obj2 != null) Destroy(obj2);
            GameObject obj3 = GameObject.Find("Managers").gameObject;
            if (obj3 != null) Destroy(obj3);
            GameObject obj4 = GameObject.Find("pvpmanager").gameObject;
            if (obj4 != null) Destroy(obj4);
            GameObject obj5 = GameObject.Find("PhotonManager").gameObject;
            if (obj5 != null) Destroy(obj5);
            GameObject obj6 = GameObject.Find("SoundManager").gameObject;
            if (obj6 != null) Destroy(obj6);
            GameObject obj7 = GameObject.Find("MusicManager").gameObject;
            if (obj7 != null) Destroy(obj7);
            Invoke("loadScene", 1.0f);
        }


        private void loadScene()
        {
            AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Login");
        }


        /// <summary>
        /// 버튼 이벤트 다른팀 결과 탭
        /// </summary>
        public void pressOtherTeamResult()
        {
#if !_Test_Local
            if (curStep == ResultStep.OtherTeam || curStep == ResultStep.Stat)
            {
                return;
            }

            otherResult.init(otherResultInit);
            otherResultInit = true;
            lastStep = curStep;
            curStep = ResultStep.OtherTeam;
#endif
        }

        /// <summary>
        /// 버튼 이벤트 내 게임 기록 보기
        /// </summary>
        public void pressStat()
        {
            
            if (curStep == ResultStep.OtherTeam || curStep == ResultStep.Stat) 
            {
                return;
            }

            playerRecord.Init(playerStatInit);
            playerStatInit = true;
            lastStep = curStep;
            curStep = ResultStep.Stat;
        }


        float autoNextTime = 0;
        private IEnumerator autoNextCoroutione;
        private IEnumerator autoNext()
        {     
            autoNextTime = 0;
            while (autoNextTime < 3.0f)
            {
                autoNextTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            DefineEnum.EGameMode mode = DefineEnum.EGameMode.Season;
            if (mode == DefineEnum.EGameMode.SeasonConsecutive)
            {
                pressNext();
                autoNextCoroutione = autoNext();
                StartCoroutine(autoNextCoroutione);
            }
        }



        public void OnClickBacktoLobby()
        {
            if (bFinishButton == false)
            {
                bFinishButton = true;
                Debug.Log("BackToLobby");
                StartCoroutine(backToLobbyProcess());
            }
        }

        private IEnumerator backToLobbyProcess()
        {
            KOBManager.FrontUI.OpenPopup<FrontUI_IngameLoading>().GotoLobby();
            //신로딩
            AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainLobby");
            while (!async.isDone)
            {
                yield return null;
            }
        }
    }
}
