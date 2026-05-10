// InventoryUI.cs
// Coloque em: Assets/Scripts/UI/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Controla o painel de inventário.
// - Abre/fecha com a tecla I
// - Atualiza automaticamente quando o inventário muda
// - Mantém rastro do item selecionado para passar ao mural
public class InventoryUI : MonoBehaviour
{
    [Header("Referências")]
    public GameObject inventoryPanel;       // painel raiz (ativar/desativar)
    public Transform slotsContainer;        // onde os slots são instanciados
    public GameObject inventorySlotPrefab;  // prefab do slot

    // Item atualmente selecionado pelo jogador
    private GlyphItem selectedItem;
    private List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private bool isOpen = false;

    // Singleton leve para o MuralUI acessar o item selecionado
    public static InventoryUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        ClosePanel();
    }

    private void Start()
    {
        // Inscreve para atualizar sempre que o inventário mudar
        InventoryManager.Instance.OnInventoryChanged.AddListener(RefreshUI);
        RefreshUI();
    }

    private void Update()
    {
        // Só abre/fecha se o mural estiver fechado
        MuralUI mural = FindObjectOfType<MuralUI>();
        if (mural != null && mural.IsOpen()) return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (isOpen) ClosePanel();
            else OpenPanel();
        }
    }

    // --------------------------------------------------------
    // Reconstrói os slots a partir dos itens no InventoryManager
    // --------------------------------------------------------
    public void RefreshUI()
    {
        // Limpa slots antigos
        foreach (Transform child in slotsContainer)
            Destroy(child.gameObject);

        slots.Clear();
        selectedItem = null;

        List<GlyphItem> items = InventoryManager.Instance.GetAllItems();

        foreach (GlyphItem item in items)
        {
            if (inventorySlotPrefab == null) break;

            GameObject slotGO = Instantiate(inventorySlotPrefab, slotsContainer);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                slotUI.Setup(item, this);
                slots.Add(slotUI);
            }
        }
    }

    // --------------------------------------------------------
    // Chamado por InventorySlotUI.OnClick()
    // --------------------------------------------------------
    public void SelectItem(GlyphItem item)
    {
        selectedItem = item;

        // Atualiza highlight em todos os slots
        foreach (InventorySlotUI slot in slots)
            slot.SetSelected(slot.GetItem() == item);

        Debug.Log($"Item selecionado: {item.displayName}");
    }

    // Retorna o item selecionado (usado pelo MuralUI)
    public GlyphItem GetSelectedItem() => selectedItem;

    // Limpa a seleção (chamado após encaixar no mural)
    public void ClearSelection()
    {
        selectedItem = null;
        foreach (InventorySlotUI slot in slots)
            slot.SetSelected(false);
    }

    public void OpenPanel()
    {
        isOpen = true;
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        // Libera o cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshUI();
    }

    public void ClosePanel()
    {
        isOpen = false;
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        // Trava o cursor de volta
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsOpen() => isOpen;
}
