public enum KOBRarity
{
    COMMON = 1,
    RARE,
    EPIC,
    LEGENDARY,
    BLACK,
    //추가될 수 있음
}


public enum KOBStatType
{
    Power = 1,
    Contact,
    Vision,
    Fielding,
    Throwing,
    Speed,
}


public enum KOBSkillType
{
    Hitting = 1,
    Pysical = 2,
    Special = 3,
    Wide = 4,
    //추가될 수 있음    
}

public enum KOBSkillKey
{
    Power = 1,
    Contact = 2,
    Vision = 3,
    Fielding = 4,
    Throwing = 5,
    Speed = 6,
    RunningSkill = 7,
    HittingSpeed = 8,
    CheckSwing = 9,     //#체크스윙 별도
    AllHitting = 10,
    AllDefense = 11,
    FieldSkill = 12,
    SkillGear = 13,     //장비장착시 스킬
    InfieldDiving = 14, //#내야 다이빙 별도
    OutFieldDiving = 15,    //#외야 다이빙 별도
    TimingPlus = 16,    //#타이밍 플러스 별도
    //계속 추가될수 있음
}

public enum KOBSkillCondition
{
    NoCondition = 1,
    Cumulative = 2,     //매구마다 축적
    RightPitcher = 3,   //우투수를 만나면
    LeftPitcher = 4,
    AfterHit = 5,       //타격후
    MainActivated = 6,   //메인이 액티브 되면 -> 2,3,번스킬에서 주로 사용
    Walkoff = 7,        //끝내기 상황
    FirstPitch = 8,     //초구
    BreakballHit = 9,   //변화구 타격
    FastballHit = 10,   //직구 타격
    LinedriveHit = 11,  //라인드라이브 힛
    FlyHit = 12,        //플라이힛
    RunAndHit =13,      //런앤힛
    HomeMatch =	14,     //홈경시
    AwayMatch = 15,     //어웨이경기
    DayMatch = 16,      //낮경기
    NightMatch = 17,     //밤경기
    UnderPitcher = 18,  //언더핸드피처(사이드포함)
    TwoOut = 19,        //투아웃시
    TwoStrike = 20,     //투스트시
    Chance = 21,        //찬스시
    Losing1 = 22,     //지고 있는 경우
    Losing2 = 23,     //크게 지고 있는 경우
    InningFirst	= 24,   //이닝첫타자
    FullBase = 25,      //만루
    Winning1 = 26,      //이기고 있는중
    Winning2 = 27,      //크게 이기고 있는중
    NoRunner = 28,       //주자없음
    ResultOut = 29,     //타격결과 아웃시
    ResultHit = 30,     //타격결과 안타시
    FullCount = 31,     //풀카운트
    ResultBB = 32,     //타격결과 포볼
    ResultHR = 33,     //타격결과 홈런
    ResultCheck	= 34,   //볼을 선구함
    ResultStrike = 35,  //스트라이크
    DefenseCatcher = 36,    //포수수비시
    DefenseInfield = 37,    //내야수비시
    DefenseOutField = 38,   //외야수비시
    DefenseAll = 39,        //모든 수비시
    OnRunning =	40,         //주루중
    WideBatter = 41,
    WideDefense = 42,
    WideRunning = 43,
    Inning123 = 44,          //123회
    Inning456 = 45,         //456회
    Inning789 = 46,          //789회
    //추가될 수 있음
}


public enum KOBGearKey
{
    Power = 1,
    Contact = 2,
    Vision = 3,
    Fielding = 4,
    Throwing = 5,
    Speed = 6,
    RunningSkill = 7,
    HittingSpeed = 8,
    CheckSwing = 9,     //#체크스윙 별도
    AllHitting = 10,
    AllDefense = 11,
    FieldSkill = 12,
    SkillGear = 13,     //장비장착시 스킬
    InfieldDiving = 14, //#내야 다이빙 별도
    OutFieldDiving = 15,    //#외야 다이빙 별도
    TimingPlus = 16,    //#타이밍 플러스 별도
    //계속 추가될수 있음
}

public enum KOBConsumeItemType
{
    Bat = 1,
    //추가될 수 있음
    Etc = 100
}

public enum KOBItemKey
{
    Power = 1,
    Contact,
    Vision,
    Fielding,
    Throwing,
    Speed,
}


public enum CharacterType
{
    Ballers = 1,
    Pitcher = 2,         //수집 가능한 영웅피처   
    MyHMan = 3,          //KOBPlayerInfo -> StarList에 포함시킨다 (초반 내팀 7명)
    MyHPitcher = 4,      //기본 주어지는 HMan 투수 (초반내팀 4명)
    HMan = 5,       //타팀에 속해있는 HMan (타9개팀, 팀당 8명)
    HPitcher = 6,       //타팀에 속해있는 HMan (타9개팀, 팀당 4명)
}


public enum PlayingType
{
    None = 0,                   //HMan이나 투수는 None으로 설정해둘것
    WellRoundedHitter = 1,      //well-rounded hitter
    PowerSlugger = 2,           //Power Slugger
    ProductiveHitter = 3,       //Productive Hitter
    PrecisionHitter = 4,        //Precision Hitter
    BaseRunningSpecialist = 5,  //Baserunning Specialist
    DefensiveAce = 6,           //Defensive Ace
    Two_Way = 100               //만약 이도류 업데이트를 한다면
}


public enum KOBHand
{ 
    Left = 0,
    Right = 1,
    Switch = 2
}


public enum KOBPosition
{
    Pitcher = 1,
    Catcher = 2,
    First = 3,
    Second = 4,
    Third = 5,
    Short = 6,
    Left = 7,
    Center = 8,
    Right = 9,
    DH = 10,
    InfieldUtil = 11,
    InfieldCatcherUtil = 12,
    OutfieldUtil = 13,
    OutfieldCatcherUtil = 14,
    InOutField = 15,
    AllRounder = 16
}

public enum KOBBody
{
    Normal = 1,
    Fat = 2,
    Muscle = 3,
    Girl = 4,
    Special = 5
}


public enum KOBSlotType
{ 
    None = 0,
    HittingSkill = 1,
    PysicalSkill = 2,
    Gear = 11,
}

public enum KOBGearType
{
    Accessory = 1,  //파워 비전
    Helmet = 2,     //타격
    Spike = 3,      //수비 스피드
    Glove = 4,      //수비 어깨
}


