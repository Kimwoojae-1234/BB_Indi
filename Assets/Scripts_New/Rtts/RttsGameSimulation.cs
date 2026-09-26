using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 서버 보상 테이블이 준비되기 전까지 사용하는 RTTS 경기 종료 보상값입니다.
/// 실제 서버 보상 정책이 확정되면 이 클래스만 테이블/응답값으로 교체하면 됩니다.
/// </summary>
public static class RttsHardcodedRewardConfig
{
    public const int TrophyRoadPoint = 10;
    public const int BallerReputationPoint = 10;
    public const int RttsRewardPoint = 1;

    public const int CardAmount = 1;
    public const int GoldMin = 100;
    public const int GoldMaxExclusive = 301;
    public const int FreeGemMin = 5;
    public const int FreeGemMaxExclusive = 16;
}

/// <summary>
/// ResultPlayer에 표시할 세 종류의 누적 보상 진행값입니다.
/// 현재 UI가 완성되지 않아 데이터와 조회 API만 먼저 제공합니다.
/// </summary>
[Serializable]
public sealed class RttsProgressReward
{
    public int TrophyRoadBefore;
    public int TrophyRoadGain;
    public int TrophyRoadAfter;

    public int BallerReputationBefore;
    public int BallerReputationGain;
    public int BallerReputationAfter;

    public int RttsRewardBefore;
    public int RttsRewardGain;
    public int RttsRewardAfter;

    public void SetPending(
        int trophyRoadBefore,
        int ballerReputationBefore,
        int rttsRewardBefore,
        int trophyRoadGain,
        int ballerReputationGain,
        int rttsRewardGain)
    {
        TrophyRoadBefore = trophyRoadBefore;
        TrophyRoadGain = trophyRoadGain;
        TrophyRoadAfter = trophyRoadBefore + trophyRoadGain;

        BallerReputationBefore = ballerReputationBefore;
        BallerReputationGain = ballerReputationGain;
        BallerReputationAfter = ballerReputationBefore + ballerReputationGain;

        RttsRewardBefore = rttsRewardBefore;
        RttsRewardGain = rttsRewardGain;
        RttsRewardAfter = rttsRewardBefore + rttsRewardGain;
    }

    public void SetActualValues(int trophyRoadAfter, int ballerReputationAfter, int rttsRewardAfter)
    {
        TrophyRoadAfter = trophyRoadAfter;
        TrophyRoadGain = TrophyRoadAfter - TrophyRoadBefore;

        BallerReputationAfter = ballerReputationAfter;
        BallerReputationGain = BallerReputationAfter - BallerReputationBefore;

        RttsRewardAfter = rttsRewardAfter;
        RttsRewardGain = RttsRewardAfter - RttsRewardBefore;
    }

    public int GetBefore(TR_RewardComp.TR_Type type)
    {
        switch (type)
        {
            case TR_RewardComp.TR_Type.TrophyRoad: return TrophyRoadBefore;
            case TR_RewardComp.TR_Type.BallerReputation: return BallerReputationBefore;
            case TR_RewardComp.TR_Type.RttsReward: return RttsRewardBefore;
            default: return 0;
        }
    }

    public int GetGain(TR_RewardComp.TR_Type type)
    {
        switch (type)
        {
            case TR_RewardComp.TR_Type.TrophyRoad: return TrophyRoadGain;
            case TR_RewardComp.TR_Type.BallerReputation: return BallerReputationGain;
            case TR_RewardComp.TR_Type.RttsReward: return RttsRewardGain;
            default: return 0;
        }
    }

    public int GetAfter(TR_RewardComp.TR_Type type)
    {
        switch (type)
        {
            case TR_RewardComp.TR_Type.TrophyRoad: return TrophyRoadAfter;
            case TR_RewardComp.TR_Type.BallerReputation: return BallerReputationAfter;
            case TR_RewardComp.TR_Type.RttsReward: return RttsRewardAfter;
            default: return 0;
        }
    }
}

/// <summary>
/// ProjectBB_Clutch 전용 RTTS 경기 결과입니다.
/// KOB의 인게임 SimulMain에 묶이지 않으므로 로비에서도 전체 리그를 계산할 수 있습니다.
/// 모든 팀/선수 배열은 [내 팀 또는 첫 번째 팀, 상대 또는 두 번째 팀] 순서입니다.
/// </summary>
[Serializable]
public sealed class RttsGameResult
{
    public const int NoPlayInning = -1;
    public const int GameEndInning = -2;

    public int FirstTeamIndex;
    public int SecondTeamIndex;
    public bool FirstTeamIsHome;
    public string FirstTeamName;
    public string SecondTeamName;
    public RttsTeamGameStat Stat = new RttsTeamGameStat();
    public List<RttsPlayerGameRecord> FirstTeamPlayers = new List<RttsPlayerGameRecord>();
    public List<RttsPlayerGameRecord> SecondTeamPlayers = new List<RttsPlayerGameRecord>();

    // 결과 화면의 성장 연출에 사용합니다. 서버 반영 직전 값으로 계산됩니다.
    public int TrophyGain;
    public int FameGain;
    public RttsProgressReward ProgressReward = new RttsProgressReward();

    public int GetResultForFirstTeam()
    {
        return KOBPointCalUtil.Cal_ResultValue(Stat.Score[0], Stat.Score[1]);
    }

    public List<RttsPlayerGameRecord> GetPlayers(int teamSlot)
    {
        return teamSlot == 0 ? FirstTeamPlayers : SecondTeamPlayers;
    }
}

[Serializable]
public sealed class RttsTeamGameStat
{
    public int[] Score = new int[2];
    public int[] Hit = new int[2];
    public int[] HomeRun = new int[2];
    public int[] Steal = new int[2];
    public int[] StrikeOut = new int[2];
    public int[] Error = new int[2];
    public int[] Walk = new int[2];
    public int[][] InningScore =
    {
        new int[12],
        new int[12]
    };

    public RttsTeamGameStat()
    {
        for (int team = 0; team < InningScore.Length; team++)
        {
            for (int inning = 0; inning < InningScore[team].Length; inning++)
            {
                InningScore[team][inning] = RttsGameResult.NoPlayInning;
            }
        }
    }
}

[Serializable]
public sealed class RttsPlayerGameRecord
{
    // 기존 GrowthInfo/KOB 시뮬레이터의 기록 인덱스와 동일합니다.
    public const int Game = 0;
    public const int PlateAppearance = 1;
    public const int AtBat = 2;
    public const int Hit = 3;
    public const int Double = 4;
    public const int Triple = 5;
    public const int HomeRun = 6;
    public const int Rbi = 7;
    public const int Run = 8;
    public const int Walk = 9;
    public const int HitByPitch = 10;
    public const int StrikeOut = 11;
    public const int Steal = 13;

    public int PlayerIndex;
    public int BattingOrder;
    public int Position;
    public int Level;
    public int Overall;
    public string Name;
    public int[] Record = new int[GrowthInfo.MAX_RECORD];

    public BatterRecord ToLeagueRecord()
    {
        return new BatterRecord
        {
            Game = ToShort(Record[Game]),
            AB = ToShort(Record[AtBat]),
            H = ToShort(Record[Hit]),
            H2 = ToShort(Record[Double]),
            H3 = ToShort(Record[Triple]),
            HR = ToShort(Record[HomeRun]),
            RBI = ToShort(Record[Rbi]),
            BB = ToShort(Record[Walk])
        };
    }

    private static short ToShort(int value)
    {
        return (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, value));
    }
}

/// <summary>
/// 한 경기의 모든 타석을 처리하고 RTTS용 팀/개인 기록을 생성합니다.
/// 동일 리그/일차/대진은 같은 seed를 사용하므로 저장 재시도 중 결과가 바뀌지 않습니다.
/// </summary>
public sealed class RttsGameSimulation
{
    private const int RegulationInnings = 9;
    private const int MaxInnings = 12;
    private const int DefaultAbility = 40;

    private sealed class SimPlayer
    {
        public RttsPlayerGameRecord Data;
        public int Contact;
        public int Power;
        public int Vision;
        public int Speed;
        public bool IsPitcher;
    }

    private sealed class SimTeam
    {
        public int TeamIndex;
        public string Name;
        public int Pitching;
        public int NextBatter;
        public List<SimPlayer> Players = new List<SimPlayer>();
    }

    private readonly int _league;
    private readonly int _playGame;
    private readonly int _leagueSeed;

    public RttsGameSimulation(int league, int playGame, int leagueSeed)
    {
        _league = league;
        _playGame = playGame;
        _leagueSeed = leagueSeed;
    }

    public RttsGameResult Simulate(int firstTeamIndex, int secondTeamIndex, bool firstTeamIsHome)
    {
        SimTeam first = BuildTeam(firstTeamIndex);
        SimTeam second = BuildTeam(secondTeamIndex);
        if (first.Players.Count == 0 || second.Players.Count == 0)
        {
            throw new InvalidOperationException("RTTS 라인업을 만들 수 없습니다. 팀/선수 차트와 내 덱을 확인해 주세요.");
        }

        int seed = MakeSeed(firstTeamIndex, secondTeamIndex);
        var random = new System.Random(seed);
        var result = new RttsGameResult
        {
            FirstTeamIndex = firstTeamIndex,
            SecondTeamIndex = secondTeamIndex,
            FirstTeamIsHome = firstTeamIsHome,
            FirstTeamName = first.Name,
            SecondTeamName = second.Name,
            FirstTeamPlayers = first.Players.Select(player => player.Data).ToList(),
            SecondTeamPlayers = second.Players.Select(player => player.Data).ToList()
        };

        for (int i = 0; i < first.Players.Count; i++) first.Players[i].Data.Record[RttsPlayerGameRecord.Game] = 1;
        for (int i = 0; i < second.Players.Count; i++) second.Players[i].Data.Record[RttsPlayerGameRecord.Game] = 1;

        int homeSlot = firstTeamIsHome ? 0 : 1;
        int awaySlot = 1 - homeSlot;
        SimTeam[] teams = { first, second };

        for (int inning = 0; inning < MaxInnings; inning++)
        {
            SimulateHalfInning(teams[awaySlot], awaySlot, teams[homeSlot].Pitching, inning, false, result, random);

            // 9회 이후 홈 팀이 앞서고 있으면 말 공격을 하지 않습니다.
            if (inning >= RegulationInnings - 1 && result.Stat.Score[homeSlot] > result.Stat.Score[awaySlot])
            {
                result.Stat.InningScore[homeSlot][inning] = RttsGameResult.GameEndInning;
                break;
            }

            bool walkOffEnabled = inning >= RegulationInnings - 1;
            SimulateHalfInning(teams[homeSlot], homeSlot, teams[awaySlot].Pitching, inning, walkOffEnabled, result, random);

            if (inning >= RegulationInnings - 1 && result.Stat.Score[0] != result.Stat.Score[1])
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// 실제 인게임에서 만들어진 기록을 담기 위한 RTTS 결과 골격을 생성합니다.
    /// 선수 인덱스/타순/레벨은 RTTS 리그 데이터 기준으로 유지하고,
    /// 경기 종료 시 인게임 기록만 이 객체에 복사합니다.
    /// </summary>
    public RttsGameResult CreateResultTemplate(int firstTeamIndex, int secondTeamIndex, bool firstTeamIsHome)
    {
        SimTeam first = BuildTeam(firstTeamIndex);
        SimTeam second = BuildTeam(secondTeamIndex);
        if (first.Players.Count == 0 || second.Players.Count == 0)
        {
            throw new InvalidOperationException("RTTS 실제 경기 결과용 라인업을 만들 수 없습니다. 팀/선수 차트와 내 덱을 확인해 주세요.");
        }

        var result = new RttsGameResult
        {
            FirstTeamIndex = firstTeamIndex,
            SecondTeamIndex = secondTeamIndex,
            FirstTeamIsHome = firstTeamIsHome,
            FirstTeamName = first.Name,
            SecondTeamName = second.Name,
            FirstTeamPlayers = first.Players.Select(player => player.Data).ToList(),
            SecondTeamPlayers = second.Players.Select(player => player.Data).ToList()
        };

        SetGamePlayed(result.FirstTeamPlayers);
        SetGamePlayed(result.SecondTeamPlayers);
        return result;
    }

    private static void SetGamePlayed(List<RttsPlayerGameRecord> players)
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Record[RttsPlayerGameRecord.Game] = 1;
        }
    }

    public static void ApplyToLeague(MyRttsInfo leagueInfo, RttsGameResult result)
    {
        if (leagueInfo == null || result == null) return;

        leagueInfo.UpdateLeagueTeamRecord(result.FirstTeamIndex, result.Stat.Score[0], result.Stat.Score[1]);
        leagueInfo.UpdateLeagueTeamRecord(result.SecondTeamIndex, result.Stat.Score[1], result.Stat.Score[0]);

        ApplyPlayerRecords(leagueInfo, result.FirstTeamIndex, result.FirstTeamPlayers);
        ApplyPlayerRecords(leagueInfo, result.SecondTeamIndex, result.SecondTeamPlayers);

        if (result.FirstTeamIndex == RttsManager.MY_TEAM || result.SecondTeamIndex == RttsManager.MY_TEAM)
        {
            int mySlot = result.FirstTeamIndex == RttsManager.MY_TEAM ? 0 : 1;
            leagueInfo.UpdateLeagueResult(new BattleResult
            {
                score = new[] { result.Stat.Score[mySlot], result.Stat.Score[1 - mySlot] }
            });
        }
    }

    private static void ApplyPlayerRecords(MyRttsInfo leagueInfo, int teamIndex, List<RttsPlayerGameRecord> players)
    {
        if (players == null) return;
        for (int i = 0; i < players.Count; i++)
        {
            RttsPlayerGameRecord player = players[i];
            if (player.PlayerIndex > 0)
            {
                leagueInfo.UpdateLeaguePlayerRecord(teamIndex, player.PlayerIndex, player.ToLeagueRecord());
            }
        }
    }

    private void SimulateHalfInning(
        SimTeam offense,
        int offenseSlot,
        int pitching,
        int inning,
        bool walkOffEnabled,
        RttsGameResult result,
        System.Random random)
    {
        int scoreBefore = result.Stat.Score[offenseSlot];
        int outs = 0;
        SimPlayer[] bases = new SimPlayer[3];

        while (outs < 3)
        {
            SimPlayer batter = offense.Players[offense.NextBatter % offense.Players.Count];
            offense.NextBatter = (offense.NextBatter + 1) % offense.Players.Count;
            RttsPlayerGameRecord record = batter.Data;
            record.Record[RttsPlayerGameRecord.PlateAppearance]++;

            TryStealBase(bases, offenseSlot, pitching, result, random);

            double roll = random.NextDouble();
            double walkRate = Clamp(0.075 + ((batter.Vision - 50) * 0.0011) - ((pitching - 50) * 0.00035), 0.025, 0.16);
            double homeRunRate = Clamp(0.024 + ((batter.Power - 50) * 0.00085) - ((pitching - 50) * 0.00020), 0.006, 0.12);
            double hitRate = Clamp(0.245 + ((batter.Contact - 50) * 0.00125) + ((batter.Power - 50) * 0.00015) - ((pitching - 50) * 0.00065), 0.16, 0.42);
            double tripleRate = Clamp(0.004 + ((batter.Speed - 50) * 0.00012), 0.002, 0.018);
            double doubleRate = Clamp(0.045 + ((batter.Power - 50) * 0.00030), 0.025, 0.085);

            if (roll < walkRate)
            {
                record.Record[RttsPlayerGameRecord.Walk]++;
                int runs = AdvanceWalk(bases, batter, result, offenseSlot);
                record.Record[RttsPlayerGameRecord.Rbi] += runs;
            }
            else
            {
                record.Record[RttsPlayerGameRecord.AtBat]++;
                double hitRoll = roll - walkRate;
                if (hitRoll < homeRunRate)
                {
                    AddHit(record, result, offenseSlot, RttsPlayerGameRecord.HomeRun);
                    int runs = AdvanceHit(bases, batter, 4, result, offenseSlot);
                    record.Record[RttsPlayerGameRecord.Rbi] += runs;
                }
                else if (hitRoll < homeRunRate + tripleRate)
                {
                    AddHit(record, result, offenseSlot, RttsPlayerGameRecord.Triple);
                    int runs = AdvanceHit(bases, batter, 3, result, offenseSlot);
                    record.Record[RttsPlayerGameRecord.Rbi] += runs;
                }
                else if (hitRoll < homeRunRate + tripleRate + doubleRate)
                {
                    AddHit(record, result, offenseSlot, RttsPlayerGameRecord.Double);
                    int runs = AdvanceHit(bases, batter, 2, result, offenseSlot);
                    record.Record[RttsPlayerGameRecord.Rbi] += runs;
                }
                else if (hitRoll < hitRate)
                {
                    AddHit(record, result, offenseSlot, -1);
                    int runs = AdvanceHit(bases, batter, 1, result, offenseSlot);
                    record.Record[RttsPlayerGameRecord.Rbi] += runs;
                }
                else
                {
                    double strikeOutRate = Clamp(0.18 - ((batter.Vision - 50) * 0.0010) + ((pitching - 50) * 0.0007), 0.08, 0.30);
                    if (random.NextDouble() < strikeOutRate)
                    {
                        record.Record[RttsPlayerGameRecord.StrikeOut]++;
                        result.Stat.StrikeOut[offenseSlot]++;
                    }
                    else if (random.NextDouble() < 0.018)
                    {
                        // 수비 실책 출루는 타수로 기록하지만 안타로 기록하지 않습니다.
                        result.Stat.Error[1 - offenseSlot]++;
                        AdvanceOnError(bases, batter, result, offenseSlot);
                        if (walkOffEnabled && result.Stat.Score[offenseSlot] > result.Stat.Score[1 - offenseSlot]) break;
                        continue;
                    }
                    outs++;
                }
            }

            if (walkOffEnabled && result.Stat.Score[offenseSlot] > result.Stat.Score[1 - offenseSlot])
            {
                break;
            }
        }

        result.Stat.InningScore[offenseSlot][inning] = result.Stat.Score[offenseSlot] - scoreBefore;
    }

    private static void AddHit(RttsPlayerGameRecord record, RttsGameResult result, int offenseSlot, int extraBaseRecord)
    {
        record.Record[RttsPlayerGameRecord.Hit]++;
        result.Stat.Hit[offenseSlot]++;
        if (extraBaseRecord >= 0) record.Record[extraBaseRecord]++;
        if (extraBaseRecord == RttsPlayerGameRecord.HomeRun) result.Stat.HomeRun[offenseSlot]++;
    }

    private static int AdvanceWalk(SimPlayer[] bases, SimPlayer batter, RttsGameResult result, int offenseSlot)
    {
        int runs = 0;
        if (bases[0] != null)
        {
            if (bases[1] != null)
            {
                if (bases[2] != null)
                {
                    ScoreRunner(bases[2], result, offenseSlot);
                    runs++;
                }
                bases[2] = bases[1];
            }
            bases[1] = bases[0];
        }
        bases[0] = batter;
        result.Stat.Walk[offenseSlot]++;
        return runs;
    }

    private static int AdvanceHit(SimPlayer[] bases, SimPlayer batter, int basesTaken, RttsGameResult result, int offenseSlot)
    {
        int runs = 0;
        for (int baseIndex = 2; baseIndex >= 0; baseIndex--)
        {
            SimPlayer runner = bases[baseIndex];
            bases[baseIndex] = null;
            if (runner == null) continue;

            int targetBase = baseIndex + basesTaken;
            if (targetBase >= 3)
            {
                ScoreRunner(runner, result, offenseSlot);
                runs++;
            }
            else
            {
                bases[targetBase] = runner;
            }
        }

        if (basesTaken >= 4)
        {
            ScoreRunner(batter, result, offenseSlot);
            runs++;
        }
        else
        {
            bases[basesTaken - 1] = batter;
        }
        return runs;
    }

    private static void AdvanceOnError(SimPlayer[] bases, SimPlayer batter, RttsGameResult result, int offenseSlot)
    {
        if (bases[2] != null) ScoreRunner(bases[2], result, offenseSlot);
        bases[2] = bases[1];
        bases[1] = bases[0];
        bases[0] = batter;
    }

    private static void ScoreRunner(SimPlayer runner, RttsGameResult result, int offenseSlot)
    {
        runner.Data.Record[RttsPlayerGameRecord.Run]++;
        result.Stat.Score[offenseSlot]++;
    }

    private static void TryStealBase(SimPlayer[] bases, int offenseSlot, int pitching, RttsGameResult result, System.Random random)
    {
        if (bases[0] == null || bases[1] != null) return;
        SimPlayer runner = bases[0];
        double attempt = Clamp(0.015 + ((runner.Speed - 50) * 0.0010), 0.0, 0.08);
        if (random.NextDouble() >= attempt) return;

        double success = Clamp(0.68 + ((runner.Speed - 50) * 0.004) - ((pitching - 50) * 0.001), 0.45, 0.92);
        if (random.NextDouble() < success)
        {
            bases[1] = runner;
            bases[0] = null;
            runner.Data.Record[RttsPlayerGameRecord.Steal]++;
            result.Stat.Steal[offenseSlot]++;
        }
    }

    private SimTeam BuildTeam(int teamIndex)
    {
        return teamIndex == RttsManager.MY_TEAM ? BuildMyTeam() : BuildCpuTeam(teamIndex);
    }

    private SimTeam BuildMyTeam()
    {
        var team = new SimTeam
        {
            TeamIndex = RttsManager.MY_TEAM,
            Name = KOBTextUtil.GetMyTeamName(),
            Pitching = DefaultAbility
        };

        MyDeckInfo deck = KOBManager.MyInfo.GameData.DeckInfo;
        if (deck?.LineupList != null)
        {
            foreach (KeyValuePair<int, KOBLineupInfo> pair in deck.LineupList.OrderBy(item => item.Key))
            {
                KOBBaller baller = KOBManager.MyInfo.GameData.GetBaller(pair.Value.idx);
                int level = baller?.level ?? 1;
                team.Players.Add(BuildPlayer(pair.Value.idx, level, pair.Key, pair.Value.position, false));
            }
        }

        int pitcherIndex = 0;
        int pitcherLevel = 1;
        if (deck?.RotationList != null && deck.RotationList.Count > 0)
        {
            KeyValuePair<int, int> pitcher = deck.RotationList.OrderBy(item => item.Key).First();
            pitcherIndex = pitcher.Value;
            pitcherLevel = KOBManager.MyInfo.GameData.GetPitcher(pitcherIndex)?.level ?? 1;
            team.Pitching = GetPitchingAbility(pitcherIndex, pitcherLevel);
        }

        // KOB의 RTTS 규칙과 동일하게 8명의 야수 다음 9번 타순에 투수를 둡니다.
        if (team.Players.Count < 9 && pitcherIndex > 0)
        {
            team.Players.Add(BuildPlayer(pitcherIndex, pitcherLevel, team.Players.Count + 1, (int)KOBPosition.Pitcher, true));
        }
        TrimLineup(team.Players);
        return team;
    }

    private SimTeam BuildCpuTeam(int teamIndex)
    {
        RttsTeam chart = KOBManager.Backend.Chart.RttsTeam.GetRttsTeam((_league * KOBConstant.TEAM_RECORD_UNIT) + teamIndex);
        var team = new SimTeam
        {
            TeamIndex = teamIndex,
            Name = chart?.Name ?? L10n.F("UI.TeamNumber", teamIndex),
            Pitching = DefaultAbility
        };
        if (chart == null) return team;

        int count = Math.Max(chart.Player?.Length ?? 0, chart.Pos?.Length ?? 0);
        count = Math.Min(9, count);
        for (int i = 0; i < count; i++)
        {
            int position = GetArrayValue(chart.Pos, i, (int)KOBPosition.DH);
            bool isPitcher = position == (int)KOBPosition.Pitcher;
            int playerIndex = isPitcher ? chart.Pitcher : GetArrayValue(chart.Player, i, 0);
            int level = GetArrayValue(chart.Level, i, 1);
            team.Players.Add(BuildPlayer(playerIndex, level, i + 1, position, isPitcher));
        }
        team.Pitching = GetPitchingAbility(chart.Pitcher, GetPitcherLevel(chart));
        TrimLineup(team.Players);
        return team;
    }

    private static void TrimLineup(List<SimPlayer> players)
    {
        if (players.Count > 9) players.RemoveRange(9, players.Count - 9);
        for (int i = 0; i < players.Count; i++) players[i].Data.BattingOrder = i + 1;
    }

    private SimPlayer BuildPlayer(int playerIndex, int level, int order, int position, bool isPitcher)
    {
        int contact = DefaultAbility;
        int power = DefaultAbility;
        int vision = DefaultAbility;
        int speed = DefaultAbility;
        int overall = DefaultAbility;

        if (!isPitcher)
        {
            HitterLevelData ability = KOBManager.Backend.Chart.HitterLevelData.GetData(playerIndex, level);
            if (ability != null)
            {
                contact = ability.contact;
                power = ability.power;
                vision = ability.vision;
                speed = ability.speed;
                overall = (ability.contact + ability.power + ability.vision + ability.fielding + ability.throwing + ability.speed) / 6;
            }
        }

        return new SimPlayer
        {
            Contact = contact,
            Power = power,
            Vision = vision,
            Speed = speed,
            IsPitcher = isPitcher,
            Data = new RttsPlayerGameRecord
            {
                PlayerIndex = playerIndex,
                BattingOrder = order,
                Position = position,
                Level = level,
                Overall = overall,
                Name = GetPlayerName(playerIndex)
            }
        };
    }

    private static string GetPlayerName(int playerIndex)
    {
        CharacterData character = KOBManager.Backend.Chart.CharacterData.GetData(playerIndex);
        if (character == null) return L10n.F("UI.PlayerNumber", playerIndex);
        string localized = KOBManager.Localization.GetUILocalizedValue2(character.name_id);
        return string.IsNullOrEmpty(localized) ? character.name_id : localized;
    }

    private static int GetPitchingAbility(int pitcherIndex, int level)
    {
        PitcherLevelData ability = KOBManager.Backend.Chart.PitcherLevelData.GetData(pitcherIndex, level);
        if (ability == null) return DefaultAbility;
        return (ability.control + ability.fastball + ability.curve + ability.slider + ability.sinker + ability.changeup) / 6;
    }

    private static int GetPitcherLevel(RttsTeam team)
    {
        if (team?.Pos == null || team.Level == null) return 1;
        int count = Math.Min(team.Pos.Length, team.Level.Length);
        for (int i = 0; i < count; i++)
        {
            if (team.Pos[i] == (int)KOBPosition.Pitcher) return team.Level[i];
        }
        return 1;
    }

    private int MakeSeed(int firstTeamIndex, int secondTeamIndex)
    {
        unchecked
        {
            int value = 17;
            value = (value * 31) + _league;
            value = (value * 31) + _playGame;
            value = (value * 31) + _leagueSeed;
            value = (value * 31) + firstTeamIndex;
            value = (value * 31) + secondTeamIndex;
            return value;
        }
    }

    private static int GetArrayValue(int[] values, int index, int defaultValue)
    {
        return values != null && index >= 0 && index < values.Length ? values[index] : defaultValue;
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
