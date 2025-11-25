using UnityEngine;
using TMPro;

/// <summary>
/// Administra el flujo completo entre una cinemática inicial,
/// el sistema de diálogos y la visualización de un mensaje final.
/// 
/// Flujo:
/// 1. Inicia una cinemática.
/// 2. Al terminar la cinemática, inicia un diálogo.
/// 3. Al finalizar el diálogo, muestra un panel final con un mensaje.
/// </summary>
public class CinematicManager : MonoBehaviour
{
    /// <summary>
    /// Referencia a la cinemática que se ejecutará al iniciar la escena.
    /// </summary>
    public SimpleCinematic cinematic;

    [Header("Dialogue")]
    /// <summary>
    /// Lista de líneas de diálogo que se mostrarán
    /// después de que termine la cinemática.
    /// </summary>
    public DialogueLine[] dialogue;

    [Header("Final Message")]
    /// <summary>
    /// Panel que se mostrará al finalizar todo el flujo
    /// (cinemática + diálogo). Puede estar oculto inicialmente.
    /// </summary>
    public GameObject finalPanel;

    /// <summary>
    /// Texto dentro del panel final donde se mostrará
    /// el mensaje de cierre personalizado.
    /// </summary>
    public TMP_Text finalMessage;

    /// <summary>
    /// Mensaje final personalizado que se presentará
    /// cuando concluya el diálogo.
    /// </summary>
    [TextArea(2, 4)]
    public string customFinalMessage;

    /// <summary>
    /// Inicializa el flujo:
    /// - Oculta el panel final si existe.
    /// - Conecta los eventos de fin de cinemática y diálogo
    ///   con sus respectivos manejadores.
    /// </summary>
    private void Start()
    {
        if (finalPanel != null) finalPanel.SetActive(false);

        cinematic.OnCinematicEnd += HandleCinematicEnd;
        DialogueSystemCinematic.Instance.OnDialogueFinished += 
            HandleDialogueEnd;
    }

    /// <summary>
    /// Manejador ejecutado cuando la cinemática termina.
    /// Inicia el diálogo configurado.
    /// </summary>
    private void HandleCinematicEnd()
    {
        DialogueSystemCinematic.Instance.StartDialogue(dialogue);
    }

    /// <summary>
    /// Manejador ejecutado cuando el diálogo termina.
    /// Si existe un panel final, lo muestra y coloca el mensaje personalizado.
    /// </summary>
    private void HandleDialogueEnd()
    {
        if (finalPanel == null) return;

        finalPanel.SetActive(true);
        finalMessage.text = customFinalMessage;
    }
}
