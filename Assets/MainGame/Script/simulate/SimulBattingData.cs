using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace BaseBall.BallPlay
{
    //[System.Serializable]
    public class SimulBattingData
    {
        public int index;                
        //카운트 관련
        public int curBallCount, curStrikeCount;
        public int pitchNum;
        //타구질 관련
        public SimulHitType hitType;
        //public SpecificFlyType flyType;
        //public SpecificGrounderType grounderType;
        //public SpecificLinerType linerType;
        public SpecificBuntType buntResultType;
        //public ErrorType errorType;
        public SimulResultState result;
        public int fIndex;    //처리야수   
        //public int fIndex2; //처리 야수
        //public int throwBase;
        public BattingTiming bTimingResult;
        public BattingContact bContactResult;
        //피칭관련
        //public int pGujong;
        //public bool bStrike;
        //public bool bCheckSwing;
        //public SimulResultByPitch pitchResult; //피칭의 결과
        //public ControlValue controlValue;

        //주자 싱크관련
        //public int[] runnerValue = new int[4];
        //public int[] runnerCurPos = new int[4];
        //public int[] runnerRunningOut = new int[4];
#if !_Test_Local
        public long[] runnerSeq = new long[4];
#endif
        public string[] runnerName = new string[4];
        public bool[] bRunnerActive = new bool[4];
        public int[] runnerCurPos = new int[4];
        public int[] runnerLineup = new int[4];
        public int[] runnerValue = new int[4];

        //도루,주루 관련
        public SimulStealState stealState;
        //public bool bStealTry;
        //public bool bStealSuccess;
        //public bool bDelayStealTry;
        //public bool bDelayStealSuccess;
        //public int doublePlayStep;      //0:없음, 1:더블플레이 2:홈->1루 더블플레이 3:병살저지 스킬발동

        //qInfo
        //public QuickGameInfo qInfo = null;
    }

    public class SimulCurrentPlayerData
    {
        public int index;
        public long curBatterSeq, curPitcherSeq;
        public long[] fielderSeq = new long[9];
        public int curRunnerIndex;

    }

    public class RewindData
    {
        public List<SimulBattingData> rewindData = new List<SimulBattingData>();
        public List<SimulCurrentPlayerData> rewindPlayerData = new List<SimulCurrentPlayerData>();
    }
}