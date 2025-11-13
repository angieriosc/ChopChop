using UnityEngine;
using UnityEngine.UI;

public class GlobalVolumeSlider : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        // Load saved volume (or default to 1)
        volumeSlider.value = PlayerPrefs.GetFloat("GlobalVolume", 1f);
        AudioListener.volume = volumeSlider.value;

        // Add listener
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("GlobalVolume", volume);
    }
}