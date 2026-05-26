using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class Ui_IngamePlayerCard : UIItem
{
    [SerializeField]
    private Ui_PlayerCard playerCard;
    [SerializeField]
    private GameObject AbilityObject = null;
    [SerializeField]
    private Text AbilityValue = null;
    [SerializeField]
    private Text AbilityValueShadow = null;
    [SerializeField]
    private Slider AbilitySlider = null;
    [SerializeField]
    private Text AbilityName = null;

    [SerializeField]
    private Text LevelText = null;
    [SerializeField]
    private GameObject ShineDirection = null;
    [SerializeField]
    private Text PlayerName = null;


    public override void OpenUI()
    {
        base.OpenUI();
    }

    public void SetIngamePlayerCardAndSkill(CardInfo card_info)
    {
        
    }
    public void SetIngamePlayerCard(CardInfo card_info,string ability_name , int ability_value, GameDefine.ePlayerPosition position, string colorIndex, bool bPitcherHitter = false)
    {
        if (card_info == null)
            return;
        /*playerCard.SetPlayerCard_Ingame(card_info);
        //if(position == GameDefine.ePlayerPosition.SP) playerCard.SetPlayerPitcherPosition();
        //else playerCard.SetPlayerPosition(position);
        AbilityName.text = colorIndex + MainManager.Localization.GetUILocalizedValue(ability_name, AbilityName) + "</color>";

        GameDefine.eLanguage lan = GameConfig.GetRegistLanguage();
        if (lan == GameDefine.eLanguage.Korea)
        {
            //한국어 단독 적용
            AbilityName.GetComponent<RectTransform>().sizeDelta = new Vector2(112, 20);
        }
        else if (lan == GameDefine.eLanguage.China_Simplified || lan == GameDefine.eLanguage.China_Traditional)
        {
            //한국어 단독 적용
            AbilityName.GetComponent<RectTransform>().sizeDelta = new Vector2(112, 22);
        }

        //5CD9F8 히팅 "<color=#5CD9F8>"
        //C4F6A5 디펜스 "<color=#C4F6A5>"
        //FFDB5D 러닝 "<color=#FFDB5D>"
        //FE3424 피칭 "<color=#FE3424>"
        AbilityValue.text = string.Format("{0}", ability_value);
        AbilityValueShadow.text = string.Format("{0}", ability_value);
        AbilitySlider.maxValue = 150;
        AbilitySlider.value = ability_value;

        

        //Transform trans = transform.Find("playerName");
        if(PlayerName != null)
        {
            PlayerName.gameObject.SetActive(true);
            PlayerName.text = baseballplay.Util.GetStringKey_Ingame(card_info.GetPlayerData().NameId, PlayerName);// //MainManager.Localization.GetUILocalizedValue(card_info.GetPlayerData().NameId, null);
        }
        if (ShineDirection != null)
        {
            ShineDirection.SetActive(card_info.GetPlayerData().GetPlayerRarity() == GameDefine.eCardRarity.Legendary);
        }*/
    }

    public override void CloseUI()
    {
        base.CloseUI();
    }

    public override void Uninitialize()
    {
        base.Uninitialize();
    }

    public override void OnRecieveEvent(GameDefine.eUIEvnet uiEvent)
    {
        base.OnRecieveEvent(uiEvent);
    }
}
