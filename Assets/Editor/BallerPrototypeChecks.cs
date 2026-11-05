using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using UnityEditor;
using UnityEngine;

public static class BallerPrototypeChecks
{
    [MenuItem("Tools/Clutch/Validate Prototype Ballers")]
    public static void Validate()
    {
        var chart = new CharacterChart();
        chart.LoadForPrototype((ok, type, method, error) => Require(ok, error));
        Require(chart.Dictionary.Count == 57, "Expected 57 character records");
        var ballers = chart.Dictionary.Values.Where(c => c.char_type == CharacterType.Ballers).ToArray();
        Require(ballers.Length == 10, "Expected 10 ballers");
        var sprites = new HashSet<Sprite>();
        var levels = new HitterLevelDataChart();
        var skills = new HitterSkillDataChart();
        foreach (var baller in ballers)
        {
            int id = baller.char_idx;
            Require(Enum.IsDefined(typeof(KOBPosition), baller.position), "Position: " + id);
            Require(Enum.IsDefined(typeof(KOBRarity), baller.rarity), "Rarity: " + id);
            var sprite = PrototypeBallers.Portrait(id);
            Require(sprite != null && sprites.Add(sprite), "Missing/duplicate portrait: " + id);
            Require(PrototypeBallers.UnlockCost(baller, baller.league) == baller.unlock_gems, "Normal cost: " + id);
            Require(PrototypeBallers.UnlockCost(baller, baller.league - 1) == baller.unlock_gems * 2, "Early cost: " + id);
            for (int lv = 1; lv <= KOBConstant.MAX_LEVEL; lv++)
            {
                var stat = levels.GetData(id, lv);
                Require(stat != null && stat.power > 0 && stat.contact > 0 && stat.vision > 0 && stat.fielding > 0 && stat.throwing > 0 && stat.speed > 0, "Stats: " + id + "/" + lv);
            }
            var skill = skills.GetData(id);
            Require(skill != null && skill.slot_list.Count == 5 && skill.slot_list[0] == 1, "Skills: " + id);
        }
        var catalog = JsonUtility.FromJson<UILocalization.Catalog>(Resources.Load<TextAsset>("Localization/UIStrings").text);
        foreach (var baller in ballers)
            foreach (var key in new[] { baller.name_id, baller.desc_id })
                Require(catalog.entries.Any(e => e.key == key && !string.IsNullOrEmpty(e.en) && !string.IsNullOrEmpty(e.ko) && !string.IsNullOrEmpty(e.ja)), "Translation: " + key);
        foreach (var path in new[] { "UI/Window/UI_Ballers", "UI/Window/UI_BallersList", "UI/Popup/Popup_StatUpgrade" })
            Require(Resources.Load<GameObject>(path) != null, "Missing UI: " + path);
        const string report = "PASS: 57 characters, 10 unique baller portraits/prefabs, 130 level records, 10 skill configurations, league prices and 3-language names/descriptions.";
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/ballers-validation.txt", report);
        Debug.Log(report);
    }
    private static void Require(bool condition, string error)
    {
        if (!condition) throw new Exception(error);
    }
}
