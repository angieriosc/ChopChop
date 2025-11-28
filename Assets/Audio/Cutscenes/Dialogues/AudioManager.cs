using UnityEngine;
using UnityEngine.UI; // Required for Button interaction

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    private int currentClipIndex = 0;

    void Start()
    {
        // Ensure an AudioSource is present
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Optionally, play the first clip on start
        PlayCurrentClip();
    }

    public void PlayNextClip()
    {
        currentClipIndex = (currentClipIndex + 1) % audioClips.Length;
        PlayCurrentClip();
    }

    public void PlayPreviousClip()
    {
        currentClipIndex--;
        if (currentClipIndex < 0)
        {
            currentClipIndex = audioClips.Length - 1;
        }
        PlayCurrentClip();
    }

    private void PlayCurrentClip()
    {
        if (audioClips.Length > 0)
        {
            audioSource.clip = audioClips[currentClipIndex];
            audioSource.Play();
        }
    }
}