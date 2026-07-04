using UnityEngine;

public class DialogueTriggerSequence : MonoBehaviour
{
    [SerializeField] private DialogueData[] dialogues;

    private int currentIndex = 0;

    public void TriggerNext()
    {
        if (currentIndex >= dialogues.Length) return;

        DialogueData linha = dialogues[currentIndex];
        currentIndex++;

        DialogueManager.Instance?.StartDialogue(linha, null);
    }
}
