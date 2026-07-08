
using UnityEngine;

public class SocialStatue : MonoBehaviour, IInteractable
{
    [Header("Configuração")]
    public GlyphItem pieceItem;               // piece_farao, piece_escribas, etc.
    [SerializeField] private string collectPrompt = "Pressione E para coletar";

    // ---- Estado ----
    private bool unlocked   = false;
    private bool collected  = false;

    public bool IsCollected => collected;
    public bool IsUnlocked  => unlocked;

    private string GsmKey => "soc.statue_" + (pieceItem != null ? pieceItem.itemID : name);

    // --------------------------------------------------------
    // Awake: restaura estado antes que SocialPuzzle.Start() rode
    // --------------------------------------------------------
    private void Awake()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null || pieceItem == null) return;

        if (gsm.GetBool(GsmKey))
        {
            collected = true;
            unlocked  = true;
            gameObject.SetActive(false);
        }
    }

    // --------------------------------------------------------
    // Chamado por SocialPecaUI quando o nome é identificado corretamente
    // --------------------------------------------------------
    public void Unlock()
    {
        if (unlocked) return;
        unlocked = true;
        Debug.Log($"[SocialStatue] '{pieceItem?.displayName}' desbloqueada para coleta!");
    }

    // --------------------------------------------------------
    // IInteractable — chamado ao pressionar E
    // --------------------------------------------------------
    public void Interact()
    {
        if (!unlocked || collected) return;

        collected = true;
        GameStateManager.Instance?.SetBool(GsmKey, true);

        if (pieceItem != null)
            InventoryManager.Instance.AddItem(pieceItem);

        Debug.Log($"[SocialStatue] Coletado: {pieceItem?.displayName}");

        gameObject.SetActive(false);
        SocialPuzzle.Instance?.OnStatueCollected();
    }

    public string GetInteractionPrompt()
        => (unlocked && !collected) ? collectPrompt : "";
}
