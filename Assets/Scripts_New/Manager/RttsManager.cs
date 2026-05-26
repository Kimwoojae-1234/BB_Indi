using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    /// <summary>
    /// 이건 테스트 혹은 추가될수도 있는 자동 티켓으로 
    /// </summary>
    public void SimulMyGame(Action action)
    {
        RttsSchedule shedule = KOBManager.Backend.Chart.RttsSchedule.GetSchedule(ScheduleIndex);
        todayIsHome = (shedule.home == true);
        todaySchedule = GetSchedule(shedule.opponent);
        Debug.Log("RTTS 체크 ===============================>> 스케쥴 :" + todaySchedule.ToString() + "  홈팀 여부 :" + todayIsHome + " 내상대 : " + shedule.opponent);

        StartCoroutine(simulateMyGame(action));
    }

    private IEnumerator simulateMyGame(Action action)
    {
        
        yield return null;
        action?.Invoke();
    }


    /// <summary>
    /// 나의 게임을 제외한 다른 게임을 플레이 한후 정보 저장
    /// </summary>
    public void SimulOtherGames()
    {
        StartCoroutine(simulOtherGames());
    }


    private IEnumerator simulOtherGames()
    {
        bool isHome = todayIsHome;
        int league = League;

        for (int i = 1; i < todaySchedule.Count; i++)
        {
            int[] value = todaySchedule[i];
           
            yield return null;
            //저장
        }


        MyRewardList = null; //결과 화면 보기전 null로 초기화
        //임시로
        TRequestBattleEnd req = new TRequestBattleEnd()
        {
            League = this.League,
            Result = CurrentMyResult,// 
            myRecord = CurrentMyBallerRecord,//
            ballerIdx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller,
            GetXP = CurrentMyXP
        };

        KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
        {
            TResultBattleEnd res = (TResultBattleEnd)response;
            if (callback?.IsSuccess() == true && res?.isSuccess == true)
            {
                Debug.Log("RTTS 배틀엔드 완료");
                MyRewardList = res.RewardList;    //보상 정보 세팅해줌
            }
            else
            {
                int ErrorCode = res.ErrorCode;
                Debug.Log("에러코드 : " + ErrorCode);
            }
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
        });
    }


    public List<int[]> GetSchedule(int myOpponent)
    {
        var list = new List<int[]>();
        list.Add(new int[] { 0, myOpponent });
        bool[] checker = new bool[MAX_TEAM];

        //기본 체커
        checker[myOpponent] = true;
        checker[0] = true;


        int curOpp = myOpponent;
        for (int i = 1; i < MAX_TEAM; i++)
        {
            if (checker[i] == false)
            {
                while (true)
                {
                    int count = 0;
                    curOpp = (curOpp + 1) % MAX_TEAM;
                    if (checker[curOpp] == false &&
                        i != curOpp)
                    {
                        list.Add(new int[] { i, curOpp });
                        checker[curOpp] = true;
                        checker[i] = true;
                        if (list.Count >= 5)
                        {
                            return list;
                        }
                        break;
                    }
                    count++;
                    if (count >= 100) break; //안전코드 -> 나중에 지워
                }
            }
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

    public List<KOBRewardInfo> GetRttsRewardInfo(int Result)
    {
        List<KOBRewardInfo> kobRewardList = new List<KOBRewardInfo>();
        int win = CurrentWinDrawLose(0)[0];
        if (Result == 0)
        {
            int newWin = win + 1;
            if (RttsRewardList.ContainsKey(newWin) == true)
            {
                int[] value = RttsRewardList[newWin];
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
                else //if (_type == KOBReward.Card)
                {
                    //카드
                    KOBRewardInfo reward = new KOBRewardInfo(_type, value[1], value[2]);
                    kobRewardList.Add(reward);
                    return kobRewardList;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Rtts경기를 마치고 로비로 돌아가는
    /// </summary>
    public void BackToLobby()
    {
       
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