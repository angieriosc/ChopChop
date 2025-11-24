using UnityEngine;

/// <summary>
/// Estación de entrega (DeliveryStation).
/// Cuando el jugador entra al collider, muestra un Canvas de UI.
/// Cuando sale, lo oculta (opcional).
/// </summary>
public class DeliveryStation : MonoBehaviour
{
    [Header("UI de la estación de entrega")]
    [SerializeField] private GameObject _deliveryCanvas;
    [SerializeField] private Camera _stationCamera;

    [Header("Configuración de detección")]
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private bool _hideCanvasOnExit = true;
    [SerializeField] private bool _hideCameraOnExit = true;

    private bool _playerInStation = false;

    /// <summary>
    /// Indica si el jugador está dentro del área de la estación.
    /// </summary>
    public bool IsPlayerInside() => _playerInStation;

    /// <summary>
    /// Muestra el canvas de la estación (si está asignado).
    /// </summary>
    public void ShowCanvas()
    {
        if (_deliveryCanvas != null)
            _deliveryCanvas.SetActive(true);
            _stationCamera.gameObject.SetActive(true);
    }

    /// <summary>
    /// Oculta el canvas de la estación (si está asignado).
    /// </summary>
    public void HideCanvas()
    {
        if (_deliveryCanvas != null)
            _deliveryCanvas.SetActive(false);
            _stationCamera.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;

        _playerInStation = true;
        ShowCanvas();
        Debug.Log("[DeliveryStation] Jugador entró a la estación de entrega.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;

        _playerInStation = false;

        if (_hideCanvasOnExit)
            HideCanvas();

        Debug.Log("[DeliveryStation] Jugador salió de la estación de entrega.");
    }
}
