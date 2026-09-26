using System;
using UnityEngine;
namespace BaseBall.BallPlay.UGUI
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameUIAnchors : MonoBehaviour
    {
        [Serializable] public struct Edge { public Transform target; public float relative; public int absolute; }
        public Edge left, right, bottom, top;
        public GameUIElement element;
        public bool screenAnchor, runOnlyOnce;
        public int side = 8;
        public Transform container;
        public Camera uiCamera;
        public Vector2 relativeOffset, pixelOffset;
        int width, height;
        void LateUpdate()
        {
            if (screenAnchor)
            {
                if (runOnlyOnce && width == Screen.width && height == Screen.height) return;
                width = Screen.width; height = Screen.height;
                Rect bounds = Bounds(container, transform.parent, uiCamera != null ? uiCamera : GameUIRoot.FindCameraForLayer(gameObject.layer));
                var points = new[] { new Vector2(0, 0), new Vector2(0, .5f), new Vector2(0, 1), new Vector2(.5f, 1), new Vector2(1, 1), new Vector2(1, .5f), new Vector2(1, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f) };
                Vector2 p = points[Mathf.Clamp(side, 0, 8)] + relativeOffset;
                transform.localPosition = new Vector3(Mathf.LerpUnclamped(bounds.xMin, bounds.xMax, p.x) + pixelOffset.x, Mathf.LerpUnclamped(bounds.yMin, bounds.yMax, p.y) + pixelOffset.y, transform.localPosition.z);
                return;
            }
            if (element == null) return;
            var position = transform.localPosition; var pivot = element.pivotOffset;
            float l = Value(left, true, position.x - pivot.x * element.width);
            float r = Value(right, true, position.x + (1 - pivot.x) * element.width);
            float b = Value(bottom, false, position.y - pivot.y * element.height);
            float t = Value(top, false, position.y + (1 - pivot.y) * element.height);
            element.SetDimensions(Mathf.Max(1, Mathf.RoundToInt(r - l)), Mathf.Max(1, Mathf.RoundToInt(t - b)));
            transform.localPosition = new Vector3(Mathf.Lerp(l, r, pivot.x), Mathf.Lerp(b, t, pivot.y), position.z);
        }
        float Value(Edge edge, bool horizontal, float fallback)
        {
            if (edge.target == null) return fallback;
            var bounds = Bounds(edge.target, transform.parent, edge.target.GetComponent<Camera>());
            return Mathf.LerpUnclamped(horizontal ? bounds.xMin : bounds.yMin, horizontal ? bounds.xMax : bounds.yMax, edge.relative) + edge.absolute;
        }
        static Rect Bounds(Transform target, Transform parent, Camera camera)
        {
            Vector3 a, b;
            if (target != null && target.TryGetComponent<GameUIElement>(out var element))
            {
                var pivot = element.pivotOffset;
                a = target.TransformPoint(new Vector3(-pivot.x * element.width, -pivot.y * element.height));
                b = target.TransformPoint(new Vector3((1 - pivot.x) * element.width, (1 - pivot.y) * element.height));
            }
            else if (target != null && target.TryGetComponent<GameUIPanel>(out var panel))
            {
                var c = panel.clipRegion; a = target.TransformPoint(new Vector3(c.x - c.z / 2, c.y - c.w / 2)); b = target.TransformPoint(new Vector3(c.x + c.z / 2, c.y + c.w / 2));
            }
            else if (camera != null)
            {
                float depth = Vector3.Dot((parent != null ? parent.position : Vector3.zero) - camera.transform.position, camera.transform.forward);
                a = camera.ViewportToWorldPoint(new Vector3(0, 0, depth)); b = camera.ViewportToWorldPoint(new Vector3(1, 1, depth));
            }
            else { a = b = target != null ? target.position : Vector3.zero; }
            if (parent != null) { a = parent.InverseTransformPoint(a); b = parent.InverseTransformPoint(b); }
            return Rect.MinMaxRect(a.x, a.y, b.x, b.y);
        }
    }
}
