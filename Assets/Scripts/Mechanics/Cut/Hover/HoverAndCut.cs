using UnityEngine;
using UnityEngine.InputSystem;

public class HoverAndCut : MonoBehaviour
{
    public static event System.Action OnTargetCutsReached;
    public static event System.Action<int, int> OnCutCountUpdated;

    [Header("Objeto de corte")]
    public GameObject cutterPrefab;

    [Header("Layer de objetos interactuables")]
    public LayerMask interactableLayer;

    [Header("Gameplay")]
    [Tooltip("Número de cortes necesarios para reiniciar")]
    public int targetCuts = 10;

    [Header("Hover Behavior")]
    [Tooltip("Altura del cuchillo sobre el objeto")]
    public float yOffset = 0.1f;

    public GameObject cutterInstance;
    private Transform knifeVisualsTransform;
    private Camera activeCamera;
    private Vector3 positionOffset;
    private Plane horizontalPlane;
    private int cutCount = 0;

    /// <summary>
    /// Activa el cuchillo y asigna la cámara que se usará para raycast.
    /// </summary>
    public void Activate(Vector3 startPosition, Camera cameraToUse)
    {
        activeCamera = cameraToUse;

        if (cutterPrefab != null)
        {
            Vector3 adjustedStartPosition = startPosition;
            adjustedStartPosition.y += yOffset;
            horizontalPlane = new Plane(Vector3.up, adjustedStartPosition);

            cutterInstance = Instantiate(cutterPrefab);
            cutterInstance.transform.position = adjustedStartPosition;

            knifeVisualsTransform = (cutterInstance.transform.childCount > 0)
                ? cutterInstance.transform.GetChild(0)
                : cutterInstance.transform;

            knifeVisualsTransform.rotation = Quaternion.Euler(90f, -90f, 0f);

            // Calculamos el offset inicial con la cámara asignada
            Ray ray = activeCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (horizontalPlane.Raycast(ray, out float distance))
            {
                Vector3 mouseOnPlane = ray.GetPoint(distance);
                positionOffset = cutterInstance.transform.position - mouseOnPlane;
            }
        }
    }

    public void OnEnable()
    {
        cutCount = 0;
        OnCutCountUpdated?.Invoke(cutCount, targetCuts);
    }

    public void OnDisable()
    {
        if (cutterInstance != null)
            Destroy(cutterInstance);
    }

    void Update()
    {
        if (cutterInstance == null || Mouse.current == null) return;

        Ray ray = activeCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (horizontalPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseOnPlane = ray.GetPoint(distance);
            cutterInstance.transform.position = mouseOnPlane + positionOffset;
        }

        knifeVisualsTransform.rotation = Quaternion.Euler(90f, -90f, 0f);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 cutPoint = cutterInstance.transform.position;
            Vector3 cutNormal = knifeVisualsTransform.up;

            Collider[] objectsToCut = Physics.OverlapBox(
                cutPoint,
                new Vector3(10f, 0.1f, 10f),
                knifeVisualsTransform.rotation,
                interactableLayer
            );

            bool aCutWasMade = false;
            foreach (var victim in objectsToCut)
            {
                InteractableObject interactable = victim.GetComponent<InteractableObject>();
                if (interactable != null && interactable.HasCapability(ObjectCapabilities.Cuttable))
                {
                    Cutter.Cut(victim.gameObject, cutPoint, cutNormal);
                    aCutWasMade = true;
                }
            }

            if (aCutWasMade)
            {
                cutCount++;
                OnCutCountUpdated?.Invoke(cutCount, targetCuts);

                if (cutCount >= targetCuts)
                {
                    OnTargetCutsReached?.Invoke();
                }
            }
        }
    }
}
