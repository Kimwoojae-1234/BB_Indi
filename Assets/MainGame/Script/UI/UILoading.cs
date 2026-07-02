using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using WebConnector;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class UILoading : MonoBehaviour
    {
        //
#if _Test_Local

#if _Skill_Display
        //연출테스트용
        public pSkillDisplay pitcherSkill = pSkillDisplay.Chu_Gyeog_Bon_Neung;
        public bSkillDisplay batterSkill = bSkillDisplay.Mea_Noon;
#endif

        public Mode.GamePlayMode testMode = Mode.GamePlayMode.Season;// Mode.GamePlayMode.Pvp433;
        public bool MY_BATTING = true;
        public bool NO_OUT_STATE = true;
        public int TEST_STADIUM_NUM = -1;
#endif
        public GameObject _active;

        public GameObject simulator;

        //아웃게임 메니저
        private GameObject Managers;
        const int MAX_GAUGE = 796;
        public UITexture texture;
        public UISprite gauge;//, ball;        
        float lastCounter;
        int tipIndex, lastIndex;

        public static float loadingCount;

        public void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                Pien.AndroidExtention.DisableNavUI();
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(simulator);
#if UNITY_EDITOR
            if (Application.isEditor)
                Application.runInBackground = true;
#endif
        }

        void OnDestroy()
        {
        
        }

#if _Test_Local  
        IEnumerator Start()
        {
            IsLoadGame = false;
#if _Skill_Display
            //연출테스트용
            InGameDebug.PitcherSkill = pitcherSkill;
            InGameDebug.BitcherSkill = batterSkill;
#endif
            Application.targetFrameRate = 60;
            lastIndex = -100;
            //loadingTexture();
            Mode.gameMode = Mode.GamePlayMode.Season;// Mode.GamePlayMode.Pvp433;
            Mode.crowdPer = Random.Range(40, 80);
            if (Mode.gameMode == Mode.GamePlayMode.Season)
            {
                //로컬에서 시즌모드 시작
                startLoadingLocal();
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Ranking)
            {
                //로컬에서 랭킹모드 시작
                startRankingLoadingLocal();
            }
            else if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                //로컬에서 랭킹모드 시작
                startNineTwoLocal();
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                startVersusLoadingLocal();
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                startVersusLoadingLocal();
            }

            yield return new WaitForSeconds(0.1f);

#if _Test_RealTime
            StartCoroutine("StartLoad", "BallPlayRealTime");
#else
            StartCoroutine("StartLoad", "BallPlay");
#endif

        }
#else
        // Use this for initialization
        void Start()
        {
            Random.InitState((int)System.DateTime.Now.Ticks);
            Application.targetFrameRate = 60;
            lastIndex = -100;
            loadingTexture();

            Mode.crowdPer = Random.Range(40, 80);
            Mode.bPauseGame = false;

            // DISABLED_MGRS: Mgrs.RestService.GameConst_Common(
                // DISABLED_MGRS_CONT: (GameConstCommon info) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("게임상수 Success");
                    // DISABLED_MGRS_CONT: SimulParm.InitSkillMap(info);
                    // DISABLED_MGRS_CONT: StartCoroutine(startGame());
                    
                // DISABLED_MGRS_CONT: }, ((ErrorResource er) =>
                // DISABLED_MGRS_CONT: {
                    // DISABLED_MGRS_CONT: Debug.Log("게임상수 Fail : " + er.code);
                    
                // DISABLED_MGRS_CONT: }));            
        }

        private IEnumerator startGame()
        {
            // DISABLED_MGRS: DefineEnum.EGameMode mode = Mgrs.userData.GetUserGameMode();

            // DISABLED_MGRS: string autoGame = Mgrs.LocalManager.LoadLastAutoGame();
            if (autoGame == null) autoGame = "auto";
            ////Debug.Log("============================>>autoGame = " + autoGame);

            // DISABLED_MGRS: Mgrs.ManagerSupervise(true);
            if (mode == DefineEnum.EGameMode.Season || mode == DefineEnum.EGameMode.SeasonConsecutive)
            {
                startLoading(autoGame);
            }
            /*else if (mode == DefineEnum.EGameMode.Rank)
            {
                startRankingLoading(autoGame);
            }*/
            else if (mode == DefineEnum.EGameMode.LiveMatch)
            {
                startLiveMatchLoading();
            }
            else if (mode == DefineEnum.EGameMode.Walkoff)
            {
                startNineTwoLoading();
            }
            else if (mode == DefineEnum.EGameMode.LeagueRace)
            {
                startRaceLoading(autoGame);
            }

            yield return new WaitForSeconds(0.2f);

            StartCoroutine("StartLoad", "BallPlay");
        }


#endif

        AsyncOperation async;
        bool IsLoadGame = false;
        public IEnumerator StartLoad(string strSceneName)
        {            
            loadingCount = 0;
            lastCounter = -1;

            _active.gameObject.SetActive(true);

            TweenAlpha.Begin(gameObject, 0.5f, 1);

            if (IsLoadGame == false)
            {
                IsLoadGame = true;
                AsyncOperation async = SceneManager.LoadSceneAsync(strSceneName);
                while (async.isDone == false)
                {
                    loadingCount = async.progress * 0.5f;
                    yield return null;
                }
                BallPlayManager manager = GameObject.FindGameObjectWithTag("BallPlayManager").GetComponent<BallPlayManager>();
                manager.gameObject.SetActive(true);
                manager.initGame(gameObject);
            }
        }

        private void loadingTexture()
        {
            do
            {
                tipIndex = 8;// Mathf.Clamp(Random.Range(1, 9), 1, 8);
            } while (tipIndex == lastIndex);
            lastIndex = tipIndex;
            texture.mainTexture = Resources.Load("MainGame/Texture/loading/tip" + tipIndex) as Texture;
            texture.MakePixelPerfect();
        }

        
        // Update is called once per frame
        void Update()
        {
            if (lastCounter != loadingCount)
            {
                setStep();
                lastCounter = loadingCount;
            }
        }


        private void setStep()
        {            
            //float gab = (590.0f / max);
            float width = loadingCount * MAX_GAUGE;
            gauge.width = (int)width;
            //ball.transform.localPosition = new Vector3(width-300, 0, 0);
        }



#if !_Test_Local        

        /// <summary>
        /// 시즌모드 로딩 시작
        /// </summary>
        private void startLoading(string autoGame)
        {            
            // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;
            // DISABLED_MGRS: DefineEnum.EGameMode mode = Mgrs.userData.GetUserGameMode();

            Mode.b2outBaseLoadedMode = false;      
            Mode.gameMode = Mode.GamePlayMode.Season;
            Mode.finalInning = 9;
            Mode.maxInning = 12;

            int myScheSeq = info.myScheNo;
            int[] teamNo = info.schedule[myScheSeq];
            SeasonTeamInfo homeTeam = info.teamInfos[teamNo[0]];
            setStadiumStyle(homeTeam.team);

            if (autoGame.Equals("auto") == true || mode == DefineEnum.EGameMode.SeasonConsecutive)
            {
                //오토
                Mode.PlayTypeFlag = Mode.ModeFlag.Auto;
                Mode.bSimulationQuickPlay = true;                
            }
            else
            {
                Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
                Mode.bSimulationQuickPlay = false;
            }

            Mode.bOnlyChanceMode = false;

            SimulPlayerManager.InitSeasonResult();
            if (info.gameType == SeasonGameType.PennantRace)
            {
                simulateSeasonGames(info);
            }
        }

        /// <summary>
        /// 시즌 모드에서 타팀경기를 시뮬레이션 한다
        /// </summary>
        /// <param name="info"></param>
        /// <param name="bMyGameInculde"></param>
        private void simulateSeasonGames(SeasonGameInfo info, bool bMyGameInculde = false)
        {
            int myScheSeq = info.myScheNo;// gameInfo.myScheSeq;
            foreach (KeyValuePair<int, int[]> value in info.schedule)
            {
                bool bMyHome = false;
                int[] teamNo = value.Value; //0:home, 1:away
                int curSeq = value.Key;
                if (curSeq == myScheSeq)
                {
                    if (bMyGameInculde == false)
                    {
                        //내게임 무시
                        continue;
                    }
                    else
                    {
                        bMyHome = info.home;
                    }
                }
                SeasonTeamInfo homeTeam = info.teamInfos[teamNo[0]];
                SeasonTeamInfo awayTeam = info.teamInfos[teamNo[1]];
                GameLineup lineup = SimulManager.GetSeasonGameLineup(homeTeam, awayTeam);
                SimulManager.SimulateOneGame(bMyHome, lineup, info.starterOdr, info.starterOdr);
                SimulPlayerManager.AddSeasonResult(curSeq, bMyHome);
                SimulPlayerManager.AddPitcherResult();
            }
        }

        private void startLiveMatchLoading()
        {
            // DISABLED_MGRS: LivePlayGameInfo info = Mgrs.userData.livePlayGmaeInfo;

            Mode.b2outBaseLoadedMode = false;
            Mode.bTieBreaker = false;
            Mode.bPvpMode = true;
            Mode.gameMode = Mode.GamePlayMode.Pvp;

            Mode.finalInning = 9; 
            Mode.maxInning = 12;

            LivePlayTeamInfo homeTeam = info.homeTeam;
            setStadiumStyle(homeTeam.team);

            //일단은 테스트용
            //Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            //Mode.bSimulationQuickPlay = false;

            //이게 진짜
            Mode.PlayTypeFlag = Mode.ModeFlag.Auto;
            Mode.bSimulationQuickPlay = true;

            Mode.bOnlyChanceMode = false;

            /*
            if (info.friendly == true)
            {
                Mode.bPvpMode = false;
            }*/

        }

        /// <summary>
        /// 랭킹모드 로딩 시작
        /// </summary>
        private void startRankingLoading(string autoGame)
        {
            // DISABLED_MGRS: RankedPlayGameInfo info = Mgrs.userData.Ingame_rankInfo;            

            Mode.b2outBaseLoadedMode = false;
            Mode.gameMode = Mode.GamePlayMode.Ranking;
            Mode.finalInning = 9;
            Mode.maxInning = 12;

            RankedPlayTeamInfo homeTeam = info.homeTeam;
            setStadiumStyle(homeTeam.team);

            if (autoGame.Equals("auto") == true)
            {
                Mode.PlayTypeFlag = Mode.ModeFlag.Auto;
                Mode.bSimulationQuickPlay = true;                
            }
            else
            {
                Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
                Mode.bSimulationQuickPlay = false;
            }

            Mode.bOnlyChanceMode = false;
        }

        /// <summary>
        /// 9회 2아웃 모드 시작
        /// </summary>
        private void startNineTwoLoading()
        {
            // DISABLED_MGRS: WalkoffPlayGameInfo info = Mgrs.userData.walkoffInfo;

            Mode.b2outBaseLoadedMode = true;
            Mode.gameMode = Mode.GamePlayMode.NineInningTwoOut;
            Mode.finalInning = 9;
            Mode.maxInning = 12;

            
            setStadiumStyle(info.otherTeam);

            Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            Mode.bSimulationQuickPlay = false;  //항상 false                  
            Mode.bOnlyChanceMode = false;
        }


        /// <summary>
        /// 리그쟁탈 시작
        /// </summary>
        private void startRaceLoading(string autoGame)
        {
            // DISABLED_MGRS: WebConnector.RacePlayGameInfo info = Mgrs.userData.raceInfo;

            Mode.b2outBaseLoadedMode = false;
            Mode.gameMode = Mode.GamePlayMode.Race;
            Mode.finalInning = 9;
            Mode.maxInning = 12;


            // DISABLED_MGRS: TeamCode homeTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.homeTeamNo).team;

            setStadiumStyle(homeTeam);

            if (autoGame.Equals("auto") == true)
            {
                Mode.PlayTypeFlag = Mode.ModeFlag.Auto;
                Mode.bSimulationQuickPlay = true;                
            }
            else
            {
                Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
                Mode.bSimulationQuickPlay = false;
            }

            Mode.bOnlyChanceMode = false;
        }
#else
        private void startLoadingLocal()
        {
#if _Local_Balance
            InGameDebug.MYHOME = true;// !MY_BATTING;
            InGameDebug._NO_OUT_COUNT = false;// NO_OUT_STATE;
#endif

            Managers = null;
            Mode.b2outBaseLoadedMode = false;
            Mode.bTieBreaker = false;            
            Mode.gameMode = Mode.GamePlayMode.Season;
            
            Mode.finalInning = 9;
            Mode.maxInning = 12;

            Mode.finalInning = 1; //지워지워
            Mode.maxInning = 1; //지워지워

            Mode.stadiumNum = Mathf.Clamp(TEST_STADIUM_NUM, 1, 6);
            setStadiumStyle(Mode.stadiumNum);

            //SimulPlayerManager.InitSeasonResult();

            //Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            //Mode.bSimulationQuickPlay = false;           //저장값 사용

            Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            Mode.bSimulationQuickPlay = true;// true;           //저장값 사용

            Mode.bOnlyChanceMode = true;            
            
        }


        private void startVersusLoadingLocal()
        {
#if _Local_Balance
            InGameDebug.MYHOME = !MY_BATTING;
            InGameDebug._NO_OUT_COUNT = false;// NO_OUT_STATE;
#endif
            Managers = null;
            Mode.b2outBaseLoadedMode = false;

            Mode.bPvpMode = false;                          //이거 true로 
            Mode.gameMode = Mode.GamePlayMode.Pvp433;       //이거 PVP로 
            Mode.bPvpMode433 = true;

            Mode.finalInning = 9;
            Mode.maxInning = 12;

            int sindex = 4;// pvpmanager.Get().stadiumIndex[PhotonNetwork.isMasterClient ? 0 : 1];
            //Debug.Log("sindex_0 = " + pvpmanager.Get().stadiumIndex[0]);
            //Debug.Log("sindex_1 = " + pvpmanager.Get().stadiumIndex[1]);
            //Debug.Log("stadium num = " + sindex);
            Mode.stadiumNum = sindex;
            Mode.stadiumNum = Mathf.Clamp(sindex, 1, 6);
            
            Mode.stadiumType = (Mode.StadiumType)(Mode.stadiumNum); //setStadiumStyle(Mode.stadiumNum);

            //SimulPlayerManager.InitSeasonResult();
            //Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            //Mode.bSimulationQuickPlay = false;           //저장값 사용

            Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            Mode.bSimulationQuickPlay = false;// true;           //저장값 사용

            Mode.bOnlyChanceMode = false;
            

        }


        private void startRankingLoadingLocal()
        {
            Mode.b2outBaseLoadedMode = false;
            Mode.bTieBreaker = false;      
            Mode.gameMode = Mode.GamePlayMode.Ranking;
            Mode.finalInning = 9;
            Mode.maxInning = 12;

            Mode.stadiumNum = Mathf.Clamp(TEST_STADIUM_NUM, 1, 6); 
            setStadiumStyle(Mode.stadiumNum);

            Mode.PlayTypeFlag = Mode.ModeFlag.Auto;
            Mode.bSimulationQuickPlay = true;                   //고속게임            
            Mode.bOnlyChanceMode = false;
        }

        private void startNineTwoLocal()
        {
            Mode.b2outBaseLoadedMode = true;
            Mode.bTieBreaker = false;      
            Mode.gameMode = Mode.GamePlayMode.NineInningTwoOut;
            Mode.finalInning = 9;
            Mode.maxInning = 12;

            Mode.stadiumNum = Mathf.Clamp(TEST_STADIUM_NUM,1,6);
            setStadiumStyle(Mode.stadiumNum);

            Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
            Mode.bSimulationQuickPlay = false;  //항상 false                  
            Mode.bOnlyChanceMode = false;
        }

#endif

#if _Test_Local
        /// <summary>
        /// 경기장 스타일을 세팅한다
        /// </summary>
        /// <param name="stadium"></param>
        private void setStadiumStyle(int stadium)
        {
        /*    Mode.stadiumType = (Mode.StadiumType)(stadium);
            if (SimulPlayerManager.homeTeamIndex == 6 || SimulPlayerManager.homeTeamIndex == 4)
            {
                Mode.stadiumNum = 1;
            }
            else if (SimulPlayerManager.homeTeamIndex == 2)
            {
                Mode.stadiumNum = 2;
            }
            else if (SimulPlayerManager.homeTeamIndex == 1)
            {
                Mode.stadiumNum = 3;
            }
            else if (SimulPlayerManager.homeTeamIndex == 8)
            {
                Mode.stadiumNum = 4;
            }
            else if (SimulPlayerManager.homeTeamIndex == 9)
            {
                Mode.stadiumNum = 5;
            }
            else if (SimulPlayerManager.homeTeamIndex == 5)
            {
                Mode.stadiumNum = 6;
            }
            else
            {
                Mode.stadiumNum = Random.Range(1,6);
            }
            Mode.stadiumType = (Mode.StadiumType)(Mode.stadiumNum);*/
        }
#else
        /// <summary>
        /// 임시
        /// </summary>
        /// <param name="stadium"></param>
        private void setStadiumStyle(TeamCode teamCode)
        {
            if (teamCode == TeamCode.DOOSAN || teamCode == TeamCode.LG)
            {
                Mode.stadiumNum = 1;        
            }
            else if (teamCode == TeamCode.NEXEN)
            {
                Mode.stadiumNum = 2;        
            }
            else if (teamCode == TeamCode.SAMSUNG)
            {
                Mode.stadiumNum = 3;                
            }            
            else if (teamCode == TeamCode.KIA)
            {
                Mode.stadiumNum = 4;        
            }
            else if (teamCode == TeamCode.HANWHA)
            {
                Mode.stadiumNum = 5;        
            }
            else if (teamCode == TeamCode.SK)
            {
                Mode.stadiumNum = 6;        
            }
            else
            {
                Mode.stadiumNum = 1;        
            }
            Mode.stadiumType = (Mode.StadiumType)(Mode.stadiumNum);            
        }
#endif
    }
}
