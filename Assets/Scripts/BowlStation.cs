using UnityEngine;

/// <summary>
/// Controla la estación de bowls, gestionando su aparición, límite máximo y
/// referencia al bowl activo.
/// </summary>
public class BowlStation : MonoBehaviour
{
    [Header("Configuración del Spawn")]
    [Tooltip("Prefab del bowl que se instanciará.")]
    [SerializeField] private GameObject bowlPrefab;

    [Tooltip("Punto de aparición del bowl en la escena.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Tiempo mínimo entre spawns consecutivos.")]
    [SerializeField] private float spawnCooldown = 2f;

    [Tooltip("Número máximo de bowls permitidos en escena.")]
    [SerializeField] private int maxBowls = 3;

    private float lastSpawnTime;
    private int currentBowls;
    public GameObject activeBowl;

    private void Start()
    {
        if (!ValidateSetup()) return;
        SpawnNewBowl();
    }

    /// <summary>
    /// Intenta crear un nuevo bowl si no se ha alcanzado el límite.
    /// </summary>
    public void TrySpawnBowl()
    {
        if (!CanSpawn()) return;
        SpawnNewBowl();
    }

    /// <summary>
    /// Disminuye el contador cuando un bowl es tomado.
    /// </summary>
    public void OnBowlTaken()
    {
        currentBowls = Mathf.Max(0, currentBowls - 1);
        activeBowl = null;
    }

    /// <summary>
    /// Verifica si el prefab y el punto de spawn están configurados.
    /// </summary>
    private bool ValidateSetup()
    {
        if (bowlPrefab == null || spawnPoint == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Comprueba si se puede generar un nuevo bowl.
    /// </summary>
    private bool CanSpawn()
    {
        if (currentBowls >= maxBowls)
        {
            return false;
        }

        if (activeBowl != null)
        {
            return false;
        }

        if (!ValidateSetup()) return false;

        if (Time.time - lastSpawnTime < spawnCooldown)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Crea una nueva instancia del bowl y la configura.
    /// </summary>
    private void SpawnNewBowl()
    {
        GameObject newBowl = Instantiate(
            bowlPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        var bowlContainer = newBowl.GetComponent<ReceivingContainer>();
        if (bowlContainer != null)
        {
            bowlContainer.cupTracker = FindFirstObjectByType<CupTracker>();
        }
        else
        {
            Debug.LogWarning(" El bowl no tiene componente ReceivingContainer.");
        }

        newBowl.name = $"Bowl_{currentBowls}";
        currentBowls++;
        lastSpawnTime = Time.time;
        activeBowl = newBowl;
    }
}
