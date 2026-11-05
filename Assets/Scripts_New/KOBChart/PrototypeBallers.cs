using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.UI;

// Local presentation-build data. Disable it in JSON to use server charts only.
public static class PrototypeBallers
{
    private static JsonData data;
    private static readonly Dictionary<int, Dictionary<int, HitterLevelData>> levels = new();
    private static readonly Dictionary<int, HitterSkillData> skills = new();
    private static readonly Dictionary<int, Sprite> portraits = new();
    private static bool loaded;
    private static void Load()
    {
        if (loaded) return;
        loaded = true;
        var asset = Resources.Load<TextAsset>("Prototype/PrototypeBallers");
        if (asset == null) return;
        data = JsonMapper.ToObject(asset.text);
        if (!(bool)data["enabled"]) { data = null; return; }
        foreach (JsonData row in data["levels"])
        {
            var item = new HitterLevelData(row);
            if (!levels.ContainsKey(item.char_idx)) levels[item.char_idx] = new Dictionary<int, HitterLevelData>();
            levels[item.char_idx][item.level] = item;
        }
        foreach (JsonData row in data["skills"])
        {
            var item = new HitterSkillData(row);
            skills[item.char_idx] = item;
        }
    }
    public static JsonData Characters { get { Load(); return data?["characters"]; } }
    public static HitterLevelData Level(int id, int level)
    {
        Load();
        return levels.TryGetValue(id, out var list) && list.TryGetValue(level, out var result) ? result : null;
    }
    public static HitterSkillData Skill(int id)
    {
        Load(); return skills.TryGetValue(id, out var result) ? result : null;
    }
    public static Sprite Portrait(int id)
    {
        Load();
        if (!levels.ContainsKey(id)) return null;
        if (portraits.TryGetValue(id, out var cached)) return cached;
        var prefab = Resources.Load<GameObject>("Ballers/baller" + id);
        var image = prefab != null ? prefab.transform.Find("Image") : null;
        var sprite = image != null ? image.GetComponent<Image>().sprite : null;
        portraits[id] = sprite;
        return sprite;
    }
    public static int UnlockCost(CharacterData character, int currentLeague)
    {
        if (character == null) return 0;
        int baseCost = character.unlock_gems > 0 ? character.unlock_gems : 80 * (int)character.rarity;
        return baseCost * (currentLeague < character.league ? 2 : 1);
    }
}
