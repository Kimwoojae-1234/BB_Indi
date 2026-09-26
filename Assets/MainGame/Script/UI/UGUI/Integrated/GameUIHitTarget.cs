using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BaseBall.BallPlay.UGUI
{
    /// <summary>An invisible UGUI hit target using the original collider only as shape data.
    /// No physics raycaster or NGUI input dispatcher participates in UI events.</summary>
    [DefaultExecutionOrder(75)]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class GameUIHitTarget : Graphic, ICanvasRaycastFilter,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public Collider shape;
        public GameUIPointer pointer;
        // Consume events at the hit surface. Native controls on its parent are driven
        // by the pointer proxy, so a single click must not also toggle them a second time.
        public void OnPointerDown(PointerEventData e) { pointer.OnPointerDown(e); }
        public void OnPointerUp(PointerEventData e) { pointer.OnPointerUp(e); }
        public void OnPointerClick(PointerEventData e) { pointer.OnPointerClick(e); }
        public void OnInitializePotentialDrag(PointerEventData e) { pointer.OnInitializePotentialDrag(e); }
        public void OnBeginDrag(PointerEventData e) { pointer.OnBeginDrag(e); }
        public void OnDrag(PointerEventData e) { pointer.OnDrag(e); }
        public void OnEndDrag(PointerEventData e) { pointer.OnEndDrag(e); }
        public void OnScroll(PointerEventData e) { pointer.OnScroll(e); }
        private void LateUpdate() { Synchronize(); }
        public void Synchronize()
        {
            if (pointer == null || canvas == null) return;
            // Collider data remains the explicit hit area; resizing art must not enlarge
            // deliberately smaller buttons. Refresh its native rectangle when data changes.
            if (shape != null && rectTransform.parent == shape.transform)
            {
                rectTransform.pivot = new Vector2(.5f, .5f);
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
                if (shape is BoxCollider box)
                {
                    rectTransform.localPosition = box.center;
                    rectTransform.sizeDelta = new Vector2(box.size.x, box.size.y);
                }
                else if (shape is CapsuleCollider capsule)
                {
                    rectTransform.localPosition = capsule.center;
                    float diameter = capsule.radius * 2;
                    rectTransform.sizeDelta = new Vector2(capsule.direction == 0 ? Mathf.Max(diameter, capsule.height) : diameter,
                        capsule.direction == 1 ? Mathf.Max(diameter, capsule.height) : diameter);
                }
            }
            var e = pointer.GetComponentInParent<GameUIElement>();
            if (!canvas.isRootCanvas) canvas.overrideSorting = true;
            canvas.sortingOrder = e != null ? GameUIRenderOrder.Get(e) : GameUIRenderOrder.Get(pointer.GetComponentInParent<GameUIPanel>(), 0);
            if (canvas.worldCamera == null) canvas.worldCamera = GameUIRoot.FindCameraForLayer(gameObject.layer);
        }
        protected override void Awake() { base.Awake(); canvasRenderer.cullTransparentMesh = false; }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            // Keep a transparent quad: an empty mesh has depth -1 and is skipped by GraphicRaycaster.
            helper.Clear(); var rect = rectTransform.rect;
            helper.AddVert(new Vector3(rect.xMin, rect.yMin), Color.clear, Vector2.zero);
            helper.AddVert(new Vector3(rect.xMin, rect.yMax), Color.clear, Vector2.up);
            helper.AddVert(new Vector3(rect.xMax, rect.yMax), Color.clear, Vector2.one);
            helper.AddVert(new Vector3(rect.xMax, rect.yMin), Color.clear, Vector2.right);
            helper.AddTriangle(0, 1, 2); helper.AddTriangle(2, 3, 0);
        }
        public bool IsRaycastLocationValid(Vector2 point, Camera camera)
        {
            if (pointer == null || !pointer.Available || shape == null || !shape.enabled) return false;
            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, point, camera, out var world)) return false;
            foreach (var panel in pointer.GetComponentsInParent<GameUIPanel>())
            {
                if (panel.clipping != 1 && panel.clipping != 3) continue;
                var p = panel.ClipTransform.InverseTransformPoint(world); var clip = panel.clipRegion;
                if (Mathf.Abs(p.x - clip.x) > clip.z * .5f || Mathf.Abs(p.y - clip.y) > clip.w * .5f) return false;
            }
            var local = shape.transform.InverseTransformPoint(world);
            if (shape is BoxCollider box)
            {
                var p = local - box.center;
                return Mathf.Abs(p.x) <= box.size.x * .5f && Mathf.Abs(p.y) <= box.size.y * .5f;
            }
            if (shape is CapsuleCollider capsule)
            {
                var p = local - capsule.center;
                float along = Mathf.Max(0, capsule.height * .5f - capsule.radius);
                if (capsule.direction == 0) p.x -= Mathf.Clamp(p.x, -along, along);
                else if (capsule.direction == 1) p.y -= Mathf.Clamp(p.y, -along, along);
                return new Vector2(p.x, p.y).sqrMagnitude <= capsule.radius * capsule.radius;
            }
            return false;
        }
    }
}
