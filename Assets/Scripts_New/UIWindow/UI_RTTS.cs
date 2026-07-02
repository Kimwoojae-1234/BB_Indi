using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class UI_RTTS : UIWindow
{
    enum RttsUIStae
    {
        Normal,
        RttsTrophy
    }


    [Header("[상단 UI]")]
    [SerializeField] private LobbyPropertyComponent UserProperty;

    [Header("[좌측 UI 그룹]")]
    [SerializeField] private GameObject origin;
    [SerializeField] private GameObject[] LeftObj;
    [SerializeField] private Image logoObj = null;
    [SerializeField] private RttsRewardComponent rttsRewardInfo = null;

    [Header("[우측 UI]")]
    [SerializeField] private GameObject[] Tab;
    [SerializeField] private TextMeshProUGUI[] TabText;
    [SerializeField] private CanvasGroup[] NewObj;
    [SerializeField] private GameObject[] Scroll;

    [Header("[Standing]")]
    [SerializeField] private StandingComponent [] StandingComp;
    [SerializeField] private RectTransform StandingContent;

    [Header("[Schedule]")]
    [SerializeField] private ScheduleComponent ScheduleComp;

    [Header("[Leaders]")]
    [SerializeField] private LeaderComponent[] LeaderComp;
    [SerializeField] private GameObject LeaderSideTab;
    [SerializeField] private TextMeshProUGUI[] LeaderSideTabTxt;

    [Header("[버튼]")]
    [SerializeField] private LobbyContentButton[] LobbyBtn;          //


    [Header("[Rtts 트로피]")]
    [SerializeField] private GameObject RttsTrophy;

    private int lastTab = 0;
    RttsUIStae State;


    bool isLocalSaveFlag = false;

    public override void Initialize()
    {
        base.Initialize();        
        
        //UserProperty.InitProperty(typeof(UI_RTTS));
        for (int i = 0; i < LobbyBtn.Length; i++)
        {
            if (LobbyBtn[i].transform.parent.gameObject.activeSelf)
            {
                LobbyBtn[i].InitContent(typeof(UI_RTTS));
            }
        }

        //for(int i = 0;i< LeftObj.Length;i++) LeftObj[i].gameObject.SetActive(true);
        LeftObj[1].gameObject.SetActive(false);
        LeftObj[2].gameObject.SetActive(false);

        //새로 생길때만 플래고 온
        isLocalSaveFlag = false;
    }



    public override void OpenWindow()
    {
        base.OpenWindow();

        UserProperty.InitProperty(typeof(UI_RTTS));
        for (int i = 0; i < LobbyBtn.Length; i++)
        {
            if (LobbyBtn[i].transform.parent.gameObject.activeSelf)
            {
                LobbyBtn[i].UpdateContent();
            }
        }
        State = RttsUIStae.Normal;
        DeActiveRttsTrophy();
        logoObj.transform.parent = origin.transform;
        logoObj.transform.localPosition = Vector3.zero;

        //현재 플레이중인 볼러
        int ballerIdx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;
        KOBManager.MyInfo.SetUISelectedBaller(ballerIdx); //RTTS진입시 안전빵으로 한번더

        //리그로고
        KOBManager.Resource.LoadMyLeagueLogo(logoObj);

        //순위 정보
        KOBManager.Rtts.InitStandingInfo();
        //리그리더 정보
        KOBManager.Rtts.InitLeagueLeaderInfo();
        //보상정보
        rttsRewardInfo.InitComp();

        InitTabMenu();
        InitStandingTab();        
        InitLeaderTab();
        InitDailyTop50Tab();

        InitScheduleTab(); //스케쥴 텝으로 초기화


        //OnClickTab(1);


        //나왔다 들어갔다 할때 계속해서 Save안하도록 하는... 
        if (isLocalSaveFlag ==false)
        {
            KOBManager.Rtts.RttsLocalSave(); //
            isLocalSaveFlag = true;
        }

    }




    private void InitTabMenu()
    {
        //리그5전
        for (int i = 0; i < TabText.Length; i++)
        {
            if (i == 3)
            {
                TabText[i].transform.Find("Off").gameObject.SetActive(true);
                Tab[i].GetComponent<Button>().enabled = false;
                TabText[i].color = new Color(0.5f, 0.5f, 0.5f, 1);
            }
            else
            {
                TabText[i].transform.Find("Off").gameObject.SetActive(false);
                Tab[i].GetComponent<Button>().enabled = true;
                TabText[i].color = new Color(1, 1, 1, 1);
            }
            TabText[i].transform.Find("New").gameObject.SetActive(false);            
        }

        //리그5이후 - top50열림
    }


    int myPosition = -1;
    private void InitStandingTab()
    {        
        for (int i = 0;i< StandingComp.Length;i++)
        {
            int idx = KOBManager.Rtts.GetTeamIndex(i);
            StandingComp[i].InitComp(idx);
        }

        var sortVar = from item in KOBManager.Rtts.StadingInfo
                      orderby item.Value descending
                      select item;

        KOBManager.Rtts.StadingInfo = sortVar.ToDictionary(x => x.Key, x => x.Value);
        
        int count = 0;
        int _firstwld = 0;        
        int lastRank = 1;
        bool bSameGab = false;
        int lastIdx = -1;
        myPosition = -1;

        Dictionary<int, TeamRecord> LeagueTeamRecord = KOBManager.MyInfo.GameData.RttsInfo.LeagueTeamRecord;
        foreach (KeyValuePair<int, int> standing in KOBManager.Rtts.StadingInfo)
        {
            int idx = standing.Key;
            if (idx == 0) myPosition = count;//내포지션
            int rank = count + 1;

            int win = 0;
            int lose = 0;
            int draw = 0;
            if(LeagueTeamRecord.ContainsKey(idx) == true)
            {
                win = LeagueTeamRecord[idx].Win;
                lose = LeagueTeamRecord[idx].Lose;
                draw = LeagueTeamRecord[idx].Draw;
            }
            int _wld = win * 1000000 + lose * 1000 + draw;

            if(lastIdx !=-1)
            {
                int lastwin = 0;
                int lastlose = 0;
                int lastdraw = 0;
                if (LeagueTeamRecord.ContainsKey(lastIdx) == true)
                {
                    lastwin = LeagueTeamRecord[lastIdx].Win;
                    lastlose = LeagueTeamRecord[lastIdx].Lose;
                    lastdraw = LeagueTeamRecord[lastIdx].Draw;
                }
                int Gab = ((lastwin - win) - (lastlose - lose));
                bSameGab = (Gab == 0 ? true : false);
            }
            lastRank = StandingComp[idx].SetRank(rank, _wld, _firstwld, lastRank, bSameGab);

            //로컬에 데이터 저장
            int saveRank = bSameGab ? lastRank : rank;
            KOBManager.Rtts.CurrentRank[idx] = saveRank;
            if(idx == 0)
            {
                if(saveRank != KOBManager.Rtts.LocalSave.Standing)
                {
                    StandingComp[idx].SetMyRankUpDown(saveRank < KOBManager.Rtts.LocalSave.Standing);
                }
                KOBManager.Rtts.LocalSave.SaveStading(saveRank);
            }

            StandingComp[idx].transform.SetSiblingIndex(count);
            if (count == 0)
            {
                _firstwld = _wld;
            }            
            lastIdx = idx;
            count++;
        }
        Scroll[1].SetActive(false);
        UIUtil.ScrollTo(Scroll[1].GetComponent<ScrollRect>(), myPosition);
    }




    private void InitScheduleTab()
    {
        ScheduleComp.InitComp();

        for (int i = 0; i < Tab.Length; i++)
        {
            Image spr = Tab[i].transform.Find("Focus").GetComponent<Image>();
            //CanvasGroup cg = Scroll[i].GetComponent<CanvasGroup>();
            if (i == 0)
            {
                spr.color = new Color(1, 1, 1, 1);
            }
            else
            {
                spr.color = new Color(1, 1, 1, 0);
            }
        }
        lastTab = 0;
        Scroll[0].SetActive(true);
    }

    private void ReOpenScheduleTab()
    {
        Scroll[1].SetActive(false);
    }

    private void InitLeaderTab()
    {
        for (int i = 0; i< LeaderSideTabTxt.Length;i++)
        {
            LeaderSideTabTxt[i].color = (i == 0 ? Color.white : Color.gray);
        }
        LeaderSideTab.gameObject.SetActive(false);
        Scroll[2].SetActive(false);
        updateLeader(0);
    }


    private void updateLeader(int arg)
    {        
        if (arg == 0) //HR
        {
            setRankUI(KOBManager.Rtts.HomerunLeader, arg);
        }
        else if (arg == 1) //AVG
        {
            setRankUI(KOBManager.Rtts.AvgLeader, arg);
        }
        else if (arg == 2) //RBI
        {
            setRankUI(KOBManager.Rtts.RbiLeader, arg);
        }
        else if (arg == 3) //H
        {
            setRankUI(KOBManager.Rtts.HitLeader, arg);
        }
        else //if (arg == 4) //OPS
        {
            setRankUI(KOBManager.Rtts.OpsLeader, arg);
        }
    }


    private void setRankUI(Dictionary<int, int> Leaders, int arg)
    {
        int count = 0;
        int myPlayerCount = -1;
        foreach (KeyValuePair<int, int> record in Leaders)
        {
            if (count < 10)
            {
                if (LeaderComp[count].InitComp(record.Key, record.Value, count, arg) == true)
                {
                    myPlayerCount = count;
                }
            }
            else
            {
                if (myPlayerCount >= 0 && myPlayerCount < 10)
                {
                    Debug.Log("10위안에 있음");
                    break;
                }
                else
                {
                    bool isMySelectBaller = KOBManager.Rtts.isSelectBaller(record.Key);
                    if (isMySelectBaller == true)
                    {
                        LeaderComp[9].InitComp(record.Key, record.Value, count, arg);
                        myPlayerCount = 9;
                        break;
                    }
                }
            }
            count++;
        }
        ScrollRect scrollRect = Scroll[2].GetComponent<ScrollRect>();
        UIUtil.ScrollTo(scrollRect, myPlayerCount);
    }



    private void ReOpenLeaderTab()
    {
        for (int i = 0; i < LeaderSideTabTxt.Length; i++)
        {
            LeaderSideTabTxt[i].color = (i == 0 ? Color.white: Color.gray);
        }
        LeaderSideTab.gameObject.SetActive(false);
        Scroll[2].SetActive(false);
    }

    private void InitDailyTop50Tab()
    {
        Scroll[3].SetActive(false);
    }

    private void ReOpenDailyTop50Tab()
    {
        Scroll[3].SetActive(false);
    }



    public void OnClickShop()
    {
        Debug.Log("===============>> RTTS에서 Shop 진입");
        KOBManager.UI.OpenWindow<UI_Shop>().LastWindow = typeof(UI_RTTS);
    }

    public void OnClickPlayer()
    {
        Debug.Log("===============>> RTTS에서 Player UI 진입");
        //KOBManager.UI.OpenWindow<UI_Player>().LastWindow = typeof(UI_RTTS);
    }

    public void OnClickTeammate()
    {
        Debug.Log("===============>> RTTS에서 Teamate UI 진입");
    }


    public void OnClickPlayBall()
    {
        Debug.Log("===============>> RTTS에서 Playball 진입");
    }


    public void OnClickTab(int arg)
    {
        if (lastTab == arg) return;
        Debug.Log("OnClickTab :" + arg);
        for (int i = 0; i < Tab.Length; i++)
        {
            Image spr = Tab[i].transform.Find("Focus").GetComponent<Image>();
            CanvasGroup cg = Scroll[i].GetComponent<CanvasGroup>();
            if (i == arg)
            {
                spr.color = new Color(1, 1, 1, 0);
                spr.DOFade(1, 0.3f);
                Scroll[i].SetActive(true);
                cg.alpha = 0;
                cg.DOFade(1, 0.3f);


            }
            else if (i == lastTab)
            {
                spr.color = new Color(1, 1, 1, 1);
                spr.DOFade(0, 0.3f);
                Scroll[i].SetActive(false);
            }
        }
        LeaderSideTab.gameObject.SetActive(arg == 2 ? true : false);

        if (arg == 0)
        {
            ScheduleComp.InitComp();
        }
        if(arg == 1)
        {
            UIUtil.ScrollTo(Scroll[1].GetComponent<ScrollRect>(), myPosition);
        }

        lastTab = arg;
    }

    public void OnClickSideTab(int arg)
    {
        Debug.Log("OnClickSideTab :" + arg);

        for (int i = 0; i < LeaderSideTabTxt.Length; i++)
        {
            LeaderSideTabTxt[i].color = (i == arg ? Color.white : Color.gray);
        }

        updateLeader(arg);
    }





    public void OnClickRttsTrophyUI()
    {
        if (State == RttsUIStae.Normal)
        {
            Debug.Log("OnClickRttsTrophyUI");
            OpenRttsTrophyUI();
        }
    }


    private void OpenRttsTrophyUI()
    {
        State = RttsUIStae.RttsTrophy;        
        ActiveRttsTrophy();
        bool isFirstTry = KOBManager.Rtts.isLeagueFirstTry();
        RttsTrophy.GetComponent<RttsTrophyComponent>().Open(KOBManager.Rtts.League, isFirstTry, logoObj.gameObject, origin);
    }


    private void ActiveRttsTrophy()
    {
        RttsTrophy.gameObject.SetActive(true);
        for (int i = 0; i < LeftObj.Length; i++) LeftObj[i].gameObject.SetActive(false);
    }

    private void DeActiveRttsTrophy()
    {
        RttsTrophy.gameObject.SetActive(false);
        for (int i = 0; i < LeftObj.Length; i++) LeftObj[i].gameObject.SetActive(true);

        LeftObj[1].gameObject.SetActive(false); //지워지워
        LeftObj[2].gameObject.SetActive(false); //지워지워
    }

    public override void ClickBackButton()
    {
        if (State == RttsUIStae.RttsTrophy)
        {
            State = RttsUIStae.Normal;
            RttsTrophy.GetComponent<RttsTrophyComponent>().Close();
            for (int i = 0; i < LeftObj.Length; i++) LeftObj[i].gameObject.SetActive(true);
            LeftObj[1].gameObject.SetActive(false); //지워지워
            LeftObj[2].gameObject.SetActive(false); //지워지워

            Invoke("DeActiveRttsTrophy", 0.5f);
        }
        else
        {
            base.ClickBackButton();
        }
    }
}



