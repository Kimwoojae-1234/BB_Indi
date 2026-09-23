using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

    private const float Master_Volule = 0.8f;
    private const string LobbySceneName = "MainLobby";
    private const string LobbyMusicResourcePath = "Sound/BGM_Bright and cheerful_1";

    public enum MusicID
    {
        Idle = 0,
        crowd = 1,
        None = 1000
    }
    public AudioSource source;
    public AudioClip[] clip;


    private static MusicManager Instance_;

    private bool MusicOn;
    private AudioClip lobbyMusic;

    private bool inGameBGPlaying = false;
    private bool inPotionPlaying = false;

    private void Awake()
    {
        if (Instance_ != null && Instance_ != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance_ = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance_ == this) Instance_ = null;
    }

    private void Start()
    {
        LoadMusic();
        SetVolume(true);
        UpdateSceneMusic(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneMusic(SceneManager.GetActiveScene());
    }

    private void UpdateSceneMusic(Scene activeScene)
    {
        if (activeScene.name == LobbySceneName)
        {
            PlayLobbyMusic();
        }
        else if (source != null && source.clip == lobbyMusic)
        {
            StopMusic();
            source.clip = null;
        }
    }

    private void PlayLobbyMusic()
    {
        if (source == null) return;

        if (lobbyMusic == null)
        {
            lobbyMusic = Resources.Load<AudioClip>(LobbyMusicResourcePath);
            if (lobbyMusic == null)
            {
                Debug.LogError($"Lobby BGM을 불러오지 못했습니다: Resources/{LobbyMusicResourcePath}");
                return;
            }
        }

        if (source.clip == lobbyMusic && source.isPlaying) return;

        source.clip = lobbyMusic;
        source.loop = true;
        if (MusicOn) source.Play();
    }


    /// <summary>
    /// 인스턴트 리턴
    /// </summary>
    /// <returns></returns>
    public static MusicManager Get()
    {
        return Instance_;
    }

    public void LoadMusic()
    {
        //Debug.Log("Init Music");
        inGameBGPlaying = false;
        inPotionPlaying = false;
        int count = clip.Length;
        //bSpecialSound = false;
        source.volume = Master_Volule;
    }

    public void SetVolume(bool bMusicOn)
    {
        //Master_Volule = bMusicOn ? 0.8f : 0;
        source.volume = Master_Volule;
        MusicOn = bMusicOn;
        if (MusicOn == true)
        {
            if(source.clip != null)
            {
                source.Play();
            }
        }
        else
        { 
            StopMusic();
        }
    }

    public void PlayMusic(MusicID id)
    {
        source.clip = clip[(int)id];
        source.loop = true;
        if (MusicOn == true)
        {
            source.Play();
        }
    }

    public void StopMusic()
    {
        source.Stop();
    }

    /*
    public void SetSound(bool bActive)
    {
        source.volume = bActive? Master_Volule : 0;
    }*/

    public bool CheckIngamePlaying()
    {
        return inGameBGPlaying;
    }

    public bool CheckPotionPlaying()
    {
        return inPotionPlaying;
    }
    
}
