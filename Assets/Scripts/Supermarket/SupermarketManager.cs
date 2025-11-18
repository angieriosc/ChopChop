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
        public BuyableIngredient ingredientPrefab;
    }
    
    [Header("Budget")]
    [SerializeField] private float initialBudget = 1200f;
    private float currentMoney;
    
    [Header("Shopping List")]
    [SerializeField] private List<ShoppingListItem> shoppingList = new List<ShoppingListItem>();
    
    [Header("References")]
    [SerializeField] private SupermarketUI uiController;
    
    [Header("Floating Text")]
    [SerializeField] private GameObject floatingPriceTextPrefab;
    
    public static SupermarketManager Instance { get; private set; }
    private bool shoppingListVisible = false;
    
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
        // Inicialmente ocultar la lista de compras
        if (uiController != null)
        {
            uiController.HideShoppingList();
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// Muestra la lista de compras por primera vez.
    /// </summary>
    public void ShowShoppingListFirstTime()
    {
        if (!shoppingListVisible && uiController != null)
        {
            shoppingListVisible = true;
            uiController.ShowShoppingList();
            Debug.Log("📋 Lista de compras activada");
        }
    }
    
    /// <summary>
    /// Inicializa la lista de compras con los ingredientes requeridos.
    /// </summary>
    private void InitializeShoppingList()
    {
    

        // Asignar iconos desde los prefabs si están disponibles
    foreach (var item in shoppingList)
    {
        if (item.ingredientPrefab != null && item.icon == null)
        {
            item.icon = item.ingredientPrefab.Icon;
        }
    }
    }
    
    /// <summary>
    /// Procesa la compra del carrito.
    /// </summary>
    public bool ProcessPurchase(ShoppingCart cart)
    {
        if (cart == null || cart.ItemCount == 0)
        {
            Debug.Log("⚠️ El carrito está vacío");
            if (SupermarketAudioManager.Instance != null)
            {
                SupermarketAudioManager.Instance.PlayPaymentFailSound();
            }
            return false;
        }
        
        float totalPrice = cart.GetTotalPrice();
        
        // Verificar si tiene suficiente dinero ANTES de cobrar
        if (totalPrice > currentMoney)
        {
            Debug.Log($"⚠️ Fondos insuficientes. Total: ${totalPrice:F2}, Disponible: ${currentMoney:F2}");
            if (SupermarketAudioManager.Instance != null)
            {
                SupermarketAudioManager.Instance.PlayPaymentFailSound();
            }
            return false;
        }
        
        // AQUÍ es donde se descuenta el dinero
        currentMoney -= totalPrice;
        
        // Procesar cada item del carrito
        List<BuyableIngredient> cartItems = cart.GetCartItems();
        foreach (var ingredient in cartItems)
        {
            MarkItemAsPurchased(ingredient.IngredientName);
        }
        
        // Limpiar carrito (esto destruirá los ingredientes)
        cart.ClearCart();
        
        Debug.Log($"✅ Compra exitosa! Total pagado: ${totalPrice:F2}. Dinero restante: ${currentMoney:F2}");
        
        // Sonido de éxito
        if (SupermarketAudioManager.Instance != null)
        {
            SupermarketAudioManager.Instance.PlayPaymentSuccessSound();
        }
        
        UpdateUI();
        
        return true;
    }
    
    /// <summary>
    /// Marca un item de la lista como comprado cuando se agrega al carrito.
    /// NO descuenta dinero, solo marca el item.
    /// </summary>
    public void MarkItemAddedToCart(string ingredientName, float price, Vector3 itemPosition)
    {
        // NO descontar dinero aquí, solo al pagar
        
        // Mostrar texto flotante
        if (floatingPriceTextPrefab != null)
        {
            Vector3 textPosition = itemPosition + Vector3.up * 1.5f;
            GameObject textObj = Instantiate(floatingPriceTextPrefab, textPosition, Quaternion.identity);
            FloatingPriceText floatingText = textObj.GetComponent<FloatingPriceText>();
            if (floatingText != null)
            {
                floatingText.Initialize(price, textPosition);
            }
        }
        
        // Marcar en la lista
        foreach (var item in shoppingList)
        {
            if (!item.isPurchased && 
                (item.itemName.ToLower().Contains(ingredientName.ToLower()) || 
                 Mathf.Abs(item.itemPrice - price) < 0.01f))
            {
                item.isPurchased = true;
                Debug.Log($"✓ {item.itemName} marcado en la lista");
                break;
            }
        }
        
        UpdateUI();
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
    /// Verifica si el jugador puede agregar un item al carrito sin pasarse del presupuesto.
    /// </summary>
    public bool CanAfford(float additionalPrice, float currentCartTotal)
    {
        float futureTotal = currentCartTotal + additionalPrice;
        return futureTotal <= currentMoney;
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
    public SupermarketUI UIController => uiController;
}