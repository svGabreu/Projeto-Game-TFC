// SocialPuzzleTrigger.cs
// Coloque em: Assets/Scripts/Interaction/
//
// Dois GameObjects com Collider (trigger) na cena:
//   SocialMesaTrigger   — perto da mesa    → abre painel de identificação (Etapa 1)
//   SocialMuralTrigger  — perto da pirâmide → abre painel da pirâmide (Etapa 2)
//
// Use o enum TriggerType para configurar no Inspector.

using UnityEngine;

public class SocialPuzzleTrigger : MonoBehaviour, IInteractable
{
    public enum TriggerType { Mesa, Mural }

    [Header("Configuração")]
    public TriggerType tipo = TriggerType.Mesa;

    [Tooltip("Texto mostrado quando o jogador se aproxima")]
    [SerializeField] private string promptMesa   = "Pressione E para examinar a mesa";
    [SerializeField] private string promptMural  = "Pressione E para examinar o mural";
    [SerializeField] private string promptBloqueado = "Colete todas as peças da mesa primeiro";

    // --------------------------------------------------------
    public void Interact()
    {
        if (SocialUI.Instance == null || SocialPuzzle.Instance == null) return;
        if (SocialUI.Instance.IsOpen()) return;

        if (tipo == TriggerType.Mesa)
        {
            // Mesa sempre acessível na Etapa 1 (também na 2 para consulta visual)
            SocialUI.Instance.OpenPanel(SocialUI.PanelMode.Mesa);
        }
        else // Mural
        {
            if (!SocialPuzzle.Instance.AllCollected && SocialPuzzle.Instance.Etapa < 2)
            {
                // Feedback: peças ainda não coletadas
                Debug.Log("[Social] Colete todas as peças da mesa antes de usar o mural.");
                return;
            }
            SocialUI.Instance.OpenPanel(SocialUI.PanelMode.Mural);
        }
    }

    public string GetInteractionPrompt()
    {
        if (SocialPuzzle.Instance == null) return "";

        if (tipo == TriggerType.Mesa)
            return promptMesa;

        // Mural
        return (SocialPuzzle.Instance.AllCollected || SocialPuzzle.Instance.Etapa >= 2)
            ? promptMural
            : promptBloqueado;
    }
}
