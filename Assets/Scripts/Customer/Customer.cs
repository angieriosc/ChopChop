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

    private bool interacted = false;


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

        if (animator != null)
            animator.CrossFade(walkStateName, crossfadeDuration);

        while (Vector3.Distance(transform.position, counterPoint.position) > 0.1f)
        {
            Vector3 direction = (counterPoint.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            transform.position = Vector3.MoveTowards(transform.position, counterPoint.position, walkSpeed * Time.deltaTime);
            yield return null;
        }

        if (animator != null)
            animator.CrossFade(idleStateName, crossfadeDuration);

        Debug.Log("Cliente llegó al mostrador y espera interacción");

        // Registrar este cliente en la zona de interacción
        InteractionZone zone = FindFirstObjectByType<InteractionZone>();
        if (zone != null)
            zone.currentCustomer = this;

        hasReachedCounter = true;
    }

    public void OnInteract()
    {
        if (interacted) return; // evitar doble interacción
        interacted = true;

        Debug.Log("Jugador interactuó con el cliente");

        StartCoroutine(DeliverRecipeAndLeave());
    }

    IEnumerator DeliverRecipeAndLeave()
    {
        // Mostrar el pergamino
        manager.ShowRecipeScroll(currentRecipe);

        yield return new WaitForSeconds(2.5f);

        manager.HideRecipeScroll();
        manager.ShowRecipeUI(currentRecipe);

        Debug.Log("UI mostrada después de interactuar");

        // Girar y retirarse
        yield return StartCoroutine(TurnAround());
        yield return StartCoroutine(WalkBackToSpawn());
    }


    IEnumerator TurnAround()
    {
        InteractionZone zone = FindFirstObjectByType<InteractionZone>();
        if (zone != null)
            zone.ClearCustomer();

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