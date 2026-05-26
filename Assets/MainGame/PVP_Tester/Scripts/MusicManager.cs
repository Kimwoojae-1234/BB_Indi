using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    private const float Master_Volule = 0.8f;

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

    private bool inGameBGPlaying = false;
    private bool inPotionPlaying = false;

    private void Awake()
    {
        Instance_ = this;
        DontDestroyOnLoad(gameObject);
    }
    void OnDestroy()
    {
        Instance_ = null;
    }

    private void Start()
    {
        LoadMusic();
        SetVolume(true);
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
