// SceneTransition.cs
// Assets/Scripts/Interaction/SceneTransition.cs
// Coloque num GameObject com Collider na porta/entrada.
// Implementa IInteractable: o jogador vê "[ E ] Entrar na Casa..." e
// pressiona E para transitar — exatamente como os outros interagíveis do jogo.
//
// Configuração no Inspector:
//   Target Scene      → nome exato da cena (use SceneNames.CASA_SOCIAL)
//   Spawn Point ID    → ID do SpawnPoint de chegada (use SpawnIDs.ENTRADA_CASA_SOCIAL)
//   Return Spawn ID   → ID do SpawnPoint de retorno no Egito (use SpawnIDs.EGITO_PORTA_SOCIAL)
//   Interaction Label → texto do prompt, ex: "Entrar na Casa Social"

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

    public void Interact()
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
