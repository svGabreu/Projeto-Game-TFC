// MuralUI.cs
// Coloque em: Assets/Scripts/Puzzle/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class MuralUI : MonoBehaviour
{
    [Header("Painel raiz")]
    public GameObject muralPanel;

    [Header("Referência ao MuralPuzzle")]
    public MuralPuzzle muralPuzzle;

    [Header("Container dos pares de slots")]
    public Transform slotPairsContainer;
    public GameObject slotPairUIPrefab;

    [Header("Inventário lateral (mini)")]
    public Transform miniInventoryContainer;
    public GameObject miniSlotPrefab;

    [Header("Mini-inventário dentro do mural")]
    public GameObject painelMiniInventario;   // PainelInventario_A ou _B

    [Header("Seletor de letras")]
    public LetterSelectorUI letterSelector;

    [Header("Fechar")]
    public string closePrompt = "Pressione ESC para fechar";

    [Header("Feedback")]
    public GameObject completionPanel; // overlay "Mural Concluído!" — começa desativado

    public static MuralUI CurrentOpen { get; private set; }

    private bool isOpen = false;
    private GlyphItem itemInHand = null;
    private GameObject selectedMiniSlotGO = null;

    private void Start()
    {
        if (muralPuzzle != null)
            muralPuzzle.OnMuralCompleted.AddListener(OnMuralCompleted);
    }

    private void OnDestroy()
    {
        if (muralPuzzle != null)
            muralPuzzle.OnMuralCompleted.RemoveListener(OnMuralCompleted);
    }

    private void Update()
    {
        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseMural();
    }

    private void OnMuralCompleted()
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);
    }

    public void OpenMural()
    {
        isOpen = true;
        CurrentOpen = this;
        if (muralPanel != null) muralPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Mostra painel de conclusão se o mural já estava resolvido
        if (completionPanel != null)
            completionPanel.SetActive(muralPuzzle != null && muralPuzzle.IsDone);

        ShowMiniInventory();
        RefreshMiniInventory();
    }

    public void CloseMural()
    {
        isOpen = false;
        if (CurrentOpen == this) CurrentOpen = null;
        if (muralPanel != null) muralPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Garante que seletor fecha junto
        if (letterSelector != null) letterSelector.ForceClose();
    }

    // Mostra o mini-inventário e esconde o seletor de letras
    public void ShowMiniInventory()
    {
        if (letterSelector != null) letterSelector.ForceClose();
        if (painelMiniInventario != null) painelMiniInventario.SetActive(true);
    }

    public bool IsLetterSelectorOpen()
    {
        return letterSelector != null && letterSelector.IsOpen();
    }

    public bool IsOpen() => isOpen;

    private void RefreshMiniInventory()
    {
        if (miniInventoryContainer == null) return;

        foreach (Transform child in miniInventoryContainer)
            Destroy(child.gameObject);

        List<GlyphItem> items = InventoryManager.Instance.GetGlyphObjects();

        foreach (GlyphItem item in items)
        {
            if (miniSlotPrefab == null) break;

            GameObject slotGO = Instantiate(miniSlotPrefab, miniInventoryContainer);

            Image icon = slotGO.GetComponentInChildren<Image>();
            if (icon != null && item.itemSprite != null)
                icon.sprite = item.itemSprite;

            TextMeshProUGUI label = slotGO.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = item.displayName;

            Button btn = slotGO.GetComponent<Button>();
            GlyphItem capturedItem = item;
            if (btn != null)
                btn.onClick.AddListener(() => OnMiniSlotClicked(capturedItem, slotGO));
        }
    }

    private void OnMiniSlotClicked(GlyphItem item, GameObject slotGO)
    {
        if (selectedMiniSlotGO != null)
        {
            Image prevImg = selectedMiniSlotGO.GetComponent<Image>();
            if (prevImg != null) prevImg.color = Color.white;
        }

        itemInHand = item;
        selectedMiniSlotGO = slotGO;

        Image img = slotGO.GetComponent<Image>();
        if (img != null) img.color = Color.yellow;

        Debug.Log($"Item em mão: {item.displayName}");
    }

    public void TryPlaceItemInSlot(MuralSlotPairUI pairUI)
    {
        if (itemInHand == null)
        {
            Debug.Log("Nenhum item selecionado. Clique em um item do inventário primeiro.");
            return;
        }

        pairUI.TryFillSilhouette(itemInHand);
        itemInHand = null;
        selectedMiniSlotGO = null;
        RefreshMiniInventory();
    }

    public void OpenLetterSelectorFor(MuralSlotPairUI pairUI)
    {
        if (letterSelector == null) return;

        // Esconde o mini-inventário — o seletor ocupa o mesmo espaço no VLG
        if (painelMiniInventario != null) painelMiniInventario.SetActive(false);
        letterSelector.OpenForMuralPair(pairUI, this);
    }
}