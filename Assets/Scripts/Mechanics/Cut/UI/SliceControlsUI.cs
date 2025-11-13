using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la interfaz de usuario para manejar la cantidad de cortes
/// y disparar la acción de cortar en la estación de corte.
/// </summary>
public class SliceControlsUI : MonoBehaviour
{
    [Header("Station Reference")]
    [SerializeField]
    [Tooltip("Referencia a la estación de corte que será controlada.")]
    private SlicingStation slicingStation;

    [Header("UI Elements")]
    [SerializeField]
    [Tooltip("Campo de entrada donde el jugador puede escribir la cantidad de cortes.")]
    private TMP_InputField sliceCountField;

    [SerializeField]
    [Tooltip("Botón para aumentar la cantidad de cortes.")]
    private Button increaseButton;

    [SerializeField]
    [Tooltip("Botón para disminuir la cantidad de cortes.")]
    private Button decreaseButton;

    [SerializeField]
    [Tooltip("Botón para ejecutar la acción de cortar.")]
    private Button cutButton;

    [Header("Slice Limits")]
    [SerializeField]
    [Tooltip("Cantidad mínima de cortes permitida.")]
    private int minSlices = 2;
    
    [SerializeField]
    [Tooltip("Cantidad máxima de cortes permitida.")]
    private int maxSlices = 12;

    /// <summary>
    /// Inicializa los botones y el campo de entrada, y sincroniza la UI
    /// con la estación de corte.
    /// </summary>
    private void Start()
    {
        if (increaseButton != null)
            increaseButton.onClick.AddListener(IncreaseSlices);

        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(DecreaseSlices);

        if (cutButton != null)
            cutButton.onClick.AddListener(OnCutButtonPressed);

        if (sliceCountField != null)
            sliceCountField.onValueChanged.AddListener(OnInputValueChanged);

        if (slicingStation != null)
        {
            // Inicializa la UI con la cantidad actual de cortes, respetando los límites
            int initialCount = Mathf.Clamp(slicingStation.sliceCount, minSlices, maxSlices);
            UpdateUI(initialCount);
        }
    }
    
    /// <summary>
    /// Establece la cantidad de cortes desde otro script y actualiza la UI.
    /// </summary>
    public void SetSliceCount(int count)
    {
        UpdateUI(count);
    }

    /// <summary>
    /// Incrementa la cantidad de cortes en 1 y actualiza la UI.
    /// </summary>
    private void IncreaseSlices()
    {
        int currentCount = slicingStation.sliceCount;
        UpdateUI(currentCount + 1);
    }

    /// <summary>
    /// Decrementa la cantidad de cortes en 1 y actualiza la UI.
    /// </summary>
    private void DecreaseSlices()
    {
        int currentCount = slicingStation.sliceCount;
        UpdateUI(currentCount - 1);
    }

    /// <summary>
    /// Se llama cuando el jugador modifica manualmente el valor en el campo de entrada.
    /// </summary>
    private void OnInputValueChanged(string value)
    {
        if (int.TryParse(value, out int newCount))
        {
            UpdateUI(newCount);
        }
    }

    /// <summary>
    /// Se llama al presionar el botón de cortar, ejecutando el corte en la estación.
    /// </summary>
    private void OnCutButtonPressed()
    {
        if (slicingStation != null)
        {
            slicingStation.SliceObject();
        }
    }

    /// <summary>
    /// Actualiza la UI y sincroniza la cantidad de cortes con la estación.
    /// </summary>
    private void UpdateUI(int count)
    {
        int clampedCount = Mathf.Clamp(count, minSlices, maxSlices);

        if (slicingStation != null)
        {
            slicingStation.sliceCount = clampedCount;
        }

        if (sliceCountField != null)
        {
            sliceCountField.text = clampedCount.ToString();
        }
    }
}
