// RioDaVidaInteractable.cs
// Coloque em: Assets/Scripts/Puzzle/
// Attach no objeto 3D do mural da Casa do Rio da Vida.
// Abre o painel do puzzle ao pressionar E.

using UnityEngine;

public class RioDaVidaInteractable : MonoBehaviour, IInteractable
{
    [Header("Painel do Puzzle")]
    public RioDaVidaUI painel;

    public string GetInteractionPrompt() => "Pressione E para examinar o mural";

    public void Interact()
    {
        if (painel != null) painel.OpenPanel();
    }
}
