using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Menus")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    [Header("Animators That Should Still Animate")]
    public Animator[] uiAnimators;   // Assign your pause menu animators here

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        foreach (Animator anim in uiAnimators)
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) 
                ResumeGame();

            else
                PauseGame();
            
        }
    }

    // -----------------------------
    // PAUSE / RESUME
    // -----------------------------
    public void PauseGame()
    {   
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        
    }

    // -----------------------------
    // OPTIONS MENU
    // -----------------------------
    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);

        
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

    }
}   