using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lobbyManager : MonoBehaviour {

    public GameObject LoginObj;
    public GameObject TeamSelectObj;
    public GameObject WaitObj;


    public GameObject player1, player2;
    public UILabel idLabel;
    public UILabel connectingLabel;
    public UILabel roomLabel;
    TouchScreenKeyboard keyboardInstance = null;
    private bool keyActive = false;
    private string userID = null;

    public GameObject loadingObj;

    public GameObject quitButton, gameStart;
         

    // Use this for initialization
    void Start ()
    {        
        loadingObj.gameObject.SetActive(false);
        LoginObj.SetActive(true);
        TeamSelectObj.SetActive(false);
        WaitObj.SetActive(false);
        keyActive = false;

        WaitObj.transform.Find("UI_PopupLiveMatchSerching").GetComponent<UI_pvpwaiting>().InitEvent();

#if UNITY_EDITOR

#else
        player1.SetActive(false);
        player2.SetActive(false);
#endif
    }
	
	// Update is called once per frame
	void Update ()
    {

#if UNITY_EDITOR
        if(Input.GetKeyUp(KeyCode.Space))
        {
            soundmanager.Get().PlaySound(soundmanager.SoundID.BallCall);
        }
#else
        if (keyActive == true)
        {
            if (keyboardInstance != null)
            {
                userID = keyboardInstance.text;
                idLabel.text = userID;

                //debugCode.text = string.Format(strUrl, key, couponString, myId);

                if (keyboardInstance.done == true)
                {
                    pvpmanager.Get().UserID[0] = userID;
                    keyActive = false;
                }
                //debugCode2.text = "" + keyboardInstance.done;
            }
        }
#endif
    }


    public void keyboardActive()
    {
#if UNITY_EDITOR

#else
        if (keyActive == false)
        {
            keyActive = true;
            keyboardInstance = TouchScreenKeyboard.Open(userID, TouchScreenKeyboardType.Default, false, false, false, false);
        }
#endif
    }


    public void player1Login()
    {
        userID = "player1";
        idLabel.text = userID;
        pvpmanager.Get().UserID[0]= userID;
    }

    public void player2Login()
    {
        userID = "player2";
        idLabel.text = userID;
        pvpmanager.Get().UserID[0] = userID;
    }


    public void Login()
    {
        if (userID != null)
        {
            //Debug.Log("user id : " + userID);     
            TeamSelectObjActive();            
        }
    }


    private void TeamSelectObjActive()
    {        
        LoginObj.SetActive(false);
        TeamSelectObj.SetActive(true);
    }


    public void selectTeam(GameObject arg)
    {
        loadingObj.gameObject.SetActive(true);
        int code = int.Parse(arg.name);
        pvpmanager.Get().teamCode[0] = (WebConnector.TeamCode)code;
        Debug.Log("index = " + pvpmanager.Get().teamCode[0]);

        PvpUserInfo myInfo = new PvpUserInfo();
        myInfo.UserName = pvpmanager.Get().UserID[0];
        myInfo.teamCode = pvpmanager.Get().teamCode[0];
        //PhotonManager.Get().ConnectToMaster(myInfo, WaitObjActive);
    }

    private void WaitObjActive()
    {
        loadingObj.gameObject.SetActive(false);
        TeamSelectObj.SetActive(false);
        WaitObj.SetActive(true);
        quitButton.SetActive(true);
        gameStart.SetActive(false);
        WaitObj.transform.Find("UI_PopupLiveMatchSerching").GetComponent<UI_pvpwaiting>().init();
    }


    public void quitWaitRoom()
    {
        Debug.Log("quitWaitRoom");

        //PhotonManager.Get().Disconnect();
        WaitObj.SetActive(false);
        LoginObj.SetActive(true);
    }
    
}
