using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Música de fondo")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.3f;

    [Header("Efectos de sonido")]
    public AudioClip cardDealSound;
    public AudioClip buttonClickSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip chipPlaceSound;
    public AudioClip dealerRevealSound;
    public AudioClip blackjackSound;
    public AudioClip bustSound;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    void SetupAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        PlayBackgroundMusic();
    }


    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null) return;
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void PlayCardDeal()
    {
        PlaySFX(cardDealSound);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlayWin()
    {
        PlaySFX(winSound);
    }

    public void PlayLose()
    {
        PlaySFX(loseSound);
    }

    public void PlayChipPlace()
    {
        PlaySFX(chipPlaceSound);
    }

    public void PlayDealerReveal()
    {
        PlaySFX(dealerRevealSound);
    }

    public void PlayBlackjack()
    {
        PlaySFX(blackjackSound);
    }

    public void PlayBust()
    {
        PlaySFX(bustSound);
    }


    void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
