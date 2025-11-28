using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animador de botones usando solo Coroutines de Unity (sin librerías externas).
/// VERSIÓN CORREGIDA: Funciona correctamente con Unity UI (RectTransform).
/// </summary>
public class ButtonAnimator : MonoBehaviour
{
    [Header("Configuración de Animaciones")]
    [SerializeField] private float _correctScaleDuration = 0.2f;
    [SerializeField] private float _correctScaleAmount = 1.2f;
    [SerializeField] private float _incorrectRotationAmount = 10f;
    [SerializeField] private float _incorrectShakeDuration = 0.1f;
    [SerializeField] private int _incorrectShakeCount = 2;
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;
    
    private Button _button;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private RectTransform _rectTransform;
    
    private Coroutine _currentAnimation;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();
        
        if (_rectTransform == null)
        {
            Debug.LogError($"ButtonAnimator en {gameObject.name}: No se encontró RectTransform! Este script solo funciona con UI de Unity.");
            return;
        }
        
        _originalScale = _rectTransform.localScale;
        _originalRotation = _rectTransform.localRotation;
        
        if (_showDebugLogs)
        {
            Debug.Log($"ButtonAnimator inicializado en {gameObject.name}. Escala original: {_originalScale}");
        }
    }
    
    /// <summary>
    /// Anima el botón cuando la respuesta es correcta (scale bounce).
    /// </summary>
    public void AnimateCorrect()
    {
        if (_rectTransform == null) return;
        
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);
        
        if (_showDebugLogs)
            Debug.Log($"AnimateCorrect llamado en {gameObject.name}");
        
        _currentAnimation = StartCoroutine(CorrectAnimation());
    }
    
    /// <summary>
    /// Anima el botón cuando la respuesta es incorrecta (shake).
    /// </summary>
    public void AnimateIncorrect()
    {
        if (_rectTransform == null) return;
        
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);
        
        if (_showDebugLogs)
            Debug.Log($"AnimateIncorrect llamado en {gameObject.name}");
        
        _currentAnimation = StartCoroutine(IncorrectAnimation());
    }
    
    /// <summary>
    /// Resetea el botón a su estado original.
    /// </summary>
    public void ResetAnimation()
    {
        if (_rectTransform == null) return;
        
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);
        
        _rectTransform.localScale = _originalScale;
        _rectTransform.localRotation = _originalRotation;
        
        if (_showDebugLogs)
            Debug.Log($"ResetAnimation llamado en {gameObject.name}");
    }
    
    // Animación de respuesta correcta: escala grande → escala normal
    private IEnumerator CorrectAnimation()
    {
        Vector3 startScale = _rectTransform.localScale;
        Vector3 targetScale = _originalScale * _correctScaleAmount;
        
        // Fase 1: Escalar hacia arriba
        float elapsed = 0f;
        
        while (elapsed < _correctScaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _correctScaleDuration;
            
            // Ease out back (rebote)
            t = EaseOutBack(t);
            
            _rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        _rectTransform.localScale = targetScale;
        
        // Fase 2: Regresar a escala normal
        elapsed = 0f;
        
        while (elapsed < _correctScaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _correctScaleDuration;
            
            // Ease out para suavidad
            t = EaseOutQuad(t);
            
            _rectTransform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
            yield return null;
        }
        
        _rectTransform.localScale = _originalScale;
        _currentAnimation = null;
        
        if (_showDebugLogs)
            Debug.Log($"CorrectAnimation completada en {gameObject.name}");
    }
    
    // Animación de respuesta incorrecta: shake horizontal
    private IEnumerator IncorrectAnimation()
    {
        Quaternion startRotation = _rectTransform.localRotation;
        
        for (int i = 0; i < _incorrectShakeCount; i++)
        {
            // Rotar a la izquierda
            float elapsed = 0f;
            Quaternion leftRotation = Quaternion.Euler(0, 0, -_incorrectRotationAmount);
            
            while (elapsed < _incorrectShakeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _incorrectShakeDuration;
                
                _rectTransform.localRotation = Quaternion.Lerp(_originalRotation, leftRotation, t);
                yield return null;
            }
            
            // Rotar a la derecha
            elapsed = 0f;
            Quaternion rightRotation = Quaternion.Euler(0, 0, _incorrectRotationAmount);
            
            while (elapsed < _incorrectShakeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _incorrectShakeDuration;
                
                _rectTransform.localRotation = Quaternion.Lerp(leftRotation, rightRotation, t);
                yield return null;
            }
        }
        
        // Regresar a rotación original
        float finalElapsed = 0f;
        Quaternion currentRotation = _rectTransform.localRotation;
        
        while (finalElapsed < _incorrectShakeDuration)
        {
            finalElapsed += Time.deltaTime;
            float t = finalElapsed / _incorrectShakeDuration;
            
            // Ease out para suavidad
            t = EaseOutQuad(t);
            
            _rectTransform.localRotation = Quaternion.Lerp(currentRotation, _originalRotation, t);
            yield return null;
        }
        
        _rectTransform.localRotation = _originalRotation;
        _currentAnimation = null;
        
        if (_showDebugLogs)
            Debug.Log($"IncorrectAnimation completada en {gameObject.name}");
    }
    
    // Función de easing para animación más natural (bounce)
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    // Función de easing suave
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}