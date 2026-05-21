using UnityEngine;

// ============================================================
//  BlackjackCard.cs  —  CORREGIDO v2
//  Cambios respecto al original:
//   • isFaceUp eliminado de aquí — se movió a CardVisual.cs
//     (era un dato de presentación, no de lógica de juego)
//   • GetSpriteName() sin cambios
// ============================================================

[System.Serializable]
public class BlackjackCard
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank
    {
        Ace = 1, Two = 2, Three = 3, Four = 4, Five = 5,
        Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10,
        Jack = 11, Queen = 12, King = 13
    }

    public Suit suit;
    public Rank rank;

    // ── isFaceUp ELIMINADO de aquí ──────────────────────────
    // Antes: public bool isFaceUp = true;
    // Ahora vive en CardVisual.cs como estado puramente visual.
    // GetValue() siempre cuenta TODAS las cartas sin importar
    // si están visibles o no.
    // ────────────────────────────────────────────────────────

    public BlackjackCard(Suit s, Rank r)
    {
        suit = s;
        rank = r;
    }

    /// <summary>
    /// Valor base para cálculo de puntaje.
    /// As = 11 por defecto; BlackjackHand lo reduce a 1 si hay bust.
    /// Figuras (J, Q, K) = 10.
    /// </summary>
    public int BaseValue()
    {
        if (rank == Rank.Ace) return 11;
        if ((int)rank >= 10) return 10;
        return (int)rank;
    }

    /// <summary>Nombre del asset de textura: "ace_of_spades", etc.</summary>
    public string GetSpriteName() => $"{rank}_of_{suit}".ToLower();

    public override string ToString() => $"{rank} of {suit}";
}