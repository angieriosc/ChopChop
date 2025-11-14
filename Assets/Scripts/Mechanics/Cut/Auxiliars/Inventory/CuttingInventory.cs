using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona una lista simple de items de inventario.
/// Implementa la lógica de apilamiento (stacking).
/// </summary>
public class CuttingInventory : MonoBehaviour
{
    public static CuttingInventory Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    [Header("Inventario")]
    [Tooltip("Lista actual de items en el inventario.")]
    public List<CuttingInventoryItem> items = new List<CuttingInventoryItem>();

    /// <summary>
    /// Añade rebanadas al inventario, apilándolas si ya existen.
    /// </summary>
    /// <param name="key">La clave del item original (ej. "Mix").</param>
    /// <param name="slicePrefab">El prefab de la rebanada resultante.</param>
    /// <param name="amount">La cantidad de rebanadas a añadir.</param>
    public void AddSlices(string key, GameObject slicePrefab, int amount)
    {
        foreach (CuttingInventoryItem item in items)
        {
            if (item.itemKey == key)
            {
                item.quantity += amount;
                Debug.Log($"[CuttingInventory] Apilado: +{amount} de '{key}'. Total: {item.quantity}");
                return;
            }
        }

        CuttingInventoryItem newItem = new CuttingInventoryItem(key, slicePrefab, amount);
        items.Add(newItem);
        Debug.Log($"[CuttingInventory] Añadido Nuevo: {amount} de '{key}'.");
    }

    /// <summary>
    /// Obtiene la cantidad de un item por su clave.
    /// </summary>
    public int GetQuantity(string key)
    {
        foreach (CuttingInventoryItem item in items)
        {
            if (item.itemKey == key)
            {
                return item.quantity;
            }
        }
        return 0;
    }
}