using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

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

    [Header("Panel de fracciones")]
    [SerializeField] public FractionPanelUI fractionPanelUI;

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
            ChangeMessage(
                "No hay receta activa o recipiente asignado."
            );
            return false;
        }

        string ingredient = activeIngredientName;
        float required = recipe.GetRequiredAmount(ingredient);
        float current = container.GetIngredientAmount(ingredient);
        float cupSize = cupValues[(int)fraction];

        fractionSelected = fraction;

        if (!IsCupValid(required, cupSize))
        {
            StartCoroutine(TemporalShowMessage(
                $"Incorrecto, intenta con otra.", messageText.text
            ));
            return false;
        }
        else
        {

            required = recipe.GetRequiredAmount(ingredient);
            current = container.GetIngredientAmount(ingredient);

            int totalCups = Mathf.CeilToInt(required / cupSize);
            int pouredCups = Mathf.FloorToInt(current / cupSize);

            progressText.text =
            $"Tazas vertidas: {pouredCups} / {totalCups}\n";

            ChangeMessage("Da <color=yellow>click</color> a las tazas");

        }
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
            $"Tazas vertidas: <color=yellow>{pouredCups} / {totalCups}</color>\n";

        bool isComplete = current >= required - tolerance;

        if (isComplete)
        {
            recipeController ??= FindFirstObjectByType<RecipeController>();
            recipeController?.OnIngredientCompleted();
            fractionPanelUI.imageFractionCups.SetActive(false);
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
    }

    /// <summary>
    /// Muestra un mensaje temporal en pantalla.
    /// </summary>
    public IEnumerator TemporalShowMessage(string newmsg, string oldmsg)
    {
        if (messageText == null) yield break;
        if (newmsg == oldmsg) yield break;

        messageText.gameObject.SetActive(true);
        messageText.text = newmsg;

        yield return new WaitForSeconds(3f);

        messageText.text = oldmsg;
    }

    /// <summary>
    /// Cambia el mensaje
    /// </summary>
    public void ChangeMessage(string msg)
    {
        messageText.text = msg;
    }
}
