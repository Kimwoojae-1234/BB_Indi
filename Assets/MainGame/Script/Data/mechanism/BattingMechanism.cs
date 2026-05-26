//#define _SURPRISE_20PER_HAPPEN

using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class BattingMechanism
    {
        /// <summary>
        /// 배팅 메카니즘이 궁극적으로 도출해내는 밸류
        /// 1. 컨택 타입
        /// 2. 컨택 파워
        /// </summary>
        
        //////////////////////////////////////////////////////////////////////
        //타임 상수 (불변)
        //////////////////////////////////////////////////////////////////////
        //타격 딜레이
        public const float _ONTIMING_HIT_DELAY = 0.1f;
        //타격 딜레이(배트 부셔짐)
        public const float _BROKEN_HIT_DELAY = 0.15f;
        
        //퍼펙트 타이밍
        public const float PERFECT_TIMING = 0.880f;
        //정타 타이밍(빠른쪽)
        //public const float PERFECT_FAST = 0.865f;
        //정타 타이밍(늦은쪽)
        //public const float PERFECT_LATE = 0.895f;

        //피칭뷰 퍼펙트 타이밍
        public const float PERFECT_TIMING_PV = 0.895f;
        //피칭뷰 정타 타이밍(빠른쪽)
        //public const float PERFECT_FAST_PV = 0.88f;
        //피칭뷰 정타 타이밍(늦은쪽)
        //public const float PERFECT_LATE_PV = (PERFECT_FAST_PV + 0.03f);
        

        //////////////////////////////////////////////////////////////////////
        //기본 능력치 보정용 밸런스 상수
        //////////////////////////////////////////////////////////////////////
        //선구가중치
        public static float EYE_VALUE = 1.0f; 
        //컨택가중치
        public static float CONTACT_VALUE = 1.0f; 
        //파워가중치
        public static float POWER_VALUE = 1.0f; 
        //탄도가중치
        public static float TANDO_VALUE = 1.0f; 

        

        //////////////////////////////////////////////////////////////////////
        //컨택관련 밸런스 
        //////////////////////////////////////////////////////////////////////
        //퍼펙트 컨택 영역 계수
        public static float PERFECT_CONTACT_COEF = 0.05f;   //이값이 커지면 퍼펙트 컨택이 잘나옴
        //굿 컨택 영역 계수
        public static float GOOD_CONTACT_COEF = 0.15f;      //이값이 커지면 굿 컨택이 잘나옴
        //노멀 컨택 영역 계수
        public static float NORMAL_CONTACT_COEF = 0.4f;     //이값이 커지면 노멀 컨택이 잘나옴
        //배트 기본 사이즈
        public static float BAT_SIZEX = 70;                 //이값이 커지면 컨택이 쉬워짐
        public static float BAT_SIZEY = 50;                 //이값이 커지면 컨택이 쉬워짐
        //강타 배트 사이즈
        public static float GANGTA_BATSIZE = 30.8f;         //이값이 커지면 강타 컨택이 쉬워짐
        //오토모드 밸런스
        public static float CONTACT_AUTO_RATE = 1.0f;       //이값이 커지면 오토 혹은 AI플레이의 컨택값이 높아짐

        ///////////////////////////////////////////////////////////////////////
        //타이밍 관련 밸런스
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 타이밍 갭
        /// </summary>
        public static float TIMING_GAB = 0.015f;            //이값이 커지면 퍼펙트 타이밍이 쉬워짐
        //오토모드 밸런스
        public static float TIMING_AUTO_RATE = 1.0f;       //이값이 커지면 오토 혹은 AI플레이의 타이밍값이 높아짐


        ///////////////////////////////////////////////////////////////////////
        //파워 관련 밸런스
        ///////////////////////////////////////////////////////////////////////
        public static float POWER_RATE = 1.5f;          //이값이 커지면 능력치별 파워증가량이 증가
        public static float MIN_POWER = 20.0f;          //이값이 커지면 최소 파워값이 증가

        //////////////////////////////////////////////////////////////////////
        //탄도관련 밸런스 값
        //////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 탄도 레인지 적을수록 홈런 가능한 상태가 쉽게 된다
        /// </summary>
        public static int TANDO_RANGE = 3000;               //이값이 커지면 홈런이 잘 안나옴

        

        //////////////////////////////////////////////////////////////////////
        //인공지능 번트 컨택 실패 관련 밸런스 값
        //////////////////////////////////////////////////////////////////////
        public static int AI_BUNT_CONTACT_FAIL = 12;    //12%
        public static int SQUEEZE_PITCHOUT = 15;        //스퀴즈 피치아웃 확률 15%
        public static int DRAG_BUNT_FOUL = 35;          //드래그 번트 실패시 파울확률 35%
        public static int SAC_BUNT_FOUL = 20;           //희생 번트 실패시 파울확률 20%

        //////////////////////////////////////////////////////////////////////
        //운 관련 밸런스 값
        //////////////////////////////////////////////////////////////////////
        public static int BABIB_SIN = 30;    //바빕지수: 좋은 코스로 타구가 향할 확률
        public static int WRIST_USE = 25;    //의도적이지 않은 손목사용 : 타이밍과 어긋나는 코스 생산                


        

        //////////////////////////////////////////////////////////////////////
        //AI 타격 관련 상수 및 각종 계산식
        //////////////////////////////////////////////////////////////////////     
        /// <summary>
        /// 투수가 던진공을 오토모드 혹은 AI가 판단하여 스윙을 할지 안할지 여부를 반환
        /// </summary>
        public static bool checkSwingDecide(ControlValue conValue, int pGuwee, int bEye, bool bStrike, int ball, int strike, bool bSimul)
        {
            int gab = Mathf.Clamp((bEye - pGuwee), -500, 500) / 2;
            
            if (bStrike == true)
            {
                int[] agg = new int[5] { 450, 650, 850, 900, 2000 };
                int aggression = agg[(int)conValue] + gab;
                //aggression은 클수록 타자에 유리
                if (bSimul == false)
                {
                    //직접 플레이시
                    if (ball == 3 && strike == 0)
                    {
                        if (conValue != ControlValue.Miss)
                        {
                            //3볼이나 노스트라이크시 공격성 떨어짐
                            aggression /= 2;
                        }
                    }
                    else if (strike >= 1)
                    {
                        aggression *= (strike + 2);
                    }
                }
                else
                {
                    //시뮬레이션시
                    //직접 플레이시
                    if (ball == 3 && strike == 0)
                    {
                        if (conValue != ControlValue.Miss)
                        {
                            //3볼이나 노스트라이크시 공격성 떨어짐
                            aggression /= 2;
                        }
                    }
                    else if (strike >= 1)
                    {
                        aggression *= (strike + 2);
                    }
                }

                if (Random.Range(0, 1000) < aggression)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                int[] disc = new int[5] { 800, 500, 300, 100, -1000 };
                int discipline = disc[(int)conValue] - gab;
                
                if (strike == 2)
                {
                    //2스트라이크시 억제력 떨어짐
                    discipline += 200;
                }
                else if ((ball == 3 || ball == 0) && strike == 0)
                {
                    //3볼이나 노스트라이크시 억제력 올라감
                    discipline /= 3;
                }
                
                if (Random.Range(0, 1000) < discipline)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        
                
        /// <summary>
        /// 투수 최종 구위와 타자의 능력치를 비교하여 컨택 혹은 타이밍의 보너스 포인트를 제공
        /// 양수이면 타자에게 유리, 음수이면 타자에게 불리, 0이면 아무 영향을 주지 못한다
        /// </summary>
        /// <param name="finalGuwee"></param>
        /// <param name="batterValue"></param>
        /// <returns></returns>
        public static int getAddValue(float finalGuwee, float batterValue)
        {
            int addValue = 0;

            float rate = (batterValue * 2) / finalGuwee;
            float random = Random.Range(0.0f, rate);
            if (rate < 1)
            {
                if (random <= 0.2f) addValue = -2;
                else if (random <= 0.5f) addValue = -1;
            }
            else
            {                
                if (random >= 2) addValue = 2;
                else if (random >= 1) addValue = 1;
            }

            ////Debug.Log("===============================>> add Timing rate = " + rate + "==============>>random = " + random);
            ////Debug.Log("===============================>> add Timing Value = " + addValue);


            return addValue;

        }


        /// <summary>
        /// 오토모드 혹은 AI플레이시 투수최종구위와 타자의 컨택능력치를 비교하여 컨택 결과를 리턴한다.
        /// </summary>
        public static BattingContact getConctactResult(bool bAutoMode, bool bStrike, int finalGuwee, int bContact)
        {
            if (bStrike == false)
            {
                return (bAutoMode ? BattingContact.BAD : BattingContact.HUT_SWING);
            }
            else
            {
                float finalContact = (bContact * BattingMechanism.CONTACT_AUTO_RATE);
                int pitcherRange = Random.Range(0, finalGuwee);                      //투수가 잘할수록 높은값
                int hutswingValue = (int)(finalContact * Random.Range(3.0f, 7.0f));
                int solidValue = (int)(finalContact * Random.Range(0.4f, 0.7f));
                int goodValue = (int)(finalContact * Random.Range(0.8f, 1.2f));
                int normalValue = (int)(finalContact * Random.Range(1.5f, 2.2f));

                /*
                //Debug.Log("===========================================>>");
                Debug.Log("컨택점 스탯비교    최종구위 " + finalGuwee + "  컨택 " + bContact);
                Debug.Log("hutswingValue " + hutswingValue);
                Debug.Log("solidValue " + solidValue);
                Debug.Log("goodValue " + goodValue);
                Debug.Log("normalValue " + normalValue);
                //Debug.Log("===========================================>>pitcherRange " + pitcherRange);
                */

                if (finalGuwee > hutswingValue)
                {
                    return BattingContact.HUT_SWING;
                }
                else
                {
                    if (pitcherRange < solidValue)
                    {
                        return BattingContact.SOLID;
                    }
                    else if (pitcherRange < goodValue)
                    {
                        return BattingContact.GOOD;
                    }
                    else if (pitcherRange < normalValue)
                    {
                        return BattingContact.NORMAL;
                    }
                }
                return BattingContact.BAD;
            }
        }


        /// <summary>
        /// 오토모드 혹은 AI플레이시 투수최종구위와 타자의 선구능력치를 비교하여 컨택 결과를 리턴한다.
        /// </summary>
        public static BattingTiming getTimingResult(bool bAutoMode, bool bFastBall, int finalGuwee, int bEye)
        {
            int pitcherRange = Random.Range(0, finalGuwee);                      //투수가 잘할수록 높은값

            float finalEye = (bEye * BattingMechanism.TIMING_AUTO_RATE);

            int hutswingValue = (int)(finalEye * Random.Range(3.0f, 7.0f));
            int perfectValue = (int)(finalEye * Random.Range(0.1f, 0.55f));
            int goodValue = (int)(finalEye * Random.Range(0.55f, 1.5f));

            /*
            //Debug.Log("===========================================>>");
            Debug.Log("컨택점 스탯비교    최종구위 " + finalGuwee + "  컨택 " + bContact);
            Debug.Log("hutswingValue " + hutswingValue);
            Debug.Log("solidValue " + solidValue);
            Debug.Log("goodValue " + goodValue);
            Debug.Log("normalValue " + normalValue);
            //Debug.Log("===========================================>>pitcherRange " + pitcherRange);
            */

            int earlyTiming = bFastBall ? 40 : 60;
            bool bEarly = (MyMath.Percent() < earlyTiming ? true :false);

            if (finalGuwee > hutswingValue)
            {
                if (bAutoMode == true)
                {
                    return (bEarly == true ? BattingTiming.EARLY : BattingTiming.LATE);
                }
                else
                {
                    return (bEarly == true ? BattingTiming.VERY_EARLY : BattingTiming.VERY_LATE);
                }
            }
            else
            {
                if (pitcherRange < perfectValue)
                {
                    return BattingTiming.PERFECT;
                }
                else if (pitcherRange < goodValue)
                {
                    return (bEarly == true ? BattingTiming.JUST_EARLY : BattingTiming.JUST_LATE);
                }                
            }
            return (bEarly == true ? BattingTiming.EARLY : BattingTiming.LATE);
        }

        /// <summary>
        /// 모든 모드에서 투수 구위와 타자의 파워를 비교하여 타자가 발휘할 수 있는 최대 파워를 리턴한다
        /// </summary>
        public static float getBatterMaxPower(int power, int finalGuwee) //투수 최종구위
        {
            float rate = (float)(power * BattingMechanism.POWER_RATE) / (float)finalGuwee;  //float rate = (float)(power * 1.5f) / (float)guwee;
            float value = BattingMechanism.MIN_POWER + (20.0f * rate); //float value = 20 + (20.0f * rate);
            return Mathf.Clamp(value, 10, 40);
        }

        /// <summary>
        /// 볼을 걸러내는 체크 스윙을 할지 여부를 리턴한다
        /// </summary>
        /// <param name="finalGuwee"></param>
        /// <param name="bEye"></param>
        /// <returns></returns>
        public static bool checkSwing(int finalGuwee, int bEye)
        {
            int range = Random.Range(0, finalGuwee * 2);
            ////Debug.Log("======================>> range = " + range + "   bEye = " + bEye);
            if (range < bEye)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 빠따를 던지는 이벤트
        /// </summary>
        /// <returns></returns>
        public static bool checkBatFlip()
        {
            return MyMath.Half();
        }

        /// <summary>
        /// 컨택에 따른 파워계수
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static float getContactPowerCoef(BattingContact contact)
        {
            if (contact == BattingContact.SOLID)
            {
                return 1.0f;// powerCoef1 = 1.0f;
            }
            else if (contact == BattingContact.GOOD)
            {
                return Random.Range(0.9f, 0.95f);
            }
            else if (contact == BattingContact.NORMAL)
            {
                return Random.Range(0.8f, 0.9f);
            }
            else
            {
                return Random.Range(0.6f, 0.8f);
            }
        }

        /// <summary>
        /// 타이밍에 따른 파워계수
        /// </summary>
        /// <param name="timing"></param>
        /// <returns></returns>
        public static float getTimingPowerCoef(BattingTiming timing)
        {
            if (timing == BattingTiming.PERFECT)
            {
                return 1.1f;
            }
            else if (timing == BattingTiming.JUST_EARLY || timing == BattingTiming.JUST_LATE)
            {
                return Random.Range(0.95f, 1.05f);
            }
            else if (timing == BattingTiming.EARLY || timing == BattingTiming.LATE)
            {
                return Random.Range(0.85f, 0.95f);
            }
            else
            {
                return Random.Range(0.65f, 0.75f);
            }
        }

#if _RewindMode
        //시뮬레이션에 의한 배팅 파워 구하기
        public static float GetBallPowerAuto(SimulBattingData bResult)
        {
            SimulHitType hitType = bResult.hitType;
            SpecificFlyType flyType = bResult.flyType;
            SpecificGrounderType grounderType = bResult.grounderType;
            SpecificLinerType linerType = bResult.linerType;

            float val = 20;

            if (hitType == SimulHitType.Fly)
            {
                if (flyType == SpecificFlyType.InfieldPopup_Fair)
                {
                    val = Random.Range(20.0f, 30.0f);
                }
                else if (flyType == SpecificFlyType.InfieldPopup_Foul)
                {
                    val = Random.Range(20.0f, 30.0f);
                }
                else if (flyType == SpecificFlyType.CatcherPopup)
                {
                    val = Random.Range(20.0f, 30.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldPopup)
                {
                    val = Random.Range(25.0f, 30.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldShort)
                {
                    val = Random.Range(23.0f, 28.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldHighFly)
                {
                    val = Random.Range(28.0f, 33.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldOverHead)
                {
                    val = Random.Range(31.0f, 34.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldHomerun)
                {
                    val = Random.Range(32.0f, 38.0f);
                }
            }
            else if (hitType == SimulHitType.Grounder)
            {
                if (grounderType == SpecificGrounderType.Weak)
                {
                    val = Random.Range(11.0f, 18.0f);
                }
                else if (grounderType == SpecificGrounderType.BigBound)
                {
                    val = Random.Range(20.0f, 28.0f);
                }
                else if (grounderType == SpecificGrounderType.Normal)
                {
                    val = Random.Range(22.0f, 28.0f);
                }
                else if (grounderType == SpecificGrounderType.Solid)
                {
                    val = Random.Range(27.0f, 35.0f);
                }
                else if (grounderType == SpecificGrounderType.VerySolid)
                {
                    val = Random.Range(34.0f, 40.0f);
                }

            }
            else
            {
                if (linerType == SpecificLinerType.Weak)
                {
                    val = Random.Range(18.0f, 27.0f);
                }
                else if (linerType == SpecificLinerType.Normal)
                {
                    val = Random.Range(27.0f, 30.0f);
                }
                else if (linerType == SpecificLinerType.Solid)
                {
                    val = Random.Range(30.0f, 34.0f);
                }
                else if (linerType == SpecificLinerType.VerySolid)
                {
                    val = Random.Range(34.0f, 40.0f);
                }
            }

            return val;
        }


        //시뮬레이션에 의한 배팅 앵글 구하기
        public static float GetBallAngleAuto(SimulBattingData bResult)
        {
            SimulHitType hitType = bResult.hitType;
            SpecificFlyType flyType = bResult.flyType;
            SpecificGrounderType grounderType = bResult.grounderType;
            SpecificLinerType linerType = bResult.linerType;

            float val = 30;
            if (hitType == SimulHitType.Fly)
            {
                if (flyType == SpecificFlyType.InfieldPopup_Fair)
                {
                    val = Random.Range(60.0f, 70.0f);
                }
                else if (flyType == SpecificFlyType.InfieldPopup_Foul)
                {
                    val = Random.Range(60.0f, 70.0f);
                }
                else if (flyType == SpecificFlyType.CatcherPopup)
                {
                    val = Random.Range(75.0f, 79.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldPopup)
                {
                    val = Random.Range(30.0f, 45.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldShort)
                {
                    val = Random.Range(25.0f, 35.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldHighFly)
                {
                    val = Random.Range(25.0f, 55.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldOverHead)
                {
                    val = Random.Range(25.0f, 40.0f);
                }
                else if (flyType == SpecificFlyType.OutfieldHomerun)
                {
                    val = Random.Range(35.0f, 45.0f);
                }
            }
            else if (hitType == SimulHitType.Grounder)
            {
                if (grounderType == SpecificGrounderType.Weak)
                {
                    val = Random.Range(-40f, 10.0f);
                }
                else if (grounderType == SpecificGrounderType.BigBound)
                {
                    val = Random.Range(-60f, -40.0f);
                }
                else if (grounderType == SpecificGrounderType.Normal)
                {
                    val = Random.Range(-35f, 10.0f);
                }
                else if (grounderType == SpecificGrounderType.Solid)
                {
                    val = Random.Range(-25f, 5.0f);
                }
                else if (grounderType == SpecificGrounderType.VerySolid)
                {
                    val = Random.Range(-25f, 5.0f);
                }

            }
            else
            {
                if (linerType == SpecificLinerType.Weak)
                {
                    val = Random.Range(10.0f, 25.0f);
                }
                else if (linerType == SpecificLinerType.Normal)
                {
                    val = Random.Range(10.0f, 25.0f);
                }
                else if (linerType == SpecificLinerType.Solid)
                {
                    val = Random.Range(7.0f, 25.0f);
                }
                else if (linerType == SpecificLinerType.VerySolid)
                {
                    val = Random.Range(7.0f, 20.0f);
                }
            }
            return val;
        }

        //시뮬레이션에 의한 배팅 방향 구하기
        public static float GetBallAngleDir(SimulBattingData bResult)
        {
            int fIndex = bResult.fIndex;
            float val = 0.0001f;// Random.Range(-44.0f, 44.0f);
                        
            if (fIndex == CPlayer._PITCHER)
            {
                val = Random.Range(-5.0f, 5.0f);
            }
            else if (fIndex == CPlayer._CATCHER)
            {
                val = Random.Range(-44.0f, 44.0f);
            }
            else if (fIndex == CPlayer._FIRSTBASEMAN)
            {
                val = Random.Range(-44.0f, -28.0f);
            }
            else if (fIndex == CPlayer._SECONDBASEMAN)
            {
                val = Random.Range(-28.0f, -0.0001f);
            }
            else if (fIndex == CPlayer._THIRDBASEMAN)
            {
                val = Random.Range(28.0f, 44.0f);
            }
            else if (fIndex == CPlayer._SHORTSTOP)
            {
                val = Random.Range(0.00001f, 28.0f);
            }
            else if (fIndex == CPlayer._LEFTFIELDER)
            {
                val = Random.Range(20.0f, 44.0f);
            }
            else if (fIndex == CPlayer._CENTERFIELDER)
            {
                val = Random.Range(-20.0f, 20.0f);
            }
            else // if (fIndex == CPlayer._RIGHTFIELDER)
            {
                val = Random.Range(-44.0f, -20.0f);
            }



            if (val == 0) val = 0.001f;
            return val;
        }
#endif

        //////////////////////////////////////////////////////////////////////
        //애니메이션 스트링값
        //////////////////////////////////////////////////////////////////////
        public const string NORMAL_FULL_SWING = "EX_SWING_NORMAL";
        public const string BROKEN_SWING = "3021_FOLLOWTHROW_BAT_BROKEN";
        public const string CORRECT_TIMING_SWING = "2000_HIT";
        public const string EARLY_TIMING_SWING = "2010_EARLYSWING_01";
        public const string FOLLOW_THROW = "FOLLOWTHROW";
        public const string CHECK_SWING = "2020_SWING_CHECK";
        public const string HUT_SWING_BRRRRR = "5003_EVENT_LOOKING4";
        public const string HUT_EARLY_TIMING_SWING = "2010_EARLYSWING_OUT";

        public const int MAX_ATBAT = 20;
    }
}
