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
        
        [Tooltip("Cantidad de items que se deben comprar de este tipo.")]
        public int requiredQuantity = 1;
        
        [HideInInspector]
        public int purchasedQuantity = 0;
        
        public bool IsComplete => purchasedQuantity >= requiredQuantity;
        
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
    
    [Header("Funds Check Settings")]
    [SerializeField] private float checkFundsInterval = 2f; // Verificar cada 2 segundos
    
    public static SupermarketManager Instance { get; private set; }
    private bool shoppingListVisible = false;
    private bool missionCompleted = false;
    private bool hasCheckedFunds = false;
    private float nextFundsCheckTime = 0f;
    
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
    
    private void Update()
    {
        // Verificar periódicamente si el jugador puede completar las compras
        if (shoppingListVisible && !missionCompleted && !hasCheckedFunds)
        {
            if (Time.time >= nextFundsCheckTime)
            {
                nextFundsCheckTime = Time.time + checkFundsInterval;
                CheckIfCanCompleteList();
            }
        }
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
            item.purchasedQuantity = 0;
        }
    }
    
    /// <summary>
    /// Verifica si el jugador tiene suficiente dinero para completar la lista.
    /// </summary>
    private void CheckIfCanCompleteList()
    {
        float remainingCost = GetRemainingItemsCost();
        
        // Si el costo restante es 0, significa que ya completó todo
        if (remainingCost <= 0.01f)
        {
            return;
        }
        
        if (remainingCost > currentMoney)
        {
            hasCheckedFunds = true;
            Debug.Log($"💸 ¡No hay fondos suficientes! Necesitas ${remainingCost:F2} pero solo tienes ${currentMoney:F2}");
            
            // Mostrar Game Over por falta de fondos
            if (GameOverScreen.Instance != null)
            {
                GameOverScreen.Instance.ShowGameOver(GameOverReason.InsufficientFunds);
            }
            else
            {
                Debug.LogError("❌ GameOverScreen.Instance no existe en la escena");
            }
            
            // Pausar el timer
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StopTimer();
            }
        }
    }
    
    /// <summary>
    /// Calcula el costo total de los items que faltan por comprar.
    /// </summary>
    private float GetRemainingItemsCost()
    {
        float total = 0f;
        foreach (var item in shoppingList)
        {
            if (!item.IsComplete)
            {
                // Calcular cuántas unidades faltan
                int remainingQuantity = item.requiredQuantity - item.purchasedQuantity;
                total += item.itemPrice * remainingQuantity;
            }
        }
        return total;
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
        
        // Procesar cada item del carrito y marcar su cantidad como comprada
        List<BuyableIngredient> cartItems = cart.GetCartItems();
        foreach (var ingredient in cartItems)
        {
            MarkItemAsPurchased(ingredient.IngredientName, 1); // Marcar 1 unidad
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
        
        // Reiniciar el flag de verificación de fondos después de cada compra
        hasCheckedFunds = false;
        
        // Verificar si se completó la misión
        CheckMissionCompletion();
        
        return true;
    }
    
    /// <summary>
    /// Verifica si se completó la misión y muestra la pantalla de felicitaciones.
    /// </summary>
    private void CheckMissionCompletion()
    {
        if (missionCompleted) return; // Ya se mostró antes
        
        if (IsShoppingComplete())
        {
            missionCompleted = true;
            Debug.Log("🎉 ¡MISIÓN COMPLETADA! Todos los ingredientes comprados");
            
            // Detener el timer
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StopTimer();
            }
            
            // Mostrar pantalla de felicitaciones
            if (CompletionScreen.Instance != null)
            {
                // Pequeño delay para que el jugador vea primero el pago
                StartCoroutine(ShowCompletionWithDelay(1.5f));
            }
            else
            {
                Debug.LogError("❌ CompletionScreen.Instance es null! Asegúrate de que CompletionScreen esté en la escena");
            }
        }
        else
        {
            Debug.Log($"📋 Progreso: {GetPurchasedItemsCount()}/{shoppingList.Count} items COMPLETADOS");
        }
    }
    
    /// <summary>
    /// Muestra la pantalla de felicitaciones con un pequeño delay.
    /// </summary>
    private System.Collections.IEnumerator ShowCompletionWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompletionScreen.Instance.ShowCompletion();
    }
    
    /// <summary>
    /// Obtiene la cantidad de items COMPLETADOS.
    /// </summary>
    private int GetPurchasedItemsCount()
    {
        int count = 0;
        foreach (var item in shoppingList)
        {
            if (item.IsComplete) count++;
        }
        return count;
    }
    
    /// <summary>
    /// Marca un item de la lista como agregado al carrito (solo para texto flotante).
    /// </summary>
    public void MarkItemAddedToCart(string ingredientName, float price, Vector3 itemPosition)
    {
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
        
        UpdateUI();
    }
    
    /// <summary>
    /// Marca un item de la lista como comprado (incrementa la cantidad comprada).
    /// </summary>
    private void MarkItemAsPurchased(string itemName, int quantity)
    {
        foreach (var item in shoppingList)
        {
            if (item.itemName.Contains(itemName) || itemName.Contains(item.itemName.Split(' ')[0]))
            {
                // Solo incrementa si no ha alcanzado la cantidad requerida
                if (!item.IsComplete)
                {
                    item.purchasedQuantity += quantity;
                    if (item.purchasedQuantity > item.requiredQuantity)
                    {
                        item.purchasedQuantity = item.requiredQuantity; // Evitar que se exceda
                    }
                    Debug.Log($"✓ {item.itemName} Cantidad comprada: {item.purchasedQuantity}/{item.requiredQuantity}");
                }
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
    /// Verifica si se han comprado todos los items de la lista (cantidad requerida).
    /// </summary>
    public bool IsShoppingComplete()
    {
        foreach (var item in shoppingList)
        {
            if (!item.IsComplete)
                return false;
        }
        return true;
    }
    
    // Getters
    public float CurrentMoney => currentMoney;
    public List<ShoppingListItem> ShoppingList => shoppingList;
    public SupermarketUI UIController => uiController;
}