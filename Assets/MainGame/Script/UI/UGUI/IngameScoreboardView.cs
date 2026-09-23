using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaseBall.BallPlay
{
    /// <summary>Presentation only. The match controller remains the source of game state.</summary>
    public sealed class IngameScoreboardView : MonoBehaviour
    {
        public Image homeLogo;
        public Image awayLogo;
        public TMP_Text homeName;
        public TMP_Text awayName;
        public TMP_Text homeScore;
        public TMP_Text awayScore;
        public TMP_Text inningInfo;
        public Image topBottom;
        public Image[] ballCount;
        public Image[] strikeCount;
        public Image[] outCount;
        public Image[] baseOn;
        public GameObject[] indicator;
        public ScoreboardSpriteCatalog sprites;
        public Canvas displayCanvas;
        public CanvasGroup opacity;
        public ScoreboardPositionTween entrance;

        private readonly List<Material> ownedMaterials = new List<Material>();
        private int currentQueue = -1;

        public void SetTeams(int homeIndex, string home, int awayIndex, string away)
        {
            SetLogo(homeLogo, homeIndex);
            SetLogo(awayLogo, awayIndex);
            homeName.text = home;
            awayName.text = away;
            inningInfo.text = "1";
            SetSprite(topBottom, "scoreboard_top");
            SetCount(ballCount, 0, "scoreboard_ball");
            SetCount(strikeCount, 0, "scoreboard_strike");
            SetCount(outCount, 0, "scoreboard_out");
            foreach (var image in baseOn) SetSprite(image, "scoreboard_base");
        }

        public void SetState(int inning, bool topInning, bool myHome,
            int myScore, int opponentScore, int balls, int strikes, int outs,
            bool firstBase, bool secondBase, bool thirdBase)
        {
            inningInfo.text = inning.ToString();
            SetSprite(topBottom, topInning ? "scoreboard_top" : "scoreboard_bottom");
            indicator[0].SetActive(topInning);
            indicator[1].SetActive(!topInning);
            homeScore.text = (myHome ? myScore : opponentScore).ToString();
            awayScore.text = (myHome ? opponentScore : myScore).ToString();
            SetCount(ballCount, balls, "scoreboard_ball");
            SetCount(strikeCount, strikes, "scoreboard_strike");
            SetCount(outCount, outs, "scoreboard_out");
            SetSprite(baseOn[0], firstBase ? "scoreboard_baseon" : "scoreboard_base");
            SetSprite(baseOn[1], secondBase ? "scoreboard_baseon" : "scoreboard_base");
            SetSprite(baseOn[2], thirdBase ? "scoreboard_baseon" : "scoreboard_base");
        }

        private void SetCount(Image[] images, int count, string litSprite)
        {
            for (int i = 0; i < images.Length; i++)
                SetSprite(images[i], i < count ? litSprite : "scoreboard_round");
        }

        private void SetSprite(Image image, string spriteName)
        {
            image.sprite = sprites.Get(spriteName).sprite;
        }

        private void SetLogo(Image image, int teamIndex)
        {
            var entry = sprites.Get("logo_" + teamIndex);
            image.sprite = entry.sprite;
            image.rectTransform.sizeDelta = entry.nativeSize;
        }

        // During incremental migration the parent still owns the other NGUI HUD elements.
        // Accept values rather than an NGUI component so the new view has no NGUI dependency.
        public void SetInheritedRendering(float alpha, int renderQueue, int sortingOrder)
        {
            opacity.alpha = alpha;
            displayCanvas.sortingOrder = sortingOrder;
            if (ownedMaterials.Count == 0)
            {
                var copies = new Dictionary<Material, Material>();
                foreach (var graphic in GetComponentsInChildren<Graphic>(true))
                {
                    var original = graphic is TMP_Text label ? label.fontSharedMaterial : graphic.material;
                    if (!copies.TryGetValue(original, out var copy))
                    {
                        copy = new Material(original) { name = original.name + " (Scoreboard)", hideFlags = HideFlags.DontSave };
                        copies.Add(original, copy);
                        ownedMaterials.Add(copy);
                    }
                    if (graphic is TMP_Text text) text.fontSharedMaterial = copy;
                    else graphic.material = copy;
                }
            }
            if (currentQueue == renderQueue) return;
            foreach (var material in ownedMaterials) material.renderQueue = renderQueue;
            currentQueue = renderQueue;
        }

        private void OnDestroy()
        {
            foreach (var material in ownedMaterials)
            {
                if (Application.isPlaying) Destroy(material);
                else DestroyImmediate(material);
            }
            ownedMaterials.Clear();
        }
    }
}
