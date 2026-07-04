// RioDaVidaInteractable.cs
// Coloque em: Assets/Scripts/Puzzle/
// Attach no objeto 3D do mural da Casa do Rio da Vida.
// Abre o painel do puzzle ao pressionar E.

using UnityEngine;

public class RioDaVidaInteractable : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt() => "Pressione E para examinar o mural";

    private void Start()
    {
        // RioDaVidaPuzzle fica dentro do painel inativo — seu Start() só dispara
        // quando o painel abre. Para o amuleto aparecer no mundo ao reentrar na cena,
        // este objeto (sempre ativo) verifica o estado salvo e ativa a recompensa.
        if (GameStateManager.Instance == null || !GameStateManager.Instance.GetBool("rdv.reward")) return;

        var puzzle = Object.FindFirstObjectByType<RioDaVidaPuzzle>(FindObjectsInactive.Include);
        if (puzzle == null || puzzle.rewardWorldObject == null) return;

        var wc = puzzle.rewardWorldObject.GetComponent<WorldClue>();
        bool jaColetado = wc != null && wc.itemToGive != null
                          && InventoryManager.Instance != null
                          && InventoryManager.Instance.HasItem(wc.itemToGive.itemID);
        if (!jaColetado)
        {
            puzzle.rewardWorldObject.SetActive(true);
            Debug.Log("[RioDaVidaInteractable] Amuleto da Vida reativado na cena.");
        }
    }

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