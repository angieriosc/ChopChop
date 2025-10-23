using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la UI de selección y detalle de recetas,
/// asignando la receta al recipiente activo en la estación.
/// </summary>
public class RecipeMenu : MonoBehaviour
{
    [Header("Recetas disponibles")]
    [Tooltip("Lista de recetas disponibles en el menú.")]
    [SerializeField] private RecipeData[] recipes;

    [Header("UI principal")]
    [Tooltip("ScrollView principal con los botones de recetas.")]
    [SerializeField] private GameObject scrollView;

    [Tooltip("Botones para seleccionar cada receta.")]
    [SerializeField] private Button[] recipeButtons;

    [Header("Estación actual")]
    [Tooltip("Estación de vertido que contiene el recipiente activo.")]
    [SerializeField] private PouringStation pouringStation;

    [Header("UI de detalles de receta")]
    [Tooltip("Panel que muestra los detalles de la receta seleccionada.")]
    [SerializeField] private GameObject recipeDetailsPanel;

    [Tooltip("Texto para mostrar ingredientes de la receta.")]
    [SerializeField] private TextMeshProUGUI ingredientsText;

    [Tooltip("Botón para volver al listado de recetas.")]
    [SerializeField] private Button backButton;

    private RecipeData currentRecipe;

    private void Start()
    {
        // Asignar listeners a los botones de receta
        for (int i = 0; i < recipeButtons.Length && i < recipes.Length; i++)
        {
            int index = i;
            recipeButtons[i].onClick.AddListener(() => OnSelectRecipe(index));

            // Mostrar nombre en el texto del botón
            TextMeshProUGUI buttonText = recipeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = recipes[i].recipeName;
        }

        // Listener del botón de volver
        if (backButton != null)
            backButton.onClick.AddListener(OnBackToList);

        // Asegurar estados iniciales
        recipeDetailsPanel?.SetActive(false);
        scrollView?.SetActive(true);
    }

    /// <summary>
    /// Asigna la receta seleccionada al recipiente activo y muestra detalles.
    /// </summary>
    /// <param name="index">Índice de la receta en el array.</param>
    private void OnSelectRecipe(int index)
    {
        if (index < 0 || index >= recipes.Length) return;

        RecipeData selected = recipes[index];
        currentRecipe = selected;

        // 🔹 Asignar receta a la estación
        if (pouringStation != null)
            pouringStation.SetActiveRecipe(selected);

        // Asignar receta al recipiente activo de la estación
        if (pouringStation?.CurrentReceiving != null)
        {
            pouringStation.CurrentReceiving.AssignRecipe(selected);
        }

        Object.FindFirstObjectByType<RecipeController>()?.StartRecipeFlow(selected);


        ShowRecipeDetails(selected);
    }

    /// <summary>
    /// Muestra el panel de detalles de la receta seleccionada.
    /// </summary>
    /// <param name="recipe">Receta a mostrar.</param>
    private void ShowRecipeDetails(RecipeData recipe)
    {
        if (recipeDetailsPanel == null || ingredientsText == null) return;

        // Ocultar ScrollView principal
        scrollView?.SetActive(false);

        // Mostrar panel de detalles
        recipeDetailsPanel.SetActive(true);

        // Construir texto con ingredientes
        string info = $"<b>{recipe.recipeName}</b>\n\nIngredientes requeridos:\n";
        foreach (var ing in recipe.ingredients)
            info += $"- {ing.ingredientName}: {ing.amountML:F0} ml\n";

        ingredientsText.text = info;
    }

    /// <summary>
    /// Vuelve al listado de recetas ocultando panel de detalles.
    /// </summary>
    private void OnBackToList()
    {
        scrollView?.SetActive(true);
        recipeDetailsPanel?.SetActive(false);

        Debug.Log("Volviendo a la lista de recetas.");
    }
}
