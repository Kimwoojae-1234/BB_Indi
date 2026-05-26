
namespace BaseBall.BallPlay
{

    //플라이 처리 타입
    public enum FlyCatchType
    {
        CATCH_ERROR = 1,            //포구 에러
        BOUND_ERROR = 2,            //바운드 처리 에러
        THROW_ERROR = 3,            //송구 에러    
        DashRun = 4,
        OverHeadRun = 5,
        LeftRun = 6,
        RIghtRun = 7,
        DashBound = 8,
        SideBound = 9,
        NormalBound = 10,
        FenceBound = 11,
        HomeRun = 12,
        HomeRunSteal = 13,  //스킬 사용
        DivingCatch = 14,   //스킬 사용
        JumpingCatch = 15,  //스킬 사용
        Normal = 16,
    }
}
