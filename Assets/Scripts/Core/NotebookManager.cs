// NotebookManager.cs — CORRIGIDO
// Assets/Scripts/Core/NotebookManager.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NotebookManager : MonoBehaviour
{
    public static NotebookManager Instance { get; private set; }

    [System.Serializable]
    public class NotebookEntry
    {
        public string entryID;
        public string title;
        public string hintText;
        public Sprite illustration;
    }

    private List<NotebookEntry> entries = new List<NotebookEntry>();
    public UnityEvent OnNotebookUpdated = new UnityEvent();

    private void Awake()
    {
        if (!ReferenceEquals(Instance, null) && Instance != this)
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

    public void AddEntry(NotebookEntry entry)
    {
        if (entries.Exists(e => e.entryID == entry.entryID)) return;
        entries.Add(entry);
        OnNotebookUpdated.Invoke();
        Debug.Log($"Dica adicionada ao caderno: {entry.title}");
    }

    public List<NotebookEntry> GetAllEntries() => new List<NotebookEntry>(entries);
    public bool HasEntry(string entryID) => entries.Exists(e => e.entryID == entryID);
}