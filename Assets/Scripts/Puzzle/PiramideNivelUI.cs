// PiramideNivelUI.cs
// Coloque em: Assets/Scripts/Puzzle/
// Representa UM nível da Pirâmide Social — Etapa 2 do Puzzle 3.
// Jogador seleciona uma peça no mini inventário → clica neste nível → valida.
// Hierarquia: 1=Faraó (topo) … 5=Camponeses (base).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PiramideNivelUI : MonoBehaviour
{
    [Header("Configuração")]
    public int nivelCorreto;                // 1 = topo (Faraó) → 5 = base (Camponeses)
    public string expectedPieceItemID;      // "piece_farao" | "piece_sacerdotes" | etc.
    public string placeholderText = "?";   // texto exibido enquanto o nível está vazio

    [Header("Visual")]
    public Button nivelButton;
    public TextMeshProUGUI nivelText;
    public Image nivelBackground;
    public Image pecaIcon;                  // ícone da peça colocada (opcional)

    [Header("Cores")]
    public Color defaultColor    = new Color(0.35f, 0.25f, 0.08f, 0.7f);  // marrom escuro
    public Color selectedColor   = new Color(0.6f,  0.5f,  0.1f,  0.8f);  // dourado médio
    public Color correctColor    = new Color(1f,    0.85f, 0.3f,  0.9f);  // dourado vivo
    public Color wrongColor      = new Color(0.8f,  0.2f,  0.2f,  0.8f);  // vermelho
    public Color completionColor = new Color(1f,    0.95f, 0.5f,  1f);    // dourado brilhante

    // ---- Estado ----
    private bool isFilled = false;
    public bool IsFilled => isFilled;

    // --------------------------------------------------------
    private void Start()
    {
        if (nivelButton     != null) nivelButton.onClick.AddListener(OnNivelClicked);
        if (nivelBackground != null) nivelBackground.color = defaultColor;
        if (nivelText       != null) nivelText.text        = placeholderText;
        if (pecaIcon        != null) pecaIcon.gameObject.SetActive(false);
    }

    // --------------------------------------------------------
    private void OnNivelClicked()
    {
        if (isFilled) return;
        SocialUI.Instance?.OnNivelClicked(this);
    }

    // --------------------------------------------------------
    // Feedback de seleção (nível realçado enquanto jogador escolhe item)
    // --------------------------------------------------------
    public void SetSelectionHighlight(bool selected)
    {
        if (nivelBackground != null)
            nivelBackground.color = selected ? selectedColor : defaultColor;
    }

    // --------------------------------------------------------
    // Chamado pelo SocialPuzzle.TryPlacePiece()
    // --------------------------------------------------------
    public bool TryPlacePiece(GlyphItem item)
    {

        if (isFilled) return false;
        Debug.Log($"[Nivel {nivelCorreto}] esperado='{expectedPieceItemID}' | recebido='{item.itemID}'");

        if (item.itemID == expectedPieceItemID)
        {
            isFilled = true;
            if (nivelBackground != null) nivelBackground.color      = correctColor;
            if (nivelText       != null) nivelText.text             = item.displayName;
            if (nivelButton     != null) nivelButton.interactable   = false;

            if (pecaIcon != null && item.itemSprite != null)
            {
                pecaIcon.sprite = item.itemSprite;
                pecaIcon.gameObject.SetActive(true);
            }

            InventoryManager.Instance.RemoveItem(item.itemID);
            return true;
        }
        else
        {
            StartCoroutine(FlashWrong());
            return false;
        }
    }

    private IEnumerator FlashWrong()
    {
        if (nivelBackground != null) nivelBackground.color = wrongColor;
        yield return new WaitForSecondsRealtime(1f);
        if (nivelBackground != null) nivelBackground.color = defaultColor;
    }

    // --------------------------------------------------------
    // Restaura estado ao reentrar na cena
    // --------------------------------------------------------
    public void RestoreFilled(string pieceDisplayName)
    {
        isFilled = true;
        if (nivelBackground != null) nivelBackground.color    = correctColor;
        if (nivelText       != null) nivelText.text           = pieceDisplayName;
        if (nivelButton     != null) nivelButton.interactable = false;
    }

    // --------------------------------------------------------
    // Efeito de conclusão do puzzle
    // --------------------------------------------------------
    public void PlayCompletionEffect()
    {
        if (nivelBackground != null) nivelBackground.color = completionColor;
    }
}
