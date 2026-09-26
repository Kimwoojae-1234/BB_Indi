using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIScroll : MonoBehaviour, IInitializePotentialDragHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public ScrollRect scrollRect;
        private PointerEventData drag;
        private bool Available => isActiveAndEnabled && scrollRect != null && scrollRect.isActiveAndEnabled;
        public void OnInitializePotentialDrag(PointerEventData data)
        {
            if (Available && drag == null && data.button == PointerEventData.InputButton.Left)
                scrollRect.OnInitializePotentialDrag(data);
        }
        public void OnBeginDrag(PointerEventData data)
        {
            if (!Available || drag != null || data.button != PointerEventData.InputButton.Left) return;
            drag = data;
            scrollRect.OnInitializePotentialDrag(data);
            scrollRect.OnBeginDrag(data);
        }
        public void OnDrag(PointerEventData data)
        {
            if (drag == null || drag.pointerId != data.pointerId) return;
            if (!Available) { CancelDrag(data.pointerId); return; }
            drag = data; scrollRect.OnDrag(data);
        }
        public void OnEndDrag(PointerEventData data)
        {
            if (drag == null || drag.pointerId != data.pointerId) return;
            if (scrollRect != null) scrollRect.OnEndDrag(data);
            drag = null;
        }
        public void OnScroll(PointerEventData data) { if (Available) scrollRect.OnScroll(data); }
        public void CancelDrag(int pointerId)
        {
            if (drag == null || drag.pointerId != pointerId) return;
            OnEndDrag(drag);
            if (scrollRect != null) scrollRect.StopMovement();
        }
        public void ResetPosition()
        {
            if (drag != null) CancelDrag(drag.pointerId);
            if (scrollRect != null) { scrollRect.StopMovement(); scrollRect.normalizedPosition = new Vector2(0, 1); }
        }
        private void OnEnable() { if (scrollRect != null) scrollRect.enabled = true; }
        private void OnDisable()
        {
            if (drag != null) CancelDrag(drag.pointerId);
            if (scrollRect != null) { scrollRect.StopMovement(); scrollRect.enabled = false; }
        }
    }
}
