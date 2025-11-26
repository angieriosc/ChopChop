using UnityEngine;

/// <summary>
/// Zona de pago donde el jugador puede procesar la compra del carrito.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckoutZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode paymentKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject paymentPrompt;
    [SerializeField] private Transform paymentPosition;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private ShoppingCart currentCart = null;
    private PlayerCartController playerInZone = null;

    private void Awake()
    {
        // Asegurar que el collider es trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("❌ CheckoutZone necesita un Collider!");
        }

        if (paymentPrompt != null)
        {
            paymentPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInZone != null && playerInZone.HasCart())
        {
            currentCart = playerInZone.GetCurrentCart();

            if (Input.GetKeyDown(paymentKey) && currentCart != null && currentCart.ItemCount > 0)
            {
                ProcessPayment();
            }
        }
    }

    /// <summary>
    /// Procesa el pago del carrito actual.
    /// </summary>
    private void ProcessPayment()
    {
        if (currentCart == null || currentCart.ItemCount == 0)
        {
            Debug.Log("⚠️ El carrito está vacío");
            return;
        }

        float totalPrice = currentCart.GetTotalPrice();

        if (showDebugLogs)
        {
            Debug.Log($"💰 Intentando procesar pago de ${totalPrice:F2}");
        }

        // Verificar si tiene suficiente dinero
        if (totalPrice > SupermarketManager.Instance.CurrentMoney)
        {
            Debug.Log($"⚠️ Fondos insuficientes. Total: ${totalPrice:F2}, Disponible: ${SupermarketManager.Instance.CurrentMoney:F2}");
            
            // Sonido de error
            if (SupermarketAudioManager.Instance != null)
            {
                SupermarketAudioManager.Instance.PlayPaymentFailSound();
            }
            return;
        }

        // Procesar la compra
        bool success = SupermarketManager.Instance.ProcessPurchase(currentCart);

        if (success)
        {
            Debug.Log($"✅ ¡Compra completada! Total pagado: ${totalPrice:F2}");

            // ✅ Desactivar prompts de tutorial después de la primera compra
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.OnFirstPurchaseComplete();
            }

            // Opcional: mover el carrito a la posición de pago
            if (paymentPosition != null)
            {
                currentCart.transform.position = paymentPosition.position;
                currentCart.transform.rotation = paymentPosition.rotation;
            }

            // Ocultar prompt después de pagar
            if (paymentPrompt != null)
            {
                paymentPrompt.SetActive(false);
            }
            
            // Soltar el carrito automáticamente después del pago
            if (playerInZone != null)
            {
                playerInZone.ReleaseCart();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🔍 CheckoutZone detectó: {other.gameObject.name}");
        }

        // Detectar si el jugador entra
        PlayerCartController cartController = other.GetComponent<PlayerCartController>();
        if (cartController != null)
        {
            playerInZone = cartController;

            if (showDebugLogs)
            {
                Debug.Log($"✅ Jugador entró a la zona de pago");
            }

            // Mostrar prompt si tiene carrito con items
            if (cartController.HasCart())
            {
                currentCart = cartController.GetCurrentCart();

                if (currentCart != null && currentCart.ItemCount > 0)
                {
                    if (paymentPrompt != null)
                    {
                        paymentPrompt.SetActive(true);
                    }

                    Debug.Log($"💰 Zona de pago activada. Total en carrito: ${currentCart.GetTotalPrice():F2} - Presiona {paymentKey} para pagar");
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Actualizar el estado del prompt mientras el jugador está en la zona
        if (playerInZone != null && paymentPrompt != null)
        {
            bool shouldShowPrompt = playerInZone.HasCart() &&
                                   playerInZone.GetCurrentCart() != null &&
                                   playerInZone.GetCurrentCart().ItemCount > 0;

            if (paymentPrompt.activeSelf != shouldShowPrompt)
            {
                paymentPrompt.SetActive(shouldShowPrompt);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerCartController cartController = other.GetComponent<PlayerCartController>();
        if (cartController != null && cartController == playerInZone)
        {
            if (showDebugLogs)
            {
                Debug.Log($"🚪 Jugador salió de la zona de pago");
            }

            playerInZone = null;
            currentCart = null;

            if (paymentPrompt != null)
            {
                paymentPrompt.SetActive(false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}