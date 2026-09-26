using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
namespace BaseBall.BallPlay.UGUI
{
    [DefaultExecutionOrder(-200)]
    public sealed class GameUIRoot : MonoBehaviour
    {
        public int manualWidth = 1280, manualHeight = 720;
        public bool fitWidth, fitHeight = true;
        public Camera uiCamera;
        public Canvas rootCanvas;
        public RectTransform hudLayer, effectsLayer, popupLayer;
        private void Awake()
        {
            EnsureInput(gameObject.scene);
            if (uiCamera != null) { uiCamera.eventMask = 0; uiCamera.transparencySortMode = TransparencySortMode.Orthographic; }
            BindCanvases();
            Resize();
        }
        public void BindCanvases()
        {
            foreach (var canvas in GetComponentsInChildren<Canvas>(true))
            {
                canvas.worldCamera = uiCamera;
                // This also covers existing UGUI card/minimap prefab instances. Preserve their
                // former root sorting, without breaking inheritance inside those prefabs.
                var parentCanvas = canvas.transform.parent != null ? canvas.transform.parent.GetComponentInParent<Canvas>(true) : null;
                if (rootCanvas != null && canvas != rootCanvas && parentCanvas == rootCanvas)
                    canvas.overrideSorting = true;
            }
        }
        public static void EnsureInput(Scene scene)
        {
            if (!Application.isPlaying) return;
            if (EventSystem.current == null)
            {
                var input = new GameObject("Ingame UGUI EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                SceneManager.MoveGameObjectToScene(input, scene);
                input.GetComponent<EventSystem>().sendNavigationEvents = false;
            }
        }
        private void Update() { Resize(); }
        private void Resize()
        {
            // CanvasScaler owns the screen-space layout. The prefab root only owns its lifetime.
            if (rootCanvas != null) return;
            float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            float height = fitWidth ? (fitHeight ? Mathf.Max(manualHeight, manualWidth / aspect) : manualWidth / aspect)
                : (fitHeight ? manualHeight : Mathf.Min(manualHeight, manualWidth / aspect));
            transform.localScale = Vector3.one * (2 / Mathf.Max(1, height));
        }
        public Transform EffectsParent => effectsLayer != null ? effectsLayer : transform;

        public static void RenderForCapture(Camera camera)
        {
            var owner = camera.GetComponentInParent<GameUIRoot>();
            var canvas = owner != null ? owner.rootCanvas : null;
            if (owner != null) owner.SynchronizePresentation();
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace) { camera.Render(); return; }

            // A secondary camera must see the same UI geometry as the Spine meshes it captures.
            // Screen-space canvases otherwise belong exclusively to their assigned UI camera.
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)canvas.transform;
            var position = rect.position; var rotation = rect.rotation;
            var scale = rect.localScale; var size = rect.sizeDelta;
            var mode = canvas.renderMode;
            try
            {
                canvas.renderMode = RenderMode.WorldSpace;
                rect.SetPositionAndRotation(position, rotation);
                rect.localScale = scale; rect.sizeDelta = size;
                Canvas.ForceUpdateCanvases();
                camera.Render();
            }
            finally
            {
                canvas.renderMode = mode;
                Canvas.ForceUpdateCanvases();
            }
        }
        public void SynchronizePresentation()
        {
            Canvas.ForceUpdateCanvases();
            // Alpha tweens and scroll changes also affect unbatched/clipped widgets.
            // Only active owners may expose detached presentation objects during capture.
            foreach (var element in GetComponentsInChildren<GameUIElement>())
                if (element.isActiveAndEnabled) element.Apply();
            foreach (var batch in GetComponentsInChildren<GameUICanvasBatch>()) batch.Apply();
            Canvas.ForceUpdateCanvases();
        }

        public static Camera FindCameraForLayer(int layer)
        {
            Camera selected = null;
            foreach (var camera in Camera.allCameras)
                if (camera.targetTexture == null && (camera.cullingMask & (1 << layer)) != 0 && (selected == null || camera.depth > selected.depth)) selected = camera;
            return selected;
        }
    }
}
