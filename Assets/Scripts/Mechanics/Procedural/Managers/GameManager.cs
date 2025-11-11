using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

/// <summary>
/// Administrador principal del juego que controla las tareas de corte de ingredientes.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Task List")]
    [SerializeField]
    [Tooltip("Lista de tareas de corte que el jugador debe completar.")]
    private List<SliceTask> tasks;

    [Header("Scene References")]
    [SerializeField]
    [Tooltip("Referencia al TextMeshProUGUI que muestra la información de la tarea actual.")]
    private TextMeshProUGUI taskText;

    [SerializeField]
    [Tooltip("UI RawImage that displays the task image.")]
    private RawImage taskImageDisplay;


    /// <summary>
    /// Índice de la tarea actual en la lista de tareas.
    /// </summary>
    private int currentTaskIndex = -1;

    /// <summary>
    /// Referencia a la tarea actual.
    /// </summary>
    private SliceTask currentTask;

    /// <summary>
    /// Lista de piezas generadas en el corte actual.
    /// </summary>
    private List<GameObject> currentCutPieces = new List<GameObject>();

    /// <summary>
    /// Inicializa el juego y comienza la primera tarea.
    /// </summary>
    void Start()
    {
        GoToNextTask();
    }

    /// <summary>
    /// Avanza a la siguiente tarea, limpia el tablero y actualiza el texto.
    /// </summary>
    private void GoToNextTask()
    {
        ClearBoard();

        currentTaskIndex++;
        if (currentTaskIndex >= tasks.Count)
        {
            taskText.text = "All tasks complete!";
            if (taskImageDisplay != null)
                taskImageDisplay.texture = null;
            return;
        }

        currentTask = tasks[currentTaskIndex];
        taskText.text = $"{currentTask.taskName} x{currentTask.requiredSlices}";

        if (taskImageDisplay != null)
            taskImageDisplay.texture = currentTask.taskTexture;
    }


    /// <summary>
    /// Llamado cuando un objeto ha sido cortado.
    /// Evalúa si el corte fue correcto según la tarea actual.
    /// </summary>
    /// <param name="originalCutObject">Objeto original que se cortó.</param>
    /// <param name="pieces">Lista de piezas resultantes del corte.</param>
    /// <param name="pieceCount">Cantidad de piezas resultantes.</param>
    public void OnCutComplete(GameObject originalCutObject, List<GameObject> pieces, int pieceCount)
    {
        currentCutPieces = pieces;

        bool correctItem = originalCutObject.name.StartsWith(currentTask.ingredientPrefab.name);
        bool correctCount = (pieceCount == currentTask.requiredSlices);

        if (correctItem && correctCount)
        {
            taskText.text = "Success!";
            StartCoroutine(HandleSuccess());
        }
        else
        {
            string failureReason = "";
            if (!correctItem) failureReason = $"That wasn't the right item!";
            else if (!correctCount) failureReason = $"Oops! That's {pieceCount} pieces.";

            taskText.text = failureReason;
            StartCoroutine(HandleFailure());
        }
    }

    /// <summary>
    /// Coroutine que maneja el éxito del corte, espera un tiempo y avanza a la siguiente tarea.
    /// </summary>
    private IEnumerator HandleSuccess()
    {
        yield return new WaitForSeconds(2.0f);
        GoToNextTask();
    }

    /// <summary>
    /// Coroutine que maneja un corte incorrecto, espera un tiempo y restablece el tablero.
    /// </summary>
    private IEnumerator HandleFailure()
    {
        yield return new WaitForSeconds(2.0f);
        ClearBoard();

        taskText.text = $"Task: Cut the {currentTask.taskName} into {currentTask.requiredSlices} pieces.";
    }

    /// <summary>
    /// Elimina todas las piezas del corte anterior del tablero.
    /// </summary>
    private void ClearBoard()
    {
        if (currentCutPieces == null) return;

        foreach (GameObject piece in currentCutPieces)
        {
            if (piece != null)
            {
                Destroy(piece);
            }
        }

        currentCutPieces.Clear();
    }
}
