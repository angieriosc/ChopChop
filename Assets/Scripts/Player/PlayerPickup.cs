using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Permite al jugador recoger, soltar y colocar objetos en estaciones.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerPickup : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] public Transform handPoint;

    [Header("Teclas de interacción")]
    [SerializeField] private KeyCode grabKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.Q;

    [Header("Detección de estaciones")]
    [SerializeField] private float detectionRange = 3f;

    private GameObject pickedObject = null;
    private InteractableObject nearbyObject = null;

    // Referencias a estaciones cercanas
    private OvenStation nearbyOven = null;
    private MixerStation nearbyMixer = null;
    private ToppingStation nearbyToppingStation = null;
    private PouringStation nearbyPouringStation = null;
    private BowlStation nearbyBowlStation = null;
    private SlicingStation nearbySlicingStation = null;

    private PlayerCartController cartController = null;

    private void Awake()
    {
        cartController = GetComponent<PlayerCartController>();
    }

    private void Update()
    {
        if (DialogueLock.IsLocked) return;
        if (InputLock.IsLocked) return;
        if (InputCooldown.BlockNextE)
        if (InputCooldown.IsOnCooldown()) 
        {
            return;
        }

        if (InputCooldown.BlockNextE)
        {
            InputCooldown.BlockNextE = false;
            return;
        }

        DetectNearbyStations();
        HandleDropInput();
        HandleGrabInput();
    }

    private void DetectNearbyStations()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRange);
        
        // Reset flags
        bool ovenFound = false;
        bool mixerFound = false;
        bool pouringStationFound = false;
        bool toppingStationFound = false;
        bool bowlStationFound = false;
        bool slicingStationFound = false;

        foreach (var col in nearbyColliders)
        {
            // OVEN
            OvenStation oven = col.GetComponent<OvenStation>();
            if (oven != null) { nearbyOven = oven; ovenFound = true; }

            // MIXER
            MixerStation mixer = col.GetComponent<MixerStation>();
            if (mixer != null) { nearbyMixer = mixer; mixerFound = true; }

            // POURING
            PouringStation pouring = col.GetComponent<PouringStation>();
            if (pouring != null) { nearbyPouringStation = pouring; pouringStationFound = true; }

            // TOPPING
            ToppingStation topping = col.GetComponentInParent<ToppingStation>();
            if (topping != null) { nearbyToppingStation = topping; toppingStationFound = true; }

            // BOWL
            BowlStation bowl = col.GetComponent<BowlStation>();
            if (bowl != null) { nearbyBowlStation = bowl; bowlStationFound = true; }
            
            // SLICING
            SlicingStation slicer = col.GetComponent<SlicingStation>();
            if (slicer != null) { nearbySlicingStation = slicer; slicingStationFound = true; }
        }

        if (!ovenFound) nearbyOven = null;
        if (!mixerFound) nearbyMixer = null;
        if (!pouringStationFound) nearbyPouringStation = null;
        if (!toppingStationFound) nearbyToppingStation = null;
        if (!bowlStationFound) nearbyBowlStation = null;
        if (!slicingStationFound) nearbySlicingStation = null;
    }

    private void HandleDropInput()
    {
        if (pickedObject != null && Input.GetKeyDown(dropKey))
            DropObject();
    }

    private void HandleGrabInput()
    {
        if (!Input.GetKeyDown(grabKey)) return;

        if (cartController != null && cartController.HasCart())
        {
            Debug.Log("💡 No puedes agarrar objetos mientras empujas el carrito");
            return;
        }

        // Prioridad especial: Agarrar pizza del ToppingStation
        if (pickedObject == null)
        {
            if (nearbyToppingStation != null &&
                nearbyToppingStation.HasPizza() &&
                nearbyToppingStation.IsPlayerInside())
            {
                GameObject pizza = nearbyToppingStation.TakePizza();
                if (pizza != null) { GrabObject(pizza); return; }
            }
        }

        // Si tenemos objeto, intentamos colocarlo
        if (pickedObject != null)
        {
            if (!PlaceInStation()) Debug.Log("No nearby station or can't place this object.");
            return;
        }

        // Si no tenemos objeto, intentamos interactuar con estaciones vacías (ej. iniciar pizza)
        if (pickedObject == null && nearbyToppingStation != null && nearbyToppingStation.IsAvailable())
        {
            nearbyToppingStation.TryPlace();
            return;
        }

        // Intentar tomar algo de una estación
        if (TakeFromStation()) return;

        // Si nada de lo anterior, agarrar objeto del suelo/mesa
        if (pickedObject == null && nearbyObject != null)
        {
            float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
            if (distance < detectionRange)
            {
                GrabObject(nearbyObject.gameObject);
            }
        }
    }

    private bool PlaceInStation()
    {
        if (pickedObject == null) return false;
        var interactable = pickedObject.GetComponent<InteractableObject>();
        if (interactable == null) return false;

        if (nearbyOven != null && nearbyOven.IsAvailable() && interactable.HasCapability(ObjectCapabilities.Bakeable))
        {
            if (nearbyOven.PutInOven(pickedObject)) { pickedObject = null; nearbyObject = null; return true; }
        }

        if (nearbyMixer != null && nearbyMixer.IsAvailable() && interactable.HasCapability(ObjectCapabilities.Mixable))
        {
            if (nearbyMixer.PutBowlIn(pickedObject)) { pickedObject = null; return true; }
        }

        if (nearbyPouringStation != null && interactable.HasCapability(ObjectCapabilities.Pourable))
        {
            if (nearbyPouringStation.TryReceiveBowl(pickedObject)) { pickedObject = null; return true; }
        }
        
        if (nearbyToppingStation != null && nearbyToppingStation.IsAvailable() && interactable.HasCapability(ObjectCapabilities.Toppingable))
        {
            if (nearbyToppingStation.TryPlace()) return true;
        }
        
        if (nearbySlicingStation != null && interactable.HasCapability(ObjectCapabilities.Cuttable))
        {
            if (nearbySlicingStation.AssignItemToStation(pickedObject)) { pickedObject = null; return true; }
        }

        return false;
    }

    /// <summary>
    /// Intenta tomar un objeto de una estación cercana.
    /// </summary>
    private bool TakeFromStation()
    {
        if (nearbyOven != null && nearbyOven.HasIngredient())
        {
            GameObject ingredient = nearbyOven.TakeFromOven();
            if (ingredient != null) { GrabObject(ingredient); return true; }
        }

        if (nearbyMixer != null && !nearbyMixer.IsAvailable())
        {
            if (nearbyMixer.IsMixingInProgress) 
            {
                return false; 
            }

            GameObject bowl = nearbyMixer.TakeBowlOut();
            if (bowl != null) { GrabObject(bowl); return true; }
        }

        if (nearbyToppingStation != null && nearbyToppingStation.HasPizza() && nearbyToppingStation.IsPlayerInside())
        {
            GameObject pizza = nearbyToppingStation.TakePizza();
            if (pizza != null) { GrabObject(pizza); return true; }
        }

        if (nearbyBowlStation != null && pickedObject == null)
        {
            if (nearbyBowlStation.activeBowl == null)
            {
                nearbyBowlStation.TrySpawnBowl();
                GrabObject(nearbyBowlStation.activeBowl);
                nearbyBowlStation.activeBowl = null;
                nearbyBowlStation.TrySpawnBowl();
            }
            else
            {
               GrabObject(nearbyBowlStation.activeBowl);
               nearbyBowlStation.activeBowl = null; 
               nearbyBowlStation.TrySpawnBowl();
            }
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<InteractableObject>();
        if (interactable != null && interactable.HasCapability(ObjectCapabilities.Grabbable))
            nearbyObject = interactable;
    }

    private void OnTriggerExit(Collider other)
    {
        if (nearbyObject == null) return;
        float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
        if (distance > detectionRange) nearbyObject = null;
    }

    public void GrabObject(GameObject obj)
    {
        var interactable = obj.GetComponentInParent<InteractableObject>() ?? obj.GetComponentInChildren<InteractableObject>();
        if (interactable == null || !interactable.HasCapability(ObjectCapabilities.Grabbable)) return;

        pickedObject = obj;
        nearbyObject = null;

        var rb = pickedObject.GetComponent<Rigidbody>();
        if (rb != null) { rb.useGravity = false; rb.isKinematic = true; }

        var col = pickedObject.GetComponent<Collider>();
        if (col != null && !col.enabled) col.enabled = true;

        pickedObject.transform.SetParent(handPoint, true);
        pickedObject.transform.localPosition = Vector3.zero;
        pickedObject.transform.localRotation = Quaternion.identity;
    }

    public void DropObject()
    {
        if (pickedObject == null) return;

        var interactable = pickedObject.GetComponent<InteractableObject>();
        if (interactable != null && !interactable.HasCapability(ObjectCapabilities.Droppable)) return;

        pickedObject.transform.SetParent(null);
        pickedObject.transform.position = transform.position + transform.forward * 1f;

        var rb = pickedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        pickedObject = null;
    }

    public bool HasObjectInHand() => pickedObject != null;
    public GameObject GetObjectInHand() => pickedObject;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}