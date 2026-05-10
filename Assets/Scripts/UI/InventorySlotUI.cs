// InventorySlotUI.cs
// Coloque em: Assets/Scripts/UI/

using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Representa um slot visual no painel de inventário.
// Cada slot exibe o sprite e o nome de um GlyphItem.
public class InventorySlotUI : MonoBehaviour
{
    [Header("Referências visuais")]
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public Image selectionHighlight; // borda amarela quando selecionado

    private GlyphItem item;
    private InventoryUI inventoryUI;

    public void Setup(GlyphItem glyphItem, InventoryUI ui)
    {
        item        = glyphItem;
        inventoryUI = ui;

        if (itemIcon != null)
        {
            itemIcon.sprite  = glyphItem.itemSprite;
            // Se não tiver sprite ainda, usa cor sólida como placeholder
            itemIcon.color   = glyphItem.itemSprite != null ? Color.white : Color.gray;
        }

        if (itemName != null)
            itemName.text = glyphItem.displayName;

        SetSelected(false);
    }

    // Chamado pelo botão do slot (configure no Inspector do Prefab)
    public void OnClick()
    {
        inventoryUI.SelectItem(item);
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.gameObject.SetActive(selected);
    }

    public GlyphItem GetItem() => item;
}
