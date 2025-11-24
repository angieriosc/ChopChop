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

    // --- Estado interno ---
    private bool _enabled = false;
    private int _currentSliceIndex = -1; // índice de la rebanada seleccionada
    public event Action OnInventoryChanged;

    [SerializeField] private int[] sliceInventory = new int[3];

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
    /// Devuelve cuántas rebanadas hay en un punto de entrega.
    /// </summary>
    public int GetSliceCountForSlot(int slotIndex)
    {
        return _slicesPerSlot.TryGetValue(slotIndex, out var list) ? list.Count : 0;
    }

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
            PlaceSliceOnArea(area, spawnPos);
        }
    }

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
    public int GetInventoryQuantity(int index)
    {
        if (index < 0 || index >= sliceInventory.Length)
            return 0;

        return sliceInventory[index];
    }

    public void ReduceInventory(int index)
    {
        if (index < 0 || index >= sliceInventory.Length)
            return;

        sliceInventory[index] = Mathf.Max(0, sliceInventory[index] - 1);

        OnInventoryChanged?.Invoke();
    }
}

[System.Serializable]
public class PizzaOption
{
    public string id;
    public string name;
    public GameObject prefab;
}
