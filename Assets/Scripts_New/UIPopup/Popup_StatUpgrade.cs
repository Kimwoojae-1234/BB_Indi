using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;
using System.Security.Cryptography;

public class Popup_StatUpgrade : UIPopup
{
    [Header("[기본 오브젝트]")]
    [SerializeField] private TextMeshProUGUI TitleTxt;
    [SerializeField] private GameObject StatObj;
    [SerializeField] private GameObject SkillObj;
    [SerializeField] private GameObject SpecialSkillObj;
    [SerializeField] private GameObject NextSlotObj;
    [SerializeField] private BubbleDescUI BubbleDescObj;
    [SerializeField] private LobbyPropertyComponent propertyObj;
    [SerializeField] private GameObject UpgradeBtnObj;
    [SerializeField] private GameObject UnlockBtnObj;
    [SerializeField] private GameObject MaxBtnObj;


    [Header("[타격 정보]")]
    [SerializeField] private TextMeshProUGUI TotalHitValue;
    [SerializeField] private TextMeshProUGUI TotalHitUpgrade;
    [SerializeField] private TextMeshProUGUI [] HittingValue;
    [SerializeField] private TextMeshProUGUI[] HittingUpgrade;


    [Header("[필딩 정보]")]
    [SerializeField] private TextMeshProUGUI TotalFieldingValue;
    [SerializeField] private TextMeshProUGUI TotalFieldingUpgrade;
    [SerializeField] private TextMeshProUGUI[] FieldingValue;
    [SerializeField] private TextMeshProUGUI[] FieldingUpgrade;

    [Header("[스페셜 스킬 정보]")]
    [SerializeField] private TextMeshProUGUI SpecialSkillTxt;
    [SerializeField] private TextMeshProUGUI SpecialSkillUpgrade;

    [Header("[언락 슬롯 인포]")]
    [SerializeField] private GameObject [] UnlockSlot;


    [SerializeField] private TextMeshProUGUI UpgradeFeeTxt;
    [SerializeField] private TextMeshProUGUI UnlockFeeTxt;


    private HitterLevelData MaxValue;
    private HitterLevelData CurValue;
    private int [] UpgradeValue = new int[6];

    int CurLevel;
    bool bUpgrade = false;
    string CurName;

    int SelectIDX;
    int UpdatedIDX = 0;
    Action<int> _action = null;

    public override void Open()
    {
        base.Open();
        BubbleDescObj.gameObject.SetActive(false);
        propertyObj.InitProperty(typeof(UI_Ballers));
    }

    public override void Close()
    {
        base.Close();
        if(_action != null)
        {
            _action(UpdatedIDX);
        }
        _action = null;
    }



    public void UpgradeSetting(int _idx, int _level, Action<int> action)
    {
        SelectIDX = _idx;
        _action = action;
        UpdatedIDX = -1;
        UpgradeBtnObj.gameObject.SetActive(true);
        UnlockBtnObj.gameObject.SetActive(false);
        CharacterData ballerData = KOBManager.Backend.Chart.CharacterData.GetData(_idx);
        setValue(ballerData, _level);
        bUpgrade = true;
        statSetting();
        skillSetting(ballerData, _level);

        int needGold = KOBManager.Backend.Chart.UpgradeData.UpgradeGold(_level + 1, ballerData.rarity);
        UpgradeFeeTxt.text = needGold.ToString("N0");
    }

    public void NormalSetting(int _idx, int _level)
    {
        SelectIDX = _idx;
        _action = null;
        UpdatedIDX = -1;
        UpgradeBtnObj.gameObject.SetActive(false);
        UnlockBtnObj.gameObject.SetActive(false);
        CharacterData ballerData = KOBManager.Backend.Chart.CharacterData.GetData(_idx);
        setValue(ballerData, _level);
        bUpgrade = false;
        statSetting();
        skillSetting(ballerData, _level);
    }

    public void UnlockSetting(int _idx, int _level, Action<int> action)
    {
        SelectIDX = _idx;
        _action = action;
        UpdatedIDX = -1;
        UpgradeBtnObj.gameObject.SetActive(false);
        UnlockBtnObj.gameObject.SetActive(true);
        CharacterData ballerData = KOBManager.Backend.Chart.CharacterData.GetData(_idx);
        setValue(ballerData, _level);
        bUpgrade = false;
        statSetting();
        skillSetting(ballerData, _level);
    }


    private void setValue(CharacterData ballerData, int _level)
    {
        CurName = KOBManager.Localization.GetUILocalizedValue2(ballerData.name_id);
        CurLevel = _level;

        //선수수정할것 - 수정함
        CurValue = KOBManager.Backend.Chart.HitterLevelData.GetData(ballerData.char_idx, _level);
        MaxValue = KOBManager.Backend.Chart.HitterLevelData.GetData(ballerData.char_idx, KOBConstant.MAX_LEVEL);//맥스값
        if (_level < KOBConstant.MAX_LEVEL)
        {
            HitterLevelData nextLevel = KOBManager.Backend.Chart.HitterLevelData.GetData(ballerData.char_idx, _level + 1);
            UpgradeValue[0] = nextLevel.power - CurValue.power;
            UpgradeValue[1] = nextLevel.contact - CurValue.contact;
            UpgradeValue[2] = nextLevel.vision - CurValue.vision;
            UpgradeValue[3] = nextLevel.fielding - CurValue.fielding;
            UpgradeValue[4] = nextLevel.throwing - CurValue.throwing;
            UpgradeValue[5] = nextLevel.speed - CurValue.speed;
        }
    }



    private void statSetting()
    {
        //타격쪽
        int totalHitting = CurValue.power + CurValue.contact + CurValue.vision;
        TotalHitValue.text = totalHitting.ToString("N0");
        HittingValue[0].text = CurValue.power.ToString("N0");
        HittingValue[1].text = CurValue.contact.ToString("N0");
        HittingValue[2].text = CurValue.vision.ToString("N0");

        //필딩쪽
        int totalFielding = CurValue.fielding + CurValue.throwing + CurValue.speed;
        TotalFieldingValue.text = totalFielding.ToString("N0");
        FieldingValue[0].text = CurValue.fielding.ToString("N0");
        FieldingValue[1].text = CurValue.throwing.ToString("N0");
        FieldingValue[2].text = CurValue.speed.ToString("N0");

        if (CurLevel == KOBConstant.MAX_LEVEL)
        {
            // MAX Level
            MaxBtnObj.gameObject.SetActive(false);
        }
        else
        {
            if (bUpgrade == true)
            {
                TitleTxt.text = string.Format("UPGRADE TO POWER LEVEL {0}?", CurLevel);                
                for (int i = 0; i < 6; i++)
                {
                    int count = 0;
                    if (i < 3)
                    {
                        count = i;
                        HittingUpgrade[count].gameObject.SetActive(UpgradeValue[i] > 0 ? true : false);
                        if (UpgradeValue[i] > 0) HittingUpgrade[count].text = string.Format("+{0}", UpgradeValue[i]);
                    }
                    else
                    {
                        count = i - 3;
                        FieldingUpgrade[count].gameObject.SetActive(UpgradeValue[i] > 0 ? true : false);
                        if (UpgradeValue[i] > 0) FieldingUpgrade[count].text = string.Format("+{0}", UpgradeValue[i]);
                    }
                }

                int totalHitupgrade = UpgradeValue[0] + UpgradeValue[1] + UpgradeValue[2];
                int totalFieldupgrade = UpgradeValue[3] + UpgradeValue[4] + UpgradeValue[5];
                TotalHitUpgrade.gameObject.SetActive(totalHitupgrade > 0 ? true : false);
                TotalFieldingUpgrade.gameObject.SetActive(totalFieldupgrade > 0 ? true : false);
                TotalHitUpgrade.text = string.Format("+{0}", totalHitupgrade);
                TotalFieldingUpgrade.text = string.Format("+{0}", totalFieldupgrade);
            }
            else
            {
                TitleTxt.text = string.Format("{0}'S STATS", CurName);
                TotalHitUpgrade.gameObject.SetActive(false);
                TotalFieldingUpgrade.gameObject.SetActive(false);
                for (int i = 0; i < HittingUpgrade.Length; i++) HittingUpgrade[i].gameObject.SetActive(false);
                for (int i = 0; i < FieldingUpgrade.Length; i++) FieldingUpgrade[i].gameObject.SetActive(false);
            }
        }
    }

    private void skillSetting(CharacterData ballerData, int _level)
    {
        //선수수정할것
        HitterSkillData skillData = KOBManager.Backend.Chart.HitterSkillData.GetData(ballerData.char_idx);
        if (skillData != null && skillData.special_skill > 0)
        {
            SkillObj.gameObject.SetActive(true);
            SpecialSkillObj.gameObject.SetActive(true);
        }
        else
        {
            SkillObj.gameObject.SetActive(false);
            SpecialSkillObj.gameObject.SetActive(false);
        }
    }

    private void maxStatSetting()
    {
        TitleTxt.text = string.Format("{0}'S MAX STATS", CurName);

        //타격쪽
        int totalHitting = MaxValue.power + MaxValue.contact + MaxValue.vision;
        TotalHitValue.text = totalHitting.ToString("N0");
        HittingValue[0].text = MaxValue.power.ToString("N0");
        HittingValue[1].text = MaxValue.contact.ToString("N0");
        HittingValue[2].text = MaxValue.vision.ToString("N0");

        //필딩쪽
        int totalFielding = MaxValue.fielding + MaxValue.throwing + MaxValue.speed;
        TotalFieldingValue.text = totalFielding.ToString("N0");
        FieldingValue[0].text = MaxValue.fielding.ToString("N0");
        FieldingValue[1].text = MaxValue.throwing.ToString("N0");
        FieldingValue[2].text = MaxValue.speed.ToString("N0");

        TotalHitUpgrade.gameObject.SetActive(false);
        TotalFieldingUpgrade.gameObject.SetActive(false);
        for (int i = 0; i < HittingUpgrade.Length; i++) HittingUpgrade[i].gameObject.SetActive(false);
        for (int i = 0; i < FieldingUpgrade.Length; i++) FieldingUpgrade[i].gameObject.SetActive(false);

    }


    private void ValueColorSetting(Color color)
    {
        TitleTxt.color = color;

        TotalHitValue.color = color;
        for(int i =0;i< HittingValue.Length;i++) HittingValue[i].color = color;

        TotalFieldingValue.color = color;
        for (int i = 0; i < FieldingValue.Length; i++) FieldingValue[i].color = color;
    }

    public void OnClickDownMax()
    {
        Debug.Log("OnClickDownMax");
        ValueColorSetting(Color.yellow);
        maxStatSetting();
    }

    public void OnClickUpMax()
    {
        Debug.Log("OnClickUpMax");
        ValueColorSetting(Color.white);
        statSetting();
    }

    public void OnClickUpgrade()
    {
        Debug.Log("OnClickUpgrade");
        TRequestUpgradeCard req = new TRequestUpgradeCard()
        { 
            CardIdx = SelectIDX
        };

        KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
        {
            TResultUpgradeCard res = (TResultUpgradeCard)response;
            if (callback?.IsSuccess() == true && res?.isSuccess == true)
            {
                UpdatedIDX = res.CardIdx;
                Close();
            }
            else
            {
                int ErrorCode = res.ErrorCode;
                Debug.Log("에러코드 : " + ErrorCode);
            }
            
        });

    }

    public void OnClickUnlock()
    {
        Debug.Log("OnClickUnlock");
    }

    public void OnTouchStat(GameObject obj)
    {
        Debug.Log("OnTouchStat obj :" + obj);
        if (BubbleDescObj.gameObject.activeSelf == false)
        {
            BubbleDescObj.gameObject.SetActive(true);
            BubbleDescObj.GetComponent<BubbleDescUI>().Init(obj.transform);
        }
    }
}
