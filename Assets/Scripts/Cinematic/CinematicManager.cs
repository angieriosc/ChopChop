using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CinematicManager : MonoBehaviour
{
    [Header("Characters Movement Before Cinematic")]
    public List<CubeMover> charactersToMove;

    [Header("Cinematic")]
    public SimpleCinematic cinematic;

    [Header("Dialogue")]
    public DialogueLine[] dialogue;

    [Header("Final Message")]
    public GameObject finalPanel;
    public TMP_Text finalMessage;
    [TextArea(2, 4)]
    public string customFinalMessage;

    [Header("Next Scene")]
    public string nextSceneName;

    [Header("Final Button")]
    public Button finalButton;   // ← Asignar el botón final aquí

    /// <summary>
    /// Inicializa el flujo general:
    /// - Asigna el listener del botón final.
    /// - Oculta el panel final.
    /// - Se suscribe a los eventos de fin de cinemática y diálogo.
    /// - Mueve personajes previos a la cinemática.
    /// - Inicia la cinemática.
    /// </summary>

    private void Start()
    {
        if (finalButton != null)
            finalButton.onClick.AddListener(LoadNextScene);

        if (finalPanel != null)
            finalPanel.SetActive(false);

        cinematic.OnCinematicEnd += HandleCinematicEnd;
        DialogueSystemCinematic.Instance.OnDialogueFinished += HandleDialogueEnd;

        MoveCharactersBeforeCinematic();
        StartCinematic();
    }

    /// <summary>
    /// Ordena a cada personaje configurado que ejecute su movimiento
    /// inicial antes de que comience la cinemática.
    /// </summary>
    private void MoveCharactersBeforeCinematic()
    {
        foreach (CubeMover mover in charactersToMove)
            mover.MoveTo();
    }

    /// <summary>
    /// Inicia la reproducción de la cinemática principal.
    /// </summary>
    private void StartCinematic()
    {
        cinematic.Play();
    }

    /// <summary>
    /// Evento ejecutado cuando la cinemática termina.
    /// Inicia el sistema de diálogo usando las líneas configuradas.
    /// </summary>
    private void HandleCinematicEnd()
    {
        DialogueSystemCinematic.Instance.StartDialogue(dialogue);
    }

    /// <summary>
    /// Evento ejecutado cuando el diálogo finaliza.
    /// Muestra el panel final y coloca el mensaje personalizado.
    /// </summary>
    private void HandleDialogueEnd()
    {
        if (finalPanel == null) return;

        finalPanel.SetActive(true);
        finalMessage.text = customFinalMessage;
    }
    
    /// <summary>
    /// Carga la siguiente escena configurada.
    /// Valida que el nombre no esté vacío antes de proceder.
    /// </summary>
    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("nextSceneName no asignado");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
