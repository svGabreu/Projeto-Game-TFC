// PlayerInteraction.cs
// Coloque em: Assets/Scripts/Interaction/

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuração")]
    // Raio da esfera de detecção ao redor do player.
    // Aumente se quiser detectar objetos mais distantes.
    public float interactionRange = 3f;
    public LayerMask interactableLayer;

    [Header("UI")]
    public TMPro.TextMeshProUGUI interactionPromptText;

    private IInteractable currentInteractable;

    // Retorna true se qualquer painel de UI estiver aberto
    private bool IsPanelOpen()
    {
        if (RioDaVidaUI.Instance   != null && RioDaVidaUI.Instance.IsOpen())   return true;
        if (SocialUI.Instance      != null && SocialUI.Instance.IsOpen())      return true;
        if (InventoryUI.Instance   != null && InventoryUI.Instance.IsOpen())   return true;
        if (ItemExamineUI.Instance != null && ItemExamineUI.Instance.IsOpen()) return true;
        return false;
    }

    private void Update()
    {
        DetectNearestInteractable();

        // Não processa E enquanto painel de puzzle estiver aberto
        if (!IsPanelOpen() &&
            currentInteractable != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentInteractable.Interact();
        }
    }

    private void DetectNearestInteractable()
    {
        // Detecta todos os colliders dentro do raio ao redor do player
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactionRange,
            interactableLayer
        );

        IInteractable nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            // Busca direto no collider; se não encontrar, sobe na hierarquia
            IInteractable interactable = col.GetComponent<IInteractable>()
                                      ?? col.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearest = interactable;
            }
        }

        currentInteractable = nearest;

        // Oculta o prompt se algum painel de puzzle estiver aberto
        bool panelAberto = IsPanelOpen();

        if (interactionPromptText != null)
        {
            if (!panelAberto && currentInteractable != null)
            {
                interactionPromptText.text = currentInteractable.GetInteractionPrompt();
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }

    // Desenha o raio de detecção na Scene View para facilitar ajuste
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
