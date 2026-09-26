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
        private static readonly Dictionary<GameUIElement, int> elementOrders = new Dictionary<GameUIElement, int>();
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
                dirty = false; orders.Clear(); elementOrders.Clear();
                var keys = new HashSet<(GameUIPanel panel, int depth)> { (null, 0) };
                foreach (var p in panels) if (p != null) keys.Add((p, 0));
                foreach (var e in elements) if (e != null) keys.Add((e.Panel, e.depth));
                var sorted = new List<(GameUIPanel panel, int depth)>(keys);
                sorted.Sort((a, b) => {
                    int comparison = (a.panel != null ? a.panel.depth : 0).CompareTo(b.panel != null ? b.panel.depth : 0);
                    if (comparison == 0) comparison = (a.panel != null ? a.panel.GetInstanceID() : 0).CompareTo(b.panel != null ? b.panel.GetInstanceID() : 0);
                    return comparison != 0 ? comparison : a.depth.CompareTo(b.depth);
                });
                // Preserve the prefab stacks' sibling order for equal-depth widgets after
                // moving their canvases under a shared override-sorting hierarchy.
                // Keep a slot for panel/native views, then order tied widgets by sibling path.
                var nested = new Dictionary<(GameUIPanel, int), List<GameUIElement>>();
                var paths = new Dictionary<GameUIElement, int[]>();
                foreach (var e in elements)
                {
                    if (e == null || e.displayCanvas == null) continue;
                    // A temporarily disabled shared canvas still belongs to its UI root.
                    // Use the hierarchy so hide/show does not collapse equal-depth ranks.
                    var parent = e.displayCanvas.transform.parent;
                    if (parent == null || parent.GetComponentInParent<Canvas>(true) == null) continue;
                    var key = (e.Panel, e.depth);
                    if (!nested.TryGetValue(key, out var group)) nested[key] = group = new List<GameUIElement>();
                    group.Add(e);
                    var path = new List<int>();
                    for (var t = e.transform; t != null; t = t.parent) path.Add(t.GetSiblingIndex());
                    path.Reverse(); paths[e] = path.ToArray();
                }
                int next = 10;
                foreach (var key in sorted)
                {
                    orders[key] = next++;
                    if (!nested.TryGetValue(key, out var group)) continue;
                    group.Sort((a, b) => {
                        var left = paths[a]; var right = paths[b];
                        for (int i = 0; i < left.Length && i < right.Length; i++)
                        {
                            int comparison = left[i].CompareTo(right[i]);
                            if (comparison != 0) return comparison;
                        }
                        int length = left.Length.CompareTo(right.Length);
                        return length != 0 ? length : a.GetInstanceID().CompareTo(b.GetInstanceID());
                    });
                    foreach (var e in group) elementOrders[e] = next++;
                }
            }
            return orders.TryGetValue((panel, depth), out int order) ? order : 0;
        }
        public static int Get(GameUIElement element)
        {
            int fallback = Get(element.Panel, element.depth);
            return elementOrders.TryGetValue(element, out int order) ? order : fallback;
        }
    }
}
