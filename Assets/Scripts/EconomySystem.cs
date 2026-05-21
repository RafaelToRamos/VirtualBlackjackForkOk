using UnityEngine;

// ============================================================
//  EconomySystem.cs  —  NUEVO (extraído de BlackjackGameManager)
//
//  ANTES: playerChips, currentBet y toda la lógica de pagos
//         vivían dentro de BlackjackGameManager.
//
//  AHORA: Este componente es el ÚNICO responsable de:
//           • Mantener el balance del jugador
//           • Validar y registrar apuestas
//           • Calcular y aplicar pagos según el resultado
//
//  CÓMO USARLO:
//    Agregar este componente al MISMO GameObject que
//    BlackjackGameManager. El GameManager lo obtiene con
//    GetComponent<EconomySystem>() en Awake().
// ============================================================

public class EconomySystem : MonoBehaviour
{
    // ── Configuración (Inspector) ────────────────────────────
    [Header("Configuración")]
    public int startingChips = 500;
    public int minBet        = 10;
    public int maxBet        = 500;

    // ── Estado interno ───────────────────────────────────────
    private int _chips;
    private int _currentBet;

    // ── Propiedades públicas de solo lectura ─────────────────
    public int Chips      => _chips;
    public int CurrentBet => _currentBet;
    public bool HasBet    => _currentBet > 0;

    // ── Eventos ──────────────────────────────────────────────
    /// <summary>El balance de fichas cambió.</summary>
    public event System.Action<int> OnChipsChanged;

    /// <summary>Una apuesta fue colocada.</summary>
    public event System.Action<int> OnBetPlaced;

    /// <summary>La ronda se liquidó. int = ganancia/pérdida neta.</summary>
    public event System.Action<int> OnRoundSettled;

    // ── Ciclo de vida ────────────────────────────────────────
    void Awake()
    {
        _chips = startingChips;
    }

    // ════════════════════════════════════════════════════════
    //  API PÚBLICA
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Intenta colocar una apuesta. Retorna false si el monto
    /// es inválido o el jugador no tiene fichas suficientes.
    /// </summary>
    public bool TryPlaceBet(int amount)
    {
        if (amount < minBet || amount > maxBet)
        {
            Debug.LogWarning($"[Economy] Apuesta inválida: {amount}. Rango: {minBet}-{maxBet}");
            return false;
        }
        if (amount > _chips)
        {
            Debug.LogWarning($"[Economy] Sin fichas suficientes. Disponible: {_chips}, solicitado: {amount}");
            return false;
        }

        _currentBet = amount;
        _chips -= amount; // Fichas en "escrow" durante la ronda
        OnChipsChanged?.Invoke(_chips);
        OnBetPlaced?.Invoke(_currentBet);
        return true;
    }

    /// <summary>
    /// Intenta duplicar la apuesta (Double Down).
    /// Retorna false si el jugador no puede pagar.
    /// </summary>
    public bool TryDoubleDown()
    {
        if (_currentBet <= 0 || _chips < _currentBet)
        {
            Debug.LogWarning("[Economy] No se puede doblar: fichas insuficientes.");
            return false;
        }

        _chips -= _currentBet;
        _currentBet *= 2;
        OnChipsChanged?.Invoke(_chips);
        return true;
    }

    /// <summary>
    /// Liquida la ronda y actualiza el balance.
    /// Llamado por BlackjackGameManager.EndRound().
    /// </summary>
    public void SettleRound(bool playerWon, bool push, bool blackjack = false)
    {
        int payout = 0;

        if (blackjack)
        {
            // Blackjack paga 3:2 → devuelve apuesta + 1.5× apuesta
            payout = _currentBet + Mathf.RoundToInt(_currentBet * 1.5f);
        }
        else if (playerWon)
        {
            // Victoria normal: devuelve apuesta + ganancia igual (1:1)
            payout = _currentBet * 2;
        }
        else if (push)
        {
            // Empate: devuelve solo la apuesta
            payout = _currentBet;
        }
        // Derrota: payout = 0 (la apuesta ya fue descontada en TryPlaceBet)

        int netChange = payout - _currentBet; // ganancia o pérdida neta
        _chips += payout;
        _currentBet = 0;

        OnChipsChanged?.Invoke(_chips);
        OnRoundSettled?.Invoke(netChange);

        // Verificar game over
        if (_chips <= 0)
        {
            Debug.Log("[Economy] ¡Game Over! Sin fichas.");
        }
    }

    /// <summary>Agrega fichas manualmente (recompra).</summary>
    public void AddChips(int amount)
    {
        if (amount <= 0) return;
        _chips += amount;
        OnChipsChanged?.Invoke(_chips);
    }

    /// <summary>Reinicia al balance inicial.</summary>
    public void ResetBalance()
    {
        _currentBet = 0;
        _chips = startingChips;
        OnChipsChanged?.Invoke(_chips);
    }
}