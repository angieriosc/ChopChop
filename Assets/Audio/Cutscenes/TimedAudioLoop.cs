using UnityEngine;
using System.Collections; // Required for Coroutines

[RequireComponent(typeof(AudioSource))] // Ensures an AudioSource is present
public class TimedAudioLoop : MonoBehaviour
{
    public AudioClip audioClipToLoop; // Assign your audio clip in the Inspector
    public float loopDurationSeconds = 5f; // Set the desired loop duration

    private AudioSource audioSource;
    private Coroutine loopCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClipToLoop;
        audioSource.loop = false; // We'll manage looping manually

        StartLooping();
    }

    public void StartLooping()
    {
        if (audioClipToLoop == null)
        {
            Debug.LogWarning("AudioClip is not assigned to TimedAudioLoop script.");
            return;
        }

        // Stop any existing loop before starting a new one
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
        }

        loopCoroutine = StartCoroutine(LoopAudioForDuration());
    }

    public void StopLooping()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
        audioSource.Stop();
    }

    private IEnumerator LoopAudioForDuration()
    {
        float startTime = Time.time;

        while (Time.time < startTime + loopDurationSeconds)
        {
            audioSource.Play();
            yield return new WaitForSeconds(audioClipToLoop.length); // Wait for the clip to finish
        }

        audioSource.Stop(); // Stop playing after the duration
        Debug.Log("Audio loop finished after " + loopDurationSeconds + " seconds.");
    }
}