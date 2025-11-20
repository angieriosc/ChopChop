using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEText;

    [Header("Customer que está esperando")]
    public Customer currentCustomer;   // Se asigna cuando el cliente llega

    private bool playerInside = false;

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


        // ⛔ NO mostrar si no hay cliente
        if (currentCustomer != null && pressEText != null)
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            // ⛔ No pasa nada si no hay cliente
            if (currentCustomer == null) return;

            currentCustomer.OnInteract();

            if (pressEText != null)
                pressEText.SetActive(false);
        }
    }

    // Llamado por el cliente cuando llega
    public void SetCustomer(Customer customer)
    {
        currentCustomer = customer;

        // Si el jugador ya está dentro del trigger → mostrar texto
        if (playerInside && pressEText != null)
            pressEText.SetActive(true);
    }

    // Llamado cuando el cliente se va
    public void ClearCustomer()
    {
        currentCustomer = null;

        if (pressEText != null)
            pressEText.SetActive(false);
    }
}
