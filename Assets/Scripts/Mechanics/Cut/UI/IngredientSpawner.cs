using UnityEngine;

/// <summary>
/// Controla la generación de un ingrediente en la escena y lo asigna
/// a la estación de corte para que pueda ser cortado.
/// </summary>
public class IngredientSpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField]
    [Tooltip("Prefab del ingrediente que se generará en la escena.")]
    private GameObject ingredientToSpawn;

    [SerializeField]
    [Tooltip("Punto de la escena donde se generará el ingrediente.")]
    private Transform spawnPoint;

    [Header("Station Reference")]
    [SerializeField]
    [Tooltip("Referencia a la estación de corte donde se colocará el ingrediente.")]
    private SlicingStation slicingStation;

    /// <summary>
    /// Inicializa el spawner y genera el ingrediente asignado en la escena.
    /// </summary>
    void Start()
    {
        // Validación de referencias
        if (ingredientToSpawn == null)
        {
            Debug.LogError("No ingredient prefab assigned to spawn!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("No spawn point assigned!");
            return;
        }

        if (slicingStation == null)
        {
            Debug.LogError("No SlicingStation assigned!");
            return;
        }

        // Instanciar el ingrediente en la posición y rotación del spawnPoint
        GameObject spawnedIngredient = Instantiate(
            ingredientToSpawn, 
            spawnPoint.position, 
            spawnPoint.rotation
        );

        // Asignar el objeto instanciado a la estación de corte
        slicingStation.objectToCut = spawnedIngredient;

        Debug.Log($"Spawned {spawnedIngredient.name} and assigned it to the Slicing Station.");
    }
}
