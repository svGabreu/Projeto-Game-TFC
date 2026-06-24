using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Configuração")]
    [Tooltip("Limite de itens. 0 = sem limite.")]
    public int maxSlots = 0;

    private List<GlyphItem> items = new List<GlyphItem>();
    public UnityEvent OnInventoryChanged = new UnityEvent();

    public GlyphItem GetItemByID(string itemID)
    {
        return items.Find(i => i.itemID == itemID);
    }

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

    public bool AddItem(GlyphItem item)
    {
        if (maxSlots > 0 && items.Count >= maxSlots)
        {
            Debug.Log("Inventário cheio!");
            return false;
        }
        if (items.Contains(item))
        {
            Debug.Log($"Item '{item.displayName}' já está no inventário.");
            return false;
        }
        items.Add(item);
        OnInventoryChanged.Invoke();
        Debug.Log($"Item adicionado: {item.displayName}");
        return true;
    }

    public bool RemoveItem(string itemID)
    {
        GlyphItem found = items.Find(i => i.itemID == itemID);
        if (found != null)
        {
            items.Remove(found);
            OnInventoryChanged.Invoke();
            return true;
        }
        return false;
    }

    public bool HasItem(string itemID) => items.Exists(i => i.itemID == itemID);
    public List<GlyphItem> GetGlyphObjects() => items.FindAll(i => i.itemType == GlyphItemType.GlyphObject);
    public List<GlyphItem> GetAllItems() => new List<GlyphItem>(items);
}