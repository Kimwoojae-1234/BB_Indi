using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUISpriteCatalog : ScriptableObject
    {
        [Serializable] public sealed class Entry
        {
            public string name;
            public Sprite sprite;
            public Vector2 size;
            public Vector4 padding;
        }
        public Entry[] entries = Array.Empty<Entry>();
        public Material material;
        private Dictionary<string, Entry> lookup;
        public bool TryGet(string name, out Entry entry)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<string, Entry>(StringComparer.Ordinal);
                foreach (var item in entries) lookup.Add(item.name, item);
            }
            return lookup.TryGetValue(name ?? "", out entry);
        }
    }
}
