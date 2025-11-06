using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Controla el flujo automatizado de preparación de una receta.
/// Gestiona la aparición, asignación y vertido secuencial de cada
/// ingrediente, mostrando instrucciones en pantalla y bloqueando
/// al jugador mientras la receta se ejecuta.
/// </summary>
public class RecipeController : MonoBehaviour
{
    [Header("Referencias principales")]
    [Tooltip("Referencia a la estación de vertido activa.")]
    [SerializeField] private PouringStation pouringStation;

    [Tooltip("Controlador que rastrea el progreso de vertido.")]
    [SerializeField] private CupTracker cupTracker;

    [Tooltip("Componente que gestiona la división de líquidos.")]
    [SerializeField] private IngredientDivider ingredientDivider;

    [Tooltip("Texto en pantalla donde se muestran instrucciones.")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Prefabs de ingredientes")]
    [Tooltip("Prefabs disponibles de ingredientes.")]
    [SerializeField] private GameObject[] ingredientPrefabs;

    [Tooltip("Nombres de ingredientes (deben coincidir con el orden de los prefabs).")]
    [SerializeField] private string[] ingredientNames;

    [Tooltip("Punto de aparición para los ingredientes.")]
    [SerializeField] private Transform spawnPoint;

    private RecipeData currentRecipe;
    private int currentIngredientIndex = 0;
    private GameObject currentIngredientGO;

    /// <summary>
    /// Inicia el flujo de preparación de una receta específica.
    /// </summary>
    /// <param name="recipe">Receta seleccionada para ejecutar.</param>
    public void StartRecipeFlow(RecipeData recipe)
    {
        if (recipe == null) return;

        currentRecipe = recipe;
        currentIngredientIndex = 0;

        AssignCurrentIngredient();
    }

    /// <summary>
    /// Busca y devuelve el prefab correspondiente al nombre del ingrediente.
    /// </summary>
    /// <param name="name">Nombre del ingrediente.</param>
    /// <returns>
    /// Prefab del ingrediente si existe; de lo contrario, null.
    /// </returns>
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
    /// Asigna e instancia el ingrediente actual en la estación,
    /// actualiza la UI y configura el flujo de vertido.
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

        // Eliminar el prefab anterior si existe
        if (currentIngredientGO != null)
            Destroy(currentIngredientGO);

        // Instanciar nuevo ingrediente
        GameObject prefab = GetPrefabByName(ingredient.ingredientName);
        Debug.Log($"Prefab obtenido: {(prefab != null ? prefab.name : "null")}");

        if (prefab != null && spawnPoint != null)
        {
            currentIngredientGO = Instantiate(
                prefab, spawnPoint.position, Quaternion.identity);
            Debug.Log($"Instanciado ingrediente: {ingredient.ingredientName}");
        }

        var pouring = currentIngredientGO.GetComponent<PouringContainer>();
        pouring.targetContainer = pouringStation.CurrentReceiving;

        // Configurar vertido y progreso
        pouringStation.SetActivePouring(pouring);
        cupTracker.activeIngredientName = ingredient.ingredientName;
        cupTracker.ResetTracker();

        StartCoroutine(ShowMessage(
            $"Añade {ingredient.ingredientName}"
        ));
    }

    /// <summary>
    /// Avanza al siguiente ingrediente tras completar el vertido actual.
    /// </summary>
    public void OnIngredientCompleted()
    {
        currentIngredientIndex++;
        AssignCurrentIngredient();
    }

    /// <summary>
    /// Ejecuta las acciones finales cuando la receta ha sido completada.
    /// </summary>
    private void RecipeCompleted()
    {
        StartCoroutine(ShowMessage("Receta completada!"));

        // Eliminar último ingrediente
        if (currentIngredientGO != null)
            Destroy(currentIngredientGO);

        // Preparar el recipiente para mezclar
        var currentBowl = pouringStation.CurrentReceiving.gameObject;
        currentBowl.AddComponent<MixableBowl>();
        currentBowl.GetComponent<InteractableObject>()
                   .AddCapability(ObjectCapabilities.Mixable);

        currentBowl.GetComponent<MixableBowl>().currentIngredients =
            new List<string>(currentRecipe.GetIngredientNames());

        // Liberar jugador
        pouringStation.UnlockPlayer();
    }

    /// <summary>
    /// Muestra un mensaje temporal en pantalla.
    /// </summary>
    /// <param name="msg">Texto a mostrar.</param>
    /// <param name="duration">Duración en segundos del mensaje.</param>
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
