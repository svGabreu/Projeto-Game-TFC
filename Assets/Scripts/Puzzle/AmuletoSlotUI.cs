// AmuletoSlotUI.cs
// Coloque em: Assets/Scripts/Puzzle/
// Representa UM slot de amuleto no painel da porta da pirâmide.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmuletoSlotUI : MonoBehaviour
{
    [Header("Configuração")]
    public string expectedItemID;   // ex: "amuleto_hieroglifos"
    public string slotLabel;        // ex: "Amuleto dos Hieróglifos"

    [Header("Visual")]
    public Image slotBackground;
    public Image itemIcon;
    public TextMeshProUGUI slotText;
    public Button slotButton;

    [Header("Cores")]
    public Color defaultColor = new Color(0.2f, 0.15f, 0.05f, 0.8f);
    public Color filledColor  = new Color(1f,   0.85f, 0.3f,  0.9f);
    public Color wrongColor   = new Color(0.8f, 0.2f,  0.2f,  0.8f);

    // ── Estado ────────────────────────────────────────────────────────────────
    private bool isFilled = false;
    public bool IsFilled => isFilled;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────
    private void Start()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(OnClicked);

        // Só reseta visual se o slot não foi restaurado antes do Start() disparar
        if (!isFilled)
        {
            if (slotBackground != null) slotBackground.color = defaultColor;
            if (slotText       != null) slotText.text        = slotLabel;
            if (itemIcon       != null) itemIcon.gameObject.SetActive(false);
        }
    }

    // ── Clique no slot ────────────────────────────────────────────────────────
    private void OnClicked()
    {
        if (isFilled) return;
        AmuletoPainelUI.Instance?.OnSlotClicked(this);
    }

    // ── Preencher slot ────────────────────────────────────────────────────────
    public bool TryFill(GlyphItem item)
    {
        if (isFilled) return false;

        isFilled = true;

        if (slotBackground != null) slotBackground.color   = filledColor;
        if (slotText       != null) slotText.text          = item.displayName;
        if (slotButton     != null) slotButton.interactable = false;

        if (itemIcon != null && item.itemSprite != null)
        {
            itemIcon.sprite = item.itemSprite;
            itemIcon.color  = Color.white;
            itemIcon.gameObject.SetActive(true);
        }

        InventoryManager.Instance.RemoveItem(item.itemID);

        Debug.Log($"[AmuletoSlot] '{slotLabel}' preenchido com {item.displayName}");
        return true;
    }

    // ── Flash de erro ─────────────────────────────────────────────────────────
    public void FlashWrong()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (slotBackground != null) slotBackground.color = wrongColor;
        yield return new WaitForSecondsRealtime(0.8f);
        if (slotBackground != null) slotBackground.color = defaultColor;
    }

    // ── Restaurar estado (ao reentrar na cena) ────────────────────────────────
    public void RestoreFilled(string displayName)
    {
        isFilled = true;
        if (slotBackground != null) slotBackground.color    = filledColor;
        if (slotText       != null) slotText.text           = displayName;
        if (slotButton     != null) slotButton.interactable = false;
    }
}
