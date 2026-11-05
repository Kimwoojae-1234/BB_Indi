using System;
using System.Collections;
using UnityEngine;

// Two independent music channels allow a real overlap when the rank-change cue ends.
public class FrontAudio : MonoBehaviour
{
    private static FrontAudio instance;
    private FrontAudioLibrary clips;
    private readonly AudioSource[] music = new AudioSource[2];
    private readonly float[] gain = new float[2];
    private AudioSource effects;
    private int active;
    private Coroutine sequence;
    private string track;
    private static FrontAudio Get()
    {
        if (instance != null) return instance;
        var go = new GameObject("FrontAudio");
        instance = go.AddComponent<FrontAudio>();
        DontDestroyOnLoad(go);
        return instance;
    }

    private void Awake()
    {
        clips = Resources.Load<FrontAudioLibrary>("Sound/FrontAudioLibrary");
        for (int i = 0; i < 2; i++) music[i] = NewSource();
        effects = NewSource();
    }

    private AudioSource NewSource()
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0;
        source.volume = 0;
        return source;
    }

    private void Update()
    {
        for (int i = 0; i < 2; i++) music[i].volume = gain[i] * ClientSettings.MusicVolume * .8f;
        effects.volume = ClientSettings.SoundVolume;
    }

    private void OnDestroy() { if (instance == this) instance = null; }

    public static void WindowOpened(Type type)
    {
        var audio = Get();
        if (audio.clips == null) return;
        if (type == typeof(UI_LobbyRe))
            audio.Play("Lobby", UnityEngine.Random.value < .5f ? audio.clips.lobby00 : audio.clips.lobby01);
        else if (type == typeof(UI_RTTS)) audio.Play("League", audio.clips.league);
        else if (type == typeof(UI_TrophyRoad) || type == typeof(UI_Pass)) audio.Play("PassTrophy", audio.clips.passTrophy);
        else if (type == typeof(UI_Shop)) audio.Play("Shop", audio.clips.shop);
        else if (audio.track == "Simulation" || audio.track == "RankChange") audio.Play("League", audio.clips.league);
    }

    public static void PlayRankChange()
    {
        var audio = Get();
        if (audio.clips?.rankChange == null) return;
        audio.CancelSequence();
        audio.track = "RankChange";
        MusicManager.Get()?.StopMusic();
        audio.sequence = audio.StartCoroutine(audio.RankSequence());
    }

    private IEnumerator RankSequence()
    {
        const float introFade = .3f, blend = .85f;
        yield return Blend(clips.rankChange, false, introFade);
        // Start the league loop before the cue ends; crossfade instead of stopping then starting.
        while (music[active].isPlaying && music[active].time < Mathf.Max(0, clips.rankChange.length - blend))
            yield return null;
        track = "League";
        yield return Blend(clips.league, true, blend);
        sequence = null;
    }

    public static void PlaySimulation()
    {
        var audio = Get();
        if (audio.clips != null) audio.Play("Simulation", audio.clips.simulation);
    }

    public static bool SimulationPlaying => instance != null && instance.track == "Simulation";
    public static void StopSimulation()
    {
        if (!SimulationPlaying) return;
        instance.CancelSequence();
        instance.track = null;
        for (int i = 0; i < 2; i++) { instance.music[i].Stop(); instance.gain[i] = 0; }
    }

    public static void Click(bool back)
    {
        var audio = Get();
        var clip = back ? audio.clips?.back : audio.clips?.confirm;
        if (clip == null || ClientSettings.SoundVolume <= 0) return;
        audio.effects.volume = ClientSettings.SoundVolume;
        audio.effects.PlayOneShot(clip);
    }

    private void Play(string key, AudioClip clip)
    {
        if (clip == null || (track == key && music[active].isPlaying)) return;
        CancelSequence();
        track = key;
        MusicManager.Get()?.StopMusic();
        sequence = StartCoroutine(Blend(clip, true, .5f));
    }

    private void CancelSequence()
    {
        if (sequence != null) StopCoroutine(sequence);
        sequence = null;
    }

    private IEnumerator Blend(AudioClip clip, bool loop, float duration)
    {
        if (clip == null) yield break;
        int previous = active;
        active = 1 - active;
        int incoming = active;
        float from = gain[previous];
        music[incoming].Stop();
        gain[incoming] = 0;
        music[incoming].volume = 0;
        music[incoming].clip = clip;
        music[incoming].loop = loop;
        music[incoming].Play();
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            gain[previous] = from * (1 - t);
            gain[incoming] = t;
            yield return null;
        }
        gain[incoming] = 1;
        gain[previous] = 0;
        music[previous].Stop();
    }
}
