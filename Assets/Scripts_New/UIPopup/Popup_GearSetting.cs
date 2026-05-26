using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Popup_GearSetting : UIPopup
{
    [SerializeField] private GearSlotBaller[] GearSlot;
    [SerializeField] private GearSlotBaller[] GearList;
    [SerializeField] private RectTransform origin;
    [SerializeField] private TextMeshProUGUI GearNameTxt;
    [SerializeField] private TextMeshProUGUI GearDescTxt;
    [SerializeField] private RectTransform Position;
    [SerializeField] private GameObject BuyBtn;
    [SerializeField] private GameObject UpgradeBtn;
    [SerializeField] private GameObject LockBtn; //누르면 언제 업글 가능한지 알려줘
    [SerializeField] private GameObject NotAvailble;


    //private GameObject BallerObj;
    public delegate void OnCloseEvent();
    private OnCloseEvent CallBack = null;



    private Dictionary<int, GearListType> CurGearList = new Dictionary<int, GearListType>();
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
        if (CallBack != null)
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
            GearSlotSetting(ballerData, ballerInfo);

            GearListSetting(ballerData, ballerInfo);//, GearSlot[CurSlot - 1].Type);
            CurSelect = GetSelectedGear();
            ShowGearList((SlotEquipType)GearSlot[CurSlot - 1].Type);
        }
        else
        {
            bAvailableBaller = false;
            GearSlotSetting(ballerData, null);

            CurSelect = 1;
            GearListSetting(ballerData, null);//, GearSlot[CurSlot - 1].Type);
            ShowGearList((SlotEquipType)GearSlot[CurSlot - 1].Type);
        }

        ShowGearText();
        ShowBtnState();
        SetCursorPos(GearSlot[CurSlot - 1].gameObject);
    }


    private int GetSelectedGear()
    {
        int EquipedGear = GearSlot[CurSlot - 1].GearIndex;
        if (EquipedGear != 0)
        {
            for (int i = 0; i < GearSlot.Length; i++)
            {
                if (GearSlot[i].GearIndex == EquipedGear)
                {
                    return (i + 1);
                }
            }
        }

        return 1;
    }


    private void GearSlotSetting(CharacterData ballerData, KOBBaller ballerInfo)
    {
        CurBallerLevel = 0;
        /*if (ballerData.gear_slot != null)
        {
            int Count = ballerData.gear_slot.Count;
            if (ballerInfo != null) //보유함
            {
                CurBallerLevel = ballerInfo.level;
                for (int i = 0; i < GearSlot.Length; i++)
                {
                    if (i < Count)
                    {
                        GearSlot[i].gameObject.SetActive(true);
                        int slot = (i + 1);
                        int openLv = ballerData.gear_slot[slot][1];
                        int type = ballerData.gear_slot[slot][0];
                        OpenLevel[i] = openLv;
                        if (CurBallerLevel < openLv)
                        {
                            //잠김
                            GearSlot[i].SetLock(slot, type, openLv, ballerData.rarity);
                        }
                        else
                        {
                            //bool bEquip = false;
                            int gearIndex = 0;
                            if (ballerInfo.GearEquip.ContainsKey(slot) == true)
                            {
                                gearIndex = ballerInfo.GearEquip[slot];
                            }

                            if (gearIndex == 0) //비었음
                            {
                                GearSlot[i].SetEmpty(slot, type);
                            }
                            else //스킬 있음
                            {
                                int GearLV = ballerInfo.GearList[gearIndex].level; //스킬 레벨
                                GearSlot[i].SetGear(gearIndex, slot, type, GearLV);
                            }
                        }
                    }
                    else
                    {
                        GearSlot[i].gameObject.SetActive(false);
                    }
                }
            }
            else //볼러 미보유
            {
                for (int i = 0; i < GearSlot.Length; i++)
                {
                    if (i < Count)
                    {
                        GearSlot[i].gameObject.SetActive(true);
                        int slot = (i + 1);
                        int openLv = ballerData.gear_slot[slot][1];
                        int type = ballerData.gear_slot[slot][0];
                        OpenLevel[i] = openLv;
                        //미보유시 Lock형태
                        GearSlot[i].SetLock(slot, type, openLv, ballerData.rarity);
                    }
                    else
                    {
                        GearSlot[i].gameObject.SetActive(false);
                    }
                }
            }
        }*/
    }



    private void GearListSetting(CharacterData ballerData, KOBBaller ballerInfo)
    {
        CurGearList.Clear();

        if (ballerInfo != null) //보유시
        {
            //일부라도 성장이 되있는 경우
            /*foreach (KeyValuePair<int, KOBGear> gear in ballerInfo.GearList) //info로 부터
            {
                int gearindex = gear.Key;
                KOBGear value = gear.Value;
                CurGearList.Add(gearindex, new GearListType(gearindex, value.level));                
            }*/
        }

        //아직 아무것도 없는 경우
        /*for (int i = 0; i < ballerData.gear_list.Count; i++) //data로 부터
        {
            int gearIdx = ballerData.gear_list[i];

            if (CurGearList.ContainsKey(gearIdx) == false)
            {
                CurGearList.Add(gearIdx, new GearListType(gearIdx, 0));
            }
        }*/

        //gear index 오름차순 정렬
        var sortVar1 = from item in CurGearList
                       orderby item.Value.idx ascending
                       select item;
        sortVar1.ToDictionary(x => x.Key, x => x.Value);

        

        Debug.Log("CurSlot : " + CurSlot);
    }

    private void ShowGearList(SlotEquipType type)
    {   
    }

    private void ChangeGearList()
    {
        
    }


    private void SetCursorPos(GameObject obj)
    {
        float newX = obj.transform.position.x;
        float newY = Position.transform.position.y;
        Position.transform.position = new Vector2(newX, newY);
    }


    private void ShowGearText()
    {
        int CurGearIndex = GearList[CurSelect - 1].GearIndex;
        int CurLevel = 0;// GearList[CurSelect - 1].Level;


        string name_id = string.Format("GearName.{0}", CurGearIndex);
        string desc_id = string.Format("GearDesc.{0}", CurGearIndex);


        if (CurLevel == 0)
        {
            GearNameTxt.text = KOBManager.Localization.GetUILocalizedValue2(name_id);
        }
        else
        {
            GearNameTxt.text = string.Format("{0} LV{1}", KOBManager.Localization.GetUILocalizedValue2(name_id), CurLevel);
        }

        int value = Random.Range(10, 15);
        int Per = Random.Range(40, 85);
        int value2 = Random.Range(10, 15);
        int Per2 = Random.Range(40, 85);
        int value3 = Random.Range(10, 15);
        int Per3 = Random.Range(40, 85);

        GearDescTxt.text = string.Format(KOBManager.Localization.GetUILocalizedValue2(desc_id), value, Per, value2, Per2, value3, Per3);
    }


    private void ShowBtnState()
    {
        BuyBtn.gameObject.SetActive(false);
        UpgradeBtn.gameObject.SetActive(false);
        LockBtn.gameObject.SetActive(false);
        NotAvailble.gameObject.SetActive(false);

        int openLv = OpenLevel[CurSlot - 1];
        if (bAvailableBaller == true)
        {
            if (CurBallerLevel < openLv)
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


    public void OnClickSlotTouch(GearSlotBaller obj)
    {
        int newSlot = obj.Slot;
        Debug.Log("newSlot = " + newSlot);
        if (newSlot != CurSlot)
        {
            CurSlot = newSlot;
            SetCursorPos(obj.gameObject);
            CurSelect = GetSelectedGear();
            Debug.Log("new slot type " + GearSlot[CurSlot - 1].Type);
            ShowGearList((SlotEquipType)GearSlot[CurSlot - 1].Type);
            ShowGearText();
            ShowBtnState();
        }
    }


    public void OnClickListTouch(GearSlotBaller obj)
    {
        int newSelect = obj.Slot;
        Debug.Log("newSelect = " + newSelect);
        if (newSelect != CurSelect)
        {
            CurSelect = newSelect;
            ChangeGearList();
            ShowGearText();
            ShowBtnState();
        }
    }

}


public class GearListType
{
    public int idx; //비활성/활성/업글가능
    public int level; //0인경우 비활성화

    public GearListType(int _idx, int _level)
    {
        idx = _idx;
        level = _level;
    }
}