using System;
using System.Globalization;
using UnityEngine;

/// <summary>Shared presentation for persisted public identity and ranking extra data.</summary>
public static class UserIdentity
{
    public static string ResolveCountry(string backendCountry)
    {
        if (!string.IsNullOrWhiteSpace(backendCountry))
        {
            try { return new RegionInfo(backendCountry).TwoLetterISORegionName; }
            catch (ArgumentException) { }
        }
        try { return RegionInfo.CurrentRegion.TwoLetterISORegionName; }
        catch (ArgumentException) { return "KR"; }
    }

    public static Sprite Portrait(int profileId)
    {
        return Resources.Load<Sprite>($"Sprite/PlayerPortrait/Ui_Deck_BATTER_{Mathf.Clamp(profileId, 1, 8):D3}");
    }

    public static Sprite Flag(string countryCode)
    {
        return string.IsNullOrEmpty(countryCode) ? null : Resources.Load<Sprite>("UserIdentity/Flags/" + countryCode.ToUpperInvariant());
    }

    public static void Decode(string extra, out int profileId, out string countryCode)
    {
        profileId = 1;
        countryCode = string.Empty;
        if (string.IsNullOrEmpty(extra)) return;
        var parts = extra.Split('|');
        if (parts.Length != 2) return;
        if (int.TryParse(parts[0], out int parsed) && parsed > 0) profileId = parsed;
        countryCode = parts[1];
    }
}
