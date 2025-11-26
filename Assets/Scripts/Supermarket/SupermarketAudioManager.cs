using UnityEngine;

/// <summary>
/// Gestiona todos los sonidos del supermercado.
/// </summary>
public class SupermarketAudioManager : MonoBehaviour
{
    [Header("Background Music")]
    [SerializeField] private AudioClip supermarketAmbience;
    [SerializeField] private float ambienceVolume = 0.5f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupItemSound;
    [SerializeField] private AudioClip paymentSuccessSound;
    [SerializeField] private AudioClip paymentFailSound;
    [SerializeField] private AudioClip cartRollingSound;
    [SerializeField] private float sfxVolume = 0.7f;
    [SerializeField] private float cartRollingVolume = 0.5f;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource cartAudioSource;
    
    public static SupermarketAudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        SetupAudioSources();
    }
    
    private void Start()
    {
        // Iniciar música de fondo automáticamente
        PlaySupermarketAmbience();
    }
    
    /// <summary>
    /// Configura los AudioSources si no están asignados.
    /// </summary>
    private void SetupAudioSources()
    {
        if (ambienceSource == null)
        {
            GameObject ambienceObj = new GameObject("AmbienceSource");
            ambienceObj.transform.SetParent(transform);
            ambienceSource = ambienceObj.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
            ambienceSource.spatialBlend = 0f;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }
        
        if (cartAudioSource == null)
        {
            GameObject cartObj = new GameObject("CartAudioSource");
            cartObj.transform.SetParent(transform);
            cartAudioSource = cartObj.AddComponent<AudioSource>();
            cartAudioSource.loop = true;
            cartAudioSource.playOnAwake = false;
            cartAudioSource.spatialBlend = 0f;
        }
    }
    
    /// <summary>
    /// Activa la música de fondo del supermercado.
    /// </summary>
    public void PlaySupermarketAmbience()
    {
        if (supermarketAmbience != null && ambienceSource != null && !ambienceSource.isPlaying)
        {
            ambienceSource.clip = supermarketAmbience;
            ambienceSource.volume = ambienceVolume;
            ambienceSource.Play();
            Debug.Log("🎵 Música de supermercado activada");
        }
    }
    
    /// <summary>
    /// Detiene la música de fondo.
    /// </summary>
    public void StopSupermarketAmbience()
    {
        if (ambienceSource != null && ambienceSource.isPlaying)
        {
            ambienceSource.Stop();
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de recoger un item.
    /// </summary>
    public void PlayPickupSound()
    {
        if (pickupItemSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(pickupItemSound, sfxVolume);
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de pago exitoso.
    /// </summary>
    public void PlayPaymentSuccessSound()
    {
        if (paymentSuccessSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(paymentSuccessSound, sfxVolume);
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de pago fallido.
    /// </summary>
    public void PlayPaymentFailSound()
    {
        if (paymentFailSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(paymentFailSound, sfxVolume);
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de las ruedas del carrito en loop.
    /// </summary>
    public void PlayCartRollingSound()
    {
        if (cartRollingSound != null && cartAudioSource != null && !cartAudioSource.isPlaying)
        {
            cartAudioSource.clip = cartRollingSound;
            cartAudioSource.volume = cartRollingVolume;
            cartAudioSource.Play();
        }
    }
    
    /// <summary>
    /// Detiene el sonido de las ruedas del carrito.
    /// </summary>
    public void StopCartRollingSound()
    {
        if (cartAudioSource != null && cartAudioSource.isPlaying)
        {
            cartAudioSource.Stop();
        }
    }
    
    /// <summary>
    /// Pausa el sonido de las ruedas del carrito.
    /// </summary>
    public void PauseCartRollingSound()
    {
        if (cartAudioSource != null && cartAudioSource.isPlaying)
        {
            cartAudioSource.Pause();
        }
    }
    
    /// <summary>
    /// Reanuda el sonido de las ruedas del carrito.
    /// </summary>
    public void ResumeCartRollingSound()
    {
        if (cartAudioSource != null && cartAudioSource.clip != null && !cartAudioSource.isPlaying)
        {
            cartAudioSource.UnPause();
        }
    }
    
    // ============================================
    // ✅ NUEVOS MÉTODOS PARA GAME OVER
    // ============================================
    
    /// <summary>
    /// Detiene TODA la música y sonidos del supermercado (para Game Over).
    /// </summary>
    public void StopAllMusic()
    {
        // Detener música de fondo
        if (ambienceSource != null)
        {
            ambienceSource.Stop();
            ambienceSource.mute = true;
            ambienceSource.enabled = false;
        }
        
        // Detener sonido de carrito
        if (cartAudioSource != null)
        {
            cartAudioSource.Stop();
            cartAudioSource.mute = true;
            cartAudioSource.enabled = false;
        }
        
        // Detener efectos de sonido
        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource.mute = true;
            sfxSource.enabled = false;
        }
        
        // Buscar y detener CUALQUIER AudioSource en este GameObject
        AudioSource[] allSources = GetComponents<AudioSource>();
        foreach (var source in allSources)
        {
            source.Stop();
            source.mute = true;
            source.enabled = false;
        }
        
        // Buscar en hijos también
        AudioSource[] childSources = GetComponentsInChildren<AudioSource>();
        foreach (var source in childSources)
        {
            source.Stop();
            source.mute = true;
            source.enabled = false;
        }
        
        Debug.Log("🔇 SupermarketAudioManager: Toda la música detenida y deshabilitada");
    }
    
    /// <summary>
    /// Reactiva el audio del supermercado (para después del reinicio).
    /// </summary>
    public void RestoreAudio()
    {
        if (ambienceSource != null)
        {
            ambienceSource.mute = false;
        }
        
        if (cartAudioSource != null)
        {
            cartAudioSource.mute = false;
        }
        
        if (sfxSource != null)
        {
            sfxSource.mute = false;
        }
        
        Debug.Log("🔊 SupermarketAudioManager: Audio restaurado");
    }
    
    /// <summary>
    /// Reduce el volumen de toda la música gradualmente (fade out).
    /// </summary>
    public void FadeOutAllMusic(float duration = 1f)
    {
        if (ambienceSource != null)
        {
            StartCoroutine(FadeOutSource(ambienceSource, duration));
        }
        
        if (cartAudioSource != null)
        {
            StartCoroutine(FadeOutSource(cartAudioSource, duration));
        }
    }
    
    /// <summary>
    /// Coroutine para hacer fade out de un AudioSource.
    /// </summary>
    private System.Collections.IEnumerator FadeOutSource(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        source.volume = 0f;
        source.Stop();
    }
}