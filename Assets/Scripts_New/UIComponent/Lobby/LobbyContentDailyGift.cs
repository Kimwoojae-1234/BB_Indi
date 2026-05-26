using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyContentDailyGift : LobbyContentButton
{
    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("데일리기프트 버튼 초기화");
    }

    public override void OnClickButton()
    {
        base.OnClickButton();
    }
}
