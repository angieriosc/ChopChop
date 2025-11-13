using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
    public Slider musicSlider;
    public AudioSource musicSource;

    void Start()
    {
        // Load saved music volume (or default to 1)
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicSource.volume = musicSlider.value;

        // Add listener
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
}