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

    [SerializeField] private PizzaToppingManager toppingManager;


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
        if (levelRecipes.Count == 0)
            return;

        RecipeDataMenu recipe;

        if (randomOrderInLevel)
        {
            recipe = levelRecipes[Random.Range(0, levelRecipes.Count)];
        }
        else
        {
            if (currentRecipeIndex >= levelRecipes.Count)
                return;

            recipe = levelRecipes[currentRecipeIndex];
            currentRecipeIndex++;
            activeRecipe = recipe;

            if (toppingManager != null)
                toppingManager.ApplyRecipeLimits(recipe);
        }

        int randomCustomerType = Random.Range(0, customerPrefabs.Count);
        GameObject customerPrefab = customerPrefabs[randomCustomerType].prefab;

        GameObject customerObj = Instantiate(customerPrefab, counterSpawnPoint.position, counterSpawnPoint.rotation);
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
        }
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
    /// Procesa la entrega de la orden y genera el siguiente cliente.
    /// </summary>
    public void OnOrderDelivered()
    {
        if (recipeUIManager != null)
            recipeUIManager.HideRecipe();

        ClearTableCustomers();

        StartCoroutine(SpawnNextCustomerDelayed());
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
