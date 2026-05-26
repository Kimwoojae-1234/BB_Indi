using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyContentShop : LobbyContentButton
{


    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("상점 버튼 초기화");
    }


    public override void OnClickButton()
    {
        base.OnClickButton();

        KOBManager.UI.OpenWindow<UI_Shop>().LastWindow = LastWindow;
    }

}
