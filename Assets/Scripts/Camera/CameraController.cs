using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Cámara del jugador")]
    [SerializeField] private Camera cameraPlayer;

    private Camera _activeCamera;

    private void Start()
    {
        if (cameraPlayer != null)
            ActivateCamera(cameraPlayer);
    }

    public void ActivateCamera(Camera cam)
    {
        if (cam == null) return;

        foreach (Camera c in Camera.allCameras)
            c.gameObject.SetActive(false);

        cam.gameObject.SetActive(true);
        _activeCamera = cam;
    }

    public Camera GetActiveCamera()
    {
        return _activeCamera;
    }
}
