using UnityEngine;
using System.Collections;
using WebConnector;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class SimulBunt
    {
        ///////////////////////////////////////////////////////////////////////
        //번트 상수
        ///////////////////////////////////////////////////////////////////////
        public static int BUNT_INVOKE_CONSTANT_PITCHERWIN = 30;
        public static int BUNT_INVOKE_CONSTANT_BATTERWIN = 15;
        public static int BUNT_SUCCESS_CONSTANT = 30;
        
        public static float SQUEEZE_BASIC_INVOKE = 0.15f;
        public static float SACRIFY_BASIC_INVOKE = 0.30f;
        public static float DRAG_BASIC_INVOKE = 0.10f;

        public static float SQUEEZE_BASIC_SUCCESS = 0.5f;
        public static float SACRIFY_BASIC_SUCCESS = 0.8f;
        public static float DRAG_BASIC_SUCCESS = 0.3f;

        public static float SQUEEZE_MIN = 0.05f;
        public static float SQUEEZE_MAX = 0.8f;
        public static float SACRIFY_MIN = 0.3f;
        public static float SACRIFY_MAX = 0.95f;
        public static float DRAG_MIN = 0.05f;
        public static float DRAG_MAX = 0.5f;


        ///////////////////////////////////////////////////////////////////////
        //번트의 필수 기본 조건
        ///////////////////////////////////////////////////////////////////////        

        /// <summary>
        /// 스퀴즈 번트 기본 조건 체크
        /// </summary>
        public static bool checkSqueezeBuntCase(int outCount, int scoreGab, bool[] bOnBase)
        {
            if (outCount < 2 && Mathf.Abs(scoreGab) < 3) //2아웃미만, 점수 3점차 이내
            {
                if (bOnBase[SimulParm.THIRDBASE_INDEX] == true) //주자가 3루에 있으면서
                {
                    if (bOnBase[SimulParm.SECONDBASE_INDEX] == false || bOnBase[SimulParm.FIRSTBASE_INDEX] == false) //만루가 아닌경우
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        


        /// <summary>
        /// 희생번트 기본 조건 체크
        /// </summary>                
        public static bool checkSacBuntCase(int outCount, int scoreGab, bool[] bOnBase)
        {
            if (outCount == 0 && Mathf.Abs(scoreGab) < 6) //노아웃, 점수 6점차 이내
            {
                if (bOnBase[SimulParm.THIRDBASE_INDEX] == false) //주자가 3루에 없고
                {
                    if (bOnBase[SimulParm.FIRSTBASE_INDEX] == true || bOnBase[SimulParm.SECONDBASE_INDEX] == true)
                        //주자가 1루 / 2루 / 1,2루인 경우
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 드래그번트 기본조건
        /// </summary>                
        public static bool checkDragBunt(int scoreGab, bool[] bOnBase)
        {
            if (Mathf.Abs(scoreGab) < 6) //점수가 6점차 이내
            {
                if (bOnBase[SimulParm.THIRDBASE_INDEX] == false && bOnBase[SimulParm.SECONDBASE_INDEX] == false && bOnBase[SimulParm.FIRSTBASE_INDEX] == false)
                {
                    //루상에 주자가 없는 경우
                    return true;
                }
            }
            return false;
        }

        ///////////////////////////////////////////////////////////////////////
        //번트의 발생확률과 발생시 결과 동시에 -> Auto & 시뮬에서 사용
        ///////////////////////////////////////////////////////////////////////  
        /// <summary>
        /// 번트 발생 여부
        /// </summary>
        /// <param name="basicInvoke">스퀴즈, 희생, 드래그 여부에 따른 기본 발생확률</param>
        /// <param name="pitcher"></param>
        /// <param name="batter"></param>
        /// <returns>발생시 true를 리턴한다</returns>
        public static bool checkBuntInvoke(float basicInvoke, CPlayer pitcher, CPlayer batter)
        {
            if (Mode.bPvpMode == false)
            {
                float invokeRate;
                int PminusB = pitcher.getPitcherAvg() - batter.getOffenseAvg();
                ////Debug.Log("=============>> PminusB = " + PminusB);
                if (PminusB < 0)
                {
                    //투수우위?                        
                    invokeRate = Mathf.Clamp(basicInvoke + (PminusB * BUNT_INVOKE_CONSTANT_PITCHERWIN) * 0.001f, 0.05f, 0.5f);
                    ////Debug.Log("=============>> 투수값이 작은 경우 진짜 invokeRate = " + (basicInvoke + (PminusB * BUNT_INVOKE_CONSTANT_PITCHERWIN) * 0.001f));
                    ////Debug.Log("=============>> 투수값이 작은 경우 invokeRate = " + invokeRate);
                }
                else
                {
                    //타자우위?
                    invokeRate = Mathf.Clamp(basicInvoke + (PminusB * BUNT_INVOKE_CONSTANT_BATTERWIN) * 0.001f, 0.05f, 0.5f);
                    ////Debug.Log("=============>> 투수값이 큰 경우 진짜 invokeRate = " + (basicInvoke + (PminusB * BUNT_INVOKE_CONSTANT_BATTERWIN) * 0.001f));
                    ////Debug.Log("=============>> 투수값이 큰 경우 invokeRate = " + invokeRate);
                }

                if (MyMath.PercentF() < invokeRate)
                {
                    ////Debug.Log("===================================================================>>번트시도");
                    return true;
                }
            }
            ////Debug.Log("===================================================================>>번트시도 안함");
            return false;
        }


        public static bool checkBuntSuccess(float basicSuccess, CPlayer pitcher, CPlayer batter, float addRate, float min, float max)
        {
            int BminusP = batter.getBuntPower() - pitcher.getPitcherAvg();
            ////Debug.Log("=============>> BminusP = " + BminusP);
            float successRate = Mathf.Clamp((basicSuccess + (BminusP * BUNT_SUCCESS_CONSTANT * 0.001f) + addRate), min, max);
            ////Debug.Log("=============>> 번트 원래 성공률 successRate = " + (basicSuccess + (BminusP * BUNT_SUCCESS_CONSTANT * 0.001f)) + "====>>> 스킬 부가 이펙트 " + +addRate);
            ////Debug.Log("=============>> 번트 성공률 successRate = " + successRate);

            if (MyMath.PercentF() < successRate)
            {
                ////Debug.Log("===================================================================>>번트성공");
                return true;

            }
            ////Debug.Log("===================================================================>>번트실패");
            return false;
        }


        /// <summary>
        /// 스퀴즈 번트의 발생확률과 발생시 결과를 동시에 리턴
        /// </summary>
        /// <returns></returns>
        public static SpecificBuntType getSqueezeResult(int outCount, int scoreGab, bool[] bOnBase, CPlayer pitcher, CPlayer batter)
        {
            if (checkSqueezeBuntCase(outCount, scoreGab, bOnBase) == true)
            {                
#if _Local_Balance
                if (InGameDebug._BUNT_TEST == true)
                {
                    return getSqueezeSuccessResult(pitcher, batter);
                }
                else
#endif
                {
                    ////Debug.Log("==============>> 스퀴즈 발생 체크");
                    if(checkBuntInvoke(SQUEEZE_BASIC_INVOKE, pitcher, batter) == true)
                    {
                        return getSqueezeSuccessResult(pitcher, batter);
                    }
                }
            }
            return SpecificBuntType.NONE;
        }


        /// <summary>
        /// 희생번트의 발생확률과 발생시 결과를 동시에 리턴
        /// </summary>
        /// <returns></returns>
        public static SpecificBuntType getSacResult(int outCount, int scoreGab, bool[] bOnBase, CPlayer pitcher, CPlayer batter)
        {
            if (checkSacBuntCase(outCount, scoreGab, bOnBase) == true)
            {                
#if _Local_Balance
                if (InGameDebug._BUNT_TEST == true)
                {
                    return getSacSuccessResult(pitcher, batter);
                }
                else
#endif
                {
                    ////Debug.Log("==============>> 희생번트 발생 체크");
                    if (checkBuntInvoke(SACRIFY_BASIC_INVOKE, pitcher, batter) == true)
                    {
                        return getSacSuccessResult(pitcher, batter);
                    }
                }
            }
            return SpecificBuntType.NONE;

        }        

        /// <summary>
        /// 드래그 번트의 발생확률과 발생시 결과를 동시에 리턴
        /// </summary>
        /// <returns></returns>
        public static SpecificBuntType getDragBuntResult(int scoreGab, bool[] bOnBase, CPlayer pitcher, CPlayer batter)
        {
            if (checkDragBunt(scoreGab, bOnBase) == true)
            {             
#if _Local_Balance
                if (InGameDebug._BUNT_TEST == true)
                {
                    return getDragBuntSuccessResult(pitcher, batter);
                }
                else
#endif
                {
                    ////Debug.Log("==============>> 드래그번트 발생 체크");
                    if (checkBuntInvoke(DRAG_BASIC_INVOKE, pitcher, batter) == true)
                    {
                        return getDragBuntSuccessResult(pitcher, batter);
                    }
                }
            }
            return SpecificBuntType.NONE;
        }


        ///////////////////////////////////////////////////////////////////////
        //번트의 성공 결과만 -> 액션시 실플레이에서만 참조
        ///////////////////////////////////////////////////////////////////////        

        /// <summary>
        /// 스퀴즈 번트의 성공여부를 리턴
        /// </summary>
        /// <returns></returns>
        public static SpecificBuntType getSqueezeSuccessResult(CPlayer pitcher, CPlayer batter)
        {
#if _Local_Balance
            if (InGameDebug._BUNT_TEST == true)
            {
                return (InGameDebug._BUNT_SUCCESS ? SpecificBuntType.SQUEEZ_SUCCESS : SpecificBuntType.SQUEEZ_FAIL);
            }
            else
#endif
            {
                ////Debug.Log("======================>> 스퀴즈 번트 성공체크");
                float addRate = batter.getSkillScopeRate(SkillIndex.GodOfBunt) * 0.01f;
                if (checkBuntSuccess(SQUEEZE_BASIC_SUCCESS, pitcher, batter, addRate, SQUEEZE_MIN, SQUEEZE_MAX) == true)
                {
                    return SpecificBuntType.SQUEEZ_SUCCESS;
                }
            }
            return SpecificBuntType.SQUEEZ_FAIL;
        }

        /// <summary>
        /// 희생번트의 성공여부를 리턴
        /// </summary>
        /// <returns></returns>
        public static SpecificBuntType getSacSuccessResult(CPlayer pitcher, CPlayer batter)
        {
#if _Local_Balance
            if (InGameDebug._BUNT_TEST == true)
            {
                return SpecificBuntType.SAC_SUCCESS;// (InGameDebug._BUNT_SUCCESS ? SpecificBuntType.SAC_SUCCESS : SpecificBuntType.SAC_FAIL);
            }
            else
#endif
            {
                ////Debug.Log("======================>> 희생 번트 성공체크");
                float addRate = batter.getSkillScopeRate(SkillIndex.GodOfBunt) * 0.01f;
                if (checkBuntSuccess(SACRIFY_BASIC_SUCCESS, pitcher, batter, addRate, SACRIFY_MIN, SACRIFY_MAX) == true)
                {
                    return SpecificBuntType.SAC_SUCCESS;
                }
            }
            return SpecificBuntType.SAC_FAIL;

        }

        


        /// <summary>
        /// 드래그번트의 성공여부를 리턴
        /// </summary>
        /// <returns></returns>
        public static SpecificBuntType getDragBuntSuccessResult(CPlayer pitcher, CPlayer batter)
        {
#if _Local_Balance
            if (InGameDebug._BUNT_TEST == true)
            {
                return (InGameDebug._BUNT_SUCCESS ? SpecificBuntType.DRAG_SUCCESS : SpecificBuntType.DRAG_FAIL);
            }
            else
#endif
            {
                ////Debug.Log("======================>> 드래그 번트 성공체크");
                if (checkBuntSuccess(DRAG_BASIC_SUCCESS, pitcher, batter, 0, DRAG_MIN, DRAG_MAX) == true)
                {
                    return SpecificBuntType.DRAG_SUCCESS;
                }
            }
            return SpecificBuntType.DRAG_FAIL;
        }
    }
}
