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
    [SerializeField] private LayerMask cartLayer;
    
    private ShoppingCart currentCart = null;
    private ShoppingCart nearbyCart = null;
    private BuyableIngredient nearbyIngredient = null;
    private Vector3 cartTargetPosition;
    
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
    }
    
    private void Update()
    {
        DetectNearbyObjects();
        HandleCartInput();
        HandleIngredientInput();
        
        if (currentCart != null)
        {
            UpdateCartPosition();
        }
    }
    
    /// <summary>
    /// Detecta carritos e ingredientes cercanos.
    /// </summary>
    private void DetectNearbyObjects()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, cartDetectionRange);
        
        bool cartFound = false;
        nearbyIngredient = null; // Resetear cada frame
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
    private void ReleaseCart()
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
        targetPos.y = currentCart.transform.position.y; // Mantener altura del carrito
        
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
        
        // Verificar si el jugador puede pagar
        if (!SupermarketManager.Instance.CanAfford(price))
        {
            Debug.Log($"⚠️ No tienes suficiente dinero para {nearbyIngredient.IngredientName}");
            return;
        }
        
        // Agregar al carrito
        bool added = currentCart.TryAddIngredient(nearbyIngredient);
        
        if (added)
        {
            Debug.Log($"✅ {nearbyIngredient.IngredientName} agregado al carrito (${price:F2})");
            nearbyIngredient = null; // Resetear para buscar el siguiente
        }
    }
    
    // Getters públicos
    public bool HasCart() => currentCart != null;
    public ShoppingCart GetCurrentCart() => currentCart;
    
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