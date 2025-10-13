using UnityEngine;

/// <summary>
/// Rota un cubo al mantener clic en él, suavemente.
/// </summary>
public class CubeClickRotateHold : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 holdRotationAxis = Vector3.back;
    [SerializeField] private float holdRotationAngle = 120f;
    [SerializeField] private float rotationSpeed = 200f;

    [Header("Raycast Camera")]
    [SerializeField] private Camera raycastCamera;

    private bool isPressed;
    private float currentAngle;

    private void Start()
    {
        if (raycastCamera != null) return;

        GameObject camObj = GameObject.FindGameObjectWithTag("PouringCamera");
        if (camObj != null)
        {
            raycastCamera = camObj.GetComponent<Camera>();
            Debug.Log($"[CubeClickRotateHold] Camera assigned: {raycastCamera.name}");
        }
        else
        {
            Debug.LogWarning("[CubeClickRotateHold] No camera found with tag 'PouringCamera'");
        }
    }

    private void Update()
    {
        if (raycastCamera == null) return;

        DetectClick();
        RotateCube();
    }

    private void DetectClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
                isPressed = true;
        }

        if (Input.GetMouseButtonUp(0)) isPressed = false;
    }

    private void RotateCube()
    {
        if (isPressed) RotateHold();
        else RotateReturn();
    }

    private void RotateHold()
    {
        float step = rotationSpeed * Time.deltaTime;
        float remaining = holdRotationAngle - currentAngle;
        if (step > remaining) step = remaining;

        transform.Rotate(holdRotationAxis, step);
        currentAngle += step;
    }

    private void RotateReturn()
    {
        if (currentAngle <= 0f) return;

        float step = rotationSpeed * Time.deltaTime;
        if (step > currentAngle) step = currentAngle;

        transform.Rotate(holdRotationAxis, -step);
        currentAngle -= step;
    }
}
