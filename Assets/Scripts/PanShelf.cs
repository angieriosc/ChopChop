using UnityEngine;

[DisallowMultipleComponent]
public class PanShelf : MonoBehaviour
{
    [Header("Prefab a instanciar al agarrar")]
    public GameObject panPrefab; // Prefab del sartén a entregar

    [Header("Opcional")]
    public bool alignToHand = true;        // Colocar en (0,0,0) local de la mano
    public bool keepWorldRotation = false; // Si false, se resetea rotación local

    [Tooltip("Ajustes finos de posición/rotación en la mano")]
    public Vector3 localPositionOffset = Vector3.zero;
    public Vector3 localEulerOffset = Vector3.zero;

    /// <summary>
    /// Instancia el prefab del sartén y lo coloca en la mano del jugador.
    /// </summary>
    public GameObject Spawn(Transform handPoint)
    {
        if (panPrefab == null || handPoint == null) return null;

        GameObject instance = Instantiate(panPrefab);

        // Parent al punto de la mano
        instance.transform.SetParent(handPoint, worldPositionStays: false);

        if (alignToHand)
        {
            instance.transform.localPosition = Vector3.zero + localPositionOffset;
            instance.transform.localRotation = Quaternion.Euler(localEulerOffset);
        }

        // Asegurar física correcta para “agarrado”
        var rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        return instance;
    }
}
