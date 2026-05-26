using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ui_RewardAcquisitionDirection : UIFrontUI
{
    [SerializeField]
    private Ui_RewardDirectionItem[] directionItems;

    [SerializeField]
    private float itembatchDelayTime;

    [SerializeField]
    private AnimationCurve lerpCurve;
    [SerializeField]
    private AnimationCurve alphaCurve;

    int list_index = 0;
    bool playing = false;
    bool notForceCloseSetting = false;
    

    public override void Open()
    {
        if (!this.playing)
        {
            this.playing = true;
            base.Open();
            list_index = 0;            
            RectTransform rectTrs = this.transform as RectTransform;
            rectTrs.SetAsLastSibling();
        }
    }
#if UNITY_EDITOR
    public void Clear()
    {
        this.playing = false;
    }
#endif
    public void StartDirection(KOBReward type, Vector3 firstPos, Vector3 finalPos)
    {
        notForceCloseSetting = false;
        /*
        if (list_index >= GameConfig.RewardDirectionCount())
        {
            this.playing = false;
            GameConfig.RewardDirectionListClear();
            MainManager.UI.CloseWindow<Ui_RewardAcquisitionDirection>();
            return;
        }*/


        StartCoroutine(PlayCoinDirection(type, firstPos, finalPos));

        
    }

 
     
    private IEnumerator PlayCoinDirection(KOBReward type, Vector3 firstPos, Vector3 finalPos)
    {
        //yield return new WaitForSeconds(1.5f + itembatchDelayTime);

        Vector3 FirstPosition = firstPos;// Vector3.zero;// MainManager.UI.GetUIWindow<UI_LobbyWindow>().GetLobbyGamePlayButtonWorldPosition();
        Vector3 FinalPosition = finalPos;// new Vector3(0, 400, 0);// MainManager.UI.GetUIWindow<UI_LobbyWindow>().GetTopMyWealthBarCoinWorldPosition();
        //MainManager.Sound.PlayOneShotSound("Coins Absorb Sound");
        for (int i = 0; i<directionItems.Length; i++)
        {
            yield return new WaitForSeconds(itembatchDelayTime);
            if (i == directionItems.Length-1)
            {
                list_index++;
                directionItems[i].SetRewardItem(type, FirstPosition, FinalPosition, lerpCurve, alphaCurve, Close);
            }
            else
            {
                directionItems[i].SetRewardItem(type, FirstPosition, FinalPosition, lerpCurve, alphaCurve);
            }
            
        }
        yield return new WaitForSeconds(itembatchDelayTime);
    }


    public void ForceCloseDirectionWindow()
    {
        /*for (int i = 0; i < directionItems.Length; i++)
        {
            directionItems[i].ResetDirectionItem();
        }
        this.playing = false;
        GameConfig.RewardDirectionListClear();
        base.CloseWindow();
        MainManager.SendEvent(GameDefine.eEvent.CurrencyUpdate, null);*/
    }

    public void ActiveAllDirectionItems(bool active)
    {
        for (int i = 0; i < directionItems.Length; i++)
        {
            directionItems[i].gameObject.SetActive(active);
        }
    }

    public override void Close()
    {
        
        base.Close();
        
        this.playing = false;
        //MainManager.SendEvent(GameDefine.eEvent.CurrencyUpdate, null);
    }

}
