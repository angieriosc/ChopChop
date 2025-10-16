using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Gestiona la interacción de agarrar un objeto y activar el corte.
/// Funciona únicamente dentro de la estación activa.
/// </summary>
public class ObjectGrabbing : MonoBehaviour
{
    /// <summary>
    /// Evento que se dispara al iniciar el corte.
    /// </summary>
    public static event System.Action OnCuttingStarted;

    [Header("Objeto a tomar")]
    [Tooltip("Objeto que se puede agarrar y cortar.")]
    public GameObject objectToGrab;

    [Tooltip("Script HoverAndCut que se habilitará al agarrar el objeto.")]
    public HoverAndCut scriptToEnable;

    [Header("Raycast")]
    [Tooltip("Layers que se pueden seleccionar con el raycast.")]
    public LayerMask pickupLayer;

    [Tooltip("Cámara desde la cual se dispara el raycast (usualmente la cámara de la estación).")]
    public Camera targetCamera;

    /// <summary>
    /// Inicializa referencias y desactiva el script hasta entrar a la estación.
    /// </summary>
    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Desactivar script hasta que se ingrese a la estación
        enabled = false;
    }

    /// <summary>
    /// Comprueba cada frame si se puede agarrar el objeto y activar el corte.
    /// </summary>
    private void Update()
    {
        if (!enabled || scriptToEnable.enabled) return;

        // Evitar raycast si el cursor está sobre la UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(Mouse.current.deviceId))
        {
            return;
        }

        // Detectar clic izquierdo
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Raycast para detectar objeto a agarrar
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, pickupLayer))
            {
                // Comprobar si el objeto clickeado es hijo del objeto a agarrar
                if (hit.collider.transform.IsChildOf(objectToGrab.transform))
                {
                    // Disparar evento de inicio de corte
                    OnCuttingStarted?.Invoke();

                    // Guardar posición inicial del objeto
                    Vector3 startPosition = objectToGrab.transform.position;

                    // Activar script de corte
                    scriptToEnable.enabled = true;
                    scriptToEnable.Activate(startPosition, targetCamera);

                    // Ocultar objeto original
                    objectToGrab.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Resetea el objeto y destruye piezas de corte generadas.
    /// Se usa al salir o reiniciar la estación.
    /// </summary>
    public void HandleReset()
    {
        // Desactivar script de corte
        scriptToEnable.enabled = false;

        // Destruir todas las piezas cortadas existentes
        foreach (var piece in GameObject.FindGameObjectsWithTag("CutPiece"))
            Destroy(piece);

        // Reactivar objeto original
        objectToGrab.SetActive(true);

        // Desactivar script hasta volver a entrar a la estación
        enabled = false;
    }
}
