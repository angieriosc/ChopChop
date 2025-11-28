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
    public RecipeDataMenu ActiveRecipe => activeRecipe;
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
    /// Genera el siguiente cliente en el mostrador según la receta asignada.
    /// </summary>
    public void SpawnNextCustomer()
    {
        if (PizzaDeliveryManager.Instance != null)
        {
            PizzaDeliveryManager.Instance.ResetForNewRecipe();
        }

        if (PatienceManager.Instance != null)
        {
            PatienceManager.Instance.ResetPatience();
        }

        if (levelRecipes.Count == 0)
            return;

        RecipeDataMenu recipe;

        if (randomOrderInLevel)
        {
            // Receta aleatoria
            recipe = levelRecipes[Random.Range(0, levelRecipes.Count)];
        }
        else
        {
            // Receta en orden
            if (currentRecipeIndex >= levelRecipes.Count)
            {
                Debug.Log("🔚 Ya no hay más recetas en levelRecipes.");
                return;
            }

            recipe = levelRecipes[currentRecipeIndex];
            currentRecipeIndex++;
        }

        // SIEMPRE actualizar receta activa y límites
        activeRecipe = recipe;

        if (toppingManager != null)
        {
            toppingManager.ApplyRecipeLimits(recipe);
            Debug.Log($"[CustomerManager] ApplyRecipeLimits -> {recipe.recipeName}");
        }

        // --- Spawnear cliente en el mostrador ---
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
    /// Muestra la UI del pergamino con la receta actual.
    /// </summary>
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
    /// Oculta el pergamino de receta.
    /// </summary>
    public void HideRecipeScroll()
    {
        if (recipeScrollUI != null)
            recipeScrollUI.SetActive(false);
    }


    /// <summary>
    /// Muestra la UI flotante con los pasos e ingredientes de la receta.
    /// </summary>
    public void ShowRecipeUI(RecipeDataMenu recipe)
    {
        activeRecipe = recipe;

        if (recipeUIManager != null)
            recipeUIManager.DisplayRecipe(recipe);
    }


    /// <summary>
    /// Ejecutado cuando el cliente del mostrador se va. Genera clientes en las mesas.
    /// </summary>
    public void OnCustomerLeftCounter(RecipeDataMenu recipe)
    {
        SpawnTableCustomers(recipe.numberOfCustomers);
    }


    /// <summary>
    /// Genera clientes adicionales en las mesas de acuerdo al número indicado.
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
            DeliveryArea area = tableCustomer.GetComponentInChildren<DeliveryArea>();
            if (area != null)
            {
                area.slotIndex = nextSeatIndex;   // 0,1,2,3...
                Debug.Log($"Asignando slotIndex {area.slotIndex} al cliente {tableCustomer.name}");
            }
            nextSeatIndex++;
        }
        nextSeatIndex = 0;
    }


    /// <summary>
    /// Elimina todos los clientes actualmente ubicados en mesas.
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
    /// Procesa la entrega de la orden y decide si spawnear otro o terminar el nivel.
    /// </summary>
    public void OnOrderDelivered()
    {
        RecordSatisfaction(); 

        if (recipeUIManager != null) recipeUIManager.HideRecipe();
        ClearTableCustomers();

        // --- DEBUG DIAGNÓSTICO ---
        Debug.Log($"[DEBUG] Revisando fin de nivel...");
        Debug.Log($"[DEBUG] Indice Actual: {currentRecipeIndex} | Total Recetas: {levelRecipes.Count}");
        Debug.Log($"[DEBUG] ¿Es Random?: {randomOrderInLevel}");

        // Condición de victoria
        bool isLevelFinished = !randomOrderInLevel && (currentRecipeIndex >= levelRecipes.Count);

        if (isLevelFinished)
        {
            Debug.Log("<color=green>[DEBUG] CONDICIÓN DE VICTORIA CUMPLIDA.</color> Buscando UI...");
            
            // Buscamos el script en la escena
            LevelResultsUI resultsUI = FindFirstObjectByType<LevelResultsUI>();
            
            if (resultsUI != null)
            {
                Debug.Log($"[DEBUG] UI encontrada: {resultsUI.gameObject.name}. Iniciando secuencia...");
                StartCoroutine(ShowResultsSequence(resultsUI));
            }
            else
            {
                // ESTE ES EL ERROR MÁS COMÚN
                Debug.LogError("<color=red>[ERROR CRÍTICO]</color> Unity devolvió NULL al buscar 'LevelResultsUI'.");
                Debug.LogError("CAUSA PROBABLE: El GameObject que tiene el script 'LevelResultsUI' está apagado (gris) en la jerarquía.");
                Debug.LogError("SOLUCIÓN: Activa el GameObject padre, el script se encarga de apagar el panel hijo en el Start().");
            }
        }
        else
        {
            Debug.Log("[DEBUG] Aún quedan recetas o es modo random. Spawneando siguiente...");
            StartCoroutine(SpawnNextCustomerDelayed());
        }
    }

    // Pequeña pausa dramática antes de mostrar la tabla
    private IEnumerator ShowResultsSequence(LevelResultsUI ui)
    {
        yield return new WaitForSeconds(4f); // Espera un poco tras entregar la última pizza
        ui.ShowResults(); // Activa el menú
    }

    /// <summary>
    /// Registra la satisfacción del cliente basado en la paciencia restante.
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

        // Calculamos una calificación simple (puedes personalizar esto)
        if (score >= 80) grade = "Perfecto";
        else if (score >= 50) grade = "Bien";
        else if (score > 0) grade = "Regular";
        else grade = "Terrible";

        // Creamos el registro
        SatisfactionResult result = new SatisfactionResult
        {
            recipeName = activeRecipe.recipeName,
            finalScore = score,
            rating = grade
        };

        // Lo agregamos a la lista
        satisfactionHistory.Add(result);

        Debug.Log($"<color=cyan>[RESULTADO]</color> Receta: {result.recipeName} | Puntos: {score} | Nota: {grade}");
    }


    /// <summary>
    /// Espera antes de spawnear un nuevo cliente después de entregar la orden.
    /// </summary>
    private IEnumerator SpawnNextCustomerDelayed()
    {
        yield return new WaitForSeconds(1f);
        SpawnNextCustomer();
    }


    /// <summary>
    /// Establece un orden personalizado de recetas para este nivel.
    /// </summary>
    public void SetRecipeOrder(List<RecipeDataMenu> customOrder)
    {
        levelRecipes = customOrder;
        currentRecipeIndex = 0;
    }
}
