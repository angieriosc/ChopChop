using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Controla el flujo automático de ingredientes de una receta.
/// Muestra un ingrediente a la vez y avanza al siguiente cuando se completa.
/// </summary>
public class RecipeController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PouringStation pouringStation;
    [SerializeField] private CupTracker cupTracker;
    [SerializeField] private IngredientDivider ingredientDivider;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Prefabs de ingredientes")]
    [SerializeField] private GameObject[] ingredientPrefabs;
    [SerializeField] private string[] ingredientNames; // mismo orden que los prefabs
    [SerializeField] private Transform spawnPoint;

    private RecipeData currentRecipe;
    private int currentIngredientIndex = 0;
    private GameObject currentIngredientGO;

    /// <summary>
    /// Inicia el flujo de la receta seleccionada.
    /// </summary>
    public void StartRecipeFlow(RecipeData recipe)
    {
        if (recipe == null) return;

        currentRecipe = recipe;
        currentIngredientIndex = 0;

        AssignCurrentIngredient();
    }

    /// <summary>
    /// Obtiene el prefab correspondiente al nombre del ingrediente.
    /// </summary>
    private GameObject GetPrefabByName(string name)
    {
        for (int i = 0; i < ingredientPrefabs.Length; i++)
        {
            if (ingredientPrefabs[i].name == name)
                return ingredientPrefabs[i];
        }
        return null;
    }

    /// <summary>
    /// Asigna el ingrediente actual al recipiente activo, instancia su prefab y actualiza UI.
    /// </summary>
    private void AssignCurrentIngredient()
    {
        ingredientDivider.ClearCups();
        if (currentIngredientIndex >= currentRecipe.ingredients.Count)
        {
            RecipeCompleted();
            return;
        }

        var ingredient = currentRecipe.ingredients[currentIngredientIndex];


        // Destruir prefab anterior si existe
        if (currentIngredientGO != null)
            Destroy(currentIngredientGO);

        // Instanciar prefab del ingrediente actual
        GameObject prefab = GetPrefabByName(ingredient.ingredientName);
        Debug.Log($"Prefab obtenido: {(prefab != null ? prefab.name : "null")}");
        if (prefab != null && spawnPoint != null)
        {
            currentIngredientGO = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            Debug.Log($"Instanciado ingrediente: {ingredient.ingredientName}");
        }

        currentIngredientGO.GetComponent<PouringContainer>().targetContainer = pouringStation.CurrentReceiving;

        // Configurar el vertido
        pouringStation.SetActivePouring(currentIngredientGO.GetComponent<PouringContainer>());
        // Resetear tracker
        cupTracker.activeIngredientName = ingredient.ingredientName;
        cupTracker.ResetTracker();

        StartCoroutine(ShowMessage($"Añade: {ingredient.ingredientName} ({ingredient.amountML} ml)"));
    }

    /// <summary>
    /// Llamar desde CupTracker cuando un ingrediente se complete.
    /// </summary>
    public void OnIngredientCompleted()
    {
        currentIngredientIndex++;
        AssignCurrentIngredient();
    }

    /// <summary>
    /// Maneja finalización de la receta.
    /// </summary>
    private void RecipeCompleted()
    {
        StartCoroutine(ShowMessage("✅ Receta completada!"));

        // Destruir el último prefab
        if (currentIngredientGO != null)
            Destroy(currentIngredientGO);

        // Desbloquear jugador y salir de la estación
        var currentBowl = pouringStation.CurrentReceiving.gameObject;
        currentBowl.AddComponent<MixableBowl>();
        currentBowl.GetComponent<InteractableObject>().AddCapability(ObjectCapabilities.Mixable);
        currentBowl.GetComponent<MixableBowl>().currentIngredients= new System.Collections.Generic.List<string>(currentRecipe.GetIngredientNames());
        pouringStation.UnlockPlayer();
    }

    private IEnumerator ShowMessage(string msg, float duration = 3f)
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = msg;
            yield return new WaitForSeconds(duration);
            messageText.gameObject.SetActive(false);
        }
    }
}
