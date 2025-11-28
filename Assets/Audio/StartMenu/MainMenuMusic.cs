using UnityEngine;
using UnityEngine.UI;

public class MainMenuMusic : MonoBehaviour
{
    public Slider slider;
    private MusicManager musicManager;

    void Start()
    {
        // Find the persistent MusicManager object
        musicManager = FindObjectOfType<MusicManager>();

        if (musicManager != null)
        {
            // Match slider to current volume
            slider.value = musicManager.GetVolume();

            // Listen for changes
            slider.onValueChanged.AddListener(ChangeVolume);
        }
    }

    void ChangeVolume(float value)
    {
        if (musicManager != null)
            musicManager.SetVolume(value);
    }
}
