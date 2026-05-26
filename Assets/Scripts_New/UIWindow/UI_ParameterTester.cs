using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BackEnd;

public class UI_ParameterTester : UIWindow
{
    [Header("[밸류 세팅]")]
    [SerializeField] private long AddXP = 0;
    [SerializeField] private long AddGold = 0;
    [SerializeField] private long AddGem = 0;
    [SerializeField] private int AddToken = 0;

    [Header("[텍스트 오브젝트]")]
    [SerializeField] private TextMeshProUGUI TextAddXP = null;
    [SerializeField] private TextMeshProUGUI TextAddGold = null;
    [SerializeField] private TextMeshProUGUI TextAddGem = null;
    [SerializeField] private TextMeshProUGUI TextAddGameToken = null;

    [Header("[동전 테스트]")]
    public ItemAcquireFx prefabItem;
    public Transform target;
    public Transform source;

    public override void OpenWindow()
    {
        base.OpenWindow();

        AddXP = 0;
        AddGold = 0;
        AddGem = 0;

        SettingValue();
    }


    public void UpdateValue()
    {
        if(AddXP != 0)
        {
            //KOBManager.Backend.GameData.KOBUserInfo.XPUpdate(AddXP);
        }
        if(AddGold != 0)
        {
            //KOBManager.Backend.GameData.KOBUserInfo.GoldUpdate(AddGold);
        }
        if (AddGem != 0)
        {
            //KOBManager.Backend.GameData.KOBUserInfo.GemUpdate(AddGem);
        }
        if (AddToken != 0)
        {
            //KOBManager.Backend.GameData.KOBUserInfo.GameTokenUpdate(AddToken);
        }

        KOBManager.Backend.UpdateAllGameData(AfterUpdateFunc);
    }

    private void AfterUpdateFunc(BackendReturnObject callback, TResponseBase response = null)
    {
        if (callback == null)
        {
            Debug.Log("업데이트 할 내용이 없음");
        }
        else
        {
            if (callback.IsSuccess())
            {
                ClickBackButton();
            }
            else
            {
                //실패
            }
        }
    }

    public void SettingValue()
    {
        TextAddXP.text = "AddXP : " + AddXP;
        TextAddGold.text = "AddGold : " + AddGold;
        TextAddGem.text = "AddGem : " + AddGem;
        TextAddGameToken.text = "AddToken : " + AddToken;
    }


}
