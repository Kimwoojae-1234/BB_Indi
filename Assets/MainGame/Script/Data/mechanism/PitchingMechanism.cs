
using UnityEngine;
using System.Collections;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class PitcherGrade
    {
        public int ballSpeed;
        public int control;
        public int ballGrade;
    }

    public class PitchingMechanism
    {
        //////////////////////////////////////////////////////////////////////
        //상수
        //////////////////////////////////////////////////////////////////////              
        //스태미너 초기값
        public const float INIT_STAMINA = 100.0f;
        //핀치스코어 세팅값
        public const int PINCH_SCORE = 10;
        //최대 구종 종류                
        public const int TYPE_NUM = 5; //종류

        //////////////////////////////////////////////////////////////////////
        //기본 능력치 보정용 밸런스 상수
        //////////////////////////////////////////////////////////////////////
        //컨트롤 가중치
        public static float CONTROL_VALUE = 1.0f;
        //구위 가중치
        public static float GUWEE_VALUE = 1.0f;
        //볼 스피드 밸류
        public static float BALLSPEED_VALUE = 1.0f;
        //볼 무브먼트 밸류
        public static float BALLMOVEMENT_VALUE = 1.0f;
        
        

        //////////////////////////////////////////////////////////////////////
        //컨트롤 밸런스 상수
        //////////////////////////////////////////////////////////////////////
        //컨트롤 노멀밸류 최저값
        public static float MIN_NORMAL_CONTROL_VALUE = 26.0f;   //이값보다 작으면 Bad밸류
        //컨트롤 굿밸류 최저값
        public static float MIN_GOOD_CONTROL_VALUE = 60.0f;
        //컨트롤 퍼펙트밸류 최저값
        public static float MIN_PERFECT_CONTROL_VALUE = 70.0f;
        //실투율
        public static int MISS_PER = 60;                //올라갈수록 실투율이 높아짐 500기준 60/1500비율이라 생각하면됨
        //유저 조정 스피드
        public static float USER_CONTROL_SPEED = 2.0f;  //올라갈수록 유저 컨트롤이 어려워짐 기본 2.0f

        //////////////////////////////////////////////////////////////////////
        //구위 밸런스 상수
        //////////////////////////////////////////////////////////////////////
        //퍼펙트 구위 배수
        public static float PERFECT_GUWEE_RATE = 8.0f;
        //굿 구위 배수
        public static float GOOD_GUWEE_RATE = 4.0f;
        //노멀 구위 배수
        public static float NORMAL_GUWEE_RATE = 2.0f;
        



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
        /// <summary>
        /// 투수가 현 상황에 알맞는 구종 선택
        /// </summary>
        public static PitchingArsenal getSelectedBall(CPlayer pitcher, int ball, int strike, bool lastFastball)
        {
            //카운트
            int curCount = (ball * 10 + strike);

            //보유 구종
            int fastballValue = getTypeValue(pitcher, PitchType.FASTBALL);
            
            int[] breakValue = new int[4];
            int breakCount = 0;
            int curveTypeCount = 0;
            int offspeedTypeCount = 0;
            int totalBreakValue = 0;
            int avgBreakValue = 0;
            int curveTypeValue = 0;
            int offspeedTypeValue = 0;
            for (int i = 0; i < 4; i++)
            {
                breakValue[i] = getTypeValue(pitcher, PitchType.CURVE + i);
                if (breakValue[i] > 0)
                {
                    totalBreakValue += breakValue[i];
                    breakCount++;
                    if (i % 2 == 0) curveTypeCount++;   //커브 타입 수
                    else offspeedTypeCount++;           //오프타입 타입 수
                }
            }

            //변화구 평균값
            if (breakCount > 0)
            {
                avgBreakValue = totalBreakValue / breakCount;
            }

            //커브타입 평균값
            if (curveTypeCount > 0)
            {
                curveTypeValue = (breakValue[0] + breakValue[2]) / curveTypeCount;
            }

            //오프스피드 타입 평균값
            if (offspeedTypeCount > 0)
            {
                offspeedTypeValue = (breakValue[1] + breakValue[3]) / offspeedTypeCount;
            }

            //직구 추가 밸류
            int addFastBallVale = 0;
            addFastBallVale += (lastFastball ? -100 : 100);

            switch (curCount)
            {
                case NOBALL_NOSTRIKE:
                    //초구에는 직구비율 약간 높임
                    addFastBallVale += 100;
                    break;
                case NOBALL_TWOSTRIKE:
                case ONEBALL_TWOSTRIKE:
                case TWOBALL_TWOSTRIKE:
                    //볼카운트 유리시 변화구 비율높임
                    addFastBallVale -= 200;
                    break;
                case TWOBALL_NOSTRIKE:
                case THREEBALL_NOSTRIKE:
                case THREEBALL_ONESTRIKE:
                    //볼카운트가 불리한 경우 직구 비율 높임
                    addFastBallVale += 200;
                    break;
            }


            float fastRange = (float)(fastballValue + addFastBallVale) / (float)(fastballValue + avgBreakValue);
            float breakingRange = (float)(curveTypeValue) / (float)(curveTypeValue + offspeedTypeValue);
            float rand = Random.Range(0.0f, 1.0f);  //직구/변화구
            float rand2 = Random.Range(0.0f, 1.0f); //브레이킹류 / 오프스피드류
            float rand3 = Random.Range(0.0f, 1.0f); // 커브vs슬라, 첸졉vs포크
            
            if (rand < fastRange)
            {
                //직구류
                return getPitchType(pitcher, PitchType.FASTBALL);
            }
            else
            {
                if (rand2 < breakingRange)
                {
                    if (breakValue[0] == -1) //커브류가 없는 경우
                    {
                        //슬라 타입 리턴
                        return getPitchType(pitcher, PitchType.SLIDER);
                    }
                    else if (breakValue[2] == -1) //슬라류가 없는 경우
                    {
                        //커브 타입 리턴
                        return getPitchType(pitcher, PitchType.CURVE);
                    }
                    else
                    {
                        float curveRange = (float)(breakValue[0]) / (float)(breakValue[0] + breakValue[2]);
                        if (rand3 < curveRange)
                        {
                            //커브 타입 리턴
                            return getPitchType(pitcher, PitchType.CURVE);
                        }
                        else
                        {
                            //슬라 타입 리턴
                            return getPitchType(pitcher, PitchType.SLIDER);
                        }
                    }
                }
                else
                {
                    if (breakValue[1] == -1) //첸졉류가 없는 경우
                    {
                        //슬라 타입 리턴
                        return getPitchType(pitcher, PitchType.FORK);
                    }
                    else if (breakValue[3] == -1) //포크류가 없는 경우
                    {
                        //커브 타입 리턴
                        return getPitchType(pitcher, PitchType.CHANGEUP);
                    }
                    else
                    {
                        float changeupRange = (float)(breakValue[1]) / (float)(breakValue[1] + breakValue[3]);
                        if (rand3 < changeupRange)
                        {
                            //커브 타입 리턴
                            return getPitchType(pitcher, PitchType.CHANGEUP);
                        }
                        else
                        {
                            //슬라 타입 리턴
                            return getPitchType(pitcher, PitchType.FORK);
                        }
                    }
                }
            }
        }

        ///
        //
        public static PitchingArsenal getSelectedBallNineTwoMode(CPlayer pitcher, int curRound)
        {
            return getPitchType(pitcher, PitchType.FASTBALL);
        }


        /// <summary>
        /// 구종의 볼 종류
        /// </summary>
        public static PitchType getBallType(PitchingArsenal ballType)
        {
            if ((ballType >= PitchingArsenal.CURVE && ballType <= PitchingArsenal.KNUCKLE_CURVE) || ballType == PitchingArsenal.UPSHOOT)
            {
                return PitchType.CURVE;
            }
            else if (ballType >= PitchingArsenal.CHANGEUP && ballType <= PitchingArsenal.KNUCKLE)
            {
                return PitchType.CHANGEUP;
            }
            else if (ballType >= PitchingArsenal.SLIDER && ballType <= PitchingArsenal.FRISBEE)
            {
                return PitchType.SLIDER;
            }
            else if (ballType >= PitchingArsenal.FORK && ballType <= PitchingArsenal.SINKING_FAST)
            {
                return PitchType.FORK;
            }

            return PitchType.FASTBALL;

        }



        /// <summary>
        /// 스트라이크를 던지는지 여부
        /// true인 경우 스트라이크를 던지고, false인 경우 볼을 던짐
        /// </summary>
        public static bool pitchStrike(int ball, int strike, bool bFastball, int battingValue, int pitchingValue, bool bSimul)
        {
            //모든 능력치 밸류는 맥스 1000기준
            //컨트롤 밸류와 구위밸류가 높을수록 유인구가 많아진다(심지어 2-3에서도 유인)
            int curCount = ball * 10 + strike;
            int strikeRange = 600;
            int offset = 0;
            int value = (battingValue - pitchingValue) / 2;

            switch (curCount)
            {
                case NOBALL_NOSTRIKE:
                    strikeRange = 600;
                    offset = value;
                    break;
                case NOBALL_ONESTRIKE:
                    if (bFastball == true)
                    {
                        strikeRange = 600;
                    }
                    else
                    {
                        strikeRange = 500;
                    }
                    offset = value / 2;
                    break;
                case NOBALL_TWOSTRIKE:
                    if (bFastball == true)
                    {
                        strikeRange = 300;
                    }
                    else
                    {
                        strikeRange = 120;
                    }
                    offset = 0;
                    break;
                case ONEBALL_NOSTRIKE:
                    strikeRange = 750;
                    offset = value;
                    break;
                case ONEBALL_ONESTRIKE:
                    if (bFastball == true)
                    {
                        strikeRange = 600;
                    }
                    else
                    {
                        strikeRange = 550;
                    }
                    offset = value;
                    break;
                case ONEBALL_TWOSTRIKE:
                    if (bFastball == true)
                    {
                        strikeRange = 400;
                    }
                    else
                    {
                        strikeRange = 300;
                    }
                    offset = value / 2;
                    break;
                case TWOBALL_NOSTRIKE:
                    strikeRange = 900;
                    offset = value / 4;
                    break;
                case TWOBALL_ONESTRIKE:
                    if (bFastball == true)
                    {
                        strikeRange = 800;
                    }
                    else
                    {
                        strikeRange = 700;
                    }
                    offset = value / 4;
                    break;
                case TWOBALL_TWOSTRIKE:
                    if (bFastball == true)
                    {
                        strikeRange = 850;
                    }
                    else
                    {
                        strikeRange = 800;
                    }
                    offset = value / 6;
                    break;
                case THREEBALL_NOSTRIKE:
                    strikeRange = 990;
                    offset = 0;
                    break;
                case THREEBALL_ONESTRIKE:
                    strikeRange = 900;
                    offset = value / 8;
                    break;
                case THREEBALL_TWOSTRIKE:
                    strikeRange = 900;
                    offset = value / 8;
                    break;
            }

            int rand = Random.Range(0, (1000 + offset));

            if (bSimul == true)
            {
                strikeRange = (strikeRange * 75) / 100;
            }

            if (rand < strikeRange)
            {
                //스트라이크를 던짐
                return true;
            }
            else
            {
                //볼을 던짐
                return false;
            }
        }


        /// <summary>
        /// 핀포인트로부터 컨트롤 밸류에 따른 옵셋을 계산하여 최종 코스 벡터값을 리턴한다.
        /// 야구 엔진에서만 쓰이고, 시뮬엔진에서는 쓰이지 않는다.
        /// </summary>
        public static Vector2 getCourse(UserControlValue conValue, float henkaX, float henkaY, bool bStrike, bool bMustLowControl, bool bNormalHard)
        {
            //Vector2 pos, offset;
            float x, y;
            float offsetX = 0, offsetY = 0;
            bool bOutCourse = MyMath.Half(); //아웃코스 55%
            bool bLowCourse = MyMath.Half();
            int sign = bStrike ? -1 : 1;

            if (conValue == UserControlValue.Perfect)
            {
                offsetX = Random.Range(1.0f, 10.0f);
                offsetY = Random.Range(1.0f, (bStrike ? 30.0f : 10.0f));
                x = (bOutCourse ? -1 : 1) * (Zone.UI_ZONE_WIDTH + (sign * offsetX));
                y = (bMustLowControl ? -1 : 1) * (Zone.UI_ZONE_HEIGHT + (sign * offsetY));
            }
            else if (conValue == UserControlValue.Good)
            {
                if (MyMath.Percent() < 70)
                {
                    //좌우 코너워크
                    offsetX = Random.Range(1.0f, 16.0f);
                    x = (bOutCourse ? -1 : 1) * (Zone.UI_ZONE_WIDTH + (sign * offsetX));
                    y = Random.Range(-Zone.UI_ZONE_HEIGHT / 2, Zone.UI_ZONE_HEIGHT / 2);
                }
                else
                {
                    //상하 코너워크
                    offsetY = Random.Range(1.0f, (bStrike ? 30.0f : 10.0f));
                    x = Random.Range(-Zone.UI_ZONE_WIDTH / 2, Zone.UI_ZONE_WIDTH / 2);
                    y = (bMustLowControl ? -1 : 1) * (Zone.UI_ZONE_HEIGHT + (sign * offsetY));
                }
            }
            else if (conValue == UserControlValue.Normal)
            {
                //bNormalHard : true이면 노멀 컨트롤에서 좀더 까다롭게                
                if (bNormalHard == false)
                {
                    if (MyMath.Percent() < 60)
                    {
                        //좌우 코너워크
                        offsetX = Random.Range(20.0f, (bStrike ? 50.0f : 30.0f));
                        x = (bOutCourse ? -1 : 1) * (Zone.UI_ZONE_WIDTH + (sign * offsetX));
                        float height = (Zone.UI_ZONE_HEIGHT / 2 - 15);
                        y = Random.Range(-height, height);
                    }
                    else
                    {
                        //상하 코너워크
                        offsetY = Random.Range(20.0f, (bStrike ? 50.0f : 30.0f));
                        float width = (Zone.UI_ZONE_WIDTH / 2 - 15);
                        x = Random.Range(-width, width);
                        y = (bLowCourse ? -1 : 1) * (Zone.UI_ZONE_HEIGHT + (sign * offsetY));
                    }
                }
                else
                {
                    offsetX = Random.Range(20.0f, 45.0f);
                    offsetY = Random.Range(20.0f, (bStrike ? 40.0f : 30.0f));
                    x = (bOutCourse ? -1 : 1) * (Zone.UI_ZONE_WIDTH + (sign * offsetX));
                    y = (bLowCourse ? -1 : 1) * (Zone.UI_ZONE_HEIGHT + (sign * offsetY));
                }
            }
            else if (conValue == UserControlValue.Bad)
            {
                if (bStrike == true)
                {
                    //비교적 가운데
                    x = (bOutCourse ? -1 : 1) * Random.Range(0.3f, 0.7f) * Zone.UI_ZONE_WIDTH;
                    y = (bLowCourse ? -1 : 1) * Random.Range(0.3f, 0.7f) * Zone.UI_ZONE_HEIGHT;
                }
                else
                {
                    //눈에 띄게 뒤로
                    x = (bOutCourse ? -1 : 1) * Random.Range(1.2f, 1.45f) * Zone.UI_ZONE_WIDTH;
                    y = (bLowCourse ? -1 : 1) * Random.Range(1.2f, 1.45f) * Zone.UI_ZONE_HEIGHT;
                }
            }
            else
            {
                //miss
                x = Random.Range(-10, 10);
                y = Random.Range(-10, 10);
            }


            return new Vector2(x - henkaX, y - henkaY);

        }


        /// <summary>
        /// 핀포인트로부터 컨트롤 밸류에 따른 옵셋을 계산하여 최종 코스 벡터값을 리턴한다.
        /// </summary>
        public static Vector2 getCourseOffset(ControlValue conValue, float courseX, float courseY)
        {
            Vector2 offset = Vector2.zero;

            float offsetX = 0;
            float offsetY = 0;

            float signX = Mathf.Sign(courseX);
            float signY = Mathf.Sign(courseY);

            if (conValue == ControlValue.PinPoint)
            {
                offsetX = offsetY = 0;
            }
            else if (conValue == ControlValue.Good)
            {
                offsetX = Random.Range(0, 20.0f);
                offsetY = Random.Range(0, 20.0f);
            }
            else if (conValue == ControlValue.Normal)
            {
                offsetX = Random.Range(20, 35.0f);
                offsetY = Random.Range(20, 35.0f);
            }
            else if (conValue == ControlValue.Bad)
            {
                offsetX = Random.Range(35, 50.0f);
                offsetY = Random.Range(35, 50.0f);
            }

            if (Mathf.Abs(courseX) < Zone.UI_ZONE_WIDTH - 8)
            {
                offsetX = -(signX * offsetX);
                if (courseX > 0)
                {
                    if (courseX + offsetX < 0)
                    {
                        offsetX = -courseX * 0.8f;
                    }
                }
                else
                {
                    if (courseX + offsetX > 0)
                    {
                        offsetX = -courseX * 0.8f;
                    }
                }
            }
            else
            {
                offsetX = (signX * offsetX);
            }

            if (Mathf.Abs(courseY) < Zone.UI_ZONE_HEIGHT - 8)
            {
                offsetY = -(signY * offsetY);
                if (courseY > 0)
                {
                    if (courseY + offsetY < 0)
                    {
                        offsetY = -courseY * 0.8f;
                    }
                }
                else
                {
                    if (courseY + offsetY > 0)
                    {
                        offsetY = -courseY * 0.8f;
                    }
                }
            }
            else
            {
                offsetY = (signY * offsetY);
            }

            offset.x = offsetX;
            offset.y = offsetY;

            return offset;
        }


        /// <summary>
        /// 실투를 던지는지 여부를 리턴한다.
        /// MISS_PER값을 조절하여 확률을 조절할 수 있다.
        /// </summary>
        
        public static bool getMiss(int guwee, FatigueStep fatigue)
        {
            float value = (1000.0f + guwee);
            float rate = (float)MISS_PER / value;

            if (fatigue == FatigueStep.STAMINA_FATIGUE)
            {
                //지침
                rate *= 2;
            }
            else if (fatigue == FatigueStep.STAMINA_VERY_FATIGUE)
            {
                //탈진
                rate *= 4;
            }
            else if (fatigue == FatigueStep.STAMINA_EXUSTED)
            {
                //방전
                rate *= 8;
            }
            float range = MyMath.PercentF();

            ////Debug.Log("=======>>> 실투율 : " + rate + " =======>> range = " + range);

            if (range < rate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 컨트롤 점수를 얻어옴
        /// 이건 테스트 모드임 더 복잡하고 구체적으로 만들것!!
        /// </summary>
        public static UserControlValue getControlValue(float guwee)
        {
            float rate = 0.2f + (guwee / 1200.0f);
            if (rate > 1.3f) rate = 1.3f; //PerfectWidth 0.2
            float rate2 = rate;
            if (rate2 < 0.5f) rate2 = 0.5f;//NormalWidth 최저0.5, //GoodWidth 최저0.5

            float badX = MIN_NORMAL_CONTROL_VALUE - (36 * rate2);
            float normalX = MIN_GOOD_CONTROL_VALUE - (30 * rate2);
            float goodX = MIN_PERFECT_CONTROL_VALUE - (10 * rate);
            float perfectX = MIN_PERFECT_CONTROL_VALUE + (10 * rate);

            float rand = Random.Range(-20, 90);

            /*
            //Debug.Log("=====================>>> 컨트롤 밸류 guwee = " + guwee);
            Debug.Log("badX = " + badX);
            Debug.Log("normalX = " + normalX);
            Debug.Log("goodX = " + goodX);
            Debug.Log("perfectX = " + perfectX);

            //Debug.Log("=====================>>> rand = " + rand);*/

            if (rand < badX)
            {
                return UserControlValue.Bad;
            }
            else if (rand < normalX)
            {
                return UserControlValue.Normal;
            }
            else if (rand < goodX)
            {
                return UserControlValue.Good;
            }
            else if (rand < perfectX)
            {
                return UserControlValue.Perfect;
            }
            else
            {
                return UserControlValue.Normal;
            }


        }
        
        /// <summary>
        /// 투수가 던진 공이 가지고 있는 최종 파워는 컨트롤 점수에 비례한 구위의 최종값
        /// </summary>
        public static int GetPitcherPower(ControlValue value, int guwee)
        {
            if (value == ControlValue.PinPoint)
            {
                //핀포인트 경우 구위 3배
                return (int)(3 * guwee); //*3;
            }
            else if (value == ControlValue.Good)
            {
                //굿인 경우 구위 2배
                return (int)(2 * guwee); //* 2;
            }
            else if (value == ControlValue.Normal)
            {
                //노멀인 경우 구위 1.5배
                return (int)(1.5f * guwee); //*1.5
            }
            else if (value == ControlValue.Miss)
            {
                //실투인 경우 구위 50%감소
                return (int)(0.5f * guwee); //*0.5
            }
            else //Bad
            {
                //배드인 경우 구위 보존
                return guwee;
            }
        }
        
        

        /// <summary>
        /// 지친 정도에 따른 체력 감소치
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fatigue"></param>
        /// <param name="pinch"></param>
        /// <returns></returns>
        public static int reductionByFatigue(int value, FatigueStep fatigue)
        {
            float reduction = value;

            if (fatigue == FatigueStep.STAMINA_FATIGUE)//1)
            {
                //지침
                reduction *= 0.85f;
            }
            else if (fatigue == FatigueStep.STAMINA_VERY_FATIGUE)
            {
                //탈진
                reduction *= 0.70f;
            }
            else if (fatigue == FatigueStep.STAMINA_EXUSTED)
            {
                //방전
                reduction *= 0.60f;
            }

            return (int)reduction;

        }


        

        /// <summary>
        /// 투수 타입과 구종, 능력치에 따른 공스피드 산출식
        /// </summary>
        /// <param name="pitcher"></param>
        /// <param name="selectBall"></param>
        /// <returns></returns>
        public static int GetBallSpeed(Pitcher pitcher, PitchingArsenal selectBall)
        {
            /*
            float value = 0;
            float speedRange = 42;        //구속 범위     
            int initSpeed = 125;        //초기 속도
            float speedWeight = 1.0f;   //구종 가중치
            float typeWeight = 1.0f;    //투구 유형 가중치
            */

            /*
            구속 = (SIN( (능력치 * A) * PI()/180)^B*C+D 
            상수A 0.45 
            상수B 1.1 
            상수C 80 
            상수D 100*/

            PitchingArsenal ball = selectBall;
            int curBallValue = pitcher.pPitcher.getBallValue(ball) / 10;

            //투수 개별 구속 가중치
            float pitcherVelocityWeight = (float)pitcher.pPitcher.getVelocityValue() / 100.0f;

            //투수의 투구 타입별 구속 가중치
            float typeWeight = 1.0f;
            if (pitcher.pitchingType == 1)
            {
                //사이드
                typeWeight = 0.97f;
            }
            else if (pitcher.pitchingType == 2)
            {
                //언더
                typeWeight = 0.95f;
            }

            //구종 타입별 구속 가중치
            float speedWeight = 1.0f;

            float A = 0.45f;
            float B = 1.1f;
            float C = 80;
            float D = 100;

            float sinValue = curBallValue * A * (Mathf.PI / 180);

            
            if (ball == PitchingArsenal.FASTBALL) //포심        
            {
                C = 80;
                speedWeight = 1.0f;
            }
            else if (ball == PitchingArsenal.TWOSEAM)    //투심
            {
                C = 80;
                speedWeight = 0.98f; 
            }
            else if (ball == PitchingArsenal.RISING)   
            {
                C = 80;
                speedWeight = 1.000f; 
            }

            //커브류
            else if (ball == PitchingArsenal.CURVE)   
            {
                C = 70;
                speedWeight = 0.9f;
            }
            else if (ball == PitchingArsenal.POWER_CURVE)   
            {
                C = 75;
                speedWeight = 0.9f; 
            }
            else if (ball == PitchingArsenal.SLOW_CURVE)   
            {
                C = 60;
                speedWeight = 0.850f; 
            }
            else if (ball == PitchingArsenal.POKPOSU_CURVE)   
            {
                C = 70;
                speedWeight = 0.9f; 
            }
            else if (ball == PitchingArsenal.KNUCKLE_CURVE)   
            {
                C = 65;
                speedWeight = 0.9f; 
            }
            else if (ball == PitchingArsenal.UPSHOOT)
            {
                C = 60;
                speedWeight = 0.85f;
            }

            //첸졉류
            else if (ball == PitchingArsenal.CHANGEUP)   
            {
                C = 70;
                speedWeight = 0.93f;
            }
            else if (ball == PitchingArsenal.CIRCLE)   
            {
                C = 70;
                speedWeight = 0.93f;
                
            }
            else if (ball == PitchingArsenal.VULCAN)   
            {
                C = 70;
                speedWeight = 0.9f;
                
            }
            else if (ball == PitchingArsenal.PALM)   
            {
                C = 55;
                speedWeight = 0.9f; 
            }
            else if (ball == PitchingArsenal.KNUCKLE)   
            {
                C = 50;
                speedWeight = 0.75f;
            }

            //슬라이더류
            else if (ball == PitchingArsenal.SLIDER)   
            {
                C = 80;
                speedWeight = 0.9f;
                
            }
            else if (ball == PitchingArsenal.H_SLIDER)   
            {
                C = 80;
                speedWeight = 0.94f;
                
            }
            else if (ball == PitchingArsenal.SLURVE)   
            {
                C = 80;
                speedWeight = 0.88f;
                
            }
            else if (ball == PitchingArsenal.CUT_FAST)   
            {
                C = 80;
                speedWeight = 0.98f;
                
            }
            else if (ball == PitchingArsenal.FRISBEE)   
            {
                C = 80;
                speedWeight = 0.9f;
                
            }
        
            //포크류
            else if (ball == PitchingArsenal.FORK)   
            {
                C = 80;
                speedWeight = 0.91f;
                
            }
            else if (ball == PitchingArsenal.SINKER)   
            {
                C = 80;
                speedWeight = 0.9f;                
            }
            else if (ball == PitchingArsenal.SFF)   
            {
                C = 80;
                speedWeight = 0.96f; 
            }
            else if (ball == PitchingArsenal.SINKING_FAST)   
            {
                C = 80;
                speedWeight = 0.98f;
            }

            float value = ((Mathf.Pow(Mathf.Sin(sinValue), B) * C) + D) * speedWeight * typeWeight * pitcherVelocityWeight;
            
            return (int)(PitchingMechanism.BALLSPEED_VALUE * value);
        }


        /// <summary>
        /// 컨트롤과 기본 구위가 더해진 최종 구위 산출식
        /// </summary>
        /// <param name="guwee"></param>
        /// <param name="controlValue"></param>
        /// <returns></returns>
        public static int GetFinalGuwee(int guwee, ControlValue controlValue)
        {            
            float finalValue;
            float rate = Random.Range(1.0f, 1.4f);
            if (controlValue == ControlValue.PinPoint)
            {
                finalValue = guwee * PitchingMechanism.PERFECT_GUWEE_RATE * rate;   //8
            }
            else if (controlValue == ControlValue.Good)
            {
                finalValue = guwee * PitchingMechanism.GOOD_GUWEE_RATE * rate;      //4
            }
            else if (controlValue == ControlValue.Normal)
            {
                finalValue = guwee * PitchingMechanism.NORMAL_GUWEE_RATE * rate;    //2    
            }
            else
            {
                finalValue = guwee * rate;
            }

            return (int)finalValue;
        }



        public static int GetFinalGuweeNineTwo(int round)
        {
            return 100 + (round * 100);
        }

        /// <summary>
        /// 투수 타입과 구종, 능력치에 따른 공의 변화각 산출 공식
        /// </summary>
        /// <param name="pitcher"></param>
        /// <param name="selectBall"></param>
        /// <returns></returns>
        public static float GetBallMovemnet(Pitcher pitcher, PitchingArsenal selectBall)
        {
            float typeWeight = 1.0f;    //투구 유형 가중치
            if (pitcher.pitchingType == 0)
            {
                //오버
                typeWeight = 1.0f;
            }
            else
            {
                //사이드
                //언더
                typeWeight = 1.1f;
            }

            int curBallValue = pitcher.pPitcher.getBallValue(selectBall) / 10;
            float A = 0.45f; 
            float B = 1.2f; 
            float C = 90f;
            float sinValue = curBallValue * A * (Mathf.PI / 180);

            //변화량 = (SIN( (능력치 * A) * PI()/180)^B*C
            float value = (Mathf.Pow(Mathf.Sin(sinValue), B) * C) * typeWeight;
            ////Debug.Log("============================>> 선택된 공: "+selectBall +"  공 밸류 " +curBallValue + "   공의 변화각 : " + value);
            return PitchingMechanism.BALLMOVEMENT_VALUE * value;
        }



        /// <summary>
        /// 현재 투수가 가지고 있는 공중에 해당 타입의 구종의 값을 리턴해줌
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int getTypeValue(CPlayer pPitcher, PitchType type)
        {
            for (int i = 0; i < 5; i++)
            {
                PitchingArsenal cur = pPitcher.getBallType()[i];
                if (type == getBallType(cur))
                {
                    return pPitcher.getBallValue2(i);
                }
            }

            return -1;
        }


        public static PitchingArsenal getPitchType(CPlayer pPitcher, PitchType type)
        {
            for (int i = 0; i < 5; i++)
            {
                PitchingArsenal cur = pPitcher.getBallType()[i];
                if (type == getBallType(cur))
                {
                    return cur;
                }
            }

            return PitchingArsenal.FASTBALL;
        }




        //세이브 상황 각 보직별 가중치
        public static int[] saveValue = new int[TYPE_NUM]  //큰 밸류가 높은 가중치를 갖는다
        {
          //선,   추,    중,   셋,    마
            1, 100000, 200000,300000,500000 
        };

        //셋업 상황 보직별 가중치
        public static int[] setupValueOver8 = new int[TYPE_NUM]  //큰 밸류가 높은 가중치를 갖는다
        {
          //선,  추,     중,   셋,    마
            1, 100000, 300000,500000,200000 
        };

        //7회 승리조 보직별 가중치
        public static int[] setupValue7 = new int[TYPE_NUM]  //작은 밸류가 높은 가중치를 갖는다
        {
            //선,      추,    중,   셋,    마
            500000,  200000,  1,   10000,500000 
        };

        //6회 승리조 보직별 가중치
        public static int[] setupValue6 = new int[TYPE_NUM]  //작은 밸류가 높은 가중치를 갖는다
        {
            //선,      추,    중,     셋,    마
            500000, 100000, 200000,   1,   500000 
        };

        //4점차 이전 추격
        public static int[] chaseValueUnder4Point = new int[TYPE_NUM]  //큰밸류가 높은 가중치를 갖는다
        {
          //선,추,     중,     셋,    마
            1, 400000, 300000, 200000,100000 
        };

        //4점차 이상 추격
        public static int[] chaseValueOver4Point = new int[TYPE_NUM]  //작은밸류가 높은 가중치를 갖는다
        {
            //선,   추,  중,     셋,    마
            500000, 1, 100000, 200000,300000 
        };

        //롱릴리프
        public static int[] longReliefValue = new int[TYPE_NUM]  //큰밸류가 높은 가중치를 갖는다
        {
          //선,  추,     중,     셋,  마
            1, 500000, 200000, 100000,1 
        };
    }

}
