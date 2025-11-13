using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Representa un elemento individual en la lista de compras UI.
/// </summary>
public class ShoppingListItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject checkmark;
    [SerializeField] private GameObject strikethrough;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color purchasedColor = Color.gray;
    
    /// <summary>
    /// Configura el item de la lista con la información proporcionada.
    /// </summary>
    public void Setup(string itemName, float price, bool isPurchased, Sprite icon = null)
    {
        // Configurar nombre
        if (itemNameText != null)
        {
            itemNameText.text = itemName;
            itemNameText.color = isPurchased ? purchasedColor : normalColor;
        }
        
        // Configurar precio
        if (itemPriceText != null)
        {
            itemPriceText.text = $"${price:F2}";
            itemPriceText.color = isPurchased ? purchasedColor : normalColor;
        }
        
        // Configurar icono
        if (itemIcon != null && icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.color = isPurchased ? purchasedColor : normalColor;
        }
        
        // Mostrar checkmark si está comprado
        if (checkmark != null)
        {
            checkmark.SetActive(isPurchased);
        }
        
        // Mostrar strikethrough si está comprado
        if (strikethrough != null)
        {
            strikethrough.SetActive(isPurchased);
        }
    }
    
    /// <summary>
    /// Marca el item como comprado.
    /// </summary>
    public void MarkAsPurchased()
    {
        if (itemNameText != null)
            itemNameText.color = purchasedColor;
        
        if (itemPriceText != null)
            itemPriceText.color = purchasedColor;
        
        if (itemIcon != null)
            itemIcon.color = purchasedColor;
        
        if (checkmark != null)
            checkmark.SetActive(true);
        
        if (strikethrough != null)
            strikethrough.SetActive(true);
    }
}