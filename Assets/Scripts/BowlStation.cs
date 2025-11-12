using UnityEngine;

public class BowlStation : MonoBehaviour
{
    [Header("Configuración del Spawn")]
    [SerializeField] public GameObject bowlPrefab;
    [SerializeField] public Transform spawnPoint;
    [SerializeField] private float spawnCooldown = 2f;
    [SerializeField] private int maxBowls = 3;

    private float lastSpawnTime;
    private int currentBowls;

    private bool hasABowl = false;

    private void Start()
    {
        if (bowlPrefab == null || spawnPoint == null)
        {
            Debug.LogError("❌ Falta asignar el prefab o spawn point.");
            return;
        }

        GameObject newBowl = Instantiate(
            bowlPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        ReceivingContainer newBowlPouring = newBowl.GetComponent<ReceivingContainer>();

        newBowlPouring.cupTracker = FindFirstObjectByType<CupTracker>();

        newBowl.name = "Bowl_" + currentBowls;
        currentBowls++;
        lastSpawnTime = Time.time;
        hasABowl = true;
    }


    public GameObject TrySpawnBowl()
    {
        if (Time.time - lastSpawnTime < spawnCooldown)
        {
            Debug.Log("⏳ Espera antes de generar otro bowl.");
            return null;
        }

        if (currentBowls >= maxBowls)
        {
            Debug.Log("🚫 Límite de bowls alcanzado.");
            return null;
        }

        if (bowlPrefab == null || spawnPoint == null)
        {
            Debug.LogError("❌ Falta asignar el prefab o spawn point.");
            return null;
        }

        GameObject newBowl = Instantiate(
            bowlPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        ReceivingContainer newBowlPouring = newBowl.GetComponent<ReceivingContainer>();

        newBowlPouring.cupTracker = FindFirstObjectByType<CupTracker>();

        newBowl.name = "Bowl_" + currentBowls;
        currentBowls++;
        lastSpawnTime = Time.time;

        return newBowl;
    }

    public void OnBowlTaken()
    {
        currentBowls = Mathf.Max(0, currentBowls - 1);
    }
}
