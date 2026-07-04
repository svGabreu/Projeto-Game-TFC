// ReturnToEgito.cs
// Assets/Scripts/Interaction/ReturnToEgito.cs
// Coloque na porta de SAÍDA de cada Casa.
// Ao pressionar E, volta ao Egito no SpawnPoint correto (salvo quando entrou).
//
// Não precisa de nenhuma configuração — usa automaticamente os dados
// salvos pelo SceneTransitionManager.GoToScene() quando entrou na Casa.

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReturnToEgito : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private string interactionLabel = "Sair para o Egito";


    // ── IInteractable ─────────────────────────────────────────────────────────
    public string GetInteractionPrompt() => $"[E] {interactionLabel}";

    public void Interact()
    {
        var mgr = SceneTransitionManager.Instance;
        if (mgr == null) { Debug.LogWarning("[ReturnToEgito] SceneTransitionManager não encontrado!"); return; }

        Debug.Log($"[ReturnToEgito] PreviousScene='{mgr.PreviousScene}' | PreviousSpawnID='{mgr.PreviousSpawnID}'");

        if (string.IsNullOrEmpty(mgr.PreviousScene))
        {
            Debug.LogWarning("[ReturnToEgito] PreviousScene vazio — fallback sem spawnID.");
            mgr.GoToScene(SceneNames.EGITO);
            return;
        }

        mgr.ReturnToPreviousScene();
    }

    // ── Visualização na Scene View ────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.25f);
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 1f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"← {interactionLabel}");
#endif
    }
}
