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

    [Header("Eventos — conecte aqui a abertura da tranca")]
    public UnityEvent OnMuralCompleted = new UnityEvent();

    private HashSet<string> completedPairs = new HashSet<string>();
    private bool muralDone = false;

    private void Start()
    {
        foreach (MuralSlotPairUI pair in pairs)
            pair.OnPairCompleted.AddListener(OnPairCompleted);
    }

    private void OnPairCompleted(string pairID)
    {
        if (muralDone) return;

        completedPairs.Add(pairID);
        Debug.Log($"Mural '{muralID}': {completedPairs.Count}/{pairs.Length} pares completos.");

        if (completedPairs.Count >= pairs.Length)
        {
            muralDone = true;
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
