using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using TMPro;
using BackEnd;
using System.Linq;

public class Popup_SkillSetting : UIPopup
{

    [SerializeField] private SkillSlotBaller[] SkillSlot;
    [SerializeField] private SkillSlotBaller[] SkillList;
    [SerializeField] private RectTransform origin;
    [SerializeField] private TextMeshProUGUI SkillNameTxt;
    [SerializeField] private TextMeshProUGUI SkillDescTxt;
    [SerializeField] private RectTransform Position;
    [SerializeField] private GameObject LearnBtn;
    [SerializeField] private GameObject UpgradeBtn;
    [SerializeField] private GameObject LockBtn; //누르면 언제 업글 가능한지 알려줘
    [SerializeField] private GameObject NotAvailble;


    //private GameObject BallerObj;
    public delegate void OnCloseEvent();
    private OnCloseEvent CallBack = null;



    private Dictionary<int, SkillListType> HittingSkillList = new Dictionary<int, SkillListType>();
    private Dictionary<int, SkillListType> FieldingSkillList = new Dictionary<int, SkillListType>();
    private int CurSlot = 1;
    private int CurSelect = 1;
    private bool bAvailableBaller;
    private int[] OpenLevel = new int[6];
    private int CurBallerLevel;
    private KOBRarity CurRarity;

    public override void Open()
    {
        base.Open();

        
    }

    public override void Close()
    {
        base.Close();
        if(CallBack != null)
        {
            CallBack();
            CallBack = null;
        }
    }


    public void Setting(GameObject baller, int _idx, int _slot, OnCloseEvent callBack)
    {
        CurSlot = _slot;
        CallBack = callBack;
        baller.transform.parent = origin.transform;
        baller.transform.localScale = Vector3.one;
        baller.transform.localPosition = Vector3.zero;

        CharacterData ballerData = KOBManager.Backend.Chart.CharacterData.GetData(_idx); //고정정보 - 선수고유정보
        CurRarity = ballerData.rarity;
        if (KOBManager.MyInfo.GameData.PlayerInfo.BallerList.ContainsKey(_idx) == true) //소유
        {
            bAvailableBaller = true;
            KOBBaller ballerInfo = KOBManager.MyInfo.GameData.PlayerInfo.BallerList[_idx]; //변동정보 - 유저가 성장
            SkillSlotSetting(ballerData, ballerInfo);

            SkillListSetting(ballerData, ballerInfo);//, SkillSlot[CurSlot - 1].Type);
            CurSelect = GetSelectedSkill();
            ShowSkillList((SlotEquipType)SkillSlot[CurSlot - 1].Type);
        }
        else
        {
            bAvailableBaller = false;
            SkillSlotSetting(ballerData, null);

            CurSelect = 1;
            SkillListSetting(ballerData, null);//, SkillSlot[CurSlot - 1].Type);
            ShowSkillList((SlotEquipType)SkillSlot[CurSlot - 1].Type);
        }

        ShowSkillText();
        ShowBtnState();
        SetCursorPos(SkillSlot[CurSlot - 1].gameObject);
    }


    private int GetSelectedSkill()
    {
        int EquipedSkill = SkillSlot[CurSlot - 1].SkillIndex;
        if(EquipedSkill != 0)
        {
            for(int i = 0; i<SkillList.Length;i++)
            {
                if(SkillList[i].SkillIndex == EquipedSkill)
                {
                    return (i + 1);
                }
            }
        }

        return 1;
    }


    private void SkillSlotSetting(CharacterData ballerData, KOBBaller ballerInfo)
    {
        CurBallerLevel = 0;
        //선수수정할것 - 수정함
        HitterSkillData skillData = KOBManager.Backend.Chart.HitterSkillData.GetData(ballerData.char_idx);
        if (skillData != null)
        {
            int Count = skillData.slot_list.Count;
            if (ballerInfo != null) //보유함
            {
                CurBallerLevel = ballerInfo.level;                
                for (int i = 0; i < SkillSlot.Length; i++)
                {
                    if (i < Count)
                    {
                        SkillSlot[i].gameObject.SetActive(true);
                        int slot = (i + 1);                        
                        int openLv = skillData.slot_list[i];
                        int type = (i % 2 == 0 ? 1 : 2);//이게 개허접하네
                        OpenLevel[i] = openLv;
                        if (CurBallerLevel < openLv)
                        {
                            //잠김
                            SkillSlot[i].SetLock(slot, type, openLv, ballerData.rarity);
                        }
                        else
                        {
                            //bool bEquip = false;
                            int skillIndex = 0;
                            if (ballerInfo.SkillEquip.ContainsKey(slot) == true)
                            {
                                skillIndex = ballerInfo.SkillEquip[slot];
                            }

                            if (skillIndex == 0) //비었음
                            {
                                SkillSlot[i].SetEmpty(slot, type);
                            }
                            else //스킬 있음
                            {
                                int SkillLV = ballerInfo.SkillList[skillIndex].level; //스킬 레벨
                                SkillSlot[i].SetSkill(skillIndex, slot, type,  SkillLV);
                            }
                        }
                    }
                    else
                    {
                        SkillSlot[i].gameObject.SetActive(false);
                    }
                }
            }
            else //볼러 미보유
            {                
                for (int i = 0; i < SkillSlot.Length; i++)
                {
                    if (i < Count)
                    {
                        SkillSlot[i].gameObject.SetActive(true);
                        int slot = (i + 1);
                        int openLv = skillData.slot_list[i];
                        int type = (i % 2 == 0 ? 1 : 2);//이게 개허접하네
                        OpenLevel[i] = openLv;
                        //미보유시 Lock형태
                        SkillSlot[i].SetLock(slot, type, openLv, ballerData.rarity); 
                    }
                    else
                    {
                        SkillSlot[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }



    private void SkillListSetting(CharacterData ballerData, KOBBaller ballerInfo)
    {
        HittingSkillList.Clear();
        FieldingSkillList.Clear();

        if (ballerInfo != null) //보유시
        {
            //일부라도 성장이 되있는 경우
            foreach (KeyValuePair<int, KOBSkill> skill in ballerInfo.SkillList) //info로 부터
            {
                int skillindex = skill.Key;
                KOBSkill value = skill.Value;
                SlotEquipType curType = KOBUtil.GetEquipType(skillindex);
                if (curType == SlotEquipType.HittingSkill)
                {
                    HittingSkillList.Add(skillindex, new SkillListType(skillindex, value.level));
                }
                else if (curType == SlotEquipType.FieldingSkill)
                {
                    FieldingSkillList.Add(skillindex, new SkillListType(skillindex, value.level));
                }
            }
        }

        //선수수정할것- 수정함
        //아직 아무것도 없는 경우
        HitterSkillData skillData = KOBManager.Backend.Chart.HitterSkillData.GetData(ballerData.char_idx);
        if (skillData != null)
        {
            for (int i = 0; i < skillData.skill_list.Count; i++) //data로 부터
            {
                int skillIdx = skillData.skill_list[i];
                SlotEquipType curType = KOBUtil.GetEquipType(skillIdx);
                if (curType == SlotEquipType.HittingSkill)
                {
                    if (HittingSkillList.ContainsKey(skillIdx) == false)
                    {
                        HittingSkillList.Add(skillIdx, new SkillListType(skillIdx, 0));
                    }
                }
                else if (curType == SlotEquipType.FieldingSkill)
                {
                    if (FieldingSkillList.ContainsKey(skillIdx) == false)
                    {
                        FieldingSkillList.Add(skillIdx, new SkillListType(skillIdx, 0));
                    }
                }
            }
        }

        //skill index 오름차순 정렬
        var sortVar1 = from item in HittingSkillList
                      orderby item.Value.idx ascending
                      select item;
        sortVar1.ToDictionary(x => x.Key, x => x.Value);

        var sortVar2 = from item in FieldingSkillList
                       orderby item.Value.idx ascending
                       select item;
        sortVar2.ToDictionary(x => x.Key, x => x.Value);


        Debug.Log("CurSlot : " + CurSlot);
    }

    private void ShowSkillList(SlotEquipType type)
    {
        int count = 0;
        for (int i = 0; i < SkillList.Length; i++) SkillList[i].gameObject.SetActive(false);
        if (type == SlotEquipType.HittingSkill)
        {
            foreach (KeyValuePair<int, SkillListType> list in HittingSkillList)
            {
                int skillindex = list.Key;
                int Slot = (count + 1);

                SkillList[count].gameObject.SetActive(true);
                SkillList[count].SetList(skillindex, Slot, 1,  list.Value.level);
                SkillList[count].SetSelect(CurSelect);
                count++;
            }
        }
        else if (type == SlotEquipType.FieldingSkill)
        {
            foreach (KeyValuePair<int, SkillListType> list in FieldingSkillList)
            {
                int skillindex = list.Key;
                int Slot = (count + 1);
                SkillList[count].gameObject.SetActive(true);
                SkillList[count].SetList(skillindex, Slot, 2, list.Value.level);
                SkillList[count].SetSelect(CurSelect);
                count++;
            }
        }
    }

    private void ChangeSkillList()
    {
        for (int i = 0; i < SkillList.Length; i++)
        {
            SkillList[i].SetSelect(CurSelect);
        }
    }


    private void SetCursorPos(GameObject obj)
    {
        float newX = obj.transform.position.x;
        float newY = Position.transform.position.y;
        Position.transform.position = new Vector2(newX, newY);
    }


    private void ShowSkillText()
    {
        int CurSkillIndex = SkillList[CurSelect - 1].SkillIndex;
        int CurLevel = SkillList[CurSelect - 1].Level;
        SkillData skillData = KOBManager.Backend.Chart.SkillData.GetData(CurSkillIndex);

        int level = 0;
        if (CurLevel == 0)
        {
            SkillNameTxt.text = KOBManager.Localization.GetUILocalizedValue2(skillData.name_id);
        }
        else
        {
            SkillNameTxt.text = string.Format("{0} LV{1}", KOBManager.Localization.GetUILocalizedValue2(skillData.name_id), CurLevel);
            level = CurLevel - 1;
        }

        int[] value = new int[3];
        int[] Per = new int[3];
        for(int i = 0; i< skillData.level_value.Count; i++)
        {
            value[i] = skillData.level_value[i].Val[level];
            Per[i] = skillData.level_value[i].Per[level];
        }

        SkillDescTxt.text = string.Format(KOBManager.Localization.GetUILocalizedValue2(skillData.desc_id), value[0], Per[0], value[1], Per[1], value[2], Per[2]);
    }


    private void ShowBtnState()
    {
        LearnBtn.gameObject.SetActive(false);
        UpgradeBtn.gameObject.SetActive(false);
        LockBtn.gameObject.SetActive(false);
        NotAvailble.gameObject.SetActive(false);

        int openLv = OpenLevel[CurSlot - 1];
        if (bAvailableBaller == true)
        {
            if(CurBallerLevel < openLv)
            {
                NotAvailble.gameObject.SetActive(true);
                TextMeshProUGUI t = NotAvailble.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                t.color = KOBUtil.GetRarityColor(CurRarity);
                t.text = string.Format("<color=white>Available to learn after reaching</color> POWER LEVEL {0}", openLv);
            }
            else
            {

            }
        }
        else
        {            
            NotAvailble.gameObject.SetActive(true);
            TextMeshProUGUI t = NotAvailble.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            t.color = KOBUtil.GetRarityColor(CurRarity);
            t.text = string.Format("<color=white>Available to learn after reaching</color> POWER LEVEL {0}", openLv);
        }
    }


    public void OnClickSlotTouch(SkillSlotBaller obj)
    {
        int newSlot = obj.Slot;
        Debug.Log("newSlot = " + newSlot);
        if(newSlot != CurSlot)
        {
            CurSlot = newSlot;            
            SetCursorPos(obj.gameObject);
            CurSelect = GetSelectedSkill();
            Debug.Log("new slot type " + SkillSlot[CurSlot - 1].Type);
            ShowSkillList((SlotEquipType)SkillSlot[CurSlot - 1].Type);
            ShowSkillText();
            ShowBtnState();
        }
    }


    public void OnClickListTouch(SkillSlotBaller obj)
    {
        int newSelect = obj.Slot;
        Debug.Log("newSelect = " + newSelect);
        if (newSelect != CurSelect)
        {
            CurSelect = newSelect;
            ChangeSkillList();
            ShowSkillText();
            ShowBtnState();
        }
    }

}


public class SkillListType
{
    public int idx; //비활성/활성/업글가능
    public int level; //0인경우 비활성화

    public SkillListType(int _idx, int _level)
    {
        idx = _idx;
        level = _level;
    }
}

public enum SlotEquipType
{
    HittingSkill = 1,
    FieldingSkill = 2,
    Gear = 11,
}