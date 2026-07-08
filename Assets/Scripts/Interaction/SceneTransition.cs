
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SceneTransition : MonoBehaviour, IInteractable
{
    [Header("Destino")]
    public string targetScene;
    public string spawnPointID;

    [Header("Retorno")]
    [Tooltip("ID do SpawnPoint no Egito para usar quando o jogador SAIR dessa cena de volta.")]
    public string returnSpawnID;

    [Header("UI")]
    [SerializeField] private string interactionLabel = "Entrar";


    // ── IInteractable ─────────────────────────────────────────────────────────
    public string GetInteractionPrompt() => $"[E] {interactionLabel}";

    public void Interact() // Chamado quando o jogador pressiona E
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning($"[SceneTransition] '{gameObject.name}': targetScene não configurado!");
            return;
        }

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("[SceneTransition] SceneTransitionManager.Instance é null! Adicione o componente SceneTransitionManager à cena.");
            return;
        }

        Debug.Log($"[SceneTransition] GoToScene → {targetScene} | spawn={spawnPointID} | return={returnSpawnID} | IsTransitioning={SceneTransitionManager.Instance.IsTransitioning}");
        SceneTransitionManager.Instance.GoToScene(targetScene, spawnPointID, returnSpawnID);
    }

    // ── Visualização na Scene View ────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.25f);
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 1f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(targetScene))
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 1.5f,
                $"→ {targetScene}\n  spawn: {spawnPointID}");
        }
#endif
    }
}
