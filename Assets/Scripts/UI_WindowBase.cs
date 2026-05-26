using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_WindowBase : MonoBehaviour
{
    public LinkedListNode<UI_WindowBase> windowNode;
    public bool IsPopup = false;
    public WindowID windowID {get; protected set;}
    public UIPanel uiPanel = null;
    [SerializeField]
    protected Animation anim;

    // param값들
    public object[] param = null;


    protected virtual void Awake()
    {
        if(windowNode == null)
            windowNode = new LinkedListNode<UI_WindowBase>(this);
        if (uiPanel == null)
            uiPanel = this.GetComponent<UIPanel>();
        
    }   

    // 열기
    public virtual void OpenWindow()
    {
        this.transform.localPosition = Vector3.zero;
        this.gameObject.SetActive(true);
        if (this.anim != null)
         {
            this.anim.wrapMode = WrapMode.Once;
            this.anim.Play("animTest");
            this.anim.Play();
        }
    }

    // 닫기
    public virtual void CloseWindow()
    {
        this.transform.localPosition = Vector3.zero;
        this.gameObject.SetActive(false);
    }

    public void SetWindowID(WindowID windowID)
    {
        this.windowID = windowID;
    }

    public virtual void SetParams(params object[] param)
    {
        this.param = param;
    }

    public virtual bool IsRecive_Esc()
    {
        return false;
    }

    public virtual void WindowOpenDirection()
    {

    }

    public virtual void WindowCloseDirection()
    {

    }
}

public enum WindowID
{
    UI_PopupConfirm,
    test
}
