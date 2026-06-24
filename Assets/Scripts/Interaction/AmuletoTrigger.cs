// AmuletoTrigger.cs
// Coloque em: Assets/Scripts/Interaction/
// Coloque num GameObject com Collider próximo à porta da pirâmide.
// Ao pressionar E, abre o painel de encaixe dos amuletos.

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmuletoTrigger : MonoBehaviour, IInteractable
{
    [Header("Prompts")]
    [SerializeField] private string promptBloqueado = "[E] Encaixar os amuletos na porta";
    [SerializeField] private string promptDesbloqueado = "[E] Entrar na Pirâmide";

    public void Interact()
    {
        if (AmuletoPainelUI.Instance == null)
        {
            Debug.LogWarning("[AmuletoTrigger] AmuletoPainelUI não encontrado na cena!");
            return;
        }

        AmuletoPainelUI.Instance.OpenPanel();
    }

    public string GetInteractionPrompt()
    {
        // Se todos encaixados, mostra prompt de entrar
        if (AmuletoPainelUI.Instance != null)
        {
            bool todosPreenchidos = true;
            foreach (var slot in AmuletoPainelUI.Instance.slots)
                if (!slot.IsFilled) { todosPreenchidos = false; break; }

            if (todosPreenchidos) return promptDesbloqueado;
        }

        return promptBloqueado;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
