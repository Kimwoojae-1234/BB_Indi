using System.Collections.Generic;
using UnityEngine;
public static class KOBUtil 
{
    public static SlotEquipType GetEquipType(int skillindex)
    {
        int type = (skillindex % 1000) / 100;
        if (type == 2 || type == 3)
        {
            return SlotEquipType.FieldingSkill;
        }
        else
        {
            return SlotEquipType.HittingSkill;
        }
    }


    public static string GetGoldIconSprite(int gold)
    {
        //50/200/500/1000
        if (gold <= 50) return "Icon_ShopIcon_Gold0";
        else if (gold <= 200) return "Icon_ShopIcon_Gold1";
        else if (gold <= 500) return "Icon_ShopIcon_Gold2";
        else if (gold <= 1000) return "Icon_ShopIcon_Gold3";
        else return "Icon_ShopIcon_Gold4";
    }

    public static string GetGemIconSprite(int gem)
    {
        //50/200/500/1000
        if (gem <= 50) return "Icon_ShopIcon_Gem0";
        else if (gem <= 200) return "Icon_ShopIcon_Gem1";
        else if (gem <= 500) return "Icon_ShopIcon_Gem2";
        else if (gem <= 1000) return "Icon_ShopIcon_Gem3";
        else return "Icon_ShopIcon_Gem4";
    }



    public static Color ConvertColor(int colorValue)
    {
        int rValue = (colorValue >> 16) & 0xff;
        int gValue = (colorValue >> 8) & 0xff;
        int bValue = (colorValue) & 0xff;


        float r = (float)rValue / 255.0f;
        float g = (float)gValue / 255.0f;
        float b = (float)bValue / 255.0f;

        //Debug.Log("r = " +rValue +"  (f) " + r);
        //Debug.Log("g = " + gValue + "  (f) " + g);
        //Debug.Log("b = " + bValue + "  (f) " + b);

        return new Color(r, g, b);
    }




    public static Color GetRarityColor(KOBRarity rarity)
    {
        if(rarity == KOBRarity.COMMON)
            return ConvertColor(0x70D4EE);
        else if (rarity == KOBRarity.RARE)
            return ConvertColor(0xFE9751);
        else if (rarity == KOBRarity.EPIC)
            return ConvertColor(0xD453F0);
        else if (rarity == KOBRarity.LEGENDARY)
            return ConvertColor(0xFFFD4A);
        else //if (rarity == KOBRarity.BLACK)
            return ConvertColor(0xDF0854);
    }


    public static string GetHandString(KOBHand hand)
    {
        string _id = string.Empty;
        if (hand == KOBHand.Left)
        {
            _id = "Ingame.LeftHand";
        }
        else if(hand == KOBHand.Right)
        {
            _id = "Ingame.RightHand";
        }
        else
        {
            _id = "Ingame.SwitchHand";
        }

        return KOBManager.Localization.GetUILocalizedValue2(_id);
    }

    public static string GetHandString2(KOBHand hand)
    {
        string _id = string.Empty;
        if (hand == KOBHand.Left)
        {
            return UILocalization.Get("ui.c67e7bb93ccc");
        }
        else if (hand == KOBHand.Right)
        {
            return UILocalization.Get("ui.d52854f39765");
        }
        else
        {
            return UILocalization.Get("ui.3e44c920427e");
        }

        //return KOBManager.Localization.GetUILocalizedValue2(_id);
    }

    public static string GetPosString(KOBPosition pos)
    {
        
        if (pos == KOBPosition.Pitcher)
        {
            return "P";
        }
        else if (pos == KOBPosition.Catcher)
        {
            return "C";
        }
        else if (pos == KOBPosition.First)
        {
            return "1B";
        }
        else if (pos == KOBPosition.Second)
        {
            return "2B";
        }
        else if (pos == KOBPosition.Third)
        {
            return "3B";
        }
        else if (pos == KOBPosition.Short)
        {
            return "SS";
        }
        else if (pos == KOBPosition.Left)
        {
            return "LF";
        }
        else if (pos == KOBPosition.Center)
        {
            return "CF";
        }
        else if (pos == KOBPosition.Right)
        {
            return "RF";
        }
        else if (pos == KOBPosition.DH)
        {
            return "RF";
        }
        else if (pos == KOBPosition.InfieldUtil)
        {
            return "IF";
        }
        else if (pos == KOBPosition.OutfieldUtil)
        {
            return "OF";
        }
        else
        {
            return UILocalization.Get("ui.6b42874e3cd2");
        }
    }


    public static string GetPosString2(KOBPosition pos)
    {

        if (pos == KOBPosition.Pitcher)
        {
            return UILocalization.Get("ui.556473af7d23");
        }
        else if (pos == KOBPosition.Catcher)
        {
            return UILocalization.Get("ui.2defbd91dfc4");
        }
        else if (pos == KOBPosition.First)
        {
            return UILocalization.Get("ui.ce8ecea6f0d7");
        }
        else if (pos == KOBPosition.Second)
        {
            return UILocalization.Get("ui.bde43fcfa65b");
        }
        else if (pos == KOBPosition.Third)
        {
            return UILocalization.Get("ui.4713e8677c40");
        }
        else if (pos == KOBPosition.Short)
        {
            return UILocalization.Get("ui.8a03f0eb05a5");
        }
        else if (pos == KOBPosition.Left)
        {
            return UILocalization.Get("ui.74b37e3b2683");
        }
        else if (pos == KOBPosition.Center)
        {
            return UILocalization.Get("ui.49a5d4be0815");
        }
        else if (pos == KOBPosition.Right)
        {
            return UILocalization.Get("ui.c6e72acc9efa");
        }
        else if (pos == KOBPosition.DH)
        {
            return "DH";
        }
        else if (pos == KOBPosition.InfieldUtil)
        {
            return UILocalization.Get("ui.9a10392087c2");
        }
        else if (pos == KOBPosition.OutfieldUtil)
        {
            return UILocalization.Get("ui.0d27eea06a01");
        }
        else
        {
            return UILocalization.Get("ui.e4746d784125");
        }
    }

    public static string GetPosPatter(KOBPosition pos)
    {
        if (pos == KOBPosition.Pitcher)
        {
            return "PosPattern2";
        }
        else if (pos == KOBPosition.Catcher)
        {
            return "PosPattern3";
        }
        else if (pos == KOBPosition.First)
        {
            return "PosPattern3";
        }
        else if (pos == KOBPosition.Second)
        {
            return "PosPattern3";
        }
        else if (pos == KOBPosition.Third)
        {
            return "PosPattern3";
        }
        else if (pos == KOBPosition.Short)
        {
            return "PosPattern3";
        }
        else if (pos == KOBPosition.Left)
        {
            return "PosPattern4";
        }
        else if (pos == KOBPosition.Center)
        {
            return "PosPattern4";
        }
        else if (pos == KOBPosition.Right)
        {
            return "PosPattern4";
        }
        else if (pos == KOBPosition.DH)
        {
            return "PosPattern2";
        }
        else if (pos == KOBPosition.InfieldUtil)
        {
            return "PosPattern1";
        }
        else if (pos == KOBPosition.OutfieldUtil)
        {
            return "PosPattern1";
        }
        else
        {
            return "PosPattern0";
        }
    }


    public static T FindFirstActiveComponent<T>() where T : MonoBehaviour
    {
        T[] components = Object.FindObjectsOfType<T>();

        if (components.Length > 0)
        {
            Debug.Log(typeof(T).Name + " �߰ߵ�: " + components[0].gameObject.name);
            return components[0];
        }
        else
        {
            Debug.Log("Ȱ��ȭ�� " + typeof(T).Name + "�� ����");
            return null;
        }
    }
}
