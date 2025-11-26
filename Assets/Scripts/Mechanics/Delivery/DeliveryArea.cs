using UnityEngine;

/// <summary>
/// Zona de entrega sobre la mesa.
/// Úsalo en cada tabla (izquierda, derecha).
/// </summary>
[RequireComponent(typeof(Collider))]
public class DeliveryArea : MonoBehaviour
{
    [Tooltip("Índice del punto de entrega (0 = izquierda, 1 = derecha, etc.)")]
    public int slotIndex = 0;
}
