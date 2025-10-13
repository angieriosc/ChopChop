using UnityEngine;
using UnityEngine.InputSystem;

public enum Angle
{
    Up,
    Forward
}

public class MouseClickCut : MonoBehaviour
{
    public Angle angle;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out hit))
            {
                GameObject victim = hit.collider.gameObject;
                if (victim.tag != "Safe")
                {
                    if (angle == Angle.Up)
                    {
                        Cutter.Cut(victim, hit.point, Vector3.up);
                    }
                    else if (angle == Angle.Forward)
                    {
                        Cutter.Cut(victim, hit.point, Vector3.forward);
                    }
                }
            }
        }
    }
}