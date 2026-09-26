using System;
using UnityEngine;

namespace BaseBall.BallPlay.UGUI
{
    [Serializable]
    public sealed class GameUIButtonFeedback
    {
        public GameObject target;
        public bool enabled = true, pixelSnap;
        public Color hover = Color.white, pressed = Color.white, disabledColor = Color.grey;
        public float duration = .2f;
        public string hoverSprite, pressedSprite, disabledSprite;
        public GameUIElement clickOtherSprite;
        Color normal;
        string normalSprite;
        GameUIElement element;
        bool initialized;
        int previous = -1;

        public void Apply(int state)
        {
            if (!enabled || target == null) return;
            if (!initialized)
            {
                element = target.GetComponent<GameUIElement>();
                if (element == null) return;
                normal = element.color; normalSprite = element.spriteName; initialized = true;
            }
            if (state == previous) return;
            previous = state;
            if (clickOtherSprite != null) clickOtherSprite.enabled = state == 2;
            var color = state == 1 ? hover : state == 2 ? pressed : state == 3 ? disabledColor : normal;
            GameUITweenColor.Begin(target, duration, color);
            string sprite = state == 1 ? hoverSprite : state == 2 ? pressedSprite : state == 3 ? disabledSprite : normalSprite;
            if (string.IsNullOrEmpty(sprite)) sprite = normalSprite;
            if (!string.IsNullOrEmpty(sprite)) { element.spriteName = sprite; if (pixelSnap) element.MakePixelPerfect(); }
        }
    }
}
