using UnityEngine;

/// <summary>
/// Representa un ingrediente que puede ser comprado en el supermercado.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class BuyableIngredient : MonoBehaviour
{
    [Header("Ingredient Info")]
    [SerializeField] private string ingredientName = "Ingredient";
    [SerializeField] private float price = 0f;
    [SerializeField] private Sprite icon;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject highlightEffect;
    
    private InteractableObject interactable;
    private bool isPurchased = false;
    private bool isInCart = false;
    
    private void Awake()
    {
        interactable = GetComponent<InteractableObject>();
        
        // Asegurarnos de que tenga las capacidades correctas
        if (!interactable.HasCapability(ObjectCapabilities.Buyable))
        {
            interactable.AddCapability(ObjectCapabilities.Buyable);
        }
        
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// Marca el ingrediente como agregado al carrito.
    /// </summary>
    public void AddToCart()
    {
        isInCart = true;
        
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// Marca el ingrediente como comprado y lo hace agarrable.
    /// </summary>
    public void MarkAsPurchased()
    {
        isPurchased = true;
        isInCart = false;
        interactable.AddCapability(ObjectCapabilities.Grabbable);
        interactable.AddCapability(ObjectCapabilities.Droppable);
        
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// Activa el efecto de resaltado cuando el jugador está cerca.
    /// </summary>
    public void ShowHighlight(bool show)
    {
        if (highlightEffect != null && !isPurchased && !isInCart)
        {
            highlightEffect.SetActive(show);
        }
    }
    
    // Getters
    public string IngredientName => ingredientName;
    public float Price => price;
    public Sprite Icon => icon;
    public bool IsPurchased => isPurchased;
    public bool IsInCart => isInCart;
}