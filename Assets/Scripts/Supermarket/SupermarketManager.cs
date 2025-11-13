using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la lista de compras, el presupuesto y el sistema de pago del supermercado.
/// </summary>
public class SupermarketManager : MonoBehaviour
{
    [System.Serializable]
    public class ShoppingListItem
    {
        public string itemName;
        public float itemPrice;
        public bool isPurchased = false;
        public Sprite icon;
    }
    
    [Header("Budget")]
    [SerializeField] private float initialBudget = 1200f;
    private float currentMoney;
    
    [Header("Shopping List")]
    [SerializeField] private List<ShoppingListItem> shoppingList = new List<ShoppingListItem>();
    
    [Header("References")]
    [SerializeField] private SupermarketUI uiController;
    
    public static SupermarketManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        currentMoney = initialBudget;
        InitializeShoppingList();
    }
    
    private void Start()
    {
        UpdateUI();
    }
    
    /// <summary>
    /// Inicializa la lista de compras con los ingredientes requeridos.
    /// </summary>
    private void InitializeShoppingList()
    {
        shoppingList.Clear();
        
        shoppingList.Add(new ShoppingListItem 
        { 
            itemName = "Costal de harina", 
            itemPrice = 499f 
        });
        
        shoppingList.Add(new ShoppingListItem 
        { 
            itemName = "Queso", 
            itemPrice = 266f 
        });
        
        shoppingList.Add(new ShoppingListItem 
        { 
            itemName = "Puré de tomate x3", 
            itemPrice = 37f * 3 
        });
        
        shoppingList.Add(new ShoppingListItem 
        { 
            itemName = "Pimientos x2", 
            itemPrice = 22f * 2 
        });
        
        shoppingList.Add(new ShoppingListItem 
        { 
            itemName = "Frasco de levadura", 
            itemPrice = 104f 
        });
    }
    
    /// <summary>
    /// Procesa la compra del carrito.
    /// </summary>
    public bool ProcessPurchase(ShoppingCart cart)
    {
        if (cart == null || cart.ItemCount == 0)
        {
            Debug.Log("⚠️ El carrito está vacío");
            return false;
        }
        
        float totalPrice = cart.GetTotalPrice();
        
        if (totalPrice > currentMoney)
        {
            Debug.Log($"⚠️ Fondos insuficientes. Necesitas ${totalPrice:F2} pero solo tienes ${currentMoney:F2}");
            return false;
        }
        
        // Procesar cada item del carrito
        List<BuyableIngredient> cartItems = cart.GetCartItems();
        foreach (var ingredient in cartItems)
        {
            MarkItemAsPurchased(ingredient.IngredientName);
        }
        
        // Descontar dinero
        currentMoney -= totalPrice;
        
        // Limpiar carrito
        cart.ClearCart();
        
        Debug.Log($"✅ Compra exitosa! Total: ${totalPrice:F2}. Dinero restante: ${currentMoney:F2}");
        UpdateUI();
        
        return true;
    }
    
    /// <summary>
    /// Marca un item de la lista como comprado cuando se agrega al carrito.
    /// </summary>
    public void MarkItemAddedToCart(string ingredientName, float price)
    {
        foreach (var item in shoppingList)
        {
            // Comparar por nombre o precio
            if (!item.isPurchased && 
                (item.itemName.ToLower().Contains(ingredientName.ToLower()) || 
                 Mathf.Abs(item.itemPrice - price) < 0.01f))
            {
                item.isPurchased = true;
                Debug.Log($"✓ {item.itemName} marcado en la lista");
                UpdateUI();
                break;
            }
        }
    }
    
    /// <summary>
    /// Marca un item de la lista como comprado (método legacy para compatibilidad).
    /// </summary>
    private void MarkItemAsPurchased(string itemName)
    {
        foreach (var item in shoppingList)
        {
            if (item.itemName.Contains(itemName) || itemName.Contains(item.itemName.Split(' ')[0]))
            {
                item.isPurchased = true;
                break;
            }
        }
    }
    
    /// <summary>
    /// Verifica si el jugador puede comprar un item específico.
    /// </summary>
    public bool CanAfford(float price)
    {
        return price <= currentMoney;
    }
    
    /// <summary>
    /// Actualiza la UI del supermercado.
    /// </summary>
    private void UpdateUI()
    {
        if (uiController != null)
        {
            uiController.UpdateMoney(currentMoney);
            uiController.UpdateShoppingList(shoppingList);
        }
    }
    
    /// <summary>
    /// Verifica si se han comprado todos los items de la lista.
    /// </summary>
    public bool IsShoppingComplete()
    {
        foreach (var item in shoppingList)
        {
            if (!item.isPurchased)
                return false;
        }
        return true;
    }
    
    // Getters
    public float CurrentMoney => currentMoney;
    public List<ShoppingListItem> ShoppingList => shoppingList;
}