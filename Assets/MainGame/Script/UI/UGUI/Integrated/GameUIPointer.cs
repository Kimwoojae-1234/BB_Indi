using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public List<GameUIAction> onPress = new List<GameUIAction>(), onRelease = new List<GameUIAction>(),
            onClick = new List<GameUIAction>(), onHoverOver = new List<GameUIAction>(), onHoverOut = new List<GameUIAction>();
        public Collider inputCollider;
        public Transform scaleTarget;
        public Vector3 pressedScale = Vector3.one, hoverScale = Vector3.one;
        public bool usesScale;
        public float scaleDuration = .2f;
        private bool hovering;
        private int? releasedPointer;
        public GameUIScroll scroll;
        private int? pointer;
        private Vector3 originalScale;
        private bool dragging;
        private bool Available => isActiveAndEnabled && (inputCollider == null || inputCollider.enabled);
        private void Awake() { if (scaleTarget != null) originalScale = scaleTarget.localScale; }
        private void OnEnable() { GameUIRoot.EnsureInput(gameObject.scene); }
        private void OnDisable() { pointer = releasedPointer = null; dragging = hovering = false; RestoreScale(); }
        private void RestoreScale()
        {
            if (!usesScale || scaleTarget == null) return;
            var tween = scaleTarget.GetComponent<GameUITweenScale>(); if (tween != null) tween.enabled = false;
            scaleTarget.localScale = originalScale;
        }
        private void AnimateScale(Vector3 scale)
        {
            if (usesScale && scaleTarget != null) GameUITweenScale.Begin(scaleTarget.gameObject, scaleDuration, Vector3.Scale(originalScale, scale)).method = GameUITween.Method.EaseInOut;
        }
        public void OnPointerDown(PointerEventData data)
        {
            if (!Available || pointer.HasValue || data.button != PointerEventData.InputButton.Left) return;
            pointer = data.pointerId; releasedPointer = null; dragging = false;
            AnimateScale(pressedScale);
            GameUIAction.InvokeAll(onPress);
        }
        public void OnPointerUp(PointerEventData data)
        {
            if (!pointer.HasValue || pointer.Value != data.pointerId) return;
            releasedPointer = pointer; pointer = null; AnimateScale(hovering ? hoverScale : Vector3.one);
            if (Available) GameUIAction.InvokeAll(onRelease);
        }
        public void OnPointerClick(PointerEventData data)
        {
            bool accepted = releasedPointer == data.pointerId; releasedPointer = null;
            if (accepted && Available && !dragging) GameUIAction.InvokeAll(onClick);
        }
        public void OnPointerEnter(PointerEventData data) { hovering = data.pointerId < 0; if (Available) { if (!pointer.HasValue) AnimateScale(hovering ? hoverScale : Vector3.one); GameUIAction.InvokeAll(onHoverOver); } }
        public void OnPointerExit(PointerEventData data) { hovering = false; if (Available) { if (!pointer.HasValue) AnimateScale(Vector3.one); GameUIAction.InvokeAll(onHoverOut); } }
        public void OnBeginDrag(PointerEventData data) { dragging = true; if (scroll != null) scroll.OnBeginDrag(data); }
        public void OnDrag(PointerEventData data) { if (scroll != null) scroll.OnDrag(data); }
        public void OnEndDrag(PointerEventData data) { if (scroll != null) scroll.OnEndDrag(data); }
    }
}
