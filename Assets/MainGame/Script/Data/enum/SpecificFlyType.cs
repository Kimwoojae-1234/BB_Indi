
namespace BaseBall.BallPlay
{
    //구체적인 플라이 타입
    public enum SpecificFlyType
    {
        InfieldPopup_Fair = 0,      //거의 아웃, 에러가능성도 적음
        InfieldPopup_Foul = 1,      //거의 아웃, 에러가능성도 적음
        CatcherPopup = 2,           //거의 아웃, 에러가능성도 적음
        OutfieldPopup = 3,          //거의 아웃, 에러가능성도 적음  , 베이스택 가능성 있음
        OutfieldShort = 4,          //단타 가능성 높음 , 에러가능성 1단계 up
        OutfieldHighFly = 5,        //단타 장타 가능성 있음 , 에러가능성 적음, 베이스택 가능성 높음
        OutfieldOverHead = 6,       //장타 가능성 높음 , 에러가능성 1단계 up, 베이스택 가능성 높음
        OutfieldHomerun = 7         //홈런
    }
}
