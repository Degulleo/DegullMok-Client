using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioClip mainBgm;
    private AudioSource audioSource;

    private void Start()
    {
        PlayMainBGM();
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
            audioSource.volume = 0.2f; // 볼륨
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
}
