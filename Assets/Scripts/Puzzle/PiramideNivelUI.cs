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
    public int nivelCorreto;
    public string expectedPieceItemID;
    public string placeholderText = "?";

    [Header("Visual")]
    public Button nivelButton;
    public TextMeshProUGUI nivelText;
    public Image nivelBackground;
    public Image pecaIcon;

    [Header("Sprites de Estado")]
    public Sprite spriteVazio;      // SlotVazio   — estado normal
    public Sprite spriteCorreto;    // slotCorreto — quando preenchido corretamente

    [Header("Cores (usadas quando sprite é nulo)")]
    public Color defaultColor = new Color(0.35f, 0.25f, 0.08f, 0.7f);
    public Color selectedColor = new Color(0.6f, 0.5f, 0.1f, 0.8f);
    public Color correctColor = new Color(1f, 0.85f, 0.3f, 0.9f);
    public Color wrongColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
    public Color completionColor = new Color(1f, 0.95f, 0.5f, 1f);

    // ---- Estado ----
    private bool isFilled = false;
    public bool IsFilled => isFilled;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────
    private void Start()
    {
        AplicarSpriteVazio();
        if (nivelText != null) nivelText.text = placeholderText;
        if (pecaIcon != null) pecaIcon.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (nivelButton != null)
            nivelButton.onClick.AddListener(OnNivelClicked);
    }

    private void OnDisable()
    {
        if (nivelButton != null)
            nivelButton.onClick.RemoveListener(OnNivelClicked);
    }

    // ── Clique no nível ───────────────────────────────────────────────────────
    private void OnNivelClicked()
    {
        Debug.Log($"[NivelBtn] Clicado! isFilled={isFilled} | SocialUI={SocialUI.Instance != null}");
        if (isFilled) return;
        SocialUI.Instance?.OnNivelClicked(this);
    }

    // ── Highlight de seleção ──────────────────────────────────────────────────
    public void SetSelectionHighlight(bool selected)
    {
        if (nivelBackground == null) return;

        if (selected)
        {
            // Tinge o sprite vazio de dourado para indicar seleção
            nivelBackground.color = selectedColor;
        }
        else
        {
            AplicarSpriteVazio();
        }
    }

    // ── Colocar peça no nível ────────────────────────────────────────────────
    public bool TryPlacePiece(GlyphItem item)
    {
        if (isFilled) return false;
        Debug.Log($"[Nivel {nivelCorreto}] esperado='{expectedPieceItemID}' | recebido='{item.itemID}'");

        if (item.itemID == expectedPieceItemID)
        {
            isFilled = true;

            // Troca para sprite correto (dourado)
            AplicarSpriteCorreto();

            // Desativa botão e esconde sua imagem para o pecaIcon aparecer limpo
            if (nivelButton != null)
            {
                nivelButton.interactable = false;
                var btnImg = nivelButton.GetComponent<Image>();
                if (btnImg != null) btnImg.enabled = false;
            }

            // Exibe ícone do personagem
            if (pecaIcon != null && item.itemSprite != null)
            {
                pecaIcon.sprite = item.itemSprite;
                pecaIcon.color = Color.white;
                pecaIcon.gameObject.SetActive(true);
                pecaIcon.transform.SetAsLastSibling();
                if (nivelText != null) nivelText.gameObject.SetActive(false);
            }
            else
            {
                if (nivelText != null) nivelText.text = item.displayName;
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
        // Mantém o sprite vazio mas tinge de vermelho
        if (nivelBackground != null) nivelBackground.color = wrongColor;
        yield return new WaitForSecondsRealtime(1f);
        AplicarSpriteVazio();
    }

    // ── Restaura estado ao reentrar na cena ───────────────────────────────────
    public void RestoreFilled(string pieceDisplayName)
    {
        isFilled = true;
        AplicarSpriteCorreto();
        if (nivelText != null) nivelText.text = pieceDisplayName;
        if (nivelButton != null) nivelButton.interactable = false;
    }

    // ── Efeito de conclusão do puzzle ─────────────────────────────────────────
    public void PlayCompletionEffect()
    {
        // Pulsa dourado brilhante sobre o sprite correto
        if (nivelBackground != null) nivelBackground.color = completionColor;
    }

    // ── Helpers de sprite ─────────────────────────────────────────────────────
    private void AplicarSpriteVazio()
    {
        if (nivelBackground == null) return;
        if (spriteVazio != null)
        {
            nivelBackground.sprite = spriteVazio;
            nivelBackground.color = Color.white; // sem tint — mostra sprite original
        }
        else
        {
            nivelBackground.color = defaultColor; // fallback sem sprite
        }
    }

    private void AplicarSpriteCorreto()
    {
        if (nivelBackground == null) return;
        if (spriteCorreto != null)
        {
            nivelBackground.sprite = spriteCorreto;
            nivelBackground.color = Color.white;
        }
        else
        {
            nivelBackground.color = correctColor; // fallback sem sprite
        }
    }
}