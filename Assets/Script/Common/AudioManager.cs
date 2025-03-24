using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private AudioClip mainBgm;
    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip closeSound;

    private AudioSource bgmAudioSource;  // AudioSource for BGM
    private AudioSource sfxAudioSource;  // AudioSource for SFX

    [HideInInspector] public float sfxVolume = 1.0f;  // SFX volume, default to 1

    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                
                if (instance == null)
                {
                    GameObject audioManagerObj = new GameObject("AudioManager");
                    instance = audioManagerObj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Ensure AudioManager persists across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);  // Avoid multiple instances
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);  // This makes AudioManager persist through scene changes

        // Create separate AudioSource components for BGM and SFX
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
    }
    
    
    // Start is called before the first frame update
    private void Start()
    {
        // Optional: Automatically play BGM at the start
        PlayMainBGM();
    }

    // Play the main BGM if it is not already playing
    public void PlayMainBGM()
    {
        if (bgmAudioSource != null && mainBgm != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.clip = mainBgm;
            bgmAudioSource.loop = true;  // Loop the BGM
            bgmAudioSource.volume = 0.1f;  // Set volume for BGM
            bgmAudioSource.Play();  // Play the BGM
        }
    }

    // Stop the BGM if it's currently playing
    public void StopMainBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();  // Stop the BGM if it's playing
        }
    }

    // Play Click Sound (SFX)
    public void PlayClickSound()
    {
        sfxAudioSource.PlayOneShot(clickSound, sfxVolume);
    }

    // Play Close Sound (SFX)
    public void PlayCloseSound()
    {
        sfxAudioSource.PlayOneShot(closeSound, sfxVolume);
    }
}