using UnityEngine;
using UnityEngine.UI;

namespace BaseBall.BallPlay.UGUI
{
    /// <summary>An invisible UGUI hit target using the original collider only as shape data.
    /// No physics raycaster or NGUI input dispatcher participates in UI events.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class GameUIHitTarget : Graphic, ICanvasRaycastFilter
    {
        public Collider shape;
        public GameUIPointer pointer;
        private void LateUpdate()
        {
            if (pointer == null) return;
            var e = pointer.GetComponentInParent<GameUIElement>();
            canvas.sortingOrder = GameUIRenderOrder.Get(pointer.GetComponentInParent<GameUIPanel>(), e != null ? e.depth : 0);
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
            if (pointer == null || !pointer.isActiveAndEnabled || shape == null || !shape.enabled) return false;
            if (GameUIElement.InheritedAlpha(pointer.transform) < .001f) return false;
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
