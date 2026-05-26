using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class InstantSimulator : MonoBehaviour
    {
        
        public class InstantGameData
        {
            public WebConnector.SeasonTeamInfo myTeamInfo;
            public WebConnector.SeasonTeamInfo cpuTeamInfo;
            //public bool isMyhome;
            //public int scheNo;
            public int myScore;
            public int cpuScore;
            public int startOrd;

            public CPlayer winPitcher, losePitcher;

            public InstantGameData(WebConnector.SeasonTeamInfo myTeamInfo, WebConnector.SeasonTeamInfo cpuTeamInfo, int myScore, int cpuScore, int startOrd, CPlayer winPitcher, CPlayer losePitcher)
            {
                this.myTeamInfo = myTeamInfo;
                this.cpuTeamInfo = cpuTeamInfo;
                //this.isMyhome = isMyhome;
                //this.scheNo = scheNo;
                this.myScore = myScore;
                this.cpuScore = cpuScore;
                this.startOrd = startOrd;

                this.winPitcher = winPitcher;
                this.losePitcher = losePitcher;
            }

        }

        public GameObject simul;
        private SimulGameInfo gameInfo;
        
        public UILabel[] awayScore;
        public UILabel[] homeScore;

        public UILabel[] awayRecord;
        public UILabel[] homeRecord;

        public UITexture[] logo;
        public UILabel[] teamName;


        private int consectiveGameNum;
        /*        
        // Use this for initialization
        void Start()
        {
            //StartCoroutine(startGame());
        }*/
        //연속경기
        private int gameCount;
        private int totalWin, totalDraw, totalLose;
        private InstantGameData[] gameData;
        private Dictionary<int, int> items;
        private int[] balance = new int[4];
        private int lastGold;
        
        public void init(int gameNum)
        {
            // DISABLED_MGRS: if(gameNum> 1) Mgrs.UI.StartLoading();
            consectiveGameNum = gameNum;
            
            // DISABLED_MGRS: Mgrs.RestService.GameConst_Common(
                // DISABLED_MGRS_CONT: (GameConstCommon info) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("게임상수 Success");
                    // DISABLED_MGRS_CONT: SimulParm.InitSkillMap(info);
                    // DISABLED_MGRS_CONT: if (consectiveGameNum > 1)
                    // DISABLED_MGRS_CONT: {
                        // DISABLED_MGRS_CONT: StartCoroutine(consectiveGame(true));
                    // DISABLED_MGRS_CONT: }
                    // DISABLED_MGRS_CONT: else
                    // DISABLED_MGRS_CONT: {
                        // DISABLED_MGRS_CONT: StartCoroutine(startGame());
                    // DISABLED_MGRS_CONT: }

                // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("게임상수 Fail : " + er.code);

                // DISABLED_MGRS_CONT: }));    
            
        }

        public void initRace()
        {
            // DISABLED_MGRS: Mgrs.UI.StartLoading();
            // DISABLED_MGRS: Mgrs.RestService.GameConst_Common(
                // DISABLED_MGRS_CONT: (GameConstCommon info) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("게임상수 Success");
                    // DISABLED_MGRS_CONT: SimulParm.InitSkillMap(info);
                    // DISABLED_MGRS_CONT: StartCoroutine(startGameRace());

                // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("게임상수 Fail : " + er.code);

                // DISABLED_MGRS_CONT: }));

        }


#if _Test_Local
        public bool bHome = false;

        private IEnumerator startGame()
        {
            Mode.b2outBaseLoadedMode = false;
            Mode.bTieBreaker = false;
            Mode.gameMode = Mode.GamePlayMode.Season;

            Mode.finalInning = 9;
            Mode.maxInning = 12;

            SimulPlayerManager.InitSeasonResult();

            Mode.bSimulationQuickPlay = false;           //고속게임
            

            SimulPlayerManager.SetInit();
            bool bMyHome = bHome;

            SimulPlayerManager.awayTeamIndex = Random.Range(1, 11);
            SimulPlayerManager.homeTeamIndex = Random.Range(1, 11);
            SimulPlayerManager.strAwayTeam = SimulPlayerManager.GetTeamName(false);
            SimulPlayerManager.strHomeTeam = SimulPlayerManager.GetTeamName(true);
            
            SimulPlayerManager.strMyTeam = bMyHome ? SimulPlayerManager.strHomeTeam : SimulPlayerManager.strAwayTeam;
            SimulPlayerManager.strCPUTeam = bMyHome ? SimulPlayerManager.strAwayTeam : SimulPlayerManager.strHomeTeam;
            SimulPlayerManager.myTeamIndex = bMyHome ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex;
            SimulPlayerManager.cpuTeamIndex = bMyHome ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex;

            /*
            for (int i = 0; i < 2; i++)
            {
                logo[i].mainTexture = Util.loadMiddleLogo(i == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex);
                logo[i].MakePixelPerfect();
            }*/

            yield return new WaitForEndOfFrame();
            //로컬 테스트용
            for (int i = 0; i < 5; i++)
            {
                SimulPlayerManager.MakePlayerLocal(i);
                yield return new WaitForEndOfFrame();
            }
            SimulManager.InitGame(bMyHome, null);

            /*bool bTopInning = true;
            int curInning = 1;
            bool bEndGame = false;
            int awayIndex = 0;
            int homeIndex = 1-awayIndex;

            while (bEndGame == false)
            {
                yield return new WaitForSeconds(0.1f);
                SimulManager.SimulNextInning(true);
                gameInfo = SimulManager.GetGameInfo();

                if (bTopInning == true)
                {
                    awayScore[curInning - 1].gameObject.SetActive(true);
                    awayScore[curInning - 1].text = gameInfo.inningScore[awayIndex, curInning - 1].ToString();

                    awayRecord[0].text = gameInfo.run[awayIndex].ToString();
                    awayRecord[1].text = gameInfo.hit[awayIndex].ToString();
                    awayRecord[2].text = gameInfo.error[awayIndex].ToString();
                    awayRecord[3].text = gameInfo.fourBall[awayIndex].ToString();
                }
                else
                {
                    homeScore[curInning - 1].gameObject.SetActive(true);
                    homeScore[curInning - 1].text = gameInfo.inningScore[homeIndex, curInning - 1].ToString();

                    homeRecord[0].text = gameInfo.run[homeIndex].ToString();
                    homeRecord[1].text = gameInfo.hit[homeIndex].ToString();
                    homeRecord[2].text = gameInfo.error[homeIndex].ToString();
                    homeRecord[3].text = gameInfo.fourBall[homeIndex].ToString();
                }

                if (bTopInning == false) curInning++;
                bTopInning = !bTopInning;
                bEndGame = SimulManager.isEndGame();
            }*/

            SimulManager.GameSimulate();

            SimulManager.SimulResultSetting(null, true);

            yield return new WaitForEndOfFrame();

            BallPlayManager manager = new BallPlayManager();
            manager.bMyHome = bMyHome;
            SimulManager.SyncData(manager, true, true, false);
            Util.Load("MainGame/prefabs/resultUI/resultPrefab", null, Vector3.zero).GetComponent<ResultUI>().Init(manager);
            Destroy(gameObject);
            
        }

        private IEnumerator consectiveGame(bool bStart)
        {
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator startGameRace()
        {
            yield return new WaitForSeconds(0.2f);

        }
#else

        /// <summary>
        /// 레이스 즉시 완료
        /// </summary>
        /// <returns></returns>
        private IEnumerator startGameRace()
        {
            // DISABLED_MGRS: Mgrs.UI.StopLoading();
            yield return new WaitForSeconds(2f);

            SimulPlayerManager.SetInit();

            // DISABLED_MGRS: WebConnector.RacePlayGameInfo info = Mgrs.userData.raceInfo;

            Mode.b2outBaseLoadedMode = false;
            Mode.gameMode = Mode.GamePlayMode.Race;
            Mode.finalInning = 9;
            Mode.maxInning = 12;

            bool bMyHome = info.home;

            //팀정보

            // DISABLED_MGRS: RacePlayTeamInfo homeTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.homeTeamNo);
            // DISABLED_MGRS: RacePlayTeamInfo awayTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.awayTeamNo);
            SimulPlayerManager.homeTeamIndex = (int)(homeTeam.team);
            SimulPlayerManager.awayTeamIndex = (int)(awayTeam.team);
            SimulPlayerManager.strHomeTeam = homeTeam.name;
            SimulPlayerManager.strAwayTeam = awayTeam.name;
            SimulPlayerManager.homeTeamIndex = (int)(homeTeam.team);
            SimulPlayerManager.awayTeamIndex = (int)(awayTeam.team);
            SimulPlayerManager.myTeamIndex = bMyHome ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex;
            SimulPlayerManager.cpuTeamIndex = bMyHome ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex;
            SimulPlayerManager.strHomeTeam = homeTeam.name;
            SimulPlayerManager.strAwayTeam = awayTeam.name;
            SimulPlayerManager.strMyTeam = bMyHome ? SimulPlayerManager.strHomeTeam : SimulPlayerManager.strAwayTeam;
            SimulPlayerManager.strCPUTeam = bMyHome ? SimulPlayerManager.strAwayTeam : SimulPlayerManager.strHomeTeam;
            
            //라인업 세팅
            GameLineup lineup = SimulManager.GetRaceGameLineup(info.homeLineup, info.awayLineup);            
            int myStarterOdr = Random.Range(1, 6);
            int otherStarterOdr = Random.Range(1, 6);
            SimulManager.SimulateOneGame(bMyHome, lineup, myStarterOdr, otherStarterOdr);
            //SimulPlayerManager.GetGameRaceResults(bMyHome);
            //SimulPlayerManager.AddPitcherResult();

            yield return new WaitForEndOfFrame();


            BallPlayManager manager = new BallPlayManager();
            manager.bMyHome = bMyHome;
            SimulManager.SyncData(manager, true, true, false);
            ResultUI resultManager = Util.Load("MainGame/prefabs/resultUI/resultPrefab", null, Vector3.zero).GetComponent<ResultUI>();
            simul.transform.parent = resultManager.transform;
            resultManager.Init(manager);

            Destroy(gameObject);
        }

        /// <summary>
        /// 한경기만 함
        /// </summary>
        /// <returns></returns>
        private IEnumerator startGame()
        {
            // DISABLED_MGRS: Mgrs.UI.StopLoading();
            yield return new WaitForSeconds(2f);

            SimulPlayerManager.SetInit();

            // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;

            seasonInitSetting(info);

            yield return new WaitForEndOfFrame();
            
            bool bMyHome = info.home;

            int myScheSeq = info.myScheNo;
            int[] teamNo = info.schedule[myScheSeq];
            bMyHome = info.home;

            //팀정보
            SeasonTeamInfo homeTeam = info.teamInfos[teamNo[0]];
            SeasonTeamInfo awayTeam = info.teamInfos[teamNo[1]];
            SimulPlayerManager.homeTeamIndex = (int)(homeTeam.team);
            SimulPlayerManager.awayTeamIndex = (int)(awayTeam.team);
            SimulPlayerManager.myTeamIndex = bMyHome ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex;
            SimulPlayerManager.cpuTeamIndex = bMyHome ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex;
            SimulPlayerManager.strHomeTeam = homeTeam.name;
            SimulPlayerManager.strAwayTeam = awayTeam.name;
            SimulPlayerManager.strMyTeam = bMyHome ? SimulPlayerManager.strHomeTeam : SimulPlayerManager.strAwayTeam;
            SimulPlayerManager.strCPUTeam = bMyHome ? SimulPlayerManager.strAwayTeam : SimulPlayerManager.strHomeTeam;
            SimulPlayerManager.myTeamSeqNum = teamNo[bMyHome ? 0 : 1];
            SimulPlayerManager.cpuTeamSeqNum = teamNo[bMyHome ? 1 : 0];

            //라인업 세팅
            GameLineup lineup = SimulManager.GetSeasonGameLineup(homeTeam, awayTeam);
            SimulManager.SimulateOneGame(bMyHome, lineup, info.starterOdr, info.starterOdr);
            SimulPlayerManager.AddSeasonResult(myScheSeq, bMyHome);
            SimulPlayerManager.AddPitcherResult();

            yield return new WaitForEndOfFrame();


            BallPlayManager manager = new BallPlayManager();
            manager.bMyHome = bMyHome;
            SimulManager.SyncData(manager, true, true, false);
            ResultUI resultManager = Util.Load("MainGame/prefabs/resultUI/resultPrefab", null, Vector3.zero).GetComponent<ResultUI>();
            simul.transform.parent = resultManager.transform;
            resultManager.Init(manager);

            Destroy(gameObject);
        }

        /// <summary>
        /// 연속경기함
        /// </summary>
        /// <param name="gameNum"></param>
        /// <returns></returns>
        UI_PopupSchedule schedulePopup = null;        
        private IEnumerator consectiveGame(bool bStart)
        {
            if (bStart == true)
            {
                gameData = new InstantGameData[10];
                gameCount = 0;
                totalWin = totalDraw = totalLose = 0;
                items = new Dictionary<int, int>();
                System.Array.Clear(balance, 0, balance.Length);
                // DISABLED_MGRS: lastGold = Mgrs.userData.GetUserHaveGold();
                // DISABLED_MGRS: schedulePopup = Mgrs.UI.OpenWindow(WindowID.UI_PopupSchedule).GetComponent<UI_PopupSchedule>();
                if (schedulePopup == null || schedulePopup.isActiveAndEnabled == false)
                    yield break;
                schedulePopup.SetInitCondition();
                yield return new WaitForSeconds(0.5f);
            }
            //yield return new WaitForSeconds(0.0f);

            //Debug.Log("===================================================>> 경기 시작 " + consectiveGameNum);

            // DISABLED_MGRS: Mgrs.UI.StopLoading();

            //시즌 초기화
            SimulPlayerManager.SetInit();
            // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;
            seasonInitSetting(info);
            
            //게임정보
            bool bMyHome = info.home;
            int myScheSeq = info.myScheNo;
            int[] teamNo = info.schedule[myScheSeq];
            bMyHome = info.home;

            //팀정보
            SeasonTeamInfo homeTeam = info.teamInfos[teamNo[0]];
            SeasonTeamInfo awayTeam = info.teamInfos[teamNo[1]];
            
            //라인업 세팅
            GameLineup lineup = SimulManager.GetSeasonGameLineup(homeTeam, awayTeam);
            SimulManager.SimulateOneGame(bMyHome, lineup, info.starterOdr, info.starterOdr);
            SimulPlayerManager.AddSeasonResult(myScheSeq, bMyHome);
            SimulPlayerManager.AddPitcherResult();

            //정보 세팅
            int myScore = SimulManager.GetGameInfo().run[0];
            int cpuScore = SimulManager.GetGameInfo().run[1];
            if(myScore > cpuScore) totalWin++;
            else if (myScore < cpuScore) totalLose++;
            else totalDraw++;
            SeasonTeamInfo myTeam = bMyHome ? homeTeam : awayTeam;
            SeasonTeamInfo cpuTeam = bMyHome ? awayTeam : homeTeam;
            CPlayer winPitcher = SimulManager.getWinPitcher();
            CPlayer losePitcher = SimulManager.getLosePitcher();
            gameData[gameCount] = new InstantGameData(myTeam, cpuTeam, myScore, cpuScore, info.starterOdr, winPitcher, losePitcher);

            //
            schedulePopup.SettingInstancePlay(gameData[gameCount], gameCount);
            gameCount++;

            yield return new WaitForSeconds(gameCount < 5 ? 0.5f : 0.8f);
            List<SeasonGameResult> gameResults = SimulPlayerManager.GetGameResults();
            string summary = SimulPlayerManager.GetGameSummary();
            // DISABLED_MGRS: Mgrs.RestService.SeasonPlay_Results(gameResults, summary,
                // DISABLED_MGRS_CONT: (SeasonGameEndInfo endinfo) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("sendTestResult Success");
                    //재화 업데이트
                    // DISABLED_MGRS: Mgrs.userData.SetUserBalances(endinfo.balances);
                    //팀레벨 업데이트
                    // DISABLED_MGRS: Mgrs.userData.UpdateTeamLevel(endinfo.teamExp, endinfo.teamLevel);
                    //아이템 업데이트
                    // DISABLED_MGRS_CONT: if (endinfo.items != null)
                    // DISABLED_MGRS_CONT: {
                        //현재 획득 아이템 처리
                        // DISABLED_MGRS_CONT: foreach (KeyValuePair<int, int> value in endinfo.items)
                        // DISABLED_MGRS_CONT: {
                            // DISABLED_MGRS_CONT: int key = value.Key;
                            // DISABLED_MGRS_CONT: int val = value.Value;
                            // DISABLED_MGRS_CONT: int lastHaveNum = 0;
                            // DISABLED_MGRS: if (Mgrs.userData.user_info_bundle.items.ContainsKey(key) == true)
                            // DISABLED_MGRS_CONT: {
                                // DISABLED_MGRS: lastHaveNum = Mgrs.userData.user_info_bundle.items[key];
                            // DISABLED_MGRS_CONT: }
                            // DISABLED_MGRS_CONT: int getNum = val - lastHaveNum;
                            // DISABLED_MGRS_CONT: if (items.ContainsKey(key) == true)
                            // DISABLED_MGRS_CONT: {
                                // DISABLED_MGRS_CONT: items[key] += getNum;
                            // DISABLED_MGRS_CONT: }
                            // DISABLED_MGRS_CONT: else
                            // DISABLED_MGRS_CONT: {
                                // DISABLED_MGRS_CONT: items.Add(key, getNum);
                            // DISABLED_MGRS_CONT: }
                        // DISABLED_MGRS_CONT: }
                        // DISABLED_MGRS: Mgrs.userData.SetUserItem(endinfo.items, true);
                    // DISABLED_MGRS_CONT: }

                    // DISABLED_MGRS_CONT: askLobbyInfo();
                    //결과전송 끝
                // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                // DISABLED_MGRS_CONT: {
                    //Debug.Log("sendTestResult Fail : " + er.code);
                    //결과전송 에러 팝업
                // DISABLED_MGRS_CONT: }));


            yield return new WaitForEndOfFrame();

            
        }

        /// <summary>
        /// 연속경기시 로비 인포
        /// </summary>
        /// <returns></returns>
        private void askLobbyInfo()
        {
            consectiveGameNum--;
            if (consectiveGameNum > 0)
            {
                // DISABLED_MGRS: Mgrs.RestService.SeasonPlay_GetLobbyInfo(
                    // DISABLED_MGRS_CONT: (SeasonLobbyInfo lobbyInfo) =>
                    // DISABLED_MGRS_CONT: {
                        // DISABLED_MGRS: Mgrs.userData.seasonLobbyInfo = lobbyInfo;
                        
                        // DISABLED_MGRS_CONT: if (lobbyInfo.annInfo != null)
                        // DISABLED_MGRS_CONT: {
                            //Debug.Log("==================>>어나운스 인포가 들어온 경우 연속경기 중단!!");                            
                            // DISABLED_MGRS_CONT: consectiveEnd(lobbyInfo, null);
                        // DISABLED_MGRS_CONT: }
                        // DISABLED_MGRS_CONT: else
                        // DISABLED_MGRS_CONT: {
                            // DISABLED_MGRS: Mgrs.RestService.SeasonPlay_Start(true, consectiveGameNum,
                            // DISABLED_MGRS_CONT: (SeasonGameInfo info) =>
                            // DISABLED_MGRS_CONT: {
                                //연속경기
                                // DISABLED_MGRS: Mgrs.userData.Ingame_seasonGameInfo = info;
                                // DISABLED_MGRS: Mgrs.userData.user_info_bundle.playballInfo = info.playballInfo;

                                // DISABLED_MGRS_CONT: if (info.items != null)
                                    // DISABLED_MGRS: Mgrs.userData.SetUserItem(info.items);

                                // DISABLED_MGRS: Mgrs.userData.SetUserGameMode(DefineEnum.EGameMode.SeasonConsecutive);
                                // DISABLED_MGRS_CONT: StartCoroutine(consectiveGame(false));// goConsecutiveGame();

                            // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                            // DISABLED_MGRS_CONT: {
                                //로비로 감
                                //Debug.Log("==================>>시즌게임 인포 못받아온 경우 연속경기 중단!!");
                                // DISABLED_MGRS_CONT: consectiveEnd(lobbyInfo, er);
                            // DISABLED_MGRS_CONT: }));
                        }

                    }, ((ErrorResource er) =>
                    {
                        //로비로 감
                        //Debug.Log("==================>>로비 인포 못받아온 경우 연속경기 중단!!");
                        consectiveEnd(null, er);
                    }));
            }
            else
            {
                consectiveEnd(null, null);
            }
        }


        private void consectiveEnd(SeasonLobbyInfo lobbyInfo, ErrorResource er)
        {
            //연속게임 종료
            schedulePopup.SetEndCondition(lobbyInfo, er, balance, items, gameCount);
            schedulePopup.ChangeCompleteObj(totalWin, totalDraw, totalLose);
            Destroy(gameObject);
        }


        private IEnumerator showScheduleWindow()
        {
            for (int i = 0; i < gameCount; i++)
            {
                //yield return new WaitForSeconds(i >= 4 ? 0.70f : 0.3f);
                //schedulePopup.SettingInstancePlay(gameData[i], i);
            }
            yield return new WaitForEndOfFrame();//yield return new WaitForSeconds(0.55f);
            schedulePopup.ChangeCompleteObj(totalWin, totalDraw, totalLose);
            yield return new WaitForEndOfFrame();
            Destroy(gameObject);
        }


        /// <summary>
        /// 시즌 모드에서 타팀경기를 시뮬레이션 한다
        /// </summary>
        /// <param name="info"></param>
        /// <param name="bMyGameInculde"></param>
        private void simulateSeasonGames(SeasonGameInfo info)
        {
            int myScheSeq = info.myScheNo;// gameInfo.myScheSeq;
            foreach (KeyValuePair<int, int[]> value in info.schedule)
            {
                bool bMyHome = false;
                int[] teamNo = value.Value; //0:home, 1:away
                int curSeq = value.Key;
                if (curSeq == myScheSeq)
                {
                    //내게임 무시
                    continue;
                }
                SeasonTeamInfo homeTeam = info.teamInfos[teamNo[0]];
                SeasonTeamInfo awayTeam = info.teamInfos[teamNo[1]];
                //Debug.Log("away = " + awayTeam.teamInfo.team + "vs home = " + homeTeam.teamInfo.team);
                GameLineup lineup = SimulManager.GetSeasonGameLineup(homeTeam, awayTeam);
                SimulManager.SimulateOneGame(bMyHome, lineup, info.starterOdr, info.starterOdr);
                SimulPlayerManager.AddSeasonResult(curSeq, bMyHome);
                SimulPlayerManager.AddPitcherResult();
            }
        }


        /// <summary>
        /// 시즌 기본 초기화 세팅
        /// </summary>
        /// <param name="info"></param>
        private void seasonInitSetting(SeasonGameInfo info)
        {
            Mode.b2outBaseLoadedMode = false;
            Mode.gameMode = Mode.GamePlayMode.Season;
            Mode.finalInning = 9;
            Mode.maxInning = 12;
            Mode.stadiumNum = 4; //임시

            Mode.bSimulationQuickPlay = false;           //저장값 사용
            Mode.bOnlyChanceMode = false;

            SimulPlayerManager.InitSeasonResult();
            if (info.gameType == SeasonGameType.PennantRace)
            {
                simulateSeasonGames(info);
            }
        }
#endif




    }
}
