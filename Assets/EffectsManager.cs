using UnityEngine;

/// <summary>
/// Módulo 5 — Effects Manager
/// Maneja partículas y feedback visual del juego.
/// Uso: EffectsManager.Instance.PlayWinEffect(transform.position);
/// </summary>
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

    /// <summary>Destello dorado en la posición indicada (ej: centro de la mesa).</summary>
    public void PlayWinEffect(Vector3 position)
    {
        SpawnEffect(winParticlePrefab, position);
    }

    /// <summary>Efecto rojo tenue al perder.</summary>
    public void PlayLoseEffect(Vector3 position)
    {
        SpawnEffect(loseParticlePrefab, position);
    }

    /// <summary>Efecto especial de Blackjack.</summary>
    public void PlayBlackjackEffect(Vector3 position)
    {
        SpawnEffect(blackjackParticlePrefab, position);
    }

    /// <summary>Rastro de movimiento al repartir una carta. Úsalo en la posición de la carta.</summary>
    public void PlayCardDealEffect(Vector3 position)
    {
        SpawnEffect(cardDealParticlePrefab, position);
    }

    // ─── Interno ──────────────────────────────────────────────

    void SpawnEffect(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        GameObject fx = Instantiate(prefab, position, Quaternion.identity);

        // Auto-destruir después de que terminen las partículas
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
