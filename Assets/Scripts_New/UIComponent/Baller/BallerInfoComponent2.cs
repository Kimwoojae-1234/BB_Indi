using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

public class BallerInfoComponent2 : MonoBehaviour
{
    [SerializeField] private Image NameBar;
    [SerializeField] private TextMeshProUGUI ballerNameTxt;
    [SerializeField] private TextMeshProUGUI ballerDescTxt;
    [SerializeField] private TextMeshProUGUI handAndTypeTxt;
    [SerializeField] private TextMeshProUGUI rarityTxt;
    [SerializeField] private Image posPattern;
    [SerializeField] private TextMeshProUGUI posTxt;
    [SerializeField] private TextMeshProUGUI posTxt2;

    //[SerializeField] private Slider TierSlider;
    //[SerializeField] private TextMeshProUGUI trophyTxt;
    //[SerializeField] private Image TierImg;
    [SerializeField] private BallerTierSliderComp TierComp;


    [SerializeField] private GameObject RankSymbol; // 추후 기획 추가할 것


    public void ShowBallerInfo(CharacterData ballerData)
    {
        NameBar.color = KOBUtil.GetRarityColor(ballerData.rarity);
        ballerNameTxt.text = KOBManager.Localization.GetUILocalizedValue2(ballerData.name_id);
        ballerDescTxt.text = KOBManager.Localization.GetUILocalizedValue2(ballerData.desc_id);

        string hand_type = KOBUtil.GetHandString2(ballerData.hand);
        string char_type = KOBManager.Localization.GetUILocalizedValue2(string.Format("CharType_{0:D4}", (int)ballerData.char_type));
        handAndTypeTxt.text = string.Format("{0} - {1}",hand_type, char_type);
        rarityTxt.text = KOBManager.Localization.GetUILocalizedValue2(string.Format("Rarity.{0}", ballerData.rarity.ToString()));
        rarityTxt.color = KOBUtil.GetRarityColor(ballerData.rarity);
        posTxt.text = KOBUtil.GetPosString(ballerData.position);        
        posTxt2.text = KOBUtil.GetPosString2(ballerData.position);
        posPattern.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UITier, KOBUtil.GetPosPatter(ballerData.position));
    }


    public void ShowTrophyInfo(CharacterData ballerData, KOBBaller ballerInfo)
    {
        posTxt2.gameObject.SetActive(false);

        //트로피(선수트로피 - 팀트로피와 다름)
        RankSymbol.gameObject.SetActive(true);   //현재 랭크에 맞게

        //선수 랭크 (선수트로픽 획득에 따른)
        //티어
        TierComp.Set(ballerInfo, true); //맥스값을 보여준다
    }


    public void HideTrophyInfo()
    {
        posTxt2.gameObject.SetActive(true);

        RankSymbol.gameObject.SetActive(true);   //1랭크

        //TierSlider.gameObject.SetActive(false);
        TierComp.gameObject.SetActive(false);
    }

}
