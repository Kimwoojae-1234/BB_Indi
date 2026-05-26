using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_ChestReward : UIPopup
{
    private GameConfig.Callback backCallBack = null;

    public void Setting(GameConfig.Callback callback)
    {
        backCallBack = callback;
    }


    public override void Close()
    {
        base.Close();
        if(backCallBack != null)
        {
            backCallBack();
        }
        backCallBack = null;
    }
       
}
