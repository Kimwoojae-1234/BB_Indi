using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallerSkillComponent : MonoBehaviour
{
    [SerializeField] private RectTransform[] Pos;
    [SerializeField] private SkillSlotBaller[] slot;

    private Dictionary<int, SkillSlotBaller> slotList = new Dictionary<int, SkillSlotBaller>();


    //210,450,690,
    //private int idx;

    public void InitForCollection(CharacterData ballerData, KOBBaller ballerInfo)
    {
        initLayout();

        //선수수정할것 - 수정함
        HitterSkillData skillData = KOBManager.Backend.Chart.HitterSkillData.GetData(ballerData.char_idx);

        if (skillData != null)
        {
            int Count = skillData.slot_list.Count;
            SetSlotLayout(Count);

            int CurLv = ballerInfo.level;
            int count = 0;
            foreach (KeyValuePair<int, SkillSlotBaller> slot in slotList)
            {
                int key = slot.Key;
                int openLv = skillData.slot_list[count];
                int type = (count % 2 == 0 ? 1 : 2);//이게 개허접하네
                if (CurLv < openLv)
                {
                    //잠김
                    slot.Value.SetLock(key, type, openLv, ballerData.rarity);
                }
                else
                {
                    //bool bEquip = false;
                    int skillIndex = 0;
                    if (ballerInfo.SkillEquip.ContainsKey(key) == true)
                    {
                        skillIndex = ballerInfo.SkillEquip[key];
                    }

                    if (skillIndex == 0) //비었음
                    {
                        slot.Value.SetEmpty(key, type);
                    }
                    else //스킬 있음
                    {
                        int SkillLV = ballerInfo.SkillList[skillIndex].level; //스킬 레벨
                        slot.Value.SetSkill(key, type, skillIndex, SkillLV);
                    }
                }
                count++;
            }
        }
    }


    public void InitForLocked(CharacterData ballerData)
    {
        initLayout();

        //선수수정할것 - 수정함
        HitterSkillData skillData = KOBManager.Backend.Chart.HitterSkillData.GetData(ballerData.char_idx);

        if (skillData != null)
        {
            int Count = skillData.slot_list.Count;
            SetSlotLayout(Count);
            int count = 0;
            foreach (KeyValuePair<int, SkillSlotBaller> slot in slotList)
            {
                int key = slot.Key;
                int openLv = skillData.slot_list[count];
                int type = (count % 2 == 0 ? 1 : 2);//이게 개허접함
                slot.Value.SetLock(key, type, openLv, ballerData.rarity);
                count++;
            }
        }
    }


    private void initLayout()
    {
        slotList.Clear();
        for (int i = 0; i < slot.Length; i++) slot[i].gameObject.SetActive(false);
    }

    private void SetSlotLayout(int Count)
    {
        int[] _size = new int[] { 210, 450, 690 };
        
        if (Count == 0)
        {
            Pos[0].gameObject.SetActive(false);
            Pos[1].gameObject.SetActive(false);
        }
        else
        {
            if (Count <= 3)
            {                
                Pos[0].gameObject.SetActive(true);
                Pos[0].GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
                Pos[0].sizeDelta = new Vector2(_size[Count - 1], 210);
                Pos[1].gameObject.SetActive(false);
                for (int i = 0; i < Count; i++)
                {
                    int Key = (i + 1);
                    slot[i].gameObject.SetActive(true);
                    slotList.Add(Key, slot[i]);
                }
            }
            else
            {
                Pos[0].gameObject.SetActive(true);
                Pos[1].gameObject.SetActive(true);
                Pos[0].sizeDelta = new Vector2(690, 210);
                if (Count == 4)
                {
                    Pos[0].GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
                    slot[0].gameObject.SetActive(true);
                    slot[1].gameObject.SetActive(true);
                    slot[3].gameObject.SetActive(true);
                    slot[4].gameObject.SetActive(true);
                    slotList.Add(1,slot[0]);
                    slotList.Add(2,slot[3]);
                    slotList.Add(3,slot[1]);
                    slotList.Add(4,slot[4]);
                }
                else if (Count == 5)
                {
                    Pos[0].GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
                    slot[0].gameObject.SetActive(true);
                    slot[1].gameObject.SetActive(true);
                    slot[2].gameObject.SetActive(true);
                    slot[3].gameObject.SetActive(true);
                    slot[4].gameObject.SetActive(true);
                    slotList.Add(1,slot[0]);
                    slotList.Add(2,slot[3]);
                    slotList.Add(3,slot[1]);
                    slotList.Add(4,slot[4]);
                    slotList.Add(5,slot[2]);
                }
                else
                {
                    Pos[0].GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
                    for (int i = 0; i < slot.Length; i++) slot[i].gameObject.SetActive(true);
                    slotList.Add(1,slot[0]);
                    slotList.Add(2,slot[3]);
                    slotList.Add(3,slot[1]);
                    slotList.Add(4,slot[4]);
                    slotList.Add(5,slot[2]);
                    slotList.Add(6,slot[5]);
                }

            }
        }
    }

}
