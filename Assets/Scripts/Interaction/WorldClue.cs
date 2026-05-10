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

    public void Interact()
    {
        if (collected) return;

        if (itemToGive != null)
        {
            bool added = InventoryManager.Instance.AddItem(itemToGive);
            if (!added) return;
        }

        if (!string.IsNullOrEmpty(hintText))
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
