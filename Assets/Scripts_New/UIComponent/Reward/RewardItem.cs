using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardItem : MonoBehaviour
{
    [SerializeField] private Image RewardIcon;
    [SerializeField] private Image CardObj;
    [SerializeField] private Image CardImage;
    [SerializeField] private Image BoxImage;

    public void SetItem(int reward_index, int pindex, bool isAquire = false) //È¹µæ °¡´É½Ã true
    {
        RewardIcon.gameObject.SetActive(false);
        CardObj.gameObject.SetActive(false);
        BoxImage.gameObject.SetActive(false);

        List<RewardData> list = KOBManager.Backend.Chart.RewardData.GetRewards(reward_index);
        if (list != null && list.Count > 0)
        {
            if (list.Count > 1)
            {
                SetBox(list, isAquire);
            }
            else
            {
                KOBRewardInfo info = new KOBRewardInfo(list[0]);
                if(pindex > 0) info.pindex = pindex;
                KOBRewardType type = info.GetRewardType();

                if (type == KOBRewardType.Card)
                {
                    SetCard(info, isAquire);
                }
                else if (type == KOBRewardType.Box)
                {
                    SetBox(list, isAquire);
                }
                else if (type == KOBRewardType.Currency)
                {
                    SetCurrency(info, isAquire);
                }
            }
        }
        
    }

    private void SetCard(KOBRewardInfo info, bool isAquire)
    {
        CardObj.gameObject.SetActive(info != null);
        if (info != null)
        {
            KOBManager.Resource.LoadBallerPortrait(CardImage, info.pindex);
            SetAquire(CardObj.gameObject, isAquire);
        }
    }

    private void SetBox(List<RewardData> list, bool isAquire)
    {
        BoxImage.gameObject.SetActive(true);
        KOBReward reward = KOBRewardUtil.SetBoxType(list);
        BoxImage.sprite = KOBManager.Atlas.GetRewarBox(reward);
        BoxImage.SetNativeSize();

        SetAquire(BoxImage.gameObject, isAquire);
    }

    private void SetCurrency(KOBRewardInfo info, bool isAquire)
    {
        RewardIcon.gameObject.SetActive(info != null);
        if (info != null)
        {
            KOBManager.Atlas.SetRewardSprite(RewardIcon, info.reward);
            SetAquire(RewardIcon.gameObject, isAquire);
        }


    }

    private void SetAquire(GameObject obj, bool isAquire)
    {
        if(isAquire == true) DotTweenUtil.Restart(obj.gameObject);
        else DotTweenUtil.Stop(obj.gameObject);
    }
}
