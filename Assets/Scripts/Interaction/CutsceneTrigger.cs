using UnityEngine;

public class CutsceneTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private CutsceneController cutsceneController;
    [SerializeField] private string             interactionPrompt = "Pressione E para descansar";

    private bool triggered = false;

    public void Interact()
    {
        if (triggered) return;
        triggered = true;
        cutsceneController.StartCutscene();
        gameObject.SetActive(false);
    }

    public string GetInteractionPrompt() => interactionPrompt;
}
