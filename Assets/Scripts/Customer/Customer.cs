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

    [Header("Animator (assign in inspector or will try to GetComponent)")]
    public Animator animator;                      // Animator reference (can assign in inspector)
    public string walkStateName = "Running";          // Animator state name for walking
    public string idleStateName = "Idle State";          // Animator state name for idle
    public float crossfadeDuration = 0.12f;        // crossfade time for smooth transitions

    private RecipeDataMenu currentRecipe;
    private CustomerManager manager;
    private bool hasReachedCounter = false;

    void Start()
    {
        // If not assigned in inspector, try to get it from the GameObject
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("⚠️ No Animator assigned or found on " + gameObject.name + ". Assign one in the inspector.");
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

        // Play walk animation state if animator exists
        if (animator != null)
        {
            // Crossfade into the walk state
            animator.CrossFade(walkStateName, crossfadeDuration);
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

        // Go back to Idle state
        if (animator != null)
        {
            animator.CrossFade(idleStateName, crossfadeDuration);
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
            animator.CrossFade(walkStateName, crossfadeDuration);
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
            animator.CrossFade(idleStateName, crossfadeDuration);
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