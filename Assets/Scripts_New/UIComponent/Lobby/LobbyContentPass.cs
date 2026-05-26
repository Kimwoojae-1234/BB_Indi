using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyContentPass : LobbyContentButton
{
    [SerializeField] private Slider PassSlider;
    [SerializeField] private Image PassNoti;
    [SerializeField] private TextMeshProUGUI PassNotiText;
    

    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("배틀패스 버튼 초기화");
    }

    public override void OnClickButton()
    {
        base.OnClickButton();
        KOBManager.UI.OpenWindow<UI_Pass>().LastWindow = LastWindow;
    }
}
