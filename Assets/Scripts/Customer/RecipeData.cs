using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Restaurant/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName;
    public Sprite recipeScrollImage; // Imagen del pergamino con la receta
    public int numberOfCustomers; // Número de personas (1, 2, 4, 6, etc.)
    
    [Header("Ingredients")]
    public List<Ingredient> ingredients = new List<Ingredient>();
    
    [Header("Visual Settings")]
    public Sprite dishIcon; // Ícono del platillo final
}