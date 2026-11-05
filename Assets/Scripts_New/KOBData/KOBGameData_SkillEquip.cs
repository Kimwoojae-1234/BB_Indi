using System.Linq;

public partial class KOBGameData
{
    // Prototype: available chart skills are learned at level 1 on first equip, at no cost.
    public int EquipBallerSkill(int ballerIdx, int slot, int skillIdx)
    {
        if (!PlayerInfo.BallerList.TryGetValue(ballerIdx, out var baller)) return 102;
        var allowed = KOBManager.Backend.Chart.HitterSkillData.GetData(ballerIdx);
        var skill = KOBManager.Backend.Chart.SkillData.GetData(skillIdx);
        if (allowed == null || slot < 1 || slot > allowed.slot_list.Count ||
            !allowed.skill_list.Contains(skillIdx) || SkillProgression.MaxLevel(skill) < 1) return 109;
        if (baller.level < allowed.slot_list[slot - 1]) return 110;
        var type = slot % 2 == 1 ? SlotEquipType.HittingSkill : SlotEquipType.FieldingSkill;
        if (KOBUtil.GetEquipType(skillIdx) != type) return 109;

        if (!baller.SkillList.TryGetValue(skillIdx, out var learned) || learned == null)
            baller.SkillList[skillIdx] = new KOBSkill { idx = skillIdx, level = 1 };
        else { learned.idx = skillIdx; learned.level = System.Math.Min(SkillProgression.MaxLevel(skill), System.Math.Max(1, learned.level)); }
        // A skill occupies one slot per baller. Equipping it elsewhere moves it.
        foreach (int key in baller.SkillEquip.Where(p => p.Value == skillIdx && p.Key != slot).Select(p => p.Key).ToArray())
            baller.SkillEquip.Remove(key);
        baller.SkillEquip[slot] = skillIdx;
        isPlayerChange = IsChangedData = true;
        return 0;
    }

    public int UpgradeBallerSkill(int ballerIdx, int skillIdx)
    {
        if (!PlayerInfo.BallerList.TryGetValue(ballerIdx, out var baller)) return 102;
        var config = KOBManager.Backend.Chart.HitterSkillData.GetData(ballerIdx);
        var skill = KOBManager.Backend.Chart.SkillData.GetData(skillIdx);
        if (config == null || !config.skill_list.Contains(skillIdx) ||
            !baller.SkillList.TryGetValue(skillIdx, out var learned) || learned == null || learned.level < 1) return 109;
        int max = SkillProgression.MaxLevel(skill);
        if (max < 1 || learned.level >= max) return 111;
        int cost = SkillProgression.UpgradeGold(learned.level + 1);
        if (cost <= 0) return 109;
        if (CurrencyInfo.Gold < cost) return (int)KOBErrorCode.Not_Enough_Gold;
        CurrencyInfo.UpdateCurrency(-cost, 0, 0, 0);
        learned.level++;
        isCurrencyChange = isPlayerChange = IsChangedData = true;
        return 0;
    }
}
