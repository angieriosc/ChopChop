using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelResultsUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject resultsPanel; // El panel completo (para prenderlo/apagarlo)
    [SerializeField] private TextMeshProUGUI recipesListText; // El texto grande del centro
    [SerializeField] private TextMeshProUGUI overallScoreText; // El texto de abajo "Overall: X%"

    [Header("Configuración de Escenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = "CinematicaFinal"; 

    // Referencia al manager que tiene los datos
    private CustomerManager customerManager;

    private void Start()
    {
        // Al inicio, asegurarnos de que el panel esté oculto
        if (resultsPanel != null) resultsPanel.SetActive(false);
        
        customerManager = FindFirstObjectByType<CustomerManager>();
    }

    /// <summary>
    /// Este método se llama cuando termina el nivel para mostrar la tabla.
    /// </summary>
    public void ShowResults()
    {
        if (customerManager == null)
        {
            Debug.LogError("No se encontró CustomerManager para leer los resultados.");
            return;
        }

        // 1. Activar el panel y pausar el juego
        if (resultsPanel != null) resultsPanel.SetActive(true);
        Time.timeScale = 0f; // Pausa el juego
        Cursor.lockState = CursorLockMode.None; // Libera el ratón
        Cursor.visible = true;

        // 2. Construir la lista de recetas
        List<SatisfactionResult> history = customerManager.satisfactionHistory;
        
        string listContent = "";
        float totalScore = 0f;

        if (history.Count > 0)
        {
            foreach (var result in history)
            {
                // Formato: "Pizza Margarita....... 85%"
                // Usamos :F0 para que no salgan decimales (85 en vez de 85.43)
                listContent += $"{result.recipeName}....... <color=yellow>{result.finalScore:F0}%</color>\n";
                
                totalScore += result.finalScore;
            }

            // 3. Calcular Promedio General
            float average = totalScore / history.Count;
            
            recipesListText.text = listContent;
            
            // Color dinámico según la nota final
            string colorHex = average >= 70 ? "green" : (average >= 40 ? "yellow" : "red");
            overallScoreText.text = $"Overall: <color={colorHex}>{average:F0}%</color>";
        }
        else
        {
            recipesListText.text = "No se entregaron órdenes.";
            overallScoreText.text = "Overall: 0%";
        }
    }

    // --- FUNCIONES PARA LOS BOTONES ---

    public void Button_Retry()
    {
        Time.timeScale = 1f; // IMPORTANTE: Despausar antes de cambiar escena
        // Recarga la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Button_Continue()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelSceneName);
    }

    public void Button_MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}