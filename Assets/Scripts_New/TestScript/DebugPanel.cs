using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BackEnd;
using LitJson;
using System;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI LobbyStepTxt;
    [SerializeField] TMP_InputField input;
    [SerializeField] TMP_InputField input_gift;
    [SerializeField] private TextMeshProUGUI GiftTxt;
    [SerializeField] TMP_InputField input_card;
    [SerializeField] private TextMeshProUGUI CardTxt;
    [SerializeField] TMP_InputField input_box;


    int arrowNum = 0;
    int max = 3;

    int cardNum = 0;
    int maxCard = 4;


    static string[] giftText = new string[3] { "COIN", "GEM", "ENERGY" };
    static int[] cardText = new int[6] { 1001, 1049, 1050, 1051, 1052, 1053 };


    private void Start()
    {
        
        arrowNum = 0;
        max = giftText.Length;
        GiftTxt.text = giftText[arrowNum];

        cardNum = 0;
        maxCard = cardText.Length;
        setName();
    }


    public void OnClickLobbyStepUpdate()
    {
        Debug.Log("OnClickLobbyStepUpdate");
        /*KOBManager.DummyNetwork.SendPacket(new TRequestLobbyStepUpdate(), (BackendReturnObject callback, TResponseBase response) =>
        {
            if (callback?.IsSuccess() == true)
            {
                //Debug.Log("테스트테스트 a = " + a);
                LobbyStepTxt.text = string.Format("LobbyStep: {0}", KOBManager.MyInfo.UserInfo.LobbyStep);
                //TResultLobbyStepUpdate respose = (TResultLobbyStepUpdate)response;                
            }
            else
            {

            }
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
        });*/
    }



    public void OnClickFindUser()
    {
        
    }


    public void OnClickGift()
    {
        string str = input_gift.text;
        int _amount = int.Parse(str);       
        KOBReward _reward = KOBReward.Gold;
        if(arrowNum == 0) _reward = KOBReward.Gold;
        else if (arrowNum == 1) _reward = KOBReward.Gem;
        else if (arrowNum == 2) _reward = KOBReward.Energy;
        Debug.Log("Amount : " + _amount + "  Reward : " + _reward);

        TRequestRewardInfo req = new TRequestRewardInfo()
        {
            Reward = new KOBRewardInfo()
            {
                reward = _reward,
                amount = _amount,
            }
        };

        KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
        {
            if (callback?.IsSuccess() == true)
            {
                TResultRewardInfo res = (TResultRewardInfo)response;
                if (res != null)
                {
                    Intent it = new Intent();                    
                    it["Reward"] = res.Reward;
                    KOBManager.Popup.OpenPopup<Popup_RewardGet>().Set(it);
                }
            }
            else
            {

            }
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
        });
    }


    public void OnClickPrev()
    {
        arrowNum--;
        if (arrowNum < 0) arrowNum = max - 1;
        GiftTxt.text = giftText[arrowNum];
    }

    public void OnClickNext()
    {
        arrowNum++;
        if(arrowNum >= max) arrowNum = 0;
        GiftTxt.text = giftText[arrowNum];
    }

    public void OnClickCard()
    {
        string str = input_card.text;
        int _amount = int.Parse(str);
        KOBReward _reward = KOBReward.Card_Common;
        int _pindex = cardText[cardNum];
        Debug.Log("PIndex : " + _pindex + "Amount : " + _amount + "  Reward : " + _reward);

        TRequestRewardInfo req = new TRequestRewardInfo()
        {
            Reward = new KOBRewardInfo()
            {
                reward = _reward,
                amount = _amount,
                pindex = _pindex
            }
        };

        KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
        {
            if (callback?.IsSuccess() == true)
            {
                TResultRewardInfo res = (TResultRewardInfo)response;
                if (res != null)
                {
                    Intent it = new Intent();
                    it["Reward"] = res.Reward;
                    KOBManager.Popup.OpenPopup<Popup_RewardGet>().Set(it);
                }
            }
            else
            {

            }
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
        });
    }


    public void OnClickCardPrev()
    {
        cardNum--;
        if (cardNum < 0) cardNum = maxCard - 1;
        setName();
    }

    public void OnClickCardNext()
    {
        cardNum++;
        if (cardNum >= maxCard) cardNum = 0;
        setName();
    }

    private void setName()
    {
        int idx = cardText[cardNum];
        CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(idx);
        CardTxt.text = KOBManager.Localization.GetUILocalizedValue2(data.name_id);
    }


    public void OnClickBox()
    {
        string str = input_box.text;
        int idx = int.Parse(str);
        Debug.Log("idx = " + idx);

        List<RewardData> rewardData = KOBManager.Backend.Chart.RewardData.GetRewards(idx);

        if (rewardData?.Count > 0)
        {
            TRequestRewardListInfo req = new TRequestRewardListInfo()
            {
                RewardList = rewardData
            };


            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                if (callback?.IsSuccess() == true)
                {
                    TResultRewardListInfo res = (TResultRewardListInfo)response;
                    if (res != null)
                    {
                        Intent it = new Intent();
                        it["RewardList"] = res.RewardList;
                        it["isBox"] = true;
                        KOBManager.Popup.OpenPopup<Popup_RewardGet>().Set(it);
                    }
                }
                else
                {

                }
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            });
        }
    }
}
