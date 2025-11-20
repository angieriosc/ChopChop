using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PizzaToppingManager : MonoBehaviour
{
    [Header("Cámara de trabajo asignada por ToppingStation")]
    [SerializeField] private Camera _workCamera;

    [Header("Referencias de pizza / superficie")]
    [SerializeField] private Transform _pizzaRoot;
    [SerializeField] private Collider _pizzaSurfaceCollider;
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

    private Dictionary<string, int> _maxPerTopping = new();
    private Dictionary<string, int> _placedPerTopping = new();
    public System.Action OnToppingCountsChanged;

    [Header("Audio")]
    [SerializeField] private AudioSource toppingAudio;

    [Header("Lógica de masa/base")]
    [SerializeField] private int _baseDoughToppingIndex = 0;
    [SerializeField] private string _baseDoughInventoryKey = "WedgeSlice";
    public string BaseDoughInventoryKey => _baseDoughInventoryKey;


    private bool _baseDoughPlaced = false;
    private int _currentToppingIndex = -1;

    /// <summary>
    /// Activa o desactiva el manager al entrar o salir de la estación.
    /// </summary>
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

    /// <summary>
    /// Selecciona un topping según su índice en la UI.
    /// </summary>
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

    /// <summary>
    /// Limpia la selección actual de topping.
    /// </summary>
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

    /// <summary>
    /// Actualiza el raycast y el cursor fantasma para colocar toppings.
    /// </summary>
    private void Update()
    {
        if (!_enabled || WorkCamera == null || _currentToppingPrefab == null)
            return;

        bool isBaseDoughSelected = (_currentToppingIndex == _baseDoughToppingIndex);

        if (!_baseDoughPlaced)
        {
            if (!isBaseDoughSelected)
                return;

            if (CuttingInventory.Instance != null)
            {
                int qty = CuttingInventory.Instance.GetQuantity(_baseDoughInventoryKey);
                if (qty <= 0)
                    return;
            }
            else
                return;
        }
        else
        {
            if (isBaseDoughSelected)
                return;
        }

        Collider surfaceCollider = _baseDoughPlaced && _pizzaRoot != null
            ? _pizzaRoot.GetComponentInChildren<Collider>()
            : PizzaSurfaceCollider;

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
    /// Instancia la masa o un topping normal según reglas actuales.
    /// </summary>
    private void SpawnTopping(Vector3 position, Vector3 normal, GameObject prefab)
    {
        if (prefab == null) return;

        if (_currentToppingIndex < 0 || _currentToppingIndex >= _toppings.Count)
            return;

        string toppingId = _toppings[_currentToppingIndex].id;

        if (!_baseDoughPlaced && _currentToppingIndex == _baseDoughToppingIndex)
        {
            Quaternion rot = Quaternion.LookRotation(Vector3.forward, normal);

            GameObject dough = Instantiate(prefab, position, rot);
            dough.transform.localScale = Vector3.Scale(dough.transform.localScale, _extraScale);

            _pizzaRoot = dough.transform;
            _pizzaCenter = _pizzaRoot;
            _baseDoughPlaced = true;

            var col = dough.GetComponentInChildren<Collider>();
            if (col != null)
                PizzaSurfaceCollider = col;

            if (CuttingInventory.Instance != null)
                CuttingInventory.Instance.Consume(_baseDoughInventoryKey, 1);

            ToppingStation station = FindObjectOfType<ToppingStation>();
            if (station != null)
                station.HideInstruction();

            OnToppingCountsChanged?.Invoke();
            return;
        }

        if (_pizzaRoot == null)
            return;

        if (!CanPlaceTopping(toppingId))
            return;

        if (toppingAudio != null)
            toppingAudio.Play();

        Quaternion toppingRot = Quaternion.LookRotation(Vector3.forward, normal);
        GameObject go = Instantiate(prefab, position, toppingRot, _pizzaRoot);
        go.transform.localScale = Vector3.Scale(go.transform.localScale, _extraScale);

        _placedPerTopping[toppingId] = GetPlacedForTopping(toppingId) + 1;

        OnToppingCountsChanged?.Invoke();
    }

    /// <summary>
    /// Limpia el ghost cursor al desactivar el manager.
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
    /// Activa o desactiva el ghost del topping.
    /// </summary>
    private void SetGhostVisible(bool visible)
    {
        _ghostVisible = visible;

        if (_ghostInstance != null && _ghostInstance.activeSelf != visible)
            _ghostInstance.SetActive(visible);

        Cursor.visible = true;
    }

    /// <summary>
    /// Aplica los límites de toppings definidos por la receta actual.
    /// </summary>
    public void ApplyRecipeLimits(RecipeDataMenu recipe)
    {
        _maxPerTopping.Clear();
        _placedPerTopping.Clear();

        if (recipe == null)
            return;

        foreach (var limit in recipe.toppingLimits)
        {
            _maxPerTopping[limit.toppingId] = limit.maxQuantity;
            _placedPerTopping[limit.toppingId] = 0;
        }

        OnToppingCountsChanged?.Invoke();
    }

    /// <summary>Obtiene el máximo permitido para un topping.</summary>
    public int GetMaxForTopping(string toppingId)
    {
        return _maxPerTopping.TryGetValue(toppingId, out int max) ? max : 0;
    }

    /// <summary>Obtiene cuántos toppings de este tipo ya fueron colocados.</summary>
    public int GetPlacedForTopping(string toppingId)
    {
        return _placedPerTopping.TryGetValue(toppingId, out int count) ? count : 0;
    }

    /// <summary>Verifica si aún se puede colocar este topping según la receta.</summary>
    public bool CanPlaceTopping(string toppingId)
    {
        if (_maxPerTopping.TryGetValue(toppingId, out int max))
        {
            int current = GetPlacedForTopping(toppingId);
            return current < max;
        }

        return false;
    }
}

[System.Serializable]
public class ToppingOption
{
    public string id;
    public string name;
    public GameObject prefab;
}