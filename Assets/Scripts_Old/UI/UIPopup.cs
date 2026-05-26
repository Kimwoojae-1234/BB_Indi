using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIPopup : UIBase
{
    public const string ON_OK = "ON_OK";
    public const string ON_CLOSE = "ON_CLOSE";
    public const string PRE_POPUP = "PRE_POPUP";

    public delegate void OnClickAction();
    protected OnClickAction _onOK = null;
    protected OnClickAction _onClose = null;

    public bool BackToPrevPopup = false;

    //public Image DownLoadImage = null;    
    public virtual void Open()
    {
        gameObject.SetActive(true);        
    }


    public override void Initialize()
    {
        base.Initialize();
        BackToPrevPopup = false;
    }


    public virtual void Set(Intent it = null)
    {
        Debug.Log("Popup Setting");
        if (it != null)
        {
            SetOkClose(it);
        }
    }

    public override void Uninitialize()
    {
        _onOK = null;
        _onClose = null;
        base.Uninitialize();
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        OnClose();
        if (BackToPrevPopup == true)
        {
            KOBManager.Popup.BackToLastPopup();
            BackToPrevPopup = false;
        }
        else
        {
            //팝업이 닫힐 경우 현재 열린 UI에 이벤트 전달
            UIWindow window = KOBManager.UI.GetCurrentUI();
            if (window != null) window.PopupClose();
        }
    }

    protected override void Update()
    {
        base.Update();
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
#else
        if(Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }
#endif
    }


    protected void SetOkClose(Intent it)
    {
        if (it.Contains(ON_OK))
        {
            _onOK = (OnClickAction)it[ON_OK];
        }
        if (it.Contains(ON_CLOSE))
        {
            _onClose = (OnClickAction)it[ON_CLOSE];
        }
        if (it.Contains(PRE_POPUP))
        {
            BackToPrevPopup = (bool)it[PRE_POPUP];
        }
    }

    public virtual void OnOK()
    {
        if (_onOK != null)
        {
            _onOK();
            _onOK = null;
        }
    }

    public virtual void OnClose()
    {
        if (_onClose != null)
        {
            _onClose();
            _onClose = null;
        }
    }

}
