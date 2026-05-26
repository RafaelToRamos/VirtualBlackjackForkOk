using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Agrega este script + XRSimpleInteractable al mismo GO del botón 3D.
// En el Inspector: arrastra GameManager al campo correspondiente.
[RequireComponent(typeof(XRSimpleInteractable))]
public class VRButtonBridge : MonoBehaviour
{
    public enum ActionType { Hit, Stand, Double, Bet10, Bet25, Bet50, Bet100 }

    [Header("Configuración")]
    public ActionType action;

    [Header("Referencias")]
    [SerializeField] private BlackjackGameManager gameManager;

    private XRSimpleInteractable _interactable;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _interactable.selectEntered.AddListener(OnActivated);
    }

    void OnDestroy()
    {
        _interactable.selectEntered.RemoveListener(OnActivated);
    }

    private void OnActivated(SelectEnterEventArgs args)
    {
        switch (action)
        {
            case ActionType.Hit:    gameManager.PlayerHit();    break;
            case ActionType.Stand:  gameManager.PlayerStand();  break;
            case ActionType.Double: gameManager.PlayerDoubleDown(); break;
            case ActionType.Bet10:  gameManager.PlaceBet(10);   break;
            case ActionType.Bet25:  gameManager.PlaceBet(25);   break;
            case ActionType.Bet50:  gameManager.PlaceBet(50);   break;
            case ActionType.Bet100: gameManager.PlaceBet(100);  break;
        }
    }
}