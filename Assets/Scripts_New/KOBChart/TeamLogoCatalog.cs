using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Logo 001 belongs exclusively to the player's team. Opponents reuse the available art.
public static class TeamLogoCatalog
{
    private static Sprite mine;
    private static List<KeyValuePair<int, Sprite>> opponents;

    private static void Load()
    {
        if (opponents != null) return;
        mine = Resources.Load<Sprite>("TeamLogo/Middle/LOGO001_M");
        opponents = new List<KeyValuePair<int, Sprite>>();
        foreach (var sprite in Resources.LoadAll<Sprite>("TeamLogo/Middle"))
        {
            string name = sprite.name;
            if (!name.StartsWith("LOGO") || !name.EndsWith("_M")) continue;
            if (int.TryParse(name.Substring(4, name.Length - 6), out int id) && id > 1)
                opponents.Add(new KeyValuePair<int, Sprite>(id, sprite));
        }
        opponents = opponents.OrderBy(p => p.Key).ToList();
    }

    public static Sprite MyTeam { get { Load(); return mine; } }

    public static Sprite Opponent(int requestedLogo)
    {
        Load();
        if (opponents.Count == 0) return null;
        // Keep existing valid assignments. Missing / out-of-range ids wrap through real assets.
        foreach (var pair in opponents) if (pair.Key == requestedLogo) return pair.Value;
        long offset = System.Math.Max(0L, (long)requestedLogo - 2);
        return opponents[(int)(offset % opponents.Count)].Value;
    }

    public static Texture Texture(int logoId)
    {
        var sprite = logoId <= 1 ? MyTeam : Opponent(logoId);
        return sprite != null ? sprite.texture : null;
    }
}
