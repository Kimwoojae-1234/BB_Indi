using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    [DefaultExecutionOrder(-20)]
    public sealed class GameUIScroll : MonoBehaviour, IInitializePotentialDragHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public ScrollRect scrollRect;
        private PointerEventData drag;
        private readonly List<GameUIElement> elements = new List<GameUIElement>();
        private readonly List<Vector3> childPositions = new List<Vector3>();
        private bool Available => isActiveAndEnabled && scrollRect != null && scrollRect.isActiveAndEnabled;
        // Native ScrollRect measures its content rectangle, whereas old lists could
        // add ordinary Transform-based widgets without updating a layout component.
        public void RefreshContentBounds()
        {
            var content = scrollRect != null ? scrollRect.content : null;
            if (content == null) return;
            content.GetComponentsInChildren(true, elements);
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var element in elements)
            {
                if (!element.enabled || element.transform == content || element.GetComponentInParent<GameUIScroll>(true) != this) continue;
                // Hidden tabs still need valid bounds before their first activation.
                // Exclude rows hidden inside the list, independently of its ancestors.
                bool visibleInContent = true;
                for (var parent = element.transform; parent != content; parent = parent.parent)
                    if (!parent.gameObject.activeSelf) { visibleInContent = false; break; }
                if (!visibleInContent) continue;
                var pivot = element.pivotOffset;
                var matrix = content.worldToLocalMatrix * element.transform.localToWorldMatrix;
                for (int corner = 0; corner < 4; corner++)
                {
                    Vector2 point = matrix.MultiplyPoint3x4(new Vector3(((corner & 1) - pivot.x) * element.width,
                        ((corner >> 1) - pivot.y) * element.height, 0));
                    min = Vector2.Min(min, point); max = Vector2.Max(max, point);
                }
            }
            bool empty = float.IsInfinity(min.x);
            var size = empty ? Vector2.zero : Vector2.Max(max - min, Vector2.one);
            var nextPivot = empty ? Vector2.one * .5f : new Vector2(-min.x / size.x, -min.y / size.y);
            if ((content.rect.size - size).sqrMagnitude < .0001f && (content.pivot - nextPivot).sqrMagnitude < .00000001f) return;
            var position = content.localPosition;
            childPositions.Clear();
            for (int i = 0; i < content.childCount; i++) childPositions.Add(content.GetChild(i).localPosition);
            content.pivot = nextPivot;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            content.localPosition = position;
            // Keep authored positions, including children with RectTransform anchors.
            for (int i = 0; i < content.childCount; i++) content.GetChild(i).localPosition = childPositions[i];
        }
        private void LateUpdate() { RefreshContentBounds(); }
        public void OnInitializePotentialDrag(PointerEventData data)
        {
            if (Available && drag == null && data.button == PointerEventData.InputButton.Left)
                scrollRect.OnInitializePotentialDrag(data);
        }
        public void OnBeginDrag(PointerEventData data)
        {
            if (!Available || drag != null || data.button != PointerEventData.InputButton.Left) return;
            RefreshContentBounds();
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
            RefreshContentBounds();
            if (scrollRect != null) { scrollRect.StopMovement(); scrollRect.normalizedPosition = new Vector2(0, 1); }
        }
        public void SetScrollingEnabled(bool value)
        {
            enabled = value;
            // Disabling a component beneath an inactive tab does not call OnDisable.
            if (scrollRect != null) { scrollRect.StopMovement(); scrollRect.enabled = isActiveAndEnabled; }
        }
        private void OnEnable() { RefreshContentBounds(); if (scrollRect != null) scrollRect.enabled = true; }
        private void OnDisable()
        {
            if (drag != null) CancelDrag(drag.pointerId);
            if (scrollRect != null) { scrollRect.StopMovement(); scrollRect.enabled = false; }
        }
    }
}
