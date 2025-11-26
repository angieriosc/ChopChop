using UnityEngine;

public class DeliverySubmitButton : MonoBehaviour
{
    [SerializeField] private PizzaDeliveryManager deliveryManager;
    [SerializeField] private CustomerManager customerManager;   // ← nuevo

    public void OnEntregaPressed()
    {
        if (deliveryManager == null)
        {
            Debug.LogError("[Entrega] DeliveryManager no asignado.");
            return;
        }

        // Validar ronda actual
        if (deliveryManager.ValidateCurrentRound(out string msg))
        {
            Debug.Log("✔ ENTREGA CORRECTA para ronda " + deliveryManager.CurrentRoundIndex);
            deliveryManager.AdvanceRound();
            if (customerManager != null)
            {
                customerManager.OnOrderDelivered();
            }

            // Agregar, sonido, sumar puntos.....
        }
        else
        {
            Debug.Log("❌ ENTREGA INCORRECTA: " + msg);
            deliveryManager.AdvanceRound();
            if (customerManager != null)
            {
                customerManager.OnOrderDelivered();
            }
            // Agregar sonido error, etc.
        }
    }
}
