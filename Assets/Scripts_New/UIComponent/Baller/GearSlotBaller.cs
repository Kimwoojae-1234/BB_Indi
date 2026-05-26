using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GearSlotBaller : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject AddObj;
    [SerializeField] private Image GearFrame;
    [SerializeField] private Image GearIcon;
    [SerializeField] public KOBGearType Type;

    public int GearIndex;    
    public int Slot;
    

    public void SetEmpty(int slot)
    {
        GearIndex = 0;
        Slot = slot;
        GearFrame.color = KOBUtil.ConvertColor(0x828282);
        GearIcon.gameObject.SetActive(false);
        AddObj.gameObject.SetActive(true);
    }

    public void SetGear(int gearIndex, int slot)
    {
        GearIndex = gearIndex;
        Slot = slot;
        GearFrame.color = KOBUtil.ConvertColor(0xFFFFFF);
        AddObj.gameObject.SetActive(false);
        GearIcon.gameObject.SetActive(true);        
    }

    /// <summary>
    /// 볼러 UI에서 터치한 경우
    /// </summary>
    public void OnClickTouch()
    {
        UI_Ballers baller = KOBManager.UI.GetUIWindow<UI_Ballers>();
        if (baller != null) baller.OpenGearPopup(Slot);


        //
    }

    /// <summary>
    /// 스켈 팝업에서 터치한 경우
    /// </summary>
    public void OnClickTouchSkillPopup()
    {

    }

}
