using UnityEngine;

public class CustomerInteractionZone : MonoBehaviour, IInteractableStation
{
    private Customer currentCustomer;

    public void SetCustomer(Customer c) => currentCustomer = c;
    public void ClearCustomer() => currentCustomer = null;

    public bool CanInteract()
    {
        return currentCustomer != null;
    }

    public void Interact()
    {
        currentCustomer.OnInteract();
    }
}
