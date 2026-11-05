using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SettingsPreviewCapture
{
    [MenuItem("Tools/Clutch/Capture Settings Previews")]
    public static void Capture()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        var cameraObject = new GameObject("PreviewCamera", typeof(Camera));
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        var camera = cameraObject.GetComponent<Camera>();
        camera.scene = scene;
        camera.orthographic = true;
        camera.orthographicSize = 540;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.03f, .08f, .14f);
        var target = new RenderTexture(1280, 720, 24);
        camera.targetTexture = target;
        var canvasObject = new GameObject("PreviewCanvas", typeof(Canvas), typeof(CanvasScaler));
        SceneManager.MoveGameObjectToScene(canvasObject, scene);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        canvasObject.GetComponent<CanvasScaler>().scaleFactor = 1280f / 1920;
        string old = UILocalization.Language;
        bool saved = PlayerPrefs.HasKey("UI.Language");
        try
        {
            foreach (var prefabName in new[] { "Popup_Setting", "Popup_Setting_Language" })
            {
                var popup = Object.Instantiate(Resources.Load<GameObject>("UI/Popup/" + prefabName), canvas.transform);
                var rect = (RectTransform)popup.transform;
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                popup.GetComponent<UIPopup>().Open();
                foreach (string language in new[] { "en", "ko", "ja" })
                {
                    UILocalization.SetLanguage(language);
                    foreach (var text in popup.GetComponentsInChildren<LocalizedUIText>(true)) text.Refresh();
                    popup.GetComponent<UIPopup>().Open();
                    Canvas.ForceUpdateCanvases();
                    foreach (var text in popup.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
                    camera.Render();
                    var previous = RenderTexture.active;
                    RenderTexture.active = target;
                    var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                    image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                    Directory.CreateDirectory("Logs/SettingsPreview");
                    File.WriteAllBytes("Logs/SettingsPreview/" + prefabName + "_" + language + ".png", image.EncodeToPNG());
                    Object.DestroyImmediate(image);
                    RenderTexture.active = previous;
                }
                Object.DestroyImmediate(popup);
            }
        }
        finally
        {
            UILocalization.SetLanguage(old);
            if (!saved) { PlayerPrefs.DeleteKey("UI.Language"); PlayerPrefs.Save(); }
            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
