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

using System.Collections;
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
    public Button closePanelButton;   // botão fechar da Etapa 1
    public Button closePanelButton2;  // botão fechar da Etapa 2 (PainelPiramide)
    public TextMeshProUGUI feedbackText; // texto de erro/acerto — começa desativado

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

    private static readonly Color COLOR_SELECTED   = new Color(1f, 0.85f, 0.1f);
    private static readonly Color COLOR_UNSELECTED = Color.white;

    private Coroutine feedbackRoutine;

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
        if (closePanelButton  != null) closePanelButton.onClick.AddListener(ClosePanel);
        if (closePanelButton2 != null) closePanelButton2.onClick.AddListener(ClosePanel);
        if (feedbackText      != null) feedbackText.gameObject.SetActive(false);
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

        if (feedbackRoutine != null) { StopCoroutine(feedbackRoutine); feedbackRoutine = null; }
        if (feedbackText    != null) feedbackText.gameObject.SetActive(false);

        if (painelRaiz != null) painelRaiz.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // --------------------------------------------------------
    // Feedback visual de erro/acerto
    // --------------------------------------------------------
    private void ShowError(string msg)
    {
        if (feedbackText == null) return;
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(FeedbackRoutine(msg));
    }

    private IEnumerator FeedbackRoutine(string msg)
    {
        feedbackText.text  = msg;
        feedbackText.color = Color.red;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        feedbackText.gameObject.SetActive(false);
        feedbackRoutine = null;
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

            // Ícone — procura primeiro Image não-raiz
            Image iconImg = null;
            foreach (var img in go.GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject == go) continue;
                iconImg = img;
                break;
            }

            // Etapa 2 usa sprite; Etapa 1 usa nome (pergaminhos têm sprite mas devem mostrar texto)
            bool usarSprite = etapa2Ativa && item.itemSprite != null;

            if (iconImg != null)
            {
                if (usarSprite)
                {
                    iconImg.sprite = item.itemSprite;
                    iconImg.color  = Color.white;
                    iconImg.gameObject.SetActive(true);
                }
                else
                    iconImg.gameObject.SetActive(false);
            }

            // Label — mostra nome na Etapa 1, esconde na Etapa 2 se há sprite
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.gameObject.SetActive(!usarSprite);
                if (!usarSprite) tmp.text = item.displayName;
            }

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
            bool ok = puzzle.TryAssignName(peca, selectedItem);
            if (!ok) ShowError("Nome incorreto!");
            selectedItem = null;
            pendingPeca  = null;
            ClearSlotHighlight();
            RefreshMiniInventory();
        }
        else
        {
            pendingPeca = peca;
        }
    }

    /// <summary>Jogador clicou num nível da pirâmide (Etapa 2).</summary>
    public void OnNivelClicked(PiramideNivelUI nivel)
    {
        if (puzzle == null || puzzle.Etapa != 2) return;

        if (selectedItem != null)
        {
            bool ok = puzzle.TryPlacePiece(nivel, selectedItem);
            if (!ok) ShowError("Posição errada!");
            selectedItem  = null;
            pendingNivel  = null;
            ClearSlotHighlight();
            DeselectAllNiveis();
            RefreshMiniInventory();
        }
        else
        {
            DeselectAllNiveis();
            pendingNivel = nivel;
            nivel.SetSelectionHighlight(true);
        }
    }

    /// <summary>Jogador clicou num item do mini inventário.</summary>
    public void OnInventoryItemClicked(GlyphItem item, GameObject slotGO = null)
    {
        ClearSlotHighlight();
        selectedItem   = item;
        selectedSlotGO = slotGO;
        SetSlotHighlight(slotGO, true);

        if (puzzle == null) return;

        if (puzzle.Etapa == 1 && pendingPeca != null)
        {
            bool ok = puzzle.TryAssignName(pendingPeca, item);
            if (!ok) ShowError("Nome incorreto!");
            selectedItem   = null;
            pendingPeca    = null;
            ClearSlotHighlight();
            RefreshMiniInventory();
        }
        else if (puzzle.Etapa == 2 && pendingNivel != null)
        {
            bool ok = puzzle.TryPlacePiece(pendingNivel, item);
            if (!ok) ShowError("Posição errada!");
            selectedItem  = null;
            pendingNivel  = null;
            ClearSlotHighlight();
            DeselectAllNiveis();
            RefreshMiniInventory();
        }
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
