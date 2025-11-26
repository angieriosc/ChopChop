using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script de emergencia para forzar el silencio de TODOS los AudioSource.
/// Úsalo como último recurso si la música persiste.
/// </summary>
public class AudioKiller : MonoBehaviour
{
    private static AudioKiller instance;
    private List<AudioSource> silencedSources = new List<AudioSource>();
    private bool isKilling = false;
    
    public static AudioKiller Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("AudioKiller");
                instance = obj.AddComponent<AudioKiller>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Silencia ABSOLUTAMENTE TODO el audio excepto los AudioSource especificados.
    /// </summary>
    public void KillAllAudioExcept(params AudioSource[] exceptions)
    {
        if (isKilling) return;
        
        isKilling = true;
        silencedSources.Clear();
        
        Debug.Log("💀 AudioKiller: Eliminando TODO el audio...");
        
        // Encontrar TODOS los AudioSource en la escena (incluyendo inactivos)
        AudioSource[] allSources = FindObjectsOfType<AudioSource>(true);
        
        int killedCount = 0;
        
        foreach (AudioSource source in allSources)
        {
            // Verificar si está en las excepciones
            bool isException = false;
            foreach (AudioSource exception in exceptions)
            {
                if (source == exception)
                {
                    isException = true;
                    break;
                }
            }
            
            // Si no es excepción, silenciarlo
            if (!isException)
            {
                source.Stop();
                source.mute = true;
                source.enabled = false;
                source.volume = 0f;
                silencedSources.Add(source);
                killedCount++;
                
                Debug.Log($"💀 Silenciado: {source.gameObject.name} (clip: {(source.clip != null ? source.clip.name : "ninguno")})");
            }
        }
        
        // Reducir volumen maestro a 0 y luego restaurarlo
        float originalVolume = AudioListener.volume;
        AudioListener.volume = 0f;
        
        // Esperar un frame y restaurar solo para las excepciones
        StartCoroutine(RestoreVolumeAfterFrame(originalVolume));
        
        Debug.Log($"💀 AudioKiller: {killedCount} AudioSources silenciados");
    }
    
    /// <summary>
    /// Restaura el volumen después de un frame.
    /// </summary>
    private System.Collections.IEnumerator RestoreVolumeAfterFrame(float volume)
    {
        yield return null;
        AudioListener.volume = volume;
    }
    
    /// <summary>
    /// Restaura el audio de todos los AudioSource silenciados.
    /// </summary>
    public void RestoreAudio()
    {
        if (!isKilling) return;
        
        Debug.Log("🔊 AudioKiller: Restaurando audio...");
        
        foreach (AudioSource source in silencedSources)
        {
            if (source != null)
            {
                source.mute = false;
                source.enabled = true;
            }
        }
        
        silencedSources.Clear();
        isKilling = false;
        
        Debug.Log("🔊 AudioKiller: Audio restaurado");
    }
    
    /// <summary>
    /// Método simple para usar desde otros scripts.
    /// </summary>
    public static void SilenceEverything()
    {
        Instance.KillAllAudioExcept();
    }
    
    /// <summary>
    /// Método simple para restaurar desde otros scripts.
    /// </summary>
    public static void RestoreEverything()
    {
        if (instance != null)
        {
            instance.RestoreAudio();
        }
    }
}