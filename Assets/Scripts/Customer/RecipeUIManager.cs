using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RecipeUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject recipePanel; // Panel completo con la receta extendida (pergamino abierto)
    public GameObject recipePanelOff; // Pergamino enrollado (versión colapsada)
    
    [Header("Ingredient Settings")]
    public GameObject ingredientItemPrefab; // Prefab para cada ingrediente
    public Transform ingredientsContainer; // Contenedor donde se colocan los ingredientes
    
    [Header("UI Elements")]
    public TextMeshProUGUI recipeNameText; // Nombre de la receta
    public Button toggleButton; // Botón para mostrar/ocultar
    
    [Header("Button Animation")]
    public RectTransform buttonRectTransform; // RectTransform del botón (se autoasigna)
    public Vector2 buttonPositionExpanded = new Vector2(-30, -30); // Posición cuando está extendido
    public Vector2 buttonPositionCollapsed = new Vector2(-30, -100); // Posición cuando está enrollado
    
    [Header("Button Icons (Optional)")]
    public Sprite buttonIconExpanded; // Ícono cuando está extendido (ej: flecha abajo ▼)
    public Sprite buttonIconCollapsed; // Ícono cuando está enrollado (ej: flecha arriba ▲)
    
    private bool isExpanded = true;
    private List<GameObject> currentIngredientItems = new List<GameObject>();
    private Image buttonImage; // Componente Image del botón
    
    void Start()
    {
        // Inicializar ambos paneles ocultos
        if (recipePanel != null)
        {
            recipePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("❌ RecipePanel NO está asignado!");
        }
        
        if (recipePanelOff != null)
        {
            recipePanelOff.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ RecipePanelOff NO está asignado - La funcionalidad de enrollar no funcionará");
        }
        
        // OCULTAR el botón al inicio
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(false);
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(ToggleRecipeVisibility);
            
            // Si no se asignó el RectTransform, obtenerlo automáticamente
            if (buttonRectTransform == null)
            {
                buttonRectTransform = toggleButton.GetComponent<RectTransform>();
                Debug.Log("✅ Button RectTransform auto-asignado");
            }
            
            // Obtener el componente Image del botón
            buttonImage = toggleButton.GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning("⚠️ ToggleButton NO está asignado");
        }
        
        Debug.Log("✅ RecipeUIManager inicializado - Botón OCULTO");
        VerifyReferences();
    }
    
    void VerifyReferences()
    {
        Debug.Log("=== VERIFICANDO REFERENCIAS ===");
        
        if (recipePanel == null) 
            Debug.LogError("❌ Recipe Panel NO asignado!");
        else
            Debug.Log("✅ Recipe Panel: " + recipePanel.name);
            
        if (recipePanelOff == null) 
            Debug.LogWarning("⚠️ Recipe Panel Off NO asignado! No podrás enrollar el pergamino");
        else
            Debug.Log("✅ Recipe Panel Off: " + recipePanelOff.name);
            
        if (ingredientItemPrefab == null) 
            Debug.LogError("❌ Ingredient Item Prefab NO asignado!");
        else
            Debug.Log("✅ Ingredient Item Prefab asignado");
            
        if (ingredientsContainer == null) 
            Debug.LogError("❌ Ingredients Container NO asignado!");
        else
            Debug.Log("✅ Ingredients Container: " + ingredientsContainer.name);
            
        if (recipeNameText == null) 
            Debug.LogWarning("⚠️ Recipe Name Text NO asignado!");
        else
            Debug.Log("✅ Recipe Name Text asignado");
            
        if (toggleButton == null) 
            Debug.LogWarning("⚠️ Toggle Button NO asignado!");
        else
            Debug.Log("✅ Toggle Button: " + toggleButton.name);
            
        if (buttonRectTransform == null) 
            Debug.LogWarning("⚠️ Button RectTransform NO encontrado!");
        else
            Debug.Log("✅ Button RectTransform asignado");
            
        Debug.Log("=== FIN VERIFICACIÓN ===");
    }
    
    public void DisplayRecipe(RecipeDataMenu recipe)
    {
        if (recipePanel == null)
        {
            Debug.LogError("❌ Recipe Panel es NULL! No se puede mostrar la receta.");
            return;
        }
        
        if (recipe == null)
        {
            Debug.LogError("❌ Recipe es NULL!");
            return;
        }
        
        Debug.Log("=== MOSTRANDO RECETA: " + recipe.recipeName + " ===");
        
        // Limpiar ingredientes anteriores
        ClearIngredients();
        
        // Mostrar el panel extendido, ocultar el enrollado
        recipePanel.SetActive(true);
        
        if (recipePanelOff != null)
        {
            recipePanelOff.SetActive(false);
        }
        
        // MOSTRAR EL BOTÓN cuando aparece la receta
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(true);
            Debug.Log("🔘 Botón ACTIVADO");
        }
        
        isExpanded = true;
        
        // Mover el botón a la posición extendida
        if (buttonRectTransform != null)
        {
            buttonRectTransform.anchoredPosition = buttonPositionExpanded;
            Debug.Log("🔘 Botón movido a posición EXTENDIDA: " + buttonPositionExpanded);
        }
        
        // Actualizar ícono del botón si está configurado
        UpdateButtonIcon();
        
        // Actualizar nombre de la receta
        if (recipeNameText != null)
        {
            recipeNameText.text = recipe.recipeName;
            recipeNameText.enabled = true;
            Debug.Log("✅ Nombre de receta actualizado: " + recipeNameText.text);
        }
        
        // Verificar que hay ingredientes
        if (recipe.ingredientSteps == null || recipe.ingredientSteps.Count == 0)
        {
            Debug.LogWarning("⚠️ La receta no tiene ingredientes!");
            return;
        }
        
        Debug.Log("📝 Creando " + recipe.ingredientSteps.Count + " ingredientes...");
        
        // Crear elementos de ingredientes
        int index = 0;
        foreach (IngredientStep step in recipe.ingredientSteps)
        {
            if (step != null && step.ingredient != null)
            {
                CreateIngredientItem(step, index);
                index++;
            }
        }
        
        // AGREGAR PIZZA FINAL AL FINAL
        CreateFinalPizzaItem(recipe);
        
        Debug.Log("✅ Receta mostrada completamente - Estado: EXTENDIDA");
    }
    
    void CreateIngredientItem(IngredientStep step, int index)
{
    if (ingredientItemPrefab == null || ingredientsContainer == null)
    {
        Debug.LogError("❌ Ingredient Item Prefab o Container es NULL!");
        return;
    }
    
    Debug.Log("  Creando ingrediente " + (index + 1) + ": " + step.ingredient.ingredientName);
    
    GameObject item = Instantiate(ingredientItemPrefab, ingredientsContainer);
    item.SetActive(true);
    currentIngredientItems.Add(item);
    
    // Obtener el componente IngredientItemUI
    IngredientItemUI itemUI = item.GetComponent<IngredientItemUI>();
    if (itemUI != null)
    {
        // IMPORTANTE: Pasar false porque NO es pizza final
        itemUI.Initialize(step, false);
        
        // Agregar listener al botón para toggle
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => itemUI.ToggleCompletion());
            Debug.Log("    ✅ Listener de toggle agregado");
        }
    }
    else
    {
        Debug.LogError("    ❌ El prefab no tiene IngredientItemUI!");
    }
    
    Canvas.ForceUpdateCanvases();
}

void CreateFinalPizzaItem(RecipeDataMenu recipe)
{
    if (ingredientItemPrefab == null || ingredientsContainer == null)
    {
        Debug.LogError("❌ No se puede crear pizza final - Prefab o Container es NULL");
        return;
    }
    
    Debug.Log("🍕 Agregando pizza final al final de la lista...");
    
    GameObject item = Instantiate(ingredientItemPrefab, ingredientsContainer);
    item.SetActive(true);
    currentIngredientItems.Add(item);
    
    // Configurar el ítem como pizza final
    IngredientItemUI itemUI = item.GetComponent<IngredientItemUI>();
    if (itemUI != null)
    {
        // Crear un step temporal para la pizza final
        IngredientStep pizzaStep = new IngredientStep();
        pizzaStep.ingredient = new Ingredient("Pizza Completa", recipe.dishIcon);
        pizzaStep.requiresCutting = true;
        pizzaStep.cutPieces = recipe.pizzaSlices;
        pizzaStep.cutIconOverride = recipe.slicedPizzaIcon; // Este será el número de rebanadas
        pizzaStep.isCompleted = false;
        
        // IMPORTANTE: Pasar true porque ES la pizza final
        itemUI.Initialize(pizzaStep, true);
        
        // Agregar listener para marcar como completo
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => itemUI.ToggleCompletion());
            Debug.Log("    ✅ Pizza final agregada con " + recipe.pizzaSlices + " rebanadas");
        }
    }
    else
    {
        Debug.LogError("    ❌ El prefab no tiene IngredientItemUI para la pizza final!");
    }
    
    Canvas.ForceUpdateCanvases();
}
    
    void ClearIngredients()
    {
        foreach (GameObject item in currentIngredientItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        currentIngredientItems.Clear();
        Debug.Log("🗑️ Ingredientes anteriores limpiados");
    }
    
    public void ToggleRecipeVisibility()
    {
        if (recipePanel == null)
        {
            Debug.LogError("❌ RecipePanel es NULL!");
            return;
        }
        
        // ASEGURAR que el botón siempre esté activo
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(true);
        }
        
        // Cambiar estado
        isExpanded = !isExpanded;
        
        if (isExpanded)
        {
            // ========== EXTENDER PERGAMINO ==========
            recipePanel.SetActive(true);
            
            if (recipePanelOff != null)
            {
                recipePanelOff.SetActive(false);
            }
            
            // Mover botón a posición extendida
            if (buttonRectTransform != null)
            {
                buttonRectTransform.anchoredPosition = buttonPositionExpanded;
                Debug.Log("🔘 Botón movido a: " + buttonPositionExpanded);
            }
            
            Debug.Log("📜 Pergamino EXTENDIDO");
        }
        else
        {
            // ========== ENROLLAR PERGAMINO ==========
            recipePanel.SetActive(false);
            
            if (recipePanelOff != null)
            {
                recipePanelOff.SetActive(true);
            }
            else
            {
                Debug.LogWarning("⚠️ RecipePanelOff es NULL, solo se ocultará el panel principal");
            }
            
            // Mover botón a posición enrollada
            if (buttonRectTransform != null)
            {
                buttonRectTransform.anchoredPosition = buttonPositionCollapsed;
                Debug.Log("🔘 Botón movido a: " + buttonPositionCollapsed);
            }
            
            Debug.Log("📜 Pergamino ENROLLADO");
        }
        
        // Actualizar ícono del botón
        UpdateButtonIcon();
        
        // VERIFICAR que el botón sigue activo después del cambio
        if (toggleButton != null && !toggleButton.gameObject.activeSelf)
        {
            Debug.LogError("❌ ¡El botón se desactivó! Reactivándolo...");
            toggleButton.gameObject.SetActive(true);
        }
    }
    
    void UpdateButtonIcon()
    {
        if (buttonImage == null) return;
        
        if (isExpanded && buttonIconExpanded != null)
        {
            buttonImage.sprite = buttonIconExpanded;
            Debug.Log("🔽 Ícono cambiado a EXTENDIDO");
        }
        else if (!isExpanded && buttonIconCollapsed != null)
        {
            buttonImage.sprite = buttonIconCollapsed;
            Debug.Log("🔼 Ícono cambiado a ENROLLADO");
        }
    }
    
    public void HideRecipe()
    {
        if (recipePanel != null)
        {
            recipePanel.SetActive(false);
        }
        
        if (recipePanelOff != null)
        {
            recipePanelOff.SetActive(false);
        }
        
        // OCULTAR EL BOTÓN cuando se oculta la receta
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(false);
            Debug.Log("🔘 Botón DESACTIVADO");
        }
        
        ClearIngredients();
        Debug.Log("❌ Receta ocultada completamente");
    }
    
    // Método útil para verificar si todos los ingredientes están completos
    public bool AreAllIngredientsCompleted()
    {
        foreach (GameObject item in currentIngredientItems)
        {
            if (item != null)
            {
                IngredientItemUI itemUI = item.GetComponent<IngredientItemUI>();
                if (itemUI != null && !itemUI.IsCompleted())
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    // Método para obtener el progreso de la receta
    public float GetRecipeProgress()
    {
        if (currentIngredientItems.Count == 0) return 0f;
        
        int completedCount = 0;
        foreach (GameObject item in currentIngredientItems)
        {
            if (item != null)
            {
                IngredientItemUI itemUI = item.GetComponent<IngredientItemUI>();
                if (itemUI != null && itemUI.IsCompleted())
                {
                    completedCount++;
                }
            }
        }
        
        return (float)completedCount / currentIngredientItems.Count;
    }
}