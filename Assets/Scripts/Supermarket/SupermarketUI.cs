using System.Collections;
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
    [SerializeField] private GameObject moneyPanel;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Image moneyIcon;
    [SerializeField] private TextMeshProUGUI cartTotalText;
    [SerializeField] private string moneyPrefix = "Presupuesto: $";
    [SerializeField] private string cartTotalPrefix = "Total: $";
    
    [Header("Shopping List")]
    [SerializeField] private Transform shoppingListContainer;
    [SerializeField] private GameObject shoppingListItemPrefab;
    [SerializeField] private GameObject shoppingListPanel;
    
    [Header("Cart Info")]
    [SerializeField] private TextMeshProUGUI cartItemCountText;
    [SerializeField] private GameObject cartInfoPanel; // NUEVO: Panel que contiene la info del carrito
    
    [Header("Animation Settings")]
    [SerializeField] private float pulseDuration = 0.3f;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float warningThreshold = 0.8f; // 80% del presupuesto
    
    private List<GameObject> listItemInstances = new List<GameObject>();
    private bool isAnimating = false;
    private Coroutine warningCoroutine;
    
    // NUEVO: Variables para guardar el estado de visibilidad
    private bool moneyPanelWasActive;
    private bool shoppingListWasActive;
    private bool cartInfoWasActive;
    
    private void Awake()
    {
        // Ocultar el panel de dinero al inicio
        if (moneyPanel != null)
        {
            moneyPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Muestra el panel de dinero por primera vez.
    /// </summary>
    public void ShowMoneyPanel()
    {
        if (moneyPanel != null)
        {
            moneyPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Muestra la lista de compras.
    /// </summary>
    public void ShowShoppingList()
    {
        if (shoppingListPanel != null)
        {
            shoppingListPanel.SetActive(true);
        }
        
        // También mostrar el panel de dinero
        ShowMoneyPanel();
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
    /// NUEVO: Oculta todos los elementos de UI del supermercado.
    /// </summary>
    public void HideAllUI()
    {
        // Guardar estados actuales
        moneyPanelWasActive = moneyPanel != null && moneyPanel.activeSelf;
        shoppingListWasActive = shoppingListPanel != null && shoppingListPanel.activeSelf;
        cartInfoWasActive = cartInfoPanel != null && cartInfoPanel.activeSelf;
        
        // Ocultar todo
        if (moneyPanel != null)
            moneyPanel.SetActive(false);
        
        if (shoppingListPanel != null)
            shoppingListPanel.SetActive(false);
        
        if (cartInfoPanel != null)
            cartInfoPanel.SetActive(false);
        
        // Detener animaciones
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }
        
        Debug.Log("🔇 UI del supermercado ocultada para pantalla de felicitaciones");
    }
    
    /// <summary>
    /// NUEVO: Restaura todos los elementos de UI del supermercado a su estado anterior.
    /// </summary>
    public void ShowAllUI()
    {
        // Restaurar estados guardados
        if (moneyPanel != null)
            moneyPanel.SetActive(moneyPanelWasActive);
        
        if (shoppingListPanel != null)
            shoppingListPanel.SetActive(shoppingListWasActive);
        
        if (cartInfoPanel != null)
            cartInfoPanel.SetActive(cartInfoWasActive);
        
        Debug.Log("🔊 UI del supermercado restaurada");
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
    public void UpdateShoppingList(List<SupermarketManager.ShoppingListItem> items) // MODIFICADO
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
                // PASAR LA CANTIDAD COMPRADA Y REQUERIDA
                itemUI.Setup(item.itemName, item.itemPrice, item.purchasedQuantity, item.requiredQuantity, item.icon);
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
                cartTotalText.text = $"{cartTotalPrefix}0.00";
            
            if (cartItemCountText != null)
                cartItemCountText.text = "Items: 0";
            
            // Detener animación de advertencia
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }
            
            return;
        }
        
        float total = cart.GetTotalPrice();
        float budget = SupermarketManager.Instance != null ? SupermarketManager.Instance.CurrentMoney : 1200f;
        float percentage = total / budget;
        
        if (cartTotalText != null)
        {
            cartTotalText.text = $"{cartTotalPrefix}{total:F2}";
            
            // Cambiar color según el porcentaje
            if (total > budget)
            {
                cartTotalText.color = Color.red;
                
                // Iniciar animación de advertencia si no está activa
                if (warningCoroutine == null)
                {
                    warningCoroutine = StartCoroutine(WarningPulse(cartTotalText.rectTransform));
                }
            }
            else if (percentage >= warningThreshold)
            {
                cartTotalText.color = new Color(1f, 0.6f, 0f); // Naranja
                
                // Iniciar animación de advertencia si no está activa
                if (warningCoroutine == null)
                {
                    warningCoroutine = StartCoroutine(WarningPulse(cartTotalText.rectTransform));
                }
            }
            else
            {
                cartTotalText.color = Color.white;
                
                // Detener animación de advertencia
                if (warningCoroutine != null)
                {
                    StopCoroutine(warningCoroutine);
                    warningCoroutine = null;
                    cartTotalText.rectTransform.localScale = Vector3.one;
                }
            }
            
            // Animación de pulso al agregar item
            if (!isAnimating)
            {
                StartCoroutine(PulseAnimation(cartTotalText.rectTransform));
            }
        }
        
        if (cartItemCountText != null)
        {
            cartItemCountText.text = $"Items: {cart.ItemCount}";
        }
    }
    
    /// <summary>
    /// Animación de pulso para el texto del carrito.
    /// </summary>
    private IEnumerator PulseAnimation(RectTransform target)
    {
        isAnimating = true;
        
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * pulseScale;
        
        // Escalar hacia arriba
        float elapsed = 0f;
        while (elapsed < pulseDuration / 2)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (pulseDuration / 2);
            target.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        // Escalar hacia abajo
        elapsed = 0f;
        while (elapsed < pulseDuration / 2)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (pulseDuration / 2);
            target.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        target.localScale = originalScale;
        isAnimating = false;
    }
    
    /// <summary>
    /// Animación de advertencia constante cuando está cerca del límite.
    /// </summary>
    private IEnumerator WarningPulse(RectTransform target)
    {
        while (true)
        {
            // Escalar hacia arriba
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float scale = 1f + Mathf.Sin(elapsed * Mathf.PI) * 0.15f;
                target.localScale = Vector3.one * scale;
                yield return null;
            }
            
            yield return new WaitForSeconds(0.3f);
        }
    }
}