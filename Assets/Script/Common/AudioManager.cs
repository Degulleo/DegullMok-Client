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
        base.Awake();  // 부모 클래스의 Awake 호출

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
        mainBgm = GetAudioClip("main bgm");
        
        if (bgmAudioSource != null && mainBgm != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.clip = mainBgm;
            bgmAudioSource.loop = true;  // BGM을 반복 재생
            bgmAudioSource.volume = 0.1f;  // BGM 볼륨 설정
            bgmAudioSource.Play();  // BGM 재생
        }
    }
    
    public void PlayGameBGM()
    {
        gameBgm = GetAudioClip("Game bgm2");
        
        if (bgmAudioSource != null && gameBgm != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.clip = gameBgm;
            bgmAudioSource.loop = true;  // BGM을 반복 재생
            bgmAudioSource.volume = 0.1f;  // BGM 볼륨 설정
            bgmAudioSource.Play();  // 게임 BGM 재생
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
            bgmAudioSource.Stop();  // 게임용 BGM을 멈춤
        }
    }
    
    // 씬이 로드될 때마다 호출되는 OnSceneLoaded 메서드
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGM();
    }

    // 클릭 사운드(SFX) 재생
    public void PlayClickSound()
    {
        if (isPlaySFX && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("Click Sound"), sfxVolume);
        }
    }

    // 닫기 사운드(SFX) 재생
    public void PlayCloseSound()
    {
        if (isPlaySFX && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("Close Sound"), sfxVolume);
        }
    }

    public void PlayCoinsAddSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("Coins ADD Sound"), sfxVolume);
        }
    }

    public void PlayCoinsEmptySound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("Coins Empty Sound"), sfxVolume);
        }
    }

    public void PlayCoinsRemoveSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("Coins Remove Sound"), sfxVolume);
        }
    }

    public void PlayLoseSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("lose sound"), sfxVolume);
        }
    }

    public void PlayWinSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("win sound"), sfxVolume);
        }
    }

    public void PlayStoneSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(GetAudioClip("stone sound3"), sfxVolume);
        }
    }
}