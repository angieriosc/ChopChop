using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper para crear iconos de feedback usando texto Unicode (más simple que sprites).
/// Adjunta este script a cada FeedbackIcon si prefieres usar texto en vez de imágenes.
/// </summary>
public class FeedbackIconHelper : MonoBehaviour
{
    [Header("Opción 1: Usar TextMeshProUGUI (Recomendado)")]
    [SerializeField] private TextMeshProUGUI _feedbackText;
    
    [Header("Opción 2: Usar Image con Sprites")]
    [SerializeField] private Image _feedbackImage;
    [SerializeField] private Sprite _checkmarkSprite;
    [SerializeField] private Sprite _crossSprite;
    
    [Header("Configuración")]
    [SerializeField] private bool _useText = true; // true = usar texto, false = usar sprites
    
    private void Awake()
    {
        // Si no hay referencias asignadas, buscarlas automáticamente
        if (_useText && _feedbackText == null)
        {
            _feedbackText = GetComponent<TextMeshProUGUI>();
            
            // Si no existe, crear uno
            if (_feedbackText == null)
            {
                _feedbackText = gameObject.AddComponent<TextMeshProUGUI>();
                ConfigureTextComponent();
            }
        }
        else if (!_useText && _feedbackImage == null)
        {
            _feedbackImage = GetComponent<Image>();
        }
        
        Hide();
    }
    
    /// <summary>
    /// Muestra el icono correcto (palomita verde).
    /// </summary>
    public void ShowCorrect()
    {
        gameObject.SetActive(true);
        
        if (_useText && _feedbackText != null)
        {
            _feedbackText.text = "✓"; // Unicode checkmark
            _feedbackText.color = Color.green;
        }
        else if (_feedbackImage != null)
        {
            _feedbackImage.sprite = _checkmarkSprite;
            _feedbackImage.color = Color.green;
        }
    }
    
    /// <summary>
    /// Muestra el icono incorrecto (X roja).
    /// </summary>
    public void ShowIncorrect()
    {
        gameObject.SetActive(true);
        
        if (_useText && _feedbackText != null)
        {
            _feedbackText.text = "✗"; // Unicode X mark
            _feedbackText.color = Color.red;
        }
        else if (_feedbackImage != null)
        {
            _feedbackImage.sprite = _crossSprite;
            _feedbackImage.color = Color.red;
        }
    }
    
    /// <summary>
    /// Oculta el icono.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Configura el componente de texto con valores predeterminados.
    /// </summary>
    private void ConfigureTextComponent()
    {
        if (_feedbackText == null) return;
        
        _feedbackText.fontSize = 50;
        _feedbackText.alignment = TextAlignmentOptions.Center;
        _feedbackText.fontStyle = FontStyles.Bold;
        _feedbackText.enableAutoSizing = false;
    }
}