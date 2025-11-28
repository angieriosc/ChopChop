using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelResultsUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject resultsPanel;
    [Header("Texto de Recetas")]
    [SerializeField] private TextMeshProUGUI recipesListText;

    [Header("Texto de Puntaje General")]
    [SerializeField] private TextMeshProUGUI overallScoreText;

    [Header("Configuración de Escenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = "CinematicaFinal"; 
    private CustomerManager customerManager;

    private void Start()
    {
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

        if (resultsPanel != null) resultsPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        List<SatisfactionResult> history = customerManager.satisfactionHistory;
        
        string listContent = "";
        float totalScore = 0f;

        if (history.Count > 0)
        {
            foreach (var result in history)
            {
                listContent += $"{result.recipeName}....... <color=yellow>{result.finalScore:F0}%</color>\n";
                
                totalScore += result.finalScore;
            }

            float average = totalScore / history.Count;
            
            recipesListText.text = listContent;
            
            string colorHex = average >= 70 ? "green" : (average >= 40 ? "yellow" : "red");
            overallScoreText.text = $"Overall: <color={colorHex}>{average:F0}%</color>";
        }
        else
        {
            recipesListText.text = "No se entregaron órdenes.";
            overallScoreText.text = "Overall: 0%";
        }
    }

    /// <summary>
    /// Reinicia el nivel actual.
    /// </summary>
    public void Button_Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Carga la cinematica final.
    /// </summary>
    public void Button_Continue()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelSceneName);
    }

    /// <summary>
    /// Regresa al menú principal.
    /// </summary>
    public void Button_MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}