using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using System;

public class RttsRewardComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI WinText;
    [SerializeField] private Slider RemainSlider;
    [SerializeField] private RewardItem rewardItem;
    [SerializeField] private TextMeshProUGUI ReaminWinText;
    [SerializeField] private GameObject RemainObj;


    public void InitComp()
    {
        int CurWin = KOBManager.Rtts.CurrentWinDrawLose(0)[0];//
        WinText.text = CurWin.ToString();

        int nextWin =0;
        foreach (KeyValuePair<int, int[]> pair in KOBManager.Rtts.RttsRewardList)
        {
            int key = pair.Key;
            if(CurWin < key)
            {
                nextWin = key;                
                break;
            }
        }

        int remain = nextWin - CurWin;

        if (remain <= 0) //더이상 보상이 없는 경우
        {
            rewardItem.gameObject.SetActive(false);
            RemainObj.gameObject.SetActive(false);
            RemainSlider.value = 1;
        }
        else
        {       
            //몇경기 남음
            RemainObj.gameObject.SetActive(true);
            ReaminWinText.text = string.Format("{0} More Win(s)", remain);
                        
            //슬라이더 
            int lower = 0;
            int upper = KOBManager.Rtts.RewardWinList[0];
            for (int i = 0; i < KOBManager.Rtts.RewardWinList.Count - 1; i++)
            {
                if (KOBManager.Rtts.RewardWinList[i] <= CurWin && CurWin <= KOBManager.Rtts.RewardWinList[i + 1])
                {
                    lower = KOBManager.Rtts.RewardWinList[i];
                    upper = KOBManager.Rtts.RewardWinList[i + 1];
                }
            }
            int total = upper - lower;
            RemainSlider.value = (float)(total-remain) / (float)(total);

            //보상아이콘
            rewardItem.gameObject.SetActive(true);
            int[] reward = KOBManager.Rtts.RttsRewardList[upper]; //보상정보
            rewardItem.SetItem(reward[1], reward[2], false);
        }

    }

}
