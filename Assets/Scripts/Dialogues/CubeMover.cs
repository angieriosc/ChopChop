using UnityEngine;

public class CubeMover : MonoBehaviour
{
    public GameObject target;    // ← ahora el destino es un GameObject
    public float speed = 3f;

    public System.Action OnReachedTarget;

    private bool moving = false;

    public void MoveTo()
    {
        if (target == null)
        {
            Debug.LogWarning("CubeMover: No se asignó un target.");
            return;
        }

        moving = true;
    }

    void Update()
    {
        if (!moving || target == null) return;

        Vector3 targetPosition = target.transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            moving = false;
            OnReachedTarget?.Invoke();
        }
    }
}

