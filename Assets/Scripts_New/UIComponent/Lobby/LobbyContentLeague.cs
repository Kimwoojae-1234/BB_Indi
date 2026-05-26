using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyContentLeague : LobbyContentButton
{

    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("RTTS리그 메뉴 버튼 초기화");
    }

    public override void UpdateContent()
    {
        if (isUpdate == false)
        {
            base.UpdateContent();

            //TUTO_STEP -> 특정조건 버튼비활성화
            /*if (lobbyStep <= 3) //LobbyFirstTuto 이거일듯
            {
                ButtonDisable();
            }*/
            
            isUpdate = true;
        }
    }


    public override void OnClickButton()
    {
        base.OnClickButton();
        KOBManager.UI.OpenWindow<UI_RTTS>().LastWindow = LastWindow;
    }
}
