using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BackEnd;
using System.Security.Cryptography;

public class TR_RewardComp : MonoBehaviour
{
    

    [SerializeField] private GameObject[] item;
    //0코인
    //1젬
    //2에너지
    //3뱃
    //4카드
    //5박스
    //6스킬
    //7기어

    [SerializeField] private TextMeshProUGUI txtMount; //센터정렬
    [SerializeField] private TextMeshProUGUI txtMount2; //오른쪽 정렬
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private GameObject Checker;
    [SerializeField] private Image GetAvailble;

    public enum TR_Type
    {
        TrophyRoad,
        BallerReputation,
        RttsReward
    }


    public enum TrophyRewartState
    {
        NotAvailable, //먹을수 없음
        Available,    //먹을수 있음
        Acquired      //획득함  
    }

    private TR_Type trType;

    public int reward_refID { get; private set; } //이걸로 보상설정
    public int reward_pindex { get; private set; } //이걸로 보상설정
    private int trophyKey;
    private TrophyRewartState State;
    //private TrophyRoad Data;
    private KOBRewardInfo rewardInfo;
    private int trophy;
    private bool isTouched = false;

    public void Init(TrophyRoad data, int key, Transform par, TrophyRewartState state)
    {
        trType = TR_Type.TrophyRoad;
        reward_refID = data.reward_index;
        reward_pindex = data.pindex;
        trophyKey = key;
        rewardInfo = new KOBRewardInfo(data.reward_index, data.pindex, KOBRewardFrom.TrophyRoad);
        trophy = data.trophy;
        init(par, state);

    }

    public void Init2(BallerTrophyRoad data, int key, Transform par, TrophyRewartState state)
    {
        trType = TR_Type.BallerReputation;
        reward_refID = data.reward_index;
        reward_pindex = data.pindex;
        trophyKey = key;
        rewardInfo = new KOBRewardInfo(data.reward_index, data.pindex, KOBRewardFrom.BallerTrophyRoad);
        trophy = data.trophy;
        init(par, state);
    }

    public void Init3(RttsTrophyRoad data, int key, Transform par, TrophyRewartState state)
    {
        rewardInfo = data.rewardInfo;
        trophy = data.wins;
        init(par, state);
    }


    private void init(Transform par, TrophyRewartState state)
    {
        isTouched = false;
        for (int i = 0; i < item.Length; i++) item[i].gameObject.SetActive(false);
        txtMount.gameObject.SetActive(false);
        txtMount2.gameObject.SetActive(false);
        Checker.gameObject.SetActive(false);
        transform.parent = par;
        transform.localScale = Vector3.one;

        State = state;        
        KOBReward reward = rewardInfo.reward;

        KOBRewardType rewardType = rewardInfo.GetRewardType();

        if (rewardType == KOBRewardType.Card)
        {
            setCard();
        }
        else if (rewardType == KOBRewardType.Box)
        {
            setBox(reward);
        }
        else
        {
            if (reward == KOBReward.Gold)
            {
                setGold();
            }
            else if (reward == KOBReward.Gem ||
                     reward == KOBReward.Gem_Free)
            {
                setGem();
            }
            else if (reward == KOBReward.Energy)
            {
                setEnergy();
            }
            else if (reward == KOBReward.Bat)
            {
                setBat();
            }
            else //기타 -> 트로피 로드에서는 안쓰일듯
            {
                if (reward == KOBReward.Skill)
                {
                    setSkill();
                }
                else if (reward == KOBReward.Gear)
                {
                    setGear();
                }
            }
        }

        //획득 가능 여부 - 상태 세팅
        setState();
    }


    private void setState()
    {
        DOTweenAnimation anim = GetAvailble.gameObject.GetComponent<DOTweenAnimation>();
        Button button = transform.GetComponent<Button>();
        button.interactable = true;
        if (State == TrophyRewartState.Available)
        {
            GetAvailble.enabled = true;
            anim.enabled = true;
        }
        else
        {
            GetAvailble.enabled = false;
            anim.enabled = false;
            anim.DOPause();
            GetAvailble.transform.localScale = Vector3.one;
            if (State == TrophyRewartState.Acquired)
            {
                Checker.gameObject.SetActive(true);
                button.interactable = false;
            }
        }
    }


    //0코인
    //1젬
    //2에너지
    //3뱃
    //4카드
    //5박스
    //6스킬
    //7기어
    private void setGold()
    {
        //10/50/100/300/1000
        item[0].gameObject.SetActive(true);
        txtName.text = "Gold";
        txtMount.gameObject.SetActive(true);
        txtMount.text = rewardInfo.amount.ToString();
        Image goldImage = item[0].GetComponent<Image>();
        if (goldImage != null)
        {
            goldImage.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, KOBUtil.GetGoldIconSprite(rewardInfo.amount));
            goldImage.SetNativeSize();
        }
    }
    private void setGem()
    {
        item[1].gameObject.SetActive(true);
        txtName.text = "Gem";
        txtMount.gameObject.SetActive(true);
        txtMount.text = rewardInfo.amount.ToString();
        Image gemImage = item[1].GetComponent<Image>();
        if (gemImage != null)
        {
            gemImage.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, KOBUtil.GetGemIconSprite(rewardInfo.amount));
            gemImage.SetNativeSize();
        }
    }

    private void setEnergy()
    {
        item[2].gameObject.SetActive(true);
        txtName.text = "Energy";
        txtMount.gameObject.SetActive(true);
        txtMount.text = rewardInfo.amount.ToString();
    }

    private void setBat()
    {
        item[3].gameObject.SetActive(true);
        txtName.text =  string.Format("Bat{0}", rewardInfo.pindex);
        txtMount2.gameObject.SetActive(true);
        txtMount2.text = string.Format("X{0}", rewardInfo.amount);
    }

    private void setCard()
    {
        item[4].gameObject.SetActive(true);
        int pIndex = rewardInfo.pindex;
        if (pIndex == KOBConstant.BALLPER_YOUPLAY) pIndex = KOBManager.MyInfo.UISelectedBaller; // UI_Baller관련 됨 (명성트로피로드 보상이기때문)

        CharacterData ballerData = KOBManager.Backend.Chart.CharacterData.GetData(pIndex); //고정정보 - 선수고유정보

        Image Portrait = item[4].transform.Find("Item").GetComponent<Image>();
        if (ballerData != null) 
        {
            txtName.text = ballerData.name_id;// KOBManager.Localization.GetUILocalizedValue2(ballerData.name_id);
            KOBManager.Resource.LoadBallerPortrait(Portrait, ballerData.char_idx);
        }
        else
        {
            txtName.text = KOBTextUtil.GetCardType(rewardInfo.reward);
            KOBManager.Resource.LoadBallerPortrait(Portrait, pIndex);
        }
        txtMount2.gameObject.SetActive(true);
        txtMount2.text = string.Format("X{0}", rewardInfo.amount);
    }

    private void setPCard()
    {
        item[4].gameObject.SetActive(true);
    }

    private void setBox(KOBReward rarity)
    {
        item[5].gameObject.SetActive(true);
        Image spr = item[5].GetComponent<Image>();
        if (spr != null)
        {
            spr.sprite = KOBManager.Atlas.GetRewarBox(rarity);
            txtName.text = "Box";
            spr.SetNativeSize();
        }
    }

    private void setSkill()
    {
        item[6].gameObject.SetActive(true);
    }

    private void setGear()
    {
        item[7].gameObject.SetActive(true);
    }


    public void OnClickTouch()
    {
        if (isTouched == true) return;
        isTouched = true;
        Debug.Log("OnClickTouch");
        if(State == TrophyRewartState.NotAvailable)
        {
            KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(string.Format("Requires {0} trophies to collect this reward", trophy));
            isTouched = false;
        }
        else if (State == TrophyRewartState.Available)
        {
            if (trType == TR_Type.TrophyRoad)
            {
                collectTrophyRoadReward();
            }
            else if(trType == TR_Type.BallerReputation)
            {
                collectBallerReward();
            }
            else
            {
                isTouched = false;
            }
        }
        else if (State == TrophyRewartState.Acquired)
        {
            KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init("This reward has already been obtained.");//
            isTouched = false;
        }
    }


    private void collectTrophyRoadReward()
    {
        
        List<RewardData> rewardData = KOBManager.Backend.Chart.RewardData.GetRewards(reward_refID);
        if (rewardData?.Count > 0)
        {
            TRequestTrophyRoadReward req = new TRequestTrophyRoadReward()
            {
                RewardList = rewardData,
                pIndex = reward_pindex,
                TrophyRoadIdx = trophyKey,
            };


            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                TResultTrophyRoadReward res = (TResultTrophyRoadReward)response;
                if (callback?.IsSuccess() == true)
                {                    
                    if (res != null)
                    {
                        Intent it = new Intent();
                        it["RewardList"] = res.RewardList;
                        it["isBox"] = (rewardData.Count > 1 ? true : false);
                        KOBManager.Popup.OpenPopup<Popup_RewardGet>().Set(it);
                        State = TrophyRewartState.Acquired;
                        setState();
                        isTouched = false;
                    }
                }
                else
                {
                    int ErrorCode = res.ErrorCode;
                    Debug.Log("에러코드 : " + ErrorCode);
                }
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            });
        }
    }

    private void collectBallerReward()
    {
        List<RewardData> rewardData = KOBManager.Backend.Chart.RewardData.GetRewards(reward_refID);
        if (rewardData?.Count > 0)
        {
            TRequestBallerTrophyRoadReward req = new TRequestBallerTrophyRoadReward()
            {
                RewardList = rewardData,
                pIndex = reward_pindex,
                TrophyRoadIdx = trophyKey,
                CharIdx = KOBManager.MyInfo.UISelectedBaller // UI_Baller관련 됨 (명성트로피로드 보상이기때문)
            };


            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                TResultBallerTrophyRoadReward res = (TResultBallerTrophyRoadReward)response;
                if (callback?.IsSuccess() == true)
                {
                    if (res != null)
                    {
                        Intent it = new Intent();
                        it["RewardList"] = res.RewardList;
                        it["isBox"] = (rewardData.Count > 1 ? true : false);
                        KOBManager.Popup.OpenPopup<Popup_RewardGet>().Set(it);
                        State = TrophyRewartState.Acquired;
                        setState();
                        isTouched = false;
                    }
                }
                else
                {
                    int ErrorCode = res.ErrorCode;
                    Debug.Log("에러코드 : " + ErrorCode);
                }
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            });
        }
    }
}
