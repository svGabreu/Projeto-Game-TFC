// SocialUI.cs
// Coloque em: Assets/Scripts/UI/
// Singleton que gerencia toda a interface do Puzzle 3 (Pirâmide Social).
//
// Fluxo Etapa 1:
//   1. Jogador clica num item do mini inventário (fica amarelo = selecionado)
//   2. Jogador clica no slot abaixo da figura correta  → OnPecaSlotClicked()
//   3. Puzzle valida; se correto, slot fica verde.
//   OU: clica no slot primeiro, depois no item.
//
// Fluxo Etapa 2:
//   1. Jogador clica num item do mini inventário (fica amarelo = selecionado)
//   2. Jogador clica no nível correto da pirâmide → OnNivelClicked()
//   3. Puzzle valida; se correto, nível fica dourado.
//   OU: clica no nível primeiro, depois no item.

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

    [Header("Mini Inventário — Etapa 1 (dentro do Etapa1Panel)")]
    public Transform miniInventoryContainer;   // Content do ScrollRect dentro do Etapa1Panel
    public GameObject miniSlotPrefab;

    [Header("Mini Inventário — Etapa 2 (dentro do Etapa2Panel)")]
    public Transform miniInventoryContainer2;  // Content do ScrollRect dentro do Etapa2Panel

    // ---- Estado ----
    private bool isOpen = false;
    private GlyphItem selectedItem = null;
    private SocialPecaUI pendingPeca = null;
    private PiramideNivelUI pendingNivel = null;

    private readonly List<GameObject> miniSlots  = new List<GameObject>();
    private GameObject selectedSlotGO = null;   // slot destacado no mini-inventário

    // Cores de destaque do slot selecionado
    private static readonly Color COLOR_SELECTED   = new Color(1f, 0.85f, 0.1f);  // amarelo
    private static readonly Color COLOR_UNSELECTED = Color.white;

    // --------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        

        // Garante Sort Order alto o suficiente para ficar acima de outros painéis (ex: UIGlobal = 10)
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            if (canvas.sortingOrder < 20)
                canvas.sortingOrder = 20;

            if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

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

        // Fecha outros painéis que possam estar bloqueando cliques
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen())
            InventoryUI.Instance.ClosePanel();

        isOpen = true;
        selectedItem  = null;
        pendingPeca   = null;
        pendingNivel  = null;
        selectedSlotGO = null;

        if (painelRaiz != null) painelRaiz.SetActive(true);

        // Mesa → Etapa 1; Mural → Etapa 2 (se puzzle já chegou lá)
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
        selectedItem   = null;
        pendingPeca    = null;
        pendingNivel   = null;
        selectedSlotGO = null;

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
    // Mini Inventário
    // Usa miniInventoryContainer  para Etapa 1 (dentro do Etapa1Panel ativo)
    // Usa miniInventoryContainer2 para Etapa 2 (dentro do Etapa2Panel ativo)
    // Isso evita instanciar itens em um pai inativo.
    // --------------------------------------------------------
    private void RefreshMiniInventory()
    {
        // Destrói slots anteriores
        foreach (var s in miniSlots) if (s != null) Destroy(s);
        miniSlots.Clear();
        selectedSlotGO = null;

        if (miniSlotPrefab == null || puzzle == null) return;
        if (InventoryManager.Instance == null) return;

        // Escolhe o container correto para a etapa atual
        bool etapa2Ativa = puzzle.Etapa >= 2;
        Transform container = etapa2Ativa ? miniInventoryContainer2 : miniInventoryContainer;
        if (container == null)
        {
            Debug.LogWarning("[SocialUI] miniInventoryContainer" + (etapa2Ativa ? "2" : "") + " não atribuído!");
            return;
        }

        string prefix = etapa2Ativa ? "piece_" : "name_";

        foreach (var item in InventoryManager.Instance.GetAllItems())
        {
            if (!item.itemID.StartsWith(prefix)) continue;

            var go = Instantiate(miniSlotPrefab, container);
            miniSlots.Add(go);

            // Ícone (primeiro Image encontrado em filhos)
            var imgs = go.GetComponentsInChildren<Image>(true);
            foreach (var img in imgs)
            {
                // Pula o Image raiz (fundo do botão)
                if (img.gameObject == go) continue;
                if (item.itemSprite != null) img.sprite = item.itemSprite;
                break;
            }

            // Label
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) tmp.text = item.displayName;

            // Botão — procura no root e em filhos
            var btn = go.GetComponent<Button>() ?? go.GetComponentInChildren<Button>(true);
            if (btn != null)
            {
                btn.interactable = true;
                // Garante que o targetGraphic recebe raycasts
                if (btn.targetGraphic != null)
                    btn.targetGraphic.raycastTarget = true;

                var capturedItem = item;
                var capturedGO   = go;
                btn.onClick.AddListener(() => OnInventoryItemClicked(capturedItem, capturedGO));
            }
            else
            {
                Debug.LogWarning($"[SocialUI] MiniSlot instanciado sem Button: {go.name}");
            }
        }
    }

    // --------------------------------------------------------
    // Highlight de seleção no mini-inventário
    // --------------------------------------------------------
    private void SetSlotHighlight(GameObject slotGO, bool highlight)
    {
        if (slotGO == null) return;
        var btn = slotGO.GetComponent<Button>() ?? slotGO.GetComponentInChildren<Button>(true);
        if (btn != null && btn.targetGraphic != null)
            btn.targetGraphic.color = highlight ? COLOR_SELECTED : COLOR_UNSELECTED;
    }

    private void ClearSlotHighlight()
    {
        if (selectedSlotGO != null)
        {
            SetSlotHighlight(selectedSlotGO, false);
            selectedSlotGO = null;
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
            // Item já selecionado → atribui imediatamente
            puzzle.TryAssignName(peca, selectedItem);
            selectedItem = null;
            pendingPeca  = null;
            ClearSlotHighlight();
            RefreshMiniInventory();
        }
        else
        {
            // Aguarda o jogador escolher o nome no mini-inventário
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
            ClearSlotHighlight();
            DeselectAllNiveis();
            RefreshMiniInventory();
        }
        else
        {
            // Aguarda o jogador escolher a peça no mini-inventário
            DeselectAllNiveis();
            pendingNivel = nivel;
            nivel.SetSelectionHighlight(true);
        }
    }

    /// <summary>Jogador clicou num item do mini inventário.</summary>
    public void OnInventoryItemClicked(GlyphItem item, GameObject slotGO = null)
    {
        // Destaca o slot clicado
        ClearSlotHighlight();
        selectedItem   = item;
        selectedSlotGO = slotGO;
        SetSlotHighlight(slotGO, true);

        if (puzzle == null) return;

        if (puzzle.Etapa == 1 && pendingPeca != null)
        {
            puzzle.TryAssignName(pendingPeca, item);
            selectedItem   = null;
            pendingPeca    = null;
            ClearSlotHighlight();
            RefreshMiniInventory();
        }
        else if (puzzle.Etapa == 2 && pendingNivel != null)
        {
            puzzle.TryPlacePiece(pendingNivel, item);
            selectedItem  = null;
            pendingNivel  = null;
            ClearSlotHighlight();
            DeselectAllNiveis();
            RefreshMiniInventory();
        }
        // Se não houver pendente, item fica selecionado (amarelo) aguardando clique na peça/nível
    }

    // --------------------------------------------------------
    // Transição Etapa 1 → 2 (chamado por SocialPuzzle)
    // --------------------------------------------------------
    public void UnlockEtapa2()
    {
        if (etapa1Panel != null) etapa1Panel.SetActive(false);
        if (etapa2Panel != null) etapa2Panel.SetActive(true);
        selectedItem   = null;
        pendingPeca    = null;
        selectedSlotGO = null;
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
        Debug.Log("[SocialUI] Puzzle 3 concluído!");
    }

    // Permite mostrar mensagens customizadas via UnityEvent no Inspector
    public void MostrarMensagem(string msg)
    {
        if (labelEtapa != null) labelEtapa.text = msg;
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
