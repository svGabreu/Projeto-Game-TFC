
// Gerencia o painel de encaixe dos 3 amuletos na porta da pirâmide.
// Ao preencher os 3 slots, libera a SceneTransition para DentroDaPiramide.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class AmuletoPainelUI : MonoBehaviour
{
    public static AmuletoPainelUI Instance { get; private set; }

    [Header("Painel")]
    public GameObject painelRaiz;

    [Header("Slots (arraste os 3 AmuletoSlotUI em ordem)")]
    public AmuletoSlotUI[] slots = new AmuletoSlotUI[3];

    [Header("Feedback")]
    public TextMeshProUGUI labelStatus;
    public Button btnFechar;

    [Header("Recompensa — porta que será desbloqueada")]
    public SceneTransition portaTransition; 
    public GameObject portaVisual;          //  animação/visual da porta abrindo
    public AudioSource audioAbertura;       //  som de porta abrindo

    [Header("Trigger da porta — desativar ao abrir")]
    public AmuletoTrigger amuletoTrigger;

    // ── Estado ────────────────────────────────────────────────────────────────
    private bool isOpen   = false;
    private bool unlocked = false;

    private const string KEY = "amuletos.";
    private GameStateManager GSM => GameStateManager.Instance;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (painelRaiz != null) painelRaiz.SetActive(false);
    }

    private void Start()
    {
        if (btnFechar != null) btnFechar.onClick.AddListener(ClosePanel);
        RestoreState();
    }

    private void Update()
    {
        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            ClosePanel();
    }

    // ── Persistência ──────────────────────────────────────────────────────────
    private void RestoreState()
    {
        if (GSM == null) return;

        // Não gatea em slot0 — qualquer slot pode ter sido preenchido primeiro
        for (int i = 0; i < slots.Length; i++)
            if (GSM.GetBool(KEY + "slot" + i))
                slots[i].RestoreFilled(GSM.GetString(KEY + "slotName" + i));

        if (GSM.GetBool(KEY + "unlocked"))
            AbrirPorta(salvar: false);
    }

    // ── Abrir / Fechar painel ─────────────────────────────────────────────────
    public void OpenPanel()
    {
        if (unlocked)
        {
            // Porta já aberta — não precisa abrir o painel
            AbrirPorta(salvar: false);
            return;
        }

        isOpen = true;
        if (painelRaiz != null) painelRaiz.SetActive(true);

        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        AtualizarStatus();
    }

    public void ClosePanel()
    {
        isOpen = false;
        if (painelRaiz != null) painelRaiz.SetActive(false);

        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public bool IsOpen() => isOpen;

    // ── Encaixe de amuleto ────────────────────────────────────────────────────
    // Chamado por AmuletoSlotUI quando o jogador clica num slot
    public void OnSlotClicked(AmuletoSlotUI slot)
    {
        // Busca item compatível no inventário
        GlyphItem item = InventoryManager.Instance.GetItemByID(slot.expectedItemID);
        if (item == null)
        {
            AtualizarStatus($"Você não tem o {slot.slotLabel} no inventário.");
            return;
        }

        bool ok = slot.TryFill(item);
        if (!ok) return;

        // Salva em tempo real
        int idx = System.Array.IndexOf(slots, slot);
        if (idx >= 0)
        {
            GSM?.SetBool(KEY + "slot" + idx, true);
            GSM?.SetString(KEY + "slotName" + idx, item.displayName);
        }

        AtualizarStatus();
        ChecarConclusao();
    }

    // ── Verificação de conclusão ──────────────────────────────────────────────
    private void ChecarConclusao()
    {
        foreach (var slot in slots)
            if (!slot.IsFilled) return;

        // Todos os slots preenchidos!
        GSM?.SetBool(KEY + "unlocked", true);
        StartCoroutine(SequenciaAbertura());
    }

    private IEnumerator SequenciaAbertura()
    {
        AtualizarStatus("Os três amuletos foram encaixados! A porta se abre...");

        yield return new WaitForSecondsRealtime(1.5f);

        ClosePanel();
        AbrirPorta(salvar: true);
    }

    private void AbrirPorta(bool salvar)
    {
        unlocked = true;
        if (salvar) GSM?.SetBool(KEY + "unlocked", true);

        // Ativa o GameObject da porta de transição
        if (portaTransition != null)
        {
            portaTransition.gameObject.SetActive(true); // ← ativa o GameObject
            portaTransition.enabled = true;
        }

        // Desativa o trigger dos amuletos (Collider + script)
        if (amuletoTrigger != null)
        {
            amuletoTrigger.enabled = false;
            var col = amuletoTrigger.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        if (portaVisual != null) portaVisual.SetActive(false);
        if (audioAbertura != null) audioAbertura.Play();

        Debug.Log("[Amuletos] Porta da pirâmide desbloqueada!");
    }

    // ── Status ────────────────────────────────────────────────────────────────
    private void AtualizarStatus(string msg = null)
    {
        if (labelStatus == null) return;

        if (msg != null) { labelStatus.text = msg; return; }

        int preenchidos = 0;
        foreach (var slot in slots) if (slot.IsFilled) preenchidos++;

        labelStatus.text = preenchidos == slots.Length
            ? "Todos os amuletos encaixados!"
            : $"{preenchidos}/{slots.Length} amuletos encaixados";
    }
}
