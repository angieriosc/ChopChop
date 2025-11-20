using UnityEngine;

/// <summary>
/// Administra la lógica de entrar/salir de la estación de toppings.
/// Ya no requiere llevar una pizza en la mano.
/// La primera masa que coloques se convierte en el root de la pizza
/// y al salir se devuelve ese objeto (masa + toppings hijos).
/// </summary>
public class ToppingStation : MonoBehaviour
{
    [Header("Cámaras y UI")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _stationCamera;
    [SerializeField] private GameObject _stationCanvas;

    [Header("Manager de Toppings")]
    [SerializeField] private PizzaToppingManager _toppingManager;

    [Header("Superficie de la mesa (para el raycast INICIAL)")]
    [SerializeField] private Collider _pizzaSurfaceCollider;

    [Header("UI instrucciones")]
    [SerializeField] private GameObject _instructionImage;

    [SerializeField] private bool _playerInStation = false;
    private bool _hasPizza = false;  // en este flujo = "estación en uso"


    /// <summary>
    /// Entra a la estación, configura el manager (raycast en mesa) y activa cámaras/UI.
    /// </summary>
    public bool TryPlace()
    {
        if (_hasPizza) return false;
        if (!_playerInStation) return false;

        if (_toppingManager != null)
        {
            _toppingManager.PizzaRoot = null; // todavía no hay masa
            _toppingManager.PizzaCenter = null;
            _toppingManager.PizzaSurfaceCollider = _pizzaSurfaceCollider;
            _toppingManager.WorkCamera = _stationCamera;
            _toppingManager.SetEnabled(true);
        }

        if (_playerCamera != null) _playerCamera.gameObject.SetActive(false);
        if (_stationCamera != null) _stationCamera.gameObject.SetActive(true);
        if (_stationCanvas != null) _stationCanvas.SetActive(true);
        if (_instructionImage != null) _instructionImage.SetActive(true);
        ToppingLock.IsLocked = true;

        Cursor.visible = true;

        _hasPizza = true; // Estamos usando la estación (creando una pizza)
        Debug.Log("[ToppingStation] Entrando a estación de toppings.");
        return true;
    }

    /// <summary>
    /// Sale de la estación y devuelve la pizza completa (masa + toppings) si existe.
    /// </summary>
    public GameObject TakePizza()
    {
        if (!_hasPizza) return null;

        GameObject pizzaResult = null;

        if (_toppingManager.PizzaRoot != null)
        {
            pizzaResult = _toppingManager.PizzaRoot.gameObject;
            pizzaResult.transform.SetParent(null, true);

            // Activar físicas
            var rb = pizzaResult.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            var col = pizzaResult.GetComponent<Collider>();
            if (col != null)
                col.enabled = true;
        }

        // Apagar cámara y manager
        _toppingManager.SetEnabled(false);
        _toppingManager.WorkCamera = null;
        _toppingManager.ClearSelection();

        _playerCamera.gameObject.SetActive(true);
        _stationCamera.gameObject.SetActive(false);
        _stationCanvas.SetActive(false);
        ToppingLock.IsLocked = false;

        Cursor.visible = true;

        _hasPizza = false;

        return pizzaResult;
    }

    /// <summary>Muestra si el jugador entro a la estación</summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInStation = true;
            Debug.Log("[ToppingStation] Jugador entró en la estación");
        }
    }

    /// <summary>Muestra si el jugador salio de la estación</summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ToppingLock.IsLocked = false;
            _playerInStation = false;
            Debug.Log("[ToppingStation] Jugador salió de la estación");
        }
    }
    /// <summary>Esconde las instrucciones</summary>
    public void HideInstruction()
    {
        if (_instructionImage != null)
            _instructionImage.SetActive(false);
    }

    /// <summary>Indica si la estación está libre.</summary>
    public bool IsAvailable() => !_hasPizza;

    /// <summary>En este flujo: true = estación en uso (hay pizza en construcción o ya hecha).</summary>
    public bool HasPizza() => _hasPizza;

    /// <summary>Indica si un jugador está actualmente en la estación.</summary>
    public bool IsPlayerInside() => _playerInStation;
}
