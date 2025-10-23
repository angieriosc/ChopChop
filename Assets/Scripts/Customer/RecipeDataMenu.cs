using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Restaurant/Recipe")]
public class RecipeDataMenu : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName;
    public int numberOfPizzas = 1;
    public int numberOfCustomers;
    
    [Header("Visual Settings")]
    public Sprite recipeScrollImage;
    public Sprite dishIcon; // Pizza terminada (sin cortar)
    public Sprite slicedPizzaIcon; // Pizza cortada en rebanadas
    public int pizzaSlices = 6; // Número de rebanadas (1, 2, 4, 6, 8, etc.)
    
    [Header("Slice Number Icons")]
    public Sprite sliceNumberIcon; // Ícono que muestra el número de rebanadas (1, 2, 3, 4, 6, 8, etc.)
    
    [Header("Ingredients")]
    public List<IngredientStep> ingredientSteps = new List<IngredientStep>();
}

[System.Serializable]
public class IngredientStep
{
    [Header("Ingredient Info")]
    public Ingredient ingredient; // Referencia al ingrediente base
    
    [Header("Processing")]
    public bool requiresCutting = false; // ¿Necesita cortarse?
    public int cutPieces = 0; // Número de piezas (0 = no se corta, 6 = 6 rebanadas, etc.)
    public Sprite cutIconOverride; // Ícono personalizado del ingrediente cortado (opcional)
    
    [Header("Completion State")]
    public bool isCompleted = false; // Se marcará en runtime
}