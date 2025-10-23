using UnityEngine;
using System.Collections;
using TMPro;

public class IngredientDivider : MonoBehaviour
{
    [Header("Prefab de taza (con PouringContainer)")]
    [SerializeField] private GameObject cupPrefab;

    [Header("Contenedor para los puntos de spawn")]
    [SerializeField] public Transform spawnParent;

    [Header("Separación entre tazas")]
    [SerializeField, Range(0.2f, 2f)] private float spacing = 0.8f;

    [Header("Estación de vertido")]
    [SerializeField] private PouringStation pouringStation;

    [Header("Animación de movimiento")]
    [SerializeField] private float moveHeight = 0.5f;
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private float tiltAngle = 45f;

    [Header("Referencia de spawn de taza")]
    [SerializeField] private Transform cupSpawnReference; 

    private Transform[] spawnPoints;

    private void Awake()
    {
        if (spawnParent == null)
        {
            GameObject parentObj = new GameObject("CupSpawnPoints");
            parentObj.transform.SetParent(transform);
            parentObj.transform.localPosition = Vector3.zero;
            spawnParent = parentObj.transform;
        }
    }

    public void DivideIntoCups(int parts)
    {
        PouringContainer[] existingCups = spawnParent.GetComponentsInChildren<PouringContainer>();
        foreach (var cup in existingCups)
        {
            Destroy(cup.gameObject);
        }

        PouringContainer activeContainer = pouringStation.GetActivePouring();
        if (activeContainer == null)
        {
            Debug.LogWarning("No hay recipiente activo para dividir.");
            return;
        }

        float total = activeContainer.currentML;
        if (total <= 0f)
        {
            Debug.LogWarning("El recipiente está vacío.");
            return;
        }

        GenerateSpawnPoints(parts);

        float perCup = total / parts;
        activeContainer.currentML = 0f;
        activeContainer.amountText.text = "0 ml";

        StartCoroutine(DivideRoutine(activeContainer, parts, perCup));
    }

    private void GenerateSpawnPoints(int count)
    {
        // Limpia spawn points anteriores
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);

        spawnPoints = new Transform[count];

        // Posición base sobre la estación
        Vector3 basePos = cupSpawnReference.position;
        float startOffset = -(count - 1) * spacing / 2f;

        for (int i = 0; i < count; i++)
        {
            GameObject point = new GameObject($"SpawnPoint_{i+1}");
            point.transform.SetParent(spawnParent);

            // Distribuir en fila sobre la estación
            point.transform.position = basePos + new Vector3(startOffset + i * spacing, 0f, 0f);
            point.transform.rotation = Quaternion.identity;

            spawnPoints[i] = point.transform;
        }
    }


    private IEnumerator DivideRoutine(PouringContainer original, int parts, float perCup)
    {
        for (int i = 0; i < parts && i < spawnPoints.Length; i++)
        {
            // Instanciar taza
            GameObject cupObj = Instantiate(
                cupPrefab,
                spawnPoints[i].position,
                spawnPoints[i].rotation,
                spawnParent 
            );
            PouringContainer cup = cupObj.GetComponent<PouringContainer>();
            InteractableObject cupCapabilities = cupObj.GetComponent<InteractableObject>();
            cupCapabilities.capabilities |= ObjectCapabilities.Pourable;
            cupCapabilities.capabilities |= ObjectCapabilities.PourableAllOnce;
            cup.ingredientName = original.ingredientName;
            if (cup == null)
            {
                Debug.LogError("El prefab de taza no tiene PouringContainer.");
                continue;
            }

            cup.targetContainer = original.targetContainer;
            cup.currentML = 0f; // empieza vacío
            SetIngredientName(cup, GetIngredientName(original));
            cup.amountText.text = "0 ml";

            // Animación del recipiente original sobre la taza
            Vector3 startPos = original.transform.position;
            Vector3 targetPos = spawnPoints[i].position + Vector3.up * moveHeight;
            Quaternion startRot = original.transform.rotation;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltAngle);

            float t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / moveDuration);
                original.transform.position = Vector3.Lerp(startPos, targetPos, normalized);
                original.transform.rotation = Quaternion.Slerp(startRot, targetRot, normalized);
                yield return null;
            }

            // Simular vertido con partículas y llenar taza
            if (original.pourParticles != null) original.pourParticles.Play();
            float poured = 0f;
            while (poured < perCup)
            {
                float delta = original.pourRateMLPerSec * Time.deltaTime;
                poured += delta;
                poured = Mathf.Min(poured, perCup);

                cup.currentML = poured;
                cup.amountText.text = $"{poured:F0} ml";
                yield return null;
            }

            if (original.pourParticles != null) original.pourParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            // Regresar recipiente original a su posición inicial
            t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / moveDuration);
                original.transform.position = Vector3.Lerp(targetPos, startPos, normalized);
                original.transform.rotation = Quaternion.Slerp(targetRot, startRot, normalized);
                yield return null;
            }
        }

        // Vaciar el recipiente original completamente
        original.currentML = 0f;
        original.amountText.text = "0 ml";
    }

    private string GetIngredientName(PouringContainer container)
    {
        var field = typeof(PouringContainer).GetField("ingredientName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(container)?.ToString() ?? "Ingrediente";
    }

    private void SetIngredientName(PouringContainer container, string value)
    {
        var field = typeof(PouringContainer).GetField("ingredientName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(container, value);
    }

    public void ClearCups()
    {
        PouringContainer[] existingCups = spawnParent.GetComponentsInChildren<PouringContainer>();
        foreach (var cup in existingCups)
        {
            Destroy(cup.gameObject);
        }
    }

}
