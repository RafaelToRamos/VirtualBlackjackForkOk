using System.Collections;
using UnityEngine;

// ============================================================
//  BlackjackGameManager.cs 
//
//  1. Economía EXTRAÍDA a EconomySystem.cs:
//       playerChips y currentBet ya no viven aquí.
//       GameManager notifica el resultado; EconomySystem calcula.
//
//  2. Comunicación con UI via EVENTOS (desacoplado):
//       Antes: gameManager.ui.UpdateHands(...)  → acoplamiento duro
//       Ahora: OnHandUpdated?.Invoke(...)       → UI se suscribe
//
//  3. FindObjectOfType ELIMINADO de BlackjackUI y VRBetChip:
//       Ahora reciben referencia por Inspector ([SerializeField]).
//
//  4. Separación de responsabilidades:
//       GameManager: flujo de juego + estados
//       EconomySystem: chips + apuestas + pagos
//       BlackjackUI: presentación (escucha eventos)
//       CardLayoutManager: posicionamiento 3D de cartas
// ============================================================

[RequireComponent(typeof(EconomySystem))]
public class BlackjackGameManager : MonoBehaviour
{
    public enum GameState { WaitingForBet, PlayerTurn, DealerTurn, RoundOver }

    // ── Referencias (asignar en Inspector) ──────────────────
    [Header("Referencias")]
    public BlackjackDeck     deck;
    public CardLayoutManager cardLayout;

    [Header("Configuración")]
    public int numDecks = 6;

    // ── Componente de economía (en el mismo GameObject) ─────
    private EconomySystem economy;

    // ── Estado interno ──────────────────────────────────────
    private BlackjackHand playerHand = new BlackjackHand();
    private BlackjackHand dealerHand = new BlackjackHand();
    private GameState     currentState;

    // ════════════════════════════════════════════════════════
    //  EVENTOS — BlackjackUI y otros módulos se suscriben aquí
    //  en lugar de ser llamados directamente por el GameManager.
    //  Esto elimina el acoplamiento duro.
    // ════════════════════════════════════════════════════════

    /// <summary>Puntaje del jugador cambió. string = score del dealer ("?" si oculto).</summary>
    public event System.Action<int, string> OnHandUpdated;

    /// <summary>Mensaje para mostrar en pantalla.</summary>
    public event System.Action<string>      OnMessage;

    /// <summary>Estado del juego cambió.</summary>
    public event System.Action<GameState>   OnStateChanged;

    /// <summary>Una ronda terminó. bool = playerWon, bool = push.</summary>
    public event System.Action<RoundOutcome> OnRoundEnded;

    /// <summary>Se repartió una carta. bool = esDelJugador.</summary>
    public event System.Action<BlackjackCard, bool> OnCardDealt;

    // ── Propiedades de solo lectura para consultas externas ─
    public GameState      CurrentState  => currentState;
    public BlackjackHand  PlayerHand    => playerHand;
    public BlackjackHand  DealerHand    => dealerHand;
    public EconomySystem  Economy       => economy;

    // ── Resultado de ronda (struct para datos agrupados) ────
    public struct RoundOutcome
    {
        public bool PlayerWon;
        public bool Push;
        public bool PlayerBlackjack;
        public bool DealerBlackjack;
        public int  PlayerScore;
        public int  DealerScore;
    }

    // ── Ciclo de vida Unity ──────────────────────────────────
    void Awake()
    {
        economy = GetComponent<EconomySystem>();
    }

    void Start()
    {
        deck.Initialize(numDecks);
        deck.OnDeckLow += () => OnMessage?.Invoke("Remezclar mazo...");
        StartNewRound();
    }

    // ════════════════════════════════════════════════════════
    //  API PÚBLICA — llamada desde BlackjackUI y VRBetChip
    // ════════════════════════════════════════════════════════

    public void StartNewRound()
    {
        playerHand.Clear();
        dealerHand.Clear();
        cardLayout.ClearTable();

        SetState(GameState.WaitingForBet);
        OnMessage?.Invoke("Coloca tu apuesta");
    }

    /// <summary>Llamado por BlackjackUI (botones de apuesta) o VRBetChip.</summary>
    public void PlaceBet(int amount)
    {
        if (currentState != GameState.WaitingForBet) return;
        if (!economy.TryPlaceBet(amount)) return; // EconomySystem valida

        StartCoroutine(DealInitialCards());
    }

    /// <summary>Jugador pide carta.</summary>
    public void PlayerHit()
    {
        if (currentState != GameState.PlayerTurn) return;
        StartCoroutine(PlayerHitRoutine());
    }

    /// <summary>Jugador se planta.</summary>
    public void PlayerStand()
    {
        if (currentState != GameState.PlayerTurn) return;
        StartCoroutine(DealerTurnRoutine());
    }

    /// <summary>Doble apuesta + una carta + plantarse.</summary>
    public void PlayerDoubleDown()
    {
        if (currentState != GameState.PlayerTurn) return;
        if (playerHand.Cards.Count != 2) return;
        if (!economy.TryDoubleDown()) return; // EconomySystem valida y duplica

        StartCoroutine(DoubleDownRoutine());
    }

    // ════════════════════════════════════════════════════════
    //  CORRUTINAS INTERNAS
    // ════════════════════════════════════════════════════════

    IEnumerator DealInitialCards()
    {
        SetState(GameState.PlayerTurn);

        // Orden clásico: Jugador → Crupier → Jugador → Crupier (hoyo)
        yield return DealCardTo(playerHand, isPlayer: true,  faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(dealerHand, isPlayer: false, faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(playerHand, isPlayer: true,  faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(dealerHand, isPlayer: false, faceUp: false); // carta hoyo
        yield return new WaitForSeconds(0.4f);

        // Notificar puntajes — dealer muestra "?" mientras tiene carta oculta
        OnHandUpdated?.Invoke(playerHand.GetValue(), "?");

        // Verificar Blackjack natural inmediatamente
        if (playerHand.IsBlackjack())
        {
            OnMessage?.Invoke("¡Blackjack!");
            yield return new WaitForSeconds(1f);
            yield return DealerTurnRoutine();
        }
    }

    IEnumerator DealCardTo(BlackjackHand hand, bool isPlayer, bool faceUp)
    {
        BlackjackCard card = deck.DrawCard();
        hand.AddCard(card);

        // Notificar a CardLayoutManager (visual) y a cualquier suscriptor
        cardLayout.PlaceCard(card, isPlayer, faceUp);
        OnCardDealt?.Invoke(card, isPlayer);

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator PlayerHitRoutine()
    {
        yield return DealCardTo(playerHand, isPlayer: true, faceUp: true);
        OnHandUpdated?.Invoke(playerHand.GetValue(), "?");

        if (playerHand.IsBust())
        {
            OnMessage?.Invoke("¡Bust! Pierdes.");
            yield return new WaitForSeconds(1.5f);
            EndRound(playerWon: false, push: false);
        }
    }

    IEnumerator DoubleDownRoutine()
    {
        yield return DealCardTo(playerHand, isPlayer: true, faceUp: true);
        OnHandUpdated?.Invoke(playerHand.GetValue(), "?");
        yield return new WaitForSeconds(0.5f);

        if (playerHand.IsBust())
        {
            OnMessage?.Invoke("¡Bust! Pierdes.");
            yield return new WaitForSeconds(1.5f);
            EndRound(playerWon: false, push: false);
        }
        else
        {
            yield return DealerTurnRoutine();
        }
    }

    IEnumerator DealerTurnRoutine()
    {
        SetState(GameState.DealerTurn);

        // Revelar carta hoyo
        cardLayout.FlipAllDealerCards();
        yield return new WaitForSeconds(0.5f);
        OnHandUpdated?.Invoke(playerHand.GetValue(), dealerHand.GetValue().ToString());

        // El crupier se planta en CUALQUIER 17, incluyendo soft 17.

        while (dealerHand.GetValue() < 17)
        {
            yield return new WaitForSeconds(0.8f);
            yield return DealCardTo(dealerHand, isPlayer: false, faceUp: true);
            OnHandUpdated?.Invoke(playerHand.GetValue(), dealerHand.GetValue().ToString());
        }

        yield return new WaitForSeconds(0.5f);
        ResolveRound();
    }

    // ════════════════════════════════════════════════════════
    //  LÓGICA DE RESOLUCIÓN
    // ════════════════════════════════════════════════════════

    void ResolveRound()
    {
        int  playerVal = playerHand.GetValue();
        int  dealerVal = dealerHand.GetValue();
        bool playerBJ  = playerHand.IsBlackjack();
        bool dealerBJ  = dealerHand.IsBlackjack();

        bool playerWon = false;
        bool push      = false;
        string message;

        if (dealerBJ && playerBJ)
        {
            message = "¡Empate — ambos Blackjack!";
            push = true;
        }
        else if (playerBJ)
        {
            message = "¡Blackjack! Ganas 3:2";
            playerWon = true;
        }
        else if (dealerBJ)
        {
            message = "Crupier tiene Blackjack. Pierdes.";
        }
        else if (playerHand.IsBust())
        {
            message = "¡Bust! Crupier gana.";
        }
        else if (dealerHand.IsBust())
        {
            message = "¡Crupier bust! Ganas.";
            playerWon = true;
        }
        else if (playerVal > dealerVal)
        {
            message = $"¡Ganas! {playerVal} vs {dealerVal}";
            playerWon = true;
        }
        else if (dealerVal > playerVal)
        {
            message = $"Crupier gana. {dealerVal} vs {playerVal}";
        }
        else
        {
            message = $"¡Empate! {playerVal}";
            push = true;
        }

        OnMessage?.Invoke(message);
        EndRound(playerWon, push, playerBJ);
    }

    void EndRound(bool playerWon, bool push, bool playerBlackjack = false)
    {
        SetState(GameState.RoundOver);

        // Delegar cálculo de fichas al EconomySystem
        economy.SettleRound(playerWon, push, playerBlackjack);

        // Notificar resultado completo a suscriptores (UI, audio, etc.)
        OnRoundEnded?.Invoke(new RoundOutcome
        {
            PlayerWon       = playerWon,
            Push            = push,
            PlayerBlackjack = playerBlackjack,
            PlayerScore     = playerHand.GetValue(),
            DealerScore     = dealerHand.GetValue()
        });

        Invoke(nameof(StartNewRound), 3f);
    }

    void SetState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}