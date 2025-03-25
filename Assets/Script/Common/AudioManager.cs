using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    [Header("BGM")]
    [SerializeField] private AudioClip mainBgm;
    [SerializeField] private AudioClip gameBgm;
    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip closeSound;

    private AudioSource bgmAudioSource;  // BGM을 위한 AudioSource
    private AudioSource sfxAudioSource;  // SFX를 위한 AudioSource

    [HideInInspector] public float sfxVolume = 1.0f;  // SFX 볼륨 (기본값 1)

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
        PlayMainBGM();  // 기본 BGM을 재생
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

    // BGM을 멈추는 함수
    public void StopMainBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();  // BGM을 멈춤
        }
    }

    // 클릭 사운드(SFX) 재생
    public void PlayClickSound()
    {
        sfxAudioSource.PlayOneShot(clickSound, sfxVolume);
    }

    // 닫기 사운드(SFX) 재생
    public void PlayCloseSound()
    {
        sfxAudioSource.PlayOneShot(closeSound, sfxVolume);
    }

    // 씬이 로드될 때마다 호출되는 OnSceneLoaded 메서드
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름에 따라 BGM을 다르게 설정
        Debug.Log($"Scene {scene.name} loaded.");

        if (scene.name == "MainMenu")
        {
            // MainMenu 씬이 로드되면 메인 메뉴 BGM 재생
            PlayMainBGM();
        }
        else if (scene.name == "GameScene")
        {
            // GameScene 씬이 로드되면 게임용 BGM 재생
            if (gameBgm != null)
            {
                bgmAudioSource.clip = gameBgm;
                bgmAudioSource.loop = true;
                bgmAudioSource.volume = 0.1f;  // BGM 볼륨 설정
                bgmAudioSource.Play();
            }
        }
    }
}