using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manager para agarrar toppings (instanciados desde ToppingSource),
/// arrastrarlos con el mouse sobre la base de pizza y soltarlos.
/// También permite "hornear/guardar" la pizza como receta (runtime)
/// y como Prefab (Editor).
/// </summary>
public class PizzaToppingManager : MonoBehaviour
{
    [Header("Cámara de trabajo (misma que stationCamera)")]
    public Camera workCamera;

    [Header("Base de pizza")]
    public Transform pizzaRoot;         // Nodo padre de la pizza armada
    public Collider pizzaSurfaceCollider; // Un collider (Mesh/Box) sobre la pizza
    public float surfaceYOffset = 0.01f;  // Pequeño offset para que no se “hunda”

    [Header("Restricciones")]
    public float pizzaRadius = 0.3f;      // Radio en metros desde el centro de la pizzaRoot
    public Transform pizzaCenter;         // Centro lógico (usa pizzaRoot si está vacío)
    public LayerMask draggableMask;       // Capa para raycast de toppings ya instanciados
    public LayerMask surfaceMask;         // Capa para la superficie de la pizza

    [Header("Arrastre")]
    public float followSpeed = 20f;       // Suavizado del drag
    public bool snapToSurfaceNormal = false; // Si true, rota el topping según normal

    [Header("Receta / Guardado")]
    public string recipeAssetName = "PizzaRecipe_Runtime";
    public bool includeScaleInRecipe = true;

    private Transform _dragging;
    private Vector3 _dragOffset; // offset local para soltar natural
    private bool _enabled;

    private readonly List<Transform> _placed = new();

    public void EnablePlacement(bool enable)
    {
        _enabled = enable;
        if (!enable && _dragging != null)
        {
            // Forzar soltar si se sale del modo
            ReleaseCurrent(true);
        }
    }

    private void Update()
    {
        if (!_enabled || workCamera == null) return;

        // Iniciar drag con Click Izq si golpea un topping ya instanciado
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryStartDragFromScene();
        }

        // Mover mientras se mantiene el drag
        if (_dragging != null)
        {
            UpdateDrag();
            // Soltar con click izquierdo otra vez
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                TryReleaseOnPizza();
            }
            // Cancelar con click derecho
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ReleaseCurrent(false); // descartar
            }
        }
    }

    /// <summary>
    /// Usado por ToppingSource cuando crea un topping nuevo para arrastrar de inmediato.
    /// </summary>
    public void BeginDrag(Transform newTopping)
    {
        if (!_enabled || newTopping == null) return;
        _dragging = newTopping;
        _dragOffset = Vector3.zero;
        // Ponerlo como hijo temporal del manager para moverlo libremente
        _dragging.SetParent(transform, true);
    }

    private void TryStartDragFromScene()
    {
        Ray r = workCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(r, out RaycastHit hit, 100f, draggableMask))
        {
            // Solo si el objeto pertenece a nuestra pizzaRoot (seguridad)
            if (hit.transform != null && hit.transform != pizzaRoot && hit.transform.IsChildOf(pizzaRoot))
            {
                _dragging = hit.transform;
                _dragOffset = _dragging.position - hit.point;
                _dragging.SetParent(transform, true);
            }
        }
    }

    private void UpdateDrag()
    {
        Ray r = workCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 target;

        // Intentar proyectar sobre la superficie de pizza
        if (Physics.Raycast(r, out RaycastHit hit, 100f, surfaceMask))
        {
            target = hit.point + hit.normal * surfaceYOffset;

            if (snapToSurfaceNormal)
            {
                _dragging.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(_dragging.forward, hit.normal), hit.normal);
            }
        }
        else
        {
            // Si no golpea la superficie, proyectar a una distancia fija frente a la cámara
            target = r.GetPoint(0.7f);
        }

        _dragging.position = Vector3.Lerp(_dragging.position, target + _dragOffset, Time.deltaTime * followSpeed);
    }

    private void TryReleaseOnPizza()
    {
        if (_dragging == null) return;

        // Validar que cae dentro del radio permitido desde el centro
        Transform center = pizzaCenter != null ? pizzaCenter : pizzaRoot;
        float dist = Vector3.Distance(new Vector3(_dragging.position.x, center.position.y, _dragging.position.z),
                                      new Vector3(center.position.x, center.position.y, center.position.z));
        if (dist > pizzaRadius)
        {
            // Fuera de zona: descartar
            ReleaseCurrent(false);
            return;
        }

        // Parenteo definitivo bajo la pizza
        _dragging.SetParent(pizzaRoot, true);
        _placed.Add(_dragging);
        _dragging = null;
    }

    private void ReleaseCurrent(bool keepIfInside)
    {
        if (_dragging == null) return;

        if (!keepIfInside)
        {
            Destroy(_dragging.gameObject);
        }
        else
        {
            // Intento “soltar” donde esté; si no es hijo de pizzaRoot, lo parenteo
            if (!_dragging.IsChildOf(pizzaRoot))
                _dragging.SetParent(pizzaRoot, true);
            _placed.Add(_dragging);
        }
        _dragging = null;
    }

    // ---------------------------
    // GUARDADO
    // ---------------------------

    [System.Serializable]
    public class ToppingEntry
    {
        public string prefabName;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    /// <summary>
    /// Crea/actualiza un ScriptableObject con las posiciones relativas de los toppings.
    /// En Editor, opcionalmente “hornea” un Prefab.
    /// </summary>
    public void SaveAssembly(bool alsoMakeEditorPrefab = true, string editorPrefabPath = "Assets/Pizza_Assembled.prefab")
    {
        if (pizzaRoot == null)
        {
            Debug.LogWarning("PizzaToppingManager: pizzaRoot no asignado.");
            return;
        }

        // Recolectar entradas
        List<ToppingEntry> entries = new();
        foreach (Transform child in pizzaRoot)
        {
            // Solo tomar toppings con Mesh/Renderer (salta colliders de la base si los hay)
            if (child == pizzaRoot) continue;
            if (!child.gameObject.activeInHierarchy) continue;

            var e = new ToppingEntry
            {
                prefabName   = child.name.Replace("(Clone)", "").Trim(),
                localPosition= child.localPosition,
                localRotation= child.localRotation,
                localScale   = includeScaleInRecipe ? child.localScale : Vector3.one
            };
            entries.Add(e);
        }

        // Guardar como receta runtime
        var recipe = ScriptableObject.CreateInstance<PizzaRecipe>();
        recipe.entries = entries.ToArray();

#if UNITY_EDITOR
        // Guardar asset ScriptableObject
        string recipePath = $"Assets/{recipeAssetName}.asset";
        AssetDatabase.CreateAsset(recipe, recipePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Pizza] Receta guardada en {recipePath} ({entries.Count} toppings).");

        if (alsoMakeEditorPrefab)
        {
            // Crear/actualizar Prefab con el estado actual del pizzaRoot
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(pizzaRoot.gameObject, editorPrefabPath, InteractionMode.UserAction);
            if (prefab != null)
                Debug.Log($"[Pizza] Prefab horneado en: {editorPrefabPath}");
            else
                Debug.LogWarning("[Pizza] No se pudo crear el prefab.");
        }
#else
        // En runtime fuera del editor, simplemente lo dejas en memoria
        Debug.Log($"[Pizza] Receta (runtime) creada con {entries.Count} toppings.");
#endif
    }

    /// <summary>
    /// Limpia todos los toppings colocados (no la base).
    /// </summary>
    public void ClearPlaced()
    {
        foreach (var t in _placed)
        {
            if (t != null) Destroy(t.gameObject);
        }
        _placed.Clear();
    }
}
