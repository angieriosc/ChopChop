using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    [Header("Customer Settings")]
    public List<CustomerPrefab> customerPrefabs;

    [Header("Spawn Points")]
    public Transform counterSpawnPoint;
    public Transform counterWaitPoint;
    public List<Transform> tableSpawnPoints;

    [Header("Recipes")]
    public List<RecipeDataMenu> levelRecipes;
    public bool randomOrderInLevel = false;

    [Header("UI References")]
    public GameObject recipeScrollUI;
    public RecipeScrollDisplay recipeScrollDisplay;
    public RecipeUIManager recipeUIManager;

    private int currentRecipeIndex = 0;
    private Customer currentCounterCustomer;
    private List<GameObject> spawnedTableCustomers = new List<GameObject>();
    private RecipeDataMenu activeRecipe;
    
    /// <summary>Receta actualmente activa en el nivel.</summary>
    public RecipeDataMenu ActiveRecipe => activeRecipe;
    
    /// <summary>Historial de puntuaciones de las entregas realizadas en este nivel.</summary>
    public List<SatisfactionResult> satisfactionHistory = new List<SatisfactionResult>();

    [SerializeField] private PizzaToppingManager toppingManager;
    private int nextSeatIndex = 0;


    /// <summary>
    /// Inicializa el sistema y genera el primer cliente del mostrador.
    /// </summary>
    private void Start()
    {
        if (recipeScrollUI != null)
            recipeScrollUI.SetActive(false);

        SpawnNextCustomer();
    }


    /// <summary>
    /// Resetea los sistemas de paciencia y entrega, y genera el siguiente cliente 
    /// en el mostrador según la receta asignada (secuencial o aleatoria).
    /// </summary>
    public void SpawnNextCustomer()
    {
        // 1. Resetear sistemas externos para el nuevo cliente
        if (PizzaDeliveryManager.Instance != null)
        {
            PizzaDeliveryManager.Instance.ResetForNewRecipe();
        }

        if (PatienceManager.Instance != null)
        {
            PatienceManager.Instance.ResetPatience();
        }

        // 2. Validar recetas disponibles
        if (levelRecipes.Count == 0)
            return;

        RecipeDataMenu recipe;

        if (randomOrderInLevel)
        {
            recipe = levelRecipes[Random.Range(0, levelRecipes.Count)];
        }
        else
        {
            // Verificar si se acabaron las recetas en modo secuencial
            if (currentRecipeIndex >= levelRecipes.Count)
            {
                Debug.Log("🔚 Ya no hay más recetas en la lista.");
                return;
            }

            recipe = levelRecipes[currentRecipeIndex];
            currentRecipeIndex++;
        }

        // 3. Configurar receta activa
        activeRecipe = recipe;

        if (toppingManager != null)
        {
            toppingManager.ApplyRecipeLimits(recipe);
        }

        // 4. Spawnear cliente visual en el mostrador
        int randomCustomerType = Random.Range(0, customerPrefabs.Count);
        GameObject customerPrefab = customerPrefabs[randomCustomerType].prefab;

        GameObject customerObj = Instantiate(
            customerPrefab,
            counterSpawnPoint.position,
            counterSpawnPoint.rotation
        );

        currentCounterCustomer = customerObj.GetComponent<Customer>();

        if (currentCounterCustomer == null)
            return;

        currentCounterCustomer.Initialize(counterWaitPoint, counterSpawnPoint, recipe, this);
    }

    /// <summary>
    /// Muestra la UI del pergamino con la información de la receta actual.
    /// </summary>
    /// <param name="recipe">Datos de la receta a mostrar.</param>
    public void ShowRecipeScroll(RecipeDataMenu recipe)
    {
        if (recipeScrollUI == null)
            return;

        recipeScrollUI.SetActive(true);

        if (recipeScrollDisplay != null)
            recipeScrollDisplay.DisplayRecipe(recipe);

        var scrollImage = recipeScrollUI.GetComponent<UnityEngine.UI.Image>();

        if (scrollImage != null && recipe.recipeScrollImage != null)
        {
            scrollImage.sprite = recipe.recipeScrollImage;
            scrollImage.enabled = true;
        }
    }


    /// <summary>
    /// Oculta el pergamino de receta de la UI.
    /// </summary>
    public void HideRecipeScroll()
    {
        if (recipeScrollUI != null)
            recipeScrollUI.SetActive(false);
    }


    /// <summary>
    /// Muestra la UI flotante (HUD) con los pasos e ingredientes de la receta activa.
    /// </summary>
    public void ShowRecipeUI(RecipeDataMenu recipe)
    {
        activeRecipe = recipe;

        if (recipeUIManager != null)
            recipeUIManager.DisplayRecipe(recipe);
    }


    /// <summary>
    /// Callback ejecutado cuando el cliente del mostrador se retira. 
    /// Genera los clientes sentados en las mesas correspondientes.
    /// </summary>
    public void OnCustomerLeftCounter(RecipeDataMenu recipe)
    {
        SpawnTableCustomers(recipe.numberOfCustomers);
    }


    /// <summary>
    /// Genera clientes visuales en las mesas (SpawnPoints) de acuerdo al número indicado.
    /// </summary>
    private void SpawnTableCustomers(int count)
    {
        ClearTableCustomers();

        int spawnCount = Mathf.Min(count, tableSpawnPoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomType = Random.Range(0, customerPrefabs.Count);
            GameObject prefab = customerPrefabs[randomType].prefab;

            GameObject tableCustomer = Instantiate(prefab, tableSpawnPoints[i].position, tableSpawnPoints[i].rotation);
            spawnedTableCustomers.Add(tableCustomer);
            
            // Asignar índice de asiento para la lógica de entrega
            DeliveryArea area = tableCustomer.GetComponentInChildren<DeliveryArea>();
            if (area != null)
            {
                area.slotIndex = nextSeatIndex;
            }
            nextSeatIndex++;
        }
        nextSeatIndex = 0;
    }


    /// <summary>
    /// Elimina todos los clientes visuales actualmente ubicados en las mesas.
    /// </summary>
    private void ClearTableCustomers()
    {
        foreach (GameObject customer in spawnedTableCustomers)
        {
            if (customer != null)
                Destroy(customer);
        }

        spawnedTableCustomers.Clear();
    }


    /// <summary>
    /// Procesa la entrega final de la orden.
    /// Guarda la satisfacción, limpia la escena y decide si continuar al siguiente cliente 
    /// o finalizar el nivel mostrando la pantalla de resultados.
    /// </summary>
    public void OnOrderDelivered()
    {
        // 1. Guardar datos de la partida
        RecordSatisfaction(); 

        // 2. Limpiar UI y mesas
        if (recipeUIManager != null) recipeUIManager.HideRecipe();
        ClearTableCustomers();

        // 3. Verificar Fin de Nivel (Solo si no es modo infinito/random)
        bool isLevelFinished = !randomOrderInLevel && (currentRecipeIndex >= levelRecipes.Count);

        if (isLevelFinished)
        {
            Debug.Log("🎉 ¡NIVEL COMPLETADO!");
            
            // Buscar y activar la pantalla de resultados
            LevelResultsUI resultsUI = FindFirstObjectByType<LevelResultsUI>();
            
            if (resultsUI != null)
            {
                StartCoroutine(ShowResultsSequence(resultsUI));
            }
            else
            {
                Debug.LogWarning("Nivel terminado pero no se encontró 'LevelResultsUI'. Asegúrate de que el objeto esté activo en la escena.");
            }
        }
        else
        {
            // Si quedan recetas, continuar
            StartCoroutine(SpawnNextCustomerDelayed());
        }
    }

    /// <summary>
    /// Corrutina para dar una pausa dramática antes de mostrar la tabla de resultados.
    /// </summary>
    private IEnumerator ShowResultsSequence(LevelResultsUI ui)
    {
        yield return new WaitForSeconds(4f); // Espera tras entregar la última pizza
        ui.ShowResults();
    }

    /// <summary>
    /// Calcula y registra la satisfacción del cliente basada en la paciencia restante en el momento de la entrega.
    /// </summary>
    private void RecordSatisfaction()
    {
        if (activeRecipe == null) return;

        float score = 0f;
        string grade = "N/A";

        // Obtenemos la paciencia final del Manager
        if (PatienceManager.Instance != null)
        {
            score = PatienceManager.Instance.GetCurrentPatience();
        }

        // Calificación simple
        if (score >= 80) grade = "Perfecto";
        else if (score >= 50) grade = "Bien";
        else if (score > 0) grade = "Regular";
        else grade = "Terrible";

        // Guardar registro
        SatisfactionResult result = new SatisfactionResult
        {
            recipeName = activeRecipe.recipeName,
            finalScore = score,
            rating = grade
        };

        satisfactionHistory.Add(result);
    }


    /// <summary>
    /// Espera un tiempo antes de generar un nuevo cliente tras una entrega exitosa.
    /// </summary>
    private IEnumerator SpawnNextCustomerDelayed()
    {
        yield return new WaitForSeconds(1f);
        SpawnNextCustomer();
    }


    /// <summary>
    /// Permite establecer un orden personalizado de recetas para este nivel desde scripts externos.
    /// </summary>
    public void SetRecipeOrder(List<RecipeDataMenu> customOrder)
    {
        levelRecipes = customOrder;
        currentRecipeIndex = 0;
    }
}