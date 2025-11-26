using UnityEngine;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Sistema de diálogo para cinemáticas que controla:
/// - Tipeo progresivo del texto
/// - Animación del personaje que habla
/// - Enfoque dinámico de cámara
/// - Avance y salto del diálogo
/// </summary>
public class DialogueSystemCinematic : MonoBehaviour
{
    public static DialogueSystemCinematic Instance;

    [Header("UI Elements")]
    public TMP_Text actorNameText;
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;

    [Header("Camera")]
    public Camera cinematicCamera;
    public float cameraFocusSpeed = 5f;

    public Action OnDialogueFinished;

    private DialogueLine[] currentLines;
    private int index;

    private bool isTyping;
    private bool skipCinematic;

    private Animator currentAnimator;
    private string talkingBool = "IsTalking";

    /// <summary>
    /// Asigna la instancia estática para acceso global al sistema.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Procesa la lógica principal del sistema mientras el panel está activo:
    /// - Detectar salto del diálogo
    /// - Detectar avance o despliegue instantáneo
    /// - Mantener enfoque de cámara en el orador
    /// </summary>
    private void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        HandleSkipInput();
        HandleAdvanceInput();
        HandleCameraFocus();
    }

    /// <summary>
    /// Permite saltar todo el diálogo presionando ESC.
    /// Finaliza el diálogo inmediatamente.
    /// </summary>
    private void HandleSkipInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        skipCinematic = true;
        StopTalkingAnimation();
        EndDialogue();
    }

    /// <summary>
    /// Maneja el clic o la barra espaciadora:
    /// - Si está escribiendo, completa la línea al instante
    /// - Si ya terminó, avanza a la siguiente línea
    /// </summary>
    private void HandleAdvanceInput()
    {
        bool pressed =
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space);

        if (!pressed) return;

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentLines[index].text;
            isTyping = false;
            StopTalkingAnimation();
        }
        else
        {
            NextLine();
        }
    }

    /// <summary>
    /// Inicia un diálogo cargando el arreglo de líneas y mostrando la primera.
    /// </summary>
    public void StartDialogue(DialogueLine[] lines)
    {
        currentLines = lines;
        index = 0;
        skipCinematic = false;

        dialoguePanel.SetActive(true);
        ShowLine();
    }

    /// <summary>
    /// Configura y muestra la línea actual:
    /// - Asigna nombre del actor
    /// - Activa animación de hablar
    /// - Inicia el tipeo progresivo
    /// </summary>
    private void ShowLine()
    {
        var line = currentLines[index];

        actorNameText.text = line.actorName;
        currentAnimator = line.actorAnimator;
        talkingBool = line.talkingBoolName;

        StartTalkingAnimation();

        StopAllCoroutines();
        StartCoroutine(TypeLine(line.text));
    }

    /// <summary>
    /// Escribe el texto carácter por carácter para generar efecto de tipeo.
    /// Detiene la animación al finalizar.
    /// </summary>
    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
        StopTalkingAnimation();
    }

    /// <summary>
    /// Avanza a la siguiente línea o finaliza el diálogo si ya no hay más.
    /// </summary>
    public void NextLine()
    {
        index++;

        if (index >= currentLines.Length)
        {
            StopTalkingAnimation();
            EndDialogue();
            return;
        }

        ShowLine();
    }

    /// <summary>
    /// Oculta el panel y ejecuta el evento de fin de diálogo.
    /// </summary>
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        OnDialogueFinished?.Invoke();
    }

    /// <summary>
    /// Apunta la cámara hacia el objetivo definido para la línea actual,
    /// interpolando suavemente su rotación.
    /// </summary>
    private void HandleCameraFocus()
    {
        if (index >= currentLines.Length) return;

        var target = currentLines[index].focusTarget;
        if (target == null) return;

        Vector3 dir = target.position - cinematicCamera.transform.position;
        Quaternion rot = Quaternion.LookRotation(dir);

        cinematicCamera.transform.rotation = Quaternion.Lerp(
            cinematicCamera.transform.rotation,
            rot,
            Time.deltaTime * cameraFocusSpeed
        );
    }

    /// <summary>
    /// Activa la animación de "hablar" del actor actual.
    /// </summary>
    private void StartTalkingAnimation()
    {
        if (currentAnimator == null || string.IsNullOrEmpty(talkingBool))
            return;

        currentAnimator.SetBool(talkingBool, true);
    }

    /// <summary>
    /// Desactiva la animación de "hablar" del actor actual.
    /// </summary>
    private void StopTalkingAnimation()
    {
        if (currentAnimator == null || string.IsNullOrEmpty(talkingBool))
            return;

        currentAnimator.SetBool(talkingBool, false);
    }
}
