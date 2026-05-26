namespace BaseBall.BallPlay
{    
    public enum PitcherPosotion
    {
        //투수 보직
        STARTER = 0,   //선발
        CHASE = 1,     //추격
        RELIEF = 2,    //중간
        SETUP = 3,     //셋업
        SAVE = 4      //세이브
    }

    public enum PitcherState
    {
        //투구동작
        _GET_SIGN,          // 사인 보기
        _PITCHING,	//와인드업 던지기	
        _RELEASE,	//릴리즈
        _FINISH,	//피칭동작 끝
        _CHECKCOUNT,	//피칭동작 끝
        _TIPCOUNT,      //파울팁
        _CHECKSWING_ASK, //체크스윙 확인
        //견제동작
        _PICK_FIRST,	//1루 견제
        _PICK_SECOND,	//2루 견제
        _PICK_THIRD,	//3루 견제
        //기타
        _GET_STRIKEOUT,	//삼진 잡음
        _ALLOW_FOURBALL,	//포볼 허용
        _WILD_PITCH,        //와일드 피치,
        _PAUSE,
        _NONE
    };


    public enum PitchType
    {
        FASTBALL = 0,
        CURVE = 1,
        CHANGEUP = 2,
        SLIDER = 3,
        FORK = 4
    };

    public enum BallMoveType
    {
        Straight,
        Curve,
        OffSpeed,
        Slide,
        FastBreaking
    }

    //구종여기고쳐 - 인덱스와 이름
    public enum PitchingArsenal
    {
        //직구류
        FASTBALL = 1,       //포심        
        TWOSEAM = 2,        //투심
        RISING = 3,         //라이징

        //커브류
        CURVE = 4,          //커브
        POWER_CURVE = 5,    //파워커브
        SLOW_CURVE = 6,     //슬로커브        
        POKPOSU_CURVE = 7,  //폭포수커브 (이전 12-6커브)
        KNUCKLE_CURVE = 8,  //너클커브

        //첸졉류
        CHANGEUP = 9,       //체인지업
        CIRCLE = 10,        //서클체인지업
        VULCAN = 11,        //벌칸체인지업
        PALM = 12,          //팜볼
        KNUCKLE = 13,       //너클볼

        //슬라이더류
        SLIDER = 14,        //슬라이더
        H_SLIDER = 15,      //고속 슬라이더
        SLURVE = 16,        //슬러브
        CUT_FAST = 17,      //컷패스트
        FRISBEE = 18,       //사이드 언더 전용
        
        //포크류
        FORK = 19,          //포크
        SINKER = 20,        //싱커
        SFF = 21,           //스플리터
        SINKING_FAST = 22,  //하드싱커

        //기타
        EEPHUS = 23,
        GIRO_CURVE = 24,
        H_FORK = 106,
        UPSHOOT = 107,
        NONE = 0
    };


}