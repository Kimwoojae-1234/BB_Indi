using System.Linq;

public static class KOBConstant
{
    //플레이어 관련
    //플레이어 맥스 레벨
    public const int MAX_LEVEL = 13;
    //플레이오 최초 볼러
    public const int FIRSTBALLER = 1001;
    public const int FIRSTPITCHER = 1009;



    //Rtts관련
    //선수 기록관련 단위수 (합산된정보를 나눠서 계산)
    public const int PLAYER_RECORD_UNIT = 10000;
    //팀 기록 관련 단위 수
    public const int TEAM_RECORD_UNIT = 1000;

    //규정타석 상수
    public const int QPA_CONSTANT = 1000000;

    //업그레이드 차트 관련 상수
    public const int UpgradeGoldIndex = 10000;
    public const int UpgradeCardIndex = 10100;
    public const int UpgradeHittingSkillIndex = 20000;
    public const int UpgradePhysicalSkillIndex = 20100;
    public const int UpgradeWideSkillIndex = 20200;
    public const int UpgradeSpecialSkillIndex = 20300;
    public const int UnlockHittingSkillIndex = 21000;
    public const int UnlockPhysicalSkillIndex = 21100;
    public const int UnlockWideSkillIndex = 21200;
    public const int UnlockSpecialSkillIndex = 21300;
    public const int UnlockBallerIndex = 50000;


    //카드 pindex 설명
    public const int RANDOM_YOUHAVE = -1; //가지고 있는것중 랜덤 (같은희귀도)
    public const int RANDOM_NOTHAVE = -2; //가지고 있지 않은것 중 랜덤 (같은희귀도)
    public const int RANDOM_NOCONDITION = -3; //소유 여부 관계없이 랜덤 (같은희귀도)
    public const int BALLPER_YOUPLAY = -4;    //네가 현재 플레이 중인...


    //티어 관련
    public static int MAX_BALLER_FAME { get; private set; }
    public static int MAX_BALLER_FAME_TIER { get; private set; }
    public static int MAX_TROPHY_TIER { get; private set; }
    public static int MAX_TROPHY { get; private set; }

    static bool isReadOnlyInit = false;
    public static void InitConstant()
    {
        //이것은 차트 로딩이 다 끝난 후 진행 한다.
        if(isReadOnlyInit == false)
        {
            TrophyRoad maxTierRoad = KOBManager.Backend.Chart.TrophyRoadData.Dictionary.Values.OrderByDescending(x => x.tier).FirstOrDefault();
            MAX_TROPHY_TIER = maxTierRoad.tier;
            MAX_TROPHY = maxTierRoad.trophy;

            BallerTrophyRoad maxBaller = KOBManager.Backend.Chart.BallerTrophyRoadData.Dictionary.Values.OrderByDescending(x => x.tier).FirstOrDefault();
            MAX_BALLER_FAME_TIER = maxBaller.tier;
            MAX_BALLER_FAME = maxBaller.trophy;

            UnityEngine.Debug.Log("최대 트로피 티어 : " + MAX_TROPHY_TIER);
            UnityEngine.Debug.Log("최대 트로피 : " + MAX_TROPHY);
            UnityEngine.Debug.Log("최대 볼러 명성 티어 : " + MAX_BALLER_FAME_TIER);
            UnityEngine.Debug.Log("최대 볼러 명성 : " + MAX_BALLER_FAME);

            isReadOnlyInit = true;
        }

    }
}
