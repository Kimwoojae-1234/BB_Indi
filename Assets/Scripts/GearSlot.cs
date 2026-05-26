using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearSlot : MonoBehaviour {
    [SerializeField]
    private UISprite bg_sprite;
    [SerializeField]
    private UISprite icon_sprite;
    [SerializeField]
    private UISprite gradeBg_sprite;
    [SerializeField]
    private UILabel grade_label;
    [SerializeField]
    private UILabel reinforce_label;

    private OldCode.GearData gearData;
    public void SetGearSlot(OldCode.GearData data)
    {
        this.gearData = data;
        bg_sprite.spriteName = string.Format("gear_bg{0}_s", this.gearData.GetGearGrade());
        icon_sprite.spriteName = data.GetIconSpriteName();
        this.grade_label.text = string.Empty;
        this.gradeBg_sprite.spriteName = string.Format("gear_star{0}", this.gearData.GetGearGrade());
        this.reinforce_label.text = this.gearData.GetGearReinforceLev() == 0 ? string.Empty : string.Format("+{0}", this.gearData.GetGearReinforceLev());
    }
    
    public void ResetGearData()
    {
        this.gearData = null;
    }

    public OldCode.GearData GetGearData()
    {
        if (gearData == null)
            return null;
        else
            return gearData;
    }
}
