using UnityEngine;
using TMPro;

public class InteractionZone : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEText;
    [SerializeField] private TMP_Text pressEMessage;
    [SerializeField] private string messagge;

    [Header("Station Flags")]
    public bool IsCusomerStation = false;
    public bool IsPouringStation = false;
    public bool IsMixerStation = false;
    public bool IsCuttingStation = false;
    public bool IsToppingStation = false;
    public bool IsOvenSation = false;

    private bool playerInside = false;

    private IInteractableStation station;
    private PlayerPickup player;   // ← referencia segura

    void Awake()
    {
        station = GetComponent<IInteractableStation>();
        if (station == null)
            Debug.LogWarning($"InteractionZone en {name} no tiene una estación que implemente IInteractableStation.");
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerPickup>();

        if (pressEMessage != null)
        {
            pressEText.SetActive(false);
            pressEMessage.text = messagge;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        DialogueSequenceRunner pointer = FindFirstObjectByType<DialogueSequenceRunner>();
        if (pointer != null) pointer.DestroyPointer();

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
        ChangeMessage();

        if (!playerInside) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (station == null) return;
        if (!station.CanInteract()) return;

        station.Interact();

        if (pressEText != null)
            pressEText.SetActive(false);
    }

    // -------------------------------------------------------
    //              CHANGE MESSAGE (WEBGL SAFE)
    // -------------------------------------------------------

    void ChangeMessage()
    {
        if (IsCusomerStation) return;

        // Si la entrada está bloqueada
        if (InputLock.IsLocked)
        {
            pressEMessage.text = "Primero toma la\norden del cliente";
            return;
        }

        // Si no existe el Player, no crashear
        if (player == null)
        {
            pressEMessage.text = messagge;
            return;
        }

        var held = player.pickedObject; // seguro y rápido de usar

        // ---------------------------
        //       POURING
        // ---------------------------
        if (IsPouringStation)
        {
            if (held == null)
            {
                pressEMessage.text =
                    "Toma un recipiente\npara usar la estación";
                return;
            }

            if (held.GetComponent<ReceivingContainer>() == null)
            {
                pressEMessage.text =
                    "Suelta el objeto con <color=#FFFF00>Q</color>\nNecesitas un recipiente";
                return;
            }

            pressEMessage.text = messagge;
            return;
        }

        // ---------------------------
        //        MIXER
        // ---------------------------
        if (IsMixerStation)
        {
            if (held == null ||
                held.GetComponent<MixableBowl>() == null)
            {
                pressEMessage.text =
                    "Necesitas un recipiente con masa";
                return;
            }

            pressEMessage.text = messagge;
            return;
        }

        // ---------------------------
        //       CUTTING
        // ---------------------------
        if (IsCuttingStation)
        {
            var interact = held?.GetComponent<InteractableObject>();

            if (held == null ||
                interact == null ||
                !interact.HasCapability(ObjectCapabilities.Cuttable))
            {
                pressEMessage.text =
                    "Necesitas algo\nque se pueda cortar";
                return;
            }

            pressEMessage.text = messagge;
            return;
        }

        // ---------------------------
        //       TOPPING
        // ---------------------------
        if (IsToppingStation)
        {
            int q = CuttingInventory.Instance.GetQuantity("WedgeSlice");

            if (q == 0)
            {
                pressEMessage.text =
                    "Se necesita \nmasa cortada";
                return;
            }

            pressEMessage.text =
                $"Tienes <color=#FFFF00>{q}</color> masas\n" +
                "presiona <color=#FFFF00>E</color> para\nhacer la pizza";
            return;
        }

        // ---------------------------
        //         OVEN
        // ---------------------------
        if (IsOvenSation)
        {
            var interact = held?.GetComponent<InteractableObject>();

            if (held == null ||
                interact == null ||
                !interact.HasCapability(ObjectCapabilities.Bakeable))
            {
                pressEMessage.text =
                    "Solo se pueden hornear pizzas";
                return;
            }

            pressEMessage.text = messagge;
            return;
        }

        // ---------------------------
        //       DEFAULT
        // ---------------------------
        pressEMessage.text = messagge;
    }
}
