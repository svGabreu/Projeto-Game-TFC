// TFC_NeblinaEgito.cs
// Assets/Editor/
// Configura a neblina de distância da cena Egito via RenderSettings.
// Use: TFC → Neblina → Configurar Neblina Egito
// Depois de rodar, salve a cena (Ctrl+S) e pode deletar este script.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TFC_NeblinaEgito
{
    [MenuItem("TFC/Neblina/Configurar Neblina — Egito")]
    static void ConfigurarNeblina()
    {
        // Neblina Linear: controle preciso sobre onde começa e termina
        // Ideal para esconder as bordas do terreno sem embaçar o centro do mapa
        RenderSettings.fog               = true;
        RenderSettings.fogMode           = FogMode.Linear;

        // Cor: bege-arenoso quente — combina com o deserto egípcio
        // Evita branco puro (pareceria neve) e cinza (pareceria Londres)
        RenderSettings.fogColor          = new Color(0.78f, 0.70f, 0.58f);

        // Distâncias: ajuste conforme o tamanho do seu terreno
        // Start = onde a neblina começa a aparecer
        // End   = onde o objeto some completamente na neblina
        RenderSettings.fogStartDistance  = 150f;
        RenderSettings.fogEndDistance    = 420f;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("[TFC] Neblina configurada! Salve a cena com Ctrl+S.\n" +
                  "Ajuste fogStartDistance e fogEndDistance na aba Lighting " +
                  "(Window → Rendering → Lighting → Environment) conforme o tamanho do mapa.");
    }

    [MenuItem("TFC/Neblina/Remover Neblina — Cena Ativa")]
    static void RemoverNeblina()
    {
        RenderSettings.fog = false;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[TFC] Neblina removida da cena ativa.");
    }
}
#endif
