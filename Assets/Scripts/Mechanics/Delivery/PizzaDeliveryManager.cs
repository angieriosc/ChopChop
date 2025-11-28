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

    [Header("Mesas a validar en la primer entrega")]
    [SerializeField] private int[] _firstDeliverySlots = new int[] { 0, 1 };

    [Header("Rondas de entrega")]
    [SerializeField] private List<DeliveryRoundConfig> _rounds = new List<DeliveryRoundConfig>();

    private int _currentRoundIndex = 0;
    public int CurrentRoundIndex => _currentRoundIndex;

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

    /// <summary>
    /// Almacena una rebanada física en el inventario interno.  
    /// </summary>
    /// <param name="inventoryKey">Key que identifica la rebanada (debe coincidir con CuttingInventory)</param>
    /// <param name="sliceObject">Objeto físico de la rebanada (GameObject)</param>               
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

    /// <summary>
    /// Habilita o deshabilita el manager al entrar o salir de la estación.
    /// </summary>
    /// <param name="value"></param>
    public void SetEnabled(bool value)
    {
        _enabled = value;
        Debug.Log($"[DEBUG] PizzaDeliveryManager Habilitado: {value}");
    }

    /// <summary>
    /// Selecciona la rebanada a entregar según el botón presionado en la UI
    /// </summary>
    /// <param name="sliceIndex"></param>
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

    /// <summary>
    /// Devuelve la cantidad de rebanadas entregadas en el slot indicado.
    /// </summary>
    /// <param name="slotIndex"></param>
    public int GetSliceCountForSlot(int slotIndex)
    {
        return _slicesPerSlot.TryGetValue(slotIndex, out var list) ? list.Count : 0;
    }

    /// <summary>
    /// ENTREGA (Llamado desde Update cuando se clickea)
    /// </summary>
    private void Update()
    {
        if (!_enabled) return;
        if (_workCamera == null) return;
        if (Mouse.current == null) return;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Debug.Log("--------------------------------------------------");
        Debug.Log("[DEBUG] Clic detectado. Iniciando proceso de entrega...");

        if (_currentSliceIndex < 0 || _currentSliceIndex >= _slicesPizza.Count)
        {
            Debug.LogWarning("[DEBUG] Cancelado: No hay rebanada seleccionada en la UI (Index es -1).");
            return;
        }

        string invKey = GetInventoryKeyForIndex(_currentSliceIndex);
        Debug.Log($"[DEBUG] Intentando entregar Key: '{invKey}'");

        if (string.IsNullOrEmpty(invKey)) 
        {
            Debug.LogError("[DEBUG ERROR] La Key es nula o vacía. Revisa el inspector de PizzaDeliveryManager.");
            return;
        }

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

    /// <summary>
    /// Coloca la rebanada física en el área de entrega especificada.
    /// </summary>
    private void PlaceSliceOnArea(DeliveryArea area, Vector3 position, GameObject sliceObj)
    {
        if (sliceObj == null)
        {
            Debug.LogError("[DEBUG ERROR] El objeto de rebanada es NULL. Revisa StoreRealSlice / Dequeue.");
            return;
        }

        int slotIndex = area.slotIndex;

        sliceObj.SetActive(true);
        sliceObj.transform.position = position;
        sliceObj.transform.rotation = Quaternion.LookRotation(Vector3.forward, area.transform.up);
        sliceObj.transform.SetParent(area.transform, true);

        if (!_slicesPerSlot.TryGetValue(slotIndex, out var list))
        {
            list = new List<GameObject>();
            _slicesPerSlot[slotIndex] = list;
        }
        list.Add(sliceObj);

        string invKey = GetInventoryKeyForIndex(_currentSliceIndex);

        var meta = sliceObj.GetComponent<DeliveredSlice>();
        if (meta == null) meta = sliceObj.AddComponent<DeliveredSlice>();

        meta.recipeKey = invKey;
        meta.seatSlot  = slotIndex;

        if (_deliverAudio != null)
            _deliverAudio.Play();

        Debug.Log($"[PizzaDeliveryManager] Rebanada colocada en slot {slotIndex}. Total = {list.Count}");
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
    /// <summary>
    /// Devuelve true si cada mesa/slot configurado en _firstDeliverySlots
    /// tiene exactamente UNA rebanada. Si alguna tiene 0 o más de 1, devuelve false.
    /// </summary>
    public bool IsFirstDeliveryValid()
    {
        foreach (int slot in _firstDeliverySlots)
        {
            int count = GetSliceCountForSlot(slot); // ya lo tienes implementado

            if (count != 1)
            {
                Debug.Log($"[Entrega] Mesa/slot {slot} tiene {count} rebanadas. Debe tener exactamente 1.");
                return false;
            }
        }

        Debug.Log("[Entrega] ✔ Todas las mesas tienen exactamente 1 rebanada.");
        return true;
    }
    /// <summary>
    /// Verifica si la entrega de la ronda actual es correcta:
    /// - Cada asiento de seatSlots tiene EXACTAMENTE slicesPerClient rebanadas.
    /// - Todas esas rebanadas son de la recipeKey correcta.
    /// - (Opcional) No hay rebanadas en asientos que no participan.
    /// </summary>
    public bool ValidateCurrentRound(out string errorMessage)
    {
        errorMessage = "";

        if (_currentRoundIndex < 0 || _currentRoundIndex >= _rounds.Count)
        {
            errorMessage = "No hay ronda configurada.";
            return false;
        }

        DeliveryRoundConfig cfg = _rounds[_currentRoundIndex];

        // 1) Revisar cada asiento de la ronda
        foreach (int seat in cfg.seatSlots)
        {
            if (!_slicesPerSlot.TryGetValue(seat, out var list) || list == null)
            {
                errorMessage = $"El asiento {seat} no tiene rebanadas.";
                return false;
            }

            // Quitar nulls si alguna rebanada fue destruida
            list.RemoveAll(s => s == null);

            if (list.Count != cfg.slicesPerClient)
            {
                errorMessage = $"El asiento {seat} tiene {list.Count} rebanadas, " +
                            $"pero se esperaba {cfg.slicesPerClient}.";
                return false;
            }

            // 2) Verificar receta de cada rebanada
            foreach (var slice in list)
            {
                var meta = slice.GetComponent<DeliveredSlice>();
                if (meta == null || meta.recipeKey != cfg.recipeKey)
                {
                    errorMessage = $"En el asiento {seat} hay una rebanada de receta incorrecta.";
                    return false;
                }
            }
        }

        // 3) (Opcional) Revisar que no haya rebanadas en asientos extra
        foreach (var kvp in _slicesPerSlot)
        {
            int seat = kvp.Key;
            var list = kvp.Value;

            if (!cfg.seatSlots.Contains(seat) && list != null && list.Count > 0)
            {
                errorMessage = $"Hay rebanadas en un asiento que no participa en esta ronda (seat {seat}).";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Llamar cuando una entrega fue válida para pasar a la siguiente ronda.
    /// </summary>
    public void AdvanceRound()
    {
        _currentRoundIndex = Mathf.Min(_currentRoundIndex + 1, _rounds.Count - 1);
        
        // Limpiar ronda anterior
        ResetSlots();
        RefreshDeliveryAreasForCurrentRound();
        Debug.Log($"[Entrega] Avanzando a la ronda {_currentRoundIndex}.");
    }

    /// <summary>
    /// Limpia todas las rebanadas de las mesas y del diccionario
    /// </summary>
    public void ResetSlots()
    {
        foreach (var kvp in _slicesPerSlot)
        {
            var list = kvp.Value;
            if (list == null) continue;

            foreach (var go in list)
            {
                if (go != null)
                    Destroy(go);
            }
        }

        _slicesPerSlot.Clear();
    }

    /// <summary>
    /// Activa solo los DeliveryArea cuyos slotIndex estén en la ronda actual.
    /// </summary>
    public void RefreshDeliveryAreasForCurrentRound()
    {
        if (_currentRoundIndex < 0 || _currentRoundIndex >= _rounds.Count)
            return;

        DeliveryRoundConfig cfg = _rounds[_currentRoundIndex];
        var allowed = new HashSet<int>(cfg.seatSlots);

        DeliveryArea[] areas = FindObjectsOfType<DeliveryArea>();

        foreach (var area in areas)
        {
            Collider col = area.GetComponent<Collider>();
            if (col == null) continue;

            bool enable = allowed.Contains(area.slotIndex);
            col.enabled = enable;
        }

        Debug.Log($"[Entrega] Mesas activas para ronda {_currentRoundIndex}: {string.Join(", ", cfg.seatSlots)}");
    }

    /// <summary>
    /// Reinicia completamente el manager para una nueva receta/cliente.
    /// Limpia mesas, resetea la ronda a 0 y prepara todo.
    /// </summary>
    public void ResetForNewRecipe()
    {
        // 1. Limpiar objetos físicos de las mesas
        ResetSlots();

        // 2. Reiniciar índice de ronda
        _currentRoundIndex = 0;

        // 3. (Opcional) Limpiar inventario de rebanadas en mano si es necesario
        // _realSliceStorage.Clear(); // Descomenta si quieres que se pierdan las rebanadas guardadas

        // 4. Actualizar colliders de las mesas
        RefreshDeliveryAreasForCurrentRound();

        Debug.Log("[PizzaDeliveryManager] Sistema reiniciado para nuevo cliente.");
    }


}

[System.Serializable]
public class PizzaOption
{
    public string id;
    public string name;
}

[System.Serializable]
public class DeliveryRoundConfig
{
    [Tooltip("Key de receta esperada en esta ronda (CuttingInventory)")]
    public string recipeKey;

    [Tooltip("Slots/asientos que deben recibir rebanada en esta ronda")]
    public List<int> seatSlots = new List<int>();

    [Tooltip("Rebanadas requeridas por cliente (en tu caso siempre 1)")]
    public int slicesPerClient = 1;
}