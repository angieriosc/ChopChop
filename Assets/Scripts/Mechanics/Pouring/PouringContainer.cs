using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Controla un recipiente que puede verter líquido en un ReceivingContainer.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class PouringContainer : MonoBehaviour
{
    [Header("Target Container")]
    [SerializeField] public ReceivingContainer targetContainer;

    [Header("Ingredient Info")]
    [SerializeField] public string ingredientName = "Agua";

    [Header("UI Amount Text")]
    [SerializeField] public TextMeshProUGUI amountText;

    [Header("Capacity Settings")]
    [SerializeField] private float capacityML = 1000f;
    [SerializeField] public float pourRateMLPerSec = 100f;
    public float currentML;

    [Header("Particle System")]
    [SerializeField] public ParticleSystem pourParticles;

    [Header("Transform Reference")]
    [SerializeField] private Transform containerTransform;

    [Header("Pour Angle Settings")]
    [SerializeField] private float pourStartAngle = 35f;
    [SerializeField] private float pourStopAngle = 20f;

    [Header("CupTracker (para notificar cambios de cantidad)")]
    [Tooltip("Asignar el CupTracker desde el inspector")]
    [SerializeField] private CupTracker cupTracker;

    private bool isPouring;
    private InteractableObject interactable;

    private void Start()
    {
        currentML = capacityML;
        interactable = GetComponent<InteractableObject>();
        amountText.text = $"{currentML:F0} ml";

        if (!interactable.HasCapability(ObjectCapabilities.Pourable))
            interactable.capabilities |= ObjectCapabilities.Pourable;

        if (containerTransform == null) containerTransform = transform;

        if (pourParticles == null)
        {
            pourParticles = GetComponentInChildren<ParticleSystem>(true);
            if (pourParticles == null)
                Debug.LogWarning($"{name}: ParticleSystem not found");
        }

        if (cupTracker == null)
            cupTracker = FindFirstObjectByType<CupTracker>();

        pourParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (!interactable.HasCapability(ObjectCapabilities.Pourable)) return;
        if (targetContainer == null) return;

    }

    public void PourAllOnce(float duration = 0.5f, float moveHeight = 0.2f, float tiltAngle = 45f)
    {
        if (targetContainer == null || currentML <= 0f) return;
        Vector3 startPos = targetContainer.transform.position + Vector3.up * 0.05f; // 5 cm sobre el contenedor
        StartCoroutine(PourAllRoutine(duration, moveHeight, tiltAngle, startPos));
    }

  private IEnumerator PourAllRoutine(float duration, float moveHeight, float tiltAngle, Vector3 startPos)
    {
        float startAmount = currentML;
        float t = 0f;

        Quaternion startRot = transform.rotation;
        Vector3 targetPos = startPos + Vector3.up * moveHeight;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltAngle);

        // 🔹 Subir e inclinar antes de vaciar
        t = 0f;
        float animDuration = duration * 0.3f;
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / animDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, norm);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, norm);
            yield return null;
        }

        // 🔹 Vaciar contenido
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float poured = startAmount * normalized;
            targetContainer.AddLiquid(ingredientName, poured - (startAmount - currentML));
            currentML = startAmount * (1f - normalized);
            amountText.text = $"{currentML:F0} ml";
            yield return null;
        }

        currentML = 0f;
        amountText.text = "0 ml";

        // 🔹 Regresar a posición inicial (sobre el contenedor)
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
        
        currentML = 0f;
        amountText.text = "0 ml";
        Destroy(gameObject);
        cupTracker.UpdateCups();    

    }
    private void OnMouseDown()
    {
        if(interactable.HasCapability(ObjectCapabilities.PourableAllOnce)){
            PourAllOnce(0.5f); // dura medio segundo            
        }
    }

}
