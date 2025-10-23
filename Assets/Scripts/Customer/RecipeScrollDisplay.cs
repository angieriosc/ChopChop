using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeScrollDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI recipeNameText;
    public Image pizzaDishIcon;
    public Image pizzaIcon;
    public TextMeshProUGUI pizzaCountText;
    public Image customersIcon;
    public TextMeshProUGUI customersCountText;
    
    [Header("Default Icons")]
    public Sprite defaultPizzaIcon; // Ícono genérico de pizza
    public Sprite defaultCustomersIcon; // Ícono de personitas
    
    void Start()
    {
        Debug.Log("🔍 RecipeScrollDisplay inicializado en: " + gameObject.name);
        VerifyReferences();
    }
    
    void VerifyReferences()
    {
        if (recipeNameText == null) Debug.LogWarning("⚠️ recipeNameText NO asignado en " + gameObject.name);
        if (pizzaDishIcon == null) Debug.LogWarning("⚠️ pizzaDishIcon NO asignado en " + gameObject.name);
        if (pizzaIcon == null) Debug.LogWarning("⚠️ pizzaIcon NO asignado en " + gameObject.name);
        if (pizzaCountText == null) Debug.LogWarning("⚠️ pizzaCountText NO asignado en " + gameObject.name);
        if (customersIcon == null) Debug.LogWarning("⚠️ customersIcon NO asignado en " + gameObject.name);
        if (customersCountText == null) Debug.LogWarning("⚠️ customersCountText NO asignado en " + gameObject.name);
        if (defaultPizzaIcon == null) Debug.LogWarning("⚠️ defaultPizzaIcon NO asignado en " + gameObject.name);
        if (defaultCustomersIcon == null) Debug.LogWarning("⚠️ defaultCustomersIcon NO asignado en " + gameObject.name);
    }
    
    public void DisplayRecipe(RecipeDataMenu recipe)
    {
        if (recipe == null)
        {
            Debug.LogError("❌ Recipe es NULL en RecipeScrollDisplay!");
            return;
        }
        
        Debug.Log("📜 === MOSTRANDO RESUMEN EN PERGAMINO ===");
        Debug.Log("Receta: " + recipe.recipeName);
        
        // Mostrar nombre de la receta
        if (recipeNameText != null)
        {
            recipeNameText.text = recipe.recipeName;
            recipeNameText.enabled = true;
            Debug.Log("  ✅ Nombre mostrado: " + recipe.recipeName);
        }
        else
        {
            Debug.LogError("  ❌ recipeNameText es NULL!");
        }
        
        // Mostrar ícono de pizza terminada (centro)
        if (pizzaDishIcon != null)
        {
            if (recipe.dishIcon != null)
            {
                pizzaDishIcon.sprite = recipe.dishIcon;
                pizzaDishIcon.enabled = true;
                pizzaDishIcon.color = Color.white;
                Debug.Log("  ✅ Ícono de pizza central: " + recipe.dishIcon.name);
            }
            else
            {
                pizzaDishIcon.enabled = false;
                Debug.LogWarning("  ⚠️ dishIcon es NULL en la receta");
            }
        }
        else
        {
            Debug.LogError("  ❌ pizzaDishIcon es NULL!");
        }
        
        // Mostrar ícono de pizza pequeño (izquierda)
        if (pizzaIcon != null)
        {
            if (defaultPizzaIcon != null)
            {
                pizzaIcon.sprite = defaultPizzaIcon;
                pizzaIcon.enabled = true;
                pizzaIcon.color = Color.white;
                Debug.Log("  ✅ Ícono de pizza pequeño asignado");
            }
            else
            {
                Debug.LogWarning("  ⚠️ defaultPizzaIcon NO está asignado");
            }
        }
        else
        {
            Debug.LogError("  ❌ pizzaIcon es NULL!");
        }
        
        // Mostrar cantidad de pizzas
        if (pizzaCountText != null)
        {
            pizzaCountText.text = recipe.numberOfPizzas.ToString();
            pizzaCountText.enabled = true;
            Debug.Log("  ✅ Cantidad de pizzas: " + recipe.numberOfPizzas);
        }
        else
        {
            Debug.LogError("  ❌ pizzaCountText es NULL!");
        }
        
        // Mostrar ícono de clientes (derecha)
        if (customersIcon != null)
        {
            if (defaultCustomersIcon != null)
            {
                customersIcon.sprite = defaultCustomersIcon;
                customersIcon.enabled = true;
                customersIcon.color = Color.white;
                Debug.Log("  ✅ Ícono de clientes asignado");
            }
            else
            {
                Debug.LogWarning("  ⚠️ defaultCustomersIcon NO está asignado");
            }
        }
        else
        {
            Debug.LogError("  ❌ customersIcon es NULL!");
        }
        
        // Mostrar número de clientes
        if (customersCountText != null)
        {
            customersCountText.text = recipe.numberOfCustomers.ToString();
            customersCountText.enabled = true;
            Debug.Log("  ✅ Número de clientes: " + recipe.numberOfCustomers);
        }
        else
        {
            Debug.LogError("  ❌ customersCountText es NULL!");
        }
        
        Debug.Log("✅ Pergamino actualizado completamente");
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("📜 Pergamino ocultado");
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("📜 Pergamino mostrado");
    }
}