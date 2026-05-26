using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class BallerPowerComponent : MonoBehaviour
{
    public enum UpgradeState
    { 
        Upgrade = 0,
        Info, 
        UnlockNow
    }
    [Header("[볼러카드]")]
    [SerializeField] private Image [] CardFrame;
    [SerializeField] private Image Portrait;

    [Header("[볼러정보]")]
    [SerializeField] private TextMeshProUGUI BallerLv;
    [SerializeField] private TextMeshProUGUI CardTxt;
    [SerializeField] private Slider CardSlider;
    [SerializeField] private GameObject UpgradeArrow;

    [Header("[스탯정보]")]
    [SerializeField] private TextMeshProUGUI[] HittingTxt;
    [SerializeField] private TextMeshProUGUI[] PhisicalTxt;
    [SerializeField] private TextMeshProUGUI[] SpSkillTxt;
    [SerializeField] private Image SpSkillIcon;

    [Header("[버튼]")]
    [SerializeField] private GameObject UpgradeBtn;
    [SerializeField] private GameObject InfoBtn;
    [SerializeField] private GameObject UnlockBtn;

    [SerializeField] private TextMeshProUGUI UpgradeFeeTxt;
    [SerializeField] private TextMeshProUGUI UnlockFeeTxt;



    private UpgradeState State;
    private int CurIdx;
    private int CurLevel;

    public void CollectionSetting(CharacterData ballerData, KOBBaller ballerInfo)
    {
        SetCard(ballerData);
        BallerLv.text = string.Format("Power <size=65>{0}</size>           <size=40><color=#FFB200>MAX13</size></color>", ballerInfo.level);
        CurIdx = ballerData.char_idx;
        CurLevel = ballerInfo.level;        
        int CardNeed = KOBManager.Backend.Chart.UpgradeData.UpgradeCard(ballerInfo.level + 1, ballerData.rarity);
        int curCard = ballerInfo.card_number;

        CardTxt.text = string.Format("{0}/{1}", curCard, CardNeed);
        if (curCard >= CardNeed)
        {
            CardSlider.value = 1;
            UpgradeSetting(ballerData, CurLevel);
        }
        else
        {
            CardSlider.value = (float)curCard / (float)CardNeed;
            InfoSetting(ballerData, CurLevel);
        }
    }

    public void UpgradeSetting(CharacterData ballerData, int lv)
    {
        State = UpgradeState.Upgrade;
        UpgradeBtn.gameObject.SetActive(true);
        InfoBtn.gameObject.SetActive(false);
        UnlockBtn.gameObject.SetActive(false);
        UpgradeArrow.gameObject.SetActive(true);
        SetStateInfo(ballerData, lv, true, false);

        int needGold = KOBManager.Backend.Chart.UpgradeData.UpgradeGold(lv + 1, ballerData.rarity);
        UpgradeFeeTxt.text = needGold.ToString("N0");
    }

    public void InfoSetting(CharacterData ballerData, int lv)
    {
        State = UpgradeState.Info;
        InfoBtn.gameObject.SetActive(true);
        UpgradeBtn.gameObject.SetActive(false);        
        UnlockBtn.gameObject.SetActive(false);
        UpgradeArrow.gameObject.SetActive(false);
        SetStateInfo(ballerData, lv, false, true);
    }

    public void UnlockSetting(CharacterData ballerData)
    {
        SetCard(ballerData);
        CurIdx = ballerData.char_idx;
        CurLevel = 1;
        BallerLv.text = string.Format("Power <size=65>{0}</size>              <size=40><color=#FFFF00>MAX13</size></color>", 1);
        CardSlider.value = 0;
        CardTxt.text = string.Empty;
        State = UpgradeState.UnlockNow;
        UnlockBtn.gameObject.SetActive(true);
        InfoBtn.gameObject.SetActive(false);
        UpgradeBtn.gameObject.SetActive(false);
        UpgradeArrow.gameObject.SetActive(false);
        SetStateInfo(ballerData, 1, false, true);
    }


    private void SetCard(CharacterData ballerData)
    {
        Color color = KOBUtil.GetRarityColor(ballerData.rarity);
        Color darkcolor = color * (0.6f);
        CardFrame[0].color = color;
        CardFrame[1].color = color;
        CardFrame[2].color = darkcolor;

        KOBManager.Resource.LoadBallerPortrait(Portrait, ballerData.char_idx);
    }


    private void SetStateInfo(CharacterData ballerData, int level, bool bUp, bool bMax)
    {
        //선수수정할것 - 수정함
        int char_idx = ballerData.char_idx;
        HitterLevelData levelData = KOBManager.Backend.Chart.HitterLevelData.GetData(char_idx, level);
        HitterLevelData maxLevelData = KOBManager.Backend.Chart.HitterLevelData.GetData(char_idx, KOBConstant.MAX_LEVEL);
        HitterLevelData nextLevelData = null;
        if (level < KOBConstant.MAX_LEVEL)
        {
            nextLevelData = KOBManager.Backend.Chart.HitterLevelData.GetData(char_idx, level + 1);
        }
        HitterSkillData skillData = KOBManager.Backend.Chart.HitterSkillData.GetData(char_idx);

        //Hitting
        int hittinigValue = levelData.power + levelData.contact + levelData.vision;
        HittingTxt[0].text = hittinigValue.ToString();
        //Physic
        int PhysicValue = levelData.fielding + levelData.throwing + levelData.speed;
        PhisicalTxt[0].text = PhysicValue.ToString();

        //스킬
        if (skillData != null && skillData.special_skill > 0)
        {
            int skLv = 0;
            SpSkillIcon.gameObject.SetActive(true);
            int key = skillData.special_skill;
            //int[] Value = skill.Value;
            int nextLevel = KOBConstant.MAX_LEVEL;
            int activateLevel = skillData.special_unlock[0];
            for (int i = 0; i < skillData.special_unlock.Length; i++)
            {
                if (level <= skillData.special_unlock[i] && skillData.special_unlock[i] > 0)
                {
                    nextLevel = skillData.special_unlock[i];
                    break;
                }
                skLv++;
            }

            if (level < activateLevel) //비활성화
            {
                SpSkillTxt[0].text = "Skill" + key;   //이름 임시
                SpSkillTxt[0].color = Color.gray;
                SpSkillTxt[1].gameObject.SetActive(false);
                SpSkillTxt[2].gameObject.SetActive(true);
                SpSkillTxt[2].text = string.Format("AVAILABLE AT\n<size=30>POWER{0}</size>", activateLevel);
            }
            else //활성화
            {
                SpSkillTxt[0].text = "Skill" + key + " LV " + skLv;   //이름 임시
                SpSkillTxt[0].color = Color.white;
                if (level == nextLevel) //업글 가능
                {

                }
                else //업글은 불가능
                {
                    SpSkillTxt[2].gameObject.SetActive(true);
                    SpSkillTxt[2].text = string.Format("UPGRADE AT\n<size=30>POWER{0}</size>", nextLevel);
                }
            }
            //"AVAILABLE AT\nPOWER{0}"
            //"AVAILABLE AT\nPOWER{0}"
            //"UPGRADE AT\nPOWER{0}"
        }
        else
        {
            SpSkillTxt[0].text = "No special skill";
            SpSkillTxt[0].color = Color.gray;
            SpSkillIcon.gameObject.SetActive(false);
            SpSkillTxt[1].gameObject.SetActive(false);
            SpSkillTxt[2].gameObject.SetActive(false);
        }


        if (bUp == true && level < KOBConstant.MAX_LEVEL)
        {
            HittingTxt[1].gameObject.SetActive(true);
            PhisicalTxt[1].gameObject.SetActive(true);
            SpSkillTxt[1].gameObject.SetActive(true);

            int nexthittinigValue = (nextLevelData.power + nextLevelData.contact + nextLevelData.vision) - hittinigValue;
            HittingTxt[1].text = "+" + nexthittinigValue;

            int nextPhysicValue = (nextLevelData.fielding + nextLevelData.throwing + nextLevelData.speed) - PhysicValue;
            PhisicalTxt[1].text = "+" + nextPhysicValue;
        }
        else
        {
            HittingTxt[1].gameObject.SetActive(false);
            PhisicalTxt[1].gameObject.SetActive(false);
            SpSkillTxt[1].gameObject.SetActive(false);
        }

        if(bMax == true && level < KOBConstant.MAX_LEVEL)
        {
            HittingTxt[2].gameObject.SetActive(true);
            PhisicalTxt[2].gameObject.SetActive(true);
            //SpSkillTxt[1].gameObject.SetActive(true);
            int hittinigMax = maxLevelData.power + maxLevelData.contact + maxLevelData.vision;
            int PhysicMax = maxLevelData.fielding + maxLevelData.throwing + maxLevelData.speed;
            HittingTxt[2].text = string.Format("MAX\n<size=45>{0}</size>", hittinigMax);
            PhisicalTxt[2].text = string.Format("MAX\n<size=45>{0}</size>", PhysicMax);
        }
        else
        {
            HittingTxt[2].gameObject.SetActive(false);
            PhisicalTxt[2].gameObject.SetActive(false);
            //SpSkillTxt[1].gameObject.SetActive(false);
        }

    }




    public void OnClickButton()
    {
        Debug.Log("OnClikcButton");
        if(State == UpgradeState.Upgrade)
        {
            Popup_StatUpgrade popup = KOBManager.Popup.OpenPopup<Popup_StatUpgrade>();
            popup.UpgradeSetting(CurIdx, CurLevel, (int UpdatedIDX) =>
            {
                if (UpdatedIDX >= 0)
                {
                    BallerUpgradeAction(UpdatedIDX);
                }
            });
        }
        else if (State == UpgradeState.Info)
        {
            Popup_StatUpgrade popup = KOBManager.Popup.OpenPopup<Popup_StatUpgrade>();
            popup.NormalSetting(CurIdx, CurLevel);
        }
        else //if (State == UpgradeState.UnlockNow)
        {
            Popup_StatUpgrade popup = KOBManager.Popup.OpenPopup<Popup_StatUpgrade>();
            popup.UnlockSetting(CurIdx, 1, (int UpdatedIDX) =>
            {
                if (UpdatedIDX >= 0)
                {
                    BallerUpgradeAction(UpdatedIDX);
                }
            });
        }
    }


    private void BallerUpgradeAction(int idx)
    {
        KOBManager.UI.GetUIWindow<UI_Ballers>().BallerUpgradeAction(idx);
    }

}
