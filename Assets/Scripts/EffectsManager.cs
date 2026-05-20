using UnityEngine;


public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    [Header("Prefabs de partículas")]
    [Tooltip("Destello dorado cuando el jugador gana")]
    public GameObject winParticlePrefab;

    [Tooltip("Efecto rojo tenue cuando el jugador pierde")]
    public GameObject loseParticlePrefab;

    [Tooltip("Rastro de movimiento al repartir una carta")]
    public GameObject cardDealParticlePrefab;

    [Tooltip("Efecto especial para Blackjack natural")]
    public GameObject blackjackParticlePrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ─── Efectos de resultado ─────────────────────────────────

    public void PlayWinEffect(Vector3 position)
    {
        SpawnEffect(winParticlePrefab, position);
    }

    public void PlayLoseEffect(Vector3 position)
    {
        SpawnEffect(loseParticlePrefab, position);
    }
    public void PlayBlackjackEffect(Vector3 position)
    {
        SpawnEffect(blackjackParticlePrefab, position);
    }
    public void PlayCardDealEffect(Vector3 position)
    {
        SpawnEffect(cardDealParticlePrefab, position);
    }

    // ─── Interno ──────────────────────────────────────────────

    void SpawnEffect(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        GameObject fx = Instantiate(prefab, position, Quaternion.identity);

        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(fx, 3f);
        }
    }
}
