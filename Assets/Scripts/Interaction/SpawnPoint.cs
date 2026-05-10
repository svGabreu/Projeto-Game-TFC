using UnityEngine;

// Coloque próximo a cada porta/entrada nas cenas.
// Ao carregar a cena, o jogador é teleportado para o SpawnPoint
// cujo ID corresponde ao registrado no SceneTransitionManager.
public class SpawnPoint : MonoBehaviour
{
    public string spawnID;

    private void Start()
    {
        if (SceneTransitionManager.Instance == null) return;
        if (SceneTransitionManager.Instance.TargetSpawnID != spawnID) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = transform.position;
    }

    // Visualização no editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
    }
}
