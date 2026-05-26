using UnityEngine;
using System.Collections;
using WebConnector;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    //[System.Serializable]
    public class SimulGameInfo
    {
        public const int ColdGame = 10;
        public const int MAX_INNING = 12;

          
        
        //Count
        public int ballCount,
                   strikeCount,
                   outCount;

        //inning
        public int[,] inningScore;

        //기록
        public int[] run,
                      hit,
                      error,
                      homerun,
                      steal,
                      strikeout,
                      doubleplay,
                      pickOffOut,
                      fourBall,
                      pitchNum;


        //public int[] curAP, maxAP;


        public int conHit, conHR, conRun;
        public int allowChulu;  //출루허용


        private int currentInning;
        

        public void init()
        {
            currentInning = 1;
            run = new int[2];
            hit = new int[2];
            error = new int[2];
            homerun = new int[2];
            steal = new int[2];
            strikeout = new int[2];
            doubleplay = new int[2];
            fourBall = new int[2];
            pickOffOut = new int[2];
            inningScore = new int[2, MAX_INNING];
            pitchNum = new int[2];

        
            for (int i = 0; i < 2; i++)
            {
                run[i] = 0;
                hit[i] = 0;
                error[i] = 0;
                homerun[i] = 0;
                steal[i] = 0;
                strikeout[i] = 0;
                doubleplay[i] = 0;
                fourBall[i] = 0;
                pitchNum[i] = 0;
                for (int j = 0; j < MAX_INNING; j++)
                {
                    inningScore[i, j] = 0;
                }

            }
            //curAP = new int[2];
            //maxAP = new int[2]; 

            DEBUG_COUNTER_INIT();
        }

        public void setCurrentInning(int inning)
        {
            currentInning = inning;
        }

        

        /*
        public void sync(BallPlayManager manager)
        {
            for (int i = 0; i < 2; i++)
            {
                run[i] = manager.nGameScore[i];
                hit[i] = manager.nHitCount[i];
                error[i] = manager.nErrorCount[i];
                homerun[i] = manager.nHomerunCount[i];
                steal[i] = manager.nStealCount[i];
                strikeout[i] = manager.nStrikeOutCount[i];
                doubleplay[i] = manager.nDPCount[i];
                fourBall[i] = manager.nFourballCount[i];
                for (int j = 0; j < MAX_INNING; j++)
                {
                    inningScore[i, j] = manager.nInningScore[i, j];
                }
            }

            ballCount = manager.nBallCount;
            strikeCount = manager.nStrikeCount;
            outCount = manager.nOutCount;
        }*/



        public void initCount(bool bInning) //bool bInning = false
        {
            ballCount = strikeCount = 0;
            if (bInning == true) outCount = 0;
        }

        public SimulResultState checkStrike(CPlayer pitcher, bool bFoul) //bool bFoul = false
        {
            strikeCount++;

            if (bFoul == true && strikeCount >= 3)
            {
                strikeCount = 2;
                return SimulResultState.NONE;
            }

            if (strikeCount >= 3)
            {
                return SimulResultState.StrikeOut;
            }
            else
            {
                pitcher.setPinchScoreReduce(1);
            }
            return SimulResultState.NONE;
        }

        public SimulResultState checkBall()
        {
            ballCount++;

            if (ballCount >= 4)
            {
                return SimulResultState.FourBall;
            }
            return SimulResultState.NONE;
        }

        /*
        public void setAp(int ap, int index)
        {
            curAP[index] += ap;
            if (curAP[index] > maxAP[index]) curAP[index] = maxAP[index];
            else if (curAP[index] < 0) curAP[index] = 0;
        }*/

        public bool checkOut(CPlayer pitcher)
        {
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수이닝: " + pitcher.getName());
            conHit = conHR = 0;

            pitcher.setPinchScoreReduce(3);
                        
            pitcher.setRecord(Param.ST_IP);  //이닝 카운트
            outCount++;
            if (outCount >= 3)
            {
                return true;
            }
            return false;
        }

        public void addPitch(int index, CPlayer pitcher)
        {
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투구수: " + pitcher.getName());
            pitcher.setRecord(Param.ST_PNP);
            //스태미너 여기서 처리 해줘~~
        }


        public void addRun(int index, int inning, CPlayer runner, CPlayer batter, CPlayer pitcher, bool bErrorFlag, bool bRbiFlag)
        {            
            conRun++;

            run[index]++;
            inningScore[index, inning-1]++;

            //투수기록
            //실점 카운트
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수실점: " + pitcher.getName());
            if (pitcher != null)
            {
                pitcher.setPiledupSkill(SkillIndex.WinSpirit, 1, false); //필승의지 효과제거
                pitcher.setRecord(Param.ST_PR);
            }

            if (bErrorFlag == false)
            {
                //에러 플래그가 아닌경우 자책 카운트
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수자책: " + pitcher.getName());
                if (pitcher != null) pitcher.setRecord(Param.ST_PER);
            }
            //setAp(-2, 1 - index);
            //setAp(2, index);

            //타자 기록
            if (bRbiFlag == true)
            {
                //rbi플래그 true이 경우 타점 카운트
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자타점: " + batter.getName());
                if (batter != null)
                {
                    batter.setRecord(Param.ST_RBI);
                    batter.setRbiRecord(1);
                }
            }

            //주자 기록
            //주자 득점 카운트
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>주자득점");
            if (runner != null) runner.setRecord(Param.ST_R);
        }

        public void addHit(int index, SimulResultState type, SimulHitType hitType, CPlayer batter, CPlayer pitcher, int run = 0)
        {
            int _run = run;

            allowChulu++;
            conHit++;

            hit[index]++;
            //타자 기록
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자안타: " + batter.getName());
            batter.setRecord(Param.ST_H);
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자타수: " + batter.getName());
            batter.setRecord(Param.ST_PA);
            batter.setRecord(Param.ST_AB);
            
            //투수기록
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수피안타: " + pitcher.getName());
            pitcher.setRecord(Param.ST_PH);
            if (type == SimulResultState.Double || type == SimulResultState.DoubleOneError)    //2루타
            {
                pitcher.setPiledupSkill(SkillIndex.DoctorK, 3, false); //닥터 K효과제거
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자2루타: " + batter.getName());
                batter.setRecord(Param.ST_2B);
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수피2루타: " + pitcher.getName());
                pitcher.setRecord(Param.ST_P2B);
                batter.setResultStr("2루타");
                //setAp(3, index);
                batter.setDetailRecord(Param.DetailRecord.Double, currentInning);

            }
            else if (type == SimulResultState.Triple || type == SimulResultState.TripleOneError)    //3루타
            {
                pitcher.setPiledupSkill(SkillIndex.DoctorK, 3, false); //닥터 K효과제거
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자3루타: " + batter.getName());
                batter.setRecord(Param.ST_3B);
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수피3루타: " + pitcher.getName());
                pitcher.setRecord(Param.ST_P3B);
                batter.setResultStr("3루타");
                //setAp(5, index);
                batter.setDetailRecord(Param.DetailRecord.Tripple, currentInning);

            }
            else if (type == SimulResultState.HomeRun)
            {
                pitcher.setPiledupSkill(SkillIndex.DoctorK, 3, false); //닥터 K효과제거
                conHR++;
                homerun[index]++;
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자홈런: " + batter.getName());
                batter.setRecord(Param.ST_HR);
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수피홈런: " + pitcher.getName());
                pitcher.setRecord(Param.ST_PHR);
                batter.setResultStr("홈런");
                //setAp(7, index);
                //setAp(-3, 1-index);
                batter.setDetailRecord(Param.DetailRecord.Homerun, currentInning);

            }
            else
            {
                batter.setResultStr("안타");
                //setAp(2, index);
                batter.setDetailRecord(Param.DetailRecord.Single, currentInning);
            }


            setHitType(hitType, batter, pitcher, true);
        }

        public void addError(int index, SimulHitType hitType, CPlayer batter, CPlayer pitcher, CPlayer fielder)
        {
            allowChulu++;
            error[index]++;

            //setAp(-1, index);

            //타자 기록
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자타수: " + batter.getName());
            if (batter != null)
            {
                batter.setRecord(Param.ST_PA);
                batter.setRecord(Param.ST_AB);
                batter.setDetailRecord(Param.DetailRecord.Error, currentInning);
            }
            //해당 야수 기록도 카운트
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>야수에러: " + fielder.getName());
            fielder.setRecord(Param.ST_E);

            setHitType(hitType, batter, pitcher, false);
        }

        public void addSteal(int index, bool bSuccess, CPlayer runner, CPlayer catcher)
        {
            steal[index]++;

            //setAp(2, index);
            //setAp(-1, 1-index);

            //해당 주자 기록도 카운트
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>주자도루: "+runner.getName());
            runner.setRecord(Param.ST_SBS);
            //해당 포수
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>포수도루허용: "+catcher.getName());
            catcher.setRecord(Param.ST_SBA);

        }

        public void addStrkeOutCount(int index, CPlayer batter, CPlayer pitcher, CPlayer catcher)
        {
            conHit = conHR = 0;

            strikeout[index]++;

            //setAp(2, index);
            //setAp(-2, 1-index);

            //해당 타자 기록도 카운트
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자삼진: " + batter.getName());
            batter.setRecord(Param.ST_SO);
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자타수: " + batter.getName());
            batter.setRecord(Param.ST_PA);
            batter.setRecord(Param.ST_AB);
            batter.setResultStr("삼진");
            batter.setDetailRecord(Param.DetailRecord.StrikeOut, currentInning);

            //해당 투수 기록도 카운트
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수탈삼진: " + pitcher.getName());
            pitcher.setRecord(Param.ST_PSO);

            //삼진시 포수의 자살 추가
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>포수자살: " + catcher.getName());
            catcher.setRecord(Param.ST_PO);

        }

        public void addFourBall(int index, CPlayer batter, CPlayer pitcher, bool bHitbyPitch) //bool bHitbyPitch = false
        {
            allowChulu++;
            conHit++;

            fourBall[index]++;

            batter.setRecord(Param.ST_PA); //타수 카운트
            if (bHitbyPitch == false)
            {
                //타자
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자4구: " + batter.getName());
                batter.setRecord(Param.ST_BB);
                //투수
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수4구: " + pitcher.getName());
                pitcher.setRecord(Param.ST_PBB);
                batter.setResultStr("포볼");
                batter.setDetailRecord(Param.DetailRecord.Baseonball, currentInning);
            }
            else
            {
                //타자
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자힛바이피치볼: " + batter.getName());
                batter.setRecord(Param.ST_HBP);
                //투수
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>투수힛바이피치볼: " + pitcher.getName());
                pitcher.setRecord(Param.ST_PHBP);
                batter.setResultStr("사구");
                batter.setDetailRecord(Param.DetailRecord.HitbyPitched, currentInning);
            }


        }

        public void addDoublePlayCount(int index, CPlayer batter)
        {
            doubleplay[index]++;
            //해당 타자 기록도 카운트
            ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자병살: " + batter.getName());
            batter.setRecord(Param.ST_DP);
        }


        public void setFieldOut(SimulResultState type, SimulHitType hitType, CPlayer batter, CPlayer pitcher, CPlayer poFielder, CPlayer aoFielder,bool bSac)
        {//SimulResultState type, SimulHitType hitType, CPlayer batter, CPlayer pitcher, CPlayer poFielder = null, CPlayer aoFielder = null
            if (batter != null)
            {
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>타자타수: " + batter.getName());
                batter.setRecord(Param.ST_PA);
                if (bSac == false)
                {
                    batter.setRecord(Param.ST_AB);
                    batter.setResultStr(type == SimulResultState.Grounder ? "땅볼" : "뜬공");

                    if(hitType == SimulHitType.Liner) batter.setDetailRecord(Param.DetailRecord.Liner, currentInning);
                    else if (hitType == SimulHitType.Fly) batter.setDetailRecord(Param.DetailRecord.Fly, currentInning);
                    else batter.setDetailRecord(Param.DetailRecord.Grounder, currentInning);
                }
                else
                {
                    //희생타인 경우 타수 안올라감
                    batter.setResultStr("희생");
                    batter.setDetailRecord(Param.DetailRecord.Sacrify, currentInning);
                }
            }

            if (pitcher != null)
            {

            }


            if (poFielder != null)
            {
                //필드 플레이에서 자살 카운트
                //Debug.Log[자살 기록] ===========>> " + poFielder.getName());
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>야수자살: " + poFielder.getName());
                poFielder.setRecord(Param.ST_PO);
            }

            if (aoFielder != null)
            {
                //필드 플레이에서 보살 카운트
                //Debug.Log[보살 기록] ===========>> " + aoFielder.getName());
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>야수보살: " + aoFielder.getName());
                aoFielder.setRecord(Param.ST_A);
            }

            if (hitType != null)
            {
                setHitType(hitType, batter, pitcher, false);
            }

        }

        public void setRunnerOut(CPlayer runner, CPlayer poFielder, CPlayer aoFielder, bool bStealOut)
        {
            //CPlayer runner, CPlayer poFielder = null, CPlayer aoFielder = null, bool bStealOut = false
            if (bStealOut == true)
            {
                if (runner != null)
                {
                    //도루자 추가
                    ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>주자도루자: " + runner.getName());
                    runner.setRecord(Param.ST_SBF);
                }
                if (aoFielder != null)
                {
                    //도루 저지 추가
                    ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>포수도루저지 " + aoFielder.getName());
                    aoFielder.setRecord(Param.ST_CS);
                }
            }

            if (poFielder != null)
            {
                //주루 플레이에서 야수 자살 추가
                //Debug.Log[자살 기록] ===========>> " + poFielder.getName());
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>야수자살: " + poFielder.getName());
                poFielder.setRecord(Param.ST_PO);
            }

            if (aoFielder != null)
            {
                //주루 플레이에서 야수 보살 추가
                //Debug.Log[보살 기록] ===========>> " + aoFielder.getName());
                ////UnityEngine.Debug.Log("[시뮬기록]=======================>>>야수보살: " + aoFielder.getName());
                aoFielder.setRecord(Param.ST_A);
            }

        }


        private void setHitType(SimulHitType hitType, CPlayer batter, CPlayer pitcher, bool bHit)
        {
            if (hitType == SimulHitType.Fly)
            {
                if (batter != null)
                {
                    batter.setHitType(Param.ST_FLY);
                    if (bHit == true) batter.setHitType(Param.ST_FLYHIT);
                }
                if (pitcher != null)
                {
                    pitcher.setHitType(Param.ST_FLY);
                }
            }
            else if (hitType == SimulHitType.Grounder)
            {
                if (batter != null)
                {
                    batter.setHitType(Param.ST_GROUNDER);
                    if (bHit == true) batter.setHitType(Param.ST_GROUNDERHIT);
                }
                if (pitcher != null)
                {
                    pitcher.setHitType(Param.ST_GROUNDER);
                }
            }
            else// if (hitType == SimulHitType.Liner)
            {
                if (batter != null)
                {
                    batter.setHitType(Param.ST_LINER);
                    if (bHit == true) batter.setHitType(Param.ST_LINERHIT);
                }
                if (pitcher != null)
                {
                    pitcher.setHitType(Param.ST_LINER);
                }
            }
        }



        //디버깅 기록 수집용
        int debug_grounder, debug_liner, debug_fly;
        int[] debugValue = new int[5];

        public void DEBUG_COUNTER_INIT()
        {
            debug_grounder = debug_liner = debug_fly = 0;
            for (int i = 0; i < 5; i++) debugValue[i] = 0;
        }

        public void DEBUG_COUNTER_RESULT()
        {
            //Debug.Log[그라운더 퍼센트] ================>> " + ((debug_grounder * 100) / (debug_grounder + debug_liner + debug_fly)));
            //Debug.Log[라이너 퍼센트] ================>> " + ((debug_liner * 100) / (debug_grounder + debug_liner + debug_fly)));
            //Debug.Log[플라이 퍼센트] ================>> " + ((debug_fly * 100) / (debug_grounder + debug_liner + debug_fly)));
        }

        public void DEBUG_GROUNDER_COUNT()
        {
            debug_grounder++;
        }
        public void DEBUG_LINER_COUNT()
        {
            debug_liner++;
        }
        public void DEBUG_FLY_COUNT()
        {
            debug_fly++;
        }

        
        public void DEBUG_CONTROLINFO(ControlValue value)
        {
            if (value == ControlValue.PinPoint) debugValue[0]++;
            else if (value == ControlValue.Good) debugValue[1]++;
            else if (value == ControlValue.Normal) debugValue[2]++;
            else if (value == ControlValue.Bad) debugValue[3]++;
            else if (value == ControlValue.Miss) debugValue[4]++;
        }

        public void DEBUG_CONTACTINFO(BattingContact value)
        {
            if (value == BattingContact.SOLID) debugValue[0]++;
            else if (value == BattingContact.GOOD) debugValue[1]++;
            else if (value == BattingContact.NORMAL) debugValue[2]++;
            else if (value == BattingContact.BAD) debugValue[3]++;
            else if (value == BattingContact.HUT_SWING) debugValue[4]++;
        }

        public void DEBUG_TIMINGINFO(BattingTiming value)
        {
            if (value == BattingTiming.PERFECT) debugValue[0]++;
            else if (value == BattingTiming.JUST_EARLY || value == BattingTiming.JUST_LATE) debugValue[1]++;
            else if (value == BattingTiming.EARLY || value == BattingTiming.LATE) debugValue[2]++;
            else debugValue[3]++;
        }

        public void DEBUG_CONTROL_RESULT()
        {
        /*    int total = debugValue[0] + debugValue[1] + debugValue[2] + debugValue[3] + debugValue[4];
            /*
            ////UnityEngine.//Debug.Log("===============================================>>핀포인트 확률 = " + (debugValue[0] * 100 / total));
            ////UnityEngine.//Debug.Log("===============================================>>굿 확률 = " + (debugValue[1] * 100 / total));
            ////UnityEngine.//Debug.Log("===============================================>>노멀 확률 = " + (debugValue[2] * 100 / total));
            ////UnityEngine.//Debug.Log("===============================================>>배드 확률 = " + (debugValue[3] * 100 / total));
            ////UnityEngine.//Debug.Log("===============================================>>미스 확률 = " + (debugValue[4] * 100 / total));*/

        /*    ////UnityEngine.//Debug.Log("===============================================>>솔리드 컨택 = " + (debugValue[0] * 100 / total)+"%");
            ////UnityEngine.//Debug.Log("===============================================>>굿 컨택 = " + (debugValue[1] * 100 / total) + "%");
            ////UnityEngine.//Debug.Log("===============================================>>노멀 컨택 = " + (debugValue[2] * 100 / total) + "%");
            ////UnityEngine.//Debug.Log("===============================================>>배드 컨택 = " + (debugValue[3] * 100 / total) + "%");
            ////UnityEngine.//Debug.Log("===============================================>>헛스윙 컨택 = " + (debugValue[4] * 100 / total) + "%");*/

            /*
            ////UnityEngine.//Debug.Log("===============================================>>퍼펙트 타이밍 = " + (debugValue[0] * 100 / total) + "%");
            ////UnityEngine.//Debug.Log("===============================================>>저스트 타이밍 = " + (debugValue[1] * 100 / total) + "%");
            ////UnityEngine.//Debug.Log("===============================================>>타이밍 늦음/빠름 = " + (debugValue[2] * 100 / total) + "%");
            ////UnityEngine.//Debug.Log("===============================================>>타이밍 미스 = " + (debugValue[3] * 100 / total) + "%");*/

        }

    }
}