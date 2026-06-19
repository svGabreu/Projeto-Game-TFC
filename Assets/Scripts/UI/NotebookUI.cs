// NotebookUI.cs
// Coloque em: Assets/Scripts/UI/
// Painel do caderno de anotacoes do jogador.
// - Abre/fecha com a tecla N ou clicando no icone na tela
// - Lista as entradas coletadas (pistas dos WorldClues)
// - Clicar numa entrada exibe o detalhe (titulo, texto, ilustracao)
//
// Hierarquia esperada (dentro do UIGlobal, igual ao InventoryUI):
//   PainelNotebook (GameObject com este script)
//   |- TituloNotebook (TextMeshProUGUI)
//   |- ScrollView > Viewport > Content   <- entriesContainer
//   |- PainelDetalhe
//   |   |- DetalheTitulo (TextMeshProUGUI)
//   |   |- DetalheIlustracao (Image)
//   |   |- DetalheTexto (TextMeshProUGUI)
//   |- BtnFecharNotebook (Button)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class NotebookUI : MonoBehaviour
{
    public static NotebookUI Instance { get; private set; }

    [Header("Painel raiz")]
    public GameObject notebookPanel;

    [Header("Lista de entradas")]
    public Transform entriesContainer;      // Content do ScrollView
    public GameObject entryButtonPrefab;    // prefab de botao com TextMeshProUGUI

    [Header("Painel de detalhe")]
    public GameObject detailPanel;
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailText;
    public Image detailIllustration;

    [Header("Botao Fechar")]
    public Button closeButton;

    [Header("Texto quando vazio")]
    [SerializeField] private string textoVazio = "Nenhuma anotacao ainda. Explore o mundo em busca de pistas!";

    // ---- Estado ----
    private bool isOpen = false;
    private readonly List<GameObject> entryButtons = new List<GameObject>();

    // --------------------------------------------------------
    private void Awake()
    {
        // Singleton persistente entre cenas (acompanha o UIGlobal raiz)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);

        if (notebookPanel != null) notebookPanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // Atualiza a lista sempre que uma nova pista for adicionada
        if (NotebookManager.Instance != null)
            NotebookManager.Instance.OnNotebookUpdated.AddListener(RefreshEntries);
    }

    private void Update()
    {
        // Nao abre se outro painel estiver ativo
        if (!isOpen && IsAnyOtherPanelOpen()) return;

        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            if (isOpen) ClosePanel();
            else OpenPanel();
        }

        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            ClosePanel();
    }

    private bool IsAnyOtherPanelOpen()
    {
        if (InventoryUI.Instance   != null && InventoryUI.Instance.IsOpen())   return true;
        if (RioDaVidaUI.Instance   != null && RioDaVidaUI.Instance.IsOpen())   return true;
        if (SocialUI.Instance      != null && SocialUI.Instance.IsOpen())      return true;
        if (ItemExamineUI.Instance != null && ItemExamineUI.Instance.IsOpen()) return true;

        // MuralUI nao e singleton — busca instancia ativa na cena
        var mural = FindFirstObjectByType<MuralUI>();
        if (mural != null && mural.IsOpen()) return true;

        return false;
    }

    // --------------------------------------------------------
    // Abrir / Fechar
    // --------------------------------------------------------
    public void OpenPanel()
    {
        isOpen = true;
        if (notebookPanel != null) notebookPanel.SetActive(true);
        if (detailPanel != null) detailPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshEntries();
    }

    public void ClosePanel()
    {
        isOpen = false;
        if (notebookPanel != null) notebookPanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsOpen() => isOpen;

    // Chamado pelo botao/icone do caderno na tela (HUD)
    public void TogglePanel()
    {
        if (isOpen) ClosePanel();
        else if (!IsAnyOtherPanelOpen()) OpenPanel();
    }

    // --------------------------------------------------------
    // Reconstroi a lista de entradas
    // --------------------------------------------------------
    public void RefreshEntries()
    {
        if (entriesContainer == null) return;

        // Limpa botoes antigos
        foreach (var b in entryButtons)
            if (b != null) Destroy(b);
        entryButtons.Clear();

        if (NotebookManager.Instance == null) return;

        List<NotebookManager.NotebookEntry> entries =
            NotebookManager.Instance.GetAllEntries();

        // Caderno vazio — mostra mensagem placeholder
        if (entries.Count == 0)
        {
            if (entryButtonPrefab == null) return;
            GameObject placeholder = Instantiate(entryButtonPrefab, entriesContainer);
            entryButtons.Add(placeholder);

            var lbl = placeholder.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.text = textoVazio;

            var b = placeholder.GetComponent<Button>();
            if (b != null) b.interactable = false;
            return;
        }

        // Cria um botao por entrada
        foreach (var entry in entries)
        {
            if (entryButtonPrefab == null) break;

            GameObject btnGO = Instantiate(entryButtonPrefab, entriesContainer);
            entryButtons.Add(btnGO);

            var lbl = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.text = entry.title;

            var btn = btnGO.GetComponent<Button>();
            NotebookManager.NotebookEntry captured = entry;
            if (btn != null)
                btn.onClick.AddListener(() => ShowDetail(captured));
        }
    }

    // --------------------------------------------------------
    // Exibe o detalhe de uma entrada
    // --------------------------------------------------------
    private void ShowDetail(NotebookManager.NotebookEntry entry)
    {
        if (detailPanel != null) detailPanel.SetActive(true);

        if (detailTitle != null) detailTitle.text = entry.title;
        if (detailText  != null) detailText.text  = entry.hintText;

        if (detailIllustration != null)
        {
            bool temImagem = entry.illustration != null;
            detailIllustration.sprite  = entry.illustration;
            detailIllustration.enabled = temImagem;
        }
    }
}
