using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contiene la información de una receta: nombre e ingredientes requeridos.
/// </summary>
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Nombre de la receta")]
    [Tooltip("Nombre que identifica la receta.")]
    public string recipeName;

    [Header("Ingredientes requeridos")]
    [Tooltip("Lista de ingredientes y cantidades necesarias.")]
    public List<IngredientRequirement> ingredients = new List<IngredientRequirement>();
}

/// <summary>
/// Define un ingrediente y la cantidad necesaria en mililitros.
/// </summary>
[System.Serializable]
public class IngredientRequirement
{
    [Tooltip("Nombre del ingrediente requerido.")]
    public string ingredientName;

    [Tooltip("Cantidad requerida en mililitros.")]
    public float amountML;
}
