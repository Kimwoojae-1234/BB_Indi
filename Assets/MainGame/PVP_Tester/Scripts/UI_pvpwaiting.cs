using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_pvpwaiting : MonoBehaviour {

    private bool bInitSendInfo, bInitPlayGame;
    private float curTime;


    public Spine.Unity.SkeletonAnimation MyCharAnim, OtherCharAnim;

    public UILabel myName, otherName;

    public void InitEvent()
    {
        bInitSendInfo = false;
        bInitPlayGame = false;
        //PhotonManager.EventConnect += EventConnect;
        pvpmanager.OnContact += OnContact;
    }

    // Use this for initialization
    void Start ()
    {
        
        
    }


    public void init()
    {
        curTime = 0;
        WebConnector.TeamCode myteamCode = pvpmanager.Get().teamCode[0];
        // DISABLED_MGRS: Mgrs.DataLoad.LoadLiveMatchSpineTexture(myteamCode, MyCharAnim, true);
        myName.text = pvpmanager.Get().UserID[0];

    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            playGame();
        }
    }

    //커넥 이벤트
    private void EventConnect(string message)
    {
        if (bInitSendInfo == false)
        {
            Debug.Log("페어링 확인!");
            //서로 코넥트 된 경우 초기 정보 보냄
            bInitSendInfo = true;
            pvpmanager.Get().SendInitInfo();
        }
    }

    //컨택 이벤트
    private void OnContact(PvpUserInfo info)
    {
        if (bInitPlayGame == false)
        {
            Debug.Log("초기 정보 수신관련 이벤트 발생 : 게임 시작");
            bInitPlayGame = true;
            OtherCharAnim.gameObject.SetActive(true);
            WebConnector.TeamCode otherteamCode = info.teamCode;// pvpmanager.Get().teamCode[1];
            otherName.text = info.UserName;// pvpmanager.Get().UserID[1];
            // DISABLED_MGRS: Mgrs.DataLoad.LoadLiveMatchSpineTexture(otherteamCode, OtherCharAnim, false);
            //Debug.Log("접속 : " + PhotonNetwork.connected + "Master : " + PhotonNetwork.isMasterClient);

            Invoke("playGame", 3.0f);
        }
    }


    void playGame()
    {
        //PhotonManager.EventConnect -= EventConnect;
        pvpmanager.OnContact -= OnContact;
        pvpmanager.Get().PlayGame();
    }
}
