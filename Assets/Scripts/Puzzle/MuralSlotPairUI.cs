// MuralSlotPairUI.cs
// Coloque em: Assets/Scripts/Puzzle/
// Substitui o MuralSlotPair antigo para trabalhar com o novo sistema de UI.
// Um GameObject com este script representa UM par (silhueta + letra) na tela do mural.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class MuralSlotPairUI : MonoBehaviour
{
    [Header("Configuração do Par")]
    public string expectedItemID;
    public string expectedLetter;
    public string pairID;

    [Header("Referências de UI")]
    public Image silhouetteImage;
    public Image filledItemImage;
    public Button silhouetteButton;
    public TextMeshProUGUI letterText;
    public Button letterButton;
    public Image backgroundImage;

    [Header("Referência ao MuralUI pai")]
    public MuralUI muralUI;

    [Header("Cores de feedback")]
    public Color correctColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    public Color incorrectColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
    public Color defaultColor = new Color(1f, 1f, 1f, 0.1f);

    // Estado interno
    private string filledItemID = "";
    private GlyphItem filledItem = null;
    private string chosenLetter = "";
    private bool isLocked = false;
    private bool isCompleted = false;

    public UnityEvent<string> OnPairCompleted = new UnityEvent<string>();

    private void Start()
    {
        if (silhouetteButton != null)
            silhouetteButton.onClick.AddListener(OnSilhouetteClicked);

        if (letterButton != null)
            letterButton.onClick.AddListener(OnLetterClicked);

        if (filledItemImage != null)
            filledItemImage.gameObject.SetActive(false);

        SetBackground(defaultColor);
    }

    // ── Interação normal ──────────────────────────────────────────────────────
    private void OnSilhouetteClicked()
    {
        if (isCompleted || isLocked) return;

        if (muralUI != null && muralUI.IsLetterSelectorOpen())
        {
            muralUI.ShowMiniInventory();
            return;
        }

        if (!string.IsNullOrEmpty(filledItemID)) return;

        if (muralUI != null)
            muralUI.TryPlaceItemInSlot(this);
    }

    private void OnLetterClicked()
    {
        if (isCompleted || isLocked) return;

        if (muralUI != null)
            muralUI.OpenLetterSelectorFor(this);
    }

    public void TryFillSilhouette(GlyphItem item)
    {
        if (item.itemType != GlyphItemType.GlyphObject) return;

        if (item.itemID != expectedItemID)
        {
            SetBackground(incorrectColor);
            StartCoroutine(ResetBackgroundAfterDelay());
            return;
        }

        filledItemID = item.itemID;
        filledItem = item;

        if (filledItemImage != null)
        {
            filledItemImage.sprite = item.itemSprite;
            filledItemImage.color = item.itemSprite != null ? Color.white : Color.gray;
            filledItemImage.gameObject.SetActive(true);
        }

        InventoryManager.Instance.RemoveItem(item.itemID);
        TryValidate();
    }

    public void SetChosenLetter(string letter)
    {
        if (isLocked || isCompleted) return;

        chosenLetter = letter;
        if (letterText != null) letterText.text = letter;

        TryValidate();
    }

    private void TryValidate()
    {
        if (string.IsNullOrEmpty(filledItemID) ||
            string.IsNullOrEmpty(chosenLetter)) return;

        bool letraCorreta = chosenLetter == expectedLetter;

        if (letraCorreta)
        {
            MarkCompleted();
        }
        else
        {
            SetBackground(incorrectColor);
            StartCoroutine(LockLetter());
        }
    }

    // ── Marcar como concluído (fluxo normal) ──────────────────────────────────
    private void MarkCompleted()
    {
        isCompleted = true;
        SetBackground(correctColor);
        Debug.Log($"Par '{pairID}' correto!");
        OnPairCompleted.Invoke(pairID);
    }

    // ── NOVO: Restaurar estado concluído ao reentrar na cena ──────────────────
    // Chamado pelo MuralPuzzle.RestoreState() — não consome itens do inventário
    public void RestoreCompleted()
    {
        isCompleted = true;

        // Desabilita botões — par já resolvido
        if (silhouetteButton != null) silhouetteButton.interactable = false;
        if (letterButton != null) letterButton.interactable = false;

        // Atualiza visual: fundo verde e letra correta
        SetBackground(correctColor);
        if (letterText != null) letterText.text = expectedLetter;

        // Mostra imagem preenchida com sprite placeholder (sem referência ao GlyphItem)
        // Se quiser mostrar o ícone correto, salve o itemID e busque via Resources
        if (filledItemImage != null)
        {
            filledItemImage.gameObject.SetActive(true);
            // Tenta buscar sprite via InventoryManager/Resources se disponível
            // Por ora mantém cinza como placeholder — funcionalidade preservada
            filledItemImage.color = Color.gray;
        }

        Debug.Log($"[MuralSlotPairUI] Par '{pairID}' restaurado como concluído.");
    }

    // ── Coroutines de feedback ────────────────────────────────────────────────
    private IEnumerator ResetBackgroundAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        SetBackground(defaultColor);
    }

    private IEnumerator LockLetter()
    {
        isLocked = true;
        chosenLetter = "";
        if (letterText != null) letterText.text = "...";
        if (letterButton != null) letterButton.interactable = false;

        yield return new WaitForSecondsRealtime(5f);

        isLocked = false;
        if (letterText != null) letterText.text = "-";
        if (letterButton != null) letterButton.interactable = true;
        SetBackground(defaultColor);
    }

    private void SetBackground(Color color)
    {
        if (backgroundImage != null) backgroundImage.color = color;
    }

    public bool IsCompleted() => isCompleted;
}