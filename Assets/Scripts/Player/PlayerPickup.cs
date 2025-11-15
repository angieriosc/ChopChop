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

    // Referencia al controlador del carrito
    private PlayerCartController cartController = null;

    private void Awake()
    {
        cartController = GetComponent<PlayerCartController>();
    }

    private BowlStation nearbyBowlStation = null;
    private SlicingStation nearbySlicingStation = null; // Re-añadido

    private void Update()
    {
        DetectNearbyStations();

        HandleDropInput();
        HandleGrabInput();
    }

    /// <summary>
    /// Detecta estaciones cercanas dentro del rango.
    /// </summary>
    private void DetectNearbyStations()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRange);
        bool ovenFound = false;
        bool mixerFound = false;
        bool pouringStationFound = false;
        bool toppingStationFound = false;
        bool bowlStationFound = false;
        bool slicingStationFound = false; // Re-añadido

        foreach (var col in nearbyColliders)
        {
            OvenStation oven = col.GetComponent<OvenStation>();
            if (oven != null)
            {
                nearbyOven = oven;
                ovenFound = true;
            }

            MixerStation mixer = col.GetComponent<MixerStation>();
            if (mixer != null)
            {
                nearbyMixer = mixer;
                mixerFound = true;
            }

            PouringStation pouringStation = col.GetComponent<PouringStation>();
            if (pouringStation != null)
            {
                nearbyPouringStation = pouringStation;
                pouringStationFound = true;
            }
            ToppingStation toppingStation = col.GetComponentInParent<ToppingStation>();
            if (toppingStation != null)
            {
                nearbyToppingStation = toppingStation;
                toppingStationFound = true;
            }
            BowlStation bowlStation = col.GetComponent<BowlStation>();
            if (bowlStation != null)
            {
                nearbyBowlStation = bowlStation;
                bowlStationFound = true;
            }
            
            // --- LÓGICA RE-AÑADIDA ---
            SlicingStation slicer = col.GetComponent<SlicingStation>();
            if (slicer != null)
            {
                nearbySlicingStation = slicer;
                slicingStationFound = true;
            }
            // --- FIN ---
        }

        if (!ovenFound) nearbyOven = null;
        if (!mixerFound) nearbyMixer = null;
        if (!pouringStationFound) nearbyPouringStation = null;
        if (!toppingStationFound) nearbyToppingStation = null;
        if (!bowlStationFound) nearbyBowlStation = null;
        if (!slicingStationFound) nearbySlicingStation = null; // Re-añadido
    }

    /// <summary>
    /// Maneja la tecla de soltar objeto.
    /// </summary>
    private void HandleDropInput()
    {
        if (pickedObject != null && Input.GetKeyDown(dropKey))
            DropObject();
    }
    /// <summary>
    /// Maneja la tecla de agarrar o interactuar.
    /// </summary>
    private void HandleGrabInput()
    {
        if (!Input.GetKeyDown(grabKey)) return;

        // Si el jugador tiene el carrito, no puede agarrar objetos normales
        if (cartController != null && cartController.HasCart())
        {
            Debug.Log("💡 No puedes agarrar objetos mientras empujas el carrito");
            return;
        }

        // ----------------------------------------
        // 1. MANO VACÍA → SALIR DE TOPPING STATION
        // ----------------------------------------
        if (pickedObject == null)
        {
            if (nearbyToppingStation != null &&
                nearbyToppingStation.HasPizza() &&
                nearbyToppingStation.IsPlayerInside())
            {
                GameObject pizza = nearbyToppingStation.TakePizza();
                if (pizza != null)
                {
                    GrabObject(pizza);
                    return; // <- YA AGARRÓ LA PIZZA
                }
            }
        }

        // ----------------------------------------
        // 2. MANO LLENA → INTENTAR COLOCAR EN ESTACIONES
        // ----------------------------------------
        if (pickedObject != null)
        {
            if (!PlaceInStation())
                Debug.Log("No nearby station or can't place this object.");
            return;
        }

        // ----------------------------------------
        // 3. MANO VACÍA → ENTRAR A TOPPING STATION
        // ----------------------------------------
        if (pickedObject == null &&
            nearbyToppingStation != null &&
            nearbyToppingStation.IsAvailable())
        {
            nearbyToppingStation.TryPlace();
            return;
        }

        // ----------------------------------------
        // 4. MANO VACÍA → OTRAS ESTACIONES
        // ----------------------------------------
        if (TakeFromStation()) return;

        // ----------------------------------------
        // 5. MANO VACÍA → AGARRAR OBJETO DEL MUNDO
        // ----------------------------------------
        if (pickedObject == null && nearbyObject != null)
        {
            float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
            if (distance < detectionRange)
            {
                GrabObject(nearbyObject.gameObject);
            }
        }
    }


    /// <summary>
    /// Intenta colocar el objeto en una estación cercana.
    /// </summary>
    private bool PlaceInStation()
    {
        if (pickedObject == null) return false;

        var interactable = pickedObject.GetComponent<InteractableObject>();
        if (interactable == null) return false;

        if (nearbyOven != null && nearbyOven.IsAvailable() &&
            interactable.HasCapability(ObjectCapabilities.Bakeable))
        {
            if (nearbyOven.PutInOven(pickedObject))
            {
                pickedObject = null;
                nearbyObject = null; 
                return true;
            }
        }

        if (nearbyMixer != null && nearbyMixer.IsAvailable() &&
            interactable.HasCapability(ObjectCapabilities.Mixable))
        {
            if (nearbyMixer.PutBowlIn(pickedObject))
            {
                pickedObject = null;
                return true;
            }
        }

        if (nearbyPouringStation != null &&
            interactable.HasCapability(ObjectCapabilities.Pourable))
        {
            if (nearbyPouringStation.TryReceiveBowl(pickedObject))
            {
                pickedObject = null;
                return true;
            }
        }
        
        if (nearbyToppingStation != null && nearbyToppingStation.IsAvailable() &&
            interactable.HasCapability(ObjectCapabilities.Toppingable))
        {
            if (nearbyToppingStation.TryPlace())
            {
                return true;
            }
        }
        
        // --- LÓGICA RE-AÑADIDA ---
        if (nearbySlicingStation != null &&
            interactable.HasCapability(ObjectCapabilities.Cuttable))
        {
            if (nearbySlicingStation.AssignItemToStation(pickedObject))
            {
                pickedObject = null;
                return true;
            }
        }
        // --- FIN ---

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
            if (ingredient != null)
            {
                GrabObject(ingredient);
                return true;
            }
        }

        if (nearbyMixer != null && !nearbyMixer.IsAvailable())
        {
            GameObject bowl = nearbyMixer.TakeBowlOut();
            if (bowl != null)
            {
                GrabObject(bowl);
                return true;
            }
        }

        if (nearbyToppingStation != null &&
            nearbyToppingStation.HasPizza() &&
            nearbyToppingStation.IsPlayerInside())
        {
            GameObject pizza = nearbyToppingStation.TakePizza();
            if (pizza != null)
            {
                GrabObject(pizza);
                return true;
            }
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
        }


        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<InteractableObject>();
        if (interactable != null &&
            interactable.HasCapability(ObjectCapabilities.Grabbable))
        {
            nearbyObject = interactable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (nearbyObject == null) return;
        float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
        if (distance > detectionRange)
        {
            nearbyObject = null;
        }
    }

    /// <summary>
    /// Agarra un objeto y lo asigna al handPoint.
    /// </summary>
    public void GrabObject(GameObject obj)
    {
        var interactable = obj.GetComponentInParent<InteractableObject>() ??
                          obj.GetComponentInChildren<InteractableObject>();
        if (interactable == null ||
            !interactable.HasCapability(ObjectCapabilities.Grabbable))
            return;

        pickedObject = obj;
        nearbyObject = null;

        Vector3 originalScale = pickedObject.transform.localScale;

        var rb = pickedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        var col = pickedObject.GetComponent<Collider>();
        if (col != null && !col.enabled) col.enabled = true;

        pickedObject.transform.SetParent(handPoint, true);
        pickedObject.transform.localPosition = Vector3.zero;
        pickedObject.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Suelta el objeto actual.
    /// </summary>
    public void DropObject()
    {
        if (pickedObject == null) return;

        var interactable = pickedObject.GetComponent<InteractableObject>();
        if (interactable != null &&
            !interactable.HasCapability(ObjectCapabilities.Droppable))
            return;

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

    // -----------------------------
    // Métodos públicos
    // -----------------------------
    public bool HasObjectInHand() => pickedObject != null;
    public GameObject GetObjectInHand() => pickedObject;

    // -----------------------------
    // Visualización
    // -----------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}