using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Administra el flujo completo:
/// 1. Mueve personajes a posición inicial.
/// 2. Inicia cinemática.
/// 3. Inicia diálogo.
/// 4. Muestra mensaje final.
/// </summary>
public class CinematicManager : MonoBehaviour
{
    [Header("Characters Movement Before Cinematic")]
    public List<CubeMover> charactersToMove;

    /// <summary>
    /// Cinemática a ejecutar una vez que los personajes llegaron
    /// a su posición inicial.
    /// </summary>
    [Header("Cinematic")]
    public SimpleCinematic cinematic;

    [Header("Dialogue")]
    public DialogueLine[] dialogue;

    [Header("Final Message")]
    public GameObject finalPanel;
    public TMP_Text finalMessage;
    [TextArea(2, 4)]
    public string customFinalMessage;

    private int arrivedCount = 0;

    private void Start()
    {
        if (finalPanel != null)
            finalPanel.SetActive(false);

        cinematic.OnCinematicEnd += HandleCinematicEnd;
        DialogueSystemCinematic.Instance.OnDialogueFinished += HandleDialogueEnd;

        // Que ambos procesos inicien juntos
        MoveCharactersBeforeCinematic();
        StartCinematic();
    }


    /// <summary>
    /// Manda a los personajes a su posición inicial antes de iniciar la cinemática.
    /// </summary>
    private void MoveCharactersBeforeCinematic()
    {
        if (charactersToMove == null || charactersToMove.Count == 0)
        {
            StartCinematic();
            return;
        }

        arrivedCount = 0;

        foreach (CubeMover mover in charactersToMove)
        {
            mover.MoveTo();
        }
    }


    private void StartCinematic()
    {
        cinematic.Play(); // Asegúrate de que tu SimpleCinematic tenga este método
    }

    private void HandleCinematicEnd()
    {
        DialogueSystemCinematic.Instance.StartDialogue(dialogue);
    }

    private void HandleDialogueEnd()
    {
        if (finalPanel == null) return;

        finalPanel.SetActive(true);
        finalMessage.text = customFinalMessage;
    }
}
