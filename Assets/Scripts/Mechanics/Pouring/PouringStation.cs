using UnityEngine;

/// <summary>
/// Estación de vertido que bloquea jugador y cámara al colocar recipiente.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PouringStation : MonoBehaviour
{
    [Header("Station Camera & UI")]
    [SerializeField] private Camera stationCamera;
    [SerializeField] private GameObject ingredientPanel;

    [Header("Player and Controllers")]
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FollowPlayer cameraFollow;
    [SerializeField] private CameraController cameraController;

    [Header("Target Receiving Container")]
    [SerializeField] private ReceivingContainer receivingContainer;

    private bool playerLocked;
    private Camera previousCamera;

    private void OnTriggerEnter(Collider other)
    {
        ReceivingContainer placed = other.GetComponent<ReceivingContainer>();
        if (placed == null) return;

        Debug.Log($"Placed {placed.name} in station");

        foreach (var pouring in Object.FindObjectsByType<PouringContainer>(FindObjectsSortMode.None))
            pouring.targetContainer = placed;

        LockPlayer();
    }

    private void OnTriggerExit(Collider other)
    {
        ReceivingContainer placed = other.GetComponent<ReceivingContainer>();
        if (placed == null) return;

        Debug.Log($"Removed {placed.name} from station");

        foreach (var pouring in Object.FindObjectsByType<PouringContainer>(FindObjectsSortMode.None))
        {
            if (pouring.targetContainer == placed)
                pouring.targetContainer = null;
        }

        UnlockPlayer();
    }

    public void LockPlayer()
    {
        if (playerLocked) return;
        playerLocked = true;

        ingredientPanel?.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraFollow != null) cameraFollow.enabled = false;

        if (cameraController != null && stationCamera != null)
        {
            previousCamera = cameraController.GetActiveCamera();
            cameraController.ActivateCamera(stationCamera);
        }
    }

    public void UnlockPlayer()
    {
        if (!playerLocked) return;
        playerLocked = false;

        ingredientPanel?.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraFollow != null) cameraFollow.enabled = true;

        if (cameraController != null && previousCamera != null)
            cameraController.ActivateCamera(previousCamera);
    }
}
