using UnityEngine;
using System.Collections;
using WebConnector;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class SimulParm
    {
        //
        public const int MAX_OUTCOUNT = 3;
        public const int MAX_BALLCOUNT = 4;
        public const int MAX_STRIKECOUNT = 3;

        //맥스 AP
        public const int MAX_AP = 100;

        public const int FIRSTBASE_INDEX = 0;
        public const int SECONDBASE_INDEX = 1;
        public const int THIRDBASE_INDEX = 2;
        public const int HOMEBASE_INDEX = 3;

        const int NOBALL_NOSTRIKE = 0;
        const int NOBALL_ONESTRIKE = 1;
        const int NOBALL_TWOSTRIKE = 2;
        const int ONEBALL_NOSTRIKE = 10;
        const int ONEBALL_ONESTRIKE = 11;
        const int ONEBALL_TWOSTRIKE = 12;
        const int TWOBALL_NOSTRIKE = 20;
        const int TWOBALL_ONESTRIKE = 21;
        const int TWOBALL_TWOSTRIKE = 22;
        const int THREEBALL_NOSTRIKE = 30;
        const int THREEBALL_ONESTRIKE = 31;
        const int THREEBALL_TWOSTRIKE = 32;


        //
        public const int NOPLAY_INNING = -100;
        public const int GAMEEND_INNING = -99;



        ///////////////////////////////////////////////////////////////////////////
        //기타 교체
        ///////////////////////////////////////////////////////////////////////////
        public const int RUNNER_CHANGE_INNING = 8; //대주자 교체 시점
        public const int BATTER_CHANGE_INNING = 6; //대타 교체 시점


        //////////////////////////////////////////////////////////////////////////////
        //밸런스 파라메터
        //////////////////////////////////////////////////////////////////////////////
        const int MAX_PARAM_VALUE = 1000;      //능력치의 최대값
        const int Pitcher_Strike_Value = 450; //이값보다 작은 경우 볼을 던지고 큰경우 스트라이크를 던진다
        public const int Contact_Foul_Max_Value = 850;   //0~Max 레인지가 기본 컨택값보다 적은 경우 파울이 나옴 

        //파워+컨택 - (구위+컨트롤) 을 이용해 만든 공식의 계산값이 이값보다 높으면 파워 힛
        const int POWER_HIT_VALUE = 2000; //이값이 크면 작을수록 장타가 늘어남
        const int CONTACT_RANGE = 1200;   //이값이 크면 작을수록 컨택형 안타확률이 늘어난다
        const int POWER_RANGE = 2500;//3500;     //이값이 크면 작을수록 홈런 확률이 늘어난다(파워 인플레의 영향을 가장 밀접하게 받을듯) - 업데이트시 참조
                
        //플라이 수비 레인지 - 이값들은 모두 크면 클수록 타자에게 유리 수비에게 불리
        const int FLY_SHORT_CATCH_RANGE = 2500;// 2000;    //이값이 크면 클수록 짧은플라이 안타 확률이 높아짐 (수비의 관점)
        const int FLY_HIGH_CATCH_RANGE = 3000;//2500;     //이값이 크면 클수록 하이플라이 안타 확률이 높아짐 (수비의 관점) 
        const int FLY_OVER_CATCH_RANGE = 3500;//3000;     //이값이 크면 클수록 아주큰플라이 안타 확률이 높아짐 (수비의 관점)  
        
        //라이너 수비 에러율
        const int LINER_CATCH_ERROR = -1;//2;    //직선타 캐치의 에러율 - 크면클수록 에러가 증가
        const int LINER_BOUND_ERROR = -1;//2;    //직선타 캐치의 에러율 - 크면클수록 에러가 증가

        //라이너 수비 레인지  - 이값들은 모두 크면 클수록 타자에게 유리 수비에게 불리
        const int LINER_NORMAL_CATCH_RANGE = 1400;// 1000;          //이값이 크면 클수록 노멀라이너 안타 확률이 높아짐 (수비의 관점)
        const int LINER_SOLID_CATCH_RANGE = 3000;// 2500;           //이값이 크면 클수록 강한라이너 안타 확률이 높아짐 (수비의 관점)
        const int LINER_VERY_SOLID_CATCH_RANGE = 3500;// 3200;      //이값이 크면 클수록 매우강한라이너 안타 확률이 높아짐 (수비의 관점)
        const int LINER_JUMPING_CATCH_SKILL_RANGE = 1000;   //이값이 작으면 작을 수록 점핑 캐치 확률 높아짐 (수비의 관점)  

        //그라운더 수비 에러율
        const int GROUNDER_CATCH_ERROR = -1;//3; //땅볼 캐치의 에러율 - 크면클수록 에러가 증가
        const int GROUNDER_THROW_ERROR = -1;//3; //땅볼 캐치의 에러율 - 크면클수록 에러가 증가

        //그라운더 수비 레인지
        const int GROUNDER_WEAK_RANGE = 800;               //이값이 크면 클수록 큰바운드 땅볼 안타 확률이 높아짐 (수비의 관점) 
        const int GROUNDER_BIGBOUND_RANGE = 1200;           //이값이 크면 클수록 큰바운드 땅볼 안타 확률이 높아짐 (수비의 관점) 
        const int GROUNDER_NORMAL_RANGE = 1600;//1200;             //이값이 크면 클수록 노멀바운드 안타 확률이 높아짐 (수비의 관점)
        const int GROUNDER_SOLID_RANGE = 2600; //1800             //이값이 크면 클수록 강한바운드 안타 확률이 높아짐 (수비의 관점)
        const int GROUNDER_VERYSOLID_RANGE = 3200;//2400          //이값이 크면 클수록 매우강한바운드 안타 확률이 높아짐 (수비의 관점)
     
        
        //한베이스 더 확률
        private const int ONEMOREBASE_CHECK_PERCENT = 15;


        //////////////////////////////////////////////////////////////////////////////
        //타격
        //////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 컨택된 공이 HitType: Grounder, Liner, Fly
        /// </summary>    
        public static SimulHitType GetHitType(int tando, int guwee, int contact, int power, int pitcherPower)
        {
            //Grounder 45%
            //Liner :20%
            //Fly 35%
          

            int max = 1000; //tando

            int flyBonus = (tando - guwee);
            if (flyBonus > 250) flyBonus = 250;
            else if (flyBonus < -200) flyBonus = -200;

            int linerBonus = (contact + power - pitcherPower);
            if (linerBonus > 100) linerBonus = 100;
            else if (linerBonus < -100) linerBonus = -100;

            //Debug.Log("[기초값]================== [contact] = " + contact + " [power] = " + power + " [control] = " + control + " [guwee] = " + guwee);
            //Debug.Log("[플라이 보너스]================== flyBonus = " + flyBonus);
            //Debug.Log("[라이너 보너스]================== linerBonus = " + linerBonus);

            int range = Random.Range(0, (max + flyBonus));

            //Debug.Log("[힛타입 레인지]================== range(" + range + ") / MAX(" + (max + flyBonus));

            if (range > 650)
            {
                return SimulHitType.Fly;
            }
            else if (range > (450 - linerBonus))
            {
                return SimulHitType.Liner;
            }


            return SimulHitType.Grounder;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////
        //플라이 타구
        ////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 플라이 타구의 디테일을 얻어옴
        /// </summary>    
        public static SpecificFlyType GetFlySpecificType(float curPower, int tandoValue)
        {
#if _Local_Balance
            if (InGameDebug._SPECIFIC_FLY_TEST == true)
            {
                //로컬 밸런스에 의한 특정 플라이볼 발생 테스트
                //return SpecificFlyType.InfieldPopup_Fair;
                return SpecificFlyType.OutfieldHomerun;
            }
#endif

            /*
            InfieldPopup_Fair,      //거의 아웃, 에러가능성도 적음
            InfieldPopup_Foul,      //거의 아웃, 에러가능성도 적음
            CatcherPopup,           //거의 아웃, 에러가능성도 적음
            OutfieldPopup,          //거의 아웃, 에러가능성도 적음  , 베이스택 가능성 있음
            OutfieldShort,          //단타 가능성 높음 , 에러가능성 1단계 up
            OutfieldHighFly,        //단타 장타 가능성 있음 , 에러가능성 적음, 베이스택 가능성 높음
            OutfieldOverHead,       //장타 가능성 높음 , 에러가능성 1단계 up, 베이스택 가능성 높음
            OutfieldHomerun         //홈런*/


            if (curPower < 24)
            {
                //팝업
                int cIndex = (int)SpecificFlyType.InfieldPopup_Fair;
                return (SpecificFlyType)Random.Range(cIndex, cIndex + 4);  
            }
            else if (curPower > 32)
            {
                //홈런 혹은 장타
                int range = Random.Range(0, BattingMechanism.TANDO_RANGE);  //0~3000
                bool noTandoEffect = (range < tandoValue ? true : false);
                if (noTandoEffect == true)
                {
                    //파워가 높을수록 홈런확률 높아짐
                    return SpecificFlyType.OutfieldHomerun;
                }
                else
                {
                    //외야 머리를 넘어가는 플라이
                    return SpecificFlyType.OutfieldOverHead;
                }
            }
            else
            {
                int range =  Random.Range(0, POWER_HIT_VALUE);

                int power, well;
                if (curPower > 29)
                {
                    power = 1200;
                    well = 500;
                }
                else if (curPower > 26)
                {
                    power = 1700;
                    well = 900;
                }
                else
                {
                    power = 1950;
                    well = 900;
                }

                if (range > power)
                {
                    //파워가 높을수록 홈런확률 높아짐
                    return SpecificFlyType.OutfieldOverHead;
                }
                else if (range > well)
                {
                    //파워가 높을수록 홈런확률 높아짐
                    return SpecificFlyType.OutfieldHighFly;
                }
                else
                {
                    //외야 머리를 넘어가는 플라이
                    return SpecificFlyType.OutfieldShort;
                }
            }


        }
                
        /// <summary>
        /// 플라이 타구를 처리하는 야수값을 얻어옴
        /// </summary>    
        public static int GetFlyCatchFieder(SpecificFlyType flyType)
        {
            int fielder = 0;
            if (flyType == SpecificFlyType.InfieldPopup_Fair)
            {
                if (MyMath.Percent() < 65)
                {
                    fielder = MyMath.Half() ? CPlayer._SHORTSTOP : CPlayer._SECONDBASEMAN;
                }
                else
                {
                    fielder = MyMath.Half() ? CPlayer._THIRDBASEMAN : CPlayer._FIRSTBASEMAN;
                }
            }
            else if (flyType == SpecificFlyType.InfieldPopup_Foul)
            {
                fielder = MyMath.Half() ? CPlayer._THIRDBASEMAN : CPlayer._FIRSTBASEMAN;
            }
            else if (flyType == SpecificFlyType.CatcherPopup)
            {
                fielder = CPlayer._CATCHER;
            }
            else
            {
                //OutfieldPopup, OutfieldHighFly, OutfieldOverHead, OutfieldHomerun
                if (MyMath.Half())
                {
                    fielder = MyMath.Percent() < 64 ? CPlayer._LEFTFIELDER : CPlayer._CENTERFIELDER;
                }
                else
                {
                    fielder = MyMath.Percent() < 64 ? CPlayer._RIGHTFIELDER : CPlayer._CENTERFIELDER;
                }
            }

            return fielder;
        }
                
        /// <summary>
        /// 플라이 타구를 어떻게 처리 할지 여부를 판단하는 함수
        /// </summary>    
        public static FlyCatchType GetFlyCatchType(SpecificFlyType flyType, SimulFielder fielder)
        {
            //필딩 능력치
            int fielding = fielder.getFieldingAbil();//
            //타구 판단과 레인지에 따른 추가
            int addFielding = fielder.addFieldingAbil(fielding);

            fielding += addFielding;

            //필딩 레인지
            int fieldingRange;

            //외야 다이빙 캐치
            bool bDivingSuccess = fielder.checkSkillOn(SkillIndex.DivingCatch);
            //에러세팅
            bool bCatchError = fielder.isCatchError();

            if (flyType == SpecificFlyType.InfieldPopup_Fair ||      //인필드 팝업
              flyType == SpecificFlyType.InfieldPopup_Foul ||       //인필드 파울 팝업
              flyType == SpecificFlyType.CatcherPopup ||            //캐쳐 팝업    
              flyType == SpecificFlyType.OutfieldPopup)             //아웃필드 팝업
            {
                if (bCatchError)
                {
                    //포구 에러
                    return FlyCatchType.CATCH_ERROR;
                }
                else
                {
                    //노멀하게 포구
                    return FlyCatchType.Normal;
                }
            }
            else if (flyType == SpecificFlyType.OutfieldShort)
            {
                //짧은 플라이시
                //fielding이 크면 클 수록 안타확률 적어짐
                fieldingRange = Random.Range(0, FLY_SHORT_CATCH_RANGE);
                //Debug.Log[짧은 플라이 필딩] =====>>> 맥스 "+FLY_SHORT_CATCH_RANGE+ " vs (필드기본값) ::" + fieldingRange + " vs " + (fielding));
                if (fieldingRange > fielding)
                {
                    //안타 케이스
                    if (bCatchError)
                    {
                        //바운드 에러 - 1힛 1에러
                        return FlyCatchType.BOUND_ERROR;
                    }
                    else
                    {
                        //if (Random.Range(0, FLY_DIVING_SKILL_RANGE) < divingCatch)
                        if(bDivingSuccess == true)
                        {
                            //특능 이펙트!!!!
                            //다이빙 캐치 스킬을 발동하여 잡음 - 아웃
                            return FlyCatchType.DivingCatch;
                        }
                        else
                        {
                            if (MyMath.Percent() < 40)
                            {
                                //평범하게 바운드 처리
                                return FlyCatchType.NormalBound;
                            }
                            else
                            {
                                //움직이며 바운드 처리                 
                                int cIndex = (int)FlyCatchType.DashBound;
                                return (FlyCatchType)Random.Range(cIndex, cIndex + 2);  //DashRun ~SideBound
                            }
                        }
                    }
                }
                else
                {
                    //아웃 케이스
                    if (bCatchError)
                    {
                        //포구 에러 - no hit 1 에러
                        return FlyCatchType.CATCH_ERROR;
                    }
                    else
                    {
                        //아웃 - 달려오며 잡음
                        return FlyCatchType.DashRun;
                    }
                }
            }
            else if (flyType == SpecificFlyType.OutfieldHighFly)
            {
                //높은 플라이시
                fieldingRange = Random.Range(0, FLY_HIGH_CATCH_RANGE);
                //Debug.Log[하이 플라이 필딩] =====>>> 맥스 "+FLY_HIGH_CATCH_RANGE+" vs (필드기본값+1500) ::" + fieldingRange + " vs " + (fielding + 1500));
                if (fieldingRange > (fielding + 1500))
                {
                    //안타 케이스
                    if (bCatchError)
                    {
                        //바운드 에러 발생 - 1힛 1에러
                        return FlyCatchType.BOUND_ERROR;
                    }
                    else
                    {
                        if (bDivingSuccess == true)
                        {
                            //특능 이펙트!!!!
                            //다이빙 캐치 스킬을 발동하여 잡음 - 아웃
                            return FlyCatchType.DivingCatch;
                        }
                        else
                        {
                            //안타 
                            if (MyMath.Percent() < 40)
                            {
                                //평범하게 바운드 처리
                                return FlyCatchType.NormalBound;
                            }
                            else
                            {
                                //옆으로 움직이며 바운드 처리                      
                                return FlyCatchType.SideBound;
                            }
                        }
                    }
                }
                else
                {
                    //아웃 케이스
                    if (bCatchError)
                    {
                        //포구 에러
                        return FlyCatchType.CATCH_ERROR;
                    }
                    else
                    {
                        //아웃
                        if (MyMath.Percent() < 30)
                        {
                            //제자리서 포구
                            return FlyCatchType.Normal;
                        }
                        else
                        {
                            //움직이며 포구
                            int cIndex = (int)FlyCatchType.DashRun;
                            return (FlyCatchType)Random.Range(cIndex, cIndex + 4);  //DashRun ~ RIghtRun
                        }
                    }
                }
            }
            else if (flyType == SpecificFlyType.OutfieldOverHead)
            {
                //오버해드
                fieldingRange = Random.Range(0, FLY_OVER_CATCH_RANGE);
                //Debug.Log[오버해드 플라이 필딩] =====>>> 맥스 "+FLY_OVER_CATCH_RANGE+" vs (필드기본값+300) ::" + fieldingRange + " vs " + (fielding + 300));
                if (fieldingRange > (fielding + 300))
                {
                    //안타 케이스
                    if (bDivingSuccess == true)
                    {
                        //특능 이펙트!!!!
                        //다이빙 캐치 스킬을 발동하여 잡음
                        return FlyCatchType.DivingCatch;
                    }
                    else
                    {
                        //안타
                        if (bCatchError)
                        {
                            //바운드 에러 발생 - 1힛 1에러
                            return FlyCatchType.BOUND_ERROR;
                        }
                        else
                        {
                            //펜스로 잡음
                            return FlyCatchType.FenceBound;
                        }
                    }
                }
                else
                {
                    //아웃 케이스
                    if (bCatchError)
                    {
                        //포구 에러
                        return FlyCatchType.CATCH_ERROR;
                    }
                    else
                    {
                        //아웃
                        return FlyCatchType.OverHeadRun;
                    }
                }
            }
            else
            {
                if (fielder.checkSkillOn(SkillIndex.HomerunSteal) == true)
                {
                    //홈런 스틸
                    return FlyCatchType.HomeRunSteal;
                }
                else
                {
                    return FlyCatchType.HomeRun;
                }
            }

        }
                
        /// <summary>
        /// 플라이 타구와 주자의 주루 능력에 따른 최종결과
        /// </summary>    
        public static SimulResultState GetFlyHitType(SpecificFlyType flyType, FlyCatchType flyCatchType, CPlayer runner, CPlayer fielder)
        {
            int runningAbil = runner.getSpeed();// +runner.getSpeedBonus();
            bool bOnemoreBase = false;

            if (flyCatchType == FlyCatchType.SideBound || flyCatchType == FlyCatchType.FenceBound)
            {
                if (MyMath.Percent() < ONEMOREBASE_CHECK_PERCENT) //15
                {
                    //타자주자의 한베이스더 -> 레이저송구와 관련없다
                    if (SimulBaseRunning.checkGetOneMoreBase(runner, fielder, true) != SimulOverrunState.NONE)
                    {
                        bOnemoreBase = true;
                    }
                }
            }

            if (flyCatchType == FlyCatchType.DashRun ||
               flyCatchType == FlyCatchType.OverHeadRun ||
               flyCatchType == FlyCatchType.LeftRun ||
               flyCatchType == FlyCatchType.RIghtRun ||
               flyCatchType == FlyCatchType.HomeRunSteal ||
               flyCatchType == FlyCatchType.DivingCatch ||
               flyCatchType == FlyCatchType.Normal)
            {
                return SimulResultState.FlyOut;
            }
            else if (flyCatchType == FlyCatchType.DashBound ||
                   flyCatchType == FlyCatchType.SideBound ||
                   flyCatchType == FlyCatchType.NormalBound ||
                   flyCatchType == FlyCatchType.FenceBound)
            {
                if (flyType == SpecificFlyType.OutfieldOverHead)
                {
                    if (bOnemoreBase == true)
                    {
                        return SimulResultState.Triple;
                    }
                    else
                    {
                        return SimulResultState.Double;
                    }
                }
                else if (flyType == SpecificFlyType.OutfieldHighFly)
                {
                    if (bOnemoreBase == true)
                    {
                        return SimulResultState.Double;
                    }
                    else
                    {
                        return SimulResultState.Single;
                    }
                }
                else //if (flyType == SpecificFlyType.OutfieldShort)
                {
                    return SimulResultState.Single;
                }
            }
            else if (flyCatchType == FlyCatchType.HomeRun)
            {
                return SimulResultState.HomeRun;
            }
            else if (flyCatchType == FlyCatchType.BOUND_ERROR)
            {
                if (flyType == SpecificFlyType.OutfieldOverHead)
                {
                    return SimulResultState.DoubleOneError;
                }
                else
                {
                    if (bOnemoreBase == true)
                    {
                        return SimulResultState.DoubleOneError;
                    }
                    else
                    {
                        return SimulResultState.SingleOneError;
                    }
                }
            }
            else if (flyCatchType == FlyCatchType.CATCH_ERROR)
            {
                return SimulResultState.CatchError;
            }
            return SimulResultState.FlyOut;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////
        //라인너 타구
        ////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 라이너의 타구 디테일을 얻어옴
        /// </summary>      
        public static SpecificLinerType GetLinerSpecificType(float curPower)
        {

            if (curPower > 30)
            {
                return SpecificLinerType.VerySolid;
            }
            else if (curPower > 28)
            {
                return SpecificLinerType.Solid;
            }
            else if (curPower > 25)
            {
                return SpecificLinerType.Normal;
            }
            else
            {
                return SpecificLinerType.Weak;
            }
        }

        /// <summary>
        /// 라이너 타구를 처리하는 야수값을 얻어옴
        /// </summary>      
        public static int GetLinerCatchFieder(SpecificLinerType linerType)
        {
            if (linerType == SpecificLinerType.VerySolid)
            {
                //외야로 감
                return Random.Range(CPlayer._LEFTFIELDER, CPlayer._RIGHTFIELDER + 1);
            }
            else if (linerType == SpecificLinerType.Solid)
            {
                //내야~외야
                return Random.Range(CPlayer._FIRSTBASEMAN, CPlayer._RIGHTFIELDER + 1);
            }
            else if (linerType == SpecificLinerType.Normal)
            {
                //내야~외야
                return Random.Range(CPlayer._FIRSTBASEMAN, CPlayer._RIGHTFIELDER + 1);
            }
            else //if (linerType == SpecificLinerType.Solid)
            {
                //내야
                return Random.Range(CPlayer._FIRSTBASEMAN, CPlayer._SHORTSTOP + 1);
            }
        }
                
        /// <summary>
        /// 라이너 타구를 어떻게 처리 할지 여부를 판단하는 함수
        /// </summary>      
        public static FlyCatchType GetLinerCatchType(SpecificLinerType linerType, SimulFielder fielder, int posIndex)
        {
            int fielding = fielder.getFieldingAbil();
            //타구 판단과 레인지에 따른 추가
            int addFielding = fielder.addFieldingAbil(fielding);

            fielding += addFielding;


            int fieldingRange;

            //스킬
            bool bJumpingCatch = false;
            if (posIndex < CPlayer._LEFTFIELDER)
            {
                bJumpingCatch = fielder.checkSkillOn(SkillIndex.SpecialCatch);
            }
            //에러
            bool bCatchError = fielder.isCatchError();

            if (linerType == SpecificLinerType.Weak)
            {
                if (bCatchError == true)
                {
                    return FlyCatchType.CATCH_ERROR;
                }
                else
                {
                    return FlyCatchType.Normal;
                }
            }
            else if (linerType == SpecificLinerType.Normal)
            {
                //보통 라이너
                //fielding이 크면 클 수록 안타확률 적어짐
                fieldingRange = Random.Range(0, LINER_NORMAL_CATCH_RANGE);
                //Debug.Log[노멀 라이너 비교] =====>>> 맥스 "+LINER_NORMAL_CATCH_RANGE+" vs (필드기본값+400) ::" + fieldingRange + " vs " + (fielding + 400));
                if (fieldingRange > (fielding + 400))
                {
                    //안타 케이스
                    if (bCatchError)
                    {
                        //바운드 에러 - 1힛 1에러
                        return FlyCatchType.BOUND_ERROR;
                    }
                    else
                    {
                        if (bJumpingCatch == true)
                        {
                            //점핑 캐치 스킬을 발동하여 잡음 - 아웃
                            return FlyCatchType.JumpingCatch;
                        }
                        else
                        {
                            //바운드 처리
                            return FlyCatchType.DashBound;
                        }
                    }
                }
                else
                {
                    //아웃 케이스
                    if (bCatchError)
                    {
                        //포구 에러 - no hit 1 에러
                        return FlyCatchType.CATCH_ERROR;
                    }
                    else
                    {
                        //아웃 - 달려오며 잡음
                        return FlyCatchType.DashRun;
                    }
                }
            }
            else if (linerType == SpecificLinerType.Solid)
            {
                //솔리드 라이너
                //fielding이 크면 클 수록 안타확률 적어짐
                fieldingRange = Random.Range(0, LINER_SOLID_CATCH_RANGE);
                //Debug.Log[솔리드 라이너 비교] =====>>> 맥스 "+LINER_SOLID_CATCH_RANGE+" vs (필드기본값+600) ::" + fieldingRange + " vs " + (fielding + 600));
                if (fieldingRange > (fielding + 600))
                {
                    //안타 케이스
                    if (bCatchError)
                    {
                        //바운드 에러 - 1힛 1에러
                        return FlyCatchType.BOUND_ERROR;
                    }
                    else
                    {
                        if (bJumpingCatch == true)
                        {
                            //점핑 캐치 스킬을 발동하여 잡음 - 아웃
                            return FlyCatchType.JumpingCatch;
                        }
                        else
                        {
                            //바운드 처리
                            return FlyCatchType.NormalBound;
                        }
                    }
                }
                else
                {
                    //아웃 케이스
                    if (bCatchError)
                    {
                        //포구 에러 - no hit 1 에러
                        return FlyCatchType.CATCH_ERROR;
                    }
                    else
                    {
                        //아웃 - 제자리서
                        return FlyCatchType.Normal;
                    }
                }
            }
            else if (linerType == SpecificLinerType.VerySolid)
            {
                //very 솔리드 라이너
                //fielding이 크면 클 수록 안타확률 적어짐
                fieldingRange = Random.Range(0, LINER_VERY_SOLID_CATCH_RANGE);
                //Debug.Log[very 솔리드 라이너 비교] =====>>> 맥스 "+LINER_VERY_SOLID_CATCH_RANGE+" vs (필드기본값+600) ::" + fieldingRange + " vs " + (fielding + 600));
                if (fieldingRange > (fielding + 600))
                {
                    //안타 케이스
                    if (bCatchError)
                    {
                        //바운드 에러 - 1힛 1에러
                        return FlyCatchType.BOUND_ERROR;
                    }
                    else
                    {
                        if (bJumpingCatch == true)
                        {
                            //점핑 캐치 스킬을 발동하여 잡음 - 아웃
                            return FlyCatchType.JumpingCatch;
                        }
                        else
                        {
                            //바운드 처리
                            return FlyCatchType.SideBound;
                        }
                    }
                }
                else
                {
                    //아웃 케이스
                    if (bCatchError)
                    {
                        //포구 에러 - no hit 1 에러
                        return FlyCatchType.CATCH_ERROR;
                    }
                    else
                    {
                        //아웃 - 달려오며 잡음
                        return FlyCatchType.LeftRun;
                    }
                }
            }


            return FlyCatchType.Normal;
        }
                
        /// <summary>
        /// 라이너 타구와 주자의 주루 능력에 따른 최종결과
        /// </summary>      
        public static SimulResultState GetLinerHitType(SpecificLinerType linerType, FlyCatchType flyCatchType, CPlayer runner, CPlayer fielder)
        {
            int runningAbil = runner.getSpeed();// +runner.getSpeedBonus();
            
            bool bOnemoreBase = false;
            if (flyCatchType == FlyCatchType.SideBound)
            {
                if (MyMath.Percent() < ONEMOREBASE_CHECK_PERCENT) //15
                {
                    //타자주자의 한베이스더 -> 레이저송구와 관련없다
                    if (SimulBaseRunning.checkGetOneMoreBase(runner, fielder, true) != SimulOverrunState.NONE)
                    {
                        bOnemoreBase = true;
                    }
                }
            }

            if (flyCatchType == FlyCatchType.DashRun ||
               flyCatchType == FlyCatchType.OverHeadRun ||
               flyCatchType == FlyCatchType.LeftRun ||
               flyCatchType == FlyCatchType.RIghtRun ||
               flyCatchType == FlyCatchType.HomeRunSteal ||
               flyCatchType == FlyCatchType.DivingCatch ||
               flyCatchType == FlyCatchType.Normal)
            {
                return SimulResultState.LineOut;
            }
            else if (flyCatchType == FlyCatchType.DashBound ||
                   flyCatchType == FlyCatchType.SideBound ||
                   flyCatchType == FlyCatchType.NormalBound ||
                   flyCatchType == FlyCatchType.FenceBound)
            {
                if (linerType == SpecificLinerType.VerySolid)
                {
                    if (bOnemoreBase == true)
                    {
                        return SimulResultState.Triple;
                    }
                    else
                    {
                        return SimulResultState.Double;
                    }
                }
                else if (linerType == SpecificLinerType.Solid)
                {
                    if (bOnemoreBase == true)
                    {
                        return SimulResultState.Double;
                    }
                    else
                    {
                        return SimulResultState.Single;
                    }
                }
                else //if (flyType == SpecificFlyType.OutfieldShort)
                {
                    return SimulResultState.Single;
                }
            }
            else if (flyCatchType == FlyCatchType.BOUND_ERROR)
            {
                if (linerType == SpecificLinerType.VerySolid)
                {
                    return SimulResultState.DoubleOneError;
                }
                else
                {
                    if (bOnemoreBase == true)
                    {
                        return SimulResultState.DoubleOneError;
                    }
                    else
                    {
                        return SimulResultState.SingleOneError;
                    }
                }
            }
            else if (flyCatchType == FlyCatchType.CATCH_ERROR)
            {
                return SimulResultState.CatchError;
            }
            return SimulResultState.LineOut;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //그라운더 타구
        ////////////////////////////////////////////////////////////////////////////////////////        
        
        /// <summary>
        /// 그라운더 타구 디테일을 얻어옴
        /// </summary>      
        public static SpecificGrounderType GetGrounderSpecificType(float curPower)
        {

            /*
            Weak,               //스페셜 송구  , (캐치시)병살 2단계 down 
            BigBound,           //스페셜 송구  , (캐치시)병살 1단계 down
            Normal,
            Solid,              //스페셜 캐치  , (캐치시)병살 1단계 up
            VerySolid           //스페셜 캐치  , (캐치시)병살 2단계 up*/

            //Debug.Log============================>>powerGab = " + powerGab);
            if (curPower > 32)
            {
                return SpecificGrounderType.VerySolid;
            }
            else if (curPower > 29)
            {
                return SpecificGrounderType.Solid;
            }
            else if (curPower > 25)
            {
                return SpecificGrounderType.Normal;
            }
            else
            {
                if (MyMath.Percent() < 20)
                {
                    return SpecificGrounderType.BigBound;
                }
                else
                {
                    return SpecificGrounderType.Weak;
                }
            }
        }

        
        /// <summary>
        /// 그라운더 타구를 처리하는 야수값을 얻어옴
        /// </summary>      
        public static int GetGrounderCatchFieder(SpecificGrounderType grounderType)
        {
            int range = Random.Range(0, 1000);
           
                if (range > 740)
                {
                    //26%
                    return CPlayer._SHORTSTOP;
                }
                else if (range > 530)
                {
                    //21%
                    return CPlayer._SECONDBASEMAN;
                }
                else if (range > 320)
                {
                    //21%
                    return CPlayer._FIRSTBASEMAN;
                }
                else if (range > 110)
                {
                    //21%
                    return CPlayer._THIRDBASEMAN;
                }

            //11%
            return CPlayer._PITCHER;
        }
                
        /// <summary>
        /// 그라운더 타구를 어떻게 처리 할지 여부를 판단하는 함수
        /// </summary>      
        public static GrounderCatchType GetGrounderCatchType(SpecificGrounderType grounderType, SimulFielder fielder, int posIndex, int shiftBonus)
        {
            int fielding = fielder.getFieldingAbil();
            //타구 판단과 레인지에 따른 추가
            int addFielding = fielder.addFieldingAbil(fielding);

            fielding += addFielding;

            int grounderShift = shiftBonus;
            int fieldingRange;

            //에러
            bool bCatchError = fielder.isCatchError();
            bool bThrowError = fielder.isThrowError();

            //투수와 내야수의 스킬
            bool bSpecialCatch = false;
            bool bSpecialThrow = false;
            bool bPitcherAct = false;
            bool bPitcherJump = false;
            if (posIndex == CPlayer._PITCHER)
            {
                bPitcherAct = fielder.checkSkillOn(SkillIndex.PitcherReaction);
                bPitcherJump = fielder.checkSkillOn(SkillIndex.PitcherJumpCatch);
            }
            else if (posIndex >= CPlayer._FIRSTBASEMAN && posIndex <= CPlayer._SHORTSTOP)
            {
                bSpecialCatch = fielder.checkSkillOn(SkillIndex.SpecialCatch);
                bSpecialThrow = fielder.checkSkillOn(SkillIndex.SpecialThrow); 
            }

            

            if (bCatchError == true)
            {
                return GrounderCatchType.BOUND_ERROR;
            }
            else if (bThrowError == true)
            {
                return GrounderCatchType.THROW_ERROR;
            }
            else
            {
                if (grounderType == SpecificGrounderType.Weak)
                {
                    fieldingRange = Random.Range(0, (GROUNDER_WEAK_RANGE + grounderShift));
                    //빅바운드 땅볼
                    if (fieldingRange > (fielding + 400))
                    {
                        //내야수 특수 던지기
                        if (bSpecialThrow == true)
                        {
                            //스킬 발동
                            return GrounderCatchType.Dash_Deep;
                        }
                        else
                        {
                            //안타
                            return GrounderCatchType.NoCatch;
                        }
                    }
                    else
                    {
                        //범타
                        return GrounderCatchType.Dash_Deep;
                    }
                }
                else if (grounderType == SpecificGrounderType.BigBound)
                {
                    if (posIndex == CPlayer._PITCHER)
                    {
                        //투수 점프 캐치
                        //bSkillActive = Random.Range(0, GROUNDER_CATCH_SKILL_RANGE) <= pitcherJump ? true : false;
                        if (bPitcherJump == true)
                        {
                            return GrounderCatchType.PitcherJump;
                        }
                        else
                        {
                            return GrounderCatchType.NoCatch;
                        }
                    }
                    else
                    {
                        fieldingRange = Random.Range(0, (GROUNDER_BIGBOUND_RANGE + grounderShift));
                        //빅바운드 땅볼
                        if (fieldingRange > (fielding + 400))
                        {
                            //내야수 특수 던지기
                            if (bSpecialThrow == true)
                            {
                                //스킬 발동
                                return GrounderCatchType.Dash_Deep;
                            }
                            else
                            {
                                //안타
                                return GrounderCatchType.NoCatch;
                            }
                        }
                        else
                        {
                            //범타
                            return GrounderCatchType.Dash_Deep;
                        }
                    }
                }
                else if (grounderType == SpecificGrounderType.Normal)
                {
                    //노멀 땅볼
                    fieldingRange = Random.Range(0, (GROUNDER_NORMAL_RANGE + grounderShift));
                    //Debug.Log[노멀 땅볼 비교] =====>>> 맥스 "+GROUNDER_NORMAL_RANGE+ " vs (필드기본값+400) ::" + fieldingRange + " vs " + (fielding + 400));
                    if (fieldingRange > (fielding + 400))
                    {
                        //내야수 슬라이딩 캐치
                        if (bSpecialCatch == true)
                        {
                            //스킬 발동!!!
                            return GrounderCatchType.SlidingCatch;
                        }
                        else
                        {
                            //안타
                            return GrounderCatchType.NoCatch;
                        }
                    }
                    else
                    {
                        //범타
                        int range = Random.Range(0, 10);
                        if (range > 5) return GrounderCatchType.Normal;
                        else if (range > 2) return GrounderCatchType.BackHand;
                        else return GrounderCatchType.ForeHand;
                    }
                }
                else if (grounderType == SpecificGrounderType.Solid)
                {
                    if (posIndex == CPlayer._PITCHER)
                    {
                        //투수 반응
                        //bSkillActive = Random.Range(0, GROUNDER_CATCH_SKILL_RANGE) <= pitcherAct ? true : false;
                        if (bPitcherAct == true)
                        {
                            return GrounderCatchType.PitcherAct;
                        }
                        else
                        {
                            return GrounderCatchType.NoCatch;
                        }
                    }
                    else
                    {
                        //솔리드 땅볼
                        fieldingRange = Random.Range(0, (GROUNDER_SOLID_RANGE + grounderShift));
                        //Debug.Log[솔리드 땅볼 비교] =====>>> 맥스 " + GROUNDER_SOLID_RANGE + " vs (필드기본값+400) ::" + fieldingRange + " vs " + (fielding + 400));
                        if (fieldingRange > (fielding + 400))
                        {
                            //내야수 슬라이딩
                            if (bSpecialCatch == true)
                            {
                                //스킬 발동
                                return GrounderCatchType.SlidingCatch;
                            }
                            else
                            {
                                //안타
                                if (posIndex == CPlayer._FIRSTBASEMAN || posIndex == CPlayer._THIRDBASEMAN)
                                {
                                    if (fieldingRange > (GROUNDER_SOLID_RANGE - 300))
                                    {
                                        //라인 선상으로 빠지는
                                        return GrounderCatchType.LineNoCatch;
                                    }
                                }
                                return GrounderCatchType.NoCatch;
                            }
                        }
                        else
                        {
                            //범타
                            int range = Random.Range(0, 10);
                            if (range > 7) return GrounderCatchType.Normal;
                            else if (range > 5) return GrounderCatchType.BackHand;
                            else if (range > 3) return GrounderCatchType.ForeHand;
                            else if (range > 1) return GrounderCatchType.ForeHand_Deep;
                            else return GrounderCatchType.BackHand_Deep;
                        }
                    }
                }
                else if (grounderType == SpecificGrounderType.VerySolid)
                {
                    if (posIndex == CPlayer._PITCHER)
                    {
                        //투수 반응
                        //bSkillActive = Random.Range(0, GROUNDER_CATCH_SKILL_RANGE + 1000) <= pitcherAct ? true : false;
                        if (bPitcherAct == true)
                        {
                            return GrounderCatchType.PitcherAct;
                        }
                        else
                        {
                            return GrounderCatchType.NoCatch;
                        }
                    }
                    else
                    {
                        //베리 솔리드 땅볼
                        fieldingRange = Random.Range(0, (GROUNDER_VERYSOLID_RANGE + grounderShift));
                        //Debug.Log[베리 솔리드 땅볼 비교] =====>>> 맥스 " + GROUNDER_VERYSOLID_RANGE + " vs (필드기본값+400) ::" + fieldingRange + " vs " + (fielding + 400));
                        if (fieldingRange > (fielding + 400))
                        {
                            //내야수 슬라이딩
                            if (bSpecialCatch == true)
                            {
                                //스킬 발동
                                return GrounderCatchType.SlidingCatch;
                            }
                            else
                            {
                                //안타
                                if (posIndex == CPlayer._FIRSTBASEMAN || posIndex == CPlayer._THIRDBASEMAN)
                                {
                                    if (fieldingRange > (GROUNDER_VERYSOLID_RANGE - 500))
                                    {
                                        //라인 선상으로 빠지는
                                        return GrounderCatchType.LineNoCatch;
                                    }
                                }
                                return GrounderCatchType.NoCatch;
                            }
                        }
                        else
                        {
                            //범타
                            int range = Random.Range(0, 10);
                            if (range > 7) return GrounderCatchType.Normal;
                            else if (range > 5) return GrounderCatchType.BackHand;
                            else if (range > 3) return GrounderCatchType.ForeHand;
                            else if (range > 1) return GrounderCatchType.ForeHand_Deep;
                            else return GrounderCatchType.BackHand_Deep;
                        }
                    }
                }
            }

            return GrounderCatchType.NoCatch;
        }
                
        /// <summary>
        /// 그라운더 타구와 주자의 주루 능력에 따른 최종결과
        /// </summary>      
        public static SimulResultState GetGrounderHitType(GrounderCatchType grounderCatchType, CPlayer runner, int fieldingValue)
        {
            if (grounderCatchType == GrounderCatchType.LineNoCatch)
            {
                return SimulResultState.Double;
            }
            else if (grounderCatchType == GrounderCatchType.NoCatch)
            {
                return SimulResultState.Single;
            }
            else if (grounderCatchType == GrounderCatchType.BOUND_ERROR)
            {
                return SimulResultState.BoundError;
            }
            else if (grounderCatchType == GrounderCatchType.THROW_ERROR)
            {
                return SimulResultState.ThrowError;
            }


            bool bTurbo = false;
            int runnerAbil = runner.getSpeed();// +runner.getSpeedBonus();

            //필더의 어깨 + fieldingValue +2000
            //러닝 레인지 + 터보가 있는경우

            int runningRange = Random.Range(0, runnerAbil);
            int fieldingRange = Random.Range(0, (fieldingValue + 1500));

            if (runnerAbil < 400)
            {
                //내야안타 없음
                runningRange = -1;
            }
            else
            {
                //Debug.Log[내야안타 체크] ==============>> runningRange MAX(" + runnerAbil + ") vs fieldingRange MAX(" + (fieldingValue + 1500) + ") :: " + runningRange + " vs " + fieldingRange);
            }

            if (fieldingValue > runningRange)
            {
                return SimulResultState.Grounder;
            }
            else
            {
                return (bTurbo ? SimulResultState.InfieldTurboSingle : SimulResultState.InfieldSingle);
            }

        }
                
        /// <summary>
        /// 그라운더 타구타입에 따른 그라운딩 수비 밸류 -> 높을수록 딜레이를 줄이고 어꺠가 좋음
        /// </summary>      
        public static int getGrounderFiedingValue(SpecificGrounderType grounderType, GrounderCatchType grounderCatchType, SimulFielder fielder)
        {
            //int value = fielder.getFielding() + fielder.getCatchBonus() + fielder.getThrowing() + fielder.getThrowBonus() + 1000;
            int value = fielder.getFielding() + fielder.getThrowing() + 1000;
                        
            //땅볼 타입에 따른 가감
            if (grounderType == SpecificGrounderType.Weak)
                value += -250;
            else if (grounderType == SpecificGrounderType.BigBound)
                value += -350;
            else if (grounderType == SpecificGrounderType.Solid)
                value += 600;
            else if (grounderType == SpecificGrounderType.VerySolid)
                value += 1200;

            //땅볼 처리 타입에 따른 가감
            bool bSpecialThrow = fielder.checkSkillOn(SkillIndex.SpecialThrow);
            if (grounderCatchType == GrounderCatchType.ForeHand)
            {
                value += (bSpecialThrow ? 0 : -200);
            }
            else if (grounderCatchType == GrounderCatchType.BackHand)
            {
                value += (bSpecialThrow ? 0 : -250);
            }
            else if (grounderCatchType == GrounderCatchType.ForeHand_Deep)
            {
                value += (bSpecialThrow ? 0 : -400);
            }
            else if (grounderCatchType == GrounderCatchType.BackHand_Deep)
            {
                value += (bSpecialThrow ? 0 : -550);
            }
            else if (grounderCatchType == GrounderCatchType.PitcherAct || grounderCatchType == GrounderCatchType.PitcherJump)
            {
                value += -200;
            }
            else if (grounderCatchType == GrounderCatchType.SlidingCatch)
            {
                value += -600;
            }

            //Debug.Log[각종 딜레이 가감한 땅볼 수비 밸류] ==============>> value :: " + value);
            return value;
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        //번트 타구
        ////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 번트타구를 잡을 야수를 얻어온다(그냥뺑뺑이)
        /// </summary>      
        public static int GetBuntCatchFielder()
        {
            int range = MyMath.Percent();
            if (range < 25) return CPlayer._PITCHER;
            else if (range < 50) return CPlayer._CATCHER;
            else if (range < 75) return CPlayer._FIRSTBASEMAN;
            else  return CPlayer._THIRDBASEMAN;
        }
                
        /// <summary>
        /// 미리계산된 번트 결과에 따른 수비 밸류(높을수록 아웃)
        /// </summary>      
        public static int GetBuntFieldValue(SpecificBuntType buntResult)
        {
            if (buntResult == SpecificBuntType.SAC_FAIL ||
                buntResult == SpecificBuntType.SQUEEZ_FAIL ||
                buntResult == SpecificBuntType.SAC_DOUBLE_PLAY)
            {
                //선행주자 아웃
                return 99999;
            }
            else if (buntResult == SpecificBuntType.SAC_SUCCESS ||
                    buntResult == SpecificBuntType.SQUEEZ_SUCCESS)
            {
                //타자주자 아웃
                return 0;
            }
            return 0;
        }
                
        /// <summary>
        /// 번트타입에 따른 최종결과
        /// </summary>      
        public static SimulResultState GetBuntResultType(SpecificBuntType buntResult)
        {
            if (buntResult == SpecificBuntType.DRAG_SUCCESS)
            {
                return SimulResultState.BuntSingle;
            }
            else if (buntResult == SpecificBuntType.SQUEEZ_FIELDER_CHOICE ||
                    buntResult == SpecificBuntType.SAC_FIELDER_CHOICE)
            {
                //야선 발생 -> 모든 주자 세잎
                return SimulResultState.FielderChoice;
            }
            else
            {
                return SimulResultState.Grounder;
            }
        }


        

        ///////////////////////////////////////////////////////////////////////
        //에러
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 포구에러를 할 확률
        /// </summary>                
        public static bool checkCatchError(int catchValue, bool bInfield)
        {
            //임시
            //return (MyMath.Percent() < 4 ? true : false);

            float catchIncrease, value;

            catchIncrease = (catchValue) * 0.069f;
            if (bInfield == true)
            {                
                value = Mathf.Sin(catchIncrease * Mathf.PI / 180) * 6.25f + 93.5f;
            }
            else
            {
                value = Mathf.Sin(catchIncrease * Mathf.PI / 180) * 5.4f + 94.5f;
            }
                        
            float range;// = Random.Range(0.0f, 100.0f);

#if _Local_Balance
            //로컬 밸런스시
            if (InGameDebug.CUSTOM_ERROR_MODE == true)
            {
                range = Random.Range(0.0f, 101.5f);
            }
            else
#endif
            {
                range = Random.Range(0.0f, 100.0f);
            }

            ////UnityEngine.//Debug.Log("=======================================================>>> range = " + range + "====>> errorValue = " + value);
            if (range > value)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 송구에러를 할 확률
        /// </summary>                
        public static bool checkThrowError(int throwValue, bool bCore)
        {
            //임시
            //return (MyMath.Percent() < 4 ? true : false);

            float throwIncrease, value;

            throwIncrease = (throwValue) * 0.069f;
            if (bCore == true)
            {
                value = Mathf.Sin(throwIncrease * Mathf.PI / 180) * 5.4f + 94.5f;
            }
            else
            {
                value = Mathf.Sin(throwIncrease * Mathf.PI / 180) * 3.95f + 96.0f;
            }

            float range;// = Random.Range(0.0f, 100.0f);

#if _Local_Balance
            //로컬 밸런스시
            if (InGameDebug.CUSTOM_ERROR_MODE == true)
            {
                range = Random.Range(0.0f, 101.5f);
            }
            else
#endif
            {
                range = Random.Range(0.0f, 100.0f);
            }

            ////UnityEngine.//Debug.Log("=======================================================>>> range = " + range + "====>> errorValue = " + value);
            if (range > value)
            {
                return true;
            }

            return false;
        }

        public static bool checkOffenseSkillWin(int offenseSkillRank, int defenseSkillRank)
        {
            return MyMath.Half();

            /*
            if (offenseSkillRank == 0)
            {
                return false;
            }

            float offenseWinRate = (float)offenseSkillRank / (float)(offenseSkillRank + defenseSkillRank);

            if (MyMath.PercentF() < offenseWinRate)
            {
                ////UnityEngine.//Debug.Log("========================>> Vs 대결 주자 승리");
                return true;
            }
            else
            {
                ////UnityEngine.//Debug.Log("========================>> Vs 대결 야수 승리");
                return false;
            }*/
        }


        /// <summary>
        /// 게임 밸런스 관련
        /// </summary>
        private static GameConstCommon gameConstCommon = null;

        /// <summary>
        /// 스킬맵
        /// </summary>
        private static Dictionary<int, skillEffectMap> skillMap = new Dictionary<int, skillEffectMap>();


        /// <summary>
        /// 게임 Const Common 값 가져오기
        /// </summary>
        /// <returns></returns>
        public static GameConstCommon GetCommon()
        {
            return gameConstCommon;
        }


        /// <summary>
        /// 스킬맵 초기화
        /// </summary>
        /// <param name="common"></param>
        public static void InitSkillMap(GameConstCommon common)
        {
#if _Test_Local
            //10001 제5의 내야수
            if(skillMap.ContainsKey(10001) == false)
                skillMap.Add(10001, new skillEffectMap("제5의내야수", Restriction_Type.Field));

            //10002 견제왕
            if (skillMap.ContainsKey(10002) == false)
                skillMap.Add(10002, new skillEffectMap("견제왕", Restriction_Type.Field));

            //10003 선두타자승부            
            if (skillMap.ContainsKey(10003) == false)
                skillMap.Add(10003, new skillEffectMap("선두타자승부", Restriction_Type.NoRestriction, Effect_InvokeCondition.InningStart, Effect_Validity.BattingEnd));

            //10004 추격본능
            List<int?> counterList = new List<int?>();
            if (skillMap.ContainsKey(10004) == false)
            {                
                counterList.Add(20010);
                counterList.Add(20012);
                skillMap.Add(10004, new skillEffectMap("추격본능", Restriction_Type.NoRestriction, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 100000, counterList));
            }

            //11104 소년가장
            if (skillMap.ContainsKey(11104) == false)
            {
                skillMap.Add(11104, new skillEffectMap("소년가장", Restriction_Type.NoRestriction, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 100000, counterList));
            }

            //10005 불꽃투혼
            if (skillMap.ContainsKey(10005) == false)
                skillMap.Add(10005, new skillEffectMap("불꽃투혼", Restriction_Type.NoRestriction, Effect_InvokeCondition.Crisis, Effect_Validity.BattingEnd, 100000, counterList));

            //10006 강심장
            if (skillMap.ContainsKey(10006) == false)
                skillMap.Add(10006, new skillEffectMap("강심장", Restriction_Type.NoRestriction, Effect_InvokeCondition.ScoringPosition, Effect_Validity.BattingEnd, 100000, counterList));

            //10007 회심의일격
            if (skillMap.ContainsKey(10007) == false)
                skillMap.Add(10007, new skillEffectMap("회심의일격", Restriction_Type.Batter, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd, 1));

            //11107 회심의일격
            if (skillMap.ContainsKey(11107) == false)
                skillMap.Add(11107, new skillEffectMap("돌직구", Restriction_Type.Batter, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd, 1));

            //10008 매혹
            if (skillMap.ContainsKey(10008) == false)
                skillMap.Add(10008, new skillEffectMap("매혹", Restriction_Type.Batter, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd, 1));

            //11108 선크림
            if (skillMap.ContainsKey(11108) == false)
                skillMap.Add(11108, new skillEffectMap("선크림", Restriction_Type.Batter, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd, 1));

            //11208 뱀직구
            if (skillMap.ContainsKey(11208) == false)
                skillMap.Add(11208, new skillEffectMap("뱀직구", Restriction_Type.Batter, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd, 1));

            //10009 투수위압
            if (skillMap.ContainsKey(10009) == false)
                skillMap.Add(10009, new skillEffectMap("투수위압", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1, counterList));

            //11109 니느님
            if (skillMap.ContainsKey(11109) == false)
                skillMap.Add(11109, new skillEffectMap("니느님", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1, counterList));

            //10010 강철어깨
            if (skillMap.ContainsKey(10010) == false)
                skillMap.Add(10010, new skillEffectMap("강철어깨", Restriction_Type.NoRestriction, Effect_InvokeCondition.Passive, Effect_Validity.GameEnd));

            //10011 카리스마
            if (skillMap.ContainsKey(10011) == false)
                skillMap.Add(10011, new skillEffectMap("카리스마", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //10012 닥터K
            if (skillMap.ContainsKey(10012) == false)
                skillMap.Add(10012, new skillEffectMap("닥터K", Restriction_Type.NoRestriction, Effect_InvokeCondition.ExceptionCase, Effect_Validity.ExceptionCase));

            //11112 닥터광
            if (skillMap.ContainsKey(11112) == false)
                skillMap.Add(11112, new skillEffectMap("닥터광", Restriction_Type.NoRestriction, Effect_InvokeCondition.ExceptionCase, Effect_Validity.ExceptionCase));

            //10013 필승의지
            if (skillMap.ContainsKey(10013) == false)
                skillMap.Add(10013, new skillEffectMap("필승의지", Restriction_Type.NoRestriction, Effect_InvokeCondition.ExceptionCase, Effect_Validity.ExceptionCase));

            //20001	내야수	철벽수비
            if (skillMap.ContainsKey(20001) == false)
                skillMap.Add(20001, new skillEffectMap("철벽수비", Restriction_Type.Field));
		
            //20002	내야수	특급송구
            if (skillMap.ContainsKey(20002) == false)
                skillMap.Add(20002, new skillEffectMap("특급송구", Restriction_Type.Field));

            //21102	내야수	평화송구
            if (skillMap.ContainsKey(21102) == false)
                skillMap.Add(21102, new skillEffectMap("평화송구", Restriction_Type.Field));

            //20003	외야수	쇠그물수비
            if (skillMap.ContainsKey(20003) == false)
                skillMap.Add(20003, new skillEffectMap("쇠그물수비", Restriction_Type.Field));		
		
            //20004	외야수	레이저송구
            if (skillMap.ContainsKey(20004) == false)
                skillMap.Add(20004, new skillEffectMap("레이저송구", Restriction_Type.Field));

            //20005	포수	도발꾼
            if (skillMap.ContainsKey(20005) == false)
                skillMap.Add(20005, new skillEffectMap("도발꾼", Restriction_Type.Batter, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd, 1));

            //21105	포수	도발꾼
            if (skillMap.ContainsKey(21105) == false)
                skillMap.Add(21105, new skillEffectMap("풍기문란", Restriction_Type.Batter, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd, 1));

            //21205	포수	도발꾼
            if (skillMap.ContainsKey(21205) == false)
                skillMap.Add(21205, new skillEffectMap("갑드래곤", Restriction_Type.Batter, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd, 1));
		
            //20006	포수	수비형포수
            if (skillMap.ContainsKey(20006) == false)
                skillMap.Add(20006, new skillEffectMap("수비형포수", Restriction_Type.Field));

            //21106	포수	안방마님
            if (skillMap.ContainsKey(21106) == false)
                skillMap.Add(21106, new skillEffectMap("안방마님", Restriction_Type.Field));	
		
            //20007	공용	질주본능
            if (skillMap.ContainsKey(20007) == false)
                skillMap.Add(20007, new skillEffectMap("질주본능", Restriction_Type.Field));

            //21107	공용	질주본능
            if (skillMap.ContainsKey(21107) == false)
                skillMap.Add(21107, new skillEffectMap("바람의아들", Restriction_Type.Field));
		
            //20008	공용	주루센스
            if (skillMap.ContainsKey(20008) == false)
                skillMap.Add(20008, new skillEffectMap("주루센스", Restriction_Type.Field));		
		
            //20009	공용	매의눈
            if (skillMap.ContainsKey(20009) == false)
                skillMap.Add(20009, new skillEffectMap("매의눈", Restriction_Type.NoRestriction, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd));

            //21109	공용	매의눈
            if (skillMap.ContainsKey(21109) == false)
                skillMap.Add(21109, new skillEffectMap("용의눈", Restriction_Type.NoRestriction, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd));

            //21209	공용	매의눈
            if (skillMap.ContainsKey(21209) == false)
                skillMap.Add(21209, new skillEffectMap("송골매", Restriction_Type.NoRestriction, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd));

            //21309	공용	매의눈
            if (skillMap.ContainsKey(21309) == false)
                skillMap.Add(21309, new skillEffectMap("스나이퍼", Restriction_Type.NoRestriction, Effect_InvokeCondition.PitchStart, Effect_Validity.PitchEnd));

            //20010	공용	타자위압
            if (skillMap.ContainsKey(20010) == false)
                skillMap.Add(20010, new skillEffectMap("타자위압", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21110	공용	타자위압
            if (skillMap.ContainsKey(21110) == false)
                skillMap.Add(21110, new skillEffectMap("출근의신", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21210	공용	타자위압
            if (skillMap.ContainsKey(21210) == false)
                skillMap.Add(21210, new skillEffectMap("소년장사", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21310	공용	타자위압
            if (skillMap.ContainsKey(21310) == false)
                skillMap.Add(21310, new skillEffectMap("금강불괴", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21410	공용	신의위압
            if (skillMap.ContainsKey(21410) == false)
                skillMap.Add(21410, new skillEffectMap("신의위압", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //20011	공용	강습타구
            if (skillMap.ContainsKey(20011) == false)
                skillMap.Add(20011, new skillEffectMap("강습타구", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21111	공용	
            if (skillMap.ContainsKey(21111) == false)
                skillMap.Add(21111, new skillEffectMap("타격기계", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21211	공용	
            if (skillMap.ContainsKey(21211) == false)
                skillMap.Add(21211, new skillEffectMap("만세타법", Restriction_Type.Inning, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //20012	공용	찬스맨
            if (skillMap.ContainsKey(20012) == false)
                skillMap.Add(20012, new skillEffectMap("찬스맨", Restriction_Type.Inning, Effect_InvokeCondition.ScoringPosition, Effect_Validity.BattingEnd, 1));

            //21112	공용	찬스맨
            if (skillMap.ContainsKey(21112) == false)
                skillMap.Add(21112, new skillEffectMap("리틀쿠바", Restriction_Type.Inning, Effect_InvokeCondition.ScoringPosition, Effect_Validity.BattingEnd, 1));

            //21212	공용	찬스맨
            if (skillMap.ContainsKey(21212) == false)
                skillMap.Add(21212, new skillEffectMap("꽃범호", Restriction_Type.Inning, Effect_InvokeCondition.ScoringPosition, Effect_Validity.BattingEnd, 1));


            //20013	공용	번트의신
            if (skillMap.ContainsKey(20013) == false)
                skillMap.Add(20013, new skillEffectMap("번트의신", Restriction_Type.Inning, Effect_InvokeCondition.NoRunner, Effect_Validity.BattingEnd, 1));
            
            //21113	공용	번트의신
            if (skillMap.ContainsKey(21113) == false)
                skillMap.Add(21113, new skillEffectMap("용규놀이", Restriction_Type.Inning, Effect_InvokeCondition.NoRunner, Effect_Validity.BattingEnd, 1));

            //20014	공용	뜬금포
            if (skillMap.ContainsKey(20014) == false)
                skillMap.Add(20014, new skillEffectMap("뜬금포", Restriction_Type.Game, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21114	공용	뜬금포
            if (skillMap.ContainsKey(21114) == false)
                skillMap.Add(21114, new skillEffectMap("박뱅포", Restriction_Type.Game, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21214	공용	뜬금포
            if (skillMap.ContainsKey(21214) == false)
                skillMap.Add(21214, new skillEffectMap("빅보이", Restriction_Type.Game, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));

            //21314	공용	뜬금포
            if (skillMap.ContainsKey(21314) == false)
                skillMap.Add(21314, new skillEffectMap("국민타자", Restriction_Type.Game, Effect_InvokeCondition.BattingStart, Effect_Validity.BattingEnd, 1));
#else
            skillMap.Clear();
            gameConstCommon = common;
            foreach (KeyValuePair<int, Skill> value in common.SkillsMap)
            {
                skillEffectMap effectMap = new skillEffectMap();

                WebConnector.Skill skill = value.Value;

                if (skill.restrictionType != null)
                {
                    effectMap.restriction = (Restriction_Type)System.Enum.Parse(typeof(Restriction_Type), skill.restrictionType); // Restriction_Type.NoRestriction;
                }
                else
                {
                    effectMap.restriction = Restriction_Type.NoRestriction;
                }

                if (skill.invokeCondition != null)
                {
                    effectMap.invokeCondition = (Effect_InvokeCondition)System.Enum.Parse(typeof(Effect_InvokeCondition), skill.invokeCondition); // Effect_InvokeCondition.ExceptionCase;
                }
                else
                {
                    effectMap.invokeCondition = Effect_InvokeCondition.ExceptionCase; 
                }

                if (skill.validity != null)
                {
                    effectMap.effectValidity = (Effect_Validity)System.Enum.Parse(typeof(Effect_Validity), skill.validity); //Effect_Validity.ExceptionCase;
                }
                else
                {
                    effectMap.effectValidity = Effect_Validity.ExceptionCase;
                }

                if (skill.restrictionCount != null)
                {
                    effectMap.restrictionCount = skill.restrictionCount;
                }
                else
                {
                    effectMap.restrictionCount = 100000;
                }

                if (skill.matchSkills != null)
                {
                    effectMap.counter = skill.matchSkills;
                }

                // DISABLED_MGRS: skill skillmap = Mgrs.GameData.GameDB_FindSkill(value.Key);
                if(skillmap != null)
                {
                    effectMap.skillName = skillmap.name;
                    ////Debug.Log("===============>>스킬이름 = " + effectMap.skillName);
                    skillMap.Add(value.Key, effectMap);
                }
            }
#endif

        }

        public static skillEffectMap GetSkillInfo(int skillId)
        {
            if (skillMap.ContainsKey(skillId) == true)
            {
                return skillMap[skillId];
            }
            else
            {
                return null;
            }
        }

        /*
        public static skillEffectMap GetSkillInfo(int skillId)
        {
            //int skillId = GetSkillID(effectID);
            if (skillMap.ContainsKey(skillId) == true)
            {
                return skillMap[skillId];
            }
            else
            {
                return null;
            }
        }*/

        

        /// <summary>
        /// 스킬 아이디로부터 스킬 대표 효과 인덱스 얻어오기
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static SkillIndex GetSkillEffect(int ID)
        {
            int id;
            if (ID < (int)SkillID.chul_byuk_su_bi)
            {
                //투수
                id = 10000 + (ID % 100);
            }
            else
            {
                //타자
                id = 20000 + (ID % 100);
            }

            return (SkillIndex)((id * 10) + 1);
        }

        public static SkillID GetPrimaryID_FromSkillEffect(SkillIndex index)
        {
            return (SkillID)((int)index / 10);
        }

    }


    
}
