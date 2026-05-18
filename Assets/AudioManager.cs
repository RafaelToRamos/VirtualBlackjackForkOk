using UnityEngine;

/// <summary>
/// Módulo 5 — Audio Manager
/// Maneja todos los sonidos del juego. Es un Singleton accesible desde cualquier script.
/// Uso: AudioManager.Instance.PlayCardDeal();
/// </summary>
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
        // Singleton: solo existe una instancia y persiste entre escenas
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
        // Source para música (loop)
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        // Source para efectos (one-shot)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    // ─── Música ───────────────────────────────────────────────

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

    // ─── Efectos de sonido ────────────────────────────────────

    /// <summary>Llamar cuando se reparte una carta al jugador o crupier.</summary>
    public void PlayCardDeal()
    {
        PlaySFX(cardDealSound);
    }

    /// <summary>Llamar cuando el jugador presiona Hit, Stand, Double o cualquier botón.</summary>
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    /// <summary>Llamar cuando el jugador gana la ronda.</summary>
    public void PlayWin()
    {
        PlaySFX(winSound);
    }

    /// <summary>Llamar cuando el jugador pierde la ronda.</summary>
    public void PlayLose()
    {
        PlaySFX(loseSound);
    }

    /// <summary>Llamar cuando el jugador hace una apuesta (coloca fichas).</summary>
    public void PlayChipPlace()
    {
        PlaySFX(chipPlaceSound);
    }

    /// <summary>Llamar cuando el crupier revela su carta oculta.</summary>
    public void PlayDealerReveal()
    {
        PlaySFX(dealerRevealSound);
    }

    /// <summary>Llamar en caso de Blackjack natural (21 con 2 cartas).</summary>
    public void PlayBlackjack()
    {
        PlaySFX(blackjackSound);
    }

    /// <summary>Llamar cuando el jugador se pasa de 21.</summary>
    public void PlayBust()
    {
        PlaySFX(bustSound);
    }

    // ─── Interno ──────────────────────────────────────────────

    void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
