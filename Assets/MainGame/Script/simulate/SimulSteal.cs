using UnityEngine;
using System.Collections;
using WebConnector;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class SimulSteal
    {
        //도루 발생
        public static int STEAL_INVOKE_CONST = 10;
        public static float STEAL_BASIC_INVOKE_2B = 0.2f; //원래 0.1
        public static float STEAL_BASIC_INVOKE_3B = 0.05f;
        public static float STEAL_INVOKE_MIN_2B = 0.01f;  //0.05f;
        public static float STEAL_INVOKE_MAX_2B = 0.45f;   // 0.2f;
        public static float STEAL_INVOKE_MIN_3B = 0.0f;
        public static float STEAL_INVOKE_MAX_3B = 0.1f;

        //도루 성공
        public static int STEAL_SUCCESS_CONST_CATCHER = 45;
        public static int STEAL_SUCCESS_CONST_RUNNER = 15;
        public static float STEAL_BASIC_SUCCESS_1B = 0.7f;
        public static float STEAL_BASIC_SUCCESS_2B = 0.5f;
        public static float STEAL_SUCCESS_MIN = 0.05f;
        public static float STEAL_SUCCESS_MAX = 0.9f;

        //견제 발생
        public static int PICKOFF_INVOKE_CONST = 10;
        public static float PICKOFF_BASIC_INVOKE = 0.3f;
        public static float PICKOFF_INVOKE_MIN = 0.05f;
        public static float PICKOFF_INVOKE_MAX = 0.5f;

        //견제 성공
        public static int PICKOFF_SUCCESS_CONST = 15;
        public static float PICKOFF_BASIC_SUCCESS = 0.15f;
        public static float PICKOFF_SUCCESS_MIN = 0.01f;
        public static float PICKOFF_SUCCESS_MAX = 0.3f;


        //도루 관련 스킬
        public static FieldSkillUse catcherSitThrow;    //포수의 앉아쏴
        public static FieldSkillUse runnerStealMarster; //주자의 대도

        //견제 관련 스킬
        //public static FieldSkillUse pitcherLaserPickoff;//피처의 광속견제
        //public static FieldSkillUse runnerRead;         //주자의 리드

        ///////////////////////////////////////////////////////////////////////
        //도루 작전
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 도루가 가능한지 여부
        /// </summary>
        /// <param name="inning"></param>
        /// <param name="outCount"></param>
        /// <param name="scoreGab"></param>
        /// <param name="bOnBase"></param>
        /// <returns></returns>
        public static bool checkStealPossible(CPlayer runner, CPlayer catcher, int inning, int outCount, int scoreGab, bool[] bOnBase)
        {
            if (bOnBase[SimulParm.FIRSTBASE_INDEX] == true || bOnBase[SimulParm.SECONDBASE_INDEX] == true)
            {
                if (bOnBase[SimulParm.SECONDBASE_INDEX] == true && bOnBase[SimulParm.THIRDBASE_INDEX] == true)
                {
                    //////Debug.Log("============>> 2루 3루 동시에 주자가 있어서 도루 불가능");
                    return false;
                }

                if (scoreGab >= 6)
                {
                    //6점이상 리드
                    return false;
                }
                if (inning == 9 && outCount == 2)
                {
                    //9회 투아웃
                    return false;
                }

                if (inning >= 6 && scoreGab >= 4)
                {
                    //6회이상 4점이상 리드
                    return false;
                }

                if (inning >= 7 && Mathf.Abs(scoreGab) <= 1)
                {
                    //7회이상 1점차 박빙
                    return false;
                }

                if (checkStealInvoke(runner, catcher, bOnBase[SimulParm.SECONDBASE_INDEX]) == true)
                {
                    ////Debug.Log("==========================================================>>도루발생");
                    return true;
                }
            }
            return false;
        }


        public static bool checkStealInvoke(CPlayer runner, CPlayer catcher, bool bOnSecondBase)
        {
#if _Local_Balance
            if (InGameDebug._ALWAYS_STEAL == true)
            {
                return true;
            }
#endif
            int BminusC = (runner.getSpeed() / 10) - catcher.getDefenseAvg();
            float invokeRate;
            ////Debug.Log("================>>주자 2루에 있음 : " + bOnSecondBase);            
            if (bOnSecondBase == true)
            {
                //3루도루
                invokeRate = STEAL_BASIC_INVOKE_3B + (BminusC * STEAL_INVOKE_CONST * 0.001f);
                //Debug.Log("BminusC = " + BminusC + "=====================>>원래 도루 발생확률 : " + invokeRate);
                invokeRate = Mathf.Clamp(invokeRate, STEAL_INVOKE_MIN_3B, STEAL_INVOKE_MAX_3B);
            }
            else
            {
                //2루도루
                invokeRate = STEAL_BASIC_INVOKE_2B + (BminusC * STEAL_INVOKE_CONST * 0.001f);
                //Debug.Log("BminusC = " + BminusC + "=====================>>원래 도루 발생확률 : " + invokeRate);
                invokeRate = Mathf.Clamp(invokeRate, STEAL_INVOKE_MIN_2B, STEAL_INVOKE_MAX_2B);
            }
            float range = MyMath.PercentF();
            ////Debug.Log("=====================>>실 도루 발생확률 : " + invokeRate + "======>>> range = " + range);
            if (range < invokeRate)
            {
                ////Debug.Log("==========================================================>>도루발생");
                return true;
            }
            ////Debug.Log("==========================================================>>도루안함");
            return false;

        }

        
        /// <summary>
        /// 도루결과 산출 새버전
        /// </summary> 
        public static SimulStealState getStealResult(CPlayer runner, CPlayer catcher, CPlayer pitcher, bool bThirdBase)
        {

#if _Local_Balance
            if (InGameDebug._STEAL_RESULT_TEST == true)
            {
                if (MyMath.Percent() < 50) return SimulStealState.Success;
                else return SimulStealState.Fail;
            }
#endif
            //특수능력 초기화
            catcherSitThrow = FieldSkillUse.Init;
            runnerStealMarster = FieldSkillUse.Init;

            //성공범위
            int BminusC = (runner.getSpeed() / 10) - catcher.getDefenseAvg();
           
            float basicSuccess = (bThirdBase ? STEAL_BASIC_SUCCESS_2B : STEAL_BASIC_SUCCESS_1B);
            float successRange = basicSuccess;
            if (BminusC <= 0)
            {
                //포수우위
                successRange += (BminusC * STEAL_SUCCESS_CONST_CATCHER * 0.001f);
            }
            else
            {
                //주자우위
                successRange += (BminusC * STEAL_SUCCESS_CONST_RUNNER * 0.001f);
            }

            float reduceRate = pitcher.getSkillScopeRate(SkillIndex.PitcherQuickMotion) * 0.01f;
            ////Debug.Log("==============>>BminusC = " + BminusC + " 원래값 =======>>>successRange = " + successRange + "============>> 퀵모션에 의한 감소 = " + reduceRate);
            //public static float STEAL_SUCCESS_MIN = 0.05f;
            //public static float STEAL_SUCCESS_MAX = 0.9f;
            successRange = Mathf.Clamp((successRange - reduceRate), STEAL_SUCCESS_MIN, STEAL_SUCCESS_MAX);
            ////Debug.Log("==============>>BminusC = " + BminusC + " 실제값 =======>>>successRange = " + successRange);
            
                        
            //앉아쏴 체크
            if (catcher.fieldSkillSuccess(SkillIndex.CatcherSitThrow) == true && MyMath.Half() == true)
            {
                ////Debug.Log("========================================>>앉아쏴 발동!!");
                catcherSitThrow = FieldSkillUse.Active;
            }
            
            //대도 체크
            if (runner.fieldSkillSuccess(SkillIndex.RunnerStealMaster) == true && MyMath.Half() == true)
            {
                ////Debug.Log("========================================>>대도 발동!!");
                runnerStealMarster = FieldSkillUse.Active;
            }

            if (catcherSitThrow == FieldSkillUse.Active && runnerStealMarster == FieldSkillUse.Active)
            {
                // VS
                ////Debug.Log("========================================>>도루 스킬 대결 발동");
                return SimulStealState.VsSkill;
            }
            else
            {
                if (catcherSitThrow == FieldSkillUse.Active)
                {
                    //앉아쏴 발동성공
                    return SimulStealState.Fail_Skill;
                }
                else if (runnerStealMarster == FieldSkillUse.Active)
                {
                    //대도 발동성공
                    return SimulStealState.Success_Skill;
                }
                else
                {
                    float range = MyMath.PercentF();
                    ////Debug.Log("========================================>>range = " + range);
                    if (range < successRange)
                    {
                        ////Debug.Log("========================================>>일반 도루 성공!!");
                        return SimulStealState.Success;
                    }
                }
            }            

            ////Debug.Log("========================================>>도루 실패!!");
            return SimulStealState.Fail;
        }


        /// <summary>
        /// 딜레이드 스킬의 새버전
        /// </summary>         
        public static bool getHomeStealResult(CPlayer curFielder, CPlayer curRunner)
        {            
            return false;
        }




        ///////////////////////////////////////////////////////////////////////
        //견제 작전
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 견제 발생여부 (true인 경우 아웃이 될 수 있는 견제를 발생시킨다)
        /// 일반 견제시만 사용
        /// </summary>         
        public static SimulPickOffState getPickOffResult(CPlayer runner, CPlayer pitcher, int count)
        {
            if (count < 2)
            {
                bool bLaserPickOffHappen = pitcher.fieldSkillSuccess(SkillIndex.LaserPickOff);                

                if (bLaserPickOffHappen == true)
                {
                    ////Debug.Log("=============================>> 레이저 견제 발생");
                    bool bLeadHappen = runner.fieldSkillSuccess(SkillIndex.RunnerLead);
                    if (bLeadHappen == true)
                    {
                        ////Debug.Log("=============================>> 리드와 대결");
                        return SimulPickOffState.VsSkill;
                    }
                    else
                    {
                        ////Debug.Log("=============================>> 레이저 견제 성공");
                        return SimulPickOffState.LaserPickOff;
                    }
                }
                else
                {
                    int PminusB = pitcher.getPitcherAvg() - (runner.getSpeed() / 10);
                    float invokeRate = PICKOFF_BASIC_INVOKE + (PminusB * PICKOFF_INVOKE_CONST * 0.001f);
                    //Debug.Log("PminusB = " + PminusB + "=================>> 원래 발동률 invokeRate = " + invokeRate);

                    invokeRate = Mathf.Clamp(invokeRate, PICKOFF_INVOKE_MIN, PICKOFF_INVOKE_MAX);
                    float range = MyMath.PercentF();
                    //Debug.Log("실제 발동률 invokeRate = " + invokeRate + " =========>> range = "+range);

                    if (range < invokeRate)
                    {
                        ////Debug.Log("=============================>> 견제 발생");
                        return pickOffSuccess(runner, pitcher);
                    }
                }
            }
            return SimulPickOffState.NONE;
        }


        /// <summary>
        /// 견제 발생여부 (true인 경우 아웃이 될 수 있는 견제를 발생시킨다)
        /// 일반 견제시만 사용
        /// </summary>         
        public static SimulPickOffState getPickOffResultMyControl(CPlayer runner, CPlayer pitcher, SimulStealState steal)
        {
            if (steal != SimulStealState.NONE)
            {
                bool bLaserPickOffHappen = pitcher.fieldSkillSuccess(SkillIndex.LaserPickOff);

                if (bLaserPickOffHappen == true)
                {
                    ////Debug.Log("=============================>> 레이저 견제 발생");
                    bool bLeadHappen = runner.fieldSkillSuccess(SkillIndex.RunnerLead);
                    if (bLeadHappen == true)
                    {
                        ////Debug.Log("=============================>> 리드와 대결");
                        return SimulPickOffState.VsSkill;
                    }
                    else
                    {
                        ////Debug.Log("=============================>> 레이저 견제 성공");
                        return SimulPickOffState.LaserPickOff;
                    }
                }
                else
                {   
                    return pickOffSuccess(runner, pitcher);
                }
            }
            return SimulPickOffState.NONE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="pitcher"></param>
        /// <returns></returns>
        public static SimulPickOffState pickOffSuccess(CPlayer runner, CPlayer pitcher)
        {
            bool bLeadHappen = runner.fieldSkillSuccess(SkillIndex.RunnerLead);
            if (bLeadHappen == true)
            {
                ////Debug.Log("=================================================>> 리드 능력으로 견제 회피");
                return SimulPickOffState.LeadDefense;
            }

            int PminusB = pitcher.getPitcherAvg() - (runner.getSpeed() / 10);
            float successRate = PICKOFF_BASIC_SUCCESS + (PminusB * PICKOFF_SUCCESS_CONST * 0.001f);

            //Debug.Log("PminusB = "+PminusB + "============>> 원래 견제 성공률 : " + successRate);

            successRate = Mathf.Clamp(successRate, PICKOFF_SUCCESS_MIN, PICKOFF_SUCCESS_MAX);

            float range = MyMath.PercentF(); 

            ////Debug.Log("============>> 진짜 견제 성공률 : " + successRate + "==============>> range = "+range);

            if (range < successRate)
            {
                ////Debug.Log("=================================================>> 진짜 견제 성공");
                return SimulPickOffState.Success;
            }

            return SimulPickOffState.Fail;
        }

    }
}