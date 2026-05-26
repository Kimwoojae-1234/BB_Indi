//#define _TEST_NOT_USE_DB_DATA //db 템플릿을 이용하지 않는 경우
//#define MAKE_LOG

using UnityEngine;
using System.Collections;
using WebConnector;
using System.Collections.Generic;
using System.IO;

namespace BaseBall.BallPlay
{
    public class SimulPlayerManager : MonoBehaviour
    {
        //인스턴스
        private static SimulPlayerManager Instance_;
        private SimulPlayer player;

        //팀의 인덱스와 팀명을 저장
        public static string strAwayTeam, strHomeTeam;
        public static string strMyTeam, strCPUTeam;
        public static int awayTeamIndex, homeTeamIndex;
        public static int myTeamIndex, cpuTeamIndex;
        public static int myTeamSeqNum, cpuTeamSeqNum;

#if GIRL_PLAY
        public static string[] _teamName = new string[10]
        {
            "Beach Breakers",
            "Sandstorm Sluggers",
            "Wave Runners",
            "Sunset Batters",
            "Coral Pirates",
            "Tidal Smash",
            "Blue Lagoon Nine",
            "Palm Hitters",
            "Seaside Storm",
            "Orca Beach Club"
        };
#else
        public static string[] _teamName = new string[10]
        {
            "삼성라이온즈","넥센히어로즈","NC다이노스","LG트윈스","SK와이번즈","두산베어즈","롯데자이언츠","기아타이거즈","한화이글스","kt위즈"
        };
#endif
        //
        void Awake()
        {
            player = new SimulPlayer();
            Instance_ = this;
        }

        //인스턴스 파괴자
        void OnDestroy()
        {
            player = null;
            Instance_ = null;
        }

#if _TEST_LOGIN
        private void testLogin(long userId)
        {
            ////UnityEngine.//Debug.Log("==============>>LOG IN");
            UserService.Login(userId, ((UserInfo ret) =>
            {
                //UnityEngine.Debug.Log("Login Success : " + ret);
                bLogin = true;
            }), ((ErrorResource er) =>
            {
                //UnityEngine.Debug.Log("Login Fail : " + er.code);
            }));
        }
#endif

        ////////////////////////////////////////////////////////////////////
        //외부 호출용 static 메쏘드
        ////////////////////////////////////////////////////////////////////
        public static SimulPlayer GetPlayer()
        {
            return Instance_.player;
        }
        
        //선수 데이터를 초기화함
        public static void SetInit()
        {
            Instance_.player.init();
        }

        // 유저의 홈 어웨이 여부를 리턴
        public static bool IsMyHome()
        {
            return Instance_.player.isMyHome();
        }

        public static string GetOffenseTeam(bool bTopInning)
        {
            return bTopInning ? strAwayTeam : strHomeTeam;
        }

        public static string GetDefenseTeam(bool bTopInning)
        {
            return bTopInning ? strHomeTeam : strAwayTeam;
        }

        public static int GetOffenseIndex(bool bTopInning)
        {
            return bTopInning ? awayTeamIndex : homeTeamIndex;
        }

        public static int GetDefenseIndex(bool bTopInning)
        {
            return bTopInning ? homeTeamIndex : awayTeamIndex;
        }


#if _Test_Local
        //MakePlayer의 로컬 버전 (실게임에서는 사용하지 않고 로컬 테스트용으로만 사용)
        public static void MakePlayerLocal(int step)
        {
            Instance_.player.MakePlayerLocal(step);
        }
#else
        //서버로 받아온 게임 시즌 정보를 통해 선수 데이터를 구축한다.
        public static void MakePlayer(bool bMyHome, GameLineup lineup, int myStaterIndex, int otherStarterIndex)
        {
            Instance_.player.MakePlayer(bMyHome, lineup, myStaterIndex, otherStarterIndex);
        }

        //9회2아웃 전용
        public static void MakePlayerWalkOff(WalkoffPlayGameInfo info)
        {
            Instance_.player.MakePlayerWalkOff(info);
        }


#endif

        //현재 배팅하고 있는 타자의 정보를 얻어옴
        public static CPlayer GetBatter(int team)
        {
            return Instance_.player.GetBatter(team);
        }

        public static void SavePlayerData()
        {
            Instance_.player.SavePlayerData();
        }

        //다음 타자 정보를 얻어옴
        public static CPlayer GetNextBatter(int team, int next = 1)
        {
            return Instance_.player.GetNextBatter(team, next);
        }

        //해당 팀과 인덱스의 야수 데이터를 얻어옴
        public static CPlayer GetFielder(int team, int index, bool bSaved = false)
        {
            return Instance_.player.GetFielder(team, index, bSaved);
            
        }

        // 해당 인덱스의 야수를 교체
        // inPlayer: 교체들어오는 선수, outPlayer: 교체 아웃되는 선수
        public static void SetFielderChange(int team, int inPlayer, int outPlayer, int changeType)
        {
            Instance_.player.SetFielderChange(team, inPlayer, outPlayer, changeType);
        }

        //해당 팀과 인덱스의 타자(야수)를 세팅한다(SeasonGameInfo 데이터로부터 야수의 정보를 초기화 할때 호출)
        public static void SetBatter(CPlayer player, int team, int index)
        {
            Instance_.player.SetBatter(player, team, index);
        }

        // 해당 팀과 인덱스의 타자(야수)가 이미 출전했는지 여부를 세팅한다.(교체시 필요)
        public static void SetFielderOut(int team, int index, bool bOut = true)
        {
            Instance_.player.SetFielderOut(team, index, bOut);
        }

        // 해당 팀과 인덱스의 타자(야수)가 이미 출전했는지 여부값을 가져온다
        public static bool GetFielderOut(int team, int index, bool bSaved = false)
        {
            return Instance_.player.GetFielderOut(team, index, bSaved);
            
        }

        // 해당 팀의 현재 타순값을 가져온다.
        public static int GetLineupCount(int team)
        {
            return Instance_.player.GetLineupCount(team);
        }

        // 해당 팀의 현재 타순을 세팅한다.
        public static void SetLineupCount(int team)
        {
            Instance_.player.SetLineupCount(team);
        }

        // 현재 타순을 강제로 세팅
        public static void SetLineup(int team, int count)
        {
            Instance_.player.SetLineUp(team, count);
        }

        // 해당 팀의 타순 사이클을 가져온다
        public static int GetCycle(int team)
        {
            return Instance_.player.GetCycle(team);
        }

        // 해당 팀의 타순별 현재 수비 포지션 값을 가져온다
        public static int GetCurPosition(int team, int index)
        {
            return Instance_.player.GetCurPosition(team, index);
        }

        public static int GetFielderIndexFromCard(int team, long seq)
        {
            return Instance_.player.GetFielderIndexFromCard(team, seq);
        }

        // 해당 팀의 타순별 현재 수비 포지션 값을 세팅한다. (초기화 메쏘드)
        public static void SetCurPosition(int team, int index, int pos)
        {
            Instance_.player.SetCurPosition(team, index, pos);
        }

        // 현재 등판 중인 투수의 데이터를 가져온다.
        public static CPlayer GetPitcher(int team)
        {
            return Instance_.player.GetPitcher(team);
        }

        // 해당 인덱스의 투수 데이터를 가져온다
        public static CPlayer GetPitcher(int team, int index, bool bSaved = false)
        {
            return Instance_.player.GetPitcher(team, index, bSaved);
        }

        public static int GetPitcherIndexFromCard(int team, long seq)
        {
            return Instance_.player.GetPitcherIndexFromCard(team, seq);
        }

        // 해당 팀과 인덱스의 투수를 세팅한다(SeasonGameInfo 데이터로부터 투수의 정보를 초기화 할때 호출)
        public static void SetPitcher(CPlayer player, int team, int index)
        {
            Instance_.player.setPitcher(player, team, index);
        }

        // 해당 팀의 현재 출전중인 투수의 인덱스를 설정한다. (초기화 혹은 교체시 호출)
        public static void SetCurrentPitcherIndex(int team, int index, bool bStarer = false)
        {
            Instance_.player.SetCurrentPitcherIndex(team, index, bStarer);
        }

        // 해당 팀의 투수가 이미 출전했는지 여부를 세팅한다.
        public static void SetPitcherOut(int team, int index, bool bOut = true)
        {
            Instance_.player.SetPitcherOut(team, index, bOut);
        }

        // 해당 팀의 투수가 이미 출전했는지 여부값을 가져온다.
        public static bool GetPitcherOut(int team, int index)
        {
            return Instance_.player.GetPitcherOut(team, index);
        }

        // 선발투수의 인덱스를 리턴해준다
        public static int GetStarterIndex(int team)
        {
            return Instance_.player.GetStarterIndex(team);
        }

        // 현재 투수의 인덱스를 리턴해준다
        public static int GetPitcherIndex(int team)
        {
            return Instance_.player.GetPitcherIndex(team);
        }

        // 지금 현재 투수가 선발투수 여부인지를 알려줌
        public static bool IsStartPitcher(int team)
        {
            return Instance_.player.IsStartPitcher(team);
        }

        //팀네임 리턴
        public static string GetTeamName(bool bHome)
        {
            return _teamName[(bHome?homeTeamIndex:awayTeamIndex) - 1];
        }

        /// <summary>
        /// 시즌 결과 리스트를 초기화
        /// </summary>
        public static void InitSeasonResult()
        {
            Instance_.initSeasonResult();
        }

        /// <summary>
        /// 다음팀의 결과를 시즌 결과 리스트에 add
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="bMyHome"></param>
        public static void AddSeasonResult(int seq, bool bMyHome)
        {
            Instance_.addSeasonResult(seq, bMyHome);
        }


        public static string GetGameSummary()
        {
            return Instance_.getGameSummary();
        }



        /// <summary>
        /// 서버에 보내기 위한 시즌 결과 리스트값을 얻어옴
        /// </summary>
        /// <returns></returns>
        public static List<SeasonGameResult> GetGameResults()
        {
            return Instance_.getGameResults();
        }



        public static void AddPitcherResult()
        {
            Instance_.addPitcherResult();
        }

        public static List<int> GetPitcherChangeList(int team)
        {
            return Instance_.player.getPitcherChangeList(team);
        }

        public static List<CPlayer> GetBatterChangeList(int team)
        {
            return Instance_.player.getBatterChangeList(team);
        }


        public static List<SeasonPitcherResult> GetPitcherResult()
        {
            return Instance_.getPitcherResult();
        }

        /*
        /// <summary>
        /// 서버에 보내기 위한 랭킹전 결과 얻어옴
        /// </summary>
        /// <returns></returns>
        public static RankedPlayGameResult GetGameRankedPlayResults()
        {
            return Instance_.getGameRankedPlayResults();
        }*/


        public static LivePlayResult GetLivePlayResults(bool bMyHome)
        {
            return Instance_.getLivePlayResults(bMyHome);
        }


        public static RacePlayResult GetGameRaceResults(bool bMyHome)
        {
            return Instance_.getGameRaceResults(bMyHome);
        }

        //////////////////////////////////////////////////////////////////
        //결과리스트
        //////////////////////////////////////////////////////////////////
        private List<SeasonGameResult> gameResults;
        private List<SeasonPitcherResult> pitcherResult; //투수승패기록 저장
        
        //결과리스트 초기화
        private void initSeasonResult()
        {
            //Debug.Log("==========================>>>결과 리스트 초기화");
            gameResults = null;
            gameResults = new List<SeasonGameResult>();
            gameResults.Clear();

            pitcherResult = null;
            pitcherResult = new List<SeasonPitcherResult>();
            pitcherResult.Clear();
        }

        //
        private void addSeasonResult(int seq, bool bMyHome)
        {
            ////Debug.Log("==========================>>>결과 리스트 add seq: "+seq);
            SeasonGameResult record = new SeasonGameResult();

            int home = bMyHome ? 0 : 1;
            int away = bMyHome ? 1 : 0;

            record.scheNo = seq;
            record.homeScore = SimulManager.GetGameInfo().run[home];
            record.awayScore = SimulManager.GetGameInfo().run[away];            
            record.homePitchers = player.getPitcherResult(home);
            record.homeHitters = player.getHitterResult(home);
            record.awayPitchers = player.getPitcherResult(away);
            record.awayHitters = player.getHitterResult(away);

            gameResults.Add(record);
            //Debug.Log("awayScore = " + record.awayScore + "   vs homeScore = " + record.homeScore);
        }

        private string getGameSummary()
        {
#if _Test_Local
            return null;
#else
            GameDetailRecord detailRecord = new GameDetailRecord();

            ///////////////////////////////////////////////////////////
            //팀기록
            ///////////////////////////////////////////////////////////
            detailRecord.myScore = new int[12];
            detailRecord.cpuScore = new int[12];
            detailRecord.myRecord = new int[6];
            detailRecord.cpuRecord = new int[6];

            SimulGameInfo info = SimulManager.GetGameInfo();
            for (int i = 0; i < 12; i++)
            {
                detailRecord.myScore[i] = info.inningScore[0, i];
                detailRecord.cpuScore[i] = info.inningScore[1, i];
            }
            //득점,안타,실책,홈런,도루,삼진
            detailRecord.myRecord[0] = info.run[0];
            detailRecord.myRecord[1] = info.hit[0];
            detailRecord.myRecord[2] = info.error[0];
            detailRecord.myRecord[3] = info.homerun[0];
            detailRecord.myRecord[4] = info.steal[0];
            detailRecord.myRecord[5] = info.strikeout[0];
            //
            detailRecord.cpuRecord[0] = info.run[1];
            detailRecord.cpuRecord[1] = info.hit[1];
            detailRecord.cpuRecord[2] = info.error[1];
            detailRecord.cpuRecord[3] = info.homerun[1];
            detailRecord.cpuRecord[4] = info.steal[1];
            detailRecord.cpuRecord[5] = info.strikeout[1];

            //승리투수/패전투수
            detailRecord.winPitcher = SimulManager.getWinPitcher().getName();
            detailRecord.losePitcher = SimulManager.getLosePitcher().getName();

            ///////////////////////////////////////////////////////////
            //타자기록
            ///////////////////////////////////////////////////////////
            detailRecord.myBatterRecord = new Dictionary<int, int[]>();
            detailRecord.cpuBatterRecord = new Dictionary<int, int[]>();
            for (int team = 0; team < 2; team++)
            {
                List<CPlayer> batterList = SimulPlayerManager.GetBatterChangeList(team);
                int max = batterList.Count;
                int count = 0;
                CPlayer lastPlayer = null;
                for (int i = 0; i < max; i++)
                {
                    CPlayer player = batterList[i];
                    if (player != lastPlayer)
                    {
                        if (count < SimulPlayer.NUM_FIELDER)
                        {
                            if (team == 0)
                            {
                                detailRecord.myBatterRecord.Add(player.getCard().cardId, player.getDetailRecord());
                            }
                            else
                            {
                                detailRecord.cpuBatterRecord.Add(player.getCard().cardId, player.getDetailRecord());
                            }
                        }
                        lastPlayer = player;
                        count++;
                    }
                }
            }


            ///////////////////////////////////////////////////////////
            //투수기록
            ///////////////////////////////////////////////////////////
            detailRecord.myPitcherRecord = new Dictionary<int, int[]>();
            detailRecord.cpuPitcherRecord = new Dictionary<int, int[]>();

            RecordInfo[] rInfo = new RecordInfo[2];
            bool bHome = IsMyHome();
            if (Mode.gameMode == Mode.GamePlayMode.Season)
            {
                // DISABLED_MGRS: SeasonGameInfo sInfo = Mgrs.userData.Ingame_seasonGameInfo;
                rInfo[0] = bHome ? sInfo.homeRecInfo : sInfo.awayRecInfo;
                rInfo[1] = bHome ? sInfo.awayRecInfo : sInfo.homeRecInfo;
            }


            for (int team = 0; team < 2; team++)
            {
                List<int> pitcherList = SimulPlayerManager.GetPitcherChangeList(team);
                int max = pitcherList.Count;
                int count = 0;
                CPlayer lastPlayer = null;
                for (int i = 0; i < max; i++)
                {
                    int pIndex = pitcherList[i];
                    CPlayer player = SimulPlayerManager.GetPitcher(team, pIndex);
                    if (player != lastPlayer)
                    {
                        //결과
                        GameRecordPitcher record = null;
                        int game, win, lose, hold, save, bs;
                        if (rInfo[team] != null)
                        {
                            record = rInfo[team].GetGameRecordPitcher(player.getCard().cardSeq, player.getCard().cardId);
                        }

                        if (record != null)
                        {
                            game = record.g + 1;
                            win = record.pW;
                            lose = record.pL;
                            hold = record.pHLD;
                            save = record.pSV;
                            bs = record.pBS;
                        }
                        else
                        {
                            game = 1;
                            win = lose = hold = save = bs = 0;
                        }
                        if (player.getStat(Param.ST_PW) > 0)
                        {
                            player.setDetailRecord2(0, 1); //승
                            win++;
                        }
                        else if (player.getStat(Param.ST_PL) > 0)
                        {
                            player.setDetailRecord2(0, 2); //패
                            lose++;
                        }
                        else if (player.getStat(Param.ST_HLD) > 0)
                        {
                            player.setDetailRecord2(0, 3); //홀드
                            hold++;
                        }
                        else if (player.getStat(Param.ST_SV) > 0)
                        {
                            player.setDetailRecord2(0, 4); //세이브
                            save++;
                        }
                        else if (player.getStat(Param.ST_BS) > 0)
                        {
                            player.setDetailRecord2(0, 5); //블론
                            bs++;
                        }
                        else player.setDetailRecord2(0, 0);
                        
                        player.setDetailRecord2(1, player.getStat(Param.ST_IP));    //이닝
                        player.setDetailRecord2(2, player.getStat(Param.ST_IP) + player.getStat(Param.ST_PH) + player.getStat(Param.ST_PBB));    //타자
                        player.setDetailRecord2(3, player.getStat(Param.ST_PNP));    //투구
                        player.setDetailRecord2(4, player.getStat(Param.ST_PSO));    //삼진
                        player.setDetailRecord2(5, player.getStat(Param.ST_PBB));    //포볼
                        player.setDetailRecord2(6, player.getStat(Param.ST_PH));    //안타
                        player.setDetailRecord2(7, player.getStat(Param.ST_PHR));    //홈런
                        player.setDetailRecord2(8, player.getStat(Param.ST_PR));    //실점
                        player.setDetailRecord2(9, player.getStat(Param.ST_PER));    //자책
                        player.setDetailRecord2(10, game * 100 + win);  //게임수, 승
                        player.setDetailRecord2(11, lose * 100 + hold); //패, 홀드
                        player.setDetailRecord2(12, save * 100 + bs);   //세이브, 블론
                        

                        if (team == 0)
                        {
                            detailRecord.myPitcherRecord.Add(player.getCard().cardId, player.getDetailRecord());
                        }
                        else
                        {
                            detailRecord.cpuPitcherRecord.Add(player.getCard().cardId, player.getDetailRecord());
                        }
                        lastPlayer = player;
                        count++;
                    }
                }

            }


            string summary = Utils.JsonUtils.Serialize<GameDetailRecord>(detailRecord);

            return summary;
#endif
        }


        private List<SeasonGameResult> getGameResults()
        {
            return gameResults;
        }



        private void addPitcherResult()
        {            
#if _Test_Local
#else
            SeasonPitcherResult pitcher = new SeasonPitcherResult();

            CPlayer winPitcher = SimulManager.getWinPitcher();
            CPlayer losePitcher = SimulManager.getLosePitcher();

            if (winPitcher != null)
            {
                pitcher.winPitcher = winPitcher.getName();
                pitcher.winPitcherOverall = Utils.TeamPowerUtils.calCardPower(winPitcher.getCard());
            }

            if (losePitcher != null)
            {
                pitcher.losePitcher = losePitcher.getName();
                pitcher.losePitcherOverall = Utils.TeamPowerUtils.calCardPower(losePitcher.getCard());
            }
            ////Debug.Log("===============>>addPitcher Result");
            pitcherResult.Add(pitcher);
#endif
        }


        private List<SeasonPitcherResult> getPitcherResult()
        {
            return pitcherResult;
        }

        /*
        private RankedPlayGameResult getGameRankedPlayResults()
        {
            RankedPlayGameResult result = new RankedPlayGameResult();

            result.homeScore = SimulManager.GetGameInfo().run[0];
            result.awayScore = SimulManager.GetGameInfo().run[1];
            result.homePitchers = player.getPitcherResult(0);
            result.homeHitters = player.getHitterResult(0);
            result.awayPitchers = player.getPitcherResult(1);
            result.awayHitters = player.getHitterResult(1);

            return result;
        }*/

        private LivePlayResult getLivePlayResults(bool bMyHome)
        {
            LivePlayResult result = new LivePlayResult();

            int awayIndex = (bMyHome ? 1 : 0);
            int homeIndex = 1 - awayIndex;

            result.homeScore = SimulManager.GetGameInfo().run[homeIndex];
            result.awayScore = SimulManager.GetGameInfo().run[awayIndex];
            result.homePitchers = player.getPitcherResult(homeIndex);
            result.homeHitters = player.getHitterResult(homeIndex);
            result.awayPitchers = player.getPitcherResult(awayIndex);
            result.awayHitters = player.getHitterResult(awayIndex);
          
            return result;
        }


        private RacePlayResult getGameRaceResults(bool bMyHome)
        {
            RacePlayResult result = new RacePlayResult();
            int awayIndex = (bMyHome ? 1 : 0);
            int homeIndex = 1 - awayIndex;

            result.homeScore = SimulManager.GetGameInfo().run[homeIndex];
            result.awayScore = SimulManager.GetGameInfo().run[awayIndex];
            result.homePitchers = player.getPitcherResult(homeIndex);
            result.homeHitters = player.getHitterResult(homeIndex);
            result.awayPitchers = player.getPitcherResult(awayIndex);
            result.awayHitters = player.getHitterResult(awayIndex);

            return result;
        }
       

        //타팀 시뮬레이션용
        //private int otherStep;
        //private List<long> list;
        //private Dictionary<long, SeasonGameInfo> todayGameInfo;
        //private Dictionary<long, GameRecordDto> seasonGameResults = new Dictionary<long, GameRecordDto>();

/*        //결과를 서버에 전송하고 다른 팀의 정보를 얻어와 클라에서 시뮬한다.
        private Dictionary<long, SeasonGameInfo> sendGameResult(bool bOtherTeam = false)
        {
            Dictionary<long, SeasonGameInfo> data = null;

            List<GameRecordPitcher> myPitcher = player.getPitcherResult(0);
            List<GameRecordPitcher> otherPitcher = player.getPitcherResult(1);
            List<GameRecordHitter> myHitter = player.getHitterResult(0);
            List<GameRecordHitter> otherHitter = player.getHitterResult(1);

            if (bOtherTeam == false)
            {
                int myScore = SimulManager.GetGameInfo().run[0];
                int otherScore = SimulManager.GetGameInfo().run[1];
                GameSeasonService.Result(false,myScore, otherScore, myPitcher, myHitter, otherPitcher, otherHitter,
                    (Dictionary<long, SeasonGameInfo> info) =>
                    {
                        //UnityEngine.Debug.Log("sendTestResult Success");
                        if (InGameDebug._Client_Simul == true)
                        {
                            todayGameInfo = info;
                            list = new List<long>(info.Keys);
                            otherStep = 0;
                            simulateOtherTeam();
                            data = info;
                        }
                        else
                        {
                            //결과전송 끝
                            //////UnityEngine.//Debug.Log("======================>> 결과 전송 성공");
                            //BallPlayManager.Instance_.LoadGameResult();
                        }

                    }, ((ErrorResource er) =>
                    {
                        //UnityEngine.Debug.Log("sendTestResult Fail : " + er.code);
                    }));
            }
            else
            {
                ////UnityEngine.//Debug.Log("======================>>otherStep " + otherStep + " 결과 세팅");
                GameRecordDto record = new GameRecordDto();
                record.myHitters = myHitter;
                record.myPitchers = myPitcher;
                record.otherHitters = otherHitter;
                record.otherPitchers = otherPitcher;

                seasonGameResults.Add(list[otherStep], record);

            }

            return data;
        }*/

        /*//다른팀의 경기를 클라에서 시뮬하는 함수
        private void simulateOtherTeam()
        {
            for (int i = 0; i < 4; i++)
            {
                SeasonGameInfo otherGameInfo = todayGameInfo[list[otherStep]];
                SimulManager.SimulateOneGame(false,otherGameInfo.lineup,false);
                sendGameResult(true);
                otherStep++;
            }

            GameSeasonService.ResultOthers(seasonGameResults,
                     ((string success) =>
                     {
                         //////UnityEngine.//Debug.Log("======================>>otherStep " + otherStep + " 결과 전송 성공");
                         //BallPlayManager.Instance_.LoadGameResult();

                     }), ((ErrorResource er) =>
                     {
                         //UnityEngine.Debug.Log("gameStart Fail : " + er.code);
                     }));
        }*/


#if MAKE_LOG
        ////////////////////////////////////////////////////////////////////
        //로그 만들기
        ////////////////////////////////////////////////////////////////////

        public static void MakeLog()
        {
            Instance_.makeLog();
        }
                
        //결과 서머리
        private string _2bSummary, _3bSummary, _HRSummary, _RbiSummary, _SBSummary, _SBCSummary, _ErrorSummary;
        private string _WinPitcher, _LosePitcer, _HoldPitcher, _BlownPitcher, _SavePitcher;
        private int numLiner, numGrounder, numFly;
        private int numLinerHit, numGrounderHit, numFlyHit;


        //로그 만들기(테스트 전용)
        public void makeLog()
        {
            ////UnityEngine.//Debug.Log("======================>>로그만들기 SimulResult");

            System.DateTime now = System.DateTime.Now;
            string date = "_" + now.Month + "_" + now.Day + "_" + now.Hour + "_" + now.Minute + "_" + now.Second;
            string rootDir = Application.persistentDataPath + "/root/gamelog/log" + date + "_TestAGame";
            string filePath = rootDir + "_" + SimulPlayerManager.strAwayTeam + "_vs_" + SimulPlayerManager.strHomeTeam + ".txt";
            //////UnityEngine.//Debug.Log("======================>>filePath = " + filePath);
            StreamWriter sw = null;
            if (File.Exists(filePath))
            {
                ////UnityEngine.//Debug.Log("======================>>파일이 이미 있음");
            }
            // Create a file to write to.
            sw = File.CreateText(filePath);

            TeamStat stat = player.getTeamStat();

            sw.WriteLine("===========================================================================================================================================");
            sw.WriteLine("\t\t1\t2\t3\t4\t5\t6\t7\t8\t9\t10\t11\t12\tR\tH\tE\tB");
            string scoreborad = "";
            for (int i = 0; i < 12; i++) scoreborad += getInningScore(stat, 0, i);
            sw.WriteLine(SimulPlayerManager.strAwayTeam + "\t" +
                     scoreborad +
                     stat.score[0] + "\t" + stat.hitCount[0] + "\t" + stat.errorCount[0] + "\t" + stat.bbCount[0] + "\t");
            scoreborad = "";
            for (int i = 0; i < 12; i++) scoreborad += getInningScore(stat, 1, i);
            sw.WriteLine(SimulPlayerManager.strHomeTeam + "\t" +
                         scoreborad +
                         stat.score[1] + "\t" + stat.hitCount[1] + "\t" + stat.errorCount[1] + "\t" + stat.bbCount[1] + "\t");


            sw.WriteLine("===========================================================================================================================================");
            sw.WriteLine("");
            sw.WriteLine("");
            sw.WriteLine("======================================================================");
            sw.WriteLine(SimulPlayerManager.strAwayTeam + "팀 타자 기록");
            sw.WriteLine("======================================================================");
            sw.WriteLine("BATTING\t\tAB\tR\tH\tRBI\tBB\tSO\tAVG");
            getHitterLog(0, sw);
            sw.WriteLine("======================================================================");
            sw.WriteLine(SimulPlayerManager.strAwayTeam + "팀 투수 기록");
            sw.WriteLine("======================================================================");
            sw.WriteLine("PITCHING\tIP\tH\tR\tER\tBB\tSO\tERA");
            getPitcherLog(0, sw);
            sw.WriteLine("");
            sw.WriteLine("");
            sw.WriteLine("======================================================================");
            sw.WriteLine(SimulPlayerManager.strHomeTeam + "팀 타자 기록");
            sw.WriteLine("======================================================================");
            sw.WriteLine("BATTING\t\tAB\tR\tH\tRBI\tBB\tSO\tAVG");
            getHitterLog(1, sw);            
            sw.WriteLine("======================================================================");
            sw.WriteLine(SimulPlayerManager.strHomeTeam + "팀 투수 기록");
            sw.WriteLine("======================================================================");
            sw.WriteLine("PITCHING\tIP\tH\tR\tER\tBB\tSO\tERA");
            getPitcherLog(1, sw);
            if (sw != null)
            {
                sw.Close();
            }
        }

        //타자의 로그 만들기
        private void getHitterLog(int team, StreamWriter sw = null)
        {

            _2bSummary = _3bSummary = _HRSummary = _RbiSummary = _SBSummary = _SBCSummary = _ErrorSummary = "";
            numLiner = numGrounder = numFly = 0;
            numLinerHit = numGrounderHit = numFlyHit = 0;

            for (int j = 0; j < SimulPlayer.NUM_FIELDER; j++)
            {
                ////////UnityEngine.Debug.Log("====> i = " + i + " ====>>j = " + j);
                CPlayer player = GetFielder(team, j);
                if (GetFielderOut(team, j) == true)// if (player.getStat(Param.ST_PA) > 0) //타석수 0보다 큼
                {
                    if (sw != null)
                    {
                        string name = player.getName();
                        if (j < 9)
                        {
                            int curPos = GetCurPosition(team, j);
                            name += " (" + Param.position[curPos] + ")";
                        }
                        else
                            name += " (out)";
                        name = string.Format("{0,-11}", name);
                        int avg;
                        if (player.getStat(Param.ST_AB) > 0)
                        {
                            avg = (player.getStat(Param.ST_H) * 1000) / player.getStat(Param.ST_AB);
                        }
                        else
                        {
                            avg = 0;
                        }
                        string strAvg = (avg / 1000) + "." + (avg % 1000).ToString("000");

                        sw.WriteLine(name + "\t" +
                                     player.getStat(Param.ST_AB) + "\t" +
                                     player.getStat(Param.ST_R) + "\t" +
                                     player.getStat(Param.ST_H) + "\t" +
                                     player.getStat(Param.ST_RBI) + "\t" +
                                     player.getStat(Param.ST_BB) + "\t" +
                                     player.getStat(Param.ST_SO) + "\t" +
                                     strAvg);

                        if (player.getStat(Param.ST_2B) > 0)
                        {
                            _2bSummary += (player.getName() + "(" + player.getStat(Param.ST_2B) + ")\t");
                        }
                        if (player.getStat(Param.ST_3B) > 0)
                        {
                            _3bSummary += (player.getName() + "(" + player.getStat(Param.ST_3B) + ")\t");
                        }
                        if (player.getStat(Param.ST_HR) > 0)
                        {
                            _HRSummary += (player.getName() + "(" + player.getStat(Param.ST_HR) + ")\t");
                        }
                        if (player.getStat(Param.ST_RBI) > 0)
                        {
                            _RbiSummary += (player.getName() + "(" + player.getStat(Param.ST_RBI) + ")\t");
                        }
                        if (player.getStat(Param.ST_SBS) > 0)
                        {
                            _SBSummary += (player.getName() + "(" + player.getStat(Param.ST_SBS) + ")\t");
                        }
                        if (player.getStat(Param.ST_SBF) > 0)
                        {
                            _SBCSummary += (player.getName() + "(" + player.getStat(Param.ST_SBF) + ")\t");
                        }
                        if (player.getStat(Param.ST_E) > 0)
                        {
                            _ErrorSummary += (player.getName() + "(" + player.getStat(Param.ST_E) + ")\t");
                        }

                        if (player.getHitType(Param.ST_GROUNDER) > 0)
                        {
                            numGrounder += player.getHitType(Param.ST_GROUNDER);
                        }

                        if (player.getHitType(Param.ST_FLY) > 0)
                        {
                            numFly += player.getHitType(Param.ST_FLY);
                        }

                        if (player.getHitType(Param.ST_LINER) > 0)
                        {
                            numLiner += player.getHitType(Param.ST_LINER);
                        }

                        if (player.getHitType(Param.ST_GROUNDERHIT) > 0)
                        {
                            numGrounderHit += player.getHitType(Param.ST_GROUNDERHIT);
                        }

                        if (player.getHitType(Param.ST_FLYHIT) > 0)
                        {
                            numFlyHit += player.getHitType(Param.ST_FLYHIT);
                        }

                        if (player.getHitType(Param.ST_LINERHIT) > 0)
                        {
                            numLinerHit += player.getHitType(Param.ST_LINERHIT);
                        }
                    }
                }
            }
            getBattingSummary(sw);
        }

        //타격 서머리 만들기
        private void getBattingSummary(StreamWriter sw)
        {
            sw.WriteLine("");
            sw.WriteLine("Summary");
            if (_2bSummary != "") sw.WriteLine("2B: " + _2bSummary);
            if (_3bSummary != "") sw.WriteLine("3B: " + _3bSummary);
            if (_HRSummary != "") sw.WriteLine("HR: " + _HRSummary);
            if (_RbiSummary != "") sw.WriteLine("RBI: " + _RbiSummary);
            if (_SBSummary != "") sw.WriteLine("도루: " + _SBSummary);
            if (_SBCSummary != "") sw.WriteLine("도루실패: " + _SBCSummary);
            if (_ErrorSummary != "") sw.WriteLine("실책: " + _ErrorSummary);
            sw.WriteLine("");
            sw.WriteLine("타구타입");
            ////UnityEngine.Debug.Log("정식 ===========>>땅볼아웃: " + (numGrounder - numGrounderHit) + "  직선아웃: " + (numLiner - numLinerHit) + "  뜬볼아웃: " + (numFly - numFlyHit));
            int totalOut = (numLiner - numLinerHit) + (numGrounder - numGrounderHit) + (numFly - numFlyHit);
            if (totalOut > 0)
            {
                sw.WriteLine("땅볼 아웃: " + (((numGrounder - numGrounderHit) * 100) / totalOut) + "% \t직선타 아웃: " + (((numLiner - numLinerHit) * 100) / totalOut) + "% \t뜬공 아웃: " + (((numFly - numFlyHit) * 100) / totalOut) + "%");
            }
            ////UnityEngine.Debug.Log("정식 ===========>>땅볼안타: " + (numGrounderHit) + "  직선안타: " + (numLinerHit) + "  뜬볼안타: " + (numFlyHit));
            int totalHit = numLinerHit + numGrounderHit + numFlyHit;
            if (totalHit > 0)
            {
                sw.WriteLine("땅볼 안타: " + ((numGrounderHit * 100) / totalHit) + "% \t직선타 안타: " + ((numLinerHit * 100) / totalHit) + "% \t뜬공 안타: " + ((numFlyHit * 100) / totalHit) + "%");
            }
            sw.WriteLine("");
        }


        //투수의 로그 만들기
        private void getPitcherLog(int team, StreamWriter sw)
        {
            _WinPitcher = _LosePitcer = _HoldPitcher = _BlownPitcher = _SavePitcher = "";
            for (int j = 0; j < SimulPlayer.NUM_PITCHER; j++)
            {
                CPlayer player = GetPitcher(team, j);
                if (player.getStat(Param.ST_PNP) > 0) //투구수 0보다 큰경우
                {
                    if (sw != null)
                    {
                        string name = player.getName();
                        if (player.getStat(Param.ST_PW) == Param.P_ACHIEVE_COMPLETE)
                        {
                            string cg = "";
                            if (player.getStat(Param.ST_CG) == Param.P_ACHIEVE_COMPLETE)
                            {
                                cg = " (완투";
                                if (player.getStat(Param.ST_SHO) == Param.P_ACHIEVE_COMPLETE)
                                {
                                    cg += ", 완봉)";
                                }
                                else
                                {
                                    cg += ")";
                                }
                            }
                            _WinPitcher += name + cg + "\t";
                            name += " (승)";
                        }
                        else if (player.getStat(Param.ST_PL) == Param.P_ACHIEVE_COMPLETE)
                        {
                            string cg = "";
                            if (player.getStat(Param.ST_CG) == Param.P_ACHIEVE_COMPLETE)
                            {
                                cg = " (완투)";
                            }
                            _LosePitcer += name + cg + "\t";
                            name += " (패)";
                        }
                        else if (player.getStat(Param.ST_HLD) == Param.P_ACHIEVE_COMPLETE)
                        {
                            _HoldPitcher += name + "\t";
                            name += " (홀)";
                        }
                        else if (player.getStat(Param.ST_SV) == Param.P_ACHIEVE_COMPLETE)
                        {
                            _SavePitcher += name + "\t";
                            name += " (세)";
                        }
                        else if (player.getStat(Param.ST_BS) == Param.P_ACHIEVE_COMPLETE)
                        {
                            _BlownPitcher += name + "\t";
                            name += " (블)";
                        }
                        name = string.Format("{0,-11}", name);

                        string ip = player.getStat(Param.ST_IP) / 3 + "." + player.getStat(Param.ST_IP) % 3;
                        int err;
                        if (player.getStat(Param.ST_IP) > 0)
                        {
                            err = (player.getStat(Param.ST_PER) * 27 * 100) / player.getStat(Param.ST_IP);
                        }
                        else
                        {
                            err = (player.getStat(Param.ST_PER) > 0 ? 9999 : 0);
                        }
                        string strErr = (err / 100) + "." + (err % 100).ToString("00");

                        sw.WriteLine(name + "\t" +
                                     ip + "\t" +
                                     player.getStat(Param.ST_PH) + "\t" +
                                     player.getStat(Param.ST_PR) + "\t" +
                                     player.getStat(Param.ST_PER) + "\t" +
                                     player.getStat(Param.ST_PBB) + "\t" +
                                     player.getStat(Param.ST_PSO) + "\t" +
                                     strErr + "\t"
                            //(player.getStat(Param.ST_CG) == Param.P_ACHIEVE_COMPLETE ? "완투\t" : "") +
                            //(player.getStat(Param.ST_SHO) == Param.P_ACHIEVE_COMPLETE ? "완봉\t" : "")
                                     );
                    }
                }
            }

            getPitchingSummary(sw);
        }

        //투구 서머리
        private void getPitchingSummary(StreamWriter sw)
        {
            //_WinPitcher = _LosePitcer = _HoldPitcher = _BlownPitcher = _SavePitcher = "";
            sw.WriteLine("");
            sw.WriteLine("Summary");
            if (_WinPitcher != "") sw.WriteLine("승리투수: " + _WinPitcher);
            if (_LosePitcer != "") sw.WriteLine("패전투수: " + _LosePitcer);
            if (_HoldPitcher != "") sw.WriteLine("홀드: " + _HoldPitcher);
            if (_BlownPitcher != "") sw.WriteLine("블론: " + _BlownPitcher);
            if (_SavePitcher != "") sw.WriteLine("세이브: " + _SavePitcher);
            sw.WriteLine("");
        }

        private string getInningScore(TeamStat stat, int team, int inning)
        {
            string strScore;
            int score = stat.inningScore[team, inning];
            if (score == -2000) strScore = "" + "\t";
            else if (score == -1000) strScore = "X" + "\t";
            else if (score < 0) strScore = (-score) + "X" + "\t";
            else strScore = score + "\t";

            return strScore;
        }
#endif
    }
}
