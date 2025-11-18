using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el spawn de toppings sobre la pizza.
/// Obliga a poner la masa primero (usando inventario) y luego toppings.
/// Después de poner la masa, el raycast se hace sobre la masa, no la mesa.
/// </summary>
public class PizzaToppingManager : MonoBehaviour
{
    [Header("Cámara de trabajo asignada por ToppingStation")]
    [SerializeField] private Camera _workCamera;

    [Header("Referencias de pizza / superficie")]
    [SerializeField] private Transform _pizzaRoot;           // Se asigna dinámicamente a la masa
    [SerializeField] private Collider _pizzaSurfaceCollider; // Superficie de la mesa INICIAL
    [SerializeField] private Transform _pizzaCenter;
    [SerializeField] private float _surfaceYOffset = 0.01f;

    [Header("Toppings disponibles")]
    [SerializeField] private List<ToppingOption> _toppings = new();

    [Header("Opciones de spawn")]
    [SerializeField] private Vector3 _extraScale = Vector3.one;

    private bool _enabled;
    private GameObject _currentToppingPrefab;

    public Camera WorkCamera { get => _workCamera; set => _workCamera = value; }
    public Transform PizzaRoot { get => _pizzaRoot; set => _pizzaRoot = value; }
    public Collider PizzaSurfaceCollider { get => _pizzaSurfaceCollider; set => _pizzaSurfaceCollider = value; }
    public Transform PizzaCenter { get => _pizzaCenter; set => _pizzaCenter = value; }

    [Header("Ghost Topping cursor")]
    [SerializeField] private Material _ghostMaterial;
    [SerializeField] private bool _hideSystemCursorWithGhost = false;
    [SerializeField] private bool _autoToggleSystemCursor = true;

    private GameObject _ghostInstance;
    private bool _ghostVisible = false;

    [Header("Stats (opcional)")]
    private int _numPeppersPlaced = 0;
    private int _numTomatoesPlaced = 0;

    [Header("Lógica de masa/base")]
    [Tooltip("Index en la lista de toppings que corresponde a la masa/base (slot 1 en la UI).")]
    [SerializeField] private int _baseDoughToppingIndex = 0;

    [Tooltip("Clave de inventario (CuttingInventory.itemKey) que representa la masa/base.")]
    [SerializeField] private string _baseDoughInventoryKey = "WedgeSlice";

    private bool _baseDoughPlaced = false;
    private int _currentToppingIndex = -1;

    /// <summary>Activa o desactiva el manager (se usa al entrar/salir de la estación).</summary>
    public void SetEnabled(bool value)
    {
        _enabled = value;

        if (!value)
        {
            _baseDoughPlaced = false;
            _currentToppingIndex = -1;
            _pizzaRoot = null;
            ClearSelection();
        }
    }

    /// <summary>Selecciona un topping desde UI.</summary>
    public void SelectToppingByIndex(int index)
    {
        if (index < 0 || index >= _toppings.Count) return;

        _currentToppingIndex = index;
        _currentToppingPrefab = _toppings[index].prefab;

        if (_ghostInstance != null)
            Destroy(_ghostInstance);

        _ghostInstance = Instantiate(_currentToppingPrefab);
        _ghostInstance.transform.localScale =
            Vector3.Scale(_ghostInstance.transform.localScale, _extraScale);
        _ghostInstance.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (_ghostMaterial != null)
        {
            foreach (var r in _ghostInstance.GetComponentsInChildren<Renderer>())
                r.material = _ghostMaterial;
        }

        SetGhostVisible(false);
    }

    /// <summary>Limpia la selección actual de topping.</summary>
    public void ClearSelection()
    {
        _currentToppingPrefab = null;
        _currentToppingIndex = -1;

        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }

        _ghostVisible = false;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (!_enabled || WorkCamera == null || _currentToppingPrefab == null)
            return;

        bool isBaseDoughSelected = (_currentToppingIndex == _baseDoughToppingIndex);

        // --- Reglas de masa/base ---
        if (!_baseDoughPlaced)
        {
            // Si aún no hay masa, solo se permite si el topping seleccionado ES la masa
            if (!isBaseDoughSelected)
                return;

            // Revisar inventario de masa
            if (CuttingInventory.Instance != null)
            {
                int qty = CuttingInventory.Instance.GetQuantity(_baseDoughInventoryKey);
                if (qty <= 0)
                {
                    Debug.Log($"[PizzaToppingManager] No hay masa '{_baseDoughInventoryKey}' en CuttingInventory.");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[PizzaToppingManager] No existe CuttingInventory.Instance en la escena.");
                return;
            }
        }
        else
        {
            if (isBaseDoughSelected)
                return;
        }

        Collider surfaceCollider = null;

        if (_baseDoughPlaced && _pizzaRoot != null)
        {
            surfaceCollider = _pizzaRoot.GetComponentInChildren<Collider>();
        }
        else
        {
            surfaceCollider = PizzaSurfaceCollider;
        }

        if (surfaceCollider == null)
            return;

        Ray ray = WorkCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (surfaceCollider.Raycast(ray, out var hit, 100f))
        {
            Vector3 target = hit.point + hit.normal * _surfaceYOffset;

            SetGhostVisible(true);

            if (_ghostInstance != null)
            {
                _ghostInstance.transform.position = target;
                _ghostInstance.transform.rotation =
                    Quaternion.LookRotation(Vector3.forward, hit.normal);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
                SpawnTopping(target, hit.normal, _currentToppingPrefab);
        }
        else
        {
            SetGhostVisible(false);
        }
    }

    /// <summary>
    /// Instancia un topping sobre la pizza en la posición indicada.
    /// Si es la masa, establece la masa como PizzaRoot.
    /// </summary>
    private void SpawnTopping(Vector3 position, Vector3 normal, GameObject prefab)
    {
        if (prefab == null) return;

        Quaternion rot = Quaternion.LookRotation(Vector3.forward, normal);

        if (!_baseDoughPlaced && _currentToppingIndex == _baseDoughToppingIndex)
        {
            GameObject dough = Instantiate(prefab, position, rot);
            dough.transform.localScale = Vector3.Scale(dough.transform.localScale, _extraScale);

            _pizzaRoot = dough.transform;
            _pizzaCenter = _pizzaRoot;
            _baseDoughPlaced = true;
            Debug.Log("[PizzaToppingManager] Masa colocada en la estación.");

            ToppingStation station = FindObjectOfType<ToppingStation>();
            if (station != null)
                station.HideInstruction();

            return;
        }

        if (_pizzaRoot == null)
        {
            Debug.LogWarning("[PizzaToppingManager] Intento de colocar toppings sin masa.");
            return;
        }

        if (prefab.name == "pimiento") _numPeppersPlaced++;
        if (prefab.name == "tomatoSlice") _numTomatoesPlaced++;

        GameObject go = Instantiate(prefab, position, rot, _pizzaRoot);
        go.transform.localScale = Vector3.Scale(go.transform.localScale, _extraScale);

        {
            ToppingStation station = FindObjectOfType<ToppingStation>();
            if (station != null)
                station.HideInstruction();
        }
    }
    /// <summary>
    /// Destruye el topping fantasma al desactivar el manager.
    /// </summary>
    private void OnDisable()
    {
        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }

        _ghostVisible = false;
        Cursor.visible = true;
    }

    /// <summary>
    /// Muestra el topping fantasma.
    /// </summary>
    private void SetGhostVisible(bool visible)
    {
        _ghostVisible = visible;

        if (_ghostInstance != null && _ghostInstance.activeSelf != visible)
            _ghostInstance.SetActive(visible);

        Cursor.visible = true;
    }

}

[System.Serializable]
public class ToppingOption
{
    public string name;
    public GameObject prefab;
}
