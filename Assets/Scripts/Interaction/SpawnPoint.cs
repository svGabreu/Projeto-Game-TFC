// SpawnPoint.cs
// Assets/Scripts/Interaction/SpawnPoint.cs
// Coloque próximo a cada porta/entrada em todas as cenas.
// Ao carregar a cena, reposiciona o jogador aqui se o spawnID bater.

using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("ID único deste ponto de spawn. Deve coincidir com o spawnPointID do SceneTransition que leva até aqui.")]
    public string spawnID;

    // ── Start(): backup caso sceneLoaded já tenha disparado antes do subscribe ──
    private void Start()
    {
        TrySpawnPlayer("Start");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TrySpawnPlayer("sceneLoaded");
    }

    // ── Lógica central de spawn (chamada por ambos os caminhos) ──────────────
    private void TrySpawnPlayer(string origem)
    {
        var mgr = SceneTransitionManager.Instance;
        if (mgr == null) return;
        if (string.IsNullOrEmpty(mgr.TargetSpawnID)) return;
        if (mgr.TargetSpawnID != spawnID) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"[SpawnPoint] '{spawnID}': Player não encontrado ({origem}).");
            return;
        }

        // Desativa física temporariamente para evitar que o Rigidbody
        // reposicione o player depois do spawn
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity        = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.SetPositionAndRotation(transform.position, transform.rotation);

        // Reseta a câmera para trás do player no novo spawn
        var cam = Camera.main?.GetComponent<CameraControl>();
        cam?.SnapCameraToPlayer();

        // Consome o ID para nenhum outro SpawnPoint tentar de novo
        mgr.ConsumeSpawnID();

        Debug.Log($"[SpawnPoint] '{spawnID}' spawnado via {origem}.");
    }

    // ── Visualização na Scene View ───────────────────────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.2f);
        Gizmos.DrawSphere(transform.position + transform.forward * 1.2f, 0.1f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, spawnID);
#endif
    }
}
