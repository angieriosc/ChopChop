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
    [SerializeField] private SlicingStation slicingStation;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField sliceCountField;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button cutButton;

    [Header("Slice Limits")]
    [SerializeField] private int minSlices = 2;
    [SerializeField] private int maxSlices = 12;

    /// <summary>
    /// Inicializa los botones y el campo de entrada, y sincroniza la UI
    /// con la estación de corte.
    /// </summary>
    private void Start()
    {
        if (increaseButton != null) increaseButton.onClick.AddListener(IncreaseSlices);
        if (decreaseButton != null) decreaseButton.onClick.AddListener(DecreaseSlices);
        if (cutButton != null) cutButton.onClick.AddListener(OnCutButtonPressed);
        if (sliceCountField != null) sliceCountField.onValueChanged.AddListener(OnInputValueChanged);

        SetButtonsState(true);

        if (slicingStation != null)
        {
            int initialCount = Mathf.Clamp(slicingStation.sliceCount, minSlices, maxSlices);
            UpdateUI(initialCount);
        }
    }

    /// <summary>
    /// Habilita los controles cuando el objeto se activa.
    /// </summary>
    private void OnEnable()
    {
        SetButtonsState(true);
    }

    public void SetSliceCount(int count)
    {
        UpdateUI(count);
    }

    /// <summary>
    /// Resetea los controles a su estado inicial, habilitando los botones.
    /// </summary>
    public void ResetControls()
    {
        SetButtonsState(true);
    }

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
    /// Actualiza la cantidad de cortes cuando el valor del campo de texto cambia.
    /// </summary>
    /// <param name="value">Nuevo valor ingresado en el campo de texto.</param>
    private void OnInputValueChanged(string value)
    {
        if (int.TryParse(value, out int newCount))
        {
            UpdateUI(newCount);
        }
    }

    /// <summary>
    /// Maneja la lógica cuando se presiona el botón de cortar.
    /// Desactiva los controles y llama a la estación para realizar el corte.
    /// </summary>
    private void OnCutButtonPressed()
    {
        SetButtonsState(false);
        gameObject.SetActive(false);

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

    /// <summary>
    /// Habilita o deshabilita los botones y el campo de entrada.
    /// </summary>
    private void SetButtonsState(bool isActive)
    {
        if (increaseButton != null) increaseButton.interactable = isActive;
        if (decreaseButton != null) decreaseButton.interactable = isActive;
        if (cutButton != null) cutButton.interactable = isActive;
        if (sliceCountField != null) sliceCountField.interactable = isActive;
    }
}