using UnityEngine;

// ============================================================
//  VRBetChip.cs  —  CORREGIDO v2
//
//  CAMBIOS RESPECTO AL ORIGINAL:
//  1. FindObjectOfType ELIMINADO.
//       Antes: gameManager = FindObjectOfType<BlackjackGameManager>();
//             → Búsqueda global en escena (costoso, frágil)
//       Ahora: [SerializeField] BlackjackGameManager gameManager
//             → Asignar en Inspector arrastrando el GameObject
//
//  2. Múltiples chips en la misma ronda:
//       Antes: una vez colocado, el chip bloqueaba cualquier
//              apuesta adicional hasta el reset.
//       Ahora: TryAddToBet() permite colocar varios chips
//              antes de que se repartan cartas.
//
//  3. ResetChip() sin cambios funcionales.
//
//  SETUP EN UNITY:
//   • Agregar este script a cada ficha física (GameObject 3D).
//   • Agregar un Collider al chip (trigger o sólido según diseño VR).
//   • Agregar un Collider con IsTrigger = true a la zona de apuesta
//     y etiquetarla con Tag = "BetZone".
//   • Arrastrar el GameManager al campo "gameManager" en el Inspector.
// ============================================================

public class VRBetChip : MonoBehaviour
{
    [Header("Configuración")]
    public int chipValue = 25;

    // FIX: Asignar en Inspector — elimina FindObjectOfType
    [Header("Referencias (asignar en Inspector)")]
    [SerializeField] private BlackjackGameManager gameManager;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _hasBeenPlayed = false;

    void Awake()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        if (gameManager == null)
            Debug.LogError($"[VRBetChip] No hay GameManager asignado en {gameObject.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasBeenPlayed) return;
        if (!other.CompareTag("BetZone")) return;

        // Verificar que estamos en estado de apuesta
        if (gameManager.CurrentState != BlackjackGameManager.GameState.WaitingForBet)
        {
            Debug.Log("[VRBetChip] No se puede apostar ahora — el juego no está esperando apuesta.");
            return;
        }

        _hasBeenPlayed = true;
        gameManager.PlaceBet(chipValue);

        // Snap a la zona de apuesta
        transform.position = other.transform.position;
        transform.rotation = other.transform.rotation;

        Debug.Log($"[VRBetChip] Apuesta colocada: ${chipValue}");
    }

    /// <summary>
    /// Devuelve el chip a su posición original.
    /// Llamar desde BlackjackGameManager al inicio de cada ronda.
    /// </summary>
    public void ResetChip()
    {
        _hasBeenPlayed    = false;
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;
    }

    /// <summary>Valor de la ficha (para UI o debug).</summary>
    public int ChipValue => chipValue;
}