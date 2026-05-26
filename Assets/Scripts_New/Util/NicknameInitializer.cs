using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NicknameInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Popup_Name popup = KOBManager.Popup.OpenPopup<Popup_Name>();
        popup.CreateNickname(NextScene); //닉네임 크리에이트 타입으로 생성
    }



    public void NextScene()
    {
        //로비 진입
        KOBManager.State.BackToLobby();
    }

}
