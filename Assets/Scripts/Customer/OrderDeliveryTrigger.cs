using UnityEngine;

public class OrderDeliveryTrigger : MonoBehaviour
{
    public CustomerManager customerManager;
    public KeyCode deliveryKey = KeyCode.E; // Tecla para entregar
    
    private bool canDeliver = false;
    
    void Update()
    {
        if (canDeliver && Input.GetKeyDown(deliveryKey))
        {
            DeliverOrder();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canDeliver = true;
            Debug.Log("Jugador puede entregar la orden. Presiona " + deliveryKey);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canDeliver = false;
            Debug.Log("Jugador salió del área de entrega");
        }
    }
    
    public void DeliverOrder()
    {
        if (customerManager != null)
        {
            Debug.Log("¡Orden entregada!");
            customerManager.OnOrderDelivered();
        }
        else
        {
            Debug.LogError("CustomerManager no está asignado en OrderDeliveryTrigger");
        }
    }
}