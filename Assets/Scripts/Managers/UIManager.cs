using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona la UI del conteo de cortes y checkmark al alcanzar el objetivo.
/// </summary>
public class UIManager : MonoBehaviour
{
    // 1. Variables públicas y serializadas
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private GameObject checkmarkImageObject;

    // 3. Métodos de Unity
    private void OnEnable()
    {
        ObjectGrabbing.OnCuttingStarted += HandleCuttingStarted;
        HoverAndCut.OnCutCountUpdated += HandleCutCountUpdated;
        HoverAndCut.OnTargetCutsReached += HandleTargetReached;
    }

    private void OnDisable()
    {
        ObjectGrabbing.OnCuttingStarted -= HandleCuttingStarted;
        HoverAndCut.OnCutCountUpdated -= HandleCutCountUpdated;
        HoverAndCut.OnTargetCutsReached -= HandleTargetReached;
    }

    private void Start()
    {
        checkmarkImageObject.SetActive(false);
        countText.gameObject.SetActive(false);
    }

    // 6. Métodos privados auxiliares
    /// <summary>
    /// Maneja el inicio del corte, mostrando el contador y ocultando el checkmark.
    /// </summary>
    private void HandleCuttingStarted()
    {
        checkmarkImageObject.SetActive(false);
        countText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Actualiza el contador de cortes en la UI.
    /// </summary>
    /// <param name="current">Cantidad de cortes actuales</param>
    /// <param name="target">Cantidad de cortes objetivo</param>
    private void HandleCutCountUpdated(int current, int target)
    {
        if (countText != null)
            countText.text = $"{current} / {target}";
    }

    /// <summary>
    /// Maneja la llegada al objetivo de cortes, mostrando el checkmark.
    /// </summary>
    private void HandleTargetReached()
    {
        countText.gameObject.SetActive(false);
        checkmarkImageObject.SetActive(true);
    }
}
