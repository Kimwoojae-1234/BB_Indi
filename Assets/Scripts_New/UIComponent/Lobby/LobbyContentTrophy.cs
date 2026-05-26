using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyContentTrophy : LobbyContentButton
{
    [Header("[트로피 버튼 전용]")]
    [SerializeField] private Slider TrophySlider;
    [SerializeField] private GameObject TierObj;
    [SerializeField] private Image TierImg;
    [SerializeField] private RewardItem rewardItem;

    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("트로피 버튼 초기화");

        
    }


    public override void UpdateContent()
    {
        if (isUpdate == false)
        {
            base.UpdateContent();

            //트로피 체크
            int CurrentTrophy = KOBManager.MyInfo.GameData.GrowthInfo.Trophy;
            BtnText.text = CurrentTrophy.ToString("N0");

            //티어는 맥스 트로피로 체크할 것
            int CurrentMaxTrophy = KOBManager.MyInfo.GameData.GrowthInfo.MaxTrophy;
            LobbyTrophyInfo info = KOBManager.Backend.Chart.TrophyRoadData.GetLobbyTrophyInfo(CurrentMaxTrophy);

            if (info != null)
            {
                //현재 티어
                int CurTier = info.Tier; 
                TierObj.gameObject.SetActive(CurTier > 0);
                if (CurTier > 0) KOBManager.Atlas.SetTierSprite(TierImg, CurTier);

                //노티
                int NotiCount = info.noti;
                if (NotiSpr != null)
                {
                    NotiSpr.gameObject.SetActive(NotiCount > 0);
                    if (NotiCount > 0) NotiText.text = NotiCount.ToString();
                }

                //보상
                if (info.isMaxTier == true && NotiCount == 0)
                {
                    //현재 트로피 맥스고, 보상 다 받음
                    rewardItem.gameObject.SetActive(false);
                    //슬라이더
                    TrophySlider.value = 1;
                }
                else
                { 
                    bool isAquire = (info.slide == 1);
                    rewardItem.SetItem(info.reward_index, info.pindex, isAquire);
                    //슬라이더
                    TrophySlider.value = info.slide;
                }
            }

            if (LockObj != null) LockObj.gameObject.SetActive(false);

            isUpdate = true;
        }
    }


    public override void OnClickButton()
    {
        base.OnClickButton();
        KOBManager.UI.OpenWindow<UI_TrophyRoad>().LastWindow = LastWindow;
    }

}

public class LobbyTrophyInfo
{
    public int noti;
    public int Tier;
    public float slide;
    public int reward_index;
    public int pindex;
    public bool isMaxTier;
}