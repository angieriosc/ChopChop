using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla las cámaras de la escena y permite alternar entre ellas.
/// </summary>
public class CameraController : MonoBehaviour
{
    // 1. Variables públicas y serializadas
    [Header("Cámaras de la escena")]
    [SerializeField] private Camera mainCamera;   // Cámara principal
    [SerializeField] private Camera cameraClose;  // Cámara cercana
    [SerializeField] private Camera cameraPlayer; // Cámara del jugador

    // 2. Variables privadas
    private Camera _activeCamera;

    // 3. Métodos de Unity
    private void Start()
    {
        // Activar la cámara principal al inicio
        if (mainCamera != null)
        {
            ActivateCamera(mainCamera);
        }
        else if (Camera.main != null)
        {
            ActivateCamera(Camera.main);
        }
    }

    private void Update()
    {
        HandleCameraSwitchInput();
    }

    // 4. Métodos públicos
    /// <summary>
    /// Activa la cámara indicada y desactiva todas las demás.
    /// </summary>
    /// <param name="cameraToActivate">Cámara a activar</param>
    public void ActivateCamera(Camera cameraToActivate)
    {
        if (cameraToActivate == null) return;

        foreach (Camera cam in Camera.allCameras)
        {
            cam.gameObject.SetActive(false);
        }

        cameraToActivate.gameObject.SetActive(true);
        _activeCamera = cameraToActivate;
    }

    /// <summary>
    /// Retorna la cámara actualmente activa.
    /// </summary>
    /// <returns>Cámara activa</returns>
    public Camera GetActiveCamera()
    {
        return _activeCamera;
    }

    // 6. Métodos privados auxiliares
    /// <summary>
    /// Maneja la entrada del usuario para alternar entre cámaras.
    /// </summary>
    private void HandleCameraSwitchInput()
    {
        // Alternar entre mainCamera y cameraClose con C
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (_activeCamera == mainCamera && cameraClose != null)
            {
                ActivateCamera(cameraClose);
            }
            else if (_activeCamera == cameraClose && mainCamera != null)
            {
                ActivateCamera(mainCamera);
            }
        }

        // Cambiar a cámara de jugador con tecla 1
        if (Input.GetKeyDown(KeyCode.Alpha1) && cameraPlayer != null)
        {
            ActivateCamera(cameraPlayer);
        }
    }
}
