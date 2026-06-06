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

    [Tooltip("Nome do GameObject que tem o MuralUI (ex: MuralPanel_A). " +
             "Usado como fallback quando a referência se perde em LoadSceneAsync.")]
    [SerializeField] private string muralPanelName = "";

    private void Awake()
    {
        ResolveMuralUI();
    }

    private void Start()
    {
        // Segunda tentativa — garante que funciona mesmo se a cena carregou em ordem diferente
        if (muralUI == null)
            ResolveMuralUI();
    }

    private void ResolveMuralUI()
    {
        if (muralUI != null) return; // já está setado — nada a fazer

        // Fallback 1: busca pelo nome configurado no Inspector
        if (!string.IsNullOrEmpty(muralPanelName))
        {
            var go = GameObject.Find(muralPanelName);
            if (go != null) muralUI = go.GetComponent<MuralUI>();
        }

        // Fallback 2: infere o nome pelo nome deste objeto (Mural_A → MuralPanel_A)
        if (muralUI == null)
        {
            string suffix = gameObject.name.Contains("_B") ? "_B" : "_A";
            var go = GameObject.Find("MuralPanel" + suffix);
            if (go != null) muralUI = go.GetComponent<MuralUI>();
        }

        // Fallback 3: pega qualquer MuralUI que ainda não esteja em uso
        if (muralUI == null)
        {
            var found = FindFirstObjectByType<MuralUI>();
            if (found != null) muralUI = found;
        }

        if (muralUI != null)
            Debug.Log($"[MuralInteractable] '{gameObject.name}': muralUI resolvido via fallback → {muralUI.gameObject.name}");
        else
            Debug.LogError($"[MuralInteractable] '{gameObject.name}': não foi possível encontrar MuralUI!");
    }

    public void Interact()
    {
        if (muralUI == null) ResolveMuralUI();

        if (muralUI != null)
            muralUI.OpenMural();
        else
            Debug.LogError($"[MuralInteractable] '{gameObject.name}': muralUI ainda NULL após tentativa de resolução.");
    }

    public string GetInteractionPrompt() => prompt;
}
