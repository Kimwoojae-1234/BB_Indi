
namespace BaseBall.BallPlay
{
    
    
    public class CFielderSkill
    {
        public bool bAvailable;
        //스킬의 기본
        public int activePer;
        public int successPer;
        public int vsWinPer;
        //스킬의 주력관련 기타 밸류
        public int[] stealValue = new int[2]; //0:일반 1:홈스틸
        public int bonusValue;


    }
}