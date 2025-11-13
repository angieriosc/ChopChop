using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el spawn de toppings sobre la pizza.
/// Validación por radio y selección desde UI.
/// </summary>
public class PizzaToppingManager : MonoBehaviour
{
    [Header("Cámara de trabajo asignada por ToppingStation")]
    [SerializeField] private Camera _workCamera;

    [Header("Referencias de pizza")]
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
    private GameObject _ghostInstance;

    [SerializeField] private Material _ghostMaterial;
    [SerializeField] private bool _hideSystemCursorWithGhost = false;

    [SerializeField] private bool _autoToggleSystemCursor = true;
    private bool _ghostVisible = false;

    private int _numPeppersPlaced = 0;
    private int _numTomatoesPlaced = 0;


    /// <summary>Activa o desactiva el manager (se usa al entrar/salir de la estación).</summary>
    public void SetEnabled(bool value) => _enabled = value;

    /// <summary>Selecciona un topping desde UI.</summary>
    public void SelectToppingByIndex(int index)
    {
        if (index < 0 || index >= _toppings.Count) return;

        _currentToppingPrefab = _toppings[index].prefab;

        if (_ghostInstance != null)
            Destroy(_ghostInstance);

        _ghostInstance = Instantiate(_currentToppingPrefab);
        _ghostInstance.transform.localScale = Vector3.Scale(_ghostInstance.transform.localScale, _extraScale);
        _ghostInstance.layer = LayerMask.NameToLayer("Ignore Raycast"); // no bloqueará el raycast a la pizza

        if (_ghostMaterial != null)
        {
            foreach (var r in _ghostInstance.GetComponentsInChildren<Renderer>())
                r.material = _ghostMaterial;
        }

        if (_hideSystemCursorWithGhost) Cursor.visible = false;
        SetGhostVisible(false);
    }


    /// <summary>Limpia la selección actual de topping.</summary>
    public void ClearSelection()
    {
        _currentToppingPrefab = null;

        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }

        Cursor.visible = true;
        _ghostVisible = false;
    }

    /// <summary>
    /// Detecta el click del mouse y, si el manager está activo, intenta colocar un topping
    /// en la superficie de la pizza.
    /// </summary>
    private void Update()
    {
        if (!_enabled || WorkCamera == null || _currentToppingPrefab == null)
            return;

        Ray ray = WorkCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (PizzaSurfaceCollider.Raycast(ray, out var hit, 100f))
        {
            Vector3 target = hit.point + hit.normal * _surfaceYOffset;

            SetGhostVisible(true);

            if (_ghostInstance != null)
            {
                _ghostInstance.transform.position = target;
                _ghostInstance.transform.rotation = Quaternion.LookRotation(Vector3.forward, hit.normal);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
                SpawnTopping(target, hit.normal, _currentToppingPrefab);
        }
        else
        {
            SetGhostVisible(false);
        }
    }


     // Código generado con ayuda de ChatGPT (OpenAI), adaptado para el proyecto ChopChop!
    /// <summary>
    /// Realiza un raycast desde la cámara de trabajo hasta la pizza.
    /// Si golpea la superficie, calcula la posición objetivo y coloca un topping.
    /// </summary>
    private void TryPlaceTopping()
    {
        Ray ray = WorkCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!PizzaSurfaceCollider.Raycast(ray, out var hit, 100f)) return;

        Vector3 target = hit.point + hit.normal * _surfaceYOffset;
        SpawnTopping(target, hit.normal, _currentToppingPrefab);
    }

    /// <summary>
    /// Instancia un topping sobre la pizza en la posición indicada, orientándolo
    /// con la normal de la superficie y aplicando una escala adicional.
    /// </summary>
    /// <param name="position">Posición final del topping sobre la pizza.</param>
    /// <param name="normal">Normal de la superficie para orientar el topping correctamente.</param>
    /// <param name="prefab">Prefab del topping que se va a instanciar.</param>
    private void SpawnTopping(Vector3 position, Vector3 normal, GameObject prefab)
    {
        if (prefab.name == "pimiento") _numPeppersPlaced++;

        if (prefab.name == "tomatoSlice") _numTomatoesPlaced++;

        Quaternion rot = Quaternion.LookRotation(Vector3.forward, normal);
        GameObject go = Instantiate(prefab, position, rot, PizzaRoot);
        go.transform.localScale = Vector3.Scale(go.transform.localScale, _extraScale);

        // Ocultar instrucción después del primer topping
        ToppingStation station = FindObjectOfType<ToppingStation>();
        if (station != null)
            station.HideInstruction();

    }

    private void OnDisable()
    {
        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }
        Cursor.visible = true;
        _ghostVisible = false;
    }
    private void SetGhostVisible(bool visible)
    {
        _ghostVisible = visible;

        if (_ghostInstance != null && _ghostInstance.activeSelf != visible)
            _ghostInstance.SetActive(visible);

        if (_autoToggleSystemCursor)
            Cursor.visible = !visible; // si el ghost está visible, oculto el cursor; si no, lo muestro
    }



}

/// <summary>
/// Opción de topping para la UI.
/// </summary>
[System.Serializable]
public class ToppingOption
{
    public string name;
    public GameObject prefab;
}
