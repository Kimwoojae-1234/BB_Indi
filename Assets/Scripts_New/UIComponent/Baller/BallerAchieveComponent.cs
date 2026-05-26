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


        Dictionary<int, int> AchievementList = KOBManager.MyInfo.GameData.GrowthInfo.AchievementList[BallerIdx];
        int rawValue = GetRawValue(BallerIdx, data.rIndex);

        //정식
        if (AchievementList.ContainsKey(idx) == true)
        {
            Lv = AchievementList[idx];
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

        //테스트
        /*Lv = Random.Range(0, 4);
        rawValue = 0;
        for (int i = 0; i < Lv; i++)
        {
            rawValue += (data.count + (i * data.next_count));
        }
        int random = Random.Range(0, 50);
        rawValue += random;
        //Debug.Log("idx : " + idx + " // Lv : " + Lv + " // random : " + random + " // rawValue : " + rawValue);
        curMount = rawValue;
        for (int i = 0; i < Lv; i++)
        {
            curMount -= (data.count + (i * data.next_count));
        }
        achiveMount = data.count + Lv * data.next_count;
        curReward = data.reward + Lv * data.add_reward;
        //테스트 여기까지*/


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
            processSlider.value = (float)curMount / (float)achiveMount;
            processText.text = string.Format("{0}/{1}", curMount, achiveMount);
        }

        AchiveIcon.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UITier, string.Format("achieve_icon_{0}", idx));
        AchiveIcon.SetNativeSize();
        //언어팩
        AchiveName.text = data.name_id + " Lv" + (Lv + 1);
        AchiveDesc.text = data.desc_id + " " + achiveMount;
    }


    /// <summary>
    /// 해당 업적 인덱스에 대응하는 기록을 불러오는 메쏘드
    /// rIndex는 차트에서 해당 기록에 대응하는 인덱스를 설정해둘것
    /// </summary>
    private int GetRawValue(int BallerIdx, int rIndex)
    {
        /*Dictionary<int, int[]> BallerStat = KOBManager.MyInfo.GameData.GrowthInfo.BallerStat[BallerIdx]; //
        int value = 0;
        foreach (KeyValuePair<int, int[]> item in BallerStat)
        {
            value += item.Value[rIndex];
        }
        return value;*/
        return 0;
    }


    public void OnClickClaim()
    {
        Debug.Log("OnClickClaim");
    }
}
