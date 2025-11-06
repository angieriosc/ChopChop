using UnityEngine;

public class MuteToggle : MonoBehaviour
{
    public Animator muteAnimator;     // UI icon animator
    private bool isMuted = false;

    public void ToggleMute()
    {
        isMuted = !isMuted;

        // Apply mute to whole game audio
        AudioListener.pause = isMuted;

        // Change icon animation state
        muteAnimator.SetBool("Mute", isMuted);
    }
}