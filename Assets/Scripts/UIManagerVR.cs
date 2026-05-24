using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using UnityEngine.UI;

public class UIManagerVR : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMenuInicial;
    public GameObject panelJuego;
    public GameObject panelGameOver;

    [Header("Textos de Juego")]
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI txtApuesta;
    public TextMeshProUGUI txtMensajeResultado;

    [Header("Mock Data (Solo para pruebas)")]
    private int mockScore = 1000;
    private int mockApuesta = 50;

    void Start()
    {
        // Estado inicial: Mostrar solo el menú
        MostrarMenuInicial();
    }

    // --- NAVEGACIÓN DE PANELES ---
    public void MostrarMenuInicial()
    {
        panelMenuInicial.SetActive(true);
        panelJuego.SetActive(false);
        panelGameOver.SetActive(false);
    }

    public void MostrarPanelJuego()
    {
        panelMenuInicial.SetActive(false);
        panelJuego.SetActive(true);
        panelGameOver.SetActive(false);
        ActualizarUIJuego();
    }

    public void MostrarGameOver(string mensajeResultado)
    {
        panelMenuInicial.SetActive(false);
        panelJuego.SetActive(false);
        panelGameOver.SetActive(true);
        txtMensajeResultado.text = mensajeResultado;
    }

    // --- MÉTODOS PARA BOTONES DEL JUEGO (MOCK) ---
    public void OnBtnHitClicked()
    {
        Debug.Log("Jugador pide carta (Hit).");
        // Aquí el Integrante F conectará: logic.DealCardToPlayer()
    }

    public void OnBtnStandClicked()
    {
        Debug.Log("Jugador se planta (Stand). Turno del crupier.");
        MostrarGameOver("Te plantaste. ¡Ganaste la simulación!"); // Mock test
    }

    public void OnBtnDoubleClicked()
    {
        Debug.Log("Jugador dobla apuesta (Double).");
        mockApuesta *= 2;
        ActualizarUIJuego();
    }
    
    public void OnBtnSurrenderClicked()
    {
        Debug.Log("Jugador se rinde (Surrender).");
        MostrarGameOver("Te rendiste.");
    }

    public void OnBtnRestartClicked()
{
    Debug.Log("Reiniciando el juego...");
    
    // 1. Restablecer los datos simulados (Mock Data) a sus valores iniciales
    mockScore = 1000;
    mockApuesta = 50;
    
    // 2. Volver a cargar el panel de juego para iniciar una nueva partida
    MostrarPanelJuego();
}

    // --- ACTUALIZACIÓN DE DATOS ---
    public void ActualizarUIJuego()
    {
        // El Integrante F cambiará estas variables por logic.GetPlayerScore() etc.
        txtScore.text = "Puntaje: $" + mockScore.ToString();
        txtApuesta.text = "Apuesta: $" + mockApuesta.ToString();
    }

    // --- BOTONES DE MENÚ ---
    public void OnBtnStartGameClicked()
    {
        MostrarPanelJuego();
    }

    public void OnBtnExitClicked()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}