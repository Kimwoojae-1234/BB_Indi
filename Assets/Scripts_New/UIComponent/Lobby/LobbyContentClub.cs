using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyContentClub : LobbyContentButton
{

    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("클럽 버튼 초기화");
    }

    public override void OnClickButton()
    {
        base.OnClickButton();
    }
}
