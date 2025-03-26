using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class AudioManager : Singleton<AudioManager>
{
    [Header("BGM")]
    [SerializeField] private AudioClip mainBgm;
    [SerializeField] private AudioClip gameBgm;
    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip coinsAddSound;
    [SerializeField] private AudioClip coinsEmptySound;
    [SerializeField] private AudioClip coinsRemoveSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip stoneSound;

    [HideInInspector] public AudioSource bgmAudioSource;  // BGM을 위한 AudioSource
    private AudioSource sfxAudioSource;  // SFX를 위한 AudioSource

    public float sfxVolume = 1.0f;  // SFX 볼륨 (기본값 1)

    [HideInInspector]public bool isPlayBGM;
    [HideInInspector]public bool isPlaySFX;

    private void Awake()
    {
        base.Awake();  // 부모 클래스의 Awake 호출

        // BGM과 SFX를 위한 별도의 AudioSource 생성
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
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
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(clickSound, sfxVolume);
        }
    }

    // 닫기 사운드(SFX) 재생
    public void PlayCloseSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(closeSound, sfxVolume);
        }
    }

    public void PlayCoinsAddSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(coinsAddSound, sfxVolume);
        }
    }

    public void PlayCoinsEmptySound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(coinsEmptySound, sfxVolume);
        }
    }

    public void PlayCoinsRemoveSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(coinsRemoveSound, sfxVolume);
        }
    }

    public void PlayLoseSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(loseSound, sfxVolume);
        }
    }

    public void PlayWinSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(winSound, sfxVolume);
        }
    }

    public void PlayStoneSound()
    {
        if (isPlaySFX && sfxAudioSource!=null)
        {
            sfxAudioSource.PlayOneShot(stoneSound, sfxVolume);
        }
    }
}