using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class IngredientItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image ingredientIcon;
    public Image checkboxImage; // Imagen del checkbox
    public Image cutIcon; // Ícono que muestra cómo está cortado
    public TextMeshProUGUI cutCountText; // Texto del número de cortes (OPCIONAL - puedes usar sprite)
    public Image cutCountIcon; // Ícono del número (alternativa al texto)
    public Image arrowIcon; // Flecha que indica "hacia"
    
    [Header("Checkbox Sprites")]
    public Sprite uncheckedSprite; // □ Vacío
    public Sprite checkedSprite; // ✓ Palomeado
    
    [Header("Tooltip")]
    public GameObject tooltipPanel; // Panel que aparece con el nombre
    public TextMeshProUGUI tooltipText;
    
   private IngredientStep currentStep;
    private bool isCompleted = false;
    private bool isPizzaFinal = false; 
    
    void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        
        // Inicializar checkbox
        UpdateCheckbox();
    }
    public void Initialize(IngredientStep step, bool isFinalPizza = false)
    {
        currentStep = step;
        isCompleted = step.isCompleted;
        isPizzaFinal = isFinalPizza; // NUEVO: Guardar si es pizza final
        
        Debug.Log("🔧 Inicializando: " + step.ingredient.ingredientName + " | Es pizza final: " + isFinalPizza);
        
        // ============ CONFIGURAR ÍCONO DEL INGREDIENTE ============
        if (ingredientIcon != null && step.ingredient != null)
        {
            ingredientIcon.sprite = step.ingredient.ingredientIcon;
            ingredientIcon.enabled = true;
            ingredientIcon.color = Color.white;
            Debug.Log("  ✅ Ícono principal asignado: " + step.ingredient.ingredientIcon.name);
        }
        else
        {
            if (ingredientIcon != null) ingredientIcon.enabled = false;
        }
        
        // ============ CONFIGURAR INFORMACIÓN DE CORTE ============
        if (step.requiresCutting && step.cutPieces > 0)
        {
            // -------- MOSTRAR FLECHA --------
            if (arrowIcon != null)
            {
                arrowIcon.enabled = true;
                arrowIcon.color = Color.white;
                Debug.Log("  ➡️ Flecha activada");
            }
            
            if (isPizzaFinal)
            {
                // ========== CASO: PIZZA FINAL ==========
                Debug.Log("  🍕 Configurando PIZZA FINAL");
                
                // OCULTAR cutIcon (no se usa en pizza final)
                if (cutIcon != null)
                {
                    cutIcon.enabled = false;
                    cutIcon.sprite = null;
                    Debug.Log("  ❌ cutIcon DESACTIVADO (pizza final no lo usa)");
                }
                
                // MOSTRAR cutCountIcon con el número de rebanadas de la pizza
                if (cutCountIcon != null)
                {
                    if (step.cutIconOverride != null)
                    {
                        cutCountIcon.sprite = step.cutIconOverride;
                        cutCountIcon.enabled = true;
                        cutCountIcon.color = Color.white;
                        Debug.Log("  ✅ cutCountIcon asignado (rebanadas pizza): " + step.cutIconOverride.name);
                    }
                    else
                    {
                        cutCountIcon.enabled = false;
                        Debug.LogWarning("  ⚠️ cutIconOverride es NULL para pizza final");
                    }
                }
            }
            else
            {
                // ========== CASO: INGREDIENTE NORMAL (Pimiento, Tomate, etc.) ==========
                Debug.Log("  🥬 Configurando INGREDIENTE CORTADO");
                
                // MOSTRAR cutIcon con el ícono del ingrediente cortado
                if (cutIcon != null)
                {
                    if (step.cutIconOverride != null)
                    {
                        cutIcon.sprite = step.cutIconOverride;
                        cutIcon.enabled = true;
                        cutIcon.color = Color.white;
                        Debug.Log("  ✅ cutIcon asignado (ingrediente cortado): " + step.cutIconOverride.name);
                    }
                    else if (step.ingredient.ingredientSlicedIcon != null)
                    {
                        cutIcon.sprite = step.ingredient.ingredientSlicedIcon;
                        cutIcon.enabled = true;
                        cutIcon.color = Color.white;
                        Debug.Log("  ✅ cutIcon asignado (desde ingrediente): " + step.ingredient.ingredientSlicedIcon.name);
                    }
                    else
                    {
                        cutIcon.enabled = false;
                        Debug.LogWarning("  ⚠️ No hay sprite de corte para el ingrediente");
                    }
                }
                
                // OCULTAR cutCountIcon (no se usa en ingredientes normales)
                if (cutCountIcon != null)
                {
                    cutCountIcon.enabled = false;
                    cutCountIcon.sprite = null;
                    Debug.Log("  ❌ cutCountIcon DESACTIVADO (ingrediente normal no lo usa)");
                }
            }
        }
        else
        {
            // ========== CASO: INGREDIENTE SIN CORTE (Masa, Salsa, Queso, etc.) ==========
            Debug.Log("  ℹ️ Ingrediente SIN CORTE");
            
            if (arrowIcon != null)
            {
                arrowIcon.enabled = false;
                arrowIcon.sprite = null;
            }
            
            if (cutIcon != null)
            {
                cutIcon.enabled = false;
                cutIcon.sprite = null;
            }
            
            if (cutCountIcon != null)
            {
                cutCountIcon.enabled = false;
                cutCountIcon.sprite = null;
            }
        }
        
        UpdateCheckbox();
        Debug.Log("✅ Inicialización completa\n");
    }
    
    public void ToggleCompletion()
    {
        isCompleted = !isCompleted;
        
        if (currentStep != null)
        {
            currentStep.isCompleted = isCompleted;
        }
        
        UpdateCheckbox();
        
        string ingredientName = currentStep != null && currentStep.ingredient != null 
            ? currentStep.ingredient.ingredientName 
            : "Ingrediente";
            
        Debug.Log("✓ " + ingredientName + " marcado como " + (isCompleted ? "COMPLETADO ✅" : "PENDIENTE ⏳"));
    }
    
    void UpdateCheckbox()
    {
        if (checkboxImage != null)
        {
            checkboxImage.sprite = isCompleted ? checkedSprite : uncheckedSprite;
            checkboxImage.enabled = true;
            checkboxImage.color = Color.white;
        }
    }
    
    // Mostrar tooltip cuando el mouse entra
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null && currentStep != null && currentStep.ingredient != null)
        {
            tooltipPanel.SetActive(true);
            
            if (tooltipText != null)
            {
                string displayText = currentStep.ingredient.ingredientName;
                
                // Si requiere corte, agregar info adicional
                if (currentStep.requiresCutting && currentStep.cutPieces > 0)
                {
                    displayText += " (Cortar en " + currentStep.cutPieces + ")";
                }
                
                tooltipText.text = displayText;
            }
        }
    }
    
    // Ocultar tooltip cuando el mouse sale
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    
    public bool IsCompleted()
    {
        return isCompleted;
    }
    
    public IngredientStep GetIngredientStep()
    {
        return currentStep;
    }
}