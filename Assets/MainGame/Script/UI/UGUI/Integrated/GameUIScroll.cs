using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ScrollRect scrollRect;
        public void OnBeginDrag(PointerEventData data) { if (enabled && scrollRect != null) scrollRect.OnBeginDrag(data); }
        public void OnDrag(PointerEventData data) { if (enabled && scrollRect != null) scrollRect.OnDrag(data); }
        public void OnEndDrag(PointerEventData data) { if (scrollRect != null) scrollRect.OnEndDrag(data); }
        public void ResetPosition() { if (scrollRect != null) { scrollRect.StopMovement(); scrollRect.normalizedPosition = new Vector2(0, 1); } }
        private void OnEnable() { if (scrollRect != null) scrollRect.enabled = true; }
        private void OnDisable() { if (scrollRect != null) scrollRect.enabled = false; }
    }
}
