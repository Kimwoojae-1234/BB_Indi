using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

public class Popup_Name : UIPopup
{
    [SerializeField] private GameObject[] quitObj;    
    [SerializeField] private TextMeshProUGUI Title;
    [SerializeField] private TextMeshProUGUI Title2;


    [SerializeField] private TextMeshProUGUI NickName;
    [SerializeField] private Button ButtonCreate;
    [SerializeField] private Button ButtonChange;


    public enum NicknameEdit
    {
        Create,
        Change
    }
    private NicknameEdit Type = NicknameEdit.Change;
    private bool bButtonActive = false;


    public delegate void CreateNicknameCallback();
    private CreateNicknameCallback CallBack = null; //닉네임 크리에이트





    public void CreateNickname(CreateNicknameCallback _callBack)
    {
        CallBack = _callBack;
        Type = NicknameEdit.Create;
        ButtonInActive();
        for (int i = 0; i < quitObj.Length; i++) quitObj[i].gameObject.SetActive(false);
        Title.text = KOBManager.Localization.GetUILocalizedValue2("PopupBody.CreateNickname1");
        Title2.text = KOBManager.Localization.GetUILocalizedValue2("PopupBody.CreateNickname2");

        ButtonCreate.gameObject.SetActive(true);
        ButtonChange.gameObject.SetActive(false);

        NickName.text = string.Empty;
    }


    public void ChangeNickname()
    {
        Type = NicknameEdit.Change;
        ButtonInActive();
        for (int i = 0; i < quitObj.Length; i++) quitObj[i].gameObject.SetActive(true);
        Title.text = KOBManager.Localization.GetUILocalizedValue2("PopupBody.ChangeNickname1");
        Title2.text = KOBManager.Localization.GetUILocalizedValue2("PopupBody.ChangeNickname2");

        ButtonCreate.gameObject.SetActive(false);
        ButtonChange.gameObject.SetActive(true);
    }


    protected override void Update()
    {
        base.Update();
        if (bButtonActive == false)
        {
            if (NickName.text.Length >= 4)
            {
                ButtonActive();
            }
        }
        else
        {
            if (NickName.text.Length < 4)
            {
                ButtonInActive();
            }
        }
    }

    private void ButtonActive()
    {
        Debug.Log("활성화");
        if (Type == NicknameEdit.Create)
        {
            ButtonCreate.interactable = true;
            ButtonCreate.GetComponent<Image>().sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Btn_MainButton_Green");
        }
        else
        {
            ButtonChange.interactable = true;
            ButtonChange.GetComponent<Image>().sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Btn_MainButton_Green");
        }
        bButtonActive = true;
    }

    private void ButtonInActive()
    {
        Debug.Log("비활성화");
        if (Type == NicknameEdit.Create)
        {
            ButtonCreate.interactable = false;
            ButtonCreate.GetComponent<Image>().sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Btn_MainButton_Gray");
        }
        else
        {
            ButtonChange.interactable = false;
            ButtonChange.GetComponent<Image>().sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Btn_MainButton_Gray");
        }
        bButtonActive = false;
    }




    public override void Close()
    {
        //if (Type == NicknameEdit.Create) return;
        base.Close();
        if(Type == NicknameEdit.Create)
        {
            if(CallBack != null)
            {
                CallBack();
                CallBack = null;
            }
        }
    }



    public void OnClickCreateNickName()
    {
        Debug.Log("닉네임 생성");
        ButtonCreate.interactable = false;

        string CreateNickName = NickName.text.Trim();
        Debug.Log("CreateNickName : " + CreateNickName);
        BackendReturnObject bro3 = Backend.BMember.CreateNickname(CreateNickName);
        if (bro3.IsSuccess())
        {
            Debug.Log("닉네임 생성 성공 " + bro3.ToString());
            CreateNicknameEnd();
        }
        else
        {
            Debug.Log("닉네임 생성 실패 " + bro3.ToString());
            ButtonCreate.interactable = true;
            KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init("Nickname creation failed: " + bro3.GetErrorCode());
        }
    }



    private void CreateNicknameEnd()
    {
        if (KOBManager.Tuto.IsTuroialComplete(TutorialManager.TutoStep.NickNameSetting) == false)
        {
            KOBManager.Tuto.SetTutorialComplete(TutorialManager.TutoStep.NickNameSetting, (bool isSuccess) =>
            {
                if (isSuccess == true)
                {
                    Close();
                }
                else
                {
                    KOBManager.Popup.OpenPopup<Popup_Error>();
                }
            });
        }
        else
        {
            Close();
        }

    }


    public void OnClickChangeNickName()
    {
        
    }
}
