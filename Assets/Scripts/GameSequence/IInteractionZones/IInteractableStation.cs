using UnityEngine;
public interface IInteractableStation
{
    void Interact(); 
    bool CanInteract();  // Sirve para saber si debe mostrar el texto
}
