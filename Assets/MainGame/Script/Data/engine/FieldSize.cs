using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class FieldSize
    {

        public const float GROUND_SIZEW = 4340;
        public const float GROUND_SIZEWC = 2170;
        public const float GROUND_SIZEH = 2420;

        //필드 타입 3 (0.7)
        public static float getWidth() //GROUND_SIZEW
        {
            return GROUND_SIZEW;//3600;
        }

        public static float getWidthC() //GROUND_SIZEWC
        {
            return GROUND_SIZEWC;//1800;
        }

        public static float getHeight() //GROUND_SIZEH
        {
            if (Mode.stadiumType == Mode.StadiumType.Dome)
            {
                return 3274;// (2934 + 340);
            }
            else
            {
                return 3100;// (2760 + 340);
            }
        }

        //유니트 단위 -> 수치해석을 통한 그래픽 산출의 기본 단위
        public static float getUnitBaseDistance() //UNIT_BASE_DISTANCE
        {
            return 506;
        }
        public static float getUnitBaseDistanceLeftView() //UNIT_BASE_DISTANCE_LEFTVIEW
        {
            return 364;
        }

        //Home	
        public static float getHomePosX() //HOME_POSX
        {
            return 2165;// 1887 + 278;                        /// getWidthC();
        }

        public static float getHomePosY() //HOME_POSY
        {
            return 332;// 261 + 71;                         // (GROUND_SIZEH - 2041);
        }

        //Mound
        public static float getMoundPosX() //MOUND_POSX
        {
            return getHomePosX();                                                       // getWidthC();
        }

        public static float getMoundPosY() //MOUND_POSY
        {
            return getHomePosY() + 373;             //634    //(GROUND_SIZEH - 1668);//x 1.15
        }

        //First	
        public static float getFirstBasePosX() //FIRST_BASE_POSX
        {
            return getHomePosX() + 616;             //(GROUND_SIZEW - 1271);
        }

        public static float getFirstBasePosY() //FIRST_BASE_POSY
        {
            return getHomePosY() + 373;             //634;// (GROUND_SIZEH - 1668);//x 1.15
        }

        //Second	
        public static float getSecondBasePosX() //SECOND_BASE_POSX
        {
            return getHomePosX();                   // (GROUND_SIZEWC);
        }

        public static float getSecondBasePosY() //SECOND_BASE_POSY
        {
            return getHomePosY() + 690;             // 951;// (GROUND_SIZEH - 1351);
        }

        //Third	
        public static float getThirdBasePosX() //THIRD_BASE_POSX
        {
            return getHomePosX() - 616;             //1271;// x1.3
        }

        public static float getThirdBasePosY() //THIRD_BASE_POSY
        {
            return getHomePosY() + 373;
        }

        /*
        //Left Pole
        public static float getLeftPolePosX() //LEFT_POLE_POSX
        {
            return getHomePosX() - 1752;             //68;
        }

        public static float getLeftPolePosY()//float ratio) //LEFT_POLE_POSY
        {
            return getHomePosY() + 1040;             //(GROUND_SIZEH - 962);
        }*/

        /*
        //Right Pole
        public static float getRightPolePosX() //RIGHT_POLE_POSX
        {
            return getHomePosX() + 1752;             //(GROUND_SIZEW - 68);
        }

        public static float getRightPolePosY() //RIGHT_POLE_POSY
        {
            return getHomePosY() + 1040;             //(GROUND_SIZEH - 962);
        }*/

        //Bench
        public static float getHomeBenchPosX() //HOME_BENCH_POSX
        {
            return getHomePosX() + 744;             //(GROUND_SIZEW - 1386);//
        }
        public static float getHomeBenchPosY() //HOME_BENCH_POSY
        {
            return getHomePosY() - 84;        //0;
        }
        public static float getAwayBenchPosX() //AWAY_BENCH_POSX
        {
            return getHomePosX() - 744;             //1386;
        }
        public static float getAwayBenchPosY() //AWAY_BENCH_POSY
        {
            return getHomePosY() - 84;        //0;
        }

        
        ///////////////////////////////////////////////////
        //펜스
        ///////////////////////////////////////////////////
        //펜스 - 타원 기준
        //public const int CENTER_FENCEX = GROUND_SIZEWC;
        public static float getCencterFenceY()// CENTER_FENCEY 
        {
            return (GROUND_SIZEH - 304);  //(GROUND_SIZEH - 258);
        }

        public const int _FENCE_HEIGHT = 130;


        public static float getLeftSideFenceX1()// _LEFT_SIDE_FENCE_X1 
        {
            return FENCE_LEFT_POLE_X;
        }

        public static float getLeftSideFenceX2()// _LEFT_SIDE_FENCE_X2 
        {
            return 1347;// 898 * 1.5f;//1368;
        }

        public static float getRightSideFenceX1()// _RIGHT_SIDE_FENCE_X1 
        {
            return FENCE_RIGHT_POLE_X;
        }

        public static float getRightSideFenceX2()// _RIGHT_SIDE_FENCE_X2 
        {
            return (GROUND_SIZEW - 1347);//898 * 1.5f);
        }

        public static float getSideFenceY1()//float ratio) // _SIDE_FENCE_Y1 
        {
            return FENCE_LEFT_POLE_Y;
        }

        public static float getSideFenceY2() // _SIDE_FENCE_Y2 = 0
        {
            return 0;
        }

        public static float getLeftFenceSlope()// _LEFT_FENCE_SLOPE 
        {
            return (getSideFenceY2() - getSideFenceY1()) / (getLeftSideFenceX2() - getLeftSideFenceX1());
        }

        public static float getRightFenceSlope()//_RIGHT_FENCE_SLOPE
        {
            return (getSideFenceY2() - getSideFenceY1()) / (getRightSideFenceX2() - getRightSideFenceX1());
        }

        public static float getFenceOriginY()//float ratio) //_FENCE_ORIGIN_Y
        {
            return FENCE_LEFT_POLE_Y;//ratio);
        }

        //폴
        public static float FENCE_LEFT_POLE_X = 2165 - 1752;
        public static float FENCE_LEFT_POLE_Y = 332 + 1040;

        public static float FENCE_RIGHT_POLE_X = 2165 + 1752;
        public static float FENCE_RIGHT_POLE_Y = 332 + 1040;      


        //펜스 - 직선 기준 //여기부터
        public static float FENCE_LEFT_POINT_X1 = 722;
        public static float FENCE_LEFT_POINT_Y1 = (GROUND_SIZEH - 717);

        public static float FENCE_LEFT_POINT_X2 = 1168;
        public static float FENCE_LEFT_POINT_Y2 = (GROUND_SIZEH - 488);

        public static float FENCE_LEFT_POINT_X3 = 1642;
        public static float FENCE_LEFT_POINT_Y3 = (GROUND_SIZEH - 352);


        public static float FENCE_LEFT_POINT_X5 = 2076;
        public static float FENCE_LEFT_POINT_Y5 = (GROUND_SIZEH - 305);


        public static float FENCE_RIGHT_POINT_X1 = (GROUND_SIZEW - 722);
        public static float FENCE_RIGHT_POINT_Y1 = FENCE_LEFT_POINT_Y1;

        public static float FENCE_RIGHT_POINT_X2 = (GROUND_SIZEW - 1168);
        public static float FENCE_RIGHT_POINT_Y2 = FENCE_LEFT_POINT_Y2;

        public static float FENCE_RIGHT_POINT_X3 = (GROUND_SIZEW - 1642);
        public static float FENCE_RIGHT_POINT_Y3 = FENCE_LEFT_POINT_Y3;

        //public const float FENCE_RIGHT_POINT_X4 = (GROUND_SIZEW - 1338);
        //public const float FENCE_RIGHT_POINT_Y4 = FENCE_LEFT_POINT_Y4;

        public static float FENCE_RIGHT_POINT_X5 = (GROUND_SIZEW - 2076);
        public static float FENCE_RIGHT_POINT_Y5 = FENCE_LEFT_POINT_Y5;


        //수비 관련 상수
        // x-y 비율
        /*public static float getYAxisCoeff()//_Y_AXIS_COEFF
        {
            return 0.6f;// (getSecondBasePosY(ratio) - getHomePosY(ratio)) / (getFirstBasePosX() - getThirdBasePosX());
        }*/
        // 얕은 플라이 기준
        public static float getShallowOutFieldFlyDistance() //SHALLOW_OUTFIED_FLY_DISTANCE
        {
            return getHomePosY() + 1100;
        }
        // 인필드 플라이 기준
        public static float getInfieldFlyDistance() //INFIED_FLY_DISTANCE
        {
            return getHomePosY() + 697;
        }
        // 얼리 땅볼 기준
        public static float getEarlyGrounderDistance() //EARLY_GROUNDER_DISTANCE
        {
            return getMoundPosY();
        }


        public static float getBasePosX(int index) // BASE_POSITION, BASECOVER_POSITION
        {
            if (index == 0) return getFirstBasePosX();
            else if (index == 1) return getSecondBasePosX();
            else if (index == 2) return getThirdBasePosX();
            else return getHomePosX();
        }

        public static float getBasePosY(int index) // BASE_POSITION, BASECOVER_POSITION
        {
            if (index == 0) return getFirstBasePosY();
            else if (index == 1) return getSecondBasePosY();
            else if (index == 2) return getThirdBasePosY();
            else return getHomePosY();
        }

        public static float getFielderPosX(int index) //_FIELDER_POS
        {  
            if (index == 0) return getMoundPosX();
            else if (index == 1) return getHomePosX();
            else if (index == 2) return getHomePosX() + 577;
            else if (index == 3) return getHomePosX() + 319;
            else if (index == 4) return getHomePosX() - 577;
            else if (index == 5) return getHomePosX() - 319;
            else if (index == 6) return getHomePosX() - 1022;
            else if (index == 7) return getHomePosX();
            else return getHomePosX() + 1022;
        }

        public static float getFielderPosY(int index) //_FIELDER_POS
        {            
            if (index == 0) return getMoundPosY();
            else if (index == 1) return getHomePosY() - 24;
            else if (index == 2) return getHomePosY() + 474;
            else if (index == 3) return getHomePosY() + 707;
            else if (index == 4) return getHomePosY() + 474;
            else if (index == 5) return getHomePosY() + 707;
            else if (index == 6) return getHomePosY() + 1168;
            else if (index == 7) return getHomePosY() + 1513;
            else return getHomePosY() + 1168;
        }

        public static float getBuntPosX(int index, bool bInfieldHit = false) //_FIELDER_POS
        {
            if (index == 0) return getMoundPosX();
            else if (index == 1) return getHomePosX();
            else if (index == 2) return getHomePosX() + 577 - 100;
            else if (index == 3) return getHomePosX() + 319;
            else if (index == 4) return getHomePosX() - 577 + 100;
            else  return getHomePosX() - 319;

        }

        public static float getBuntPosY(int index, bool bInfieldHit = false) //_FIELDER_POS
        {
            if (index == 0) return getMoundPosY();
            else if (index == 1) return getHomePosY() - 24;
            else if (index == 2) return getHomePosY() + 474 - 100;
            else if (index == 3) return getHomePosY() + 707;
            else if (index == 4) return getHomePosY() + 474 - 100;
            else return getHomePosY() + 707;

        }


        public static float getJudgePosX(int index) //_FIELDER_POS
        {
            /*
            if (index == FieldParm.HOMEBASE_INDEX) return getHomePosX();  //홈
            else if (index == FieldParm.FIRSTBASE_INDEX) return getHomePosX() + 731; //1
            else if (index == FieldParm.SECONDBASE_INDEX) return getHomePosX();    //2
            else return getHomePosX() - 731;    /*/

            if (index == FieldParm.HOMEBASE_INDEX) return getHomePosX();  //홈
            else if (index == FieldParm.FIRSTBASE_INDEX) return getHomePosX() + 858; //1
            else if (index == FieldParm.SECONDBASE_INDEX) return getHomePosX();    //2
            else return getHomePosX() - 858;
        }

        public static float getJudgePosY(int index) //_FIELDER_POS
        {
            /*
            if (index == FieldParm.HOMEBASE_INDEX) return getHomePosY(ratio) - 85;
            else if (index == FieldParm.FIRSTBASE_INDEX) return getHomePosY(ratio) + 405;
            else if (index == FieldParm.SECONDBASE_INDEX) return getHomePosY(ratio) + 692;
            else return getHomePosY(ratio) + 405; */

            if (index == FieldParm.HOMEBASE_INDEX) return getHomePosY() - 85;
            else if (index == FieldParm.FIRSTBASE_INDEX) return getHomePosY() + 410;
            else if (index == FieldParm.SECONDBASE_INDEX) return getHomePosY() + 770;
            else return getHomePosY() + 410;
        }

        public static float getRunnerInitPosX(int index) //RunnerInitPos
        {
            if (index == 0) return getFirstBasePosX();
            else if (index == 1) return getSecondBasePosX();
            else if (index == 2) return getThirdBasePosX();
            else return getHomePosX();
        }
        public static float getRunnerInitPosY(int index) //RunnerInitPos
        {
            if (index == 0) return getFirstBasePosY() + 4;
            else if (index == 1) return getSecondBasePosY();
            else if (index == 2) return getThirdBasePosY() + 4;
            else return getHomePosY();
        }

        public static float getFirstBasemanSiftPosX() //_FIRSTBASEMAN_SHIFT_POS
        {
            return getHomePosX() + 512;
        }

        public static float getFirstBasemanSiftPosY() //_FIRSTBASEMAN_SHIFT_POS
        {
            return getHomePosY() + 380;
        }


        //쉬프트 포메이션
        //내야 기본 쉬프트
        //public static int[] _DOUBLEPLAY_OFFSET = new int[2] { 50, 50 };
        //외야 기본 쉬프트
        //전진 쉬프트
        //특수 쉬프트
        //

        public static float getFieldCenterFenceDistance() //_FIELD_CENTER_FENCE_DISTANCE
        {
            return getCencterFenceY() - getHomePosY();
        }

        public static float getFieldFenceSecondDistance() //_FIELD_FENCE_SECOND_DISTANCE
        {
            return getCencterFenceY() - getSecondBasePosY();
        }

        public static float getFieldSecondHomeDistance() //_FIELD_SECOND_HOME_DISTANCE
        {
            return getSecondBasePosY() - getHomePosY();
        }

    }
}