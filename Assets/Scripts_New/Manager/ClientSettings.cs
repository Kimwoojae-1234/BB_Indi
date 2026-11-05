using UnityEngine;

public static class ClientSettings
{
    public static float SoundVolume => Mathf.Clamp01(PlayerPrefs.GetFloat("Settings.Sound", 1));
    public static float MusicVolume => Mathf.Clamp01(PlayerPrefs.GetFloat("Settings.Music", 1));
    public static bool Vibration => PlayerPrefs.GetInt("Settings.Vibration", 1) != 0;
    public static void SetSound(float value)
    {
        PlayerPrefs.SetFloat("Settings.Sound", Mathf.Clamp01(value)); PlayerPrefs.Save();
        soundmanager.Get()?.ApplySettingsVolume();
    }
    public static void SetMusic(float value)
    {
        PlayerPrefs.SetFloat("Settings.Music", Mathf.Clamp01(value)); PlayerPrefs.Save();
        MusicManager.Get()?.ApplySettingsVolume();
    }
    public static void SetVibration(bool value)
    {
        PlayerPrefs.SetInt("Settings.Vibration", value ? 1 : 0); PlayerPrefs.Save();
        if (!value) ToolShed.Android.OS.Vibrator.Cancel();
        else ToolShed.Android.OS.Vibrator.Vibrate(80);
    }
}
