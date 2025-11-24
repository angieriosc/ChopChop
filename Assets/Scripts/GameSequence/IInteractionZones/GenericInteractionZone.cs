using UnityEngine;

public class GenericInteractionZone : MonoBehaviour, IInteractableStation
{
    // Si siempre es interactuable
    public bool CanInteract() => true;

    // O simplemente no hace nada
    public void Interact()
    {
        Debug.Log("Interacción vacía: esta estación no tiene lógica.");
    }
}
