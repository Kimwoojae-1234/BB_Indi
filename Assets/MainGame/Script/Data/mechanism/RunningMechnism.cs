#define _HOMERUSH_HAPPEN
#define _DELAY_STEAL_HAPPEN
//#define _DPBREAK_HAPPEN
#define _SLIDING_HAPPEN

using UnityEngine;
using System.Collections;


namespace BaseBall.BallPlay
{

    public class RunningMechnism
    {

        //기본 능력치 값
        public static float BASIC_DELAY = 0.25f;                    //기본딜레이 (단위 초)       
        public static float DELAY_DECREASE_RATE = 0.002857f;        //능력치 대비 딜레이 감소 비율 -> 크면 클수록 딜레이가 급속히 감소
        public static float BASIC_SPEED_MINIMUM = 230.0f;           //최대 320 (단위 pixel/s)
        public static float SPEED_INCREASE_RATE = 0.09f;            //능력치 대비 이동속도 증가비율 -> 크면 클수록 이동속도가 급속히 증가     
        public static float BASIC_ACCEL_RATE_MINIMUM = 50.0f;       //최대 1 (단위 pixel / s^2)

        //범위 밸류
        public static int SLIDING_RANGE = 150;                       //기본 슬라이딩 (단위 pixel)
        public static int RUSH_RANGE = 300;                          //홈돌진 레인지 (단위 pixel)   
        public static int CLOSE_PLAY_RANGE = 80;                     //접전 범위     (단위 pixel)
        public static int BASE_ARRIVE_RANGE = 5;                     //도착 범위     (단위 pixel)


        //도루 관련 밸류
        public static float STEAL_DELAY = 0.2f;                     //도루 딜레이 (단위 pixel)
        public static int STEAL_SPEED = 700;                        //도루 속도 (단위 pixel/s)
        public static int BUNT_SPEED = 600;                        //도루 속도 (단위 pixel/s)
        public static float STEAL_ACCEL = 40.0f;                    //도루 가속 (단위 pixel/s^2)    

        //견제 관련 밸류
        public static float PICKOFF_SAFE_DELAY = 0.5f;              //견제 세이프 딜레이 (s)
        public static float PICKOFF_SAFE_SPEED = 250.0f;            //견제 세이프 스피드 (pixel / s)
        public static float PICKOFF_OUT_DELAY = 0.7f;               //견제 아웃 딜레이 (s)
        public static float PICKOFF_OUT_SPEED = 150.0f;             //견제 아웃 스피드 (pixel / s)

        //오버런
        public static float OVERRUN_SAFE_LIMIT = -0.35f;        //안전빵 오버런 (s)
        public static float OVERRUN_DANGER_LIMIT = -0.7f;       //위험빵 오버런 (s)
        public static float OVERRUN_HOMERUSH_LIMIT = -0.9f;     //홈돌진 오버런 (s)

        //귀루
        public static float RUNNER_BACK_DELAY = 1.0f;           //치고다리기시 백할 딜레이

        //특수
        public static float HITTERRUNNER_DELAY_RATE = 0.25f;            //타자주자 추가 딜레이 시간


        ///////////////////////////////////////////////////////////////////////
        //기본 수비의 2차 능력치를 얻어오는 함수 - 엔진에서만 사용
        ///////////////////////////////////////////////////////////////////////
        //주자 딜레이 얻어오기
        public static float getRunnerDelay(float senseLevel)
        {
            float delay = RunningMechnism.BASIC_DELAY - (senseLevel * RunningMechnism.DELAY_DECREASE_RATE); // 센스 스킬이 이 수치에 영향
            if (delay < 0.05f) delay = 0.05f; //1보다 작으면 안됨

            return delay;
        }

        //주자 주력 얻어오기 //200.0f - 320   230 - 320
        public static float getRunnerSpeed(float speed)
        {
            return RunningMechnism.BASIC_SPEED_MINIMUM + (speed * SPEED_INCREASE_RATE);   //최신 230-320 :현재는 이걸로 적용
        }

        //주자 가속 비율 얻어오기
        public static float getAccelRate(float accelLevel)
        {
            //accelLevel 값은 주자의 수퍼소닉값에 투수의 주자속박 값을 뺀값
            float rate = 0.333f;
            float accelRate = RunningMechnism.BASIC_ACCEL_RATE_MINIMUM - (accelLevel * rate);// 적어질수록 가속 빨라짐 : 수퍼소닉 스킬이 이 수치에 영향
            if (accelRate < 1) accelRate = 1; //1보다 작으면 안됨

            return accelRate;
        }


        ///////////////////////////////////////////////////////////////////////
        //스킬 발동 및 성공 여부 - 엔진과 시뮬레이터 공통 사용
        ///////////////////////////////////////////////////////////////////////
        public const int TURBO_SUCCESS = 1200; //크면 클수록 터보 성공확률이 낮아짐
        public const int DELAYSTEAL_HAPPEN = 10000; //크면 클수록 발동확률 낮아짐
        public const int SLIDINGSKILL_HAPPEN = 10000; //크면 클수록 발동확률 낮아짐
        public const int HOMERUSH_HAPPEN = 2000; //크면 클수록 발동확률 낮아짐
        public const int DPBREAK_HAPPEN = 1000; //크면 클수록 발동확률 낮아짐


        //[슬라이딩] 스킬로 인해 추가되는 태그 딜레이
        public static float getSlidingTagDelay(float slidingLevel)
        {
            return 0.5f;
        }


        //[홈돌진] 스킬 성공여부
        public static bool checkRushSkillSuccess(int rushLevel, int blockLevel)
        {
            Debug.Log("rushLevel = " + rushLevel);
            Debug.Log("blockLevel = " + blockLevel);

            int defenseLevel = blockLevel + 30;
            int offenseLevel = rushLevel + 30;
            int defensRange = Random.Range(defenseLevel - 40, defenseLevel + 40);
            int offenseRange = Random.Range(offenseLevel - 40, offenseLevel + 40);

            Debug.Log("offenseRange = " + offenseRange);
            Debug.Log("defensRange = " + defensRange);

            if (offenseRange > defensRange)
            {
                return true;
            }

            return false;
        }
        


        ///////////////////////////////////////////////////////////////////////
        //모션 - 엔진에서만 사용
        ///////////////////////////////////////////////////////////////////////
        //주자가 아웃되는 모션을 얻어옴
        public static RunnerOutMotion getOutMotion(bool forceout, bool sliding, int basePos, bool bSkillOut)
        {
            if (basePos == FieldParm.FIRSTBASE_INDEX)
            {
                if (sliding == true)
                {
                    return RunnerOutMotion._FIRSTBASE_SLIDING_OUT;
                }
                else
                {
                    return RunnerOutMotion._FIRSTBASE_RUN_OUT;
                }
            }
            else if (basePos == FieldParm.SECONDBASE_INDEX)
            {
                if (sliding == true)
                {
                    if (bSkillOut == true)
                    {
                        return RunnerOutMotion._SECONDBASE_SKILL_OUT;
                    }
                    else
                    {
                        return RunnerOutMotion._SECONDBASE_SLIDING_OUT;
                    }
                }
            }
            else if (basePos == FieldParm.THIRDBASE_INDEX)
            {
                if (sliding == true)
                {
                    return RunnerOutMotion._THIRDBASE_SLIDING_OUT;
                }
            }
            else //if (destPos == FieldParm.HOMEBASE_INDEX)
            {
                if (sliding == true)
                {
                    return RunnerOutMotion._HOMEBASE_SLIDING_OUT;
                }
            }
            return RunnerOutMotion._NORMAL;
        }


        ///////////////////////////////////////////////////////////////////////
        //상태 - 엔진에서만 사용
        ///////////////////////////////////////////////////////////////////////
        //주자 상태
        public const int ADD_SCORE = 97;
        public const int NOT_EXIST = 98;
        public const int GO_BENCH = 99;

        public const int STANDBY = 100;
        public const int MOVE = 101;
        public const int WAIT = 102;
        public const int SLIDING = 103;
        public const int CHECK = 104;
        public const int RUSH = 105;
        public const int BLOCKED = 106;
        public const int DOUBLEPLAY = 107;
        public const int STEAL = 108;
        public const int PICKOFF = 109;
        public const int GOODBYEHIT = 110;
        public const int DO_NOTHING = 111;
        public const int FIRSTBASE_SAFE = 112;
        public const int SECOND_THIRD_SAFE = 113;

        ///////////////////////////////////////////////////////////////////////
        //기타 및 유틸함수 - 엔진에서만 사용
        ///////////////////////////////////////////////////////////////////////
        public static float[,] LEAD_GAB = new float[4, 2] { { -1, 1 }, { -1.5f, -1.5f }, { 0.8f, -0.8f }, { 0, 0 } };
        public static int[] RunnerDir = new int[4] { FieldParm._NORTHWEST, FieldParm._SOUTHWEST, FieldParm._SOUTHEAST, FieldParm._NORTHEAST };
        public static int[] RunnerLeadDir = new int[4] { FieldParm._SOUTHWEST, FieldParm._SOUTH, FieldParm._NORTHEAST, FieldParm._NORTHEAST };
        public static int[] offsetY = new int[4] { -30, 0, 0, -15 };

        //기타 계산식
        public static float getDistance(float disX, float disY)
        {

            return Mathf.Sqrt((disX * disX) + (disY * disY));
        }
        public static float getAngle(float disX, float disY)
        {

            return Mathf.Atan2(disY, disX);
        }


        //애니메이션 문자열
        public const string _HOLD = "0000_HOLD_",
                         _RUN = "0100_RUN_",
                         _WALK = "0001_WALK_",
                         _SLIDING = "2000_SLIDING_LEG_",
                         _HEADSLIDING = "2010_SLIDING_HEAD_",
                          _FIRSTOUT_TYPE1 = "3000_FIRSTBASE_OUT_TYPE2",
                          _FIRSTOUT_TYPE2 = "3010_FIRSTBASE_SAFE_TYPE2",
                          _FIRSTSAFT_TYPE1 = "3010_FIRSTBASE_SAFE_TYPE1",
                          _FIRSTSAFT_TYPE2 = "3010_FIRSTBASE_SAFE_TYPE2",

                         _SECOND_ARRIVE = "1100_BASEARRIVE_2",
                         _THIRD_ARRIVE = "1100_BASEARRIVE_3";


    }

}
