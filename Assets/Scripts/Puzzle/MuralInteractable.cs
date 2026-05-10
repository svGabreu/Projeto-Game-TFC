// MuralInteractable.cs
// Coloque em: Assets/Scripts/Puzzle/
// Adicione este script ao GameObject do mural na cena (junto com MuralPuzzle).
// Ele implementa IInteractable para que o PlayerInteraction detecte o mural.

using UnityEngine;

public class MuralInteractable : MonoBehaviour, IInteractable
{
    [Header("Referências")]
    public MuralUI muralUI; // arraste o painel de UI do mural aqui

    [Header("Configuração")]
    public string prompt = "Pressione E para examinar o mural";

    public void Interact()
    {
        if (muralUI != null)
            muralUI.OpenMural();
    }

    public string GetInteractionPrompt() => prompt;
}
