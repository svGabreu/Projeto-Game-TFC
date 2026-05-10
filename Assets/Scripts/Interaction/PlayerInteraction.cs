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

    private void Update()
    {
        DetectNearestInteractable();

        if (currentInteractable != null &&
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
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearest = interactable;
            }
        }

        currentInteractable = nearest;

        if (interactionPromptText != null)
        {
            if (currentInteractable != null)
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
