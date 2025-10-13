using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el inventario de objetos del jugador.
/// </summary>
public class Inventory : MonoBehaviour
{
    // 2. Variables privadas
    private Dictionary<string, int> _items = new Dictionary<string, int>();

    // 4. Métodos públicos
    /// <summary>
    /// Agrega una cantidad de un objeto al inventario.
    /// </summary>
    /// <param name="itemName">Nombre del objeto.</param>
    /// <param name="amount">Cantidad a agregar (por defecto 1).</param>
    public void AddItem(string itemName, int amount = 1)
    {
        if (_items.ContainsKey(itemName))
            _items[itemName] += amount;
        else
            _items[itemName] = amount;

        Debug.Log($"Agregado {amount} {itemName}(s). Total: {_items[itemName]}");
    }

    /// <summary>
    /// Quita una cantidad de un objeto del inventario.
    /// </summary>
    /// <param name="itemName">Nombre del objeto.</param>
    /// <param name="amount">Cantidad a quitar (por defecto 1).</param>
    public void RemoveItem(string itemName, int amount = 1)
    {
        if (_items.ContainsKey(itemName))
        {
            _items[itemName] -= amount;
            if (_items[itemName] <= 0)
                _items.Remove(itemName);
        }
    }

    /// <summary>
    /// Retorna la cantidad de un objeto en el inventario.
    /// </summary>
    /// <param name="itemName">Nombre del objeto.</param>
    /// <returns>Cantidad disponible</returns>
    public int GetQuantity(string itemName)
    {
        if (_items.TryGetValue(itemName, out int quantity))
            return quantity;
        return 0;
    }

    /// <summary>
    /// Retorna todos los objetos del inventario.
    /// </summary>
    /// <returns>Copia del diccionario de objetos</returns>
    public Dictionary<string, int> GetAllItems()
    {
        return new Dictionary<string, int>(_items);
    }

    /// <summary>
    /// Limpia todo el inventario.
    /// </summary>
    public void ClearInventory()
    {
        _items.Clear();
    }
}
