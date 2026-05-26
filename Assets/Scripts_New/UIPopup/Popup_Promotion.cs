using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Popup_Promotion : UIPopup
{
    [SerializeField] private GameObject preLogoObj;
    [SerializeField] private GameObject nextLogoObj;
    [SerializeField] private Image preLogo;
    [SerializeField] private Image nextLogo;
    [SerializeField] private TextMeshProUGUI textTitle;
    [SerializeField] private GameObject LogoObj;
    [SerializeField] private GameObject ballerObj;
    [SerializeField] private GameObject ballerPos;
    [SerializeField] private GameObject continueBtn;



    public enum PromotionType
    {
        Account_Tier,
        Baller_Reputation,
        League_Promotion
    }



    private PromotionType CurType;

    public override void Set(Intent it = null)
    {
        base.Set(it);

        textTitle.gameObject.SetActive(false);
        preLogoObj.gameObject.SetActive(false);
        nextLogoObj.gameObject.SetActive(false);
        continueBtn.gameObject.SetActive(false);

        //ballerObj.gameObject.SetActive(true);

        CurType = (PromotionType)Enum.Parse(typeof(PromotionType), it["PromotionType"].ToString()); 


        if(CurType == PromotionType.Account_Tier)
        {
            SetAccountTier();
        }
        else if (CurType == PromotionType.Baller_Reputation)
        {
            SetBallerReputation();
        }
        else if (CurType == PromotionType.League_Promotion)
        {
            SetLeaguePromotion();
        }

        StartCoroutine(process());
    }


    private void SetAccountTier()
    {
        textTitle.text = "Your Tier has increased!";
    }


    private void SetBallerReputation()
    {
        textTitle.text = "Baller's Reputation has increased!";
    }


    private void SetLeaguePromotion()
    {
        textTitle.text = "League Promotion achieved!";
    }


    IEnumerator process()
    {
        LogoObj.gameObject.SetActive(true);
        preLogoObj.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        preLogoObj.gameObject.SetActive(false);
        nextLogoObj.gameObject.SetActive(true);
        textTitle.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        continueBtn.gameObject.SetActive(true);
    }


    public void OnClickContinue()
    {
        Close();
    }

}
