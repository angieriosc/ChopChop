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
}