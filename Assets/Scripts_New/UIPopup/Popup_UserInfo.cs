using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Popup_UserInfo : UIPopup
{
    [Header("[주요 오브젝트]")]
    [SerializeField] private GameObject[] Mode = null;
    
    [Header("[유저 정보]")]
    [SerializeField] private TextMeshProUGUI UserName = null;
    [SerializeField] private Image UserProfile = null;
    
    [Header("[정보 텍스트]")]
    [SerializeField] private TextMeshProUGUI[] RTTS_Info = null;
    [SerializeField] private TextMeshProUGUI[] Tournament_Info = null;
    [SerializeField] private TextMeshProUGUI[] League_Info = null;

    [Header("[정보 텍스트]")]
    [SerializeField] private TextMeshProUGUI ModeTitile = null;

    [Header("[알림]")]
    [SerializeField] private GameObject[] AlarmObj = null;


    private string[] __title = new string[] { "RTTS STATS", "TOURNAMENT STATS", "LEAGUE STATS" };

    private enum UserInfoMode
    {
        RTTS = 0,
        Tournament,
        League
    }

    private UserInfoMode CurrentMode = UserInfoMode.RTTS;

    public override void Open()
    {
        Debug.Log("Open");
        base.Open();
        for (int i = 0; i < AlarmObj.Length; i++) AlarmObj[i].gameObject.SetActive(false); //추후 알람 세팅
        
        UserInfo info = KOBManager.MyInfo.BackkendUserInfo;
        UserName.text = info.nickname;
        
        /*
        KOBUserInfo LobbyInfo = KOBManager.MyInfo.UserInfo;
        //RTTS
        RTTS_Info[0].text = LobbyInfo.Trophy.ToString();
        RTTS_Info[1].text = string.Format("LEAGUE {0}", LobbyInfo.League);
        //RTTS_Info[2].text = LobbyInfo.rttsInfo.PlayerHR.ToString();
        //RTTS_Info[3].text = LobbyInfo.rttsInfo.PlayerH.ToString();
        //RTTS_Info[4].text = BaseballStatUtil.GetAverage(LobbyInfo.rttsInfo.PlayerAB, LobbyInfo.rttsInfo.PlayerH);
        //RTTS_Info[5].text = BaseballStatUtil.GetOps(LobbyInfo.rttsInfo.PlayerPA, LobbyInfo.rttsInfo.PlayerAB, LobbyInfo.rttsInfo.PlayerH, LobbyInfo.rttsInfo.Player2H, LobbyInfo.rttsInfo.Player3H, LobbyInfo.rttsInfo.PlayerHR, LobbyInfo.rttsInfo.PlayerBB, 0);
        
        //토너먼트
        //Tournament_Info[0].text = LobbyInfo.tournamentInfo.Num1st.ToString();
        //Tournament_Info[1].text = LobbyInfo.tournamentInfo.Num2nd.ToString();
        //Tournament_Info[2].text = LobbyInfo.tournamentInfo.NumTop10.ToString();
        //Tournament_Info[3].text = LobbyInfo.tournamentInfo.TotalWin.ToString();
        //Tournament_Info[4].text = LobbyInfo.tournamentInfo.TotalLoss.ToString();
        //Tournament_Info[5].text = LobbyInfo.tournamentInfo.TotalDraw.ToString();

        //League정보
        //League_Info[0].text = LobbyInfo.leagueInfo.Num1st.ToString();
        //League_Info[1].text = LobbyInfo.leagueInfo.Num2nd.ToString();
        //League_Info[2].text = LobbyInfo.leagueInfo.NumTop10.ToString();
        //League_Info[3].text = LobbyInfo.leagueInfo.TotalWin.ToString();
        //League_Info[4].text = LobbyInfo.leagueInfo.TotalLoss.ToString();
        //League_Info[5].text = LobbyInfo.leagueInfo.TotalDraw.ToString();*/


        SetMode(UserInfoMode.RTTS);
    }


    private void OnEnable()
    {
        Debug.Log("OnEnable");
        UserInfo info = KOBManager.MyInfo.BackkendUserInfo;
        UserName.text = info.nickname;
        
    }

    private void SetMode(UserInfoMode _mode)
    {
        CurrentMode = _mode;
        for (int i = 0; i < Mode.Length; i++)
        {
            Mode[i].gameObject.SetActive(i == (int)CurrentMode ? true : false);
        }
        ModeTitile.text = __title[(int)_mode];
    }


    public void OnClickLeftArrow()
    {
        int index = (int)CurrentMode;
        index--;
        if (index < 0) index = 2;
        SetMode((UserInfoMode)index);
    }

    public void OnClickRightArrow()
    {
        int index = (int)CurrentMode;
        index++;
        if (index > 2) index = 0;
        SetMode((UserInfoMode)index);
    }

    public void OnClickEditName()
    {
        KOBManager.Popup.OpenPopup<Popup_ChangeName>().BackToPrevPopup = true;
    }

    public void OnClickProfileButton()
    {
        //추후 알람 비활성화 세팅
    }

    public void OnClickEmojiButton()
    {
        //추후 알람 비활성화 세팅
    }

    public void OnClickAchiveButton()
    {
        //추후 알람 비활성화 세팅
    }
}
