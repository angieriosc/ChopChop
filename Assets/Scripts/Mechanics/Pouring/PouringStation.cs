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

    [Header("Recipiente activo en la estación")]
    [SerializeField] public PouringContainer activePouringContainer;

    [Header("Player Pickup Script")]
    [SerializeField] private PlayerPickup playerPickup; // Referencia al script PlayerPickup

    [Header("Punto de colocación del bowl")]
    [SerializeField] private Transform bowlPoint;

    private bool playerLocked;         // Indica si el jugador está bloqueado
    private Camera previousCamera;     // Cámara previa antes de entrar a la estación

    [Header("Receta actual en la estación")]
    public RecipeData currentRecipe;



    /*

    /// <summary>
    /// Detecta cuando un recipiente entra en la estación.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        ReceivingContainer placed = other.GetComponent<ReceivingContainer>();
        if (placed == null) return;

        receivingContainer = placed;

        Rigidbody rb = placed.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll; // Bloquea todo movimiento
        }


        // Asignar targetContainer a todos los PouringContainer activos
        foreach (var pouring in Object.FindObjectsByType<PouringContainer>(
                     FindObjectsSortMode.None))
        {
            pouring.targetContainer = placed;
        }

        LockPlayer();


    }
    */

    /// <summary>
    /// Intenta recibir el bowl que el jugador tiene en la mano.
    /// </summary>
    public bool TryReceiveBowl(GameObject bowl)
    {
        if (receivingContainer != null || bowl == null) return false;

        ReceivingContainer placed = bowl.GetComponent<ReceivingContainer>();
        if (placed == null) return false;

        receivingContainer = placed;

        // Bloquear física y anclar al bowlPoint
        Rigidbody rb = placed.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // Colocar en el bowlPoint
        placed.transform.SetParent(bowlPoint);
        placed.transform.localPosition = Vector3.zero;
        placed.transform.localRotation = Quaternion.identity;

        // Asignar targetContainer a todos los PouringContainer activos
        foreach (var pouring in Object.FindObjectsByType<PouringContainer>(FindObjectsSortMode.None))
        {
            pouring.targetContainer = placed;
        }
        

        LockPlayer();
        return true;
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

        // Si hay un bowl en la estación, pasarlo a la mano del jugador
        if (receivingContainer != null)
        {
            GameObject bowl = receivingContainer.gameObject;
            Rigidbody rb = bowl.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None; // Desbloquea todo movimiento


            playerPickup.GrabObject(bowl);

            receivingContainer = null;
            if (activePouringContainer != null)
            {
                Destroy(activePouringContainer.gameObject);
                activePouringContainer = null;
            }
        }
    }


    public void SetActivePouring(PouringContainer container)
    {
        activePouringContainer = container;
    }

    public PouringContainer GetActivePouring()
    {
        return activePouringContainer;
    }

    public void SetActiveRecipe(RecipeData recipe)
    {
        currentRecipe = recipe;
    }

    public RecipeData GetActiveRecipe()
    {
        return currentRecipe;
    }

}
