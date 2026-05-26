using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

public enum EquipType
{
    Skill,
    Gear,
    Bat,
    Skin
}

public enum EquipState
{
    Equip,
    Release
}



public partial class Popup_Equip : UIPopup
{
    [SerializeField] private RectTransform PopupBody;

    /*
    [Header("[타이틀 관련]")]
    [SerializeField] private Image TitleLabel;
    [SerializeField] private TextMeshProUGUI TitleText;


    [Header("[장착 아이콘 관련]")]
    [SerializeField] private CardComponent SkillIcon;
    [SerializeField] private CardComponent GearIcon;
    [SerializeField] private CardComponent BatIcon;
    [SerializeField] private CardComponent SkinIcon;
    [SerializeField] private TextMeshProUGUI ItemNameText;
    [SerializeField] private TextMeshProUGUI ItemDescText;

    [Header("[버튼 관련]")]
    [SerializeField] private GameObject[] ButtonObj;

    [Header("[화살표]")]
    [SerializeField] private GameObject ArrowObj;

    [Header("[디스크립션]")]
    [SerializeField] private GameObject DescObj;
    [SerializeField] private GameObject MaxDesc;

    [Header("[슬롯 대체]")]
    [SerializeField] private GameObject DeDimed_Replace;
    [SerializeField] private GameObject Equip_Skill_Slot;
    [SerializeField] private GameObject Equip_Gear_Slot;
    [SerializeField] private SlotComponent[] Skill_Slot;
    [SerializeField] private SlotComponent[] Gear_Slot;

    [Header("[재화 관련]")]
    [SerializeField] private RectTransform PropertyObj;
    [SerializeField] private TextMeshProUGUI PropertyText;
    [SerializeField] private TextMeshProUGUI ItemText;
    [SerializeField] private Image PropertyGold;
    [SerializeField] private Image PropertySP;
    [SerializeField] private Image ItemIcon;

    //업그레이드 콜배 대리자
    public delegate void EventUpdateCallback(int index, int slot);
    private EventUpdateCallback EquipCallBack = null; //장착 이벤트 처리
    private EventUpdateCallback ReleaseCallBack = null; //해제 이벤트 처리
    private EventUpdateCallback upgradeCallBack = null; //업그레이드 이벤트 처리

    private EquipType Type; //타입
    private int selectedIndex = -1; //스킬,장비,배트 인덱스
    private bool bReleaseCase = false; //릴리즈 케이스
    private int EquipSlot = -1;     //장착슬롯 인덱스
    private int ReleaseSlot = -1;   //해제슬롯 인덱스

    private int openSkillSlotCount = 0; //현재 오픈되어 있는 스킬 슬롯 수
    private int openGearSlotCount = 0;  //현재 오픈되어 있는 장비 슬롯 수

    //현재레벨의 desc텍스트와 max인 경우 desc를 저장하는 버퍼 텍스트
    private string curLevelTitle, maxLevelTitle; 

    //이게 true인 경우만 업그레이드가 가능
    private bool bUpgradePossible = false;


    //업그레이드시 버튼 작동 안함
    private bool bUpgradeProcessBunttonSetting = false;

    /// <summary>
    /// 추후 스킨 작업시
    /// </summary>
    /// <param name="idx"></param>
    public void SkinSetting(int idx)
    {

    }


    /// <summary>
    /// 대체 슬롯 팝업 (기존 슬롯이 다 차 있는 경우 생기는) 초기화
    /// </summary>
    private void slotReplaceInit()
    {
        bUpgradeProcessBunttonSetting = false;
        DeDimed_Replace.gameObject.SetActive(false);
        Equip_Skill_Slot.gameObject.SetActive(false);
        Equip_Gear_Slot.gameObject.SetActive(false);
    }


    /// <summary>
    /// 타이틀 세팅 (희귀도 색 설정)
    /// </summary>
    /// <param name="_rarity"></param>
    private void TitleLabelSetting(string _rarity, string _type, string _desc)
    {
        KOBRarity rarity = (KOBRarity)Enum.Parse(typeof(KOBRarity), _rarity);
        string desc = KOBManager.Localization.GetUILocalizedValue(_desc, null);

        if (rarity == KOBRarity.LEGENDARY)
        {
            TitleLabel.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.Frame2, "Frame_CardFrame_Legend");
            ItemNameText.text = string.Format("<color=yellow>{0} {1}</color>\n{2}", _rarity, _type, desc);
        }
        else if (rarity == KOBRarity.EPIC)
        {
            TitleLabel.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.Frame2, "Frame_CardFrame_Epic");
            ItemNameText.text = string.Format("<color=purple>{0} {1}</color>\n{2}", _rarity, _type, desc);
        }
        else if (rarity == KOBRarity.RARE)
        {
            TitleLabel.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.Frame2, "Frame_CardFrame_Rare");
            ItemNameText.text = string.Format("<color=orange>{0} {1}</color>\n{2}", _rarity, _type, desc);
        }
        else  //common
        {
            TitleLabel.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.Frame2, "Frame_CardFrame_Common");
            ItemNameText.text = string.Format("<color=blue>{0} {1}</color>\n{2}", _rarity, _type, desc);
        }        
    }


    /// <summary>
    /// 맥스 버튼을 누르고 있는 경우 - Max상태 desc를 나타냄
    /// </summary>
    public void OnPointerDownMax()
    {
        if (bUpgradeProcessBunttonSetting == true)
        {
            Debug.Log("업글시 버튼 작동안함");
            return;
        }
        Debug.Log("OnPointerDownMax");
        MaxDesc.gameObject.SetActive(true);
        DescObj.gameObject.SetActive(false);
        TitleText.text = maxLevelTitle;
    }

    /// <summary>
    /// 맥스버튼을 Point Up하는 상태 - 원래 desc를 보여줌
    /// </summary>
    public void OnPointerUpMax()
    {
        if (bUpgradeProcessBunttonSetting == true)
        {
            Debug.Log("업글시 버튼 작동안함");
            return;
        }
        Debug.Log("OnPointerUpMax");
        MaxDesc.gameObject.SetActive(false);
        DescObj.gameObject.SetActive(true);
        TitleText.text = curLevelTitle;
    }

    /// <summary>
    /// 업그레이드 버튼
    /// </summary>
    public void OnClickUpgrade()
    {
        if (bUpgradeProcessBunttonSetting == true)
        {
            Debug.Log("업글시 버튼 작동안함");
            return;
        }

        if (bUpgradePossible == false)
        {
            Debug.Log("업그레이드가 불가능함!!!!");
            return;
        }

        if (Type == EquipType.Skill)
        {
            SkillUpgrade();
        }
        else if (Type == EquipType.Gear)
        {
            GearUpgrade();
        }
    }


    private void UpgradeEndCallback(BackendReturnObject callback)
    {
        FrontUI_NetworkLoading popup = KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>();
        if (popup != null) popup.Close();

        if (callback == null)
        {
            Debug.Log("업데이트 할 내용이 없음");
        }
        else
        {
            
        }
    }

    /// <summary>
    /// 장착후 Player UI를 업데이트
    /// </summary>
    /// <param name="slot"></param>
    public void UpgradePlayerUIUpdate()
    {
        if(Type == EquipType.Skill)
        {
            StartCoroutine(skillUpgradeProcess());
        }
        else if(Type == EquipType.Gear)
        {
            StartCoroutine(gearUpgradeProcess());
        }

        if (upgradeCallBack != null)
        {
            upgradeCallBack(selectedIndex, ReleaseSlot);
        }
        //upgradeCallBack = null;
    }


    

    /// <summary>
    /// 장비 버튼 클릭
    /// </summary>
    public void OnClickEquip()
    {
        if (bUpgradeProcessBunttonSetting == true)
        {
            Debug.Log("업글시 버튼 작동안함");
            return;
        }

        if (Type == EquipType.Skill)
        {
            SkillEquip();
        }
        else if (Type == EquipType.Gear)
        {
            GearEquip();            
        }
        else if (Type == EquipType.Bat)
        {
            BatEquip();            
        }
    }


    /// <summary>
    /// 장착후 Player UI를 업데이트
    /// </summary>
    /// <param name="slot"></param>
    public void EquipPlayerUIUpdate(int slot)
    {
        if (EquipCallBack != null)
        {
            EquipCallBack(selectedIndex, slot);
        }
        EquipCallBack = null;

        Close();
    }


    /// <summary>
    /// 장착시 해당 슬롯에 이미 장착되어 있는 스킬을 다른 스킬로 대체
    /// </summary>
    /// <param name="arg"></param>
    public void OnClickSkillReplace(int arg)
    {
        int slot = arg;
        KOBManager.Backend.GameData.KOBSettingInfo.SkillSetting(slot, selectedIndex);
        EquipPlayerUIUpdate(EquipSlot);//대신
    }


    /// <summary>
    /// 장착 시 해당 슬롯에 이미 장착되어 있는 장비를 다른 장비로 대체
    /// </summary>
    /// <param name="arg"></param>
    public void OnClickGearReplace(int arg)
    {
        int slot = arg;
        KOBManager.Backend.GameData.KOBSettingInfo.GearSetting(slot, selectedIndex);
        EquipPlayerUIUpdate(EquipSlot); //대신
    }



    /// <summary>
    /// 장비제거 버튼 클릭
    /// </summary>
    public void OnClickRelease()
    {
        if (bUpgradeProcessBunttonSetting == true)
        {
            Debug.Log("업글시 버튼 작동안함");
            return;
        }
    }



    /// <summary>
    /// 바뀐내용에 따라 Player UI를 업데이트 해준다
    /// </summary>
    /// <param name="slot"></param>
    public void ReleasePlayerUIUpdate(int slot)
    {
        if (ReleaseCallBack != null)
        {
            ReleaseCallBack(selectedIndex, slot);
        }
        ReleaseCallBack = null;

        Close();
    }



    /// <summary>
    /// 다음 버튼 (왼쪽화살표)
    /// </summary>
    public void PushLeftArrow()
    {
        if (bReleaseCase == false)
        {
            NextEquipPopup(false);
        }
        else
        {
            NextReleasePopup(false);
        }
    }

    /// <summary>
    /// 다음 버튼 (오른쪽 화살표)
    /// </summary>
    public void PushRightArrow()
    {
        if (bReleaseCase == false)
        {
            NextEquipPopup(true);
        }
        else
        {
            NextReleasePopup(true);
        }
    }

    /// <summary>
    /// 화살표 버튼을 눌러 다음 팝업 
    /// </summary>
    /// <param name="bRight"></param>
    private void NextEquipPopup(bool bRight)
    {
        int nextKey = -1;
        int count = -1;
        UI_Player playerUI = KOBManager.UI.GetUIWindow<UI_Player>();
        if (Type == EquipType.Skill)
        {
            List<int> sort = playerUI.GetSkillSortList();
            for (int i = 0; i < sort.Count; i++)
            {
                if (sort[i] == selectedIndex)
                {
                    count = i;
                    break;
                }
            }
            if (count != -1)
            {
                if (bRight == true)
                {
                    int next = count + 1;
                    if (next >= sort.Count) next = 0;
                    nextKey = sort[next];
                    skillSetting(nextKey);
                }
                else
                {
                    int next = count - 1;
                    if (next < 0) next = sort.Count - 1;
                    nextKey = sort[next];
                    skillSetting(nextKey);
                }
            }
        }
        else if (Type == EquipType.Gear)
        {
            List<int> sort = playerUI.GetGearSortList();
            for (int i = 0; i < sort.Count; i++)
            {
                if (sort[i] == selectedIndex)
                {
                    count = i;
                    break;
                }
            }
            if (count != -1)
            {
                if (bRight == true)
                {
                    int next = count + 1;
                    if (next >= sort.Count) next = 0;
                    nextKey = sort[next];
                    gearSetting(nextKey);
                }
                else
                {
                    int next = count - 1;
                    if (next < 0) next = sort.Count - 1;
                    nextKey = sort[next];
                    gearSetting(nextKey);
                }
            }
        }
        else if (Type == EquipType.Bat)
        {
            List<int> sort = playerUI.GetBatSortList();
            for (int i = 0; i < sort.Count; i++)
            {
                if (sort[i] == selectedIndex)
                {
                    count = i;
                    break;
                }
            }
            if (count != -1)
            {
                if (bRight == true)
                {
                    int next = count + 1;
                    if (next >= sort.Count) next = 0;
                    nextKey = sort[next];
                    batSetting(nextKey);
                }
                else
                {
                    int next = count - 1;
                    if (next < 0) next = sort.Count - 1;
                    nextKey = sort[next];
                    batSetting(nextKey);
                }
            }
        }
    }

    private void NextReleasePopup(bool bRight)
    {
        int nextKey = -1;
        int count = -1;
        UI_Player playerUI = KOBManager.UI.GetUIWindow<UI_Player>();
        if (Type == EquipType.Skill)
        {
            List<int> equip = playerUI.GetSkillEquipList();
            for (int i = 0; i < equip.Count; i++)
            {
                if (equip[i] == selectedIndex)
                {
                    count = i;
                    break;
                }
            }
            if (count != -1)
            {
                if (bRight == true)
                {
                    int next = count + 1;
                    if (next >= equip.Count) next = 0;
                    nextKey = equip[next];
                    skillSetting(nextKey);
                    ReleaseSlot = (next + 1);
                }
                else
                {
                    int next = count - 1;
                    if (next < 0) next = equip.Count - 1;
                    nextKey = equip[next];
                    skillSetting(nextKey);
                    ReleaseSlot = (next + 1);
                }
            }
        }
        else if (Type == EquipType.Gear)
        {
            List<int> equip = playerUI.GetGearEquipList();
            for (int i = 0; i < equip.Count; i++)
            {
                if (equip[i] == selectedIndex)
                {
                    count = i;
                    break;
                }
            }
            if (count != -1)
            {
                if (bRight == true)
                {
                    int next = count + 1;
                    if (next >= equip.Count) next = 0;
                    nextKey = equip[next];
                    gearSetting(nextKey);
                    ReleaseSlot = (next + 1);
                }
                else
                {
                    int next = count - 1;
                    if (next < 0) next = equip.Count - 1;
                    nextKey = equip[next];
                    gearSetting(nextKey);
                    ReleaseSlot = (next + 1);
                }
            }
        }
        
    }


    public override void Close()
    {
        if (bUpgradeProcessBunttonSetting == true)
        {
            Debug.Log("업그레이드시 팝업 안닫힘");
            return;
        }

        base.Close();
        slotReplaceInit();
    }


    /// <summary>
    /// 아이템 정보로 Desc Text를 만드는 메쏘드 - 스킬, 장비
    /// </summary>
    /// <param name="CurDesc"></param>
    /// <param name="NextDesc">이값이 NULL이면 MAX값 표현해준다</param>
    /// <returns></returns>
    private string MakeItemDetail(Dictionary<int, int> CurDesc, Dictionary<int, int> NextDesc, int overall, int next)
    {
        string detail = string.Empty;

        int count = 0;
        int totalCount = CurDesc.Count;

        bool bMaxLevel = false;
        if(NextDesc == null) bMaxLevel = true;

        if (bMaxLevel == true)
        {
            string overvalue = string.Format("<size=65><color=#ffdf00>OVERALL</color> <color=yellow>+{0}</size> <size=50><color=orange>(MAX)</color></size></color>\n", overall);
            detail += overvalue;
        }
        else
        {
            string overvalue = string.Format("<size=65><color=#ffdf00>OVERALL</color> <color=yellow>+{0}</size> <size=50><color=green>(+{1})</color></size></color>\n", overall, next - overall);
            detail += overvalue;
        }

        foreach (KeyValuePair<int, int> kv in CurDesc)
        {
            count++;
            int Key = kv.Key;
            string statID = KOBManager.Localization.GetUILocalizedValue(string.Format("Stat.Type{0}", Key), null);

            int curValue = kv.Value;
            if (bMaxLevel == true) //맥스레벨
            {
                if (kv.Value < 100) //수치 증가
                {
                    string value = string.Format("      • {0} <color=yellow>+{1}</color> <size=40><color=orange>(MAX)</color></size>", statID, curValue);
                    detail += value;
                }
                else //퍼센트 증가
                {
                    string value = string.Format("      • {0} <color=yellow>+{1}%</color> <size=40><color=orange>(MAX)</color></size>", statID, (curValue / 100));
                    detail += value;
                }
            }
            else
            {
                int nextValue = NextDesc[Key] - curValue;
                if (kv.Value < 100) //수치 증가
                {
                    string value = string.Format("      • {0} <color=yellow>+{1}</color> <size=40><color=green>(+{2})</color></size>", statID, curValue, nextValue);
                    detail += value;
                }
                else //퍼센트 증가
                {
                    string value = string.Format("      • {0} <color=yellow>+{1}%</color> <size=40><color=green>(+{2}%)</color></size>", statID, (curValue / 100), (nextValue / 100));
                    detail += value;
                }
            }

            if(count < totalCount)
            {
                detail += "\n";
            }

        }

        return detail;
    }

    /// <summary>
    /// 아이템 정보로 Desc Text를 만드는 메쏘드 - 배트(컨슘아이템)
    /// </summary>
    /// <param name="CurDesc"></param>
    /// <returns></returns>
    private string MakeConsumeItemDetail(Dictionary<int, int> CurDesc, int overall)
    {
        string detail = string.Empty;

        int count = 0;
        int totalCount = CurDesc.Count;

        string overvalue = string.Format("<size=65><color=#ffdf00>OVERALL</color> <color=yellow>+{0}</size></color>\n", overall);
        detail += overvalue;

        foreach (KeyValuePair<int, int> kv in CurDesc)
        {
            count++;
            int Key = kv.Key;
            string statID = KOBManager.Localization.GetUILocalizedValue(string.Format("Stat.Type{0}", Key), null);

            int curValue = kv.Value;

            if (kv.Value < 100) //수치 증가
            {
                string value = string.Format("      • {0} <color=yellow>+{1}</color>", statID, curValue);
                detail += value;
            }
            else //퍼센트 증가
            {
                string value = string.Format("      • {0} <color=yellow>+{1}%</color>", statID, (curValue / 100));
                detail += value;
            }

            if (count < totalCount)
            {
                detail += "\n";
            }

        }

        return detail;
    }
}

/// <summary>
/// 스킬관련
/// </summary>
public partial class Popup_Equip : UIPopup //스킬
{
    /// <summary>
    /// 스킬 세팅 - releaseSlot값이 -1이 아닌 경우 스킬 제거 팝업
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="_callBack"></param>
    /// <param name="releaseSlot"></param>
    public void SkillPopupSetting(int idx, EventUpdateCallback _callBack, EventUpdateCallback _callBackUpgrade, int releaseSlot = -1)
    {
        //장비/제거 여부
        ReleaseSlot = releaseSlot;
        if (releaseSlot == -1)
        {
            EquipCallBack = _callBack;
            ReleaseCallBack = null;
            bReleaseCase = false;
        }
        else
        {
            EquipCallBack = null;
            ReleaseCallBack = _callBack;
            bReleaseCase = true;
        }
        upgradeCallBack = _callBackUpgrade;

        //바디 설정
        PopupBody.sizeDelta = new Vector2(1480, 1300);


        //아이콘 세팅
        SkillIcon.gameObject.SetActive(true);
        GearIcon.gameObject.SetActive(false);
        BatIcon.gameObject.SetActive(false);

        //버튼 세팅
        ButtonObj[0].SetActive(true);
        ButtonObj[1].SetActive(true);
        ButtonObj[2].SetActive(!bReleaseCase);
        ButtonObj[3].SetActive(bReleaseCase);

        //디스크립션
        DescObj.gameObject.SetActive(true);
        MaxDesc.gameObject.SetActive(false);

        //재화 안내
        PropertyObj.gameObject.SetActive(true);
        PropertyGold.gameObject.SetActive(false);
        PropertySP.gameObject.SetActive(true);

        //화살표 세팅
        skillArrowSetting(bReleaseCase);

        //대체될 스킬 세팅
        replaceSkillSetting();


        //스킬 세팅
        skillSetting(idx);
    }


    private void skillSetting(int idx)
    {
        
    }


    private void replaceSkillSetting()
    {
       
    }


    private void skillArrowSetting(bool bRelease)
    {
        if (bRelease == false)
        {
            UI_Player playerUI = KOBManager.UI.GetUIWindow<UI_Player>();
            if (playerUI.GetSkillSortList().Count > 1) ArrowObj.gameObject.SetActive(true);
            else ArrowObj.gameObject.SetActive(false);
        }
        else
        {
            Dictionary<int, int> SkillSet = KOBManager.Backend.GameData.KOBSettingInfo.SkillSet;
            if (SkillSet.Count > 1) ArrowObj.gameObject.SetActive(true);
            else ArrowObj.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 스킬 UI에서 Equip 버튼 클릭시 스킬 장착
    /// </summary>
    private void SkillEquip()
    {
        //KOBPlayerInfo kobPlayerInfo = KOBManager.Backend.GameData.KOBPlayerInfo;
        //빈슬롯 찾음
        //빈슬롯이 없는 경우 바꿀 목록 로딩
        //Debug.Log("==================>> 현재 열린 스킬 슬롯 수 : " + openSkillSlotCount);
        //빈슬롯 찾기
        for (int i = 0; i < openSkillSlotCount; i++)
        {
            int slot = i + 1;
            if (KOBManager.Backend.GameData.KOBSettingInfo.SkillSet.ContainsKey(slot) == false)
            {
                //Debug.Log("==================>> Slot 이 비었음 : " + slot);
                KOBManager.Backend.GameData.KOBSettingInfo.SkillSetting(slot, selectedIndex);
                EquipPlayerUIUpdate(EquipSlot); //업데이트 대신
                return;
            }
        }

        //스킬 비우기 위한 별도 UI열어
        //Debug.Log("==================>> Slot 이 비지 않아 대체 팝업 필요");
        DeDimed_Replace.gameObject.SetActive(true);
        Equip_Skill_Slot.gameObject.SetActive(true);
    }


    /// <summary>
    /// 업그레이드 버튼을 눌러 스킬을 업그레이드
    /// </summary>
    private void SkillUpgrade()
    {
       
    }

    private IEnumerator skillUpgradeProcess()
    {
        bUpgradeProcessBunttonSetting = true;
        SkillIcon.SkillUpgrade();
        yield return new WaitForSeconds(0.5f);
        skillSetting(selectedIndex);
        bUpgradeProcessBunttonSetting = false;
    }

}

/// <summary>
/// 장비관련
/// </summary>
public partial class Popup_Equip : UIPopup //장비
{
    /// <summary>
    /// 장비 세팅 - releaseSlot값이 -1이 아닌 경우 아이템 제거 팝업
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="_callBack"></param>
    /// <param name="releaseSlot"></param>
    public void GearPopupSetting(int idx, EventUpdateCallback _callBack, EventUpdateCallback _callBackUpgrade, int releaseSlot = -1)
    {
        //장비/제거 여부
        ReleaseSlot = releaseSlot;
        if (releaseSlot == -1)
        {
            EquipCallBack = _callBack;
            ReleaseCallBack = null;
            bReleaseCase = false;
        }
        else
        {
            EquipCallBack = null;
            ReleaseCallBack = _callBack;
            bReleaseCase = true;
        }
        upgradeCallBack = _callBackUpgrade;

        //바디 설정
        PopupBody.sizeDelta = new Vector2(1480, 1300);

        //아이콘 세팅
        SkillIcon.gameObject.SetActive(false);
        GearIcon.gameObject.SetActive(true);
        BatIcon.gameObject.SetActive(false);

        //버튼 세팅
        ButtonObj[0].SetActive(true);
        ButtonObj[1].SetActive(true);
        ButtonObj[2].SetActive(!bReleaseCase);
        ButtonObj[3].SetActive(bReleaseCase);

        //디스크립션
        DescObj.gameObject.SetActive(true);
        MaxDesc.gameObject.SetActive(false);

        //재화 안내
        PropertyObj.gameObject.SetActive(true);
        PropertyGold.gameObject.SetActive(true);
        PropertySP.gameObject.SetActive(false);

        //화살표 세팅
        gearArrowSetting(bReleaseCase);

        //대체될 스킬 세팅
        replaceGearSetting();

        //장비 세팅
        gearSetting(idx);

    }

    private void gearSetting(int idx)
    {
       

    }

    private void replaceGearSetting()
    {
       
    }


    private void gearArrowSetting(bool bRelease)
    {
        UI_Player playerUI = KOBManager.UI.GetUIWindow<UI_Player>();
        if (bRelease == false)
        {
            if (playerUI.GetGearSortList().Count > 1) ArrowObj.gameObject.SetActive(true);
            else ArrowObj.gameObject.SetActive(false);
        }
        else
        {
            Dictionary<int, int> GearSet = KOBManager.Backend.GameData.KOBSettingInfo.GearSet;
            if (GearSet.Count > 1) ArrowObj.gameObject.SetActive(true);
            else ArrowObj.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 기어 UI에서 Equip 버튼 클릭시 기어 장착
    /// </summary>
    private void GearEquip()
    {
        //KOBPlayerInfo kobPlayerInfo = KOBManager.Backend.GameData.KOBPlayerInfo;
        //빈슬롯 찾음
        //빈슬롯이 없는 경우 바꿀 목록 로딩
        //Debug.Log("==================>> 현재 열린 장비 슬롯 수 : " + openGearSlotCount);
        //빈슬롯 찾기
        for (int i = 0; i < openGearSlotCount; i++)
        {
            int slot = i + 1;
            if (KOBManager.Backend.GameData.KOBSettingInfo.GearSet.ContainsKey(slot) == false)
            {
                Debug.Log("==================>> Slot 이 비었음 : " + slot);
                KOBManager.Backend.GameData.KOBSettingInfo.GearSetting(slot, selectedIndex);
                EquipPlayerUIUpdate(EquipSlot);//업데이트 대신
                return;
            }
        }

        //기어 비우기 위한 별도 UI열어
        //Debug.Log("==================>> Slot 이 비지 않아 대체 팝업 필요");
        //Debug.Log("==================>> Slot 이 비지 않아 대체 팝업 필요");
        DeDimed_Replace.gameObject.SetActive(true);
        Equip_Gear_Slot.gameObject.SetActive(true);
    }

    /// <summary>
    /// 업그레이드 버튼을 눌러 스킬을 업그레이드
    /// </summary>
    private void GearUpgrade()
    {
       
    }


    private IEnumerator gearUpgradeProcess()
    {
        bUpgradeProcessBunttonSetting = true;
        GearIcon.GearUpgrade();
        yield return new WaitForSeconds(0.5f);
        gearSetting(selectedIndex);
        bUpgradeProcessBunttonSetting = false;
    }

}


/// <summary>
/// 배트관련
/// </summary>
public partial class Popup_Equip : UIPopup //배트
{

    /// <summary>
    /// 배트 세팅 - releaseSlot값이 -1이 아닌 경우 아이템 제거 팝업
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="_callBack"></param>
    /// <param name="releaseSlot"></param>
    public void BatPopupSetting(int idx, EventUpdateCallback _callBack, int releaseSlot = -1)
    {
        //장비/제거 여부
        ReleaseSlot = releaseSlot;
        if (releaseSlot == -1)
        {
            EquipCallBack = _callBack;
            ReleaseCallBack = null;
            bReleaseCase = false;
        }
        else
        {
            EquipCallBack = null;
            ReleaseCallBack = _callBack;
            bReleaseCase = true;
        }
        upgradeCallBack = null;

        //바디 설정
        PopupBody.sizeDelta = new Vector2(1480, 1230);

        //아이콘 세팅
        SkillIcon.gameObject.SetActive(false);
        GearIcon.gameObject.SetActive(false);
        BatIcon.gameObject.SetActive(true);

        //버튼 세팅
        ButtonObj[0].SetActive(false);
        ButtonObj[1].SetActive(false);
        ButtonObj[2].SetActive(!bReleaseCase);
        ButtonObj[3].SetActive(bReleaseCase);

        //디스크립션
        DescObj.gameObject.SetActive(true);
        MaxDesc.gameObject.SetActive(false);

        //화살표 세팅
        batArrowSetting(bReleaseCase);

        //재화 안내
        PropertyObj.gameObject.SetActive(false);

        //배트 세팅
        batSetting(idx);
    }

    private void batSetting(int idx)
    {
        Type = EquipType.Bat;
        selectedIndex = idx;

        slotReplaceInit();

        KOBBat curBat = KOBManager.Backend.GameData.KOBPlayerInfo.BatList[selectedIndex];
        ConsumeItem data = KOBManager.Backend.Chart.ConsumeItem.Dictionary[selectedIndex];

        //TitleLabelSetting(data.rarity, "BAT", data.desc_id);
        TitleText.text = KOBManager.Localization.GetUILocalizedValue(data.name_id, null);

        BatIcon.BatEquipSetting(curBat);

        int overall = KOBManager.MyInfo.GetBatOverall(selectedIndex);

        //ItemDescText.text = MakeConsumeItemDetail(data.Value, overall);

        bUpgradePossible = false;
    }


    private void batArrowSetting(bool bRelease)
    {
        if (bRelease == false)
        {
            UI_Player playerUI = KOBManager.UI.GetUIWindow<UI_Player>();
            if (playerUI.GetBatSortList().Count > 1) ArrowObj.gameObject.SetActive(true);
            else ArrowObj.gameObject.SetActive(false);
        }
        else
        {
            ArrowObj.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 배트 UI에서 Equip 버튼 클릭시 배트 장책
    /// </summary>
    private void BatEquip()
    {
        //KOBPlayerInfo kobPlayerInfo = KOBManager.Backend.GameData.KOBPlayerInfo;
        //배트는 단일 슬롯이므로 바로 꽂아!!
        KOBManager.Backend.GameData.KOBSettingInfo.BatSetting(selectedIndex);
        EquipPlayerUIUpdate(EquipSlot);//업데이트 대신
    }*/
}