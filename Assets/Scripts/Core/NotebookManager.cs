using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// ============================================================
// NotebookManager.cs
// Singleton que gerencia as dicas/anotações do caderno.
// Uma entrada é adicionada quando o jogador interage com um
// objeto de pista no mundo (ex: papiro embaixo do camelo).
// ============================================================

public class NotebookManager : MonoBehaviour
{
    public static NotebookManager Instance { get; private set; }

    // Estrutura de uma entrada no caderno
    [System.Serializable]
    public class NotebookEntry
    {
        public string entryID;      // identificador único
        public string title;        // ex: "O Pássaro"
        public string hintText;     // ex: "O pássaro canta como o som 'A'"
        public Sprite illustration; // imagem opcional da dica
    }

    private List<NotebookEntry> entries = new List<NotebookEntry>();

    // A UI do caderno se inscreve aqui para atualizar quando
    // uma nova dica é adicionada.
    public UnityEvent OnNotebookUpdated = new UnityEvent();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --------------------------------------------------------
    // Adiciona uma entrada ao caderno.
    // Chamado pelos objetos de pista do mundo quando o jogador
    // interage com eles (IInteractable.Interact()).
    // --------------------------------------------------------
    public void AddEntry(NotebookEntry entry)
    {
        // Evita duplicatas
        if (entries.Exists(e => e.entryID == entry.entryID))
            return;

        entries.Add(entry);
        OnNotebookUpdated.Invoke();
        Debug.Log($"Dica adicionada ao caderno: {entry.title}");
    }

    // Retorna todas as entradas (para popular a UI do caderno)
    public List<NotebookEntry> GetAllEntries() => new List<NotebookEntry>(entries);

    public bool HasEntry(string entryID)
    {
        return entries.Exists(e => e.entryID == entryID);
    }
}
