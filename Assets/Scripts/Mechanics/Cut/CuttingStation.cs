using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la estación de corte donde el jugador puede interactuar con objetos
/// para iniciar la mecánica de corte.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CuttingStation : MonoBehaviour
{
    [Header("Cámara de estación de corte")]
    [Tooltip("Cámara que se activa al entrar en la estación de corte.")]
    public Camera stationCamera;

    [Header("Jugador y controladores")]
    [Tooltip("Referencia al jugador.")]
    public GameObject player;

    [Tooltip("Script de movimiento del jugador.")]
    public PlayerMovement playerMovement;

    [Tooltip("Script para seguir al jugador con la cámara.")]
    public FollowPlayer cameraFollow;

    [Tooltip("Controlador de cámaras para cambiar la cámara activa.")]
    public CameraController cameraController;

    [Header("Paneles de UI opcionales")]
    [Tooltip("Panel de UI que se muestra al entrar a la estación de corte.")]
    public GameObject cuttingPanel;

    [Header("Sistema de corte")]
    [Tooltip("Script ObjectGrabbing que se activa para cortar.")]
    public ObjectGrabbing objectGrabbing;

    private bool playerNearby = false;     // Indica si el jugador está cerca
    private bool playerLocked = false;     // Indica si el jugador está bloqueado en la estación
    private Camera previousCamera;         // Guarda la cámara previa para restaurarla al salir

    /// <summary>
    /// Detecta la entrada del jugador a la estación y presionar E para bloquearlo.
    /// </summary>
    private void Update()
    {
        if (playerNearby && !playerLocked &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            LockPlayer();
        }
    }

    /// <summary>
    /// Detecta cuando el jugador entra al área de la estación.
    /// </summary>
    /// <param name="other">Collider que entró.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            playerNearby = true;
            Debug.Log("Jugador cerca de la estación de corte. Presiona E para entrar.");
        }
    }

    /// <summary>
    /// Detecta cuando el jugador sale del área de la estación.
    /// </summary>
    /// <param name="other">Collider que salió.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerNearby = false;
            if (playerLocked)
                UnlockPlayer();
        }
    }

    /// <summary>
    /// Bloquea al jugador en la estación, cambia cámara y activa el sistema de corte.
    /// </summary>
    public void LockPlayer()
    {
        if (playerLocked) return;
        playerLocked = true;

        cuttingPanel?.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraFollow != null) cameraFollow.enabled = false;

        // Cambiar a cámara de estación
        if (cameraController != null && stationCamera != null)
        {
            previousCamera = cameraController.GetActiveCamera();
            cameraController.ActivateCamera(stationCamera);
        }

        // Activar ObjectGrabbing mientras estamos en la estación
        if (objectGrabbing != null)
            objectGrabbing.enabled = true;

        Debug.Log("Jugador bloqueado en estación de corte.");
    }

    /// <summary>
    /// Desbloquea al jugador, restaura cámara y desactiva el sistema de corte.
    /// </summary>
    public void UnlockPlayer()
    {
        if (!playerLocked) return;
        playerLocked = false;

        cuttingPanel?.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraFollow != null) cameraFollow.enabled = true;

        // Restaurar cámara previa
        if (cameraController != null && previousCamera != null)
            cameraController.ActivateCamera(previousCamera);

        // Desactivar ObjectGrabbing y resetear estado
        if (objectGrabbing != null)
        {
            objectGrabbing.enabled = false;
            objectGrabbing.HandleReset();
        }

        Debug.Log("Jugador desbloqueado y estación liberada.");
    }
}
