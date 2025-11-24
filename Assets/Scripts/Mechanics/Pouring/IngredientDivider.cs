using UnityEngine;
using System.Collections;
using TMPro;
using System.Diagnostics;

/// <summary>
/// Gestiona la división de un recipiente principal en múltiples tazas,
/// distribuyendo el contenido equitativamente y animando el proceso.
/// </summary>
public class IngredientDivider : MonoBehaviour
{
    [Header("Prefab de taza (con PouringContainer)")]
    [SerializeField] private GameObject cupPrefab;

    [Header("Contenedor para los puntos de spawn")]
    [SerializeField] public Transform spawnParent;

    [Header("Separación entre tazas")]
    private float spacing = 0.3f;

    [Header("Estación de vertido activa")]
    [SerializeField] private PouringStation pouringStation;

    [Header("Parámetros de animación")]
    private float moveHeight = 0.8f;
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private float tiltAngle = 45f;

    [Header("Referencia para posicionar las tazas")]
    [SerializeField] private Transform cupSpawnReference;

    private Transform[] spawnPoints;

    /// <summary>
    /// Crea dinámicamente el contenedor de puntos de aparición si no existe.
    /// </summary>
    private void Awake()
    {
        if (spawnParent != null) return;

        GameObject parentObj = new("CupSpawnPoints");
        parentObj.transform.SetParent(transform);
        parentObj.transform.localPosition = Vector3.zero;
        spawnParent = parentObj.transform;
    }

    /// <summary>
    /// Divide el contenido del recipiente activo en un número de partes iguales.
    /// </summary>
    /// <param name="parts">Número de tazas a generar.</param>
    public void DivideIntoCups(int parts, string fraction)
    {
        ClearCups();

        PouringContainer activeContainer = pouringStation.GetActivePouring();
        if (activeContainer == null)
        {
            UnityEngine.Debug.LogWarning("No hay recipiente activo para dividir.");
            return;
        }

        float total = activeContainer.currentML;
        if (total <= 0f)
        {
            UnityEngine.Debug.LogWarning("El recipiente está vacío.");
            return;
        }

        GenerateSpawnPoints(parts);

        float perCup = total / parts;
        activeContainer.currentML = 0f;
        activeContainer.amountText.text = "...";

        StartCoroutine(DivideRoutine(activeContainer, parts, perCup, fraction));
    }

    /// <summary>
    /// Genera los puntos de aparición de las tazas en una fila.
    /// </summary>
    private void GenerateSpawnPoints(int count)
    {
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);

        spawnPoints = new Transform[count];
        Vector3 basePos = cupSpawnReference.position;
        float startOffset = -(count - 1) * spacing / 2f;

        for (int i = 0; i < count; i++)
        {
            GameObject point = new($"SpawnPoint_{i + 1}");
            point.transform.SetParent(spawnParent);
            point.transform.position =
                basePos + new Vector3(startOffset + i * spacing, 0f, 0f);
            point.transform.rotation = Quaternion.identity;
            spawnPoints[i] = point.transform;
        }
    }

    /// <summary>
    /// Realiza la animación y vertido de líquido en cada taza.
    /// </summary>
    private IEnumerator DivideRoutine(
    PouringContainer original, int parts, float perCup, string fractionLabel)
    {
        for (int i = 0; i < parts && i < spawnPoints.Length; i++)
        {
            // Crear la taza
            GameObject cupObj = Instantiate(
                cupPrefab, spawnPoints[i].position,
                spawnPoints[i].rotation, spawnParent
            );

            PouringContainer cup = cupObj.GetComponent<PouringContainer>();
            cup.streamPrefab = original.streamPrefab;
            if (cup == null)
            {
                UnityEngine.Debug.LogError(
                    "El prefab de taza no contiene PouringContainer.");
                continue;
            }

            InteractableObject capabilities =
            cupObj.GetComponent<InteractableObject>();
            capabilities.capabilities |= ObjectCapabilities.Pourable;
            capabilities.capabilities |= ObjectCapabilities.PourableAllOnce;

            cup.ingredientName = original.ingredientName;
            cup.targetContainer = original.targetContainer;
            cup.currentML = 0f;
            cup.amountText.text = "";
            SetIngredientName(cup, GetIngredientName(original));

            // Animación de vertido
            yield return StartCoroutine(AnimatePour(
                original, spawnPoints[i].position, perCup, cup, fractionLabel
            ));
        }

        original.currentML = 0f;
        original.amountText.text = "Vacío";
    }

    /// <summary>
    /// Ejecuta la animación y llenado de una taza individual.
    /// </summary>
    private IEnumerator AnimatePour(
        PouringContainer original, Vector3 targetPos,
        float perCup, PouringContainer cup, string fractionLabel)
    {
        //  Offset local de la punta (respecto al pivote)
        Vector3 localTipOffset =
            original.transform.InverseTransformPoint(original.origin.position);

        // Calcular posición objetivo para que la punta quede en el centro de la taza
        Vector3 desiredTipWorld = targetPos; // el centro exacto de la taza
        Vector3 liftedPos =
            desiredTipWorld - original.transform.TransformVector(localTipOffset)
            + Vector3.up * moveHeight;

        liftedPos += Vector3.right * 0.19f;

        // Guardar rotaciones
        Quaternion startRot = original.transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltAngle);

        // Movimiento hacia la taza
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / moveDuration);
            original.transform.position =
                Vector3.Lerp(original.transform.position, liftedPos, norm);
            original.transform.rotation =
                Quaternion.Slerp(startRot, targetRot, norm);
            yield return null;
        }

        // Iniciar vertido
        original.StartStream();

        float poured = 0f;
        while (poured < perCup)
        {
            float delta = original.pourRateMLPerSec * Time.deltaTime;
            poured = Mathf.Min(poured + delta, perCup);
            cup.currentML = poured;
            cup.amountText.text = $"{fractionLabel}";
            yield return null;
        }

        original.EndStream();

        // 6️⃣ Regresar
        t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / moveDuration);
            original.transform.position =
                Vector3.Lerp(liftedPos, original.transform.position, norm);
            original.transform.rotation =
                Quaternion.Slerp(targetRot, startRot, norm);
            yield return null;
        }
    }

    /// <summary>
    /// Obtiene el nombre del ingrediente usando reflexión.
    /// </summary>
    private string GetIngredientName(PouringContainer container)
    {
        var field = typeof(PouringContainer).GetField(
            "ingredientName",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );
        return field?.GetValue(container)?.ToString() ?? "Ingrediente";
    }

    /// <summary>
    /// Asigna un nombre al ingrediente por reflexión.
    /// </summary>
    private void SetIngredientName(PouringContainer container, string value)
    {
        var field = typeof(PouringContainer).GetField(
            "ingredientName",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );
        field?.SetValue(container, value);
    }

    /// <summary>
    /// Elimina todas las tazas generadas.
    /// </summary>
    public void ClearCups()
    {
        PouringContainer[] existingCups =
            spawnParent.GetComponentsInChildren<PouringContainer>();
        foreach (var cup in existingCups)
            Destroy(cup.gameObject);
    }
}
