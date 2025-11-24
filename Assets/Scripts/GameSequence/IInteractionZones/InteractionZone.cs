using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEText;

    private bool playerInside = false;

    private IInteractableStation station;

    void Awake()
    {
        // Buscar la estación implementando la interfaz
        station = GetComponent<IInteractableStation>();
        if (station == null)
            Debug.LogWarning($"InteractionZone en {name} no tiene una estación que implemente IInteractableStation.");
    }

    void Start()
    {
        if (pressEText != null)
            pressEText.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        DialogueSequenceRunner pointer = FindFirstObjectByType<DialogueSequenceRunner>(); 
        
        pointer.DestroyPointer();

        if (pressEText != null && station != null && station.CanInteract())
            pressEText.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (pressEText != null)
            pressEText.SetActive(false);
    }

    void Update()
    {
        if (!playerInside) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (station == null) return;
        if (!station.CanInteract()) return;

        station.Interact();

        if (pressEText != null)
            pressEText.SetActive(false);
    }
}
