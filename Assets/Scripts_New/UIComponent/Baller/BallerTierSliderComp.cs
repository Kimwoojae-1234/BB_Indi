using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BackEnd;
using static Popup_Promotion;

public class BallerTierSliderComp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI trophyTxt;
    [SerializeField] private Image TierImg;
    [SerializeField] private Slider TierSlider;

    // Start is called before the first frame update
    public void Set(KOBBaller baller, bool isMaxShow = true)
    {
        gameObject.SetActive(true);

        BallerTierInfo tierInfo = KOBManager.Backend.Chart.BallerTrophyRoadData.GetTierInfo(baller.baller_trophy);
        //Debug.Log("ballerTier : " + tierInfo.CurTier);
        //Debug.Log("curTrophy : " + tierInfo.CurTrophy + "          nextTrophy : " + tierInfo.NextTrophy);
        if (tierInfo.isMax == true)
        {
            trophyTxt.text = "MAX";
        }
        else
        {
            if (isMaxShow)
            {
                trophyTxt.text = string.Format("{0}/{1}", baller.baller_trophy, tierInfo.NextTrophy);// ballerInfo.baller_trophy.ToString(); //트로피텍스트
            }
            else
            {
                trophyTxt.text = baller.baller_trophy.ToString();
            }
        }
        KOBManager.Atlas.SetBallerTierSprite(TierImg, tierInfo.CurTier);
        TierSlider.value = (float)baller.baller_trophy / (float)tierInfo.NextTrophy;
    }



    public void SetGainProcess(KOBBaller baller, int GainNum, float delay, System.Action action = null)
    {
        bool isTierUpgrade = false;

        gameObject.SetActive(true);

        int curValue = baller.baller_trophy - GainNum;
        int target = baller.baller_trophy;

        BallerTierInfo tierInfo = KOBManager.Backend.Chart.BallerTrophyRoadData.GetTierInfo(curValue);
        KOBManager.Atlas.SetBallerTierSprite(TierImg, tierInfo.CurTier);
        if (tierInfo.isMax == true)
        {
            trophyTxt.text = "MAX";
            TierSlider.value = 1;
        }
        else
        {
            trophyTxt.text = string.Format("{0}/{1}", curValue, tierInfo.NextTrophy);
            TierSlider.value = (float)curValue / (float)tierInfo.NextTrophy;
        }

        if (GainNum > 0)
        {                        
            DOTween.To(() => curValue, x =>
            {
                curValue = x;
                if (tierInfo.isMax == true)
                {
                    trophyTxt.text = "MAX";
                    TierSlider.value = 1;
                }
                else
                {
                    trophyTxt.text = string.Format("{0}/{1}", curValue, tierInfo.NextTrophy);
                    TierSlider.value = (float)curValue / (float)tierInfo.NextTrophy;
                }

                if(curValue > tierInfo.NextTrophy)
                {
                    Debug.Log("티어 : 연출 도중 티어 바뀐 경우!!");
                    isTierUpgrade = true;
                    tierInfo = KOBManager.Backend.Chart.BallerTrophyRoadData.GetTierInfo(curValue + 10);
                    KOBManager.Atlas.SetBallerTierSprite(TierImg, tierInfo.CurTier);
                }

            }, target, 1.5f)
            .SetDelay(delay)
            .OnComplete(() =>
            {
                //Debug.Log("애니메이션 완료!");
                if (baller.baller_trophy >= tierInfo.NextTrophy ||
                    isTierUpgrade == true)
                {
                    if (action != null)
                    {
                        action();
                    }
                }
            });
        }
        
    }



}
