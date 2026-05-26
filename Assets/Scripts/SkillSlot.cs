using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSlot : MonoBehaviour {
    [SerializeField]
    private UISprite icon_sprite;
    [SerializeField]
    private UISprite rank_sprite;
    [SerializeField]
    private UISprite lock_sprite;
    [SerializeField]
    private UISprite circle_sprite;
    [SerializeField]
    private IconSIze eSize = IconSIze.Medium;
    [SerializeField]
    private Transform L_rank_pos;
    [SerializeField]
    private Transform M_rank_pos;
    [SerializeField]
    private Transform S_rank_pos;
    
    private OldCode.SkillData skill_data;
    private string icon_sprite_name;
    private string rank_sprite_name;
    private string circle_sprite_name;
    public bool islock;


    public enum IconSIze
    {
        Small,
        Medium,
        Large,
        MAX,
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skillData"></param>
    /// <param name="skill_size"></param>
	public void SetSkillSlot(OldCode.SkillData skillData, IconSIze skill_size)
    {
        string size = string.Empty;
        eSize = skill_size;
        switch (eSize)
        {
            case IconSIze.Small:
                size = "S";
                rank_sprite.transform.localPosition = S_rank_pos.localPosition;
                break;
            case IconSIze.Medium:
                size = "M";
                rank_sprite.transform.localPosition = M_rank_pos.localPosition;
                break;
            case IconSIze.Large:
                size = "L";
                rank_sprite.transform.localPosition = L_rank_pos.localPosition;
                break;
        }

        skill_data = skillData;
        int skillPlayerType = skillData.CardSkillInfo.skillId / 10000;
        int skillNo = skillData.CardSkillInfo.skillId % 100;
        icon_sprite_name = string.Format("{0}{1}_{2}", skillPlayerType, skillNo.ToString("D4"), eSize == IconSIze.Medium ? "S" : size);
        rank_sprite_name = string.Format("rank{0}_{1}", skillData.CardSkillInfo.rank, size);
        circle_sprite_name = string.Format("circle_{0}_{1}", skillData.CardSkillInfo.rank == 2 ? 3 : skillData.CardSkillInfo.rank, size);
        icon_sprite.spriteName = icon_sprite_name;
        rank_sprite.spriteName = rank_sprite_name;
        circle_sprite.spriteName = circle_sprite_name;
        icon_sprite.MakePixelPerfect();
        rank_sprite.MakePixelPerfect();
        circle_sprite.MakePixelPerfect();
        if(eSize == IconSIze.Small)
        {
            icon_sprite.width = circle_sprite.width - 2;
            icon_sprite.height = circle_sprite.height - 2;
        }
        
        islock = false;            
        lock_sprite.enabled = false;
    }

    public void SetLockSlot()
    {
        icon_sprite.spriteName = string.Empty;
        rank_sprite.spriteName = string.Empty;
        circle_sprite.spriteName = string.Empty;
        islock = true;
        lock_sprite.enabled = true;
    }

    public void SetLockSlot(IconSIze skill_size)
    {
        icon_sprite.spriteName = string.Empty;
        rank_sprite.spriteName = string.Empty;
        circle_sprite.spriteName = string.Empty;
        islock = true;
        lock_sprite.enabled = true;

        lock_sprite.spriteName = (skill_size == IconSIze.Small ? "skill_lock_S" : "skill_lock_M");
        lock_sprite.MakePixelPerfect();
    }

    public void SetSkillEmpty(IconSIze skill_size)
    {
        SetLockSlot();
        lock_sprite.spriteName = (skill_size == IconSIze.Small ? "skill_none_S" : "skill_none_M");
        lock_sprite.MakePixelPerfect();
    }

    /// <summary>
    /// 스킬 연출에서 아이콘 세팅
    /// </summary>
    /// <param name="skillData"></param>
    public void SetSkillSlotSkillUI(OldCode.SkillData skillData)
    {
        string size = "L";
        eSize = IconSIze.Large;
        skill_data = skillData;
        icon_sprite_name = string.Format("{0}_{1}", skillData.CardSkillInfo.skillId, size);
        rank_sprite_name = string.Format("rank{0}_{1}", skillData.CardSkillInfo.rank, size);
        circle_sprite_name = string.Format("circle_{0}_{1}", skillData.CardSkillInfo.rank == 2 ? 3 : skillData.CardSkillInfo.rank, size);
        icon_sprite.spriteName = icon_sprite_name;
        rank_sprite.spriteName = rank_sprite_name;
        circle_sprite.spriteName = circle_sprite_name;
        icon_sprite.MakePixelPerfect();
        rank_sprite.MakePixelPerfect();
        circle_sprite.MakePixelPerfect();
        islock = false;
        lock_sprite.enabled = false;
    }

    public OldCode.SkillData GetSkillData()
    {
        return this.skill_data;
    }
}
