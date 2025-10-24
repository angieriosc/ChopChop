using UnityEngine;

/// <summary>
/// Representa un objeto interactuable con capacidades definidas.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Capabilities")]
    [SerializeField] public ObjectCapabilities capabilities;

    /// <summary>
    /// Verifica si el objeto tiene la capacidad especificada.
    /// </summary>
    /// <param name="capability">Capacidad a verificar</param>
    /// <returns>True si tiene la capacidad, False si no</returns>
    public bool HasCapability(ObjectCapabilities capability)
    {
        return (capabilities & capability) == capability;
    }

    /// <summary>
    /// Permite añadir una capacidad al objeto.
    /// </summary>
    /// <param name="capability">Capacidad a añadir</param>
    public void AddCapability(ObjectCapabilities capability)
    {
        capabilities |= capability;
    }

    /// <summary>
    /// Permite remover una capacidad del objeto.
    /// </summary>
    /// <param name="capability">Capacidad a remover</param>
    public void RemoveCapability(ObjectCapabilities capability)
    {
        capabilities &= ~capability;
    }
}
