using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton persistente entre cenas.
// Guarda qual SpawnPoint o jogador deve usar ao entrar em uma nova cena.
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    // ID do SpawnPoint que será usado na próxima cena carregada.
    // "" significa usar a posição padrão da cena.
    public string TargetSpawnID { get; private set; } = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Carrega uma cena e registra o ponto de spawn de destino.
    public void GoToScene(string sceneName, string spawnPointID = "")
    {
        TargetSpawnID = spawnPointID;
        SceneManager.LoadScene(sceneName);
    }
}
