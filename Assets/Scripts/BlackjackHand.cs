using System.Collections.Generic;

// ============================================================
//  BlackjackHand.cs  —  CORREGIDO v2
//  Cambios respecto al original:
//   • GetValue() — eliminado "if (!card.isFaceUp) continue"
//     El puntaje real siempre se calcula con todas las cartas.
//     La visibilidad es responsabilidad de CardVisual.cs.
//   • IsSoft() — corregido para no depender de isFaceUp tampoco.
//   • IsBlackjack() — sin cambios (correcto).
//   • IsBust() — sin cambios (correcto).
// ============================================================

public class BlackjackHand
{
    public List<BlackjackCard> Cards { get; private set; } = new List<BlackjackCard>();

    public void AddCard(BlackjackCard card) => Cards.Add(card);
    public void Clear() => Cards.Clear();

    /// <summary>
    /// Calcula el puntaje óptimo de la mano.
    /// SIEMPRE cuenta todas las cartas — la visibilidad (cara arriba/abajo)
    /// es un dato de presentación y no afecta el valor real de la mano.
    ///
    /// Algoritmo del As flexible:
    ///   1. Suma todos los BaseValue() (As = 11).
    ///   2. Por cada As, si el total > 21, réstale 10 (As pasa a 1).
    /// </summary>
    public int GetValue()
    {
        int total = 0;
        int aces  = 0;

        foreach (var card in Cards)
        {
            // ── FIX PRINCIPAL ──────────────────────────────
            // ELIMINADO: if (!card.isFaceUp) continue;
            // Razón: isFaceUp es estado visual, no lógico.
            // El crupier necesita conocer su puntaje real para
            // decidir si pide carta, aunque una esté boca abajo.
            // ───────────────────────────────────────────────
            int val = card.BaseValue();
            if (card.rank == BlackjackCard.Rank.Ace) aces++;
            total += val;
        }

        // Reducir Ases de 11 a 1 mientras haya bust
        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        return total;
    }

    /// <summary>El puntaje supera 21.</summary>
    public bool IsBust() => GetValue() > 21;

    /// <summary>
    /// Blackjack natural: exactamente 2 cartas que suman 21.
    /// </summary>
    public bool IsBlackjack() => Cards.Count == 2 && GetValue() == 21;

    /// <summary>
    /// Mano "blanda" (soft): contiene un As que aún cuenta como 11.
    /// Usado por el crupier para verificar soft 17.
    /// Corregido: ya no depende de isFaceUp.
    /// </summary>
    public bool IsSoft()
    {
        int total = 0;
        int aces  = 0;

        foreach (var card in Cards)
        {
            total += card.BaseValue();
            if (card.rank == BlackjackCard.Rank.Ace) aces++;
        }

        // Reducir Ases igual que GetValue para encontrar el estado real
        int remainingAces = aces;
        while (total > 21 && remainingAces > 0)
        {
            total -= 10;
            remainingAces--;
        }

        // La mano es soft si quedó algún As contado como 11
        return remainingAces > 0 && total <= 21;
    }

    public override string ToString()
    {
        string cards = string.Join(", ", Cards);
        return $"[{cards}] = {GetValue()}{(IsSoft() ? " soft" : "")}{(IsBust() ? " BUST" : "")}{(IsBlackjack() ? " BJ" : "")}";
    }
}