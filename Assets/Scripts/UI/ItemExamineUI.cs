// ItemExamineUI.cs
// Coloque em: Assets/Scripts/UI/
// Mostra um painel de visualização do item antes de coletar.
// Chamado por WorldClue quando o jogador pressiona E num coletável.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ItemExamineUI : MonoBehaviour
{
    public static ItemExamineUI Instance { get; private set; }

    [Header("Painel raiz")]
    public GameObject painelRaiz;

    [Header("Visual do Item")]
    public Image          itemIcon;            // imagem grande do item
    public TextMeshProUGUI itemNameText;       // nome do item
    public TextMeshProUGUI itemDescriptionText;// hintText do WorldClue

    [Header("Botões")]
    public Button btnColetar;   // adiciona ao inventário
    public Button btnLargar;    // fecha sem coletar

    // ---- estado interno ----
    private WorldClue pendingSource;
    private bool isOpen = false;

    // --------------------------------------------------------
    private void Awake()
    {
        // Singleton persistente entre cenas
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject); // persiste o Canvas raiz inteiro
        if (painelRaiz != null) painelRaiz.SetActive(false);
    }

    private void Start()
    {
        if (btnColetar != null) btnColetar.onClick.AddListener(OnColetar);
        if (btnLargar  != null) btnLargar.onClick.AddListener(OnLargar);
    }

    private void Update()
    {
        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            OnLargar();
    }

    // --------------------------------------------------------
    // Abre o painel exibindo o item e a descrição
    // --------------------------------------------------------
    public void OpenExamine(GlyphItem item, string description, WorldClue source)
    {
        pendingSource = source;
        isOpen        = true;

        if (itemIcon != null)
        {
            itemIcon.sprite  = item != null ? item.itemSprite : null;
            itemIcon.enabled = item != null && item.itemSprite != null;
        }
        if (itemNameText != null)
            itemNameText.text = item != null ? item.displayName : "";
        if (itemDescriptionText != null)
            itemDescriptionText.text = !string.IsNullOrEmpty(description) ? description : "";

        if (painelRaiz != null) painelRaiz.SetActive(true);
        Time.timeScale       = 0f;
        Cursor.lockState     = CursorLockMode.None;
        Cursor.visible       = true;
    }

    // --------------------------------------------------------
    // Jogador clica "Coletar"
    // --------------------------------------------------------
    private void OnColetar()
    {
        Close();
        pendingSource?.ConfirmCollect();
        pendingSource = null;
    }

    // --------------------------------------------------------
    // Jogador clica "Largar" ou pressiona ESC
    // --------------------------------------------------------
    private void OnLargar()
    {
        Close();
        pendingSource = null;
    }

    // Fecha só o visual — não limpa pendingSource (OnColetar precisa dele depois)
    private void Close()
    {
        isOpen = false;
        if (painelRaiz != null) painelRaiz.SetActive(false);
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // Versão pública para o SceneTransitionManager fechar tudo — aqui sim limpa
    public void ClosePanel()
    {
        pendingSource = null;
        Close();
    }

    public bool IsOpen() => isOpen;
}
