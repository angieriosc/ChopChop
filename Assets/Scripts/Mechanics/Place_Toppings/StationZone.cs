using UnityEngine;
using UnityEngine.InputSystem;


public class StationZone : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera playerCamera;
    public Camera stationCamera;

    [Header("UI estación")]
    public GameObject stationCanvas; // Canvas con tus botones 1..6

    [Header("Manager de toppings")]
    public PizzaToppingManager toppingManager;

    [Header("Centro de la mesa (dónde va la masa)")]
    public Transform pizzaCenter; // un Empty en el centro de la mesa

    [Header("Detección de la masa")]
    public string doughTag = "PizzaDough";  // pon esta tag a tu masa
    public float snapYOffset = 0.005f;      // pequeño offset al centrar

    private Collider _lastDough;   // masa que entró al trigger
    private bool _playerInside;
    private bool _inStation;       // estado actual (dentro o fuera de estación)

    private void Awake()
    {
        SetStationActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(doughTag))
        {
            _lastDough = other;
            Debug.Log("[Station] Masa detectada dentro del trigger");
        }

        _playerInside = true;
        // (Opcional) muestra un cartel “E para entrar”
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _lastDough) _lastDough = null;

        _playerInside = false;
        if (_inStation)
            ExitStation();
    }

    private void Update()
    {
        if (!_playerInside) return;

        // Presiona E para entrar
        if (!_inStation && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            EnterStation();
        }
        // Presiona E de nuevo para salir
        else if (_inStation && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExitStation();
        }
    }

    private void EnterStation()
    {
        if (stationCamera == null || stationCanvas == null || toppingManager == null)
        {
            Debug.LogError("[Station] Faltan referencias en el inspector");
            return;
        }

        _inStation = true;
        SetStationActive(true);

        // Si hay masa dentro del trigger, la centramos en la mesa
        if (_lastDough != null && pizzaCenter != null)
        {
            var t = _lastDough.transform;
            t.position = pizzaCenter.position + Vector3.up * snapYOffset;
            // mantén la orientación del jugador o alínea a la mesa:
            t.rotation = Quaternion.Euler(0f, pizzaCenter.eulerAngles.y, 0f);
            Debug.Log("[Station] Masa colocada en el centro de la mesa");
        }

        // Conectar el manager con cámara y componentes de la masa
        if (_lastDough != null)
        {
            var identifiers = _lastDough.GetComponentInParent<PizzaIdentifiers>();
            if (identifiers != null)
            {
                toppingManager.pizzaRoot           = identifiers.pizzaRoot;
                toppingManager.pizzaSurfaceCollider= identifiers.pizzaSurface;
                toppingManager.pizzaCenter         = identifiers.pizzaCenter != null ? identifiers.pizzaCenter : identifiers.pizzaRoot;
                Debug.Log("[Station] Referencias de pizza asignadas desde PizzaIdentifiers");
            }
            else
            {
                Debug.LogWarning("[Station] La masa no tiene PizzaIdentifiers; arrastra manualmente pizzaRoot/surface/center al manager si es necesario.");
            }
        }

        // Activar el manager (esto permite el spawn con click)
        toppingManager.workCamera = stationCamera;
        toppingManager.SetEnabled(true);
    }

    private void ExitStation()
    {
        _inStation = false;
        SetStationActive(false);

        if (toppingManager != null)
        {
            toppingManager.SetEnabled(false);
            toppingManager.workCamera = null;
            toppingManager.ClearSelection();
        }
    }

    private void SetStationActive(bool active)
    {
        if (playerCamera)  playerCamera.gameObject.SetActive(!active);
        if (stationCamera) stationCamera.gameObject.SetActive(active);
        if (stationCanvas) stationCanvas.SetActive(active);
    }
}
