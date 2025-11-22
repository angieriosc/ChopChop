using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Muestra prompts de interacción flotantes al jugador.
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Animation")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 10f;
    
    [Header("Tutorial Settings")]
    [SerializeField] private bool disableAfterFirstPurchase = true;
    
    public static InteractionPrompt Instance { get; private set; }
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private bool isVisible = false;
    private bool hasCompletedFirstPurchase = false;
    
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
        
        if (promptPanel != null)
        {
            canvasGroup = promptPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = promptPanel.AddComponent<CanvasGroup>();
            }
            
            rectTransform = promptPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalPosition = rectTransform.anchoredPosition;
            }
            
            canvasGroup.alpha = 0f;
            promptPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("❌ InteractionPrompt: promptPanel no está asignado!");
        }
    }
    
    private void Update()
    {
        if (isVisible && rectTransform != null)
        {
            // Animación de bobbing (flotante)
            float newY = originalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            rectTransform.anchoredPosition = new Vector3(originalPosition.x, newY, 0);
        }
    }
    
    /// <summary>
    /// Muestra el prompt con el texto especificado.
    /// </summary>
    public void ShowPrompt(string text)
    {
        // Si ya completó la primera compra y está configurado para desactivar, no mostrar
        if (disableAfterFirstPurchase && hasCompletedFirstPurchase)
        {
            return;
        }
        
        if (promptPanel == null)
        {
            Debug.LogWarning("⚠️ InteractionPrompt: No hay panel asignado");
            return;
        }
        
        // Activar el panel si no está activo
        if (!promptPanel.activeSelf)
        {
            promptPanel.SetActive(true);
        }
        
        isVisible = true;
        
        if (promptText != null)
        {
            promptText.text = text;
        }
        
        StopAllCoroutines();
        StartCoroutine(FadeIn());
        
        Debug.Log($"💬 Mostrando prompt: {text}");
    }
    
    /// <summary>
    /// Oculta el prompt.
    /// </summary>
    public void HidePrompt()
    {
        if (promptPanel == null) return;
        
        // ✅ SOLUCIÓN: Solo ocultar si está visible/activo
        if (!isVisible || !promptPanel.activeSelf)
        {
            return; // Ya está oculto, no hacer nada
        }
        
        isVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// Oculta el prompt inmediatamente sin animación.
    /// </summary>
    public void HidePromptImmediate()
    {
        if (promptPanel == null || canvasGroup == null) return;
        
        StopAllCoroutines();
        
        isVisible = false;
        canvasGroup.alpha = 0f;
        promptPanel.SetActive(false);
        
        Debug.Log("💤 Prompt ocultado inmediatamente");
    }
    
    /// <summary>
    /// Marca que se completó la primera compra y desactiva los prompts de tutorial.
    /// Llamar este método desde CheckoutZone después de un pago exitoso.
    /// </summary>
    public void OnFirstPurchaseComplete()
    {
        if (!hasCompletedFirstPurchase)
        {
            hasCompletedFirstPurchase = true;
            
            // Ocultar el prompt actual si está visible
            if (isVisible)
            {
                HidePromptImmediate();
            }
            
            Debug.Log("✅ Tutorial completado - Prompts desactivados");
        }
    }
    
    /// <summary>
    /// Reactiva los prompts (útil si quieres volver a mostrarlos).
    /// </summary>
    public void ResetTutorial()
    {
        hasCompletedFirstPurchase = false;
        Debug.Log("🔄 Tutorial reactivado");
    }
    
    /// <summary>
    /// Verifica si los prompts están activos (útil para debugging).
    /// </summary>
    public bool ArePromptsEnabled()
    {
        return !hasCompletedFirstPurchase || !disableAfterFirstPurchase;
    }
    
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
}