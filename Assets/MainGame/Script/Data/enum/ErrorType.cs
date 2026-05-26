
namespace BaseBall.BallPlay
{

    //에러 타입
    public enum ErrorType
    {
        NONE = 0,
        CATCH = 1,      //포구 에러
        BOUND = 2,      //바운드 처리 에러
        THROW = 3,      //송구 에러
        THROW_CATCH = 4 //송구된 공 포구 에러
    }
}
