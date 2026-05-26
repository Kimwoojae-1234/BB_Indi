
namespace BaseBall.BallPlay
{
    public enum BatterState
    {
        _WAITING,	//공기달리기
        _LOOKING,	//공보는 중(스윙가능함)
        _SWING,	//스윙 (No Key Event)
        _SWING_BACK,	//스윙 (No Key Event)
        _HITTING,
        _HUT_SWING,  //헛스윙	
        _BUNT,//
        _BUNT_BACK, //번트백
        _BUSTER,    //버스터
        //특수 동작	
        //_ALLOW_STRIKEOUT 			10	//삼진
        //_GET_FOURBALL 			12	//포볼 얻음
        _DEADBALL,	//데드볼 얻음
        _NEXT_BATTER,	//다음 타자
        _JUST_MISS,	//스트라잌 아닌거같음
        _NONE
    };

    public enum BattingTiming
    {
        NOSWING = 100,
        PERFECT = 3,
        VERY_EARLY = -1,
        TOO_EARLY = 0,
        EARLY = 1,
        JUST_EARLY = 2,
        JUST_LATE = 4,
        LATE = 5,
        TOO_LATE = 6,
        VERY_LATE = 7
    };

    public enum BattingContact
    {
        SOLID = 0,
        GOOD = 1,
        NORMAL = 2,
        BAD = 3,
        JAMMED = 4,
        TIP = 5,
        HUT_SWING = 6,
        NO_SWING = 7
    };

    public enum AutoModeBatting
    {
        Normal,
        StrikeOut,
        BaseOnBall
    };
}
