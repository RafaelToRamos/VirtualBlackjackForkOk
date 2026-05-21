using System.Collections.Generic;
using UnityEngine;

// ============================================================
//  CardLayoutManager.cs  —  CORREGIDO v2
//
//  CAMBIOS RESPECTO AL ORIGINAL:
//  1. PlaceCard() ahora recibe el parámetro faceUp explícitamente.
//       Antes: PlaceCard(card, isPlayer)
//             → card.isFaceUp decidía cómo mostrarse
//       Ahora: PlaceCard(card, isPlayer, faceUp)
//             → CardVisual recibe la instrucción directamente
//             → BlackjackCard ya no tiene campo isFaceUp
//
//  2. Posicionamiento de cartas con rotación para carta hoyo:
//       La carta boca abajo se rota 180° en Y para que se vea
//       el reverso en VR.
//
//  3. Sin otros cambios estructurales — la separación de
//     responsabilidades ya era correcta en el original.
// ============================================================

public class CardLayoutManager : MonoBehaviour
{
    [Header("Prefab de carta")]
    public GameObject cardPrefab;

    [Header("Posiciones de layout")]
    public Transform playerCardOrigin;
    public Transform dealerCardOrigin;

    [Header("Espaciado (metros en VR)")]
    public float cardSpacing = 0.08f;

    private List<GameObject> playerCardObjects = new List<GameObject>();
    private List<GameObject> dealerCardObjects = new List<GameObject>();

    /// <summary>
    /// Instancia y posiciona una carta en la mesa.
    /// faceUp controla si se muestra cara o reverso.
    /// </summary>
    public void PlaceCard(BlackjackCard card, bool isPlayer, bool faceUp = true)
    {
        GameObject cardObj = Instantiate(cardPrefab);

        // Asegurar collider para interacción VR
        if (cardObj.GetComponent<Collider>() == null)
        {
            BoxCollider col = cardObj.AddComponent<BoxCollider>();
            col.size = new Vector3(0.063f, 0.001f, 0.088f);
        }

        // Configurar visual — FIX: pasar faceUp explícitamente
        CardVisual visual = cardObj.GetComponent<CardVisual>();
        if (visual == null) visual = cardObj.AddComponent<CardVisual>();
        visual.Setup(card, faceUp); // FIX: CardVisual maneja isFaceUp

        // Posicionar en mesa
        Transform origin = isPlayer ? playerCardOrigin : dealerCardOrigin;
        List<GameObject> list = isPlayer ? playerCardObjects : dealerCardObjects;

        Vector3 pos = origin.position + origin.right * (list.Count * cardSpacing);
        cardObj.transform.position = pos;
        cardObj.transform.rotation = origin.rotation;

        // Carta boca abajo: rotar 180° en Y para mostrar reverso
        if (!faceUp)
        {
            cardObj.transform.Rotate(0f, 180f, 0f, Space.Self);
        }

        list.Add(cardObj);
    }

    /// <summary>Voltea todas las cartas del crupier (revela carta hoyo).</summary>
    public void FlipAllDealerCards()
    {
        foreach (var obj in dealerCardObjects)
            obj.GetComponent<CardVisual>()?.FlipUp();
    }

    /// <summary>Destruye todos los GameObjects de cartas en la mesa.</summary>
    public void ClearTable()
    {
        foreach (var obj in playerCardObjects) Destroy(obj);
        foreach (var obj in dealerCardObjects) Destroy(obj);
        playerCardObjects.Clear();
        dealerCardObjects.Clear();
    }
}