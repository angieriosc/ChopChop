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
    [SerializeField] private string ingredientName = "Agua";

    [Header("UI Amount Text")]
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Capacity Settings")]
    [SerializeField] private float capacityML = 1000f;
    [SerializeField] private float pourRateMLPerSec = 100f;
    public float currentML;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem pourParticles;

    [Header("Transform Reference")]
    [SerializeField] private Transform containerTransform;

    [Header("Pour Angle Settings")]
    [SerializeField] private float pourStartAngle = 35f;
    [SerializeField] private float pourStopAngle = 20f;

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

        pourParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (!interactable.HasCapability(ObjectCapabilities.Pourable)) return;
        if (targetContainer == null) return;

        float tiltAngle = Vector3.Angle(containerTransform.up, Vector3.up);

        if (tiltAngle >= pourStartAngle && !isPouring && currentML > 0f)
            StartPouring();
        else if (tiltAngle <= pourStopAngle && isPouring)
            StopPouring();

        if (isPouring && currentML > 0f) PourLiquid();
    }

    private void PourLiquid()
    {
        float pouredAmount = pourRateMLPerSec * Time.deltaTime;
        currentML -= pouredAmount;
        currentML = Mathf.Max(currentML, 0f);
        amountText.text = $"{currentML:F0} ml";

        targetContainer.AddLiquid(ingredientName, pouredAmount);

        if (currentML <= 0f) StopPouring();
    }

    public void StartPouring()
    {
        if (isPouring || currentML <= 0f) return;

        isPouring = true;
        if (pourParticles != null) pourParticles.Play();
        else Debug.LogWarning($"{name}: No particle system assigned");
    }

    public void StopPouring()
    {
        if (!isPouring) return;
        isPouring = false;
        pourParticles?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
