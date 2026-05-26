using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Popup_Error : UIPopup
{

    public delegate void ErrorResultCallback();
    private ErrorResultCallback CallBack = null; //에러처리 콜백


    public void InitPopup(ErrorResultCallback callBack = null)
    {
        CallBack = null;
    }


    public override void Close()
    {
        base.Close();
        if (CallBack != null)
        {
            CallBack();
            CallBack = null;
        }
    }


}
