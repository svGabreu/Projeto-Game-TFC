// WorldClue.cs
// Coloque em: Assets/Scripts/Interaction/

using UnityEngine;

public class WorldClue : MonoBehaviour, IInteractable
{
    [Header("Item para o Inventário")]
    public GlyphItem itemToGive;

    [Header("Dica para o Caderno")]
    public string entryID;
    public string entryTitle;
    [TextArea(2, 5)]
    public string hintText;
    public Sprite hintIllustration;

    [Header("Comportamento após coleta")]
    public bool destroyAfterCollect = true;
    public string interactionPrompt = "Pressione E para examinar";

    private bool collected = false;

    // --------------------------------------------------------
    // E pressionado pelo jogador
    // --------------------------------------------------------
    public void Interact()
    {
        if (collected) return;

        // Se o painel de exame existir, abre preview antes de coletar
        if (ItemExamineUI.Instance != null)
        {
            ItemExamineUI.Instance.OpenExamine(itemToGive, hintText, this);
            return;
        }

        // Fallback: coleta imediata (sem painel de exame na cena)
        ConfirmCollect();
    }

    // --------------------------------------------------------
    // Chamado pelo ItemExamineUI ao clicar em "Coletar"
    // --------------------------------------------------------
    public void ConfirmCollect()
    {
        if (collected) return;

        if (itemToGive != null)
        {
            bool added = InventoryManager.Instance.AddItem(itemToGive);

            // Se não foi adicionado, verifica o motivo:
            // • Item já está no inventário → prossegue (scroll some normalmente)
            // • Inventário genuinamente cheio → aborta (scroll fica no chão)
            if (!added && !InventoryManager.Instance.HasItem(itemToGive.itemID))
            {
                Debug.Log("[WorldClue] Inventário cheio — não foi possível coletar.");
                return;
            }
        }

        if (!string.IsNullOrEmpty(hintText) && NotebookManager.Instance != null)
        {
            NotebookManager.NotebookEntry entry = new NotebookManager.NotebookEntry
            {
                entryID      = entryID,
                title        = entryTitle,
                hintText     = hintText,
                illustration = hintIllustration
            };
            NotebookManager.Instance.AddEntry(entry);
        }

        collected = true;

        if (destroyAfterCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    public string GetInteractionPrompt() => interactionPrompt;
}
