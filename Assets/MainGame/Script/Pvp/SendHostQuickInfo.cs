using BaseBall.BallPlay;
using System.Collections.Generic;

[System.Serializable]
public class SendHostQuickInfo
{
    //기본 quickInfo
    public sbyte result; //(SimulResultState)
    public sbyte fIndex;
    public sbyte hitType;//    (SimulHitType)  = battingData.hitType;
    public bool[] bRunnerActive = new bool[4];
    public sbyte[] runnerCurPos = new sbyte[4];
    public string[] runnerName = new string[4];
    public sbyte[] runnerLineup = new sbyte[4];
    public sbyte[] runnerValue = new sbyte[4];
    public sbyte stealState;

    //public SimulGameInfo gameInfo;
    public sbyte currentInning;                   //현재 이닝
    public bool bTopInning;
    public bool bInningEnd;
    public bool bGameEnd;
    public sbyte curOutCount;
    public sbyte[] run = new sbyte[2];

    //simul gameInfo
    public sbyte[,] inningScore = new sbyte[2, 12];
    public sbyte[] hit = new sbyte[2];
    public sbyte[] error = new sbyte[2];
    public sbyte[] pitchNum = new sbyte[2];

    //투수상태
    public sbyte stamina;
    public sbyte pinch;
    public sbyte pinchPoint;
    //교체상태
    public sbyte[] pitcherIndex = new sbyte[2];
    public sbyte[,] fielderIndex = new sbyte[2, 9];

    //스킬
    public bool MeaNoonShow;
    public bool HeosimShow;
    public bool MeahogShow;


    //스킬 딕셔너리
    public sbyte countNum;
    public sbyte[] skillStep;
    public bool[] bAvailable;
    public sbyte[] vsType;
    public int[] offenseID;
    public sbyte[] offenseRank;
    public int[] defenseID;
    public sbyte[] defenseRank;
    public int[] catcherID;
    public sbyte[] catcherRank;


    public void set(BallPlayManager manager, SimulBattingData Data, QuickGameInfo info, CPlayer pitcher)
    {
        //배팅 데이터
        result = (sbyte)Data.result; //(SimulResultState)
        fIndex = (sbyte)Data.fIndex;
        hitType = (sbyte)Data.hitType; //(SimulHitType)

        //주자동기화 정보
        for (int i = 0; i < 4; i++)
        {
            bRunnerActive[i] = Data.bRunnerActive[i];
            runnerCurPos[i] = (sbyte)Data.runnerCurPos[i];
            runnerName[i] = Data.runnerName[i];
            runnerLineup[i] = (sbyte)Data.runnerLineup[i];
            runnerValue[i] = (sbyte)Data.runnerValue[i];
        }
        //도루 정보
        stealState = (sbyte)Data.stealState;

        //퀵게임 인포
        currentInning = (sbyte)info.currentInning;
        bTopInning = info.bTopInning;
        bInningEnd = info.bInningEnd;
        bGameEnd = info.bGameEnd;
        if (bInningEnd == false)
        {
            curOutCount = (sbyte)info.curOutCount;
        }
        else
        {
            curOutCount = 0;
            for (int i = 0; i < 4; i++)
            {
                bRunnerActive[i] = false;
                runnerCurPos[i] = -1;
                runnerLineup[i] = -1;
                runnerValue[i] = -1;
            }
        }

        //gameInfo
        for (int i = 0; i < 2; i++)
        {
            run[i] = (sbyte)info.run[i];
            for (int j = 0; j < 12; j++) inningScore[i, j] = (sbyte)info.gameInfo.inningScore[i, j];
            hit[i] = (sbyte)info.gameInfo.hit[i];
            error[i] = (sbyte)info.gameInfo.error[i];
            pitchNum[i] = (sbyte)info.gameInfo.pitchNum[i];
        }

        //투수상태            
        stamina = (sbyte)pitcher.getCurrentStamina();
        pinch = (sbyte)pitcher.getPinchState();
        pinchPoint = (sbyte)pitcher.getPinchScore();

        //UnityEngine.//Debug.Log("=============>> bInningEnd : " + bInningEnd);
        //UnityEngine.//Debug.Log("=============>> 보내는 아웃카운트 : " + curOutCount);
        //UnityEngine.//Debug.Log("=============>> 보내는 stamina : " + stamina);
        //UnityEngine.//Debug.Log("=============>> 보내는 pinch : " + (PinchStep)pinch);

        //교체 상태
        pitcherIndex[0] = (sbyte)SimulPlayerManager.GetPitcherIndex(1);
        pitcherIndex[1] = (sbyte)SimulPlayerManager.GetPitcherIndex(0);
        for (int i = 0; i < 9; i++)
        {
            fielderIndex[0, i] = (sbyte)SimulPlayerManager.GetFielder(1, i).originLineup;
            fielderIndex[1, i] = (sbyte)SimulPlayerManager.GetFielder(0, i).originLineup;
        }

        //특수 스킬 플래그
        MeaNoonShow = info.MeaNoonShow;
        HeosimShow = info.HeosimShow;
        MeahogShow = info.MeahogShow;


        //스킬 딕셔너리
        countNum = (sbyte)info.skillInfo.Count;
        skillStep = new sbyte[countNum];
        bAvailable = new bool[countNum];
        vsType = new sbyte[countNum];
        offenseID = new int[countNum];
        offenseRank = new sbyte[countNum];
        defenseID = new int[countNum];
        defenseRank = new sbyte[countNum];
        catcherID = new int[countNum];
        catcherRank = new sbyte[countNum];

        int count2 = 0;
        foreach (KeyValuePair<SkillUseStep, SimulSkillInfo> value in info.skillInfo)
        {
            skillStep[count2] = (sbyte)value.Key;
            bAvailable[count2] = value.Value.bAvailable;
            vsType[count2] = (sbyte)value.Value.vsType;
            offenseID[count2] = (int)value.Value.offenseID;
            offenseRank[count2] = (sbyte)value.Value.offenseRank;
            defenseID[count2] = (int)value.Value.defenseID;
            defenseRank[count2] = (sbyte)value.Value.defenseRank;
            catcherID[count2] = (int)value.Value.catcherID;
            catcherRank[count2] = (sbyte)value.Value.catcherRank;

            count2++;
        }

    }
}


public enum ApplyInfo
{
    Init = 0,           //초기화
    SendReply = 1,      //게임정보 받음
    ChanceHost = 2,     //호스트의 찬스
    ChanceGuest = 3,     //게스트의 찬스
    ChanceAccept = 4,
    ChanceDecline = 5,
    HostChange = 6,      //호스트 교체 여부
    ReconnectAsked = 7,     //재연결 요구
    ReconnectDone = 8,     //재연결 완료
    ChangeWait = 9,        //선수교체 대기
    ChangeFinish = 10,     //선수교체 대기 종료        
    PitchSelect = 11,      //투수가 피치를 셀렉트 한경우 -> 상대 타자 교체 UI를 diable시킨다.
    PitchTimer = 12,       //투수 타이머 호출
    OtherForceDisconnect = 13,   //상대를 강제로 접속 종료시킴
    SkipAsk = 14,           //현재 상태에 대해 스킵 요청 
    GameEnd = 15,       //결과 상태임
}


[System.Serializable]
public class SendQuickGameReplyInfo
{
    //type 0 : 초기화
    //type 1 : 응답완료
    //type 2 :
    public int type;

    public void set(ApplyInfo _type)
    {
        type = (int)_type;
    }
}

[System.Serializable]
public class SendResultSync
{
    public bool bGameEnd;
    public sbyte[] gameScore = new sbyte[2];
    public sbyte[,] inningScore = new sbyte[2, 12];
    public sbyte[] hit = new sbyte[2];
    public sbyte[] error = new sbyte[2];
    public sbyte[] fourballCount = new sbyte[2];   //사구 카운트
    public sbyte[] strikeOutCount = new sbyte[2];  //삼진 카운트
    public sbyte[] homerunCount = new sbyte[2];    //홈런 카운트
    public sbyte[] dpCount = new sbyte[2];         //병살 카운트
    public sbyte[] stealCount = new sbyte[2];


    public sbyte[,] myBatter = new sbyte[14, 7];
    public sbyte[,] myPitcher = new sbyte[11, 8];
    public sbyte[,] cpuBatter = new sbyte[14, 7];
    public sbyte[,] cpuPatter = new sbyte[11, 8];
    public sbyte[,] pitcherAchieve = new sbyte[11, 2];

    public void set(BallPlayManager manager, bool bEnd)
    {
        bGameEnd = bEnd;
        //결과 동기화
        for (int i = 0; i < 2; i++)
        {
            int team = (1 - i);
            gameScore[i] = (sbyte)manager.nGameScore[team];
            for (int j = 0; j < 12; j++) inningScore[i, j] = (sbyte)manager.nInningScore[team, j];
            hit[i] = (sbyte)manager.nHitCount[team];
            error[i] = (sbyte)manager.nErrorCount[team];
            fourballCount[i] = (sbyte)manager.nFourballCount[team];
            strikeOutCount[i] = (sbyte)manager.nStrikeOutCount[team];
            homerunCount[i] = (sbyte)manager.nHomerunCount[team];
            dpCount[i] = (sbyte)manager.nDPCount[team];
            stealCount[i] = (sbyte)manager.nStealCount[team];

        }

        //타자 기록 동기화
        for (int i = 0; i < 14; i++)
        {
            CPlayer player = SimulPlayerManager.GetFielder(1, i);
            myBatter[i, 0] = (sbyte)player.getStat(Param.ST_AB);
            myBatter[i, 1] = (sbyte)player.getStat(Param.ST_H);
            myBatter[i, 2] = (sbyte)player.getStat(Param.ST_HR);
            myBatter[i, 3] = (sbyte)player.getStat(Param.ST_RBI);
            myBatter[i, 4] = (sbyte)player.getStat(Param.ST_SBS);
            myBatter[i, 5] = (sbyte)player.getStat(Param.ST_BB);
            myBatter[i, 6] = (sbyte)player.getStat(Param.ST_R);

            CPlayer player2 = SimulPlayerManager.GetFielder(0, i);
            cpuBatter[i, 0] = (sbyte)player2.getStat(Param.ST_AB);
            cpuBatter[i, 1] = (sbyte)player2.getStat(Param.ST_H);
            cpuBatter[i, 2] = (sbyte)player2.getStat(Param.ST_HR);
            cpuBatter[i, 3] = (sbyte)player2.getStat(Param.ST_RBI);
            cpuBatter[i, 4] = (sbyte)player2.getStat(Param.ST_SBS);
            cpuBatter[i, 5] = (sbyte)player2.getStat(Param.ST_BB);
            cpuBatter[i, 6] = (sbyte)player2.getStat(Param.ST_R);
        }

        //투수기록 동기화
        for (int i = 0; i < 11; i++)
        {
            //내선수
            CPlayer myplayer = SimulPlayerManager.GetPitcher(1, i);
            myPitcher[i, 0] = (sbyte)myplayer.getStat(Param.ST_IP);
            myPitcher[i, 1] = (sbyte)myplayer.getStat(Param.ST_PR);
            myPitcher[i, 2] = (sbyte)myplayer.getStat(Param.ST_PER);
            myPitcher[i, 3] = (sbyte)myplayer.getStat(Param.ST_PSO);
            myPitcher[i, 4] = (sbyte)myplayer.getStat(Param.ST_PH);
            myPitcher[i, 5] = (sbyte)myplayer.getStat(Param.ST_PBB);
            myPitcher[i, 6] = (sbyte)myplayer.getStat(Param.ST_PHR);
            myPitcher[i, 7] = (sbyte)myplayer.getStat(Param.ST_PNP);

            if (bGameEnd == true)
            {
                //투수 성적
                if (myplayer.getStat(Param.ST_PW) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 0] = (sbyte)Param.ST_PW;
                else if (myplayer.getStat(Param.ST_PL) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 0] = (sbyte)Param.ST_PL;
                else if (myplayer.getStat(Param.ST_HLD) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 0] = (sbyte)Param.ST_HLD;
                else if (myplayer.getStat(Param.ST_SV) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 0] = (sbyte)Param.ST_SV;
                else if (myplayer.getStat(Param.ST_BS) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 0] = (sbyte)Param.ST_BS;
            }

            //cpu선수
            CPlayer cpuplayer = SimulPlayerManager.GetPitcher(0, i);
            cpuPatter[i, 0] = (sbyte)cpuplayer.getStat(Param.ST_IP);
            cpuPatter[i, 1] = (sbyte)cpuplayer.getStat(Param.ST_PR);
            cpuPatter[i, 2] = (sbyte)cpuplayer.getStat(Param.ST_PER);
            cpuPatter[i, 3] = (sbyte)cpuplayer.getStat(Param.ST_PSO);
            cpuPatter[i, 4] = (sbyte)cpuplayer.getStat(Param.ST_PH);
            cpuPatter[i, 5] = (sbyte)cpuplayer.getStat(Param.ST_PBB);
            cpuPatter[i, 6] = (sbyte)cpuplayer.getStat(Param.ST_PHR);
            cpuPatter[i, 7] = (sbyte)cpuplayer.getStat(Param.ST_PNP);

            if (bGameEnd == true)
            {
                //투수 성적
                if (cpuplayer.getStat(Param.ST_PW) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 1] = (sbyte)Param.ST_PW;
                else if (cpuplayer.getStat(Param.ST_PL) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 1] = (sbyte)Param.ST_PL;
                else if (cpuplayer.getStat(Param.ST_HLD) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 1] = (sbyte)Param.ST_HLD;
                else if (cpuplayer.getStat(Param.ST_SV) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 1] = (sbyte)Param.ST_SV;
                else if (cpuplayer.getStat(Param.ST_BS) == Param.P_ACHIEVE_COMPLETE) pitcherAchieve[i, 1] = (sbyte)Param.ST_BS;
            }
        }

    }
}



[System.Serializable]
public class SendChangePlayerSync
{
    //내팀여부
    public bool bMyTeam;
    //교체 타입
    public int type;
    //교체 아웃되는 선수 인덱스
    public int outIndex;
    //교체 인 되는 선수 인덱스
    public int inIndex;
    //필더 체인지인경우 현재 포지션, 주자 교체 경우 베이스 위치
    public int index;


    public void set(bool _bMyTeam, UIPlayerChange.PlayerChangeType _type, int _outIndex, int _inIndex, int _index)
    {
        bMyTeam = _bMyTeam;
        type = (int)_type;
        outIndex = _outIndex;
        inIndex = _inIndex;
        index = _index;
    }
}