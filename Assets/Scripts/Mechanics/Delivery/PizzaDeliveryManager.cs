using System;
using System.Collections.Generic;
using System.Linq; // Necesario para imprimir las keys del diccionario
using UnityEngine;
using UnityEngine.InputSystem;

public class PizzaDeliveryManager : MonoBehaviour
{
    public static PizzaDeliveryManager Instance { get; private set; }

    [Header("Cámara usada en la estación de entrega")]
    [SerializeField] private Camera _workCamera;

    [Header("Rebanadas disponibles (Solo para UI y nombres)")]
    [SerializeField] private List<PizzaOption> _slicesPizza = new();

    [Header("Configuración Física")]
    [SerializeField] private float _surfaceYOffset = 0.01f;
    [SerializeField] private Transform _storageContainer; 
    [SerializeField] private AudioSource _deliverAudio;

    [Header("Mapeo de Keys (Deben coincidir con SlicingStation)")]
    [SerializeField] private string _PizzaInventoryKey1 = "Recipe_CheeseSimple";
    [SerializeField] private string _PizzaInventoryKey2 = "Recipe_Classic";
    [SerializeField] private string _PizzaInventoryKey3 = "Recipe_Vegetal";

    // --- Inventario de Objetos Reales ---
    private Dictionary<string, Queue<GameObject>> _realSliceStorage = new Dictionary<string, Queue<GameObject>>();

    private bool _enabled = false;
    private int _currentSliceIndex = -1; 

    public event Action OnInventoryChanged;

    private readonly Dictionary<int, List<GameObject>> _slicesPerSlot = new Dictionary<int, List<GameObject>>();

    public Camera WorkCamera { get => _workCamera; set => _workCamera = value; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (_storageContainer == null)
        {
            GameObject container = new GameObject("RealSliceStorage");
            container.transform.SetParent(this.transform);
            _storageContainer = container.transform;
        }
    }

    /// <summary>
    /// Debug: Muestra qué tiene el inventario ahora mismo.
    /// </summary>
    private void PrintDebugInventory()
    {
        string content = "";
        foreach (var kvp in _realSliceStorage)
        {
            content += $"Key: '{kvp.Key}' = {kvp.Value.Count} objetos. | ";
        }
        Debug.Log($"[DEBUG INVENTARIO] Estado actual: {content}");
    }

    // -----------------------------------------------------------------------------------
    // 1. GUARDADO (Llamado desde SlicingStation)
    // -----------------------------------------------------------------------------------
    public void StoreRealSlice(string inventoryKey, GameObject sliceObject)
    {
        Debug.Log($"[DEBUG] StoreRealSlice LLAMADO. Key recibida: '{inventoryKey}'. Objeto: {sliceObject.name}");

        sliceObject.transform.SetParent(_storageContainer);
        sliceObject.transform.localPosition = Vector3.zero;
        sliceObject.SetActive(false);

        if (!_realSliceStorage.ContainsKey(inventoryKey))
        {
            _realSliceStorage[inventoryKey] = new Queue<GameObject>();
            Debug.Log($"[DEBUG] Nueva Key creada en diccionario: '{inventoryKey}'");
        }

        _realSliceStorage[inventoryKey].Enqueue(sliceObject);
        
        Debug.Log($"[DEBUG] Objeto guardado exitosamente. Cantidad actual para '{inventoryKey}': {_realSliceStorage[inventoryKey].Count}");
        PrintDebugInventory();
    }

    public void SetEnabled(bool value)
    {
        _enabled = value;
        Debug.Log($"[DEBUG] PizzaDeliveryManager Habilitado: {value}");
    }

    // -----------------------------------------------------------------------------------
    // 2. SELECCIÓN (Llamado desde botones UI)
    // -----------------------------------------------------------------------------------
    public void SelectSlice(int sliceIndex)
    {
        Debug.Log($"[DEBUG] Botón presionado. Index: {sliceIndex}");

        if (sliceIndex < 0 || sliceIndex >= _slicesPizza.Count)
        {
            Debug.LogError($"[DEBUG ERROR] Index {sliceIndex} fuera de rango.");
            _currentSliceIndex = -1;
            return;
        }
        _currentSliceIndex = sliceIndex;
        string expectedKey = GetInventoryKeyForIndex(sliceIndex);
        Debug.Log($"[DEBUG] Slice seleccionado: Indice {sliceIndex} -> Espera Key: '{expectedKey}'");
    }

    public int GetSliceCountForSlot(int slotIndex)
    {
        return _slicesPerSlot.TryGetValue(slotIndex, out var list) ? list.Count : 0;
    }

    // -----------------------------------------------------------------------------------
    // 3. UPDATE (Clic en la mesa)
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        if (!_enabled) return;
        if (_workCamera == null) return;
        if (Mouse.current == null) return;

        // Solo debuggeamos si hace clic izquierdo
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Debug.Log("--------------------------------------------------");
        Debug.Log("[DEBUG] Clic detectado. Iniciando proceso de entrega...");

        // CHEQUEO 1: ¿Hay algo seleccionado?
        if (_currentSliceIndex < 0 || _currentSliceIndex >= _slicesPizza.Count)
        {
            Debug.LogWarning("[DEBUG] Cancelado: No hay rebanada seleccionada en la UI (Index es -1).");
            return;
        }

        // CHEQUEO 2: ¿Cuál es la Key?
        string invKey = GetInventoryKeyForIndex(_currentSliceIndex);
        Debug.Log($"[DEBUG] Intentando entregar Key: '{invKey}'");

        if (string.IsNullOrEmpty(invKey)) 
        {
            Debug.LogError("[DEBUG ERROR] La Key es nula o vacía. Revisa el inspector de PizzaDeliveryManager.");
            return;
        }

        // CHEQUEO 3: ¿Tenemos el objeto real?
        bool hasKey = _realSliceStorage.ContainsKey(invKey);
        int count = hasKey ? _realSliceStorage[invKey].Count : 0;

        Debug.Log($"[DEBUG] Verificando almacenamiento... ExisteKey: {hasKey}, Cantidad: {count}");

        if (!hasKey || count == 0)
        {
            Debug.LogError($"[DEBUG FALLO CRÍTICO] NO TENGO OBJETOS FÍSICOS DE '{invKey}'.");
            Debug.LogError($"[DEBUG INFO] Keys disponibles en mi caja ahora mismo: {string.Join(", ", _realSliceStorage.Keys)}");
            Debug.LogError("CONSEJO: Verifica que el nombre en SlicingStation sea IDÉNTICO letra por letra a la Key en PizzaDeliveryManager.");
            return; 
        }

        // CHEQUEO 4: Raycast
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _workCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log($"[DEBUG] Raycast golpeó: {hit.collider.name}");

            DeliveryArea area = hit.collider.GetComponent<DeliveryArea>();
            if (area != null)
            {
                Debug.Log("[DEBUG] ¡Es una DeliveryArea válida! Procediendo a colocar.");
                
                // Sacamos el objeto real
                GameObject realSlice = _realSliceStorage[invKey].Dequeue();

                PlaceSliceOnArea(area, hit.point + hit.normal * _surfaceYOffset, realSlice);

                // Consumo numérico (UI)
                if (CuttingInventory.Instance != null)
                {
                    CuttingInventory.Instance.Consume(invKey, 1);
                    OnInventoryChanged?.Invoke(); 
                }
            }
            else
            {
                Debug.LogWarning("[DEBUG] El objeto golpeado NO tiene el componente 'DeliveryArea'.");
            }
        }
        else
        {
            Debug.LogWarning("[DEBUG] El Raycast no golpeó nada (¿La cámara apunta bien?).");
        }
    }

    private void PlaceSliceOnArea(DeliveryArea area, Vector3 position, GameObject sliceObj)
    {
        if (sliceObj == null)
        {
            Debug.LogError("[DEBUG ERROR] El objeto recuperado de la cola es NULL. ¿Fue destruido?");
            return;
        }

        sliceObj.SetActive(true);
        sliceObj.transform.position = position;
        sliceObj.transform.rotation = Quaternion.LookRotation(Vector3.forward, area.transform.up);
        sliceObj.transform.SetParent(area.transform, true);

        int slotIndex = area.slotIndex;
        if (!_slicesPerSlot.TryGetValue(slotIndex, out var list))
        {
            list = new List<GameObject>();
            _slicesPerSlot[slotIndex] = list;
        }
        list.Add(sliceObj);

        if (_deliverAudio != null) _deliverAudio.Play();

        Debug.Log($"[DEBUG ÉXITO] Rebanada colocada en mesa. Restantes de este tipo: {_realSliceStorage[GetInventoryKeyForIndex(_currentSliceIndex)].Count}");
    }

    // API UI
    public int GetInventoryQuantity(int index)
    {
        string key = GetInventoryKeyForIndex(index);
        if (CuttingInventory.Instance == null || string.IsNullOrEmpty(key)) return 0;
        return CuttingInventory.Instance.GetQuantity(key);
    }

    private string GetInventoryKeyForIndex(int index)
    {
        switch (index)
        {
            case 0: return _PizzaInventoryKey1;
            case 1: return _PizzaInventoryKey2;
            case 2: return _PizzaInventoryKey3;
            default: return null;
        }
    }
}

[System.Serializable]
public class PizzaOption
{
    public string id;
    public string name;
}