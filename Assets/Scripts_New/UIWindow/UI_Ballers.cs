using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using BackEnd;

public class UI_Ballers : UIWindow
{
    enum BallerUIStae
    {
        Normal,
        BallerTrophy
    }

    [Header("[볼러 기본 오브젝트]")]
    //[SerializeField] private GameObject BallerMain;
    [SerializeField] private GameObject origin;
    [SerializeField] private BallerInfoComponent2 BallerInfo;
    [SerializeField] private BallerPowerComponent BallerPower;
    [SerializeField] private BallerSkillComponent BallerSkill;
    [SerializeField] private BallerGearComponent BallerGear;
    [SerializeField] private GameObject BallerSelect;

    [Header("[볼러 트로피]")]
    [SerializeField] private GameObject BallerTrophy;

    [Header("[재화]")]
    [SerializeField] private LobbyPropertyComponent Property;
    [SerializeField] private GameObject [] Buttons;

    [Header("[버튼]")]
    [SerializeField] private GameObject SelectBtn;
    [SerializeField] private GameObject PracticeBtn;

    [Header("[업그레이드 관련]")]
    [SerializeField] private SkeletonGraphic UpgradeEffect;

    private BallerUIStae State;
    private int SelectIdx = -1;
    private CardBaller.CardBallerState CardState;
    private GameObject ballerObj = null;

    public override void Initialize()
    {
        base.Initialize();
        //Property.InitProperty(typeof(UI_Ballers));
        SelectIdx = -1;
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        Property.InitProperty(typeof(UI_Ballers));
        State = BallerUIStae.Normal;
        DeActiveBallerTrophy();
        BallerInfo.GetComponent<CanvasGroup>().alpha = 1;
        BallerSelect.GetComponent<CanvasGroup>().alpha = 1;
        InitUpgrade();
    }

    public override void ClickBackButton()
    {        
        if(State == BallerUIStae.BallerTrophy)
        {
            State = BallerUIStae.Normal;            
            BallerTrophy.GetComponent<BallerTrophyComponent>().Close();

            BallerInfo.GetComponent<CanvasGroup>().DOFade(1, 0.3f).SetDelay(0.2f);
            BallerSelect.GetComponent<CanvasGroup>().DOFade(1, 0.3f).SetDelay(0.2f);

            Invoke("DeActiveBallerTrophy", 0.5f);
        }
        else
        {
            base.ClickBackButton();
        }
    }


    public void LoadBaller(int idx, CardBaller.CardBallerState state)
    {
        KOBManager.MyInfo.SetUISelectedBaller(idx); //볼러 UI진입시 UISelected 바꿔줌

        CardState = state;
        if (SelectIdx != idx)
        {
            SelectIdx = idx;

            if (ballerObj != null)
            {
                Destroy(ballerObj.gameObject);
            }
            else
            {
                foreach (Transform child in origin.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            //캐릭터 로딩
            ballerObj = KOBManager.Resource.LoadGameObject("Ballers", "baller" + idx, origin.transform);

            UpdateInfo(idx);
        }
        else
        {
            if(ballerObj != null)
            {
                ballerObj.transform.parent = origin.transform;
                ballerObj.transform.localPosition = Vector3.zero;
            }
        }
    }


    public void DicrectTropyroadOpen(int idx)
    {
        LoadBaller(idx, CardBaller.CardBallerState.Collection);
        Invoke("OpenBallerTrophyUI", 0.5f);
    }




    public void UpdateInfo(int _idx)
    {        
        CharacterData ballerData = KOBManager.Backend.Chart.CharacterData.GetData(_idx); //고정정보 - 선수고유정보
        BallerInfo.ShowBallerInfo(ballerData);

        if (CardState == CardBaller.CardBallerState.Collection)
        {
            KOBBaller ballerInfo = KOBManager.MyInfo.GameData.PlayerInfo.BallerList[_idx]; //변동정보 - 유저가 성장
            BallerInfo.ShowTrophyInfo(ballerData, ballerInfo);
            BallerSkill.InitForCollection(ballerData, ballerInfo);
            BallerGear.InitForCollection(ballerData, ballerInfo);
            BallerPower.CollectionSetting(ballerData, ballerInfo);
            SelectBtn.gameObject.SetActive(true);
        }
        else
        {
            BallerInfo.HideTrophyInfo();
            BallerSkill.InitForLocked(ballerData);
            BallerGear.InitForLocked(ballerData);
            BallerPower.UnlockSetting(ballerData);
            SelectBtn.gameObject.SetActive(false);
            if (CardState == CardBaller.CardBallerState.Unlocking)
            {

            }
            else if (CardState == CardBaller.CardBallerState.Locked)
            {

            }
        }
    }


    public void OnClickSelect()
    {
        if(State == BallerUIStae.Normal)
        {
            //KOBManager.Backend.GameData.KOBGameData.SetBaller(SelectIdx);
            TRequestSelectBaller req = new TRequestSelectBaller()
            {
                CardIdx = SelectIdx
            };

            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                TResultSelectBaller res = (TResultSelectBaller)response;
                if (callback?.IsSuccess() == true && res?.isSuccess == true)
                {
                    KOBManager.UI.OpenWindow<UI_LobbyRe>().loadBaller();
                    KOBManager.MyInfo.SetUISelectedBaller(SelectIdx);  //볼러 UI진입시 UISelected 바꿔줌
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

    public void OnClickPractice()
    {
        if (State == BallerUIStae.Normal)
        {

        }
    }

    public void OnClickBallerTrophyUI()
    {
        if (State == BallerUIStae.Normal && CardState == CardBaller.CardBallerState.Collection)
        {
            Debug.Log("OnClickBallerTrophyUI");            
            OpenBallerTrophyUI();
        }
    }

    public void OpenSkillPopup(int slot)
    {
        if (State == BallerUIStae.Normal)
        {
            Popup_SkillSetting popup = KOBManager.Popup.OpenPopup<Popup_SkillSetting>();
            popup.Setting(ballerObj, SelectIdx, slot, BackToUI);
        }
    }

    public void OpenGearPopup(int slot)
    {
        if (State == BallerUIStae.Normal)
        {
            Popup_GearSetting popup = KOBManager.Popup.OpenPopup<Popup_GearSetting>();
            popup.Setting(ballerObj, SelectIdx, slot, BackToUI);
        }
    }

    private void BackToUI()
    {
        ballerObj.transform.parent = origin.transform;
        ballerObj.transform.localScale = Vector3.one;
        ballerObj.transform.localPosition = Vector3.zero;
    }


    private void OpenBallerTrophyUI()
    {
        State = BallerUIStae.BallerTrophy;
        BallerInfo.GetComponent<CanvasGroup>().DOFade(0, 0.2f);
        BallerSelect.GetComponent<CanvasGroup>().DOFade(0, 0.2f);
        //BallerTrophy.gameObject.SetActive(true);
        ActiveBallerTrophy();
        BallerTrophy.GetComponent<BallerTrophyComponent>().Open(SelectIdx, ballerObj,origin);        
    }


    private void ActiveBallerTrophy()
    {
        BallerTrophy.gameObject.SetActive(true);
        for (int i = 0; i < Buttons.Length; i++) Buttons[i].gameObject.SetActive(false);
    }

    private void DeActiveBallerTrophy()
    {
        BallerTrophy.gameObject.SetActive(false);
        for (int i = 0; i < Buttons.Length; i++) Buttons[i].gameObject.SetActive(true);
    }


    public void BallerUpgradeAction(int idx)
    {
        SelectIdx = idx;        
        UpdateInfo(idx);
        Property.InitProperty(typeof(UI_Ballers));
        Debug.Log("업데이트 되는 연출을 넣어주면 됨");
        StartCoroutine(UpgradeProcess());
    }

    private void InitUpgrade()
    {
        UpgradeEffect.gameObject.SetActive(false);
    }

    private IEnumerator UpgradeProcess()
    {
        UpgradeEffect.gameObject.SetActive(true);
        UpgradeEffect.AnimationState.SetAnimation(0, "etc1", false);
        yield return new WaitForSeconds(1.667f);

        //TODO
        //슬롯 오픈 체크
        //레벨 업 연출 보여주기

        InitUpgrade();
    }
}
