using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private AudioClip mainBgm;
    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip closeSound;

    private AudioSource audioSource;

    [HideInInspector] public float sfxVolume;

    private void Start()
    {
        PlayMainBGM();
        sfxVolume = 1.0f;   //테스트 코드
    }
    
    // 배경음악 시작
    public void PlayMainBGM()
    {
        // AudioSource 컴포넌트 가져오기
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null && mainBgm != null)
        {
            // 배경음악이 설정되면 재생
            audioSource.clip = mainBgm; // 음악 클립 설정
            audioSource.loop = true; // 반복 재생
            audioSource.volume = 0.1f; // 볼륨
            audioSource.Play(); // 음악 시작
        }
    }

    // 배경음악 멈추기
    public void StopMainBGM()
    {
        if (audioSource != null)
        {
            audioSource.Stop(); // 배경음악 멈추기
        }
    }

    public void PlayClickSound()
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, sfxVolume);
        }
    }

    public void PlayCloseSound()
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(closeSound, sfxVolume);
        }
    }
}
