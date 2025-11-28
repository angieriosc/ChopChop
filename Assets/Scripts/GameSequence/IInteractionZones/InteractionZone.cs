using UnityEngine;
using TMPro;
public class InteractionZone : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEText;

    [SerializeField] private TMP_Text pressEMessage; 

    [SerializeField] private string messagge;

    public bool IsCusomerStation = false;

    public bool IsPouringStation = false;

    public bool IsMixerStation = false;

    public bool IsCuttingStation = false;

    public bool IsToppingStation = false;

    public bool IsOvenSation = false;

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
            ChangeMessage();
        if (!playerInside) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (station == null) return;
        if (!station.CanInteract()) return;

        station.Interact();

        if (pressEText != null)
            pressEText.SetActive(false);
    }

    void ChangeMessage()
    {
        if(IsCusomerStation)
            return;
        if(InputLock.IsLocked)
        {
            pressEMessage.text = "Primero toma la\norden del cliente";
        }
        else
        {
            PlayerPickup player = FindAnyObjectByType<PlayerPickup>();

            if (IsPouringStation)
            {
                // No trae nada en las manos
                if (player.pickedObject == null)
                {
                    pressEMessage.text = "Toma un recipiente\npara usar la estación";
                    return;
                }

                // Trae algo pero NO es un recipiente
                if (player.pickedObject.GetComponent<ReceivingContainer>() == null)
                {
                    pressEMessage.text = "Suelta el objeto con <color=#FFFF00>Q</color>\nNecesitas un recipiente";
                    return;
                }

                else
                {
                    pressEMessage.text=messagge;
                }
            }

            if (IsMixerStation)
            {
                // Trae algo pero NO es un recipiente
                if (player.pickedObject == null || player.pickedObject.GetComponent<MixableBowl>() == null)
                {
                    pressEMessage.text = "Necesitas un recipiente con masa";
                    return;
                }

                else
                {
                    pressEMessage.text=messagge;
                }
            }

            if(IsCuttingStation)
            {
                // Trae algo pero NO es un recipiente
                if (player.pickedObject == null || !player.pickedObject.GetComponent<InteractableObject>().HasCapability(ObjectCapabilities.Cuttable))
                {
                    pressEMessage.text = "Necesitas algo \nque se pueda cortar";
                    return;
                }

                else
                {
                    pressEMessage.text=messagge;
                }
            }

            if(IsToppingStation)
            {
                int DoughQuantity= CuttingInventory.Instance.GetQuantity("WedgeSlice");
                if ( DoughQuantity == 0)
                {
                    pressEMessage.text = "Se necesita \nmasa cortada";
                    return;                
                }
                else
                {
                    pressEMessage.text="Tienes <color=#FFFF00>"+DoughQuantity+"</color> masas \npresiona <color=#FFFF00>E</color> para\nhacer la pizza" ;
                }

                
            } 
            if(IsOvenSation)
            {
                // Trae algo pero NO es un recipiente
                if (player.pickedObject == null || !player.pickedObject.GetComponent<InteractableObject>().HasCapability(ObjectCapabilities.Bakeable))
                {
                    pressEMessage.text = "Solo se pueden hornear pizzas";
                    return;
                }

                else
                {
                    pressEMessage.text=messagge;
                }
            }

            else
            {
                if(!InputLock.IsLocked && pressEMessage.text!=messagge)
                {
                    pressEMessage.text=messagge;
                }
            }
        }

    }
}
