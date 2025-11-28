using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la estación de horno, cocción de ingredientes, barra de progreso y estado del ingrediente.
/// </summary>
public class OvenStation : MonoBehaviour
{
    // 1. Variables públicas y serializadas
    [Header("Configuración del horno")]
    [SerializeField] private float _cookingTime = 10f;
    [SerializeField] private float _burningTime = 15f;

    [Header("Referencias UI")]
    [SerializeField] private GameObject _ovenCanvas;
    [SerializeField] private Image _progressBar;
    [SerializeField] private Image _backgroundBar;
    [SerializeField] private TextMeshProUGUI _stateText;

    [Header("Colores de barra")]
    [SerializeField] private Color _cookingColor = Color.yellow;
    [SerializeField] private Color _readyColor = Color.green;
    [SerializeField] private Color _burningColor = Color.red;

    [Header("Punto de colocación")]
    [SerializeField] private Transform _ingredientPoint;
    [SerializeField] private Vector3 _positionOffset = Vector3.zero;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _cookingSound;
    [SerializeField] private AudioClip _readySound;
    [SerializeField] private AudioClip _burnedSound;

    // 2. Variables privadas
    private GameObject _currentIngredient;
    private BakeableIngredient _bakeableData;
    private float _currentTime = 0f;
    private bool _isCooking = false;
    private CookingStage _currentStage = CookingStage.Raw;
    private Vector3 _originalPosition;
    private bool _readySoundPlayed = false;

    private enum CookingStage
    {
        Raw,
        Cooking,
        Ready,
        Burning,
        Burned
    }

    // 3. Métodos de Unity
    private void Start()
    {
        if (_ovenCanvas != null) _ovenCanvas.SetActive(false);
        
        // Crear AudioSource si no existe
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
        }
    }

    private void Update()
    {
        if (_isCooking && _currentIngredient != null)
            UpdateCooking();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isCooking ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);

        if (_ingredientPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_ingredientPoint.position + _positionOffset, Vector3.one * 0.5f);
        }
    }

    // 6. Métodos privados auxiliares
    /// <summary>
    /// Actualiza el proceso de cocción y la barra de progreso.
    /// </summary>
    private void UpdateCooking()
    {
        _currentTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_currentTime / _cookingTime);

        if (_currentTime < _cookingTime)
        {
            _currentStage = CookingStage.Cooking;
            _progressBar.fillAmount = progress;
            _progressBar.color = _cookingColor;
            _stateText.text = $"Cocinando... {Mathf.CeilToInt(_cookingTime - _currentTime)}s";
        }
        else if (_currentTime >= _cookingTime && _currentTime < _burningTime)
        {
            if (_currentStage == CookingStage.Cooking)
            {
                _currentStage = CookingStage.Ready;
                ChangeIngredientAppearance(CookingState.Cooked);
                
                // Reproducir sonido de listo (una sola vez)
                if (!_readySoundPlayed)
                {
                    PlaySound(_readySound);
                    _readySoundPlayed = true;
                }
            }

            _progressBar.fillAmount = 1f;
            _progressBar.color = _readyColor;
            _stateText.text = "Cocinado! Tomalo ahora";
        }
        else
        {
            if (_currentStage != CookingStage.Burned)
            {
                _currentStage = CookingStage.Burning;
                _progressBar.fillAmount = 1f;
                _progressBar.color = _burningColor;
                _stateText.text = "BURNING!";

                if (_currentTime >= _burningTime)
                {
                    _currentStage = CookingStage.Burned;
                    ChangeIngredientAppearance(CookingState.Burned);
                    _stateText.text = "Quemado";
                    
                    // Detener sonido de cocción y reproducir sonido de quemado
                    StopCookingSound();
                    PlaySound(_burnedSound);

                    if (PatienceManager.Instance != null)
                    {
                        PatienceManager.Instance.ApplyPenalty(PenaltyType.BurntPizza);
                    }
                }
            }
        }
    }

    // 5. Métodos públicos
    /// <summary>
    /// Coloca un ingrediente en el horno y comienza la cocción.
    /// </summary>
    public bool PutInOven(GameObject ingredient)
    {
        if (_isCooking || _currentIngredient != null) return false;

        BakeableIngredient bakeable = ingredient.GetComponent<BakeableIngredient>() 
            ?? ingredient.transform.parent?.GetComponent<BakeableIngredient>();

        if (bakeable == null) return false;

        InteractableObject interactable = bakeable.GetComponent<InteractableObject>();
        if (interactable == null || !interactable.HasCapability(ObjectCapabilities.Bakeable)) return false;

        GameObject realObject = bakeable.gameObject;
        _originalPosition = realObject.transform.position;

        Rigidbody rb = realObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        realObject.transform.SetParent(_ingredientPoint);
        realObject.transform.localPosition = _positionOffset;
        realObject.transform.localRotation = Quaternion.identity;

        Collider col = realObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        _currentIngredient = realObject;
        _bakeableData = bakeable;
        _isCooking = true;
        _currentTime = 0f;
        _currentStage = CookingStage.Cooking;
        _readySoundPlayed = false;

        if (_ovenCanvas != null)
        {
            _ovenCanvas.SetActive(true);
            _progressBar.fillAmount = 0f;
        }

        // Reproducir sonido de cocción en loop
        PlayCookingSound();

        return true;
    }

    /// <summary>
    /// Retira el ingrediente del horno, restaurando su posición y estado.
    /// </summary>
    public GameObject TakeFromOven()
    {
        if (_currentIngredient == null) return null;

        if (_currentStage == CookingStage.Raw || _currentStage == CookingStage.Cooking)
        {
            if (PatienceManager.Instance != null)
            {
                PatienceManager.Instance.ApplyPenalty(PenaltyType.Undercooked);
            }
        }

        GameObject ingredient = _currentIngredient;
        InteractableObject interactable = ingredient.GetComponent<InteractableObject>();

        switch (_currentStage)
        {
            case CookingStage.Ready:
                _bakeableData.SetState(CookingState.Cooked);
                break;
            case CookingStage.Burning:
            case CookingStage.Burned:
                _bakeableData.SetState(CookingState.Burned);
                break;
            default:
                _bakeableData.SetState(CookingState.Raw);
                break;
        }

        if (interactable != null)
        {
            interactable.RemoveCapability(ObjectCapabilities.Bakeable);
            interactable.AddCapability(ObjectCapabilities.Cuttable);
        }

        ingredient.transform.SetParent(null);
        ingredient.transform.position = _originalPosition;

        Collider col = ingredient.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = ingredient.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("Restoring Rigidbody properties.");
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (_ovenCanvas != null) _ovenCanvas.SetActive(false);

        // Detener cualquier sonido que esté reproduciéndose
        StopAllSounds();

        _currentIngredient = null;
        _bakeableData = null;
        _isCooking = false;
        _currentTime = 0f;
        _currentStage = CookingStage.Raw;
        _readySoundPlayed = false;

        return ingredient;
    }

    /// <summary>
    /// Retorna si el horno está disponible.
    /// </summary>
    public bool IsAvailable() => !_isCooking && _currentIngredient == null;

    /// <summary>
    /// Retorna si hay un ingrediente actualmente en el horno.
    /// </summary>
    public bool HasIngredient() => _currentIngredient != null;

    // 6. Métodos privados auxiliares
    private void ChangeIngredientAppearance(CookingState newState)
    {
        _bakeableData?.SetState(newState);
    }

    /// <summary>
    /// Reproduce el sonido de cocción en loop.
    /// </summary>
    private void PlayCookingSound()
    {
        if (_audioSource != null && _cookingSound != null)
        {
            _audioSource.clip = _cookingSound;
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }

    /// <summary>
    /// Detiene el sonido de cocción.
    /// </summary>
    private void StopCookingSound()
    {
        if (_audioSource != null && _audioSource.isPlaying && _audioSource.clip == _cookingSound)
        {
            _audioSource.Stop();
            _audioSource.loop = false;
        }
    }

    /// <summary>
    /// Reproduce un sonido específico (one-shot).
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Detiene todos los sonidos.
    /// </summary>
    private void StopAllSounds()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.loop = false;
        }
    }
}