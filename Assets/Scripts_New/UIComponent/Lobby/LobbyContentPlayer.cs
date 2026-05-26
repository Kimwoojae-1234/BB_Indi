using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyContentPlayer : LobbyContentButton
{

    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("볼러 버튼 초기화");
    }


    public override void OnClickButton()
    {
        base.OnClickButton();
        KOBManager.UI.OpenWindow<UI_BallersList>().LastWindow = LastWindow;
    }
}
