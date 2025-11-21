using UnityEngine;

public class DialogueFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        transform.position = target.position + offset;
        transform.LookAt(transform.position + cam.transform.forward);
    }
}
