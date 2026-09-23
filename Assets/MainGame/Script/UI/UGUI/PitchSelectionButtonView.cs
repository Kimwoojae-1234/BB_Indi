using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BaseBall.BallPlay
{
    /// <summary>UGUI presentation and release input for one pitch. Match rules stay in ControlPitchingSelect.</summary>
    public sealed class PitchSelectionButtonView : Button
    {
        public Image strength;
        public Image pitchName;
        public TMP_Text number;
        public RawImage effect;
        public GameObject light;
        public ScoreboardSpriteCatalog sprites;
        public ScoreboardPositionTween movement;
        public Vector3 effectOffset;

        private Coroutine entrance;
        private Coroutine pressAnimation;
        private int? pressedPointer;
        private int slot;
        private float effectScale = 1;

        // Rings overlap neighbouring rows. Their shared top layer preserves NGUI
        // depth 5 while the row itself moves and scales independently.
        public void SynchronizeEffect()
        {
            if (effect == null) return;
            effect.transform.position = transform.TransformPoint(effectOffset);
            effect.transform.rotation = transform.rotation;
            effect.transform.localScale = Vector3.Scale(new Vector3(effectScale, effectScale, 1), transform.localScale);
        }

        private void LateUpdate() { if (effect != null && effect.gameObject.activeSelf) SynchronizeEffect(); }

        public void SetEffectScale(float scale) { effectScale = scale; SynchronizeEffect(); }

        public void SetPresentation(PitchingArsenal pitch, int value)
        {
            // The legacy atlas has no 106/107 entries. Preserve its blank label
            // for those unsupported types rather than substitute a different pitch.
            string key = "pselect_" + (int)pitch;
            Sprite sprite = null;
            foreach (var entry in sprites.entries)
                if (entry.name == key) { sprite = entry.sprite; break; }
            pitchName.sprite = sprite;
            pitchName.enabled = sprite != null;
            number.text = value.ToString();
            strength.sprite = sprites.Get("pselect_ball_stat" + (value >= 100 ? 4 : value >= 80 ? 3 : value >= 60 ? 2 : 1)).sprite;
        }

        public void Begin(PitchingArsenal pitch, int value, int index)
        {
            StopAllCoroutines();
            entrance = pressAnimation = null;
            pressedPointer = null;
            slot = index;
            transform.localScale = Vector3.one;
            movement.StopAt(new Vector3(400 + 20 * slot, 66 * slot, 0));
            light.SetActive(false);
            effect.gameObject.SetActive(false);
            SetPresentation(pitch, value);
            interactable = true;
            entrance = StartCoroutine(Enter());
        }

        public void Release(int selected, int index)
        {
            interactable = false;
            pressedPointer = null;
            if (entrance != null) { StopCoroutine(entrance); entrance = null; }
            StartCoroutine(Exit(selected, index));
        }

        public void PlaySelectionEffect() { StartCoroutine(SelectionEffect()); }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable() || pressedPointer.HasValue) return;
            base.OnPointerDown(eventData);
            pressedPointer = eventData.pointerId;
            ScaleTo(Vector3.one * .9f);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || pressedPointer != eventData.pointerId) return;
            base.OnPointerUp(eventData);
            pressedPointer = null;
            ScaleTo(Vector3.one);
            // NGUI's onRelease selected even if the pointer left the rectangle.
            if (IsActive() && IsInteractable()) onClick.Invoke();
        }

        // Dispatch only once, from release; normal UGUI also sends PointerClick.
        public override void OnPointerClick(PointerEventData eventData) { }

        protected override void OnDisable()
        {
            StopAllCoroutines();
            entrance = pressAnimation = null;
            pressedPointer = null;
            if (movement != null) movement.enabled = false;
            transform.localScale = Vector3.one;
            if (light != null) light.SetActive(false);
            if (effect != null) effect.gameObject.SetActive(false);
            base.OnDisable();
        }

        private void MoveTo(float duration, Vector3 target)
        {
            movement.from = transform.localPosition;
            movement.to = target;
            movement.duration = duration;
            movement.PlayFromStart();
        }

        private IEnumerator Enter()
        {
            yield return new WaitForSeconds(slot * .1f);
            MoveTo(.2f, new Vector3(20 * slot - 50, 66 * slot, 0));
            yield return new WaitForSeconds(.2f);
            MoveTo(.1f, new Vector3(20 * slot, 66 * slot, 0));
            entrance = null;
        }

        private IEnumerator Exit(int selected, int index)
        {
            yield return new WaitForSeconds(slot == selected ? .95f : index * .05f);
            MoveTo(.1f, new Vector3(20 * index - 50, 66 * index, 0));
            yield return new WaitForSeconds(.1f);
            MoveTo(.2f, new Vector3(20 * index + 500, 66 * index, 0));
        }

        private IEnumerator SelectionEffect()
        {
            light.SetActive(true);
            yield return new WaitForSeconds(.2f);
            light.SetActive(false);
            effect.gameObject.SetActive(true);
            effect.color = new Color(effect.color.r, effect.color.g, effect.color.b, 0);
            SetEffectScale(.3f);
            StartCoroutine(ExpandEffect());
            yield return new WaitForSeconds(.4f);
            effect.gameObject.SetActive(false);
        }

        private IEnumerator ExpandEffect()
        {
            for (float elapsed = 0; elapsed < .4f; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / .4f);
                var color = effect.color; color.a = t; effect.color = color;
                SetEffectScale(Mathf.Lerp(.3f, .87f, t));
                yield return null;
            }
            var final = effect.color; final.a = 1; effect.color = final;
            SetEffectScale(.87f);
        }

        private void ScaleTo(Vector3 target)
        {
            if (pressAnimation != null) StopCoroutine(pressAnimation);
            pressAnimation = StartCoroutine(Scale(target));
        }

        private IEnumerator Scale(Vector3 target)
        {
            var from = transform.localScale;
            for (float elapsed = 0; elapsed < .2f; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / .2f);
                t -= Mathf.Sin(t * Mathf.PI * 2) / (Mathf.PI * 2);
                transform.localScale = Vector3.LerpUnclamped(from, target, t);
                yield return null;
            }
            transform.localScale = target;
            pressAnimation = null;
        }
    }
}
