using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Muestra una pantalla de felicitaciones cuando se completa la compra.
/// </summary>
public class CompletionScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI congratsText;
    [SerializeField] private GameObject particleEffectPrefab;
    [SerializeField] private Image backgroundOverlay; // Fondo semi-transparente
    
    [Header("Animation")]
    [SerializeField] private float scaleAnimationDuration = 1f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioClip celebrationSound;
    
    [Header("UI to Hide")]
    [SerializeField] private GameObject[] uiElementsToHide; // Array de elementos a ocultar
    
    public static CompletionScreen Instance { get; private set; }
    
    private AudioSource audioSource;
    private bool[] originalUIStates; // Para guardar el estado original de cada elemento
    
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
        
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("❌ CompletionScreen: completionPanel no está asignado!");
        }
        
        // Guardar estados originales de los elementos de UI
        if (uiElementsToHide != null && uiElementsToHide.Length > 0)
        {
            originalUIStates = new bool[uiElementsToHide.Length];
            for (int i = 0; i < uiElementsToHide.Length; i++)
            {
                if (uiElementsToHide[i] != null)
                {
                    originalUIStates[i] = uiElementsToHide[i].activeSelf;
                }
            }
        }
        
        // Configurar el overlay si existe
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Muestra la pantalla de felicitaciones.
    /// </summary>
    public void ShowCompletion()
    {
        if (completionPanel == null)
        {
            Debug.LogError("❌ CompletionScreen: No se puede mostrar, panel no asignado");
            return;
        }
        
        Debug.Log("🎉 Mostrando pantalla de felicitaciones");
        
        // ✅ DETENER TODA LA MÚSICA DEL SUPERMERCADO
        StopAllBackgroundMusic();
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Ocultar otros elementos de UI
        HideOtherUIElements();
        
        // Mostrar overlay de fondo
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
            StartCoroutine(FadeInOverlay());
        }
        
        completionPanel.SetActive(true);
        
        // Reproducir sonido
        if (celebrationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(celebrationSound);
        }
        else
        {
            Debug.LogWarning("⚠️ No hay sonido de celebración asignado");
        }
        
        // Crear efecto de partículas
        if (particleEffectPrefab != null)
        {
            Vector3 centerScreen = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            GameObject particles = Instantiate(particleEffectPrefab, centerScreen, Quaternion.identity, transform);
            Destroy(particles, 5f);
        }
        
        // Animar el texto
        if (congratsText != null)
        {
            StartCoroutine(AnimateText());
        }
        else
        {
            Debug.LogWarning("⚠️ No hay texto de felicitaciones asignado");
        }
    }
    
    /// <summary>
    /// Detiene toda la música de fondo del supermercado.
    /// </summary>
    private void StopAllBackgroundMusic()
    {
        // Detener música del supermercado
        if (SupermarketAudioManager.Instance != null)
        {
            SupermarketAudioManager.Instance.StopAllMusic();
        }
        
        // Buscar y detener TODOS los AudioSource en la escena excepto el de CompletionScreen
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>(true);
        foreach (AudioSource source in allAudioSources)
        {
            // No detener el AudioSource de este script
            if (source != audioSource)
            {
                source.Stop();
                source.mute = true;
                source.enabled = false;
            }
        }
        
        Debug.Log("🔇 Toda la música de fondo detenida (Victoria)");
    }
    
    /// <summary>
    /// Oculta la pantalla de felicitaciones.
    /// </summary>
    public void HideCompletion()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
        
        // Restaurar elementos de UI
        RestoreUIElements();
        
        // Ocultar overlay
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }
        
        // Restaurar timeScale
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Reinicia el nivel actual.
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("🔄 Reiniciando nivel...");
        
        // Restaurar timeScale
        Time.timeScale = 1f;
        
        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Oculta los elementos de UI especificados.
    /// </summary>
    private void HideOtherUIElements()
    {
        // Ocultar el prompt de interacción
        if (InteractionPrompt.Instance != null)
        {
            InteractionPrompt.Instance.HidePromptImmediate();
        }
        
        // Ocultar timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.HideTimer();
        }
        
        // Ocultar elementos especificados
        if (uiElementsToHide == null || uiElementsToHide.Length == 0)
        {
            // Si no se especificaron elementos manualmente, buscar SupermarketUI
            if (SupermarketManager.Instance != null && SupermarketManager.Instance.UIController != null)
            {
                // Aquí podrías llamar a un método HideAllUI si lo implementas en SupermarketUI
                Debug.Log("💡 Considera implementar HideAllUI() en SupermarketUI");
            }
            return;
        }
        
        for (int i = 0; i < uiElementsToHide.Length; i++)
        {
            if (uiElementsToHide[i] != null)
            {
                uiElementsToHide[i].SetActive(false);
            }
        }
        
        Debug.Log("🔇 UI del supermercado ocultada");
    }
    
    /// <summary>
    /// Restaura los elementos de UI a su estado original.
    /// </summary>
    private void RestoreUIElements()
    {
        if (uiElementsToHide == null || uiElementsToHide.Length == 0)
        {
            return;
        }
        
        for (int i = 0; i < uiElementsToHide.Length && i < originalUIStates.Length; i++)
        {
            if (uiElementsToHide[i] != null)
            {
                uiElementsToHide[i].SetActive(originalUIStates[i]);
            }
        }
        
        Debug.Log("🔊 UI del supermercado restaurada");
    }
    
    /// <summary>
    /// Fade in del overlay de fondo.
    /// </summary>
    private IEnumerator FadeInOverlay()
    {
        if (backgroundOverlay == null) yield break;
        
        float duration = 0.5f;
        float elapsed = 0f;
        Color startColor = backgroundOverlay.color;
        startColor.a = 0f;
        Color endColor = startColor;
        endColor.a = 0.7f; // 70% de opacidad
        
        backgroundOverlay.color = startColor;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Usar unscaledDeltaTime porque timeScale = 0
            float progress = elapsed / duration;
            backgroundOverlay.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }
        
        backgroundOverlay.color = endColor;
    }
    
    private IEnumerator AnimateText()
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        
        RectTransform textRect = congratsText.GetComponent<RectTransform>();
        if (textRect == null) yield break;
        
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Usar unscaledDeltaTime porque timeScale = 0
            float progress = elapsed / scaleAnimationDuration;
            float curveValue = scaleCurve.Evaluate(progress);
            
            textRect.localScale = Vector3.Lerp(startScale, endScale, curveValue);
            
            yield return null;
        }
        
        textRect.localScale = endScale;
        
        // Efecto de bounce
        StartCoroutine(BounceEffect(textRect));
    }
    
    private IEnumerator BounceEffect(RectTransform textRect)
    {
        while (completionPanel != null && completionPanel.activeSelf)
        {
            float scale = 1f + Mathf.Sin(Time.unscaledTime * 2f) * 0.1f; // Usar unscaledTime
            textRect.localScale = Vector3.one * scale;
            yield return null;
        }
    }
}