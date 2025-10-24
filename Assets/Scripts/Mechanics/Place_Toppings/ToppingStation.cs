using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Estación de toppings para montar pizzas:
/// - Activa cámara de estación y bloquea al jugador.
/// - Coloca la masa en un "masaPoint" centrado en la mesa.
/// - Habilita el PizzaToppingManager para arrastrar/soltar toppings.
/// - Permite guardar receta/prefab y salir devolviendo la pizza.
/// Controles por defecto: E (entrar), F (guardar), Q (salir).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ToppingStation : MonoBehaviour
{
    [Header("Cámara de estación")]
    [SerializeField] private Camera stationCamera;

    [Header("Jugador y controladores")]
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerMovement playerMovement;   // tu script de movimiento
    [SerializeField] private FollowPlayer cameraFollow;       // tu cámara follow (si aplica)
    [SerializeField] private CameraController cameraController;

    [Header("UI (opcional)")]
    [SerializeField] private GameObject toppingsPanel;        // panel con botones de guardar/salir, etc.

    [Header("Puntos de colocación")]
    [SerializeField] private Transform masaPoint;             // <--- centra aquí la masa
    [SerializeField] private Vector3 masaOffset = new Vector3(0, 0.02f, 0);
    [SerializeField] private Transform dropPoint;             // dónde dejar la pizza al salir (si no hay inventario)

    [Header("Detección de masa si no la pasas por código")]
    [SerializeField] private string pizzaTag = "PizzaBase";   // tag de tu masa
    [SerializeField] private float enterRadius = 2.5f;        // radio visual/ayuda en gizmos

    [Header("Toppings Manager")]
    [SerializeField] private PizzaToppingManager toppingManager;
    [SerializeField] private LayerMask pizzaSurfaceMask = ~0; // capa de superficie (asignada tb en manager)
    [SerializeField] private LayerMask draggableMask = ~0;    // capa de toppings arrastrables

    [Header("Teclas")]
    [SerializeField] private Key enterKey = Key.E;
    [SerializeField] private Key saveKey  = Key.F;
    [SerializeField] private Key exitKey  = Key.Q;

    // --- Estado interno ---
    private bool _playerNearby = false;
    private bool _locked = false;
    private Camera _previousCamera;

    // Pizza actual montándose
    private GameObject _currentPizza;
    private Transform _originalParent;
    private Vector3 _originalPos;
    private Quaternion _originalRot;
    private Rigidbody _pizzaRb;
    private List<Collider> _pizzaCols = new();

    private void Start()
    {
        if (toppingsPanel != null) toppingsPanel.SetActive(false);

        // Sanea manager si está asignado
        if (toppingManager != null)
        {
            // Asegura cámara y masks del manager:
            if (stationCamera != null) toppingManager.workCamera = stationCamera;
            toppingManager.surfaceMask  = pizzaSurfaceMask;
            toppingManager.draggableMask = draggableMask;
            // El pizzaRoot/pizzaSurfaceCollider se asignan al entrar cuando ya conocemos la masa real.
            toppingManager.EnablePlacement(false);
        }
    }

    private void Update()
    {
        if (_locked)
        {
            // Guardar (F)
            if (Keyboard.current != null && Keyboard.current[saveKey].wasPressedThisFrame)
                SaveCurrentPizza();

            // Salir (Q)
            if (Keyboard.current != null && Keyboard.current[exitKey].wasPressedThisFrame)
                UnlockAndExit();
            return;
        }

        if (_playerNearby && Keyboard.current != null && Keyboard.current[enterKey].wasPressedThisFrame)
        {
            // Si no tenemos referencia directa, intenta detectar una pizza por tag en hijos del player
            if (_currentPizza == null && !string.IsNullOrEmpty(pizzaTag))
            {
                var found = FindPizzaOnPlayer();
                if (found != null) _currentPizza = found;
            }
            LockAndEnter(_currentPizza);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player) _playerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            _playerNearby = false;
            if (_locked) UnlockAndExit();
        }
    }

    /// <summary>
    /// Entrada principal para “entregar” una pizza desde código externo (inventario, etc).
    /// Llama esto antes de presionar E o directamente llama LockAndEnter(pizza).
    /// </summary>
    public void SetIncomingPizza(GameObject pizza)
    {
        _currentPizza = pizza;
    }

    /// <summary>
    /// Bloquea jugador, cambia cámara y coloca la masa en masaPoint.
    /// </summary>
    public void LockAndEnter(GameObject pizza)
    {
        if (_locked) return;
        if (pizza == null)
        {
            Debug.LogWarning("[ToppingStation] No hay masa/pizza para montar toppings.");
            return;
        }
        _locked = true;

        if (toppingsPanel != null) toppingsPanel.SetActive(true);

        // Bloquea control de jugador
        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraFollow   != null) cameraFollow.enabled   = false;

        // Cambia a cámara de estación
        if (cameraController != null && stationCamera != null)
        {
            _previousCamera = cameraController.GetActiveCamera();
            cameraController.ActivateCamera(stationCamera);
        }

        // Colocar pizza en masaPoint
        PlacePizzaOnMasaPoint(pizza);

        // Configurar manager para este pizzaRoot real
        if (toppingManager != null)
        {
            toppingManager.workCamera = stationCamera;
            toppingManager.pizzaRoot  = _currentPizza.transform;

            // Busca collider de superficie; si no hay, usa cualquiera sobre la pizza
            var surface = _currentPizza.GetComponentInChildren<Collider>();
            toppingManager.pizzaSurfaceCollider = surface;
            toppingManager.pizzaCenter = _currentPizza.transform;

            toppingManager.EnablePlacement(true);
        }

        Debug.Log("[ToppingStation] Entraste a la estación de toppings. Arrastra con el mouse; F = guardar, Q = salir.");
    }

    /// <summary>
    /// Guarda receta y “hornea” un Prefab en Editor (F).
    /// </summary>
    public void SaveCurrentPizza()
    {
        if (_currentPizza == null || toppingManager == null)
        {
            Debug.LogWarning("[ToppingStation] No hay pizza o manager para guardar.");
            return;
        }

        toppingManager.SaveAssembly(alsoMakeEditorPrefab: true);
        Debug.Log("[ToppingStation] Receta/Pizza guardada (ver Console para rutas).");
    }

    /// <summary>
    /// Restaura controles/cámara, desactiva modo toppings y suelta/retorna la pizza.
    /// </summary>
    public void UnlockAndExit()
    {
        if (!_locked) return;
        _locked = false;

        if (toppingsPanel != null) toppingsPanel.SetActive(false);

        // Deshabilita placement
        if (toppingManager != null) toppingManager.EnablePlacement(false);

        // Restaura cámara previa
        if (cameraController != null && _previousCamera != null)
            cameraController.ActivateCamera(_previousCamera);

        // Restaura control del jugador
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraFollow   != null) cameraFollow.enabled   = true;

        // Suelta/coloca pizza en dropPoint o regresa a su parent original
        if (_currentPizza != null)
        {
            ReleasePizza();
        }

        Debug.Log("[ToppingStation] Saliste de la estación de toppings.");
    }

    // -----------------------
    // Auxiliares de Pizza
    // -----------------------

    private void PlacePizzaOnMasaPoint(GameObject pizza)
    {
        _currentPizza = pizza;

        _originalParent = pizza.transform.parent;
        _originalPos    = pizza.transform.position;
        _originalRot    = pizza.transform.rotation;

        // Físicas/colisiones
        _pizzaRb = pizza.GetComponent<Rigidbody>();
        if (_pizzaRb != null)
        {
            _pizzaRb.linearVelocity  = Vector3.zero;
            _pizzaRb.angularVelocity = Vector3.zero;
            _pizzaRb.useGravity = false;
            _pizzaRb.isKinematic = true;
        }

        _pizzaCols.Clear();
        pizza.GetComponentsInChildren(true, _pizzaCols);
        foreach (var col in _pizzaCols) col.enabled = true; // mantenlos activos para raycasts sobre la superficie

        // Parenteo y colocación exacta
        pizza.transform.SetParent(masaPoint != null ? masaPoint : transform, worldPositionStays: true);
        pizza.transform.localPosition = masaOffset;
        pizza.transform.localRotation = Quaternion.identity;
    }

    private void ReleasePizza()
    {
        // Destino al salir
        Transform target = dropPoint != null ? dropPoint : _originalParent;

        // Quitar parent y posicionar
        _currentPizza.transform.SetParent(null, true);
        if (target != null)
        {
            _currentPizza.transform.position = target.position;
            _currentPizza.transform.rotation = target.rotation;
        }
        else
        {
            // Si no hay destino, vuelve a su posición original
            _currentPizza.transform.position = _originalPos;
            _currentPizza.transform.rotation = _originalRot;
        }

        // Restaurar físicas
        if (_pizzaRb != null)
        {
            _pizzaRb.isKinematic = false;
            _pizzaRb.useGravity = true;
        }

        _currentPizza = null;
        _originalParent = null;
        _pizzaCols.Clear();
        _pizzaRb = null;
    }

    private GameObject FindPizzaOnPlayer()
    {
        if (player == null) return null;
        var pizzas = player.GetComponentsInChildren<Transform>(true);
        foreach (var t in pizzas)
        {
            if (t.CompareTag(pizzaTag)) return t.gameObject;
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        // Radio para referencia visual
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enterRadius);

        // MasaPoint
        if (masaPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(masaPoint.position + masaOffset, Vector3.one * 0.2f);
        }

        if (dropPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(dropPoint.position, Vector3.one * 0.2f);
        }
    }
}
