// DiagnosticoManager.cs
// Assets/Scripts/Core/DiagnosticoManager.cs
// Script TEMPORÁRIO de diagnóstico — delete após resolver o bug.
// Coloque num GameObject vazio chamado "Diagnostico" na cena do Egito.
// Ele monitora o GameStateManager e reporta quando ele morre ou é recriado.

using UnityEngine;
using UnityEngine.SceneManagement;

public class DiagnosticoManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        Debug.Log($"[DIAG] Iniciado. GSM vivo={GameStateManager.Instance != null}");
    }

    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        Debug.Log($"[DIAG] Cena DESCARREGADA: '{scene.name}' | GSM vivo={GameStateManager.Instance != null}");
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[DIAG] Cena CARREGADA: '{scene.name}' | GSM vivo={GameStateManager.Instance != null}");
    }

    private void Update()
    {
        // Checa a cada frame se o GSM morreu
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[DIAG] *** GSM É NULL AGORA *** frame=" + Time.frameCount);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
