using System;
using System.Collections.Generic;
using System.Linq;
using BaseBall.BallPlay;
using UnityEngine;

// Identity shared by the league, legacy simulation and NGUI presentation.
public static class RttsLiveRoster
{
    private static RttsGameResult match;
    public static int SelectedId { get; private set; }
    public static int League { get; private set; }
    private static readonly bool[] actionUsed = new bool[3];
    public static int Ability100(int league, bool selected) => 80 + Math.Max(0, league - 1) * 3 + (selected ? 10 : 0);
    public static void Begin(RttsGameResult result, int league, int selected)
    {
        match = result; League = league; SelectedId = selected;
        Array.Clear(actionUsed, 0, actionUsed.Length);
    }
    public static bool IsSelected(CPlayer player) => player != null && player.RttsTeamId == 0 && player.RttsPlayerId == SelectedId;
    public static bool TakeAction(int inning, CPlayer batter, CPlayer pitcher, bool myTurn)
    {
        int block = (inning - 1) / 3;
        if (inning < 1 || block >= 3 || actionUsed[block]) return false;
        if (!(myTurn ? IsSelected(batter) : IsSelected(pitcher))) return false;
        actionUsed[block] = true;
        return true;
    }
#if _Test_Local
    public static void Apply(CPlayer player, int side, int slot, bool pitcher)
    {
        if (!Mode.bRttsMode || match == null) return;
        var roster = side == 0 ? match.FirstTeamPlayers : match.SecondTeamPlayers;
        var record = pitcher ? roster.FirstOrDefault(p => p.Position == (int)KOBPosition.Pitcher)
            : slot < roster.Count ? roster[slot] : null;
        player.RttsTeamId = side == 0 ? match.FirstTeamIndex : match.SecondTeamIndex;
        player.RttsPlayerId = record?.PlayerIndex ?? 0;
        if (record == null) return;
        var character = KOBManager.Backend.Chart.CharacterData.GetData(record.PlayerIndex);
        int hitHand = character == null ? CPlayer._RIGHTHAND : (int)character.hand;
        int position = Mathf.Clamp(record.Position - 1, 0, 9);
        player.setIdentity(record.Name, pitcher ? 0 : position, pitcher ? player.getPitcherPosition() : record.BattingOrder - 1,
            CPlayer._RIGHTHAND, hitHand, 0, 0, pitcher ? 0 : position == 0 ? CPlayer._DH : position, slot);
        int ability = Ability100(League, IsSelected(player)) * 10;
        player.setBatterAbility(ability, ability, ability, ability, ability, ability, ability);
        if (pitcher) player.setPitcherAbility(ability, ability, ability, ability, ability, side, slot);
    }
#endif
    public static Texture Portrait(CPlayer player)
    {
        var face = BallerFacePortraits.GetTexture(player.RttsPlayerId);
        if (face != null) return face;
        var character = KOBManager.Backend.Chart.CharacterData.GetData(player.RttsPlayerId);
        var sprite = character == null ? null : PrototypeBallers.Portrait(character.char_idx);
        if (sprite != null) return sprite.texture;
        return KOBManager.Resource.LoadBallerPortraitTemp(player.RttsPlayerId);
    }
}
