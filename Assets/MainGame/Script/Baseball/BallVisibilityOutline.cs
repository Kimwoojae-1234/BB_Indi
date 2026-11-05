using UnityEngine;

namespace BaseBall.BallPlay
{
    // Reuses the batting-view ballline art for pitched and field-view balls.
    [DefaultExecutionOrder(1000)]
    public sealed class BallVisibilityOutline : MonoBehaviour
    {
        private Renderer ballRenderer;
        private Renderer outlineRenderer;
        private float meshDiameter;

        public static void Attach(GameObject ball)
        {
            if (ball == null || ball.transform.Find("VisibilityBallLine") != null) return;
            var source = Resources.Load<GameObject>("MainGame/prefabs/Pitch/PitchSystem");
            var batting = source != null ? source.GetComponentInChildren<BattingSystem>(true) : null;
            if (batting == null || batting.ballLine == null) return;
            var marker = Instantiate(batting.ballLine, ball.transform, false);
            marker.name = "VisibilityBallLine";
            foreach (var tween in marker.GetComponentsInChildren<UITweener>(true)) tween.enabled = false;
            foreach (var child in marker.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = ball.layer;
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localRotation = Quaternion.identity;
            marker.SetActive(true);
            var follow = marker.AddComponent<BallVisibilityOutline>();
            follow.ballRenderer = ball.GetComponent<Renderer>();
            follow.outlineRenderer = marker.GetComponent<Renderer>();
            var mesh = marker.GetComponent<MeshFilter>();
            if (mesh != null && mesh.sharedMesh != null)
                follow.meshDiameter = Mathf.Max(mesh.sharedMesh.bounds.size.x, mesh.sharedMesh.bounds.size.y);
            if (follow.outlineRenderer != null)
            {
                follow.outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                follow.outlineRenderer.receiveShadows = false;
                if (follow.ballRenderer != null)
                {
                    follow.outlineRenderer.sortingLayerID = follow.ballRenderer.sortingLayerID;
                    follow.outlineRenderer.sortingOrder = follow.ballRenderer.sortingOrder + 1;
                }
            }
            follow.LateUpdate();
        }

        private void LateUpdate()
        {
            if (outlineRenderer == null) return;
            bool visible = ballRenderer != null && ballRenderer.enabled && ballRenderer.gameObject.activeInHierarchy;
            outlineRenderer.enabled = visible;
            if (!visible || meshDiameter <= 0) return;
            // Fit around the ball rather than inheriting the original batting-view scale.
            Vector3 size = ballRenderer.bounds.size;
            float diameter = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) * 1.3f;
            Vector3 parentScale = transform.parent.lossyScale;
            float scale = diameter / meshDiameter;
            transform.localScale = new Vector3(scale / Mathf.Max(Mathf.Abs(parentScale.x), 0.0001f),
                scale / Mathf.Max(Mathf.Abs(parentScale.y), 0.0001f), 1);
            transform.position = ballRenderer.bounds.center;
        }

        private void OnWillRenderObject()
        {
            // The field ball rotates in 3D. Keep the flat ring facing each rendering camera.
            var camera = Camera.current;
            if (camera != null) transform.rotation = camera.transform.rotation;
        }
    }
}
