using System;
using System.Linq;
using UnityEngine;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUISpriteAnimation : MonoBehaviour
    {
        public GameUIElement element;
        public int mFPS = 30;
        public string mPrefix;
        public bool mLoop = true, mSnap = true, mEndOff;
        public string[] frames = Array.Empty<string>();
        private float elapsed;
        private bool active = true;
        public bool isPlaying => active;
        public bool endoff { get => mEndOff; set => mEndOff = value; }
        private int index;
        public string namePrefix { get => mPrefix; set { mPrefix = value; RebuildSpriteList(); } }
        public int framesPerSecond { get => mFPS; set => mFPS = value; }
        public bool loop { get => mLoop; set => mLoop = value; }
        public void RebuildSpriteList()
        {
            if (element == null || element.sprites == null) return;
            frames = element.sprites.entries.Where(e => e.name.StartsWith(mPrefix ?? "", StringComparison.Ordinal))
                .Select(e => e.name).OrderBy(n => n, StringComparer.Ordinal).ToArray();
        }
        public void ResetToBeginning() { index = 0; active = true; Show(); }
        public void Play() { active = true; }
        public void Pause() { active = false; }
        private void Show() { if (element != null && frames.Length != 0) { element.spriteName = frames[index]; if (mSnap) element.MakePixelPerfect(); } }
        private void Update()
        {
            if (!active || frames.Length <= 1 || mFPS <= 0) return;
            elapsed += Mathf.Min(1, Time.unscaledDeltaTime);
            while (elapsed > 1f / mFPS)
            {
                elapsed -= 1f / mFPS; index++;
                if (index >= frames.Length)
                {
                    index = 0; active = mLoop;
                    if (!active && mEndOff && element != null) element.enabled = false;
                }
                if (active) Show();
            }
        }
    }
}
