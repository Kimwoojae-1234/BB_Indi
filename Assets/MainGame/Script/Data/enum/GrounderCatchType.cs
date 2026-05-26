
namespace BaseBall.BallPlay
{


    //땅볼 처리 타입
    public enum GrounderCatchType
    {
        CATCH_ERROR = 1,            //포구 에러
        BOUND_ERROR = 2,            //바운드 처리 에러
        THROW_ERROR = 3,            //송구 에러    
        Normal = 4,
        Dash = 5,
        ForeHand = 6,
        BackHand = 7,
        Dash_Deep = 8,
        ForeHand_Deep = 9,
        BackHand_Deep = 10,
        PitcherAct = 11,        //투수 반응
        PitcherJump = 12,       //투수 점프
        SlidingCatch = 13,      //내야수 다이빙 캐치
        Bunt = 18,
        NoCatch = 20,            //내야수가 잡지 못함
        LineNoCatch = 21         //라인 선상으로 빠짐
    }
}
