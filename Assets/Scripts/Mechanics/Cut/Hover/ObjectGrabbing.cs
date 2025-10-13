using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabbing : MonoBehaviour
{
    public static event System.Action OnCuttingStarted;

    [Header("Objeto a tomar")]
    public GameObject objectToGrab;
    public HoverAndCut scriptToEnable;

    [Header("Raycast")]
    public LayerMask pickupLayer;
    [Tooltip("Asigna aquí la cámara que usará el raycast")]
    public Camera targetCamera; // <-- cámara de estación o principal

    void Start()
    {
        // Si no asignas cámara, usa la principal
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        if (scriptToEnable.enabled) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, pickupLayer))
            {
                if (hit.collider.transform.IsChildOf(objectToGrab.transform))
                {
                    OnCuttingStarted?.Invoke();

                    Vector3 startPosition = objectToGrab.transform.position;
                    scriptToEnable.enabled = true;

                    // Le pasamos la cámara activa
                    scriptToEnable.Activate(startPosition, targetCamera);

                    objectToGrab.SetActive(false);
                }
            }
        }
    }

    public void HandleReset()
    {
        scriptToEnable.enabled = false;
        foreach (var piece in GameObject.FindGameObjectsWithTag("CutPiece"))
            Destroy(piece);

        objectToGrab.SetActive(true);
        enabled = true;
    }
}
