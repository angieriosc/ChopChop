using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

/// <summary>
/// Administrador principal del juego.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField]
    [Tooltip("Referencia al TextMeshProUGUI que muestra la información.")]
    private TextMeshProUGUI taskText;

    [SerializeField]
    [Tooltip("UI RawImage that displays the task image.")]
    private RawImage taskImageDisplay;

    private SlicingStation currentSlicingStation;

    /// <summary>
    /// Lista de piezas generadas en el corte actual.
    /// </summary>
    private List<GameObject> currentCutPieces = new List<GameObject>();

    /// <summary>
    /// Inicializa el juego y muestra un mensaje de bienvenida.
    /// </summary>
    void Start()
    {
        if (taskText != null)
        {
            taskText.text = "Place an item on the station to cut.";
        }

        if (taskImageDisplay != null)
        {
            taskImageDisplay.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Llamado cuando un objeto ha sido cortado.
    /// Acepta CUALQUIER corte, lo reporta y reinicia el tablero.
    /// </summary>
    public void OnCutComplete(GameObject originalCutObject, List<GameObject> pieces, int pieceCount, SlicingStation station)
    {

        this.currentSlicingStation = station;
        currentCutPieces = pieces;
        
        if (taskText != null)
        {
            taskText.text = $"Success! You cut {pieceCount} pieces.";
        }

        StartCoroutine(ResetAfterCut());
    }

    /// <summary>
    /// Coroutine that handles resetting the board after a cut.
    /// </summary>
    private IEnumerator ResetAfterCut()
    {
        yield return new WaitForSeconds(2.0f);

        if (currentSlicingStation != null)
        {
            currentSlicingStation.UnlockPlayer();
            currentSlicingStation = null;
        }

        ClearBoard();

        if (taskText != null)
        {
            taskText.text = "Place an item on the station to cut.";
        }
    }
    
    /// <summary>
    /// Resets the text and clears all cut pieces from the board.
    /// </summary>
    public void ResetBoard()
    {
        ClearBoard();

        if (taskText != null)
        {
            taskText.text = "Place an item on the station to cut.";
        }
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