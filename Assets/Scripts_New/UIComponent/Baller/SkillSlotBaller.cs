using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotBaller : MonoBehaviour
{
    [SerializeField] private Image Frame;
    [SerializeField] private GameObject AddObj;
    [SerializeField] private Image LockFrame;
    [SerializeField] private TextMeshProUGUI LockLvTxt;
    [SerializeField] private Image SkillFrame;
    [SerializeField] private TextMeshProUGUI SkillLvTxt;
    [SerializeField] private Image SkillIcon;
    [SerializeField] private Image Select;
    [SerializeField] private TextMeshProUGUI SkillTypeTxt;

    public int SkillIndex;
    public int Type;
    public int Slot;
    public int Level;

    public void SetEmpty(int slot, int type)
    {
        TypeSetting(0, slot, type, 1);
        Level = 0;
    }


    public void SetLock(int slot, int type, int lv, KOBRarity rarity)
    {
        TypeSetting(0, slot, type, 2);
        Level = 0;
        LockLvTxt.text = lv.ToString();
        LockFrame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "LevelFrame_" + rarity.ToString());
    }

    public void SetSkill(int skillIndex, int slot, int type,  int skillLv)
    {
        TypeSetting(skillIndex, slot, type, 3);
        Level = skillLv;
        SkillFrame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UISkill, "skilltype" + type);
        SkillLvTxt.text = "LV" + skillLv;
        SkillIcon.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UISkill, "s" + skillIndex);
    }

    private void TypeSetting(int skillIndex, int slot, int type, int state)
    {
        SkillIndex = skillIndex;
        Slot = slot;
        Type = type;
        Frame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UISkill, "skillback" + type);        
        AddObj.gameObject.SetActive(state == 1 ? true : false);
        LockFrame.gameObject.SetActive(state == 2 ? true : false);
        SkillFrame.gameObject.SetActive(state == 3 ? true : false);
        if(SkillTypeTxt != null)
        {
            SkillTypeTxt.color = (state == 3 ? Color.white : Color.gray);
            SkillTypeTxt.text = (type == 1 ? "H SKILL" : "F SKILL");
        }
    }


    public void SetList(int skillIndex, int slot, int type,  int skillLv)
    {
        SkillIndex = skillIndex;
        Slot = slot;
        Type = type;
        Level = skillLv;
        Select.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UISkill, "skill_select" + type);
        Frame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UISkill, "skillback" + type);
        SkillFrame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UISkill, "skilltype" + type);
        SkillIcon.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UISkill, "s" + skillIndex);
        if (skillLv == 0)
        {
            SkillFrame.color = KOBUtil.ConvertColor(0x2E2E2E);
            Select.gameObject.SetActive(false);
            SkillLvTxt.gameObject.SetActive(false);
        }
        else
        {
            SkillFrame.color = Color.white;
            Select.gameObject.SetActive(false);
            SkillLvTxt.gameObject.SetActive(true);
            SkillLvTxt.text = "LV" + skillLv;
        }
    }


    public void SetSelect(int select)
    {
        Select.gameObject.SetActive(select == Slot ? true : false);
    }




    /// <summary>
    /// 볼러 UI에서 터치한 경우
    /// </summary>
    public void OnClickTouch()
    {
        UI_Ballers baller = KOBManager.UI.GetUIWindow<UI_Ballers>();
        if (baller != null) baller.OpenSkillPopup(Slot);


        //
    }

    /// <summary>
    /// 스켈 팝업에서 터치한 경우
    /// </summary>
    public void OnClickTouchSkillPopup()
    {

    }


}
