using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDefine 
{

    public enum eUIEvnet
    {
        MissionUpdate,
        FieldBonusUpdate,
        FreeGiftUpdate,
        SocialUpdate,
        DeckUpdate,
        ShopUpdate,
        BattleReady,
        FacebookDataUpdate,
        DeckChangeUseMode,
        MoveBottomMenu,
        StartBattleBotPlay,
        OpenSearchMatching,
        OpenSearchMatchingTutorial,
        ShowCoinCharge,
        HideCoinCharge,
        ShowGemCharge,
        HideGemCharge,
        AccountLinkUpdate,
        FacebookSocialLoginComplete,
        TopCoinTextAnimation,
        TopCoinFlash,
        TopGemTextAnimation,
        TopGemFlash,
        TopTrophyTextAnimation,
        TopTrophyFlash,
        //LobbyTrophyTextAnimation,
        //LobbyTrophyFlash,
        LobobyFieldBonusFlash,
        GameOptionConfigUpdate,
        ProfileUpdate,
        IAPInitComplete,
        ViewConditionPopup,
        AdsInfoUpdate,
        AssetbundleDownloadComplete,
        ShowLobbyBannerAnimation,
        HideLobbyBannerAnimation,
        NewSeasonStart,
        StadiumSelectInit,
        UpdateDeckAlram,
        SmoothMoveDeck,
        StartGuideClickDeckItem,
        StartGuideUpgradeStart,
        StartGuideAutoPositionComplete,
        StadiumUnlockDirectionEnd,
        NickNameExistCheckComplete,
        DailyCardUpdate,
        TrophyRoadUIUpdte,
        CardExpDirectionEnd,
        CheckCardChangeTutorial,
        TrophyroadDirectionEnd,
        TutorialPlayerInfoPopupClose,
        MissionTokenDirectionEnd,
        CardUpgradeUpdate,
        AdsPopupUpdate,
        AdsConditionPopupClose,
        AdsAttendanceUpdateRequest,
        AdsAttendanceUpdateResponse,
    }



    public enum ePlayerPosition
    {
        SP = 1,         //StartingPitcher
        C,              //Catcher
        B1,             //FirstBaseMan
        B2,             //SecondBaseMan
        B3,             //ThirdBaseMan
        Ss,             //ShotStop
        Lf,             //LeftFielder
        Cf,             //CenterFielder
        Rf,             //RightFielder
        MAX
    }


    public enum eStartingPitcherOrder
    {
        SP1 = 1,
        SP2,
        SP3,
        SP4,
        MAX
    }


    public enum eCardType
    {
        HItter = 0,
        Pitcher,
        Gear,
        MAX
    }


    public enum eHand
    {
        Left,
        Right
    }

    public enum eCardRarity
    {
        Common = 1,
        Rare,
        Epic,
        Legendary,
        Highest
    }


    public enum DefenseTypeDefault
    {
        C = 1,
        IF,
        OF,
        IF_C,
        OF_C,
        IF_OF,
        ALL,
        P,
        MAX
    }


    public enum InGameSkillType
    {
        None = 0,
        STAT_INCREASE,
        BATTING,
        FIELDING,
        DEFENSE,
        THROWING,
        RUNNING,
        LEGEND_STAT_INCREASE,
        LEGEND_BATTING,
        LEGEND_FIELDING,
        LEGEND_THROWING,
        LEGEND_RUNNING,
        BUFF_STAT_INCREASE,
        BUFF_BATTING,
        BUFF_FIELDING,
        BUFF_THROWING,
        BUFF_RUNNING,
        BATTING_DEBUFF,
        BUFF_BATTING_DEBUFF,
        MAX
    }

    public enum eEvent
    {
        CurrencyUpdate,
        DeckUpdate,
        DeckChangeUseMode,
        AssetDownloadUpdate,
        AssetDownComplete,
        AssetDownFail,
        LoadAssetBundleComplete,
        DestroyManager,
        NoHaveAssetBundle,
        LoginComplete,
        SceneChangeComplete,
        MissionUpdate,
        FieldBonusUpdate,
        ShopUpdate,
        BottomMenuUpdownMove,
        TopMenuUpdownMove,
        BattleReady,
        FacebookDataUpdate,
        PhotonRegistRoomComplete,
        BotPlayRegistRoomComplete,
        OpenSearchMatching,
        OpenSearchMatchingTutorial,
        ShowAds,
        ShowInboxAdsReward,
        GameOptionConfigUpdate,
        PhotonConnectComplete,
        PhotonCreatedRoom,
        Photon,
        BuyIAPProduct,
        GemWealthUpdate,
        CoinWealthUpdate,
        TrophyWealthUpdate,
        CheckConditionPopup,
        OpenNextConditionPopup,
        NightPushOnOff,
        BottomShopAlarmUpdate,
        BottomClanAlarmUpdate,
        CheckCardChangeTutorial,
        PlusDownLoadCount,
        AssetBundleDownloadVersionUpdate,
    }

    public struct EventParam
    {
        public int[] intValue;
        public float[] floatValue;
        public string[] strValue;
        public double[] doubleValue;
        public long[] longValue;
        public uint[] uintValue;
        public bool[] boolValue;
    }


    public enum eLanguage
    {
        English,
        China_Simplified,
        France,
        Germany,
        China_Traditional,
        Indonesia,
        Italy,
        Japan,
        Korea,
        Portugal,
        Russia,
        Spain,
        Thailand,
        Turkey,
        Vietnam,
        MAX,
    }

}
