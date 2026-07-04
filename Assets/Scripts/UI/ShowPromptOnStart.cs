using System.Collections;
using UnityEngine;

public class ShowPromptOnStart : MonoBehaviour
{
    [SerializeField] private string message  = "Encontre um local para dormir...";
    [SerializeField] private float  delay    = 1f;
    [SerializeField] private float  duration = 5f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        PromptManager.Instance?.Show(message);
        yield return new WaitForSeconds(duration);
        PromptManager.Instance?.Hide();
    }
}
