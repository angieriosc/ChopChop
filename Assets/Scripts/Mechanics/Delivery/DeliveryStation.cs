using UnityEngine;

/// <summary>
/// Estación de entrega (DeliveryStation).
/// Cuando el jugador entra al collider, muestra un Canvas de UI
/// y activa la cámara especial de entrega.
/// Al salir, lo oculta todo.
/// </summary>
public class DeliveryStation : MonoBehaviour
{
    [Header("UI y Cámara")]
    [SerializeField] private GameObject _deliveryCanvas;
    [SerializeField] private GameObject _botonfinalizar;
    [SerializeField] private Camera _stationCamera;
    [SerializeField] private CustomerManager customerManager;   
    [SerializeField] private RecipeUIManager _recipeUiManager;

    [Header("Sistema de entrega (manager que raycastea y coloca rebanadas)")]
    [SerializeField] private PizzaDeliveryManager _deliveryManager;

    [Header("Configuración de detección")]
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private bool _hideCanvasOnExit = true;
    [SerializeField] private bool _hideCameraOnExit = true;

    private bool _playerInStation = false;

    public bool IsPlayerInside() => _playerInStation;

    /// <summary>
    /// Activa UI, cámara y manager de entrega.
    /// </summary>
    public void ShowCanvas()
    {
        if (_deliveryCanvas != null)
            _deliveryCanvas.SetActive(true);
        if (_botonfinalizar != null)
        {
            bool mostrarBoton = false;

            if (_recipeUiManager != null)
            {
                mostrarBoton = _recipeUiManager.ActiveRecipe.IsRecipeCompleted();
            }

            _botonfinalizar.SetActive(mostrarBoton);
        }
        if (_stationCamera != null)
            _stationCamera.gameObject.SetActive(true);

        if (_deliveryManager != null)
        {
            _deliveryManager.WorkCamera = _stationCamera;
            _deliveryManager.SetEnabled(true);
        }
    }

    /// <summary>
    /// Desactiva UI, cámara y manager.
    /// </summary>
    public void HideCanvas()
    {
        if (_deliveryCanvas != null)
            _deliveryCanvas.SetActive(false);

        if (_stationCamera != null && _hideCameraOnExit)
            _stationCamera.gameObject.SetActive(false);

        if (_deliveryManager != null)
            _deliveryManager.SetEnabled(false);
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
