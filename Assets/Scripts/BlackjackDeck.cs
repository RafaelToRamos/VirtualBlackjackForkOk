using System.Collections.Generic;
using UnityEngine;

// ============================================================
//  BlackjackDeck.cs  —  CORREGIDO v2
//  Cambios respecto al original:
//   • DrawCard() ahora extrae desde el FINAL de la lista (O(1))
//     en vez del inicio (O(n)) — sin impacto en aleatoriedad.
//   • Agregado umbral de remezcla (penetración de mazo).
//     Con 6 mazos (312 cartas), remezclar cuando quedan < 78
//     simula el corte estándar de casino (25% restante).
//   • Evento OnDeckLow para que el GameManager pueda reaccionar
//     visualmente (animación de remezcla, etc.).
// ============================================================

public class BlackjackDeck : MonoBehaviour
{
    private List<BlackjackCard> cards = new List<BlackjackCard>();

    // ── Evento opcional: notifica cuando el mazo está bajo ──
    public event System.Action OnDeckLow;

    // Penetración de mazo: remezclar cuando queda este % de cartas
    [Range(0.1f, 0.5f)]
    public float reshuffleThreshold = 0.25f; // 25% restante = remezclar

    private int _totalCards = 0;

    public int CardsRemaining => cards.Count;

    public void Initialize(int numDecks = 1)
    {
        cards.Clear();

        foreach (BlackjackCard.Suit s in System.Enum.GetValues(typeof(BlackjackCard.Suit)))
            foreach (BlackjackCard.Rank r in System.Enum.GetValues(typeof(BlackjackCard.Rank)))
                for (int d = 0; d < numDecks; d++)
                    cards.Add(new BlackjackCard(s, r));

        _totalCards = cards.Count;
        Shuffle();
    }

    public void Shuffle()
    {
        // Fisher-Yates — sin cambios, ya era correcto
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }

    public BlackjackCard DrawCard()
    {
        // Auto-reiniciar si está completamente vacío
        if (cards.Count == 0)
        {
            Debug.LogWarning("[Deck] Mazo vacío — reiniciando.");
            Initialize(_totalCards / 52); // mantener misma cantidad de mazos
        }

        // ── FIX: extraer desde el final (O(1)) en vez del inicio (O(n)) ──
        // Original: cards[0] + RemoveAt(0) — desplaza toda la lista
        // Correcto: cards[last] + RemoveAt(last) — sin desplazamiento
        int last = cards.Count - 1;
        BlackjackCard card = cards[last];
        cards.RemoveAt(last);

        // Verificar umbral de remezcla
        if (cards.Count <= _totalCards * reshuffleThreshold)
        {
            OnDeckLow?.Invoke();
        }

        return card;
    }

    /// <summary>
    /// Fuerza una reinicialización completa (por ejemplo, al inicio de sesión).
    /// </summary>
    public void Reset(int numDecks = 6)
    {
        Initialize(numDecks);
    }
}