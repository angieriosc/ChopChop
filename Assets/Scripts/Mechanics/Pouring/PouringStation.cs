using UnityEngine;

/// <summary>
/// Estación de vertido donde el jugador coloca un recipiente 
/// para llenarlo con ingredientes. 
/// Controla el bloqueo del jugador, la cámara activa y la interfaz.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PouringStation : MonoBehaviour
{
    // ──────────────────────────── CONFIGURACIÓN VISUAL ────────────────────────────

    [Header("Station Camera & UI")]
    [Tooltip("Cámara que se activa al colocar un recipiente.")]
    [SerializeField] 
    private Camera stationCamera;

    [Tooltip("Panel de ingredientes visible al colocar el recipiente.")]
    [SerializeField] 
    private GameObject ingredientPanel;

    // ─────────────────────────────── CONTROL DEL JUGADOR ──────────────────────────

    [Header("Player and Controllers")]
    [Tooltip("Referencia al objeto del jugador.")]
    [SerializeField] 
    private GameObject player;

    [Tooltip("Script de movimiento del jugador.")]
    [SerializeField] 
    private PlayerMovement playerMovement;

    [Tooltip("Script que hace que la cámara siga al jugador.")]
    [SerializeField] 
    private FollowPlayer cameraFollow;

    [Tooltip("Controlador central de cámaras.")]
    [SerializeField] 
    private CameraController cameraController;

    // ─────────────────────────────── RECIPIENTES ──────────────────────────────────

    [Header("Target Receiving Container")]
    [Tooltip("Recipiente actualmente asignado a la estación.")]
    [SerializeField] 
    private ReceivingContainer receivingContainer;

    /// <summary>
    /// Obtiene el recipiente actual colocado en la estación.
    /// </summary>
    public ReceivingContainer CurrentReceiving => receivingContainer;

    [Header("Recipiente activo en la estación")]
    [Tooltip("Contenedor activo que se está utilizando para verter.")]
    [SerializeField] 
    public PouringContainer activePouringContainer;

    // ─────────────────────────────── INTERACCIÓN ──────────────────────────────────

    [Header("Player Pickup Script")]
    [Tooltip("Referencia al script que maneja la recolección de objetos.")]
    [SerializeField] 
    private PlayerPickup playerPickup;

    [Header("Punto de colocación del bowl")]
    [Tooltip("Posición donde se ancla el recipiente al colocarlo.")]
    [SerializeField] 
    private Transform bowlPoint;

    private bool playerLocked;          // Indica si el jugador está bloqueado.
    private Camera previousCamera;      // Guarda la cámara activa previa.

    [Header("Receta actual en la estación")]
    [Tooltip("Receta que se prepara en la estación.")]
    public RecipeData currentRecipe;

    // ─────────────────────────────── MÉTODOS PRINCIPALES ──────────────────────────

    /// <summary>
    /// Intenta recibir el bowl que el jugador sostiene y lo coloca en la estación.
    /// </summary>
    /// <param name="bowl">Objeto bowl que contiene un ReceivingContainer.</param>
    /// <returns>
    /// true si el bowl fue recibido correctamente; de lo contrario, false.
    /// </returns>
    public bool TryReceiveBowl(GameObject bowl)
    {
        if (receivingContainer != null || bowl == null) return false;

        var placed = bowl.GetComponent<ReceivingContainer>();
        if (placed == null) return false;

        receivingContainer = placed;

        // Bloquea la física y fija el recipiente al punto designado.
        var rb = placed.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // Anclar al punto de colocación.
        placed.transform.SetParent(bowlPoint);
        placed.transform.localPosition = Vector3.zero;
        placed.transform.localRotation = Quaternion.identity;

        // Asigna el recipiente a todos los PouringContainers activos.
        foreach (var pouring in Object.FindObjectsByType<PouringContainer>(
                     FindObjectsSortMode.None))
        {
            pouring.targetContainer = placed;
        }

        LockPlayer();
        return true;
    }

    /// <summary>
    /// Bloquea el movimiento del jugador y activa la UI y cámara de la estación.
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
    /// Desbloquea al jugador, desactiva la UI y restaura la cámara anterior.
    /// </summary>
    public void UnlockPlayer()
    {
        if (!playerLocked) return;
        playerLocked = false;

        ingredientPanel?.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraFollow != null) cameraFollow.enabled = true;

        if (cameraController != null && previousCamera != null)
        {
            cameraController.ActivateCamera(previousCamera);
        }

        // Devuelve el bowl al jugador si hay uno en la estación.
        if (receivingContainer != null)
        {
            var bowl = receivingContainer.gameObject;
            var rb = bowl.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;

            playerPickup.GrabObject(bowl);

            receivingContainer = null;

            if (activePouringContainer != null)
            {
                Destroy(activePouringContainer.gameObject);
                activePouringContainer = null;
            }
        }
    }

    // ─────────────────────────────── GETTERS / SETTERS ────────────────────────────

    /// <summary>
    /// Asigna un contenedor de vertido activo a la estación.
    /// </summary>
    /// <param name="container">Instancia del PouringContainer activo.</param>
    public void SetActivePouring(PouringContainer container)
    {
        activePouringContainer = container;
    }

    /// <summary>
    /// Obtiene el contenedor de vertido actualmente activo.
    /// </summary>
    /// <returns>Instancia activa de PouringContainer.</returns>
    public PouringContainer GetActivePouring()
    {
        return activePouringContainer;
    }

    /// <summary>
    /// Establece la receta actual asignada a la estación.
    /// </summary>
    /// <param name="recipe">Instancia de la receta a utilizar.</param>
    public void SetActiveRecipe(RecipeData recipe)
    {
        currentRecipe = recipe;
    }

    /// <summary>
    /// Obtiene la receta actualmente activa en la estación.
    /// </summary>
    /// <returns>Instancia actual de RecipeData.</returns>
    public RecipeData GetActiveRecipe()
    {
        return currentRecipe;
    }
}
