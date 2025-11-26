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
    [SerializeField] private TextMeshProUGUI itemQuantityText; // NUEVO: Para mostrar la cantidad
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject checkmark;
    [SerializeField] private GameObject strikethrough;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color purchasedColor = Color.gray;
    
    /// <summary>
    /// Configura el item de la lista con la información proporcionada.
    /// </summary>
    public void Setup(string itemName, float price, int purchasedQuantity, int requiredQuantity, Sprite icon = null) // MODIFICADO: Recibe ambas cantidades
    {
        bool isPurchased = purchasedQuantity >= requiredQuantity;
        
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
        
        // NUEVO: Configurar cantidad
        if (itemQuantityText != null)
        {
            itemQuantityText.text = $"({purchasedQuantity}/{requiredQuantity})";
            itemQuantityText.color = isPurchased ? purchasedColor : new Color(0.8f, 0.8f, 0.8f); // Color ligeramente diferente para la cantidad no completada
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
    
    // **NOTA:** El método MarkAsPurchased() ya no es relevante ya que la UI se actualiza con Setup
    // y se basa en el conteo, no en un simple llamado a marcar. Lo mantendré por si lo usas
    // en otra parte, aunque te recomiendo eliminarlo o refactorizarlo.
    
    /// <summary>
    /// Marca el item como comprado.
    /// </summary>
    public void MarkAsPurchased()
    {
        if (itemNameText != null)
            itemNameText.color = purchasedColor;
        
        if (itemPriceText != null)
            itemPriceText.color = purchasedColor;

        if (itemQuantityText != null) // NUEVO
            itemQuantityText.color = purchasedColor; 
        
        if (itemIcon != null)
            itemIcon.color = purchasedColor;
        
        if (checkmark != null)
            checkmark.SetActive(true);
        
        if (strikethrough != null)
            strikethrough.SetActive(true);
    }
}