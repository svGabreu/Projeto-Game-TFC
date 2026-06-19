using UnityEngine;
using UnityEngine.Playables;

public class NarratorCardTrigger : MonoBehaviour
{
    [SerializeField] private NarratorCard     narratorCard;
    [SerializeField] private PlayableDirector director;
    [SerializeField] private string           message;

    public void Trigger()
    {
        if (narratorCard == null || string.IsNullOrEmpty(message)) return;
        narratorCard.ShowAndPauseDirector(message, director);
    }
}
