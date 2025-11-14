using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la interfaz de usuario del supermercado.
/// Muestra el dinero disponible y la lista de compras.
/// </summary>
public class SupermarketUI : MonoBehaviour
{
    [Header("Money Display")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private string moneyPrefix = "Dinero: $";
    
    [Header("Shopping List")]
    [SerializeField] private Transform shoppingListContainer;
    [SerializeField] private GameObject shoppingListItemPrefab;
    [SerializeField] private GameObject shoppingListPanel; // Panel completo para mostrar/ocultar
    
    [Header("Cart Info")]
    [SerializeField] private TextMeshProUGUI cartTotalText;
    [SerializeField] private TextMeshProUGUI cartItemCountText;
    
    private List<GameObject> listItemInstances = new List<GameObject>();
    
    /// <summary>
    /// Muestra la lista de compras.
    /// </summary>
    public void ShowShoppingList()
    {
        if (shoppingListPanel != null)
        {
            shoppingListPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Oculta la lista de compras.
    /// </summary>
    public void HideShoppingList()
    {
        if (shoppingListPanel != null)
        {
            shoppingListPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Actualiza el texto del dinero disponible.
    /// </summary>
    public void UpdateMoney(float currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"{moneyPrefix}{currentMoney:F2}";
        }
    }
    
    /// <summary>
    /// Actualiza la lista de compras visual.
    /// </summary>
    public void UpdateShoppingList(List<SupermarketManager.ShoppingListItem> items)
    {
        // Limpiar lista anterior
        foreach (var instance in listItemInstances)
        {
            if (instance != null)
            {
                Destroy(instance);
            }
        }
        listItemInstances.Clear();
        
        // Crear nuevos items
        if (shoppingListContainer == null || shoppingListItemPrefab == null) return;
        
        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(shoppingListItemPrefab, shoppingListContainer);
            listItemInstances.Add(itemObj);
            
            // Configurar el item
            ShoppingListItemUI itemUI = itemObj.GetComponent<ShoppingListItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(item.itemName, item.itemPrice, item.isPurchased, item.icon);
            }
        }
    }
    
    /// <summary>
    /// Actualiza la información del carrito actual.
    /// </summary>
    public void UpdateCartInfo(ShoppingCart cart)
    {
        if (cart == null)
        {
            if (cartTotalText != null)
                cartTotalText.text = "Total: $0.00";
            
            if (cartItemCountText != null)
                cartItemCountText.text = "Items: 0";
            
            return;
        }
        
        if (cartTotalText != null)
        {
            cartTotalText.text = $"Total: ${cart.GetTotalPrice():F2}";
        }
        
        if (cartItemCountText != null)
        {
            cartItemCountText.text = $"Items: {cart.ItemCount}";
        }
    }
}