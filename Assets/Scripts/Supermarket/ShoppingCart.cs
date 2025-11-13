using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Carrito de compras que puede ser empujado por el jugador y almacenar ingredientes.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class ShoppingCart : MonoBehaviour
{
    [Header("Cart Settings")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private Transform handlePoint; // Punto donde el jugador agarra
    [SerializeField] private float itemSpacing = 0.3f;
    [SerializeField] private int maxItems = 10;
    [SerializeField] private float followSmoothness = 15f;
    
    [Header("Detection")]
    [SerializeField] private float ingredientDetectionRange = 2f;
    [SerializeField] private LayerMask ingredientLayer;
    
    private List<BuyableIngredient> cartItems = new List<BuyableIngredient>();
    private InteractableObject interactable;
    private bool isBeingPushed = false;
    private Rigidbody rb;
    
    private void Awake()
    {
        interactable = GetComponent<InteractableObject>();
        rb = GetComponent<Rigidbody>();
        
        // Configurar Rigidbody para evitar problemas
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Evitar que ruede
        }
        
        // Asegurar que tenga la capacidad de ser empujado
        if (!interactable.HasCapability(ObjectCapabilities.Pushable))
        {
            interactable.AddCapability(ObjectCapabilities.Pushable);
        }
    }
    
    private void Update()
    {
        if (isBeingPushed)
        {
            DetectNearbyIngredients();
        }
    }
    
    /// <summary>
    /// Detecta ingredientes cercanos que pueden ser agregados al carrito.
    /// </summary>
    private void DetectNearbyIngredients()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, ingredientDetectionRange, ingredientLayer);
        
        foreach (var col in nearbyColliders)
        {
            BuyableIngredient ingredient = col.GetComponent<BuyableIngredient>();
            if (ingredient != null && !cartItems.Contains(ingredient))
            {
                ingredient.ShowHighlight(true);
            }
        }
    }
    
    /// <summary>
    /// Intenta agregar un ingrediente al carrito.
    /// </summary>
    public bool TryAddIngredient(BuyableIngredient ingredient)
    {
        if (ingredient == null)
        {
            Debug.Log("⚠️ Ingrediente es nulo");
            return false;
        }
        
        if (ingredient.IsInCart)
        {
            Debug.Log("⚠️ Este ingrediente específico ya está en el carrito");
            return false;
        }
        
        if (cartItems.Count >= maxItems)
        {
            Debug.Log("⚠️ Carrito lleno");
            return false;
        }
        
        // Marcar como agregado ANTES de añadirlo
        ingredient.AddToCart();
        cartItems.Add(ingredient);
        
        // Posicionar el ingrediente en el carrito
        if (itemsContainer != null)
        {
            ingredient.transform.SetParent(itemsContainer);
            ingredient.transform.localPosition = Vector3.up * (cartItems.Count - 1) * itemSpacing;
            ingredient.transform.localRotation = Quaternion.identity;
        }
        
        // Desactivar física del ingrediente
        Rigidbody rb = ingredient.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        // Desactivar collider para evitar detección múltiple
        Collider col = ingredient.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        ingredient.ShowHighlight(false);
        
        // Notificar al SupermarketManager para actualizar la UI
        if (SupermarketManager.Instance != null)
        {
            SupermarketManager.Instance.MarkItemAddedToCart(ingredient.IngredientName, ingredient.Price);
        }
        
        Debug.Log($"✅ {ingredient.IngredientName} agregado al carrito");
        return true;
    }
    
    /// <summary>
    /// Obtiene la lista de ingredientes en el carrito.
    /// </summary>
    public List<BuyableIngredient> GetCartItems()
    {
        return new List<BuyableIngredient>(cartItems);
    }
    
    /// <summary>
    /// Calcula el total de la compra.
    /// </summary>
    public float GetTotalPrice()
    {
        float total = 0f;
        foreach (var item in cartItems)
        {
            total += item.Price;
        }
        return total;
    }
    
    /// <summary>
    /// Vacía el carrito después de la compra.
    /// </summary>
    public void ClearCart()
    {
        foreach (var item in cartItems)
        {
            item.MarkAsPurchased();
        }
        cartItems.Clear();
    }
    
    /// <summary>
    /// Marca que el jugador está empujando el carrito.
    /// </summary>
    public void SetBeingPushed(bool pushed)
    {
        isBeingPushed = pushed;
    }
    
    public bool IsBeingPushed => isBeingPushed;
    public int ItemCount => cartItems.Count;
    public Transform HandlePoint => handlePoint;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ingredientDetectionRange);
    }
}