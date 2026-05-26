using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    //[System.Serializable]
    public class QuickGameInfo
    {
        public SimulGameInfo gameInfo;
        //public int curBallCount, curStrikeCount;    //현재 볼카운트
        //public int curIndex;                        //공격팀 인덱스
        public int currentInning;                   //현재 이닝
        public bool bTopInning;                     //초이닝    
        public bool bInningEnd;
        public bool bGameEnd;
        public int curOutCount;
        //
        //public bool doublePlaySuccess;// = true;
        //public int hitterRunnerIndex;

        public int[] run = new int[2];
        //public int[] record = new int[4];
        //public string batterName;
        //public CPlayer batter, pitcher;
        


        public Dictionary<SkillUseStep, SimulSkillInfo> skillInfo;

        public bool MeaNoonShow;
        public bool HeosimShow;
        public bool MeahogShow;

        

    }
}

