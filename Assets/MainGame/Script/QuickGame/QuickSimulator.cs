using Spine.Unity;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BaseBall.BallPlay
{
    public class QuickSimulator : MonoBehaviour
    {
        //
        //private readonly float simulSpeed = 1.0f;

        public enum SimulState
        {
            init,
            simulating,
            simulover,
            gameover,
            nextInning
        }

        public enum SimulPreCalled
        {
            None,
            SingleType,
            VsType
        }

        [SerializeField] SkeletonAnimation bgAnim;
        [SerializeField] PlayerCardInfo[] cardInfo;

        //상태
        public SimulState curState;

        
        //게임 메니저
        private BallPlayManager manager;
        private int myIndex, cpuIndex;
        private bool bMyHome;
        private bool bMyTurn;
        private int[] curLineupCount, lineupCycle;
        //기록
        private QuickGameInfo info;
        private int inning;
        private bool bTopInning;

        //
        //public GameObject _active;

        public GameObject Panel;

        //스코어 보드
        public scoreboard board;

        //선수정보
        public QuickPlayerInfo[] playerInfo;

        //선수바
        public QuickPlayerNameBar[] myLineup;
        public QuickPlayerNameBar[] cpuLineup;

        //게임정보
        public UILabel numPitchLabel;
        public UISprite topBottom;
        public UILabel inningLabel;
        public GameObject arrowObj;
        public UISprite arrow;
        public UISprite[] outSpr;
        //이닝전환
        public GameObject inningChange;
        public GameObject playerObj;
        public UITexture myLogo;
        public UITexture cpuLogo;

        //스킬 연출
        public skillUISetter [] skillUI;
        public vsUISetter vsSetter;

        //콜
        public UIFieldCall callUI;


        //퀵필드
        public quickField qField;
        public GameObject fieldObj;

        //수동자동
        public GameObject topUI;
        public GameObject manual, wait;

        //연속경기
        public GameObject consectiveGame;



        private CPlayer curBatter, lastBatter;
        private CPlayer curPitcher, lastPitcher;
             



        //찬스 팝업
        public GameObject chancePopup;

        //PVP 대기 팝업
        public GameObject waitPopup;

        //재연결 팝업
        public GameObject reconnectPopup;


        //채팅 UI
        public UIChatting chattingUI;




        public QuickRunner[] runnerObj;
        private bool[] bBaseOn;
        private int runnerIndex;
        bool[] runnerActive = new bool[4] { false, false, false, false };
        //private UILabel[] runnerName;
        //private UISprite[] runnerCap;
       

        private int[] awayScore = new int[12];
        private int[] homeScore = new int[12];
        private int[] awayRecord = new int[3];
        private int[] homeRecord = new int[3];


        //모드관련
        private bool bChanceMode;
        private bool[] bChanceFlag;
        private bool bGoodbyeChanceFlag;
        //private int battingIndex;
        private int changeRemain;

        //개입
        private bool bGoToGameFlag;

        //기타
        private int arrowStep;
        private float curTime;
        private int offenseTeamIndex;



        private bool bPausePopup = false;


        //PVP
        private bool bHost;
        private bool bReSync;
        private bool bReconnectAsked;
        private bool bReconnectDone;
        private bool bReconnectProcess;
        public GameObject networkObj;

        private bool[] bPvpChanceFlag;
        private bool[] bOffenseChanceFlag;
        private bool[] bDefenseChanceFlag;




        void Update()
        {
#if UNITY_EDITOR
            if (Mode.bPvpMode == false)
            {
                if (Input.GetKeyDown(KeyCode.Space) == true)
                {
                    setSkip();
                }
            }
#endif

            curTime+=Time.deltaTime;
            if(curTime > 0.3f)
            {
                arrowStep++;
                if (arrowStep > 2) arrowStep = 0;
                arrow.transform.localPosition = new Vector3(-14 + arrowStep * 14, 0, 0);
                curTime = 0;
            }

            checkPvpException();

        }

        /*
        public void setActive(bool bActive)
        {
            _active.SetActive(bActive);
        }*/

        private bool bPitchingChanceOnce;

        public void init(BallPlayManager _manager)
        {
            string[] _skinName = new string[] { "Morning", "Afternoon", "Night" };
            bgAnim.Skeleton.SetSkin(_skinName[Background.TimeIndex]);

            logoValue1 = getRandomArray();
            logoValue2 = getRandomArray();            


            bPausePopup = false;

            lastBatter = null;
            lastPitcher = null;

            chancePopup.gameObject.SetActive(false);
            waitPopup.gameObject.SetActive(false);
            reconnectPopup.gameObject.SetActive(false);
            networkObj.SetActive(false);

            bGoToGameFlag = false;
            //awayPitcherSeq = homePitcherSeq = -1;
            bPitchingChanceOnce = false;
            bChanceMode = false;

            bChanceMode = true;
            bChanceFlag = new bool[4] { false, false, false, false };
            bGoodbyeChanceFlag = false;
            changeRemain = 3;

            curLineupCount = new int[2] { 0, 0 };
            lineupCycle = new int[2] { 0, 0 };
            info = new QuickGameInfo();
            curState = SimulState.init;
            //setActive(true);
            this.manager = _manager;

            

            myIndex = (manager.bMyHome == true ? 1 : 0);
            cpuIndex = 1 - myIndex;
            bMyHome = manager.bMyHome;
            bMyTurn = (bMyHome ? false : true);
            inning = 1;
            bTopInning = true;

            //로고세팅
            // DISABLED_MGRS: myLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(SimulPlayerManager.myTeamIndex))));
            // DISABLED_MGRS: cpuLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(SimulPlayerManager.cpuTeamIndex)))); 

            myLogo.mainTexture = KOBManager.Resource.LoadLogoTemp(SimulPlayerManager.myTeamIndex);
            cpuLogo.mainTexture = KOBManager.Resource.LoadLogoTemp(SimulPlayerManager.cpuTeamIndex);


            inningChange.SetActive(false);

            //스코어 보드 초기화
            board.initScoreBoard(SimulPlayerManager.strAwayTeam, SimulPlayerManager.strHomeTeam, SimulPlayerManager.awayTeamIndex, SimulPlayerManager.homeTeamIndex);

            //이닝정보 초기화
            updateInningInfo();
                        
            //주자 UI초기화
            initRunnerUI();

            //재접속 요구
            bReconnectAsked = false;
            //
            bReconnectDone = false;
            //
            bReconnectProcess = false;
            //재 동기화 
            bReSync = false;

            if (Mode.bPvpMode == true)
            {
                Max_Wait_Time = 15;
                bHost = bMyHome ? false : true; //원정일경우 호스트 -> 우선은 이걸 기본으로
                //bHost = bMyHome ? true : false; // 홈인경우 호스트
                topUI.SetActive(false);

                bPvpChanceFlag = new bool[2] { false, false };
                bOffenseChanceFlag = new bool[2] { false, false };
                bDefenseChanceFlag = new bool[2] { false, false };

                chattingUI.gameObject.SetActive(true);
                chattingUI.Init(manager);
            }
            else
            {
                topUI.SetActive(true);
                chattingUI.gameObject.SetActive(false);
            }

        }

        /// <summary>
        /// 현재 대결중인 투타 선수들 가지고 온다
        /// </summary>
        private void setCurPlayers(int curIndex)
        {
            curBatter = SimulPlayerManager.GetBatter(curIndex);
            curPitcher = SimulPlayerManager.GetPitcher(1 - curIndex);

            int lineup = curLineupCount[curIndex];
            int idx = curLineupCount[curIndex] + (curIndex * 10);
            int logo = curIndex == 0 ? logoValue1[lineup] : logoValue2[lineup];
            cardInfo[curIndex].SetInfo(idx+1, curBatter.getName(), logo);
            cardInfo[1 - curIndex].SetInfo(curIndex==0?10:20, curPitcher.getName(), curIndex == 0 ? logoValue1[9] : logoValue2[9]);

        }

        /// <summary>
        /// 이닝시작시 게임정보를 업데이트
        /// </summary>
        private void updateInningInfo()
        {
            offenseTeamIndex = bMyTurn ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex;
            int curIndex = bTopInning ? awayIndex() : homeIndex();
            setCurPlayers(curIndex);

            if (bMyTurn == true)
            {
                playerInfo[0].setBatter(curBatter);
                playerInfo[1].setPitcher(curPitcher);
                for (int i = 0; i < 9; i++)
                {
                    CPlayer offense = SimulPlayerManager.GetFielder(0, i);
                    myLineup[i].setInit(offense, 0, true);

                    if (i == 0)
                    {
                        //투수세팅
                        cpuLineup[0].setInit(curPitcher, 1, false);
                        cpuLineup[0].setFocus(true);
                    }
                    CPlayer defense = SimulPlayerManager.GetFielder(1, i);
                    int pos = defense.getCurPos();
                    if (pos >= 0 && pos <= CPlayer._RIGHTFIELDER)
                    {
                        cpuLineup[pos].setInit(defense, 1, false);
                    }

                }
                myLineup[curLineupCount[curIndex]].setFocus(true);
            }
            else
            {
                playerInfo[1].setBatter(curBatter);
                playerInfo[0].setPitcher(curPitcher);
                for (int i = 0; i < 9; i++)
                {
                    CPlayer offense = SimulPlayerManager.GetFielder(1, i);
                    cpuLineup[i].setInit(offense, 1, true);

                    if (i == 0)
                    {
                        //투수세팅
                        myLineup[0].setInit(curPitcher, 0, false);
                        myLineup[0].setFocus(true);
                    }
                    
                    CPlayer defense = SimulPlayerManager.GetFielder(0, i);
                    int pos = defense.getCurPos();
                    if (pos != CPlayer._DH && pos < 9)
                        myLineup[pos].setInit(defense, 0, false);
                }
                cpuLineup[curLineupCount[curIndex]].setFocus(true);
            }

            topBottom.spriteName = (bTopInning ? "inningchange_top" : "inningchange_bottom");
            inningLabel.text = inning.ToString();
            arrowObj.transform.localScale = new Vector3((bMyTurn ? 1 : -1), 1, 1);

            lastBatter = curBatter;
            lastPitcher = curPitcher;

            updateGameInfo();
        }

        /// <summary>
        /// 동적 UI할당
        /// </summary>
        /// <param name="uiName"></param>
        private void LoadDynamicUI(string uiName, float scale, float timeRemain , Vector3 pos)
        {
            GameObject uiObj = Util.Load("MainGame/prefabs/dynamicUI/" + uiName, Panel.transform, pos);
            uiObj.transform.localScale = new Vector3(scale, scale, scale);
            Destroy(uiObj, timeRemain);
        }

        /// <summary>
        /// 게임정보를 업데이트 한다
        /// </summary>
        private void updateGameInfo()
        {                        
            if(lastBatter != curBatter)
            {
                playerInfo[bMyTurn ? 0 : 1].setBatter(curBatter);
                lastBatter = curBatter;
            }
            if(lastPitcher != curPitcher)
            {
                playerInfo[bMyTurn ? 1 : 0].setPitcher(curPitcher);
                lastPitcher = curPitcher;
            }

            playerInfo[bMyTurn ? 1 : 0].pitcherStatmina(curPitcher);

            numPitchLabel.text = curPitcher.getStat(Param.ST_PNP).ToString();

            for(int i=0; i<2;i++)
            {
                outSpr[i].gameObject.SetActive((i < info.curOutCount ? true : false));
            }

            //스코어보드
            boardSetting();
        }

        /// <summary>
        /// 스코어 보드 세팅
        /// </summary>
        private void boardSetting()
        {            
            if (info.gameInfo != null)
            {
                int away = awayIndex();
                int home = homeIndex();
                for (int i = 0; i < 12; i++)
                {
                    awayScore[i] = info.gameInfo.inningScore[away, i];
                    homeScore[i] = info.gameInfo.inningScore[home, i];
                }
                awayRecord[0] = info.gameInfo.run[away];
                awayRecord[1] = info.gameInfo.hit[away];
                awayRecord[2] = info.gameInfo.error[away];

                homeRecord[0] = info.gameInfo.run[home];
                homeRecord[1] = info.gameInfo.hit[home];
                homeRecord[2] = info.gameInfo.error[home];

                board.setPlaying(inning, bTopInning, awayScore, homeScore, awayRecord, homeRecord);
            }
        }
        

        /// <summary>
        /// 주자 UI초기화
        /// </summary>
        private void initRunnerUI()
        {
            bBaseOn = new bool[3] { false, false, false };
            /*runnerName = new UILabel[4];
            runnerCap = new UISprite[4];

            for (int i = 0; i < 4; i++)
            {
                runnerName[i] = runnerObj[i].transform.FindChild("Label").GetComponent<UILabel>();
                runnerCap[i] = runnerObj[i].GetComponent<UISprite>();
            }*/

            //
            runnerInningChange();
            //
            setRunner(SimulParm.HOMEBASE_INDEX, curBatter.getName());
        }

        private void runnerInningChange()
        {
            runnerIndex = 0;
            System.Array.Clear(runnerActive, 0, 4);
            
            for (int i = 0; i < 4; i++)
            {
                runnerObj[i].deActive();
                //runnerCap[i].spriteName = "minimap_team" + offenseTeamIndex;
            }
        }

        private int getAvailableIndex()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == false)
                {
                    return i;
                }
            }
            return 0;
        }


        private void setRunner(int curPos, string name)
        {
            //runnerObj[index].gameObject.SetActive(true);
            //runnerName[index].text = name;
            runnerIndex = getAvailableIndex(); 
            //Debug.Log("시뮬레이터==========================>> runnerIndex = " + runnerIndex + " 현재위치 " + curPos + "    name = " + name);
            runnerObj[runnerIndex].gameObject.SetActive(true);
            runnerObj[runnerIndex].init(curPos, name, offenseTeamIndex);
        }


        private int awayIndex()
        {
            return bMyHome ? 1 : 0;
        }

        private int homeIndex()
        {
            return bMyHome ? 0 : 1;
        }




        public void continueSimul(bool inningChange, bool simulatorChangeInning)
        {
            //Application.targetFrameRate = 45;
            Mode.bOnlyChanceMode = false;
            bGoToGameFlag = false;


            if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                if (PvpManager.connectState == PvpManager.ConnectState.DisConnectMine)
                {
                    //PVP모드에서 내가 끊긴 경우 재연결 팝업 
                    setReconnectPopup();
                    return;
                }
            }

            checkConsetiveGame();


            if (inningChange == true)
            {
                if (simulatorChangeInning == true)
                {
                    ////Debug.Log("======================>>시뮬레이터 단에서  이닝 체인지 해줌");
                    SimulManager.SimulChangeInning(true);
                }
                changeInningPre();
                changeInning(false);
            }
            else
            {
                //이닝 동기화
                bTopInning = manager.bTopInning;
                bMyTurn = manager.bMyTurn;
                inning = manager.nInningCount;
                /*살펴봐
                team[bTopInning ? 0 : 1].gameObject.SetActive(true);
                team[bTopInning ? 1 : 0].gameObject.SetActive(false);*/
                setSyncRunner();
            }

            //보드 동기화
            if (info.gameInfo != null)
            {
                int away = awayIndex();
                int home = homeIndex();
                for (int i = 0; i < 12; i++)
                {
                    awayScore[i] = info.gameInfo.inningScore[away, i];
                    homeScore[i] = info.gameInfo.inningScore[home, i];
                }
                board.boardActiveByCurrentInning(inning, bTopInning, awayScore, homeScore);
            }

            if (Mode.bPvpMode == true)
            {
                IngameUI.GetEmoticonChatting().gameObject.SetActive(false); //인게임 채팅창 닫음
                chattingUI.simulResetTimer();
                bLastChancePlayed = true;
                PvpManager.bWaitStateQuit = true;
                StartCoroutine(waitNextUpdate(true));
            }
            else
            {
                StartCoroutine(updater());//0.2f));
            }

            CameraManager.GetInstance().CameraOff();
            IngameUI.GetInstance().gameObject.SetActive(false);
        }




#if UNITY_EDITOR
        /// <summary>
        /// 스킵 - 테스트용
        /// </summary>
        public void setSkip()
        {
            SimulManager.GameSimulate();
            curState = SimulState.gameover;
        }
#endif

        /// <summary>
        /// 개입
        /// </summary>
        public void goToGame()
        {
            if (bGoToGameFlag == false)
            {
                if (Mode.PlayTypeFlag == Mode.ModeFlag.Auto)
                {
                    //Debug.Log("=======>>>수동으로 전환");
                    Mode.PlayTypeFlag = Mode.ModeFlag.Manual;
                    bGoToGameFlag = true;
                    setWait(true);
#if !_Test_Local
                    // DISABLED_MGRS: Mgrs.LocalManager.SaveLastAutoGame("action");
#endif
                }
            }
        }


        private void setWait(bool bActive)
        {
            wait.gameObject.SetActive(bActive);
            manual.GetComponent<Collider>().enabled = !bActive;
        }

        private int [] skillUseCount = new int[2];

        private float skillDisplay(SimulSkillInfo skillInfo, int curIndex, SkillUseStep step)
        {
            int defenseIndex = 1 - curIndex;
            float delay = 0;
            if (skillInfo.vsType != VsResult.None)
            {
                //skillUI[defenseIndex].init((int)skillInfo.defenseID, skillInfo.defenseRank);
                //skillUI[curIndex].init((int)skillInfo.offenseID, skillInfo.offenseRank);
                //투타 대결
                bool bOffenseWin = (skillInfo.vsType == VsResult.OffenseWin ? true : false);
                //skillUI
                vsSetter.init(0, bMyTurn, (int)skillInfo.offenseID, skillInfo.offenseRank, (int)skillInfo.defenseID, skillInfo.defenseRank, bOffenseWin);
                //버프 UI
                showBuff((bOffenseWin ? skillInfo.offenseID : skillInfo.defenseID), bOffenseWin, true);
                //delay
                delay += 3;// 3;
            }
            else
            {
                bool bSameTime = (skillInfo.defenseID != SkillID.None && skillInfo.offenseID != SkillID.None ? true : false);
                bool bNoUse = false;

                if (skillInfo.catcherID != SkillID.None)
                {
                    //버프 UI
                    if (skillInfo.catcherID == SkillID.do_bal_ggun) showBuff(skillInfo.catcherID, false); //도발꾼
                    //캐처
                    skillUI[defenseIndex].init((int)skillInfo.catcherID, skillInfo.catcherRank);
                    delay += 1.5f;// 1.5f;
                }
                if (skillInfo.defenseID != SkillID.None)
                {
                    if (skillInfo.defenseID == SkillID.mea_hog || skillInfo.defenseID == SkillID.hoe_sim_il_gyeog) bSameTime = false; //매의눈, 회심은 동타임에서도 출력

                    if ((skillInfo.defenseID == SkillID.mea_hog && info.MeahogShow == false) ||
                        (skillInfo.defenseID == SkillID.hoe_sim_il_gyeog && info.HeosimShow == false))
                    {
                        bNoUse = true;
                    }

                    if(bNoUse == false)
                    {
                        if (bSameTime == false || bMyTurn == false)
                        {
                            //버프 UI
                            showBuff(skillInfo.defenseID, false);
                            //투수 or 수비
                            skillUI[defenseIndex].init((int)skillInfo.defenseID, skillInfo.defenseRank);
                            if (step != SkillUseStep.Fielding) delay += 1.5f;// 1.5f;
                        }
                    }
                }
                if (skillInfo.offenseID != SkillID.None)
                {
                    if (skillInfo.offenseID == SkillID.mea_nun && info.MeaNoonShow == false) bNoUse = true;

                    if (bNoUse == false)
                    {
                        if (bSameTime == false || bMyTurn == true)
                        {
                            //버프 UI
                            showBuff(skillInfo.offenseID, true);
                            //타자 or 주자
                            skillUI[curIndex].init((int)skillInfo.offenseID, skillInfo.offenseRank);
                            if (step != SkillUseStep.Fielding) delay += 1.5f;// 1.5f;
                        }
                    }
                }
            }
            return delay;
        }


        private void showBuff(SkillID id, bool bOffense, bool vs = false)
        {
            int index2;
            CPlayer player;
            if (bMyTurn == true)
            {
                if (bOffense == true)
                {
                    player = curBatter;
                    index2 = 0;
                }
                else
                {
                    player = curPitcher;
                    index2 = 1;
                }
            }
            else
            {
                if (bOffense == true)
                {
                    player = curBatter;
                    index2 = 1;
                }
                else
                {
                    player = curPitcher;
                    index2 = 0;
                }
            }
            playerInfo[index2].activateSkill(player, (int)id);
            
            SkillBuffType type = SkillParm.GetBuffType(id);
            if (type != SkillBuffType.None)
            {
                int index = 0;//bMyTurn
                if(bMyTurn) index = bOffense?0:1;
                else index = bOffense?1:0;
                if (type == SkillBuffType.PitcherDown || type == SkillBuffType.BatterDown) index = 1 - index; 
                Transform trans = playerInfo[index].playerCard.transform;
                quickBuffUI obj = Util.Load("MainGame/prefabs/QuickUI/quickBuffPrefab", trans, new Vector3(0,-12,0)).GetComponent<quickBuffUI>();
                obj.Init(type, index == 0 ? true : false, (vs == true ? 4: skillUseCount[index]));
                skillUseCount[index]++;
            }
        }


        private float callDisplay(SimulBattingData battingData)
        {
            SimulResultState result = battingData.result;
            float delay = 0.6f;
            if (result == SimulResultState.HomeRun)
            {                
                qField.Active(battingData, callUI);
                delay = 2.0f;
            }            
            else if (result == SimulResultState.StrikeOut)
            {
                callUI.Call("strikeout");
            }
            else if (result == SimulResultState.FourBall)
            {
                callUI.Call("baseonball");
            }
            else if (result >= SimulResultState.CatchError && result <= SimulResultState.ThrowError)
            {
                //표현 안함
                delay = 0.5f;
            }
            else
            {
                qField.Active(battingData, callUI);
                delay = 1;// 2.5f;
            }
            return delay;

        }

        //타자교체 이벤트 처리
        private void batterChangeEvent(int curIndex)
        {
            playerInfo[curIndex].ChangeEvent(true);
            int index = curLineupCount[bMyTurn?0:1];
            if (bMyTurn == true)
            {   
                myLineup[index].setInit(curBatter, 0, true);
                myLineup[index].setFocus(true);
            }
            else
            {
                cpuLineup[index].setInit(curBatter, 1, true);
                cpuLineup[index].setFocus(true);
            }
        }

        //투수교체 이벤트 처리
        private void pitcherChangeEvent(int curIndex)
        {
            playerInfo[1 - curIndex].ChangeEvent(false);
            if (bMyTurn == false)
            {
                myLineup[0].setInit(curPitcher, 0, false);
                myLineup[0].setFocus(true);
            }
            else
            {
                cpuLineup[0].setInit(curPitcher, 1, false);
                cpuLineup[0].setFocus(true);
            }
        }


        private IEnumerator updater()//float delay)
        {
            if (manager.bPlayBallEvent == false)
            {
                LoadDynamicUI("playballPrefab", 100, 1.5f, new Vector3(-63,-47,0));
                manager.bPlayBallEvent = true;
                yield return new WaitForSeconds(1.5f);
            }

            if (bPausePopup == true)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            if (bChanceMode == true)
            {
                if (checkChance() == true || bGoToGameFlag == true)
                {
                    StartCoroutine(setChancePopup());
                    yield break;
                }
            }

            SimulBattingData battingResultData;

            SimulManager.SimulationBatting(true);
            battingResultData = SimulManager.GetBattingResult();
            ////Debug.Log("========================>>>>이닝 " + info.currentInning + " battingResultData = " + battingResultData.result);
            SimulManager.SetQuickgameInfo(info);

            //인덱스
            int curIndex = bTopInning ? awayIndex() : homeIndex();

            //선수 교체 - 타자교체
            if (curBatter.bChangeIn == true)
            {
                curBatter.bChangeIn = false;
                batterChangeEvent(curIndex);                
            }
            //선수 교체 - 투수교체
            if (curPitcher.bChangeIn == true)
            {
                curPitcher.bChangeIn = false;
                pitcherChangeEvent(curIndex);
            }
            //yield return new WaitForSeconds(1.0f);
            yield return new WaitForSeconds(0.1f);

            //도루 연출
            if (battingResultData.stealState != SimulStealState.NONE)
            {
                SimulStealState stealState = battingResultData.stealState;
                //도루 스킬
                yield return new WaitForSeconds(setStealSkill(stealState, curIndex));
                //도루 딜레이
                yield return new WaitForSeconds(setRunnerSteal(stealState));
            }
                        
            //현재 발동된 스킬 옅출
            skillUseCount[0] = skillUseCount[1] = 0;
            foreach (var step in (SkillUseStep[])System.Enum.GetValues(typeof(SkillUseStep)))
            {
                if (info.skillInfo.ContainsKey(step) == true)
                {
                    //배팅뷰 -> 피칭 -> 필드 순차적으로 스킬 연출
                    yield return new WaitForSeconds(skillDisplay(info.skillInfo[step], curIndex, step));
                }
            }
            


            //주자 연출
            setRunnerUI(battingResultData);


            //필드뷰 공 궤적 연출 & 콜
            float callDelay = callDisplay(battingResultData);
            if (callDelay > 0) yield return new WaitForSeconds(callDelay);


            
            //라인업 처리            
            nextLineup(curIndex, battingResultData.result);

            


            //결과 및 뒷처리
            if (info.bGameEnd == true)
            {
                ////UnityEngine.//Debug.Log("===========================>> 패스트 시뮬 끝끝");
                curState = SimulState.gameover;
            }
            else
            {
                //찬스체크
                if (info.bInningEnd == true)
                {
                    changeInningEvent();
                    yield return new WaitForSeconds(1.65f);
                                        
                    SimulManager.SimulChangeInning(true);
                    bool vsSkill = SimulManager.SetBattingviewSkill();
                    manager.simulCalled = (vsSkill ? SimulPreCalled.VsType : SimulPreCalled.SingleType);
                    
                    changeInning(true);
                    yield return new WaitForSeconds(0.6f);
                }
                //라인업 업데이트
                SimulManager.SetQuickgameInfo(info);

                //PVP모드에서 재접속 여부
                if (Mode.gameMode == Mode.GamePlayMode.Pvp)
                {
                    if (bReconnectAsked == true)
                    {
                        reconnectAskedFinish();                        
                        yield break;
                    }
                }
                
                //찬스 아닌경우
                StartCoroutine(updater());//simulSpeed));
            }
        }




        /// <summary>
        /// 다음 라인업 세팅
        /// </summary>
        /// <param name="curIndex"></param>
        /// <param name="result"></param>
        public void nextLineup(int curIndex, SimulResultState result)
        {            
            //카운트 처리 및 게임종료, 이닝 전환 처리
            bool bChangeCheck = true;
            if (Mode.bPvpMode == true && bHost == false) bChangeCheck = false;
            bool vsSkill = SimulManager.SimulNextBatter(bChangeCheck);
            manager.simulCalled = (vsSkill ? SimulPreCalled.VsType : SimulPreCalled.SingleType); //액션 엔진에서 스킬의 중복 호출을 방지하기위한 미봉책
            int lastLineup = curLineupCount[curIndex];
            curLineupCount[curIndex]++;
            if (curLineupCount[curIndex] > 8)
            {
                curLineupCount[curIndex] = 0;
                lineupCycle[curIndex]++;
            }

            bool bShake  = false;
            if (bMyTurn == true)
            {
                //myLineup[lastLineup].setFocus(false);
                bShake = myLineup[lastLineup].setHitFlag(result, myLineup[curLineupCount[curIndex]]);
                //myLineup[curLineupCount[curIndex]].setFocus(true);
            }
            else
            {
                //cpuLineup[lastLineup].setFocus(false);
                bShake = cpuLineup[lastLineup].setHitFlag(result, cpuLineup[curLineupCount[curIndex]]);
                //cpuLineup[curLineupCount[curIndex]].setFocus(true);
            }

            if (bShake == true)
            {
                StartCoroutine(fieldShake());
            }

            setCurPlayers(curIndex);
            setRunner(SimulParm.HOMEBASE_INDEX, curBatter.getName());
            updateGameInfo();

            //타자 스킬 초기화
            playerInfo[curIndex].initSkillEffect();
            playerInfo[1 - curIndex].updatePitcher(curPitcher);
            
        }

        private IEnumerator fieldShake()
        {
            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                if (count == 0) fieldObj.transform.localPosition = new Vector3(3f, -197, 0);
                else if (count == 0) fieldObj.transform.localPosition = new Vector3(0, -197 + 6, 0);
                else fieldObj.transform.localPosition = new Vector3(-3f, -197, 0);
                yield return new WaitForEndOfFrame();
                count++;
                if (count > 2) count = 0;
            }
            yield return new WaitForEndOfFrame();
            fieldObj.transform.localPosition = new Vector3(0, -197, 0);
        }

        /// <summary>
        /// 액션 상태에서 다음 라인업 세팅
        /// </summary>
        /// <param name="result"></param>
        public void NextLineupFromGame(SimulResultState result)
        {            
            int curIndex = bTopInning ? awayIndex() : homeIndex();
            nextLineup(curIndex, result);
        }


        private float setStealSkill(SimulStealState state, int curIndex)
        {
            float delay = 0.01f;
            
            if (state == SimulStealState.Fail_Skill)
            {
                //앉아쏴 발동
                skillUI[1 - curIndex].init((int)SkillID.su_bi_hyung_po_su, 3);
                delay += 1.5f;
            }
            else if (state == SimulStealState.Success_Skill)
            {
                //대도 발동
                skillUI[curIndex].init((int)SkillID.jil_ju_bon_neung, 3);
                delay += 1.5f;
            }
            else if (state == SimulStealState.VsSkill_CatcherWin || state == SimulStealState.VsSkill_RunnerWin)
            {
                //대도 vs 앉아쏴 
                bool bOffenseWin = (state == SimulStealState.VsSkill_RunnerWin ? true : false);
                vsSetter.init(0, bMyTurn, (int)SkillID.jil_ju_bon_neung, 3, (int)SkillID.su_bi_hyung_po_su, 3, bOffenseWin);
                delay += 3;
            }
            else if (state == SimulStealState.PickOffLaserOut)
            {
                //견제왕 발동
                skillUI[1 - curIndex].init((int)SkillID.gyeun_we_wang, 3);
                delay += 1.5f;
            }
            else if (state == SimulStealState.PickOffVsSafe || state == SimulStealState.PickOffVsOut)
            {
                //주루센스 vs 견제왕
                bool bOffenseWin = (state == SimulStealState.PickOffVsSafe ? true : false);
                vsSetter.init(0, bMyTurn, (int)SkillID.ju_lu_sense, 3, (int)SkillID.gyeun_we_wang, 3, bOffenseWin);
                delay += 3;
            }

            return delay;
        }

        /// <summary>
        /// 주자가 도루 하는 상태 세팅
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private float setRunnerSteal(SimulStealState state)
        {
            //bool bStealSuccess = false;
            float delay = 2.0f;

            int index = -1;
            for (int i = 0; i < 4; i++)
            {
                if (runnerObj[i].bActive == true)
                {
                    if (runnerObj[i].curPos == FieldParm.FIRSTBASE_INDEX)
                    {
                        index = i;
                        break;
                    }
                }
            }
            if (index == -1) return 0;

           
            if (state == SimulStealState.PickOffVsSafe)
            {
                //견제 실패
                runnerObj[index].pickOffRunner(QuickRunner.BaseRunningType.PickOffSafe);
            }
            else if (state == SimulStealState.PickOffOut || state == SimulStealState.PickOffLaserOut || state == SimulStealState.PickOffVsOut)
            {
                //견제성공
                runnerObj[index].pickOffRunner(QuickRunner.BaseRunningType.PickOffOut);
                runnerActive[index] = false;
            }
            else if (state == SimulStealState.Success_Skill || state == SimulStealState.Success || state == SimulStealState.VsSkill_RunnerWin)
            {
                //도루성공 연출
                runnerObj[index].moveRunner(FieldParm.SECONDBASE_INDEX, true, QuickRunner.BaseRunningType.StealSafe);
            }
            else
            {
                //도루실패 연출
                runnerObj[index].moveRunner(FieldParm.SECONDBASE_INDEX, true, QuickRunner.BaseRunningType.StealOut);
                runnerActive[index] = false;
            }

            return delay;
        }

        /// <summary>
        /// 주자 UI세팅
        /// </summary>
        /// <param name="data"></param>
        private void setRunnerUI(SimulBattingData data)
        {
            for (int i = 0; i < 4; i++) runnerActive[i] = data.bRunnerActive[i];

            for (int i = 0; i < 3; i++)
            {
                //runnerObj[i].gameObject.SetActive(false);
                bBaseOn[i] = false;
            }

            for (int i = 0; i < 4; i++)
            {
                if (data.bRunnerActive[i] == true)
                {                    
                    int curBase = data.runnerCurPos[i];                    
                    ////Debug.Log("=================>> index = " + i + "===================>> 목적지 : " + curBase);                    
                    if (curBase != 3) bBaseOn[curBase] = true;                    
                    runnerObj[i].moveRunner(curBase);
                }
                else
                {
                    if (data.runnerValue[i] == (int)RunnerState.Score)
                    {
                        //득점
                        ////Debug.Log("=================>> 주자 득점");
                        runnerObj[i].moveRunner(FieldParm.HOMEBASE_INDEX);
                    }
                    else if (data.runnerValue[i] == (int)RunnerState.AssistOut3B)
                    {
                        //득점
                        ////Debug.Log("=================>> 주자 득점");
                        runnerObj[i].moveRunner(FieldParm.THIRDBASE_INDEX, true, QuickRunner.BaseRunningType.OnemoreBaseOut);
                    }
                    else if (data.runnerValue[i] == (int)RunnerState.AssistOutHB)
                    {
                        //득점
                        ////Debug.Log("=================>> 주자 득점");
                        runnerObj[i].moveRunner(FieldParm.HOMEBASE_INDEX, true, QuickRunner.BaseRunningType.OnemoreBaseOut);
                    }
                    else
                    {
                        //주루사
                        runnerObj[i].deadRunner(info.curOutCount);
                    }
                }
            }
        }

        /// <summary>
        /// 게임 엔진으로 부터 퀵시뮬레이터 주자 싱크
        /// </summary>
        /// <param name="manager"></param>
        private void setSyncRunner()
        {            
            /*
            for (int i = 0; i < 3; i++)
            {
                bool bActive = manager.field.run .bOnBase[i];
                runnerObj[i].gameObject.SetActive(bActive);
                bBaseOn[i] = bActive;
                if (bActive == true)
                {
                    Runner runner = manager.field.run.getRunner(i);
                    setRunner(i, runner.pRunner.getName());
                }
            }*/

            for (int i = 0; i < 3; i++) bBaseOn[i] = false;
            for (int i = 0; i < 4; i++)
            {
                runnerActive[i] = false;
                runnerObj[i].deActive();
            }

            for (int i = 0; i < 4; i++)
            {
                if (manager.field.run.runnerActive[i] == true)
                {
                    Runner runner = manager.field.run.runner[i];
                    if (runner != null)
                    {
                        int curBase = runner.currentPos;
                        if (curBase == FieldParm.HOMEBASE_INDEX)
                        {
                            setRunner(curBase, manager.batter.pBatter.getName());
                            runnerActive[i] = true;
                        }
                        else
                        {
                            bBaseOn[curBase] = true;
                            setRunner(curBase, runner.pRunner.getName());
                            runnerActive[i] = true;
                        }

                    }
                }
            }


        }


        /// <summary>
        /// 라인업 싱크
        /// </summary>
        private void syncLineup()
        {
            int team = bMyTurn ? 0 : 1;
            SimulPlayerManager.SetLineup(team, curLineupCount[team]);


            Debug.Log("주요 플래그 동기화 여부");
            Debug.Log("bMyTurn = " + bMyTurn);
            Debug.Log("bMyHome = " + bMyHome);
            Debug.Log("bTopInning = " + bTopInning);
            manager.bMyTurn = bMyTurn;
            manager.bMyHome = bMyHome;
            manager.bTopInning = bTopInning;

            //투수의 손 동기화 시켜
            curPitcher = SimulPlayerManager.GetPitcher(1 - team);
            manager.pitcher.syncHand(curPitcher);
        }


        private void changeInningEvent()
        {
            changeInningPre();

            playerObj.SetActive(false);
            inningChange.SetActive(true);
            Util.SetAnimation(Panel.GetComponent<Animator>(), "changeAnim");
        }

        

        private void changeInningPre()
        {
            //이닝 전환            
            if (checkEndGame() == true)
            {
                //끝
                //Debug.Log("=================>>게임 오버1");
                if (Mode.bPvpMode == true)
                {
                    PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.GameEnd);
                }
                curState = SimulState.gameover;
                StopAllCoroutines();
            }
            else
            {
                //bPvpChanceFlag = false; //나중에 지워

                if (bTopInning == false) inning++;
                inningChangeState();
                runnerInningChange();
            }
        }

        /// <summary>
        /// 이닝 체인지
        /// </summary>
        private void inningChangeState()
        {
            bTopInning = !bTopInning;
            bMyTurn = !bMyTurn;
            topBottom.spriteName = (bTopInning ? "inningchange_top" : "inningchange_bottom");
            inningLabel.text = inning.ToString();
            arrowObj.transform.localScale = new Vector3((bMyTurn ? 1 : -1), 1, 1);
        }


        private void changeInning(bool bAnim)
        {   
            updateInningInfo();
            
            Panel.GetComponent<Animator>().enabled = false;                        
            inningChange.SetActive(false);
            if (bAnim == true)
            {
                playerObj.GetComponent<UIWidget>().alpha = 0;
                playerObj.SetActive(true);
                TweenAlpha.Begin(playerObj, 0.15f, 1);
                playerInfo[0].setAnim(true);
                playerInfo[1].setAnim(false);
                playerInfo[bMyTurn ? 0 : 1].SetLight(0.15f);
            }
            else
            {
                playerObj.GetComponent<UIWidget>().alpha = 1;
                playerObj.SetActive(true);
                playerInfo[0].initPos(true);
                playerInfo[1].initPos(false);
            }

            runnerInningChange();
            setRunner(SimulParm.HOMEBASE_INDEX, curBatter.getName());
        }


        private bool checkChance()
        {
            int index = Mathf.Clamp((inning-1) / 3 ,0 ,3);

            //진짜
            if (bChanceFlag[index] == false)
            {
                if (checkCase() == true)
                {
                    bChanceFlag[index] = true;
                    if (bMyTurn == false)
                    {
                        bPitchingChanceOnce = true;
                    }
                    Mode.bOnlyChanceMode = true;
                    return true;
                }                
            }//복원

            /*
            ////////////////////////////////////////////////////////////////////////////////
            //2아웃 테스트용
            ////////////////////////////////////////////////////////////////////////////////
            if (bChanceFlag[index] == false)
            {
                //if (bPitchingChanceOnce == false) 
                {
                    if (bMyTurn == true)//if (bMyTurn == false)
                    {
                        if (info.curOutCount == 2)
                        {
                            bChanceFlag[index] = true;
                            //Debug.Log("======================>>테스트용 bMyTurn " + bMyTurn + "  bPitchingChanceOnce = " + bPitchingChanceOnce);
                            //bPitchingChanceOnce = true;
                            return true;
                        }
                    }
                }
            }//여기까지*/


            //끝내기 찬스 - 진짜 추가
            if (checkGoodbye() == true)
            {
                return true;
            }
            
            return false;
        }


        private bool checkGoodbye()
        {
            if (bGoodbyeChanceFlag == false)
            {
                if (inning >= 9)
                {
                    int gab = info.run[0] - info.run[1];
                    if (info.curOutCount >= 2)
                    {
                        if (bBaseOn[1] || bBaseOn[2])
                        {
                            if (bMyTurn)
                            {
                                if (gab >= -2 && gab <= 0)
                                {
                                    bGoodbyeChanceFlag = true;
                                    return true;
                                }
                            }
                            else
                            {
                                if (gab <= -2 && gab > 0)
                                {
                                    bGoodbyeChanceFlag = true;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool checkCase()
        {
            if (bMyTurn == false && bPitchingChanceOnce == true)
            {
                return false;
            }

            
            int per = 0;
            if (bBaseOn[0] && bBaseOn[1] && bBaseOn[2])
            {
                //만루
                per = 50;
            }
            else if (bBaseOn[0] && bBaseOn[2])
            {
                //1,3루
                per = 45;
            }
            else if (bBaseOn[1] && bBaseOn[2])
            {
                //2,3루
                per = 47;
            }
            else if (bBaseOn[0] && bBaseOn[1])
            {
                //1,2루
                per = 15;
            }
            else if (bBaseOn[2])
            {
                //3루
                per = 45;
            }
            else if (bBaseOn[1])
            {
                if (inning > 3)
                {
                    //2루
                    per = 10;
                }
            }
            /*else if (bBaseOn[0])
            {
                if (inning > 3)
                {
                    //1루
                    per = 7;
                }
            }*/
            else
            {
                if (Mode.bPvpMode == true)
                {
                    if (inning == 6 || inning == 9)
                    {
                        //6이닝,9이닝 공격 기회 없었다면 2아웃에 100프로확률
                        if (info.curOutCount == 2)
                        {
                            per = 100;
                        }
                    }                    
                }
                else
                {
                    if ((inning % 3 == 0) && info.curOutCount == 2)
                    {
                        //턴 다 끝나가는데 안걸린경우
                        if (inning == 9 || bMyTurn == true)
                        {
                            per = 100;
                        }
                    }
                }
            }

            if (per > 0)
            {
                per += addPercent();
                if (MyMath.Percent() < per)
                {
                    return true;
                }
            }

            return false;
        }

        private int addPercent()
        {
            int addValue = 0;

            if (Mode.bPvpMode == true)
            {
                int gab = 0;
                if (inning < 7)
                {
                    gab = (inning - 4) * 10;
                }
                else
                {
                    gab = (inning - 7) * 10;
                }

                addValue += gab;                
            }
            else
            {
                int gab = info.run[0] - info.run[1];

                if (gab == 0)
                {
                    //동점
                    if (inning >= 9)
                    {
                        addValue = bMyTurn ? 100 : 25;
                    }
                    else if (inning > 6)
                    {
                        addValue = bMyTurn ? 50 : 25;
                    }
                    else
                    {
                        addValue = 7;
                    }
                }
                else if (gab == -1 || gab == -2)
                {
                    if (inning >= 9)
                    {
                        addValue = (bMyTurn ? 30 : -30);
                    }
                    else
                    {
                        addValue = 0;
                    }
                }
                else if (gab == -3 || gab == -4)
                {
                    addValue = (inning >= 9 ? -20 : 0);
                }
                else if (gab == 1 || gab == 2)
                {
                    if (inning >= 9)
                    {
                        addValue = (bMyTurn ? -30 : 30);
                    }
                    else
                    {
                        addValue = 0;
                    }
                }
                else if (gab == 3 || gab == 4)
                {
                    addValue = (inning >= 9 ? -30 : 0);
                }
                else if (gab <= -5 || gab >= 5)
                {
                    addValue = (inning >= 7 ? -100 : 0);
                }
            }
            return addValue;
        }



        private bool checkEndGame()
        {
            if (Mathf.Abs(info.run[0] - info.run[1]) >= SimulGameInfo.ColdGame)
            {
                if (bTopInning == false)
                {
                    //콜드 게임 종료
                    return true;
                }
            }

            if (inning >= Mode.maxInning)        //연장포함
            {
                if (bTopInning == false)
                {
                    inning = Mode.maxInning;
                    bTopInning = false;
                    return true;
                }
            }

            if (inning >= Mode.finalInning) //정규이닝
            {
                if (bTopInning == true)
                {
                    //초공격 끝난후
                    if (bMyHome == true)
                    {
                        //내가 홈인데 이기고 있으면 종료
                        if (info.run[0] > info.run[1])
                        {
                            inning = Mode.finalInning;
                            return true;
                        }
                    }
                    else
                    {
                        //상대가 홈인데 이기고 있으면 종료
                        if (info.run[0] < info.run[1])
                        {
                            inning = Mode.finalInning;
                            return true;
                        }
                    }
                }
                else
                {
                    //말공격 끝난 후
                    if (info.run[0] != info.run[1])
                    {
                        inning = Mode.finalInning;
                        return true;
                    }
                }
            }

            return false;
        }




        private bool bPopupButtonPress;
        private IEnumerator setChancePopup()
        {
            bPopupButtonPress = false;
            int index = (inning - 1) / 3;
            if (bGoToGameFlag == true)
            {
                yield return new WaitForSeconds(1.0f);
                //직접
                if (bChanceFlag[index] == false)
                {
                    //쓸데없는 찬스모드 방지
                    bChanceFlag[index] = true;
                }
                Mode.bOnlyChanceMode = false;
                syncLineup();
                manager.setChanceMode();
                setWait(false);
            }
            else
            {
                //찬스모드
                chancePopup.SetActive(true);
                SkeletonAnimation anim = chancePopup.transform.Find("anim").GetComponent<SkeletonAnimation>();
                anim.skeleton.SetToSetupPose();
                anim.state.SetAnimation(0, MyMath.Half() ? "chance_time" : "game_over", false);
                UISprite gauge = chancePopup.transform.Find("gaugebar").GetComponent<UISprite>();
                changeRemain--;
                float remainTime = 5.0f;
                while(remainTime > 0)
                {
                    remainTime -= 0.1f;
                    int w = Mathf.Clamp((int)(296 * (remainTime / 5.0f)), 5, 326);
                    gauge.SetDimensions(w, 8);
                    yield return new WaitForSeconds(0.1f);
                }
                if (bPopupButtonPress == false)
                {
                    chancePopup.SetActive(false);
                    bChanceFlag[index] = true;
                    StartCoroutine(updater());//simulSpeed));
                }
            }
        }

        public void setActionPlay()
        {
            Mode.bOnlyChanceMode = true;
            bPopupButtonPress = true;

            if (Mode.bPvpMode == true)
            {
                StopCoroutine("setPVPChancePopup");
                chancePopup.SetActive(false);
                guestSyncState();
                syncLineup();
                manager.setChanceMode();
                PvpManager.chanceState = PvpManager.ChanceState.None;
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceAccept);
            }
            else
            {
                StopCoroutine("setChancePopup");
                chancePopup.SetActive(false);
                syncLineup();
                manager.setChanceMode();
            }
        }

        public void setContinueAutoMode()
        {
            bPopupButtonPress = true;

            if (Mode.bPvpMode == true)
            {
                StopCoroutine("setPVPChancePopup");
                chancePopup.SetActive(false);
                Debug.Log("rState를 None으로 세팅");
                PvpManager.rState = PvpManager.RecieveState.None;
                PvpManager.chanceState = PvpManager.ChanceState.None;
                StartCoroutine(waitNextUpdate(true));                
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceDecline);
            }
            else
            {
                StopCoroutine("setChancePopup");
                chancePopup.SetActive(false);
                StartCoroutine(updater());//simulSpeed));
            }
        }



        public void pauseGame()
        {
            if (bPausePopup == false)
            {
                StopCoroutine("updater");
                Util.Load("MainGame/prefabs/gameUI/QuitPopupPrefab", transform, Vector3.zero).GetComponent<UIQuit>().init(manager);            
                bPausePopup = true;
            }
        }

        public void resumeGame()
        {
            bPausePopup = false;
            StartCoroutine(updater());//simulSpeed));
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



        //////////////////////////////////////////////////////////////////////
        //PVP
        //////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 배팅 결과 데이터
        /// 호스트는 직접계산 게스트는 네트워크로부터 받아옴
        /// </summary>
        SimulBattingData netBattingResultData = null;

        /// <summary>
        /// 바로전에 찬스 플레이를 했는지 여부
        /// </summary>
        private bool bLastChancePlayed;

        /// <summary>
        /// 대기시간
        /// </summary>
        private float waitTime, waitTime2;
        private const int MAXWAIT = 100;

        //게스트 투수 상태
        private int guestStamina;
        private PinchStep guestPinchState;                               //핀치 스텝        
        private int guestPinchScore;
        private int[] guestPitcherIndex = new int[2];
        private int[,] guestFielderIndex = new int[2,9];
        //private int[,] saveFielderIndex = new int[2, 9];

        /// <summary>
        /// 호스트 세팅
        /// </summary>
        /// <param name="host"></param>
        public void setHost(bool host)
        {
            Debug.Log("끊긴 경우 호스트 여부 변동 : "+host);
            bHost = host;
        }

        /// <summary>
        /// 호스트 여부
        /// </summary>
        /// <returns></returns>
        public bool IsHost()
        {
            return bHost;
        }

        

        /// <summary>
        /// 다시 동기를 맞춤
        /// </summary>
        public void setReSync()
        {
            bReSync = true;
        }

        /// <summary>
        /// 네트워크가 끊긴 경우 로컬 모드로 전환
        /// </summary>
        private void setLocalModeFromLiveMatch()
        {
            if (networkObj.activeSelf) networkObj.SetActive(false);
            PvpManager.rState = PvpManager.RecieveState.None;
            Mode.bPvpMode = false;
            setLocalChanceFlag();
            StartCoroutine(updater());
        }

        /// <summary>
        /// 로컬에 사용되는 찬스 플래그를 초기화
        /// </summary>
        public void setLocalChanceFlag()
        {
            bPitchingChanceOnce = true;
            int max = Mathf.Clamp((inning-1) / 3 ,0 ,3);

            for(int i = 0; i<= max ;i++)
            {
                bChanceFlag[i] = true;
            }
        }

        private float Max_Wait_Time = 15;
        private PvpManager.ConnectState checkWaitTimeOut(float curWaitTime)
        {
            if (networkObj.activeSelf == false)
            {
                if (curWaitTime > 0.5f)
                {
                    //Debug.Log("===================>>네트워크 대기 UI");
                    networkObj.SetActive(true);
                }
            }

            if (curWaitTime > Max_Wait_Time)
            {                
                if (bHost == true)
                {
                    //Debug.Log("===================>>호스트가 게스트 강제 끊김 UI");
                    return PvpManager.ConnectState.DisConnectOther;
                }
                else
                {
                    //Debug.Log("===================>>게스트가 호스트 강제 끊김 UI");
                    return PvpManager.ConnectState.DisConnectMine;
                }
            }

            return PvpManager.ConnectState.Connect;
        }



        /// <summary>
        /// 다음 업데이트를위한 대기 -> 호스트와 게스트의 분기를 해준다
        /// </summary>
        /// <param name="Init"></param>
        /// <returns></returns>
        private IEnumerator waitNextUpdate(bool Init)
        {
            if (manager.bPlayBallEvent == false)
            {
                LoadDynamicUI("playballPrefab", 100, 1.5f, new Vector3(-63, -47, 0));
                manager.bPlayBallEvent = true;
                yield return new WaitForSeconds(1.5f);
            }

            waitTime2 = 0;
            while (PvpManager.bGameReady == false)
            {
                Debug.Log("여기에 들어오냐? Max_Wait_Time " + Max_Wait_Time);
                PvpManager.ConnectState state = checkWaitTimeOut(waitTime2 += Time.deltaTime);
                if (state != PvpManager.ConnectState.Connect)
                {
                    //Debug.Log("================================>> 상대를 강제로 디스코넥트하게 만듬");
                    networkObj.SetActive(false);
                    setLocalModeFromLiveMatch();
                    PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.OtherForceDisconnect);
                    yield break;
                }
                yield return new WaitForEndOfFrame();
            }
            Max_Wait_Time = 7.5f; //최대 대기 시간 바꿔줌
            if (networkObj.activeSelf) networkObj.SetActive(false);
            
            if (bHost == true)
            {
                if (Init == false)
                {                    
                    //응답을 받으면 다음 스텝으로
                    while (PvpManager.rState != PvpManager.RecieveState.GuestQuickInfo)
                    {
                        if (PvpManager.connectState == PvpManager.ConnectState.DisConnectOther)
                        {
                            Debug.Log("상대방이 끊긴경우 bPvpMode를 false로 세팅하고 로컬로 진행");
                            setLocalModeFromLiveMatch();
                            yield break;
                        }
                        else if (PvpManager.connectState == PvpManager.ConnectState.DisConnectMine)
                        {
                            //disconect가 떨어지지 않은 경우 이루틴으로 오면 엇나간것이므로 무시하고 진행
                            Debug.Log("내가 끊긴경우 루프를 벗어남");
                            yield break;
                        }
                        else
                        {
                            Debug.Log("게스트로부터 응답대기 " + PvpManager.rState + "        Max_Wait_Time " + Max_Wait_Time);
                            PvpManager.ConnectState state = checkWaitTimeOut(waitTime2 += Time.deltaTime);
                            if (state == PvpManager.ConnectState.DisConnectOther)
                            {
                                setLocalModeFromLiveMatch();
                                yield break;
                            }
                            else
                            {
                                yield return new WaitForEndOfFrame();
                            }
                        }                        
                    }

                    if (PvpManager.rState == PvpManager.RecieveState.BatterInfo)
                    {
                        ////Debug.Log("=================>>>배터 인포를 받은 경우 yield break");
                        yield break;
                    }
                    //Debug.Log("rState를 None으로 세팅");
                    PvpManager.rState = PvpManager.RecieveState.None;
                }
            }
            else
            {
                //응답을 받으면 다음 스텝으로
                while (PvpManager.rState != PvpManager.RecieveState.HostQuickInfo)
                {
                    if (PvpManager.connectState == PvpManager.ConnectState.DisConnectOther)
                    {
                        Debug.Log("게스트로 플레이시 상대방이 끊긴경우 bPvpMode를 false로 세팅");
                        bHost = true;   //이경우 게스트가 호스트가 됨
                        setLocalModeFromLiveMatch();
                        yield break;
                    }
                    else if (PvpManager.connectState == PvpManager.ConnectState.DisConnectMine)
                    {
                        Debug.Log("내가 끊긴경우 루프를 벗어남");
                        yield break;
                    }
                    else
                    {                        
                        if (PvpManager.chanceState == PvpManager.ChanceState.ChanceWait)
                        {
                            StartCoroutine(waitChanceStae());
                            yield break;
                        }
                        else if (PvpManager.chanceState == PvpManager.ChanceState.ChanceSelect)
                        {
                            yield break;
                        }
                        else
                        {                            
                            if (bHost == true)
                            {
                                ////Debug.Log("=================================================>>호스트가 된경우 대기 루프 빠져나감");
                                break;
                            }
                            //Debug.Log("호스트부터 응답대기 = " + PvpManager.rState + "        Max_Wait_Time " + Max_Wait_Time);
                            PvpManager.ConnectState state = checkWaitTimeOut(waitTime2 += Time.deltaTime);
                            if (state == PvpManager.ConnectState.DisConnectMine)
                            {
                                PvpManager.GetInstance().forceDisconnect();
                                yield break;
                            }
                            else
                            {
                                yield return new WaitForEndOfFrame();
                            }
                        }

                        
                    }
                }

                if (PvpManager.rState == PvpManager.RecieveState.BatterInfo)
                {
                    ////Debug.Log("=================>>>배터 인포를 받은 경우 yield break");
                    yield break;
                }
                //Debug.Log("rState를 None으로 세팅");
                PvpManager.rState = PvpManager.RecieveState.None;
                if (bHost == false)
                {
                    //수신했음을 호스트에 보냄
                    PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.SendReply);
                }
            }

            if (networkObj.activeSelf) networkObj.SetActive(false);

            yield return new WaitForEndOfFrame();

            StartCoroutine(updaterPvp());
        }

        /// <summary>
        /// 상대의 찬스 상태를 대기
        /// </summary>
        /// <returns></returns>        
        private IEnumerator waitChanceStae()
        {
            //waitTime = 0;
            manager.bMyTurn = bMyTurn;
            yield return new WaitForEndOfFrame();

            waitPopup.gameObject.SetActive(true);
            while (true)
            {
                if (PvpManager.connectState != PvpManager.ConnectState.Connect)
                {
                    //Debug.Log("=============================>> 상대의 찬스 선택 대기중 연결이 끊긴경우");
                    PvpManager.chanceState = PvpManager.ChanceState.ChanceDecline;
                }

                //찬스 대기중
                ////Debug.Log("=============================>> 상대의 찬스 선택 대기중");
                if (PvpManager.chanceState == PvpManager.ChanceState.ChanceDecline)
                {
                    //찬스 거부시                    
                    waitPopup.gameObject.SetActive(false);
                    Debug.Log("rState를 None으로 세팅");
                    PvpManager.rState = PvpManager.RecieveState.None;
                    PvpManager.chanceState = PvpManager.ChanceState.None;
                    StartCoroutine(waitNextUpdate(true));
                    break;
                }
                else if (PvpManager.chanceState == PvpManager.ChanceState.ChanceAccept)
                {
                    //찬스 허락시    
                    Mode.bOnlyChanceMode = true;                    
                    waitPopup.gameObject.SetActive(false);
                    guestSyncState();
                    syncLineup();
                    manager.setChanceMode();
                    PvpManager.chanceState = PvpManager.ChanceState.None;
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            //Debug.Log("=============================>> 찬스 선택 대기상태 종료");
            PvpManager.chanceState = PvpManager.ChanceState.None;
        }


        /// <summary>
        /// 호스트가 보낸 게임정보를 파싱
        /// </summary>
        /// <param name="quickInfo"></param>
        private void QuickInfoParsing(SendHostQuickInfo quickInfo)
        {
            //배팅 결과
            netBattingResultData = new SimulBattingData();
            netBattingResultData.result = (SimulResultState)quickInfo.result;
            netBattingResultData.fIndex = quickInfo.fIndex;
            netBattingResultData.hitType = (SimulHitType)quickInfo.hitType; 
            //주자동기화
            for (int i = 0; i < 4; i++)
            {
                netBattingResultData.bRunnerActive[i] = quickInfo.bRunnerActive[i];
                netBattingResultData.runnerCurPos[i] = quickInfo.runnerCurPos[i];
                netBattingResultData.runnerName[i] = quickInfo.runnerName[i];
                netBattingResultData.runnerLineup[i] = quickInfo.runnerLineup[i];
                netBattingResultData.runnerValue[i] = quickInfo.runnerValue[i];
            }
            netBattingResultData.stealState = (SimulStealState)quickInfo.stealState;

            //퀵게임 인포
            info.currentInning = quickInfo.currentInning;
            info.bInningEnd = quickInfo.bInningEnd;
            info.bGameEnd = quickInfo.bGameEnd;
            info.curOutCount = quickInfo.curOutCount;

            //UnityEngine.//Debug.Log("=============>> 받는 아웃카운트 " + quickInfo.curOutCount);
            ////Debug.Log("================================>>이닝교체 여부 bInningEnd : " + info.bInningEnd);

            if (info.gameInfo == null)
            {
                info.gameInfo = new SimulGameInfo();
                info.gameInfo.init();
            }
            
            //gameInfo
            for (int i = 0; i < 2; i++)
            {
                info.run[i] = quickInfo.run[1 - i];
                info.gameInfo.run[i] = quickInfo.run[1 - i];
                for (int j = 0; j < 12; j++)
                {
                    info.gameInfo.inningScore[i, j] = quickInfo.inningScore[1 - i, j];
                }
                info.gameInfo.hit[i] = quickInfo.hit[1 - i];
                info.gameInfo.error[i] = quickInfo.error[1 - i];
                info.gameInfo.pitchNum[i] = quickInfo.pitchNum[1 - i];
            }
                        
            //투수상태
            guestStamina = (int)quickInfo.stamina;
            guestPinchState = (PinchStep)quickInfo.pinch;
            guestPinchScore = quickInfo.pinchPoint;

            curPitcher.setCurrentStamina(guestStamina);
            curPitcher.setPinchState(guestPinchState);
            curPitcher.setPinchScore(guestPinchScore);

            //UnityEngine.//Debug.Log("=============>> 받는 bInningEnd : " + info.bGameEnd);
            //UnityEngine.//Debug.Log("=============>> 받는 아웃카운트 : " + info.curOutCount);
            //UnityEngine.//Debug.Log("=============>> 받는 stamina : " + guestStamina);
            //UnityEngine.//Debug.Log("=============>> 받는 pinch : " + (PinchStep)guestPinchState);

            //교체 상태
            for (int i = 0; i < 2; i++)
            {
                guestPitcherIndex[i] = quickInfo.pitcherIndex[i];
                for (int j = 0; j < 9; j++)
                {
                    guestFielderIndex[i,j] = quickInfo.fielderIndex[i,j];
                }
            }

            //
            info.MeaNoonShow = quickInfo.MeaNoonShow;
            info.HeosimShow = quickInfo.HeosimShow;
            info.MeahogShow = quickInfo.MeahogShow;

            System.Collections.Generic.Dictionary<SkillUseStep, SimulSkillInfo> skill = new System.Collections.Generic.Dictionary<SkillUseStep,SimulSkillInfo>();
            for (int i = 0; i < quickInfo.countNum; i++)
            {
                SimulSkillInfo tempSkillInfo = new SimulSkillInfo();

                tempSkillInfo.bAvailable = quickInfo.bAvailable[i];
                tempSkillInfo.vsType = (VsResult)quickInfo.vsType[i];
                tempSkillInfo.offenseID = (SkillID)quickInfo.offenseID[i];
                tempSkillInfo.offenseRank = quickInfo.offenseRank[i];
                tempSkillInfo.defenseID = (SkillID)quickInfo.defenseID[i];
                tempSkillInfo.defenseRank = quickInfo.defenseRank[i];
                tempSkillInfo.catcherID = (SkillID)quickInfo.catcherID[i];
                tempSkillInfo.catcherRank = quickInfo.catcherRank[i];
                SkillUseStep key = (SkillUseStep)quickInfo.skillStep[i];
                skill.Add(key, tempSkillInfo);
            }
            info.skillInfo = skill;

        }

        
        /// <summary>
        /// Pvp모드에서의 업데이터 호스트와 게스트의 루틴이 다르다
        /// </summary>
        /// <returns></returns>
        private IEnumerator updaterPvp()
        {
            yield return new WaitForSeconds(0.2f);

            if (bHost == true)
            {
                if (bLastChancePlayed == false)
                {
                    if (pvpChanceMode() == true)
                    {
                        bLastChancePlayed = true;
                        if (bMyTurn == true)
                        {
                            //내턴인경우 찬스를 선택할 수 있고
                            StartCoroutine(setPVPChancePopup());
                        }
                        else
                        {
                            //내턴이 아닌 경우는 쓰레드를 빠져나와 대기 상태
                            StartCoroutine(waitChanceStae());
                        }
                        yield break;
                    }
                }

                bLastChancePlayed = false;
                //호스트는 직접 계산
                SimulManager.SimulationBatting(true);
                netBattingResultData = SimulManager.GetBattingResult();
                SimulManager.SetQuickgameInfo(info);
                PvpManager.GetInstance().SendHostQuickInfo(netBattingResultData, info, curPitcher);
            }
            else
            {
                SendHostQuickInfo quickInfo = PvpManager.GetInstance().sendQuickInfo;

                //재접속 처리
                if (bReSync == true ||
                    inning != quickInfo.currentInning ||
                    bTopInning != quickInfo.bTopInning)
                {                    
                    ////Debug.Log("=============================================>>>>재접속시");
                    ////Debug.Log("=============================================>>>>bTopInning : " + bTopInning);
                    ////Debug.Log("=============================================>>>>quickInfo.bTopInning : " + quickInfo.bTopInning);
                    ////Debug.Log("=============================================>>>>inning : " + inning);
                    ////Debug.Log("=============================================>>>>info.currentInning : " + info.currentInning);
                    inning = info.currentInning = quickInfo.currentInning; //이닝 동기화
                    if (bTopInning != quickInfo.bTopInning) //회초가 다른 경우
                    {
                        Debug.Log("이닝 보정");
                        changeInningPre();
                        changeInning(false);
                    }
                    bReSync = false;
                    yield return new WaitForSeconds(0.5f);
                }

                //게스트는 받아서 처리
                QuickInfoParsing(quickInfo);
                checkGuestPlayerChange();
            }

            ////Debug.Log("========================>>>>이닝 " + info.currentInning + " battingResultData = " + battingResultData.result);
            

            //인덱스
            int curIndex = bTopInning ? awayIndex() : homeIndex();

            //선수 교체 - 타자교체
            if (curBatter.bChangeIn == true)
            {
                curBatter.bChangeIn = false;
                batterChangeEvent(curIndex);
            }
            //선수 교체 - 투수교체
            if (curPitcher.bChangeIn == true)
            {
                curPitcher.bChangeIn = false;
                pitcherChangeEvent(curIndex);
            }
            yield return new WaitForSeconds(1.0f);

            //도루 연출
            if (netBattingResultData.stealState != SimulStealState.NONE)
            {
                SimulStealState stealState = netBattingResultData.stealState;
                //도루 스킬
                yield return new WaitForSeconds(setStealSkill(stealState, curIndex));
                //도루 딜레이
                yield return new WaitForSeconds(setRunnerSteal(stealState));
            }


            //현재 발동된 스킬 옅출
            skillUseCount[0] = skillUseCount[1] = 0;
            foreach (var step in (SkillUseStep[])System.Enum.GetValues(typeof(SkillUseStep)))
            {
                if (info.skillInfo.ContainsKey(step) == true)
                {
                    //배팅뷰 -> 피칭 -> 필드 순차적으로 스킬 연출
                    yield return new WaitForSeconds(skillDisplay(info.skillInfo[step], curIndex, step));
                }
            }
            

            //주자 연출
            setRunnerUI(netBattingResultData);

            //필드뷰 공 궤적 연출 & 콜
            float callDelay = callDisplay(netBattingResultData);
            if (callDelay > 0) yield return new WaitForSeconds(callDelay);
            

            //라인업 처리            
            nextLineup(curIndex, netBattingResultData.result);
                        

            //결과 및 뒷처리
            if (info.bGameEnd == true)
            {
                //Debug.Log("=================>>게임 오버2");
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.GameEnd);
                curState = SimulState.gameover;
            }
            else
            {
                //찬스체크
                if (info.bInningEnd == true)
                {
                    bLastChancePlayed = true;
                    changeInningEvent();
                    if (bHost == true)
                    {
                        //매이닝 기록 동기화 해줌
                        PvpManager.GetInstance().SendResultSyncInfo(false);
                    }
                    else
                    {
                        
                    }

                    yield return new WaitForSeconds(1.65f);
                    
                    SimulManager.SimulChangeInning(bHost);
                    bool vsSkill = SimulManager.SetBattingviewSkill();
                    manager.simulCalled = (vsSkill ? SimulPreCalled.VsType : SimulPreCalled.SingleType);

                    changeInning(true);
                    yield return new WaitForSeconds(0.6f);
                }

                if (bHost == true)
                {
                    //라인업 업데이트
                    SimulManager.SetQuickgameInfo(info);
                }
                
                //다음 웨이트
                StartCoroutine(waitNextUpdate(false));
            }
        }

        /// <summary>
        /// 게스트의 선슈 교체 체크
        /// </summary>
        private void checkGuestPlayerChange()
        {  
            int offenseIndex = bTopInning ? awayIndex() : homeIndex();
            for (int i = 0; i < 2; i++)
            {
                //투수교체 여부
                int curIndex = SimulPlayerManager.GetPitcherIndex(i);
                if (curIndex != guestPitcherIndex[i])
                {
                    //Debug.Log("=============================================>>>투수교체 플래그 전달받음");
                    SimulPlayerManager.SetCurrentPitcherIndex(i, guestPitcherIndex[i], false); //바뀐투수의 피처 인덱스 세팅
                    SimulPlayerManager.SetPitcherOut(i, guestPitcherIndex[i], true);           //현재 투수를 출전한 선수로 세팅
                    CPlayer pitcher = SimulPlayerManager.GetPitcher(i);
                    pitcher.setCurrentStamina(100);
                    pitcher.setPinchScore(-10);
                    pitcher.setPinchState(PinchStep.Normal);
                   
                    if(i != offenseIndex)
                    {
                        //수비시인 경우만
                        pitcher.bChangeIn = true;
                    }
                }

                //타자교체 여부
                for (int j = 0; j < 9; j++)
                {
                    CPlayer curFielder = SimulPlayerManager.GetFielder(i, j);
                    if (guestFielderIndex[i, j] != curFielder.originLineup)
                    {
                        Debug.Log("타자교체 플래그 전달받음    i  =" + i + "===============  j = " + j + "   게스트 현재 선수 타자 : " + curFielder.getName());
                        Debug.Log("//offenseIndex = " + offenseIndex + "  //   lineupCount = " + curLineupCount[offenseIndex]);
                        Debug.Log("//guestFielderIndex[i, j] = " + guestFielderIndex[i, j] + "  //   curFielder.originLineup = " + curFielder.originLineup);
                        SimulPlayerManager.SetFielderChange(i, guestFielderIndex[i, j], curFielder.getOrder(), 200);
                        Debug.Log("바꾼 선수 : "+SimulPlayerManager.GetFielder(i, j).getName());
                        if (i == offenseIndex)
                        {
                            int lineupCount = curLineupCount[offenseIndex];
                            if (lineupCount == j)
                            {
                                //공격시 해당 카운트
                                Debug.Log("바꾼 선수는 현재 타자임!!!");
                                curBatter = SimulPlayerManager.GetFielder(i, j);
                                curBatter.bChangeIn = true;
                            }
                        }
                    }
                }
            }

        }


        /// <summary>
        /// 게스트의 기록정보는 따로
        /// </summary>
        private void setGuestRecord(SimulBattingData Data)
        {

        }


        //테스트용 ->지워지워
        //private bool[] bTestCheck = new bool[12]{false,false,false,false,false,false,false,false,false,false,false,false};

        /// <summary>
        /// PVP모드에서 찬스모드
        /// 오직 호스트에서만 계산
        /// </summary>
        /// <returns></returns>
        private bool pvpChanceMode()
        {
            //테스트용 지워지워
            /*if (bTestCheck[inning - 1] == false)
            {
                if((inning % 2 == 0 && bMyTurn == false) || (inning % 2 == 1 && bMyTurn == true))
                {
                    if (info.curOutCount ==  (inning % 3))
                    {
                        if (bMyTurn == true)
                        {
                            PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceHost);
                        }
                        else
                        {
                            PvpManager.chanceState = PvpManager.ChanceState.ChanceWait;
                            PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceGuest);
                        }
                        bTestCheck[inning - 1] = true;
                        return true;
                    }
                }
            }//*/

            
            /*//테스트용 지워지워 (특정베이스 테스트)
            if (bTestCheck[inning - 1] == false)
            {
                if (bBaseOn[1] == true || bBaseOn[2] == true || bBaseOn[0] == true)
                {
                    if (bMyTurn == true)
                    {
                        PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceHost);
                    }
                    else
                    {
                        PvpManager.chanceState = PvpManager.ChanceState.ChanceWait;
                        PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceGuest);
                    }
                    bTestCheck[inning - 1] = true;
                    return true;
                }
            }//*/

            
            //이거진짜
            if (inning >= 3)
            {
                int index = (inning < 7 ? 0 : 1);
                if (bPvpChanceFlag[index] == false)
                {
                    if ((bMyTurn && bOffenseChanceFlag[index])
                      || (!bMyTurn && bDefenseChanceFlag[index]))
                    {
                        return false;
                    }
                    bPitchingChanceOnce = false;
                    if (checkCase() == true)
                    {
                        if (bMyTurn) bOffenseChanceFlag[index] = true;
                        else bDefenseChanceFlag[index] = true;

                        if (bOffenseChanceFlag[index] && bDefenseChanceFlag[index])
                        {
                            bPvpChanceFlag[index] = true;
                        }

                        if (bMyTurn == true)
                        {
                            PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceHost);
                        }
                        else
                        {
                            PvpManager.chanceState = PvpManager.ChanceState.ChanceWait;
                            PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceGuest);
                        }
                        return true;
                    }
                }
            }//*/

            /*
            //끝내기 찬스 - 진짜 추가 -> 완성안됨
            if (checkGoodbye() == true)
            {
                return true;
            }*/

            return false;
        }


        


        //bool bPvpChanceFlag = false;    //임시 나중에 지워
        /// <summary>
        /// PVP모드에서의 찬스 팝업 세팅
        /// </summary>
        /// <returns></returns>
        private IEnumerator setPVPChancePopup()
        {
            manager.bMyTurn = bMyTurn;

            bPopupButtonPress = false;

            //찬스모드
            chancePopup.SetActive(true);
            SkeletonAnimation anim = chancePopup.transform.Find("anim").GetComponent<SkeletonAnimation>();
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(0, MyMath.Half() ? "chance_time" : "game_over", false);
            UISprite gauge = chancePopup.transform.Find("gaugebar").GetComponent<UISprite>();
            changeRemain--;
            float remainTime = 5.0f;
            while (remainTime > 0)
            {
                remainTime -= 0.2f;
                int w = Mathf.Clamp((int)(326 * (remainTime / 5.0f)), 5, 326);
                gauge.SetDimensions(w, 8);
                yield return new WaitForSeconds(0.2f);
            }

            if (bPopupButtonPress == false)
            {
                chancePopup.SetActive(false);
                Debug.Log("rState를 None으로 세팅");
                PvpManager.rState = PvpManager.RecieveState.None;
                PvpManager.chanceState = PvpManager.ChanceState.None;
                StartCoroutine(waitNextUpdate(true));
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChanceDecline);
            }
        }

        //
        public void SetPVPChancePopup()
        {
            StartCoroutine(setPVPChancePopup());
        }

        /// <summary>
        /// 게스트가 액션모드(찬스모드)로 갈때 정보가 누락된 부분 동기화 시켜줌
        /// </summary>
        private void guestSyncState()
        {
            if (Mode.bPvpMode == true)
            {
                if (bHost == false)
                {
                    int team = (bMyTurn ? 0 : 1);
                    for(int i =0; i< 4; i++)
                    {
                        if(netBattingResultData.bRunnerActive[i] == true)
                        {
                            int curPos = netBattingResultData.runnerCurPos[i];
                            int lineup = netBattingResultData.runnerLineup[i];
                            CPlayer player = SimulPlayerManager.GetFielder(team, lineup);
                            SimulManager.SetRunner(player, i, curPos);
                        }
                    }

                    SimulManager.SetGameInfo(info, bTopInning, bMyTurn);
                   
                    //카운트 동기화
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            manager.nInningScore[i, j] = info.gameInfo.inningScore[i, j];
                        }
                    }
                }
                //manager.nInningCount = info.currentInning;
            }
        }

        /// <summary>
        /// 재연결 팝업 설정
        /// </summary>
        public void setReconnectPopup()
        {
            //StopAllCoroutines();

            Mode.bPvpMode = false;

            //다른 팝업창 닫음
            chancePopup.gameObject.SetActive(false);
            waitPopup.gameObject.SetActive(false);
            //재연결 팝업 열음
            reconnectPopup.gameObject.SetActive(true);

        }

        /// <summary>
        /// 재연결
        /// </summary>
        public void setReconnect()
        {
            //Debug.Log("========================>>> 재연결");
            bReconnectProcess = true;
            networkObj.SetActive(true);
            reconnectPopup.gameObject.SetActive(false);            
            bHost = false;  //한번더
            PvpManager.GetInstance().reconnect();            
        }

        /// <summary>
        /// 재연결하지 않고 게임 종료
        /// </summary>
        public void setQuit()
        {            
            // DISABLED_MGRS: Mgrs.ManagerSupervise(false);
            SkillEffectDisplayManager.Destroy();
            Destroy(GameObject.FindWithTag("SIMUL_TAG").gameObject);
            // DISABLED_MGRS: Mgrs.userData.UserLobbyReason = UserData.EReason.OutGame_Lobby;
            // DISABLED_MGRS: Mgrs.SceneLoad.LoadScene(SceneID.Lobby);
        }

        /// <summary>
        /// 재연결 프로세스 시작
        /// </summary>
        public void setReconnectProcess()
        {   
            if (bHost == false)
            {
                //게스트는 호스트에게 재접속을 요구
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ReconnectAsked);
            }
        }



        /// <summary>
        /// 재연결을 요구 받음
        /// </summary>
        public void setReconnectAsked()
        {
            if (bHost == true)
            {
                bReconnectAsked = true;
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ReconnectDone);
            }
            else
            {
                //게스트가 이 요청을 받은 경우는 서로 끊긴 경우
                //Debug.Log("=======================================>>서로 재연결을 요구하는 경우 둘다 끊긴상황");
                bReconnectProcess = true;
                waitTime = MAXWAIT;
            }

        }

        /// <summary>
        /// 재접속 요청받음 완료 후 상태 세팅 완료(호스트쪽)
        /// </summary>
        private void reconnectAskedFinish()
        {
            bReconnectProcess = false;
            bReconnectAsked = false;
            bHost = true; //한번 더 해줘            
            Mode.bPvpMode = true;
            PvpManager.bGameReady = true;
            StartCoroutine(waitNextUpdate(true));
        }

        /// <summary>
        /// 재접속 프로세스 완료
        /// </summary>
        public void setReconnectDone()
        {
            if (bHost == false)
            {
                bReconnectDone = true;                 
            }
        }

        /// <summary>
        /// 재접속 완료 수신후 상태 세팅 완료(게스트쪽)
        /// </summary>
        private void reconnectDoneFinish()
        {
            //bool bOtherConnect = PvpManager.GetInstance().IsOtherConnected();
            ////Debug.Log("=======================================>>bOtherConnect = " + bOtherConnect);      
            bReconnectProcess = false;
            bReconnectDone = false;
            networkObj.SetActive(false);
            Mode.bPvpMode = true;
            PvpManager.bGameReady = true;
            StartCoroutine(waitNextUpdate(false));
        }


        private void reconnectTimeCheck()
        {
            waitTime += Time.deltaTime;
            if (waitTime > 20.0f)
            {
                bReconnectProcess = false;
                //Debug.Log("=======================================>>우선 재연결이 불가능한 경우 강제종료로 처리");
                setQuit();
            }
        }

        /// <summary>
        /// PVP익셉션 체크
        /// </summary>
        private void checkPvpException()
        {
            //게스트가 종료가 되지 않는 경우 강제 종료 루틴
            if (Mode.bPvpMode == true)
            {
                /*
                if (bHost == false)
                {
                    if (PvpManager.rState == PvpManager.RecieveState.ResultSync)
                    {
                        if (info.bGameEnd == true)
                        {
                            //Debug.Log("======================>> 강제 종료 루틴");
                            //강제 게임 종료 루틴
                            curState = SimulState.gameover;
                            StopAllCoroutines();
                        }
                    }
                }*/
                
                if (curState != SimulState.gameover)
                {
                    if (PvpManager.bGameEndAsk == true)
                    {
                        PvpManager.bGameEndAsk = false;
                        curState = SimulState.gameover;
                        StopAllCoroutines();                        
                    }
                }
            }
            else
            {
                if (Mode.gameMode == Mode.GamePlayMode.Pvp)
                {
                    if (bHost == false)
                    {
                        if (bReconnectDone == true)
                        {
                            reconnectDoneFinish();
                        }
                    }

                    if (bReconnectProcess == true)
                    {
                        reconnectTimeCheck();
                    }

                }
            }
        }

        static int[] logoValue1 = new int[10];
        static int[] logoValue2 = new int[10];

        static int[] getRandomArray()
        {
            int[] numbers = Enumerable.Range(1, 10).ToArray();

            for (int i = numbers.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);

                (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
            }
            return numbers;
        }

    }
}
