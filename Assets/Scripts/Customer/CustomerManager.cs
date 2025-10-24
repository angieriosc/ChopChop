using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    [Header("Customer Settings")]
    public List<CustomerPrefab> customerPrefabs; // 3 tipos de clientes
    
    [Header("Spawn Points")]
    public Transform counterSpawnPoint;
    public Transform counterWaitPoint;
    public List<Transform> tableSpawnPoints; // Puntos en las mesas
    
    [Header("Recipes")]
    public List<RecipeDataMenu> levelRecipes;
    public bool randomOrderInLevel = false; // False para orden secuencial, True para aleatorio
    
    [Header("UI References")]
    public GameObject recipeScrollUI; // Canvas completo del pergamino
    public RecipeScrollDisplay recipeScrollDisplay; // Script que maneja el contenido
    public RecipeUIManager recipeUIManager; // UI en la esquina superior derecha
    
    private int currentRecipeIndex = 0;
    private Customer currentCounterCustomer;
    private List<GameObject> spawnedTableCustomers = new List<GameObject>();
    private RecipeDataMenu activeRecipe;
    
    void Start()
    {
        if (recipeScrollUI != null)
        {
            recipeScrollUI.SetActive(false);
        }
        
        Debug.Log("CustomerManager iniciado. Total de recetas: " + levelRecipes.Count);
        
        // Comenzar con el primer cliente
        SpawnNextCustomer();
    }
    
    public void SpawnNextCustomer()
    {
        if (levelRecipes.Count == 0)
        {
            Debug.LogWarning("No hay recetas configuradas en el nivel");
            return;
        }
        
        // Determinar qué receta usar
        RecipeDataMenu recipe;
        if (randomOrderInLevel)
        {
            // Orden aleatorio
            recipe = levelRecipes[Random.Range(0, levelRecipes.Count)];
            Debug.Log("Receta aleatoria seleccionada: " + recipe.recipeName);
        }
        else
        {
            // Orden secuencial
            if (currentRecipeIndex >= levelRecipes.Count)
            {
                Debug.Log("Todas las recetas completadas en este nivel");
                return;
            }
            
            recipe = levelRecipes[currentRecipeIndex];
            currentRecipeIndex++;
            Debug.Log("Receta secuencial seleccionada: " + recipe.recipeName + " (Índice: " + (currentRecipeIndex - 1) + ")");
        }
        
        // Seleccionar tipo de cliente aleatorio
        int randomCustomerType = Random.Range(0, customerPrefabs.Count);
        GameObject customerPrefab = customerPrefabs[randomCustomerType].prefab;
        
        Debug.Log("Spawneando cliente tipo: " + customerPrefabs[randomCustomerType].type);
        
        // Instanciar el cliente en el punto de spawn
        GameObject customerObj = Instantiate(customerPrefab, counterSpawnPoint.position, counterSpawnPoint.rotation);
        currentCounterCustomer = customerObj.GetComponent<Customer>();
        
        if (currentCounterCustomer == null)
        {
            Debug.LogError("El prefab del cliente no tiene el componente Customer!");
            return;
        }
        
        // Inicializar el cliente
        currentCounterCustomer.Initialize(counterWaitPoint, counterSpawnPoint, recipe, this);
    }
    
    public void ShowRecipeScroll(RecipeDataMenu recipe)
{
    if (recipeScrollUI != null)
    {
        recipeScrollUI.SetActive(true);
        
        // ✅ ACTUALIZAR EL CONTENIDO DEL PERGAMINO
        if (recipeScrollDisplay != null)
        {
            recipeScrollDisplay.DisplayRecipe(recipe);
            Debug.Log("✅ RecipeScrollDisplay actualizado con datos de: " + recipe.recipeName);
        }
        else
        {
            Debug.LogError("❌ RecipeScrollDisplay es NULL! No se pueden actualizar los datos del pergamino");
        }
        
        // Actualizar la imagen de fondo del pergamino (si existe)
        UnityEngine.UI.Image scrollImage = recipeScrollUI.GetComponent<UnityEngine.UI.Image>();
        if (scrollImage != null)
        {
            if (recipe.recipeScrollImage != null)
            {
                scrollImage.sprite = recipe.recipeScrollImage;
                scrollImage.enabled = true;
                Debug.Log("Sprite del pergamino asignado: " + recipe.recipeScrollImage.name);
            }
            else
            {
                Debug.LogWarning("¡RecipeScrollImage es NULL en la receta: " + recipe.recipeName + "!");
            }
        }
        
        Debug.Log("Mostrando pergamino de receta: " + recipe.recipeName);
    }
    else
    {
        Debug.LogError("¡recipeScrollUI es NULL!");
    }
}
    
    public void HideRecipeScroll()
    {
        if (recipeScrollUI != null)
        {
            recipeScrollUI.SetActive(false);
            Debug.Log("Pergamino ocultado");
        }
    }
    
    public void ShowRecipeUI(RecipeDataMenu recipe)
    {
        activeRecipe = recipe;
        
        if (recipeUIManager != null)
        {
            recipeUIManager.DisplayRecipe(recipe);
            Debug.Log("UI de receta mostrada con " + recipe.ingredientSteps.Count + " ingredientes");
        }
    }
    
    public void OnCustomerLeftCounter(RecipeDataMenu recipe)
    {
        Debug.Log("Cliente dejó el mostrador. Spawneando " + recipe.numberOfCustomers + " clientes en las mesas");
        
        // Spawnear clientes en las mesas
        SpawnTableCustomers(recipe.numberOfCustomers);
    }
    
    void SpawnTableCustomers(int count)
    {
        // Limpiar clientes anteriores de las mesas
        ClearTableCustomers();
        
        if (tableSpawnPoints.Count < count)
        {
            Debug.LogWarning("No hay suficientes puntos de spawn en las mesas. Se necesitan " + count + " pero solo hay " + tableSpawnPoints.Count);
        }
        
        int spawnCount = Mathf.Min(count, tableSpawnPoints.Count);
        
        for (int i = 0; i < spawnCount; i++)
        {
            // Seleccionar tipo de cliente aleatorio
            int randomType = Random.Range(0, customerPrefabs.Count);
            GameObject customerPrefab = customerPrefabs[randomType].prefab;
            
            // Instanciar en la mesa
            GameObject tableCustomer = Instantiate(customerPrefab, tableSpawnPoints[i].position, tableSpawnPoints[i].rotation);
            spawnedTableCustomers.Add(tableCustomer);
            
            Debug.Log("Cliente spawneado en mesa " + (i + 1) + "/" + spawnCount);
        }
    }
    
    void ClearTableCustomers()
    {
        foreach (GameObject customer in spawnedTableCustomers)
        {
            if (customer != null)
            {
                Destroy(customer);
            }
        }
        spawnedTableCustomers.Clear();
        Debug.Log("Clientes de mesas anteriores eliminados");
    }
    
    public void OnOrderDelivered()
    {
        Debug.Log("Orden entregada, ocultando UI de receta");
        
        if (recipeUIManager != null)
        {
            recipeUIManager.HideRecipe();
        }
        
        // Limpiar clientes de las mesas
        ClearTableCustomers();
        
        // Esperar un momento antes de spawnear el siguiente cliente
        StartCoroutine(SpawnNextCustomerDelayed());
    }
    
    IEnumerator SpawnNextCustomerDelayed()
    {
        yield return new WaitForSeconds(1f);
        SpawnNextCustomer();
    }
    
    // Método para configurar el orden de recetas manualmente
    public void SetRecipeOrder(List<RecipeDataMenu> customOrder)
    {
        levelRecipes = customOrder;
        currentRecipeIndex = 0;
        Debug.Log("Orden de recetas configurado manualmente con " + customOrder.Count + " recetas");
    }
}