using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Spine.Unity;
using System;

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
    private Tween unlockScaleTween;
    private Tween unlockColorTween;

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
        KillUnlockTweens();
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
        L10n.SetText(Title, "UI.Label.Box.Uppercase"); //임시
        canvasGroup.alpha = 1;
        bActive = true;
    }

    private void SetRewardListFinal()
    {
        for (int i = 0; i < Obj.Length; i++) Obj[i].SetActive(false);

        foreach (Transform child in content.transform)
        {
            //Destroy는 프레임 종료 시 처리되므로 먼저 숨겨 레이아웃에 중복 반영되지 않게 합니다.
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        if (Clone != null && _multiRewardList != null)
        {
            for (int i = 0; i < _multiRewardList.Count; i++)
            {
                GameObject rewardObject = KOBManager.Resource.LoadClone(
                    Clone.gameObject,
                    Vector3.zero,
                    Vector3.one,
                    content.transform);
                if (rewardObject == null) continue;

                rewardObject.name = string.Format("Reward_Item_small_{0}", i);
                rewardObject.SetActive(true);
                rewardObject.GetComponent<Reward_Item_small>()?.InitItem(_multiRewardList[i]);
            }
        }
        
        Title.gameObject.SetActive(false);
        State = RewardGetState.RewardListFinal;
        Obj[4].gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(Obj[4].GetComponent<RectTransform>());
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
            CardAmount.text = L10n.F("Common.Format.ItemQuantityUppercase", _rewardInfo.amount);
            Card.Init(_rewardInfo, 1.0f);
            canvasGroup.alpha = 1;
            DotTweenUtil.Restart(Card.gameObject);
            bActive = true;
        }
    }


    private void UnlockCard()
    {
        KillUnlockTweens();

        if(baller != null)
        {
            Destroy(baller.gameObject);
            baller = null;  
        }

        Title.gameObject.SetActive(false);
        Obj[2].gameObject.SetActive(true);

        int idx = _rewardInfo.pindex;
        string resourcePath = "Ballers/baller" + idx;
        GameObject ballerPrefab = Resources.Load<GameObject>(resourcePath);
        if (ballerPrefab == null)
        {
            Debug.LogError($"[Popup_RewardGet] 해금 캐릭터 프리팹을 찾을 수 없습니다. Resources/{resourcePath}");
            canvasGroup.alpha = 1;
            bActive = true;
            return;
        }

        baller = Instantiate(ballerPrefab, Pos.transform, false);
        baller.transform.localPosition = Vector3.zero;
        baller.transform.localScale = Vector3.one;
        canvasGroup.alpha = 1;

        Action<Color> setRevealColor = CreateRevealColorSetter(baller);
        if (setRevealColor != null)
        {
            setRevealColor(Color.black);
        }
        else
        {
            Debug.LogWarning($"[Popup_RewardGet] baller{idx}에서 표시 가능한 캐릭터 렌더러를 찾지 못했습니다.");
        }

        DotTweenUtil.Restart(Pos.gameObject);
        Obj[2].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        unlockScaleTween = Obj[2].transform.DOScale(Vector3.one, 1.5f).OnComplete(() =>
        {
            Pos.GetComponent<DOTweenAnimation>()?.DOKill();
            Pos.transform.localEulerAngles = Vector3.zero;
            if (setRevealColor == null)
            {
                bActive = true;
            }
        });

        if (setRevealColor == null)
        {
            return;
        }

        float colorValue = 0f;
        unlockColorTween = DOTween.To(() => colorValue, value => colorValue = value, 1f, 0.3f)
            .SetDelay(1.5f)
            .OnUpdate(() =>
            {
                if (baller != null)
                {
                    setRevealColor(new Color(colorValue, colorValue, colorValue, 1f));
                }
            })
            .OnComplete(() => bActive = true);
    }

    private Action<Color> CreateRevealColorSetter(GameObject ballerObject)
    {
        VideoCharacterPlayer videoPlayer = ballerObject.GetComponentInChildren<VideoCharacterPlayer>(true);
        if (videoPlayer != null && videoPlayer.gameObject.activeInHierarchy)
        {
            videoPlayer.Replay();
            return color => videoPlayer.TintColor = color;
        }

        SkeletonGraphic skeletonGraphic = ballerObject.GetComponentInChildren<SkeletonGraphic>(true);
        if (skeletonGraphic != null && skeletonGraphic.gameObject.activeInHierarchy)
        {
            skeletonGraphic.Initialize(false);
            return color => skeletonGraphic.color = color;
        }

        Image[] images = ballerObject.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].gameObject.name == "Image")
            {
                Image image = images[i];
                image.gameObject.SetActive(true);
                return color => image.color = color;
            }
        }

        return null;
    }

    private void KillUnlockTweens()
    {
        unlockScaleTween?.Kill();
        unlockColorTween?.Kill();
        unlockScaleTween = null;
        unlockColorTween = null;
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
