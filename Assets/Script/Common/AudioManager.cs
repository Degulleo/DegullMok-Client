using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class AudioManager : Singleton<AudioManager>
{
    private AudioClip mainBgm;
    private AudioClip gameBgm;

    [HideInInspector] public AudioSource bgmAudioSource;  // BGM을 위한 AudioSource
    private AudioSource sfxAudioSource;  // SFX를 위한 AudioSource

    public float sfxVolume = 1.0f;  // SFX 볼륨 (기본값 1)

    [HideInInspector]public bool isPlayBGM;
    [HideInInspector]public bool isPlaySFX;

    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    
    protected override void Awake()
    {
        base.Awake();  

        // BGM과 SFX를 위한 별도의 AudioSource 생성
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        
        //Sounds폴더 내의 모든 오디오클립 로드
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");
        
        foreach (AudioClip clip in clips)
        {
            audioClips[clip.name] = clip;
        }
        
        Debug.Log($"총 {audioClips.Count}개의 오디오클립이 로드됨.");
        
    }
    
    public AudioClip GetAudioClip(string clipName)
    {
        if (audioClips.TryGetValue(clipName, out AudioClip clip))
        {
            return clip;
        }
        else
        {
            Debug.LogError($"패널 '{clipName}'을 찾을 수 없습니다.");
        }
        return null;
    }

    // 시작 시 BGM을 자동으로 재생
    private void Start()
    {
        isPlayBGM = UserManager.IsPlayBGM;
        isPlaySFX = UserManager.IsPlaySFX;
        
        PlayBGM();
    }

    // 메인 BGM을 재생하는 함수
    public void PlayMainBGM()
    {
        mainBgm = GetAudioClip("MainBGM");
        
        if (bgmAudioSource != null && mainBgm != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.clip = mainBgm;
            bgmAudioSource.loop = true;  
            bgmAudioSource.volume = 0.1f;  
            bgmAudioSource.Play();  
        }
    }
    
    public void PlayGameBGM()
    {
        gameBgm = GetAudioClip("GameBGM");
        
        if (bgmAudioSource != null && gameBgm != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.clip = gameBgm;
            bgmAudioSource.loop = true;  
            bgmAudioSource.volume = 0.1f; 
            bgmAudioSource.Play();  
        }
    }

    public void PlayBGM()
    {
        if (isPlayBGM)
        {
            Scene currentScene = SceneManager.GetActiveScene();
        
            if (currentScene.name == "Main")
            {
                StopBGM();
                PlayMainBGM();
            }
            else if (currentScene.name == "Game")
            {
                StopBGM();
                PlayGameBGM();
            }
        }
    }
    
    public void StopBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();  
        }
    }
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGM();
    }

    public void PlayClickSound()
    {
        if (isPlaySFX && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("ClickSound"), sfxVolume);
        }
    }

    public void PlayCloseSound()
    {
        if (isPlaySFX && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("CloseSound"), sfxVolume);
        }
    }

    public void PlayCoinsAddSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("CoinsAddSound"), sfxVolume);
        }
    }

    public void PlayCoinsEmptySound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("CoinsEmptySound"), sfxVolume);
        }
    }

    public void PlayCoinsRemoveSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("CoinsRemoveSound"), sfxVolume);
        }
    }

    public void PlayLoseSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("LoseSound"), sfxVolume);
        }
    }

    public void PlayWinSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("WinSound"), sfxVolume);
        }
    }
    
    public void PlayDrawSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("DrawSound"), sfxVolume);
        }
    }

    public void PlayStoneSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            int randomIndex = UnityEngine.Random.Range(1, 8);
            sfxAudioSource.PlayOneShot(GetAudioClip("StoneSound"+randomIndex), sfxVolume);
        }
    }
    
    public void PlayRatingSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("RatingSound"), sfxVolume);
        }
    }
    
    public void PlayRatingUpSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("RatingUpSound"), sfxVolume);
        }
    }
    
    public void PlayRatingDownSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("RatingDownSound"), sfxVolume);
        }
    }
    
    public void PlayErrorSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("ErrorSound"), sfxVolume);
        }
    }
}