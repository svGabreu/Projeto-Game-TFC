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

        filledItemID = item.itemID;
        filledItem   = item;   // guarda referência para devolver se errado

        // Exibe o sprite do item no slot
        if (filledItemImage != null)
        {
            filledItemImage.sprite = item.itemSprite;
            filledItemImage.color  = item.itemSprite != null ? Color.white : Color.gray;
            filledItemImage.gameObject.SetActive(true);
        }

        // Remove do inventário
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
    // --------------------------------------------------------
    private void TryValidate()
    {
        if (string.IsNullOrEmpty(filledItemID) ||
            string.IsNullOrEmpty(chosenLetter)) return;

        bool silhuetaCorreta = filledItemID == expectedItemID;
        bool letraCorreta    = chosenLetter  == expectedLetter;

        if (silhuetaCorreta && letraCorreta)
        {
            isCompleted = true;
            SetBackground(correctColor);
            Debug.Log($"Par '{pairID}' correto!");
            OnPairCompleted.Invoke(pairID);
        }
        else
        {
            SetBackground(incorrectColor);

            // Devolve item se silhueta errada
            if (!silhuetaCorreta && !string.IsNullOrEmpty(filledItemID))
            {
                // ✅ usa referência direta — não depende de Resources.Load
                if (filledItem != null)
                    InventoryManager.Instance.AddItem(filledItem);

                filledItemID = "";
                filledItem   = null;
                if (filledItemImage != null)
                    filledItemImage.gameObject.SetActive(false);
            }

            // Penalidade na letra se estava errada
            if (!letraCorreta && !string.IsNullOrEmpty(chosenLetter))
                StartCoroutine(LockLetter());
        }
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
