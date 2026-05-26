
namespace BaseBall.BallPlay
{
    
    public enum PicthControlType
    {
        IndicatorType,
        ClassicType,
        PulseType
    };

    public enum BatControlType
    {
        PushType,          //푸시 타입
        ReleaseType         //릴리즈 타입
    }


    public enum PlayState
    {
        //PLAY_START_GAME,
        PLAY_INIT_INNING, //애매.. 나중에 고려
        PLAY_BATTING_VIEW_READY,
        PLAY_BATTING_VIEW,
        PLAY_BATTING_VIEW_PRE,
        PLAY_BATTING_VIEW_INFO,
        //PLAY_BATTING_VIEW_SP,   //특능화면
        PLAY_FIELDING_VIEW,
        PLAY_CHANGE_INNING,
        PLAY_START_INNING,
        //PLAY_CONTINUE,
        //PLAY_SHOW_SCOREBOARD,
        PLAY_CHANGE_PLAYER,
        PLAY_CHANGE_OPTION,
        PLAY_GAME_RESULT,
        PLAY_FAST_INNING_SIMUL,
        PLAY_AI_PITCHER_CHANGE,
        PLAY_AI_BATTER_CHANGE,
        PLAY_AI_RUNNER_CHANGE,
        PLAY_AI_FIELDER_CHANGE,
        NONE
    };

    public enum SkillFlag
    {
        None = 0,

        //회심의 일격
        TenderStroke = 100071,            //회심의 일격

        //매혹
        Charm = 100081,                   //매혹

        //도발꾼
        CatcherMeatJil = 200051,          //도발꾼
        CatcherProvoke = 200052,          //도발

        //매의눈
        FalconEye = 200091,               //매의눈

        //강습타구
        AssaultBall = 200111,             //강습타구

        //번트의신
        GodOfBunt = 200131,               //번트의 신
        
        //뜬금포
        Unexpected = 200141,              //뜬금포
    }

}
