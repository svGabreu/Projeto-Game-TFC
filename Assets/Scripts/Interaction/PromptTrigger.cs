using UnityEngine;

public class PromptTrigger : MonoBehaviour
{
    [SerializeField] private string message;
    [SerializeField] private bool   hideOnExit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            PromptManager.Instance?.Show(message);
    }

    private void OnTriggerExit(Collider other)
    {
        if (hideOnExit && other.CompareTag("Player"))
            PromptManager.Instance?.Hide();
    }
}
