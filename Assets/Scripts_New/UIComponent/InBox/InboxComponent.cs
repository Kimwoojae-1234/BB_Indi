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


    [SerializeField] private GameObject btnClaim; //�����ִ� ��ư
    [SerializeField] private Image btnIcon;
    [SerializeField] private GameObject btnOk; //���� ���� ��ư


    int idx = -1;

        
    public void Init(PostData postData)
    {
        //Portrait //����
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
            //�뺸��            
            btnClaim.gameObject.SetActive(false);
            btnOk.gameObject.SetActive(true);
        }
        else
        {
            //����            
            setRewardUI(postData.postReward);
            btnClaim.gameObject.SetActive(true);
            btnIcon.gameObject.SetActive(true); //���� ����
            btnOk.gameObject.SetActive(false);
        }
    }

    private void setRewardUI(List<AdminPostReward> postReward)
    {
        if (postReward.Count >= 2)
        {
            //���ڷ� ǥ��            
            rewardObj.SetActive(true);
            rewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.RandomBox, "itembox1");
            rewardImg.GetComponent<RectTransform>().sizeDelta = new Vector2(185, 185);
            UILocalization.Set(rewardAmount, "ui.5d27af0eb9a7");
        }
        else
        {
#if false //���� �ٽ� �����
            if (postReward[0].reward == KOBReward.Card)
            {
                cardObj.SetActive(true);
                KOBManager.Resource.LoadBallerPortrait(cardImg, postReward[0].pindex);
                UILocalization.Set(cardAmount, "Arena.Ads.Point", postReward[0].amount);
            }
            else
#endif
            {
                rewardObj.SetActive(true);
                //������ ǥ��
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
                UILocalization.Set(rewardAmount, "Arena.Ads.Point", postReward[0].amount);
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
                //���� �˾�
                Debug.Log("�����˾� ���´�");
                rewardSetting(data);
            }
            else
            {
                //���⼭ ������
                Debug.Log("�׳ɳ�");
            }            
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            KOBManager.Popup.GetPopup<Popup_Inbox>()?.SetClaimAllBtn(KOBManager.Backend.PostList.Count);
            Destroy(gameObject);
        });
    }

    private void rewardSetting(PostData data)
    {
        //���� �̺κ� ���������� ����
        KOBManager.FrontUI.OpenPopup<FrontUI_NetworkLoading>();
        List<AdminPostReward> list = new List<AdminPostReward>();
        for (int i = 0; i < data.postReward.Count; i++)
        {
            list.Add(data.postReward[i]);
        }

        //�켱 �ӽ� - ���� �����Ը� �Ҽ� �ִ°ɷ�
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
