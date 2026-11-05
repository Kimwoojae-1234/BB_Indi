using System;
using System.Collections.Generic;
using System.Linq;

public static class BeachLeagueRules
{
    public static int StandingWindowStart(int position, int count)
        => Math.Max(0, Math.Min(Math.Max(0, count - 5), position - 2));

    // Keep a fixed five-row view for nearby changes so the player's row visibly moves.
    // Long jumps scroll the same viewport to the final neighbourhood.
    public static int StandingTransitionStart(int oldPosition, int newPosition, int count)
    {
        int target = StandingWindowStart(newPosition, count);
        if (Math.Abs(oldPosition - newPosition) >= 5) return StandingWindowStart(oldPosition, count);
        int minimum = Math.Max(0, Math.Max(oldPosition, newPosition) - 4);
        int maximum = Math.Min(Math.Min(oldPosition, newPosition), Math.Max(0, count - 5));
        return Math.Max(minimum, Math.Min(maximum, target));
    }
    public static int Rank(Dictionary<int, int> table, int id, int minimum = int.MinValue)
    {
        if (!table.TryGetValue(id, out int value) || value < minimum) return 0;
        return 1 + table.Values.Count(v => v > value);
    }

    // Eight fielders; the selected Baller is mandatory. Unlocked Ballers replace stand-ins.
    public static List<T> Roster<T>(IEnumerable<T> players, Func<T, int> id,
        Func<T, bool> isBaller, Func<T, int> power, int selected)
    {
        return players.GroupBy(id).Select(g => g.First())
            .OrderByDescending(p => id(p) == selected)
            .ThenByDescending(isBaller).ThenByDescending(power).ThenBy(id).Take(8).ToList();
    }
}
