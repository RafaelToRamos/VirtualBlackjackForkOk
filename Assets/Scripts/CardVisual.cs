using UnityEngine;

// ============================================================
//  CardVisual.cs  —  CORREGIDO v2
//
//  CAMBIOS RESPECTO AL ORIGINAL:
//  1. isFaceUp MOVIDO aquí desde BlackjackCard.
//       Antes: card.isFaceUp era un campo de BlackjackCard
//             → mezclaba datos de lógica con presentación
//       Ahora: _isFaceUp vive en este componente
//             → CardVisual es el ÚNICO responsable de si
//               la carta se muestra cara arriba o abajo.
//
//  2. Setup() ahora recibe faceUp como parámetro:
//       visual.Setup(card, faceUp)
//
//  3. FlipUp() sin cambios funcionales.
//  4. FlipDown() agregado (útil para animaciones futuras).
// ============================================================

public class CardVisual : MonoBehaviour
{
    // Índices de materiales del prefab de carta
    // (ajustar si tu prefab usa un orden diferente)
    private const int FACE_SLOT = 0;
    private const int BACK_SLOT = 1;

    [Header("Textura del reverso")]
    public Texture2D cardBackTexture;

    private MeshRenderer  _meshRenderer;
    private Material[]    _materials;
    private BlackjackCard _cardData;

    // ── FIX: isFaceUp vive aquí, no en BlackjackCard ────────
    private bool _isFaceUp = true;

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer != null)
            _materials = _meshRenderer.materials; // clonar para no modificar shared assets
    }

    /// <summary>
    /// Configura la carta con sus datos y estado visual inicial.
    /// Llamado por CardLayoutManager al instanciar.
    /// </summary>
    public void Setup(BlackjackCard card, bool faceUp = true)
    {
        _cardData  = card;
        _isFaceUp  = faceUp;

        if (_isFaceUp)
            ShowFace();
        else
            ShowBack();
    }

    /// <summary>Voltea la carta cara arriba (revela el valor).</summary>
    public void FlipUp()
    {
        if (_cardData == null || _isFaceUp) return;
        _isFaceUp = true;

        // Animación de giro — versión básica sin animación
        // Para animación suave, ver FlipWithAnimation() más abajo
        transform.Rotate(0f, 180f, 0f, Space.Self);
        ShowFace();
    }

    /// <summary>Voltea la carta cara abajo (ocultar).</summary>
    public void FlipDown()
    {
        if (_cardData == null || !_isFaceUp) return;
        _isFaceUp = false;
        transform.Rotate(0f, 180f, 0f, Space.Self);
        ShowBack();
    }

    public bool IsFaceUp => _isFaceUp;

    // ── Privado ──────────────────────────────────────────────

    void ShowFace()
    {
        if (_cardData == null || _meshRenderer == null) return;

        string texName = _cardData.GetSpriteName();
        Texture2D tex = Resources.Load<Texture2D>($"Cards/{texName}");

        if (tex != null)
        {
            _materials[FACE_SLOT].mainTexture = tex;
            _meshRenderer.materials = _materials;
        }
        else
        {
            Debug.LogWarning($"[CardVisual] Textura no encontrada: Resources/Cards/{texName}");
        }
    }

    void ShowBack()
    {
        if (_meshRenderer == null) return;

        if (cardBackTexture != null)
        {
            _materials[BACK_SLOT].mainTexture = cardBackTexture;
            _meshRenderer.materials = _materials;
        }
        else
        {
            Debug.LogWarning("[CardVisual] No hay textura de reverso asignada.");
        }
    }
}