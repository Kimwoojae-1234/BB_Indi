using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;
using System.Text;
using System.Threading;

namespace BaseBall.BallPlay
{
    public class PvpManager : MonoBehaviour
    {
        

        private readonly string batterInfo = "SendBatterInfo";
        private readonly string pitchInfo = "SendPitchInfo";
        private readonly string battingInfo = "SendBattingInfo";
        private readonly string noHitInfo = "SendNoHitInfo";
        //private readonly string noSwingInfo = "SendNoSwingInfo";
        //private readonly string hutSwingInfo = "SendHutSwingInfo";
        private readonly string powerBattingInfo = "SendPowerBattingInfo";
        private readonly string pickoffInfo = "SendPickOffInfo";
        private readonly string stealInfo = "SendStealInfo";
        //private readonly string runnerSyncInfo = "SendRunnerSyncInfo";
        private readonly string fieldSyncInfo = "SendFieldSyncInfo";
        private readonly string sendSync = "SendGameSync";
        private readonly string syncAsk = "SendAskSync";
        private readonly string hostQuickInfo = "SendHostQuickInfo";
        private readonly string quickReplyInfo = "SendQuickGameReplyInfo";
        private readonly string resultSyncInfo = "SendResultSync";
        private readonly string changeSyncInfo = "SendChangePlayerSync";


        
        public enum RecieveState
        {
            None,
            BatterInfo,
            PitchInfo,
            BattingInfo,
            HostQuickInfo,
            GuestQuickInfo,            
            ResultSync
        }

        public enum ChanceState
        {
            None,
            ChanceWait,
            ChanceSelect,
            ChanceAccept,
            ChanceDecline
        }

        public enum ConnectState
        {
            Connect,
            DisConnectOther,
            DisConnectMine
        }

        public enum SyncAskState
        {
            None,
            Ask,
            Recieve,
            Done
        }

        public enum ChangeWaitState
        {
            None,
            Wait,
            Finish,
            ChangeEvent,
            PitchSelect,
            PitchTimer
        }



        /// <summary>
        /// 랜덤 시드
        /// </summary>
        public static int RandomSeed, RandomSeed2;


        /// <summary>
        /// 싱크 상태
        /// </summary>
        public static SyncAskState syncState;

        /// <summary>
        /// 수신 상태
        /// </summary>
        public static RecieveState rState;

        /// <summary>
        /// 찬스 상태
        /// </summary>
        public static ChanceState chanceState;

        /// <summary>
        /// 배팅 결과가 업데이트 된경우
        /// </summary>
        public static bool bBattingResultUpdate;
        
        /// <summary>
        /// 카운트 싱크 여부
        /// </summary>
        public static bool bCountSync;
        
        /// <summary>
        /// 필드 상태 여부
        /// </summary>
        public static bool bFieldState;


        //
        public static bool bGameReady = false;

        //디스커넥트 여부
        public static ConnectState connectState = ConnectState.Connect;


        //대기상태를 빠져나옴
        public static bool bWaitStateQuit = false;


        //강제 게임 종료
        public static bool bGameEndAsk = false;


        /// <summary>
        /// 인스턴스
        /// </summary>
        private static PvpManager Instance_;

        /// <summary>
        /// 야구 매니저
        /// </summary>
        private BallPlayManager manager;

#if _Test_Local

        //private Connector.Pvp.PvpConnector pvp = new Connector.Pvp.PvpConnector();
#else
        /// <summary>
        /// pvp 커넥터
        /// </summary>
        private LivePlayPvpService pvp;
#endif

        

        /// <summary>
        /// 준비여부
        /// </summary>
        private bool bReady = false;
        
        /// <summary>
        /// 강제 시뮬레이션 전환
        /// </summary>
        private bool bForceBackToSimul = false;

        /// <summary>
        /// 이모티콘 채팅을 수신한 경우
        /// </summary>
        private bool bEmoticonFlag;

        /// <summary>
        /// 상대가 선수교체 관련 수신한 경우
        /// </summary>
        private ChangeWaitState changeWaitState = ChangeWaitState.None;

        
        /// <summary>
        /// 업데이트에서 도루와 견제 체크
        /// </summary>
        private bool bStealFlag, bPickoffFlag;

        /// <summary>
        /// 타자 정보 수신한 경우
        /// </summary>
        private bool bBatterInfoWait;

        /// <summary>
        /// 피치 정보 수신한 경우
        /// </summary>
        private bool bPitchInfoWait;


        /// <summary>
        /// 디스커넥트시 AI가 던지게 하는 경우
        /// </summary>
        private bool bDisconnectAndAiThrow;


        /// <summary>
        /// 게스트의 찬스 상태를 수신한 경우
        /// </summary>
        private bool bGuestChance;

        /// <summary>
        /// 게임 초기화 여부
        /// </summary>
        private bool bInitGame;

        
        /// <summary>
        /// 새 타자 여부
        /// </summary>
        private bool bNewBatter;


        /// <summary>
        /// 한타자에게 던진 투구수
        /// </summary>
        private int nPitchNum;

        /// <summary>
        /// 타구를 쳤는지 여부
        /// </summary>
        private bool bHit;

        /// <summary>
        /// 스윙을 했는지 여부
        /// </summary>
        private bool bSwing;

        /// <summary>
        /// 번트를 했는지 여부
        /// </summary>
        private bool bBunt;




        

        void Awake()
        {
            bInitGame = false;
            PvpManager.syncState = SyncAskState.None;
            connectState = ConnectState.Connect;
            rState = RecieveState.None;
            chanceState = ChanceState.None;
            bGameReady = false;
            bReady = false;
            bGuestChance = false;
            bWaitStateQuit = false;
            bDisconnectAndAiThrow = false;
            changeWaitState = ChangeWaitState.None;
            bEmoticonFlag = false;
            bForceBackToSimul = false;
            bGameEndAsk = false;
            Instance_ = this;
        }


        void OnDestroy()
        {
            //pvp.Close();
            Instance_ = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        //메인 쓰레드
        ////////////////////////////////////////////////////////////////////////////
        void Update()
        {
            //네트워크 클로즈 테스트 -> 나중에 지워
            /*if (Input.GetKeyDown(KeyCode.C))
            {
                pvp.Close();
            }*/

            if (syncState == SyncAskState.Recieve)
            {
                //동기화 정보 처리
                syncSendDone();
            }

            if (bEmoticonFlag == true)
            {
                otherEmoticon(getMessage);
                bEmoticonFlag = false;
            }

            if (bForceBackToSimul == true)
            {
                //시뮬레이션으로 돌아감
                manager.checkChanceModeEnd(SimulResultState.NONE);
                bForceBackToSimul = false;
            }

            if (changeWaitState == ChangeWaitState.Wait)
            {
                //선수교체 대기
                setChangeWait(true);
                changeWaitState = ChangeWaitState.None;
            }

            if (changeWaitState == ChangeWaitState.Finish)
            {
                //선수교체 종료
                setChangeWait(false);
                changeWaitState = ChangeWaitState.None;
            }

            if (changeWaitState == ChangeWaitState.ChangeEvent)
            {
                //선수교체 이벤트
                setChangeEvent();
                changeWaitState = ChangeWaitState.None;
            }

            if (changeWaitState == ChangeWaitState.PitchSelect)
            {
                //탑UI
                IngameUI.GetScoreBoard().TopUIActive(false);
                changeWaitState = ChangeWaitState.None;
                //Debug.Log("============================>>> bPause = " + Mode.bPauseGame);
                if (Mode.bPauseGame == true)
                {
                    //Debug.Log("==============================================================>>> 이걸로 해결?");
                    UIPause pause = IngameUI.GetPauseUI();
                    if (pause._active.activeSelf)
                    {
                        pause._active.SetActive(false);
                    }
                    if (pause._waitActive.activeSelf)
                    {
                        pause.SetChangeWait(false);
                    }
                    manager.pitcher.setResume();
                    Mode.bPauseGame = false;
                }
            }

            if (changeWaitState == ChangeWaitState.PitchTimer)
            {
                if (manager.bMyTurn) IngameUI.GetScoreBoard().SetPitchTimerActive(true);
                changeWaitState = ChangeWaitState.None;
            }

            if (bPickoffFlag == true)
            {
                //견제정보 처리
                pickoffWaitDone();
            }

            if (bStealFlag == true)
            {
                //도루정보 처리
                stealWaitDone();
            }
            
            if (bBatterInfoWait == true)
            {
                //배터 인포 처리
                if (nPitchNum <= 1)
                {
                    //첫투구시
                    firstPitchWaitDone();
                }
                else
                {
                    //두번쨰 이후
                    nextPitchWaitDone();
                }
            }

            if (bPitchInfoWait == true)
            {
                //피치 인포 처리
                pitchingWaitDone();
            }

            if (bGuestChance == true)
            {
                //게스트의 찬스 팝업 처리
                guestChancePopup();
            }

            if (bDisconnectAndAiThrow == true)
            {
                ////Debug.Log("=================================>> 인공지능에 의한 투구 재개");
                StartCoroutine(manager.pitcher.startPichingAnim3());
                bDisconnectAndAiThrow = false;
            }
        }


        /// <summary>
        /// 인스턴스
        /// </summary>
        /// <returns></returns>
        public static PvpManager GetInstance()
        {
            return Instance_;
        }

        ////////////////////////////////////////////////////////////////////////////
        //초기화
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// PVP대전 초기화
        /// </summary>
        /// <param name="_manager"></param>
        public void Init(BallPlayManager _manager)
        {
            manager = _manager;

#if _Test_Local
            //이벤트 연결
            bReady = false;
            //pvp.OnClose += CloseEvent;
            //pvp.OnReady += ReadyEvent;
            //pvp.OnDisconnectOther += DisconnectOtherEvent;
            //pvp.OnDisconnect += DisconnectMineEvent;
            //pvp.OnMessage += RecieveDataEvent;

            //pvp.Connect();
#else
            // DISABLED_MGRS: pvp = Mgrs.userData.LivePVP_Service;

            //이벤트 연결
            bReady = false;
            //pvp.OnClose += CloseEvent;
            pvp.OnPairingSuccess += PairSuccessEvent;
            pvp.OnReadyToStart += ReadyEvent;
            pvp.OnDisconnectOther += DisconnectOtherEvent;
            pvp.OnDisconnect += DisconnectMineEvent;
            pvp.OnReceiveData += RecieveDataEvent;
            pvp.OnReceiveMessage += ReciveStringEvent;
#endif

        }


        /// <summary>
        /// InitBatter에서 호출
        /// </summary>
        public void InitBatter()
        {
            ////Debug.Log("===================>> InitBatter");
            bNewBatter = true;

            SendNewBatterInfo();

            nPitchNum = 0;
            bSwing = false;
            bHit = false;
            bBunt = false;
            
            bBattingResultUpdate = false;

            bStealFlag = bPickoffFlag = false;

            bBatterInfoWait = false;

            bPitchInfoWait = false;
        }

        /// <summary>
        /// SetPitch 에서 호출
        /// </summary>
        public void SetPitch()
        {
            ////Debug.Log("===================>> InitState");
            //waitState = WaitState.None;
            bCountSync = false;
            if (bNewBatter == true)
            {
                bNewBatter = false;
            }
            else
            {
                //볼카운트 동기화
                SendNoHitInfo(NoHitType.BallCountSync);
            }
            nPitchNum++;

            if (manager.bMyTurn == true)
            {
                manager.pitch.pitchOrigin.setPitchCursor(false);
            }
            bSwing = false;
            bHit = false;
            bBunt = false;
            bFieldState = false;            
            //bTip = false; 

            if (syncState == SyncAskState.Ask)
            {
                syncAskDone();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        //서버로 부터 받는 이벤트
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 클로즈 이벤트
        /// </summary>
        private void CloseEvent()
        {
            //Debug.Log("Pvp1 Close:");
        }

        /// <summary>
        /// 페어 석세스 이벤트
        /// bRePair가 true인 경우 재접속한 상태임
        /// </summary>
        private void PairSuccessEvent(bool bRePair)
        {
            //Debug.Log("PairSuccessEvent ==> bRePair : " + bRePair);
            connectState = ConnectState.Connect;
            if (bRePair == true)
            {
                manager.simulator.setReconnectProcess();
            }
        }

        /// <summary>
        /// 레디 이벤트
        /// </summary>
        private void ReadyEvent()
        {
            bReady = true;
            //Debug.Log("Pvp1 Ready");
        }

        /// <summary>
        /// 상대 디스커넥트 이벤트
        /// </summary>
        private void DisconnectOtherEvent()
        {
            ////Debug.Log("===========================================================>>>>> DisconnectOther");
            connectState = ConnectState.DisConnectOther;
            setDisconnect(true);
        }

        /// <summary>
        /// 나의 디스커넥스 이벤트
        /// </summary>
        private void DisconnectMineEvent()
        {
            ////Debug.Log("===========================================================>>>>> DisconnectMine");
            connectState = ConnectState.DisConnectMine;
            setDisconnect(false);
        }

        /// <summary>
        /// 상대가 전송한 메시지 수신
        /// </summary>
        /// <param name="data"></param>
        private void RecieveDataEvent(byte[] data)
        {
            ////Debug.Log("메세지 수신 = " + data[0]);

            object info = ObjectSerializationExtension.Deserialize<object>(data);
            //string type = info.GetType().ToString();
            ////Debug.Log("메세지 수신 = " + info.GetType());
            System.Type type = info.GetType();


            if (type == typeof(SendHostQuickInfo)) //if (type.Equals(hostQuickInfo) == true)
            {
                //호스트 퀵인포 수신
                recieveHostQuickInfo((SendHostQuickInfo)info);
            }
            else if (type == typeof(SendQuickGameReplyInfo)) //if (type.Equals(quickReplyInfo) == true)
            {
                //퀵게임의 응답값 확인
                recieveQuickGameReplyInfo((SendQuickGameReplyInfo)info);
            }
            else if (type == typeof(SendBatterInfo)) ////if (type.Equals(batterInfo) == true)
            {
                //새 타자 정보 수신
                if (manager.bMyTurn == false)
                {
                    recieveBatterInfo((SendBatterInfo)info);
                }
            }
            else if (type == typeof(SendPitchInfo)) //if (type.Equals(pitchInfo) == true)
            {
                //피치정보 수신
                if (manager.bMyTurn == true)
                {
                    recievePitchInfo((SendPitchInfo)info);
                }
            }
            else if (type == typeof(SendBattingInfo)) //if (type.Equals(battingInfo) == true)
            {
                //타격 정보 수신
                if (manager.bMyTurn == false)
                {
                    recieveBattingInfo((SendBattingInfo)info);
                }
            }
            else if (type == typeof(SendNoHitInfo)) //if (type.Equals(noHitInfo) == true)
            {
                //투구를 휘두르지 않은 정보 수신
                if (manager.bMyTurn == false)
                {
                    recieveNoHitInfo((SendNoHitInfo)info);
                }
            }
            /*
            else if (type == typeof(SendNoSwingInfo)) //if (type.Equals(noSwingInfo) == true)
            {
                //투구를 휘두르지 않은 정보 수신
                if (manager.bMyTurn == false)
                {
                    recieveNoSwingInfo((SendNoSwingInfo)info);
                }
            }
            else if (type == typeof(SendHutSwingInfo)) //if (type.Equals(hutSwingInfo) == true)
            {
                //헛스윙 정보 수신
                if (manager.bMyTurn == false)
                {
                    recieveHutSwingInfo((SendHutSwingInfo)info);
                }
            }*/
            else if (type == typeof(SendPowerBattingInfo)) //if (type.Equals(powerBattingInfo) == true)
            {
                //파워스윙 인포
                if (manager.bMyTurn == false)
                {
                    recievePowerBattingInfo((SendPowerBattingInfo)info);
                }
            }
            else if (type == typeof(SendPickOffInfo)) //if (type.Equals(pickoffInfo) == true)
            {
                //견제 정보
                if (manager.bMyTurn == true)
                {
                    recievePickoffInfo((SendPickOffInfo)info);
                }
            }
            else if (type == typeof(SendStealInfo)) //if (type.Equals(stealInfo) == true)
            {
                //도루 정보
                if (manager.bMyTurn == false)
                {
                    recieveStealInfo((SendStealInfo)info);
                }
            }
            /*
            //필드 동기화
            else if (type == typeof(SendRunnerSyncInfo)) //if (type.Equals(runnerSyncInfo) == true)
            {
                //러너 동기화 정보
                recieveRunnerSyncInfo((SendRunnerSyncInfo)info);
            }*/
            else if (type == typeof(SendFieldSyncInfo)) //if (type.Equals(fieldSyncInfo) == true)
            {
                //필드 동기화 정보
                recieveFieldSyncInfo((SendFieldSyncInfo)info);
            }
            else if (type == typeof(SendGameSync)) //if (type.Equals(sendSync) == true)
            {
                //게임 재 동기화
                recieveGameSync((SendGameSync)info);
            }
            else if (type == typeof(SendAskSync)) //if (type.Equals(syncAsk) == true)
            {
                //게임 동기화 요청
                recieveAskSync((SendAskSync)info);
            }            
            else if (type == typeof(SendResultSync)) //if (type.Equals(resultSyncInfo) == true)
            {
                //결과 동기화
                recieveResultSyncInfo((SendResultSync)info);
            }
            else if (type == typeof(SendChangePlayerSync)) //if (type.Equals(changeSyncInfo) == true)
            {
                //선수교체 동기화
                recieveChangeSyncInfo((SendChangePlayerSync)info);
            }

        }

        /// <summary>
        /// 데이터 전송
        /// </summary>
        /// <param name="data"></param>
        private void SendData(byte[] data)
        {
#if _Test_Local
            //pvp.SendData(data);
#else
            pvp.SendGameData(data);
#endif
        }


        /// <summary>
        /// 문자열 메세지
        /// </summary>
        /// <param name="message"></param>
        private string getMessage;
        private void ReciveStringEvent(string message)
        {
            getMessage = message;
            bEmoticonFlag = true;
        }

        /*
        public string GetMessage()
        {
            return getMessage;
        }*/

        /// <summary>
        /// 문자열 메세지 전송
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
#if _Test_Local

#else
            pvp.SendMessage(message);
#endif
        }



        /// <summary>
        /// 2명의 플레이어가 connect되었는지 여부
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
#if _Test_Local
            return true;
#else
            return bReady;
#endif
        }



        ////////////////////////////////////////////////////////////////////////////
        //정보 수신후 상태 세팅
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 첫투구시 피치UI를 화면에 출력할 수 있는 상태가 됨
        /// </summary>
        private void firstPitchWaitDone()
        {            
            //스킬연출과 투수 UI출력
            StartCoroutine(manager.battingviewSkillEffect(true));

            bBatterInfoWait = false;
        }


        /// <summary>
        /// 두번째 이후 투구시 피치 UI를 화면에 출력할 수 있는 상태가 됨
        /// </summary>
        private void nextPitchWaitDone()
        {
            if (bBattingResultUpdate == true)
            {
                manager.settingPitchingUIbyForce();

                bBatterInfoWait = false;
                bBattingResultUpdate = false;
            }
            else
            {
                //타임 체크
                //여기서 타임아웃처리되면 동기화 요청
                ////Debug.Log("=======================>>여기서 타임아웃처리되면 동기화 요청");
            }
        }

        /// <summary>
        /// 타자가 투수의 투구 정보를 완벽하게 수신한 상태
        /// </summary>
        private void pitchingWaitDone()
        {
            PitchingArsenal index = manager.pitcher.selectedBallIndex;
            manager.pitcher.setBallAndGuwee(index);
            manager.pitcher.coursePvpX -= (manager.pitcher.preHenkaX);
            manager.pitcher.coursePvpY -= (manager.pitcher.preHenkaY);
            manager.pitcher.courseX = manager.pitcher.courseX2 = manager.pitcher.coursePvpX;
            manager.pitcher.courseY = manager.pitcher.courseY2 = manager.pitcher.coursePvpY;

            manager.pitch.pitchOrigin.setPvpZoneInit(manager.pitcher.courseX, manager.pitcher.courseY, Zone.STRIKE_ZONE_WIDTH, Zone.STRIKE_ZONE_HEIGHT);
            manager.pitcher.startPitchingAnim();

            bPitchInfoWait = false;
        }

        /// <summary>
        /// 견제정보를 수신한 상태
        /// </summary>
        private void pickoffWaitDone()
        {
            PvpManager.RandomSeed = PickoffInfo.RandomSeed;
            int target = PickoffInfo.nTargetIndex;

            IngameUI.GetScoreBoard().SetPitchTimerActive(false);    //피치타이머 디액티브
            IngameUI.GetControlRunner().setPickoff(target);         //픽오프
            bPickoffFlag = false;
        }

        /// <summary>
        /// 도루 정보를 수신한 상태
        /// </summary>
        private void stealWaitDone()
        {
            ////Debug.Log("============>> 도루 세팅");
            PvpManager.RandomSeed = StealInfo.RandomSeed;
            int target = StealInfo.nTargetIndex;
            IngameUI.GetControlRunner().setBaseSteal(target);
            bStealFlag = false;
        }


        /// <summary>
        /// 동기화 요청을 받은 경우 처리
        /// </summary>
        private void syncAskDone()
        {
            //Debug.Log("동기화 요청 수신후 처리 -> 동기화 정보를 보낸다");
            SendGameSync();
            syncState = SyncAskState.None;
        }

        /// <summary>
        /// 동기화 정보를 받은 경우 처리
        /// </summary>
        private void syncSendDone()
        {
            //Debug.Log("동기화 정보를 받은 후 호스트와 동기화 한다.");
            ///동기화 정보 이곳에서 처리
            if (gameSyncInfo != null)
            {
                bool bLastMyTurn = manager.bMyTurn;
                manager.bMyTurn = gameSyncInfo.bMyTurn;

                if (bLastMyTurn == manager.bMyTurn)
                {
                    //이닝교체가 아닌경우 공격팀 카운트 하나 빼줌
                    int myLineup = gameSyncInfo.batterLineupCount[0] - (manager.bMyTurn ? 1 : 0);
                    SimulPlayerManager.SetLineup(0, (9 + myLineup) % 9);
                    int cpuLineup = gameSyncInfo.batterLineupCount[1] - (manager.bMyTurn ? 0 : 1);
                    SimulPlayerManager.SetLineup(1, (9 + cpuLineup) % 9);
                }
                else
                {
                    //이닝교체시
                    SimulPlayerManager.SetLineup(0, gameSyncInfo.batterLineupCount[0]);
                    SimulPlayerManager.SetLineup(1, gameSyncInfo.batterLineupCount[1]);
                }

                manager.nInningCount = gameSyncInfo.inning;
                manager.nBallCount = gameSyncInfo.ballCount;
                manager.nStrikeCount = gameSyncInfo.strikeCount;
                manager.nOutCount = gameSyncInfo.outCount;

                for (int i = 0; i < 2; i++)
                {
                    manager.nGameScore[i] = gameSyncInfo.scoreNum[i];
                    manager.nHitCount[i] = gameSyncInfo.hitNum[i];
                    manager.nErrorCount[i] = gameSyncInfo.errorNum[i];
                }

                manager.field.run.transform.DestroyChildren();// .checkDestroyRunner
                for (int i = 0; i < 4; i++) manager.field.run.runnerActive[i] = false;


                int team = manager.bMyTurn ? 0 : 1;

                for (int i = 0; i < 3; i++)
                {
                    manager.field.run.bOnBase[i] = gameSyncInfo.bOnBase[i];
                    if (manager.field.run.bOnBase[i] == true)
                    {
                        int index = gameSyncInfo.runnerIndex[i];
                        if (index != -1)
                        {
                            CPlayer runner = SimulPlayerManager.GetFielder(team, index);
                            manager.field.run.makeChanceRunner(runner, i);
                        }
                    }
                }

                IngameUI.GetScoreBoard().BoardUpdate();
            }
            ///
            syncState = SyncAskState.Done;
        }



        /// <summary>
        /// 게스트의 찬스 팝업 처리
        /// </summary>
        private void guestChancePopup()
        {
            manager.simulator.SetPVPChancePopup();
            bGuestChance = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        //자동플레이 송신 수신
        ////////////////////////////////////////////////////////////////////////////

        public SendHostQuickInfo sendQuickInfo;

        private void recieveHostQuickInfo(SendHostQuickInfo info)
        {
            //Debug.Log("호스트 -> 게스트 퀵인포 수신");
            
            sendQuickInfo = info;            
            rState = RecieveState.HostQuickInfo;
        }

        public void SendHostQuickInfo(SimulBattingData battingData, QuickGameInfo qInfo, CPlayer pitcher)
        {
            //Debug.Log("호스트 -> 게스트 퀵인포 발신");            
            SendHostQuickInfo info = new SendHostQuickInfo();
            info.set(manager, battingData, qInfo, pitcher);
            //byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
            //////Debug.Log("===========>> byte size = " + data.Length);
            //SendData(data);

            byte[] data = info.SerializeToByteArray();
            //////Debug.Log("===========>> byte size = " + data.Length);
            SendData(data);

        }

        private void recieveQuickGameReplyInfo(SendQuickGameReplyInfo info)
        {
            ApplyInfo type = (ApplyInfo)info.type;
            if (type == ApplyInfo.Init)
            {
                //Debug.Log("초기화 완료");
                bGameReady = true;
            }
            else if (type == ApplyInfo.SendReply)
            {
                //Debug.Log("게스트 -> 호스트 퀵인포 수신");
                rState = RecieveState.GuestQuickInfo;
            }
            else if (type == ApplyInfo.ChanceHost)
            {
                //Debug.Log("호스트의 찬스");
                chanceState = ChanceState.ChanceWait;
            }
            else if (type == ApplyInfo.ChanceGuest)
            {
                //Debug.Log("게스트의 찬스");
                chanceState = ChanceState.ChanceSelect;
                bGuestChance = true;
            }
            else if (type == ApplyInfo.ChanceAccept)
            {
                //Debug.Log("찬스 수락");
                chanceState = ChanceState.ChanceAccept;
            }
            else if (type == ApplyInfo.ChanceDecline)
            {
                //Debug.Log("찬스 거부");
                chanceState = ChanceState.ChanceDecline;
            }
            else if (type == ApplyInfo.HostChange)
            {
                //Debug.Log("호스트 교체");
                manager.simulator.setHost(true);
            }
            else if (type == ApplyInfo.ReconnectAsked)
            {
                //Debug.Log("재접속을 요구받음!!!");
                manager.simulator.setReconnectAsked();
            }
            else if (type == ApplyInfo.ReconnectDone)
            {
                //Debug.Log("재접속완료");
                manager.simulator.setReconnectDone();
            }
            else if (type == ApplyInfo.ChangeWait)
            {
                //Debug.Log("선수교체 대기 팝업");
                changeWaitState = ChangeWaitState.Wait;
            }
            else if (type == ApplyInfo.ChangeFinish)
            {
                //Debug.Log("선수교체 대기 팝업 대기 종료");
                changeWaitState = ChangeWaitState.Finish;
            }
            else if (type == ApplyInfo.PitchSelect)
            {
                //Debug.Log("피치 셀렉트시 topui없앰");
                changeWaitState = ChangeWaitState.PitchSelect;
            }
            else if (type == ApplyInfo.PitchTimer)
            {
                //Debug.Log("피치 타이머");
                changeWaitState = ChangeWaitState.PitchTimer;
            }
            else if (type == ApplyInfo.OtherForceDisconnect)
            {
                //Debug.Log("강제 디스커넥");
                forceDisconnect();
            }
            else if (type == ApplyInfo.SkipAsk)
            {
                Mode.SkipAsk = true;
            }
            else if (type == ApplyInfo.GameEnd)
            {
                bGameEndAsk = true;
            }
        }

        public void SendQuickGameReplyInfo(ApplyInfo type)
        {
            //Debug.Log("응답 정보 발신 현재 정보 : "+type);

            SendQuickGameReplyInfo info = new SendQuickGameReplyInfo();
            info.set(type);

            byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
            SendData(data);
        }

        ////////////////////////////////////////////////////////////////////////////
        //결과 동기화 
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 결과 동기화 정보 수신
        /// </summary>
        /// <param name="info"></param>
        private SendResultSync resultSync;
        private void recieveResultSyncInfo(SendResultSync info)
        {
            //Debug.Log("recieveResultSyncInfo");
            resultSync = info;

            bool bEnd = info.bGameEnd;

            if (bEnd == true)
            {
                rState = RecieveState.ResultSync;
            }
            else
            {
                SetResultSync();
            }
        }

        /// <summary>
        /// 동기화 정보로부터 각각의 결과 값들을 동기화 시킨다
        /// </summary>
        public void SetResultSync()
        {
            if (resultSync != null)
            {
                bool bEnd = resultSync.bGameEnd;
                for (int i = 0; i < 2; i++)
                {
                    manager.nGameScore[i] = resultSync.gameScore[i];
                    for (int j = 0; j < 12; j++)
                    {
                        manager.nInningScore[i, j] = resultSync.inningScore[i, j];
                    }
                    manager.nHitCount[i] = resultSync.hit[i];
                    manager.nErrorCount[i] = resultSync.error[i];
                    manager.nFourballCount[i] = resultSync.fourballCount[i];
                    manager.nStrikeOutCount[i] = resultSync.strikeOutCount[i];
                    manager.nHomerunCount[i] = resultSync.homerunCount[i];
                    manager.nDPCount[i] = resultSync.dpCount[i];
                    manager.nStealCount[i] = resultSync.stealCount[i];
                }

                //타자 기록 동기화
                for (int i = 0; i < 14; i++)
                {
                    CPlayer player = SimulPlayerManager.GetFielder(0, i);
                    player.setRecordInit();
                    player.setRecord(Param.ST_AB, resultSync.myBatter[i, 0]);
                    player.setRecord(Param.ST_H, resultSync.myBatter[i, 1]);
                    player.setRecord(Param.ST_HR, resultSync.myBatter[i, 2]);
                    player.setRecord(Param.ST_RBI, resultSync.myBatter[i, 3]);
                    player.setRecord(Param.ST_SBS, resultSync.myBatter[i, 4]);
                    player.setRecord(Param.ST_BB, resultSync.myBatter[i, 5]);
                    player.setRecord(Param.ST_R, resultSync.myBatter[i, 6]);

                    CPlayer player2 = SimulPlayerManager.GetFielder(1, i);
                    player2.setRecordInit();
                    player2.setRecord(Param.ST_AB, resultSync.cpuBatter[i, 0]);
                    player2.setRecord(Param.ST_H, resultSync.cpuBatter[i, 1]);
                    player2.setRecord(Param.ST_HR, resultSync.cpuBatter[i, 2]);
                    player2.setRecord(Param.ST_RBI, resultSync.cpuBatter[i, 3]);
                    player2.setRecord(Param.ST_SBS, resultSync.cpuBatter[i, 4]);
                    player2.setRecord(Param.ST_BB, resultSync.cpuBatter[i, 5]);
                    player2.setRecord(Param.ST_R, resultSync.cpuBatter[i, 6]);
                }

                //투수기록 동기화
                for (int i = 0; i < 11; i++)
                {
                    CPlayer myplayer = SimulPlayerManager.GetPitcher(0, i);
                    myplayer.setRecordInit();
                    myplayer.setRecord(Param.ST_IP, resultSync.myPitcher[i, 0]);
                    myplayer.setRecord(Param.ST_PR, resultSync.myPitcher[i, 1]);
                    myplayer.setRecord(Param.ST_PER, resultSync.myPitcher[i, 2]);
                    myplayer.setRecord(Param.ST_PSO, resultSync.myPitcher[i, 3]);
                    myplayer.setRecord(Param.ST_PH, resultSync.myPitcher[i, 4]);
                    myplayer.setRecord(Param.ST_PBB, resultSync.myPitcher[i, 5]);
                    myplayer.setRecord(Param.ST_PHR, resultSync.myPitcher[i, 6]);
                    myplayer.setRecord(Param.ST_PNP, resultSync.myPitcher[i, 7]);

                    if (bEnd == true)
                    {
                        //승패세홀드블론
                        if (resultSync.pitcherAchieve[i, 0] == Param.ST_PW) myplayer.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_COMPLETE);
                        else if (resultSync.pitcherAchieve[i, 0] == Param.ST_PL) myplayer.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_COMPLETE);
                        else if (resultSync.pitcherAchieve[i, 0] == Param.ST_HLD) myplayer.setPitcherAchieve(Param.ST_HLD, Param.P_ACHIEVE_COMPLETE);
                        else if (resultSync.pitcherAchieve[i, 0] == Param.ST_SV) myplayer.setPitcherAchieve(Param.ST_SV, Param.P_ACHIEVE_COMPLETE);
                        else if (resultSync.pitcherAchieve[i, 0] == Param.ST_BS) myplayer.setPitcherAchieve(Param.ST_BS, Param.P_ACHIEVE_COMPLETE);
                    }

                    CPlayer cpuplayer = SimulPlayerManager.GetPitcher(1, i);
                    cpuplayer.setRecordInit();
                    cpuplayer.setRecord(Param.ST_IP, resultSync.cpuPatter[i, 0]);
                    cpuplayer.setRecord(Param.ST_PR, resultSync.cpuPatter[i, 1]);
                    cpuplayer.setRecord(Param.ST_PER, resultSync.cpuPatter[i, 2]);
                    cpuplayer.setRecord(Param.ST_PSO, resultSync.cpuPatter[i, 3]);
                    cpuplayer.setRecord(Param.ST_PH, resultSync.cpuPatter[i, 4]);
                    cpuplayer.setRecord(Param.ST_PBB, resultSync.cpuPatter[i, 5]);
                    cpuplayer.setRecord(Param.ST_PHR, resultSync.cpuPatter[i, 6]);
                    cpuplayer.setRecord(Param.ST_PNP, resultSync.cpuPatter[i, 7]);

                    if (bEnd == true)
                    {
                        //승패세홀드블론
                        if (resultSync.pitcherAchieve[i, 1] == Param.ST_PW) cpuplayer.setPitcherAchieve(Param.ST_PW, Param.P_ACHIEVE_COMPLETE);
                        else if (resultSync.pitcherAchieve[i, 1] == Param.ST_PL) cpuplayer.setPitcherAchieve(Param.ST_PL, Param.P_ACHIEVE_COMPLETE);
                        else if (resultSync.pitcherAchieve[i, 1] == Param.ST_HLD) cpuplayer.setPitcherAchieve(Param.ST_HLD, Param.P_ACHIEVE_COMPLETE);
                        else if (resultSync.pitcherAchieve[i, 1] == Param.ST_SV) cpuplayer.setPitcherAchieve(Param.ST_SV, Param.P_ACHIEVE_COMPLETE);
                        else if (resultSync.pitcherAchieve[i, 1] == Param.ST_BS) cpuplayer.setPitcherAchieve(Param.ST_BS, Param.P_ACHIEVE_COMPLETE);
                    }
                }
            }
        }

        /// <summary>
        /// 결과 동기화 정보 송신
        /// </summary>
        public void SendResultSyncInfo(bool bGameEnd)
        {
            //Debug.Log("결과 동기화 정보 송신");

            SendResultSync info = new SendResultSync();
            info.set(manager, bGameEnd);

            byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);

            ////Debug.Log("===================>>결과 동기화 크기 : " + data.Length);

            SendData(data);
        }

        ////////////////////////////////////////////////////////////////////////////
        //선수교체정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 선수교체 동기화 정보 수신
        /// </summary>
        /// <param name="info"></param>
        private SendChangePlayerSync changeSync;
        private void recieveChangeSyncInfo(SendChangePlayerSync info)
        {
            //Debug.Log("recieveChangeSyncInfo");
            changeSync = info;
            changeWaitState = ChangeWaitState.ChangeEvent;
        }


        /// <summary>
        /// 선수교체 동기화 정보 송신
        /// </summary>
        public void SendChangeSyncInfo(bool _bMyTeam, UIPlayerChange.PlayerChangeType _type, int _outIndex, int _inIndex, int _index)
        {
            //Debug.Log("선수교체 정보 송신");

            SendChangePlayerSync info = new SendChangePlayerSync();
            info.set(_bMyTeam, _type, _outIndex, _inIndex, _index);

            byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);

            ////Debug.Log("===================>>결과 동기화 크기 : " + data.Length);

            SendData(data);
        }

        ////////////////////////////////////////////////////////////////////////////
        //타자정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 새 타자 정보 수신
        /// </summary>
        /// <param name="data"></param>
        private void recieveBatterInfo(SendBatterInfo info1)
        {
            ////Debug.Log("==========================================================>>recieveBatterInfo");
            rState = RecieveState.BatterInfo;
            PvpManager.RandomSeed2 = info1.RandomSeed;
            
            CSkill batterSkill = null;
            CSkill pitcherSkill = null;

            ////Debug.Log("==========================================================>>1");

            if (info1.batterSkill != -1)
            {
                batterSkill = manager.batter.pBatter.getSkillValue((SkillIndex)info1.batterSkill);
            }

            ////Debug.Log("==========================================================>>2");

            if (info1.pitcherSkill != -1)
            {
                pitcherSkill = manager.pitcher.pPitcher.getSkillValue((SkillIndex)info1.pitcherSkill);
            }

            ////Debug.Log("==========================================================>>3");

            //스킬 동기화
            SimulManager.SetBatterSkill(batterSkill);
            SimulManager.SetPitcherSkill(pitcherSkill);
            SimulManager.SetVsBatterWin(info1.bOffenseSkillWin);
            manager.batterSkillFlag = (SkillFlag)info1.batterSkillFlag;
            manager.vsType = info1.bVsType;

            ////Debug.Log("==========================================================>>4");
            //에러 플래그 동기화
            manager.field.setErrorFlagSync(info1.bCatchError, info1.bThrowError);

            ////Debug.Log("==========================================================>>5 완료");
        }

        /// <summary>
        /// 다음타자 정보 보내기 
        /// 발동된 투타 정보 포함 : 타자 -> 투수
        /// </summary>
        public void SendNewBatterInfo()
        {
            if (Mode.bSimulationQuickPlay == true) return;

            if (manager.bMyTurn == true)
            {
                ////Debug.Log("==========================================================>>SendNewBatterInfo");

                manager.checkBattingviewSkill();

                PvpManager.RandomSeed2 = (int)System.DateTime.Now.Ticks;
                                
                SendBatterInfo info = new SendBatterInfo();
                info.Set(manager, PvpManager.RandomSeed2);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }

        /// <summary>
        /// 다음타자 정보 받는 대기 상태로 변환
        /// 발동된 투타 정보 포함 : 타자 -> 투수
        /// </summary>
        public void WaitNewBatterInfo()
        {
            if (manager.bMyTurn == false)
            {
                //////Debug.Log("============>>nPitchNum = "+nPitchNum);
                StartCoroutine(waitBatterInfo());
            }
        }

        private IEnumerator waitBatterInfo()
        {
            if (nPitchNum <= 1)
            {
                //첫투구시
                ////Debug.Log("===================>>새 배터 정보 투수가 수신");
                while (rState != RecieveState.BatterInfo)
                {
                    if (connectState != ConnectState.Connect)
                    {
                        ////Debug.Log("===================>>상대 연결이 끊겨 강제로 pvp모드에서 빠져나옴");
                        Mode.bPvpMode = false;
                        yield break;
                    }
                    else
                    {
                        ////Debug.Log("===================>>rState = " + rState);
                        if (bWaitStateQuit == true)
                        {
                            ////Debug.Log("===================>>강재로 대기상태 빠져나감");
                            bWaitStateQuit = false;
                            yield break;
                        }
                        //배터 인포가 안들어 왔을 경우 대기
                        yield return new WaitForSeconds(0.3f);
                    }
                }                
            }
            //두번째 투구는 직행으로
            //Debug.Log("rState를 None으로 세팅");
            rState = RecieveState.None;
            bBatterInfoWait = true;

        }


        ////////////////////////////////////////////////////////////////////////////
        //피치 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 피치 정보 수신
        /// </summary>
        /// <param name="data"></param>
        private void recievePitchInfo(SendPitchInfo info2)
        {
            rState = RecieveState.PitchInfo;
            PvpManager.RandomSeed = info2.RandomSeed;
            //Debug.Log("받는 랜덤 시드=================>> " + PvpManager.RandomSeed);
            
            //볼스피드
            manager.pitcher.curBallSpeed = info2.curBallSpeed;
            //선택구종
            manager.pitcher.selectedBallIndex = (PitchingArsenal)info2.selectBallType; // -> setBallAndGuwee()함수에 넣어
            //미스 여부
            manager.pitcher.bMissControl = info2.bMissControl;
            //유저 컨트롤 밸류
            manager.pitcher.userControlValue =  (UserControlValue)info2.userControlValue; // 

            manager.pitcher.coursePvpX = info2.courseX;
            manager.pitcher.coursePvpY = info2.courseY;
            ////Debug.Log("전달받은값===============>> courseX = " + manager.pitcher.coursePvpX);
            ////Debug.Log("전달받은값===============>> courseY = " + manager.pitcher.coursePvpY);

            manager.pitcher.hitByPitchStep = info2.hitByPitchStep;

            ////Debug.Log("========================================>> 1");

            int pSkill = info2.pitchSkill;
            if (pSkill != -1)
            {
                SimulManager.SetPitchPitcherSkill(manager.pitcher.pPitcher.getSkillValue((SkillIndex)pSkill));
            }
            else
            {
                SimulManager.SetPitchPitcherSkill(null);
            }

            ////Debug.Log("========================================>> 2");

            int bSkill = info2.battingSkill;
            if (bSkill != -1)
            {
                SimulManager.SetPitchBatterSkill(manager.batter.pBatter.getSkillValue((SkillIndex)bSkill));
            }
            else
            {
                SimulManager.SetPitchBatterSkill(null);
            }

            ////Debug.Log("========================================>> 3");

            int cSkill = info2.catcherSkill;
            if (cSkill != -1)
            {
                SimulManager.SetPitchCatcherSkill(manager.field.fielder[CPlayer._CATCHER].pFielder.getSkillValue((SkillIndex)cSkill));
            }
            else
            {
                SimulManager.SetPitchCatcherSkill(null);
            }

            ////Debug.Log("========================================>> 4 완료");
        }

        /// <summary>
        /// 투구 정보 보내기 
        /// 투수 -> 타자
        /// </summary>
        public void SendPitchingInfo()
        {
            if (manager.bMyTurn == false) //타자쪽 동기화 테스트시 비활성화(PVP테스트_반드시_복구)
            {
                //////Debug.Log("===================>>투구 정보를 타자에게 보냄");
                PvpManager.RandomSeed = (int)System.DateTime.Now.Ticks;

                //Debug.Log("보내는 랜덤 시드=================>> " + PvpManager.RandomSeed);

                SendPitchInfo info = new SendPitchInfo();
                info.Set(manager, PvpManager.RandomSeed);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }
        
        /// <summary>
        /// 투구 정보 대기 상태로 변환
        /// 투수 -> 타자
        /// </summary>
        public void WaitPitchingInfo()
        {
            if (manager.bMyTurn == true)
            {
                StartCoroutine(waitPitchInfo());
            }
        }

        private IEnumerator waitPitchInfo()
        {
            while (rState != RecieveState.PitchInfo)
            {
                if (connectState != ConnectState.Connect)
                {
                    ////Debug.Log("===================>>상대 연결이 끊겨 강제로 pvp모드에서 빠져나옴");
                    Mode.bPvpMode = false;
                    yield break;
                }
                else
                {
                    //피치 인포가 들어오지 않으면 대기
                    if (bWaitStateQuit == true)
                    {
                        ////Debug.Log("===================>>강재로 대기상태 빠져나감");
                        bWaitStateQuit = false;
                        yield break;
                    }
                    yield return new WaitForSeconds(0.3f);
                }
            }            
            bPitchInfoWait = true;
            //Debug.Log("rState를 None으로 세팅");
            rState = RecieveState.None;
        }

        ////////////////////////////////////////////////////////////////////////////
        //배팅 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 배팅정보 수신
        /// </summary>
        /// <param name="data"></param>
        private SendBattingInfo BattingInfo = null;
        private void recieveBattingInfo(SendBattingInfo info3)
        {
            //
            BattingInfo = info3;

            bool bunt = BattingInfo.bBunt;

            bSwing = !bunt;
            bBunt = bunt;
            //bTip = BattingInfo.bTip;

            bHit = true;

            manager.batter.bCheckSwing = false; //체크스윙 예외처리
            //타자주자 터보스킬 동기화
            manager.field.run.getHitterRunner().bTurboSkillOn = info3.bTurboOn;

            bFieldState = true;

            rState = RecieveState.BattingInfo;
        }

        /// <summary>
        /// 타구 정보 보내기 
        /// 타자 -> 투수
        /// </summary>
        public void SendBattingInfo()
        {
            if (manager.bMyTurn == true)
            {
                //////Debug.Log("===========================>> 타격정보 보냄");
                SendBattingInfo info = new SendBattingInfo();
                info.Set(manager);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        //노힛 정보 관련
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 노힛 정보 수신
        /// </summary>
        /// <param name="info"></param>
        private void recieveNoHitInfo(SendNoHitInfo info)
        {
            //배트에 공이 맞지 않은 경우 정보 수신
            NoHitType type = (NoHitType)info.type;
            manager.pitcher.bWildPitch = info.bWildPitch;
            if (type == NoHitType.NoSwing)
            {
                ////Debug.Log("===========================>> 스윙을 하지 않은 경우");
                manager.bStrikeCheck = info.bStrikeCheck;
                manager.batter.bSwing = info.bSwing;
                bSwing = manager.batter.bSwing;
                bBattingResultUpdate = true;
            }
            else if (type == NoHitType.HutSwing)
            {
                ////Debug.Log("===========================>> 헛스윙인 경우");
                bBunt = info.bBunt;
                if (bBunt == false)
                {
                    //스윙시
                    bSwing = true;
                    manager.batter.bCheckSwing = info.bCheckSwing;
                }
                else
                {
                    //번트 스윙시
                    bSwing = false;
                    manager.batter.bCheckSwing = false;
                }
                bHit = false;
                bBattingResultUpdate = true;
            }
            else if (type == NoHitType.BallCountSync)
            {
                ////Debug.Log("===========================>> 볼카운트 동기화");
                manager.nBallCount = info.ball;
                manager.nStrikeCount = info.strike;
                manager.nOutCount = info.outCount;
                manager.setNewCount();
                bCountSync = true;
            }
        }

        /// <summary>
        /// 노 힛 정보 송신
        /// 타자 -> 투수
        /// </summary>
        public void SendNoHitInfo(NoHitType type)
        {
            if (manager.bMyTurn == true)
            {
                ////Debug.Log("===========================>> 노 힛 정보 보내기");
                SendNoHitInfo info = new SendNoHitInfo();
                info.Set(type, manager);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        //노스윙 정보 송신 수신 -> 삭제
        ////////////////////////////////////////////////////////////////////////////
        /*
        /// <summary>
        /// 투구를 휘두르지 않은 상태 정보 수신
        /// </summary>
        //private SendNoSwingInfo NoSwingInfo = null;
        private void recieveNoSwingInfo(SendNoSwingInfo info4)
        {
            //NoSwingInfo = info4;

            manager.pitcher.bWildPitch = info4.bWildPitch;
            manager.bStrikeCheck = info4.bStrikeCheck;
            manager.batter.bSwing = info4.bSwing;
            bSwing = manager.batter.bSwing;

            bBattingResultUpdate = true;
        }

        /// <summary>
        /// 스윙을 하지 않은 상태 보내기 
        /// 타자 -> 투수
        /// </summary>
        public void SendNoSwingInfo()
        {
            if (manager.bMyTurn == true)
            {
                ////Debug.Log("===========================>> 노 스윙 정보 보내기");
                SendNoSwingInfo info = new SendNoSwingInfo();
                info.Set(manager);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }*/
        
        ////////////////////////////////////////////////////////////////////////////
        //헛스윙 정보 송신 수신  -> 삭제
        ////////////////////////////////////////////////////////////////////////////
        /*
        /// <summary>
        /// 헛스윙 정보 수신
        /// </summary>
        /// <param name="info5"></param>
        private void recieveHutSwingInfo(SendHutSwingInfo info5)
        {
            bBunt = info5.bBunt;
            if (bBunt == false)
            {
                bSwing = true;
                manager.batter.bCheckSwing = info5.bCheckSwing;
            }
            else
            {
                bSwing = false;
                manager.batter.bCheckSwing = false;
            }
            
            bHit = false;

            bBattingResultUpdate = true;
        }
        
        /// <summary>
        /// 헛스윙 정보 보내기 
        /// 타자 -> 투수
        /// </summary>
        public void SendHutSwingInfo()
        {
            if (manager.bMyTurn == true)
            {
                ////Debug.Log("===========================>> 헛스윙한 정보 보냄");
                SendHutSwingInfo info = new SendHutSwingInfo();
                info.Set(manager);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }*/

        ////////////////////////////////////////////////////////////////////////////
        //파워배팅 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 파워배팅 여부
        /// </summary>
        /// <param name="info6"></param>
        private void recievePowerBattingInfo(SendPowerBattingInfo info6)
        {
            manager.batter.bGangTa = info6.bPowerBatting;
        }

        /// <summary>
        /// 파워배팅 정보 보냄
        /// </summary>
        public void SendPowerBattingInfo()
        {
            if (manager.bMyTurn == true)
            {
                //////Debug.Log("===========================>> 파워 배팅 정보 보냄");

                SendPowerBattingInfo info = new SendPowerBattingInfo();
                info.Set(manager);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        //견제 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 견제 정보 수신
        /// </summary>
        /// <param name="info7"></param>
        private SendPickOffInfo PickoffInfo;
        private void recievePickoffInfo(SendPickOffInfo info7)
        {
            PickoffInfo = info7;
            bPickoffFlag = true;
        }
        
        /// <summary>
        /// 견제 정보 보내기
        /// </summary>
        public void SendPickOffInfo(int target)
        {
            if (manager.bMyTurn == false)
            {
                IngameUI.GetScoreBoard().SetPitchTimerActive(false); //견제시 피치 타이머 디액티브

                PvpManager.RandomSeed = (int)System.DateTime.Now.Ticks;

                SendPickOffInfo info = new SendPickOffInfo();
                info.Set(target, PvpManager.RandomSeed);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        //도루 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 도루 정보 수신
        /// </summary>
        /// <param name="info7"></param>
        private SendStealInfo StealInfo;
        private void recieveStealInfo(SendStealInfo info8)
        {
            ////Debug.Log("================>> 도루정보 수신");
            StealInfo = info8;
            bStealFlag = true;
        }

        /// <summary>
        /// 도루 정보 보내기
        /// </summary>
        public void SendStealInfo(int target)
        {
            if (manager.bMyTurn == true)
            {
                ////Debug.Log("================>> 도루정보 보냄");
                PvpManager.RandomSeed = (int)System.DateTime.Now.Ticks;

                SendStealInfo info = new SendStealInfo();
                info.Set(target, PvpManager.RandomSeed);

                byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
                SendData(data);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        //필드 주루 동기화 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /*/// <summary>
        /// 주루 동기화 정보
        /// </summary>
        /// <param name="info9"></param>
        private void recieveRunnerSyncInfo(SendRunnerSyncInfo info9)
        {            
            int index = info9.arrayIndex;
            ////Debug.Log("================>> 주자 동기화 정보 수신 index : " + index);
            manager.field.run.runner[index].oneMoreBaseCheckValue = info9.timeValue;
        }

        /// <summary>
        /// 주루 동기화 정보 보내기
        /// </summary>
        /// <param name="index"></param>
        /// <param name="tValue"></param>
        public void SendRunnerSyncInfo(int index, float tValue)
        {
            //Debug.Log("주자 동기화 정보 보내기 index : " + index);
            SendRunnerSyncInfo info = new SendRunnerSyncInfo();
            info.Set(index, tValue);

            byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
            SendData(data);
        }*/

        ////////////////////////////////////////////////////////////////////////////
        //필드 수비 동기화 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 필드 동기화 정보
        /// </summary>
        /// <param name="info10"></param>
        private void recieveFieldSyncInfo(SendFieldSyncInfo info10)
        {
            FieldSyncType type = (FieldSyncType)info10.type;
            //////Debug.Log("================>> 필드 동기화 정보 수신 type : " + type);
            //////Debug.Log("================>> 필드 동기화 정보 수신 index : " + info10.arrayIndex);
            //////Debug.Log("================>> 필드 동기화 정보 수신 value : " + info10.value);
            int index = info10.arrayIndex;
            if (type == FieldSyncType.Target)
            {
                manager.field.netTarget[index] = (int)info10.value;
                //////Debug.Log("================>> manager.field.netTarget[index] : " + manager.field.netTarget[index]);
            }
            else if (type == FieldSyncType.OneMoreValue)
            {
                manager.field.netOneMoreValue[index] = info10.value;
                //////Debug.Log("================>> manager.field.netOneMoreValue[index] : " + manager.field.netOneMoreValue[index]);
            }
            else if (type == FieldSyncType.BaseSafe)
            {
                manager.field.netBaseSafe[index] = (info10.value == 0 ? false : true);
                //////Debug.Log("================>> manager.field.netBaseSafe[index] : " + manager.field.netBaseSafe[index]);
            }


        }


        /// <summary>
        /// 필드 동기화 정보
        /// </summary>
        public void SendFieldSyncInfo(FieldSyncType type, int index, float value)
        {
            //Debug.Log("필드 동기화 정보 보내기 type : "+type + "    index : " + index);
            SendFieldSyncInfo info = new SendFieldSyncInfo();
            info.Set(type, index, value);
            byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
            SendData(data);
        }


        ////////////////////////////////////////////////////////////////////////////
        //게임 동기화 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 게임 동기화 정보
        /// </summary>
        /// <param name="info11"></param>
        private SendGameSync gameSyncInfo = null;
        private void recieveGameSync(SendGameSync info11)
        {
            ////Debug.Log("============>> 동기화 게임정보 수신");
            gameSyncInfo = info11;
            syncState = SyncAskState.Recieve;
        }

        /// <summary>
        /// 게임 동기화 정보를 보낸다
        /// </summary>
        public void SendGameSync()
        {
            ////Debug.Log("============>> 동기화 게임정보 발신");
            SendGameSync info = new SendGameSync();
            info.Set(manager);

            byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
            SendData(data);
        }

        ////////////////////////////////////////////////////////////////////////////
        //게임 동기화요청 정보 송신 수신
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 게임 동기화 요청
        /// </summary>
        /// <param name="info12"></param>
        //private SyncType curSyncType; 
        private void recieveAskSync(SendAskSync info12)
        {
            ////Debug.Log("============>> 동기화 요청 수신");
            //curSyncType = info12.type;
            syncState = SyncAskState.Ask;
        }

        /// <summary>
        /// 동기화 요청 정보를 보낸다
        /// </summary>
        public void SendAskSync()
        {
            ////Debug.Log("============>> 동기화 요청 발신");
            SendAskSync info = new SendAskSync();
            info.Set();

            byte[] data = ObjectSerializationExtension.SerializeToByteArray(info);
            SendData(data);
        }



















        ////////////////////////////////////////////////////////////////////////////
        //타구 정보
        ////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 타구 정보를 넷상으로 부터 받아와 컨택전에 세팅
        /// </summary>
        public void SetBattingInfo()
        {
            manager.field.ballPower = BattingInfo.ballPower;
            manager.field.ball.angleZ = BattingInfo.ballAngleZ;
            manager.field.ball.angle = BattingInfo.ballAngle;
            manager.field.ball.angleHookSlice = BattingInfo.hookSlice;
            manager.field.ball.bHookorSlice = BattingInfo.bHookSlice;
            manager.field.ball.bTopSpin = BattingInfo.bTopSpin;


            manager.batter.bBunt = BattingInfo.bBunt;
            if (manager.batter.bBunt == true)
            {
                manager.batter.bBuntHit = true;
                manager.batter.buntType = (SimulBuntType)BattingInfo.buntType;
                manager.batter.buntResult = (SpecificBuntType)BattingInfo.buntResult;
                manager.batter.buntFielder = BattingInfo.buntFielder;
                if (manager.batterSkillFlag == SkillFlag.GodOfBunt)
                {
                    //번트 신 발생시 무조건 성공
                    manager.batter.buntResult = SpecificBuntType.DRAG_SUCCESS;
                }
            }

            bBattingResultUpdate = true;
        }

        /// <summary>
        /// 투구플레이시 상대 스윙여부
        /// </summary>
        /// <returns></returns>
        public bool IsSwing()
        {
            return bSwing;
        }

        /// <summary>
        /// 투구 플레이시 상대 컨택여부
        /// </summary>
        /// <returns></returns>
        public bool IsContact()
        {
            return bHit; //투수쪽 동기화 테스트시 비활성화(PVP테스트_반드시_복구)
        }

        /// <summary>
        /// 번트를 했는 지 여부
        /// </summary>
        /// <returns></returns>
        public bool IsBunt()
        {
            return bBunt;
        }

        /*
        /// <summary>
        /// 파울팁 여부
        /// </summary>
        /// <returns></returns>
        public bool IsTip()
        {
            return bTip;
        }*/

        /*/// <summary>
        /// 체크 스윙 여부
        /// </summary>
        /// <returns></returns>
        public bool IsCheckSwing()
        {
            return bCheckSwing;
        }*/

        /// <summary>
        /// 투구 플레이시 상대 타이밍
        /// </summary>
        /// <returns></returns>
        public BattingTiming GetTiming()
        {
            if (BattingInfo == null)
            {
                return BattingTiming.NOSWING;
            }
            else
            {
                return (BattingTiming)BattingInfo.Timing;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        //예외 처리
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 커넥트 이벤트시 예외 처리
        /// </summary>
        private void setDisconnect(bool bOtherDisconnect)
        {
            ////Debug.Log("============================>> 호스트 여부 " + manager.simulator.IsHost());
            if (bOtherDisconnect == true)
            {
                //상대가 끊긴 경우 내가 호스트가 됨
                manager.simulator.setHost(true);                
            }
            else
            {
                //내가 끊긴 경우 내가 게스트가 됨
                manager.simulator.setHost(false);
                if (Mode.bSimulationQuickPlay == true)
                {
                    //PVP모드에서 내가 끊긴 경우 재연결 팝업 
                    manager.simulator.setReconnectPopup();
                }
            }

            if (Mode.bSimulationQuickPlay == false)
            {
                ////Debug.Log("============================>> 액션 플레이중 끊긴경우");
                Mode.bPvpMode = false;
                manager.simulator.setLocalChanceFlag();
                if (manager.playState == PlayState.PLAY_BATTING_VIEW)
                {
                    if (manager.bMyTurn == true)
                    {
                        if (manager.pitcher.pState == PitcherState._GET_SIGN && manager.pitcher.bRelease == false)
                        {
                            bDisconnectAndAiThrow = true;
                        }
                    }
                    else
                    {
                        //
                    }                    
                }
                //강제로 시뮬로
                bForceBackToSimul = true;                
            }

        }


        /// <summary>
        /// 재접속
        /// </summary>
        public void reconnect()
        {
            //pvp.Connect();
        }

        /// <summary>
        /// 강제 디스커넥
        /// </summary>
        public void forceDisconnect()
        {
            //pvp.Close();
        }

        /// <summary>
        /// 다른쪽이 접속해있는지 여부
        /// </summary>
        /// <returns></returns>
        public bool IsOtherConnected()
        {
#if _Test_Local
            return true;
#else
            return pvp.IsOtherConnected;
#endif
        }










        /// <summary>
        /// 선수교체시 상대로부터 Pause상태 받아옴
        /// </summary>
        /// <param name="bPause"></param>
        private void setChangeWait(bool bPause)
        {
            Mode.bPauseGame = bPause;
            ControlBattingUI.CheckPauseState(bPause);
            IngameUI.GetPauseUI().SetChangeWait(bPause);
            if(bPause == true)
            {
                //포즈인 경우 피치 타이머 디액티브
                IngameUI.GetScoreBoard().SetPitchTimerActive(false);
            }
            else
            {
                if (manager.bMyTurn == false)
                {
                    //리쥼인 경우 내가 투수인 경우 피치 타이머 액티브
                    IngameUI.GetScoreBoard().SetPitchTimerActive(true);
                }
            }
        }

        /// <summary>
        /// 선수교체시 상대로부터 ChangeEvent 받아옴
        /// </summary>
        private void setChangeEvent()
        {
            bool bMyTeam = (bool)changeSync.bMyTeam;
            UIPlayerChange.PlayerChangeType type = (UIPlayerChange.PlayerChangeType)changeSync.type;
            int outIndex = changeSync.outIndex;
            int inIndex = changeSync.inIndex;
            int index = changeSync.index;

            int team = bMyTeam?0:1;

            CPlayer outCard, inCard;

            if (type == UIPlayerChange.PlayerChangeType.PitcherChange)
            {
                outCard = SimulPlayerManager.GetPitcher(team, outIndex);
                inCard = SimulPlayerManager.GetPitcher(team, inIndex);
            }
            else
            {
                outCard = SimulPlayerManager.GetFielder(team, outIndex);
                inCard = SimulPlayerManager.GetFielder(team, inIndex);
            }

            //선수교체 이벤트 호출
            IngameUI.GetChangeEventUI().InitPlayerChangeUI(bMyTeam, manager, outCard, inCard, type, index);

            setChangeWait(false);

        }

        /// <summary>
        /// PVP모드에서 교체 후 알맞은 형태로 세팅
        /// </summary>
        public void setChangeState()
        {
            ////Debug.Log("=================>> PVP모드에서 교체후 상태 세팅");
            nPitchNum = 2;  //임시
            bBattingResultUpdate = true;   //임시
        }



        //이모티콘 채팅
        private void otherEmoticon(string message)
        {
            if (Mode.bSimulationQuickPlay == true)
            {
                //시뮬모드
                manager.simulator.chattingUI.otherEmoticon(message);
            }
            else
            {
                //인게임 모드
                IngameUI.GetEmoticonChatting().otherEmoticon(message);
            }

        }


    }
}
