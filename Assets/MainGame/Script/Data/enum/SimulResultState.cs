
namespace BaseBall.BallPlay
{
    //한타자의 최종 시뮬레이션 결과
    public enum SimulResultState
    {
        NONE  = 0,
        StrikeOut = 1,
        FourBall = 2,
        Grounder = 3,
        FlyOut = 4,
        LineOut = 5,
        Single = 6,
        Double = 7,
        Triple = 8,
        HomeRun = 9,
        InfieldSingle = 10,
        InfieldTurboSingle = 11, //터보 능력치를 사용함
        BuntSingle = 12,
        CatchError = 13,         //플라이볼 포구 에러 특이점: 루상 주자가 1베이스 더 추가
        ThrowError = 14,         //그라운더 송구 에러
        BoundError = 15,         //바운드볼 포구 에러 : single가 거의 유사        
        SingleOneError = 16,
        DoubleOneError = 17,
        TripleOneError = 18,
        //BuntSuccess,        //번트성공(타수증가안함)
        //BuntFail,           //번트실패
        //BuntDoublePlay,     //번트실패에 더블플레이까지
        FielderChoice = 19       //야수선택(번트시 성공과 더블어 타수 증가 안함)

    }
}
