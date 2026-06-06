// WorldClue.cs — CORRIGIDO
// Assets/Scripts/Interaction/WorldClue.cs

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

    // Chave de persistência: usa entryID se definido, senão usa itemID, senão o nome do objeto
    private string SaveKey => "clue.collected." +
        (!string.IsNullOrEmpty(entryID) ? entryID :
         itemToGive != null ? itemToGive.itemID :
                                         gameObject.name);

    // --------------------------------------------------------
    private void Start()
    {
        // Adicione temporariamente no Start() do WorldClue, logo no início:
        Debug.Log($"[WorldClue] '{gameObject.name}' Start() | " +
                  $"GSM={GameStateManager.Instance != null} | " +
                  $"SaveKey='{SaveKey}' | " +
                  $"Coletado={GameStateManager.Instance?.GetBool(SaveKey)}");

        bool jaRegistrado = GameStateManager.Instance != null
                            && GameStateManager.Instance.GetBool(SaveKey);

        bool jaNoInventario = itemToGive != null
                              && InventoryManager.Instance != null
                              && InventoryManager.Instance.HasItem(itemToGive.itemID);

        if (jaRegistrado || jaNoInventario)
        {
            collected = true;
            Debug.Log($"[WorldClue] '{gameObject.name}' já coletado — removendo. " +
                      $"(GameState={jaRegistrado}, Inventário={jaNoInventario})");

            // CORREÇÃO: respeita destroyAfterCollect ao restaurar estado
            if (destroyAfterCollect)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    // --------------------------------------------------------
    public void Interact()
    {
        if (collected) return;

        if (ItemExamineUI.Instance != null)
        {
            ItemExamineUI.Instance.OpenExamine(itemToGive, hintText, this);
            return;
        }

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
                entryID = entryID,
                title = entryTitle,
                hintText = hintText,
                illustration = hintIllustration
            };
            NotebookManager.Instance.AddEntry(entry);
        }

        collected = true;

        // Persiste — sobrevive ao LoadScene
        GameStateManager.Instance?.SetBool(SaveKey, true);

        if (destroyAfterCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    public string GetInteractionPrompt() => interactionPrompt;
}