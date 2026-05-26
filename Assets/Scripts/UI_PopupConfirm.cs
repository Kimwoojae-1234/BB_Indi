using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_PopupConfirm : UI_WindowBase
{
    [SerializeField]
    private GameObject btnSet_OK;
    [SerializeField]
    private GameObject btnSet_OkCancle;
    // 버튼 4개짜리 팝업
    [SerializeField]
    private GameObject btnSet_Four;


    [SerializeField]
    private UILabel label_Title;
    [SerializeField]
    private UILabel label_Message;

    [SerializeField]
    private UILabel label_BtnOne_OK;
    [SerializeField]
    private UISprite sprite_BtnTwo_OK;
    [SerializeField]
    private UISprite sprite_BtnTwo_Cancel;
    
    /// <summary>
    /// 왼쪽을 기준
    /// </summary>
    [SerializeField]
    private List<UILabel> list_label_BtnFour;
    /// <summary>
    /// 왼쪽을 기준
    /// </summary>
    private List<EventDelegate.Callback> list_callBack_Four;

    [SerializeField]
    private TweenAlpha bgTween;
    [SerializeField]
    private TweenAlpha mainWidgetTween;


    [SerializeField]
    private GameObject CenterContext;
    [SerializeField]
    private UILabel retry_txt;




    private EventDelegate.Callback callBack_OK;
    private EventDelegate.Callback callBack_Cancel;

    //인게임 전용
    private bool bIngameOnly;

    public enum EPOPUP
    {
        Message_YN, // 확인 취소 팝업
        Message_OK, // 제목 없는 확인 팝업
        Message_TitleOK,    // 제목 있는 확인 팝업
        Message_Toast,  // 내용만 있는 포스트 팝업
        Message_Four,   // 버튼이 4개나 존재하는 팝업
    }

    protected override void Awake()
    {
        base.Awake();
        this.SetWindowID(WindowID.UI_PopupConfirm);
        // 팝업이니 표시해준다.
        this.IsPopup = true;
        this.bIngameOnly = false;
    }

    public void SetPopup(EPOPUP ePopupType, string title = "", string message = "", string text_ok = "", string text_cancel = "",  EventDelegate.Callback okCallBack = null, EventDelegate.Callback cancelCallBack = null, string[] list_text_four = null, EventDelegate.Callback[] list_callback_four = null)
    {
        if (message.Contains("[-]") == false)
            this.label_Message.supportEncoding = false;
        else
            this.label_Message.supportEncoding = true;

        switch(ePopupType)
        {
            case EPOPUP.Message_Toast:
                {
                    // 이건 글로벌로 빼야 한다. 
                }
                break;
            case EPOPUP.Message_TitleOK:
                {
                    this.label_Title.text = title;
                    this.label_Message.text = message;
                    this.label_BtnOne_OK.text = text_ok;
                    this.btnSet_OK.SetActive(true);
                    this.btnSet_OkCancle.SetActive(false);
                    this.btnSet_Four.SetActive(false);
                    this.callBack_OK = okCallBack;
                }
                break;
            case EPOPUP.Message_OK:
                {
                    this.label_Title.text = "";
                    this.label_Message.text = message;
                    this.label_BtnOne_OK.text = text_ok;
                    this.btnSet_OK.SetActive(true);
                    this.btnSet_OkCancle.SetActive(false);
                    this.btnSet_Four.SetActive(false);
                    this.callBack_OK = okCallBack;
                }
                break;

            case EPOPUP.Message_YN:
                {
                    this.label_Title.text = "";
                    this.label_Message.text = message;
                    this.sprite_BtnTwo_OK.spriteName = "txt_ok";
                    this.sprite_BtnTwo_Cancel.spriteName = "txt_cancel";
                    this.btnSet_OK.SetActive(false);
                    this.btnSet_OkCancle.SetActive(true);
                    this.btnSet_Four.SetActive(false);
                    this.callBack_OK = okCallBack;
                    this.callBack_Cancel = cancelCallBack;
                }
                break;
            case EPOPUP.Message_Four:
                {
                    this.label_Title.text = "";
                    this.label_Message.text = message;
                    this.btnSet_OK.SetActive(false);
                    this.btnSet_OkCancle.SetActive(false);
                    this.btnSet_Four.SetActive(true);
                    this.list_callBack_Four = new List<EventDelegate.Callback>();
                    for (int i = 0; i < list_text_four.Length; ++i)
                    {
                        this.list_callBack_Four.Add(list_callback_four[i]);
                        this.list_label_BtnFour[i].text = list_text_four[i];
                    }

                }
                break;
        }
    }

    public void SetPopup_OneBtn(string title, string message, EventDelegate.Callback callBack = null)
    {
        
        this.SetPopup_OneBtn(title, message, "확인", callBack);
    }

    public void SetPopup_OneBtn(string title, string message, string text_Btn, EventDelegate.Callback callBack = null)
    {
        if (message.Contains("[-]") == false)
            this.label_Message.supportEncoding = false;
        else
            this.label_Message.supportEncoding = true;

        this.label_Title.text = title;
        this.label_Message.text = message;
        this.label_BtnOne_OK.text = text_Btn;
        this.callBack_OK = callBack;

        this.btnSet_OK.SetActive(true);
        this.btnSet_OkCancle.SetActive(false);
        this.btnSet_Four.SetActive(false);

        if (bIngameOnly == false)
        {
            //임시
            ViewCenterContext(string.Empty, false);
            //
        }
    }

    public void SetPopup_TwoBtn(string title, string message, EventDelegate.Callback callBack_Left = null, EventDelegate.Callback callBack_Right = null)
    {
        this.SetPopup_TwoBtn(title, message, "확인", "취소", callBack_Left, callBack_Right);
    }

    public void SetPopup_TwoBtn(string title, string message, string text_BtnLeft, string text_BtnRight, EventDelegate.Callback callBack_Left = null, EventDelegate.Callback callBack_Right = null)
    {
        if (message.Contains("[-]") == false)
            this.label_Message.supportEncoding = false;
        else
            this.label_Message.supportEncoding = true;

        this.label_Title.text = title;
        this.label_Message.text = message;
        this.sprite_BtnTwo_OK.spriteName = "txt_ok";
        this.sprite_BtnTwo_Cancel.spriteName = "txt_cancel";
        this.callBack_OK = callBack_Left;
        this.callBack_Cancel = callBack_Right;

        this.btnSet_OK.SetActive(false);
        this.btnSet_OkCancle.SetActive(true);
        this.btnSet_Four.SetActive(false);

        if (bIngameOnly == false)
        {
            //임시
            ViewCenterContext(string.Empty, false);
            //
        }
    }

    public void Click_OK()
    {
        if (bIngameOnly == false)
        {
            // DISABLED_MGRS: Mgrs.UI.CloseWindow(this.windowID);
        }

        if (callBack_OK != null)
        {
            EventDelegate.Callback callback = callBack_OK;
            callBack_OK = null;
            callBack_Cancel = null;
            callback();
        }
    }

    public void Click_Cancel()
    {
        if (bIngameOnly == false)
        {
            // DISABLED_MGRS: Mgrs.UI.CloseWindow(this.windowID);
        }

        if (callBack_Cancel != null)
        {
            EventDelegate.Callback callback = callBack_Cancel;
            callBack_OK = null;
            callBack_Cancel = null;
            callback();
        }
    }

    public override bool IsRecive_Esc()
    {
        return false;
    }

    private void Click_FourMode(int btnIndex)
    {
        if (bIngameOnly == false)
        {
            // DISABLED_MGRS: Mgrs.UI.CloseWindow(this.windowID);
        }

        EventDelegate.Callback callBack = this.list_callBack_Four[btnIndex];
        if(callBack != null)
        {
            callBack();
            callBack = null;
        }
    }

    public void Click_FourMode_One()
    {
        this.Click_FourMode(0);
    }

    public void Click_FourMode_Two()
    {
        this.Click_FourMode(1);
    }

    public void Click_FourMode_Three()
    {
        this.Click_FourMode(2);
    }

    public void Click_FourMode_Four()
    {
        this.Click_FourMode(3);
    }

    public void BGTweenPlay()
    {
        bgTween.ResetToBeginning();
        bgTween.PlayForward();
    }

    public void MainWidgetTweenPlay()
    {
        mainWidgetTween.ResetToBeginning();
        mainWidgetTween.PlayForward();
    }

    /// <summary>
    /// 임시함수
    /// </summary>
    /// <param name="message"></param>
    /// <param name="isView"></param>
    public void ViewCenterContext(string message, bool isView)
    {
        CenterContext.SetActive(isView);
        retry_txt.text = message;
    }


    public void SetIngameMode()
    {
        bIngameOnly = true;
    }
}
