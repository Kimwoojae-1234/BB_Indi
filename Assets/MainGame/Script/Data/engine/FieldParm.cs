

using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class FieldParm
    {
        //에러 타입
        public enum ErrorType
        {
            None,               //에러 안함
            Drop,               //드랍
            Fumble,             //펌블
            Tunnel,             //알까기
            Juggle,
            WildThrow,          //악송구
            WildThrowUp,        //악송구 위로
            WildWrongPlace      //잘못된위치로(외야 전용)

        }

        //와일드 피치 케이스
        public enum WildPitchCase
        {
            NoRunner,
            RunnerOnBase,
            NotOut,
            BaseOnBall
        };

        public enum ThrowAgain
        {
            Available,
            NoThrow,
            BackToPosition
        }

        public enum BattingViewFieldingType
        {
            None,
            NormalGrounder,
            SpecialGrounder,
            JumpCatch
        }
        
        /////////////////////////////////////////////////////////////////////////////////////////
        //엔진에서만 사용하는 함수
        /////////////////////////////////////////////////////////////////////////////////////////
        //야수의 방향을 구한다
        public static int getDir(int dx, int dy, int offset)
        {
            if (MyMath.Abs(dy) < offset)
            {
                if (dx > 0) return _EAST;
                else return _WEST;

            }
            else if (MyMath.Abs(dx) < offset)
            {
                if (dy < 0) return _SOUTH; //if (dy > 0) return _SOUTH;
                else return _NORTH;
            }
            else
            {

                if (dy < 0) //if (dy > 0)
                {
                    if (dx > 0) return _SOUTHEAST;
                    else return _SOUTHWEST;

                }
                else
                {
                    if (dx > 0) return _NORTHEAST;
                    else return _NORTHWEST;

                }
            }
        }
        
        //공을 쳐다보는 필더의 방향을 리턴
        public static int getDir(float angleDir)
        {
            int angle = (int)(angleDir * Mathf.Rad2Deg);


            int dir = (((360 + angle + 23) % 360) / 45) % 8;

            if (dir >= 8 || dir < 0)
            {
                dir = 0;
                ///////////UnityEngine.//Debug.Log("=======================>> posindex = " + posIndex + "=====>>dir = " + dir);
            }
            //////////UnityEngine.//Debug.Log("=======================>> posindex = " + posIndex + "=====>>dir = " + dir);
            return FieldParm.DIR[dir];

        }

        public static int getDir2(float dx, float dy)
        {
            int dir;
            if (dx > 0)
            {
                if (Mathf.Abs(dy) > Mathf.Abs(dx))
                {
                    if (dy > 0) dir = FieldParm._NORTH;
                    else dir = FieldParm._SOUTH;
                }
                else
                {
                    dir = FieldParm._WEST;
                }
            }
            else
            {
                if (Mathf.Abs(dy) > Mathf.Abs(dx))
                {
                    if (dy > 0) dir = FieldParm._NORTH;
                    else dir = FieldParm._SOUTH;
                }
                else
                {
                    dir = FieldParm._EAST;
                }
            }

            return dir;
        }

#if _RewindMode 
        //시뮬레이션에 의해 계산된 타구를 엔진에 맞게 변형해주는 함수
        public static Vector3 GetHitVectorBySimul(SimulBattingData result)
        {
            Vector3 vector;
            float power, angle, dir;

            int fIndex = result.fIndex; //처리하는 수비수
            ////UnityEngine.Debug.Log("[처리 수비수]===================================>>" + fIndex);
            ////UnityEngine.Debug.Log("[타구 타입]===================================>>" + result.hitType);

            dir = getDirBySimul(fIndex);

            if (result.hitType == SimulHitType.Grounder)
            {
                ////UnityEngine.Debug.Log("[그라운더 타입]===================================>>" + result.grounderType);
                Vector2 grounder = getGrounderPower(result.grounderType); //타입
                power = grounder.x;
                angle = grounder.y;

            }
            else if (result.hitType == SimulHitType.Fly)
            {
                ////UnityEngine.Debug.Log("[플라이 타입]===================================>>" + result.flyType);
                Vector2 fly = getFlyPower(result.flyType);
                power = fly.x;
                angle = fly.y;
            }
            else //Liner
            {
                ////UnityEngine.Debug.Log("[라이너 타입]===================================>>" + result.linerType);
                Vector2 liner = getLinerPower(result.linerType, result.result);
                power = liner.x;
                angle = liner.y;
            }

            vector.x = power;
            vector.y = angle;
            vector.z = dir;


            return vector;
        }
#endif
        //방향
        static float getDirBySimul(int fIndex)
        {
            float dir = 0;
            if (fIndex == CPlayer._PITCHER)
            {
                dir = Random.Range(-3, 3);
            }
            else if (fIndex == CPlayer._CATCHER)
            {
                dir = Random.Range(-20, 20);
            }
            else if (fIndex == CPlayer._FIRSTBASEMAN)
            {
                dir = Random.Range(-40, -35);
            }
            else if (fIndex == CPlayer._SECONDBASEMAN)
            {
                dir = Random.Range(-15, -25);
            }
            else if (fIndex == CPlayer._THIRDBASEMAN)
            {
                dir = Random.Range(35, 40);
            }
            else if (fIndex == CPlayer._SHORTSTOP)
            {
                dir = Random.Range(15, 25);
            }
            else if (fIndex == CPlayer._LEFTFIELDER)
            {
                dir = Random.Range(25, 35);
            }
            else if (fIndex == CPlayer._CENTERFIELDER)
            {
                dir = Random.Range(-10, 10);
            }
            else// if (fIndex == CPlayer._RIGHTFIELDER)
            {
                dir = Random.Range(-35, -25);
            }

            if (dir == 0) dir = 0.00001f;
            return dir;
        }

        //땅볼 파워
        static Vector2 getGrounderPower(SpecificGrounderType type)
        {
            float power, angle;
            if (type == SpecificGrounderType.Weak)
            {
                power = Random.Range(15f, 20f);
                angle = Random.Range(-30f, 5f);
            }
            else if (type == SpecificGrounderType.BigBound)
            {
                power = Random.Range(25f, 28f);
                angle = Random.Range(-40f, -30f);
            }
            else if (type == SpecificGrounderType.Normal)
            {
                power = Random.Range(20f, 26f);
                angle = Random.Range(-25f, 5f);
            }
            else if (type == SpecificGrounderType.Solid)
            {
                power = Random.Range(26f, 30f);
                angle = Random.Range(-25f, 5f);
            }
            else //if(type == SpecificGrounderType.VerySolid)
            {
                power = Random.Range(30f, 35f);
                angle = Random.Range(-25f, 5f);
            }

            Vector2 vector;
            vector.x = power;
            vector.y = angle;
            return vector;
        }

        //플라이볼 파워
        static Vector2 getFlyPower(SpecificFlyType type)
        {
            float power, angle;
            if (type == SpecificFlyType.InfieldPopup_Fair)
            {
                power = Random.Range(25f, 30f);
                angle = Random.Range(60f, 70f);
            }
            else if (type == SpecificFlyType.InfieldPopup_Foul)
            {
                power = Random.Range(25f, 30f);
                angle = Random.Range(60f, 70f);
            }
            else if (type == SpecificFlyType.CatcherPopup)
            {
                power = Random.Range(25f, 30f);
                angle = Random.Range(80f, 90f);
            }
            else if (type == SpecificFlyType.OutfieldPopup)
            {
                power = Random.Range(25f, 30f);
                angle = Random.Range(40f, 60f);
            }
            else if (type == SpecificFlyType.OutfieldShort)
            {
                power = Random.Range(25f, 28f);
                angle = Random.Range(25f, 40f);
            }
            else if (type == SpecificFlyType.OutfieldHighFly)
            {
                power = Random.Range(28f, 32f);
                angle = Random.Range(35f, 50f);
            }
            else if (type == SpecificFlyType.OutfieldOverHead)
            {
                power = Random.Range(30f, 32f);
                angle = Random.Range(30f, 45f);
            }
            else //if(type == SpecificFlyType.OutfieldHomerun)
            {
                power = Random.Range(35f, 40f);
                angle = Random.Range(30f, 45f);
            }
            Vector2 vector;
            vector.x = power;
            vector.y = angle;
            return vector;
        }

        //라이너 파워
        static Vector2 getLinerPower(SpecificLinerType type, SimulResultState state)
        {
            float power, angle;
            if (type == SpecificLinerType.Weak)
            {
                power = Random.Range(24f, 26f);
                if (state == SimulResultState.FlyOut) angle = Random.Range(12f, 17f);
                else angle = Random.Range(5f, 25f);
            }
            else if (type == SpecificLinerType.Normal)
            {
                power = Random.Range(26f, 30f);
                if (state == SimulResultState.FlyOut) angle = Random.Range(12f, 17f);
                else angle = Random.Range(5f, 25f);
            }
            else if (type == SpecificLinerType.Solid)
            {
                power = Random.Range(30, 35);
                if (state == SimulResultState.FlyOut) angle = Random.Range(12f, 17f);
                else angle = Random.Range(10f, 25);
            }
            else //if (type == SpecificLinerType.VerySolid)
            {
                power = Random.Range(35, 40);
                if (state == SimulResultState.FlyOut) angle = Random.Range(12f, 17f);
                else angle = Random.Range(5, 25);
            }

            Vector2 vector;
            vector.x = power;
            vector.y = angle;
            return vector;
        }



        public static float GetBuntPower(SimulBuntType buntType,bool bBuntSuccess,int buntFielder, bool bBuntFly)
        {
            float value = 8;
            if (bBuntFly == true)
            {
                if (buntFielder <= CPlayer._CATCHER)
                {
                    value = Random.Range(10f, 12.0f);
                }
                else
                {
                    value = Random.Range(15.0f, 17.0f);
                }
            }
            else
            {
                if (buntType == SimulBuntType.DRAG)
                {
                    if (bBuntSuccess == false)
                    {
                        //드래그 실패
                        value = Random.Range(10.0f, 13.5f);
                    }
                    else
                    {
                        //드래그 성공
                        if (buntFielder == CPlayer._THIRDBASEMAN || buntFielder == CPlayer._FIRSTBASEMAN)
                        {
                            //3루수쪽
                            value = Random.Range(8.0f, 8.5f);
                        }
                        else if (buntFielder == CPlayer._CATCHER)
                        {
                            value = Random.Range(6.15f, 6.5f);
                        }
                        else
                        {
                            //투수 확정
                            value = Random.Range(6.8f, 8.0f);
                        }

                    }
                }
                else //희생 스퀴즈
                {
                    if (bBuntSuccess == true)
                    {
                        if (buntFielder == CPlayer._PITCHER)
                        {
                            if (buntType == SimulBuntType.SACRIFY)
                                value = Random.Range(5.8f, 7.0f);
                            else
                                value = Random.Range(6.8f, 7.4f);
                        }
                        else  if(buntFielder == CPlayer._CATCHER)
                        {
                            if (buntType == SimulBuntType.SACRIFY)
                                value = Random.Range(4.5f, 5.5f);
                            else
                                value = Random.Range(6.0f, 6.5f);
                        }
                        else
                        {
                            value = Random.Range(7.5f, 9.0f);
                        }
                    }
                    else
                    {
                        if (buntFielder == CPlayer._PITCHER)
                        {
                            //투수
                            value = Random.Range(8.5f, 9.0f);
                        }
                        else if (buntFielder == CPlayer._CATCHER)
                        {
                            //포수
                            value = Random.Range(3.5f, 4.5f);
                        }
                        else
                        {
                            //코너 내야수
                            value = Random.Range(12.5f, 14.0f);
                        }
                    }
                }
            }
            
            ////UnityEngine.//Debug.Log("================================>>Power = " + value);

            return value;
        }

        public static float GetBuntAngleZ(SimulBuntType buntType, bool bBuntSuccess, int buntFielder, bool bBuntFly)
        {
            float value = -10;
            if (bBuntFly == true)
            {
                if (buntFielder <= CPlayer._CATCHER)
                {
                    value = Random.Range(70.0f, 80.0f);
                }
                else
                {
                    value = Random.Range(50f, 60.0f);
                }
            }
            else
            {
                if (buntType == SimulBuntType.DRAG)
                {
                    if (bBuntSuccess == true)
                    {
                        //드래그 성공
                        if (buntFielder == CPlayer._THIRDBASEMAN)
                        {
                            //코너쪽 - 확정
                            value = Random.Range(-37, -34);
                        }
                        else if(buntFielder == CPlayer._FIRSTBASEMAN)
                        {
                            //1루
                            value = Random.Range(-28f, -20f);
                        }
                        else if (buntFielder == CPlayer._PITCHER)
                        {
                            //투수쪽
                            value = Random.Range(-35f, -30f);
                        }
                        else
                        {
                            //포수쪽 - 확정
                            value = -25;
                        }
                    }
                    else
                    {
                        //드래그 실패
                        value = Random.Range(-30.0f, -10.0f);
                    }
                }
                else //희생 //스퀴즈
                {
                    if (bBuntSuccess == true)
                    {
                        value = Random.Range(-40.0f, -10.0f);
                    }
                    else
                    {
                        if (buntType == SimulBuntType.SACRIFY)
                        {
                            //희생 실패
                            value = Random.Range(-15.0f, 0.0f);
                        }
                        else
                        {
                            //스퀴즈 실패
                            value = Random.Range(-5.0f, 0.0f);
                        }
                    }
                }
            }
            
            ////UnityEngine.//Debug.Log("================================>>AngleZ = " + value);
            return value;
        }

        public static float GetBuntAngleX(SimulBuntType buntType, bool bBuntSuccess, int buntFielder, bool bBuntFly, bool bSqueezeFieldOut)
        {
            float value = 1;
            int foulRange = MyMath.Percent();

            if (bBuntFly == true)
            {
                if (buntFielder <= CPlayer._CATCHER)
                {
                    value = Random.Range(0f, 180.0f);
                }
                else
                {
                    value = Random.Range(0f, 20.0f);
                }
            }
            else
            {
                if (buntType == SimulBuntType.DRAG)
                {
                    if (bBuntSuccess == false && foulRange < BattingMechanism.DRAG_BUNT_FOUL)   //35
                    {
                        //드래그 번트실패 인데 파울 처리
                        if (buntFielder == CPlayer._FIRSTBASEMAN || buntFielder == CPlayer._CATCHER)
                        {
                            value = Random.Range(-55.0f, -100.0f);
                        }
                        else
                        {
                            value = Random.Range(55.0f, 100.0f);
                        }
                    }
                    else
                    {
                        if (buntFielder == CPlayer._CATCHER)
                        {
                            if (bBuntSuccess == true)
                            {
                                value = Random.Range(33.0f, 40.0f);
                            }
                            else
                            {
                                value = Random.Range(0f, 30f);
                            }
                        }
                        else if (buntFielder == CPlayer._FIRSTBASEMAN)
                        {
                            //확정
                            value = -Random.Range(26, 30);
                        }
                        else if (buntFielder == CPlayer._THIRDBASEMAN)
                        {
                            //확정
                            value = Random.Range(32, 44);
                        }
                        else //투수
                        {
                            //확정                   
                            value = Random.Range(27f, 28f);
                        }
                    }
                }
                else if (buntType == SimulBuntType.SACRIFY)//희생
                {
                    if (bBuntSuccess == false && foulRange < BattingMechanism.SAC_BUNT_FOUL)
                    {
                        //희생 번트실패 인데 파울 처리
                        if (buntFielder == CPlayer._FIRSTBASEMAN || buntFielder == CPlayer._CATCHER)
                        {
                            value = Random.Range(-55.0f, -100.0f);
                        }
                        else
                        {
                            value = Random.Range(55.0f, 100.0f);
                        }
                    }
                    else
                    {
                        if (buntFielder == CPlayer._FIRSTBASEMAN)
                        {
                            value = Random.Range(-44.0f, -26.0f);
                        }
                        else if (buntFielder == CPlayer._THIRDBASEMAN)
                        {
                            value = Random.Range(26, 44);
                        }
                        else
                        {
                            //투수쪽
                            value = Random.Range(-12.0f, 12.0f);
                        }
                    }
                }
                else  //스퀴즈
                {
                    //////UnityEngine.//Debug.Log("=========================================>>스퀴즈 X");
                    if (bBuntSuccess == false && bSqueezeFieldOut == false)
                    {
                        //스퀴즈 번트실패 인데 파울 처리
                        if (buntFielder == CPlayer._FIRSTBASEMAN || buntFielder == CPlayer._CATCHER)
                        {
                            value = Random.Range(-55.0f, -80.0f);
                        }
                        else
                        {
                            value = Random.Range(55.0f, 80.0f);
                        }
                    }
                    else
                    {
                        if (buntFielder == CPlayer._FIRSTBASEMAN)
                        {
                            value = Random.Range(-44.0f, -35.0f);
                        }
                        else if (buntFielder == CPlayer._THIRDBASEMAN)
                        {
                            value = Random.Range(35, 44);
                        }
                        else
                        {
                            //투수쪽
                            if (bBuntSuccess == true)
                            {
                                value = (MyMath.Half() ? 1 : -1) * Random.Range(5.0f, 10.0f);
                            }
                            else
                            {
                                //스퀴즈 번트 실패 필드에서 죽음
                                value = Random.Range(-3.0f, 3.0f);
                            }
                        }
                    }
                }
            }            
            ////UnityEngine.//Debug.Log("================================>>AngleX = " + value);
            return value;
        }
        /////////////////////////////////////////////////////////////////////////////////////////
        //기타 상수
        /////////////////////////////////////////////////////////////////////////////////////////
        public const float InitRatio = 0.6f;
        public const float InverseRatio = (1.0f / 0.6f);

#if _OrthoCamera
        public const float InitZoom = 0.8f;// 1;//1.4f;
#else
        public const float InitZoom = 1;
#endif
        public const float InitActiveRatio = 1;
        public const float _DEFAULT_ZOOM_SPEED = 0.01f;


        public const int FIRSTBASE_INDEX = 0;
        public const int SECONDBASE_INDEX = 1;
        public const int THIRDBASE_INDEX = 2;
        public const int HOMEBASE_INDEX = 3;
        public const int RELAY_INDEX = -1;  //가까운 중계
        public const int RELAY_INDEX2 = -2; //먼 중계

       
        public const int BALL_INIT_HEIGHT = 50; //치는
        public const int BALL_INIT_HEIGHT_END = 10; //받는
        public const int BALL_INIT_HEIGHT2 = 150;   //던지는

        public const int JUMPCATCH_HEIGHT = 100;
        public const int STANDINGCATCH_HEIGHT = 45;

        public const int CARRY_DISTANCE = 100;
        public const int TOSS_DISTANCE = 150; //220

        //딜레이
        public const float FENCE_DELAY = 0.5f;  //펜스 충돌시 발생하는 야수의 딜레이
        public const float RELAY_DELAY = 0.2f;  //릴레이시 발생하는 야수의 딜레이

        //밸런스
        public const int AICONTROL = 300;

        //방향
        public const int _NORTH = 0;
        public const int _NORTHEAST = 1;
        public const int _EAST = 2;
        public const int _SOUTHEAST = 3;
        public const int _SOUTH = 4;
        public const int _SOUTHWEST = 5;
        public const int _WEST = 6;
        public const int _NORTHWEST = 7;

        public static int[] DIR = new int[8] { _EAST, _NORTHEAST, _NORTH, _NORTHWEST, _WEST, _SOUTHWEST, _SOUTH, _SOUTHEAST };
        //public static int[] dirConverse = new int[16] { _EAST, _NORTHEAST, _NORTHEAST, _NORTH, _NORTH, _NORTHWEST, _NORTHWEST, _WEST, _WEST, _SOUTHWEST, _SOUTHWEST, _SOUTH, _SOUTH, _SOUTHEAST, _SOUTHEAST,_EAST };
        public static int[,] forceoutOffset = new int[8, 2] { { 0, 30 }, { 25, 10 }, { 30, 0 }, { 25, -10 }, { 0, -30 }, { -25, -10 }, { -30, 0 }, { -25, 10 } };
        public static string[] _dir = new string[8] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        public static string[] _dir2 = new string[8] { "n", "ne", "e", "se", "s", "sw", "w", "nw" };
        public static int[] _angleDir = new int[8] { 90, 45, 0, 315, 270, 225, 180, 135 };
        //public static int[] tKey = new int[5] { _EAST, _NORTH, _WEST, _SOUTH, -1 };


        public const float CHAR_SIZE_RATE = 0.32f;
        public const float JUDGE_SIZE_RATE = 0.35f;
        public const float CHAR_ZOOM_SIZE_RATE = 0.25f;

        /*
        public static string[,] _name = new string[2, 9] 
        {
            {"양현종 14","차일목 14","브렛필 14","안치홍 14","이범호 14","김선빈 14","김주찬 14","이대형 14","신종길 14"},
            {"벤덴헐크 14","진갑용 14","이승엽 14","나바로 14","박석민 14","김상수 14","최형우 14","박해민 14","박한이 14"},
        };*/

        public const int NoLink = -100; //네트워크로부터 값 못받아옴


        public const int NO_DOUBLEPLAY = -1,
                  DOUBLEPLAY_163 = 0,   //투->유(이)->일
                  DOUBLEPLAY_153 = 1,   //투->삼->일
                  DOUBLEPLAY_123 = 2,   //투->포->일
                  DOUBLEPLAY_263 = 10,  //포->유->일
                  DOUBLEPLAY_253 = 11,  //포->삼->일
                  DOUBLEPLAY_361 = 20,  //일->유->투
                  DOUBLEPLAY_351 = 21,  //일->삼->투   (대희박)
                  DOUBLEPLAY_321 = 22,  //일->포->투
                  DOUBLEPLAY_463 = 30,  //이->유->일
                  DOUBLEPLAY_453 = 31,  //이->삼->일   (대희박)
                  DOUBLEPLAY_421 = 32,  //이->포->투   (희박)
                  DOUBLEPLAY_543 = 40,  //삼->이->일                
                  DOUBLEPLAY_553 = 41,  //삼->삼찍고->일   (멋있음 구현하고 싶어)
                  DOUBLEPLAY_523 = 42,  //삼->포->일
                  DOUBLEPLAY_643 = 50,  //유->이->일
                  DOUBLEPLAY_653 = 51,  //유->이->일   (대희박)
                  DOUBLEPLAY_621 = 52,  //유->포->투   (희박)
                  DOUBLEPLAY_ETC = 100,
                  NOT_CHECKED = -2;
    }

}
