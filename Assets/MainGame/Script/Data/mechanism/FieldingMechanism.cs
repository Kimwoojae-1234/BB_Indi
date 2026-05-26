//#define _DIVING_SUCCESS
#define _PICKOFF_SUCCESS
#define _SITTHROW_HAPPEN

using UnityEngine;
using System.Collections;


namespace BaseBall.BallPlay
{
    public class FieldingMechanism
    {
        ///////////////////////////////////////////////////////////////////////
        //기본 능력치 밸런스값
        ///////////////////////////////////////////////////////////////////////
        public static float BASIC_FIELD_DELAY = 0.55f;          //높을수록 수비 못함
        public static float BASIC_THROW_DELAY = 0.4f;           //높을수록 수비 못함
        public static float BASIC_FIELDER_SPEED = 177.5f;       //높을수록 수비 잘함
        public static float BASIC_THROW_SPEED = 1125f;          //높을수록 수비 잘함

        ///////////////////////////////////////////////////////////////////////
        //필딩 보정 밸런스
        ///////////////////////////////////////////////////////////////////////
        public static float FIELDING_ADJUST_RATE = 0.7f;         //높을수록 수비 잘함

        ///////////////////////////////////////////////////////////////////////
        //내야 특수동작
        ///////////////////////////////////////////////////////////////////////
        public static float SPECIAL_GROUNDER_MIN_VALUE = 1.05f; //특수캐치 최저값 높을수록 특수땅볼캐치가 잘일어남   
        public static float SLIDING_CATCH_OFFSET = 0.1f;        //그라운더 다이빙 캐치 옵셋 높을수록 슬라이딩캐치 잘일어남

        ///////////////////////////////////////////////////////////////////////
        //외야 특수동작
        ///////////////////////////////////////////////////////////////////////
        public static float SPECIAL_FLYCATCH_MIN_VALUE = 1.05f; //특수캐치 최저값 높을수록 특수땅볼캐치가 잘일어남   
        public static float DIVING_CATCH_OFFSET = 0.1f;         //그라운더 다이빙 캐치 옵셋 높을수록 슬라이딩캐치 잘일어남
        public static float HR_STEAL_MAX_VALUE = 1.3f;          //홈런 캐치가 가능한 최대값

        //팀수비
        public static int TEAM_DEFENSE = 75;                  //높으면 좋음
        public static int JUGGLE_PER = 25;                  //저글확률

        //홈런캐치 거리
        public const int HOMERUN_STEAL_DISTANCE = 180;

        ///////////////////////////////////////////////////////////////////////
        //기본 수비의 2차 능력치를 얻어오는 함수 - 엔진에서만 사용됨
        ///////////////////////////////////////////////////////////////////////    
        /*//야수의 [캐치딜레이] 얻어옴
        public static float getCatchDelay(float rate, float level)
        {
            float levelRate = 0.8f + 0.15f * level;
            float delayRate = rate * levelRate;

            if (delayRate < 0.7f) delayRate = 0.7f;
            else if (delayRate > 1.3f) delayRate = 1.3f;

            return delayRate;
        }*/

        //야수의 필드 딜레이 (수비 능력치)
        public static float getFieldDelay(int value)
        {
            //float basicDelay = 0.55f;
            float delay = FieldingMechanism.BASIC_FIELD_DELAY - (value * 0.0002f);
            return Mathf.Clamp(delay, 0.2f, 0.55f);
        }

        //쓰로잉 딜레이
        public static float getThrowDelay()
        {
            return FieldingMechanism.BASIC_THROW_DELAY;
        }

        //필딩스피드 (주루 70%, 수비 30%) 
        public static float getFieldSpeed(float value1, float value2)
        {
            float value = ((value1 * 0.7f) + (value2 * 0.3f)) * FIELDING_ADJUST_RATE; //조정수치를 넣어주자
            if (value < 250.0f) value = 250.0f;   //하한

            float offset = (value - 500.0f) * 0.146f;

            if (offset < -45) offset = -45f; //if (offset < -73) offset = -73f;
            else if (offset > 73f) offset = 73f;

            return (FieldingMechanism.BASIC_FIELDER_SPEED + offset) * 1.15f; 
        }

        //송구 스피드
        public static float getThrowSpeed(int value)
        {
            //송구가 너무 좋아 조정수치를 넣어주자
            float changedValue = value * FIELDING_ADJUST_RATE;
            if (changedValue < 250.0f) changedValue = 250.0f;   //하한

            float offset = (changedValue - 500.0f) * 1.25f;

            if (offset < -625) offset = -625f;
            else if (offset > 625f) offset = 625f;
            return (FieldingMechanism.BASIC_THROW_SPEED + offset);
        }

        
        ///////////////////////////////////////////////////////////////////////
        //송구의 딜레이 레이트를 얻어오는 함수 - 엔진에서만 사용
        ///////////////////////////////////////////////////////////////////////
        //일반송구 [딜레이 레이트]를 얻어옴
        public static float getNormalThrowDelayRate(int throwing, int fielding)
        {
            int value = (throwing + fielding) / 2;
            float rate = 1.2f - (0.0008f * value); // Random.Range(0.4f, 0.8f);

            return rate;
        }

        //각각의 송구 상태별로 [딜레이 레이트] 계산
        public static float getCatchDelayRate(ThrowType type, float fielding, bool spinDelay)
        {
            if (type == ThrowType._INFIELD_SIDE_QUICK)
            {
                return 0.65f;
            }
            else if (type == ThrowType._INFIELD_SIDE_SPIN)
            {
                //2루에서 스핀 무브 땜시 걸리는 딜레이 더블플레이시 계산
                return 0.1f + (spinDelay ? 0.05f : 0);
            }
            else if (type == ThrowType._INFIELD_OVER_JUMPING)
            {
                return 0.1f;
            }
            return 1;
        }

        //각각의 송구 준비 동작의 상태별 [딜레이 레이트] 계산
        public static float getThrowReadyDelay(ThrowType type)
        {
            if (type == ThrowType._INFIELD_SIDE_QUICK)
            {
                return DELAY_THROW_SIDEQUICK;
            }
            else if (type == ThrowType._INFIELD_SIDE_SPIN)
            {
                return DELAY_THROW_SIDESPIN;
            }
            else if (type == ThrowType._INFIELD_OVER_JUMPING)
            {
                return DELAY_THROW_JUMPING;
            }

            return DELAY_THROW_NORMAL;
        }
        
        //나의 송구로 인한 상대의 태그 딜레이 얻어옴 (송구 정확도 개념)
        public static float getTaggingDelay(float fielding, float throwing)
        {
            //이값은 정확히 송구 되었을 경우
            return FieldingMechanism.DELAY_TAGGING;
        }

        
                


        
        ///////////////////////////////////////////////////////////////////////
        //상황별 최적화된 송구 형태를 얻어오는 함수
        ///////////////////////////////////////////////////////////////////////    
        //야수의 상태별 최적화된 쓰로잉 상태 얻어옴
        public static ThrowType getThrowType(ThrowState state)
        {
            //////Debug.Log("=================>>check here");
            if (state == ThrowState.DOUBLE_PLAY)
            {
                return ThrowType._INFIELD_SIDE_QUICK; //_INFIELD_DOUBLE_PLAY1;// _INFIELD_SIDE_QUICK;
            }
            else if (state == ThrowState.FORE_HAND_CATCH)
            {
                return ThrowType._INFIELD_SIDE_SPIN;
            }
            else if (state == ThrowState.BACK_HAND_CATCH)
            {
                return ThrowType._INFIELD_OVER_JUMPING;
            }
            else if (state == ThrowState.CENTER_MOVING)
            {
                return ThrowType._INFIELD_SIDE_QUICK;
            }

            //////Debug.Log("=================>>노멀");
            return ThrowType._NORMAL;
        }

        /*
        //퀵스로우 능력이 없을시 노멀 송구를 얻어옴
        public static bool checkQuickThrow(int level, float ballPower, bool bDoublePlay)
        {
            if (level > 0)
            {
                if (ballPower < 23 || bDoublePlay == true)
                {
                    int value = level + 20;
                    if (MyMath.Percent() < value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }*/

        /*public static ThrowType getThrowTypeNormal(bool bSkillActive)
        {
            //////Debug.Log("=================>>check here");
            if (bSkillActive == true)
            {
                return ThrowType._INFIELD_SIDE_QUICK;
            }
            return ThrowType._NORMAL;
        }*/

        //퀵쓰로가 가능한 상태인지 판단 ->이거 안쓰일 것 같음
        public static bool checkQuickThrow(ThrowState state)
        {

            //이거 안쓰일 것 같음

            if (state == ThrowState.DOUBLE_PLAY)
            {
                return true;
            }
            else if (state == ThrowState.FORE_HAND_CATCH)
            {
                return true;
            }
            else if (state == ThrowState.CENTER_MOVING)
            {
                return true;
            }
            /*else if (state == ThrowState.BACK_HAND_CATCH)
            {
                //역동작 퀵쓰로우는 불가

            }*/

            //////Debug.Log("=================>>노멀");
            return false;
        }

        ///////////////////////////////////////////////////////////////////////
        //상태 예외처리 함수 - 엔진에서만 사용
        ///////////////////////////////////////////////////////////////////////
        //더블플레이시 쓰로우 타입 익셉션
        public static ThrowType getDPThrowException(int posIndex, ThrowType throwType)
        {
            ThrowType type = throwType;

            if (posIndex == CPlayer._SECONDBASEMAN)
            {
                if (throwType == ThrowType._INFIELD_OVER_JUMPING)
                {
                    type = ThrowType._INFIELD_SIDE_QUICK;
                }
                else if (throwType == ThrowType._INFIELD_SIDE_SPIN)
                {
                    type = ThrowType._INFIELD_SIDE_QUICK;
                }
            }
            else if (posIndex == CPlayer._SHORTSTOP)
            {
                if (throwType == ThrowType._INFIELD_SIDE_SPIN)
                {
                    type = ThrowType._INFIELD_SIDE_QUICK;
                }
            }
            return type;
        }

        //포지션별 쓰로잉 상태의 익셉션 계산
        public static bool getThrowAddDelayException(int posIndex, ThrowType throwType)
        {
            if (posIndex == CPlayer._FIRSTBASEMAN)
            {
            }
            else if (posIndex == CPlayer._SECONDBASEMAN)
            {
                if (throwType == ThrowType._INFIELD_SIDE_SPIN)
                {
                    return false;
                }

            }
            else if (posIndex == CPlayer._THIRDBASEMAN)
            {
                if (throwType == ThrowType._INFIELD_OVER_JUMPING)
                {
                    return false;
                }
                else if (throwType == ThrowType._INFIELD_SIDE_SPIN)
                {
                    return false;
                }
            }
            else if (posIndex == CPlayer._SHORTSTOP)
            {
                if (throwType == ThrowType._INFIELD_OVER_JUMPING)
                {
                    return false;
                }

            }
            return true;
        }

        //커버인덱스 값이 익셉션이 나왔을 경우 강제로 인덱스를 정해 리턴
        public static int getCoverIndexException(int posIndex, float firstAngle)
        {
            int index = CPlayer._FIRSTBASEMAN;
            if (posIndex >= CPlayer._LEFTFIELDER)
            {
                if (firstAngle > 0) index = CPlayer._SHORTSTOP;
                else index = CPlayer._SECONDBASEMAN;
            }
            return index;
        }

        //펜스충돌 예외처리 
        public static int getFenceColException(int dir)
        {
            int newDir = dir;

            if (dir == FieldParm._EAST || dir == FieldParm._SOUTHEAST)
            {
                newDir = FieldParm._NORTHEAST;
            }
            else if (dir == FieldParm._WEST || dir == FieldParm._SOUTHWEST)
            {
                newDir = FieldParm._NORTHWEST;
            }
            else if (dir == FieldParm._SOUTH)
            {
                newDir = FieldParm._NORTH;
            }

            return newDir;
        }

        //대쉬 방향 예외처리 
        public static int getDashDirException(int dir)
        {
            int curDir = dir;

            if (dir == FieldParm._EAST || dir == FieldParm._NORTHEAST)
            {
                curDir = FieldParm._SOUTHEAST;
            }
            else if (dir == FieldParm._WEST || dir == FieldParm._NORTHWEST)
            {
                curDir = FieldParm._SOUTHWEST;
            }

            return curDir;
        }

        //포수 방향의 예외처리 
        public static int   getCatcherCatchException(int tIndex, float angle)
        {
            if (tIndex == CPlayer._LEFTFIELDER || tIndex == CPlayer._THIRDBASEMAN)
            {
                return FieldParm._NORTHWEST;
            }
            else if (tIndex == CPlayer._RIGHTFIELDER || tIndex == CPlayer._FIRSTBASEMAN)
            {
                return FieldParm._NORTHEAST;
            }
            else
            {
                if (angle > 15) return FieldParm._NORTHWEST;
                else if (angle < -15) return FieldParm._NORTHEAST;
                else return FieldParm._NORTH;
            }
        }

        //땅볼시 방향 예외처리
        public static int getGrounderDir(int dir)
        {
            if (dir <= FieldParm._SOUTHEAST) return FieldParm._EAST;
            else return FieldParm._WEST;
        }

        //방향 예외처리
        public static int getDirException(int curDir, float dX, float dY)
        {
            //방향 예외처리
            int dir = curDir;
            if (curDir == FieldParm._NORTHEAST) dir = Mathf.Abs(dY) > Mathf.Abs(dX) ? FieldParm._NORTH : FieldParm._EAST;
            else if (curDir == FieldParm._NORTHWEST) dir = Mathf.Abs(dY) > Mathf.Abs(dX) ? FieldParm._NORTH : FieldParm._WEST;
            else if (curDir == FieldParm._SOUTHEAST) dir = Mathf.Abs(dY) > Mathf.Abs(dX) ? FieldParm._SOUTH : FieldParm._EAST;
            else if (curDir == FieldParm._SOUTHWEST) dir = Mathf.Abs(dY) > Mathf.Abs(dX) ? FieldParm._SOUTH : FieldParm._WEST;

            //if (dir == FieldParm._NORTH) if (dX < 0) dir = FieldParm._NORTHWEST;

            return dir;
        }

        //플라이볼 처리시 방향 예외처리
        public static int getFlyballDirException(int dir)
        {
            int curDir = dir;

            if (curDir == FieldParm._NORTHEAST || curDir == FieldParm._EAST || curDir == FieldParm._NORTHWEST || curDir == FieldParm._WEST || curDir == FieldParm._NORTH)
            {
                curDir = FieldParm._SOUTH;
            }

            return curDir;
        }

        //포구준비 자세시 방향 예외처리
        public static int getFielderCatchReadyDir(int pos, float firstAngle)
        {
            bool bCorner = false;
            bool bLeft = false;

            //float angle = ball.angle;
            //if (angle > 180) angle = angle-360;
            float angle = firstAngle;

            if (Mathf.Abs(angle) > 17) bCorner = true;
            ////////Debug.Log("=====================>>catch angle = " + angle);*/

            if (pos == CPlayer._CATCHER)
            {
                return FieldParm._NORTH;
            }
            else
            {
                if (pos == CPlayer._PITCHER || pos == CPlayer._CENTERFIELDER)
                {
                    if (angle > 0) bLeft = true;
                    else bLeft = false;
                }
                else if (pos == CPlayer._THIRDBASEMAN || pos == CPlayer._SHORTSTOP || pos == CPlayer._LEFTFIELDER)
                {
                    bLeft = true;
                }
                else
                {
                    bLeft = false;
                }
            }

            if (bLeft == true)
            {
                if (bCorner == true) return FieldParm._SOUTHEAST;
                else return FieldParm._SOUTH;
            }
            else
            {
                if (bCorner == true) return FieldParm._SOUTHWEST;
                else return FieldParm._SOUTH;
            }

        }

        public static int getJumpCatchException(int dir, int pos)
        {
            if (pos == CPlayer._PITCHER) return FieldParm._SOUTH;

            if (dir < FieldParm._SOUTHEAST) return FieldParm._SOUTHEAST;
            else if (dir > FieldParm._SOUTHWEST) return FieldParm._SOUTHWEST;

            else return dir;

        }


        public static int getDashDir(int pos)
        {
            if (pos == CPlayer._LEFTFIELDER || pos == CPlayer._THIRDBASEMAN)
            {
                return FieldParm._SOUTHEAST;
            }
            else if (pos == CPlayer._FIRSTBASEMAN || pos == CPlayer._RIGHTFIELDER)
            {
                return FieldParm._SOUTHWEST;
            }
            else
            {
                return FieldParm._SOUTH;
            }

        }

        ///////////////////////////////////////////////////////////////////////
        //기타
        ///////////////////////////////////////////////////////////////////////  
        //타구방향의 범위 여부 리턴 (이건 엔진에서만 씀)
        public static bool checkPitcherActRange(float posY, float originY, float power, float angle, float angleZ)
        {
            //Debug.Log("posY = " + posY + "====>originY =" + originY);
            if (Mathf.Abs(posY - originY) < 65)
            {
                if (angleZ > -30 && angleZ < 0)
                {
                    if (power >= 27 && Mathf.Abs(angle) <= 5)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        //코너 내야수 옵셋 증가분
        public static float getCornerInfielderRangeOffset(int posIndex, float power, float angle)
        {
            if (posIndex == CPlayer._FIRSTBASEMAN || posIndex == CPlayer._THIRDBASEMAN)
            {
                if (power < 28 && Mathf.Abs(angle) > 40)
                {
                    return 0.1f;
                }

            }
            return 0;
        }


        ///////////////////////////////////////////////////////////////////////
        //각종 유틸 함수
        ///////////////////////////////////////////////////////////////////////
        //해당 포지션이 라이트 코너 필더인지
        public static bool checkRightCornerFielder(int index)
        {
            if (index == CPlayer._FIRSTBASEMAN
                || index == CPlayer._SECONDBASEMAN
                || index == CPlayer._RIGHTFIELDER)
            {
                return true;
            }
            return false;
        }

        //라이트 코너 야수 타입2
        public static bool checkRightCornerFielder2(int index)
        {
            if (index == CPlayer._FIRSTBASEMAN
                || index == CPlayer._RIGHTFIELDER)
            {
                return true;
            }
            return false;
        }

        //레프트 코너 야수 타입1
        public static bool checkLeftCornerFielder(int index)
        {
            if (index == CPlayer._THIRDBASEMAN
             || index == CPlayer._SHORTSTOP
             || index == CPlayer._LEFTFIELDER)
            {
                return true;
            }
            return false;
        }

        //레프트 코너 야수 타입2
        public static bool checkLeftCornerFielder2(int index)
        {
            if (index == CPlayer._THIRDBASEMAN
             || index == CPlayer._LEFTFIELDER)
            {
                return true;
            }
            return false;
        }

        //필더와 특정 위치간의 거리를 리턴
        public static float getDistance(float posX1, float posX2, float posY1, float posY2)
        {
            float a = (posX2 - posX1);
            float b = (posY2 - posY1);

            return Mathf.Sqrt((a * a) + (b * b));

        }


        public static float getGroundTime(float posX1, float posX2, float posY1, float posY2, float firstSpeed)
        {
            float distance = getDistance(posX1, posX2, posY1, posY2);
            float a = 0.5f * FBall._BOUND_FRICTION_COEFF;
            float b = firstSpeed * FBall._BOUND_SPEED_RATE;
            float c = -1 * distance;


            float value = MyMath.getEquation(a, b, c, true);

            if (value == 0)
            {
                return distance / firstSpeed;
            }
            else
            {
                return value;
            }
        }

        //특정 목적지까지 걸리는 시간
        public static float getTime(float ox, float oy, float dstx, float dsty, float speed, float delay)
        {
            float a = (dstx - ox);
            float b = (dsty - oy);
            float time = (delay + (Mathf.Sqrt(a * a + b * b) / speed));

            return time;
        }

        ///////////////////////////////////////////////////////////////////////
        //타임 상수
        ///////////////////////////////////////////////////////////////////////
        //포어백 그라운드
        public const float TIME_FORE_BACK_CATCH = 0.80f;
        public const float TIME_FORE_BACK_END = 1.1f;
        //슬라이딩 그라운드
        public const float TIME_SLIDING_CATCH = 0.5f;//7f;//0.5f;
        public const float TIME_SLIDING_CATCH2 = 0.4f;//35f;//7f;//0.5f;
        public const float TIME_SLIDING_END = 2;//(2.25f - TIME_SLIDING_CATCH);
        public const float TIME_GRONDER_SLIDING_END = 1.2f;//(2.25f - TIME_SLIDING_CATCH);
        //그라운더 센터 무빙
        public const float TIME_GROUNDER_MOVING_CATCH = 0.63f;
        public const float TIME_GROUNDER_MOVING_END = 0.7f;
        //그라운더 센터
        public const float TIME_NORMAL_CATCH = 1;
        //플라이 러닝
        public const float TIME_RUNNING_CATCH = 1.0f;
        public const float TIME_RUNNING_END = 1.5f;//2.33f - TIME_RUNNING_CATCH;  //애니메이션 분리때문에 앞타임과 관계없음
        //플라이 워킹
        public const float TIME_SLOWMOVE_START = 1.1f;
        public const float TIME_SLOWMOVE_END = 0.6f; //0.85f;
        //점핑 캐치
        public const float TIME_JUMPING_END = 1.5f;

        ///////////////////////////////////////////////////////////////////////
        //딜레이 상수
        ///////////////////////////////////////////////////////////////////////
        //송구
        public const float DELAY_THROW_TOSS = 0.15f;    //토스
        public const float DELAY_THROW_NORMAL = 0.3f;   //노멀
        public const float DELAY_THROW_ONESTEP = 0.6f;//54f; //원스텝 오버
        public const float DELAY_THROW_TWOSTEP = 1.55f; //투스텝 오버
        public const float DELAY_THROW_SIDESPIN = 0.95f;  //원스텝 사이드 스핀
        public const float DELAY_THROW_JUMPING = 0.55f;//45f;//0.78f;//0.73f;  //점핑
        public const float DELAY_THROW_SIDEQUICK = 0.15f;    //사이드 퀵
        public const float DELAY_THROW_SIDEDASH = 0.3f;    //사이드 대쉬
        public const float DELAY_THROW_OVEROUTFIELD = 0.6f; //외야 오버
        public const float DELAY_SIT_THROW = 0.15f;     //앉아쏴
        public const float DELAY_THROW_LASER = 1.56f;     //레이저
        public const float DELAY_HOMESTEAL_WAIT = 0.5f; //딜레이드스틸 웨이틸 딜레이

        //필딩
        public const float DELAY_DASHCATCH = 0.6f;          //대쉬 캐치 딜레이
        public const float DELAY_COVER = 0.45f;          //커버 딜레이
        public const float DELAY_TAGGING = 0.4f;        //태그 딜레이
        public const float DELAY_CRASH = 0.5f;          //충돌 딜레이
        public const float DELAY_PICKOFF = 0.3f;
        public const float DELAY_LONGTAG = 0.23f;

        //에러
        public const float DELAY_CATCH_ERROR = 1.0f;
        public const float DELAY_ERROR_CHECK = 0.5f;

        //충돌
        public const float DELAY_CATCHER_BLOCK = 1.0f;
        public const float DELAY_CATCHER_CRUSH = 2.0f;
        public const float DELAY_DP_STOP = 3.0f;
        public const float DELAY_FENCE_COLLISION = 0.85f;// 2.0f;
        public const float DELAY_FIELDER_COLLISION = 1.0f;// 2.4f;

        ///////////////////////////////////////////////////////////////////////
        //범위 상수
        ///////////////////////////////////////////////////////////////////////
        public const int RANGE_FIELDING = 50;
        public const int RANGE_OUTFIELDER_WAITING = 150;    //대쉬 관련
        public const int RANGE_OUT_DASH_CHECK_SPEED = 40;   //대쉬를 결정짓는 스피드 - 외야
        public const int RANGE_IN_DASH_CHECK_SPEED = 10;    //대쉬를 결정짓는 스피드 - 내야//20;
        public const int RANGE_MINIMUM_RELAY = 550;//200;     //최소 릴레이 거리

        //점핑 캐치 레인지
        public const int RANGE_JUMPING_CHTCH_HEIGHT = 140; //점핑 캐치
        public const float RANGE_JUMPING_CHTCH_SLOPE = 0.20f; //점핑 캐치

        ///////////////////////////////////////////////////////////////////////
        //위치조정 옵셋
        ///////////////////////////////////////////////////////////////////////
        public const int MOVING_GROUNDER_OFFSET_FORE = -25;//15;
        public const int MOVING_GROUNDER_OFFSET_BACK = 20;//10;

        public const int DIVING_GROUNDER_OFFSET_FORE = -20;//15;
        public const int DIVING_GROUNDER_OFFSET_BACK = 20;//10;

        public const int MOVING_GROUNDER2_OFFSET_FORE = -15;//15;
        public const int MOVING_GROUNDER2_OFFSET_BACK = 15;//10;

        //1루 베이스 커버 한계 조정 앵글
        public const int FIRSTBASE_COVER_LIMIT_ANGLE = -25;

        //베이스 옵셋
        /*public static int[,] baseOffset = new int[4, 2]
        {
            {-10,0},{0,15},{0,24},{0,0} //30
        };*/
        public static int[,] baseOffset = new int[4, 2]
        {
            {0,0},{0,0},{0,24},{0,10} //30
        };

        //포수 필딩 범위
        public const float CATCHER_FIELDING_BALLSPEED = 300;

        //스페셜 바운드 처리가 가능한 볼스피드
        public const float SPECIAL_GROUNDER_BALLSPEED = 1300;



        //////////////////////////////////////////////////////////////////////
        //기타
        ///////////////////////////////////////////////////////////////////////
        //애니메이션 text
        public const string _HOLD = "0000_HOLD_",
                         _RUN = "0100_RUN_",
                         _GROUNDBALL_RUN = "0101_GROUNDBALL_RUN_",
                         //_WALK = "0100_RUN_",
                         _CATCHER_HOLD = "9000_CATCHER_HOLD_",
                         _CATCHER_RUN = "9001_CATCHER_RUN_",
                         _CATCHER_WALK = "0100_RUN_",
                         _CRASH = "0300_CRASH_",

                         _GROUNDBALL_CENTER = "2000_GROUNDBALL_CENTER2_",
                         _GROUNDBALL_CENTER10 = "2000_GROUNDBALL_CENTER_",
                         _GROUNDBALL_CENTER20 = "2000_GROUNDBALL_CENTER3_",
                         _GROUNDBALL_CENTER_HIGH = "2001_GROUNDBALL_CENTER_HIGH_",
                         _GROUNDBALL_CENTER_OUTFIELD = "2003_GROUNDBALL_CENTER_OUTFIELD_",
                         _GROUNDBALL_CENTER2 = "2010_GROUNDBALL_CENTER2_",
                         _GROUNDBALL_CENTER2_CATCH = "2011_GROUNDBALL_CENTER2_",
                         _GROUNDBALL_CENTER2_HIGH = "2010_GROUNDBALL_CENTER2_",
                         _GROUNDBALL_CENTER2_OUTFIELD = "2011_GROUNDBALL_CENTER2_OUTFIELD_",
                         _GROUNDBALL_FORE_BACK = "2020_GROUNDBALL_FORE_BACK_",
                         _GROUNDBALL_FORE_BACK_NOCATCH = "2020_GROUNDBALL_FORE_BACK_NOCATCH_",
                         _GROUNDBALL_FORE_BACK2 = "2021_GROUNDBALL_FORE_BACK_",
                         _GROUNDBALL_FORE_BACK_HIGH = "2021_GROUNDBALL_FORE_BACK_HIGH_",
                         _GROUNDBALL_SLIDING = "2030_GROUNDBALL_SLIDING_",
                         _GROUNDBALL_SLIDING2 = "2031_GROUNDBALL_SLIDING_",
                         _GROUNDBALL_DASH = "2040_GROUNDBALL_DASH_",
                         _GROUNDBALL_DASHCATCH = "2041_GROUNDBALL_DASHCATCH_",
                         _GROUNDBALL_DASHCATCH2 = "2042_GROUNDBALL_DASHCATCH2_",
                         _GROUNDBALL_ANYTYPE = "2900_GROUNDBALL_ANYTYPE_",

                         _CATCHER_GROUNDBALL = "9200_CATCHER_GROUND_CATCH_",

                         _FLYBALL_CENTER = "3000_FLYBALL_CENTER_",
                         _FLYBALL_CENTER_DASH = "3001_FLYBALL_CENTER_DASH_",
                         _FLYBALL_CENTER_JUMP = "3002_FLYBALL_CENTER_JUMP_",
                         _FLYBALL_SLOWMOVE = "3010_FLYBALL_SLOWMOVE_",
                         _FLYBALL_SLOWCATCH = "3011_FLYBALL_SLOWCATCH_",
                         _FLYBALL_FASTMOVE = "3020_FLYBALL_FASTMOVE_",
                         _FLYBALL_FASTCATCH = "3021_FLYBALL_FASTCATCH_",
                         _FLYBALL_SLIDING = "3030_FLYBALL_SLIDING_",
                         _FLYBALL_JUMPING = "3002_FLYBALL_CENTER_JUMP_",
                         _FLYBALL_SPECIAL_HOMERUNSTEAL = "3040_FLYBALL_SPECIAL_HOMERUNSTEAL_",

                         _CATCHER_FLYBALL = "9300_CATCHER_FLY_CATCH_",
                         _CATCHER_FLYBALL_SLOWMOVE = "9301_CATCHER_FLYBALL_SLOWMOVE_",

                        _DOUBLEPLAY_2B_MOVE = "4000_DOUBLEPLAY_2B_MOVE",
                        _DOUBLEPLAY_SS_MOVE = "4010_DOUBLEPLAY_SS_MOVE",
                        _GROUNDBALL_ERROR_CENTER = "4100_GROUNDBALL_ERROR_CENTER_",
                        _GROUNDBALL_ERROR_FORE = "4110_GROUNDBALL_ERROR_FORE",
                        _GROUNDBALL_ERROR_FORE_HIGH = "4111_GROUNDBALL_ERROR_FORE_HIGH",
                        _GROUNDBALL_ERROR_BACK = "4120_GROUNDBALL_ERROR_BACK",
                        _GROUNDBALL_ERROR_BACK_HIGH = "4121_GROUNDBALL_ERROR_BACK_HIGH",
            //  _GROUNDBALL_ERROR_BACK =  "4130_GROUNDBALL_ERROR_BACK",
            //  _GROUNDBALL_ERROR_BACK_HIGH = "4131_GROUNDBALL_ERROR_BACK_HIGH",
                        _GROUNDBALL_ERROR_DASH = "4140_GROUNDBALL_ERROR_DASH_",
                        _FLYBALL_ERROR_CENTER = "4150_FLYBALL_ERROR_CENTER_",
                        _FLYBALL_MOVE_ERROR = "4160_FLYBALL_MOVE_ERROR_",
                        _FLYBALL_ERROR_DASH = "4170_FLYBALL_ERROR_DASH_",
                        _DELAY_ROTATE_E_TO = "4200_DELAY_ROTATE_E_TO_",
                        _DELAY_ROTATE_W = "4201_DELAY_ROTATE_W_TO_",
                        _DELAY_TO_STOP = "4210_DELAY_TO_STOP_",
                        _DELAY_COLLISION_FENCE = "4260_DELAY_COLLISION_FENCE_",
                        _TAGRUNNER_1B_MOVE = "4300_TAGRUNNER_1B_MOVE_",
                        _TAGRUNNER_2B_MOVE = "4301_TAGRUNNER_2B_MOVE_",

                        _THROW_NORMAL = "5000_THROW_NORMAL_",
                        _THROW_CATCHER_NORMAL = "9500_CATCHER_THROW_",
                        _THROW_CATCHER_SIT = "9510_CATCHER_THROW_SIT_",
                        _THROW_OUTFIELDER_NORMAL = "5013_OVERTHROW_OUTFIELD_ONESTEP_",
                        //_OVERTHROW_INFIELD_ONESTEP = "5010_OVERTHROW_INFIELD_ONESTEP_",
                        //_OVERTHROW_INFIELD_TWOSTEP = "5011_OVERTHROW_INFIELD_TWOSTEP_",
                        _OVERTHROW_INFIELD_JUMPING = "5012_OVERTHROW_INFIELD_JUMPING_",
                        _SIDETHROW_INFIELD_SPIN = "5020_SIDETHROW_INFIELD_SPIN_",
                        _SIDETHROW_INFIELD_NOSTEP_QUICK = "5021_SIDETHROW_INFIELD_NOSTEP_QUICK_",
                        _SIDETHROW_INFIELD_DASH = "5022_SIDETHROW_INFIELD_DASH_",
                        _TOSS_INFIELD = "5030_TOSS_INFIELD_",//"5030_TOSS_INFIELD_",

                        _GLOVE_TOSS_INFIELD = "5031_GLOVE_TOSS_INFIELD_",

                        _BALLCATCH_FORCEOUT = "6000_BALLCATCH_FORCEOUT_",
                        _BALLCATCH_TAGOUT = "6001_BALLCATCH_TAGOUT_",
                        _BALLCATCH_NORMAL = "6002_BALLCATCH_NORMAL_",

                        _TAG = "6100_TAG_",
                        _CATCHER_BALL_CATCH = "9600_CATCHER_THROW_CATCH_",
                        _CATCHER_TAG = "9700_CATCHER_TAG_",
                        _CATCHER_BLOCK = "9701_CATCHER_BLOCK_";
            


        //초기 포지션별 방향
        public static int[] _InitFielderDir = new int[9]//=
        {
	        FieldParm._SOUTH,			//투수
		    FieldParm._NORTH,			//포수
            FieldParm._SOUTHWEST ,	//1루
            FieldParm._SOUTH,			//2루
            FieldParm._SOUTHEAST ,	//3루
            FieldParm._SOUTH,			//SS
            FieldParm._SOUTHEAST,		//LF
            FieldParm._SOUTH,			//CF
            FieldParm._SOUTHWEST,		//RF
        };

        public static float[] _InitDashRatio = new float[9]
        {
            1,
            1,
            0.81f,      //1루수
            0.75f,      //2루수
            0.81f,      //3루수
            0.75f,      //유격수
            0,
            0,
            0
        };
    }
}
