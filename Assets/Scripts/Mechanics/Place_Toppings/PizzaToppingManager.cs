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

    /// <summary>Activa o desactiva el manager (se usa al entrar/salir de la estación).</summary>
    public void SetEnabled(bool value) => _enabled = value;

    /// <summary>Selecciona un topping desde UI.</summary>
    public void SelectToppingByIndex(int index)
    {
        if (index < 0 || index >= _toppings.Count) return;
        _currentToppingPrefab = _toppings[index].prefab;
    }

    /// <summary>Limpia la selección actual de topping.</summary>
    public void ClearSelection() => _currentToppingPrefab = null;

    /// <summary>
    /// Detecta el click del mouse y, si el manager está activo, intenta colocar un topping
    /// en la superficie de la pizza.
    /// </summary>
    private void Update()
    {
        if (!_enabled || WorkCamera == null || _currentToppingPrefab == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryPlaceTopping();
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
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, normal);
        GameObject go = Instantiate(prefab, position, rot, PizzaRoot);
        go.transform.localScale = Vector3.Scale(go.transform.localScale, _extraScale);
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
