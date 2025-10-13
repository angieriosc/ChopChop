using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la UI del menú de ingredientes, spawn de prefabs y salida del menú.
/// </summary>
public class IngredientMenu : MonoBehaviour
{
    // 1. Variables públicas y serializadas
    [Header("Lista de prefabs de ingredientes")]
    [SerializeField] private GameObject[] ingredientPrefabs;

    [Header("Punto de spawn en la escena")]
    [SerializeField] private Transform spawnPoint;

    [Header("Botones de ingredientes")]
    [SerializeField] private Button[] ingredientButtons;

    [Header("Botón de salida")]
    [SerializeField] private Button exitButton;

    [Header("Referencias al jugador y estación")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FollowPlayer cameraFollow;
    [SerializeField] private PouringStation pouringStation;

    // 3. Métodos de Unity
    private void Start()
    {
        AssignButtonListeners();
    }

    // 6. Métodos privados auxiliares
    /// <summary>
    /// Asigna los eventos de clic a los botones de ingredientes y de salida.
    /// </summary>
    private void AssignButtonListeners()
    {
        for (int i = 0; i < ingredientButtons.Length; i++)
        {
            int index = i;
            ingredientButtons[i].onClick.AddListener(() => SpawnIngredient(index));
        }

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitMenu);
    }

    /// <summary>
    /// Genera el prefab del ingrediente correspondiente al índice.
    /// </summary>
    /// <param name="index">Índice del ingrediente en el array</param>
    private void SpawnIngredient(int index)
    {
        if (index < 0 || index >= ingredientPrefabs.Length) return;

        GameObject prefab = ingredientPrefabs[index];
        Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        Debug.Log($"Se generó {prefab.name} en la escena");
    }

    /// <summary>
    /// Cierra el menú de ingredientes y desbloquea al jugador y cámara.
    /// </summary>
    private void ExitMenu()
    {
        gameObject.SetActive(false);

        if (pouringStation != null)
            pouringStation.UnlockPlayer();

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        Debug.Log("Menú de ingredientes cerrado. Jugador desbloqueado.");
    }
}
