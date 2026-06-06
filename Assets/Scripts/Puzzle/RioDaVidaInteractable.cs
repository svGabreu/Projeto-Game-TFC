// RioDaVidaInteractable.cs
// Coloque em: Assets/Scripts/Puzzle/
// Attach no objeto 3D do mural da Casa do Rio da Vida.
// Abre o painel do puzzle ao pressionar E.

using UnityEngine;

public class RioDaVidaInteractable : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt() => "Pressione E para examinar o mural";

    public void Interact()
    {
        // FindObjectsByType com includeInactive=true encontra objetos desativados tambem
        var ui = Object.FindFirstObjectByType<RioDaVidaUI>(FindObjectsInactive.Include);
        if (ui != null)
            ui.OpenPanel();
        else
            Debug.LogWarning("[RioDaVidaInteractable] RioDaVidaUI nao encontrado na cena.");
    }
}