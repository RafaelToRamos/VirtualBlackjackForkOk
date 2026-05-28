using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManagerVR : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMenuInicial;
    public GameObject panelJuego;
    public GameObject panelGameOver;

    [Header("Paneles dentro de GamePanel")]
    public GameObject actionButtonPanel;  // ActionButtonPanel
    public GameObject betButtonPanel;     // BetButtonPanel

    [Header("Textos")]
    public TextMeshProUGUI txtScore;        // ScoreTag
    public TextMeshProUGUI txtApuesta;      // BetTag
    public TextMeshProUGUI txtDealer;       // DealerScoreTag
    public TextMeshProUGUI txtMensaje;      // MessageTag
    public TextMeshProUGUI txtChips;        // ChipsTag

    [Header("Botones de acción")]
    public Button hitButton;
    public Button standButton;
    public Button doubleButton;
    public Button surrenderButton;

    [Header("Botones de apuesta")]
    public Button bet10Button;
    public Button bet25Button;
    public Button bet50Button;
    public Button bet100Button;

    [Header("Referencia al juego")]
    [SerializeField] private BlackjackGameManager gameManager;

    // ── Ciclo de vida ────────────────────────────────────────
    void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("[UIManagerVR] No hay BlackjackGameManager asignado.");
            return;
        }

        // Conectar botones de acción
        hitButton.onClick.AddListener(()       => gameManager.PlayerHit());
        standButton.onClick.AddListener(()     => gameManager.PlayerStand());
        doubleButton.onClick.AddListener(()    => gameManager.PlayerDoubleDown());
        surrenderButton.onClick.AddListener(() => gameManager.PlayerSurrender());

        // Conectar botones de apuesta
        bet10Button.onClick.AddListener(()  => gameManager.PlaceBet(10));
        bet25Button.onClick.AddListener(()  => gameManager.PlaceBet(25));
        bet50Button.onClick.AddListener(()  => gameManager.PlaceBet(50));
        bet100Button.onClick.AddListener(() => gameManager.PlaceBet(100));

        // Suscribir a eventos del GameManager
        gameManager.OnHandUpdated  += HandleHandUpdated;
        gameManager.OnMessage      += HandleMessage;
        gameManager.OnStateChanged += HandleStateChanged;
        gameManager.OnRoundEnded   += HandleRoundEnded;
        gameManager.OnDoubleAvailable += canDouble =>
        {
            if (doubleButton != null)
                doubleButton.interactable = canDouble;
        };
        gameManager.OnSurrenderAvailable += canSurrender =>
        {
            if (surrenderButton != null)
                surrenderButton.interactable = canSurrender;
        };

        // Suscribir a eventos de economía
        gameManager.Economy.OnChipsChanged += HandleChipsChanged;
        gameManager.Economy.OnBetPlaced    += HandleBetPlaced;
    }

    void OnDestroy()
    {
        if (gameManager == null) return;
        gameManager.OnHandUpdated  -= HandleHandUpdated;
        gameManager.OnMessage      -= HandleMessage;
        gameManager.OnStateChanged -= HandleStateChanged;
        gameManager.OnRoundEnded   -= HandleRoundEnded;
        gameManager.Economy.OnChipsChanged -= HandleChipsChanged;
        gameManager.Economy.OnBetPlaced    -= HandleBetPlaced;
    }

    void Start()
    {
        MostrarMenuInicial();
    }

    // ── Navegación de paneles ────────────────────────────────
    public void MostrarMenuInicial()
    {
        if (panelMenuInicial != null) panelMenuInicial.SetActive(true);
        if (panelJuego != null)       panelJuego.SetActive(false);
        if (panelGameOver != null)    panelGameOver.SetActive(false);
    }

    public void MostrarPanelJuego()
    {
        if (panelMenuInicial != null) panelMenuInicial.SetActive(false);
        if (panelJuego != null)       panelJuego.SetActive(true);
        if (panelGameOver != null)    panelGameOver.SetActive(false);
    }

    public void MostrarGameOver(string mensaje)
    {
        if (panelMenuInicial != null) panelMenuInicial.SetActive(false);
        if (panelJuego != null)       panelJuego.SetActive(false);
        if (panelGameOver != null)    panelGameOver.SetActive(true);
        if (txtMensaje != null)       txtMensaje.text = mensaje;
    }

    // ── Manejadores de eventos del GameManager ───────────────
    void HandleHandUpdated(int playerScore, string dealerScore)
    {
        if (txtScore != null)  txtScore.text  = $"Jugador: {playerScore}";
        if (txtDealer != null) txtDealer.text = $"Crupier: {dealerScore}";
    }

    void HandleMessage(string msg)
    {
        if (txtMensaje != null) txtMensaje.text = msg;
    }

    void HandleStateChanged(BlackjackGameManager.GameState state)
    {
        switch (state)
        {
            case BlackjackGameManager.GameState.WaitingForBet:
                MostrarPanelJuego();
                ShowBetButtons(true);
                ShowActionButtons(false);
                break;

            case BlackjackGameManager.GameState.PlayerTurn:
                ShowBetButtons(false);
                ShowActionButtons(true);
                // Double solo disponible con 2 cartas y fichas suficientes
                /*
                if (doubleButton != null)
                    doubleButton.interactable =
                        gameManager.PlayerHand.Cards.Count == 2 &&
                        gameManager.Economy.Chips >= gameManager.Economy.CurrentBet;
                */
                break;

            case BlackjackGameManager.GameState.DealerTurn:
                ShowActionButtons(false);
                break;

            case BlackjackGameManager.GameState.RoundOver:
                ShowActionButtons(false);
                ShowBetButtons(false);
                // Si se quedó sin fichas, mostrar Game Over
                if (gameManager.Economy.Chips <= 0)
                    MostrarGameOver("¡Sin fichas! Fin del juego.");
                break;
        }
    }

    void HandleRoundEnded(BlackjackGameManager.RoundOutcome outcome)
    {
        if (txtScore != null)
            txtScore.text = $"Jugador: {outcome.PlayerScore}";
        if (txtDealer != null)
            txtDealer.text = $"Crupier: {outcome.DealerScore}";
    }

    void HandleChipsChanged(int chips)
    {
        if (txtChips != null)
            txtChips.text = $"Fichas: ${chips}";
    }

    void HandleBetPlaced(int bet)
    {
        if (txtApuesta != null)
            txtApuesta.text = $"Apuesta: ${bet}";
    }

    public void OnBtnStartGameClicked()
    {
        MostrarPanelJuego();
        gameManager.StartNewRound();
    }

    public void OnBtnRestartClicked()
    {
        gameManager.Economy.ResetBalance();
        gameManager.StartNewRound();
        MostrarPanelJuego();
    }

    public void OnBtnExitClicked()
    {
        Application.Quit();
    }

    // ── Helpers ──────────────────────────────────────────────
    void ShowActionButtons(bool show)
    {
        if (actionButtonPanel != null) actionButtonPanel.SetActive(show);
    }

    void ShowBetButtons(bool show)
    {
        if (betButtonPanel != null) betButtonPanel.SetActive(show);
    }
}