using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GearSlotBaller : MonoBehaviour
{
    [SerializeField] private GameObject AddObj;
    [SerializeField] private Image GearFrame, GearIcon;
    [SerializeField] private TMP_Text GearLvTxt, GearTypeTxt;
    [SerializeField] private Image Select;
    [SerializeField] public KOBGearType Type;
    public int GearIndex, Slot;

    public void SetEmpty(int slot)
    {
        GearIndex = 0; Slot = slot; Type = BallerGearCatalog.SlotType(slot);
        if (GearFrame != null) GearFrame.color = Color.white;
        if (GearIcon != null) GearIcon.gameObject.SetActive(false);
        if (AddObj != null) AddObj.SetActive(true);
        if (GearLvTxt != null) GearLvTxt.gameObject.SetActive(false);
        if (GearTypeTxt != null) UILocalization.Set(GearTypeTxt, "gear.type." + Type);
        SetSelect(false);
    }
    public void SetGear(int id, int slot)
    {
        var gear = KOBManager.Backend.Chart.GearData.GetData(id);
        if (gear == null) { SetEmpty(slot); return; }
        GearIndex = id; Slot = slot; Type = gear.gearType;
        if (GearFrame != null) GearFrame.color = Color.white;
        if (AddObj != null) AddObj.SetActive(false);
        if (GearIcon != null)
        {
            GearIcon.gameObject.SetActive(true);
            GearIcon.sprite = BallerGearCatalog.Icon(Type);
            GearIcon.color = Color.white;
            GearIcon.preserveAspect = true;
        }
        if (GearLvTxt != null) { GearLvTxt.gameObject.SetActive(true); GearLvTxt.text = "Lv." + gear.att_lv; }
        if (GearTypeTxt != null) UILocalization.Set(GearTypeTxt, "gear.type." + Type);
        SetSelect(false);
    }
    public void SetSelect(bool selected)
    {
        if (Select == null) return;
        Select.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UISkill, "gear_select");
        Select.gameObject.SetActive(selected);
    }
    public void OnClickTouch() => KOBManager.UI.GetUIWindow<UI_Ballers>()?.OpenGearPopup(Slot);
    public void OnClickTouchSkillPopup() { }
}
