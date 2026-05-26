using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public enum TutoStep
    {
        FirstTuto = 0,      //첫튜토리얼 수행
        NickNameSetting,    //닉네임 세팅
        LobbyFirstTuto,     //로비 첫 튜토
    }




    public bool IsTuroialComplete(TutoStep step)
    {
        if(KOBManager.MyInfo.GameData.TutoInfo.ContainsKey(step))
        {
            return KOBManager.MyInfo.GameData.TutoInfo[step];
        }
        else
        {
            return false;   
        }
    }


    public void SetTutorialComplete(TutoStep step, Action<bool> action)
    {
        TRequestTutoStep req = new TRequestTutoStep()
        { 
            Step = step,
        };

        KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
        {
            if (callback?.IsSuccess() == true)
            {
                //성공
                action?.Invoke(true);
            }
            else
            {
                //실패
                action?.Invoke(false);
            }
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
        });
    }

}
