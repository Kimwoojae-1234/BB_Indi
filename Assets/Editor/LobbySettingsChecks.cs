#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Runs on disposable preview objects without saving assets or changing Play Mode.
public static class LobbySettingsChecks
{
    [MenuItem("Tools/UI/Check Lobby Settings Button")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before checking the lobby settings fixture.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/Window/UI_LobbyRe.prefab");
        CheckButton(prefab.GetComponent<UI_LobbyRe>());
        var lobbyScene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/MainLobby.unity");
        try
        {
            var lobby = lobbyScene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<UI_LobbyRe>(true)).Single();
            CheckButton(lobby);
        }
        finally { EditorSceneManager.ClosePreviewScene(lobbyScene); }

        var preview = EditorSceneManager.NewPreviewScene();
        var managerField = typeof(KOBManager).GetField("popManager", BindingFlags.Static | BindingFlags.NonPublic);
        var previousManager = managerField.GetValue(null);
        GameObject fixture = null;
        try
        {
            fixture = new GameObject("Settings check", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(fixture, preview);
            fixture.SetActive(false);
            var lobby = UnityEngine.Object.Instantiate(prefab, fixture.transform).GetComponent<UI_LobbyRe>();
            lobby.gameObject.SetActive(false);
            var popupPrefab = Resources.Load<GameObject>("UI/Popup/Popup_Setting");
            Require(popupPrefab != null && popupPrefab.GetComponent<Popup_Setting>() != null, "Settings popup resource is missing.");
            var popup = UnityEngine.Object.Instantiate(popupPrefab, fixture.transform).GetComponent<Popup_Setting>();
            popup.gameObject.SetActive(false);
            var manager = fixture.AddComponent<PopupManager>();
            manager.RegistPopup<Popup_Setting>(popup.gameObject);
            managerField.SetValue(null, manager);
            fixture.SetActive(true);

            var button = CheckButton(lobby);
            // Allow the serialized callback only on this disposable Edit Mode instance.
            button.onClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
            button.onClick.Invoke();
            Require(popup.gameObject.activeInHierarchy && manager.GetPopup<Popup_Setting>() == popup,
                "The serialized settings click did not open Popup_Setting.");
            Require(popup.transform.localScale == Vector3.one && popup.GetComponent<RectTransform>().anchoredPosition3D == Vector3.zero,
                "The settings popup was not positioned by PopupManager.");
        }
        finally
        {
            managerField.SetValue(null, previousManager);
            if (fixture != null) UnityEngine.Object.DestroyImmediate(fixture);
            EditorSceneManager.ClosePreviewScene(preview);
        }
        Directory.CreateDirectory("Library/UIRegression");
        File.WriteAllText("Library/UIRegression/lobby-settings.txt", "PASS: prefab and MainLobby event targets; serialized button click opens and positions Popup_Setting.\n" + DateTime.UtcNow.ToString("O"));
        Debug.Log("[UI] Lobby settings button checks passed.");
    }

    private static Button CheckButton(UI_LobbyRe lobby)
    {
        Require(lobby != null, "Lobby controller is missing.");
        var button = lobby.GetComponentsInChildren<Button>(true).Single(b => b.name == "Button_Settings");
        Require(button.interactable && button.onClick.GetPersistentEventCount() == 1, "Settings button is disabled or has unexpected callbacks.");
        Require(button.onClick.GetPersistentTarget(0) == lobby && button.onClick.GetPersistentMethodName(0) == nameof(UI_LobbyRe.OnClickSetting),
            "Settings button has a missing or stale callback target.");
        Require(button.onClick.GetPersistentListenerState(0) != UnityEventCallState.Off, "Settings callback is disabled.");
        return button;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
#endif
