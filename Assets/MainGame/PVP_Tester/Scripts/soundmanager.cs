using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundmanager : MonoBehaviour {

    const int MAX_PLAY = 2;
    private const float Master_Volule = 1;//0.35f;

    public enum SoundID
    {
        BallCall = 0,
        StrikeCall = 1,
        StrikeOutCall =2,
        OutCall = 3,
        HomerunCall = 4,
        SafeCall = 5,
        SoundCatch = 6,
        HitBest= 7,
        HitGood = 8,
        HitWeak = 9,
        HitTip = 10,
        Release = 11,
        InningStart = 12,
        ScoreSound = 13,
        None = 1000
    }

    public AudioSource source;
    public AudioClip[] clip;   

    private int [] soundPlayNum;

    private static soundmanager Instance_;

    private bool SoundOn;

    private void Awake()
    {
        Instance_ = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadSound();
        SetVolume(true);

    }


    void OnDestroy()
    {
        Instance_ = null;
    }


    /// <summary>
    /// 인스턴트 리턴
    /// </summary>
    /// <returns></returns>
    public static soundmanager Get()
    {
        return Instance_;
    }

    /// <summary>
    /// 메인 DB를 로드
    /// </summary>
    public void LoadSound()
    {
        //Debug.Log("Init Sound");
        int count = clip.Length;
        soundPlayNum = new int[count];
        source.volume = Master_Volule;
        source.loop = false;
    }



    public void SetVolume(bool bSoundOn)
    {
        //Master_Volule = bSoundOn ? 0.35f : 0;
        source.volume = Master_Volule;
        SoundOn = bSoundOn;
        if (SoundOn == false)
        {            
            SoundStop();
        }
    }


    public void PlaySound(SoundID id)
    {
        if (SoundOn == false) return;
        int index = (int)id;         
        if (soundPlayNum[index] < MAX_PLAY)
        {
            StartCoroutine(playSound(index));
        }
    }

    

    public void SoundMute(bool bActive)
    {
        source.volume = bActive ? 0 : Master_Volule;
    }

    public void SoundStop()
    {
        source.Stop();
    }


    
    private IEnumerator playSound(int index)
    {
        soundPlayNum[index]++;
        AudioClip curClip = clip[index];
        source.PlayOneShot(curClip);
        yield return new WaitForSeconds(curClip.length);
        soundPlayNum[index]--;
        if (soundPlayNum[index] < 0) soundPlayNum[index] = 0;
    }
    

}
