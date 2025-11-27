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

    private void MoveCharactersBeforeCinematic()
    {
        foreach (CubeMover mover in charactersToMove)
            mover.MoveTo();
    }

    private void StartCinematic()
    {
        cinematic.Play();
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
