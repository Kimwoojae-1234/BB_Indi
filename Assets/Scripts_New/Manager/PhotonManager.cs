using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun.Demo.PunBasics;


public class PhotonManager : MonoBehaviourPunCallbacks
{
    //이벤트
    // Connect 성공 이벤트 이벤트
    public delegate void EventConnected();
    public static event EventConnected eventConnected;

    // 랜덤룸 조인 실패
    public delegate void EventJoinRandomFailed();
    public static event EventJoinRandomFailed eventJoinRandomFailed;

    // 룸 조인 성공
    public delegate void EventJoinedRoom(Player masterPlayer);
    public static event EventJoinedRoom eventJoinedRoom;

    // 룸 생성 성공
    public delegate void EventCreatedRoom();
    public static event EventCreatedRoom eventCreatedRoom;

    // 룸에 상대 들어옴
    public delegate void EventPlayerEnteredRoom(Player newPlayer);
    public static event EventPlayerEnteredRoom eventPlayerEnteredRoom;

    // 룸에서 상대 나감
    public delegate void EventPlayerLeftRoom(Player otherPlayer);
    public static event EventPlayerLeftRoom eventPlayerLeftRoom;

    //나의 디스커넥트
    public delegate void EventDisconnected();
    public static event EventDisconnected eventDisconnected;

    private int Group = 0;


    public void PhotonConnect(int group)
    {
        Group = group;

        // 유저의 ID와 닉네임 설정
        string userid = Random.Range(100000, 999999).ToString();//MainManager.MyInfo.UserInfo.gamerId;
        string userNickname = "guest";// MainManager.MyInfo.UserInfo.nickname;
        PhotonNetwork.AuthValues = new AuthenticationValues(userid);
        PhotonNetwork.NickName = userNickname;

        // Photon 서버에 연결
        PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
        //PhotonNetwork.PhotonServerSettings.AppSettings.EnableLobbyStatistics = true;
        //PhotonNetwork.ConnectToBestCloudServer();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon Callback : OnConnectedToMaster with ID: " + PhotonNetwork.AuthValues.UserId + " and Nickname: " + PhotonNetwork.NickName);

        if (eventConnected != null)
        {
            eventConnected();
        }

        // 그룹 ID를 정의합니다.
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "groupID", Group } };
        // 특정 그룹의 방에 바로 입장 시도
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Photon Callback : OnJoinRandomFailed");

        if (eventJoinRandomFailed != null)
        {
            eventJoinRandomFailed();
        }

        // 그룹 ID를 정의합니다.
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable { { "groupID", Group } };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            CustomRoomProperties = customRoomProperties,
            PlayerTtl = 20000,
            CustomRoomPropertiesForLobby = new string[] { "groupID" }
        };

        // 새로운 방 생성
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Photon Callback : OnCreatedRoom");

        if (eventCreatedRoom != null)
        {
            eventCreatedRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Photon Callback : OnJoinedRoom");

        Player MaterClient = null;
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("I am the Master Client.");
        }
        else
        {
            Debug.Log("I am not the Master Client.");
            MaterClient = PhotonNetwork.MasterClient;
            Debug.Log("I am not the Master Client. // MaterClient : " + MaterClient);
        }

        if (eventJoinedRoom != null)
        {
            eventJoinedRoom(MaterClient);
        }
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Photon Callback : OnPlayerEnteredRoom => Player: " + newPlayer.NickName);
        if (eventPlayerEnteredRoom != null)
        {
            eventPlayerEnteredRoom(newPlayer);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Photon Callback : OnPlayerLeftRoom => Player: " + otherPlayer.NickName);
        if (eventPlayerLeftRoom != null)
        {
            eventPlayerLeftRoom(otherPlayer);
        }
    }


    public override void OnLeftRoom()
    {
        Debug.Log("Photon Callback : OnLeftRoom");
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Photon Callback : OnDisconnected : " + cause.ToString());
        if (eventDisconnected != null)
        {
            eventDisconnected();
        }
    }



    public void PhotonDisconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public void PhotonReConnectAgain()
    {
        //추후 작업 할것 -> 리커넥트 프로세스
        Debug.Log("PhotonReConnectAgain");
        //isReconnectProcess = true;
        PhotonNetwork.Reconnect();
    }


    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }



}
