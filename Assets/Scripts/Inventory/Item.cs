using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "ChopChop/Item", fileName = "NewItem")]
public class Item : ScriptableObject
{
    [Header("Identidad")]
    public string itemName;                    // "Tomate", "Harina", etc.
    [TextArea] public string description;     // Tooltip/ayuda
    [SerializeField, HideInInspector] private string id; // Único por asset
    public ItemCategory category = ItemCategory.Ingredient;

    [Header("UI")]
    public Sprite icon;                        // Sprite para inventario/HUD
    public bool stackable = true;              // ¿Se apila?
    [Min(1)] public int maxStack = 99;         // Límite de apilado

    [Header("Spawn en mundo (opcional)")]
    [Tooltip("Prefab que se instancia al soltar desde inventario. Debe incluir InteractableObject con sus capacidades.")]
    public GameObject worldPrefab;
    public Vector3 spawnOffset;                // Ajuste fino al instanciar
    public Vector3 spawnRotationEuler;         // Rotación por defecto

    [Header("Economía (opcional)")]
    public int price;                          // Para tienda/compra-venta

    // Acceso de sólo lectura al id
    public string Id => id;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Asigna un GUID la primera vez para mantener unicidad
        if (string.IsNullOrWhiteSpace(id))
            id = System.Guid.NewGuid().ToString();
    }
#endif
}

public enum ItemCategory
{
    Ingredient,
    Topping,
    Tool,
    Dish,
    Other
}
