using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public List<GameUIAction> onPress = new List<GameUIAction>(), onRelease = new List<GameUIAction>(),
            onClick = new List<GameUIAction>(), onHoverOver = new List<GameUIAction>(), onHoverOut = new List<GameUIAction>();
        public Collider inputCollider;
        public Transform scaleTarget;
        public Vector3 pressedScale = Vector3.one, hoverScale = Vector3.one;
        public bool usesScale;
        public List<GameUIButtonFeedback> buttonFeedback = new List<GameUIButtonFeedback>();
        public float scaleDuration = .2f;
        private bool hovering;
        private int? releasedPointer;
        public GameUIScroll scroll;
        private GameUIScroll dragScroll;
        public GameUIProgress progress;
        public GameUIToggle toggle;
        public GameUIInput input;
        public List<GameUIAction> onDragStart = new List<GameUIAction>(), onDrag = new List<GameUIAction>(), onDragEnd = new List<GameUIAction>(), onDoubleClick = new List<GameUIAction>();
        private int? pointer;
        private Vector3 originalScale;
        private bool dragging;
        private int? dragPointer;
        private readonly List<CanvasGroup> groups = new List<CanvasGroup>();
        public bool Available
        {
            get
            {
                float alpha = GameUIElement.InheritedAlpha(transform);
                if (!isActiveAndEnabled || (inputCollider != null && !inputCollider.enabled) || alpha < .001f) return false;
                for (var node = transform; node != null; node = node.parent)
                {
                    node.GetComponents(groups);
                    bool stop = false;
                    foreach (var group in groups)
                    {
                        if (!group.isActiveAndEnabled) continue;
                        if (!group.interactable || !group.blocksRaycasts) return false;
                        alpha *= group.alpha; stop |= group.ignoreParentGroups;
                    }
                    if (stop) break;
                }
                return alpha >= .001f;
            }
        }
        private void Awake() { if (scaleTarget != null) originalScale = scaleTarget.localScale; }
        private void OnEnable() { GameUIRoot.EnsureInput(gameObject.scene); }
        private void LateUpdate()
        {
            int state = !Available ? 3 : pointer.HasValue ? 2 : hovering ? 1 : 0;
            foreach (var feedback in buttonFeedback) feedback.Apply(state);
        }
        private void OnDisable() { CancelDrag(); pointer = releasedPointer = null; dragging = hovering = false; RestoreScale(); }
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
            if (progress != null) progress.Press(data);
            if (input != null) { input.input.Select(); input.input.ActivateInputField(); }
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
            if (accepted && Available && !dragging)
            {
                if (toggle != null) toggle.Click();
                GameUIAction.InvokeAll(onClick);
                if (data.clickCount == 2) GameUIAction.InvokeAll(onDoubleClick);
            }
        }
        public void OnPointerEnter(PointerEventData data) { hovering = data.pointerId < 0; if (Available) { if (!pointer.HasValue) AnimateScale(hovering ? hoverScale : Vector3.one); GameUIAction.InvokeAll(onHoverOver); } }
        public void OnPointerExit(PointerEventData data) { hovering = false; if (Available) { if (!pointer.HasValue) AnimateScale(Vector3.one); GameUIAction.InvokeAll(onHoverOut); } }
        // Runtime row prefabs cannot serialize a reference to their host list.
        // Resolve the nearest list at input time, preserving any explicit binding.
        private GameUIScroll ResolveScroll() { return scroll != null ? scroll : GetComponentInParent<GameUIScroll>(true); }
        public void OnInitializePotentialDrag(PointerEventData data) { if (Available) ResolveScroll()?.OnInitializePotentialDrag(data); }
        public void OnBeginDrag(PointerEventData data)
        {
            if (!Available || pointer != data.pointerId || data.button != PointerEventData.InputButton.Left || dragPointer.HasValue) return;
            dragging = true; dragPointer = data.pointerId;
            dragScroll = ResolveScroll();
            if (dragScroll != null) dragScroll.OnBeginDrag(data);
            GameUIAction.InvokeAll(onDragStart);
        }
        public void OnDrag(PointerEventData data)
        {
            if (dragPointer != data.pointerId) return;
            if (!Available || dragScroll != ResolveScroll()) { CancelDrag(); pointer = releasedPointer = null; RestoreScale(); return; }
            if (dragScroll != null) dragScroll.OnDrag(data);
            if (progress != null) progress.Drag(data);
            GameUIAction.InvokeAll(onDrag);
        }
        public void OnEndDrag(PointerEventData data)
        {
            if (dragPointer != data.pointerId) return;
            if (dragScroll != null) dragScroll.OnEndDrag(data);
            GameUIAction.InvokeAll(onDragEnd);
            dragPointer = null; dragScroll = null;
        }
        public void OnScroll(PointerEventData data) { if (Available) ResolveScroll()?.OnScroll(data); }
        private void CancelDrag()
        {
            if (dragPointer.HasValue && dragScroll != null) dragScroll.CancelDrag(dragPointer.Value);
            dragPointer = null; dragScroll = null;
        }
    }
}
