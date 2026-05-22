// RioDaVidaPuzzle.cs
// Coloque em: Assets/Scripts/Puzzle/
// Coordena as duas etapas do Puzzle 2 (Casa do Rio da Vida).
//
// Etapa 1: jogador associa cada pergaminho ao quadro correto.
// Etapa 2: jogador coloca os quadros na ordem correta (Akhet→Peret→Shemu).
// Conclusão: libera o Amuleto da Vida.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RioDaVidaPuzzle : MonoBehaviour
{
    [Header("Os 3 Quadros")]
    public RioDaVidaQuadroUI[] quadros = new RioDaVidaQuadroUI[3];

    [Header("Recompensa")]
    public GlyphItem rewardItem;                // Amuleto da Vida
    public GameObject rewardWorldObject;        // objeto físico na cena (ativar ao concluir)

    [Header("Eventos")]
    public UnityEvent OnEtapa1Complete;         // dispara ao nomear todos os quadros
    public UnityEvent OnEtapa2Unlocked;         // dispara ao embaralhar e liberar Etapa 2
    public UnityEvent OnPuzzleCompleted;        // dispara ao ordenar corretamente

    // ---- Estado ----
    private int etapa = 1;
    private RioDaVidaQuadroUI quadroSelecionado = null;

    public int Etapa => etapa;

    // --------------------------------------------------------
    // Ciclo de vida — salva/restaura via GameStateManager
    // --------------------------------------------------------
    private void Start()
    {
        RestoreState();
    }

    private void OnDestroy()
    {
        SaveState();
    }

    // Prefixo único deste puzzle no GameStateManager
    private const string KEY = "rdv.";

    private void SaveState()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        gsm.SetInt(KEY + "etapa", etapa);
        for (int i = 0; i < quadros.Length && i < 3; i++)
        {
            gsm.SetBool  (KEY + "named"  + i, quadros[i].IsNamed);
            // scrollDisplayNames são salvos em TryAssignScroll
        }
    }

    private void RestoreState()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null || !gsm.HasKey(KEY + "etapa")) return;

        // Restaura quadros nomeados
        for (int i = 0; i < quadros.Length && i < 3; i++)
        {
            if (gsm.GetBool(KEY + "named" + i))
                quadros[i].RestoreNamed(gsm.GetString(KEY + "scrollName" + i));
        }

        etapa = gsm.GetInt(KEY + "etapa", 1);

        if (etapa >= 2)
        {
            foreach (var q in quadros) q.EnableEtapa2();
            EmbaralharQuadros();
        }

        if (etapa >= 3)
        {
            foreach (var q in quadros) q.PlayCompletionEffect();
            RioDaVidaUI.Instance?.OnPuzzleComplete();
        }

        Debug.Log($"[RioDaVida] Estado restaurado — Etapa {etapa}");
    }

    // --------------------------------------------------------
    // Etapa 1: tenta atribuir pergaminho ao quadro
    // Chamado por RioDaVidaUI.OnSlotClicked()
    // --------------------------------------------------------
    public void TryAssignScroll(RioDaVidaQuadroUI quadro, GlyphItem item)
    {
        if (etapa != 1) return;

        bool acerto = quadro.TryAssignScroll(item);
        if (acerto)
        {
            // Persiste o nome do scroll para restauração futura
            int idx = System.Array.IndexOf(quadros, quadro);
            if (GameStateManager.Instance != null && idx >= 0)
                GameStateManager.Instance.SetString(KEY + "scrollName" + idx, item.displayName);

            ChecarEtapa1();
        }
    }

    private void ChecarEtapa1()
    {
        foreach (var q in quadros)
            if (!q.IsNamed) return;

        // Todos nomeados corretamente
        Debug.Log("[RioDaVida] Etapa 1 completa!");
        OnEtapa1Complete?.Invoke();

        // Embaralha visualmente os quadros para a Etapa 2
        EmbaralharQuadros();

        etapa = 2;
        foreach (var q in quadros) q.EnableEtapa2();

        OnEtapa2Unlocked?.Invoke();
        Debug.Log("[RioDaVida] Etapa 2 desbloqueada — ordene os quadros!");
    }

    // Embaralha simplesmente trocando o 1º com o 3º
    private void EmbaralharQuadros()
    {
        if (quadros.Length < 3) return;
        int idxA = quadros[0].transform.GetSiblingIndex();
        int idxC = quadros[2].transform.GetSiblingIndex();
        quadros[0].transform.SetSiblingIndex(idxC);
        quadros[2].transform.SetSiblingIndex(idxA);
    }

    // --------------------------------------------------------
    // Etapa 2: jogador clica nos quadros para trocá-los
    // Chamado por RioDaVidaUI.OnQuadroClicked()
    // --------------------------------------------------------
    public void HandleQuadroClick(RioDaVidaQuadroUI quadro)
    {
        if (etapa != 2) return;

        if (quadroSelecionado == null)
        {
            // Primeira seleção
            quadroSelecionado = quadro;
            quadro.SetSelectionHighlight(true);
            Debug.Log($"[RioDaVida] Quadro selecionado: {quadro.name}");
        }
        else if (quadroSelecionado == quadro)
        {
            // Clicou no mesmo → deseleciona
            quadroSelecionado.SetSelectionHighlight(false);
            quadroSelecionado = null;
        }
        else
        {
            // Segunda seleção → troca posições
            TrocarQuadros(quadroSelecionado, quadro);
            quadroSelecionado.SetSelectionHighlight(false);
            quadroSelecionado = null;
            ChecarEtapa2();
        }
    }

    private void TrocarQuadros(RioDaVidaQuadroUI a, RioDaVidaQuadroUI b)
    {
        int idxA = a.transform.GetSiblingIndex();
        int idxB = b.transform.GetSiblingIndex();
        a.transform.SetSiblingIndex(idxB);
        b.transform.SetSiblingIndex(idxA);
        Debug.Log($"[RioDaVida] Trocou '{a.name}' ↔ '{b.name}'");
    }

    private void ChecarEtapa2()
    {
        // Ordena os quadros pelo sibling index atual
        var ordenados = new List<RioDaVidaQuadroUI>(quadros);
        ordenados.Sort((a, b) =>
            a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        // Verifica se a sequência de correctPosition é 1, 2, 3
        for (int i = 0; i < ordenados.Count; i++)
        {
            if (ordenados[i].CorrectPosition != i + 1)
                return; // ordem errada
        }

        // Ordem correta!
        Debug.Log("[RioDaVida] Puzzle completo! Ordem correta: Akhet → Peret → Shemu");
        etapa = 3;

        foreach (var q in quadros) q.PlayCompletionEffect();
        OnPuzzleCompleted?.Invoke();
        DarRecompensa();
    }

    private void DarRecompensa()
    {
        if (rewardItem != null)
        {
            InventoryManager.Instance.AddItem(rewardItem);
            Debug.Log($"[RioDaVida] Recompensa: {rewardItem.displayName}");
        }
        if (rewardWorldObject != null)
            rewardWorldObject.SetActive(true);

        RioDaVidaUI.Instance?.OnPuzzleComplete();
    }
}
