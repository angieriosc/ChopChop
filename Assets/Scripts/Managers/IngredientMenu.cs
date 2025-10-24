using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Controla el menú de selección y spawn de ingredientes con scroll.
/// </summary>
public class IngredientMenu : MonoBehaviour
{
    [Header("Prefabs de ingredientes")]
    [SerializeField] private GameObject[] ingredientPrefabs;

    [Header("Punto de aparición")]
    [SerializeField] private Transform spawnPoint;

    [Header("UI")]
    [SerializeField] private Button[] ingredientButtons;
    [SerializeField] private Button exitButton;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Referencias del jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FollowPlayer cameraFollow;
    [SerializeField] private PouringStation pouringStation;

    [SerializeField] private IngredientDivider ingredientDivider;


    // Internos
    private GameObject currentIngredient;

    private Button selectedButton;
    
    private void Start()
    {
        AssignButtonListeners();
    }

    /// <summary>
    /// Asigna eventos a los botones de ingredientes y al de salida.
    /// </summary>
    private void AssignButtonListeners()
    {
        for (int i = 0; i < ingredientButtons.Length; i++)
        {
            int index = i;
            ingredientButtons[i].onClick.AddListener(() => OnIngredientSelected(index));
        }

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitMenu);
    }

    /// <summary>
    /// Maneja la selección de un ingrediente desde el menú.
    /// </summary>
    private void OnIngredientSelected(int index)
    {
        if (index < 0 || index >= ingredientPrefabs.Length)
            return;

        // 🔹 Destruir tazas existentes del ingrediente anterior
        if (ingredientDivider != null && ingredientDivider.spawnParent != null)
        {
            PouringContainer[] existingCups = ingredientDivider.spawnParent
                .GetComponentsInChildren<PouringContainer>();
            foreach (var cup in existingCups)
                Destroy(cup.gameObject);
        }

        // 🔹 Destruir ingrediente actual si existe
        if (currentIngredient != null)
            Destroy(currentIngredient);

        // 🔹 Instanciar nuevo ingrediente
        GameObject prefab = ingredientPrefabs[index];
        currentIngredient = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        // 🔹 Asignar a la estación y configurar targetContainer
        PouringContainer pouring = currentIngredient.GetComponent<PouringContainer>();
        if (pouring != null)
        {
            pouringStation.SetActivePouring(pouring);
            pouring.targetContainer = pouringStation.CurrentReceiving;
            Debug.Log($"Asignado targetContainer: {pouring.targetContainer.name}");
        }
        else
        {
            Debug.LogWarning($"{prefab.name} no tiene componente PouringContainer.");
        }

        // 🔹 Centrar el scroll en el botón seleccionado
        HighlightSelectedButton(ingredientButtons[index]);
        
    }


    /// <summary>
    /// Resalta el botón seleccionado y deselecciona el anterior.
    /// </summary>
    private void HighlightSelectedButton(Button newButton)
    {
        if (selectedButton != null)
            selectedButton.transform.localScale = Vector3.one;

        selectedButton = newButton;
        selectedButton.transform.localScale = Vector3.one * 1.2f;
    }


    /// <summary>
    /// Cierra el menú y desbloquea jugador y cámara.
    /// </summary>
    private void ExitMenu()
    {
        gameObject.SetActive(false);

        pouringStation?.UnlockPlayer();
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraFollow != null) cameraFollow.enabled = true;

        Debug.Log("Menú cerrado. Jugador desbloqueado.");
    }
}
