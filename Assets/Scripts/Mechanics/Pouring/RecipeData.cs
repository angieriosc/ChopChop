using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Representa los datos de una receta utilizada en el sistema de cocina.
/// Incluye el nombre de la receta y los ingredientes necesarios con sus
/// respectivas cantidades.
/// </summary>
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Nombre de la receta")]
    [Tooltip("Nombre que identifica la receta en el sistema.")]
    public string recipeName;

    [Header("Ingredientes requeridos")]
    [Tooltip("Lista de ingredientes y las cantidades necesarias (en mililitros).")]
    public List<IngredientRequirement> ingredients =
        new List<IngredientRequirement>();

    /// <summary>
    /// Obtiene la cantidad requerida en mililitros para un ingrediente
    /// específico dentro de la receta.
    /// </summary>
    /// <param name="ingredientName">
    /// Nombre del ingrediente que se desea consultar.
    /// </param>
    /// <returns>
    /// Devuelve la cantidad en mililitros si el ingrediente se encuentra
    /// en la receta; en caso contrario, devuelve 0.
    /// </returns>
    public float GetRequiredAmount(string ingredientName)
    {
        foreach (var ingredient in ingredients)
        {
            if (ingredient.ingredientName.Equals(
                    ingredientName,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return ingredient.amountML;
            }
        }

        Debug.LogWarning(
            $"Ingrediente '{ingredientName}' no encontrado en la receta '{recipeName}'.");
        return 0f;
    }

    /// <summary>
    /// Devuelve una lista con los nombres de todos los ingredientes
    /// requeridos por la receta.
    /// </summary>
    /// <returns>
    /// Lista de cadenas que contiene los nombres de los ingredientes.
    /// </returns>
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
/// Define los datos de un ingrediente dentro de una receta,
/// incluyendo su nombre y la cantidad requerida en mililitros.
/// </summary>
[System.Serializable]
public class IngredientRequirement
{
    [Tooltip("Nombre del ingrediente requerido por la receta.")]
    public string ingredientName;

    [Tooltip("Cantidad necesaria de este ingrediente en mililitros.")]
    public float amountML;
}
