using System.Linq;

public partial class KOBGameData
{
    public int EquipBallerGear(int ballerIdx, int slot, int baseId)
    {
        if (!PlayerInfo.BallerList.TryGetValue(ballerIdx, out var baller)) return 102;
        var gear = KOBManager.Backend.Chart.GearData.GetData(baseId);
        if (slot < 1 || slot > 4 || gear == null || gear.att_lv != 1 ||
            gear.gearType != BallerGearCatalog.SlotType(slot) || KOBManager.Backend.Chart.GearAttributes.GetData(gear.att_refID) == null) return 109;
        int level = System.Math.Max(1, BallerGearCatalog.OwnedLevel(baller, gear));
        var actual = BallerGearCatalog.AtLevel(gear, level);
        if (actual == null) return 109;
        baller.GearList[baseId] = new KOBGear { idx = baseId, level = level };
        baller.GearEquip[slot] = actual.idx;
        isPlayerChange = IsChangedData = true;
        return 0;
    }
    public int UpgradeBallerGear(int ballerIdx, int baseId)
    {
        if (!PlayerInfo.BallerList.TryGetValue(ballerIdx, out var baller)) return 102;
        var gear = KOBManager.Backend.Chart.GearData.GetData(baseId);
        if (gear == null || gear.att_lv != 1) return 109;
        int level = BallerGearCatalog.OwnedLevel(baller, gear);
        if (level < 1) return 109;
        var next = BallerGearCatalog.AtLevel(gear, level + 1);
        if (next == null) return 111;
        int cost = SkillProgression.UpgradeGold(level + 1);
        if (cost <= 0) return 109;
        if (CurrencyInfo.Gold < cost) return (int)KOBErrorCode.Not_Enough_Gold;
        baller.GearList[baseId] = new KOBGear { idx = baseId, level = level + 1 };
        foreach (int slot in baller.GearEquip.Keys.ToArray())
        {
            var equipped = KOBManager.Backend.Chart.GearData.GetData(baller.GearEquip[slot]);
            if (equipped != null && equipped.gearType == gear.gearType && equipped.att_refID == gear.att_refID)
                baller.GearEquip[slot] = next.idx;
        }
        CurrencyInfo.UpdateCurrency(-cost, 0, 0, 0);
        isCurrencyChange = isPlayerChange = IsChangedData = true;
        return 0;
    }
}
