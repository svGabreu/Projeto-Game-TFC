// SocialPuzzle.cs
// Coloque em: Assets/Scripts/Puzzle/
// Coordena as duas etapas do Puzzle 3 (Casa da Pirâmide Social).
//
// Etapa 1: jogador identifica os 5 personagens no painel → coleta cada estátua com E.
// Etapa 2: jogador encaixa as 5 peças nos níveis corretos da pirâmide.
// Conclusão: libera o Amuleto da Ordem.

using UnityEngine;
using UnityEngine.Events;

public class SocialPuzzle : MonoBehaviour
{
    public static SocialPuzzle Instance { get; private set; }

    [Header("Etapa 1 — Painel de Identificação (5 peças)")]
    public SocialPecaUI[] pecas = new SocialPecaUI[5];

    [Header("Estátuas 3D na Mesa")]
    public SocialStatue[] estatuas = new SocialStatue[5];

    [Header("Etapa 2 — Pirâmide Hierárquica (5 níveis)")]
    public PiramideNivelUI[] niveis = new PiramideNivelUI[5];

    [Header("Recompensa")]
    public GameObject rewardWorldObject;    // objeto com WorldClue na cena — começa desativado
    public DialogueData completionDialogue; // opcional — Anjinho comenta a conclusão

    [Header("Eventos")]
    public UnityEvent OnAllNamed;
    public UnityEvent OnAllCollected;
    public UnityEvent OnPuzzleCompleted;

    private int etapa = 1;
    private int collectedCount = 0;
    public int Etapa => etapa;
    public bool AllCollected => collectedCount >= estatuas.Length;

    private const string KEY = "soc.";
    private const string KEY_REWARD = "soc.reward";
    private GameStateManager GSM => GameStateManager.Instance;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => RestoreState();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        // REMOVIDO: SaveState() aqui — não confiável
    }

    // ── Restauração ───────────────────────────────────────────────────────────
    private void RestoreState()
    {
        if (GSM == null || !GSM.HasKey(KEY + "etapa")) return;

        etapa = GSM.GetInt(KEY + "etapa", 1);
        collectedCount = GSM.GetInt(KEY + "collected", 0);

        for (int i = 0; i < pecas.Length; i++)
            if (GSM.GetBool(KEY + "named" + i))
                pecas[i].RestoreNamed(GSM.GetString(KEY + "nameLabel" + i));

        for (int i = 0; i < niveis.Length; i++)
            if (GSM.GetBool(KEY + "filled" + i))
                niveis[i].RestoreFilled(GSM.GetString(KEY + "pieceLabel" + i));

        if (etapa >= 3)
        {
            foreach (var n in niveis) n.PlayCompletionEffect();
            TryRestoreReward();
        }

        Debug.Log($"[Social] Estado restaurado — Etapa {etapa}, {collectedCount} coletadas.");
    }

    // ── Etapa 1 — Identificação ───────────────────────────────────────────────
    public bool TryAssignName(SocialPecaUI peca, GlyphItem item)
    {
        if (etapa != 1) return false;

        bool acerto = peca.TryAssignName(item);
        if (!acerto) return false;

        int idx = System.Array.IndexOf(pecas, peca);
        if (idx >= 0)
        {
            GSM?.SetBool(KEY + "named" + idx, true);
            GSM?.SetString(KEY + "nameLabel" + idx, item.displayName);
            GSM?.SetInt(KEY + "etapa", etapa); // garante que RestoreState() roda ao voltar na cena
        }

        bool todasNomeadas = true;
        foreach (var p in pecas) if (!p.IsNamed) { todasNomeadas = false; break; }

        if (todasNomeadas)
        {
            Debug.Log("[Social] Todos os personagens identificados!");
            OnAllNamed?.Invoke();
        }
        return true;
    }

    // ── Coleta de estátuas ────────────────────────────────────────────────────
    public void OnStatueCollected()
    {
        collectedCount++;
        GSM?.SetInt(KEY + "collected", collectedCount);

        Debug.Log($"[Social] Estátua coletada ({collectedCount}/{estatuas.Length}).");

        if (collectedCount >= estatuas.Length)
        {
            Debug.Log("[Social] Todas as peças coletadas! Etapa 2 disponível.");
            etapa = 2;
            GSM?.SetInt(KEY + "etapa", etapa);
            OnAllCollected?.Invoke();
        }
    }

    // ── Etapa 2 — Pirâmide ────────────────────────────────────────────────────
    public bool TryPlacePiece(PiramideNivelUI nivel, GlyphItem item)
    {
        if (etapa != 2) return false;

        bool acerto = nivel.TryPlacePiece(item);
        if (!acerto) return false;

        int idx = System.Array.IndexOf(niveis, nivel);
        if (idx >= 0)
        {
            GSM?.SetBool(KEY + "filled" + idx, true);
            GSM?.SetString(KEY + "pieceLabel" + idx, item.displayName);
        }

        bool todasPreenchidas = true;
        foreach (var n in niveis) if (!n.IsFilled) { todasPreenchidas = false; break; }

        if (todasPreenchidas)
        {
            Debug.Log("[Social] Pirâmide Social completa!");
            etapa = 3;
            GSM?.SetInt(KEY + "etapa", etapa);

            foreach (var n in niveis) n.PlayCompletionEffect();
            OnPuzzleCompleted?.Invoke();
            DarRecompensa();
        }
        return true;
    }

    private void DarRecompensa()
    {
        SocialUI.Instance?.ClosePanel();

        if (rewardWorldObject != null)
        {
            GSM?.SetBool(KEY_REWARD, true);
            rewardWorldObject.SetActive(true);
            Debug.Log("[Social] Amuleto da Ordem ativado na cena.");
        }

        if (completionDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(completionDialogue, null);

        SocialUI.Instance?.OnPuzzleComplete();
    }

    private void TryRestoreReward()
    {
        if (rewardWorldObject == null) return;
        if (GSM == null || !GSM.GetBool(KEY_REWARD)) return;

        var wc = rewardWorldObject.GetComponent<WorldClue>();
        bool jaColetado = wc != null && wc.itemToGive != null
                          && InventoryManager.Instance != null
                          && InventoryManager.Instance.HasItem(wc.itemToGive.itemID);

        if (!jaColetado)
            rewardWorldObject.SetActive(true);
    }
}
