using UnityEngine;
using UnityEngine.Playables;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData     dialogueData;
    [SerializeField] private PlayableDirector director;  // se preenchido, pausa o Timeline durante o diálogo

    public void Trigger()
    {
        DialogueManager.Instance?.StartDialogue(dialogueData, director);
    }
}
