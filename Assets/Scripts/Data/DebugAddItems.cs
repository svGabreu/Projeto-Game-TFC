//DebugAddItems.cs — REMOVER ANTES DA BUILD FINAL
using UnityEngine;

public class DebugAddItems : MonoBehaviour
{
    [Header("Arraste os 3 GlyphItems dos amuletos")]
    public GlyphItem amuleto1;
    public GlyphItem amuleto2;
    public GlyphItem amuleto3;

    private void Start()
    {
        if (amuleto1 != null) InventoryManager.Instance.AddItem(amuleto1);
        if (amuleto2 != null) InventoryManager.Instance.AddItem(amuleto2);
        if (amuleto3 != null) InventoryManager.Instance.AddItem(amuleto3);
    }
}