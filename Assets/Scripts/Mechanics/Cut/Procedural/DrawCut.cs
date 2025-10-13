using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class DrawCut : MonoBehaviour
{
    private Vector3 pointA;
    private Vector3 pointB;
    private LineRenderer cutRender;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        cutRender = GetComponent<LineRenderer>();

        // Line appearance setup
        cutRender.positionCount = 2;
        cutRender.startWidth = 0.05f;
        cutRender.endWidth = 0.05f;
        cutRender.startColor = Color.gray;
        cutRender.endColor = Color.gray;
        cutRender.enabled = false;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10));

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            pointA = mouseWorldPos;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            cutRender.enabled = true;
            cutRender.SetPosition(0, pointA);
            cutRender.SetPosition(1, mouseWorldPos);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            pointB = mouseWorldPos;
            CreateSlicePlane();
            cutRender.enabled = false;
        }
    }

    void CreateSlicePlane()
    {
        Vector3 pointInPlane = (pointA + pointB) / 2;
        Vector3 cutPlaneNormal = Vector3.Cross((pointA - pointB), (pointA - cam.transform.position)).normalized;
        Quaternion orientation = Quaternion.FromToRotation(Vector3.up, cutPlaneNormal);

        var all = Physics.OverlapBox(pointInPlane, new Vector3(100, 0.01f, 100), orientation);

        foreach (var hit in all)
        {
            InteractableObject interactable = hit.GetComponent<InteractableObject>();

            if (interactable != null && interactable.HasCapability(ObjectCapabilities.Cuttable))
            {
                MeshFilter filter = hit.GetComponentInChildren<MeshFilter>();
                if (filter != null)
                {
                    Cutter.Cut(hit.gameObject, pointInPlane, cutPlaneNormal);
                }
            }
        }
    }
}
