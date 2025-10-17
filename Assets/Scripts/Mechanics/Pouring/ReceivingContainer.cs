using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Recipiente que recibe líquidos de los PouringContainers.
/// Maneja mezcla de ingredientes, UI y seguimiento de recetas.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class ReceivingContainer : MonoBehaviour
{
    [Header("Capacity (ml)")]
    [Tooltip("Capacidad máxima del recipiente en mililitros.")]
    [SerializeField] private float capacityML = 1000f;

    [Header("UI Text (Optional)")]
    [Tooltip("Texto para mostrar cantidad total e ingredientes.")]
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Recipe Data")]
    [Tooltip("Receta activa asignada al recipiente.")]
    public RecipeData currentRecipe;

    private Dictionary<string, float> ingredients = new Dictionary<string, float>();
    private InteractableObject interactable;

    /// <summary>
    /// Cantidad total de líquido actual en el recipiente.
    /// </summary>
    private float TotalML
    {
        get
        {
            float total = 0f;
            foreach (var ing in ingredients.Values) total += ing;
            return total;
        }
    }

    private void Start()
    {
        interactable = GetComponent<InteractableObject>();

        // Asegurar que el recipiente pueda mezclarse
        if (!interactable.HasCapability(ObjectCapabilities.Mixable))
            interactable.capabilities |= ObjectCapabilities.Mixable;

        UpdateVisual();
    }

    /// <summary>
    /// Asigna una receta al recipiente y reinicia ingredientes.
    /// </summary>
    /// <param name="recipe">Receta a asignar.</param>
    public void AssignRecipe(RecipeData recipe)
    {
        currentRecipe = recipe;
        ingredients.Clear();
        UpdateVisual();

        Debug.Log($"[ReceivingContainer] Receta asignada: {recipe.recipeName}");
    }

    /// <summary>
    /// Agrega líquido de un ingrediente al recipiente.
    /// </summary>
    /// <param name="ingredient">Nombre del ingrediente.</param>
    /// <param name="amount">Cantidad en ml.</param>
    public void AddLiquid(string ingredient, float amount)
    {
        if (TotalML + amount > capacityML)
            amount = capacityML - TotalML;

        if (!ingredients.ContainsKey(ingredient)) ingredients[ingredient] = 0f;
        ingredients[ingredient] += amount;

        UpdateVisual();
        CheckRecipeProgress();
    }

    /// <summary>
    /// Verifica si los ingredientes coinciden con la receta actual.
    /// </summary>
    private void CheckRecipeProgress()
    {
        if (currentRecipe == null) return;

        bool allMatch = true;
        foreach (var req in currentRecipe.ingredients)
        {
            if (!ingredients.ContainsKey(req.ingredientName) ||
                ingredients[req.ingredientName] < req.amountML)
            {
                allMatch = false;
                break;
            }
        }

        if (allMatch)
        {
            Debug.Log($"✅ Receta completada: {currentRecipe.recipeName}");
        }
    }

    /// <summary>
    /// Actualiza la UI del recipiente mostrando ingredientes y total.
    /// </summary>
    private void UpdateVisual()
    {
        if (amountText == null) return;

        string text = currentRecipe != null
            ? $"Receta: {currentRecipe.recipeName}\n"
            : "";

        text += $"Total: {TotalML:F0} ml\n";
        foreach (var kvp in ingredients)
            text += $"{kvp.Key}: {kvp.Value:F0} ml\n";

        amountText.text = text;
    }
}
