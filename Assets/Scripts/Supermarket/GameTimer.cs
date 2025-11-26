using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Controla el cronómetro del juego y las condiciones de victoria/derrota.
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float gameDuration = 60f; // 1 minuto
    [SerializeField] private bool startOnAwake = false;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerPanel;
    
    [Header("Warning Settings")]
    [SerializeField] private float warningThreshold = 10f; // Advertir a los 10 segundos
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip tickingSound;
    
    public static GameTimer Instance { get; private set; }
    
    private float currentTime;
    private bool isRunning = false;
    private bool hasWarned = false;
    private AudioSource audioSource;
    private Coroutine tickingSoundCoroutine;
    
    public float CurrentTime => currentTime;
    public bool IsRunning => isRunning;
    public float TimeRemaining => Mathf.Max(0, currentTime);
    
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        currentTime = gameDuration;
        UpdateTimerDisplay();
        
        // ✅ CAMBIO: Ocultar el timer al inicio
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        if (startOnAwake)
        {
            StartTimer();
        }
    }
    
    private void Update()
    {
        if (!isRunning) return;
        
        currentTime -= Time.deltaTime;
        UpdateTimerDisplay();
        
        // Verificar advertencia de tiempo bajo
        if (currentTime <= warningThreshold && !hasWarned)
        {
            TriggerWarning();
        }
        
        // Verificar si se acabó el tiempo
        if (currentTime <= 0)
        {
            TimeUp();
        }
    }
    
    /// <summary>
    /// Inicia el cronómetro.
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
        
        // ✅ NUEVO: Mostrar el panel cuando se inicia el timer
        ShowTimer();
        
        Debug.Log("⏰ Cronómetro iniciado");
    }
    
    /// <summary>
    /// Pausa el cronómetro.
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;
        StopTickingSound();
        Debug.Log("⏸️ Cronómetro pausado");
    }
    
    /// <summary>
    /// Detiene el cronómetro completamente.
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        StopTickingSound();
        Debug.Log("⏹️ Cronómetro detenido");
    }
    
    /// <summary>
    /// Reinicia el cronómetro.
    /// </summary>
    public void ResetTimer()
    {
        currentTime = gameDuration;
        isRunning = false;
        hasWarned = false;
        UpdateTimerDisplay();
        
        if (timerText != null)
        {
            timerText.color = normalColor;
        }
        
        StopTickingSound();
        
        // Ocultar el panel al reiniciar
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
        
        Debug.Log("🔄 Cronómetro reiniciado");
    }
    
    /// <summary>
    /// Actualiza la visualización del tiempo.
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    /// <summary>
    /// Activa la advertencia de tiempo bajo.
    /// </summary>
    private void TriggerWarning()
    {
        hasWarned = true;
        
        if (timerText != null)
        {
            timerText.color = warningColor;
            StartCoroutine(BlinkTimer());
        }
        
        // Reproducir sonido de advertencia
        if (warningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
        
        // Iniciar sonido de tic-tac
        if (tickingSound != null)
        {
            tickingSoundCoroutine = StartCoroutine(PlayTickingSound());
        }
        
        Debug.Log("⚠️ ¡Advertencia! Quedan menos de 10 segundos");
    }
    
    /// <summary>
    /// Efecto de parpadeo en el timer.
    /// </summary>
    private IEnumerator BlinkTimer()
    {
        while (isRunning && currentTime > 0)
        {
            if (timerText != null)
            {
                timerText.enabled = !timerText.enabled;
            }
            yield return new WaitForSeconds(0.5f);
        }
        
        if (timerText != null)
        {
            timerText.enabled = true;
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de tic-tac repetidamente.
    /// </summary>
    private IEnumerator PlayTickingSound()
    {
        while (isRunning && currentTime > 0)
        {
            if (audioSource != null && tickingSound != null)
            {
                audioSource.PlayOneShot(tickingSound);
            }
            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// Detiene el sonido de tic-tac.
    /// </summary>
    private void StopTickingSound()
    {
        if (tickingSoundCoroutine != null)
        {
            StopCoroutine(tickingSoundCoroutine);
            tickingSoundCoroutine = null;
        }
    }
    
    /// <summary>
    /// Se llama cuando se acaba el tiempo.
    /// </summary>
    private void TimeUp()
    {
        StopTimer();
        currentTime = 0;
        UpdateTimerDisplay();
        
        Debug.Log("⏰ ¡Se acabó el tiempo!");
        
        // Mostrar pantalla de Game Over
        if (GameOverScreen.Instance != null)
        {
            GameOverScreen.Instance.ShowGameOver(GameOverReason.TimeUp);
        }
    }
    
    /// <summary>
    /// Oculta el panel del timer.
    /// </summary>
    public void HideTimer()
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
            Debug.Log("⏰ Timer ocultado");
        }
    }
    
    /// <summary>
    /// Muestra el panel del timer.
    /// </summary>
    public void ShowTimer()
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
            Debug.Log("⏰ Timer mostrado");
        }
    }
}