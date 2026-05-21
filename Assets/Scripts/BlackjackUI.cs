using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  BlackjackUI.cs  —  CORREGIDO v2
//
//  CAMBIOS RESPECTO AL ORIGINAL:
//  1. FindObjectOfType ELIMINADO.
//       Antes: gameManager = FindObjectOfType<BlackjackGameManager>();
//       Ahora: [SerializeField] private BlackjackGameManager gameManager;
//              (asignar en el Inspector arrastrando el GameObject)
//
//  2. Suscripción a EVENTOS en vez de ser llamada directamente.
//       Antes: GameManager llamaba ui.UpdateHands(...) directamente
//       Ahora: UI escucha gameManager.OnHandUpdated, etc.
//              GameManager no sabe que BlackjackUI existe.
//
//  3. Nuevo panel de estado de fichas/apuesta reactivo.
//  4. Botones VR (Hit/Stand/Double) con feedback visual de estado.
// ============================================================

public class BlackjackUI : MonoBehaviour
{
    // ── Referencias a texto ──────────────────────────────────
    [Header("Puntajes")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI dealerScoreText;
    public TextMeshProUGUI messageText;

    [Header("Economía")]
    public TextMeshProUGUI chipsText;
    public TextMeshProUGUI betText;

    [Header("Botones de acción")]
    public GameObject actionButtonPanel;
    public Button     hitButton;
    public Button     standButton;
    public Button     doubleButton;

    [Header("Botones de apuesta")]
    public GameObject betButtonPanel;
    public Button     bet10Button;
    public Button     bet25Button;
    public Button     bet50Button;
    public Button     bet100Button;

    // ── Referencia al GameManager (asignar en Inspector) ────
    // FIX: Reemplaza FindObjectOfType<BlackjackGameManager>()
    [Header("Sistema de juego")]
    [SerializeField] private BlackjackGameManager gameManager;

    // ── Ciclo de vida ────────────────────────────────────────
    void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("[BlackjackUI] No hay BlackjackGameManager asignado en el Inspector.");
            return;
        }

        // ── Suscribir a eventos del GameManager ──────────────
        // FIX: GameManager ya no llama ui.* directamente.
        //      UI se suscribe a los eventos que le interesan.
        gameManager.OnHandUpdated  += HandleHandUpdated;
        gameManager.OnMessage      += HandleMessage;
        gameManager.OnStateChanged += HandleStateChanged;
        gameManager.OnRoundEnded   += HandleRoundEnded;

        // Suscribir a eventos del EconomySystem
        gameManager.Economy.OnChipsChanged  += HandleChipsChanged;
        gameManager.Economy.OnBetPlaced     += HandleBetPlaced;

        // ── Conectar botones de acción ────────────────────────
        hitButton.onClick.AddListener(()    => gameManager.PlayerHit());
        standButton.onClick.AddListener(()  => gameManager.PlayerStand());
        doubleButton.onClick.AddListener(() => gameManager.PlayerDoubleDown());

        // ── Conectar botones de apuesta ───────────────────────
        bet10Button.onClick.AddListener(()  => gameManager.PlaceBet(10));
        bet25Button.onClick.AddListener(()  => gameManager.PlaceBet(25));
        bet50Button.onClick.AddListener(()  => gameManager.PlaceBet(50));
        bet100Button.onClick.AddListener(() => gameManager.PlaceBet(100));

        // Estado inicial
        ShowActionButtons(false);
        ShowBetButtons(false);
        UpdateChipsDisplay(gameManager.Economy.Chips, 0);
    }

    void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        if (gameManager == null) return;
        gameManager.OnHandUpdated  -= HandleHandUpdated;
        gameManager.OnMessage      -= HandleMessage;
        gameManager.OnStateChanged -= HandleStateChanged;
        gameManager.OnRoundEnded   -= HandleRoundEnded;
        gameManager.Economy.OnChipsChanged -= HandleChipsChanged;
        gameManager.Economy.OnBetPlaced    -= HandleBetPlaced;
    }

    // ── Manejadores de eventos ───────────────────────────────

    void HandleHandUpdated(int playerScore, string dealerScore)
    {
        if (playerScoreText != null)
            playerScoreText.text = $"Jugador: {playerScore}";
        if (dealerScoreText != null)
            dealerScoreText.text = $"Crupier: {dealerScore}";
    }

    void HandleMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
    }

    void HandleStateChanged(BlackjackGameManager.GameState state)
    {
        switch (state)
        {
            case BlackjackGameManager.GameState.WaitingForBet:
                ShowBetButtons(true);
                ShowActionButtons(false);
                break;

            case BlackjackGameManager.GameState.PlayerTurn:
                ShowBetButtons(false);
                ShowActionButtons(true);
                // Double solo disponible con exactamente 2 cartas
                UpdateDoubleButton();
                break;

            case BlackjackGameManager.GameState.DealerTurn:
                ShowActionButtons(false);
                break;

            case BlackjackGameManager.GameState.RoundOver:
                ShowActionButtons(false);
                ShowBetButtons(false);
                break;
        }
    }

    void HandleRoundEnded(BlackjackGameManager.RoundOutcome outcome)
    {
        // Actualizar puntajes finales (dealer ya está revelado)
        if (dealerScoreText != null)
            dealerScoreText.text = $"Crupier: {outcome.DealerScore}";
    }

    void HandleChipsChanged(int newChips)
    {
        UpdateChipsDisplay(newChips, gameManager.Economy.CurrentBet);
    }

    void HandleBetPlaced(int betAmount)
    {
        UpdateChipsDisplay(gameManager.Economy.Chips, betAmount);
    }

    // ── Helpers de UI ────────────────────────────────────────

    void UpdateChipsDisplay(int chips, int bet)
    {
        if (chipsText != null)
            chipsText.text = $"Fichas: ${chips}";
        if (betText != null)
            betText.text = bet > 0 ? $"Apuesta: ${bet}" : "";
    }

    void UpdateDoubleButton()
    {
        if (doubleButton == null) return;
        bool canDouble = gameManager.PlayerHand.Cards.Count == 2
                      && gameManager.Economy.Chips >= gameManager.Economy.CurrentBet;
        doubleButton.interactable = canDouble;
    }

    public void ShowActionButtons(bool show)
    {
        if (actionButtonPanel != null)
            actionButtonPanel.SetActive(show);
    }

    public void ShowBetButtons(bool show)
    {
        if (betButtonPanel != null)
            betButtonPanel.SetActive(show);
    }
}