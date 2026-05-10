using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// ============================================================
// InventoryManager.cs
// Singleton que gerencia os itens coletados pelo jogador.
// Coloque este script num GameObject chamado "GameManager"
// na cena e marque como DontDestroyOnLoad se quiser persistir
// entre cenas.
// ============================================================

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Configuração")]
    public int maxSlots = 12;

    // Lista de itens atualmente no inventário
    private List<GlyphItem> items = new List<GlyphItem>();

    // Evento disparado sempre que o inventário muda.
    // A UI do inventário pode se inscrever aqui para atualizar.
    public UnityEvent OnInventoryChanged = new UnityEvent();

    private void Awake()
    {
        // Garante que só existe uma instância
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // --------------------------------------------------------
    // Adiciona um item. Retorna false se o inventário estiver
    // cheio ou o item já existir.
    // --------------------------------------------------------
    public bool AddItem(GlyphItem item)
    {
        if (items.Count >= maxSlots)
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

    // --------------------------------------------------------
    // Remove um item pelo ID (usado quando o mural consome o
    // item ao ser encaixado no slot).
    // --------------------------------------------------------
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

    // --------------------------------------------------------
    // Verifica se o jogador possui um item específico.
    // --------------------------------------------------------
    public bool HasItem(string itemID)
    {
        return items.Exists(i => i.itemID == itemID);
    }

    // --------------------------------------------------------
    // Retorna todos os itens do tipo GlyphObject — esses são
    // os que podem ser encaixados nas silhuetas do mural.
    // --------------------------------------------------------
    public List<GlyphItem> GetGlyphObjects()
    {
        return items.FindAll(i => i.itemType == GlyphItemType.GlyphObject);
    }

    // Retorna cópia da lista completa (para a UI do inventário)
    public List<GlyphItem> GetAllItems() => new List<GlyphItem>(items);
}
