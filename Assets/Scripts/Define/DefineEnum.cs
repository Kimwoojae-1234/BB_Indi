using UnityEngine;
using System.Collections;

/// <summary>
/// 전체적으로 쓰이는 Enum 정리
/// 기존 프로젝트에서 갖고온 값이므로 추후 수정 가능
/// </summary>
public class DefineEnum
{
    /// 게임모드
    public enum EGameMode
    {
        None,
        Season,
        Rank,
        LeagueRace,
        Walkoff,
        Tranning,
        SeasonInstant,
        LiveMatch,
        SeasonConsecutive,
        LeagueRaceInstant,
    }

    /// 아이콘 타입
    public enum EIconType
    {
        Small,
        Medium,
        Large,
    }

    public enum ECardGrade
    {
        None,
        Normal,
        Rare,
        Hero,
        Legend,
    }


    // 투수는 1 ~ 11, 타자는 101 ~ 114
    public enum ELineUpAll
    {
        //투수일때는 보직순 (1~5선발, 6~7필승조, 8~9패전조, 셋업, 마무리)
        Minor = 0,  // 2군
        SP_1,   // 선발 투수 1
        SP_2,   // 선발 투수 2
        SP_3,   // 선발 투수 3
        SP_4,   // 선발 투수 4
        SP_5,   // 선발 투수 5
        RP_1,   // 불펜 1 // 필승조 1
        RP_2,   // 불펜 2 // 필승조 2
        RP_3,   // 불펜 3 // 패전조 1
        RP_4,   // 불펜 4 // 패전조 2
        SU, // 셋업
        CP, // 클러져

        /// 타자일때는 타순 (101~109, 110~114 벤치)
		/// 2군일때는 0.
        // 여기 갈아엎어야 한다. 
        Hitter_One = 101,
        Hitter_Two,
        Hitter_Three,
        Hitter_Four,
        Hitter_Five,
        Hitter_Six,
        Hitter_Seven,
        Hitter_Eight,
        Hitter_Nine,

        // 벤치도 순서가 있다. 
        Bench_1,    // 벤치 1
        Bench_2,    // 벤치 2
        Bench_3,    // 벤치 3
        Bench_4,    // 벤치 4
        Bench_5,    // 벤치 5
    }

    // 포지션
    public enum EPosition
    {
        SP =1,
        RP,
        CP,
        C,
        B1,
        B2,
        B3,
        SS,
        LF,
        CF,
        RF,
    }

    public enum EProperPosition
    {
        None,
        SP,
        RP,
        SU,
        CP,
        C,
        BASE_1,
        BASE_2,
        BASE_3,
        SS,
        LF,
        CF,
        RF,
        DH,
    }

    public enum ECardSize
    {
        Small,
        Large,
    }

    public enum ETeamLineUpFilter
    {
        None,
        Major_Pitcher,  // 1군 투수
        Major_Hitter,   // 1군 타자

        Minor_Pitcher_All,   // 2군 전체 투수
        Minor_Pitcher_SP,   // 2군 전체 선발 투수
        Minor_Pitcher_RP,   // 2군 전체 불펜 투수
        Minor_Putcher_CP,   // 2군 전체 마무리 투수

        Minor_Hitter_All,   // 2군 전체 타자
        Minor_Hitter_C,     // 2군 전체 포수 타자
        Minor_Hitter_IF,    // 2군 전체 내야수
        Minor_Hitter_OF,    // 2군 전체 외야수

        Major,  // 1군 라인업
        Pitcher,    // 전체 투수
        Hitter, // 전체 타자
        Minor,

    }

    // 재화
    public enum ECurrency
    {       
        Ruby = 0,
        Gold,
        FriendPoint,
        // 코인상점 재화
        LvpCoin,
        //도감 마일리지
        DogamPoint,
        [System.Obsolete("삭제됨")]
        Mileage,
    }

    // 충전되는 배터리류
    public enum ERecharge
    {
        Heart = 1,
        Ticket,
    }

    public enum ECardAbility
    {
        Trajectory = 1, // 타구각
        Contact,    // 컨택
        BattingEye, // 선구
        Throwing,   // 송구
        Fileding,   // 수비
        Running,    // 주력
        Power,  // 파워
        Stamina = 10,   // 체력


        FourSeam_FourSeam = 11,     // 포심
        FourSeam_TwoSeam,           // 투심
        FourSeam_RisingFastBall,    // 라이징 패스트볼

        Curve_Curve,                // 커브
        Curve_PowerCurve,           // 파워 커브
        Curve_SlowCurve,            // 슬로우 커브
        Curve_DropCurve,            // 폭포수 커브
        Curve_KnuckleCurve,         // 너클 커브

        ChangeUp_ChangeUp,          // 체인지업
        ChangeUp_CircleChangeUp,    // 서클 체인지업
        ChangeUp_VulcanChangeUp,    // 벌컨 체인지업
        ChangeUp_PalmBall,          // 팜볼
        ChangeUp_KnuckleBall,       // 너클볼

        Slider_Slider,              // 슬라이더
        Slider_FastSlider,          // 고속 슬라이더
        Slider_Sluve,               // 슬러브
        Slider_Cutter,              // 컷패스트볼
        Slider_FrisBeeSlider,       // 프리스비 슬라이더

        Fork_Fork,                  // 포크볼
        Fork_Sinker,                // 싱커
        Fork_Splitter,              // 스플리터
        Fork_HardSinker,            // 하드 싱커
    }

    public enum ESeasonLeague
    {
        None,
        Beginner,
        Rookie,
        Pro,
        Minor,
        Major,
        AllStar,
        Ace,
        Legend,
        Champion,
    }

    public enum ESeasonType
    {
        NewSeason,
        PennantRace,
        WildCard,
        SemiPlayOff,
        PlayOff,
        KoreaSeries,
    }

    public enum ERecordSort
    {
        Sort_Win_Descend,
        Sort_Win_Ascend,
        Sort_Draw_Descend,
        Sort_Draw_Ascend,
        Sort_Lose_Descend,
        Sort_Lose_Ascend,
        Sort_WinRatio_Descend,
        Sort_WinRatio_Ascend,
        Sort_HomeRun_Descend,
        Sort_HomeRun_Ascend,
        Sort_StealBase_Descend,
        Sort_StealBase_Ascend,
        Sort_BatAverage_Descend,
        Sort_BatAverage_Ascend,
        Sort_OPS_Descend,
        Sort_OPS_Ascend,
        Sort_ERA_Descend,
        Sort_ERA_Ascend,


        Sort_GameRound_Descend,
        Sort_GameRound_Ascend,
        Sort_RBI_Descend,   // 타점
        Sort_RBI_Ascend,   // 타점

        Sort_Hit_Descend,   // 안타
        Sort_Hit_Ascend,   // 안타

        Sort_Ball_Descend,  // 볼넷
        Sort_Ball_Ascend,  // 볼넷

        Sort_Run_Descend,   // 득점
        Sort_Run_Ascend,   // 득점

        Sort_SVG_Descend,   // 장타율
        Sort_SVG_Ascend,   // 장타율
        Sort_OBP_Descend,   // 출루율
        Sort_OBP_Ascend,   // 출루율
        Sort_RISP_Descend,  // 득점권타율
        Sort_RISP_Ascend,  // 득점권타율

        Sort_Inning_Descend,    // 이닝
        Sort_Inning_Ascend,    // 이닝
        Sort_Save_Descend,  // 게임 세이브
        Sort_Save_Ascend,  // 게임 세이브
        Sort_Hold_Descend,  // 게임 홀드
        Sort_Hold_Ascend,  // 게임 홀드
        Sort_WHIP_Descend,
        Sort_WHIP_Ascend,
        Sort_StrikeOut_Descend,
        Sort_StrikeOut_Ascend,
        Sort_BBHPB_Descend, // 사사구(볼넷 + 사구)
        Sort_BBHPB_Ascend, // 사사구(볼넷 + 사구)
        Sort_ER_Descend,    // 자책점
        Sort_ER_Ascend,    // 자책점

        Sort_None,
    }

    public enum EPostScheduleState
    {
        NotYet,
        Curr,
        Finish,
    }

    public enum EPostType
    {
        Reward = 1, // 선물
        Purchase = 2,   // 상점
        FriendPoint = 3 // 우정 포인트
    }

    public enum EPostAttachType
    {
        Ruby = 1,
        Gold = 2,
        Heart = 3,
        FriendPoint = 4,
        Item = 5
    }

    public enum EItemType
    {
        Ruby = 10,
        Gold = 11,
        Heart = 12,
        Mileage = 13,
        PlayerCard = 14,
        PlayerPack_Normal = 15,
        PlayerPack_Team = 16,
        PlayerPack_TeamYear = 17,
        Package_Normal = 18,
        Package_Random = 19,
        Package_Time = 20,
        MatchSkip = 21,
        Simulation = 22,
        Lotto = 23,
        Coupon = 24,
        Etc = 99,
    }

    public enum EItemCategory
    {
        Currency,
        PlayerCard,
        Item,
    }

    public enum ERankGroup
    {
        None,
        Bronze3,
        Bronze2,
        Bronze1,
        Silver3,
        Silver2,
        Silver1,
        Gold3,
        Gold2,
        Gold1,
        Diamond2,
        Diamond1,
    }

    public enum EResult
    {
        Win,
        Draw,
        Lose,
    }

    public enum EShopTab
    {        
        Package,
        PlayerCard,
        Ruby,
        Gold,
        Heart,
        Mileage,
        FriendPoint,
        Etc,
        None,
    }

    public enum EItemSorting
    {
        Currency,
        Recharge,
        Item,
        PlayerCard,
        Coupon,
        Package,
    }
    public enum ETeamLineUpSort
    {
        Total_Dsc,    // 종합 능력 내림차순
        Total_Asc,  // 종합 능력 오름차순
        Enhance_Dsc,  // 강화 단계 내림차순
        Enhance_Asc,    // 강화 단계 오름차순
        GainTime,
        Name,
        Year,
        Team,
        Max,
    }

    public enum ETradeSort
    {
        Rank_Dsc,      // 등급 내림차순
        Rank_Asc,      // 등급 오름차순

        Enhance_Dsc,  // 강화 단계 내림차순
        Enhance_Asc,    // 강화 단계 오름차순

        Total_Dsc,    // 종합 능력 내림차순
        Total_Asc,  // 종합 능력 오름차순

        Level_Dsc,  // 레벨 내림차순
        Level_Asc,  // 레벨 오름차순

        Team,   // 구단명
        Name,   // 이름
        Max,    // 맥슥밧
        

    }

    public enum ETradeOption
    {
        None,
        Alittle,
        Normal,
    }

    public enum EButton
    {
        On,
        Off,
    }

    public enum EBannerDirection
    {
        Left,
        Right,
    }

    public enum EMissionType
    {
        Daily,
        Main,
    }

    public enum EDirectLink
    {
        //         0	없음	
        // 1	시즌모드로 이동	
        // 2	랭킹모드로 이동	
        // 3	선수획득으로 상점 카드탭 이동
        // 4	라인업으로 이동
        None = 0,
        GoSeasonMode = 1,
        GoRankMode = 2,
        GoShopPlayerCard = 3,
        GoTeamLineUp = 4,
    }

    public enum EAccountType
    {
        None = 0,
        Guest = 1,
        Google = 2,
        FaceBook = 3,
    }

    /// <summary>
    /// 플레이어 체형
    /// </summary>
    public enum EPlayerBody
    {
        /// <summary>
        /// 노말
        /// </summary>
        NORMAL = 1,
        /// <summary>
        /// 근육
        /// </summary>
        MUSCLE = 2,
        /// <summary>
        /// 뚱뚱
        /// </summary>
        FAT = 3,
    }

    /// <summary>
    /// 플레이어 피부색
    /// </summary>
    public enum EPlayerColor
    {
        /// <summary>
        /// 황인
        /// </summary>
        YELLOW = 1,
        /// <summary>
        /// 백인
        /// </summary>
        WHITE = 2,
        /// <summary>
        /// 흑인
        /// </summary>
        BLACK = 3,
    }

    public enum EMainHander
    {
        /// <summary>
        /// 우투우타
        /// </summary>
        RORH,
        /// <summary>
        /// 우투좌타
        /// </summary>
        ROLH,
        /// <summary>
        /// 좌투우타
        /// </summary>
        LORH,
        /// <summary>
        /// 좌투좌타
        /// </summary>
        LOLH,
        /// <summary>
        /// 우투양타
        /// </summary>
        ROSH,
        /// <summary>
        /// 좌투양타
        /// </summary>
        LOSH,
        /// <summary>
        /// 우언우타
        /// </summary>
        RURH,
        /// <summary>
        /// 우언좌타
        /// </summary>
        RULH,
        /// <summary>
        /// 우언양타
        /// </summary>
        RUSH,
        /// <summary>
        /// 우사우타
        /// </summary>
        RSRH,
        /// <summary>
        /// 우사좌타
        /// </summary>
        RSLH,
        /// <summary>
        /// 우사양타
        /// </summary>
        RSSH,
        /// <summary>
        /// 좌언우타
        /// </summary>
        LURH,
        /// <summary>
        /// 좌언좌타
        /// </summary>
        LULH,
        /// <summary>
        /// 좌언양타
        /// </summary>
        LUSH,
        /// <summary>
        /// 좌사우타
        /// </summary>
        LSRH,
        /// <summary>
        /// 좌사좌타
        /// </summary>
        LSLH,
        /// <summary>
        /// 좌사양타
        /// </summary>
        LSSH,
    }

    public enum ESideMenuMode
    {
        LineUp,
        Enhance,
        Trade,
    }

    public enum EDecimalPoint
    {
        // 각각 모두 빈자리에 0을 붙임
        None,   // 소수점을 쓰지 않음
        One,    // 소수점 1자리
        Two,    // 소수점 2자리
        Three,  // 소수점 3자리
    }

    public enum EItemSize
    {
        Large,
        Small,
    }

    public enum ESkillGrade
    {
        //스킬등급    id
        C = 1,
        B = 2,
        A = 3,
        S = 4,
    }

    public enum EHaveItem
    {
        SkipTicket = 1, // 스킵 티켓
        SimulTicket = 2,    // 시뮬 티켓
    }

    public enum ESkillPosition
    {
//         내야수	1
//         외야수	2
//         포수	3
//         주자	4
//         타자	5
//         야수	6
//         투수공통	7
//         선발	8
//         중계	9
//         마무리	10

    }

    public enum EFullPosition
    {
        Minor,
        StartingPitcher_1,
        StartingPitcher_2,
        StartingPitcher_3,
        StartingPitcher_4,
        StartingPitcher_5,
        ReliefPitcher_1,
        ReliefPitcher_2,
        ReliefPitcher_3,
        ReliefPitcher_4,
        SetupPitcher,
        ClosingPitcher,

        Catcher,
        BaseMan_1, 
        BaseMan_2,
        BaseMan_3,
        ShortStop,
        LeftFielder,
        CenterFielder,
        RightFielder,
        DesignatedHitter,
        Bench_1,
        Bench_2,
        Bench_3,
        Bench_4,
        Bench_5,
    }

    public enum CardPackType
    {
        STAR3=1,
        STAR4,
        STAR5
    }

    public enum RandomCardPackType
    {
        BronzeCardPack=1,
        SilverCardPack,
        GoldCardPack,
    }

    public enum TrophyType
    {
        OneTrophy = 1,
        TweenTrophy,
        BronzeTrophy,
        SilverTrophy,
        GoldTrophy,
    }

    public enum BoostCardType
    {
        Star2Tranning = 1,
        Star3Tranning,
        Star4Tranning,
        Star5Tranning,
    }

    public enum NoneItemType
    {
        SkillRankChangeTicket = 1,
        DirectFinishTicket,
    }

    public enum ClientSpriteName
    {
        CARD_BG = 1,
        CARD_BG_L,
        CARD_STAR_GOLD,
        CARD_STAR_SILVER,
        CARD_LEVEL_BG_N,
        CARD_LEVEL_BG_L,
        CARD_NAME_BG_N,
        CARD_NAME_BG_L,
        CARD_REINFORCE,
        CARD_REINFORCE_BG,
        ITEM_ICON,
        ITEM_BG,
        CARD_BG_MINI,
        CARD_STAR_GOLD_MINI,
        CARD_STAR_SILVER_MINI,
        TRAINING_CENTER_NAME_ON,
        TRAINING_CENTER_NAME_OFF,
        ITEM_ICON_MINI,
        RANK_MARK,
    }

    public enum ItemCategory
    {
        PACK,
        GEAR,
        TROPHY,
        Etc,
        MAX
    }
}
