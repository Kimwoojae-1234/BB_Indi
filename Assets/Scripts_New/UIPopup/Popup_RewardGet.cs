using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Spine.Unity;

public class Popup_RewardGet : UIPopup
{
    enum RewardGetState
    {
        None,
        Reward,
        RewardList,
        RewardListFinal,
    }

    [SerializeField] private TextMeshProUGUI Title;
    [SerializeField] private GameObject[] Obj;
    [Header("박스")]
    [SerializeField] private Image BoxImg;
    
    [Header("재화류")]
    [SerializeField] private TextMeshProUGUI CurrencyAmount;
    [SerializeField] private Image RewardImg;
    
    [Header("선수카드")]
    [SerializeField] private TextMeshProUGUI CardAmount;
    [SerializeField] private CardReward Card;
    
    [Header("선수언락")]
    [SerializeField] private GameObject Pos;
    private GameObject baller;

    [Header("박스오픈 최종결과")]
    [SerializeField] private Reward_Item_small Clone;
    [SerializeField] private Image BoxImg2; //박스 아닌경우 비활성화
    [SerializeField] private RectTransform content;

    [SerializeField] private CanvasGroup canvasGroup;

    bool bActive = false;
    RewardGetState State = RewardGetState.None;
    KOBRewardInfo _rewardInfo; //보상 정보 버퍼
    List<KOBRewardInfo> _multiRewardList = null; //여러개 보상시 리스트
    int CurCount = 0;   //리스트의 현재 카운트

    public override void Open()
    {
        base.Open();
    }


    public override void Close()
    {
        bActive = false;
        State = RewardGetState.None;
        _rewardInfo = null;
        _multiRewardList = null;

        canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            base.Close();            
        });        
    }



    public override void Set(Intent it = null)
    {
        base.Set(it);

        if (it.Contains("Reward") == true) //보상이 단품으로
        {
            KOBRewardInfo rewardInfo = it["Reward"] as KOBRewardInfo;

            bActive = false;
            canvasGroup.alpha = 0;
            State = RewardGetState.Reward;

            SetNextRewardItem(rewardInfo);
        }
        else if (it.Contains("RewardList") == true) //보상이 리스트로
        {
            List<KOBRewardInfo> rewardList = it["RewardList"] as List<KOBRewardInfo>;
            bool isBox = false;// (bool)it["isBox"];
            if (it.Contains("isBox")) isBox = (bool)it["isBox"]; //박스연출 여부

            if (rewardList?.Count > 0)
            {
                bActive = false;
                BoxImg2.gameObject.SetActive(isBox);
                canvasGroup.alpha = 0;
                State = RewardGetState.RewardList;
                CurCount = 0;

                _multiRewardList = rewardList;

                if (isBox == true)
                {
                    SetBox();
                }
                else
                {
                    SetNextRewardItem(rewardList[0]);
                    CurCount++;
                }
            }
        }
    }






    private void SetBox()
    {
        for (int i = 0; i < Obj.Length; i++) Obj[i].SetActive(false);
        Obj[3].gameObject.SetActive(true);
        Title.gameObject.SetActive(true);
        Title.text = "BOX"; //임시
        canvasGroup.alpha = 1;
        bActive = true;
    }

    private void SetRewardListFinal()
    {
        for (int i = 0; i < Obj.Length; i++) Obj[i].SetActive(false);
        foreach (Transform child in content.transform) Destroy(child.gameObject);
        for (int i = 0; i < _multiRewardList.Count; i++)
        {
            //GameObject obj = baseballplay.Util.CloneObj(Clone.gameObject, content.transform, Vector3.zero);
            //obj.GetComponent<Reward_Item_small>().InitItem(_multiRewardList[i]);
        }
        
        Title.gameObject.SetActive(false);
        State = RewardGetState.RewardListFinal;
        Obj[4].gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(Obj[4].GetComponent<RectTransform>());
        Title.gameObject.SetActive(false);
        bActive = true;
    }


    private void SetNextRewardItem(KOBRewardInfo rewardInfo)
    {
        for (int i = 0; i < Obj.Length; i++) Obj[i].SetActive(false);
        _rewardInfo = rewardInfo;
        if (_rewardInfo != null)
        {
            Title.text = _rewardInfo.reward.ToString().ToUpper(); //임시
            if (rewardInfo.GetRewardType() == KOBRewardType.Card)
            {
                SetCard();
            }
            else
            {
                SetItem();
            }
        }
    }


    


    /// <summary>
    /// 우편으로부터 초기화
    /// </summary>
    /// <param name="postRewardList"></param>
    public void InitFromPost(List<AdminPostReward> postRewardList)
    {
        if(postRewardList.Count >= 2) //상자혹은 멀티
        {
            List<KOBRewardInfo> infoList = new List<KOBRewardInfo>();
            for(int i = 0;i < postRewardList.Count;i++)
            {
                infoList.Add(new KOBRewardInfo(postRewardList[i]));
            }
            //TODO -> 우편 초기화 다시 할것
            //Init(infoList, true);
        }
        else
        {
            //TODO -> 우편 초기화 다시 할것
            //Init(new KOBRewardInfo(postRewardList[0]));
        }
    }



    private void SetItem()
    {
        Title.gameObject.SetActive(true);   
        Obj[0].gameObject.SetActive(true);          
        CurrencyAmount.text = string.Format("+{0}", _rewardInfo.amount);
        if (_rewardInfo.reward == KOBReward.Gold)
        {
            setgold();
        }
        else if (_rewardInfo.reward == KOBReward.Gem || _rewardInfo.reward == KOBReward.Gem_Free)
        {
            setgem();
        }
        else if (_rewardInfo.reward == KOBReward.Energy)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_Energy_Green");
        }        
        RewardImg.SetNativeSize();
        canvasGroup.alpha = 1;
        DotTweenUtil.Restart(RewardImg.gameObject);
        bActive = true;
    }

    private void setgold()
    {
        int amount = _rewardInfo.amount;
        if(amount < 10)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gold0");
        }
        else if (amount < 50)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gold1");
        }
        else if (amount < 500)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gold2");
        }
        else if (amount < 5000)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gold3");
        }
        else 
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gold4");
        }
    }

    private void setgem()
    {
        int amount = _rewardInfo.amount;
        if (amount < 10)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gem0");
        }
        else if (amount < 50)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gem1");
        }
        else if (amount < 500)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gem2");
        }
        else if (amount < 5000)
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gem3");
        }
        else
        {
            RewardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gem4");
        }
    }



    private void SetCard()
    {
        if(_rewardInfo.unlock == true)
        {
            //언락
            UnlockCard();
        }
        else
        {
            //카드수 증가
            Title.gameObject.SetActive(true);   
            Obj[1].gameObject.SetActive(true);
            CardAmount.text = string.Format("X{0}", _rewardInfo.amount);
            Card.Init(_rewardInfo, 1.0f);
            canvasGroup.alpha = 1;
            DotTweenUtil.Restart(Card.gameObject);
            bActive = true;
        }
    }


    private void UnlockCard()
    {
        if(baller != null)
        {
            Destroy(baller.gameObject);
            baller = null;  
        }

        Title.gameObject.SetActive(false);
        Obj[2].gameObject.SetActive(true);

        int idx = _rewardInfo.pindex;
        baller = KOBManager.Resource.LoadGameObject("Ballers", "baller" + idx, Pos.transform);
        SkeletonGraphic anim = baller.transform.Find("anim").GetComponent<SkeletonGraphic>();
        canvasGroup.alpha = 1;
        anim.color = new Color(0, 0, 0);
        DotTweenUtil.Restart(Pos.gameObject);
        Obj[2].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        // 스케일을 (1, 1, 1)로 3초 동안 변경
        Obj[2].transform.DOScale(new Vector3(1f, 1f, 1f), 1.5f).OnComplete(() =>
        {
            Pos.GetComponent<DOTweenAnimation>().DOKill();
            Pos.transform.localEulerAngles = Vector3.zero;
        });


        float color = 0;
        float targetColor = 1;


        DOTween.To(() => color, x => color = x, targetColor, 0.3f)
               .SetDelay(1.5f)
               .OnUpdate(() => {
                   anim.color = new Color(color, color, color);
               })
               .OnComplete(() => { bActive = true; });
    }





    protected override void Update()
    {
        base.Update();
        if (bActive == true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (State == RewardGetState.Reward || State == RewardGetState.RewardListFinal)
                {
                    Close();
                }
                else if(State == RewardGetState.RewardList)
                {
                    if (CurCount < _multiRewardList.Count)
                    {
                        SetNextRewardItem(_multiRewardList[CurCount]);
                        CurCount++;
                    }
                    else
                    {
                        SetRewardListFinal();
                    }
                }
            }
        }

    }

}
