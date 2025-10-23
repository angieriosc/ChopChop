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

    /// <summary>
    /// Devuelve la cantidad requerida (en mililitros) para un ingrediente dado.
    /// </summary>
    /// <param name="ingredientName">Nombre del ingrediente a buscar.</param>
    /// <returns>
    /// Cantidad en mililitros si existe en la receta; de lo contrario, 0.
    /// </returns>
    public float GetRequiredAmount(string ingredientName)
    {
        foreach (var ingredient in ingredients)
        {
            if (ingredient.ingredientName.Equals(ingredientName,
                                                 System.StringComparison.OrdinalIgnoreCase))
            {
                return ingredient.amountML;
            }
        }
        Debug.LogWarning($"Ingrediente '{ingredientName}' no encontrado en la receta '{recipeName}'.");
        return 0f;
    }

    public List<string> GetIngredientNames()
    {
        var names = new List<string>();
        foreach (var ingredient in ingredients)
        {
            names.Add(ingredient.ingredientName);
        }
        return names;
    }

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
