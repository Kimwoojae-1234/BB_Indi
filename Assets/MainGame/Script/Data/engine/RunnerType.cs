
namespace BaseBall.BallPlay
{
    public enum BaseArriveMotion
    {
        _NORMAL,
        _FIRSTBASE_RUN_ARRIVE,
        _FIRSTBASE_SLIDING_ARRIVE,
        _SECONDBASE_RUN_ARRIVE,
        _SECONDBASE_SLIDING_ARRIVE,
        _THIRDBASE_RUN_ARRIVE,
        _THIRDBASE_SLIDING_ARRIVE,
        _HOMEBASE_RUN_ARRIVE,
        _HOMEBASE_SLIDING_ARRIVE,
        _SENCOND_THIRD_ARRIVE
    }

    public enum RunnerOutMotion
    {
        _NORMAL,
        _FIRSTBASE_RUN_OUT,
        _FIRSTBASE_SLIDING_OUT,
        _SECONDBASE_RUN_OUT,
        _SECONDBASE_SLIDING_OUT,
        _SECONDBASE_SKILL_OUT,
        _THIRDBASE_RUN_OUT,
        _THIRDBASE_SLIDING_OUT,
        _HOMEBASE_RUN_OUT,
        _HOMEBASE_SLIDING_OUT,
        _HOME_CEREMONY1,
        _HOME_CEREMONY2
    }

    //슬라이딩 관련
    public enum SlidingType
    {
        _NO_SLIDING,
        _NORMAL,
        _HEAD_FIRST,
        _DOUBLEPLAY_SLIDING
    }

    public enum HomeShobu
    {
        _SLIDING,
        _RUSH,
        _NONE
    }


}