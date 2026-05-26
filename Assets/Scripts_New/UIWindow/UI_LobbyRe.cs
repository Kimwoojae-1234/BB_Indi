using BackEnd;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Popup_Promotion;




public class UI_LobbyRe : UIWindow
{

    public enum LastPlay
    {
        None,
        BallerTierUpgrade,
        Rtts
    }
    public static LastPlay lastPlay = LastPlay.None;

    public enum BtnType
    {
        User = 0,
        Trophy,
        DailyGigt,
        Shop,
        Baller,
        Team,
        Pass,
        Mission,
        Ranking,
        League,
        Club,
        PlayBall
    }

    [Header("[기본 오브젝트]")]
    [SerializeField] private GameObject TopLeft;
    [SerializeField] private GameObject TopLeft2;
    [SerializeField] private GameObject SideLeft;
    [SerializeField] private GameObject BottomLeft;    
    [SerializeField] private GameObject SideRight;
    [SerializeField] private GameObject BottomRight;

    [Header("[재화]")]
    [SerializeField] private LobbyPropertyComponent Property;

    [Header("[버튼]")]
    [SerializeField] private LobbyContentButton [] LobbyBtn;          //

    [Header("[볼러]")]
    [SerializeField] private BallerInfoComponent LobbyBaller;

    [Header("[우편]")]
    [SerializeField] private GameObject InBoxObj;


    //로비에서 나갈때 이값을 false로 세팅하면 다시 로비로 돌아올때 Rtts정보를 초기화 한다!!!
    private bool isRttsInit = false;


    public override void Initialize()
    {
        base.Initialize();
                
        LobbyStepSetting();

        //Property.InitProperty(typeof(UI_LobbyRe));
        for (int i = 0; i < LobbyBtn.Length; i++)
        {
            if (LobbyBtn[i].transform.parent.gameObject.activeSelf)
            {
                LobbyBtn[i].InitContent(typeof(UI_LobbyRe));                
            }
        }
        loadBaller();

        //PlayBtn 비활성화
        isRttsInit = false;
    }


    public override void OpenWindow()
    {
        base.OpenWindow();

        //select baller 다시한번 세팅해줘
        int ballerIdx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;
        KOBManager.MyInfo.SetUISelectedBaller(ballerIdx);  // 로비 진입시 안전빵으로 한번더 호출해줘

        Property.InitProperty(typeof(UI_LobbyRe));
        LobbyContentPlayBall playBtn = null;
        for (int i = 0; i < LobbyBtn.Length; i++)
        {
            if (LobbyBtn[i].transform.parent.gameObject.activeSelf)
            {
                LobbyBtn[i].UpdateContent();
                if (LobbyBtn[i].GetComponent<LobbyContentPlayBall>() != null)
                {
                    playBtn = LobbyBtn[i].GetComponent<LobbyContentPlayBall>();
                }
            }
        }

        tutorialSetting();
        KOBManager.Backend.PostListGet(BackEnd.PostType.Admin, UpdateInbox);


        if (isRttsInit == false)
        {
            isRttsInit = true;
            KOBManager.Rtts.RttsEnter((bool isNewLeague) =>
            {                
                //Rtts데이터 초기화 되면 관련 이벤트 체크
                if (playBtn != null) playBtn.SetRttsRewardInfo(); //플레이 버튼 세팅
                LobbyEvent(isNewLeague);
            });
        }

    }



    private void LobbyEvent(bool isNewLeague)
    {
        //계정 티어 업글 연출
        if(KOBManager.Baller.TierUpgradeEvent((TResultTierUpgrade res) => showTierUpgradeEvent(res)) == true)
        {
            //계정 티어 업글시 다음 연출 중단
            UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.None;
            return;
        }

        //선수 명성 업글 연출
        if (KOBManager.Baller.BallerFameUpgradeEvent((TResultBallerFameUpgrade res) => showFameUpgradeEvent(res)) == true)
        {
            //선수 명성 업글시 다음 연출 중단
            UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.None;
            return;
        }

        //이전 결과 화면에서 볼러 티어 업글 한 경우
        if (UI_LobbyRe.lastPlay == UI_LobbyRe.LastPlay.BallerTierUpgrade)
        {
            //이전에 티어 업글 했을 때
            int selected_idx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;
            KOBManager.UI.OpenWindow<UI_Ballers>().DicrectTropyroadOpen(selected_idx);
            UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.None;
            return;
        }

        //RTTS리그가 종료되었는지 여부 -> //리그 종료 이벤트 발생시 해당 팝업 호출
        if (KOBManager.Rtts.RttsLeagueEndEvent((TResultRttsLeagueUpgrade res) => KOBManager.UI.OpenWindow<UI_RTTSResult>().Set(res)) == true)
        {
            //선수 명성 업글시 다음 연출 중단
            UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.None;
            return;
        }

        //뉴리그 체크
        if(CheckRttsNewLeague(isNewLeague) == true)
        {
            Debug.Log("뉴 리그 연출");
            UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.None;
            return;
        }


        if(UI_LobbyRe.lastPlay == UI_LobbyRe.LastPlay.Rtts)
        {
            //이전에 Rtts했을 경우
            UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.None;
            KOBManager.UI.OpenWindow<UI_RTTS>();
        }

    }


    private void showTierUpgradeEvent(TResultTierUpgrade res)
    {
        isRttsInit = false; //로비로 돌아올때 재 연출할수 있도록 세팅
        Intent it = new Intent();
        it["PromotionType"] = PromotionType.Account_Tier;
        it.AddIntentData<UIPopup.OnClickAction>(UIPopup.ON_CLOSE, () => { KOBManager.UI.OpenWindow<UI_TrophyRoad>(); });
        KOBManager.Popup.OpenPopup<Popup_Promotion>().Set(it);
    }

    private void showFameUpgradeEvent(TResultBallerFameUpgrade res)
    {
        isRttsInit = false; //로비로 돌아올때 재 연출할수 있도록 세팅                    
        Intent it = new Intent();
        it["PromotionType"] = PromotionType.Baller_Reputation;
        it.AddIntentData<UIPopup.OnClickAction>(UIPopup.ON_CLOSE, () => { KOBManager.UI.OpenWindow<UI_Ballers>().DicrectTropyroadOpen(res.baller_idx); });
        KOBManager.Popup.OpenPopup<Popup_Promotion>().Set(it);
    }


    /// <summary>
    /// 새로운 리그가 열릴시 이벤트 체크
    /// </summary>
    /// <param name="isNewLeague"></param>
    /// <returns></returns>
    private bool CheckRttsNewLeague(bool isNewLeague)
    {
        int CurLeague = KOBManager.MyInfo.GameData.RttsInfo.League;
        
        if (isNewLeague == true)
        {
            if (CurLeague == 0)
            {
                //게임시작
                Debug.Log("게임 첫시작 이벤트를 여기에");
            }
            else
            {
                //새로운 RTTS시작
                Intent it = new Intent();
                it["PromotionType"] = PromotionType.League_Promotion;
                it.AddIntentData<UIPopup.OnClickAction>(UIPopup.ON_CLOSE, () =>
                {
                    KOBManager.UI.OpenWindow<UI_RTTS>();
                });
                KOBManager.Popup.OpenPopup<Popup_Promotion>().Set(it);
            }
            return true; //로비 연출 중단
        }
        return false;
    }



    public void loadBaller()
    {
        int ballerIdx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;
        KOBManager.MyInfo.SetUISelectedBaller(ballerIdx); //로비 초기화시 세팅
        LobbyBaller.LoadBaller(ballerIdx, TouchBaller);
    }


    public override void PopupClose()
    {
        base.PopupClose();
        Property.UpdateProperty();
        UpdateInbox(null);
    }


    private void LobbyStepSetting()
    {
        /*int lobbyStep = 10;// KOBManager.MyInfo.UserInfo.LobbyStep;
        if(lobbyStep == 2) //최초 로비 진입//LobbyFirstTuto 이거일듯
        {
            TopLeft.SetActive(true);
            TopLeft2.SetActive(false);
            SideLeft.SetActive(false);
            BottomLeft.SetActive(true);
            SideRight.SetActive(false);
            BottomRight.SetActive(false);
        }
        else*/
        {
            TopLeft.SetActive(true);
            TopLeft2.SetActive(true);
            SideLeft.SetActive(true);
            BottomLeft.SetActive(true);
            SideRight.SetActive(true);
            BottomRight.SetActive(true);
        }
    }


    private void tutorialSetting()
    {
        //lobbyStep 최초 로비 진입 //LobbyFirstTuto 이거일듯
        {
            //FrontUI_Tutorial1 tutorial1 = KOBManager.FrontUI.OpenPopup<FrontUI_Tutorial1>();
            //tutorial1.Init();
        }
    }



    /// <summary>
    /// 버튼 정보를 로비로 부터 얻어올 경우 사용한다.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public LobbyContentButton GetButton(BtnType type)
    {
        return LobbyBtn[(int)type];
    }


    public void TouchBaller()
    {
        GetButton(BtnType.Baller).OnClickButton();
    }


    public void OnClickInBox()
    {
        KOBManager.Popup.OpenPopup<Popup_Inbox>();
    }

    private void UpdateInbox(object pram)
    {
        Transform noty = InBoxObj.transform.Find("Notify");
        Transform Text_Count = noty?.transform.Find("Text_Count");
        int num = KOBManager.Backend.PostList.Count;
        if (num > 0)
        {
            noty.gameObject.SetActive(true);
            if (Text_Count != null)
            {
                Text_Count.GetComponent<TextMeshProUGUI>().text = num.ToString();
            }
        }
        else
        {
            noty.gameObject.SetActive(false);   
        }
    }






    //테스트
#if UNITY_EDITOR
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyUp(KeyCode.A))
        {
            int cIndex = KOBRewardUtil.GetRandomYouHave(KOBRarity.COMMON);
            int rIndex = KOBRewardUtil.GetRandomYouHave(KOBRarity.RARE);
            int eIndex = KOBRewardUtil.GetRandomYouHave(KOBRarity.EPIC);
            int lIndex = KOBRewardUtil.GetRandomYouHave(KOBRarity.LEGENDARY);
            Debug.Log("You Have => 커먼:" + cIndex + " //레어:" + rIndex + " //에픽:" + eIndex + " //레전드:" + lIndex);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            int cIndex = KOBRewardUtil.GetRandomNoHave(KOBRarity.COMMON);
            int rIndex = KOBRewardUtil.GetRandomNoHave(KOBRarity.RARE);
            int eIndex = KOBRewardUtil.GetRandomNoHave(KOBRarity.EPIC);
            int lIndex = KOBRewardUtil.GetRandomNoHave(KOBRarity.LEGENDARY);
            Debug.Log("No Have => 커먼:" + cIndex + " //레어:" + rIndex + " //에픽:" + eIndex + " //레전드:" + lIndex);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            int cIndex = KOBRewardUtil.GetRandomNocondition(KOBRarity.COMMON);
            int rIndex = KOBRewardUtil.GetRandomNocondition(KOBRarity.RARE);
            int eIndex = KOBRewardUtil.GetRandomNocondition(KOBRarity.EPIC);
            int lIndex = KOBRewardUtil.GetRandomNocondition(KOBRarity.LEGENDARY);
            Debug.Log("No con Have => 커먼:" + cIndex + " //레어:" + rIndex + " //에픽:" + eIndex + " //레전드:" + lIndex);
        }

        /*
        if(Input.GetKeyUp(KeyCode.A))
        {
            TResultRttsLeagueUpgrade res = new TResultRttsLeagueUpgrade();
            res.CurrentLeague = 5;
            res.NextLeague = 6;
            res.FinalStanding = new[] { 1, 30, 2, 10 };
            res.HRLeader = new int[] { 1001, 20 };
            res.AvgLeader = new int[] { 11001, 20 };
            res.RbiLeader = new int[] { 1049, 50 };
            res.HitLeader = new int[] { 21001, 20 };
            res.OpsLeader = new int[] { 1050, 12000 };
            KOBManager.UI.OpenWindow<UI_RTTSResult>().Set(res);
        }*/

    }


    public void OnClickRandomTest()
    {
        KOBManager.DummyNetwork.SendPacket(new TRequestRandomTest(), (BackendReturnObject callback, TResponseBase response) =>
        {
            if(callback?.IsSuccess() == true)
            {
                Debug.Log("성공");
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            }
        });
    }



    public void OnClickRandomTest2()
    {
        KOBManager.DummyNetwork.SendPacket(new TRequestRandomTest2(), (BackendReturnObject callback, TResponseBase response) =>
        {
            if (callback?.IsSuccess() == true)
            {
                Debug.Log("성공2");
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            }
        });
    }
#endif
}
