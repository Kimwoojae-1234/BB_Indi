using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System;

public class InboxComponent : MonoBehaviour
{
    [SerializeField] private Image Portrait;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private TextMeshProUGUI remainTime;

    [SerializeField] private GameObject rewardObj;
    [SerializeField] private Image rewardImg;
    [SerializeField] private TextMeshProUGUI rewardAmount;

    [SerializeField] private GameObject cardObj;
    [SerializeField] private Image cardImg;
    [SerializeField] private TextMeshProUGUI cardAmount;


    [SerializeField] private GameObject btnClaim; //보상있는 버튼
    [SerializeField] private Image btnIcon;
    [SerializeField] private GameObject btnOk; //보상 없는 버튼


    int idx = -1;

        
    public void Init(PostData postData)
    {
        //Portrait //추후
        idx = postData.idx;
        gameObject.name = string.Format("Post_idx{0}", idx);
        title.text = postData.title.ToUpper();
        content.text = postData.content;
        //remainTime.text = string.Format("Remain : {0}", TimeUtil.RemainTime(postData.expirationDate)); 

        rewardObj.SetActive(false);
        cardObj.SetActive(false);

        if (postData.isCanReceive == false ||
           postData.postReward.Count == 0)
        {
            //노보상            
            btnClaim.gameObject.SetActive(false);
            btnOk.gameObject.SetActive(true);
        }
        else
        {
            //보상            
            setRewardUI(postData.postReward);
            btnClaim.gameObject.SetActive(true);
            btnIcon.gameObject.SetActive(true); //추후 광고
            btnOk.gameObject.SetActive(false);
        }
    }

    private void setRewardUI(List<AdminPostReward> postReward)
    {
        if (postReward.Count >= 2)
        {
            //상자로 표시            
            rewardObj.SetActive(true);
            rewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.RandomBox, "itembox1");
            rewardImg.GetComponent<RectTransform>().sizeDelta = new Vector2(185, 185);
            rewardAmount.text = "BOX";
        }
        else
        {
#if false //추후 다시 만들것
            if (postReward[0].reward == KOBReward.Card)
            {
                cardObj.SetActive(true);
                KOBManager.Resource.LoadBallerPortrait(cardImg, postReward[0].pindex);
                cardAmount.text = string.Format("+{0}", postReward[0].amount);
            }
            else
#endif
            {
                rewardObj.SetActive(true);
                //아이콘 표시
                if (postReward[0].reward == KOBReward.Gold)
                {
                    rewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gold0");
                }
                else if (postReward[0].reward == KOBReward.Gem || postReward[0].reward == KOBReward.Gem_Free)
                {
                    rewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gem0");
                }
                else if (postReward[0].reward == KOBReward.Energy)
                {
                    rewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_Energy_Green");
                }
                rewardImg.SetNativeSize();
                rewardAmount.text = string.Format("+{0}", postReward[0].amount);
            }
        }
    }


    public void OnClickClaim()
    {
        KOBManager.FrontUI.OpenPopup<FrontUI_NetworkLoading>();
        KOBManager.Backend.PostReceive(BackEnd.PostType.Admin, idx, (obj) =>
        {            
            PostData data = (PostData)obj;
            if(data.isCanReceive == true)
            {
                //보상 팝업
                Debug.Log("보상팝업 나온다");
                rewardSetting(data);
            }
            else
            {
                //여기서 마무리
                Debug.Log("그냥끝");
            }            
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            KOBManager.Popup.GetPopup<Popup_Inbox>()?.SetClaimAllBtn(KOBManager.Backend.PostList.Count);
            Destroy(gameObject);
        });
    }

    private void rewardSetting(PostData data)
    {
        //현재 이부분 만족스럽지 않음
        KOBManager.FrontUI.OpenPopup<FrontUI_NetworkLoading>();
        List<AdminPostReward> list = new List<AdminPostReward>();
        for (int i = 0; i < data.postReward.Count; i++)
        {
            list.Add(data.postReward[i]);
        }

        //우선 임시 - 추후 여러게를 할수 있는걸로
        TRequestRewardInfo req = new TRequestRewardInfo()
        {
            Reward = new KOBRewardInfo()
            {
                reward = list[0].reward,
                pindex = list[0].pindex,
                amount = list[0].amount
            }
        };

        KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
        {
            if (callback?.IsSuccess() == true)
            {
                TResultRewardInfo res = (TResultRewardInfo)response;
                if (res != null)
                {
                    
                    KOBManager.Popup.OpenPopup<Popup_RewardGet>().InitFromPost(list);
                }
            }
            else
            {

            }
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
        });
    }

}
