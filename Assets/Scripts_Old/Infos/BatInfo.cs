using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatInfo
{
    private BatBaseData batBaseData;
    private BatAbilityBaseData AType_Ability;
    private BatAbilityBaseData BType_Ability;
    private BatAbilityBaseData CType_Ability;
    private int BatHaveCount;
    private bool isEquip;
    public BatInfo(int batIndex)
    {
        /*batBaseData = MainManager.Database.LoadBatBaseData(batIndex);
        if (batBaseData == null)
            return;
        if (batBaseData.AbilityTypeA > 0)
        {
            AType_Ability = MainManager.Database.LoadBatAbilityBaseData(batBaseData.AbilityTypeA);
        }

        if (batBaseData.AbilityTypeB > 0)
        {
            BType_Ability = MainManager.Database.LoadBatAbilityBaseData(batBaseData.AbilityTypeB);
        }

        if (batBaseData.AbilityTypeC > 0)
        {
            CType_Ability = MainManager.Database.LoadBatAbilityBaseData(batBaseData.AbilityTypeC);
        }*/
    }

    public BatInfo()
    {
        batBaseData = new BatBaseData();
        batBaseData.Idx = 0;
        batBaseData.AbilityTypeA = 0;
        batBaseData.AbilityTypeB = 0;
        batBaseData.AbilityTypeC = 0;
    }

    public void SetHaveBatCount(int Quantity)
    {
        BatHaveCount = Quantity;
    }

    public void AddHaveBatCount(int Quantity)
    {
        BatHaveCount += Quantity;
    }

    public void SetIsEquipBat(bool isEquip)
    {
        this.isEquip = isEquip;
    }

    public int GetBatIndex()
    {
        if (batBaseData == null)
            return 0;
        return batBaseData.Idx;
    }

    public string GetBatNameID()
    {
        if (batBaseData == null)
            return string.Empty;
        return batBaseData.NameId;
    }

    public string GetBatDescID()
    {
        if (batBaseData == null)
            return string.Empty;
        return batBaseData.DescId;
    }

    public Sprite GetBatSprite()
    {
        if (batBaseData == null)
            return null;

        Sprite BatIcon = null;// MainManager.UI.GetAtlas("Bat", batBaseData.IconTag);

        return BatIcon;
    }

    public string GetBatIconTag()
    {
        if (batBaseData == null)
            return string.Empty;
        return batBaseData.IconTag;
    }

    public bool GetIsEquip()
    {
        return isEquip;
    }

    public bool isDisplay()
    {
        if (batBaseData == null)
            return false;

        return System.Convert.ToBoolean(batBaseData.Display);
    }

    public BatAbilityBaseData GetATypeAbilityData()
    {
        if (AType_Ability == null || batBaseData.AbilityTypeA <= 0)
            return null;
        return AType_Ability;
    }

    public short GetATypeAbilityLevel()
    {
        if (batBaseData == null || batBaseData.AbilityTypeA <= 0)
            return 0;

        return batBaseData.AbilityTypeALevel;
    }

    public BatAbilityBaseData GetBTypeAbilityData()
    {
        if (BType_Ability == null || batBaseData.AbilityTypeB <= 0)
            return null;
        return BType_Ability;
    }

    public short GetBTypeAbilityLevel()
    {
        if (batBaseData == null || batBaseData.AbilityTypeB <= 0)
            return 0;

        return batBaseData.AbilityTypeBLevel;
    }

    public BatAbilityBaseData GetCTypeAbilityData()
    {
        if (CType_Ability == null || batBaseData.AbilityTypeC <= 0)
            return null;
        return CType_Ability;
    }

    public short GetCTypeAbilityLevel()
    {
        if (batBaseData == null || batBaseData.AbilityTypeC <= 0)
            return 0;

        return batBaseData.AbilityTypeCLevel;
    }

    public int GetHaveBatCount()
    {
        return BatHaveCount;
    }
}
