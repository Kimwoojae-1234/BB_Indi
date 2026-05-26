
namespace BaseBall.BallPlay
{
    public enum CallType
    {
        _SAFE,
        _OUT,
        _FLYOUT,
        _FOUL,
        _HOMERUN,
        _LINECALL,
        _STRONGOUT
    }


    public enum HITBALLTYPE
    {
        _NONE,
        _GROUNDER,
        _LINEDRIVE,
        _FLYBALL,
        _POPUP,
        _PPICSAL,   //삑사리
        _HOMERUN,
        _STRONG_FLY,
        _BROKEN,
        _BUNT
    }



    public enum FielderAction
    {
        _STANDBY,	//수비대기
        _MOVE,	    //움직임
        _FIELDING,  //필딩
        _MOTION,    //연결모션
        _CATCHING,  //잡기
        _THROW_READY,   //송구 대기
        _THROWING,  //던지기
        _THROWING_CATCH,
        _CATCHER_LONG_TAG,  //도루 송구
        _PICKOFF,           //견제
        _COLLISION,     //충돌
        _ERROR_PANIC,    //에러 패닉
        _NOTHING_STATE,
        _SPECIAL_CATCH_THROW
    }

    public enum ActionStep
    {
        //move
        _MOVE_READY,
        _MOVING,
        _AFTER_FORCEOUT,

        //fielding
        _FIELDING_READY,
        _FIELDING_MOVE,
        _GROUNDER_SPECIAL,
        _FLYBALL_SPECIAL,
        _CHASE,

        //catching
        _CATCHING_READY,
        _CATCHING,
        _CATCHING_DASH,

        //motion
        _MOTION_SET,
        _THROW_READY,
        _THROW_DELAY,
        _THROW_LASER,
        _DOUBLEPLAY,


        //throwing
        _THROW_NORMAL,
        _THROW_SPECIAL,

        //throwng_catchng
        _FORCE_OUT,
        _TAG_OUT,
        _CATCH_NORMAL,
        _TAGGING,

        //collision
        _CATCHER_BLOCK,
        _CATCHER_CRUSHED,
        _DOUBLEPLAY_STOP,
        _FIELDER_COLLISION,

        _NONE
    };


    public enum NextAction
    {
        _NONE,
        _ROUNDING,
        _TO_PITCHER,
        _JUST_WALK,
        _BASE_COVER,
        _RELAY_POSITION,

        _NEXT_SECOND_RELAY,
        _NEXT_FIRSTBASE_BACKUP,
        _NEXT_SECONDBASE_COVER
    }

    public enum ThrowType
    {
        _NORMAL,
        _NORMAL_STRONG,
        _INFIELD_OVER_ONESTEP,
        _INFIELD_OVER_TWOSTEP,
        _INFIELD_OVER_JUMPING,
        _INFIELD_SIDE_SPIN,
        _INFIELD_SIDE_QUICK,
        _INFIELD_SIDE_DASH,
        _INFIELD_DOUBLE_PLAY1,
        _OUTFIELD_OVER_ONESTEP,
        _OUTFIELD_LASER,

        //병살 특수
        _INFIELD_DODGE  //피하면서 송구

    }

    public enum ThrowState
    {
        DOUBLE_PLAY,
        FORE_HAND_CATCH,
        BACK_HAND_CATCH,
        FORE_DIVING_CATCH,
        BACK_DIVING_CATCH,
        CENTER_MOVING,
    }

    public enum FlyCatch
    {
        FLYCATCH_NORMAL = 0,
        FLYCATCH_SLOWMOVE = 1,
        FLYCATCH_FULLSPEED = 2,
        FLYCATCH_DIVING = 3,
        FLYCATCH_DASH_FOR_ASSIST = 4,
        FLYCATCH_JUMPING = 5,
        FLYCATCH_HOMERUNSTEAL = 6,

        FLYCATCH_BACKWARD_JUMPING = 7,
    }

    public enum GrounderCatch
    {
        GROUNDERCATCH_NORMAL = 0,
        GROUNDERCATCH_MOVING_NORMAL = 4,
        GROUNDERCATCH_MOVING = 1,
        //GROUNDERCATCH_FULLSPEED = 2,
        GROUNDERCATCH_DIVING = 3,
        GROUNDERCATCH_CATCH_AND_THROW = 6,
        GROUNDERCATCH_DASH_FIRST = 7,   //처음부터 대쉬
        GROUNDERCATCH_JUMP = 10,
        GROUNDERCATCH_TRY = 20,
    }




    public enum FieldState
    {
        NORMAL_FIELD,
        ACTIVE_CENTER,
        ACTIVE_LEFT,
        ACTIVE_RIGHT
    };


    public enum FieldSkillUse
    {
        Init,
        Active,
        Success,
        Fail
    }
}

