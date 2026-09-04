using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using BaseBall.BallPlay;
using BallPlayParam = BaseBall.BallPlay.Param;

public class RttsManager : MonoBehaviour
{
    public const int MAX_TEAM = 10; //RTTS 리그 참여팀
    public const int MY_TEAM = 0;

    private const int RTTS_CURCULATING_VAULE = 36;  //36경기를 기점으로 순환함

    //기본정보
    public int League { get; private set; }
    //private bool bFirstTry;
    public int PlayGame { get; private set; }
    public int ScheduleIndex { get; private set; }
    public int TotalGame { get; private set; }
    public bool bPlayOff { get; private set; } = false;
    public int[] LeagueTeam { get; private set; } = new int[MAX_TEAM];


    //기록
    public Dictionary<int, int> StadingInfo { get; set; } = new Dictionary<int, int>();
    //
    public Dictionary<int, int> HomerunLeader { get; private set; } = new Dictionary<int, int>();
    public Dictionary<int, int> AvgLeader { get; private set; } = new Dictionary<int, int>();
    public Dictionary<int, int> RbiLeader { get; private set; } = new Dictionary<int, int>();
    public Dictionary<int, int> HitLeader { get; private set; } = new Dictionary<int, int>();
    public Dictionary<int, int> OpsLeader { get; private set; } = new Dictionary<int, int>();


    public int[] MyLeagueTeam { get; private set; } = new int[MAX_TEAM];

    //Rtts 보상 로드
    public Dictionary<int, int[]> RttsRewardList { get; private set; } = new Dictionary<int, int[]>();
    public List<int> RewardWinList { get; private set; } = new List<int>();//키값(승리)만을 넣은 리스트로 계산을 편하게 하기 위해 존재



    //플레이한 게임 결과
    private int CurrentMyResult;
    private int[] CurrentMyBallerRecord = null;
    private int CurrentMyXP;

    //플레이시 보상 있는지 여부
    public List<KOBRewardInfo> MyRewardList { get; private set; }


    //현재 순위를 버퍼에 저장
    public int[] CurrentRank = new int[10];


    //Todo
    //게임 인덱스로를 참고하여 선수 명단 만들기
    //만든 선수 명단을 이용하여 siuml돌림
    //simul돌린후 MyRttsInfo에 저장

    //Todo
    //MyRttsInfo를 이용하여 팀순위

    //Todo
    //MyRttsInfo를 이용하여 개인순위

    //Todo
    //명성로드
    //rtts로드


    //
    private KOBLocalRttsSave _localSave;
    public KOBLocalRttsSave LocalSave
    {
        get
        {
            if(_localSave == null)
            {
                _localSave = new KOBLocalRttsSave();
            }
            return _localSave;
        }
    }


    private int Seed; //스케쥴을 다양하게 하기 위한 값 : 추후 필요하니 저장을 해둘 것



    public void RttsEnter(Action<bool> action)
    {
        _localSave = KOBLocalRttsSave.Load();

        //Rtts기본정보
        MyRttsInfo myRttsInfo = KOBManager.MyInfo.GameData.RttsInfo;
        League = myRttsInfo.League;
        bool isNewLeague = false;
        if (myRttsInfo.PlayGame < 0)
        {
            isNewLeague = true;
        }
        PlayGame = myRttsInfo.PlayGame;
        Seed = myRttsInfo.Seed;
        ScheduleIndex = ((PlayGame + Seed) % RTTS_CURCULATING_VAULE) + 1;
        RttsInfo info = KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(League);
        TotalGame = info.TotalGame;
        bPlayOff = info.Playoff;

        //보상 정보 세팅
        MakeRttsRewardInfo();

        MyRewardList = null;

        if (isNewLeague)
        {
            TRequestRttsStart req = new TRequestRttsStart()
            {
                League = this.League
            };

            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                TResultRttsStart res = (TResultRttsStart)response;
                if (callback?.IsSuccess() == true && res?.isSuccess == true)
                {
                    action?.Invoke(true);
                }
                else
                {
                    int ErrorCode = res.ErrorCode;
                    Debug.Log("에러코드 : " + ErrorCode);
                }
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();

                //리그 시작 시 이거 다시 해줘 //--> 이런거 진짜 싫다 ㅠㅠ
                myRttsInfo = KOBManager.MyInfo.GameData.RttsInfo;
                PlayGame = myRttsInfo.PlayGame;
                Seed = myRttsInfo.Seed;
                ScheduleIndex = ((PlayGame + Seed) % RTTS_CURCULATING_VAULE) + 1;
                RttsInfo info = KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(League);
                TotalGame = info.TotalGame;
                bPlayOff = info.Playoff;
            });

        }
        else
        {
            action?.Invoke(false);
        }
    }


    public bool isLeagueFirstTry()
    {
        if (KOBManager.MyInfo.GameData.RttsInfo.League != KOBManager.MyInfo.GameData.RttsInfo.LastLeague)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    public void InitStandingInfo()
    {
        StadingInfo.Clear();
        Dictionary<int, TeamRecord> LeagueTeamRecord = KOBManager.MyInfo.GameData.RttsInfo.LeagueTeamRecord;
        for (int i = 0; i < LeagueTeam.Length; i++)
        {
            if (i == MY_TEAM) //내팀 인덱스
            {
                LeagueTeam[MY_TEAM] = 0;
            }
            else
            {
                int count = i;
                LeagueTeam[i] = League * KOBConstant.TEAM_RECORD_UNIT + count;//
            }

            //팀순위 정보 생성
            int WinningPer = 0;
            if (LeagueTeamRecord.ContainsKey(i) == true)
            {
                int win = LeagueTeamRecord[i].Win;
                int lose = LeagueTeamRecord[i].Lose;
                //int draw = LeagueTeamRecord[i].Draw;

                if (win + lose > 0) WinningPer = (win * 1000000) / (win + lose);
            }
            StadingInfo.Add(i, WinningPer);
        }
        MyLeagueTeam = LeagueTeam; //전역으로 쓸수 있도록
                                   //

    }



    public void InitLeagueLeaderInfo()
    {
        HomerunLeader.Clear();
        AvgLeader.Clear();
        RbiLeader.Clear();
        HitLeader.Clear();
        OpsLeader.Clear();

        Dictionary<int, BatterRecord> LeaguePlayerRecord = KOBManager.MyInfo.GameData.RttsInfo.LeaguePlayerRecord;
        //IReadOnlyDictionary<int, RttsTeam> teamChart = KOBManager.Backend.Chart.RttsTeam.Dictionary;

        int QPA = PlayGame * 2; //규정타석 -> 추후 조정할 것

        if (LeaguePlayerRecord.Count == 0)
        {
            //없으면 초기화 할것!!
            KOBManager.MyInfo.GameData.RttsInfo.InitLeaguePlayerRecord();
        }


        foreach (KeyValuePair<int, BatterRecord> record in LeaguePlayerRecord)
        {
            int idx = record.Key;
            int homerun = record.Value.HR;
            int hit = record.Value.H;
            int hit2 = record.Value.H2;
            int hit3 = record.Value.H3;
            int rbi = record.Value.RBI;
            int bb = record.Value.BB;
            int ab = record.Value.AB;
            int pa = ab + bb;
            int single = hit - (hit2 + hit3 + homerun);

            //타율계산
            int avg = 0;
            if (ab > 0)
            {
                avg = (hit * 100000) / ab;
            }
            //출루율계산
            int obp = 0;
            if (pa > 0)
            {
                obp = ((hit + bb) * 100000) / pa;
            }
            //장타율계산
            int slg = 0;
            if (ab > 0)
            {
                int total = (single + (hit2 * 2) + (hit3 * 3) + (homerun * 4));
                slg = (total * 100000) / ab;
            }
            //ops계산
            int ops = obp + slg;

            if (ab >= QPA) //규정타석 적용
            {
                avg += KOBConstant.QPA_CONSTANT;
                ops += KOBConstant.QPA_CONSTANT;
            }

            HomerunLeader.Add(idx, homerun);
            AvgLeader.Add(idx, avg);
            RbiLeader.Add(idx, rbi);
            HitLeader.Add(idx, hit);
            OpsLeader.Add(idx, ops);
        }

        //홈런 정렬
        var sortVar1 = from item in HomerunLeader
                       orderby item.Value descending
                       select item;
        HomerunLeader = sortVar1.ToDictionary(x => x.Key, x => x.Value);

        //타율 정렬
        var sortVar2 = from item in AvgLeader
                       orderby item.Value descending
                       select item;
        AvgLeader = sortVar2.ToDictionary(x => x.Key, x => x.Value);

        //rbi 정렬
        var sortVar3 = from item in RbiLeader
                       orderby item.Value descending
                       select item;
        RbiLeader = sortVar3.ToDictionary(x => x.Key, x => x.Value);

        //안타 정렬
        var sortVar4 = from item in HitLeader
                       orderby item.Value descending
                       select item;
        HitLeader = sortVar4.ToDictionary(x => x.Key, x => x.Value);

        //ops 정렬
        var sortVar5 = from item in OpsLeader
                       orderby item.Value descending
                       select item;
        OpsLeader = sortVar5.ToDictionary(x => x.Key, x => x.Value);

    }


    private List<int[]> todaySchedule = null;
    private bool todayIsHome;

    private bool _roundInProgress;
    private RttsGameResult _pendingMyResult;
    private readonly List<RttsGameResult> _pendingOtherResults = new List<RttsGameResult>();
    private Action<bool> _roundComplete;
    private bool _returnToRttsWindow;
    private bool _liveResultReceived;

    public bool IsRoundInProgress => _roundInProgress;
    public RttsGameResult LastGameResult { get; private set; }
    public int CurrentRttsRewardPoint => KOBManager.MyInfo.GameData.RttsInfo?.RewardPoint ?? 0;

    /// <summary>
    /// 한 일차의 전체 흐름을 실행합니다.
    /// 타 팀 4경기 시뮬레이션 -> 내 팀 실제 경기 -> 결과/보상 저장 -> 결과 화면 순서입니다.
    /// </summary>
    public void PlayRound(Action<bool> onComplete = null, bool returnToRttsWindow = false)
    {
        if (_roundInProgress)
        {
            Debug.LogWarning("RTTS 경기가 이미 진행 중입니다.");
            onComplete?.Invoke(false);
            return;
        }

        if (CheckRttsLeagueEnd())
        {
            Debug.LogWarning("RTTS 리그의 모든 경기가 종료되었습니다.");
            onComplete?.Invoke(false);
            return;
        }

        if (!PrepareTodaySchedule())
        {
            onComplete?.Invoke(false);
            return;
        }

        _roundInProgress = true;
        _roundComplete = onComplete;
        _returnToRttsWindow = returnToRttsWindow;
        _pendingMyResult = null;
        _pendingOtherResults.Clear();
        LastGameResult = null;
        MyRewardList = null;
        _liveResultReceived = false;
        StartCoroutine(playRound());
    }

    private IEnumerator playRound()
    {
        // 사용자가 설명한 리그 진행 순서대로 타 팀 경기를 먼저 완료합니다.
        yield return simulOtherGames(false);
        if (!_roundInProgress) yield break;

        // 내 팀 경기는 MainLoading -> BallPlay 정식 인게임으로 넘깁니다.
        // 결과는 BallPlayManager.setResult에서 CompleteLiveGame으로 돌아옵니다.
        yield return launchLiveGame();
    }

    private IEnumerator launchLiveGame()
    {
        int[] game = todaySchedule[0];
        try
        {
            var simulation = new RttsGameSimulation(League, PlayGame, Seed);
            _pendingMyResult = simulation.CreateResultTemplate(game[0], game[1], todayIsHome);

            tempSelectPage.ConfigureDongneYagu();
            Mode.BeginRttsGame(
                todayIsHome,
                game[1],
                _pendingMyResult.FirstTeamName,
                _pendingMyResult.SecondTeamName);
        }
        catch (Exception exception)
        {
            AbortRound("RTTS 정식 경기 진입 정보를 만들지 못했습니다.", exception);
            yield break;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync("MainLoading");
        if (loadOperation == null)
        {
            AbortRound("RTTS 정식 경기 로딩을 시작하지 못했습니다.", new InvalidOperationException("MainLoading scene is unavailable."));
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }

    private bool PrepareTodaySchedule()
    {
        RttsSchedule schedule = KOBManager.Backend.Chart.RttsSchedule.GetSchedule(ScheduleIndex);
        if (schedule == null || schedule.opponent <= MY_TEAM || schedule.opponent >= MAX_TEAM)
        {
            Debug.LogError("RTTS 스케줄을 불러오지 못했습니다. ScheduleIndex=" + ScheduleIndex);
            return false;
        }

        todayIsHome = schedule.home;
        todaySchedule = GetSchedule(schedule.opponent);
        if (todaySchedule == null || todaySchedule.Count != MAX_TEAM / 2)
        {
            Debug.LogError("RTTS 전체 대진표 생성에 실패했습니다. opponent=" + schedule.opponent);
            return false;
        }

        Debug.Log($"RTTS {PlayGame + 1}일차: 내 상대={schedule.opponent}, 홈={todayIsHome}, 전체 경기={todaySchedule.Count}");
        return true;
    }

    /// <summary>
    /// 이건 테스트 혹은 추가될수도 있는 자동 티켓으로 
    /// </summary>
    public void SimulMyGame(Action action)
    {
        if (todaySchedule == null && !PrepareTodaySchedule())
        {
            action?.Invoke();
            return;
        }
        StartCoroutine(simulateMyGame(action));
    }

    private IEnumerator simulateMyGame(Action action)
    {
        int[] game = todaySchedule[0];
        try
        {
            var simulation = new RttsGameSimulation(League, PlayGame, Seed);
            _pendingMyResult = simulation.Simulate(game[0], game[1], todayIsHome);
            SetMyResult(_pendingMyResult);
        }
        catch (Exception exception)
        {
            AbortRound("내 팀 경기 시뮬레이션에 실패했습니다.", exception);
        }

        yield return null;
        action?.Invoke();
    }

    public void SetMyResult(RttsGameResult result)
    {
        if (result == null) return;

        LastGameResult = result;
        CurrentMyResult = result.GetResultForFirstTeam();
        CurrentMyXP = 0;

        int selectedBaller = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;
        RttsPlayerGameRecord selectedRecord = result.FirstTeamPlayers.Find(player => player.PlayerIndex == selectedBaller);
        CurrentMyBallerRecord = selectedRecord != null
            ? (int[])selectedRecord.Record.Clone()
            : new int[GrowthInfo.MAX_RECORD];

        KOBGameData gameData = KOBManager.MyInfo.GameData;
        KOBBaller baller = gameData.GetBaller(selectedBaller);
        int ballerReputationBefore = baller != null ? baller.baller_trophy : 0;

        result.ProgressReward.SetPending(
            gameData.GrowthInfo.Trophy,
            ballerReputationBefore,
            gameData.RttsInfo.RewardPoint,
            RttsHardcodedRewardConfig.TrophyRoadPoint,
            RttsHardcodedRewardConfig.BallerReputationPoint,
            RttsHardcodedRewardConfig.RttsRewardPoint);

        //기존 ResultPlayer 연출과의 호환을 위해 실제 UI 필드에도 같은 값을 제공합니다.
        result.TrophyGain = RttsHardcodedRewardConfig.TrophyRoadPoint;
        result.FameGain = RttsHardcodedRewardConfig.BallerReputationPoint;
    }

    /// <summary>
    /// BallPlay 정식 경기가 끝났을 때 인게임 기록을 RTTS 결과로 회수합니다.
    /// true를 반환하면 BallPlay의 레거시 ResultUI 대신 RTTS 결과 흐름이 결과를 처리합니다.
    /// </summary>
    public bool CompleteLiveGame(BallPlayManager manager)
    {
        if (!Mode.bRttsMode)
        {
            return false;
        }

        Mode.EndRttsGame();
        if (!_roundInProgress || _liveResultReceived || manager == null || _pendingMyResult == null)
        {
            Debug.LogError("RTTS 정식 경기 결과를 받을 수 없는 상태입니다.");
            return false;
        }

        _liveResultReceived = true;
        try
        {
            CopyLiveGameResult(manager, _pendingMyResult);
            SetMyResult(_pendingMyResult);
        }
        catch (Exception exception)
        {
            AbortRound("RTTS 정식 경기 결과 변환에 실패했습니다.", exception);
            StartCoroutine(returnFromLiveGame(false));
            return true;
        }

        StartCoroutine(returnFromLiveGame(true));
        return true;
    }

    private static void CopyLiveGameResult(BallPlayManager manager, RttsGameResult result)
    {
        for (int team = 0; team < 2; team++)
        {
            result.Stat.Score[team] = manager.nGameScore[team];
            result.Stat.Hit[team] = manager.nHitCount[team];
            result.Stat.HomeRun[team] = manager.nHomerunCount[team];
            result.Stat.Steal[team] = manager.nStealCount[team];
            result.Stat.Error[team] = manager.nErrorCount[team];
            result.Stat.Walk[team] = manager.nFourballCount[team];

            for (int inning = 0; inning < SimulGameInfo.MAX_INNING; inning++)
            {
                result.Stat.InningScore[team][inning] = ConvertLiveInningScore(manager.nInningScore[team, inning]);
            }
        }

        CopyLivePlayerRecords(0, result.FirstTeamPlayers);
        CopyLivePlayerRecords(1, result.SecondTeamPlayers);

        // 인게임의 nStrikeOutCount는 일부 경로에서 투수 팀 기준으로 쌓이므로,
        // RTTS 결과에서는 타자 개인 기록의 삼진 합계를 사용합니다.
        result.Stat.StrikeOut[0] = SumPlayerRecord(result.FirstTeamPlayers, RttsPlayerGameRecord.StrikeOut);
        result.Stat.StrikeOut[1] = SumPlayerRecord(result.SecondTeamPlayers, RttsPlayerGameRecord.StrikeOut);
    }

    private static int ConvertLiveInningScore(int score)
    {
        if (score == SimulParm.NOPLAY_INNING) return RttsGameResult.NoPlayInning;
        if (score == SimulParm.GAMEEND_INNING) return RttsGameResult.GameEndInning;

        // 레거시 엔진은 끝내기 득점이 나온 이닝을 음수로 표시합니다.
        return Math.Abs(score);
    }

    private static void CopyLivePlayerRecords(int liveTeamIndex, List<RttsPlayerGameRecord> destination)
    {
        if (destination == null) return;

        for (int index = 0; index < BallPlayManager.NUM_FIELDER; index++)
        {
            CPlayer source = SimulPlayerManager.GetFielder(liveTeamIndex, index);
            if (source == null) continue;

            // 교체된 선발은 벤치 배열로 이동할 수 있으므로 현재 배열 위치가 아니라
            // 경기 시작 당시 타순(originLineup)으로 RTTS 선수를 찾습니다.
            int battingOrder = source.originLineup;
            if (battingOrder < 0 || battingOrder >= destination.Count) continue;

            RttsPlayerGameRecord target = destination[battingOrder];
            target.Record[RttsPlayerGameRecord.Game] = 1;
            target.Record[RttsPlayerGameRecord.PlateAppearance] = source.getStat(BallPlayParam.ST_PA);
            target.Record[RttsPlayerGameRecord.AtBat] = source.getStat(BallPlayParam.ST_AB);
            target.Record[RttsPlayerGameRecord.Hit] = source.getStat(BallPlayParam.ST_H);
            target.Record[RttsPlayerGameRecord.Double] = source.getStat(BallPlayParam.ST_2B);
            target.Record[RttsPlayerGameRecord.Triple] = source.getStat(BallPlayParam.ST_3B);
            target.Record[RttsPlayerGameRecord.HomeRun] = source.getStat(BallPlayParam.ST_HR);
            target.Record[RttsPlayerGameRecord.Rbi] = source.getStat(BallPlayParam.ST_RBI);
            target.Record[RttsPlayerGameRecord.Run] = source.getStat(BallPlayParam.ST_R);
            target.Record[RttsPlayerGameRecord.Walk] = source.getStat(BallPlayParam.ST_BB);
            target.Record[RttsPlayerGameRecord.HitByPitch] = source.getStat(BallPlayParam.ST_HBP);
            target.Record[RttsPlayerGameRecord.StrikeOut] = source.getStat(BallPlayParam.ST_SO);
            target.Record[RttsPlayerGameRecord.Steal] = source.getStat(BallPlayParam.ST_SBS);
        }
    }

    private static int SumPlayerRecord(List<RttsPlayerGameRecord> players, int recordIndex)
    {
        int total = 0;
        if (players == null) return total;

        for (int i = 0; i < players.Count; i++)
        {
            total += players[i].Record[recordIndex];
        }
        return total;
    }

    private IEnumerator returnFromLiveGame(bool completeRound)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync("MainLobby");
        if (loadOperation == null)
        {
            if (completeRound)
            {
                AbortRound("RTTS 경기 후 로비로 돌아가지 못했습니다.", new InvalidOperationException("MainLobby scene is unavailable."));
            }
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // LobbyInitializer가 UI 레지스트리를 새 씬의 오브젝트로 교체할 시간을 줍니다.
        yield return null;

        if (completeRound && _roundInProgress && _pendingMyResult != null)
        {
            CompleteRound();
        }
    }


    /// <summary>
    /// 나의 게임을 제외한 다른 게임을 플레이 한후 정보 저장
    /// </summary>
    public void SimulOtherGames()
    {
        if (todaySchedule == null && !PrepareTodaySchedule()) return;
        StartCoroutine(simulOtherGames(true));
    }


    private IEnumerator simulOtherGames(bool completeRound)
    {
        _pendingOtherResults.Clear();
        for (int i = 1; i < todaySchedule.Count; i++)
        {
            int[] game = todaySchedule[i];
            try
            {
                // 타 팀 홈/원정도 일차와 대진 위치에 따라 교차시켜 한쪽 편향을 막습니다.
                bool firstTeamIsHome = ((ScheduleIndex + i) & 1) == 0;
                var simulation = new RttsGameSimulation(League, PlayGame, Seed);
                RttsGameResult result = simulation.Simulate(game[0], game[1], firstTeamIsHome);
                _pendingOtherResults.Add(result);
            }
            catch (Exception exception)
            {
                AbortRound($"타 팀 경기 시뮬레이션에 실패했습니다. team={game[0]} vs {game[1]}", exception);
                yield break;
            }

            yield return null;
        }

        if (completeRound)
        {
            if (_pendingMyResult == null)
            {
                Debug.LogError("SimulOtherGames 전에 SimulMyGame 결과가 필요합니다.");
                _roundInProgress = false;
                yield break;
            }
            CompleteRound();
        }
    }

    private void CompleteRound()
    {
        try
        {
            MyRttsInfo leagueInfo = KOBManager.Backend.GameData.KOBGameData.RttsInfo;
            for (int i = 0; i < _pendingOtherResults.Count; i++)
            {
                RttsGameSimulation.ApplyToLeague(leagueInfo, _pendingOtherResults[i]);
            }
            RttsGameSimulation.ApplyToLeague(leagueInfo, _pendingMyResult);
        }
        catch (Exception exception)
        {
            AbortRound("RTTS 경기 결과를 리그 기록에 반영하지 못했습니다.", exception);
            return;
        }

        MyRewardList = null; //결과 화면 보기전 null로 초기화
        TRequestBattleEnd req = new TRequestBattleEnd()
        {
            League = this.League,
            Result = CurrentMyResult,
            myRecord = CurrentMyBallerRecord,
            ballerIdx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller,
            GetXP = CurrentMyXP,
            TrophyRoadPoint = RttsHardcodedRewardConfig.TrophyRoadPoint,
            BallerReputationPoint = RttsHardcodedRewardConfig.BallerReputationPoint,
            RttsRewardPoint = RttsHardcodedRewardConfig.RttsRewardPoint
        };

        KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
        {
            TResultBattleEnd res = response as TResultBattleEnd;
            if (callback?.IsSuccess() == true && res?.isSuccess == true)
            {
                Debug.Log("RTTS 배틀엔드 완료");
                MyRewardList = res.RewardList;    //보상 정보 세팅해줌
                FinalizeProgressReward(_pendingMyResult);
                RefreshLeagueState();

                Intent intent = new Intent();
                intent["RttsGameResult"] = _pendingMyResult;
                KOBManager.Popup.OpenPopup<Popup_GameResult>().Set(intent);
                _roundComplete?.Invoke(true);
                _roundComplete = null;
            }
            else
            {
                int errorCode = res?.ErrorCode ?? -1;
                Debug.LogError("RTTS 배틀엔드 실패. 에러코드 : " + errorCode);
                _roundInProgress = false;
                _returnToRttsWindow = false;
                _roundComplete?.Invoke(false);
                _roundComplete = null;
            }
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
        });
    }

    private void FinalizeProgressReward(RttsGameResult result)
    {
        if (result?.ProgressReward == null) return;

        KOBGameData gameData = KOBManager.MyInfo.GameData;
        KOBBaller baller = gameData.GetBaller(gameData.ManageInfo.SelectBaller);
        int ballerReputationAfter = baller != null ? baller.baller_trophy : result.ProgressReward.BallerReputationBefore;

        result.ProgressReward.SetActualValues(
            gameData.GrowthInfo.Trophy,
            ballerReputationAfter,
            gameData.RttsInfo.RewardPoint);

        result.TrophyGain = result.ProgressReward.TrophyRoadGain;
        result.FameGain = result.ProgressReward.BallerReputationGain;
    }

    private void AbortRound(string message, Exception exception)
    {
        Debug.LogError(message + "\n" + exception);
        Mode.EndRttsGame();
        _roundInProgress = false;
        _liveResultReceived = false;
        _pendingMyResult = null;
        _pendingOtherResults.Clear();
        _returnToRttsWindow = false;
        _roundComplete?.Invoke(false);
        _roundComplete = null;
    }


    public List<int[]> GetSchedule(int myOpponent)
    {
        var list = new List<int[]> { new[] { MY_TEAM, myOpponent } };
        var remainingTeams = new List<int>();
        for (int team = 1; team < MAX_TEAM; team++)
        {
            if (team != myOpponent) remainingTeams.Add(team);
        }

        // 상대 팀 번호부터 순환해 같은 일차에 모든 팀이 정확히 한 번만 출전하게 합니다.
        remainingTeams = remainingTeams
            .OrderBy(team => (team - myOpponent + MAX_TEAM) % MAX_TEAM)
            .ToList();
        for (int i = 0; i + 1 < remainingTeams.Count; i += 2)
        {
            list.Add(new[] { remainingTeams[i], remainingTeams[i + 1] });
        }
        return list;
    }

    public int GetTeamIndex(int index)
    {
        int teamIdx = League * KOBConstant.TEAM_RECORD_UNIT + index;
        return teamIdx;
    }

    public RttsTeam GetTeam(int index)
    {
        int teamIdx = GetTeamIndex(index);
        RttsTeam teamInfo = KOBManager.Backend.Chart.RttsTeam.GetRttsTeam(teamIdx);
        return teamInfo;
    }

    public RttsSchedule GetMySchedule()
    {
        return KOBManager.Backend.Chart.RttsSchedule.GetSchedule(ScheduleIndex);
    }

    public RttsInfo GetMyLeagueInfo()
    {
        return KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(League);
    }


    public RttsSchedule GetMySchedule(int gab)
    {
        int _ScheduleIndex = ((PlayGame + Seed + gab) % RTTS_CURCULATING_VAULE) + 1;
        return KOBManager.Backend.Chart.RttsSchedule.GetSchedule(_ScheduleIndex);
    }


    public bool isSelectBaller(int teamIdx, int playerIdx)
    {
        if (teamIdx == 0 && playerIdx == KOBManager.MyInfo.GameData.ManageInfo.SelectBaller)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 인덱스로 팀과 플레이어 구분
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public bool isSelectBaller(int idx)
    {
        int teamIdx = idx / KOBConstant.PLAYER_RECORD_UNIT;
        int playerIdx = idx % KOBConstant.PLAYER_RECORD_UNIT;
        if (teamIdx == 0 && playerIdx == KOBManager.MyInfo.GameData.ManageInfo.SelectBaller)
        {
            return true;
        }
        return false;
    }





    public int[] CurrentWinDrawLose(int teamIdx)
    {
        if (KOBManager.MyInfo.GameData.RttsInfo.LeagueTeamRecord.ContainsKey(teamIdx) == true)
        {
            TeamRecord teamRecord = KOBManager.MyInfo.GameData.RttsInfo.LeagueTeamRecord[teamIdx];
            return new int[] { teamRecord.Win, teamRecord.Draw, teamRecord.Lose };
        }
        else
        {
            return new int[] { 0, 0, 0 };   
        }
    }


    private void MakeRttsRewardInfo()
    {
        RttsRewardList.Clear();
        RewardWinList.Clear();

        RttsReward rewardInfo = KOBManager.Backend.Chart.RttsReward.GetRttsReward(League);
        int TotalGame = KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(League).TotalGame;
        bool bFirstTry = isLeagueFirstTry();

        int[] small = rewardInfo.small_reward_pos;
        int[] big = rewardInfo.big_reward_pos;
        int bigValue = big[0];
        int smallValue = small[0];
        
  

        //이 작업을 로비 진입시 하여 글로벌에 보관한다!!!!!
        for (int i = 0; i < TotalGame; i++)
        {
            int count = i + 1;

            if (count == TotalGame)
            {
                //최종
                //Debug.Log("최종 보상 : " + count);
                if (RttsRewardList.ContainsKey(count) == false)
                {
                    RttsRewardList.Add(count, new int[] { 0, rewardInfo.winall_reward[0], rewardInfo.winall_reward[1] });
                    RewardWinList.Add(count);
                }
            }
            else
            {
                bool bCardGet = false;
                
                if (bCardGet == false)
                {
                    if (bFirstTry == true && count == bigValue)
                    {
                        Debug.Log("빅 보상 : " + count);
                        if (RttsRewardList.ContainsKey(count) == false)
                        {
                            RttsRewardList.Add(count, new int[] { 0, big[2], big[3] });
                            RewardWinList.Add(count);
                        }
                        bigValue += big[1];
                    }
                    else if (count == smallValue)
                    {
                        Debug.Log("스몰 보상 : " + count);
                        if (RttsRewardList.ContainsKey(count) == false)
                        {
                            RttsRewardList.Add(count, new int[] { 0, small[2], small[3] });
                            RewardWinList.Add(count);
                        }
                        smallValue += small[1];
                    }
                }
            }
        }
    }

    public List<KOBRewardInfo> GetRttsRewardInfo(int currentPoint)
    {
        List<KOBRewardInfo> kobRewardList = new List<KOBRewardInfo>();
        if (RttsRewardList.ContainsKey(currentPoint) == true)
        {
            int[] value = RttsRewardList[currentPoint];
            KOBReward _type = (KOBReward)value[0];
            if (_type == KOBReward.None)
            {
                List<RewardData> rewardData = KOBManager.Backend.Chart.RewardData.GetRewards(value[1]);
                if(rewardData != null)
                {
                    for (int i = 0; i < rewardData.Count; i++)
                    {
                        KOBRewardInfo reward = new KOBRewardInfo(rewardData[i]);
                        kobRewardList.Add(reward);
                    }
                    return kobRewardList;
                }
                return null;
            }
            else
            {
                KOBRewardInfo reward = new KOBRewardInfo(_type, value[1], value[2]);
                kobRewardList.Add(reward);
                return kobRewardList;
            }
        }
        return null;
    }

    /// <summary>
    /// Rtts경기를 마치고 로비로 돌아가는
    /// </summary>
    public void BackToLobby()
    {
        bool returnToRttsWindow = _returnToRttsWindow;
        Mode.EndRttsGame();
        _roundInProgress = false;
        _liveResultReceived = false;
        _returnToRttsWindow = false;
        _pendingMyResult = null;
        _pendingOtherResults.Clear();
        UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.Rtts;

        if (MyRewardList != null && MyRewardList.Count > 0)
        {
            Intent intent = new Intent();
            intent["RewardList"] = MyRewardList;
            intent["isBox"] = false; //경기 보상은 상자가 아니라 각 보상을 순서대로 노출
            intent.AddIntentData<UIPopup.OnClickAction>(UIPopup.ON_CLOSE, () =>
            {
                MyRewardList = null;
                ReturnAfterResult(returnToRttsWindow);
            });
            KOBManager.Popup.OpenPopup<Popup_RewardGet>().Set(intent);
            return;
        }

        MyRewardList = null;
        ReturnAfterResult(returnToRttsWindow);
    }

    private void ReturnAfterResult(bool returnToRttsWindow)
    {
        if (returnToRttsWindow)
        {
            RefreshLeagueState();
            KOBManager.UI.OpenWindow<UI_RTTS>();
            return;
        }

        KOBManager.State.BackToLobby();
    }

    private void RefreshLeagueState()
    {
        MyRttsInfo leagueInfo = KOBManager.MyInfo.GameData.RttsInfo;
        if (leagueInfo == null) return;

        League = leagueInfo.League;
        PlayGame = leagueInfo.PlayGame;
        Seed = leagueInfo.Seed;
        ScheduleIndex = ((PlayGame + Seed) % RTTS_CURCULATING_VAULE) + 1;

        RttsInfo chartInfo = KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(League);
        if (chartInfo != null)
        {
            TotalGame = chartInfo.TotalGame;
            bPlayOff = chartInfo.Playoff;
        }
    }



    public bool CheckRttsLeagueEnd()
    {
        Debug.Log("리그종료 체크 PlayGame : " + PlayGame + "   // TotalGame = " + TotalGame);
        if(PlayGame >= TotalGame)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateRttsLeague()
    {

    }



    public List<int[]> GetLeagueLeaders()
    {
        //선수 인덱스 / 개수
        InitLeagueLeaderInfo(); //정확도를 위해 재계산

        List<int[]> list = new List<int[]>();
        var firstHr = HomerunLeader.FirstOrDefault();
        list.Add(new int[] { firstHr.Key, firstHr.Value });

        var firstAvg = AvgLeader.FirstOrDefault();
        list.Add(new int[] { firstAvg.Key, firstAvg.Value });

        var firstRbi = RbiLeader.FirstOrDefault();
        list.Add(new int[] { firstRbi.Key, firstRbi.Value });

        var firstHit = HitLeader.FirstOrDefault();
        list.Add(new int[] { firstHit.Key, firstHit.Value });

        var firstOps = OpsLeader.FirstOrDefault();
        list.Add(new int[] { firstOps.Key, firstOps.Value });

        return list;

    }

    public int[] GetLeagueStanding()
    {
        //0순위 1 승리 2 무승부 3 패배
        InitStandingInfo(); //정확도를 위해 재계산

        var sortVar1 = from item in StadingInfo
                       orderby item.Value descending
                       select item;
        StadingInfo = sortVar1.ToDictionary(x => x.Key, x => x.Value);
        int Rank = 0;
        foreach ( var item in StadingInfo )
        {
            Rank++;
            if (item.Key == MY_TEAM)
            {
                break;
            }
        }
        int[] myTeam = CurrentWinDrawLose(MY_TEAM);

        return new int[] {Rank, myTeam[0], myTeam[1], myTeam[2] };
    }



    public void RttsLocalSave()
    {
        KOBLocalRttsSave.Save(_localSave);
    }



    /// <summary>
    /// Rtts리그 종료에 의한 리그 최종 정산 체크
    /// </summary>
    /// <returns></returns>
    public bool RttsLeagueEndEvent(Action<TResultRttsLeagueUpgrade> action = null)
    {
        bool leagueEnd = CheckRttsLeagueEnd();
        if (leagueEnd == true)
        {
            TRequestRttsLeagueUpgrade req = new TRequestRttsLeagueUpgrade()
            {
                CurrentLeague = League,
            };
            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                TResultRttsLeagueUpgrade res = (TResultRttsLeagueUpgrade)response;
                if (callback?.IsSuccess() == true && res?.isSuccess == true)
                {
                    //KOBManager.UI.OpenWindow<UI_RTTSResult>().Set(res);
                    action?.Invoke(res);
                }
                else
                {
                    int ErrorCode = res.ErrorCode;
                    Debug.Log("에러코드 : " + ErrorCode);
                }
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            });
            return true; //로비 연출 중단
        }
        return false;
    }

}
