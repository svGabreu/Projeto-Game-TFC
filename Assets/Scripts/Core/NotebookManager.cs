// gerencia as entradas do caderno no jogo, permitindo adicionar novas entradas e notificar ouvintes quando o caderno for atualizado.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NotebookManager : MonoBehaviour
{
    public static NotebookManager Instance { get; private set; } // instancia Singleton do NotebookManager

    [System.Serializable]
    public class NotebookEntry
    {
        public string entryID;
        public string title;
        public string hintText;
        public Sprite illustration;
    }

    private List<NotebookEntry> entries = new List<NotebookEntry>(); // Lista de entradas do caderno
    public UnityEvent OnNotebookUpdated = new UnityEvent(); // Evento para notificar mudanças no caderno

    private void Awake()
    {
        if (!ReferenceEquals(Instance, null) && Instance != this) // Verifica se já existe uma instância do NotebookManager
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null; 
    } 

    public void AddEntry(NotebookEntry entry) // Adiciona uma nova entrada ao caderno, se ainda não existir
    {
        if (entries.Exists(e => e.entryID == entry.entryID)) return;
        entries.Add(entry);
        OnNotebookUpdated.Invoke();
        Debug.Log($"Dica adicionada ao caderno: {entry.title}");
    }

    public List<NotebookEntry> GetAllEntries() => new List<NotebookEntry>(entries); // Retorna uma cópia da lista de entradas do caderno
    public bool HasEntry(string entryID) => entries.Exists(e => e.entryID == entryID); // Verifica se uma entrada específica existe no caderno
} 