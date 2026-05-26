using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.U2D;
using System.Linq;

public static class GameConfig
{
#if BS_DEV
#if UNITY_ANDROID
    public const string UDWURL = "http://ts-udw-v2.four33.co.kr:10030/API/getserviceinfo.aspx?ssn=1045&market=12&bundleid={0}&version={1}";
    public const string NoticeCheckURL = "http://ts-notice-ext.four33.com:10025/api/baseinfo?ssn=1045&market=12&zone=1&country=kr&lang=ko";
    public const string NoticeURL = "http://ts-notice-ext.four33.com:10025/list?ssn=1045&market=12&zone=1&lang={0}&country={1}&ftt_ptype=7&ftt_pusn=1&ftt_world=1&ftt_gusn={2}&ftt_deviceId=&ftt_fp={3}";
#elif UNITY_IOS
    public const string UDWURL = "http://ts-udw-v2.four33.co.kr:10030/API/getserviceinfo.aspx?ssn=1045&market=11&bundleid={0}&version={1}";
    public const string NoticeCheckURL = "http://ts-notice-ext.four33.com:10025/api/baseinfo?ssn=1045&market=11&zone=1&country=kr&lang=ko";
    public const string NoticeURL = "http://ts-notice-ext.four33.com:10025/list?ssn=1045&market=11&zone=1&lang={0}&country={1}&ftt_ptype=6&ftt_pusn=1&ftt_world=1&ftt_gusn={2}&ftt_deviceId=&ftt_fp={3}";
#endif

#elif BS_QA
#if UNITY_ANDROID
    public const string UDWURL = "http://ts-udw-v2.four33.co.kr:10030/API/getserviceinfo.aspx?ssn=1045&market=12&bundleid={0}&version={1}";
    public const string NoticeCheckURL = "http://ts-notice-ext.four33.com:10025/api/baseinfo?ssn=1045&market=12&zone=1&country=kr&lang=ko";
    public const string NoticeURL = "http://ts-notice-ext.four33.com:10025/list?ssn=1045&market=12&zone=1&lang={0}&country={1}&ftt_ptype=7&ftt_pusn=1&ftt_world=1&ftt_gusn={2}&ftt_deviceId=&ftt_fp={3}";
#elif UNITY_IOS
    public const string UDWURL = "http://ts-udw-v2.four33.co.kr:10030/API/getserviceinfo.aspx?ssn=1045&market=11&bundleid={0}&version={1}";
    public const string NoticeCheckURL = "http://ts-notice-ext.four33.com:10025/api/baseinfo?ssn=1045&market=11&zone=1&country=kr&lang=ko";
    public const string NoticeURL = "http://ts-notice-ext.four33.com:10025/list?ssn=1045&market=11&zone=1&lang={0}&country={1}&ftt_ptype=6&ftt_pusn=1&ftt_world=1&ftt_gusn={2}&ftt_deviceId=&ftt_fp={3}";
#endif
#elif BS_LIVE
#if UNITY_ANDROID
    public const string UDWURL = "http://udw-v2.four33.co.kr:10030/API/getserviceinfo.aspx?ssn=1045&market=12&bundleid={0}&version={1}";
    public const string NoticeCheckURL = "http://notice-ext.four33.com:10025/api/baseinfo?ssn=1045&market=12&zone=1&country={0}&lang={1}";
    public const string NoticeURL = "http://notice-ext.four33.com:10025/list?ssn=1045&market=12&zone=1&lang={0}&country={1}&ftt_ptype=7&ftt_pusn=1&ftt_world=1&ftt_gusn={2}&ftt_deviceId=&ftt_fp={3}";

#elif UNITY_IOS
    public const string UDWURL = "http://udw-v2.four33.co.kr:10030/API/getserviceinfo.aspx?ssn=1045&market=11&bundleid={0}&version={1}";
    public const string NoticeCheckURL = "http://notice-ext.four33.com:10025/api/baseinfo?ssn=1045&market=11&zone=1&country={0}&lang={1}";
    public const string NoticeURL = "http://notice-ext.four33.com:10025/list?ssn=1045&market=11&zone=1&lang={0}&country={1}&ftt_ptype=6&ftt_pusn=1&ftt_world=1&ftt_gusn={2}&ftt_deviceId=&ftt_fp={3}";

#endif
#endif
    public static string GameServerURL = string.Empty;
    public static string ChatServerURL = string.Empty;
    public static string AssetBundleUrl = string.Empty;
    public static string StadiumAssetBundleURL = string.Empty;
    public static string ToolTipAssetBundleURL = string.Empty;
    public static string ProvisionURL = string.Empty;
    public static Vector2 ScreenSize = new Vector2(750, 1625);
    public const string AssetBundleExtension = ".psbs";
    public const int MAXDECKCOUNT = 3;
    public const string API_URL = "";
    public const int PLAYERMAXLEVEL = 13;
    public const int SKILLMAXLEVEL = 5;
    public const int PlayerAbilityMaxValue = 180;
    public const int GearAbilityMaxValue = 20;
    public const int SkillAbilityPercentTypeMaxValue = 10000;
    public const int SkillAbilityValueTypeMaxValue = 30;
    public const int MATCH_MAX_PLAYER = 2;
    public static float MATCH_SEARCHING_TIMEOUT = 7.0f;
    public static float REMATCH_SEARCHING_TIMEOUT = 30.0f;
    public static float TOURNAMENT_SEARCHING_TIMEOUT = 30.0f;
    public static int TOURNAMENT_OPEN_EXTRA_TIME = 10;
    public const int GEAROPENSTADIUM = 4;

    // Server
    public static bool IapEnable = false;
    public static bool PassEnabled = false;
    public static bool LobbyLeftShortCutEnabled = false;
    public static bool LobbyRightShortCutEnabled = false;
    public static bool InReviewMode = false;
    public static bool ClientLogging = false;
    public static byte PunRegion = 8;
    public static int MyTeamWin = 0;
    //public static int UserScenario = 0;
    public static bool FacebookLoginEnable = false;
    public static bool PlayWithFriendEnable = false;
    public static bool StrikeOutEnable = false;
    public static bool ToDayFirstVictoryEnable = false;
    public static bool Clan_ContentsEnable = false;
    public static bool Clan_LeagueEnable = false;
    public static int ClanChatMsgExpiresDay = 7;
    public static int Code307RetryMs = 0;
    public static bool Rematch_ContentEnable = false;
    public static bool InGame_ControlCorrection_Enable = true; //희귀도별 변화구 변화 타이밍 조정
    public static bool InGame_BreakingBallCorrection_Enable = true; //희귀도별 변화구 변화량 타이밍
    public static bool InGame_BattingPowerCorrection_Enable = true; //희귀도별 파워배팅 어드벤티지
    public static bool InGame_PitchingGuageCorrection_Enable = true; //희귀도별 피칭게이지 조정
    public static bool InGame_BattingMaxPowerLimit_Enable = true; //희귀도별 배팅 맥스파워 제한
    public static bool InGame_BatterFX_Enable = true;
    public static bool InGame_BatterUIFX_Enable = true;
    public static bool InGame_ComicsFX_Enable = true;
    public static bool Enable_Button_Social_All = true;
    public static bool Enable_Button_Social_1 = true;
    public static bool Enable_Button_Social_2 = true;
    public static bool Enable_Button_Social_3 = true;
    public static bool Enable_Button_Social_4 = true;
    public static bool LobbyLeftShortcut_LowCoin_Enabled = false;
    public static bool AdvancedMatchingEnable = true;
    public static bool Enable_AdsShopPopUp = false;
    public static short Rematch_WaitingTimeForAcceptance = 30;
    public static bool Enable_Ads_Stadium_Bat = false;
    public static bool Enable_Ads_Stadium_Coin_Lose = false;
    public static short Open_Max_Stadium = 0;
    public static short Ingame_Overtime = 12;
    public static bool Enable_Joystic_Guide = false;
    public static short Joystic_Guide_Max_Stadium = 0;
    public static bool Enable_Pitch_Guide = false;
    public static short Pitch_Guide_Max_Stadium = 0;
    public static short Tutorial_PowerBetting_PlayNum = 0;
    public static short DeviceHacking_CheckCount = 0;
    public static bool DeviceHacking_Enable = false;
    public static List<int> DeviceHacking_StartPvENum = new List<int>();
    public static List<int> Card_Rarity_Highest = new List<int>();
    public static List<int> Levelup_Package_Skip_Idx = new List<int>();

    public static int PvE_Bat_LvLimit = 4;
    public static int PvE_Ball_LvLimit = 4;

    //홈런더비
    public static int Homerun_Derby_BasicTime = 90;
    public static int Homerun_Derby_BonusTime = 10;
    public static int Homerun_Derby_BonusDistance = 135;

    public static bool AdsShopCoinEnable = false;
    public static bool Enable_AdsShop_Sort = false;
    public static short AdsShop_Category_ThreeOut = 0;
    public static short AdsShop_Category_LuckySpin = 0;
    public static short AdsShop_Category_FG1 = 0;
    public static short AdsShop_Category_FG2 = 0;
    public static short AdsShop_Category_FG3 = 0;
    public static short AdsShop_Category_FG4 = 0;
    public static bool AdsFreeReward4Enable = true;
    public static bool ShopBatLuckyDrawEnable = false;
    public static int Tooltip_Max_Stadium = 0;
    public static bool Enable_Ads_EventGame_Free_Entrance = false;
    public static bool Enable_Ads_EventGame_Bonus_Chance = false;
    public static bool Enable_Ads_Stadium_Lose_Trophy = false;
    public static bool Enable_Ads_UltimateArena_Lose_Point = false;
    public static DateTime Equip_Return_Time = DateTime.Now.AddDays(+1);
    public static int RankingOpenStadium = 4;
    public static int UltimateArenaOpenStadium = 4;
    public static int UltimateArena_Calculate_Timer = 0;
    public static string UltimateArenaLeaguePushTimer = string.Empty;
    public static int UltimateArenaConnectTime = 10;
    public static int CalledGame_ScoreValue = 6;
    public static short emoji_deck_open_stadium = 0;
    public static bool ChallengerMissonEnable = true;
    public static int ChallengerMissonAbleLevel = 4;
    public static int Max_Tournament_Point_Value = 60000;
    public static int Ranking_Category_Winners = 1;
    public static int Ranking_Category_Global = 2;
    public static int Ranking_Category_Ultimate = 3;
    public static int Ranking_Catrgory_Tournament = 4;

    // shop
    public static int Shop_Category_StarPass = 1;
    public static int Shop_Category_Package = 2;
    public static int Shop_Category_DailyCard = 3;
    public static int Shop_Category_Bag = 4;
    public static int Shop_Category_Coin = 5;
    public static int Shop_Category_Gem = 6;
    public static int Shop_Category_Bat = 8;
    public static int Shop_Category_Ball = 7;
    public static int Shop_Category_Focus = 2;

    // strikeout cost.
    public static int StrikeOutStartGem = 100;
    public static int StrikeOutContinueGem = 50;

    // 
    public static bool isKorean = false;
    public static bool isJapanese = false;
    public static string CurrencyCode = string.Empty;

    // DIrectionValue
    public static int TrophyDirectionPlusValue = 0;
    public static long CoinDirectionPlusValue = 0;
    public static int GemDirectionPlusValue = 0;
    public static int FieldBonusPlusValue = 0;
    public static List<int> MissionTokenPlusValue = new List<int>();
    
    //

    public static bool ChallengeMissionRankDirection = false;
    public static bool ChallengeMissionCompletePopup = false;
    public static bool ChallengeUpdateInBattleRecover = false;

    public static bool EnableScout = true;

    public static int LevelUpChanceLevel = 12;
    public static int LevelUpChanceExp = 5000;

    public static bool TournamentPassSwitchButton = false;
    public static int TournamentRankUpdateMinute = 5;

    public static int TournamentScheduleShowLimit = 10;
    public static bool AssetBundleCheck = false;

    public static string UserCountryCode = string.Empty;
    public static string UserTimeZone = string.Empty;

    public static bool Stadium1Loading = false;
    public static bool InitMarketData = false;

    public static bool UDWReviewMode = false;

    public static bool MatchEnded = false;
    public static bool UpdateArenaRanking = false;

    // condition popup manager.
    public static int PvE_MatchingNumber = -1;
    public static int PvE_MatchingResult = -1;
    public static int TrophyRoadRewardMaxIndex = -1;
    public static List<int> TrophyRoadRewardGetList = new List<int>();
    public static int TrophyRoadRewardNextMatchingResult = -1;
    public static bool TrophyRoadRewardNextCheck = false;
    public static bool CountryAdsViewed = false;

    public static int TooltipConditionValue;
    
    public static bool Enable_AdsShop_Tap = true;
    public static bool Enable_Lobby_AdsShop = true;

    public static bool EnableAccountLinking = true;
    public static string Discord_Gem = string.Empty;
#if BS_LIVE
    public static string Discord_Link = "https://dl-1045-bs-cdn-origin.s3.amazonaws.com/AUW/1045/live/WBSWeb/Discord.html";
#elif BS_QA
    public static string Discord_Link = "https://dl-1045-bs-cdn-origin.s3.amazonaws.com/AUW/1045/qa/WBSWeb/Discord.html";
#elif BS_DEV
    public static string Discord_Link = "https://dl-1045-bs-cdn-origin.s3.amazonaws.com/AUW/1045/dev/WBSWeb/Discord.html";
#endif
    public static string EventGame_SchedulePopUp_URL_EN = string.Empty; // 영어
    public static string EventGame_SchedulePopUp_URL_SC = string.Empty; // 간체
    public static string EventGame_SchedulePopUp_URL_TC = string.Empty; // 번체
    public static string EventGame_SchedulePopUp_URL_JP = string.Empty; // 일본어
    public static string EventGame_SchedulePopUp_URL_KR = string.Empty; // 한국어
    public static string EventGame_SchedulePopUp_URL_ES = string.Empty; // 스페인어
#if !BS_LIVE
    public static string GearRefundInfoURL = "http://ts-notice-ext.four33.co.kr:10025/view?cont_no=14{0}";
#else
    public static string GearRefundInfoURL = "http://notice-ext.four33.co.kr:10025/view?cont_no=141{0}";
#endif

#if !BS_LIVE
    public static string ToolTipFixedName = string.Empty;
#endif

    public static int RematchInning = 0;
    public const int TournamentStadiumIndex = 102;
    public const int SurvivalStadiumIndex = 101;
    public static bool TooltipImageLoadComplete = false;
    public static bool AdsRewardServertoServer = false;
#if UNITY_EDITOR
    public static bool UseEditorAssetbundleCheck = false;
#endif
    public static int PhotonPing = 9999;



    public delegate void Callback();


    public static int BallHitReductionPreventValue = 5;
    public static int BallHenkaRate = 100;
    public static int BallBotStadiumLimit = 4;  //해당스타디움 밑은 볼을 던지지마



    public static GameDefine.eLanguage CurrentLanguage = GameDefine.eLanguage.English;

    public static void ChangeLanguage()
    {
        if (PlayerPrefs.HasKey("RegistLanguae"))
        {
            CurrentLanguage = GetRegistLanguage();
            return;
        }

        PlayerPrefs.SetString("RegistLanguae", nameof(GameDefine.eLanguage.English)); //임시 - 우선은 영어로
        CurrentLanguage = GameDefine.eLanguage.English;

        /* //추후 이걸로...
        switch (Application.systemLanguage)
        {
            case SystemLanguage.English:
                PlayerPrefs.SetString("RegistLanguae", nameof(GameDefine.eLanguage.English));
                CurrentLanguage = GameDefine.eLanguage.English;
                break;
            case SystemLanguage.Korean:
                PlayerPrefs.SetString("RegistLanguae", nameof(GameDefine.eLanguage.Korea));
                CurrentLanguage = GameDefine.eLanguage.Korean;
                break;
            case SystemLanguage.Japanese:
                PlayerPrefs.SetString("RegistLanguae", nameof(GameDefine.eLanguage.Japan));
                CurrentLanguage = GameDefine.eLanguage.Japanese;
                break;
            case SystemLanguage.Spanish:
                PlayerPrefs.SetString("RegistLanguae", nameof(GameDefine.eLanguage.Spain));
                CurrentLanguage = GameDefine.eLanguage.Spanish;
                break;
            case SystemLanguage.ChineseTraditional:
                PlayerPrefs.SetString("RegistLanguae", nameof(GameDefine.eLanguage.China_Traditional));
                CurrentLanguage = GameDefine.eLanguage.ChineseTraditional;
                break;
            case SystemLanguage.ChineseSimplified:
                PlayerPrefs.SetString("RegistLanguae", nameof(GameDefine.eLanguage.China_Simplified));
                CurrentLanguage = GameDefine.eLanguage.ChineseSimplified;
                break;
            default:
                PlayerPrefs.SetString("RegistLanguae", nameof(GameDefine.eLanguage.English));
                CurrentLanguage = GameDefine.eLanguage.English;
                break;
        }*/
    }


    public static GameDefine.eLanguage GetRegistLanguage()
    {
        GameDefine.eLanguage langauge = GameDefine.eLanguage.English;
        if (PlayerPrefs.HasKey("RegistLanguae"))
        {
            langauge = (GameDefine.eLanguage)System.Enum.Parse(typeof(GameDefine.eLanguage), PlayerPrefs.GetString("RegistLanguae"));
        }
        return langauge;
    }
}