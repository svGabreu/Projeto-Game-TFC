using UnityEngine;

// Coloque num GameObject com Collider (Is Trigger = true) na porta/entrada.
// Quando o jogador entrar no trigger, carrega a cena de destino e
// posiciona o jogador no SpawnPoint correspondente.
public class SceneTransition : MonoBehaviour
{
    [Header("Destino")]
    public string targetScene;      // nome exato da cena (ex: "Casa_Hieroglifos")
    public string spawnPointID;     // ID do SpawnPoint na cena de destino

    [Header("Interação manual (opcional)")]
    [Tooltip("Se true, o jogador precisa apertar E para transitar. Se false, basta entrar no trigger.")]
    public bool requireKeyPress = true;

    private bool playerInRange = false;

    private void Update()
    {
        if (!requireKeyPress || !playerInRange) return;

        if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            Transition();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!requireKeyPress)
            Transition();
        else
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void Transition()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.GoToScene(targetScene, spawnPointID);
    }
}
