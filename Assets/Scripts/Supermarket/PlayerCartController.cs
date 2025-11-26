using UnityEngine;

/// <summary>
/// Controla la interacción del jugador con el carrito de compras.
/// Permite agarrar, empujar y soltar el carrito.
/// </summary>
public class PlayerCartController : MonoBehaviour
{
    [Header("Cart Settings")]
    [SerializeField] private Transform cartHoldPoint;
    [SerializeField] private float cartOffset = 1.5f;
    [SerializeField] private float cartDetectionRange = 2f;
    [SerializeField] private float cartFollowSpeed = 10f;
    
    [Header("Input")]
    [SerializeField] private KeyCode grabCartKey = KeyCode.E;
    [SerializeField] private KeyCode dropCartKey = KeyCode.Q;
    [SerializeField] private KeyCode addToCartKey = KeyCode.F;
    
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private InteractionPrompt interactionPrompt;
    
    private ShoppingCart currentCart = null;
    private ShoppingCart nearbyCart = null;
    private BuyableIngredient nearbyIngredient = null;
    private Vector3 cartTargetPosition;
    private Vector3 cartInitialPosition;
    private Quaternion cartInitialRotation;
    private bool hasGrabbedCartBefore = false;
    private Vector3 lastPlayerPosition;
    
    private void Awake()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        // Si no hay punto de agarre, crearlo automáticamente
        if (cartHoldPoint == null)
        {
            GameObject holdPoint = new GameObject("CartHoldPoint");
            holdPoint.transform.SetParent(transform);
            holdPoint.transform.localPosition = new Vector3(0, 0.5f, cartOffset);
            cartHoldPoint = holdPoint.transform;
        }
        
        // Si no se asignó manualmente, usar el singleton
        if (interactionPrompt == null)
        {
            interactionPrompt = InteractionPrompt.Instance;
        }
        
        lastPlayerPosition = transform.position;
    }
    
    private void Update()
    {
        DetectNearbyObjects();
        HandleCartInput();
        HandleIngredientInput();
        
        if (currentCart != null)
        {
            UpdateCartPosition();
            HandleCartAudio();
            
            // Actualizar UI del carrito en tiempo real
            if (SupermarketManager.Instance != null && SupermarketManager.Instance.UIController != null)
            {
                SupermarketManager.Instance.UIController.UpdateCartInfo(currentCart);
            }
        }
        
        lastPlayerPosition = transform.position;
    }
    
    /// <summary>
    /// Detecta carritos e ingredientes cercanos.
    /// </summary>
    private void DetectNearbyObjects()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, cartDetectionRange);
        
        bool cartFound = false;
        nearbyIngredient = null;
        float closestIngredientDist = float.MaxValue;
        
        foreach (var col in nearbyColliders)
        {
            // Detectar carrito solo si no tenemos uno
            if (!cartFound && currentCart == null)
            {
                ShoppingCart cart = col.GetComponent<ShoppingCart>();
                if (cart != null && !cart.IsBeingPushed)
                {
                    nearbyCart = cart;
                    cartFound = true;
                }
            }
            
            // Detectar ingrediente más cercano solo si tenemos carrito
            if (currentCart != null)
            {
                BuyableIngredient ingredient = col.GetComponent<BuyableIngredient>();
                if (ingredient != null && !ingredient.IsPurchased && !ingredient.IsInCart)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < closestIngredientDist)
                    {
                        nearbyIngredient = ingredient;
                        closestIngredientDist = dist;
                    }
                }
            }
        }
        
        if (!cartFound) nearbyCart = null;
        
        // Actualizar prompts
        UpdatePrompts();
    }
    
    /// <summary>
    /// Maneja el input para agarrar y soltar el carrito.
    /// </summary>
    private void HandleCartInput()
    {
        // Agarrar carrito
        if (Input.GetKeyDown(grabCartKey) && currentCart == null && nearbyCart != null)
        {
            GrabCart(nearbyCart);
        }
        
        // Soltar carrito
        if (Input.GetKeyDown(dropCartKey) && currentCart != null)
        {
            ReleaseCart();
        }
    }
    
    /// <summary>
    /// Maneja el input para agregar ingredientes al carrito.
    /// </summary>
    private void HandleIngredientInput()
    {
        if (Input.GetKeyDown(addToCartKey) && currentCart != null && nearbyIngredient != null)
        {
            AddIngredientToCart();
        }
    }
    
    /// <summary>
    /// Agarra el carrito y comienza a empujarlo.
    /// </summary>
    private void GrabCart(ShoppingCart cart)
    {
        currentCart = cart;
        currentCart.SetBeingPushed(true);
        nearbyCart = null;
        
        // Guardar posición inicial del carrito
        if (!hasGrabbedCartBefore)
        {
            cartInitialPosition = cart.transform.position;
            cartInitialRotation = cart.transform.rotation;
            hasGrabbedCartBefore = true;
            
            // Mostrar lista de compras por primera vez
            if (SupermarketManager.Instance != null)
            {
                SupermarketManager.Instance.ShowShoppingListFirstTime();
            }
            
            // ✅ NUEVO: Iniciar el timer cuando se agarra el carrito por primera vez
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StartTimer();
                Debug.Log("⏰ ¡Timer iniciado! Tienes 1 minuto para completar las compras");
            }
            else
            {
                Debug.LogWarning("⚠️ GameTimer.Instance no existe. Asegúrate de tener el GameTimer en la escena");
            }
        }
        
        // Desactivar colisiones entre el jugador y el carrito
        Collider cartCollider = currentCart.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        
        if (cartCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(cartCollider, playerCollider, true);
        }
        
        // Hacer el carrito cinemático para control total
        Rigidbody cartRb = currentCart.GetComponent<Rigidbody>();
        if (cartRb != null)
        {
            cartRb.isKinematic = true;
            cartRb.useGravity = false;
        }
        
        Debug.Log("🛒 Carrito agarrado - Usa WASD para empujarlo, Q para soltar, F para agregar items");
    }
    
    /// <summary>
    /// Suelta el carrito.
    /// </summary>
    public void ReleaseCart()
    {
        if (currentCart == null) return;
        
        currentCart.SetBeingPushed(false);
        
        // Reactivar colisiones
        Collider cartCollider = currentCart.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        
        if (cartCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(cartCollider, playerCollider, false);
        }
        
        // Reactivar física del carrito
        Rigidbody cartRb = currentCart.GetComponent<Rigidbody>();
        if (cartRb != null)
        {
            cartRb.isKinematic = false;
            cartRb.useGravity = true;
            cartRb.linearVelocity = Vector3.zero;
            cartRb.angularVelocity = Vector3.zero;
        }
        
        // Detener el sonido del carrito
        if (SupermarketAudioManager.Instance != null)
        {
            SupermarketAudioManager.Instance.StopCartRollingSound();
        }
        
        Debug.Log("🛒 Carrito soltado");
        currentCart = null;
    }
    
    /// <summary>
    /// Actualiza la posición del carrito mientras es empujado.
    /// </summary>
    private void UpdateCartPosition()
    {
        if (currentCart == null) return;
        
        // Calcular posición objetivo del carrito (frente al jugador)
        Vector3 targetPos = cartHoldPoint.position;
        targetPos.y = currentCart.transform.position.y;
        
        // Mover suavemente el carrito a la posición objetivo
        currentCart.transform.position = Vector3.Lerp(
            currentCart.transform.position,
            targetPos,
            Time.deltaTime * cartFollowSpeed
        );
        
        // Rotar el carrito para que mire en la misma dirección que el jugador
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward);
        currentCart.transform.rotation = Quaternion.Slerp(
            currentCart.transform.rotation,
            targetRotation,
            Time.deltaTime * cartFollowSpeed
        );
    }
    
    /// <summary>
    /// Maneja el audio del carrito basado en el movimiento del jugador.
    /// </summary>
    private void HandleCartAudio()
    {
        if (SupermarketAudioManager.Instance == null) return;
        
        // Calcular si el jugador se está moviendo
        float moveDistance = Vector3.Distance(transform.position, lastPlayerPosition);
        bool isMoving = moveDistance > 0.001f;
        
        if (isMoving)
        {
            // Si el jugador se mueve, reproducir/reanudar el sonido
            SupermarketAudioManager.Instance.PlayCartRollingSound();
        }
        else
        {
            // Si el jugador está quieto, pausar el sonido
            SupermarketAudioManager.Instance.PauseCartRollingSound();
        }
    }
    
    /// <summary>
    /// Agrega el ingrediente cercano al carrito.
    /// </summary>
    private void AddIngredientToCart()
    {
        if (nearbyIngredient == null || currentCart == null) 
        {
            Debug.Log("⚠️ No hay ingrediente cercano o no tienes carrito");
            return;
        }
        
        if (nearbyIngredient.IsInCart)
        {
            Debug.Log("⚠️ Este ingrediente ya fue agregado");
            return;
        }
        
        float price = nearbyIngredient.Price;
        float currentCartTotal = currentCart.GetTotalPrice();
        
        // Verificar si el jugador puede pagar (sin exceder presupuesto)
        if (!SupermarketManager.Instance.CanAfford(price, currentCartTotal))
        {
            Debug.Log($"⚠️ No puedes agregar {nearbyIngredient.IngredientName}. Se pasaría del presupuesto de ${SupermarketManager.Instance.CurrentMoney:F2}");
            return;
        }
        
        // Agregar al carrito
        bool added = currentCart.TryAddIngredient(nearbyIngredient);
        
        if (added)
        {
            Debug.Log($"✅ {nearbyIngredient.IngredientName} agregado al carrito (${price:F2})");
            nearbyIngredient = null;
        }
    }
    
    /// <summary>
    /// Regresa el carrito a su posición inicial.
    /// </summary>
    public void ResetCartPosition()
    {
        if (currentCart != null)
        {
            currentCart.transform.position = cartInitialPosition;
            currentCart.transform.rotation = cartInitialRotation;
            Debug.Log("🛒 Carrito regresado a su posición inicial");
        }
    }
    
    // Getters públicos
    public bool HasCart() => currentCart != null;
    public ShoppingCart GetCurrentCart() => currentCart;
    
    /// <summary>
    /// Actualiza los prompts de interacción según el contexto.
    /// </summary>
    private void UpdatePrompts()
    {
        if (interactionPrompt == null) return;
        
        // Si no tiene carrito y hay uno cerca
        if (currentCart == null && nearbyCart != null)
        {
            interactionPrompt.ShowPrompt($"Presiona [{grabCartKey}] para agarrar el carrito");
        }
        // Si tiene carrito y hay ingrediente cerca
        else if (currentCart != null && nearbyIngredient != null)
        {
            interactionPrompt.ShowPrompt($"Presiona [{addToCartKey}] para agregar {nearbyIngredient.IngredientName} (${nearbyIngredient.Price:F2})");
        }
        // Si tiene carrito pero no hay ingrediente cerca
        else if (currentCart != null)
        {
            interactionPrompt.ShowPrompt($"Presiona [{dropCartKey}] para soltar el carrito");
        }
        // Si no hay nada cerca
        else
        {
            interactionPrompt.HidePrompt();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cartDetectionRange);
        
        if (cartHoldPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(cartHoldPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, cartHoldPoint.position);
        }
    }
}