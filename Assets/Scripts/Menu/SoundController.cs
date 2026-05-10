using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    private bool soundState = true; // true for unmuted, false for muted
    [SerializeField] private AudioSource audioSource; // Reference to the AudioSource component

    [SerializeField] private Sprite unmuteSound;
    [SerializeField] private Sprite muteSound;

    [SerializeField] private Image muteImage;

    public void MuteUnmute () // Method to toggle sound state
    {
        soundState = !soundState; 
        audioSource.enabled = soundState; 

        if (soundState)
        {
            muteImage.sprite = unmuteSound; 
        }
        else
        {
            muteImage.sprite = muteSound; 
        }
    }

    public void VolumeSlider(float value)
    {
        audioSource.volume = value; 

    }
}
