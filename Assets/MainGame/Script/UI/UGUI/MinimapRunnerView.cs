using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaseBall.BallPlay
{
    public sealed class MinimapRunnerView : MonoBehaviour
    {
        public Image team;
        public Image namebar;
        public TMP_Text playerName;
        public ScoreboardSpriteCatalog sprites;
        private FieldMinimapView owner;
        private bool hideName;
        private Vector3 teamOffset, barOffset, nameOffset;
        private float teamAlpha, barAlpha, nameAlpha;
        private float fadeDuration, fadeElapsed;
        private bool fading;

        public void SetPresentation(int teamIndex, string name, bool hideLabels)
        {
            team.sprite = sprites.Get("minimap_team" + teamIndex).sprite;
            playerName.text = name;
            hideName = hideLabels;
            if (owner == null)
            {
                owner = GetComponentInParent<FieldMinimapView>(true);
                if (owner == null) throw new System.InvalidOperationException("UGUI runner requires a field minimap");
                teamOffset = team.transform.localPosition;
                barOffset = namebar.transform.localPosition;
                nameOffset = playerName.transform.localPosition;
                teamAlpha = team.color.a;
                barAlpha = namebar.color.a;
                nameAlpha = playerName.color.a;
                team.transform.SetParent(owner.teams, false);
                namebar.transform.SetParent(owner.namebars, false);
                playerName.transform.SetParent(owner.names, false);
                owner.rendering.Register(team);
                owner.rendering.Register(namebar);
                owner.rendering.Register(playerName);
            }
            SetVisible(isActiveAndEnabled);
            SyncPosition();
        }

        public void SyncPosition()
        {
            if (owner == null) return;
            Follow(team.transform, teamOffset);
            Follow(namebar.transform, barOffset);
            Follow(playerName.transform, nameOffset);
        }

        private void Follow(Transform visual, Vector3 offset)
        {
            visual.position = transform.TransformPoint(offset);
            visual.rotation = transform.rotation;
            visual.localScale = transform.localScale;
        }

        private void SetVisible(bool visible)
        {
            if (owner == null) return;
            if (team != null) team.gameObject.SetActive(visible);
            if (namebar != null) namebar.gameObject.SetActive(visible && !hideName);
            if (playerName != null) playerName.gameObject.SetActive(visible && !hideName);
        }

        // Runner.destroyRunner also fades this marker before its scheduled removal.
        // The marker's graphics live in separate depth layers and fade together here.
        public void FadeOut(float duration)
        {
            teamAlpha = team.color.a;
            barAlpha = namebar.color.a;
            nameAlpha = playerName.color.a;
            fadeDuration = duration;
            fadeElapsed = 0;
            fading = true;
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private void LateUpdate()
        {
            SyncPosition();
            if (!fading) return;
            fadeElapsed += Time.unscaledDeltaTime;
            float alpha = fadeDuration > 0 ? 1 - Mathf.Clamp01(fadeElapsed / fadeDuration) : 0;
            SetAlpha(team, teamAlpha * alpha);
            SetAlpha(namebar, barAlpha * alpha);
            SetAlpha(playerName, nameAlpha * alpha);
            if (alpha == 0) fading = false;
        }
        private void OnEnable() { SetVisible(true); }
        private void OnDisable() { SetVisible(false); }
        private void OnDestroy()
        {
            if (owner == null) return;
            foreach (var graphic in new Graphic[] { team, namebar, playerName })
            {
                if (graphic == null) continue;
                owner.rendering.Forget(graphic);
                if (Application.isPlaying) Destroy(graphic.gameObject); else DestroyImmediate(graphic.gameObject);
            }
        }
    }
}
