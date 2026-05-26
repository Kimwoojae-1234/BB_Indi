using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_RTTSResult : UIWindow
{
    public enum ResultStep
    {
        LeagueStading = 100,
        HrLeader,
        AvgLeader,
        RbiLeader,
        HitLeader,
        OpsLeader,
        Others,
        Promotion
    }

    [SerializeField] private GameObject[] btnObj;
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _LeagueStandingObj;
    [SerializeField] private GameObject _MyLeaderObj;
    [SerializeField] private GameObject _OtherLeaderObj;
    [SerializeField] private GameObject _MyChampObj;



    private int showCount = 0;
    private int MaxCount = 0;
    private int OtherLeaderCount = 0;


    public override void OpenWindow()
    {
        base.OpenWindow();
    }


    public void Set(TResultRttsLeagueUpgrade res)
    {
        showCount = 0;
        MaxCount = 0;
        OtherLeaderCount = 0;

        //리그 최종순위
        MakeLeagueStading(res);

        //홈런시상 -->   선수 와 해당 타이틀 + 보상 -> 컨티뉴 누르면 보상연출
        //타율 시상-->   선수 와 해당 타이틀 + 보상 -> 컨티뉴 누르면 보상연출
        //타점 시상-->   선수 와 해당 타이틀 + 보상 -> 컨티뉴 누르면 보상연출
        //안타 시상-->   선수 와 해당 타이틀 + 보상 -> 컨티뉴 누르면 보상연출
        //OPS 시상-->   선수 와 해당 타이틀 + 보상 -> 컨티뉴 누르면 보상연출
        MakeMyLeaders(res);

        //기타 1위 보여주고-->   텍스트와 팀만 보여줄것!!
        MakeOtherLeaders(res);

        //리그 1위 한경우-->   리그 승급 연출
        MakeMyChamp(res);
    }


    private void MakeLeagueStading(TResultRttsLeagueUpgrade res)
    {
        GameObject clone = KOBManager.Resource.LoadClone(_LeagueStandingObj, Vector2.zero, Vector3.one, _content.transform);
        clone.GetComponent<ResultLeagueStading>().Set(res);

        MaxCount++;
    }

    private void MakeMyLeaders(TResultRttsLeagueUpgrade res)
    {
        bool isHrTitle = (res.HRLeader[0] / KOBConstant.PLAYER_RECORD_UNIT) == 0;
        if(isHrTitle)
        {
            //조건 설정
            GameObject clone = KOBManager.Resource.LoadClone(_MyLeaderObj, Vector2.zero, Vector3.one, _content.transform);
            clone.GetComponent<ResultMyLeaders>().Set(res, ResultMyLeaders.LeaderType.Homerun);
            MaxCount++;
        }
        else OtherLeaderCount++;

        bool isAvgTitle = (res.AvgLeader[0] / KOBConstant.PLAYER_RECORD_UNIT) == 0;
        if (isAvgTitle)
        {
            //조건 설정
            GameObject clone = KOBManager.Resource.LoadClone(_MyLeaderObj, Vector2.zero, Vector3.one, _content.transform);
            clone.GetComponent<ResultMyLeaders>().Set(res, ResultMyLeaders.LeaderType.Avg);
            MaxCount++;
        }
        else OtherLeaderCount++;

        bool isRbiTitle = (res.RbiLeader[0] / KOBConstant.PLAYER_RECORD_UNIT) == 0;
        if (isRbiTitle)
        {
            //조건 설정
            GameObject clone = KOBManager.Resource.LoadClone(_MyLeaderObj, Vector2.zero, Vector3.one, _content.transform);
            clone.GetComponent<ResultMyLeaders>().Set(res, ResultMyLeaders.LeaderType.Rbi);
            MaxCount++;
        }
        else OtherLeaderCount++;

        bool isHitTitle = (res.HitLeader[0] / KOBConstant.PLAYER_RECORD_UNIT) == 0;
        if (isHitTitle)
        {
            //조건 설정
            GameObject clone = KOBManager.Resource.LoadClone(_MyLeaderObj, Vector2.zero, Vector3.one, _content.transform);
            clone.GetComponent<ResultMyLeaders>().Set(res, ResultMyLeaders.LeaderType.Hit);
            MaxCount++;
        }
        else OtherLeaderCount++;

        bool isOpsTitle = (res.OpsLeader[0] / KOBConstant.PLAYER_RECORD_UNIT) == 0;
        if (isOpsTitle)
        {
            //조건 설정
            GameObject clone = KOBManager.Resource.LoadClone(_MyLeaderObj, Vector2.zero, Vector3.one, _content.transform);
            clone.GetComponent<ResultMyLeaders>().Set(res, ResultMyLeaders.LeaderType.Ops);
            MaxCount++;
        }
        else OtherLeaderCount++;
    }


    private void MakeOtherLeaders(TResultRttsLeagueUpgrade res)
    {
        //조건 설정
        if(OtherLeaderCount > 0) //이값이 0보다 큰경우
        {

        }
    }

    private void MakeMyChamp(TResultRttsLeagueUpgrade res)
    {
        //조건 설정
        int Rank = res.FinalStanding[0];
        if (Rank == 1)
        {
            GameObject clone = KOBManager.Resource.LoadClone(_MyChampObj, Vector2.zero, Vector3.one, _content.transform);
            clone.GetComponent<ResultMyTeamChamp>().SetChamp(res);
            MaxCount++;
        }
    }


    private void DestroyObj()
    {
        //여기서 로딩된 리소스 전체 삭제할 것!!
        foreach (Transform child in _content.transform) Destroy(child.gameObject);
    }





    public void OnClickContinue()
    {
        if (showCount >= MaxCount)
        {
            KOBManager.Rtts.RttsEnter((bool isNewLeague) =>
            {
                if (isNewLeague == true)
                {
                    //리그가 바뀌면 연출
                    Intent it = new Intent();
                    it["PromotionType"] =  Popup_Promotion.PromotionType.League_Promotion;
                    it.AddIntentData<UIPopup.OnClickAction>(UIPopup.ON_CLOSE, () =>
                    {
                        DestroyObj();//로비로 가기전 요소들 다 지워
                        KOBManager.UI.OpenWindow<UI_RTTS>().LastWindow = typeof(UI_LobbyRe);
                    });
                    KOBManager.Popup.OpenPopup<Popup_Promotion>().Set(it);
                }
                else
                {
                    //아닌경우
                    DestroyObj();//로비로 가기전 요소들 다 지워
                    KOBManager.UI.OpenWindow<UI_RTTS>().LastWindow = typeof(UI_LobbyRe);
                }
            });
        }
        else
        {
            //보상연출한후 다음으로 넘어감
            showCount++;
        }
    }


    public void OnClickClaim()
    {

    }
}
