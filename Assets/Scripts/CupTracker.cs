using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Controla el conteo de tazas vertidas y verifica si la fracción seleccionada
/// coincide con la cantidad total del ingrediente en la receta.
/// </summary>
public class CupTracker : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Referencias externas")]
    [SerializeField] private PouringStation pouringStation;

    [Header("Recipe Controller")]
    [SerializeField] public RecipeController recipeController;

    public string activeIngredientName;
    // Fracciones de taza y su valor en ml
    public enum CupFraction { Half, Third, Quarter, Fifth, Sixth }
    private readonly float[] cupValues = { 500f, 333f, 250f, 200f, 166.5f };

    private CupFraction fractionselected;


    private const float tolerance = 0.5f;

    /// <summary>
    /// Se llama cuando el jugador selecciona una fracción de taza.
    /// Verifica si coincide con la receta y actualiza el progreso.
    /// </summary>
    public bool SelectCup(CupFraction fraction)
    {
        activeIngredientName = pouringStation.activePouringContainer.ingredientName;
        var container = pouringStation?.CurrentReceiving;
        var recipe = container?.currentRecipe;

        if (container == null || recipe == null)
        {
            StartCoroutine(ShowMessage("❌ No hay receta activa o recipiente asignado."));
            return false;
        }

        // Obtener el ingrediente y cantidades
        string ingredient = activeIngredientName;
        float required = recipe.GetRequiredAmount(ingredient);
        float current = container.GetIngredientAmount(ingredient);
        float cupSize = cupValues[(int)fraction];

        fractionselected = fraction;

        // Validar si la fracción seleccionada divide correctamente la receta
        if (!IsCupValid(required, cupSize))
        {
            StartCoroutine(ShowMessage($"❌ La fracción seleccionada ({cupSize:F0} ml) no coincide."));
            return false;
        }
        else
        {
            StartCoroutine(ShowMessage($"✅ Fracción válida seleccionada: {cupSize:F0} ml."));
            return true;
        }
    }

    public void UpdateCups()
    {

        var container = pouringStation?.CurrentReceiving;
        var recipe = container?.currentRecipe;
        float cupSize = cupValues[(int)fractionselected];


        string ingredient = activeIngredientName;
        float required = recipe.GetRequiredAmount(ingredient);
        float current = container.GetIngredientAmount(ingredient);
        // Calcular tazas necesarias y vertidas
        int totalCups = Mathf.CeilToInt(required / cupSize);
        int pouredCups = Mathf.FloorToInt(current / cupSize);

        Debug.Log("required: " + required + "cupSize: " + cupSize);
        Debug.Log("current: " + current + "cupSize: " + cupSize);

        // Actualizar UI
        progressText.text =
            $"Ingrediente: {ingredient}\n" +
            $"Tazas vertidas: {pouredCups} / {totalCups}\n" +
            $"({cupSize:F0} ml por taza)\n" +
            $"Total: {current:F0} / {required:F0} ml";

        StartCoroutine(ShowMessage(current >= required - tolerance
            ? "✅ Ingrediente completo!"
            : "🧪 Sigue vertiendo..."));

            // Avisar al RecipeController si se completó
        if (current >= required - tolerance)
        {
            StartCoroutine(ShowMessage("✅ Ingrediente completo!"));
            // Notificar al flujo de receta
            Object.FindFirstObjectByType<RecipeController>()?.OnIngredientCompleted();
    
        }
    }

    /// <summary>
    /// Valida si la cantidad requerida puede dividirse por el tamaño de taza.
    /// </summary>
    private bool IsCupValid(float required, float cupSize)
    {
        float remainder = Mathf.Abs(required % cupSize);
        return remainder <= tolerance || Mathf.Abs(cupSize - required) <= tolerance;
    }

    /// <summary>
    /// Limpia la UI de progreso y mensajes.
    /// </summary>
    public void ResetTracker()
    {
        progressText.text = "";
        messageText.text = "";
    }

    public IEnumerator ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = msg;

            yield return new WaitForSeconds(5f);

            messageText.gameObject.SetActive(false);
        }
    }
}

