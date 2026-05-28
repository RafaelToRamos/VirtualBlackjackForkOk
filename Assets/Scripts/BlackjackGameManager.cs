using System.Collections;
using UnityEngine;

// ============================================================
//  BlackjackGameManager.cs — VERSIÓN FINAL
//
//  Cambios respecto al original:
//  - Eliminada referencia a BlackjackUI (ya no existe)
//  - Eliminado card.isFaceUp (ya no existe en BlackjackCard)
//  - Regla soft 17 corregida (hard 17 = Vegas Strip)
//  - Economía delegada a EconomySystem
//  - Comunicación con UI via EVENTOS C# (no referencias directas)
//  - RoundOutcome struct agregado para OnRoundEnded
//  - AudioManager y EffectsManager mantenidos con ?.
// ============================================================

[RequireComponent(typeof(EconomySystem))]
public class BlackjackGameManager : MonoBehaviour
{
    public enum GameState { WaitingForBet, PlayerTurn, DealerTurn, RoundOver }

    // ── Resultado de ronda ───────────────────────────────────
    public struct RoundOutcome
    {
        public bool PlayerWon;
        public bool Push;
        public bool PlayerBlackjack;
        public bool DealerBlackjack;
        public int  PlayerScore;
        public int  DealerScore;
    }

    [Header("Referencias")]
    public BlackjackDeck     deck;
    public CardLayoutManager cardLayout;

    [Header("Configuración")]
    public int numDecks = 6;

    // ── Componente de economía ───────────────────────────────
    private EconomySystem economy;
    public  EconomySystem Economy => economy;

    // ── Manos ────────────────────────────────────────────────
    private BlackjackHand playerHand = new BlackjackHand();
    private BlackjackHand dealerHand = new BlackjackHand();
    public  BlackjackHand PlayerHand => playerHand;
    public  BlackjackHand DealerHand => dealerHand;

    // ── Estado ───────────────────────────────────────────────
    private GameState currentState;
    public  GameState CurrentState => currentState;

    // ── Eventos (UIManagerVR se suscribe a estos) ────────────
    public event System.Action<int, string>      OnHandUpdated;
    public event System.Action<string>           OnMessage;
    public event System.Action<GameState>        OnStateChanged;
    public event System.Action<RoundOutcome>     OnRoundEnded;
    public event System.Action<BlackjackCard, bool> OnCardDealt;
    public event System.Action<bool> OnDoubleAvailable;
    public event System.Action<bool> OnSurrenderAvailable;

    // ── Ciclo de vida ────────────────────────────────────────
    void Awake()
    {
        economy = GetComponent<EconomySystem>();
    }

    void Start()
    {
        deck.Initialize(numDecks);
        deck.OnDeckLow += () => OnMessage?.Invoke("Remezclar mazo...");
        // StartNewRound() lo llama UIManagerVR al presionar Start
        // Si quieres que arranque solo, descomenta la línea de abajo:
        // StartNewRound();
    }

    // ── API Pública ──────────────────────────────────────────

    public void StartNewRound()
    {
        playerHand.Clear();
        dealerHand.Clear();
        cardLayout.ClearTable();

        SetState(GameState.WaitingForBet);
        OnMessage?.Invoke("Coloca tu apuesta");
    }

    public void PlaceBet(int amount)
    {
        if (currentState != GameState.WaitingForBet) return;
        if (!economy.TryPlaceBet(amount)) return;

        AudioManager.Instance?.PlayChipPlace();
        StartCoroutine(DealInitialCards());
    }

    public void PlayerHit()
    {
        if (currentState != GameState.PlayerTurn) return;
        //Debug.Log("[GM] PlayerHit llamado"); // ← agregar
        StartCoroutine(PlayerHitRoutine());
    }

    public void PlayerStand()
    {
        if (currentState != GameState.PlayerTurn) return;
        StartCoroutine(DealerTurnRoutine());
    }

    public void PlayerSurrender()
    {
        if (currentState != GameState.PlayerTurn) return;
        if (playerHand.Cards.Count != 2) return;

        OnMessage?.Invoke("Te rendiste. Recuperas la mitad.");
        economy.SettleSurrender();
        StartCoroutine(SurrenderRoutine());
    }

    public void PlayerDoubleDown()
    {
        if (currentState != GameState.PlayerTurn) return;
        if (playerHand.Cards.Count != 2) return;
        if (!economy.TryDoubleDown()) return;

        StartCoroutine(DoubleDownRoutine());
    }

    // ── Corrutinas ───────────────────────────────────────────

    IEnumerator DealInitialCards()
    {
        SetState(GameState.PlayerTurn);

        yield return DealCardTo(playerHand, isPlayer: true,  faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(dealerHand, isPlayer: false, faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(playerHand, isPlayer: true,  faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(dealerHand, isPlayer: false, faceUp: false); // carta hoyo
        yield return new WaitForSeconds(0.4f);

        OnHandUpdated?.Invoke(playerHand.GetValue(), "?");
        UpdateDoubleButton();

        if (playerHand.IsBlackjack())
        {
            OnMessage?.Invoke("¡Blackjack!");
            AudioManager.Instance?.PlayBlackjack();
            EffectsManager.Instance?.PlayBlackjackEffect(transform.position);
            yield return new WaitForSeconds(1f);
            yield return DealerTurnRoutine();
        }
    }

    void UpdateDoubleButton()
    {
        bool firstTwoCards = playerHand.Cards.Count == 2;
        bool canDouble = playerHand.Cards.Count == 2 &&
                        economy.Chips >= economy.CurrentBet;
        OnDoubleAvailable?.Invoke(canDouble);
        OnSurrenderAvailable?.Invoke(firstTwoCards);
    }
    IEnumerator SurrenderRoutine()
    {
        yield return StartCoroutine(cardLayout.FlipDealerCardsCoroutine());
        AudioManager.Instance?.PlayDealerReveal();
        yield return new WaitForSeconds(0.5f);
        OnHandUpdated?.Invoke(playerHand.GetValue(), dealerHand.GetValue().ToString());
        EndRound(playerWon: false, push: false);
    }

    IEnumerator DealCardTo(BlackjackHand hand, bool isPlayer, bool faceUp)
    {
        BlackjackCard card = deck.DrawCard();
        hand.AddCard(card);
        cardLayout.PlaceCard(card, isPlayer, faceUp);
        OnCardDealt?.Invoke(card, isPlayer);

        AudioManager.Instance?.PlayCardDeal();
        if (cardLayout != null)
        {
            Vector3 cardPos = isPlayer
                ? cardLayout.GetNextPlayerCardPosition()
                : cardLayout.GetNextDealerCardPosition();
            EffectsManager.Instance?.PlayCardDealEffect(cardPos);
        }

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator PlayerHitRoutine()
    {
        yield return DealCardTo(playerHand, isPlayer: true, faceUp: true);
        OnHandUpdated?.Invoke(playerHand.GetValue(), "?");
        UpdateDoubleButton(); // deshabilita doble y rendirse después de 3+ cartas

        if (playerHand.IsBust())
        {
            OnMessage?.Invoke("¡Bust! Pierdes.");
            AudioManager.Instance?.PlayBust();
            EffectsManager.Instance?.PlayLoseEffect(transform.position);
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

        // Revelar carta hoyo con animación
        yield return StartCoroutine(cardLayout.FlipDealerCardsCoroutine());
        AudioManager.Instance?.PlayDealerReveal();
        yield return new WaitForSeconds(0.5f);
        OnHandUpdated?.Invoke(playerHand.GetValue(), dealerHand.GetValue().ToString());

        // Regla hard 17 — crupier se planta en cualquier 17
        while (dealerHand.GetValue() < 17)
        {
            yield return new WaitForSeconds(0.8f);
            yield return DealCardTo(dealerHand, isPlayer: false, faceUp: true);
            OnHandUpdated?.Invoke(playerHand.GetValue(), dealerHand.GetValue().ToString());
        }

        yield return new WaitForSeconds(0.5f);
        ResolveRound();
    }

    // ── Lógica de resolución ─────────────────────────────────

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
            AudioManager.Instance?.PlayBlackjack();
            EffectsManager.Instance?.PlayBlackjackEffect(transform.position);
        }
        else if (dealerBJ)
        {
            message = "Crupier tiene Blackjack. Pierdes.";
            AudioManager.Instance?.PlayLose();
            EffectsManager.Instance?.PlayLoseEffect(transform.position);
        }
        else if (playerHand.IsBust())
        {
            message = "¡Bust! Crupier gana.";
        }
        else if (dealerHand.IsBust())
        {
            message = "¡Crupier bust! Ganas.";
            playerWon = true;
            AudioManager.Instance?.PlayWin();
            EffectsManager.Instance?.PlayWinEffect(transform.position);
        }
        else if (playerVal > dealerVal)
        {
            message = $"¡Ganas! {playerVal} vs {dealerVal}";
            playerWon = true;
            AudioManager.Instance?.PlayWin();
            EffectsManager.Instance?.PlayWinEffect(transform.position);
        }
        else if (dealerVal > playerVal)
        {
            message = $"Crupier gana. {dealerVal} vs {playerVal}";
            AudioManager.Instance?.PlayLose();
            EffectsManager.Instance?.PlayLoseEffect(transform.position);
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
    // 1. Primero liquidar — que las fichas estén correctas
    economy.SettleRound(playerWon, push, playerBlackjack);

    // 2. Luego cambiar estado — UIManagerVR ya ve las fichas correctas
    SetState(GameState.RoundOver);

    // 3. Emitir resultado
    OnRoundEnded?.Invoke(new RoundOutcome
    {
        PlayerWon        = playerWon,
        Push             = push,
        PlayerBlackjack  = playerBlackjack,
        PlayerScore      = playerHand.GetValue(),
        DealerScore      = dealerHand.GetValue()
    });

    // 4. Decidir si continuar o game over
    if (economy.Chips < 10)
        return;

    Invoke(nameof(StartNewRound), 3f);
}

    void SetState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}