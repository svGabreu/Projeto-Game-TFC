using UnityEngine;

// Coloque este script no GameObject "UIGlobal" (o root ativo que contém todos os painéis).
// Garante que UIGlobal sobreviva às trocas de cena via DontDestroyOnLoad.
// Necessário porque os painéis filhos (PainelExame, PainelInventario, etc.)
// iniciam inativos — seus Awake() não são chamados, então eles não podem
// registrar o DontDestroyOnLoad por conta própria.
public class UIGlobalPersist : MonoBehaviour
{
    private static UIGlobalPersist Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"[UIGlobalPersist] UIGlobal persistido via DontDestroyOnLoad ({gameObject.name})");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
