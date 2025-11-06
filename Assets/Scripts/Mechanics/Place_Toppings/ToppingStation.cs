using UnityEngine;

/// <summary>
/// Administra la lógica de colocar y retirar la pizza en la estación de toppings.
/// Activa/desactiva cámaras y UI, y habilita el PizzaToppingManager.
/// </summary>
public class ToppingStation : MonoBehaviour
{
    // 1. Variables públicas y serializadas
    [Header("Cámaras y UI")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _stationCamera;
    [SerializeField] private GameObject _stationCanvas;

    [Header("Manager de Toppings")]
    [SerializeField] private PizzaToppingManager _toppingManager;

    [Header("Punto donde se coloca la pizza")]
    [SerializeField] private Transform _ingredientPoint;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _snapYOffset = 0.01f;

    // 2. Variables privadas
    private GameObject _currentPizza;
    private Rigidbody _rb;
    private Collider _col;
    private bool _hasPizza;

    /// <summary>
    /// Intenta colocar la pizza en la estación. Valida que sea un objeto utilizable y la centra.
    /// </summary>
    /// <param name="pizzaObj">Instancia de la pizza que se intenta colocar.</param>
    /// <returns>True si se colocó correctamente; false en caso contrario.</returns>
    public bool TryPlace(GameObject pizzaObj)
    {
        if (_hasPizza || pizzaObj == null) return false;

        var identifiers = pizzaObj.GetComponentInParent<PizzaIdentifiers>();
        Transform root = identifiers != null ? identifiers.PizzaRoot : pizzaObj.transform;

        // Guardar referencia y congelar físicas
        _currentPizza = root.gameObject;
        _hasPizza = true;

        _rb = root.GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = false;
            _rb.isKinematic = true;
        }

        _col = root.GetComponent<Collider>();
        if (_col != null) _col.enabled = false;

        // Colocación exacta en el centro de la estación
        root.SetParent(_ingredientPoint, true);
        Vector3 worldPos = _ingredientPoint.TransformPoint(_offset + Vector3.up * _snapYOffset);
        root.position = worldPos;
        root.rotation = _ingredientPoint.rotation;
        
        // Configurar el manager de toppings
        if (identifiers != null)
        {
            _toppingManager.PizzaRoot = identifiers.PizzaRoot;
            _toppingManager.PizzaSurfaceCollider = identifiers.PizzaSurface;
            _toppingManager.PizzaCenter = identifiers.PizzaCenter != null ? identifiers.PizzaCenter : identifiers.PizzaRoot;
        }

        _toppingManager.WorkCamera = _stationCamera;
        _toppingManager.SetEnabled(true);

        // Activar UI y cámara de estación
        _playerCamera.gameObject.SetActive(false);
        _stationCamera.gameObject.SetActive(true);
        _stationCanvas.SetActive(true);

        return true;
    }

    /// <summary>
    /// Libera la pizza y restaura cámaras, físicas y UI.
    /// </summary>
    public GameObject TakePizza()
    {
        if (!_hasPizza || _currentPizza == null) return null;

        var root = _currentPizza.transform;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

        if (_col != null) _col.enabled = true;

        root.SetParent(null, true);

        _toppingManager.SetEnabled(false);
        _toppingManager.WorkCamera = null;
        _toppingManager.ClearSelection();

        _playerCamera.gameObject.SetActive(true);
        _stationCamera.gameObject.SetActive(false);
        _stationCanvas.SetActive(false);

        _hasPizza = false;
        _currentPizza = null;

        return root.gameObject;
    }

    /// <summary>Indica si la estación está libre.</summary>
    public bool IsAvailable() => !_hasPizza;

    /// <summary>Indica si hay una pizza actualmente en la estación.</summary>
    public bool HasPizza() => _hasPizza;
}
