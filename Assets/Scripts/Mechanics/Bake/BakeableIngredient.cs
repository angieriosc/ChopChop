using UnityEngine;

/// <summary>
/// Estados de cocción de un ingrediente.
/// </summary>
public enum CookingState
{
    Raw,
    Cooked,
    Burned
}

/// <summary>
/// Controla un ingrediente que puede ser horneado, mostrando modelos, colores y efectos de partículas según su estado.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class BakeableIngredient : MonoBehaviour
{
    // 1. Variables públicas y serializadas
    [Header("Configuración del ingrediente")]
    [SerializeField] private string _ingredientName = "Pizza";
    [SerializeField] private CookingState _currentState = CookingState.Raw;

    [Header("Modelos visuales (baja fidelidad)")]
    [SerializeField] private GameObject _rawModel;
    [SerializeField] private GameObject _cookedModel;
    [SerializeField] private GameObject _burnedModel;

    [Header("Efectos de partículas")]
    [SerializeField] private ParticleSystem _smokeEffect;
    [SerializeField] private ParticleSystem _darkSmokeEffect;

    [Header("Cambio de color (si no hay modelos separados)")]
    [SerializeField] private bool _useColorChange = false;
    [SerializeField] private Color _rawColor = new Color(1f, 0.9f, 0.7f);
    [SerializeField] private Color _cookedColor = new Color(0.6f, 0.4f, 0.2f);
    [SerializeField] private Color _burnedColor = new Color(0.2f, 0.15f, 0.1f);

    // 2. Variables privadas
    private Renderer _objectRenderer;
    private InteractableObject _interactable;

    // 3. Métodos de Unity
    private void Awake()
    {
        _interactable = GetComponent<InteractableObject>();

        // Asegurar capacidades necesarias
        if (!_interactable.HasCapability(ObjectCapabilities.Bakeable))
        {
            _interactable.capabilities |= ObjectCapabilities.Bakeable;
            Debug.Log($"✅ Added Bakeable capability to '{_ingredientName}'");
        }

        if (!_interactable.HasCapability(ObjectCapabilities.Grabbable))
            _interactable.capabilities |= ObjectCapabilities.Grabbable;

        if (!_interactable.HasCapability(ObjectCapabilities.Droppable))
            _interactable.capabilities |= ObjectCapabilities.Droppable;
    }

    private void Start()
    {
        Debug.Log($"🍕 BakeableIngredient '{_ingredientName}' initialized. Estado: {_currentState}");

        if (GetComponent<Collider>() == null)
            Debug.LogError($"❌ ERROR: '{_ingredientName}' no tiene Collider!");
        else
            Debug.Log("✅ Tiene Collider");

        _objectRenderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();

        UpdateAppearance();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            UpdateAppearance();
    }

    // 5. Métodos públicos
    /// <summary>
    /// Cambia el estado de cocción del ingrediente.
    /// </summary>
    /// <param name="newState">Nuevo estado de cocción</param>
    public void SetState(CookingState newState)
    {
        if (_currentState == newState)
        {
            Debug.Log($"⚠️ '{_ingredientName}' ya está en estado: {newState}");
            return;
        }

        Debug.Log($"🔄 '{_ingredientName}' cambia de {_currentState} → {newState}");
        _currentState = newState;
        UpdateAppearance();
    }

    /// <summary>
    /// Retorna el estado actual de cocción.
    /// </summary>
    public CookingState GetState() => _currentState;

    /// <summary>
    /// Retorna el nombre del ingrediente.
    /// </summary>
    public string GetIngredientName() => _ingredientName;

    /// <summary>
    /// Indica si el ingrediente está correctamente cocido.
    /// </summary>
    public bool IsWellCooked() => _currentState == CookingState.Cooked;

    // 6. Métodos privados auxiliares
    /// <summary>
    /// Actualiza el aspecto visual según el estado de cocción.
    /// </summary>
    private void UpdateAppearance()
    {
        // Modelos
        if (_rawModel != null && _cookedModel != null && _burnedModel != null)
        {
            _rawModel.SetActive(_currentState == CookingState.Raw);
            _cookedModel.SetActive(_currentState == CookingState.Cooked);
            _burnedModel.SetActive(_currentState == CookingState.Burned);
        }

        // Cambio de color
        if (_useColorChange && _objectRenderer != null)
        {
            Color newColor = _currentState switch
            {
                CookingState.Raw => _rawColor,
                CookingState.Cooked => _cookedColor,
                CookingState.Burned => _burnedColor,
                _ => _rawColor
            };

            _objectRenderer.material.color = newColor;
        }

        // Efectos de partículas
        if (_smokeEffect != null)
        {
            if (_currentState == CookingState.Cooked && !_smokeEffect.isPlaying)
                _smokeEffect.Play();
            else if (_currentState != CookingState.Cooked && _smokeEffect.isPlaying)
                _smokeEffect.Stop();
        }

        if (_darkSmokeEffect != null)
        {
            if (_currentState == CookingState.Burned && !_darkSmokeEffect.isPlaying)
                _darkSmokeEffect.Play();
            else if (_currentState != CookingState.Burned && _darkSmokeEffect.isPlaying)
                _darkSmokeEffect.Stop();
        }
    }
}
