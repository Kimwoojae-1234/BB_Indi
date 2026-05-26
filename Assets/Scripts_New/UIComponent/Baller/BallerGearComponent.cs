using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallerGearComponent : MonoBehaviour
{
    [SerializeField] private GearSlotBaller[] slot;


    public void InitForCollection(CharacterData ballerData, KOBBaller ballerInfo)
    {
        //4개 모두다 열려있음
        for (int i = 0; i < slot.Length; i++)
        {
            slot[i].gameObject.SetActive(true);
            int slotKey = (i + 1);
            if(ballerInfo.GearEquip.ContainsKey(slotKey))
            {
                int gearIndex = ballerInfo.GearEquip[slotKey];
                slot[i].SetGear(gearIndex, slotKey);
            }
            else
            {
                slot[i].SetEmpty(slotKey);
            }
        }
    }


    public void InitForLocked(CharacterData ballerData)
    {
        //락된건 보여줄 필요 없음
        for (int i = 0; i < slot.Length; i++)
        {
            slot[i].gameObject.SetActive(false);
        }
    }


}
