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

    [Header("Estátuas 3D na Mesa (mesma ordem das pecas[])")]
    public SocialStatue[] estatuas = new SocialStatue[5];

    [Header("Etapa 2 — Pirâmide Hierárquica (5 níveis)")]
    public PiramideNivelUI[] niveis = new PiramideNivelUI[5];

    [Header("Recompensa")]
    public GlyphItem rewardItem;            // Amuleto da Ordem
    public GameObject rewardWorldObject;    // objeto na cena ativado ao concluir

    [Header("Eventos")]
    public UnityEvent OnAllNamed;           // todas as peças identificadas no painel
    public UnityEvent OnAllCollected;       // todas as estátuas coletadas → Etapa 2 disponível
    public UnityEvent OnPuzzleCompleted;    // pirâmide completa

    // ---- Estado ----
    private int etapa = 1;          // 1 = identificação / 2 = pirâmide / 3 = concluído
    private int collectedCount = 0;
    public int Etapa => etapa;
    public bool AllCollected => collectedCount >= estatuas.Length;

    // ---- Persistência ----
    private const string KEY = "soc.";

    // --------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()    => RestoreState();
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SaveState();
    }

    // --------------------------------------------------------
    private void SaveState()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        gsm.SetInt(KEY + "etapa",     etapa);
        gsm.SetInt(KEY + "collected", collectedCount);

        for (int i = 0; i < pecas.Length; i++)
            gsm.SetBool(KEY + "named" + i, pecas[i].IsNamed);

        for (int i = 0; i < niveis.Length; i++)
            gsm.SetBool(KEY + "filled" + i, niveis[i].IsFilled);
    }

    private void RestoreState()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null || !gsm.HasKey(KEY + "etapa")) return;

        etapa          = gsm.GetInt(KEY + "etapa",     1);
        collectedCount = gsm.GetInt(KEY + "collected", 0);

        // Restaura peças identificadas
        for (int i = 0; i < pecas.Length; i++)
            if (gsm.GetBool(KEY + "named" + i))
                pecas[i].RestoreNamed(gsm.GetString(KEY + "nameLabel" + i));

        // Restaura níveis preenchidos
        for (int i = 0; i < niveis.Length; i++)
            if (gsm.GetBool(KEY + "filled" + i))
                niveis[i].RestoreFilled(gsm.GetString(KEY + "pieceLabel" + i));

        if (etapa >= 3)
            foreach (var n in niveis) n.PlayCompletionEffect();

        Debug.Log($"[Social] Estado restaurado — Etapa {etapa}, {collectedCount} coletadas.");
    }

    // --------------------------------------------------------
    // Etapa 1 — Identificação no painel
    // --------------------------------------------------------
    public void TryAssignName(SocialPecaUI peca, GlyphItem item)
    {
        if (etapa != 1) return;

        bool acerto = peca.TryAssignName(item);
        if (!acerto) return;

        // Persiste
        int idx = System.Array.IndexOf(pecas, peca);
        if (idx >= 0 && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetBool  (KEY + "named"     + idx, true);
            GameStateManager.Instance.SetString(KEY + "nameLabel" + idx, item.displayName);
        }

        // Checa se todas foram nomeadas
        bool todasNomeadas = true;
        foreach (var p in pecas) if (!p.IsNamed) { todasNomeadas = false; break; }

        if (todasNomeadas)
        {
            Debug.Log("[Social] Todos os personagens identificados! Colete as estátuas.");
            OnAllNamed?.Invoke();
        }
    }

    // --------------------------------------------------------
    // Chamado por SocialStatue ao ser coletada com E
    // --------------------------------------------------------
    public void OnStatueCollected()
    {
        collectedCount++;
        GameStateManager.Instance?.SetInt(KEY + "collected", collectedCount);

        Debug.Log($"[Social] Estátua coletada ({collectedCount}/{estatuas.Length}).");

        if (collectedCount >= estatuas.Length)
        {
            Debug.Log("[Social] Todas as peças coletadas! Etapa 2 disponível.");
            etapa = 2;
            GameStateManager.Instance?.SetInt(KEY + "etapa", etapa);
            OnAllCollected?.Invoke();
        }
    }

    // --------------------------------------------------------
    // Etapa 2 — Pirâmide
    // --------------------------------------------------------
    public void TryPlacePiece(PiramideNivelUI nivel, GlyphItem item)
    {
        if (etapa != 2) return;

        bool acerto = nivel.TryPlacePiece(item);
        if (!acerto) return;

        int idx = System.Array.IndexOf(niveis, nivel);
        if (idx >= 0 && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetBool  (KEY + "filled"     + idx, true);
            GameStateManager.Instance.SetString(KEY + "pieceLabel" + idx, item.displayName);
        }

        // Checa conclusão
        bool todasPreenchidas = true;
        foreach (var n in niveis) if (!n.IsFilled) { todasPreenchidas = false; break; }

        if (todasPreenchidas)
        {
            Debug.Log("[Social] Pirâmide Social completa!");
            etapa = 3;
            GameStateManager.Instance?.SetInt(KEY + "etapa", etapa);

            foreach (var n in niveis) n.PlayCompletionEffect();
            OnPuzzleCompleted?.Invoke();
            DarRecompensa();
        }
    }

    private void DarRecompensa()
    {
        if (rewardItem != null)
        {
            InventoryManager.Instance.AddItem(rewardItem);
            Debug.Log($"[Social] Recompensa: {rewardItem.displayName}");
        }
        if (rewardWorldObject != null)
            rewardWorldObject.SetActive(true);

        SocialUI.Instance?.OnPuzzleComplete();
    }
}
