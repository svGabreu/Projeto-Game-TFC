// RioDaVidaUI.cs
// Coloque em: Assets/Scripts/Puzzle/
// Gerencia o painel da Casa do Rio da Vida:
//   - Abre/fecha com cursor
//   - Exibe mini-inventário dos pergaminhos coletados
//   - Roteia cliques nos slots (Etapa 1) e nos quadros (Etapa 2) para o RioDaVidaPuzzle

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RioDaVidaUI : MonoBehaviour
{
    [Header("Painel raiz")]
    public GameObject painelRaiz;

    [Header("Referencia ao Puzzle")]
    public RioDaVidaPuzzle puzzle;

    [Header("Mini-inventario")]
    public Transform miniInventoryContainer;
    public GameObject miniSlotPrefab;

    [Header("Textos de estado")]
    public TextMeshProUGUI labelEtapa;

    [Header("Textos das etapas")]
    [SerializeField] private string textoEtapa1 = "Etapa 1 - Nomeie cada estacao do Nilo";
    [SerializeField] private string textoEtapa2 = "Etapa 2 - Ordene as estacoes do calendario do Nilo";
    [SerializeField] private string textoConcluido = "Puzzle concluido!";

    [Header("Feedback de Erro")]
    [SerializeField] private TextMeshProUGUI feedbackErroLabel;
    [SerializeField] private float feedbackDuracao = 2f;

    [Header("Botao Fechar")]
    public Button closePanelButton;

    public static RioDaVidaUI Instance { get; private set; }

    private bool isOpen = false;
    private GlyphItem itemEmMao = null;
    private GameObject slotSelecionadoGO = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (painelRaiz != null) painelRaiz.SetActive(false);
        if (feedbackErroLabel != null) feedbackErroLabel.gameObject.SetActive(false);
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

    private void ResolvePuzzleRef()
    {
        if (puzzle != null) return;
        puzzle = Object.FindFirstObjectByType<RioDaVidaPuzzle>(FindObjectsInactive.Include);
        if (puzzle != null)
            Debug.Log("[RioDaVidaUI] puzzle resolvido: " + puzzle.gameObject.name);
        else
            Debug.LogWarning("[RioDaVidaUI] RioDaVidaPuzzle nao encontrado.");
    }

    public void OpenPanel()
    {
        isOpen = true;

        // 1. Ativa o painel — isso dispara o Start() do RioDaVidaPuzzle
        //    se for a primeira vez que o painel e aberto na sessao
        if (painelRaiz != null) painelRaiz.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. So AGORA resolve a referencia e atualiza a UI —
        //    o puzzle.Start() ja rodou e etapa ja foi restaurada
        ResolvePuzzleRef();
        AtualizarLabelEtapa();

        if (puzzle != null && puzzle.Etapa == 1)
            RefreshMiniInventory();
    }

    public void ClosePanel()
    {
        isOpen = false;
        if (painelRaiz != null) painelRaiz.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsOpen() => isOpen;

    public void RefreshMiniInventory()
    {
        if (miniInventoryContainer == null) return;

        foreach (Transform child in miniInventoryContainer)
            Destroy(child.gameObject);

        itemEmMao = null;
        slotSelecionadoGO = null;

        // Monta conjunto dos IDs esperados pelos quadros deste puzzle
        var scrollsEsperados = new System.Collections.Generic.HashSet<string>();
        if (puzzle != null)
            foreach (var quadro in puzzle.quadros)
                if (quadro != null && !string.IsNullOrEmpty(quadro.expectedScrollID))
                    scrollsEsperados.Add(quadro.expectedScrollID);

        List<GlyphItem> items = InventoryManager.Instance.GetAllItems();

        foreach (GlyphItem item in items)
        {
            // Só exibe pergaminhos que pertencem a este puzzle
            if (scrollsEsperados.Count > 0 && !scrollsEsperados.Contains(item.itemID)) continue;
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

    private void OnScrollSelecionado(GlyphItem item, GameObject slotGO)
    {
        if (slotSelecionadoGO != null)
        {
            Image prev = slotSelecionadoGO.GetComponent<Image>();
            if (prev != null) prev.color = Color.white;
        }

        itemEmMao = item;
        slotSelecionadoGO = slotGO;

        Image img = slotGO.GetComponent<Image>();
        if (img != null) img.color = Color.yellow;

        Debug.Log("[RioDaVida] Pergaminho selecionado: " + item.displayName);
    }

    public void OnSlotClicked(RioDaVidaQuadroUI quadro)
    {
        ResolvePuzzleRef();
        if (puzzle == null || puzzle.Etapa != 1) return;

        if (itemEmMao == null)
        {
            Debug.Log("[RioDaVida] Selecione um pergaminho primeiro.");
            return;
        }

        puzzle.TryAssignScroll(quadro, itemEmMao);

        itemEmMao = null;
        slotSelecionadoGO = null;
        RefreshMiniInventory();
        AtualizarLabelEtapa();
    }

    public void OnQuadroClicked(RioDaVidaQuadroUI quadro)
    {
        ResolvePuzzleRef();
        if (puzzle == null || puzzle.Etapa != 2) return;
        puzzle.HandleQuadroClick(quadro);
    }

    public void MostrarErro(string mensagem)
    {
        if (feedbackErroLabel == null) return;
        StopCoroutine(nameof(OcultarErroApos));
        feedbackErroLabel.text = mensagem;
        feedbackErroLabel.gameObject.SetActive(true);
        StartCoroutine(nameof(OcultarErroApos));
    }

    private IEnumerator OcultarErroApos()
    {
        yield return new WaitForSecondsRealtime(feedbackDuracao);
        if (feedbackErroLabel != null) feedbackErroLabel.gameObject.SetActive(false);
    }

    public void OnPuzzleComplete()
    {
        AtualizarLabelEtapa();
    }

    public void OnEtapa2Restored()
    {
        AtualizarLabelEtapa();
    }

    private void AtualizarLabelEtapa()
    {
        if (labelEtapa == null || puzzle == null) return;

        if (puzzle.Etapa == 1)
            labelEtapa.text = textoEtapa1;
        else if (puzzle.Etapa == 2)
            labelEtapa.text = textoEtapa2;
        else
            labelEtapa.text = textoConcluido;
    }
}
