using UnityEngine;
using System.Collections;


public class Customer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float rotationSpeed = 5f;
    
    [Header("References")]
    public Transform counterPoint;
    public Transform spawnPoint;
    
    private RecipeDataMenu currentRecipe;
    private CustomerManager manager;
    private Animator animator; // Si tienes animaciones
    private bool hasReachedCounter = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    public void Initialize(Transform counter, Transform spawn, RecipeDataMenu recipe, CustomerManager mgr)
    {
        counterPoint = counter;
        spawnPoint = spawn;
        currentRecipe = recipe;
        manager = mgr;
        
        Debug.Log("Cliente inicializado con receta: " + recipe.recipeName);
        
        StartCoroutine(WalkToCounter());
    }
    
    IEnumerator WalkToCounter()
    {
        Debug.Log("Cliente caminando al mostrador");
        
        // Activar animación de caminar si existe
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
        
        // Caminar hacia el mostrador
        while (Vector3.Distance(transform.position, counterPoint.position) > 0.1f)
        {
            // Rotar hacia el objetivo
            Vector3 direction = (counterPoint.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            
            // Mover hacia adelante
            transform.position = Vector3.MoveTowards(transform.position, counterPoint.position, walkSpeed * Time.deltaTime);
            yield return null;
        }
        
        // Detener animación de caminar
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        hasReachedCounter = true;
        Debug.Log("Cliente llegó al mostrador");
        
        // Mostrar el pergamino con la receta
        manager.ShowRecipeScroll(currentRecipe);
        
        // Esperar 2-3 segundos
        yield return new WaitForSeconds(2.5f);
        
        // Ocultar pergamino y mostrar UI de receta
        manager.HideRecipeScroll();
        manager.ShowRecipeUI(currentRecipe);
        
        Debug.Log("Pergamino ocultado, UI de receta mostrada");
        
        // Dar la vuelta (rotar 180 grados)
        yield return StartCoroutine(TurnAround());
        
        // Caminar de vuelta al spawn
        yield return StartCoroutine(WalkBackToSpawn());
    }
    
    IEnumerator TurnAround()
    {
        Debug.Log("Cliente dándose la vuelta");
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);
        float elapsed = 0f;
        float turnDuration = 0.5f;
        
        while (elapsed < turnDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / turnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = endRotation;
    }
    
    IEnumerator WalkBackToSpawn()
    {
        Debug.Log("Cliente regresando al punto de spawn");
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
        
        while (Vector3.Distance(transform.position, spawnPoint.position) > 0.1f)
        {
            Vector3 direction = (spawnPoint.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            
            transform.position = Vector3.MoveTowards(transform.position, spawnPoint.position, walkSpeed * Time.deltaTime);
            yield return null;
        }
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        Debug.Log("Cliente desapareciendo del spawn");
        
        // Notificar al manager que el cliente se va y debe spawnear los clientes en las mesas
        manager.OnCustomerLeftCounter(currentRecipe);
        
        // Destruir este cliente
        Destroy(gameObject);
    }
    
    public RecipeDataMenu GetCurrentRecipe()
    {
        return currentRecipe;
    }
}