using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Supervisa el vertido de tazas y verifica si la fracción seleccionada
/// coincide con la cantidad del ingrediente especificada en la receta.
/// </summary>
public class CupTracker : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("External References")]
    [SerializeField] private PouringStation pouringStation;

    [Header("Recipe Controller")]
    [SerializeField] public RecipeController recipeController;

    /// <summary>Nombre del ingrediente activo en la receta.</summary>
    public string activeIngredientName;

    /// <summary>Tipos de fracciones de taza.</summary>
    public enum CupFraction { Half, Third, Quarter, Fifth, Sixth }

    /// <summary>Valores equivalentes de cada fracción en mililitros.</summary>
    private readonly float[] cupValues = { 500f, 333f, 250f, 200f, 166.5f };

    /// <summary>Fracción seleccionada por el usuario.</summary>
    private CupFraction fractionSelected;

    /// <summary>Tolerancia de error al comparar cantidades (en ml).</summary>
    private const float tolerance = 0.5f;

    /// <summary>
    /// Se ejecuta cuando el jugador selecciona una fracción.
    /// Valida si coincide con la receta activa.
    /// </summary>
    public bool SelectCup(CupFraction fraction)
    {
        activeIngredientName =
            pouringStation.activePouringContainer.ingredientName;

        var container = pouringStation?.CurrentReceiving;
        var recipe = container?.currentRecipe;

        if (container == null || recipe == null)
        {
            StartCoroutine(ShowMessage(
                "❌ No hay receta activa o recipiente asignado."
            ));
            return false;
        }

        string ingredient = activeIngredientName;
        float required = recipe.GetRequiredAmount(ingredient);
        float current = container.GetIngredientAmount(ingredient);
        float cupSize = cupValues[(int)fraction];

        fractionSelected = fraction;

        if (!IsCupValid(required, cupSize))
        {
            StartCoroutine(ShowMessage(
                $"❌ La fracción seleccionada ({cupSize:F0} ml) no coincide."
            ));
            return false;
        }

        StartCoroutine(ShowMessage(
            $"✅ Fracción válida seleccionada: {cupSize:F0} ml."
        ));
        return true;
    }

    /// <summary>
    /// Actualiza el conteo de tazas vertidas y notifica al controlador
    /// de receta cuando un ingrediente se completa.
    /// </summary>
    public void UpdateCups()
    {
        var container = pouringStation?.CurrentReceiving;
        var recipe = container?.currentRecipe;
        if (container == null || recipe == null) return;

        float cupSize = cupValues[(int)fractionSelected];
        string ingredient = activeIngredientName;
        float required = recipe.GetRequiredAmount(ingredient);
        float current = container.GetIngredientAmount(ingredient);

        int totalCups = Mathf.CeilToInt(required / cupSize);
        int pouredCups = Mathf.FloorToInt(current / cupSize);

        progressText.text =
            $"Ingrediente: {ingredient}\n" +
            $"Tazas vertidas: {pouredCups} / {totalCups}\n" +
            $"({cupSize:F0} ml por taza)\n" +
            $"Total: {current:F0} / {required:F0} ml";

        bool isComplete = current >= required - tolerance;
        string msg = isComplete
            ? "✅ Ingrediente completo!"
            : "🧪 Sigue vertiendo...";

        StartCoroutine(ShowMessage(msg));

        if (isComplete)
        {
            recipeController ??= FindFirstObjectByType<RecipeController>();
            recipeController?.OnIngredientCompleted();
        }
    }

    /// <summary>
    /// Valida si la cantidad requerida puede dividirse con la fracción
    /// seleccionada dentro del margen de tolerancia.
    /// </summary>
    private bool IsCupValid(float required, float cupSize)
    {
        float remainder = Mathf.Abs(required % cupSize);
        return remainder <= tolerance ||
               Mathf.Abs(cupSize - required) <= tolerance;
    }

    /// <summary>
    /// Limpia el texto de progreso y los mensajes en pantalla.
    /// </summary>
    public void ResetTracker()
    {
        progressText.text = "";
        messageText.text = "";
    }

    /// <summary>
    /// Muestra un mensaje temporal en pantalla.
    /// </summary>
    public IEnumerator ShowMessage(string msg)
    {
        if (messageText == null) yield break;

        messageText.gameObject.SetActive(true);
        messageText.text = msg;

        yield return new WaitForSeconds(5f);

        messageText.gameObject.SetActive(false);
    }
}
