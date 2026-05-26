using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using LitJson;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class MyRttsInfo
{
    [JsonProperty] public int League { get; private set; } = 0;    //리그
    [JsonProperty] public int LastLeague { get; private set; } = -1; //이전리그
    [JsonProperty] public int Seed { get; private set; }       //시드
    [JsonProperty] public int PlayGame { get; private set; } = -1;//      //시드
    [JsonProperty] public int PlayoffStep { get; private set; }    //플레이오프 스텝
    [JsonProperty] public int RewardAcquisitionStep { get; private set; } //보상획득 스텝


    [JsonProperty] public Dictionary<int, TeamRecord> LeagueTeamRecord { get; private set; }
    [JsonProperty] public Dictionary<int, BatterRecord> LeaguePlayerRecord { get; private set; }
    [JsonProperty] public Dictionary<int, BattleResult> LeagueResult { get; private set; }

    public MyRttsInfo()
    {
        InitLeague();
    }

    public MyRttsInfo(JsonData json)
    {
        if (json.ContainsKey("League")) League = int.Parse(json["League"].ToString());
        if (json.ContainsKey("LastLeague")) LastLeague = int.Parse(json["LastLeague"].ToString());        
        if (json.ContainsKey("Seed")) Seed = int.Parse(json["Seed"].ToString());
        if (json.ContainsKey("PlayGame")) PlayGame = int.Parse(json["PlayGame"].ToString());
        if (json.ContainsKey("PlayoffStep")) PlayoffStep = int.Parse(json["PlayoffStep"].ToString());
        if (json.ContainsKey("RewardAcquisitionStep")) RewardAcquisitionStep = int.Parse(json["RewardAcquisitionStep"].ToString());        

        if (json.ContainsKey("LeagueTeamRecord"))
        {
            LeagueTeamRecord = KOBTableUtil.DeserializeDictionary<int, TeamRecord>(json["LeagueTeamRecord"],
                                                                keyStr => int.Parse(keyStr),
                                                                json => JsonHelper.DeserializeObject<TeamRecord>(json.ToJson()));
        }

        if (json.ContainsKey("LeaguePlayerRecord"))
        {
            LeaguePlayerRecord = KOBTableUtil.DeserializeDictionary<int, BatterRecord>(json["LeaguePlayerRecord"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<BatterRecord>(json.ToJson()));
        }

        if (json.ContainsKey("LeagueResult"))
        {
            LeagueResult = KOBTableUtil.DeserializeDictionary<int, BattleResult>(json["LeagueResult"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<BattleResult>(json.ToJson()));
        }
    }

    public void InitLeague()
    {
        League = 0; //리그 0부터 시작해야
        LastLeague = -1;
                
        PlayGame = -1;
        PlayoffStep = 0;
        RewardAcquisitionStep = 0;

        LeagueTeamRecord = new Dictionary<int, TeamRecord>();
        LeagueTeamRecord.Clear();

        LeaguePlayerRecord = new Dictionary<int, BatterRecord>();
        LeaguePlayerRecord.Clear();

        LeagueResult = new Dictionary<int, BattleResult>();
        LeagueResult.Clear();
    }

    public void StartLeague()
    {
        PlayGame = 0;
        Seed = UnityEngine.Random.Range(0, 36); //스케쥴을 랜덤하게 보여주기 위한 장치
    }

    public bool BattleEnd()
    {
        PlayGame++;        

        return true;
    }


    /// <summary>
    /// 리그가 업데이트 될때
    /// </summary>
    public bool UpdateLeague(bool isNextLeagueExist)
    {        
        LastLeague = League;
        if (isNextLeagueExist == true)
        {
            League++;
        }
        Seed = UnityEngine.Random.Range(0, 36);
        PlayGame = -1;
        RewardAcquisitionStep = 0;
        LeagueTeamRecord.Clear();
        LeaguePlayerRecord.Clear();
        LeagueResult.Clear();

        return true;    
    }


    public void UpdateLeagueTeamRecord(int team, int myScore, int oppScore)
    {
        if(LeagueTeamRecord.ContainsKey(team) == true)
        {
            LeagueTeamRecord[team].SetRecord(myScore, oppScore);
        }
        else
        {
            TeamRecord record = new TeamRecord();
            record.SetRecord(myScore, oppScore);
            LeagueTeamRecord.Add(team, record);
        }
    }



    public void UpdateLeaguePlayerRecord(int team, int idx, BatterRecord addRecord)
    {
        int playerIdx = team * KOBConstant.PLAYER_RECORD_UNIT + idx;

        if (LeaguePlayerRecord.ContainsKey(playerIdx) == true)
        {
            LeaguePlayerRecord[playerIdx].SetRecord(addRecord);
        }
        else
        {
            LeaguePlayerRecord.Add(playerIdx, addRecord);
        }
    }

    public void UpdateLeagueResult(BattleResult result)
    {
        int idx = KOBManager.Rtts.PlayGame;
        if (LeagueResult.ContainsKey(idx) == true)
        {
            LeagueResult[idx] = result;
        }
        else
        {
            LeagueResult.Add(idx, result);
        }
    }


    public void InitLeaguePlayerRecord()
    {
        IReadOnlyDictionary<int, RttsTeam> teamChart = KOBManager.Backend.Chart.RttsTeam.Dictionary;

        
        //내팀 세팅
        List<int> MyPlayer = new List<int>();
        foreach (KeyValuePair<int, KOBLineupInfo> lineup in KOBManager.MyInfo.GameData.DeckInfo.LineupList)
        {
            MyPlayer.Add(lineup.Value.idx); //라인업에 있는 타자
        }
        foreach (KeyValuePair<int, int> rotation in KOBManager.MyInfo.GameData.DeckInfo.RotationList)
        {
            MyPlayer.Add(rotation.Value); //라인업에 있는 투수
        }
        for (int player = 0; player < MyPlayer.Count; player++)
        {
            int PlayerIndex = MyPlayer[player]; //(team : 팀인덱스 / player 플레이어 인덱스) -> 동일선수를 팀으로 구분함
            LeaguePlayerRecord.Add(PlayerIndex, new BatterRecord());
        }

        //기타 팀 세팅
        for (int team = 1; team < 10; team++)
        {
            int team_idx = League * KOBConstant.TEAM_RECORD_UNIT + team;
            int [] Player = teamChart[team_idx].Player;
            for(int player=0; player < Player.Length; player++)
            {
                int PlayerIndex = (team * KOBConstant.PLAYER_RECORD_UNIT) + Player[player]; //(team : 팀인덱스 / player 플레이어 인덱스) -> 동일선수를 팀으로 구분함 10000단위
                LeaguePlayerRecord.Add(PlayerIndex, new BatterRecord());
            }
        }
    }
}

[Serializable]
public class BatterRecord
{
    public short Game;  //출전게임수
    //public short PA;    //타수
    public short AB;    //타석
    //public short PlayerR;     //득점
    public short H;     //안타
    public short H2;    //2루타
    public short H3;    //3루타
    public short HR;    //홈런
    public short RBI;   //타점
    //public short PlayerSB;    //도루
    public short BB;    //볼넷
    //public short PlayerSO;    //삼진

    public BatterRecord()
    {
        Reset();
    }

    public void Reset()
    {        
        Game = 0;
        //PA = 0;
        AB = 0;
        //R = 0;
        H = 0;
        H2 = 0;
        H3 = 0;
        HR = 0;
        RBI = 0;
        //SB = 0;
        BB = 0;
    }

    public void SetRecord(BatterRecord record)
    {
        Game += record.Game;
        //PA = 0;
        AB += record.AB;
        //R = 0;
        H += record.H;
        H2 += record.H2;
        H3 += record.H3;
        HR += record.HR;
        RBI += record.RBI;
        //SB = 0;
        BB += record.BB;
    }
}

[Serializable]
public class TeamRecord
{
    public short Win;
    public short Draw;
    public short Lose;

    public TeamRecord()
    {
        Reset();
    }



    public void Reset()
    {
        Win = 0;
        Draw = 0;
        Lose = 0;
    }

    public void SetRecord(int myScore, int oppScore)
    {
        if (myScore > oppScore) Win++;
        else if (myScore < oppScore) Lose++;
        else Draw++; 
    }
}

[Serializable]
public class BattleResult
{
    public int[] score;
}