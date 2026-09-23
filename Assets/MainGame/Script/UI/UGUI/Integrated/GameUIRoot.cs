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
        private void Awake()
        {
            EnsureInput(gameObject.scene);
            if (uiCamera != null) { uiCamera.eventMask = 0; uiCamera.transparencySortMode = TransparencySortMode.Orthographic; }
            foreach (var canvas in GetComponentsInChildren<Canvas>(true))
                if (canvas.renderMode == RenderMode.WorldSpace) canvas.worldCamera = uiCamera;
            Resize();
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
            float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            float height = fitWidth ? (fitHeight ? Mathf.Max(manualHeight, manualWidth / aspect) : manualWidth / aspect)
                : (fitHeight ? manualHeight : Mathf.Min(manualHeight, manualWidth / aspect));
            transform.localScale = Vector3.one * (2 / Mathf.Max(1, height));
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
