using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using UnityEngine.U2D;

public class Ui_PlayerCard : UIItem
{
    [SerializeField]
    private Image RarityBG = null;
    [SerializeField]
    private Image RarityBorder = null;
    [SerializeField]
    private Image RarityBorder_Empty = null;
    [SerializeField]
    private Text PositionText = null;
    [SerializeField]
    private Text DefensePositionText = null;
    [SerializeField]
    private Image LegendAura = null;
    [SerializeField]
    private Image BlackAura = null;
    [SerializeField]
    private Image PlayerPortrait = null;
    [SerializeField]
    private GameObject GameOffObj = null;
    [SerializeField]
    private Text GameOffText = null;
    [SerializeField]
    private Material GrayScaleMaterial;
    [SerializeField]
    private Material DistortionMaterial;
    [SerializeField]
    private GameObject SparksObject = null;
    [SerializeField]
    //private DummyVisual dummyVisual = null;

    private CardInfo PlayerCardInfo = null;
    public CardInfo GetPlayerCardInfo { get { return PlayerCardInfo; } }

    public Image Portrait
    {
        get
        {
            return PlayerPortrait;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void SetParentWidnow(Transform parent_window)
    {
        base.SetParentWidnow(parent_window);
    }

    public void SetPlayerCard(CardInfo card_info)
    {
        if (card_info == null)
            return;
        PlayerCardInfo = card_info;
        GameDefine.eCardType cardtype = GetPlayerCardType();
        //RarityBG.sprite = GameConfig.GetSpriteCardRarityBG(GetPlayerCardRarity());
        RarityBG.material = null;
        //RarityBorder.sprite = GameConfig.GetSpriteCardRarityBorder(GetPlayerCardRarity());
        RarityBorder.material = null;
        RarityBorder.gameObject.SetActive(true);
        RarityBorder_Empty.gameObject.SetActive(false);
        PlayerPortrait.sprite = GetSpritePlayerPortrait(card_info.GetPlayerData().GetPlayerPortraitTag());
        PlayerPortrait.material = null;
        //SetPlayerPosition(GetPlayerPosition(GetPlayerCardType() == GameDefine.eCardType.HItter ? MainManager.MyInfo.GetUserHitterPresetNo() : 0));
        SetGameOff(card_info.GetGameOffCount());
        UpdateCard(GetPlayerCardRarity());
    }

    public void SetPlayerCard_Ingame(CardInfo card_info)
    {
        
    }

    public void SetPlayerCardAndSkill_Ingame(CardInfo card_info)
    {
        
    }

    public void SetNotFoundPlayerCard()
    {
        RarityBorder.gameObject.SetActive(false);
        RarityBorder_Empty.gameObject.SetActive(true);
        //RarityBorder_Empty.sprite = GameConfig.GetSpriteCardRarityBorderEmpty(GetPlayerCardRarity());
        UpdateCard(GameDefine.eCardRarity.Common);
    }

    public void SetDarkCard(bool isDark)
    {
        if(isDark)
        {
            RarityBG.color = new Color(113 / 255f, 113 / 255f, 113 / 255f, 1);
            RarityBorder_Empty.color = new Color(113 / 255f, 113 / 255f, 113 / 255f, 1);
            PlayerPortrait.color = new Color(113 / 255f, 113 / 255f, 113 / 255f, 1);
            DefensePositionText.color = new Color(113 / 255f, 113 / 255f, 113 / 255f, 1);
            UpdateCard(GameDefine.eCardRarity.Common);
        }
        else
        {
            RarityBG.color = Color.white;
            RarityBorder_Empty.color = Color.white;
            PlayerPortrait.color = Color.white;
            DefensePositionText.color = Color.white;
            UpdateCard(GetPlayerCardRarity());
        }        
    }

    public void SetPlayerCard_TrophyRoadReward(CardInfo card_info)
    {
        if (card_info == null)
            return;
        PlayerCardInfo = card_info;
        //RarityBG.sprite = GameConfig.GetSpriteCardRarityBG(GetPlayerCardRarity());
        RarityBG.material = null;
        PlayerPortrait.sprite = GetSpritePlayerPortrait(card_info.GetPlayerData().GetPlayerPortraitTag());
        ShowBorderEmpty(true);
        HidePlayerPosition();
        GameOffObj.gameObject.SetActive(false);
        UpdateCard(GetPlayerCardRarity());
    }

    public void SetPlayerPosition(GameDefine.ePlayerPosition eposition)
    {
        string strPosition = GetSpritePositionIcon(eposition);
        if(string.IsNullOrEmpty(strPosition))
        {
            DefensePositionText.gameObject.SetActive(true);
            PositionText.gameObject.SetActive(false);
            DefensePositionText.text = GetStringDefenseTypeDefault(this.PlayerCardInfo.GetDefenseTypeDefault());
            //DefensePositionText.font = MainManager.Localization.LoadFont(GameConfig.GetRegistLanguage());
        }
        else
        {
            DefensePositionText.gameObject.SetActive(false);
            PositionText.gameObject.SetActive(true);
            PositionText.text = GetSpritePositionIcon(eposition);
            //PositionText.font = MainManager.Localization.LoadFont(GameConfig.GetRegistLanguage());
        }
    }

    public void SetPlayerPitcherPosition()
    {
        PositionText.text = "P";
        PositionText.gameObject.SetActive(PositionText == null ? false : true);
    }

    public void HidePlayerPosition ()
    {
        PositionText.gameObject.SetActive(false);
        DefensePositionText.gameObject.SetActive(true);
        PositionText.gameObject.SetActive(false);
        DefensePositionText.text = GetStringDefenseTypeDefault(this.PlayerCardInfo.GetDefenseTypeDefault());
        //DefensePositionText.font = MainManager.Localization.LoadFont(GameConfig.GetRegistLanguage());
    }

    public void ShowBorderEmpty(bool view)
    {
        //RarityBorder_Empty.sprite = GameConfig.GetSpriteCardRarityBorderEmpty(GetPlayerCardRarity());
        RarityBorder_Empty.gameObject.SetActive(view);
        RarityBorder.gameObject.SetActive(!view);
    }
     
    public void SetGameOff(int gameoff_count)
    {
        if (GameOffObj == null)
            return;
        if(gameoff_count<=0)
        {
            GameOffObj.SetActive(false);
        }
        else
        {
            GameOffObj.SetActive(true);
            //GameOffText.text = string.Format(MainManager.Localization.GetUILocalizedValue("Deck.Gameoff", GameOffText), gameoff_count);
        }
    }

    public void SetRayCastTarget(bool onoff)
    {
        /*if (dummyVisual == null)
            return;
        GameConfig.SetRaycastTarget(dummyVisual, onoff);*/
    }

    public void SetGrayScale(bool isGray)
    {
        if (isGray)
        {
            RarityBG.material = GrayScaleMaterial;
            RarityBorder.material = GrayScaleMaterial;
            PlayerPortrait.material = GrayScaleMaterial;
        }
        else
        {
            RarityBG.material = null;
            RarityBorder.material = null;
            PlayerPortrait.material = null;
        }
    }

    private void UpdateCard(GameDefine.eCardRarity rarity)
    {
        if ((PlayerCardInfo != null) && (LegendAura != null))
        {
            SparksObject.SetActive(false);
            LegendAura.gameObject.SetActive(false);
            BlackAura.gameObject.SetActive(false);
            if (rarity == GameDefine.eCardRarity.Highest)
            {
                BlackAura.gameObject.SetActive(true);
                BlackAura.sprite = GetSpritePlayerPortrait(PlayerCardInfo.GetPlayerData().GetPlayerPortraitTag());
                BlackAura.material.shader = Shader.Find(BlackAura.material.shader.name);
                RarityBG.material = DistortionMaterial;
                RarityBG.material.shader = Shader.Find(RarityBG.material.shader.name);
            }
            else if (rarity == GameDefine.eCardRarity.Legendary)
            {
                LegendAura.gameObject.SetActive(true);
                LegendAura.sprite = GetSpritePlayerPortrait(PlayerCardInfo.GetPlayerData().GetPlayerPortraitTag());
                LegendAura.material.shader = Shader.Find(LegendAura.material.shader.name);
                RarityBG.material = DistortionMaterial;
                RarityBG.material.shader = Shader.Find(RarityBG.material.shader.name);
            }
            else if (rarity == GameDefine.eCardRarity.Epic)
            {
                SparksObject.SetActive(true);
                RarityBG.material = null;

            }
            else
            {
                RarityBG.material = null;
            }
        }
    }
    public void UpdatePlayerCardUI()
    {
        GameDefine.eCardType cardtype = GetPlayerCardType();
        //RarityBG.sprite = GameConfig.GetSpriteCardRarityBG(GetPlayerCardRarity());
        //RarityBorder.sprite = GameConfig.GetSpriteCardRarityBorder(GetPlayerCardRarity());
        //PositionText.text = GetSpritePositionIcon(GetPlayerPosition(GetPlayerCardType() == GameDefine.eCardType.HItter ? MainManager.MyInfo.GetUserHitterPresetNo() : 0));
    }

    public string GetPlayerCardNameID()
    {
        if (PlayerCardInfo == null || PlayerCardInfo.GetPlayerData() == null)
            return string.Empty;

        return PlayerCardInfo.GetPlayerData().GetPlayerNameID();
    }

    public string GetSpritePositionIcon(GameDefine.ePlayerPosition PlayerPosition)
    {
        string strPosition = string.Empty;
        /*switch(PlayerPosition)
        {
            case GameDefine.ePlayerPosition.C:
                strPosition = MainManager.Localization.GetUILocalizedValue("Deck.Position07", null);
                break;
            case GameDefine.ePlayerPosition.B1:
                strPosition = MainManager.Localization.GetUILocalizedValue("Deck.Position08", null);
                break;
            case GameDefine.ePlayerPosition.B2:
                strPosition = MainManager.Localization.GetUILocalizedValue("Deck.Position03", null);
                break;
            case GameDefine.ePlayerPosition.B3:
                strPosition = MainManager.Localization.GetUILocalizedValue("Deck.Position01", null);
                break;
            case GameDefine.ePlayerPosition.Ss:
                strPosition = MainManager.Localization.GetUILocalizedValue("Deck.Position04", null);
                break;
            case GameDefine.ePlayerPosition.Cf:
                strPosition = MainManager.Localization.GetUILocalizedValue("Deck.Position02", null);
                break;
            case GameDefine.ePlayerPosition.Rf:
                strPosition = MainManager.Localization.GetUILocalizedValue("Deck.Position05", null);
                break;
            case GameDefine.ePlayerPosition.Lf:
                strPosition = MainManager.Localization.GetUILocalizedValue("Deck.Position06", null);
                break;
        }*/
        return strPosition;
    }

    public string GetStringDefenseTypeDefault(GameDefine.DefenseTypeDefault defenseTypeDefault)
    {
        string strPosition = string.Empty;
        /*switch(defenseTypeDefault)
        {
            case GameDefine.DefenseTypeDefault.C:
                strPosition = MainManager.Localization.GetUILocalizedValue("CardPosition.Type.01", null);
                break;
            case GameDefine.DefenseTypeDefault.IF:
                strPosition = MainManager.Localization.GetUILocalizedValue("CardPosition.Type.02", null);
                break;
            case GameDefine.DefenseTypeDefault.OF:
                strPosition = MainManager.Localization.GetUILocalizedValue("CardPosition.Type.03", null);
                break;
            case GameDefine.DefenseTypeDefault.IF_C:
                strPosition = MainManager.Localization.GetUILocalizedValue("CardPosition.Type.04", null);
                break;
            case GameDefine.DefenseTypeDefault.OF_C:
                strPosition = MainManager.Localization.GetUILocalizedValue("CardPosition.Type.05", null);
                break;
            case GameDefine.DefenseTypeDefault.IF_OF:
                strPosition = MainManager.Localization.GetUILocalizedValue("CardPosition.Type.06", null);
                break;
            case GameDefine.DefenseTypeDefault.ALL:
                strPosition = MainManager.Localization.GetUILocalizedValue("CardPosition.Type.07", null);
                break;
            case GameDefine.DefenseTypeDefault.P:
                strPosition = MainManager.Localization.GetUILocalizedValue("CardPosition.Type.08", null);
                break;
        }*/
        return strPosition;
    }

    private Sprite GetSpritePlayerPortrait(string portraitTag)
    {
        return null;// MainManager.AssetBundle.ResourcesLoad<Sprite>("PlayerPortrait", portraitTag);
    }

    public GameDefine.eCardType GetPlayerCardType()
    {
        if (PlayerCardInfo == null || PlayerCardInfo.GetPlayerData() == null)
            return GameDefine.eCardType.MAX;

        return PlayerCardInfo.GetPlayerData().GetPlayerType();
    }

    public GameDefine.eCardRarity GetPlayerCardRarity()
    {
        if (PlayerCardInfo == null || PlayerCardInfo.GetPlayerData() == null)
            return GameDefine.eCardRarity.Common;

        return PlayerCardInfo.GetPlayerData().GetPlayerRarity();
    }

    public GameDefine.ePlayerPosition GetPlayerPosition(int deckPreset)
    {
        if (PlayerCardInfo == null)
            return GameDefine.ePlayerPosition.MAX;
        return PlayerCardInfo.GetPlayerPosition(deckPreset);
    }

    public int GetCardLevel()
    {
        if (PlayerCardInfo == null || PlayerCardInfo.GetPlayerData() == null)
            return 0;

        return PlayerCardInfo.GetCardLevel();
    }

    public int GetCardEXP()
    {
        if (PlayerCardInfo == null || PlayerCardInfo.GetPlayerData() == null)
            return 0;

        return PlayerCardInfo.GetCardExp();
    }

    public int GetLevelUpNeedCard()
    {
        if (PlayerCardInfo == null)
            return 0;

        /*CardLevelBalanceData Balancedata = MainManager.Database.LoadCardLevelBalance(PlayerCardInfo.GetPlayerData().Idx, PlayerCardInfo.GetCardLevel());
        if (Balancedata == null)
            return 0;
        return Balancedata.LvupExp;*/
        return 0;

    }

    public int GetDeckNo()
    {
        if (PlayerCardInfo == null)
            return 0;
        return 0;// PlayerCardInfo.GetDeckPreset(GetPlayerCardType() == GameDefine.eCardType.HItter ? MainManager.MyInfo.GetUserHitterPresetNo() : 0);
    }

    public long GetDeckItemID(int deckPreset)
    {
        if (PlayerCardInfo == null)
            return 0;
        return PlayerCardInfo.GetDeckItemID(deckPreset);
    }

    public override void CloseUI()
    {
        base.CloseUI();
    }

    public override void Uninitialize()
    {
        base.Uninitialize();
    }
}
