using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(InteractableObject))]
public class ReceivingContainer : MonoBehaviour
{
    [Header("Receta asignada")]
    [SerializeField] public RecipeData currentRecipe;

    [Header("CupTracker (para notificar cambios de cantidad)")]
    [Tooltip("Asignar el CupTracker desde el inspector")]
    [SerializeField] private CupTracker cupTracker;

    // Diccionario para llevar la cantidad actual por ingrediente
    private Dictionary<string, float> ingredientAmounts = new Dictionary<string, float>();

    /// <summary>
    /// Asigna una receta al recipiente.
    /// </summary>
    public void AssignRecipe(RecipeData recipe)
    {
        currentRecipe = recipe;
        ingredientAmounts.Clear();

        if (recipe != null)
        {
            // Inicializar todos los ingredientes en 0
            foreach (var ing in recipe.ingredients)
            {
                ingredientAmounts[ing.ingredientName] = 0f;
            }
        }
    }

    /// <summary>
    /// Devuelve la cantidad actual de un ingrediente.
    /// </summary>
    public float GetIngredientAmount(string ingredientName)
    {
        if (ingredientAmounts.TryGetValue(ingredientName, out float amount))
            return amount;
        // si no existe, devolver 0 (no lo inicializamos)
        return 0f;
    }

    /// <summary>
    /// Agrega líquido al ingrediente especificado.
    /// </summary>
    public void AddLiquid(string ingredientName, float amountML)
    {
        if (currentRecipe == null)
        {
            Debug.LogWarning("No hay receta asignada - AddLiquid no hará nada.");
            return;
        }

        if (!ingredientAmounts.ContainsKey(ingredientName))
        {
            // Si el ingrediente no estaba en la receta, opcionalmente añadirlo:
            ingredientAmounts[ingredientName] = 0f;
        }

        ingredientAmounts[ingredientName] += amountML;

        // No exceder la cantidad requerida de la receta (si quieres permitir overflow, quita esto)
        float max = currentRecipe.GetRequiredAmount(ingredientName);
        if (ingredientAmounts[ingredientName] > max)
            ingredientAmounts[ingredientName] = max;

    }

    /// <summary>
    /// Resetea todas las cantidades de los ingredientes.
    /// </summary>
    public void ResetIngredients()
    {
        List<string> keys = new List<string>(ingredientAmounts.Keys);
        foreach (var key in keys)
            ingredientAmounts[key] = 0f;

    }

    /// <summary>
    /// Devuelve la receta asignada actualmente.
    /// </summary>
    public RecipeData GetRecipe()
    {
        return currentRecipe;
    }
}
