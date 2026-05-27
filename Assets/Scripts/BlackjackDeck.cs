using System.Collections.Generic;
using UnityEngine;

public class BlackjackDeck : MonoBehaviour
{
    private List<BlackjackCard> cards = new List<BlackjackCard>();

    // ── NUEVO: evento para notificar mazo bajo ───────────────
    public event System.Action OnDeckLow;
    private int _totalCards = 0;
    [Range(0.1f, 0.5f)]
    public float reshuffleThreshold = 0.25f;
    // ────────────────────────────────────────────────────────

    public void Initialize(int numDecks = 1)
    {
        cards.Clear();
        for (int d = 0; d < numDecks; d++)
            foreach (BlackjackCard.Suit s in System.Enum.GetValues(typeof(BlackjackCard.Suit)))
                foreach (BlackjackCard.Rank r in System.Enum.GetValues(typeof(BlackjackCard.Rank)))
                    cards.Add(new BlackjackCard(s, r));

        _totalCards = cards.Count; // NUEVO: guardar total
        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }

    public BlackjackCard DrawCard()
    {
        if (cards.Count == 0)
        {
            Debug.LogWarning("Deck empty — reshuffling!");
            Initialize(_totalCards / 52);
        }

        // NUEVO: extraer desde el final (O(1) en vez de O(n))
        int last = cards.Count - 1;
        BlackjackCard card = cards[last];
        cards.RemoveAt(last);

        // NUEVO: verificar umbral y emitir evento
        if (_totalCards > 0 && cards.Count <= _totalCards * reshuffleThreshold)
            OnDeckLow?.Invoke();

        return card;
    }

    public int CardsRemaining => cards.Count;
}