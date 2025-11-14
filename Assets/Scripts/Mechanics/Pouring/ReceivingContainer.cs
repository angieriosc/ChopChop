using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Gestiona un recipiente que recibe ingredientes líquidos para 
/// completar una receta asignada.
/// </summary>
/// <remarks>
/// Mantiene un registro de las cantidades añadidas de cada ingrediente, 
/// valida contra los valores requeridos por la receta, 
/// y permite reiniciar los valores acumulados.
/// </remarks>
[RequireComponent(typeof(InteractableObject))]
public class ReceivingContainer : MonoBehaviour
{
    [Header("Receta asignada")]
    [Tooltip("Receta que define los ingredientes y cantidades requeridas.")]
    [SerializeField] 
    public RecipeData currentRecipe;

    [Header("CupTracker (para notificar cambios)")]
    [Tooltip("Referencia al componente CupTracker que detecta variaciones.")]
    [SerializeField] 
    public CupTracker cupTracker;

    // Cantidades actuales registradas por nombre de ingrediente.
    private Dictionary<string, float> ingredientAmounts =
        new Dictionary<string, float>();

    /// <summary>
    /// Asigna una nueva receta al recipiente y reinicia los valores acumulados.
    /// </summary>
    /// <param name="recipe">Instancia de la receta que se va a asignar.</param>
    public void AssignRecipe(RecipeData recipe)
    {
        currentRecipe = recipe;
        ingredientAmounts.Clear();

        if (recipe == null) return;

        // Inicializa todos los ingredientes de la receta en cero.
        foreach (var ing in recipe.ingredients)
        {
            ingredientAmounts[ing.ingredientName] = 0f;
        }
    }

    /// <summary>
    /// Obtiene la cantidad actual de un ingrediente en el recipiente.
    /// </summary>
    /// <param name="ingredientName">Nombre del ingrediente a consultar.</param>
    /// <returns>
    /// Cantidad actual en mililitros si existe; de lo contrario, 0.
    /// </returns>
    public float GetIngredientAmount(string ingredientName)
    {
        return ingredientAmounts.TryGetValue(ingredientName, out float amount)
            ? amount
            : 0f;
    }

    /// <summary>
    /// Agrega una cantidad específica de líquido al ingrediente indicado.
    /// </summary>
    /// <param name="ingredientName">Nombre del ingrediente a agregar.</param>
    /// <param name="amountML">Cantidad en mililitros a añadir.</param>
    /// <remarks>
    /// Si el ingrediente no forma parte de la receta, se añade 
    /// automáticamente al diccionario con valor inicial 0.
    /// </remarks>
    public void AddLiquid(string ingredientName, float amountML)
    {
        if (currentRecipe == null)
        {
            Debug.LogWarning(
                "No hay receta asignada. AddLiquid no ejecutará ninguna acción."
            );
            return;
        }

        if (!ingredientAmounts.ContainsKey(ingredientName))
        {
            ingredientAmounts[ingredientName] = 0f;
        }

        ingredientAmounts[ingredientName] += amountML;

        // Limita el valor máximo según la cantidad requerida.
        float max = currentRecipe.GetRequiredAmount(ingredientName);
        if (ingredientAmounts[ingredientName] > max)
        {
            ingredientAmounts[ingredientName] = max;
        }
    }

    /// <summary>
    /// Reinicia todas las cantidades de los ingredientes a cero.
    /// </summary>
    public void ResetIngredients()
    {
        var keys = new List<string>(ingredientAmounts.Keys);
        foreach (var key in keys)
        {
            ingredientAmounts[key] = 0f;
        }
    }

    /// <summary>
    /// Devuelve la receta actualmente asignada al recipiente.
    /// </summary>
    /// <returns>Instancia de la receta actual o null si no hay asignada.</returns>
    public RecipeData GetRecipe()
    {
        return currentRecipe;
    }
}
