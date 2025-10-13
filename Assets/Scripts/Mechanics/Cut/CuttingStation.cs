using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class CuttingStation : MonoBehaviour
{
    [Header("Cámara de estación de corte")]
    public Camera stationCamera;

    [Header("Jugador y controladores")]
    public GameObject player;
    public PlayerMovement playerMovement;
    public FollowPlayer cameraFollow;
    public CameraController cameraController;

    [Header("Paneles de UI opcionales")]
    public GameObject cuttingPanel;

    [Header("Sistema de corte")]
    public ObjectGrabbing objectGrabbing; // Script que activa el cuchillo

    private bool playerNearby = false;
    private bool playerLocked = false;

    private Camera previousCamera;


    private void Update()
    {
        if (!playerNearby || playerLocked) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Presionaste E: entrando a la estación de corte");
            LockPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            playerNearby = true;
            Debug.Log("Jugador cerca de la estación de corte. Presiona E para entrar.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerNearby = false;
            if (playerLocked)
                UnlockPlayer(); // Al salir, desbloquea al jugador
        }
    }

    public void LockPlayer()
    {
        if (playerLocked) return;
        playerLocked = true;

        cuttingPanel?.SetActive(true);

        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraFollow != null) cameraFollow.enabled = false;

        if (cameraController != null && stationCamera != null)
            previousCamera = cameraController.GetActiveCamera();
            cameraController.ActivateCamera(stationCamera);

        if (objectGrabbing != null)
        {
            objectGrabbing.enabled = true;
            Debug.Log("Modo corte activado.");
        }
    }

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

        // Soltar el cuchillo en la escena, pero no destruirlo
        if (objectGrabbing != null && objectGrabbing.scriptToEnable != null)
        {
            // Limpiar las piezas cortadas
            foreach (var piece in GameObject.FindGameObjectsWithTag("CutPiece"))
                Destroy(piece);

            // Soltar el cutterInstance
            if (objectGrabbing.scriptToEnable.cutterInstance != null)
            {
                objectGrabbing.scriptToEnable.cutterInstance.transform.parent = null;
                // Opcional: activar physics para que caiga suavemente
                Rigidbody rb = objectGrabbing.scriptToEnable.cutterInstance.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
            }
            if (objectGrabbing != null)
            {
                objectGrabbing.HandleReset(); // Suelta el cuchillo y destruye piezas
            }

            // Desactivar solo el modo de corte, no el cuchillo
            objectGrabbing.enabled = false;
            objectGrabbing.scriptToEnable.enabled = false;
        }

        Debug.Log("Modo corte desactivado. Jugador desbloqueado y cuchillo suelto.");
    }

}
    
    

