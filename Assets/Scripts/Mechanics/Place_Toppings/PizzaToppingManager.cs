using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ToppingOption
{
    public string name;
    public GameObject prefab;
}

public class PizzaToppingManager : MonoBehaviour
{
    [Header("Cámara de trabajo (se asigna al entrar a la estación)")]
    public Camera workCamera;

    [Header("Base de pizza")]
    public Transform pizzaRoot;                 // padre de todos los toppings
    public Collider pizzaSurfaceCollider;       // collider de la pizza (mesh/box)
    public Transform pizzaCenter;               // centro lógico; si es null usa pizzaRoot
    public float surfaceYOffset = 0.01f;        // para no hundirse
    public float pizzaRadius = 0.30f;           // radio permitido desde el centro

    [Header("Catálogo de toppings (para UI)")]
    public List<ToppingOption> toppings = new List<ToppingOption>();

    [Header("Opciones de spawn")]
    public bool randomYaw = true;               // rotación aleatoria sobre Y
    public Vector3 extraScale = Vector3.one;    // por si quieres escalar ligero
    public LayerMask pizzaLayerMask = ~0;       // opcional: limita el raycast a la pizza

    // estado
    private bool _enabled;
    private GameObject _currentToppingPrefab;

    // ===== API =====
    public void SetEnabled(bool value) => _enabled = value;

    // dentro de PizzaToppingManager
    public void DebugDump()
    {
        Debug.Log($"[ToppingManager] enabled={_enabled}, cam={(workCamera?workCamera.name:"null")}, " +
                $"pizzaRoot={(pizzaRoot?pizzaRoot.name:"null")}, surface={(pizzaSurfaceCollider?pizzaSurfaceCollider.name:"null")}, " +
                $"center={(pizzaCenter?pizzaCenter.name:"null")}, currentPrefab={(_currentToppingPrefab?_currentToppingPrefab.name:"null")}");
    }


    /// Llamado por la UI (botones) para elegir qué topping colocar
    public void SelectToppingByIndex(int index)
    {
        if (index < 0 || index >= toppings.Count)
        {
            Debug.LogWarning($"[ToppingManager] Índice de topping inválido: {index}");
            return;
        }
        _currentToppingPrefab = toppings[index].prefab;
        Debug.Log($"[ToppingManager] Topping seleccionado: {toppings[index].name}");
    }

    /// Des-selecciona topping (p.ej., botón "Mano vacía")
    public void ClearSelection() => _currentToppingPrefab = null;

    private void Update()
    {
        if (!_enabled || workCamera == null || pizzaSurfaceCollider == null) return;
        if (_currentToppingPrefab == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlaceToppingAtMouse();
        }
    }

    private void TryPlaceToppingAtMouse()
    {
        if (workCamera == null || pizzaSurfaceCollider == null)
        {
            Debug.LogWarning("[ToppingManager] Falta cámara o collider de pizza");
            return;
        }
        Ray ray = workCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // Raycast contra el collider de la pizza
        bool hitPizza;
        if (pizzaLayerMask.value != ~0)
            hitPizza = Physics.Raycast(ray, out hit, 100f, pizzaLayerMask, QueryTriggerInteraction.Ignore);
        else
            hitPizza = pizzaSurfaceCollider.Raycast(ray, out hit, 100f);

        if (!hitPizza) return;

        // Posición objetivo con pequeño offset a lo largo de la normal
        Vector3 target = hit.point + hit.normal * surfaceYOffset;

        // Clamp al radio desde el centro
        Transform center = pizzaCenter != null ? pizzaCenter : pizzaRoot;
        Vector3 flat = target;
        flat.y = center.position.y; // proyectamos a plano para medir distancia radial
        Vector3 centerFlat = center.position;
        centerFlat.y = flat.y;

        Vector3 fromCenter = flat - centerFlat;
        if (fromCenter.magnitude > pizzaRadius)
        {
            fromCenter = fromCenter.normalized * pizzaRadius;
        }
        // reconstruimos la posición respetando la altura original (con offset)
        target = new Vector3(centerFlat.x + fromCenter.x, hit.point.y, centerFlat.z + fromCenter.z) + hit.normal * surfaceYOffset;

        SpawnTopping(target, hit.normal);
    }

    private void SpawnTopping(Vector3 position, Vector3 normal)
    {
        if (_currentToppingPrefab == null || pizzaRoot == null) return;

        Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, normal), normal);
        if (randomYaw)
        {
            rot = Quaternion.AngleAxis(Random.Range(0f, 360f), normal) * rot;
        }

        GameObject go = Instantiate(_currentToppingPrefab, position, rot, pizzaRoot);
        if (extraScale != Vector3.one)
            go.transform.localScale = Vector3.Scale(go.transform.localScale, extraScale);
    }
}
