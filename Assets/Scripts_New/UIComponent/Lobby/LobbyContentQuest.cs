using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyContentQuest : LobbyContentButton
{

    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("퀘스트 미션 버튼 초기화");
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
            }
            else
            {

            }*/
            isUpdate = true;    
        }
    }

    public override void OnClickButton()
    {
        base.OnClickButton();
    }
}
