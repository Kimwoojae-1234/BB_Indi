using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BallerAchieveComponent : MonoBehaviour
{
    [SerializeField] private Image Bg;
    [SerializeField] private Image AchiveIcon;
    [SerializeField] private TextMeshProUGUI AchiveName;
    [SerializeField] private TextMeshProUGUI AchiveDesc;
    [SerializeField] private GameObject[] State;
    [SerializeField] private GameObject Noti;

    //프로세스
    [SerializeField] private Slider processSlider;
    [SerializeField] private TextMeshProUGUI processText;
    [SerializeField] private TextMeshProUGUI processRewardGem;
    //클레임
    [SerializeField] private TextMeshProUGUI claimRewardGem;



    

    public void Init(AchievementData data, int BallerIdx)
    {
        int Lv = 0;
        int curMount = 0;
        int achiveMount = 0;
        int curReward = 0;

        int idx = data.idx;
        Noti.gameObject.SetActive(false);


        KOBManager.MyInfo.GameData.GrowthInfo.AchievementList.TryGetValue(BallerIdx, out var AchievementList);
        int rawValue = GetRawValue(BallerIdx, data.rIndex);

        //정식
        if (AchievementList != null && AchievementList.ContainsKey(idx))
        {
            Lv = Mathf.Max(0, AchievementList[idx]);
            curMount = rawValue;
            for (int i = 0; i < Lv; i++)
            {
                curMount -= (data.count + (i * data.next_count));
            }
            achiveMount = data.count + Lv * data.next_count;
            curReward = data.reward + Lv*data.add_reward;
        }
        else
        {
            curMount = rawValue;
            achiveMount = data.count;
            curReward = data.reward;
        }




        curMount = Mathf.Max(0, curMount);
        achiveMount = Mathf.Max(1, achiveMount);
        bool bClaim = (curMount >= achiveMount ? true : false);

        if (bClaim == true) //클레임
        {
            Bg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_ListFrame07_s");
            State[0].gameObject.SetActive(false);
            State[1].gameObject.SetActive(true);
            claimRewardGem.text = curReward.ToString();
        }
        else //프로세스
        {
            Bg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_ListFrame07_n");
            State[0].gameObject.SetActive(true);
            State[1].gameObject.SetActive(false);            
            processRewardGem.text = curReward.ToString();
            processSlider.value = Mathf.Clamp01((float)curMount / achiveMount);
            processText.text = string.Format("{0}/{1}", curMount, achiveMount);
        }

        AchiveIcon.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UITier, string.Format("achieve_icon_{0}", idx));
        AchiveIcon.SetNativeSize();
        //언어팩
        UILocalization.Set(AchiveName, "baller.achievement.level", UILocalization.Get(data.name_id), Lv + 1);
        UILocalization.Set(AchiveDesc, data.desc_id, achiveMount);
    }


    /// <summary>
    /// 해당 업적 인덱스에 대응하는 기록을 불러오는 메쏘드
    /// rIndex는 차트에서 해당 기록에 대응하는 인덱스를 설정해둘것
    /// </summary>
    private int GetRawValue(int BallerIdx, int rIndex)
    {
        var progress = KOBManager.MyInfo.GameData.GrowthInfo.BallerAchievementProgress;
        return progress.TryGetValue(BallerIdx, out var totals) && totals != null && totals.TryGetValue(rIndex, out int value)
            ? Mathf.Max(0, value) : 0;
    }


    public void OnClickClaim()
    {
        KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(UILocalization.Get("baller.achievement.reward.pending"));
    }
}
