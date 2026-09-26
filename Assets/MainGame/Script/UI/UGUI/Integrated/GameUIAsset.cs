using TMPro;
using UnityEngine;
namespace BaseBall.BallPlay.UGUI
{
    // Retains the resource prefab identity while its data uses Unity sprites/fonts.
    public sealed class GameUIAsset : MonoBehaviour
    {
        public GameUISpriteCatalog sprites;
        public TMP_FontAsset bitmapFont;
        public Font dynamicFont;
    }
}
