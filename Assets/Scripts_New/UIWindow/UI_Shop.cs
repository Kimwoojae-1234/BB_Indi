using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Shop : UIWindow
{

    [SerializeField] private HorizontalLayoutGroup group;

    [SerializeField] private LobbyPropertyComponent UserProperty;


    public override void Initialize()
    {
        base.Initialize();
    }


    public override void OpenWindow()
    {
        base.OpenWindow();

        PropertySetting();

        //Debug.Log("===============>> 로비 상점");
        group.gameObject.SetActive(true);

    }


    public void PropertySetting()
    {
        //LocalLobbyInfo lastLobbyInfo = KOBManager.MyInfo.LocalLobbyInfo;
        //UserProperty.SetUserProperty(lastLobbyInfo, null);
    }

    private void SetShopItems()
    {
    }
}
