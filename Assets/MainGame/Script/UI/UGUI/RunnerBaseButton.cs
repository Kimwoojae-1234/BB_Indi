using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BaseBall.BallPlay
{
    /// <summary>Press input with the legacy radius-40 capsule's circular projection.</summary>
    public sealed class RunnerBaseButton : Button, ICanvasRaycastFilter
    {
        public float hitRadius = 40;
        public RunnerBaseButton[] peers;
        private int? pointer;

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screenPoint, eventCamera, out var local)) return false;
            float distance = local.sqrMagnitude;
            if (distance > hitRadius * hitRadius) return false;
            // NGUI's equal-depth overlap order was unspecified. Resolve the
            // same circular hit regions by their nearest centre deterministically.
            foreach (var other in peers)
            {
                if (other == null || other == this || !other.IsActive()) continue;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)other.transform, screenPoint, eventCamera, out var point) &&
                    point.sqrMagnitude < distance && point.sqrMagnitude <= other.hitRadius * other.hitRadius) return false;
            }
            return true;
        }

        public override void OnPointerDown(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left || pointer.HasValue || !IsActive() || !IsInteractable()) return;
            base.OnPointerDown(data);
            pointer = data.pointerId;
            onClick.Invoke();
        }

        public override void OnPointerUp(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left || pointer != data.pointerId) return;
            pointer = null;
            base.OnPointerUp(data);
        }

        public override void OnPointerClick(PointerEventData data) { }
        protected override void OnDisable() { pointer = null; base.OnDisable(); }
    }
}
