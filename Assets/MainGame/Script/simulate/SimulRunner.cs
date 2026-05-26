//#define CHECKONEMORE_TEST //주루사 테스트
//#define BASETAG_TEST
//#define DOUBLEPLAY_TEST

using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class SimulRunner //: MonoBehaviour
    {

        //바운드에 따른 가감값
        const int SIDEBOUND_VALUE = 100;
        const int FENSEBOUND_VALUE = 200;
        const int NORMALBOUND_VALUE = -400;
        const int DASHBOUND_VALUE = -700;
        //포구형태에 따른 가감
        const int SIDECATCH_VALUE = 100;
        const int NORMALCATCH_VALUE = 0;
        const int DASHCATCH_VALUE = -400;

        //주루사(보살)
        const int ASSIST_VALUE = 100;        //보살 가능 값
        const int ASSIST_MINIMUM = 650;     //보살성공을 위한 최소값

        public int runnerIndex, arrayInex;
        public int dstPos, curPos, lastPos;
        public CPlayer runner;
        public bool bHitterRunner;
        public bool bErrorRunner;       //에러로 출루
        public bool bLastPitcher;       //승계주자
        public bool bChangedRunner;
        public bool bAddScore;


        public int lineup;

        //주루 능력
        public int baseRunningValue;
        
        /*
        //스킬 - 패시브
        public byte sSuperSonic;               //가속을 증가            : 지속 -> done
        public byte sRunningSense;             //주루딜레이 감소, 판단 증가
        public byte sLead;                     //주자의 리드폭을 증가   : 지속 -> done
        public byte sSliding;                  //슬라이딩 스킬을 증가   : 지속 -> done
        //스킬 - 액티브
        public byte sTurboEngineActive;         //타자주자 터보          : 액티브
        public byte sHomeRushActive;            //홈돌진                 : 액티브 -> done
        public byte sDoublePlayActive;          //병살저지               : 액티브
        public byte sStealTurboActive;          //도루시 터보          : 액티브
        public byte sDelayStealActive;         //딜레이 스틸        :액티브
        */
        

        public void makeHitterRunner(CPlayer hitter, int index)
        {
            bAddScore = false;
            bLastPitcher = false;
            bErrorRunner = false;
            bHitterRunner = true;
            dstPos = SimulParm.FIRSTBASE_INDEX;
            curPos = SimulParm.HOMEBASE_INDEX;
            lastPos = curPos;
            runner = hitter;
            bChangedRunner = false;
            arrayInex = index;
            initSkill();
        }

        public void makeRunnerOnBase(CPlayer _runner, int index, int curBase)
        {
            bAddScore = false;
            bLastPitcher = false;
            bErrorRunner = false;
            bHitterRunner = false;            
            curPos = curBase;
            dstPos = curBase + 1;
            lastPos = curPos;
            runner = _runner;
            bChangedRunner = false;
            arrayInex = index;
            initSkill();
        }

        public void setEmpty(bool bHomeIn = false)
        {
            bChangedRunner = false;
            dstPos = curPos = -1;// (bHomeIn == false ? -1 : -100);
        }

        public bool setArriveBase(int baseIndex)
        {
            curPos = baseIndex;
            if (curPos == SimulParm.HOMEBASE_INDEX)
            {
                //득점 처리
                return true;
            }
            else
            {
                bHitterRunner = false;
                dstPos = baseIndex + 1;
                return false;
            }
        }
        /*
        public int checkOneMoreBase(CPlayer fielder, FlyCatchType catchType)
        {

            //안타 발생시 추가 베이스 획득 여부
            //return 1; //1베이스 획득
            //return -1; //주루사

            if (catchType == FlyCatchType.CATCH_ERROR ||
                catchType == FlyCatchType.BOUND_ERROR ||
                catchType == FlyCatchType.BOUND_ERROR)
            {
                //에러시 무조건 1베이스 추가
                return 1;
            }

            int throwAbil = ((fielder.getThrowing() + fielder.getThrowBonus()) * 7 + (fielder.getFielding() + fielder.getCatchBonus()) * 3) / 10;
            int runnerAbil = runner.getSpeed() + runner.getSpeedBonus();
            int skillPoint = 0;
            int laserPoint = 0;
            

            runnerAbil += skillPoint;   //스킬 포인트 더해줌

            if (catchType == FlyCatchType.SideBound)
            {
                runnerAbil += SIDEBOUND_VALUE;//100;
            }
            else if (catchType == FlyCatchType.FenceBound)
            {
                runnerAbil += FENSEBOUND_VALUE;
            }
            else if (catchType == FlyCatchType.NormalBound)
            {
                runnerAbil += NORMALBOUND_VALUE;
            }
            else if (catchType == FlyCatchType.DashBound)
            {
                runnerAbil += DASHBOUND_VALUE;
            }

            int runningRange = Random.Range(0, runnerAbil);
            int throwRange = Random.Range(0, throwAbil + 1500);
            int gab = runningRange - throwRange;

            //////UnityEngine.//Debug.Log("======================>>한베이스 더가기 위한 주루와 어깨 레인지의 차이값 = " + gab);

            if (gab > 0)
            {
                if (gab < ASSIST_VALUE)
                {                    
                    if (Random.Range(0, throwAbil + laserPoint) > ASSIST_MINIMUM)
                    {
                        ////UnityEngine.//Debug.Log("======================>>보살");
                        return -1;
                    }
                }

                return 1;
            }

            return 0;
        }
*/
        public int checkBaseTag(CPlayer fielder, FlyCatchType catchType)
        {
#if BASETAG_TEST
            //베이스택
            //return 0; //안함
            //return 1; //1베이스 획득
            //return -1; //주루사
            return 0;  
#else
            if (catchType == FlyCatchType.HomeRunSteal ||
                catchType == FlyCatchType.DivingCatch ||
                catchType == FlyCatchType.OverHeadRun)
            {
                //이경우는 무조건 베이스택 성공
                return 1;
            }

            int throwAbil = ((fielder.getThrowing()) * 7 + (fielder.getFielding()) * 3) / 10;
            int runnerAbil = runner.getSpeed();
            int skillPoint = 0;
            int laserPoint = 0;
            

            runnerAbil += skillPoint;   //스킬 포인트 더해줌

            if (catchType == FlyCatchType.RIghtRun || catchType == FlyCatchType.LeftRun)
            {
                runnerAbil += SIDECATCH_VALUE;//100;
            }
            else if (catchType == FlyCatchType.Normal)
            {
                runnerAbil += NORMALCATCH_VALUE;
            }
            else if (catchType == FlyCatchType.DashRun)
            {
                runnerAbil += DASHCATCH_VALUE;
            }

            int runningRange = Random.Range(0, runnerAbil);
            int throwRange = Random.Range(0, throwAbil);
            int gab = runningRange - throwRange;

            //////UnityEngine.//Debug.Log("======================>>베이스택을 하기 위한 주루와 어깨 레인지의 차이값 = " + gab);

            if (gab > 0)
            {
                if (gab < ASSIST_VALUE)
                {
                    //필더의 스킬 세팅
                    //


                    if (Random.Range(0, throwAbil + laserPoint) > ASSIST_MINIMUM)
                    {
                        ////UnityEngine.//Debug.Log("======================>>보살");
                        return -1;//주루사
                    }
                }

                return 1; //베이스 획득
            }


            return 0;
#endif
        }



        public bool checkDoublePlay(int fieldValue)
        {
#if DOUBLEPLAY_TEST
            return true;
#else

            //fieldValue vs runnerValue
            int runnerValue = runner.getSpeed();

            int runningRange = Random.Range(0, runnerValue);
            int fieldingRange = Random.Range(0, fieldValue);

            ////UnityEngine.Debug.Log("[더블플레이 체크]============>> " + runner.getName() + " 향하고 있는 루: " + (dstPos + 1) + "루 " + "runningRange MAX(" + runnerValue + ") vs fieldingRange MAX(" + fieldValue + ")::" + runningRange + " vs " + fieldingRange);

            if (fieldingRange > runningRange)
            {
                return true;
            }
            else
            {
                return false;
            }
#endif
        }


        public CPlayer getRunner()
        {
            return runner;
        }

        public void setRunner(CPlayer player)
        {
            runner = player;
            initSkill();
        }

        public void initSkill()
        {
            //값을 초기화
        /*    sSuperSonic = 0;
            sRunningSense = 0;
            sLead = 0;
            sSliding = 0;
            sTurboEngineActive = 0;
            sHomeRushActive = 0;
            sDoublePlayActive = 0;
            sStealTurboActive = 0;
            sDelayStealActive = 0;*/

        }

        //액티브 스킬 세팅(매 setStandBy 혹은 checkOneMoreBase 이후 호출)
        void setCurSkill()
        {
            
        }

        public int getSpeed()
        {
            return runner.getSpeed();
        }

        //////////////////////////////////////////////////////////
        //스킬
        //////////////////////////////////////////////////////////
        /// <summary>
        /// 시뮬엔진에서 스킬 가지고 있는지 여부 체크
        /// </summary>
        public bool skillAvailable(SkillIndex index)
        {
            //가지고 있는지 여부
            return runner.skillAvailable(index);

        }

        /// <summary>
        /// 시뮬엔진에서 발동여부 체크
        /// </summary>
        public bool skillActiveSuccess(SkillIndex index)
        {
            //발동
            return runner.fieldSkillSuccess(index);
        }

    }
}