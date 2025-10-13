using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Permite animar y controlar un recipiente clickeable para verter.
/// </summary>
[RequireComponent(typeof(PouringContainer))]
public class ClickableIngredientButton : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Transform model;
    [SerializeField] private float liftHeight = 0.25f;
    [SerializeField] private float liftSpeed = 5f;
    [SerializeField] private float pourRotationAngle = 120f;
    [SerializeField] private float rotationSmooth = 3f;
    [SerializeField] private float snapThreshold = 0.001f;

    [Header("Button Reference")]
    [SerializeField] private Button clickButton;

    private PouringContainer pouringContainer;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private bool isHeld;
    private float liftProgress;

    private void Start()
    {
        pouringContainer = GetComponent<PouringContainer>();
        if (model == null) model = transform;

        initialLocalPos = model.localPosition;
        initialLocalRot = model.localRotation;

        if (clickButton != null)
        {
            clickButton.onClick.AddListener(TogglePouring);
            Debug.Log("[ClickableIngredientButton] Button listener added");
        }
        else
        {
            Debug.LogWarning("[ClickableIngredientButton] ClickButton not assigned!");
        }
    }

    private void Update() => AnimateLiftAndRotation();

    /// <summary>
    /// Alterna estado de vertido al hacer clic en el botón.
    /// </summary>
    public void TogglePouring()
    {
        isHeld = !isHeld;
        Debug.Log($"Button clicked! isHeld={isHeld}, currentML={pouringContainer.currentML}");
    }

    private void AnimateLiftAndRotation()
    {
        float targetLift = isHeld ? 1f : 0f;
        liftProgress = Mathf.MoveTowards(liftProgress, targetLift, Time.deltaTime * liftSpeed);
        model.localPosition = initialLocalPos + Vector3.up * (liftHeight * liftProgress);

        float targetAngle = isHeld ? pourRotationAngle : 0f;
        Quaternion targetRot = initialLocalRot * Quaternion.Euler(targetAngle, 0, 0);
        model.localRotation = Quaternion.Slerp(model.localRotation, targetRot,
            Time.deltaTime * rotationSmooth);

        if (!isHeld) SnapToInitial();
    }

    private void SnapToInitial()
    {
        if (Vector3.Distance(model.localPosition, initialLocalPos) < snapThreshold)
            model.localPosition = initialLocalPos;

        if (Quaternion.Angle(model.localRotation, initialLocalRot) < snapThreshold * 10f)
            model.localRotation = initialLocalRot;
    }
}
