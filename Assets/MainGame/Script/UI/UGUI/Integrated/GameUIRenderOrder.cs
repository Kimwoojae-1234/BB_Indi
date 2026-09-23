using System.Collections.Generic;
using UnityEngine;

namespace BaseBall.BallPlay.UGUI
{
    // Compress the panel/widget depth pairs instead of multiplying depths into Canvas's 16-bit range.
    public static class GameUIRenderOrder
    {
        private static readonly HashSet<GameUIPanel> panels = new HashSet<GameUIPanel>();
        private static readonly HashSet<GameUIElement> elements = new HashSet<GameUIElement>();
        private static readonly Dictionary<(GameUIPanel, int), int> orders = new Dictionary<(GameUIPanel, int), int>();
        private static bool dirty = true;
        public static void Register(GameUIPanel panel) { panels.Add(panel); dirty = true; }
        public static void Remove(GameUIPanel panel) { panels.Remove(panel); dirty = true; }
        public static void Register(GameUIElement element) { elements.Add(element); dirty = true; }
        public static void Remove(GameUIElement element) { elements.Remove(element); dirty = true; }
        public static void Invalidate() { dirty = true; }
        public static int Get(GameUIPanel panel, int depth)
        {
            if (dirty)
            {
                dirty = false; orders.Clear();
                var keys = new HashSet<(GameUIPanel panel, int depth)> { (null, 0) };
                foreach (var p in panels) if (p != null) keys.Add((p, 0));
                foreach (var e in elements) if (e != null) keys.Add((e.Panel, e.depth));
                var sorted = new List<(GameUIPanel panel, int depth)>(keys);
                sorted.Sort((a, b) => {
                    int comparison = (a.panel != null ? a.panel.depth : 0).CompareTo(b.panel != null ? b.panel.depth : 0);
                    if (comparison == 0) comparison = (a.panel != null ? a.panel.GetInstanceID() : 0).CompareTo(b.panel != null ? b.panel.GetInstanceID() : 0);
                    return comparison != 0 ? comparison : a.depth.CompareTo(b.depth);
                });
                for (int i = 0; i < sorted.Count; i++) orders[sorted[i]] = i + 10;
            }
            return orders.TryGetValue((panel, depth), out int order) ? order : 0;
        }
    }
}
