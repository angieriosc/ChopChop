using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

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

    [SerializeField] private GameObject stateView;

    [Tooltip("Texto para mostrar ingredientes de la receta.")]
    [SerializeField] private TextMeshProUGUI ingredientsText;

    [Tooltip("Botón para volver al listado de recetas.")]
    [SerializeField] private Button backButton;

    [Tooltip("UI del manejo de fracciones")]
    [SerializeField] public GameObject fractionPanel;

    [Header("Botón de salida de la estación")]

    [SerializeField] private Button exitButton;

    [Header("Controlador de recetas")]
    public RecipeController recipeController;


    /// <summary>Valores equivalentes de cada fracción en mililitros.</summary>
    private readonly float[] cupValues = { 500f, 333f, 250f, 200f, 166.5f };

    // <summary> Lista de cantidades necesarias
    public Dictionary<string, string> ingredientRequireStrings = new Dictionary<string, string>();


    private RecipeData currentRecipe;

    private void Start()
    {
        recipeController = FindFirstObjectByType<RecipeController>();
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

        // Listerner del botón de salir
        if (exitButton != null) {
            exitButton.onClick.AddListener(ExitMenu);
        }

        // Asegurar estados iniciales
        recipeDetailsPanel?.SetActive(false);
        scrollView?.SetActive(true);
        fractionPanel?.SetActive(false);
        backButton.gameObject.SetActive(false);
        stateView?.SetActive(false);

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

        FindFirstObjectByType<RecipeController>()?.StartRecipeFlow(selected);
    

        ShowRecipeDetails(selected);
        fractionPanel?.SetActive(true);
        backButton.gameObject.SetActive(true);

        
        recipeController.ShowMessage(ingredientRequireStrings[selected.ingredients[0].ingredientName]);
        
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
        recipeDetailsPanel.SetActive(false);

        // Mostrar estado
        stateView?.SetActive(true);

        // Construir texto con ingredientes
        string info = $"<b>{recipe.recipeName}</b>\n\nIngredientes requeridos:\n";

        ingredientRequireStrings.Clear();
        foreach (var ing in recipe.ingredients)
        {
            string line = $"Agrega <color=yellow>{GetCupFraction(ing.amountML)}</color> de {ing.ingredientName}";
            info += line;
            ingredientRequireStrings.Add(ing.ingredientName, line);
        }
        
        ingredientsText.text = info;
    }

    /// <summary>
    /// Vuelve al listado de recetas ocultando panel de detalles.
    /// </summary>
    private void OnBackToList()
    {
        scrollView?.SetActive(true);
        recipeDetailsPanel?.SetActive(false);

        recipeDetailsPanel?.SetActive(false);
        scrollView?.SetActive(true);
        stateView?.SetActive(false);
        fractionPanel?.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Obtiene la fracción de taza correspondiente al tamaño en ml.
    /// </summary>
    public string GetCupFraction(float ml)
    {
        Dictionary<float, string> map = new()
        {
            { 500f, "2" },
            { 333f, "3" },
            { 250f, "4" },
            { 200f, "5" },
            { 166.5f, "6" }
        };
        List<float> fractions = new List<float>();
        for (int i = 0; i < cupValues.Length; i++)
        {
            if (ml % cupValues[i] == 0)
                fractions.Add(cupValues[i]);
        }
        if (fractions.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, fractions.Count);
            float randomValue = fractions[randomIndex];
            var fracctionCup = "";
            foreach (var kv in map)
            {
                if (kv.Key == randomValue)
                    fracctionCup = kv.Value;
            }
            return $"{ml / randomValue}/{fracctionCup}";
        }
        else
        {
            return "0";
        }
    }

    /// <summary>
    /// Cierra el menú y desbloquea jugador y cámara.
    /// </summary>
    private void ExitMenu()
    {
        Animator anim = exitButton.GetComponent<Animator>();

        anim.SetBool("isPressed", false);
        anim.SetTrigger("Normal");
        anim.SetInteger("state", 0);


        gameObject.SetActive(false);

        pouringStation?.UnlockPlayer();

    }
}
