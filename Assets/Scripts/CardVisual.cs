using UnityEngine;

public class CardVisual : MonoBehaviour
{
    private const int FACE_SLOT = 0;

    [Header("Card Back Texture")]
    public Texture2D cardBackTexture;

    private MeshRenderer  _meshRenderer;
    private Material[]    _materials;
    private BlackjackCard _cardData;
    private bool          _isFaceUp = true;

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer != null)
            _materials = _meshRenderer.materials;
    }

    public void Setup(BlackjackCard card, bool faceUp = true)
    {
        _cardData = card;
        _isFaceUp = faceUp;

        if (_isFaceUp)
            ShowFace();
        else
            ShowBack();
    }

    /// <summary>
    /// En lugar de rotar el GameObject, simplemente
    /// cambia la textura del slot 0 por la de la carta real.
    /// </summary>
    public void FlipUp()
    {
        if (_cardData == null || _isFaceUp) return;
        _isFaceUp = true;
        ShowFace(); // solo swap de textura, sin rotación
    }

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
            _materials[FACE_SLOT].mainTexture = cardBackTexture; // mismo slot, distinta textura
            _meshRenderer.materials = _materials;
        }
        else
        {
            Debug.LogWarning("[CardVisual] No hay textura de reverso asignada en el prefab.");
        }
    }
}