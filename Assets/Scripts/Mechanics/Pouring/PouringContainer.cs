using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Controla un recipiente que puede verter líquido hacia un 
/// <see cref="ReceivingContainer"/>.
/// Incluye animaciones de vertido, efectos de partículas y control de UI.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class PouringContainer : MonoBehaviour
{
    // ──────────────────────────────── CONTENEDOR DESTINO ──────────────────────────

    [Header("Target Container")]
    [Tooltip("Recipiente objetivo donde se verterá el líquido.")]
    [SerializeField] 
    public ReceivingContainer targetContainer;

    // ──────────────────────────────── INFORMACIÓN DEL INGREDIENTE ─────────────────

    [Header("Ingredient Info")]
    [Tooltip("Nombre del ingrediente contenido.")]
    [SerializeField] 
    public string ingredientName = "Agua";

    // ──────────────────────────────── UI DE CANTIDAD ──────────────────────────────

    [Header("UI Amount Text")]
    [Tooltip("Texto en pantalla que muestra la cantidad restante.")]
    [SerializeField] 
    public TextMeshProUGUI amountText;

    // ──────────────────────────────── CAPACIDAD Y TASA ────────────────────────────

    [Header("Capacity Settings")]
    [Tooltip("Capacidad máxima en mililitros.")]
    [SerializeField] 
    private float capacityML = 1000f;

    [Tooltip("Tasa de vertido en mililitros por segundo.")]
    [SerializeField] 
    public float pourRateMLPerSec = 100f;

    [Tooltip("Cantidad actual de líquido.")]
    public float currentML;

    // ──────────────────────────────── EFECTOS Y TRANSFORM ─────────────────────────


    [Header("Transform Reference")]
    [Tooltip("Referencia al transform del recipiente.")]
    [SerializeField]
    private Transform containerTransform;

    [Header("Stream Prefab")]
    [Tooltip("Prefab del efecto de chorro de vertido.")]
    public GameObject streamPrefab;

    [Header("Stream State")]
    [Tooltip("Estado actual del stream.")]
    private Stream currentStream = null; // Reference to the current stream effect

    
    [Header("Origin Point")]
    [Tooltip("Punto de origen del vertido.")]   
    public Transform origin;

    // ──────────────────────────────── CUP TRACKER ─────────────────────────────────

    [Header("CupTracker (para notificar cambios de cantidad)")]
    [Tooltip("Referencia al CupTracker global.")]
    [SerializeField] 
    private CupTracker cupTracker;

    // ──────────────────────────────── VARIABLES PRIVADAS ──────────────────────────

    private bool isPouring;
    private InteractableObject interactable;

    // ──────────────────────────────── CICLO DE VIDA ───────────────────────────────

    /// <summary>
    /// Inicializa el recipiente, configura capacidades, partículas y referencias.
    /// </summary>
    private void Start()
    {
        currentML = capacityML;
        interactable = GetComponent<InteractableObject>();

        // Asegura que tenga capacidad de verter.
        if (!interactable.HasCapability(ObjectCapabilities.Pourable))
        {
            interactable.capabilities |= ObjectCapabilities.Pourable;
        }

        if (containerTransform == null) containerTransform = transform;



        // Busca el CupTracker si no está asignado.
        if (cupTracker == null)
            cupTracker = FindFirstObjectByType<CupTracker>();

    }

    /// <summary>
    /// Valida condiciones de vertido en cada frame.
    /// (Reservado para lógica futura o control de ángulo).
    /// </summary>
    private void Update()
    {
        if (!interactable.HasCapability(ObjectCapabilities.Pourable)) return;
        if (targetContainer == null) return;
    }

    // ──────────────────────────────── FUNCIONALIDAD PRINCIPAL ─────────────────────

    /// <summary>
    /// Vierte todo el contenido del recipiente de una sola vez con animación.
    /// </summary>
    /// <param name="duration">Duración total del vertido.</param>
    /// <param name="moveHeight">Altura de movimiento al inclinar.</param>
    /// <param name="tiltAngle">Ángulo de inclinación durante el vertido.</param>
    public void PourAllOnce(
        float duration = 0.5f, float moveHeight = 0.2f, float tiltAngle = 45f)
    {
        if (targetContainer == null || currentML <= 0f) return;

        // Define posición inicial sobre el recipiente destino.
        Vector3 startPos = targetContainer.transform.position + 
                           Vector3.up * 0.05f;

        StartCoroutine(
            PourAllRoutine(duration, moveHeight, tiltAngle, startPos)
        );
    }

    /// <summary>
    /// Corrutina que ejecuta la animación completa del vertido:
    /// inclinación, vaciado, y retorno a posición inicial.
    /// </summary>
    private IEnumerator PourAllRoutine(
        float duration, float moveHeight, float tiltAngle, Vector3 startPos)
    {
        float startAmount = currentML;
        float t = 0f;

        Quaternion startRot = transform.rotation;
        Vector3 targetPos = startPos + Vector3.up * moveHeight;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltAngle);

        targetPos += Vector3.right * 0.19f;


        // 🔹 Animar elevación e inclinación inicial
        float animDuration = duration * 0.3f;
        t = 0f;
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / animDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, norm);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, norm);
            yield return null;
        }
        StartStream();
        // 🔹 Vaciar contenido progresivamente
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float poured = startAmount * normalized;
            targetContainer.AddLiquid(
                ingredientName, poured - (startAmount - currentML)
            );

            currentML = startAmount * (1f - normalized);
            amountText.text = $"{currentML:F0}";
            yield return null;
        }

        // 🔹 Actualizar UI y estado final
        currentML = 0f;
        amountText.text = "0";
        // 🔹 Retornar a la posición original
        t = 0f;
        animDuration = duration * 0.3f;
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / animDuration);
            transform.position = Vector3.Lerp(targetPos, startPos, norm);
            transform.rotation = Quaternion.Slerp(targetRot, startRot, norm);
            yield return null;
        }

        // 🔹 Finalizar y actualizar registro global
        EndStream();
        currentML = 0f;
        amountText.text = "0 ml";
        Destroy(gameObject);
        cupTracker.UpdateCups();
    }

    // ──────────────────────────────── EVENTOS DE INTERACCIÓN ──────────────────────

    /// <summary>
    /// Detecta clic sobre el recipiente y ejecuta vertido completo si es posible.
    /// </summary>
    public void OnMouseDown()
    {
        if (interactable.HasCapability(ObjectCapabilities.PourableAllOnce))
        {
            PourAllOnce(0.5f);
        }
    }

    public void StartStream()
    {
        currentStream = CreateStream();
        currentStream.BeginStream();
    }

    public void EndStream()
    {
        currentStream.End();
    }

    /// <summary>
    /// Calcula el ángulo actual de inclinación del recipiente.
    /// </summary>
    private Stream CreateStream()
    {
        GameObject streamObj = Instantiate(streamPrefab, origin.position, Quaternion.identity, transform);
        return streamObj.GetComponent<Stream>();
    }
}
