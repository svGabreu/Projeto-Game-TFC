// RioDaVidaQuadroUI.cs
// Coloque em: Assets/Scripts/Puzzle/
// Representa UM quadro (pintura da estação) + o slot de pergaminho abaixo dele.
// Etapa 1: recebe um pergaminho do inventário e valida.
// Etapa 2: pode ser selecionado e trocado de posição.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RioDaVidaQuadroUI : MonoBehaviour
{
    [Header("Configuração")]
    public int correctPosition;       // 1 = Akhet, 2 = Peret, 3 = Shemu
    public string expectedScrollID;   // "scroll_akhet" | "scroll_peret" | "scroll_shemu"
    public string seasonName;         // "Akhet" | "Peret" | "Shemu" — exibido no slot antes da coleta

    [Header("Visual do Quadro")]
    public Image paintingImage;           // imagem da estação
    public Image borderImage;             // borda que acende dourada ao nomear corretamente
    public Image selectionHighlight;      // destaque ao selecionar na Etapa 2
    public Button quadroButton;           // botão para clicar na Etapa 2

    [Header("Slot do Pergaminho (Etapa 1)")]
    public Button slotButton;             // botão para atribuir pergaminho
    public TextMeshProUGUI slotLabel;     // exibe o nome do pergaminho atribuído
    public Image slotBackground;          // fundo do slot (muda de cor)

    [Header("Cores de Feedback")]
    public Color correctColor   = new Color(0.2f, 0.8f, 0.2f, 0.8f);
    public Color wrongColor     = new Color(0.8f, 0.2f, 0.2f, 0.8f);
    public Color defaultColor   = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    public Color namedBorder    = new Color(1f,   0.85f, 0.3f, 0.4f); // dourado semitransparente (não cobre a pintura)

    // ---- Estado interno ----
    private bool isNamed = false;

    public bool IsNamed => isNamed;
    public int  CorrectPosition => correctPosition;

    // --------------------------------------------------------
    private void Start()
    {
        if (slotButton  != null) slotButton.onClick.AddListener(OnSlotClicked);
        if (quadroButton != null) quadroButton.onClick.AddListener(OnQuadroClicked);

        SetSelectionHighlight(false);
        if (borderImage    != null) borderImage.color    = Color.clear;
        if (slotBackground != null) slotBackground.color = defaultColor;

        // Mostra o nome da estação como placeholder; fallback genérico
        if (slotLabel != null)
            slotLabel.text = string.IsNullOrEmpty(seasonName)
                ? "— Arraste um pergaminho —"
                : $"— {seasonName} —";

        // Apaga o texto padrão "Button" do filho do SlotButton (criado automaticamente pelo Unity)
        if (slotButton != null)
        {
            var builtinText = slotButton.GetComponentInChildren<TMP_Text>();
            if (builtinText != null && builtinText != slotLabel)
                builtinText.text = "";
        }

        // Quadro não é clicável na Etapa 1
        if (quadroButton != null) quadroButton.interactable = false;
    }

    // --------------------------------------------------------
    // Slot clicado → pede ao RioDaVidaUI para processar
    // --------------------------------------------------------
    private void OnSlotClicked()
    {
        if (isNamed) return;
        RioDaVidaUI.Instance?.OnSlotClicked(this);
    }

    // --------------------------------------------------------
    // Quadro clicado (Etapa 2) → pede ao RioDaVidaUI para processar
    // --------------------------------------------------------
    private void OnQuadroClicked()
    {
        RioDaVidaUI.Instance?.OnQuadroClicked(this);
    }

    // --------------------------------------------------------
    // Tenta atribuir pergaminho (chamado pelo RioDaVidaPuzzle)
    // Retorna true se correto, false se errado
    // --------------------------------------------------------
    public bool TryAssignScroll(GlyphItem item)
    {
        if (isNamed) return false;

        if (item.itemID == expectedScrollID)
        {
            isNamed = true;

            // Feedback visual de acerto
            if (slotLabel      != null) slotLabel.text       = item.displayName;
            if (slotBackground != null) slotBackground.color = correctColor;
            if (slotButton     != null) slotButton.interactable = false;
            // borderImage fica transparente (não cobre a pintura)

            InventoryManager.Instance.RemoveItem(item.itemID);
            return true;
        }
        else
        {
            // Feedback visual de erro
            StartCoroutine(FlashWrong());
            return false;
        }
    }

    private IEnumerator FlashWrong()
    {
        if (slotBackground != null) slotBackground.color = wrongColor;
        yield return new WaitForSecondsRealtime(1f);
        if (slotBackground != null) slotBackground.color = defaultColor;
    }

    // --------------------------------------------------------
    // Restaura o estado "nomeado" ao reentrar na cena (sem remover do inventário)
    // --------------------------------------------------------
    public void RestoreNamed(string scrollDisplayName)
    {
        isNamed = true;
        if (slotLabel      != null) slotLabel.text          = scrollDisplayName;
        if (slotBackground != null) slotBackground.color    = correctColor;
        if (slotButton     != null) slotButton.interactable = false;
        // borderImage fica transparente
    }

    // --------------------------------------------------------
    // Ativa modo Etapa 2: quadro vira clicável, label do nome permanece visível
    // --------------------------------------------------------
    public void EnableEtapa2()
    {
        if (quadroButton != null) quadroButton.interactable = true;

        // Esconde apenas o SlotButton (era ele que bloqueava raycasts com interactable=false)
        if (slotButton != null)
            slotButton.gameObject.SetActive(false);

        // Desativa raycast no fundo e no label — eles não precisam capturar eventos na Etapa 2
        if (slotBackground != null) slotBackground.raycastTarget = false;
        if (slotLabel      != null) slotLabel.raycastTarget      = false;

        // Exibe o nome da estação no label — visível como legenda na Etapa 2
        if (slotLabel != null && !string.IsNullOrEmpty(seasonName))
            slotLabel.text = seasonName;
    }

    // --------------------------------------------------------
    // Destaque de seleção (Etapa 2)
    // --------------------------------------------------------
    public void SetSelectionHighlight(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.gameObject.SetActive(selected);
    }

    // --------------------------------------------------------
    // Celebração final: dá brilho dourado à pintura (sem cobri-la com sólido)
    // --------------------------------------------------------
    public void PlayCompletionEffect()
    {
        // Tinta a pintura com tom dourado suave — sinal de conclusão sem bloquear a imagem
        if (paintingImage != null)
            paintingImage.color = new Color(1f, 0.95f, 0.6f, 1f);

        // borderImage permanece transparente
    }
}
