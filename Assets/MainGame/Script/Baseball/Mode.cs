using UnityEngine;
using System.Collections;


namespace BaseBall.BallPlay
{
    public class Mode
    {
        public enum GamePlayMode
        {
            Season,
            Ranking,
            Race,
            NineInningTwoOut,
            Pvp,
            Pvp433,
        }

        
        public enum StadiumType
        {
            Jamsil = 1,  
            Dome = 2,    
            LionsPark = 3,
            ChampionsField = 4,
            HanhwaField = 5,
            HappyDream = 6,
        }

        public enum ModeFlag
        {
            Auto,
            Manual
        }

        
        public static GamePlayMode gameMode = Mode.GamePlayMode.Season;
        public static int finalInning = 9;
        public static int maxInning = 12;
         

        //디폴트 타입... 변하지 않음
        //타격/피칭 모드
        public static PicthControlType pitchControlType = PicthControlType.IndicatorType;//
        public static BatControlType batControlType = BatControlType.ReleaseType;
        //파워풀 타격 피칭 모드
        public static bool bPowerfulType = true;


        //자동플레이
        public const bool bAutoPlay = false;      //자동플레이         
        
        //리와인드 플레이
        //public static bool bRewindPlay = false;

        //시뮬레이션 & 찬스모드
        public static bool bSimulationQuickPlay = false;
        public static bool bSiumlSetting = false; //나중 지워
        
        //오직 찬스모드 여부
        public static bool bOnlyChanceMode = false;

        //승부치기(아직 안열음)
        public static bool bTieBreaker = false;      //타이브레이크 모드
        public static bool bPvpMode = false;

        public static bool bPvpMode433 = false;

        //2사만루 모드
        public static bool b2outBaseLoadedMode = false; //

       
        //일시정지 
        public static bool bPauseGame = false;
        public static bool bPauseReady = false;
        
        
        
        public static int stadiumNum = 4;   //1번이 잠실구장
        public static StadiumType stadiumType = StadiumType.ChampionsField;

        //카메라 모드
        public static CameraView cameraView = CameraView.BatterLow; //이걸 디폴트로

        //관중 모드
        public static bool crowdAnimMode = true;

        //관중 비율
        public static int crowdPer = 70;



        //
        public static ModeFlag PlayTypeFlag = ModeFlag.Manual;


        //
        public static int ConsecutiveNum;



        //상대가 스킵 요청
        public static bool SkipAsk = false;





        //임시
        public static bool bPitchingViewActive = false;
        public static bool bBattingSPMode = false;

    }
}
