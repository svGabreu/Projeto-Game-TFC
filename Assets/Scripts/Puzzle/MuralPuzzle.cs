// MuralPuzzle.cs
// Coloque em: Assets/Scripts/Puzzle/
// Versão atualizada — usa MuralSlotPairUI

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MuralPuzzle : MonoBehaviour
{
    [Header("Pares do Mural")]
    public MuralSlotPairUI[] pairs;

    [Header("ID do Mural")]
    public string muralID;

    [Header("Eventos")]
    public UnityEvent OnMuralCompleted = new UnityEvent();

    private HashSet<string> completedPairs = new HashSet<string>();
    private bool muralDone = false;
    public bool IsDone => muralDone;

    // Prefixo de chave: "mural.{muralID}."
    private string KEY => "mural." + muralID + ".";
    private GameStateManager GSM => GameStateManager.Instance;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────
    private void Start()
    {
        foreach (MuralSlotPairUI pair in pairs)
            pair.OnPairCompleted.AddListener(OnPairCompleted);

        RestoreState();
    }

    // ── Restauração ───────────────────────────────────────────────────────────
    private void RestoreState()
    {
        if (GSM == null || !GSM.HasKey(KEY + "done")) return;

        muralDone = GSM.GetBool(KEY + "done");

        // Restaura cada par concluído
        for (int i = 0; i < pairs.Length; i++)
        {
            string pairKey = KEY + "pair." + pairs[i].pairID;
            if (GSM.GetBool(pairKey))
            {
                completedPairs.Add(pairs[i].pairID);
                pairs[i].RestoreCompleted();
            }
        }

        if (muralDone)
        {
            Debug.Log($"[MuralPuzzle] '{muralID}' restaurado como concluído.");
            OnMuralCompleted.Invoke();
        }
        else
        {
            Debug.Log($"[MuralPuzzle] '{muralID}' restaurado — {completedPairs.Count}/{pairs.Length} pares.");
        }
    }

    // ── Callback quando um par é concluído ────────────────────────────────────
    private void OnPairCompleted(string pairID)
    {
        if (muralDone) return;

        completedPairs.Add(pairID);

        // Salva em tempo real
        GSM?.SetBool(KEY + "pair." + pairID, true);

        Debug.Log($"Mural '{muralID}': {completedPairs.Count}/{pairs.Length} pares completos.");

        if (completedPairs.Count >= pairs.Length)
        {
            muralDone = true;
            GSM?.SetBool(KEY + "done", true);

            Debug.Log($"Mural '{muralID}' concluído!");
            OnMuralCompleted.Invoke();
        }
    }

    public float GetProgress()
    {
        if (pairs.Length == 0) return 0f;
        return (float)completedPairs.Count / pairs.Length;
    }
}