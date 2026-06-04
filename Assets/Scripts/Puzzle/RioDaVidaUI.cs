// RioDaVidaUI.cs
// Coloque em: Assets/Scripts/Puzzle/
// Gerencia o painel da Casa do Rio da Vida:
//   - Abre/fecha com cursor
//   - Exibe mini-inventário dos pergaminhos coletados
//   - Roteia cliques nos slots (Etapa 1) e nos quadros (Etapa 2) para o RioDaVidaPuzzle

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RioDaVidaUI : MonoBehaviour
{
    [Header("Painel raiz")]
    public GameObject painelRaiz;

    [Header("Referência ao Puzzle")]
    public RioDaVidaPuzzle puzzle;

    [Header("Mini-inventário")]
    public Transform miniInventoryContainer;
    public GameObject miniSlotPrefab;       // mesmo MiniSlot prefab do MuralUI

    [Header("Textos de estado")]
    public TextMeshProUGUI labelEtapa;      // opcional — exibe "Etapa 1" / "Etapa 2"

    [Header("Textos das etapas")]
    [SerializeField] private string textoEtapa1 = "Etapa 1 — Nomeie cada estação do Nilo";
    [SerializeField] private string textoEtapa2 = "Etapa 2 — Ordene as estações do calendário do Nilo";
    [SerializeField] private string textoConcluido = "Puzzle concluído!";

    [Header("Botão Fechar")]
    public Button closePanelButton;         // botão X para fechar o painel

    // Singleton leve
    public static RioDaVidaUI Instance { get; private set; }

    private bool isOpen = false;
    private GlyphItem itemEmMao = null;
    private GameObject slotSelecionadoGO = null;

    // --------------------------------------------------------
    private void Awake()
    {
        Instance = this;
        if (painelRaiz != null) painelRaiz.SetActive(false);
    }

    private void Start()
    {
        if (closePanelButton != null)
            closePanelButton.onClick.AddListener(ClosePanel);
    }

    private void Update()
    {
        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            ClosePanel();
    }

    // --------------------------------------------------------
    // Abrir / Fechar
    // --------------------------------------------------------
    public void OpenPanel()
    {
        isOpen = true;
        if (painelRaiz != null) painelRaiz.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        AtualizarLabelEtapa();
        RefreshMiniInventory();
    }

    public void ClosePanel()
    {
        isOpen = false;
        if (painelRaiz != null) painelRaiz.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public bool IsOpen() => isOpen;

    // --------------------------------------------------------
    // Atualiza o mini-inventário mostrando apenas QuestItems (pergaminhos)
    // --------------------------------------------------------
    public void RefreshMiniInventory()
    {
        if (miniInventoryContainer == null) return;

        foreach (Transform child in miniInventoryContainer)
            Destroy(child.gameObject);

        itemEmMao      = null;
        slotSelecionadoGO = null;

        List<GlyphItem> items = InventoryManager.Instance.GetAllItems();

        foreach (GlyphItem item in items)
        {
            // Só exibe pergaminhos (QuestItem)
            if (item.itemType != GlyphItemType.QuestItem) continue;
            if (miniSlotPrefab == null) break;

            GameObject slotGO = Instantiate(miniSlotPrefab, miniInventoryContainer);

            Image icon = slotGO.GetComponentInChildren<Image>();
            if (icon != null && item.itemSprite != null)
                icon.sprite = item.itemSprite;

            TextMeshProUGUI label = slotGO.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = item.displayName;

            Button btn = slotGO.GetComponent<Button>();
            GlyphItem capturedItem = item;
            GameObject capturedSlot = slotGO;
            if (btn != null)
                btn.onClick.AddListener(() => OnScrollSelecionado(capturedItem, capturedSlot));
        }
    }

    // --------------------------------------------------------
    // Jogador clica num pergaminho no mini-inventário
    // --------------------------------------------------------
    private void OnScrollSelecionado(GlyphItem item, GameObject slotGO)
    {
        // Deseleciona o anterior
        if (slotSelecionadoGO != null)
        {
            Image prev = slotSelecionadoGO.GetComponent<Image>();
            if (prev != null) prev.color = Color.white;
        }

        itemEmMao         = item;
        slotSelecionadoGO = slotGO;

        Image img = slotGO.GetComponent<Image>();
        if (img != null) img.color = Color.yellow;

        Debug.Log($"[RioDaVida] Pergaminho selecionado: {item.displayName}");
    }

    // --------------------------------------------------------
    // Chamado por RioDaVidaQuadroUI quando slot de pergaminho é clicado (Etapa 1)
    // --------------------------------------------------------
    public void OnSlotClicked(RioDaVidaQuadroUI quadro)
    {
        if (puzzle == null || puzzle.Etapa != 1) return;

        if (itemEmMao == null)
        {
            Debug.Log("[RioDaVida] Selecione um pergaminho primeiro.");
            return;
        }

        puzzle.TryAssignScroll(quadro, itemEmMao);

        // Reset seleção
        itemEmMao         = null;
        slotSelecionadoGO = null;
        RefreshMiniInventory();
        AtualizarLabelEtapa();
    }

    // --------------------------------------------------------
    // Chamado por RioDaVidaQuadroUI quando o quadro é clicado (Etapa 2)
    // --------------------------------------------------------
    public void OnQuadroClicked(RioDaVidaQuadroUI quadro)
    {
        if (puzzle == null || puzzle.Etapa != 2) return;
        puzzle.HandleQuadroClick(quadro);
    }

    // --------------------------------------------------------
    // Chamado pelo RioDaVidaPuzzle ao concluir o puzzle
    // --------------------------------------------------------
    public void OnPuzzleComplete()
    {
        AtualizarLabelEtapa();
        // Opcional: fechar painel após alguns segundos
        // Invoke(nameof(ClosePanel), 2f);
    }

    // --------------------------------------------------------
    // Helpers
    // --------------------------------------------------------
    private void AtualizarLabelEtapa()
    {
        if (labelEtapa == null || puzzle == null) return;

        labelEtapa.text = puzzle.Etapa switch
        {
            1 => textoEtapa1,
            2 => textoEtapa2,
            _ => textoConcluido
        };
    }
}
