using System;

namespace WebConnector {
    public enum TeamCode {
        SAMSUNG = 1,
        NEXEN = 2,
        NC = 3,
        LG = 4,
        SK = 5,
        DOOSAN = 6,
        LOTTE = 7,
        KIA = 8,
        HANWHA = 9,
        KT = 10
    }

    [Obsolete("삭제됨")]
    public enum Rank {
        Normal = 1,
        Rare = 2,
        Hero = 3,
        Legend = 4
    }

    public enum PlayerType {
        /// <summary>
        /// 투수
        /// </summary>
        Pitcher = 1,
        /// <summary>
        /// 타자
        /// </summary>
        Hitter = 2
    }
    /// <summary>
    /// 선수카드가 가지는 고유 포지션값
    /// </summary>
    public enum Position {
        /// <summary>
        /// 선발투수
        /// </summary>
        SP = 1,
        /// <summary>
        /// 중계
        /// </summary>
        RP = 2,
        /// <summary>
        /// 마무리
        /// </summary>
        CP = 3,
        /// <summary>
        /// 포수
        /// </summary>
        C = 4,
        /// <summary>
        /// 1루수
        /// </summary>
        B1 = 5,
        /// <summary>
        /// 2루수
        /// </summary>
        B2 = 6,
        /// <summary>
        /// 3루수
        /// </summary>
        B3 = 7,
        /// <summary>
        /// 유격수
        /// </summary>
        SS = 8,
        /// <summary>
        /// 좌익수
        /// </summary>
        LF = 9,
        /// <summary>
        /// 중견수
        /// </summary>
        CF = 10,
        /// <summary>
        /// 우익수
        /// </summary>
        RF = 11
    }

    /// <summary>
    /// 선수 카드 라인업 종류
    /// </summary>
    public enum Lineup {
        /// <summary>
        /// 2군
        /// </summary>
        [Obsolete("삭제됨")]
        Minor = -1,
        [Obsolete("삭제됨")]
        SU = 0,
        /// <summary>
        /// 선발투수
        /// </summary>
        SP = 1,
        /// <summary>
        /// 중계
        /// </summary>
        RP = 2,
        /// <summary>
        /// 마무리투수
        /// </summary>
        CP = 3,
        /// <summary>
        /// 포수
        /// </summary>
        C = 4,
        /// <summary>
        /// 1루수
        /// </summary>
        B1 = 5,
        /// <summary>
        /// 2루수
        /// </summary>
        B2 = 6,
        /// <summary>
        /// 3루수
        /// </summary>
        B3 = 7,
        /// <summary>
        /// 유격수
        /// </summary>
        SS = 8,
        /// <summary>
        /// 좌익수
        /// </summary>
        LF = 9,
        /// <summary>
        /// 중견수
        /// </summary>
        CF = 10,
        /// <summary>
        /// 우익수
        /// </summary>
        RF = 11,
        /// <summary>
        /// 지명타자
        /// </summary>
        DH = 12,
        /// <summary>
        /// 벤치
        /// </summary>
        BC = 13
    }

    [Obsolete("삭제됨")]
    public enum SeasonAnnounceType {
        /// <summary>
        /// 포스트시즌 없음
        /// </summary>
        NoPostSeason = 1,
        /// <summary>
        /// 포스트시즌 탈락
        /// </summary>
        MissPostSeason = 2,
        /// <summary>
        /// 포스트시즌 진출(와일드카드 결정전)
        /// </summary>
        GoWildCard = 3,
        /// <summary>
        /// 포스트시즌 진출(준플레이오프)
        /// </summary>
        GoSemiPlayOff = 4,
        /// <summary>
        /// 포스트시즌 진출(플레이오프)
        /// </summary>
        GoPlayOff = 5,
        /// <summary>
        /// 포스트시즌 진출(한국시리즈)
        /// </summary>
        GoKoriaSeries = 6,
        /// <summary>
        /// 다음라운드진출(준플레이오프)
        /// </summary>
        NextSemiPlayOff = 7,
        /// <summary>
        /// 다음라운드진출(플레이오프)
        /// </summary>
        NextPlayOff = 8,
        /// <summary>
        /// 다음라운드진출(한국시리즈)
        /// </summary>
        NextKoriaSeries = 9,
        /// <summary>
        /// 중도탈락
        /// </summary>
        DropPostSeason = 10,
        /// <summary>
        /// 우승
        /// </summary>
        Champion = 11
    }
    /// <summary>
    /// Top기록 타입
    /// </summary>
    public enum MvpType {
        /// <summary>
        /// (투수)MVP
        /// </summary>
        P_MVP,
        /// <summary>
        /// (투수) 다승
        /// </summary>
        P_WIN,
        /// <summary>
        /// (투수) 평균자책
        /// </summary>
        P_ERA,
        /// <summary>
        /// (투수) 구원
        /// </summary>
        P_SAVE,
        /// <summary>
        /// (투수) 홀드
        /// </summary>
        P_HOLD,
        /// <summary>
        /// (투수) 삼진
        /// </summary>
        P_SO,
        /// <summary>
        /// 타자 MVP
        /// </summary>
        H_MVP,
        /// <summary>
        /// (타자) 홈런
        /// </summary>
        H_HOMERUN,
        /// <summary>
        /// (타자) 타율
        /// </summary>
        H_BA,
        /// <summary>
        /// (타자) 타점
        /// </summary>
        H_RBI,
        /// <summary>
        /// (타자) 도루
        /// </summary>
        H_SB,
        /// <summary>
        /// (타자) 안타
        /// </summary>
        H_HIT
    }

    [Obsolete("삭제됨")]
    public enum SeasonGameMode {
        Inning3 = 1,
        Inning6 = 2,
        Inning9 = 3,
        Simulation = 4
    }

    /// <summary>
    /// 시즌모드 스케줄상 경기타입
    /// </summary>
    public enum SeasonGameType {
        PennantRace = 1,
        WildCard = 2,
        SemiPlayOff = 3,
        PlayOff = 4,
        KoreaSeries = 5
    }

    /// <summary>
    /// 경기 진행 상태
    /// </summary>
    public enum SeasonScheduleState {
        /// <summary>
        /// 진행되지 않음
        /// </summary>
        Notyet = 1,
        /// <summary>
        /// 스킵됨. (포스트시즌에서 다음라운드 진출 경기수를 먼저 달성한경우 나머지 경기는 스킵됨)
        /// </summary>
        Skipped = 2,
        /// <summary>
        /// 경기 완료
        /// </summary>
        Finished = 3
    }
    [Obsolete("삭제됨")]
    public enum PostType {
        /// <summary>
        /// 선물
        /// </summary>
        Reward = 1,
        /// <summary>
        /// 상점
        /// </summary>
        Purchase = 2,
        /// <summary>
        /// 우정포인트
        /// </summary>
        FriendPoint = 3
    }
    /// <summary>
    /// 우편물 첨부 타입
    /// </summary>
    [Obsolete("삭제됨")]
    public enum PostAttachType {
        Ruby = 1,
        Gold = 2,
        Mileage = 3,
        FriendPoint = 4,
        Playball = 5,
        Item = 6
    }

    /// <summary>
    /// 재화 타입
    /// </summary>
    public enum MoneyType {
        /// <summary>
        /// 현금 (해당 타입은 앱스토어 구매)
        /// </summary>
        Cash,
        Ruby,
        Gold,
        Mileage,
        FriendPoint,
        LvpCoin
    }
    
    [Obsolete("삭제됨")]
    public enum StoreTag {
        /// <summary>
        /// 태그 없음
        /// </summary>
        None = 0,
        SALE = 1,
        NEW = 2,
        HOT = 3
    }

    /// <summary>
    /// 아이템 종류
    /// </summary>
    public enum ItemType {
        CardPack = 10, //선수팩 = 일반
        CardPackRandom = 11, //선수팩랜덤
        Trophy = 12, //트로피 (스킬변환재료)
        BoostCard = 13, //훈련선수카드
        CardPackTeam = 14, //구단팩
        GearPack = 15, //장비팩
        CardPackTeamYear = 16, //선수팩(팀&연도 선택)
        None = 99 //기타
    }

    /// <summary>
    /// 랭킹전 게임모드
    /// </summary>
    public enum RPGameMode {
        /// <summary>
        /// 일반
        /// </summary>
        Normal = 1,
        /// <summary>
        /// 리벤지
        /// </summary>
        Revenge = 2,
        /// <summary>
        /// 친선전
        /// </summary>
        Friendly = 3
    }

    /// <summary>
    /// 랭킹전 액션 플레이 종류
    /// </summary>
    public enum RankedPlayAction {
        /// <summary>
        /// 2 스트라이크 달성횟수
        /// </summary>
        Pitcher2Strike = 1,
        /// <summary>
        /// 삼진아웃 수
        /// </summary>
        PitcherStrikeOut = 2,
        /// <summary>
        /// 1루타
        /// </summary>
        HitterSingle = 3,
        /// <summary>
        /// 2루타
        /// </summary>
        HitterDouble = 4,
        /// <summary>
        /// 3루타
        /// </summary>
        HitterTriple = 5,
        /// <summary>
        /// 홈런
        /// </summary>
        HitterHomerun = 6,
        /// <summary>
        /// 번트로 점수를 냄
        /// </summary>
        HitterSqueezeplay = 7,
        /// <summary>
        /// 득점
        /// </summary>
        HitterRunsScored = 8
    }

    public enum Wdl {
        Win = 1, Draw = 2, Lose = 3
    }

    [Obsolete("삭제됨")]
    public enum RPRevengeStatus {
        /// <summary>
        /// 보호막 적용
        /// </summary>
        Shield = 2,
        /// <summary>
        /// 리벤치 가능
        /// </summary>
        NotYet = 3,
        /// <summary>
        /// 리벤지 성공
        /// </summary>
        Success = 4,
        /// <summary>
        /// 리벤지 실패
        /// </summary>
        Fail = 5
    }

    /// <summary>
    /// 친구상태
    /// </summary>
    public enum FriendStatus {
        None = 0,
        /// <summary>
        /// 요청중
        /// </summary>
        Requested = 1,
        /// <summary>
        /// 친구
        /// </summary>
        Friend = 2
    }

    /// <summary>
    /// 선수카드 능력치(스텟) 코드
    /// </summary>
    public enum CardAbCode
    {
        /// <summary>
        /// 체력
        /// </summary>
        SM,
        /// <summary>
        /// 포심
        /// </summary>
        FF,
        /// <summary>
        /// 체인지업
        /// </summary>
        CU,
        /// <summary>
        /// 슬라이더
        /// </summary>
        SD,
        /// <summary>
        /// 커브
        /// </summary>
        CV,
        /// <summary>
        /// 포크볼
        /// </summary>
        FB,
        /// <summary>
        /// 구속(히든)
        /// </summary>
        VC,
        /// <summary>
        /// 파워
        /// </summary>
        PW,
        /// <summary>
        /// 컨텍
        /// </summary>
        CT,
        /// <summary>
        /// 선구
        /// </summary>
        BE,
        /// <summary>
        /// 주력
        /// </summary>
        RN,
        /// <summary>
        /// 송구
        /// </summary>
        TW,
        /// <summary>
        /// 수비
        /// </summary>
        FD,
        /// <summary>
        /// 타구각(히든)
        /// </summary>
        TJ
    }

    public enum CardType
    {
        Normal,
        Legend
    }
    /// <summary>
    /// 일반 라인업 : Main
    /// 쟁탈전 라인업 : Sub
    /// </summary>
    public enum LineupType
    {
        A,
        B
    }

    public enum TrainingType
    {
        H1 = 1,
        H3 = 3,
        H10 = 10
    }

    public enum HomeOrAway
    {
        home, away
    }

    public enum GachaType
    {
        Normal,
        Premium
    }
    public enum GachaPoolType
    {
        /// <summary>
        /// 전체
        /// </summary>
        All,
        /// <summary>
        /// 투수
        /// </summary>
        Pitcher,
        /// <summary>
        /// 타자
        /// </summary>
        Hitter
    }
    /// <summary>
    /// 시너지 효과 타입
    /// </summary>
    public enum SynergyType
    {
        TEAM, YEAR, SEASON, GOLDEN, LEGEND
    }

    /// <summary>
    /// 장비 타입
    /// </summary>
    public enum GearType
    {
        P_Necklace = 10,
        P_Spike = 11,
        P_Glove = 12,
        P_Rosin = 13,
        H_Glasses = 20,
        H_Bat = 21,
        H_Glove = 22,
        H_Spike = 23
    }
}