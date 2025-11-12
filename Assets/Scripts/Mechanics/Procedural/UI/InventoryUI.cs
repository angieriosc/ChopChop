using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz de inventario para seleccionar ingredientes y generar
/// el objeto seleccionado en la estación de corte.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Ingredient Prefabs")]
    [SerializeField]
    [Tooltip("Lista de prefabs de ingredientes disponibles para seleccionar.")]
    private List<GameObject> ingredientPrefabs;

    [Header("UI Buttons")]
    [SerializeField]
    [Tooltip("Botones de UI correspondientes a cada ingrediente.")]
    private List<Button> ingredientButtons;

    [Header("Scene References")]
    [SerializeField]
    [Tooltip("Punto en la escena donde se generará el ingrediente seleccionado.")]
    private Transform spawnPoint;

    [SerializeField]
    [Tooltip("Referencia a la estación de corte donde se colocará el ingrediente.")]
    private SlicingStation slicingStation;

    // --- Private ---

    /// <summary>
    /// Ingrediente actualmente generado en la escena.
    /// </summary>
    private GameObject currentSpawnedIngredient;

    /// <summary>
    /// Índice del ingrediente actualmente seleccionado.
    /// </summary>
    private int currentSelectedIndex = -1;

    /// <summary>
    /// Inicializa la UI y asigna los listeners a los botones.
    /// </summary>
    private void Start()
    {
        if (ingredientPrefabs.Count != ingredientButtons.Count)
        {
            Debug.LogError("The number of prefabs and buttons does not match!");
            return;
        }

        // Asignar el evento de selección a cada botón
        for (int i = 0; i < ingredientButtons.Count; i++)
        {
            int index = i; // Captura el índice local para el lambda
            ingredientButtons[i].onClick.AddListener(() => OnIngredientSelected(index));
        }
    }

    /// <summary>
    /// Llamado cuando un ingrediente es seleccionado desde la UI.
    /// Genera el prefab correspondiente y lo asigna a la estación de corte.
    /// </summary>
    /// <param name="index">Índice del ingrediente seleccionado.</param>
    private void OnIngredientSelected(int index)
    {
        if (index == currentSelectedIndex)
        {
            return;
        }

        if (currentSpawnedIngredient != null)
        {
            Destroy(currentSpawnedIngredient);
        }

        GameObject prefabToSpawn = ingredientPrefabs[index];
        currentSpawnedIngredient = Instantiate(
            prefabToSpawn, 
            spawnPoint.position, 
            spawnPoint.rotation
        );

        slicingStation.objectToCut = currentSpawnedIngredient;
        currentSelectedIndex = index;
    }
}
