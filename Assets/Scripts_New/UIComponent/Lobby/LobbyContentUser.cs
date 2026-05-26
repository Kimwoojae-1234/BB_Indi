using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyContentUser : LobbyContentButton
{

    [Header("[유저 버튼 전용]")]
    [SerializeField] private GameObject MaskObj;


    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("유저 버튼 초기화");        
    }


    public override void UpdateContent()
    {
        base.UpdateContent();
        if (isUpdate == false)
        {
            //닉네임 세팅
            BtnText.text = KOBManager.MyInfo.BackkendUserInfo.nickname;

            //초상화 이미지 세팅
            //string potrait = string.Format("Profile_Portrait_{0:D3}", KOBManager.MyInfo.UserInfo.Profile.PortraitNo);
            //Icon.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.BotProfile, potrait);


            isUpdate = true;
        }
    }


    public override void OnClickButton()
    {
        base.OnClickButton();
        KOBManager.Popup.OpenPopup<Popup_UserInfo>();
    }
}