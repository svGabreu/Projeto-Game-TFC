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
    public string expectedItemID;    // ID do GlyphItem correto
    public string expectedLetter;    // letra correta (ex: "N")
    public string pairID;

    [Header("Referências de UI")]
    public Image silhouetteImage;       // imagem da silhueta (cinza)
    public Image filledItemImage;       // imagem do item encaixado (começa oculta)
    public Button silhouetteButton;     // botão sobre a silhueta
    public TextMeshProUGUI letterText;  // exibe letra escolhida
    public Button letterButton;         // abre seletor de letras
    public Image backgroundImage;       // fundo do par (muda de cor ao validar)

    [Header("Referência ao MuralUI pai")]
    public MuralUI muralUI;

    [Header("Cores de feedback")]
    public Color correctColor   = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    public Color incorrectColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
    public Color defaultColor   = new Color(1f,   1f,   1f,   0.1f);

    // Estado interno
    private string    filledItemID  = "";
    private GlyphItem filledItem    = null;   // referência direta ao item encaixado
    private string    chosenLetter  = "";
    private bool      isLocked      = false;
    private bool      isCompleted   = false;

    // Evento disparado para o MuralPuzzle quando o par é concluído
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

    // --------------------------------------------------------
    // Jogador clica no slot de silhueta
    // --------------------------------------------------------
    private void OnSilhouetteClicked()
    {
        if (isCompleted || isLocked) return;
        if (!string.IsNullOrEmpty(filledItemID)) return; // slot já ocupado

        if (muralUI != null)
            muralUI.TryPlaceItemInSlot(this);
    }

    // --------------------------------------------------------
    // Jogador clica no slot de letra
    // --------------------------------------------------------
    private void OnLetterClicked()
    {
        if (isCompleted || isLocked) return;

        if (muralUI != null)
            muralUI.OpenLetterSelectorFor(this);
    }

    // --------------------------------------------------------
    // Chamado pelo MuralUI após o jogador selecionar um item
    // --------------------------------------------------------
    public void TryFillSilhouette(GlyphItem item)
    {
        if (item.itemType != GlyphItemType.GlyphObject) return;

        // Verifica silhueta antes de remover do inventário
        if (item.itemID != expectedItemID)
        {
            SetBackground(incorrectColor);
            StartCoroutine(ResetBackgroundAfterDelay());
            return;
        }

        filledItemID = item.itemID;
        filledItem   = item;

        if (filledItemImage != null)
        {
            filledItemImage.sprite = item.itemSprite;
            filledItemImage.color  = item.itemSprite != null ? Color.white : Color.gray;
            filledItemImage.gameObject.SetActive(true);
        }

        InventoryManager.Instance.RemoveItem(item.itemID);
        TryValidate();
    }

    // --------------------------------------------------------
    // Chamado pelo LetterSelectorUI quando uma letra é escolhida
    // --------------------------------------------------------
    public void SetChosenLetter(string letter)
    {
        if (isLocked || isCompleted) return;

        chosenLetter = letter;
        if (letterText != null) letterText.text = letter;

        TryValidate();
    }

    // --------------------------------------------------------
    // Valida se os dois slots estão corretos
    // (silhueta já garantida correta ao chegar aqui)
    // --------------------------------------------------------
    private void TryValidate()
    {
        if (string.IsNullOrEmpty(filledItemID) ||
            string.IsNullOrEmpty(chosenLetter)) return;

        bool letraCorreta = chosenLetter == expectedLetter;

        if (letraCorreta)
        {
            isCompleted = true;
            SetBackground(correctColor);
            Debug.Log($"Par '{pairID}' correto!");
            OnPairCompleted.Invoke(pairID);
        }
        else
        {
            SetBackground(incorrectColor);
            StartCoroutine(LockLetter());
        }
    }

    private IEnumerator ResetBackgroundAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        SetBackground(defaultColor);
    }

    private IEnumerator LockLetter()
    {
        isLocked = true;
        chosenLetter = "";
        if (letterText   != null) letterText.text = "...";
        if (letterButton != null) letterButton.interactable = false;

        // Usa WaitForSecondsRealtime porque o jogo pode estar pausado (timeScale=0)
        yield return new WaitForSecondsRealtime(5f);

        isLocked = false;
        if (letterText   != null) letterText.text = "-";
        if (letterButton != null) letterButton.interactable = true;
        SetBackground(defaultColor);
    }

    private void SetBackground(Color color)
    {
        if (backgroundImage != null) backgroundImage.color = color;
    }

    public bool IsCompleted() => isCompleted;
}
