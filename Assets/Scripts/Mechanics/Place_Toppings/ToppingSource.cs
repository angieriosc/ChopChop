using UnityEngine;

/// <summary>
/// Fuente de toppings: instacia un prefab y delega el drag al PizzaToppingManager.
/// Pon este script en un objeto con collider (la charola/botón del topping).
/// </summary>
public class ToppingSource : MonoBehaviour
{
    public PizzaToppingManager manager;
    public GameObject toppingPrefab;  // Prefab a instanciar
    public Transform spawnPoint;      // Donde aparece (si es null, usa la posición de este objeto)

    private void OnMouseDown()
    {
        if (manager == null || manager.enabled == false || toppingPrefab == null) return;

        var p = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up * 0.1f;
        var r = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        var inst = Instantiate(toppingPrefab, p, r);
        // Opcional: etiqueta/capa para raycast de arrastre
        // inst.layer = LayerMask.NameToLayer("Draggable"); 

        manager.BeginDrag(inst.transform);
    }
}
