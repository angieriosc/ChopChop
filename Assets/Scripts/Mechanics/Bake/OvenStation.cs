using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la estación de horno con sistema de reto de divisiones.
/// </summary>
public class OvenStation : MonoBehaviour
{
    [Header("Configuración del horno")]
    [SerializeField] private float _cookingTime = 10f;
    [SerializeField] private float _burningTime = 15f;
    [SerializeField] private float _readyToBurnedTime = 5f; // Tiempo fijo de "Cocinado" a "Quemado" (sin bonus)

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
    
    [Header("Sistema de Reto")]
    [SerializeField] private DivisionChallenge _divisionChallenge;
    [SerializeField] private TextMeshProUGUI _bonusIndicator; // Muestra "x3 Speed!" en el horno

    private GameObject _currentIngredient;
    private BakeableIngredient _bakeableData;
    private float _currentTime = 0f;
    private bool _isCooking = false;
    private bool _isWaitingForChallenge = false;
    private CookingStage _currentStage = CookingStage.Raw;
    private Vector3 _originalPosition;
    private bool _readySoundPlayed = false;
    private int _currentBonusMultiplier = 1;
    
    // Variables temporales para guardar el ingrediente durante el reto
    private GameObject _pendingIngredient;

    private enum CookingStage
    {
        Raw,
        Cooking,
        Ready,
        Burning,
        Burned
    }

    private void Start()
    {
        if (_ovenCanvas != null) _ovenCanvas.SetActive(false);
        
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
        }
        
        // Suscribirse a eventos del reto
        if (_divisionChallenge != null)
        {
            _divisionChallenge.OnChallengeCompleted += OnChallengeCompleted;
            _divisionChallenge.OnChallengeFailed += OnChallengeFailed;
        }
        
        if (_bonusIndicator != null)
        {
            _bonusIndicator.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (_divisionChallenge != null)
        {
            _divisionChallenge.OnChallengeCompleted -= OnChallengeCompleted;
            _divisionChallenge.OnChallengeFailed -= OnChallengeFailed;
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

    private void UpdateCooking()
    {
        _currentTime += Time.deltaTime;
        
        // Calcular tiempo de cocción ajustado por el bonus
        float adjustedCookingTime = _cookingTime / _currentBonusMultiplier;
        
        // El tiempo de quemado NO se divide por el bonus, siempre es fijo
        float adjustedBurningTime = adjustedCookingTime + _readyToBurnedTime;
        
        float progress = Mathf.Clamp01(_currentTime / adjustedCookingTime);

        if (_currentTime < adjustedCookingTime)
        {
            _currentStage = CookingStage.Cooking;
            _progressBar.fillAmount = progress;
            _progressBar.color = _cookingColor;
            _stateText.text = $"Cocinando... {Mathf.CeilToInt(adjustedCookingTime - _currentTime)}s";
        }
        else if (_currentTime >= adjustedCookingTime && _currentTime < adjustedBurningTime)
        {
            if (_currentStage == CookingStage.Cooking)
            {
                _currentStage = CookingStage.Ready;
                ChangeIngredientAppearance(CookingState.Cooked);
                
                if (!_readySoundPlayed)
                {
                    PlaySound(_readySound);
                    _readySoundPlayed = true;
                }
            }

            _progressBar.fillAmount = 1f;
            _progressBar.color = _readyColor;
            
            // Mostrar cuenta regresiva de cuánto tiempo queda antes de quemarse
            float timeLeftToburn = adjustedBurningTime - _currentTime;
            _stateText.text = $"¡Cocinado! Tómalo ahora ({Mathf.CeilToInt(timeLeftToburn)}s)";
        }
        else
        {
            if (_currentStage != CookingStage.Burned)
            {
                _currentStage = CookingStage.Burning;
                _progressBar.fillAmount = 1f;
                _progressBar.color = _burningColor;
                _stateText.text = "¡QUEMANDO!";

                if (_currentTime >= adjustedBurningTime)
                {
                    _currentStage = CookingStage.Burned;
                    ChangeIngredientAppearance(CookingState.Burned);
                    _stateText.text = "Quemado";
                    
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

    /// <summary>
    /// Intenta poner un ingrediente en el horno (inicia el reto).
    /// </summary>
    public bool PutInOven(GameObject ingredient)
    {
        if (_isCooking || _currentIngredient != null || _isWaitingForChallenge) return false;

        BakeableIngredient bakeable = ingredient.GetComponent<BakeableIngredient>() 
            ?? ingredient.transform.parent?.GetComponent<BakeableIngredient>();

        if (bakeable == null) return false;

        InteractableObject interactable = bakeable.GetComponent<InteractableObject>();
        if (interactable == null || !interactable.HasCapability(ObjectCapabilities.Bakeable)) return false;

        // Guardar el ingrediente y preparar para el reto
        _pendingIngredient = bakeable.gameObject;
        _originalPosition = _pendingIngredient.transform.position;
        _isWaitingForChallenge = true;
        
        // Iniciar el reto de divisiones
        if (_divisionChallenge != null)
        {
            _divisionChallenge.StartChallenge();
        }

        return true;
    }

    /// <summary>
    /// Llamado cuando el reto se completa exitosamente.
    /// </summary>
    private void OnChallengeCompleted(int bonusMultiplier)
    {
        _isWaitingForChallenge = false;
        _currentBonusMultiplier = bonusMultiplier;
        
        // Mostrar indicador de bonus
        if (_bonusIndicator != null && bonusMultiplier > 1)
        {
            _bonusIndicator.gameObject.SetActive(true);
            _bonusIndicator.text = $"x{bonusMultiplier} Velocidad!";
        }
        
        // Ahora sí, poner el ingrediente en el horno
        StartCooking();
    }

    /// <summary>
    /// Llamado cuando el reto falla.
    /// </summary>
    private void OnChallengeFailed()
    {
        _isWaitingForChallenge = false;
        _pendingIngredient = null;
        Debug.Log("Reto fallido. El horno no se activó.");
    }

    /// <summary>
    /// Inicia el proceso de cocción después de completar el reto.
    /// </summary>
    private void StartCooking()
    {
        if (_pendingIngredient == null) return;

        GameObject realObject = _pendingIngredient;
        
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
        _bakeableData = realObject.GetComponent<BakeableIngredient>();
        _isCooking = true;
        _currentTime = 0f;
        _currentStage = CookingStage.Cooking;
        _readySoundPlayed = false;
        _pendingIngredient = null;

        if (_ovenCanvas != null)
        {
            _ovenCanvas.SetActive(true);
            _progressBar.fillAmount = 0f;
        }

        PlayCookingSound();
    }

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
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (_ovenCanvas != null) _ovenCanvas.SetActive(false);
        if (_bonusIndicator != null) _bonusIndicator.gameObject.SetActive(false);

        StopAllSounds();

        _currentIngredient = null;
        _bakeableData = null;
        _isCooking = false;
        _currentTime = 0f;
        _currentStage = CookingStage.Raw;
        _readySoundPlayed = false;
        _currentBonusMultiplier = 1;

        DialogueSequenceRunner sequenceManager = FindFirstObjectByType<DialogueSequenceRunner>();
        if (sequenceManager.stepIndex==6)
        {
            //Continuar cinematica
            StartCoroutine(sequenceManager.ContinueSequence());
        }

        return ingredient;
    }

    public bool IsAvailable() => !_isCooking && _currentIngredient == null && !_isWaitingForChallenge;
    public bool HasIngredient() => _currentIngredient != null;

    private void ChangeIngredientAppearance(CookingState newState)
    {
        _bakeableData?.SetState(newState);
    }

    private void PlayCookingSound()
    {
        if (_audioSource != null && _cookingSound != null)
        {
            _audioSource.clip = _cookingSound;
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }

    private void StopCookingSound()
    {
        if (_audioSource != null && _audioSource.isPlaying && _audioSource.clip == _cookingSound)
        {
            _audioSource.Stop();
            _audioSource.loop = false;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    private void StopAllSounds()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.loop = false;
        }
    }
}