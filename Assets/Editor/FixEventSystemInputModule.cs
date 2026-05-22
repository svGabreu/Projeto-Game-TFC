// FixEventSystemInputModule.cs
// Coloque em: Assets/Editor/
// Menu: Tools > Fix EventSystem Input Module
// Substitui StandaloneInputModule por InputSystemUIInputModule no EventSystem da cena.

using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem.UI;

public static class FixEventSystemInputModule
{
    [MenuItem("Tools/Fix EventSystem Input Module")]
    public static void Fix()
    {
        // Procura todos os StandaloneInputModules na cena
        var old = Object.FindObjectsByType<UnityEngine.EventSystems.StandaloneInputModule>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var sim in old)
        {
            var go = sim.gameObject;
            Undo.RecordObject(go, "Replace StandaloneInputModule");

            // Adiciona o novo módulo se não existir
            if (go.GetComponent<InputSystemUIInputModule>() == null)
            {
                Undo.AddComponent<InputSystemUIInputModule>(go);
            }

            // Remove o antigo
            Undo.DestroyObjectImmediate(sim);
            count++;
            Debug.Log($"[Fix] EventSystem '{go.name}': StandaloneInputModule substituído por InputSystemUIInputModule.");
        }

        if (count == 0)
            Debug.Log("[Fix] Nenhum StandaloneInputModule encontrado na cena.");
        else
            Debug.Log($"[Fix] {count} EventSystem(s) corrigido(s).");
    }
}
