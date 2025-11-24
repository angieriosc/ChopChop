using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PizzaDeliveryManager : MonoBehaviour
{
    [Header("Cámara usada en la estación de entrega")]
    [SerializeField] private Camera _workCamera;

    [Header("Rebanadas disponibles (ligadas al inventario)")]
    [SerializeField] private List<PizzaOption> _slicesPizza = new();

    [Header("Altura sobre la mesa")]
    [SerializeField] private float _surfaceYOffset = 0.01f;

    [Header("Audio")]
    [SerializeField] private AudioSource _deliverAudio;

    [Header("Lógica de pizzas (keys en CuttingInventory)")]
    [SerializeField] private string _PizzaInventoryKey1 = "Recipe_CheeseSimple";
    [SerializeField] private string _PizzaInventoryKey2 = "Recipe_Classic";
    [SerializeField] private string _PizzaInventoryKey3 = "Recipe_Vegetal";

    public string PizzaInventoryKey1 => _PizzaInventoryKey1;
    public string PizzaInventoryKey2 => _PizzaInventoryKey2;
    public string PizzaInventoryKey3 => _PizzaInventoryKey3;

    // --- Estado interno ---
    private bool _enabled = false;
    private int _currentSliceIndex = -1; // índice de la rebanada seleccionada

    public event Action OnInventoryChanged;

    // slotIndex -> lista de instancias de rebanadas colocadas
    private readonly Dictionary<int, List<GameObject>> _slicesPerSlot =
        new Dictionary<int, List<GameObject>>();

    public Camera WorkCamera
    {
        get => _workCamera;
        set => _workCamera = value;
    }

    /// <summary>
    /// Activar / desactivar el sistema (lo llama DeliveryStation al entrar/salir).
    /// </summary>
    public void SetEnabled(bool value)
    {
        _enabled = value;
    }

    /// <summary>
    /// Selecciona la rebanada según el índice que viene del inventario (0,1,2...).
    /// Llamar esto desde los botones 1,2,3 del inventario.
    /// </summary>
    public void SelectSlice(int sliceIndex)
    {
        if (sliceIndex < 0 || sliceIndex >= _slicesPizza.Count)
        {
            Debug.LogWarning($"[PizzaDeliveryManager] Índice de slice inválido: {sliceIndex}");
            _currentSliceIndex = -1;
            return;
        }

        _currentSliceIndex = sliceIndex;
        Debug.Log($"[PizzaDeliveryManager] Slice seleccionado: {_slicesPizza[sliceIndex].name}");
    }

    /// <summary>
    /// Devuelve cuántas rebanadas hay colocadas en un punto de entrega.
    /// </summary>
    public int GetSliceCountForSlot(int slotIndex)
    {
        return _slicesPerSlot.TryGetValue(slotIndex, out var list) ? list.Count : 0;
    }

    // ----------- LOOP PRINCIPAL -----------

    private void Update()
    {
        if (!_enabled || _workCamera == null)
            return;

        if (Mouse.current == null)
            return;

        // Esperamos al click izquierdo del mouse
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Debe haber una rebanada seleccionada
        if (_currentSliceIndex < 0 || _currentSliceIndex >= _slicesPizza.Count)
        {
            Debug.Log("[PizzaDeliveryManager] No hay slice seleccionada en el inventario.");
            return;
        }

        // 1) Revisar inventario en CuttingInventory
        string invKey = GetInventoryKeyForIndex(_currentSliceIndex);

        if (CuttingInventory.Instance != null && !string.IsNullOrEmpty(invKey))
        {
            int qty = CuttingInventory.Instance.GetQuantity(invKey);
            if (qty <= 0)
            {
                Debug.Log($"[PizzaDeliveryManager] No hay más rebanadas disponibles para '{invKey}'.");
                return;
            }
        }

        // 2) Raycast sobre la mesa
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _workCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // ¿Golpeamos una zona de entrega?
            DeliveryArea area = hit.collider.GetComponent<DeliveryArea>();
            if (area == null)
            {
                // clic en la mesa pero no en una zona válida
                return;
            }

            Vector3 spawnPos = hit.point + hit.normal * _surfaceYOffset;

            // 3) Colocar rebanada físicamente
            PlaceSliceOnArea(area, spawnPos);

            // 4) Consumir del inventario (CuttingInventory)
            if (CuttingInventory.Instance != null && !string.IsNullOrEmpty(invKey))
            {
                CuttingInventory.Instance.Consume(invKey, 1);
                OnInventoryChanged?.Invoke(); // avisar al HUD que cambió el inventario
            }
        }
    }

    // ----------- LÓGICA INTERNA -----------

    private void PlaceSliceOnArea(DeliveryArea area, Vector3 position)
    {
        PizzaOption option = _slicesPizza[_currentSliceIndex];
        if (option.prefab == null)
        {
            Debug.LogWarning($"[PizzaDeliveryManager] Prefab vacío para slice '{option.name}'.");
            return;
        }

        // Rotamos la rebanada respetando la normal de la mesa
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, area.transform.up);

        GameObject instance = Instantiate(option.prefab, position, rot);
        instance.transform.SetParent(area.transform, true);

        int slotIndex = area.slotIndex;

        if (!_slicesPerSlot.TryGetValue(slotIndex, out var list))
        {
            list = new List<GameObject>();
            _slicesPerSlot[slotIndex] = list;
        }
        list.Add(instance);

        if (_deliverAudio != null)
            _deliverAudio.Play();

        Debug.Log($"[PizzaDeliveryManager] Colocada '{option.name}' en slot {slotIndex}, total = {list.Count}");
    }

    // ----------- API PARA EL HUD -----------

    /// <summary>
    /// Devuelve la cantidad disponible de una rebanada según su índice,
    /// leyendo directamente de CuttingInventory.
    /// </summary>
    public int GetInventoryQuantity(int index)
    {
        string key = GetInventoryKeyForIndex(index);

        if (CuttingInventory.Instance == null || string.IsNullOrEmpty(key))
            return 0;

        return CuttingInventory.Instance.GetQuantity(key);
    }

    /// <summary>
    /// Consume 1 unidad de la rebanada en CuttingInventory y dispara el evento.
    /// Puedes llamarlo si prefieres explícitamente desde fuera.
    /// </summary>
    public void ReduceInventory(int index)
    {
        string key = GetInventoryKeyForIndex(index);

        if (CuttingInventory.Instance == null || string.IsNullOrEmpty(key))
            return;

        CuttingInventory.Instance.Consume(key, 1);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Mapea el índice de rebanada (0,1,2,...) a la key correspondiente en CuttingInventory.
    /// </summary>
    private string GetInventoryKeyForIndex(int index)
    {
        switch (index)
        {
            case 0: return _PizzaInventoryKey1;
            case 1: return _PizzaInventoryKey2;
            case 2: return _PizzaInventoryKey3;
            default:
                Debug.LogWarning($"[PizzaDeliveryManager] No hay InventoryKey configurada para index {index}.");
                return null;
        }
    }
}

[System.Serializable]
public class PizzaOption
{
    public string id;
    public string name;
    public GameObject prefab;
}
