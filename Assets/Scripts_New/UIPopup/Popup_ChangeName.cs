using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using TMPro;
using BackEnd;

public class Popup_ChangeName : UIPopup
{
    [Header("[유저 정보]")]
    [SerializeField] private TMP_InputField UserName = null;
    [Header("[버튼 정보]")]
    [SerializeField] private Button button = null;

    private UserInfo info = null;
    private string EditName;
    //private bool bButtonCheck = false;
    
    public override void Open()
    {
        base.Open();        
        info = KOBManager.MyInfo.BackkendUserInfo;
        EditName = info.nickname;
        UserName.text = EditName;
        //bButtonCheck = false;

        buttonChecking();
    }

    public void OnClickChangename()
    {        
        //if (bButtonCheck == true) return;
        Debug.Log("OnClickChangename");

        //bButtonCheck = true;
        buttonChecking();


        int num = EditName.Length;
        Debug.Log("num : " + num);
        if(num < 5 || num >= 12)
        {
            NumberOfCharacters();
        }
        else
        {
            Backend.BMember.CheckNicknameDuplication(EditName, callback =>
            {
                if (callback.IsSuccess())
                {
                    ChangeName();
                }
                else
                {
                    DuplicateName();
                }
            });
        }
    }

    public void NameSelect()
    {
        //if (bButtonCheck == true) return;
        EditName = info.nickname;
        UserName.text = EditName;        
        Debug.Log("NameSelect : " + EditName);
        buttonAvailable();
    }

    public void NameChanged(TextMeshProUGUI _name)
    {
        //if (bButtonCheck == true) return;
        EditName = _name.text;
        UserName.text = EditName;
        Debug.Log("NameChanged : " + EditName);
        buttonAvailable();
    }

    private void ChangeName()
    {
        Debug.Log("ChangeName");
        Backend.BMember.UpdateNickname(EditName.Trim(), callback =>
        {
            if (callback.IsSuccess())
            {
                Success();
            }
            else
            {
                Fail();
            }
        });
    }

    
    private void Success()
    {
        Debug.Log("Success");
        KOBManager.MyInfo.InitUserInfo();
        buttonAvailable();
        Close();
        KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init("Name Change Success");
    }

    private void Fail()
    {
        Debug.Log("Fail");
        KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init("Fail", buttonAvailable);
    }

    private void DuplicateName()
    {
        Debug.Log("DuplicateName");
        KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init("DuplicateName", buttonAvailable);
    }

    private void NumberOfCharacters()
    {
        Debug.Log("NumberOfCharacters");
        KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init("NumberOfCharacters", buttonAvailable);
    }


    private void buttonChecking()
    {
        //bButtonCheck = true;
        button.interactable = false;
        //button.GetComponent<Image>().sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.Button, "Btn_MainButton_Gray");
    }

    private void buttonAvailable()
    {
        //bButtonCheck = false;
        button.interactable = true;
        //button.GetComponent<Image>().sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.Button, "Btn_MainButton_Green");
    }

}
