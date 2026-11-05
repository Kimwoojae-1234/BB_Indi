using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BallerGearCatalog
{
    public static KOBGearType SlotType(int slot) => (KOBGearType)slot;
    public static IEnumerable<GearData> Families(KOBGearType type) =>
        KOBManager.Backend.Chart.GearData.Dictionary.Values.Where(g => g.gearType == type && g.att_lv == 1).OrderBy(g => g.idx);
    public static GearData Base(GearData gear) => gear == null ? null : Families(gear.gearType).FirstOrDefault(g => g.att_refID == gear.att_refID);
    public static GearData AtLevel(GearData basis, int level) => basis == null ? null :
        KOBManager.Backend.Chart.GearData.Dictionary.Values.FirstOrDefault(g => g.gearType == basis.gearType && g.att_refID == basis.att_refID && g.att_lv == level);
    public static int OwnedLevel(KOBBaller baller, GearData basis)
    {
        if (baller == null || basis == null) return 0;
        int level = baller.GearList.TryGetValue(basis.idx, out var owned) && owned != null ? owned.level : 0;
        foreach (int id in baller.GearEquip.Values)
        {
            var gear = KOBManager.Backend.Chart.GearData.GetData(id);
            if (gear != null && gear.gearType == basis.gearType && gear.att_refID == basis.att_refID) level = Mathf.Max(level, gear.att_lv);
        }
        return level;
    }
    public static Sprite Icon(KOBGearType type) => Resources.Load<Sprite>("Prototype/GearIcons/" + type);
    public static string TypeName(KOBGearType type) => UILocalization.Get("gear.type." + type);
    public static string Description(GearData gear)
    {
        var a = KOBManager.Backend.Chart.GearAttributes.GetData(gear.att_refID);
        if (a == null) return "";
        var parts = new List<string>();
        string[] keys = { "power", "contact", "vision", "fielding", "throwing", "speed" };
        int[] values = { a.pow, a.con, a.vis, a.fld, a.thw, a.spd };
        for (int i = 0; i < keys.Length; i++) if (values[i] != 0)
            parts.Add(UILocalization.Get("gear.stat." + keys[i]) + " +" + values[i] * gear.att_lv);
        foreach (int id in new[] { a.skill1, a.skill2 }) if (id > 0)
            parts.Add(UILocalization.Get("SkillName." + id));
        return string.Join("\n", parts);
    }
}
