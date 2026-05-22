// SocialStatue.cs
// Coloque em: Assets/Scripts/Interaction/
// Coloque este script em cada estátua 3D da mesa na Casa_Social.
// A estátua começa bloqueada. Quando o nome correto é identificado
// no painel, Unlock() é chamado e o jogador pode pressionar E para coletar.

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

    // --------------------------------------------------------
    // Chamado por SocialPecaUI quando o nome é identificado corretamente
    // --------------------------------------------------------
    public void Unlock()
    {
        if (unlocked) return;
        unlocked = true;
        Debug.Log($"[SocialStatue] '{pieceItem?.displayName}' desbloqueada para coleta!");

        // Efeito visual opcional (ex: outline, brilho)
        // Se quiser, ative um componente de highlight aqui
    }

    // --------------------------------------------------------
    // IInteractable — chamado ao pressionar E
    // --------------------------------------------------------
    public void Interact()
    {
        if (!unlocked || collected) return;

        collected = true;
        if (pieceItem != null)
            InventoryManager.Instance.AddItem(pieceItem);

        Debug.Log($"[SocialStatue] Coletado: {pieceItem?.displayName}");

        // Remove a estátua da mesa
        gameObject.SetActive(false);

        // Notifica o puzzle para checar se todas as peças foram coletadas
        SocialPuzzle.Instance?.OnStatueCollected();
    }

    public string GetInteractionPrompt()
        => (unlocked && !collected) ? collectPrompt : "";
}
