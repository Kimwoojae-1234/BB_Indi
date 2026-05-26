using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Tournament : UIWindow
{
    // Start is called before the first frame update
    public override void OpenWindow()
    {
        base.OpenWindow();
        //Debug.Log("===============>> 토너먼트 오픈 윈도우");
    }


    public void OnClickShop()
    {
        KOBManager.UI.OpenWindow<UI_Shop>().LastWindow = typeof(UI_Tournament);
    }

    public void OnClickPlayer()
    {
        //KOBManager.UI.OpenWindow<UI_Player>().LastWindow = typeof(UI_Tournament);
    }
}
