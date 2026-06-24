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
    public GameObject rewardWorldObject;    // WorldClue com Amuleto da Vida — começa desativado
    public DialogueData completionDialogue; // opcional

    [Header("Eventos")]
    public UnityEvent OnEtapa1Complete;
    public UnityEvent OnEtapa2Unlocked;
    public UnityEvent OnPuzzleCompleted;

    private int etapa = 1;
    private RioDaVidaQuadroUI quadroSelecionado = null;

    public int Etapa => etapa;

    private const string KEY = "rdv.";
    private const string KEY_ETAPA = "rdv.etapa";

    private void Start()
    {
        RestoreState();
    }

    private void SaveEtapa()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetInt(KEY_ETAPA, etapa);
        else
            Debug.LogError("[RioDaVida] SaveEtapa FALHOU - GSM null");
    }

    private void SaveNamed(int i)
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetBool(KEY + "named" + i, quadros[i].IsNamed);
            // CORRECAO: salva a etapa atual junto com o primeiro named
            // para que RestoreState() encontre a chave rdv.etapa mesmo
            // quando o jogador sai antes de completar a etapa 1
            GameStateManager.Instance.SetInt(KEY_ETAPA, etapa);
        }
        else
        {
            Debug.LogError("[RioDaVida] SaveNamed FALHOU - GSM null");
        }
    }

    private void SaveScrollName(int i, string name)
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetString(KEY + "scrollName" + i, name);
    }

    private void SaveOrdem()
    {
        if (GameStateManager.Instance == null) return;
        for (int i = 0; i < quadros.Length; i++)
            GameStateManager.Instance.SetInt(KEY + "siblingIdx" + i, quadros[i].transform.GetSiblingIndex());
    }

    private void RestoreState()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null || !gsm.HasKey(KEY_ETAPA))
        {
            Debug.Log("[RioDaVida] RestoreState - sem estado salvo.");
            return;
        }

        for (int i = 0; i < quadros.Length && i < 3; i++)
        {
            if (gsm.GetBool(KEY + "named" + i))
                quadros[i].RestoreNamed(gsm.GetString(KEY + "scrollName" + i));
        }

        etapa = gsm.GetInt(KEY_ETAPA, 1);

        if (etapa >= 2)
        {
            foreach (var q in quadros) q.EnableEtapa2();

            if (gsm.HasKey(KEY + "siblingIdx0"))
            {
                for (int i = 0; i < quadros.Length; i++)
                {
                    int savedIdx = gsm.GetInt(KEY + "siblingIdx" + i, i);
                    quadros[i].transform.SetSiblingIndex(savedIdx);
                }
            }
            else
            {
                EmbaralharQuadros();
            }
        }

        if (etapa >= 3)
        {
            foreach (var q in quadros) q.PlayCompletionEffect();
            RioDaVidaUI.Instance?.OnPuzzleComplete();
        }

        Debug.Log("[RioDaVida] Estado restaurado - Etapa " + etapa);
    }

    public void TryAssignScroll(RioDaVidaQuadroUI quadro, GlyphItem item)
    {
        if (etapa != 1) return;

        bool acerto = quadro.TryAssignScroll(item);
        if (!acerto) return;

        int idx = System.Array.IndexOf(quadros, quadro);
        if (idx >= 0)
        {
            SaveNamed(idx);
            SaveScrollName(idx, item.displayName);
        }

        ChecarEtapa1();
    }

    private void ChecarEtapa1()
    {
        foreach (var q in quadros)
            if (!q.IsNamed) return;

        Debug.Log("[RioDaVida] Etapa 1 completa!");
        OnEtapa1Complete?.Invoke();
        EmbaralharQuadros();

        etapa = 2;
        foreach (var q in quadros) q.EnableEtapa2();

        SaveEtapa();
        SaveOrdem();

        OnEtapa2Unlocked?.Invoke();
    }

    private void EmbaralharQuadros()
    {
        if (quadros.Length < 3) return;
        int idxA = quadros[0].transform.GetSiblingIndex();
        int idxC = quadros[2].transform.GetSiblingIndex();
        quadros[0].transform.SetSiblingIndex(idxC);
        quadros[2].transform.SetSiblingIndex(idxA);
    }

    public void HandleQuadroClick(RioDaVidaQuadroUI quadro)
    {
        if (etapa != 2) return;

        if (quadroSelecionado == null)
        {
            quadroSelecionado = quadro;
            quadro.SetSelectionHighlight(true);
        }
        else if (quadroSelecionado == quadro)
        {
            quadroSelecionado.SetSelectionHighlight(false);
            quadroSelecionado = null;
        }
        else
        {
            TrocarQuadros(quadroSelecionado, quadro);
            quadroSelecionado.SetSelectionHighlight(false);
            quadroSelecionado = null;
            SaveOrdem();
            ChecarEtapa2();
        }
    }

    private void TrocarQuadros(RioDaVidaQuadroUI a, RioDaVidaQuadroUI b)
    {
        int idxA = a.transform.GetSiblingIndex();
        int idxB = b.transform.GetSiblingIndex();
        a.transform.SetSiblingIndex(idxB);
        b.transform.SetSiblingIndex(idxA);
    }

    private void ChecarEtapa2()
    {
        var ordenados = new List<RioDaVidaQuadroUI>(quadros);
        ordenados.Sort((a, b) =>
            a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        for (int i = 0; i < ordenados.Count; i++)
            if (ordenados[i].CorrectPosition != i + 1) return;

        Debug.Log("[RioDaVida] Puzzle completo!");
        etapa = 3;
        SaveEtapa();

        foreach (var q in quadros) q.PlayCompletionEffect();
        OnPuzzleCompleted?.Invoke();
        DarRecompensa();
    }

    private void DarRecompensa()
    {
        RioDaVidaUI.Instance?.ClosePanel();

        if (rewardWorldObject != null)
        {
            rewardWorldObject.SetActive(true);
            Debug.Log("[RioDaVida] Amuleto da Vida ativado na cena.");
        }

        if (completionDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(completionDialogue, null);

        RioDaVidaUI.Instance?.OnPuzzleComplete();
    }
}