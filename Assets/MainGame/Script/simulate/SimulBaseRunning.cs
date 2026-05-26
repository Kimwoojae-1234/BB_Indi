using UnityEngine;
using System.Collections;
using WebConnector;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class SimulBaseRunning
    {
        private const int SAFE_OVERRUN = 40;
        private const int DANGER_OVERRUN = 7;

        private const float SAFE_OVERRUN_OUT = 0.1f;
        private const float DANGER_OVERRUN_OUT = 0.4f;

        ///////////////////////////////////////////////////////////////////////
        //주루: 오버런 발생
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 오버런 체크
        /// </summary> 
        public static SimulOverrunState checkGetOneMoreBase(CPlayer runner, CPlayer fielder, bool bSafeCase)
        {
#if _Local_Balance
            if (InGameDebug._ALWAYS_OVERRUN == true)
            {
                //오버런 테스트용
                return SimulOverrunState.SAFE;
            }
            else
#endif
            {
                ////Debug.Log("================================================================>> 오버런 체크");
                int per = MyMath.Percent();

                if (per < (bSafeCase ? SAFE_OVERRUN : DANGER_OVERRUN))
                {
                    /*float defaultPer = (bSafeCase ? SAFE_OVERRUN_OUT : DANGER_OVERRUN_OUT);

                    int B = runner.getSpeed() / 10;
                    int P = fielder.getThrowing() / 10;
                    int S = 10;

                    float F = defaultPer + ((P - B) * S * 0.001f);
                    ////Debug.Log("================================================================>> 최종 주루사 확률 : "+defaultPer);

                    if (MyMath.PercentF() < F)
                    {
                        //주루사함
                        ////Debug.Log("================================================================>> 주루사가 발생!!!!!!!!!!");
                        return SimulOverrunState.OUT;
                    }
                    else
                    {
                        //오버런 성공
                        ////Debug.Log("================================================================>> 한베이스 더가 발생!!!!!!!!!!");
                        return SimulOverrunState.SAFE;
                    }*/
                    //pvp에서는 무조건 오버런 성공
                    return SimulOverrunState.SAFE;
                }
                //주루사가 발생하지 않는다
                return SimulOverrunState.NONE;
            }
        }

        /// <summary>
        /// 시뮬에서 오버런 체크
        /// </summary> 
        /// 
        public static SimulOverrunState checkGetOneMoreBaseSimul(CPlayer runner, CPlayer fielder, FlyCatchType catchType)
        {
            SimulOverrunState overrun = SimulOverrunState.NONE;

            if (catchType == FlyCatchType.CATCH_ERROR ||
                catchType == FlyCatchType.BOUND_ERROR ||
                catchType == FlyCatchType.BOUND_ERROR)
            {
                //에러시 무조건 1베이스 추가
                return SimulOverrunState.SAFE;
            }

            int range = MyMath.Percent();
            int limit = 30;
            if (catchType == FlyCatchType.SideBound)
            {
                limit += 15;
            }
            else if (catchType == FlyCatchType.FenceBound)
            {
                limit += 40;
            }
            else if (catchType == FlyCatchType.NormalBound)
            {
                limit -= 5;
            }
            else if (catchType == FlyCatchType.DashBound)
            {
                limit -= 15;
            }

            //limit = 1000;   //지워지워 -> 레이저 발생 테스트용

            if (range < limit)
            {
                bool bSafeCase = MyMath.Percent() < 35 ? true : false;      //35% 확률로 안전오버런
                overrun = checkGetOneMoreBase(runner, fielder, bSafeCase);  
                if (overrun != SimulOverrunState.NONE)
                {
                    //스킬 체크
                    if (fielder.fieldSkillSuccess(SkillIndex.Laser) == true && MyMath.Percent() < 30)
                    {
                        //레이저 발동시
                        if (runner.fieldSkillSuccess(SkillIndex.RunnerSliding) == true)
                        {
                            //카운터 슬라이딩 (질주본능)
                            int oRank = runner.getSkillRank(SkillIndex.RunnerSliding); //슬라이딩
                            int dRank = fielder.getSkillRank(SkillIndex.Laser);        //레이저
                            bool bOffenseWin = SimulParm.checkOffenseSkillWin(oRank, dRank);
                            overrun = bOffenseWin ? SimulOverrunState.VsSafe : SimulOverrunState.VsOut;
                        }
                        else
                        {
                            overrun = SimulOverrunState.LaserOut;
                        }
                    }
                }
            }

            return overrun;
        }

        
    }
}
