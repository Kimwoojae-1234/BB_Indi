using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyContentButton : MonoBehaviour
{
    [SerializeField] protected Image Icon;
    [SerializeField] protected TextMeshProUGUI BtnText;
    [SerializeField] protected Image ButtonSprite;
    [SerializeField] protected Image NotiSpr;
    [SerializeField] protected TextMeshProUGUI NotiText;
    [SerializeField] protected GameObject LockObj;
    [SerializeField] protected Button button;

    public bool isUpdate = false;

    protected System.Type LastWindow;

    public virtual void InitContent(System.Type type)
    {
        LastWindow = type;
        if (button != null) button.interactable = true;
        if(Icon != null) Icon.color = Color.white;

        //노티 초기화
        if (NotiSpr != null) NotiSpr.gameObject.SetActive(false);
        if (LockObj != null) LockObj.gameObject.SetActive(false);

        isUpdate = false;
    }

    public virtual void UpdateContent()
    {

    }


    public virtual void SetNotify()
    {

    }


    public virtual void OnClickButton()
    {
        isUpdate = false; //다시 로비로 돌아올 경우 초기화 시킴
    }


    protected void SetNewContent()
    {
        ButtonSprite.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Btn_MainButton_Yellow");
    }

    protected void NoNewContent()
    {
        ButtonSprite.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Btn_OtherButton_Square05");
    }

    protected void SetNotiOff()
    {
        NotiSpr.gameObject.SetActive(false);
    }

    protected void SetRedNotiOn(int num)
    {
        NotiSpr.gameObject.SetActive(true);
        NotiSpr.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Notify_Count_Red_l");
        NotiText.text = num.ToString();
    }

    protected void SetGreenNotiOn(int num)
    {
        NotiSpr.gameObject.SetActive(true);
        NotiSpr.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Notify_Count_Green_l");
        NotiText.text = num.ToString();
    }



    protected void ButtonDisable()
    {
        button.interactable = false;
        if (LockObj != null) LockObj.gameObject.SetActive(true);
        if(Icon != null)
        {
            Icon.color = new Color(0.6f, 0.6f, 0.6f);
        }
    }

}
