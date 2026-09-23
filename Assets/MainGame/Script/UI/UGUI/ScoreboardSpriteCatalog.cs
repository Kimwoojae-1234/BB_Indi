using System;
using UnityEngine;

namespace BaseBall.BallPlay
{
    public sealed class ScoreboardSpriteCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string name;
            public Sprite sprite;
            public Vector2 nativeSize;
        }

        public Entry[] entries;

        public Entry Get(string spriteName)
        {
            foreach (var entry in entries)
                if (entry.name == spriteName) return entry;
            throw new InvalidOperationException("Scoreboard sprite is missing: " + spriteName);
        }
    }
}
