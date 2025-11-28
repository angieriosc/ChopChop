using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public string[] scenesToPlayIn;
    private AudioSource musicSource;

    void Awake()
    {
        if (FindObjectsOfType<MusicManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        musicSource = GetComponent<AudioSource>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldPlay = false;

        foreach (string s in scenesToPlayIn)
        {
            if (scene.name == s)
            {
                shouldPlay = true;
                break;
            }
        }

        if (shouldPlay)
        {
            if (!musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }
    }

    public void DestroyMusic()
    {
        Destroy(gameObject);
    }

    // 🔊 Add this so the settings slider can change your music volume
    public void SetVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }

    public float GetVolume()
    {
        return musicSource != null ? musicSource.volume : 1f;
    }
}