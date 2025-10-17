using UnityEngine;

/// <summary>
/// Estación de vertido donde el jugador coloca un recipiente para llenar con ingredientes.
/// Bloquea movimiento del jugador y activa la cámara de estación.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PouringStation : MonoBehaviour
{
    [Header("Station Camera & UI")]
    [Tooltip("Cámara que se activa al colocar recipiente.")]
    [SerializeField] private Camera stationCamera;

    [Tooltip("Panel de ingredientes que se muestra al colocar recipiente.")]
    [SerializeField] private GameObject ingredientPanel;

    [Header("Player and Controllers")]
    [Tooltip("Referencia al jugador.")]
    [SerializeField] private GameObject player;

    [Tooltip("Script de movimiento del jugador.")]
    [SerializeField] private PlayerMovement playerMovement;

    [Tooltip("Script que sigue al jugador con la cámara.")]
    [SerializeField] private FollowPlayer cameraFollow;

    [Tooltip("Controlador de cámaras.")]
    [SerializeField] private CameraController cameraController;

    [Header("Target Receiving Container")]
    [Tooltip("Recipiente actualmente en la estación.")]
    [SerializeField] private ReceivingContainer receivingContainer;

    public ReceivingContainer CurrentReceiving => receivingContainer;

    private bool playerLocked;         // Indica si el jugador está bloqueado
    private Camera previousCamera;     // Cámara previa antes de entrar a la estación

    /// <summary>
    /// Detecta cuando un recipiente entra en la estación.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        ReceivingContainer placed = other.GetComponent<ReceivingContainer>();
        if (placed == null) return;

        receivingContainer = placed;

        // Asignar targetContainer a todos los PouringContainer activos
        foreach (var pouring in Object.FindObjectsByType<PouringContainer>(
                     FindObjectsSortMode.None))
        {
            pouring.targetContainer = placed;
        }

        LockPlayer();
    }

    /// <summary>
    /// Detecta cuando un recipiente sale de la estación.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        ReceivingContainer placed = other.GetComponent<ReceivingContainer>();
        if (placed == null) return;

        if (placed == receivingContainer) receivingContainer = null;

        foreach (var pouring in Object.FindObjectsByType<PouringContainer>(
                     FindObjectsSortMode.None))
        {
            if (pouring.targetContainer == placed)
                pouring.targetContainer = null;
        }

        UnlockPlayer();
    }

    /// <summary>
    /// Bloquea al jugador y activa la UI y cámara de estación.
    /// </summary>
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

    /// <summary>
    /// Desbloquea al jugador, desactiva UI y restaura cámara previa.
    /// </summary>
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
