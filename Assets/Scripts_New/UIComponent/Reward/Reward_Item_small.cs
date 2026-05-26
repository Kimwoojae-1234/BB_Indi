using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Reward_Item_small : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountTxt;
    [SerializeField] private GameObject bonusObj;
    [SerializeField] private TextMeshProUGUI bonusTxt;

    public void InitItem(KOBRewardInfo item)
    {
        SetSpr(item);
        amountTxt.text = string.Format("+{0}", item.amount);
        bonusObj.gameObject.SetActive(item.unlock);
    }


    private void SetSpr(KOBRewardInfo item)
    {        
        if (item.GetRewardType() == KOBRewardType.Card)
        {
            KOBManager.Resource.LoadBallerPortrait(icon, item.pindex);
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 140);
        }
        else
        {
            if(item.reward == KOBReward.Energy) icon.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_Energy_Green");
            else if (item.reward == KOBReward.Gem || item.reward == KOBReward.Gem_Free) icon.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ImageIcon_Gem01_l");
            else if (item.reward == KOBReward.Gold) icon.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ImageIcon_Glod01_l");
            icon.SetNativeSize();
        }
    }
}
