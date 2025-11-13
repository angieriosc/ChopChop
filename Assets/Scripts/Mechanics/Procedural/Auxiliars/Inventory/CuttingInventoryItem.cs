using UnityEngine;

/// <summary>
/// Define los datos de un item apilable en el inventario simple.
/// </summary>
[System.Serializable]
public class CuttingInventoryItem
{
    [Tooltip("Clave única del item (ej. el nombre del prefab original 'Mix').")]
    public string itemKey;
    
    [Tooltip("El prefab que se debe usar/instanciar de este item (ej. 'DoughSlice').")]
    public GameObject sliceResultPrefab;
    
    [Tooltip("Cantidad de este item que posee el jugador.")]
    public int quantity;

    public CuttingInventoryItem(string key, GameObject prefab, int amount)
    {
        this.itemKey = key;
        this.sliceResultPrefab = prefab;
        this.quantity = amount;
    }
}