// SocialUI.cs
// Coloque em: Assets/Scripts/UI/
// Singleton que gerencia toda a interface do Puzzle 3 (Pirâmide Social).
//
// Fluxo Etapa 1:
//   1. Jogador clica num slot de nome abaixo de uma peça  → OnPecaSlotClicked()
//   2. Jogador clica num item do mini inventário          → OnInventoryItemClicked()
//   3. Puzzle valida; se correto, slot fica verde.
//
// Fluxo Etapa 2:
//   1. Jogador clica num item do mini inventário          → OnInventoryItemClicked()
//   2. Jogador clica num nível da pirâmide               → OnNivelClicked()
//   3. Puzzle valida; se correto, nível fica dourado.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class SocialUI : MonoBehaviour
{
    public static SocialUI Instance { get; private set; }

    public enum PanelMode { Mesa, Mural }

    [Header("Referências")]
    public SocialPuzzle puzzle;

    [Header("Painel raiz")]
    public GameObject painelRaiz;

    [Header("Etapa 1 — Mesa de Identificação")]
    public GameObject etapa1Panel;

    [Header("Etapa 2 — Pirâmide Hierárquica")]
    public GameObject etapa2Panel;

    [Header("Geral")]
    public TextMeshProUGUI labelEtapa;
    public Button closePanelButton;

    [Header("Mini Inventário")]
    public Transform miniInventoryContainer;
    public GameObject miniSlotPrefab;       // mesmo prefab usado no Puzzle 2

    // ---- Estado ----
    private bool isOpen = false;
    private GlyphItem selectedItem = null;
    private SocialPecaUI pendingPeca = null;
    private PiramideNivelUI pendingNivel = null;

    private readonly List<GameObject> miniSlots = new List<GameObject>();

    // --------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
        if (painelRaiz != null) painelRaiz.SetActive(false);
    }

    private void Start()
    {
        if (closePanelButton != null)
            closePanelButton.onClick.AddListener(ClosePanel);
    }

    private void Update()
    {
        if (!isOpen) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ClosePanel();
    }

    public bool IsOpen() => isOpen;

    // --------------------------------------------------------
    // Abrir / Fechar painel
    // --------------------------------------------------------
    public void OpenPanel(PanelMode mode = PanelMode.Mesa)
    {
        if (puzzle == null) return;

        isOpen = true;
        selectedItem  = null;
        pendingPeca   = null;
        pendingNivel  = null;

        if (painelRaiz != null) painelRaiz.SetActive(true);

        // Se o modo for Mesa, mostra Etapa 1; se Mural, mostra Etapa 2.
        // Respeita também o estado atual do puzzle (não permite regredir).
        bool showEtapa2 = (mode == PanelMode.Mural) && puzzle.Etapa >= 2;
        if (etapa1Panel != null) etapa1Panel.SetActive(!showEtapa2);
        if (etapa2Panel != null) etapa2Panel.SetActive(showEtapa2);

        UpdateLabel();
        RefreshMiniInventory();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void ClosePanel()
    {
        isOpen = false;
        selectedItem  = null;
        pendingPeca   = null;
        pendingNivel  = null;

        DeselectAllNiveis();

        if (painelRaiz != null) painelRaiz.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void UpdateLabel()
    {
        if (labelEtapa == null || puzzle == null) return;
        labelEtapa.text = puzzle.Etapa switch
        {
            1 => "Etapa 1 — Identifique cada personagem",
            2 => "Etapa 2 — Monte a Pirâmide Social",
            _ => "Pirâmide concluída!"
        };
    }

    // --------------------------------------------------------
    // Mini Inventário — mostra apenas itens relevantes para a etapa
    // --------------------------------------------------------
    private void RefreshMiniInventory()
    {
        foreach (var s in miniSlots) if (s != null) Destroy(s);
        miniSlots.Clear();

        if (miniInventoryContainer == null || miniSlotPrefab == null || puzzle == null) return;

        string prefix = puzzle.Etapa == 1 ? "name_" : "piece_";

        foreach (var item in InventoryManager.Instance.GetAllItems())
        {
            if (!item.itemID.StartsWith(prefix)) continue;

            var go = Instantiate(miniSlotPrefab, miniInventoryContainer);
            miniSlots.Add(go);

            // Ícone
            var img = go.GetComponentInChildren<Image>();
            if (img != null && item.itemSprite != null) img.sprite = item.itemSprite;

            // Label
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = item.displayName;

            // Clique
            var btn = go.GetComponent<Button>();
            var captured = item;
            if (btn != null) btn.onClick.AddListener(() => OnInventoryItemClicked(captured));
        }
    }

    // --------------------------------------------------------
    // Callbacks de interação
    // --------------------------------------------------------

    /// <summary>Jogador clicou no slot abaixo de uma peça (Etapa 1).</summary>
    public void OnPecaSlotClicked(SocialPecaUI peca)
    {
        if (puzzle == null || puzzle.Etapa != 1) return;

        if (selectedItem != null)
        {
            // Já tem item selecionado → tenta atribuir imediatamente
            puzzle.TryAssignName(peca, selectedItem);
            selectedItem = null;
            pendingPeca  = null;
            RefreshMiniInventory();
        }
        else
        {
            // Marca a peça como pendente (jogador vai escolher o nome agora)
            pendingPeca = peca;
        }
    }

    /// <summary>Jogador clicou num nível da pirâmide (Etapa 2).</summary>
    public void OnNivelClicked(PiramideNivelUI nivel)
    {
        if (puzzle == null || puzzle.Etapa != 2) return;

        if (selectedItem != null)
        {
            puzzle.TryPlacePiece(nivel, selectedItem);
            selectedItem  = null;
            pendingNivel  = null;
            DeselectAllNiveis();
            RefreshMiniInventory();
        }
        else
        {
            // Marca nível como pendente
            DeselectAllNiveis();
            pendingNivel = nivel;
            nivel.SetSelectionHighlight(true);
        }
    }

    /// <summary>Jogador clicou num item do mini inventário.</summary>
    public void OnInventoryItemClicked(GlyphItem item)
    {
        selectedItem = item;

        if (puzzle == null) return;

        if (puzzle.Etapa == 1 && pendingPeca != null)
        {
            puzzle.TryAssignName(pendingPeca, item);
            selectedItem = null;
            pendingPeca  = null;
            RefreshMiniInventory();
        }
        else if (puzzle.Etapa == 2 && pendingNivel != null)
        {
            puzzle.TryPlacePiece(pendingNivel, item);
            selectedItem  = null;
            pendingNivel  = null;
            DeselectAllNiveis();
            RefreshMiniInventory();
        }
        // Se não houver pendente, o item fica "selecionado" aguardando clique na peça/nível
    }

    // --------------------------------------------------------
    // Transição Etapa 1 → 2 (chamado por SocialPuzzle)
    // --------------------------------------------------------
    public void UnlockEtapa2()
    {
        if (etapa1Panel != null) etapa1Panel.SetActive(false);
        if (etapa2Panel != null) etapa2Panel.SetActive(true);
        selectedItem = null;
        pendingPeca  = null;
        UpdateLabel();
        RefreshMiniInventory();
        Debug.Log("[SocialUI] Etapa 2 desbloqueada — Pirâmide disponível!");
    }

    // --------------------------------------------------------
    // Conclusão (chamado por SocialPuzzle)
    // --------------------------------------------------------
    public void OnPuzzleComplete()
    {
        UpdateLabel();
        // Não fecha automaticamente — permite o jogador ver o resultado
        Debug.Log("[SocialUI] Puzzle 3 concluído!");
    }

    // --------------------------------------------------------
    // Utilitários
    // --------------------------------------------------------
    private void DeselectAllNiveis()
    {
        if (puzzle == null) return;
        foreach (var n in puzzle.niveis)
            if (n != null) n.SetSelectionHighlight(false);
    }
}
